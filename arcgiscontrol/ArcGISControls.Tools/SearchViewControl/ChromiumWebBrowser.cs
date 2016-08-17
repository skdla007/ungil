using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Xilium.CefGlue.WPF;

namespace ArcGISControls.Tools.SearchViewControl
{
    public class ChromiumWebBrowser : IWebBrowser
    {
        WpfCefBrowser control = new WpfCefBrowser();

        public UIElement GetControl()
        {
            return this.control;
        }

        public void Navigate(string url)
        {
            this.control.NavigateTo(url);
        }

        public void Dispose()
        {
            this.control.Dispose();
        }
    }
}
