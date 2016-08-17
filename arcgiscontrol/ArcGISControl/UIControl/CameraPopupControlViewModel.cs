using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArcGISControl.Helper;
using DataChangedNotify;

namespace ArcGISControl.UIControl
{
    public class CameraPopupControlViewModel : BaseModel
    {
        #region Fields

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            set
            {
                this.selectedIndex = value;
                OnPropertyChanged("SelectedIndex");

                if (this.selectedIndex < 0 || this.presetList == null) return;

                var presetIndex = this.presetList[this.selectedIndex];
                this.RaisePresetListSelectionEvent(presetIndex);
            }
        }

        private bool usePreset = true;
        public bool UsePreset
        {
            get { return this.usePreset; }
            set
            {
                this.usePreset = value;
                OnPropertyChanged("UsePreset");
            }
        }

        private ObservableCollection<string> presetList;
        public ObservableCollection<string> PresetList
        {
            get { return this.presetList; }
            set
            {
                this.SelectedIndex = -1;
                this.presetList = value;
                OnPropertyChanged("PresetList");

                this.UsePreset = this.presetList.Count > 0;
            }
        }

        private string cameraName;
        public string CameraName
        {
            get { return this.cameraName; }
            set
            {
                this.cameraName = value;
                OnPropertyChanged("CameraName");
            }
        }

        public class PresetListSelectionChangedEventArgs : EventArgs
        {
            public string PresetIndex { get; set; }

            public PresetListSelectionChangedEventArgs(string presetIndex)
            {
                this.PresetIndex = presetIndex;
            }
        }

        public EventHandler<PresetListSelectionChangedEventArgs> ePresetListSelectionChanged;

        private void RaisePresetListSelectionEvent(string presetIndex)
        {
            var handler = this.ePresetListSelectionChanged;
            if(handler != null)
            {
                handler(this, new PresetListSelectionChangedEventArgs(presetIndex));
            }
        }

        public EventHandler eCloseButtonClicked;

        private void RaiseCloseButtonClickedEvent()
        {
            var handler = this.eCloseButtonClicked;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        #endregion //Fields

        #region Construction

        public CameraPopupControlViewModel()
        {
        }

        #endregion //Construction

        #region Method

        #endregion //Method

        #region Command

        private RelayCommand closeCameraPopupControlCommand;
        public ICommand CloseCameraPopupControlCommand
        {
            get
            {
                return this.closeCameraPopupControlCommand ??
                       (this.closeCameraPopupControlCommand = new RelayCommand(param => this.RaiseCloseButtonClickedEvent(), null));
            }
        }

        #endregion //Command
    }
}
