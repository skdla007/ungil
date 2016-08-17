
using System.Windows.Input;

namespace ArcGISControls.Tools
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using ArcGISControl;
    using ArcGISControl.Helper;
    using ArcGISControls.CommonData.Models;
    using N3N.Controls.TimePickerControls;

    /// <summary>
    /// ArcGISClientViewerToolControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VWToolControl
    {
        #region Field

        private readonly VWToolControlViewModel viewModel;

        private VWBooKMarkRegisterWindow vwBooKMarkRegisterWindow;

        private VWLocationRegisterWindow vwLocationRegisterWindow;

        public ArcGISClientViewer ArcGisClientViewer
        {
            set { this.viewModel.ArcGisClientViewer = value; }
        }

        private readonly SolidColorBrush dateNormalForgroundBrush = BrushUtil.ConvertFromString("#FFE4E4E4");
        private readonly SolidColorBrush dateMouseOverForgroundBrush = BrushUtil.ConvertFromString("#FFFFAF00");

        public bool IsShowCalendar
        {
            get
            {
                return this.xTimePickerControlPopup.IsOpen;
            }
        }

        public bool IsDataPlaybackMode
        {
            get 
            {
                return this.xGridPlaybackToolBar.Visibility == Visibility.Visible;
            }
        }

        #endregion Field

        #region Construction

        public VWToolControl()
        {
            InitializeComponent();

            this.viewModel = new VWToolControlViewModel();
            this.viewModel.eBookMarkWindowShowClicked += this.viewModel_eBookMarkWindowShowClicked; 
            this.viewModel.eObjectAdded += this.ViewModel_eObjectAdded;
            this.viewModel.eMapObjectLoaded += this.viewModel_eMapObjectLoaded;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

            this.DataContext = this.viewModel;

            // Data Playback
            this.xIntervalTimePicker.SelectedTimeChanged += xIntervalTimePicker_SelectedTimeChanged;
            this.xDateGrid.MouseLeftButtonUp += xDateGrid_MouseLeftButtonUp;
            this.xDateGrid.MouseEnter += xDateGrid_MouseEnter;
            this.xDateGrid.MouseLeave += xDateGrid_MouseLeave;
            this.xTimePickerControl.eGoButtonClicked += xTimePickerControl_eGoButtonClicked;
        }

        private void viewModel_eMapObjectLoaded(object sender, EventArgs e)
        {
        }

        #endregion 

        #region Events

        /// MapObject 생성 통보
        public event EventHandler<ObjectDataEventArgs> eObjectAdded;

        private void RaiseObjectAddedEvent(BaseMapObjectInfoData data)
        {
            var objectEvent = this.eObjectAdded;
            if (objectEvent != null)
            {
                objectEvent(this, new ObjectDataEventArgs(data));
            }
        }

        private void RaiseLocationObjectAddedEvent(MapLocationObjectDataInfo dataInfo)
        {
            var copyData = new MapLocationObjectDataInfo(dataInfo);

            //Save를 하기 위해 LAT LNG로 변경 한다.
            var latLng = this.viewModel.ArcGisClientViewer.ToGeographic(copyData.Position);

            copyData.Position = new Point(latLng.Lng, latLng.Lat);

            this.RaiseObjectAddedEvent(copyData);
        }

        private void RaiseBookMarkObjectAddedEvent(MapBookMarkDataInfo dataInfo)
        {
            var copyData = new MapBookMarkDataInfo(dataInfo);

            var latLng = this.viewModel.ArcGisClientViewer.ToGeographic(copyData.ExtentMax);
            var maxPoint = new Point(latLng.Lng, latLng.Lat);

            latLng = this.viewModel.ArcGisClientViewer.ToGeographic(copyData.ExtentMin);
            var minPoint = new Point(latLng.Lng, latLng.Lat);

            copyData.ExtentRegion = new Rect(minPoint, maxPoint);

            this.RaiseObjectAddedEvent(copyData);
        }

        #endregion Events

        #region EventHandlers

        #region UserControl EventHandlers

        #endregion UserContorl EventHandlers

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.IsVisibleChanged += OnIsVisibleChanged;

            if (this.vwBooKMarkRegisterWindow == null)
            {
                this.vwBooKMarkRegisterWindow = new VWBooKMarkRegisterWindow(Window.GetWindow(this));
                this.vwBooKMarkRegisterWindow.onButtonOkClick += VwBooKMarkRegisterWindow_OnButtonOkClick;
                this.vwBooKMarkRegisterWindow.onButtonCancelClick += VwBooKMarkRegisterWindow_OnButtonCancelClick;
            }

            if (this.vwLocationRegisterWindow == null)
            {
                this.vwLocationRegisterWindow = new VWLocationRegisterWindow(Window.GetWindow(this));
                this.vwLocationRegisterWindow.onButtonOkClick += VwLocationRegisterWindow_OnButtonOkClick;
                this.vwLocationRegisterWindow.onButtonCancelClick += VwLocationRegisterWindow_OnButtonCancelClick;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.vwBooKMarkRegisterWindow != null)
            {
                this.vwBooKMarkRegisterWindow.onButtonOkClick -= VwBooKMarkRegisterWindow_OnButtonOkClick;
                this.vwBooKMarkRegisterWindow.onButtonCancelClick -= VwBooKMarkRegisterWindow_OnButtonCancelClick;
                this.vwBooKMarkRegisterWindow.Close();
                this.vwBooKMarkRegisterWindow = null;
            }

            if(this.vwLocationRegisterWindow != null)
            {
                this.vwLocationRegisterWindow.onButtonOkClick -= VwLocationRegisterWindow_OnButtonOkClick;
                this.vwLocationRegisterWindow.onButtonCancelClick -= VwLocationRegisterWindow_OnButtonCancelClick;
                this.vwLocationRegisterWindow.Close();
                this.vwLocationRegisterWindow = null;
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (this.viewModel != null && this.Visibility == Visibility.Visible) this.viewModel.SetIsEnableControlCamera();
        }

        #region PopupWindow EventHandlers

        private void VwLocationRegisterWindow_OnButtonCancelClick(object sender, EventArgs eventArgs)
        {
            this.vwLocationRegisterWindow.Hide();
            this.viewModel.ArcGisClientViewer.CancelAddedObjectData(this.vwLocationRegisterWindow.MapLocationObjectData);
        }

        private void VwLocationRegisterWindow_OnButtonOkClick(object sender, EventArgs eventArgs)
        {
            this.RaiseLocationObjectAddedEvent(this.vwLocationRegisterWindow.MapLocationObjectData);

            this.vwLocationRegisterWindow.Hide();
        }

        private void VwBooKMarkRegisterWindow_OnButtonCancelClick(object sender, EventArgs eventArgs)
        {
            this.vwBooKMarkRegisterWindow.MapBookMarkData.Name = string.Empty;

            this.vwBooKMarkRegisterWindow.Hide();
        }

        private void VwBooKMarkRegisterWindow_OnButtonOkClick(object sender, EventArgs eventArgs)
        {
            this.viewModel.MakeNewBookMark(this.vwBooKMarkRegisterWindow.MapBookMarkData.Name);

            this.vwBooKMarkRegisterWindow.Hide();
        }
        
        #endregion PopupWindow EventHandlers

        #region ViewModel EventHandlers

        /// <summary>
        /// Show Book Mark Window 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewModel_eBookMarkWindowShowClicked(object sender, EventArgs e)
        {
            if (this.vwBooKMarkRegisterWindow != null)
            {
                this.vwBooKMarkRegisterWindow.MapBookMarkData.Name = this.viewModel.GetNewBookMarkName();

                this.vwBooKMarkRegisterWindow.ShowDialog();
            }
        }

        private void ViewModel_eObjectAdded(object sender, ObjectDataEventArgs e)
        {
            var mapObjectDataInfo = e.mapObjectDataInfo;

            if (this.vwLocationRegisterWindow != null && mapObjectDataInfo is MapLocationObjectDataInfo)
            {
                this.vwLocationRegisterWindow.SetData(mapObjectDataInfo);
                this.vwLocationRegisterWindow.ShowDialog();
            }
            else if (mapObjectDataInfo is MapBookMarkDataInfo)
            {
                this.RaiseBookMarkObjectAddedEvent(mapObjectDataInfo as MapBookMarkDataInfo);
            }
        }

        #endregion VieModel EventHandlers

        #region Data Playback EventHandlers

        private void xTimePickerControl_eGoButtonClicked(object sender, EventArgs e)
        {
            this.xTimePickerControlPopup.IsOpen = false;
            this.viewModel.SeekPlayback(this.xTimePickerControl.GetDateTime());
        }

        private void xDateGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.xDateTextBlock.Foreground = dateNormalForgroundBrush;
            this.xTimeTextBlock.Foreground = dateNormalForgroundBrush;
        }

        private void xDateGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.xDateTextBlock.Foreground = dateMouseOverForgroundBrush;
            this.xTimeTextBlock.Foreground = dateMouseOverForgroundBrush;
        }

        private void xDateGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.xTimePickerControlPopup.IsOpen)
            {
                this.xTimePickerControlPopup.IsOpen = true;

                this.xTimePickerControl.SetDateTiem(this.viewModel.PlaybackCurrentDateTime);
            }
            else
            {
                this.ReleaseMouseCapture();
                this.xTimePickerControlPopup.IsOpen = false;
            }
        }

        private void xIntervalTimePicker_SelectedTimeChanged(object sender, TimeSelectedChangedRoutedEventArgs e)
        {
            this.viewModel.MovePlaybackInterval = this.xIntervalTimePicker.SelectedTime;
        }

        #endregion //Data Playback EventHandlers

        #endregion EventHandlers

        #region Methods

        public void SetIsReportWritingMode(bool isReportWritingMode)
        {
            if (this.viewModel == null)
                throw new InvalidOperationException("viewModel이 null일 때 사용할 수 없습니다.");

            this.viewModel.IsReportWritingMode = isReportWritingMode;
        }

        public void Dispose()
        {
            if (this.vwBooKMarkRegisterWindow != null)
            {
                this.vwBooKMarkRegisterWindow.onButtonOkClick -= VwBooKMarkRegisterWindow_OnButtonOkClick;
                this.vwBooKMarkRegisterWindow.onButtonCancelClick -= VwBooKMarkRegisterWindow_OnButtonCancelClick;
            }

            if (this.vwLocationRegisterWindow != null)
            {
                this.vwLocationRegisterWindow.onButtonOkClick -= VwLocationRegisterWindow_OnButtonOkClick;
                this.vwLocationRegisterWindow.onButtonCancelClick -= VwLocationRegisterWindow_OnButtonCancelClick;
            }

            if(this.viewModel != null)
            {
                this.viewModel.eBookMarkWindowShowClicked -= viewModel_eBookMarkWindowShowClicked;
                this.viewModel.eObjectAdded -= ViewModel_eObjectAdded;
                this.viewModel.eMapObjectLoaded -= this.viewModel_eMapObjectLoaded;
                this.viewModel.Dispose();
            }

            this.xIntervalTimePicker.SelectedTimeChanged -= xIntervalTimePicker_SelectedTimeChanged;
            this.xDateGrid.MouseLeftButtonUp -= xDateGrid_MouseLeftButtonUp;
            this.xDateGrid.MouseEnter -= xDateGrid_MouseEnter;
            this.xDateGrid.MouseLeave -= xDateGrid_MouseLeave;
            this.xTimePickerControl.eGoButtonClicked -= xTimePickerControl_eGoButtonClicked;
        }

        #endregion Methods

    }
}
