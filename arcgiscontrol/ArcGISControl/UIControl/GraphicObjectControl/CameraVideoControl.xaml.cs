using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    public partial class CameraVideoControl : UserControl
    {
        public static readonly DependencyProperty HideImageVisibilityProperty =
            DependencyProperty.Register("HideImageVisibility", typeof(Visibility), typeof(CameraVideoControl));

        public Visibility HideImageVisibility
        {
            get { return (Visibility)this.GetValue(HideImageVisibilityProperty); }
            set { this.SetValue(HideImageVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SelectionBorderVisibilityProperty =
            DependencyProperty.Register("SelectionBorderVisibility", typeof(Visibility), typeof(CameraVideoControl), new PropertyMetadata(Visibility.Collapsed));

        public Visibility SelectionBorderVisibility
        {
            get { return (Visibility)this.GetValue(SelectionBorderVisibilityProperty); }
            set { this.SetValue(SelectionBorderVisibilityProperty, value); }
        }

        public CameraVideoControl()
        {
            InitializeComponent();
        }

        public void EnsureRectangleCount(int rectangleCount)
        {
            if (rectangleCount < 0)
                throw new ArgumentOutOfRangeException("rectangleCount");

            var childrenCount = this.xCanvas.Children.Count;

            if (childrenCount < rectangleCount)
            {
                for (var i = 0; i < rectangleCount - childrenCount; i++)
                    this.xCanvas.Children.Add(new Rectangle());
            }
            else if (childrenCount > rectangleCount)
            {
                this.xCanvas.Children.RemoveRange(childrenCount - 1, childrenCount - rectangleCount);
            }
        }

        public void SetPositionAndBrush(int rectangleIndex, double left, double top, double width, double height, Brush brush)
        {
            if (rectangleIndex < 0 || rectangleIndex >= this.xCanvas.Children.Count)
                throw new ArgumentOutOfRangeException("rectangleIndex");

            var rectangle = this.xCanvas.Children[rectangleIndex] as Rectangle;
            System.Diagnostics.Debug.Assert(rectangle != null);

            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Fill = brush;
        }
    }
}
