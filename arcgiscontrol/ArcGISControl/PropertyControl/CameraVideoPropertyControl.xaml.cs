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
    /// CameraVideoPropertyControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraVideoPropertyControl : UserControl
    {

        public CameraVideoPropertyControl()
        {
            InitializeComponent();
        }

        private void CheckBox_LockedSize_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as CameraVideoPropertyControlViewModel);
            if(viewModel != null)
            {
                if (viewModel.DataInfo.IsLockSize == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.IsLockSize = false;
                }
            }
        }

        private void CheckBox_ConstrainProportion_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as CameraVideoPropertyControlViewModel);
            if (viewModel != null)
            {
                if (viewModel.DataInfo.ConstrainProportion == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.ConstrainProportion = false;
                }
            }
        }

        private void CheckBox_AlwaysKeepToCameraVideo_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as CameraVideoPropertyControlViewModel);
            if (viewModel != null)
            {
                if (viewModel.DataInfo.AlwaysKeepToCameraVideo == null && viewModel.IsInitializeValues)
                {
                    viewModel.DataInfo.AlwaysKeepToCameraVideo = false;
                }
            }
        }
    }
}
