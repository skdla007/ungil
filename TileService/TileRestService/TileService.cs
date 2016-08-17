using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace TileRestService
{
    using Commons;
    using Commons.Debug;
    using Commons.Maps;
    using System.Web;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TileService : ITileService
    {
        #region Fields
        private const string FileShareConfigName = "UseFileShare";

        private const string ExceptionFolderConfigName = "FolderNameWithoutFileShare";

        private string[] sep = { "/" };

        private readonly string StorageFullPath =
    Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
        ConfigurationManager.AppSettings["MapStoragePath"]
    ));
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TileService"/> class.
        /// </summary>
        public TileService()
        {
            try
            {
                Console.WriteLine("Construction Start");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.WriteErrorLog(ex.ToString());
            }
        }

        #region GET
        /// <summary>
        /// Get Tile Resource
        /// </summary>
        /// <returns></returns>
        public Stream Get()
        {
            Console.WriteLine("GET Start");

            //return null;
            using (var consolePrinter = new MapServiceMessage())
            {
                try
                {
                    //Url 에서 ZoomLevel, Row, Col 에 대한 정보를 가져온다.
                    var requestPath = this.GetRequestPath();

                    string message = String.Format("[Time : {0}] \nGET => {1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.FFF"), requestPath);
                    consolePrinter.PrintMessage(message);

                    //해당되는 타일 Resource를 가져온다
                    MemoryStream stream = this.LoadResource(requestPath);

                    if (stream == null) // Path에 해당되는 타일이 없는경우 404 Not Found
                    {
                        throw new HttpException(404, "HTTP/1.1 404 Not Found");
                    }
                    WebOperationContext.Current.OutgoingResponse.ContentType = "image/png";
                    //Cross Domain Header 정의
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                    return stream;
                    // 이전 방식 사용
                    //return this.LoadResource(requestPath);
                }
                catch (Exception ex)
                {
                    consolePrinter.PrintExceptionMessage(ex);
                    throw;
                }
            }
        }
        #endregion

        #region PUT
        /// <summary>
        /// PUT
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream Put(Stream stream)
        {
            using (var consolePrinter = new MapServiceMessage())
            {
                try
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";

                    var requestPath = this.GetRequestPath();
                    string message = String.Format("PUT => {0}", requestPath);
                    consolePrinter.PrintMessage(message);
                    Console.WriteLine(message);

                    if (System.String.CompareOrdinal("true", ConfigurationManager.AppSettings[FileShareConfigName].ToLower()) == 0)
                    {
                        // 레퍼런스 카운팅 사용

                        var items = requestPath.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                        if (items.Length != 2)
                        {
                            throw new ArgumentException("Request path is invalid.");
                        }

                        if (System.String.CompareOrdinal(items[0].ToLower(), ConfigurationManager.AppSettings[ExceptionFolderConfigName].ToLower()) == 0)
                        {
                            // 예외 폴더
                            //this.SaveResource(requestPath, stream);
                        }

                        return CommonUtil.ResponseData(ResponseType.Success);
                    }
                    else
                    {
                        // 이전 방식 사용
                        this.SaveResource(requestPath, stream);
                    }

                    return CommonUtil.ResponseData(ResponseType.Success);
                }
                catch (Exception ex)
                {
                    consolePrinter.PrintExceptionMessage(ex);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return CommonUtil.ResponseData(ResponseType.Failed, ex.ToString());
                }
            }
        }
        #endregion

        #region DELETE
        public Stream Delete()
        {
            using (var consolePrinter = new MapServiceMessage())
            {
                try
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";

                    var requestPath = this.GetRequestPath();

                    string message = String.Format("DELETE => {0}", requestPath);
                    consolePrinter.PrintMessage(message);
                    Console.WriteLine(message);

                    if (System.String.CompareOrdinal("true", ConfigurationManager.AppSettings[FileShareConfigName].ToLower()) == 0)
                    {
                        // 레퍼런스 카운팅 사용
                        var items = requestPath.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                        if (System.String.CompareOrdinal(items[0].ToLower(), ConfigurationManager.AppSettings[ExceptionFolderConfigName].ToLower()) == 0)
                        {
                            // 예외 폴더
                            this.DeleteResource(requestPath);
                        }
                    }
                    else
                    {
                        // 기존 방식
                        this.DeleteResource(requestPath);
                    }

                    return CommonUtil.ResponseData(ResponseType.Success);
                }
                catch (Exception ex)
                {
                    consolePrinter.PrintExceptionMessage(ex);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return CommonUtil.ResponseData(ResponseType.Failed, ex.ToString());
                }
            }
        }
        #endregion

        #region CLONE
        /// <summary>
        /// dest 폴더를 생성하고 src 폴더의 모든 파일에 대해 dest 폴더에 하드 링크를 만든다.
        /// </summary>
        /// <param name="src">소스 폴더</param>
        /// <param name="dest">타켓 폴더</param>
        /// <returns></returns>
        public Stream Clone(string src, string dest)
        {
            using (var consolePrinter = new MapServiceMessage())
            {
                try
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";

                    string message = String.Format(@"CLONE - src : /{0}  dest : /{1}", src, dest);
                    consolePrinter.PrintMessage(message);
                    Console.WriteLine(message);

                    if (System.String.CompareOrdinal("true", ConfigurationManager.AppSettings[FileShareConfigName].ToLower()) == 0)
                    {
                        // 레퍼런스 카운팅 사용

                        if (System.String.CompareOrdinal(src, ConfigurationManager.AppSettings[ExceptionFolderConfigName].ToLower()) == 0)
                        {
                            // 예외 폴더
                            throw new Exception("Can not clone the resource.");
                        }
                    }
                    else
                    {
                        var srcPath = this.GetRequestFullPath(src);

                        if (!Directory.Exists(srcPath))
                        {
                            throw new DirectoryNotFoundException();
                        }

                        // 폴더 생성
                        var destPath = this.GetRequestFullPath(dest);
                        Directory.CreateDirectory(destPath);

                        foreach (var file in Directory.EnumerateFiles(srcPath))
                        {
                            File.Copy(file, file.Replace(srcPath, destPath));
                        }
                    }

                    return CommonUtil.ResponseData(ResponseType.Success);
                }
                catch (Exception ex)
                {
                    consolePrinter.PrintExceptionMessage(ex);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return CommonUtil.ResponseData(ResponseType.Failed, ex.ToString());
                }
            }
        }
        #endregion

        #region Method

        #region Get Path Method
        /// <summary>
        /// Rest Parameter
        /// </summary>
        /// <returns></returns>
        private string GetRequestPath()
        {
            var ctx = WebOperationContext.Current;
            if (ctx == null)
                return String.Empty;

            return ctx.IncomingRequest.UriTemplateMatch.RequestUri.AbsolutePath.Replace("/rest/tile/", "");
        }

        /// <summary>
        /// 저장 경로
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        private string GetRequestFullPath(string requestPath)
        {
            var convertedPath = requestPath.TrimStart('/').Replace('/', '\\');
            var combinedPath = Path.GetFullPath(Path.Combine(this.StorageFullPath, ".", convertedPath));
            if (!combinedPath.StartsWith(this.StorageFullPath))
                throw new HttpException(400, "HTTP/1.1 400 Bad Request");
            return combinedPath;
        }
        #endregion

        #region Resource Method

        /// <summary>
        /// 리소스 등록. 리소스 파일을 저장하고 리소스 정보를 등록한다.
        /// </summary>
        /// <param name="aMapId"></param>
        /// <param name="aPath"></param>
        /// <param name="aStream"></param>
        /// <returns></returns>
        private void AddResource(string aMapId, string aPath, Stream aStream)
        {
            //직접 그려 리소스를 추가해야 한다 (SK 하이닉스)
            //Adder..


            // 파일 저장 방식
            //var dirPath = Path.GetDirectoryName(aPath);

            //if (string.IsNullOrWhiteSpace(dirPath))
            //{
            //    throw new ArgumentException("Directory path is invalid.");
            //}

            //if (!Directory.Exists(dirPath))
            //{
            //    Directory.CreateDirectory(dirPath);
            //}

            //using (var fileStream = File.OpenWrite(aPath))
            //{
            //    var buffer = new byte[1024];
            //    int numReadBytes;
            //    while ((numReadBytes = aStream.Read(buffer, 0, buffer.Length)) > 0)
            //    {
            //        fileStream.Write(buffer, 0, numReadBytes);
            //    }
            //}
        }

        /// <summary>
        /// 리소스 저장
        /// </summary>
        /// <param name="aPath"></param>
        /// <param name="aStream"></param>
        private void SaveResource(string aPath, Stream aStream)
        {
            // SK하이닉스

            // 리소스 파일 저장
            //var fullPath = this.GetRequestFullPath(aPath);

            //var dirPath = Path.GetDirectoryName(fullPath);

            //if (string.IsNullOrWhiteSpace(dirPath))
            //{
            //    throw new ArgumentException("Directory path is invalid.");
            //}

            //if (!Directory.Exists(dirPath))
            //{
            //    Directory.CreateDirectory(dirPath);
            //}

            //using (var fileStream = File.OpenWrite(fullPath))
            //{
            //    var buffer = new byte[1024];
            //    int numReadBytes;
            //    while ((numReadBytes = aStream.Read(buffer, 0, buffer.Length)) > 0)
            //    {
            //        fileStream.Write(buffer, 0, numReadBytes);
            //    }
            //}
        }

        /// <summary>
        /// 리소스 가져오기
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        private MemoryStream LoadResource(string aPath)
        {
            //직접 그려 리소스를 가져온다 (SK 하이닉스)
            //Adder..
            var items = aPath.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length != 3) 
            {
                return null;
            }
            else
            {
                int zoom, x, y;

                if (Int32.TryParse(items[0], out zoom) &&
                    Int32.TryParse(items[1], out x) &&
                    Int32.TryParse(items[2], out y))
                {
                    return WaferTileCreator.Instance.GetTile(zoom, y, x);
                }

                return null;
            }
            //기존 방식은 파일을 가져와 Stream 으로 전송한다.
            //var fullPath = this.GetRequestFullPath(aPath);

            //if (MemoryCache.Default.Contains(fullPath))
            //    return new MemoryStream((byte[])MemoryCache.Default[fullPath]);


            //if (!File.Exists(fullPath))
            //    throw new WebFaultException(HttpStatusCode.NotFound);

            //var bytes = File.ReadAllBytes(fullPath);
            //MemoryCache.Default[fullPath] = bytes;

            //return new MemoryStream(bytes);
        }

        /// <summary>
        /// 리소스 제거
        /// </summary>
        /// <param name="aPath"></param>
        private void DeleteResource(string aPath)
        {
            //직접 리소스를 삭제한다. (SK 하이닉스)
            //Adder..


            //기존 파일 방식..
            //var fullPath = this.GetRequestFullPath(aPath);

            //if (File.Exists(fullPath))
            //{
            //    MemoryCache.Default.Remove(fullPath);
            //    File.Delete(fullPath);
            //}
            //else if (Directory.Exists(fullPath))
            //{
            //    var directoryInfo = new DirectoryInfo(fullPath);
            //    foreach (var fileInfo in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            //    {
            //        MemoryCache.Default.Remove(fileInfo.FullName);
            //    }
            //    directoryInfo.Delete(true);
            //}
            //else
            //{
            //    throw new DirectoryNotFoundException();
            //}
        }

        #endregion // Resource

        #endregion //Method
    }
}