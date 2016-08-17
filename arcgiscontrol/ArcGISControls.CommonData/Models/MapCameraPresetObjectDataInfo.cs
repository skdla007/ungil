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
    [XmlRoot("CameraObjectDataComponent")]
    public class MapCameraPresetObjectDataInfo : BaseMapPolygonObjectInfoData
    {
        #region Field

        [XmlElement("PresetNum")]
        private string presetNum;
        public string PresetNum
        {
            get { return this.presetNum; }
            set
            {
                this.presetNum = value;
                OnPropertyChanged("PresetNum");
            }
        }

        #endregion //Field

        #region Construction

        public MapCameraPresetObjectDataInfo() {}

        public MapCameraPresetObjectDataInfo(object data)
            : base(data)
        {
            var presetObjectDataInfo = data as MapCameraPresetObjectDataInfo;

            if(presetObjectDataInfo == null)
                return;

            this.PresetNum = presetObjectDataInfo.PresetNum;
        }

        #endregion //Construction

        #region Method
        
        #endregion 

        #region Command

        #endregion 
    }
}
