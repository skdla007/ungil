using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.Bases
{
    public partial class BaseArcGISMap
    {
        #region Methods

        protected virtual void MakeUniversalGraphic(MapUniversalObjectDataInfo dataInfo)
        {
            this.universalObjectDataManager.AddObject(dataInfo);

            var controlGraphic = this.universalObjectDataManager.GetControlGraphic(dataInfo.ObjectID);
            var iconGraphic = this.universalObjectDataManager.GetIconGraphic(dataInfo.ObjectID);

            this.SetBaseGraphic(controlGraphic, dataInfo.ObjectZIndex, ZLevel.L0);
            this.SetBaseGraphic(iconGraphic, dataInfo.ObjectZIndex, ZLevel.L1);

            controlGraphic.Control.IsHitTestVisible = this.IsConsoleMode;
        }

        #endregion // Methods
    }
}
