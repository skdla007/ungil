using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace ArcGISControls.CommonData.Models
{
    using System.Xml.Serialization;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;
    using Types;
    


    [XmlRoot("MapLineObjectDataInfo")]
    public class MapLineObjectDataInfo : BaseMapObjectInfoData
    {
        #region Fields

        public enum LineCapTypes
        {
            Flat,
            Arrow
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
                    var minx = this.pointCollection.Min(e => e.X);
                    var miny = this.pointCollection.Min(e => e.Y);
                    var maxx = this.pointCollection.Max(e => e.X);
                    var maxy = this.pointCollection.Max(e => e.Y);

                    this.ExtentMin = new Point(minx, miny);
                    this.ExtentMax = new Point(maxx, maxy);
                }
            }
        }

        private LineCapTypes startLineCap;

        [XmlElement("StartLineCap")]
        public LineCapTypes StartLineCap
        {
            get { return this.startLineCap; }
            set 
            { 
                this.startLineCap = value;
                OnPropertyChanged("StartLineCap");
            }
        }

        private LineCapTypes endLineCap;

        [XmlElement("EndLineCap")]
        public LineCapTypes EndLineCap
        {
            get { return this.endLineCap; }
            set
            {
                this.endLineCap = value;
                OnPropertyChanged("EndLineCap");
            }
        }

        private string colorString;

        [XmlElement("ColorString")]
        public string ColorString
        {
            get { return this.colorString; }
            set
            {
                this.colorString = value;
                OnPropertyChanged("ColorString");
            }
        }

        private int strokeThickness;

        [XmlElement("StrokeThickness")]
        public int StrokeThickness
        {
            get { return this.strokeThickness; }
            set
            {
                this.strokeThickness = value;
                OnPropertyChanged("StrokeThickness");
            }
        }

        private LineStrokeType? lineStrokeType;

        [XmlElement("LineStrokeType")]
        public LineStrokeType? LineStrokeType
        {
            get { return this.lineStrokeType; }
            set
            {
                this.lineStrokeType = value;
                OnPropertyChanged("LineStrokeType");
            }
        }

        private PenLineJoin? lineJoin;

        [XmlElement("LineJoin")]
        public PenLineJoin? LineJoin
        {
            get { return this.lineJoin; }
            set
            {
                this.lineJoin = value;
                OnPropertyChanged("LineJoin");
            }
        }

        #endregion Fields

        #region Constructions

        public MapLineObjectDataInfo()
            : base()
        {

        }

        public MapLineObjectDataInfo(MapLineObjectDataInfo dataInfo)
            : base(dataInfo)
        {
            this.PointCollection = dataInfo.PointCollection;
            this.ColorString = dataInfo.ColorString;
            this.StrokeThickness = dataInfo.StrokeThickness;
            this.LineStrokeType = dataInfo.LineStrokeType;
            this.LineJoin = dataInfo.LineJoin;
            this.StartLineCap = dataInfo.StartLineCap;
            this.EndLineCap = dataInfo.EndLineCap;
        }

        #endregion Constructions

        #region Methods
        public override void Init(MapObjectType SelectedMapObjectType, string pName, System.Windows.Point MouseClickByMapPoint, System.Collections.Generic.List<System.Windows.Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            Name = pName;
            PointCollection = pPointCollection;
            ObjectType = SelectedMapObjectType;
            ColorString = ArcGISConstSet.LineColor.ToString();
            LineStrokeType = ArcGISConstSet.LineStrokeType;
            StrokeThickness = ArcGISConstSet.LineStrokeThickness;
            LineJoin = ArcGISConstSet.LineJoin;
        }
        #endregion Methods 
    }
}
