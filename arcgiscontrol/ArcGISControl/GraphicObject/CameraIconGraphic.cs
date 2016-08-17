using System.Windows;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.GraphicObject
{
    public class CameraIconGraphic : IconGraphic
    {
        #region Field

        public CameraNameTextBoxGraphic CameraNameTextBoxGraphic { get; set; }

        public IconGraphic ViwzonePlusButtonIcon { get; set; }

        #endregion 

        #region Method

        public CameraIconGraphic(Point position, string iconUrl, string iconSelectedUrl, MapObjectType type, string id, double size = 1)
            : base(position, iconUrl, iconSelectedUrl, type, id, size)
        {
        }

        #endregion
    }
}
