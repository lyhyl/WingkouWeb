using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WingkouWeb.Utils
{
    public static class Logger
    {
        public static void SetConfig(string root, string logName)
        {
            FileAppender fAppender = new FileAppender();
            PatternLayout layout = new PatternLayout
            {
                ConversionPattern = "[%date] %thread -- %-5level -- %logger [%M] -- %message%newline"
            };

            layout.ActivateOptions();

            fAppender.File = Path.Combine(root, $@"Log\{logName}.log");
            fAppender.Layout = layout;
            fAppender.AppendToFile = true;
            fAppender.ActivateOptions();
            BasicConfigurator.Configure(fAppender);
        }

        public static void WriteLog(string info)
        {
            if (LogInfo.IsInfoEnabled)
            {
                try
                {
                    LogInfo.Info(info);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(info);
#endif
                }
                catch
                {
                }
            }
        }

        public static void WriteLog(string info, Exception se)
        {
            if (LogError.IsErrorEnabled)
            {
                try
                {
                    LogError.Error(info, se);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(info);
#endif
                }
                catch
                {
                }
            }
        }

        private static readonly ILog LogInfo = LogManager.GetLogger("loginfo");
        private static readonly ILog LogError = LogManager.GetLogger("logerror");
    }
}