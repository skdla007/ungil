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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// ProcessingUserControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProcessingUserControl : UserControl
    {


        public ProcessingUserControl()
        {
            InitializeComponent();

            this.IsVisibleChanged += this.ProcessingUserControl_IsVisibleChanged;
        }

        private void ProcessingUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var storyBoard = (Storyboard)this.FindResource("sbLoading");
            if (storyBoard != null)
            {
                if ((bool)e.NewValue)
                {
                    if (ArcGISConstSet.QualityMode == ArcGISConstSet.QualitySettings.High)
                    {
                        storyBoard.Begin();
                    }
                }
                else
                {
                    storyBoard.Stop();
                }
            }
        }
    }
}
