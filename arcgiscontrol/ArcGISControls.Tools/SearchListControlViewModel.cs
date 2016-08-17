using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArcGISControl;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.Tools
{
    public class SearchListControlViewModel : BaseModel
    {
        #region Field

        private bool isFirstSetting = true;

        private ObservableCollection<MapAddressObjectDataInfo> mapAddressObjectDataInfos;
        public ObservableCollection<MapAddressObjectDataInfo> MapAddressObjectDataInfos
        {
            get { return this.mapAddressObjectDataInfos; }
            set
            {
                this.mapAddressObjectDataInfos = value;
                OnPropertyChanged("MapAddressObjectDataInfos");
            }
        }

        //리스트에서 선택 할때와 Search 하고 난후에만 Goto 한다.
        private bool doGotoLocation = true;

        private MapAddressObjectDataInfo selectedItem;
        public MapAddressObjectDataInfo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                this.selectedItem = value;
                OnPropertyChanged("SelectedItem");

                if (value != null)
                {
                    this.arcGisClientViewer.SelectObject(value);
                    if (this.doGotoLocation) this.arcGisClientViewer.GoToLocation(value);
                }

                this.doGotoLocation = true;
            }
        }

        private ArcGISClientViewer arcGisClientViewer;
        public ArcGISClientViewer ArcGisClientViewer
        {
            set
            {
                this.arcGisClientViewer = value;

                if (arcGisClientViewer != null)
                {
                    if (isFirstSetting)
                    {
                        this.arcGisClientViewer.eSearchedAddressSaveButtonClick += ArcGisClientViewer_eSearchedAddressSaveButtonClick; 
                        this.arcGisClientViewer.eObjectSelected += ArcGisClientViewer_eObjectSelected;  
                    }

                    this.MapAddressObjectDataInfos = this.arcGisClientViewer.SearchedAddressDatas;

                    isFirstSetting = false;
                }

                OnPropertyChanged("ArcGISClientViewer");
            }
        }

        private string searchText;
        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        private string searchedCounts;
        public string SearchedCounts
        {
            get { return this.searchedCounts; }
            set
            {
                this.searchedCounts = value;
                OnPropertyChanged("SearchedCounts");
            }
        }

        private bool isEditMode;
        public bool IsEditMode
        {
            get { return this.isEditMode; }
            set
            {
                this.isEditMode = value;
                OnPropertyChanged("IsEditMode");
            }
        }

        #endregion Field

        #region Methods

        public void SaveMapAddressObjectDataInfo(MapAddressObjectDataInfo mapAddressObjectDataInfo)
        {
            this.arcGisClientViewer.SaveSearchedAddressObject(mapAddressObjectDataInfo);
        }

        public void DeleteSavedMapAddressObjectDataInfo(MapAddressObjectDataInfo mapAddressObjectDataInfo)
        {
            this.arcGisClientViewer.DeleteMapObjectData(new MapAddressObjectDataInfo(mapAddressObjectDataInfo)
                                                            {
                                                                ObjectType = MapObjectType.Address
                                                            });
        }

        public override void Dispose()
        {
            base.Dispose();

            if(this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.eSearchedAddressSaveButtonClick -= ArcGisClientViewer_eSearchedAddressSaveButtonClick;
                this.arcGisClientViewer.eObjectSelected -= ArcGisClientViewer_eObjectSelected;  
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Map Control 에서 Searched Address ICon 을 Saved Icon으로 저장 할때 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objectEventArgs"></param>
        private void ArcGisClientViewer_eSearchedAddressSaveButtonClick(object sender, ObjectEventArgs objectEventArgs)
        {
            var unigridGuid = objectEventArgs.Id;

            var mapAddressObjectDataInfo = this.mapAddressObjectDataInfos.Cast<MapAddressObjectDataInfo>().FirstOrDefault(item => item.ObjectID == unigridGuid);
            if (mapAddressObjectDataInfo != null) this.arcGisClientViewer.SaveSearchedAddressObject(mapAddressObjectDataInfo);
        }

        /// <summary>
        /// Map Control 에서 Object 가 하나 추가되었을때 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArcGisClientViewer_eObjectSelected(object sender, SelectedObjectEventArgs e)
        {
            if (!e.SelectedGraphicList.Any())
            {
                return;
            }

            var unigridGuid = e.SelectedGraphicList.First().ObjectId;
            var mapObjectType = e.SelectedGraphicList.First().Type;

            if (this.MapAddressObjectDataInfos == null)
            {
                return;
            }

            if (this.SelectedItem != null &&
                (this.SelectedItem == null ||
                 this.SelectedItem.ObjectID == unigridGuid && SelectedItem.ObjectType == mapObjectType))
            {
                return;
            }

            this.doGotoLocation = false;

            this.SelectedItem = this.MapAddressObjectDataInfos.Cast<MapAddressObjectDataInfo>().FirstOrDefault(data => data.ObjectID == unigridGuid && data.ObjectType == mapObjectType);
        }

        #endregion Event Handlers

        #region Commands

        private RelayCommand buttonSearchGeoCodingCommand;
        public ICommand ButtonSearchGeoCodingCommand
        {
            get
            {
                return this.buttonSearchGeoCodingCommand ??
                       (this.buttonSearchGeoCodingCommand = new RelayCommand(param => this.SearchGeoCoding(), param => this.CanSearchGeoCoding()));
            }
        }

        public void SearchGeoCoding()
        {   
            try
            {
                if (this.CanSearchGeoCoding())
                {
                    this.SelectedItem = null;

                    this.arcGisClientViewer.SearchMapGeoCoding(this.searchText);

                    this.SearchedCounts = this.MapAddressObjectDataInfos != null ? this.MapAddressObjectDataInfos.Count.ToString(CultureInfo.InvariantCulture) : "0";

                    if (this.MapAddressObjectDataInfos != null && this.MapAddressObjectDataInfos.Count > 0)
                    {
                        this.doGotoLocation = true;

                        this.SelectedItem = this.MapAddressObjectDataInfos[0] as MapAddressObjectDataInfo;
                    }
                }
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }
        }

        private bool CanSearchGeoCoding()
        {
            return !string.IsNullOrEmpty(this.searchText);
        }

        #endregion Commands
    }
}
