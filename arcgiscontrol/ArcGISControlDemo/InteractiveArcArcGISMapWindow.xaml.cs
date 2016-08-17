using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArcGISControl;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Interface;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ArcGISControls.Tools;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace ArcGISControlDemo
{
    /// <summary>
    /// Interaction logic for InteractiveArcArcGISMapWindow.xaml
    /// </summary>
    public partial class InteractiveArcArcGISMapWindow : Window, IArcGISControlViewerAPI
    {
        private MapControlType mapControlType = MapControlType.ManagerEditMode;

        private ArcGISClientViewer arcGisClientViewer;

        private List<MapSettingDataInfo> mapSettingDataInfos = new List<MapSettingDataInfo>();

        private MapSettingDataInfo mapSettingDataInfo;

        private MapSettingDataInfo bingMapSettingDataInfo;

        private MapSettingDataInfo esriMapSettingDataInfo;

        private MapSettingDataInfo naverMapSettingDataInfo;

        private MapSettingDataInfo daumMapSettingDataInfo;

        private VWToolControl vwToolControl;

        private readonly string dataServiceIp = "68.68.101.10";
        private readonly int dataServicePort = 25000;

        public InteractiveArcArcGISMapWindow()
        {
            InitializeComponent();

            this.mapSettingDataInfo = new MapSettingDataInfo()
            {
                CustomMapServiceDir = "3f09d345-4746-4b1e-a3e1-c7f1828cf259_140704_14329",
                CustomMapServiceGuid = "CustomMapService",
                Description = "dddxddsdffgsdfg",
                ID = "000d915e-3821-45cb-8b6c-32a207cb1af7",
                ExtentMax = new Point(1997.22149410223, 1200),
                ExtentMin = new Point(-77.221494102228, 0),
                MapServiceUrl = "http://68.68.101.10:34000/3f09d345-4746-4b1e-a3e1-c7f1828cf259_140704_14329",
                MapType = MapProviderType.CustomMap,
                Name = "INFRA_zzrwpadb",
                Version = 1
            };



            var customDataInfo1 = new MapSettingDataInfo()
            {
                CustomMapServiceDir = "8d14da76-e637-4a86-8e07-2df461984865_140704_14329",
                CustomMapServiceGuid = "CustomMapService",
                Description = "INFRA_rpf",
                ID = "0048dc2e-2cd8-45ea-b4af-763397e7fc24",
                ExtentMax = new Point(3770.43831640058, 2037.10885341074),
                ExtentMin = new Point(1406.03773584906, 741.10885341074),
                MapServiceUrl = "http://68.68.101.10:34000/8d14da76-e637-4a86-8e07-2df461984865_140704_14329",
                MapType = MapProviderType.CustomMap,
                Name = "INFRA_rpf",
                Version = 1
            };

            var customDataInfo2 = new MapSettingDataInfo()
            {
                CustomMapServiceDir = "ac65408c-ec13-41f7-8a10-a354c37aee33_140704_14329",
                CustomMapServiceGuid = "CustomMapService",
                Description = "INFRA_zzbwcrrq",
                ID = "004c1121-f588-4943-83ee-308a7cfe3533",
                ExtentMax = new Point(3770.43831640058, 2037.10885341074),
                ExtentMin = new Point(1406.03773584906, 741.10885341074),
                MapServiceUrl = "http://68.68.101.10:34000/ac65408c-ec13-41f7-8a10-a354c37aee33_140704_14329",
                MapType = MapProviderType.CustomMap,
                Name = "INFRA_zzbwcrrq",
                Version = 1
            };

            var customDataInfo3 = new MapSettingDataInfo()
            {
                CustomMapServiceDir = "4d2c1f86-829b-4873-b633-ec134f605594_140722_205201",
                CustomMapServiceGuid = "CustomMapService",
                Description = "冬の大三角形＠KSHO",
                ID = "00573fb4-577e-4595-a52c-a92fb17e1a37",
                ExtentMax = new Point(3770.43831640058, 2037.10885341074),
                ExtentMin = new Point(1406.03773584906, 741.10885341074),
                MapServiceUrl = "http://68.68.101.10:34000/4d2c1f86-829b-4873-b633-ec134f605594_140722_205201",
                MapType = MapProviderType.CustomMap,
                Name = "APP_Cisco IT - Export Max Supply+Kinaxis+Supply Shortage Analysis",
                Version = 1
            };
            
            this.bingMapSettingDataInfo = new MapSettingDataInfo()
            {  
                LicenseKey = "AtTKHpsuWBem1qlizV1EdBj6AywW5-kU64vwOmTz5ksLZhiXSq20WBnTGhuAKpQF",
                CustomMapServiceGuid = "CustomMapService",
                Description = "dddxddsdffgsdfg",
                ID = "1b61b5a8-80d4-4859-8dec-19225a18fe79",
                MapServiceUrl = "http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial",
                MapType = MapProviderType.BingArialMap,
                Name = "001-WorldMap",
                Version = 1,
                ExtentMax = new Point(-115.171658359941, 36.1155206120965),
                ExtentMin = new Point(-115.189317385706, 36.1080684570361),
                HomeExtent = new MapSettingDataInfo.Extent() { XMax = 21236054.546275906, XMin = -18838962.139302492, YMax = 13368947.390914002, YMin = -6653369.5126463939 },
            };


            this.esriMapSettingDataInfo = new MapSettingDataInfo()
            {
                LicenseKey = "AtTKHpsuWBem1qlizV1EdBj6AywW5-kU64vwOmTz5ksLZhiXSq20WBnTGhuAKpQF",
                CustomMapServiceGuid = "CustomMapService",
                Description = "dddxddsdffgsdfg",
                ID = "2c3d25e9-8694-4820-a0ad-76c97232dff6",
                MapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer",
                MapType = MapProviderType.ArcGisClientMap,
                Name = "Internet Map",
                Version = 1,
                ExtentMax = new Point(92.7297472300035, 68.2527821333539),
                ExtentMin = new Point(-113.16602942743, -12.757772286672),
            };


            this.naverMapSettingDataInfo = new MapSettingDataInfo()
            {
                LicenseKey = "9eb74d59f55e25d243f5658b0594f0e4",
                CustomMapServiceGuid = "CustomMapService",
                Description = "dddxddsdffgsdfg",
                ID = "60dab24a-8dda-43f1-9965-b20f81891243",
                MapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer",
                MapType = MapProviderType.NaverMap,
                Name = "NaverMap",
                Version = 1,
                ExtentMax = new Point(126.803056004787, 37.5339286363265),
                ExtentMin = new Point(126.742970251549, 37.5058713119064),
            };

            this.daumMapSettingDataInfo = new MapSettingDataInfo()
            {
                LicenseKey = "9a5911554d5ad8eb9a7cab3b0bd8469a6e64a500",
                CustomMapServiceGuid = "CustomMapService",
                Description = "dddxddsdffgsdfg",
                ID = "60dab24a-8dda-43f1-9965-b20f81891243",
                MapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer",
                MapType = MapProviderType.DaumMap,
                Name = "DaumMap",
                Version = 1,
                ExtentMax = new Point(126.803056004787, 37.5339286363265),
                ExtentMin = new Point(126.742970251549, 37.5058713119064),
            };

            this.mapSettingDataInfos.Add(mapSettingDataInfo);
            this.mapSettingDataInfos.Add(bingMapSettingDataInfo);
            this.mapSettingDataInfos.Add(esriMapSettingDataInfo);
            this.mapSettingDataInfos.Add(naverMapSettingDataInfo);
            this.mapSettingDataInfos.Add(daumMapSettingDataInfo);
            this.mapSettingDataInfos.Add(customDataInfo1);
            this.mapSettingDataInfos.Add(customDataInfo2);
            this.mapSettingDataInfos.Add(customDataInfo3);
            
            this.Loaded += OnLoaded;
            this.xbutton.Click += XbuttonOnClick;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.xListBoxMap.ItemsSource = mapSettingDataInfos;
            this.xListBoxMap.SelectionChanged += XListBoxMapOnSelectionChanged;

            //this.xListBoxMapType.ItemsSource = Enum.GetNames(typeof(MapProviderType)).ToList();
            //this.xListBoxMapType.SelectionChanged += XListBoxMapTypeOnSelectionChanged;

            var searchListControl = new SearchListControl();
           
            
            this.xGridSearchedList.Children.Add(searchListControl);
            this.arcGisClientViewer = new ArcGISClientViewer(dataServiceIp, dataServicePort, mapControlType, this);
            this.arcGisClientViewer.eLevelChanged += ArcGisMapOnELevelChanged;
            this.arcGisClientViewer.eObjectLoaded += ArcGisClientViewerOnEObjectLoaded;
            this.xMapControl.Children.Add(this.arcGisClientViewer);

            this.vwToolControl = new VWToolControl { ArcGisClientViewer = this.arcGisClientViewer };
            this.vwToolControl.eObjectAdded += VwToolControlOnEObjectAdded;

            this.xMapControl.Children.Add(vwToolControl);
            vwToolControl.VerticalAlignment = VerticalAlignment.Bottom;
            vwToolControl.HorizontalAlignment = HorizontalAlignment.Stretch;

            xLabelType.Content = mapControlType.ToString();
            searchListControl.ArcGisClientViewer = arcGisClientViewer;

            //this.vwToolControl.eChangedPlaybackMode += vwToolControl_eChangedPlaybackMode;

            var searchViewControl = new ArcGISControls.Tools.SearchViewControl.SearchViewControl(
                Guid.Empty,
                this.arcGisClientViewer,
                new SessionInfo() { Id = "admin", Password = "inno1029#", Url = "http://172.16.40.49:8000/dj/en-us/perseus/map_search", UseSessionInfo = true },
                new SessionInfo() { Id = "admin", Password = "inno1029#", Url = "http://172.16.40.49:8000/dj/en-us/perseus/trend_analysis", UseSessionInfo = true }
            );
            this.xMapControl.Children.Add(searchViewControl);
        }

        private void XListBoxMapOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            if (selectionChangedEventArgs.AddedItems != null && selectionChangedEventArgs.AddedItems.Count == 1)
            {
                var mapSettingData = new MapSettingDataInfo(this.xListBoxMap.SelectedItem as MapSettingDataInfo);

                if (this.arcGisClientViewer != null)
                {
                    mapSettingData.MapType =
                        (MapProviderType)Enum.Parse(typeof(MapProviderType), mapSettingData.MapType.ToString());

                    switch (mapSettingData.MapType)
                    {
                        case MapProviderType.DaumSatelliteHybridMap:
                        case MapProviderType.DaumMap:
                        case MapProviderType.DaumSatelliteMap:
                        case MapProviderType.DaumSatelliteTrafficMap:
                        case MapProviderType.DaumTrafficMap:

                            mapSettingData.LicenseKey = this.daumMapSettingDataInfo.LicenseKey;
                            mapSettingData.MapServiceUrl = this.daumMapSettingDataInfo.MapServiceUrl;

                            break;
                        case MapProviderType.NaverSatelliteHybridMap:
                        case MapProviderType.NaverMap:
                        case MapProviderType.NaverSatelliteMap:
                        case MapProviderType.NaverSatelliteTrafficMap:
                        case MapProviderType.NaverTrafficMap:

                            mapSettingData.LicenseKey = this.naverMapSettingDataInfo.LicenseKey;
                            mapSettingData.MapServiceUrl = this.naverMapSettingDataInfo.MapServiceUrl;

                            break;
                        case MapProviderType.ArcGisImageryMap:
                        case MapProviderType.ArcGisStreetMap:
                        case MapProviderType.ArcGisTogoMap:
                        case MapProviderType.ArcGisClientMap:

                            mapSettingData.LicenseKey = this.esriMapSettingDataInfo.LicenseKey;
                            mapSettingData.MapServiceUrl = this.esriMapSettingDataInfo.MapServiceUrl;

                            break;
                        case MapProviderType.BingMap:
                        case MapProviderType.BingArialMap:
                        case MapProviderType.BingArialWithLabelMap:

                            mapSettingData.LicenseKey = this.bingMapSettingDataInfo.LicenseKey;
                            mapSettingData.MapServiceUrl = this.bingMapSettingDataInfo.MapServiceUrl;

                            break;
                    }

                    this.arcGisClientViewer.LoadMapData(mapSettingData);
                    this.arcGisClientViewer.LoadPrivateObjects(new ObservableCollection<BaseMapObjectInfoData>());

                    this.xLabelCurrentMaptype.Content = mapSettingData.MapType.ToString();
                }
            }
        }

        private void VwToolControlOnEObjectAdded(object sender, ObjectDataEventArgs objectDataEventArgs)
        {
            MessageBox.Show("Added Private Object" + objectDataEventArgs.mapObjectDataInfo.Name);
        }

        private void XListBoxMapTypeOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
        }

        private void XListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if(selectionChangedEventArgs.AddedItems != null && selectionChangedEventArgs.AddedItems.Count == 1)
            {
                //this.xListBox.IsEnabled = false;

                var mapSettingData = selectionChangedEventArgs.AddedItems[0] as MapSettingDataInfo;

                if (mapSettingData != null)
                {
                    this.arcGisClientViewer.LoadMapData(mapSettingData);
                    this.arcGisClientViewer.LoadPrivateObjects(new ObservableCollection<BaseMapObjectInfoData>());
                    //this.arcGisClientViewer.LoadPrivateObjects(new ObservableCollection<BaseMapObjectInfoData>());

                    //this.xListBoxMapType.IsEnabled = ArcGISDataConvertHelper.IsGISMapType(mapSettingData.MapType);

                    //this.xListBoxMapType.SelectedItem = mapSettingData.MapType.ToString();
                }
            }
        }

        private void ReleaseMap()
        {
            if (this.arcGisClientViewer == null) return;
            this.arcGisClientViewer.eLevelChanged -= ArcGisMapOnELevelChanged;
            this.xMapControl.Children.Remove(this.arcGisClientViewer);
            this.arcGisClientViewer = null;
        }

        private void XbuttonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.xMapControl.Children.Remove(this.arcGisClientViewer);

            this.arcGisClientViewer = null;
        }

        private void ArcGisMapOnELevelChanged(object sender, LevelChangedEventArgs levelChangedEventArgs)
        {
            xLabelLevel.Content = levelChangedEventArgs.Level.ToString();
        }


        private void ArcGisClientViewerOnEObjectLoaded(object sender, EventArgs eventArgs)
        {
            //MessageBox.Show("End");
        }


        #region Interface

        public void ExcutePreset(string id, string presetNumber)
        {
            //MessageBox.Show(string.Format("{0}  : {1} ", id, presetNumber));
        }

        public bool GetMapCellVisible()
        {
            return true;
        }

        public bool GetOnHideAllCameraStatus()
        {
            return false;
        }

        public void OpenSearchViewControl(string url)
        {
            
        }

        #endregion //Interface

        #region ArcGIS Interface APIS

        /// <summary>
        /// ImageBlush 생성
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <returns></returns>
        public ImageSource GetImageBrush(int centerX, int centerY)
        {
            //return DxCameraRenderer.Instance.GetImageSource(centerX, centerY);
            return null;
        }

        /// <summary>
        /// Camera Border 색 qusrud
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="isSelect"></param>
        public void SetBorderCameraVideo(string unigridGuid, bool isSelect)
        {
            /*
            uint SelectCameraBorderColor = 4294901760;
            int SelectCameraBorderThickness = 2;

            uint borderColor = constset.NormalCameraBorderColor;
            int borderThickness = constset.NormalCameraBorderThickness;

            if (isSelect)
            {
                borderColor = SelectCameraBorderColor;
                borderThickness = SelectCameraBorderThickness;
            }

            var guid = new Guid(unigridGuid);

            DxCameraRenderer.Instance.ShowCellBorder(guid, isSelect, borderColor, borderThickness);
             * */
        }

        /// <summary>
        /// Camera Preset List를 받아 온다
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <returns></returns>
        public int GetPresetCount(string cameraGuid)
        {
            /*
            var camera = ConfigData.Instance.CameraInformations.FirstOrDefault(CameraInformation => CameraInformation.General.Id == cameraGuid);

            return camera != null && camera.Ptz.IsActivate ? camera.Ptz.PresetCount : 0;
             * */

            return 14;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unigridGuid"></param>
        public void ShowVideo(string unigridGuid)
        {
            //DxCameraRenderer.Instance.CreateUnigrid()
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unigridGuid"></param>
        public void HideVideo(string unigridGuid)
        {
            //DxCameraRenderer.Instance.MoveUniGrid(0, new Guid(unigridGuid), 0, 0, 0, 0);
        }

        /// <summary>
        /// Unigrid만 지우기
        /// </summary>
        /// <param name="unigridGuid"></param>
        public void EraseVideo(string unigridGuid)
        {
            //DxCameraRenderer.Instance.EraseUnigrid(unigridGuid);
        }

        /// <summary>
        /// Camera와 Unigrid 함께 지우기
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="camerGuid"></param>
        public void EraseCameraVideo(string unigridGuid, string camerGuid)
        {
            //DxCameraRenderer.Instance.EraseVideoAndUnigrid(unigridGuid, camerGuid);
        }

        /// <summary>
        /// Camera 위치 이동
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="d"></param>
        /// <param name="d1"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void MoveCameraVideo(string unigridGuid, string cameraId, double d, double d1, double width, double height, int? zIndex, bool b)
        {
            //DxCameraRenderer.Instance.MoveUniGrid(0, new Guid(unigridGuid), d, d1, width, height);
        }

        /// <summary>
        /// Camera Video 생성
        /// </summary>
        /// <param name="cameraID"></param>
        /// <returns></returns>
        public string CreateCameraVideo(string cameraID, bool isNewCamera, bool b)
        {/*
            var cameraDataList = ConfigData.Instance.CameraInformations.Where(data => data.General.Id == cameraID).ToList();

            if (!cameraDataList.Any())
                return string.Empty;

            var cameraData = cameraDataList[0] as CameraInformation;

            string uniGrid = DxCameraRenderer.Instance.CreateUnigrid(0, ScissorType.CameraGrid, new Rect()).ToString();

            if (showNameInCamera) DxCameraRenderer.Instance.SetTitle(uniGrid.ToString(), cameraData.General.Name, 0xFFFFFFFF, constset.DRAWTEXT_FORMATS.DT_VCENTER, 0xFF000000, constset.CellTitleHeight);

            if (isNewCamera) this.viewModel.CreateCameraControlVideo(cameraData);

            //Unigrid와 video 연동
            DxCameraRenderer.Instance.PushVideoIntoUnigrid(new Guid(uniGrid), new Guid(cameraData.General.Id));

            //Camera Name 변경
            DxCameraRenderer.Instance.SetCameraTitle(new Guid(uniGrid), cameraData.General.Name);

            return uniGrid;
          * */

            return Guid.NewGuid().ToString();
        }

        public void MouseMoveCameraVideo(string unigridGuid)
        {
            //todo : VW에서만 사용
        }

        public void MouseOutCameraVideo(string unigridGuid)
        {
            //todo : VW에서만 사용
        }

        /// <summary>
        /// Map List 를 받아 온다
        /// Linked Map Setting 중 사용
        /// </summary>
        /// <returns></returns>
        public List<MapSettingDataInfo> GetMapList()
        {
            //return ConfigData.Instance.MapDataList.ToList();
            return null;
        }

        /// <summary>
        /// LinkZone 클릭 시 이동하기 위해 Map 정보를 받아온다.
        /// </summary>
        /// <param name="mapGuid"></param>
        /// <returns>MapSettingInfoData</returns>
        public MapSettingDataInfo GetMapSetting(string mapGuid)
        {
            //todo : VW에서만 사용
            return null;
        }

        public MapSettingDataInfo GetMapSettingByName(string mapName)
        {
            //todo : VW에서만 사용
            return null;
        }

        /// <summary>
        /// Panomorph 상태를 받아온다
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <returns></returns>
        public bool GetPanomorphState(string cameraGuid)
        {
            return false;
        }

        /// <summary>
        /// Panomorph 모드를 저장한다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        public void SetPanomorphControlMode(string unigridGuid)
        {
            
        }

        /// <summary>
        /// Panomorph 타입을 저장한다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <returns></returns>
        public string GetPanomorphViewType(string unigridGuid)
        {
            return string.Empty;
        }

        public SplunkBasicInformationData SetMapSplunkData(SplunkBasicInformationData splunkBasicInformationData)
        {
            /*
            var selectedSplunkData =
                ConfigData.Instance.SplunkInformationDatas.FirstOrDefault(item => item.DataId == splunkBasicInformationData.SplunkDataInformationID);

            if (selectedSplunkData == null) return null;

            splunkBasicInformationData.DataExpressType = selectedSplunkData.Type.MainType.ToString();
            splunkBasicInformationData.IP = selectedSplunkData.Ip;
            splunkBasicInformationData.Port = selectedSplunkData.Port;
            splunkBasicInformationData.App = selectedSplunkData.App;
            splunkBasicInformationData.Name = selectedSplunkData.Name;
            splunkBasicInformationData.UserId = selectedSplunkData.UserId;
            splunkBasicInformationData.Password = selectedSplunkData.Password;
            splunkBasicInformationData.SplunkInfoDataSearchType = selectedSplunkData.SearchType.ToString();

            splunkBasicInformationData.SplunkInfoDataChartSubType = DataExpressType.GetSubTypeString(selectedSplunkData.Type);
            splunkBasicInformationData.SplunkInfoDataChartTheme = selectedSplunkData.ChartSetting.Theme.ToString();
            splunkBasicInformationData.SplunkInfoDataChartPalette = selectedSplunkData.ChartSetting.Palette.ToString();
            splunkBasicInformationData.BackgroundColor = selectedSplunkData.ChartSetting.BackgroundColor;
            splunkBasicInformationData.BorderColor = selectedSplunkData.ChartSetting.BorderColor;
            splunkBasicInformationData.FontColor = selectedSplunkData.ChartSetting.FontColor;
            splunkBasicInformationData.Legend = selectedSplunkData.ChartSetting.Legend;
            splunkBasicInformationData.LineMarker = selectedSplunkData.ChartSetting.LineMarker;
            splunkBasicInformationData.XAxisTitle = selectedSplunkData.ChartSetting.XAxisTitle;
            splunkBasicInformationData.YAxisTitle = selectedSplunkData.ChartSetting.YAxisTitle;
            splunkBasicInformationData.YAxisRangeMaximum = selectedSplunkData.ChartSetting.YAxisRangeMaximum;
            splunkBasicInformationData.YAxisRangeMinimum = selectedSplunkData.ChartSetting.YAxisRangeMinimum;

            splunkBasicInformationData.HeaderBackgroundColor = selectedSplunkData.TableSetting.HeaderBackgroundColor;
            splunkBasicInformationData.HeaderFontColor = selectedSplunkData.TableSetting.HeaderFontColor;
            splunkBasicInformationData.RowBackgroundColor = selectedSplunkData.TableSetting.RowBackgroundColor;
            splunkBasicInformationData.AlternatingRowColor = selectedSplunkData.TableSetting.AlternatingRowColor;
            splunkBasicInformationData.AlternatingRowFontColor = selectedSplunkData.TableSetting.AlternatingRowFontColor;
            splunkBasicInformationData.GridLineVisible = selectedSplunkData.TableSetting.GridLineVisible;
            splunkBasicInformationData.HorizontalGridLineColor = selectedSplunkData.TableSetting.HorizontalGridLineColor;
            splunkBasicInformationData.VerticalGridLineColor = selectedSplunkData.TableSetting.VerticalGridLineColor;

            splunkBasicInformationData.SplunkDataInformationID = selectedSplunkData.DataId;

            return splunkBasicInformationData;
             * */

            return null;
        }

        public string GetCameraName(string cameraGuid)
        {
            return cameraGuid; // gmsim 임시
        }

        #region Playback Interface API

        public bool GetRecordingState(string cameraGuid)
        {
            // Not use in CU
            return true;
        }

        public void StartPlaybackMode(string unigridGuid, string cameraId, DateTime seekTime, Rect displayRect)
        {
            // Not use in CU
        }

        public void EndPlaybackMode(string unigridGuid)
        {
            // Not use in CU
        }

        public void PlayPlayback(string unigridGuid, double speed)
        {
            // Not use in CU
        }

        public void RewindPlayback(string unigridGuid, double speed)
        {
            // Not use in CU
        }

        public void PausePlayback(string unigridGuid)
        {
            // Not use in CU
        }

        public void SeekPlayback(string unigridGuid, DateTime seekTime)
        {
            // Not use in CU
        }

        #endregion //Playback Interface API

        #region RDS Interface API

        public bool GeRdsState(string cameraGuid)
        {
            // Not use in CU
            return true;
        }

        public void StartRdsControl(string cameraGuid)
        {
            // Not use in CU
        }

        public void EndRdsControl(string cameraGuid)
        {
            // Not use in CU
        }

        public void SendRdsControlData(string cameraGuid, byte[] data)
        {
            // Not use in CU
        }

        #endregion // RDS Interface API

        #endregion ArcGIS Interface APIS

        #region ArcGIS Setting Interface APIS

        public List<SplunkBasicInformationData> GetSplunkInformationDataList()
        {
            /*
            var mapSplunkInformationDatas = new List<SplunkBasicInformationData>();

            foreach (var splunkInformationData in DataHandlers.ConfigData.Instance.SplunkInformationDatas)
            {
                mapSplunkInformationDatas.Add(
                    new SplunkBasicInformationData()
                    {
                        App = splunkInformationData.App,
                        IP = splunkInformationData.Ip,
                        Password = splunkInformationData.Password,
                        Port = splunkInformationData.Port,
                        UserId = splunkInformationData.UserId,
                        Name = splunkInformationData.Name,
                        SplunkDataInformationID = splunkInformationData.DataId
                    }
                    );
            }

            return mapSplunkInformationDatas;
             * */

            return null;
        }

        public int GetCellZIndex()
        {
            return 0;
        }

        public bool GetPanomortphState(string cameraGuid)
        {
            return true;
        }
        public void AlertBroadCast(string cameraGuid, DateTime currentTime)
        {
            
        }

        /// <summary>
        /// Panomorph Control 의 Default view 로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewType"></param>
        /// <param name="popupCameraGuid">Popup카메라가 있을 경우 이 값이 세팅이되어 들어간다.</param>
        public void MovePanomorphCameraDefaultView(string objectId, string viewType)
        {
           
        }

        /// <summary>
        /// Panomorph Control 의 MouseDrag로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="panomorphMovedPoint"></param>
        /// <param name="isLeftButtonDown"></param>
        /// <param name="popupCameraGuid">Popup카메라가 있을 경우 이 값이 세팅이되어 들어간다.</param>
        public void MovePanomorphCameraMouseControl(string objectId, int viewNumber, Point panomorphMovedPoint, bool isLeftButtonDown)
        {
           
        }

        /// <summary>
        /// Panomorph Control 의 Point로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="panomorphMovedPoint"></param>
        /// <param name="popupCameraGuid">Popup카메라가 있을 경우 이 값이 세팅이되어 들어간다.</param>
        public void MovePanomorphCameraPoint(string objectId, int viewNumber, Point panomorphMovedPoint)
        {
            
        }

        /// <summary>
        /// Panomorph Control 의 Point로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="selectionArea"></param>
        /// <param name="popupCameraGuid">Popup카메라가 있을 경우 이 값이 세팅이되어 들어간다.</param>
        public void MovePanomorphCameraRect(string objectId, int viewNumber, Rect selectionArea)
        {
          
        }

        /// <summary>
        /// PanomorphView Type 변경
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewType"></param>
        /// <param name="popupCameraGuid">Popup카메라가 있을 경우 이 값이 세팅이되어 들어간다.</param>
        public void ChangePanomorphViewType(string objectId, string viewType)
        {
          
        }

        public void OpenPanomorphPopupCamera()
        {
           
        }

        public void StartPlaybackMode(string unigridGuid, string cameraId, DateTime seekTime, Rect displayRect, int? zIndex)
        {
            
        }

        public void SeekDataPlayback(DateTime seekTime)
        {
            
        }

        public void PlayDataPlayback(DateTime seekTime)
        {
            
        }

        public void PauseDataPlayback()
        {
            
        }

        public void SetZIndexCameraVideo(string unigridGuid, int zindex)
        {
            
        }

        public bool ShowMessagePopup(string alertMessage, bool onlyConfirm = false)
        {
            return false;
        }

        public string GetUserID()
        {
            return string.Empty;
        }

        public void OpenLinkzoneViewControl(string url)
        {
            
        }

        #endregion

        private void xNameMakeText_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.IsMakingText = true;
        }

        private void xButtonMakeLinkZone_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.IsMakingLinkZone = true;
        }

        private void xButtonGetSavedSearchName_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.GetSavedSearchNameListInExtent();
        }

        private void xButtonMakeLine_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.IsMakingLine = true;
        }

        private void xButtonMakeImageLinkZone_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.IsMakingImageLinkZone = true;
        }

        private void xButtonMakeImage_Click_1(object sender, RoutedEventArgs e)
        {
            this.arcGisClientViewer.IsMakingImage = true;
        }

        private void XButtonLocation_OnClick(object sender, RoutedEventArgs e)
        {
           this.arcGisClientViewer.IsMakingLocation = true;
        }
    }
}
