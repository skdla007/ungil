using ESRI.ArcGIS.Client;
using System;
using System.Collections.Generic;
using ArcGISControl.GraphicObject;

namespace ArcGISControl.Command
{
    public class UndoManager
    {
        #region Member Fields
        private GraphicsLayer _ObjectGraphicLayer;
        private ArcGISControl.Helper.GraphicGeometryEditor _Editor;
        private ArcGISControl.Command.CommandBase.UnSelectGraphicObject _UnSelectGraphicObjectMethod;
        private List<CommandBase> _HistoryList;
        private int _NextUndo;
        public event EventHandler StateChanged;
        #endregion

        #region Constructor
        internal UndoManager(GraphicsLayer ObjectGraphicLayer, ArcGISControl.Helper.GraphicGeometryEditor Editor, ArcGISControl.Command.CommandBase.UnSelectGraphicObject UnSelectGraphicObjectMethod)
        {
            _ObjectGraphicLayer = ObjectGraphicLayer;
            _Editor = Editor;
            _UnSelectGraphicObjectMethod = UnSelectGraphicObjectMethod;

            this.ClearHistory();
        }
        #endregion

        #region Properties
        public bool CanUndo
        {
            get
            {
                if (_NextUndo < 0 ||
                    _NextUndo > _HistoryList.Count - 1)
                {
                    return false;
                }

                return true;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (_NextUndo == _HistoryList.Count - 1)
                {
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Method
        public void ClearHistory()
        {
            _HistoryList = new List<CommandBase>();
            _NextUndo = -1;
            RaiseStateChangedEvent();
        }

        public void AddCommandToHistory(CommandBase command)
        {
            command.Editor = _Editor;
            command.UnSelectGraphicObjectMethod = _UnSelectGraphicObjectMethod;

            this.TrimHistoryList();

            _HistoryList.Add(command);
            _NextUndo++;
            RaiseStateChangedEvent();


            /*

            // CommandAdd 명령일경우 다음 조건에 따른다.     [2015. 01. 23 엄태영]
            // Camera, Universal Graphic같은 경우 Add시 여러번 Graphic이 만들어진다. 따라서 같은 Object의 Graphic일 경우 새로 Command를 생성하지 않고, 기존 Command에 추가한다.    [2015. 01. 23 엄태영]
            if (_NextUndo != -1 && _HistoryList.Count > _NextUndo && command is CommandAdd && _HistoryList[_NextUndo].GetType().Equals(command.GetType()) &&
                _HistoryList[_NextUndo].newObjectClone.ObjectID == command.newObjectClone.ObjectID)
            {
                _HistoryList[_NextUndo].AddNewObjectClone(command.newObjectClone);
            }
            else
            {
                _HistoryList.Add(command);
                _NextUndo++;
                RaiseStateChangedEvent();
            }
             * */
        }

        public void RemoveCommandToHistory(CommandBase command)
        {
            if (_HistoryList.Contains(command))
            {
                command.Editor = null;
                command.UnSelectGraphicObjectMethod = null;

                _HistoryList.Remove(command);
                this.TrimHistoryList();
                _NextUndo--;
            }
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            CommandBase command = _HistoryList[_NextUndo];

            command.Undo(_ObjectGraphicLayer);

            _NextUndo--;

            RaiseStateChangedEvent();
        }

        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            int itemToRedo = _NextUndo + 1;
            CommandBase command = _HistoryList[itemToRedo];

            command.Redo(_ObjectGraphicLayer);

            _NextUndo++;

            RaiseStateChangedEvent();
        }

        private void TrimHistoryList()
        {
            if (_HistoryList.Count == 0)
            {
                return;
            }

            if (_NextUndo == _HistoryList.Count - 1)
            {
                return;
            }

            for (int i = _HistoryList.Count - 1; i > _NextUndo; i--)
            {
                _HistoryList.RemoveAt(i);
            }
        }

        private void RaiseStateChangedEvent()
        {
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
