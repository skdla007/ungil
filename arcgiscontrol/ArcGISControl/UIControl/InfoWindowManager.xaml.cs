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
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for LocationInfoWindow.xaml
    /// </summary>
    public partial class InfoWindowManager : UserControl
    {
        /// Popup Window Object
        protected InfoWindow locationInfoWindow;

        public InfoWindow LocationInfoWIndow
        {
            get { return this.locationInfoWindow; }
        }

        protected MapLocationObjectDataInfo mapLocationObjectData;

        public InfoWindowManager(Map map)
        {
            this.locationInfoWindow = new InfoWindow
            {
                Map = map,
                Background = null,
                BorderBrush = null
            };

            this.locationInfoWindow.MouseLeftButtonDown += InfoWindowOnMouseLeftButtonDown;
            this.locationInfoWindow.MouseLeftButtonUp += InfoWindowOnMouseLeftButtonUp;
            this.locationInfoWindow.MouseMove += InfoWindowOnMouseMove;
        }

        public void ChangeMap(Map map)
        {
            var infoWindow = this.locationInfoWindow;
            if (infoWindow != null)
                infoWindow.Map = map;
        }

        /// <summary>
        /// Show Popup Window
        /// </summary>
        /// <param name="mapLocationObjectData"></param>
        /// <param name="point"></param>
        public virtual void ShowInfoWindow(MapLocationObjectDataInfo mapLocationObjectData, MapPoint point)
        {
            if (this.locationInfoWindow == null) return;

            this.locationInfoWindow.IsOpen = false;

            if (mapLocationObjectData == null) return;

            this.mapLocationObjectData = mapLocationObjectData;

            this.locationInfoWindow.Anchor = point;
        }

        public void HideInfoWindow()
        {
            if (this.locationInfoWindow != null)
                this.locationInfoWindow.IsOpen = false;

            this.mapLocationObjectData = null;
        }

        public void ReleaseInfoWindow()
        {
            if (this.locationInfoWindow == null) return;

            this.locationInfoWindow.MouseLeftButtonDown -= InfoWindowOnMouseLeftButtonDown;
            this.locationInfoWindow.MouseLeftButtonUp -= InfoWindowOnMouseLeftButtonUp;
            this.locationInfoWindow.MouseMove -= InfoWindowOnMouseMove;
            this.locationInfoWindow = null;
        }

        private void InfoWindowOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.Cursor = Cursors.Arrow;
            mouseButtonEventArgs.Handled = true;
        }

        private void InfoWindowOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.Cursor = Cursors.Arrow;
            mouseButtonEventArgs.Handled = true;
        }

        private void InfoWindowOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            this.Cursor = Cursors.Arrow;
            mouseEventArgs.Handled = true;
        }
    }
}
