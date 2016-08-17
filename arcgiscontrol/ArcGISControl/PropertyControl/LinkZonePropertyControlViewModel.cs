using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using Innotive.SplunkManager.SplunkManager;
using ArcGISControls.CommonData.ServiceHandlers;

namespace ArcGISControl.PropertyControl
{
    public enum SETSPLTYPE
    {
        color,
        table
    }

    public delegate void ApplyLinkZoneSplunkData(MapLinkZoneObjectDataInfo data, SETSPLTYPE setspltype);

    public class LinkZonePropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region events
        
        public event ApplyLinkZoneSplunkData onApplyLinkZoneSplunkData;

        #endregion events 

        #region Fields

        private readonly MapDataServiceHandler dataServiceHandler;

        private Object dataServiceHandlerToken;

        private MapLinkZoneObjectDataInfo dataInfo;

        public MapLinkZoneObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;

                this.dataInfo = value;

                this.IsVisibleImageSplData = dataInfo.ObjectType == MapObjectType.ImageLinkZone ? true : false;
                this.IsVisibleTableSplData = !this.IsVisibleImageSplData;

                this.OnPropertyChanged("DataInfo");

                this.SettingColorSplunkArguments(dataInfo.ColorSplunkBasicInformationData);
                this.SettingTableSplunkArguments(dataInfo.TableSplunkBasicInformationData);
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

