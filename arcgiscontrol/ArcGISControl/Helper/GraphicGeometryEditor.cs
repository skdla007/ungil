using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.GraphicObject;
using ArcGISControl.UIControl.GraphicObjectControl;

namespace ArcGISControl.Helper
{
    using ESRI.ArcGIS.Client;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Symbols;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Collections.ObjectModel;

    internal class GraphicGeometryEditor
    {
        public enum EditType
        {
            None = -1,
            ScaleBox = 0,
            ScalePointer = 1,
            VertexPointer = 2,
            SnapVertex = 3,
        }

        #region Fields

        public bool IsEditing { get; private set; }

        public bool SeletedGeometryEditor
        {
            get { return this.editingType != EditType.None; }
            set
            {
                if (!value) this.SetEditType(EditType.None);
            }
        }

        private bool editSnapVertexEnabled;
        public bool EditSnapVertexEnabled
        {
            get { return this.editSnapVertexEnabled; }
            set
            {
                this.editSnapVertexEnabled = value;
                this.DrawSnapVertex();
            }
        }

        private bool editDragVertexEnabled;
        public bool EditDragVertexEnabled
        {
            get { return this.editDragVertexEnabled; }
            set
            {
                this.editDragVertexEnabled = value;
                this.DrawDraggingVertex();
                this.DrawSnapVertex();
            }
        }

        private bool editScaleEnabled;
        public bool EditScaleEnabled
        {
            get { return this.editScaleEnabled; }
            set
            {
                this.editScaleEnabled = value;
                this.DrawScalePointer();
            }
        }

        private EditType editingType;
        public EditType EditingType
        {
            get { return this.editingType; }
        }


        private int? numberOfPointer;

        private const int ScaleBoxCount = 5;
        private const int LEFT = 0, BOTTOM = 1, RIGHT = 2, TOP = 3;
        private const string Attribute_Key_Index = "Index";
        private const int ZIndex_ScaleBox = 0;
        private const int ZIndex_ScalePointer = 1;
        private const int ZIndex_Vertex = 2;

        private readonly GraphicsLayer editorGraphicLayer;

        /// <summary>
        /// 수정 하기 위한 데이터 
        /// Map의 Object중에 하나임
        /// 메모는 EditorManager를 통하여 따로 처리됨
        /// </summary>
        private Graphic activatedGraphic;
        public Graphic ActivatedGraphic
        {
            get { return this.activatedGraphic; }
        }

        /// <summary>
        /// activatedGraphic을 Editing 하기위해
        /// 아래의 scalePointGraphics/draggingVertexGraphics/snapVertexGraphics 중
        /// 선택된 하나의 아이템이 저장된다.
        /// </summary>
        private Graphic selectedGraphic;

        /// <summary>
        /// Scale을 하기위한 범위지정되어 있는 네모 점선 박스
        /// </summary>
        private readonly Graphic scaleBoxGraphic;

        /// <summary>
        /// 실제로 Scale하기위한 그래픽
        /// selectedGraphic에 scalePointGraphics 중 하나가 들어가면 
        /// 그 위치를 기준으로 activatedGraphic을 Scale.
        /// </summary>
        private readonly List<Graphic> scalePointGraphics;

        /// <summary>
        /// 꼭지점 위에서 드래그 하면 그 꼭지점만 위치이동 시키는 그래픽
        /// selectedGraphic에 draggingVertexGraphics 중 하나가 들어가면 
        /// 그 위치를 기준으로 activatedGraphic의 한 꼭지점의 위치를 변경.
        /// </summary>
        private List<Graphic> draggingVertexGraphics;

        /// <summary>
        /// 꼭지점과 꼭지점 사이에(면) 정 중앙에 꼭지점이 생기게 하는 그래픽
        /// 마우스 오버시에만 보여지게 보임
        /// selectedGraphic에 snapVertexGraphics 중 하나가 들어가면
        /// 그 위치에 꼭지점 하나가 생성
        /// </summary>
        private readonly List<Graphic> snapVertexGraphics;

