using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using InnowatchConverter;
using InnowatchDebug;
using ServiceContract;

namespace ArcGISControls.CommonData.ServiceHandlers
{
    public class MapDataServiceHandler
    {
        #region Fields

        private readonly BackgroundWorker backgroundWorker;
      
        private string mapDataServerIp;

        private int mapServerPort;

        class BackgroundWorkInfo
        {
            public string MapId { get; set; }
            public object Token { get; set; }
            public string FeatureType { get; set; }
        }

        private BackgroundWorkInfo workInfo;

        private bool disposedToken = false;
        
        private ObservableCollection<BaseMapObjectInfoData> cameraObjectComponentDataInfos = new ObservableCollection<BaseMapObjectInfoData>();
        public ObservableCollection<BaseMapObjectInfoData> CameraObjectComponentDataInfos
        {
            get { return this.cameraObjectComponentDataInfos; }
        }

        public object CompletedToken { get; set; }

        private int reDoBackgrond = 0;

        #endregion //Fields

        #region Events
        
        /// <summary>
        /// Map Service 종료 되었을 경우 Event 생성 
        /// </summary>
        public event EventHandler<GetMapDataProcessCompletedEventArgs> eGetMapDataProcessCompleted;

        #endregion //Events

        #region Construction

        public MapDataServiceHandler(string ip, int port)
        {
            this.mapDataServerIp = ip;
            this.mapServerPort = port;

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            this.backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            this.backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
        }

        #endregion //Construction 

        #region Method

        private void RaiseLoadingProcessCompletedEventArgs(string data, bool isCompleted)
        {
            var loadingCompleted = this.eGetMapDataProcessCompleted;
            if (loadingCompleted != null)
            {
                loadingCompleted(this, new GetMapDataProcessCompletedEventArgs(data, isCompleted));
            }
        }

        /// <summary>
        /// Map의 Feature data를 받아오는 작업을 비동기로 시작한다.
        /// </summary>
        /// <param name="mapId">데이터를 받아올 map의 ID</param>
        /// <param name="workToken">작업이 완료되면 CompletedToken에 설정할 token object</param>
        public void StartGetMapFeatureData(string mapId, object workToken, string featureType = null)
        {
            this.workInfo = new BackgroundWorkInfo()
            {
                MapId = mapId,
                Token = workToken,
                FeatureType = featureType
            };

            if (this.backgroundWorker != null)
            {
                if (this.backgroundWorker.IsBusy)
                {
                    this.reDoBackgrond = 1;
                    this.backgroundWorker.CancelAsync();
                }
                else
                {
                    this.reDoBackgrond = 0;
                    this.backgroundWorker.RunWorkerAsync();
                }
            }
        }

        public void CancelGetMapFeatureData()
        {
            if (this.backgroundWorker != null)
            {
                this.reDoBackgrond = 0;
                this.backgroundWorker.CancelAsync();
            }
        }

        ///
        private bool GetFeatureData()
        {
            var currentWorkInfo = this.workInfo;
            var result = this.RequestGetFeatureData(currentWorkInfo);

            this.InitializeMapFeatureInformation(result);

            this.CompletedToken = currentWorkInfo.Token;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string RequestGetFeatureData(BackgroundWorkInfo currentWorkInfo)
        {
            try
            {
                // CT로 로그인 요청.
                var getMapFeatureDataUrl =
                    string.Format(
                        "http://{0}:{1}/rest/data/getmapfeatureinformation?mapid=" + currentWorkInfo.MapId,
                        this.mapDataServerIp,
                        this.mapServerPort.ToString(CultureInfo.InvariantCulture));

                if (currentWorkInfo.FeatureType != null)
                {
                    getMapFeatureDataUrl += "&featureType=" + currentWorkInfo.FeatureType;
                }

                var mapDataParam = new RequestParameter() { Url = getMapFeatureDataUrl };

                // 접속한 자기 자신의 서버 정보(ServiceType, RatingLevel)를 내려준다.
                var responsemapDataResult = ServiceHandler.Request(mapDataParam);

                return responsemapDataResult;
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLine(string.Format("[Login Error] {0}", ex));
                return ex.Message;
            }
        }

        #endregion

        #region Event Handelrs

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            this.GetFeatureData();
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            var redo = Interlocked.Exchange(ref this.reDoBackgrond, 0);
            if (redo != 0)
            {
                this.backgroundWorker.RunWorkerAsync();
                return;
            }

            if (runWorkerCompletedEventArgs.Cancelled)
            {
                InnowatchDebug.Logger.Trace("N Branching Statement Processing - if (runWorkerCompletedEventArgs.Cancelled)");
            }
            else if(runWorkerCompletedEventArgs.Error == null )
            {
                this.RaiseLoadingProcessCompletedEventArgs("Complete Get Data Service", true);
            }
        }

        private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
        }


        #endregion Event Handlers

        #region Method

