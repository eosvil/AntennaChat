using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;

namespace AntennaChat.Views.Activity
{
    /// <summary>
    /// ActivityMapView.xaml 的交互逻辑
    /// </summary>
    public partial class ActivityMapView : Window
    {

        //System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
        //System.Windows.Forms.WebBrowser webBrowser = new System.Windows.Forms.WebBrowser();
        private float _lat;
        private float _lng;
        private string _address;
        private bool _isGetLoaction;
        private WebBrowserOverlay wbo;
        private System.Windows.Forms.WebBrowser fwb;
        private ChromiumWebBrowser webBrowser;
        public ActivityMapView(float lng, float lat, string address, bool isGetLoaction = false)
        {
            InitializeComponent();
            _lat = lat;
            _lng = lng;
            _address = address;
            _isGetLoaction = isGetLoaction;
            //wbo = new WebBrowserOverlay(BdBrowser);
            //fwb = wbo.WebBrowser;
            var url = Environment.CurrentDirectory + "\\Views\\Activity\\BMap.html";
            if (_isGetLoaction)
            {
                url = Environment.CurrentDirectory + "\\Views\\Activity\\Index.html";
                //wbo.Height = 600;
            }
            webBrowser = new ChromiumWebBrowser { Address = url };
            webBrowser.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser.PreviewTextInput += (obj, args) =>
            {
                foreach (var character in args.Text)
                {
                    // 把每个字符向浏览器组件发送一遍
                    webBrowser.GetBrowser().GetHost().SendKeyEvent((int)WM.CHAR, (int)character, 0);
                }

                // 不让cef自己处理
                args.Handled = true;
            };
            //封装WinForm的WebBroswer
            // webBrowser.Url = new Uri(url, UriKind.Absolute);
            //fwb.Navigate(new System.Uri(url));
            //禁止弹出JS中的错误信息，否则会在界面上出现很多的JS错误报告
            //wbo.WebBrowser.ScriptErrorsSuppressed = true;
            //wbo.WebBrowser.WebBrowserShortcutsEnabled = false;
            //wbo.WebBrowser.ScrollBarsEnabled = false;
            // wbo.WebBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            Grid.SetRow(webBrowser, 1);
            grid.Children.Add(webBrowser);
            Border.Visibility = _isGetLoaction ? Visibility.Visible : Visibility.Collapsed;


        }

        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (_isGetLoaction) return;
            if (webBrowser.GetBrowser() != null)
                webBrowser.EvaluateScriptAsync($"LoadLocationMap({_lng},{_lat},'{_address}');");
            //var htmlDocument = wbo.WebBrowser.Document;
            //htmlDocument?.InvokeScript("LoadLocationMap", new object[] { _lng, _lat, _address });
        }


        private void WebBrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (_isGetLoaction) return;
            var htmlDocument = wbo.WebBrowser.Document;
            htmlDocument?.InvokeScript("LoadLocationMap", new object[] { _lng, _lat, _address });
        }

        private void BtnConfirm_OnClick(object sender, RoutedEventArgs e)
        {
            ClosedGetMapInfo();
            webBrowser.Dispose();
            this.Close();
        }
        /// <summary>
        /// 关闭之后获取地图信息
        /// </summary>
        private void ClosedGetMapInfo()
        {
            var Lng = 0f;
            var Lat = 0f;
            var Geo = "";
            var script = string.Format("document.getElementById('lng').innerText;");
            var task = webBrowser.EvaluateScriptAsync(script);
            task.Wait();
            //await task.ContinueWith(t =>
            //{
            //    if (!t.IsFaulted)
            //    {
            var responseLng = task.Result;
            if (responseLng.Success && responseLng.Result != null)
            {
                var strLng = (string)responseLng.Result;
                if (!string.IsNullOrEmpty(strLng))
                    float.TryParse(strLng, out Lng);
            }
            //    }
            //});


            var scriptlat = string.Format("document.getElementById('lat').innerText;");
            var tasklat = webBrowser.EvaluateScriptAsync(scriptlat);
            tasklat.Wait();
            var responseLat = tasklat.Result;
            if (responseLat.Success && responseLat.Result != null)
            {
                var strLat = (string)responseLat.Result;
                if (!string.IsNullOrEmpty(strLat))
                    float.TryParse(strLat, out Lat);
            }

            var scriptgeo = string.Format("document.getElementById('geo').innerText;");
            var taskgeo = webBrowser.EvaluateScriptAsync(scriptgeo);
            taskgeo.Wait();
            var response = taskgeo.Result;
            if (response.Success && response.Result != null)
            {
                Geo = (string)response.Result;
            }

            MapInfo = new MapInfo { Lng = Lng, Lat = Lat, Geo = Geo };
        }

        public MapInfo MapInfo { get; set; }

        private void BtnBackTalkMsg_OnClick(object sender, RoutedEventArgs e)
        {
            ClosedGetMapInfo();
            webBrowser.Dispose();
            this.Close();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        //ReleaseCapture releases a mouse capture
        [DllImportAttribute("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern bool ReleaseCapture();

        //SetWindowLong lets you set a window style
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);


        const int GWL_STYLE = -16;
        const long WS_POPUP = 2147483648;

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
    public class CallbackObjectForJs
    {
        public void loadLocationMap(float lng, float lat, string address)
        {

        }
    }
    public class MapInfo
    {
        public float Lng { get; set; }
        public float Lat { get; set; }
        public string Geo { get; set; }
    }
}
