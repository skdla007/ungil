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

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for WorkStationContextControl.xaml
    /// </summary>
    public partial class WorkStationContextControl : UserControl
    {
        public WorkStationContextControl()
        {
            InitializeComponent(); this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
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
    }
}
