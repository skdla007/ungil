
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Interface;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CameraOverlayControl.xaml
    /// </summary>
    public partial class CameraOverlayControl : UserControl
    {
        #region Fields

        /// <summary>
        /// 플레이백 컨트롤의 상태를 저장하기 위한 리스트
        /// </summary>
        private readonly Dictionary<string, PlaybackControlStateInfo> playbackControlStateInfoDic = new Dictionary<string, PlaybackControlStateInfo>();

        /// <summary>
        /// CameraUnigrid ID
        /// 카메라 오브젝트 하나당 한개
        /// </summary>
        private string cameraUnigridGuid = string.Empty;
        public string UnigridGuid
        {
            get { return this.cameraUnigridGuid; }
        }

        /// <summary>
        /// 카메라 정보 Id
        /// </summary>
        private string cameraInformationId = string.Empty;

        private PlaybackControlStateInfo syncedPlaybackControlStateInfo;

        private IArcGISControlViewerAPI arcGisControlViewerApi;

        private bool canPlaybackOverlayControl;
        public bool CanPlaybackOverlayControl
        {
            get { return this.canPlaybackOverlayControl; }
            set { this.canPlaybackOverlayControl = value; }
        }

        private bool canRdsOverlayControl;
        public bool CanRdsOverlayControl
        {
            get { return this.canRdsOverlayControl; }
            set { this.canRdsOverlayControl = value; }
        }

        private bool canPanomorphControl;
        public bool CanPanomorphControl
        {
            get { return this.canPanomorphControl; }
            set { this.canPanomorphControl = value; }
        }

        private int? zIndex; 

        public bool IsUsingOverlayControl
        {
            get
            {
                return this.xIndependentPlaybackOverlayControl.xTimePickerControlPopup.IsOpen ||
                       this.xRdsOverlayControl.EnableRdsControl;
            }
        }
        
        #endregion Fields
        
        #region Construction

        public CameraOverlayControl(IArcGISControlViewerAPI arcGisControlViewerApi)
        {
            InitializeComponent();

            this.Cursor = Cursors.Arrow;

            this.MouseLeave += OnMouseLeave;
            this.SizeChanged += CameraOverlayControl_SizeChanged;
            this.IsVisibleChanged += CameraOverlayControl_IsVisibleChanged;

            this.xIndependentPlaybackOverlayControl.eStateChanged += IndependentPlaybackOverlayControl_eStateChanged;
            this.xIndependentPlaybackOverlayControl.eStartIndependentPlayback += xIndependentPlaybackOverlayControl_eStartIndependentPlayback;
            this.xIndependentPlaybackOverlayControl.eEndIndependentPlayback += xIndependentPlaybackOverlayControl_eEndIndependentPlayback;
            this.xIndependentPlaybackOverlayControl.ePlayButtonClicked += XIndependentPlaybackOverlayControlOnEPlayButtonClicked;
            this.xIndependentPlaybackOverlayControl.eGoButtonClicked += XIndependentPlaybackOverlayControlOnEGoButtonClicked;
            this.xIndependentPlaybackOverlayControl.eRewindButtonClicked += XIndependentPlaybackOverlayControlOnERewindButtonClicked;
            this.xIndependentPlaybackOverlayControl.ePauseButtonClicked += XIndependentPlaybackOverlayControlOnEPauseButtonClicked;
            this.xIndependentPlaybackOverlayControl.eSpeedChanged += XIndependentPlaybackOverlayControlOnESpeedChanged;
            this.xIndependentPlaybackOverlayControl.eAlertBroadcastCapture += xIndependentPlaybackOverlayControl_eAlertBroadcastCapture;
            this.xIndependentPlaybackOverlayControl.ePlayTimeChanged += xIndependentPlaybackOverlayControl_ePlayTimeChanged;

            this.xRdsOverlayControl.eStartRdsControl += XRdsOverlayControl_eStartRdsControl;
            this.xRdsOverlayControl.eEndRdsControl += XRdsOverlayControlOnEEndRdsControl;
            this.xRdsOverlayControl.eRdsControled += XRdsOverlayControlOnERdsControled;

            this.xPanomorphOverlayControl.eDefaultViewChanging += XPanomorphOverlayControlOnEDefaultViewChanging;
            this.xPanomorphOverlayControl.eViewDragChanging += XPanomorphOverlayControlOnEViewDragChanging;
            this.xPanomorphOverlayControl.eViewPositionChanging += XPanomorphOverlayControlOnEViewPositionChanging;
            this.xPanomorphOverlayControl.eViewRectChanging += XPanomorphOverlayControlOnEViewRectChanging;
            this.xPanomorphOverlayControl.eViewTypeChanged += XPanomorphOverlayControlOnEViewTypeChanged;

            this.arcGisControlViewerApi = arcGisControlViewerApi;

            this.Visibility = Visibility.Collapsed;
        }

        #endregion 

        #region Event Handlers

        #region Overlay Control EventHandlers

        private void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!this.IsUsingOverlayControl)
            {
               this.HideControl();
            }
        }

        private void CameraOverlayControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.ChangedVisibleChanged(true);
        }

        private void CameraOverlayControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ChangedVisibleChanged();
        }

        #endregion //OverlayControl EventHandlers

        #region IndependentPlayBackOverlayControl EventHandlers

        private void IndependentPlaybackOverlayControl_eStateChanged(object sender, EventArgs e)
        {
            this.RestorePlaybackControlState();
        }

        private void xIndependentPlaybackOverlayControl_eStartIndependentPlayback(object sender, EventArgs e)
        {
            this.xRdsOverlayControl.Visibility = Visibility.Collapsed;

            var point = new Point(0, 0);
            if (PresentationSource.FromVisual(this) != null)
                point = this.PointToScreenDIU(new Point(0, 0));

            if (arcGisControlViewerApi != null)
            {
                var layoutInPixels = ScreenUtil.DIUToPixels(new Rect(point, this.RenderSize));
                this.arcGisControlViewerApi.StartPlaybackMode(this.cameraUnigridGuid,
                                                              this.cameraInformationId,
                                                              this.xIndependentPlaybackOverlayControl.PlayPosition,
                                                              layoutInPixels,
                                                              this.zIndex);
            }
        }

        private void xIndependentPlaybackOverlayControl_eEndIndependentPlayback(object sender, EventArgs e)
        {
            if (this.canRdsOverlayControl)
            {
                this.xRdsOverlayControl.Visibility = Visibility.Visible;
            }

            if (arcGisControlViewerApi == null) return;

            this.arcGisControlViewerApi.EndPlaybackMode(this.cameraUnigridGuid);
            if (PresentationSource.FromVisual(this) != null)
            {
                var layoutInPixels = ScreenUtil.DIUToPixels(new Rect(this.PointToScreenDIU(new Point(0, 0)),
                                                                                           this.RenderSize));
                this.arcGisControlViewerApi.MoveCameraVideo(this.cameraUnigridGuid, this.cameraInformationId,
                    layoutInPixels.X, layoutInPixels.Y, layoutInPixels.Width, layoutInPixels.Height);
            }
        }

        private void XIndependentPlaybackOverlayControlOnEPlayButtonClicked(object sender, EventArgs eventArgs)
        {
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.PlayPlayback(this.cameraUnigridGuid, 1);
        }

        private void XIndependentPlaybackOverlayControlOnEGoButtonClicked(object sender, EventArgs eventArgs)
        {
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.SeekPlayback(this.cameraUnigridGuid, this.xIndependentPlaybackOverlayControl.PlayPosition);
        }

        private void XIndependentPlaybackOverlayControlOnERewindButtonClicked(object sender, EventArgs eventArgs)
        {
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.RewindPlayback(this.cameraUnigridGuid, 1);
        }

        private void XIndependentPlaybackOverlayControlOnEPauseButtonClicked(object sender, EventArgs eventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.PausePlayback(this.cameraUnigridGuid);
            }
        }

        private void XIndependentPlaybackOverlayControlOnESpeedChanged(object sender, ValueChangedEventArgs valueChangedEventArgs)
        {
            //throw new NotImplementedException();

            var speed = Convert.ToDouble(valueChangedEventArgs.DoubleValue);

            if (Math.Abs(speed - 0) < 0.1)
            {
                this.arcGisControlViewerApi.PausePlayback(this.cameraUnigridGuid);
            }
            else if (speed < -0.1)
            {
                this.arcGisControlViewerApi.RewindPlayback(this.cameraUnigridGuid, Math.Abs(speed));
            }
            else if (speed > 0.1)
            {
                this.arcGisControlViewerApi.PlayPlayback(this.cameraUnigridGuid, Math.Abs(speed));
            }
        }


        void xIndependentPlaybackOverlayControl_eAlertBroadcastCapture(object sender, EventArgs e)
        {
            if (!this.canPlaybackOverlayControl)
                return;

            if (!this.syncedPlaybackControlStateInfo.EnablePlayback)
                return;

            if (!this.syncedPlaybackControlStateInfo.CanPlayback)
                return;

            this.arcGisControlViewerApi.AlertBroadCast(this.syncedPlaybackControlStateInfo.PlaybackCameraGuid, this.syncedPlaybackControlStateInfo.CurrentTime);

        }

        void xIndependentPlaybackOverlayControl_ePlayTimeChanged(object sender, EventArgs e)
        {
            var syncInfo = this.syncedPlaybackControlStateInfo;
            if (syncInfo != null)
            {
                syncInfo.CurrentTime = this.xIndependentPlaybackOverlayControl.PlayPosition;
            }
        }

        #endregion //IndependentPalybackOverlayControl EventHandlers

        #region RdsOverlayControl EventHandlers

        private void XRdsOverlayControl_eStartRdsControl(object sender, EventArgs e)
        {
            this.xIndependentPlaybackOverlayControl.Visibility = Visibility.Collapsed;
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.StartRdsControl(this.cameraInformationId);
        }

        private void XRdsOverlayControlOnEEndRdsControl(object sender, EventArgs eventArgs)
        {
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.EndRdsControl(this.cameraInformationId);
        }

        private void XRdsOverlayControlOnERdsControled(object sender, RdsControlEventArgs rdsControlEventArgs)
        {
            if (arcGisControlViewerApi != null) this.arcGisControlViewerApi.SendRdsControlData(this.cameraInformationId, rdsControlEventArgs.Data);
        }

        #endregion //RdsOverlayControl EventHandlers

        #region PanomorphOverlayControl EventHandlers

        private void XPanomorphOverlayControlOnEDefaultViewChanging(object sender, PanomorphOverlayUserControl.ViewTypeChangedEventArgs viewTypeChangedEventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.MovePanomorphCameraDefaultView(viewTypeChangedEventArgs.ObjectId,
                                                                           viewTypeChangedEventArgs.PanomorphViewType);
            }
                
        }

        private void XPanomorphOverlayControlOnEViewDragChanging(object sender, PanomorphOverlayUserControl.ViewDragChangingEventArgs viewDragChangingEventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.MovePanomorphCameraMouseControl(viewDragChangingEventArgs.ObjectId,
                                                                            viewDragChangingEventArgs.ViewNumber,
                                                                            viewDragChangingEventArgs.
                                                                            PanomorphMovedPoint,
                                                                            viewDragChangingEventArgs.IsLeftButtonDown);
            }   
        }

        private void XPanomorphOverlayControlOnEViewPositionChanging(object sender, PanomorphOverlayUserControl.ViewPositionChangingEventArgs viewPositionChangingEventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.MovePanomorphCameraPoint(viewPositionChangingEventArgs.ObjectId,
                                                                     viewPositionChangingEventArgs.ViewNumber,
                                                                     viewPositionChangingEventArgs.PanomorphMovedPoint);
            }
                
        }

        private void XPanomorphOverlayControlOnEViewRectChanging(object sender, PanomorphOverlayUserControl.ViewRectChangingEventArgs viewAreaChangingEventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.MovePanomorphCameraRect(viewAreaChangingEventArgs.ObjectId,
                                                                    viewAreaChangingEventArgs.ViewNumber,
                                                                    viewAreaChangingEventArgs.SelectionArea);
            }
                
        }


        private void XPanomorphOverlayControlOnEViewTypeChanged(object sender, PanomorphOverlayUserControl.ViewTypeChangedEventArgs viewTypeChangedEventArgs)
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.ChangePanomorphViewType(viewTypeChangedEventArgs.ObjectId,
                                                                    viewTypeChangedEventArgs.PanomorphViewType);
            }

        }

        #endregion PanomorphOverlayControl EventHandlers

        #endregion // Event Handlers

        #region Methods

        #region Control State

        public void ClearControlState()
        {
            this.playbackControlStateInfoDic.Clear();
        }

        private void ApplyControlState()
        {
            // 저장 정보 적용
            if (this.syncedPlaybackControlStateInfo.CanPlayback)
            {
                this.xIndependentPlaybackOverlayControl.Visibility = Visibility.Visible;

                if (this.syncedPlaybackControlStateInfo.EnablePlayback)
                {
                    this.xIndependentPlaybackOverlayControl.EnablePlayback = true;

                    switch (this.syncedPlaybackControlStateInfo.PlayState)
                    {
                        case PlaybackControlStateInfo.PlaybackStates.Pause:
                            this.xIndependentPlaybackOverlayControl.xPlayToggleButton.IsChecked = false;
                            this.xIndependentPlaybackOverlayControl.xRewindToggleButton.IsChecked = false;
                            break;

                        case PlaybackControlStateInfo.PlaybackStates.Play:
                            this.xIndependentPlaybackOverlayControl.xPlayToggleButton.IsChecked = true;
                            this.xIndependentPlaybackOverlayControl.xRewindToggleButton.IsChecked = false;
                            break;

                        case PlaybackControlStateInfo.PlaybackStates.Rewind:
                            this.xIndependentPlaybackOverlayControl.xPlayToggleButton.IsChecked = false;
                            this.xIndependentPlaybackOverlayControl.xRewindToggleButton.IsChecked = true;
                            break;
                    }

                    this.xIndependentPlaybackOverlayControl.PlayPosition =
                        this.syncedPlaybackControlStateInfo.CurrentTime;
                }
                else
                {
                    this.xIndependentPlaybackOverlayControl.EnablePlayback = false;
                }
            }
            else
            {
                this.xIndependentPlaybackOverlayControl.Visibility = Visibility.Collapsed;
                this.xIndependentPlaybackOverlayControl.EnablePlayback = false;
            }

            if (this.canRdsOverlayControl && !this.xIndependentPlaybackOverlayControl.EnablePlayback)
            {
                this.xRdsOverlayControl.Visibility = Visibility.Visible;
            }
            else
            {
                this.xRdsOverlayControl.Visibility = Visibility.Collapsed;
            }

            if (this.canPanomorphControl)
            {
                this.xPanomorphOverlayControl.Visibility = Visibility.Visible;
            }
            else
            {
                this.xPanomorphOverlayControl.Visibility = Visibility.Collapsed;
            }
        }

        private void RestorePlaybackControlState()
        {
            this.syncedPlaybackControlStateInfo.CanPlayback = this.canPlaybackOverlayControl;

            this.syncedPlaybackControlStateInfo.CurrentTime = this.xIndependentPlaybackOverlayControl.PlayPosition;

            this.syncedPlaybackControlStateInfo.EnablePlayback = this.xIndependentPlaybackOverlayControl.EnablePlayback;

            if (this.xIndependentPlaybackOverlayControl.xPlayToggleButton.IsChecked == true)
            {
                this.syncedPlaybackControlStateInfo.PlayState = PlaybackControlStateInfo.PlaybackStates.Play;
            }
            else if (this.xIndependentPlaybackOverlayControl.xRewindToggleButton.IsChecked == true)
            {
                this.syncedPlaybackControlStateInfo.PlayState = PlaybackControlStateInfo.PlaybackStates.Rewind;
            }
            else
            {
                this.syncedPlaybackControlStateInfo.PlayState = PlaybackControlStateInfo.PlaybackStates.Pause;
            }
        }

        /// <summary>
        /// Instant Playback 및 RDS Button Resizing시 MinWidth에 따라 Visibility처리
        /// </summary>
        /// <param name="callBySizeChanged"></param>
        private void ChangedVisibleChanged(bool callBySizeChanged = false)
        {
            if (this.canPlaybackOverlayControl)
            {
                this.xIndependentPlaybackOverlayControl.Visibility
                    = this.ActualWidth < this.xIndependentPlaybackOverlayControl.MinWidth
                          ? Visibility.Collapsed
                          : Visibility.Visible;
            }

            if (this.canRdsOverlayControl)
            {
                if (this.xIndependentPlaybackOverlayControl.EnablePlayback)
                {
                    this.xRdsOverlayControl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.xRdsOverlayControl.Visibility
                    = this.ActualWidth < this.xRdsOverlayControl.MinWidth
                          ? Visibility.Collapsed
                          : Visibility.Visible;
                }
            }

            if (this.CanPanomorphControl)
            {
                this.xPanomorphOverlayControl.Visibility 
                    = this.ActualHeight < this.xPanomorphOverlayControl.MinHeight
                            ? Visibility.Collapsed
                            : Visibility.Visible;

                if(!callBySizeChanged)
                {
                    this.xPanomorphOverlayControl.xControlVisibleToggleButton.IsChecked = false;
                    this.xPanomorphOverlayControl.IsControlOpened = false;
                    this.xPanomorphOverlayControl.EnableSelection = false;
                    this.xPanomorphOverlayControl.EnablePTZ = false;
                }
                
            }
        }

        #endregion // Control State

        public void Initialize(string aObjectId, string aCameraInformationId, bool aCanPlaybackControl, bool aCanRdsControl, bool aPanomorphControl, string aCameraGuid, int? zIndex)
        {
            this.cameraUnigridGuid = aObjectId;

            this.cameraInformationId = aCameraInformationId;

            this.CanPlaybackOverlayControl = aCanPlaybackControl;

            this.CanRdsOverlayControl = aCanRdsControl;

            this.CanPanomorphControl = aPanomorphControl;

            this.zIndex = zIndex;

            this.syncedPlaybackControlStateInfo = null;

            if (this.playbackControlStateInfoDic.ContainsKey(aObjectId))
            {
                // 저장된 정보가 있는경우
                this.syncedPlaybackControlStateInfo = this.playbackControlStateInfoDic[aObjectId];
            }
            else
            {
                // 저장된 정보가 없는 경우
                var newInfo = new PlaybackControlStateInfo(aObjectId, aCameraGuid);
                this.playbackControlStateInfoDic.Add(aObjectId, newInfo);
                this.syncedPlaybackControlStateInfo = newInfo;
            }

            this.xPanomorphOverlayControl.Initailize(aObjectId);

            this.syncedPlaybackControlStateInfo.CanPlayback = aCanPlaybackControl;
            this.ApplyControlState();
        }

        public void UpdatePlayTime(string syncId, DateTime playTime)
        {
            if (this.playbackControlStateInfoDic.ContainsKey(syncId))
            {
                this.playbackControlStateInfoDic[syncId].CurrentTime = playTime;
            }

            if (this.syncedPlaybackControlStateInfo != null && this.syncedPlaybackControlStateInfo.UnigridGuid.Equals(syncId))
            {
                this.xIndependentPlaybackOverlayControl.PlayPosition = playTime;
            }
        }

        #region PopupCamera

        private void HideControl()
        {
            this.Visibility = Visibility.Collapsed;
            this.cameraInformationId = string.Empty;
        }

        public void ShowCameraPopupControl()
        {
            if (arcGisControlViewerApi != null)
            {
                this.arcGisControlViewerApi.OpenPanomorphPopupCamera();
            }
        }

        public void SetPanomorphViewTypeUI(PanomorphOverlayUserControl.PanomorphViewTypes type)
        {
            this.xPanomorphOverlayControl.SetPanomorphViewTypeUI(type);
        }

        #endregion //PopupCamera

        #endregion Methods
    }

    /// <summary>
    /// 플레이백 컨트롤의 상태 저장을 위한 클래스
    /// </summary>
    public class PlaybackControlStateInfo
    {
        /// <summary>
        /// 플레이백의 재생 상태.
        /// </summary>
        public enum PlaybackStates
        {
            Pause,
            Play,
            Rewind
        }

        // 플레이백 모드 사용 여부
        public bool CanPlayback { get; set; }

        // 플레이백 모드 실행 상태
        public bool EnablePlayback { get; set; }


        public string UnigridGuid { get; set; }

        public string PlaybackCameraGuid { get; set; }

        public PlaybackStates PlayState { get; set; }

        public DateTime CurrentTime { get; set; }

        public PlaybackControlStateInfo(string unigridGuid, string cameraGuid)
        {
            this.UnigridGuid = unigridGuid;
            this.PlaybackCameraGuid = cameraGuid;
            PlayState = PlaybackStates.Pause;
            CurrentTime = DateTime.Now;
            CanPlayback = false;
            EnablePlayback = false;
        }
    }
}
