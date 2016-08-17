using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ArcGISControl.UIControl.GraphicObjectControl;
using ESRI.ArcGIS.Client.Symbols;


namespace ArcGISControl.GraphicObject
{
    using ArcGISControls.CommonData.Models;
    using ArcGISControls.CommonData.Types;
    using System.Windows;
    using ESRI.ArcGIS.Client.Geometry;
    using System.Windows.Media;
    using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

    public class LineGraphic : BaseGraphic, IPointCollectionOwner
    {
        #region Fields

        public MapLineObjectDataInfo DataInfo { get; private set; }

        public List<VertexIconGraphic> VertexIconGraphics { get; set; }

        private List<Point> pointCollection;

        public List<Point> PointCollection
        {
            get
            {
                return this.pointCollection;
            }

            set
            {
                this.SetGeometry(value);

                this.pointCollection = value;

                this.RaisePointCollectionChangedEvent();

                this.DataInfo.PointCollection = this.pointCollection;
            }
        }

        public MapLineObjectDataInfo.LineCapTypes StartLineCap
        {
            set
            {
                this.DataInfo.StartLineCap = value;
                this.SetGeometry(this.pointCollection);
            }
        }

        public MapLineObjectDataInfo.LineCapTypes EndLineCap
        {
            set
            {
                this.DataInfo.EndLineCap = value;
                this.SetGeometry(this.pointCollection);
            }
        }

        public Color LineColor
        {
            set
            {
                var lineSymbol = this.Symbol as CustomLineSymbol;

                if(lineSymbol == null) return;

                lineSymbol.LineColor = new SolidColorBrush(value);

                this.DataInfo.ColorString = value.ToString();
            }
        }

        public int StrokeThickness
        {
            set
            {
                var lineSymbol = this.Symbol as CustomLineSymbol;

                if (lineSymbol == null) return;

                lineSymbol.StrokeThickness = value;

                this.DataInfo.StrokeThickness = value;

                this.SetGeometry(this.pointCollection);
            }
        }
        
        public PenLineJoin LineJoin
        {
            set
            {
                var lineSymbol = this.Symbol as CustomLineSymbol;

                if (lineSymbol == null) return;

                lineSymbol.LineJoin = value;

                this.DataInfo.LineJoin = value;
            }
        }

        public LineStrokeType LineStrokeType
        {
            set
            {
                var lineSymbol = this.Symbol as CustomLineSymbol;

                if (lineSymbol == null) return;

                lineSymbol.LineStrokeType = value;

                this.DataInfo.LineStrokeType = value;
            }
        }

        #endregion Fields

        #region Events

        public event EventHandler<EventArgs> PointCollectionChanged;

        private void RaisePointCollectionChangedEvent()
        {
            if (this.PointCollectionChanged != null)
            {
                this.PointCollectionChanged(this, EventArgs.Empty);
            }
        }

        #endregion Events

        #region Construction

        public LineGraphic(MapLineObjectDataInfo dataInfo)
            : base(dataInfo.ObjectType, dataInfo.ObjectID)
        {
            this.DataInfo = dataInfo;

            if (this.DataInfo == null || string.IsNullOrEmpty(this.DataInfo.ColorString)) return;

            var convertFromString = ColorConverter.ConvertFromString(dataInfo.ColorString);

            this.Symbol = new CustomLineSymbol()
            {
                StrokeThickness = this.DataInfo.StrokeThickness,
                LineColor = convertFromString == null ? new SolidColorBrush() : new SolidColorBrush((Color)convertFromString),
                LineStrokeType = this.DataInfo.LineStrokeType != null ? this.DataInfo.LineStrokeType.Value : ArcGISConstSet.LineStrokeType,
                LineJoin = this.DataInfo.LineJoin != null ? this.DataInfo.LineJoin.Value : ArcGISConstSet.LineJoin,
            };
            
            this.PointCollection = dataInfo.PointCollection;
        }
        #endregion Construction

        #region Methods

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

        private void SetGeometry(List<Point> aPoints)
        {
            var mapPointCollection = new PointCollection();

            foreach (var point in aPoints)
            {
                mapPointCollection.Add(new MapPoint(point.X, point.Y));
            }

            var polyline = new Polyline();
            polyline.Paths.Add(mapPointCollection);

            //var polygon = new Polygon();
            //polygon.Rings.Add(mapPointCollection);

            // 화살표
            if (aPoints.Count > 1)
            {
                if (this.DataInfo.StartLineCap == MapLineObjectDataInfo.LineCapTypes.Arrow)
                {
                    polyline.Paths.Add(this.GetArrowPath(aPoints[1], aPoints[0]));
                }

                if (this.DataInfo.EndLineCap == MapLineObjectDataInfo.LineCapTypes.Arrow)
                {
                    polyline.Paths.Add(this.GetArrowPath(aPoints[aPoints.Count - 2], aPoints[aPoints.Count - 1]));
                }
            }

            Geometry = polyline;
        }

        private PointCollection GetArrowPath(Point aStart, Point aEnd)
        {
            var path = new PointCollection();

            const double arrowAngle = 80.0;

            var matx = new Matrix();
            Vector vect = aStart - aEnd;
            vect.Normalize();
            vect *= this.DataInfo.StrokeThickness * 5;

            matx.Rotate(arrowAngle / 2);
            var temp = aEnd + vect * matx;
            path.Add(new MapPoint(temp.X, temp.Y));

            path.Add(new MapPoint(aEnd.X, aEnd.Y));

            matx.Rotate(-arrowAngle);
            temp = aEnd + vect * matx;
            path.Add(new MapPoint(temp.X, temp.Y));

            return path;
        }

        #endregion Methods
    }
}
