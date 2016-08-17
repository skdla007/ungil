using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Utils
{
    public class XmlUtil
    {
        public static string RequestString(string url)
        {
            try
            {
                var xmldoc = new XmlDocument();
                xmldoc.Load(url);
                var xmlString = xmldoc.InnerXml;
                return xmlString;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static T Deserialize<T>(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString)) throw new NullReferenceException("no xml data");
            try
            {
                using (StringReader reader = new StringReader(xmlString))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
