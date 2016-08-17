using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    public partial class UniversalControl : Viewbox
    {
        public static readonly DependencyProperty ControlSizeProperty = DependencyProperty.Register(
            "ControlSize",
            typeof(Size), typeof(UniversalControl),
            new PropertyMetadata(
                new Size(200, 100),
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyTitleArea();
                    control.ApplyIconArea();
                    control.ApplyRectangleRadius();
                }
            )
        );

        public Size ControlSize
        {
            get { return (Size)this.GetValue(ControlSizeProperty); }
            set { this.SetValue(ControlSizeProperty, value); }
        }

        public static readonly DependencyProperty ShapeTypeProperty = DependencyProperty.Register(
            "ShapeType",
            typeof(MapUniversalObjectDataInfo.ShapeTypes), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                    {
                        var control = (UniversalControl) s;
                        control.ApplyShapeType();
                        control.ApplyFill();
                    }
                )
            );

        public MapUniversalObjectDataInfo.ShapeTypes ShapeType
        {
            get { return (MapUniversalObjectDataInfo.ShapeTypes)this.GetValue(ShapeTypeProperty); }
            set { this.SetValue(ShapeTypeProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.FitTitleFontSize();
                }
            )
        );

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleMinMaxPositionProperty = DependencyProperty.Register(
            "TitleMinMaxPosition",
            typeof(Rect), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyTitleArea();
                }
            )
        );

        public Rect TitleMinMaxPosition
        {
            get { return (Rect)this.GetValue(TitleMinMaxPositionProperty); }
            set { this.SetValue(TitleMinMaxPositionProperty, value); }
        }

        public Size TitleSize
        {
            get
            {
                return new Size(
                    this.ControlSize.Width * this.TitleMinMaxPosition.Width * 0.01,
                    this.ControlSize.Height * this.TitleMinMaxPosition.Height * 0.01
                );
            }
        }

        public Thickness TitleMargin
        {
            get
            {
                return new Thickness(
                    this.ControlSize.Width * this.TitleMinMaxPosition.Left * 0.01,
                    this.ControlSize.Height * (1 - this.TitleMinMaxPosition.Bottom * 0.01),
                    this.ControlSize.Width * (1 - this.TitleMinMaxPosition.Right * 0.01),
                    this.ControlSize.Height * this.TitleMinMaxPosition.Top * 0.01
                );
            }
        }

        public static readonly DependencyProperty TitleColorProperty = DependencyProperty.Register(
            "TitleColor",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata("black")
        );

        public string TitleColor
        {
            get { return (string)this.GetValue(TitleColorProperty); }
            set { this.SetValue(TitleColorProperty, value); }
        }

        public TextAlignment TitleAlignment
        {
            get { return (TextAlignment)this.GetValue(TitleAlignmentProperty); }
            set { this.SetValue(TitleAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TitleAlignmentProperty = DependencyProperty.Register(
            "TitleAlignment",
            typeof(TextAlignment), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyTitleAlignment();
                }
            )
        );

        public static readonly DependencyProperty IconImageUrlProperty = DependencyProperty.Register(
            "IconImageUrl",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyIconImage();
                }
            )
        );

        public string IconImageUrl
        {
            get { return (string)this.GetValue(IconImageUrlProperty); }
            set { this.SetValue(IconImageUrlProperty, value); }
        }

        public static readonly DependencyProperty IconMinMaxPositionProperty = DependencyProperty.Register(
            "IconMinMaxPosition",
            typeof(Rect), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyIconArea();
                }
            )
        );

        public Rect IconMinMaxPosition
        {
            get { return (Rect)this.GetValue(IconMinMaxPositionProperty); }
            set { this.SetValue(IconMinMaxPositionProperty, value); }
        }

        public Size IconSize
        {
            get
            {
                return new Size(
                    this.ControlSize.Width * this.IconMinMaxPosition.Width * 0.01,
                    this.ControlSize.Height * this.IconMinMaxPosition.Height * 0.01
                );
            }
        }

        public Thickness IconMargin
        {
            get
            {
                return new Thickness(
                    this.ControlSize.Width * this.IconMinMaxPosition.Left * 0.01,
                    this.ControlSize.Height * (1 - this.IconMinMaxPosition.Bottom * 0.01),
                    this.ControlSize.Width * (1 - this.IconMinMaxPosition.Right * 0.01),
                    this.ControlSize.Height * this.IconMinMaxPosition.Top * 0.01
                );
            }
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(double), typeof(UniversalControl)
        );

        public double StrokeThickness
        {
            get { return (double)this.GetValue(StrokeThicknessProperty); }
            set { this.SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register(
            "StrokeColor",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata("black")
        );

        public string StrokeColor
        {
            get { return (string)this.GetValue(StrokeColorProperty); }
            set { this.SetValue(StrokeColorProperty, value); }
        }

        public static readonly DependencyProperty StrokeRadiusProperty = DependencyProperty.Register(
            "StrokeRadius",
            typeof(double), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyRectangleRadius();
                }
            )
        );

        public double StrokeRadius
        {
            get { return (double)this.GetValue(StrokeRadiusProperty); }
            set { this.SetValue(StrokeRadiusProperty, value); }
        }

        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register(
            "FillColor",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata(
                "transparent",
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyFill();
                }
            )
        );

        public string FillColor
        {
            get { return (string)this.GetValue(FillColorProperty); }
            set { this.SetValue(FillColorProperty, value); }
        }

        public static readonly DependencyProperty FillImageUrlProperty = DependencyProperty.Register(
            "FillImageUrl",
            typeof(string), typeof(UniversalControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var control = (UniversalControl)s;
                    control.ApplyFill();
                }
            )
        );

        public string FillImageUrl
        {
            get { return (string)this.GetValue(FillImageUrlProperty); }
            set { this.SetValue(FillImageUrlProperty, value); }
        }

        private bool isLoaded;

        public UniversalControl()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                isLoaded = true;
                this.ApplyShapeType();
                this.ApplyTitleArea();
                this.ApplyIconArea();
                this.ApplyFill();
                this.ApplyRectangleRadius();
                this.ApplyIconImage();
                this.ApplyTitleAlignment();
            };

            this.Unloaded += (s, e) =>
            {
                isLoaded = false;
            };
        }

        #region Metods

        #region TItle

        private void FitTitleFontSize()
        {
            if (!this.isLoaded)
                return;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var fitFontSize = this.FindFitTitleFontSize(1, this.TitleSize.Height);
                this.xTitle.FontSize = fitFontSize;
            }));
        }

        private double FindFitTitleFontSize(double small, double big)
        {
            while (big - small > 1)
            {
                var mid = (small + big) / 2;

                this.xTitle.FontSize = mid;
                this.xTitle.UpdateLayout();

                if (this.IsTitleOversized())
                {
                    big = mid;
                }
                else
                {
                    small = mid;
                }
            }

            return small;
        }

        private bool IsTitleOversized()
        {
            return this.xTitle.ActualWidth > this.TitleSize.Width
                || this.xTitle.ActualHeight > this.TitleSize.Height;
        }

        private void ApplyTitleArea()
        {
            if (!this.isLoaded)
                return;

            this.xTitleArea.Margin = this.TitleMargin;

            this.FitTitleFontSize();
        }

        private void ApplyTitleAlignment()
        {
            this.xTitle.TextAlignment = this.TitleAlignment;
            switch (this.TitleAlignment)
            {
                case TextAlignment.Left:
                    this.xTitleArea.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case TextAlignment.Center:
                    this.xTitleArea.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case TextAlignment.Right:
                    this.xTitleArea.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
            }
        }
        #endregion // TItle

        #region Icon
        private void ApplyIconArea()
        {
            if (!this.isLoaded)
                return;

            this.xIcon.Margin = this.IconMargin;
        }

        private void ApplyIconImage()
        {
            if (!this.isLoaded)
                return;

            if (string.IsNullOrWhiteSpace(this.IconImageUrl))
                return;

            if (Uri.IsWellFormedUriString(this.IconImageUrl, UriKind.RelativeOrAbsolute))
            {
                try
                {
                    this.xIcon.Source = new BitmapImage(new Uri(this.IconImageUrl));
                }
                catch (Exception)
                {
                    this.xIcon.Source = null;
                }
            }
        }
        #endregion // Icon

        #region Shape

        private void ApplyShapeType()
        {
            switch (this.ShapeType)
            {
                case MapUniversalObjectDataInfo.ShapeTypes.Rectangle:
                case MapUniversalObjectDataInfo.ShapeTypes.Image:
                    this.xRectangle.Visibility = Visibility.Visible;
                    this.xVerticalPipe.Visibility = Visibility.Collapsed;
                    this.xHorizontalPipe.Visibility = Visibility.Collapsed;
                    break;
                case MapUniversalObjectDataInfo.ShapeTypes.VerticalPipe:
                    this.xRectangle.Visibility = Visibility.Collapsed;
                    this.xVerticalPipe.Visibility = Visibility.Visible;
                    this.xHorizontalPipe.Visibility = Visibility.Collapsed;
                    break;
                case MapUniversalObjectDataInfo.ShapeTypes.HorizontalPipe:
                    this.xRectangle.Visibility = Visibility.Collapsed;
                    this.xVerticalPipe.Visibility = Visibility.Collapsed;
                    this.xHorizontalPipe.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ApplyFill()
        {
            if (!this.isLoaded)
                return;

            switch (this.ShapeType)
            {
                case MapUniversalObjectDataInfo.ShapeTypes.Rectangle:
                case MapUniversalObjectDataInfo.ShapeTypes.VerticalPipe:
                case MapUniversalObjectDataInfo.ShapeTypes.HorizontalPipe:
                    var brush = BrushUtil.ConvertFromString(this.FillColor);
                    this.xRectangle.Fill = brush;
                    this.xVerticalPipe.Fill = brush;
                    this.xHorizontalPipe.Fill = brush;
                    break;

                case MapUniversalObjectDataInfo.ShapeTypes.Image:
                    this.xRectangle.Fill = BrushUtil.CreateImageBrush(this.FillImageUrl);
                    break;
            }
        }

        private void ApplyRectangleRadius()
        {
            if (!this.isLoaded)
                return;

            var maxSide = Math.Max(this.ControlSize.Width, this.ControlSize.Height);
            var radius = maxSide * this.StrokeRadius * 0.01;

            this.xRectangle.RadiusX = radius;
            this.xRectangle.RadiusY = radius;
        }
        #endregion // Shape

        #endregion // Metods
    }
}
