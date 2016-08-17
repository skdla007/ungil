using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArcGISControl.PropertyControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;
using Innotive.SplunkManager.SplunkManager;

namespace ArcGISControl.PropertyControl
{

    internal class PropertyManager
    {
        private CommonPropertyWindow window;

        private CommonPropertyWindowViewModel propertyWindowViewModel;

        private readonly MapDataServiceHandler dataServiceHandler;

        public event ApplyLinkZoneSplunkData onApplyLinkZoneSplunkData;

        public event ApplySplunkArgumentData onApplySplunkArgumentData;

        public bool IsShowWindow
        {
            get
            {
                return this.propertyWindowViewModel.CameraIconSelected || this.propertyWindowViewModel.CameraVideoSelected ||
                   this.propertyWindowViewModel.CameraViewZoneSelected || this.propertyWindowViewModel.LinkZoneSelected ||
                   this.propertyWindowViewModel.PlaceSelected;
            }
        }

        public double MapCurrentZoomLevel
        {
            set { this.propertyWindowViewModel.MapLevel =  value; }
        }

        public double MapResolution
        {
            set
            {
                this.propertyWindowViewModel.MapResolution = value;
            }
        }

        internal PropertyManager(Window propertyWindowOwner, MapDataServiceHandler dataServiceHandler)
        {
            this.dataServiceHandler = dataServiceHandler;

            this.propertyWindowViewModel = new CommonPropertyWindowViewModel();

            window = new CommonPropertyWindow(propertyWindowOwner)
                {
                    DataContext = this.propertyWindowViewModel
                };

            window.Show();
            this.DeselectAll();
        }

        internal void ShowMapObjectGraphicProperty(List<MapObjectPropertied> ShowMapObjectPropertiesList)
        {
            propertyWindowViewModel.MapObjectPropertiedVisible = ShowMapObjectPropertiesList.ToArray();
        }

        internal void SelectCameraIcon(string cameraName, MapCameraIconObjectDataInfo dataInfo)
        {
            var viewModel = new CameraIconPropertyControlViewModel()
            {
                CameraName = cameraName,
                DataInfo = dataInfo
            };

            this.propertyWindowViewModel.CameraIconPropertyControlViewModel = viewModel;
        }

        internal void SelectCameraVideo(string cameraName, MapCameraVideoObjectDataInfo dataInfo)
        {
            var viewModel = new CameraVideoPropertyControlViewModel()
            {
                CameraName = cameraName,
                DataInfo = dataInfo,
                IsInitializeValues =  false,
            };
            
            this.propertyWindowViewModel.CameraVideoPropertyControlViewModel = viewModel;
            this.propertyWindowViewModel.CameraVideoPropertyControlViewModel.IsInitializeValues = true;
        }

        internal void SelectCameraViewZone(string cameraName, MapCameraPresetObjectDataInfo dataInfo, List<string> presetList)
        {
            var viewModel = new CameraPresetPropertyControlViewModel()
            {
                CameraName = cameraName,
                DataInfo = dataInfo,
                PresetList = presetList
            };
            this.propertyWindowViewModel.CameraPresetPropertyControlViewModel = viewModel;
        }

        internal void SelectSplunk(MapSplunkObjectDataInfo dataInfo, bool allChart)
        {
            var viewModel = new SplunkPropertyControlViewModel()
            {
                DataInfo = dataInfo,
                AllChart = allChart,
                IsInitializeValues = false,
            };

            viewModel.onApplySplunkArgumentData += SplunkPropertyControlViewModelOnApplySplunkArgumentData;
            
            this.propertyWindowViewModel.SplunkPropertyControlViewModel = viewModel;
            this.propertyWindowViewModel.SplunkPropertyControlViewModel.IsInitializeValues = true;
        }

        internal void SelectLinkZone(MapLinkZoneObjectDataInfo objectDataInfo, List<MapSettingDataInfo> mapSettingInfoDatas, List<SplunkBasicInformationData> splunkInformationDatas, bool useSplunk, bool isSingleSetting)
        {  
            var viewModel = new LinkZonePropertyControlViewModel(this.dataServiceHandler)
            {
                DataInfo = objectDataInfo,
                MapSettingInfoDatas = mapSettingInfoDatas,
                SplunkInformationDatas = splunkInformationDatas,
                UseSplunk = useSplunk,
                IsSingleSetting =  isSingleSetting
            };

            if (objectDataInfo.LinkedMapGuid != null && mapSettingInfoDatas != null)
            {
                viewModel.SelectedLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == objectDataInfo.LinkedMapGuid);
            }
            
            viewModel.onApplyLinkZoneSplunkData += LinkZonePropertyControlViewModelOnApplyLinkZoneSplunkData;

            this.propertyWindowViewModel.LinkZonePropertyControlViewModel = viewModel;
        }


