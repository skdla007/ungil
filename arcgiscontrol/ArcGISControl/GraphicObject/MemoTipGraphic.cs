using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class MemoTipGraphic : PolygonGraphic
    {
        public Point TipPosition
        {
            get
            {
                return this.pointCollection[1];
            }
        }

        public MemoTipGraphic(string id, List<Point> pointCollection)
            : base(pointCollection, ArcGISConstSet.MapTipFillColor, ArcGISConstSet.MapTipBorderColor, MapObjectType.MemoTip, id)
        {
            var fillSymbol = this.Symbol as SimpleFillSymbol;
            Debug.Assert(fillSymbol != null);
            fillSymbol.BorderThickness = ArcGISConstSet.MapTipBorderThickness;
        }
    }
}
