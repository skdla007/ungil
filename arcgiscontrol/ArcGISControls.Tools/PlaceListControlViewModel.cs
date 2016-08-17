using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using ArcGISControl;
using ArcGISControl.Bases;
using ArcGISControl.Collections;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using DataChangedNotify;

namespace ArcGISControls.Tools
{
    public class PlaceListControlViewModel : BaseModel
    {
        #region Field

        protected CancellationTokenSource cancellationTokenSource;
        protected CancellationToken token;

        private bool isFirstSetting = true;

        private ExtendedObservableCollection<PlaceListItemModel> placeListItems = new ExtendedObservableCollection<PlaceListItemModel>();
        public ExtendedObservableCollection<PlaceListItemModel> PlaceListItems
        {
            get { return this.placeListItems; }
            set
            {
                this.placeListItems = value;
                OnPropertyChanged("PlaceListItems");
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
                        this.arcGisClientViewer.eObjectSelected +=ArcGisClientViewer_eObjectSelected; 
                        this.arcGisClientViewer.eObjectDeleted +=ArcGisClientViewer_eObjectDeleted;
                        this.arcGisClientViewer.eObjectAdded +=ArcGisClientViewer_eObjectAdded;
                    }

                    this.RefreshPlaceList();

                    this.isFirstSetting = false;
                }
            }
            get { return arcGisClientViewer; }
        }



        private bool isEditMode;
        public bool IsEditMode
        {
            get { return isEditMode; }
            set
            {
                this.isEditMode = value;
                OnPropertyChanged("IsEditMode");
            }
        }

        #endregion //Field

        #region Construction

        public PlaceListControlViewModel(bool isEditMode)
        {
            this.IsEditMode = isEditMode;
        }

        #endregion //Construction

        #region EventHandlers

        /// <summary>
        /// Map Control 에서 Object 선택 되었을때 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArcGisClientViewer_eObjectSelected(object sender, SelectedObjectEventArgs e)
        {
            var idList = e.SelectedGraphicList.Select(item => item.ObjectId);

            foreach (var item in this.PlaceListItems)
            {
                item.IsSelected = idList.Contains(item.ObjectData.ObjectID);
            }
        }

        /// <summary>
        /// Map Control 에서 Object 삭제 되었을때 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArcGisClientViewer_eObjectDeleted(object sender, ObjectEventArgs e)
        {
            var unigridGuid = e.Id;
            var mapObjectType = e.Type;

            var item =
                  this.PlaceListItems.FirstOrDefault(
                      data => data.ObjectData.ObjectID == unigridGuid && data.ObjectData.ObjectType == mapObjectType);

            if (item != null) this.PlaceListItems.Remove(item);
        }

        /// <summary>
        /// Map Control 에서 Object 가 하나 추가되었을때 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArcGisClientViewer_eObjectAdded(object sender, ObjectDataEventArgs e)
        {
            var mapObjectDataInfo = e.mapObjectDataInfo;

            this.DeselectAll();

            this.PlaceListItems.Add(new PlaceListItemModel { ObjectData = mapObjectDataInfo, IsSelected = true });

            this.arcGisClientViewer.SelectObject(mapObjectDataInfo);
        }

        #endregion EventHanders

        #region Method

        public void SelectItems(List<PlaceListItemModel> items)
        {
            if (items.Count == 0)
            {
                this.arcGisClientViewer.SelectObjects(null);
            }

            var selectedItemList = items.Select(item => item.ObjectData).ToList();

            this.arcGisClientViewer.SelectObjects(selectedItemList);
        }

        public void RefreshPlaceList()
        {
            if (this.arcGisClientViewer != null)
            {
                if (this.cancellationTokenSource != null && this.token != null &&
                    this.token.IsCancellationRequested) this.cancellationTokenSource.Cancel();

                this.cancellationTokenSource = new CancellationTokenSource();
                this.token = cancellationTokenSource.Token;

                var GetDataTask = Task.Factory.StartNew(
                    new Action(() => Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        var cameraPlaceList = this.arcGisClientViewer.CameraList.Select(camera => new PlaceListItemModel { ObjectData = camera, IsSelected = false });
                        var locationPlaceList = this.arcGisClientViewer.PublicLocationList.Select(location => new PlaceListItemModel { ObjectData = location, IsSelected = false });
                        var savedSplunkPlaceList = this.arcGisClientViewer.SavedSplunkList.Select(splunk => new PlaceListItemModel { ObjectData = splunk, IsSelected = false });
                        var universalObjectPlaceList = this.arcGisClientViewer.UniversalDataInfoList.Select(universal => new PlaceListItemModel { ObjectData = universal, IsSelected = false });
                        this.PlaceListItems.ReplaceAll(cameraPlaceList.Concat(locationPlaceList).Concat(savedSplunkPlaceList).Concat(universalObjectPlaceList));
                    }
                        ))), token);

                GetDataTask.ContinueWith(task =>
                {
                    this.DeselectAll();
                    this.cancellationTokenSource.Dispose();
                }, token);

                this.DeselectAll();
            }
        }

        public void ClearPlaceList()
        {
            if (this.token.IsCancellationRequested) this.cancellationTokenSource.Cancel();

            Application.Current.Dispatcher.BeginInvoke(new Action(() => this.PlaceListItems.Clear()));
        }

        public void DeleteMapObjectData(PlaceListItemModel item)
        {
            var bookmarkData = item.ObjectData as MapBookMarkDataInfo;
            if (bookmarkData != null && bookmarkData.IsHome)
            {
                this.arcGisClientViewer.ClearHome();
            }
            else
            {
                this.arcGisClientViewer.DeleteMapObjectData(item.ObjectData);
            }
        }

        public void GotoItem(PlaceListItemModel item)
        {
            this.arcGisClientViewer.GoToLocation(item.ObjectData);
            this.arcGisClientViewer.SelectObject(item.ObjectData);
        }

        public override void Dispose()
        {
            if (this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.eObjectSelected -= ArcGisClientViewer_eObjectSelected;
                this.arcGisClientViewer.eObjectDeleted -= ArcGisClientViewer_eObjectDeleted;
                this.arcGisClientViewer.eObjectAdded -= ArcGisClientViewer_eObjectAdded;
                this.arcGisClientViewer = null;
            }

            this.PlaceListItems.Clear();
        }

        /// <summary>
        /// 리스트의 모든 선택을 해제한다.
        /// </summary>
        private void DeselectAll()
        {
            foreach (var item in this.PlaceListItems)
            {
                item.IsSelected = false;
            }
        }

        #endregion //Method

        #region Command

        #endregion //Command
    }
}
