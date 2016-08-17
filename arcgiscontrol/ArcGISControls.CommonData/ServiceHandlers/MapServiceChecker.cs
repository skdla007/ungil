using System;
using System.Collections.Generic;
using System.IO;
using ArcGISControls.CommonData.Models.MapServicesDatas;
using ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Daum;
using ArcGISControls.CommonData.Models.MapServicesDatas.Xml.Naver;
using ArcGISControls.CommonData.Types;
using ArcGISControls.CommonData.Utils;
using InnowatchDebug;

namespace ArcGISControls.CommonData.ServiceHandlers
{
    public class MapServiceChecker
    {
        /// <summary>
        /// Custom Map 의 Data를 받아온다
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetCustomMapServiceData(string url, out string message)
        {
            Dictionary<string, string> readSummary = null;
            message = string.Empty;
            var requestString = string.Empty;
            try
            {
                requestString = ServiceContract.ServiceHandler.Request(url + "/summary.txt");
                readSummary = new Dictionary<string, string>();
                foreach (var s in requestString.Replace("\r\n", ",").Split(','))
                {
                    var data = s.Split(':');
                    if (data.Length != 2) continue;
                    var key = data[0];
                    var value = data[1];

                    readSummary.Add(key.Trim(), value.Trim());
                }

                if (readSummary.Count < 7)
                {
                    throw new InvalidDataException("커스텀맵의 기본정보가 잘못되었습니다. 기본 7가지 정보를 가지고 있어야 합니다. 확인해 보십시오.");
                }
            }
            catch(Exception e)
            {
                message = e.Message;
                readSummary = null;
                Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                Logger.WriteErrorLogAndTrace("request url : " + url + "\r\npage content : " + requestString);
            }

            return readSummary;
        }

        /// <summary>
        /// License 체크
        /// </summary>
        /// <param name="mapProviderType"></param>
        /// <param name="serviceUrl"></param>
        /// <param name="licenseKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool CheckLicenseKey(MapProviderType mapProviderType, string serviceUrl, string licenseKey, out string message)
        {
            var returnValue = false;
            message = string.Empty;
            try
            {
                switch (mapProviderType)
                {
                    case MapProviderType.NaverSatelliteHybridMap:
                    case MapProviderType.NaverMap:
                    case MapProviderType.NaverSatelliteMap:
                    case MapProviderType.NaverSatelliteTrafficMap:
                    case MapProviderType.NaverTrafficMap:
                        returnValue = this.CheckNaverMapLicenseKey(serviceUrl, licenseKey);
                        break;
                    case MapProviderType.DaumSatelliteHybridMap:
                    case MapProviderType.DaumMap:
                    case MapProviderType.DaumSatelliteMap:
                    case MapProviderType.DaumSatelliteTrafficMap:
                    case MapProviderType.DaumTrafficMap:
                        returnValue = this.CheckDaumMapLicenseKey(serviceUrl, licenseKey);
                        break;
                    case MapProviderType.BingMap:
                    case MapProviderType.BingArialMap:
                    case MapProviderType.BingArialWithLabelMap:
                        returnValue = this.CheckBingMapLicenseKey(serviceUrl, licenseKey);
                        break;
                    case MapProviderType.GoogleSatelliteMap:
                    case MapProviderType.GoogleMap:
                    case MapProviderType.GoogleSatelliteHybridMap:
                        returnValue = this.CheckGoogleMapLicenseKey(serviceUrl, licenseKey);
                        break;
                }
            }
            catch (Exception e)
            {
                message = mapProviderType.ToString() + " : " + e.Message;
                InnowatchDebug.Logger.WriteLogExceptionMessage(e,e.GetType().ToString());
            }
 
            return returnValue;
        }

        private bool CheckNaverMapLicenseKey(string licenseCheckUri, string licenseKey)
        {
            var xmlString = string.Empty;
            try
            {
                xmlString = XmlUtil.RequestString(string.Format(licenseCheckUri, licenseKey, "seoul"));
                XmlUtil.Deserialize<GeoCode>(xmlString);
                return true;
            }
            catch(InvalidOperationException)
            {
                try
                {
                    var naverErrorData = XmlUtil.Deserialize<Error>(xmlString);
                    throw new InvalidDataException(naverErrorData.Message);
                }
                catch (Exception e)
                {
                    InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                    throw e;
                }
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                throw e;
            }
        }

        private bool CheckDaumMapLicenseKey(string licenseCheckUri, string licenseKey)
        {
            var xmlString = string.Empty;
            try
            {
                xmlString = XmlUtil.RequestString(string.Format(licenseCheckUri, licenseKey, "seoul"));
                XmlUtil.Deserialize<Channel>(xmlString);
                return true;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    var naverErrorData = XmlUtil.Deserialize<ApiError>(xmlString);
                    throw new InvalidDataException(naverErrorData.Message + " : " + naverErrorData.DetailMessage);
                }
                catch (Exception e)
                {
                    InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                    throw e;
                }
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                throw e;
            }
        }

        private bool CheckGoogleMapLicenseKey(string licenseCheckUri, string licenseKey)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                throw e;
            }
        }

        private bool CheckBingMapLicenseKey(string licenseCheckUri, string licenseKey)
        {
            try
            {
                var stream = JsonUtil.Request(licenseCheckUri + "?supressStatus=true&key=" + licenseKey);
                var data = JsonUtil.Deserialize<BingAuthentication>(stream);
                stream.Close();
                return BingAuthentication.ValidationCode == data.AuthenticationResultCode;
            }
            catch (Exception e)
            {
                InnowatchDebug.Logger.WriteLogExceptionMessage(e, e.GetType().ToString());
                throw e;
            }
        }

        #region Singletone

        public static MapServiceChecker Instance
        {
            get { return Nested.Instance; }
        }

        private class Nested
        {
            internal static readonly MapServiceChecker Instance = new MapServiceChecker();
        }

        #endregion Singletone
    }
}
