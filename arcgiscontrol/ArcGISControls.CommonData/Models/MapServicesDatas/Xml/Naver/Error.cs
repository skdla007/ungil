using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Naver
{
    [XmlRoot("error", Namespace = "naver:openapi", IsNullable = true)]
    public class Error
    {
        [XmlElement("error_code")]
        public string ErrorCode { get; set; }
        [XmlElement("message")]
        public string Message { get; set; }
    }
}
