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

namespace ArcGISControl.PropertyControl
{
    /// <summary>
    /// SplunkPropertyControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplunkPropertyControl : UserControl
    {

        public SplunkPropertyControl()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var cmd = (TextBox)sender;

            var data = cmd.DataContext as SplunkArgumentItem;

            var viewModel = this.DataContext as SplunkPropertyControlViewModel;

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
