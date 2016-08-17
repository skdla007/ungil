using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using ArcGISControl;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.Tools
{   
    public class VWToolControlViewModel : DataChangedNotify.BaseModel
    {
        public VWToolControlViewModel()
        {
            this.PlaybackCurrentDateTime = DateTime.Now;

            this.DataPlaybackToolbarVisibility = Visibility.Collapsed;
            this.PrivateResourceToolbarVisibility = Visibility.Visible;
        }

        #region Field
        
        private bool isFirstSetting = true;

        private ArcGISClientViewer arcGisClientViewer;
        public ArcGISClientViewer ArcGisClientViewer
        {   
            get { return this.arcGisClientViewer; }
                
            set
            {
                if (this.arcGisClientViewer != null)
                {
                    this.arcGisClientViewer.eObjectAdded -= ArcGisClientViewer_eObjectAdded;
                    this.arcGisClientViewer.eObjectLoaded -= ArcGisClientViewer_eObjectLoaded;
                    this.arcGisClientViewer.eMapLoadingErrorOccured -= ArcGisClientViewer_eMapLoadingErrorOccured;
                    this.arcGisClientViewer.dataPlayBackTimerElapsed -= ArcGisClientViewerOnDataPlayBackTimerElapsed;
                    this.arcGisClientViewer.eDataPlaybackModeChanged -= arcGisClientViewer_eDataPlaybackModeChanged;
                }

                this.arcGisClientViewer = value;

                if (arcGisClientViewer != null)
                {
                    if (isFirstSetting)
                    {
                        this.arcGisClientViewer.eObjectAdded += ArcGisClientViewer_eObjectAdded; 
                        this.arcGisClientViewer.eObjectLoaded += ArcGisClientViewer_eObjectLoaded;
                        this.arcGisClientViewer.eMapLoadingErrorOccured += ArcGisClientViewer_eMapLoadingErrorOccured;
                        this.arcGisClientViewer.dataPlayBackTimerElapsed += ArcGisClientViewerOnDataPlayBackTimerElapsed;
                        this.arcGisClientViewer.eDataPlaybackModeChanged += arcGisClientViewer_eDataPlaybackModeChanged;
                        this.arcGisClientViewer.eChangeLinkedMap += ArcGisClientViewer_eChangeLinkedMap;
                    }

                    this.CameraList = this.arcGisClientViewer.CameraList;

                    isFirstSetting = false;
                }
            }
        }

        private bool isReportWritingMode;

        public bool IsReportWritingMode
        {
            get { return this.isReportWritingMode; }
            set
            {
                if (this.isReportWritingMode == value)
                    return;

                this.isReportWritingMode = value;
                this.OnPropertyChanged("IsReportWritingMode");
            }
        }

        private BaseMapObjectInfoData selectedCameraData;
        public BaseMapObjectInfoData SelectedCameraData
        {
            get { return this.selectedCameraData; }
            set
            {
                if (value != null && this.selectedCameraData != value && this.arcGisClientViewer != null)
                {
                    this.selectedCameraData = value;

                    if (this.arcGisClientViewer.SelectObject(selectedCameraData))
                        this.arcGisClientViewer.GoToLocation(selectedCameraData);
                    
                    OnPropertyChanged("SelectedCameraData");
                }
            }
        }

        private ObservableCollection<MapCameraObjectComponentDataInfo> cameraList;
        public ObservableCollection<MapCameraObjectComponentDataInfo> CameraList
        {
            get { return this.cameraList; }
            set
            {
                this.cameraList = value;
                OnPropertyChanged("CameraList");
            }
        }
        
        private bool doMakeLocation;
        public bool DoMakeLocation
        {
            get { return doMakeLocation; }
            set
            {   
                this.doMakeLocation = value;
                OnPropertyChanged("DoMakeLocation");
            }
        }

        public bool DoHideAllCamera
        {
            get { return this.arcGisClientViewer.IsHideAllCamera; }
            set
            {
                this.arcGisClientViewer.IsHideAllCamera = value;
                OnPropertyChanged("DoHideAllCamera");
            }
        }

        /// <summary>
        /// Camera List 가 없을 경우 Show/Hide Camera 와
        /// CameraListBox를 Enable 시킨다.
        /// </summary>
        private bool isEnabledControlCameraObject;
        public bool IsEnabledControlCameraObject
        {
            get { return this.isEnabledControlCameraObject; }
            set
            {
                this.isEnabledControlCameraObject = value;
                OnPropertyChanged("IsEnabledControlCameraObject");
            }
        }

        // Data Playback 

        private string playbackDateText;
        public string PlaybackDateText
        {
            get
            {
                return this.playbackDateText;
            }
            set
            {
                this.playbackDateText = value;
                this.OnPropertyChanged("PlaybackDateText");
            }
        }

        private string playbackTimeText;
        public string PlaybackTimeText
        {
            get
            {
                return this.playbackTimeText;
            }
            set
            {
                this.playbackTimeText = value;
                this.OnPropertyChanged("PlaybackTimeText");
            }
        }

        private TimeSpan movePlaybackInterval;
        public TimeSpan MovePlaybackInterval
        {
            get
            {
                return this.movePlaybackInterval;
            }
            set
            {
                this.movePlaybackInterval = value;
            }
        }

        private DateTime playbackCurrentDateTime;
        public DateTime PlaybackCurrentDateTime
        {
            get
            {
                return this.playbackCurrentDateTime;
            }

            set
            {
                this.playbackCurrentDateTime = value;

                this.PlaybackDateText = value.ToString("d", CultureInfo.CurrentCulture);
                this.PlaybackTimeText = value.ToString("tt h:mm:ss", CultureInfo.CurrentCulture);
            }
        }

        private Visibility dataPlaybackToolbarVisibility;
        public Visibility DataPlaybackToolbarVisibility
        {
            get { return this.dataPlaybackToolbarVisibility; }
            set 
            { 
                this.dataPlaybackToolbarVisibility = value;
                this.OnPropertyChanged("DataPlaybackToolbarVisibility");
            }
        }

        private Visibility privateResourceToolbarVisibility;
        public Visibility PrivateResourceToolbarVisibility
        {
            get { return this.privateResourceToolbarVisibility; }
            set
            {
                this.privateResourceToolbarVisibility = value;
                this.OnPropertyChanged("PrivateResourceToolbarVisibility");
            }
        }

        private int defaultIntervalSeconds = 0;
        public int DefaultIntervalSeconds
        {
            get { return this.defaultIntervalSeconds; }
            set
            {
                this.defaultIntervalSeconds = value;
                this.OnPropertyChanged("DefaultIntervalSeconds");
            }
        }

        private int defaultIntervalMinutes = 0;
        public int DefaultIntervalMinutes
        {
            get { return this.defaultIntervalMinutes; }
            set
            {
                this.defaultIntervalMinutes = value;
                this.OnPropertyChanged("DefaultIntervalMinutes");
            }
        }

        private int defaultIntervalHours = 0;
        public int DefaultIntervalHours
        {
            get { return this.defaultIntervalHours; }
            set
            {
                this.defaultIntervalHours = value;
                this.OnPropertyChanged("DefaultIntervalHours");
            }
        }

        private bool canChangeLinkedMap = false;

        #endregion //Field

        #region Events

        public event EventHandler eBookMarkWindowShowClicked;

        private void RaiseBookMarkWindowShowClickedEvent()
        {
            var handler = this.eBookMarkWindowShowClicked;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

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

        public event EventHandler<EventArgs> eMapReloaded;

        protected virtual void OnEMapReloaded()
        {
            var handler = eMapReloaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> eMapObjectLoaded;

        protected virtual void OnEMapObjectLoaded()
        {
            var handler = eMapObjectLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion Events
        
        #region EventHandlers

        private void ArcGisClientViewer_eObjectAdded(object sender, ObjectDataEventArgs objectDataEventArgs)
        {
            var mapObjectDataInfo = objectDataEventArgs.mapObjectDataInfo;

            if (mapObjectDataInfo == null) //Object 등록 취소
                this.DoMakeLocation = false;

            else if (mapObjectDataInfo is MapLocationObjectDataInfo) //Object 등록 
            {
                this.DoMakeLocation = false;

                this.RaiseObjectAddedEvent(mapObjectDataInfo);
            }
            else if (mapObjectDataInfo is MapBookMarkDataInfo)
            {
                this.RaiseObjectAddedEvent(mapObjectDataInfo);
            }
        }


        private void ArcGisClientViewerOnAddedMapObjectData(BaseMapObjectInfoData mapObjectDataInfo)
        {
            
        }

        private void ArcGisClientViewer_eMapLoadingErrorOccured(object sender, MapLoadingErrorEventArgs mapLoadingError)
        {
            this.SetIsEnableControlCamera();
        }

        private void ArcGisClientViewerOnDataPlayBackTimerElapsed(DateTime timer)
        {
            this.PlaybackCurrentDateTime = timer;
        }

        private void ArcGisClientViewer_eObjectLoaded(object sender, EventArgs eventArgs)
        {
            this.canChangeLinkedMap = true;

            this.SetIsEnableControlCamera();

            this.OnEMapObjectLoaded();
        }

        private void arcGisClientViewer_eDataPlaybackModeChanged(object sender, EventArgs e)
        {
            if (this.arcGisClientViewer.IsDataPlayBackMode)
            {
                this.DataPlaybackToolbarVisibility = Visibility.Visible;
                this.PrivateResourceToolbarVisibility = Visibility.Collapsed;
            }
            else
            {
                this.DataPlaybackToolbarVisibility = Visibility.Collapsed;
                this.PrivateResourceToolbarVisibility = Visibility.Visible;
            }
        }

        private void ArcGisClientViewer_eChangeLinkedMap(object sender, EventArgs eventArgs)
        {
            this.canChangeLinkedMap = false;
        }

        #endregion EventHandlers

        #region Methods

        /// <summary>
        /// Map List가 Reload 된 후 Enable 설정을 다시 해준다.
        /// </summary>
        public void SetIsEnableControlCamera()
        {
            if (this.CameraList == null || this.CameraList.Count <= 0)
            {
                this.IsEnabledControlCameraObject = false;
            }
            else
            {
                this.IsEnabledControlCameraObject = true;
            }
        }

        /// <summary>
        /// 새로 BookMark 만들때 새로운 이름 받기
        /// </summary>
        /// <returns></returns>
        public string GetNewBookMarkName()
        {
            return this.arcGisClientViewer == null ? string.Empty : this.arcGisClientViewer.GetNewLocationObjectName(MapObjectType.BookMark);
        }

        /// <summary>
        /// Book Mark 만들기
        /// </summary>
        /// <param name="bookMarkName"></param>
        public bool MakeNewBookMark(string bookMarkName)
        {
            if (this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.MakeBookMark(bookMarkName);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.ArcGisClientViewer = null;
        }

        public void SeekPlayback(DateTime aDateTime)
        {
            if (aDateTime > DateTime.Now)
            {
                return;
            }

            this.arcGisClientViewer.SetSeekTime(aDateTime);
        }

        private void SetInterval(int hour = 0, int min = 0, int second = 0)
        {
            this.DefaultIntervalHours = hour;
            this.DefaultIntervalMinutes = min;
            this.DefaultIntervalSeconds = second;   
        }

        #endregion //Methods

        #region Command

        private RelayCommand buttonMakeLocationCommand;

        public ICommand ButtonMakeLocationCommand
        {
            get
            {
                return this.buttonMakeLocationCommand ??
                (this.buttonMakeLocationCommand =
                 new RelayCommand(param => this.MakeLocation(), param => this.CanMakeLocation()));
            }
        }

        public void MakeLocation()
        {
            if (this.arcGisClientViewer == null) return;

            if (this.doMakeLocation)
            {
                this.arcGisClientViewer.SelectedMapObjectType = MapObjectType.Location;
            }
            else
            {
                this.arcGisClientViewer.SelectedMapObjectType = MapObjectType.None;
            }
        }

        private bool CanMakeLocation()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.arcGisClientViewer.IsAbleToEditMap;
        }

        private RelayCommand buttonMakeBookMarkCommand;

        public ICommand ButtonMakeBookMarkCommand
        {
            get
            {
                return this.buttonMakeBookMarkCommand ??
                (this.buttonMakeBookMarkCommand =
                 new RelayCommand(param => this.MakeBookMark(), param => this.CanMakeBookMark()));
            }
        }

        public void MakeBookMark()
        {
            if (this.arcGisClientViewer == null) return;

            this.DoMakeLocation = false;

            this.RaiseBookMarkWindowShowClickedEvent();
        }

        private bool CanMakeBookMark()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.arcGisClientViewer.IsAbleToEditMap;
        }

        private RelayCommand buttonGoPreMapCommand;

        public ICommand ButtonGoPreMapCommand
        {
            get
            {
                return this.buttonGoPreMapCommand ??
                (this.buttonGoPreMapCommand =
                 new RelayCommand(param => this.ExecuteGoPreMap(), param => this.CanGoPreMap()));
            }
        }

        public void ExecuteGoPreMap()
        {
            if (this.arcGisClientViewer != null) this.arcGisClientViewer.GoLinkedMap(true);
        }

        private bool CanGoPreMap()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.canChangeLinkedMap && this.arcGisClientViewer.ViewingLinkedMapIndex > 0;
        }

        private RelayCommand buttonGoNextMapCommand;

        public ICommand ButtonGoNextMapCommand
        {
            get
            {
                return this.buttonGoNextMapCommand ??
                (this.buttonGoNextMapCommand =
                 new RelayCommand(param => this.ExecuteGoNextMap(), param => this.CanGoNextMap()));
            }
        }

        public void ExecuteGoNextMap()
        {

            if (this.arcGisClientViewer != null) this.arcGisClientViewer.GoLinkedMap(false);
        }

        private bool CanGoNextMap()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.canChangeLinkedMap && this.arcGisClientViewer.LinkedMapTotalCount - 1 > this.arcGisClientViewer.ViewingLinkedMapIndex;
        }

        private RelayCommand buttonGoHomeMapCommand;

        public ICommand ButtonGoHomeMapCommand
        {
            get
            {
                return this.buttonGoHomeMapCommand ??
                (this.buttonGoHomeMapCommand =
                 new RelayCommand(param => this.ExecuteGoHomeMap(), param => this.CanGoHomeMap()));
            }
        }

        public void ExecuteGoHomeMap()
        {
            if (this.arcGisClientViewer != null) this.arcGisClientViewer.GoHomeMap();
        }

        private bool CanGoHomeMap()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.canChangeLinkedMap && this.arcGisClientViewer.LinkedMapTotalCount != 1 && this.arcGisClientViewer.ViewingLinkedMapIndex != 0;
        }

        #region Memo

        private RelayCommand addMemoCommand;

        public ICommand AddMemoCommand
        {
            get
            {
                if (this.addMemoCommand == null)
                {
                    this.addMemoCommand =
                        new RelayCommand(
                            param => this.AddMemo(),
                            param => this.CanAddMemo()
                        );
                }
                return this.addMemoCommand;
            }
        }

        private bool CanAddMemo()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.arcGisClientViewer.IsAbleToEditMap;
        }

        private void AddMemo()
        {
            this.arcGisClientViewer.MakeMemo();
        }

        #endregion

        // Data Playback

        #region StartPlaybackCommand

        private RelayCommand startPlaybackCommand;

        public ICommand StartPlaybackCommand
        {
            get
            {
                return this.startPlaybackCommand ??
                (this.startPlaybackCommand =
                 new RelayCommand(param => this.StartPlayback(), param => this.CanStartPlayback()));
            }
        }

        private bool CanStartPlayback()
        {
            if (this.arcGisClientViewer == null) return false;
            return this.arcGisClientViewer.IsAbleToEditMap;
        }

        private void StartPlayback()
        {
            this.arcGisClientViewer.TurnOnPlayBackMode();
            this.SetInterval(0, this.cameraList.Count > 0 ? 1 : 5, 0);
        }

        public void StartPlayback(DateTime seekTime, OperationPlaybackArgs operationPlayback = null)
        {
            this.arcGisClientViewer.TurnOnPlayBackMode(seekTime, operationPlayback);
        }

        #endregion // StartPlaybackCommand

        #region EndPlaybackCommand

        private RelayCommand endPlaybackCommand;

        public ICommand EndPlaybackCommand
        {
            get
            {
                return this.endPlaybackCommand ??
                (this.endPlaybackCommand =
                 new RelayCommand(param => this.EndPlayback(), param => this.CanEndPlayback()));
            }
        }

        private bool CanEndPlayback()
        {
            return true;
        }

        private void EndPlayback()
        {
            this.PausePlayback();
            this.arcGisClientViewer.TurnOffPlayBackMode();
        }

        #endregion // EndPlaybackCommand

        #region PlayPlaybackCommand

        private RelayCommand playPlaybackCommand;

        public ICommand PlayPlaybackCommand
        {
            get
            {
                return this.playPlaybackCommand ??
                (this.playPlaybackCommand =
                 new RelayCommand(param => this.PlayPlayback(), param => this.CanPlayPlayback()));
            }
        }

        private bool CanPlayPlayback()
        {
            if (this.arcGisClientViewer == null || this.arcGisClientViewer.IsDoingDataPlayBack)
            {
                return false;
            }

            return true;
        }

        private void PlayPlayback()
        {
            this.arcGisClientViewer.PlayData();
        }

        #endregion // PlayPlaybackCommand

        #region PausePlaybackCommand

        private RelayCommand pausePlaybackCommand;

        public ICommand PausePlaybackCommand
        {
            get
            {
                return this.pausePlaybackCommand ??
                (this.pausePlaybackCommand =
                 new RelayCommand(param => this.PausePlayback(), param => this.CanPausePlayback()));
            }
        }

        private bool CanPausePlayback()
        {
            if (this.arcGisClientViewer == null || !this.arcGisClientViewer.IsDoingDataPlayBack)
            {
                return false;
            }

            return true;
        }

        private void PausePlayback()
        {
            this.arcGisClientViewer.PauseData();
        }

        #endregion // PausePlaybackCommand

        #region MoveForwardPlaybackCommand

        private RelayCommand moveForwardPlaybackCommand;

        public ICommand MoveForwardPlaybackCommand
        {
            get
            {
                return this.moveForwardPlaybackCommand ??
                (this.moveForwardPlaybackCommand =
                 new RelayCommand(param => this.MoveForwardPlayback(), param => this.CanMoveForwardPlayback()));
            }
        }

        private bool CanMoveForwardPlayback()
        {
            if ((this.playbackCurrentDateTime + this.movePlaybackInterval) > DateTime.Now)
            {
                return false;
            }

            return true;
        }

        private void MoveForwardPlayback()
        {
            var seekTime = this.playbackCurrentDateTime + this.movePlaybackInterval;

            this.SeekPlayback(seekTime);
        }

        #endregion // MoveForwardPlaybackCommand

        #region MoveBackwardPlaybackCommand

        private RelayCommand moveBackwardPlaybackCommand;

        public ICommand MoveBackwardPlaybackCommand
        {
            get
            {
                return this.moveBackwardPlaybackCommand ??
                (this.moveBackwardPlaybackCommand =
                 new RelayCommand(param => this.MoveBackwardPlayback(), param => this.CanMoveBackwardPlayback()));
            }
        }

        private bool CanMoveBackwardPlayback()
        {
            
            return true;
        }

        private void MoveBackwardPlayback()
        {
            var seekTime = this.playbackCurrentDateTime - this.movePlaybackInterval;

            this.SeekPlayback(seekTime);
        }

        #endregion // MoveBackwardPlaybackCommand

        #endregion
    }
}
