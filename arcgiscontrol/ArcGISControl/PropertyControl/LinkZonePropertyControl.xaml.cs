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
using ArcGISControls.CommonData.Models;
using Innotive.SplunkManager.SplunkManager;

namespace ArcGISControl.PropertyControl
{
    /// <summary>
    /// Interaction logic for LinkZonePropertyControl.xaml
    /// </summary>
    public partial class LinkZonePropertyControl : UserControl
    {
        public LinkZonePropertyControl()
        {
            InitializeComponent();
        }

        private void TextBoxColor_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var cmd = (TextBox)sender;

            var data = cmd.DataContext as SplunkArgumentItem;

            var viewModel = this.DataContext as LinkZonePropertyControlViewModel;

            if (viewModel == null) return;

            int i = 0;
            
            foreach (var key in viewModel.DataInfo.ColorSplunkBasicInformationData.SplArgumentKeys)
            {
                if (key == data.SplunkArgumentKey)
                {
                    viewModel.DataInfo.ColorSplunkBasicInformationData.SplArgumentValues[i] = data.SplunkArgumentValue;
                }

                i++;
            }
        }

        private void TextBoxTable_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var cmd = (TextBox)sender;

            var data = cmd.DataContext as SplunkArgumentItem;

            var viewModel = this.DataContext as LinkZonePropertyControlViewModel;

            if(viewModel == null) return;

            int i = 0;

            foreach (var key in viewModel.DataInfo.TableSplunkBasicInformationData.SplArgumentKeys)
            {
                if (key == data.SplunkArgumentKey)
                {
                    viewModel.DataInfo.TableSplunkBasicInformationData.SplArgumentValues[i] = data.SplunkArgumentValue;
                }

                i++;
            }
        }
    }
}
