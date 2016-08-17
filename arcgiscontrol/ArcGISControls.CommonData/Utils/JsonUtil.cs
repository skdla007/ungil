using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace ArcGISControls.CommonData.Utils
{
    static class JsonUtil
    {
        public static Stream Request(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Accept = "*/*";
                var response = request.GetResponse() as HttpWebResponse;
                return response.GetResponseStream();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static T Deserialize<T>(Stream stream)
        {
            T returnValue;
            using (stream)
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    returnValue = (T)serializer.ReadObject(stream);
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return returnValue;
        }

        public static T Deserialize<T>(string json)
        {
            T returnValue;
            using (var memoryStream = new MemoryStream())
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                memoryStream.Write(jsonBytes, 0, jsonBytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(memoryStream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    returnValue = (T)serializer.ReadObject(jsonReader);
                }
            }
            return returnValue;
        }
    }
}
