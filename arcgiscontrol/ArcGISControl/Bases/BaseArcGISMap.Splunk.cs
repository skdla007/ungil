using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using ArcGISControl.GraphicObject;
using ArcGISControl.UIControl;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using Innotive.SplunkControl.Table;
using Innotive.SplunkControl.Table.Event;

namespace ArcGISControl.Bases
{

    public partial class BaseArcGISMap
    {
        #region Methods

        /// <summary>
        /// Base Object Component 하나 추가
        /// </summary>
        /// <param name="splunkObjectData"></param>
        protected virtual void MakeSplunkGraphic(MapSplunkObjectDataInfo splunkObjectData)
        {
            if(splunkObjectData == null) return;

            var addedObjectList = this.savedSplunkObjectDataManager.MakeOneObjectGraphics(splunkObjectData);

            if (addedObjectList == null) return;

            var baseGraphics = addedObjectList as BaseGraphic[] ?? addedObjectList.ToArray();

            var iconGraphic = baseGraphics.ElementAt(0);
            var controlGraphic = baseGraphics.ElementAt(1);

            if (iconGraphic == null || controlGraphic == null) return;

            this.SetBaseGraphic(iconGraphic, splunkObjectData.IconZIndex, ZLevel.L0);

            this.SetBaseGraphic(controlGraphic, splunkObjectData.ObjectZIndex, ZLevel.L0);

            var splunkControl = this.savedSplunkObjectDataManager.GetSplunkControl(controlGraphic.ObjectID);
            
            if (splunkControl == null) return;

            splunkControl.IsHitTestVisible = this.IsConsoleMode;
        }

        #endregion //Methods

    }
}
