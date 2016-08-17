using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControl.Language;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using Innotive.SplunkManager.SplunkManager;
using Innotive.SplunkManager.SplunkManager.Data;
using InnowatchConverter;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ArcGISControl.DataManager
{
    public delegate void ChangedSplunkWorkstationData(string id, WorkStationReturnedSplunkData data);

    public class WorkStationDataChangedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public WorkStationReturnedSplunkData Data { get; set; }

        public WorkStationDataChangedEventArgs(string id, WorkStationReturnedSplunkData data)
        {
            this.Id = id;
            this.Data = data;
        }
    }

    public class LocationGraphicDataManager : SplunkServiceHandler
    {
        #region Field

        private ObservableCollection<BaseMapObjectInfoData> objectDatas;

        public ObservableCollection<BaseMapObjectInfoData> ObjectDatas
        {
            get { return this.objectDatas as ObservableCollection<BaseMapObjectInfoData>; }
        }

        #endregion //Field

        #region events

        /// <summary>
        /// Splunk Service를 통하여 Color값 변경 통보
        /// </summary>
        public event EventHandler<ColorChangedEventArgs> eColorChanged;

        /// <summary>
        /// Splunk Service 에서 Workstation 의 정보 변경 통보
        /// </summary>
        public event EventHandler<WorkStationDataChangedEventArgs> eWorkstationDataChanged; 

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<CollectionChangeEventArgs<MapBookMarkDataInfo>> homeBookmarkChanged;

        #endregion //events

        #region Construction

        public LocationGraphicDataManager()
        {
            this.objectDatas = new ObservableCollection<BaseMapObjectInfoData>();
            this.objectDatas.CollectionChanged += OnCollectionChanged;
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            var senderCollection = sender as ICollection;
            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Add:
                    {
                        var homeBookmark = arg.NewItems.OfType<MapBookMarkDataInfo>().FirstOrDefault(item => item.IsHome);
                        if (homeBookmark != null)
                        {
                            this.RaiseHomeBookmarkChanged(new CollectionChangeEventArgs<MapBookMarkDataInfo>(CollectionChangeAction.Add, homeBookmark));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var homeBookmark = arg.OldItems.OfType<MapBookMarkDataInfo>().FirstOrDefault(item => item.IsHome);
                        if (homeBookmark != null)
                        {
                            this.RaiseHomeBookmarkChanged(new CollectionChangeEventArgs<MapBookMarkDataInfo>(CollectionChangeAction.Remove, homeBookmark));
                        }
                    }
                    break;
                default:
                    if (senderCollection != null)
                    {
                        var homeBookmark = senderCollection.OfType<MapBookMarkDataInfo>().FirstOrDefault(item => item.IsHome);
                        this.RaiseHomeBookmarkChanged(new CollectionChangeEventArgs<MapBookMarkDataInfo>(CollectionChangeAction.Refresh, homeBookmark));
                    }
                    break;
            }
        }

        #endregion //Construction

        #region Methods

        #region Graphic 생성 수정 관련 Methods

        #region Object 생성

        /// <summary>
        /// Object 이름 생성
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetNewObjectName(MapObjectType type)
        {
            var i = 0;
            var typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_Unknown;

            switch (type)
            {
                case MapObjectType.BookMark:
                    i = this.GetBookMarkDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_BookMark;

                    break;
                case MapObjectType.Location:
                    i = this.GetLocationObjectDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_Location;
                    break;

                case MapObjectType.Workstation:
                    i = this.GetWorkStationObjectDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_WorkStation;
                    break;

                case MapObjectType.LinkZone:
                    i = this.GetLinkZoneObjectDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_LinkZone;
                    break;

                case MapObjectType.Text:
                    i = this.GetTextDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_Text;
                    break;
                
                case MapObjectType.Line:
                     i = this.GetLineDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_Line;
                    break;
                case MapObjectType.Image:
                    i = this.GetImageDatas.Count() + 1;
                    typeString = Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNamePrefix_Image;
                    break;
            }

            return string.Format(Resource_ArcGISControl_ArcGISClientViewer.Const_DefaultNameFormat_Location, typeString, i.ToString(CultureInfo.InvariantCulture));
        }

        public MapBookMarkDataInfo CreateBookMarkObjectData(string name, Point maxPoint, Point minPoint)
        {
            var dataInfo
                = new MapBookMarkDataInfo()
                {
                    ObjectID = Guid.NewGuid().ToString(),
                    Name = name,
                    ExtentRegion = new Rect(minPoint, maxPoint),
                    ObjectType = MapObjectType.BookMark
                };

            this.objectDatas.Add(dataInfo);

            return dataInfo;
        }

        public MapBookMarkDataInfo CreateHomeBookMarkObjectData(Point maxPoint, Point minPoint)
        {
            var dataInfo = this.CreateBookMarkObjectData("[HOME]", maxPoint, minPoint);
            dataInfo.IsHome = true;
            return dataInfo;
        }

        /// <summary>
        /// WorkStationObjectData 만들기
        /// </summary>
        /// <param name="point"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public MapWorkStationObjectDataInfo CreateWorkStationObjectData(Point point, double resolution)
        {
            var workstationData = new MapWorkStationObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.GetNewObjectName(MapObjectType.Workstation),
                BorderColorString = ArcGISConstSet.LinkZoneNormalColor.ToString(),
                FillColorString = ArcGISConstSet.LinkZoneNormalColor.ToString(),
                SelectedBorderColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString(),
                SelectedFillColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString(),
                PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resolution),
                ObjectType = MapObjectType.Workstation
            };

            return workstationData;
        }

        /// <summary>
        /// LinkZoneObjectData 만들기
        /// </summary>
        /// <param name="point"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public MapLinkZoneObjectDataInfo CreateMapLinkZoneObjectData(Point point, double resolution)
        {   
            var linkZoneData = new MapLinkZoneObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.GetNewObjectName(MapObjectType.LinkZone),
                BorderColorString = ArcGISConstSet.LinkZoneNormalColor.ToString(),
                FillColorString = ArcGISConstSet.LinkZoneNormalColor.ToString(),
                SelectedBorderColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString(),
                SelectedFillColorString = ArcGISConstSet.LinkZoneSelectedColor.ToString(),
                PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resolution),
                ObjectType = MapObjectType.LinkZone,
            };

            return linkZoneData;
        }

        /// <summary>
        /// ImageLinkZoneObjectData 만들기
        /// </summary>
        /// <param name="point"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public MapLinkZoneObjectDataInfo CreateMapImageLinkZoneObjectData(Point point, double resolution)
        {
            var linkZoneData = this.CreateMapLinkZoneObjectData(point, resolution);
            linkZoneData.ImageObjectData.ImageOpacity = 1;
            linkZoneData.ObjectType = MapObjectType.ImageLinkZone;

            return linkZoneData;
        }

        /// <summary>
        /// LocationObjectData만들기
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public MapLocationObjectDataInfo CreateMapLocationObjectData(Point position)
        {
            var locationData = new MapLocationObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Position = position,
                Name = this.GetNewObjectName(MapObjectType.Location),
                InitName = this.GetNewObjectName(MapObjectType.Location)
            };

            return locationData;
        }

        /// <summary>
        /// TextObjectData 만들기
        /// </summary>
        /// <param name="point"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public MapTextObjectDataInfo CreateMapTextObjectData(Point point, double resolution)
        {   
            var textData = new MapTextObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Text = this.GetNewObjectName(MapObjectType.Text),
                Name = this.GetNewObjectName(MapObjectType.Text),
                PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resolution),
                ObjectType = MapObjectType.Text,
                IsBold = ArcGISConstSet.TextObjectBold,
                IsItalic = ArcGISConstSet.TextObjectItalic,
                IsUnderline = ArcGISConstSet.TextObjectUnderline,
                FontSize = ArcGISConstSet.TextObjectfontSize,
                FontColor = ArcGISConstSet.TextObjectFontColor.ToString(),
                BackgroundColor = ArcGISConstSet.TextObjectBackgroundColor.ToString(),
                TextAlignment = ArcGISConstSet.TextObjectAlignment,
                TextFont = MapTextObjectDataInfo.StringFromFontFamily(ArcGISConstSet.TextFontFamily),
                TextBoxSize = ArcGISConstSet.ObjectBasicSize,
                TextVerticalAlignment = ArcGISConstSet.TextObjectVerticalAlignment
            };

            return textData;
        }

        /// <summary>
        /// LineObjectData 만들기
        /// </summary>
        /// <param name="point"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public MapLineObjectDataInfo CreateMapLineObjectData(Point point, double resolution)
        {
            var lineData = new MapLineObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.GetNewObjectName(MapObjectType.Line),
                PointCollection = GeometryHelper.GetLinePoints(point, ArcGISConstSet.ObjectBasicSize, resolution),
                ObjectType = MapObjectType.Line,
                ColorString = ArcGISConstSet.LineColor.ToString(),
                LineStrokeType = ArcGISConstSet.LineStrokeType,
                StrokeThickness = ArcGISConstSet.LineStrokeThickness,
                LineJoin = ArcGISConstSet.LineJoin
            };

            return lineData;
        }

        /// <summary>
        /// DrawLineObjectData 만들기
        /// </summary>
        /// <param name="point">맵 위에 최초 클릭한 지점</param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="size">그려진 UserControl의 사이즈</param>
        /// <param name="resolution">맵의 고정된 사이즈</param>
        /// <returns></returns>
        public MapLineObjectDataInfo CreateMapLineObjectData(Point point, Point point1, Point point2, Size size, double resolution)
        {
            var lineData = new MapLineObjectDataInfo()
            {
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.GetNewObjectName(MapObjectType.Line),
                PointCollection = GeometryHelper.GetLinePoints(point, point1, point2, size, resolution),
                ObjectType = MapObjectType.Line,
                ColorString = ArcGISConstSet.LineColor.ToString(),
                LineStrokeType = ArcGISConstSet.LineStrokeType,
                StrokeThickness = ArcGISConstSet.LineStrokeThickness,
                LineJoin = ArcGISConstSet.LineJoin
            };

            return lineData;
        }

        public MapImageObjectDataInfo CreateMapImageObjectData(Point point, double resolution)
        {
            var imageData = new MapImageObjectDataInfo()
            {
                ObjectType = MapObjectType.Image,
                ObjectID = Guid.NewGuid().ToString(),
                Name = this.GetNewObjectName(MapObjectType.Image),
                PointCollection = GeometryHelper.GetRectanglePoints(point, ArcGISConstSet.ObjectBasicSize, resolution)
            };

            imageData.ImageObjectData.ImageOpacity = 1;

            return imageData;
        }

        #endregion //Object 생성

        #region Graphic 만들기

        /// <summary>
        /// 현재 생성하려는 Object의 유효성 검사
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool CheckVaidationMakingObject(BaseMapObjectInfoData data)
        {
            if (data == null) return false;

            if (string.IsNullOrEmpty(data.ObjectID))
            {
                data.ObjectID = Guid.NewGuid().ToString();
            }

            if (this.ObjectDatas.Any(item => item.ObjectID == data.ObjectID)) return false;

            return true;
        }

        /// <summary>
        /// Book Mark 저장
        /// </summary>
        /// <param name="dataInfo"></param>
        public void AddBookMark(MapBookMarkDataInfo dataInfo)
        {
            if (dataInfo == null) return;
            
            this.ObjectDatas.Add(dataInfo);
        }

        /// <summary>
        /// Location Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public IconGraphic MakeLocationGraphic(MapLocationObjectDataInfo dataInfo, bool isEditMode)
        {
            return this.MakeIconGraphic(dataInfo, isEditMode);
        }

        /// <summary>
        /// Image Linkzone 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public IEnumerable<BaseGraphic> MakeImageLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var imagePolygonGraphic = new ImagePolygonGraphic(dataInfo.PointCollection, dataInfo.ImageObjectData, dataInfo.ObjectType,
                                                              dataInfo.ObjectID);

            imagePolygonGraphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            var addedGraphicList = new List<BaseGraphic>
                                       {
                                           imagePolygonGraphic,
                                           new LinkZoneGraphic(dataInfo.PointCollection, dataInfo.FillColor,
                                                               dataInfo.BorderColor, dataInfo.ObjectType,
                                                               dataInfo.ObjectID)
                                       };

            this.ObjectDatas.Add(dataInfo);

            return addedGraphicList;
        }

        /// <summary>
        /// Linkzone Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public LinkZoneGraphic MakeLinkZoneGraphic(MapLinkZoneObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var graphic = new LinkZoneGraphic(dataInfo.PointCollection, dataInfo.FillColor,
                                dataInfo.BorderColor, dataInfo.ObjectType,
                                dataInfo.ObjectID);

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.ObjectDatas.Add(dataInfo);
            return graphic;
        }

        /// <summary>
        /// Text Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public TextBoxControlGraphic MakeTextGraphic(MapTextObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var textBoxControl = new TextBoxControl()
            {
                DataInfo = dataInfo
            };

            var graphic = new TextBoxControlGraphic(dataInfo.ObjectID, dataInfo.PointCollection, textBoxControl);
            
            graphic.PointCollectionChanged += this.TextGraphic_PointCollectionChanged;

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.ObjectDatas.Add(dataInfo);
            return graphic;
        }

        /// <summary>
        /// WorkStation Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public LinkZoneGraphic MakeWorkStationGraphic(MapWorkStationObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var graphic = new LinkZoneGraphic(dataInfo.PointCollection, dataInfo.FillColor,
                                              dataInfo.BorderColor, dataInfo.ObjectType,
                                              dataInfo.ObjectID);

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.ObjectDatas.Add(dataInfo);

            return graphic;
        }

        /// <summary>
        /// Line Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public LineGraphic MakeLineGraphic(MapLineObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var graphic = new LineGraphic(dataInfo);

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.ObjectDatas.Add(dataInfo);

            return graphic;
        }

        /// <summary>
        /// DrawLine Graphic 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public PolygonControlGraphic<LineCanvasControl> MakeDrawLineGraphic(LineCanvasControl lcc)
        {
            string ojbectID = Guid.NewGuid().ToString();
            
            var graphic = new PolygonControlGraphic<LineCanvasControl>(lcc, MapObjectType.DrawLine, ojbectID, lcc.LineControlPoint);
            
            return graphic;
        }

        /// <summary>
        /// Image 생성
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <returns></returns>
        public ImagePolygonGraphic MakeImageGraphic(MapImageObjectDataInfo dataInfo)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            var graphic = new ImagePolygonGraphic(dataInfo.PointCollection, dataInfo.ImageObjectData,
                                                  dataInfo.ObjectType,
                                                  dataInfo.ObjectID);

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            if (!string.IsNullOrEmpty(dataInfo.ImageObjectData.ImageDataStream) && !string.IsNullOrEmpty(dataInfo.ImageObjectData.ImageFileName))
            {
                graphic.ChageSymbolImage(dataInfo.ImageObjectData.ImageDataStream, dataInfo.ImageObjectData.ImageOpacity);
            }
            else
            {
                dataInfo.ImageObjectData.ImageFileName = "Default";
                dataInfo.ImageObjectData.ImageDataStream = ImageStreamContorl.ResourceUriToStream((Bitmap)Properties.Resources.ImageDefault);
                graphic.ChageSymbolImage(dataInfo.ImageObjectData.ImageDataStream, dataInfo.ImageObjectData.ImageOpacity);
            }

            this.ObjectDatas.Add(dataInfo);

            return graphic;
        }

        /// <summary>
        /// Address Graphic 만들기
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public IconGraphic MakeAddressGraphic(MapAddressObjectDataInfo dataInfo, bool isEditMode)
        {
            return this.MakeIconGraphic(dataInfo, isEditMode);
        }

        /// <summary>
        /// Icon Graphic 만들기
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        private IconGraphic MakeIconGraphic(MapLocationObjectDataInfo dataInfo, bool isEditMode)
        {
            if (!this.CheckVaidationMakingObject(dataInfo)) return null;

            //Edit Mode 일때는 Icon이 select 상황에서 달라진다.
            var iconSelectedStringUri = isEditMode ? dataInfo.IconSelectedStringUri : dataInfo.IconUri;

            var graphic = new IconGraphic(dataInfo.ExtentMin, dataInfo.IconUri,
                                      iconSelectedStringUri, dataInfo.ObjectType,
                                      dataInfo.ObjectID, dataInfo.IconSize);

            graphic.ZIndexChanged += (s, e) =>
            {
                dataInfo.ObjectZIndex = ((BaseGraphic)s).ZIndex;
            };

            this.ObjectDatas.Add(dataInfo);

            return graphic;
        }

        #endregion Graphic 만들기

        protected void RaiseHomeBookmarkChanged(CollectionChangeEventArgs<MapBookMarkDataInfo> arg)
        {
            var e = this.homeBookmarkChanged;
            if (e != null)
            {
                e(this, arg);
            }
        }

        /// <summary>
        /// Object Data 하나 제거
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DeleteOneObject(BaseMapObjectInfoData data)
        {
            if (data == null) return false;
            
            var objectData = this.objectDatas.FirstOrDefault(item => item.ObjectID == data.ObjectID);

            if (objectData != null) this.ObjectDatas.Remove(objectData);
            
            return true;
        }

        #endregion //Graphic 생성 수정 관련 Methods

        #region Splunk관련

        /// <summary>
        /// 모든 스플렁크 서비스 정지
        /// </summary>
        override public void StopAllSplunkServices()
        {
            foreach (var splunkObjectData in this.objectDatas.OfType<MapLinkZoneObjectDataInfo>().ToArray())
            {
                this.StopSplunkService(splunkObjectData.ObjectID, splunkObjectData.ColorSplunkBasicInformationData);
            }

            foreach (var splunkObjectData in this.objectDatas.OfType<MapWorkStationObjectDataInfo>().ToArray())
            {
                this.StopSplunkService(splunkObjectData.ObjectID, splunkObjectData.SplunkBasicInformation);
            }
        }

        /// <summary>
        /// 스플렁크 서비스 Callback 함수
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="splunkResultSet"></param>
        override protected void SetSplunkCallbackData(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkResultSet splunkResultSet)
        {
            if (splunkResultSet.SplunkDataTable == null) return;

            var dataTable = splunkResultSet.SplunkDataTable;

            var obj = this.objectDatas.FirstOrDefault(item => item.ObjectID == objectId);

            if (!dataTable.Columns.Contains("_IW_COLOR") || dataTable.Rows.Count < 1 || obj == null) return;

            var color = this.StringToSolidBrush(DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_IW_COLOR"]));

            switch (obj.ObjectType)
            {
                case MapObjectType.LinkZone:
                case MapObjectType.ImageLinkZone:
                    this.RaiseColorChangeEvent(objectId, color, obj.ObjectType);
                    break;
                case MapObjectType.Workstation:

                    if (!dataTable.Columns.Contains("_SOFTWARE_COLOR")) return;
                    if (!dataTable.Columns.Contains("_NETWORK_COLOR")) return;
                    if (!dataTable.Columns.Contains("_HARDWARE_COLOR")) return;
                    if (!dataTable.Columns.Contains("_ASSET_HOSTNAME")) return;
                    if (!dataTable.Columns.Contains("_ASSET_IP")) return;
                    if (!dataTable.Columns.Contains("_ASSET_OS")) return;
                    if (!dataTable.Columns.Contains("_ASSET_WINDOWS_EVENT_LOGNAME")) return;
                    if (!dataTable.Columns.Contains("_ASSET_WINDOWS_EVENT_COUNT")) return;
                    if (!dataTable.Columns.Contains("_ASSET_WINDOWS_PERF_OBJECT")) return;
                    if (!dataTable.Columns.Contains("_ASSET_WINDOWS_PERF_COUNTER")) return;

                    var ip = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_IP"]);
                    var hostName = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_HOSTNAME"]);
                    var os = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_OS"]);
                    var windowEventLogName = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_WINDOWS_EVENT_LOGNAME"]);
                    var eventCount = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_WINDOWS_EVENT_COUNT"]);
                    var perfObject = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_WINDOWS_PERF_OBJECT"]);
                    var perfCounter = DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_ASSET_WINDOWS_PERF_COUNTER"]);

                    var networkColor = this.ConvertWorkStationColor(DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_NETWORK_COLOR"]));
                    var softwareColor = this.ConvertWorkStationColor(DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_SOFTWARE_COLOR"]));
                    var hardwareColor = this.ConvertWorkStationColor(DataTypeConverter.ConvertNullableString(dataTable.Rows[0]["_HARDWARE_COLOR"]));

                    var splunkReturnData = new WorkStationReturnedSplunkData()
                    {
                        Color = color as SolidColorBrush,
                        NetworkColor = networkColor,
                        SoftwareColor = softwareColor,
                        HardwareColor = hardwareColor,
                        BrightnessNetWorkColor = this.ConvertBrightnessColor(networkColor),
                        BrightnessSoftWareColor = this.ConvertBrightnessColor(softwareColor),
                        BrightnessHardwareColor = this.ConvertBrightnessColor(hardwareColor),
                        IP = ip,
                        HostName = hostName,
                        OS = os,
                        WindowEventLogName = windowEventLogName,
                        EventCount = eventCount,
                        PerfCounter = perfCounter,
                        PerfObject = perfObject
                    };

                    this.RaiseWorkstationDataChangedEvent(objectId, splunkReturnData);

                    break;
            }
        }

        private object StringToSolidBrush(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString.ToLower() == "none")
            {
                return null;
            }

            return BrushUtil.ConvertFromString(colorString);
        }

       

       

        #region Location

        private void RaiseColorChangeEvent(string id, Object color, MapObjectType type)
        {
            var args = new ColorChangedEventArgs(id, color, false, type);
            var colorChangedEvent = this.eColorChanged;
            if (colorChangedEvent != null)
            {
                colorChangedEvent(this, args);
            }
        }

        #endregion //Location

        #region WorkStation

        /// <summary>
        /// Workstation Color 변경
        /// </summary>
        /// <param name="colorString"></param>
        /// <returns></returns>
        private Color ConvertWorkStationColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString.ToLower() == "none")
            {
                return WorkStationReturnedSplunkData.defaultIconColor;
            }

            return BrushUtil.ConvertFromString(colorString).Color;
        }

        /// <summary>
        /// Workstation Button 색상 변경 될때 gradation 효과 주기
        /// </summary>
        /// <param name="originalColor"></param>
        /// <returns></returns>
        private Color ConvertBrightnessColor(Color originalColor)
        {
            var solidBrush =
                    BrushUtil.SetSaturation(new SolidColorBrush(originalColor), 0.5d) as SolidColorBrush;

            return solidBrush == null ? originalColor : solidBrush.Color;
        }

        /// <summary>
        /// WorkSation Data 변경
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        private void RaiseWorkstationDataChangedEvent(string id, WorkStationReturnedSplunkData data)
        {
            var args = new WorkStationDataChangedEventArgs(id, data);
            var workStationDataChanged = this.eWorkstationDataChanged;
            if (workStationDataChanged != null)
            {
                workStationDataChanged(this, args);
            }
        }

        #endregion //WorkStation

        #endregion Splunk 관련
        

        #region List Data 관련

        public IEnumerable<MapWorkStationObjectDataInfo> GetWorkStationObjectDatas
        {
            get
            {
                return this.objectDatas.OfType<MapWorkStationObjectDataInfo>();
            }
        }

        /// <summary>
        /// LinkZone Data들만 받아온다
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseMapObjectInfoData> GetLinkZoneObjectDatas
        {
            get
            {
                return this.objectDatas.OfType<MapLinkZoneObjectDataInfo>();
            }
        }

        /// <summary>
        /// Location Data들만 받아온다
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseMapObjectInfoData> GetLocationObjectDatas
        {
            get
            {
                 return this.objectDatas.OfType<MapLocationObjectDataInfo>().Where(item=>item.ObjectType == MapObjectType.Location);
            }
        }

        /// <summary>
        /// Address Data들만 받아온다
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseMapObjectInfoData> GetAddressObjectDatas
        {
            get
            {
                return this.objectDatas.OfType<MapLocationObjectDataInfo>().Where(item => item.ObjectType == MapObjectType.Address);
            }
        }

        /// <summary>
        /// BookMark Data들만 받아온다
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MapBookMarkDataInfo> GetBookMarkDatas
        {
            get
            {
                return this.objectDatas.OfType<MapBookMarkDataInfo>();
            }
        }

        public IEnumerable<MapTextObjectDataInfo> GetTextDatas
        {
            get { return this.objectDatas.OfType<MapTextObjectDataInfo>(); }
        }

        /// <summary>
        /// Line Data들만 받아온다
        /// </summary>
        public IEnumerable<MapLineObjectDataInfo> GetLineDatas
        {
            get { return this.objectDatas.OfType<MapLineObjectDataInfo>(); }
        }

        /// <summary>
        /// Image Data들만 받아온다
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseMapObjectInfoData> GetImageDatas
        {
            get { return this.objectDatas.OfType<MapImageObjectDataInfo>(); }
        }

        /// <summary>
        /// 현재 선택된 정보 받아오기
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public BaseMapObjectInfoData GetObjectDataByObjectID(string objectID, MapObjectType objectType)
        {
            try
            {
                return objectType == MapObjectType.None ? this.objectDatas.FirstOrDefault(item => item.ObjectID == objectID) : this.objectDatas.FirstOrDefault(item => item.ObjectID == objectID && item.ObjectType == objectType);
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.Trace(e.ToString());
                return new BaseMapObjectInfoData();
            }
        }

        /// <summary>
        /// Clear Datas
        /// </summary>
        public void ClearObjectDatas()
        {
            if (this.ObjectDatas != null) this.ObjectDatas.Clear();
        }

        #endregion //List Data 관련

        #endregion //Methods

        #region Event Handlers

        private void TextGraphic_PointCollectionChanged(object sender, EventArgs e)
        {
            var graphic = sender as PolygonControlGraphic<TextBoxControl>;
            if (graphic == null)
                return;

            var objectData = this.GetObjectDataByObjectID(graphic.ObjectID, MapObjectType.Text) as MapTextObjectDataInfo;
            if (objectData != null)
            {
                objectData.PointCollection = graphic.PointCollection;
                objectData.TextBoxSize = graphic.Control.DataInfo.TextBoxSize;
            }
        }

        #endregion // Event Handlers

        public MapLineObjectDataInfo DataInfo { get; set; }
    }
}
