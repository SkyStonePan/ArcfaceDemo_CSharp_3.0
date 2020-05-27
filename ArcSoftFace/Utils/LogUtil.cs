using log4net;
using System;
namespace ArcSoftFace.Utils
{
    /// <summary>
    /// Log工具类
    /// </summary>
    public class LogUtil
    {
        /// <summary>
        /// loginfo
        /// </summary>
        private static ILog loginfo = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Info等级的日志记录
        /// </summary>
        /// <param name="type">Type</param>param>
        /// <param name="se">info msg</param>
        public static void LogInfo(Type type, string msg)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(type.ToString(), new Exception(msg));
            }
        }

        /// <summary>
        /// Info等级的日志记录
        /// </summary>
        /// <param name="type">Type</param>param>
        /// <param name="se">异常对象</param>
        public static void LogInfo(Type type, Exception ex)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(type.ToString(), ex);
            }
        }
        
    }
}
