using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    using System.Windows;

    [XmlRoot("MapBookMarkDataInfo")]
    public class MapBookMarkDataInfo : BaseMapObjectInfoData
    {
        #region Properties

        [XmlElement]
        public bool IsHome { get; set; }
        
        [XmlIgnore]
        public Rect ExtentRegion
        {
            get { return new Rect(this.ExtentMin, this.ExtentMax); } 
            set
            {
                this.ExtentMin = value.TopLeft;
                this.ExtentMax = value.BottomRight;
            }
        }

        #endregion

        #region Construction

        public MapBookMarkDataInfo() : base()
        {
            this.ObjectType = MapObjectType.BookMark;
        }

        public MapBookMarkDataInfo(object data) : base(data)
        {
            this.ObjectType = MapObjectType.BookMark;
        }

        #endregion 
        
        #region Method

        #endregion 
    }
}
