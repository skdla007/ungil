using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapLinkZoneObjectDataInfo")]
    public class MapLinkZoneObjectDataInfo : BaseMapObjectInfoData
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
                var convertFromString = ColorConverter.ConvertFromString(this.selectedFillColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        public Color SelectedBorderColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.selectedBorderColorString);
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

                if(this.pointCollection.Count > 0)
                {
                    var minx = this.pointCollection.Min(e => e.X);
                    var miny = this.pointCollection.Min(e => e.Y);
                    var maxx = this.pointCollection.Max(e => e.X);
                    var maxy = this.pointCollection.Max(e => e.Y);

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
                if (this.linkedMapGuid == value)
                    return;

                this.linkedMapGuid = value;
                OnPropertyChanged("LinkedMapGuid");
            }
        }

        private string linkedMapBookmarkName;
        [XmlElement("LinkedMapBookmarkName")]
        public string LinkedMapBookmarkName
        {
            get { return this.linkedMapBookmarkName; }
            set
            {
                if (this.linkedMapBookmarkName == value)
                    return;

                this.linkedMapBookmarkName = value;
                OnPropertyChanged("LinkedMapBookmarkName");
            }
        }

        private SplunkBasicInformationData colorSplunkBasicInformationData;
        [XmlElement("ColorSplunkBasicInformationData")]
        public SplunkBasicInformationData ColorSplunkBasicInformationData
        {
            get { return this.colorSplunkBasicInformationData; }
            set
            {
                this.colorSplunkBasicInformationData = value;
                OnPropertyChanged("ColorSplunkBasicInformationData");
            }
        }

        private SplunkBasicInformationData tableSplunkBasicInformationData;
        [XmlElement("TableSplunkBasicInformationData")]
        public SplunkBasicInformationData TableSplunkBasicInformationData
        {
            get { return this.tableSplunkBasicInformationData; }
            set
            {
                this.tableSplunkBasicInformationData = value;
                OnPropertyChanged("TableSplunkBasicInformationData");
            }
        }

        private CommonImageObjectDataInfo imageObjectData;
        [XmlElement("ImageObjectData")]
        public CommonImageObjectDataInfo ImageObjectData
        {
            get { return this.imageObjectData; }
            set
            {
                this.imageObjectData = value;
                OnPropertyChanged("ImageObjectData");
            }
        }

        private bool shouldShowBrowserOnClick;
        [XmlElement("ShouldShowBrowserOnClick")]
        public bool ShouldShowBrowserOnClick
        {
            get { return this.shouldShowBrowserOnClick; }
            set
            {
                this.shouldShowBrowserOnClick = value;
                OnPropertyChanged("ShouldShowBrowserOnClick");
            }
        }

        private string browserUrl;
        [XmlElement("BrowserUrl")]
        public string BrowserUrl
        {
            get { return this.browserUrl; }
            set
            {
                this.browserUrl = value;
                OnPropertyChanged("BrowserUrl");
            }
        }

        #endregion //Field

        #region Construction 

        public MapLinkZoneObjectDataInfo(): base()
        {   
            this.ColorSplunkBasicInformationData = new SplunkBasicInformationData();
            this.TableSplunkBasicInformationData = new SplunkBasicInformationData();
            this.ImageObjectData = new CommonImageObjectDataInfo();
        }

        public MapLinkZoneObjectDataInfo(object data) : base(data)
        {
            var linkZone = data as MapLinkZoneObjectDataInfo;

            if(linkZone == null)
                return;

            this.FillColorString = linkZone.FillColorString;
            this.LinkedMapGuid = linkZone.LinkedMapGuid;
            this.LinkedMapBookmarkName = linkZone.LinkedMapBookmarkName;
            this.PointCollection = linkZone.PointCollection;
            this.BorderColorString = linkZone.borderColorString;
            this.FillColorString = linkZone.FillColorString;
            this.SelectedBorderColorString = linkZone.SelectedBorderColorString;
            this.SelectedFillColorString = linkZone.SelectedFillColorString;
            this.ColorSplunkBasicInformationData = new SplunkBasicInformationData(linkZone.ColorSplunkBasicInformationData);
            this.TableSplunkBasicInformationData = new SplunkBasicInformationData(linkZone.TableSplunkBasicInformationData);
            this.ObjectType = linkZone.ObjectType;
            this.ImageObjectData = new CommonImageObjectDataInfo(linkZone.ImageObjectData);
            this.ShouldShowBrowserOnClick = linkZone.ShouldShowBrowserOnClick;
            this.BrowserUrl = linkZone.BrowserUrl;
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

            if (SelectedMapObjectType == Types.MapObjectType.ImageLinkZone)
            {
                ImageObjectData.ImageOpacity = 1;
            }
        }
        #endregion 
    }
}
