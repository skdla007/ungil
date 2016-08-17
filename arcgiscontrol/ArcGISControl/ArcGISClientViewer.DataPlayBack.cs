using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControl.GraphicObject;

namespace ArcGISControl
{
    public delegate void DataPlayBackTimerElapsed(DateTime timer);

    public partial class ArcGISClientViewer
    {
        #region Field

        public bool IsDataPlayBackMode { get; private set; }

        public bool IsDoingDataPlayBack { get; private set; }

        public DateTime DataPlayBackPosition { get; private set; }

        public OperationPlaybackArgs OperationPlaybackArgs { get; private set; }

        #endregion //Field

        #region Events

        public event DataPlayBackTimerElapsed dataPlayBackTimerElapsed;

        public event EventHandler eDataPlaybackModeChanged;

        public void OnEDataPlaybackModeChanged(EventArgs e)
        {
            var handler = eDataPlaybackModeChanged;
            if (handler != null) handler(this, e);
        }

        #endregion EVents

        #region Methods

        private void InitialIzedPlayBackDatas()
        {
            this.dataPlayBackTimer = new Timer();
            this.dataPlayBackTimer.AutoReset = true;
            this.dataPlayBackTimer.Interval = 1000;
            this.dataPlayBackTimer.Elapsed += DataPlayBackTimerOnElapsed;
        }

