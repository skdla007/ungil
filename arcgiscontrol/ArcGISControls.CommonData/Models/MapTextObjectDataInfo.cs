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
    [XmlRoot("MapTextObjectDataInfo")]
    public class MapTextObjectDataInfo : BaseMapTextObjectInfo
    {
        #region fields

        private List<Point> pointCollection;

        [XmlArray("PointCollection")]
        [XmlArrayItem("Point", typeof (Point))]
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

        /// <summary>
        /// 이전 데이터 사용을 위해 
        /// </summary>
        [XmlElement("FontColorString")]
        public string FontColorString
        {
            get { return this.fontColor; }
            set
            {
                this.fontColor = value;
                OnPropertyChanged("FontColorString");
            }
        }

        /// <summary>
        /// 이전 데이터 사용을 위해 
        /// </summary>
        [XmlElement("BackgroundColorString")]
        public string BackgroundColorString
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                OnPropertyChanged("BackgroundColorString");
            }
        }

        #endregion //fields

        #region Construction

        public MapTextObjectDataInfo() : base()
        {
        }

        public MapTextObjectDataInfo(MapTextObjectDataInfo data) : base(data)
        {
            if (data == null) return;

            this.PointCollection = data.PointCollection;
            this.UseBorder = false;
        }

        #endregion //Construction

        #region Method

        public override void Init(ArcGISControls.CommonData.Types.MapObjectType SelectedMapObjectType, string pName, System.Windows.Point MouseClickByMapPoint, System.Collections.Generic.List<System.Windows.Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            Text = pName;
            Name = pName;
            PointCollection = pPointCollection;
            ObjectType = SelectedMapObjectType;
            IsBold = ArcGISConstSet.TextObjectBold;
            IsItalic = ArcGISConstSet.TextObjectItalic;
            IsUnderline = ArcGISConstSet.TextObjectUnderline;
            FontSize = ArcGISConstSet.TextObjectfontSize;
            FontColor = ArcGISConstSet.TextObjectFontColor.ToString();
            BackgroundColor = ArcGISConstSet.TextObjectBackgroundColor.ToString();
            TextAlignment = ArcGISConstSet.TextObjectAlignment;
            TextFont = MapTextObjectDataInfo.StringFromFontFamily(ArcGISConstSet.TextFontFamily);
            TextBoxSize = ArcGISConstSet.ObjectBasicSize;
            TextVerticalAlignment = ArcGISConstSet.TextObjectVerticalAlignment;
        }

        public static string StringFromFontFamily(FontFamily fontFamily)
        {
            return fontFamily.FamilyNames.ElementAt(0).Value;
        }

    #endregion 
    }
}
