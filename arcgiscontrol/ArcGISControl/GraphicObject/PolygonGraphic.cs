using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

namespace ArcGISControl.GraphicObject
{
    public class PolygonGraphic : BaseGraphic, IPointCollectionOwner
    {
        #region Field

        protected bool doNotChangeSymbolColor = false;

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

        protected Color normalColor;
        public Color NormalColor
        {
            get { return this.normalColor; }
        }

        protected Color borderColor;
        public Color BorderColor
        {
            get { return this.borderColor; }

        }

        protected Color selectedColor;
        public Color SelectedColor
        {
            get { return this.selectedColor; }
        }

        public bool IsShowGraphic
        {
            get
            {
                return this.Symbol != null;
            }
        }

        #endregion // Field

        #region Construction

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointCollection"></param>
        /// <param name="color"></param>
        /// <param name="borderColor"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public PolygonGraphic(List<Point> pointCollection, Color color, Color borderColor, MapObjectType type, string id)
            : base(type, id)
        {
            this.PointCollection = pointCollection;

            this.normalColor = color;
            this.borderColor = borderColor;
            this.selectedColor = Color.FromArgb(color.A, color.R, color.G, color.B);

            this.ChangeSymbolColor(true);
        }

        #endregion // Construcstion

        #region Method

        /// <summary>
        /// 원래 저장되어 있던 값으로 변경
        /// </summary>
        public void ChangeOriginalColor()
        {
            if (!this.doNotChangeSymbolColor)
            {
                this.Fill = new SolidColorBrush(this.normalColor);
            }
            
            this.BorderBrush = new SolidColorBrush(this.borderColor);
        }

        /// <summary>
        /// Porperty 에서 사용자에 의해 바뀐 값
        /// </summary>
        /// <param name="color"></param>
        /// <param name="borderColor"></param>
        public void ChangeColors(Color color, Color borderColor)
        {
            if (this.doNotChangeSymbolColor) return;
            
            this.Fill = new SolidColorBrush(color);
            this.BorderBrush = new SolidColorBrush(borderColor);

            this.normalColor = color;
            this.borderColor = borderColor;
            this.selectedColor = Color.FromArgb(color.A, color.R, color.G, color.B);

            this.ChangeSymbolColor();
        }

        /// <summary>
        /// Select 되었을때 값까지 Binding 
        /// Server 에서 받아온 Color 적용시 사용 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="borderColor"></param>
        /// <param name="selectedColor"></param>
        public void ChangeColors(Color color, Color borderColor, Color selectedColor)
        {
            if (this.doNotChangeSymbolColor) return;

            this.Fill = new SolidColorBrush(color);
            this.BorderBrush = new SolidColorBrush(borderColor);

            this.normalColor = color;
            this.borderColor = borderColor;
            this.selectedColor = selectedColor;

            this.ChangeSymbolColor();
        }

        private void ChangeSymbolColor(bool isShowColor = false)
        {
            if (this.doNotChangeSymbolColor) return;

            if (!isShowColor && this.Symbol == null)
                return;

            this.Fill = new SolidColorBrush(this.Selected ? this.selectedColor : this.normalColor);
            this.BorderBrush = new SolidColorBrush(this.borderColor);
        } 

        public Brush Fill
        {
            set
            {
                if (this.Symbol == null || !(this.Symbol is SimpleFillSymbol)) this.Symbol = new SimpleFillSymbol();

                var symbol = this.Symbol as SimpleFillSymbol;

                symbol.Fill = value;
            }
        }

        public SolidColorBrush BorderBrush
        {
            set
            {
                if (this.Symbol == null || !(this.Symbol is SimpleFillSymbol)) this.Symbol = new SimpleFillSymbol();

                var symbol = this.Symbol as SimpleFillSymbol;

                symbol.BorderBrush = value;
            }
        }

        virtual public void HideGraphic()
        {  
            this.Symbol = null;
        }

        virtual public void ShowGraphic()
        {
            this.ChangeSymbolColor(true);
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

        #endregion Method

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            base.OnPropertyChanged(sender, propertyChangedEventArgs);

            if (propertyChangedEventArgs.PropertyName == "Selected")
            {
                var g = sender as PolygonGraphic;

                if (g != null)
                {
                    this.ChangeSymbolColor();
                }
            }
        }

        #endregion Event Handler
    }
}
