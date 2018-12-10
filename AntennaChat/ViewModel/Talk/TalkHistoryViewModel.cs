using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Command;
using AntennaChat.ViewModel.FileUpload;
using AntennaChat.Views.Talk;
using CefSharp.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Antenna.Model.SendMessageDto;
using CefSharp;
using System.Threading.Tasks;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;

namespace AntennaChat.ViewModel.Talk
{
    public class TalkHistoryViewModel : PropertyNotifyObject
    {
        #region 显示区域
        SendMessage_ctt s_ctt;
        AntSdkContact_User user;
        AntSdkGroupInfo GroupInfo;
        List<AntSdkGroupMember> GroupMembers;
        string lastShowTime = "";
        string preTime = "";
        string firstTime = "";
        /// <summary>
        /// 一对一聊天
        /// </summary>
        /// <param name="s_ctt"></param>
        /// <param name="user"></param>
        public TalkHistoryViewModel(SendMessage_ctt s_ctt, AntSdkContact_User user)
        {
            try
            {
                this.s_ctt = s_ctt;
                this.user = user;
                UserName = user.userName;
                this._chromiumWebBrowser.RegisterJsObject("callbackObj", new CallbackObjectForJs());
                _chromiumWebBrowser.IsBrowserInitializedChanged += _chromiumWebBrowser_IsBrowserInitializedChanged;
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_TalkHistoryViewModel(SendMessage_ctt s_ctt, AntSdkContact_User user)]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 群聊
        /// </summary>
        /// <param name="s_ctt"></param>
        /// <param name="groupInfo"></param>
        /// <param name="groupMembers"></param>
        public TalkHistoryViewModel(SendMessage_ctt s_ctt, AntSdkGroupInfo groupInfo, List<AntSdkGroupMember> groupMembers)
        {
            try
            {
                this.s_ctt = s_ctt;
                this.GroupInfo = groupInfo;
                this.GroupMembers = groupMembers;
                UserName = groupInfo.groupName;
                this._chromiumWebBrowser.RegisterJsObject("callbackObj", new CallbackObjectForJs());
                _chromiumWebBrowser.IsBrowserInitializedChanged += _chromiumWebBrowser_IsBrowserInitializedChanged;
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_TalkHistoryViewModel(SendMessage_ctt s_ctt, GroupInfo groupInfo, List<GetGroupMembers_User> groupMembers)]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        public ChromiumWebBrowser cwm = null;
        DispatcherTimer timer;
        private void _chromiumWebBrowser_IsBrowserInitializedChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (GroupInfo == null)
                {
                    #region 显示区域
                    cwm = sender as ChromiumWebBrowser;
                    string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    string indexPath = "file:///" + pathHtml + "web_content/index.html";
                    cwm.Address = indexPath;
                   
                    #endregion
                }
                else
                {
                    cwm = sender as ChromiumWebBrowser;
                    string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    string indexPath = "file:///" + pathHtml + "web_content/group.html";
                    cwm.Address = indexPath;
                  
                }

                TipVisibility = Visibility.Visible;
                if (pageCount > 0)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(500);
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_chromiumWebBrowser_IsBrowserInitializedChanged]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 双击查看图片
        /// </summary>
        public class CallbackObjectForJs
        {
            public void ShowMessage(string id, string src, string title)
            {
                try
                {
                    //System.Windows.MessageBox.Show(id);
                    string imagePath = "";
                    if (src != null)
                    {
                        if (src.StartsWith("http://"))
                        {
                            imagePath = src;
                        }
                        else
                        {
                            imagePath =PublicTalkMothed.strUrlDecode(src.Substring(8, src.Length - 8).Replace(@"/", @"\").Replace("%20", " "));
                        }
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            ShowImage(imagePath);
                        }));
                    }
                }catch(Exception ex)
                {
                    LogHelper.WriteError("[TalkHistoryViewModel_ShowMessage]:" + ex.Message + ex.StackTrace + ex.Source);
                }
            }
            private void ShowImage(string path)
            {
                if (string.IsNullOrEmpty(path)) return;
                PictureViewerView win = new PictureViewerView();
                PictureViewerViewModel model = new PictureViewerViewModel(path);
                win.DataContext = model;
                win.Owner = Antenna.Framework.Win32.GetTopWindow();
                win.Show();
            }
        }
        private object obj = new object();
        int pageCount = 10;
        int counter = 0;
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                string add = "";
                counter++;
                if (GroupInfo == null)
                {
                    totalRow = t_chat_point.GetMaxCount(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.companyCode);
                    GetTotalPage();
                    DisplayPagingInfo();
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat_point.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, 0, pageCount));
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    Task<JavascriptResponse> task = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.add());
                    if (task.Result.Success)
                    {
                        timer.Stop();
                        counter = 0;
                        pageCount = 0;
                        add = task.Result.Result.ToString();
                        LogHelper.WriteDebug("TalkHistoryViewModelChat时间Success:" + DateTime.Now.ToLongTimeString());
                    }
                    if (add == "2")
                    {
                        foreach (var list in listChatdata)
                        {
                            ////Thread.Sleep(50);
                            receiveMsg(list);
                        }
                    }
                    if (counter == 10)
                    {
                        if (add != "2")
                        {
                            timer.Stop();
                            counter = 0;
                        }
                        LogHelper.WriteDebug("TalkGroupViewModelChat_Timer_Tick超时:" + DateTime.Now.ToLongTimeString());
                    }
                }
                else
                {
                    totalRow = t_chat_group.GetMaxCount(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.companyCode);
                    GetTotalPage();
                    DisplayPagingInfo();
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat_group.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, 0, pageCount));
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    Task<JavascriptResponse> task = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.add());
                    if (task.Result.Success)
                    {
                        timer.Stop();
                        counter = 0;
                        pageCount = 0;
                        add = task.Result.Result.ToString();
                        LogHelper.WriteDebug("TalkHistoryViewModelGroup时间Success:" + DateTime.Now.ToLongTimeString());
                    }
                    if (add == "2")
                    {
                        LogHelper.WriteDebug("TalkHistoryViewModelGroup时间while:" + DateTime.Now.ToLongTimeString());
                        foreach (var list in listChatdata)
                        {
                            ////Thread.Sleep(50);
                            receiveMsg(list);
                        }
                    }
                    if (counter == 10)
                    {
                        if (add != "2")
                        {
                            timer.Stop();
                            counter = 0;
                        }
                        LogHelper.WriteDebug("TalkHistoryViewModelGroup_Timer_Tick超时:" + DateTime.Now.ToLongTimeString());
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModelGroup_Timer_Tick]:" + ex.Message + ex.StackTrace + ex.Source);
            }
            finally
            {
                TipVisibility = Visibility.Collapsed;
            }
        }
        private ChromiumWebBrowser _chromiumWebBrowser = new ChromiumWebBrowser();
        public ChromiumWebBrowser chromiumWebBrowser
        {
            set
            {
                _chromiumWebBrowser = value;
                RaisePropertyChanged(() => chromiumWebBrowser);
            }
            get { return _chromiumWebBrowser; }
        }
        private object objMsg = new object();
        #region 接受消息
        public void receiveMsg(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            try
            {
                #region 构造时间差
                if (lastShowTime == "")
                {
                    preTime = DataConverter.GetTimeByTimeStamp(msg.sendTime).ToString();
                    lastShowTime = preTime;
                    firstTime = preTime;
                    this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showHistoryCenterTime(DataConverter.GetTimeByTimeStamp(msg.sendTime)));
                    ////Thread.Sleep(50);
                }
                else
                {
                    DateTime dt = DataConverter.GetTimeByTimeStamp(msg.sendTime);
                    if (PublicTalkMothed.showHistoryTimeSend(dt, preTime))
                    {
                        this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.showHistoryCenterTime(dt));
                        lastShowTime = dt.ToString();
                        ////Thread.Sleep(50);
                    }
                    preTime = dt.ToString();
                }
                #endregion
                lock (objMsg)
                {
                    //一对一聊天解析
                    if (GroupInfo == null)
                    {
                        string pathImage = user.picture == null ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") : user.picture;
                        switch (Convert.ToInt32(/*TODO:AntSdk_Modify:msg.MTP*/msg.MsgType))
                        {

                            #region 文本
                            case (int)GlobalVariable.MsgType.Text:
                                string localEmojiPath = (AppDomain.CurrentDomain.BaseDirectory + "Emoji\\").Replace(@"\", @"/");
                                string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 文本消息右边
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");


                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }
                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");
                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right';");



                                        sbRight.AppendLine("three.innerHTML ='" + showMsg + "';");

                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);}");

                                        sbRight.AppendLine("myFunction();");
                                        var taskRight = this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());
                                        //Console.WriteLine("--------------" + taskRight.Result.ToString());

                                        #endregion
                                        break;
                                    case "0":
                                        #region  文本消息左边
                                        StringBuilder sbLeft = new StringBuilder();
                                        sbLeft.AppendLine("function myFunction()");
                                        sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeft.AppendLine("nodeFirst.className='leftd';");

                                        sbLeft.AppendLine("var second=document.createElement('div');");
                                        sbLeft.AppendLine("second.className='leftimg';");

                                        //string imgUrl = "http://www.bing.com/az/hprichbg/rb/SalteeGannets_ZH-CN12304087974_1366x768.jpg";


                                        sbLeft.AppendLine("var img = document.createElement('img');");


                                        sbLeft.AppendLine("img.src='" + pathImage + "';");
                                        sbLeft.AppendLine("img.className='divcss5Left';");
                                        sbLeft.AppendLine("second.appendChild(img);");
                                        sbLeft.AppendLine("nodeFirst.appendChild(second);");

                                        sbLeft.AppendLine("var node=document.createElement('div');");
                                        sbLeft.AppendLine("node.className='speech left';");

                                        //string localEmojiPath = (AppDomain.CurrentDomain.BaseDirectory + "Emoji\\").Replace(@"\", @"/");
                                        //string showMsg = PublicTalkMothed.talkContentReplace(msg.content);


                                        sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

                                        sbLeft.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                                        sbLeft.AppendLine("myFunction();");

                                        var taskLeft = this._chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                        //Console.WriteLine("--------------" + taskLeft.Result.ToString());
                                        break;
                                }

                                #endregion
                                break;
                            #endregion

                            #region 截图
                            case (int)GlobalVariable.MsgType.Picture:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 图片消息右边
                                        SendImageDto limgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");


                                        //string imgUrl = "http://www.divcss5.com/yanshi/2014/2014063001/images/1.jpg";

                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }

                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");



                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right';");

                                        sbRight.AppendLine("var img1 = document.createElement('img');");


                                        sbRight.AppendLine("img1.src='" + limgDto.thumbnailUrl + "';");
                                        //if (limgDto.imgSize == null)
                                        //{
                                        //    sbRight.AppendLine("img1.style.width='300px';");
                                        //    sbRight.AppendLine("img1.style.height='400px';");
                                        //}
                                        //else
                                        //{
                                        //    string[] LlistImageHandW = limgDto.imgSize.Split('_');
                                        //    if (Convert.ToInt32(LlistImageHandW[0]) > 500)
                                        //    {
                                        //        sbRight.AppendLine("img1.style.width='500px';");
                                        //    }
                                        //    sbRight.AppendLine("img1.style.height='" + LlistImageHandW[1] + "px';");
                                        //}
                                        sbRight.AppendLine("img1.style.width='100%';");
                                        sbRight.AppendLine("img1.style.height='100%';");
                                        sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                                        sbRight.AppendLine("img1.title='双击查看原图';");
                                        sbRight.AppendLine("three.appendChild(img1);");

                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);");

                                        sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                                        sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                                        sbRight.AppendLine("}");

                                        sbRight.AppendLine("myFunction();");
                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());
                                        #endregion
                                        break;
                                    case "0":
                                        #region 图片消息左边
                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                        StringBuilder sbLeftImage = new StringBuilder();
                                        sbLeftImage.AppendLine("function myFunction()");
                                        sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeftImage.AppendLine("nodeFirst.className='leftd';");

                                        sbLeftImage.AppendLine("var second=document.createElement('div');");
                                        sbLeftImage.AppendLine("second.className='leftimg';");

                                        //string imgUrlLeft = "http://www.bing.com/az/hprichbg/rb/SalteeGannets_ZH-CN12304087974_1366x768.jpg";


                                        sbLeftImage.AppendLine("var img = document.createElement('img');");
                                        sbLeftImage.AppendLine("img.src='" + pathImage + "';");
                                        sbLeftImage.AppendLine("img.className='divcss5Left';");
                                        sbLeftImage.AppendLine("second.appendChild(img);");
                                        sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

                                        sbLeftImage.AppendLine("var node=document.createElement('div');");
                                        sbLeftImage.AppendLine("node.className='speech left';");

                                        sbLeftImage.AppendLine("var img = document.createElement('img');");

                                        //if (rimgDto.imgSize == null)
                                        //{
                                        //    sbLeftImage.AppendLine("img.style.width='300px';");
                                        //    sbLeftImage.AppendLine("img.style.height='400px';");
                                        //}
                                        //else
                                        //{
                                        //    string[] listImageHandW = rimgDto.imgSize.Split('_');
                                        //    if (Convert.ToInt32(listImageHandW[0]) > 500)
                                        //    {
                                        //        sbLeftImage.AppendLine("img.style.width='500px';");
                                        //    }
                                        //    sbLeftImage.AppendLine("img.style.height='" + listImageHandW[1] + "px';");
                                        //}
                                        sbLeftImage.AppendLine("img.style.width='100%';");
                                        sbLeftImage.AppendLine("img.style.height='100%';");
                                        sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
                                        sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                                        sbLeftImage.AppendLine("img.title='双击查看原图';");

                                        sbLeftImage.AppendLine("node.appendChild(img);");

                                        sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
                                        sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                                        sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                                        sbLeftImage.AppendLine("}");

                                        sbLeftImage.AppendLine("myFunction();");
                                        lock (obj)
                                        {
                                            var taskLeft = this._chromiumWebBrowser.EvaluateScriptAsync(sbLeftImage.ToString());
                                        }
                                        #endregion
                                        break;
                                }
                                #region 图片消息

                                #endregion
                                break;
                            #endregion

                            #region 文件
                            case (int)GlobalVariable.MsgType.File:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":

                                        UpLoadFilesDto Lreceive = JsonConvert.DeserializeObject<UpLoadFilesDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                                        #region 文件消息右边
                                        StringBuilder rsbFileUpload = new StringBuilder();
                                        rsbFileUpload.AppendLine("function myFunction()");

                                        rsbFileUpload.AppendLine("{ var first=document.createElement('div');");
                                        rsbFileUpload.AppendLine("first.className='rightd';");


                                        rsbFileUpload.AppendLine("var seconds=document.createElement('div');");
                                        rsbFileUpload.AppendLine("seconds.className='rightimg';");


                                        //string imgUrl = "http://www.divcss5.com/yanshi/2014/2014063001/images/1.jpg";
                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }

                                        rsbFileUpload.AppendLine("var img = document.createElement('img');");
                                        rsbFileUpload.AppendLine("img.src='" + imgUrl + "';");
                                        rsbFileUpload.AppendLine("img.className='divcss5';");
                                        rsbFileUpload.AppendLine("seconds.appendChild(img);");
                                        rsbFileUpload.AppendLine("first.appendChild(seconds);");


                                        rsbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
                                        rsbFileUpload.AppendLine("bubbleDiv.className='speech right';");

                                        rsbFileUpload.AppendLine("first.appendChild(bubbleDiv);");

                                        rsbFileUpload.AppendLine("var second=document.createElement('div');");
                                        rsbFileUpload.AppendLine("second.className='fileDiv';");

                                        rsbFileUpload.AppendLine("bubbleDiv.appendChild(second);");




                                        rsbFileUpload.AppendLine("var three=document.createElement('div');");
                                        rsbFileUpload.AppendLine("three.className='fileDivOne';");

                                        rsbFileUpload.AppendLine("second.appendChild(three);");



                                        rsbFileUpload.AppendLine("var four=document.createElement('div');");
                                        rsbFileUpload.AppendLine("four.className='fileImg';");

                                        rsbFileUpload.AppendLine("three.appendChild(four);");

                                        //文件显示图片类型
                                        rsbFileUpload.AppendLine("var fileimage = document.createElement('img');");
                                        if (Lreceive.fileExtendName == null)
                                        {
                                            Lreceive.fileExtendName = Lreceive.fileName.Substring((Lreceive.fileName.LastIndexOf('.') + 1), Lreceive.fileName.Length - 1 - Lreceive.fileName.LastIndexOf('.'));
                                            //sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                                        }
                                        rsbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(Lreceive.fileExtendName, Lreceive.localOrServerPath) + "';");

                                        rsbFileUpload.AppendLine("four.appendChild(fileimage);");



                                        rsbFileUpload.AppendLine("var five=document.createElement('div');");
                                        rsbFileUpload.AppendLine("five.className='fileName';");
                                        rsbFileUpload.AppendLine("five.title='" + Lreceive.fileName + "';");
                                        string rfileName = Lreceive.fileName.Length > 12 ? Lreceive.fileName.Substring(0, 10) + "..." : Lreceive.fileName;
                                        rsbFileUpload.AppendLine("five.innerText='" + rfileName + "';");


                                        rsbFileUpload.AppendLine("three.appendChild(five);");

                                        rsbFileUpload.AppendLine("var six=document.createElement('div');");
                                        rsbFileUpload.AppendLine("six.className='fileSize';");
                                        rsbFileUpload.AppendLine("six.innerText='" + Lreceive.Size + "';");
                                        //listfile.fileSize = listfile.fileSize;
                                        rsbFileUpload.AppendLine("three.appendChild(six);");


                                        rsbFileUpload.AppendLine("var seven=document.createElement('div');");
                                        rsbFileUpload.AppendLine("seven.className='fileProgressDiv';");

                                        rsbFileUpload.AppendLine("second.appendChild(seven);");

                                        //进度条
                                        rsbFileUpload.AppendLine("var sevenFist=document.createElement('div');");
                                        rsbFileUpload.AppendLine("sevenFist.className='processcontainer';");

                                        rsbFileUpload.AppendLine("seven.appendChild(sevenFist);");

                                        rsbFileUpload.AppendLine("var sevenSecond=document.createElement('div');");
                                        rsbFileUpload.AppendLine("sevenSecond.className='processbar';");
                                        string rprogressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        Lreceive.progressId = rprogressId;
                                        rsbFileUpload.AppendLine("sevenSecond.id='" + rprogressId + "';");
                                        //rsbFileUpload.AppendLine("sevenSecond.style.width='30%';");

                                        rsbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");


                                        rsbFileUpload.AppendLine("var eight=document.createElement('div');");
                                        rsbFileUpload.AppendLine("eight.className='fileOperateDiv';");

                                        rsbFileUpload.AppendLine("second.appendChild(eight);");

                                        rsbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
                                        rsbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");

                                        rsbFileUpload.AppendLine("eight.appendChild(imgSorR);");

                                        //上传中图片添加
                                        rsbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
                                        rsbFileUpload.AppendLine("showSOFImg.className='onging';");
                                        rsbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
                                        string rshowImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        Lreceive.fileImgGuid = rshowImgGuid;
                                        rsbFileUpload.AppendLine("showSOFImg.id='" + rshowImgGuid + "';");

                                        rsbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


                                        rsbFileUpload.AppendLine("var night=document.createElement('div');");
                                        rsbFileUpload.AppendLine("night.className='fileRorS';");

                                        rsbFileUpload.AppendLine("eight.appendChild(night);");

                                        //上传中添加文字
                                        rsbFileUpload.AppendLine("var nightButton=document.createElement('button');");
                                        rsbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
                                        string rfileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        rsbFileUpload.AppendLine("nightButton.id='" + rfileshowText + "';");
                                        Lreceive.fileTextGuid = rfileshowText;
                                        rsbFileUpload.AppendLine("nightButton.innerHTML='上传中';");

                                        rsbFileUpload.AppendLine("night.appendChild(nightButton);");



                                        rsbFileUpload.AppendLine("var ten=document.createElement('div');");
                                        rsbFileUpload.AppendLine("ten.className='fileOpen';");

                                        rsbFileUpload.AppendLine("eight.appendChild(ten);");

                                        //打开按钮添加
                                        rsbFileUpload.AppendLine("var btnten=document.createElement('button');");
                                        rsbFileUpload.AppendLine("btnten.className='btnOpenFile';");
                                        string rfileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        rsbFileUpload.AppendLine("btnten.id='" + rfileOpenguid + "';");

                                        //后期处理
                                        // string localPath = Lreceive.localOrServerPath.Replace(@"\", @"/");


                                        //rsbFileUpload.AppendLine("btnten.value='" + localPath + "';");
                                        rsbFileUpload.AppendLine("btnten.innerHTML='打开';");
                                        rsbFileUpload.AppendLine("ten.appendChild(btnten);");

                                        rsbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");

                                        rsbFileUpload.AppendLine("var eleven=document.createElement('div');");
                                        rsbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

                                        rsbFileUpload.AppendLine("eight.appendChild(eleven);");

                                        //打开文件夹按钮添加
                                        rsbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
                                        rsbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                                        string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        rsbFileUpload.AppendLine("btnEleven.id='" + fileOpenDirectory + "';");

                                        //后期处理
                                        //string localPathDirectory = Lreceive.localOrServerPath.Replace(@"\", @"/");
                                        //rsbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");


                                        rsbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
                                        rsbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
                                        rsbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");

                                        rsbFileUpload.AppendLine("document.body.appendChild(first);");

                                        rsbFileUpload.AppendLine("}");

                                        rsbFileUpload.AppendLine("myFunction();");

                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(rsbFileUpload.ToString());
                                        #endregion
                                        break;
                                    case "0":
                                        #region 文件消息左边
                                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                        #region 构造发送文件解析
                                        StringBuilder sbFileUpload = new StringBuilder();
                                        sbFileUpload.AppendLine("function myFunction()");

                                        sbFileUpload.AppendLine("{ var first=document.createElement('div');");
                                        sbFileUpload.AppendLine("first.className='leftd';");


                                        sbFileUpload.AppendLine("var seconds=document.createElement('div');");
                                        sbFileUpload.AppendLine("seconds.className='leftimg';");


                                        sbFileUpload.AppendLine("var img = document.createElement('img');");
                                        //string pathImage = user.picture == null ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/头像-个人资料.png").Replace(@"\", @"/") : user.picture;
                                        sbFileUpload.AppendLine("img.src='" + pathImage + "';");
                                        sbFileUpload.AppendLine("img.className='divcss5Left';");
                                        sbFileUpload.AppendLine("seconds.appendChild(img);");
                                        sbFileUpload.AppendLine("first.appendChild(seconds);");


                                        sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
                                        sbFileUpload.AppendLine("bubbleDiv.className='speech left';");

                                        sbFileUpload.AppendLine("first.appendChild(bubbleDiv);");

                                        sbFileUpload.AppendLine("var second=document.createElement('div');");
                                        sbFileUpload.AppendLine("second.className='fileDiv';");

                                        sbFileUpload.AppendLine("bubbleDiv.appendChild(second);");




                                        sbFileUpload.AppendLine("var three=document.createElement('div');");
                                        sbFileUpload.AppendLine("three.className='fileDivOne';");

                                        sbFileUpload.AppendLine("second.appendChild(three);");



                                        sbFileUpload.AppendLine("var four=document.createElement('div');");
                                        sbFileUpload.AppendLine("four.className='fileImg';");

                                        sbFileUpload.AppendLine("three.appendChild(four);");

                                        //文件显示图片类型
                                        sbFileUpload.AppendLine("var fileimage = document.createElement('img');");
                                        if (receive.fileExtendName == null)
                                        {
                                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                                            //sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                                        }
                                        sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");

                                        sbFileUpload.AppendLine("four.appendChild(fileimage);");



                                        sbFileUpload.AppendLine("var five=document.createElement('div');");
                                        sbFileUpload.AppendLine("five.className='fileName';");

                                        sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
                                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                                        sbFileUpload.AppendLine("five.innerText='" + fileName + "';");

                                        //sbFileUpload.AppendLine("five.innerText='" + receive.fileName + "';");


                                        sbFileUpload.AppendLine("three.appendChild(five);");

                                        sbFileUpload.AppendLine("var six=document.createElement('div');");
                                        sbFileUpload.AppendLine("six.className='fileSize';");
                                        sbFileUpload.AppendLine("six.innerText='" + receive.Size + "';");
                                        //listfile.fileSize = listfile.fileSize;
                                        sbFileUpload.AppendLine("three.appendChild(six);");


                                        sbFileUpload.AppendLine("var seven=document.createElement('div');");
                                        sbFileUpload.AppendLine("seven.className='fileProgressDiv';");

                                        sbFileUpload.AppendLine("second.appendChild(seven);");

                                        //进度条
                                        sbFileUpload.AppendLine("var sevenFist=document.createElement('div');");
                                        sbFileUpload.AppendLine("sevenFist.className='processcontainer';");

                                        sbFileUpload.AppendLine("seven.appendChild(sevenFist);");

                                        sbFileUpload.AppendLine("var sevenSecond=document.createElement('div');");
                                        sbFileUpload.AppendLine("sevenSecond.className='processbar';");
                                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.progressId = progressId;
                                        sbFileUpload.AppendLine("sevenSecond.id='" + progressId + "';");
                                        //sbFileUpload.AppendLine("sevenSecond.style.width='30%';");

                                        sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");


                                        sbFileUpload.AppendLine("var eight=document.createElement('div');");
                                        sbFileUpload.AppendLine("eight.className='fileOperateDiv';");

                                        sbFileUpload.AppendLine("second.appendChild(eight);");

                                        sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
                                        sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");

                                        sbFileUpload.AppendLine("eight.appendChild(imgSorR);");

                                        //接收图片添加
                                        sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
                                        sbFileUpload.AppendLine("showSOFImg.className='onging';");
                                        sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("reveiving", "") + "';");
                                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.fileImgGuid = showImgGuid;
                                        sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");

                                        sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


                                        sbFileUpload.AppendLine("var night=document.createElement('div');");
                                        sbFileUpload.AppendLine("night.className='fileRorS';");

                                        sbFileUpload.AppendLine("eight.appendChild(night);");

                                        //接收中添加文字
                                        sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
                                        sbFileUpload.AppendLine("nightButton.className='fileUploadProgressLeft';");
                                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
                                        receive.fileTextGuid = fileshowText;
                                        sbFileUpload.AppendLine("nightButton.innerHTML='未下载';");

                                        sbFileUpload.AppendLine("night.appendChild(nightButton);");



                                        sbFileUpload.AppendLine("var ten=document.createElement('div');");
                                        sbFileUpload.AppendLine("ten.className='fileOpen';");

                                        sbFileUpload.AppendLine("eight.appendChild(ten);");

                                        //打开
                                        sbFileUpload.AppendLine("var btnten=document.createElement('button');");
                                        sbFileUpload.AppendLine("btnten.className='btnOpenFileLeft';");
                                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");

                                        sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
                                        receive.fileOpenGuid = fileOpenguid;

                                        sbFileUpload.AppendLine("btnten.innerHTML='接收';");
                                        sbFileUpload.AppendLine("ten.appendChild(btnten);");

                                        sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");

                                        sbFileUpload.AppendLine("var eleven=document.createElement('div');");
                                        sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

                                        sbFileUpload.AppendLine("eight.appendChild(eleven);");

                                        //打开文件夹
                                        sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
                                        sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.fileDirectoryGuid = fileDirectoryId;
                                        sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                                        sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
                                        sbFileUpload.AppendLine("btnEleven.value='';");
                                        sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");

                                        sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

                                        sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

                                        sbFileUpload.AppendLine("document.body.appendChild(first);");



                                        sbFileUpload.AppendLine("}");

                                        sbFileUpload.AppendLine("myFunction();");
                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbFileUpload.ToString());
                                        #endregion
                                        #endregion
                                        break;
                                }
                                break;
                            #endregion

                            #region 文本
                            case (int)GlobalVariable.MsgType.Voice:

                                mp3Dto msgDto = JsonConvert.DeserializeObject<mp3Dto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 文本消息右边
                                        #region 圆形图片Right
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");


                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }
                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");
                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right divmp3_contain';");

                                        ////PublicTalkMothed.talkContentReplace(msg.content);
                                        //sbRight.AppendLine("var audio=document.createElement('audio');");
                                        //sbRight.AppendLine("audio.controls='controls';");



                                        //sbRight.AppendLine("var source=document.createElement('source');");
                                        //sbRight.AppendLine("source.src='"+ msgDto.audioUrl + "';");
                                        ////sbRight.AppendLine("source.type='audio/ogg';");

                                        //sbRight.AppendLine("audio.appendChild(source);");

                                        sbRight.AppendLine("var div3 = document.createElement('div');");
                                        sbRight.AppendLine("div3.innerHTML ='音频文件暂不支持';");
                                        sbRight.AppendLine("div3.className ='divmp3';");
                                        sbRight.AppendLine("three.appendChild(div3);");

                                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        sbRight.AppendLine("var imgmp3 = document.createElement('img');");
                                        sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
                                        sbRight.AppendLine("imgmp3.className ='divmp3';");
                                        sbRight.AppendLine("three.appendChild(imgmp3);");


                                        //sbRight.AppendLine("three.innerHTML ='音频文件暂不支持';");
                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);}");

                                        sbRight.AppendLine("myFunction();");

                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());

                                        #endregion
                                        #endregion
                                        break;
                                    case "0":
                                        #region 文本消息左边
                                        StringBuilder sbLeft = new StringBuilder();
                                        sbLeft.AppendLine("function myFunction()");
                                        sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeft.AppendLine("nodeFirst.className='leftd';");

                                        sbLeft.AppendLine("var second=document.createElement('div');");
                                        sbLeft.AppendLine("second.className='leftimg';");

                                        //string imgUrl = "http://www.bing.com/az/hprichbg/rb/SalteeGannets_ZH-CN12304087974_1366x768.jpg";


                                        sbLeft.AppendLine("var img = document.createElement('img');");


                                        sbLeft.AppendLine("img.src='" + pathImage + "';");
                                        sbLeft.AppendLine("img.className='divcss5Left';");
                                        sbLeft.AppendLine("second.appendChild(img);");
                                        sbLeft.AppendLine("nodeFirst.appendChild(second);");

                                        sbLeft.AppendLine("var node=document.createElement('div');");
                                        sbLeft.AppendLine("node.className='speech left';");

                                        string musicImgleft = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
                                        sbLeft.AppendLine("imgmp3.src='" + musicImgleft + "';");
                                        sbLeft.AppendLine("imgmp3.className ='divmp3';");
                                        sbLeft.AppendLine("node.appendChild(imgmp3);");

                                        sbLeft.AppendLine("var div3 = document.createElement('div');");
                                        sbLeft.AppendLine("div3.innerHTML ='音频文件暂不支持';");
                                        sbLeft.AppendLine("div3.className ='divmp3';");
                                        sbLeft.AppendLine("node.appendChild(div3);");

                                        sbLeft.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                                        sbLeft.AppendLine("myFunction();");

                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                        break;
                                        #endregion
                                }
                                break;
                                #endregion
                        }
                    }
                    //群聊天解析
                    else
                    {
                        //获取接收者头像
                        string localEmojiPath = (AppDomain.CurrentDomain.BaseDirectory + "Emoji\\").Replace(@"\", @"/");
                        string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        AntSdkGroupMember user = GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId);
                        string pathImage = "";
                        if (user == null)
                        {
                            AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
                            if (cus != null)
                            {
                                user = new AntSdkGroupMember();
                                if (cus.picture == null)
                                {
                                    pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                }
                                else
                                {
                                    pathImage = cus.picture;
                                }
                                //user.picture = cus.picture;
                                user.userName = cus.userName;
                            }
                            if (cus == null)
                            {
                                pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                                user.userName = "离职人员";
                            }
                        }
                        //if (user == null)
                        //{
                        //    pathImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/头像-个人资料.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //}
                        else
                        {
                            pathImage = (user.picture == null) || (user.picture == "") ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") : user.picture;
                        }
                        switch (Convert.ToInt32(/*TODO:AntSdk_Modify:msg.MTP*/ msg.MsgType))
                        {
                            #region 文本
                            case (int)GlobalVariable.MsgType.Text:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 圆形图片Right
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");


                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }
                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");
                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right';");


                                        sbRight.AppendLine("three.innerHTML ='" + showMsg + "';");

                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);}");

                                        sbRight.AppendLine("myFunction();");

                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());

                                        #endregion
                                        break;
                                    case "0":
                                        #region  文本消息左边
                                        StringBuilder sbLeft = new StringBuilder();
                                        sbLeft.AppendLine("function myFunction()");
                                        sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeft.AppendLine("nodeFirst.className='leftd';");

                                        sbLeft.AppendLine("var second=document.createElement('div');");
                                        sbLeft.AppendLine("second.className='leftimg';");


                                        sbLeft.AppendLine("var img = document.createElement('img');");
                                        sbLeft.AppendLine("img.src='" + pathImage + "';");
                                        sbLeft.AppendLine("img.className='divcss5Left';");
                                        sbLeft.AppendLine("second.appendChild(img);");
                                        sbLeft.AppendLine("nodeFirst.appendChild(second);");

                                        //添加用户名称
                                        sbLeft.AppendLine("var username=document.createElement('div');");
                                        sbLeft.AppendLine("username.className='leftUsername';");
                                        sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
                                        sbLeft.AppendLine("nodeFirst.appendChild(username);");

                                        sbLeft.AppendLine("var node=document.createElement('div');");
                                        sbLeft.AppendLine("node.className='speech left';");

                                        sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

                                        sbLeft.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                                        sbLeft.AppendLine("myFunction();");

                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());

                                        #endregion
                                        break;
                                }
                                break;
                            #endregion

                            #region 截图
                            case (int)GlobalVariable.MsgType.Picture:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 图片消息右边
                                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");

                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }

                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");



                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right';");

                                        sbRight.AppendLine("var img1 = document.createElement('img');");


                                        sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
                                        //if (rimgDto.imgSize == null)
                                        //{
                                        //    sbRight.AppendLine("img1.style.width='300px';");
                                        //    sbRight.AppendLine("img1.style.height='400px';");
                                        //}
                                        //else
                                        //{
                                        //    string[] listImageHandW = rimgDto.imgSize.Split('_');

                                        //    if (Convert.ToInt32(listImageHandW[0]) > 500)
                                        //    {
                                        //        sbRight.AppendLine("img1.style.width='500px';");
                                        //    }
                                        //    sbRight.AppendLine("img1.style.height='" + listImageHandW[1] + "px';");
                                        //}
                                        sbRight.AppendLine("img1.style.width='100%';");
                                        sbRight.AppendLine("img1.style.height='100%';");

                                        sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                                        sbRight.AppendLine("img1.title='双击查看原图';");
                                        sbRight.AppendLine("three.appendChild(img1);");

                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);");

                                        sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                                        sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                                        
                                        sbRight.AppendLine("}");

                                        sbRight.AppendLine("myFunction();");

                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());

                                        StringBuilder sbEnds = new StringBuilder();
                                        sbEnds.AppendLine("setscross();");
                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbEnds.ToString());
                                        #endregion
                                        break;
                                    case "0":
                                        #region 图片消息左边
                                        SendImageDto LimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                                        StringBuilder sbLeftImage = new StringBuilder();
                                        sbLeftImage.AppendLine("function myFunction()");
                                        sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeftImage.AppendLine("nodeFirst.className='leftd';");

                                        sbLeftImage.AppendLine("var second=document.createElement('div');");
                                        sbLeftImage.AppendLine("second.className='leftimg';");

                                        //string imgUrlLeft = "http://www.bing.com/az/hprichbg/rb/SalteeGannets_ZH-CN12304087974_1366x768.jpg";


                                        sbLeftImage.AppendLine("var img = document.createElement('img');");
                                        sbLeftImage.AppendLine("img.src='" + pathImage + "';");
                                        sbLeftImage.AppendLine("img.className='divcss5Left';");
                                        sbLeftImage.AppendLine("second.appendChild(img);");
                                        sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

                                        //添加用户名称
                                        sbLeftImage.AppendLine("var username=document.createElement('div');");
                                        sbLeftImage.AppendLine("username.className='leftUsername';");
                                        sbLeftImage.AppendLine("username.innerHTML ='" + user.userName + "';");
                                        sbLeftImage.AppendLine("nodeFirst.appendChild(username);");

                                        sbLeftImage.AppendLine("var node=document.createElement('div');");
                                        sbLeftImage.AppendLine("node.className='speech left';");

                                        sbLeftImage.AppendLine("var img = document.createElement('img');");
                                        //if (LimgDto.imgSize == null)
                                        //{
                                        //    sbLeftImage.AppendLine("img.style.width='300px';");
                                        //    sbLeftImage.AppendLine("img.style.height='400px';");
                                        //}
                                        //else
                                        //{
                                        //    string[] LlistImageHandW = LimgDto.imgSize.Split('_');
                                        //    if (Convert.ToInt32(LlistImageHandW[0]) > 500)
                                        //    {
                                        //        sbLeftImage.AppendLine("img.style.width='500px';");
                                        //    }
                                        //    sbLeftImage.AppendLine("img.style.height='" + LlistImageHandW[1] + "px';");
                                        //}
                                        sbLeftImage.AppendLine("img.style.width='100%';");
                                        sbLeftImage.AppendLine("img.style.height='100%';");
                                        sbLeftImage.AppendLine("img.src='" + LimgDto.picUrl + "';");
                                        sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                                        sbLeftImage.AppendLine("img.title='双击查看原图';");

                                        sbLeftImage.AppendLine("node.appendChild(img);");

                                        sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
                                        sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                                        sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

                                        sbLeftImage.AppendLine("}");

                                        sbLeftImage.AppendLine("myFunction();");

                                        var taskLeft = this._chromiumWebBrowser.EvaluateScriptAsync(sbLeftImage.ToString());

                                        StringBuilder LsbEnds = new StringBuilder();
                                        LsbEnds.AppendLine("setscross();");
                                        this._chromiumWebBrowser.EvaluateScriptAsync(LsbEnds.ToString());
                                        #endregion
                                        break;
                                }
                                break;
                            #endregion

                            #region 文件
                            case (int)GlobalVariable.MsgType.File:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 文件消息右边
                                        UpLoadFilesDto Lreceive = JsonConvert.DeserializeObject<UpLoadFilesDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                                        StringBuilder LsbFileUpload = new StringBuilder();
                                        LsbFileUpload.AppendLine("function myFunction()");

                                        LsbFileUpload.AppendLine("{ var first=document.createElement('div');");
                                        LsbFileUpload.AppendLine("first.className='rightd';");


                                        LsbFileUpload.AppendLine("var seconds=document.createElement('div');");
                                        LsbFileUpload.AppendLine("seconds.className='rightimg';");


                                        //string imgUrl = "http://www.divcss5.com/yanshi/2014/2014063001/images/1.jpg";
                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }

                                        LsbFileUpload.AppendLine("var img = document.createElement('img');");
                                        LsbFileUpload.AppendLine("img.src='" + imgUrl + "';");
                                        LsbFileUpload.AppendLine("img.className='divcss5';");
                                        LsbFileUpload.AppendLine("seconds.appendChild(img);");
                                        LsbFileUpload.AppendLine("first.appendChild(seconds);");

                                        //添加用户名称
                                        //LsbFileUpload.AppendLine("var username=document.createElement('div');");
                                        //LsbFileUpload.AppendLine("username.className='leftUsername';");
                                        //LsbFileUpload.AppendLine("username.innerHTML ='" + user.userName + "';");
                                        //LsbFileUpload.AppendLine("first.appendChild(username);");


                                        LsbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
                                        LsbFileUpload.AppendLine("bubbleDiv.className='speech right';");

                                        LsbFileUpload.AppendLine("first.appendChild(bubbleDiv);");

                                        LsbFileUpload.AppendLine("var second=document.createElement('div');");
                                        LsbFileUpload.AppendLine("second.className='fileDiv';");

                                        LsbFileUpload.AppendLine("bubbleDiv.appendChild(second);");




                                        LsbFileUpload.AppendLine("var three=document.createElement('div');");
                                        LsbFileUpload.AppendLine("three.className='fileDivOne';");

                                        LsbFileUpload.AppendLine("second.appendChild(three);");



                                        LsbFileUpload.AppendLine("var four=document.createElement('div');");
                                        LsbFileUpload.AppendLine("four.className='fileImg';");

                                        LsbFileUpload.AppendLine("three.appendChild(four);");

                                        //文件显示图片类型
                                        LsbFileUpload.AppendLine("var fileimage = document.createElement('img');");
                                        if (Lreceive.fileExtendName == null)
                                        {
                                            Lreceive.fileExtendName = Lreceive.fileName.Substring((Lreceive.fileName.LastIndexOf('.') + 1), Lreceive.fileName.Length - 1 - Lreceive.fileName.LastIndexOf('.'));
                                            //sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                                        }
                                        LsbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(Lreceive.fileExtendName, Lreceive.localOrServerPath) + "';");

                                        LsbFileUpload.AppendLine("four.appendChild(fileimage);");



                                        LsbFileUpload.AppendLine("var five=document.createElement('div');");
                                        LsbFileUpload.AppendLine("five.className='fileName';");
                                        LsbFileUpload.AppendLine("five.title='" + Lreceive.fileName + "';");
                                        string LfileName = Lreceive.fileName.Length > 12 ? Lreceive.fileName.Substring(0, 10) + "..." : Lreceive.fileName;
                                        LsbFileUpload.AppendLine("five.innerText='" + LfileName + "';");


                                        LsbFileUpload.AppendLine("three.appendChild(five);");

                                        LsbFileUpload.AppendLine("var six=document.createElement('div');");
                                        LsbFileUpload.AppendLine("six.className='fileSize';");
                                        LsbFileUpload.AppendLine("six.innerText='" + Lreceive.fileSize + "';");
                                        //listfile.fileSize = listfile.fileSize;
                                        LsbFileUpload.AppendLine("three.appendChild(six);");


                                        LsbFileUpload.AppendLine("var seven=document.createElement('div');");
                                        LsbFileUpload.AppendLine("seven.className='fileProgressDiv';");

                                        LsbFileUpload.AppendLine("second.appendChild(seven);");

                                        //进度条
                                        LsbFileUpload.AppendLine("var sevenFist=document.createElement('div');");
                                        LsbFileUpload.AppendLine("sevenFist.className='processcontainer';");

                                        LsbFileUpload.AppendLine("seven.appendChild(sevenFist);");

                                        LsbFileUpload.AppendLine("var sevenSecond=document.createElement('div');");
                                        LsbFileUpload.AppendLine("sevenSecond.className='processbar';");
                                        string LprogressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        Lreceive.progressId = LprogressId;
                                        LsbFileUpload.AppendLine("sevenSecond.id='" + LprogressId + "';");
                                        //LsbFileUpload.AppendLine("sevenSecond.style.width='30%';");

                                        LsbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");


                                        LsbFileUpload.AppendLine("var eight=document.createElement('div');");
                                        LsbFileUpload.AppendLine("eight.className='fileOperateDiv';");

                                        LsbFileUpload.AppendLine("second.appendChild(eight);");

                                        LsbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
                                        LsbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");

                                        LsbFileUpload.AppendLine("eight.appendChild(imgSorR);");

                                        //上传中图片添加
                                        LsbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
                                        LsbFileUpload.AppendLine("showSOFImg.className='onging';");
                                        LsbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
                                        string LshowImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        Lreceive.fileImgGuid = LshowImgGuid;
                                        LsbFileUpload.AppendLine("showSOFImg.id='" + LshowImgGuid + "';");

                                        LsbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


                                        LsbFileUpload.AppendLine("var night=document.createElement('div');");
                                        LsbFileUpload.AppendLine("night.className='fileRorS';");

                                        LsbFileUpload.AppendLine("eight.appendChild(night);");

                                        //上传中添加文字
                                        LsbFileUpload.AppendLine("var nightButton=document.createElement('button');");
                                        LsbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
                                        string LfileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        LsbFileUpload.AppendLine("nightButton.id='" + LfileshowText + "';");
                                        Lreceive.fileTextGuid = LfileshowText;
                                        LsbFileUpload.AppendLine("nightButton.innerHTML='上传中';");

                                        LsbFileUpload.AppendLine("night.appendChild(nightButton);");



                                        LsbFileUpload.AppendLine("var ten=document.createElement('div');");
                                        LsbFileUpload.AppendLine("ten.className='fileOpen';");

                                        LsbFileUpload.AppendLine("eight.appendChild(ten);");

                                        //打开按钮添加
                                        LsbFileUpload.AppendLine("var btnten=document.createElement('button');");
                                        LsbFileUpload.AppendLine("btnten.className='btnOpenFile';");
                                        string LfileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        LsbFileUpload.AppendLine("btnten.id='" + LfileOpenguid + "';");

                                        //后期处理
                                        //string localPath = Lreceive.localOrServerPath.Replace(@"\", @"/");
                                        //LsbFileUpload.AppendLine("btnten.value='" + localPath + "';");

                                        LsbFileUpload.AppendLine("btnten.innerHTML='打开';");
                                        LsbFileUpload.AppendLine("ten.appendChild(btnten);");

                                        LsbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");

                                        LsbFileUpload.AppendLine("var eleven=document.createElement('div');");
                                        LsbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

                                        LsbFileUpload.AppendLine("eight.appendChild(eleven);");

                                        //打开文件夹按钮添加
                                        LsbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
                                        LsbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                                        string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
                                        LsbFileUpload.AppendLine("btnEleven.id='" + fileOpenDirectory + "';");

                                        //后期处理
                                        //string localPathDirectory = Lreceive.localOrServerPath.Replace(@"\", @"/");
                                        //LsbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");

                                        LsbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
                                        LsbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
                                        LsbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");

                                        LsbFileUpload.AppendLine("document.body.appendChild(first);");

                                        LsbFileUpload.AppendLine("}");

                                        LsbFileUpload.AppendLine("myFunction();");

                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(LsbFileUpload.ToString());
                                        #endregion
                                        break;
                                    case "0":
                                        #region 文件消息左边
                                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                                        StringBuilder sbFileUpload = new StringBuilder();
                                        sbFileUpload.AppendLine("function myFunction()");

                                        sbFileUpload.AppendLine("{ var first=document.createElement('div');");
                                        sbFileUpload.AppendLine("first.className='leftd';");


                                        sbFileUpload.AppendLine("var seconds=document.createElement('div');");
                                        sbFileUpload.AppendLine("seconds.className='leftimg';");


                                        sbFileUpload.AppendLine("var img = document.createElement('img');");
                                        //string pathImage = user.picture == null ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/头像-个人资料.png").Replace(@"\", @"/") : user.picture;
                                        sbFileUpload.AppendLine("img.src='" + pathImage + "';");
                                        sbFileUpload.AppendLine("img.className='divcss5Left';");
                                        sbFileUpload.AppendLine("seconds.appendChild(img);");
                                        sbFileUpload.AppendLine("first.appendChild(seconds);");

                                        //添加用户名称
                                        sbFileUpload.AppendLine("var username=document.createElement('div');");
                                        sbFileUpload.AppendLine("username.className='leftUsername';");
                                        sbFileUpload.AppendLine("username.innerHTML ='" + user.userName + "';");
                                        sbFileUpload.AppendLine("first.appendChild(username);");

                                        sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
                                        sbFileUpload.AppendLine("bubbleDiv.className='speech left';");

                                        sbFileUpload.AppendLine("first.appendChild(bubbleDiv);");

                                        sbFileUpload.AppendLine("var second=document.createElement('div');");
                                        sbFileUpload.AppendLine("second.className='fileDiv';");

                                        sbFileUpload.AppendLine("bubbleDiv.appendChild(second);");




                                        sbFileUpload.AppendLine("var three=document.createElement('div');");
                                        sbFileUpload.AppendLine("three.className='fileDivOne';");

                                        sbFileUpload.AppendLine("second.appendChild(three);");



                                        sbFileUpload.AppendLine("var four=document.createElement('div');");
                                        sbFileUpload.AppendLine("four.className='fileImg';");

                                        sbFileUpload.AppendLine("three.appendChild(four);");

                                        //文件显示图片类型
                                        sbFileUpload.AppendLine("var fileimage = document.createElement('img');");
                                        if (receive.fileExtendName == null)
                                        {
                                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                                            //sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                                        }
                                        sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");

                                        sbFileUpload.AppendLine("four.appendChild(fileimage);");



                                        sbFileUpload.AppendLine("var five=document.createElement('div');");
                                        sbFileUpload.AppendLine("five.className='fileName';");
                                        sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
                                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                                        sbFileUpload.AppendLine("five.innerText='" + fileName + "';");


                                        sbFileUpload.AppendLine("three.appendChild(five);");

                                        sbFileUpload.AppendLine("var six=document.createElement('div');");
                                        sbFileUpload.AppendLine("six.className='fileSize';");
                                        sbFileUpload.AppendLine("six.innerText='" + receive.Size + "';");
                                        //listfile.fileSize = listfile.fileSize;
                                        sbFileUpload.AppendLine("three.appendChild(six);");


                                        sbFileUpload.AppendLine("var seven=document.createElement('div');");
                                        sbFileUpload.AppendLine("seven.className='fileProgressDiv';");

                                        sbFileUpload.AppendLine("second.appendChild(seven);");

                                        //进度条
                                        sbFileUpload.AppendLine("var sevenFist=document.createElement('div');");
                                        sbFileUpload.AppendLine("sevenFist.className='processcontainer';");

                                        sbFileUpload.AppendLine("seven.appendChild(sevenFist);");

                                        sbFileUpload.AppendLine("var sevenSecond=document.createElement('div');");
                                        sbFileUpload.AppendLine("sevenSecond.className='processbar';");
                                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.progressId = progressId;
                                        sbFileUpload.AppendLine("sevenSecond.id='" + progressId + "';");
                                        //sbFileUpload.AppendLine("sevenSecond.style.width='30%';");

                                        sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");


                                        sbFileUpload.AppendLine("var eight=document.createElement('div');");
                                        sbFileUpload.AppendLine("eight.className='fileOperateDiv';");

                                        sbFileUpload.AppendLine("second.appendChild(eight);");

                                        sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
                                        sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");

                                        sbFileUpload.AppendLine("eight.appendChild(imgSorR);");

                                        //接收图片添加
                                        sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
                                        sbFileUpload.AppendLine("showSOFImg.className='onging';");
                                        sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("reveiving", "") + "';");
                                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.fileImgGuid = showImgGuid;
                                        sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");

                                        sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


                                        sbFileUpload.AppendLine("var night=document.createElement('div');");
                                        sbFileUpload.AppendLine("night.className='fileRorS';");

                                        sbFileUpload.AppendLine("eight.appendChild(night);");

                                        //接收中添加文字
                                        sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
                                        sbFileUpload.AppendLine("nightButton.className='fileUploadProgressLeft';");
                                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                                        sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
                                        receive.fileTextGuid = fileshowText;
                                        sbFileUpload.AppendLine("nightButton.innerHTML='未下载';");

                                        sbFileUpload.AppendLine("night.appendChild(nightButton);");



                                        sbFileUpload.AppendLine("var ten=document.createElement('div');");
                                        sbFileUpload.AppendLine("ten.className='fileOpen';");

                                        sbFileUpload.AppendLine("eight.appendChild(ten);");

                                        //打开
                                        sbFileUpload.AppendLine("var btnten=document.createElement('button');");
                                        sbFileUpload.AppendLine("btnten.className='btnOpenFileLeft';");
                                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");

                                        sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
                                        receive.fileOpenGuid = fileOpenguid;

                                        sbFileUpload.AppendLine("btnten.innerHTML='接收';");
                                        sbFileUpload.AppendLine("ten.appendChild(btnten);");

                                        sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");

                                        sbFileUpload.AppendLine("var eleven=document.createElement('div');");
                                        sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

                                        sbFileUpload.AppendLine("eight.appendChild(eleven);");

                                        //打开文件夹
                                        sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
                                        sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                                        receive.fileDirectoryGuid = fileDirectoryId;
                                        sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                                        sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
                                        sbFileUpload.AppendLine("btnEleven.value='';");
                                        sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");

                                        sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

                                        sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

                                        sbFileUpload.AppendLine("document.body.appendChild(first);");



                                        sbFileUpload.AppendLine("}");

                                        sbFileUpload.AppendLine("myFunction();");

                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbFileUpload.ToString());
                                        #endregion
                                        break;
                                }
                                break;
                            #endregion
                            #region 文本
                            case (int)GlobalVariable.MsgType.Voice:
                                switch (msg.SENDORRECEIVE)
                                {
                                    case "1":
                                        #region 圆形图片Right
                                        StringBuilder sbRight = new StringBuilder();
                                        sbRight.AppendLine("function myFunction()");

                                        sbRight.AppendLine("{ var first=document.createElement('div');");
                                        sbRight.AppendLine("first.className='rightd';");

                                        sbRight.AppendLine("var second=document.createElement('div');");
                                        sbRight.AppendLine("second.className='rightimg';");


                                        string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                                        if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                                        {
                                            imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        }
                                        sbRight.AppendLine("var img = document.createElement('img');");
                                        sbRight.AppendLine("img.src='" + imgUrl + "';");
                                        sbRight.AppendLine("img.className='divcss5';");
                                        sbRight.AppendLine("second.appendChild(img);");
                                        sbRight.AppendLine("first.appendChild(second);");

                                        sbRight.AppendLine("var three=document.createElement('div');");
                                        sbRight.AppendLine("three.className='speech right divmp3_contain';");

                                        ////PublicTalkMothed.talkContentReplace(msg.content);
                                        //sbRight.AppendLine("var audio=document.createElement('audio');");
                                        //sbRight.AppendLine("audio.controls='controls';");



                                        //sbRight.AppendLine("var source=document.createElement('source');");
                                        //sbRight.AppendLine("source.src='"+ msgDto.audioUrl + "';");
                                        ////sbRight.AppendLine("source.type='audio/ogg';");

                                        //sbRight.AppendLine("audio.appendChild(source);");

                                        sbRight.AppendLine("var div3 = document.createElement('div');");
                                        sbRight.AppendLine("div3.innerHTML ='音频文件暂不支持';");
                                        sbRight.AppendLine("div3.className ='divmp3';");
                                        sbRight.AppendLine("three.appendChild(div3);");

                                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        sbRight.AppendLine("var imgmp3 = document.createElement('img');");
                                        sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
                                        sbRight.AppendLine("imgmp3.className ='divmp3';");
                                        sbRight.AppendLine("three.appendChild(imgmp3);");

                                        sbRight.AppendLine("first.appendChild(three);");
                                        sbRight.AppendLine("document.body.appendChild(first);}");

                                        sbRight.AppendLine("myFunction();");

                                        var task = this._chromiumWebBrowser.EvaluateScriptAsync(sbRight.ToString());

                                        #endregion
                                        break;
                                    case "0":
                                        StringBuilder sbLeft = new StringBuilder();
                                        sbLeft.AppendLine("function myFunction()");
                                        sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                                        sbLeft.AppendLine("nodeFirst.className='leftd';");


                                        sbLeft.AppendLine("var second=document.createElement('div');");
                                        sbLeft.AppendLine("second.className='leftimg';");
                                        sbLeft.AppendLine("nodeFirst.appendChild(second);");

                                        //string imgUrl = "http://www.bing.com/az/hprichbg/rb/SalteeGannets_ZH-CN12304087974_1366x768.jpg";

                                        sbLeft.AppendLine("var img = document.createElement('img');");
                                        sbLeft.AppendLine("img.src='" + pathImage + "';");
                                        sbLeft.AppendLine("img.className='divcss5Left';");
                                        sbLeft.AppendLine("second.appendChild(img);");


                                        //添加用户名称
                                        sbLeft.AppendLine("var username=document.createElement('div');");
                                        sbLeft.AppendLine("username.className='leftUsername';");
                                        sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
                                        sbLeft.AppendLine("nodeFirst.appendChild(username);");

                                        sbLeft.AppendLine("var node=document.createElement('div');");
                                        sbLeft.AppendLine("node.className='speech left';");

                                        string musicImgleft = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
                                        sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
                                        sbLeft.AppendLine("imgmp3.src='" + musicImgleft + "';");
                                        sbLeft.AppendLine("imgmp3.className ='divmp3';");
                                        sbLeft.AppendLine("node.appendChild(imgmp3);");

                                        sbLeft.AppendLine("var div3 = document.createElement('div');");
                                        sbLeft.AppendLine("div3.innerHTML ='音频文件暂不支持';");
                                        sbLeft.AppendLine("div3.className ='divmp3';");
                                        sbLeft.AppendLine("node.appendChild(div3);");

                                        sbLeft.AppendLine("nodeFirst.appendChild(node);");

                                        sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                                        sbLeft.AppendLine("myFunction();");

                                        this._chromiumWebBrowser.EvaluateScriptAsync(sbLeft.ToString());
                                        break;
                                }
                                break;
                                #endregion
                        }
                    }

                    #region 滚动条置底
                    StringBuilder sbEnd = new StringBuilder();
                    sbEnd.AppendLine("setscross();");
                    var taskEnd = this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                    #endregion
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_receiveMsg]" + ex.Message + ex.StackTrace + /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent != null ? /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent : "");
            }
        }
        #endregion
        #endregion
        #region 字段
        /// <summary>
        /// 当前页数
        /// </summary>
        private int currentPageNum = 0;
        /// <summary>
        /// 总条数
        /// </summary>
        private int totalRow = 0;
        /// <summary>
        /// 选中日期之前的总记录条数
        /// </summary>
        private int totalRowBeforeSelectDate = 0;
        /// <summary>
        /// 选中日期返回的记录最后一条之后的记录条数
        /// </summary>
        private int totalRowAfterSelectDate = 0;
        /// <summary>
        /// 总页数
        /// </summary>
        private int totalPages = 0;
        /// <summary>
        /// 选中日期之前记录的总页数
        /// </summary>
        private int PagesBefore = 0;
        /// <summary>
        /// 计算选中日期返回记录的最后一条之后的记录总页数
        /// </summary>
        private int PagesAfter = 0;
        /// <summary>
        /// 当前消息索引
        /// </summary>
        private int currentIndex = 0;
        /// <summary>
        /// 每页显示条数
        /// </summary>
        private int everyPageRow = 10;
        /// <summary>
        /// 一对一聊天
        /// </summary>
        T_Chat_MessageDAL t_chat_point = new T_Chat_MessageDAL();
        /// <summary>
        /// 讨论组
        /// </summary>
        T_Chat_Message_GroupDAL t_chat_group = new T_Chat_Message_GroupDAL();
        /// <summary>
        /// 通过选择日期查询，返回结果的开始索引
        /// </summary>
        private int startIndex = 0;
        /// <summary>
        ///  通过选择日期查询，返回结果的结束索引
        /// </summary>
        private int endIndex = 0;
        /// <summary>
        /// 是否通过选中日期查询记录
        /// </summary>
        private bool isSelectedByDateTime = false;
        #endregion
        #region 属性
        /// <summary>
        /// 用户名称
        /// </summary>
        private string _UserName;
        public string UserName
        {
            get { return this._UserName; }
            set
            {
                this._UserName = value;
                RaisePropertyChanged(() => UserName);
            }
        }
        /// <summary>
        /// 首页按钮是否禁用
        /// </summary>
        private bool _HomeIsEnabled = false;
        public bool HomeIsEnabled
        {
            get { return this._HomeIsEnabled; }
            set
            {
                this._HomeIsEnabled = value;
                RaisePropertyChanged(() => HomeIsEnabled);
            }
        }
        /// <summary>
        /// 上一页按钮是否禁用
        /// </summary>
        private bool _PreviousIsEnabled = false;
        public bool PreviousIsEnabled
        {
            get { return this._PreviousIsEnabled; }
            set
            {
                this._PreviousIsEnabled = value;
                RaisePropertyChanged(() => PreviousIsEnabled);
            }
        }
        /// <summary>
        /// 下一页按钮是否禁用
        /// </summary>
        private bool _NextIsEnabled;
        public bool NextIsEnabled
        {
            get { return this._NextIsEnabled; }
            set
            {
                this._NextIsEnabled = value;
                RaisePropertyChanged(() => NextIsEnabled);
            }
        }
        /// <summary>
        /// 末页按钮是否禁用
        /// </summary>
        private bool _LastIsEnabled;
        public bool LastIsEnabled
        {
            get { return this._LastIsEnabled; }
            set
            {
                this._LastIsEnabled = value;
                RaisePropertyChanged(() => LastIsEnabled);
            }
        }
        /// <summary>
        /// 加载提示框是否显示
        /// </summary>
        private Visibility _TipVisibility = Visibility.Collapsed;
        public Visibility TipVisibility
        {
            get { return this._TipVisibility; }
            set
            {
                this._TipVisibility = value;
                RaisePropertyChanged(() => TipVisibility);
            }
        }
        #endregion
        #region 命令
        /// <summary>
        /// 首页
        /// </summary>
        private ICommand _HomeCommand;
        public ICommand HomeCommand
        {
            get
            {
                if (this._HomeCommand == null)
                {
                    this._HomeCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            currentPageNum = isSelectedByDateTime ? totalPages - 2 : totalPages - 1;
                            isSelectedByDateTime = false;
                            UpdateChatdata();
                            DisplayPagingInfo();
                            StringBuilder sbEnd = new StringBuilder();
                            sbEnd.AppendLine("setscross();");
                            this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        catch(Exception ex)
                        {
                            LogHelper.WriteError("[TalkHistoryViewModel_HomeCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._HomeCommand;
            }
        }
        /// <summary>
        /// 上一页
        /// </summary>
        private ICommand _PreviousCommand;
        public ICommand PreviousCommand
        {
            get
            {
                if (this._PreviousCommand == null)
                {
                    this._PreviousCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            if (isSelectedByDateTime)
                            {
                                UpdateChatdata(false);
                                StringBuilder sbEnd = new StringBuilder();
                                currentPageNum++;
                                DisplayPagingInfo();
                                sbEnd.AppendLine("setscross();");
                                this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                            }
                            else
                            {
                                if (currentPageNum < totalPages)
                                {
                                    currentPageNum++;
                                    UpdateChatdata();
                                    DisplayPagingInfo();
                                    StringBuilder sbEnd = new StringBuilder();
                                    sbEnd.AppendLine("setscross();");
                                    this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            LogHelper.WriteError("[TalkHistoryViewModel_PreviousCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._PreviousCommand;
            }
        }
        /// <summary>
        /// 下一页
        /// </summary>
        private ICommand _NextCommand;
        public ICommand NextCommand
        {
            get
            {
                if (this._NextCommand == null)
                {
                    this._NextCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            if (isSelectedByDateTime)
                            {
                                currentPageNum--;
                                UpdateChatdata(true);
                                DisplayPagingInfo();
                                StringBuilder sbEnd = new StringBuilder();
                                sbEnd.AppendLine("setscross();");
                                this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                            }
                            else
                            {
                                if (currentPageNum > -1)
                                {
                                    currentPageNum--;
                                    UpdateChatdata();
                                    DisplayPagingInfo();
                                    StringBuilder sbEnd = new StringBuilder();
                                    sbEnd.AppendLine("setscross();");
                                    this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            LogHelper.WriteError("[TalkHistoryViewModel_NextCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._NextCommand;
            }
        }
        /// <summary>
        /// 末页
        /// </summary>
        private ICommand _LastCommand;
        public ICommand LastCommand
        {
            get
            {
                if (this._LastCommand == null)
                {
                    this._LastCommand = new DefaultCommand(o =>
                    {
                        try
                        {
                            isSelectedByDateTime = false;
                            currentPageNum = 0;
                            UpdateChatdata();
                            DisplayPagingInfo();
                            StringBuilder sbEnd = new StringBuilder();
                            sbEnd.AppendLine("setscross();");
                            this._chromiumWebBrowser.EvaluateScriptAsync(sbEnd.ToString());
                        }
                        catch(Exception ex)
                        {
                            LogHelper.WriteError("[TalkHistoryViewModel_LastCommand]:" + ex.Message + ex.StackTrace + ex.Source);
                        }
                    });
                }
                return this._LastCommand;
            }
        }
        /// <summary>
        /// 日期变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DatePicker dp = sender as DatePicker;
                string selecteDate = dp.Text;
                DateTime dt_Start;
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                dtFormat.ShortDatePattern = "yyyy/MM/dd";
                dt_Start = Convert.ToDateTime(selecteDate, dtFormat);
                string tm_Start = DataConverter.ConvertDateTimeInt(dt_Start).ToString();
                DateTime dt_End = dt_Start.AddHours(23).AddMinutes(59).AddSeconds(59);
                string tm_End = DataConverter.ConvertDateTimeInt(dt_End).ToString();
                if (GroupInfo==null)
                {
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat_point.GetCurrentDayHistoryMsg(s_ctt.sessionId, tm_Start, tm_End, everyPageRow));
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    chromiumWebBrowser.Address = null;
                    string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    string indexPath = "file:///" + pathHtml + "web_content/index.html";
                    chromiumWebBrowser.Address = indexPath;
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    startIndex = Convert.ToInt32(listChatdata[0].chatIndex);
                    endIndex = Convert.ToInt32(listChatdata[listChatdata.Count - 1].chatIndex);
                    totalRowBeforeSelectDate= t_chat_point.GetHistoryCountPrevious(s_ctt.sessionId, startIndex.ToString());
                    totalRowAfterSelectDate=t_chat_point.GetHistoryCountNext(s_ctt.sessionId, endIndex.ToString());
                    isSelectedByDateTime = true;
                    GetTotalPage();
                    DisplayPagingInfo();
                    foreach (var list in listChatdata)
                    {
                        ////Thread.Sleep(50);
                        receiveMsg(list);
                    }
                }
                else
                {
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel(t_chat_group.GetCurrentDayHistoryMsg(s_ctt.sessionId, tm_Start, tm_End, everyPageRow));
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    chromiumWebBrowser.Address = null;
                    string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    string indexPath = "file:///" + pathHtml + "web_content/group.html";
                    chromiumWebBrowser.Address = indexPath;
                    if (listChatdata == null || listChatdata.Count == 0) return;
                    startIndex = Convert.ToInt32(listChatdata[0].chatIndex);
                    endIndex = Convert.ToInt32(listChatdata[listChatdata.Count - 1].chatIndex);
                    totalRowBeforeSelectDate = t_chat_group.GetHistoryCountPrevious(s_ctt.sessionId, startIndex.ToString());
                    totalRowAfterSelectDate = t_chat_group.GetHistoryCountNext(s_ctt.sessionId, endIndex.ToString());
                    isSelectedByDateTime = true;
                    GetTotalPage();
                    DisplayPagingInfo();
                    foreach (var list in listChatdata)
                    {
                        ////Thread.Sleep(50);
                        receiveMsg(list);
                    }
                }
                //todo
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_DatePickerSelectedDateChanged]:" + ex.Message + "," + ex.StackTrace);
            }
        }
        #endregion
        #region 其他方法
        /// <summary>
        /// 更新聊天记录
        /// </summary>
        private void UpdateChatdata()
        {
            try
            {
                int index = currentPageNum * everyPageRow;
                if (GroupInfo == null)
                {
                    if (!isSelectedByDateTime)
                    {
                        totalRow = t_chat_point.GetMaxCount(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.companyCode);
                        GetTotalPage();
                        DisplayPagingInfo();
                    }
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                        (t_chat_point.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, index, everyPageRow));
                    if (listChatdata.Count > 0)
                    {
                        //cwm.Address = null;
                        //string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                        //string indexPath = "file:///" + pathHtml + "web_content/index.html";
                        //cwm.Address = indexPath;
                        var taskEnd = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.clearHtml());
                        foreach (var list in listChatdata)
                        {
                            ////Thread.Sleep(50);
                            receiveMsg(list);
                        }
                    }
                }
                else
                {
                    if (!isSelectedByDateTime)
                    {
                        totalRow = t_chat_group.GetMaxCount(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.companyCode);
                        GetTotalPage();
                        DisplayPagingInfo();
                    }
                    IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                        (t_chat_group.GetDataTable(s_ctt.sessionId, s_ctt.sendUserId, s_ctt.targetId, s_ctt.companyCode, index, everyPageRow));
                    if (listChatdata.Count > 0)
                    {
                        //cwm.Address = null;
                        //string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                        //string indexPath = "file:///" + pathHtml + "web_content/group.html";
                        //cwm.Address = indexPath;
                        var taskEnd = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.clearHtml());
                        foreach (var list in listChatdata)
                        {
                            ////Thread.Sleep(50);
                            receiveMsg(list);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_UpdateChatdata]:" + ex.Message + "," + ex.StackTrace);
            }
        }
        /// <summary>
        /// 更新聊天记录（用于日期选择）
        /// </summary>
        /// <param name="isNext">是否为下一页</param>
        private void UpdateChatdata(bool isNext)
        {
            try
            {
                IList<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> listChatdata = null;
                if (GroupInfo == null)
                {
                    if (isNext)
                        listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                        (t_chat_point.GetHistoryNext(s_ctt.sessionId, this.endIndex.ToString(), everyPageRow));
                    else
                        listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                       (t_chat_point.GetHistoryPrevious(s_ctt.sessionId, this.startIndex.ToString(), everyPageRow));
                    //cwm.Address = null;
                    //string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    //string indexPath = "file:///" + pathHtml + "web_content/index.html";
                    //cwm.Address = indexPath;
                    var taskEnd = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.clearHtml());
                    
                }
                else
                {
                    if (isNext)
                        listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                        (t_chat_group.GetHistoryNext(s_ctt.sessionId, this.endIndex.ToString(), everyPageRow));
                    else
                        listChatdata = AntSdkSqliteHelper.ModelConvertHelper<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase>.ConvertToChatMsgModel
                       (t_chat_group.GetHistoryPrevious(s_ctt.sessionId, this.startIndex.ToString(), everyPageRow));
                    //cwm.Address = null;
                    //string pathHtml = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\", @"/");
                    //string indexPath = "file:///" + pathHtml + "web_content/group.html";
                    //cwm.Address = indexPath;
                    var taskEnd = this._chromiumWebBrowser.EvaluateScriptAsync(PublicTalkMothed.clearHtml());
                }
                if (listChatdata == null || listChatdata.Count == 0) return;
                startIndex = Convert.ToInt32(listChatdata[0].chatIndex);
                endIndex = Convert.ToInt32(listChatdata[listChatdata.Count - 1].chatIndex);

                foreach (var list in listChatdata)
                {
                    ////Thread.Sleep(50);
                    receiveMsg(list);
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_UpdateChatdata]:" + ex.Message + "," + ex.StackTrace);
                
            }
        }
        /// <summary>  
        /// 设置按钮状态  
        /// </summary>  
        private void DisplayPagingInfo()
        {
            try
            {
                if (this.currentPageNum == 0)
                {
                    this.NextIsEnabled = false;
                    this.LastIsEnabled = false;
                }
                else
                {
                    this.NextIsEnabled = true;
                    this.LastIsEnabled = true;
                }
                if (this.currentPageNum == this.totalPages - 1)
                {
                    this.HomeIsEnabled = false;
                    this.PreviousIsEnabled = false;
                }
                else
                {
                    this.HomeIsEnabled = true;
                    this.PreviousIsEnabled = true;
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_DisplayPagingInfo]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        /// <summary>
        /// 计算最大页面
        /// </summary>
        private void GetTotalPage()
        {
            try
            {
                if (!isSelectedByDateTime)
                {
                    //多少页  
                    int Pages = totalRow / everyPageRow;
                    if (totalRow != (Pages * everyPageRow))
                    {
                        if (totalRow < (Pages * everyPageRow))
                            Pages--;
                        else
                            Pages++;
                    }
                    this.totalPages = Pages == 0 ? 1 : Pages;
                }
                else
                {
                    //计算选中日期之前的记录总页数
                    PagesBefore = totalRowBeforeSelectDate / everyPageRow;
                    if (totalRowBeforeSelectDate != (PagesBefore * everyPageRow))
                    {
                        if (totalRowBeforeSelectDate < (PagesBefore * everyPageRow))
                            PagesBefore--;
                        else
                            PagesBefore++;
                    }
                    //计算选中日期返回记录的最后一条之后的记录总页数
                    PagesAfter = totalRowAfterSelectDate / everyPageRow;
                    if (totalRowAfterSelectDate != (PagesAfter * everyPageRow))
                    {
                        if (totalRowAfterSelectDate < (PagesAfter * everyPageRow))
                            PagesAfter--;
                        else
                            PagesAfter++;
                    }
                    this.totalPages = PagesBefore + PagesAfter + 1;
                    this.currentPageNum = this.totalPages - PagesBefore - 1;
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError("[TalkHistoryViewModel_GetTotalPage]:" + ex.Message + ex.StackTrace + ex.Source);
            }
        }
        #endregion
    }
}
