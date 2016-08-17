using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Concurrent;
using System.Windows.Threading;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ArcGISControls.CommonData.Parsers;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using Innotive.SplunkControl.Table;
using Innotive.SplunkControl.Table.Event;
using Innotive.SplunkManager.SplunkManager;
using Innotive.SplunkManager.SplunkManager.Data;

namespace ArcGISControl
{
    using Language;

    public class MapSplStateEventArgs : EventArgs
    {
        public MapSplStateEventArgs(ArcGISClientViewer.MapSplStatus aStatus, string aMessage)
        {
            this.MapSplStatus = aStatus;
            this.Message = aMessage;
        }

        public ArcGISClientViewer.MapSplStatus MapSplStatus { get; set; }

        public string Message { get; set; }
    }

    public partial class ArcGISClientViewer
    {
        public enum MapSplStatus
        {
            Start,
            Stop,
            Request,
            Idle,
            Cancel,
            Fail
        }

        /// <summary>
        /// line to 정보를 담고 있을 Column의 이름이다.
        /// </summary>
        internal const string LineToColumnName = "_IW_LINETO";

        /// <summary>
        /// Splunk의 LineTo에 의해 생성된 Line을 그려주는 Graphic이다.
        /// </summary>
        protected Graphic splunkLineToGraphic;

        // MapSpl을 수행하기 위한 Splunk 서비스
        private SplunkService mapSplService;

        // MapSpl 기능을 수행하기 위한 정보
        private SplunkSavedSearchArgs mapSplArgs;

        private SplunkOperationPlaybackArgs mapSplOperationArgs;

        // MapSpl 업데이트 타이머
        private System.Timers.Timer mapSplTimer;

        // MapSpl Callback Action
        private Action<SplunkResultSet> mapSplCallbackAction;

        // MapSpl 요청 후 결과가 오기 전에 다시 시도되는 요청을 무시하기 위해 사용
        private bool isRequestingMapSpl;

        /// <summary>
        /// true로 설정 할 경우 맵 로드 시 SPL 변수들만 생성하고 타이머 시작을 지나치고 나서 다시 false로 변경된다.
        /// </summary>
        public bool isSkipMapSplTimerStart;

        #region Events

        public event EventHandler<ShowSplunkPostItEventArgs> eShowSplunkPostIt;

        private void RaiseShowSplunkPostIt(ShowSplunkPostItEventArgs e)
        {
            if (this.eShowSplunkPostIt != null)
            {
                this.eShowSplunkPostIt(this, e);
            }
        }


        public event EventHandler<MapSplStateEventArgs> eMapSplStateChanged;

        private void RaiseMapSplStateChanged(MapSplStateEventArgs e)
        {
            if (this.eMapSplStateChanged != null)
            {
                this.eMapSplStateChanged(this, e);
            }
        }

        #endregion // Events

        #region Methods

        /// <summary>
        /// Base Object Component 하나 추가
        /// </summary>
        /// <param name="mapSplunkObjectData"></param>
        protected override void MakeSplunkGraphic(MapSplunkObjectDataInfo mapSplunkObjectData)
        {
            if (mapSplunkObjectData == null) return;

            if (this.arcGISControlApi.SetMapSplunkData(mapSplunkObjectData.SplunkBasicInformation) != null)
            {
                base.MakeSplunkGraphic(mapSplunkObjectData);
            }
            else
            {
                mapSplunkObjectData = new MapSplunkObjectDataInfo()
                {
                    ObjectID = mapSplunkObjectData.ObjectID,
                    Name = mapSplunkObjectData.Name,
                    ControlSize = mapSplunkObjectData.ControlSize,
                    HiddenMaxLevel = mapSplunkObjectData.HiddenMaxLevel,
                    HiddenMinLevel = mapSplunkObjectData.HiddenMinLevel,
                    IconPosition = mapSplunkObjectData.IconPosition,
                    PointCollection = mapSplunkObjectData.PointCollection
                };

                base.MakeSplunkGraphic(mapSplunkObjectData);
            }

            var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(mapSplunkObjectData.ObjectID);

            if(splunkControl == null) return;

            if (this.IsConsoleMode)
            {
                splunkControl.MouseMove += SplunkControlOnMouseMove;
                splunkControl.MouseDown += SplunkControlOnMouseDown;
            }

            this.ChangeSplunkControlVisible(mapSplunkObjectData);
            this.ChangeSplunkServiceStatus(mapSplunkObjectData);

            if (!this.IsConsoleMode || !splunkControl.IsTableControl || splunkControl.TableControl == null) return;

            splunkControl.TableControl.Tag = mapSplunkObjectData.SplunkBasicInformation;
            splunkControl.TableControl.ButtonColumnCellClick += TableControlOnButtonColumnCellClick;
            splunkControl.TableControl.DataRowClick += TableControlOnDataRowClick;
        }

