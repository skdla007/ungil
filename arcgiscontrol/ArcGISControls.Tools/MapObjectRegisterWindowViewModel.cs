using System;
using System.Windows.Input;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.Tools
{
    public class MapObjectRegisterWindowViewModel :DataChangedNotify.BaseModel
    {
        #region Field

        private BaseMapObjectInfoData baseMapObjectData = new BaseMapObjectInfoData();
        public MapBookMarkDataInfo MapBookMarkData
        {
            get { return this.baseMapObjectData as MapBookMarkDataInfo; }
            set
            {
                this.baseMapObjectData = value;
                OnPropertyChanged("MapBookMarkData");
            }
        }

        public MapLocationObjectDataInfo MapLocationObjectData
        {
            get { return this.baseMapObjectData as MapLocationObjectDataInfo; }
            set
            {
                this.baseMapObjectData = value;
                OnPropertyChanged("MapLocationObjectData");
            }
        }

        public event EventHandler<EventArgs> onButtonOkClick;
        public event EventHandler<EventArgs> onButtonCancelClick;

        #endregion //Field

        #region Construction

        #endregion //Construction

        #region Command

        private RelayCommand buttonOkCommand;

        public ICommand ButtonOkCommand
        {
            get
            {
                return this.buttonOkCommand ??
                (this.buttonOkCommand =
                 new RelayCommand(param => this.ExecuteButtonOk(), param => this.CanExcuteButtonOk()));
            }
        }

        public void ExecuteButtonOk()
        {
            if (this.onButtonOkClick != null)
                this.onButtonOkClick(this, null);
        }

        public bool CanExcuteButtonOk()
        {
            return this.baseMapObjectData != null && !string.IsNullOrEmpty(this.baseMapObjectData.Name);
        }

        private RelayCommand buttonCancelCommand;

        public ICommand ButtonCancelCommand
        {
            get
            {  
                return this.buttonCancelCommand ??
                (this.buttonCancelCommand = new RelayCommand(param => this.ExecuteButtonCancel(), null));
            }
        }

        public void ExecuteButtonCancel()
        {
            if (this.onButtonCancelClick != null)
                this.onButtonCancelClick(this, null);
        }

        #endregion //Command
    }
}
