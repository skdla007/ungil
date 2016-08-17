namespace ArcGISControl.GraphicObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using ESRI.ArcGIS.Client.Geometry;
    using Geometry = System.Windows.Media.Geometry;
    using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

    /// <summary>
    ///     Interaction logic for TrailGraphic.xaml
    /// </summary>
    public partial class Trail
    {
        public static readonly DependencyProperty ExtentProperty =
            DependencyProperty.Register("Extent", typeof (Envelope), typeof (Trail),
                new PropertyMetadata(default(Envelope), PropertyChangedCallback));

        public Envelope Extent
        {
            get { return (Envelope) this.GetValue(ExtentProperty); }
            set { this.SetValue(ExtentProperty, value); }
        }

        public static readonly DependencyProperty TrailPointsProperty =
            DependencyProperty.Register("TrailPoints", typeof (PointCollection), typeof (Trail),
                new PropertyMetadata(default(PointCollection), PropertyChangedCallback));

        public PointCollection TrailPoints
        {
            get { return (PointCollection) this.GetValue(TrailPointsProperty); }
            set { this.SetValue(TrailPointsProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof (double), typeof (Trail),
                new PropertyMetadata(default(double), PropertyChangedCallback));

        public double Progress
        {
            get { return (double) this.GetValue(ProgressProperty); }
            set { this.SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ActualGeometryProperty =
            DependencyProperty.Register("ActualGeometry", typeof (Geometry), typeof (Trail),
                new PropertyMetadata(default(Geometry), PropertyChangedCallback));

        public Geometry ActualGeometry
        {
            get { return (Geometry) this.GetValue(ActualGeometryProperty); }
            set { this.SetValue(ActualGeometryProperty, value); }
        }

        public static readonly DependencyProperty Stage1DotPenProperty =
            DependencyProperty.Register("Stage1DotPen", typeof (Pen), typeof (Trail),
                new PropertyMetadata(default(Pen), PropertyChangedCallback));

        public Pen Stage1DotPen
        {
            get { return (Pen) this.GetValue(Stage1DotPenProperty); }
            set { this.SetValue(Stage1DotPenProperty, value); }
        }

        public static readonly DependencyProperty Stage1DotFillProperty =
            DependencyProperty.Register("Stage1DotFill", typeof (Brush), typeof (Trail),
                new PropertyMetadata(default(Brush)));

        public Brush Stage1DotFill
        {
            get { return (Brush) this.GetValue(Stage1DotFillProperty); }
            set { this.SetValue(Stage1DotFillProperty, value); }
        }

        public static readonly DependencyProperty Stage2DotPenProperty =
            DependencyProperty.Register("Stage2DotPen", typeof (Pen), typeof (Trail),
                new PropertyMetadata(default(Pen), PropertyChangedCallback));

        public Pen Stage2DotPen
        {
            get { return (Pen) this.GetValue(Stage2DotPenProperty); }
            set { this.SetValue(Stage2DotPenProperty, value); }
        }

        public static readonly DependencyProperty Stage2DotFillProperty =
            DependencyProperty.Register("Stage2DotFill", typeof (Brush), typeof (Trail),
                new PropertyMetadata(default(Brush)));

        public Brush Stage2DotFill
        {
            get { return (Brush) this.GetValue(Stage2DotFillProperty); }
            set { this.SetValue(Stage2DotFillProperty, value); }
        }

        public static readonly DependencyProperty Stage2LinePenProperty =
            DependencyProperty.Register("Stage2LinePen", typeof (Pen), typeof (Trail),
                new PropertyMetadata(default(Pen)));

        public Pen Stage2LinePen
        {
            get { return (Pen) this.GetValue(Stage2LinePenProperty); }
            set { this.SetValue(Stage2LinePenProperty, value); }
        }

        public static readonly DependencyProperty Stage1DotRadiusProperty =
            DependencyProperty.Register("Stage1DotRadius", typeof (double), typeof (Trail),
                new PropertyMetadata(default(double)));

        public double Stage1DotRadius
        {
            get { return (double) this.GetValue(Stage1DotRadiusProperty); }
            set { this.SetValue(Stage1DotRadiusProperty, value); }
        }

        public static readonly DependencyProperty Stage2DotRadiusProperty =
            DependencyProperty.Register("Stage2DotRadius", typeof (double), typeof (Trail),
                new PropertyMetadata(default(double)));

        public double Stage2DotRadius
        {
            get { return (double) this.GetValue(Stage2DotRadiusProperty); }
            set { this.SetValue(Stage2DotRadiusProperty, value); }
        }

        public static readonly DependencyProperty Stage1DurationProperty =
            DependencyProperty.Register("Stage1Duration", typeof (Duration), typeof (Trail),
                new PropertyMetadata(default(Duration)));

        public Duration Stage1Duration
        {
            get { return (Duration) this.GetValue(Stage1DurationProperty); }
            set { this.SetValue(Stage1DurationProperty, value); }
        }

        public static readonly DependencyProperty Stage2DurationProperty =
            DependencyProperty.Register("Stage2Duration", typeof (Duration), typeof (Trail),
                new PropertyMetadata(default(Duration)));

        public Duration Stage2Duration
        {
            get { return (Duration) this.GetValue(Stage2DurationProperty); }
            set { this.SetValue(Stage2DurationProperty, value); }
        }

        public static readonly DependencyProperty Stage3DurationProperty =
            DependencyProperty.Register("Stage3Duration", typeof (Duration), typeof (Trail), new PropertyMetadata(default(Duration)));

        public Duration Stage3Duration
        {
            get { return (Duration) GetValue(Stage3DurationProperty); }
            set { SetValue(Stage3DurationProperty, value); }
        }

        public Duration TotalDuration
        {
            get
            {
                return this.Stage1Duration.Add(this.Stage2Duration).Add(this.Stage3Duration);
            }
        }

        protected List<double> LengthList;

        protected double TotalLength;

        public Trail()
        {
            this.InitializeComponent();

            this.DesignModeSupport();

            this.StartAnimation();
        }

        private void DesignModeSupport()
        {
            if ((bool) DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof (DependencyObject)).DefaultValue)
            {
                this.Extent = new Envelope(-1, -1, 3, 3);
                this.TrailPoints = new PointCollection(new[]
                {
                    new MapPoint(0, 0),
                    new MapPoint(1, 1),
                    new MapPoint(2, 1),
                    new MapPoint(1.5, 0.5)
                });
            }
        }

        private void StartAnimation()
        {
            var animation = new DoubleAnimation(0, 1, this.TotalDuration)
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            this.BeginAnimation(ProgressProperty, animation);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as Trail;
            Debug.Assert(obj != null);

            if (dependencyPropertyChangedEventArgs.Property == TrailPointsProperty)
            {
                obj.CalculateLengths();
            }

            obj.RefreshTrail();
        }

        protected void CalculateLengths()
        {
            var points = this.TrailPoints;
            if (points == null || points.Count == 0)
            {
                this.LengthList = null;
                this.TotalLength = 0;
                return;
            }

            this.LengthList = new List<double>(points.Count - 1);
            for (var i = 0; i + 1 < points.Count; i++)
            {
                var deltaX = points[i].X - points[i + 1].X;
                var deltaY = points[i].Y - points[i + 1].Y;
                var length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                this.LengthList.Add(length);
            }
            this.TotalLength = this.LengthList.Sum();
        }

        protected virtual void RefreshTrail()
        {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (this.RenderSize.IsEmpty ||
                this.RenderSize.Width == 0 ||
                this.RenderSize.Height == 0 ||
                double.IsNaN(this.RenderSize.Width) ||
                double.IsNaN(this.RenderSize.Height))
                return;

            var realRenderSize = this.RenderSize;
            // ArcGIS에서는 Element라는 Template 내의 UIElement를 찾아서 객체가 Path가 아니면 Clip 속성을 갱신해준다.
            // 따라서 여기서 Clip 갱신 내용을 받아다가 우리가 실제로 그려야 할 크기를 찾는다.
            if (this.ActualGeometry != null)
            {
                realRenderSize.Width = Math.Max(0,Math.Min(realRenderSize.Width, this.ActualGeometry.Bounds.Width));
                realRenderSize.Height = Math.Max(0,Math.Min(realRenderSize.Height, this.ActualGeometry.Bounds.Height));
            }

            if (realRenderSize.Width == 0 || realRenderSize.Height == 0)
                return;
            // ReSharper restore CompareOfFloatsByEqualityOperator


            var progress = this.Progress;
            var stage1EndProgress = 0.3;
            var stage2EndProgress = 0.6;
            var total = this.TotalDuration;
            if (total.HasTimeSpan && this.Stage1Duration.HasTimeSpan && this.Stage2Duration.HasTimeSpan)
            {
                stage1EndProgress =
                    this.Stage1Duration.TimeSpan.TotalSeconds / total.TimeSpan.TotalSeconds;
                stage2EndProgress = stage1EndProgress +
                    this.Stage2Duration.TimeSpan.TotalSeconds / total.TimeSpan.TotalSeconds;
            }

            if (progress < stage1EndProgress)
            {
                progress /= stage1EndProgress;
                progress = Math.Max(0, Math.Min(1, progress));

                this.DrawStage1(drawingContext, progress, realRenderSize);
            }
            else
            {
                progress -= stage1EndProgress;
                progress /= stage2EndProgress - stage1EndProgress;
                progress = Math.Max(0, Math.Min(1, progress));

                this.DrawStage2(drawingContext, progress, realRenderSize);
            }
        }

        private void DrawStage1(DrawingContext drawingContext, double progress, Size realRenderSize)
        {
            var extent = this.Extent;
            var pointCollection = this.TrailPoints.ToList();
            var showingIndex = (int) Math.Floor(progress * (pointCollection.Count +  1)) - 1;
            for (var i = 0; i <= showingIndex; i++)
            {
                var mapPoint = pointCollection[i];
                var drawPointX = (mapPoint.X - extent.XMin) * realRenderSize.Width / extent.Width;
                var drawPointY = (extent.YMax - mapPoint.Y) * realRenderSize.Height / extent.Height;
                var drawPoint = new Point(drawPointX, drawPointY);

                drawingContext.DrawEllipse(this.Stage1DotFill, this.Stage1DotPen, drawPoint, this.Stage1DotRadius,
                    this.Stage1DotRadius);
            }
        }

        private void DrawStage2(DrawingContext drawingContext, double progress, Size realRenderSize)
        {
            // readonly variables
            var extent = this.Extent;
            var pointCollection = this.TrailPoints.ToList();
            var lengthList = this.LengthList;
            var preciseLength = progress * this.TotalLength;

            { // line drawing
                var lastDrawnPoint = new Point();
                int i;
                var totalDrawnLength = 0d;
                for (i = 0; i < pointCollection.Count; i++)
                {
                    var mapPoint = pointCollection[i];
                    var drawPointX = (mapPoint.X - extent.XMin) * realRenderSize.Width / extent.Width;
                    var drawPointY = (extent.YMax - mapPoint.Y) * realRenderSize.Height / extent.Height;
                    var drawPoint = new Point(drawPointX, drawPointY);

                    if (i != 0)
                    {
                        if (totalDrawnLength + lengthList[i - 1] > preciseLength)
                            break;
                        totalDrawnLength += lengthList[i - 1];
                        drawingContext.DrawLine(this.Stage2LinePen, lastDrawnPoint, drawPoint);
                    }

                    lastDrawnPoint = drawPoint;
                }

                if (i != 0 && i < pointCollection.Count)
                {
                    var ratio = (preciseLength - totalDrawnLength) / lengthList[i - 1];
                    var mapPoint = pointCollection[i];
                    var drawPoint1X = lastDrawnPoint.X;
                    var drawPoint1Y = lastDrawnPoint.Y;
                    var drawPoint2X = (mapPoint.X - extent.XMin) * realRenderSize.Width / extent.Width;
                    var drawPoint2Y = (extent.YMax - mapPoint.Y) * realRenderSize.Height / extent.Height;
                    var drawPoint = new Point(
                        drawPoint1X * (1 - ratio) + drawPoint2X * ratio,
                        drawPoint1Y * (1 - ratio) + drawPoint2Y * ratio);

                    drawingContext.DrawLine(this.Stage2LinePen, lastDrawnPoint, drawPoint);
                }
            }


            { // dot drawing
                var totalDrawnLength = 0d;
                for (var i = 0; i < pointCollection.Count; i++)
                {
                    var mapPoint = pointCollection[i];
                    var drawPointX = (mapPoint.X - extent.XMin) * realRenderSize.Width / extent.Width;
                    var drawPointY = (extent.YMax - mapPoint.Y) * realRenderSize.Height / extent.Height;
                    var drawPoint = new Point(drawPointX, drawPointY);

                    if (i != 0)
                    {
                        if (totalDrawnLength + lengthList[i - 1] > preciseLength)
                            break;
                        totalDrawnLength += lengthList[i - 1];
                    }

                    drawingContext.DrawEllipse(this.Stage2DotFill, this.Stage2DotPen, drawPoint, this.Stage2DotRadius,
                        this.Stage2DotRadius);
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.InvalidateVisual();
        }
    }
}
