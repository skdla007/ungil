using System;
using System.Windows;
using System.Windows.Input;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.Tools
{
    public class BaseBookMarkRegisterWindow : Window
    {
        protected MapObjectRegisterWindowViewModel viewModel;

        public MapBookMarkDataInfo MapBookMarkData
        {
            get
            {  
                return this.viewModel.MapBookMarkData;
            }
        }

        public MapLocationObjectDataInfo MapLocationObjectData
        {
            get { return this.viewModel.MapLocationObjectData; }
        }

        public event EventHandler<EventArgs> onButtonOkClick;
        public event EventHandler<EventArgs> onButtonCancelClick;

        public BaseBookMarkRegisterWindow(Window window)
        {   
            this.Owner = window;
            
            this.viewModel = new MapObjectRegisterWindowViewModel();
            this.viewModel.MapBookMarkData = new MapBookMarkDataInfo();

            this.viewModel.onButtonOkClick += ViewModelOnOnButtonOkClick;
            this.viewModel.onButtonCancelClick += ViewModelOnOnButtonCancelClick;
            this.DataContext = this.viewModel;

            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            if(this.viewModel != null)
            {
                this.viewModel.onButtonOkClick -= ViewModelOnOnButtonOkClick;
                this.viewModel.onButtonCancelClick -= ViewModelOnOnButtonCancelClick;
            }
        }

        protected void ViewModelOnOnButtonOkClick(object sender, EventArgs eventArgs)
        {   
            if (this.onButtonOkClick != null)
                this.onButtonOkClick(this, null);
        }

        protected void ViewModelOnOnButtonCancelClick(object sender, EventArgs eventArgs)
        {
            if (this.onButtonCancelClick != null)
                this.onButtonCancelClick(this, null);
        }

        protected void Border_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void SetData(BaseMapObjectInfoData data)
        {
            if (data is MapBookMarkDataInfo)
                this.viewModel.MapBookMarkData = data as MapBookMarkDataInfo;
            else if (data is MapLocationObjectDataInfo)
                this.viewModel.MapLocationObjectData = data as MapLocationObjectDataInfo;
        }
    }
}
