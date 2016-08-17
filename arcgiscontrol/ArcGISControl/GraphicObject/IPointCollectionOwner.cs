using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControl.GraphicObject
{
    interface IPointCollectionOwner
    {
        List<Point> PointCollection { get; set; }

        event EventHandler<EventArgs> PointCollectionChanged;

        List<VertexIconGraphic> VertexIconGraphics { get; set; }

        void SetVertexIconGraphics(List<Point> movePointCollection);

        void SetVertexIconGraphics(Vector displacement);
    }
}
