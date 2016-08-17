using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapCameraIconData")]
    public class MapCameraIconObjectDataInfo : BaseMapIconObjectInfoData
    {
        #region Field

        [XmlElement("IconSize")] 
        private decimal iconSize;
        public decimal IconSize
        {
            get { return this.iconSize; }
            set
            {
                this.iconSize = value;
                OnPropertyChanged("IconSize");
            }
        }

        [XmlElement("LabelSize")]
        private decimal labelSize = 1;
        public decimal LabelSize
        {
            get { return this.labelSize; }
            set
            {
                this.labelSize = value;
                OnPropertyChanged("LabelSize");
            }
        }

        [XmlElement("IsVisibleLabel")]
        private bool? isVisibleLabel = true;
        public bool? IsVisibleLabel
        {
            get { return this.isVisibleLabel; }
            set
            {
                this.isVisibleLabel = value;
                OnPropertyChanged("IsVisibleLabel");
            }
        }

        [XmlElement("IsIconVisible")]
        private bool? isIconVisible = ArcGISConstSet.CameraIconDefaultVisibility;
        public bool? IsIconVisible
        {
            get { return this.isIconVisible; }
            set
            {
                this.isIconVisible = value;
                OnPropertyChanged("IsIconVisible");
            }
        }

        #endregion //Field

        #region Construction
        
        public MapCameraIconObjectDataInfo()
        {
            
        }

        public MapCameraIconObjectDataInfo(object data)
            : base(data)
        {
            var iconData = data as MapCameraIconObjectDataInfo;

            if(iconData == null)
                return;

            this.IconSize = iconData.IconSize;
            this.IsVisibleLabel = iconData.IsVisibleLabel;
            this.LabelSize = iconData.LabelSize;
            this.IsIconVisible = iconData.IsIconVisible;
        }

        #endregion //Construction

    }
}
