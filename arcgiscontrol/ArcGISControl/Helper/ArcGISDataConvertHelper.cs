using ArcGISControls.CommonData.Types;

namespace ArcGISControl.Helper
{
    public class ArcGISDataConvertHelper
    {
        /// <summary>
        /// GIS Map인지 확인
        /// </summary>
        public static bool IsGISMapType(MapProviderType mapProviderType)
        {
            return mapProviderType != MapProviderType.CustomMap && mapProviderType != MapProviderType.None;
        }

        /// <summary>
        /// 한국 전용 맵 확인
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsKoreaMapType(MapProviderType type)
        {
            switch (type)
            {
                case MapProviderType.NaverMap:
                case MapProviderType.NaverSatelliteHybridMap:
                case MapProviderType.NaverSatelliteMap:
                case MapProviderType.NaverSatelliteTrafficMap:
                case MapProviderType.NaverTrafficMap:
                case MapProviderType.DaumMap:
                case MapProviderType.DaumSatelliteHybridMap:
                case MapProviderType.DaumSatelliteMap:
                case MapProviderType.DaumSatelliteTrafficMap:
                case MapProviderType.DaumTrafficMap:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Camera Type 중 하나의 Graphic을 가지고 Camera Graphic List 에서 받아오기 위해 
        /// 현재 Camera List 를 확인 한다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCameraGraphic(MapObjectType type)
        {
            return (type == MapObjectType.CameraPresetPlus ||
                    type == MapObjectType.CameraPreset ||
                    type == MapObjectType.Camera ||
                    type == MapObjectType.CameraIcon ||
                    type == MapObjectType.CameraVideo);
        }

        public static bool IsMemoGraphic(MapObjectType type)
        {
            return type == MapObjectType.MemoTextBox
                || type == MapObjectType.MemoTip;
        }

        public static bool IsUniversalGraphic(MapObjectType type)
        {
            return type == MapObjectType.UniversalIcon
                || type == MapObjectType.UniversalControl;
        }

        public static MapProviderType GetRepresentType(MapProviderType type)
        {
            switch (type)
            {   
                case MapProviderType.ArcGisClientMap:
                case MapProviderType.ArcGisImageryMap:
                case MapProviderType.ArcGisStreetMap:
                case MapProviderType.ArcGisTogoMap:
                case MapProviderType.BingMap:
                case MapProviderType.BingArialMap:
                case MapProviderType.BingArialWithLabelMap:
                    return MapProviderType.ArcGisClientMap;
                case MapProviderType.NaverMap:
                case MapProviderType.NaverSatelliteHybridMap:
                case MapProviderType.NaverSatelliteMap:
                case MapProviderType.NaverSatelliteTrafficMap:
                case MapProviderType.NaverTrafficMap:
                case MapProviderType.DaumMap:
                case MapProviderType.DaumSatelliteHybridMap:
                case MapProviderType.DaumSatelliteMap:
                case MapProviderType.DaumSatelliteTrafficMap:
                case MapProviderType.DaumTrafficMap:
                case MapProviderType.GoogleSatelliteMap:
                case MapProviderType.GoogleMap:
                case MapProviderType.GoogleSatelliteHybridMap:
                    return MapProviderType.NaverMap;
                case MapProviderType.CustomMap:
                    return MapProviderType.CustomMap;
            }

            return MapProviderType.None;
        }
    }
}
