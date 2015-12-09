﻿// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be

// Modified to use UTF8, TS3 ClientQuery, autoreconnect.

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace MinimalisticTelnet
{
    enum Verbs {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    class TelnetConnection
    {
        public TcpClient tcpSocket;

        int TimeOutMs = 100;

        public TelnetConnection()
        {
            tcpSocket = new TcpClient();
        }

        public void connect() {
            Console.WriteLine("Attempting a new connection");
            new Thread(() =>
            {
                try
                {
                    tcpSocket = new TcpClient("127.0.0.1", 25639);
                    //tcpSocket = new TcpClient("10.211.55.2", 25639);
                    WriteLine("clientnotifyregister schandlerid=1 event=notifytextmessage");
                    WriteLine("clientnotifyregister schandlerid=1 event=notifyclientpoke");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Connection failed" + e);
                }

            }).Start();
        }

        public string Login(string Username,string Password,int LoginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = LoginTimeOutMs;
            string s = Read();
            if (!s.TrimEnd().EndsWith(":"))
               throw new Exception("Failed to connect : no login prompt");
            WriteLine(Username);

            s += Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(Password);

            s += Read();
            TimeOutMs = oldTimeOutMs;
            return s;
        }

        public void WriteLine(string cmd)
        {
            Write(cmd + "\n");
            //Write(cmd);
        }

        public void Write(string cmd)
        {
            try 
            {
                byte[] buf = Encoding.UTF8.GetBytes(cmd);
                tcpSocket.GetStream().Write(buf, 0, buf.Length);
            }
            catch
            {
                connect();
            }
        }

        public string Read()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                do
                {
                    ParseTelnet(sb);
                    System.Threading.Thread.Sleep(TimeOutMs);
                } while (tcpSocket.Available > 0);
                return sb.ToString();
            }
            catch
            {
                connect();
                return null;
            }
        }

        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1 :
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC: 
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO: 
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA )
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL:(byte)Verbs.DO); 
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT); 
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append( (char)input );
                        break;
                }
            }
        }
    }
}
