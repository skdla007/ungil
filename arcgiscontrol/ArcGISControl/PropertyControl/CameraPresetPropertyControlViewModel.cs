using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public class CameraPresetPropertyControlViewModel : CameraPropertyControlBaseViewModel
    {
        private MapCameraPresetObjectDataInfo dataInfo;

        public MapCameraPresetObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;
                this.dataInfo = value;
                this.OnPropertyChanged("DataInfo");
            }
        }

        private List<string> presetList;

        public List<string> PresetList
        {
            get { return this.presetList; }
            set
            {
                if (this.presetList == value)
                {
                    this.IsEnabledPresetList = false;
                    return;
                }
                    
                this.presetList = value;
                this.IsEnabledPresetList = this.presetList.Count > 0 ? true : false;
                this.OnPropertyChanged("PresetList");
            }
        }

        private bool isEnabledPresetList;
        public bool IsEnabledPresetList
        {
            get { return this.isEnabledPresetList; }
            set
            {
                this.isEnabledPresetList = value;
                OnPropertyChanged("IsEnabledPresetList");
            }
        }


        public Color FillColor
        {
            get { return this.dataInfo.FillColor; }
            set
            {
                if (this.dataInfo.FillColor == value)
                    return;
                this.dataInfo.FillColorString = value.ToString();
                this.OnPropertyChanged("FillColor");
            }
        }

        public Color OutlineColor
        {
            get { return this.dataInfo.BorderColor; }
            set
            {
                if (this.dataInfo.BorderColor == value)
                    return;
                this.dataInfo.BorderColorString = value.ToString();
                this.OnPropertyChanged("OutlineColor");
            }
        }

        #region Command


        #endregion
    }
}
