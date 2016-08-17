using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArcGISControl.Command;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

namespace ArcGISControl
{
    public partial class ArcGISClientViewer
    {
        #region Fields

        /// <summary>
        /// Location List 
        /// </summary>
        public List<BaseMapObjectInfoData> PublicLocationList
        {
            get { return this.publicGraphicDataManager.ObjectDatas.ToList(); }
        }

        /// <summary>
        /// Location List
        /// </summary>
        public List<BaseMapObjectInfoData> PrivateLocationList
        {
            get { return this.privateGraphicDataManager.ObjectDatas.ToList(); }
        }

        /// <summary>
        /// CameraList
        /// </summary>
        public ObservableCollection<MapCameraObjectComponentDataInfo> CameraList
        {
            get { return this.cameraGraphicDataManager.CameraObjectComponentDatas; }
        }

        /// <summary>
        /// Searched Address Data
        /// </summary>
        public ObservableCollection<MapAddressObjectDataInfo> SearchedAddressDatas
        {
            get { return this.searchAddressGraphicDataManager.SearchAddressObjectDatas; }
        }

        /// <summary>
        /// SplunkDatas
        /// </summary>
        public ObservableCollection<MapSplunkObjectDataInfo> SavedSplunkList
        {
            get { return this.savedSplunkObjectDataManager.SplunkObjectDatas; }
        }

        /// <summary>
        /// Universal Object DataInfo List
        /// </summary>
        public IEnumerable<MapUniversalObjectDataInfo> UniversalDataInfoList
        {
            get { return this.universalObjectDataManager.DataInfoList; }
        }

        /// <summary>
        /// VW Control 에서 Map Editing 여부 확인
        /// </summary>
        public bool IsAbleToEditMap
        {
            get
            {
                return this.baseMap != null && this.baseTiledMapServiceLayer != null &&
                       this.baseTiledMapServiceLayer.FullExtent != null;
            }
        }

        /// <summary>
        /// 현재 Map의 Guid 를 받아 온다 
        /// </summary>
        public string CurrentMapSettingInfoDataId
        {
            get { return this.mapSettingDataInfo.ID; }
        }

        /// <summary>
        /// 현재 보고 있는 Map의 Index
        /// </summary>
        public int ViewingLinkedMapIndex
        {
            get { return this.linkedMapHistory.Index; }
        }

        /// <summary>
        /// Linked Map의 전체 개수
        /// </summary>
        public int LinkedMapTotalCount
        {
            get { return this.linkedMapHistory.Count; }
        }

        /// <summary>
        /// Visual change All Camera
        /// </summary>
        public bool IsHideAllCamera
        {
            get { return this.isHideAllCamera; }
            set
            {
                this.isHideAllCamera = value;

                this.ChangeCameraVideoVisible();
            }
        }

        public bool isReportWritingMode;

        public bool IsReportWritingMode
        {
            get { return this.isReportWritingMode; }
            set
            {
                this.isReportWritingMode = value;
                this.memoObjectDataManager.IsReportWritingMode = this.isReportWritingMode;
                this.ReleaseAllSelectedObject();
            }
        }

        public Rect CurrentMapExtent
        {
            get
            {
                if (this.baseMap == null || this.baseMap.Extent == null)
                    throw new InvalidOperationException("baseMap이 null일 때 호출할 수 없습니다.");

                var extent = this.baseMap.Extent;

                return new Rect(
                    new Point(extent.XMin, extent.YMin),
                    new Point(extent.XMax, extent.YMax)
                );
            }

            set
            {
                this.ChangeZoomToLocation(new Envelope(value.Left, value.Top, value.Right, value.Bottom));
            }
        }

        public string CameraPopupUnigridGuid
        {
            get { return this.cameraPopupControlManager.CameraUnigridId; }
        }

        public string CameraPopupOriginalUnigridGuid
        {
            get { return this.cameraPopupControlManager.OriginalCameraUnigridId; }
        }

        #endregion Fields

        #region Methods

        #region Objects

