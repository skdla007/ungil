using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

namespace AxisJoystick
{
    class JoystickInfo      // 조이스틱 제어를 위한 정보를 담고 있는 클래스
    {
        private Logger logger;
        public string CameraControllerServerIP {get; set;} //Camera Controller Server IP
        public string CameraControllerServerPort {get; set;} //Camera Controller Server Port

        //Camera Controller Server로 Rest URL 전송을 위한 URL
        const string commonURL = "http://{0}:{1}/rest/cameracontroller/ptz/control?command={2}&parameters={3}";
        const string commonNotParamURL = "http://{0}:{1}/rest/cameracontroller/ptz/control?command={2}";

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="ip">
        /// Camera Controller Server IP
        /// </param>
        /// <param name="port">
        /// Camera Controller Server Port
        /// </param>
        public JoystickInfo(string cameracontrollerserverIP, string cameracontrollerserverPort)
        {
            this.CameraControllerServerIP = cameracontrollerserverIP;
            this.CameraControllerServerPort = cameracontrollerserverPort;
            logger = new Logger();
        }

        /// <summary>
        /// URL Param들을 추가해주는 함수
        /// </summary>
        /// <param name="param">
        /// URLParams object
        /// </param>
        private string AddOptions(URLParams param)
        {
            string options = "";

            options = param.ToParameter();

            return options;
        }

        /// <summary>
        /// Pan,Tilt URL 생성
        /// </summary>
        /// <param name="command">
        /// joystick move command
        /// </param>
        /// <param name="speed">
        /// Speed value
        /// </param>
        /// <param name="param">
        /// URLParams object
        /// </param>
        /// <param name="existParameters">
        /// Parameters에 값이 있는지 여부
        /// </param>
        public string MakePanTiltURL(string command, int speed, URLParams param, bool existParameters)
        {
            string URL = "";

            if (existParameters == true) //pantilt
            {
                URL = string.Format(commonURL, CameraControllerServerIP, CameraControllerServerPort, command, Math.Abs(speed).ToString());
                URL += "&";
                URL += AddOptions(param);

                SendURL(this.CameraControllerServerIP, this.CameraControllerServerPort, URL, param.username, param.password, "GET");
            }
            else
            {
                URL = string.Format(commonNotParamURL, CameraControllerServerIP, CameraControllerServerPort, command);
                URL += "&";
                URL += AddOptions(param);

                SendURL(this.CameraControllerServerIP, this.CameraControllerServerPort, URL, param.username, param.password, "GET");
            }

            return URL;
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Zoom URL 생성
        /// </summary>
        /// <param name="command">
        /// joystick move command
        /// </param>
        /// <param name="speed">
        /// Speed value
        /// </param>
        /// <param name="param">
        /// URLParams object
        /// </param>
        public string MakeZoomURL(string command, int speed, URLParams param)
        {
            string URL = "";

            URL = string.Format(commonURL, CameraControllerServerIP, CameraControllerServerPort, command, Math.Abs(speed).ToString());
            URL += "&";
            URL += AddOptions(param);

            SendURL(this.CameraControllerServerIP, this.CameraControllerServerPort, URL, param.username, param.password, "GET");

            return URL;
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Focus URL 생성
        /// </summary>       
        /// <param name="command">
        /// joystick move command
        /// </param>
        /// <param name="focusValue">
        /// Focus value
        /// </param>
        /// <param name="param">
        /// URLParams object
        /// </param>
        public string MakeFocusURL(string command, int focusValue, URLParams param)
        {
            string URL = "";

            URL = string.Format(commonURL, CameraControllerServerIP, CameraControllerServerPort, command, Math.Abs(focusValue).ToString());
            URL += "&";
            URL += AddOptions(param);

            SendURL(this.CameraControllerServerIP, this.CameraControllerServerPort, URL, param.username, param.password, "GET");

            return URL;
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Preset URL 생성
        /// </summary>
        /// <param name="command">
        /// joystick move command
        /// </param>
        /// <param name="button">
        /// preset button value
        /// </param>
        /// <param name="param">
        /// URLParams object
        /// </param>
        public string MakePresetURL(string command, int button, URLParams param)
        {
            string URL = "";

            URL = string.Format(commonURL, CameraControllerServerIP, CameraControllerServerPort, command, button.ToString());
            URL += "&";
            URL += AddOptions(param);

            SendURL(this.CameraControllerServerIP, this.CameraControllerServerPort, URL, param.username, param.password, "GET");

            return URL;
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// PrevZ와 현재 Z의 절대값 차이 계산
        /// </summary>
        /// <param name="prevZ">
        /// Previous Z Data
        /// </param>
        /// <param name="curZ">
        /// Current Z Data
        /// </param>
        public int CompareZoom(int prevZ, int curZ)
        {
            int iResult;
            iResult = Math.Abs(prevZ) - Math.Abs(curZ);

            return Math.Abs(iResult);
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// 카메라로 URL전송
        /// </summary>
        /// <param name="cameracontrollerserverIP">
        /// Camera Controller Server IP
        /// </param>
        /// <param name="cameracontrollerserverPort">
        /// Camera Controller Server Port
        /// </param>
        /// <param name="url">
        /// URL string
        /// </param>
        /// <param name="cameraID">
        /// 카메라 계정 ID
        /// </param>
        /// <param name="cameraPW">
        /// 카메라 계정 PW
        /// </param>
        /// <param name="method">
        /// GET or POST method
        /// </param>
        public bool SendURL(string cameracontrollerserverIP, string cameracontrollerserverPort, string url, string cameraID, string cameraPW, string method)
        {
            string sResult;
            HttpWebRequest wReq;
            HttpWebResponse wRes;

            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    Console.WriteLine(url);
                    wReq = (HttpWebRequest)WebRequest.Create(url); //WebRequest생성
                    wReq.Method = method; //GET 설정
                    wReq.Credentials = new System.Net.NetworkCredential(cameraID, cameraPW); //Credential설정

                    using (wRes = (HttpWebResponse)wReq.GetResponse())
                    {
                        Stream respStream = wRes.GetResponseStream();
                        StreamReader reader = new StreamReader(respStream, Encoding.GetEncoding("utf-8"), true);

                        sResult = reader.ReadToEnd();
                        //Console.WriteLine(sResult);

                        reader.Close();
                        respStream.Close();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Write(ex);
                return false;
            }
        }
        //------------------------------------------------------------------------------------
    }
}
