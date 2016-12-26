using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using System.Threading;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using NAudio.Wave.SampleProviders;

namespace Razzle
{
    class sourcePlayer : sourceParent
    {

        public enum playModes { Single, RepeatOne, Continous, Normal }


        /* Music File Manager */
        public ObservableCollection<musicItem> playlist;
        public musicItem selectedTrack;

        AudioFileReader audioFileReader;
        ISampleProvider audioFileReaderConverted;

        MixingSampleProvider mixer;

        string[] allowedFileTypes = { ".mp3", ".wma", ".wav" };

        /* Trackers */
        bool stopped = true;

        private playModes _selectedPlayMode;
        public playModes selectedPlayMode
        {
            get { return _selectedPlayMode; }
            set { if (_selectedPlayMode != value) { _selectedPlayMode = value; NotifyPropertyChanged("selectedPlayMode"); } }
        }

        /* Program */

        //Create the player instance, connect it to the mixer
        public sourcePlayer(MixingSampleProvider targetMixer)
        {
            playlist = new ObservableCollection<musicItem>();
            volume = 1f;
            sourceState = "Stopped";
            selectedPlayMode = playModes.Single;

            mixer = targetMixer;
        }


        //Start track, with multithread toggle
        public void Start(bool multithread)
        {
            if (multithread)
            {
                new Thread(() =>
                {
                    AcceptStart();
                }).Start();
            }
            else
            {
                AcceptStart();
            }
        }

        // Play selected track
        public void AcceptStart()
        {
            stopped = false;
            //Check if another load operation is running. Halt if one is
            if (locked)
            {
                return;
            }
            else
            {
                locked = true;
            }

            //Check if something's playing. Stop it if it is
            if (audioFileReader != null)
            {
                Stop();
            }

            //Check if a track is selected. If not, play the first one.
            if (selectedTrack == null && playlist != null && playlist.Count > 0)
            {
                selectTrack(playlist[0]);
            }

            try
            {
                if (audioFileReader == null && selectedTrack != null && File.Exists(selectedTrack.url))
                {
                    //Sets up the source, applies the volume too
                    audioFileReader = new AudioFileReader(selectedTrack.url);
                    audioFileReader.Volume = volume;

                    //Converts output to proper sample format
                    WdlResamplingSampleProvider audioFileReaderCSampler = new WdlResamplingSampleProvider(audioFileReader, 44100);
                    SampleToWaveProvider audioFileReaderCSampleAsWave = new SampleToWaveProvider(audioFileReaderCSampler);
                    audioFileReaderConverted = new SampleToWaveProvider(new SampleChannel(audioFileReaderCSampleAsWave, true)).ToSampleProvider();

                    // Adds prepared output to mixer
                    mixer.AddMixerInput(audioFileReaderConverted);

                    //Send notification
                    sourceState = "Playing: " + selectedTrack.name;
                    tsInterface.sendMessage("Now Playing: " + Path.GetFileNameWithoutExtension(selectedTrack.name));

                    audioFileReader.Volume = volume;

                    locked = false;
                    while (audioFileReader != null && audioFileReader.Position < audioFileReader.Length)
                    {
                        Thread.Sleep(1000);
                    }
                    sourceState = "Stopped";

                }
            }
            catch (Exception e)
            {
                sourceState = "Cannot play file.";
                Console.WriteLine(e.ToString());
                stopped = false;
                locked = false;
            }
            handleTrackFinish();
        }

        public void handleTrackFinish()
        {
            //What happens after track finishes
            if (!stopped)
            {
                if (selectedTrack != null && !File.Exists(selectedTrack.url))
                {
                    Console.WriteLine("Selected track {0} was deleted, can't play.", selectedTrack.name);
                }

                if (selectedPlayMode == playModes.Continous)
                {
                    skip(true, true);
                }
                else if (selectedPlayMode == playModes.RepeatOne)
                {
                    Start(true);
                }
                else if (selectedPlayMode == playModes.Normal)
                {
                    skip(true, false);
                }
                else if (selectedPlayMode == playModes.Single)
                {
                    Stop();
                }

            }
        }

