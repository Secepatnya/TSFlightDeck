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

namespace TSFlightDeck
{
    class sourcePlayer : sourceParent
    {
        public enum playModes { Single, RepeatOne, Continous }

        /* Music File Manager */
        public ObservableCollection<musicItem> playlist;

        private musicItem selectedTrack;

        AudioFileReader audioFileReader;
        MixingSampleProvider mixer;

        string[] allowedFileTypes = { ".mp3", ".wma", ".wav" };

        /* Trackers */
        public bool autoNext = false;
        bool stopped = true;

        private playModes _selectedPlayMode;
        public playModes selectedPlayMode
        {
            get { return _selectedPlayMode; }
            set { if (_selectedPlayMode != value) { _selectedPlayMode = value; NotifyPropertyChanged("selectedPlayMode"); } }
        }
      
        /* Program */

        public sourcePlayer(MixingSampleProvider targetMixer)
        {
            
            playlist = new ObservableCollection<musicItem>();
            volume = 1f;
            sourceState = "Stopped";
            selectedPlayMode = playModes.Single;

            mixer = targetMixer;

        }

        #region PlayerControls
        
        public void Start(musicItem track)
        {
            selectTrack(track);
            Start();
        }

        public void Start()
        {
            if (audioFileReader != null)
            {
                Stop();
            }

            if (selectedTrack == null && playlist != null && playlist.Count > 0)
            {
                selectTrack(playlist[0]);
            }

            try
            {

                if (audioFileReader == null && selectedTrack != null && File.Exists(selectedTrack.url))
                {
                    audioFileReader = new AudioFileReader(selectedTrack.url);
                    audioFileReader.Volume = volume;

                    //mixer.AddInputStream(audioFileReader);
                    mixer.AddMixerInput((ISampleProvider)audioFileReader);
                    sourceState = "Playing: " + selectedTrack.name;
                    tsInterface.sendMessage("正在播放: " + Path.GetFileNameWithoutExtension(selectedTrack.name));

                    stopped = false;

                    while (audioFileReader != null && audioFileReader.Position < audioFileReader.Length)
                    {
                        Thread.Sleep(1000);
                    }
                    sourceState = "Stopped";
                }
            }
            catch
            {
                sourceState = "Cannot play file.";
				stopped = false;
            }

            if (!stopped)
            {
                if (selectedTrack != null && !File.Exists(selectedTrack.url))
                {
                    Console.WriteLine("Selected track {0} was deleted, can't play.", selectedTrack.name);
                }

                if (selectedPlayMode == playModes.Continous)
                {
                    skip(true);
                }
                else if (selectedPlayMode == playModes.RepeatOne)
                {
                    Start();
                }
            }
        }

        public void skip(bool isNext)
        {
            if (selectedTrack != null) {
                int currentIndex;

                currentIndex = playlist.IndexOf(selectedTrack);

                // Skip next
                if (isNext && currentIndex < playlist.Count - 1 )
                {
                    selectedTrack = playlist[currentIndex + 1];
                    Start();
                }
                else if (isNext && playlist[0] != null)
                {
                    selectedTrack = playlist[0];
                    Start();
                }

                //Skip prev
                else if (!isNext && currentIndex > 0)
                {
                    selectedTrack = playlist[currentIndex - 1];
                    Start();
                }
                else if (!isNext && playlist[playlist.Count-1] != null)
                {
                    selectedTrack = playlist[playlist.Count-1];
                    Start();
                }
                else
                {
                    Start();
                }
            }
        }


        public void Stop()
        {
            if (audioFileReader != null)
            {
                mixer.RemoveMixerInput(audioFileReader);

                audioFileReader.Dispose();
                audioFileReader = null;

                sourceState = "Stopped" ;
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
            if (audioFileReader != null && !locked)
            {
                locked = true;
                if (audioFileReader.Volume > setTo)
                {
                    while (audioFileReader != null && audioFileReader.Volume > setTo)
                    {
                        SetVolume(audioFileReader.Volume - 0.05f);
                        Thread.Sleep(11);
                    }
                }
                else
                {
                    while (audioFileReader != null && audioFileReader.Volume < setTo)
                    {
                        SetVolume(audioFileReader.Volume + 0.05f);
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
