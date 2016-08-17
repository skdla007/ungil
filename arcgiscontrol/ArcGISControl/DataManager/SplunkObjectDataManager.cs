using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using Innotive.SplunkManager.SplunkManager;
using Innotive.SplunkManager.SplunkManager.Data;
using Application = System.Windows.Application;

namespace ArcGISControl.DataManager
{
    public class ColorChangedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public Object Color { get; set; }
        public bool IsBlinking { get; set; }
        public MapObjectType ObjectType { get; set; }

        public ColorChangedEventArgs(string id, Object color, bool isBlinking, MapObjectType type)
        {
            this.Id = id;
            this.Color = color;
            this.IsBlinking = isBlinking;
            this.ObjectType = type;
        }
    }

    public class SplunkObjectDataManager : SplunkServiceHandler
    {
        #region Fields

        protected ObservableCollection<MapSplunkObjectDataInfo> objectDatas;

        public ObservableCollection<MapSplunkObjectDataInfo> SplunkObjectDatas
        {
            get { return this.objectDatas; }
        }

        protected readonly List<SplunkChartTableWrapperControl> splunkChartTableWrapperControls;
        public List<SplunkChartTableWrapperControl> SplunkChartTableWrapperControls
        {
            get { return this.splunkChartTableWrapperControls; }
        }

        #endregion //Fields

        #region events

        /// <summary>
        /// Splunk Service를 통하여 Color값 변경 통보
        /// </summary>
        public event EventHandler<ColorChangedEventArgs> eColorChanged;

        #endregion //events

        #region Construction

        public SplunkObjectDataManager()
        {
            this.objectDatas = new ObservableCollection<MapSplunkObjectDataInfo>();
            this.splunkChartTableWrapperControls = new List<SplunkChartTableWrapperControl>();
        }

        #endregion //Construction

        #region Methods

        #region Graphic 관련

        public MapSplunkObjectDataInfo CreatePositionMapSplunkObjectData(Point point, double resoluiton, MapSplunkObjectDataInfo splunkObjectDataInfo)
        {
            splunkObjectDataInfo.PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resoluiton);

            var gap = (5*resoluiton);

            splunkObjectDataInfo.IconPosition = new Point(point.X - gap, point.Y + gap); 

            splunkObjectDataInfo.ControlSize = ArcGISConstSet.ObjectBasicSize;

            return splunkObjectDataInfo;
        }

        /// <summary>
        /// Object Data 하나 추가
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public IEnumerable<BaseGraphic> MakeOneObjectGraphics(MapSplunkObjectDataInfo data)
        {
            var mapSplunkData = data;

            if (mapSplunkData == null) return null;

            if (mapSplunkData.ObjectID == null)
            {
                mapSplunkData.ObjectID = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(mapSplunkData.Name))
            {
                mapSplunkData.Name = mapSplunkData.SplunkBasicInformation.Name;
            }

            if (string.IsNullOrWhiteSpace(mapSplunkData.ChartAxisXTitle))
            {
                mapSplunkData.ChartAxisXTitle = mapSplunkData.SplunkBasicInformation.XAxisTitle;
            }

            if (string.IsNullOrWhiteSpace(mapSplunkData.ChartAxisYTitle))
            {
                mapSplunkData.ChartAxisYTitle = mapSplunkData.SplunkBasicInformation.YAxisTitle;
            }

            var splunkIconGraphic
                = new SplunkIconGraphic(mapSplunkData.IconPosition, MapObjectType.SplunkIcon, mapSplunkData.ObjectID)
                {
                    IsVisible = mapSplunkData.IsIconHidden == false
                };

            splunkIconGraphic.ZIndexChanged += (s, e) =>
            {
                mapSplunkData.IconZIndex = ((BaseGraphic)s).ZIndex;
            };
            
            var splunkControl = this.MakeSplunkControl(mapSplunkData);

            var splunkControlGraphic =
                new PolygonControlGraphic<SplunkChartTableWrapperControl>(
                    splunkControl,
                    MapObjectType.SplunkControl,
                    mapSplunkData.ObjectID,
                    mapSplunkData.PointCollection
                ) { IsVisible = false };

            splunkControlGraphic.ZIndexChanged += (s, e) =>
            {
                mapSplunkData.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.SplunkObjectDatas.Add(mapSplunkData);

            mapSplunkData.SetSplunkBounds();

            return new BaseGraphic[] { splunkIconGraphic, splunkControlGraphic };
        }

        /// <summary>
        /// Splunk 하나 추가
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private SplunkChartTableWrapperControl MakeSplunkControl(MapSplunkObjectDataInfo data)
        {
            var splunkChartTableWrapperControl = new SplunkChartTableWrapperControl(false)
            {
                MapSplunkObjectData = data
            };

            splunkChartTableWrapperControl.SetTitle(data.Title);
            splunkChartTableWrapperControl.ChartControl.ChartAxisXTitle = data.ChartAxisXTitle;
            splunkChartTableWrapperControl.ChartControl.ChartAxisYTitle = data.ChartAxisYTitle;
            splunkChartTableWrapperControl.ChartControl.ShowXAxis = data.ShowXAxis;
            splunkChartTableWrapperControl.ChartControl.SetYAxisRange(data.YAxisRangeMin, data.YAxisRangeMax);
            splunkChartTableWrapperControl.ChartControl.DateTimeFormat = data.ChartDateTimeFormat;
            splunkChartTableWrapperControl.ChartControl.LegendFontSize = data.GetLegendFontSizeCombinedWithDefaultSetting();
            splunkChartTableWrapperControl.ChartControl.LegendProportion = data.GetLegendSizeCombinedWithDefaultSetting();
            splunkChartTableWrapperControl.eColorChanged += SplunkChartTableWrapperControl_ColorChanged;

            this.splunkChartTableWrapperControls.Add(splunkChartTableWrapperControl);
            return splunkChartTableWrapperControl;
        }

        /// <summary>
        /// Object Data 하나 삭제
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DeleteOneObject(MapSplunkObjectDataInfo data)
        {
            var cameraObjectComponentData = data;
            if (cameraObjectComponentData != null && this.objectDatas.Contains(cameraObjectComponentData)) this.SplunkObjectDatas.Remove(cameraObjectComponentData);

            var splunkControl =
                this.splunkChartTableWrapperControls.FirstOrDefault(item => item.ObjectID == data.ObjectID);

            if (splunkControl != null)
            {
                splunkControl.eColorChanged -= this.SplunkChartTableWrapperControl_ColorChanged;
                this.splunkChartTableWrapperControls.Remove(splunkControl);
            }

            return true;
        }

        public SplunkChartTableWrapperControl GetSplunkControl(string id)
        {
            return this.SplunkChartTableWrapperControls.FirstOrDefault(item => item.ObjectID == id);
        }

        #endregion //Grphics 관련

        #region List 관련

        /// <summary>
        /// Get Splunk Data
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public MapSplunkObjectDataInfo GetObjectDataByObjectID(string objectID)
        {
            return this.objectDatas.FirstOrDefault(item => item.ObjectID == objectID);
        }

        public virtual List<string> GetObjectIDListInExtent(Envelope extent)
        {
            if (this.objectDatas == null) return null;

            var list = new List<string>();
            foreach (var objectData in this.objectDatas)
            {
                var objectExtent = new Envelope(
                    objectData.ExtentMin.X, objectData.ExtentMin.Y,
                    objectData.ExtentMax.X, objectData.ExtentMax.Y
                );
                if (extent.Intersects(objectExtent))
                {
                    list.Add(objectData.ObjectID);
                }
            }
            return list;
        }

        public virtual List<MapSplunkObjectDataInfo> GetObjectListInExtent(Envelope extent)
        {
            if (this.objectDatas == null) return null;

            var list = new List<MapSplunkObjectDataInfo>();
            foreach (var objectData in this.objectDatas)
            {
                var objectExtent = new Envelope(
                    objectData.ExtentMin.X, objectData.ExtentMin.Y,
                    objectData.ExtentMax.X, objectData.ExtentMax.Y
                );
                if (extent.Intersects(objectExtent))
                {
                    list.Add(objectData);
                }
            }
            return list;
        }

        /// <summary>
        /// Clear Object Datas
        /// </summary>
        public void ClearObjectDatas()
        {
            foreach (var splunkControl in splunkChartTableWrapperControls)
            {
                splunkControl.eColorChanged -= this.SplunkChartTableWrapperControl_ColorChanged;
            }

            this.SplunkObjectDatas.Clear();
            this.splunkChartTableWrapperControls.Clear();
        }

        #endregion //List 관련

        #region Splunk 관련
        
        /// <summary>
        /// 스플렁크 서비스 Callback 함수
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="splunkResultSet"></param>
        override protected void SetSplunkCallbackData(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkResultSet splunkResultSet)
        {
            if (splunkResultSet.SplunkDataTable == null) return;

            var splunkControl = this.splunkChartTableWrapperControls.FirstOrDefault(Item => Item.ObjectID == objectId);

            if (splunkControl != null)
            {
                splunkControl.SetSplunkControlUI(objectId, splunkBasicInformation, splunkResultSet);
                splunkControl.SetSplunkControlSize();
            }

        }

        /// <summary>
        /// 스플렁크 서비스 시작
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="savedSearchArgs"></param>
        override protected void StartSplunkService(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkSavedSearchArgs savedSearchArgs)
        {
            base.StartSplunkService(objectId, splunkBasicInformation, savedSearchArgs);
            
            var splunkControl = this.GetSplunkControl(objectId);

            if (splunkControl != null)
            {
                splunkControl.ShowLoading();
            }
        }

        /// <summary>
        /// 모든 스플렁크 서비스 정지
        /// </summary>
        override public void StopAllSplunkServices()
        {
            foreach (var splunkObjectData in this.SplunkObjectDatas)
            {
                this.StopSplunkService(splunkObjectData.ObjectID, splunkObjectData.SplunkBasicInformation);
            }
        }
        
        #endregion //Splunk 관련

        #endregion //Methods 관련

        #region Event Handlers

        /// <summary>
        /// Splunk Wrapper Control color 변경 Event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="colorChangedEventArgs"></param>
        private void SplunkChartTableWrapperControl_ColorChanged(object sender, ColorChangedEventArgs colorChangedEventArgs)
        {
            this.RaiseColorChangeEvent(colorChangedEventArgs);
        }

        /// <summary>
        /// Color Event 발생
        /// </summary>
        /// <param name="eventArgs"></param>
        private void RaiseColorChangeEvent(ColorChangedEventArgs eventArgs)
        {
            var colorChangedEvent = this.eColorChanged;
            if (colorChangedEvent != null)
            {
                colorChangedEvent(this, eventArgs);
            }
        }

        #endregion Event Handlers
    }
}
