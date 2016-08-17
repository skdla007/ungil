using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControls.Tools.SearchViewControl
{
    public interface IWebBrowser : IDisposable
    {
        UIElement GetControl();

        void Navigate(string url);
    }
}
