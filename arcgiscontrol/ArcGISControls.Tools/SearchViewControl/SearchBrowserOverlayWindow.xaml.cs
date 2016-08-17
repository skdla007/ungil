
using System.Windows.Controls.Primitives;

namespace ArcGISControls.Tools.SearchViewControl
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ArcGISControl.Helper;
    using ArcGISControls.CommonData.Windows;
    using Xilium.CefGlue;

    public enum WebBrowserType
    {
        IE,
        Chromium
    }

    /// <summary>
    /// WebBrowserControl이 Window의 AllowsTransparency 값에 영향을 받지 않기 위해 감싼 Window.
    /// </summary>
    public partial class SearchBrowserOverlayWindow : Window, INotifyPropertyChanged
    {
        #region Static

        private static bool isInitialized;

        private static WebBrowserType webBrowserType;

        #region Assembly Resolve

        private static readonly List<string> relatedAssemblyNameList = new List<string>()
        {
            "Xilium.CefGlue",
            "Xilium.CefGlue.WPF",
            "NLog",
        };

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name.Split(',')[0];
            if (relatedAssemblyNameList.Exists(name => String.Compare(name, assemblyName, true) == 0))
            {
                string platform;
                if (IntPtr.Size == 4) platform = "x86";
                else if (IntPtr.Size == 8) platform = "x64";
                else throw new PlatformNotSupportedException();

                var assemblyFullPath = Path.GetFullPath(String.Format("{0}\\{1}.dll", platform, assemblyName));
                return Assembly.LoadFile(assemblyFullPath);
            }
            return null;
        }

        #endregion Assembly Resolve

        public static void Initialize(WebBrowserType aWebBrowserType, string[] args)
        {
            webBrowserType = aWebBrowserType;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            switch (webBrowserType)
            {
                case WebBrowserType.Chromium:
                    LoadCef(args);
                    break;
                default:
                    break;
            }

            isInitialized = true;
        }

        public static void Deinitialize()
        {
            if (!isInitialized)
                return;

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

            switch (webBrowserType)
            {
                case WebBrowserType.Chromium:
                    ShutdownCef();
                    break;
                default:
                    break;
            }

            isInitialized = false;
        }

        #endregion Static

        #region CEF

        private sealed class SampleCefApp : CefApp
        {
            public SampleCefApp()
            {
            }
        }

        private static int LoadCef(string[] args)
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
            catch (CefRuntimeException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 3;
            }

            var mainArgs = new CefMainArgs(args);
            var cefApp = new SampleCefApp();

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, cefApp);
            if (exitCode != -1) { return exitCode; }

            CefSettings cefSettings;
            var exeName  = AppDomain.CurrentDomain.FriendlyName;
            if (exeName.EndsWith(".vshost.exe"))
            {
                cefSettings = new CefSettings
                {
                    // BrowserSubprocessPath = browserSubprocessPath,
                    SingleProcess = true,
                    WindowlessRenderingEnabled = true,
                    MultiThreadedMessageLoop = true,
                    LogSeverity = CefLogSeverity.Verbose,
                    LogFile = "cef.log",
                };
            }
            else
            {
                cefSettings = new CefSettings
                {
                    SingleProcess = false,
                    WindowlessRenderingEnabled = true,
                    MultiThreadedMessageLoop = true,
                    LogSeverity = CefLogSeverity.Disable,
                    LogFile = "cef.log",
                };
            }

            try
            {
                CefRuntime.Initialize(mainArgs, cefSettings, cefApp);
            }
            catch (CefRuntimeException ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 4;
            }

            return 0;
        }

        private static void ShutdownCef()
        {
            // shutdown CEF
            CefRuntime.Shutdown();
        }

        #endregion CEF

        #region Fields

        private ChromeRemover chromeRemover;

        private IWebBrowser webBrowser;

        private string browserTitle;
        
        private string mapName;

        #endregion

        #region Construct

        public SearchBrowserOverlayWindow()
        {
            InitializeComponent();
            this.InitializeWebBrowserControl();

            this.DataContext = this;
            this.StateChanged += this.MainWindow_StateChanged;
            this.xNormalMaximizeButton.Click += NormalMaximizeButton_OnClick;
            this.xTaskBarPanel.MouseLeftButtonDown += this.xTaskBarPanel_MouseLeftButtonDown;
            this.Closing += SearchBrowserOverlayWindow_Closing;
        }

        private void InitializeWebBrowserControl()
        {
            if (!isInitialized)
                throw new InvalidOperationException("Initialize first!");

            switch (webBrowserType)
            {
                case WebBrowserType.Chromium:
                    this.webBrowser = new ChromiumWebBrowser();
                    break;
                default:
                    this.webBrowser = new DefaultWebBrowser();
                    break;
            }

            this.xWebBrowserPlacementTarget.Child = this.webBrowser.GetControl();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.chromeRemover = new ChromeRemover(this)
            {
                ResizeBorderWidth = this.xWebBrowserPlacementTarget.Margin
            };
            this.chromeRemover.Apply();
        }

        #endregion

        #region Properties

        public string BrowserTitle
        {
            get { return this.browserTitle; }
            set
            {
                this.browserTitle = value;
                this.OnPropertyChanged("BrowserTitle");
            }
        }

        public string MapName
        {
            get { return this.mapName; }
            set
            {
                this.mapName = value;
                this.OnPropertyChanged("MapName");
            }
        }

        #endregion

        #region Methods

        #region Event Handlers

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            this.xNormalMaximizeButton.IsChecked = this.WindowState != WindowState.Maximized;
        }

        void xTaskBarPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualElementHelper.FindParentControl<ButtonBase>(Mouse.DirectlyOver);
            if (parent != null)
            {
                return;
            }

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    var mousePoint = e.GetPosition(this);
                    var mouseAbsolutePoint = this.PointToScreenDIU(mousePoint);
                    var prevWidth = this.ActualWidth;

                    this.WindowState = WindowState.Normal;
                    // ActualWidth, ActualHeight는 이제 Normal의 Size를 가리킴.

                    this.Top = mouseAbsolutePoint.Y - mousePoint.Y;

                    var bound = ScreenUtil.GetTotalScreenBound();

                    var moveLeft =
                        (mouseAbsolutePoint.X - mousePoint.X / prevWidth * this.ActualWidth) / ScreenUtil.GetDpiRatioForDiu().X;

                    if (moveLeft < bound.Left)
                    {
                        moveLeft = bound.Left;
                    }
                    else if (moveLeft + this.ActualWidth > bound.Right)
                    {
                        moveLeft = bound.Right - this.ActualWidth;
                    }

                    this.Left = moveLeft;
                }

                this.DragMove();
            }
        }

        void SearchBrowserOverlayWindow_Closing(object sender, CancelEventArgs e)
        {
            this.Dispose();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void NormalMaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle == null)
            {
                return;
            }

            if (!toggle.IsChecked.HasValue)
            {
                toggle.IsChecked = false;
            }

            this.WindowState = (bool)toggle.IsChecked ? WindowState.Normal: WindowState.Maximized;
        }

        private void XCloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Go("about:blank");
            this.Hide();
        }

        #endregion

        public void Go(Uri url)
        {
            this.webBrowser.Navigate(url.ToString());
        }

        public void Go(string url)
        {
            Uri parsedUri;

            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out parsedUri))
            {
                // Messaging.
                return;
            }

            try
            {
                this.webBrowser.Navigate(url);
            }
            catch (ArgumentException ex)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(ex, ex.GetType().ToString());
            }
        }

        public void Dispose()
        {
            this.xWebBrowserPlacementTarget.Child = null;
            this.webBrowser.Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        public void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The clear all property changed handlers.
        /// </summary>
        public void ClearAllPropertyChangedHandlers()
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            foreach (PropertyChangedEventHandler handler in this.PropertyChanged.GetInvocationList())
            {
                this.PropertyChanged -= handler;
            }
        }

        #endregion
    }
}
