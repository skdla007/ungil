using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.PropertyControl
{
    public class SplunkArgumentItem : DataChangedNotify.NotifyPropertyChanged
    {
        private string splunkArgumentKey;

        public string SplunkArgumentKey
        {
            get { return this.splunkArgumentKey; }
            set
            {
                this.splunkArgumentKey = value;
                OnPropertyChanged("SplunkArgumentKey");
            }
        }

        private string splunkArgumentvalue;

        public string SplunkArgumentValue
        {
            get { return this.splunkArgumentvalue; }
            set
            {
                this.splunkArgumentvalue = value;
                OnPropertyChanged("SplunkArgumentValue");
            }
        }

    }
}
