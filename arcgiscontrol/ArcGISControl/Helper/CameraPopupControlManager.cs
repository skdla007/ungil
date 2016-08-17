using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.UIControl;
using System.Windows.Controls;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Interface;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.Helper
{
    public class CameraPopupControlManager
    {
        #region filed

        private CameraPopupControl cameraPopupControl;

        private CameraPopupControlViewModel viewModel;

        private int zIndex;

        public int ZIndex
        {
            get { return this.zIndex; }
            set
            {
                this.zIndex = value;

                if (this.cameraPopupControl != null && this.cameraPopupControl.Parent != null)
                {
                    Panel.SetZIndex(this.cameraPopupControl, value);
                }
            }
        }

        /// <summary>
        /// 현재 Popup 에서의 Unigrid ID
        /// </summary>
        private string cameraUnigridId;
        public string CameraUnigridId
        {
            get { return cameraUnigridId; }
            set { this.cameraUnigridId = value; }
        }
        
        /// <summary>
        /// Camera 객체의 DB ID 
        /// 값 받아올때 쓰임
        /// </summary>
        private string camerainformationId;
        public string CameraInformationId
        {
            get { return this.camerainformationId; }
            set { this.camerainformationId = value; }
        }

        /// <summary>
        /// 현재 Popup의 기본 CameraUnigridID
        /// </summary>
        public string OriginalCameraUnigridId
        {
            get { return this.selectedCameraIconGraphic.ObjectID; }
        }

        public bool IsShowCameraPoupupControl
        {
            get { return this.cameraPopupControl.Visibility == Visibility.Visible; }
        }

        private bool canView = false;
        public bool CanView
        {
            get { return this.canView; }
            set 
            {
                this.canView = !string.IsNullOrEmpty(this.CameraUnigridId) && value;
            }
        }
        
        /// <summary>
        /// Show가 되면 IconGraphic 이 Setting 된다.
        /// </summary>
        private CameraIconGraphic selectedCameraIconGraphic;

        #endregion

        #region Events

        public EventHandler<CameraPopupControlViewModel.PresetListSelectionChangedEventArgs> ePresetListSelectionChanged;

        private void RaisePresetListSelectionEvent(CameraPopupControlViewModel.PresetListSelectionChangedEventArgs eventArgs)
        {
            var handler = this.ePresetListSelectionChanged;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public EventHandler eCloseButtonClicked;

        private void RaiseCloseButtonClickedEvent()
        {
            var handler = this.eCloseButtonClicked;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        public EventHandler eControlOpend;

        private void RaiseControlOpendEvent()
        {
            if(string.IsNullOrEmpty(this.CameraInformationId) ||
                string.IsNullOrEmpty(this.OriginalCameraUnigridId) ||
                string.IsNullOrEmpty(this.CameraUnigridId))
                return;

            var handler = this.eControlOpend;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        #endregion Events

        #region constructor

        public CameraPopupControlManager()
        {
            this.cameraPopupControl = new CameraPopupControl();
            this.viewModel = new CameraPopupControlViewModel();
            this.cameraPopupControl.DataContext = this.viewModel; // Data Binding 또는 Command를 실행해주기 위한 DataContext viewModel 설정.
            this.cameraPopupControl.Visibility = Visibility.Collapsed;
            this.viewModel.ePresetListSelectionChanged += ViewModel_ePresetListSelectionChanged;
            this.viewModel.eCloseButtonClicked += ViewModel_eCloseButtonClicked;
            this.cameraPopupControl.xCameraCanvas.Loaded += XCameraCanvasOnLoaded;
        }
        
        #endregion

        #region methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public void AddToParent(Panel parent)
        {
            parent.Children.Add(this.cameraPopupControl);
            Panel.SetZIndex(this.cameraPopupControl, this.zIndex);
        }

        public void RemoveFromParent(Panel parent)
        {
            parent.Children.Remove(this.cameraPopupControl);
        }

        /// <summary>
        /// Show
        /// </summary>
        /// <param name="mapCameraObjectComponentData"></param>
        /// <param name="cameraIconGraphic"></param>
        /// <returns></returns>
        public bool Show(MapCameraObjectComponentDataInfo mapCameraObjectComponentData, CameraIconGraphic cameraIconGraphic)
        {
            this.cameraPopupControl.Visibility = Visibility.Visible;

            this.selectedCameraIconGraphic = cameraIconGraphic;

            if (selectedCameraIconGraphic == null) return false;
            
            if (mapCameraObjectComponentData == null)
                return false;

            if (!this.cameraPopupControl.xCameraGrid.Children.Contains(this.cameraPopupControl.xCameraCanvas))
            {
                this.cameraPopupControl.xCameraGrid.Children.Add(this.cameraPopupControl.xCameraCanvas);
            }

            this.viewModel.CameraName = mapCameraObjectComponentData.Name;
            this.viewModel.PresetList = mapCameraObjectComponentData.PresetList;
            this.CameraInformationId = mapCameraObjectComponentData.CameraInformationID;

            if (mapCameraObjectComponentData.Video.StreamSizeList != null)
            {
                cameraPopupControl.xCameraCanvas.Height = (cameraPopupControl.xCameraCanvas.Width * (mapCameraObjectComponentData.Video.StreamSizeList[0].Height / mapCameraObjectComponentData.Video.StreamSizeList[0].Width));
            }

            return true;
        }

        /// <summary>
        /// Hide
        /// </summary>
        public void Hide()
        {
            this.cameraPopupControl.Visibility = Visibility.Collapsed;

            

            this.CameraUnigridId = string.Empty;
            this.CameraInformationId = string.Empty;
            this.selectedCameraIconGraphic = null;
            
            if (this.cameraPopupControl.xCameraGrid.Children.Contains(this.cameraPopupControl.xCameraCanvas))
            {
                this.cameraPopupControl.xCameraGrid.Children.Remove(this.cameraPopupControl.xCameraCanvas);
            }
        }

        /// <summary>
        /// Move
        /// </summary>
        /// <param name="mapResolution"></param>
        /// <param name="xMin"></param>
        /// <param name="yMax"></param>
        public void Move(double mapResolution, double xMin, double yMax)
        {
            if(this.selectedCameraIconGraphic == null) return;

            var mapPoint = this.selectedCameraIconGraphic.Position;

            var x = (mapPoint.X - xMin) / mapResolution;
            var y = ((yMax - mapPoint.Y) / mapResolution - this.selectedCameraIconGraphic.OffsetPoint.Y);
            
            if (double.IsInfinity(x) || double.IsNaN(x) || double.IsInfinity(y) || double.IsNaN(y))
                return;

            Canvas.SetLeft(this.cameraPopupControl, (x - (this.cameraPopupControl.Width / 2)));
            Canvas.SetTop(this.cameraPopupControl, (y - (this.cameraPopupControl.Height)));
        }

        public Size GetVideoSize()
        {
            var size = new Size();

            if (this.cameraPopupControl != null && this.cameraPopupControl.xCameraCanvas != null)
            {
                size = new Size(this.cameraPopupControl.xCameraCanvas.Width, this.cameraPopupControl.xCameraCanvas.Height);
            }

            return size;
        }


        /// <summary>
        /// Camera 표출할 Rect의 위치
        /// </summary>
        public Rect GetTotalVideoRect(Point mapStartPoint)
        {
            var rect = new Rect();

            if (this.cameraPopupControl != null && this.cameraPopupControl.xCameraCanvas != null)
            {
                var controlPointX = Canvas.GetLeft(this.cameraPopupControl);
                var controlPointY = Canvas.GetTop(this.cameraPopupControl);

                var canvasPointX = 8;
                var canvasPointY = 30;

                rect = new Rect(mapStartPoint.X + controlPointX + canvasPointX, mapStartPoint.Y + controlPointY + canvasPointY,
                                this.cameraPopupControl.xCameraCanvas.Width,
                                this.cameraPopupControl.xCameraCanvas.Height);
            }

            return rect;
        }

        /// <summary>
        /// Image Rect 를 실제 화면 갯수에 맞추어 작업
        /// </summary>
        /// <param name="addedRectCount"></param>
        public void ControlVideoImageRect(int addedRectCount)
        {
            var cameraCanvas = this.cameraPopupControl.xCameraCanvas;

            if(cameraCanvas == null) return;

            if(cameraCanvas.Children.Count != addedRectCount)
            {
                cameraCanvas.Children.Clear();

                for (int i = 0; i < addedRectCount; i++)
                {
                    cameraCanvas.Children.Add(new Rectangle());
                }
            }
        }

        /// <summary>
        /// 선택된 Rect 받아오기
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Rectangle GetImageRect(int index)
        {
            var cameraCanvas = this.cameraPopupControl.xCameraCanvas;

            if(cameraCanvas == null || cameraCanvas.Children.Count <= index) return null;

            return cameraCanvas.Children[index] as Rectangle;
        }

        #endregion methos

        #region event Handlers

        private void ViewModel_ePresetListSelectionChanged(object sender, CameraPopupControlViewModel.PresetListSelectionChangedEventArgs presetListSelectionChangedEventArgs)
        {
            this.RaisePresetListSelectionEvent(presetListSelectionChangedEventArgs);
        }

        private void ViewModel_eCloseButtonClicked(object sender, EventArgs eventArgs)
        {   
            this.RaiseCloseButtonClickedEvent();
        }

        private void XCameraCanvasOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.RaiseControlOpendEvent();
        }

        #endregion //event Handlers
    }
}
