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

namespace ArcGISControls.Tools.PostItControl
{
    /// <summary>
    /// Interaction logic for EditEventPopupControl.xaml
    /// </summary>
    public partial class EditEventPopupControl : UserControl
    {
        private EditEventPopupControlViewModel viewModel;

        public EditEventPopupControl()
        {
            this.viewModel = new EditEventPopupControlViewModel();
            this.DataContext = this.viewModel;

            InitializeComponent();
        }

        public EditEventPopupControlViewModel ViewModel
        {
            get { return this.viewModel; }
        }
    }
}
