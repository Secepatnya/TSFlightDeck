using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace TSFlightDeck
{
    class tsInterface
    {
        static DispatcherTimer timer;
        static mControllerPanel cs;
        static List<string> allowuid = new List<string>();

        
        public static MinimalisticTelnet.TelnetConnection tc = new MinimalisticTelnet.TelnetConnection();

        static tsInterface()
        {
            
        }

        public static void connect()
        {
            if (!tc.IsConnected)
                tc.connect();
        }

        public static void sendMessage(string msg)
        {
            msg = msg.Replace(" ", "\\s");
            tc.WriteLine("sendtextmessage targetmode=2 msg=" + msg);
        }

        #region songrequest
        public static void setController(mControllerPanel target)
        {
            cs = target;
        }

        public static void parseMessage(string input)
        {
            if (input == null) return; 
            
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

            }
        }

        public static void processCommand(string senderuid, string cmd, string argue)
        {
            cmd = cmd.ToLower();
            argue = argue.ToLower();

            string senderuidtr = senderuid.Substring(11);

            // permissions check
            if (allowuid.All(senderuidtr.Contains))
            //if (true)
            {
                if (cmd == "msg=fdselect1")  // find player 1 track
                {
                    if (cs.P1findTrack(argue))
                    {
                        sendMessage("选择的曲目: " + cs.player1.selectedTrack.name + "。");
                        //cs.P1Start();
                    }
                    else
                    {
                        sendMessage("找不到 " + argue + "。");
                    }

                }
                else if (cmd == "msg=fdselect2")  // find player 2 track
                {
                    if (cs.P2findTrack(argue))
                    {
                        sendMessage("选择的曲目: " + cs.player2.selectedTrack.name + "。");
                    }
                    else
                    {
                        sendMessage("找不到 " + argue + "。");
                    }
                }
                else if (cmd == "msg=fdplay1") // start player 1
                {
                    cs.P1Start();
                }
                else if (cmd == "msg=fdplay2") // start player 2
                {
                    cs.P2Start();
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
                    sendMessage("音频子系统已复位。");
                }
                else if (cmd == "msg=fdresetvol") // reset outputs
                {
                    cs.player1.SetVolume(1f);
                    cs.player2.SetVolume(1f);
                    cs.satellite.SetVolume(1f);
                    sendMessage("音量已复位。");
                }
                else if (cmd == "msg=fdtsreset") // force TS unmute
                {
                    tc.WriteLine("clientupdate client_nickname=MusicBot-Primary");
                    tc.WriteLine("clientupdate client_output_muted=0");
                    tc.WriteLine("clientupdate client_input_muted=0");
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
        }

        public static void pollServiceStop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        static void TimerOnTick(object sender, EventArgs args)
        {
            connect();
            parseMessage(tc.Read());
        }

        #endregion

    }
}
