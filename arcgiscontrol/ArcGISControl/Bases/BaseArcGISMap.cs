using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ArcGISControl.DataManager;
using ArcGISControl.GraphicObject;
using ArcGISControl.Language;
using ArcGISControl.TiledMapLayer;
using ArcGISControl.UIControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using Innotive.SplunkControl.Table;

namespace ArcGISControl.Bases
{
    using ArcGISInternalHack;
    using ArcGISControl.Helper;
    using ArcGISControls.MapTileImageProxy;

    /// <summary>
    /// Cient 와 Server의 상속 정보를 위해
    /// </summary>
    public partial class BaseArcGISMap : UserControl, IDisposable
    {
        #region Field

        #region Property & Status Field

        /// <summary>
        /// Map 모드 (CU,VW,Edit,VIEW)
        /// </summary>
        private readonly MapControlType controlType = MapControlType.None;

        /// <summary>
        /// Type 
        /// </summary>
        protected bool isEditMode
        {
            get
            {
                return this.controlType == MapControlType.ViewerEditMode ||
                       this.controlType == MapControlType.ManagerEditMode;
            }
        }

        /// <summary>
        /// Type 
        /// </summary>
        public bool IsConsoleMode
        {
            get
            {
                return this.controlType == MapControlType.ViewerEditMode ||
                       this.controlType == MapControlType.ViewerViewMode;
            }
            
        }
       
        /// <summary>
        /// Map Type 저장
        /// </summary>
        protected MapProviderType currentMapType = MapProviderType.None;

        private bool disposed = false;

        #endregion Property & Status Field

        #region Tile Fields

        /// <summary>
        /// tile Load 된 후 Set
        /// </summary>
        protected bool isInitializedMapTileService;

        /// <summary>
        /// Tile 만 Refresh 할지 Flag
        /// </summary>
        protected bool justTileRefresh = false;

        /// <summary>
        /// 취소를 위한 토큰
        /// </summary>
        protected CancellationTokenSource cancellationTokenSourceTileLoad;

        /// <summary>
        /// 취소를 위한 토큰
        /// </summary>
        protected CancellationToken tileLoadCancellationToken;

        #endregion Tile Fields

        #region Objects Fields

        /// <summary>
        /// Camera Graphic Manager
        /// </summary>
        protected readonly CameraGraphicDataManager cameraGraphicDataManager;

        /// <summary>
        /// Location Data Manager
        /// </summary>
        protected readonly LocationGraphicDataManager publicGraphicDataManager;

        /// <summary>
        /// Location Data Manager
        /// </summary>
        protected readonly LocationGraphicDataManager privateGraphicDataManager;

        /// <summary>
        /// Searched Address Manager
        /// </summary>
        protected readonly SearchAddressGraphicManager searchAddressGraphicDataManager;

        /// <summary>
        /// SavedSplunk Manager
        /// </summary>
        protected readonly SplunkObjectDataManager savedSplunkObjectDataManager;

        /// <summary>
        /// Memo Manager
        /// </summary>
        protected readonly MemoObjectDataManager memoObjectDataManager;

        /// <summary>
        /// Universal Manager
        /// </summary>
        protected readonly UniversalObjectDataManager universalObjectDataManager;

        #endregion Objects Fields

        #region UI ELEMENT

        #region UserControl Elements

        /// <summary>
        /// User Control Base Grid
        /// </summary>
        protected Grid baseGrid;

        /// <summary>
        /// Map 상위에 올라는 Canvas 
        /// Popup 들이 올라 간다.
        /// </summary>
        protected Canvas mainCanvas;

        /// <summary>
        /// ErrorMessage 표현을 위한 UserControl
        /// </summary>
        private ErrorMessageUserControl errorMessageUserControl;

        /// <summary>
        /// Camera Poup Control Manager
        /// Camera Poup 교체 될 용도
        /// </summary>
        protected CameraPopupControlManager cameraPopupControlManager;

        /// <summary>
        /// Sub Table Popup Manager
        /// LinkZone Click 시 Splunk Control Manager
        /// </summary>
        protected DynamicSplunkControlManager dynamicSplunkControlManager;

        /// <summary>
        /// WorkStation ContextMenu Manager
        /// </summary>
        protected WorkStationControlManager workStationControlManager;

        /// <summary>
        /// Splunk Poup Control Manager
        /// </summary>
        protected SplunkPopupControlManager splunkPopupControlManager;

        /// <summary>
        /// Location Info Window Manager
        /// </summary>
        protected LocationInfoWindowManager locationInfoWindowManager;

        /// <summary>
        /// Serached Address Graphic Manager
        /// </summary>
        protected SearchedAddressInfoWindowManager searchedAddressInfoWindowManager;

        #endregion UserControl Elements

        #region Map UI Elements

        /// <summary>
        /// Base Map
        /// </summary>
        protected Map baseMap;

        /// <summary>
        /// Tile Layer
        /// </summary>
        protected TiledLayer baseTiledMapServiceLayer;

        /// <summary>
        /// Graphic Layer
        /// </summary>
        protected GraphicsLayer objectGraphicLayer;

        /// <summary>
        /// Graphic Editor 사용 Layer
        /// </summary>
        protected readonly GraphicsLayer editorGraphicsLayer;

        #endregion Map UI Elements
        
        #region baseMap double buffering 관련
        /// <summary>
        /// basemap을 부드럽게 교체하기 위한 백버퍼 객체. baseMap과 swap된다.
        /// </summary>
        protected Map backbufferBaseMap;

        /// <summary>
        /// baseMap을 부드럽게 교체해야하는 상황인지를 기억해두고 Progress 100이 되었을 때 확인하여 처리한다.
        /// </summary>
        private bool needBackBufferSwap;

