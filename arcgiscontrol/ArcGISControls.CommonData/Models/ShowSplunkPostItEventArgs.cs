using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Models
{
    public class ShowSplunkPostItEventArgs : EventArgs
    {
        public SplunkBasicInformationData SplunkBasicInformation { get; private set; }

        public SplunkPostItData SplunkPostItData { get; private set; }

        public ShowSplunkPostItEventArgs(SplunkBasicInformationData splunkBasicInformation, SplunkPostItData splunkPostItData)
        {
            this.SplunkBasicInformation = splunkBasicInformation;
            this.SplunkPostItData = splunkPostItData;
        }
    }
}
