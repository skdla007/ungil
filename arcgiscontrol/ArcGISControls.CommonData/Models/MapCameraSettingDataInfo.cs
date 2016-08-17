using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    public class MapCameraSettingDataInfo : BaseModel
    {
        public List<Size> StreamSizeList { get; set; }
        public string CameraName { get; set; }
        public string CameraInformationId { get; set; }

        private readonly int space = 30;
        public int Space
        {
            get { return this.space; }
        }

        private readonly int standSize = 100;
        public int StandSize
        {
            get { return this.standSize; }
        }

        private readonly int minSize = 30;
        public int MinSize
        {
            get { return this.minSize; }
        }
    }
}
