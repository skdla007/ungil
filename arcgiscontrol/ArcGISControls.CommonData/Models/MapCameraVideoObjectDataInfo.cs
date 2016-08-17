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
    [XmlRoot("MapCameraVideoData")]
    public class MapCameraVideoObjectDataInfo : BaseMapPolygonObjectInfoData
    {
        #region Field

        /// <summary>
        /// Locked Level로 처리 해야할듯 
        /// 이름 변경 요망
        /// </summary>
        [XmlElement("IsLockSize")]
        private bool? isLockSize;
        public bool? IsLockSize
        {
            get { return this.isLockSize; }
            set
            {
                this.isLockSize = value;
                OnPropertyChanged("IsLockSize");
            }
        }

        [XmlElement("LockedResoultion")]
        private double lockedResoultion;
        public double LockedResoultion
        {
            get { return this.lockedResoultion; }
            set
            {  
                if(value  == 0.0)
                    return;

                this.lockedResoultion = value;

                OnPropertyChanged("LockedResoultion");
            }
        }

        [XmlElement("LockedPosition")]
        public Point LockedPosition { get; set; }

        [XmlElement("ConstrainProportion")]
        private bool? constrainProprtion;
        public bool? ConstrainProportion
        {
            get { return this.constrainProprtion; }
            set
            {
                this.constrainProprtion = value;
                OnPropertyChanged("ConstrainProportion");
            }
        }

        [XmlElement("AlwaysKeepToCameraVideo")]
        private bool? alwaysKeepToCameraVideo;
        public bool? AlwaysKeepToCameraVideo
        {
            get { return this.alwaysKeepToCameraVideo; }
            set
            {
                this.alwaysKeepToCameraVideo = value;
                OnPropertyChanged("AlwaysKeepToCameraVideo");
            }
        }

        /// <summary>
        /// 현재 Video의 크기로 설정된 값 
        /// HiddenMinLeve은 이 이하로 설정 할수 없다
        /// </summary>
        public double DefaultHiddenMinLevel { get; set; }

        /// <summary>
        /// 현재 Video의 크기로 설정된 값 
        /// HiddenMinLeve은 이 이상으로 설정 할수 없다
        /// 대부분 Map이 제공하는 Max Level 이겠지
        /// </summary>
        public double DefaultHiddenMaxLevel { get; set; }

        [XmlElement("HiddenMinLevel")]
        private int hiddenMinLevel;
        public int HiddenMinLevel
        {
            get { return this.hiddenMinLevel; }
            set
            {
                if (value == 0)
                    return;

                if (value != -1 && this.HiddenMaxLevel != 0 && (this.HiddenMaxLevel != -1 && this.HiddenMaxLevel < value))
                    return;

                this.hiddenMinLevel = value;
                OnPropertyChanged("HiddenMinLevel");
            }
        }

        [XmlElement("HiddenMaxLevel")]
        private int hiddenMaxLevel;
        public int HiddenMaxLevel
        {
            get { return this.hiddenMaxLevel; }
            set
            {
                if (value == 0)
                    return;

                if (value != -1 && this.hiddenMinLevel != 0 && (this.hiddenMinLevel != -1 && this.hiddenMinLevel > value))
                    return;

                this.hiddenMaxLevel = value;
                OnPropertyChanged("HiddenMaxLevel");
            }
        }

        public bool IsScaled { get; set; }
        
        [XmlElement("ScaleMaxLevel")]
        private double scaleMaxLevel;
        public double ScaleMaxLevel
        {
            get { return this.scaleMaxLevel; }
            set
            {
                if (value == 0.0)
                    return;

                this.scaleMaxLevel = value;
                OnPropertyChanged("ScaleMaxLevel");
            }
        }

        [XmlElement("ScaleMaxResoultion")]
        private double scaleMaxResoultion;
        public double ScaleMaxResoultion 
        {
            get
            {
                return this.scaleMaxResoultion;
            }
            set
            {
                if (value == 0.0)
                    return;

                this.scaleMaxResoultion = value;
                OnPropertyChanged("ScaleMaxResoultion");
            }
        }

        //Camera Ratio 정보
        [XmlArray("StreamSizeList")]
        [XmlArrayItem("Size", typeof(Size))]
        private List<Size> streamSizeList;
        public List<Size> StreamSizeList
        {
            get { return this.streamSizeList; }
            set 
            {
                this.streamSizeList = value;
            }
        }

        //Camera의 최고 작아지는 사이즈 (일단 30으로)
        [XmlElement("MinSize")]
        private double minSize;
        public double MinSize
        {
            get { return this.minSize; }
            set { this.minSize = value; }
        }

        #endregion //Field

        #region Construction

        public MapCameraVideoObjectDataInfo() : base()
        {
            this.ConstrainProportion = false;
            this.AlwaysKeepToCameraVideo = false;
            this.IsLockSize = false;
            this.HiddenMinLevel = -1;
            this.HiddenMaxLevel = -1;
            this.minSize = 30;
        }

        public MapCameraVideoObjectDataInfo(object data)
            : base(data)
        {
            var videoData = data as MapCameraVideoObjectDataInfo;

            if(videoData == null)
                return;

            this.IsLockSize = videoData.IsLockSize;
            this.LockedPosition = videoData.LockedPosition;
            this.ConstrainProportion = videoData.ConstrainProportion;
            this.AlwaysKeepToCameraVideo = videoData.AlwaysKeepToCameraVideo;
            this.HiddenMinLevel = videoData.HiddenMinLevel;
            this.HiddenMaxLevel = videoData.HiddenMaxLevel;
            this.ScaleMaxLevel = videoData.ScaleMaxLevel;
            this.LockedResoultion = videoData.LockedResoultion;
            this.ScaleMaxResoultion = videoData.ScaleMaxResoultion;
            this.MinSize = videoData.MinSize;
        }

        #endregion //Construction

        #region Method

        #endregion 
    }
}
