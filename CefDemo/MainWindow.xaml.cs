using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CefDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();

        }
        private void Init()
        {
            var settings = new CefSettings();
            settings.Locale = "zh-CN";
            settings.CefCommandLineArgs.Add("ppapi-flash-path", System.AppDomain.CurrentDomain.BaseDirectory + "pepflashplayer.dll"); //指定flash的版本，不使用系统安装的flash版本
            settings.CefCommandLineArgs.Add("ppapi-flash-version", "18.0.0.232");
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            if (!Cef.Initialize(settings))
            {

            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var webbrowser = new ChromiumWebBrowser("http://mp.weixin.qq.com/s?__biz=MjM5MzE2ODY4MA==&mid=2663400213&idx=1&sn=30ddc46a88b3165e22fb349b65a6abc8&chksm=bdafcf528ad846441f0ebbf3982e3b2544682bf9596053df252c2f0d9f5cf1e686b950c12560&mpshare=1&scene=23&srcid=1214yMeZeRJCfCR7LgjDDInb#rd");


            var winform = new WindowsFormsHost()
            {
                Child = webbrowser
            };
            this.Content = winform;
        }
    }
}
