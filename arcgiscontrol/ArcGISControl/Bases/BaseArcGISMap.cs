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
    /// Cient �� Server�� ��� ������ ����
    /// </summary>
    public partial class BaseArcGISMap : UserControl, IDisposable
    {
        #region Field

        #region Property & Status Field

        /// <summary>
        /// Map ��� (CU,VW,Edit,VIEW)
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
        /// Map Type ����
        /// </summary>
        protected MapProviderType currentMapType = MapProviderType.None;

        private bool disposed = false;

        #endregion Property & Status Field

        #region Tile Fields

        /// <summary>
        /// tile Load �� �� Set
        /// </summary>
        protected bool isInitializedMapTileService;

        /// <summary>
        /// Tile �� Refresh ���� Flag
        /// </summary>
        protected bool justTileRefresh = false;

        /// <summary>
        /// ��Ҹ� ���� ��ū
        /// </summary>
        protected CancellationTokenSource cancellationTokenSourceTileLoad;

        /// <summary>
        /// ��Ҹ� ���� ��ū
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
        /// Map ������ �ö�� Canvas 
        /// Popup ���� �ö� ����.
        /// </summary>
        protected Canvas mainCanvas;

        /// <summary>
        /// ErrorMessage ǥ���� ���� UserControl
        /// </summary>
        private ErrorMessageUserControl errorMessageUserControl;

        /// <summary>
        /// Camera Poup Control Manager
        /// Camera Poup ��ü �� �뵵
        /// </summary>
        protected CameraPopupControlManager cameraPopupControlManager;

        /// <summary>
        /// Sub Table Popup Manager
        /// LinkZone Click �� Splunk Control Manager
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
        /// Graphic Editor ��� Layer
        /// </summary>
        protected readonly GraphicsLayer editorGraphicsLayer;

        #endregion Map UI Elements
        
        #region baseMap double buffering ����
        /// <summary>
        /// basemap�� �ε巴�� ��ü�ϱ� ���� ����� ��ü. baseMap�� swap�ȴ�.
        /// </summary>
        protected Map backbufferBaseMap;

        /// <summary>
        /// baseMap�� �ε巴�� ��ü�ؾ��ϴ� ��Ȳ������ ����صΰ� Progress 100�� �Ǿ��� �� Ȯ���Ͽ� ó���Ѵ�.
        /// </summary>
        private bool needBackBufferSwap;

        /// <summary>
        /// �ε巯�� ��ü�� �ִϸ��̼� ���丮����.
        /// 
        /// �� ��ü���� ��ü�� �ٲ��. �̰��� �̿��� ���� �ִϸ��̼��� ����ϰų� ���� ������ üũ�� �� �� �ִ�.
        /// </summary>
        private Storyboard mapTransitionStoryboard;

        /// <summary>
        /// baseMap�� backbufferBaseMap�� ��ü�Ǵ� ��� �߻��ϴ� �̺�Ʈ
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
        /// ���� Map�� ���� ������
        /// </summary>
        protected MapSettingDataInfo mapSettingDataInfo;

        #endregion MapData

        #endregion // Field

        #region Events

        /// <summary>
        /// Make Key ���� ���н� 
        /// </summary>
        public event EventHandler<LicenseKeyNotAllowedEventArgs> eLicenseKeyNotAllowed;

        /// <summary>
        /// Map Loading Erring �� �߻�
        /// </summary>
        public event EventHandler<MapLoadingErrorEventArgs> eMapLoadingErrorOccured;

        /// <summary>
        /// Map Loading�� ��� ������ Event ����
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

            // [yjjang] : CameraPopupControlManager �۾�.
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
                // ����۴� ���ܳ��� ��ü�� ���� �����ش�.
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

                // ȭ�� ��ȯ �ִϸ��̼�
                var animationDuration = new Duration(ArcGISConstSet.SmoothTransitionAnimationTimeSpan);

                var fadeInAnimation = new DoubleAnimation(0, 1, animationDuration);
                Storyboard.SetTarget(fadeInAnimation, this.baseMap);
                Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(Map.OpacityProperty));
                fadeInAnimation.FillBehavior = FillBehavior.HoldEnd;

                var capturedStoryboard = new Storyboard();
                this.mapTransitionStoryboard = capturedStoryboard;
                // fadeOutAnimation���� �տ� ��Ÿ�� �ִ� ���� ���� ������Ƿ� �ڿ� �ִ� ���� ���� ���������� �־�� �Ѵ�.
                capturedStoryboard.Children.Add(fadeInAnimation);

                capturedStoryboard.Completed += (s, e) =>
                {
                    // �� �̻� �� �۾��� �������� �ʴ´�.
                    if (!ReferenceEquals(capturedStoryboard, this.mapTransitionStoryboard))
                        return;

                    this.AfterTransitionStoryboard();
                };

                capturedStoryboard.Begin();
            }
        }

        protected virtual void CleanupTransitionStoryboard()
        {
            // �ִϸ��̼� ����
            this.backbufferBaseMap.BeginAnimation(Map.OpacityProperty, null);
            this.baseMap.BeginAnimation(Map.OpacityProperty, null);
            this.baseMap.Opacity = 1.0;
            this.backbufferBaseMap.Opacity = 1.0;

            // ���� �ִ� ���� Ÿ�� �� ���̾�� ����
            this.backbufferBaseMap.Layers.Clear();

            // Z ����, ���̱� ���µ��� ��Ģ��� �ǵ���
            this.backbufferBaseMap.Visibility = Visibility.Collapsed;
        }

        protected virtual void AfterTransitionStoryboard()
        {
            this.CleanupTransitionStoryboard();
        }

        /// <summary>
        /// map Tile ������ ���� 
        /// BingMap�� �ø��� 
        /// �׳� map�� �ø��� Ȯ�� �Ѵ�.
        /// </summary>
        protected void LoadMapTileService(bool smoothTransition = false)
        {
            if (this.justTileRefresh)
                this.ClearTiledMapLayers();
            else if (!smoothTransition)
                this.ReleaseMap();
            else
            {
                // ��ü ��� ����
                // Task Invariant:
                // baseMap�� ���̴� -1�̰�, backbufferBaseMap�� ���̴� -2�̴�.
                //
                // �ʱ� ����:
                // front buffer(baseMap)�� -1 ���̿� �ְ�, back buffer(backbufferBaseMap)�� -2 ���̿� �ִ�. front buffer�� ���̴� ����./
                //
                // ��ü ����:
                // ��ü ��û�� ������ �� �� ���̰� �����ϰ� back buffer���� ���� Opacity�� 0���� �����Ѵ�.
                // back buffer�� front buffer�� ���̸� ���� �ٲ��ְ� baseMap�� backbufferBaseMap ��ü�� swap�Ѵ�.
                // swap�ϸ鼭 BaseMapChanged �̺�Ʈ�� �߻������ش�. TODO: �ʿ��ϸ� BaseMapChanging �̺�Ʈ�� �߰��Ѵ�.
                //
                // ��ü �����ֱ�:
                // �ε尡 �Ϸ�Ǿ� Progress�� 100�� �Ǹ� back buffer���� ���� Opacity Fade In �ִϸ��̼��� �����Ѵ�.
                // front buffer���� ���� �����.

                // ���� �ִϸ��̼� ���� ���� üũ�ϰ� null�� �����Ͽ� Completed ó���� ���´�.
                var lastStoryboard = Interlocked.Exchange(ref this.mapTransitionStoryboard, null);
                if (lastStoryboard != null)
                {
                    lastStoryboard.Remove();
                    this.CleanupTransitionStoryboard();
                }

                // ��� �����ְ� �� �׷��� ���̾� ����. ���� ����� �۵����� �ʴ� ���� �ǵ��̴�. �����ֱ⸸ �� �͵�.
                // �뵵�� �ε巴�� Transition �Ǵ� ��Ȳ���� ������ ���̴� Graphic���� ������� �ʰ� ������ ���̵��� �ϰ� �ϱ� ���� ���̴�.
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
        /// Map Tile Service�� �ʱ�ȭ
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
        /// Map ����
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
        /// Digital Zoom�� ���� �Ѵ�.
        /// </summary>
        protected void SetDigitalZoom(int originalLevel)
        {
            this.baseMap.MinimumResolution = 1 / Math.Pow(2, originalLevel + 100);
        }
       
        /// <summary>
        /// ���� Extent�� LAT LNG ���·� ��ȯ
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
        /// Map�� ���̴� ������ �ٲپ� �ش�
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
        /// �׳� Extent�� �ٷ� set�ϸ� Extent Changing Extent Changed�� ����� �� �Ҹ��Ƿ� �̰��� ���� �̸� �ʿ��� ������ �Ѵ�.
        /// </summary>
        /// <param name="env">���� ���� Extent</param>
        /// <param name="prev">optional�� ���� Extent ����</param>
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
        /// CustomMap�� ����� ���忡���� ���� ����� (0,0)����
        ///  �޾Ƶ��̱� ������ �������� �ʿ䰡 �ִ�.
        /// ����ڰ� ���⿡ ������ ��ǥ ����� �Ͽ� ��ȯ�Ѵ�.
        /// </summary>
        /// <param name="latLng">�Է� ���浵</param>
        /// <returns>��ȯ�� MapPoint�� �����Ǵ� Point</returns>
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
        /// �������� �����Ǿ� �ϴ� ��ü�� WrapAroundPoint ����
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
        /// ���� ���� WrapAround Point
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
        /// baseMap�� backbufferBaseMap�� ��ü�Ǵ� ��� �Ҹ��� �Լ�
        /// </summary>
        protected virtual void OnBaseMapChanged()
        {
            var eh = BaseMapChanged;
            if (eh != null)
                eh(this, null);
        }

        /// <summary>
        /// SearchedAddressInfoWindowManager ���� ������ �̺�Ʈ �ڵ鷯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objectEventArgs"></param>
        protected virtual void InfoWindowManager_SearchedAddressSaveButtonClick(object sender, ObjectEventArgs objectEventArgs)
        {
            //Viewer �ܺο����� ��� �Ǳ� ������ Base���� ȣ�� �ǰ� �ܺο��� �۵� �Ѵ�.
        }

        #endregion

        #region �̺�Ʈ �ڵ鷯 ����

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

            // ZoomIn/Out �� ��
            if (!NumberUtil.AreSame(extentEventArgs.OldExtent.Width, extentEventArgs.NewExtent.Width)
                || !NumberUtil.AreSame(extentEventArgs.OldExtent.Height, extentEventArgs.NewExtent.Height))
            {
                // ControlGraphic�� ũ�Ⱑ �ִϸ��̼� ���߿��� �ٲ�� �ϱ� ����
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

            // ���� Call Stack ������ �Ʒ� �۾��� �����ϸ� �ȵȴ�.
            Dispatcher.BeginInvoke(new Action(this.InitializeDefaultMapExtent));
        }

        /// <summary>
        /// Tile Initialize ���н�
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
