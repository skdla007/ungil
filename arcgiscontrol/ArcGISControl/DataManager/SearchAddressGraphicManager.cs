using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ArcGISControl.GraphicObject;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.DataManager
{   
    public class SearchAddressGraphicManager
    {
        #region Field

        protected ObservableCollection<MapAddressObjectDataInfo> objectDatas;

        public ObservableCollection<MapAddressObjectDataInfo> SearchAddressObjectDatas
        {
            get { return this.objectDatas as ObservableCollection<MapAddressObjectDataInfo>; }
        }

        #endregion //Field

        #region Construction

        public SearchAddressGraphicManager()
        {
            this.objectDatas = new ObservableCollection<MapAddressObjectDataInfo>();
        }

        #endregion //Construction

        #region Methods

        #region Graphic 생성 수정 관련 Methods

        /// <summary>
        /// Object Data 하나 추가
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isEditMode"></param>
        /// <returns></returns>
        public IEnumerable<SearchedAddressIconGraphic> MakeOneObjectGraphics(MapAddressObjectDataInfo data, bool isEditMode)
        {
            if (string.IsNullOrEmpty(data.ObjectID))
            {
                data.ObjectID = Guid.NewGuid().ToString();
            }

            if (this.objectDatas.Any(item => item.ObjectID == data.ObjectID)) return null;

            this.SearchAddressObjectDatas.Add(data);

            return null;
        }

        #endregion //Graphic 생성 수정 관련 Methods

        #region List Data 관련

        /// <summary>
        /// Search Map
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="mapType"></param>
        /// <param name="licenseKey"></param>
        /// <returns></returns>
        public List<SearchedAddressIconGraphic> SearchMapGeoCoding(string searchText, MapProviderType mapType, string licenseKey)
        {
            var searchedAddressIconGraphics = new List<SearchedAddressIconGraphic>();
            var searchedAddressObjecDatas = MapSearchGeocoding.GetMapSearchDataList(searchText, mapType, licenseKey);

            this.SearchAddressObjectDatas.Clear();

            foreach (var searchedAddressObjecData in searchedAddressObjecDatas)
            {
                this.SearchAddressObjectDatas.Add(searchedAddressObjecData);

                if (ArcGISDataConvertHelper.IsGISMapType(mapType)) GeometryHelper.FromGeographicObjectPosition(searchedAddressObjecData, mapType);

                if (string.IsNullOrEmpty(searchedAddressObjecData.ObjectID)) searchedAddressObjecData.ObjectID = Guid.NewGuid().ToString();

                var addressGraphic 
                    = new SearchedAddressIconGraphic(searchedAddressObjecData.Position, searchedAddressObjecData.ObjectType, searchedAddressObjecData.ObjectID, searchedAddressObjecData.SearchedIndexLabel);

                searchedAddressIconGraphics.Add(addressGraphic);
            }

            return searchedAddressIconGraphics;
        }

        /// <summary>
        /// 현재 선택된 정보 받아오기
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public MapAddressObjectDataInfo GetObjectDataByObjectID(string objectID)
        {
            try
            {
                return this.objectDatas.FirstOrDefault(item => item.ObjectID == objectID);
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.Trace(e.ToString());
                return new MapAddressObjectDataInfo();
            }
        }

        /// <summary>
        /// Save 눌렀을 당시 List 받기
        /// </summary>
        /// <param name="addressData"></param>
        /// <returns></returns>
        public MapAddressObjectDataInfo GetSavedSearchedAddressObjectData(MapAddressObjectDataInfo addressData)
        {
            var searchedAddressObjectData = this.objectDatas.FirstOrDefault(item => item.ObjectID == addressData.ObjectID) as MapAddressObjectDataInfo;

            if (searchedAddressObjectData == null) return null;

            searchedAddressObjectData.IsSaved = true;

            return new MapAddressObjectDataInfo(addressData);

        }

        /// <summary>
        /// Clear Datas
        /// </summary>
        public void ClearObjectDatas()
        {
            if (this.objectDatas != null) this.SearchAddressObjectDatas.Clear();
        }

        #endregion //List Data 관련

        #endregion //Methods
    }
}

