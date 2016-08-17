using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.DataManager
{
    public class CameraGraphicDataManager
    {
        #region Field

        protected ObservableCollection<MapCameraObjectComponentDataInfo> objectDatas;

        public ObservableCollection<MapCameraObjectComponentDataInfo> CameraObjectComponentDatas
        {
            get { return this.objectDatas; }
        }

        #endregion //Field

        #region Construction

        public CameraGraphicDataManager()
        {
            this.objectDatas = new ObservableCollection<MapCameraObjectComponentDataInfo>();
        }

        #endregion //Construction 

        #region Methods

        #region Graphic 생성 수정 관련 Methods

        /// <summary>
        /// Object Data 하나 추가
        /// DATABASE에서 같은 종류의 DATA로 받아오는 겨우
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public IEnumerable<BaseGraphic> MakeOneObjectGraphics(MapCameraObjectComponentDataInfo data, bool isEditMode)
        {
            var cameraObjectComponentData = data;

            if (cameraObjectComponentData == null) return null;

            var addedGraphicList = new List<BaseGraphic>();

            cameraObjectComponentData.ObjectType = MapObjectType.Camera;

            if (string.IsNullOrEmpty(cameraObjectComponentData.ObjectID))
                cameraObjectComponentData.ObjectID = Guid.NewGuid().ToString();

            //ADD Preset ZONE
            var i = 0;
            foreach (var presetData in cameraObjectComponentData.PresetDatas)
            {
                var presetGraphic = this.MakeCameraPresetGraphic(presetData, cameraObjectComponentData.ObjectID, i, isEditMode);

                addedGraphicList.Add(presetGraphic);

                i++;
            }

            //Close path
            if (cameraObjectComponentData.Video.PointCollection.Count == 4)
            {
                cameraObjectComponentData.Video.PointCollection.Add(cameraObjectComponentData.Video.PointCollection[0]);
            }

            //ADD VIDEO
            var videoGraphic = new CameraVideoGraphic(cameraObjectComponentData.Video.PointCollection,
                                                      cameraObjectComponentData.ObjectID, cameraObjectComponentData.CameraInformationID);

            videoGraphic.ZIndexChanged += (s, e) =>
            {
                cameraObjectComponentData.Video.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            if (cameraObjectComponentData.Video.HiddenMaxLevel == 0 && cameraObjectComponentData.Video.HiddenMinLevel == 0 && cameraObjectComponentData.Video.ScaleMaxLevel == 0)
            {
                if (cameraObjectComponentData.Video.HiddenMaxLevel > cameraObjectComponentData.Video.DefaultHiddenMaxLevel ||
                    cameraObjectComponentData.Video.DefaultHiddenMinLevel >= cameraObjectComponentData.Video.HiddenMaxLevel)
                {
                    cameraObjectComponentData.Video.HiddenMaxLevel = (int)cameraObjectComponentData.Video.DefaultHiddenMaxLevel;
                }

                if (cameraObjectComponentData.Video.HiddenMinLevel < cameraObjectComponentData.Video.DefaultHiddenMinLevel ||
                    cameraObjectComponentData.Video.DefaultHiddenMaxLevel <= cameraObjectComponentData.Video.HiddenMinLevel)
                {
                    cameraObjectComponentData.Video.HiddenMinLevel = (int)cameraObjectComponentData.Video.DefaultHiddenMinLevel;
                }

                if (cameraObjectComponentData.Video.ScaleMaxLevel > cameraObjectComponentData.Video.HiddenMaxLevel ||
                    cameraObjectComponentData.Video.ScaleMaxLevel < cameraObjectComponentData.Video.HiddenMinLevel)
                    cameraObjectComponentData.Video.ScaleMaxLevel = cameraObjectComponentData.Video.HiddenMaxLevel;
            }

            videoGraphic.AlwaysKeepToCameraVideo = cameraObjectComponentData.Video.AlwaysKeepToCameraVideo.Value;

            addedGraphicList.Add(videoGraphic);

            var iconUri = isEditMode || cameraObjectComponentData.CameraIcon.IsIconVisible.GetValueOrDefault(true) ? cameraObjectComponentData.CameraIcon.IconUri : null;
            var iconSelectedUri = isEditMode || cameraObjectComponentData.CameraIcon.IsIconVisible.GetValueOrDefault(true) ? cameraObjectComponentData.CameraIcon.IconSelectedStringUri : null;

            //ADD Icon
            var iconGraphic = new CameraIconGraphic(cameraObjectComponentData.CameraIcon.Position,
                                              iconUri,
                                              iconSelectedUri,
                                              MapObjectType.CameraIcon,
                                              cameraObjectComponentData.ObjectID, (double)cameraObjectComponentData.CameraIcon.IconSize);

            iconGraphic.ZIndexChanged += (s, e) =>
            {
                cameraObjectComponentData.CameraIcon.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            addedGraphicList.Add(iconGraphic);

            //ADD Camera Label
            iconGraphic.CameraNameTextBoxGraphic
                = new CameraNameTextBoxGraphic(new Point(cameraObjectComponentData.CameraIcon.Position.X, 
                    cameraObjectComponentData.CameraIcon.Position.Y), MapObjectType.CameraNameTextBox,
                    cameraObjectComponentData.ObjectID, cameraObjectComponentData.Name, 
                    (double)cameraObjectComponentData.CameraIcon.LabelSize);

            if (cameraObjectComponentData.CameraIcon.IsVisibleLabel != null && cameraObjectComponentData.CameraIcon.IsVisibleLabel.Value) 
                addedGraphicList.Add(iconGraphic.CameraNameTextBoxGraphic);

            //ADD VIEWZONE PLUS
            if (isEditMode)
            {
                var currentImageSize = new Size(26, 26);
                var x = iconGraphic.OffsetPoint.X - (iconGraphic.ImagePixelSize.Width);
                var y = iconGraphic.OffsetPoint.Y + (currentImageSize.Height);

                var iconOffset = new Point(x, y);

                iconGraphic.ViwzonePlusButtonIcon = new IconGraphic(cameraObjectComponentData.CameraIcon.Position,
                                                        ArcGISConstSet.CameraPresetPlusNormalUri,
                                                        ArcGISConstSet.CameraPresetPlusSelectedUri,
                                                        MapObjectType.CameraPresetPlus,
                                                        cameraObjectComponentData.ObjectID, iconOffset);

                addedGraphicList.Add(iconGraphic.ViwzonePlusButtonIcon);
            }

            this.CameraObjectComponentDatas.Add(cameraObjectComponentData);

            cameraObjectComponentData.SetCameraComponentBounds();

            return addedGraphicList;
        }

        /// <summary>
        /// Preset 하나 추가 (Plus 버튼으로 추가 한다)
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <param name="currentResolution"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public CameraPresetGraphic MakeCameraPresetGraphic(MapCameraObjectComponentDataInfo cameraObjectComponentData, double currentResolution, bool isEditMode)
        {
            List<Point> pointCollection = null;

            var presetIndex = cameraObjectComponentData.PresetDatas.Count;
            if (presetIndex > 0)
            {
                var prePreset = cameraObjectComponentData.PresetDatas[presetIndex - 1];

                var maxPoint = new Point(prePreset.ExtentMax.X, prePreset.ExtentMax.Y);
                var minPoint = new Point(prePreset.ExtentMin.X, prePreset.ExtentMin.Y);

                var gapWidth = maxPoint.X - minPoint.X;

                pointCollection = new List<Point>
                                    {
                                        new Point(maxPoint.X, minPoint.Y),
                                        new Point(maxPoint.X + (gapWidth/2), maxPoint.Y),
                                        new Point(maxPoint.X + gapWidth, minPoint.Y)
                                    };
            }
            else
            {
                var point = cameraObjectComponentData.CameraIcon.Position;

                var presetOnSide = ArcGISConstSet.PresetOneSide * currentResolution;
                var mapPointsize = new Point(point.X + presetOnSide, y: point.Y - presetOnSide);

                pointCollection = new List<Point>
                                    {
                                        new Point(point.X + presetOnSide, mapPointsize.Y),
                                        new Point(point.X , point.Y),
                                        new Point(point.X - presetOnSide, mapPointsize.Y),
                                        new Point(point.X + presetOnSide, mapPointsize.Y),
                                    };
            }

            var currentPresetData = new MapCameraPresetObjectDataInfo()
            {
                PointCollection = pointCollection,
                BorderColorString = ArcGISConstSet.CameraPresetNormalColor.ToString(),
                FillColorString = ArcGISConstSet.CameraPresetNormalColor.ToString(),
                FillSelectedColorString =
                    ArcGISConstSet.CameraPresetNormalColor.ToString(),
                BorderSelectedColorString =
                    ArcGISConstSet.CameraPresetNormalColor.ToString()
            };


            var newPresetGraphic = this.MakeCameraPresetGraphic(currentPresetData, cameraObjectComponentData.ObjectID,
                                         cameraObjectComponentData.PresetDatas.Count, isEditMode);

            cameraObjectComponentData.PresetDatas.Add(currentPresetData);

            return newPresetGraphic;
        }

        /// <summary>
        /// Preset 만드는 공용 함수
        /// </summary>
        /// <param name="mapCameraPresetObjectDataInfo"></param>
        /// <param name="objectID"></param>
        /// <param name="index"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        private CameraPresetGraphic MakeCameraPresetGraphic(MapCameraPresetObjectDataInfo mapCameraPresetObjectDataInfo, string objectID, int index, bool isEditMode)
        {
            //Close path
            if(mapCameraPresetObjectDataInfo.PointCollection.Count == 3)
            {
                mapCameraPresetObjectDataInfo.PointCollection.Add(mapCameraPresetObjectDataInfo.PointCollection[0]);
            }

            var presetGraphic = new CameraPresetGraphic(mapCameraPresetObjectDataInfo.PointCollection,
                                                        mapCameraPresetObjectDataInfo.FillColor,
                                                        mapCameraPresetObjectDataInfo.BorderColor,
                                                        mapCameraPresetObjectDataInfo.FillSelectedColor,
                                                        mapCameraPresetObjectDataInfo.BorderSelectedColor,
                                                        MapObjectType.CameraPreset,
                                                        objectID, index);

            presetGraphic.ZIndexChanged += (s, e) =>
            {
                mapCameraPresetObjectDataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };
            
            return presetGraphic;
        }

        /// <summary>
        /// Object Data 하나 추가
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DeleteOneObject(MapCameraObjectComponentDataInfo cameraObjectComponentData)
        {
            bool Result = false;
            MapCameraObjectComponentDataInfo RemoveMapCameraObject = this.objectDatas.FirstOrDefault(p => p.ObjectType == cameraObjectComponentData.ObjectType && p.ObjectID == cameraObjectComponentData.ObjectID);
            if (RemoveMapCameraObject != null)
            {
                this.CameraObjectComponentDatas.Remove(RemoveMapCameraObject);
                Result = true;
            }

            //if (cameraObjectComponentData != null && this.objectDatas.Contains(cameraObjectComponentData)) this.CameraObjectComponentDatas.Remove(cameraObjectComponentData);
            return Result;
        }

        /// <summary>
        /// Camera 에 Preset 하나만 삭제
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <param name="cameraPresetGraphic"></param>
        public bool DeleteOneCameraPresetData(MapCameraObjectComponentDataInfo cameraObjectComponentData, CameraPresetGraphic cameraPresetGraphic)
        {
            if (cameraObjectComponentData == null || cameraPresetGraphic == null || cameraObjectComponentData.PresetDatas.Count < 0) return false;

            if(cameraObjectComponentData.PresetDatas.Count > cameraPresetGraphic.PresetIndex)
            {
                var cameraPresetData = cameraObjectComponentData.PresetDatas.ElementAt(cameraPresetGraphic.PresetIndex);

                if (cameraPresetData == null) return false;

                cameraObjectComponentData.PresetDatas.RemoveAt(cameraPresetGraphic.PresetIndex);
            }

            return true;
        }

        #endregion //Graphic 생성 수정 관련 Methods

        #region List Data 관련

        /// <summary>
        /// 같은 ID의 카메라가 List에 있는지 확인
        /// </summary>
        /// <param name="cameraInformationID"></param>
        public bool IsExistSameCameraInList(string cameraInformationID)
        {
            return this.objectDatas != null && this.objectDatas.Count(item => item.CameraInformationID == cameraInformationID) > 1;
        }

        /// <summary>
        /// CU Camera 정보를 Map Camera 정보로 변경 해준다.
        /// </summary>
        /// <param name="mapCameraSettingDataInfo"></param>
        /// <param name="resolution"></param>
        /// <param name="firstPoint"></param>
        /// <returns></returns>
        public MapCameraObjectComponentDataInfo GetNewCameraComponentObjectData(MapCameraSettingDataInfo mapCameraSettingDataInfo, double resolution, Point firstPoint)
        {
            var cameraObjectData = new MapCameraObjectComponentDataInfo()
            {
                Name = mapCameraSettingDataInfo.CameraName,
                CameraInformationID = mapCameraSettingDataInfo.CameraInformationId,
            };

            var streamSizeList = mapCameraSettingDataInfo.StreamSizeList;

            double firstX = 0;
            double firstY = 0;

            if (streamSizeList == null || !streamSizeList.Any())
            {
                streamSizeList = new List<Size> { new Size(4, 3) };
            }

            var camSize = streamSizeList[0].Height < streamSizeList[0].Width
                              ? new Size(mapCameraSettingDataInfo.StandSize,
                                         (double)mapCameraSettingDataInfo.StandSize *
                                         ((double)streamSizeList[0].Height / (double)streamSizeList[0].Width))
                              : new Size(
                                    (double)mapCameraSettingDataInfo.StandSize *
                                    ((double)streamSizeList[0].Width / (double)streamSizeList[0].Height),
                                    mapCameraSettingDataInfo.StandSize);

            //실제 카메라 사이즈를 ArcGis 버전으로 계산
            camSize = new Size(camSize.Width * resolution, camSize.Height * resolution);

            firstX = firstPoint.X;
            firstY = firstPoint.Y;

            //ICON
            cameraObjectData.CameraIcon = new MapCameraIconObjectDataInfo()
            {
                Position = new Point(firstX, firstY),
                IconUri = ArcGISConstSet.CameraIconNormalUri,
                IconSelectedStringUri = ArcGISConstSet.CameraIconSelectedUri,
                IconSize = ArcGISConstSet.CameraIconSize,
            };

            if (!cameraObjectData.CameraIcon.IsIconVisible.GetValueOrDefault(true))
            {
                cameraObjectData.CameraIcon.IconUri = ArcGISConstSet.CameraIconHideUri;
                cameraObjectData.CameraIcon.IconSelectedStringUri = ArcGISConstSet.CameraIconHideUri;
            }

            //VIEWZONES
            var presetOnSide = ArcGISConstSet.PresetOneSide * resolution;
            var mapPointsize = new Point(firstX + presetOnSide, y: firstY - presetOnSide);

            var pointCollection = new List<Point>
                                      {
                                          new Point(firstX + presetOnSide, mapPointsize.Y),
                                          new Point(firstX , firstY),
                                          new Point(firstX - presetOnSide, mapPointsize.Y)
                                      };

            cameraObjectData.PresetDatas.Add(new MapCameraPresetObjectDataInfo()
            {
                PointCollection = pointCollection,
                BorderColorString = ArcGISConstSet.CameraPresetNormalColor.ToString(),
                FillColorString = ArcGISConstSet.CameraPresetNormalColor.ToString(),
                FillSelectedColorString = ArcGISConstSet.CameraPresetSelectedColor.ToString(),
                BorderSelectedColorString = ArcGISConstSet.CameraPresetSelectedColor.ToString()
            });


            pointCollection = new List<Point>
                                      {
                                          new Point(firstX - presetOnSide, mapPointsize.Y - camSize.Height),
                                          new Point(firstX - presetOnSide, mapPointsize.Y),
                                          new Point(firstX - presetOnSide + camSize.Width, mapPointsize.Y),
                                          new Point(firstX - presetOnSide + camSize.Width, mapPointsize.Y - camSize.Height)
                                      };

            //VIDEO
            cameraObjectData.Video = new MapCameraVideoObjectDataInfo()
            {
                PointCollection = pointCollection,
                BorderColorString = ArcGISConstSet.CameraRectNormalColor.ToString(),
                FillColorString = ArcGISConstSet.CameraRectNormalColor.ToString(),
                FillSelectedColorString = ArcGISConstSet.CameraRectNormalColor.ToString(),
                BorderSelectedColorString = ArcGISConstSet.CameraRectNormalColor.ToString(),
                StreamSizeList = streamSizeList,
                MinSize = mapCameraSettingDataInfo.MinSize
            };

            return cameraObjectData;
        }

        /// <summary>
        /// 현재 선택된 정보 받아오기
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public MapCameraObjectComponentDataInfo GetObjectDataByObjectID(string objectID)
        {
            try
            {
                return this.objectDatas.FirstOrDefault(item => item.ObjectID == objectID);
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.Trace(e.ToString());
                return new MapCameraObjectComponentDataInfo();
            }
        }

        /// <summary>
        /// 해당 데이터가 List에 있는지 확인
        /// </summary>
        /// <param name="objectId"></param>
        public bool IsExistInList(string objectId)
        {
            return this.objectDatas != null && this.objectDatas.Any(item => item.ObjectID == objectId);
        }

        public void ClearObjectDatas()
        {
            if (this.objectDatas != null) this.CameraObjectComponentDatas.Clear();
        }

        #endregion //List 관련
        
        #endregion // Methods
    }
}
