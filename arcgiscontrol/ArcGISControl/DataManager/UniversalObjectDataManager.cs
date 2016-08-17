using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;
using Innotive.SplunkControl.Table;
using Innotive.SplunkManager.SplunkManager.Data;

namespace ArcGISControl.DataManager
{
    public class UniversalObjectDataManager
    {
        #region Nested classes

        protected class UniversalObject
        {
            public MapUniversalObjectDataInfo DataInfo { get; private set; }

            public UniversalControlGraphic ControlGraphic { get; private set; }

            public UniversalIconGraphic IconGraphic { get; private set; }

            public UniversalObject(MapUniversalObjectDataInfo dataInfo, UniversalControlGraphic controlGraphic, UniversalIconGraphic iconGraphic)
            {
                if (dataInfo == null || controlGraphic == null || iconGraphic == null)
                    throw new ArgumentNullException();

                this.DataInfo = dataInfo;
                this.ControlGraphic = controlGraphic;
                this.IconGraphic = iconGraphic;
            }
        }

        protected class MapSearchResultItem
        {
            public string Health { get; private set; }

            public string Metric { get; private set; }

            public string Url { get; private set; }

            public bool IsBlinking { get; private set; }

            public MapSearchResultItem(string health, string metric, string url, bool isBlinking)
            {
                this.Health = health;
                this.Metric = metric;
                this.Url = url;
                this.IsBlinking = isBlinking;
            }
        }

        #endregion Nested classes

        #region Fields

        protected Dictionary<string, UniversalObject> objects = new Dictionary<string, UniversalObject>();

        protected Dictionary<string, MapSearchResultItem> mapSearchResult = new Dictionary<string,MapSearchResultItem>();

        enum MapSearchResultStatus
        { Clean, Assigned };
        private MapSearchResultStatus mapSearchResultStatus = MapSearchResultStatus.Clean;

        private Func<double> GetMapResolution;

        private DataTable cellLinkTable;

        #endregion Fields

        #region Properties

        public IEnumerable<MapUniversalObjectDataInfo> DataInfoList
        {
            get
            {
                return this.objects.Select(pair => pair.Value.DataInfo);
            }
        }

        public string QueryStatusMessage { get; set; }

        public DataTable CellLinkTable
        {
            get { return this.cellLinkTable; }
            private set { this.cellLinkTable = value; }
        }

        #endregion Properties

        #region Constructors

        public UniversalObjectDataManager(Func<double> GetMapResolution)
        {
            this.GetMapResolution = GetMapResolution;
        }

        #endregion Constructors

        #region Private methods

        private UniversalControlGraphic CreateControlGraphic(MapUniversalObjectDataInfo dataInfo)
        {
            var graphic = new UniversalControlGraphic(dataInfo.ObjectID, dataInfo.PointCollection);

            graphic.PointCollectionChanged += this.Graphic_PointCollectionChanged;

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = graphic.ZIndex;
            };

            var universalControl = graphic.Control;

            universalControl.ControlSize = this.GetControlSize(dataInfo);
            universalControl.ShapeType = dataInfo.ShapeType;
            universalControl.Title = dataInfo.Title;
            universalControl.TitleMinMaxPosition = new Rect(dataInfo.TitleMinPosition, dataInfo.TitleMaxPosition);
            universalControl.TitleColor = dataInfo.TitleColor;
            universalControl.TitleAlignment = dataInfo.TitleAlignment;
            universalControl.IconImageUrl = dataInfo.IconImageUrl;
            universalControl.IconMinMaxPosition = new Rect(dataInfo.IconMinPosition, dataInfo.IconMaxPosition);
            universalControl.StrokeThickness = dataInfo.BorderThickness;
            universalControl.StrokeColor = dataInfo.BorderColor;
            universalControl.StrokeRadius = dataInfo.BorderRadius;
            universalControl.FillColor = dataInfo.FillColor;
            universalControl.FillImageUrl = dataInfo.FillImageUrl;

            return graphic;
        }

