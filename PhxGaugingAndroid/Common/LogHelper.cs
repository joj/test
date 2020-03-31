using NLog;
using System;

namespace PhxGaugingAndroid.Common
{
    public class LogHelper
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 重要日志信息
        /// </summary>
        /// <param name="msg"></param>
        public static void InfoLog(string msg)
        {
            log.Info(msg + "\r\n" + System.Environment.NewLine);
        }
        /// <summary>
        /// 错误日志信息
        /// </summary>
        /// <param name="operation">操作</param>
        /// <param name="ex">错误</param>
        public static void ErrorLog(string operation, Exception ex)
        {
            string info = string.Empty;
            if (ex != null)
            {
                info = ex.Message + ">>>" + ex.StackTrace;
            }
            else
            {
                info = operation;
            }

            log.Error(string.Format("{0}发生错误,信息：{1}", operation, info + "\r\n" + System.Environment.NewLine));
        }
        /// <summary>
        /// 调试日志信息
        /// </summary>
        /// <param name="msg"></param>
        public static void DebugLog(string msg)
        {
            log.Debug(msg + "\r\n" + System.Environment.NewLine);
        }
    }
}
