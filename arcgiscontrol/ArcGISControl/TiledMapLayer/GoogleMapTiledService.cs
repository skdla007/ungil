using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.TiledMapLayer
{
    using ESRI.ArcGIS.Client;
    using System.Threading.Tasks;
    using System.IO;
    using ArcGISControls.MapTileImageProxy;

    public class GoogleMapTiledService : TiledMapServiceLayer
    {
        //구글 지도의 버전
        private static int version = 160;
        //지도 표시 언어
        private string hyperlinkLanguage = "en";

        private const double cornerCoordinate = 20037508.3427892;
        private const int WKID = 102100;

        public MapStyle Style
        {
            get { return (MapStyle)GetValue(StyleProperty); }
            set { SetValue(StyleProperty, value); }
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register("Style", typeof(MapStyle), typeof(GoogleMapTiledService), new PropertyMetadata(MapStyle.General, OnStylePropertyChanged));

        public GoogleMapTiledService(MapStyle style) : base()
        {
            this.Style = style;
        }

        private static void OnStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GoogleMapTiledService)d;
            if (obj.IsInitialized)
                obj.Refresh();
        }

        public override void Initialize()
        {
            const int totalLevel = 20;
            this.FullExtent =
            new Envelope(-cornerCoordinate, -cornerCoordinate, cornerCoordinate, cornerCoordinate)
            {
                SpatialReference = new SpatialReference(WKID)
            };

            this.SpatialReference = new SpatialReference(WKID);

            this.TileInfo = new TileInfo()
            {
                Height = 256,
                Width = 256,
                Origin = new MapPoint(-cornerCoordinate, cornerCoordinate) { SpatialReference = new SpatialReference(WKID) },
                Lods = new Lod[totalLevel]
            };

            double resolution = cornerCoordinate / 256;
            for (int i = 0; i < TileInfo.Lods.Length; i++)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution /= 2;
            }

            base.Initialize();
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            string url = "";

            switch (this.Style)
            {
                case MapStyle.General :
                    url = "https://mts0.google.com/vt/?" +
                          "x=" + col +
                          "&hl=" + this.hyperlinkLanguage +
                          "&y=" + row +
                          "&z=" + (level + 1).ToString();
                    break;
                case MapStyle.Satellite :
                    url = "https://khms1.google.com/kh/" +
                          "v=" + version +
                          "&src=app" +
                          "&x=" + col +
                          "&y=" + row +
                          "&z=" + (level + 1).ToString();
                    break;
                case MapStyle.SatelliteOver :
                    url = "https://mts0.google.com/vt/lyrs=h@254000000" +
                          "&hl=" + this.hyperlinkLanguage +
                          "&x=" + col +
                          "&y=" + row +
                          "&z=" + (level + 1).ToString();
                    break;
            }

            return  ProxyServer.Instance.ProxyServerUrl +"?" + Uri.EscapeDataString(url);
        }

        public enum MapStyle : int
        {
            General = 0,
            Satellite = 1,
            SatelliteOver = 2
        }
    }
}
