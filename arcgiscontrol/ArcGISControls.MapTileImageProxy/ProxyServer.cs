using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISControls.MapTileImageProxy
{
    using System.Net;
    using System.Threading;

    public class ProxyServer
    {
        private HttpListener listener;
        private readonly ImageCache imageCache;
        private readonly String defaultProxsyServerUrl = "http://localhost:{0}/tileproxy/{1}/";
        public string ProxyServerUrl { get; private set; }

        public bool IsStarted
        {
            get { return this.listener != null && this.listener.IsListening; }
        }

        public ProxyServer()
        {
            this.imageCache = new ImageCache();
        }

        public void Start(int port)
        {
            if (this.listener != null)
            {
                throw new InvalidOperationException();
            }

            for (int id = 0 ;; id++ )
            {
                try
                {
                    this.ProxyServerUrl = string.Format(this.defaultProxsyServerUrl, port, id);
                    this.listener = new HttpListener();
                    this.listener.Prefixes.Add(this.ProxyServerUrl);
                    this.listener.Start();
                    break;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode != 183)
                    {
                        throw;
                    }
                }

                if (id > 10)
                {
                    throw new HttpListenerException(183);
                }

            }

            var threadStart = new ParameterizedThreadStart(Work);
            var thread = new Thread(threadStart);
            thread.IsBackground = true;
            thread.Start(this.listener);
        }

        public void Stop()
        {
            if(this.listener == null) return;

            this.listener.Abort();
            this.listener = null;
        }

        private void Work(object param)
        {
            for(;;)
            {
                var listener = (HttpListener)param;

                try
                {
                    var context = listener.GetContext();

                    Task.Factory.StartNew(() =>
                    {
                        var request = context.Request;
                        var response = context.Response;
                        var rawUrl = request.RawUrl;

                        try
                        {
                            var url = Uri.UnescapeDataString(rawUrl.Substring(rawUrl.IndexOf('?') + 1));
                            var imageStream = imageCache.GetImageStream(url);
                            response.ContentLength64 = imageStream.Length;
                            imageStream.Position = 0;
                            imageStream.CopyTo(response.OutputStream);
                        }
                        catch (WebException webException)
                        {
                            try
                            {
                                if (webException.Status == WebExceptionStatus.ProtocolError &&
                                webException.Response != null)
                                {
                                    response.ContentLength64 = 0;
                                    response.StatusCode = (int)((HttpWebResponse)webException.Response).StatusCode;
                                    response.StatusDescription = ((HttpWebResponse)webException.Response).StatusDescription;
                                }
                                else
                                {
                                    this.ReportInternalServerError(response);
                                }
                            }
                            catch (Exception)
                            {
                                this.ReportInternalServerError(response);
                            }
                        }
                        catch (Exception exception)
                        {
                            this.ReportInternalServerError(response);
                        }

                        response.OutputStream.Close();

                    },
                    TaskCreationOptions.LongRunning).ContinueWith((task) =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("N Branching Statement Processing - if (task.IsFaulted)");
                            //task의 error
                        }
                    });
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch(HttpListenerException)
                {
                    if (!listener.IsListening) break;

                    throw;
                }
            }
        }

        private void ReportInternalServerError(HttpListenerResponse response)
        {
            response.ContentLength64 = 0;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.StatusDescription = "Internal Server Error";
        }

        #region Singleton member

        public static ProxyServer Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ProxyServer instance = new ProxyServer();
        }

        #endregion
    }
}
