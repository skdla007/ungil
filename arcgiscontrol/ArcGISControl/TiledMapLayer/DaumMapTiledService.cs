using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControls.CommonData.Models;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.TiledMapLayer
{
    public class DaumMapTiledService : TiledMapServiceLayer
    {
        private static int _basicLevel = 14;
        private static int[] _subDomains = { 0, 1, 2, 3 };
        
        private static string[] baseUrl =
        {
            "http://i{0}.maps.daum-img.net/map/image/G03/i/1.04/L{1}/{2}/{3}.png",
            "http://s{0}.maps.daum-img.net/L{1}/{2}/{3}.jpg?v=090323",
            "http://h{0}.maps.daum-img.net/map/image/G03/h/1.04/L{1}/{2}/{3}.png",
            "http://r{0}.maps.daum-img.net/map/mapdata/L{1}/{2}/{3}.png?ANTI_SVC=TRUE"
        };

        public const int skId = 102100;

        public MapStyle Style
        {
            get { return (MapStyle)GetValue(StyleProperty); }
            set { SetValue(StyleProperty, value); }
        }

        public DaumMapTiledService(MapStyle style)
        {
            Style = style;
            this.SpatialReference = new SpatialReference(skId);
        }

        /// <summary>
        /// Initializes the resource.
        /// </summary>
        /// <remarks>
        /// 	<para>Override this method if your resource requires asyncronous requests to initialize,
        /// and call the base method when initialization is completed.</para>
        /// 	<para>Upon completion of initialization, check the <see cref="ESRI.ArcGIS.Client.Layer.InitializationFailure"/> for any possible errors.</para>
        /// </remarks>
        /// <seealso cref="ESRI.ArcGIS.Client.Layer.Initialized"/>
        /// <seealso cref="ESRI.ArcGIS.Client.Layer.InitializationFailure"/>
        public override void Initialize()
        {
            //Full extent fo the layer-2696440,-1465920.81739776,-2286840,-1138240.81739776
            this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(-2696440, -1460720, 3857160, 3782160)
            {
                SpatialReference = new SpatialReference(skId)
            };
            //This layer's spatial reference
            //Set up tile information. Each tile is 256x256px, 19 levels.
            this.TileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                Origin = new MapPoint(-2696440, 3782160) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(skId) },
                Lods = new Lod[14]
            };
            //Set the resolutions for each level. Each level is half the resolution of the previous one.
            double resolution = 5120;
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }

            //Call base initialize to raise the initialization event
            base.Initialize();
        }


        /// <summary>
        /// Returns a url to the specified tile
        /// </summary>
        /// <param name="level">Layer level</param>
        /// <param name="row">Tile row</param>
        /// <param name="col">Tile column</param>
        /// <returns>URL to the tile image</returns>
        public override string GetTileUrl(int level, int row, int col)
        {
            // Select a subdomain based on level/row/column so that it will always
            // be the same for a specific tile. Multiple subdomains allows the user
            // to load more tiles simultanously. To take advantage of the browser cache
            // the following expression also makes sure that a specific tile will always 
            // hit the same subdomain.

            int standR = 0, standC = 0;
            int RowCnt = (int)(5242880 / 256 / TileInfo.Lods[level].Resolution);

            int i = 1;
            while (-2696440 + (TileInfo.Lods[level].Resolution * i * 256) <= -75000)
            {
                standC = i;
                i++;
            }

            i = 1;
            while (-1460720 + (TileInfo.Lods[level].Resolution * i * 256) <= -150000)
            {
                standR = i;
                i++;
            }

            //6레벨 부터 Naver Map을 표출 해 준다.
            int newLevel = _basicLevel - level;
            int newCol = col - standC;
            int newRow = RowCnt - row - 1;
            //RowCnt = RowCnt - standR - 1 ;
            newRow = newRow - standR;
            //double rowCnt = _basicRowCnt * Math.Pow(2, level);

            //Daum Map의 Row의 규칙은 ArcGis와 반대로 되어 있기 때문에 Row의 위치를 변경 해 준다.
            //newRow = (int)rowCnt - newRow;

            //newRow = (int)rowCnt - newRow - 1;
            /*
            if (newLevel < 0 || newRow < 0 || newCol < 0)
            {
                return "http://i1.daumcdn.net/imap/apis/white.png";
            }
            */
            string subdomain = _subDomains[GetSubDomain(newCol)].ToString();
            return string.Format(baseUrl[(int)Style], subdomain, newLevel, newRow, newCol);
        }

        private int GetSubDomain(int col)
        {
            col = col % 4;

            if (col < 0)
            {
                col += 4;
            }
            return col;
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register("Style", typeof(MapStyle), typeof(DaumMapTiledService), new PropertyMetadata(MapStyle.General, OnStylePropertyChanged));

        private static void OnStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DaumMapTiledService obj = (DaumMapTiledService)d;
            if (obj.IsInitialized)
                obj.Refresh();
        }

        public enum MapStyle : int
        {
            General = 0,
            Satellite = 1,
            SatelliteOver = 2,
            Traffic = 3
        }

        /// <summary>
        /// 다음 좌표에서 지리좌표로 반환
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static LatLng ToGeographic(double x, double y)
        {
            CoordPoint ktmPt = new CoordPoint(x, y);
            CoordPoint pt = CoordinateConverter.getTransCoord(ktmPt, CoordinateConverter.COORD_TYPE_WCONGNAMUL, CoordinateConverter.COORD_TYPE_WGS84);

            return new LatLng(pt.Y, pt.X);
        }

        /// <summary>
        /// 지리좌표에서 다음 좌표로
        /// </summary>
        /// <param name="x">경도</param>
        /// <param name="y">위도</param>
        /// <returns></returns>
        public static Point FromGeographic(double x, double y)
        {
            CoordPoint pt = new CoordPoint(x, y);
            CoordPoint ktmPt = CoordinateConverter.getTransCoord(pt, CoordinateConverter.COORD_TYPE_WGS84, CoordinateConverter.COORD_TYPE_WCONGNAMUL);
            return new Point(ktmPt.X, ktmPt.Y);
        }


    }
}
