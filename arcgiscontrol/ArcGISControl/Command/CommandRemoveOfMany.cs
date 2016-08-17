using System;
using System.Collections.Generic;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;

namespace ArcGISControl.Command
{
    /// <summary>
    /// Remove a number of graphics Command
    /// </summary>
    class CommandRemoveOfMany : CommandBase
    {
        private List<BaseMapObjectInfoData> _RemoveMapObjectInfoDataCloneList = new List<BaseMapObjectInfoData>();
        private Action<BaseMapObjectInfoData, bool> _UnDoCallBackAction;
        internal delegate bool ReDoCallBackAction(BaseMapObjectInfoData data, bool isAlwaysDelete = false);
        private ReDoCallBackAction _ReDoCallBackAction;
        private Action<BaseMapObjectInfoData> _ObjectAddedEvent;

        #region Constructor
        public CommandRemoveOfMany(List<BaseMapObjectInfoData> RemoveMapObjectInfoDataCloneList, Action<BaseMapObjectInfoData, bool> UnDoCallBackAction,
            ReDoCallBackAction ReDoCallBack, Action<BaseMapObjectInfoData> ObjectAddedEvent)
            : base()
        {
            // Keep copy of removed ObjectInfoData
            _RemoveMapObjectInfoDataCloneList.AddRange(RemoveMapObjectInfoDataCloneList);
            _UnDoCallBackAction = UnDoCallBackAction;
            _ReDoCallBackAction = ReDoCallBack;
            _ObjectAddedEvent = ObjectAddedEvent;
        }
        #endregion

        #region CommandBase 구현
        public override void Undo(GraphicsLayer ObjectGraphicLayer)
        {
            _RemoveMapObjectInfoDataCloneList.ForEach(Item =>
            {
                Item.IsUndoManage = true;
                _UnDoCallBackAction(Item, true);
                _ObjectAddedEvent(Item);
            });
        }

        public override void Redo(GraphicsLayer ObjectGraphicLayer)
        {
            _RemoveMapObjectInfoDataCloneList.ForEach(Item =>
            {
                _ReDoCallBackAction(Item);
            });
        }
        #endregion
    }
}
