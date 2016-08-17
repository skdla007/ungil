using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using ArcGISControl.DataManager;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl;
using ArcGISControl.Bases;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Interface;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Parsers;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace ArcGISControl
{
    using ArcGISInternalHack;
    using Language;
    using ArcGISControl.Command;

    /// <summary>
    /// ArcGISClientViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArcGISClientViewer : BaseArcGISMap, IDisposable
    {
        #region Field

        /// <summary>
        /// Map Data Service IP를 Map을 실행하는 Parent에서 받아 온다
        /// </summary>
        private string mapDataServerIp;

        /// <summary>
        /// Map Data Service Port를 Map을 실행하는 Parent에서 받아 온다
        /// </summary>
        private int mapDataServerPort;

        /// <summary>
        /// Map Service의 Data 부분 Handling
        /// </summary>
        private MapDataServiceHandler mapDataServiceHandler;

        /// <summary>
        /// map data service handler에 작업을 태깅할 token. 마지막으로 작업을 보냈을 때 사용한 토큰이 여기 들어간다.
        /// </summary>
        private object mapDataServiceToken;

        private Cursor _OldCursor;

        private UndoManager _UndoManager;
        public UndoManager UndoManager
        {
            get { return _UndoManager; }
        }

        private ClipBoardManager _clipBoardManager;

        private bool _IsPanning = false;

        public bool IsPanning
        {
            set { _IsPanning = value; }
            get { return _IsPanning; }
        }

        public bool IsPanningToggleBtn
        {
            get;
            set;
        }

        protected HistoryManager historyManager;

        public int HistorySaveInterval
        {
            get
            {
                return historyManager == null ? 0 : this.historyManager.AutoSavingSeconds;
            }

            set
            {
                if (this.historyManager == null)
                {
                    return;
                }

                this.historyManager.AutoSavingSeconds = value;
            }
        }

        /// <summary>
        /// Zoom-in/out 시 Current Level 변경
        /// </summary>
        protected double mapLevel = -1;

        public double MapLevel
        {
            get { return this.mapLevel; }

            protected set
            {
                this.mapLevel = value;

                if (this.eLevelChanged != null)
                {
                    this.eLevelChanged(this, new LevelChangedEventArgs { Level = this.mapLevel });
                }
            }
        }

        /// <summary>
        /// Linked Map에 의해 발생하는 히스토리를 관리하는 객체
        /// </summary>
        private MapHistory linkedMapHistory;

        /// <summary>
        /// Data Play back 사용시 Timer 구동
        /// </summary>
        private System.Timers.Timer dataPlayBackTimer;

        /// <summary>
        /// Splunk 사용 여부 체크
        /// </summary>
        private bool useSplunk = true;
        public bool UseSplunk
        {
            set
            {
                this.useSplunk = value;
            }
        }

        private Color mapBackgroundColor = Colors.White;
        public Color MapBackgroundColor
        {
            get
            {
                if (this.mapSettingDataInfo == null) return Colors.White;
                return this.mapSettingDataInfo.MapBgColor;
            }

            set
            {
                if (this.mapSettingDataInfo != null)
                {
                    this.mapSettingDataInfo.MapBgColor = value;
                }
                
                this.Background = new SolidColorBrush(value);
            }
        }

        #region Camera Field

        /// <summary>
        /// CAMERA RESIZE, MOVE
        /// </summary>
        private MapPoint mapPointByClick = null;
        [Flags]
        private enum ResizingFlag { None = 0, X = 1, Y = 2, Width = 4, Height = 8 }
        private ResizingFlag resizingFlag = 0;
        private const int ResizingBorderWidth = 15;

        /// <summary>
        /// 선택된 Camera Preset Vertex Graphic
        /// </summary>
        private VertexIconGraphic selectedVertexGraphic;

        /// <summary>
        /// Camera Brush 생성을 위해
        /// </summary>
        private ImageBrushGraphicManager imageBrushGraphicManager;

        /// <summary>
        /// Graphic Editor
        /// 현재는 Memo이외에 Object들이 사용하고 있음
        /// </summary>
        private GraphicGeometryEditor editor;

        /// <summary>
        /// Graphic Context Menu
        /// </summary>
        private GraphicContextMenu _GraphicContextMenu;

        /// <summary>
        /// GraphicEditor 객체 관리하는 아이
        /// 현재는 Memo만 쓰고 있음 : 2014.05.29
        /// </summary>
        private GraphicEditorManager graphicEditorManager;

        /// <summary>
        /// Hide All/Show All Flag
        /// </summary>
        private bool isHideAllCamera;

        /// <summary>
        /// 공통 Contorl
        /// </summary>
        private IArcGISControlAPI arcGISControlApi;

        /// <summary>
        /// VIEWER Control
        /// </summary>
        private IArcGISControlViewerAPI arcGisControlViewerApi;
        
        /// <summary>
        /// 이부분은 VIEWER쪽으로 뺴도록 한다.
        /// </summary>
        protected ProcessingUserControl processingIcon;

        #endregion Camera Field
        
        #region Status Fields

        /// <summary>
        /// BaseMapOnExtentChanging 함수에서 다시 Zoom을 불러서 재귀호출이 일어나는 것을 막기 위한 카운터.
        /// Zoom을 다시 부를 때마다 counter를 계산해준다.
        /// </summary>
        int extentChangingRecurseCounter;

        /// <summary>
        /// Map이 처음 준비되면 이동할 범위를 나타낸다. null이면 기본 위치로 이동
        /// DP에서 맵이 로드될 때 좌표 변경 싱크가 타일 로드보다 일찍 전달되어 처리되지 못해 맵의 위치가 틀어지는 현상을 
        /// 해결하기 위해서도 사용됨.
        /// </summary>
        private Envelope initialExtent;

        private string initialBookmarkName;

        private string initialObjectName;

        internal static DependencyPropertyKey HomeExtentPropertyKey = DependencyProperty.RegisterReadOnly("HomeExtent", typeof(Envelope),
            typeof(ArcGISClientViewer), new PropertyMetadata(null));

        public static DependencyProperty HomeExtentProperty = HomeExtentPropertyKey.DependencyProperty;

        /// <summary>
        /// 북마크에 설정된 HomeExtent readonly dependency property
        /// </summary>
        public Envelope HomeExtent
        {
            get
            {
                return (Envelope)this.GetValue(HomeExtentProperty);
            }
        }

        /// <summary>
        /// 현재 Map의 Extent 의 최대 값을 Point로 받는다
        /// </summary>
        /// <returns></returns>
        public Point MaxExtentToPoint
        {
            get { return new Point(this.baseMap.Extent.XMax, this.baseMap.Extent.YMax); }
        }

        /// <summary>
        /// 현재 Min의 Extent 의 최대 값을 Point로 받는다
        /// </summary>
        public Point MinExtentToPoint
        {
            get { return new Point(this.baseMap.Extent.XMin, this.baseMap.Extent.YMin); }
        }

        /// <summary>
        /// 북마크에 있는 HomeExtent와 Map 메타 데이터에 있는 HomeExtent를 조합하여 최종적으로 사용할 HomeExtent를 얻어온다.
        /// </summary>
        public Envelope CombinedHomeExtent
        {
            get
            {
                var mapMetadata = this.mapSettingDataInfo;
                if (mapMetadata != null)
                {
                    var extent = mapMetadata.HomeExtent;
                    if (extent.HasValue)
                    {
                        return new Envelope(
                            extent.Value.XMin,
                            extent.Value.YMin,
                            extent.Value.XMax,
                            extent.Value.YMax);
                    }
                }
                return this.HomeExtent;
            }
        }

        /// <summary>
        /// VW : Object가 모두 로드 되었을때 true
        /// CU : 무조건 true
        /// </summary>
        private bool isPrivateObjectLoaded;

        /// <summary>
        /// MapServiceHandler를 통하여 받아온 Objects 가 모두 Load되면 true
        /// </summary>
        private bool isPublicObjectLoaded;

        /// <summary>
        /// map이 순차로 이동해야할 맵 이름들의 목록
        /// </summary>
        private ConcurrentQueue<string> mapTransitionChain;

        #endregion Status Fields

        #region UI Fields

        private CameraOverlayControl cameraOverlayControl;

        private PolygonControlGraphic<CameraOverlayControl> cameraOverlayGraphic;

        #endregion UI Fields

        #endregion

        private CommandTransform _CommandTransform;
        private List<ESRI.ArcGIS.Client.Geometry.Geometry> _OriginGeometryList;

        #region Events

        /// <summary>
        /// Object Data Load 완료 후 
        /// </summary>
        public event EventHandler<EventArgs> eObjectLoaded;

        private void RaiseObjectLoadedEvent()
        {
            var objectEvent = eObjectLoaded;
            if (objectEvent != null)
            {
                objectEvent(this, new EventArgs());
            }
        }

        /// <summary>
        /// Map의 변화가 생길때 마다 호출 된다.
        /// </summary>
        public event EventHandler<EventArgs> eHaveSavingMapData;

        /// <summary>
        /// Map의 Extet가 변경 된 후 발생
        /// </summary>
        public event EventHandler<MapExtentChangeEventArgs> eMapExtentChanged;

        public void OnEMapExtentChanged(MapExtentChangeEventArgs e)
        {
            var handler = eMapExtentChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Map 이 Reload 될때 발생
        /// </summary>
        public event EventHandler<EventArgs> eMapReloaded;

        /// <summary>
        /// Level 이 변경될때 발생
        /// </summary>
        public event EventHandler<LevelChangedEventArgs> eLevelChanged;

        #endregion Events

        #region Construction

        /// <summary>
        /// </summary>
        /// <param name="mapDataServiceIp"></param>
        /// <param name="mapDataServicePort"></param>
        /// <param name="controlType"></param>
        /// <param name="arcGisControlApi"></param>
        public ArcGISClientViewer(string mapDataServiceIp, int mapDataServicePort, MapControlType controlType, IArcGISControlAPI arcGisControlApi)
            : base(controlType)
        {
            InitializeComponent();
            
            Resource_ArcGISControl_ArcGISClientViewer.Culture = this.Dispatcher.Thread.CurrentUICulture;
            Resource_ArcGISControl_Properties.Culture = this.Dispatcher.Thread.CurrentUICulture;
            Resource_ArcGISControl_UIControl.Culture = this.Dispatcher.Thread.CurrentUICulture;

            this.mapDataServerIp = mapDataServiceIp;
            this.mapDataServerPort = mapDataServicePort;
            this.arcGISControlApi = arcGisControlApi;

            if(arcGISControlApi is IArcGISControlViewerAPI)
            {
                this.arcGisControlViewerApi = arcGisControlApi as IArcGISControlViewerAPI;
            }

            this.InitializeArcGISClientViewer();

            _clipBoardManager = new ClipBoardManager();
            _UndoManager = new UndoManager(base.objectGraphicLayer, editor, this.UnSelectGraphicObject);
        }

        public ArcGISClientViewer(string mapDataServiceIp, int mapDataServicePort, MapControlType controlType, IArcGISControlAPI arcGisControlApi, Rect initialExtent)
            : this(mapDataServiceIp, mapDataServicePort, controlType, arcGisControlApi)
        {
            this.initialExtent = new Envelope(initialExtent.Left, initialExtent.Top, initialExtent.Right, initialExtent.Bottom);
        }

        #endregion Construction

        #region Method

        protected virtual void OnEMapReloaded()
        {
            var handler = eMapReloaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected override void OnBaseMapChanged()
        {
            base.OnBaseMapChanged();

            if (searchedAddressInfoWindowManager != null) this.searchedAddressInfoWindowManager.ChangeMap(this.baseMap);

            if(locationInfoWindowManager != null) this.locationInfoWindowManager.ChangeMap(this.baseMap);
        }

        /// <summary>
        /// 선택한 아이템 복사
        /// </summary>
        public void SelectedItemCopy()
        {
            List<BaseMapObjectInfoData> ObjectDataInfo = new List<BaseMapObjectInfoData>();

            foreach (BaseGraphic graphic in selectedGraphicList)
            {
                //카메라일 경우 아이콘 선택만 복사 허용
                if (graphic.Type == MapObjectType.CameraPreset || graphic.Type == MapObjectType.CameraVideo)
                    continue;

                BaseMapObjectInfoData ClipBoardDataInfo = null;

                if (graphic.Type == MapObjectType.CameraIcon)
                {
                    ClipBoardDataInfo = cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);
                }
                else if (graphic.Type == MapObjectType.SplunkControl || graphic.Type == MapObjectType.SplunkIcon || graphic.Type == MapObjectType.Splunk ||
                    graphic.Type == MapObjectType.SplunkChartControl || graphic.Type == MapObjectType.SplunkTableControl)
                {
                    ClipBoardDataInfo = savedSplunkObjectDataManager.GetObjectDataByObjectID(graphic.ObjectID);
                }
                else if (graphic.Type == MapObjectType.SearchedAddress)
                {
                    ClipBoardDataInfo = searchAddressGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);
                }
                else if (graphic.Type == MapObjectType.MemoTextBox)
                {
                    ClipBoardDataInfo = memoObjectDataManager.GetMemoObjectDataInfo(graphic.ObjectID);
                }
                else if (graphic.Type == MapObjectType.UniversalControl)
                {
                    ClipBoardDataInfo = universalObjectDataManager.GetDataInfo(graphic.ObjectID);
                }
                else
                {
                    ClipBoardDataInfo = publicGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID, graphic.Type);
                }

                if (ClipBoardDataInfo != null && ObjectDataInfo.Contains(ClipBoardDataInfo) == false)
                    ObjectDataInfo.Add(ClipBoardDataInfo);
            }

            _clipBoardManager.Copy(ObjectDataInfo);
        }

        /// <summary>
        /// 메모리에 있는 복사된 아이템 붙여넣기
        /// </summary>
        public void PasteItem()
        {
            List<BaseMapObjectInfoData> PastedGraphicDataInfoList = _clipBoardManager.Paste(this.AddMapObject, base.baseMap);
            // Undo / Redo - AddCommand History 추가
            this.AddCommandToHistory(new CommandAdd(PastedGraphicDataInfoList, this.DeleteMapObjectData, this.AddMapObject, this.RaiseObjectAddedEvent));
            // 모든 카메라 Refresh
            this.RefreshAllCameraVideoRect();

            // 붙여넣기된 Graphic 좌측 PlaceList에 표시하기 위해 이벤트 발생
            PastedGraphicDataInfoList.ForEach(Item =>
            {
                Item.IsUndoManage = true;
                this.RaiseObjectAddedEvent(Item);
                Item.IsUndoManage = false;
            });
        }

        #region CommandToHistory
        private void AddCommandToHistory(CommandBase command)
        {
            _UndoManager.AddCommandToHistory(command);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void InitializeArcGISClientViewer()
        {
            this.isHideAllCamera = false;
            this.isPrivateObjectLoaded = false;
            this.isPublicObjectLoaded = false;

            this.linkedMapHistory = new MapHistory();

            if (this.isEditMode)
            {
                if (this.editor == null)
                {
                    this.editor = new GraphicGeometryEditor(editorGraphicsLayer);
                    this.editor.eGraphicGeometryChanged += EditorOnEGraphicGeometryChanged;
                    this.editor.eMouseRightButtonUp += this.Editor_eMouseRightButtonUp;
                    this.editor.eMouseLeftButtonDown += this.Editor_eMouseLeftButtonDown;
                }
            }

            if (this.graphicEditorManager == null)
            {
                this.graphicEditorManager = new GraphicEditorManager();
            }

            this.mapDataServiceHandler = new MapDataServiceHandler(this.mapDataServerIp, this.mapDataServerPort);
            this.mapDataServiceHandler.eGetMapDataProcessCompleted += MapDataServiceHandlerOnEGetMapDataProcessCompleted;
            
            this.cameraOverlayControl= new CameraOverlayControl(arcGisControlViewerApi);
            this.cameraOverlayControl.PreviewMouseLeftButtonDown += cameraOverlayControl_PreviewMouseLeftButtonDown;

            if (this.workStationControlManager != null)
            {
                this.workStationControlManager.onGoLinkedMap += WorkStationControlManager_OnGoLinkedMap;
                this.workStationControlManager.onShowSearchViewControl += WorkStationControlManager_OnShowSearchViewControl;
            }
            
            if (this.cameraPopupControlManager != null)
            {
                this.cameraPopupControlManager.eCloseButtonClicked += CameraPopupManager_eCloseButtonClicked;
                this.cameraPopupControlManager.ePresetListSelectionChanged += CameraPopup_ePresetListSelectionChanged;
                this.cameraPopupControlManager.eControlOpend += CameraPopup_eControlOpend;
            }
            
            if (this.savedSplunkObjectDataManager != null)
            {
                this.savedSplunkObjectDataManager.eColorChanged += this.SavedSplunkObjectDataManager_ColorChanged;
            }

            if(this.publicGraphicDataManager != null)
            {
                this.publicGraphicDataManager.eColorChanged += this.PublicGraphicDataManager_ColorChanged;
                this.publicGraphicDataManager.eWorkstationDataChanged += this.PublicGraphicDataManager_WorkstationDataChanged;
                this.publicGraphicDataManager.homeBookmarkChanged += this.PublicGraphicDataManager_homeBookmarkChanged;
            }

            if (this.dynamicSplunkControlManager != null)
            {
                this.dynamicSplunkControlManager.DataRowClick += this.dynamicSplunkControlManagerDataRowClick;
                this.dynamicSplunkControlManager.ButtonColumnCellClick += this.dynamicSplunkControlManagerButtonColumnCellClick;
            }

            if (this.splunkPopupControlManager != null)
            {
                this.splunkPopupControlManager.eTableCellClick += this.splunkPopupControlManager_eTableCellClick;
            }
            
            this.processingIcon = new ProcessingUserControl(){ Visibility = Visibility.Collapsed };

            this.baseGrid.Children.Add(this.processingIcon);

            this.imageBrushGraphicManager = new ImageBrushGraphicManager(this.objectGraphicLayer, this.arcGISControlApi);

            this.historyManager = new HistoryManager(this);

            this.mapSplTimer = new Timer();
            this.mapSplTimer.Elapsed += this.MapSplTimerOnElapsed;

            _GraphicContextMenu = new GraphicContextMenu(Application.Current.MainWindow);
            _GraphicContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            _GraphicContextMenu.Name = "GraphicContextMenu";
            _GraphicContextMenu.Select += this.GraphicContextMenu_Select;
            _GraphicContextMenu.DeSelect += this.GraphicContextMenu_DeSelect;
            _GraphicContextMenu.Delete += this.GraphicContextMenu_Delete;
            _GraphicContextMenu.Copy += this.GraphicContextMenu_Copy;
            _GraphicContextMenu.Lock += this.GraphicContextMenu_Lock;
            _GraphicContextMenu.UnLock += this.GraphicContextMenu_UnLock;
        }

        private bool isDataPlaybackForLink;
        private bool isPlayingDataPlaybackForLink;
        private DateTime dataPlaybackTimeForLink;

        public EventHandler<ChangeLinkedMapEventArgs> eChangeLinkedMap;

        private void RaiseChangeLinkedMapEvent(ChangeLinkedMapEventArgs e)
        {
            var handler = eChangeLinkedMap;
            if (handler != null) handler(this, e);
        }
        
        /// <summary>
        /// LinkZone Double Click 으로 연결된 Linked map으로 이동
        /// </summary>
        /// <param name="id">저장된 map의 ID</param>
        /// <param name="extent">이동할 범위. null이면 기본 위치를 사용</param>
        /// <param name="smoothTransition">애니메이션으로 fade 화면 전환되어야할 경우 true</param>
        /// <param name="linkedMapBookmarkName"></param>
        private bool ChangeSelectedLinkedMap(string id, Envelope extent, bool smoothTransition = false, string linkedMapBookmarkName = null, string linkedMapObjectName = null)
        {
            if (!this.isPublicObjectLoaded || !this.isPrivateObjectLoaded)
            {
                return false;
            }

            this.splunkPopupControlManager.Hide();
            this.dynamicSplunkControlManager.Hide();
            this.workStationControlManager.Hide();

            MapSettingDataInfo mapSettingDataInfo = null;

            if (this.arcGISControlApi != null)
                mapSettingDataInfo = this.arcGISControlApi.GetMapSetting(id);

            if (mapSettingDataInfo != null && !string.IsNullOrEmpty(mapSettingDataInfo.ID))
            {
                
                this.Dispatcher.Invoke(new Action(() =>
                                                      {
                                                          this.isDataPlaybackForLink =
                                                              this.IsDataPlayBackMode;
                                                          this.isPlayingDataPlaybackForLink =
                                                              this.IsDoingDataPlayBack;
                                                          this.dataPlaybackTimeForLink =
                                                              this.DataPlayBackPosition;
                                                          this.isSkipMapSplTimerStart =
                                                              this.IsDataPlayBackMode;

                                                          this.eObjectLoaded += ArcGISClientViewer_eObjectLoaded;

                                                          this.initialExtent = extent;
                                                          this.initialBookmarkName = linkedMapBookmarkName;
                                                          this.initialObjectName = linkedMapObjectName;
                                                          this.mapSettingDataInfo = mapSettingDataInfo;
                                                          this.MapBackgroundColor = mapSettingDataInfo.MapBgColor;
                                                          this.RefreshMapTiles(smoothTransition);

                                                          this.RaiseChangeLinkedMapEvent(new ChangeLinkedMapEventArgs { MapId = mapSettingDataInfo.ID });
                                                      }));

                return true;
            }

            return false;
        }

        private void ArcGISClientViewer_eObjectLoaded(object sender, EventArgs e)
        {
            this.eObjectLoaded -= ArcGISClientViewer_eObjectLoaded;

            this.Dispatcher.BeginInvoke(new Action(() =>
                                                       {
                                                           if (this.isDataPlaybackForLink)
                                                           {
                                                               this.TurnOnPlayBackMode(dataPlaybackTimeForLink);

                                                               if (this.isPlayingDataPlaybackForLink)
                                                               {
                                                                   this.PlayData();
                                                               }
                                                           }
                                                       }));
        }

        private void GraphicContextMenuShow(BaseGraphic targetGraphic)
        {
            if (this.IsConsoleMode || targetGraphic.Type == MapObjectType.CameraNameTextBox || targetGraphic.Type == MapObjectType.CameraPresetPlus) return;

            // ContextMenu의 대상 Graphic이 선택된 Graphic일 경우
            if(selectedGraphicList.Contains(targetGraphic))
            {
                this.GraphicContextMenuShow(selectedGraphicList);
                return;
            }

            Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
            //Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);

            _GraphicContextMenu.Left = mousePositionInApp.X + 10;
            _GraphicContextMenu.Top = mousePositionInApp.Y + 20;
            _GraphicContextMenu.GraphicSelected = false;
            _GraphicContextMenu.SetTargetGraphic(targetGraphic);
            _GraphicContextMenu.Visibility = System.Windows.Visibility.Visible;

            if (_GraphicContextMenu.IsLoaded == false)
                _GraphicContextMenu.Show();
        }

        private void GraphicContextMenuShow(List<BaseGraphic> targetGraphic)
        {
            if (this.IsConsoleMode) return;

            Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
            //Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);

            _GraphicContextMenu.Left = mousePositionInApp.X + 10;
            _GraphicContextMenu.Top = mousePositionInApp.Y + 20;
            _GraphicContextMenu.GraphicSelected = true;
            _GraphicContextMenu.SetTargetGraphic(targetGraphic);
            _GraphicContextMenu.Visibility = System.Windows.Visibility.Visible;

            if (_GraphicContextMenu.IsLoaded == false)
                _GraphicContextMenu.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isZoomIn"></param>
        public void ChangeMapExtentByMousePoint(bool isZoomIn)
        {
            //Zoom Out
            var newScreenPoint = Mouse.GetPosition(baseMap);
            var newMapPoint = baseMap.ScreenToMap(newScreenPoint);

            try
            {
                //if (baseMap.WrapAroundIsActive)
                //    newMapPoint = ESRI.ArcGIS.Client.Geometry.Geometry.NormalizeCentralMeridian(newMapPoint) as ESRI.ArcGIS.Client.Geometry.MapPoint;

                if (!isZoomIn)
                {
                    var minX = this.baseMap.Extent.XMin - (newMapPoint.X - this.baseMap.Extent.XMin);
                    var minY = this.baseMap.Extent.YMin - (newMapPoint.Y - this.baseMap.Extent.YMin);
                    var maxX = this.baseMap.Extent.XMax + (this.baseMap.Extent.XMax - newMapPoint.X);
                    var maxY = this.baseMap.Extent.YMax + (this.baseMap.Extent.YMax - newMapPoint.Y);

                    this.baseMap.ZoomTo(new Envelope(minX, minY, maxX, maxY));
                }
                else
                {
                    var minX = this.baseMap.Extent.XMin + (newMapPoint.X - this.baseMap.Extent.XMin) / 2;
                    var minY = this.baseMap.Extent.YMin + (newMapPoint.Y - this.baseMap.Extent.YMin) / 2;
                    var maxX = this.baseMap.Extent.XMax - (this.baseMap.Extent.XMax - newMapPoint.X) / 2;
                    var maxY = this.baseMap.Extent.YMax - (this.baseMap.Extent.YMax - newMapPoint.Y) / 2;

                    this.baseMap.ZoomTo(new Envelope(minX, minY, maxX, maxY));
                }
            }
            catch (NullReferenceException exception)
            {
                InnowatchDebug.Logger.WriteInfoLogAndTrace(exception.ToString());   
            }
        }

        /// <summary>
        /// Object Data DB에서 받아온다. 그리고 그후에 Map에 표출 한다.
        /// </summary>
        private void InitializeObjectDatas()
        {
            if (this.justTileRefresh || this.mapSettingDataInfo == null ||
                string.IsNullOrEmpty(this.mapSettingDataInfo.ID))
            {
                if (this.isEditMode)
                {
                    this.historyManager.SaveHistory();
                }
                return;
            }

            if(!this.IsConsoleMode)
            {
                this.isPrivateObjectLoaded = true;
            }

            this.isPublicObjectLoaded = false;

            this.mapDataServiceToken = new Object();
            this.mapDataServiceHandler.StartGetMapFeatureData(this.mapSettingDataInfo.ID, this.mapDataServiceToken);
        }

        protected override void AddMapEventHandlers(Map map)
        {
            base.AddMapEventHandlers(map);

            map.Loaded += this.BaseMapOnLoaded;
            map.MouseDown += this.BaseMapOnMouseDown;
            map.MouseLeftButtonUp += this.BaseMapOnMouseLeftButtonUp;
            map.MouseMove += this.BaseMapOnMouseMove;
            map.PreviewMouseWheel += this.BaseMapOnPreviewMouseWheel;
            map.PreviewKeyUp += BaseMapOnPreviewKeyUp;
            map.PreviewKeyDown += BaseMapOnPreviewKeyDown;
            map.PreviewMouseDown += BaseMapOnPreviewMouseDown;
            map.ExtentChanged += this.BaseMapOnExtentChanged;
        }

        protected override void RemoveMapEventHandlers(Map map)
        {
            base.RemoveMapEventHandlers(map);

            map.Loaded -= this.BaseMapOnLoaded;
            map.MouseDown -= this.BaseMapOnMouseDown;
            map.MouseLeftButtonUp -= this.BaseMapOnMouseLeftButtonUp;
            map.MouseMove -= this.BaseMapOnMouseMove;
            map.PreviewMouseWheel -= this.BaseMapOnPreviewMouseWheel;
            map.PreviewKeyUp -= BaseMapOnPreviewKeyUp;
            map.PreviewKeyDown -= BaseMapOnPreviewKeyDown;
            map.PreviewMouseDown -= BaseMapOnPreviewMouseDown;
            map.ExtentChanged -= this.BaseMapOnExtentChanged;
        }

        protected override void FailedMapTileLoaded(string message)
        {
            base.FailedMapTileLoaded(message);

            if (this.processingIcon != null) this.processingIcon.Visibility = Visibility.Collapsed;
        }

        private void ReleaseDatas()
        {
            if (this.isEditMode)
            {
                this.historyManager.StopManagement();
            }

            this.ReleaseAllSelectedObject();

            this.ReleasePlayBackDatas();

            this.ClearSplunkObjectDatas();

            this.ClearObjectDatas();

            this.ClearPopupControls();

            if (this.editor != null) this.editor.Stop();
        }

        protected override void ReleaseMap(bool preserveMap = false)
        {
            if (this.isEditMode)
            {
                this.historyManager.ClearHistory();
            }

            this.StopMapSpl();

            base.ReleaseMap(preserveMap);

            this.ReleaseDatas();

            this.ReleasePlayBackDatas();

            try
            {
                var cts = Interlocked.Exchange(ref this.cancellationTokenSourceTileLoad, null);
                if (cts != null)
                    cts.Cancel();
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.Trace(e.ToString());
            }
            
            if (this.mapDataServiceHandler != null)
            {
                this.mapDataServiceHandler.CancelGetMapFeatureData();
            }

            this.MapLevel = -1;
        }

        protected override void ClearSplunkObjectDatas()
        {
            foreach (var wrapperControl in this.savedSplunkObjectDataManager.SplunkChartTableWrapperControls)
            {
                if (wrapperControl.IsTableControl)
                {
                    var tableControl = wrapperControl.TableControl;

                    if (tableControl != null && this.IsConsoleMode)
                    {
                        tableControl.DataRowClick -= TableControlOnDataRowClick;
                        tableControl.ButtonColumnCellClick -= TableControlOnButtonColumnCellClick;
                    }
                }
            }

            base.ClearSplunkObjectDatas();
        }

        protected override void ClearObjectDatas()
        {
            if (this.objectGraphicLayer != null)
            {
                var videoGraphics = this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>();

                foreach (var videoGraphic in videoGraphics.Where(videoGraphic => this.arcGISControlApi != null))
                {
                    this.arcGISControlApi.EraseCameraVideo(videoGraphic.ObjectID, videoGraphic.CameraInformationID);
                }
            }

            if (this.selectedGraphicList != null) this.selectedGraphicList.Clear();

            if (this.propertyManager != null) this.propertyManager.DeselectAll();

            base.ClearObjectDatas();

            if (this.cameraOverlayGraphic != null &&  this.objectGraphicLayer != null)
            {
                this.objectGraphicLayer.Graphics.Add(this.cameraOverlayGraphic);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newCursor"></param>
        private void ChangeCursor(Cursor newCursor)
        {
            this.Cursor = newCursor;
        }

        /// <summary>
        /// Mous Left Down 시 Click Position 지정
        /// </summary>
        private void SetClickPositionDatas()
        {   
            this.mapPointByClick = baseMap.ScreenToMap(Mouse.GetPosition(baseMap));

            //if (baseMap.WrapAroundIsActive)
            //S    this.mapPointByClick = Geometry.NormalizeCentralMeridian(mapPointByClick) as MapPoint;
        }

        /// <summary>
        /// Mouse Left Up시 Click Position을 없앤다.
        /// </summary>
        public void ReleaseClickPositionDatas()
        {
            this.mapPointByClick = null;
            this.resizingFlag = ResizingFlag.None;

            if (this.selectedVertexGraphic == null)
                return;

            this.selectedVertexGraphic.UnSelect();

            this.selectedVertexGraphic = null;
        }

        protected override void AfterTransitionStoryboard()
        {
            base.AfterTransitionStoryboard();

            this.ProcessNextTransitionChain();
        }

        protected override void InitializeDefaultMapExtent()
        {
            if (this.initialExtent != null)
            {
                this.SafeSetExtent(this.initialExtent);
                this.initialExtent = null;
            }
            else
            {
                var layer = baseTiledMapServiceLayer;
                if (this.CombinedHomeExtent != null && layer != null)
                {
                    this.SafeSetExtent(this.CombinedHomeExtent);
                }
                else
                {
                    base.InitializeDefaultMapExtent();
                }
            }
        }

        protected void ApplyHomeExtentToMapSettingData(Envelope curExtent)
        {
            // Map의 meta data에도 Home의 내용을 설정한다.
            var setting = this.mapSettingDataInfo;
            if (setting == null)
                return;
            if (curExtent == null)
            {
                setting.HomeExtent = null;
            }
            else
            {
                setting.HomeExtent = new MapSettingDataInfo.Extent
                {
                    XMin = curExtent.XMin,
                    XMax = curExtent.XMax,
                    YMin = curExtent.YMin,
                    YMax = curExtent.YMax,
                };
            }
        }


        /// <summary>
        /// Backup file에 데이터 저장
        /// </summary>
        /// <param name="baseMapObjectInfoDatas"></param>
        private void SaveMapAndFeaturesToBackupFile(IEnumerable<BaseMapObjectInfoData> baseMapObjectInfoDatas)
        {
            try
            {
                var fileName = string.Format("backup-{0}-{1:yyyy}{1:MM}{1:dd}{1:HH}{1:mm}{1:ss}.xml",
                    this.mapSettingDataInfo.ID, DateTime.Now);
                var savingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Innowatch", "MapBackups");
                var savingPath = Path.Combine(savingDir, fileName);
                var dataSet = MapDataServiceHandler.SetMapDataSet(this.mapSettingDataInfo,
                    baseMapObjectInfoDatas);

                Directory.CreateDirectory(savingDir);
                using (var fileStream = new FileStream(savingPath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(DataSet));
                    serializer.Serialize(fileStream, dataSet);
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException ||
                    ex is SecurityException)
                {
                    var arcGisControlApi = this.arcGISControlApi;

                    if (arcGisControlApi != null)
                    {
                        arcGisControlApi.ShowMessagePopup(
                               ex.ToString(),
                               true);
                    }
                }
                else throw;
            }
        }

        private void RestoreFromBackupFile()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true)
                return;
            using (var fileStream = dialog.OpenFile())
            {
                var xml = new XmlSerializer(typeof(DataSet));
                var dataSet = xml.Deserialize(fileStream) as DataSet;
                if (dataSet == null)
                {
                    var arcGisControlApi = this.arcGISControlApi;

                    if (arcGisControlApi != null)
                    {
                        arcGisControlApi.ShowMessagePopup(
                               Resource_ArcGISControl_ArcGISClientViewer.Message_InvalidBackupFile,
                               true);
                    }
                    
                    return;
                }
                try
                {
                    var metadataTable = dataSet.Tables[0];
                    var id = metadataTable.Rows[0][0] as string;
                    if (id == null || id != this.CurrentMapSettingInfoDataId)
                    {
                        var arcGisControlApi = this.arcGISControlApi;

                        if (arcGisControlApi != null)
                        {
                            if(!arcGisControlApi.ShowMessagePopup(
                                   string.Format(Resource_ArcGISControl_ArcGISClientViewer.Message_AskApplyingWrongBackup,
                                    id ?? "<null>",
                                    this.CurrentMapSettingInfoDataId ?? "<null>"),
                                   true)) return;
                        }
                    }
                }
                catch(IndexOutOfRangeException)
                {
                    var arcGisControlApi = this.arcGISControlApi;

                    if (arcGisControlApi != null)
                    {
                        arcGisControlApi.ShowMessagePopup(
                              Resource_ArcGISControl_ArcGISClientViewer.Message_InvalidBackupFile,true);
                    }

                    return;
                }

                var res = MapDataServiceHandler.GetMapFeatureFromSentDataSet(dataSet);

                this.ReleaseDatas();
                
                this.LoadMapObjectInfoDatas(res, true);
            }
        }

        private void HideClickedPopups()
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (this.dynamicSplunkControlManager != null)
                {
                    this.dynamicSplunkControlManager.Hide();
                }

                if (this.workStationControlManager != null)
                {
                    this.workStationControlManager.Hide();
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                                                           {
                                                               if (this.dynamicSplunkControlManager != null)
                                                               {
                                                                   this.dynamicSplunkControlManager.Hide();
                                                               }

                                                               if (this.workStationControlManager != null)
                                                               {
                                                                   this.workStationControlManager.Hide();
                                                               }
                                                           }));
            }
        }

        #endregion //Method

        #region Event Handlers

        #region MapControl EventHandlers

        override protected void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            base.OnLoaded(sender, routedEventArgs);

            if (!this.IsConsoleMode && this.isEditMode)
            {
                this.InitializePropertyWindow();
            }
            else if (this.IsConsoleMode)
            {
                this.InitializeMemoPropertyControl();
            }
        }

        #endregion //MapControl EventHandlers

        #region ArcGIS Map Event Handlers

        protected override void BaseMapOnProgress(object sender, ProgressEventArgs progressEventArgs)
        {
            base.BaseMapOnProgress(sender, progressEventArgs);

            if (this.processingIcon == null) return;

            this.processingIcon.Visibility = progressEventArgs.Progress == 100 ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Main Map Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void BaseMapOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.objectGraphicLayer.IsHitTestVisible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void BaseMapOnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
            {
                if (_GraphicContextMenu.Visibility == System.Windows.Visibility.Visible)
                {
                    _GraphicContextMenu.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.editor != null && this.editor.IsEditing && this.editor.SeletedGeometryEditor)
                {
                    this.SetClickPositionDatas();
                    return;
                }

                this.ReleaseAllSelectedObject();

                if (this.isDoingNewObjectByClick)
                {
                    mouseButtonEventArgs.Handled = true;


                    switch (SelectedMapObjectType)
                    {
                        case MapObjectType.None :
                            return;
                            break;
                        case MapObjectType.DrawLine :
                            this.MakeDrawLine();
                            break;
                        case MapObjectType.Universal :
                            this.MakeUniversal();
                            break;
                        default :
                            string MapObjectDataTypeName = SelectedMapObjectType.GetMapObjectTypeInfoAttribute(0).MapObjectInfoDataType.ToString();
                            Type MapObjectDataType = typeof(BaseMapObjectInfoData).Assembly.GetType(MapObjectDataTypeName, true);
                            BaseMapObjectInfoData MapObjectBaseData = (BaseMapObjectInfoData)Activator.CreateInstance(MapObjectDataType);
                            this.MakeMapObject(SelectedMapObjectType, MapObjectBaseData);
                            break;
                    }
                }

                this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold));
            }
            else if (mouseButtonEventArgs.ChangedButton == MouseButton.Right && mouseButtonEventArgs.ClickCount == 2)
            {
                this.ChangeMapExtentByMousePoint(false);
            }

            //Deselect All 변경 통보
            if (this.selectedGraphicList.Count <= 0)
            {
                this.RaiseObjectSelectedEvent();
            }
        }

        /// <summary>
        /// Map Mouse Left Up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void BaseMapOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.ReleaseObjectDragSetting();
            this.ReleaseClickPositionDatas();
            if (this.editor != null) this.editor.SeletedGeometryEditor = false;

            if (selectedGraphicList != null && selectedGraphicList.Count > 0 && _CommandTransform != null && _OriginGeometryList != null && selectedGraphicList.Count == _OriginGeometryList.Count)
            {
                for (int i = 0; i < selectedGraphicList.Count; i++)
                {
                    if(selectedGraphicList[0].Geometry.Equals(_OriginGeometryList[0]) == false)
                        _CommandTransform.AddTransformObjectClone(selectedGraphicList[i], _OriginGeometryList[i], selectedGraphicList[i].Geometry);
                }
                if(_CommandTransform.ModifiedGeometryCount > 0)
                    this.AddCommandToHistory(_CommandTransform);

                _CommandTransform = null;
                _OriginGeometryList.Clear();
                _OriginGeometryList = null;
            }
        }


        /// <summary>
        /// Map Mouse Move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void BaseMapOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.markerGraphic != null)
            {
                mouseEventArgs.Handled = true;
                this.MoveMarker();
                return;
            }

            if (this.isDoingNewObjectByClick)
            {
                string StrMapObjectCursor = SelectedMapObjectType.GetMapObjectTypeInfoAttribute(0).MapObjectStrCursor;

                ArcGISControl.Helper.CursorType MapObjectCursor = Helper.CursorType.HandHold;
                Enum.TryParse<ArcGISControl.Helper.CursorType>(StrMapObjectCursor, out MapObjectCursor);

                this.ChangeCursor(CursorManager.Instance.GetCursor(MapObjectCursor));
                return;
            }

            if (this.selectedGraphicList.Count < 1)
            {
                if (Mouse.LeftButton == MouseButtonState.Released) ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
                return;
            }

            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed && !IsPanning)
            {
                if (mapPointByClick == null)
                    return;

                if (this.selectedGraphicList.Count > 0 && _CommandTransform == null && _OriginGeometryList == null)
                {
                    _CommandTransform = new CommandTransform(this.GeometryChange, this.MoveGraphicPosition);
                    _OriginGeometryList = new List<ESRI.ArcGIS.Client.Geometry.Geometry>();

                    this.selectedGraphicList.ForEach(Item =>
                    {
                        _OriginGeometryList.Add(Item.Geometry);
                    });
                }

                var newMapPoint = baseMap.ScreenToMap(mouseEventArgs.GetPosition(baseMap));

                var displacement = new Vector(newMapPoint.X - mapPointByClick.X, newMapPoint.Y - mapPointByClick.Y);

                if(this.editor != null && this.editor.IsEditing && this.selectedGraphicList.Count == 1)
                {
                    if (this.editor.EditingType != GraphicGeometryEditor.EditType.None)
                    {
                        switch(this.editor.EditingType)
                        {
                            case GraphicGeometryEditor.EditType.ScaleBox:
                                this.editor.Move(displacement);
                                break;
                            case GraphicGeometryEditor.EditType.ScalePointer:
                                var selectedGraphic = this.selectedGraphicList.ElementAt(0);
                                var setRateMaintenanceScale = false;

                                if (selectedGraphic.Type == MapObjectType.CameraVideo)
                                {
                                    var cameraObjectData = this.cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID);
                                    setRateMaintenanceScale = cameraObjectData.Video.ConstrainProportion ?? false;
                                }
                                this.editor.Scale(new Vector(newMapPoint.X, newMapPoint.Y), setRateMaintenanceScale);
                                break;
                            case GraphicGeometryEditor.EditType.VertexPointer:
                                this.editor.DragVertex(new MapPoint(newMapPoint.X, newMapPoint.Y));
                                break;
                        }

                        if (this.isEditMode)
                        {
                            this.historyManager.IsChanged = true;
                        }
                    }
                }
                else
                {
                    this.MoveGraphicsPosition(displacement);

                    if (this.isEditMode)
                    {
                        this.historyManager.IsChanged = true;
                    }
                }

                this.mapPointByClick = newMapPoint;
                mouseEventArgs.Handled = true;
            }

            if (this.editor != null && this.editor.IsEditing && this.editor.SeletedGeometryEditor) return;
            if (Mouse.LeftButton == MouseButtonState.Released) ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
            else if (CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen) == this.Cursor) ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold));
        }

        /// <summary>
        /// Map Mouse Preview Wheel 
        /// Zoom in - out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseWheelEventArgs"></param>
        private void BaseMapOnPreviewMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
        {
            Debug.Assert(sender is Map);
            var map = (Map)sender;

            if (this.IsConsoleMode)
            {
                this.backbufferBaseMap.ZoomFactor = this.baseMap.ZoomFactor = 1 + Math.Abs(mouseWheelEventArgs.Delta / 240f);

                if (mouseWheelEventArgs.Delta > 0)
                {
                    // 링크존 진입
                    this.CheckLinkZoneOnWheel(mouseWheelEventArgs, map);
                }
                else
                {
                    // 링크존 탈출
                    this.GoPreviousOnWheel(mouseWheelEventArgs, map);
                }
            }
            else
            {
                this.backbufferBaseMap.ZoomFactor = this.baseMap.ZoomFactor = 1 + Math.Abs(mouseWheelEventArgs.Delta / 480f);
            }
        }

        /// <summary>
        /// Map Mouse PreviewDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void BaseMapOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.baseMap.Focus();
        }

        /// <summary>
        /// Map Mouse Previe Key Down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void BaseMapOnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)
                && Keyboard.IsKeyDown(Key.O))
            {   
                this.RestoreFromBackupFile();
            }

            if (keyEventArgs.Key == Key.LeftCtrl)
            {
                if (this.editor == null || this.selectedGraphicList.Count == 0)
                {
                    return;
                }

                if (this.selectedGraphicList.ElementAt(0).Type == MapObjectType.LinkZone || this.selectedGraphicList.ElementAt(0).Type == MapObjectType.Workstation)
                {
                    this.editor.EditDragVertexEnabled = true;
                    this.editor.EditSnapVertexEnabled = true;
                }
            }

            if (!_IsPanning && keyEventArgs.Key == Key.Space)
            {
                _IsPanning = true;
                _OldCursor = this.Cursor;
                this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
            }
        }

        /// <summary>
        /// Map Mouse Previe Key UP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void BaseMapOnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Delete)
            {
                this.DeleteSelectedObjects();
            }

            if (keyEventArgs.Key == Key.LeftCtrl)
            {
                if (this.editor == null || this.selectedGraphicList.Count == 0)
                {
                    return;
                }

                if (this.selectedGraphicList.ElementAt(0).Type == MapObjectType.LinkZone || this.selectedGraphicList.ElementAt(0).Type == MapObjectType.Workstation)
                {
                    if (Mouse.LeftButton != MouseButtonState.Pressed)
                    {
                        this.editor.EditDragVertexEnabled = false;
                        this.editor.EditSnapVertexEnabled = false;
                    }
                }
            }

            if (IsPanningToggleBtn == false && _IsPanning && keyEventArgs.Key == Key.Space)
            {
                _IsPanning = false;
                this.ChangeCursor(_OldCursor);
            }
        }

        /// <summary>
        /// Map Extent Changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="extentEventArgs"></param>
        protected override void BaseMapOnExtentChanging(object sender, ExtentEventArgs extentEventArgs)
        {
            base.BaseMapOnExtentChanging(sender, extentEventArgs);

            if (this.baseTiledMapServiceLayer == null || this.baseTiledMapServiceLayer.FullExtent == null ||
                extentEventArgs.NewExtent == null) return;

            if (this.processingIcon != null) this.processingIcon.Visibility = Visibility.Visible;

            try
            {
                this.extentChangingRecurseCounter++;

                // 이 조건이 바뀌면 GoPreviousOnWheel() 조건도 신경 써준다.
                var newExtent = (this.ActualHeight >= 1 && this.ActualHeight >= 1 && this.extentChangingRecurseCounter <= 1)
                    ? AdjustProperExtent(extentEventArgs.NewExtent, extentEventArgs.OldExtent)
                    : null;
                if (newExtent != null)
                {
                    // 조정이 필요하다는 뜻이다.
                    this.baseMap.ForceZoomTo(newExtent, true);
                }
                else
                {
                    this.ChangedZoomLevel();
                }
            }
            finally
            {
                this.extentChangingRecurseCounter--;
            }

            this.ChangeCameraVideoVisible();
            this.ChangeAllSplunkControlVisible();
            this.ChangeAllSplunkServiceStatus();

            this.MoveCameraPopupControl();

            this.OnEMapExtentChanged(new MapExtentChangeEventArgs
            {
                Extent =
                    new Rect(extentEventArgs.NewExtent.XMin, extentEventArgs.NewExtent.YMin,
                             extentEventArgs.NewExtent.Width,
                             extentEventArgs.NewExtent.Height)
            });

            if (this.dynamicSplunkControlManager != null)
            {
                this.dynamicSplunkControlManager.Hide();
            }

            if (this.workStationControlManager != null)
            {
                this.workStationControlManager.Hide();
            }

            this.MoveSplunkPopupControl();
        }

        /// <summary>
        /// Map Extent Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="extentEventArgs"></param>
        private void BaseMapOnExtentChanged(object sender, ExtentEventArgs extentEventArgs)
        {
            if (this.processingIcon != null) this.processingIcon.Visibility = Visibility.Collapsed;

            if (this.baseTiledMapServiceLayer == null) return;

            this.SetCurrentLevel();

            if (this.eHaveSavingMapData != null)
                this.eHaveSavingMapData(this, null);

            this.ChangedZoomLevel();

            this.ChangeCameraVideoVisible();
            this.ChangeAllSplunkControlVisible();
            this.ChangeAllSplunkServiceStatus();

            this.MoveCameraPopupControl();

            this.ExtentChangedLinkZoneDoubleClick();

            this.OnEMapExtentChanged(new MapExtentChangeEventArgs
                                         {
                                             Extent =
                                                 new Rect(extentEventArgs.NewExtent.XMin, extentEventArgs.NewExtent.YMin,
                                                          extentEventArgs.NewExtent.Width,
                                                          extentEventArgs.NewExtent.Height)
                                         });

            this.MoveSplunkPopupControl();
        }

        private void MoveSplunkPopupControl()
        {
            if (this.splunkPopupControlManager != null)
            {
                var baseGraphic = this.GetOneBaseGraphicInGraphicLayer(this.splunkPopupControlManager.ShowingSplunkId, MapObjectType.SplunkIcon);

                if (baseGraphic == null && this.universalObjectDataManager.ContainsId(this.splunkPopupControlManager.ShowingSplunkId))
                {
                    baseGraphic = this.universalObjectDataManager.GetIconGraphic(this.splunkPopupControlManager.ShowingSplunkId);
                }

                if (baseGraphic != null)
                {
                    var position = new Point((baseGraphic.Geometry.Extent.XMin - baseMap.Extent.XMin) / baseMap.Resolution,
                                                            (baseMap.Extent.YMax - baseGraphic.Geometry.Extent.YMax) / baseMap.Resolution);

                    this.splunkPopupControlManager.AdjustPopupPosition(position);
                }
            }
        }

        public bool IsLargerThanProperExtent(Envelope env)
        {
            if (this.isInitializedMapTileService &&
                (Math.Floor(env.Height) <= Math.Floor(this.baseTiledMapServiceLayer.FullExtent.Height) ||
                Math.Floor(env.Width) <= Math.Floor(this.baseTiledMapServiceLayer.FullExtent.Width)))
            {
                return false;
            }

            return true;
        }

        private Envelope InnerAdjustProperExtent(Envelope constraint, Envelope env, Envelope old)
        {
            // 조정이 필요없을 때는 null를 리턴하도록 하는 함수이다.
            if (constraint == null)
                return null;
            if (!env.Intersects(constraint))
                return constraint;

            var result = env.Clone();
            var hWidth = constraint.Width;
            var hHeight = constraint.Height;

            var destWidth = env.Width;
            var destHeight = env.Height;

            if (env.Width > constraint.Width ||
                env.Height > constraint.Height)
            {
                if (env.Width * hHeight < hWidth * env.Height)
                {
                    destHeight = hHeight;
                    destWidth = env.Width * destHeight / env.Height;
                }
                else
                {
                    destWidth = hWidth;
                    destHeight = env.Height * destWidth / env.Width;
                }
            }
            var center = env.GetCenter();
            result.XMin = center.X - destWidth / 2;
            result.XMax = center.X + destWidth / 2;
            result.YMin = center.Y - destHeight / 2;
            result.YMax = center.Y + destHeight / 2;

            double deltaX = 0, deltaY = 0;
            if (result.XMin < constraint.XMin)
            {
                deltaX = constraint.XMin - result.XMin;
            }
            else if (result.XMax > constraint.XMax)
            {
                deltaX = constraint.XMax - result.XMax;
            }

            if (result.YMin < constraint.YMin)
            {
                deltaY = constraint.YMin - result.YMin;
            }
            else if (result.YMax > constraint.YMax)
            {
                deltaY = constraint.YMax - result.YMax;
            }

            result.XMin += deltaX;
            result.XMax += deltaX;
            result.YMin += deltaY;
            result.YMax += deltaY;

            if (GeometryHelper.IsSimilar(result.XMin, env.XMin) &&
                GeometryHelper.IsSimilar(result.XMax, env.XMax) &&
                GeometryHelper.IsSimilar(result.YMin, env.YMin) &&
                GeometryHelper.IsSimilar(result.YMax, env.YMax))
                return null;

            return result;
        }

        private Envelope InnerAdjustProperExtentLoose(Envelope constraint, Envelope env, Envelope old)
        {
            // 조정이 필요없을 때는 null를 리턴하도록 하는 함수이다.
            if (constraint == null)
                return null;

            var result = env.Clone();

            var pendingScaleBasedOnConstraintHeight = false;
            if (!env.Intersects(constraint))
            {
                if (Double.IsInfinity(constraint.Width))
                {
                    pendingScaleBasedOnConstraintHeight = true;
                }
                else
                {
                    return constraint;
                }
            }
            else if ((GeometryHelper.IsGreaterThan(result.Width, constraint.Width) || Double.IsInfinity(constraint.Width)) &&
                GeometryHelper.IsGreaterThan(result.Height, constraint.Height))
            {
                if (Double.IsInfinity(constraint.Width))
                {
                    pendingScaleBasedOnConstraintHeight = true;
                }
                else
                {
                    return constraint;
                }
            }

            if (pendingScaleBasedOnConstraintHeight)
            {
                var middleX = (result.XMin + result.XMax) / 2;
                var width = result.Width;
                var ratio = constraint.Height / result.Height;

                result.XMin = middleX - ratio * width / 2;
                result.XMax = middleX + ratio * width / 2;
                result.YMin = constraint.YMin;
                result.YMax = constraint.YMax;
            }

            if (GeometryHelper.IsSimilar(result.XMin, env.XMin) &&
                GeometryHelper.IsSimilar(result.XMax, env.XMax) &&
                GeometryHelper.IsSimilar(result.YMin, env.YMin) &&
                GeometryHelper.IsSimilar(result.YMax, env.YMax))
                return null;

            return result;
        }
        /// <summary>
        /// 만약 조정이 필요없다면 null을 리턴한다.
        /// 해당 env로 가고 싶은데, 이것이 최종적으로는 어디로 가야할지 얻어온다.
        /// env는 null이면 안된다.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="old"></param>
        /// <returns></returns>
        public Envelope AdjustProperExtent(Envelope env, Envelope old)
        {
            if (env == null)
                throw new ArgumentNullException("env");
            var overallContraint = this.GetStrictExtentConstraint();

            return InnerAdjustProperExtentLoose(overallContraint, env, old);
        }

        /// <summary>
        /// Extent를 엄격하게 제한할 Extent를 가져온다.
        /// </summary>
        /// <returns></returns>
        protected virtual Envelope GetStrictExtentConstraint()
        {
            Envelope overallContraint = null;
            var mapLayer = this.baseTiledMapServiceLayer;
            if (mapLayer != null)
            {
                var fullExtent = mapLayer.FullExtent.Clone();

                if (fullExtent != null)
                {
                    if (this.baseMap.WrapAroundIsActive)
                    {
                        fullExtent.XMin = Double.NegativeInfinity;
                        fullExtent.XMax = Double.PositiveInfinity;
                    }

                    overallContraint = overallContraint != null ? overallContraint.Intersection(fullExtent) : fullExtent;
                }
            }
            return overallContraint;
        }

        /// <summary>
        /// Extent를 엄격하게 제한할 Extent의 최대 크기를 가져온다.
        /// </summary>
        /// <returns></returns>
        protected virtual Size GetStrictExtentSizeConstraint()
        {
            Envelope overallContraint = null;
            var mapLayer = this.baseTiledMapServiceLayer;
            if (mapLayer != null)
            {
                var fullExtent = mapLayer.FullExtent.Clone();

                if (fullExtent != null)
                {
                    var originalFullWidth = fullExtent.Width;
                    if (this.baseMap.WrapAroundIsActive)
                    {
                        fullExtent.XMin = Double.NegativeInfinity;
                        fullExtent.XMax = Double.PositiveInfinity;
                    }

                    overallContraint = overallContraint != null ? overallContraint.Intersection(fullExtent) : fullExtent;
                    // fullextent의 가로 크기는 wraparound이더라도 엄격히 지켜지도록 map이 작동한다.
                    return new Size(Math.Min(originalFullWidth, overallContraint.Width), overallContraint.Height);
                }
            }

            if (overallContraint == null)
                return new Size(0, 0);

            return new Size(overallContraint.Width, overallContraint.Height);
        }

        /// <summary>
        /// 그냥 Extent를 바로 set하면 Extent Changing Extent Changed가 제대로 안 불리므로 이것을 통해 미리 필요한 조정을 한다.
        /// </summary>
        /// <param name="env">가고 싶은 Extent</param>
        /// <param name="prev">optional한 이전 Extent 지정</param>
        public override void SafeSetExtent(Envelope env, Envelope prev = null)
        {
            var map = this.baseMap;
            if (map == null)
                throw new InvalidOperationException("baseMap is null");

            var size = map.RenderSize;

            if (!(size.IsEmpty || size.Width == 0 || size.Height == 0))
            {
                var sizeWHRatio = size.Width / size.Height;
                var envWHRatio = env.Width / env.Height;
                if (!GeometryHelper.IsSimilar(sizeWHRatio, envWHRatio))
                {
                    var center = env.GetCenter();
                    if (sizeWHRatio > envWHRatio)
                    {
                        env = new Envelope(
                            center.X - sizeWHRatio * env.Height / 2,
                            env.YMin,
                            center.X + sizeWHRatio * env.Height / 2,
                            env.YMax);
                    }
                    else
                    {
                        env = new Envelope(
                            env.XMin,
                            center.Y - env.Width / sizeWHRatio / 2,
                            env.XMax,
                            center.Y + env.Width / sizeWHRatio / 2);
                    }
                }
            }

            map.Extent = AdjustProperExtent(env, prev) ?? env;
        }

        private bool durationInit;

        public void SetExtentForSync(Rect aExtent)
        {
            if (!this.durationInit)
            {
                this.baseMap.ZoomDuration = new TimeSpan(0);
                this.baseMap.PanDuration = new TimeSpan(0);

                this.durationInit = true;
            }

            // DP에서 맵 로드 시 타일이 로드완료되기 전에 좌표 이동 싱크가 처리되면서 좌표 설정 싱크가 무시되는 현상이 발생하여 
            // 타일 레이어가 로드되기 전에 들어오는 좌표 설정을 기본 좌표로 입력해서 타일 로딩 후 적용되도록 한다.
            if (this.baseTiledMapServiceLayer == null)
            {
                this.initialExtent = new Envelope(aExtent.Left, aExtent.Top, aExtent.Right, aExtent.Bottom);
            }
            else
            {
                this.ChangeZoomToLocation(new Envelope(aExtent.Left, aExtent.Top, aExtent.Right, aExtent.Bottom));
            }
        }

        /// <summary>
        /// ArcGISControl 에 보이는 Full 화면이 (화면크기에 상관없이 ) 1레벨이 되도록 세팅함
        /// 
        /// </summary>
        /// <returns></returns>
        protected double SetCurrentLevel()
        {
            if (this.baseTiledMapServiceLayer == null || this.baseTiledMapServiceLayer.FullExtent == null ||
                this.baseMap == null || this.baseMap.Extent == null) return -1;

            try
            {
                var isCheckLevelByWidth = this.baseMap.ActualHeight / this.baseMap.ActualWidth > this.baseTiledMapServiceLayer.FullExtent.Height / this.baseTiledMapServiceLayer.FullExtent.Width;

                this.MapLevel = isCheckLevelByWidth ?
                                    Math.Log(this.baseTiledMapServiceLayer.FullExtent.Width / this.baseMap.Extent.Width, 2) + 1 :
                                    Math.Log(this.baseTiledMapServiceLayer.FullExtent.Height / this.baseMap.Extent.Height, 2) + 1;

            }
            catch (NullReferenceException exception)
            {
                InnowatchDebug.Logger.Trace(exception.ToString());
            }

            return this.mapLevel;
        }
        
        
        #endregion //ArcGISMapEvents

        #region MapDataHandler Event

        /// <summary>
        /// Map Data Hanlder가 Data를 받아온 후 Event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="getMapDataProcessCompletedEventArgs">
        /// <Data>Message</Data>
        /// <IsCompleted>true, false</IsCompleted>
        /// </param>
        private void MapDataServiceHandlerOnEGetMapDataProcessCompleted(object sender, GetMapDataProcessCompletedEventArgs getMapDataProcessCompletedEventArgs)
        {
            if (getMapDataProcessCompletedEventArgs.IsCompleted)
            {
                var objectInfos = mapDataServiceHandler.CameraObjectComponentDataInfos;
                var token = mapDataServiceHandler.CompletedToken;

                Application.Current.Dispatcher.BeginInvoke(new Action(()=>
                {
                    // 이전에 로딩하던 map data이다. 사용하지 않는다.
                    if (!Object.ReferenceEquals(this.mapDataServiceToken, token))
                        return;

                    this.LoadMapObjectInfoDatas(objectInfos, true);

                    if (this.isEditMode)
                    {
                        this.historyManager.SaveHistory();
                    }

                    if (!String.IsNullOrEmpty(this.initialBookmarkName))
                        this.ApplyInitialBookMark(objectInfos);
                    else if (!String.IsNullOrEmpty(this.initialObjectName))
                        this.ApplyInitialObject(objectInfos);

                    if (this.IsConsoleMode && this.mapSettingDataInfo.UseMapSpl)
                    {
                        if (!this.mapSettingDataInfo.IsValidMapSplData())
                        {
                            return;
                        }

                        this.StartMapSpl(this.mapSettingDataInfo);
                    }
                }), DispatcherPriority.Loaded);
            }
        }

        private void ApplyInitialBookMark(IEnumerable<BaseMapObjectInfoData> objectInfos)
        {
            if (string.IsNullOrWhiteSpace(this.initialBookmarkName))
                return;

            var initialBookmarkDataInfo = objectInfos.FirstOrDefault(objectInfo =>
                objectInfo.ObjectType == MapObjectType.BookMark && objectInfo.Name == this.initialBookmarkName)
                as MapBookMarkDataInfo;

            this.initialBookmarkName = null;

            if (initialBookmarkDataInfo == null)
                return;

            var initialBookMarkExtent = new Envelope(
                initialBookmarkDataInfo.ExtentMin.X,
                initialBookmarkDataInfo.ExtentMin.Y,
                initialBookmarkDataInfo.ExtentMax.X,
                initialBookmarkDataInfo.ExtentMax.Y);
            this.        SafeSetExtent(initialBookMarkExtent);
        }

        private void ApplyInitialObject(IEnumerable<BaseMapObjectInfoData> objectInfos)
        {
            if (string.IsNullOrWhiteSpace(this.initialObjectName))
                return;

            var initialObjectDataInfo = objectInfos.FirstOrDefault(objectInfo =>
                objectInfo.ObjectType != MapObjectType.BookMark && objectInfo.Name == this.initialObjectName);

            this.initialObjectName = null;

            if (initialObjectDataInfo == null)
                return;

            this.GoToLocation(initialObjectDataInfo, true);
        }

        protected override void LoadMapObjectInfoDatas(ObservableCollection<BaseMapObjectInfoData> mapObjectInfoDatas, bool isPublicData, bool IsConvertCoordinates = true)
        {
            base.LoadMapObjectInfoDatas(mapObjectInfoDatas, isPublicData, IsConvertCoordinates);  // IsConvertCoordinates 플래그 추가    [2014. 10. 21 엄태영]

            if (isPublicData) this.isPublicObjectLoaded = true;
            else isPrivateObjectLoaded = true;

            if(this.isPublicObjectLoaded && this.isPrivateObjectLoaded)
            {
                this.RaiseObjectLoadedEvent();

                this.RefreshAllCameraVideoRect();

                this.MoveCameraPopupControl();

                this.processingIcon.Visibility = Visibility.Collapsed;

                this.ChangeCameraVideoVisible();
                this.ChangeAllSplunkControlVisible();

                if (this.isEditMode)
                {
                    this.historyManager.StartManagement();
                }
            }   
        }

        public ObservableCollection<HistoryInfo> GetHistoryInfos()
        {
            if (this.historyManager == null)
            {
                return null;
            }

            return this.historyManager.HistoryList;
        }

        public void ApplyHistory(string hitoryId)
        {            
            if (this.historyManager == null)
            {
                return;
            }

            var dataSet = this.historyManager.GetHistoryData(hitoryId);

            var res = MapDataServiceHandler.GetMapFeatureFromSentDataSet(dataSet);

            this.ReleaseDatas();

            this.LoadMapObjectInfoDatas(res, true, false);
        }

        #endregion //MapDataHandler Event

        #region Object Data Event Handlers
        
        /// <summary>
        /// Splunk Data Color Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="colorChangedEventArgs"></param>
        private void SavedSplunkObjectDataManager_ColorChanged(object sender, ColorChangedEventArgs colorChangedEventArgs)
        {
            var baseGraphicData = this.GetOneBaseGraphicInGraphicLayer(colorChangedEventArgs.Id, colorChangedEventArgs.ObjectType);

            if (baseGraphicData != null)
            {
                var splunkIconGraphic = baseGraphicData as SplunkIconGraphic;

                if (splunkIconGraphic != null) splunkIconGraphic.ChangeIconColor(colorChangedEventArgs.Color, colorChangedEventArgs.IsBlinking);
            }
        }

        /// <summary>
        /// Location Data Color Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="colorChangedEventArgs"></param>
        private void PublicGraphicDataManager_ColorChanged(object sender, ColorChangedEventArgs colorChangedEventArgs)
        {
            var baseGraphic = this.GetOneBaseGraphicInGraphicLayer(colorChangedEventArgs.Id, colorChangedEventArgs.ObjectType);

            if (baseGraphic != null)
            {
                if(baseGraphic is LinkZoneGraphic)
                {
                    (baseGraphic as LinkZoneGraphic).ChageColorOnlyBySplunk(colorChangedEventArgs.Color);
                }
                else if(baseGraphic is ImagePolygonGraphic)
                {
                    (baseGraphic as ImagePolygonGraphic).ChageColorOnlyBySplunk(colorChangedEventArgs.Color);
                }
            }
        }

        /// <summary>
        /// WorkStation Data Changed (For VW)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="workStationDataChangedEventArgs"></param>
        private void PublicGraphicDataManager_WorkstationDataChanged(object sender, WorkStationDataChangedEventArgs workStationDataChangedEventArgs)
        {
            /*
             * todo : Workstatuib  Data의 Type이 왜 None 일까요? 
             */
            var dataInfo = this.publicGraphicDataManager.GetObjectDataByObjectID(workStationDataChangedEventArgs.Id,MapObjectType.None);

            if (dataInfo == null) return;

            var workStationDataInfo = dataInfo as MapWorkStationObjectDataInfo;

            if (workStationDataInfo == null)
                return;

            workStationDataInfo.WorkStationReturnedData = workStationDataChangedEventArgs.Data;

            BaseGraphic baseGraphic = this.GetOneBaseGraphicInGraphicLayer(workStationDataChangedEventArgs.Id, dataInfo.ObjectType);

            if (baseGraphic == null) return;

            var linkZoneGraphic = baseGraphic as LinkZoneGraphic;

            if (linkZoneGraphic != null)
            {
                linkZoneGraphic.ChageColorOnlyBySplunk(workStationDataChangedEventArgs.Data.Color);
            }
        }

        /// <summary>
        /// Home Bookmark changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublicGraphicDataManager_homeBookmarkChanged(object sender, CollectionChangeEventArgs<MapBookMarkDataInfo> e)
        {
            switch (e.Action)
            {
                case CollectionChangeAction.Refresh:
                case CollectionChangeAction.Add:
                    if (e.Element != null)
                    {
                        this.SetValue(HomeExtentPropertyKey,
                            new Envelope(
                                e.Element.ExtentMin.X,
                                e.Element.ExtentMin.Y,
                                e.Element.ExtentMax.X,
                                e.Element.ExtentMax.Y));
                    }
                    else
                    {
                        this.SetValue(HomeExtentPropertyKey, null);
                    }
                    break;
                case CollectionChangeAction.Remove:
                    this.SetValue(HomeExtentPropertyKey, null);
                    break;
            }
        }

        #endregion //Object Data Event Handlers

        #region Map Tiled Service Event Handlers

        /// <summary>
        /// ArcGIS MAP TILE INITALIZED
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void tileLayer_Initialized(object sender, EventArgs e)
        {
            if (!ReferenceEquals(this.baseTiledMapServiceLayer, sender))
                return;

            try
            {
                var initializedAction = new Action(() =>
                {
                    if (!ReferenceEquals(this.baseTiledMapServiceLayer, sender))
                        return;

                    if (!this.isInitializedMapTileService)
                    {
                        this.isInitializedMapTileService = true;

                        this.InitializeObjectDatas();
                    }
                });

                if (Dispatcher.CheckAccess())
                {
                    initializedAction();
                }
                else
                {
                    Dispatcher.BeginInvoke(initializedAction);
                }
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(ex, "tileLayer_Initialized(object sender, EventArgs e)");
            }

            base.tileLayer_Initialized(sender, e);
        }

        #endregion //Map Tiled Service Event Handlers

        #region Managers Event Handlers

        private void WorkStationControlManager_OnGoLinkedMap(object sender, LinkedMapEventArgs linkedMapEventArgs)
        {
            this.splunkPopupControlManager.Hide();
            this.dynamicSplunkControlManager.Hide();
            this.workStationControlManager.Hide();

            this.linkedMapHistory.NewLocation(new MapHistoryEntity(linkedMapEventArgs.LinkedMapID, null));

            if (!this.ChangeSelectedLinkedMap(linkedMapEventArgs.LinkedMapID, null))
            {
                this.linkedMapHistory.CancelMove();
            }
        }


        private void WorkStationControlManager_OnShowSearchViewControl(object sender, WorkStationEventArgs workStationEventArgs)
        {
            var max = this.ToGeographic(this.MaxExtentToPoint);
            var min = this.ToGeographic(this.MinExtentToPoint);

            var hasScheme = UrlParseTool.ExtractScheme(workStationEventArgs.DataInfo.SearchViewUrl) != null;
            string url = string.Format("{0}?session_id={1}&lat_1={2}&long_1={3}&lat_2={4}&long_2={5}",
                                       !hasScheme
                                           ? "http://" + workStationEventArgs.DataInfo.SearchViewUrl
                                           : workStationEventArgs.DataInfo.SearchViewUrl, this.mapSettingDataInfo.ID,
                                       min.Lat.ToString(), min.Lng.ToString(), max.Lat.ToString(), max.Lng.ToString());

            if (this.mapSettingDataInfo.MapType == MapProviderType.CustomMap)
            {
                url += string.Format("&custom_map={0}", this.mapSettingDataInfo.Name);
            }

            if (this.arcGisControlViewerApi != null) this.arcGisControlViewerApi.OpenSearchViewControl(url);
        }

        #endregion Managers Event Handlers

        #region Graphic Geometry Editor Event Handlers

        private void EditorOnEGraphicGeometryChanged(object sender, GraphicGeometryChangedEventArgs graphicGeometryChangedEventArgs)
        {
            var eventArgs = graphicGeometryChangedEventArgs;
            if (eventArgs == null) return;
            var baseGraphic = eventArgs.Graphic as BaseGraphic;
            if (baseGraphic == null) return;
            var changeControlSize = true;

            if (baseGraphic is IPointCollectionOwner)
            {
                var oldPoints = (baseGraphic as IPointCollectionOwner).PointCollection;
                var newPoints = eventArgs.Points;

                if (NumberUtil.AreSame(oldPoints.Max(e => e.X) - oldPoints.Min(e => e.X), newPoints.Max(e => e.X) - newPoints.Min(e => e.X)) &&
                    NumberUtil.AreSame(oldPoints.Max(e => e.Y) - oldPoints.Min(e => e.Y), newPoints.Max(e => e.Y) - newPoints.Min(e => e.Y))) changeControlSize = false;

                (baseGraphic as IPointCollectionOwner).PointCollection = eventArgs.Points;
            }

            this.GeometryChange(baseGraphic, eventArgs.Points, changeControlSize);

            /*
            this.SetObjectPosition(baseGraphic, eventArgs.Points);

            switch (baseGraphic.Type)
            {
                case MapObjectType.CameraVideo:
                    this.RefreshGraphicObjectVideo(baseGraphic as CameraVideoGraphic);
                    break;
                case MapObjectType.SplunkControl:
                    if(changeControlSize) this.ResizeSplunkControl(baseGraphic);
                    break;
                case MapObjectType.Text:
                    if (baseGraphic is PolygonControlGraphic<TextBoxControl> && isEditMode)
                    {
                        var texBoxControl = (baseGraphic as ControlGraphic<TextBoxControl>).Control as TextBoxControl;
                        if (changeControlSize) texBoxControl.FitTextBoxSize();

                        texBoxControl.SetMoveModeTextBox();
                    }
                    break;
            }
             * */
        }

        private void GeometryChange(BaseGraphic baseGraphic, List<Point> NewChangedPoint, bool ChangeControlSize)
        {
            this.SetObjectPosition(baseGraphic, NewChangedPoint);

            switch (baseGraphic.Type)
            {
                case MapObjectType.CameraVideo:
                    this.RefreshGraphicObjectVideo(baseGraphic as CameraVideoGraphic);
                    break;
                case MapObjectType.SplunkControl:
                    if (ChangeControlSize) this.ResizeSplunkControl(baseGraphic);
                    break;
                case MapObjectType.Text:
                    if (baseGraphic is PolygonControlGraphic<TextBoxControl> && isEditMode)
                    {
                        var texBoxControl = (baseGraphic as ControlGraphic<TextBoxControl>).Control as TextBoxControl;
                        if (ChangeControlSize) texBoxControl.FitTextBoxSize();

                        texBoxControl.SetMoveModeTextBox();
                    }
                    break;
            }
        }

        private void Editor_eMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_GraphicContextMenu != null && _GraphicContextMenu.Visibility == System.Windows.Visibility.Visible)
            {
                _GraphicContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Editor_eMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GraphicContextMenuShow(this.selectedGraphicList);
        }

        #endregion Graphic Geometry Editor Event Handlers

        #region ContextMenu Action Event Handlers
        private void GraphicContextMenu_Select(List<BaseGraphic> TargetGraphicList)
        {
            this.ReleaseAllSelectedObject();
            TargetGraphicList.ForEach(Item => this.ChangeGraphicSelection(Item));
        }

        private void GraphicContextMenu_DeSelect(List<BaseGraphic> TargetGraphicList)
        {
            this.ReleaseAllSelectedObject();
        }

        private void GraphicContextMenu_Delete(List<BaseGraphic> TargetGraphicList)
        {
            this.ReleaseAllSelectedObject();
            TargetGraphicList.ForEach(Item => this.ChangeGraphicSelection(Item));

            this.DeleteSelectedObjects();
        }

        private void GraphicContextMenu_Copy(List<BaseGraphic> TargetGraphicList)
        {
            this.ReleaseAllSelectedObject();
            TargetGraphicList.ForEach(Item => this.ChangeGraphicSelection(Item));

            this.SelectedItemCopy();
        }

        private void GraphicContextMenu_Lock(List<BaseGraphic> TargetGraphicList)
        {
            TargetGraphicList.ForEach(Item => Item.IsLocked = true);
        }

        private void GraphicContextMenu_UnLock(List<BaseGraphic> TargetGraphicList)
        {
            TargetGraphicList.ForEach(Item => Item.IsLocked = false);
        }
        #endregion

        #region IDisposable Members

        private bool disposed;

        private void DisposeAction()
        {
            if (this.linkedMapHistory != null) this.linkedMapHistory.Clear();

            if (this.editor != null)
            {
                this.editor.eGraphicGeometryChanged -= EditorOnEGraphicGeometryChanged;
                this.editor = null;
            }

            /*
            if (this.searchedAddressInfoWindow != null)
            {
                this.searchedAddressInfoWindow.MouseLeftButtonDown -= InfoWindowOnMouseLeftButtonDown;
                this.searchedAddressInfoWindow.MouseLeftButtonUp -= InfoWindowOnMouseLeftButtonUp;
                this.searchedAddressInfoWindow.MouseMove -= InfoWindowOnMouseMove;
                this.searchedAddressInfoWindow = null;
            }
            */
            if (this.mapDataServiceHandler != null)
            {
                this.mapDataServiceHandler.eGetMapDataProcessCompleted -= MapDataServiceHandlerOnEGetMapDataProcessCompleted;
                this.mapDataServiceHandler.CancelGetMapFeatureData();
            }

            if (this.propertyManager != null)
            {
                this.propertyManager.onApplyLinkZoneSplunkData -= PropertyManagerOnApplyLinkZoneSplunkData;
                this.propertyManager.onApplySplunkArgumentData -= PropertyManagerOnOnApplySplunkArgumentData;
                this.propertyManager.CloseWindow();
            }

            if (this.workStationControlManager != null)
            {
                this.workStationControlManager.onGoLinkedMap -= WorkStationControlManager_OnGoLinkedMap;
                this.workStationControlManager.onShowSearchViewControl -= WorkStationControlManager_OnShowSearchViewControl;
                this.workStationControlManager.Dispose();
            }

            if (this.mapSplTimer != null)
            {
                this.mapSplTimer.Stop();
                this.mapSplTimer.Elapsed -= this.MapSplTimerOnElapsed;
                this.mapSplTimer = null;
            }

            if (this.cameraOverlayControl != null)
            {
                this.cameraOverlayControl.PreviewMouseLeftButtonDown -= this.cameraOverlayControl_PreviewMouseLeftButtonDown;
            }

            if (this.cameraPopupControlManager != null)
            {
                this.cameraPopupControlManager.eCloseButtonClicked -= this.CameraPopupManager_eCloseButtonClicked;
                this.cameraPopupControlManager.ePresetListSelectionChanged -= this.CameraPopup_ePresetListSelectionChanged;
                this.cameraPopupControlManager.eControlOpend -= this.CameraPopup_eControlOpend;
            }

            if (this.savedSplunkObjectDataManager != null)
            {
                this.savedSplunkObjectDataManager.eColorChanged -= this.SavedSplunkObjectDataManager_ColorChanged;
            }

            if (this.dynamicSplunkControlManager != null)
            {
                this.dynamicSplunkControlManager.DataRowClick -= this.dynamicSplunkControlManagerDataRowClick;
                this.dynamicSplunkControlManager.ButtonColumnCellClick -= this.dynamicSplunkControlManagerButtonColumnCellClick;
            }

            if (this.splunkPopupControlManager != null)
            {
                this.splunkPopupControlManager.eTableCellClick -= this.splunkPopupControlManager_eTableCellClick;
            }

            if (this.publicGraphicDataManager != null)
            {
                this.publicGraphicDataManager.eColorChanged -= this.PublicGraphicDataManager_ColorChanged;
                this.publicGraphicDataManager.eWorkstationDataChanged -= this.PublicGraphicDataManager_WorkstationDataChanged;
                this.publicGraphicDataManager.homeBookmarkChanged -= this.PublicGraphicDataManager_homeBookmarkChanged;
            }

            if (this.MemoPropertyWindow != null)
            {
                this.MemoPropertyWindow.MouseLeftButtonDown -= this.MemoPropertyWindow_MouseLeftButtonDown;
                this.MemoPropertyWindow.Close();
            }

            if (this.historyManager != null)
            {
                this.historyManager.Dispose();
            }

            this.disposed = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.DisposeAction();
                    //this.Dispose(true);
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(this.DisposeAction));
                }
            }

            base.Dispose(disposing);
        }

        #endregion // IDisposable Members

        #endregion //Events
    }
}
