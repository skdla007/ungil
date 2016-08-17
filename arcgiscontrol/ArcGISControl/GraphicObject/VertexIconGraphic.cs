using System.Windows;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.GraphicObject
{
    public class VertexIconGraphic : IconGraphic
    {
        /// <summary>
        /// Preset�� ���õǸ� ����
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id">Camera Object�� ID</param>
        public VertexIconGraphic(Point position):
            base(position, ArcGISConstSet.EditingMarkerNormalUri, ArcGISConstSet.EditingMarkerNormalUri, MapObjectType.VertexSeletedMarker, string.Empty)
        {
            this.ChangeOffsetPoint(new Point(5, 5));
            this.SetZIndex(int.MaxValue);
        }
    }
}
