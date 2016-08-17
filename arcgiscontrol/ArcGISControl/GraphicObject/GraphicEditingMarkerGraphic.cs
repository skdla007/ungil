using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.GraphicObject
{
    public class GraphicEditingMarkerGraphic : IconGraphic
    {
        public int Index { get; private set; }

        public GraphicEditingMarkerGraphic(Point position, int index)
            : base(
                position,
                ArcGISConstSet.EditingMarkerNormalUri,
                ArcGISConstSet.EditingMarkerSelectedUri,
                MapObjectType.GraphicEditingMarker,
                ArcGISConstSet.GraphicEditingMarkerGraphicId,
                new Point(5, 5)
            )
        {
            this.Index = index;
        }
    }
}
