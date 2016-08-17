using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.Models
{
    public struct MapCertificationDataInfo
    {
        private string mapServiceUrl;
        private MapProviderType mapType;
        private string licenseKey;

        public string MapServiceUrl
        {
            get { return this.mapServiceUrl; }
        }

        public MapProviderType MapType
        {
            get { return this.mapType; }
        }

        public String LicenseKey
        {
            get { return licenseKey; }
        }

        public MapCertificationDataInfo(string mapServiceUrl, MapProviderType mapType, string licenseKey)
        {
            this.mapServiceUrl = mapServiceUrl;
            this.mapType = mapType;
            this.licenseKey = licenseKey;
        }
    }
}
