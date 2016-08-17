using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    public class WorkStationReturnedSplunkData : DataChangedNotify.NotifyPropertyChanged
    {
        public readonly static Color defaultIconColor = System.Windows.Media.Color.FromArgb((byte)255, (byte)10, (byte)10, (byte)10);
        public readonly static Color defaultBrightnessColor = System.Windows.Media.Color.FromArgb((byte)255, (byte)29, (byte)29, (byte)29);

        private Brush color;
        public Brush Color
        {
            get { return color; }
            set { this.color = value; }
        }

        private Color netWorkColor;
        public Color NetworkColor
        {
            get { return this.netWorkColor; }
            set
            {
                if(this.netWorkColor == value) return;

                this.netWorkColor = value;

                OnPropertyChanged("NetworkColor");
            }
        }

        private Color brightnessNetWorkColor;
        public Color BrightnessNetWorkColor
        {
            get { return this.brightnessNetWorkColor; }
            set
            {
                if (this.brightnessNetWorkColor == value)
                    return;

                this.brightnessNetWorkColor = value;

                OnPropertyChanged("BrightnessNetWorkColor");
            }
        }

        private Color softWareColor;
        public Color SoftwareColor
        {
            get { return this.softWareColor; }
            set
            {
                if(this.softWareColor == value) return;

                this.softWareColor = value;

                OnPropertyChanged("SoftwareColor");
            }
        }

        private Color brightnessSoftWareColor;
        public Color BrightnessSoftWareColor
        {
            get { return this.brightnessSoftWareColor; }
            set
            {
                if (this.brightnessSoftWareColor == value)
                    return;

                this.brightnessSoftWareColor = value;
                OnPropertyChanged("BrightnessSoftWareColor");
            }
        }


        private Color hardWareColor;
        public Color HardwareColor
        {
            get { return this.hardWareColor; }
            set
            {
                if (this.hardWareColor == value) return;
                
                this.hardWareColor = value;

                OnPropertyChanged("HardwareColor");
            }
        }

        private Color brightnessHardwareColor;
        public Color BrightnessHardwareColor
        {
            get { return this.brightnessHardwareColor; }
            set
            {
                if (this.brightnessHardwareColor == value)
                    return;

                this.brightnessHardwareColor = value;
                OnPropertyChanged("BrightnessHardwareColor");
            }
        }


        private string ip;
        public string IP
        {
            get { return this.ip; }
            set
            {
                if (this.IP == value) return;

                this.ip = value;
                OnPropertyChanged("IP");
            }
        }

        private string hostName;
        public string HostName
        {
            get { return this.hostName; }
            set
            {
                if(this.hostName == value) return;

                this.hostName = value;
                OnPropertyChanged("HostName");
            }
        }

        private string os;
        public string OS
        {
            get { return this.os; }
            set
            {
                if(this.os == value) return;

                this.os = value;
                OnPropertyChanged("OS");
            }
        }

        private string windowEventLogName;
        public string WindowEventLogName
        {
            get { return this.windowEventLogName; }
            set
            {
                if(this.windowEventLogName == value) return;

                this.windowEventLogName = value;
                OnPropertyChanged("WindowEventLogName");
            }
        }

        private string eventCount;
        public string EventCount
        {
            get { return this.eventCount; }
            set
            {
                if(this.eventCount == value) return;

                this.eventCount = value;
                OnPropertyChanged("EventCount");
            }
        }

        private string perfObject;
        public string PerfObject
        {
            get { return this.perfObject; }
            set
            {
                if (this.perfObject == value) return;
                
                this.perfObject = value;
                OnPropertyChanged("PerfObject");
            }
        }

        private string perfCounter;
        public string PerfCounter
        {
            get { return this.perfCounter; }
            set
            {
                if(this.perfCounter == value) return;

                this.perfCounter = value;
                OnPropertyChanged("PerfCounter");
            }
        }

        public WorkStationReturnedSplunkData()
        {
            this.HardwareColor = WorkStationReturnedSplunkData.defaultIconColor;
            this.SoftwareColor = WorkStationReturnedSplunkData.defaultIconColor;
            this.NetworkColor = WorkStationReturnedSplunkData.defaultIconColor;

            this.BrightnessHardwareColor = WorkStationReturnedSplunkData.defaultBrightnessColor;
            this.BrightnessSoftWareColor = WorkStationReturnedSplunkData.defaultBrightnessColor;
            this.brightnessNetWorkColor = WorkStationReturnedSplunkData.defaultBrightnessColor;
        }
    }
}
