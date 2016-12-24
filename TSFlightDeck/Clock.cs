using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Razzle
{
    public class timeDisplay: DependencyObject
    {
        public static DependencyProperty DateTimeProperty = 
                DependencyProperty.Register("DateTime", typeof(DateTime), 
                                            typeof(timeDisplay));
        
        public DateTime DateTime
        {
            set { SetValue(DateTimeProperty, value); }
            get { return (DateTime) GetValue(DateTimeProperty); }
        }

        public timeDisplay()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        void TimerOnTick(object sender, EventArgs args)
        {
            DateTime = DateTime.Now;
        }
    }
}