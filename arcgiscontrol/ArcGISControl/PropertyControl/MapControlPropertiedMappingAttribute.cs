using System;
using System.Collections.Generic;
using System.Reflection;
using ArcGISControl.DataManager;
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using System.Linq;
using DataChangedNotify;

namespace ArcGISControl.PropertyControl
{
    public class MapControlPropertiedMappingAttribute : Attribute
    {
        private bool _IsUse = false;
        private MapObjectType[] _Objs = null;

        #region Constructor
        /// <summary>
        /// 모든 오브젝트 컨트롤에서 해당 속성이 사용된다.
        /// </summary>
        public MapControlPropertiedMappingAttribute() {}

        /// <summary>
        /// 각 오브젝트의 속성 설정
        /// 해당 속성이 어떤 오브젝트에 사용 되는지 설정한다.
        /// </summary>
        /// <param name="IsUse">특정 오브젝트에 속성이 사용되는지 여부 true : 지정된 오브젝트 컨트롤에서만 사용 / false : 지정된 오브젝트 컨트롤에서는 제외</param>
        /// <param name="pParams">특정 오브젝트 지정</param>
        public MapControlPropertiedMappingAttribute(bool IsUse, params MapObjectType[] MapObjectTypes)
        {
            _IsUse = IsUse;
            _Objs = MapObjectTypes;
        }
        #endregion

        #region Properties
        public bool IsUse
        {
            get { return _IsUse; }
        }

        public MapObjectType[] Objs
        {
            get { return _Objs; }
        }
        #endregion

        #region Method

