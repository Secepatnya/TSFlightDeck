using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Lame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Razzle
{
    class sourceSatellite : sourceParent
    {
        private string _recState;
        public string recState
        {
            get { return _recState; }
            set { if (_recState != value) { _recState = value; NotifyPropertyChanged("recState"); } }
        }

        public static String directory;
        /* Settings */
        private int devNum = 0;

        /* Declarations */
        private WaveInEvent inputSat = null;
        private WaveInEvent inputSatRec = null;
        private VolumeSampleProvider inputSatVSP = null;

        LameMP3FileWriter mp3writer;

        public sourceSatellite(MixingSampleProvider targetMixer)
        {
            volume = 1f;
            sourceState = "OFF";
            recState = "STOPPED";

            //Open channel 
            inputSat = new WaveInEvent();
            inputSat.DeviceNumber = devNum;
            inputSat.WaveFormat = new WaveFormat();
            inputSat.BufferMilliseconds = 500;

            //Volume VSP
            inputSatVSP = new VolumeSampleProvider(new WaveInProvider(inputSat).ToSampleProvider());

            //Send to mixer
            targetMixer.AddMixerInput(inputSatVSP);
        }

        /* Record Controls */
        public void RecordStart()
        {
            if (recState == "STOPPED")
            {
                recState = "STARTING";
                //Open channel for recording
                inputSatRec = new WaveInEvent();
                inputSatRec.DeviceNumber = devNum;
                inputSatRec.WaveFormat = new WaveFormat();
                inputSatRec.BufferMilliseconds = 500;

                inputSatRec.DataAvailable += recDataAvailable;
                mp3writer = new LameMP3FileWriter(generateFileName(), inputSatRec.WaveFormat, 128);
                inputSatRec.StartRecording();
                recState = "RECORDING";
                tsInterface.sendMessage("Recording started successfully!");
            }
        }

        public void RecordStop()
        {
            if (recState == "RECORDING")
            {
                recState = "STOPPING";
                if (inputSatRec != null)
                {
                    inputSatRec.StopRecording();
                    inputSatRec.Dispose();
                }

                if (mp3writer != null)
                {
                    mp3writer.Flush();
                    mp3writer.Dispose();
                    tsInterface.sendMessage("Recording stopped successfully!");
                }
            }
            recState = "STOPPED";
        }

        private String generateFileName()
        {
            String dateprefix = DateTime.Now.ToString("yyyyMMdd");
            String namebody = "_liverec";
            String suffix = "";
            String extension = ".mp3";
            int suffixcount = 0;

            String final = directory + dateprefix + namebody + suffix + extension;

            while (File.Exists(final))
            {
                suffixcount += 1;
                suffix = suffixcount.ToString();
                final = directory + dateprefix + namebody + suffix + extension;
            }

            return final;
        }

        private void recDataAvailable(object sender, WaveInEventArgs e)
        {
            Console.WriteLine("Offloading buffers");
            if (mp3writer != null)
            {
                mp3writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        public static void setRecordDirectory(string input)
        {
            if (!input.EndsWith(@"\"))
            {
                directory = input + @"\";
            }
            else
            {
                directory = input;
            }
        }

        /* Toggle switch */

        public void Start()
        {
            if (sourceState == "OFF")
            {
                //destinationMixer.AddMixerInput(inputSatVSP);
                inputSat.StartRecording();
                sourceState = "ON";
                tsInterface.sendMessage("Switched to External Input.");
            }
        }

        public void Stop()
        {
            if (sourceState == "ON")
            {
                //destinationMixer.RemoveMixerInput(inputSatVSP);
                inputSat.StopRecording();
                sourceState = "OFF";
                tsInterface.sendMessage("Stopped playing from External Input");
            }
        }

        /* Volumes */
        public void SetVolume(float setTo)
        {
            if (inputSatVSP != null)
            {
                inputSatVSP.Volume = setTo;
            }
            volume = setTo;
        }

        public void DuckTo(float setTo)
        {
            new Thread(() =>
            {
                if (inputSatVSP != null && !locked)
                {
                    locked = true;
                    // increase
                    if (inputSatVSP.Volume > setTo)
                    {
                        while (inputSatVSP != null && inputSatVSP.Volume > setTo)
                        {
                            SetVolume(inputSatVSP.Volume - 0.1f);
                            Thread.Sleep(80);
                        }
                    }
                    else
                    {
                        while (inputSatVSP != null && inputSatVSP.Volume < setTo)
                        {
                            SetVolume(inputSatVSP.Volume + 0.1f);
                            Thread.Sleep(80);
                        }
                    }
                }
                locked = false;
            }).Start();
        }


    }
}
