
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for UniversalObjectPropertyControl.xaml
    /// </summary>
    public partial class UniversalObjectPropertyControl : UserControl
    {
        private UniversalObjectPropertyControlViewModel viewModel;

        public UniversalObjectPropertyControl()
        {
            InitializeComponent();

            this.DataContextChanged += UniversalObjectPropertyControl_DataContextChanged;

            var sd = new SortDescription("Name", ListSortDirection.Ascending);

            this.xLinkMapComboBox.Items.SortDescriptions.Add(sd);
            this.xLinkBookmarkComboBox.Items.SortDescriptions.Add(sd);
            this.xLinkObjectComboBox.Items.SortDescriptions.Add(sd);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            //if (string.Compare(e.Property.Name, "MapSettingInfoDatas") == 0)
            //{
            //    var view = CollectionViewSource.GetDefaultView(this.xLinkMapComboBox.ItemsSource);
            //    if (view == null)
            //        return;

            //    view.Refresh();
            //}
        }

        private void UniversalObjectPropertyControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            this.viewModel = this.DataContext as UniversalObjectPropertyControlViewModel;

            if (this.viewModel == null)
            {
                return;
            }

            // event handler
            this.xTitleMinX.ValueChanged -= this.TitleMin_ValueChanged;
            this.xTitleMinY.ValueChanged -= this.TitleMin_ValueChanged;

            this.xTitleMaxX.ValueChanged -= this.TitleMax_ValueChanged;
            this.xTitleMaxY.ValueChanged -= this.TitleMax_ValueChanged;

            this.xIconMinX.ValueChanged -= this.IconMin_ValueChanged;
            this.xIconMinY.ValueChanged -= this.IconMin_ValueChanged;

            this.xIconMaxX.ValueChanged -= this.IconMax_ValueChanged;
            this.xIconMaxY.ValueChanged -= this.IconMax_ValueChanged;

            this.xLampX.ValueChanged -= this.LampPosition_ValueChanged;
            this.xLampY.ValueChanged -= this.LampPosition_ValueChanged;

            this.xRadioButtonRectangle.Checked -= this.xRadioButtonRectangle_Checked;
            this.xRadioButtonImage.Checked -= this.xRadioButtonRectangle_Checked;
            this.xRadioButtonVerticalPipe.Checked -= this.xRadioButtonRectangle_Checked;
            this.xRadioButtonHorizontalPipe.Checked -= this.xRadioButtonRectangle_Checked;

            // default setting
            this.xTitleMinX.Value = (int)this.viewModel.DataInfo.TitleMinPosition.X;
            this.xTitleMinY.Value = (int)this.viewModel.DataInfo.TitleMinPosition.Y;

            this.xTitleMaxX.Value = (int)this.viewModel.DataInfo.TitleMaxPosition.X;
            this.xTitleMaxY.Value = (int)this.viewModel.DataInfo.TitleMaxPosition.Y;

            this.xIconMinX.Value = (int)this.viewModel.DataInfo.IconMinPosition.X;
            this.xIconMinY.Value = (int)this.viewModel.DataInfo.IconMinPosition.Y;

            this.xIconMaxX.Value = (int)this.viewModel.DataInfo.IconMaxPosition.X;
            this.xIconMaxY.Value = (int)this.viewModel.DataInfo.IconMaxPosition.Y;

            this.xLampX.Value = (int)this.viewModel.DataInfo.AlarmLampPosition.X;
            this.xLampY.Value = (int)this.viewModel.DataInfo.AlarmLampPosition.Y;

            // event handler
            this.xTitleMinX.ValueChanged += this.TitleMin_ValueChanged;
            this.xTitleMinY.ValueChanged += this.TitleMin_ValueChanged;

            this.xTitleMaxX.ValueChanged += this.TitleMax_ValueChanged;
            this.xTitleMaxY.ValueChanged += this.TitleMax_ValueChanged;

            this.xIconMinX.ValueChanged += this.IconMin_ValueChanged;
            this.xIconMinY.ValueChanged += this.IconMin_ValueChanged;

            this.xIconMaxX.ValueChanged += this.IconMax_ValueChanged;
            this.xIconMaxY.ValueChanged += this.IconMax_ValueChanged;

            this.xLampX.ValueChanged += this.LampPosition_ValueChanged;
            this.xLampY.ValueChanged += this.LampPosition_ValueChanged;

            this.xRadioButtonRectangle.Checked += this.xRadioButtonRectangle_Checked;
            this.xRadioButtonImage.Checked += this.xRadioButtonRectangle_Checked;
            this.xRadioButtonVerticalPipe.Checked += this.xRadioButtonRectangle_Checked;
            this.xRadioButtonHorizontalPipe.Checked += this.xRadioButtonRectangle_Checked;
        }

        private void xRadioButtonRectangle_Checked(object sender, RoutedEventArgs e)
        {
            switch (this.viewModel.DataInfo.ShapeType)
            {
                case MapUniversalObjectDataInfo.ShapeTypes.Rectangle:
                    this.xStackPanelRadius.Visibility = Visibility.Visible;
                    this.xStackPanelImageUrl.Visibility = Visibility.Collapsed;
                    this.xStackPanelFill.Visibility = Visibility.Visible;
                    break;
                case MapUniversalObjectDataInfo.ShapeTypes.Image:
                    this.xStackPanelRadius.Visibility = Visibility.Visible;
                    this.xStackPanelImageUrl.Visibility = Visibility.Visible;
                    this.xStackPanelFill.Visibility = Visibility.Collapsed;
                    break;

                case MapUniversalObjectDataInfo.ShapeTypes.VerticalPipe:
                case MapUniversalObjectDataInfo.ShapeTypes.HorizontalPipe:
                    this.xStackPanelRadius.Visibility = Visibility.Collapsed;
                    this.xStackPanelImageUrl.Visibility = Visibility.Collapsed;
                    this.xStackPanelFill.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void TitleMin_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.DataInfo.TitleMinPosition = new Point(this.xTitleMinX.Value, this.xTitleMinY.Value);
        }

        private void TitleMax_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.DataInfo.TitleMaxPosition = new Point(this.xTitleMaxX.Value, this.xTitleMaxY.Value);
        }

        private void IconMin_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.DataInfo.IconMinPosition = new Point(this.xIconMinX.Value, this.xIconMinY.Value);
        }

        private void IconMax_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.DataInfo.IconMaxPosition = new Point(this.xIconMaxX.Value, this.xIconMaxY.Value);
        }

        private void LampPosition_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                return;
            }

            this.viewModel.DataInfo.AlarmLampPosition = new Point(this.xLampX.Value, this.xLampY.Value);
        }
    }
}
