using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using ArcGISControl.ArcGISInternalHack;
using ArcGISControl.Command;
using ArcGISControl.DataManager;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.Language;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Parsers;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Brushes = System.Windows.Media.Brushes;
using CursorType = ArcGISControl.Helper.CursorType;
using Geometry = ESRI.ArcGIS.Client.Geometry.Geometry;
using Point = System.Windows.Point;
using Polygon = ESRI.ArcGIS.Client.Geometry.Polygon;
using Timer = System.Timers.Timer;

namespace ArcGISControl
{
    public partial class ArcGISClientViewer
    {
        #region Field

        //선택된 Graphic에 의해 함께 선택된 Graphic
        private readonly List<BaseGraphic> selectedGraphicList = new List<BaseGraphic>();

        /// <summary>
        /// 링크존 더블 클릭 상태. 
        /// </summary>
        private bool isLinkZoneDoubleClicked;

        private Timer clickTimer = new Timer();

        private MapLinkZoneObjectDataInfo selectedMapLinkZoneObjectDataInfo;

        private MapUniversalObjectDataInfo selectedMapUniversalObjectDataInfo;

        private Point linkZoneClickPos;

        private Point splunkControlClickPos;

        /// <summary>
        /// 외부에서 선택한 MapObjectType
        /// </summary>
        public MapObjectType SelectedMapObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// Multi Select 중인지 체크
        /// </summary>
        private bool isDoingMultiSelect
        {
            get { return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && this.isEditMode; }
        }

        private bool isDoingNewObjectByClick
        {
            get
            {
                return SelectedMapObjectType == MapObjectType.Location || SelectedMapObjectType == MapObjectType.LinkZone || SelectedMapObjectType == MapObjectType.ImageLinkZone ||
                    SelectedMapObjectType == MapObjectType.Universal || SelectedMapObjectType == MapObjectType.Workstation || SelectedMapObjectType == MapObjectType.Text ||
                    SelectedMapObjectType == MapObjectType.Line || SelectedMapObjectType == MapObjectType.DrawLine || SelectedMapObjectType == MapObjectType.Image;
            }
        }

        public class LinkZoneZoomInfo
        {
            public MapLinkZoneObjectDataInfo LinkZoneData { get; set; }
            public MapUniversalObjectDataInfo UniversalData { get; set; }
            public Envelope PrevExtent { get; set; }
            public Envelope TargetExtent { get; set; }
        }

        /// <summary>
        /// LinkZone 더블클릭시 줌이 끝났을 때 사용할 데이터
        /// 
        /// Item1: 링크존 데이터
        /// Item2: 링크존을 클릭한 시점의 extent 데이터
        /// </summary>
        private LinkZoneZoomInfo doubleClickLinkZoneObjectData;

        /// <summary>
        /// Use for the DrawLine
        /// </summary>
        private Line Line;

        #endregion //Field

        #region Events

        /// 로케이션에 선택 변경 통보
        /// Null로 넘어 갈때는 모두 선택 안됨
        public event EventHandler<SelectedObjectEventArgs> eObjectSelected;

        private void RaiseObjectSelectedEvent()
        {
            var handler = this.eObjectSelected;
            if (handler != null)
            {
                var selectedObject = this.selectedGraphicList.Select(graphic => new SelectdObjectItem(graphic.ObjectID, graphic.Type));

                handler(this, new SelectedObjectEventArgs(selectedObject));
            }
        }

        //MapObject 삭제시 통보
        public event EventHandler<ObjectEventArgs> eObjectDeleted;

        private void RaiseObjectDeletedEvent(string objectId, MapObjectType type)
        {
            var objectEvent = this.eObjectDeleted;
            if (objectEvent != null)
            {
                objectEvent(this, new ObjectEventArgs(objectId, type));

                if (this.isEditMode)
                {
                    this.historyManager.IsChanged = true;
                }
            }
        }

        /// MapObject 생성 통보
        public event EventHandler<ObjectDataEventArgs> eObjectAdded;

        private void RaiseObjectAddedEvent(BaseMapObjectInfoData data)
        {
            var handler = this.eObjectAdded;
            if (handler != null)
            {
                if (data.IsUndoManage == false)
                {
                    this.AddCommandToHistory(new CommandAdd(data, this.DeleteMapObjectData, this.AddMapObject, this.RaiseObjectAddedEvent));
                }

                handler(this, new ObjectDataEventArgs(data));

                if (this.isEditMode)
                {
                    this.historyManager.IsChanged = true;
                }
            }
        }

        //Save
        public event EventHandler<ObjectEventArgs> eSearchedAddressSaveButtonClick;

        private void RaiseSearchedAddressSaveButtonClickEvent(ObjectEventArgs objectEventArgs)
        {
            var handler = this.eSearchedAddressSaveButtonClick;
            if (handler != null)
            {
                handler(this, objectEventArgs);
            }
        }

        #endregion Events

        #region Method

        #region Make Graphic&Object Methods

        protected override IconGraphic MakeLocationGraphic(MapLocationObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeLocationGraphic(dataInfo, isPublicData);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override IEnumerable<BaseGraphic> MakeImageLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphics = base.MakeImageLinkZoneGraphic(dataInfo, isPublicData);

            var baseGraphics = graphics as BaseGraphic[] ?? graphics.ToArray();

            if (baseGraphics.Count() != 2) return null;

            var imageGraphic = baseGraphics[0] as ImagePolygonGraphic;
            var linkZoneGraphic = baseGraphics[1] as LinkZoneGraphic;

            if (imageGraphic == null || linkZoneGraphic == null) return null;

            #region imageGraphic

            if (!string.IsNullOrEmpty(dataInfo.ImageObjectData.ImageDataStream) && !string.IsNullOrEmpty(dataInfo.ImageObjectData.ImageFileName))
            {
                imageGraphic.ChageSymbolImage(dataInfo.ImageObjectData.ImageDataStream, dataInfo.ImageObjectData.ImageOpacity);
            }
            else if (!this.IsConsoleMode)
            {

                dataInfo.ImageObjectData.ImageFileName = "Go";
                dataInfo.ImageObjectData.ImageDataStream = ImageStreamContorl.ResourceUriToStream((Bitmap)Properties.Resources.LinkZoneDefault);
                imageGraphic.ChageSymbolImage(dataInfo.ImageObjectData.ImageDataStream, dataInfo.ImageObjectData.ImageOpacity);
            }

            if (this.IsConsoleMode)
            {
                //Event 없이 화면에만 표출
                base.SetBaseGraphic(imageGraphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);
            }
            else
            {
                //Event 함게 호출
                this.SetBaseGraphic(imageGraphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);
            }

            #endregion //imageGraphic

            #region Over Graphic

            if (this.IsConsoleMode)
            {
                linkZoneGraphic.ChangeColors(ArcGISConstSet.ImageLinkZoneNormalColor, ArcGISConstSet.ImageLinkZoneNormalColor);
                this.SetBaseGraphic(linkZoneGraphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);
            }

            #endregion //Over Graphic

            var data = this.arcGISControlApi.SetMapSplunkData(dataInfo.ColorSplunkBasicInformationData);

            if (data != null) this.publicGraphicDataManager.StartSplunkService(dataInfo.ObjectID, dataInfo.ColorSplunkBasicInformationData, !this.isEditMode);

            return baseGraphics;
        }

        protected override LinkZoneGraphic MakeLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeLinkZoneGraphic(dataInfo, isPublicData);

            var data = this.arcGISControlApi.SetMapSplunkData(dataInfo.ColorSplunkBasicInformationData);

            if (data != null) this.publicGraphicDataManager.StartSplunkService(dataInfo.ObjectID, dataInfo.ColorSplunkBasicInformationData, !this.isEditMode);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override LinkZoneGraphic MakeWorkStationGraphic(MapWorkStationObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeWorkStationGraphic(dataInfo, isPublicData);

            var data = this.arcGISControlApi.SetMapSplunkData(dataInfo.SplunkBasicInformation);

            if (data != null) this.publicGraphicDataManager.StartSplunkService(dataInfo.ObjectID, dataInfo.SplunkBasicInformation, !this.isEditMode);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override IconGraphic MakeAddressGraphic(MapAddressObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeAddressGraphic(dataInfo, isPublicData);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override PolygonControlGraphic<TextBoxControl> MakeTextGraphic(MapTextObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeTextGraphic(dataInfo, isPublicData);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            //todo : Edit mode가 변경 될때 마다 호출 되도록 한다.
            graphic.Control.IsReadOnly = !this.isEditMode;

            return graphic;
        }

        protected override LineGraphic MakeLineGraphic(MapLineObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeLineGraphic(dataInfo, isPublicData);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override PolygonControlGraphic<LineCanvasControl> MakeDrawLineGraphic(LineCanvasControl lcc, bool isPublicData)
        {
            var graphic = base.MakeDrawLineGraphic(lcc, isPublicData);

            this.SetBaseGraphic(graphic, 99999, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }

        protected override ImagePolygonGraphic MakeImageGraphic(MapImageObjectDataInfo dataInfo, bool isPublicData)
        {
            var graphic = base.MakeImageGraphic(dataInfo, isPublicData);

            this.SetBaseGraphic(graphic, dataInfo.ObjectZIndex, isPublicData ? ZLevel.L0 : ZLevel.L2);

            return graphic;
        }


        protected override void SetBaseGraphic(BaseGraphic graphic, int zIndex, ZLevel zLevel)
        {
            base.SetBaseGraphic(graphic, zIndex, zLevel);

            graphic.MouseLeftButtonDown += LocationGraphicOnMouseLeftButtonDown;
            graphic.MouseLeftButtonUp += LocationGraphicOnMouseLeftButtonUp;
            graphic.MouseRightButtonUp += graphic_MouseRightButtonUp;
            graphic.MouseMove += LocationGraphicOnMouseMove;
            graphic.MouseLeave += LocationGraphicOnMouseLeave;
            graphic.MouseEnter += LocationGraphicOnMouseEnter;
        }

        private void SetMarkerGraphic(BaseGraphic graphic, int zIndex, ZLevel zLevel)
        {
            base.SetBaseGraphic(graphic, zIndex, zLevel);

            graphic.MouseLeftButtonDown += GraphicOnMouseLeftButtonDown;
            graphic.MouseLeftButtonUp += GraphicOnMouseLeftButtonUp;
            graphic.MouseMove += GraphicOnMouseMove;
            graphic.MouseLeave += GraphicOnMouseLeave;
            graphic.MouseEnter += GraphicOnMouseEnter;
        }

        private void GraphicOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            this.ChangeCursor(Cursors.Arrow);
        }

        private void GraphicOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            //throw new NotImplementedException();
        }

        private void GraphicOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            mouseEventArgs.Handled = true;

            if (Mouse.LeftButton == MouseButtonState.Released || this.mapPointByClick == null) return;

            var baseGraphic = sender as BaseGraphic;
            if (baseGraphic == null) return;

            var newMapPoint = baseMap.ScreenToMap(mouseEventArgs.GetPosition(baseMap));
            var displacement = new Vector(newMapPoint.X - mapPointByClick.X, newMapPoint.Y - mapPointByClick.Y);

            this.MoveGraphicPosition(baseGraphic, displacement);

            this.mapPointByClick = newMapPoint;
        }

        private void GraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            mouseButtonEventArgs.Handled = true;

            var baseGraphic = sender as BaseGraphic;
            if (baseGraphic == null) return;
            baseGraphic.SelectFlag = false;
            
            this.mapPointByClick = null;
        }

