using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.Tools.SearchViewControl
{
    public class SearchViewWindowIsVisibleChangedEventArgs : EventArgs
    {
        public bool IsVisible { get; private set; }

        public SearchViewWindowIsVisibleChangedEventArgs(bool isVisible)
        {
            this.IsVisible = isVisible;
        }
    }
}
