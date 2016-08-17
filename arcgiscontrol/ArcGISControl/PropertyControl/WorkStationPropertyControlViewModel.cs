using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ArcGISControls.CommonData;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    
    public class WorkStationPropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private MapWorkStationObjectDataInfo dataInfo;

        public MapWorkStationObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if(this.dataInfo == value)
                    return;

                this.dataInfo = value;

                this.OnPropertyChanged("DataInfo");

                this.SettingSplunkArguments(dataInfo.SplunkBasicInformation);
            }
        }

        private bool isEnabledLinkedMap = false;
        public bool IsEnabledLinkedMap
        {
            get { return this.isEnabledLinkedMap; }
            set
            {
                this.isEnabledLinkedMap = value;
                OnPropertyChanged("IsEnabledLinkedMap");
            }
        }

        private bool isEnalbedSplunkData = false;
        public bool IsEnabledSplunkData
        {
            get { return this.isEnalbedSplunkData; }
            set
            {
                this.isEnalbedSplunkData = value;
                OnPropertyChanged("IsEnabledSplunkData");
            }
        }

        public Color FillColor
        {
            get { return this.dataInfo.FillColor; }
            set
            {
                if (this.dataInfo.FillColor == value)
                    return;
                this.dataInfo.FillColorString = value.ToString();
                this.OnPropertyChanged("FillColor");
            }
        }

        public Color OutlineColor
        {
            get { return this.dataInfo.BorderColor; }
            set
            {
                if (this.dataInfo.BorderColor == value)
                    return;
                this.dataInfo.BorderColorString = value.ToString();
                this.OnPropertyChanged("OutlineColor");
            }
        }


        private List<MapSettingDataInfo> mapSettingInfoDatas;
        public List<MapSettingDataInfo> MapSettingInfoDatas
        {
            get { return mapSettingInfoDatas; }
            set
            {
                mapSettingInfoDatas = value;
                OnPropertyChanged("MapSettingInfoDatas");

                this.IsEnabledLinkedMap = true;

                if (this.dataInfo != null && string.IsNullOrEmpty(this.dataInfo.LinkedMapGuid))
                {
                    this.SelectedLinkedMapDataIndex = 0;
                }

                if (this.mapSettingInfoDatas == null)
                {
                    this.IsEnabledLinkedMap = false;
                }
            }
        }

        private int selectedLinkedMapDataIndex;
        public int SelectedLinkedMapDataIndex
        {
            get { return this.selectedLinkedMapDataIndex; }
            set
            {
                this.selectedLinkedMapDataIndex = value;
                OnPropertyChanged("SelectedLinkedMapDataIndex");

                if (this.MapSettingInfoDatas != null &&
                    this.MapSettingInfoDatas.Count > this.selectedLinkedMapDataIndex &&
                    this.selectedLinkedMapDataIndex >= 0)
                {
                    var data = this.MapSettingInfoDatas[this.selectedLinkedMapDataIndex];
                    if (data.Name.ToLower() != "none") this.dataInfo.LinkedMapGuid = data == null ? null : data.ID;
                }
            }
        }

        private int selectedNetworkViewLinkedMapDataIndex;
        public int SelectedNetworkViewLinkedMapDataIndex
        {
            get { return this.selectedNetworkViewLinkedMapDataIndex; }
            set
            {
                this.selectedNetworkViewLinkedMapDataIndex = value;
                OnPropertyChanged("SelectedNetworkViewLinkedMapDataIndex");

                if (this.MapSettingInfoDatas != null &&
                    this.MapSettingInfoDatas.Count > this.selectedNetworkViewLinkedMapDataIndex &&
                    this.selectedNetworkViewLinkedMapDataIndex >= 0)
                {
                    var data = this.MapSettingInfoDatas[this.selectedNetworkViewLinkedMapDataIndex];
                    if (data.Name.Equals("none", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        this.dataInfo.NetworkViewLinkedMapGuid = null;
                    }
                    else
                    {
                        this.dataInfo.NetworkViewLinkedMapGuid = data == null ? null : data.ID;                        
                    }
                }
            }
        }

        private int selectedSoftwareViewLinkedMapDataIndex;
        public int SelectedSoftwareViewLinkedMapDataIndex
        {
            get { return this.selectedSoftwareViewLinkedMapDataIndex; }
            set
            {
                this.selectedSoftwareViewLinkedMapDataIndex = value;
                OnPropertyChanged("SelectedSoftwareViewLinkedMapDataIndex");

                if (this.MapSettingInfoDatas != null &&
                    this.MapSettingInfoDatas.Count > this.selectedSoftwareViewLinkedMapDataIndex &&
                    this.selectedSoftwareViewLinkedMapDataIndex >= 0)
                {
                    var data = this.MapSettingInfoDatas[this.selectedSoftwareViewLinkedMapDataIndex];
                    if (data.Name.Equals("none", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        this.dataInfo.SoftwareViewLinkedMapGuid = null;  
                    }
                    else
                    {
                        this.dataInfo.SoftwareViewLinkedMapGuid = data == null ? null : data.ID;                        
                    }
                }

            }
        }

        private int selectedHardWareViewLinkedMapDataIndex;
        public int SelectedHardWareViewLinkedMapDataIndex
        {
            get { return this.selectedHardWareViewLinkedMapDataIndex; }
            set
            {
                this.selectedHardWareViewLinkedMapDataIndex = value;
                OnPropertyChanged("SelectedHardWareViewLinkedMapDataIndex");

                if (this.MapSettingInfoDatas != null &&
                    this.MapSettingInfoDatas.Count > this.selectedHardWareViewLinkedMapDataIndex &&
                    this.selectedHardWareViewLinkedMapDataIndex >= 0)
                {
                    var data = this.MapSettingInfoDatas[this.selectedHardWareViewLinkedMapDataIndex];
                    if (data.Name.Equals("none", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        this.dataInfo.HardwareViewLinkedMapGuid = null;
                    }
                    else
                    {
                        this.dataInfo.HardwareViewLinkedMapGuid = data == null ? null : data.ID;
                    }
                }
            }
        }

        private ObservableCollection<SplunkArgumentItem> splunkArgumentItems = new ObservableCollection<SplunkArgumentItem>();
        public ObservableCollection<SplunkArgumentItem> SplunkArgumentItems
        {
            get { return this.splunkArgumentItems; }
            set
            {
                this.splunkArgumentItems = value;
                OnPropertyChanged("SplunkArgumentItems");
            }
        }

        private List<SplunkBasicInformationData> splunkInformationDatas;
        public List<SplunkBasicInformationData> SplunkInformationDatas
        {
            get { return this.splunkInformationDatas; }
            set
            {
                splunkInformationDatas = value;
                OnPropertyChanged("SplunkInformationDatas");

                if (this.dataInfo.SplunkBasicInformation.Name != null)
                {
                    var selectedData = this.splunkInformationDatas.FirstOrDefault(item => item.Name == this.dataInfo.SplunkBasicInformation.Name);
                    this.selectedSplunkInformationDataIndex = this.splunkInformationDatas.IndexOf(selectedData);
                }

                if (this.splunkInformationDatas != null && this.splunkInformationDatas.Count > 0)
                {
                    this.IsEnabledSplunkData = true;
                }
                else
                {
                    this.IsEnabledSplunkData = false;
                }
            }
        }

        private int selectedSplunkInformationDataIndex;
        public int SelectedSplunkInformationDataIndex
        {
            get { return this.selectedSplunkInformationDataIndex; }
            set
            {
                this.selectedSplunkInformationDataIndex = value;
                OnPropertyChanged("SelectedSplunkInformationDataIndex");

                if (this.splunkInformationDatas != null &&
                    this.splunkInformationDatas.Count > this.selectedSplunkInformationDataIndex &&
                    this.selectedSplunkInformationDataIndex >= 0)
                {
                    var data = this.splunkInformationDatas[this.selectedSplunkInformationDataIndex];
                    this.SettingSplunkArguments(data);
                }
            }
        }

        #region Methods

        private void SettingSplunkArguments(SplunkBasicInformationData data)
        {
            if (!this.dataInfo.SplunkBasicInformation.IsSameSplunkService(data)) this.dataInfo.SplunkBasicInformation = data;

            this.SplunkArgumentItems.Clear();

            for (int i = 0; i < data.SplArgumentKeys.Count; i++)
            {
                this.SplunkArgumentItems.Add
                    (
                        new SplunkArgumentItem()
                            {
                                SplunkArgumentKey = data.SplArgumentKeys.ElementAt(i),
                                SplunkArgumentValue = data.SplArgumentValues.ElementAt(i)
                            }
                    );
            }
        }

        #endregion //Methods
    }
}