        private UniversalIconGraphic CreateIconGraphic(MapUniversalObjectDataInfo dataInfo)
        {
            var iconPosition = this.GetIconPosition(dataInfo);

            var graphic = new UniversalIconGraphic(iconPosition, dataInfo.ObjectID, dataInfo.AlarmLampColor)
            {
                IsVisible = dataInfo.ShowAlarmLamp,
                Size = dataInfo.AlarmLampSize,
            };

            return graphic;
        }

        private Size GetControlSize(MapUniversalObjectDataInfo dataInfo)
        {
            if(dataInfo.ControlSize.IsEmpty)
            {
                var v = (dataInfo.ExtentMax - dataInfo.ExtentMin);
                dataInfo.ControlSize = new Size(v.X, v.Y);
            }

            return dataInfo.ControlSize;
        }

        private Size SetControlSize(MapUniversalObjectDataInfo dataInfo)
        {
            var v = (dataInfo.ExtentMax - dataInfo.ExtentMin) / this.GetMapResolution();
            dataInfo.ControlSize = new Size(v.X, v.Y);

            return dataInfo.ControlSize;
        }

        private void ParseMapSearchResult(SplunkResultSet result, bool clearPreviousResult)
        {
            try
            {
                if (result.SplunkException != null)
                {
                    // TODO: 적당한 예외처리
                    return;
                }

                if (clearPreviousResult)
                    this.mapSearchResult.Clear();

                var table = result.SplunkDataTable;
                if (table == null)
                    return;

                bool existUrlColumn = false;
                bool existBlinkColumn = false;

                if (!table.Columns.Contains("obj_id")
                    || !table.Columns.Contains("health")
                    || !table.Columns.Contains("metric"))
                    return;

                if (table.Columns.Contains("url"))
                    existUrlColumn = true;

                if (table.Columns.Contains("_IW_ICON_BLINK"))
                    existBlinkColumn = true;

                foreach (DataRow row in table.Rows)
                {
                    var id = row["obj_id"].ToString();

                    if (id.Equals("_query_status"))
                    {
                        if (table.Columns.Contains("_IW_DESC"))
                        {
                            this.QueryStatusMessage = row["_IW_DESC"].ToString();
                        }

                        continue;
                    }

                    var item = new MapSearchResultItem(
                        row["health"].ToString(),
                        row["metric"].ToString(),
                        existUrlColumn ? row["url"].ToString() : string.Empty,
                        existBlinkColumn && row["_IW_ICON_BLINK"].ToString() != "no"
                    );

                    this.mapSearchResult[id] = item;
                }

                if (this.mapSearchResult.Count == 0)
                {
                    this.QueryStatusMessage = Resource_ArcGISControl_ArcGISClientViewer.Message_MapSplNoResult;
                }
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(ex, ex.GetType().ToString(), false);
            }
        }

        private string NewName()
        {
            const string PREFIX = "Universal";

            var defaultNamedNumbers = this.objects.Values.Select(
                obj =>
                {
                    var name = obj.DataInfo.Name;
                    if (name == null)
                        return 0;

                    var tokens = name.Split();
                    if (tokens.Count() != 2)
                        return 0;

                    if (tokens[0] != PREFIX)
                        return 0;

                    int n;
                    if (!int.TryParse(tokens[1], out n))
                        return 0;

                    return n;
                }
            ).Where(n => n > 0).OrderBy(n => n).Distinct();

            var i = 1;
            foreach (var number in defaultNamedNumbers)
            {
                if (number > i)
                    break;
                i++;
            }

            return String.Format("{0} {1}", PREFIX, i);
        }

        #endregion Private methods

        #region Event handlers

        private void Graphic_PointCollectionChanged(object sender, EventArgs e)
        {
            var controlGraphic = (UniversalControlGraphic)sender;
            var control = controlGraphic.Control;
            var iconGraphic = this.GetIconGraphic(controlGraphic.ObjectID);
            var dataInfo = this.GetDataInfo(controlGraphic.ObjectID);

            var changeControlSize = true;
            var oldSize = dataInfo.ExtentMax - dataInfo.ExtentMin;

            if (NumberUtil.AreSame(controlGraphic.Geometry.Extent.Width, oldSize.X) &&
                NumberUtil.AreSame(controlGraphic.Geometry.Extent.Height, oldSize.Y)) changeControlSize = false;

            dataInfo.PointCollection = controlGraphic.PointCollection;

            if(changeControlSize) control.ControlSize = this.SetControlSize(dataInfo);
            iconGraphic.Position = this.GetIconPosition(dataInfo);
        }

