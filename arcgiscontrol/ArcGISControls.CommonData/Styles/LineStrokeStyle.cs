using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Styles
{
    using System.Windows.Media;
    using Types;

    public class LineStrokeStyle
    {
        public DoubleCollection DashArray { get; private set; }
        public PenLineCap PenLineCap { get; private set; }
        public LineStrokeType? StrokeType { get; private set; }

        public LineStrokeStyle(LineStrokeType? lineStrokeType)
        {
            switch (lineStrokeType)
            {
                case LineStrokeType.CircleDottedLine:
                    this.DashArray = new DoubleCollection() { 0, 2 };
                    this.PenLineCap = PenLineCap.Round;
                    break;
                case LineStrokeType.SquareDottedLine:
                    this.DashArray = new DoubleCollection() { 1, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                case LineStrokeType.BrokenLine:
                    this.DashArray = new DoubleCollection() { 4, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                case LineStrokeType.OnePointChainLine:
                    this.DashArray = new DoubleCollection() { 4, 2, 2, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                case LineStrokeType.LongBrokenLine:
                    this.DashArray = new DoubleCollection() { 8, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                case LineStrokeType.LongOnePointChainLine:
                    this.DashArray = new DoubleCollection() { 8, 2, 2, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                case LineStrokeType.TwoPointChainLine:
                    this.DashArray = new DoubleCollection() { 8, 2, 2, 2, 2, 2 };
                    this.PenLineCap = PenLineCap.Square;
                    break;
                default:
                    this.DashArray = new DoubleCollection() { 1, 0 };
                    this.PenLineCap = PenLineCap.Flat;
                    lineStrokeType = LineStrokeType.FullLine;
                    break;
            }

            this.StrokeType = lineStrokeType;
        }
    }
}
