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
using System.Windows.Forms;
using System.Xml.Linq;


namespace Razzle
{
    class mControllerPanel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /* Tracking */


        /* Settings */
        private string configfilename = "RazzleConfig.xml";
        private string autopilotconfig = "autopilotDefs.xml";

        /* Init Stuff */
        private WaveOutEvent outputMaster;
        private MixingSampleProvider mixer;

        public mAutopilot ap;
        public sourceSatellite satellite;
        public sourcePlayer player1;
        public sourcePlayer player2;
        public sourcePlayer player3;

        private float duckedVol = 0.2f;

        public mControllerPanel()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Init audio
            // Open mixer
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            mixer.ReadFully = true;

            // Init sources
            satellite = new sourceSatellite(mixer);
            player1 = new sourcePlayer(mixer);
            player2 = new sourcePlayer(mixer);
            player3 = new sourcePlayer(mixer);

            // Open output line
            outputMaster = new WaveOutEvent();

            outputMaster.DesiredLatency = 500;
            outputMaster.Init(mixer);
            outputMaster.Play();

            //Connect the Teamspeak interface
            ap = new mAutopilot(this);
            tsInterface.setController(this);
            tsInterface.pollServiceStart();
            tsInterface.connect();

            loadConfig();
        }

        public void resetOutput()
        {
            shutdown();
            Application.Restart();
            Environment.Exit(-1);
        }

        public void P2StartQuacking()
        {
            new Thread(() =>
            {
                satellite.DuckTo(duckedVol);
                player1.DuckTo(duckedVol);
                player3.DuckTo(duckedVol);
                player2.Start(false);
                satellite.DuckTo(1f);
                player1.DuckTo(1f);
            }).Start();

        }

        public bool P1findTrack(string target)
        {
            return player1.findTrack(target);
        }

        public bool P2findTrack(string target)
        {
            return player2.findTrack(target);
        }

        public bool P3findTrack(string target)
        {
            return player3.findTrack(target);
        }

        public void reloadConfig()
        {
            player1.playlist.Clear();
            player2.playlist.Clear();
            player3.playlist.Clear();
            ap.apClearRoutines();
            loadConfig();
        }

        public void loadConfig()
        {
            try
            {
                XElement config = XElement.Load(configfilename);

                sourceSatellite.setRecordDirectory(@"C:\");
                sourceSatellite.setRecordDirectory(config.Element("RecordDir").Value);
                
                foreach (XElement user in config.Element("TSUsers").Elements())
                {
                    tsInterface.addUser(user.Element("name").Value, user.Element("uid").Value);
                }

                foreach (XElement item in config.Element("playlist1").Elements())
                {
                    player1.addTrack(item.Value);
                }

                foreach (XElement item in config.Element("playlist2").Elements())
                {
                    player2.addTrack(item.Value);
                }

                foreach (XElement item in config.Element("playlist3").Elements())
                {
                    player3.addTrack(item.Value);
                }

            }
            catch
            {

            }

            try
            {
                XDocument config = XDocument.Load(autopilotconfig);

                foreach (XElement item in config.Element("autopilotDefs").Elements())
                {
                    Console.WriteLine(ap.apAddRoutineFromFile(item.Element("name").Value, item.Element("action").Value, item.Element("triggerHour").Value, item.Element("triggerMinute").Value, item.Element("triggerDayOfWeek").Value));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown: " + e.ToString());
            }


        }

        public void shutdown()
        {
            player1.Stop();
            player2.Stop();
            player3.Stop();
            satellite.Stop();
            satellite.RecordStop();
            outputMaster.Stop();

            //Test XML
            XDocument config = new XDocument(
                new XElement("RazzleConfig",
                    new XElement("RecordDir", sourceSatellite.directory),
                    new XElement("TSUsers", tsInterface.allowuid.Select(x=> new XElement("user",
                        new XElement("name", x.name),
                        new XElement("uid", x.uid)))),
                    new XElement("playlist1", player1.playlist.Select(x => new XElement("item", x.url))),
                    new XElement("playlist2", player2.playlist.Select(x => new XElement("item", x.url))),
                    new XElement("playlist3", player3.playlist.Select(x => new XElement("item", x.url)))
                )
            );

            config.Save(configfilename);
        }


    }
}