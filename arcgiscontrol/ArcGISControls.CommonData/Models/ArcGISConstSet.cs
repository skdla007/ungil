using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    using System;

    public class ArcGISConstSet
    {
        public static readonly string SearchUrlGoogle = "http://maps.google.com/maps/api/geocode/xml?{0}={1}&sensor={2}&language={3}";
        public static readonly string TestArcGisMapUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        public static readonly string SearchTextUrlNaver = "http://maps.naver.com/api/geocode.php?key={0}&encoding=utf-8&coord=latlng&query={1}";
        public readonly static string SearchTextUrlDaum = "http://apis.daum.net/local/geo/addr2coord?apikey={0}&q={1}&output=xml";
        public readonly static string SearchLatLngUrlDuam = "http://apis.daum.net/local/geo/coord2addr?apikey={0}&longitude={1}&latitude={2}&format=simple&output=xml&inputCoordSystem=WGS84";


        public static string Test_ArcGisMapUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        public static string Test_NaverMapKey = "9eb74d59f55e25d243f5658b0594f0e4";
        public static string Test_BingMapKey = "AtTKHpsuWBem1qlizV1EdBj6AywW5-kU64vwOmTz5ksLZhiXSq20WBnTGhuAKpQF";
        public static string Test_BingMapUrl = "http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial";
        public static string Test_DaumMapKey = "9a5911554d5ad8eb9a7cab3b0bd8469a6e64a500";

        #region Application 공통 설정

        public enum QualitySettings
        {
            Low,
            High
        }

        public static readonly QualitySettings QualityMode = QualitySettings.High;

        public static readonly bool StopSplunkControlOutOfScreen = true;

        public static readonly bool AlwaysKeepToCameraVideo = true;

        public static readonly bool AlwaysKeepToSplunkData = true;

        #endregion

        #region Camer Graphic Object
       
        public static readonly string AddressIconSymbolTemplateUri =
            @"pack://application:,,,/ArcGISControl;component/Resources/ResourceDictionaries/MapGraphicSymbol.xaml";

        public static readonly string SplunkIconSymbolTemplateUri =
            @"pack://application:,,,/ArcGISControl;component/Resources/ResourceDictionaries/SplunkIconGraphicSymbol.xaml";

        public static readonly string UniversalIconSymbolTemplateUri =
            @"pack://application:,,,/ArcGISControl;component/Resources/ResourceDictionaries/UniversalIconGraphicSymbol.xaml";

        public static readonly string InteractiveResourceDictionaryUri =
            @"pack://application:,,,/ArcGISControl;component/Resources/ResourceDictionaries/InteractiveArcGISMap.xaml";
        
        public static readonly string CameraIconNormalUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/CAMERA_A_cam_n.png";
        public static readonly string CameraIconSelectedUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/CAMERA_A_cam_o.png";
        public static readonly string CameraPtzIconNormalUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/CAMERA_A_ptz_n.png";
        public static readonly string CameraPtzIconSelectedUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/CAMERA_A_ptz_o.png";
        public static readonly string CameraIconHideUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/CAMERA_A_cam_h.png";
        public static readonly decimal CameraIconSize = 0.7m;
        public static readonly bool CameraIconDefaultVisibility = true;

        public static readonly Color CameraRectNormalColor = Colors.Transparent;
        public static readonly Color CameraRectSelectedColor = Colors.Transparent;

        public static readonly Color CameraPresetNormalColor = Color.FromArgb(0x99, 0x00, 0x72, 0xff);
        public static readonly Color CameraPresetSelectedColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);

        public static readonly string CameraPresetPlusNormalUri =
            "pack://application:,,,/ArcGISControl;component/Resources/Images/ViewZone_A_plus_n.png";

        public static readonly string CameraPresetPlusSelectedUri =
            "pack://application:,,,/ArcGISControl;component/Resources/Images/ViewZone_A_plus_o.png";

        public static readonly int PresetOneSide = 30;
        public static readonly int IconSize = 30;

        public static readonly string EditingMarkerSelectedUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/EditMarker_ViewZone_n.jpg";
        public static readonly string EditingMarkerNormalUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/EditMarker_ViewZone_s.jpg";

        #endregion Cmaera Graphic Object

        #region Style Resources uri

        public static readonly string GeometryEditorTemplateUri =
            @"pack://application:,,,/ArcGISControl;component/Resources/ResourceDictionaries/GeometryEditor.xaml";

        #endregion //Styel Resources uri

        #region Location Graphic Object

        public static readonly string LocationIconNormalUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/bookmark_a_location_n.png";
        public static readonly string LocationIconSelectedUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/bookmark_a_location_o.png";

        public static readonly string LocationIconNormalUri2 = "pack://application:,,,/ArcGISControl;component/Resources/Images/btn_cam_n.png";
        public static readonly string LocationIconSelectedUri2 = "pack://application:,,,/ArcGISControl;component/Resources/Images//btn_cam_o.png";

        public static readonly string ImageLinkZoneDefaultUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/linkzone_default.png";
        public static readonly string ImageDefaultUri = "pack://application:,,,/ArcGISControl;component/Resources/Images/Image_Default.png";

        public static readonly double LocationIconSize = 0.7;

        #endregion //Location Graphic Object

        #region LinkZone Graphic Object

        public static readonly Color LinkZoneNormalColor = Color.FromArgb(0x99, 0xfe, 0xde, 0x00);
        public static readonly Color LinkZoneSelectedColor = Color.FromArgb(0xff, 0xff, 0x84, 0x00);

        public static readonly Color ImageLinkZoneNormalColor = Color.FromArgb(0x00, 0xff, 0xff, 0xff);

        #endregion LinkZone Graphic Object
        
        #region Graphic Editing Marker Graphic

        public static readonly string GraphicEditingMarkerGraphicId = "0ad3b6b6-d7c6-4c1c-af1a-ef4d5592366b";

        #endregion Graphic Editing Marker Graphic

        #region MemoTip Graphic Object

        public static readonly Color MapTipFillColor = Color.FromArgb(0xFF, 0xC6, 0xA5, 0x65);
        public static readonly Color MapTipBorderColor = Color.FromArgb(0xFF, 0xC6, 0xA5, 0x65);
        public static readonly double MapTipBorderThickness = 2;

        #endregion MemoTip Graphic Object

        #region Text Graphic Object

        public static readonly bool TextObjectBold = false;
        public static readonly bool TextObjectItalic = false;
        public static readonly bool TextObjectUnderline = false;
        public static readonly int TextObjectfontSize = (int)SystemFonts.IconFontSize;
        public static readonly Color TextObjectFontColor = Colors.Black;
        public static readonly Color TextObjectBackgroundColor = Colors.Transparent;
        public static readonly TextAlignment TextObjectAlignment = TextAlignment.Center;
        public static readonly VerticalAlignment TextObjectVerticalAlignment = VerticalAlignment.Center;

        public static readonly FontFamily TextFontFamily = SystemFonts.CaptionFontFamily;

        #endregion Text Graphic Object

        #region Line Graphic Object

        public static readonly Color LineColor = Colors.Black;
        public static readonly int LineStrokeThickness = 3;
        public static readonly LineStrokeType LineStrokeType = LineStrokeType.FullLine;
        public static readonly PenLineJoin LineJoin = PenLineJoin.Miter;

        #endregion Line Graphic Object

        #region CameraObjectSettingInformation


        #endregion CameraObjectSettingInformation

        #region CustomMapCreatorSetting

        public static readonly int TileWidth = 256;
        public static readonly int TileHegiht = 256;
        public static readonly int TileImageQuality = 95;
        public static readonly int MakerMaxLevel = -1;

        #endregion //CustomMapCreatorSetting

        #region Animation

        public static readonly TimeSpan ZoomDuration = TimeSpan.Parse("00:00:00.75");
        //public static readonly TimeSpan ZoomDuration = TimeSpan.Parse("00:00:02.00");
        public static readonly TimeSpan PanDuration = TimeSpan.Parse("00:00:00.00");
        public static readonly TimeSpan SmoothTransitionAnimationTimeSpan = TimeSpan.FromSeconds(0.4);
        public static readonly TimeSpan ZoomDurationDuringLinkZoneDoubleClick = TimeSpan.FromSeconds(1.1);

        #endregion Animation

        #region Splunk Graphics
        #region Splunk LineTo

        public static readonly Brush LineToColor = Brushes.Red;
        public static readonly double LineToWidth = 4;

        #endregion Splunk LineTo

        #region Splunk Table Max Size

        public static double SplunkTableMaxWidth = double.MaxValue;
        public static double SplunkTableMaxHeight = double.MaxValue;

        #endregion //Splunk Table Max Size
        #endregion Splunk Graphics

        #region Splunk 임시 Data

        public static readonly int TimeLinePlaySpeed = 6;
        public static readonly int PlaybackSchedulingInterval = 10;

        #endregion

        #region Z Index

        /// <summary>
        /// 앞으로 할당받아야 할 Z Index를 표현하는데 이 값을 쓰기로 약속
        /// </summary>
        public const int UndefinedZIndex = -1;

        #endregion Z Index

        #region Object 공통사항

        public static readonly Size ObjectBasicSize = new Size(150, 200);

        #endregion 

        static ArcGISConstSet()
        {
            ProxyServerPort = int.Parse(ConfigurationManager.AppSettings["ProxyServerPort"]);

            var iconSizeMode = ConfigurationManager.AppSettings.Get("DefaultMapIconSize");
            if (iconSizeMode != null &&
                iconSizeMode.Trim().StartsWith("small", StringComparison.CurrentCultureIgnoreCase))
            {
                ArcGISConstSet.LocationIconSize = 0.4;
                ArcGISConstSet.CameraIconDefaultVisibility = false;
            }

            var performanceMode = ConfigurationManager.AppSettings.Get("Quality");
            if (performanceMode != null &&
                performanceMode.Trim().StartsWith("low", StringComparison.CurrentCultureIgnoreCase))
            {
                ArcGISConstSet.QualityMode = QualitySettings.Low;
            }

            ArcGISConstSet.StopSplunkControlOutOfScreen = bool.Parse(ConfigurationManager.AppSettings["StopSplunkControlOutOfScreen"]);
            ArcGISConstSet.AlwaysKeepToCameraVideo = bool.Parse(ConfigurationManager.AppSettings["AlwaysKeepToCameraVideo"]);
            ArcGISConstSet.AlwaysKeepToSplunkData = bool.Parse(ConfigurationManager.AppSettings["AlwaysKeepToSplunkData"]);
        }

        #region Proxy Server

        public static readonly int ProxyServerPort;

        #endregion
    }
}
