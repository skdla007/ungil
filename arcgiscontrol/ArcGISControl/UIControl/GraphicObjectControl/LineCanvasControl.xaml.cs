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
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    /// <summary>
    /// Interaction logic for LineCanvasControl.xaml
    /// </summary>
    public partial class LineCanvasControl : UserControl
    {
        private Line Line;
        private bool _Flag = false;

        public ArcGISClientViewer ArcGISClientVW { get; set; }
        public List<Point> LineControlPoint { get; set; }
        public Line DrawLine{ get { return Line; }}
        public Point FirstClickPosition { get; set; }

        public LineCanvasControl()
        {
            InitializeComponent();

            if (this.DrawLineCtrl.Children.Count >= 1) return;
            if (_Flag) return;

            Point MousPoint = Mouse.GetPosition(DrawLineCtrl);

            Line = new Line();
            Line.Stroke = Brushes.Black;
            Line.StrokeThickness = 2;
            Line.X1 = Line.X2 = MousPoint.X;
            Line.Y1 = Line.Y2 = MousPoint.Y;

            this.DrawLineCtrl.Children.Add(Line);
        }
        
        void ArcGISClientVW_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_Flag)
            {
                Point MousPoint = Mouse.GetPosition(this.DrawLineCtrl);

                (this.DrawLineCtrl.Children[this.DrawLineCtrl.Children.IndexOf(Line)] as Line).X2 = MousPoint.X;
                (this.DrawLineCtrl.Children[this.DrawLineCtrl.Children.IndexOf(Line)] as Line).Y2 = MousPoint.Y;
            }

            e.Handled = true;
        }


        public event EventHandler ControlAdded;

        private void ArcGISClient_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_Flag && Line != null)
            {
                this.EndDrawLine();
                ArcGISClientVW.MouseMove -= ArcGISClientVW_MouseMove;
                
                if(ControlAdded != null)
                ControlAdded(this, new EventArgs());
            }
        }

        private void EndDrawLine()
        {
            Line.UpdateLayout();
            double w = 0;
            double h = 0;
            if (Line.X1 == Line.X2)
            {
                w = 0;
            }
            else if (Line.X1 > Line.X2)
            {
                w = Line.X1 - Line.X2;
            }
            else
            {
                w = Line.X2 - Line.X1;
            }
            if (Line.Y1 == Line.Y2)
            {
                h = 0;
            }
            else if (Line.Y1 > Line.Y2)
            {
                h = Line.Y1 - Line.Y2;
            }
            else
            {
                h = Line.Y2 - Line.Y1;
            }

            Line.Width = w;
            Line.Height = h;

            DrawLineControl.UpdateLayout();
            this.DrawLineCtrl.UpdateLayout();

            _Flag = true;
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_Flag)
            {
                this.Focus();
                ArcGISClientVW.MouseMove += ArcGISClientVW_MouseMove;
                ArcGISClientVW.MouseLeftButtonUp += ArcGISClient_MouseLeftButtonUp;
            }
        }
    }
}
