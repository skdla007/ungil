using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace ArcGISControl.GraphicObject
{
    public class SplunkIconGraphic : BaseGraphic, IPositionOwner
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

        public bool IsVisible
        {
            get
            {
                if (this.Attributes.ContainsKey("IconVisibility") == false)
                    return false;

                return (Visibility)this.Attributes["IconVisibility"] == Visibility.Visible;
            }

            set
            {
                this.Attributes["IconVisibility"] = value ? Visibility.Visible : Visibility.Collapsed;
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

        private readonly ResourceDictionary resourceDictionary;
        
        private readonly SolidColorBrush originalColor = new SolidColorBrush()
                                                        {
                                                            //Color = Color.FromArgb(0xff, 0x00, 0x4f, 0xb0)
                                                            Color = Colors.Green
                                                        };

        #endregion //Field

        public SplunkIconGraphic(Point position, MapObjectType type, string id) 
            : base(type, id)
        {
            this.Position = position;

            this.resourceDictionary = new ResourceDictionary
                                           {
                                               Source = new Uri(ArcGISConstSet.SplunkIconSymbolTemplateUri,
                                                                UriKind.RelativeOrAbsolute)
                                           };

            var template = this.resourceDictionary["SplunkIconGraphicSymbol"] as ControlTemplate;
            
            this.Symbol = new MarkerSymbol()
            {
                ControlTemplate = template,
                OffsetX = 10,
                OffsetY = 10
            };

            this.HideEventIcon();
        }

        public void ChangeIconColor(object color, bool isBlinking)
        {
            var brush = color as SolidColorBrush;
            if (brush == null)
            {
                this.HideEventIcon();
            }
            else
            {
                this.ShowEventIcon(brush, isBlinking);
            }
        }

        private void ShowEventIcon(SolidColorBrush brush, bool isBlinking)
        {
            this.SetColorAttribute(brush);
            this.SetEventIconVisibleAttribute(isBlinking ? Visibility.Visible : Visibility.Collapsed);
        }

        private void HideEventIcon()
        {
            this.SetColorAttribute(originalColor);
            this.SetEventIconVisibleAttribute(Visibility.Collapsed);
        }

        private void SetColorAttribute(SolidColorBrush brush)
        {
            this.Attributes["Color"] = brush;
            this.Attributes["Color1"] = brush.Color;
            this.Attributes["TransparentColor1"] = ColorUtil.Transparentize(brush.Color);
        }

        private void SetEventIconVisibleAttribute(Visibility visibility)
        {
            this.Attributes["EventIconVisible"] = visibility;
        }

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            /*
            base.OnPropertyChanged(sender, propertyChangedEventArgs);

            if (propertyChangedEventArgs.PropertyName == "Selected")
            {
                var g = sender as SearchedAddressIconGraphic;

                if (g != null)
                {
                    this.Symbol.ControlTemplate = g.Selected ? this.resourceDictionary["SelectedAddressSymbol"] as ControlTemplate : this.resourceDictionary["NormalAddressSymbol"] as ControlTemplate;
                }
            }*/
        }

        #endregion Event Handler
    }
}
