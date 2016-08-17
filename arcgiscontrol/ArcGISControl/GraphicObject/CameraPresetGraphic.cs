using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.Bases;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

namespace ArcGISControl.GraphicObject
{
    public class CameraPresetGraphic : PolygonGraphic
    {
        #region Field
        
        private int presetIndex;
        public int PresetIndex
        {
            get { return this.presetIndex; }
            set
            {
                this.presetIndex = value;
                this.Attributes["PresetIndex"] = this.presetIndex;
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
        /// <param name="selectedColor"></param>
        /// <param name="selectedBorderColor"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="presetIndex"></param>
        public CameraPresetGraphic(List<Point> pointCollection, Color color, Color borderColor, Color selectedColor,
            Color selectedBorderColor, MapObjectType type, string id, int presetIndex)
            : base(pointCollection, color, borderColor, type, id)
        {   
            this.presetIndex = presetIndex;
        }

        #endregion // Construcstion

        #region Method

        /// <summary>
        /// Click 했을 당시에 색 변경
        /// Only Preset
        /// </summary>
        public void ChangeOpaqueColor()
        {
            if (this.doNotChangeSymbolColor) return;

            var symbol = (this.Symbol as SimpleFillSymbol);
            var color = Color.FromRgb(NormalColor.R, NormalColor.G, NormalColor.B);
            var bordercolor = Color.FromRgb(this.BorderColor.R, this.BorderColor.G, this.BorderColor.B);

            symbol.Fill = new SolidColorBrush(color);
            symbol.BorderBrush = new SolidColorBrush(bordercolor);
        }


        #endregion
    }
}
