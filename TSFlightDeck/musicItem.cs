using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using System.Threading;
using System.Configuration;

namespace Razzle
{

    public class musicItem : ApplicationSettingsBase
    {
        public string url { get; set; }
        public string name { get; set; }

        
        public override string ToString()
        {
            return name;
        }
    }

}
