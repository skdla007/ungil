using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
//using Microsoft.DirectX;
//using Microsoft.DirectX.DirectInput;
using SlimDX;
using SlimDX.DirectInput;
using System.Threading;

namespace AxisJoystick
{
    public class Joystick
    {
        private SlimDX.DirectInput.Joystick myJoystick;
        private SlimDX.DirectInput.JoystickState state;
        //private Device joystick; //Joystick Device
        private JoystickInfo joyInfo; //JoystickInfo 
        private Thread thread; //Joystick 처리하는 thread
        private URLParams urlParams; //URL params
        private Logger logger; //로그기록
        private int prevX; //이전 X좌표 값
        private int prevY; //이전 Y좌표 값
        private int prevZ; //이전 Z좌표 값
        private int focus;
        private bool panTiltStart; //Pan, Tilt 사용여부
        private bool zoomUse; //zoom 사용여부
        private bool zoomStart; //zoom 시작시 여부
        private bool focusStart; //focus 사용여부
        private bool workStop; //thread 종료여부
        private string way;
        private string cameracontrollerserverIP;
        private string cameracontrollerserverPort;
        public string paramURL { get; set; }

        public Joystick()
        {
            try
            {
                state = new SlimDX.DirectInput.JoystickState();
                urlParams = new URLParams();
                logger = new Logger();
                workStop = false;
                focus = 0;
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        
        //public Joystick GetJoystick()
        //{
        //    return new Joystick();
        //}

        /// <summary>
        /// Joystick 장비 여부를 확인하고 획득하는 함수
        /// </summary>
        /// <param name="ip">
        /// Camera Controller Server IP
        /// </param>
        /// <param name="cameracontrollerserverPort">
        /// Camera Controller Server Port
        /// </param>
        public void Init(string cameracontrollerserverIP, string cameracontrollerserverPort)
        {
            try
            {
                //test
                Console.WriteLine("Init()");

                joyInfo = new JoystickInfo(cameracontrollerserverIP, cameracontrollerserverPort);

                // make sure that DirectInput has been initialized
                DirectInput dinput = new DirectInput();

                // search for devices
                foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
                {
                    // create the device
                    try
                    {
                        myJoystick = new SlimDX.DirectInput.Joystick(dinput, device.InstanceGuid);
                        //myJoystick.SetCooperativeLevel(this, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
                        break;
                    }
                    catch (DirectInputException)
                    {
                    }
                }

                if (myJoystick == null)
                {
                    return;
                }

                foreach (DeviceObjectInstance deviceObject in myJoystick.GetObjects())
                {
                    if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                        myJoystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

                    //UpdateControl(deviceObject);
                }

                myJoystick.Properties.SetRange(-100, 100);

                // acquire the device
                myJoystick.Acquire();

                //test
                Console.WriteLine("GetDevice()");
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// thread실행
        /// </summary>
        public void Start()
        {
            try
            {
                if (this.thread == null)
                {
                    this.thread = new Thread(Poll);
                }

                if (this.thread != null)
                {
                    this.thread.Start();
                }
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// fnPoll함수에 while문 빠져나오게 하고 스레드 종료를 위한 함수
        /// </summary>
        public void Stop()
        {
            //test
            Console.WriteLine("Stop()");
            this.workStop = true;
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// 제어할 카메라 정보 설정 
        /// </summary>
        /// <param name="cameraProduct">
        /// 카메라DLL명
        /// </param>
        /// <param name="dllName">
        /// DllName
        /// </param>
        /// <param name="cameraIP">
        /// Camera IP
        /// </param>
        /// /// <param name="cameraID">
        /// Camera ID
        /// </param>
        /// /// <param name="cameraPW">
        /// Camera PW
        /// </param>
        /// <param name="channel">
        /// 해당 카메라가 할당된 비디오 서버상의 채널 번호
        /// </param>
        /// <param name="baudrate">
        /// SerialPort 사용시에만 기재 (속도)
        /// </param>
        /// <param name="databits">
        /// SerialPort 사용시에만 기재 (데이터 전송 단위)
        /// </param>
        /// <param name="parity">
        /// SerialPort 사용시에만 기재 (Checksum)
        /// </param>
        /// <param name="stopbits">
        /// SerialPort 사용시에만 기재
        /// </param>
        /// <param name="intervaltime">
        /// Sj3000Rx 프로토콜 전용 옵션 (PTZ 제어 명령 전달 간격)
        /// </param>
        /// <param name="state">
        /// Sj3000Rx 프로토콜 전용 옵션 (PTZ 현재 상태)
        /// </param>
        /// <param name="actiontime">
        /// Sj3000Rx 프로토콜 전용 옵션 (PTZ 동작시간)
        /// </param>
        /// <param name="targetpoint">
        /// Sj3000Rx와 Axis 프로토콜 전용 옵션 (PTZ Preset Target-Point)
        /// </param>
        /// <param name="groupselectdata">
        /// Sj100 프로토콜 전용 옵션 (PTZ 제어 ID 그룹)
        /// </param>
        /// <param name="ipport">
        /// Sj100 프로토콜 전용 옵션 (PTZ 제어 ID 그룹)
        /// </param>
        /// <param name="audioport">
        /// Audio Port
        /// </param>
        /// <param name="serialport">
        /// Serial Port
        /// </param>
        /// <param name="receiverid">
        /// Receiver ID
        /// </param>
        public void SetParams(string dllName, string cameraIP, string cameraID, string cameraPW, string channel, string baudRate, string dataBits,
            string parity, string stopBits, string intervalTime, string state, string actionTime, string targetPoint, string groupSelectdate, string ipPort,
            string audioport, string serialport, string receiverid, string sid)
        {
            try
            {
                urlParams.dllname = dllName;
                urlParams.address = cameraIP;
                urlParams.username = cameraID;
                urlParams.password = cameraPW;
                urlParams.channel = channel;
                urlParams.BaudRate = baudRate;
                urlParams.DataBits = dataBits;
                urlParams.Parity = parity;
                urlParams.StopBits = stopBits;
                urlParams.ActionTime = actionTime;
                urlParams.IntervalTime = intervalTime;
                urlParams.State = state;
                urlParams.TargetPoint = targetPoint;
                urlParams.GroupSelectData = groupSelectdate;
                urlParams.IpPort = ipPort;
                urlParams.SerialPort = serialport;
                urlParams.sid = sid;
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// 제어할 카메라 정보 설정 
        /// </summary>
        /// <param name="dllName">
        /// DllName
        /// </param>
        /// <param name="cameraIP">
        /// Camera IP
        /// </param>
        /// /// <param name="cameraID">
        /// Camera ID
        /// </param>
        /// /// <param name="cameraPW">
        /// Camera PW
        /// </param>
        /// <param name="channel">
        /// 해당 카메라가 할당된 비디오 서버상의 채널 번호
        /// </param>
        public void SetParams(string dllName, string cameraIP, string cameraID, string cameraPW, string channel, string sid)
        {
            try
            {
                urlParams.dllname = dllName;
                urlParams.address = cameraIP;
                urlParams.username = cameraID;
                urlParams.password = cameraPW;
                urlParams.channel = channel;
                urlParams.sid = sid;
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Pan, Tilt, Zoom 처리
        /// </summary>
        private void Poll()
        {
            try
            {
                //먼저 CC로 URL던져 연결 활성화
                joyInfo.MakePanTiltURL("pantiltstop", 0, urlParams, false);//pantiltstop

                while (this.workStop == false)
                {
                    //조이스틱 state 획득
                    state = myJoystick.GetCurrentState();

                    //Pan, Tilt기능 처리
                    PanTilt(state);
                    
                    if (this.prevZ != state.Z)
                    {
                        //Zoom기능 처리
                        Zoom(state);
                    }

                    //버튼 획득
                    bool[] buttons = state.GetButtons();

                    //버튼의 개수가 6개이므로
                    for (int iCnt = 0; iCnt < 6; iCnt++)
                    {
                        if (buttons[iCnt] != false && iCnt < 4)
                        {
                            //Preset기능 처리
                            Preset(state);
                        }

                        if (buttons[iCnt] != false && iCnt >= 4 && iCnt < 6)
                        {
                            //Focus기능 처리
                            Focus(state, 100, 10000);
                        }
                    }

                    //CC로 명령 메시지 전송 시 약간의 delay주기 위해 사용
                    //sleep주지 않으면 CC에서 명령 메시지 전부 처리하지 못함.
                    //Thread.Sleep(40);
                }

                if (thread != null)
                {
                    while (!thread.IsAlive);
                    Thread.Sleep(1);
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Pan, Tilt의 move 메시지 처리 
        /// </summary>
        /// <param name="move">
        /// move종류(left, right, up, down, leftup, leftdown, rightup, rightdown)
        /// </param>
        /// /// <param name="value">
        /// parameters value
        /// </param>
        private void PanTiltMove(string move, int speed, int maxSpeed)
        {
            try
            {
                this.way = move;

                //현재 max값 없이 SetRange로 설정한 값 그대로 사용하도록 하였음. 추수 max값 설정 필요시 아래 주석 풀고 사용하면됨.
                //max이상의 Speed값을 주면 화면이동 속도가 빨라 사용하기 불편하여 Speed의 Max값을 설정
                //if (speed > maxSpeed)
                //    speed = maxSpeed;

                //move와 move_start를 같이 보내는 이유
                //move 또는 move_start중 하나만 보내는 경우 Speed값이 변경되어도 이전 Speed값으로 이동되는 문제가 있어
                //move와 move_start를 같이 보내면 위의 문제가 해결되어서 move와 move_start를 같이 보내도록 하였음.

                //move_start
                joyInfo.MakePanTiltURL(move + "_start", speed, urlParams, true);
                Thread.Sleep(50);

                //move
                joyInfo.MakePanTiltURL(move, speed, urlParams, true);
                Thread.Sleep(50);

                //다른 방향으로 전환 시 pantiltstop해줘야 보다 자연스럽게 화면이동됨.
                if (this.way != move)
                {
                    joyInfo.MakePanTiltURL("pantiltstop", 0, urlParams, false);//pantiltstop
                    Thread.Sleep(50);
                }
                this.panTiltStart = true;
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// 조이스틱으로 Pan, Tilt 방향 설정 
        /// </summary>
        /// <param name="curX">
        /// current X position
        /// </param>
        /// <param name="curY">
        /// current Y position
        /// </param>
        /// <param name="maxSpeed">
        /// Speed 최대값
        /// </param>
        private void Position(int curX, int curY, int maxSpeed)
        {
            try
            {
                //이전 X, Y값과 현재 X, Y값을 비교하여 다르면 fnPanTiltMove사용 할 수 있도록함.
                if (this.prevX != curX ^ this.prevY != curY)
                {
                    //InputRange 범위를 -100, 100으로 설정하여 로직의 범위에 맞게 설정함.
                    //small left
                    if ((curX < -4 && curX > -16) && (curY < 8 && curY > -8))
                    {
                        PanTiltMove("left", curX, maxSpeed);
                    }

                    //small leftup
                    if ((curX < -4 && curX > -16) && (curY < -6 && curY > -16))
                    {
                        PanTiltMove("leftup", curX, maxSpeed);
                    }

                    //small leftdown
                    if ((curX < -4 && curX > -16) && (curY > 6 && curY < 16))
                    {
                        PanTiltMove("leftdown", curX, maxSpeed);
                    }

                    //small up
                    if ((curX < 5 && curX > -5) && (curY < -5 && curY > -16))
                    {
                        PanTiltMove("up", curY, maxSpeed);
                    }

                    //small down
                    if ((curX < 5 && curX > -5) && (curY > 5 && curY < 16))
                    {
                        PanTiltMove("down", curY, maxSpeed);
                    }

                    //small right
                    if ((curX > 4 && curX < 16) && (curY < 8 && curY > -8))
                    {
                        PanTiltMove("right", curX, maxSpeed);
                    }

                    //small rightup
                    if ((curX > 4 && curX < 16) && (curY < -6 && curY > -16))
                    {
                        PanTiltMove("rightup", curX, maxSpeed);
                    }

                    //small rightdown
                    if ((curX > 4 && curX < 16) && (curY > 6 && curY < 16))
                    {
                        PanTiltMove("rightdown", curX, maxSpeed);
                    }

                    //---------------------------------------------------------------

                    //left
                    if ((curX < -16 && curX > -100) && (curY < 16 && curY > -16))
                    {
                        PanTiltMove("left", curX, maxSpeed);
                    }

                    //leftup
                    if ((curX < -16 && curX > -100) && (curY < -16 && curY > -100))
                    {
                        PanTiltMove("leftup", curX, maxSpeed);
                    }

                    //leftdown
                    if ((curX < -16 && curX > -100) && (curY > 16 && curY < 100))
                    {
                        PanTiltMove("leftdown", curX, maxSpeed);
                    }

                    //up
                    if ((curX > -16 && curX < 16) && (curY < -16 && curY > -100))
                    {
                        PanTiltMove("up", curY, maxSpeed);
                    }

                    //down
                    if ((curX > -16 && curX < 16) && (curY > 16 && curY < 100))
                    {
                        PanTiltMove("down", curY, maxSpeed);
                    }

                    //right
                    if ((curX > 16 && curX < 100) && (curY < 16 && curY > -16))
                    {
                        PanTiltMove("right", curX, maxSpeed);
                    }

                    //rightup
                    if ((curX > 16 && curX < 100) && (curY < -16 && curY > -100))
                    {
                        PanTiltMove("rightup", curX, maxSpeed);
                    }

                    //rightdown
                    if ((curX > 16 && curX < 100) && (curY > 16 && curY < 100))
                    {
                        PanTiltMove("rightdown", curX, maxSpeed);
                    }

                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Pan, Tilt 기능
        /// </summary>
        /// <param name="state">
        /// JoystaickState object
        /// </param>
        private void PanTilt(JoystickState state)
        {
            try
            {
                //조이스틱 좌표에 따른 메시지 처리
                Position(state.X, state.Y, 40);

                //X, Y값이 5미만이고 panTiltStart값이 true일 때 pantiltstop을 전송한다.
                if (Math.Abs(state.X) < 5 && Math.Abs(state.Y) < 5 && panTiltStart == true)
                {
                    joyInfo.MakePanTiltURL("pantiltstop", 0, urlParams, false);//pantiltstop
                    this.panTiltStart = false;
                }

                //Input Position
                this.prevX = state.X;
                this.prevY = state.Y;

                //조이스틱을 가만히 두었을때 이전 X, Y값을 0으로 설정
                if (Math.Abs(state.X) < 5 && Math.Abs(state.Y) < 5)
                {
                    this.prevX = 0;
                    this.prevY = 0;
                }
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Zoom기능
        /// </summary>
        /// <param name="state">
        /// JoystaickState object
        /// </param>
        private void Zoom(JoystickState state)
        {
            try
            {
                ////줌 사용중 조이스틱 풀때 멈추도록 함
                //if (joyInfo.CompareZoom(prevZ, state.Z) > 4 && zoomUse == true && state.Z != 0)
                //{
                //    joyInfo.MakeZoomURL("zoomstop", state.Z, urlParams);
                //    this.zoomUse = false;
                //}
                //else //줌 사용중
                //{
                //    if (state.Z != 0)
                //    {
                //        if (state.Z > 4) //zoomin
                //        {
                //            joyInfo.MakeZoomURL("zoomin_start", state.Z, urlParams);
                //            joyInfo.MakeZoomURL("zoomin", state.Z, urlParams);
                //            Thread.Sleep(100);

                //            this.zoomStart = true;
                //        }
                //        else if (state.Z < -4) //zoomout
                //        {
                //            joyInfo.MakeZoomURL("zoomout_start", state.Z, urlParams);
                //            joyInfo.MakeZoomURL("zoomout", state.Z, urlParams);
                //            Thread.Sleep(100);

                //            this.zoomStart = true;
                //        }

                //        this.zoomUse = true;
                //    }
                //}

                if (state.Z != 0)
                {
                    if (state.Z > 4) //zoomin
                    {
                        joyInfo.MakeZoomURL("zoomin_start", state.Z, urlParams);
                        joyInfo.MakeZoomURL("zoomin", state.Z, urlParams);
                        Thread.Sleep(50);

                        this.zoomStart = true;
                    }
                    else if (state.Z < -4) //zoomout
                    {
                        joyInfo.MakeZoomURL("zoomout_start", state.Z, urlParams);
                        joyInfo.MakeZoomURL("zoomout", state.Z, urlParams);
                        Thread.Sleep(50);

                        this.zoomStart = true;
                    }

                    this.zoomUse = true;
                }

                //조이스틱 사용 후 가끔 0이아닌 -1등의 값으로 설정되는 경우가 있어 줌의 범위가 -5에서 5 사이에 있을 때 zoomstop을 해주도록 함.
                if (state.Z > -5 && state.Z < 5 && zoomStart == true)
                {
                    joyInfo.MakeZoomURL("zoomstop", state.Z, urlParams);
                    zoomUse = false;
                    zoomStart = false;
                }

                this.prevZ = state.Z;

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Preset 기능
        /// </summary>
        /// <param name="state">
        /// JoystaickState object
        /// </param>
        private void Preset(JoystickState state)
        {
            try
            {
                //버튼 획득
                bool[] buttons = state.GetButtons();                

                if (buttons[0] != false) //J1버튼으로 Preset1으로 설정
                {
                    joyInfo.MakePresetURL("presetmove", 1, urlParams);
                }

                if (buttons[1] != false) //J2버튼으로 Preset2으로 설정
                {
                    joyInfo.MakePresetURL("presetmove", 2, urlParams);
                }

                if (buttons[2] != false) //J3버튼으로 Preset3으로 설정
                {
                    joyInfo.MakePresetURL("presetmove", 3, urlParams);
                }

                if (buttons[3] != false) //J4버튼으로 Preset4으로 설정
                {
                    joyInfo.MakePresetURL("presetmove", 4, urlParams);
                }

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------

        /// <summary>
        /// Focus 기능
        /// </summary>
        /// <param name="state">
        /// JoystaickState object
        /// </param>
        /// <param name="focusunit">
        /// 일정 단위로 포커스 증가시킬 값
        /// </param>
        /// <param name="maxFocus">
        /// 포커스 최대값
        /// </param>
        private void Focus(JoystickState state, int focusUnit, int maxFocus)
        {
            try
            {
                //버튼 획득
                bool[] buttons = state.GetButtons();

                if (buttons[4] != false) //L버튼으로 focus near로 설정
                {
                    if (this.focus > 0)
                        this.focus -= 100; //Focus 범위가 1..9999여서 감도 조절함

                    joyInfo.MakeFocusURL("FocusNear_Start", this.focus, urlParams);
                    joyInfo.MakeFocusURL("FocusNear", this.focus, urlParams);

                    this.focusStart = true;
                }

                if (buttons[5] != false) //R버튼으로 focus far로 설정
                {
                    if (this.focus < maxFocus)
                        this.focus += focusUnit;//Focus 범위가 1..9999여서 감도 조절함

                    joyInfo.MakeFocusURL("FocusFar_Start", this.focus, urlParams);
                    joyInfo.MakeFocusURL("FocusFar", this.focus, urlParams);

                    this.focusStart = true;
                }

                //포커스를 사용하고 난 후 focusstop을 전송
                if (buttons[4] == false && buttons[5] == false && focusStart == true)
                {
                    joyInfo.MakeFocusURL("FocusStop", this.focus, urlParams);

                    this.focusStart = false;
                }

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }
        //------------------------------------------------------------------------------------               
    }
}
