using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapUniversalObjectDataInfo")]
    public class MapUniversalObjectDataInfo : BaseMapObjectInfoData
    {
        private string splunkObjectID;

        [XmlElement("SplunkObjectID")]
        public string SplunkObjectID
        {
            get { return this.splunkObjectID; }
            set
            {
                if (this.splunkObjectID == value)
                    return;

                this.splunkObjectID = value;
                this.OnPropertyChanged("SplunkObjectID");
            }
        }

        public enum ShapeTypes
        {
            Rectangle,
            VerticalPipe,
            HorizontalPipe,
            Image,
        }

        private ShapeTypes shapeType;
        [XmlElement("ShapeType")]
        public ShapeTypes ShapeType
        {
            get { return this.shapeType; }
            set
            {
                if (this.shapeType == value)
                    return;

                this.shapeType = value;
                this.OnPropertyChanged("ShapeType");
            }
        }

        private string title;

        [XmlElement("Title")]
        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title == value)
                    return;

                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        private Point titleMinPosition = new Point(20, 20);

        [XmlElement("TitleMinPosition")]
        public Point TitleMinPosition
        {
            get { return this.titleMinPosition; }
            set
            {
                if (this.titleMinPosition == value)
                    return;

                this.titleMinPosition = value;
                this.OnPropertyChanged("TitleMinPosition");
            }
        }

        private Point titleMaxPosition = new Point(80, 80);

        [XmlElement("TitleMaxPosition")]
        public Point TitleMaxPosition
        {
            get { return this.titleMaxPosition; }
            set
            {
                if (this.titleMaxPosition == value)
                    return;

                this.titleMaxPosition = value;
                this.OnPropertyChanged("TitleMaxPosition");
            }
        }

        private string titleColor = "black";

        [XmlElement("TitleColor")]
        public string TitleColor
        {
            get { return this.titleColor; }
            set
            {
                if (this.titleColor == value)
                    return;

                this.titleColor = value;
                this.OnPropertyChanged("TitleColor");
            }
        }

        private TextAlignment titleAlignment = TextAlignment.Center;

        [XmlElement("TitleAlignment")]
        public TextAlignment TitleAlignment
        {
            get { return this.titleAlignment; }
            set
            {
                if (this.titleAlignment == value)
                    return;

                this.titleAlignment = value;
                this.OnPropertyChanged("TitleAlignment");
            }
        }

        private string iconImageUrl;

        [XmlElement("IconImageUrl")]
        public string IconImageUrl
        {
            get { return this.iconImageUrl; }
            set
            {
                if (this.iconImageUrl == value)
                    return;

                this.iconImageUrl = value;
                this.OnPropertyChanged("IconImageUrl");
            }
        }

        private Point iconMinPosition = new Point(10, 80);

        [XmlElement("IconMinPosition")]
        public Point IconMinPosition
        {
            get { return this.iconMinPosition; }
            set
            {
                if (this.iconMinPosition == value)
                    return;

                this.iconMinPosition = value;
                this.OnPropertyChanged("IconMinPosition");
            }
        }

        private Point iconMaxPosition = new Point(20, 90);

        [XmlElement("IconMaxPosition")]
        public Point IconMaxPosition
        {
            get { return this.iconMaxPosition; }
            set
            {
                if (this.iconMaxPosition == value)
                    return;

                this.iconMaxPosition = value;
                this.OnPropertyChanged("IconMaxPosition");
            }
        }

        private double borderThickness = 1;

        [XmlElement("BorderThickness")]
        public double BorderThickness
        {
            get { return this.borderThickness; }
            set
            {
                if (this.borderThickness == value)
                    return;

                this.borderThickness = value;
                this.OnPropertyChanged("BorderThickness");
            }
        }

        private string borderColor = "black";

        [XmlElement("BorderColor")]
        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                if (this.borderColor == value)
                    return;

                this.borderColor = value;
                this.OnPropertyChanged("BorderColor");
            }
        }

        private double borderRadius = 10;

        [XmlElement("BorderRadius")]
        public double BorderRadius
        {
            get { return this.borderRadius; }
            set
            {
                if (this.borderRadius == value)
                    return;

                this.borderRadius = value;
                this.OnPropertyChanged("BorderRadius");
            }
        }

        private string linkUrl;

        [XmlElement("LinkUrl")]
        public string LinkUrl
        {
            get { return this.linkUrl; }
            set
            {
                if (this.linkUrl == value)
                    return;

                this.linkUrl = value;
                this.OnPropertyChanged("LinkUrl");
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
                this.OnPropertyChanged("LinkedMapGuid");
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
                this.OnPropertyChanged("LinkedMapBookmarkName");
            }
        }

        private string linkedMapObjectName;

        [XmlElement("LinkedMapObjectName")]
        public string LinkedMapObjectName
        {
            get { return this.linkedMapObjectName; }
            set
            {
                if (this.linkedMapObjectName == value)
                    return;

                this.linkedMapObjectName = value;
                this.OnPropertyChanged("LinkedMapObjectName");
            }
        }

        private List<Point> pointCollection = new List<Point>();

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
                    var minx = double.MaxValue;
                    var miny = double.MaxValue;
                    var maxx = double.MinValue;
                    var maxy = double.MinValue;

                    foreach (var point in this.pointCollection)
                    {
                        if (point.X < minx) minx = point.X;
                        if (point.Y < miny) miny = point.Y;
                        if (point.X > maxx) maxx = point.X;
                        if (point.Y > maxy) maxy = point.Y;
                    }

                    this.ExtentMin = new Point(minx, miny);
                    this.ExtentMax = new Point(maxx, maxy);
                }
            }
        }

        private string fillColor = "#00000000";

        [XmlElement("FillColor")]
        public string FillColor
        {
            get { return this.fillColor; }
            set
            {
                if (this.fillColor == value)
                    return;

                this.fillColor = value;
                this.OnPropertyChanged("FillColor");
            }
        }

        private string fillImageUrl;

        [XmlElement("FillImageUrl")]
        public string FillImageUrl
        {
            get { return this.fillImageUrl; }
            set
            {
                if (this.fillImageUrl == value)
                    return;

                this.fillImageUrl = value;
                this.OnPropertyChanged("FillImageUrl");
            }
        }

        private bool showAlarmLamp = true;

        [XmlElement("ShowAlarmLamp")]
        public bool ShowAlarmLamp
        {
            get { return this.showAlarmLamp; }
            set
            {
                if (this.showAlarmLamp == value)
                    return;

                this.showAlarmLamp = value;
                this.OnPropertyChanged("ShowAlarmLamp");
            }
        }

        private Point alarmLampPosition = new Point(90, 90);

        [XmlElement("AlarmLampPosition")]
        public Point AlarmLampPosition
        {
            get { return this.alarmLampPosition; }
            set
            {
                if (this.alarmLampPosition == value)
                    return;

                this.alarmLampPosition = value;
                this.OnPropertyChanged("AlarmLampPosition");
            }
        }

        private double alarmLampSize = 6;

        [XmlElement("AlarmLampSize")]
        public double AlarmLampSize
        {
            get { return this.alarmLampSize; }
            set
            {
                if (this.alarmLampSize == value)
                    return;

                this.alarmLampSize = value;
                this.OnPropertyChanged("AlarmLampSize");
            }
        }

        private string alarmLampColor = "green";

        [XmlElement("AlarmLampColor")]
        public string AlarmLampColor
        {
            get { return this.alarmLampColor; }
            set
            {
                if (this.alarmLampColor == value)
                    return;

                this.alarmLampColor = value;
                this.OnPropertyChanged("AlarmLampColor");
            }
        }

        private Size controlSize = Size.Empty;

        [XmlElement("ControlSize")]
        public Size ControlSize
        {
            get { return this.controlSize; }
            set
            {
                if(this.controlSize == value)
                    return;

                this.controlSize = value;
                OnPropertyChanged("ControlSize");
            }
        }

        public MapUniversalObjectDataInfo()
            : base()
        {
            this.ObjectType = MapObjectType.Universal;
        }

        public MapUniversalObjectDataInfo(MapUniversalObjectDataInfo other)
            : base(other)
        {
            if (other == null)
                return;

            this.SplunkObjectID = other.SplunkObjectID;
            this.ShapeType = other.ShapeType;
            this.Title = other.Title;
            this.TitleMinPosition = other.TitleMinPosition;
            this.TitleMaxPosition = other.TitleMaxPosition;
            this.TitleColor = other.TitleColor;
            this.TitleAlignment = other.titleAlignment;
            this.IconImageUrl = other.IconImageUrl;
            this.IconMinPosition = other.IconMinPosition;
            this.IconMaxPosition = other.IconMaxPosition;
            this.BorderThickness = other.BorderThickness;
            this.BorderColor = other.BorderColor;
            this.BorderRadius = other.BorderRadius;
            this.LinkUrl = other.LinkUrl;
            this.LinkedMapGuid = other.LinkedMapGuid;
            this.LinkedMapBookmarkName = other.LinkedMapBookmarkName;
            this.LinkedMapObjectName = other.LinkedMapObjectName;
            this.PointCollection = new List<Point>(other.PointCollection);
            this.FillColor = other.FillColor;
            this.FillImageUrl = other.FillImageUrl;
            this.ShowAlarmLamp = other.ShowAlarmLamp;
            this.AlarmLampPosition = other.AlarmLampPosition;
            this.AlarmLampSize = other.AlarmLampSize;
            this.AlarmLampColor = other.AlarmLampColor;
            this.ControlSize = other.ControlSize;
        }

        public override void Init(MapObjectType SelectedMapObjectType, string pName, Point MouseClickByMapPoint, List<Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            Name = pName;
            PointCollection = pPointCollection;
            ControlSize = ArcGISConstSet.ObjectBasicSize;
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
}