        /// <summary>
        /// 
        /// </summary>
        private readonly double[] scaleNetValues;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<MapPoint> snapVertexPoints;

        /// <summary>
        /// 
        /// </summary>
        private readonly ResourceDictionary symbolsDictionary;

        #endregion Fields

        #region Events

        internal event EventHandler<GraphicGeometryChangedEventArgs> eGraphicGeometryChanged;
        internal event EventHandler<MouseButtonEventArgs> eMouseLeftButtonDown;
        internal event EventHandler<MouseButtonEventArgs> eMouseRightButtonUp;

        #endregion Events

        #region Construction

        public GraphicGeometryEditor(GraphicsLayer graphicsLayer)
        {
            this.symbolsDictionary = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/ArcGISControl;component/Helper/SymbolsDictionary.xaml", UriKind.RelativeOrAbsolute)
            };

            this.editorGraphicLayer = graphicsLayer;

            this.scaleBoxGraphic = new Graphic()
            {
                Symbol = new LineSymbol()
                {
                    ControlTemplate = this.symbolsDictionary["EditLineSymbol"] as ControlTemplate,
                },
            };

            this.scaleBoxGraphic.SetZIndex(ZIndex_ScaleBox);
            this.scaleBoxGraphic.MouseLeftButtonDown += ScaleBoxGraphicOnMouseLeftButtonDown;
            this.scaleBoxGraphic.MouseLeftButtonUp += ScaleBoxGraphicOnMouseLeftButtonUp;
            this.scaleBoxGraphic.MouseRightButtonUp += ScaleBoxGraphic_MouseRightButtonUp;

            this.scalePointGraphics = new List<Graphic>();
            this.draggingVertexGraphics = new List<Graphic>();

            this.scaleNetValues = new double[4];
            this.snapVertexGraphics = new List<Graphic>();
            this.snapVertexPoints = new List<MapPoint>();

            this.editDragVertexEnabled = true;
            this.editScaleEnabled = true;
            this.editSnapVertexEnabled = true;
        }

        #endregion Construction

        #region Methods

        #region External Methods

        public void Start(Graphic graphic)
        {
            this.Stop();

            this.IsEditing = true;
            this.activatedGraphic = graphic;

            if (this.activatedGraphic == null) return;

            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);

            this.SetVertexNetValues(points);
            this.SetSnapVertextValues(points);

            this.DrawEditor();

