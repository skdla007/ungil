
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Daum
{
    [XmlRoot("apierror")]
    public class ApiError
    {
        [XmlElement("code")]
        public int Code { get; set; }
        [XmlElement("dcode")]
        public int DetailCode { get; set; }
        [XmlElement("message")]
        public string Message { get; set; }
        [XmlElement("dmessage")]
        public string DetailMessage { get; set; }
    }
}
