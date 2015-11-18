using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;


namespace TSFlightDeck
{
    class mControllerPanel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /* Settings */

        /* Init Stuff */
        private DirectSoundOut outputMaster;
        private MixingSampleProvider mixer;

        public mAutopilot ap;
        public sourceSatellite satellite;
        public sourcePlayer player1;
        public sourcePlayer player2;

        private float duckedVol = 0.2f;

        public mControllerPanel()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            
            // Open mixer
            //mixer = new MixingWaveProvider32();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            mixer.ReadFully = true;

            // Init sources
            satellite = new sourceSatellite(mixer);
            player1 = new sourcePlayer(mixer);
            player2 = new sourcePlayer(mixer);
            
            // Open output line
            outputMaster = new DirectSoundOut();
            
            outputMaster.Init(mixer);
            outputMaster.Play();
            loadConfig();

            ap = new mAutopilot(this);
            tsInterface.setController(this);
            //tsInterface.pollServiceStart();
            tsInterface.connect();
        }

        public void resetOutput()
        {
            satellite.Stop();
            player1.Stop();
            player2.Stop();
            Thread.Sleep(500);
            outputMaster.Stop();
            Thread.Sleep(500);
            outputMaster.Play();
        }

        public void satDuck()
        {
            new Thread(() =>
            {
                satellite.Duck();
            }).Start();
        }

        public void P1Start()
        {
            new Thread(() =>
            {
                player1.Start();
            }).Start();
        }

        public void P1Duck()
        {
            new Thread(() =>
            {
                player1.Duck();
            }).Start();
        }

        public void P1SkipPrev()
        {
            new Thread(() =>
            {
                player1.skip(false);
            }).Start();
        }

        public void P1SkipNext()
        {
            new Thread(() =>
            {
                player1.skip(true);
            }).Start();
        }

        public void P2Start()
        {
            new Thread(() =>
            {
                player2.Start();
            }).Start();
        }

        public void P2StartToSat()
        {
            new Thread(() =>
            {
                player2.Start();
                satellite.Start();
            }).Start();
        }

        public void P2StartQuacking() {
            new Thread(() =>
            {
                satellite.DuckTo(duckedVol);
                player1.DuckTo(duckedVol);

                player2.Start();
                
                satellite.DuckTo(1f);
                player1.DuckTo(1f);
            }).Start();
        }

        public void P2Duck()
        {
            new Thread(() =>
            {
                player2.Duck();
            }).Start();
        }

        public bool P1findTrack(string target)
        {
            Console.WriteLine("Looking for track with name: {0}", target);
            List<musicItem> tempList = player1.playlist.ToList<musicItem>();
            foreach (musicItem item in tempList)
            {
                if (item.name.ToLower().Contains(target))
                {
                    player1.selectTrack(item);
                    P1Start();
                    return true;
                }
            }
            return false;
        }

        public void loadConfig() {

            //Properties.Settings.Default.contentPL1 = null;
            //Properties.Settings.Default.contentPL2 = null;

            if (Properties.Settings.Default.contentPL1 != null)
            {
                foreach (string item in Properties.Settings.Default.contentPL1)
                {
                    player1.addTrack(item);
                }
            }

            if (Properties.Settings.Default.contentPL1 != null)
            {
                foreach (string item in Properties.Settings.Default.contentPL2)
                {
                    player2.addTrack(item);
                }
            }

        }

        public void shutdown()
        {
            player1.Stop();      
            player2.Stop();      
            satellite.Stop();    
            outputMaster.Stop(); 

            // Clear savefile
            Properties.Settings.Default.contentPL1 = null;
            Properties.Settings.Default.contentPL2 = null;

            Properties.Settings.Default.contentPL1 = new System.Collections.Specialized.StringCollection();
            Properties.Settings.Default.contentPL2 = new System.Collections.Specialized.StringCollection();
            
            // Save content
            foreach (musicItem track in player1.playlist) {
                Properties.Settings.Default.contentPL1.Add(track.url);
            }

            foreach (musicItem track in player2.playlist) {
                Properties.Settings.Default.contentPL2.Add(track.url);    
            }



            Properties.Settings.Default.Save();


        }
    }
}