        #region GetData From CT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public bool InitializeMapFeatureInformation(string xml)
        {
            try
            {
                if (this.backgroundWorker.CancellationPending)
                {
                    return false;
                }

                var dataList = this.GetReceivedDataList(xml);

                foreach (var dataSet in dataList)
                {
                    
                    this.GetMapFeatureDataSet(dataSet);
                }

                return true;
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLine(String.Format("[InitializeCameraData Error] {0}", ex));
                return false;
            }

            //return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSet"></param>
        private void GetMapFeatureDataSet(DataSet dataSet)
        {
            if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count <= 0)
            {
                InnowatchDebug.Logger.Trace("Camera DataSet has no table. Invalid data recived");
            }

            this.cameraObjectComponentDataInfos = null;

            this.cameraObjectComponentDataInfos 
                = new ObservableCollection<BaseMapObjectInfoData>();

            if(dataSet == null || dataSet.Tables.Count < 1 )
            {
                InnowatchDebug.Logger.Trace(String.Format("[InitializeCameraData Error] Cannot Convert DataSet"));
                return;
            }

            foreach (DataRow cameraDataRow in dataSet.Tables[0].Rows)
            {
                if (this.backgroundWorker.CancellationPending)
                {   
                    return;
                }

                BaseMapObjectInfoData objectData = null;
                var type = (MapObjectType)Enum.Parse(typeof(MapObjectType), DataTypeConverter.ConvertNullableString(cameraDataRow["FeatureType"]));
                var data = DataTypeConverter.ConvertNullableString(cameraDataRow["data"]);
                
                if(string.IsNullOrEmpty(data))
                    continue;

                objectData = BaseMapObjectInfoData.Deserialize(type, data);

                cameraObjectComponentDataInfos.Add(objectData);
            }
        }

        #endregion //GetData From CT