        // skipper 
        public void skip(bool isNext, bool shouldWrap)
        {
            if (selectedTrack != null)
            {
                int currentIndex;

                currentIndex = playlist.IndexOf(selectedTrack);

                // Skip next
                if (isNext && currentIndex < playlist.Count - 1)
                {
                    selectedTrack = playlist[currentIndex + 1];
                    Start(true);
                }
                else if (isNext && playlist[0] != null && shouldWrap)
                {
                    selectedTrack = playlist[0];
                    Start(true);
                }

                //Skip prev
                else if (!isNext && currentIndex > 0)
                {
                    selectedTrack = playlist[currentIndex - 1];
                    Start(true);
                }
                else if (!isNext && playlist[playlist.Count - 1] != null)
                {
                    selectedTrack = playlist[playlist.Count - 1];
                    Start(true);
                }
                else
                {
                    Start(true);
                }
            }
        }


        public void Stop()
        {
            if (audioFileReader != null)
            {
                mixer.RemoveMixerInput(audioFileReaderConverted);

                audioFileReader.Dispose();
                audioFileReader = null;

                sourceState = "Stopped";
            }
            stopped = true;
        }

        /* File dialog open */
        public void browseFile()
        {
            OpenFileDialog dialogbox = new OpenFileDialog();

            dialogbox.Filter = "Music Files|*.mp3; *.wma; *.wav|All Files (*.*)|*.*";
            dialogbox.Multiselect = true;
            bool? userClickedOK = dialogbox.ShowDialog();

            if (userClickedOK == true)
            {
                foreach (string item in dialogbox.FileNames)
                {
                    addTrack(Path.GetFullPath(item));
                }
            }

        }

        /* Playlist control */
        public void addTrack(string url)
        {
            if (File.Exists(url) && allowedFileTypes.Contains(Path.GetExtension(url)))
            {
                playlist.Add(new musicItem
                {
                    url = Path.GetFullPath(url),
                    name = Path.GetFileName(url)
                });
            }
        }

        public void delSelectedTrack()
        {
            if (selectedTrack != null && playlist.Count > 0)
            {
                playlist.Remove(selectedTrack);
                selectedTrack = null;
            }
        }

        public void selectTrack(musicItem targetItem)
        {
            selectedTrack = targetItem;
        }

        public bool findTrack(string target)
        {
            foreach (musicItem item in playlist)
            {
                if (item.name.ToLower().Contains(target))
                {
                    selectTrack(item);
                    return true;
                }
            }
            return false;
        }

        public void clearPlaylist()
        {
            playlist.Clear();
        }

        /* Volumes */
        public void SetVolume(float setTo)
        {
            if (audioFileReader != null)
            {
                audioFileReader.Volume = setTo;
            }
            volume = setTo;
        }

        public void DuckTo(float setTo)
        {

            new Thread(() =>
            {
                if (audioFileReader != null && !locked)
                {
                    locked = true;
                    if (audioFileReader.Volume > setTo)
                    {
                        while (audioFileReader != null && audioFileReader.Volume > setTo)
                        {
                            SetVolume(audioFileReader.Volume - 0.1f);
                            Thread.Sleep(80);
                        }
                    }
                    else
                    {
                        while (audioFileReader != null && audioFileReader.Volume < setTo)
                        {
                            SetVolume(audioFileReader.Volume + 0.1f);
                            Thread.Sleep(80);
                        }
                    }
                }
                locked = false;
            }).Start();
        }



        public void delTracks(System.Collections.IList list)
        {
            List<musicItem> tempList = new List<musicItem>();
            foreach (musicItem item in list)
            {
                tempList.Add(item);
            }

            foreach (musicItem item in tempList)
            {
                playlist.Remove(item);
                if (selectedTrack != null && selectedTrack == item)
                {
                    selectedTrack = null;
                }
            }
        }


    }
}
