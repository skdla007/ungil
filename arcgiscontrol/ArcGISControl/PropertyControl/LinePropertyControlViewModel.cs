using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Styles;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.PropertyControl
{
    public class LinePropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region Fields

        private bool isSingleSetting = true;

        public bool IsSingleSetting
        {
            get { return this.isSingleSetting; }
            set
            {
                this.isSingleSetting = value;
                OnPropertyChanged("IsSingleSetting");
            }
        }

        private MapLineObjectDataInfo dataInfo;

        public MapLineObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                this.dataInfo = value;

                this.OnPropertyChanged("DataInfo");
            }
        }

        public string StrokeThickness
        {
            get { return this.dataInfo.StrokeThickness > 0 ? this.dataInfo.StrokeThickness.ToString() : "-"; }
            set
            {
                int i;

                if (!int.TryParse(value, out i)) return;
                this.OnPropertyChanged("StrokeThickness");
                this.dataInfo.StrokeThickness = i;
            }
        }

        public int SelectedStrokeIndex
        {
            get
            {
                for (int i = 0; i < this.LineStrokeTypes.Count ;i++)
                {
                    var style = this.LineStrokeTypes[i];

                    if(style.StrokeType == this.DataInfo.LineStrokeType)
                    {
                        return i;
                    }
                }

                return -1;
            }
            set
            {
                if (value < 0 || value >= this.LineStrokeTypes.Count) return;
                this.DataInfo.LineStrokeType = this.LineStrokeTypes[value].StrokeType;
                OnPropertyChanged("SelectedStrokeIndex");
            }
        }

        public List<LineStrokeStyle> LineStrokeTypes
        {
            get
            {
                var types = Enum.GetValues(typeof (LineStrokeType)).OfType<LineStrokeType>().ToList();
                return types.Select(type => new LineStrokeStyle(type)).ToList();
            }
        }

        public List<PenLineJoin> LineJoins
        {
            get { return Enum.GetValues(typeof(PenLineJoin)).OfType<PenLineJoin>().ToList(); }
        }

        public List<MapLineObjectDataInfo.LineCapTypes> LineCapTypes
        {
            get { return Enum.GetValues(typeof(MapLineObjectDataInfo.LineCapTypes)).OfType<MapLineObjectDataInfo.LineCapTypes>().ToList(); }
        }

        #endregion Fields
    }
}
