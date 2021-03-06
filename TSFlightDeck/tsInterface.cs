﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;


namespace Razzle
{
    class tsInterface
    {
        static DispatcherTimer timer;
        static DispatcherTimer timer2;
        static mControllerPanel cs;
        public static ObservableCollection<tsuser> allowuid = new ObservableCollection<tsuser>();

        
        public static MinimalisticTelnet.TelnetConnection tc = new MinimalisticTelnet.TelnetConnection();

        static tsInterface()
        {
        }

        public static void addUser(string name, string uid)
        {
            allowuid.Add(
                new tsuser
                {
                    name = name,
                    uid = uid
                });
        }

        public static void connect()
        {
            if (!tc.IsConnected)
            {
                tc.connect();
            }
            sendMessage("TS Music Bot 'Razzle' has started successfully!");
            
        }

        public static void sendMessage(string msg)
        {
            msg = msg.Replace(" ", "\\s");
            tc.WriteLine("sendtextmessage targetmode=2 msg=" + msg);
        }

        public static void sendPrivateMessage(string receiver, string msg)
        {
            msg = msg.Replace(" ", "\\s");
            tc.WriteLine("sendtextmessage targetmode=1 target=" + receiver + "  msg=" + msg);
        }

        #region songrequest
        public static void setController(mControllerPanel target)
        {
            cs = target;
        }

        // need to convert the parser to use dictionaries 
        public static void parseMessage(string input)
        {
            if (input == null) return;

            //Console.WriteLine("message: " + input);
            if (input.Length > 0)
            {
                string sender;
                string cmd;
                string argue;

                // Split notification into array
                char[] delimiter = { ' ' };
                string[] notifAsArray = input.Split(delimiter);

                // get message content from notification
                if (notifAsArray.Length >= 2 && notifAsArray[0].Equals("notifytextmessage"))
                {
                    sender = notifAsArray[6];

                    string[] delimiter2 = { "\\s" };
                    string[] msgAsArray = notifAsArray[3].Split(delimiter2, StringSplitOptions.None);

                    // separate command from params
                    if (msgAsArray.Length > 0)
                    {
                        cmd = msgAsArray[0];
                        argue = string.Join(" ", msgAsArray.Skip(1));
                        processCommand(sender, cmd, argue);
                    }
                }
                else if (notifAsArray.Length >= 2 && notifAsArray[0].Equals("notifyclientpoke")) // poke fun
                {
                    sender = notifAsArray[3];

                    string[] delimiter1 = { "=" };
                    string[] delimiter2 = { "\\s" };
                    string[] senderAsArray = notifAsArray[3].Split(delimiter2, StringSplitOptions.None);
                    string[] senderAsArray2 = string.Join(" ", senderAsArray).Split(delimiter1, StringSplitOptions.None);


                    // separate command from params
                    if (senderAsArray.Length > 0)
                    {
                        cmd = senderAsArray[0];
                        argue = string.Join(" ", senderAsArray2.Skip(1));
                        sendMessage("Ow! " + argue + ", why did you poke me?");
                    }
                }

            }
        }

