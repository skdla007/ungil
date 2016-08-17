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

namespace ArcGISControlDemo
{
    using ArcGISControls.MapTileImageProxy;

    /// <summary>
    /// Interaction logic for ProxyTest.xaml
    /// </summary>
    public partial class ProxyTest : Window
    {
        public ProxyTest()
        {
            InitializeComponent();
            this.xButtonStart.Click += XButtonStartOnClick;
            this.xButtonStop.Click += XButtonStopOnClick;
        }

        private void XButtonStartOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ProxyServer.Instance.Start(25000);
        }

        private void XButtonStopOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ProxyServer.Instance.Stop();
        }
    }
}
