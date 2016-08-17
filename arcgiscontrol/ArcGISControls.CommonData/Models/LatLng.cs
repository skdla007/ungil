using System.Windows;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// Point를 Lat Lng 형태로 표현 하기 위한 Class
    /// </summary>
    public class LatLng
    {
       
        public LatLng(double lat, double lng)
        {
            this.Lat = lat;
            this.Lng = lng;
        }

        public LatLng(Point point)
        {
            this.Lat = point.Y;
            this.Lng = point.X;
        }

        public Point ToPoint()
        {
            return new Point(this.Lat, this.Lng);
        }

        public LatLng()
        {
            
        }

        [XmlAttribute("Lat")]
        public double Lat { get; set; }

        [XmlAttribute("Lng")]
        public double Lng { get; set; }
    }
}
