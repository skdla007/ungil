
namespace ArcGISControls.Tools
{
    using ArcGISControls.CommonData.Models;
    using DataChangedNotify;

    public class PlaceListItemModel : BaseModel
    {
        private BaseMapObjectInfoData objectData;
        private bool isSelected;

        public BaseMapObjectInfoData ObjectData
        {
            get { return objectData; }
            set
            {
                objectData = value;
                this.OnPropertyChanged("ObjectData");
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }
    }
}
