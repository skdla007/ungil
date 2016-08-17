using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Commons.Debug.Base
{
    public class BaseMessage : IDisposable
    {
        /// <summary>
        /// The default message prefix.
        /// 0: 스레드 ID, 1: 호출 메서드 이름, 2: 메시지.
        /// </summary>
        private string messageFormat;

        private const string ReceiveRequestMessage = "Request Received.";

        private const string EndRequestMessage = "Request End.";

        private bool isConsoleService;

        /// <summary>
        /// The initialize default message.
        /// </summary>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        /// <param name="queryString">
        /// The query string.
        /// </param>
        protected void InitializeDefaultMessage(string serviceName, string[] queryString = null)
        {
            this.isConsoleService = ServiceTypeInfo.GetTypeInfo() == RunType.ConsoleService;

            try
            {
                Logger.Trace("[chris] : Message Start!");

                var args = string.Empty;

                this.messageFormat = string.Format(
                "[{0}] [Thread ID : {1}] {2} : {{0}}",
                serviceName,
                Thread.CurrentThread.ManagedThreadId,
                GetMethodInfoStrings.GetMethodName(3));

                if (this.isConsoleService)
                {
                    if (queryString != null)
                    {
                        args = queryString.Aggregate(string.Empty, (current, s) => current + string.Format("{0},", s));
                        args = args.Remove(args.Length - 1);
                        args = string.Format("({0}) ", args);
                    }

                    args += ReceiveRequestMessage;

                    Console.WriteLine(this.messageFormat, args);
                }

                var resultMessage = string.Format(this.messageFormat, args);

                Logger.WriteLine(resultMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(this.messageFormat, ex);
                Logger.Trace("[chris] : {0}", ex.ToString());
                Logger.WriteLogExceptionMessage(ex, ex.ToString());
            }
        }

        /// <summary>
        /// The print response messaged.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void PrintResponseMessage(string message = null)
        {
            var sendMessage = string.IsNullOrWhiteSpace(message) ? string.Empty : string.Format("\r\n : {0}", message);
            var sendResponse = string.Format("Send Response Message {0}", sendMessage);

            var resultMessage = string.Format(this.messageFormat, sendResponse);

            Logger.WriteInfoLog(resultMessage);
            Console.WriteLine(resultMessage);
        }

        /// <summary>
        /// The print exception message.
        /// </summary>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public void PrintExceptionMessage(Exception ex)
        {
            var resultMessage = string.Format(this.messageFormat, ex);
            this.LoggingAndConsolePrinting(resultMessage, true);
        }

        public static void PrintExceptionMessage(Exception ex, string methodName)
        {
            var resultMessage = string.Format("[{0} Error] {1}", methodName, ex);

            Logger.WriteLogExceptionMessage(ex, resultMessage);
            Console.WriteLine(resultMessage);
        }

        public static void PrintMessage(string message, bool needLog)
        {
            Console.WriteLine(message);

            if (!needLog)
            {
                return;
            }

            Logger.WriteInfoLog(message);
        }

        /// <summary>
        /// The print message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void PrintMessage(string message)
        {
            var resultMessage = string.Format(this.messageFormat, message);
            this.LoggingAndConsolePrinting(resultMessage);
        }

        private void LoggingAndConsolePrinting(string resultMessage, bool isError = false)
        {
            Task.Factory.StartNew(() =>
            {
                if (isError)
                {
                    Logger.WriteErrorLogAndTrace(resultMessage);
                }
                else
                {
                    Logger.WriteInfoLogAndTrace(resultMessage);
                }

                if (!this.isConsoleService)
                {
                    return;
                }

                Console.WriteLine(resultMessage);
            });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Dispose()
        {
            var resultMessage = string.Format(this.messageFormat, EndRequestMessage);

            Logger.WriteInfoLogAndTrace(resultMessage);

            if (!this.isConsoleService)
            {
                return;
            }

            Console.WriteLine(resultMessage);
        }
    }
}
