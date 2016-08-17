using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.UIControl
{
    public class WorkStationContextControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        public event EventHandler<LinkedMapEventArgs> onGoLinkedMap;

        public event EventHandler<WorkStationEventArgs> onShowSearchViewControl;

        private MapWorkStationObjectDataInfo dataInfo;
        public MapWorkStationObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set 
            {
                if (this.dataInfo == value)
                    return;
                this.dataInfo = value;
                this.OnPropertyChanged("DataInfo");
            }
        }

        private void RaiseGoLinkedMap(string linkedMapGuid)
        {
            var e = this.onGoLinkedMap;
            if (e != null)
                e(this, new LinkedMapEventArgs
                {
                    LinkedMapID = linkedMapGuid
                });
        }

        private void RaiseShowSearchViewControl(MapWorkStationObjectDataInfo dataInfo)
        {
            var e = this.onShowSearchViewControl;
            if (e != null)
                e(this, new WorkStationEventArgs
                {
                    DataInfo = dataInfo
                });

        }

        private RelayCommand clickSoftWareViewCommand;
        public ICommand ClickSoftWareViewCommand
        {
            get
            {
                return this.clickSoftWareViewCommand ??
                       (this.clickSoftWareViewCommand =
                        new RelayCommand(param => this.ClickSoftWareView(), null));
            }
        }

        private void ClickSoftWareView()
        {
            if (!string.IsNullOrEmpty(this.dataInfo.SoftwareViewLinkedMapGuid) && this.dataInfo.SoftwareViewLinkedMapGuid.ToLower() != "none")
                this.RaiseGoLinkedMap(this.dataInfo.SoftwareViewLinkedMapGuid);
        }

        private RelayCommand clickHardWareViewCommand;
        public ICommand ClickHardWareViewCommand
        {
            get
            {
                return this.clickHardWareViewCommand ??
                       (this.clickHardWareViewCommand =
                        new RelayCommand(param => this.ClickHardWareView(), null));
            }
        }

        private void ClickHardWareView()
        {
            if (!string.IsNullOrEmpty(this.dataInfo.HardwareViewLinkedMapGuid) && this.dataInfo.HardwareViewLinkedMapGuid.ToLower() != "none")
                this.RaiseGoLinkedMap(this.dataInfo.HardwareViewLinkedMapGuid);
        }

        private RelayCommand clickNetWorkViewCommand;
        public ICommand ClickNetWorkViewCommand
        {
            get
            {
                return this.clickNetWorkViewCommand ??
                       (this.clickNetWorkViewCommand =
                        new RelayCommand(param => this.ClickNetWorkView(), null));
            }
        }

        private void ClickNetWorkView()
        {
            if (!string.IsNullOrEmpty(this.dataInfo.NetworkViewLinkedMapGuid) && this.dataInfo.NetworkViewLinkedMapGuid.ToLower() != "none")
                this.RaiseGoLinkedMap(this.dataInfo.NetworkViewLinkedMapGuid);
        }

        private RelayCommand clickSearchViewCommand;
        public ICommand ClickSearchViewCommand
        {
            get
            {
                return this.clickSearchViewCommand ??
                       (this.clickSearchViewCommand =
                        new RelayCommand(param => this.ClickSearchView(), null));
            }
        }

        private void ClickSearchView()
        {
            if (!string.IsNullOrEmpty(this.dataInfo.SearchViewUrl))
                this.RaiseShowSearchViewControl(this.dataInfo);
        }
    }
}
