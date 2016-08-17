using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.Command
{
    internal class ClipBoardManager
    {
        #region Member Fields
        public event EventHandler StateChanged;
        private List<BaseMapObjectInfoData> _ClipBoard_GraphicList = new List<BaseMapObjectInfoData>();
        #endregion

        #region Constructor
        public ClipBoardManager() { }
        #endregion

        #region Properties
        public bool CanCopy
        {
            get
            {
                // Copy는 항상 활성화
                return true;
            }
        }

        public bool CanPaste
        {
            get
            {
                // ClipBoard 리스트에 있으면 붙여넣기 활성화
                return (_ClipBoard_GraphicList.Count > 0);
            }
        }
        #endregion

        public void Copy(BaseMapObjectInfoData copyGraphic)
        {
            _ClipBoard_GraphicList.Clear();

            _ClipBoard_GraphicList.Add(copyGraphic);
            this.RaiseStateChangedEvent();
        }

        public void Copy(List<BaseMapObjectInfoData> baseGraphicList)
        {
            _ClipBoard_GraphicList.Clear();

            _ClipBoard_GraphicList.AddRange(baseGraphicList);
            this.RaiseStateChangedEvent();
        }

        public List<BaseMapObjectInfoData> Paste(Action<BaseMapObjectInfoData, bool> addMapObject, Map baseMap)
        {
            List<BaseMapObjectInfoData> PastedGraphicDataInfoList = new List<BaseMapObjectInfoData>();

            foreach (BaseMapObjectInfoData CopyGraphic in _ClipBoard_GraphicList)
            {
                BaseMapObjectInfoData copiedGraphic = (BaseMapObjectInfoData)CopyGraphic.Clone();
                copiedGraphic.ObjectID = Guid.NewGuid().ToString();
                addMapObject(copiedGraphic, true);

                PastedGraphicDataInfoList.Add(copiedGraphic);
            }

            return PastedGraphicDataInfoList;
        }

        private void RaiseStateChangedEvent()
        {
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
        }
    }
}
