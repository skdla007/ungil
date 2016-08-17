using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGISControl.Helper;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public delegate void ApplySplunkArgumentData(MapSplunkObjectDataInfo data);
    
    public class SplunkPropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region eventArguments

        public event ApplySplunkArgumentData onApplySplunkArgumentData;

        #endregion eventArguments

        #region Field

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
                    this.dataInfo.HiddenMinLevel = -1;
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

                if (int.TryParse(value, out i))
                {
                    this.dataInfo.HiddenMaxLevel = i;
                }
                else
                {
                    this.dataInfo.HiddenMaxLevel = -1;
                }

                OnPropertyChanged("HiddenMaxLevel");
            }
        }

        private MapSplunkObjectDataInfo dataInfo;

        public MapSplunkObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;
                this.dataInfo = value;
                this.OnPropertyChanged("DataInfo");

                this.IsCheckedInterval = this.dataInfo.UseSchedule;

                if (this.IsCheckedInterval != null && this.IsCheckedInterval.Value)
                {
                    this.SelectedIntervalUnit = this.dataInfo.IntervalUnitType;
                    
                    this.IntervalSeconds = this.RevertSeconds(this.SelectedIntervalUnit, this.dataInfo.IntervalSeconds);
                }

                this.IsCheckedHideIcon = this.dataInfo.IsIconHidden;
                this.ShowXAxis = value.ShowXAxis;
                this.ChartDateTimeFormat = value.ChartDateTimeFormat;

                // 주의: Property로 대입하면 대입하는 순서에 따라 Range 설정이 실패할 수 있으므로 이렇게 처리한다.
                this.yAxisRangeMin = value.YAxisRangeMin;
                this.yAxisRangeMax = value.YAxisRangeMax;
                this.OnPropertyChanged("YAxisRangeMin");
                this.OnPropertyChanged("YAxisRangeMax");

                this.SettingSplunkArguments(this.dataInfo.SplunkBasicInformation);
            }
        }

        private bool allChart;

        public bool AllChart
        {
            get { return this.allChart; }
            set
            {
                if (this.allChart == value)
                    return;
                this.allChart = value;
                this.OnPropertyChanged("AllChart");
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

        public Dictionary<IntervalUnitType, string> IntervalUnitTypes { get; private set; }

        private IntervalUnitType selectedIntervalUnit;
        public IntervalUnitType SelectedIntervalUnit
        {
            get { return this.selectedIntervalUnit; }
            set
            {
                this.selectedIntervalUnit = value;
                OnPropertyChanged("SelectedIntervalUnit");

                if(this.IsInitializeValues)
                {
                    this.dataInfo.IntervalUnitType = this.selectedIntervalUnit;
                    this.IntervalSeconds = 1;
                }
            }
        }

        private bool? isCheckedInterval;
        public bool? IsCheckedInterval
        {
            get { return this.isCheckedInterval; }
            set
            {
                if (this.isCheckedInterval == value && value == false) return;

                this.isCheckedInterval = value;
                OnPropertyChanged("IsCheckedInterval");

                if (!IsInitializeValues) return;

                if (this.isCheckedInterval == null)
                {
                    this.IsCheckedInterval = false;
                }
                else if (this.isCheckedInterval.Value)
                {
                    this.IntervalSeconds = 1;
                    this.SelectedIntervalUnit = IntervalUnitType.Seconds;
                }

                this.dataInfo.UseSchedule = this.isCheckedInterval;
            }
        }

        private int? intervalSeconds;
        public int? IntervalSeconds
        {
            get { return this.intervalSeconds; }
            set
            {
                this.intervalSeconds = value;
                OnPropertyChanged("IntervalSeconds");

                if (this.IsInitializeValues)
                {
                    this.dataInfo.IntervalSeconds = this.ConvertSeconds();
                }
            }
        }

        private bool? isCheckedHideIcon;
        public bool? IsCheckedHideIcon
        {
            get { return this.isCheckedHideIcon; }
            set
            {
                if (this.isCheckedHideIcon == value && value == false) return;

                this.isCheckedHideIcon = value;
                OnPropertyChanged("IsCheckedHideIcon");

                if (!IsInitializeValues) return;

                if (this.isCheckedHideIcon == null)
                {
                    this.IsCheckedHideIcon = false;
                }

                this.dataInfo.IsIconHidden = this.isCheckedHideIcon;
            }
        }
        private bool showXAxis;
        public bool ShowXAxis
        {
            get { return this.showXAxis; }
            set
            {
                if (value.Equals(this.showXAxis)) return;
                this.showXAxis = value;
                this.OnPropertyChanged("ShowXAxis");
                this.dataInfo.ShowXAxis = value;
            }
        }

        private double yAxisRangeMin = double.NaN;
        public double YAxisRangeMin
        {
            get { return this.yAxisRangeMin; }
            set
            {
                if (value.Equals(this.yAxisRangeMin)) return;
                this.yAxisRangeMin = value;
                this.OnPropertyChanged("YAxisRangeMin");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private double yAxisRangeMax = double.NaN;
        public double YAxisRangeMax
        {
            get { return this.yAxisRangeMax; }
            set
            {
                if (value.Equals(this.yAxisRangeMax)) return;
                this.yAxisRangeMax = value;
                this.OnPropertyChanged("YAxisRangeMax");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string chartDateTimeFormat;
        public string ChartDateTimeFormat
        {
            get { return this.chartDateTimeFormat; }
            set
            {
                if (value == this.chartDateTimeFormat) return;
                this.chartDateTimeFormat = value;
                this.OnPropertyChanged("ChartDateTimeFormat");
                this.dataInfo.ChartDateTimeFormat = value;
            }
        }

        #endregion Field

        #region Construction

        public SplunkPropertyControlViewModel()
        {
            this.IsInitializeValues = false;

            this.IntervalUnitTypes = new Dictionary<IntervalUnitType, string>();
            IntervalUnitTypes.Add(IntervalUnitType.Seconds, Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Seconds);
            IntervalUnitTypes.Add(IntervalUnitType.Minutes, Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Minutes);
            IntervalUnitTypes.Add(IntervalUnitType.Hours, Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Hours);
        }

        #endregion Construction

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

        private int? ConvertSeconds()
        {
            if (this.IntervalUnitTypes[this.selectedIntervalUnit] == Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Seconds)
            {
                return this.intervalSeconds;
            }
            else if (this.IntervalUnitTypes[this.selectedIntervalUnit] == Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Minutes)
            {
                return this.intervalSeconds * 60;
            }
            else if (this.IntervalUnitTypes[this.selectedIntervalUnit] == Resource_ArcGISControl_Properties.Splunk_ComboBox_IntervalUnit_Hours)
            {
                return this.intervalSeconds * 60 * 60;
            }

            return null;
        }

        private int? RevertSeconds(IntervalUnitType intervalUnitType, int? seconds)
        {
            var newValue = seconds;

            switch (intervalUnitType)
            {
                case IntervalUnitType.Seconds:
                break;
                case IntervalUnitType.Minutes:
                    newValue = seconds / 60;
                break;
                case IntervalUnitType.Hours:
                    newValue = seconds / 60 / 60;
                break;
            }
            return newValue;
        }

        #endregion //Methods

        #region Command

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

        private RelayCommand applySPLCommand;
        public ICommand ApplySPLCommand
        {
            get
            {
                return this.applySPLCommand ??
                       (this.applySPLCommand =
                        new RelayCommand(param => this.ApplySPL(), param2 => this.CanApplySPL()));
            }
        }

        private void ApplySPL()
        {
            if (this.onApplySplunkArgumentData != null) this.onApplySplunkArgumentData(this.dataInfo);
        }

        private bool CanApplySPL()
        {
            return this.dataInfo.SplunkBasicInformation != null && !string.IsNullOrEmpty(this.dataInfo.SplunkBasicInformation.Name) &&
                this.dataInfo.SplunkBasicInformation.Name.ToLower() != "none";
        }

        private RelayCommand applyYRangeCommand;
        public ICommand ApplyYRangeCommand
        {
            get
            {
                return this.applyYRangeCommand ??
                    (this.applyYRangeCommand =
                    new RelayCommand(param => this.ApplyYRange(), param => this.CanApplyYRange()));
            }
        }

        protected virtual void ApplyYRange()
        {
            this.dataInfo.SetYAxisRange(this.YAxisRangeMin, this.YAxisRangeMax);
        }

        protected virtual bool CanApplyYRange()
        {
            if (this.YAxisRangeMin >= this.YAxisRangeMax)
                return false;
            return true;
        }

        #endregion
    }
}
