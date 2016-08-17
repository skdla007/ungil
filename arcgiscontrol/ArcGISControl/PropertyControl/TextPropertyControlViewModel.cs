using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.PropertyControl
{
    public class TextPropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private BaseMapTextObjectInfo dataInfo;

        public bool IsInitializeValues = false;

        public bool IsMultiSelected;

        public BaseMapTextObjectInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;

                this.dataInfo = value;

                this.OnPropertyChanged("DataInfo");
            }
        }

        private List<int> fontSizeList;
        public List<int> FontSizeList
        {
            get { return this.fontSizeList; }
            set
            {
                this.fontSizeList = value;
                OnPropertyChanged("FontSizeList");
            }
        }

        public TextPropertyControlViewModel()
        {

            if (this.FontSizeList == null)
            {
                this.FontSizeList = new List<int>();
                for (int i = 4; i < 400; i++)
                {
                    this.FontSizeList.Add(i);
                }
            }
        }
    }
}
