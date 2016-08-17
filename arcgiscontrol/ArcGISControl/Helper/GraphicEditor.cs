using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControl.GraphicObject;

namespace ArcGISControl.Helper
{
    /// <summary>
    /// Memo 이외의 GraphicObject에서 사용되고 있지 않음
    /// </summary>
    public class GraphicEditor
    {
        #region Delegates

        private delegate void NetValuesUpdater(int markerIndex, Point markerPosition, double[] netValues);

        private delegate Point[] MarkerPositionSpecifier(double[] copiedNetValues);

        private delegate void NetValuesMover(Vector displacement, MoveState moveState, double[] netValues);

        #endregion Delegates

        #region Events

        public event EventHandler<NetValuesChangedEventArgs> NetValuesChanged;

        private void RaiseNetValuesChangedEvent()
        {
            if (this.NetValuesChanged != null)
            {
                this.NetValuesChanged(this, new NetValuesChangedEventArgs(this.CopyNetValues()));
            }
        }

        #endregion Events

        #region Fields

        private double[] netValues;

        private int numberOfMarkers;

        private NetValuesUpdater netValuesUpdater;

        private MarkerPositionSpecifier markerPositionSpecifier;

        private NetValuesMover netValuesMover;

        private GraphicEditingMarkerGraphic[] markerGraphics;

        private bool isUpdatingMarkerPositionByItself;

        #endregion Fields

        #region Properties

        public IEnumerable<GraphicEditingMarkerGraphic> MarkerGraphics
        {
            get
            {
                return this.markerGraphics;
            }
        }

        #endregion Properties

        #region Constructors

        private GraphicEditor(
            double[] netValues,
            int numberOfMarkers,
            NetValuesUpdater netValuesUpdater,
            MarkerPositionSpecifier markerPositionSpecifier,
            NetValuesMover netValuesMover)
        {
            if (netValuesUpdater == null
                || markerPositionSpecifier == null)
                throw new ArgumentNullException();

            this.netValues = this.CopyNetValues(netValues);
            this.numberOfMarkers = numberOfMarkers;
            this.netValuesUpdater = netValuesUpdater;
            this.markerPositionSpecifier = markerPositionSpecifier;
            this.netValuesMover = netValuesMover;

            this.MakeMarkerGraphics();
        }

        #endregion Constructors

        #region Private methods

        private double[] CopyNetValues()
        {
            return this.CopyNetValues(this.netValues);
        }

        private double[] CopyNetValues(double[] netValues)
        {
            if (netValues == null)
                throw new ArgumentNullException();

            var copiedNetValues = new double[netValues.Length];
            netValues.CopyTo(copiedNetValues, 0);
            return copiedNetValues;
        }

        private void MakeMarkerGraphics()
        {
            var markerPositions = this.markerPositionSpecifier(this.CopyNetValues());
            if (this.numberOfMarkers != markerPositions.Length)
                throw new ArgumentException();

            this.markerGraphics = new GraphicEditingMarkerGraphic[this.numberOfMarkers];
            for (var i = 0; i < this.numberOfMarkers; i++)
            {
                var markerGraphic = new GraphicEditingMarkerGraphic(markerPositions[i], i);
                markerGraphic.PositionChanged += this.MarkerGraphic_PositionChanged;
                this.markerGraphics[i] = markerGraphic;
            }
        }

        private void UpdateAllMarkerPosition()
        {
            var markerPositions = this.markerPositionSpecifier(this.CopyNetValues());
            Debug.Assert(this.numberOfMarkers == markerPositions.Length);
            for (var i = 0; i < this.numberOfMarkers; i++)
            {
                this.markerGraphics[i].Position = markerPositions[i];
            }
        }

        private void UpdateMarkerPosition(int markerIndex, Point markerPosition)
        {
            this.isUpdatingMarkerPositionByItself = true;

            Debug.Assert(markerIndex >= 0 && markerIndex < numberOfMarkers);
            this.netValuesUpdater(markerIndex, markerPosition, this.netValues);
            this.RaiseNetValuesChangedEvent();

            this.UpdateAllMarkerPosition();

            this.isUpdatingMarkerPositionByItself = false;
        }

        #endregion Private methods

        #region Public methods

        public void Move(Vector displacement, MoveState moveState = MoveState.Default)
        {
            this.isUpdatingMarkerPositionByItself = true;

            this.netValuesMover(displacement, moveState, this.netValues);
            //this.RaiseNetValuesChangedEvent(); related graphic의 move는 다른 곳에서 해주므로 이벤트 발생시키면 안 된다.

            this.UpdateAllMarkerPosition();

            this.isUpdatingMarkerPositionByItself = false;
        }

        #endregion Public methods

        #region Event handlers

        private void MarkerGraphic_PositionChanged(object sender, EventArgs eventArgs)
        {
            if (this.isUpdatingMarkerPositionByItself)
                return;

            var markerGraphic = sender as GraphicEditingMarkerGraphic;
            if (markerGraphic == null)
                return;

            this.UpdateMarkerPosition(markerGraphic.Index, markerGraphic.Position);
        }

