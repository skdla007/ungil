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
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Interface;

namespace ArcGISControlDemo
{
    /// <summary>
    /// Interaction logic for CameraOverlayControlTest.xaml
    /// </summary>
    public partial class CameraOverlayControlTest : Window
    {
        private CameraOverlayControl cameraOverlayControl;
        private IArcGISControlViewerAPI arcGisControlViewerApi;
 
        public CameraOverlayControlTest()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.cameraOverlayControl = new CameraOverlayControl(arcGisControlViewerApi);

            this.cameraOverlayControl.Loaded += CameraOverlayControlOnLoaded;
            
            this.xGrid.Children.Add(this.cameraOverlayControl);
            xGrid.MouseEnter += CameraOverlayControlOnMouseEnter;
            xGrid.MouseLeave += cameraOverlayControl_MouseLeave;
            //cameraOverlayControl.PreviewMouseLeftButtonDown += cameraOverlayControl_PreviewMouseLeftButtonDown;
        }

        private void CameraOverlayControlOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            if(this.cameraOverlayControl.Visibility != Visibility.Visible)
            {
                
            }
            /*
            this.cameraOverlayControl.Initialize(
              Guid.NewGuid().ToString(),
              "",
              true,
              false,
              true,
              string.Format("000"));
            */
            this.cameraOverlayControl.Visibility = Visibility.Visible;

            //this.RefreshCameraOverlayControlPosition(displayRect);
        }

        private void CameraOverlayControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        #region Camera Overlay Control Envet Handler

        private void cameraOverlayControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.HideCameraOverlayControl();
        }

        #endregion // Camera Overlay Control Envet Handler

        private void HideCameraOverlayControl()
        {   
            {
                this.cameraOverlayControl.Visibility = Visibility.Collapsed;
            }
        }

    }
}
