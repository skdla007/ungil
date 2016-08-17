using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
    /// <summary>
    /// Interaction logic for PanomorphOverlayUserControl.xaml
    /// </summary>
    public partial class PanomorphOverlayUserControl : UserControl
    {
        #region Viewer에 설정된 값들

        public static double PanomorphSelectionLimit = 10;

        public enum PanomorphViewTypes
        {
            PTZ,
            QUAD,
            PERI
        }
        
        #endregion //Viewer에 설정된 값들

        #region Fields

        private System.Timers.Timer panomorphPTZTimer;

        private string objectId;

        public bool IsControlOpened
        {
            get
            {
                return this.xControlVisibleToggleButton.IsChecked == true;
            }
            set
            {
                this.xControlPanelGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.xControlVisibleToggleButton.IsChecked = value;
            }
        }

        private bool enableSelection;
        public bool EnableSelection
        {
            get
            {
                return this.enableSelection;
            }
            set
            {
                this.enableSelection = value;
                this.SetSelectionMode(value);
                this.xSelectionToggleButton.IsChecked = value;
            }
        }

        private bool enablePtz;
        public bool EnablePTZ
        {
            get
            {
                return this.enablePtz;
            }
            set
            {
                this.enablePtz = value;
                if (value)
                {
                    this.xControlGrid.CaptureMouse();
                }
                else
                {
                    this.xControlGrid.ReleaseMouseCapture();
                }
            }
        }

        private bool isPanomorphControlEnabled;
        public bool IsPanomorphControlEnabled
        {
            get
            {
                return this.isPanomorphControlEnabled;
            }
            set
            {
                if (this.EnableSelection)
                {
                    this.isPanomorphControlEnabled = true;
                    this.Visibility = Visibility.Visible;
                    return;
                }

                this.isPanomorphControlEnabled = value;
                this.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.IsControlOpened = false;
            }
        }

        

        private PanomorphViewTypes panomorphViewType;
        public PanomorphViewTypes PanomorphViewType
        {
            get
            {
                return this.panomorphViewType;
            }
            set
            {
                if (this.panomorphViewType == value) return;

                this.panomorphViewType = value;

                this.IsControlOpened = false;

                if (this.EnableSelection)
                {
                    this.EnableSelection = false;
                }

                this.RaiseViewTypeChangedEvent(this.objectId, this.panomorphViewType);

                this.SetPanomorphViewTypeUI(value);
            }
        }


        private Point mouseDownPos;

        private Point mouseMovePos;

        private int mouseDownPanomorphViewNum;

        private Rect panomorphSelectionArea;

        private Rect panomorphSelectionBondary;

        private readonly TimeSpan invokeTimeOut = new TimeSpan(0, 0, 0, 100);

        #endregion //Fields
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectId">현재 Camera의 ObjectId</param>
        public PanomorphOverlayUserControl()
        {
            InitializeComponent();

            this.Visibility = Visibility.Collapsed;

            this.panomorphPTZTimer = new Timer();
            this.panomorphPTZTimer.Elapsed += this.panomorphPTZTimer_Tick;
            this.panomorphPTZTimer.Interval = 100;

            this.xControlPanelGrid.Visibility = Visibility.Collapsed;
            this.xControlVisibleToggleButton.IsChecked = false;
            
            this.xControlVisibleToggleButton.Click += ControlVisibleToggleButtonClick;
            this.xDefaultViewButton.Click += XDefaultViewButtonOnClick;
            this.xPtzViewRadioButton.Click += XPtzViewRadioButtonOnClick;
            this.xPeriViewRadioButton.Click += XPeriViewRadioButtonOnClick;
            this.xQuadViewRadioButton.Click += XQuadViewRadioButtonOnClick;
            this.xSelectionToggleButton.Click += XSelectionToggleButtonOnClick;

            this.xControlGrid.MouseLeftButtonDown += XControlGridOnMouseLeftButtonDown;
            this.xControlGrid.MouseLeftButtonUp += XControlGridOnMouseLeftButtonUp;
            this.xControlGrid.MouseRightButtonDown += XControlGridOnMouseRightButtonDown;
            this.xControlGrid.MouseRightButtonUp += XControlGridOnMouseRightButtonUp;
            this.xControlGrid.MouseMove += XControlGridOnMouseMove;
        }


        #region Event Handlers

        private void panomorphPTZTimer_Tick(object sender, EventArgs e)
        {
            if (!this.enablePtz)
            {
                return;
            }

            bool leftButtonPressed = false;
            bool rightButtonPressed = false;
            bool isTimeout = true;

            this.Dispatcher.Invoke(new Action(() =>
            {
                isTimeout = false;
                leftButtonPressed = Mouse.LeftButton == MouseButtonState.Pressed;
                rightButtonPressed = Mouse.RightButton == MouseButtonState.Pressed;
            }), invokeTimeOut);

            if (isTimeout)
            {
                return;
            }

            if (!leftButtonPressed && !rightButtonPressed)
            {
                this.enablePtz = false;
                this.panomorphPTZTimer.Stop();
                return;
            }

            this.Dispatcher.Invoke(
                       new Action(delegate
                                      {
                                          var x = (this.mouseMovePos.X - this.mouseDownPos.X);
                                          var y = (this.mouseMovePos.Y - this.mouseDownPos.Y);
                           this.RaiseViewDragChangingEvent(this.objectId, this.mouseDownPanomorphViewNum, new Point(x,y), leftButtonPressed);
                       }));
        }
        
        private void XSelectionToggleButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.EnableSelection = this.xSelectionToggleButton.IsChecked == true; 
        }

        private void XQuadViewRadioButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.PanomorphViewType = PanomorphViewTypes.QUAD;
        }

        private void XPeriViewRadioButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.PanomorphViewType = PanomorphViewTypes.PERI;
        }

        private void XPtzViewRadioButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.PanomorphViewType = PanomorphViewTypes.PTZ;
        }

        private void XDefaultViewButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.RaiseDefaultViewChangingEvent(this.objectId, this.panomorphViewType.ToString());
        }

        private void ControlVisibleToggleButtonClick(object sender, RoutedEventArgs e)
        {
            this.IsControlOpened = this.xControlVisibleToggleButton.IsChecked == true;
        }

        private void XControlGridOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.IsControlOpened = false;

            this.mouseDownPos = mouseButtonEventArgs.GetPosition(this);

            if (this.mouseDownPos.X < 0 || this.mouseDownPos.Y < 0 || this.mouseDownPos.X > this.ActualWidth || this.mouseDownPos.Y > this.ActualHeight)
            {
                if (this.EnableSelection)
                {
                    this.EnableSelection = false;
                }

                return;
            }

            this.mouseDownPanomorphViewNum = this.GetViewNum(this.panomorphViewType, this.mouseDownPos);

            if (mouseButtonEventArgs.ClickCount == 2)
            {
                this.EnableSelection = false;

                this.RaiseViewPositionChangingEvent(this.objectId, this.mouseDownPanomorphViewNum, this.mouseDownPos);

                return;
            }

            if (this.EnableSelection)
            {
                this.mouseMovePos = this.mouseDownPos;

                this.panomorphSelectionArea.X = 0;
                this.panomorphSelectionArea.Y = 0;
                this.panomorphSelectionArea.Width = 0;
                this.panomorphSelectionArea.Height = 0;

                this.xPanomorphSelectionRectangle.Width = 0;
                this.xPanomorphSelectionRectangle.Height = 0;

                this.xPanomorphSelectionRectangle.Visibility = Visibility.Visible;

                this.SetSelectionBoundary(this.panomorphViewType, this.mouseDownPanomorphViewNum, this.mouseMovePos);

            }
            else
            {
                this.EnablePTZ = true;
            }

            mouseButtonEventArgs.Handled = true;
        }

        private void XControlGridOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (this.EnableSelection)
            {
                this.EnableSelection = false;
                this.RaiseViewRectChangingEvent(this.objectId, this.mouseDownPanomorphViewNum, this.panomorphSelectionArea);
            }

            if (this.EnablePTZ)
            {
                this.EnablePTZ = false;
                if (panomorphPTZTimer != null) this.panomorphPTZTimer.Stop();
            }
        }

        private void XControlGridOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.IsControlOpened = false;

            this.mouseDownPos = mouseButtonEventArgs.GetPosition(this);

            if (this.mouseDownPos.X < 0 || this.mouseDownPos.X > this.ActualWidth || this.mouseDownPos.Y > this.ActualHeight)
            {
                if (this.EnableSelection)
                {
                    this.EnableSelection = false;
                }

                return;
            }

            this.mouseDownPanomorphViewNum = this.GetViewNum(this.panomorphViewType, this.mouseDownPos);

            if (this.EnableSelection)
            {
                this.EnableSelection = false;
            }
            else
            {
                this.EnablePTZ = true;
            }
        }

        private void XControlGridOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (this.EnablePTZ)
            {
                this.EnablePTZ = false;
                this.panomorphPTZTimer.Stop();
            }
        }

        private void XControlGridOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            this.mouseMovePos = mouseEventArgs.GetPosition(this);

            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed || mouseEventArgs.RightButton == MouseButtonState.Pressed)
            {
                if (this.EnableSelection)
                {
                    this.panomorphSelectionArea.X = this.mouseDownPos.X < this.mouseMovePos.X ? this.mouseDownPos.X : this.mouseMovePos.X;
                    this.panomorphSelectionArea.Y = this.mouseDownPos.Y < this.mouseMovePos.Y ? this.mouseDownPos.Y : this.mouseMovePos.Y;
                    this.panomorphSelectionArea.Width = Math.Abs(mouseMovePos.X - mouseDownPos.X);
                    this.panomorphSelectionArea.Height = Math.Abs(mouseMovePos.Y - mouseDownPos.Y);

                    this.panomorphSelectionArea.Intersect(this.panomorphSelectionBondary);

                    this.xPanomorphSelectionRectangle.Margin = new Thickness(
                        this.panomorphSelectionArea.X,
                        this.panomorphSelectionArea.Y,
                        0, 0);

                    this.xPanomorphSelectionRectangle.Width = this.panomorphSelectionArea.Width;
                    this.xPanomorphSelectionRectangle.Height = this.panomorphSelectionArea.Height;
                }
                else
                {
                    if (this.EnablePTZ && !this.panomorphPTZTimer.Enabled)
                    {
                        this.panomorphPTZTimer.Start();
                    }
                }
            }
        }

        #endregion Event Hadlers

        #region Methods

        public void Initailize(string objectId)
        {
            this.objectId = objectId;
        }

        public void SetPanomorphViewTypeUI(PanomorphViewTypes type)
        {
            switch (type)
            {
                case PanomorphViewTypes.PTZ:
                    this.xPtzViewRadioButton.IsChecked = true;
                    break;

                case PanomorphViewTypes.PERI:
                    this.xPeriViewRadioButton.IsChecked = true;
                    break;

                case PanomorphViewTypes.QUAD:
                    this.xQuadViewRadioButton.IsChecked = true;
                    break;
            }

            this.panomorphViewType = type;
        }

        private void SetSelectionMode(bool aEnable)
        {  
            this.IsControlOpened = false;

            this.xSelectionToggleButton.IsChecked = aEnable;

            if (aEnable)
            {
                this.IsPanomorphControlEnabled = true;
                this.xPanomorphSelectionRectangle.Width = 0;
                this.xPanomorphSelectionRectangle.Height = 0;
                this.xPanomorphSelectionRectangle.Visibility = Visibility.Visible;
                this.xControlGrid.CaptureMouse();
                this.Cursor = Cursors.Cross;
            }
            else
            {
                this.xPanomorphSelectionRectangle.Visibility = Visibility.Collapsed;
                this.Cursor = Cursors.Arrow;
                this.xControlGrid.ReleaseMouseCapture();
            }
        }

        private void SetSelectionBoundary(PanomorphViewTypes aViewType, int aViewNum, Point aMousePos)
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            var halfWidth = this.ActualWidth / 2;
            var halfHeight = this.ActualHeight / 2;

            switch (aViewType)
            {
                case PanomorphViewTypes.PTZ:
                    this.panomorphSelectionBondary.X = 0;
                    this.panomorphSelectionBondary.Y = 0;
                    this.panomorphSelectionBondary.Width = width;
                    this.panomorphSelectionBondary.Height = height;
                    break;

                case PanomorphViewTypes.PERI:
                    if (aMousePos.Y < halfHeight)
                    {
                        this.panomorphSelectionBondary.X = 0;
                        this.panomorphSelectionBondary.Y = 0;
                        this.panomorphSelectionBondary.Width = width;
                        this.panomorphSelectionBondary.Height = halfHeight;
                    }
                    else
                    {
                        this.panomorphSelectionBondary.X = 0;
                        this.panomorphSelectionBondary.Y = halfHeight;
                        this.panomorphSelectionBondary.Width = width;
                        this.panomorphSelectionBondary.Height = halfHeight;
                    }
                    break;

                case PanomorphViewTypes.QUAD:
                    switch (aViewNum)
                    {
                        case 1:
                            this.panomorphSelectionBondary.X = 0;
                            this.panomorphSelectionBondary.Y = 0;
                            this.panomorphSelectionBondary.Width = halfWidth;
                            this.panomorphSelectionBondary.Height = halfHeight;
                            break;

                        case 2:
                            this.panomorphSelectionBondary.X = halfWidth;
                            this.panomorphSelectionBondary.Y = 0;
                            this.panomorphSelectionBondary.Width = halfWidth;
                            this.panomorphSelectionBondary.Height = halfHeight;
                            break;

                        case 3:
                            this.panomorphSelectionBondary.X = 0;
                            this.panomorphSelectionBondary.Y = halfHeight;
                            this.panomorphSelectionBondary.Width = halfWidth;
                            this.panomorphSelectionBondary.Height = halfHeight;
                            break;

                        case 4:
                            this.panomorphSelectionBondary.X = halfWidth;
                            this.panomorphSelectionBondary.Y = halfHeight;
                            this.panomorphSelectionBondary.Width = halfWidth;
                            this.panomorphSelectionBondary.Height = halfHeight;
                            break;
                    }
                    break;
            }
        }

        private int GetViewNum(PanomorphViewTypes aViewType, Point aPos)
        {
            var halfWidth = this.ActualWidth / 2;
            var halfHeight = this.ActualHeight / 2;

            var viewNum = 1;

            if (aViewType == PanomorphViewTypes.QUAD)
            {
                if (aPos.X > halfWidth)
                {
                    viewNum++;
                }

                if (aPos.Y > halfHeight)
                {
                    viewNum += 2;
                }
            }

            return viewNum;
        }

        #endregion //Methods

        #region Events

        public class ViewTypeChangedEventArgs : EventArgs
        {
            public string ObjectId { get; set; }
            public string PanomorphViewType { get; set; }

            public ViewTypeChangedEventArgs(string objectId, string panomorphViewType)
            {
                this.ObjectId = objectId;
                this.PanomorphViewType = panomorphViewType;
            }
        }

        /// <summary>
        /// Panomorph_SetViewType 대체
        /// 
        /// ViewType이 변경 될 경우 호출
        /// </summary>
        public event EventHandler<ViewTypeChangedEventArgs> eViewTypeChanged;

        public void RaiseViewTypeChangedEvent(string objectId, PanomorphViewTypes panomorphViewType)
        {
            var handler = this.eViewTypeChanged;
            if (handler != null)
            {
                handler(this, new ViewTypeChangedEventArgs(objectId, panomorphViewType.ToString()));
            }
        }

        public class PanomorphControlViewChangingEventArgs : EventArgs
        {
            public string ObjectId { get; set; }
            public int ViewNumber { get; set; }
           

            public PanomorphControlViewChangingEventArgs(string objectId, int viewNumber)
            {
                this.ObjectId = objectId;
                this.ViewNumber = viewNumber;
            }
        }

        public class ViewRectChangingEventArgs : PanomorphControlViewChangingEventArgs
        {
            public Rect SelectionArea { get; set; }

            public ViewRectChangingEventArgs(string objectId, int viewNumber, Rect selectionArea)
                : base(objectId, viewNumber)
            {
                this.SelectionArea = selectionArea;
            }
        }

        /// <summary>
        /// Panomorph_MoveViewArea 대체
        /// </summary>
        public event EventHandler<ViewRectChangingEventArgs> eViewRectChanging;

        private void RaiseViewRectChangingEvent(string objectId, int viewNumber, Rect selectionArea)
        {
            if (selectionArea.Width < PanomorphOverlayUserControl.PanomorphSelectionLimit && selectionArea.Height < PanomorphOverlayUserControl.PanomorphSelectionLimit)
            {
                return;
            }
            
            var handler = this.eViewRectChanging;
            if (handler != null)
            {
                handler(this, new ViewRectChangingEventArgs(objectId, viewNumber, panomorphSelectionArea));
            }

            this.xPtzViewRadioButton.IsChecked = true;
            this.panomorphViewType = PanomorphViewTypes.PTZ;
        }

        public class ViewPositionChangingEventArgs : PanomorphControlViewChangingEventArgs
        {
            public Point PanomorphMovedPoint { get; set; }

            public ViewPositionChangingEventArgs(string objectId, int viewNumber, Point panomorphMovedPoint)
                : base(objectId, viewNumber)
            {
                this.PanomorphMovedPoint = panomorphMovedPoint;
            }
        }

        /// <summary>
        /// Panomorph_MoveViewPosition 대체
        /// </summary>
        public event EventHandler<ViewPositionChangingEventArgs> eViewPositionChanging;

        private void RaiseViewPositionChangingEvent(string objectId, int viewNumber, Point panomorphMovedPoint)
        {
            var handler = this.eViewPositionChanging;
            if (handler != null)
            {
                handler(this, new ViewPositionChangingEventArgs(objectId, viewNumber, panomorphMovedPoint));
            }

            this.xPtzViewRadioButton.IsChecked = true;
            this.panomorphViewType = PanomorphViewTypes.PTZ;
        }
        
        
        public class ViewDragChangingEventArgs : ViewPositionChangingEventArgs
        {
            public enum ButtonType
            {
                Right = 1,
                Left = 2
            }

            public bool IsLeftButtonDown;

            public ViewDragChangingEventArgs(string objectId, int viewNumber, Point panomorphMovedPoint, bool isLeftButtonDown)
                : base(objectId, viewNumber, panomorphMovedPoint)
            {
                this.IsLeftButtonDown = isLeftButtonDown;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ViewDragChangingEventArgs> eViewDragChanging;

        private void RaiseViewDragChangingEvent(string objectId, int viewNumber, Point mouseDownPosition, bool isLeftButtonDown)
        {
            var handler = this.eViewDragChanging;
            if (handler != null)
            {
                handler(this, new ViewDragChangingEventArgs(objectId, viewNumber, mouseDownPosition, isLeftButtonDown));
            }
        }

        public event EventHandler<ViewTypeChangedEventArgs> eDefaultViewChanging;

        private void RaiseDefaultViewChangingEvent(string objectId, string panomorphCoordinateType)
        {
            
            var handler = this.eDefaultViewChanging;
            if (handler != null)
            {
                handler(this, new ViewTypeChangedEventArgs(objectId, panomorphCoordinateType));
            }
        }

        #endregion //Events
    }
}
