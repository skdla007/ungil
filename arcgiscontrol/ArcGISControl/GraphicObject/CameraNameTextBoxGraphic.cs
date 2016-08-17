using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class CameraNameTextBoxGraphic : BaseGraphic, IPositionOwner
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

        private const double borderSize = 18;
        private const double fontSize = 12;

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

        #region Construction 

        public CameraNameTextBoxGraphic(Point position, MapObjectType type, string id, string name, double size) 
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
                ControlTemplate = this.resourceDictionary["CameraNameTextSymbol"] as ControlTemplate
            };

            this.Attributes.Add("Name", name);
            this.Attributes.Add("BorderHegiht", borderSize * size);
            this.Attributes.Add("FontSize", fontSize * size);
            
        }

        #endregion //Construction 

        #region Methods

        public void SetLabelSize(double size)
        {
            if (size < 0.1)
                return;

            this.Attributes["BorderHegiht"] = borderSize * size;
            this.Attributes["FontSize"] = fontSize * size;
        }

        #endregion //Methods
    }
}
