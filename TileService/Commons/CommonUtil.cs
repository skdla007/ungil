using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    using Commons.Debug;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The response type.
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// The success.
        /// </summary>
        Success,

        /// <summary>
        /// The failed.
        /// </summary>
        Failed
    }

    public static class CommonUtil
    {
        public static Stream ResponseData(ResponseType responseType, string message = null, string userid = null, string clientid = null, string ip = null)
        {
            switch (responseType)
            {
                case ResponseType.Success:
                    {
                        return SetResponseData("success", message);
                    }

                case ResponseType.Failed:
                    {
                        return SetResponseData("failed", message);
                    }
            }

            return null;
        }

        private static Stream SetResponseData(string value, string message = null)
        {
            var xmlDocument = new XmlDocument();
            var xmlDec = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.AppendChild(xmlDec);

            var elementResponseData = xmlDocument.CreateElement("ResponseData");
            xmlDocument.AppendChild(elementResponseData);

            var elementResult = xmlDocument.CreateElement("Result");
            elementResponseData.AppendChild(elementResult);

            var attributeValue = xmlDocument.CreateAttribute("value");
            attributeValue.Value = value;
            elementResult.Attributes.Append(attributeValue);

            if (!string.IsNullOrEmpty(message))
            {
                var attributeMessage = xmlDocument.CreateAttribute("message");
                attributeMessage.Value = message;
                elementResult.Attributes.Append(attributeMessage);
                if (value.ToUpper() == "FAILED")
                {
                    Logger.WriteLine(message);
                }
            }

            var bytes = Encoding.UTF8.GetBytes(xmlDocument.InnerXml);

            return new MemoryStream(bytes);
        }
    }
}
