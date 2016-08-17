using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Polygon = ESRI.ArcGIS.Client.Geometry.Polygon;
using VerticalAlignment = System.Windows.VerticalAlignment;
using ArcGISControl.Command;

namespace ArcGISControl
{
    public partial class ArcGISClientViewer
    {   
        #region Methods

        #region Camera Object Methods

        /// <summary>
        /// Camera Component 하나 추가
        /// </summary>
        /// <param name="cameraObjectComponentDataInfo"></param>
        /// <param name="TmpGraphicObjectID">Undo / Redo로 인해 제거 되었다 다시 생성되는 경우 처음 제거 되었던 ObjectID</param>
        /// <returns></returns>
        protected override IEnumerable<BaseGraphic> MakeCameraComponentObjectGraphic(MapCameraObjectComponentDataInfo cameraObjectComponentDataInfo, string TmpGraphicObjectID)
        {
            var isNewCamera = true;
            var haveRight = true;

            if (this.cameraGraphicDataManager.IsExistInList(cameraObjectComponentDataInfo.ObjectID))
            {
                isNewCamera = false;
            }

            //외부(비디오 컨트롤 부분)에서 카메라 unigrid ID 설정 
            if (this.arcGISControlApi != null)
            {
                cameraObjectComponentDataInfo.ObjectID = this.arcGISControlApi.CreateCameraVideo(cameraObjectComponentDataInfo.CameraInformationID, isNewCamera, false);
                var newName = this.arcGISControlApi.GetCameraName(cameraObjectComponentDataInfo.CameraInformationID);
                if(!string.IsNullOrEmpty(newName))cameraObjectComponentDataInfo.Name = newName;
            }

            //카메라 unigrid ID 설정 
            if (string.IsNullOrEmpty(cameraObjectComponentDataInfo.ObjectID))
            {
                haveRight = false;
                cameraObjectComponentDataInfo.ObjectID = Guid.NewGuid().ToString();
            }

            var cameraGraphicList = base.MakeCameraComponentObjectGraphic(cameraObjectComponentDataInfo, TmpGraphicObjectID);
            if (cameraGraphicList == null)
                return null;

            //외부(프리셋 제어 부분)에서 Preset정보를 받아 온다.
            if (this.arcGISControlApi != null)
            {
                cameraObjectComponentDataInfo.PresetCount = this.arcGISControlApi.GetPresetCount(cameraObjectComponentDataInfo.CameraInformationID);
            }

            this.ReleaseAllSelectedObject();

            foreach (var baseGraphic in cameraGraphicList.ToList())
            {
                if(cameraObjectComponentDataInfo.IsUndoManage) baseGraphic.FirstObjectID = TmpGraphicObjectID;
                int zIndex;
                switch (baseGraphic.Type)
                {
                    case MapObjectType.CameraPreset:
                        {
                            var index = ((CameraPresetGraphic)baseGraphic).PresetIndex;
                            zIndex = cameraObjectComponentDataInfo.PresetDatas[index].ObjectZIndex;
                        }
                        break;
                    case MapObjectType.CameraVideo:
                        zIndex = cameraObjectComponentDataInfo.Video.ObjectZIndex;
                        break;
                    default:
                        zIndex = cameraObjectComponentDataInfo.CameraIcon.ObjectZIndex;
                        break;
                }
                this.SetCameraGraphic(baseGraphic, zIndex);
            }

            var cameraVideoGraphic = this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>().FirstOrDefault(baseGraphic => baseGraphic.ObjectID == cameraObjectComponentDataInfo.ObjectID);

            if (cameraVideoGraphic != null)
            {
                cameraVideoGraphic.CanView = haveRight;
                this.RefreshGraphicObjectVideo(cameraVideoGraphic);
                this.ChangeCameraVideoVisible(cameraObjectComponentDataInfo, cameraVideoGraphic);
            }

            return cameraGraphicList;
        }

        /// <summary>
        /// Camera Graphic 하나 추가
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="insertIndex"></param>
        protected override void SetCameraGraphic(BaseGraphic graphic, int insertIndex)
        {
            base.SetCameraGraphic(graphic, insertIndex);

            graphic.MouseLeftButtonDown += CameraGraphicOnMouseLeftButtonDown;
            graphic.MouseLeftButtonUp += CameraGraphicOnMouseLeftButtonUp;
            graphic.MouseRightButtonUp += CameraGraphicOnMouseRightButtonUp;
            //graphic.PropertyChanged += CameraGraphicOnPropertyChanged;
            graphic.MouseEnter += CameraGraphicOnMouseEnter;
            graphic.MouseMove += CameraGraphicOnMouseMove;
            graphic.MouseLeave += CameraGraphicOnMouseLeave;

            if(graphic is CameraVideoGraphic)
            {
                graphic.ZIndexChanged += GraphicOnZIndexChanged;
            }
        }

