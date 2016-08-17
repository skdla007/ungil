
namespace ArcGISControls.CommonData.Models
{
    using System.Xml.Serialization;

    [XmlRoot("SessionInfo")]
    public class SessionInfo
    {
        [XmlElement]
        public string Url { get; set; }

        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string Password { get; set; }

        [XmlElement]
        public bool UseSessionInfo { get; set; }
    }
}
