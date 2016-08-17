using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    [XmlRoot("CameraObjectComponentData")]
    public class MapCameraObjectComponentDataInfo : BaseMapObjectInfoData
    {
        #region Field

        [XmlElement("IconData")]
        private MapCameraIconObjectDataInfo cameraIcon;
        public MapCameraIconObjectDataInfo CameraIcon
        {
            get { return this.cameraIcon; }
            set
            {
                this.cameraIcon = value;
                OnPropertyChanged("IconData");
            }
        }

        [XmlElement("VideoData")]
        private MapCameraVideoObjectDataInfo video;
        public MapCameraVideoObjectDataInfo Video
        {
            get { return this.video; }
            set
            {
                this.video = value;
                OnPropertyChanged("VideoData");
            }
        }

        [XmlArray("PresetDatas")]
        [XmlArrayItem("PresetData", typeof(MapCameraPresetObjectDataInfo))]
        private List<MapCameraPresetObjectDataInfo> presetDatas;
        public List<MapCameraPresetObjectDataInfo> PresetDatas
        {
            get { return this.presetDatas; }
            set
            {
                this.presetDatas = value;
                OnPropertyChanged("PresetDatas");
            }
        }

        [XmlElement("CameraInformationID")]
        private string cameraInformationID;
        public string CameraInformationID
        {
            get { return this.cameraInformationID; }
            set
            {
                this.cameraInformationID = value;
                OnPropertyChanged("CameraInformationID");
            }
        }

        public int PresetCount { get; set; }

        public ObservableCollection<string> PresetList
        {
            get
            {
                var presetList = new ObservableCollection<string>();

                if (this.PresetCount > 0)
                {
                    for(int i = 1 ;  i <= this.PresetCount ; i++)
                    {
                        presetList.Add(i.ToString());
                    }
                }

                return presetList;
            }
        }

        #endregion //Field

        #region Construction 

        public MapCameraObjectComponentDataInfo() : base()
        {
            this.ObjectType = MapObjectType.Camera;
            this.CameraIcon = new MapCameraIconObjectDataInfo();
            this.video = new MapCameraVideoObjectDataInfo();

            this.presetDatas = new List<MapCameraPresetObjectDataInfo>();
        }

        public MapCameraObjectComponentDataInfo(object data) :base(data)
        {
            this.ObjectType = MapObjectType.Camera;
            var cameraData = data as MapCameraObjectComponentDataInfo;
            
            if(cameraData == null)
                return;

            this.CameraIcon = new MapCameraIconObjectDataInfo(cameraData.cameraIcon);
            this.video = new MapCameraVideoObjectDataInfo(cameraData.Video);

            this.presetDatas = new List<MapCameraPresetObjectDataInfo>();
            this.CameraInformationID = cameraData.CameraInformationID;

            foreach (var mapCameraPresetObjectDataInfo in cameraData.PresetDatas)
            {
                presetDatas.Add(new MapCameraPresetObjectDataInfo(mapCameraPresetObjectDataInfo));
            }
        }

        #endregion //Construction

        #region Method

        public override string SaveDataToXML()
        {
            this.SetCameraComponentBounds();

            return base.SaveDataToXML();
        }

        public void SetCameraComponentBounds()
        {
            var pointList = new List<Point>();
            pointList.Add(this.CameraIcon.Position);
            
            foreach (var point in this.Video.PointCollection)
            {
                pointList.Add(point);
            }

            foreach (var presetData in this.presetDatas)
            {
                foreach (var point in presetData.PointCollection)
                {
                    pointList.Add(point);
                }
            }

            if (pointList.Count > 0)
            {
                var minx = (from t in pointList select t).Min(e => e.X);
                var miny = (from t in pointList select t).Min(e => e.Y);
                var maxx = (from t in pointList select t).Max(e => e.X);
                var maxy = (from t in pointList select t).Max(e => e.Y);

                this.ExtentMin = new Point(minx, miny);
                this.ExtentMax = new Point(maxx, maxy);
            }
        }

        public override object Clone()
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
                    
                    ////Step : 4 check whether property type is value type, enum or string type
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
        #endregion 
    }
}
