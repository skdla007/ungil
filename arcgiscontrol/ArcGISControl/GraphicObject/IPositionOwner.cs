using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControl.GraphicObject
{
    interface IPositionOwner
    {
        Point Position { get; set; }

        event EventHandler<EventArgs> PositionChanged;
    }
}