        internal void SelectWorkStation(MapWorkStationObjectDataInfo dataInfo, List<MapSettingDataInfo> mapSettingInfoDatas, List<SplunkBasicInformationData> splunkInformationDatas)
        {
            var viewModel = new WorkStationPropertyControlViewModel()
            {
                DataInfo = dataInfo,
                MapSettingInfoDatas = mapSettingInfoDatas,
                SplunkInformationDatas = splunkInformationDatas
            };

            if (dataInfo.LinkedMapGuid != null)
            {
                viewModel.SelectedLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == dataInfo.LinkedMapGuid);
            }

            if (dataInfo.NetworkViewLinkedMapGuid != null)
            {
                viewModel.SelectedNetworkViewLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == dataInfo.NetworkViewLinkedMapGuid);
            }

            if (dataInfo.HardwareViewLinkedMapGuid != null)
            {
                viewModel.SelectedHardWareViewLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == dataInfo.HardwareViewLinkedMapGuid);
            }

            if (dataInfo.SoftwareViewLinkedMapGuid != null)
            {
                viewModel.SelectedSoftwareViewLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == dataInfo.SoftwareViewLinkedMapGuid);
            }
            
            this.propertyWindowViewModel.WorkStationPropertyControlViewModel = viewModel;
        }

        internal void SelectText(MapTextObjectDataInfo dataInfo)
        {
            var viewModel = new TextPropertyControlViewModel()
            {  
                DataInfo = dataInfo,
            };
            this.propertyWindowViewModel.TextPropertyControlViewModel = viewModel;
            this.propertyWindowViewModel.TextPropertyControlViewModel.IsInitializeValues = true;
        }

        internal void SelectPlace(MapLocationObjectDataInfo dataInfo, bool isSingleSetting)
        {
            var viewModel = new PlacePropertyControlViewModel()
            {
                DataInfo = dataInfo,
                IsSingleSetting = isSingleSetting
            };
            this.propertyWindowViewModel.PlacePropertyControlViewModel = viewModel;
        }

        internal void SelectLine(MapLineObjectDataInfo dataInfo, bool isSingleSetting)
        {
            var viewModel = new LinePropertyControlViewModel()
            {
                DataInfo = dataInfo,
                IsSingleSetting = isSingleSetting
            };

            this.propertyWindowViewModel.LinePropertyControlViewModel = viewModel;
        }

        internal void SelectImage(MapImageObjectDataInfo dataInfo, bool isSingleSetting)
        {
            var viewModel = new ImagePropertyControlViewModel()
            {
                DataInfo = dataInfo,
                IsSingleSetting = isSingleSetting
            };

            this.propertyWindowViewModel.ImagePropertyControlViewModel = viewModel;
        }

        internal void SelectUniversalObject(MapUniversalObjectDataInfo dataInfo, List<MapSettingDataInfo> mapSettingInfoDatas, bool isSingleSetting)
        {
            var viewModel = new UniversalObjectPropertyControlViewModel(this.dataServiceHandler)
            {
                DataInfo = dataInfo,
                IsSingleSetting = isSingleSetting,
                MapSettingInfoDatas = mapSettingInfoDatas
            };

            if (dataInfo.LinkedMapGuid != null && mapSettingInfoDatas != null)
            {
                viewModel.SelectedLinkedMapDataIndex = mapSettingInfoDatas.FindIndex(0, mapSettingInfoDatas.Count, a => a.ID == dataInfo.LinkedMapGuid);
            }

            this.propertyWindowViewModel.UniversalObjectPropertyControlViewModel = viewModel;
        }

        internal void DeselectAll()
        {
            if (propertyWindowViewModel.MapObjectPropertiedVisible.Contains(MapObjectPropertied.LinkZone))
            {   
                this.propertyWindowViewModel.LinkZonePropertyControlViewModel.onApplyLinkZoneSplunkData -= LinkZonePropertyControlViewModelOnApplyLinkZoneSplunkData;
            }

            if (propertyWindowViewModel.MapObjectPropertiedVisible.Contains(MapObjectPropertied.Splunk))
            {
                this.propertyWindowViewModel.SplunkPropertyControlViewModel.onApplySplunkArgumentData -= SplunkPropertyControlViewModelOnApplySplunkArgumentData;
            }

            MapObjectPropertied[] mapObjectPropertied = { MapObjectPropertied.None };
            propertyWindowViewModel.MapObjectPropertiedVisible = mapObjectPropertied;
        }

        internal void CloseWindow()
        {
            this.window.Close();
        }

        #region Events

        /// <summary>
        /// LinkZone 에서 Apply 버튼 클릭시 호출
        /// </summary>
        /// <param name="data"></param>
        /// <param name="setspltype"></param>
        private void LinkZonePropertyControlViewModelOnApplyLinkZoneSplunkData(MapLinkZoneObjectDataInfo data, SETSPLTYPE setspltype)
        {
            if (this.onApplyLinkZoneSplunkData != null) this.onApplyLinkZoneSplunkData(data, setspltype);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SplunkPropertyControlViewModelOnApplySplunkArgumentData(MapSplunkObjectDataInfo data)
        {
            if (this.onApplySplunkArgumentData != null) this.onApplySplunkArgumentData(data);
        }


        #endregion Events
    }
}