        public static void processCommand(string senderuid, string cmd, string argue)
        {
            cmd = cmd.ToLower();
            argue = argue.ToLower();
            string combined = String.Concat(cmd, argue);

            string senderuidtr = senderuid.Substring(11);

            // permissions check
            if (allowuid.Any(tsuser => tsuser.uid.Equals(senderuidtr))) // build for CHTEA TS
            //if (true) // build for Cherie's TS
            {
                if (cmd == "msg=fdselect1")  // find player 1 track
                {
                    if (cs.P1findTrack(argue))
                    {
                        sendMessage("Selected track on primary player: " + cs.player1.selectedTrack.name + "。");
                        //cs.P1Start();
                    }
                    else
                    {
                        sendMessage("Cannot find " + argue + ".");
                    }
                }
                else if (cmd == "msg=play")  // Bot code for Cherie's TS
                {
                    if (cs.P1findTrack(argue))
                    {
                        //sendMessage("选择的曲目: " + cs.player1.selectedTrack.name + "。");
                        //cs.P1Start();
                    }
                    else
                    {
                        sendMessage("Cannot find " + argue + ".");
                    }
                }
                else if (cmd == "msg=fdselect2")  // find player 2 track
                {
                    if (cs.P2findTrack(argue))
                    {
                        sendMessage("Selected track on secondary player: " + cs.player2.selectedTrack.name + ".");
                    }
                    else
                    {
                        sendMessage("Cannot find " + argue + ".");
                    }
                }
                else if (cmd == "msg=fdplay1") // start player 1
                {
                    cs.player1.Start(true);
                }
                else if (cmd == "msg=fdplay2") // start player 2
                {
                    cs.player2.Start(true);
                }
                else if (cmd == "msg=fdann") // duck all sources and start player 2
                {
                    cs.P2StartQuacking();
                }
                else if (cmd == "msg=fdstop1") // stop player 1
                {
                    cs.player1.Stop();
                }
                else if (cmd == "msg=fdstop2") // stop player 2
                {
                    cs.player2.Stop();
                }
                else if (cmd == "msg=fdsat") // solo satellite
                {
                    cs.player1.Stop();
                    cs.player2.Stop();
                    cs.satellite.Start();
                    cs.satellite.SetVolume(1f);
                }
                else if (cmd == "msg=fdsatstop") // stop satellite
                {
                    cs.satellite.Stop();
                }
                else if (cmd == "msg=fdresetsys") // reset outputs
                {
                    cs.resetOutput();
                    sendMessage("Soundboard is reset");
                }
                else if (cmd == "msg=fdresetvol") // reset outputs
                {
                    cs.player1.SetVolume(1f);
                    cs.player2.SetVolume(1f);
                    cs.satellite.SetVolume(1f);
                    sendMessage("Volume is reset");
                }
                else if (cmd == "msg=fdtsreset") // force TS unmute
                {
                    //tc.WriteLine("clientupdate client_nickname=MusicBot-Primary");
                    tc.WriteLine("clientupdate client_output_muted=0");
                    tc.WriteLine("clientupdate client_input_muted=0");
                }
                else if (cmd == "msg=fdplaymode") // playmode for primary player
                {
                    switch (argue) 
                    {
                        case "single": 
                            cs.player1.selectedPlayMode = sourcePlayer.playModes.Single;
                            sendMessage("Playmode is set to Single");
                            break;
                        case "cont": 
                            cs.player1.selectedPlayMode = sourcePlayer.playModes.Continous;
                            sendMessage("Playmode is set to Continous");
                            break;
                        case "repeatone": 
                            cs.player1.selectedPlayMode = sourcePlayer.playModes.RepeatOne;
                            sendMessage("Playmode is set to RepeatOne");
                            break;
                        default: 
                            sendMessage("Invalid option. Available options are: single, cont, repeatone.");
                            break;
                    }
                }
            }
        }

        #endregion

        #region poll for messages
        public static void pollServiceStart()
        {
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Tick += TimerOnTick;
                timer.Interval = TimeSpan.FromSeconds(0.5);
                timer.Start();
            }
            if (timer2 == null)
            {
                timer2 = new DispatcherTimer();
                timer2.Tick += TimerOnTick2;
                timer2.Interval = TimeSpan.FromMinutes(4);
                timer2.Start();
            }
        }

        public static void pollServiceStop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
            if (timer2 != null)
            {
                timer2.Stop();
                timer2 = null;
            }
        }

        static void TimerOnTick(object sender, EventArgs args)
        {
            parseMessage(tc.Read());
        }
        //keepalive
        static void TimerOnTick2(object sender, EventArgs args)
        {
            tc.WriteLine("whoami"); //keepalive
        }

        #endregion

    }
}
