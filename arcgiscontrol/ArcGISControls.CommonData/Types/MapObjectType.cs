using ArcGISControls.CommonData.Models;
namespace ArcGISControls.CommonData.Types
{
    public enum MapObjectType
    {
        None = 0,
        Address = 1,
        [MapObjectTypeInfo("BookMark", typeof(MapLocationObjectDataInfo), "MakeLocationGraphic")]
        Location = 2,
        BookMark = 3,
        Camera = 4, /*Camear�� Componet ��ü*/
        [MapObjectTypeInfo("LinkZone", typeof(MapLinkZoneObjectDataInfo), "MakeLinkZoneGraphic")]
        LinkZone = 5,
        [MapObjectTypeInfo("ImageLinkZone", typeof(MapLinkZoneObjectDataInfo), "MakeImageLinkZoneGraphic")]
        ImageLinkZone = 6, /*ImageLinkZone�� ���� �̹���*/
        VertexSeletedMarker = 8,/* Link Zone Select �� */
        Panning = 10,

        [MapObjectTypeInfo("Text", typeof(MapTextObjectDataInfo), "MakeTextGraphic")]
        Text = 40,
        TextEditorMarker = 41,/* Link Zone Editor �� */
        SearchedAddress = 11,

        [MapObjectTypeInfo("Line", typeof(MapLineObjectDataInfo), "MakeLineGraphic")]
        Line = 50,
        [MapObjectTypeInfo("Line", typeof(MapLineObjectDataInfo), "MakeLineGraphic")]
        DrawLine = 55,

        [MapObjectTypeInfo("Image", typeof(MapImageObjectDataInfo), "MakeImageGraphic")]
        Image = 60,
        
        CameraIcon = 100,/*Camear�� Componet �� Icon*/
        CameraVideo = 101,/*Camear�� Componet �� video*/
        CameraPreset = 102,/*Camear�� Componet �� Preset */
        CameraPresetPlus = 103,/*Camear�� Componet �� Plus*/
        CameraNameTextBox = 104,/*Camear�� Componet �� Label Text*/
        CameraPresetEditorMarker = 105,/*Camear Preset�� Editing Marker*/
        CameraOverlayControl = 106,/*Camear Preset�� Editing Marker*/
        
        Splunk = 20,
        SplunkIcon = 21,
        SplunkControl = 22,
        SplunkChartControl = 23,
        SplunkTableControl =24,

        [MapObjectTypeInfo("WorkStation", typeof(MapWorkStationObjectDataInfo), "MakeWorkStationGraphic")]
        Workstation = 30,

        MemoTextBox = 200,
        MemoTip = 201,

        [MapObjectTypeInfo("Universal", typeof(MapUniversalObjectDataInfo), "MakeUniversalGraphic")]
        Universal = 301,
        UniversalIcon = 302,
        UniversalControl = 303,

        GraphicEditingMarker = 1000,
    }
}
