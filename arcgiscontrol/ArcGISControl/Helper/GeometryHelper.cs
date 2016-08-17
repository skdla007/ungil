
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ArcGISControl.GraphicObject;
using ArcGISControl.TiledMapLayer;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Projection;

namespace ArcGISControl.Helper
{
    public class GeometryHelper
    {
        /// <summary>
        /// 메카토르 좌표 표준 변경을 위한 객체
        /// </summary>
        private static WebMercator mercator = new WebMercator();

        public static LatLng ToGeographic(Point mapPoint, MapProviderType type)
        {
            LatLng pLatLng;

            switch (type)
            {
                case MapProviderType.DaumSatelliteHybridMap:
                case MapProviderType.DaumMap:
                case MapProviderType.DaumSatelliteMap:
                case MapProviderType.DaumSatelliteTrafficMap:
                case MapProviderType.DaumTrafficMap:
                    pLatLng = DaumMapTiledService.ToGeographic(mapPoint.X, mapPoint.Y);
                    break;
                case MapProviderType.NaverSatelliteHybridMap:
                case MapProviderType.NaverMap:
                case MapProviderType.NaverSatelliteMap:
                case MapProviderType.NaverSatelliteTrafficMap:
                case MapProviderType.NaverTrafficMap:
                    pLatLng = NaverMapTiledService.ToGeographic(mapPoint.X, mapPoint.Y);
                    break;
                case MapProviderType.ArcGisImageryMap:
                case MapProviderType.ArcGisStreetMap:
                case MapProviderType.ArcGisTogoMap:
                case MapProviderType.ArcGisClientMap:
                case MapProviderType.BingMap:
                case MapProviderType.BingArialMap:
                case MapProviderType.BingArialWithLabelMap:
                case MapProviderType.GoogleMap:
                case MapProviderType.GoogleSatelliteMap:
                case MapProviderType.GoogleSatelliteHybridMap:
                    var tmp = mercator.ToGeographic(new MapPoint(mapPoint.X, mapPoint.Y)) as MapPoint;
                    pLatLng = new LatLng(tmp.Y, tmp.X);
                    break;
                default:
                    pLatLng = new LatLng(mapPoint.Y, mapPoint.X);
                    break;
            }
            return pLatLng;
        }

