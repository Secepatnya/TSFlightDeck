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

        
        public static MinimalisticTelnet.TelnetConnection tc = new MinimalisticTelnet.TelnetConnection();

        public static void connect()
        {
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

        public static void processMessage(string input)
        {
            if (input == null) return; 
            
            input = input.ToLower();
            if (input.Length > 0)
            {

                char[] delimiter = { ' ' };
                string[] temp = input.Split(delimiter);

                // parse notification into message
                if (temp.Length >= 2)
                {
                    Console.WriteLine("Last message: {0}", temp[3]);
                    string[] delimiter2 = { "\\s" };
                    string[] temp2 = temp[3].Split(delimiter2, StringSplitOptions.None);


                    //  parse song name from message
                    if (temp2.Length > 1 && temp2[0].Contains("play"))
                    {
                        string finalLookup = string.Join(" ", temp2.Skip(1));
                        Console.WriteLine(finalLookup);

                        // look for song
                        if (!cs.P1findTrack(finalLookup))
                            sendMessage("找不到 " + temp2[1]);
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
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Start();
                Console.WriteLine("Polling started");
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
            processMessage(tc.Read());
        }

        #endregion

    }
}
