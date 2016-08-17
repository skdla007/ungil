using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public class CameraVideoPropertyControlViewModel : CameraPropertyControlBaseViewModel
    {
        private int mapLevel;

        public bool IsInitializeValues;

        public int MapLevel
        {
            get { return this.mapLevel; }
            set
            {
                if (this.mapLevel == value)
                    return;

                this.mapLevel = value;
                this.OnPropertyChanged("MapLevel");
            }
        }

        private double mapCurrentResoultion;
        public double MapCurrentResoultion
        {
            set
            {
                if (this.mapCurrentResoultion == value)
                    return;

                this.mapCurrentResoultion = value;
                this.OnPropertyChanged("MapCurrentResoultion");
            }
            get
            {

                return this.mapCurrentResoultion;
            }
        }

        private string hiddenMinLevel;
        public string HiddenMinLevel
        {
            get
            {
                if (this.dataInfo.HiddenMinLevel < 0)
                    return "-";
                else
                    return this.dataInfo.HiddenMinLevel.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int i;

                if (int.TryParse(value, out i))
                {
                    this.dataInfo.HiddenMinLevel = i;
                }
                else
                {
                    this.dataInfo.HiddenMinLevel = (int)this.dataInfo.DefaultHiddenMinLevel;
                }

                OnPropertyChanged("HiddenMinLevel");
            }
        }

        private string hiddenMaxLevel;
        public string HiddenMaxLevel
        {
            get
            {
                if (this.dataInfo.HiddenMaxLevel < 0)
                    return "-";
                else
                    return this.dataInfo.HiddenMaxLevel.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int i;

                if(int.TryParse(value, out i))
                {
                    this.dataInfo.HiddenMaxLevel = i;
                }
                else
                {
                    this.dataInfo.HiddenMaxLevel = (int)this.dataInfo.DefaultHiddenMaxLevel;
                }

                OnPropertyChanged("HiddenMaxLevel");
            }
        }

        private MapCameraVideoObjectDataInfo dataInfo;

        public MapCameraVideoObjectDataInfo DataInfo
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

        #region Command

        private RelayCommand buttonSetScaleMaxLevelCommand;
        public ICommand ButtonSetScaleMaxLevelCommand
        {
            get
            {
                return this.buttonSetScaleMaxLevelCommand ??
                       (this.buttonSetScaleMaxLevelCommand =
                        new RelayCommand(param => this.SetScaleMaxLevel(), null));
            }
        }

        private void SetScaleMaxLevel()
        {
            this.dataInfo.ScaleMaxLevel = this.MapLevel;
            this.dataInfo.ScaleMaxResoultion = this.MapCurrentResoultion;
        }

        private RelayCommand buttonSetHiddenMaxLevelCommand;
        public ICommand ButtonSetHiddenMaxLevelCommand
        {
            get
            {
                return this.buttonSetHiddenMaxLevelCommand ??
                       (this.buttonSetHiddenMaxLevelCommand =
                        new RelayCommand(param => this.SetHiddenMaxLevel(), null));
            }
        }

        private void SetHiddenMaxLevel()
        {
            this.HiddenMaxLevel = ((int)this.MapLevel).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// HiddenMaxLevel을 초기화한다.
        /// </summary>
        private RelayCommand buttonResetHiddenMaxLevelCommand;
        public ICommand ButtonResetHiddenMaxLevelCommand
        {
            get
            {
                return this.buttonResetHiddenMaxLevelCommand ??
                       (this.buttonResetHiddenMaxLevelCommand =
                        new RelayCommand(param => this.ResetHiddenMaxLevel(), null));
            }
        }

        private void ResetHiddenMaxLevel()
        {
            this.dataInfo.HiddenMaxLevel = -1;
            this.HiddenMaxLevel = "-1";
        }

        private RelayCommand buttonSetHiddenMinLevelCommand;
        public ICommand ButtonSetHiddenMinLevelCommand
        {
            get
            {
                return this.buttonSetHiddenMinLevelCommand ??
                       (this.buttonSetHiddenMinLevelCommand =
                        new RelayCommand(param => this.SetHiddenMinLevel(), null));
            }
        }

        private void SetHiddenMinLevel()
        {
            this.HiddenMinLevel = ((int)this.MapLevel).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// HiddenMinLevel을 초기화 한다.
        /// </summary>
        private RelayCommand buttonResetHiddenMinLevelCommand;
        public ICommand ButtonResetHiddenMinLevelCommand
        {
            get
            {
                return this.buttonResetHiddenMinLevelCommand ??
                       (this.buttonResetHiddenMinLevelCommand =
                        new RelayCommand(param => this.ResetHiddenMinLevel(), null));
            }
        }

        private void ResetHiddenMinLevel()
        {
            this.dataInfo.HiddenMinLevel = -1;
            this.HiddenMinLevel = "-1";
        }
        

        private RelayCommand checkedLockedSizeCommand;
        public ICommand CheckedLockedSizeCommand
        {
            get
            {
                return this.checkedLockedSizeCommand ??
                       (this.checkedLockedSizeCommand =
                        new RelayCommand(param => this.SaveLockedResoultion(), null));
            }
        }

        public void SaveLockedResoultion()
        {
            
        }

        #endregion
    }
}
