using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// Map Polygon Data 저장을 위한 Model
    /// </summary>
    public class BaseMapPolygonObjectInfoData : BaseModel
    {
        [XmlElement("Visible")]
        private bool? visible = true;
        public bool? Visible
        {
            get { return this.visible; }
            set
            {
                this.visible = value;
                OnPropertyChanged("Visible");
            }
        }

        [XmlElement("FillColorString")]
        private string fillColorString;
        public string FillColorString
        {
            get { return this.fillColorString; }
            set
            {
                this.fillColorString = value;
                OnPropertyChanged("FillColorString");
            }
        }

        [XmlElement("BorderColorString")]
        private string borderColorString;
        public string BorderColorString
        {
            get { return this.borderColorString; }
            set
            {
                this.borderColorString = value;
                OnPropertyChanged("BorderColorString");
            }
        }

        [XmlElement("FillSelectedColorString")]
        private string fillSelectedColorString;
        public string FillSelectedColorString
        {
            get { return this.fillSelectedColorString; }
            set
            {
                this.fillSelectedColorString = value;
                OnPropertyChanged("FillSelectedColorString");
            }
        }

        [XmlElement("BorderSelectedColorString")]
        private string borderSelectedColorString;
        public string BorderSelectedColorString
        {
            get { return this.borderSelectedColorString; }
            set
            {
                this.borderSelectedColorString = value;
                OnPropertyChanged("BorderSelectedColorString");
            }
        }

        [XmlArray("PointCollection")]
        [XmlArrayItem("Poiint", typeof(Point))]
        private List<Point> pointCollection;
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
        /// TODO: LAT LNG 형태로 어찌 저장할지.... 구조 잡아야 함 
        /// </summary>
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
        /// TODO: LAT LNG 형태로 어찌 저장할지.... 구조 잡아야 함 
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

        public Color FillSelectedColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.fillSelectedColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
            }
        }

        public Color BorderSelectedColor
        {
            get
            {
                var convertFromString = ColorConverter.ConvertFromString(this.borderSelectedColorString);
                if (convertFromString != null)
                    return (Color)convertFromString;
                return new Color();
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

        public BaseMapPolygonObjectInfoData(object data)
        {
            var polygonObj = data as BaseMapPolygonObjectInfoData;
            if (polygonObj == null)
                return;

            this.Visible = polygonObj.Visible;
            this.BorderColorString = polygonObj.BorderColorString;
            this.FillColorString = polygonObj.FillColorString;
            this.BorderSelectedColorString = polygonObj.BorderSelectedColorString;
            this.FillSelectedColorString = polygonObj.FillSelectedColorString;
            this.PointCollection = polygonObj.PointCollection;
            this.ObjectZIndex = polygonObj.ObjectZIndex;
        }

        public BaseMapPolygonObjectInfoData()
        {
            
        }
    }
}