        private void GraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            mouseButtonEventArgs.Handled = true;

            var baseGraphic = sender as BaseGraphic;
            if (baseGraphic == null) return;
            baseGraphic.SelectFlag = true;
            
            this.mapPointByClick = baseMap.ScreenToMap(Mouse.GetPosition(baseMap));
            this.ChangeCursor(Cursors.Arrow);
        }

        protected override void SetSearchAddressGraphic(SearchedAddressIconGraphic searchedAddressIconGraphic, int zIndex = -1)
        {
            base.SetSearchAddressGraphic(searchedAddressIconGraphic, zIndex);

            searchedAddressIconGraphic.MouseLeftButtonDown += SearchedAddressIconOnMouseLeftButtonDown;
            searchedAddressIconGraphic.MouseMove += SearchedAddressIconOnMouseMove;
        }

        private void MakeMapObject(MapObjectType SelectedMapObjectType, BaseMapObjectInfoData MapObjectBaseData)
        {
            var mousPoint = Mouse.GetPosition(baseMap);

            if (mousPoint.X < 0 || mousPoint.Y < 0)
                return;

            string Name = (!this.IsConsoleMode) ? publicGraphicDataManager.GetNewObjectName(MapObjectType.Location) : privateGraphicDataManager.GetNewObjectName(MapObjectType.Location);
            MapPoint newMapPoint = baseMap.ScreenToMap(mousPoint);
            Point Position = new Point(newMapPoint.X, newMapPoint.Y);

            List<Point> PointCollection;
            if (SelectedMapObjectType == MapObjectType.Line)
            {
                PointCollection = GeometryHelper.GetLinePoints(Position, ArcGISConstSet.ObjectBasicSize, base.baseMap.Resolution);
            }
            else
            {
                PointCollection = GeometryHelper.GetRectanglePoints(Position, ArcGISConstSet.ObjectBasicSize,
                                                                                base.baseMap.Resolution);
            }

            MapObjectBaseData.Init(SelectedMapObjectType, Name, Position, PointCollection);

            dynamic dynamic_MapObjectBaseData = MapObjectBaseData;
            object MapObjectGraphic = this.GetType().GetMethod(SelectedMapObjectType.GetMapObjectTypeInfoAttribute(0).CreateMethodNameByDataInfo,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(this,
                new Object[] { dynamic_MapObjectBaseData, !this.IsConsoleMode });

            if(MapObjectGraphic is BaseGraphic)
            {
                this.HandleMakedGraphic((BaseGraphic)MapObjectGraphic);
            }
            else if (MapObjectGraphic is IEnumerable<BaseGraphic>)
            {
                this.HandleMakedGraphic((IEnumerable<BaseGraphic>)MapObjectGraphic);
            }

            this.RaiseObjectAddedEvent(MapObjectBaseData);
        }

        /// <summary>
        /// 통합 객체 생성
        /// </summary>
        private void MakeUniversal()
        {
            var mousePoint = Mouse.GetPosition(baseMap);

            var newMapPoint = baseMap.ScreenToMap(mousePoint);

            var universalData = this.universalObjectDataManager.CreateDataInfo(new Point(newMapPoint.X, newMapPoint.Y), this.baseMap.Resolution);

            this.MakeUniversalGraphic(universalData);

            this.HandleMakedGraphic(this.universalObjectDataManager.GetControlGraphic(universalData.ObjectID));

            this.RaiseObjectAddedEvent(universalData);
        }

        /// <summary>
        /// DrawLine 을 위한 override
        /// </summary>
        /// <param name="point"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="size"></param>
        private void MakeLine(Point point, Point point1, Point point2, System.Windows.Size size)
        {
            var lineData = this.publicGraphicDataManager.CreateMapLineObjectData(point, point1, point2, size, baseMap.Resolution);

            var graphic = this.MakeLineGraphic(lineData, !this.IsConsoleMode);

            this.HandleMakedGraphic(graphic);

            this.RaiseObjectAddedEvent(lineData);
        }

        /// <summary>
        /// DrawLine 생성
        /// </summary>
        private void MakeDrawLine()
        {
            var mousPoint = Mouse.GetPosition(baseMap);
            var newMapPoint = baseMap.ScreenToMap(mousPoint);
            
            LineCanvasControl lcc = new LineCanvasControl();

            lcc.ControlAdded += lcc_ControlAdded;

            lcc.ArcGISClientVW = this;

            lcc.FirstClickPosition = new Point(newMapPoint.X, newMapPoint.Y);

            List<Point> pointCollection = GeometryHelper.GetRectanglePoints(lcc.FirstClickPosition, ArcGISConstSet.ObjectBasicSize, base.baseMap.Resolution);

            lcc.LineControlPoint = pointCollection;

            var graphic = this.MakeDrawLineGraphic(lcc, !this.IsConsoleMode);

            this.HandleMakedGraphic(graphic);
        }

        void lcc_ControlAdded(object sender, EventArgs e)
        {
            // LineCanvas 컨트롤은 SymbolGraphic의 ContentControl [UserControl]로 Add한 후 다시 LineGraphic으로 변환 작업을 거친다.
            if (sender is LineCanvasControl)
            {
                LineCanvasControl lcc = sender as LineCanvasControl;
                // 드래깅해서 그린 라인 사이즈
                System.Windows.Size DrawLineSize = new System.Windows.Size(lcc.DrawLine.Width, lcc.DrawLine.Height);

                // 라인 X1, Y1 좌표 -> 맵 좌표로 변환
                MapPoint Mpoint1 = baseMap.ScreenToMap(new Point(lcc.DrawLine.X1, lcc.DrawLine.Y1));
                // 라인 X2, Y2 좌표 -> 맵 좌표로 변환
                MapPoint Mpoint2 = baseMap.ScreenToMap(new Point(lcc.DrawLine.X2, lcc.DrawLine.Y2));

                Point point = lcc.FirstClickPosition;
                Point point1 = new Point(Mpoint1.X, Mpoint1.Y);
                Point point2 = new Point(Mpoint2.X, Mpoint2.Y);

                base.objectGraphicLayer.Graphics.Remove(base.objectGraphicLayer.Graphics.Last());

                this.MakeLine(point, point1, point2, DrawLineSize);
            }
        }

        /// <summary>
        /// Graphic 만든 후에 처리
        /// </summary>
        /// <param name="graphicList"></param>
        private void HandleMakedGraphic(IEnumerable<BaseGraphic> graphicList)
        {
            var baseGraphics = graphicList as BaseGraphic[] ?? graphicList.ToArray();
            if (graphicList == null || !baseGraphics.Any()) return;

            this.HandleMakedGraphic(baseGraphics.ElementAt(0));
        }

        /// <summary>
        /// Graphic 만든 후 처리
        /// </summary>
        /// <param name="graphic"></param>
        private void HandleMakedGraphic(BaseGraphic graphic)
        {
            this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));

