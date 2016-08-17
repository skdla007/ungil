using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.GraphicObject
{
    using Helper;
    using ESRI.ArcGIS.Client.Symbols;
    using System.Windows;
    using System.Windows.Media;
    using ArcGISControls.CommonData.Types;
    using ESRI.ArcGIS.Client.Geometry;
    using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

    public class ImagePolygonGraphic : BaseGraphic, IPointCollectionOwner
    {
        #region Fields

        public CommonImageObjectDataInfo ImageObjectData { get; private set; }

        public List<VertexIconGraphic> VertexIconGraphics { get; set; }

        protected List<Point> pointCollection;

        public List<Point> PointCollection
        {
            get
            {
                return this.pointCollection;
            }

            set
            {
                var mapPointCollection = new PointCollection();

                foreach (var point in value)
                {
                    mapPointCollection.Add(new MapPoint(point.X, point.Y));
                }

                var polygon = new Polygon();

                polygon.Rings.Add(mapPointCollection);

                this.Geometry = polygon;

                this.pointCollection = value;

                this.RaisePointCollectionChangedEvent();
            }
        }

        public event EventHandler<EventArgs> PointCollectionChanged;

        private void RaisePointCollectionChangedEvent()
        {
            if (this.PointCollectionChanged != null)
            {
                this.PointCollectionChanged(this, EventArgs.Empty);
            }
        }


        #endregion //Fields

        /// <summary>
        /// 
        /// </summary>
        public ImagePolygonGraphic(List<Point> pointCollection, CommonImageObjectDataInfo imageObjectData, MapObjectType type, string id)
            : base(type, id)
        {
            this.PointCollection = pointCollection;
            this.ImageObjectData = imageObjectData;
            this.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Symbol에 색깔 대신 Image를 채운다
        /// </summary>
        /// <param name="imageStream"></param>
        public void ChageSymbolImage(string imageStream)
        {
            this.ChageSymbolImage(imageStream, 1);
        }

        public void ChageSymbolImage(string imageStream, double opacity)
        {
            var bitmapImage = ImageStreamContorl.FileSteamToImage(imageStream);

            var symbol = (this.Symbol as SimpleFillSymbol);

            var imageBrush = new ImageBrush()
            {
                ImageSource = bitmapImage,
                Opacity = opacity
            };

            if (symbol != null)
            {
                symbol.Fill = imageBrush;
            }
            else
            {
                this.Symbol = new SimpleFillSymbol()
                                  {
                                      Fill = imageBrush,
                                      BorderThickness = 0
                                  };
                
            }

            this.ImageObjectData.ImageDataStream = imageStream;
            this.ImageObjectData.ImageOpacity = opacity;
        }

        public void ChangeSymbolImageOpacity(double opacity)
        {
            var symbol = (this.Symbol as SimpleFillSymbol);
            if (symbol == null)
                return;

            var imageBrush = symbol.Fill as ImageBrush;
            if (imageBrush == null)
                return;

            imageBrush.Opacity = opacity;
            this.ImageObjectData.ImageOpacity = opacity;
        }

        /// <summary>
        /// Splunk 에서 색깔만 변경 해줄 경우
        /// </summary>
        /// <param name="color"></param>
        public void ChageColorOnlyBySplunk(object color)
        {
            var symbol = (this.Symbol as SimpleFillSymbol);
            if (symbol == null)
                return;

            var newBrush = color is Brush ? (SolidColorBrush)color : new SolidColorBrush(new Color());

            if (color == null)
            {
                symbol.BorderThickness = 0;
            }
            else
            {
                symbol.BorderThickness = 2;
                symbol.BorderBrush = newBrush;
            }
            
        }

        public void SetVertexIconGraphics(List<Point> movePointCollection)
        {
            if (this.VertexIconGraphics == null)
            {
                this.VertexIconGraphics = new List<VertexIconGraphic>();
            }

            var pointCnt = movePointCollection.Count;
            var vertextGraphicCount = this.VertexIconGraphics.Count;

            if (vertextGraphicCount > pointCnt)
            {
                this.VertexIconGraphics.RemoveRange(pointCnt, vertextGraphicCount - pointCnt);
            }

            for (int i = 0; i < pointCnt; i++)
            {
                var point = movePointCollection[i];
                if (vertextGraphicCount > i)
                {
                    this.VertexIconGraphics[i].Position = point;
                }
                else
                {
                    this.VertexIconGraphics.Add(new VertexIconGraphic(point));
                }
            }
        }

        public void SetVertexIconGraphics(Vector displacement)
        {
            if (this.VertexIconGraphics == null) return;
            foreach (var vertexIconGraphic in this.VertexIconGraphics)
            {
                var oldPoint = vertexIconGraphic.Position;
                vertexIconGraphic.Position = new Point(oldPoint.X + displacement.X, oldPoint.Y + displacement.Y);
            }
        }
    }
}
