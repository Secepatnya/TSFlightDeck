using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Razzle
{
    class eventItem
    {
        
        
        public string name { get; set; }
        public Action command { get; set; }
        public DateTime startTime { get; set; }
        public DayOfWeek dayName { get; set; }
        public bool hasFired { get; set; }


        public override string ToString()
        {
            return name;
        }
    }
}
