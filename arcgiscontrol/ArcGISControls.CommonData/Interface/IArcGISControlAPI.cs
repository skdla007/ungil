using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.CommonData.Interface
{
    public interface IArcGISControlAPI
    {
        #region Camera Interface

        #region Camera 표출

        /// <summary>
        /// Camera 와 Video 생성
        /// </summary>
        /// <param name="camerId"></param>
        /// <param name="isNewCamera">새로운 카메라이면 카메라도 동시에 설정 : Viewer에서는 무조건 = false</param>
        /// <param name="showNameInCamera"></param>
        /// <returns></returns>
        string CreateCameraVideo(string camerId, bool isNewCamera, bool showNameInCamera = true);

        /// <summary>
        /// Camera 위치 이동 시 호출
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="camerId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="zIndex"></param>
        /// <param name="isPopupCamera"></param>
        void MoveCameraVideo(string unigridGuid, string camerId, double x, double y, double width, double height, int? zIndex = null, bool isPopupCamera = false);
        
        /// <summary>
        /// Camera 와 Unigrid 함께 지우기
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="camerGuid"></param>
        void EraseCameraVideo(string unigridGuid, string camerGuid);

        /// <summary>
        /// Unigrid를 화면에서 보이지 않게 한다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        void HideVideo(string unigridGuid);

        /// <summary>
        /// Canera 화면에서 보이게 함
        /// </summary>
        /// <param name="unigridGuid"></param>
        void ShowVideo(string unigridGuid);

        /// <summary>
        /// Unigrid를 화면에서 지운다
        /// Video를 화면에서 지운다.
        /// </summary>
        /// <param name="unigridGuid"></param>
        void EraseVideo(string unigridGuid);

        /// <summary>
        /// 해당 Camera의 Preset List를 받아 온다.
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <returns></returns>
        int GetPresetCount(string cameraGuid);

        /// <summary>
        /// Video Control에 Border 변경
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="isSelect"></param>
        void SetBorderCameraVideo(string unigridGuid, bool isSelect);

        /// <summary>
        /// Video control에 Z index 변경
        /// </summary>
        /// <param name="unigridGuid"></param>
        /// <param name="zindex"></param>
        void SetZIndexCameraVideo(string unigridGuid, int zindex);

        /// <summary>
        /// 카메라 Image Brush 받아온다
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <returns></returns>
        ImageSource GetImageBrush(int centerX, int centerY);

        /// <summary>
        /// Recording State
        /// </summary>
        /// <param name="cameraGuid"></param>
        /// <returns></returns>
        bool GetRecordingState(string cameraGuid);

        /// <summary>
        /// 카메라 Name을 받아온다.
        /// 생성당시와 View할 당시의 Camera의 이름이 달라질 수 있기 때문에
        /// </summary>
        /// <returns></returns>
        string GetCameraName(string cameraGuid);

        #endregion Camera 표출

        #region Rds

        bool GeRdsState(string cameraGuid);

        void StartRdsControl(string cameraGuid);

        void EndRdsControl(string cameraGuid);

        void SendRdsControlData(string cameraGuid, byte[] data);

        #endregion // Rds

        #endregion //Camera Interface

        #region Linked Map Object Interface

        /// <summary>
        /// LinkZone 세팅에서 사용하기 위해 Map List를 받아 온다.
        /// </summary>
        /// <returns></returns>
        List<MapSettingDataInfo> GetMapList();

        /// <summary>
        /// LinkZone 클릭 시 이동하기 위해 Map 정보를 받아온다.
        /// </summary>
        /// <param name="mapGuid"></param>
        /// <returns>MapSettingInfoData</returns>
        MapSettingDataInfo GetMapSetting(string mapGuid);

        /// <summary>
        /// LinkZone 클릭 시 이동하기 위해 Map 이름을 이용해 정보를 받아온다.
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns>MapSettingInfoData</returns>
        MapSettingDataInfo GetMapSettingByName(string mapName);

        #endregion //Linked Map Object Interface

        #region Splunk Data

        SplunkBasicInformationData SetMapSplunkData(SplunkBasicInformationData splunkBasicInformationData);

        #endregion //Splunk Data

        #region Setting 관련 

        List<SplunkBasicInformationData> GetSplunkInformationDataList();

        #endregion Setting 관련

        #region UI 제어

        /// <summary>
        /// Map 에서 Alert Message 일때 
        /// </summary>
        /// <param name="alertMessage"></param>
        /// <param name="onlyConfirm"></param>
        bool ShowMessagePopup(string alertMessage, bool onlyConfirm = false);

        #endregion 
    }
}
