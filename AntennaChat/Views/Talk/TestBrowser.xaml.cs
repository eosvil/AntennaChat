using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Internals;

namespace AntennaChat.Views.Talk
{
    /// <summary>
    /// TestBrowser.xaml 的交互逻辑
    /// </summary>
    public partial class TestBrowser : Window
    {
        public TestBrowser()
        {
            InitializeComponent();
            //Browser.RenderProcessMessageHandler=new RenderProcessMessageHandler();
            Browser.LoadError += Browser_LoadError;
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Browser.ExecuteScriptAsync("alert('IsBrowserInitializedChanged');");
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                e.Frame.ExecuteJavaScriptAsync("alert('MainFrame finished loading');");
            }
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == false)
            {
                Browser.GetMainFrame().ExecuteJavaScriptAsync("alert('All Resources Have Loaded');");
            }
        }

        //private void browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{

        //}

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Random r=new Random();
            var randow = r.Next(1,4);
            var script="";
            if (randow%2 == 0)
            {
                script = string.Format("document.body.style.background = '{0}'", "red");
            }
            else
            {
                script = string.Format("document.body.style.background = '{0}'", "black");
            }
        
            Browser.GetMainFrame().ExecuteJavaScriptAsync(script);
            //Browser.ExecuteScriptAsync(script);
        }

        private void TestBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
            string indexPath = "file:///" + pathHtml + "web_content/index.html";
            Browser.Address = indexPath;
            //Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            //Browser.FrameLoadEnd += Browser_FrameLoadEnd;
        }

        //private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        //{
        //    if (e.Frame.IsMain)
        //    {
        //        e.Frame.ExecuteJavaScriptAsync("alert('MainFrame finished loading');");
        //    }
        //}

        //private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        //{
        //    if (e.IsLoading == false)
        //    {
        //        Browser.ExecuteScriptAsync("alert('All Resources Have Loaded');");
        //    }
        //}
    }
}
