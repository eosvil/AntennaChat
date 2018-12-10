using Antenna.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Antenna.UI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        /// <summary>
        /// 处理UI线程未处理异常
        /// </summary>
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogHelper.WriteError("[Current_DispatcherUnhandledException]:" + e.Exception.Message);
            e.Handled = true;
        }
        /// <summary>
        /// 处理其他线程未处理异常
        /// </summary>
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper.WriteError("[CurrentDomain_UnhandledException]" + e.ToString());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            #region 获取配置文件信息
            GlobalVariable.ConfigEntity = new Model.ConfigInfo();
            GlobalVariable.ConfigEntity.HttpPrdfix = ConfigurationManager.AppSettings["HttpPrdfix"];
            GlobalVariable.ConfigEntity.MQTT_Host = ConfigurationManager.AppSettings["MQTT_Host"];
            GlobalVariable.ConfigEntity.MQTT_Password = ConfigurationManager.AppSettings["MQTT_Password"];
            GlobalVariable.ConfigEntity.MQTT_UserName = ConfigurationManager.AppSettings["MQTT_UserName"];
            GlobalVariable.ConfigEntity.DebugLogEnable = bool.Parse(ConfigurationManager.AppSettings["DebugLogEnable"]);
            GlobalVariable.ConfigEntity.InfoLogEnable = bool.Parse(ConfigurationManager.AppSettings["InfoLogEnable"]);
            GlobalVariable.ConfigEntity.WarnLogEnable = bool.Parse(ConfigurationManager.AppSettings["WarnLogEnable"]);
            GlobalVariable.ConfigEntity.ErrorLogEnable = bool.Parse(ConfigurationManager.AppSettings["ErrorLogEnable"]);
            GlobalVariable.ConfigEntity.FatalLogEnable = bool.Parse(ConfigurationManager.AppSettings["FatalLogEnable"]);
            LogHelper.LoadConfig(GlobalVariable.ConfigEntity);
            #endregion
        }
    }
}
