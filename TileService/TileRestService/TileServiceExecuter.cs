using Commons;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace TileRestService
{
    public class TileServiceExecuter : IDisposable
    {
        private readonly List<ServiceHost> hosts = new List<ServiceHost>();

        private bool hostOpenInfo = false;

        public TileServiceExecuter(string serviceUrl)
        {
            var tileServiceRestHostAddress = serviceUrl + "rest/tile/";

            var restServiceHost = new WebServiceHost(typeof(TileService), new Uri(tileServiceRestHostAddress));

            var webHttpBinding = new WebHttpBinding
            {
                CrossDomainScriptAccessEnabled = true,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferPoolSize = int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxArrayLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = int.MaxValue,
                    MaxNameTableCharCount = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                }
            };

            NetworkHelper.AddDiscovery(restServiceHost, typeof(TileService));

            var restServiceEndpoint = restServiceHost.AddServiceEndpoint(typeof(ITileService), webHttpBinding, new Uri(tileServiceRestHostAddress));
            restServiceEndpoint.Behaviors.Add(new WebHttpBehavior
            {
                HelpEnabled = true,
                AutomaticFormatSelectionEnabled = false,
            });


            hosts.Add(restServiceHost);

            Console.WriteLine("Start Tile Service : ");
            Console.WriteLine(" \t" + tileServiceRestHostAddress);

            this.Run();
        }

        /// <summary>
        ///Gets BaseAddresses.
        /// </summary>
        public List<Uri> BaseAddresses
        {
            get
            {
                if (hosts == null || hosts.Count == 0)
                {
                    return null;
                }

                var addresses = new List<Uri>();

                foreach (var serviceHost in hosts)
                {
                    addresses.AddRange(serviceHost.BaseAddresses);
                }

                return addresses;
            }
        }

        /// <summary>
        /// The close.
        /// </summary>
        public void Close()
        {
            if (hosts != null && hosts.Count != 0)
            {
                hosts.AsParallel().ForAll(host => host.Close());
                hosts.Clear();
            }
        }

        /// <summary>
        /// The run.
        /// </summary>
        public void Run()
        {
            try
            {
                if (hosts == null || hosts.Count == 0)
                {
                    throw new Exception("Run Server Fail. There are no host Exists.");
                }

                foreach (var serviceHost in hosts)
                {
                    try
                    {
                        serviceHost.Open();
                        Console.WriteLine("ServiceState : " + serviceHost.State);
                        serviceHost.Description.Endpoints.AsParallel()
                                   .ForAll(
                                       ep => Console.WriteLine(string.Format("Service is Running on {0}", ep.Address)));

                        this.hostOpenInfo = true;
                    }
                    catch (CommunicationException cex)
                    {
                        Console.WriteLine("ServiceState : " + serviceHost.State);
                        Console.WriteLine(cex.ToString());
                        serviceHost.Abort();
                        
                        hostOpenInfo = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            this.Close();
        }

        public bool HostOpenInfo()
        {
            return this.hostOpenInfo;
        }
    }
}