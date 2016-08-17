using ESRI.ArcGIS.Client.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.Helper
{
    public struct MapHistoryEntity
    {
        public string MapId;
        public Envelope Extent;

        public MapHistoryEntity(string mapId, Envelope extent)
        {
            this.MapId = mapId;
            this.Extent = extent;
        }
    }

    public class MapHistory
    {
        #region Fields
        /// <summary>
        ///  히스토리를 갖고 있는 리스트
        /// </summary>
        private readonly List<MapHistoryEntity> data = new List<MapHistoryEntity>();
        
        /// <summary>
        /// 히스토리의 어느 인덱스를 현재 보고 있는지 나타낸다.
        /// 리스트가 비어있지 않다면 항상 리스트 내의 인덱스이다.
        /// </summary>
        private int currentIndex = -1;

        /// <summary>
        /// 마지막 히스토리 이동 기록.
        /// </summary>
        private int lastHistoryPosition = -1;

        #endregion Fields

        #region Properties
        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }

        public int Index
        {
            get
            {
                return this.currentIndex;
            }
            set
            {
                if (value < 0 || value >= this.Count)
                    throw new ArgumentOutOfRangeException();
                this.currentIndex = value;
            }
        }

        public MapHistoryEntity Current
        {
            get
            {
                if (this.currentIndex == -1)
                    throw new InvalidOperationException("no history");
                return this.data[this.currentIndex];
            }
            set
            {
                if (this.currentIndex == -1)
                    throw new InvalidOperationException("no history");
                this.data[this.currentIndex] = value;
            }
        }
        #endregion Properties

        /// <summary>
        /// 히스토리를 모두 지우고 초기 상태로 만든다.
        /// </summary>
        public void Clear()
        {
            this.data.Clear();
            this.currentIndex = -1;
            this.lastHistoryPosition = -1;
        }

        /// <summary>
        /// 히스토리 사이의 이동이 아닌 새로운 맵으로 이동할 때 호출한다.
        /// 앞으로 가기 히스토리를 모두 제거하고 새로운 맵을 히스토리에 넣는다.
        /// </summary>
        /// <param name="mapHistory">map history entity를 나타내는 객체</param>
        public void NewLocation(MapHistoryEntity mapHistory)
        {
            this.lastHistoryPosition = this.currentIndex;
            this.data.RemoveRange(this.currentIndex + 1, this.data.Count - (this.currentIndex + 1));
            this.data.Add(mapHistory);
            this.currentIndex++;
        }

        /// <summary>
        /// 히스토리의 가장 처음으로 이동한다. 
        /// index를 지정하면 처음을 0으로 해서 센 위치로 이동한다.
        /// 실패하면 Nullable 타입 null을 반환한다.
        /// </summary>
        /// <param name="index">이동할 위치</param>
        /// <returns>성공하면 해당 Map 히스토리의 내용을 가져오고, 실패하면 null을 반환한다</returns>
        public MapHistoryEntity? TryGoToFirst(int index = 0)
        {
            if (index < 0 || this.Count <= index)
            {
                return null;
            }
            this.lastHistoryPosition = this.currentIndex;

            this.currentIndex = index;

            return this.data[this.currentIndex];
        }

        /// <summary>
        /// 히스토리의 가장 처음으로 이동한다. 
        /// index를 지정하면 처음을 0으로 해서 센 위치로 이동한다.
        /// 실패하면 Nullable 타입 null을 반환한다.
        /// </summary>
        /// <param name="index">이동할 위치</param>
        /// <returns>성공하면 해당 Map 히스토리의 내용을 가져오고, 실패하면 null을 반환한다</returns>
        public MapHistoryEntity? TryGoToLast(int index = 0)
        {
            if (index < 0 || this.Count <= index)
            {
                return null;
            }
            this.lastHistoryPosition = this.currentIndex;

            this.currentIndex = this.Count - index - 1;

            return this.data[this.currentIndex];
        }

        /// <summary>
        /// 지정한 횟수만큼 되돌아가는 시도를 한다.
        /// 실패하면 Nullable 타입 null을 반환한다.
        /// </summary>
        /// <param name="count">이동할 거리</param>
        /// <returns>성공하면 해당 Map 히스토리의 내용을 가져오고, 실패하면 null을 반환한다</returns>
        public MapHistoryEntity? TryGoBack(int count = 1)
        {
            if (count < 0 || this.currentIndex < count)
            {
                return null;
            }
            this.lastHistoryPosition = this.currentIndex;

            this.currentIndex -= count;

            return this.data[this.currentIndex];
        }

        /// <summary>
        /// 지정한 횟수만큼 앞으로 가는 시도를 한다.
        /// 실패하면 Nullable 타입 null을 반환한다.
        /// </summary>
        /// <param name="count">이동할 거리</param>
        /// <returns>성공하면 해당 Map 히스토리의 내용을 가져오고, 실패하면 null을 반환한다</returns>
        public MapHistoryEntity? TryGoNext(int count = 1)
        {
            if (count < 0 || this.currentIndex + count >= this.Count)
            {
                return null;
            }
            this.lastHistoryPosition = this.currentIndex;

            this.currentIndex += count;

            return this.data[this.currentIndex];
        }

        /// <summary>
        /// 상대적으로 이동한다.
        /// </summary>
        /// <param name="distance">이동할 거리</param>
        /// <returns>성공하면 해당 Map 히스토리의 내용을 가져오고, 실패하면 null을 반환한다</returns>
        public MapHistoryEntity? TryMove(int distance)
        {
            if (this.currentIndex + distance < 0 || this.currentIndex + distance >= this.Count)
            {
                return null;
            }
            this.lastHistoryPosition = this.currentIndex;

            this.currentIndex += distance;

            return this.data[this.currentIndex];
        }

        /// <summary>
        /// 마지막 명령에 의한 이동을 취소한다.
        /// </summary>
        /// <returns>성공할 경우 true, 아니면 false</returns>
        public bool CancelMove()
        {
            if (this.lastHistoryPosition < 0 || this.lastHistoryPosition >= this.Count)
                return false;

            var tmp = this.lastHistoryPosition;
            this.lastHistoryPosition = this.currentIndex;
            this.currentIndex = tmp;

            return true;
        }
    }
}
