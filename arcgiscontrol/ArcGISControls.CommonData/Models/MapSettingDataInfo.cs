using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// Map 기본 정보
    /// </summary>
    [XmlRoot("MapSettingDataInfo")]
    public class MapSettingDataInfo : BaseModel
    {
        /// <summary>
        /// Map 데이터 형식이 바뀔 때마다, 동작이 바뀔 떄마다 아래 버젼을 올려준다.
        /// </summary>
        //public static readonly int SerializerVersion = 1;
        public static readonly int SerializerVersion = 2; // bgcolor 추가

        [XmlElement("Version")]
        public int Version { get; set; }

        [Serializable]
        public struct Extent
        {
            public double XMin;
            public double XMax;
            public double YMin;
            public double YMax;
        }

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

        [XmlIgnoreAttribute]
        public string MapTypeForMainPage
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
                    case MapProviderType.GoogleSatelliteMap:
                    case MapProviderType.GoogleMap:
                    case MapProviderType.GoogleSatelliteHybridMap:
                        return "Internet Map";
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                    case MapProviderType.ArcGisImageryMap:
                    case MapProviderType.ArcGisStreetMap:
                    case MapProviderType.ArcGisTogoMap:
                    case MapProviderType.ArcGisClientMap:
                        return "GIS Map";
                    default:
                        return "Custom";
                }
            }
        }

        [XmlElement("Name")]
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == null || this.name == value)
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
                    case MapProviderType.NaverSatelliteHybridMap:
                    case MapProviderType.NaverMap:
                    case MapProviderType.NaverSatelliteMap:
                    case MapProviderType.NaverSatelliteTrafficMap:
                    case MapProviderType.NaverTrafficMap:
                    case MapProviderType.GoogleSatelliteMap:
                    case MapProviderType.GoogleMap:
                    case MapProviderType.GoogleSatelliteHybridMap:
                        return MapProviderType.NaverMap;
                    case MapProviderType.ArcGisImageryMap:
                    case MapProviderType.ArcGisStreetMap:
                    case MapProviderType.ArcGisTogoMap:
                    case MapProviderType.ArcGisClientMap:
                        return MapProviderType.ArcGisClientMap;
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                        return MapProviderType.BingMap;
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
                    case MapProviderType.GoogleSatelliteMap:
                    case MapProviderType.GoogleMap:
                    case MapProviderType.GoogleSatelliteHybridMap:
                        return MapProviderType.NaverMap;
                    case MapProviderType.ArcGisImageryMap:
                    case MapProviderType.ArcGisStreetMap:
                    case MapProviderType.ArcGisTogoMap:
                    case MapProviderType.ArcGisClientMap:
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                        return MapProviderType.ArcGisClientMap;
                    default:
                        return MapProviderType.CustomMap;
                }
            }
        }

        private SessionInfo mapSearchSessionInfo = new SessionInfo();

        [XmlElement("MapSearchSessionInfo")]
        public SessionInfo MapSearchSessionInfo
        {
            get { return this.mapSearchSessionInfo; }
            set { this.mapSearchSessionInfo = value; }
        }

        private SessionInfo trendAnalysisSessionInfo = new SessionInfo();

        [XmlElement("TrendAnalysisSessionInfo")]
        public SessionInfo TrendAnalysisSessionInfo
        {
            get { return this.trendAnalysisSessionInfo; }
            set { this.trendAnalysisSessionInfo = value; }
        }

        private string searchViewUrl;

        [XmlElement("SearchViewUrl")]
        public string SearchViewUrl
        {
            get { return this.searchViewUrl; }
            set
            {
                this.searchViewUrl = value;
                this.OnPropertyChanged("SearchViewUrl");
            }
        }

        private string trendAnalysisUrl;
        [XmlElement("TrendAnalysisUrl")]
        public string TrendAnalysisUrl
        {
            get { return this.trendAnalysisUrl; }
            set
            {
                this.trendAnalysisUrl = value;
                this.OnPropertyChanged("TrendAnalysisUrl");
            }
        }

        private Extent? homeExtent;
        [XmlElement("HomeExtent")]
        public Extent? HomeExtent
        {
            get { return this.homeExtent; }
            set
            {
                this.homeExtent = value;
                OnPropertyChanged("HomeExtent");
            }
        }

        private Color mapBgColor;
        [XmlElement("MapBgColor")]
        public Color MapBgColor
        {
            get { return this.mapBgColor; }
            set
            {
                this.mapBgColor = value;
                OnPropertyChanged("MapBgColor");
            }
        }

        #region MapSplunkQuery

        private bool useMapSpl;
        [XmlElement("UseMapSpl")]
        public bool UseMapSpl
        {
            get { return this.useMapSpl; }
            set
            {
                this.useMapSpl = value;
                this.OnPropertyChanged("UseMapSpl");
            }
        }

        private string mapSplHost;
        [XmlElement("MapSplHost")]
        public string MapSplHost
        {
            get { return this.mapSplHost; }
            set
            {
                this.mapSplHost = value;
                this.OnPropertyChanged("MapSplHost");
            }
        }

        private int mapSplPort;
        [XmlElement("MapSplPort")]
        public int MapSplPort
        {
            get { return this.mapSplPort; }
            set
            {
                this.mapSplPort = value;
                this.OnPropertyChanged("MapSplPort");
            }
        }

        private string mapSplApp;
        [XmlElement("MapSplApp")]
        public string MapSplApp
        {
            get { return this.mapSplApp; }
            set
            {
                this.mapSplApp = value;
                this.OnPropertyChanged("MapSplApp");
            }
        }

        private string mapSplId;
        [XmlElement("MapSplId")]
        public string MapSplId
        {
            get { return this.mapSplId; }
            set
            {
                this.mapSplId = value;
                this.OnPropertyChanged("MapSplId");
            }
        }

        private string mapSplPassword;
        [XmlElement("MapSplPassword")]
        public string MapSplPassword
        {
            get { return this.mapSplPassword; }
            set
            {
                this.mapSplPassword = value;
                this.OnPropertyChanged("MapSplPassword");
            }
        }

        private string mapSplQuery;
        [XmlElement("MapSplQuery")]
        public string MapSplQuery
        {
            get { return this.mapSplQuery; }
            set
            {
                this.mapSplQuery = value;
                this.OnPropertyChanged("MapSplQuery");
            }
        }

        private int mapSplEarliestMinutes = 60;
        [XmlElement("MapSplEarliestMinutes")]
        public int MapSplEarliestMinutes
        {
            get { return this.mapSplEarliestMinutes; }
            set
            {
                this.mapSplEarliestMinutes = value;
                this.OnPropertyChanged("MapSplEarliestMinutes");
            }
        }

        private int mapSplIntervalSeconds = 60;
        [XmlElement("MapSplIntervalSeconds")]
        public int MapSplIntervalSeconds
        {
            get { return this.mapSplIntervalSeconds; }
            set
            {
                this.mapSplIntervalSeconds = value;
                this.OnPropertyChanged("MapSplIntervalSeconds");
            }
        }

        private bool mapSplClearPreviousResult = true;
        [XmlElement("MapSplClearPreviousResult")]
        public bool MapSplClearPreviousResult
        {
            get { return this.mapSplClearPreviousResult; }
            set
            {
                this.mapSplClearPreviousResult = value;
                this.OnPropertyChanged("MapSplClearPreviousResult");
            }
        }

        /// <summary>
        /// 맵 버전을 확인하기 위해 사용하며 Serialize되어 저장되어 있던 값은 의미가 없으므로 바로 사용하면 안됨.
        /// 수정된 정확한 날짜는 DB테이블에 들어있고 그 값을 여기 넣어서 사용할 때 의미가 있음.
        /// </summary>
        private int mapDateModified;
        [XmlElement("MapDateModified")]
        public int MapDateModified
        {
            get { return this.mapDateModified; }
            set
            {
                this.mapDateModified = value;
                this.OnPropertyChanged("MapDateModified");
            }
        }

        #endregion //MapSplunkQuery

        public MapSettingDataInfo()
        {
            this.MapType = MapProviderType.CustomMap;

            this.ExtentMin = new Point();
            this.ExtentMax = new Point();

            this.Version = SerializerVersion;

            this.mapBgColor = Colors.White;
        }

        /// <summary>
        /// Copy Data
        /// </summary>
        /// <param name="data"></param>
        public MapSettingDataInfo(object data)
        {
            var mapSettingInfo = data as MapSettingDataInfo;

            if (mapSettingInfo == null)
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

            this.HomeExtent = mapSettingInfo.HomeExtent;

            this.MapSearchSessionInfo.Id = mapSettingInfo.MapSearchSessionInfo.Id;
            this.MapSearchSessionInfo.Password = mapSettingInfo.MapSearchSessionInfo.Password;
            this.MapSearchSessionInfo.Url = mapSettingInfo.MapSearchSessionInfo.Url;
            this.MapSearchSessionInfo.UseSessionInfo = mapSettingInfo.MapSearchSessionInfo.UseSessionInfo;
            
            this.TrendAnalysisSessionInfo.Id = mapSettingInfo.TrendAnalysisSessionInfo.Id;
            this.TrendAnalysisSessionInfo.Password = mapSettingInfo.TrendAnalysisSessionInfo.Password;
            this.TrendAnalysisSessionInfo.Url = mapSettingInfo.TrendAnalysisSessionInfo.Url;
            this.TrendAnalysisSessionInfo.UseSessionInfo = mapSettingInfo.TrendAnalysisSessionInfo.UseSessionInfo;

            this.Version = mapSettingInfo.Version;

            this.MapBgColor = mapSettingInfo.MapBgColor;

            this.UseMapSpl = mapSettingInfo.UseMapSpl;
            this.MapSplHost = mapSettingInfo.MapSplHost;
            this.MapSplPort = mapSettingInfo.MapSplPort;
            this.MapSplApp = mapSettingInfo.MapSplApp;
            this.MapSplId = mapSettingInfo.MapSplId;
            this.MapSplPassword = mapSettingInfo.MapSplPassword;
            this.MapSplIntervalSeconds = mapSettingInfo.MapSplIntervalSeconds;
            this.MapSplQuery = mapSettingInfo.MapSplQuery;
            this.MapSplEarliestMinutes = mapSettingInfo.MapSplEarliestMinutes;
            this.MapSplClearPreviousResult = mapSettingInfo.MapSplClearPreviousResult;

            this.MapDateModified = mapSettingInfo.MapDateModified;
        }

        public static MapSettingDataInfo ReadDataFromXML(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData) || string.IsNullOrWhiteSpace(xmlData))
                return null;

            try
            {
                // 임시로 이전 데이터가 있을 경우
                xmlData = xmlData.Replace("MapSettingInfoData", "MapSettingDataInfo");

                MapSettingDataInfo data = null;
                var serializer = new XmlSerializer(typeof(MapSettingDataInfo));
                using (StringReader stringReader = new StringReader(xmlData))
                {
                    var xmlReader = new XmlTextReader(stringReader);

                    data = serializer.Deserialize(xmlReader) as MapSettingDataInfo;

                    xmlReader.Close();
                    //stringReader.Close();
                }
                return data;
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }

            return null;
        }

        public string SaveDataToXML()
        {
            this.Version = SerializerVersion;

            var serializer = new XmlSerializer(typeof(MapSettingDataInfo));

            var settings
                = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = new string(' ', 4),
                        NewLineOnAttributes = false,
                        Encoding = Encoding.UTF8
                    };

            try
            {
                string xmlData = null;
                using (MemoryStream memStream = new MemoryStream())
                {
                    var xmlWriter = XmlWriter.Create(memStream, settings);
                    serializer.Serialize(xmlWriter, this);
                    xmlWriter.Close();
                    //memStream.Close();

                    xmlData = Encoding.UTF8.GetString(memStream.GetBuffer());
                }
                
                xmlData = xmlData.Substring(xmlData.IndexOf('<'));
                xmlData = xmlData.Substring(0, xmlData.LastIndexOf('>') + 1);

                return xmlData;
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }

            return null;
        }

        public bool IsEditableVersion()
        {
            return this.Version <= SerializerVersion;
        }

        public bool IsValidMapSplData()
        {
            if (!this.useMapSpl)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(this.mapSplHost)
                || (this.mapSplPort < 1 || this.mapSplPort > 65535)
                || string.IsNullOrWhiteSpace(this.mapSplId)
                || string.IsNullOrWhiteSpace(this.mapSplPassword)
                || string.IsNullOrWhiteSpace(this.mapSplApp)
                || string.IsNullOrWhiteSpace(this.mapSplQuery)
                || (this.mapSplIntervalSeconds < 0 || this.mapSplPort > 86400)
                || (this.mapSplEarliestMinutes < 0 || this.mapSplEarliestMinutes > 86400))
            {
                return false;
            }

            return true;
        }
    }
}