        /// <summary>
        /// CameraPresetPlus Button으로 Preset 추가 
        /// </summary>
        /// <param name="unigridGuid"></param>
        private void AddCameraPresetGraphic(string unigridGuid)
        {
            var cameraObjectComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(unigridGuid) as MapCameraObjectComponentDataInfo;

            if (cameraObjectComponentData == null)
                return;

            var presetGraphic = this.cameraGraphicDataManager.MakeCameraPresetGraphic(cameraObjectComponentData, this.baseMap.Resolution, this.isEditMode);

            this.SetCameraGraphic(presetGraphic, cameraObjectComponentData.PresetDatas[presetGraphic.PresetIndex].ObjectZIndex);

            this.SelectGraphicObject(presetGraphic);
        }

        /// <summary>
        /// 카메라 하나 삭제 한다.
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <returns></returns>
        protected override void DeleteCameraObject(MapCameraObjectComponentDataInfo cameraObjectComponentData)
        {
            if (cameraObjectComponentData == null) return;

            // EraseCameraVideo 실행 
            if (this.arcGISControlApi != null)
            {
                //같은 카메라가 있을 경우 Unigrid만 지우고 마지막 카메라 이면 Unigrid 와 Video도 함께 지운다
                if (this.cameraGraphicDataManager.IsExistSameCameraInList(cameraObjectComponentData.CameraInformationID))
                    this.arcGISControlApi.EraseVideo(cameraObjectComponentData.ObjectID);
                else
                    this.arcGISControlApi.EraseCameraVideo(cameraObjectComponentData.ObjectID, cameraObjectComponentData.CameraInformationID);
            }

            base.DeleteCameraObject(cameraObjectComponentData);
        }

