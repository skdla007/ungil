
using System.Runtime.InteropServices;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for RdsOverlayControl.xaml
    /// </summary>
    public partial class RdsOverlayControl : UserControl
    {
        private bool enableRdsControl;

        private const int KEYDOWN = 9;
        private const int KEYUP = 10;
        private const int MOUSELBUTTONDOWN = 2;
        private const int MOUSELBUTTONUP = 3;
        private const int MOUSEMOVE = 1;
        private const int MOUSERBUTTONDOWN = 5;
        private const int MOUSERBUTTONUP = 6;
        private const int MOUSEWHEEL = 8;

        private const int ScreenWidth = 65535;
        private const int ScreenHeight = 65535;

        public RdsOverlayControl()
        {
            InitializeComponent();

            InputMethod.SetIsInputMethodEnabled(this, false);

            this.xRdsButton.Click += xRdsButton_Click;
            this.xRdsButton.IsVisibleChanged += xRdsButton_IsVisibleChanged;
            this.PreviewMouseDown += RdsOverlayControl_PreviewMouseDown;

            this.MouseMove += RdsOverlayControl_MouseMove;
            this.MouseLeftButtonDown += RdsOverlayControl_MouseLeftButtonDown;
            this.MouseLeftButtonUp += RdsOverlayControl_MouseLeftButtonUp;
            this.MouseRightButtonDown += RdsOverlayControl_MouseRightButtonDown;
            this.MouseRightButtonUp += RdsOverlayControl_MouseRightButtonUp;
            this.MouseWheel += RdsOverlayControl_MouseWheel;
            this.KeyDown += RdsOverlayControl_KeyDown;
            this.KeyUp += RdsOverlayControl_KeyUp;
            this.PreviewKeyDown += RdsOverlayControl_PreviewKeyDown;

            this.IsMouseCapturedChanged += RdsOverlayControl_IsMouseCapturedChanged;
        }

        #region Propertis

        public string SyncId { get; set; }

        public bool EnableRdsControl
        {
            get
            {
                return this.enableRdsControl;
            }
            set
            {
                if (value)
                {
                    this.xRdsButton.Visibility = Visibility.Collapsed;
                    this.CaptureMouse();
                    this.ConnectRds();
                    this.enableRdsControl = true;
                }
                else
                {
                    this.xRdsButton.Visibility = Visibility.Visible;
                    this.DisconnecetRds();
                    this.enableRdsControl = false;
                    this.ReleaseMouseCapture();
                }
            }
        }

        #endregion // Propertis


        #region Events

        public event EventHandler eStartRdsControl;

        private void OnEStartRdsControl(EventArgs e)
        {
            var handler = this.eStartRdsControl;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler eEndRdsControl;

        private void OnEEndRdsControl(EventArgs e)
        {
            var handler = this.eEndRdsControl;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// 컨트롤 모드에서 마우스, 키보드 제어 시 발생
        /// </summary>
        public event EventHandler<RdsControlEventArgs> eRdsControled;

        private void OnERdsControled(RdsControlEventArgs e)
        {
            var handler = this.eRdsControled;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion // Events


        #region Event Handler

        private void RdsOverlayControl_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue && this.enableRdsControl)
            {
                this.CaptureMouse();
            }
        }

        private void RdsOverlayControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var pos = this.CalcRegion(e.GetPosition(this));

            var signal = new byte[12];

            signal[0] = MOUSEMOVE;
            signal[4] = (byte)pos.X;
            signal[5] = (byte)(pos.X / 256.0);
            signal[8] = (byte)pos.Y;
            signal[9] = (byte)(pos.Y / 256.0);

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            if (!IsKeyboardFocused)
            {
                Focusable = true;
                Focus();

                Keyboard.Focus(this);
                FocusManager.SetIsFocusScope(this, true);
            }

            var signal = new byte[12];

            signal[0] = MOUSELBUTTONDOWN;

            this.OnERdsControled(new RdsControlEventArgs(signal));
        }

        private void RdsOverlayControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var signal = new byte[12];

            signal[0] = MOUSELBUTTONUP;

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            if (!IsKeyboardFocused)
            {
                Focusable = true;
                Focus();

                Keyboard.Focus(this);
                FocusManager.SetIsFocusScope(this, true);
            }

            var signal = new byte[12];

            signal[0] = MOUSERBUTTONDOWN;

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var signal = new byte[12];

            signal[0] = MOUSERBUTTONUP;

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var signal = new byte[12];

            signal[0] = MOUSEWHEEL;
            signal[2] = (byte)e.Delta;
            signal[3] = (byte)(e.Delta >> 8);
            signal[4] = (byte)(e.Delta >> 16);
            signal[5] = (byte)(e.Delta >> 24);

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var actualKey = GetRealProcessedKey(e);

            var convertedKey = (byte)KeyInterop.VirtualKeyFromKey(actualKey);

            var signal = new byte[12];

            signal[0] = KEYDOWN;
            signal[4] = convertedKey;

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        private void RdsOverlayControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (!this.enableRdsControl)
            {
                return;
            }

            var actualKey = GetRealProcessedKey(e);

            var convertedKey = (byte)KeyInterop.VirtualKeyFromKey(actualKey);

            var signal = new byte[12];

            signal[0] = KEYUP;
            signal[4] = convertedKey;

            this.OnERdsControled(new RdsControlEventArgs(signal));

            e.Handled = true;
        }

        //////////
        //특수키 처리를 위해 필요한 함수들 !!
        [DllImport("Imm32.dll", SetLastError = true)]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        public static extern bool ImmSetConversionStatus(IntPtr hIMC, UInt32 fdwConversion, UInt32 fdwSentence);

        [DllImport("imm32.dll")]
        public static extern uint ImmGetVirtualKey(IntPtr hWnd);

        public IntPtr Handle
        {
            get { return (new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow)).Handle; }
        }

        private void RdsOverlayControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            if (e.Key == Key.ImeProcessed)
            {
                if (!IsKeyboardFocused)
                {
                    return;
                }

                var signal = new byte[12];

                var convertedKey = (byte)KeyInterop.VirtualKeyFromKey(e.ImeProcessedKey);

                signal[0] = KEYDOWN;
                signal[4] = convertedKey;

                this.OnERdsControled(new RdsControlEventArgs(signal));

                //원격 컴퓨터에 메세지를 보내고 자신의 컴퓨터의 한,영 전환이 되는 것을 막음 !!
                var HIme = ImmGetContext(this.Handle);
                ImmSetConversionStatus(HIme, 0, 0);
                e.Handled = true;
            }
        }

        private void RdsOverlayControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMouseCaptureWithin)
            {
                return;
            }

            var pos = Mouse.GetPosition(this);

            if (pos.X < 0 || pos.Y < 0 || pos.X > this.ActualWidth || pos.Y > this.ActualHeight)
            {
                this.EnableRdsControl = false;
            }
        }

        private void xRdsButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.xWatermarkTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.xWatermarkTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void xRdsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.EnableRdsControl = true;
        }

        #endregion // Event Handler


        #region Methods

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        private static T FindVisualParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }

        private Point CalcRegion(Point pos)
        {
            var returnPoint = new Point
            {
                X = (pos.X / this.ActualWidth) * ScreenWidth,
                Y = (pos.Y / this.ActualHeight) * ScreenHeight
            };

            return returnPoint;
        }

        private static Key GetRealProcessedKey(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.ImeProcessed:
                    return e.ImeProcessedKey;
                case Key.System:
                    return e.SystemKey;
                default:
                    return e.Key;
            }
        }

        #endregion // Methods


        #region Rds Control

        private void ConnectRds()
        {
            if (!IsKeyboardFocused)
            {
                Focusable = true;
                Focus();

                Keyboard.Focus(this);
                FocusManager.SetIsFocusScope(this, true);
            }

            this.OnEStartRdsControl(new EventArgs());
        }

        private void DisconnecetRds()
        {
            var mainWindow = Application.Current.MainWindow as IInputElement;

            if (mainWindow != null)
            {
                mainWindow.Focusable = true;
                mainWindow.Focus();

                Focusable = false;

                Keyboard.Focus(mainWindow);

                if (mainWindow is DependencyObject)
                {
                    FocusManager.SetIsFocusScope(mainWindow as DependencyObject, true);
                }
            }

            this.OnEEndRdsControl(new EventArgs());
        }

        #endregion // Rds Control
    }

    public class RdsControlEventArgs : EventArgs
    {
        public RdsControlEventArgs(byte[] aData)
        {
            this.Data = aData;
        }

        public byte[] Data { get; set; }
    }
}
