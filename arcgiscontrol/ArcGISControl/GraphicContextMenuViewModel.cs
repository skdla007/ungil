using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataChangedNotify;

namespace ArcGISControl
{
    class GraphicContextMenuViewModel : BaseModel
    {
        #region Member Fields
        private bool _GraphicSelected = false;
        private bool _LockFlag;
        #endregion

        #region Properties
        public bool GraphicSelected
        {
            get
            {
                return _GraphicSelected;
            }
            set
            {
                _GraphicSelected = value;
                this.OnPropertyChanged("GraphicSelected");
            }
        }

        public bool GraphicLocked
        {
            get
            {
                return _LockFlag;
            }
            set
            {
                _LockFlag = value;
                this.OnPropertyChanged("GraphicLocked");
            }
        }
        #endregion
    }
}
