using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SDK.Service
{
    // <summary>
    /// 文件名:  LogHelper.cs
    /// Copyright (c) 2016  华海乐盈
    /// 创建者:  
    /// 创建日期:  2016-06-06
    /// 修改时间：2016-06-08
    /// 描  述: 日志记录接口
    internal class LogHelper
    {
        /// <summary>
        /// 
        /// </summary>
        static object syncLock = new object();
        /// <summary>
        /// 
        /// </summary>
        private static ILog log;
        /// <summary>
        /// 
        /// </summary>
        private static bool isAddErrorAppender = false;
        /// <summary>
        /// 
        /// </summary>
        private static bool isAddInfoAppender = false;
        /// <summary>
        /// 
        /// </summary>
        private static bool isAddDebugAppender = false;
        /// <summary>
        /// 
        /// </summary>
        private static log4net.Repository.ILoggerRepository repository = LogManager.CreateRepository("SDKRepository");
        /// <summary>
        /// 日志队列
        /// </summary>
        public static Queue<LogLevel> ExceptionQueue = new Queue<LogLevel>();

        /// <summary>
        /// 加载log4net配置文件
        /// </summary>
        public static void LoadConfig()
        {
            StartMonitor();
        }

        /// <summary>
        /// 记录debug日志
        /// </summary>
        /// <param name="message">要写入的信息</param>
        public static void WriteDebug(object message)
        {
            if (SdkService.SdkSysParam != null && SdkService.SdkSysParam.DebugLogEnable)
            {
                LogLevel logLevel = new LogLevel();
                logLevel.level = 0;
                logLevel.message = message.ToString();
                WriteLog(logLevel);
            }
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">要写入的信息</param>
        public static void WriteInfo(object message)
        {
            if (SdkService.SdkSysParam!=null&&SdkService.SdkSysParam.InfoLogEnable)
            {
                LogLevel logLevel = new LogLevel();
                logLevel.level = 1;
                logLevel.message = message.ToString();
                WriteLog(logLevel);
            }
        }

        /// <summary>
        /// 记录告警日志
        /// </summary>
        /// <param name="message">要写入的信息</param>
        public static void WriteWarn(object message)
        {
            if (SdkService.SdkSysParam != null && SdkService.SdkSysParam.WarnLogEnable)
            {
                LogLevel logLevel = new LogLevel();
                logLevel.level = 2;
                logLevel.message = message.ToString();
                WriteLog(logLevel);
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">要写入的信息</param>
        public static void WriteError(object message)
        {
            if (SdkService.SdkSysParam != null && SdkService.SdkSysParam.ErrorLogEnable)
            {
                LogLevel logLevel = new LogLevel();
                logLevel.level = 3;
                logLevel.message = message.ToString();
                WriteLog(logLevel);
            }
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message"></param>
        public static void WriteFatal(object message)
        {
            if (SdkService.SdkSysParam.FatalLogEnable)
            {
                LogLevel logLevel = new LogLevel();
                logLevel.level = 4;
                logLevel.message = message.ToString();
                WriteLog(logLevel);
            }
        }

        /// <summary>
        /// 打开日志文件
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        public static void OpenLogFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", filePath);
                }
            }
            catch
            {
            }
        }

        private static void WriteLog(LogLevel logLevel)
        {
            int level = Convert.ToInt32(logLevel.level);
            if (!isAddErrorAppender && level > 2)
            {
                var appender = (RollingFileAppender) CreateFileAppender();
                log4net.Config.BasicConfigurator.Configure(repository, appender);
                isAddErrorAppender = true;
            }
            if (!isAddInfoAppender && level < 3 && level > 0)
            {
                var appender = (RollingFileAppender)CreateTrailFileAppender();
                log4net.Config.BasicConfigurator.Configure(repository, appender);
                isAddInfoAppender = true;
            }
            if (!isAddDebugAppender && level == 0)
            {
                var appender = (RollingFileAppender)CreateDefaultFileAppender();
                log4net.Config.BasicConfigurator.Configure(repository, appender);
                isAddDebugAppender = true;
            }

            if (log == null)
                log = GetLog();
            lock (syncLock)
            {
                ExceptionQueue.Enqueue(logLevel);
            }
        }

        /// <summary>
        /// 获取logger
        /// </summary>
        /// <returns></returns>
        private static ILog GetLog()
        {
            ILog logger = LogManager.GetLogger(repository.Name, "MyLog");
            return logger;
        }

        /// <summary>
        /// 创建默认Appender
        /// DEBUG级别的输出
        /// </summary>
        /// <returns></returns>
        private static IAppender CreateDefaultFileAppender()
        {
            log4net.Filter.LevelRangeFilter levfilter = new log4net.Filter.LevelRangeFilter();
            levfilter.LevelMax = log4net.Core.Level.Debug;
            levfilter.LevelMin = log4net.Core.Level.Debug;
            levfilter.ActivateOptions();
            RollingFileAppender appender = new RollingFileAppender();
            appender.AddFilter(levfilter);
            appender.Name = "DefaultAppender";
            appender.File = "SDK_Logs//Debug//";
            appender.AppendToFile = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
            appender.DatePattern = "yyyy-MM-dd\".txt\"";
            appender.StaticLogFileName = false;
            appender.LockingModel = new FileAppender.MinimalLock();
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "【日志级别】%-5level %n【记录时间】%date{HH:mm:ss,fff}  %n【描    述】%message %n %n";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }

        /// <summary>
        /// 创建默认Appender
        /// INFO、WARN二个级别的输出
        /// </summary>
        /// <returns></returns>
        private static IAppender CreateTrailFileAppender()
        {
            log4net.Filter.LevelRangeFilter levfilter = new log4net.Filter.LevelRangeFilter();
            levfilter.LevelMax = log4net.Core.Level.Warn;
            levfilter.LevelMin = log4net.Core.Level.Info;
            levfilter.ActivateOptions();
            RollingFileAppender appender = new RollingFileAppender();
            appender.AddFilter(levfilter);
            appender.Name = "DefaultAppender";
            appender.File = "SDK_Logs//Info//";
            appender.AppendToFile = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
            appender.DatePattern = "yyyy-MM-dd\".txt\"";
            appender.StaticLogFileName = false;
            appender.LockingModel = new FileAppender.MinimalLock();
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "【日志级别】%-5level %n【记录时间】%date{HH:mm:ss,fff}  %n【描    述】%message %n %n";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }

        /// <summary>
        /// 创建高级Appender
        /// ERROR、FATAL两个级别的输出
        /// </summary>
        /// <returns></returns>
        private static IAppender CreateFileAppender()
        {
            log4net.Filter.LevelRangeFilter levfilter = new log4net.Filter.LevelRangeFilter();
            levfilter.LevelMax = log4net.Core.Level.Fatal;
            levfilter.LevelMin = log4net.Core.Level.Error;
            levfilter.ActivateOptions();
            RollingFileAppender appender = new RollingFileAppender();
            appender.AddFilter(levfilter);
            appender.Name = "errorAppender";
            appender.File = "SDK_Logs//Error//";
            appender.AppendToFile = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
            appender.DatePattern = "yyyy-MM-dd\".txt\"";
            appender.StaticLogFileName = false;
            appender.LockingModel = new FileAppender.MinimalLock();
            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "【日志级别】%-5level %n【记录时间】%date{HH:mm:ss,fff}  %n【描    述】%message %n %n";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }

        /// <summary>
        /// 开始监视日志队列
        /// </summary>
        private static void StartMonitor()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    if (ExceptionQueue.Count > 0)
                    {
                        LogLevel logLevel = null;
                        lock (syncLock)
                        {
                            if (ExceptionQueue.Count > 0)
                            {
                                logLevel = ExceptionQueue.Dequeue();
                            }
                            else
                            {
                                Thread.Sleep(50);
                                continue;
                            }
                        }
                        if (logLevel != null)
                        {
                            switch (logLevel.level)
                            {
                                case 0:
                                    log.Debug(logLevel.message);
                                    break;
                                case 1:
                                    log.Info(logLevel.message);
                                    break;
                                case 2:
                                    log.Warn(logLevel.message);
                                    break;
                                case 3:
                                    log.Error(logLevel.message);
                                    break;
                                case 4:
                                    log.Fatal(logLevel.message);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            });
        }
    }

    /// <summary>
    /// 日志信息和级别
    /// </summary>
    internal class LogLevel
    {
        /// <summary>
        /// 0-debug 1-info 2-warn 3-error
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string message { get; set; } = string.Empty;
    }
}
