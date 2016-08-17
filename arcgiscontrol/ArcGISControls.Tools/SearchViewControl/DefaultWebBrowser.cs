using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISControls.Tools.SearchViewControl
{
    public class DefaultWebBrowser : IWebBrowser
    {
        WebBrowser control = new WebBrowser();

        public UIElement GetControl()
        {
            return this.control;
        }

        public void Navigate(string url)
        {
            this.control.Navigate(url);
        }

        public void Dispose()
        {
            this.control.Dispose();
        }
    }
}
