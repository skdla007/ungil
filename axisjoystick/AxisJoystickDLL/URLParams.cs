using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AxisJoystick
{
    /// <summary>
    /// The camera information.
    /// </summary>
    [DataContract]
    public class URLParams
    {
        /// <summary>
        /// Gets or sets DllName.
        /// </summary>
        [DataMember]
        public string dllname { get; set; }

        /// <summary>
        /// Gets or sets address.
        /// </summary>
        [DataMember]
        public string address { get; set; }

        /// <summary>
        /// Gets or sets ID.
        /// </summary>
        [DataMember]
        public string username { get; set; }

        /// <summary>
        /// Gets or sets PW.
        /// </summary>
        [DataMember]
        public string password { get; set; }

        /// <summary>
        /// Gets or sets Channel.
        /// </summary>
        [DataMember]
        public string channel { get; set; }

        /// <summary>
        /// Gets or sets BaudRate.
        /// </summary>
        [DataMember]
        public string BaudRate { get; set; }

        /// <summary>
        /// Gets or sets DataBits.
        /// </summary>
        [DataMember]
        public string DataBits { get; set; }

        /// <summary>
        /// Gets or sets Parity.
        /// </summary>
        [DataMember]
        public string Parity { get; set; }

        /// <summary>
        /// Gets or sets StopBits.
        /// </summary>
        [DataMember]
        public string StopBits { get; set; }

        /// <summary>
        /// Gets or sets SerialPort.
        /// </summary>
        [DataMember]
        public string SerialPort { get; set; }

        /// <summary>
        /// Gets or sets ActionTime.
        /// </summary>
        [DataMember]
        public string ActionTime { get; set; }

        /// <summary>
        /// Gets or sets IntervalTime.
        /// </summary>
        [DataMember]
        public string IntervalTime { get; set; }

        /// <summary>
        /// Gets or sets State.
        /// </summary>
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets TargetPoint.
        /// </summary>
        [DataMember]
        public string TargetPoint { get; set; }

        /// <summary>
        /// Gets or sets GroupSelectData.
        /// </summary>
        [DataMember]
        public string GroupSelectData { get; set; }

        /// <summary>
        /// Gets or sets IpPort.
        /// </summary>
        [DataMember]
        public string IpPort { get; set; }

        /// <summary>
        /// Gets or sets sid.
        /// </summary>
        [DataMember]
        public string sid { get; set; }

        /// <summary>
        /// The to parameter.
        /// </summary>
        /// <returns>
        /// The to parameter.
        /// </returns>
        public string ToParameter()
        {
            var sb = new StringBuilder();
            var returnProperties = this.GetType().GetProperties();

            foreach (var returnProperty in returnProperties)
            {
                var value = returnProperty.GetValue(this, null) as string;

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                sb.Append(returnProperty.Name.ToLower());
                sb.Append("=");
                sb.Append(value);
                sb.Append("&");

            }

            return sb.ToString().Trim('&');
        }
    }
}