        #endregion Event handlers

        #region Public methods

        public Point GetIconPosition(MapUniversalObjectDataInfo dataInfo)
        {
            var size = dataInfo.ExtentMax - dataInfo.ExtentMin;

            var scaleMat = Matrix.Identity;
            scaleMat.Scale(dataInfo.AlarmLampPosition.X / 100, dataInfo.AlarmLampPosition.Y / 100);

            var displacement = size * scaleMat;

            return dataInfo.ExtentMin + displacement;
        }

        public MapUniversalObjectDataInfo CreateDataInfo(Point point, double resolution)
        {
            return new MapUniversalObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.NewName(),
                PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resolution),
                ControlSize = ArcGISConstSet.ObjectBasicSize
            };
        }

        public void AddObject(MapUniversalObjectDataInfo dataInfo)
        {
            if (dataInfo == null)
                throw new ArgumentNullException("dataInfo");

            if (this.objects.ContainsKey(dataInfo.ObjectID))
                throw new ArgumentException("같은 아이디가 존재합니다.");

            var controlGraphic = this.CreateControlGraphic(dataInfo);

            var iconGraphic = this.CreateIconGraphic(dataInfo);

            // DataInfo와 Graphic들 묶어서 Object 만들기
            var universalObject = new UniversalObject(dataInfo, controlGraphic, iconGraphic);
            this.objects[dataInfo.ObjectID] = universalObject;
        }

        public bool RemoveObject(string objectId)
        {
            return this.objects.Remove(objectId);
        }

        public void ClearObjects()
        {
            if (this.objects != null)
            {
                this.objects.Clear();
            }
            this.ClearMapSearchResult();
        }

        public void ClearMapSearchResult()
        {
            if (this.mapSearchResult != null)
            {
                this.mapSearchResult.Clear();
            }
            this.mapSearchResultStatus = MapSearchResultStatus.Clean;
        }

        public void ParseAndApplyMapSearchResult(SplunkResultSet result, bool clearPreviousReault)
        {
            this.ParseMapSearchResult(result, clearPreviousReault);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var universalObject in this.objects.Values)
                    {
                        var splunkObjectID = universalObject.DataInfo.SplunkObjectID;
                        MapSearchResultItem item;
                        if (splunkObjectID == null || !this.mapSearchResult.TryGetValue(splunkObjectID, out item))
                        {
                            universalObject.IconGraphic.ResetAlarm();
                            continue;
                        }

                        universalObject.IconGraphic.Color = item.Health;
                        // 헬스쿼리에서 받은 URL 사용하지 말자고 결정되어 주석처리함. 
                        //universalObject.DataInfo.LinkUrl = item.Url;
                        universalObject.IconGraphic.IsBlinking = item.IsBlinking;
                    }
                    this.mapSearchResultStatus = MapSearchResultStatus.Assigned;
                }), DispatcherPriority.Loaded);
        }

        public bool ContainsId(string objectId)
        {
            return this.objects.ContainsKey(objectId);
        }

        public MapUniversalObjectDataInfo GetDataInfo(string objectId)
        {
            return this.objects[objectId].DataInfo;
        }

        public UniversalControlGraphic GetControlGraphic(string objectId)
        {
            return this.objects[objectId].ControlGraphic;
        }

        public UniversalIconGraphic GetIconGraphic(string objectId)
        {
            return this.objects[objectId].IconGraphic;
        }

        public SplunkResultSet GetMetric(string objectId)
        {
            MapSearchResultItem item;
            if (this.GetDataInfo(objectId).SplunkObjectID == null
                || !this.mapSearchResult.TryGetValue(this.GetDataInfo(objectId).SplunkObjectID, out item)
                || String.IsNullOrWhiteSpace(item.Metric))
                if (this.mapSearchResultStatus == MapSearchResultStatus.Assigned)
                    return new SplunkResultSet(Guid.NewGuid().ToString(), new DataTable(), new Exception("No metric"), null);
                else
                    return new SplunkResultSet(Guid.NewGuid().ToString(), new DataTable(), new Exception("Loading..."), null);

            var splunkResultSet = new SplunkResultSet(Guid.NewGuid().ToString(), new DataTable(), null, null);

            try
            {
                var metricTable = ParseMetric(item.Metric);
                splunkResultSet.SplunkDataTable = metricTable;
            }
            catch (Exception ex)
            {
                splunkResultSet.SplunkException = ex;
            }

            return splunkResultSet;

            //var newTable = new DataTable();
            //newTable.Columns.Add("c1");
            //newTable.Columns.Add("_IW_TREND");
            //newTable.Columns.Add("c3");
            //newTable.Columns.Add("_IW_COLOR");
            //newTable.Columns.Add("_IW_LINKED_MAP");
            //newTable.Rows.Add(1, "a", "kono omoi wo", "white", "1,2,3");
            //newTable.Rows.Add(1, "b", "tsutaete hoshii kara loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong", "blue", "4,5");
            //newTable.Rows.Add(3, "c", "heavy rotation", "white", "");
            //newTable.Rows.Add(11, "x", "i want you", "blue", "");
            //newTable.Rows.Add(11, "y", "i need you", "#123456", "6");
            //newTable.Rows.Add(12, "z", "i love you", "gold", "");
            //newTable.Rows.Add(1, "aa", "kono omoi wo", "white", "1,2,3");
            //newTable.Rows.Add(1, "bb", "tsutaete hoshii kara loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong", "yellow", "4,5");
            //newTable.Rows.Add(3, "cc", "heavy rotation", "white", "");
            //newTable.Rows.Add(11, "xx", "i want you", "blue", "");
            //newTable.Rows.Add(11, "yy", "i need you", "#123456", "6");
            //newTable.Rows.Add(12, "zz", "i love you", "gold", "");
            //newTable.Rows.Add(1, "aaa", "kono omoi wo", "white", "1,2,3");
            //newTable.Rows.Add(1, "bbb", "tsutaete hoshii kara loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong", "red", "4,5");
            //newTable.Rows.Add(3, "ccc", "heavy rotation", "white", "");
            //newTable.Rows.Add(11, "xxx", "i want you", "blue", "");
            //newTable.Rows.Add(11, "yyy", "i need you", "#123456", "6");
            //newTable.Rows.Add(12, "zzz", "i love you", "gold", "");

            //return new SplunkResultSet(Guid.NewGuid().ToString(), newTable, null, null);
        }

        public string GetCellLinkUrl(string aObjectId, int aRowIndex, int aColIndex)
        {
            if (this.cellLinkTable == null || !this.ContainsId(aObjectId))
            {
                return string.Empty;
            }

            var row = this.cellLinkTable.Rows[aRowIndex];

            return row[aColIndex].ToString();
        }

        #endregion Public methods

        #region Static methods

        private DataTable ParseMetric(string metric)
        {
            var linkTable = new DataTable();

            var dataTable = new DataTable();
            var isFirstRow = true;

            var document = new XmlDocument();
            document.LoadXml(metric);
            foreach (XmlElement row in document.LastChild.ChildNodes)
            {
                if (isFirstRow)
                {
                    foreach (XmlElement col in row.FirstChild.ChildNodes)
                    {
                        var key = col.Attributes["name"].Value;
                        dataTable.Columns.Add(key);
                        linkTable.Columns.Add(key);
                    }

                    isFirstRow = false;
                }

                var dataRow = dataTable.NewRow();
                var linkRow = linkTable.NewRow();

                foreach (XmlElement col in row.FirstChild.ChildNodes)
                {
                    var key = col.Attributes["name"].Value;
                    var value = col.InnerText;

                    dataRow[key] = value;

                    //linkRow[key] = "https://www.sixbitsoftware.com/clients/knowledgebase.php?action=displayarticle&id=36";

                    if (col.HasAttribute("href"))
                    {
                        var url = col.Attributes["href"].Value;
                        linkRow[key] = url;
                    }
                }

                dataTable.Rows.Add(dataRow);
                linkTable.Rows.Add(linkRow);
            }

            this.CellLinkTable = linkTable;

            return dataTable;
        }

        #endregion Static methods
    }
}
