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
    public class NaverMapTiledService : TiledMapServiceLayer
    {
        private static int _basicRowCnt = 3;
        private static int[] _subDomains = { 1, 2, 3, 4 };
        private static string[] baseUrl =
        {
            "http://onetile{0}.map.naver.net/get/29/0/0/{1}/{2}/{3}/bl_vc_bg/ol_vc_an",
            "http://onetile{0}.map.naver.net/get/29/0/1/{1}/{2}/{3}/bl_st_bg",
            "http://onetile{0}.map.naver.net/get/29/0/0/{1}/{2}/{3}/empty/ol_st_rd/ol_st_an",
            "http://onetile{0}.map.naver.net/get/29/296779/0/{1}/{2}/{3}/empty/ol_tr_rt/ol_vc_an"
        };

        public const int skId = 102100;
        //public const string skt = "PROJCS[\"UTMK\",GEOGCS[\"GCS_ITRF_2000\",DATUM[\"D_ITRF_2000\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",1000000.0],PARAMETER[\"False_Northing\",2000000.0],PARAMETER[\"Central_Meridian\",127.5],PARAMETER[\"Scale_Factor\",0.9996],PARAMETER[\"Latitude_Of_Origin\",38.0],UNIT[\"Meter\",1.0]]";

        public MapStyle Style
        {
            get { return (MapStyle)GetValue(StyleProperty); }
            set { SetValue(StyleProperty, value); }
        }

        public NaverMapTiledService(MapStyle style)
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
            //Full extent fo the layer
            this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(90107, 1192895, 2187259, 2765759)
            {
                SpatialReference = new SpatialReference(skId)
            };
            //This layer's spatial reference
            //Set up tile information. Each tile is 256x256px, 19 levels.
            this.TileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                Origin = new MapPoint(90107, 2765759) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(skId) },
                Lods = new Lod[14]
            };
            //Set the resolutions for each level. Each level is half the resolution of the previous one.
            double resolution = 2048;
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
        /*public override string GetTileUrl(int level, int row, int col)
        {
            // Select a subdomain based on level/row/column so that it will always
            // be the same for a specific tile. Multiple subdomains allows the user
            // to load more tiles simultanously. To take advantage of the browser cache
            // the following expression also makes sure that a specific tile will always 
            // hit the same subdomain.

            //6레벨 부터 Naver Map을 표출 해 준다.
            int newLevel = level - _startLevel;
            int newCol = col - (int)(_startCol * Math.Pow(2, newLevel)) ;
            int newRow = row - (int)(_startRow * Math.Pow(2, newLevel));

            double rowCnt = _basicRowCnt * Math.Pow(2, newLevel);
            
            //Naver Map의 Row의 규칙은 ArcGis와 반대로 되어 있기 때문에 Row의 위치를 변경 해 준다.
            newRow = (int)rowCnt - newRow - 1;

            if (newLevel < 0 || newRow < 0 || newCol < 0)
            {
                //return "http://onetile1.map.naver.net/get/3/0/1/8/62/382/bl_st_bg";
                return "http://static.naver.net/maps3/mapbg_pattern1.gif";
            }

            string subdomain = _subDomains[(newCol + newRow) % _subDomains.Length].ToString();
            return string.Format(baseUrl[(int)Style], subdomain, (newLevel + 1), newCol, newRow);
        }*/

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

            //6레벨 부터 Naver Map을 표출 해 준다.
            int newLevel = level;
            int newCol = col;
            int newRow = row;

            double rowCnt = _basicRowCnt * Math.Pow(2, newLevel);

            //Naver Map의 Row의 규칙은 ArcGis와 반대로 되어 있기 때문에 Row의 위치를 변경 해 준다.
            newRow = (int)rowCnt - newRow - 1;

            if (newLevel < 0 || newRow < 0 || newCol < 0)
            {
                //return "http://onetile1.map.naver.net/get/3/0/1/8/62/382/bl_st_bg";
                return "http://static.naver.net/maps3/mapbg_pattern1.gif";
            }

            string subdomain = _subDomains[(newCol + newRow) % _subDomains.Length].ToString();
            return string.Format(baseUrl[(int)Style], subdomain, (newLevel + 1), newCol, newRow);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register("Style", typeof(MapStyle), typeof(NaverMapTiledService), new PropertyMetadata(MapStyle.General, OnStylePropertyChanged));

        private static void OnStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NaverMapTiledService obj = (NaverMapTiledService)d;
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
        /// 네이버 좌표에서 지리좌표로 반환
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static LatLng ToGeographic(double x, double y)
        {
            /*
            CoordPoint utmk = new CoordPoint(x, y);
            CoordPoint pt = CoordinateConverter.getTransCoord(utmk, CoordinateConverter.)
            
             * */
            /*
             * TO DO : UTMK 도 CoodPoint 시스템으로 받아 올수 있도록 넣어 보자~~~
             */

            Point tmp = new CoordinateConverter().GetWGS84FromUTMK(new Point(x, y));
            return new LatLng(tmp.Y, tmp.X);
        }

        /// <summary>
        /// 지리좌표에서 네이버 좌표로
        /// </summary>
        /// <param name="x">경도</param>
        /// <param name="y">위도</param>
        /// <returns></returns>
        public static Point FromGeographic(double x, double y)
        {
            return new CoordinateConverter().GetUTMKFromWGS84(new Point(x, y));
        }
    }
}
