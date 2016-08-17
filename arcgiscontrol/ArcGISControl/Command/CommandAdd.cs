using System;
using System.Collections.Generic;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;

namespace ArcGISControl.Command
{
    /// <summary>
    /// Graphic Add Command
    /// </summary>
    class CommandAdd : CommandBase
    {
        private List<BaseMapObjectInfoData> _newMapObjectInfoDataCloneList = new List<BaseMapObjectInfoData>();
        internal delegate bool UnDoCallBackAction(BaseMapObjectInfoData data, bool isAlwaysDelete = false);
        private Action<BaseMapObjectInfoData, bool> _ReDoCallBackAction;
        private UnDoCallBackAction _UnDoCallBackAction;
        private Action<BaseMapObjectInfoData> _ObjectAddedEvent;

        #region Constructor

        public CommandAdd(BaseMapObjectInfoData MapObjectInfoDataClone, UnDoCallBackAction UnDoCallBack,
            Action<BaseMapObjectInfoData, bool> ReDoCallBackAction, Action<BaseMapObjectInfoData> ObjectAddedEvent)
            : base()
        {
            // Keep copy of added ObjectInfoData
            _newMapObjectInfoDataCloneList.Add(MapObjectInfoDataClone);
            _UnDoCallBackAction = UnDoCallBack;
            _ReDoCallBackAction = ReDoCallBackAction;
            _ObjectAddedEvent = ObjectAddedEvent;
        }

        public CommandAdd(List<BaseMapObjectInfoData> MapObjectInfoDataCloneList, UnDoCallBackAction UnDoCallBack,
            Action<BaseMapObjectInfoData, bool> ReDoCallBackAction, Action<BaseMapObjectInfoData> ObjectAddedEvent)
            : base()
        {
            // Keep copy of added ObjectInfoData
            _newMapObjectInfoDataCloneList.AddRange(MapObjectInfoDataCloneList);
            _UnDoCallBackAction = UnDoCallBack;
            _ReDoCallBackAction = ReDoCallBackAction;
            _ObjectAddedEvent = ObjectAddedEvent;
        }

        #endregion

        #region CommandBase 구현
        public override void Undo(GraphicsLayer ObjectGraphicLayer)
        {
            _newMapObjectInfoDataCloneList.ForEach(Item =>
            {
                _UnDoCallBackAction(Item);
            });
        }

        public override void Redo(GraphicsLayer ObjectGraphicLayer)
        {
            _newMapObjectInfoDataCloneList.ForEach(Item =>
            {
                Item.IsUndoManage = true;
                _ReDoCallBackAction(Item, true);
                _ObjectAddedEvent(Item);
            });
        }
        #endregion
    }
}
