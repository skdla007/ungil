using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControls.CommonData.Interface
{
    public interface IArcGISControlViewerAPI : IArcGISControlAPI
    {   
        /// <summary>
        /// Map Cell 이 현재 Activate 되어 있는지 확인 한다.
        /// </summary>
        /// <returns></returns>
        bool GetMapCellVisible();

        /// <summary>
        /// Preset 작동을 위해
        /// </summary>
        /// <param name="id"></param>
        /// <param name="presetNumber"></param>
        void ExcutePreset(string id, string presetNumber);

        /// <summary>
        /// Map Cell의 ZIndex를 받아온다.
        /// </summary>
        /// <returns></returns>
        int GetCellZIndex();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <param name="currentTime"></param>
        void AlertBroadCast(string cameraGuid, DateTime currentTime);

        /// <summary>
        /// Panomorph Control 의 Default view 로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewType"></param>
        void MovePanomorphCameraDefaultView(string objectId, string viewType);

        /// <summary>
        /// Panomorph Control 의 MouseDrag로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="panomorphMovedPoint"></param>
        /// <param name="isLeftButtonDown"></param>
        void MovePanomorphCameraMouseControl(string objectId, int viewNumber, Point panomorphMovedPoint, bool isLeftButtonDown);

        /// <summary>
        /// Panomorph Control 의 Point로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="panomorphMovedPoint"></param>
        void MovePanomorphCameraPoint(string objectId, int viewNumber, Point panomorphMovedPoint);

        /// <summary>
        /// Panomorph Control 의 Point로 이동
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewNumber"></param>
        /// <param name="selectionArea"></param>
        void MovePanomorphCameraRect(string objectId, int viewNumber, Rect selectionArea);

        /// <summary>
        /// Panomorph Control의 Type 변경
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="viewType"></param>
        void ChangePanomorphViewType(string objectId, string viewType);

        /// <summary>
        /// Panomorph Control Popup 카메라가 띄워질 경우
        /// </summary>
        void OpenPanomorphPopupCamera();

        /// <summary>
        /// Panomorph 상태를 받아온다
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <returns></returns>
        bool GetPanomorphState(string cameraGuid);

        /// <summary>
        /// Panomorph 모드를 저장한다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        void SetPanomorphControlMode(string unigridGuid);

        /// <summary>
        /// Panomorph 타입을 저장한다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <returns></returns>
        string GetPanomorphViewType(string unigridGuid);

        /// <summary>
        /// SearchView Control 띄우는 메시지 호출
        /// </summary>
        /// <param name="url"></param>
        void OpenSearchViewControl(string url);

        /// <summary>
        /// LinkzoneView Control 띄우는 메시지 호출
        /// </summary>
        /// <param name="url"></param>
        void OpenLinkzoneViewControl(string url);

        #region Playback 

        void StartPlaybackMode(string unigridGuid, string cameraId, DateTime seekTime, Rect displayRect, int? zIndex);

        void EndPlaybackMode(string unigridGuid);

        void PlayPlayback(string unigridGuid, double speed);

        void RewindPlayback(string unigridGuid, double speed);

        void PausePlayback(string unigridGuid);

        void SeekPlayback(string unigridGuid, DateTime seekTime);

        void PlayDataPlayback(DateTime seekTime);

        void SeekDataPlayback(DateTime seekTime);

        void PauseDataPlayback();

        #endregion //Palyback

        #region Get Viewer Log On User

        string GetUserID();

        #endregion
    }
}