        private bool isEnabledPopupSetting = false;
        public bool IsEnabledPopupSetting
        {
            get { return this.isEnabledPopupSetting; }
            set
            {
                this.isEnabledPopupSetting = value;
                OnPropertyChanged("IsEnabledPopupSetting");
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

        private bool isVisibleImageSplData = false;
        public bool IsVisibleImageSplData
        {
            get { return this.isVisibleImageSplData; }
            set
            {
                this.isVisibleImageSplData = value;
                OnPropertyChanged("IsVisibleImageSplData");
            }
        }

        private bool isVisibleTableSplData = false;
        public bool IsVisibleTableSplData
        {
            get { return this.isVisibleTableSplData; }
            set
            {
                this.isVisibleTableSplData = value;
                OnPropertyChanged("IsVisibleTableSplData");
            }
        }

        public Color FillColor
        {
            get { return this.dataInfo.FillColor; }
            set
            {
                if (this.dataInfo.FillColor == value)
                    return;
                this.dataInfo.FillColorString = value.ToString();
                this.OnPropertyChanged("FillColor");
            }
        }

        public Color OutlineColor
        {
            get { return this.dataInfo.BorderColor; }
            set
            {
                if (this.dataInfo.BorderColor == value)
                    return;
                this.dataInfo.BorderColorString = value.ToString();
                this.OnPropertyChanged("OutlineColor");
            }
        }

        private List<MapSettingDataInfo> mapSettingInfoDatas;
        public List<MapSettingDataInfo> MapSettingInfoDatas
        {
            get { return mapSettingInfoDatas; }
            set
            {
                this.mapSettingInfoDatas = value;
                OnPropertyChanged("MapSettingInfoDatas");

                if (this.mapSettingInfoDatas == null)
                {
                    this.IsEnabledLinkedMap = false;
                    this.IsEnabledPopupSetting = false;
                }
                else
                {
                    this.IsEnabledLinkedMap = true;
                    this.IsEnabledPopupSetting = true;                    
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
                    if (data == null || data.Name.ToLower() == "none")
                    {
                        this.dataInfo.LinkedMapGuid = null;
                    }
                    else
                    {
                        this.dataInfo.LinkedMapGuid = data.ID;
                    }
                    this.BeginGetBookMarkList(this.dataInfo.LinkedMapGuid);
                }
            }
        }

        private List<MapBookMarkDataInfo> bookmarkList;
        public List<MapBookMarkDataInfo> BookmarkList
        {
            get { return this.bookmarkList; }
            set
            {
                if (this.bookmarkList == value)
                    return;

                this.bookmarkList = value;
                OnPropertyChanged("BookMarkList");

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

                if (value == null || value.Name.ToLower() == "none")
                {
                    this.dataInfo.LinkedMapBookmarkName = null;
                }
                else
                {
                    this.dataInfo.LinkedMapBookmarkName = value.Name;
                }
            }
        }

        private List<SplunkBasicInformationData> splunkInformationDatas;
        public List<SplunkBasicInformationData> SplunkInformationDatas
        {
            get { return this.splunkInformationDatas; }
            set
            {
                splunkInformationDatas = value;
                OnPropertyChanged("SplunkInformationDatas");

                if (this.dataInfo.ColorSplunkBasicInformationData.Name != null)
                {
                    var selectedData = this.splunkInformationDatas.FirstOrDefault(item => item.Name == this.dataInfo.ColorSplunkBasicInformationData.Name);
                    this.selectedColorSplunkInformationDataIndex = this.splunkInformationDatas.IndexOf(selectedData);
                }

                if (this.dataInfo.TableSplunkBasicInformationData.Name != null)
                {
                    var selectedData = this.splunkInformationDatas.FirstOrDefault(item => item.Name == this.dataInfo.TableSplunkBasicInformationData.Name);
                    this.selectedTableSplunkInformationDataIndex = this.splunkInformationDatas.IndexOf(selectedData);
                }   
            }
        }

        private int selectedColorSplunkInformationDataIndex;
        public int SelectedColorSplunkInformationDataIndex
        {
            get { return this.selectedColorSplunkInformationDataIndex; }
            set
            {
                this.selectedColorSplunkInformationDataIndex = value;
                OnPropertyChanged("SelectedColorSplunkInformationDataIndex");

                if (this.splunkInformationDatas != null &&
                    this.splunkInformationDatas.Count > this.selectedColorSplunkInformationDataIndex &&
                    this.selectedColorSplunkInformationDataIndex >= 0)
                {
                    var data = this.splunkInformationDatas[this.selectedColorSplunkInformationDataIndex];
                    this.SettingColorSplunkArguments(data);
                }
            }
        }

        private ObservableCollection<SplunkArgumentItem> splunkColorArgumentItems = new ObservableCollection<SplunkArgumentItem>();
        public ObservableCollection<SplunkArgumentItem> SplunkColorArgumentItems
        {
            get { return this.splunkColorArgumentItems; }
            set
            {
                this.splunkColorArgumentItems = value;
                OnPropertyChanged("SplunkColorArgumentItems");
            }
        }

        private int selectedTableSplunkInformationDataIndex;
        public int SelectedTableSplunkInformationDataIndex
        {
            get { return this.selectedTableSplunkInformationDataIndex; }
            set
            {
                this.selectedTableSplunkInformationDataIndex = value;
                OnPropertyChanged("SelectedTableSplunkInformationDataIndex");

                if (this.splunkInformationDatas != null &&
                    this.splunkInformationDatas.Count > this.selectedTableSplunkInformationDataIndex &&
                    this.selectedTableSplunkInformationDataIndex >= 0 )
                {
                    var data = this.splunkInformationDatas[this.selectedTableSplunkInformationDataIndex];
                    this.SettingTableSplunkArguments(data);
                }
            }
        }

        private ObservableCollection<SplunkArgumentItem> splunkTableArgumentItems = new ObservableCollection<SplunkArgumentItem>();
        public ObservableCollection<SplunkArgumentItem> SplunkTableArgumentItems
        {
            get { return this.splunkTableArgumentItems; }
            set
            {
                this.splunkTableArgumentItems = value;
                OnPropertyChanged("SplunkTableArgumentItems");
            }
        }

        private bool useSplunk;
        public bool UseSplunk
        {
            get { return this.useSplunk; }
            set
            {
                this.useSplunk = value;
                OnPropertyChanged("UseSplunk");
            }
        }

        public double ImageOpacity
        {
            get { return this.dataInfo.ImageObjectData.ImageOpacity; }
            set
            {   
                this.dataInfo.ImageObjectData.ImageOpacity = value;
                
                OnPropertyChanged("ImageOpacity");
            }
        }

        public bool IsPopupData
        {
            get { return !this.dataInfo.ShouldShowBrowserOnClick; }
            set
            {
                this.dataInfo.ShouldShowBrowserOnClick = !value;
                this.OnPropertyChanged("IsPopupData");
            }
        }

        public bool IsBrowser
        {
            get { return this.dataInfo.ShouldShowBrowserOnClick; }
            set
            {
                this.dataInfo.ShouldShowBrowserOnClick = value;
                this.OnPropertyChanged("IsBrowser");
            }
        }

        #endregion //Fieds

        #region Constructors

        public LinkZonePropertyControlViewModel(MapDataServiceHandler dataServiceHandler)
        {
            this.dataServiceHandler = dataServiceHandler;
        }

        #endregion Constructors

        #region Methods

        private void SettingColorSplunkArguments(SplunkBasicInformationData data)
        {
            if (!this.dataInfo.ColorSplunkBasicInformationData.IsSameSplunkService(data)) this.dataInfo.ColorSplunkBasicInformationData = data;

            this.SplunkColorArgumentItems.Clear();

            for (int i = 0; i < data.SplArgumentKeys.Count; i++)
            {
                this.SplunkColorArgumentItems.Add
                    (
                        new SplunkArgumentItem()
                        {
                            SplunkArgumentKey = data.SplArgumentKeys.ElementAt(i),
                            SplunkArgumentValue = data.SplArgumentValues.ElementAt(i)
                        }
                    );
            }
        }

        private void SettingTableSplunkArguments(SplunkBasicInformationData data)
        {
            if (!this.dataInfo.TableSplunkBasicInformationData.IsSameSplunkService(data)) this.dataInfo.TableSplunkBasicInformationData = data;

            this.SplunkTableArgumentItems.Clear();

            for (int i = 0; i < data.SplArgumentKeys.Count; i++)
            {
                this.SplunkTableArgumentItems.Add
                    (
                        new SplunkArgumentItem()
                        {
                            SplunkArgumentKey = data.SplArgumentKeys.ElementAt(i),
                            SplunkArgumentValue = data.SplArgumentValues.ElementAt(i)
                        }
                    );
            }
        }

        private void BeginGetBookMarkList(string linkedMapGuid)
        {
            this.BookmarkList = null;

            if (linkedMapGuid == null)
            {
                return;
            }

            this.dataServiceHandler.eGetMapDataProcessCompleted += this.dataServiceHandler_eGetMapDataProcessCompleted;
            this.dataServiceHandlerToken = new Object();
            this.dataServiceHandler.StartGetMapFeatureData(linkedMapGuid, this.dataServiceHandlerToken, "BookMark");
        }

        private void dataServiceHandler_eGetMapDataProcessCompleted(object sender, GetMapDataProcessCompletedEventArgs e)
        {
            this.dataServiceHandler.eGetMapDataProcessCompleted -= this.dataServiceHandler_eGetMapDataProcessCompleted;

            if (e.IsCompleted)
            {
                if (!Object.ReferenceEquals(this.dataServiceHandler.CompletedToken, this.dataServiceHandlerToken))
                    return;

                var list = new List<MapBookMarkDataInfo>();
                list.Add(new MapBookMarkDataInfo { Name = "None" });

                foreach (var bookMarkDataInfo in this.dataServiceHandler.CameraObjectComponentDataInfos.OfType<MapBookMarkDataInfo>())
                {
                    list.Add(bookMarkDataInfo);
                }

                this.BookmarkList = list;
            }
        }

        #endregion Methods

        #region Load FIle Button Command

        private RelayCommand applyColorSPLCommand;
        public ICommand ApplyColorSPLCommand
        {
            get
            {
                return this.applyColorSPLCommand ??
                       (this.applyColorSPLCommand =
                        new RelayCommand(param => this.ApplyColorSPL(), param2 => this.CanApplyColrolSPL()));
            }
        }

        private void ApplyColorSPL()
        {
            if (this.onApplyLinkZoneSplunkData != null) this.onApplyLinkZoneSplunkData(this.dataInfo, SETSPLTYPE.color);
        }

        private bool CanApplyColrolSPL()
        {
            return this.dataInfo.ColorSplunkBasicInformationData != null && !string.IsNullOrEmpty(this.dataInfo.ColorSplunkBasicInformationData.Name) &&
                this.dataInfo.ColorSplunkBasicInformationData.Name.ToLower() != "none";
        }

        private RelayCommand applyTableSPLCommand;
        public ICommand ApplyTableSPLCommand
        {
            get
            {
                return this.applyTableSPLCommand ??
                       (this.applyTableSPLCommand =
                        new RelayCommand(param => this.ApplyTableSPL(), param2 => this.CanApplyTableSPL()));
            }
        }

        private void ApplyTableSPL()
        {
            if (this.onApplyLinkZoneSplunkData != null) this.onApplyLinkZoneSplunkData(this.dataInfo, SETSPLTYPE.table);
        }

        private bool CanApplyTableSPL()
        {
            return this.dataInfo.TableSplunkBasicInformationData != null && !string.IsNullOrEmpty(this.dataInfo.TableSplunkBasicInformationData.Name) &&
                this.dataInfo.TableSplunkBasicInformationData.Name.ToLower() != "none";
        }

        #endregion //Load FIle Button Command

        #region File Dialog Command

        private RelayCommand importImagesButtonCommand;
        public ICommand ImportImagesButtonCommand
        {
            get
            {
                return this.importImagesButtonCommand ??
                       (this.importImagesButtonCommand =
                        new RelayCommand(param => this.FileBrowserDialogOpen(), null));
            }
        }

        /// <summary>
        /// File Browser Dialog Open
        /// </summary>
        /// <returns></returns>
        private void FileBrowserDialogOpen()
        {
            var dlg = new OpenFileDialog { FileName = "", Filter = GetImageFilter(), Multiselect = false };

            var result = dlg.ShowDialog();


            if (result.ToString().ToLower() == "ok")
            {
                var filename = dlg.FileName;
                var replaceFilename = filename.Replace("\\", "/");

                try
                {
                    dataInfo.ImageObjectData.ImageDataStream = ImageStreamContorl.FilePathToString(filename);

                    dataInfo.ImageObjectData.ImageFileName = replaceFilename.Split('/').Last();
                }
                catch (IOException ex)
                {
                    InnowatchDebug.Logger.WriteLine(ex.ToString());
                }


            }
        }

        /// <summary>
        /// Get All Image File Extent Format List
        /// </summary>
        /// <returns></returns>
        private string GetImageFilter()
        {
            var allImageExtensions = new StringBuilder();
            string separator = "";
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            var images = new Dictionary<string, string>();

            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg" || codec.MimeType == "image/bmp" || codec.MimeType == "image/gif" || codec.MimeType == "image/png")
                {
                    allImageExtensions.Append(separator);
                    allImageExtensions.Append(codec.FilenameExtension);
                    separator = ";";
                    images.Add(string.Format("{0} Files: ({1})", codec.FormatDescription, codec.FilenameExtension),
                               codec.FilenameExtension);
                }
            }

            var sb = new StringBuilder();

            var isFirst = true;

            foreach (KeyValuePair<string, string> image in images)
            {
                if (!isFirst) sb.AppendFormat("|");
                sb.AppendFormat("{0}|{1}", image.Key, image.Value);
                if (isFirst) isFirst = false;
            }

            return sb.ToString();
        }

        #endregion //File Dialog  Command
    }
}
