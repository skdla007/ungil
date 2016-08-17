using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    public enum IntervalUnitType
    {
        None = -1,
        Seconds = 0,
        Minutes = 1,
        Hours = 2,
    }

    [XmlRoot("MapSplunkObjectDataInfo")]
    public class MapSplunkObjectDataInfo : BaseMapObjectInfoData
    {
        #region Field

        #region Splunk 기본 정보

        [XmlElement("SplunkBasicInformation")]
        public SplunkBasicInformationData SplunkBasicInformation { get; set; }

        #endregion //Splunk 기본 정보

        #region DB에 저장될 값들

        private string title;

        [XmlElement("Title")]
        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title == value) return;
                this.title = value;
                if (!String.IsNullOrWhiteSpace(this.title))
                {
                    this.Name = this.title;
                }
                else
                {
                    if (this.SplunkBasicInformation != null)
                    {
                        this.Name = this.SplunkBasicInformation.Name;
                    }
                }
                this.OnPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Icon Position
        /// </summary>
        private Point iconPosition;
        
        [XmlElement("IconPosition")]
        public Point IconPosition
        {
            get { return this.iconPosition; }
            set { this.iconPosition = value; }
        }

        /// <summary>
        /// Map 에 뿌려질 SPlunk Control의 크기 Postion
        /// </summary>
        private List<Point> pointCollection;
        
        [XmlArray("PointCollection")]
        [XmlArrayItem("Poiint", typeof(Point))]
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


        /// <summary>
        /// Chart가 저장될 Size
        /// </summary>
        private Size controlSize = new Size(50, 50);

        [XmlElement("ControlSize")]
        public Size ControlSize
        {
            get { return this.controlSize; }
            set
            {
                this.controlSize = value;
                OnPropertyChanged("ControlSize");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int hiddenMinLevel;

        [XmlElement("HiddenMinLevel")]
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

        /// <summary>
        /// 
        /// </summary>
        private int hiddenMaxLevel;

        [XmlElement("HiddenMaxLevel")]
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

        private string chartAxisXTitle;

        [XmlElement("ChartAxisXTitle")]
        public string ChartAxisXTitle
        {
            get { return this.chartAxisXTitle; }
            set
            {
                if (this.chartAxisXTitle == value) return;
                this.chartAxisXTitle = value;
                OnPropertyChanged("ChartAxisXTitle");
            }
        }

        private string chartAxisYTitle;

        [XmlElement("ChartAxisYTitle")]
        public string ChartAxisYTitle
        {
            get { return this.chartAxisYTitle; }
            set
            {
                if (this.chartAxisYTitle == value) return;
                this.chartAxisYTitle = value;
                OnPropertyChanged("ChartAxisYTitle");
            }
        }

        private int iconZIndex = ArcGISConstSet.UndefinedZIndex;

        [XmlElement("IconZIndex")]
        public int IconZIndex
        {
            get { return this.iconZIndex; }
            set
            {
                this.iconZIndex = value;
                OnPropertyChanged("IconZIndex");
            }
        }

        private bool? useSchedule = false;

        [XmlElement("UseSchedule")]
        public bool? UseSchedule
        {
            get { return this.useSchedule; }
            set
            {
                this.useSchedule = value;
                OnPropertyChanged("UseSchedule");
            }
        }

        private int? intervalSeconds;

        [XmlElement("IntervalSeconds")]
        public int? IntervalSeconds
        {
            get { return this.intervalSeconds; }
            set
            {
                this.intervalSeconds = value;
                OnPropertyChanged("IntervalSeconds");
            }
        }

        private IntervalUnitType intervalUnitType;

        public IntervalUnitType IntervalUnitType
        {
            get { return this.intervalUnitType; }
            set
            {
                this.intervalUnitType = value;
                OnPropertyChanged("IntervalUnitType");
            }
        }

        private bool? isIconHidden = false;

        public bool? IsIconHidden
        {
            get { return this.isIconHidden; }
            set
            {
                this.isIconHidden = value;
                OnPropertyChanged("IsIconHidden");
            }
        }
        private bool showXAxis = true;

        [XmlElement("ShowXAxis")]
        public bool ShowXAxis
        {
            get { return this.showXAxis; }
            set
            {
                if (value.Equals(this.showXAxis)) return;
                this.showXAxis = value;
                this.OnPropertyChanged("ShowXAxis");
            }
        }

        private double yAxisRangeMin = double.NaN;

        [XmlElement("YAxisRangeMin")]
        public double YAxisRangeMin
        {
            get { return this.yAxisRangeMin; }
            set
            {
                if (value.Equals(this.yAxisRangeMin)) return;
                this.yAxisRangeMin = value;
                this.OnPropertyChanged("YAxisRangeMin");
            }
        }

        private double yAxisRangeMax = double.NaN;

        [XmlElement("YAxisRangeMax")]
        public double YAxisRangeMax
        {
            get { return this.yAxisRangeMax; }
            set
            {
                if (value.Equals(this.yAxisRangeMax)) return;
                this.yAxisRangeMax = value;
                this.OnPropertyChanged("YAxisRangeMax");
            }
        }

        private string linkUrl = string.Empty;
        [XmlElement("LinkUrl")]
        public string LinkUrl
        {
            get { return this.linkUrl; }
            set
            {
                if (value.Equals(this.linkUrl)) return;

                this.linkUrl = value;
                this.OnPropertyChanged("LinkUrl");
            }
        }

        private string chartDateTimeFormat;

        [XmlElement("ChartDateTimeFormat")]
        public string ChartDateTimeFormat
        {
            get { return this.chartDateTimeFormat; }
            set
            {
                if (value == this.chartDateTimeFormat) return;
                this.chartDateTimeFormat = value;
                this.OnPropertyChanged("ChartDateTimeFormat");
            }
        }

        private double chartLegendFontSize = double.NaN;

        [XmlElement("ChartLegendFontSize")]
        public double ChartLegendFontSize
        {
            get { return this.chartLegendFontSize; }
            set
            {
                if (value.Equals(this.chartLegendFontSize)) return;
                this.chartLegendFontSize = value;
                this.OnPropertyChanged("ChartLegendFontSize");
            }
        }

        private double chartLegendSize = double.NaN;

        [XmlElement("ChartLegendSize")]
        public double ChartLegendSize
        {
            get { return this.chartLegendSize; }
            set
            {
                if (value.Equals(this.chartLegendSize)) return;
                this.chartLegendSize = value;
                this.OnPropertyChanged("ChartLegendSize");
            }
        }

        #endregion //DB에 저장될 값들
        
        #endregion //Field

        #region Construction

        public MapSplunkObjectDataInfo() : base()
        {
            this.ObjectType = MapObjectType.Splunk;
            this.SplunkBasicInformation = new SplunkBasicInformationData();
            this.PointCollection = new List<Point>();
        }

        public MapSplunkObjectDataInfo(object data)
            : base(data)
        {
            var splunkData = data as MapSplunkObjectDataInfo;

            if(splunkData == null) return;

            if (this.SplunkBasicInformation == null) this.SplunkBasicInformation = new SplunkBasicInformationData(splunkData.SplunkBasicInformation);

            this.Title = splunkData.Title;
            this.ChartAxisXTitle = splunkData.ChartAxisXTitle;
            this.ChartAxisYTitle = splunkData.ChartAxisYTitle;
            this.IconPosition = splunkData.IconPosition;
            this.PointCollection = new List<Point>(splunkData.PointCollection);
            this.HiddenMinLevel = splunkData.HiddenMinLevel;
            this.HiddenMaxLevel = splunkData.HiddenMaxLevel;
            this.ControlSize = splunkData.ControlSize;
            this.IconZIndex = splunkData.IconZIndex;
            this.UseSchedule = splunkData.UseSchedule;
            this.IntervalSeconds = splunkData.IntervalSeconds;
            this.IntervalUnitType = splunkData.IntervalUnitType;
            this.IsIconHidden = splunkData.IsIconHidden;
            this.ShowXAxis = splunkData.ShowXAxis;
            this.YAxisRangeMin = splunkData.YAxisRangeMin;
            this.YAxisRangeMax = splunkData.YAxisRangeMax;
            this.LinkUrl = splunkData.LinkUrl;
            this.ChartDateTimeFormat = splunkData.ChartDateTimeFormat;
            this.ChartLegendFontSize = splunkData.ChartLegendFontSize;
            this.ChartLegendSize = splunkData.ChartLegendSize;
        }

        #endregion //Construction

        #region Method

        internal static MapSplunkObjectDataInfo ReadDataFromXML(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData) || string.IsNullOrWhiteSpace(xmlData))
                return null;

            MapSplunkObjectDataInfo data = null;
            var serializer = new XmlSerializer(typeof (MapSplunkObjectDataInfo));
            using (StringReader stringReader = new StringReader(xmlData))
            {
                var xmlReader = new XmlTextReader(stringReader);

                data = serializer.Deserialize(xmlReader) as MapSplunkObjectDataInfo;

                xmlReader.Close();
                //stringReader.Close();
            }

            return data;
        }

        public void SetSplunkBounds()
        {
            var pointList = new List<Point>();
            pointList.Add(this.IconPosition);

            foreach (var point in this.PointCollection)
            {
                pointList.Add(point);
            }

            if (pointList.Count > 0)
            {
                var minx = (from t in pointList select t).Min(e => e.X);
                var miny = (from t in pointList select t).Min(e => e.Y);
                var maxx = (from t in pointList select t).Max(e => e.X);
                var maxy = (from t in pointList select t).Max(e => e.Y);

                this.ExtentMin = new Point(minx, miny);
                this.ExtentMax = new Point(maxx, maxy);
            }
        }

        public void SetYAxisRange(double min, double max)
        {
            this.yAxisRangeMin = min;
            this.yAxisRangeMax = max;

            this.OnPropertyChanged("YAxisRangeMin");
            this.OnPropertyChanged("YAxisRangeMax");
        }

        public double GetLegendSizeCombinedWithDefaultSetting()
        {
            if (double.IsNaN(this.ChartLegendSize))
            {
                return this.SplunkBasicInformation.LegendWidth;
            }

            return this.ChartLegendSize;
        }

        public double GetLegendFontSizeCombinedWithDefaultSetting()
        {
            if (double.IsNaN(this.ChartLegendFontSize))
            {
                return this.SplunkBasicInformation.LegendFontSize;
            }

            return this.ChartLegendFontSize;
        }

        public override object Clone()
        {
            return this.CloneObject(this);
        }

        private object CloneObject(object objSource)
        {
            //step : 1 Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);

            //Step2 : Get all the properties of source object type
            System.Reflection.PropertyInfo[] propertyInfo = typeSource.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Step : 3 Assign all source property to taget object 's properties
            foreach (System.Reflection.PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    
                    //Step : 4 check whether property type is value type, enum or string type
                    //if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    //{
                    //    property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    //}
                    ////else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    //else
                    //{
                    //    object objPropertyValue = property.GetValue(objSource, null);
                    //    if (objPropertyValue == null)
                    //    {
                    //        property.SetValue(objTarget, null, null);
                    //    }
                    //    else
                    //    {
                    //        property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                    //    }
                    //}
                }
            }
            return objTarget;
        }
    }
        #endregion 
}
