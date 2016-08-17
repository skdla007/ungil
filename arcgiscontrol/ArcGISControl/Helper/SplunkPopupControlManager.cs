using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using Innotive.SplunkControl.ChartControl;
using Innotive.SplunkControl.Table;
using Innotive.SplunkControl.Table.Event;
using Innotive.SplunkManager.SplunkManager.Data;

namespace ArcGISControl.Helper
{
    public class SplunkPopupControlManager
    {
        private Point positionOffset;

        private Panel parentPanel;

        private SplunkPopupControl splunkPopupControl;

        private SplunkChartTableWrapperControl chartTableWrapperControl;

        private Point iconPosition;

        private bool adjustPopupPositionRequired;

        private string showingSplunkId;
        public string ShowingSplunkId
        {
            get { return this.showingSplunkId; }
        }

        public bool DoShowing
        {
            get { return !string.IsNullOrEmpty(this.showingSplunkId); }
        }

        private int zIndex;

        public int ZIndex
        {
            get { return this.zIndex; }
            set
            {
                this.zIndex = value;

                if (this.splunkPopupControl != null && this.splunkPopupControl.Parent != null)
                {
                    Panel.SetZIndex(this.splunkPopupControl, value);
                }
            }
        }

        #region Events

        public event EventHandler<CellClickEventArgs> eTableCellClick;

        public void OnEUrlLinkRequest(CellClickEventArgs e)
        {
            var handler = eTableCellClick;
            if (handler != null) handler(this, e);
        }

        #endregion // Events

        #region Methods

        public SplunkPopupControlManager()
        {
            this.splunkPopupControl = new SplunkPopupControl();

            this.splunkPopupControl.Loaded += this.SplunkPopupControlOnLoaded;
            this.splunkPopupControl.xButtonClose.Click += this.XButtonCloseOnClick;
            //this.splunkPopupControl.xSplunkChartTableControl.SizeChanged += xSplunkChartTableControl_SizeChanged;
            this.splunkPopupControl.eResizing += this.splunkPopupControl_eResizing;
            this.splunkPopupControl.xSplunkChartTableControl.DataCellClick += this.xSplunkChartTableControlOnDataCellClick;
        }

        public void AddToParent(Panel parent)
        {
            this.parentPanel = parent;
            parent.Children.Add(this.splunkPopupControl);
            Panel.SetZIndex(this.splunkPopupControl, this.zIndex);
        }

        public void RemoveFromParent(Panel parent)
        {
            parent.Children.Remove(this.splunkPopupControl);
        }

        public void AdjustPopupPosition(Point aIconPosition)
        {
            if (string.IsNullOrEmpty(showingSplunkId)) return;

            this.iconPosition = aIconPosition;

            var control = this.splunkPopupControl;

            var controlHeight = control.ActualHeight;

            var controlWidth = control.ActualWidth;

            if (this.adjustPopupPositionRequired)
            {
                if (aIconPosition.Y > controlHeight)
                {
                    this.positionOffset.Y = -controlHeight - 10;
                }
                else if (controlHeight < this.parentPanel.ActualHeight - aIconPosition.Y)
                {
                    this.positionOffset.Y = 10;
                }
                else
                {
                    if (this.parentPanel.ActualHeight / 2 < this.positionOffset.Y)
                    {
                        this.positionOffset.Y = -controlHeight - 10;
                    }
                    else
                    {
                        this.positionOffset.Y = 10;
                    }
                }

                if (this.parentPanel.ActualWidth > aIconPosition.X + controlWidth)
                {
                    this.positionOffset.X = 0;
                }
                else if (controlWidth < aIconPosition.X)
                {
                    this.positionOffset.X = -controlWidth;
                }
                else
                {
                    if (this.parentPanel.ActualWidth / 2 < aIconPosition.X)
                    {
                        this.positionOffset.X = 0;
                    }
                    else
                    {
                        this.positionOffset.X = -controlWidth;
                    }
                }

                this.adjustPopupPositionRequired = false;
            }

            if (double.IsInfinity(this.positionOffset.X) || double.IsNaN(this.positionOffset.X) || double.IsInfinity(this.positionOffset.Y) || double.IsNaN(this.positionOffset.Y))
            {
                Canvas.SetLeft(control, aIconPosition.X);
                Canvas.SetTop(control, aIconPosition.Y + 10);
            }
            else
            {
                Canvas.SetLeft(control, aIconPosition.X + this.positionOffset.X);
                Canvas.SetTop(control, aIconPosition.Y + this.positionOffset.Y);
            }
        }

