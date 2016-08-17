using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.PropertyControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;
using Innotive.SplunkManager.SplunkManager;
using Innotive.SplunkManager.SplunkManager.Data;

namespace ArcGISControl
{
    public partial class ArcGISClientViewer
    {
        #region Fields
        
        private PropertyManager propertyManager;

        private readonly PropertyDataHelper propertyDataHelper = new PropertyDataHelper();

        private Window MemoPropertyWindow;

        private TextPropertyControl memoPropertyControl;

        #endregion Fields

        #region Methods

        private void InitializeMemoPropertyControl()
        {
            this.memoPropertyControl = new TextPropertyControl();

            this.MemoPropertyWindow = new Window()
            {
                Owner = Window.GetWindow(this),
                Visibility = Visibility.Collapsed,
                Topmost = true,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Content = memoPropertyControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowActivated = false
            };

            this.MemoPropertyWindow.MouseLeftButtonDown += MemoPropertyWindow_MouseLeftButtonDown;

            this.HideMemoProperty();
        }

        private void MemoPropertyWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.MemoPropertyWindow.DragMove();
        }

        private void ShowMemoProperty(MapMemoObjectDataInfo aDataInfo)
        {
            if (this.MemoPropertyWindow == null)
            {
                return;
            }

            var viewModel = new TextPropertyControlViewModel() {DataInfo = aDataInfo};
            this.memoPropertyControl.DataContext = viewModel;
            this.MemoPropertyWindow.Visibility = Visibility.Visible;
        }

        private void HideMemoProperty()
        {
            if (this.MemoPropertyWindow == null)
            {
                return;
            }

            this.memoPropertyControl.DataContext = null;
            this.MemoPropertyWindow.Visibility = Visibility.Collapsed;
        }

        public void InitializePropertyWindow()
        {
            if (this.propertyManager == null)
            {
                var window = Window.GetWindow(this);
                var dataServiceHandler = new MapDataServiceHandler(this.mapDataServerIp, this.mapDataServerPort);
                this.propertyManager = new PropertyManager(window, dataServiceHandler);
                this.propertyManager.onApplyLinkZoneSplunkData += PropertyManagerOnApplyLinkZoneSplunkData;
                this.propertyManager.onApplySplunkArgumentData += PropertyManagerOnOnApplySplunkArgumentData;
            }
        }

        /// <summary>
        /// 선택된 Map Object Graphic에 해당되는 속성창 표시
        /// </summary>
        /// <param name="SelectedMapObjectGraphic"></param>
        private void ShowMapObjectGraphicPropertyWindow(MapObjectType SelectedMapObjectGraphic)
        {
            List<MapObjectPropertied> VisibleMapObjectProperties = new List<MapObjectPropertied>();
            foreach (MapObjectPropertied mapObjectPropertied in Enum.GetValues(typeof(MapObjectPropertied)))
            {
                MapControlPropertiedMappingAttribute MappingInfo = mapObjectPropertied.GetMapMapControlPropertiedMappinggAttribute(0);
                if (MappingInfo != null && MappingInfo.IsUse == false && MappingInfo.Objs == null)
                {
                    VisibleMapObjectProperties.Add(mapObjectPropertied);
                }
                else if (MappingInfo != null && MappingInfo.IsUse && MappingInfo.Objs.Contains(SelectedMapObjectGraphic))
                {
                    VisibleMapObjectProperties.Add(mapObjectPropertied);
                }
                else if (MappingInfo != null && MappingInfo.IsUse == false && MappingInfo.Objs.Contains(SelectedMapObjectGraphic) == false)
                {
                    VisibleMapObjectProperties.Add(mapObjectPropertied);
                }
            }

            this.propertyManager.ShowMapObjectGraphicProperty(VisibleMapObjectProperties);
        }

