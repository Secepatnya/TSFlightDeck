using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Razzle
{
    public class musicItem
    {
        public string url { get; set; }
        public string name { get; set; }

        
        public override string ToString()
        {
            return name;
        }
    }

}
