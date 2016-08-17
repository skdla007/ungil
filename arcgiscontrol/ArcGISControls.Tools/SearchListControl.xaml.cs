using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGISControl;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.Tools
{
    /// <summary>
    /// Interaction logic for SearchListControl.xaml
    /// </summary>
    public partial class SearchListControl : UserControl
    {
        #region Field

        private readonly SearchListControlViewModel viewModel;

        public ArcGISClientViewer ArcGisClientViewer
        {
            set { this.viewModel.ArcGisClientViewer = value; }
        }

        #endregion //Field

        #region Construction 

        public SearchListControl()
        {
            InitializeComponent();
            this.viewModel = new SearchListControlViewModel();
            this.DataContext = this.viewModel;
            this.Unloaded += OnUnloaded;
        }

        #endregion //Construction

        #region Methods

        public void ClearSearchList()
        {
            if (this.viewModel != null)
            {
                if (this.viewModel.MapAddressObjectDataInfos != null) this.viewModel.MapAddressObjectDataInfos.Clear();
                this.viewModel.SearchText = string.Empty;
                this.viewModel.SearchedCounts = "0";
            }
        }

        #endregion //Methods

        #region Event Handlers

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.viewModel != null)
            {
                this.ClearSearchList();
                this.viewModel.Dispose();
            }
        }
        
        /// <summary>
        /// Key 입력으로 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xTextBoxAddress_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if(e.Key  == Key.Enter)
            {
                this.viewModel.SearchGeoCoding();
            }
        }

        private void ToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;

            var data = button.DataContext as MapAddressObjectDataInfo;

            if (data == null)
                return;

            if (data.IsSaved)
                this.viewModel.SaveMapAddressObjectDataInfo(data);
            else
                this.viewModel.DeleteSavedMapAddressObjectDataInfo(data);
        }

        #endregion //Event Handlers
    }
}