        /// <summary>
        /// Property 선택 될경우 호출
        /// </summary>
        private void SelectObjectProperty()
        {
            if (!this.isEditMode && this.propertyManager == null) return;

            if (this.selectedGraphicList.Count >= 1)
            {
                var graphic = this.selectedGraphicList.Last();
                var type = graphic.Type;

                int count;

                if (type == MapObjectType.SplunkControl || type == MapObjectType.SplunkIcon)
                {
                    count =
                        this.selectedGraphicList.Count(
                            g => g.Type == MapObjectType.SplunkControl || g.Type == MapObjectType.SplunkIcon);
                }
                else
                {
                    count = this.selectedGraphicList.Count(g => g.Type == graphic.Type);
                }

                //Select된 Object의 종류가 다를 경우 Property 창 Hide
                if (count != this.selectedGraphicList.Count)
                {
                    this.propertyManager.DeselectAll();
                    return;
                }


                this.ShowMapObjectGraphicPropertyWindow(type);


                if (ArcGISDataConvertHelper.IsCameraGraphic(type))
                {
                    var cameraObjectComponentDataInfo = this.cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);

                    if (cameraObjectComponentDataInfo == null)
                    {
                        this.propertyManager.DeselectAll();
                        return;
                    }

                    var name = this.selectedGraphicList.Count == 1 ? cameraObjectComponentDataInfo.Name : "";

                    switch (type)
                    {
                        case MapObjectType.CameraPreset:

                            MapCameraPresetObjectDataInfo presetData = MapObjectPropertied.CameraViewZone.GetMapMapControlPropertiedMappinggAttribute(0).
                                CreateCameraObjectDataInfo<MapCameraPresetObjectDataInfo>(selectedGraphicList, cameraGraphicDataManager);

                            var presetList = this.selectedGraphicList.Count == 1 ? cameraObjectComponentDataInfo.PresetList.ToList() : new List<string>();
                            this.propertyManager.SelectCameraViewZone(name, presetData, presetList);
                            presetData.PropertyChanged += CameraPresetObjectComponentDataInfoOnPropertyChanged;


                            /*
                            var presetGraphic = graphic as CameraPresetGraphic;

                            if (presetGraphic != null)
                            {
                                try
                                {
                                    var presetData = new MapCameraPresetObjectDataInfo(cameraObjectComponentDataInfo.PresetDatas[presetGraphic.PresetIndex]);

                                    //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
                                    if (this.selectedGraphicList.Count > 1)
                                    {
                                        var borderColorString =
                                            cameraObjectComponentDataInfo.PresetDatas[presetGraphic.PresetIndex].BorderColorString;

                                        var sameColorCnt = this.selectedGraphicList.OfType<PolygonGraphic>().Count(g => g.BorderColor.ToString() == borderColorString);

                                        if (sameColorCnt != this.selectedGraphicList.Count)
                                        {
                                            borderColorString = borderColorString.Remove(3);
                                            borderColorString += "ffffff";
                                        }

                                        var fillColorString =
                                            cameraObjectComponentDataInfo.PresetDatas[
                                                presetGraphic.PresetIndex].FillColorString;

                                        sameColorCnt = this.selectedGraphicList.OfType<PolygonGraphic>().Count(g => g.NormalColor.ToString() == fillColorString);

                                        if (sameColorCnt != this.selectedGraphicList.Count)
                                        {
                                            fillColorString = fillColorString.Remove(3);
                                            fillColorString += "ffffff";
                                            MessageBox.Show(fillColorString);
                                        }

                                        presetData = new MapCameraPresetObjectDataInfo()
                                        {
                                            BorderColorString = borderColorString,
                                            FillColorString = fillColorString
                                        };
                                    }

                                    var presetList = this.selectedGraphicList.Count == 1
                                                         ? cameraObjectComponentDataInfo.PresetList.ToList()
                                                         : new List<string>();

                                    this.propertyManager.SelectCameraViewZone(name, presetData, presetList);

                                    presetData.PropertyChanged += CameraPresetObjectComponentDataInfoOnPropertyChanged;
                                }
                                catch (ArgumentOutOfRangeException exception)
                                {
                                    InnowatchDebug.Logger.Trace(exception.ToString());
                                }
                            }
                             * */

                            break;

                        case MapObjectType.CameraVideo:

                            MapCameraVideoObjectDataInfo videoData = MapObjectPropertied.CameraVideo.GetMapMapControlPropertiedMappinggAttribute(0).
                                CreateCameraObjectDataInfo<MapCameraVideoObjectDataInfo>(selectedGraphicList, cameraGraphicDataManager);

                            this.propertyManager.SelectCameraVideo(name, videoData);
                            videoData.PropertyChanged += this.CameraVideoObjectComponentDataInfoOnPropertyChanged;




                            /*
                            var videoData = new MapCameraVideoObjectDataInfo(cameraObjectComponentDataInfo.Video);

                            //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
                            if (this.selectedGraphicList.Count > 1)
                            {
                                var isLockSize = cameraObjectComponentDataInfo.Video.IsLockSize;
                                var constrainProportion = cameraObjectComponentDataInfo.Video.ConstrainProportion;
                                var hiddenMinLevel = cameraObjectComponentDataInfo.Video.HiddenMinLevel;
                                var hiddenMaxLevel = cameraObjectComponentDataInfo.Video.HiddenMaxLevel;

                                var isLockSizeCnt = 0;
                                var constrainProportionCnt = 0;
                                var hiddenMinLevelCnt = 0;
                                var hiddenMaxLevelCnt = 0;

                                foreach (var selectedGraphic in this.selectedGraphicList)
                                {
                                    var selectedCameraObjectComponent
                                        = this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID)
                                                            as MapCameraObjectComponentDataInfo;

                                    if (selectedCameraObjectComponent == null) continue;

                                    isLockSizeCnt = selectedCameraObjectComponent.Video.IsLockSize == isLockSize
                                                  ? isLockSizeCnt + 1
                                                  : isLockSizeCnt;

                                    constrainProportionCnt = selectedCameraObjectComponent.Video.ConstrainProportion == constrainProportion
                                                  ? constrainProportionCnt + 1
                                                  : constrainProportionCnt;

                                    hiddenMinLevelCnt = selectedCameraObjectComponent.Video.HiddenMinLevel == hiddenMinLevel
                                                  ? hiddenMinLevelCnt + 1
                                                  : hiddenMinLevelCnt;

                                    hiddenMaxLevelCnt = selectedCameraObjectComponent.Video.HiddenMaxLevel == hiddenMaxLevel
                                                  ? hiddenMaxLevelCnt + 1
                                                  : hiddenMaxLevelCnt;
                                }

                                if (isLockSizeCnt != selectedGraphicList.Count) isLockSize = null;
                                if (constrainProportionCnt != selectedGraphicList.Count) constrainProportion = null;
                                if (hiddenMinLevelCnt != selectedGraphicList.Count) hiddenMinLevel = -1;
                                if (hiddenMaxLevelCnt != selectedGraphicList.Count) hiddenMaxLevel = -1;

                                videoData = new MapCameraVideoObjectDataInfo()
                                {
                                    IsLockSize = isLockSize,
                                    ConstrainProportion = constrainProportion,
                                    HiddenMaxLevel = hiddenMaxLevel,
                                    HiddenMinLevel = hiddenMinLevel
                                };
                            }

                            this.propertyManager.SelectCameraVideo(name, videoData);

                            videoData.PropertyChanged += this.CameraVideoObjectComponentDataInfoOnPropertyChanged;
                             * */

                            break;

                        case MapObjectType.CameraIcon:
                            MapCameraIconObjectDataInfo cameraIconData = MapObjectPropertied.CameraIcon.GetMapMapControlPropertiedMappinggAttribute(0).
                                CreateCameraObjectDataInfo<MapCameraIconObjectDataInfo>(selectedGraphicList, cameraGraphicDataManager);

                            this.propertyManager.SelectCameraIcon(name, cameraIconData);
                            cameraIconData.PropertyChanged += CameraIconObjectComponentDataInfoOnPropertyChanged;



                            /*
                            var cameraIconData =
                                new MapCameraIconObjectDataInfo(cameraObjectComponentDataInfo.CameraIcon);

                            //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
                            if (this.selectedGraphicList.Count > 1)
                            {
                                var isVisibleLabel = cameraObjectComponentDataInfo.CameraIcon.IsVisibleLabel;
                                var iconSize = cameraObjectComponentDataInfo.CameraIcon.IconSize;
                                var isIconVisible = cameraObjectComponentDataInfo.CameraIcon.IsIconVisible;

                                var labelCnt = 0;
                                var iconSizeCnt = 0;
                                var isIconVisibleCnt = 0;

                                foreach (var selectedGraphic in this.selectedGraphicList)
                                {
                                    var selectedCameraObjectComponent =
                                        this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                                    if (selectedCameraObjectComponent != null)
                                    {
                                        labelCnt = selectedCameraObjectComponent.CameraIcon.IsVisibleLabel == isVisibleLabel
                                                      ? labelCnt + 1
                                                      : labelCnt;

                                        iconSizeCnt = selectedCameraObjectComponent.CameraIcon.IconSize == iconSize
                                                      ? iconSizeCnt + 1
                                                      : iconSizeCnt;

                                        isIconVisibleCnt = selectedCameraObjectComponent.CameraIcon.IsIconVisible == isIconVisible
                                                      ? isIconVisibleCnt + 1
                                                      : isIconVisibleCnt;
                                    }
                                }

                                if (iconSizeCnt != selectedGraphicList.Count) iconSize = ArcGISConstSet.CameraIconSize;
                                if (labelCnt != selectedGraphicList.Count) isVisibleLabel = null;
                                if (isIconVisibleCnt != selectedGraphicList.Count) isIconVisible = null;

                                cameraIconData = new MapCameraIconObjectDataInfo()
                                {
                                    IsVisibleLabel = isVisibleLabel,
                                    IconSize = iconSize,
                                    IsIconVisible =  isIconVisible
                                };
                            }


                            this.propertyManager.SelectCameraIcon(name, cameraIconData);

                            cameraIconData.PropertyChanged += CameraIconObjectComponentDataInfoOnPropertyChanged;
                            */
                            break;
                    }

                    this.propertyManager.MapCurrentZoomLevel = this.mapLevel;
                    this.propertyManager.MapResolution = this.baseMap.Resolution;
                }
                else if (type == MapObjectType.SplunkIcon || type == MapObjectType.SplunkControl)
                {
                    MapSplunkObjectDataInfo SplunkIconData = MapObjectPropertied.Splunk.GetMapMapControlPropertiedMappinggAttribute(0).
                                SetSplunkProperyValue(selectedGraphicList, savedSplunkObjectDataManager);

                    var allChart = !SplunkBasicInformationData.IsTableDataType(SplunkIconData.SplunkBasicInformation.DataExpressType);
                    this.propertyManager.SelectSplunk(SplunkIconData, allChart);
                    SplunkIconData.PropertyChanged += SplunkObjectDataInfoOnPropertyChanged;

                    this.propertyManager.MapCurrentZoomLevel = this.mapLevel;
                    this.propertyManager.MapResolution = this.baseMap.Resolution;


                    /*
                    var propertyData = this.savedSplunkObjectDataManager.GetObjectDataByObjectID(graphic.ObjectID);

                    if (propertyData == null)
                    {
                        this.propertyManager.DeselectAll();
                        return;
                    }

                    var splunkDataInfo = new MapSplunkObjectDataInfo(propertyData);

                    var allChart = !SplunkBasicInformationData.IsTableDataType(splunkDataInfo.SplunkBasicInformation.DataExpressType);

                    //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
                    if (this.selectedGraphicList.Count > 1)
                    {
                        var hiddenMinLevel = splunkDataInfo.HiddenMinLevel;
                        var hiddenMaxLevel = splunkDataInfo.HiddenMaxLevel;
                        var title = splunkDataInfo.Title;
                        var axisX = splunkDataInfo.ChartAxisXTitle;
                        var axisY = splunkDataInfo.ChartAxisYTitle;
                        var useSchedule = splunkDataInfo.UseSchedule;
                        var intervalSeconds = splunkDataInfo.IntervalSeconds;
                        var intervalUnitType = splunkDataInfo.IntervalUnitType;
                        var isIconHidden = splunkDataInfo.IsIconHidden;
                        var linkUrl = splunkDataInfo.LinkUrl;

                        var hiddenMinLevelCnt = 0;
                        var hiddenMaxLevelCnt = 0;
                        var titlesAreAllSame = true;
                        var axisXAreAllSame = true;
                        var axisYAreAllSame = true;
                        var useScheduleSame = true;
                        var intervalSecondsSame = true;
                        var intervalUnitTypeSame = true;
                        var isIconHiddenSame = true;
                        var linkUrlSame = true;
                        
                        foreach (var selectedGraphic in this.selectedGraphicList)
                        {
                            var selectedSplunkObject
                                = this.savedSplunkObjectDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID);

                            if (selectedSplunkObject == null) continue;

                            if (allChart && SplunkBasicInformationData.IsTableDataType(selectedSplunkObject.SplunkBasicInformation.DataExpressType))
                            {
                                allChart = false;
                            }

                            hiddenMinLevelCnt = selectedSplunkObject.HiddenMinLevel == hiddenMinLevel
                                          ? hiddenMinLevelCnt + 1
                                          : hiddenMinLevelCnt;

                            hiddenMaxLevelCnt = selectedSplunkObject.HiddenMaxLevel == hiddenMaxLevel
                                          ? hiddenMaxLevelCnt + 1
                                          : hiddenMaxLevelCnt;

                            if (titlesAreAllSame && selectedSplunkObject.Title != title)
                                titlesAreAllSame = false;

                            if (axisXAreAllSame && selectedSplunkObject.ChartAxisXTitle != axisX)
                                axisXAreAllSame = false;

                            if (axisYAreAllSame && selectedSplunkObject.ChartAxisYTitle != axisY)
                                axisYAreAllSame = false;

                            if (useScheduleSame && useSchedule != selectedSplunkObject.UseSchedule)
                            {
                                 useScheduleSame = false;
                            }

                            if (intervalUnitTypeSame && intervalUnitType != selectedSplunkObject.IntervalUnitType)
                            {
                                intervalUnitTypeSame = false;
                                intervalSecondsSame = false;
                            }

                            if (intervalSecondsSame && intervalSeconds != selectedSplunkObject.IntervalSeconds)
                            {
                                intervalSecondsSame = false;
                            }    

                            if (linkUrlSame && linkUrl.Equals(selectedSplunkObject.LinkUrl))
                            {
                                linkUrlSame = false;
                            } 

                            if(isIconHiddenSame && isIconHidden != selectedSplunkObject.IsIconHidden)
                            {
                                isIconHiddenSame = false;
                            }
                        }

                        if (hiddenMinLevelCnt != selectedGraphicList.Count) hiddenMinLevel = -1;
                        if (hiddenMaxLevelCnt != selectedGraphicList.Count) hiddenMaxLevel = -1;
                        if (!titlesAreAllSame) title = "-";
                        if (!axisXAreAllSame) axisX = "-";
                        if (!axisYAreAllSame) axisY = "-";
                        if(!useScheduleSame) useSchedule = null;
                        if(!intervalUnitTypeSame) intervalUnitType = IntervalUnitType.None;
                        if(!intervalSecondsSame) intervalSeconds = null;
                        if (!isIconHiddenSame) isIconHidden = null;
                        if (!linkUrlSame) linkUrl = string.Empty;

                        splunkDataInfo = new MapSplunkObjectDataInfo()
                        {
                            Name = "",
                            Title = title,
                            ChartAxisXTitle = axisX,
                            ChartAxisYTitle = axisY,
                            HiddenMaxLevel = hiddenMaxLevel,
                            HiddenMinLevel = hiddenMinLevel,
                            UseSchedule = useSchedule,
                            IntervalUnitType = intervalUnitType,
                            IntervalSeconds = intervalSeconds,
                            IsIconHidden = isIconHidden,
                            LinkUrl = linkUrl
                        };
                    }

                    this.propertyManager.SelectSplunk(splunkDataInfo, allChart);

                    splunkDataInfo.PropertyChanged += SplunkObjectDataInfoOnPropertyChanged;

                    this.propertyManager.MapCurrentZoomLevel = this.mapLevel;
                    this.propertyManager.MapResolution = this.baseMap.Resolution;
                     * */
                }
                else if (type == MapObjectType.UniversalControl || type == MapObjectType.UniversalIcon)
                {
                    var propertyData = universalObjectDataManager.GetDataInfo(graphic.ObjectID);

                    if (propertyData is MapUniversalObjectDataInfo)
                    {
                        MapUniversalObjectDataInfo universalObjectDataInfo;
                        
                        if (this.selectedGraphicList.Count > 1)
                        {
                            universalObjectDataInfo = new MapUniversalObjectDataInfo();
                        }
                        else
                        {
                            universalObjectDataInfo = new MapUniversalObjectDataInfo(propertyData as MapUniversalObjectDataInfo);
                        }

                        List<MapSettingDataInfo> mapList = null;
                        if (this.arcGISControlApi != null)
                        {
                            mapList = this.arcGISControlApi.GetMapList();

                            if (mapList != null && mapList.Count > 0)
                            {
                                var currentMap =
                                     mapList.FirstOrDefault(data => data.ID == this.mapSettingDataInfo.ID);

                                if (currentMap != null) mapList.Remove(currentMap);
                            }
                        }

                        this.propertyManager.SelectUniversalObject(universalObjectDataInfo, mapList, this.selectedGraphicList.Count == 1);

                        universalObjectDataInfo.PropertyChanged += UniversalObjectDataInfoOnPropertyChanged;
                    }
                }
                else
                {
                    object propertyData = this.GetObjectData(graphic.ObjectID, graphic.Type);

                    if (propertyData == null)
                    {
                        this.propertyManager.DeselectAll();
                        return;
                    }

                    if (propertyData is MapLinkZoneObjectDataInfo)
                    {
                        List<MapSettingDataInfo> mapList = null;
                        List<SplunkBasicInformationData> mapSplunkList = null;

                        var linkZoneObject = propertyData as MapLinkZoneObjectDataInfo;

                        if (this.selectedGraphicList.Count > 1)
                        {
                            var borderColorString = linkZoneObject.BorderColorString;

                            var cnt = this.selectedGraphicList.OfType<LinkZoneGraphic>().Count(g => g.BorderColor.ToString() == borderColorString);

                            if (cnt != this.selectedGraphicList.Count)
                            {
                                borderColorString = borderColorString.Remove(3);
                                borderColorString += "ffffff";
                            }

                            var fillColorString = linkZoneObject.FillColorString;

                            cnt = this.selectedGraphicList.OfType<LinkZoneGraphic>().Count(g => g.NormalColor.ToString() == fillColorString);

                            if (cnt != this.selectedGraphicList.Count)
                            {
                                fillColorString = fillColorString.Remove(3);
                                fillColorString += "ffffff";
                            }

                            linkZoneObject = new MapLinkZoneObjectDataInfo()
                            {
                                ObjectType = type,
                                BorderColorString = borderColorString,
                                FillColorString = fillColorString,
                                ImageObjectData = new CommonImageObjectDataInfo(linkZoneObject.ImageObjectData)
                            };
                        }
                        else
                        {
                            if (this.arcGISControlApi != null)
                            {
                                mapList = this.arcGISControlApi.GetMapList();

                                if (mapList != null && mapList.Count > 0)
                                {
                                    var currentMap =
                                         mapList.FirstOrDefault(data => data.ID == this.mapSettingDataInfo.ID);

                                    if (currentMap != null) mapList.Remove(currentMap);

                                    mapList.Insert(0, new MapSettingDataInfo() { Name = "None" });
                                }
                            }

                            if (this.arcGISControlApi != null && this.isEditMode)
                            {
                                mapSplunkList = this.arcGISControlApi.GetSplunkInformationDataList();

                                if (mapSplunkList != null && mapSplunkList.Count > 0)
                                {
                                    mapSplunkList.Insert(0, new SplunkBasicInformationData()
                                    {
                                        Name = "None"
                                    });
                                }
                            }

                            linkZoneObject = new MapLinkZoneObjectDataInfo(linkZoneObject);
                        }

                        if (linkZoneObject.ObjectType == MapObjectType.ImageLinkZone)
                        {  
                            linkZoneObject.ImageObjectData = this.propertyDataHelper.GetCommonData(linkZoneObject.ImageObjectData,
                                                                                  this.selectedGraphicList);

                            linkZoneObject.ImageObjectData.PropertyChanged += ImageObjectDataOnPropertyChanged;

                            linkZoneObject.ImageObjectData.ImageOpacity = linkZoneObject.ImageObjectData.ImageOpacity;
                        }

                        this.propertyManager.SelectLinkZone(linkZoneObject, mapList, mapSplunkList, this.useSplunk, this.selectedGraphicList.Count == 1);

                        linkZoneObject.PropertyChanged += OnLocationDataPropertyChanged;

                    }
                    else if (propertyData is MapLocationObjectDataInfo)
                    {
                        var locationData = new MapLocationObjectDataInfo(propertyData as MapLocationObjectDataInfo);

                        this.propertyManager.SelectPlace(locationData, true);

                        locationData.PropertyChanged += OnLocationDataPropertyChanged;
                    }
                    else if (propertyData is MapWorkStationObjectDataInfo)
                    {
                        List<MapSettingDataInfo> mapList = null;
                        List<SplunkBasicInformationData> mapSplunkList = null;

                        if (this.selectedGraphicList.Count > 1)
                        {
                            var borderColorString = (propertyData as MapWorkStationObjectDataInfo).BorderColorString;

                            var cnt = this.selectedGraphicList.OfType<LinkZoneGraphic>().Count(g => g.BorderColor.ToString() == borderColorString);

                            if (cnt != this.selectedGraphicList.Count)
                            {
                                borderColorString = borderColorString.Remove(3);
                                borderColorString += "ffffff";
                            }

                            var fillColorString = (propertyData as MapWorkStationObjectDataInfo).FillColorString;

                            cnt = this.selectedGraphicList.OfType<LinkZoneGraphic>().Count(g => g.NormalColor.ToString() == fillColorString);

                            if (cnt != this.selectedGraphicList.Count)
                            {
                                fillColorString = fillColorString.Remove(3);
                                fillColorString += "ffffff";
                            }

                            propertyData = new MapWorkStationObjectDataInfo()
                            {
                                BorderColorString = borderColorString,
                                FillColorString = fillColorString
                            };
                        }
                        else
                        {
                            if (this.arcGISControlApi != null)
                            {
                                mapList = this.arcGISControlApi.GetMapList();

                                if (mapList != null && mapList.Count > 0)
                                {
                                    var currentMap =
                                    mapList.FirstOrDefault(data => data.ID == this.mapSettingDataInfo.ID);
                                    if (currentMap != null) mapList.Remove(currentMap);

                                    mapList.Insert(0, new MapSettingDataInfo() { Name = "None" });
                                }
                            }

                            if (this.arcGISControlApi != null && this.isEditMode)
                            {
                                mapSplunkList = this.arcGISControlApi.GetSplunkInformationDataList();

                                if (mapSplunkList != null && mapSplunkList.Count > 0)
                                {
                                    mapSplunkList.Insert(0, new SplunkBasicInformationData()
                                    {
                                        Name = "None"
                                    });
                                }
                            }

                            propertyData = new MapWorkStationObjectDataInfo(propertyData as MapWorkStationObjectDataInfo);
                        }

                        this.propertyManager.SelectWorkStation(propertyData as MapWorkStationObjectDataInfo, mapList, mapSplunkList);

                        (propertyData as MapWorkStationObjectDataInfo).PropertyChanged += WorkStationObjectDataInfoOnPropertyChanged;
                    }
                    else if (propertyData is MapTextObjectDataInfo)
                    {
                        var textDataInfo = new MapTextObjectDataInfo(propertyData as MapTextObjectDataInfo);

                        textDataInfo = this.propertyDataHelper.GetCommonData(textDataInfo, this.selectedGraphicList);

                        this.propertyManager.SelectText(textDataInfo);

                        textDataInfo.PropertyChanged += TextDataInfoOnPropertyChanged;
                    }
                    else if(propertyData is MapLineObjectDataInfo)
                    {
                        var lineDataInfo = new MapLineObjectDataInfo(propertyData as MapLineObjectDataInfo);

                        lineDataInfo = this.propertyDataHelper.GetCommonData(lineDataInfo, this.selectedGraphicList);

                        this.propertyManager.SelectLine(lineDataInfo, this.selectedGraphicList.Count == 1);

                        lineDataInfo.PropertyChanged += LineDataInfoOnPropertyChanged;
                    }
                    else if(propertyData is MapImageObjectDataInfo)
                    {
                        var imageDataInfo = new MapImageObjectDataInfo(propertyData as MapImageObjectDataInfo);

                        imageDataInfo.ImageObjectData = this.propertyDataHelper.GetCommonData(imageDataInfo.ImageObjectData, this.selectedGraphicList);

                        imageDataInfo.ImageObjectData.PropertyChanged += ImageObjectDataOnPropertyChanged;

                        this.propertyManager.SelectImage(imageDataInfo, this.selectedGraphicList.Count == 1);

                        imageDataInfo.PropertyChanged += ImageDataInfoOnPropertyChanged;
                    }
                }
            }
            else
            {
                this.propertyManager.DeselectAll();
                //this.DeleteLinkZoneVertexGraphic();
            }
        }

        /// <summary>
        /// Map의 Level이 변경 될때 마다 호출 된다.
        /// Property에 Map의 Level을 보내 준다 
        /// 
        /// </summary>
        private void ChangedZoomLevel()
        {
            if (this.propertyManager != null)
            {
                this.propertyManager.MapCurrentZoomLevel = this.mapLevel;
                this.propertyManager.MapResolution = this.baseMap.Resolution;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <returns></returns>
        private List<string> GetSplunkArguments(string objectID, SplunkBasicInformationData splunkBasicInformation)
        {
            var arguments = new List<string>();

            if (string.IsNullOrEmpty(splunkBasicInformation.Name) ||
                splunkBasicInformation.Name.ToLower() == "none" ||
                string.IsNullOrEmpty(splunkBasicInformation.IP) ||
                string.IsNullOrEmpty(splunkBasicInformation.App) ||
                string.IsNullOrEmpty(splunkBasicInformation.Password) ||
                string.IsNullOrEmpty(splunkBasicInformation.UserId) ||
                splunkBasicInformation.Port == 0)
                return arguments;

            try
            {
                var args = new SplunkServiceFactoryArgs
                {
                    Host = splunkBasicInformation.IP,
                    Port = splunkBasicInformation.Port,
                    App = splunkBasicInformation.App,
                    UserName = splunkBasicInformation.UserId,
                    UserPwd = splunkBasicInformation.Password
                };

                arguments = SplunkServiceFactory.Instance.GetSplunkService(args).GetSplunkSavedSearchArguments(splunkBasicInformation.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return arguments;
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// LinkZone에서 OnApply 클릭시 Data 적용
        /// </summary>
        /// <param name="data"></param>
        /// <param name="setspltype"></param>
        private void PropertyManagerOnApplyLinkZoneSplunkData(MapLinkZoneObjectDataInfo data, SETSPLTYPE setspltype)
        {
            if (setspltype == SETSPLTYPE.color)
            {
                var baseGraphic = this.GetOneBaseGraphicInGraphicLayer(data.ObjectID, data.ObjectType);
                var linkZoneGraphic = baseGraphic as LinkZoneGraphic;

                if (linkZoneGraphic != null)
                {
                    linkZoneGraphic.ChangeOriginalColor();
                }

                this.publicGraphicDataManager.StartSplunkService(data.ObjectID, data.ColorSplunkBasicInformationData, !this.isEditMode);
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Splunk Onapply 클릭시 Data적용
        /// </summary>
        /// <param name="data"></param>
        private void PropertyManagerOnOnApplySplunkArgumentData(MapSplunkObjectDataInfo data)
        {
            var basicData = this.savedSplunkObjectDataManager.GetObjectDataByObjectID(data.ObjectID);

            var splunkData = basicData as MapSplunkObjectDataInfo;

            if (splunkData == null) return;

            splunkData.SplunkBasicInformation.SplArgumentKeys = new ObservableCollection<string>(data.SplunkBasicInformation.SplArgumentKeys);
            splunkData.SplunkBasicInformation.SplArgumentValues = new ObservableCollection<string>(data.SplunkBasicInformation.SplArgumentValues);

            //서비스를 내려야 다시 시작 할수 있다.
            this.savedSplunkObjectDataManager.StopSplunkService(splunkData.ObjectID, splunkData.SplunkBasicInformation);

            this.ChangeSplunkServiceStatus(splunkData);

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Camera Icon Property Change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void CameraIconObjectComponentDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if ((sender as MapCameraIconObjectDataInfo) == null) return;

            foreach (var selectedGraphic in this.selectedGraphicList.ToList())
            {
                var selectedCameraIcongGraphic = selectedGraphic as CameraIconGraphic;
                var selectedCameraObjectComponent = this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                if (selectedCameraIcongGraphic == null || selectedCameraObjectComponent == null)
                    continue;

                var selectedCameraIconData = selectedCameraObjectComponent.CameraIcon;

                if (selectedCameraIconData == null)
                    continue;

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("iconsize"))
                {
                    selectedCameraIconData.IconSize = (Decimal)(sender as MapCameraIconObjectDataInfo).IconSize;

                    selectedCameraIcongGraphic.ChangeIconSize((double)selectedCameraIconData.IconSize);

                    var currentImageSize = new Size(26, 26);
                    var x = selectedCameraIcongGraphic.OffsetPoint.X - (selectedCameraIcongGraphic as CameraIconGraphic).ImagePixelSize.Width;
                    var y = selectedCameraIcongGraphic.OffsetPoint.Y + (currentImageSize.Height);

                    selectedCameraIcongGraphic.ViwzonePlusButtonIcon.ChangeOffsetPoint(new Point(x, y));
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("labelsize"))
                {
                    selectedCameraIconData.LabelSize = (sender as MapCameraIconObjectDataInfo).LabelSize;
                    selectedCameraIcongGraphic.CameraNameTextBoxGraphic.SetLabelSize((double)selectedCameraIconData.LabelSize);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("isvisiblelabel"))
                {
                    selectedCameraIconData.IsVisibleLabel = (sender as MapCameraIconObjectDataInfo).IsVisibleLabel;

                    var labelGraphic = selectedCameraIcongGraphic.CameraNameTextBoxGraphic;

                    //Show Label
                    if (selectedCameraIconData.IsVisibleLabel != null && selectedCameraIconData.IsVisibleLabel.Value)
                    {
                        if (!this.objectGraphicLayer.Graphics.Contains(labelGraphic))
                        {
                            this.objectGraphicLayer.Graphics.Add(labelGraphic);
                        }
                    }
                    //Hide Label
                    else
                    {
                        if (this.objectGraphicLayer.Graphics.Contains(labelGraphic))
                        {
                            this.objectGraphicLayer.Graphics.Remove(labelGraphic);
                        }
                    }
                }

                if (string.CompareOrdinal(propertyChangedEventArgs.PropertyName , "IsIconVisible") == 0)
                {
                    selectedCameraIconData.IsIconVisible = (sender as MapCameraIconObjectDataInfo).IsIconVisible;

                    if (selectedCameraIconData.IsIconVisible != null && selectedCameraIconData.IsIconVisible.Value == false)
                    {
                        selectedCameraIconData.IconSelectedStringUri = ArcGISConstSet.CameraIconHideUri;
                        selectedCameraIconData.IconUri = ArcGISConstSet.CameraIconHideUri;
                    }
                    else
                    {
                        selectedCameraIconData.IconSelectedStringUri = ArcGISConstSet.CameraIconSelectedUri;
                        selectedCameraIconData.IconUri = ArcGISConstSet.CameraIconNormalUri;
                    }

                    selectedCameraIcongGraphic.IconNormalUriString = selectedCameraIconData.IconUri;
                    selectedCameraIcongGraphic.IconSelectedrUriString = selectedCameraIconData.IconSelectedStringUri;
                    selectedCameraIcongGraphic.ChangeSymbolImage(selectedCameraIconData.IconSelectedStringUri);
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void CameraVideoObjectComponentDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!(sender is MapCameraVideoObjectDataInfo) || this.selectedGraphicList.Count == 0) return;

            foreach (var selectedGraphic in this.selectedGraphicList.ToList())
            {
                var selectedVideoGraphic = selectedGraphic as CameraVideoGraphic;
                var selectedCameraObjectComponent = this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                if (selectedCameraObjectComponent == null || selectedVideoGraphic == null) continue;

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("islocksize") || propertyChangedEventArgs.PropertyName.ToLower().Contains("scalemaxlevel"))
                {
                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("islocksize"))
                    {
                        selectedCameraObjectComponent.Video.IsLockSize =
                            (sender as MapCameraVideoObjectDataInfo).IsLockSize;

                        selectedCameraObjectComponent.Video.LockedResoultion = this.mapLevel;
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("scalemaxlevel"))
                        selectedCameraObjectComponent.Video.ScaleMaxLevel = (sender as MapCameraVideoObjectDataInfo).ScaleMaxLevel;

                    var movePointCollection = new List<Point>();
                    movePointCollection.Clear();
                    movePointCollection.Add(new Point(selectedVideoGraphic.Geometry.Extent.XMin, selectedVideoGraphic.Geometry.Extent.YMax));
                    movePointCollection.Add(new Point(selectedVideoGraphic.Geometry.Extent.XMin, selectedVideoGraphic.Geometry.Extent.YMin));
                    movePointCollection.Add(new Point(selectedVideoGraphic.Geometry.Extent.XMax, selectedVideoGraphic.Geometry.Extent.YMin));
                    movePointCollection.Add(new Point(selectedVideoGraphic.Geometry.Extent.XMax, selectedVideoGraphic.Geometry.Extent.YMax));

                    this.SetObjectPosition(selectedVideoGraphic, movePointCollection);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("alwayskeeptocameravideo"))
                {
                    selectedCameraObjectComponent.Video.AlwaysKeepToCameraVideo =
                            (sender as MapCameraVideoObjectDataInfo).AlwaysKeepToCameraVideo;

                    selectedVideoGraphic.AlwaysKeepToCameraVideo = selectedCameraObjectComponent.Video.AlwaysKeepToCameraVideo.Value;
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("hiddenmaxlevel"))
                {
                    selectedCameraObjectComponent.Video.HiddenMaxLevel =
                                (sender as MapCameraVideoObjectDataInfo).HiddenMaxLevel;

                    this.ChangeCameraVideoVisible(selectedCameraObjectComponent, selectedVideoGraphic);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("hiddenminlevel"))
                {
                    selectedCameraObjectComponent.Video.HiddenMinLevel =
                            (sender as MapCameraVideoObjectDataInfo).HiddenMinLevel;

                    this.ChangeCameraVideoVisible(selectedCameraObjectComponent, selectedVideoGraphic);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("constrainproportion"))
                {
                    selectedCameraObjectComponent.Video.ConstrainProportion =
                            (sender as MapCameraVideoObjectDataInfo).ConstrainProportion;
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void CameraPresetObjectComponentDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!(sender is MapCameraPresetObjectDataInfo)) return;

            foreach (var selectedGraphic in this.selectedGraphicList.ToList())
            {
                var presetGraphic = selectedGraphic as CameraPresetGraphic;
                var selectedCameraObjectComponent = this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                if (presetGraphic == null || selectedCameraObjectComponent == null) continue;

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("fillcolorstring"))
                {
                    selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].FillColorString =
                       (sender as MapCameraPresetObjectDataInfo).FillColorString;

                    presetGraphic.ChangeColors(selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].FillColor, selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].BorderColor);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("bordercolorstring"))
                {
                    selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].BorderColorString =
                      (sender as MapCameraPresetObjectDataInfo).BorderColorString;

                    presetGraphic.ChangeColors(selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].FillColor, selectedCameraObjectComponent.PresetDatas[presetGraphic.PresetIndex].BorderColor);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("presetnum"))
                {
                    selectedCameraObjectComponent.PresetDatas[(presetGraphic as CameraPresetGraphic).PresetIndex].PresetNum =
                            (sender as MapCameraPresetObjectDataInfo).PresetNum;
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void SplunkObjectDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!(sender is MapSplunkObjectDataInfo) || this.selectedGraphicList.Count == 0) return;

            foreach (var selectedGraphic in this.selectedGraphicList.ToList())
            {
                // Icon에 관계 없이 값이 변경되면 적용되도록 수정
                var selectedSplunkObject = this.savedSplunkObjectDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapSplunkObjectDataInfo;
                if (selectedSplunkObject == null) continue;

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("hiddenmaxlevel"))
                {
                    selectedSplunkObject.HiddenMaxLevel =
                                (sender as MapSplunkObjectDataInfo).HiddenMaxLevel;

                    this.ChangeSplunkControlVisible(selectedSplunkObject);
                }

                if (propertyChangedEventArgs.PropertyName.ToLower().Contains("hiddenminlevel"))
                {
                    selectedSplunkObject.HiddenMinLevel =
                            (sender as MapSplunkObjectDataInfo).HiddenMinLevel;

                    this.ChangeSplunkControlVisible(selectedSplunkObject);
                }

                if (propertyChangedEventArgs.PropertyName == "Title")
                {
                    selectedSplunkObject.Title =
                            (sender as MapSplunkObjectDataInfo).Title;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.SetTitle(selectedSplunkObject.Title);
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "ChartAxisXTitle")
                {
                    selectedSplunkObject.ChartAxisXTitle =
                            (sender as MapSplunkObjectDataInfo).ChartAxisXTitle;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        var axisTitle = selectedSplunkObject.ChartAxisXTitle;
                        if (String.IsNullOrWhiteSpace(axisTitle) && selectedSplunkObject.SplunkBasicInformation != null)
                            axisTitle = selectedSplunkObject.SplunkBasicInformation.XAxisTitle;
                        selectedSplunkControl.ChartControl.ChartAxisXTitle = axisTitle;
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "ChartAxisYTitle")
                {
                    selectedSplunkObject.ChartAxisYTitle =
                            (sender as MapSplunkObjectDataInfo).ChartAxisYTitle;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        var axisTitle = selectedSplunkObject.ChartAxisYTitle;
                        if (String.IsNullOrWhiteSpace(axisTitle) && selectedSplunkObject.SplunkBasicInformation != null)
                            axisTitle = selectedSplunkObject.SplunkBasicInformation.YAxisTitle;
                        selectedSplunkControl.ChartControl.ChartAxisYTitle = axisTitle;
                    }
                }

                if(propertyChangedEventArgs.PropertyName == "UseSchedule")
                {
                    selectedSplunkObject.UseSchedule =
                            (sender as MapSplunkObjectDataInfo).UseSchedule;
                }

                if (propertyChangedEventArgs.PropertyName == "IntervalSeconds")
                {
                    selectedSplunkObject.IntervalSeconds =
                            (sender as MapSplunkObjectDataInfo).IntervalSeconds;
                }

                if (propertyChangedEventArgs.PropertyName == "IntervalUnitType")
                {
                    selectedSplunkObject.IntervalUnitType =
                            (sender as MapSplunkObjectDataInfo).IntervalUnitType;
                }

                if (propertyChangedEventArgs.PropertyName == "ShowXAxis")
                {
                    selectedSplunkObject.ShowXAxis =
                            (sender as MapSplunkObjectDataInfo).ShowXAxis;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.ChartControl.ShowXAxis = selectedSplunkObject.ShowXAxis;
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "LinkUrl")
                {
                    selectedSplunkObject.LinkUrl = (sender as MapSplunkObjectDataInfo).LinkUrl;
                }

                if (propertyChangedEventArgs.PropertyName == "YAxisRangeMin" || propertyChangedEventArgs.PropertyName == "YAxisRangeMax")
                {
                    selectedSplunkObject.YAxisRangeMin =
                            (sender as MapSplunkObjectDataInfo).YAxisRangeMin;
                    selectedSplunkObject.YAxisRangeMax =
                            (sender as MapSplunkObjectDataInfo).YAxisRangeMax;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.ChartControl.SetYAxisRange(
                            selectedSplunkObject.YAxisRangeMin, selectedSplunkObject.YAxisRangeMax);
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "ChartDateTimeFormat")
                {
                    selectedSplunkObject.ChartDateTimeFormat =
                            (sender as MapSplunkObjectDataInfo).ChartDateTimeFormat;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.ChartControl.DateTimeFormat = selectedSplunkObject.ChartDateTimeFormat;
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "ChartLegendFontSize")
                {
                    selectedSplunkObject.ChartLegendFontSize =
                            (sender as MapSplunkObjectDataInfo).ChartLegendFontSize;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.ChartControl.LegendFontSize = selectedSplunkObject.GetLegendFontSizeCombinedWithDefaultSetting();
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "ChartLegendSize")
                {
                    selectedSplunkObject.ChartLegendSize =
                            (sender as MapSplunkObjectDataInfo).ChartLegendSize;

                    var selectedSplunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);
                    if (selectedSplunkControl != null)
                    {
                        selectedSplunkControl.ChartControl.LegendProportion = selectedSplunkObject.GetLegendSizeCombinedWithDefaultSetting();
                    }
                }

                if (propertyChangedEventArgs.PropertyName == "IsIconHidden")
                {
                    var isIconHidden = (sender as MapSplunkObjectDataInfo).IsIconHidden;

                    selectedSplunkObject.IsIconHidden = isIconHidden;

                    var splunkIcon = this.GetOneBaseGraphicInGraphicLayer(selectedGraphic.ObjectID, MapObjectType.SplunkIcon) as SplunkIconGraphic;
                    if (splunkIcon != null)
                        splunkIcon.IsVisible = isIconHidden == false;
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Location Property가 변경 될때 마다 Graphic도 변경 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void OnLocationDataPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (sender is MapLinkZoneObjectDataInfo)
            {
                var propertyData = sender as MapLinkZoneObjectDataInfo;

                foreach (var selectedGraphic in this.selectedGraphicList.ToList())
                {
                    var objectData = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type);
                    var selectedObjectData = objectData as MapLinkZoneObjectDataInfo;
                    if (selectedObjectData == null) continue;

                    if(selectedGraphic.Type == MapObjectType.LinkZone)
                    {   
                        var selectedMapLinkZoneGraphic = selectedGraphic as LinkZoneGraphic;

                        if (selectedMapLinkZoneGraphic == null) continue;

                        if (propertyChangedEventArgs.PropertyName.ToLower().Contains("fillcolorstring"))
                        {
                            selectedObjectData.FillColorString = propertyData.FillColorString;

                            selectedMapLinkZoneGraphic.ChangeColors(selectedObjectData.FillColor, selectedObjectData.BorderColor);
                        }

                        if (propertyChangedEventArgs.PropertyName.ToLower().Contains("bordercolorstring"))
                        {
                            selectedObjectData.BorderColorString = propertyData.BorderColorString;

                            selectedMapLinkZoneGraphic.ChangeColors(selectedObjectData.FillColor, selectedObjectData.BorderColor);
                        }
                    }
                    else
                    {
                        var imageGraphic = selectedGraphic as ImagePolygonGraphic;
                       
                        if (imageGraphic == null) return;

                        if (propertyChangedEventArgs.PropertyName.ToLower().Contains("imageobjectdata"))
                        {
                            if (propertyData.ImageObjectData.ImageDataStream != selectedObjectData.ImageObjectData.ImageDataStream)
                            {
                                imageGraphic.ChageSymbolImage(propertyData.ImageObjectData.ImageDataStream);
                            }

                            if (propertyData.ImageObjectData.ImageFileName != selectedObjectData.ImageObjectData.ImageFileName)
                            {
                                imageGraphic.ImageObjectData.ImageFileName = propertyData.ImageObjectData.ImageFileName;
                            }

                            if (propertyData.ImageObjectData.ImageOpacity != selectedObjectData.ImageObjectData.ImageOpacity)
                            {
                                imageGraphic.ChangeSymbolImageOpacity(propertyData.ImageObjectData.ImageOpacity);
                            }
                        }
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower() == "name")
                    {
                        selectedObjectData.Name = propertyData.Name;
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("linkedmapguid"))
                    {
                        selectedObjectData.LinkedMapGuid = propertyData.LinkedMapGuid;
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower() == "linkedmapbookmarkname")
                    {
                        selectedObjectData.LinkedMapBookmarkName = propertyData.LinkedMapBookmarkName;
                    }

                    if (propertyChangedEventArgs.PropertyName == "ShouldShowBrowserOnClick")
                    {
                        if (selectedGraphicList.Count == 1)
                        {
                            selectedObjectData.ShouldShowBrowserOnClick = propertyData.ShouldShowBrowserOnClick;
                        }
                    }

                    if (propertyChangedEventArgs.PropertyName == "BrowserUrl")
                    {
                        selectedObjectData.BrowserUrl = propertyData.BrowserUrl;
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("colorsplunkbasicinformationdata"))
                    {
                        var newData = propertyData.ColorSplunkBasicInformationData;

                        if (newData.IsSameSplunkService(selectedObjectData.ColorSplunkBasicInformationData)) return;

                        var argumentKeys = this.GetSplunkArguments(selectedObjectData.ObjectID, newData);

                        selectedObjectData.ColorSplunkBasicInformationData = newData;

                        selectedObjectData.ColorSplunkBasicInformationData.SetSplArgumentsKeys(argumentKeys);
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("tablesplunkbasicinformationdata"))
                    {
                        var newData = (sender as MapLinkZoneObjectDataInfo).TableSplunkBasicInformationData;

                        if (newData.IsSameSplunkService(selectedObjectData.TableSplunkBasicInformationData)) return;

                        var argumentKeys = this.GetSplunkArguments(selectedObjectData.ObjectID, newData);
                        
                        selectedObjectData.TableSplunkBasicInformationData = newData;

                        selectedObjectData.TableSplunkBasicInformationData.SetSplArgumentsKeys(argumentKeys);
                    }
                }
            }
            else if (sender is MapLocationObjectDataInfo)
            {
                foreach (var selectedGraphic in this.selectedGraphicList.ToList())
                {
                    var selectedMapLocationGraphic = selectedGraphic as IconGraphic;
                    var selectedMapLocationData = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type) as MapLocationObjectDataInfo;

                    if (selectedMapLocationGraphic == null ||
                        (selectedMapLocationGraphic.Type != MapObjectType.Location &&
                        selectedMapLocationGraphic.Type != MapObjectType.Address) ||
                        selectedMapLocationData == null) continue;

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("name"))
                    {
                        string StrName = (sender as MapLocationObjectDataInfo).Name;
                        if(string.IsNullOrWhiteSpace(StrName))
                        {
                            // TODO : 변경된 프로퍼티 NewValue가 OldValue와 같을 경우 UpdateSourceTrigger가 일어나지 않는다.
                            // TODO : 때문에 다른 방법을 찾기 전까지 아래 분기문으로 체크하여 처리한다.    [2014. 07. 21 엄태영]
                            if (selectedMapLocationData.Name == selectedMapLocationData.InitName)
                            {
                                (sender as MapLocationObjectDataInfo).Name = selectedMapLocationData.InitName + " ";
                                (sender as MapLocationObjectDataInfo).OnPropertyChanged("Name");
                            }
                            else
                            {
                                (sender as MapLocationObjectDataInfo).Name = selectedMapLocationData.InitName;
                                (sender as MapLocationObjectDataInfo).OnPropertyChanged("Name");
                            }
                            this.arcGISControlApi.ShowMessagePopup("Name can not be empty.", true);
                        }
                        else
                        {
                            selectedMapLocationData.Name = (sender as MapLocationObjectDataInfo).Name;
                        }
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("address"))
                    {
                        selectedMapLocationData.Address = (sender as MapLocationObjectDataInfo).Address;
                    }

                    if (string.CompareOrdinal(propertyChangedEventArgs.PropertyName, "IconSize") == 0)
                    {
                        selectedMapLocationData.IconSize = (sender as MapLocationObjectDataInfo).IconSize;

                        selectedMapLocationGraphic.ChangeIconSize(selectedMapLocationData.IconSize);
                    }
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// WorkStation Property 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void WorkStationObjectDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (sender is MapWorkStationObjectDataInfo)
            {
                foreach (var selectedGraphic in this.selectedGraphicList.ToList())
                {
                    var selectedMapWorkStationGraphic = selectedGraphic as LinkZoneGraphic;
                    var selectedWorkStationData = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type) as MapWorkStationObjectDataInfo;

                    if (selectedMapWorkStationGraphic == null || selectedWorkStationData == null) continue;

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("fillcolorstring"))
                    {
                        selectedWorkStationData.FillColorString = (sender as MapWorkStationObjectDataInfo).FillColorString;

                        selectedMapWorkStationGraphic.ChangeColors(selectedWorkStationData.FillColor, selectedWorkStationData.BorderColor);
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("bordercolorstring"))
                    {
                        selectedWorkStationData.BorderColorString = (sender as MapWorkStationObjectDataInfo).BorderColorString;

                        selectedMapWorkStationGraphic.ChangeColors(selectedWorkStationData.FillColor, selectedWorkStationData.BorderColor);
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("name"))
                    {
                        selectedWorkStationData.Name = (sender as MapWorkStationObjectDataInfo).Name;
                    }

                    if (propertyChangedEventArgs.PropertyName.ToLower().Contains("linkedmapguid"))
                    {
                        selectedWorkStationData.LinkedMapGuid = (sender as MapWorkStationObjectDataInfo).LinkedMapGuid;
                    }

                    if (propertyChangedEventArgs.PropertyName.Contains("NetworkViewLinkedMapGuid"))
                    {
                        selectedWorkStationData.NetworkViewLinkedMapGuid = (sender as MapWorkStationObjectDataInfo).NetworkViewLinkedMapGuid;
                    }

                    if (propertyChangedEventArgs.PropertyName.Contains("SoftwareViewLinkedMapGuid"))
                    {
                        selectedWorkStationData.SoftwareViewLinkedMapGuid = (sender as MapWorkStationObjectDataInfo).SoftwareViewLinkedMapGuid;
                    }

                    if (propertyChangedEventArgs.PropertyName.Contains("HardwareViewLinkedMapGuid"))
                    {
                        selectedWorkStationData.HardwareViewLinkedMapGuid = (sender as MapWorkStationObjectDataInfo).HardwareViewLinkedMapGuid;
                    }

                    if (propertyChangedEventArgs.PropertyName.Contains("SearchViewUrl"))
                    {
                        selectedWorkStationData.SearchViewUrl = (sender as MapWorkStationObjectDataInfo).SearchViewUrl;
                    }

                    if (propertyChangedEventArgs.PropertyName.Contains("SplunkBasicInformation"))
                    {
                        var newData = (sender as MapWorkStationObjectDataInfo).SplunkBasicInformation;

                        if (newData.IsSameSplunkService(selectedWorkStationData.SplunkBasicInformation)) return;

                        selectedWorkStationData.SplunkBasicInformation = newData;

                        var argumentKeys = this.GetSplunkArguments(selectedWorkStationData.ObjectID, selectedWorkStationData.SplunkBasicInformation);

                        selectedWorkStationData.SplunkBasicInformation.SetSplArgumentsKeys(argumentKeys);
                    }
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Property Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void TextDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var newData = (sender as MapTextObjectDataInfo);

            if (newData == null) return;

            foreach (var graphic in this.selectedGraphicList.OfType<PolygonControlGraphic<TextBoxControl>>())
            {
                var dataInfo = this.GetObjectData(graphic.ObjectID, graphic.Type) as MapTextObjectDataInfo;
                this.propertyDataHelper.ChangeTextGraphic(propertyChangedEventArgs.PropertyName, newData, dataInfo, graphic);
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Line Property Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void LineDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var newData = (sender as MapLineObjectDataInfo);

            if (newData == null) return;

            foreach (var graphic in this.selectedGraphicList.OfType<LineGraphic>())
            {
                this.propertyDataHelper.ChangeGraphic(propertyChangedEventArgs.PropertyName, newData, graphic);
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// Image Property Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void ImageDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var newData = (sender as MapImageObjectDataInfo);

            if (newData == null) return;

            foreach (var graphic in this.selectedGraphicList.OfType<ImagePolygonGraphic>())
            {
                var dataInfo = this.GetObjectData(graphic.ObjectID, graphic.Type, !this.IsConsoleMode) as MapImageObjectDataInfo;
                this.propertyDataHelper.ChangeGraphic(propertyChangedEventArgs.PropertyName, newData, dataInfo ,graphic);
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        private void ImageObjectDataOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var newData = (sender as CommonImageObjectDataInfo);

            if (newData == null) return;

            foreach (var graphic in this.selectedGraphicList.OfType<ImagePolygonGraphic>())
            {
                this.propertyDataHelper.ChangeGraphic(propertyChangedEventArgs.PropertyName, newData, graphic);
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        /// <summary>
        /// UniversalObject Property Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void UniversalObjectDataInfoOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var newData = (sender as MapUniversalObjectDataInfo);

            if (newData == null) return;

            UniversalControlGraphic controlGraphic;
            UniversalIconGraphic iconGraphic;

            foreach (var graphic in this.selectedGraphicList.OfType<UniversalControlGraphic>())
            {
                var dataInfo = this.universalObjectDataManager.GetDataInfo(graphic.ObjectID);

                if (dataInfo == null)
                {
                    continue;
                }

                switch (propertyChangedEventArgs.PropertyName)
                {
                    case "Name":
                        dataInfo.Name = newData.Name;
                        break;
                    case "SplunkObjectID":
                        dataInfo.SplunkObjectID = newData.SplunkObjectID;
                        break;
                    case "LinkUrl":
                        dataInfo.LinkUrl = newData.LinkUrl;
                        break;
                    case "LinkedMapGuid":
                        dataInfo.LinkedMapGuid = newData.LinkedMapGuid;
                        break;
                    case "LinkedMapBookmarkName":
                        dataInfo.LinkedMapBookmarkName = newData.LinkedMapBookmarkName;
                        break;
                    case "LinkedMapObjectName":
                        dataInfo.LinkedMapObjectName = newData.LinkedMapObjectName;
                        break;
                    case "Title":
                        dataInfo.Title = newData.Title;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.Title = newData.Title;
                        break;
                    case "TitleMinPosition":
                    case "TitleMaxPosition":
                        dataInfo.TitleMinPosition = newData.TitleMinPosition;
                        dataInfo.TitleMaxPosition = newData.TitleMaxPosition;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.TitleMinMaxPosition = new Rect(newData.TitleMinPosition, newData.TitleMaxPosition);
                        break;
                    case "TitleColor":
                        dataInfo.TitleColor = newData.TitleColor;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.TitleColor = newData.TitleColor;
                        break;
                    case "TitleAlignment":
                        dataInfo.TitleAlignment = newData.TitleAlignment;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.TitleAlignment = newData.TitleAlignment;
                        break;
                    case "IconImageUrl":
                        dataInfo.IconImageUrl = newData.IconImageUrl;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.IconImageUrl = newData.IconImageUrl;
                        break;
                    case "IconMinPosition":
                    case "IconMaxPosition":
                        dataInfo.IconMinPosition = newData.IconMinPosition;
                        dataInfo.IconMaxPosition = newData.IconMaxPosition;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.IconMinMaxPosition = new Rect(newData.IconMinPosition, newData.IconMaxPosition);
                        break;
                    case "BorderThickness":
                        dataInfo.BorderThickness = newData.BorderThickness;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.StrokeThickness = newData.BorderThickness;
                        break;
                    case "BorderRadius":
                        dataInfo.BorderRadius = newData.BorderRadius;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.StrokeRadius = newData.BorderRadius;
                        break;
                    case "BorderColor":
                        dataInfo.BorderColor = newData.BorderColor;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.StrokeColor = newData.BorderColor;
                        break;
                    case "FillColor":
                        dataInfo.FillColor = newData.FillColor;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.FillColor = newData.FillColor;
                        break;
                    case "ShapeType":
                        dataInfo.ShapeType = newData.ShapeType;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.ShapeType = newData.ShapeType;
                        break;
                    case "FillImageUrl":
                        dataInfo.FillImageUrl = newData.FillImageUrl;
                        controlGraphic = this.universalObjectDataManager.GetControlGraphic(graphic.ObjectID);
                        controlGraphic.Control.FillImageUrl = newData.FillImageUrl;
                        break;
                    case "ShowAlarmLamp":
                        dataInfo.ShowAlarmLamp = newData.ShowAlarmLamp;
                        iconGraphic = this.universalObjectDataManager.GetIconGraphic(graphic.ObjectID);
                        iconGraphic.IsVisible = newData.ShowAlarmLamp;
                        break;
                    case "AlarmLampPosition":
                        dataInfo.AlarmLampPosition = newData.AlarmLampPosition;
                        iconGraphic = this.universalObjectDataManager.GetIconGraphic(graphic.ObjectID);
                        iconGraphic.Position = this.universalObjectDataManager.GetIconPosition(newData);
                        break;
                    case "AlarmLampSize":
                        dataInfo.AlarmLampSize = newData.AlarmLampSize;
                        iconGraphic = this.universalObjectDataManager.GetIconGraphic(graphic.ObjectID);
                        iconGraphic.Size = newData.AlarmLampSize;
                        break;
                    case "AlarmLampColor":
                        dataInfo.AlarmLampColor = newData.AlarmLampColor;
                        iconGraphic = this.universalObjectDataManager.GetIconGraphic(graphic.ObjectID);
                        iconGraphic.Color = newData.AlarmLampColor;
                        break;
                }
            }

            if (this.isEditMode)
            {
                this.historyManager.IsChanged = true;
            }
        }

        #endregion Event Handlers
    }
}
