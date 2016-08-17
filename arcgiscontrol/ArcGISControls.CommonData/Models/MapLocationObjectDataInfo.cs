using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapLocationObjectDataInfo")]
    public class MapLocationObjectDataInfo : BaseMapObjectInfoData
    {
        #region Field

        [XmlElement("Position")]
        private Point position;
        public Point Position
        {
            get { return this.position; }
            set
            {
                this.position = value;
                OnPropertyChanged("Position");

                this.ExtentMin = position;
                this.ExtentMax = position;
            }
        }

        [XmlElement("Address")]
        private string address;
        public string Address
        {
            get { return this.address; }
            set
            {
                this.address = value;
                OnPropertyChanged("Address");
            }
        }
        
        [XmlElement("ImageUrl")]
        private string imageUrl;
        public string ImageUrl
        {
            get { return this.imageUrl; }
            set
            {
                this.imageUrl = value;
                OnPropertyChanged("ImageUrl");
            }
        }
       
        [XmlElement("IconUri")]
        protected string iconUri;
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

        [XmlElement("IconSize")]
        private double iconSize;
        public double IconSize
        {
            get { return this.iconSize; }
            set
            {
                this.iconSize = value;
                OnPropertyChanged("IconSize");
            }
        }


        #endregion //Field

        #region Construction 

        public MapLocationObjectDataInfo() : base()
        {
            this.ObjectType = MapObjectType.Location;
            this.IconSize = ArcGISConstSet.LocationIconSize;
            this.IconUri = ArcGISConstSet.LocationIconNormalUri;
            this.IconSelectedStringUri = ArcGISConstSet.LocationIconSelectedUri;
        }

        public MapLocationObjectDataInfo(object data) : base(data)
        {
            var mapLocationData = data as MapLocationObjectDataInfo;

            if(mapLocationData == null)
                return;

            this.Address = mapLocationData.Address;
            this.IconUri = mapLocationData.IconUri;
            this.IconSelectedStringUri =  mapLocationData.IconSelectedStringUri;
            this.Position = mapLocationData.Position;
            this.ImageUrl = mapLocationData.ImageUrl;
            this.ObjectType = mapLocationData.ObjectType;
            this.IconSize = mapLocationData.IconSize;
        }

        #endregion //Construciton 

        #region Method
        public override void Init(MapObjectType SelectedMapObjectType, string pName, Point MouseClickByMapPoint, List<Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            Position = MouseClickByMapPoint;
            Name = pName;
            InitName = pName;
        }
        #endregion 
    }
}