        private void ReleasePlayBackDatas()
        {
            this.IsDoingDataPlayBack = false;
            this.IsDataPlayBackMode = false;
            this.OperationPlaybackArgs = null;

            if (this.dataPlayBackTimer != null)
            {
                this.dataPlayBackTimer.Elapsed -= DataPlayBackTimerOnElapsed;
                this.dataPlayBackTimer = null;
            }

            this.ChangePlaybackToLive();

            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        public void TurnOnPlayBackMode()
        {
            this.TurnOnPlayBackMode(DateTime.Now - new TimeSpan(0, 5, 0));
        }

        public void TurnOnPlayBackMode(DateTime dataPlayBackPosition, OperationPlaybackArgs operationPlayback = null)
        {
            this.HideClickedPopups();

            if (this.dataPlayBackTimer == null)
            {
                this.InitialIzedPlayBackDatas();
            }

            this.IsDataPlayBackMode = true;
            this.DataPlayBackPosition = dataPlayBackPosition;
            this.ChangeLiveToPlayback();
            this.OperationPlaybackArgs = operationPlayback;

            this.universalObjectDataManager.ClearMapSearchResult();
            this.StartMapSplForPlayback();

            this.dataPlayBackTimerElapsed(this.DataPlayBackPosition);

            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        public void TurnOffPlayBackMode(bool isReloaded = false)
        {
            this.HideClickedPopups();

            if (this.dataPlayBackTimer == null) return;

            this.IsDataPlayBackMode = false;
            this.dataPlayBackTimer.Stop();
            this.OperationPlaybackArgs = null;
            this.ChangePlaybackToLive();
            this.universalObjectDataManager.ClearMapSearchResult();
            if (!isReloaded)
            {
                this.StartSplunkDatas();
            }
            this.StopMapSplForPlayback();
            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        public void PlayData()
        {
            this.HideClickedPopups();

            if (this.dataPlayBackTimer == null) return;

            this.IsDoingDataPlayBack = true;
            this.PlayDataPlaybackCamera();
            this.StartSplunkDatas();
            this.PlayMapSplForPlayback();
            this.dataPlayBackTimer.Start();

            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        public void PauseData()
        {
            this.HideClickedPopups();

            if (this.dataPlayBackTimer == null) return;

            this.IsDoingDataPlayBack = false;
            this.dataPlayBackTimer.Stop();
            this.PauseDataPlaybackCamera();
            this.StopSplunkDatas();
            this.PauseMapSplForPlayback();

            this.dataPlayBackTimerElapsed(this.DataPlayBackPosition);

            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        public void SetSeekTime(DateTime playTime)
        {
            this.HideClickedPopups();

            if (this.dataPlayBackTimer == null) return;

            this.dataPlayBackTimer.Stop();
            this.IsDoingDataPlayBack = false;
            this.DataPlayBackPosition = playTime;
            this.SeekDataPlaybackCamera(playTime);
            
            this.universalObjectDataManager.ClearMapSearchResult();
            this.StartSplunkDatas();
            this.SeekMapSplForPlayback();

            this.dataPlayBackTimerElapsed(this.DataPlayBackPosition);

            this.OnEDataPlaybackModeChanged(new EventArgs());
        }

        private void StartSplunkDatas()
        {
            var timeSpan = DateTime.Now - this.DataPlayBackPosition;

            this.StopSplunkDatas();
            
            this.ChangeAllSplunkServiceStatus();

            foreach (var workstationDataInfo in this.publicGraphicDataManager.ObjectDatas.OfType<MapWorkStationObjectDataInfo>())
            {
                var workStationGraphic = this.GetOneBaseGraphicInGraphicLayer(workstationDataInfo.ObjectID, workstationDataInfo.ObjectType) as LinkZoneGraphic;

                if (workStationGraphic != null)
                {
                    workStationGraphic.ChangeOriginalColor();
                }

                if (this.IsDataPlayBackMode)
                {
                    this.publicGraphicDataManager.StartSplunkService(workstationDataInfo.ObjectID,
                                                                             workstationDataInfo.SplunkBasicInformation,
                                                                             timeSpan, this.IsDoingDataPlayBack,
                                                                             this.OperationPlaybackArgs);

                }
                else
                {
                    this.publicGraphicDataManager.StartSplunkService(workstationDataInfo.ObjectID, workstationDataInfo.SplunkBasicInformation, !this.isEditMode);
                }
            }

            foreach (var linkZoneDataInfo in this.publicGraphicDataManager.ObjectDatas.OfType<MapLinkZoneObjectDataInfo>())
            {
                var linkZoneGraphic = this.GetOneBaseGraphicInGraphicLayer(linkZoneDataInfo.ObjectID, linkZoneDataInfo.ObjectType) as LinkZoneGraphic;

                if (linkZoneGraphic != null)
                {
                    linkZoneGraphic.ChangeOriginalColor();
                }

                if (this.IsDataPlayBackMode)
                {
                    this.publicGraphicDataManager.StartSplunkService(linkZoneDataInfo.ObjectID,
                                                                             linkZoneDataInfo.ColorSplunkBasicInformationData,
                                                                             timeSpan, this.IsDoingDataPlayBack,
                                                                             this.OperationPlaybackArgs);
                }
                else
                {
                    this.publicGraphicDataManager.StartSplunkService(linkZoneDataInfo.ObjectID,
                                                                             linkZoneDataInfo.ColorSplunkBasicInformationData,
                                                                             !this.isEditMode);
                }

            }
        }

        public void StopSplunkDatas()
        {
            this.savedSplunkObjectDataManager.StopAllSplunkServices();
            this.publicGraphicDataManager.StopAllSplunkServices();
        }

        // 전체 카메라를 플레이백으로 전환.
        private void ChangeLiveToPlayback()
        {
            this.cameraOverlayControl.ClearControlState();

            foreach (var graphic in this.objectGraphicLayer.Graphics)
            {
                if (graphic is CameraVideoGraphic)
                {
                    var cameraVideoGraphic = (CameraVideoGraphic)graphic;

                    var mapStartPoint = this.PointToScreenDIU(new Point(0, 0));

                    var Objectsize = new Size(cameraVideoGraphic.Geometry.Extent.Width / this.baseMap.Resolution,
                        cameraVideoGraphic.Geometry.Extent.Height / this.baseMap.Resolution);
                    var ObjectPoint = new Point((cameraVideoGraphic.Geometry.Extent.XMin - this.baseMap.Extent.XMin) / this.baseMap.Resolution,
                        (this.baseMap.Extent.YMax - cameraVideoGraphic.Geometry.Extent.YMax) / this.baseMap.Resolution);

                    var cameraGraphicRectByMonitor = new Rect(ObjectPoint.X + mapStartPoint.X,
                        ObjectPoint.Y + mapStartPoint.Y, Objectsize.Width, Objectsize.Height);

                    if (arcGisControlViewerApi == null) return;

                    this.arcGisControlViewerApi.StartPlaybackMode(cameraVideoGraphic.ObjectID,
                        cameraVideoGraphic.CameraInformationID,
                        this.DataPlayBackPosition,
                        ScreenUtil.DIUToPixels(cameraGraphicRectByMonitor),
                        cameraVideoGraphic.ZIndex);
                }
            }

            this.SeekDataPlaybackCamera(this.DataPlayBackPosition);
        }

        // 전체 카메라를 라이브로 전환
        private void ChangePlaybackToLive()
        {
            this.PauseDataPlaybackCamera();

            if (arcGisControlViewerApi == null) return;

            foreach (var cameraVideoGraphic in this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>())
            {
                this.arcGisControlViewerApi.EndPlaybackMode(cameraVideoGraphic.ObjectID);
            }

            this.RefreshAllCameraVideoRect();
        }

        private void PlayDataPlaybackCamera()
        {
            if (arcGisControlViewerApi == null) return;

            this.arcGisControlViewerApi.PlayDataPlayback(this.DataPlayBackPosition);
        }

        private void PauseDataPlaybackCamera()
        {
            if (arcGisControlViewerApi == null) return;

            this.arcGisControlViewerApi.PauseDataPlayback();
        }

        private void SeekDataPlaybackCamera(DateTime aSeekTime)
        {
            if (arcGisControlViewerApi == null) return;

            this.arcGisControlViewerApi.SeekDataPlayback(aSeekTime);
        }

        #endregion //Methods

        #region EventHandlers

        private void DataPlayBackTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!this.IsDataPlayBackMode && !this.IsDoingDataPlayBack) return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.dataPlayBackTimer == null) return;
                this.DataPlayBackPosition = this.DataPlayBackPosition.AddMilliseconds(this.dataPlayBackTimer.Interval);

                if (this.dataPlayBackTimerElapsed != null)
                    this.dataPlayBackTimerElapsed(this.DataPlayBackPosition);
            }));
        }

        #endregion //EventHandlers
    }
}
