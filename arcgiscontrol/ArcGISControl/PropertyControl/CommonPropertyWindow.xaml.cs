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
using System.Windows.Shapes;

namespace ArcGISControl.PropertyControl
{
    /// <summary>
    /// CommonPropertyWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommonPropertyWindow : Window
    {
        public CommonPropertyWindow(Window owner)
        {
            InitializeComponent();

            this.MouseLeftButtonDown += CommonPropertyWindow_MouseLeftButtonDown;
            this.Owner = owner;
        }

        private void CommonPropertyWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }
    }
}