        public static Point FromGeographic(LatLng LatLng, MapProviderType type)
        {
            Point mapPoint;

            switch (type)
            {
                case MapProviderType.DaumSatelliteHybridMap:
                case MapProviderType.DaumMap:
                case MapProviderType.DaumSatelliteMap:
                case MapProviderType.DaumSatelliteTrafficMap:
                case MapProviderType.DaumTrafficMap:
                    mapPoint = DaumMapTiledService.FromGeographic(LatLng.Lng, LatLng.Lat);
                    break;
                case MapProviderType.NaverSatelliteHybridMap:
                case MapProviderType.NaverMap:
                case MapProviderType.NaverSatelliteMap:
                case MapProviderType.NaverSatelliteTrafficMap:
                case MapProviderType.NaverTrafficMap:
                    mapPoint = NaverMapTiledService.FromGeographic(LatLng.Lng, LatLng.Lat);
                    break;
                case MapProviderType.ArcGisImageryMap:
                case MapProviderType.ArcGisStreetMap:
                case MapProviderType.ArcGisTogoMap:
                case MapProviderType.ArcGisClientMap:
                case MapProviderType.BingMap:
                case MapProviderType.BingArialMap:
                case MapProviderType.BingArialWithLabelMap:
                case MapProviderType.GoogleMap:
                case MapProviderType.GoogleSatelliteMap:
                case MapProviderType.GoogleSatelliteHybridMap:
                    var tmp = mercator.FromGeographic(new MapPoint(LatLng.Lng, LatLng.Lat)) as MapPoint;
                    mapPoint = new Point(tmp.X, tmp.Y);
                    break;
                default:
                    mapPoint = new Point(LatLng.Lng, LatLng.Lat);
                    break;
            }
            return mapPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInfoData"></param>
        /// <param name="type"></param>
        public static BaseMapObjectInfoData FromGeographicObjectPosition(BaseMapObjectInfoData objectInfoData, MapProviderType type)
        {
            if (objectInfoData == null) return null;

            if (objectInfoData is MapLinkZoneObjectDataInfo)
            {
                var linkZoneData = objectInfoData as MapLinkZoneObjectDataInfo;

                var newPointCollection = linkZoneData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();

                linkZoneData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapCameraObjectComponentDataInfo)
            {
                var video = (objectInfoData as MapCameraObjectComponentDataInfo).Video;

                var newPointCollection = video.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();

                video.PointCollection = newPointCollection;

                var icon = (objectInfoData as MapCameraObjectComponentDataInfo).CameraIcon;

                icon.Position = FromGeographic(new LatLng(icon.Position.Y, icon.Position.X), type);

                foreach (var preset in (objectInfoData as MapCameraObjectComponentDataInfo).PresetDatas)
                {
                    newPointCollection = preset.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();

                    preset.PointCollection = newPointCollection;
                }
            }
            else if (objectInfoData is MapLocationObjectDataInfo)
            {
                var locationData = objectInfoData as MapLocationObjectDataInfo;

                var latLng = new LatLng(locationData.Position.Y, locationData.Position.X);

                var point = FromGeographic(latLng, type);

                locationData.Position = point;
            }
            else if(objectInfoData is MapSplunkObjectDataInfo)
            {
                var mapSplunkData = objectInfoData as MapSplunkObjectDataInfo;

                var latLng = new LatLng(mapSplunkData.IconPosition.Y, mapSplunkData.IconPosition.X);
                mapSplunkData.IconPosition = FromGeographic(latLng, type);

                var newPointCollection = mapSplunkData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();
                mapSplunkData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapTextObjectDataInfo)
            {
                var mapTextData = objectInfoData as MapTextObjectDataInfo;
                var newPointCollection = mapTextData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();
                mapTextData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapLineObjectDataInfo)
            {
                var mapLineData = objectInfoData as MapLineObjectDataInfo;
                var newPointCollection = mapLineData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();
                mapLineData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapImageObjectDataInfo)
            {
                var mapImageData = objectInfoData as MapImageObjectDataInfo;
                var newPointCollection = mapImageData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();
                mapImageData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapBookMarkDataInfo)
            {
                var mapBookMarkData = objectInfoData as MapBookMarkDataInfo;

                var pointExtentMin = FromGeographic(new LatLng(mapBookMarkData.ExtentRegion.Top, mapBookMarkData.ExtentRegion.Left), type);
                var pointExtentMax = FromGeographic(new LatLng(mapBookMarkData.ExtentRegion.Bottom, mapBookMarkData.ExtentRegion.Right), type);
                mapBookMarkData.ExtentRegion = new Rect(pointExtentMin, pointExtentMax);
            }
            else if (objectInfoData is MapWorkStationObjectDataInfo)
            {
                var mapWorkStationData = objectInfoData as MapWorkStationObjectDataInfo;

                var newPointCollection = mapWorkStationData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();

                mapWorkStationData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapUniversalObjectDataInfo)
            {
                var mapUniversalData = objectInfoData as MapUniversalObjectDataInfo;

                var newPointCollection = mapUniversalData.PointCollection.Select(point => FromGeographic(new LatLng(point.Y, point.X), type)).ToList();
                mapUniversalData.PointCollection = newPointCollection;

                var pointExtentMin = FromGeographic(new LatLng(mapUniversalData.IconMinPosition.Y, mapUniversalData.IconMinPosition.X), type);
                var pointExtentMax = FromGeographic(new LatLng(mapUniversalData.IconMaxPosition.Y, mapUniversalData.IconMaxPosition.X), type);

                mapUniversalData.IconMaxPosition = pointExtentMin;

                mapUniversalData.IconMinPosition = pointExtentMax;
            }
            else
            {
                InnowatchDebug.Logger.Trace("새로운 오브젝트가 추가되면 좌표변환 로직 추가해 주십시오.");
                throw new NotImplementedException("새로운 오브젝트가 추가되면 좌표변환 로직 추가해 주십시오.");
            }

            return objectInfoData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInfoData"></param>
        /// <param name="type"></param>
        public static BaseMapObjectInfoData ToGeographicObjectPosition(BaseMapObjectInfoData objectInfoData, MapProviderType type)
        {
            if (objectInfoData == null) return null;

            if (objectInfoData is MapLinkZoneObjectDataInfo)
            {
                var linkZoneData = objectInfoData as MapLinkZoneObjectDataInfo;

                var newPointCollection = linkZoneData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();

                linkZoneData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapCameraObjectComponentDataInfo)
            {
                var video = (objectInfoData as MapCameraObjectComponentDataInfo).Video;

                var newPointCollection = video.PointCollection.Select(p => ToGeographic(p, type)).Select(LatLng => new Point(LatLng.Lng, LatLng.Lat)).ToList();

                video.PointCollection = newPointCollection;

                var icon = (objectInfoData as MapCameraObjectComponentDataInfo).CameraIcon;

                var latLng = ToGeographic(icon.Position, type);
                icon.Position = new Point(latLng.Lng, latLng.Lat);

                foreach (var preset in (objectInfoData as MapCameraObjectComponentDataInfo).PresetDatas)
                {
                    newPointCollection = preset.PointCollection.Select(p => ToGeographic(p, type)).Select(LatLng => new Point(LatLng.Lng, LatLng.Lat)).ToList();

                    preset.PointCollection = newPointCollection;
                }
            }
            else if (objectInfoData is MapLocationObjectDataInfo)
            {
                var locationData = objectInfoData as MapLocationObjectDataInfo;
                var latLng = ToGeographic(locationData.Position, type);
                locationData.Position = new Point(latLng.Lng, latLng.Lat);

            }
            else if (objectInfoData is MapSplunkObjectDataInfo)
            {
                var mapSplunkData = objectInfoData as MapSplunkObjectDataInfo;

                {
                    var latLng = ToGeographic(mapSplunkData.IconPosition, type);
                    mapSplunkData.IconPosition = new Point(latLng.Lng, latLng.Lat);
                }

                var newPointCollection = mapSplunkData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();
                mapSplunkData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapTextObjectDataInfo)
            {
                var mapTextData = objectInfoData as MapTextObjectDataInfo;
                var newPointCollection = mapTextData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();
                mapTextData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapLineObjectDataInfo)
            {
                var mapLineData = objectInfoData as MapLineObjectDataInfo;
                var newPointCollection = mapLineData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();
                mapLineData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapImageObjectDataInfo)
            {
                var mapImageData = objectInfoData as MapImageObjectDataInfo;
                var newPointCollection = mapImageData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();
                mapImageData.PointCollection = newPointCollection;
            }
            else if (objectInfoData is MapBookMarkDataInfo)
            {
                var mapBookMarkData = objectInfoData as MapBookMarkDataInfo;
                var latLng1 = ToGeographic(mapBookMarkData.ExtentRegion.TopLeft, type);
                var latLng2 = ToGeographic(mapBookMarkData.ExtentRegion.BottomRight, type);
                mapBookMarkData.ExtentRegion = new Rect(new Point(latLng1.Lng, latLng1.Lat), new Point(latLng2.Lng, latLng2.Lat));
            }
            else if (objectInfoData is MapWorkStationObjectDataInfo)
            {
                var mapWorkStationData = objectInfoData as MapWorkStationObjectDataInfo;

                var newPointCollection = mapWorkStationData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();

                mapWorkStationData.PointCollection = newPointCollection;
            }
            else if(objectInfoData is MapUniversalObjectDataInfo)
            {
                var mapUniversalData = objectInfoData as MapUniversalObjectDataInfo;

                var newPointCollection = mapUniversalData.PointCollection.Select(p => ToGeographic(p, type)).Select(latLng => new Point(latLng.Lng, latLng.Lat)).ToList();
                mapUniversalData.PointCollection = newPointCollection;

                var latLng1 = ToGeographic(mapUniversalData.IconMaxPosition, type);
                mapUniversalData.IconMaxPosition = new Point(latLng1.Lng, latLng1.Lat);

                latLng1 = ToGeographic(mapUniversalData.IconMinPosition, type);
                mapUniversalData.IconMinPosition = new Point(latLng1.Lng, latLng1.Lat);
            }
            else
            {
                InnowatchDebug.Logger.Trace("새로운 오브젝트가 추가되면 좌표변환 로직 추가해 주십시오.");
                throw new NotImplementedException("새로운 오브젝트가 추가되면 좌표변환 로직 추가해 주십시오.");
            }

            return objectInfoData;
        }

        public static double Area(Envelope envelope)
        {
            return envelope.Width * envelope.Height;
        }

        public static bool IsSimilar(double a, double b, double eps = 1e-8)
        {
            var res = Math.Abs(a - b);
            if (res < eps) return true;
            var scale = Math.Max(Math.Abs(a), Math.Abs(b));
            if (scale > 1) return false;
            return res / scale < eps;
        }

        public static bool IsGreaterThan(double a, double b, double eps = 1e-8)
        {
            if (IsSimilar(a, b, eps))
                return false;
            return a > b;
        }

        public static bool IsLessThan(double a, double b, double eps = 1e-8)
        {
            if (IsSimilar(a, b, eps))
                return false;
            return a < b;
        }

        public static bool IsGreaterOrEqualTo(double a, double b, double eps = 1e-8)
        {
            if (IsSimilar(a, b, eps))
                return true;
            return a > b;
        }

        public static bool IsLessOrEqualTo(double a, double b, double eps = 1e-8)
        {
            if (IsSimilar(a, b, eps))
                return true;
            return a < b;
        }

        /// <summary>
        /// Point와 Size를 가지고 PointCollection을 받아온다.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static List<Point> GetRectanglePoints(Point point, Size size, double resolution)
        {
            var hieght = (size.Height * resolution);
            var width = (size.Width * resolution);

            var newMapMinY = point.Y - hieght;
            var newMapMaxX = point.X + width;

            var pointList = new List<Point>()
                {
                    new Point(point.X, point.Y),
                    new Point(point.X, newMapMinY),
                    new Point(newMapMaxX, newMapMinY),
                    new Point(newMapMaxX, point.Y),
                    new Point(point.X, point.Y),
                };

            return pointList;
        }

        /// <summary>
        /// Point 와 Size를 가지고 PointCollection 을 받아온다.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static List<Point> GetLinePoints(Point point, Size size, double resolution)
        {
            var hieght = (size.Height * resolution);
            var width = (size.Width * resolution);

            var newMapMinY = point.Y - hieght;
            var newMapMaxX = point.X + width;

            var pointList = new List<Point>()
                {
                    new Point(point.X, newMapMinY),
                    new Point(newMapMaxX,  point.Y)
                };

            return pointList;
        }

        /// <summary>
        /// for the DrawLine
        /// </summary>
        /// <param name="point">map에서의 포인터 위치 </param>
        /// <param name="point1">Line의 x1, x2</param>
        /// <param name="point2">Line의 y1, y2</param>
        /// <param name="size">그려진 라인의 바운더리 사이즈</param>
        /// <param name="resolution">지도의 resolution</param>
        /// <returns></returns>
        public static List<Point> GetLinePoints(Point point, Point point1, Point point2, Size size, double resolution)
        {
            double hieght = (size.Height * resolution);
            double width = (size.Width * resolution);

            double newMapMinY = point.Y - hieght;
            double newMapMaxX = point.X + width;

            if (point1.X < point2.X)  // 오른쪽으로 마우스 드래깅
            {
                //newMapMinY = point.Y - hieght;
                newMapMaxX = point.X + width;
            }
            else if (point1.X > point2.X)  // 왼쪽으로 마우스 드래깅
            {
                //newMapMinY = point.Y - hieght;
                newMapMaxX = point.X - width;
            }

            if (point1.Y < point2.Y)  // 아래로 마우스 드래깅
            {
                newMapMinY = point.Y + hieght;
                //newMapMaxX = point.X + width;
            }
            else if (point1.Y > point2.Y)  // 위로 마우스 드래깅
            {
                newMapMinY = point.Y - hieght;
                //newMapMaxX = point.X - width;
            }

            var pointList = new List<Point>()
                {
                    new Point(point.X, point.Y),
                    new Point(newMapMaxX,  newMapMinY)
                };

            return pointList;
        }

        /// <summary>
        /// 랩 어라운드 맵인 경우
        /// Postion을 
        /// full extent 기준으로 설정 한다
        /// 
        /// 현재 쓰이고 있지 않음 혹시 필요할지 몰라 남겨놓았음
        /// </summary>
        /// <param name="fullextent">현재 맵을 최대로 보여줬을때의 좌표 범위</param>
        /// <param name="point">변환하고싶은좌표</param>
        /// <returns></returns>
        public static double ChangePostionInFullExtent(Envelope fullextent, double point)
        {
            var newPosition = point;

            if (fullextent.XMin > point)
            {
                newPosition = ((point + fullextent.XMin) % fullextent.Width) - fullextent.XMin;
            }
            else if (fullextent.XMax < point)
            {
                newPosition = ((point - fullextent.XMin) % fullextent.Width) + fullextent.XMin;
            }

            return newPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map">현재 구동 중인 맵</param>
        /// <param name="videoExtent">비디오 좌표</param>
        /// <param name="fullExtent">전체화면 영역</param>
        /// <returns></returns>
        public static Rect ToVideoRect(Map map, Envelope videoExtent, Envelope fullExtent)
        {
            //wraparound의 맵일때 
            //Xmin이 0보다 작을때와 0보다 클경우를 하나씩 놓고 비교해서 맞는 값을
            //사용하기 위해
            var candidateMinX = new List<double>();

            //비디오 좌표를 현재 보고있는 맵의 상대좌표로 변경해준다.
            var x = (videoExtent.XMin - map.Extent.XMin);

            if (map.WrapAround)
            {
                x %= fullExtent.Width;
                //비디오의 좌표가 보이는 영역 밖으로 표시된경우
                if (x < 0)
                {
                    candidateMinX.Add(x);
                    candidateMinX.Add(x + fullExtent.Width);
                }
                else
                {
                    candidateMinX.Add(x);
                    candidateMinX.Add(x - fullExtent.Width);
                }
            }
            else
            {
                candidateMinX.Add(x);
            }

            //Y 좌표는 Wrap Around 되지 않기때문에 한번만 계산한다.
            var top = (map.Extent.YMax - videoExtent.YMax) / map.Resolution;
            var bottom = (map.Extent.YMax - videoExtent.YMin) / map.Resolution;

            foreach (var videoMinX in candidateMinX)
            {
                var videoMaxX = videoMinX + videoExtent.Width;
                //Map의 기준 좌표에서 Resolution 을 사용하여 Pixel 단위로 변환한다.
                var left = videoMinX / map.Resolution;
                var right = videoMaxX / map.Resolution;

                if (!(left > map.ActualWidth || right < 0 || top > map.ActualHeight || bottom < 0))
                {
                    return new Rect(new Point(left, top), new Point(right, bottom));
                }
            }

            return Rect.Empty;
        }

        public static bool IsInMap(List<Point> PointCollection, ESRI.ArcGIS.Client.Map baseMap)
        {
            var minX = PointCollection.Min(point => point.X);
            var maxX = PointCollection.Max(point => point.X);
            var minY = PointCollection.Min(point => point.Y);
            var maxY = PointCollection.Max(point => point.Y);

            if (baseMap.Extent.XMax < minX || baseMap.Extent.XMin > maxX) return false;
            if (baseMap.Extent.YMax < minY || baseMap.Extent.YMin > maxY) return false;

            return true;
        }
    }
}
