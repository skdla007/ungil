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

namespace ArcGISControls.Tools.PostItControl
{
    /// <summary>
    /// Interaction logic for PostItPopupControl.xaml
    /// </summary>
    public partial class PostItPopupControl : UserControl
    {
        private PostItPopupControlViewModel viewModel;

        public PostItPopupControl()
        {
            InitializeComponent();
            this.viewModel = new PostItPopupControlViewModel();
            this.DataContext = this.viewModel;
        }

        public PostItPopupControlViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        public void ShowPopupBody()
        {
            this.xTitlePanel.Visibility = Visibility.Visible;
            this.xBodyPanel.Visibility = Visibility.Visible;
            this.xFooterPanel.Visibility = Visibility.Visible;
        }

        public void HidePopupBody()
        {
            this.xTitlePanel.Visibility = Visibility.Collapsed;
            this.xBodyPanel.Visibility = Visibility.Collapsed;
            this.xFooterPanel.Visibility = Visibility.Collapsed;
        }

        private void XHideButton_OnClick(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle == null)
            {
                return;
            }

            if (!toggle.IsChecked.HasValue)
            {
                toggle.IsChecked = false;
            }

            if ((bool) toggle.IsChecked)
            {
                this.HidePopupBody();
            }
            else
            {
                this.ShowPopupBody();
            }
        }
    }
}
