using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISControls.Tools
{
    using System.Windows.Input;
    using ArcGISControl;
    using ArcGISControls.CommonData.Models;
   
    /// <summary>
    /// PlaceListBoxControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlaceListControl : UserControl
    {
        private PlaceListControlViewModel viewModel;

        public ArcGISClientViewer ArcGisClientViewer
        {
            set { this.viewModel.ArcGisClientViewer = value; }
        }

        public int TotalItemCount
        {
            get { return this.xListBoxLocations.Items.Count; }
        }

        /// <summary>
        /// 리스트에서 아이템을 선택한 경우 발생시키는 이벤트
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> eSelectionChanged;

        public void OnESelectionChanged(SelectionChangedEventArgs e)
        {
            var handler = eSelectionChanged;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// ArcGIS Client View의 List를 받아 사용한다.
        /// </summary>
        public PlaceListControl(bool isEditMode)
        {
            InitializeComponent();

            this.viewModel = new PlaceListControlViewModel(isEditMode);
            this.DataContext = viewModel;
            this.Unloaded += PlaceListControl_Unloaded;
            this.xListBoxLocations.PreviewKeyUp += this.xListBoxLocations_PreviewKeyUp;
        }

        private void xListBoxLocations_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                this.viewModel.ArcGisClientViewer.DeleteSelectedObjects();
            }
        }

        void PlaceListControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.Dispose();
            }
        }

        public void ClearPlaceList()
        {
            if (this.viewModel != null)
            {
                this.viewModel.ClearPlaceList();
            }
        }

        public void RefreshPlaceList()
        {
            if (this.viewModel != null)
            {
                this.viewModel.RefreshPlaceList();
            }
        }


        private void ButtonxDeleteObjectOnClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;

            var data = cmd.DataContext as PlaceListItemModel;

            if (data != null)
            {
                this.viewModel.DeleteMapObjectData(data);
            }
        }

        private void Grid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || e.ClickCount != 2)
            {
                return;
            }

            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }

            var item = grid.DataContext as PlaceListItemModel;

            if (item == null)
            {
                return;
            }

            this.viewModel.GotoItem(item);
            e.Handled = true;
        }

        private void Grid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                return;
            }

            var selectedItems = this.xListBoxLocations.SelectedItems.Cast<PlaceListItemModel>().ToList();

            this.viewModel.SelectItems(selectedItems);

            this.OnESelectionChanged(new SelectionChangedEventArgs(selectedItems));
        }
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(List<PlaceListItemModel> aSelectedItems)
        {
            this.SelectedItems = aSelectedItems;
        }

        public List<PlaceListItemModel> SelectedItems { get; set; }
    }
}

