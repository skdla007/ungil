using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.TiledMapLayer
{
    public class CustomMapTiledService : TiledMapServiceLayer
    {
        private int _basicLevel = 14;
        public int Level
        {
            get { return this._basicLevel; }
        }
        
        private string basUrl = "{0}/{1}-{2}-{3}.jpg";
        private string domain;
        public string Domain
        {
            get { return this.domain; }
        }

        public const int skId = 102100;

        private int tileWidth = 0;
        public int TileWidth
        {
            get { return this.tileWidth; }
        }

        private int  tileHeight = 0;
        public int TileHeight
        {
            get { return this.tileHeight; }
        }

        private double totalWidth = 0;
        public double TotalWidth
        {
            get { return this.totalWidth; }
        }

        private double totalHeight = 0;
        public double TotalHeight
        {
            get { return this.totalHeight; }
        }

        private double minWidth = 0;
        public double MinWidth
        {
            get { return this.minWidth; }
        }

        private double minHeight = 0;
        public double MinHeight
        {
            get { return this.minHeight; }
        }

        public CustomMapTiledService()
        {
            
        }

        public CustomMapTiledService(int level, string domain, int tileWidth, int tileHeight, double totalWidth, double totalHeight, double minWidth, double minHeight)
        {
            _basicLevel = level;
            this.domain = domain;
            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.minWidth = minWidth;
            this.minHeight = minHeight;

            this.SpatialReference = new SpatialReference(skId);
        }

        public override void Initialize()
        {    
            //Full extent fo the layer-2696440,-1465920.81739776,-2286840,-1138240.81739776
            this.FullExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(0, 0, totalWidth, totalHeight)
            {
                SpatialReference = new SpatialReference(skId)
            };
            //This layer's spatial reference
            //Set up tile information. Each tile is 256x256px, 19 levels.
            this.TileInfo = new TileInfo()
            {
                Height = tileWidth,
                Width = tileHeight,
                Origin = new MapPoint(0, totalHeight) { SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(skId) },
                Lods = new Lod[_basicLevel]
            };
            //Set the resolutions for each level. Each level is half the resolution of the previous one.
            double resolution = 1;
            for (var i = TileInfo.Lods.Length; i-->0 ;)
            {
                TileInfo.Lods[i] = new Lod() { Resolution = resolution };
                resolution *= 2;
            }

            try
            {
                //Call base initialize to raise the initialization event
                base.Initialize();
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(ex, ex.GetType().ToString());   
            }
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
            int newlevel = _basicLevel - level;
            string a = string.Format(basUrl, domain, newlevel, row, col);
            return a;
        }
    }
}
