using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.PropertyControl
{
    public enum MapObjectPropertied
    {
        None = 0,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.CameraIcon)]
        CameraIcon = 1,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.CameraVideo)]
        CameraVideo = 2,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.CameraPreset)]
        CameraViewZone = 3,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.LinkZone, MapObjectType.ImageLinkZone)]
        LinkZone = 4,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.Location)]
        Place = 5,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.SplunkControl, MapObjectType.SplunkIcon)]
        Splunk = 6,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.Workstation)]
        WorkStation = 7,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.Text)]
        Text = 8,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.Line)]
        Line = 9,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.Image)]
        Image = 10,
        [MapControlPropertiedMappingAttribute(true, MapObjectType.UniversalControl, MapObjectType.UniversalIcon)]
        UniversalObject = 11
    }
}