        #region Set Data To CT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapSettingDataInfo"></param>
        /// <param name="objectDatas"></param>
        /// <param name="userId"></param>
        /// <param name="isAdd"></param>
        /// <returns></returns>
        public int SaveMapAndFeatures(
            MapSettingDataInfo mapSettingDataInfo,  
            IEnumerable<BaseMapObjectInfoData> objectDatas, 
            string userId,
            bool isAdd = false)
        {
            if (objectDatas == null)
                return -1;

            var dataSet = SetMapDataSet(mapSettingDataInfo, objectDatas);

            var returnDataSet = 
                this.RequestAndReceiveResultDataSet(
                    dataSet, 
                    "Map", 
                    string.Format("type={0}&userid={1}&relay={2}", isAdd ? "add" : "modify", userId, true));

            if (returnDataSet == null)
            {
                Logger.WriteInfoLog("[SaveMapAndFeatures Error] Failed to save map. Response data is null.");
                return -1;
            }

            // CT로 현재 ResourceRevision 요청
            var url =
                string.Format(
                    "http://{0}:{1}/rest/data/{2}",
                    this.mapDataServerIp,
                        this.mapServerPort,
                    "GetResourceVersion");
            var result = ServiceHandler.Request(url);
            var revisionSet = SerializeHelper.DeserializeByDataContractSerializer<DataSet>(result);
            if (dataSet == null)
            {
                return -1;
            }

            return (int)revisionSet.Tables[0].Rows[0]["Revision"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapSettingDataInfo"></param>
        /// <param name="objectDatas"></param>
        /// <returns></returns>
        public static DataSet SetMapDataSet(MapSettingDataInfo mapSettingDataInfo,  IEnumerable<BaseMapObjectInfoData> objectDatas)
        {
            var dataSet = new DataSet();

            //Map Data
            var mapTable = new DataTable("Map");
            dataSet.Tables.Add(mapTable);

            mapTable.Columns.Add("MapGuid",typeof(string));
            mapTable.Columns.Add("Data", typeof(string));

            var newRow = mapTable.NewRow();

            newRow["MapGuid"] = mapSettingDataInfo.ID;
            newRow["Data"] = mapSettingDataInfo.SaveDataToXML();

            mapTable.Rows.Add(newRow);

            var mapFeatureTable = new DataTable("MapFeature");
            dataSet.Tables.Add(mapFeatureTable);

            mapFeatureTable.Columns.Add("MapGuid", typeof(string));
            mapFeatureTable.Columns.Add("FeatureName", typeof(string));
            mapFeatureTable.Columns.Add("Data", typeof(string));
            mapFeatureTable.Columns.Add("FeatureType", typeof(string));
            mapFeatureTable.Columns.Add("MaxLat", typeof(double));
            mapFeatureTable.Columns.Add("MaxLng", typeof(double));
            mapFeatureTable.Columns.Add("MinLat", typeof(double));
            mapFeatureTable.Columns.Add("MinLng", typeof(double));

            foreach(var objectData in objectDatas)
            {
                string objectDataString = objectData.SaveDataToXML();

                newRow = mapFeatureTable.NewRow();

                newRow["MapGuid"] = DataTypeConverter.ConvertNullableString(mapSettingDataInfo.ID);
                newRow["FeatureName"] = DataTypeConverter.ConvertNullableString(objectData.Name);
                newRow["Data"] = DataTypeConverter.ConvertNullableString(objectDataString);
                newRow["FeatureType"] = objectData.ObjectType.ToString();
                
                newRow["MaxLat"] = DataTypeConverter.ConvertNullableDouble(objectData.ExtentMax.X);
                newRow["MaxLng"] = DataTypeConverter.ConvertNullableDouble(objectData.ExtentMax.Y);
                newRow["MinLat"] = DataTypeConverter.ConvertNullableDouble(objectData.ExtentMin.X);
                newRow["MinLng"] = DataTypeConverter.ConvertNullableDouble(objectData.ExtentMin.Y);

                mapFeatureTable.Rows.Add(newRow);
            }

            //mapTable.Dispose();
            //mapFeatureTable.Dispose();
            
            return dataSet;
        }

        /// <summary>
        /// SetMapDataSet에 의해 만들어진 DataSet을 이용하여 BaseMapObjectInfoData의 목록을 만들어내는 함수이다.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static ObservableCollection<BaseMapObjectInfoData> GetMapFeatureFromSentDataSet(DataSet dataSet)
        {
            var cameraObjectComponentDataInfos
                = new ObservableCollection<BaseMapObjectInfoData>();

            foreach (DataRow cameraDataRow in dataSet.Tables[1].Rows)
            {
                BaseMapObjectInfoData objectData = null;
                var type = (MapObjectType)Enum.Parse(typeof(MapObjectType), DataTypeConverter.ConvertNullableString(cameraDataRow["FeatureType"]));
                var data = DataTypeConverter.ConvertNullableString(cameraDataRow["data"]);

                if (string.IsNullOrEmpty(data))
                    continue;

                objectData = BaseMapObjectInfoData.Deserialize(type, data);

                cameraObjectComponentDataInfos.Add(objectData);
            }
            return cameraObjectComponentDataInfos;
        }

        #endregion //Set Data To CT

        #region Manage Data Set 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultDataSet"></param>
        /// <returns></returns>
        private bool CheckProcessResultFromDataSet(DataSet resultDataSet)
        {
            try
            {
                if (resultDataSet == null)
                    return false;

                if (resultDataSet.Tables.Count == 0)
                    return false;

                if (resultDataSet.Tables[0].Rows.Count == 0)
                    return false;

                if (string.CompareOrdinal(resultDataSet.Tables[0].Rows[0][0].ToString(), "Failed") == 0)
                {
                    var methodName = GetMethodInfoStrings.GetMethodName();
                    var errorMessage = resultDataSet.Tables[0].Rows[0][1].ToString();
                    InnowatchDebug.Logger.WriteLine(string.Format("[{0} Process Result Error] {1}", methodName, errorMessage));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                var methodName = GetMethodInfoStrings.GetMethodName();
                InnowatchDebug.Logger.WriteLine(string.Format("[{0} Process Result Error] {1}", methodName, ex));
                return false;
            }
        }

        /// <summary>
        /// 전달받은 xml 을 Deserialize 하여 데이터 리스트를 가져온다.
        /// Deserialize 에 관련된 유효성 검사를 함께 실행한다.
        /// </summary>
        /// <param name="xml">Serialized xml.</param>
        /// <returns>DataSet List.</returns>
        private IEnumerable<DataSet> GetReceivedDataList(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                // TODO : Resource 화. - by shwlee.
                throw new Exception("Serialized string is empty. Invalid data received.");
            }

            var dataList = SerializeHelper.DeserializeByDataContractSerializer<List<DataSet>>(xml);
            if (dataList == null)
            {
                // TODO : Resource 화. - by shwlee.
                throw new Exception("Deserialize failed. Invalid data received.");
            }

            if (dataList.Count == 0)
            {
                // TODO : Resource 화. - by shwlee.
                throw new Exception("Data list count is 0. Invalid data received.");
            }

            return dataList;
        }

        private bool RequestAndReceiveResult<T>(T target, string service, string queryString, bool isPost = true)
        {
            var resultDataSet = this.RequestAndReceiveResultDataSet(target, service, queryString, isPost);

            // 작업 성공 여부 반환.
            return this.CheckProcessResultFromDataSet(resultDataSet);
        }

        private DataSet RequestAndReceiveResultDataSet<T>(T target, string service, string queryString, bool isPost = true)
        {
            try
            {
                var serialized = SerializeHelper.SerializeByDataContractSerializer(target);

                // CT로 저장 요청.
                var url =
                    string.Format(
                        "http://{0}:{1}/rest/data/{2}{3}",
                        this.mapDataServerIp,
                        this.mapServerPort,
                        service,
                        string.IsNullOrWhiteSpace(queryString) ? string.Empty : string.Format("?{0}", queryString));

                var param = new RequestParameter { Url = url, PostMessage = serialized };
                return ServiceHandler.RequestToDataSetResponse(param);
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLine(string.Format("[{0} Error] {1}", GetMethodInfoStrings.GetMethodName(), ex));
                return null;
            }
        }


        #endregion Manage Data Set
        
        #endregion 
    }
}
