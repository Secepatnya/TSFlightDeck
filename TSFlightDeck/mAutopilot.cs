using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace TSFlightDeck
{
    class mAutopilot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /* Toggle trackers */
        public ObservableCollection<eventItem> events;
        DispatcherTimer timer;

        private string _apState;
        public string apState
        {
            get { return _apState; }
            private set { if (_apState != value) { _apState = value; NotifyPropertyChanged("apState"); } }
        }



        /* Declare Controllers */
        private mControllerPanel cs;

        /* Constructor */
        public mAutopilot(mControllerPanel targetPanel)
        {
            apState = "OFF";
            cs = targetPanel;
            events = new ObservableCollection<eventItem>();

            apRoutines();
            mainMusicStartOnly(); // Build for Cherie's TS 
            //autoStart(); // Build for CHTEA TS
        }

        public void apAddRoutine(string name, Action command, int hour, int minute, DayOfWeek dayName)
        {
            events.Add(new eventItem
            {
                name = name,
                command = command,
                startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 00),
                dayName = dayName,
                hasFired = false
            });

        }

        public void apRoutines()
        {
            apAddRoutine("Saturday Main Program BOARDING", mainMusicStartOnly, 15, 00, DayOfWeek.Saturday);
            apAddRoutine("Saturday Main Program START", announceAndMainStart, 15, 55, DayOfWeek.Saturday);
            apAddRoutine("Saturday Main Program FINISH", mainProgramFinish, 17, 03, DayOfWeek.Saturday);

            apAddRoutine("Saturday Night Program START", auxStartOnly, 22, 00, DayOfWeek.Saturday);

            apAddRoutine("SUNDAY Main Program BOARDING", mainMusicStartOnly, 13, 00, DayOfWeek.Sunday);
            apAddRoutine("SUNDAY Main Program START", announceAndMainStart, 13, 55, DayOfWeek.Sunday);
            apAddRoutine("SUNDAY Main Program FINISH", mainProgramFinish, 15, 03, DayOfWeek.Sunday);

            apAddRoutine("SUNDAY Night Program START", auxStartOnly, 22, 00, DayOfWeek.Sunday);



            //apAddRoutine("TEST", announceAndMainStart, 18, 01, DayOfWeek.Thursday);
            //apAddRoutine("TEST STOP", mainProgramFinish, 18, 05, DayOfWeek.Thursday);
        }

        public void pollServiceStart() 
        {
            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Start();
        }

        public void pollServiceStop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        void TimerOnTick(object sender, EventArgs args)
        {
            foreach (eventItem item in events)
            {
                if (item.startTime.Hour == DateTime.Now.Hour && item.startTime.Minute == DateTime.Now.Minute && item.dayName == DateTime.Now.DayOfWeek)
                {
                    if (!item.hasFired)
                    {
                        item.hasFired = true;
                        item.command();
                    }
                }
                else
                {
                    item.hasFired = false;
                }
            }
        }

        /* Individual subroutines */
        void mainProgramStart()
        {
            cs.player1.Stop();
            cs.player2.Stop();
            cs.satellite.Start();
            cs.satellite.SetVolume(1f);
        }

        void mainProgramFinish()
        {
            cs.player1.selectedPlayMode = sourcePlayer.playModes.Continous;
            cs.satellite.Stop();
            //cs.P1Start();
            cs.player1.SetVolume(1f);
        }

        void announceAndMainStart()
        {
            cs.player1.Stop();
            cs.P2StartToSat();
        }

        void auxStartOnly()
        {
            cs.satellite.Stop();
            cs.player1.Stop();

            cs.P2Start();
            cs.player2.SetVolume(1f);
        }

        void auxStopOnly()
        {
            cs.player2.Stop();
        }

        void mainMusicStartOnly()
        {
            cs.satellite.Stop();
            cs.player2.Stop();
            cs.player1.selectedPlayMode = sourcePlayer.playModes.Continous;
            cs.P1Start();
            cs.player1.SetVolume(1f);
        }

        void mainMusicStopOnly()
        {
            cs.player1.Stop();
        }


        /* Master Switch */
        public void autoStart()
        {
            if (apState != "ON")
            {
                pollServiceStart();
            }
            apState = "ON";
        }

        public void autoStop()
        {
            pollServiceStop();
            apState = "OFF";
        }

    }
}
