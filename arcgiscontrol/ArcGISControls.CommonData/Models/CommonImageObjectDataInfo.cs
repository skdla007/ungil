using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("CommonImageObjectDataInfo")]
    public class CommonImageObjectDataInfo : BaseModel
    {
        private string imageDataStream;
        [XmlElement("ImageDataStream")]
        public string ImageDataStream
        {
            get { return this.imageDataStream; }
            set
            {
                this.imageDataStream = value;
                OnPropertyChanged("ImageDataStream");
            }
        }

        private string imageFileName;
        [XmlElement("ImageFileName")]
        public string ImageFileName
        {
            get { return this.imageFileName; }
            set
            {
                this.imageFileName = value;
                OnPropertyChanged("ImageFileName");
            }
        }

        private double imageOpacity;
        [XmlElement("ImageOpacity")]
        public double ImageOpacity
        {
            get { return this.imageOpacity; }
            set
            {
                this.imageOpacity = value;
                OnPropertyChanged("ImageOpacity");
            }
        }

        public CommonImageObjectDataInfo()
        {
            
        }

        public CommonImageObjectDataInfo(CommonImageObjectDataInfo dataInfo) :base()
        {
            this.ImageDataStream = dataInfo.ImageDataStream;
            this.ImageFileName = dataInfo.ImageFileName;
            this.ImageOpacity = dataInfo.ImageOpacity;
        }
    }
}
