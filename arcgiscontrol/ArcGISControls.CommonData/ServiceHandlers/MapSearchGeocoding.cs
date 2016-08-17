using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControls.CommonData.ServiceHandlers
{
    public class MapSearchGeocoding
    {
        /// <summary>
        /// 텍스트로 서치 했을 경우
        /// </summary>
        /// <param name="searchTxt"></param>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<MapAddressObjectDataInfo> GetMapSearchDataList(string searchTxt, MapProviderType provider, string key)
        {
            var enCodingsearchTxt =  EncodingUrl(searchTxt);

            var url = string.Format(ArcGISConstSet.SearchUrlGoogle, "address", enCodingsearchTxt, true, CultureInfo.CurrentCulture);

            if (provider.ToString().Substring(0, 4).ToUpper() == "DAUM")
            {
                url = string.Format(ArcGISConstSet.SearchTextUrlDaum, key, enCodingsearchTxt);
            }
            else if (provider.ToString().Substring(0, 4).ToUpper() == "NAVE")
            {
                url = string.Format(ArcGISConstSet.SearchTextUrlNaver, key, enCodingsearchTxt); 
            }
            
            url = url.ToLower();

            var xml = new XmlDocument();

            try
            {
                xml.Load(url);
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(string.Format("{0} : {1}", url,  e.ToString()));
            }

            //xml parsing 분할 

            try
            {
                switch (provider.ToString().Substring(0, 4).ToUpper())
                {
                    case "DAUM":
                        return ParsingDaumSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)), searchTxt);
                    case "NAVE":
                        return ParsingNaverSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)), searchTxt);
                    default:
                        return ParsingGoogleSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)), searchTxt);
                }
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(e.ToString());
            }

            return null;
        }

        /// <summary>
        /// 위치로 서치 했을 경우
        /// </summary>
        /// <param name="pLatLng"></param>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static MapAddressObjectDataInfo GetMapSearchDataList(Point pLatLng, MapProviderType provider, string key)
        {
            string url = string.Format(ArcGISConstSet.SearchUrlGoogle, "latlng", pLatLng.ToString(), true, CultureInfo.CurrentCulture.ToString());

            if (provider.ToString().Substring(0, 4).ToUpper() == "DAUM" || provider.ToString().Substring(0, 4).ToUpper() == "NAVE")
            {
                url = string.Format(ArcGISConstSet.SearchLatLngUrlDuam, key, pLatLng.Y.ToString(), pLatLng.X.ToString());
            }

            url = url.ToLower();

            var xml = new XmlDocument();

            try
            {
                xml.Load(url);
            }
            catch(Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(string.Format("{0} : {1}", url, e.ToString()));
            }

            //xml parsing 분할 
            List<MapAddressObjectDataInfo> mapAddressObjectDataInfos;

            try
            {
                switch (provider.ToString().Substring(0, 4).ToUpper())
                {
                    case "DAUM":
                        mapAddressObjectDataInfos = ParsingDaumSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)));
                        break;
                    case "NAVE":
                        mapAddressObjectDataInfos = ParsingNaverSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)));
                        break;
                    default:
                        mapAddressObjectDataInfos = ParsingGoogleSearchResult(new XmlTextReader(new System.IO.StringReader(xml.InnerXml)));
                        break;
                }

                if (mapAddressObjectDataInfos != null && mapAddressObjectDataInfos.Count > 0) return mapAddressObjectDataInfos[0];
            }
            catch(Exception e)
            {
                InnowatchDebug.Logger.WriteInfoLog(string.Format("{0} : {1}", provider.ToString(), e.ToString()));
            }

            return null;
        }

        /// <summary>
        /// NAVER SEARCH
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="searchTxt"> 위치로 검색 했을 경우 ""</param>
        /// <returns></returns>
        private static List<MapAddressObjectDataInfo> ParsingNaverSearchResult(XmlTextReader reader, string searchTxt = "")
        {
            var mapAddressObjectDataInfos = new List<MapAddressObjectDataInfo>();
            MapAddressObjectDataInfo mapAddressObjectDataInfo = null;

            string stag = string.Empty;
            string value = string.Empty;
            double? x = null , y = null; 

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        stag = reader.Name;

                        if (reader.Name == "item")
                        {
                            mapAddressObjectDataInfo = new MapAddressObjectDataInfo
                                                            {
                                                                SearchText = searchTxt,
                                                                Name = searchTxt,
                                                                ObjectType = MapObjectType.SearchedAddress
                                                            };
                        }

                        break;

                    case XmlNodeType.Text:
                        value = reader.Value;
                        break;

                    case XmlNodeType.EndElement:
                        if (mapAddressObjectDataInfo != null)
                        {
                            switch (stag)
                            {
                                case "address":
                                    mapAddressObjectDataInfo.Address = value;
                                    break;
                                case "x":
                                    x = double.Parse(value);
                                    break;
                                case "y":
                                    y = double.Parse(value);
                                    break;
                            }

                            if (reader.Name == "item")
                            {
                                if (x != null && y != null)
                                {
                                    mapAddressObjectDataInfo.Position = new Point(x.Value, y.Value);
                                }

                                mapAddressObjectDataInfo.SearchedIndex = mapAddressObjectDataInfos.Count;
                                mapAddressObjectDataInfos.Add(mapAddressObjectDataInfo);

                                stag = string.Empty;
                                value = string.Empty;
                                x = null;
                                y = null;

                                mapAddressObjectDataInfo = null;
                            }
                        }

                        break;
                }
            }

            return mapAddressObjectDataInfos;
        }

        /// <summary>
        /// DAUM SEARCH
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="searchTxt"> 위치로 검색 했을 경우 ""</param>
        /// <returns></returns>
        private static List<MapAddressObjectDataInfo> ParsingDaumSearchResult(XmlTextReader reader, string searchTxt = "")
        {
            var mapAddressObjectDataInfos = new List<MapAddressObjectDataInfo>();
            MapAddressObjectDataInfo mapAddressObjectDataInfo = null;

            string stag = string.Empty;
            string value = string.Empty;
            double? x = null, y = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        stag = reader.Name;

                        if (reader.Name == "item")
                        {
                            mapAddressObjectDataInfo = new MapAddressObjectDataInfo()
                                                            {
                                                                SearchText = searchTxt,
                                                                Name = searchTxt,
                                                                ObjectType = MapObjectType.SearchedAddress
                                                            };
                        }
                        break;

                    case XmlNodeType.Text:
                        value = reader.Value;
                        value = value.Replace("<b>", "");
                        value = value.Replace("</b>", "");
                        value = value.Replace("<B>", "");
                        value = value.Replace("</B>", "");
                        break;

                    case XmlNodeType.EndElement:
                        if (mapAddressObjectDataInfo != null)
                        {
                            switch (stag)
                            {
                                case "title":
                                    mapAddressObjectDataInfo.Address = value;
                                    break;
                                case "lng":
                                    x = double.Parse(value);

                                    break;
                                case "lat":
                                    y = double.Parse(value);
                                    break;
                            }

                            if (reader.Name == "item")
                            {
                                if (x != null && y != null)
                                {
                                    mapAddressObjectDataInfo.Position = new Point(x.Value, y.Value);
                                }

                                mapAddressObjectDataInfo.SearchedIndex = mapAddressObjectDataInfos.Count;
                                mapAddressObjectDataInfos.Add(mapAddressObjectDataInfo);

                                stag = string.Empty;
                                value = string.Empty;
                                x = null;
                                y = null;

                                mapAddressObjectDataInfo = null;
                            }
                        }
                        break;
                }
            }

            return mapAddressObjectDataInfos;
        }

        /// <summary>
        /// GOOGLE SEARCH
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="searchTxt"> 위치로 검색 했을 경우 ""</param>
        /// <returns></returns>
        private static List<MapAddressObjectDataInfo> ParsingGoogleSearchResult(XmlTextReader reader, string searchTxt = "")
        {
            var mapAddressObjectDataInfos = new List<MapAddressObjectDataInfo>();
            string stag = string.Empty;
            string value = "";
            //viewPort로 영역 설정 하고 싶을때는 false로 하고 end tag에서도 false 부분의 주석을 뺀다.
            bool isViewport = true;
            MapAddressObjectDataInfo mapAddressObjectDataInfo = null;

            double? lat = null, lng = null, swlat = null, swlng = null, nelat = null, nelng = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (stag == "location" || stag == "southwest" || stag == "northeast")
                        {
                            stag += "_" + reader.Name;
                        }
                        else if (reader.Name == "result")
                        {
                            mapAddressObjectDataInfo = new MapAddressObjectDataInfo()
                                                          {
                                                              SearchText = searchTxt, 
                                                              Name = searchTxt, 
                                                              ObjectType = MapObjectType.SearchedAddress
                                                          };
                        }
                        else if (reader.Name == "viewport")
                        {
                            isViewport = true;
                        }
                        else if (stag != "address_component")
                        {
                            stag = reader.Name;
                        }
                        break;

                    case XmlNodeType.Text:
                        value = reader.Value;
                        break;

                    case XmlNodeType.EndElement:
                        //MapSearch Data값을 넣는다.
                        switch (stag)
                        {
                            case "type":
                                mapAddressObjectDataInfo.Types += value + ",";
                                break;
                            case "formatted_address":
                                mapAddressObjectDataInfo.Address = value;
                                break;
                            case "location_lng":
                                lng = double.Parse(value);
                                break;
                            case "location_lat":
                                lat = double.Parse(value);
                                stag = "location";
                                break;
                            case "southwest_lng":
                                if (isViewport)
                                    swlng = double.Parse(value);
                                break;
                            case "southwest_lat":
                                if (isViewport)
                                    swlat = double.Parse(value);
                                stag = "southwest";
                                break;
                            case "northeast_lng":
                                if (isViewport)
                                    nelng = double.Parse(value);
                                break;
                            case "northeast_lat":
                                if (isViewport)
                                    nelat = double.Parse(value);
                                stag = "northeast";
                                break;

                        }

                        if (reader.Name == "address_component")
                        {
                            stag = "";
                        }
                        else if (reader.Name == "viewport")
                        {
                            InnowatchDebug.Logger.Trace("N Branching Statement Processing - else if (reader.Name == \"viewport\")");
                            //isViewport = false;
                        }
                        else if (reader.Name == "result")
                        {   
                            mapAddressObjectDataInfo.Position = new Point(lng.Value, lat.Value);
                            mapAddressObjectDataInfo.ExtentRegion = new Rect(new Point(swlng.Value, swlat.Value),new Point(nelng.Value, nelat.Value));

                            mapAddressObjectDataInfo.SearchedIndex = mapAddressObjectDataInfos.Count;
                            mapAddressObjectDataInfos.Add(mapAddressObjectDataInfo);

                            stag = "";
                            value = "";
                            mapAddressObjectDataInfo = null;
                        }

                        break;
                }

                if (stag.ToLower() == "status" && (value.ToLower() == "zero_results" || value.ToLower() == "over_query_limit" || value.ToLower() == "request_denied" || value.ToLower() == "invalid_request"))
                {
                    return null;
                }
            }

            return mapAddressObjectDataInfos;
        }

        private static string EncodingUrl(string parameter)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buffer = encoding.GetBytes(parameter);

            string hex = BitConverter.ToString(buffer).Replace("-", "%");
            hex = "%" + hex;

            return hex;
        }
    }
}
