using System;
using System.Reflection;
using System.Windows.Input;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.CommonData.Types
{
    public class MapObjectTypeInfoAttribute : Attribute
    {
        #region Member Fields
        private string _MapObjectStrCursor;
        private Type _MapObjectInfoDataType;
        private string _CreateMethodNameByDataInfo;
        #endregion

        #region Constructor
        /// <summary>
        /// Object의 Cursor이름
        /// </summary>
        /// <param name="MapObjectStrCursor">Object의 DataInfo Class Type</param>
        /// <param name="MapObjectInfoDataType"></param>
        /// <param name="CreateMethodNameByDataInfo">Object의 DataInfo 생성 메서드 이름</param>
        public MapObjectTypeInfoAttribute(string MapObjectStrCursor, Type MapObjectInfoDataType, string CreateMethodNameByDataInfo)
        {
            _MapObjectStrCursor = MapObjectStrCursor;
            _MapObjectInfoDataType = MapObjectInfoDataType;
            _CreateMethodNameByDataInfo = CreateMethodNameByDataInfo;
        }
        #endregion

        #region Properties

        public string MapObjectStrCursor
        {
            get
            {
                return _MapObjectStrCursor;
            }
        }

        public Type MapObjectInfoDataType
        {
            get
            {
                return _MapObjectInfoDataType;
            }
        }

        public string CreateMethodNameByDataInfo
        {
            get
            {
                return _CreateMethodNameByDataInfo;
            }
        }
        #endregion
    }

    public static class MapObjectTypeInfoAttributeHelper
    {
        public static MapObjectTypeInfoAttribute GetMapObjectTypeInfoAttribute(this Enum pEnum, int AttIndex)
        {
            Type tType = pEnum.GetType();
            FieldInfo FI = tType.GetField(pEnum.ToString());
            object[] ObjCustomAttributes = FI.GetCustomAttributes(false);
            if (ObjCustomAttributes.Length > 0)
            {
                MapObjectTypeInfoAttribute tObj = ObjCustomAttributes[AttIndex] as MapObjectTypeInfoAttribute;
                return tObj;
            }
            else
            {
                return null;
            }
        }
    }
}
