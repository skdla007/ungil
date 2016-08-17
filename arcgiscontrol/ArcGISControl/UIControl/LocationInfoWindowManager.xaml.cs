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
using ESRI.ArcGIS.Client;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for LocationInfoWindowManager.xaml
    /// </summary>
    public partial class LocationInfoWindowManager : InfoWindowManager
    {
        public LocationInfoWindowManager(Map map) : base(map)
        {
            InitializeComponent();

            locationInfoWindow.ContentTemplate =
               this.Resources["InfoWindowTemplate"] as System.Windows.DataTemplate;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.locationInfoWindow.IsOpen = false;
        }

        public override void ShowInfoWindow(MapLocationObjectDataInfo mapLocationObjectData, ESRI.ArcGIS.Client.Geometry.MapPoint point)
        {
            base.ShowInfoWindow(mapLocationObjectData, point);

            this.locationInfoWindow.Content = mapLocationObjectData;
            this.locationInfoWindow.IsOpen = true;
        }
    }
}
