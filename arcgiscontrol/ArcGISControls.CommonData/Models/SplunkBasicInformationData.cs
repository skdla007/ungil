using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("SplunkBasicInformationData")]
    public class SplunkBasicInformationData : BaseModel
    {
        #region Field

        #region 기본 정보

        [XmlElement("App")]
        public string App { get; set; }

        [XmlElement("IP")]
        public string IP { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlElement("UserId")]
        public string UserId { get; set; }

        [XmlElement("Port")]
        public int Port { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("SplunkInfoDataSearchType")]
        public string SplunkInfoDataSearchType { get; set; }

        public string DataExpressType { get; set; }

        public static bool IsTableDataType(string dataExpressType)
        {
            if (dataExpressType == null) return false;
            return (dataExpressType.ToLower() == "table");
        }

        #endregion 기본 정보

        #region Chart

        public string SplunkInfoDataChartSubType { get; set; }

        public string SplunkInfoDataChartTheme { get; set; }

        public string SplunkInfoDataChartPalette { get; set; }

        public string BackgroundColor { get; set; }

        public string BorderColor { get; set; }

        public string FontColor { get; set; }

        public string ReportTitle { get; set; }

        public string Legend { get; set; }

        public string LineMarker { get; set; }

        public string XAxisTitle { get; set; }

        public string YAxisTitle { get; set; }

        public int YAxisRangeMinimum { get; set; }

        public int YAxisRangeMaximum { get; set; }

        public double LegendWidth { get; set; }

        public double LegendFontSize { get; set; }

        #endregion Chart

        #region Table

        public string HeaderBackgroundColor { get; set; }

        public string HeaderFontColor { get; set; }

        public string RowBackgroundColor { get; set; }

        public string AlternatingRowColor { get; set; }

        public string AlternatingRowFontColor { get; set; }

        public DataGridGridLinesVisibility GridLineVisible { get; set; }

        public string HorizontalGridLineColor { get; set; }

        public string VerticalGridLineColor { get; set; }

        #endregion //Table

        #region DB에 저장될 값

        /// <summary>
        /// DB에 저장되어 있는 SplunkInfoData
        /// </summary>
        private string splunkDataInformationID;

        [XmlAttribute("SplunkDataInformationID")]
        public string SplunkDataInformationID
        {
            get { return this.splunkDataInformationID; }
            set
            {
                this.splunkDataInformationID = value;
                OnPropertyChanged("SplunkDataInformationID");
            }
        }

        /// <summary>
        /// Argument List 저장
        /// </summary>
        private ObservableCollection<string> splArgumentKeys;
        [XmlArray("SplArgumentKeys")]
        [XmlArrayItem("SplArgumentKey", typeof(string))]
        public ObservableCollection<string> SplArgumentKeys
        {
            get { return this.splArgumentKeys; }
            set
            {
                this.splArgumentKeys = value;
                OnPropertyChanged("SplArgumentKeys");
            }
        }

        /// <summary>
        /// Value List 저장
        /// </summary>
        private ObservableCollection<string> splArgumentValues;
        [XmlArray("SplArgumentValues")]
        [XmlArrayItem("SplArgumentValue", typeof(string))]
        public ObservableCollection<string> SplArgumentValues
        {
            get { return this.splArgumentValues; }
            set
            {
                this.splArgumentValues = value;
                OnPropertyChanged("SplArgumentValues");
            }
        }

        #endregion DB에 저장될 값
        
        #endregion

        #region Construction 

        public SplunkBasicInformationData()
        {
            this.App = "search";
            this.IP = "localhost";
            this.Password = "changeme";
            this.UserId = "admin";
            this.Port = 8089;
            this.SplArgumentKeys = new ObservableCollection<string>();
            this.SplArgumentValues = new ObservableCollection<string>();
            this.LegendWidth = double.NaN;
            this.LegendFontSize = double.NaN;
            this.Name = null;
            this.SplunkInfoDataSearchType = "SavedSearch";
            this.DataExpressType = "Table";
            this.SplunkInfoDataChartSubType = "Table";
            this.SplunkInfoDataChartTheme = "None";
            this.SplunkInfoDataChartPalette = "Standard";
            this.BackgroundColor = "#0000000";
            this.BorderColor = "#FF919191";
            this.FontColor = "#FF919191";
            this.ReportTitle = null;
            this.Legend = "right";
            this.LineMarker = null;
            this.XAxisTitle = null;
            this.YAxisTitle = null;
            this.YAxisRangeMaximum = 0;
            this.YAxisRangeMinimum = 0;
            this.SplunkDataInformationID = null;
            this.HeaderBackgroundColor = "#FF000000";
            this.HeaderFontColor = "#FFFFFFFF";
            this.RowBackgroundColor = "#FF101010";
            this.AlternatingRowColor = "#FF161616";
            this.AlternatingRowFontColor = "#FFFFFFFF";
            this.GridLineVisible = DataGridGridLinesVisibility.All;
            this.HorizontalGridLineColor = "#FF222222";
            this.VerticalGridLineColor = "#FF222222";
        }

        public SplunkBasicInformationData(SplunkBasicInformationData data)
        {
            this.App = data.App;
            this.IP = data.IP;
            this.Password = data.Password;
            this.UserId = data.UserId;
            this.Port = data.Port;
            this.SplArgumentKeys = data.SplArgumentKeys;
            this.SplArgumentValues = data.SplArgumentValues;
            this.Name = data.Name;
            this.SplunkInfoDataSearchType = data.SplunkInfoDataSearchType;
            this.DataExpressType = data.DataExpressType;
            this.SplunkInfoDataChartSubType = data.SplunkInfoDataChartSubType;
            this.SplunkInfoDataChartTheme = data.SplunkInfoDataChartTheme;
            this.SplunkInfoDataChartPalette = data.SplunkInfoDataChartPalette;
            this.BackgroundColor = data.BackgroundColor;
            this.BorderColor = data.BorderColor;
            this.FontColor = data.FontColor;
            this.ReportTitle = data.ReportTitle;
            this.Legend = data.Legend;
            this.LineMarker = data.LineMarker;
            this.XAxisTitle = data.XAxisTitle;
            this.YAxisTitle = data.YAxisTitle;
            this.YAxisRangeMaximum = data.YAxisRangeMaximum;
            this.YAxisRangeMinimum = data.YAxisRangeMinimum;
            this.LegendWidth = data.LegendWidth;
            this.LegendFontSize = data.LegendFontSize;
            this.SplunkDataInformationID = data.SplunkDataInformationID;
            this.HeaderBackgroundColor = data.HeaderBackgroundColor;
            this.HeaderFontColor = data.HeaderFontColor;
            this.RowBackgroundColor = data.RowBackgroundColor;
            this.AlternatingRowColor = data.AlternatingRowColor;
            this.AlternatingRowFontColor = data.AlternatingRowFontColor;
            this.GridLineVisible = data.GridLineVisible;
            this.HorizontalGridLineColor = data.HorizontalGridLineColor;
            this.VerticalGridLineColor = data.VerticalGridLineColor;
        }

        #endregion Construction 

        #region Methods

        public void SetSplArgumentsKeys(List<string> keys )
        {
            this.splArgumentKeys.Clear();
            this.splArgumentValues.Clear();

            if(keys == null) return;

            foreach (var key in keys)
            {
                this.splArgumentKeys.Add(key);
                this.splArgumentValues.Add("");
            }
        }

        public void SetSplArgumentsKeyAndValues(List<string> keys, List<string> values)
        {
            this.splArgumentKeys.Clear();
            this.splArgumentValues.Clear();

            if(keys == null || values == null) return;

            for (int i = 0; i < keys.Count; i++ )
            {
                this.splArgumentKeys.Add(keys[i]);
                this.splArgumentValues.Add(values[i]);
            }
        }

        public bool IsSameSplunkService(SplunkBasicInformationData newSplunkBasicInformation)
        {
            return (this.IP == newSplunkBasicInformation.IP &&
                    this.App == newSplunkBasicInformation.App &&
                    this.Name == newSplunkBasicInformation.Name &&
                    this.Password == newSplunkBasicInformation.Password &&
                    this.Port == newSplunkBasicInformation.Port &&
                    this.UserId == newSplunkBasicInformation.UserId);
        }

        public static bool IsUsableServiceInfo(SplunkBasicInformationData splunkBasicInformation)
        {
            return !(string.IsNullOrEmpty(splunkBasicInformation.IP) ||
                    string.IsNullOrEmpty(splunkBasicInformation.App) ||
                    string.IsNullOrEmpty(splunkBasicInformation.Name) ||
                    string.IsNullOrEmpty(splunkBasicInformation.Password) ||
                    splunkBasicInformation.Port == 0  ||
                    string.IsNullOrEmpty(splunkBasicInformation.UserId));
        }

        #endregion //Methods
    }
}
