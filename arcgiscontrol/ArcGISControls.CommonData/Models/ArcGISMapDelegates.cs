using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Media;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    public class ChangeLinkedMapEventArgs : EventArgs
    {
        public string MapId { get; set; }
    }

    public class MapExtentChangeEventArgs : EventArgs
    {
        public Rect Extent { get; set; }
    }

    public class LicenseKeyNotAllowedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Map Loading ���н� ȣ��
    /// </summary>
    public class MapLoadingErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    /// <summary>
    /// Map Level�� ����ɶ� ���� �뺸 
    /// </summary>
    /// <param name="level"></param>
    public class LevelChangedEventArgs : EventArgs
    {
        public double Level { get; set; }
    }

    public class SelectdObjectItem
    {
        public string ObjectId { get; set; }
        public MapObjectType Type { get; set;  }

        public SelectdObjectItem(string objectId, MapObjectType type)
        {
            this.ObjectId = objectId;
            this.Type = type;
        }
    }

    /// <summary>
    /// Object Delete/ Select ���� �� Event
    /// </summary>
    public class ObjectEventArgs : EventArgs
    {
        public string Id { get; set; }
        public MapObjectType Type { get; set; }

        public ObjectEventArgs(string id, MapObjectType type)
        {
            this.Id = id;
            this.Type = type;
        }
    }

    public class SelectedObjectEventArgs : EventArgs
    {
        public IEnumerable<SelectdObjectItem> SelectedGraphicList { get; set; }

        public SelectedObjectEventArgs(IEnumerable<SelectdObjectItem> selectedGraphics)
        {
            this.SelectedGraphicList = selectedGraphics;
        }
    }

    public class ObjectDataEventArgs : EventArgs
    {
        public BaseMapObjectInfoData mapObjectDataInfo { get; set; }

        public ObjectDataEventArgs(BaseMapObjectInfoData mapObjectDataInfo)
        {
            this.mapObjectDataInfo = mapObjectDataInfo;
        }
    }

    /// <summary>
    /// Map Searched Addres Popup �����쿡�� Save ��ư Ŭ���� Searched List�� �뺸
    /// </summary>
    /// <param name="unigridGuid"></param>
    /// <param name="mapObjectType"></param>
    /// <returns></returns>
    public delegate void ClickedSaveSearchedAddress(string unigridGuid, MapObjectType mapObjectType);
    
    
    /// <summary>
    /// 
    /// </summary>
    public class GetMapDataProcessCompletedEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public bool IsCompleted { get; private set; }

        public GetMapDataProcessCompletedEventArgs(string data, bool isCompleted)
        {
            this.Data = data;
            this.IsCompleted = isCompleted;
        }
    }
}