        /// <summary>
        /// Property 체크하여 
        /// 현재의 Splunk정보를 표현한다.
        /// </summary>
        protected void ChangeAllSplunkControlVisible()
        {
            foreach (var splunkObjectData in this.savedSplunkObjectDataManager.SplunkObjectDatas)
            {
                if (splunkObjectData != null) this.ChangeSplunkControlVisible(splunkObjectData);
            }
        }

        /// <summary>
        /// Property 체크하여 
        /// 현재의 Splunk정보를 표현한다.
        /// </summary>
        /// <param name="splunkObject"></param>
        private void ChangeSplunkControlVisible(MapSplunkObjectDataInfo splunkObject)
        {
            var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(splunkObject.ObjectID, MapObjectType.SplunkControl)
                as PolygonControlGraphic<SplunkChartTableWrapperControl>;

            if (splunkControlGraphic == null) return;

            if ((splunkObject.HiddenMinLevel > 0 && splunkObject.HiddenMinLevel > Math.Round(mapLevel)) ||
                  (splunkObject.HiddenMaxLevel > 0 && splunkObject.HiddenMaxLevel < Math.Round(mapLevel)))
            {
                if (splunkControlGraphic.IsVisible) splunkControlGraphic.IsVisible = false;
            }
            else
            {
                if (!splunkControlGraphic.IsVisible) splunkControlGraphic.IsVisible = true;
            }
        }

        private void ChangeAllSplunkServiceStatus()
        {
            foreach (var splunkObjectData in this.savedSplunkObjectDataManager.SplunkObjectDatas)
            {
                if (splunkObjectData != null) this.ChangeSplunkServiceStatus(splunkObjectData);
            }
        }

        /// <summary>
        /// SplunK Control & Icon 이 화면 UI 에 보이는지 여부에 따라 
        /// Service를 Start Or Stop 한다.
        /// </summary>
        /// <param name="splunkObject"></param>
        private void ChangeSplunkServiceStatus(MapSplunkObjectDataInfo splunkObject)
        {
            var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(splunkObject.ObjectID, MapObjectType.SplunkControl)
               as PolygonControlGraphic<SplunkChartTableWrapperControl>;

            var splunkIconGraphic = this.GetOneBaseGraphicInGraphicLayer(splunkObject.ObjectID, MapObjectType.SplunkIcon) as SplunkIconGraphic;

            if (splunkControlGraphic == null || splunkIconGraphic == null) return;

            #region SplunkService Start & Stop

            Rect SplunkIconRect = GeometryHelper.ToVideoRect(this.baseMap, splunkIconGraphic.Geometry.Extent, this.baseTiledMapServiceLayer.FullExtent);
            
            var isInMap = (this.IsInMap(splunkIconGraphic.Position) ||
                          (splunkControlGraphic.IsVisible && this.IsInMap(splunkControlGraphic.PointCollection)));

            var useSchedule = (splunkObject.UseSchedule != null && splunkObject.UseSchedule.Value && !this.isEditMode);

            int? schedulingInterval = (splunkObject.UseSchedule != null && splunkObject.UseSchedule.Value)
                                          ? splunkObject.IntervalSeconds
                                          : null;

            if (!ArcGISConstSet.AlwaysKeepToSplunkData && SplunkIconRect == Rect.Empty)
            {
                this.savedSplunkObjectDataManager.StopSplunkService(splunkObject.ObjectID, splunkObject.SplunkBasicInformation);
            }
            else
            {
                // issue #17835 의 이유로 아래 if (isInMap) 조건 주석 처리함.
                //if (isInMap)
                //{
                if (this.IsDataPlayBackMode)
                {
                    var timeSpan = DateTime.Now - this.DataPlayBackPosition;

                    schedulingInterval = ArcGISConstSet.PlaybackSchedulingInterval;

                    this.savedSplunkObjectDataManager.StartSplunkService(splunkObject.ObjectID,
                                                                         splunkObject.SplunkBasicInformation,
                                                                         timeSpan, this.IsDoingDataPlayBack,
                                                                         this.OperationPlaybackArgs,
                                                                         schedulingInterval);
                }
                else
                {
                    this.savedSplunkObjectDataManager.StartSplunkService(splunkObject.ObjectID,
                                                                         splunkObject.SplunkBasicInformation,
                                                                         useSchedule,
                                                                         schedulingInterval);

                }
                //}
            }
            if ((!this.IsDataPlayBackMode && useSchedule) || (this.IsDataPlayBackMode && this.IsDoingDataPlayBack))
            {  
                //posco poc에만 적용된 사항 : 스케쥴, 데이터 플레이백 시 패닝, 줌확대/축소 시에 Stop 하지 않고 계속 수행하도록 수정. 개선 필요.
                //this.savedSplunkObjectDataManager.StopSplunkService(splunkObject.ObjectID, splunkObject.SplunkBasicInformation); 
                if (ArcGISConstSet.StopSplunkControlOutOfScreen)
                {
                    this.savedSplunkObjectDataManager.StopSplunkService(splunkObject.ObjectID, splunkObject.SplunkBasicInformation);
                }
            }

            #endregion Start & Stop
        }

