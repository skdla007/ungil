

namespace Network
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Discovery;
    using System.Xml.Linq;

    /// <summary>
    /// The network helper.
    /// </summary>
    public class NetworkHelper
    {
        /// <summary>
        /// The xelement.
        /// 현재 컴퓨터 주소들을 XML 형식으로 반환 함.
        /// </summary>
        /// <returns>
        /// The <see cref="XElement"/>.
        /// </returns>
        public static XElement GetLocalAddresses()
        {
            var root = new XElement("Addresses");

            IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (var item in address)
            {
                root.Add(new XElement("Address", item.ToString()));
            }

            return root;
        }

        /// <summary>
        /// The add discovery.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        public static void AddDiscovery(ServiceHost host, Type serviceType)
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return;
            }
            // Add a ServiceDiscoveryBehavior                
            host.Description.Behaviors.Add(new ServiceDiscoveryBehavior());

            // Add a UdpDiscoveryEndpoint
            var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
            host.AddServiceEndpoint(udpDiscoveryEndpoint);

            // Add a discovery behavior for Meta Extensions
            EndpointDiscoveryBehavior endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
            endpointDiscoveryBehavior.Extensions.Add(GetLocalAddresses());
            host.Description.Endpoints[0].Behaviors.Add(endpointDiscoveryBehavior);
        }

        /// <summary>
        /// The check url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The url string.
        /// </returns>
        public static string CheckUrl(string url)
        {
            var val = url;

            if (!url.EndsWith("/"))
            {
                val = val + "/";
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                val = "http://" + val;
            }

            return val;
        }

        /// <summary>
        /// IP 주소가 *이라면 스스로가 사용하기 위해 사용할 IP로 변환해주는 함수
        /// </summary>
        /// <param name="inputAddress"></param>
        /// <returns></returns>
        public static string SubstituteGeneralAddress(string inputAddress, string requestUri = "127.0.0.1")
        {
            if (string.CompareOrdinal(inputAddress, "*") == 0)
            {
                return requestUri;
            }
            return inputAddress;
        }
    }
}
