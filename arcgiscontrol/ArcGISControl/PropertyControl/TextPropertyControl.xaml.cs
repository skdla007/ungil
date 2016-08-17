using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISControl.PropertyControl
{
    /// <summary>
    /// Interaction logic for TextPropertyControl.xaml
    /// </summary>
    public partial class TextPropertyControl : UserControl
    {
        public TextPropertyControl()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked_IsBold(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as TextPropertyControlViewModel);
            if (viewModel != null)
            {
                if (viewModel.DataInfo.IsBold == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.IsBold = false;
                }
            }
        }

        private void CheckBox_Checked_IsItalic(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as TextPropertyControlViewModel);
            if (viewModel != null)
            {
                if (viewModel.DataInfo.IsItalic == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.IsItalic = false;
                }
            }
        }

        private void CheckBox_Checked_IsUnderline(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as TextPropertyControlViewModel);
            if (viewModel != null)
            {
                if (viewModel.DataInfo.IsUnderline == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.IsUnderline = false;
                }
            }
        }

    }
}