        /// <summary>
        /// Add Camera From CU LIST
        /// </summary>
        /// <param name="cameraSettingDatas">Camera 정보</param>
        public void AddCameraObjects(IEnumerable<MapCameraSettingDataInfo> cameraSettingDatas)
        {
            var isFirstObject = true;

            foreach (var cameraSettingdata in cameraSettingDatas)
            {
                try
                {
                    Point firstPoint;

                    if (isFirstObject)
                    {
                        firstPoint = new Point(baseMap.Extent.GetCenter().X, baseMap.Extent.GetCenter().Y);
                        isFirstObject = false;
                    }
                    else
                    {
                        var lastCameraComponentObjectData = this.cameraGraphicDataManager.CameraObjectComponentDatas.Last() as MapCameraObjectComponentDataInfo;

                        if (lastCameraComponentObjectData != null)
                        {
                            lastCameraComponentObjectData.SetCameraComponentBounds();
                            firstPoint = new Point(lastCameraComponentObjectData.ExtentMax.X + (cameraSettingdata.Space * this.baseMap.Resolution), 
                                lastCameraComponentObjectData.ExtentMax.Y);
                        }
                        else
                        {
                            firstPoint = new Point(baseMap.Extent.GetCenter().X, baseMap.Extent.GetCenter().Y);
                        }
                    }

                    var cameraComponentObjectData = this.cameraGraphicDataManager.GetNewCameraComponentObjectData(cameraSettingdata, this.baseMap.Resolution, firstPoint);

                    this.MakeCameraComponentObjectGraphic(cameraComponentObjectData, null);

                    this.RaiseObjectAddedEvent(cameraComponentObjectData);

                }
                catch (Exception exception)
                {
                    InnowatchDebug.Logger.WriteLogExceptionMessage(exception, exception.GetType().ToString());
                }
            }

            this.RefreshAllCameraVideoRect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splunkObjectDataInfo"></param>
        public void AddSplunkObject(MapSplunkObjectDataInfo splunkObjectDataInfo)
        {
            try
            {
                if (splunkObjectDataInfo == null) return;

                this.savedSplunkObjectDataManager.CreatePositionMapSplunkObjectData(
                                                    new Point(baseMap.Extent.GetCenter().X, baseMap.Extent.GetCenter().Y),
                                                    this.baseMap.Resolution, splunkObjectDataInfo);

                this.MakeSplunkGraphic(splunkObjectDataInfo);

                this.RaiseObjectAddedEvent(splunkObjectDataInfo);
            }
            catch (Exception exception)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(exception, exception.GetType().ToString(), true);
            }
        }

