using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGISControl.Helper;

namespace ArcGISControls.Tools.SearchViewControl
{
    public class SearchViewControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region Fields

        private bool isSearchViewButtonVisible;
        
        private bool isTrendAnalysisButtonVisible;

        #endregion

        #region Properties

        public bool IsSearchViewButtonVisible
        {
            get { return this.isSearchViewButtonVisible; }
            set
            {
                this.isSearchViewButtonVisible = value;
                this.OnPropertyChanged("IsSearchViewButtonVisible");
            }
        }

        public bool IsTrendAnalysisButtonVisible
        {
            get { return this.isTrendAnalysisButtonVisible; }
            set
            {
                this.isTrendAnalysisButtonVisible = value;
                this.OnPropertyChanged("IsTrendAnalysisButtonVisible");
            }
        }

        #endregion
    }
}
