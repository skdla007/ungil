using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.MapTileImageProxy
{
    using System.IO;
    using System.Net;

    internal class ImageCache
    {
        private object imagesLock;
        private CacheQueue<string, Stream> images;

        public ImageCache()
        {
            this.images = new CacheQueue<string, Stream>();
            this.imagesLock = new object();
        }

        /// <summary>
        /// WebException 나올수 있음
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Stream GetImageStream(string url)
        {
            lock (imagesLock)
            {
                Stream tempStream;
                var memoryStream = new MemoryStream();

                if (!this.images.TryGetValue(url, out tempStream))
                {
                    var webRequest = WebRequest.CreateDefault(new Uri(url));
                    ((HttpWebRequest)webRequest).UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                    var webResponse = webRequest.GetResponse();
                    
                    var stream = webResponse.GetResponseStream();
                    tempStream = new MemoryStream();
                    if (stream == null)
                    {
                        throw new WebException("No content", WebExceptionStatus.ReceiveFailure);
                    }

                    stream.CopyTo(tempStream);
                    tempStream.Position = 0;
                    this.images.Add(url, tempStream);
                }

                tempStream.CopyTo(memoryStream);
                tempStream.Position = 0;
                return memoryStream;
            }
        }
    }
}