        public void Show(Point iconPosition, SplunkChartTableWrapperControl chartTableWrapperControl)
        {
            this.ShowInternal(iconPosition, chartTableWrapperControl.MapSplunkObjectData, chartTableWrapperControl.ResultSet);

            this.chartTableWrapperControl = chartTableWrapperControl;

            chartTableWrapperControl.onChangedSplunkControl += ChartTableWrapperControlOnOnChangedSplunkControl;
        }

        /// <summary>
        /// 매니저 내부의 차트/테이블 컨트롤을 사용한 스플렁크 팝업 컨트롤 띄우기. 
        /// Universal Object만 사용 중.
        /// </summary>
        /// <param name="aIconPosition"></param>
        /// <param name="aObjectId"></param>
        /// <param name="aSplunkResultSet"></param>
        /// <param name="aTitle"> </param>
        /// <param name="aLinkUrlTable"> </param>
        public void Show(Point aIconPosition, string aObjectId, SplunkResultSet aSplunkResultSet, string aTitle, DataTable aLinkUrlTable = null)
        {
            var splunkObjectData = new MapSplunkObjectDataInfo
                                       {ObjectID = aObjectId, SplunkBasicInformation = {Name = "dummy"}, Title = aTitle};

            this.ShowInternal(aIconPosition, splunkObjectData, aSplunkResultSet);

            if (this.splunkPopupControl != null && aLinkUrlTable != null)
            {
                var decos = new List<TextDecoInfo>();

                for(var r = 0; r < aLinkUrlTable.Rows.Count; r++)
                {
                    for(var c = 0; c < aLinkUrlTable.Columns.Count; c++)
                    {
                        var row = aLinkUrlTable.Rows[r];

                        if (!string.IsNullOrWhiteSpace(row[c].ToString()))
                        {
                            decos.Add(new TextDecoInfo{RowIndex = r, ColumnIndex = c, UnderLine = true});
                        }
                    }
                }

                this.splunkPopupControl.xSplunkChartTableControl.TableControl.SetCellTextDeco(decos);
            }

            this.chartTableWrapperControl = null;
        }

        /// <summary>
        /// 데이타 없이 로딩 아이콘만 보여주기.
        /// Universal Object만 사용 중.
        /// </summary>
        public void ShowLoading(Point aIconPosition, string aObjectId, string aTitle)
        {
            this.splunkPopupControl.Visibility = Visibility.Visible;

            if (this.chartTableWrapperControl != null) this.chartTableWrapperControl.onChangedSplunkControl -= ChartTableWrapperControlOnOnChangedSplunkControl;

            this.iconPosition = aIconPosition;

            /*
            this.SetSplunkControlUI(
                new MapSplunkObjectDataInfo
                    {ObjectID = aObjectId, SplunkBasicInformation = {Name = "dummy"}, Title = aTitle},
                new SplunkResultSet(Guid.NewGuid().ToString(), new DataTable(), null, null));
             * */

            MapSplunkObjectDataInfo mapSplunkObjectDataInfo = new MapSplunkObjectDataInfo() { ObjectID = aObjectId, SplunkBasicInformation = { Name = "dummy" }, Title = aTitle };
            SplunkResultSet splunkResultSet = new SplunkResultSet(Guid.NewGuid().ToString(), new DataTable(), null, null);
            this.SetSplunkControlUI(mapSplunkObjectDataInfo, splunkResultSet);

            this.splunkPopupControl.xSplunkChartTableControl.ShowLoading(aTitle);

            this.splunkPopupControl.UpdateLayout();

            this.adjustPopupPositionRequired = true;
            this.AdjustPopupPosition(aIconPosition);

            this.splunkPopupControl.UseResize = false;

            this.chartTableWrapperControl = null;
        }

