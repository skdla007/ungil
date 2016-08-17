using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.CommonData.Models
{
    /// <summary>
    /// Map에 표현되는 모든 오브젝트 저장구조의 기본
    /// 
    /// Camera모음
    /// Location
    /// ScreenShot
    /// LinkZone
    /// Address
    /// 로 구분
    /// </summary>
    public class BaseMapObjectInfoData : BaseModel, ICloneable
    {
        [XmlIgnore]
        public string InitName
        {
            get;
            set;
        }

        private string objectId;
        public string ObjectID
        {
            get { return objectId; }
            set
            {
                objectId = value;
                OnPropertyChanged("ID");
            }
        }

        [XmlElement("Name")]
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlElement("ObjectType")]
        private MapObjectType objectType;
        public MapObjectType ObjectType
        {
            get { return this.objectType; }
            set
            {
                this.objectType = value;

                OnPropertyChanged("ObjectType");

                this.objectTypeString = this.objectType.ToString();
                this.ObjectTypeOrder = this.objectType.GetHashCode();
            }
        }

        /// <summary>
        /// TODO: LAT LNG 형태로 어찌 저장할지.... 구조 잡아야 함 
        /// </summary>
        [XmlElement("ExtentMin")]
        private Point extentMin;
        public Point ExtentMin
        {
            get { return this.extentMin; }
            set
            {
                this.extentMin = value;
                OnPropertyChanged("ExtentMin");
            }
        }

        /// <summary>
        /// TODO: LAT LNG 형태로 어찌 저장할지.... 구조 잡아야 함 
        /// </summary>
        [XmlElement("ExtentMax")]
        private Point extentMax;
        public Point ExtentMax
        {
            get { return this.extentMax; }
            set
            {
                this.extentMax = value;
                OnPropertyChanged("ExtentMax");
            }
        }

        [XmlElement("ObjectTypeString")]
        private string objectTypeString;
        public string ObjectTypeString
        {
            get
            {
                return objectTypeString;
            }
            set
            {
                objectTypeString = value;
                OnPropertyChanged("ObjectTypeString");
            }
        }

        private int objectTypeOrder;
        public int ObjectTypeOrder
        {
            get { return this.objectTypeOrder; }
            set
            {
                this.objectTypeOrder = value;
                OnPropertyChanged("ObjectTypeOrder");
            }

        }

        private int objectZIndex = ArcGISConstSet.UndefinedZIndex;
        [XmlElement("ObjectZIndex")]
        public int ObjectZIndex
        {
            get { return this.objectZIndex; }
            set
            {
                this.objectZIndex = value;
                OnPropertyChanged("ObjectZIndex");
            }
        }

        [XmlIgnore]
        public bool IsUndoManage
        {
            get;
            set;
        }

        public BaseMapObjectInfoData()
        {

        }

        public BaseMapObjectInfoData(object data)
        {
            var mapData = data as BaseMapObjectInfoData;

            if (mapData == null)
                return;

            this.ObjectID = mapData.ObjectID;
            this.Name = mapData.Name;
            this.ObjectType = mapData.ObjectType;
            this.ExtentMin = new Point(mapData.ExtentMin.X, mapData.ExtentMin.Y);
            this.ExtentMax = new Point(mapData.ExtentMax.X, mapData.ExtentMax.Y);
            this.ObjectZIndex = mapData.ObjectZIndex;
            this.IsUndoManage = mapData.IsUndoManage;
        }

        public virtual void Init(MapObjectType SelectedMapObjectType, string pName, Point MouseClickByMapPoint, List<Point> pPointCollection)
        {
            throw new Exception("상속받아 재 구현하십시요.");
        }

        public virtual string SaveDataToXML()
        {
            var serializer = new XmlSerializer(this.GetType());

            var settings 
                = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = new string(' ', 4),
                        NewLineOnAttributes = false,
                        Encoding = Encoding.UTF8
                    };
            try
            {
                string xmlData = null;
                using (MemoryStream memStream = new MemoryStream())
                {
                    var xmlWriter = XmlWriter.Create(memStream, settings);

                    serializer.Serialize(xmlWriter, this);
                    xmlWriter.Close();
                    //memStream.Close();

                    xmlData = Encoding.UTF8.GetString(memStream.GetBuffer());
                }
                xmlData = xmlData.Substring(xmlData.IndexOf('<'));
                xmlData = xmlData.Substring(0, xmlData.LastIndexOf('>') + 1);

                return xmlData;
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }

            return null;
        }

        public static T ReadDataFromXML<T>(string xmlData)
            where T : BaseMapObjectInfoData
        {
            if (string.IsNullOrWhiteSpace(xmlData))
                return null;

            var serializer = new XmlSerializer(typeof(T));
            var xmlReader = new XmlTextReader(new StringReader(xmlData));

            try
            {
                var data = serializer.Deserialize(xmlReader) as T;
                return data;
            }
            catch (InvalidOperationException ex)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(ex, "InvalidOperationException");
                return null;
            }
            finally
            {
                xmlReader.Close();
            }
        }

        public static BaseMapObjectInfoData Deserialize(MapObjectType type, string xmlData)
        {
            switch (type)
            {
                case MapObjectType.Camera:
                    return ReadDataFromXML<MapCameraObjectComponentDataInfo>(xmlData);
                case MapObjectType.BookMark:
                    return ReadDataFromXML<MapBookMarkDataInfo>(xmlData);
                case MapObjectType.Location:
                    return ReadDataFromXML<MapLocationObjectDataInfo>(xmlData);
                case MapObjectType.LinkZone:
                case MapObjectType.ImageLinkZone:
                    return ReadDataFromXML<MapLinkZoneObjectDataInfo>(xmlData);
                case MapObjectType.Address:
                    return ReadDataFromXML<MapAddressObjectDataInfo>(xmlData);
                case MapObjectType.Splunk:
                    return ReadDataFromXML<MapSplunkObjectDataInfo>(xmlData);
                case MapObjectType.Workstation:
                    return ReadDataFromXML<MapWorkStationObjectDataInfo>(xmlData);
                case MapObjectType.Text:
                    return ReadDataFromXML<MapTextObjectDataInfo>(xmlData);
                case MapObjectType.Line:
                    return ReadDataFromXML<MapLineObjectDataInfo>(xmlData);
                case MapObjectType.Image:
                    return ReadDataFromXML<MapImageObjectDataInfo>(xmlData);
                case MapObjectType.Universal:
                    return ReadDataFromXML<MapUniversalObjectDataInfo>(xmlData);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Desirialize 할때 ObjectType을 지정 하지 않을 경우 사용 하는 함수
        /// 
        /// Private 는 저장 할때 FeatureType을 DataTable에저장 하지 않고 
        /// data자체만 serialize한다.
        /// 그래서 featureType으로는 type을 받아 올수 없으므로 
        /// 한번 Serialize 된 Data로 받아와야한다.
        /// TODO :  저장할때 public과 private를 같은 저장 구조를 만들어야 함
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static BaseMapObjectInfoData Deserialize(string xmlData)
        {
            var objectType = MapObjectType.None;

            try
            {
                using (var reader = XmlReader.Create(new StringReader(xmlData)))
                {
                    if (!reader.ReadToFollowing("ObjectType"))
                    {
                        InnowatchDebug.Logger.WriteErrorLogAndTrace("InvalidOperationException");
                        return null;
                    }

                    var objectTypeString = reader.ReadElementContentAsString();

                    Enum.TryParse(objectTypeString, out objectType);
                }
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }
            return Deserialize(objectType,xmlData);
        }

        public virtual object Clone()
        {
            return this.CloneObject(this);
        }

        private object CloneObject(object objSource)
        {
            //step : 1 Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);

            //Step2 : Get all the properties of source object type
            System.Reflection.PropertyInfo[] propertyInfo = typeSource.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Step : 3 Assign all source property to taget object 's properties
            foreach (System.Reflection.PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    property.SetValue(objTarget, property.GetValue(objSource, null), null);

                    //Step : 4 check whether property type is value type, enum or string type
                    //if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    //{
                    //    property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    //}
                    ////else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    //else
                    //{
                    //    object objPropertyValue = property.GetValue(objSource, null);
                    //    if (objPropertyValue == null)
                    //    {
                    //        property.SetValue(objTarget, null, null);
                    //    }
                    //    else
                    //    {
                    //        property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                    //    }
                    //}
                }
            }
            return objTarget;
        }
    }
}
