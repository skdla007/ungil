using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public class CameraIconPropertyControlViewModel : CameraPropertyControlBaseViewModel
    {
        #region Field

        private MapCameraIconObjectDataInfo dataInfo;

        private List<string> cameraIconList = new List<string>();
        public List<string> CameraIconList
        {
            get { return this.cameraIconList; }
        }

        public MapCameraIconObjectDataInfo DataInfo
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

        #endregion //Field

        #region Construction

        public CameraIconPropertyControlViewModel()
        {
            this.cameraIconList.Add(Resource_ArcGISControl_Properties.Item_CameraIcon_A);
        }
        #endregion //Construction 
    }
}
