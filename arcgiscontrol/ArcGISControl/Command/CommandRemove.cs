using ESRI.ArcGIS.Client;
using System;
using System.Collections.Generic;
using ArcGISControl.GraphicObject;

namespace ArcGISControl.Command
{
    ///// <summary>
    ///// One Graphic Remove Command
    ///// </summary>
    //class CommandRemove : CommandBase
    //{
    //    private BaseGraphic _RemovedObjectClone;

    //    #region Constructor
    //    public CommandRemove(BaseGraphic RemovedObjectClone)
    //        : base()
    //    {
    //        // Keep copy of removed object
    //        _RemovedObjectClone = RemovedObjectClone;
    //    }
    //    #endregion

    //    public override BaseGraphic newObjectClone
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public override void AddNewObjectClone(BaseGraphic newObjectClone)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Undo(GraphicsLayer ObjectGraphicLayer)
    //    {
    //        /*
    //        if (_RemovedObjectClone is MapObjectGraphic<MapEdit.MapObjects.IMapControl>)
    //        {
    //            MapObjectGraphic<MapEdit.MapObjects.IMapControl> MapControlGraphic = _RemovedObjectClone as MapObjectGraphic<MapEdit.MapObjects.IMapControl>;
    //            ((System.Windows.Controls.Control)MapControlGraphic.Control).Tag = null;
    //        }

    //        if (ObjectGraphicLayer.Graphics.Contains(_RemovedObjectClone) == false)
    //            ObjectGraphicLayer.Graphics.Add(_RemovedObjectClone);
    //        if (GraphicList.Contains(_RemovedObjectClone) == false)
    //            GraphicList.Add(_RemovedObjectClone);
    //         * */
    //    }

    //    public override void Redo(GraphicsLayer ObjectGraphicLayer)
    //    {
    //        /*
    //        if (_RemovedObjectClone is MapObjectGraphic<MapEdit.MapObjects.IMapControl>)
    //        {
    //            MapObjectGraphic<MapEdit.MapObjects.IMapControl> MapControlGraphic = _RemovedObjectClone as MapObjectGraphic<MapEdit.MapObjects.IMapControl>;
    //            ((System.Windows.Controls.Control)MapControlGraphic.Control).Tag = "Delete";
    //        }

    //        if (ObjectGraphicLayer.Graphics.Contains(_RemovedObjectClone))
    //            ObjectGraphicLayer.Graphics.Remove(_RemovedObjectClone);
    //        if (GraphicList.Contains(_RemovedObjectClone))
    //            GraphicList.Remove(_RemovedObjectClone);
    //         * */
    //    }
    //}
}
