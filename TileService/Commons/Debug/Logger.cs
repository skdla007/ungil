using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Debug
{
    using log4net;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// The logger.
    /// </summary>
    public class Logger
    {
        static Logger()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The write line.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void WriteLine(string message)
        {
            Debug.WriteLine(
                    "[" + MethodBase.GetCurrentMethod().ReflectedType.FullName + "] " +
                    "[" + MethodBase.GetCurrentMethod().Name + "] " +
                    message);
        }

        /// <summary>
        /// TRACE method.
        /// </summary>
        /// <param name="originClass">
        /// The origin class of the message.
        /// </param>
        /// <param name="priority">
        /// The priority of the message.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(object originClass, int priority, string message, params object[] obj)
        {
#if DEBUG
            DoTrace(obj, message, originClass, priority);
#endif
        }

        /// <summary>
        /// The TRACE method.
        /// </summary>
        /// <param name="priority">
        /// Priority of the message.
        /// </param>
        /// <param name="message">
        /// The Message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(int priority, string message, params object[] obj)
        {
            Trace((object)null, priority, message, obj);
        }

        /// <summary>
        /// TRACE method.
        /// </summary>
        /// <param name="message">
        /// The Message.
        /// </param>
        /// <param name="obj">
        /// String object will be replaced with count in the message.
        /// </param>
        public static void Trace(string message, params object[] obj)
        {
            Trace(-1, message, obj);
        }

        /// <summary>
        /// The do trace.
        /// 디버깅용 트레이스 함수.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="originClass">
        /// The origin class.
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        private static void DoTrace(object[] obj, string message, object originClass, int priority)
        {
            var count = 0;
            foreach (var strObject in obj)
            {
                string strSubFormat = "{" + count + "}";

                message = strObject == null ? message.Replace(strSubFormat, "null") : message.Replace(strSubFormat, strObject.ToString());

                count++;
            }

            string className = string.Empty;

            if (originClass != null)
            {
                className = originClass.GetType().Name;
            }

            var overallMessage = new StringBuilder();

            if (originClass != null)
            {
                overallMessage.Append("[" + className + "] ");
            }

            overallMessage.Append(message);
            if (priority != -1)
            {
                overallMessage.Append("(Priority : " + priority + ")");
            }

            // System.Diagnostics.Trace.WriteLine(overallMessage.ToString());
            Debug.WriteLine(overallMessage.ToString());
        }

        public static void WriteLogExceptionMessage(Exception ex, string exceptionType, bool isTrace = true)
        {
            var dataString = string.Empty;
            if (ex != null)
            {
                dataString += exceptionType;
                dataString += string.Format("\nError Message : {0}\n", ex.Message);
                if (null != ex.InnerException)
                {
                    dataString += string.Format("\nInnerException Message : {0}\n", ex.InnerException.Message);
                }

                Log.Error(dataString, ex);
            }
            else
            {
                dataString += exceptionType;

                Log.Error(dataString);
            }

            if (isTrace)
            {
                Debug.WriteLine("[{0} Error] {1} \r\n{2}", GetMethodInfoStrings.GetMethodName(2), dataString, ex);
            }
        }

        public static void WriteErrorLogAndTrace(string message)
        {
            Log.Error(message);
            Debug.WriteLine(message);
        }

        public static void WriteInfoLog(string logMessage)
        {
            Log.Info(logMessage);
        }

        public static void WriteInfoLogAndTrace(string logMessage)
        {
            Log.Info(logMessage);
            Debug.WriteLine(logMessage);
        }

        public static void WriteErrorLog(string logMessage)
        {
            Log.Error(logMessage);
        }
    }
}
