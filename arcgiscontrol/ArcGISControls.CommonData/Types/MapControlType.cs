using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Types
{
    /// <summary>
    /// 현재 Map의 제어 모드
    /// </summary>
    public enum MapControlType
    {
        None = 0,

        ManagerEditMode = 10,
        ViewerEditMode = 20,

        ManagerViewMode = 11,
        ViewerViewMode = 21
    }
}
