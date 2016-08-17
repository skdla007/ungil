using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ArcGISControl.ArcGISInternalHack;

namespace ArcGISControl.Helper
{
    public class ExtentMaintainer : IDisposable
    {
        private Map map;

        private MapPoint center;

        private double area;

        private bool _IsMapSizeChanged = false;


        public ExtentMaintainer(Map map)
        {
            this.map = map;
            this.AddEventHandlers();
        }

        private void AddEventHandlers()
        {
            this.map.ExtentChanged += this.Map_ExtentChanged;
            this.map.SizeChanged += this.Map_SizeChanged;
        }

        private void RemoveEventHandlers()
        {
            this.map.ExtentChanged -= this.Map_ExtentChanged;
            this.map.SizeChanged -= this.Map_SizeChanged;
        }

        private void Map_ExtentChanged(object sender, ExtentEventArgs e)
        {
            if (_IsMapSizeChanged)
            {
                MapReSize();
                _IsMapSizeChanged = false;
            }

            this.center = this.map.Extent.GetCenter();
            this.area = this.map.Extent.Width * this.map.Extent.Height;
        }

        private void MapReSize()
        {
            if (this.center == null)
                return;

            if (NumberUtil.AreSame(this.map.ActualWidth, 0) || NumberUtil.AreSame(this.map.ActualHeight, 0))
                return;

            var ratio = this.map.ActualWidth / this.map.ActualHeight;
            var height = Math.Sqrt(this.area / ratio);
            var width = height * ratio;

            var extent = new Envelope(center.X - width / 2, center.Y - height / 2, center.X + width / 2, center.Y + height / 2);
            this.map.ForceZoomTo(extent, true);
        }

        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 2014. 07. 24 엄태영
            // 기존 코드 주석처리 MapReSize()로 대체,
            // MapSizeChanged가 일어나면 Map_ExtentChanged도 동시에 발생함으로 Map_ExtentChanged안에서 한번에 처리로 변경.
            // Map_SizeChanged이벤트는 좌측 Panel이 Visible될때 Pannel Control이 UI에 랜더링 되기 전에 발생하므로 Map_SizeChanged에서 Map을 이동처리하게 되면
            // Map_ExtentChanged에서 UI 렌더링이 완료되어 this.map.Extent.XMin, this.map.Extent.XMax값 등이 다시 틀려지게 된다.

            _IsMapSizeChanged = true;

            /*
            if (this.center == null)
                return;

            if (NumberUtil.AreSame(e.NewSize.Width, 0) || NumberUtil.AreSame(e.NewSize.Height, 0))
                return;

            var ratio = e.NewSize.Width / e.NewSize.Height;
            var height = Math.Sqrt(this.area / ratio);
            var width = height * ratio;

            var extent = new Envelope(center.X - width / 2, center.Y - height / 2, center.X + width / 2, center.Y + height / 2);
            this.map.ForceZoomTo(extent, true);
             * */
        }

        public void Dispose()
        {
            this.RemoveEventHandlers();
            this.map = null;
            this.center = null;
        }
    }
}
