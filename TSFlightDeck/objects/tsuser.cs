using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Razzle
{
    class tsuser
    {
        public string name { get; set; }
        public string uid { get; set; }

        public override string ToString()
        {
            return (name + " (" + uid + ")");
        }
    }
}
