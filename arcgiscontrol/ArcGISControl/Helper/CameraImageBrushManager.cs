using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Interface;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;

namespace ArcGISControl.Helper
{
    public class ImageBrushGraphicManager
    {
        private GraphicsLayer baseMapGraphicLayer;

        private IArcGISControlAPI arcGISControlApi;

        public ImageBrushGraphicManager(GraphicsLayer baseMapGraphicLayer, IArcGISControlAPI arcGISControlApi)
        {
            this.baseMapGraphicLayer = baseMapGraphicLayer;
            this.arcGISControlApi = arcGISControlApi;
        }

        /// <summary>
        /// D3dImage Refresho 
        /// </summary>
        /// <param name="tupleRect"></param>
        /// <returns></returns>
        public ImageBrush GetImageBrush(Tuple<Rect, Rect> tupleRect)
        {
            if (this.arcGISControlApi == null) return null;

            var xDxImageBrush = new ImageBrush(this.arcGISControlApi.GetImageBrush((int)(tupleRect.Item1.X + tupleRect.Item1.Width / 2),
                                                                    (int)(tupleRect.Item1.Y + tupleRect.Item1.Height / 2)));

            xDxImageBrush.AlignmentX = AlignmentX.Left;
            xDxImageBrush.AlignmentY = AlignmentY.Top;
            xDxImageBrush.Viewbox = tupleRect.Item2;
            xDxImageBrush.ViewboxUnits = BrushMappingMode.Absolute;
            xDxImageBrush.RelativeTransform = null;
            xDxImageBrush.Transform = null;
            xDxImageBrush.Stretch = Stretch.Fill;
            xDxImageBrush.TileMode = TileMode.None;

            return xDxImageBrush;
        }
    }
}
