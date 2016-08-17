using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ArcGISControls.Tools.SearchViewControl;

namespace ArcGISControlDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        WebBrowserType webBrowserType = WebBrowserType.Chromium;

        protected override void OnStartup(StartupEventArgs e)
        {
            SearchBrowserOverlayWindow.Initialize(this.webBrowserType, e.Args);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SearchBrowserOverlayWindow.Deinitialize();

            base.OnExit(e);
        }
    }
}
