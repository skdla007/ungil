using System;
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;

namespace ArcGISControl.Command
{
    public abstract class CommandBase
    {
        internal delegate void UnSelectGraphicObject(BaseGraphic baseGraphic, bool needToRaiseEvent = true);

        internal ArcGISControl.Helper.GraphicGeometryEditor Editor
        {
            get;
            set;
        }
        internal UnSelectGraphicObject UnSelectGraphicObjectMethod
        {
            get;
            set;
        }

        public abstract void Undo(GraphicsLayer ObjectGraphicLayer);
        public abstract void Redo(GraphicsLayer ObjectGraphicLayer);
    }
}
