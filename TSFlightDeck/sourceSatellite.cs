using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSFlightDeck
{
    class sourceSatellite : sourceParent
    {


        /* Settings */
        private int devNum = 0;

        /* Declarations */
        private WaveInEvent inputSat = null;
        private VolumeSampleProvider inputSatVSP = null;

        public sourceSatellite(MixingSampleProvider targetMixer)
        {
            volume = 1f;
            sourceState = "OFF";

            //Open channel 
            inputSat = new WaveInEvent();
            inputSat.DeviceNumber = devNum;
            inputSat.WaveFormat = new WaveFormat();
            inputSat.BufferMilliseconds = 400;

            //Volume VSP
            //inputSatVSP = new VolumeSampleProvider(new Pcm16BitToSampleProvider(new WaveInProvider(inputSat)));
            inputSatVSP = new VolumeSampleProvider(new WaveInProvider(inputSat).ToSampleProvider());

            //Send to mixer
            targetMixer.AddMixerInput(inputSatVSP);
        }

        /* Toggle switch */
        #region SatelliteControls
        public void Start()
        {
            if (sourceState == "OFF")
            {
                inputSat.StartRecording();
                sourceState = "ON";
                tsInterface.sendMessage("Now playing from External Input");
            }
        }

        public void Stop()
        {
            if (sourceState == "ON")
            {
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
            if (inputSatVSP != null && !locked) {
                locked = true;
                // increase
                if (inputSatVSP.Volume > setTo)
                {
                    while (inputSatVSP != null && inputSatVSP.Volume > setTo)
                    {
                        SetVolume(inputSatVSP.Volume - 0.05f);
                        Thread.Sleep(11);
                    }
                }
                else
                {
                    while (inputSatVSP != null && inputSatVSP.Volume < setTo)
                    {
                        SetVolume(inputSatVSP.Volume + 0.05f);
                        Thread.Sleep(11);
                    }
                }
            }
            locked = false;
        }

        public void Duck()
        {
            if (!isDucked)
            {
                isDucked = true;
                DuckTo(0.2f);
            }
            else
            {
                isDucked = false;
                DuckTo(1f);
            }
        }

        #endregion

    }
}