        private void ChangeLinkzonGraphicColor(string graphicId, string color)
        {
            var linkZoneObjcetGraphic = this.GetOneBaseGraphicInGraphicLayer(graphicId, MapObjectType.ImageLinkZone);

            if (linkZoneObjcetGraphic == null)
                linkZoneObjcetGraphic = this.GetOneBaseGraphicInGraphicLayer(graphicId, MapObjectType.LinkZone);

            if (linkZoneObjcetGraphic is LinkZoneGraphic)
            {
                var newColor = (Color)ColorConverter.ConvertFromString(color);
                var selectedColor = Color.FromArgb(127, newColor.R, newColor.G, newColor.B);

                (linkZoneObjcetGraphic as LinkZoneGraphic).ChangeColors(newColor, newColor, selectedColor);
            }
        }

        private void ResizeSplunkControl(BaseGraphic baseGraphic)
        {
            var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(baseGraphic.ObjectID);
            if (splunkControl == null) return;

            var width = baseGraphic.Geometry.Extent.Width / this.baseMap.Resolution;
            var height = baseGraphic.Geometry.Extent.Height / this.baseMap.Resolution;
            splunkControl.SetSplunkControlSize(width, height);
        }

        /// <summary>
        /// 지정된 현재 Map의 좌표계로 된 MapPoint 두 개를 잇는 직선을 그린다.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        protected void DrawLine(MapPoint point1, MapPoint point2)
        {
            if (point1 == null) throw new ArgumentNullException("point1");
            if (point2 == null) throw new ArgumentNullException("point2");

            var graphicToAdd = new Graphic()
            {
                Geometry = new Polyline()
                {
                    Paths = new ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection>()
                        {
                            new ESRI.ArcGIS.Client.Geometry.PointCollection()
                            {
                                point1,
                                point2
                            }
                        },
                    SpatialReference = this.baseMap.SpatialReference
                },
                Symbol = new SimpleLineSymbol()
                {
                    Color = ArcGISConstSet.LineToColor,
                    Style = SimpleLineSymbol.LineStyle.Solid,
                    Width = ArcGISConstSet.LineToWidth
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

        protected void SplunkClickSearch(SplunkBasicInformationData basicInformation, DataRow dataRow)
        {
            var dataTable = dataRow.Table;

            if (dataTable.Columns.Contains("_IW_CLICK_SEARCH"))
            {
                var search = dataRow.Field<string>("_IW_CLICK_SEARCH");
                var searchApp = dataTable.Columns.Contains("_IW_CLICK_SEARCH_APP") ?
                    dataRow.Field<string>("_IW_CLICK_SEARCH_APP") : null;

                if (this.IsDataPlayBackMode)
                {
                    var timeSpan = DateTime.Now - this.DataPlayBackPosition;

                    this.dynamicSplunkControlManager.StartFetchingDataFromSplSearch(search, searchApp, basicInformation, timeSpan, this.OperationPlaybackArgs);
                }
                else
                {
                    this.dynamicSplunkControlManager.StartFetchingDataFromSplSearch(search, searchApp, basicInformation);
                }

                this.dynamicSplunkControlManager.Show(this.mainCanvas);
            }
        }

        protected void SplunkLineTo(DataRow dataRow)
        {
            var dataTable = dataRow.Table;

            if (dataTable.Columns.Contains(LineToColumnName))
            {
                var data = dataRow.Field<string>(LineToColumnName);
                var parseResult = data == null ? null : SplunkLineToParser.ParseLineInfo(data);

                if (parseResult != null)
                {
                    var o1 = new LatLng(parseResult.Item1, parseResult.Item2);
                    var o2 = new LatLng(parseResult.Item3, parseResult.Item4);

                    var p1 = this.FromGeographicAdjustForUser(o1);
                    var p2 = this.FromGeographicAdjustForUser(o2);

                    this.DrawLine(new MapPoint(p1.X, p1.Y), new MapPoint(p2.X, p2.Y));
                }
                else
                {
                    this.arcGISControlApi.ShowMessagePopup(
                        string.Format(Resource_ArcGISControl_ArcGISClientViewer.Message_IllFormedLineToData,
                                      data ?? "<Null>"), true);
                }
            }
        }

        protected void SplunkPostIt(SplunkBasicInformationData basicInformation, DataRow dataRow)
        {
            if (dataRow == null) return;

            var postItData = SplunkPostItParser.Parse(dataRow);
            if (postItData == null) return;

            var showSplunkPostItEventArgs = new ShowSplunkPostItEventArgs(basicInformation, postItData);
            this.RaiseShowSplunkPostIt(showSplunkPostItEventArgs);
        }

        protected void SplunkLinkedMap(string linkedMapData)
        {
            var data = linkedMapData;
            var maps = SplunkMapListParser.ParseMapList(data);

            if (maps != null)
            {
                this.mapTransitionChain = new ConcurrentQueue<string>(maps);

                this.ProcessNextTransitionChain();
            }
            else
            {
                this.arcGISControlApi.ShowMessagePopup(
                    string.Format(Resource_ArcGISControl_ArcGISClientViewer.Message_IllFormedLinkedMapData,
                                  data ?? "<Null>"), true);
            }
        }

        #region MapSpl
        private void StartMapSpl(MapSettingDataInfo aMapSettingDataInfo)
        {
            if (aMapSettingDataInfo == null || !aMapSettingDataInfo.UseMapSpl || !aMapSettingDataInfo.IsValidMapSplData())
            {
                return;
            }

            this.StartMapSpl(aMapSettingDataInfo.MapSplHost, aMapSettingDataInfo.MapSplPort, aMapSettingDataInfo.MapSplApp,
                             aMapSettingDataInfo.MapSplId, aMapSettingDataInfo.MapSplPassword,
                             aMapSettingDataInfo.MapSplIntervalSeconds, aMapSettingDataInfo.MapSplQuery, aMapSettingDataInfo.MapSplEarliestMinutes);
        }

        private void StartMapSpl(string host, int port, string appName, string userId, string password, int interval, string spl, int preTimeMin)
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            try
            {
                if (this.mapSplTimer == null)
                {
                    return;
                }

                if (this.mapSplTimer.Enabled || this.isRequestingMapSpl)
                {
                    this.StopMapSpl();
                }

                var args = new SplunkServiceFactoryArgs
                {
                    Host = host,
                    Port = port,
                    App = appName,
                    UserName = userId,
                    UserPwd = password,
                };

                this.mapSplService = SplunkServiceFactory.Instance.GetSplunkService(args);
   
                this.mapSplArgs = new SplunkSavedSearchArgs
                    {
                        SearchIndex = Guid.NewGuid().ToString(),
                        Name = spl,
                        OperationPlaybackArgs =
                            new SplunkOperationPlaybackArgs {PreMinutes = preTimeMin == 0 ? 0 : preTimeMin, PostMinutes = 0}
                    };

                this.mapSplCallbackAction = new Action<SplunkResultSet>(
                    (resultSet) =>
                        {
                            // 이전 서치 스킵...
                            if (!resultSet.SplunkSearchIndex.Equals(this.mapSplArgs.SearchIndex))
                            {
                                return;
                            }

                            if (resultSet.SplunkException != null)
                            {
                                this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Fail, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplFail));
                                InnowatchDebug.Logger.WriteLogExceptionMessage(resultSet.SplunkException, resultSet.SplunkException.GetType().ToString(), false);
                            }

                            // cancel일 경우 이렇게 들어오는듯...
                            if (resultSet.SplunkDataTable == null && resultSet.SplunkException == null && resultSet.SplunkReportData == null)
                            {
                                this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Cancel, null));
                                return;
                            }

                            this.isRequestingMapSpl = false;
                            this.ApplyMapSplResult(resultSet);
                            this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Idle, this.universalObjectDataManager.QueryStatusMessage));
                        }
                    );

                this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Start, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplLoading));

                if (interval < 1)
                {
                    this.mapSplTimer.AutoReset = false;
                }
                else
                {
                    this.mapSplTimer.Interval = interval * 1000;
                    this.mapSplTimer.AutoReset = true;
                }

                if (!this.isSkipMapSplTimerStart && !this.IsDataPlayBackMode)
                {
                    this.MapSplTimerOnElapsed(null, null);

                    if (this.mapSplTimer.AutoReset)
                    {
                        this.mapSplTimer.Start();
                    }
                }
                else
                {
                    this.isSkipMapSplTimerStart = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MapSplTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (this.IsDataPlayBackMode && !this.IsDoingDataPlayBack)
            {
                this.PauseMapSplForPlayback();
            }

            this.RequestMapSpl();
        }

        private void StopMapSpl()
        {
            if (this.mapSettingDataInfo == null || !this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            try
            {
                if (this.mapSplTimer != null)
                {
                    this.mapSplTimer.Stop();
                }

                if (this.mapSplService == null || this.mapSplArgs == null)
                {
                    return;
                }

                this.mapSplService.BeginExecuteAbortSearchThread(this.mapSplArgs.SearchIndex);
                this.isRequestingMapSpl = false;
                this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Stop, null));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RequestMapSpl()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            if (this.isRequestingMapSpl)
            {
                if (this.IsDataPlayBackMode)
                {
                    this.mapSplService.BeginExecuteAbortSearchThread(this.mapSplArgs.SearchIndex);
                    this.isRequestingMapSpl = false;
                }
                else
                {
                    return;
                }
            }

            try
            {
                this.isRequestingMapSpl = true;

                var newArgs = new SplunkSavedSearchArgs
                                  {
                                      SearchIndex = this.mapSplArgs.SearchIndex,
                                      Name = this.mapSplArgs.Name,
                                      OperationPlaybackArgs = this.mapSplArgs.OperationPlaybackArgs,
                                      TimeLineDateTime = DateTime.Now,
                                      TimeLineTimeSpan = TimeSpan.Zero,
                                  };

                if (this.IsDataPlayBackMode)
                {
                    newArgs.TimeLineDateTime = DateTime.Now;
                    newArgs.TimeLineTimeSpan = newArgs.TimeLineDateTime - this.DataPlayBackPosition;
                }

                this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Request, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplLoading));
                this.mapSplService.BeginExecuteSplSearch(this.mapSplCallbackAction, newArgs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                this.StopMapSpl();
                this.isSkipMapSplTimerStart = true;
                this.StartMapSpl(this.mapSettingDataInfo);
            }
        }

        private void ApplyMapSplResult(SplunkResultSet result)
        {
            this.universalObjectDataManager.ParseAndApplyMapSearchResult(result, this.mapSettingDataInfo.MapSplClearPreviousResult);

            if (this.universalObjectDataManager.ContainsId(this.splunkPopupControlManager.ShowingSplunkId))
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var metric = this.universalObjectDataManager.GetMetric(this.splunkPopupControlManager.ShowingSplunkId);
                        var iconGraphic = this.universalObjectDataManager.GetIconGraphic(this.splunkPopupControlManager.ShowingSplunkId);

                        var position = new Point(
                            (this.GetWrapAroundPoint(iconGraphic.Geometry.Extent.XMin) - baseMap.Extent.XMin) / baseMap.Resolution,
                            (baseMap.Extent.YMax - iconGraphic.Geometry.Extent.YMax) / baseMap.Resolution
                        );

                        this.splunkPopupControlManager.Show(position, iconGraphic.ObjectID, metric, "Metrics", this.universalObjectDataManager.CellLinkTable);
                    }), DispatcherPriority.Loaded);
            }
        }

        private void StartMapSplForPlayback()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            this.mapSplTimer.Stop();

            this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Start, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplLoading));
            this.RequestMapSpl();
        }

        private void StopMapSplForPlayback()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Start, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplLoading));
            this.RequestMapSpl();
            this.mapSplTimer.Start();
        }

        private void PlayMapSplForPlayback()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            this.RequestMapSpl();
            this.mapSplTimer.Start();
        }

        private void PauseMapSplForPlayback()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            this.mapSplTimer.Stop();
        }

        private void SeekMapSplForPlayback()
        {
            if (!this.mapSettingDataInfo.UseMapSpl)
            {
                return;
            }

            this.mapSplTimer.Stop();

            this.RaiseMapSplStateChanged(new MapSplStateEventArgs(MapSplStatus.Start, Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplLoading));
            this.RequestMapSpl();
        }

        #endregion //MapSpl

        #endregion Methods

        #region Event Handlers

        private void SplunkControlOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!this.isEditMode)
            {
                return;
            }

            mouseButtonEventArgs.Handled = true;

            var splunkControl = sender as SplunkChartTableWrapperControl;

            if (splunkControl == null) return;

            var objectData = this.savedSplunkObjectDataManager.GetObjectDataByObjectID(splunkControl.ObjectID);

            this.SelectObject(objectData);

            this.ChangeCursor(Cursors.Arrow);
        }

        private void SplunkControlOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!this.isEditMode)
            {
                return;
            }

            this.ChangeCursor(Cursors.Arrow);
            mouseEventArgs.Handled = true;
        }

        private void TableControlOnDataRowClick(object sender, DataRowClickEventArgs dataRowClickEventArgs)
        {
            var tableControl = sender as TableControl;
            if (tableControl == null) return;
            if (dataRowClickEventArgs == null) return;
            if (dataRowClickEventArgs.DataRow == null) return;
            if (dataRowClickEventArgs.DataRow.Table == null) return;

            var dataRow = dataRowClickEventArgs.DataRow;

            this.SplunkLineTo(dataRow);

            var basicInformation = tableControl.Tag as SplunkBasicInformationData;
            if (basicInformation == null) return;

            this.SplunkClickSearch(basicInformation, dataRow);
        }

        private void dynamicSplunkControlManagerDataRowClick(object sender, DataRowClickEventArgs dataRowClickEventArgs)
        {
            var tableControl = sender as TableControl;
            if (tableControl == null) return;
            if (dataRowClickEventArgs == null) return;
            if (dataRowClickEventArgs.DataRow == null) return;
            if (dataRowClickEventArgs.DataRow.Table == null) return;

            var dataRow = dataRowClickEventArgs.DataRow;

            this.SplunkLineTo(dataRow);

            var basicInformation = tableControl.Tag as SplunkBasicInformationData;
            if (basicInformation == null) return;

            this.SplunkPostIt(basicInformation, dataRow);
        }

        private void TableControlOnButtonColumnCellClick(object sender, ButtonColumnCellClickEventArgs buttonColumnCellClickEventArgs)
        {
            this.SplunkLinkedMap(buttonColumnCellClickEventArgs.CellData);
        }

        private void dynamicSplunkControlManagerButtonColumnCellClick(object sender, ButtonColumnCellClickEventArgs buttonColumnCellClickEventArgs)
        {
            this.SplunkLinkedMap(buttonColumnCellClickEventArgs.CellData);
        }

        private void splunkPopupControlManager_eTableCellClick(object sender, CellClickEventArgs e)
        {
            var url = this.universalObjectDataManager.GetCellLinkUrl(e.ObjectId, e.RowIndex, e.ColumnIndex);

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            this.arcGisControlViewerApi.OpenLinkzoneViewControl(url);
        }
        #endregion //Event Handlers
    }
}
