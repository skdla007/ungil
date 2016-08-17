using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArcGISControl.GraphicObject;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.Bases
{
    /// <summary>
    /// CAMERA GRAPHIC 생성하는 Method들만 모아 놨음
    /// </summary>
    public partial class BaseArcGISMap 
    {
        #region Method

        /// <summary>
        /// Unigrid ID로 Base Camera List 받아 온다.
        /// Camera 관련 Graphic들은 같은 Unigrid ID로 되어 있다.
        /// Icon, ViewZone, VideoRect.....
        /// </summary>
        /// <param name="unigridID"></param>
        /// <returns></returns>
        protected List<BaseGraphic> GetOneCameraComponentGraphicsInGraphicLayer(string unigridID)
        {
            return this.objectGraphicLayer.Graphics.OfType<BaseGraphic>().Where(baseGraphic => baseGraphic.ObjectID == unigridID).ToList();
        }
        
        /// <summary>
        /// Camera Component 하나 추가
        /// </summary>
        /// <param name="cameraObjectComponentDataInfo"></param>
        /// <param name="TmpGraphicObjectID">Undo / Redo로 인해 제거 되었다 다시 생성되는 경우 처음 제거 되었던 ObjectID</param>
        /// <returns></returns>
        protected virtual IEnumerable<BaseGraphic> MakeCameraComponentObjectGraphic(MapCameraObjectComponentDataInfo cameraObjectComponentDataInfo, string TmpGraphicObjectID)
        {
            return this.cameraGraphicDataManager.MakeOneObjectGraphics(cameraObjectComponentDataInfo, this.isEditMode);
        }

        /// <summary>
        /// Camera Graphic 하나 추가
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="zIndex">일단 카메라는 100 으로</param>
        protected virtual void SetCameraGraphic(BaseGraphic graphic, int zIndex)
        {
            BaseMapObjectInfoData ObjectInfoData = this.GetObjectData(graphic.ObjectID, graphic.Type, !this.IsConsoleMode);
            graphic.ObjectInfoData = ObjectInfoData;

            this.objectGraphicLayer.Graphics.Add(graphic);

            if (zIndex == ArcGISConstSet.UndefinedZIndex)
            {
                zIndex = this.NextZIndex(ZLevel.L0);
            }

            graphic.ZIndex = zIndex;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        protected virtual void DeleteCameraGraphic(BaseGraphic graphic)
        {
            this.objectGraphicLayer.Graphics.Remove(graphic);
        }

        /// <summary>
        /// 카메라 하나 삭제 한다.
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <returns></returns>
        protected virtual void DeleteCameraObject(MapCameraObjectComponentDataInfo cameraObjectComponentData)
        {
            var cameraGraphicList = this.GetOneCameraComponentGraphicsInGraphicLayer(cameraObjectComponentData.ObjectID);

            foreach (var baseGraphic in cameraGraphicList)
            {
                this.DeleteCameraGraphic(baseGraphic);
            }
            
            this.cameraGraphicDataManager.DeleteOneObject(cameraObjectComponentData);
        }

        /// <summary>
        /// Camera Element 하나만 삭제
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <param name="selectedGraphic"></param>
        /// <returns></returns>
        virtual protected void DeleteCameraElement(MapCameraObjectComponentDataInfo cameraObjectComponentData, BaseGraphic selectedGraphic)
        {
            switch (selectedGraphic.Type)
            {
                case MapObjectType.CameraPreset:

                    var presetGraphic = selectedGraphic as CameraPresetGraphic;
                    
                    if(presetGraphic == null) return;

                    if (this.cameraGraphicDataManager.DeleteOneCameraPresetData(cameraObjectComponentData, presetGraphic))
                    {
                        var currentPresetList = this.objectGraphicLayer.Graphics.OfType<CameraPresetGraphic>().Where(g => g.ObjectID == presetGraphic.ObjectID);
                        
                        var cameraPresetGraphics = currentPresetList as CameraPresetGraphic[] ?? currentPresetList.ToArray();
                        
                        if(!cameraPresetGraphics.Any()) return;

                        for (int i = 0; i < cameraObjectComponentData.PresetDatas.Count; i++)
                        {
                            var remainedPresetGraphic = cameraPresetGraphics.ElementAt(i);
                            
                            if (remainedPresetGraphic == null) continue;
                            
                            remainedPresetGraphic.PresetIndex = i;
                        }
                    }

                    this.DeleteCameraGraphic(selectedGraphic);

                    break;
            }
        }

        protected virtual void ShowCameraPopupControl(CameraIconGraphic graphic)
        {   
            var objectData =
                    this.cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID);

            this.cameraPopupControlManager.Show(objectData, graphic);
        }

        protected virtual void MoveCameraPopupControl()
        {
            // 맵로드 시 Extent가 null이어서 죽는 경우가 발생하여 예외처리함.
            if (this.cameraPopupControlManager.IsShowCameraPoupupControl)
            {
                if (this.baseMap != null && this.baseMap.Extent != null)
                {
                    this.cameraPopupControlManager.Move(this.baseMap.Resolution, this.baseMap.Extent.XMin, this.baseMap.Extent.YMax);
                }
            }
        }

        protected virtual void HideCameraPopupControl()
        {
            this.cameraPopupControlManager.Hide();
        }

        #endregion //Method

    }
}
