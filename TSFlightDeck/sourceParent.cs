using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Razzle
{
    class sourceParent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        /* Trackers */
        private float _volume;
        public float volume
        {
            get { return _volume; }
            set { if (_volume != value) { _volume = value; NotifyPropertyChanged("volume"); } }
        }

        private string _sourceState;
        public string sourceState
        {
            get { return _sourceState; }
            set { if (_sourceState != value) { _sourceState = value; NotifyPropertyChanged("sourceState"); } }
        }

        public bool locked = false;

    }
}
