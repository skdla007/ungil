using System;
using System.ServiceModel;
using System.Configuration;
using TileRestService;
using Commons;

namespace TileConsoleService
{
    public class ConsoleService
    {
        private static TileServiceExecuter tileServiceExecuter;

        public static void Main(string[] args)
        {

            ServiceTypeInfo.SetTypeInfo(RunType.ConsoleService);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                string url = ConfigurationManager.AppSettings["MapServiceUrl"];
                tileServiceExecuter = new TileServiceExecuter(url);

                if (tileServiceExecuter.HostOpenInfo())
                {
                    if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        Console.WriteLine("Unix!!!");
                    }
                    else
                    {
                        Console.ReadLine();
                    }
                }

                Console.WriteLine("Console Service Final");
            }
            catch (CommunicationException cex)
            {
                Console.WriteLine(string.Format("An exception occurred : {0}", cex.Message));
                tileServiceExecuter.Dispose();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            Console.WriteLine(String.Format("UnhandledException => ", exception.ToString()));
        }
    }
}
