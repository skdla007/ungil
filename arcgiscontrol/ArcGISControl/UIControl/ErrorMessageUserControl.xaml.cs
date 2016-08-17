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

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for ErrorMessageUserControl.xaml
    /// </summary>
    public partial class ErrorMessageUserControl : UserControl
    {
        public String ErrorMessage
        {
            set 
            {
                this.xLabelErrorMessage.Content = value;
            }
        }

        public ErrorMessageUserControl()
        {
            InitializeComponent();
        }
    }
}
