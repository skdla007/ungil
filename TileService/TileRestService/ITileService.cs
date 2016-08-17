using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TileRestService
{
    // 참고: "리팩터링" 메뉴에서 "이름 바꾸기" 명령을 사용하여 코드 및 config 파일에서 인터페이스 이름  "IService1"을 변경할 수 있습니다.
    [ServiceContract]
    public interface ITileService
    {
        /// <summary>
        /// Get Map Resource
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "*", RequestFormat = WebMessageFormat.Xml)]
        Stream Get();

        ///// <summary>
        ///// Put Map Resources
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "PUT", UriTemplate = "*")]
        //Stream Put(Stream stream);

        ///// <summary>
        ///// Delete Map Resources
        ///// </summary>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "DELETE", UriTemplate = "*")]
        //Stream Delete();

        ///// <summary>
        ///// Clone Map Resources
        ///// </summary>
        ///// <param name="src">기존 경로</param>
        ///// <param name="dest">복사 경로</param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebGet(UriTemplate = "clone?src={src}&dest={dest}")]
        //Stream Clone(string src, string dest);

        ///// <summary>
        ///// POST
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="id"></param>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "*")]
        //Stream PostTileService(Stream stream);

        ///// <summary>
        ///// PUT
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="id"></param>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "PUT", UriTemplate = "PutTileService?id={id}&width={width}&height={height}")]
        //Stream PutTileService(Stream stream, string id, string width, string height);

        ///// <summary>
        ///// GET
        ///// </summary>
        ///// <returns></returns>
        //[OperationContract]
        //[WebGet(UriTemplate = "GetTileService?")]
        //Stream GetTileService();

        ///// <summary>
        ///// DELETE
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "DELETE", UriTemplate = "OptionTileService?id={id}")]
        //Stream DeleteTileService(Stream stream, string id);

        ///// <summary>
        ///// OPTION
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[OperationContract]
        //[WebInvoke(Method = "OPTION", UriTemplate = "OptionTileService?id={id}")]
        //Stream OptionTileService(Stream stream, string id);
    }
}
