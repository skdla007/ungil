using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ArcGISControls.CommonData.Models.MapServicesDatas
{
    [DataContract]
    public class BingAuthentication
    {
        public const string ValidationCode = "ValidCredentials";

        [DataMember(Name = "authenticationResultCode")]
        public string AuthenticationResultCode { get; set; }

        [DataMember(Name = "errorDetails")]
        public string ErrorDetails { get; set; }
    }
}
