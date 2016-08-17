using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models 
{
    [XmlRoot("MapAddressObjectDataInfo")]
    public class MapAddressObjectDataInfo : MapLocationObjectDataInfo
    {
        #region Field

        [XmlElement("SearchText")]
        private string searchText;
        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        [XmlElement("Types")]
        private string types;
        public string Types
        {
            get { return this.types; }
            set
            {
                this.types = value;
                OnPropertyChanged("Types");
            }
        }

        private bool isSaved;
        public bool IsSaved
        {
            get { return this.isSaved; }
            set
            {
                this.isSaved = value;
                OnPropertyChanged("IsSaved");
            }
        }

        public int SearchedIndex
        {
            set
            {
                var c = (Char)(65 + value);
                this.searchedIndexLabel = c.ToString(CultureInfo.InvariantCulture);
            }
        }

        private string searchedIndexLabel;
        public string SearchedIndexLabel
        {
            get { return searchedIndexLabel; }
        }

        [XmlIgnore]
        public Rect ExtentRegion
        {
            get { return new Rect(this.ExtentMin, this.ExtentMax); }
            set
            {
                this.ExtentMin = value.TopLeft;
                this.ExtentMax = value.BottomRight;
            }
        }

        #endregion //Field

        #region Construction 

        public MapAddressObjectDataInfo() : base()
        {
            this.ObjectType = MapObjectType.Address;
            this.IconUri = ArcGISConstSet.LocationIconNormalUri;
            this.IconSelectedStringUri = ArcGISConstSet.LocationIconSelectedUri;
        }

        public MapAddressObjectDataInfo(object data)
            : base(data)
        {
            var mapLocationData = data as MapAddressObjectDataInfo;
            this.ObjectType = MapObjectType.Address;

            if(mapLocationData == null)
                return;

            this.SearchText = mapLocationData.SearchText;
            this.Types = mapLocationData.Types;
        }

        #endregion //Construciton 

        #region Method
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
    }
        #endregion 
}
