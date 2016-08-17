namespace ArcGISControl.ArcGISInternalHack
{
    using ESRI.ArcGIS.Client;
    using System;
    using System.Reflection;

    internal static class MapTool
    {
        private static readonly MethodInfo checkSpatialReference = typeof (Map).GetMethod("checkSpatialReference",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo beginZoomToExtent = typeof(Map).GetMethod("beginZoomToExtent",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 알아서 멋대로 판단해서 Pan만 한다거나 범위를 변경한다거나 애니메이션을 스킵하는 기능을 빼버리고 진짜 주어진 그대로 동작을 하게 만드는 함수.
        /// 주의: SnapToLevels가 turnon 된 경우 구현이 안 되어 있어서 exception을 발생시킴
        /// </summary>
        /// <param name="map"></param>
        /// <param name="geom"></param>
        /// <param name="skipAnimation"></param>
        /// <returns>zoom을 시작한 경우 true, 사소한 바르지 않은 상태에 의해 실패한 경우 false</returns>
        public static bool ForceZoomTo(this Map map, ESRI.ArcGIS.Client.Geometry.Geometry geom, bool skipAnimation)
        {
            if (geom == null)
                return false;

            checkSpatialReference.Invoke(map, new object[] {geom});

            var envelope = geom.Extent;
            var extent = map.Extent;

            if (envelope == null || envelope.Width == 0.0 && envelope.Height == 0.0)
                return false;

            if (map.SnapToLevels)
            {
                throw new NotSupportedException("Snap To Levels not supported");
            }

            // FIXME:
            // 만약에 문제가 생긴다면 envelope을 실제 viewSize와 비율을 맞추지 않아서 그런 것일 수도 있다.
            // 동작에 이상이 생긴다면 그 부분을 체크해볼 것

            beginZoomToExtent.Invoke(map, new object[] { envelope, skipAnimation } );

            return true;
        }
    }
}
