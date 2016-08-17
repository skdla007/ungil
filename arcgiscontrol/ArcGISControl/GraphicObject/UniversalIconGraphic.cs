using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class UniversalIconGraphic : BaseGraphic, IPositionOwner
    {
        #region Field

        /// <summary>
        /// Geometry가 MapPoint가 아니거나 null일 때 참조하면 InvalidOperationException을 던집니다.
        /// </summary>
        public Point Position
        {
            get
            {
                var mapPoint = this.Geometry as MapPoint;
                if (mapPoint == null)
                    throw new InvalidOperationException("Geometry가 MapPoint가 아니거나 null입니다.");

                return new Point(mapPoint.X, mapPoint.Y);
            }

            set
            {
                this.Geometry = new MapPoint(value.X, value.Y);
                this.RaisePositionChangedEvent();
            }
        }

        public event EventHandler<EventArgs> PositionChanged;

        private void RaisePositionChangedEvent()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
        }

        public bool IsVisible
        {
            get
            {
                if (this.Attributes.ContainsKey("Visibility") == false)
                    return false;

                return (Visibility)this.Attributes["Visibility"] == Visibility.Visible;
            }

            set
            {
                this.Attributes["Visibility"] = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsBlinking
        {
            get
            {
                if (this.Attributes.ContainsKey("EventVisibility") == false)
                    return false;

                return (Visibility)this.Attributes["EventVisibility"] == Visibility.Visible;
            }

            set
            {
                this.Attributes["EventVisibility"] = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public double Size
        {
            get
            {
                if (!this.Attributes.ContainsKey("Size"))
                    return double.NaN;

                return (double)this.Attributes["Size"];
            }

            set
            {
                this.Attributes["Size"] = value;
            }
        }

        private string originalColor;

        private string color;

        public string Color
        {
            get { return this.color; }

            set
            {
                var brush = BrushUtil.ConvertFromString(value);

                if (brush == null)
                {
                    return;
                }

                this.color = value;
                
                this.Attributes["Fill"] = brush;
                this.Attributes["Color"] = brush.Color;
                this.Attributes["TransparentColor"] = ColorUtil.Transparentize(brush.Color);
            }
        }

        #endregion // Field

        public UniversalIconGraphic(Point position, string id, string color)
            : base(MapObjectType.UniversalIcon, id)
        {
            this.Position = position;
            this.IsVisible = true;
            this.IsBlinking = false;
            this.Size = 6;
            this.Color = this.originalColor = color;

            var resourceDictionary = new ResourceDictionary()
            {
                Source = new Uri(ArcGISConstSet.UniversalIconSymbolTemplateUri, UriKind.RelativeOrAbsolute)
            };

            var template = resourceDictionary["UniversalIconGraphicSymbol"] as ControlTemplate;

            this.Symbol = new MarkerSymbol()
            {
                ControlTemplate = template,
                OffsetX = 10,
                OffsetY = 10
            };
        }

        public void ResetAlarm()
        {
            this.IsBlinking = false;
            this.Color = this.originalColor;
        }
    }
}
