using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.PropertyControl
{
    public class UniversalObjectPropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private readonly MapDataServiceHandler dataServiceHandler;

        private Object dataServiceHandlerToken;

        private MapUniversalObjectDataInfo dataInfo;

        private string noneLabelForComboBox;

        public MapUniversalObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;

                this.dataInfo = value;

                this.OnPropertyChanged("DataInfo");
            }
        }

        private bool isSingleSetting = true;

        public bool IsSingleSetting
        {
            get { return this.isSingleSetting; }
            set
            {
                this.isSingleSetting = value;
                OnPropertyChanged("IsSingleSetting");
            }
        }

        public Color TitleColor
        {
            get
            {
                if (dataInfo == null)
                {
                    return Colors.White;
                }

                var color = ColorConverter.ConvertFromString(DataInfo.TitleColor);
                if (color == null)
                {
                    return Colors.White;
                }

                return (Color)color;
            }
            set
            {
                this.dataInfo.TitleColor = value.ToString();
                this.OnPropertyChanged("TitleColor");
            }
        }

        public Color FillColor
        {
            get
            {
                if (dataInfo == null)
                {
                    return Colors.White;
                }

                var color = ColorConverter.ConvertFromString(DataInfo.FillColor);
                if (color == null)
                {
                    return Colors.White;
                }

                return (Color)color;
            }
            set
            {
                this.dataInfo.FillColor = value.ToString();
                this.OnPropertyChanged("TitleColor");
            }
        }

        public Color BorderColor
        {
            get
            {
                if (dataInfo == null)
                {
                    return Colors.White;
                }

                var color = ColorConverter.ConvertFromString(DataInfo.BorderColor);
                if (color == null)
                {
                    return Colors.White;
                }

                return (Color)color;
            }
            set
            {
                this.dataInfo.BorderColor = value.ToString();
                this.OnPropertyChanged("TitleColor");
            }
        }

        public Color LampColor
        {
            get
            {
                if (dataInfo == null)
                {
                    return Colors.White;
                }

                var color = ColorConverter.ConvertFromString(DataInfo.AlarmLampColor);
                if (color == null)
                {
                    return Colors.White;
                }

                return (Color)color;
            }
            set
            {
                this.dataInfo.AlarmLampColor = value.ToString();
                this.OnPropertyChanged("TitleColor");
            }
        }

        private bool isEnabledLinkedMap = false;
        public bool IsEnabledLinkedMap
        {
            get { return this.isEnabledLinkedMap; }
            set
            {
                this.isEnabledLinkedMap = value;
                OnPropertyChanged("IsEnabledLinkedMap");
            }
        }

        private bool isEnabledLinkedMapBookMark = false;
        public bool IsEnabledLinkedMapBookMark
        {
            get { return this.isEnabledLinkedMapBookMark; }
            set
            {
                this.isEnabledLinkedMapBookMark = value;
                OnPropertyChanged("IsEnabledLinkedMapBookMark");
            }
        }

        private bool isEnabledLinkedMapObject = false;
        public bool IsEnabledLinkedMapObject
        {
            get { return this.isEnabledLinkedMapObject; }
            set
            {
                this.isEnabledLinkedMapObject = value;
                OnPropertyChanged("IsEnabledLinkedMapObject");
            }
        }

        private List<MapSettingDataInfo> mapSettingInfoDatas;
        public List<MapSettingDataInfo> MapSettingInfoDatas
        {
            get { return mapSettingInfoDatas; }
            set
            {
                value.Insert(0, new MapSettingDataInfo() { Name = this.noneLabelForComboBox });

                this.mapSettingInfoDatas = value;
                OnPropertyChanged("MapSettingInfoDatas");

                if (this.mapSettingInfoDatas == null)
                {
                    this.IsEnabledLinkedMap = false;
                }
                else
                {
                    this.IsEnabledLinkedMap = true;
                }

                if (this.dataInfo != null && string.IsNullOrEmpty(this.dataInfo.LinkedMapGuid))
                {
                    this.SelectedLinkedMapDataIndex = 0;
                }
            }
        }

        private int selectedLinkedMapDataIndex;
        public int SelectedLinkedMapDataIndex
        {
            get { return this.selectedLinkedMapDataIndex; }
            set
            {
                this.selectedLinkedMapDataIndex = value;
                OnPropertyChanged("SelectedLinkedMapDataIndex");

                if (this.MapSettingInfoDatas != null &&
                    this.MapSettingInfoDatas.Count > this.selectedLinkedMapDataIndex &&
                    this.selectedLinkedMapDataIndex >= 0)
                {
                    var data = this.MapSettingInfoDatas[this.selectedLinkedMapDataIndex];
                    if (data == null || data.Name == this.noneLabelForComboBox)
                    {
                        this.dataInfo.LinkedMapGuid = null;
                    }
                    else
                    {
                        this.dataInfo.LinkedMapGuid = data.ID;
                    }
                    this.BeginGetMapFeatureList(this.dataInfo.LinkedMapGuid);
                }
            }
        }

        private List<MapBookMarkDataInfo> bookMarkList;
        public List<MapBookMarkDataInfo> BookMarkList
        {
            get { return this.bookMarkList; }
            set
            {
                if (this.bookMarkList == value)
                    return;

                this.bookMarkList = value;
                this.OnPropertyChanged("BookMarkList");

                if (value == null || !value.Any())
                {
                    this.dataInfo.LinkedMapBookmarkName = null;
                    this.IsEnabledLinkedMapBookMark = false;
                }
                else
                {
                    this.SelectedBookmark = value.FirstOrDefault(bm => bm.Name.Equals(this.dataInfo.LinkedMapBookmarkName)) ?? value[0];
                    this.IsEnabledLinkedMapBookMark = true;
                }
            }
        }

        private MapBookMarkDataInfo selectedBookmark;
        public MapBookMarkDataInfo SelectedBookmark
        {
            get { return this.selectedBookmark; }
            set
            {
                this.selectedBookmark = value;
                OnPropertyChanged("SelectedBookmark");

                if (value == null || value.Name == this.noneLabelForComboBox)
                {
                    this.dataInfo.LinkedMapBookmarkName = null;
                }
                else
                {
                    this.dataInfo.LinkedMapBookmarkName = value.Name;
                    this.SelectedObject = this.ObjectList == null || !this.ObjectList.Any() ? null : this.ObjectList[0];
                }
            }
        }

        private List<BaseMapObjectInfoData> objectList;
        public List<BaseMapObjectInfoData> ObjectList
        {
            get { return this.objectList; }
            set
            {
                if (this.objectList == value)
                    return;

                this.objectList = value;
                this.OnPropertyChanged("ObjectList");

                if (value == null || !value.Any())
                {
                    this.dataInfo.LinkedMapBookmarkName = null;
                    this.IsEnabledLinkedMapObject = false;
                }
                else
                {
                    this.SelectedObject = value.FirstOrDefault(bm => bm.Name.Equals(this.dataInfo.LinkedMapObjectName)) ?? value[0];
                    this.IsEnabledLinkedMapObject = true;
                }
            }
        }

        private BaseMapObjectInfoData selectedObject;
        public BaseMapObjectInfoData SelectedObject
        {
            get { return this.selectedObject; }
            set
            {
                this.selectedObject = value;
                this.OnPropertyChanged("SelectedObject");

                if (value == null || value.Name == this.noneLabelForComboBox)
                {
                    this.dataInfo.LinkedMapObjectName = null;
                }
                else
                {
                    this.dataInfo.LinkedMapObjectName = value.Name;
                    this.SelectedBookmark = this.bookMarkList == null || !this.bookMarkList.Any() ? null : this.bookMarkList[0];
                }
            }
        }

        public UniversalObjectPropertyControlViewModel(MapDataServiceHandler dataServiceHandler)
        {
            this.dataServiceHandler = dataServiceHandler;

            if (System.Threading.Thread.CurrentThread.Name == "ko_KR")
            {
                noneLabelForComboBox = " 선택안함";
            }
            else
            {
                noneLabelForComboBox = " None";
            }
        }

        private void BeginGetMapFeatureList(string linkedMapGuid)
        {
            this.BookMarkList = null;
            this.ObjectList = null;

            if (linkedMapGuid == null)
            {
                return;
            }

            this.dataServiceHandler.eGetMapDataProcessCompleted += this.dataServiceHandler_eGetMapDataProcessCompleted;
            this.dataServiceHandlerToken = new Object();
            this.dataServiceHandler.StartGetMapFeatureData(linkedMapGuid, this.dataServiceHandlerToken, null);
        }

        private void dataServiceHandler_eGetMapDataProcessCompleted(object sender, GetMapDataProcessCompletedEventArgs e)
        {
            this.dataServiceHandler.eGetMapDataProcessCompleted -= this.dataServiceHandler_eGetMapDataProcessCompleted;

            if (e.IsCompleted)
            {
                if (!Object.ReferenceEquals(this.dataServiceHandler.CompletedToken, this.dataServiceHandlerToken))
                    return;

                var bookmarkList = new List<MapBookMarkDataInfo>();
                
                var objList = new List<BaseMapObjectInfoData>();

                bookmarkList.Insert(0, new MapBookMarkDataInfo { Name = this.noneLabelForComboBox });
                objList.Insert(0, new BaseMapObjectInfoData { Name = this.noneLabelForComboBox });
                

                foreach (var dataInfo in this.dataServiceHandler.CameraObjectComponentDataInfos)
                {
                    if (dataInfo.ObjectType == MapObjectType.BookMark)
                    {
                        bookmarkList.Add((MapBookMarkDataInfo)dataInfo);
                    }
                    else
                    {
                        objList.Add(dataInfo);
                    }
                    
                }

                this.BookMarkList = bookmarkList;
                this.ObjectList = objList;
            }
        }
    }
}
