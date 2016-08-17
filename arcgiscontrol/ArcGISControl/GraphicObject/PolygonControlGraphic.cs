using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class PolygonControlGraphic<T> : ControlGraphic<T>, IPointCollectionOwner
        where T : UIElement
    {
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
                    mapPointCollection.Add(new MapPoint(point.X, point.Y) as MapPoint);
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

        public PolygonControlGraphic(T control, MapObjectType type, string id, List<Point> pointCollection)
            : base(control, type, id)
        {
            this.PointCollection = pointCollection;
            this.Symbol = new SimpleFillSymbol();
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