        private void ShowInternal(Point aIconPosition, MapSplunkObjectDataInfo aSplunkObjectData, SplunkResultSet aSplunkResultSet)
        {
            this.splunkPopupControl.Visibility = Visibility.Visible;

            if (this.chartTableWrapperControl != null) this.chartTableWrapperControl.onChangedSplunkControl -= ChartTableWrapperControlOnOnChangedSplunkControl;

            this.iconPosition = aIconPosition;
            this.SetSplunkControlUI(aSplunkObjectData, aSplunkResultSet);
            
            this.splunkPopupControl.UpdateLayout();

            this.adjustPopupPositionRequired = true;
            this.AdjustPopupPosition(aIconPosition);
        }

        private void SetSplunkControlUI(MapSplunkObjectDataInfo splunkObjectData, SplunkResultSet splunkResultSet)
        {
            var currentWrapperControl = this.splunkPopupControl.xSplunkChartTableControl;

            this.showingSplunkId = splunkObjectData.ObjectID;

            currentWrapperControl.MapSplunkObjectData = splunkObjectData;

            currentWrapperControl.SetSplunkControlUI(this.showingSplunkId, splunkObjectData.SplunkBasicInformation, splunkResultSet);
            currentWrapperControl.SetTitle(splunkObjectData.Title);
        }

        public void Hide()
        {
            if (chartTableWrapperControl != null) this.chartTableWrapperControl.onChangedSplunkControl -= ChartTableWrapperControlOnOnChangedSplunkControl;

            this.chartTableWrapperControl = null;
            
            this.splunkPopupControl.Visibility = Visibility.Collapsed;

            this.showingSplunkId = string.Empty;

            this.splunkPopupControl.SetDefaultSize();
        }

        #endregion Methods

        #region Event Handlers

        private void XButtonCloseOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Hide();
        }

        private void SplunkPopupControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var splunkContorl = sender as SplunkPopupControl;

            if(splunkContorl == null) return;

            splunkContorl.Visibility = Visibility.Collapsed;
        }

        private void ChartTableWrapperControlOnOnChangedSplunkControl(object sender, EventArgs eventArgs)
        {
            var chartTableWrapperControl = sender as SplunkChartTableWrapperControl;
            if (chartTableWrapperControl == null) return;
            this.SetSplunkControlUI(chartTableWrapperControl.MapSplunkObjectData, chartTableWrapperControl.ResultSet);
        }

        private void xSplunkChartTableControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (!this.adjustPopupPositionRequired) return;

            //this.adjustPopupPositionRequired = true;
            this.AdjustPopupPosition(this.iconPosition);
        }

        private void splunkPopupControl_eResizing(object sender, EventArgs e)
        {
            this.positionOffset.X = Canvas.GetLeft(this.splunkPopupControl) - this.iconPosition.X;
            this.positionOffset.Y = Canvas.GetTop(this.splunkPopupControl) - this.iconPosition.Y;
        }

        private void xSplunkChartTableControlOnDataCellClick(object sender, DataCellClickEventArgs e)
        {
            this.OnEUrlLinkRequest(new CellClickEventArgs(this.ShowingSplunkId, e.RowIndex, e.ColumnIndex));
        }

        #endregion Event Handlers

    }

    public class CellClickEventArgs : EventArgs
    {
        public CellClickEventArgs(string aObjectId, int aRowIndex, int aColIndex)
        {
            this.ObjectId = aObjectId;
            this.RowIndex = aRowIndex;
            this.ColumnIndex = aColIndex;
        }

        public string ObjectId { get; set; }

        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }
    }
}
