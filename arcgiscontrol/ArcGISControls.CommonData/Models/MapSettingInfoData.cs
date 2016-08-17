using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// Map 기본 정보
    /// </summary>
    [XmlRoot("MapSettingInfoData")]
    public class MapSettingInfoData : BaseModel
    {
        [XmlElement("ExtentMin")]
        private Point extentMin;
        public Point ExtentMin
        {
            get { return this.extentMin; }
            set
            {
                this.extentMin = value;
                OnPropertyChanged("ExtentMin");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ExtentMax")]
        private Point extentMax;
        public Point ExtentMax
        {
            get { return this.extentMax; }
            set
            {
                this.extentMax = value;
                OnPropertyChanged("ExtentMax");
            }
        }

        [XmlElement("LicenseKey")]
        private string licenseKey;
        public string LicenseKey
        {
            get { return this.licenseKey; }
            set
            {
                if (value == null || this.licenseKey == value)
                    return;

                this.licenseKey = value;
                OnPropertyChanged("LicenseKey");
            }
        }

        [XmlElement("Level")]
        private double level;
        public double Level
        {
            get { return this.level; }
            set
            { 
                this.level = value;
                OnPropertyChanged("Level");
            }
        }

        [XmlElement("MapServiceUrl")]
        private string mapServiceUrl;
        public string MapServiceUrl
        {
            get { return this.mapServiceUrl; }
            set
            {
                if (value == null || this.mapServiceUrl == value)
                    return;

                this.mapServiceUrl = value;
                OnPropertyChanged("MapServiceUrl");
            }
        }

        [XmlElement("CustomMapServiceDir")]
        private string customMapServiceDir;
        public string CustomMapServiceDir
        {
            get { return this.customMapServiceDir; }
            set
            {
                this.customMapServiceDir = value;
                OnPropertyChanged("CustomMapServiceDir");
            }
        }

        [XmlElement("CustomMapServiceGuid")]
        private string customMapServiceGuid;
        public string CustomMapServiceGuid
        {
            get { return this.customMapServiceGuid; }
            set
            {
                this.customMapServiceGuid = value;
                OnPropertyChanged("CustomMapServiceGuid");
            }
        }

        [XmlElement("MapServiceProviderInfoIndex")]
        private int mapServiceProviderInfoIndex;
        public int MapServiceProviderInfoIndex
        {
            get { return this.mapServiceProviderInfoIndex; }
            set
            {
                this.mapServiceProviderInfoIndex = value;
                OnPropertyChanged("MapServiceProviderInfoIndex");
            }
        }

        [XmlElement("MapType")]
        private MapProviderType mapType;
        public MapProviderType MapType
        {
            get { return this.mapType; }
            set 
            {
                if (this.mapType == value)
                    return;

                this.mapType = value;
                OnPropertyChanged("MapType");
            }
        }

        [XmlElement("Name")]
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if(value == null || this.name == value)
                    return;

                this.name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlElement("Description")]
        private string description;
        public string Description
        {
            get { return this.description; }
            set
            {
                if (value == null || this.description == value)
                    return;

                this.description = value;
                OnPropertyChanged("Description");
            }
        }

        [XmlElement("ID")]
        private string id;
        public string ID
        {
            get { return this.id; }
            set
            {
                if (value == null || this.id == value)
                    return;

                this.id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Service 타입
        /// </summary>
        public MapProviderType MapServiceType
        {
            get
            {
                switch (this.mapType)
                {
                    case MapProviderType.DaumSatelliteHybridMap:
                    case MapProviderType.DaumMap:
                    case MapProviderType.DaumSatelliteMap:
                    case MapProviderType.DaumSatelliteTrafficMap:
                    case MapProviderType.DaumTrafficMap:
                        return MapProviderType.DaumMap;
                        break;
                    case MapProviderType.NaverSatelliteHybridMap:
                    case MapProviderType.NaverMap:
                    case MapProviderType.NaverSatelliteMap:
                    case MapProviderType.NaverSatelliteTrafficMap:
                    case MapProviderType.NaverTrafficMap:
                        return MapProviderType.NaverMap;
                        break;
                    case MapProviderType.ArcGisImageryMap:
                    case MapProviderType.ArcGisStreetMap:
                    case MapProviderType.ArcGisTogoMap:
                    case MapProviderType.ArcGisClientMap:
                        return MapProviderType.ArcGisClientMap;
                        break;
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                        return MapProviderType.ArcGisClientMap;
                        break;
                    default:
                        return MapProviderType.CustomMap;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MapProviderType MapSelectedType
        {
            get
            {
                switch (this.mapType)
                {
                    case MapProviderType.DaumSatelliteHybridMap:
                    case MapProviderType.DaumMap:
                    case MapProviderType.DaumSatelliteMap:
                    case MapProviderType.DaumSatelliteTrafficMap:
                    case MapProviderType.DaumTrafficMap:
                    case MapProviderType.NaverSatelliteHybridMap:
                    case MapProviderType.NaverMap:
                    case MapProviderType.NaverSatelliteMap:
                    case MapProviderType.NaverSatelliteTrafficMap:
                    case MapProviderType.NaverTrafficMap:
                        return MapProviderType.NaverMap;
                        break;
                    case MapProviderType.ArcGisImageryMap:
                    case MapProviderType.ArcGisStreetMap:
                    case MapProviderType.ArcGisTogoMap:
                    case MapProviderType.ArcGisClientMap:
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                        return MapProviderType.ArcGisClientMap;
                        break;
                    default:
                        return MapProviderType.CustomMap;
                }
            }
        }

        public MapSettingInfoData()
        {
            this.MapType = MapProviderType.CustomMap;

            this.ExtentMin = new Point();
            this.ExtentMax = new Point();
        }

        /// <summary>
        /// Copy Data
        /// </summary>
        /// <param name="data"></param>
        public MapSettingInfoData(object data)
        {
            var mapSettingInfo = data as MapSettingInfoData;

            if(mapSettingInfo == null)
                return;

            this.ID = mapSettingInfo.ID;
            this.Level = mapSettingInfo.Level;
            this.LicenseKey = mapSettingInfo.LicenseKey;
            
            this.MapType = mapSettingInfo.MapType;
            this.ExtentMax = mapSettingInfo.ExtentMax;
            this.ExtentMin = mapSettingInfo.ExtentMin;
            this.Name = mapSettingInfo.Name;
            this.Description = mapSettingInfo.Description;

            this.CustomMapServiceDir = mapSettingInfo.CustomMapServiceDir;
            this.CustomMapServiceGuid = mapSettingInfo.CustomMapServiceGuid;
            this.MapServiceUrl = mapSettingInfo.MapServiceUrl;
        }

        public static MapSettingInfoData ReadDataFromXML(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData) || string.IsNullOrWhiteSpace(xmlData))
                return null;

            var serializer = new XmlSerializer(typeof(MapSettingInfoData));
            var stringReader = new StringReader(xmlData);
            var xmlReader = new XmlTextReader(stringReader);

            var data = serializer.Deserialize(xmlReader) as MapSettingInfoData;

            xmlReader.Close();
            stringReader.Close();

            return data;
        }

        public string SaveDataToXML()
        {
            var serializer = new XmlSerializer(typeof(MapSettingInfoData));
            var memStream = new MemoryStream();

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = new string(' ', 4);
            settings.NewLineOnAttributes = false;
            settings.Encoding = Encoding.UTF8;

            XmlWriter xmlWriter = XmlWriter.Create(memStream, settings);
            if (xmlWriter != null)
            {
                serializer.Serialize(xmlWriter, this);
                xmlWriter.Close();
            }

            memStream.Close();

            string xmlData = Encoding.UTF8.GetString(memStream.GetBuffer());
            xmlData = xmlData.Substring(xmlData.IndexOf('<'));
            xmlData = xmlData.Substring(0, xmlData.LastIndexOf('>') + 1);

            return xmlData;
        }
    }
}
