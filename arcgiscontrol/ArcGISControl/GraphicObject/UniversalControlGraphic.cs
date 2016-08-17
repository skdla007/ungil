using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControl.Helper;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.GraphicObject
{
    public class UniversalControlGraphic : PolygonControlGraphic<UniversalControl>
    {
        public UniversalControlGraphic(string id, List<Point> pointCollection)
            : base(new UniversalControl(), MapObjectType.UniversalControl, id, pointCollection)
        {
            //this.Control.Cursor = CursorManager.Instance.GetCursor(CursorType.HandOpen);
        }
    }
}
