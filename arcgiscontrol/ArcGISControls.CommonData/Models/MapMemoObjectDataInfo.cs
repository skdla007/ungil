using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapMemoObjectDataInfo")]
    public class MapMemoObjectDataInfo : BaseMapTextObjectInfo
    {
        private List<Point> boxBoundary;
        [XmlArray("BoxBoundary")]
        [XmlArrayItem("Point", typeof(Point))]
        public List<Point> BoxBoundary
        {
            get { return this.boxBoundary; }
            set
            {
                this.boxBoundary = value;
                this.SetExtentMinMax();
                OnPropertyChanged("BoxBoundary");
            }
        }

        private Point tipPosition;
        [XmlElement("TipPosition")]
        public Point TipPosition
        {
            get { return this.tipPosition; }
            set
            {
                this.tipPosition = value;
                this.SetExtentMinMax();
                OnPropertyChanged("TipPosition");
            }
        }

        private int tipZIndex = ArcGISConstSet.UndefinedZIndex;
        [XmlElement("TipZIndex")]
        public int TipZIndex
        {
            get { return this.tipZIndex; }
            set
            {
                this.tipZIndex = value;
                OnPropertyChanged("TipZIndex");
            }
        }

        public MapMemoObjectDataInfo()
            : base()
        {
            this.BackgroundColor = "#FFFCFBB5";
            this.BorderColor = "#FFC6A565";
        }

        public MapMemoObjectDataInfo(MapMemoObjectDataInfo data)
            : base(data)
        {
            if (data == null) return;

            this.BoxBoundary = data.BoxBoundary;
            this.TipPosition = data.TipPosition;
            this.TipZIndex = data.TipZIndex;
            this.UseBorder = true;
        }

        private void SetExtentMinMax()
        {
            var minX = this.tipPosition.X;
            var maxX = this.tipPosition.X;

            var minY = this.tipPosition.Y;
            var maxY = this.tipPosition.Y;

            foreach (var point in this.boxBoundary)
            {
                if (point.X < minX)
                    minX = point.X;
                else if (point.X > maxX)
                    maxX = point.X;

                if (point.Y < minY)
                    minY = point.Y;
                else if (point.Y > maxY)
                    maxY = point.Y;
            }

            this.ExtentMin = new Point(minX, minY);
            this.ExtentMax = new Point(maxX, maxY);
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
