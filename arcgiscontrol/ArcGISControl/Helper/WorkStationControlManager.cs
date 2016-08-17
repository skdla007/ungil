using ArcGISControl.UIControl;
using ArcGISControls.CommonData.Models;
using Innotive.SplunkControl.Table;
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
    public class LinkedMapEventArgs:EventArgs
    {
        public string LinkedMapID { get; set; }
    }

    public class WorkStationEventArgs:EventArgs
    {
        public MapWorkStationObjectDataInfo DataInfo { get; set; }
    }

    public class WorkStationControlManager
    {
        public event EventHandler<LinkedMapEventArgs> onGoLinkedMap;

        public event EventHandler<WorkStationEventArgs> onShowSearchViewControl;

        private readonly WorkStationContextControl workStationContextControl;

        private WorkStationContextControlViewModel viewModel;

        private FrameworkElement showingElement;

        private Point showingMousePosition;

        private int zIndex;

        public int ZIndex
        {
            get { return this.zIndex; }
            set
            {
                this.zIndex = value;

                if (this.workStationContextControl != null && this.workStationContextControl.Parent != null)
                {
                    Panel.SetZIndex(this.workStationContextControl, value);
                }
            }
        }

        public WorkStationControlManager()
        {
            this.workStationContextControl = new WorkStationContextControl();
            this.workStationContextControl.IsVisibleChanged += WorkStationContextControlOnIsVisibleChanged;
            this.workStationContextControl.Visibility = Visibility.Hidden;
            this.viewModel = new WorkStationContextControlViewModel();
            this.workStationContextControl.DataContext = this.viewModel;

            this.viewModel.onGoLinkedMap += ViewModelOnGoLinkedMap;
            this.viewModel.onShowSearchViewControl += ViewModelOnShowSearchViewControl;
        }

        private void WorkStationContextControlOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            AdjustTablePosition();
        }

        public void AddToParent(Panel parent)
        {
            parent.Children.Add(this.workStationContextControl);
            Panel.SetZIndex(this.workStationContextControl, this.zIndex);
        }

        public void RemoveFromParent(Panel parent)
        {
            this.showingElement = parent;
            parent.Children.Remove(this.workStationContextControl);
        }

        private void AdjustTablePosition()
        {
            var control = this.workStationContextControl;
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

        public void Show(MapWorkStationObjectDataInfo dataInfo)
        {
            this.showingMousePosition = Mouse.GetPosition(this.showingElement);
            this.viewModel.DataInfo = dataInfo;

            this.workStationContextControl.Visibility = Visibility.Visible;

            this.AdjustTablePosition();
        }

        public void Hide()
        {
            this.workStationContextControl.Visibility = Visibility.Collapsed;
        }

        #region Methods

        private void RaiseGoLinkedMap(string linkedMapGuid)
        {
            var e = this.onGoLinkedMap;
            if (e != null)
                e(this, new LinkedMapEventArgs
                {
                    LinkedMapID = linkedMapGuid
                });
        }

        private void RaiseShowSearchViewControl(MapWorkStationObjectDataInfo dataInfo)
        {
            var e = this.onShowSearchViewControl;
            if (e != null)
                e(this, new WorkStationEventArgs
                {
                    DataInfo = dataInfo
                });
            
        }

        public void Dispose()
        {
            if(this.viewModel != null)
            {
                this.viewModel.onGoLinkedMap -= ViewModelOnGoLinkedMap;
                this.viewModel.onShowSearchViewControl -= ViewModelOnShowSearchViewControl;
            }
        }

        #endregion //Methods

        #region Event Handlers

        private void ViewModelOnShowSearchViewControl(object sender, WorkStationEventArgs workStationEventArgs)
        {
            this.RaiseShowSearchViewControl(workStationEventArgs.DataInfo);
        }

        private void ViewModelOnGoLinkedMap(object sender, LinkedMapEventArgs linkedMapEventArgs)
        {
            this.RaiseGoLinkedMap(linkedMapEventArgs.LinkedMapID);
        }

        #endregion Event Handlers
    }
}
