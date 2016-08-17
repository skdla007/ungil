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

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapWorkStationObjectDataInfo")]
    public class MapWorkStationObjectDataInfo : BaseMapObjectInfoData
    {
        #region Field

        public Color FillColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.fillColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        public Color BorderColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.borderColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        public Color SelectedFillColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.SelectedFillColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        public Color SelectedBorderColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.SelectedBorderColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        private string fillColorString;
        [XmlElement("FillColorString")]
        public string FillColorString
        {
            get { return this.fillColorString; }
            set
            {
                this.fillColorString = value;
                OnPropertyChanged("FillColorString");
            }
        }

        private string borderColorString;
        [XmlElement("BorderColorString")]
        public string BorderColorString
        {
            get { return this.borderColorString; }
            set
            {
                this.borderColorString = value;
                OnPropertyChanged("BorderColorString");
            }
        }

        private string selectedFillColorString;
        [XmlElement("SelectedFillColorString")]
        public string SelectedFillColorString
        {
            get { return this.selectedFillColorString; }
            set
            {
                this.selectedFillColorString = value;
                OnPropertyChanged("SelectedFillColorString");
            }
        }

        private string selectedBorderColorString;
        [XmlElement("SelectedBorderColorString")]
        public string SelectedBorderColorString
        {
            get { return this.selectedBorderColorString; }
            set
            {
                this.selectedBorderColorString = value;
                OnPropertyChanged("SelectedBorderColorString");
            }
        }

        private List<Point> pointCollection;
        [XmlArray("PointCollection")]
        [XmlArrayItem("Point", typeof(Point))]
        public List<Point> PointCollection
        {
            get { return this.pointCollection; }
            set
            {
                this.pointCollection = value;
                OnPropertyChanged("PointCollection");

                if (this.pointCollection.Count > 0)
                {
                    var minx = (from t in this.pointCollection select t).Min(e => e.X);
                    var miny = (from t in this.pointCollection select t).Min(e => e.Y);
                    var maxx = (from t in this.pointCollection select t).Max(e => e.X);
                    var maxy = (from t in this.pointCollection select t).Max(e => e.Y);

                    this.ExtentMin = new Point(minx, miny);
                    this.ExtentMax = new Point(maxx, maxy);
                }
            }
        }

        private string linkedMapGuid;
        [XmlElement("LinkedMapGuid")]
        public string LinkedMapGuid
        {
            get { return this.linkedMapGuid; }
            set
            {
                this.linkedMapGuid = value;
                OnPropertyChanged("LinkedMapGuid");
            }
        }

        private string networkViewLinkedMapGuid;
        [XmlElement("NetworkViewLinkedMapGuid")]
        public string NetworkViewLinkedMapGuid
        {
            get { return this.networkViewLinkedMapGuid; }
            set
            {
                this.networkViewLinkedMapGuid = value;
                OnPropertyChanged("NetworkViewLinkedMapGuid");
            }
        }

        private string softwareViewLinkedMapGuid;
        [XmlElement("SoftwareViewLinkedMapGuid")]
        public string SoftwareViewLinkedMapGuid
        {
            get { return this.softwareViewLinkedMapGuid; }
            set
            {
                this.softwareViewLinkedMapGuid = value;
                OnPropertyChanged("SoftwareViewLinkedMapGuid");
            }
        }

        private string hardwareViewLinkedMapGuid;
        [XmlElement("HardwareViewLinkedMapGuid")]
        public string HardwareViewLinkedMapGuid
        {
            get { return this.hardwareViewLinkedMapGuid; }
            set
            {
                this.hardwareViewLinkedMapGuid = value;
                OnPropertyChanged("HardwareViewLinkedMapGuid");
            }
        }

        private string searchViewUrl;
        [XmlElement("SearchViewUrl")]
        public string SearchViewUrl
        {
            get { return this.searchViewUrl; }
            set
            {
                this.searchViewUrl = value;
                OnPropertyChanged("SearchViewUrl");
            }
        }

        private SplunkBasicInformationData splunkBasicInformation;
        [XmlElement("SplunkBasicInformation")]
        public SplunkBasicInformationData SplunkBasicInformation
        {
            get { return this.splunkBasicInformation; }
            set
            {
                this.splunkBasicInformation = value;
                OnPropertyChanged("SplunkBasicInformation");
            }
        }

        private WorkStationReturnedSplunkData workStationReturnedData;
        
        [XmlIgnore]
        public WorkStationReturnedSplunkData WorkStationReturnedData
        {
            get { return this.workStationReturnedData; }
            set
            {
                this.workStationReturnedData = value;
                OnPropertyChanged("WorkStationReturnedData");
            }
        }

        #endregion //Field

        #region Construction

        public MapWorkStationObjectDataInfo() : base()
        {
            this.SplunkBasicInformation = new SplunkBasicInformationData();
            this.WorkStationReturnedData = new WorkStationReturnedSplunkData();
        }

        public MapWorkStationObjectDataInfo(BaseMapObjectInfoData data) : base(data)
        {
            var workStationObjectData = data as MapWorkStationObjectDataInfo;

            if(workStationObjectData == null) return;

            this.FillColorString = workStationObjectData.FillColorString;
            this.LinkedMapGuid = workStationObjectData.LinkedMapGuid;
            this.PointCollection = workStationObjectData.PointCollection;
            this.BorderColorString = workStationObjectData.borderColorString;
            this.FillColorString = workStationObjectData.FillColorString;
            this.SelectedBorderColorString = workStationObjectData.SelectedBorderColorString;
            this.SelectedFillColorString = workStationObjectData.SelectedFillColorString;
            this.ObjectType = workStationObjectData.ObjectType;
            this.NetworkViewLinkedMapGuid = workStationObjectData.NetworkViewLinkedMapGuid;
            this.HardwareViewLinkedMapGuid = workStationObjectData.HardwareViewLinkedMapGuid;
            this.SoftwareViewLinkedMapGuid = workStationObjectData.SoftwareViewLinkedMapGuid;
            this.SearchViewUrl = workStationObjectData.SearchViewUrl;
            this.SplunkBasicInformation = new SplunkBasicInformationData(workStationObjectData.SplunkBasicInformation);
        }

        #endregion //Construction

        #region Method
        public override void Init(ArcGISControls.CommonData.Types.MapObjectType SelectedMapObjectType, string pName, Point MouseClickByMapPoint, List<Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            Name = pName;
            BorderColorString = ArcGISConstSet.LinkZoneNormalColor.ToString();
            FillColorString = ArcGISConstSet.LinkZoneNormalColor.ToString();
            SelectedBorderColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString();
            SelectedFillColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString();
            PointCollection = pPointCollection;
            ObjectType = SelectedMapObjectType;
        }
        #endregion //Method
    }
}
