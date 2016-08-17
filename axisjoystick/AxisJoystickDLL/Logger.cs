using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace AxisJoystick
{
    #region
    using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;

    #endregion

    /// <summary>
    /// The logger.
    /// </summary>
    public class Logger
    {
        #region Constants and Fields

        /// <summary>
        ///   The default trace event type.
        /// </summary>
        private const TraceEventType DefaultTraceEventType = TraceEventType.Information;

        #endregion

        #region Public Methods

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Write(string message)
        {
            Write(message, DefaultTraceEventType);
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="severity">
        /// The severity.
        /// </param>
        public void Write(string message, TraceEventType severity)
        {
            Write(string.Empty, message, severity);
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Write(string title, string message)
        {
            Write(title, message, DefaultTraceEventType);
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public void Write(Exception exception)
        {
            var innerException = string.Empty;

            if (exception.InnerException != null)
            {
                innerException = exception.InnerException.Message;
            }

            var message = string.Format(
                "Message:{0}\nInnerException:{1}\nStackTrace:{2}",
                exception.Message,
                innerException,
                exception.StackTrace);
            AxisJoystickLog(exception.ToString());
            //Write("Exception", message, TraceEventType.Error);
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="severity">
        /// The severity.
        /// </param>
        public void Write(string title, string message, TraceEventType severity)
        {
            try
            {
                Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(message, "General", 0, 0, severity, title);

                var logOnConsole = ConfigurationManager.AppSettings["LogOnConsole"];

                if (logOnConsole != null && logOnConsole.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    const string consoleLogFormatter = "{0} | {1,8} | [{2}] - {3}";

                    Console.WriteLine(string.Format(consoleLogFormatter, DateTime.Now, severity, title, message));
                }

                CheckLogFilesForRemove();
            }
            catch (Exception ex)
            {
                //Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(ex.ToString());
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 현재 시간 구하는 함수
        /// </summary>
        public string GetDateTime()
        {
            DateTime NowDate = DateTime.Now;
            return NowDate.ToString("yyyy-MM-dd HH:mm:ss") + ":" + NowDate.Millisecond.ToString("000");
        }

        /// <summary>
        /// 로그파일 생성 또는 기록하는 함수
        /// </summary>
        /// <param name="logMessage">
        /// 로그 메시지
        /// </param>
        public void AxisJoystickLog(string logMessage)
        {

            string FilePath = AppDomain.CurrentDomain.BaseDirectory + @"\Logs\Log" + DateTime.Today.ToString("yyyyMMdd") + ".log";
            string DirPath = AppDomain.CurrentDomain.BaseDirectory + @"\Logs";
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            if (di.Exists != true) Directory.CreateDirectory(DirPath);

            if (fi.Exists != true)
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    temp = string.Format("[{0}] : {1}", GetDateTime(), logMessage);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(FilePath))
                {
                    temp = string.Format("[{0}] : {1}", GetDateTime(), logMessage);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
        }

        /// <summary> 
        /// 일정 기준에 따른 로그 삭제를 위한 메서드 
        /// </summary> 
        public void CheckLogFilesForRemove()
        {
            try
            {
                Configuration entLibConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                LoggingSettings loggingSettings = (LoggingSettings)entLibConfig.GetSection(LoggingSettings.SectionName);
                TraceListenerData traceListenerData = loggingSettings.TraceListeners.Get(2);
                RollingFlatFileTraceListenerData obj = traceListenerData as RollingFlatFileTraceListenerData;

                if (new DirectoryInfo(obj.FileName).Exists)
                {
                    var dirPath = new FileInfo(obj.FileName).DirectoryName;
                    var fullName = new FileInfo(obj.FileName).FullName;
                    var logFiles = Directory.GetFiles(dirPath);
                    var logSize = 0.0;

                    foreach (var logFile in logFiles)
                    {
                        FileInfo fileInfo = new FileInfo(logFile);
                        logSize += fileInfo.Length;

                        if (fileInfo.CreationTime < DateTime.Now.AddMonths(-3))
                        {
                            if (logFile == fullName)
                            {
                                continue;
                            }

                            logSize -= fileInfo.Length;
                            fileInfo.Delete();
                        }
                    }

                    logFiles = Directory.GetFiles(dirPath);

                    if (logSize > 21474836480)
                    {
                        foreach (var logFile in logFiles)
                        {
                            if (logFile == fullName)
                            {
                                continue;
                            }

                            FileInfo files = new FileInfo(logFile);
                            logSize -= files.Length;
                            files.Delete();

                            if (logSize < 21474836480)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(ex.ToString());
            }
        }
        #endregion
    }
}
