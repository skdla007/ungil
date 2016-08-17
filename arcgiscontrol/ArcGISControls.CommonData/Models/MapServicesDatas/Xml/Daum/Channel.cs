using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Daum
{
    [XmlRoot("channel")]
    public class Channel
    {
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
    }
}