        /// <summary>
        /// Camera Element 하나만 삭제
        /// </summary>
        /// <param name="cameraObjectComponentData"></param>
        /// <param name="selectedGraphic"></param>
        /// <returns></returns>
        override protected void DeleteCameraElement(MapCameraObjectComponentDataInfo cameraObjectComponentData, BaseGraphic selectedGraphic)
        {
            if (selectedGraphic != null) base.DeleteCameraElement(cameraObjectComponentData, selectedGraphic);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        protected override void DeleteCameraGraphic(BaseGraphic graphic)
        {
            if (this.editor != null) this.editor.Stop();

            base.DeleteCameraGraphic(graphic);

            if (!this.objectGraphicLayer.Contains(graphic))
            {
                graphic.MouseLeftButtonDown -= CameraGraphicOnMouseLeftButtonDown;
                graphic.MouseLeftButtonUp -= CameraGraphicOnMouseLeftButtonUp;
                graphic.MouseRightButtonUp -= CameraGraphicOnMouseRightButtonUp;
                graphic.MouseMove -= CameraGraphicOnMouseMove;
                graphic.MouseLeave -= CameraGraphicOnMouseLeave;

            }

            this.DeleteVertexSelectedGraphic(graphic as IPointCollectionOwner);
        }

        /// <summary>
        /// 모든 Camera Rect를 Refhresh 
        /// </summary>
        public void RefreshAllCameraVideoRect()
        {
            if (this.objectGraphicLayer == null)
                return;

            int graphicIndex = 0;

            var cameraVideoGraphics = this.objectGraphicLayer.Graphics.OfType<CameraVideoGraphic>().ToArray();
            
            foreach (var cameraVideoGraphic in cameraVideoGraphics)
            {
                if (cameraVideoGraphic != null)
                {
                    this.RefreshGraphicObjectVideo(cameraVideoGraphic);
                }

                graphicIndex++;
            }

            this.RefreshCameraPopupControlVideo();
        }

        /// <summary>
        /// Property 체크하여 
        /// 현재의 Camera정보를 표현한다.
        /// </summary>
        protected void ChangeCameraVideoVisible()
        {
            foreach (var cameraObjectCompoentData in this.cameraGraphicDataManager.CameraObjectComponentDatas)
            {
                var cameraVideoGraphic = this.GetOneBaseGraphicInGraphicLayer(cameraObjectCompoentData.ObjectID, MapObjectType.CameraVideo) as CameraVideoGraphic;
                if (cameraVideoGraphic != null) this.ChangeCameraVideoVisible(cameraObjectCompoentData, cameraVideoGraphic);
            }
        }

        /// <summary>
        /// Property 체크하여 
        /// 현재의 Camera정보를 표현한다.
        /// </summary>
        /// <param name="cameraObjectCompoent"></param>
        /// <param name="cameraVideoGraphic"></param>
        private void ChangeCameraVideoVisible(MapCameraObjectComponentDataInfo cameraObjectCompoent, CameraVideoGraphic cameraVideoGraphic)
        {
            if (cameraVideoGraphic == null)
                return;

            var cameraPresetGraphics = this.objectGraphicLayer.Graphics.OfType<CameraPresetGraphic>().Where(graphic => graphic.ObjectID == cameraObjectCompoent.ObjectID).ToArray();

            //HIDE
            if ((cameraObjectCompoent.Video.HiddenMinLevel > 0 && cameraObjectCompoent.Video.HiddenMinLevel > Math.Round(mapLevel)) ||
                  (cameraObjectCompoent.Video.HiddenMaxLevel > 0 && cameraObjectCompoent.Video.HiddenMaxLevel < Math.Round(mapLevel)) ||
                    this.isHideAllCamera)
            {
                if (cameraVideoGraphic.ShowVideo)
                {
                    if (this.IsConsoleMode) cameraVideoGraphic.IsVisible = false;
                    cameraVideoGraphic.ShowVideo = false;

                    foreach (var cameraPresetGraphic in cameraPresetGraphics)
                    {
                        if (cameraPresetGraphic == null) continue;

                        cameraPresetGraphic.HideGraphic();

                        if(this.editor != null)
                        {
                            this.editor.Stop();
                        }
                    }

                    if (cameraVideoGraphic.SelectFlag)
                    {
                        this.UnSelectGraphicObject(cameraVideoGraphic);
                    }

                    this.cameraOverlayControl.Visibility = Visibility.Collapsed;
                }

            }//END HIDE
            else // SHOW MAP
            {
                if (!cameraVideoGraphic.ShowVideo)
                {
                    cameraVideoGraphic.IsVisible = true;
                    cameraVideoGraphic.ShowVideo = true;

                    foreach (var cameraPresetGraphic in cameraPresetGraphics)
                    {
                        if (cameraPresetGraphic != null) cameraPresetGraphic.ShowGraphic();
                    }

                    if (cameraVideoGraphic.SelectFlag)
                    {
                        this.UnSelectGraphicObject(cameraVideoGraphic);
                    }
                }
            }

            this.RefreshGraphicObjectVideo(cameraVideoGraphic);
        }

        #endregion Camera Object Moethods

        #region Video Control 통신을 위한 Methods

        /// <summary>
        /// PopupCameraControl
        /// </summary>
        private void RefreshCameraPopupControlVideo(Point? mapStartPoint = null)
        {
            if (this.arcGISControlApi == null || this.cameraPopupControlManager == null) return;

            if (string.IsNullOrEmpty(this.cameraPopupControlManager.CameraUnigridId)) return;

            var isMapVisible = (arcGisControlViewerApi == null || (arcGisControlViewerApi != null && arcGisControlViewerApi.GetMapCellVisible()));

            var placedBrushes = false;

            if (PresentationSource.FromVisual(this) != null)
            {
                if (mapStartPoint == null) mapStartPoint = this.PointToScreenDIU(new Point(0, 0));

                var cameraUnigridGuid = this.cameraPopupControlManager.CameraUnigridId;
                var cameraObjectRect = this.cameraPopupControlManager.GetTotalVideoRect(mapStartPoint.Value);
                var cameraObjectRectInPixels = ScreenUtil.DIUToPixels(cameraObjectRect);
                
                if (this.cameraPopupControlManager.CanView && isMapVisible)
                {
                    // SHOW
                    this.arcGISControlApi.MoveCameraVideo(
                        cameraUnigridGuid,
                        this.cameraPopupControlManager.CameraInformationId,
                        cameraObjectRectInPixels.X,
                        cameraObjectRectInPixels.Y,
                        cameraObjectRectInPixels.Width,
                        cameraObjectRectInPixels.Height,
                        int.MaxValue,
                        true
                    );
                }
                else
                {
                    // HIDE
                    this.arcGISControlApi.MoveCameraVideo(
                        cameraUnigridGuid,
                        this.cameraPopupControlManager.CameraInformationId,
                        cameraObjectRectInPixels.X,
                        cameraObjectRectInPixels.Y,
                        0,
                        0,
                        int.MaxValue,
                        true
                    );
                }

                var addRect = ScreenUtil.GetTotalRectanglesByMonitor(cameraObjectRectInPixels);

                this.cameraPopupControlManager.ControlVideoImageRect(addRect.Count);

                int dupicatedIndex = 0;
                placedBrushes = true;
                foreach (Tuple<Rect, Rect> tupleRect in addRect)
                {
                    var xImageRect = this.cameraPopupControlManager.GetImageRect(dupicatedIndex);

                    var xDxImageBrush = this.imageBrushGraphicManager.GetImageBrush(tupleRect);

                    if (xDxImageBrush != null && xImageRect != null)
                    {
                        if (this.cameraPopupControlManager.CanView && isMapVisible)
                        {
                            xImageRect.Fill = xDxImageBrush;
                        }
                        else
                        {
                            xImageRect.Fill = Brushes.Black;
                        }

                        xImageRect.VerticalAlignment = VerticalAlignment.Top;
                        xImageRect.HorizontalAlignment = HorizontalAlignment.Left;

                        var diuLayout = ScreenUtil.PixelsToDIU(tupleRect.Item1);
                        Canvas.SetLeft(xImageRect, diuLayout.Left - cameraObjectRect.X);
                        Canvas.SetTop(xImageRect, diuLayout.Top - cameraObjectRect.Y);
                        xImageRect.Width = diuLayout.Width;
                        xImageRect.Height = diuLayout.Height;
                        xImageRect.IsHitTestVisible = false;
                    }

                    dupicatedIndex++;
                }
            }

            if (!placedBrushes)
            {
                // Brush를 따서 배치하지 않은 상황이기 때문에,
                // WPF의 Render Performance를 위해 제거한다.
                this.cameraPopupControlManager.ControlVideoImageRect(0);
            }
        }

        /// <summary>
        /// 기본 상황에서 사용
        /// </summary>
        /// <param name="videoGraphic"></param>
        private void RefreshGraphicObjectVideo(CameraVideoGraphic videoGraphic)
        {
            var mapStartPoint = new Point();
            
            //현재 UserControl의 Rect 설정
            if (PresentationSource.FromVisual(this) != null)
                mapStartPoint = this.PointToScreenDIU(new Point(0, 0));

            this.RefreshGraphicObjectVideo(videoGraphic, mapStartPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="videoGraphic"></param>
        /// <param name="mapStartPoint"></param>
        private void RefreshGraphicObjectVideo(CameraVideoGraphic videoGraphic, Point mapStartPoint)
        {
            if (this.arcGISControlApi == null || videoGraphic == null || this.baseMap == null || this.baseTiledMapServiceLayer == null || double.IsNaN(this.baseMap.Resolution)) return;

            var videoRect = GeometryHelper.ToVideoRect(this.baseMap, videoGraphic.Geometry.Extent, this.baseTiledMapServiceLayer.FullExtent);
            // AlwaysKeepToCameraVideo여부에 따라 현재 보이고 있는 맵 영역에 카메라가 보이지 않을 경우 영상 Hide처리   [2015. 02. 16 엄태영]
            if (ArcGISConstSet.AlwaysKeepToCameraVideo && videoRect == Rect.Empty)
            {
                return;
            }
            else if (!ArcGISConstSet.AlwaysKeepToCameraVideo && videoRect == Rect.Empty)
            {
                arcGISControlApi.HideVideo(videoGraphic.ObjectID);
                return;
            }

            var objectPoint = new Point((videoGraphic.Geometry.Extent.XMin - baseMap.Extent.XMin) / this.baseMap.Resolution, (baseMap.Extent.YMax - videoGraphic.Geometry.Extent.YMax) / this.baseMap.Resolution);
            var objectsize = new Size(videoGraphic.Geometry.Extent.Width / this.baseMap.Resolution, videoGraphic.Geometry.Extent.Height / this.baseMap.Resolution);

            var cameraGraphicRectByMonitor = new Rect(videoRect.X + mapStartPoint.X, videoRect.Y + mapStartPoint.Y, objectsize.Width, objectsize.Height);
            var cameraGraphicRectInPixels = ScreenUtil.DIUToPixels(cameraGraphicRectByMonitor);

            var addRect = ScreenUtil.GetTotalRectanglesByMonitor(cameraGraphicRectInPixels);

            var zIndex = videoGraphic.ZIndex;

            var isMapVisible = (arcGisControlViewerApi == null || (arcGisControlViewerApi != null && arcGisControlViewerApi.GetMapCellVisible()));

            var placedBrushes = false;

            if (PresentationSource.FromVisual(this) != null)
            {
                if (!videoGraphic.ShowVideo && !this.IsConsoleMode)
                {
                    this.arcGISControlApi.HideVideo(videoGraphic.ObjectID);
                }
                else if (videoGraphic.CanView && isMapVisible)
                {
                    // SHOW
                    this.arcGISControlApi.MoveCameraVideo(
                        videoGraphic.ObjectID,
                        videoGraphic.CameraInformationID,
                        cameraGraphicRectInPixels.X,
                        cameraGraphicRectInPixels.Y,
                        cameraGraphicRectInPixels.Width,
                        cameraGraphicRectInPixels.Height,
                        zIndex,
                        false
                    );
                }   
                else
                {
                    // HIDE
                    this.arcGISControlApi.MoveCameraVideo(
                        videoGraphic.ObjectID,
                        videoGraphic.CameraInformationID,
                        cameraGraphicRectInPixels.X,
                        cameraGraphicRectInPixels.Y,
                        0,
                        0,
                        zIndex,
                        false
                    );
                }

                videoGraphic.Control.EnsureRectangleCount(addRect.Count);

                if (!videoGraphic.ShowVideo && !this.IsConsoleMode)
                {
                    videoGraphic.Control.HideImageVisibility = Visibility.Visible;
                }
                else if (videoGraphic.CanView && isMapVisible)
                {
                    videoGraphic.Control.HideImageVisibility = Visibility.Collapsed;

                    int rectangleIndex = 0;
                    placedBrushes = true;
                    foreach (Tuple<Rect, Rect> tupleRect in addRect.Reverse<Tuple<Rect, Rect>>())
                    {
                        var diuLayout = ScreenUtil.PixelsToDIU(tupleRect.Item1);
                        videoGraphic.Control.SetPositionAndBrush(
                            rectangleIndex,
                            diuLayout.Left - cameraGraphicRectByMonitor.Left,
                            diuLayout.Top - cameraGraphicRectByMonitor.Top,
                            diuLayout.Width,
                            diuLayout.Height,
                            this.imageBrushGraphicManager.GetImageBrush(tupleRect)
                        );
                        rectangleIndex++;
                    }
                }
            }

            if (!placedBrushes)
            {
                // Brush를 따서 배치하지 않은 상황이기 때문에,
                // WPF의 Render Performance를 위해 제거한다.
                videoGraphic.Control.EnsureRectangleCount(0);
            }

            // 카메라 오버레이 컨트롤 위치 업데이트
            if (this.cameraOverlayControl.UnigridGuid != null && this.cameraOverlayControl.UnigridGuid.Equals(videoGraphic.ObjectID))
            {
                this.RefreshCameraOverlayControlPosition(videoGraphic);
            }
        }

        #endregion //Video Control 통신을 위한 Methods

        #region CameraGraphic MouseLeftButtonDown Method

        private void DoCameraIconMouseLeftButtonDown(BaseGraphic currentGraphic)
        {
            if (this.isEditMode == true)
            {
                this.SetClickPositionDatas();
            }
            else
            {
                #region View

                this.ShowCameraPopupControl(currentGraphic as CameraIconGraphic);
                this.MoveCameraPopupControl();

                #endregion
            }
        }

        private void DoCameraPresetMouseLeftButtonDown(BaseGraphic currentGraphic)
        {
            if (this.isEditMode == true)
            {
                #region Edit
                this.SetClickPositionDatas();
                ESRI.ArcGIS.Client.Geometry.PointCollection pc = (currentGraphic.Geometry as Polygon).Rings[0];

                foreach (MapPoint tmp in pc)
                {
                    //좌측 좌표 확인
                    if (tmp.Y != currentGraphic.Geometry.Extent.XMax && tmp.X == currentGraphic.Geometry.Extent.XMin)
                    {
                        //클릭한 위치 확인
                        if (this.mapPointByClick.X < tmp.X + (ArcGISClientViewer.ResizingBorderWidth * this.baseMap.Resolution) &&
                            this.mapPointByClick.Y < tmp.Y + (ArcGISClientViewer.ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            this.resizingFlag = ResizingFlag.X;
                        }
                    }
                    //우측 좌표
                    else if (tmp.Y != currentGraphic.Geometry.Extent.XMax && tmp.X == currentGraphic.Geometry.Extent.XMax)
                    {
                        //클릭한 위치 확인
                        if (this.mapPointByClick.X > tmp.X - (ArcGISClientViewer.ResizingBorderWidth * this.baseMap.Resolution) &&
                            this.mapPointByClick.Y < tmp.Y + (ArcGISClientViewer.ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            this.resizingFlag = ResizingFlag.Width;
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region View
                this.ChangeCursor(Cursors.Arrow);
                var presetGraphic = currentGraphic as CameraPresetGraphic;

                if (presetGraphic != null)
                {
                    presetGraphic.ChangeOpaqueColor();

                    var cameraComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(presetGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                    if (this.arcGisControlViewerApi != null && cameraComponentData != null && cameraComponentData.PresetDatas != null &&
                        presetGraphic.PresetIndex >= 0 && cameraComponentData.PresetDatas.Count > presetGraphic.PresetIndex)
                    {
                        var presetNum = cameraComponentData.PresetDatas.ElementAt(presetGraphic.PresetIndex).PresetNum;
                        this.arcGisControlViewerApi.ExcutePreset(cameraComponentData.CameraInformationID, presetNum);
                    }
                }
                #endregion
            }
        }

        private void DoCameraPresetPlusMouseLeftButtonDown(BaseGraphic currentGraphic)
        {
            if (this.isEditMode == true)
            {
                #region Edit
                this.SetClickPositionDatas();
                this.AddCameraPresetGraphic(currentGraphic.ObjectID);
                this.ChangeCursor(Cursors.Arrow);
                #endregion
            }
            else
            {
                this.ChangeCursor(Cursors.Arrow);
            }
        }

        #endregion

        #region CameraPopup Method

        protected override void ShowCameraPopupControl(CameraIconGraphic graphic)
        {
            if (this.cameraPopupControlManager.IsShowCameraPoupupControl)
            {
                if (this.arcGISControlApi != null && !String.IsNullOrEmpty(this.cameraPopupControlManager.CameraUnigridId))
                    this.arcGISControlApi.EraseVideo(this.cameraPopupControlManager.CameraUnigridId);
            }

            var videoGraphic = this.GetOneBaseGraphicInGraphicLayer(graphic.ObjectID,
                                                                    MapObjectType.CameraVideo) as CameraVideoGraphic;

            base.ShowCameraPopupControl(graphic);

            this.cameraPopupControlManager.CameraUnigridId = this.arcGISControlApi.CreateCameraVideo(videoGraphic.CameraInformationID, true, false);
            this.cameraPopupControlManager.CanView = videoGraphic.CanView;

            this.cameraOverlayControl.ShowCameraPopupControl();

            this.ChangeCursor(Cursors.Arrow);
        }

        protected override void MoveCameraPopupControl()
        {
            base.MoveCameraPopupControl();

            this.RefreshAllCameraVideoRect();
        }

        protected override void HideCameraPopupControl()
        {
            if (this.arcGISControlApi != null && !String.IsNullOrEmpty(this.cameraPopupControlManager.CameraUnigridId))
                this.arcGISControlApi.EraseVideo(this.cameraPopupControlManager.CameraUnigridId);

            base.HideCameraPopupControl();
        }

        #endregion //CameraPopup Method

        #endregion //Methods

        #region EventHandlers

        #region Camera Graphic Event Handlers

        /// <summary>
        /// Camera Graphic Object Mouse Down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void CameraGraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (isDoingNewObjectByClick)
            {
                return;
            }

            if (this.isEditMode && _IsPanning)
            {
                return; // Space 누르면 패닝만 된다.
            }
            
            var currentGraphic = sender as BaseGraphic;

            if (currentGraphic == null)
                return;

            //자신 선택
            this.SelectGraphicObject(currentGraphic);

            mouseButtonEventArgs.Handled = true;

            switch (currentGraphic.Type)
            {
                case MapObjectType.CameraPresetPlus:
                    DoCameraPresetPlusMouseLeftButtonDown(currentGraphic);
                    break;

                case MapObjectType.CameraVideo:
                    this.SetClickPositionDatas();
                    break;

                case MapObjectType.CameraPreset:
                    DoCameraPresetMouseLeftButtonDown(currentGraphic);
                    break;

                case MapObjectType.CameraIcon:
                    DoCameraIconMouseLeftButtonDown(currentGraphic);
                    break;
            }

            #region 코드 리펙토링 이전 소스 : 테스트 후 삭제.
            //Edit Mode 일 경우
            //if (this.isEditMode)
            //{
            //    this.SetClickPositionDatas();

            //    //Add View Zone Plus
            //    switch (currentGraphic.Type)
            //    {
            //        case MapObjectType.CameraPresetPlus:
            //            DoCameraPresetPlusMouseLeftButtonDown(currentGraphic);
            //            break;

            //        case MapObjectType.CameraVideo:
            //            DoCameraVideoMouseLeftButtonDown(currentGraphic);
            //            break;

            //        case MapObjectType.CameraPreset:
            //            DoCameraPresetMouseLeftButtonDown(currentGraphic);
            //            break;
            //    }
            //} //End of Edit Mode
            //else
            //{
            //    this.ChangeCursor(Cursors.Arrow);

            //    switch (currentGraphic.Type)
            //    {
            //        case MapObjectType.CameraIcon:
            //            DoCameraIconMouseLeftButtonDown(currentGraphic);
            //            break;

            //        case MapObjectType.CameraPreset:
            //            DoCameraPresetMouseLeftButtonDown(currentGraphic);
            //            break;
            //    }
            //}
            #endregion
        }

        private void CameraGraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (this.isEditMode)
            {
                this.ReleaseClickPositionDatas();
            }
            else
            {
                var currentGraphic = sender as BaseGraphic;

                if (currentGraphic is CameraPresetGraphic)
                {
                    var presetGraphic = currentGraphic as CameraPresetGraphic;
                    var cameraComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(presetGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                    presetGraphic.ChangeOriginalColor();

                    if (this.arcGisControlViewerApi != null && cameraComponentData != null)
                    {
                        // 추가, PresetIndex값이 MouseDown과 다르게 동작되어 카메라 ptz제어 명령시 잘못 전송됨.  [2014. 07. 17 엄태영]
                        // NOTE : MouseDownd이벤트와 동일한 동작(카메라 Ptz관련 ExcutePreset 호출)을 하는데 왜 굳이 MouseUp에서도 ExcutePreset호출을 하는지 의문이듬    [2014. 07. 17 엄태영]
                        string StrpresetNum = cameraComponentData.PresetDatas.ElementAt(presetGraphic.PresetIndex).PresetNum;
                        int presetNum = 0;
                        if (int.TryParse(StrpresetNum, out presetNum))
                            this.arcGisControlViewerApi.ExcutePreset(cameraComponentData.CameraInformationID, presetNum.ToString(CultureInfo.InvariantCulture));
                    }

                }

                mouseButtonEventArgs.Handled = true;
            }
            //issue #15405 이유료 주석처리함. 
            //mouseButtonEventArgs.Handled = true;
        }

        private void CameraGraphicOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.GraphicContextMenuShow((BaseGraphic)sender);
        }

        /// <summary>
        /// Camera Move 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void CameraGraphicOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.isDoingNewObjectByClick)
            {
                return;
            }
            
            var currentGraphic = sender as BaseGraphic;

            if (currentGraphic == null)
                return;
            
            if (Mouse.LeftButton == MouseButtonState.Released) mouseEventArgs.Handled = true;

            if (this.isEditMode)
            {
                //선택된 위치 저장
                var currentMapPointByClick = baseMap.ScreenToMap(mouseEventArgs.GetPosition(baseMap));

                //CHANGE CAMERA RECT CUSOR
                if (currentGraphic.Type == MapObjectType.CameraVideo)
                {
                    #region Camera Video
                    bool left = false, top = false, right = false, bottom = false;

                    var minX = this.GetWrapAroundPoint(currentGraphic.Geometry.Extent.XMin);

                    //if (mouseEventArgs.LeftButton != MouseButtonState.Pressed)
                    {
                        //좌측 좌표
                        if (currentMapPointByClick.X < minX + (ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            left = true;
                        }

                        //우측 좌표
                        if (currentMapPointByClick.X > (minX + currentGraphic.Geometry.Extent.Width) - (ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            right = true;
                        }

                        //하단 좌표
                        if (currentMapPointByClick.Y < currentGraphic.Geometry.Extent.YMin + (ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            bottom = true;
                        }

                        //상단 좌표
                        if (currentMapPointByClick.Y > currentGraphic.Geometry.Extent.YMax - (ResizingBorderWidth * this.baseMap.Resolution))
                        {
                            top = true;
                        }
                    }

                    if (left && top)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown));
                    }
                    else if (left && bottom)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftDownRightUp));
                    }
                    else if (right && top)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftDownRightUp));
                    }
                    else if (right && bottom)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown));
                    }
                    else if (left)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftRight));
                    }
                    else if (right)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftRight));
                    }
                    else if (top)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.UpDown));
                    }
                    else if (bottom)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.UpDown));
                    }
                    else
                    {
                        if (IsPanning && Mouse.LeftButton == MouseButtonState.Released)
                        {
                            this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
                        }
                        else if (IsPanning && Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold));
                        }
                        else
                        {
                            this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                        }
                    }
                    #endregion
                }
                //CHANGE ICON CUSOR
                else if (currentGraphic.Type == MapObjectType.CameraIcon)
                {
                    if (IsPanning && Mouse.LeftButton == MouseButtonState.Released)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
                    }
                    else if (IsPanning && Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold));
                    }
                    else
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                    }
                }
                else if (currentGraphic.Type == MapObjectType.CameraPreset)
                {
                    if (IsPanning && Mouse.LeftButton == MouseButtonState.Released)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));
                    }
                    else if (IsPanning && Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold));
                    }
                    else
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                    }
                }
                else
                {
                    this.ChangeCursor(Cursors.Arrow);
                }
            }
            else
            {
                this.ChangeCursor(Cursors.Arrow);
            }
        }

        /// <summary>
        /// Camera Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void CameraGraphicOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.isDoingNewObjectByClick)
            {
                return;
            }

            var currentGraphic = sender as BaseGraphic;

            if (currentGraphic == null)
                return;

            if (this.arcGISControlApi != null && currentGraphic.Type == MapObjectType.CameraVideo && IsConsoleMode)
            {
                this.ShowCameraOverlayControl(currentGraphic);
            }
        }
        /// <summary>
        /// Camera Graphic Mouse Leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void CameraGraphicOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
                this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));

            var currentGraphic = sender as BaseGraphic;

            if (!this.isEditMode)
            {
                if (currentGraphic is CameraPresetGraphic)
                {
                    var presetGraphic = currentGraphic as CameraPresetGraphic;

                    presetGraphic.ChangeOriginalColor();
                }
            }
        }

        private void cameraOverlayControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectCameraVideoGraphic(this.cameraOverlayControl.UnigridGuid);
        }

        private PanomorphOverlayUserControl.PanomorphViewTypes GetPanomortphViewType(string unigridGuid)
        {
            var typeString = this.arcGisControlViewerApi.GetPanomorphViewType(unigridGuid);
            PanomorphOverlayUserControl.PanomorphViewTypes type;
            if (!Enum.TryParse(typeString, out type))
            {
                return PanomorphOverlayUserControl.PanomorphViewTypes.PTZ;
            }

            return type;
        }

        private void GraphicOnZIndexChanged(object sender, EventArgs eventArgs)
        {
            var graphic = sender as BaseGraphic;

            if(this.arcGISControlApi != null && graphic != null) this.arcGISControlApi.SetZIndexCameraVideo(graphic.ObjectID, graphic.ZIndex); 
        }

        #endregion //Camera Graphic Event Handlers
        
        #region Popup Camera Control Event Handler

        /// <summary>
        /// Preset 선택이 변경후 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="presetListSelectionChangedEventArgs"></param>
        private void CameraPopup_ePresetListSelectionChanged(object sender, CameraPopupControlViewModel.PresetListSelectionChangedEventArgs presetListSelectionChangedEventArgs)
        {
            if (this.arcGisControlViewerApi != null && presetListSelectionChangedEventArgs != null)
            {
                this.arcGisControlViewerApi.ExcutePreset(this.cameraPopupControlManager.CameraInformationId, presetListSelectionChangedEventArgs.PresetIndex);
            }
        }

        /// <summary>
        /// Close 버튼 클릭후 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void CameraPopupManager_eCloseButtonClicked(object sender, EventArgs eventArgs)
        {
            this.HideCameraPopupControl();
        }

        /// <summary>
        /// Popup Control 이 Visible 된후 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void CameraPopup_eControlOpend(object sender, EventArgs eventArgs)
        {
            if(this.arcGisControlViewerApi != null )this.arcGisControlViewerApi.OpenPanomorphPopupCamera();
            this.RefreshCameraPopupControlVideo();
        }

        #endregion Popup Camera Control Event Handler

        #endregion EventHandlers

        #region Camera Overlay Control Methods

        private void ShowCameraOverlayControl(BaseGraphic BaseGraphic)
        {
            var cameraGraphic = BaseGraphic as CameraVideoGraphic;

            if (cameraGraphic == null || this.cameraOverlayControl.xRdsOverlayControl.EnableRdsControl)
            {
                return;
            }

            if (cameraOverlayGraphic == null)
            {
                cameraOverlayGraphic = new PolygonControlGraphic<CameraOverlayControl>(cameraOverlayControl, MapObjectType.CameraOverlayControl, cameraGraphic.ObjectID, cameraGraphic.PointCollection);
                cameraOverlayGraphic.MouseLeave += CameraOverlayGraphicOnMouseLeave;
            }

            if(!this.objectGraphicLayer.Graphics.Contains(cameraOverlayGraphic))
            {
                this.objectGraphicLayer.Graphics.Add(cameraOverlayGraphic);
            }

            var isRecording = this.arcGISControlApi.GetRecordingState(cameraGraphic.CameraInformationID) && !this.IsDataPlayBackMode;
            var isRds = this.arcGISControlApi.GeRdsState(cameraGraphic.CameraInformationID) && !this.IsDataPlayBackMode;
            var isPanomorph = this.arcGisControlViewerApi.GetPanomorphState(cameraGraphic.CameraInformationID);

            this.cameraOverlayControl.Initialize(
                cameraGraphic.ObjectID,
                cameraGraphic.CameraInformationID,
                isRecording,
                isRds,
                isPanomorph,
                cameraGraphic.CameraInformationID,
                cameraGraphic.ZIndex);

            this.arcGisControlViewerApi.SetPanomorphControlMode(cameraGraphic.ObjectID);

            this.cameraOverlayControl.SetPanomorphViewTypeUI(this.GetPanomortphViewType(cameraGraphic.ObjectID));

            this.cameraOverlayControl.Visibility = Visibility.Visible;
            this.cameraOverlayGraphic.IsVisible = true;

            this.RefreshCameraOverlayControlPosition(cameraGraphic);
        }

        private void CameraOverlayGraphicOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!this.cameraOverlayControl.IsUsingOverlayControl)
            {
                if (this.objectGraphicLayer.Graphics.Contains(cameraOverlayGraphic))
                {
                    this.objectGraphicLayer.Graphics.Remove(cameraOverlayGraphic);
                }
            }
            else
            {
                this.cameraOverlayControl.xIndependentPlaybackOverlayControl.xTimePickerControlPopup.Closed += this.xTimePickerControlPopup_Closed;
            }
        }

        private void RefreshCameraOverlayControlPosition(CameraVideoGraphic cameraVideoGraphic)
        {
            if (this.cameraOverlayGraphic == null || this.cameraOverlayControl == null)
                return;
            
            this.cameraOverlayGraphic.ObjectID = cameraVideoGraphic.ObjectID;
            this.cameraOverlayGraphic.PointCollection = cameraVideoGraphic.PointCollection;
            this.cameraOverlayGraphic.ZIndex = cameraVideoGraphic.ZIndex + 1;
        }

        private void xTimePickerControlPopup_Closed(object sender, EventArgs e)
        {
            this.cameraOverlayControl.xIndependentPlaybackOverlayControl.xTimePickerControlPopup.Closed -= xTimePickerControlPopup_Closed;

            if (!this.cameraOverlayControl.IsMouseOver)
            {
                if (this.objectGraphicLayer.Graphics.Contains(cameraOverlayGraphic))
                {
                    this.objectGraphicLayer.Graphics.Remove(cameraOverlayGraphic);
                }
            }
        }

        #endregion // Camera Overlay Control Methods
    }
}
