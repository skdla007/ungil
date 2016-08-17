using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Naver
{
    [XmlRoot("geocode", Namespace="naver:openapi", IsNullable = true)]
    public class GeoCode
    {
        [XmlElement(ElementName = "userquery")]
        public string Userquery { get; set; }
        [XmlElement(ElementName = "total")]
        public string Total { get; set; }
    }
}