            SelectedMapObjectType = MapObjectType.None;
        }

        #endregion //Make Graphic&Object Methods

        #region DeleteMap Graphic&Object Methods

        private void DeleteMapSplunkObject(BaseMapObjectInfoData data)
        {
            var splunkObjectData = data as MapSplunkObjectDataInfo;

            var splunkIconGraphic = this.GetOneBaseGraphicInGraphicLayer(data.ObjectID, MapObjectType.SplunkIcon);
            var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(data.ObjectID, MapObjectType.SplunkControl);

            if (splunkIconGraphic != null) this.DeleteGraphic(splunkIconGraphic);
            if (splunkControlGraphic != null) this.DeleteGraphic(splunkControlGraphic);

            if (splunkObjectData != null)
            {
                this.savedSplunkObjectDataManager.StopSplunkService(data.ObjectID, splunkObjectData.SplunkBasicInformation);
                var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(data.ObjectID);

                if (splunkControl != null)
                {
                    splunkControl.MouseMove -= SplunkControlOnMouseMove;
                    splunkControl.MouseDown -= SplunkControlOnMouseDown;
                }

                if (!this.IsConsoleMode) this.savedSplunkObjectDataManager.DeleteOneObject(splunkObjectData);
            }
        }

        private void DeleteMapGraphicObject(BaseMapObjectInfoData data)
        {
            var baseGraphic = this.GetOneBaseGraphicInGraphicLayer(data.ObjectID, data.ObjectType);

            if (baseGraphic != null) this.DeleteGraphic(baseGraphic);


            if (this.IsConsoleMode) this.privateGraphicDataManager.DeleteOneObject(data);
            else this.publicGraphicDataManager.DeleteOneObject(data);

            if (this.locationInfoWindowManager != null) this.locationInfoWindowManager.HideInfoWindow();

            if (this.searchedAddressInfoWindowManager != null) this.searchedAddressInfoWindowManager.HideInfoWindow();
        }

        protected override void DeleteGraphic(BaseGraphic graphic)
        {
            if (this.editor != null) this.editor.Stop();

            base.DeleteGraphic(graphic);

            graphic.MouseLeftButtonDown -= LocationGraphicOnMouseLeftButtonDown;
            graphic.MouseLeftButtonUp -= LocationGraphicOnMouseLeftButtonUp;
            graphic.MouseRightButtonUp -= graphic_MouseRightButtonUp;
            graphic.MouseMove -= LocationGraphicOnMouseMove;
            graphic.MouseLeave -= LocationGraphicOnMouseLeave;
            graphic.MouseEnter -= LocationGraphicOnMouseEnter;

            this.DeleteVertexSelectedGraphic(graphic as IPointCollectionOwner);
            var selectedGraphic = this.selectedGraphicList.FirstOrDefault(item => item.ObjectID == graphic.ObjectID);
            this.selectedGraphicList.Remove(selectedGraphic);
        }

        protected override void DeleteSearchedAddressGraphic(SearchedAddressIconGraphic searchedAddressIconGraphic)
        {
            base.DeleteSearchedAddressGraphic(searchedAddressIconGraphic);

            searchedAddressIconGraphic.MouseLeftButtonDown -= SearchedAddressIconOnMouseLeftButtonDown;
            searchedAddressIconGraphic.MouseMove -= SearchedAddressIconOnMouseMove;
        }

        #endregion DeleteMap Graphic&Object Methods

        #region Select/UnSelect Graphic Methods

        /// <summary>
        /// Select 된 Object를 해제 한다.
        /// </summary>
        private void ReleaseAllSelectedObject(bool shouldStopGraphicEditor = true)
        {
            //LinkZone Editor
            if (editor != null)
            {
                try
                {
                    this.editor.Stop();
                }
                catch (Exception ex)
                {
                    InnowatchDebug.Logger.WriteLine(ex.ToString());
                }
            }

            if (shouldStopGraphicEditor)
                this.graphicEditorManager.StopEditing(this.DeleteGraphic);

            this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));

            if (this.selectedGraphicList != null)
            {
                this.selectedGraphicList.ToList().ForEach(selectedGraphic => this.UnSelectGraphicObject(selectedGraphic,false));
                this.selectedGraphicList.Clear();
            }

            if (this.dynamicSplunkControlManager != null)
            {
                this.dynamicSplunkControlManager.Hide();
            }

            if (this.workStationControlManager != null)
            {
                this.workStationControlManager.Hide();
            }

            if (this.locationInfoWindowManager != null) this.locationInfoWindowManager.HideInfoWindow();

            if (this.searchedAddressInfoWindowManager != null) this.searchedAddressInfoWindowManager.HideInfoWindow();

            this.SelectObjectProperty();
        }

        /// <summary>
        /// 하나의 Graphic DeSelect
        /// </summary>
        /// <param name="baseGraphic"></param>
        private void UnSelectGraphicObject(BaseGraphic baseGraphic, bool needToRaiseEvent = true)
        {
            if (baseGraphic.Type == MapObjectType.CameraIcon)
            {
                var datas = this.GetOneCameraComponentGraphicsInGraphicLayer(baseGraphic.ObjectID);

                foreach (var graphic in datas)
                {
                    graphic.SelectFlag = false;
                    this.DeleteVertexSelectedGraphic(graphic as IPointCollectionOwner);
                }

                if (this.selectedGraphicList.Contains(baseGraphic)) this.selectedGraphicList.Remove(baseGraphic);
            }
            else
            {
                baseGraphic.SelectFlag = false;

                switch (baseGraphic.Type)
                {
                    case MapObjectType.SplunkIcon:
                        var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(baseGraphic.ObjectID, MapObjectType.SplunkControl);
                        splunkControlGraphic.SelectFlag = false;

                        goto case MapObjectType.SplunkControl;

                    case MapObjectType.SplunkControl:
                        baseGraphic.SelectFlag = false;
                        var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(baseGraphic.ObjectID);
                        Debug.Assert(splunkControl != null);
                        splunkControl.IsSelected = false;
                        if (baseGraphic.Type == MapObjectType.SplunkIcon)
                        {
                            var pointCollectionOwner =
                                this.GetOneBaseGraphicInGraphicLayer(baseGraphic.ObjectID, MapObjectType.SplunkControl) as
                                IPointCollectionOwner;
                            this.DeleteVertexSelectedGraphic(pointCollectionOwner);
                        }
                        break;

                    case MapObjectType.Text:

                        var textBoxControl = (baseGraphic as PolygonControlGraphic<TextBoxControl>) != null ? (baseGraphic as PolygonControlGraphic<TextBoxControl>).Control : null;

                        if (textBoxControl == null) break;

                        textBoxControl.TakeTextBoxFocus();

                        break;

                    case MapObjectType.UniversalIcon:
                        var universalControlGraphic = this.universalObjectDataManager.GetControlGraphic(baseGraphic.ObjectID);
                        this.DeleteVertexSelectedGraphic(universalControlGraphic);
                        break;

                    case MapObjectType.MemoTextBox:
                    case MapObjectType.MemoTip:
                        this.HideMemoProperty();
                        break;
                }

                if (this.selectedGraphicList.Contains(baseGraphic))
                {
                    if (this.editor != null && !this.editor.IsEditing)
                    {
                        this.DeleteVertexSelectedGraphic(baseGraphic as IPointCollectionOwner);
                    }

                    this.selectedGraphicList.Remove(baseGraphic);

                    if (needToRaiseEvent) this.RaiseObjectSelectedEvent();
                }
            }

            //Property
            this.SelectObjectProperty();
        }

        /// <summary>
        /// 하나의 Graphic Select.
        /// 멀티셀렉트가 아니면 모든 선택을 해제하고 선택됨. 
        /// </summary>
        /// <param name="baseGraphic"></param>
        /// <param name="occurEvent"></param>
        private void SelectGraphicObject(BaseGraphic baseGraphic, bool occurEvent = true)
        {
            if (baseGraphic == null) return;

            if (this.isDoingMultiSelect && this.isEditMode)
            {
                if (this.selectedGraphicList.Count > 0 &&
                    (this.selectedGraphicList.OfType<SearchedAddressIconGraphic>().Any() || baseGraphic is SearchedAddressIconGraphic))
                {
                    foreach (var searchedAddressGraphic in this.selectedGraphicList.OfType<SearchedAddressIconGraphic>().ToList())
                    {
                        this.UnSelectGraphicObject(searchedAddressGraphic);
                    }
                }
                if (this.IsSelectedGraphicObject(baseGraphic))
                {
                    this.UnSelectGraphicObject(baseGraphic);

                    this.DeleteVertexSelectedGraphic(baseGraphic as IPointCollectionOwner);

                    return;
                }
            }
            else
            {
                if (!this.selectedGraphicList.Contains(baseGraphic))
                {
                    this.ReleaseAllSelectedObject(baseGraphic is GraphicEditingMarkerGraphic == false);
                }
            }

            this.ChangeGraphicSelection(baseGraphic);

            if (occurEvent)
            {
                var type = ArcGISDataConvertHelper.IsCameraGraphic(baseGraphic.Type) ? MapObjectType.Camera : baseGraphic.Type;

                if (type == MapObjectType.SplunkControl || type == MapObjectType.SplunkIcon) type = MapObjectType.Splunk;

                this.RaiseObjectSelectedEvent();
            }
        }

        /// <summary>
        /// 그래픽을 선택상태로 만든다.
        /// </summary>
        /// <param name="baseGraphic"></param>
        private void ChangeGraphicSelection(BaseGraphic baseGraphic)
        {
            if (_GraphicContextMenu.Visibility == System.Windows.Visibility.Visible)
            {
                _GraphicContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            }

            switch (baseGraphic.Type)
            {
                case MapObjectType.CameraIcon:
                    this.selectedGraphicList.RemoveAll(g => g.ObjectID == baseGraphic.ObjectID);

                    foreach (var graphic in this.GetOneCameraComponentGraphicsInGraphicLayer(baseGraphic.ObjectID))
                    {
                        if (IsConsoleMode && graphic.Type == MapObjectType.CameraIcon)
                        {
                            continue;
                        }

                        graphic.SelectFlag = true;

                        this.AddVertexSelectedGraphic(graphic as IPointCollectionOwner);
                    }

                    break;

                case MapObjectType.Location:
                case MapObjectType.Address:
                    baseGraphic.SelectFlag = true;
                    this.ShowLocationPoupWindow(baseGraphic as IconGraphic);
                    break;

                case MapObjectType.SearchedAddress:
                    baseGraphic.SelectFlag = true;
                    this.ShowSearchedAddressPopupWindow(baseGraphic as SearchedAddressIconGraphic);
                    break;

                case MapObjectType.SplunkIcon:
                case MapObjectType.SplunkControl:
                    baseGraphic.SelectFlag = true;
                    var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(baseGraphic.ObjectID, MapObjectType.SplunkControl) as PolygonControlGraphic<SplunkChartTableWrapperControl>;
                    Debug.Assert(splunkControlGraphic != null);
                    splunkControlGraphic.SelectFlag = true;
                    splunkControlGraphic.Control.IsSelected = true;
                    if (this.selectedGraphicList.Count == 0 && baseGraphic.Type == MapObjectType.SplunkControl)
                    {
                        this.StartEditWithGraphicEditor(baseGraphic);
                    }
                    else
                    {
                        this.AddVertexSelectedGraphic(splunkControlGraphic);
                    }
                    break;

                case MapObjectType.UniversalIcon:
                    var universalControlGraphic = this.universalObjectDataManager.GetControlGraphic(baseGraphic.ObjectID);
                    baseGraphic = universalControlGraphic; // UniversalIconGraphic이 선택되는 일이 없도록 한다.
                    goto default;

                default:
                    baseGraphic.SelectFlag = true;
                    if (this.selectedGraphicList.Count == 0)
                    {
                        this.StartEditWithGraphicEditor(baseGraphic);
                    }
                    else if (this.selectedGraphicList.Count == 1 && this.selectedGraphicList[0] == baseGraphic)  // 같은 오브젝트를 더블클릭으로 선택했을 경우
                    {
                        return;
                    }
                    else
                    {
                        if (this.editor != null)
                        {
                            this.AddVertexSelectedGraphic(this.editor.ActivatedGraphic as IPointCollectionOwner);
                            this.editor.Stop();
                        }

                        this.graphicEditorManager.StopEditing(this.DeleteGraphic);
                    }
                    break;
            }

            if (!this.selectedGraphicList.Contains(baseGraphic))
            {
                this.selectedGraphicList.Add(baseGraphic);

                if(this.selectedGraphicList.Count > 1 ) this.AddVertexSelectedGraphic(baseGraphic as IPointCollectionOwner);
            }

            if (this.IsConsoleMode) return;
            this.SelectObjectProperty();
        }

        /// <summary>
        /// Multi Select 상황에서 
        /// 현재 Select 된  
        /// </summary>
        /// <param name="baseGraphic"></param>
        /// <returns></returns>
        private bool IsSelectedGraphicObject(BaseGraphic baseGraphic)
        {
            //Camera ICon은 Camera 모두를 Select 한다.
            bool isSelectedGraphic = this.selectedGraphicList.Any(
                    g => g.ObjectID == baseGraphic.ObjectID &&
                     (g.Type == baseGraphic.Type || g.Type == MapObjectType.CameraIcon || g.Type == MapObjectType.SplunkControl || g.Type == MapObjectType.SplunkIcon));

            //Preset의 경우 하나의 Camrea에 여러개가 있으므로 재 Select 해준다.
            if (isSelectedGraphic && baseGraphic is CameraPresetGraphic)
            {
                isSelectedGraphic = this.selectedGraphicList.Any(
                    g => (g is CameraPresetGraphic &&
                          g.ObjectID == baseGraphic.ObjectID &&
                          (g as CameraPresetGraphic).PresetIndex == (baseGraphic as CameraPresetGraphic).PresetIndex)
                         ||
                         (g is CameraIconGraphic &&
                          g.ObjectID == baseGraphic.ObjectID));
            }

            if (isSelectedGraphic)
            {
                //Camera List 중에 Icon 이 선택되어 있을 경우에는 다른 Graphic 클릭해도 Icon 선택 해제되도록 한다.
                if (ArcGISDataConvertHelper.IsCameraGraphic(baseGraphic.Type) && baseGraphic.Type != MapObjectType.CameraIcon)
                {
                    var cameraIconGraphic = this.selectedGraphicList.FirstOrDefault(
                        g => g.ObjectID == baseGraphic.ObjectID && g.Type == MapObjectType.CameraIcon);

                    if (cameraIconGraphic != null) baseGraphic = cameraIconGraphic;
                }

                this.UnSelectGraphicObject(baseGraphic);
            }

            return isSelectedGraphic;
        }

        private void AddVertexSelectedGraphic(IPointCollectionOwner pointCollectionOwner)
        {
            if (pointCollectionOwner == null || !this.isEditMode) return;
            pointCollectionOwner.SetVertexIconGraphics(pointCollectionOwner.PointCollection);
            foreach (var graphic in pointCollectionOwner.VertexIconGraphics)
            {
                if (!this.objectGraphicLayer.Contains(graphic)) this.objectGraphicLayer.Graphics.Add(graphic);
            }
        }

        /// <summary>
        /// 현재 선택된 LinkZone의 Vertex를 지운다
        /// </summary>
        /// <param name="pointCollectionOwner"></param>
        private void DeleteVertexSelectedGraphic(IPointCollectionOwner pointCollectionOwner)
        {
            if (pointCollectionOwner == null || pointCollectionOwner.VertexIconGraphics == null) return;
            foreach (var graphic in pointCollectionOwner.VertexIconGraphics)
            {
                if (this.objectGraphicLayer.Contains(graphic)) this.objectGraphicLayer.Graphics.Remove(graphic);
            }
        }

        #endregion //Select/UnSelect Graphic Methods

        /// <summary>
        /// Popu Drag 되어 있던 세팅들을 해제 한다.
        /// </summary>
        private void ReleaseObjectDragSetting()
        {
            this.RefreshCameraPopupControlVideo();

            if (this.Cursor == CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandHold))
                this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen));

            if (this.dynamicSplunkControlManager.IsShow)
            {
                this.ChangeCursor(Cursors.Arrow);
            }
        }

        /// <summary>
        /// 선택된 그래픽을 모두 Move 시킨다.
        /// </summary>
        /// <param name="displacement"></param>
        private void MoveGraphicsPosition(Vector displacement)
        {
            if (this.graphicEditorManager.IsEditing)
            {
                if (this.selectedGraphicList.Any(g => g is GraphicEditingMarkerGraphic) == false)
                {
                    var moveState = MoveState.Default;
                    if (this.selectedGraphicList.Count == 1 && this.selectedGraphicList[0] is TextBoxControlGraphic)
                        moveState = MoveState.WordBalloonRectangle;

                    this.graphicEditorManager.MoveEditor(displacement, moveState);
                }
            }

            foreach (var currentGraphic in this.selectedGraphicList)
            {
                // 그래픽이 Lock상태 라면 Move하지 않고 그냥 재낀다.     [2015. 01. 28 엄태영]
                if (currentGraphic.IsLocked) continue;

                //선택된 Graphic 체크 
                //LinkZone 일 경우는 GraphicEditor 사용
                if (currentGraphic == null)
                    return;

                if (!this.isEditMode)
                {
                    if (!this.IsAlwaysMovableGraphic(currentGraphic))
                        continue;
                }

                switch (currentGraphic.Type)
                {
                    case MapObjectType.CameraIcon:
                        this.MoveGraphicPosition(currentGraphic, displacement);


                        /*
                        var mapCameraObjectComponentDataInfo
                                = this.cameraGraphicDataManager.GetObjectDataByObjectID(currentGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                        if (mapCameraObjectComponentDataInfo == null)
                            return;

                        var cameraGraphicComponent = this.GetOneCameraComponentGraphicsInGraphicLayer(currentGraphic.ObjectID);

                        foreach (var selectedGraphic in cameraGraphicComponent)
                        {
                            switch (selectedGraphic.Type)
                            {
                                case MapObjectType.CameraPresetEditorMarker:
                                case MapObjectType.CameraNameTextBox:
                                    break;
                                case MapObjectType.CameraIcon:

                                    var cameraIconGraphic = selectedGraphic as CameraIconGraphic;

                                    this.MoveGraphicPosition(selectedGraphic, displacement);

                                    if (cameraIconGraphic != null)
                                    {
                                        cameraIconGraphic.CameraNameTextBoxGraphic.Geometry = cameraIconGraphic.Geometry;
                                    }
                                    break;
                                default:
                                    MoveGraphicPosition(selectedGraphic, displacement);
                                    break;
                            }
                        }
                         * */
                        break;
                    case MapObjectType.CameraNameTextBox:
                    case MapObjectType.CameraPresetPlus:
                        break;
                    case MapObjectType.SplunkIcon:
                        this.MoveGraphicPosition(currentGraphic, displacement);
                        var splunkControlGraphic = this.GetOneBaseGraphicInGraphicLayer(currentGraphic.ObjectID, MapObjectType.SplunkControl);
                        this.MoveGraphicPosition(splunkControlGraphic, displacement);
                        break;
                    case MapObjectType.MemoTextBox:
                        var memoTipGraphic = this.memoObjectDataManager.GetMemoTipGraphic(currentGraphic.ObjectID);
                        this.MoveGraphicPosition(currentGraphic, displacement);
                        memoTipGraphic.PointCollection = MemoObjectDataManager.GetTipBoundary(
                            memoTipGraphic.TipPosition,
                            (currentGraphic as TextBoxControlGraphic).PointCollection
                        );
                        break;
                    case MapObjectType.MemoTip:
                        var TextBoxControlGraphic = this.memoObjectDataManager.GetTextBoxControlGraphic(currentGraphic.ObjectID);
                        this.MoveGraphicPosition(TextBoxControlGraphic, displacement);
                        this.MoveGraphicPosition(currentGraphic, displacement);
                        break;
                    default:
                        MoveGraphicPosition(currentGraphic, displacement);
                        break;
                }
            }

            if (this.selectedGraphicList.Count > 1)
            {
                if (this.editor != null)
                {

                    try
                    {
                        this.editor.Stop();
                    }
                    catch (Exception ex)
                    {
                        InnowatchDebug.Logger.WriteLine(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Graphic Object의 위치를 변경 한다.
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="displacement"></param>
        private void MoveGraphicPosition(BaseGraphic graphic, Vector displacement, bool CameraGraphicMoveLoop = false)
        {
            if (graphic.Type == MapObjectType.CameraIcon && CameraGraphicMoveLoop == false)
            {
                var mapCameraObjectComponentDataInfo
                                = this.cameraGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID) as MapCameraObjectComponentDataInfo;

                if (mapCameraObjectComponentDataInfo == null)
                    return;

                var cameraGraphicComponent = this.GetOneCameraComponentGraphicsInGraphicLayer(graphic.ObjectID);

                foreach (var selectedGraphic in cameraGraphicComponent)
                {
                    switch (selectedGraphic.Type)
                    {
                        case MapObjectType.CameraPresetEditorMarker:
                        case MapObjectType.CameraNameTextBox:
                            break;
                        case MapObjectType.CameraIcon:

                            var cameraIconGraphic = selectedGraphic as CameraIconGraphic;

                            this.MoveGraphicPosition(selectedGraphic, displacement, true);

                            if (cameraIconGraphic != null)
                            {
                                cameraIconGraphic.CameraNameTextBoxGraphic.Geometry = cameraIconGraphic.Geometry;
                            }
                            break;
                        default:
                            MoveGraphicPosition(selectedGraphic, displacement);
                            break;
                    }
                }

                return;
            }

            if (graphic is IPositionOwner)
            {
                var positionOwnerGraphic = graphic as IPositionOwner;

                var currentPoint = positionOwnerGraphic.Position;
                var newPosition = currentPoint + displacement;

                positionOwnerGraphic.Position = newPosition;

                this.SetObjectPosition(graphic, newPosition);
            }
            else
            {
                var polygon = graphic as IPointCollectionOwner;
                if (polygon == null) return;
                var movePointCollection = this.GetMovedPosition(polygon, displacement);
                polygon.PointCollection = movePointCollection;
                this.SetObjectPosition(graphic, movePointCollection);
                if (graphic is CameraVideoGraphic) this.RefreshGraphicObjectVideo(graphic as CameraVideoGraphic);

                polygon.SetVertexIconGraphics(displacement);
            }
        }

        /// <summary>
        /// 실제 DB에 저장될 Object의 위치 값도 Refresh 해주어야 한다.
        /// MapPoint로 되어 있는 Object
        /// </summary>
        /// <param name="baseGraphic"></param>
        /// <param name="movedPosition"></param>
        private void SetObjectPosition(BaseGraphic baseGraphic, Point movedPosition)
        {
            if (baseGraphic == null || movedPosition == new Point(0, 0))
                return;

            switch (baseGraphic.Type)
            {
                case MapObjectType.CameraIcon:
                    var cameraObjectComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(baseGraphic.ObjectID) as MapCameraObjectComponentDataInfo;
                    if (cameraObjectComponentData != null)
                    {
                        cameraObjectComponentData.CameraIcon.Position = movedPosition;
                        cameraObjectComponentData.SetCameraComponentBounds();
                    }
                    break;
                case MapObjectType.Address:
                    var addressObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapAddressObjectDataInfo;
                    if (addressObjectData != null) addressObjectData.Position = movedPosition;
                    break;
                case MapObjectType.Location:
                    var locationObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapLocationObjectDataInfo;
                    if (locationObjectData != null) locationObjectData.Position = movedPosition;
                    break;
                case MapObjectType.SplunkIcon:
                    var splunkObjectData = this.GetSplunkData(baseGraphic.ObjectID);
                    if (splunkObjectData != null) splunkObjectData.IconPosition = movedPosition;
                    break;
            }
            if (baseGraphic.Type == MapObjectType.CameraIcon)
            {
                InnowatchDebug.Logger.Trace("N Branching Statement Processing - if (baseGraphic.Type == MapObjectType.CameraIcon)");
            }
            else if (!ArcGISDataConvertHelper.IsCameraGraphic(baseGraphic.Type))
            {
                InnowatchDebug.Logger.Trace("N Branching Statement Processing - else if (!ArcGISDataConvertHelper.IsCameraGraphic(baseGraphic.Type))");
            }
        }

        /// <summary>
        /// 실제 DB에 저장될 Object의 위치 값도 Refresh 해주어야 한다.
        /// Polygon으로 되어 있는 Object
        /// </summary>
        /// <param name="baseGraphic"></param>
        /// <param name="pointList"></param>
        private void SetObjectPosition(BaseGraphic baseGraphic, List<Point> pointList)
        {
            if (baseGraphic == null || pointList == null)
                return;

            if (ArcGISDataConvertHelper.IsCameraGraphic(baseGraphic.Type))
            {
                var cameraObjectComponentData = this.cameraGraphicDataManager.GetObjectDataByObjectID(baseGraphic.ObjectID) as MapCameraObjectComponentDataInfo;

                if (cameraObjectComponentData == null) return;

                switch (baseGraphic.Type)
                {
                    case MapObjectType.CameraVideo:
                        cameraObjectComponentData.Video.PointCollection = pointList;
                        cameraObjectComponentData.SetCameraComponentBounds();
                        break;

                    case MapObjectType.CameraPreset:
                        var cameraPresetGraphic = baseGraphic as CameraPresetGraphic;
                        if (cameraPresetGraphic != null)
                        {
                            try
                            {
                                cameraObjectComponentData.PresetDatas[cameraPresetGraphic.PresetIndex].PointCollection = pointList;
                                cameraObjectComponentData.SetCameraComponentBounds();
                            }
                            catch (Exception exception)
                            {
                                InnowatchDebug.Logger.Trace(exception.ToString());
                            }
                        }

                        break;
                }
            }
            else
            {
                switch (baseGraphic.Type)
                {
                    case MapObjectType.LinkZone:
                    case MapObjectType.ImageLinkZone:
                        var linkZoneObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapLinkZoneObjectDataInfo;
                        if (linkZoneObjectData != null) linkZoneObjectData.PointCollection = pointList;
                        break;
                    case MapObjectType.Workstation:
                        var workstationObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapWorkStationObjectDataInfo;
                        if (workstationObjectData != null) workstationObjectData.PointCollection = pointList;
                        break;
                    case MapObjectType.SplunkControl:
                        var splunkObjectData = this.GetSplunkData(baseGraphic.ObjectID);
                        if (splunkObjectData != null)
                        {
                            splunkObjectData.PointCollection = pointList;
                            splunkObjectData.SetSplunkBounds();
                        }
                        break;
                    case MapObjectType.Line:
                        var lineObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapLineObjectDataInfo;
                        if (lineObjectData != null) lineObjectData.PointCollection = pointList;
                        break;
                    case MapObjectType.Image:
                        var imageObjectData = this.GetObjectData(baseGraphic.ObjectID, baseGraphic.Type) as MapImageObjectDataInfo;
                        if (imageObjectData != null) imageObjectData.PointCollection = pointList;
                        break;
                }

            }
        }


        /// <summary>
        /// Address Icon Popup
        /// </summary>
        /// <param name="searchedAddressIconGraphic"></param>
        private void ShowSearchedAddressPopupWindow(SearchedAddressIconGraphic searchedAddressIconGraphic)
        {
            if (searchedAddressIconGraphic == null) return;

            var addressObjectData = this.searchAddressGraphicDataManager.GetObjectDataByObjectID(searchedAddressIconGraphic.ObjectID);

            if (this.searchedAddressInfoWindowManager == null || addressObjectData == null || searchedAddressIconGraphic == null) return;

            var point = searchedAddressIconGraphic.Geometry as MapPoint;

            this.searchedAddressInfoWindowManager.ShowInfoWindow(addressObjectData, point);
        }

        /// <summary>
        /// Extent가 바뀌었다는 이벤트가 날아왔을 때 LinkZone 더블클릭시 마무리 처리
        /// </summary>
        private void ExtentChangedLinkZoneDoubleClick()
        {
            var data = Interlocked.Exchange(ref doubleClickLinkZoneObjectData, null);
            if (data == null)
                return;

            this.baseMap.ZoomDuration = ArcGISConstSet.ZoomDuration;
            this.ChangeMapUsingLinkZone(data);
        }

        /// <summary>
        /// 링크존 정보에 의해 맵을 바꾸는 경우.
        /// 더블클릭하거나 휠로 들어가는 경우에 바로 화면 전환하기 위해 처리할 내용이다.
        /// </summary>
        /// <param name="linkZoneObjectData">링크존 정보</param>
        /// <param name="extentToRecord">히스토리에서 현재로 복귀했을 때 사용할 extent</param>
        /// <returns>성공 여부 반환</returns>
        private bool ChangeMapUsingLinkZone(LinkZoneZoomInfo data)
        {
            string linkedMapGuid = null;
            string linkedMapBookmarkName = null;
            string linkedMapObjectName = null;
            var extentToRecord = data.TargetExtent;

            if (data.LinkZoneData != null)
            {
                linkedMapGuid = data.LinkZoneData.LinkedMapGuid;
                linkedMapBookmarkName = data.LinkZoneData.LinkedMapBookmarkName;
            }
            else if (data.UniversalData != null)
            {
                linkedMapGuid = data.UniversalData.LinkedMapGuid;
                linkedMapBookmarkName = data.UniversalData.LinkedMapBookmarkName;
                linkedMapObjectName = data.UniversalData.LinkedMapObjectName;
            }

            if (linkedMapGuid == null)
            {
                //맵의 권한이 없다는 메세지 출력되지 않음
                this.arcGISControlApi.ShowMessagePopup(Resource_ArcGISControl_ArcGISClientViewer.Message_DidNotSetLinkedMap, true);

                return false;
            }

            //Map Changed
            if (this.ChangeSelectedLinkedMap(linkedMapGuid, null, true, linkedMapBookmarkName, linkedMapObjectName))
            {
                if (extentToRecord != null)
                {
                    // 현재 위치의 extent 기억시키기
                    var currentHistoryInfo = this.linkedMapHistory.Current;
                    currentHistoryInfo.Extent = extentToRecord;
                    this.linkedMapHistory.Current = currentHistoryInfo;
                }

                this.linkedMapHistory.NewLocation(new MapHistoryEntity(linkedMapGuid, null));
            }
            else
            {
                //맵 권한 없을 시 표시 될 메세지 박스
                this.arcGISControlApi.ShowMessagePopup(Resource_ArcGISControl_ArcGISClientViewer.Message_DoesNotHavePermission, true);

                return false;
            }

            return true;
        }

        private void CheckLinkZoneOnWheel(MouseWheelEventArgs mouseWheelEventArgs, Map map)
        {
            var graphics = GraphicTool.GetGraphicsFromPoint(map, mouseWheelEventArgs.GetPosition(map));
            foreach (var graphic in graphics.OfType<BaseGraphic>())
            {
                LinkZoneZoomInfo zoomInfo;
                BaseMapObjectInfoData dataInfo;

                switch (graphic.Type)
                {
                    case MapObjectType.LinkZone:
                    case MapObjectType.ImageLinkZone:
                        var linkZoneDataInfo = this.publicGraphicDataManager.GetObjectDataByObjectID(graphic.ObjectID, graphic.Type) as MapLinkZoneObjectDataInfo;
                        zoomInfo = new LinkZoneZoomInfo() { LinkZoneData = linkZoneDataInfo };
                        dataInfo = linkZoneDataInfo;
                        break;

                    case MapObjectType.UniversalControl:
                        var universalDataInfo = this.universalObjectDataManager.GetDataInfo(graphic.ObjectID);
                        zoomInfo = new LinkZoneZoomInfo() { UniversalData = universalDataInfo };
                        dataInfo = universalDataInfo;
                        break;

                    default:
                        continue;
                }

                if (!this.CanLinkZoneBeExecutedByWheel(map, graphic, mouseWheelEventArgs))
                    continue;

                var objectExtent = new Envelope(
                    dataInfo.ExtentMin.X,
                    dataInfo.ExtentMin.Y,
                    dataInfo.ExtentMax.X,
                    dataInfo.ExtentMax.Y
                );
                zoomInfo.TargetExtent = objectExtent;

                if (this.ChangeMapUsingLinkZone(zoomInfo))
                {
                    // map에 마우스 휠이 발생했다는 사실을 전달하지 않는다.
                    mouseWheelEventArgs.Handled = true;
                    return;
                }
            }
        }

        protected void BeginLinkZoneZoom(Map map, MapLinkZoneObjectDataInfo linkZoneData)
        {
            var objectExtent = new Envelope(
                linkZoneData.ExtentMin.X,
                linkZoneData.ExtentMin.Y,
                linkZoneData.ExtentMax.X,
                linkZoneData.ExtentMax.Y
                );

            this.doubleClickLinkZoneObjectData = new LinkZoneZoomInfo()
            {
                LinkZoneData = linkZoneData,
                PrevExtent = map.Extent,
                TargetExtent = objectExtent
            };
            this.baseMap.ZoomDuration = ArcGISConstSet.ZoomDurationDuringLinkZoneDoubleClick;
            map.ForceZoomTo(objectExtent, false);
        }

        protected void BeginLinkZoneZoom(Map map, MapUniversalObjectDataInfo universalData)
        {
            var objectExtent = new Envelope(
                universalData.ExtentMin.X,
                universalData.ExtentMin.Y,
                universalData.ExtentMax.X,
                universalData.ExtentMax.Y
                );

            this.doubleClickLinkZoneObjectData = new LinkZoneZoomInfo()
            {
                UniversalData = universalData,
                PrevExtent = map.Extent,
                TargetExtent = objectExtent
            };
            this.baseMap.ZoomDuration = ArcGISConstSet.ZoomDurationDuringLinkZoneDoubleClick;
            map.ForceZoomTo(objectExtent, false);
        }

        private bool CanLinkZoneBeExecutedByWheel(Map map, BaseGraphic graphic, MouseWheelEventArgs mouseWheelEventArgs)
        {
            var mapExtent = map.Extent;
            if (mapExtent == null)
                return false;

            var intersection = mapExtent.Intersection(graphic.Geometry.Extent);

            if (map.WrapAroundIsActive)
            {
                var tileLayer = this.baseTiledMapServiceLayer;
                if (tileLayer != null)
                {
                    var xmin = tileLayer.FullExtent.XMin;
                    var xmax = tileLayer.FullExtent.XMax;
                    var xDelta = -Math.Floor((mapExtent.XMin - xmin) / (xmax - xmin)) * (xmax - xmin);
                    var xDelta2 = -Math.Floor((graphic.Geometry.Extent.XMin - xmin) / (xmax - xmin)) * (xmax - xmin);

                    if (!GeometryHelper.IsSimilar(xDelta, xDelta2))
                    {
                        var newMapExtent = mapExtent.Clone();
                        newMapExtent.XMin += xDelta - xDelta2;
                        newMapExtent.XMax += xDelta - xDelta2;

                        intersection = newMapExtent.Intersection(graphic.Geometry.Extent);
                    }
                }
            }

            if (intersection == null)
                return false;

            var intersectionArea = GeometryHelper.Area(intersection);
            var mapArea = GeometryHelper.Area(mapExtent);

            return intersectionArea / mapArea >= 0.5;
        }

        private void GoPreviousOnWheel(MouseWheelEventArgs mouseWheelEventArgs, Map map)
        {
            var baseLayer = this.baseTiledMapServiceLayer;
            if (baseLayer == null)
                return;

            var baseLayerFullExtent = baseLayer.FullExtent;
            if (baseLayerFullExtent == null)
                return;

            var mapExtent = map.Extent;

            // home extent 지정 체크 루틴
            var isFullConstrainedExtent = false;
            {
                var constraint = this.GetStrictExtentSizeConstraint();
                if (GeometryHelper.IsLessOrEqualTo(constraint.Width, mapExtent.Width) &&
                    GeometryHelper.IsLessOrEqualTo(constraint.Height, mapExtent.Height))
                {
                    isFullConstrainedExtent = true;
                }

                if (map.WrapAroundIsActive)
                {
                    var tileLayer = this.baseTiledMapServiceLayer;
                    if (tileLayer != null)
                    {
                        var fullExtent = tileLayer.FullExtent;
                        //if (GeometryHelper.IsSimilar(fullExtent.Width, mapExtent.Width))
                        if (GeometryHelper.IsLessOrEqualTo(fullExtent.Width, mapExtent.Width))  // ArcGIS 맵의 전체 사이즈와 현재 1레벨일 때의 전체 사이즈의 차의 값이 심하게 나와 IsLessOrEqualTo로 비교    [2014. 04. 08. 25 엄태영]
                        {
                            isFullConstrainedExtent = true;
                        }
                    }
                }
            }

            if (isFullConstrainedExtent)
            {
                var nextHistory = this.linkedMapHistory.TryGoBack();

                if (nextHistory.HasValue)
                {
                    if (this.ChangeSelectedLinkedMap(nextHistory.Value.MapId, nextHistory.Value.Extent, true))
                    {
                        // map으로 wheel 되었다는 사실을 전달하지 않는다.
                        mouseWheelEventArgs.Handled = true;
                    }
                    else
                    {
                        this.linkedMapHistory.CancelMove();
                    }
                }
            }
        }

        private void ProcessNextTransitionChain()
        {
            var chain = this.mapTransitionChain;
            string nextMapName;
            if (chain == null || !chain.TryDequeue(out nextMapName))
                return;

            MapSettingDataInfo mapSettingDataInfo = null;

            if (this.arcGISControlApi != null)
            {
                mapSettingDataInfo = this.arcGISControlApi.GetMapSettingByName(nextMapName);
            }

            if (mapSettingDataInfo != null && !string.IsNullOrEmpty(mapSettingDataInfo.ID))
            {
                var linkZones = this.publicGraphicDataManager.GetLinkZoneObjectDatas
                    .Concat(this.privateGraphicDataManager.GetLinkZoneObjectDatas)
                    .OfType<MapLinkZoneObjectDataInfo>();
                foreach (var linkZone in linkZones)
                {
                    if (linkZone.LinkedMapGuid == mapSettingDataInfo.ID)
                    {
                        this.BeginLinkZoneZoom(this.baseMap, linkZone);
                        return;
                    }
                }

                // not found any.
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    this.initialExtent = null;
                    this.mapSettingDataInfo = mapSettingDataInfo;
                    this.MapBackgroundColor = mapSettingDataInfo.MapBgColor;
                    this.RefreshMapTiles(true);
                }));

                this.linkedMapHistory.NewLocation(new MapHistoryEntity(mapSettingDataInfo.ID, this.baseMap.Extent));
                return;
            }

            // 이동에 실패, 더 이상 해당 링크존 이동 작동하지 않게 수정
            this.mapTransitionChain = null;

            var arcGisControlApi = this.arcGISControlApi;

            if (arcGisControlApi != null)
            {
                arcGisControlApi.ShowMessagePopup(
                       string.Format(Resource_ArcGISControl_ArcGISClientViewer.Message_CanNotFindMapName_LinkedMap, nextMapName),
                       true);
            }
        }

        private void StartEditWithGraphicEditor(BaseGraphic graphic)
        {
            if (this.selectedGraphicList.Count > 1 || this.editor == null) return;

            try
            {
                switch (graphic.Type)
                {
                    case MapObjectType.CameraPreset:
                        this.editor.EditSnapVertexEnabled = false;
                        this.editor.EditDragVertexEnabled = true;
                        break;
                    case MapObjectType.Line:
                        this.editor.EditSnapVertexEnabled = true;
                        this.editor.EditDragVertexEnabled = true;
                        break;
                    default:
                        this.editor.EditSnapVertexEnabled = false;
                        this.editor.EditDragVertexEnabled = false;
                        break;
                }
                this.editor.Start(graphic);
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(e, "StartEditWithGraphicEditor", true);
            }
        }

        #region LocationGraphic MouseLeftButtonDown Event Method

        private void DoViewModeSplunkIconMouseLeftButtonDown(BaseGraphic selectedGraphic)
        {
            if (this.splunkPopupControlManager.ShowingSplunkId == selectedGraphic.ObjectID)
            {
                this.splunkPopupControlManager.Hide();
            }
            else
            {
                var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(selectedGraphic.ObjectID);

                if (splunkControl != null)
                {
                    var position = new Point((this.GetWrapAroundPoint(selectedGraphic.Geometry.Extent.XMin) - baseMap.Extent.XMin) / baseMap.Resolution,
                                                (baseMap.Extent.YMax - selectedGraphic.Geometry.Extent.YMax) / baseMap.Resolution);



                    this.splunkPopupControlManager.Show(position, splunkControl);

                }
            }
        }

        private void DoViewModeWorkStationMouseLeftButtonDown(BaseGraphic selectedGraphic)
        {
            var workstationDataInfo = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type) as MapWorkStationObjectDataInfo;
            this.workStationControlManager.Show(workstationDataInfo);
        }

        private void DoViewModeLinkZoneMouseLeftButtonDown(MouseButtonEventArgs mouseButtonEventArgs, BaseGraphic selectedGraphic)
        {
            var linkZoneObjectData = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type) as MapLinkZoneObjectDataInfo;

            if (mouseButtonEventArgs.ClickCount == 2)
            {
                this.isLinkZoneDoubleClicked = true;

                this.ChangeCursor(Cursors.Arrow);

                if (linkZoneObjectData == null || linkZoneObjectData.LinkedMapGuid == null)
                {
                    InnowatchDebug.Logger.Trace("if (linkZoneObjectData == null || linkZoneObjectData.LinkedMapGuid == null)");
                    // 링크 맵 없을 때 뜨는 메시지 박스 제거
                    //var arcGisControlApi = this.arcGISControlApi;

                    //if (arcGisControlApi != null)
                    //{
                    //    arcGisControlApi.ShowMessagePopup(
                    //           Resource_ArcGISControl_ArcGISClientViewer.Message_DidNotSetLinkedMap,
                    //           true);
                    //}
                }
                else
                {
                    this.dynamicSplunkControlManager.Hide();

                    this.BeginLinkZoneZoom(this.baseMap, linkZoneObjectData);
                    // 이후 작업은 ExtentChangedLinkZoneDoubleClick()에서 처리
                }
            }
            else if (mouseButtonEventArgs.ClickCount > 2)
            {
                mouseButtonEventArgs.Handled = true;
            }
            else
            {
                //LinkZone 에서는 Panning이 되도록 한다.
                this.isLinkZoneDoubleClicked = false;
                mouseButtonEventArgs.Handled = false;
            }
        }

        private void DoViewModeUniversalIconMouseLeftButtonDown(BaseGraphic selectedGraphic)
        {
            if (this.splunkPopupControlManager.ShowingSplunkId == selectedGraphic.ObjectID)
            {
                this.splunkPopupControlManager.Hide();
            }
            else
            {
                var position = new Point(
                   (this.GetWrapAroundPoint(selectedGraphic.Geometry.Extent.XMin) - baseMap.Extent.XMin) / baseMap.Resolution,
                   (baseMap.Extent.YMax - selectedGraphic.Geometry.Extent.YMax) / baseMap.Resolution
               );

                if (this.isRequestingMapSpl)
                {
                    this.splunkPopupControlManager.ShowLoading(position, selectedGraphic.ObjectID, "Metrics");
                }
                else
                {
                    var metric = this.universalObjectDataManager.GetMetric(selectedGraphic.ObjectID);
                    this.splunkPopupControlManager.Show(position, selectedGraphic.ObjectID, metric, "Metrics", this.universalObjectDataManager.CellLinkTable);
                }
            }
        }

        private void DoViewModeUniversalControlMouseLeftButtonDown(MouseButtonEventArgs mouseButtonEventArgs, BaseGraphic selectedGraphic)
        {
            var dataInfo = this.universalObjectDataManager.GetDataInfo(selectedGraphic.ObjectID);

            if (mouseButtonEventArgs.ClickCount == 2)
            {
                this.isLinkZoneDoubleClicked = true;

                this.ChangeCursor(Cursors.Arrow);

                if (dataInfo != null)
                {
                    // 링크 정보 없어도 대상 오브젝트로 줌인 까지 동작하도록 수정함. 
                    this.dynamicSplunkControlManager.Hide();

                    this.BeginLinkZoneZoom(this.baseMap, dataInfo);

                    // 이 후 맵 링크 이동 작업은 ExtentChangedLinkZoneDoubleClick()에서 처리
                }
            }
            else if (mouseButtonEventArgs.ClickCount > 2)
            {
                mouseButtonEventArgs.Handled = true;
            }
            else
            {
                //UniversalControl 에서는 Panning이 되도록 한다.
                this.isLinkZoneDoubleClicked = false;
                mouseButtonEventArgs.Handled = false;
            }
        }

        #endregion LocationGraphic MouseLeftButtonDown Event Method

        #region LocationGraphic MouseMove Event Method

        private void DoSplunkControlMouseMove(MouseEventArgs mouseEventArgs, BaseGraphic currentGraphic)
        {
            if (this.isEditMode == true)
            {
                //선택된 위치 저장
                var currentMapPointByClick = baseMap.ScreenToMap(mouseEventArgs.GetPosition(baseMap));
                bool left = false, top = false, right = false, bottom = false;

                var minX = this.GetWrapAroundPoint(currentGraphic.Geometry.Extent.XMin);

                //좌측 좌표
                if (currentMapPointByClick.X <
                    minX + (ResizingBorderWidth * this.baseMap.Resolution))
                {
                    left = true;
                }

                //우측 좌표
                if (currentMapPointByClick.X >
                    (minX + currentGraphic.Geometry.Extent.Width) - (ResizingBorderWidth * this.baseMap.Resolution))
                {
                    right = true;
                }

                //하단 좌표
                if (currentMapPointByClick.Y <
                    currentGraphic.Geometry.Extent.YMin + (ResizingBorderWidth * this.baseMap.Resolution))
                {
                    bottom = true;
                }

                //상단 좌표
                if (currentMapPointByClick.Y >
                    currentGraphic.Geometry.Extent.YMax - (ResizingBorderWidth * this.baseMap.Resolution))
                {
                    top = true;
                }

                if (left && top)
                {
                    this.ChangeCursor(
                        CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown));
                }
                else if (left && bottom)
                {
                    this.ChangeCursor(
                        CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftDownRightUp));
                }
                else if (right && top)
                {
                    this.ChangeCursor(
                        CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftDownRightUp));
                }
                else if (right && bottom)
                {
                    this.ChangeCursor(
                        CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown));
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
                    this.ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                }
            }
            else
            {
                InnowatchDebug.Logger.Trace("N Branching Statement Processing - if (this.isEditMode == true) else");
            }
        }

        #endregion LocationGraphic MouseMove Event Method

        #region ZIndex

        public void BringSelectedGraphicsToDirect(string bringToDirection)
        {
            this.BringSelectedGraphicsTo((BringToDirection)Enum.Parse(typeof(BringToDirection), bringToDirection));
        }

        #endregion ZIndex

        #region Move Graphic Method

        private bool IsAlwaysMovableGraphic(BaseGraphic graphic)
        {
            return graphic.Type == MapObjectType.GraphicEditingMarker
                || graphic.Type == MapObjectType.MemoTextBox
                || graphic.Type == MapObjectType.MemoTip;
        }

        /// <summary>
        /// PolygonGraphic의 새로운 위치값을 반환한다.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="displacement"></param>
        /// <returns></returns>
        private List<Point> GetMovedPosition(IPointCollectionOwner polygon, Vector displacement)
        {
            var movePointCollection = new List<Point>();

            if (polygon == null) return movePointCollection;

            var tmpPointCollection = polygon.PointCollection;

            movePointCollection.AddRange(
                tmpPointCollection.Select(tmp => tmp + displacement));

            return movePointCollection;
        }

        /// <summary>
        /// IconGraphic의 새로운 위치값을 반환한다.
        /// </summary>
        /// <param name="currentPoint"></param>
        /// <param name="movedPoint"></param>
        /// <returns></returns>
        private MapPoint GetMovedPosition(MapPoint currentPoint, Point movedPoint)
        {
            var newPoint = new MapPoint(currentPoint.X + movedPoint.X, currentPoint.Y + movedPoint.Y);
            return newPoint;
        }

        #endregion

        #region Alignment

        public enum AlignmentDirections
        {
            Left,
            Top,
            Right,
            Bottom,
            VerticalCenter,
            HorizontalCenter
        }

        /// <summary>
        /// 선택된 오브젝트들의 Alignment를 마지막 선택된 오브젝트에 맞춤
        /// </summary>
        /// <param name="aDirection"></param>
        public void FitAlignment(AlignmentDirections aDirection)
        {
            if (!this.isEditMode)
            {
                return;
            }

            if (this.selectedGraphicList.Count > 1)
            {
                var lastGraphic = this.selectedGraphicList.Last();
                var displacement = new Vector(0, 0);

                var refExtent = lastGraphic.Geometry.Extent;

                double half = 0;
                if (aDirection == AlignmentDirections.VerticalCenter)
                {
                    half = this.CalcHalf(refExtent.XMin, refExtent.XMax);
                }
                else if (aDirection == AlignmentDirections.HorizontalCenter)
                {
                    half = this.CalcHalf(refExtent.YMin, refExtent.YMax);
                }

                for (var i = 0; i < this.selectedGraphicList.Count - 1; i++)
                {
                    var graphic = this.selectedGraphicList[i];

                    switch (aDirection)
                    {
                        case AlignmentDirections.Left:
                            displacement.X = refExtent.XMin - graphic.Geometry.Extent.XMin;
                            break;
                        case AlignmentDirections.Top:
                            displacement.Y = refExtent.YMax - graphic.Geometry.Extent.YMax;
                            break;
                        case AlignmentDirections.Right:
                            displacement.X = refExtent.XMax - graphic.Geometry.Extent.XMax;
                            break;
                        case AlignmentDirections.Bottom:
                            displacement.Y = refExtent.YMin - graphic.Geometry.Extent.YMin;
                            break;
                        case AlignmentDirections.VerticalCenter:
                            displacement.X = half - this.CalcHalf(graphic.Geometry.Extent.XMin, graphic.Geometry.Extent.XMax);
                            break;
                        case AlignmentDirections.HorizontalCenter:
                            displacement.Y = half - this.CalcHalf(graphic.Geometry.Extent.YMin, graphic.Geometry.Extent.YMax);
                            break;
                    }

                    this.MoveGraphicPosition(graphic, displacement);
                }

                this.historyManager.IsChanged = true;
            }
        }

        private double CalcHalf(double aVal1, double aVal2)
        {
            return aVal1 + (aVal2 - aVal1) / 2;
        }

        #endregion // Alignment

        #region Fit Size

        /// <summary>
        /// 선택된 오브젝트들의 사이즈를 마지막 선택된 오브젝트와 동일하게 맞춤.
        /// </summary>
        //public void FitSize()
        //{
        //    if (!this.isEditMode)
        //    {
        //        return;
        //    }
        //}


        #endregion // Fit Size

        #region Memo & Marker

        private void StartMemoEditing(BaseGraphic aMemoGraphic)
        {
            var TextBoxControlGraphic = this.memoObjectDataManager.GetTextBoxControlGraphic(aMemoGraphic.ObjectID);
            var memoTipGraphic = this.memoObjectDataManager.GetMemoTipGraphic(aMemoGraphic.ObjectID);
            TextBoxControlGraphic.SelectFlag = true;
            memoTipGraphic.SelectFlag = true;
            this.ShowMemoProperty(TextBoxControlGraphic.Control.DataInfo as MapMemoObjectDataInfo);
            this.graphicEditorManager.StartWordBalloonEditing(this.SetBaseGraphic, TextBoxControlGraphic, memoTipGraphic);
        }

        private void SelectMarker(BaseGraphic aGraphic)
        {
            this.markerGraphic = aGraphic;

            aGraphic.SelectFlag = true;

            this.mapPointByClick = baseMap.ScreenToMap(Mouse.GetPosition(baseMap));
            this.ChangeCursor(Cursors.Arrow);
        }

        private void MoveMarker()
        {
            if (this.markerGraphic == null || this.mapPointByClick == null) return;

            var newMapPoint = baseMap.ScreenToMap(Mouse.GetPosition(baseMap));
            var displacement = new Vector(newMapPoint.X - mapPointByClick.X, newMapPoint.Y - mapPointByClick.Y);

            this.MoveGraphicPosition(this.markerGraphic, displacement);

            this.mapPointByClick = newMapPoint;
        }

        private void DeselectMarker()
        {
            if (this.markerGraphic == null) return;

            this.markerGraphic.SelectFlag = false;
            this.mapPointByClick = null;
            this.markerGraphic = null;
        }

        #endregion // Memo & Marker

        #endregion //Method

        #region Events Handler

        #region Info Window Event Handlers

        #endregion //Info WIndow Event Handlers

        #region Graphic Event Handlers

        private BaseGraphic markerGraphic;

        /// <summary>
        /// Location Object Graphic Mouse Down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void LocationGraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (this.isDoingNewObjectByClick) return;

            var selectedGraphic = sender as BaseGraphic;
            if (selectedGraphic == null) return;

            if ((this.isEditMode && _IsPanning) || (!this.isEditMode && !this.IsConsoleMode))
            {
                if (selectedGraphic is TextBoxControlGraphic)
                {
                    ((TextBoxControlGraphic)selectedGraphic).Control.IsPanning = _IsPanning;
                }
                return; // Space 누르면 패닝만 된다.
            }

            if (selectedGraphic.Type == MapObjectType.GraphicEditingMarker)
            {
                mouseButtonEventArgs.Handled = true;
                this.SelectMarker(selectedGraphic);
                return;
            }

            //자신 선택
            this.SelectGraphicObject(selectedGraphic);

            mouseButtonEventArgs.Handled = true;

            if (!this.isEditMode && !this.IsAlwaysMovableGraphic(selectedGraphic))
            {
                switch (selectedGraphic.Type)
                {
                    case MapObjectType.Address:
                    case MapObjectType.Location:
                    case MapObjectType.SearchedAddress:
                        this.ChangeCursor(Cursors.Arrow);
                        break;

                    case MapObjectType.ImageLinkZone:
                    case MapObjectType.LinkZone:
                        this.DoViewModeLinkZoneMouseLeftButtonDown(mouseButtonEventArgs, selectedGraphic);
                        break;

                    case MapObjectType.Workstation:
                        this.DoViewModeWorkStationMouseLeftButtonDown(selectedGraphic);
                        break;

                    case MapObjectType.SplunkIcon:
                        this.DoViewModeSplunkIconMouseLeftButtonDown(selectedGraphic);
                        break;

                    case MapObjectType.SplunkControl:
                        this.splunkControlClickPos = Mouse.GetPosition(this.mainCanvas);
                        this.ChangeCursor(Cursors.Arrow);
                        mouseButtonEventArgs.Handled = false;
                        break;

                    case MapObjectType.Line:
                    case MapObjectType.Text:
                    case MapObjectType.Image:
                        this.ChangeCursor(Cursors.Arrow);
                        mouseButtonEventArgs.Handled = false;
                        break;

                    case MapObjectType.UniversalIcon:
                        this.DoViewModeUniversalIconMouseLeftButtonDown(selectedGraphic);
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.UniversalHealth));
                        break;

                    case MapObjectType.UniversalControl:
                        this.DoViewModeUniversalControlMouseLeftButtonDown(mouseButtonEventArgs, selectedGraphic);
                        break;
                }

            }//End of Read Only Mode
            else
            {
                switch (selectedGraphic.Type)
                {
                    case MapObjectType.MemoTextBox:
                    case MapObjectType.MemoTip:
                        if (this.IsReportWritingMode)
                        {
                            this.SetClickPositionDatas();
                            ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                            this.StartMemoEditing(selectedGraphic);
                        }
                        else
                        {
                            mouseButtonEventArgs.Handled = false;
                        }

                        break;

                    case MapObjectType.GraphicEditingMarker:
                        this.SetClickPositionDatas();
                        this.ChangeCursor(Cursors.Arrow);
                        break;

                    default :
                        this.SetClickPositionDatas();
                        ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                        break;
                }
            }
        }

        private void LocationGraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var selectedGraphic = sender as BaseGraphic;

            if (selectedGraphic == null)
                return;

            if (this.markerGraphic != null)
            {
                mouseButtonEventArgs.Handled = true;
                this.DeselectMarker();
                return;
            }

            if (this.isEditMode)
            {
                this.ReleaseClickPositionDatas();
            }
            else
            {
                var shouldExecuteTimer = false;

                if (selectedGraphic.Type == MapObjectType.LinkZone || selectedGraphic.Type == MapObjectType.ImageLinkZone)
                {
                    var linkZoneObjectData = this.GetObjectData(selectedGraphic.ObjectID, selectedGraphic.Type) as MapLinkZoneObjectDataInfo;

                    if (linkZoneObjectData != null)
                    {
                        if (!string.IsNullOrWhiteSpace(linkZoneObjectData.TableSplunkBasicInformationData.SplunkDataInformationID) || !string.IsNullOrWhiteSpace(linkZoneObjectData.BrowserUrl))
                        {
                            this.selectedMapLinkZoneObjectDataInfo = linkZoneObjectData;
                            this.selectedMapUniversalObjectDataInfo = null;
                            shouldExecuteTimer = true;
                        }
                    }
                }
                else if (selectedGraphic.Type == MapObjectType.UniversalControl)
                {
                    var universalObjectData = this.universalObjectDataManager.GetDataInfo(selectedGraphic.ObjectID);

                    if (universalObjectData != null && !string.IsNullOrWhiteSpace(universalObjectData.LinkUrl))
                    {
                        this.selectedMapUniversalObjectDataInfo = universalObjectData;
                        this.selectedMapLinkZoneObjectDataInfo = null;
                        shouldExecuteTimer = true;
                    }
                }
                else if(selectedGraphic.Type == MapObjectType.SplunkControl)
                {
                    var splunkObjectData =
                            this.savedSplunkObjectDataManager.GetObjectDataByObjectID(selectedGraphic.ObjectID);

                    if (splunkObjectData != null && !string.IsNullOrWhiteSpace(splunkObjectData.LinkUrl))
                    {
                        var upClickPos = Mouse.GetPosition(this.mainCanvas);

                        if (upClickPos.Equals(this.splunkControlClickPos))
                        {
                            if (this.arcGisControlViewerApi != null)
                            {
                                // URL에 대한 체크 시 잘못된 URL인 경우 http:// 만 붙여주는 편의성을 제공해준다. 다른 잘못된 주소에 대해서는 보장해주지 않는다.
                                var url = string.Format("{0}?session_id={1}",
                                                           Uri.IsWellFormedUriString(splunkObjectData.LinkUrl, UriKind.Absolute)
                                                           ? splunkObjectData.LinkUrl : "http://" + splunkObjectData.LinkUrl
                                                           , this.mapSettingDataInfo.ID);

                                this.arcGisControlViewerApi.OpenSearchViewControl(url);
                            }
                        }
                    }
                }

                if (shouldExecuteTimer)
                {
                    this.linkZoneClickPos = Mouse.GetPosition(this.mainCanvas);

                    if (this.isLinkZoneDoubleClicked)
                    {
                        mouseButtonEventArgs.Handled = true;
                    }
                    else
                    {
                        this.clickTimer.AutoReset = false;
                        this.clickTimer.Interval = 400;
                        this.clickTimer.Elapsed += clickTimer_Elapsed;
                        this.clickTimer.Start();
                    }
                }
            }
        }

        private void graphic_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.GraphicContextMenuShow((BaseGraphic)sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void LocationGraphicOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.isDoingNewObjectByClick) return;

            var currentGraphic = sender as BaseGraphic;
            if (currentGraphic == null) return;

            if (this.markerGraphic != null)
            {
                mouseEventArgs.Handled = true;
                this.MoveMarker();
                return;
            }

            if (Mouse.LeftButton == MouseButtonState.Released) mouseEventArgs.Handled = true;

            if (this.isEditMode)
            {
                if (currentGraphic.Type == MapObjectType.SplunkControl)
                {
                    DoSplunkControlMouseMove(mouseEventArgs, currentGraphic);
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
                        ChangeCursor(CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver));
                    }
                }
            }
            else if (!(!this.IsConsoleMode && sender is PolygonGraphic))
            {
                if (SelectedMapObjectType != MapObjectType.Location && Mouse.LeftButton == MouseButtonState.Released) this.ChangeCursor(Cursors.Arrow);

                if (currentGraphic is UniversalIconGraphic)
                {
                    this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.UniversalHealth));
                }
                else if (currentGraphic is UniversalControlGraphic && Mouse.LeftButton == MouseButtonState.Released)
                {
                    var dataInfo = this.universalObjectDataManager.GetDataInfo(currentGraphic.ObjectID);

                    if (!string.IsNullOrWhiteSpace(dataInfo.LinkUrl) && !string.IsNullOrWhiteSpace(dataInfo.LinkedMapGuid))
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.UniversalUrlMapLink));
                    }
                    else if (!string.IsNullOrWhiteSpace(dataInfo.LinkedMapGuid))
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.UniversalMapLink));
                    }
                    else if (!string.IsNullOrWhiteSpace(dataInfo.LinkUrl))
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.UniversalUrlLink));
                    }
                    else
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.HandOpen));
                    }
                }
                else if (currentGraphic is TextBoxControlGraphic || currentGraphic is MemoTipGraphic)
                {
                    if (this.IsReportWritingMode)
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.MoveOver));
                    }
                    else
                    {
                        this.ChangeCursor(CursorManager.Instance.GetCursor(CursorType.HandOpen));
                    }
                }
            }
        }

        private void LocationGraphicOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            var linkZoneGraphic = sender as LinkZoneGraphic;

            if (this.IsConsoleMode && linkZoneGraphic != null && (linkZoneGraphic.Type == MapObjectType.LinkZone || linkZoneGraphic.Type == MapObjectType.ImageLinkZone))
            {
                linkZoneGraphic.RevertBrightnessColor();
            }
        }

        private void LocationGraphicOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var currentGraphic = sender as BaseGraphic;
            if (currentGraphic == null) return;

            if (this.IsConsoleMode)
            {
                switch (currentGraphic.Type)
                {
                    case MapObjectType.LinkZone:
                        var linkZoneGraphic = currentGraphic as LinkZoneGraphic;
                        if (linkZoneGraphic != null)
                            linkZoneGraphic.ConvertBrightnessColor();
                        break;

                    case MapObjectType.ImageLinkZone:
                        var imageLinkZoneGraphic = currentGraphic as LinkZoneGraphic;
                        if (imageLinkZoneGraphic != null)
                            imageLinkZoneGraphic.ConvertOpaqueColor();
                        break;

                    case MapObjectType.GraphicEditingMarker:
                        this.ChangeCursor(Cursors.Arrow);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        private void SearchedAddressIconOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.isDoingNewObjectByClick)
            {
                return;
            }

            this.ChangeCursor(Cursors.Arrow);
            mouseEventArgs.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void SearchedAddressIconOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.ChangeCursor(Cursors.Arrow);

            mouseButtonEventArgs.Handled = true;

            if (this.isDoingNewObjectByClick)
            {
                return;
            }

            this.ReleaseAllSelectedObject();

            //자신 선택
            this.SelectGraphicObject(sender as BaseGraphic);
        }

        #endregion //Graphic Event Handlers

        /// <summary>
        /// SearchedAddressInfoWindowManager 에서 생성된 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objectEventArgs"></param>
        protected override void InfoWindowManager_SearchedAddressSaveButtonClick(object sender, ObjectEventArgs objectEventArgs)
        {
            base.InfoWindowManager_SearchedAddressSaveButtonClick(sender, objectEventArgs);
            this.RaiseSearchedAddressSaveButtonClickEvent(objectEventArgs);
        }

        void clickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.clickTimer.Elapsed -= clickTimer_Elapsed;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isLinkZoneDoubleClicked)
                {
                    this.isLinkZoneDoubleClicked = false;
                    return;
                }

                if (this.selectedMapUniversalObjectDataInfo != null)
                {
                    if (this.arcGisControlViewerApi != null)
                    {
                        // URL에 대한 체크 시 잘못된 URL인 경우 http:// 만 붙여주는 편의성을 제공해준다. 다른 잘못된 주소에 대해서는 보장해주지 않는다.
                        this.arcGisControlViewerApi.OpenLinkzoneViewControl(
                            Uri.IsWellFormedUriString(this.selectedMapUniversalObjectDataInfo.LinkUrl, UriKind.Absolute)
                            ? this.selectedMapUniversalObjectDataInfo.LinkUrl
                            : "http://" + this.selectedMapUniversalObjectDataInfo.LinkUrl);
                    }
                }
                else if (this.selectedMapLinkZoneObjectDataInfo != null)
                {
                    // Data Popup인지 URL 링크인지 판단하여 처리
                    if (this.selectedMapLinkZoneObjectDataInfo.ShouldShowBrowserOnClick)
                    {
                        if (this.arcGisControlViewerApi != null)
                        {
                            // URL에 대한 체크 시 잘못된 URL인 경우 http:// 만 붙여주는 편의성을 제공해준다. 다른 잘못된 주소에 대해서는 보장해주지 않는다.
                            this.arcGisControlViewerApi.OpenLinkzoneViewControl(
                            Uri.IsWellFormedUriString(this.selectedMapLinkZoneObjectDataInfo.BrowserUrl, UriKind.Absolute)
                            ? this.selectedMapLinkZoneObjectDataInfo.BrowserUrl
                            : "http://" + this.selectedMapLinkZoneObjectDataInfo.BrowserUrl);
                        }
                    }
                    else
                    {
                        if (this.arcGISControlApi.SetMapSplunkData(this.selectedMapLinkZoneObjectDataInfo.TableSplunkBasicInformationData) != null)
                        {
                            if (SplunkBasicInformationData.IsUsableServiceInfo(this.selectedMapLinkZoneObjectDataInfo.TableSplunkBasicInformationData))
                            {
                                if (this.IsDataPlayBackMode)
                                {
                                    var timeSpan = DateTime.Now - this.DataPlayBackPosition;
                                    this.dynamicSplunkControlManager.StartFetchingDataFromSavedSearch(this.selectedMapLinkZoneObjectDataInfo.TableSplunkBasicInformationData, timeSpan, this.OperationPlaybackArgs);
                                }
                                else
                                {
                                    this.dynamicSplunkControlManager.StartFetchingDataFromSavedSearch(this.selectedMapLinkZoneObjectDataInfo.TableSplunkBasicInformationData);
                                }


                                this.dynamicSplunkControlManager.Show(this.mainCanvas, this.linkZoneClickPos);
                            }
                        }
                        else
                        {
                            var splunkBasicInformationData = new SplunkBasicInformationData
                            {
                                Name = this.selectedMapLinkZoneObjectDataInfo.TableSplunkBasicInformationData.Name,
                            };

                            this.dynamicSplunkControlManager.StartFetchingDataFromSavedSearch(splunkBasicInformationData);
                            this.dynamicSplunkControlManager.Show(this.mainCanvas, this.linkZoneClickPos);
                        }
                    }
                }
            }));
        }

        #endregion //Events Handler
    }
}
