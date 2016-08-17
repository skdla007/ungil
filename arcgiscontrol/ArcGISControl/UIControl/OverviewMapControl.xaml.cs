
using System;
using System.Windows.Controls;
using ArcGISControl.TiledMapLayer;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for OverviewMapControl.xaml
    /// </summary>
    public partial class OverviewMapControl : UserControl
    {
        public OverviewMapControl(Map baseMap)
        {
            InitializeComponent();

            this.xBaseMapOver.Map = baseMap;
        }

        public void ChangeLayer(TiledMapServiceLayer tiledMapServiceLayer)
        {
            if (tiledMapServiceLayer == null ||  tiledMapServiceLayer.FullExtent == null) return;

            this.xBaseMapOver.Map.Extent = tiledMapServiceLayer.FullExtent;
            
            if (tiledMapServiceLayer is CustomMapTiledService)
            {
                var customMapTileServie = tiledMapServiceLayer as CustomMapTiledService;
                var newCustomMapTileService = new CustomMapTiledService(customMapTileServie.Level, customMapTileServie.Domain,
                        customMapTileServie.TileWidth, customMapTileServie.TileHeight,
                        customMapTileServie.TotalWidth, customMapTileServie.TotalHeight, customMapTileServie.MinWidth, customMapTileServie.MinHeight);
                this.xBaseMapOver.Layer
                    = newCustomMapTileService;
                
            }
            else
            {
                InnowatchDebug.Logger.Trace("N Branching Statement Processing - if (tiledMapServiceLayer is CustomMapTiledService) else");
            }
        }
    }
}