        /// <summary>
        /// 부드러운 교체의 애니메이션 스토리보드.
        /// 
        /// 매 교체마다 객체가 바뀐다. 이것을 이용해 이전 애니메이션을 취소하거나 현재 것인지 체크를 할 수 있다.
        /// </summary>
        private Storyboard mapTransitionStoryboard;

        /// <summary>
        /// baseMap이 backbufferBaseMap과 교체되는 경우 발생하는 이벤트
        /// </summary>
        public event EventHandler<EventArgs> BaseMapChanged;

        #endregion

        #region Z Index

        public const int PopupDynamicSplunkControlZIndex = 400;

        public const int PopupWorkStationControlZIndex = 350;

        public const int PopupSplunkControlZIndex = 350;

        public const int PopupCameraControlZIndex = 350;

        public const int CameraOverlayControlZIndex = 300;

        public const int CameraSelectionBorderZIndex = 250;

        #endregion //Z Index

        #endregion //UI ELEMENT

        #region MapData

        /// <summary>
        /// 현재 Map의 설정 데이터
        /// </summary>
        protected MapSettingDataInfo mapSettingDataInfo;

        #endregion MapData

        #endregion // Field

        #region Events

        /// <summary>
        /// Make Key 인증 실패시 
        /// </summary>
        public event EventHandler<LicenseKeyNotAllowedEventArgs> eLicenseKeyNotAllowed;

        /// <summary>
        /// Map Loading Erring 시 발생
        /// </summary>
        public event EventHandler<MapLoadingErrorEventArgs> eMapLoadingErrorOccured;

        /// <summary>
        /// Map Loading이 모두 끝난후 Event 생성
        /// </summary>
        public event EventHandler<EventArgs> eMapTileLoaded;

        #endregion Events

        #region Construct

        protected BaseArcGISMap(MapControlType controlType)
        {
            var proxyServerPort = ArcGISConstSet.ProxyServerPort;
            this.controlType = controlType;

            this.baseGrid = new Grid();

            this.baseMap = new Map
            {
                WrapAround = true,
                SnapToLevels = false,
                IsLogoVisible = false,
                Background = null,
                PanDuration = ArcGISConstSet.PanDuration,
                ZoomDuration = ArcGISConstSet.ZoomDuration,
            };

            this.backbufferBaseMap = new Map
            {
                WrapAround = true,
                SnapToLevels = false,
                IsLogoVisible = false,
                Background = null,
                PanDuration = ArcGISConstSet.PanDuration,
                ZoomDuration = ArcGISConstSet.ZoomDuration,
            };

            this.errorMessageUserControl = new ErrorMessageUserControl(){Visibility = Visibility.Collapsed};

            new ExtentMaintainer(this.baseMap);
            new ExtentMaintainer(this.backbufferBaseMap);

            this.mainCanvas = new Canvas();
            this.objectGraphicLayer = new GraphicsLayer();

            // [yjjang] : CameraPopupControlManager 작업.
            this.cameraPopupControlManager = new CameraPopupControlManager { ZIndex = PopupCameraControlZIndex };

            this.dynamicSplunkControlManager = new DynamicSplunkControlManager { ZIndex = PopupDynamicSplunkControlZIndex };
            
            this.workStationControlManager = new WorkStationControlManager {ZIndex = PopupSplunkControlZIndex};
            this.splunkPopupControlManager = new SplunkPopupControlManager { ZIndex = PopupWorkStationControlZIndex };

            this.Loaded += OnLoaded;

            this.AddMapEventHandlers(this.baseMap);

            this.AddChild(this.baseGrid);

            {
                Panel.SetZIndex(this.backbufferBaseMap, -2);
                Panel.SetZIndex(this.baseMap, -1);
                // 백버퍼는 숨겨놓고 교체할 때만 보여준다.
                this.backbufferBaseMap.Visibility = Visibility.Collapsed;

                this.baseGrid.Children.Add(this.backbufferBaseMap);
                this.baseGrid.Children.Add(this.baseMap);
                this.baseGrid.Children.Add(this.errorMessageUserControl);
            }

            this.baseGrid.Children.Add(this.mainCanvas);

            this.dynamicSplunkControlManager.AddToParent(this.mainCanvas);
            this.workStationControlManager.AddToParent(this.mainCanvas);
            this.splunkPopupControlManager.AddToParent(this.mainCanvas);
            this.cameraPopupControlManager.AddToParent(this.mainCanvas);

            Canvas.SetLeft(this.mainCanvas, 0);
            Canvas.SetTop(this.mainCanvas, 0);

            this.baseMap.Layers.Add(this.objectGraphicLayer);

            this.cameraGraphicDataManager = new CameraGraphicDataManager();
            this.publicGraphicDataManager = new LocationGraphicDataManager();
            this.privateGraphicDataManager = new LocationGraphicDataManager();
            this.searchAddressGraphicDataManager = new SearchAddressGraphicManager();
            this.savedSplunkObjectDataManager = new SplunkObjectDataManager();
            this.memoObjectDataManager = new MemoObjectDataManager();
            this.universalObjectDataManager = new UniversalObjectDataManager(this.GetMapResolution);

            if(!this.isEditMode)
            {   
                this.locationInfoWindowManager = new LocationInfoWindowManager(this.baseMap);
                this.baseGrid.Children.Add(this.locationInfoWindowManager.LocationInfoWIndow);

                this.searchedAddressInfoWindowManager = new SearchedAddressInfoWindowManager(this.baseMap);
                this.searchedAddressInfoWindowManager.eSearchedAddressSaveButtonClick += InfoWindowManager_SearchedAddressSaveButtonClick;
                this.baseGrid.Children.Add(this.searchedAddressInfoWindowManager.LocationInfoWIndow);
            }
            else
            {
                this.editorGraphicsLayer = new GraphicsLayer();
                this.baseMap.Layers.Add(this.editorGraphicsLayer);
            }

            if(!ProxyServer.Instance.IsStarted) ProxyServer.Instance.Start(proxyServerPort);
        }

