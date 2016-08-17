using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxisJoystick;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Joystick joystick = new Joystick();

            //SetParams함수 오버로딩함.
            //기본적인 URL 정보입력 또는 모든 URL 정보입력
            joystick.SetParams("axis", "172.16.40.188", "admin", "4321", "1");

            //CC의 IP, Port를 입력
            joystick.Init("172.16.10.82", "35000");

            //thread execute
            joystick.Start();

            //thread exit
            //joystick.Stop();
        }
    }
}
