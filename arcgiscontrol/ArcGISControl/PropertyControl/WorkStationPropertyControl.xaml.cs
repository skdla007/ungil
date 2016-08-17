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
    /// Interaction logic for WorkStationPropertyControl.xaml
    /// </summary>
    public partial class WorkStationPropertyControl : UserControl
    {
        public WorkStationPropertyControl()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var cmd = (TextBox)sender;

            var data = cmd.DataContext as SplunkArgumentItem;

            var viewModel = this.DataContext as WorkStationPropertyControlViewModel;

            int i = 0;
            foreach (var key in viewModel.DataInfo.SplunkBasicInformation.SplArgumentKeys)
            {
                if (key == data.SplunkArgumentKey)
                {
                    viewModel.DataInfo.SplunkBasicInformation.SplArgumentValues[i] = data.SplunkArgumentValue;
                }

                i++;
            }
        }
    }
}
