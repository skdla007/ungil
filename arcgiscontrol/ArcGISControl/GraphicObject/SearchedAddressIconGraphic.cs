using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class SearchedAddressIconGraphic : BaseGraphic, IPositionOwner
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

        private readonly ResourceDictionary resourceDictionary;

        public Point OffsetPoint
        {
            get
            {
                if (this.Symbol == null || (this.Symbol as MarkerSymbol) == null)
                    return new Point(0, 0);
                else
                    return new Point((this.Symbol as MarkerSymbol).OffsetX, (this.Symbol as MarkerSymbol).OffsetY);
            }
        }

        #endregion //Field

       
        public SearchedAddressIconGraphic(Point position, MapObjectType type, string id, string label) 
            : base(type, id)
        {
            this.Position = position;

            this.resourceDictionary = new ResourceDictionary
                                           {
                                               Source = new Uri(ArcGISConstSet.AddressIconSymbolTemplateUri,
                                                                UriKind.RelativeOrAbsolute)
                                           };

            this.Symbol = new MarkerSymbol()
            {
                ControlTemplate = this.resourceDictionary["NormalAddressSymbol"] as ControlTemplate,
                OffsetX = 12,
                OffsetY = 38
            };

            this.Attributes.Add("Label", label);
        }

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            base.OnPropertyChanged(sender, propertyChangedEventArgs);

            if (propertyChangedEventArgs.PropertyName == "Selected")
            {
                var g = sender as SearchedAddressIconGraphic;

                if (g != null)
                {
                    this.Symbol.ControlTemplate = g.Selected ? this.resourceDictionary["SelectedAddressSymbol"] as ControlTemplate : this.resourceDictionary["NormalAddressSymbol"] as ControlTemplate;
                }
            }
        }

        #endregion Event Handler
    }
}