            if (this.activatedGraphic is TextBoxControlGraphic)
            {
                (this.activatedGraphic as TextBoxControlGraphic).Control.SetMoveModeTextBox();
            }
        }

        public void Stop()
        {
            this.activatedGraphic = null;
            this.IsEditing = false;
            this.scaleBoxGraphic.Geometry = null;
            this.editorGraphicLayer.Graphics.Clear();
            this.draggingVertexGraphics.Clear();
            this.snapVertexPoints.Clear();
            this.snapVertexGraphics.Clear();
            this.SetEditType(EditType.None);
            this.numberOfPointer = null;
        }

        public void Move(Vector displacement)
        {
            if (this.activatedGraphic == null || this.editingType != EditType.ScaleBox)
                return;

            List<MapPoint> movePointCollection = null;

            movePointCollection = this.MoveGraphic(displacement);

            if (movePointCollection == null) return;

            this.SetGraphicPoints(this.activatedGraphic, movePointCollection);

            this.DrawEditor();
        }

        public void Scale(Vector replacement, bool setRateMaintenanceScale)
        {
            if (this.activatedGraphic == null || this.editingType != EditType.ScalePointer)
                return;

            List<MapPoint> movePointCollection = null;

            movePointCollection = this.ScaleGraphic(replacement, setRateMaintenanceScale);

            if (movePointCollection == null) return;

            this.SetGraphicPoints(this.activatedGraphic, movePointCollection);

            this.DrawEditor();
        }

        public void DragVertex(MapPoint newMapPoint)
        {
            if (this.activatedGraphic == null || this.editingType != EditType.VertexPointer)
                return;

            List<MapPoint> movePointCollection = null;

            movePointCollection = this.DragVertexGraphic(newMapPoint);

            if (movePointCollection == null) return;

            this.SetGraphicPoints(this.activatedGraphic, movePointCollection);

            this.DrawEditor();
        }

        #endregion External Methods

        #region UI Update Methods

        private void DrawEditor()
        {
            this.DrawScaleBox();
            this.DrawScalePointer();
            this.DrawDraggingVertex();
            this.DrawSnapVertex();
        }

        private void DrawScaleBox()
        {
            if (!this.editorGraphicLayer.Graphics.Contains(this.scaleBoxGraphic))
                this.editorGraphicLayer.Graphics.Add(this.scaleBoxGraphic);

            var left = this.scaleNetValues[LEFT];
            var bottom = this.scaleNetValues[BOTTOM];
            var right = this.scaleNetValues[RIGHT];
            var top = this.scaleNetValues[TOP];

            var scaleBoxPoint = new List<MapPoint>()
                                    {
                                        new MapPoint(left, bottom),
                                        new MapPoint(left, top),
                                        new MapPoint(right, top),
                                        new MapPoint(right, bottom),
                                        new MapPoint(left, bottom)
                                    };

            if (scaleBoxPoint.Count != ScaleBoxCount)
                throw new ArgumentException();

            var polyline = new Polyline()
            {
                Paths = new ObservableCollection<PointCollection>() { new PointCollection(scaleBoxPoint) }
            };

            this.scaleBoxGraphic.Geometry = polyline;
        }

        private void DrawScalePointer()
        {
            if (!this.editScaleEnabled)
            {
                foreach (var graphic in this.scalePointGraphics)
                {
                    if (this.editorGraphicLayer.Graphics.Contains(graphic))
                        this.editorGraphicLayer.Graphics.Remove(graphic);
                }
                this.scalePointGraphics.Clear();
                return;
            }

            // Y
            // ^
            // | 076
            // | 1 5
            // | 234
            // +-----> X

            var newCount = 8;
            var oldCount = this.scalePointGraphics.Count;
            var gapCount = newCount - oldCount;

            var left = this.scaleNetValues[LEFT];
            var bottom = this.scaleNetValues[BOTTOM];
            var right = this.scaleNetValues[RIGHT];
            var top = this.scaleNetValues[TOP];
            var vCenter = (left + right) / 2;
            var hCenter = (bottom + top) / 2;

            var points = new MapPoint[]
            {
                new MapPoint(left, top),
                new MapPoint(left, hCenter),
                new MapPoint(left, bottom),
                new MapPoint(vCenter, bottom),
                new MapPoint(right, bottom),
                new MapPoint(right, hCenter),
                new MapPoint(right, top),
                new MapPoint(vCenter, top),
            };

            for (var i = 0; i < gapCount; i++)
            {
                var graphic = new Graphic()
                {
                    Symbol = new MarkerSymbol()
                    {
                        ControlTemplate = this.symbolsDictionary["ScalePointSymbol"] as ControlTemplate,
                        OffsetX = 6,
                        OffsetY = 6,
                    }
                };

                this.scalePointGraphics.Add(graphic);

                this.scalePointGraphics[i].SetZIndex(ZIndex_ScalePointer);
                this.scalePointGraphics[i].MouseLeftButtonDown += ScalePointorGraphicMouseLeftButtonDown;
                this.scalePointGraphics[i].MouseLeftButtonUp += ScalePointorGraphicMouseLeftButtonUp;
            }

            for (var i = 0; i < this.scalePointGraphics.Count; i++)
            {
                if (!this.editorGraphicLayer.Graphics.Contains(scalePointGraphics[i]))
                    this.editorGraphicLayer.Graphics.Add(scalePointGraphics[i]);

                this.scalePointGraphics[i].Geometry = points[i];
                this.scalePointGraphics[i].Attributes[Attribute_Key_Index] = i;
            }
        }

        private void DrawDraggingVertex()
        {
            if (this.activatedGraphic == null || this.activatedGraphic.Geometry == null) return;

            if (!this.editDragVertexEnabled)
            {
                foreach (var graphic in this.draggingVertexGraphics)
                {
                    if (this.editorGraphicLayer.Graphics.Contains(graphic))
                        this.editorGraphicLayer.Graphics.Remove(graphic);
                }
                this.draggingVertexGraphics.Clear();
                return;
            }

            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);
            var newCount = this.activatedGraphic.Geometry is Polygon ? points.Count - 1 : points.Count;
            var oldCount = this.draggingVertexGraphics.Count;
            var gapCount = newCount - oldCount;

            for (var i = 0; i < gapCount; i++)
            {
                var graphic = new Graphic()
                {
                    Symbol = new MarkerSymbol()
                    {
                        ControlTemplate = this.symbolsDictionary["EditVertexSymbol"] as ControlTemplate,
                        OffsetX = 5,
                        OffsetY = 5,
                    }
                };

                graphic.SetZIndex(ZIndex_Vertex);

                this.draggingVertexGraphics.Add(graphic);
                this.editorGraphicLayer.Graphics.Add(graphic);

                graphic.MouseLeftButtonDown += DraggingVertexGraphicOnMouseLeftButtonDown;
                graphic.MouseLeftButtonUp += DraggingVertexGraphicOnMouseLeftButtonUp;
            }

            for (var i = 0; i < newCount; i++)
            {
                this.draggingVertexGraphics[i].Attributes[Attribute_Key_Index] = i;
                this.draggingVertexGraphics[i].Geometry = points[i];
            }
        }

        private void DrawSnapVertex()
        {
            if (!this.editDragVertexEnabled || !this.editSnapVertexEnabled)
            {
                foreach (var graphic in this.snapVertexGraphics)
                {
                    if (this.editorGraphicLayer.Graphics.Contains(graphic))
                        this.editorGraphicLayer.Graphics.Remove(graphic);
                }
                this.snapVertexGraphics.Clear();
                return;
            }

            var newCount = this.snapVertexPoints.Count;
            var oldCount = this.snapVertexGraphics.Count;
            var gapCount = newCount - oldCount;

            for (var i = 0; i < gapCount; i++)
            {
                var graphic = new Graphic()
                {
                    Symbol = new MarkerSymbol()
                    {
                        ControlTemplate = this.symbolsDictionary["SnapVertexSymbol"] as ControlTemplate,
                        OffsetX = 5,
                        OffsetY = 5,
                    }
                };

                graphic.SetZIndex(ZIndex_Vertex);

                this.snapVertexGraphics.Add(graphic);

                if (!this.editorGraphicLayer.Graphics.Contains(graphic))
                {
                    this.editorGraphicLayer.Graphics.Add(graphic);
                }

                graphic.MouseLeftButtonDown += SnapVertexGraphicOnMouseLeftButtonDown;
                graphic.MouseLeftButtonUp += SnapVertexGraphicOnMouseLeftButtonUp;
            }

            for (var i = 0; i < newCount; i++)
            {
                this.snapVertexGraphics[i].Attributes[Attribute_Key_Index] = i;
                this.snapVertexGraphics[i].Geometry = this.snapVertexPoints[i];
            }
        }

        private void SetEditType(EditType editType)
        {
            this.editingType = editType;

            switch (editType)
            {
                case EditType.None:
                    if (this.selectedGraphic == null) return;
                    this.selectedGraphic.UnSelect();
                    this.selectedGraphic = null;
                    break;
                case EditType.ScaleBox:
                case EditType.ScalePointer:
                case EditType.VertexPointer:
                default:
                    this.selectedGraphic.Select();
                    break;
            }
        }

        private void SetSnapVertextValues(List<MapPoint> points)
        {
            this.snapVertexPoints.Clear();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                this.snapVertexPoints.Add(new MapPoint(p1.X + (p2.X - p1.X) / 2, p1.Y + (p2.Y - p1.Y) / 2));
            }
        }

        private void SetVertexNetValues(List<MapPoint> points)
        {
            this.scaleNetValues[LEFT] = points.Min(p => p.X);
            this.scaleNetValues[BOTTOM] = points.Min(p => p.Y);
            this.scaleNetValues[RIGHT] = points.Max(p => p.X);
            this.scaleNetValues[TOP] = points.Max(p => p.Y);
        }

        #endregion UI Update Methods

        #region Factory

        private List<MapPoint> MoveGraphic(Vector displacement)
        {
            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);
            var movePointCollection = new List<MapPoint>();
            points.ForEach(p => movePointCollection.Add(new MapPoint(p.X + displacement.X, p.Y + displacement.Y)));

            this.SetVertexNetValues(movePointCollection);
            this.SetSnapVertextValues(movePointCollection);

            return movePointCollection;
        }

        private List<MapPoint> ScaleGraphic(Vector replacement, bool setRateMaintenanceScale)
        {
            // Y
            // ^
            // | 076
            // | 1 5
            // | 234
            // +-----> X

            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);
            var fixedPoint = new Point();
            var width = scaleNetValues[RIGHT] - scaleNetValues[LEFT];
            var height = scaleNetValues[TOP] - scaleNetValues[BOTTOM];

            switch (this.numberOfPointer)
            {
                case 0:
                    scaleNetValues[LEFT] = replacement.X;
                    scaleNetValues[TOP] = replacement.Y;
                    fixedPoint = new Point(scaleNetValues[RIGHT], scaleNetValues[BOTTOM]);
                    break;
                case 1:
                    fixedPoint = new Point(scaleNetValues[RIGHT], 0);
                    scaleNetValues[LEFT] = replacement.X;
                    break;
                case 2:
                    fixedPoint = new Point(scaleNetValues[RIGHT], scaleNetValues[TOP]);
                    scaleNetValues[LEFT] = replacement.X;
                    scaleNetValues[BOTTOM] = replacement.Y;
                    break;
                case 3:
                    fixedPoint = new Point(0, scaleNetValues[TOP]);
                    scaleNetValues[BOTTOM] = replacement.Y;
                    break;
                case 4:
                    fixedPoint = new Point(scaleNetValues[LEFT], scaleNetValues[TOP]);
                    scaleNetValues[RIGHT] = replacement.X;
                    scaleNetValues[BOTTOM] = replacement.Y;
                    break;
                case 5:
                    fixedPoint = new Point(scaleNetValues[LEFT], 0);
                    scaleNetValues[RIGHT] = replacement.X;
                    break;
                case 6:
                    fixedPoint = new Point(scaleNetValues[LEFT], scaleNetValues[BOTTOM]);
                    scaleNetValues[RIGHT] = replacement.X;
                    scaleNetValues[TOP] = replacement.Y;
                    break;
                case 7:
                    fixedPoint = new Point(0, scaleNetValues[BOTTOM]);
                    scaleNetValues[TOP] = replacement.Y;
                    break;
            }

            var xMag = (scaleNetValues[RIGHT] - scaleNetValues[LEFT]) / width;
            var yMag = (scaleNetValues[TOP] - scaleNetValues[BOTTOM]) / height;

            //비율 유지 코드
            if(setRateMaintenanceScale)
            {
                switch (this.numberOfPointer)
                {
                    case 0:
                        if (xMag > yMag)
                        {
                            scaleNetValues[TOP] = scaleNetValues[BOTTOM] + (height * xMag);
                            yMag = xMag;
                        }
                        else
                        {
                            scaleNetValues[LEFT] = scaleNetValues[RIGHT] - (width * yMag);
                            xMag = yMag;
                        }
                        break;
                    case 1:
                        fixedPoint = new Point(scaleNetValues[RIGHT], scaleNetValues[BOTTOM]);
                        scaleNetValues[TOP] = scaleNetValues[BOTTOM] + (height * xMag);
                        yMag = xMag;
                        break;
                    case 2:
                        if (xMag > yMag)
                        {
                            scaleNetValues[BOTTOM] = scaleNetValues[TOP] - (height * xMag);
                            yMag = xMag;
                        }
                        else
                        {
                            scaleNetValues[LEFT] = scaleNetValues[RIGHT] - (width * yMag);
                            xMag = yMag;
                        }
                        break;
                    case 3:
                        fixedPoint = new Point(scaleNetValues[LEFT], scaleNetValues[TOP]);
                        scaleNetValues[RIGHT] = scaleNetValues[LEFT] + (width * yMag);
                        xMag = yMag;
                        break;
                    case 4:
                        if (xMag > yMag)
                        {
                            scaleNetValues[BOTTOM] = scaleNetValues[TOP] - (height * xMag);
                            yMag = xMag;
                        }
                        else
                        {
                            scaleNetValues[RIGHT] = scaleNetValues[LEFT] + (width * yMag);
                            xMag = yMag;
                        }
                        break;
                    case 5:
                        fixedPoint = new Point(scaleNetValues[LEFT], scaleNetValues[TOP]);
                        scaleNetValues[BOTTOM] = scaleNetValues[TOP] - (height * xMag);
                        yMag = xMag;
                        break;
                    case 6:
                        if (xMag > yMag)
                        {
                            scaleNetValues[TOP] = scaleNetValues[BOTTOM] + (height * xMag);
                            yMag = xMag;
                        }
                        else
                        {
                            scaleNetValues[RIGHT] = scaleNetValues[LEFT] + (width * yMag);
                            xMag = yMag;
                        }
                        break;
                    case 7:
                        fixedPoint = new Point(scaleNetValues[RIGHT], scaleNetValues[BOTTOM]);
                        scaleNetValues[LEFT] = scaleNetValues[RIGHT] - (width * yMag);
                        xMag = yMag;
                        break;
                }
            }

            List<MapPoint> movePointCollection;

            if (NumberUtil.AreSame(scaleNetValues[LEFT], scaleNetValues[RIGHT]) ||
                NumberUtil.AreSame(scaleNetValues[TOP], scaleNetValues[BOTTOM]) ||
                double.IsNaN(xMag) || double.IsInfinity(xMag) ||
                double.IsNaN(yMag) || double.IsInfinity(yMag))
            {
                movePointCollection = points;

                this.scaleNetValues[LEFT] = points.Min(p => p.X);
                this.scaleNetValues[BOTTOM] = points.Min(p => p.Y);
                this.scaleNetValues[RIGHT] = points.Max(p => p.X);
                this.scaleNetValues[TOP] = points.Max(p => p.Y);
            }
            else
            {
                movePointCollection = points.Select(oldMapPoint => new MapPoint((oldMapPoint.X - fixedPoint.X) * xMag + fixedPoint.X, (oldMapPoint.Y - fixedPoint.Y) * yMag + fixedPoint.Y)).ToList();
            }

            this.SetSnapVertextValues(movePointCollection);

            return movePointCollection;
        }

        private List<MapPoint> DragVertexGraphic(MapPoint newMapPoint)
        {
            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);

            if (this.numberOfPointer == null) return points;

            if (points != null)
            {
                points[this.numberOfPointer.Value] = newMapPoint;
                if (this.numberOfPointer.Value == 0 && this.activatedGraphic.Geometry is Polygon) points[points.Count - 1] = newMapPoint;
            }

            this.SetVertexNetValues(points);
            this.SetSnapVertextValues(points);

            return points;
        }

        private void RemoveVertex()
        {
            if (this.numberOfPointer == null) return;

            if ((this.activatedGraphic.Geometry is Polygon && this.draggingVertexGraphics.Count <= 3) ||
                (this.activatedGraphic.Geometry is Polyline && this.draggingVertexGraphics.Count <= 2)) return;

            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);

            var vertexGraphic = this.draggingVertexGraphics.ElementAt(this.numberOfPointer.Value);
            this.editorGraphicLayer.Graphics.Remove(vertexGraphic);
            this.draggingVertexGraphics.RemoveAt(this.numberOfPointer.Value);

            points.RemoveAt(this.numberOfPointer.Value);
            if (this.numberOfPointer.Value == 0)
            {
                points[points.Count - 1] = points[0];
            }
            this.SetGraphicPoints(this.activatedGraphic, points);

            this.SetVertexNetValues(points);
            this.SetSnapVertextValues(points);

            this.DrawEditor();

            this.snapVertexGraphics.RemoveAt(this.numberOfPointer.Value);
        }

        private void SnapVertex()
        {
            if (this.numberOfPointer == null) return;

            var points = this.GetGraphicPoints(this.activatedGraphic.Geometry);

            if (points.Count <= this.numberOfPointer.Value + 1) return;

            points.Insert(this.numberOfPointer.Value + 1, this.snapVertexPoints[this.numberOfPointer.Value]);
            this.SetGraphicPoints(this.activatedGraphic, points);

            this.SetSnapVertextValues(points);

            this.DrawEditor();
        }

        private void SetGraphicPoints(Graphic graphic, List<MapPoint> points)
        {
            if (graphic.Geometry is Polygon)
            {
                var polygon = new Polygon()
                {
                    Rings = new ObservableCollection<PointCollection>()
                                                          {
                                                              new PointCollection(points)
                                                          }
                };
                graphic.Geometry = polygon;
            }
            else if (graphic.Geometry is Polyline)
            {
                var polyline = new Polyline()
                {
                    Paths = new ObservableCollection<PointCollection>()
                                                           {
                                                               new PointCollection(points)
                                                           }
                };
                graphic.Geometry = polyline;
            }

            this.RaiseeGraphicGeometryChanged(new GraphicGeometryChangedEventArgs(graphic, points.Select(p => new Point(p.X, p.Y)).ToList()));
        }

        private List<MapPoint> GetGraphicPoints(Geometry graphicGeometry)
        {
            var points = new List<MapPoint>();
            if (graphicGeometry is Polygon)
            {
                points = (graphicGeometry as Polygon).Rings.ElementAt(0).ToList();
            }
            else if (graphicGeometry is Polyline)
            {
                points = (graphicGeometry as Polyline).Paths.ElementAt(0).ToList();
            }
            return points;
        }

        private void RaiseeGraphicGeometryChanged(GraphicGeometryChangedEventArgs eventArgs)
        {
            var e = this.eGraphicGeometryChanged;
            if (e != null)
            {
                e(this, eventArgs);
            }
        }

        private void RaiseeMouseLeftButtonDown(MouseButtonEventArgs MouseEventArgs)
        {
            EventHandler<MouseButtonEventArgs> e = eMouseLeftButtonDown;
            if (e != null)
            {
                e(this, MouseEventArgs);
            }
        }

        private void RaiseeMouseRightButtonUp(MouseButtonEventArgs MouseEventArgs)
        {
            EventHandler<MouseButtonEventArgs> e = eMouseRightButtonUp;
            if (e != null)
            {
                e(this, MouseEventArgs);
            }
        }

        #endregion Factory

        #endregion Methods

        #region Event Handlers

        private void ScaleBoxGraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (((BaseGraphic)activatedGraphic).IsLocked)
            {
                mouseButtonEventArgs.Handled = true;
                return;
            }

            this.selectedGraphic = sender as Graphic;

            this.SetEditType(EditType.ScaleBox);

            if (mouseButtonEventArgs.ClickCount >= 2)
            {
                // TextBox Object Mouse Double Click
                if (this.activatedGraphic is TextBoxControlGraphic)
                {
                    (this.activatedGraphic as TextBoxControlGraphic).Control.SetEditModeTextBox();
                    mouseButtonEventArgs.Handled = true;
                }
            }

            this.RaiseeMouseLeftButtonDown(mouseButtonEventArgs);
        }

        private void ScaleBoxGraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.SetEditType(EditType.None);

            if (this.activatedGraphic is TextBoxControlGraphic)
            {
                (this.activatedGraphic as TextBoxControlGraphic).Control.SetMoveModeTextBox();
            }
        }

        private void ScaleBoxGraphic_MouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.RaiseeMouseRightButtonUp(mouseButtonEventArgs);
        }

        private void ScalePointorGraphicMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (((BaseGraphic)activatedGraphic).IsLocked)
            {
                mouseButtonEventArgs.Handled = true;
                return;
            }

            this.selectedGraphic = sender as Graphic;
            if (this.selectedGraphic == null) return;

            this.SetEditType(EditType.ScalePointer);
            this.numberOfPointer = int.Parse(this.selectedGraphic.Attributes[Attribute_Key_Index].ToString());

            this.RaiseeMouseLeftButtonDown(mouseButtonEventArgs);
        }

        private void ScalePointorGraphicMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.SetEditType(EditType.None);
        }

        private void DraggingVertexGraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (((BaseGraphic)activatedGraphic).IsLocked)
            {
                mouseButtonEventArgs.Handled = true;
                return;
            }

            this.selectedGraphic = sender as Graphic;
            if (this.selectedGraphic == null) return;

            this.SetEditType(EditType.VertexPointer);
            this.numberOfPointer = int.Parse(this.selectedGraphic.Attributes[Attribute_Key_Index].ToString());

            if (mouseButtonEventArgs.ClickCount == 2)
            {
                this.RemoveVertex();
                mouseButtonEventArgs.Handled = true;
            }

            this.RaiseeMouseLeftButtonDown(mouseButtonEventArgs);
        }

        private void DraggingVertexGraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.SetEditType(EditType.None);
        }

        private void SnapVertexGraphicOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (((BaseGraphic)activatedGraphic).IsLocked)
            {
                mouseButtonEventArgs.Handled = true;
                return;
            }

            this.selectedGraphic = sender as Graphic;
            if (this.selectedGraphic == null) return;

            this.SetEditType(EditType.SnapVertex);
            this.numberOfPointer = int.Parse(this.selectedGraphic.Attributes[Attribute_Key_Index].ToString());

            this.SnapVertex();

            this.selectedGraphic.UnSelect();
            this.numberOfPointer = int.Parse(this.selectedGraphic.Attributes[Attribute_Key_Index].ToString()) + 1;
            if (this.draggingVertexGraphics.Count > this.numberOfPointer.Value)
            {
                this.selectedGraphic = this.draggingVertexGraphics.ElementAt(this.numberOfPointer.Value);
                this.SetEditType(EditType.VertexPointer);
            }

            this.RaiseeMouseLeftButtonDown(mouseButtonEventArgs);
        }

        private void SnapVertexGraphicOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.SetEditType(EditType.None);
        }

        #endregion Event Handlers
    }

    internal class GraphicGeometryChangedEventArgs : EventArgs
    {
        public Graphic Graphic { get; set; }
        public List<Point> Points { get; set; }
        public GraphicGeometryChangedEventArgs(Graphic graphic, List<Point> points)
        {
            this.Graphic = graphic;
            this.Points = points;
        }
    }

    internal class GraphicGeometrySelectedEventArgs : EventArgs
    {
        public Graphic Graphic { get; set; }
        public GraphicGeometrySelectedEventArgs(Graphic graphic)
        {
            this.Graphic = graphic;
        }
    }
}