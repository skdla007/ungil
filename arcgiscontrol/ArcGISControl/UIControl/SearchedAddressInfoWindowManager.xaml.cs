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
using ArcGISControl.GraphicObject;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// Interaction logic for SearchedAddressInfoWindowManager.xaml
    /// </summary>
    public partial class SearchedAddressInfoWindowManager : InfoWindowManager
    {
        //Save
        public event EventHandler<ObjectEventArgs> eSearchedAddressSaveButtonClick;

        private void RaiseSearchedAddressSaveButtonClickEvent(string objectId, MapObjectType type)
        {
            var handler = this.eSearchedAddressSaveButtonClick;
            if (handler != null)
            {
                handler(this, new ObjectEventArgs(objectId, type));
            }
        }

        public SearchedAddressInfoWindowManager(Map map)
            : base(map)
        {  
            InitializeComponent();

            locationInfoWindow.ContentTemplate =
               this.Resources["InfoWindowTemplate"] as System.Windows.DataTemplate;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.locationInfoWindow.IsOpen = false;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.mapLocationObjectData == null) return;

            this.RaiseSearchedAddressSaveButtonClickEvent(this.mapLocationObjectData.ObjectID, this.mapLocationObjectData.ObjectType);
        }

        public override void ShowInfoWindow(MapLocationObjectDataInfo mapLocationObjectData, MapPoint point)
        {
            base.ShowInfoWindow(mapLocationObjectData, point);

            var datatable = new Dictionary<string, object>();

            var addressObjectData = mapLocationObjectData as MapAddressObjectDataInfo;

            if (addressObjectData == null) return;

            datatable["Address"] = addressObjectData.Address;
            datatable["Name"] = addressObjectData.Name;
            datatable["IsNotSaved"] = !addressObjectData.IsSaved;

            this.locationInfoWindow.Content = datatable;
            this.locationInfoWindow.IsOpen = true;
        }
    }
}