        #endregion //Construct

        #region Method

        #region Common Loading Map Method

        /// <summary>
        /// Load Map Tile 
        /// </summary>
        protected void RefreshMapTiles(bool smoothTransition = false)
        {
            this.isInitializedMapTileService = false;
            this.justTileRefresh = false;

            this.LoadMapTileService(smoothTransition);
        }

        private void BeginSmoothTransition()
        {
            if (this.needBackBufferSwap)
            {
                this.needBackBufferSwap = false;

                // 화면 전환 애니메이션
                var animationDuration = new Duration(ArcGISConstSet.SmoothTransitionAnimationTimeSpan);

                var fadeInAnimation = new DoubleAnimation(0, 1, animationDuration);
                Storyboard.SetTarget(fadeInAnimation, this.baseMap);
                Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(Map.OpacityProperty));
                fadeInAnimation.FillBehavior = FillBehavior.HoldEnd;

                var capturedStoryboard = new Storyboard();
                this.mapTransitionStoryboard = capturedStoryboard;
                // fadeOutAnimation으로 앞에 나타나 있던 것이 점차 사라지므로 뒤에 있던 맵은 완전 불투명으로 있어야 한다.
                capturedStoryboard.Children.Add(fadeInAnimation);

                capturedStoryboard.Completed += (s, e) =>
                {
                    // 더 이상 이 작업은 진행하지 않는다.
                    if (!ReferenceEquals(capturedStoryboard, this.mapTransitionStoryboard))
                        return;

                    this.AfterTransitionStoryboard();
                };

                capturedStoryboard.Begin();
            }
        }

        protected virtual void CleanupTransitionStoryboard()
        {
            // 애니메이션 해제
            this.backbufferBaseMap.BeginAnimation(Map.OpacityProperty, null);
            this.baseMap.BeginAnimation(Map.OpacityProperty, null);
            this.baseMap.Opacity = 1.0;
            this.backbufferBaseMap.Opacity = 1.0;

            // 갖고 있던 남은 타일 맵 레이어들 제거
            this.backbufferBaseMap.Layers.Clear();

            // Z 깊이, 보이기 상태등을 규칙대로 되돌림
            this.backbufferBaseMap.Visibility = Visibility.Collapsed;
        }

        protected virtual void AfterTransitionStoryboard()
        {
            this.CleanupTransitionStoryboard();
        }

        /// <summary>
        /// map Tile 정보에 따라서 
        /// BingMap을 올릴지 
        /// 그냥 map을 올릴지 확인 한다.
        /// </summary>
        protected void LoadMapTileService(bool smoothTransition = false)
        {
            if (this.justTileRefresh)
                this.ClearTiledMapLayers();
            else if (!smoothTransition)
                this.ReleaseMap();
            else
            {
                // 교체 방식 개요
                // Task Invariant:
                // baseMap의 깊이는 -1이고, backbufferBaseMap의 깊이는 -2이다.
                //
                // 초기 상태:
                // front buffer(baseMap)이 -1 깊이에 있고, back buffer(backbufferBaseMap)이 -2 깊이에 있다. front buffer만 보이는 상태./
                //
                // 교체 시작:
                // 교체 요청이 들어오면 둘 다 보이게 설정하고 back buffer였던 것을 Opacity를 0으로 설정한다.
                // back buffer와 front buffer의 깊이를 서로 바꿔주고 baseMap과 backbufferBaseMap 객체를 swap한다.
                // swap하면서 BaseMapChanged 이벤트를 발생시켜준다. TODO: 필요하면 BaseMapChanging 이벤트를 추가한다.
                //
                // 교체 보여주기:
                // 로드가 완료되어 Progress가 100이 되면 back buffer였던 것의 Opacity Fade In 애니메이션을 진행한다.
                // front buffer였던 것을 감춘다.

                // 이전 애니메이션 존재 여부 체크하고 null을 대입하여 Completed 처리를 막는다.
                var lastStoryboard = Interlocked.Exchange(ref this.mapTransitionStoryboard, null);
                if (lastStoryboard != null)
                {
                    lastStoryboard.Remove();
                    this.CleanupTransitionStoryboard();
                }

                // 잠깐 보여주고 말 그래픽 레이어 복제. 관련 기능은 작동하지 않는 것이 의도이다. 보여주기만 할 것들.
                // 용도는 부드럽게 Transition 되는 상황에서 이전에 보이던 Graphic들이 사라지지 않고 여전히 보이도록 하게 하기 위한 것이다.
                //var graphicLayerClone = new GraphicsLayer()
                //{
                //    Graphics = new GraphicCollection(this.objectGraphicLayer.Graphics.Select(p=>new Graphic
                //    {
                //        Symbol = p.Symbol,
                //        Geometry = p.Geometry
                //    }))
                //};

                this.ReleaseMap(true);

                this.RemoveMapEventHandlers(this.baseMap);
                //this.baseMap.Layers.Add(graphicLayerClone);
                this.baseMap.Layers.Remove(this.objectGraphicLayer);
                
                this.savedSplunkObjectDataManager.StopAllSplunkServices();
                this.publicGraphicDataManager.StopAllSplunkServices();

                {
                    this.baseMap.Visibility = Visibility.Visible;
                    this.backbufferBaseMap.Visibility = Visibility.Visible;

                    this.backbufferBaseMap.Opacity = 0;
                    Panel.SetZIndex(this.backbufferBaseMap, -1);
                    Panel.SetZIndex(this.baseMap, -2);
                }

                this.backbufferBaseMap.Layers.Clear();
                this.backbufferBaseMap.Layers.Add(this.objectGraphicLayer);
                if(this.editorGraphicsLayer != null) this.backbufferBaseMap.Layers.Add(this.editorGraphicsLayer);
                this.AddMapEventHandlers(this.backbufferBaseMap);

                {
                    var tmp = this.baseMap;
                    this.baseMap = this.backbufferBaseMap;
                    this.backbufferBaseMap = tmp;
                }

                this.needBackBufferSwap = true;

                this.OnBaseMapChanged();
            }

            if (string.IsNullOrEmpty(this.mapSettingDataInfo.MapServiceUrl) &&
                this.mapSettingDataInfo.MapSelectedType == MapProviderType.ArcGisClientMap)
            {
                this.Dispatcher.Invoke(new Action(() => this.FailedMapTileLoaded(Resource_ArcGISControl_ArcGISClientViewer.Message_MapNullUrl)));
                return;
            }

            var mapType = this.mapSettingDataInfo.MapType;
            var licenseKey = this.mapSettingDataInfo.LicenseKey;
            var mapServiceurl = this.mapSettingDataInfo.MapServiceUrl;

            string returnMessage = string.Empty;
            this.cancellationTokenSourceTileLoad = new CancellationTokenSource();
            this.tileLoadCancellationToken = cancellationTokenSourceTileLoad.Token;

            switch (mapType)
            {
                case MapProviderType.DaumMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo,ArcGISConstSet.SearchTextUrlDaum, 
                                        new DaumMapTiledService(DaumMapTiledService.MapStyle.General),false);
                    break;

                case MapProviderType.DaumSatelliteMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, ArcGISConstSet.SearchTextUrlDaum, 
                                        new DaumMapTiledService(DaumMapTiledService.MapStyle.Satellite),false);
                    break;

