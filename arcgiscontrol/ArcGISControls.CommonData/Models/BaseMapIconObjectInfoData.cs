using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// IconData 저장을 위한 DataModel 이랄까? 
    /// </summary>
    public class BaseMapIconObjectInfoData : BaseModel
    {
        #region Field

        [XmlElement("Position")]
        protected Point position;
        public Point Position
        {
            get { return this.position; }
            set
            {
                this.position = value;
                OnPropertyChanged("Position");
            }
        }

        [XmlElement("Visible")]
        protected bool visible = true;
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                this.visible = value;
                OnPropertyChanged("Visible");
            }
        }

        [XmlElement("IconUri")]
        protected  string iconUri;
        public string IconUri
        {
            get { return this.iconUri; }
            set
            {   
                this.iconUri = value;
                OnPropertyChanged("IconUri");
            }
        }

        [XmlElement("IconSelectedUri")]
        protected string iconSelectedUri;
        public string IconSelectedStringUri
        {
            get { return this.iconSelectedUri; }
            set
            {
                this.iconSelectedUri = value;
                OnPropertyChanged("IconSelectedStringUri");
            }
        }

        private int objectZIndex = ArcGISConstSet.UndefinedZIndex;
        [XmlElement("ObjectZIndex")]
        public int ObjectZIndex
        {
            get { return this.objectZIndex; }
            set
            {
                this.objectZIndex = value;
                OnPropertyChanged("ObjectZIndex");
            }
        }

        #endregion //Field

        #region Construction
        public BaseMapIconObjectInfoData()
        {
            
        }

        public BaseMapIconObjectInfoData(object data)
        {
            var baseMapObjectInfoData = data as BaseMapIconObjectInfoData;

            if(baseMapObjectInfoData == null)
                return;

            this.IconUri = baseMapObjectInfoData.IconUri;
            this.Position = baseMapObjectInfoData.Position;
            this.Visible = baseMapObjectInfoData.Visible;
            this.IconSelectedStringUri = baseMapObjectInfoData.IconSelectedStringUri;
            this.ObjectZIndex = baseMapObjectInfoData.ObjectZIndex;
        }

        #endregion //Construction

        #region Method

        
        #endregion 
    }
}
