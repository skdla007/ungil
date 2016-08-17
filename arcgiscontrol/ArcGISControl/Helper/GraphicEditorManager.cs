using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControl.Bases;
using ArcGISControl.DataManager;
using ArcGISControl.GraphicObject;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.Helper
{
    /// <summary>
    /// Thread-safe 하지 않습니다!
    /// 
    /// Memo 에서만 사용되고 있음
    /// </summary>
    public class GraphicEditorManager
    {
        private GraphicEditor graphicEditor;

        public bool IsEditing
        {
            get { return this.graphicEditor != null; }
        }

        public GraphicEditorManager()
        {
        }

        private void AddMarkerGraphicsToLayer(Action<BaseGraphic, int, BaseArcGISMap.ZLevel> addGraphicToLayer)
        {
            if (this.graphicEditor == null) return;

            foreach (var markerGraphic in this.graphicEditor.MarkerGraphics)
                addGraphicToLayer(markerGraphic, int.MaxValue, BaseArcGISMap.ZLevel.L3);
        }

        public void StartRectangleEditing(Action<BaseGraphic, int, BaseArcGISMap.ZLevel> addGraphicToLayer, BaseGraphic graphic)
        {
            if (this.graphicEditor != null) return;

            this.graphicEditor = GraphicEditor.CreateRectangleGraphicEditor(
                graphic.Geometry.Extent.XMin,
                graphic.Geometry.Extent.YMin,
                graphic.Geometry.Extent.XMax,
                graphic.Geometry.Extent.YMax
            );

            this.graphicEditor.NetValuesChanged += (s, e) =>
            {
                this.UpdateRectangleGraphic(graphic as IPointCollectionOwner, e.NetValues);
            };

            this.AddMarkerGraphicsToLayer(addGraphicToLayer);
        }

        private void UpdateRectangleGraphic(IPointCollectionOwner graphic, double[] netValues)
        {
            if (graphic == null || netValues == null || netValues.Length != 4)
                return;

            var minX = netValues[0];
            var minY = netValues[1];
            var maxX = netValues[2];
            var maxY = netValues[3];

            graphic.PointCollection = new List<Point>
            {
                new Point(minX, minY),
                new Point(minX, maxY),
                new Point(maxX, maxY),
                new Point(maxX, minY),
            };

            var textBoxControlGraphic = graphic as PolygonControlGraphic<TextBoxControl>;
            if (textBoxControlGraphic != null)
            {
                textBoxControlGraphic.Control.FitTextBoxSize();
            }
        }

        public void StartWordBalloonEditing(Action<BaseGraphic, int, BaseArcGISMap.ZLevel> addGraphicToLayer, TextBoxControlGraphic TextBoxControlGraphic, MemoTipGraphic memoTipGraphic)
        {
            if (this.graphicEditor != null) return;

            var rectExtent = TextBoxControlGraphic.Geometry.Extent;
            var tipPosition = memoTipGraphic.TipPosition;
            this.graphicEditor = GraphicEditor.CreateWordBalloonGraphicEditor(
                rectExtent.XMin,
                rectExtent.YMin,
                rectExtent.XMax,
                rectExtent.YMax,
                tipPosition.X,
                tipPosition.Y
            );

            this.graphicEditor.NetValuesChanged += (s, e) =>
            {
                this.UpdateWordBalloonGraphic(TextBoxControlGraphic, memoTipGraphic, e.NetValues);
            };

            this.AddMarkerGraphicsToLayer(addGraphicToLayer);
        }

        private void UpdateWordBalloonGraphic(TextBoxControlGraphic TextBoxControlGraphic, MemoTipGraphic memoTipGraphic, double[] netValues)
        {
            if (TextBoxControlGraphic == null
                || memoTipGraphic == null
                || netValues.Length != 6)
                return;

            var rectMinX = netValues[0];
            var rectMinY = netValues[1];
            var rectMaxX = netValues[2];
            var rectMaxY = netValues[3];
            var tipX = netValues[4];
            var tipY = netValues[5];

            var oldRect = TextBoxControlGraphic.Geometry.Extent;

            if (!NumberUtil.AreSame(oldRect.Width, Math.Abs(rectMaxX - rectMinX))
                || !NumberUtil.AreSame(oldRect.Height, Math.Abs(rectMaxY - rectMinY)))
            {
                TextBoxControlGraphic.PointCollection = new List<Point>
                {
                    new Point(rectMinX, rectMinY),
                    new Point(rectMinX, rectMaxY),
                    new Point(rectMaxX, rectMaxY),
                    new Point(rectMaxX, rectMinY),
                };
                TextBoxControlGraphic.Control.FitTextBoxSize();
            }

            memoTipGraphic.PointCollection = MemoObjectDataManager.GetTipBoundary(new Point(tipX, tipY), TextBoxControlGraphic.PointCollection);
        }

        public void MoveEditor(Vector displacement, MoveState moveState = MoveState.Default)
        {
            if (this.graphicEditor == null) return;

            this.graphicEditor.Move(displacement, moveState);
        }

        public void StopEditing(Action<BaseGraphic> removeGraphicFromLayer)
        {
            if (this.graphicEditor == null) return;

            foreach (var markerGraphic in this.graphicEditor.MarkerGraphics)
                removeGraphicFromLayer(markerGraphic);

            this.graphicEditor = null;
        }
    }
}
