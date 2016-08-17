using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.ServiceHandlers;
using InnowatchConverter;
using Timer = System.Timers.Timer;

namespace ArcGISControl.DataManager
{
    public class HistoryManager
    {
        private const int HistoryLimit = 20;

        private const int IntervalMinLimit = 3;

        private readonly Timer idleTimer;

        private readonly ArcGISClientViewer arcGISClientViewer;

        private int idleCount;

        private bool isIdle;
        private bool isChanged;
        private int autoSavingSeconds = 10;

        public ObservableCollection<HistoryInfo> HistoryList { get; private set; }

        public bool IsRunning
        {
            get { return this.idleTimer.Enabled; }
        }

        public bool IsChanged
        {
            get { return this.isChanged; }
            set
            {
                this.isChanged = value;
                this.idleCount = 0;
            }
        }

        public bool IsIdle
        {
            get { return this.isIdle; }
            set
            {
                this.isIdle = value;

                if (!value)
                {
                    this.idleCount = 0;
                }
            }
        }

        public int AutoSavingSeconds
        {
            get { return this.autoSavingSeconds; }
            set
            {
                this.autoSavingSeconds = value;
                //this.autoSavingSeconds = value < 5 ? 5 : value;
            }
        }

        public HistoryManager(ArcGISClientViewer arcGISClientViewer)
        {
            this.arcGISClientViewer = arcGISClientViewer;
            this.arcGISClientViewer.PreviewMouseMove += this.arcGISClientViewer_PreviewMouseMove;
            this.arcGISClientViewer.PreviewMouseDown += this.arcGISClientViewer_PreviewMouseDown;
            this.arcGISClientViewer.PreviewMouseUp += this.arcGISClientViewer_PreviewMouseUp;
            this.arcGISClientViewer.PreviewMouseWheel += this.arcGISClientViewer_PreviewMouseWheel;
            this.arcGISClientViewer.PreviewKeyDown += this.arcGISClientViewer_PreviewKeyDown;
            this.arcGISClientViewer.PreviewKeyUp += this.arcGISClientViewer_PreviewKeyUp;

            this.HistoryList = new ObservableCollection<HistoryInfo>();

            this.idleTimer = new Timer {AutoReset = true, Interval = 1000};

            this.idleTimer.Elapsed += this.idleTimer_Elapsed;
        }

        void arcGISClientViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            this.IsIdle = false;
        }

        void arcGISClientViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsIdle = false;
        }

        void arcGISClientViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.IsIdle = false;
        }

        void arcGISClientViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.IsIdle = false;
        }

        void arcGISClientViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.IsIdle = false;
        }

        void arcGISClientViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            this.IsIdle = false;
        }


        private void idleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.isChanged)
            {
                return;
            }

            this.arcGISClientViewer.Dispatcher.Invoke(new Action(() =>
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed
                            || Mouse.RightButton == MouseButtonState.Pressed
                            || Mouse.MiddleButton == MouseButtonState.Pressed)
                    {
                        this.IsIdle = false;
                    }
                }));

            this.idleCount++;
            this.IsIdle = true;

            if (this.idleCount > this.autoSavingSeconds)
            {
                this.idleCount = 0;
                this.IsChanged = false;

                this.arcGISClientViewer.Dispatcher.Invoke(new Action(this.AddHistory));
            }
        }

        /// <summary>
        /// 히스토리 관리 시작.
        /// </summary>
        public void StartManagement()
        {
            if (this.arcGISClientViewer == null || this.autoSavingSeconds == 0)
            {
                return;
            }

            if (this.idleTimer.Enabled)
            {
                this.StopManagement();
            }

            this.IsChanged = false;
            this.idleCount = 0;

            this.idleTimer.Start();
        }

        /// <summary>
        /// 히스토리 관리 정지.
        /// </summary>
        public void StopManagement()
        {
            if (this.arcGISClientViewer == null)
            {
                return;
            }

            this.idleTimer.Stop();

            this.IsChanged = false;
            this.idleCount = 0;
        }

        /// <summary>
        /// 히스토리 목록 지우기.
        /// </summary>
        public void ClearHistory()
        {
            this.HistoryList.Clear();
        }

        /// <summary>
        /// 히시토리 데이타 반환.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DataSet GetHistoryData(string name)
        {
            var history = this.HistoryList.First(h => h.Name.Equals(name));

            if (history == null)
            {
                return null;
            }

            return this.Deserialize(history.Data);
        }

        /// <summary>
        /// 현재 상태를 히스토리에 저장.
        /// </summary>
        public void SaveHistory()
        {
            if (this.arcGISClientViewer == null)
            {
                return;
            }

            this.AddHistory();
        }

        private void AddHistory()
        {
            var time = DateTime.UtcNow;
            var name = time.ToLocalTime().ToString();
            var data = this.Serialize(this.arcGISClientViewer.GetCurrentMapSettingDataInfo(), this.GetObjectList());

            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            // 히스토리 저장
            this.HistoryList.Insert(0, new HistoryInfo {Time = time, Name = name, Data = data});

            // 저장 한계 초과 시 마지막 아이템 제거
            if (this.HistoryList.Count > HistoryLimit)
            {
                this.HistoryList.RemoveAt(this.HistoryList.Count - 1);
            }
        }

        private List<BaseMapObjectInfoData> GetObjectList()
        {
            if (this.arcGISClientViewer == null)
            {
                return null;
            }

            var mapInfo = this.arcGISClientViewer.GetCurrentMapSettingDataInfo();

            if (mapInfo == null)
            {
                return null;
            }

            var objectList = new List<BaseMapObjectInfoData>();
            objectList.AddRange(this.arcGISClientViewer.CameraList.ToList());
            objectList.AddRange(this.arcGISClientViewer.PublicLocationList.ToList());
            objectList.AddRange(this.arcGISClientViewer.SavedSplunkList.ToList());
            objectList.AddRange(this.arcGISClientViewer.UniversalDataInfoList);

            return objectList;
        }

        private string Serialize(MapSettingDataInfo aMapSettingInfo, IEnumerable<BaseMapObjectInfoData> aBaseMapObjectInfoDatas)
        {
            if (aMapSettingInfo == null || aBaseMapObjectInfoDatas == null)
            {
                return null;
            }

            try
            {
                var dataSet = MapDataServiceHandler.SetMapDataSet(aMapSettingInfo, aBaseMapObjectInfoDatas);

                var data = SerializeHelper.SerializeByXmlSerializer(dataSet);

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DataSet Deserialize(string aData)
        {
            if (string.IsNullOrWhiteSpace(aData))
            {
                return null;
            }

            try
            {
                return SerializeHelper.DeserializeByXmlSerializer<DataSet>(aData);
            }
            catch (Exception ex)
            {
                return null;
            }
            
            //var res = MapDataServiceHandler.GetMapFeatureFromSentDataSet(dataSet);
        }

        public void Dispose()
        {
            this.arcGISClientViewer.PreviewMouseMove -= this.arcGISClientViewer_PreviewMouseMove;
            this.arcGISClientViewer.PreviewMouseDown -= this.arcGISClientViewer_PreviewMouseDown;
            this.arcGISClientViewer.PreviewMouseUp -= this.arcGISClientViewer_PreviewMouseUp;
            this.arcGISClientViewer.PreviewMouseWheel -= this.arcGISClientViewer_PreviewMouseWheel;
            this.arcGISClientViewer.PreviewKeyDown -= this.arcGISClientViewer_PreviewKeyDown;
            this.arcGISClientViewer.PreviewKeyUp -= this.arcGISClientViewer_PreviewKeyUp;
        }
    }
}
