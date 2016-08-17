using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public class PlacePropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private MapLocationObjectDataInfo dataInfo;

        public MapLocationObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;
                this.dataInfo = value;
                this.OnPropertyChanged("DataInfo");

                if (dataInfo is MapAddressObjectDataInfo)
                    this.DescriptionLabel = Resource_ArcGISControl_Properties.Label_PlaceAddress;
                else
                    this.DescriptionLabel = Resource_ArcGISControl_Properties.Label_PlaceValue;
            }
        }

        private bool isSingleSetting;
        public bool IsSingleSetting
        {
            get { return this.isSingleSetting; }
            set
            {
                this.isSingleSetting = value;
                OnPropertyChanged("IsSingleSetting");
            }
        }

        private string descriptionLabel;
        public string DescriptionLabel
        {
            get { return this.descriptionLabel; }
            set
            {
                this.descriptionLabel = value;
                OnPropertyChanged("DescriptionLabel");
            }
        }
    }
}
