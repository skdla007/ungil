using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    [XmlRoot("MapImageObjectDataInfo")]
    public class MapImageObjectDataInfo : BaseMapObjectInfoData
    {
        #region field

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

        #endregion //fields

        #region Constructions

        public MapImageObjectDataInfo()
        {
            this.ImageObjectData = new CommonImageObjectDataInfo();
        }

        public MapImageObjectDataInfo(MapImageObjectDataInfo dataInfo)
             :base(dataInfo)
        {
            this.ImageObjectData = new CommonImageObjectDataInfo(dataInfo.ImageObjectData);
        }

        #endregion //Constructions

        #region Methods
        public override void Init(ArcGISControls.CommonData.Types.MapObjectType SelectedMapObjectType, string pName, System.Windows.Point MouseClickByMapPoint, System.Collections.Generic.List<System.Windows.Point> pPointCollection)
        {
            ObjectID = Guid.NewGuid().ToString();
            ObjectType = SelectedMapObjectType;
            Name = pName;
            PointCollection = pPointCollection;
            
            ImageObjectData.ImageOpacity = 1;
        }
        #endregion //Methods
    }
}