        /// <summary>
        /// Private Object List를 받아 화면에 뿌려 준다
        /// From Viewer
        /// </summary>
        /// <param name="datas"></param>
        public void LoadPrivateObjects(ObservableCollection<BaseMapObjectInfoData> datas)
        {
            if (datas == null)
            {
                this.isPrivateObjectLoaded = true;
                return;
            }

            this.isPrivateObjectLoaded = false;

            if (datas.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => this.LoadMapObjectInfoDatas(datas, false)), DispatcherPriority.Loaded);
            }
            else
            {
                this.isPrivateObjectLoaded = true;
            }
        }

        /// <summary>
        /// Memo 생성
        /// </summary>
        public void MakeMemo()
        {
            var center = this.baseMap.Extent.GetCenter();
            var centerPoint = new Point(center.X, center.Y);
            var graphics = this.memoObjectDataManager.AddMemoObject(centerPoint, this.baseMap.Resolution, this.arcGisControlViewerApi.GetUserID());

            if (graphics == null) return;

            this.ReleaseAllSelectedObject(true);

            foreach (var graphic in graphics)
            {
                this.SetBaseGraphic(graphic, ArcGISConstSet.UndefinedZIndex, ZLevel.L1);
            }

            this.selectedGraphicList.Add(graphics.First());
            this.StartMemoEditing(graphics.First());
        }

        private static readonly string Delim = "<>";

        /// <summary>
        /// 맵에 있는 모든 메모 오브젝트를 serialize 하여 string으로 돌려줍니다.
        /// </summary>
        /// <returns>A serialized memo data string</returns>
        public string GetMemoDataString()
        {
            var list = this.memoObjectDataManager.SerializeObjects();

            foreach (var objectString in list)
                Debug.Assert(!objectString.Contains(Delim));

            return String.Join(Delim, list);
        }

        /// <summary>
        /// <see cref="GetMemoDataString"/>에서 리턴한 형태의 string을 받아서
        /// deserialize 하고, 메모 오브젝트를 만들어 맵에 추가합니다.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// data string이 null일 때 발생합니다.
        /// </exception>
        /// <param name="data">A serialized memo data string</param>
        public void SetMemoDataString(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var graphics = this.memoObjectDataManager.DeserializeObjects(data.Split(new string[] { Delim }, StringSplitOptions.None));

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var graphic in graphics)
                {   
                    var dataInfo = this.memoObjectDataManager.GetMemoObjectDataInfo(graphic.ObjectID);
                    var zIndex = dataInfo.ObjectZIndex;
                    if (graphic.Type == MapObjectType.MemoTip)
                        zIndex = dataInfo.TipZIndex;
                    this.SetBaseGraphic(graphic, zIndex, ZLevel.L1);
                }
            }), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Book Mark 생성 
        /// </summary>
        /// <param name="name"></param>
        public void MakeBookMark(string name)
        {
            var bookMarkDataInfo
                = this.IsConsoleMode ? this.privateGraphicDataManager.CreateBookMarkObjectData(name, this.MaxExtentToPoint, this.MinExtentToPoint) :
                this.publicGraphicDataManager.CreateBookMarkObjectData(name, this.MaxExtentToPoint, this.MinExtentToPoint);

            this.RaiseObjectAddedEvent(bookMarkDataInfo);
        }

        /// <summary>
        /// 외부 에서 Location 선택
        /// </summary>
        /// <param name="selectedObjects"></param>
        /// <returns></returns>
        public bool SelectObjects(List<BaseMapObjectInfoData> selectedObjects)
        {
            this.ReleaseAllSelectedObject();

            if (selectedObjects == null)
            {
                return false;
            }

            foreach (var selectedObject in selectedObjects)
            {
                this.SelectObjectInternal(selectedObject);
            }

            return true;
        }

        /// <summary>
        /// 외부 에서 Location 선택
        /// </summary>
        /// <param name="selectedObject"></param>
        /// <returns></returns>
        public bool SelectObject(BaseMapObjectInfoData selectedObject)
        {
            this.ReleaseAllSelectedObject();

            return this.SelectObjectInternal(selectedObject);
        }

        internal bool SelectObjectInternal(BaseMapObjectInfoData selectedObject)
        {
            if (selectedObject == null)
            {
                return false;
            }


            if (selectedObject.ObjectType == MapObjectType.Camera)
            {
                var cameraDataObject = selectedObject as MapCameraObjectComponentDataInfo;

                if (cameraDataObject == null) return false;

                //현재 선택된 Data와 뭉치 데이터가 있음 리스트 형태로 받아 온다.
                var baseDataList =
                    this.objectGraphicLayer.Graphics.OfType<CameraIconGraphic>().Where(g => g.ObjectID == cameraDataObject.ObjectID).ToList();

                foreach (var baseGraphic in baseDataList.Where(baseGraphic => baseGraphic != null && !baseGraphic.SelectFlag))
                {
                    this.ChangeGraphicSelection(baseGraphic);

                    if (!this.isEditMode && (baseGraphic.Type == MapObjectType.Location || baseGraphic.Type == MapObjectType.Address))
                    {
                        this.ShowLocationPoupWindow(baseGraphic as IconGraphic);
                    }
                }
            }
            else if (selectedObject.ObjectType == MapObjectType.Splunk)
            {
                var splunkDataObject = selectedObject as MapSplunkObjectDataInfo;

                if (splunkDataObject == null) return false;

                var baseDataList =
                    this.objectGraphicLayer.Graphics.OfType<SplunkIconGraphic>().Where(g => g.ObjectID == splunkDataObject.ObjectID).ToList();

                foreach (var baseGraphic in baseDataList.Where(baseGraphic => baseGraphic != null && !baseGraphic.SelectFlag))
                {
                    this.ChangeGraphicSelection(baseGraphic);
                }
            }
            else if (selectedObject.ObjectType == MapObjectType.Universal)
            {
                var universalControlGraphic = this.universalObjectDataManager.GetControlGraphic(selectedObject.ObjectID);

                this.ChangeGraphicSelection(universalControlGraphic);
            }
            else
            {
                var baseDataList
                    = this.objectGraphicLayer.Graphics.OfType<BaseGraphic>().Where(
                        g =>
                            g.ObjectID == selectedObject.ObjectID &&
                            g.Type == selectedObject.ObjectType).ToList();

                foreach (var baseGraphic in baseDataList.Where(baseGraphic => baseGraphic != null && !baseGraphic.SelectFlag))
                {
                    this.ChangeGraphicSelection(baseGraphic);

                    if (!this.isEditMode && (baseGraphic.Type == MapObjectType.Location || baseGraphic.Type == MapObjectType.Address))
                    {
                        this.ShowLocationPoupWindow(baseGraphic as IconGraphic);
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// VW Panomorph 에서만 쓰인다.
        /// 
        /// </summary>
        /// <param name="objectID"></param>
        public void SelectCameraVideoGraphic(string objectID)
        {
            var videoGraphic = this.objectGraphicLayer.OfType<CameraVideoGraphic>().FirstOrDefault(g => g.ObjectID == objectID);

            this.SelectGraphicObject(videoGraphic);
        }

        /// <summary>
        /// 밖에서 혹시나 Object를 Deselect가 필요 할듯 하여 만들어 놓았음 
        /// </summary>
        public void DeselectMapObjects()
        {
            this.ReleaseAllSelectedObject();
        }

        public void CancelAddedObjectData(BaseMapObjectInfoData data)
        {
            this.DeleteMapObjectData(data, true);
        }

        /// <summary>
        /// 지정된 Data의 Object 가 삭제 됨
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DeleteMapObjectData(BaseMapObjectInfoData data, bool isAlwaysDelete = false)
        {
            if (!this.isEditMode && !isAlwaysDelete) return false;

            switch (data.ObjectType)
            {
                case MapObjectType.Camera:
                case MapObjectType.CameraIcon:
                    this.DeleteCameraObject(data as MapCameraObjectComponentDataInfo);
                    break;

                case MapObjectType.Address:
                    var addressObjectData = this.searchAddressGraphicDataManager.GetObjectDataByObjectID(data.ObjectID) as MapAddressObjectDataInfo;
                    if (addressObjectData != null)
                    {
                        addressObjectData.IsSaved = false;
                    }
                    this.DeleteMapGraphicObject(data);
                    break;

                case MapObjectType.Splunk:
                    this.DeleteMapSplunkObject(data);
                    break;

                case MapObjectType.ImageLinkZone:
                case MapObjectType.LinkZone:
                    var linkZoneData = data as MapLinkZoneObjectDataInfo;
                    if (linkZoneData != null) this.publicGraphicDataManager.StopSplunkService(linkZoneData.ObjectID, linkZoneData.ColorSplunkBasicInformationData);
                    this.DeleteMapGraphicObject(data);
                    break;

                case MapObjectType.Universal:
                    var universalIconGraphic = this.universalObjectDataManager.GetIconGraphic(data.ObjectID);
                    var universalControlGraphic = this.universalObjectDataManager.GetControlGraphic(data.ObjectID);
                    this.DeleteGraphic(universalIconGraphic);
                    this.DeleteGraphic(universalControlGraphic);
                    this.universalObjectDataManager.RemoveObject(data.ObjectID);
                    break;

                default:
                    this.DeleteMapGraphicObject(data);
                    break;
            }

            this.RaiseObjectDeletedEvent(data.ObjectID, data.ObjectType);

            this.SelectObjectProperty();

            return true;
        }

        /// <summary>
        /// MapControl에 Selected 한 Object 를 삭제 한다.
        /// </summary>
        public void DeleteSelectedObjects()
        {
            if (this.selectedGraphicList == null || this.selectedGraphicList.Count <= 0)
            {
                return;
            }

            var selectedList = this.selectedGraphicList.ToList();
            List<BaseMapObjectInfoData> RemoveMapObjectInfoData = new List<BaseMapObjectInfoData>();

            foreach (var graphic in selectedList)
            {
                //카메라 지우기
                if (ArcGISDataConvertHelper.IsCameraGraphic(graphic.Type))
                {
                    var cameraObjectComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);

                    if (cameraObjectComponentData is MapCameraObjectComponentDataInfo)
                    {
                        if (graphic.Type != MapObjectType.CameraIcon)
                        {
                            this.DeleteCameraElement(cameraObjectComponentData as MapCameraObjectComponentDataInfo, graphic);
                        }
                        else
                        {
                            this.DeleteMapObjectData(cameraObjectComponentData);
                            RemoveMapObjectInfoData.Add(cameraObjectComponentData);
                        }
                    }

                }
                // 메모 지우기
                else if (ArcGISDataConvertHelper.IsMemoGraphic(graphic.Type))
                {
                    if (this.isReportWritingMode)
                    {
                        var textBoxControlGraphic = this.memoObjectDataManager.GetTextBoxControlGraphic(graphic.ObjectID);
                        var memoTipGraphic = this.memoObjectDataManager.GetMemoTipGraphic(graphic.ObjectID);
                        if (textBoxControlGraphic != null && memoTipGraphic != null && !textBoxControlGraphic.Control.Focusable)
                        {
                            this.DeleteGraphic(textBoxControlGraphic);
                            this.DeleteGraphic(memoTipGraphic);
                            this.memoObjectDataManager.RemoveMemoObject(graphic.ObjectID);
                            this.graphicEditorManager.StopEditing(this.DeleteGraphic);

                            this.HideMemoProperty();
                        }
                    }
                }
                // Universal Object 지우기
                else if (ArcGISDataConvertHelper.IsUniversalGraphic(graphic.Type))
                {
                    var universalDataInfo = this.universalObjectDataManager.GetDataInfo(graphic.ObjectID);
                    RemoveMapObjectInfoData.Add(universalDataInfo);
                    this.DeleteMapObjectData(universalDataInfo);
                }
                //그 외 오브젝트 지우기
                else
                {
                    switch (graphic.Type)
                    {
                        case MapObjectType.Splunk:
                        case MapObjectType.SplunkControl:
                        case MapObjectType.SplunkIcon:
                            var splunkData = this.SavedSplunkList.FirstOrDefault(data => data.ObjectID == graphic.ObjectID);
                            RemoveMapObjectInfoData.Add(splunkData);
                            if (splunkData != null) this.DeleteMapObjectData(splunkData);
                            break;
                        default:
                            if (graphic.Type == MapObjectType.Text)
                            {
                                var textControlGraphic = this.objectGraphicLayer.OfType<PolygonControlGraphic<TextBoxControl>>().FirstOrDefault(item => item.ObjectID == graphic.ObjectID && item.Type == graphic.Type);
                                if (textControlGraphic == null || (textControlGraphic != null && textControlGraphic.Control.IsEditingTextBox() == true))
                                    return;
                            }

                            var locationData = this.PublicLocationList.FirstOrDefault(data => data.ObjectID == graphic.ObjectID && data.ObjectType == graphic.Type);
                            RemoveMapObjectInfoData.Add(locationData);
                            if (locationData != null) this.DeleteMapObjectData(locationData);
                            break;
                    }
                }

                this.selectedGraphicList.Remove(graphic);
                if(this.editor != null) this.editor.Stop();
            }

            if(RemoveMapObjectInfoData.Count > 0 )
                this.AddCommandToHistory(new CommandRemoveOfMany(RemoveMapObjectInfoData, this.AddMapObject, this.DeleteMapObjectData, this.RaiseObjectAddedEvent));

            //if (this.onSelectLocationData != null)
            //            this.onSelectLocationData(string.Empty, MapObjectType.None);

            this.ChangeCursor(Cursors.Arrow);
        }

        /// <summary>
        /// Location의 위치로 이동한다.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isFit"></param>
        /// <returns></returns>
        public bool GoToLocation(BaseMapObjectInfoData data, bool isFit = false)
        {
            var newExtent = new Envelope(data.ExtentMin.X, data.ExtentMin.Y, data.ExtentMax.X, data.ExtentMax.Y);
            
            if (isFit)
            {
                this.ChangeZoomToLocation(newExtent);

                return true;
            }

            var resolutionAfterPan = (newExtent.Width / this.baseMap.ActualWidth);

            const int basicGap = 150;

            var w = basicGap * resolutionAfterPan;
            var h = basicGap * resolutionAfterPan;

            if (data is MapLocationObjectDataInfo)
            {
                this.ChangeLocation((data as MapLocationObjectDataInfo).Position);
            }
            else if (data is MapSplunkObjectDataInfo)
            {
                newExtent = new Envelope(newExtent.XMin - w, newExtent.YMin - h,
                                          newExtent.XMax + w, newExtent.YMax + h);

                this.ChangeZoomToLocation(newExtent);
            }
            else if (data is MapBookMarkDataInfo)
            {
                this.ChangeZoomToLocation(newExtent);
            }
            else if (data is MapCameraObjectComponentDataInfo)
            {
                var cmeraData = data as MapCameraObjectComponentDataInfo;

                if (cmeraData.CameraIcon.Position.Y >= data.ExtentMax.Y)
                {
                    if (this.IsConsoleMode || !this.isEditMode)
                    {
                        newExtent.YMax = (350 * resolutionAfterPan * 2) + newExtent.YMax;
                    }
                    else
                    {
                        newExtent.YMax = (50 * resolutionAfterPan * 2) + newExtent.YMax;
                    }
                }
                else
                {
                    if (this.IsConsoleMode || !this.isEditMode)
                    {
                        newExtent.YMax = (300 * resolutionAfterPan * 2) + newExtent.YMax;
                    }
                }

                newExtent = new Envelope(newExtent.XMin - w, newExtent.YMin - h,
                                           newExtent.XMax + w, newExtent.YMax + h);

                this.ChangeZoomToLocation(newExtent);
            }
            else
            {
                w = newExtent.Width;
                h = newExtent.Height;

                newExtent = new Envelope(newExtent.XMin - (w), newExtent.YMin - (h),
                                           newExtent.XMax + (w), newExtent.YMax + (h));

                this.ChangeZoomToLocation(newExtent);
            }

            return true;
        }

        /// <summary>
        /// Object Data 등록시 자동으로 이름 생성 해 주기 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetNewLocationObjectName(MapObjectType type)
        {
            if (this.IsConsoleMode && this.PrivateLocationList != null)
            {
                return this.privateGraphicDataManager.GetNewObjectName(type);
            }

            if (!this.IsConsoleMode && this.PublicLocationList != null)
            {
                return this.publicGraphicDataManager.GetNewObjectName(type);
            }

            return this.publicGraphicDataManager.GetNewObjectName(MapObjectType.None);
        }

        /// <summary>
        /// 선택된 Camera Rect 의 Size를 받아 온다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <returns>
        /// MapUserControl 기준으로 한 Rect
        /// </returns>
        public Rect GetCameraVideoGraphicRectSize(string unigridGuid)
        {
            var videoGraphic = this.objectGraphicLayer.OfType<CameraVideoGraphic>().FirstOrDefault(g => g.ObjectID == unigridGuid);

            if (videoGraphic != null)
            {
                var videoRect =
                        new Rect((videoGraphic.Geometry.Extent.XMin - baseMap.Extent.XMin) / this.baseMap.Resolution,
                                 (baseMap.Extent.YMax - videoGraphic.Geometry.Extent.YMax) / this.baseMap.Resolution,
                                 videoGraphic.Geometry.Extent.Width / this.baseMap.Resolution,
                                 videoGraphic.Geometry.Extent.Height / this.baseMap.Resolution);

                return videoRect;
            }

            return Rect.Empty;
        }


        /// <summary>
        /// GIS 맵에서는 x를 위도, y를 경도로 하고, Custom map에서는 픽셀 좌표계가 된다.
        /// </summary>
        /// <param name="trail"></param>
        public void DrawLines(IEnumerable<Point> trail)
        {
            var mapPointTrail = trail.Select(p =>
            {
                var converted = this.FromGeographicAdjustForUser(new LatLng(p));
                return new MapPoint(converted.X, converted.Y);
            });

            var pointCollection = new PointCollection(mapPointTrail);

            var polyLine = new Polyline()
            {
                Paths = new ObservableCollection<PointCollection>()
                {
                    pointCollection
                },
                SpatialReference = this.baseMap.SpatialReference
            };

            var graphicToAdd = new Graphic()
            {
                Geometry = polyLine,
                Symbol = new TrailSymbol()
                {
                    Extent = polyLine.Extent,
                    TrailPoints = pointCollection,
                    Progress = 0.49
                }
            };

            if (this.splunkLineToGraphic != null)
            {
                // 이전 라인 제거
                this.objectGraphicLayer.Graphics.Remove(this.splunkLineToGraphic);
            }

            this.splunkLineToGraphic = graphicToAdd;

            this.objectGraphicLayer.Graphics.Add(graphicToAdd);
        }

        #region Search Address

        /// <summary>
        /// Map Search 그리고 Object도 올린다.
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public void SearchMapGeoCoding(string searchText)
        {
            var searchedAddressIconGraphics = this.objectGraphicLayer.Graphics.OfType<SearchedAddressIconGraphic>().ToList();

            //Delete Old Graphics
            foreach (var addressIconGraphic in searchedAddressIconGraphics)
            {
                this.DeleteSearchedAddressGraphic(addressIconGraphic);
            }

            searchedAddressIconGraphics = this.searchAddressGraphicDataManager.SearchMapGeoCoding(searchText, this.mapSettingDataInfo.MapType, this.mapSettingDataInfo.LicenseKey);

            //Add new Graphics
            foreach (var addressIconGraphic in searchedAddressIconGraphics)
            {
                this.SetSearchAddressGraphic(addressIconGraphic);
            }
        }

        /// <summary>
        /// 외부에서 Search되 Object를 Place List 에 Save한다.
        /// </summary>
        /// <param name="addressData"></param>
        public void SaveSearchedAddressObject(MapAddressObjectDataInfo addressData)
        {
            var mapAddressObjectData = this.searchAddressGraphicDataManager.GetSavedSearchedAddressObjectData(addressData);

            var graphic = this.MakeAddressGraphic(mapAddressObjectData, !this.IsConsoleMode);

            //이전에 선택되었던 Object들 모두 해제
            this.ReleaseAllSelectedObject();

            //현재 OBJECT 선택 
            if (graphic != null)
            {
                this.SelectGraphicObject(graphic);

                this.RaiseObjectAddedEvent(mapAddressObjectData);
            }
        }

        #endregion //Search Address

        #endregion //Objects

        #region Map

        public MapSettingDataInfo GetCurrentMapSettingDataInfo()
        {
            return this.mapSettingDataInfo;
        }

        /// <summary>
        /// Map Type 만 변경 했을 경우 Map 불러온다.
        /// </summary>
        /// <param name="mapSettingDataInfo"></param>
        public void ChangeMapType(MapSettingDataInfo mapSettingDataInfo)
        {
            if (ArcGISDataConvertHelper.IsGISMapType(mapSettingDataInfo.MapType))
            {
                this.ChangeMapExtentToGeographic(this.currentMapType);
            }
            else
            {
                mapSettingDataInfo.MapServiceUrl = String.Empty;
            }

            this.currentMapType = mapSettingDataInfo.MapType;

            this.justTileRefresh = true;

            this.LoadMapData(mapSettingDataInfo);
        }

        /// <summary>
        /// Load BaseMap
        /// </summary>
        /// <param name="mapSettingDataInfo"></param>
        public void LoadMapData(MapSettingDataInfo mapSettingDataInfo)
        {
            if (mapSettingDataInfo == null) return;

            mapSettingDataInfo.ExtentMax = new Point();
            mapSettingDataInfo.ExtentMin = new Point();
            this.mapSettingDataInfo = mapSettingDataInfo;
            this.MapBackgroundColor = mapSettingDataInfo.MapBgColor;

            if (string.IsNullOrEmpty(this.mapSettingDataInfo.ID))
                this.mapSettingDataInfo.ID = Guid.NewGuid().ToString();

            this.linkedMapHistory.Clear();
            this.linkedMapHistory.NewLocation(new MapHistoryEntity(mapSettingDataInfo.ID, null));

            var canCutomMapLoad = !ArcGISDataConvertHelper.IsGISMapType(this.mapSettingDataInfo.MapType) && !String.IsNullOrEmpty(this.mapSettingDataInfo.MapServiceUrl);
            if (canCutomMapLoad || ArcGISDataConvertHelper.IsGISMapType(this.mapSettingDataInfo.MapType))
            {
                Application.Current.Dispatcher.Invoke(
                    new Action
                    (
                        delegate
                        {
                            if (this.processingIcon != null) this.processingIcon.Visibility = Visibility.Visible;

                            this.RefreshMapTiles();
                        }
                     )
                );
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                    new Action
                    (
                        delegate()
                        {
                            this.processingIcon.Visibility = Visibility.Collapsed;

                            this.ReleaseMap();
                        }
                    )
                );
            }

            _UndoManager.ClearHistory();
        }

        /// <summary>
        /// Map Service URL 변경 후 호출
        /// </summary>
        /// <param name="url"></param>
        public void ChangeMapTileServiceUrl(string url)
        {
            this.isInitializedMapTileService = false;

            this.justTileRefresh = true;

            this.mapSettingDataInfo.MapServiceUrl = url;

            if (this.processingIcon != null) this.processingIcon.Visibility = Visibility.Visible;

            Application.Current.Dispatcher.Invoke(new Action(() => this.LoadMapTileService(false)));
        }

        /// <summary>
        /// Save Button 으로 호출 
        /// 현재 Data저장
        /// </summary>
        public int SaveCurrentMapSettingData(bool doAdd, string userId)
        {
            if (this.editor != null)
            {
                try
                {
                    this.editor.Stop();
                }
                catch (Exception ex)
                {
                    InnowatchDebug.Logger.WriteLine(ex.ToString());
                } 
            }

            if(this.editor != null) this.editor.Stop();
            this.graphicEditorManager.StopEditing(this.DeleteGraphic);

            InnowatchDebug.Logger.WriteInfoLog(string.Format("[SaveCurrentMapSettingData] Map save start. User ID : {0}", userId));

            //현재 맵 위치 저장
            this.ChangeMapExtentToGeographic(this.mapSettingDataInfo.MapType);

            //var datas = this.CameraList.Concat(this.PublicLocationList);
            //datas = datas.Concat(this.SavedSplunkList);

            //var baseMapObjectInfoDatas = datas as BaseMapObjectInfoData[] ?? datas.ToArray();

            //if (ArcGISDataConvertHelper.IsGISMapType(this.CurrentMapSettingDataInfo.MapType))
            //{
            //    baseMapObjectInfoDatas.ToList().ForEach(
            //    data => GeometryHelper.ToGeographicObjectPosition(data, this.CurrentMapSettingDataInfo.MapType));
            //}

            //this.SaveMapAndFeaturesToBackupFile(baseMapObjectInfoDatas);

            //return mapDataServiceHandler.SaveMapAndFeatures(this.CurrentMapSettingDataInfo, baseMapObjectInfoDatas, userId, doAdd);
                
            var objectList = new List<BaseMapObjectInfoData>();
            objectList.AddRange(this.CameraList.ToList());
            objectList.AddRange(this.PublicLocationList.ToList());
            objectList.AddRange(this.SavedSplunkList.ToList());
                objectList.AddRange(this.UniversalDataInfoList);

                InnowatchDebug.Logger.WriteInfoLog(
                    string.Format("[SaveCurrentMapSettingData] Map saving. Contained object count : {0}, \r\n \t MapCamera : {1}, PublicLocation : {2}, SavedSplunk : {3}, UniversalObject : {4}", 
                        objectList.Count,
                        this.CameraList.Count,
                        this.PublicLocationList.Count,
                        this.SavedSplunkList.Count,
                        this.UniversalDataInfoList.Count()));

            if (ArcGISDataConvertHelper.IsGISMapType(this.mapSettingDataInfo.MapType))
            {
                foreach (var baseMapObjectInfoData in objectList)
                {
                    GeometryHelper.ToGeographicObjectPosition(baseMapObjectInfoData, this.mapSettingDataInfo.MapType);
                }
            }

            this.SaveMapAndFeaturesToBackupFile(objectList);

            return mapDataServiceHandler.SaveMapAndFeatures(this.mapSettingDataInfo, objectList, userId, doAdd);
        }

        /// <summary>
        /// Map Unload 한다
        /// </summary>
        public void UnloadMapControl()
        {
            Application.Current.Dispatcher.Invoke(new Action(()=>this.ReleaseMap()));
        }

        /// <summary>
        /// Map Cell 이 사라질 경우 호출 From Viewer
        /// </summary>
        public void HideMapCell()
        {
            /// todo 
            /// Hide 하기전에 MapCell Show false로 바꾸기
            this.RefreshAllCameraVideoRect();
        }

        /// <summary>
        /// Map Cell 다시 보일경우 호출 Viewer
        /// </summary>
        public void ShowMapCell()
        {
            /// todo 
            /// Hide 하기전에 MapCell Show true로 바꾸기

            this.RefreshAllCameraVideoRect();
        }

        /// <summary>
        /// Map Cell 이 전환된 후 
        /// 자식에서 Point가 인지 되기 전에
        /// Change 함수를 호출 하여 
        /// 자신의 Point를 함께 호출 하여 준다.
        /// </summary>
        /// <param name="mapCellStartPosition"></param>
        public void ChangedPositionMapCell(Point mapCellStartPosition)
        {
            if (this.objectGraphicLayer == null)
                return;

            //Popup Video Refresh
            if (this.cameraPopupControlManager != null)
            {
                this.RefreshCameraPopupControlVideo(mapCellStartPosition);
            }

            //Camera Video Object Refresh
            var videoGraphics = this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>();

            foreach (var videoGraphic in videoGraphics.ToList())
            {
                this.RefreshGraphicObjectVideo(videoGraphic, mapCellStartPosition);
            }
        }

        /// <summary>
        /// Map Cell의 Zindex가 변경 된 후 
        /// 호출
        /// </summary>
        /// <param name="zIndex"></param>
        public void SetCellZindex(int zIndex)
        {
            if (this.objectGraphicLayer == null)
                return;

            //Popup Video Refresh
            if (this.cameraPopupControlManager != null)
            {
                this.RefreshCameraPopupControlVideo();
            }

            //Camera Video Object Refresh
            var videoGraphics = this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>();

            foreach (var videoGraphic in videoGraphics)
            {
                this.RefreshGraphicObjectVideo(videoGraphic);
            }
        }

        /// <summary>
        /// 외부 에서 Map Change 호출
        /// </summary>
        /// <param name="isPrev"></param>
        public void GoLinkedMap(bool isPrev)
        {
            MapHistoryEntity? nextHistory = isPrev ? this.linkedMapHistory.TryGoBack() : this.linkedMapHistory.TryGoNext();

            if (nextHistory.HasValue)
            {
                if (!this.ChangeSelectedLinkedMap(nextHistory.Value.MapId, null))
                {
                    this.linkedMapHistory.CancelMove();
                }
            }
        }

        /// <summary>
        /// 외부 에서 Map Change 호출
        /// </summary>
        /// <param name="aMapId"></param>
        public void GoLinkedMap(string aMapId)
        {
            if (!this.ChangeSelectedLinkedMap(aMapId, null))
            {
                this.linkedMapHistory.CancelMove();
            }
        }

        public void GoToMapByName(string name)
        {
            MapSettingDataInfo mapSettingDataInfo = null;

            if (this.arcGISControlApi != null)
            {
                mapSettingDataInfo = this.arcGISControlApi.GetMapSettingByName(name);
            }

            if (mapSettingDataInfo != null && !string.IsNullOrEmpty(mapSettingDataInfo.ID))
            {
                this.linkedMapHistory.NewLocation(new MapHistoryEntity(mapSettingDataInfo.ID, null));
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.initialExtent = null;
                    this.mapSettingDataInfo = mapSettingDataInfo;
                    this.MapBackgroundColor = mapSettingDataInfo.MapBgColor;
                    this.RefreshMapTiles();
                }));
            }
        }

        /// <summary>
        /// 외부에서 Map 호출
        /// </summary>
        public void GoHomeMap()
        {
            var nextHistory = this.linkedMapHistory.TryGoToFirst();
            if (nextHistory.HasValue)
            {
                if (!this.ChangeSelectedLinkedMap(nextHistory.Value.MapId, null))
                {
                    this.linkedMapHistory.CancelMove();
                }
            }
        }

        /// <summary>
        /// 현재 보고 있는 위치를 Home으로 지정한다.
        /// </summary>
        public void SetHome()
        {
            if (this.IsConsoleMode)
                return;

            var map = this.baseMap;
            if (map == null)
                return;

            var curExtent = map.Extent;
            if (curExtent == null)
                return;

            this.ClearHome();

            var bookmarkDataInfo = this.publicGraphicDataManager.CreateHomeBookMarkObjectData(this.MaxExtentToPoint, this.MinExtentToPoint);

            this.RaiseObjectAddedEvent(bookmarkDataInfo);

            this.ApplyHomeExtentToMapSettingData(curExtent);
        }

        /// <summary>
        /// Home 옵션을 제거한다.
        /// </summary>
        public void ClearHome()
        {
            if (this.IsConsoleMode)
                return;

            var homes = this.publicGraphicDataManager.GetBookMarkDatas.Where(p => p.IsHome).ToArray();
            foreach (var home in homes)
            {
                this.DeleteMapObjectData(home);
            }

            this.ApplyHomeExtentToMapSettingData(null);
        }

        /// <summary>
        /// 현재 Map의 데이터(설정, Object 등)를 다시 받아올 때 사용한다.
        /// </summary>
        public void Reload()
        {
            this.TurnOffPlayBackMode(true);

            this.ChangeSelectedLinkedMap(this.CurrentMapSettingInfoDataId, this.baseMap.Extent, false);

            this.OnEMapReloaded();
        }

        #endregion //Map

        #region Playback

        public void SetPlaybackPlayTime(string objectId, DateTime playTime)
        {
            if (!this.IsDataPlayBackMode)
            {
                this.cameraOverlayControl.UpdatePlayTime(objectId, playTime);
            }
        }

        public void PausePlayback()
        {
            this.cameraOverlayControl.xIndependentPlaybackOverlayControl.xPlayToggleButton.IsChecked = false;
            this.cameraOverlayControl.xIndependentPlaybackOverlayControl.xRewindToggleButton.IsChecked = false;
        }

        #endregion // Playback

        #region Splunk Trend

        public List<string> GetSplunkTrendValues()
        {
            if (this.savedSplunkObjectDataManager == null) return null;

            var list = new List<string>();

            foreach (var objectID in this.savedSplunkObjectDataManager.GetObjectIDListInExtent(this.baseMap.Extent))
            {
                var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(objectID);
                if (!splunkControl.IsTableControl) continue;

                var checkedValues = splunkControl.TableControl.GetCheckedValues();
                if (checkedValues == null) continue;

                list.AddRange(checkedValues);
            }

            return list;
        }

        public List<string> GetSavedSearchNameListInExtent()
        {
            if (this.savedSplunkObjectDataManager == null)
            {
                return null;
            }

            var searchNameList = new List<string>();
            foreach (var dataInfo in this.savedSplunkObjectDataManager.GetObjectListInExtent(this.baseMap.Extent))
            {
                this.arcGISControlApi.SetMapSplunkData(dataInfo.SplunkBasicInformation);
                if (string.IsNullOrWhiteSpace(dataInfo.SplunkBasicInformation.Name))
                {
                    continue;
                }

                searchNameList.Add(dataInfo.SplunkBasicInformation.Name);
            }

            return searchNameList;
        }

        #endregion // Splunk Trend

        #endregion Methods

    }
}
