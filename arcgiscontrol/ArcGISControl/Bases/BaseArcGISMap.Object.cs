using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.Bases
{
    public partial class BaseArcGISMap : UserControl
    {   
        #region Method

        #region CommonData

        protected BaseGraphic GetOneBaseGraphicInGraphicLayer(string objectID, MapObjectType objectType)
        {
            BaseGraphic baseGraphic = null;

            switch (objectType)
            {   
                case MapObjectType.CameraIcon:
                    baseGraphic = this.objectGraphicLayer.OfType<CameraIconGraphic>().FirstOrDefault(item => item.ObjectID == objectID);
                    break;
                case MapObjectType.CameraVideo:
                    baseGraphic = this.objectGraphicLayer.OfType<CameraVideoGraphic>().FirstOrDefault(item => item.ObjectID == objectID);
                    break;
                case MapObjectType.CameraPreset:
                    baseGraphic = this.objectGraphicLayer.OfType<CameraPresetGraphic>().FirstOrDefault(item => item.ObjectID == objectID);
                    break;

                case MapObjectType.SearchedAddress:
                    baseGraphic = this.objectGraphicLayer.OfType<SearchedAddressIconGraphic>().FirstOrDefault(item => item.ObjectID == objectID);
                    break;

                case MapObjectType.VertexSeletedMarker:
                    baseGraphic = this.objectGraphicLayer.OfType<VertexIconGraphic>().FirstOrDefault(item => item.ObjectID == objectID);
                    break;

                case MapObjectType.Workstation:
                case MapObjectType.LinkZone:
                    baseGraphic = this.objectGraphicLayer.OfType<LinkZoneGraphic>().FirstOrDefault(item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                
                case MapObjectType.CameraPresetPlus:
                case MapObjectType.Address:
                case MapObjectType.Location:
                    baseGraphic = this.objectGraphicLayer.OfType<IconGraphic>().FirstOrDefault(item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.SplunkControl:
                    baseGraphic = this.objectGraphicLayer.OfType<PolygonControlGraphic<SplunkChartTableWrapperControl>>().FirstOrDefault(item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.Splunk:
                case MapObjectType.SplunkIcon:
                    baseGraphic = this.objectGraphicLayer.OfType<SplunkIconGraphic>().FirstOrDefault(item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.Text:
                    baseGraphic = this.objectGraphicLayer.OfType<PolygonControlGraphic<TextBoxControl>>().FirstOrDefault(item => item.ObjectID == objectID && item.Type == objectType);
                   break;
                case MapObjectType.Line:
                    baseGraphic =
                        this.objectGraphicLayer.OfType<LineGraphic>().FirstOrDefault(
                            item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.DrawLine:
                    baseGraphic =
                        this.objectGraphicLayer.OfType<LineGraphic>().FirstOrDefault(
                            item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.Image:
                    baseGraphic =
                        this.objectGraphicLayer.OfType<ImagePolygonGraphic>().FirstOrDefault(
                            item => item.ObjectID == objectID && item.Type == objectType);
                    break;
                case MapObjectType.ImageLinkZone:
                    if(this.IsConsoleMode)
                        goto case MapObjectType.LinkZone;
                    
                    goto case MapObjectType.Image;

                case MapObjectType.None:
                    baseGraphic =
                       this.objectGraphicLayer.OfType<BaseGraphic>().FirstOrDefault(
                           item => item.ObjectID == objectID);
                    break;
                default:
                    baseGraphic =
                       this.objectGraphicLayer.OfType<BaseGraphic>().FirstOrDefault(
                           item => item.ObjectID == objectID && item.Type == objectType);
                    break;

            }

            return baseGraphic;
        }

        /// <summary>
        /// Object Data 불러오기
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType"></param>
        /// <param name="isPublicData"></param>
        /// <returns></returns>
        protected BaseMapObjectInfoData GetObjectData(string objectId, MapObjectType objectType, bool isPublicData = true)
        {
            if (!isPublicData) return this.privateGraphicDataManager.GetObjectDataByObjectID(objectId, objectType);

            var data = this.publicGraphicDataManager.GetObjectDataByObjectID(objectId, objectType) ??
                       this.privateGraphicDataManager.GetObjectDataByObjectID(objectId, objectType);

            return data;
        }

        /// <summary>
        /// Splunk Object Data 불러오기
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        protected MapSplunkObjectDataInfo GetSplunkData(string objectId)
        {
            return this.savedSplunkObjectDataManager.GetObjectDataByObjectID(objectId) as MapSplunkObjectDataInfo;
        }

        /// <summary>
        /// DataModel을 가지고 Camera에 그래픽 데이터를 표출 함
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isPublicData"></param>
        protected void AddMapObject(BaseMapObjectInfoData data, bool isPublicData)
        {
            switch (data.ObjectType)
            {
                case MapObjectType.Camera:
                    // Camera Object가 Undo / Redo로 인해 제거된 후 다시 생성될때는 ObjectID를 다시 할당해준다.
                    // Video Lib에서 기존 ObjectID가 관리되고 있어 Undo / Redo로 다시 생성될때 영상이 제대로 보이지 않는 문제가 있다.
                    string TmpGraphicObjectID = data.ObjectID;
                    if (data.IsUndoManage)
                    {
                        data.ObjectID = Guid.NewGuid().ToString();
                    }
                    
                    MapCameraObjectComponentDataInfo CameraObjectInfo = new MapCameraObjectComponentDataInfo(data);
                    this.MakeCameraComponentObjectGraphic(CameraObjectInfo, TmpGraphicObjectID);
                    data.ObjectID = CameraObjectInfo.ObjectID;
                    data = CameraObjectInfo;
                     
                    /*
                    if (IsMapDataLoad)
                    {
                        MapCameraObjectComponentDataInfo CameraObjectInfo = new MapCameraObjectComponentDataInfo(data);
                        this.MakeCameraComponentObjectGraphic(CameraObjectInfo);
                        data.ObjectID = CameraObjectInfo.ObjectID;
                        data = CameraObjectInfo;
                    }
                    else
                    {
                        this.MakeCameraComponentObjectGraphic(data as MapCameraObjectComponentDataInfo);
                    }
                     * */
                    break;
                case MapObjectType.BookMark:
                    var bookMarkDataInfo = data as MapBookMarkDataInfo;
                    this.AddBookMark(bookMarkDataInfo, isPublicData);
                    break;
                case MapObjectType.LinkZone:
                    var linkZoneDataInfo = data as MapLinkZoneObjectDataInfo;
                    this.MakeLinkZoneGraphic(linkZoneDataInfo, isPublicData);
                    break;
                case MapObjectType.ImageLinkZone:
                    var imageLinkZoneDataInfo = data as MapLinkZoneObjectDataInfo;
                    this.MakeImageLinkZoneGraphic(imageLinkZoneDataInfo, isPublicData);
                    break;
                case MapObjectType.Location:
                    var locationDataInfo = data as MapLocationObjectDataInfo;
                    this.MakeLocationGraphic(locationDataInfo, isPublicData);
                    break;
                case MapObjectType.Address:
                    var addressDataInfo = data as MapAddressObjectDataInfo;
                    this.MakeAddressGraphic(addressDataInfo, isPublicData);
                    break;
                case MapObjectType.Workstation:
                    var workStationDataInfo = data as MapWorkStationObjectDataInfo;
                    this.MakeWorkStationGraphic(workStationDataInfo, isPublicData);
                    break;
                case MapObjectType.Splunk:
                    this.MakeSplunkGraphic(data as MapSplunkObjectDataInfo);
                    break;
                case MapObjectType.Text:
                    this.MakeTextGraphic(data as MapTextObjectDataInfo, isPublicData);
                    break;
                case MapObjectType.Line:
                    this.MakeLineGraphic(data as MapLineObjectDataInfo, isPublicData);
                    break;
                case MapObjectType.Image:
                    this.MakeImageGraphic(data as MapImageObjectDataInfo, isPublicData);
                    break;
                case MapObjectType.Universal:
                    this.MakeUniversalGraphic(data as MapUniversalObjectDataInfo);
                    break;
                default:
                    InnowatchDebug.Logger.Trace("해당 MapObjectType에 대한 구현이 없습니다.");
                    throw new NotImplementedException("해당 MapObjectType에 대한 구현이 없습니다.");
                    //break;
            }
        }

        /// <summary>
        /// Map Object Data들은 한번에 Load 한다
        /// </summary>
        /// <param name="mapObjectInfoDatas"></param>
        /// <param name="isPublicData"></param>
        protected virtual void LoadMapObjectInfoDatas(ObservableCollection<BaseMapObjectInfoData> mapObjectInfoDatas, bool isPublicData, bool IsConvertCoordinates = true)
        {
            if (mapObjectInfoDatas == null)
            {
                return;
            }

            foreach (var data in mapObjectInfoDatas.OrderBy(data => data.Name).Where(data => data != null))
            {
                try
                {
                    // IsConvertCoordinates Map에서 History에 의해 Object가 추가 될 경우 mapObjectInfoDatas의 좌표 정보는 이미 해당 Map에 맞는 좌표 정보로 저장되어 있기 때문에 새로운 좌표 계산할 필요가 없다.    [2014. 10. 21 엄태영]
                    if (ArcGISDataConvertHelper.IsGISMapType(this.mapSettingDataInfo.MapType) && IsConvertCoordinates)
                        GeometryHelper.FromGeographicObjectPosition(data, this.mapSettingDataInfo.MapType);

                    this.AddMapObject(data, isPublicData);
                }
                catch (NullReferenceException exception)
                {
                    InnowatchDebug.Logger.WriteInfoLog(exception.ToString());
                }
            }
        }

        #endregion //CommonData
                
        #region PlaceList Object

        protected void AddBookMark(MapBookMarkDataInfo dataInfo, bool isPublicData)
        {
            if(isPublicData) this.publicGraphicDataManager.AddBookMark(dataInfo);
            else this.privateGraphicDataManager.AddBookMark(dataInfo);
        }

        protected virtual IconGraphic MakeLocationGraphic(MapLocationObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeLocationGraphic(dataInfo, this.isEditMode)
                : this.privateGraphicDataManager.MakeLocationGraphic(dataInfo, this.isEditMode);
        }

        protected virtual IEnumerable<BaseGraphic> MakeImageLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeImageLinkZoneGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeImageLinkZoneGraphic(dataInfo);
        }

        protected virtual LinkZoneGraphic MakeLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeLinkZoneGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeLinkZoneGraphic(dataInfo);
        }

        protected virtual LinkZoneGraphic MakeWorkStationGraphic(MapWorkStationObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeWorkStationGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeWorkStationGraphic(dataInfo);
        }

        protected virtual IconGraphic MakeAddressGraphic(MapAddressObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeAddressGraphic(dataInfo, this.isEditMode)
                : this.privateGraphicDataManager.MakeAddressGraphic(dataInfo, this.isEditMode);
        }

        protected virtual PolygonControlGraphic<TextBoxControl> MakeTextGraphic(MapTextObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeTextGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeTextGraphic(dataInfo);
        }

        protected virtual LineGraphic MakeLineGraphic(MapLineObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeLineGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeLineGraphic(dataInfo);
        }

        protected virtual PolygonControlGraphic<LineCanvasControl> MakeDrawLineGraphic(LineCanvasControl lcc, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeDrawLineGraphic(lcc)
                : this.privateGraphicDataManager.MakeDrawLineGraphic(lcc);
        }

        protected virtual ImagePolygonGraphic MakeImageGraphic(MapImageObjectDataInfo dataInfo, bool isPublicData)
        {
            return isPublicData
                ? this.publicGraphicDataManager.MakeImageGraphic(dataInfo)
                : this.privateGraphicDataManager.MakeImageGraphic(dataInfo);
        }

        /// <summary>
        /// Search 후에 Address Graphic 하나 추가 
        /// </summary>
        /// <param name="searchedAddressIconGraphic"></param>
        /// <param name="zIndex"></param>
        protected  virtual void SetSearchAddressGraphic(SearchedAddressIconGraphic searchedAddressIconGraphic, int zIndex = -1)
        {
            if(searchedAddressIconGraphic == null) return;
            
            this.objectGraphicLayer.Graphics.Add(searchedAddressIconGraphic);

            if (zIndex >= 0)
            {
                searchedAddressIconGraphic.ZIndex = zIndex;
            }
        }

        /// <summary>
        /// Location Graphic 하나 추가 
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="zIndex"></param>
        protected virtual void SetBaseGraphic(BaseGraphic graphic, int zIndex, ZLevel zLevel)
        {
            BaseMapObjectInfoData ObjectInfoData = this.GetObjectData(graphic.ObjectID, graphic.Type, !this.IsConsoleMode);
            graphic.ObjectInfoData = ObjectInfoData;

            this.objectGraphicLayer.Graphics.Add(graphic);

            if (zIndex == ArcGISConstSet.UndefinedZIndex || zIndex == int.MaxValue)
            {
                zIndex = this.NextZIndex(zLevel);
            }

            if ((zIndex & ZLevelMask) != (int)zLevel)
            {
                // zLevel 개념이 생기기 이전에 만든 오브젝트들의 zIndex를 zLevel에 맞게 바꿔주기 위함.
                Debug.Print("zIndex({0})가 zLevel({1})에 포함되지 않습니다.", zIndex, zLevel);
                zIndex = (int)zLevel;
            }

            graphic.ZIndex = zIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        protected  virtual void DeleteGraphic(BaseGraphic graphic)
        {
            if (graphic == null) return;

            this.objectGraphicLayer.Graphics.Remove(graphic);
        }

        protected virtual void DeleteSearchedAddressGraphic(SearchedAddressIconGraphic searchedAddressIconGraphic)
        {
            if (searchedAddressIconGraphic == null) return;
            
            this.objectGraphicLayer.Graphics.Remove(searchedAddressIconGraphic);
        }

        #endregion PlaceList Object

        #region Object

        protected bool IsInMap(List<Point> pointCollection)
        {
            var minX = pointCollection.Min(point => point.X);
            var maxX = pointCollection.Max(point => point.X);
            var minY = pointCollection.Min(point => point.Y);
            var maxY = pointCollection.Max(point => point.Y);

            if (this.baseMap.Extent.XMax < minX || this.baseMap.Extent.XMin > maxX) return false;
            if (this.baseMap.Extent.YMax < minY || this.baseMap.Extent.YMin > maxY) return false;
            
            return true;
        }

        protected bool IsInMap(Point position)
        {
            if (this.baseMap.Extent.XMax < position.X || this.baseMap.Extent.XMin > position.X) return false;
            if (this.baseMap.Extent.YMax < position.Y || this.baseMap.Extent.YMin > position.Y) return false;
            
            return true;
        }

        #endregion 

        #region UI Control

        protected void ShowLocationPoupWindow(IconGraphic iconGraphic)
        {
            if (isEditMode) return;

            if (iconGraphic == null) return;

            var locationObjectData = this.GetObjectData(iconGraphic.ObjectID, iconGraphic.Type) as MapLocationObjectDataInfo;

            var point = iconGraphic.Geometry as MapPoint;

            this.locationInfoWindowManager.ShowInfoWindow(locationObjectData, point);
        }

        #endregion UI Control

        #region Z Index

        public enum ZLevel : int
        {
            L0 = 0,
            L1 = 0x20000000,
            L2 = 0x40000000,
            L3 = 0x60000000,
        }

        public static readonly int ZLevelMask = unchecked((int)0xE0000000);

        protected int NextZIndex(ZLevel zLevel)
        {
            var maxZIndex = (int)this.Dispatcher.Invoke(new Func<int>(() =>
            {
                try
                {
                    return this.objectGraphicLayer.Graphics
                        .OfType<BaseGraphic>()
                        .Where(graphic => (graphic.ZIndex & ZLevelMask) == (int)zLevel)
                        .Max(graphic => graphic.ZIndex);
                }
                catch (InvalidOperationException)
                {
                    // 하나도 없을 때
                    return (int)zLevel;
                }
            }));

            if ((maxZIndex & ~ZLevelMask) == ~ZLevelMask)
            {
                throw new InvalidOperationException(String.Format("ZLevel {0}에 넣을 수 있는 그래픽 개수를 초과하였습니다.", zLevel));
            }

            return maxZIndex + 1;
        }

        protected enum BringToDirection { Front, ToBack, Forward, Back }

        protected void BringSelectedGraphicsTo(BringToDirection direction)
        {
            Debug.Assert(this.objectGraphicLayer != null);

            List<BaseGraphic> sortedGraphicList = new List<BaseGraphic>();

            if (direction == BringToDirection.Back)
            {
                sortedGraphicList = this.objectGraphicLayer.Graphics
                .OfType<BaseGraphic>()
                .Where(graphic => (graphic.ZIndex & ZLevelMask) == (int)ZLevel.L0)
                .OrderBy(graphic =>
                    Tuple.Create(
                        graphic.SelectFlag == (direction == BringToDirection.Back) ? graphic.ZIndex - 1 : graphic.ZIndex + 1,
                        graphic.ZIndex
                    )
                ).ToList();
            }
            else if (direction == BringToDirection.Forward)
            {
                sortedGraphicList = this.objectGraphicLayer.Graphics
                .OfType<BaseGraphic>()
                .Where(graphic => (graphic.ZIndex & ZLevelMask) == (int)ZLevel.L0)
                .OrderBy(graphic =>
                    Tuple.Create(
                        graphic.SelectFlag == (direction == BringToDirection.Forward) ? graphic.ZIndex + 1 : graphic.ZIndex - 1,
                        graphic.ZIndex
                    )
                ).ToList();
            }
            else if (direction == BringToDirection.ToBack || direction == BringToDirection.Front)
            {
                sortedGraphicList = this.objectGraphicLayer.Graphics
                .OfType<BaseGraphic>()
                .Where(graphic => (graphic.ZIndex & ZLevelMask) == (int)ZLevel.L0)
                .OrderBy(graphic =>
                    Tuple.Create(
                        graphic.SelectFlag == (direction == BringToDirection.Front) ? 1 : 0,
                        graphic.ZIndex
                    )
                ).ToList();
            }

            for (var i = 0; i < sortedGraphicList.Count; i++)
            {
                sortedGraphicList[i].ZIndex = i;
            }
        }

        #endregion Z Index
        #endregion //Method
    }
}