        #endregion Event handlers

        #region Factory

        public static GraphicEditor CreateRectangleGraphicEditor(double minX, double minY, double maxX, double maxY)
        {
            // Y
            // ^
            // | 076
            // | 1 5
            // | 234
            // +-----> X
            const int LEFT = 0, BOTTOM = 1, RIGHT = 2, TOP = 3;
            return new GraphicEditor(
                new double[] { minX, minY, maxX, maxY },
                8,
                (index, position, netValues) =>
                {
                    switch (index)
                    {
                        case 0:
                            netValues[LEFT] = position.X;
                            netValues[TOP] = position.Y;
                            break;
                        case 1:
                            netValues[LEFT] = position.X;
                            break;
                        case 2:
                            netValues[LEFT] = position.X;
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 3:
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 4:
                            netValues[RIGHT] = position.X;
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 5:
                            netValues[RIGHT] = position.X;
                            break;
                        case 6:
                            netValues[RIGHT] = position.X;
                            netValues[TOP] = position.Y;
                            break;
                        case 7:
                            netValues[TOP] = position.Y;
                            break;
                    }
                },
                (copiedNetValues) =>
                {
                    var left = copiedNetValues[LEFT];
                    var bottom = copiedNetValues[BOTTOM];
                    var right = copiedNetValues[RIGHT];
                    var top = copiedNetValues[TOP];
                    var vCenter = (left + right) / 2;
                    var hCenter = (bottom + top) / 2;
                    return new Point[]
                    {
                        new Point(left, top),
                        new Point(left, hCenter),
                        new Point(left, bottom),
                        new Point(vCenter, bottom),
                        new Point(right, bottom),
                        new Point(right, hCenter),
                        new Point(right, top),
                        new Point(vCenter, top),
                    };
                },
                (displacement, moveState, netValues) =>
                {
                    netValues[LEFT] += displacement.X;
                    netValues[BOTTOM] += displacement.Y;
                    netValues[RIGHT] += displacement.X;
                    netValues[TOP] += displacement.Y;
                }
            );
        }

        public static GraphicEditor CreateWordBalloonGraphicEditor(
            double rectMinX, double rectMinY, double rectMaxX, double rectMaxY,
            double tipX, double tipY)
        {
            // Y
            // ^
            // | 076
            // | 1 5  8
            // | 234
            // +-----> X
            const int LEFT = 0, BOTTOM = 1, RIGHT = 2, TOP = 3, TIP_X = 4, TIP_Y = 5;
            return new GraphicEditor(
                new double[] { rectMinX, rectMinY, rectMaxX, rectMaxY, tipX, tipY },
                9,
                (index, position, netValues) =>
                {
                    switch (index)
                    {
                        case 0:
                            netValues[LEFT] = position.X;
                            netValues[TOP] = position.Y;
                            break;
                        case 1:
                            netValues[LEFT] = position.X;
                            break;
                        case 2:
                            netValues[LEFT] = position.X;
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 3:
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 4:
                            netValues[RIGHT] = position.X;
                            netValues[BOTTOM] = position.Y;
                            break;
                        case 5:
                            netValues[RIGHT] = position.X;
                            break;
                        case 6:
                            netValues[RIGHT] = position.X;
                            netValues[TOP] = position.Y;
                            break;
                        case 7:
                            netValues[TOP] = position.Y;
                            break;
                        case 8:
                            netValues[TIP_X] = position.X;
                            netValues[TIP_Y] = position.Y;
                            break;
                    }
                },
                (copiedNetValues) =>
                {
                    var left = copiedNetValues[LEFT];
                    var bottom = copiedNetValues[BOTTOM];
                    var right = copiedNetValues[RIGHT];
                    var top = copiedNetValues[TOP];
                    var vCenter = (left + right) / 2;
                    var hCenter = (bottom + top) / 2;
                    var tip_x = copiedNetValues[TIP_X];
                    var tip_y = copiedNetValues[TIP_Y];
                    return new Point[]
                    {
                        new Point(left, top),
                        new Point(left, hCenter),
                        new Point(left, bottom),
                        new Point(vCenter, bottom),
                        new Point(right, bottom),
                        new Point(right, hCenter),
                        new Point(right, top),
                        new Point(vCenter, top),
                        new Point(tip_x, tip_y),
                    };
                },
                (displacement, moveState, netValues) =>
                {
                    netValues[LEFT] += displacement.X;
                    netValues[BOTTOM] += displacement.Y;
                    netValues[RIGHT] += displacement.X;
                    netValues[TOP] += displacement.Y;
                    if (moveState != MoveState.WordBalloonRectangle)
                    {
                        netValues[TIP_X] += displacement.X;
                        netValues[TIP_Y] += displacement.Y;
                    }
                }
            );
        }

        #endregion Factory
    }

    public class NetValuesChangedEventArgs : EventArgs
    {
        public double[] NetValues { get; set; }

        public NetValuesChangedEventArgs(double[] netValues)
        {
            this.NetValues = netValues;
        }
    }

    public enum MoveState
    {
        Default,
        WordBalloonRectangle,
    }
}