        #region CreateCameraObjectDataInfo
        internal T CreateCameraObjectDataInfo<T>(List<BaseGraphic> selectedGraphicList, CameraGraphicDataManager cameraGraphicDataManager) where T : BaseModel
        {
            if (_Objs.Contains(MapObjectType.CameraPreset))
            {
                MapCameraPresetObjectDataInfo CameraPresetDataInfo = this.SetCameraViewZoneProperyValue(selectedGraphicList, cameraGraphicDataManager);

                if (CameraPresetDataInfo != null)
                {
                    return (T)Convert.ChangeType(CameraPresetDataInfo, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            else if (_Objs.Contains(MapObjectType.CameraVideo))
            {
                MapCameraVideoObjectDataInfo CameraVideoObjectDataInfo = this.SetCameraVideoProperyValue(selectedGraphicList, cameraGraphicDataManager);

                if (CameraVideoObjectDataInfo != null)
                {
                    return (T)Convert.ChangeType(CameraVideoObjectDataInfo, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            else if (_Objs.Contains(MapObjectType.CameraIcon))
            {
                MapCameraIconObjectDataInfo CameraIconObjectDataInfo = this.SetCameraIconProperyValue(selectedGraphicList, cameraGraphicDataManager);

                if (CameraIconObjectDataInfo != null)
                {
                    return (T)Convert.ChangeType(CameraIconObjectDataInfo, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        private MapCameraPresetObjectDataInfo SetCameraViewZoneProperyValue(List<BaseGraphic> selectedGraphicList, CameraGraphicDataManager cameraGraphicDataManager)
        {
            var graphic = selectedGraphicList.Last();
            var presetGraphic = graphic as CameraPresetGraphic;

            if (presetGraphic != null)
            {
                try
                {
                    var cameraObjectComponentDataInfo = cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);
                    var presetData = new MapCameraPresetObjectDataInfo(cameraObjectComponentDataInfo.PresetDatas[presetGraphic.PresetIndex]);

                    //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
                    if (selectedGraphicList.Count > 1)
                    {
                        var borderColorString =
                            cameraObjectComponentDataInfo.PresetDatas[presetGraphic.PresetIndex].BorderColorString;

                        var sameColorCnt = selectedGraphicList.OfType<PolygonGraphic>().Count(g => g.BorderColor.ToString() == borderColorString);

                        if (sameColorCnt != selectedGraphicList.Count)
                        {
                            borderColorString = borderColorString.Remove(3);
                            borderColorString += "ffffff";
                        }

                        var fillColorString =
                            cameraObjectComponentDataInfo.PresetDatas[
                                presetGraphic.PresetIndex].FillColorString;

                        sameColorCnt = selectedGraphicList.OfType<PolygonGraphic>().Count(g => g.NormalColor.ToString() == fillColorString);

                        if (sameColorCnt != selectedGraphicList.Count)
                        {
                            fillColorString = fillColorString.Remove(3);
                            fillColorString += "ffffff";
                        }

                        presetData = new MapCameraPresetObjectDataInfo()
                        {
                            BorderColorString = borderColorString,
                            FillColorString = fillColorString
                        };
                    }

                    return presetData;
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    InnowatchDebug.Logger.Trace(exception.ToString());
                }
            }

            return null;
        }

        private MapCameraVideoObjectDataInfo SetCameraVideoProperyValue(List<BaseGraphic> selectedGraphicList, CameraGraphicDataManager cameraGraphicDataManager)
        {
            var graphic = selectedGraphicList.Last();
            var cameraObjectComponentDataInfo = cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);

            if (cameraObjectComponentDataInfo == null) return null;

            var videoData = new MapCameraVideoObjectDataInfo(cameraObjectComponentDataInfo.Video);


            //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
            if (selectedGraphicList.Count > 1)
            {
                var isLockSize = cameraObjectComponentDataInfo.Video.IsLockSize;
                var constrainProportion = cameraObjectComponentDataInfo.Video.ConstrainProportion;
                var alwaysKeepToCameraVideo = cameraObjectComponentDataInfo.Video.AlwaysKeepToCameraVideo;
                var hiddenMinLevel = cameraObjectComponentDataInfo.Video.HiddenMinLevel;
                var hiddenMaxLevel = cameraObjectComponentDataInfo.Video.HiddenMaxLevel;

                var isLockSizeCnt = 0;
                var constrainProportionCnt = 0;
                var alwaysKeepToCameraVideoCnt = 0;
                var hiddenMinLevelCnt = 0;
                var hiddenMaxLevelCnt = 0;

                foreach (var selectedGraphic in selectedGraphicList)
                {
                    var selectedCameraObjectComponent
                        = cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID)
                                            as MapCameraObjectComponentDataInfo;

                    if (selectedCameraObjectComponent == null) continue;

                    isLockSizeCnt = selectedCameraObjectComponent.Video.IsLockSize == isLockSize
                                  ? isLockSizeCnt + 1
                                  : isLockSizeCnt;

                    constrainProportionCnt = selectedCameraObjectComponent.Video.ConstrainProportion == constrainProportion
                                  ? constrainProportionCnt + 1
                                  : constrainProportionCnt;

                    alwaysKeepToCameraVideoCnt = selectedCameraObjectComponent.Video.AlwaysKeepToCameraVideo == alwaysKeepToCameraVideo
                                  ? alwaysKeepToCameraVideoCnt + 1
                                  : alwaysKeepToCameraVideoCnt;

                    hiddenMinLevelCnt = selectedCameraObjectComponent.Video.HiddenMinLevel == hiddenMinLevel
                                  ? hiddenMinLevelCnt + 1
                                  : hiddenMinLevelCnt;

                    hiddenMaxLevelCnt = selectedCameraObjectComponent.Video.HiddenMaxLevel == hiddenMaxLevel
                                  ? hiddenMaxLevelCnt + 1
                                  : hiddenMaxLevelCnt;
                }

                if (isLockSizeCnt != selectedGraphicList.Count) isLockSize = null;
                if (constrainProportionCnt != selectedGraphicList.Count) constrainProportion = null;
                if (alwaysKeepToCameraVideoCnt != selectedGraphicList.Count) alwaysKeepToCameraVideo = null;
                if (hiddenMinLevelCnt != selectedGraphicList.Count) hiddenMinLevel = -1;
                if (hiddenMaxLevelCnt != selectedGraphicList.Count) hiddenMaxLevel = -1;

                videoData = new MapCameraVideoObjectDataInfo()
                {
                    IsLockSize = isLockSize,
                    ConstrainProportion = constrainProportion,
                    AlwaysKeepToCameraVideo = alwaysKeepToCameraVideo,
                    HiddenMaxLevel = hiddenMaxLevel,
                    HiddenMinLevel = hiddenMinLevel
                };
            }

            return videoData;
        }

        private MapCameraIconObjectDataInfo SetCameraIconProperyValue(List<BaseGraphic> selectedGraphicList, CameraGraphicDataManager cameraGraphicDataManager)
        {
            var graphic = selectedGraphicList.Last();
            var cameraObjectComponentDataInfo = cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);

            if (cameraObjectComponentDataInfo == null) return null;

            var cameraIconData =
                                new MapCameraIconObjectDataInfo(cameraObjectComponentDataInfo.CameraIcon);

            //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
            if (selectedGraphicList.Count > 1)
            {
                var isVisibleLabel = cameraObjectComponentDataInfo.CameraIcon.IsVisibleLabel;
                var iconSize = cameraObjectComponentDataInfo.CameraIcon.IconSize;
                var isIconVisible = cameraObjectComponentDataInfo.CameraIcon.IsIconVisible;

                var labelCnt = 0;
                var iconSizeCnt = 0;
                var isIconVisibleCnt = 0;

                foreach (var selectedGraphic in selectedGraphicList)
                {
                    var selectedCameraObjectComponent =
                        cameraGraphicDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                    if (selectedCameraObjectComponent != null)
                    {
                        labelCnt = selectedCameraObjectComponent.CameraIcon.IsVisibleLabel == isVisibleLabel
                                      ? labelCnt + 1
                                      : labelCnt;

                        iconSizeCnt = selectedCameraObjectComponent.CameraIcon.IconSize == iconSize
                                      ? iconSizeCnt + 1
                                      : iconSizeCnt;

                        isIconVisibleCnt = selectedCameraObjectComponent.CameraIcon.IsIconVisible == isIconVisible
                                      ? isIconVisibleCnt + 1
                                      : isIconVisibleCnt;
                    }
                }

                if (iconSizeCnt != selectedGraphicList.Count) iconSize = ArcGISConstSet.CameraIconSize;
                if (labelCnt != selectedGraphicList.Count) isVisibleLabel = null;
                if (isIconVisibleCnt != selectedGraphicList.Count) isIconVisible = null;

                cameraIconData = new MapCameraIconObjectDataInfo()
                {
                    IsVisibleLabel = isVisibleLabel,
                    IconSize = iconSize,
                    IsIconVisible = isIconVisible
                };
            }

            return cameraIconData;
        }
        #endregion

        #region CreateSplunkObjectDataInfo
        internal MapSplunkObjectDataInfo SetSplunkProperyValue(List<BaseGraphic> selectedGraphicList, SplunkObjectDataManager savedSplunkObjectDataManager)
        {
            var graphic = selectedGraphicList.Last();
            var propertyData = savedSplunkObjectDataManager.GetObjectDataByObjectID(graphic.ObjectID);

            if (propertyData == null)
            {
                return null;
            }

            var splunkDataInfo = new MapSplunkObjectDataInfo(propertyData);

            var allChart = !SplunkBasicInformationData.IsTableDataType(splunkDataInfo.SplunkBasicInformation.DataExpressType);

            //여러개 선택 되었을 경우 선택 값의 동일 여부 확인
            if (selectedGraphicList.Count > 1)
            {
                var hiddenMinLevel = splunkDataInfo.HiddenMinLevel;
                var hiddenMaxLevel = splunkDataInfo.HiddenMaxLevel;
                var title = splunkDataInfo.Title;
                var axisX = splunkDataInfo.ChartAxisXTitle;
                var axisY = splunkDataInfo.ChartAxisYTitle;
                var useSchedule = splunkDataInfo.UseSchedule;
                var intervalSeconds = splunkDataInfo.IntervalSeconds;
                var intervalUnitType = splunkDataInfo.IntervalUnitType;
                var isIconHidden = splunkDataInfo.IsIconHidden;
                var linkUrl = splunkDataInfo.LinkUrl;

                var hiddenMinLevelCnt = 0;
                var hiddenMaxLevelCnt = 0;
                var titlesAreAllSame = true;
                var axisXAreAllSame = true;
                var axisYAreAllSame = true;
                var useScheduleSame = true;
                var intervalSecondsSame = true;
                var intervalUnitTypeSame = true;
                var isIconHiddenSame = true;
                var linkUrlSame = true;

                foreach (var selectedGraphic in selectedGraphicList)
                {
                    var selectedSplunkObject
                        = savedSplunkObjectDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID);

                    if (selectedSplunkObject == null) continue;

                    if (allChart && SplunkBasicInformationData.IsTableDataType(selectedSplunkObject.SplunkBasicInformation.DataExpressType))
                    {
                        allChart = false;
                    }

                    hiddenMinLevelCnt = selectedSplunkObject.HiddenMinLevel == hiddenMinLevel
                                  ? hiddenMinLevelCnt + 1
                                  : hiddenMinLevelCnt;

                    hiddenMaxLevelCnt = selectedSplunkObject.HiddenMaxLevel == hiddenMaxLevel
                                  ? hiddenMaxLevelCnt + 1
                                  : hiddenMaxLevelCnt;

                    if (titlesAreAllSame && selectedSplunkObject.Title != title)
                        titlesAreAllSame = false;

                    if (axisXAreAllSame && selectedSplunkObject.ChartAxisXTitle != axisX)
                        axisXAreAllSame = false;

                    if (axisYAreAllSame && selectedSplunkObject.ChartAxisYTitle != axisY)
                        axisYAreAllSame = false;

                    if (useScheduleSame && useSchedule != selectedSplunkObject.UseSchedule)
                    {
                        useScheduleSame = false;
                    }

                    if (intervalUnitTypeSame && intervalUnitType != selectedSplunkObject.IntervalUnitType)
                    {
                        intervalUnitTypeSame = false;
                        intervalSecondsSame = false;
                    }

                    if (intervalSecondsSame && intervalSeconds != selectedSplunkObject.IntervalSeconds)
                    {
                        intervalSecondsSame = false;
                    }

                    if (linkUrlSame && linkUrl.Equals(selectedSplunkObject.LinkUrl))
                    {
                        linkUrlSame = false;
                    }

                    if (isIconHiddenSame && isIconHidden != selectedSplunkObject.IsIconHidden)
                    {
                        isIconHiddenSame = false;
                    }
                }

                if (hiddenMinLevelCnt != selectedGraphicList.Count) hiddenMinLevel = -1;
                if (hiddenMaxLevelCnt != selectedGraphicList.Count) hiddenMaxLevel = -1;
                if (!titlesAreAllSame) title = "-";
                if (!axisXAreAllSame) axisX = "-";
                if (!axisYAreAllSame) axisY = "-";
                if (!useScheduleSame) useSchedule = null;
                if (!intervalUnitTypeSame) intervalUnitType = IntervalUnitType.None;
                if (!intervalSecondsSame) intervalSeconds = null;
                if (!isIconHiddenSame) isIconHidden = null;
                if (!linkUrlSame) linkUrl = string.Empty;

                splunkDataInfo = new MapSplunkObjectDataInfo()
                {
                    Name = "",
                    Title = title,
                    ChartAxisXTitle = axisX,
                    ChartAxisYTitle = axisY,
                    HiddenMaxLevel = hiddenMaxLevel,
                    HiddenMinLevel = hiddenMinLevel,
                    UseSchedule = useSchedule,
                    IntervalUnitType = intervalUnitType,
                    IntervalSeconds = intervalSeconds,
                    IsIconHidden = isIconHidden,
                    LinkUrl = linkUrl
                };
            }

            return splunkDataInfo;
        }
        #endregion

        #endregion
    }

    public static class MapControlPropertiedMappingAttributeHelper
    {
        public static MapControlPropertiedMappingAttribute GetMapMapControlPropertiedMappinggAttribute(this Enum pEnum, int AttIndex)
        {
            Type tType = pEnum.GetType();
            FieldInfo FI = tType.GetField(pEnum.ToString());
            object[] ObjCustomAttributes = FI.GetCustomAttributes(false);
            if (ObjCustomAttributes.Length > 0)
            {
                MapControlPropertiedMappingAttribute tObj = ObjCustomAttributes[AttIndex] as MapControlPropertiedMappingAttribute;
                return tObj;
            }
            else
            {
                return null;
            }
        }
    }
}
