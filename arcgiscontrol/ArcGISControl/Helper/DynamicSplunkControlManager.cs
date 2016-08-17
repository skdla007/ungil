using System.Runtime.CompilerServices;
using ArcGISControl.UIControl;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using Innotive.SplunkControl.Table;
using Innotive.SplunkControl.Table.Event;
using Innotive.SplunkManager.SplunkManager;
using Innotive.SplunkManager.SplunkManager.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGISControl.Helper
{
    public class DynamicSplunkControlManager
    {
        public const double tableCellMaxWidth = 200;

        public const double chartDefaultWidth = 200;

        public const double chartDefautHegiht = 200;

        private SplunkChartTableWrapperControl chartTableWrapperControl;

        private SplunkService searchingService;

        private string searchingIndex;

        private FrameworkElement showingElement;

        private Point showingMousePosition;

        public event EventHandler<DataRowClickEventArgs> DataRowClick;

        public event EventHandler<ButtonColumnCellClickEventArgs> ButtonColumnCellClick;

        private bool adjustTablePositionRequired;

        private int zIndex;

        public bool IsShow { get; set; }

        public int ZIndex
        {
            get { return this.zIndex; }
            set
            {
                this.zIndex = value;

                if (this.chartTableWrapperControl != null && this.chartTableWrapperControl.Parent != null)
                {
                    Panel.SetZIndex(this.chartTableWrapperControl, value);
                }
            }
        }

        public DynamicSplunkControlManager()
        {  
            this.chartTableWrapperControl = new SplunkChartTableWrapperControl(true);
            this.chartTableWrapperControl.Visibility = Visibility.Collapsed;
            this.chartTableWrapperControl.SizeChanged += tableControl_SizeChanged;
            this.chartTableWrapperControl.DataRowClick += tableControl_DataRowClick;
            this.chartTableWrapperControl.ButtonColumnCellClick += tableControl_ButtonColumnCellClick;
            this.chartTableWrapperControl.MouseDown += ChartTableWrapperControlOnMouseDown;
            this.chartTableWrapperControl.MouseEnter += ChartTableWrapperControlOnMouseEnter;
        }

        private void tableControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.adjustTablePositionRequired) return;
            this.adjustTablePositionRequired = false;

            this.AdjustTablePosition();
        }

        private void tableControl_DataRowClick(object sender, DataRowClickEventArgs e)
        {
            if (this.DataRowClick != null)
            {
                this.DataRowClick(sender, e); // forward
            }
        }

        private void tableControl_ButtonColumnCellClick(object sender, ButtonColumnCellClickEventArgs e)
        {
            if (this.ButtonColumnCellClick != null)
            {
                this.ButtonColumnCellClick(sender, e); // forward
            }
        }

        private void ChartTableWrapperControlOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var control = sender as SplunkChartTableWrapperControl;

            if(control == null) return;

            control.Cursor = Cursors.Arrow;
        }

        private void ChartTableWrapperControlOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var control = sender as SplunkChartTableWrapperControl;

            if (control == null) return;

            control.Cursor = Cursors.Arrow;
        }

        public void AddToParent(Panel parent)
        {
            parent.Children.Add(this.chartTableWrapperControl);
            Panel.SetZIndex(this.chartTableWrapperControl, this.zIndex);
        }

        public void RemoveFromParent(Panel parent)
        {
            parent.Children.Remove(this.chartTableWrapperControl);
        }

        private void AdjustTablePosition()
        {
            var control = this.chartTableWrapperControl;
            var element = this.showingElement;
            var mousePoint = this.showingMousePosition;

            var elementCenterX = element.ActualWidth / 2;
            var elementCenterY = element.ActualHeight / 2;
            
            var x = mousePoint.X;
            var y = mousePoint.Y;

            var overX = double.NaN;
            if (x > elementCenterX)
            {
                x -= (control.ActualWidth + 1);
                if (control.ActualWidth > mousePoint.X)
                    overX = control.ActualWidth - mousePoint.X;
            }
            else
            {
                x += 1;
                if (control.ActualWidth > element.ActualWidth - mousePoint.X)
                    overX = (element.ActualWidth - mousePoint.X) - control.ActualWidth;
            }

            var overY = double.NaN;
            if (y > elementCenterY)
            {
                y -= (control.ActualHeight + 1);
                if (control.ActualHeight > mousePoint.Y)
                    overY = control.ActualHeight - mousePoint.Y;
            }
            else
            {
                y += 1;
                if (control.ActualHeight > element.ActualHeight - mousePoint.Y)
                    overY = (element.ActualHeight - mousePoint.Y) - control.ActualHeight;
            }

            if (!double.IsNaN(overX) && !double.IsNaN(overY))
            {
                if (Math.Abs(overX) > Math.Abs(overY))
                {
                    x += overX;
                }
                else
                {
                    y += overY;
                }
            }
            else if (!double.IsNaN(overX))
            {
                x += overX;
            }
            else if (!double.IsNaN(overY))
            {
                y += overY;
            }

            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
        }

        public void Show(FrameworkElement canvas, Point pos)
        {
            this.IsShow = true;

            this.showingElement = canvas;
            this.showingMousePosition = pos;

            this.chartTableWrapperControl.Visibility = Visibility.Visible;

            this.adjustTablePositionRequired = true;
            this.AdjustTablePosition();
        }

        public void Show(FrameworkElement canvas)
        {
            this.Show(canvas, Mouse.GetPosition(canvas));
        }

        public void Hide()
        {
            this.StopFetchingData();
            this.chartTableWrapperControl.Visibility = Visibility.Collapsed;
            this.IsShow = false;
        }

        /// <summary>
        /// LinkZone Dynamic Table
        /// </summary>
        /// <param name="splunkBasicInformationData"></param>
        public void StartFetchingDataFromSavedSearch(SplunkBasicInformationData splunkBasicInformationData)
        {
            var copiedData = new SplunkBasicInformationData(splunkBasicInformationData);

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformationData.Name,
                SearchIndex = searchingIndex
            };

            for (int i = 0; i < splunkBasicInformationData.SplArgumentKeys.Count; i++)
            {
                savedSearchArgs.SavedSearchArgs.Add(splunkBasicInformationData.SplArgumentKeys[i], splunkBasicInformationData.SplArgumentValues[i]);
            }

            this.StartFetchingData(copiedData, SearchType.SavedSearch, savedSearchArgs);
        }

        /// <summary>
        /// DataPlayback 상황
        /// LinkZone Dynamic Table
        /// </summary>
        /// <param name="splunkBasicInformationData"></param>
        public void StartFetchingDataFromSavedSearch(SplunkBasicInformationData splunkBasicInformationData, TimeSpan timeSpan, OperationPlaybackArgs operationPlaybackArgs)
        {
            var copiedData = new SplunkBasicInformationData(splunkBasicInformationData);

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformationData.Name,
                SearchIndex = searchingIndex,
                TimeLineTimeSpan = timeSpan,
                TimeLinePlayWay = SplunkSavedSearchArgs.PlayWayMode.Seek,
            };

            SplunkServiceHandler.ApplyOperationPlaybackArgs(savedSearchArgs, operationPlaybackArgs);

            for (int i = 0; i < splunkBasicInformationData.SplArgumentKeys.Count; i++)
            {
                savedSearchArgs.SavedSearchArgs.Add(splunkBasicInformationData.SplArgumentKeys[i], splunkBasicInformationData.SplArgumentValues[i]);
            }

            this.StartFetchingData(copiedData, SearchType.SavedSearch, savedSearchArgs);
        }

        /// <summary>
        /// Splunk Table DataRow Click
        /// </summary>
        /// <param name="spl"></param>
        /// <param name="splunkApp"></param>
        /// <param name="invokingData"></param>
        public void StartFetchingDataFromSplSearch(string spl, string splunkApp, SplunkBasicInformationData invokingData)
        {
            var splunkBasicInformationData = new SplunkBasicInformationData(invokingData);

            splunkBasicInformationData.Name = spl;

            if (splunkApp != null)
                splunkBasicInformationData.App = splunkApp;

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformationData.Name,
                SearchIndex = searchingIndex
            };

            for (int i = 0; i < splunkBasicInformationData.SplArgumentKeys.Count; i++)
            {
                savedSearchArgs.SavedSearchArgs.Add(splunkBasicInformationData.SplArgumentKeys[i], splunkBasicInformationData.SplArgumentValues[i]);
            }

            this.StartFetchingData(splunkBasicInformationData, SearchType.SplSearch, savedSearchArgs);
        }

        /// <summary>
        /// Data Play back 상황
        /// Splunk Table DataRow Click
        /// </summary>
        /// <param name="spl"></param>
        /// <param name="splunkApp"></param>
        /// <param name="invokingData"></param>
        public void StartFetchingDataFromSplSearch(string spl, string splunkApp, SplunkBasicInformationData invokingData, TimeSpan timeSpan, OperationPlaybackArgs operationPlaybackArgs)
        {
            var splunkBasicInformationData = new SplunkBasicInformationData(invokingData);

            splunkBasicInformationData.Name = spl;

            if (splunkApp != null)
                splunkBasicInformationData.App = splunkApp;

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformationData.Name,
                SearchIndex = searchingIndex,
                TimeLineTimeSpan = timeSpan,
                TimeLinePlayWay = SplunkSavedSearchArgs.PlayWayMode.Seek,
            };

            SplunkServiceHandler.ApplyOperationPlaybackArgs(savedSearchArgs, operationPlaybackArgs);

            for (int i = 0; i < splunkBasicInformationData.SplArgumentKeys.Count; i++)
            {
                savedSearchArgs.SavedSearchArgs.Add(splunkBasicInformationData.SplArgumentKeys[i], splunkBasicInformationData.SplArgumentValues[i]);
            }

            this.StartFetchingData(splunkBasicInformationData, SearchType.SplSearch, savedSearchArgs);
        }

        private enum SearchType { SavedSearch, SplSearch }

        private void StartFetchingData(SplunkBasicInformationData splunkBasicInformationData, SearchType searchType, SplunkSavedSearchArgs savedSearchArgs)
        {   
            try
            {
                var args = new SplunkServiceFactoryArgs
                {
                    Host = splunkBasicInformationData.IP,
                    Port = splunkBasicInformationData.Port,
                    App = splunkBasicInformationData.App,
                    UserName = splunkBasicInformationData.UserId,
                    UserPwd = splunkBasicInformationData.Password,
                };

                this.searchingIndex = Guid.NewGuid().ToString();
                savedSearchArgs.SearchIndex = this.searchingIndex;
               
                if (this.chartTableWrapperControl.StartFetchingData(splunkBasicInformationData))
                {
                    this.searchingService = SplunkServiceFactory.Instance.GetSplunkService(args);

                    var callbackAction = new Action<SplunkResultSet>(
                        (resultSet) =>
                        {
                            if (savedSearchArgs.SearchIndex != this.searchingIndex) return;

                            Application.Current.Dispatcher.Invoke(
                                new Action(() =>
                                {
                                    //var message = resultSet.SplunkException == null ? string.Empty : resultSet.SplunkException.Message;
                                    //this.chartTableWrapperControl.SetSplunkControlUI(resultSet.SplunkDataTable, message);
                                    this.chartTableWrapperControl.SetSplunkControlUI(resultSet);

                                    this.adjustTablePositionRequired = true;
                                }
                                )
                            );
                        }
                    );

                    if (searchType == SearchType.SavedSearch)
                    {
                        this.searchingService.BeginExecuteSavedSearch(callbackAction, savedSearchArgs);
                    }
                    else if (searchType == SearchType.SplSearch)
                    {
                        this.searchingService.BeginExecuteSplSearch(callbackAction, savedSearchArgs);
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void StopFetchingData()
        {
            if (this.searchingService == null
                || this.searchingIndex == null)
                return;

            this.searchingService.BeginExecuteAbortSearchThread(this.searchingIndex);
            this.searchingService = null;
        }
    }
}