                case MapProviderType.DaumSatelliteHybridMap:

                    this.baseMap.WrapAround = false;
                    this.baseTiledMapServiceLayer = new DaumMapTiledService(DaumMapTiledService.MapStyle.SatelliteOver);

                    baseMap.Layers.Insert(0, this.baseTiledMapServiceLayer);

                    goto case MapProviderType.DaumSatelliteMap;

                case MapProviderType.DaumSatelliteTrafficMap:

                    baseMap.Layers.Insert(0, new DaumMapTiledService(DaumMapTiledService.MapStyle.Traffic));

                    goto case MapProviderType.DaumSatelliteHybridMap;

                case MapProviderType.DaumTrafficMap:

                    baseMap.Layers.Insert(0, new DaumMapTiledService(DaumMapTiledService.MapStyle.Traffic));

                    goto case MapProviderType.DaumMap;

                case MapProviderType.NaverMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, ArcGISConstSet.SearchTextUrlNaver, 
                                        new NaverMapTiledService(NaverMapTiledService.MapStyle.General),false);
                    break;

                case MapProviderType.NaverSatelliteMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, ArcGISConstSet.SearchTextUrlNaver, 
                                        new NaverMapTiledService(NaverMapTiledService.MapStyle.Satellite), false);
                    break;

                case MapProviderType.NaverSatelliteTrafficMap:

                    baseMap.Layers.Insert(0, new NaverMapTiledService(NaverMapTiledService.MapStyle.Traffic));

                    goto case MapProviderType.NaverSatelliteHybridMap;

                case MapProviderType.NaverSatelliteHybridMap:

                    baseMap.Layers.Insert(0, new NaverMapTiledService(NaverMapTiledService.MapStyle.SatelliteOver));

                    goto case MapProviderType.NaverSatelliteMap;

                case MapProviderType.NaverTrafficMap:

                    baseMap.Layers.Insert(0, new NaverMapTiledService(NaverMapTiledService.MapStyle.Traffic));

                    goto case MapProviderType.NaverMap;

                case MapProviderType.ArcGisImageryMap:

                    this.baseMap.WrapAround = true;
                    this.InitializeMainTileLayer(new ArcGISTiledMapServiceLayer() { Url = this.mapSettingDataInfo.MapServiceUrl });
                    break;

                case MapProviderType.ArcGisStreetMap:

                    goto case MapProviderType.ArcGisImageryMap;

                case MapProviderType.ArcGisTogoMap:

                    goto case MapProviderType.ArcGisImageryMap;

                case MapProviderType.ArcGisClientMap:

                    goto case MapProviderType.ArcGisImageryMap;

                case MapProviderType.BingMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, this.mapSettingDataInfo.MapServiceUrl, 
                                        new TileLayer()
                                        {
                                            ID = "BingLayer",
                                            LayerStyle = TileLayer.LayerType.Road,
                                            ServerType = ServerType.Production,
                                            Token = licenseKey
                                        },true);
                    break;

                case MapProviderType.BingArialMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, this.mapSettingDataInfo.MapServiceUrl,
                                        new TileLayer()
                                        {
                                            ID = "BingLayer",
                                            LayerStyle = TileLayer.LayerType.Aerial,
                                            ServerType = ServerType.Production,
                                            Token = licenseKey
                                        },true);
                    break;

                case MapProviderType.BingArialWithLabelMap:

                    this.CheckLicenseKey(this.mapSettingDataInfo, this.mapSettingDataInfo.MapServiceUrl,
                                        new TileLayer()
                                        {
                                            ID = "BingLayer",
                                            LayerStyle = TileLayer.LayerType.AerialWithLabels,
                                            ServerType = ServerType.Production,
                                            Token = licenseKey
                                        }, true);
                    break;

                case MapProviderType.CustomMap:

                    if (!string.IsNullOrEmpty(mapServiceurl) && mapServiceurl != "/")
                    {
                        var licenseCheckTask
                                        = Task<Dictionary<string,string>>.Factory.StartNew(() =>
                                                                        MapServiceChecker.Instance.GetCustomMapServiceData(mapServiceurl, out returnMessage));

                        licenseCheckTask.ContinueWith((contiuation) =>
                        {
                            var summary = contiuation.Result;
                            if (summary != null && string.IsNullOrEmpty(returnMessage))
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.baseMap.WrapAround = false;
                                    int levelCount = Convert.ToInt16(summary["level count"]);
                                    this.InitializeMainTileLayer(
                                                                new CustomMapTiledService(levelCount,
                                                                mapServiceurl,
                                                                Convert.ToInt32(summary["tile width"]),
                                                                Convert.ToInt32(summary["tile height"]),
                                                                Convert.ToInt32(summary["level1 valid total width"]),
                                                                Convert.ToInt32(summary["level1 valid total height"]),
                                                                Convert.ToInt32(summary[string.Format("level{0} valid total width", levelCount)]),
                                                                Convert.ToInt32(summary[string.Format("level{0} valid total height", levelCount)])));

                                    this.SetDigitalZoom(levelCount);
                                }));
                            }
                            else
                            {
                                this.Dispatcher.Invoke(new Action(() => this.FailedMapTileLoaded(Resource_ArcGISControl_ArcGISClientViewer.Message_CanNotLoadMap)));
                            }
                        });
                    }
                    break;

                case MapProviderType.GoogleMap:
                    this.InitializeMainTileLayer(new GoogleMapTiledService(GoogleMapTiledService.MapStyle.General));
                    break;
                case MapProviderType.GoogleSatelliteMap:
                    this.InitializeMainTileLayer(new GoogleMapTiledService(GoogleMapTiledService.MapStyle.Satellite));
                    break;
                case MapProviderType.GoogleSatelliteHybridMap:
                    baseMap.Layers.Insert(0, new GoogleMapTiledService(GoogleMapTiledService.MapStyle.SatelliteOver));
                    goto case MapProviderType.GoogleSatelliteMap;
            }
        }

        private void CheckLicenseKey(MapSettingDataInfo mapSettingData, string checkServiceUrl, TiledLayer tiledLayer, bool isWapAroundMap)
        {
            var mapType = mapSettingData.MapType;
            var licenseKey = mapSettingData.LicenseKey;
            var mapServiceurl = checkServiceUrl;
            var returnMessage = string.Empty;

            var licenseCheckTask 
                            = Task<bool>.Factory.StartNew(() =>
                                                            MapServiceChecker.Instance.CheckLicenseKey(mapType,
                                                            mapServiceurl,
                                                            licenseKey, out returnMessage)
                                                            );

            licenseCheckTask.ContinueWith((contiuation) =>
                                                {
                                                    if (contiuation.Result && string.IsNullOrEmpty(returnMessage))
                                                    {
                                                        this.Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            this.baseMap.WrapAround = isWapAroundMap;
                                                            this.InitializeMainTileLayer(tiledLayer);
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        this.Dispatcher.Invoke(new Action(() => this.FailedMapTileLoaded(Resource_ArcGISControl_ArcGISClientViewer.Message_CanNotLoadMap)));
                                                    }
                                                });
        }

        /// <summary>
        /// Map Tile Service의 초기화
        /// </summary>
        /// <param name="tiledLayer"></param>
        private void InitializeMainTileLayer(TiledLayer tiledLayer)
        {
            if (tiledLayer == null) return;

            this.baseTiledMapServiceLayer = tiledLayer;

            this.baseTiledMapServiceLayer.Initialized += tileLayer_Initialized;
            this.baseTiledMapServiceLayer.InitializationFailed += tileLayer_InitializationFailed;

            try
            {
                baseMap.Layers.Insert(0, this.baseTiledMapServiceLayer);
            }
            catch
            {
                if (this.eLicenseKeyNotAllowed != null)
                {
                    this.eLicenseKeyNotAllowed(this, new LicenseKeyNotAllowedEventArgs{Message = Resource_ArcGISControl_ArcGISClientViewer.Message_MapKeyNotAllowed});
                }
            }
        }

        /// <summary>
        /// Fail Map tile loade
        /// </summary>
        /// <param name="message"></param>
        protected virtual void FailedMapTileLoaded(string message)
        {
            this.errorMessageUserControl.ErrorMessage = message;
            this.errorMessageUserControl.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Map 해제
        /// </summary>
        protected virtual void ReleaseMap(bool preserveMapLayer = false)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>{this.errorMessageUserControl.Visibility = Visibility.Collapsed;}));
            this.ClearTiledMapLayers(preserveMapLayer);

            var cts = Interlocked.Exchange(ref this.cancellationTokenSourceTileLoad, null);
            if (cts != null)
                cts.Cancel();
        }

        protected virtual void ClearSplunkObjectDatas()
        {
            this.savedSplunkObjectDataManager.StopAllSplunkServices();

            this.savedSplunkObjectDataManager.ClearObjectDatas();

            this.savedSplunkObjectDataManager.SplunkObjectDatas.Clear();
        }

        protected virtual void ClearObjectDatas()
        {
            this.cameraGraphicDataManager.ClearObjectDatas();
            this.privateGraphicDataManager.ClearObjectDatas();

            this.publicGraphicDataManager.StopAllSplunkServices();
            this.publicGraphicDataManager.ClearObjectDatas();

            this.memoObjectDataManager.ClearObjects();

            this.universalObjectDataManager.ClearObjects();

            if (this.objectGraphicLayer != null) this.objectGraphicLayer.Graphics.Clear();
        }

        protected virtual void ClearPopupControls()
        {
            this.HideCameraPopupControl();
            this.splunkPopupControlManager.Hide();

            if (this.mainCanvas != null)
            {
                this.workStationControlManager.RemoveFromParent(this.mainCanvas);
                this.splunkPopupControlManager.RemoveFromParent(this.mainCanvas);

                this.mainCanvas.Children.Clear();

                if (this.workStationControlManager != null)
                {
                    this.workStationControlManager.AddToParent(this.mainCanvas);
                }

                if (this.splunkPopupControlManager != null)
                {
                    this.splunkPopupControlManager.AddToParent(this.mainCanvas);
                }

                if (this.dynamicSplunkControlManager != null)
                {
                    this.dynamicSplunkControlManager.AddToParent(this.mainCanvas);
                }

                if (this.cameraPopupControlManager != null)
                {
                    this.cameraPopupControlManager.AddToParent(this.mainCanvas);
                }
            }

            if (this.locationInfoWindowManager != null) this.locationInfoWindowManager.HideInfoWindow();

            if (this.searchedAddressInfoWindowManager != null) this.searchedAddressInfoWindowManager.HideInfoWindow();

        }

        /// <summary>
        /// Map Layer Clear
        /// </summary>
        protected void ClearTiledMapLayers(bool preserveMapLayer = false)
        {
            if (this.baseTiledMapServiceLayer != null)
            {
                if (!preserveMapLayer && baseMap.Layers.Contains(this.baseTiledMapServiceLayer))
                    baseMap.Layers.Remove(this.baseTiledMapServiceLayer);

                this.baseTiledMapServiceLayer.Initialized -= tileLayer_Initialized;
                this.baseTiledMapServiceLayer.InitializationFailed -= tileLayer_InitializationFailed;

                this.baseTiledMapServiceLayer = null;
            }

            if (!preserveMapLayer)
            {
                var layers = (from layer in this.baseMap.Layers where layer is ArcGISTiledMapServiceLayer || layer is TiledLayer select layer as TiledLayer).ToList();

                foreach (var arcGisTiledMapServiceLayer in layers.Where(arcGisTiledMapServiceLayer => baseMap.Layers.Contains(arcGisTiledMapServiceLayer)))
                {
                    baseMap.Layers.Remove(arcGisTiledMapServiceLayer);
                }
            }
        }

        /// <summary>
        /// Change Location
        /// </summary>
        /// <param name="min"></param>
        protected void ChangeLocation(Point min)
        {
            this.baseMap.PanTo(new MapPoint(min.X, min.Y));
        }

        /// <summary>
        /// Change Zoom and Location
        /// </summary>
        /// <param name="extent"></param>
        protected void ChangeZoomToLocation(Envelope extent)
        {
            if (this.baseTiledMapServiceLayer == null) return;

            if (this.baseTiledMapServiceLayer.FullExtent != null &&
                (Math.Floor(extent.Height) <= Math.Floor(this.baseTiledMapServiceLayer.FullExtent.Height) &&
                Math.Floor(extent.Width) <= Math.Floor(this.baseTiledMapServiceLayer.FullExtent.Width)))
            {
                var mapExtentRatio = this.baseMap.Extent.Width / this.baseMap.Extent.Height;
                var newWidth = Math.Max(extent.Height * mapExtentRatio, extent.Width);
                if (NumberUtil.AreSame(newWidth, this.baseMap.Extent.Width))
                    this.baseMap.PanTo(extent.GetCenter());
                else
                    this.baseMap.ZoomTo(extent);
            }
            else
            {
                if (this.baseTiledMapServiceLayer.FullExtent != null)
                {
                    var fullExtentW = this.baseTiledMapServiceLayer.FullExtent.Width / 2;
                    var fullExtentH = this.baseTiledMapServiceLayer.FullExtent.Height / 2;
                    
                    var newCenter = extent.GetCenter();

                    var newExtent = new Envelope(newCenter.X - fullExtentW, newCenter.Y - fullExtentH,
                                                 newCenter.X + fullExtentW, newCenter.Y + fullExtentH);

                    this.baseMap.ZoomTo(newExtent);
                }
            }
        }

        /// <summary>
        /// Digital Zoom을 설정 한다.
        /// </summary>
        protected void SetDigitalZoom(int originalLevel)
        {
            this.baseMap.MinimumResolution = 1 / Math.Pow(2, originalLevel + 100);
        }
       
        /// <summary>
        /// 현재 Extent를 LAT LNG 형태로 변환
        /// From ChangeMap, Save Map
        /// </summary>
        protected void ChangeMapExtentToGeographic(MapProviderType mapType)
        {
            if (this.baseMap == null || this.baseMap.Extent == null || mapType == MapProviderType.None) return;

            this.mapSettingDataInfo.ExtentMax = new Point(this.baseMap.Extent.XMax, this.baseMap.Extent.YMax);
            this.mapSettingDataInfo.ExtentMin = new Point(this.baseMap.Extent.XMin, this.baseMap.Extent.YMin);

            if (ArcGISDataConvertHelper.IsGISMapType(mapType))
            {
                var latlng = GeometryHelper.ToGeographic(this.mapSettingDataInfo.ExtentMax, mapType);
                this.mapSettingDataInfo.ExtentMax = new Point(latlng.Lng, latlng.Lat);

                latlng = GeometryHelper.ToGeographic(this.mapSettingDataInfo.ExtentMin, mapType);
                this.mapSettingDataInfo.ExtentMin = new Point(latlng.Lng, latlng.Lat);
            }
        }

        /// <summary>
        /// Map의 보이는 영역을 바꾸어 준다
        /// </summary>
        protected virtual void InitializeDefaultMapExtent()
        {
            if (this.baseTiledMapServiceLayer == null || this.mapSettingDataInfo == null ||
                    this.baseMap == null || this.baseTiledMapServiceLayer.FullExtent == null) return;

            if (ArcGISDataConvertHelper.IsGISMapType(this.mapSettingDataInfo.MapType))
            {
                if (this.mapSettingDataInfo.ExtentMax == new Point() || this.mapSettingDataInfo.ExtentMin == new Point())
                {
                    this.SafeSetExtent(this.baseTiledMapServiceLayer.FullExtent);
                }
                else //Change LAT,LNG To Extent
                {
                    var maxPoint = this.FromGeographic(new LatLng(this.mapSettingDataInfo.ExtentMax.Y, this.mapSettingDataInfo.ExtentMax.X), this.mapSettingDataInfo.MapType.ToString());
                    var minPoint = this.FromGeographic(new LatLng(this.mapSettingDataInfo.ExtentMin.Y, this.mapSettingDataInfo.ExtentMin.X), this.mapSettingDataInfo.MapType.ToString());

                    if ((maxPoint.X < this.baseTiledMapServiceLayer.FullExtent.XMin) ||
                        (maxPoint.Y < this.baseTiledMapServiceLayer.FullExtent.YMin) ||
                        (minPoint.Y > this.baseTiledMapServiceLayer.FullExtent.YMax) ||
                        (minPoint.X > this.baseTiledMapServiceLayer.FullExtent.XMax))
                    {
                        this.SafeSetExtent(this.baseTiledMapServiceLayer.FullExtent);
                    }
                    else
                    {
                        this.SafeSetExtent(new Envelope(minPoint.X, minPoint.Y, maxPoint.X, maxPoint.Y));
                    }
                }
            }
            else if (this.mapSettingDataInfo.MapType == MapProviderType.CustomMap || this.baseMap.Extent == null)
            {
                this.SafeSetExtent(this.baseTiledMapServiceLayer.FullExtent);
            }

            this.currentMapType = this.mapSettingDataInfo.MapType;
        }

        /// <summary>
        /// 그냥 Extent를 바로 set하면 Extent Changing Extent Changed가 제대로 안 불리므로 이것을 통해 미리 필요한 조정을 한다.
        /// </summary>
        /// <param name="env">가고 싶은 Extent</param>
        /// <param name="prev">optional한 이전 Extent 지정</param>
        public virtual void SafeSetExtent(Envelope env, Envelope prev = null)
        {
            if (env == null)
                throw new ArgumentNullException("env");
            var map = this.baseMap;
            if (map == null)
                throw new InvalidOperationException("baseMap is null");
            map.Extent = env;
        }

        #endregion //Commonly

        #region GeometryHelper

        protected Point GetNewMapTypePoint(Point point, MapProviderType preMapType)
        {
            var latlng = GeometryHelper.ToGeographic(new Point(point.X, point.Y), preMapType);
            var newPoint = this.FromGeographic(latlng);
            return newPoint;
        }

        public Point FromGeographic(LatLng LatLng)
        {
            var type = this.mapSettingDataInfo.MapType;
            return GeometryHelper.FromGeographic(LatLng, type);
        }

        public Point FromGeographic(LatLng LatLng, string sType)
        {
            var type = (MapProviderType)Enum.Parse(typeof(MapProviderType), sType, true);
            return GeometryHelper.FromGeographic(LatLng, type);
        }

        public LatLng ToGeographic(Point mapPoint)
        {
            return GeometryHelper.ToGeographic(mapPoint, this.mapSettingDataInfo.MapType);
        }

        /// <summary>
        /// CustomMap은 사용자 입장에서는 좌측 상단을 (0,0)으로
        ///  받아들이기 때문에 조정해줄 필요가 있다.
        /// 사용자가 보기에 적당한 좌표 계산을 하여 반환한다.
        /// </summary>
        /// <param name="latLng">입력 위경도</param>
        /// <returns>변환된 MapPoint에 대응되는 Point</returns>
        public Point FromGeographicAdjustForUser(LatLng latLng)
        {
            var type = this.mapSettingDataInfo.MapType;
            if (type == MapProviderType.CustomMap)
            {
                var raw = GeometryHelper.FromGeographic(latLng, type);
                var layer = this.baseTiledMapServiceLayer;
                if (layer == null) throw new InvalidOperationException("layer is null");
                var fullExtent = layer.FullExtent;
                if (fullExtent == null) throw new InvalidOperationException("fullExtent is null");
                return new Point(raw.X, fullExtent.Height - 1 - raw.Y);
            }
            return GeometryHelper.FromGeographic(latLng, type);
        }

        /// <summary>
        /// 영역으로 지정되야 하는 객체의 WrapAroundPoint 지정
        /// </summary>
        /// <param name="xMin"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        protected double GetWrapAroundPoint(double xMin, double width)
        {
            var newX = xMin;

            if (this.baseMap.WrapAroundIsActive)
            {
                var basicExtent = this.baseTiledMapServiceLayer.FullExtent;
                var currentExtent = this.baseMap.Extent;

                //Set Basic Position
                var rotationCount = Math.Floor((xMin - basicExtent.XMin) / basicExtent.Width);

                var newMinX = basicExtent.XMin + basicExtent.Width * rotationCount;

                var basicX = xMin - (newMinX - basicExtent.XMin);

                rotationCount = Math.Floor((currentExtent.XMin - basicExtent.XMin) / basicExtent.Width);

                newX = (basicExtent.Width * rotationCount) + basicX;

                if (newX < currentExtent.XMin)
                {
                    if (currentExtent.XMin - newX > width/2)
                    {
                        if (newX + width < currentExtent.XMax)
                        {
                            var tmpNewX = (basicExtent.Width*(rotationCount + 1)) + basicX;

                            if(tmpNewX < currentExtent.XMax) newX = tmpNewX;
                        }
                    }
                }
            }

            return newX;
        }

        /// <summary>
        /// 단일 점의 WrapAround Point
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected double GetWrapAroundPoint(double x)
        {
            var newX = x;

            if (this.baseMap.WrapAroundIsActive)
            {
                var basicExtent = this.baseTiledMapServiceLayer.FullExtent;
                var currentExtent = this.baseMap.Extent;

                //Set Basic Position
                var rotationCount = Math.Floor((x - basicExtent.XMin) / basicExtent.Width);

                var newMinX = basicExtent.XMin + basicExtent.Width * rotationCount;

                var basicX = x - (newMinX - basicExtent.XMin);

                rotationCount = Math.Floor((currentExtent.XMin - basicExtent.XMin) / basicExtent.Width);

                newX = (basicExtent.Width*rotationCount) + basicX;

                if (newX < currentExtent.XMin)
                {
                    var tmpNewX = (basicExtent.Width * (rotationCount + 1)) + basicX;

                    if (tmpNewX < currentExtent.XMax) newX = tmpNewX;
                }
            }

            return newX;
        }

        protected double GetMapResolution()
        {
            return this.baseMap.Resolution;
        }

        #endregion 

        #region OnEventMethods

        /// <summary>
        /// baseMap이 backbufferBaseMap과 교체되는 경우 불리는 함수
        /// </summary>
        protected virtual void OnBaseMapChanged()
        {
            var eh = BaseMapChanged;
            if (eh != null)
                eh(this, null);
        }

        /// <summary>
        /// SearchedAddressInfoWindowManager 에서 생성된 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objectEventArgs"></param>
        protected virtual void InfoWindowManager_SearchedAddressSaveButtonClick(object sender, ObjectEventArgs objectEventArgs)
        {
            //Viewer 외부에서만 사용 되기 때문에 Base에서 호출 되고 외부에서 작동 한다.
        }

        #endregion

        #region 이벤트 핸들러 관리

        protected virtual void AddMapEventHandlers(Map map)
        {
            map.Progress += this.BaseMapOnProgress;
            map.ExtentChanging += this.BaseMapOnExtentChanging;
        }

        protected virtual void RemoveMapEventHandlers(Map map)
        {
            map.Progress -= this.BaseMapOnProgress;
            map.ExtentChanging -= this.BaseMapOnExtentChanging;
        }

        #endregion

        #endregion //Methods

        #region Events Handlers

        #region Map Event Handler

        protected virtual void BaseMapOnProgress(object sender, ProgressEventArgs progressEventArgs)
        {
            if (progressEventArgs.Progress == 100)
            {
                this.Dispatcher.BeginInvoke(new Action(this.BeginSmoothTransition));
            }
        }

        protected virtual void BaseMapOnExtentChanging(object sender, ExtentEventArgs extentEventArgs)
        {
            if (extentEventArgs.OldExtent == null || extentEventArgs.NewExtent == null)
                return;

            // ZoomIn/Out 할 때
            if (!NumberUtil.AreSame(extentEventArgs.OldExtent.Width, extentEventArgs.NewExtent.Width)
                || !NumberUtil.AreSame(extentEventArgs.OldExtent.Height, extentEventArgs.NewExtent.Height))
            {
                // ControlGraphic의 크기가 애니메이션 도중에도 바뀌게 하기 위함
                if (this.objectGraphicLayer != null)
                    this.objectGraphicLayer.ResetGeometryTransforms();
            }
        }

        #endregion //Map Event Handler

        #region Map Tiled Service Event Handlers

        /// <summary>
        /// ArcGIS MAP TILE INITALIZED
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tileLayer_Initialized(object sender, EventArgs e)
        {
            var initializedAction = new Action(() =>
            {
                if (!ReferenceEquals(this.baseTiledMapServiceLayer, sender))
                    return;

                if (this.eMapTileLoaded != null)
                    this.eMapTileLoaded(this, null);

                if (sender is CustomMapTiledService) return;
            });

            if (Dispatcher.CheckAccess())
            {
                initializedAction();
            }
            else
            {
                Dispatcher.BeginInvoke(initializedAction);
            }

            // 현재 Call Stack 위에서 아래 작업을 진행하면 안된다.
            Dispatcher.BeginInvoke(new Action(this.InitializeDefaultMapExtent));
        }

        /// <summary>
        /// Tile Initialize 실패시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tileLayer_InitializationFailed(object sender, EventArgs e)
        {
            this.FailedMapTileLoaded(e.ToString());
        }

        #endregion //Map Tiled Service Event Handlers

        #region Map Main Control Event Handler

        protected virtual void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        #endregion //Map Main Control Event Handler

        #endregion Events

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DisposeAction()
        {
            this.mainCanvas.Children.Clear();
            this.baseMap.Layers.Clear();
            this.baseGrid.Children.Clear();

            if (this.baseMap != null)
            {
                RemoveMapEventHandlers(this.baseMap);
            }
            if (this.backbufferBaseMap != null)
            {
                RemoveMapEventHandlers(this.backbufferBaseMap);
            }

            
            if (this.objectGraphicLayer != null)
            {
                /*
                foreach (var graphic in this.objectGraphicLayer.Graphics)
                {
                    this.DeleteGraphic(graphic as BaseGraphic);
                    /
                    graphic.MouseLeftButtonDown -= LocationGraphicOnMouseLeftButtonDown;
                    graphic.MouseLeftButtonUp -= LocationGraphicOnMouseLeftButtonUp;
                    graphic.PropertyChanged -= LocationGraphicOnPropertyChanged;
                    graphic.MouseMove -= LocationGraphicOnMouseMove;
                    graphic.MouseLeave -= LocationGraphicOnMouseLeave;
                     
                }
                */
                this.objectGraphicLayer.Graphics.Clear();
            }

            if (this.searchedAddressInfoWindowManager != null)
            {
                this.searchedAddressInfoWindowManager.eSearchedAddressSaveButtonClick -= InfoWindowManager_SearchedAddressSaveButtonClick;
            }

            this.Loaded -= OnLoaded;

            this.disposed = true;
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.mainCanvas.Dispatcher.CheckAccess())
                {
                    this.DisposeAction();
                    //this.Dispose(true);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(this.DisposeAction));
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BaseArcGISMap"/> class. 
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~BaseArcGISMap()
        {
            this.Dispose(false);
        }

        #endregion // IDisposable Members
    }
}
