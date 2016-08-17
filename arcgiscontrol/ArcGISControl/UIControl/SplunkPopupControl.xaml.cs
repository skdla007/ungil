using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for TablePopupControl.xaml
    /// </summary>
    public partial class SplunkPopupControl : UserControl
    {
        private bool resized;
        private bool useResize;

        public bool UseResize
        {
            get
            {
                return this.useResize;
            }

            set
            {
                this.useResize = value;

                this.xResizeControl.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool Resized
        {
            get
            {
                return this.resized;
            }

            private set
            {
                this.resized = value;

                if (value)
                {
                    this.xSplunkChartTableControl.TableControl.MaxWidth = double.PositiveInfinity;
                    this.xSplunkChartTableControl.TableControl.MaxHeight = double.PositiveInfinity;
                }
                else
                {
                    this.xSplunkChartTableControl.TableControl.MaxWidth = ArcGISConstSet.SplunkTableMaxWidth;
                    this.xSplunkChartTableControl.TableControl.MaxHeight = ArcGISConstSet.SplunkTableMaxHeight;
                }
            }
        }

        public event EventHandler eResizing;

        public void OnEResizing(EventArgs e)
        {
            var handler = eResizing;
            if (handler != null) handler(this, e);
        }

        public SplunkPopupControl()
        {
            InitializeComponent();

            this.SetDefaultSize();

            this.xSplunkChartTableControl.xControlWrappingViewbox.Stretch = Stretch.None;
            this.xSplunkChartTableControl.onChangedSplunkControl += xSplunkChartTableControl_onChangedSplunkControl;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void xSplunkChartTableControl_onChangedSplunkControl(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.xSplunkChartTableControl.ErrorMessage))
            {
                this.UseResize = true;
            }
            else
            {
                this.UseResize = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.MouseDown += OnMouseDown;
            this.MouseEnter += OnMouseEnter;
        }

        private void OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.MouseDown -= OnMouseDown;
            this.MouseEnter -= OnMouseEnter;
        }

        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var element = sender as FrameworkElement;

            if (element == null)
            {
                return;
            }

            this.Resized = true;

            double deltaVertical, deltaHorizontal;

            double w = this.ActualWidth, h = this.ActualHeight;

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    deltaVertical = Math.Min(-e.VerticalChange, this.ActualHeight - this.MinHeight);
                    h = this.ActualHeight - deltaVertical;
                    break;

                case VerticalAlignment.Top:
                    deltaVertical = Math.Min(e.VerticalChange, this.ActualHeight - this.xSplunkChartTableControl.TableControl.MinHeight);
                    Canvas.SetTop(this, Canvas.GetTop(this) + deltaVertical);
                    h = this.ActualHeight - deltaVertical;
                    break;
            }

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    deltaHorizontal = Math.Min(e.HorizontalChange, this.ActualWidth - this.xSplunkChartTableControl.TableControl.MinWidth);
                    Canvas.SetLeft(this, Canvas.GetLeft(this) + deltaHorizontal);
                    w = this.ActualWidth - deltaHorizontal;
                    break;

                case HorizontalAlignment.Right:
                    deltaHorizontal = Math.Min(-e.HorizontalChange, this.ActualWidth - this.MinWidth);
                    w = this.ActualWidth - deltaHorizontal;
                    break;
            }
            
            this.xSplunkChartTableControl.SetSplunkControlSize(w, h);

            e.Handled = true;

            this.OnEResizing(new EventArgs());
        }

        public void SetDefaultSize()
        {
            this.Resized = false;

            this.xSplunkChartTableControl.SetSplunkControlSize(double.NaN, double.NaN);
        }
    }
}
