using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace TileRestService
{
    public class DelayTimeCheck
    {
        private static readonly Lazy<DelayTimeCheck> lazy =
                            new Lazy<DelayTimeCheck>(() => new DelayTimeCheck());

        public static DelayTimeCheck Instance { get { return lazy.Value; } }

        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        private TimeSpan DelayedTime { get; set; }

        private int TimerCount = 0;
        
        private Timer delayedTimer;
        private Timer DelayedTimer
        {
            get
            {
                if (this.delayedTimer == null)
                {
                    this.delayedTimer = new Timer();
                    this.delayedTimer.Interval = 1000;

                    this.delayedTimer.Elapsed += (o, e) =>
                        {
                            this.TimerCount++;
                        };
                }
                return delayedTimer;
            }

            set
            {
                this.delayedTimer = value;
            }
        }

        private DelayTimeCheck()
        {
        }

        public void TimerStart()
        {
            this.StartTime = DateTime.Now;
            this.TimerCount = 0;
            this.DelayedTimer.Start();
        }

        public string GetDelayedTime()
        {
            this.DelayedTime = DateTime.Now.Subtract(this.StartTime);
            string time = String.Format("Delayed Time => {0}ms", Convert.ToInt32(this.DelayedTime.TotalMilliseconds));
            this.TimerStop();
            return time;
        }

        public void TimerStop()
        {
            this.TimerCount = 0;
            this.DelayedTimer.Stop();
        }
    }
}