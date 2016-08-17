using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.PropertyControl;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.DataManager
{
    public class MemoObjectDataManager
    {
        #region Nested classes

        protected class MemoObject
        {
            public MapMemoObjectDataInfo DataInfo { get; private set; }

            public TextBoxControlGraphic TextBoxGraphic { get; private set; }

            public MemoTipGraphic TipGraphic { get; private set; }

            public MemoObject(MapMemoObjectDataInfo dataInfo, TextBoxControlGraphic textBoxGraphic, MemoTipGraphic tipGraphic)
            {
                if (dataInfo == null || textBoxGraphic == null || tipGraphic == null)
                    throw new ArgumentNullException();

                this.DataInfo = dataInfo;
                this.TextBoxGraphic = textBoxGraphic;
                this.TipGraphic = tipGraphic;

                this.DataInfo.PropertyChanged += DataInfo_PropertyChanged;
            }

            void DataInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("BorderColor"))
                {
                    var convertFromString = ColorConverter.ConvertFromString(this.DataInfo.BorderColor);
                    if (convertFromString != null)
                    {
                        var newColor = (Color) convertFromString;

                        this.TipGraphic.ChangeColors(newColor, newColor);
                    }
                }
            }
        }

        #endregion Nested classes

        #region Fields

        protected Dictionary<string, MemoObject> memoObjects = new Dictionary<string, MemoObject>();

        private bool isReportWritingMode;

        #endregion Fields

        #region Properties

        public bool IsReportWritingMode
        {
            set
            {
                this.isReportWritingMode = value;
                this.SetAllMemoReadOnly(!this.isReportWritingMode);
            }
        }

        #endregion Properties

        #region Constructors

        public MemoObjectDataManager()
        {
        }

        #endregion Constructors

        #region Private methods

        private void SetAllMemoReadOnly(bool isReadOnly)
        {
            foreach (var memoObjectPair in this.memoObjects)
            {
                var TextBoxControlGraphic = memoObjectPair.Value.TextBoxGraphic as TextBoxControlGraphic;
                TextBoxControlGraphic.Control.IsReadOnly = isReadOnly;
            }
        }

        private List<Point> GetBoxBoundary(Point tipPosition, Size size, double resolution)
        {
            var diff = new Size(57, 31);
            var point = new Point(
                tipPosition.X + diff.Width * resolution,
                tipPosition.Y - diff.Height * resolution
            );

            var height = (size.Height * resolution);
            var width = (size.Width * resolution);

            var newMapMinY = point.Y - height;
            var newMapMaxX = point.X + width;

            var pointList = new List<Point>()
                {
                    new Point(point.X, point.Y),
                    new Point(point.X, newMapMinY),
                    new Point(newMapMaxX, newMapMinY),
                    new Point(newMapMaxX, point.Y),
                };

            return pointList;
        }

        private TextBoxControlGraphic MakeTextBoxGraphic(MapMemoObjectDataInfo dataInfo, bool isReadOnly)
        {
            var graphic = new TextBoxControlGraphic(dataInfo.ObjectID, dataInfo.BoxBoundary);
            graphic.Control.DataInfo = dataInfo;
            graphic.Control.IsReadOnly = isReadOnly;

            graphic.PointCollectionChanged += this.TextBoxGraphic_PointCollectionChanged;

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            return graphic;
        }

        private MemoTipGraphic MakeTipGraphic(MapMemoObjectDataInfo dataInfo)
        {
            var tipBoundary = GetTipBoundary(dataInfo.TipPosition, dataInfo.BoxBoundary);

            var graphic = new MemoTipGraphic(dataInfo.ObjectID, tipBoundary);

            var convertFromString = ColorConverter.ConvertFromString(dataInfo.BorderColor);
            if (convertFromString != null)
            {
                var newColor = (Color)convertFromString;

                graphic.ChangeColors(newColor, newColor);
            }

            graphic.PointCollectionChanged += this.TipGraphic_PointCollectionChanged;

            // DataInfo의 ZIndex는 TextBoxGraphic의 ZIndex를 따른다.
            // graphic.ZIndexChanged +=

            return graphic;
        }

        #endregion Private methods

        #region Event handlers

        private void TextBoxGraphic_PointCollectionChanged(object sender, EventArgs e)
        {
            var textBoxGraphic = sender as TextBoxControlGraphic;
            if (textBoxGraphic == null) return;

            var dataInfo = this.memoObjects[textBoxGraphic.ObjectID].DataInfo;
            dataInfo.BoxBoundary = textBoxGraphic.PointCollection;
        }

        private void TipGraphic_PointCollectionChanged(object sender, EventArgs e)
        {
            var tipGraphic = sender as MemoTipGraphic;
            if (tipGraphic == null) return;

            var dataInfo = this.memoObjects[tipGraphic.ObjectID].DataInfo;
            dataInfo.TipPosition = tipGraphic.TipPosition;
        }

        #endregion Event handlers

        #region Public methods

        public IEnumerable<BaseGraphic> AddMemoObject(Point point, double resolution, string userID = null)
        {
            var size = new Size(138, 93);
            var boxBoundary = this.GetBoxBoundary(point, size, resolution);
            var logOnID = string.IsNullOrWhiteSpace(userID) == true ? string.Empty : string.Format("{0} : ", userID);
            var dataInfo = new MapMemoObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                BoxBoundary = boxBoundary,
                TipPosition = point,
                TextBoxSize = size,
                Text = logOnID,
            };

            return this.AddMemoObject(dataInfo, false);
        }

        public IEnumerable<BaseGraphic> AddMemoObject(MapMemoObjectDataInfo dataInfo, bool isReadOnly)
        {
            if (dataInfo == null)
            {
                return new BaseGraphic[] { };
            }

            if (this.memoObjects.ContainsKey(dataInfo.ObjectID))
            {
                // 같은 아이디 있으면 덮어쓰기.
                this.memoObjects.Remove(dataInfo.ObjectID);

                //throw new ArgumentException("같은 아이디가 존재합니다.");
            }

            var textBoxGraphic = this.MakeTextBoxGraphic(dataInfo, isReadOnly);

            var tipGraphic = this.MakeTipGraphic(dataInfo);

            // DataInfo와 Graphic들 묶어서 Object 만들기
            var memoObject = new MemoObject(dataInfo, textBoxGraphic, tipGraphic);
            this.memoObjects[dataInfo.ObjectID] = memoObject;

            return new BaseGraphic[] { tipGraphic, textBoxGraphic };
        }

        public void RemoveMemoObject(string objectId)
        {
            if (this.memoObjects.ContainsKey(objectId))
            {
                this.memoObjects.Remove(objectId);
            }
        }

        public void ClearObjects()
        {
            if (this.memoObjects != null)
            {
                this.memoObjects.Clear();
            }
        }

        /// <summary>
        /// Memo object를 각각 serialize 하여 IEnumerable로 돌려줍니다.
        /// </summary>
        public IEnumerable<string> SerializeObjects()
        {
            return this.memoObjects.Select(pair => pair.Value.DataInfo.SaveDataToXML());
        }

        /// <summary>
        /// Serialize 된 memo object의 IEnumerable을 받아서 각각 deserialize 하여 추가하고
        /// 추가된 graphic들을 돌려줍니다.
        /// </summary>
        public IEnumerable<BaseGraphic> DeserializeObjects(IEnumerable<string> serializedObjects)
        {
            return serializedObjects
                .Select(objectString => this.AddMemoObject(BaseMapObjectInfoData.ReadDataFromXML<MapMemoObjectDataInfo>(objectString), !this.isReportWritingMode))
                .Aggregate((x, y) => x.Concat(y));
        }

        public MapMemoObjectDataInfo GetMemoObjectDataInfo(string objectId)
        {
            if (this.memoObjects.ContainsKey(objectId))
            {
                return this.memoObjects[objectId].DataInfo;
            }

            return null;
        }

        public TextBoxControlGraphic GetTextBoxControlGraphic(string objectId)
        {
            if (this.memoObjects.ContainsKey(objectId))
            {
                return this.memoObjects[objectId].TextBoxGraphic;
            }

            return null;
        }

        public MemoTipGraphic GetMemoTipGraphic(string objectId)
        {
            if (this.memoObjects.ContainsKey(objectId))
            {
                return this.memoObjects[objectId].TipGraphic;
            }

            return null;
        }

        #endregion Public methods

        #region Static methods

        // Left < Right (X)
        // Bottom < Top (Y)
        private enum NearestType
        {
            Left, Right, Bottom, Top, LeftBottom, LeftTop, RightBottom, RightTop
        }
        public static List<Point> GetTipBoundary(Point tipPosition, List<Point> boxBoundary)
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            foreach (var point in boxBoundary)
            {
                if (point.X < minX)
                    minX = point.X;
                else if (point.X > maxX)
                    maxX = point.X;

                if (point.Y < minY)
                    minY = point.Y;
                else if (point.Y > maxY)
                    maxY = point.Y;
            }

            var p = tipPosition;
            var distances = new double[] {
                p.Y > minY && p.Y < maxY ? Math.Abs(p.X - minX) : double.PositiveInfinity, // Left
                p.Y > minY && p.Y < maxY ? Math.Abs(p.X - maxX) : double.PositiveInfinity, // Right
                p.X > minX && p.X < maxX ? Math.Abs(p.Y - minY) : double.PositiveInfinity, // Bottom
                p.X > minX && p.X < maxX ? Math.Abs(p.Y - maxY) : double.PositiveInfinity, // Top
                Math.Sqrt((p.X - minX) * (p.X - minX) + (p.Y - minY) * (p.Y - minY)), // LeftBottom
                Math.Sqrt((p.X - minX) * (p.X - minX) + (p.Y - maxY) * (p.Y - maxY)), // LeftTop
                Math.Sqrt((p.X - maxX) * (p.X - maxX) + (p.Y - minY) * (p.Y - minY)), // RightBottom
                Math.Sqrt((p.X - maxX) * (p.X - maxX) + (p.Y - maxY) * (p.Y - maxY)), // RightTop
            };
            var nearestDistance = double.PositiveInfinity;
            NearestType nearestType = NearestType.LeftBottom;
            for (var i = 0; i < distances.Length; i++)
            {
                if (distances[i] < nearestDistance)
                {
                    nearestDistance = distances[i];
                    nearestType = (NearestType)i;
                }
            }

            var boundary = new Point[3];
            boundary[1] = tipPosition;

            var halfW = (maxX - minX) / 4 / 2;
            var halfH = (maxY - minY) / 4 / 2;
            switch (nearestType)
            {
                case NearestType.Left:
                    if (p.Y - halfH < minY)
                    {
                        boundary[0] = new Point(minX, minY);
                        boundary[2] = new Point(minX, minY + halfH * 2);
                    }
                    else if (p.Y + halfH > maxY)
                    {
                        boundary[0] = new Point(minX, maxY - halfH * 2);
                        boundary[2] = new Point(minX, maxY);
                    }
                    else
                    {
                        boundary[0] = new Point(minX, p.Y - halfH);
                        boundary[2] = new Point(minX, p.Y + halfH);
                    }
                    break;

                case NearestType.Right:
                    if (p.Y - halfH < minY)
                    {
                        boundary[0] = new Point(maxX, minY);
                        boundary[2] = new Point(maxX, minY + halfH * 2);
                    }
                    else if (p.Y + halfH > maxY)
                    {
                        boundary[0] = new Point(maxX, maxY - halfH * 2);
                        boundary[2] = new Point(maxX, maxY);
                    }
                    else
                    {
                        boundary[0] = new Point(maxX, p.Y - halfH);
                        boundary[2] = new Point(maxX, p.Y + halfH);
                    }
                    break;

                case NearestType.Bottom:
                    if (p.X - halfW < minX)
                    {
                        boundary[0] = new Point(minX, minY);
                        boundary[2] = new Point(minX + halfW * 2, minY);
                    }
                    else if (p.X + halfW > maxX)
                    {
                        boundary[0] = new Point(maxX - halfW * 2, minY);
                        boundary[2] = new Point(maxX, minY);
                    }
                    else
                    {
                        boundary[0] = new Point(p.X - halfW, minY);
                        boundary[2] = new Point(p.X + halfW, minY);
                    }
                    break;

                case NearestType.Top:
                    if (p.X - halfW < minX)
                    {
                        boundary[0] = new Point(minX, maxY);
                        boundary[2] = new Point(minX + halfW * 2, maxY);
                    }
                    else if (p.X + halfW > maxX)
                    {
                        boundary[0] = new Point(maxX - halfW * 2, maxY);
                        boundary[2] = new Point(maxX, maxY);
                    }
                    else
                    {
                        boundary[0] = new Point(p.X - halfW, maxY);
                        boundary[2] = new Point(p.X + halfW, maxY);
                    }
                    break;

                case NearestType.LeftBottom:
                    boundary[0] = new Point(minX, minY + halfH);
                    boundary[2] = new Point(minX + halfW, minY);
                    break;

                case NearestType.LeftTop:
                    boundary[0] = new Point(minX, maxY - halfH);
                    boundary[2] = new Point(minX + halfW, maxY);
                    break;

                case NearestType.RightBottom:
                    boundary[0] = new Point(maxX - halfW, minY);
                    boundary[2] = new Point(maxX, minY + halfH);
                    break;

                case NearestType.RightTop:
                    boundary[0] = new Point(maxX - halfW, maxY);
                    boundary[2] = new Point(maxX, maxY - halfH);
                    break;
            }

            return boundary.ToList();
        }

        #endregion Static methods
    }
}
