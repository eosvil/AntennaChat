using Antenna.Framework;
using Antenna.Model;
using AntennaChat.ViewModel.FileUpload;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using static Antenna.Framework.GlobalVariable;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AntennaChat.Resource;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;
using System.Runtime.InteropServices;
using static SDK.AntSdk.AntModels.AntSdkChatMsg;
using Antenna.Model.PictureAndTextMix;
using AntennaChat.CefSealedHelper.OneAndGroupCommon;
using static Antenna.Model.ModelEnum;
using SDK.Service.Models;

namespace AntennaChat.ViewModel.Talk
{
    public class PublicTalkMothed
    {
        public static bool isRegester { set; get; }
        public static bool isOneToOneRegester { set; get; }
        /// <summary>
        /// 文件大小转换
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string FileSize(string length)
        {
            long lengthFile = Convert.ToInt64(length);
            string fileSize = "";
            if (lengthFile < 1024)
            {
                fileSize = length.ToString() + "B";
            }
            if (lengthFile > 1024)
            {
                fileSize = Math.Round((double)lengthFile / 1024, 2) + "KB";
            }
            if (lengthFile > 1024 * 1024)
            {
                fileSize = Math.Round((double)lengthFile / 1024 / 1024, 2) + "MB";
            }
            return fileSize;
        }
        #region 内存回收  
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>  
        /// 释放内存  
        /// </summary>  
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Process[] process = System.Diagnostics.Process.GetProcessesByName("CefSharp.BrowserSubprocess");
                foreach (var list in process)
                {
                    SetProcessWorkingSetSize(list.Handle, -1, -1);
                }
                //SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
        private static string defaultHeaderImage = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
        /// <summary>
        /// 判断文件是否占用
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileInUsing(string path)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                if (fs != null)
                    fs.Close();
            }
            return inUse; //true表示正在使用,false没有使用  
        }
        /// <summary>
        /// 自动回复
        /// </summary>
        /// <param name="cef">cef控件</param>
        /// <param name="user">AntSdkContact_User</param>
        /// <param name="contents">内容</param>
        public static void AutoReplyMessage(ChromiumWebBrowser cef, AntSdkContact_User user, string contents, string lastMsgTime = "")
        {
            string pathImage = user.picture + "" == "" ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") : user.picture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");


            sbLeft.AppendLine("var img = document.createElement('img');");


            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            if (string.IsNullOrEmpty(lastMsgTime))
                sbLeft.AppendLine("timeshow.innerHTML ='" + DateTime.Now.ToString("HH:mm:ss") + "';");
            else
            {
                //var  msgTime= DataConverter.FormatTimeByTimeStamp(lastMsgTime);
                var msgTime = ConvertStringToDateTime(lastMsgTime);
                sbLeft.AppendLine("timeshow.innerHTML ='" + msgTime.ToString("HH:mm:ss") + "';");
            }
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            //string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            //sbLeft.AppendLine(divLeftCopyContent(divid));
            //sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");


            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/contents);


            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");
            var content = sbLeft.ToString();
            var task = cef.EvaluateScriptAsync(content);
            task.Wait();
            cef.EvaluateScriptAsync("setscross();");
        }

        /// <summary>
        /// 解析表情
        /// </summary>
        /// <param name="content">会话内容</param>
        /// <returns>解析后的消息</returns>
        public static string talkContentReplace(string content)
        {
            string localEmojiPath = (AppDomain.CurrentDomain.BaseDirectory + "Emoji\\").Replace(@"\", @"/").Replace(" ", "%20"); ;
            return content.Replace(" ", "&#160;").Replace("<", "&lt;").Replace("'", "&#39;").Replace(" ", "&nbsp;").Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("\r", "<br/>").Replace("[1f01]", "<img src=file:///" + localEmojiPath + "1f01.png></img>").Replace("[1f02]", "<img src=file:///" + localEmojiPath + "1f02.png></img>").Replace("[1f03]", "<img src=file:///" + localEmojiPath + "1f03.png></img>").Replace("[1f04]", "<img src=file:///" + localEmojiPath + "1f04.png></img>").Replace("[1f05]", "<img src=file:///" + localEmojiPath + "1f05.png></img>").Replace("[1f06]", "<img src=file:///" + localEmojiPath + "1f06.png></img>").Replace("[1f07]", "<img src=file:///" + localEmojiPath + "1f07.png></img>").Replace("[1f08]", "<img src=file:///" + localEmojiPath + "1f08.png></img>").Replace("[1f09]", "<img src=file:///" + localEmojiPath + "1f09.png></img>").Replace("[1f10]", "<img src=file:///" + localEmojiPath + "1f10.png></img>").Replace("[1f11]", "<img src=file:///" + localEmojiPath + "1f11.png></img>").Replace("[1f12]", "<img src=file:///" + localEmojiPath + "1f12.png></img>").Replace("[1f13]", "<img src=file:///" + localEmojiPath + "1f13.png></img>").Replace("[1f14]", "<img src=file:///" + localEmojiPath + "1f14.png></img>").Replace("[1f15]", "<img src=file:///" + localEmojiPath + "1f15.png></img>").Replace("[1f16]", "<img src=file:///" + localEmojiPath + "1f16.png></img>").Replace("[1f17]", "<img src=file:///" + localEmojiPath + "1f17.png></img>").Replace("[1f18]", "<img src=file:///" + localEmojiPath + "1f18.png></img>").Replace("[1f19]", "<img src=file:///" + localEmojiPath + "1f19.png></img>").Replace("[1f20]", "<img src=file:///" + localEmojiPath + "1f20.png></img>").Replace("[1f21]", "<img src=file:///" + localEmojiPath + "1f21.png></img>").Replace("[1f22]", "<img src=file:///" + localEmojiPath + "1f22.png></img>").Replace("[1f23]", "<img src=file:///" + localEmojiPath + "1f23.png></img>").Replace("[1f24]", "<img src=file:///" + localEmojiPath + "1f24.png></img>").Replace("[1f25]", "<img src=file:///" + localEmojiPath + "1f25.png></img>").Replace("[1f26]", "<img src=file:///" + localEmojiPath + "1f26.png></img>").Replace("[1f27]", "<img src=file:///" + localEmojiPath + "1f27.png></img>").Replace("[1f28]", "<img src=file:///" + localEmojiPath + "1f28.png></img>").Replace("[1f29]", "<img src=file:///" + localEmojiPath + "1f29.png></img>").Replace("[1f30]", "<img src=file:///" + localEmojiPath + "1f30.png></img>").Replace("[1f31]", "<img src=file:///" + localEmojiPath + "1f31.png></img>").Replace("[1f32]", "<img src=file:///" + localEmojiPath + "1f32.png></img>").Replace("[1f33]", "<img src=file:///" + localEmojiPath + "1f33.png></img>").Replace("[1f34]", "<img src=file:///" + localEmojiPath + "1f34.png></img>").Replace("[1f35]", "<img src=file:///" + localEmojiPath + "1f35.png></img>").Replace("[1f36]", "<img src=file:///" + localEmojiPath + "1f36.png></img>");
        }
        /// <summary>
        /// 单聊信息入库
        /// </summary>
        /// <param name="cmr"></param>
        public static bool addDataOneToOne(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            if (t_chat.ReceiveInsert(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool addSelfData(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            if (t_chat.InsertSelfData(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 群聊信息入库
        /// </summary>
        /// <param name="cmr"></param>
        public static bool addDataGroup(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
            if (t_chat.ReceiveInsert(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 群聊阅后即焚消息入库
        /// </summary>
        /// <param name="cmr"></param>
        public static bool addDataGroupBurn(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
            if (t_chat.ReceiveInsertBurn(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool addSelfDataGroup(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
            if (t_chat.InsertSelfData(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool addSelfBurnDataGroup(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase cmr)
        {
            T_Chat_Message_GroupBurnDAL t_chat = new T_Chat_Message_GroupBurnDAL();
            if (t_chat.InsertSelfData(cmr) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="strBase64"></param>
        /// <returns></returns>
        public static string ConvertToBase64(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            string baseStr = Convert.ToBase64String(bytes);
            return baseStr;
        }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="strBase64"></param>
        /// <returns></returns>
        public static string Base64ToString(string strBase64)
        {
            byte[] bytes = Convert.FromBase64String(strBase64);
            string outStr = Encoding.UTF8.GetString(bytes);
            return outStr;
        }
        /// <summary>
        /// 是否显示时间
        /// </summary>
        /// <param name="lastShowTime">上次提示时间</param>
        /// <param name="preTime">上次时间</param>
        /// <returns>返回时间</returns>
        public static bool showTimeSend(string lastShowTime, string preTime, DateTime dtNow)
        {
            bool result = false;
            //DataConverter.GetTimeByTimeStamp()
            if (string.IsNullOrEmpty(preTime.ToString()))
            {
                result = true;
                //return string.Format("{0:T}", DateTime.Now);
            }
            else
            {
                DateTime dt = Convert.ToDateTime(preTime);
                //DateTime dtNow = DateTime.Now;
                double timeInterval = (double)dtNow.Subtract(dt).TotalMinutes;
                if (timeInterval > 3)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 是否显示时间
        /// </summary>
        /// <param name="lastShowTime">上次提示时间</param>
        /// <param name="preTime">上次时间</param>
        /// <returns>返回时间</returns>
        public static bool showTimeReceive(DateTime preTime, DateTime dtNow)
        {
            bool result = false;
            //DataConverter.GetTimeByTimeStamp()
            if (string.IsNullOrEmpty(preTime.ToString()))
            {
                result = true;
                //return string.Format("{0:T}", DateTime.Now);
            }
            else
            {
                DateTime dt = Convert.ToDateTime(preTime);
                //DateTime dtNow = DateTime.Now;
                double timeInterval = (double)dt.Subtract(dtNow).TotalMinutes;
                if (timeInterval > 3)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 历史纪录是否显示时间
        /// </summary>
        /// <param name="lastShowTime">上次提示时间</param>
        /// <param name="preTime">上次时间</param>
        /// <returns>返回时间</returns>
        public static bool showHistoryTimeSend(DateTime lastShowTime, string preTime)
        {
            bool result = false;
            //DataConverter.GetTimeByTimeStamp()
            if (string.IsNullOrEmpty(preTime.ToString()))
            {
                result = true;
                //return string.Format("{0:T}", DateTime.Now);
            }
            else
            {
                DateTime dt = Convert.ToDateTime(preTime);
                //DateTime dtNow = DateTime.Now;
                double timeInterval = (double)lastShowTime.Subtract(dt).TotalMinutes;
                if (timeInterval > 3)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 插入时间
        /// </summary>
        /// <returns></returns>
        public static string showCenterTime()
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            sbCenter.AppendLine("firsts.innerHTML ='" + string.Format("{0:t}", DateTime.Now) + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 是否显示更多消息记录
        /// </summary>
        /// <param name="cef"></param>
        public static void IsShowMoreScrollMsg(ChromiumWebBrowser cef, string showMsg)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            sbCenter.AppendLine("firsts.innerHTML ='" + showMsg + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="divID"></param>
        public static void HiddenMsgDiv(ChromiumWebBrowser cef, string divID)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ document.getElementById('" + divID + "').style.display = 'none';}");
            sbCenter.AppendLine("centerDiv();");
            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 移除当前界面div
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="divID"></param>
        public static void RemoveMsgDiv(ChromiumWebBrowser cef, string divID)
        {
            string mstr = "removeDiv('" + divID + "');";
            cef.ExecuteScriptAsync(mstr);
        }
        /// <summary>
        /// 消息撤回
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="divID"></param>
        public static void ReCallMsgDiv(ChromiumWebBrowser cef, string divID)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                StringBuilder sbCenter = new StringBuilder();
                sbCenter.AppendLine("function centerDiv()");
                sbCenter.AppendLine("{ document.getElementById('" + divID + "').style.display = 'none';}");
                sbCenter.AppendLine("centerDiv();");
                Task task = cef.EvaluateScriptAsync(sbCenter.ToString());
                task.Wait();
            }));
        }
        public static void VisibleMsgDiv(ChromiumWebBrowser cef, string divID)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ document.getElementById('" + divID + "').style.visibility = 'visible';}");
            sbCenter.AppendLine("centerDiv();");
            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 阅后即焚提示
        /// </summary>
        /// <returns></returns>
        public static void showBurnTips(ChromiumWebBrowser cef)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var first=document.createElement('div');");
            sbCenter.AppendLine("first.className='speechShowTipsBurn';");
            sbCenter.AppendLine("var burndiv=document.createElement('div');");
            sbCenter.AppendLine("burndiv.className='burnDiv';");
            sbCenter.AppendLine("first.appendChild(burndiv);");

            sbCenter.AppendLine("var burnul=document.createElement('ul');");
            sbCenter.AppendLine("burnul.className='burnUl';");
            sbCenter.AppendLine("burndiv.appendChild(burnul);");

            sbCenter.AppendLine("var burnli1=document.createElement('li');");
            sbCenter.AppendLine("burnli1.className='burnLi';");
            sbCenter.AppendLine("burnli1.innerHTML='一键销毁所有消息';");

            sbCenter.AppendLine("burnul.appendChild(burnli1);");

            sbCenter.AppendLine("var burnli2=document.createElement('li');");
            sbCenter.AppendLine("burnli2.className='burnLi';");
            sbCenter.AppendLine("burnli2.innerHTML='消息彻底从服务器删除';");
            sbCenter.AppendLine("burnul.appendChild(burnli2);");

            sbCenter.AppendLine("var burnli3=document.createElement('li');");
            sbCenter.AppendLine("burnli3.className='burnLi';");
            sbCenter.AppendLine("burnli3.innerHTML='消息禁止复制';");
            sbCenter.AppendLine("burnul.appendChild(burnli3);");

            sbCenter.AppendLine("var burnli4=document.createElement('li');");
            sbCenter.AppendLine("burnli4.className='burnLi';");
            sbCenter.AppendLine("burnli4.innerHTML='该模式下所有成员匿名';");
            sbCenter.AppendLine("burnul.appendChild(burnli4);");

            sbCenter.AppendLine("document.body.appendChild(first);}");
            sbCenter.AppendLine("centerDiv();");
            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 截取userid
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SubString(string str)
        {
            string id = "";
            if (str.Length > 35)
            {
                id = str.Substring(32, str.Length - 32);
            }
            else
            {
                id = str;
            }
            return id;
        }
        /// <summary>
        /// 生成新userid
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string NewUserIdString(string userid)
        {
            return Guid.NewGuid().ToString().Replace("-", "") + userid;
        }
        /// <summary>
        /// 切换添加阅后即焚
        /// </summary>
        /// <param name="cef"></param>
        public static void addContentTips(ChromiumWebBrowser cef)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.id='history';");
            sbCenter.AppendLine("firsts.className='speechCenterHistory';");


            sbCenter.AppendLine("var image=document.createElement('img');");
            sbCenter.AppendLine("image.src='" + (AppDomain.CurrentDomain.BaseDirectory + "web_content/images/查看历史消息.png").Replace(@"\", @"/").Replace(" ", "%20") + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);");

            sbCenter.AppendLine("firsts.appendChild(image);");
            sbCenter.AppendLine("firsts.innerHTML='&nbsp;&nbsp;滚动鼠标滑轮查看历史消息';");


            sbCenter.AppendLine("var speendiv=document.createElement('div');");
            sbCenter.AppendLine("speendiv.className='speechShowTipsBurn';");

            sbCenter.AppendLine("var speentitle=document.createElement('div');");
            sbCenter.AppendLine("speentitle.style='font-size: 14px; color: #ffffff; padding: 15px;';");
            sbCenter.AppendLine("speentitle.innerHTML='无痕模式';");
            sbCenter.AppendLine("speendiv.appendChild(speentitle);");

            sbCenter.AppendLine("var speenul=document.createElement('ul');");
            sbCenter.AppendLine("speenul.style='margin-top: -11px; line-height: 18px; text-align: left;';");


            sbCenter.AppendLine("var speenli=document.createElement('li');");
            sbCenter.AppendLine("speenli.style='font-size: 12px; color: #ffffff';");
            sbCenter.AppendLine("speenli.innerHTML='和普通群聊独立分割';");
            sbCenter.AppendLine("speenul.appendChild(speenli);");

            sbCenter.AppendLine("var speenli1=document.createElement('li');");
            sbCenter.AppendLine("speenli1.style='font-size: 12px; color: #ffffff';");
            sbCenter.AppendLine("speenli1.innerHTML='管理员一键删除记录';");
            sbCenter.AppendLine("speenul.appendChild(speenli1);");

            sbCenter.AppendLine("var speenli2=document.createElement('li');");
            sbCenter.AppendLine("speenli2.style='font-size: 12px; color: #ffffff';");
            sbCenter.AppendLine("speenli2.innerHTML='头像姓名当前不可见';");
            sbCenter.AppendLine("speenul.appendChild(speenli2);");

            sbCenter.AppendLine("speendiv.appendChild(speenul);");

            sbCenter.AppendLine("document.body.appendChild(speendiv);");


            sbCenter.AppendLine("var speenenddiv=document.createElement('div');");
            sbCenter.AppendLine("speenenddiv.id='bodydiv';");

            sbCenter.AppendLine("document.body.appendChild(speenenddiv);}");
            sbCenter.AppendLine("centerDiv();");

            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 加法
        /// </summary>
        /// <returns></returns>
        public static string add()
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function addMethod()");
            sbCenter.AppendLine("{ var first=1;");
            sbCenter.AppendLine("var second=1;");
            sbCenter.AppendLine("var total=first+second;");
            sbCenter.AppendLine("return total;}");
            sbCenter.AppendLine("addMethod();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 插入时间
        /// </summary>
        /// <returns></returns>
        public static string showCenterTime(DateTime dt)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            sbCenter.AppendLine("firsts.innerHTML ='" + string.Format("{0:T}", dt) + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 更新界面div id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string updateDivId(string id, string updateid)
        {
            StringBuilder sbUpdate = new StringBuilder();
            sbUpdate.AppendLine("function updateDivId()");
            sbUpdate.AppendLine("{ var getdiv= document.getElementById('" + id + "');");
            sbUpdate.AppendLine("getdiv.id='" + updateid + "';");
            sbUpdate.AppendLine("}");
            sbUpdate.AppendLine("updateDivId();");
            return sbUpdate.ToString();
        }
        /// <summary>
        /// 更新界面sid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateid"></param>
        /// <returns></returns>
        public static string updateDivSid(string id, string updateid)
        {
            StringBuilder sbUpdate = new StringBuilder();
            sbUpdate.AppendLine("function updateImgSid()");
            sbUpdate.AppendLine("{ var getdiv= document.getElementById('" + id + "');");
            sbUpdate.AppendLine("getdiv.setAttribute('sid','" + updateid + "');");
            sbUpdate.AppendLine("}");
            sbUpdate.AppendLine("updateImgSid();");
            return sbUpdate.ToString();
        }
        /// <summary>
        /// 隐藏对应消息
        /// </summary>
        /// <param name="hideId"></param>
        /// <returns></returns>
        public static string hideDivById(string hideId)
        {
            StringBuilder sbUpdate = new StringBuilder();
            sbUpdate.AppendLine("function hideDivId()");
            sbUpdate.AppendLine("{ var getdiv= document.getElementById('" + hideId + "');");
            sbUpdate.AppendLine("getdiv.hidden='hidden';");
            sbUpdate.AppendLine("}");
            sbUpdate.AppendLine("hideDivId();");
            return sbUpdate.ToString();
        }
        /// <summary>
        /// 插入时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string newShowCenterTime(string dt)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            sbCenter.AppendLine("firsts.innerHTML ='" + dt + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        public static void swithStaticJpg(ChromiumWebBrowser cef, string imgId, string oneOrZero)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function switchSrc()");

            sbCenter.AppendLine("{");
            if (oneOrZero == "1")
            {
                sbCenter.AppendLine(" document.getElementById('" + imgId + "').src='../Images/htmlImages/语音right.png';");
            }
            else
            {
                sbCenter.AppendLine(" document.getElementById('" + imgId + "').src='../Images/htmlImages/语音left.png';");
            }
            sbCenter.AppendLine("}");
            sbCenter.AppendLine("switchSrc();");
            cef.ExecuteScriptAsync(sbCenter.ToString());
        }
        /// <summary>
        /// 插入撤回消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string InsertUIRecallMsg(string msg)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenterTips';");
            sbCenter.AppendLine("firsts.innerHTML ='" + msg + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 滚动撤销消息显示
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string ScrollUIRecallMsg(string msg)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenterTips';");
            sbCenter.AppendLine("firsts.innerHTML ='" + msg + "';");
            //sbCenter.AppendLine("document.body.appendChild(firsts);}");
            //获取body层
            sbCenter.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbCenter.AppendLine("listbody.insertBefore(firsts,listbody.childNodes[0]);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 历史记录插入时间
        /// </summary>
        /// <returns></returns>
        public static string showHistoryCenterTime(DateTime dt)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            //sbCenter.AppendLine("firsts.innerHTML ='" + string.Format("{0:T}", dt) + "';");
            sbCenter.AppendLine("firsts.innerHTML ='" + string.Format("{0}", dt.ToString("yyyy-MM-dd HH:mm:ss")) + "';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 插入大数据构造
        /// </summary>
        /// <param name="tableName">表名字</param>
        /// <param name="list">插入数据集合</param>
        /// <returns></returns>
        public static string insertSqliteBigData(string tableName, List<SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase> list)
        {
            StringBuilder stStr = new StringBuilder();
            foreach (var l in list)
            {
                stStr.Append("(" + /*TODO:AntSdk_Modify:l.MTP*/(int)l.MsgType + "," + l.chatIndex + "," + /*//TODO:AntSdk_Modify:l.content*/ l.sourceContent + "," + l.messageId + "," + l.sendTime + "," + l.sendUserId + "," + l.sessionId + "," + l.targetId + "," + l.SENDORRECEIVE + "," + l.sendsucessorfail + "),");
            }
            string preStr = "insert into " + tableName + "(MTP,CHATINDEX,CONTENT,MESSAGEID,SENDTIME,SENDUSERID,SESSIONID,TARGETID,SENDORRECEIVE,SENDSUCESSORFAIL) values " + stStr.ToString().Substring(0, stStr.Length - 1);
            return preStr;
        }
        /// <summary>
        /// 时间戳加随机数 对messageId生成
        /// </summary>
        /// <returns></returns>
        public static string timeStampAndRandom()
        {
            int hashcode = Guid.NewGuid().GetHashCode();
            Random random = new Random(hashcode);
            int rd = random.Next(10000000, 99999999);
            //Console.WriteLine("rd:------------------------------------"+rd.ToString());
            long ts = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            return "M" + ts.ToString() + (int)GlobalVariable.OSType.PC + rd.ToString();
        }
        /// <summary>
        /// 判断RichTextBox 内容长度
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static bool textLength(System.Windows.Controls.RichTextBox rt)
        {
            TextRange textRange = new TextRange(rt.Document.ContentStart, rt.Document.ContentEnd);
            if (textRange.Text.Length > 1000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int textLengths(System.Windows.Controls.RichTextBox rt)
        {
            TextRange textRange = new TextRange(rt.Document.ContentStart, rt.Document.ContentEnd);
            if (textRange.Text.Trim().Length == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        /// <summary>
        /// 消息过长提示
        /// </summary>
        /// <returns></returns>
        public static string showTips()
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenter';");
            sbCenter.AppendLine("firsts.innerHTML ='发送消息内容超长，请分条发送。';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 发送过快提示
        /// </summary>
        /// <returns></returns>
        public static string quickly()
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenterq';");
            sbCenter.AppendLine("firsts.innerHTML ='客官，你发消息太快了，悠着点奥！^_^';");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 滚动到相应位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string scrollPosition(string position)
        {
            StringBuilder sbPosition = new StringBuilder();
            sbPosition.AppendLine("function scrollDiv()");
            sbPosition.AppendLine("{ var firsts=document.getElementById(" + position + ").scrollIntoView();}");
            sbPosition.AppendLine("scrollDiv();");
            return sbPosition.ToString();
        }
        /// <summary>
        /// Url解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string strUrlDecode(string str)
        {
            return System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// 清理页面内容
        /// </summary>
        /// <returns></returns>
        public static string clearHtml()
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function clearHtmls()");
            sbCenter.AppendLine("{ document.body.innerHTML='';}");
            sbCenter.AppendLine("clearHtmls();");
            return sbCenter.ToString();
        }
        /// <summary>
        /// 获取文件Md5值
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getFileMd5Value(string path)
        {
            FileStream file = null;
            byte[] getMd5 = null;
            string md5Value = "";
            try
            {
                file = new FileStream(path, FileMode.OpenOrCreate);
                MD5 fileMd5 = new MD5CryptoServiceProvider();

                getMd5 = fileMd5.ComputeHash(file);
                file.Flush();
                file.Close();
                file.Dispose();
                StringBuilder sb = new StringBuilder();
                foreach (var list in getMd5)
                {
                    sb.Append(list.ToString("x2"));
                }
                md5Value = sb.ToString();
                fileMd5.Clear();
                fileMd5.Dispose();

            }
            catch (Exception ex)
            {
                file.Close();
                file.Dispose();
                LogHelper.WriteError("PublicTalkMothed_getFileMd5Value:" + ex.Message + ex.StackTrace + ex.TargetSite);
            }
            return md5Value;
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFile(string path)
        {
            try
            {
                string first = path.Replace(@"/", "\\");
                //string fileToSelect = first;

                //string args = string.Format("/Select, {0}", fileToSelect);
                //ProcessStartInfo pfi = new ProcessStartInfo("Explorer.exe", args);
                System.Diagnostics.Process.Start(first);
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("[TalkViewModel_GetFilePath]:" + ex.Message + ex.Source + ex.StackTrace);
            }
        }
        /// <summary>
        /// 中间插入
        /// </summary>
        /// <returns></returns>
        public static void showCenterMsg(ChromiumWebBrowser cef, string msg)
        {
            StringBuilder sbCenter = new StringBuilder();
            sbCenter.AppendLine("function centerDiv()");
            sbCenter.AppendLine("{ var firsts=document.createElement('div');");
            sbCenter.AppendLine("firsts.className='speechCenterTips';");
            sbCenter.AppendLine("var image=document.createElement('img');");
            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/录音-设备提示.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbCenter.AppendLine("image.src='" + musicImg + "';");
            sbCenter.AppendLine("image.style.height = '12px';");
            sbCenter.AppendLine("image.style.width = '12px';");
            sbCenter.AppendLine("firsts.appendChild(image);");
            sbCenter.AppendLine("var node = document.createElement('span');");
            sbCenter.AppendLine("node.innerHTML ='&nbsp;&nbsp;" + msg + "';");
            sbCenter.AppendLine("firsts.appendChild(node);");
            sbCenter.AppendLine("document.body.appendChild(firsts);}");
            sbCenter.AppendLine("centerDiv();");
            cef.EvaluateScriptAsync(sbCenter.ToString());
            #region 滚动条置底
            StringBuilder sbEnd = new StringBuilder();
            sbEnd.AppendLine("setscross();");
            var taskEnd = cef.EvaluateScriptAsync(sbEnd.ToString());
            #endregion
        }
        /// <summary>
        /// 滚动条置地
        /// </summary>
        /// <param name="cef"></param>
        //public static void scrollEnd(ChromiumWebBrowser cef)
        //{
        //    App.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        var task = cef.EvaluateScriptAsync("setscross();");
        //        if (!task.Result.Success)
        //        {
        //            LogHelper.WriteWarn("[PublicTalkMothed]-scrollEnd:1");
        //            var task2 = cef.EvaluateScriptAsync("setscross();");
        //            if (!task2.Result.Success)
        //            {
        //                LogHelper.WriteWarn("[PublicTalkMothed]-scrollEnd:2");
        //                var task3 = cef.EvaluateScriptAsync("setscross();");
        //                if (!task3.Result.Success)
        //                {
        //                    LogHelper.WriteWarn("[PublicTalkMothed]-scrollEnd:3");
        //                }
        //            }
        //        }
        //    }));
        //}
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name=”timeStamp”></param>
        /// <returns></returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }
        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name=”time”></param>
        /// <returns></returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time)
        {
            long ts = ConvertDateTimeToIntLong(time);
            return ts.ToString();
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToIntLong(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        /// <summary>        
        /// 时间戳转为C#格式时间        
        /// </summary>        
        /// <param name=”timeStamp”></param>        
        /// <returns></returns>        
        public static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        /// <summary>
        /// 时间显示格式
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string timeComparison(string timeStamp)
        {
            string showStr = "";
            //时间戳时间
            DateTime dtHistory = ConvertStringToDateTime(timeStamp);
            DateTime nowTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            double differ = nowTime.Subtract(dtHistory).TotalDays;
            if (differ > 0 && differ < 1)
            {
                showStr = dtHistory.ToString("HH:mm:ss");
            }
            else if (differ >= 1 && differ <= 2)
            {
                showStr = "昨天 " + dtHistory.ToString("HH:mm:ss");
            }
            else
            {
                showStr = dtHistory.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return showStr;
        }
        static int i = 0;
        /// <summary>
        /// 测试追加
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>  
        public static void appendPre(ChromiumWebBrowser cef, string msg)
        {
            StringBuilder sbAppend = new StringBuilder();
            sbAppend.AppendLine("function apPre()");
            sbAppend.AppendLine("{ var firsts=document.createElement('div');");
            sbAppend.AppendLine("firsts.className='speechCenter';");
            i++;
            sbAppend.AppendLine("firsts.innerHTML ='" + i.ToString() + "';");
            sbAppend.AppendLine("var list = document.getElementById('bodydiv');");
            sbAppend.AppendLine("list.insertBefore(firsts,list.childNodes[0]);}");

            sbAppend.AppendLine("apPre();");
            cef.EvaluateScriptAsync(sbAppend.ToString());
        }
        /// <summary>
        /// 设置DIV id以及设置倒计时时间
        /// </summary>
        /// <param name="wordCounts">内容大小</param>
        /// <returns></returns>
        public static DivIdGather getDivID(string chatIndex)
        {
            DivIdGather dg = new DivIdGather();
            string guid = Guid.NewGuid().ToString().Replace("-", "") + chatIndex;
            dg.guid = guid;
            dg.hideContentId = "hideContent" + guid;
            dg.imageIsShowId = "image" + guid;
            dg.countDownShowId = "span" + guid;
            dg.hideDivId = "outdiv" + guid;
            dg.countDownTimes = "20";
            return dg;
        }
        public static DivIdGather getDivVoiceID(string chatIndex, int second)
        {
            DivIdGather dg = new DivIdGather();
            string guid = Guid.NewGuid().ToString().Replace("-", "") + chatIndex;
            dg.guid = guid;
            dg.hideContentId = "hideContent" + guid;
            dg.imageIsShowId = "image" + guid;
            dg.countDownShowId = "span" + guid;
            dg.hideDivId = "outdiv" + guid;
            dg.countDownTimes = (4 + second * 2).ToString();
            return dg;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int getSeconds(selectType type, int length)
        {
            int second = 0;
            switch (type)
            {
                //文本消息
                case selectType.text:
                    if (length > 0 && length <= 10)
                    {
                        second = length + 10;
                    }
                    else
                    {
                        second = ((length / 10) * 5) + 10 + length;
                    }
                    break;
                //图片消息
                case selectType.image:
                    second = 20;
                    break;
                //语音消息
                case selectType.voice:
                    second = 4 + length * 2;
                    break;
                //文件消息
                case selectType.file:
                    second = 20;
                    break;
            }
            return second;
        }

        /*阅后即焚左侧显示对应方法*/
        /// <summary>
        /// 阅后即焚左侧文字显示  Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void leftBurnAfterText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            #region 正常显示
            //生成新的层ID
            DivIdGather divId = getDivID(msg.chatIndex);

            string pathImage = user.copyPicture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");

            //头像显示
            sbLeft.AppendLine("var imgHead = document.createElement('img');");
            sbLeft.AppendLine("imgHead.src='" + pathImage + "';");
            sbLeft.AppendLine("imgHead.className='divcss5Left';");
            sbLeft.AppendLine("imgHead.id='" + user.userId + "';");
            sbLeft.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(imgHead);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //消息显示
            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");

            //单击显示外层
            sbLeft.AppendLine("var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" + divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" + divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId + "');outdiv.setAttribute('time','" + 15 + "');outdiv.setAttribute('hideMessageId','" + msg.messageId + "');");
            sbLeft.AppendLine("outdiv.id='" + divId.guid + "';");
            sbLeft.AppendLine("outdiv.addEventListener('click',clickshow);");
            sbLeft.AppendLine("node.appendChild(outdiv);");

            //内容显示层
            sbLeft.AppendLine("var contentDiv=document.createElement('div');");
            sbLeft.AppendLine("contentDiv.id='" + divId.hideContentId + "';");
            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            sbLeft.AppendLine("contentDiv.innerHTML ='" + showMsg + "';");
            sbLeft.AppendLine("contentDiv.className='contentHidden';");
            //sbLeft.AppendLine("contentDiv.style.visibility='hidden;';");
            sbLeft.AppendLine("outdiv.appendChild(contentDiv);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");


            //阅后即焚和倒计时层
            sbLeft.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbLeft.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //阅后即焚显示图片层
            sbLeft.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbLeft.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbLeft.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


            //阅后即焚倒计时层
            sbLeft.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbLeft.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbLeft.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbLeft.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbLeft.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.text, /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent.Length) + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
            #endregion

        }
        /// <summary>
        /// 阅后即焚左侧图片显示  image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftBurnAfterImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            //生成新的层ID
            DivIdGather divId = getDivID(msg.chatIndex);

            string pathImage = user.copyPicture;
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            //sbLeftImage.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");

            //头像显示
            sbLeftImage.AppendLine("var imgHead = document.createElement('img');");
            sbLeftImage.AppendLine("imgHead.src='" + pathImage + "';");
            sbLeftImage.AppendLine("imgHead.className='divcss5Left';");
            sbLeftImage.AppendLine("imgHead.id='" + user.userId + "';");
            sbLeftImage.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(imgHead);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");

            //图片消息层
            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //图片最外层
            sbLeftImage.AppendLine("var outImageDiv=document.createElement('div');");
            sbLeftImage.AppendLine("node.appendChild(outImageDiv);");


            //单击显示外层
            sbLeftImage.AppendLine("var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" + divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" + divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId + "');outdiv.setAttribute('time','" + 20 + "');outdiv.setAttribute('hideMessageId','" + msg.messageId + "');");
            sbLeftImage.AppendLine("outdiv.id='" + divId.guid + "';");
            sbLeftImage.AppendLine("outdiv.addEventListener('click',clickshow);");
            sbLeftImage.AppendLine("outImageDiv.appendChild(outdiv);");

            //图片消息显示层
            sbLeftImage.AppendLine("var img = document.createElement('img');");
            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.style.visibility='hidden';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='" + divId.hideContentId + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            //sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbLeftImage.AppendLine("img.title='双击查看原图';");
            sbLeftImage.AppendLine("img.values='1';");

            sbLeftImage.AppendLine("outdiv.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");



            //阅后即焚和倒计时层
            sbLeftImage.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbLeftImage.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //阅后即焚显示图片层
            sbLeftImage.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbLeftImage.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbLeftImage.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeftImage.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbLeftImage.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


            //阅后即焚倒计时层
            sbLeftImage.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbLeftImage.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbLeftImage.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbLeftImage.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbLeftImage.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.image, 20) + "';");
            sbLeftImage.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            if (msg.IsSetImgLoadComplete)
            {
                sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            }

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeftImage.ToString());
            task.Wait();
        }
        /// <summary>
        /// 阅后即焚左侧文件显示 温馨提示 文件不需要遮住 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftBurnAfterFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            //生成新的层ID
            DivIdGather divId = getDivID(msg.chatIndex);
            //头像图片
            string pathImage = user.copyPicture;
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.flag = "1";
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            //最外层DIV
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            //头像显示
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + pathImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + user.userId + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            string fileOpenguid = divId.guid;
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
            string fileDirectoryId = "dct" + divId.guid;
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //阅后即焚和倒计时层
            sbFileUpload.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbFileUpload.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbFileUpload.AppendLine("first.appendChild(outAfterBurnDiv);");
            //阅后即焚显示图片层
            sbFileUpload.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbFileUpload.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbFileUpload.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");
            //阅后即焚倒计时层
            sbFileUpload.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbFileUpload.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbFileUpload.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbFileUpload.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbFileUpload.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.file, 20) + "';");
            sbFileUpload.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");
            sbFileUpload.AppendLine("first.appendChild(outAfterBurnDiv);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbFileUpload.ToString());
            task.Wait();
        }
        /// <summary>
        /// 阅后即焚左侧语音显示  Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void LeftBurnAfterVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            //生成新的层ID
            DivIdGather divId = getDivVoiceID(msg.chatIndex, receive.duration);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');nodeFirst.setAttribute('divGuid','" + divId.guid + "');");
            // sbLeft.AppendLine("nodeFirst.addEventListener('click',clickDivVoiceCall);");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");

            //头像显示
            sbLeft.AppendLine("var imgHead = document.createElement('img');");
            sbLeft.AppendLine("imgHead.src='" + pathImage + "';");
            sbLeft.AppendLine("imgHead.className='divcss5Left';");
            sbLeft.AppendLine("imgHead.id='" + user.userId + "';");
            sbLeft.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(imgHead);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');node.setAttribute('divGuid','" + divId.guid + "');node.setAttribute('divChatIndex','" + msg.chatIndex + "');node.setAttribute('isBurn','0');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");

            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //阅后即焚和倒计时层
            sbLeft.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbLeft.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //阅后即焚显示图片层
            sbLeft.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbLeft.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbLeft.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");

            //阅后即焚倒计时层
            sbLeft.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbLeft.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbLeft.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbLeft.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbLeft.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.voice, /*TODO:AntSdk_Modify:msg.content*/receive.duration) + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //未读圆形提示
            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var divCircle = document.createElement('div');");
            sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
            sbLeft.AppendLine("divCircle.className ='leftVoiceBurn';");
            sbLeft.AppendLine("var imgCircle = document.createElement('img');");
            sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
            sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
            sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }

        /*阅后即焚右侧显示对应方法*/
        /// <summary>
        /// 阅后即焚右侧文字显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void rightBurnAfterText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            //头像显示层
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //内容显示层
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            sbRight.AppendLine("three.innerHTML ='" + showMsg + "';");
            sbRight.AppendLine("first.appendChild(three);");

            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
        }
        /// <summary>
        /// 阅后即焚右侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightBurnAfterImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            //头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");

            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");


            //图片显示层
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.chatIndex + "');");
            sbRight.AppendLine("img1.setAttribute('mid', 'M" + msg.messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");


            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
        }
        /// <summary>
        /// 阅后即焚右侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightBurnAfterFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
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
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
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
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            var setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //阅后即焚图片显示外层
            sbFileUpload.AppendLine("var imageOutDiv=document.createElement('div');");
            sbFileUpload.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
            //阅后即焚图片显示内层
            sbFileUpload.AppendLine("var imageInDiv=document.createElement('img');");
            sbFileUpload.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbFileUpload.AppendLine("first.appendChild(imageOutDiv);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbFileUpload.ToString());
            task.Wait();
            #endregion
        }
        /// <summary>
        /// 阅后即焚右侧语音显示 Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightBurnAfterVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbRight.AppendLine("first.setAttribute('onmousedown','VoiceRightMenuMethod(event)');first.setAttribute('selfmethods','one');first.setAttribute('audioUrl','" + receive.audioUrl + "');first.setAttribute('isRead','0');first.setAttribute('isShowImg','" + isShowReadImgId + "');first.setAttribute('isShowGif','" + isShowGifId + "');first.setAttribute('isLeftOrRight','1');");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");



            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");


            sbRight.AppendLine("first.appendChild(three);");

            ////未读圆形提示
            //string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接受成功.png").Replace(@"\", @"/").Replace(" ", "%20");
            //sbRight.AppendLine("var divCircle = document.createElement('div');");
            //sbRight.AppendLine("divCircle.className ='rightVoice';");
            //sbRight.AppendLine("var imgCircle = document.createElement('img');");
            //sbRight.AppendLine("imgCircle.src='" + CirCleImg + "';");
            //sbRight.AppendLine("divCircle.appendChild(imgCircle);");
            //sbRight.AppendLine("first.appendChild(divCircle);");

            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");

            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
            #endregion
        }

        /*阅后即焚左侧滚轮滚动显示对应方法*/
        /// <summary>
        /// 阅后即焚左侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftAfterBurnScrollText(ChromiumWebBrowsers cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user, ref bool deleteSuccess)
        {
            //比对readtime 如果为空执行下面显示 如果有值则现在时间与已读时间多比对  
            if (msg.readtime == "")
            {
                #region 正常显示
                //生成新的层ID
                DivIdGather divId = getDivID(msg.chatIndex);

                string pathImage = user.copyPicture;
                StringBuilder sbLeft = new StringBuilder();
                sbLeft.AppendLine("function myFunction()");
                sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                sbLeft.AppendLine("nodeFirst.className='leftd';");
                //sbLeft.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
                sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                sbLeft.AppendLine("var second=document.createElement('div');");
                sbLeft.AppendLine("second.className='leftimg';");

                //头像显示
                sbLeft.AppendLine("var imgHead = document.createElement('img');");
                sbLeft.AppendLine("imgHead.src='" + pathImage + "';");
                sbLeft.AppendLine("imgHead.className='divcss5Left';");
                sbLeft.AppendLine("imgHead.id='" + user.userId + "';");
                sbLeft.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
                sbLeft.AppendLine("second.appendChild(imgHead);");
                sbLeft.AppendLine("nodeFirst.appendChild(second);");

                //时间显示
                sbLeft.AppendLine("var timeshow = document.createElement('div');");
                sbLeft.AppendLine("timeshow.className='leftTimeText';");
                sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

                //消息显示
                sbLeft.AppendLine("var node=document.createElement('div');");
                sbLeft.AppendLine("node.className='speech left';");

                //单击显示外层
                sbLeft.AppendLine("var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" + divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" + divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId + "');outdiv.setAttribute('time','" + 15 + "');outdiv.setAttribute('hideMessageId','" + msg.messageId + "');");
                sbLeft.AppendLine("outdiv.id='" + divId.guid + "';");
                sbLeft.AppendLine("outdiv.addEventListener('click',clickshow);");
                sbLeft.AppendLine("node.appendChild(outdiv);");

                //内容显示层
                sbLeft.AppendLine("var contentDiv=document.createElement('div');");
                sbLeft.AppendLine("contentDiv.id='" + divId.hideContentId + "';");
                string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                sbLeft.AppendLine("contentDiv.innerHTML ='" + showMsg + "';");
                sbLeft.AppendLine("contentDiv.className='contentHidden';");
                //sbLeft.AppendLine("contentDiv.style.visibility='hidden;';");
                sbLeft.AppendLine("outdiv.appendChild(contentDiv);");

                sbLeft.AppendLine("nodeFirst.appendChild(node);");


                //阅后即焚和倒计时层
                sbLeft.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                sbLeft.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                //阅后即焚显示图片层
                sbLeft.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                sbLeft.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                sbLeft.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
                sbLeft.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


                //阅后即焚倒计时层
                sbLeft.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                sbLeft.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                sbLeft.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                sbLeft.AppendLine("outAfterTimeSpan.hidden='hidden';");
                sbLeft.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.text, showMsg.Length) + "';");
                sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");


                //获取body层
                sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
                sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

                //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                sbLeft.AppendLine("myFunction();");

                cef.EvaluateScriptAsync(sbLeft.ToString());
                #endregion
            }
            else
            {
                //现在时间
                DateTime nowTime = DateTime.Now;
                //点击时刻时间戳
                DateTime clickTime = GetTime(msg.readtime);
                //阅读总秒数
                int seconds = getSeconds(selectType.text, (PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent)).Length);
                //点击时间加阅读总秒数
                DateTime clickTimeAdd = clickTime.AddSeconds(seconds);
                TimeSpan ts = clickTimeAdd.Subtract(nowTime);
                TimeSpan tsLeftTime = nowTime.Subtract(clickTime);
                int leftTime = Convert.ToInt32(seconds - tsLeftTime.TotalSeconds);
                if (ts.TotalSeconds < 0 || ts.TotalSeconds == 0)
                {
                    T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                    t_chat.DeleteByMessageId(GlobalVariable.CompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, msg.messageId);
                    deleteSuccess = false;
                    //Console.WriteLine("ts.Seconds小于0:" + ts.Seconds);
                }
                else
                {
                    #region 正常显示
                    //生成新的层ID
                    DivIdGather divId = getDivID(msg.chatIndex);

                    string pathImage = user.copyPicture;
                    StringBuilder sbLeft = new StringBuilder();
                    sbLeft.AppendLine("function myFunction()");
                    sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
                    sbLeft.AppendLine("nodeFirst.className='leftd';");
                    //sbLeft.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
                    sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                    sbLeft.AppendLine("var second=document.createElement('div');");
                    sbLeft.AppendLine("second.className='leftimg';");

                    //头像显示
                    sbLeft.AppendLine("var imgHead = document.createElement('img');");
                    sbLeft.AppendLine("imgHead.src='" + pathImage + "';");
                    sbLeft.AppendLine("imgHead.className='divcss5Left';");
                    sbLeft.AppendLine("imgHead.id='" + user.userId + "';");
                    sbLeft.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
                    sbLeft.AppendLine("second.appendChild(imgHead);");
                    sbLeft.AppendLine("nodeFirst.appendChild(second);");

                    //时间显示
                    sbLeft.AppendLine("var timeshow = document.createElement('div');");
                    sbLeft.AppendLine("timeshow.className='leftTimeText';");
                    sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                    sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

                    //消息显示
                    sbLeft.AppendLine("var node=document.createElement('div');");
                    sbLeft.AppendLine("node.className='speech left';");

                    //单击显示外层
                    sbLeft.AppendLine("var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" + divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" + divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId + "');outdiv.setAttribute('time','" + 15 + "');outdiv.setAttribute('hideMessageId','" + msg.messageId + "');");
                    sbLeft.AppendLine("outdiv.id='" + divId.guid + "';");
                    //sbLeft.AppendLine("outdiv.addEventListener('click',clickshow);");
                    sbLeft.AppendLine("node.appendChild(outdiv);");

                    //内容显示层
                    sbLeft.AppendLine("var contentDiv=document.createElement('div');");
                    sbLeft.AppendLine("contentDiv.id='" + divId.hideContentId + "';");
                    string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                    sbLeft.AppendLine("contentDiv.innerHTML ='" + showMsg + "';");
                    sbLeft.AppendLine("contentDiv.className='contentHidden';");
                    //sbLeft.AppendLine("contentDiv.style.visibility='hidden;';");
                    sbLeft.AppendLine("outdiv.appendChild(contentDiv);");

                    sbLeft.AppendLine("nodeFirst.appendChild(node);");


                    //阅后即焚和倒计时层
                    sbLeft.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                    sbLeft.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                    sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                    //阅后即焚显示图片层
                    sbLeft.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                    sbLeft.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                    sbLeft.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                    string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
                    sbLeft.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                    sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


                    //阅后即焚倒计时层
                    sbLeft.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                    sbLeft.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                    sbLeft.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                    sbLeft.AppendLine("outAfterTimeSpan.hidden='hidden';");
                    sbLeft.AppendLine("outAfterTimeSpan.innerHTML='" + Convert.ToInt32(leftTime) + "';");
                    sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                    sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");


                    //获取body层
                    sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
                    sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

                    //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

                    sbLeft.AppendLine("myFunction();");

                    cef.EvaluateScriptAsync(sbLeft.ToString());
                    cef.ExecuteScriptAsync("clickshows('" + divId.guid + "','" + msg.messageId + "');");
                    #endregion
                    //Console.WriteLine("ts.Seconds大于0:" + ts.Seconds);
                }
            }
        }
        /// <summary>
        /// 阅后即焚左侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftAfterBurnScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            //生成新的层ID
            DivIdGather divId = getDivID(msg.chatIndex);

            string pathImage = user.copyPicture;
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            //sbLeftImage.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");

            //头像显示
            sbLeftImage.AppendLine("var imgHead = document.createElement('img');");
            sbLeftImage.AppendLine("imgHead.src='" + pathImage + "';");
            sbLeftImage.AppendLine("imgHead.className='divcss5Left';");
            sbLeftImage.AppendLine("imgHead.id='" + user.userId + "';");
            sbLeftImage.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(imgHead);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");

            //图片消息层
            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //图片最外层
            sbLeftImage.AppendLine("var outImageDiv=document.createElement('div');");
            sbLeftImage.AppendLine("node.appendChild(outImageDiv);");


            //单击显示外层
            sbLeftImage.AppendLine("var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" + divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" + divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId + "');outdiv.setAttribute('time','" + 20 + "');outdiv.setAttribute('hideMessageId','" + msg.messageId + "');");
            sbLeftImage.AppendLine("outdiv.id='" + divId.guid + "';");
            sbLeftImage.AppendLine("outdiv.addEventListener('click',clickshow);");
            sbLeftImage.AppendLine("outImageDiv.appendChild(outdiv);");

            //图片消息显示层
            sbLeftImage.AppendLine("var img = document.createElement('img');");
            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.style.visibility='hidden';");

            sbLeftImage.AppendLine("img.src='" + "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") + "';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='" + divId.hideContentId + "';");
            //sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("outdiv.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");



            //阅后即焚和倒计时层
            sbLeftImage.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbLeftImage.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //阅后即焚显示图片层
            sbLeftImage.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbLeftImage.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbLeftImage.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeftImage.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbLeftImage.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


            //阅后即焚倒计时层
            sbLeftImage.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbLeftImage.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbLeftImage.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbLeftImage.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbLeftImage.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.image, 20) + "';");
            sbLeftImage.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //获取body层
            sbLeftImage.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeftImage.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);");

            //sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());
        }
        /// <summary>
        /// 阅后即焚左侧文件显示 温馨提示 文件不需要遮住 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftAterBurnScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            //生成新的层ID
            DivIdGather divId = getDivID(msg.chatIndex);
            //头像图片
            string pathImage = user.copyPicture;
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.seId = msg.sessionId;
            receive.flag = "1";
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            //最外层DIV
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            //头像显示
            sbFileUpload.AppendLine("var imgHead = document.createElement('img');");
            sbFileUpload.AppendLine("imgHead.src='" + pathImage + "';");
            sbFileUpload.AppendLine("imgHead.className='divcss5Left';");
            sbFileUpload.AppendLine("imgHead.id='" + user.userId + "';");
            sbFileUpload.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(imgHead);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            string fileOpenguid = divId.guid;
            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;
            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
            string fileDirectoryId = "dct" + divId.guid;
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //阅后即焚和倒计时层
            sbFileUpload.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbFileUpload.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbFileUpload.AppendLine("first.appendChild(outAfterBurnDiv);");
            //阅后即焚显示图片层
            sbFileUpload.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbFileUpload.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbFileUpload.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");
            //阅后即焚倒计时层
            sbFileUpload.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbFileUpload.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbFileUpload.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbFileUpload.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbFileUpload.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.file, 20) + "';");
            sbFileUpload.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");
            sbFileUpload.AppendLine("first.appendChild(outAfterBurnDiv);");
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
        }
        /// <summary>
        /// 阅后即焚左侧语音显示 Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void LeftAfterBurnShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            //生成新的层ID
            DivIdGather divId = getDivVoiceID(msg.chatIndex, receive.duration);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');nodeFirst.setAttribute('divGuid','" + divId.guid + "');");
            // sbLeft.AppendLine("nodeFirst.addEventListener('click',clickDivVoiceCall);");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            //头像
            sbLeft.AppendLine("var imgHead = document.createElement('img');");
            sbLeft.AppendLine("imgHead.src='" + pathImage + "';");
            sbLeft.AppendLine("imgHead.className='divcss5Left';");
            sbLeft.AppendLine("imgHead.id='" + user.userId + "';");
            sbLeft.AppendLine("imgHead.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(imgHead);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');node.setAttribute('divGuid','" + divId.guid + "');node.setAttribute('divChatIndex','" + msg.chatIndex + "');node.setAttribute('isBurn','0');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");

            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //阅后即焚和倒计时层
            sbLeft.AppendLine("var outAfterBurnDiv=document.createElement('div');");
            sbLeft.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            //阅后即焚显示图片层
            sbLeft.AppendLine("var outAfterIsShowImage=document.createElement('img');");
            sbLeft.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
            sbLeft.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");

            //阅后即焚倒计时层
            sbLeft.AppendLine("var outAfterTimeSpan=document.createElement('span');");
            sbLeft.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
            sbLeft.AppendLine("outAfterTimeSpan.className='countTimeValues';");
            sbLeft.AppendLine("outAfterTimeSpan.hidden='hidden';");
            sbLeft.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.voice, /*TODO:AntSdk_Modify:msg.content*/receive.duration) + "';");
            sbLeft.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

            sbLeft.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

            if (msg.voiceread + "" != "1")
            {
                //未读圆形提示
                string CirCleImg = "file:///" +
                                   (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(
                                       @"\", @"/").Replace(" ", "%20");
                sbLeft.AppendLine("var divCircle = document.createElement('div');");
                sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                sbLeft.AppendLine("divCircle.className ='leftVoiceBurn';");
                sbLeft.AppendLine("var imgCircle = document.createElement('img');");
                sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
                sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
                sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");
            }
            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbLeft.ToString());
        }

        /*阅后即焚右侧滚轮显示对应方法*/
        /// <summary>
        /// 阅后即焚右侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightAfterBurnScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            //头像显示层
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //内容显示层
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            sbRight.AppendLine("three.innerHTML ='" + showMsg + "';");
            sbRight.AppendLine("first.appendChild(three);");

            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                sbRight.AppendLine(OnceSendHistoryMsgDiv("sendBurnText", msg.messageId, showMsg, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
            }

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");

            //sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 阅后即焚右侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightAfterBurnScollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            //头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");

            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");


            //图片显示层
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");


            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                string imgLocalPath = "file:///" + pathHtml;
                sbRight.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
            }


            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");

            //sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            //sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 阅后即焚右侧文件显示 File
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightAfterBurnScollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            var txtShowSucessImg = "success";//接收上传状态图片
            string txtShowDown = "上传成功";//接收上传状态文字
            string txtOpenOrReceive = "打开";//打开接收文字
            string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
            //同一端发送
            if (isOpenOrReceive)
            {
                if (msg.sendsucessorfail == 1)//发送成功
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "上传成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//发送失败
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "fail";
                    txtShowDown = "上传失败";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
            }
            //不同端同步的消息
            else
            {
                if (isDownFile)//已经下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "下载成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//未下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "reveiving";
                    txtShowDown = "未下载";
                    txtOpenOrReceive = "接收";
                    txtSaveAs = "另存为";
                }
            }
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
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") +
                                                "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //阅后即焚图片显示外层
            sbFileUpload.AppendLine("var imageOutDiv=document.createElement('div');");
            sbFileUpload.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
            //阅后即焚图片显示内层
            sbFileUpload.AppendLine("var imageInDiv=document.createElement('img');");
            sbFileUpload.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbFileUpload.AppendLine("first.appendChild(imageOutDiv);");
            //发送失败提示
            if (msg.sendsucessorfail == 0)
            {
                sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath,
                    Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
            }
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 阅后即焚右侧文件下载后显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name=""></param>
        public static void rightAfterBurnScollFileDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");

            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");


            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");

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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");

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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            //string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
            //receive.progressId = progressId;
            //sbFileUpload.AppendLine("sevenSecond.id='" + progressId + "';");
            sbFileUpload.AppendLine("sevenSecond.style.width='" + (msg.sendsucessorfail > 0 ? "100%" : "0%") + "';");

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
            sbFileUpload.AppendLine("showSOFImg.src='" + (msg.sendsucessorfail > 0 ? fileShowImage.showImageHtmlPath("success", "") : fileShowImage.showImageHtmlPath("fail", "")) + "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");

            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");

            sbFileUpload.AppendLine("eight.appendChild(night);");

            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + (msg.sendsucessorfail > 0 ? "上传成功" : "上传失败") + "';");

            sbFileUpload.AppendLine("night.appendChild(nightButton);");



            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");

            sbFileUpload.AppendLine("eight.appendChild(ten);");

            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");

            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;

            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");

            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");


            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

            sbFileUpload.AppendLine("eight.appendChild(eleven);");

            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("btnEleven.value='" + msg.uploadOrDownPath.Replace(@"\", "/") + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            //赋上传之前的地址
            receive.haveDownFile = msg.uploadOrDownPath.Replace(@"\", "/");
            receive.messageId = msg.messageId;
            sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

            //阅后即焚图片显示外层
            sbFileUpload.AppendLine("var imageOutDiv=document.createElement('div');");
            sbFileUpload.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbFileUpload.AppendLine("var imageInDiv=document.createElement('img');");
            sbFileUpload.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbFileUpload.AppendLine("first.appendChild(imageOutDiv);");

            if (msg.sendsucessorfail == 0)
            {
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                //2017-07-23 添加 重发div
                sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
            }

            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbFileUpload.AppendLine("document.body.appendChild(first);");



            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 阅后即焚右侧语音显示 Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightAfterBurnShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            var voiceFile = string.IsNullOrEmpty(receive.audioUrl) ? msg.uploadOrDownPath : receive.audioUrl;
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbRight.AppendLine("first.setAttribute('onmousedown','VoiceRightMenuMethod(event)');first.setAttribute('selfmethods','one');first.setAttribute('audioUrl','" + receive.audioUrl + "');first.setAttribute('isRead','0');first.setAttribute('isShowImg','" + isShowReadImgId + "');first.setAttribute('isShowGif','" + isShowGifId + "');first.setAttribute('isLeftOrRight','1');");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");



            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");


            sbRight.AppendLine("first.appendChild(three);");

            ////未读圆形提示
            //string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接受成功.png").Replace(@"\", @"/").Replace(" ", "%20");
            //sbRight.AppendLine("var divCircle = document.createElement('div');");
            //sbRight.AppendLine("divCircle.className ='rightVoice';");
            //sbRight.AppendLine("var imgCircle = document.createElement('img');");
            //sbRight.AppendLine("imgCircle.src='" + CirCleImg + "';");
            //sbRight.AppendLine("divCircle.appendChild(imgCircle);");
            //sbRight.AppendLine("first.appendChild(divCircle);");

            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");

            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");
            //重发
            if (msg.sendsucessorfail == 0)
            {
                sbRight.AppendLine(OnceSendHistoryMsgDiv("sendBurnVoice", msg.messageId, voiceFile, Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
            }
            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            //sbRight.AppendLine("document.body.appendChild(first);}");
            sbRight.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }
        /*正常左侧显示对应方法*/
        /// <summary>
        /// 正常左侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");


            sbLeft.AppendLine("var img = document.createElement('img');");


            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");


            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);


            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");
            var content = sbLeft.ToString();
            var task = cef.EvaluateScriptAsync(content);
            task.Wait();

        }
        /// <summary>
        /// 正常左侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            //string pathImage = user.picture + "" == "" ? "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20") : user.picture;
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");

            sbLeftImage.AppendLine("img.src='" + user.copyPicture + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + user.userId + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //sbLeftImage.AppendLine("var img = document.createElement('img');");
            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbLeftImage.AppendLine(oneLeftImageString(imageid));

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            sbLeftImage.AppendLine("img.id='" + imageid + "';");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            if (msg.IsSetImgLoadComplete)
            {
                sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            }

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeftImage.ToString());
            task.Wait();
        }
        /// <summary>
        /// 正常左侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + pathImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + user.userId + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            //另存为
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbFileUpload.ToString());
            task.Wait();
        }
        /// <summary>
        /// 正常左侧语音显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');nodeFirst.setAttribute('sid','" + msg.messageId + "');");
            // sbLeft.AppendLine("nodeFirst.addEventListener('click',clickDivVoiceCall);");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");

            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");

            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //未读圆形提示
            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var divCircle = document.createElement('div');");
            sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
            sbLeft.AppendLine("divCircle.className ='leftVoice';");
            sbLeft.AppendLine("var imgCircle = document.createElement('img');");
            sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
            sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
            sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }
        /// <summary>
        /// 左侧语音电话显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="dt"></param>
        /// <param name="mode"></param>
        public static void LefttShowAudio(ChromiumWebBrowser cef, AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
            if (string.IsNullOrEmpty(audioMsg.text))
                audioMsg.text = "发起了语音电话";
            var pathImage = user.copyPicture;
            var sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var first=document.createElement('div');");
            sbLeft.AppendLine("first.className='leftd';");
            sbLeft.AppendLine("first.id='" + msg.messageId + "';");
            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("first.appendChild(second);");
            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("first.appendChild(timeshow);");
            string showMsg = PublicTalkMothed.talkContentReplace(msg.sourceContent);
            sbLeft.AppendLine("var three=document.createElement('div');");
            sbLeft.AppendLine("three.className='speech left';");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("three.appendChild(imgmp3);");
            sbLeft.AppendLine("var node = document.createElement('span');");
            sbLeft.AppendLine("node.innerHTML='&nbsp;&nbsp;" + audioMsg.text + "';");
            sbLeft.AppendLine("three.appendChild(node);");
            sbLeft.AppendLine("first.appendChild(three);");
            sbLeft.AppendLine("document.body.appendChild(first);}");
            sbLeft.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }

        /*正常右侧显示对应方法*/
        /// <summary>
        /// 正常右侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void rightShowText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbRight.AppendLine(divRightCopyContent(divid, msg.messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbRight.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(node);");
            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
        }
        /// <summary>
        /// 正常右侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            //sbRight.AppendLine("var img1 = document.createElement('img');");
            string imageid = "wlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbRight.AppendLine(oneSentImageString(imageid, msg.messageId));

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.chatIndex + "');");
            sbRight.AppendLine("img1.setAttribute('mid', 'M" + msg.messageId + "');");
            sbRight.AppendLine("img1.id='" + imageid + "';");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");
            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
        }
        /// <summary>
        /// 正常右侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            #region 构造发送文件解析
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine(oneSentFile(msg.messageId));
            sbFileUpload.Append("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
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
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
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
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            var setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbFileUpload.ToString());
            task.Wait();
            #endregion
        }
        /// <summary>
        /// 正常右侧语音显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbRight.AppendLine("first.setAttribute('onmousedown','VoiceRightMenuMethod(event)');first.setAttribute('selfmethods','one');first.setAttribute('audioUrl','" + receive.audioUrl + "');first.setAttribute('isRead','0');first.setAttribute('isShowImg','" + isShowReadImgId + "');first.setAttribute('isShowGif','" + isShowGifId + "');first.setAttribute('isLeftOrRight','1');");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");



            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");


            sbRight.AppendLine("first.appendChild(three);");

            ////未读圆形提示
            //string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接受成功.png").Replace(@"\", @"/").Replace(" ", "%20");
            //sbRight.AppendLine("var divCircle = document.createElement('div');");
            //sbRight.AppendLine("divCircle.className ='rightVoice';");
            //sbRight.AppendLine("var imgCircle = document.createElement('img');");
            //sbRight.AppendLine("imgCircle.src='" + CirCleImg + "';");
            //sbRight.AppendLine("divCircle.appendChild(imgCircle);");
            //sbRight.AppendLine("first.appendChild(divCircle);");

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
            #endregion
        }
        /// <summary>
        /// 正常右侧语音电话显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowAudio(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
            if (string.IsNullOrEmpty(audioMsg.text))
                audioMsg.text = "发起了语音电话";
            var sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var first=document.createElement('div');");
            sbLeft.AppendLine("first.className='rightd';");
            sbLeft.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='rightTimeStyle';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("first.appendChild(timeshow);");
            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='rightimg';");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbLeft.AppendLine("img.className='divcss5';");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("first.appendChild(second);");

            sbLeft.AppendLine("var three=document.createElement('div');");
            sbLeft.AppendLine("three.className='speech right';");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-右.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbLeft.AppendLine("three.appendChild(imgmp3);");

            sbLeft.AppendLine("var node = document.createElement('span');");
            sbLeft.AppendLine("node.innerHTML='" + audioMsg.text + "&nbsp;&nbsp;';");
            sbLeft.AppendLine("three.appendChild(node);");
            sbLeft.AppendLine("first.appendChild(three);");
            sbLeft.AppendLine("document.body.appendChild(first);}");
            sbLeft.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }
        /*正常右侧发送对应方法*/
        /// <summary>
        /// 正常右侧发送文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="strMsg"></param>
        public static void rightSendText(ChromiumWebBrowser cef, string strMsg, string messageId, string imageTipId, string imageSendingId, DateTime dt, string msgStr, bool IsRobot)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");

            //sbRight.AppendLine(oneSentFile(messageId));
            //2017-04-07添加
            sbRight.AppendLine("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat")||(imgUrl+"")=="")
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            if (IsRobot)
            {
                sbRight.AppendLine(divLeftCopyContent(divid));
            }
            else
            {
                sbRight.AppendLine(divRightCopyContent(divid, messageId));
            }

            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            //System.Windows.Forms.MessageBox.Show(SendStr);
            sbRight.AppendLine("node.innerHTML ='" + strMsg.Replace("\r\n", "<br/>").Replace("\n", "<br/>") + "';");
            sbRight.AppendLine("first.appendChild(node);");

            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendText", messageId, strMsg, imageTipId, imageSendingId, msgStr, ""));

            sbRight.AppendLine("document.body.appendChild(first);}");
            sbRight.AppendLine("myFunction();");

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                var task = cef.EvaluateScriptAsync(sbRight.ToString());
                task.Wait();
                #region
                StringBuilder sbEnd = new StringBuilder();
                sbEnd.AppendLine("setscross();");
                var taskEnd = cef.EvaluateScriptAsync(sbEnd.ToString());

                #endregion
            }));

            #endregion
        }
        /// <summary>
        /// 正常右侧发送图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="lists"></param>
        /// <param name="fileFileName"></param>
        /// <param name="_richTextBox"></param>
        /// <param name="e"></param>
        public static void rightSendImage(ChromiumWebBrowser cef, System.Windows.Controls.Image lists, string fileFileName, RichTextBox _richTextBox, System.Windows.Input.KeyEventArgs e, string messageId, string imageTipId, string imageSendingId, DateTime dt, bool IsRobot)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            //sbRight.AppendLine("var img1 = document.createElement('img');");
            string imageid = "wlc" + fileFileName + "";
            if (IsRobot == true)
            {
                sbRight.AppendLine(RobitImageString(imageid));
            }
            else
            {
                sbRight.AppendLine(oneSentImageString(imageid, messageId));
            }

            string imgLocalPath = lists.Source.ToString();
            sbRight.AppendLine("img1.src='" + imgLocalPath + "';");


            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='" + imageid + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //重发div
            string isSucessOrFail = "0";
            if (imgLocalPath.StartsWith("file:///"))
            {
                isSucessOrFail = "1";
            }
            sbRight.AppendLine(OnceSendMsgDiv("0", messageId, imgLocalPath, imageTipId, imageSendingId, isSucessOrFail, ""));

            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            //task.Wait();
            if (_richTextBox != null)
            {
                _richTextBox.Document.Blocks.Clear();
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 正常右侧发送文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="listfile"></param>
        public static void rightSendFile(ChromiumWebBrowser cef, UpLoadFilesDto listfile)
        {
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            if (!listfile.IsRobot)
            {
                sbFileUpload.AppendLine(oneSentFile(listfile.messageId));
            }
            sbFileUpload.Append("first.id='" + listfile.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + listfile.DtTime.ToString("HH:mm:ss") + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(listfile.fileExtendName, listfile.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + listfile.fileName + "';");
            string fileName = listfile.fileName.Length > 10 ? listfile.fileName.Substring(0, 10) + "..." : listfile.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(listfile.fileSize) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.id='" + listfile.progressId + "';");
            sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");
            sbFileUpload.AppendLine("var eight=document.createElement('div');");
            sbFileUpload.AppendLine("eight.className='fileOperateDiv';");
            sbFileUpload.AppendLine("second.appendChild(eight);");
            sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
            sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");
            sbFileUpload.AppendLine("eight.appendChild(imgSorR);");
            //上传中图片添加
            sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
            sbFileUpload.AppendLine("showSOFImg.className='onging';");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
            sbFileUpload.AppendLine("showSOFImg.id='" + listfile.fileImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //上传中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            sbFileUpload.AppendLine("nightButton.id='" + listfile.fileTextGuid + "';");
            sbFileUpload.AppendLine("nightButton.innerHTML='上传中';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开按钮添加
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            sbFileUpload.AppendLine("btnten.id='" + listfile.fileOpenguid + "';");
            string localPath = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnten.value='" + localPath + "';");
            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹按钮添加
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            sbFileUpload.AppendLine("btnEleven.id='" + listfile.fileOpenDirectory + "';");
            string localPathDirectory = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");
            //重发div
            sbFileUpload.AppendLine(OnceSendMsgDiv("1", listfile.messageId, localPathDirectory, listfile.imageTipId, listfile.imageSendingId, "", ""));
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 正常右侧发送语音显示 Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="dt"></param>
        /// <param name="voice"></param>
        public static void RightShowSendVoice(ChromiumWebBrowser cef, DateTime dt, ReturnCutImageDto voice, int duration)
        {
            var voiceFile = string.IsNullOrEmpty(voice.fileUrl) ? voice.prePath : voice.fileUrl;
            var isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            var isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            var sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + voice.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //右侧头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            //点击播放
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + voice.messageId + "';");
            //sbRight.AppendLine("three.id='M" + Guid.NewGuid().GetHashCode() + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            //时长
            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendVoice", voice.messageId, voiceFile, voice.imagedTipId, voice.imageSendingId, voice.prePath, ""));
            sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");
            sbRight.AppendLine("myFunction();");
            var task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();
        }
        /// <summary>
        /// 显示右侧语音电话或者视频
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="dt"></param>
        /// <param name="mode">0-语音电话 1-视频</param>
        public static void RightShowSendAudio(ChromiumWebBrowser cef, string msg, DateTime dt, int mode)
        {
            var sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //右侧头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-右.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            sbRight.AppendLine("var node = document.createElement('span');");
            sbRight.AppendLine("node.innerHTML='" + msg + "&nbsp;&nbsp;';");
            sbRight.AppendLine("three.appendChild(node);");
            sbRight.AppendLine("first.appendChild(three);");
            sbRight.AppendLine("document.body.appendChild(first);}");
            sbRight.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbRight.ToString());
            StringBuilder sbEnds = new StringBuilder();
            sbEnds.AppendLine("setscross();");
            var task = cef.EvaluateScriptAsync(sbEnds.ToString());
            task.Wait();
        }

        /*阅后即焚右侧发送对应方法*/
        /// <summary>
        /// 正常右侧发送阅后即焚文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="strMsg"></param>
        public static void rightSendTextBurn(ChromiumWebBrowser cef, string strMsg, string messageId, string imageTipId, string imageSendingId, DateTime dt, string msgStr)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.innerHTML ='" + strMsg.Replace("\r\n", "<br/>").Replace("\n", "<br/>") + "';");
            sbRight.AppendLine("first.appendChild(three);");


            //阅后即焚图片显示外层
            //sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            //sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("first.appendChild(imageInDiv);");
            //sbRight.AppendLine("first.appendChild(imageOutDiv);");

            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendBurnText", messageId, strMsg, imageTipId, imageSendingId, msgStr, ""));

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());

            #endregion
            #region
            StringBuilder sbEnd = new StringBuilder();
            sbEnd.AppendLine("setscross();");
            var taskEnd = cef.EvaluateScriptAsync(sbEnd.ToString());
            #endregion
        }
        /// <summary>
        /// 正常右侧发送阅后即焚图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="lists"></param>
        /// <param name="fileFileName"></param>
        /// <param name="_richTextBox"></param>
        /// <param name="e"></param>
        public static void rightSendImageBurn(ChromiumWebBrowser cef, System.Windows.Controls.Image lists, string fileFileName, RichTextBox _richTextBox, System.Windows.Input.KeyEventArgs e, string messageId, string imageTipId, string imageSendingId, DateTime dt)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyleOnce';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = "http://www.divcss5.com/yanshi/2014/2014063001/images/1.jpg";

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");

            string imgLocalPath = lists.Source.ToString();
            sbRight.AppendLine("img1.src='" + imgLocalPath + "';");

            //if (Convert.ToInt32(scid.imageWidth) > 500)
            //{
            //    sbRight.AppendLine("img1.style.width='500px';");
            //}
            //sbRight.AppendLine("img1.style.height='" + scid.imageHeight + "px';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + messageId + "');");
            sbRight.AppendLine("img1.id='wlc" + fileFileName + "';");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("2", messageId, imgLocalPath, imageTipId, imageSendingId, "", ""));

            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDivOnce';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");

            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            if (e != null)
            {
                e.Handled = true;
            }
            if (_richTextBox != null)
            {
                _richTextBox.Document.Blocks.Clear();
            }
            if (e != null)
            {
                e.Handled = true;
            }
            #endregion
        }
        /// <summary>
        /// 正常右侧发送阅后即焚文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="listfile"></param>
        public static void rightSendFileBurn(ChromiumWebBrowser cef, UpLoadFilesDto listfile)
        {
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + listfile.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyleOnce';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + listfile.DtTime.ToString("HH:mm:ss") + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(listfile.fileExtendName, listfile.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + listfile.fileName + "';");
            string fileName = listfile.fileName.Length > 10 ? listfile.fileName.Substring(0, 10) + "..." : listfile.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(listfile.fileSize) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.id='" + listfile.progressId + "';");
            sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");
            sbFileUpload.AppendLine("var eight=document.createElement('div');");
            sbFileUpload.AppendLine("eight.className='fileOperateDiv';");
            sbFileUpload.AppendLine("second.appendChild(eight);");
            sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
            sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");
            sbFileUpload.AppendLine("eight.appendChild(imgSorR);");
            //上传中图片添加
            sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
            sbFileUpload.AppendLine("showSOFImg.className='onging';");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
            sbFileUpload.AppendLine("showSOFImg.id='" + listfile.fileImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //上传中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            sbFileUpload.AppendLine("nightButton.id='" + listfile.fileTextGuid + "';");
            sbFileUpload.AppendLine("nightButton.innerHTML='上传中';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开按钮添加
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            sbFileUpload.AppendLine("btnten.id='" + listfile.fileOpenguid + "';");
            string localPath = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnten.value='" + localPath + "';");
            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹按钮添加
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            sbFileUpload.AppendLine("btnEleven.id='" + listfile.fileOpenDirectory + "';");
            string localPathDirectory = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");
            //重发div
            sbFileUpload.AppendLine(OnceSendMsgDiv("3", listfile.messageId, localPathDirectory, listfile.imageTipId, listfile.imageSendingId, "", ""));
            //阅后即焚图片显示外层
            sbFileUpload.AppendLine("var imageOutDiv=document.createElement('div');");
            sbFileUpload.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
            //阅后即焚图片显示内层
            sbFileUpload.AppendLine("var imageInDiv=document.createElement('img');");
            sbFileUpload.AppendLine("imageInDiv.className='rightInAfterImageDivOnce';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbFileUpload.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbFileUpload.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbFileUpload.AppendLine("first.appendChild(imageOutDiv);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 右侧发送阅后即焚语音显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="dt"></param>
        /// <param name="voice"></param>
        /// <param name="duration"></param>
        public static void rightSendVoiceBurn(ChromiumWebBrowser cef, DateTime dt, ReturnCutImageDto voice, int duration)
        {
            var voiceFile = string.IsNullOrEmpty(voice.fileUrl) ? voice.prePath : voice.fileUrl;
            var isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            var isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            var sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + voice.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //右侧头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            //点击播放
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + voice.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            //时长
            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            //阅后即焚图片显示外层
            sbRight.AppendLine("var imageOutDiv=document.createElement('div');");
            sbRight.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
            //阅后即焚图片显示内层
            sbRight.AppendLine("var imageInDiv=document.createElement('img');");
            sbRight.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
            sbRight.AppendLine("imageOutDiv.appendChild(imageInDiv);");
            sbRight.AppendLine("first.appendChild(imageOutDiv);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendBurnVoice", voice.messageId, voiceFile, voice.imagedTipId, voice.imageSendingId, voice.prePath, ""));
            sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");
            sbRight.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbRight.ToString());
        }

        /*正常左侧滚轮显示对应方法*/
        /// <summary>
        /// 正常左侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name=""></param>
        /// <param name="user"></param>
        public static void leftShowScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");


            sbLeft.AppendLine("var img = document.createElement('img');");


            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");


            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);


            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 正常左侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");
            sbLeftImage.AppendLine("img.src='" + pathImage + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + user.userId + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //sbLeftImage.AppendLine("var img = document.createElement('img');");
            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbLeftImage.AppendLine(oneLeftImageString(imageid));

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            sbLeftImage.AppendLine("img.id='" + imageid + "';");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeftImage.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeftImage.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);");

            //sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());
        }
        /// <summary>
        /// 正常左侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.seId = msg.sessionId;
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + pathImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + user.userId + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            if (isDownFile == true)
            {
                sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
            }
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
            string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
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
            string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
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
            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
        }
        /// <summary>
        /// 正常左侧语音显示 voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            string pathImage = user.copyPicture;
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');");
            // sbLeft.AppendLine("nodeFirst.addEventListener('click',clickDivVoiceCall);");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");

            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','" + msg.voiceread + "');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");

            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");
            if (msg.voiceread + "" != "1")
            {
                //未读圆形提示
                string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
                sbLeft.AppendLine("var divCircle = document.createElement('div');");
                sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                sbLeft.AppendLine("divCircle.className ='leftVoice';");
                sbLeft.AppendLine("var imgCircle = document.createElement('img');");
                sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
                sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
                sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");
            }

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            //cef.ExecuteScriptAsync(sbLeft.ToString());

            cef.ExecuteScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 正常左侧滚轮语音电话显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void leftShowScrollAudio(ChromiumWebBrowser cef, AntSdkChatMsg.ChatBase msg,
            AntSdkContact_User user)
        {
            var pathImage = user.copyPicture;
            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
            if (string.IsNullOrEmpty(audioMsg.text))
                audioMsg.text = "发起了语音电话";
            var sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var first=document.createElement('div');");
            sbLeft.AppendLine("first.className='leftd';");
            sbLeft.AppendLine("first.id='" + msg.messageId + "';");
            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + pathImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("first.appendChild(second);");
            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("first.appendChild(timeshow);");
            string showMsg = PublicTalkMothed.talkContentReplace(msg.sourceContent);
            sbLeft.AppendLine("var three=document.createElement('div');");
            sbLeft.AppendLine("three.className='speech left';");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-左.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("three.appendChild(imgmp3);");
            sbLeft.AppendLine("var node = document.createElement('span');");
            sbLeft.AppendLine("node.innerHTML='&nbsp;&nbsp;" + audioMsg.text + "';");
            sbLeft.AppendLine("three.appendChild(node);");
            sbLeft.AppendLine("first.appendChild(three);");
            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            sbLeft.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbLeft.ToString());
        }

        /*正常右侧滚轮显示对应方法*/
        /// <summary>
        /// 正常右侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            if (msg.IsRobot)
            {
                sbRight.AppendLine(divLeftCopyContent(divid));
            }
            else
            {
                sbRight.AppendLine(divRightCopyContent(divid, msg.messageId));
            }
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbRight.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(node);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                sbRight.AppendLine(OnceSendHistoryMsgDiv("sendText", msg.messageId, showMsg, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
            }

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");

            //sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 正常右侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            //sbRight.AppendLine("var img1 = document.createElement('img');");
            string imageid = "wlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            if (msg.IsRobot)
            {
                sbRight.AppendLine(RobitImageString(imageid));
            }
            else
            {
                sbRight.AppendLine(oneSentImageString(imageid, msg.messageId));
            }

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='" + imageid + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                string imgLocalPath = pathHtml;
                sbRight.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
            }

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            //sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 下载过的显示方式
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowScrollImageDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 正常右侧显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            #region 构造发送文件解析
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            if (!msg.IsRobot)
            {
                sbFileUpload.AppendLine(oneSentFile(msg.messageId));
            }
            sbFileUpload.Append("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            var txtShowSucessImg = "success";//接收上传状态图片
            string txtShowDown = "上传成功";//接收上传状态文字
            string txtOpenOrReceive = "打开";//打开接收文字
            string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
                                       //同一端发送
            if (isOpenOrReceive)
            {
                if (msg.sendsucessorfail == 1)//发送成功
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "上传成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//发送失败
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "fail";
                    txtShowDown = "上传失败";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
            }
            //不同端同步的消息
            else
            {
                if (isDownFile)//已经下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "下载成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//未下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "reveiving";
                    txtShowDown = "未下载";
                    txtOpenOrReceive = "接收";
                    txtSaveAs = "另存为";
                }
            }
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
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //发送失败提示
            if (msg.sendsucessorfail == 0)
            {
                sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath,
                    Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
            }
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 正常右侧下载过的文件显示方式 HavedDownFile
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void rightShowScrollFileDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            if (!msg.IsRobot)
            {
                sbFileUpload.AppendLine(oneSentFile(msg.messageId));
            }
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");

            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");


            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");

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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");

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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            //string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
            //receive.progressId = progressId;
            //sbFileUpload.AppendLine("sevenSecond.id='" + progressId + "';");
            sbFileUpload.AppendLine("sevenSecond.style.width='" + (msg.sendsucessorfail > 0 ? "100%" : "0%") + "';");

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
            sbFileUpload.AppendLine("showSOFImg.src='" + (msg.sendsucessorfail > 0 ? fileShowImage.showImageHtmlPath("success", "") : fileShowImage.showImageHtmlPath("fail", "")) + "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");

            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");

            sbFileUpload.AppendLine("eight.appendChild(night);");

            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + (msg.sendsucessorfail > 0 ? "上传成功" : "上传失败") + "';");

            sbFileUpload.AppendLine("night.appendChild(nightButton);");



            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");

            sbFileUpload.AppendLine("eight.appendChild(ten);");

            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");

            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;

            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");

            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");

            if (msg.sendsucessorfail == 0)
            {
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                //2017-07-23 添加 重发div
                sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
            }

            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

            sbFileUpload.AppendLine("eight.appendChild(eleven);");

            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("btnEleven.value='" + msg.uploadOrDownPath.Replace(@"\", "/") + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            //赋上传之前的地址
            receive.haveDownFile = msg.uploadOrDownPath.Replace(@"\", "/");

            sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbFileUpload.AppendLine("document.body.appendChild(first);");



            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 正常右侧语音显示 voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            var voiceFile = string.IsNullOrEmpty(receive.audioUrl) ? msg.uploadOrDownPath : receive.audioUrl;
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            var isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            var isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            //头像
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            //播放
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            if (msg.IsRobot == true)
            {
                sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');three.setAttribute('isRecallMenu','1');");
            }
            else
            {
                sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");
            }
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            //时长
            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            //重发
            if (msg.sendsucessorfail == 0)
            {
                sbRight.AppendLine(OnceSendHistoryMsgDiv("sendVoice", msg.messageId, voiceFile, Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
            }
            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            sbRight.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }
        /// <summary>
        /// 正常右侧滚轮语音电话显示
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        public static void rightShowScrollAudio(ChromiumWebBrowser cef, AntSdkChatMsg.ChatBase msg)
        {
            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
            if (string.IsNullOrEmpty(audioMsg.text))
                audioMsg.text = "发起了语音电话";
            var sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var first=document.createElement('div');");
            sbLeft.AppendLine("first.className='rightd';");
            sbLeft.AppendLine("first.id='" + msg.messageId + "';");
            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className=''rightimg;");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbLeft.AppendLine("img.className='divcss5';");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("first.appendChild(second);");
            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className=''rightTimeStyle;");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("first.appendChild(timeshow);");
            sbLeft.AppendLine("var three=document.createElement('div');");
            sbLeft.AppendLine("three.className='speech right';");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-右.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbLeft.AppendLine("three.appendChild(imgmp3);");
            sbLeft.AppendLine("var node = document.createElement('span');");
            sbLeft.AppendLine("node.innerHTML='" + audioMsg.text + "&nbsp;&nbsp;';");
            sbLeft.AppendLine("three.appendChild(node);");
            sbLeft.AppendLine("first.appendChild(three);");
            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            sbLeft.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbLeft.ToString());
        }

        /*-----------------------------------群消息展示-------------------------------------*/
        /// <summary>
        /// 获取群聊个人头像
        /// </summary>
        /// <param name="GroupMembers"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static string getPathImage(List<AntSdkGroupMember> GroupMembers, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            //获取接收者头像
            string pathImages = "";
            //获取接收者头像
            var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
            AntSdkGroupMember users = GroupMembers != null && GroupMembers.Count > 0 ? GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId) : null;
            if (listUser == null)
            {
                if (users == null)
                {
                    AntSdkContact_User cuss = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
                    if (cuss == null)
                    {
                        //users = new AntSdkGroupMember();
                        pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //users.picture = pathImages;
                        //users.userName = "离职人员";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(cuss.picture + ""))
                        {
                            pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                        }
                        else
                        {
                            pathImages = cuss.picture;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(users.picture))
                        pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                    else
                    {
                        pathImages = users.picture;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(listUser.Url))
                {
                    pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                }
                else
                {
                    pathImages = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
                }
            }


            return pathImages;
        }
        /// <summary>
        /// 获取获取对应群员信息
        /// </summary>
        /// <param name="GroupMembers"></param>
        /// <returns></returns>
        private static AntSdkGroupMember getGroupMembersUser(List<AntSdkGroupMember> GroupMembers, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            string pathImages = "";
            //获取接收者头像
            var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
            //AntSdkGroupMember users = GroupMembers != null && GroupMembers.Count > 0 ? GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId) : null;
            var users = new AntSdkGroupMember();
            //if (users == null)
            //{
            AntSdkContact_User cuss = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
            if (cuss == null)
            {
                users = new AntSdkGroupMember();
                pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
                users.picture = pathImages;
                users.userId = msg.sendUserId;
                users.userName = "离职人员";
            }
            else
            {
                users = new AntSdkGroupMember
                {
                    userId = cuss.userId,
                    userName = cuss.status == 0 && cuss.state == 0 ? cuss.userName + "（停用）" : cuss.userName,
                    userNum = cuss.userNum,
                    picture = cuss.picture,
                    position = cuss.position
                };

            }
            //}
            //else
            //{
            //    if (listUser != null)
            //        pathImages = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
            //   // users.picture = pathImages;
            //}
            return users;
        }


        /// <summary>
        /// 正常群聊左侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void leftGroupShowText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);


            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");

            //添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            try
            {
                Task<JavascriptResponse> results = cef.EvaluateScriptAsync(sbLeft.ToString());
                if (!results.Result.Success)
                {
                    cef.WebBrowser.ExecuteScriptAsync(sbLeft.ToString());
                    LogHelper.WriteDebug("-----------------" + /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent + "---------------------------");
                    //Console.WriteLine("-----------------------------------------error:" + msg.content + "--------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError("cef.EvaluateScriptAsync:" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 接收到@消息解析
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        /// <param name="contects"></param>
        public static void leftGroupShowAtText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers, string contects)
        {

            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");


            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.id='" + msg.chatIndex + "';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");

            //添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = contects;

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 滚动@消息解析
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        /// <param name="contects"></param>
        public static void leftGroupShowAtScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers, string contects)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");


            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");

            //添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = contects;

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");
            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 正常群聊左侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeftImage.AppendLine(groupImageString(userid));
            sbLeftImage.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + userid + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");


            ////添加用户名称
            //sbLeftImage.AppendLine("var username=document.createElement('div');");
            //sbLeftImage.AppendLine("username.className='leftUsername';");
            //sbLeftImage.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeftImage.AppendLine("nodeFirst.appendChild(username);");

            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //sbLeftImage.AppendLine("var img = document.createElement('img');");
            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbLeftImage.AppendLine(oneLeftImageString(imageid));

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='" + imageid + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            if (msg.IsSetImgLoadComplete)
            {
                sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            }

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());

            //StringBuilder sbEnds = new StringBuilder();
            //sbEnds.AppendLine("setscross();");
            //cef.EvaluateScriptAsync(sbEnds.ToString());
        }
        /// <summary>
        /// 正常群聊左侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        public static void LeftGroupShowVioce(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            ////添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");
            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");


            //未读圆形提示
            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var divCircle = document.createElement('div');");
            sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
            sbLeft.AppendLine("divCircle.className ='leftVoice';");
            sbLeft.AppendLine("var imgCircle = document.createElement('img');");
            sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
            sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
            sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }

        /*正常右侧发送对应方法*/
        /// <summary>
        /// 正常群聊右侧发送文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="sendStr"></param>
        public static void RightGroupShowSendText(ChromiumWebBrowser cef, string sendStr, KeyEventArgs e, string messageid, string imageTipId, string imageSendingId, DateTime dt, string msgStr)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            //sbRight.AppendLine(oneSentFile(messageid));
            sbRight.AppendLine("first.id='" + messageid + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + DateTime.Now.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbRight.AppendLine(divRightCopyContent(divid, messageid));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string test = sendStr.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            sbRight.AppendLine("node.innerHTML ='" + test + "';");

            sbRight.AppendLine("first.appendChild(node);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendText", messageid, sendStr, imageTipId, imageSendingId, msgStr, ""));

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            //if (e != null)
            //{
            //    e.Handled = true;
            //}
            #endregion
        }
        /// <summary>
        /// 发送At消息
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="sendStr"></param>
        /// <param name="e"></param>
        /// <param name="messageid"></param>
        /// <param name="imageTipId"></param>
        /// <param name="imageSendingId"></param>
        /// <param name="dt"></param>
        /// <param name="msgStr"></param>
        /// <param name="atPreValues"></param>
        public static void RightGroupShowSendAtText(ChromiumWebBrowser cef, string sendStr, KeyEventArgs e, string messageid, string imageTipId, string imageSendingId, DateTime dt, string msgStr, string atPreValues)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + messageid + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            //头像
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            //复制内容
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbRight.AppendLine(divRightCopyContent(divid, messageid));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            sbRight.AppendLine("node.innerHTML ='" + sendStr.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\n", "<br/>") + "';");
            sbRight.AppendLine("first.appendChild(node);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendAtText", messageid, sendStr, imageTipId, imageSendingId, msgStr, atPreValues));
            sbRight.AppendLine("document.body.appendChild(first);}");
            sbRight.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbRight.ToString());
            #endregion
        }
        /// <summary>
        /// 正常群聊右侧发送图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="lists"></param>
        /// <param name="e"></param>
        /// <param name="richTextBox"></param>
        /// <param name="fileFileName"></param>
        public static void RightGroupShowSendImage(ChromiumWebBrowser cef, Image lists, KeyEventArgs e, RichTextBox richTextBox, string fileFileName, string messageId, string imageTipId, string imageSendingId, DateTime dt)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");



            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            //sbRight.AppendLine("var img1 = document.createElement('img');");
            string imageid = "wlc" + fileFileName + "";
            sbRight.AppendLine(oneSentImageString(imageid, messageId));

            string imgLocalPath = lists.Source.ToString();
            sbRight.AppendLine("img1.src='" + imgLocalPath + "';");


            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='" + imageid + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");
            //重发div
            string isSucessOrFail = "0";
            if (imgLocalPath.StartsWith("file:///"))
            {
                isSucessOrFail = "1";
            }
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("0", messageId, imgLocalPath, imageTipId, imageSendingId, isSucessOrFail, ""));

            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            if (richTextBox != null)
            {
                richTextBox.Document.Blocks.Clear();
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 正常群聊右侧发送文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="listfile"></param>
        public static void RightGroupShowSendFile(ChromiumWebBrowser cef, UpLoadFilesDto listfile)
        {
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine(oneSentFile(listfile.messageId));
            //201-04-06 添加
            sbFileUpload.Append("first.id='" + listfile.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + listfile.DtTime.ToString("HH:mm:ss") + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(listfile.fileExtendName, listfile.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + listfile.fileName + "';");
            string fileName = listfile.fileName.Length > 12 ? listfile.fileName.Substring(0, 10) + "..." : listfile.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(listfile.fileSize) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.id='" + listfile.progressId + "';");
            sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");
            sbFileUpload.AppendLine("var eight=document.createElement('div');");
            sbFileUpload.AppendLine("eight.className='fileOperateDiv';");
            sbFileUpload.AppendLine("second.appendChild(eight);");
            sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
            sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");
            sbFileUpload.AppendLine("eight.appendChild(imgSorR);");
            //上传中图片添加
            sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
            sbFileUpload.AppendLine("showSOFImg.className='onging';");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
            sbFileUpload.AppendLine("showSOFImg.id='" + listfile.fileImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //上传中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            sbFileUpload.AppendLine("nightButton.id='" + listfile.fileTextGuid + "';");
            sbFileUpload.AppendLine("nightButton.innerHTML='上传中';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开按钮添加
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            sbFileUpload.AppendLine("btnten.id='" + listfile.fileOpenguid + "';");
            string localPath = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnten.value='" + localPath + "';");
            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹按钮添加
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            sbFileUpload.AppendLine("btnEleven.id='" + listfile.fileOpenDirectory + "';");
            string localPathDirectory = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");
            //2017-04-06 添加 重发div
            sbFileUpload.AppendLine(OnceSendMsgDiv("1", listfile.messageId, localPathDirectory, listfile.imageTipId, listfile.imageSendingId, "", ""));
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbFileUpload.ToString());
        }
        /// <summary>
        /// 正常群聊右侧发送语音显示 Voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="dt"></param>
        /// <param name="voice"></param>
        public static void RightGroupShowSendVoice(ChromiumWebBrowser cef, DateTime dt, ReturnCutImageDto voice, int duration)
        {
            var voiceFile = string.IsNullOrEmpty(voice.fileUrl) ? voice.prePath : voice.fileUrl;
            var isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            var isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            var sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + voice.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            //右侧头像显示层
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            //点击播放
            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + voice.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");
            var musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            //时长
            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendVoice", voice.messageId, voiceFile, voice.imagedTipId, voice.imageSendingId, voice.prePath, ""));
            sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");
            sbRight.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbRight.ToString());
        }

        /*正常左侧滚轮显示对应方法*/
        /// <summary>
        /// 正常群聊左侧滚轮文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);


            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            //sbLeft.AppendLine("var node=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbLeft.AppendLine(divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");
            //App.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    try
            //    {
            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            //        if (!task.Result.Success)
            //        {

            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}));
        }
        /// <summary>
        /// 正常群聊左侧滚轮图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeftImage.AppendLine(groupImageString(userid));
            sbLeftImage.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + userid + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");


            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //sbLeftImage.AppendLine("var img = document.createElement('img');");
            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbLeftImage.AppendLine(oneLeftImageString(imageid));

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='" + imageid + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeftImage.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeftImage.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);");

            //sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            //sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());
        }
        /// <summary>
        /// 正常群聊左侧滚轮文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.seId = msg.sessionId;
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            if (isDownFile == true)
            {
                sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
            }
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
            string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
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
            string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
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
            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
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
            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 正常左侧滚轮下载完成文件显示 filedowm
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupShowScrollFileDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);//IsDownFileMethod(receive.fileUrl, msg.uploadOrDownPath, msg.sendsucessorfail.ToString());
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");


            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");


            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");

            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");

            ////添加用户名称
            //sbFileUpload.AppendLine("var username=document.createElement('div');");
            //sbFileUpload.AppendLine("username.className='leftUsername';");
            //sbFileUpload.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbFileUpload.AppendLine("first.appendChild(username);");

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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.style.width='100%';");

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
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("success", "") + "';");
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
            sbFileUpload.AppendLine("nightButton.innerHTML='下载完成';");

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

            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
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
            sbFileUpload.AppendLine("btnEleven.value='" + msg.uploadOrDownPath.Replace(@"\", "/") + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");

            //下载完成更新路径
            receive.messageId = msg.messageId;

            sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbFileUpload.AppendLine("document.body.appendChild(first);");



            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        public static void LeftGroupShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(" + msg.messageId + ")');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            ////添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");
            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            if (msg.voiceread + "" != "1")
            {
                //未读圆形提示
                string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
                sbLeft.AppendLine("var divCircle = document.createElement('div');");
                sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                sbLeft.AppendLine("divCircle.className ='leftVoice';");
                sbLeft.AppendLine("var imgCircle = document.createElement('img');");
                sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
                sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
                sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");
            }
            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");
            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbLeft.ToString());
        }


        /*正常右侧滚轮显示对应方法*/
        /// <summary>
        /// 正常群聊右侧滚轮文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupShowScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbRight.AppendLine(divRightCopyContent(divid, msg.messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbRight.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(node);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                if (msg.flag == 1)
                {
                    sbRight.AppendLine(OnceSendHistoryMsgDiv("sendBurnText", msg.messageId, showMsg, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                }
                else
                {
                    sbRight.AppendLine(OnceSendHistoryMsgDiv("sendText", msg.messageId, showMsg, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                }
            }

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");

            //sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        public static void RightGroupShowAtScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers, string contects)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");



            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbRight.AppendLine(divRightCopyContent(divid, msg.messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string showMsg = contects;

            sbRight.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(node);");

            //是否失败
            if (msg.sendsucessorfail == 0)
            {
                //sbRight.AppendLine(PictureAndTextMixMethod.OnceSendMixPicDiv("sendAtMixPic", msg.MessageId, "", arg.RepeatId, arg.SendIngId, "", ""));
                sbRight.AppendLine(PictureAndTextMixMethod.OnceSendHistoryPicDiv("sendAtMixPic", msg.messageId, contects, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
            }
            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");

            //sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        public static void RightGroupShowAtext(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers, string contects)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");



            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //sbRight.AppendLine("var three=document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
            sbRight.AppendLine(divRightCopyContent(divid, msg.messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            sbRight.AppendLine("node.className='speech right';");

            string showMsg = contects;

            sbRight.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(node);");

            //获取body层
            //sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            //sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 正常群聊右侧滚轮图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupShowScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            //sbRight.AppendLine("var img1 = document.createElement('img');");
            string imageid = "wlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbRight.AppendLine(oneSentImageString(imageid, msg.messageId));

            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");
            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='" + imageid + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //发送失败
            if (msg.sendsucessorfail == 0)
            {
                if (msg.flag == 1)
                {
                    string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                    string imgLocalPath = pathHtml;
                    sbRight.AppendLine(OnceSendHistoryMsgDiv("2", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                }
                else
                {
                    string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                    string imgLocalPath = pathHtml;
                    sbRight.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                }
            }

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            //sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 正常群聊右侧滚轮文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupShowScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            receive.flag = msg.flag.ToString();
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            var txtShowSucessImg = "success";//接收上传状态图片
            string txtShowDown = "上传成功";//接收上传状态文字
            string txtOpenOrReceive = "打开";//打开接收文字
            string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
            //同一端发送
            if (isOpenOrReceive)
            {
                if (msg.sendsucessorfail == 1)//发送成功
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "上传成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//发送失败
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "fail";
                    txtShowDown = "上传失败";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
            }
            //不同端同步的消息
            else
            {
                if (isDownFile)//已经下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
                    txtShowSucessImg = "success";
                    txtShowDown = "下载成功";
                    txtOpenOrReceive = "打开";
                    txtSaveAs = "打开文件夹";
                }
                else//未下载
                {
                    sbFileUpload.AppendLine("sevenSecond.style.width='0%';");
                    txtShowSucessImg = "reveiving";
                    txtShowDown = "未下载";
                    txtOpenOrReceive = "接收";
                    txtSaveAs = "另存为";
                }
            }
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
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");
            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");
            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");
            sbFileUpload.AppendLine("eight.appendChild(night);");
            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
            sbFileUpload.AppendLine("night.appendChild(nightButton);");
            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");
            sbFileUpload.AppendLine("eight.appendChild(ten);");
            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");
            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");
            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");
            sbFileUpload.AppendLine("eight.appendChild(eleven);");
            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //发送失败提示
            if (msg.sendsucessorfail == 0)
            {
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                if (msg.flag == 1)
                {
                    sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("3", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                }
                else
                {
                    sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                }
            }
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 正常群聊右侧滚动文件
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupShowScrollFileDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            sbFileUpload.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");

            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");


            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");

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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");

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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.style.width='" + (msg.sendsucessorfail > 0 ? "100%" : "0%") + "';");

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
            sbFileUpload.AppendLine("showSOFImg.src='" + (msg.sendsucessorfail > 0 ? fileShowImage.showImageHtmlPath("success", "") : fileShowImage.showImageHtmlPath("fail", "")) + "';");
            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + showImgGuid + "';");

            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");

            sbFileUpload.AppendLine("eight.appendChild(night);");

            //接收中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            sbFileUpload.AppendLine("nightButton.id='" + fileshowText + "';");
            receive.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.innerHTML='" + (msg.sendsucessorfail > 0 ? "上传成功" : "上传失败") + "';");

            sbFileUpload.AppendLine("night.appendChild(nightButton);");



            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");

            sbFileUpload.AppendLine("eight.appendChild(ten);");

            //打开
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");

            sbFileUpload.AppendLine("btnten.id='" + fileOpenguid + "';");
            receive.fileOpenGuid = fileOpenguid;

            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");

            sbFileUpload.AppendLine("btnten.addEventListener('click',clickBtnCall);");

            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

            sbFileUpload.AppendLine("eight.appendChild(eleven);");

            //打开文件夹
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("btnEleven.value='" + msg.uploadOrDownPath.Replace(@"\", "/") + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            //赋上传之前的地址
            receive.haveDownFile = msg.uploadOrDownPath.Replace(@"\", "/");
            receive.messageId = msg.messageId;
            receive.localOrServerPath = msg.uploadOrDownPath.Replace(@"\", "/");
            sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //2017-07-23 添加 重发div
            if (msg.sendsucessorfail == 0)
            {
                string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                if (msg.flag == 1)
                {
                    sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("3", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                }
                else
                {
                    sbFileUpload.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                }
            }

            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbFileUpload.AppendLine("document.body.appendChild(first);");



            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        public static void RightGroupShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            var voiceFile = string.IsNullOrEmpty(receive.audioUrl) ? msg.uploadOrDownPath : receive.audioUrl;
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + voiceFile.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");
            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");
            //时长
            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            //重发
            if (msg.sendsucessorfail == 0)
            {
                sbRight.AppendLine(OnceSendHistoryMsgDiv("sendVoice", msg.messageId, voiceFile, Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
            }
            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            sbRight.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }

        /*------------------------------------------------群聊阅后即焚显示对应方法-------------------------------------------------*/
        /*正常右侧发送显示对应方法*/
        /// <summary>
        /// 群聊阅后即焚右侧侧发送文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupBurnShowSendText(ChromiumWebBrowser cef, string sendStr, KeyEventArgs e, string messageid, string imageTipId, string imageSendingId, DateTime dt, string msgStr)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + messageid + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");


            sbRight.AppendLine("three.innerHTML ='" + sendStr.Replace("\r\n", "<br/>") + "';");

            sbRight.AppendLine("first.appendChild(three);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendBurnText", messageid, sendStr, imageTipId, imageSendingId, msgStr, ""));

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            //if (e != null)
            //{
            //    e.Handled = true;
            //}
            #endregion
        }
        public static void RightGroupBurnShowSendAtText(ChromiumWebBrowser cef, string sendStr, KeyEventArgs e, string messageid, string imageTipId, string imageSendingId, DateTime dt, string msgStr)
        {
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + messageid + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");


            sbRight.AppendLine("three.innerHTML ='" + sendStr.Replace("\r\n", "<br/>") + "';");

            sbRight.AppendLine("first.appendChild(three);");
            //重发div
            sbRight.AppendLine(OnceSendMsgDiv("sendAtText", messageid, sendStr, imageTipId, imageSendingId, msgStr, ""));

            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            //if (e != null)
            //{
            //    e.Handled = true;
            //}
            #endregion
        }
        /// <summary>
        /// 群聊阅后即焚右侧发送图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="lists"></param>
        /// <param name="e"></param>
        /// <param name="richTextBox"></param>
        /// <param name="fileFileName"></param>
        public static void RightGroupBurnShowSendImage(ChromiumWebBrowser cef, Image lists, KeyEventArgs e,
            RichTextBox richTextBox, string fileFileName, string messageId, string imageTipId, string imageSendingId, DateTime dt)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            //2017-04-06添加
            sbRight.Append("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + dt.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");



            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}


            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");

            string imgLocalPath = lists.Source.ToString();
            sbRight.AppendLine("img1.src='" + imgLocalPath + "';");


            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");
            sbRight.AppendLine("img1.id='wlc" + fileFileName + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");

            //2017-04-06 重发div
            sbRight.AppendLine(OnceSendMsgDiv("2", messageId, imgLocalPath, imageTipId, imageSendingId, "", ""));

            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
            if (richTextBox != null)
            {
                richTextBox.Document.Blocks.Clear();
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 群聊阅后即焚右侧发送文件显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="listfile"></param>
        public static void RightGroupBurnShwoSendFile(ChromiumWebBrowser cef, UpLoadFilesDto listfile)
        {
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            //201-04-06 添加
            sbFileUpload.Append("first.id='" + listfile.messageId + "';");

            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + listfile.DtTime.ToString("HH:mm:ss") + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");

            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");



            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");


            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");

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
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(listfile.fileExtendName, listfile.localOrServerPath) + "';");

            sbFileUpload.AppendLine("four.appendChild(fileimage);");



            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + listfile.fileName + "';");
            string fileName = listfile.fileName.Length > 12 ? listfile.fileName.Substring(0, 10) + "..." : listfile.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");


            sbFileUpload.AppendLine("three.appendChild(five);");

            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(listfile.fileSize) + "';");
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
            //2017-04-06 提前
            //string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
            //listfile.progressId = progressId;
            sbFileUpload.AppendLine("sevenSecond.id='" + listfile.progressId + "';");
            //sbFileUpload.AppendLine("sevenSecond.style.width='30%';");

            sbFileUpload.AppendLine("sevenFist.appendChild(sevenSecond);");


            sbFileUpload.AppendLine("var eight=document.createElement('div');");
            sbFileUpload.AppendLine("eight.className='fileOperateDiv';");

            sbFileUpload.AppendLine("second.appendChild(eight);");

            sbFileUpload.AppendLine("var imgSorR=document.createElement('div');");
            sbFileUpload.AppendLine("imgSorR.className='fileRorSImage';");

            sbFileUpload.AppendLine("eight.appendChild(imgSorR);");

            //上传中图片添加
            sbFileUpload.AppendLine("var showSOFImg=document.createElement('img');");
            sbFileUpload.AppendLine("showSOFImg.className='onging';");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("onging", "") + "';");
            //2017-04-06 提前
            //string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
            //listfile.fileImgGuid = showImgGuid;
            sbFileUpload.AppendLine("showSOFImg.id='" + listfile.fileImgGuid + "';");

            sbFileUpload.AppendLine("imgSorR.appendChild(showSOFImg);");


            sbFileUpload.AppendLine("var night=document.createElement('div');");
            sbFileUpload.AppendLine("night.className='fileRorS';");

            sbFileUpload.AppendLine("eight.appendChild(night);");

            //上传中添加文字
            sbFileUpload.AppendLine("var nightButton=document.createElement('button');");
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
            //2017-04-06 提前
            //string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
            //listfile.fileTextGuid = fileshowText;
            sbFileUpload.AppendLine("nightButton.id='" + listfile.fileTextGuid + "';");

            sbFileUpload.AppendLine("nightButton.innerHTML='上传中';");

            sbFileUpload.AppendLine("night.appendChild(nightButton);");



            sbFileUpload.AppendLine("var ten=document.createElement('div');");
            sbFileUpload.AppendLine("ten.className='fileOpen';");

            sbFileUpload.AppendLine("eight.appendChild(ten);");

            //打开按钮添加
            sbFileUpload.AppendLine("var btnten=document.createElement('button');");
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
            //2017-04-06 提前
            //string fileOpenguid = "ql" + Guid.NewGuid().ToString().Replace(" - ", "");
            //listfile.fileOpenguid = fileOpenguid;
            sbFileUpload.AppendLine("btnten.id='" + listfile.fileOpenguid + "';");
            string localPath = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnten.value='" + localPath + "';");
            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
            sbFileUpload.AppendLine("ten.appendChild(btnten);");

            sbFileUpload.AppendLine("btnten.addEventListener('click',clickSendBtnCall);");

            sbFileUpload.AppendLine("var eleven=document.createElement('div');");
            sbFileUpload.AppendLine("eleven.className='fileOpenDirectory';");

            sbFileUpload.AppendLine("eight.appendChild(eleven);");

            //打开文件夹按钮添加
            sbFileUpload.AppendLine("var btnEleven=document.createElement('button');");
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            //2017-04-06 提前
            //string fileOpenDirectory = "od" + Guid.NewGuid().ToString().Replace(" - ", "");
            //listfile.fileOpenDirectory = fileOpenDirectory;
            sbFileUpload.AppendLine("btnEleven.id='" + listfile.fileOpenDirectory + "';");
            string localPathDirectory = listfile.localOrServerPath.Replace(@"\", @"/");
            sbFileUpload.AppendLine("btnEleven.value='" + localPathDirectory + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='打开文件夹';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickSendOpenBtnCall);");

            //2017-04-06 添加 重发div
            sbFileUpload.AppendLine(OnceSendMsgDiv("3", listfile.messageId, localPathDirectory, listfile.imageTipId, listfile.imageSendingId, "", ""));

            sbFileUpload.AppendLine("document.body.appendChild(first);");

            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
        }

        /*正常右侧滚轮显示对应方法*/
        /// <summary>
        /// 正常右侧阅后即焚文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupBurnShowText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");


            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");


            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbRight.AppendLine("three.innerHTML ='" + showMsg + "';");

            sbRight.AppendLine("first.appendChild(three);");


            sbRight.AppendLine("document.body.appendChild(first);}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());
        }
        /// <summary>
        /// 正常右侧阅后即焚图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupBurnShowImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.Append("first.id='" + msg.messageId + "';");
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");

            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
            //{
            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
            //}

            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");



            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");

            sbRight.AppendLine("var img1 = document.createElement('img');");


            sbRight.AppendLine("img1.src='" + rimgDto.picUrl + "';");

            sbRight.AppendLine("img1.style.width='100%';");
            sbRight.AppendLine("img1.style.height='100%';");

            sbRight.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbRight.AppendLine("img1.setAttribute('sid', '" + msg.chatIndex + "');");
            sbRight.AppendLine("img1.setAttribute('mid', 'M" + msg.messageId + "');");
            sbRight.AppendLine("img1.title='双击查看原图';");
            sbRight.AppendLine("three.appendChild(img1);");

            sbRight.AppendLine("first.appendChild(three);");
            sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
            sbRight.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbRight.ToString());

            StringBuilder sbEnds = new StringBuilder();
            sbEnds.AppendLine("setscross();");
            cef.EvaluateScriptAsync(sbEnds.ToString());
        }
        /// <summary>
        /// 正常右侧阅后即焚文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        public static void RightGroupBurnShowFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            #region 构造发送文件解析
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.seId = msg.sessionId;
            receive.flag = "1";
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='rightd';");
            //时间显示层
            sbFileUpload.AppendLine("var timeDiv=document.createElement('div');");
            sbFileUpload.AppendLine("timeDiv.className='rightTimeStyle';");
            sbFileUpload.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeDiv);");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='rightimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            sbFileUpload.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbFileUpload.AppendLine("img.className='divcss5';");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            sbFileUpload.AppendLine("var bubbleDiv=document.createElement('div');");
            sbFileUpload.AppendLine("bubbleDiv.className='speech right';");
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
            }
            sbFileUpload.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
            sbFileUpload.AppendLine("four.appendChild(fileimage);");
            sbFileUpload.AppendLine("var five=document.createElement('div');");
            sbFileUpload.AppendLine("five.className='fileName';");
            sbFileUpload.AppendLine("five.title='" + receive.fileName + "';");
            string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
            sbFileUpload.AppendLine("five.innerText='" + fileName + "';");
            sbFileUpload.AppendLine("three.appendChild(five);");
            sbFileUpload.AppendLine("var six=document.createElement('div');");
            sbFileUpload.AppendLine("six.className='fileSize';");
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("nightButton.className='fileUploadProgress';");
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
            sbFileUpload.AppendLine("btnten.className='btnOpenFile';");
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
            sbFileUpload.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
            string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
            receive.fileDirectoryGuid = fileDirectoryId;
            sbFileUpload.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            var setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        public static void RightGroupBurnShowVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            #region 圆形图片Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            sbRight.AppendLine("var three=document.createElement('div');");
            sbRight.AppendLine("three.className='speech right';");
            sbRight.AppendLine("three.id='M" + msg.messageId + "';");
            sbRight.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbRight.AppendLine("var imgmp3 = document.createElement('img');");
            sbRight.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbRight.AppendLine("imgmp3.src='" + musicImg + "';");
            sbRight.AppendLine("imgmp3.className ='divmp3_img_right';");
            sbRight.AppendLine("three.appendChild(imgmp3);");

            sbRight.AppendLine("var div3 = document.createElement('div');");
            sbRight.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbRight.AppendLine("div3.className ='divmp3_contain_right';");
            sbRight.AppendLine("three.appendChild(div3);");
            sbRight.AppendLine("first.appendChild(three);");
            sbRight.AppendLine("document.body.appendChild(first);}");
            sbRight.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }

        /*群聊阅后即焚滚轮显示对应方法*/
        /// <summary>
        /// 群聊阅后即焚滚轮左侧文本显示方法 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowScrollText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
    List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.Append("nodeFirst.id='" + msg.messageId + "';");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");

            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 群聊阅后即焚滚轮左侧图片显示方法 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowScrollImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.Append("nodeFirst.id='" + msg.messageId + "';");

            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeftImage.AppendLine(groupImageString(userid));
            sbLeftImage.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + userid + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");


            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            sbLeftImage.AppendLine("var img = document.createElement('img');");

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeftImage.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeftImage.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);");

            //sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            //sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());

        }
        /// <summary>
        /// 群聊阅后即焚滚轮左文件显示方法 File
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowScrollFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
    List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            //判断是否已经下载
            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.seId = msg.sessionId;
            receive.flag = "1";
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            if (isDownFile == true)
            {
                sbFileUpload.AppendLine("sevenSecond.style.width='100%';");
            }
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
            string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
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
            string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
            sbFileUpload.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
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
            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
            sbFileUpload.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
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
            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
            sbFileUpload.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 群聊阅后即焚滚轮左侧文件下载完成方法 fileDown
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowScrollFileDown(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
    List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");

            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");


            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");


            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");

            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");

            ////添加用户名称
            //sbFileUpload.AppendLine("var username=document.createElement('div');");
            //sbFileUpload.AppendLine("username.className='leftUsername';");
            //sbFileUpload.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbFileUpload.AppendLine("first.appendChild(username);");

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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("sevenSecond.style.width='100%';");

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
            sbFileUpload.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath("success", "") + "';");
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
            sbFileUpload.AppendLine("nightButton.innerHTML='下载完成';");

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

            sbFileUpload.AppendLine("btnten.innerHTML='打开';");
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
            sbFileUpload.AppendLine("btnEleven.value='" + msg.uploadOrDownPath.Replace(@"\", "/") + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");

            //下载完成更新路径
            receive.chatIndex = msg.chatIndex;

            sbFileUpload.AppendLine("btnten.value='" + JsonConvert.SerializeObject(receive) + "';");

            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");

            //获取body层
            sbFileUpload.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbFileUpload.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
            //sbFileUpload.AppendLine("document.body.appendChild(first);");



            sbFileUpload.AppendLine("}");

            sbFileUpload.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        /// 群聊阅后即焚滚轮左侧语音显示方法 voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowScrollVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
    List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().ToString().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().ToString().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            ////添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");
            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            if (msg.voiceread + "" != "1")
            {
                //未读圆形提示
                string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
                sbLeft.AppendLine("var divCircle = document.createElement('div');");
                sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                sbLeft.AppendLine("divCircle.className ='leftVoice';");
                sbLeft.AppendLine("var imgCircle = document.createElement('img');");
                sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
                sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
                sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");
            }
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");
            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.ExecuteScriptAsync(sbLeft.ToString());

        }

        /*群聊阅后即焚正常显示对应方法*/
        /// <summary>
        /// 正常群聊阅后即焚左侧文本显示 Text
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void leftGroupBurnShowText(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);


            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");


            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            sbLeft.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + userid + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");

            //添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");

            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

            sbLeft.AppendLine("node.innerHTML ='" + showMsg + "';");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeft.ToString());
        }
        /// <summary>
        /// 正常群聊阅后即焚左侧图片显示 image
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowImage(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            StringBuilder sbLeftImage = new StringBuilder();
            sbLeftImage.AppendLine("function myFunction()");
            sbLeftImage.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeftImage.AppendLine("nodeFirst.className='leftd';");
            sbLeftImage.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            sbLeftImage.AppendLine("var second=document.createElement('div');");
            sbLeftImage.AppendLine("second.className='leftimg';");


            sbLeftImage.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbLeftImage.AppendLine(groupImageString(userid));
            sbLeftImage.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeftImage.AppendLine("img.className='divcss5Left';");
            sbLeftImage.AppendLine("img.id='" + userid + "';");
            sbLeftImage.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeftImage.AppendLine("second.appendChild(img);");
            sbLeftImage.AppendLine("nodeFirst.appendChild(second);");

            //时间显示
            sbLeftImage.AppendLine("var timeshow = document.createElement('div');");
            sbLeftImage.AppendLine("timeshow.className='leftTimeText';");
            sbLeftImage.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeftImage.AppendLine("nodeFirst.appendChild(timeshow);");


            ////添加用户名称
            //sbLeftImage.AppendLine("var username=document.createElement('div');");
            //sbLeftImage.AppendLine("username.className='leftUsername';");
            //sbLeftImage.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeftImage.AppendLine("nodeFirst.appendChild(username);");

            sbLeftImage.AppendLine("var node=document.createElement('div');");
            sbLeftImage.AppendLine("node.className='speech left';");

            //sbLeftImage.AppendLine("var img = document.createElement('img');");
            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
            sbLeftImage.AppendLine(oneImageString(imageid));

            sbLeftImage.AppendLine("img.style.width='100%';");
            sbLeftImage.AppendLine("img.style.height='100%';");
            sbLeftImage.AppendLine("img.src='" + rimgDto.picUrl + "';");
            sbLeftImage.AppendLine("img.id='" + imageid + "';");
            sbLeftImage.AppendLine("img.setAttribute('sid', '" + msg.chatIndex + "');");
            sbLeftImage.AppendLine("img.title='双击查看原图';");

            sbLeftImage.AppendLine("node.appendChild(img);");

            sbLeftImage.AppendLine("nodeFirst.appendChild(node);");

            sbLeftImage.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeftImage.AppendLine("img.addEventListener('dblclick',clickImgCall);");
            if (msg.IsSetImgLoadComplete)
            {
                sbLeftImage.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
            }

            sbLeftImage.AppendLine("}");

            sbLeftImage.AppendLine("myFunction();");

            cef.EvaluateScriptAsync(sbLeftImage.ToString());
        }
        /// <summary>
        /// 正常群聊阅后即焚左侧文件显示 file
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowFile(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);
            receive.messageId = msg.messageId;
            receive.chatIndex = msg.chatIndex;
            receive.flag = "1";
            #region 构造发送文件解析
            StringBuilder sbFileUpload = new StringBuilder();
            sbFileUpload.AppendLine("function myFunction()");
            sbFileUpload.AppendLine("{ var first=document.createElement('div');");
            sbFileUpload.AppendLine("first.className='leftd';");
            sbFileUpload.AppendLine("var seconds=document.createElement('div');");
            sbFileUpload.AppendLine("seconds.className='leftimg';");
            sbFileUpload.AppendLine("var img = document.createElement('img');");
            string userid = NewUserIdString(user.userId);
            sbFileUpload.AppendLine(groupImageString(userid));
            sbFileUpload.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbFileUpload.AppendLine("img.className='divcss5Left';");
            sbFileUpload.AppendLine("img.id='" + userid + "';");
            sbFileUpload.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbFileUpload.AppendLine("seconds.appendChild(img);");
            sbFileUpload.AppendLine("first.appendChild(seconds);");
            //时间显示
            sbFileUpload.AppendLine("var timeshow = document.createElement('div');");
            sbFileUpload.AppendLine("timeshow.className='leftTimeText';");
            sbFileUpload.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbFileUpload.AppendLine("first.appendChild(timeshow);");
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
            sbFileUpload.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
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
            sbFileUpload.AppendLine("btnEleven.innerHTML='另存为';");
            string setValues = JsonConvert.SerializeObject(receive);
            sbFileUpload.AppendLine("btnEleven.value='" + setValues + "';");
            sbFileUpload.AppendLine("eleven.appendChild(btnEleven);");
            sbFileUpload.AppendLine("btnten.value='" + setValues + "';");
            sbFileUpload.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
            sbFileUpload.AppendLine("document.body.appendChild(first);");
            sbFileUpload.AppendLine("}");
            sbFileUpload.AppendLine("myFunction();");
            cef.EvaluateScriptAsync(sbFileUpload.ToString());
            #endregion
        }
        /// <summary>
        ///  正常群聊阅后即焚左侧语音显示 voice
        /// </summary>
        /// <param name="cef"></param>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        public static void LeftGroupBurnShowVoice(ChromiumWebBrowser cef, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
            AntSdkGroupMember user = getGroupMembersUser(GroupMembers, msg);

            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");
            string isShowReadImgId = "v" + Guid.NewGuid().ToString().GetHashCode();
            string isShowGifId = "g" + Guid.NewGuid().ToString().GetHashCode();
            //sbLeft.AppendLine("nodeFirst.setAttribute('onmousedown','VoiceRightMenuMethod(event)');nodeFirst.setAttribute('selfmethods','one');nodeFirst.setAttribute('audioUrl','" + receive.audioUrl + "');nodeFirst.setAttribute('isRead','0');nodeFirst.setAttribute('isShowImg','" + isShowReadImgId + "');nodeFirst.setAttribute('isShowGif','" + isShowGifId + "');");

            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            string userid = NewUserIdString(user.userId);
            sbLeft.AppendLine(groupImageString(userid));
            sbLeft.AppendLine("img.src='" + defaultHeaderImage + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");


            ////添加用户名称
            //sbLeft.AppendLine("var username=document.createElement('div');");
            //sbLeft.AppendLine("username.className='leftUsername';");
            //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
            //sbLeft.AppendLine("nodeFirst.appendChild(username);");

            sbLeft.AppendLine("var node=document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            sbLeft.AppendLine("node.id='M" + msg.messageId + "';");
            sbLeft.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

            string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var imgmp3 = document.createElement('img');");
            sbLeft.AppendLine("imgmp3.id ='" + isShowGifId + "';");
            sbLeft.AppendLine("imgmp3.src='" + musicImg + "';");
            sbLeft.AppendLine("imgmp3.className ='divmp3_img';");
            sbLeft.AppendLine("node.appendChild(imgmp3);");

            sbLeft.AppendLine("var div3 = document.createElement('div');");
            sbLeft.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
            sbLeft.AppendLine("div3.className ='divmp3_contain';");
            sbLeft.AppendLine("node.appendChild(div3);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");


            //未读圆形提示
            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
            sbLeft.AppendLine("var divCircle = document.createElement('div');");
            sbLeft.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
            sbLeft.AppendLine("divCircle.className ='leftVoice';");
            sbLeft.AppendLine("var imgCircle = document.createElement('img');");
            sbLeft.AppendLine("imgCircle.src='" + CirCleImg + "';");
            sbLeft.AppendLine("divCircle.appendChild(imgCircle);");
            sbLeft.AppendLine("nodeFirst.appendChild(divCircle);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);}");

            sbLeft.AppendLine("myFunction();");

            var task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }
        private static Paragraph pTalk;
        /// <summary>
        /// 插入@消息
        /// </summary>
        /// <param name="richText"></param>
        /// <param name="atName"></param>
        /// <param name="id"></param>
        public static void InsertAtBlock(RichTextBoxEx richText, string atName, string id)
        {
            //pTalk = null;
            //if (richText.CaretPosition.Paragraph != null)
            //{
            //    pTalk=richText.CaretPosition.Paragraph;
            //}
            //else
            //{
            //    pTalk= new Paragraph();
            //}
            atName = "@" + atName;
            TextBlock tbLeft = new TextBlock() { Text = atName, FontSize = 12, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006EFE")) };
            tbLeft.Tag = id;
            TextPointer point = richText.Selection.Start;
            InlineUIContainer uiContainer = new InlineUIContainer(tbLeft, point);
            TextPointer nextPoint = uiContainer.ContentEnd;
            richText.CaretPosition = nextPoint;
            richText.CaretPosition.InsertTextInRun(" ");//加入空格
            richText.Focus();
            //pTalk.Inlines.Add(tbLeft);
            //richText.Document.Blocks.Add(pTalk);
            //TextPointer pointer = richText.Document.ContentEnd;
            //if (pointer != null)
            //{
            //    richText.CaretPosition = pointer;
            //}
            //richText.CaretPosition.InsertTextInRun(" ");//加入空格
            //TextPointer pp = richText.CaretPosition.GetPositionAtOffset(1);
            //if (pp != null)
            //{
            //    richText.CaretPosition = pp;
            //}
        }
        /// <summary>
        /// 保存图片方法
        /// </summary>
        /// <param name="filePath"></param>
        public static bool SavePicture(string filePath)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            sfd.Filter = "JPG Files(*.jpg)|*.jpg|BMP Files (*.bmp)|*.bmp|PNG Files(*.png)|*.png|GIF Files(*.gif)|*.gif";
            var extension = Path.GetExtension(filePath);
            switch (extension)
            {
                case ".jpg":
                    sfd.FilterIndex = 1;
                    break;
                case ".bmp":
                    sfd.FilterIndex = 2;
                    break;
                case ".png":
                    sfd.FilterIndex = 3;
                    break;
                case ".gif":
                    sfd.FilterIndex = 4;
                    break;
            }
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (filePath.StartsWith("http:"))
                {
                    string strFilePath = sfd.FileName;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(new Uri(filePath, UriKind.RelativeOrAbsolute)));
                    FileStream fileStream = new FileStream(strFilePath, FileMode.Create, FileAccess.ReadWrite);
                    encoder.Save(fileStream);
                    fileStream.Close();
                    return true;
                }
                else
                {
                    string strFilePath = sfd.FileName;
                    System.Drawing.Bitmap bit = new System.Drawing.Bitmap(filePath);
                    bit.Save(strFilePath);
                    return true;
                    //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    //encoder.Frames.Add(BitmapFrame.c)
                    //FileStream fileStream = new FileStream(strFilePath, FileMode.Create, FileAccess.ReadWrite);
                    //encoder.Save(fileStream);
                    //fileStream.Close();
                    //return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 讨论组img自定义
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static string groupImageString(string userid)
        {
            string groupStr =
                "var img = document.createElement('img');img.setAttribute('onmousedown','rightMenuMethod(event)');img.setAttribute('userId','" +
                userid + "');img.setAttribute('selfmethods','group');";
            return groupStr;
        }
        /// <summary>
        /// 单聊img自定义
        /// </summary>
        /// <param name="imageid"></param>
        /// <returns></returns>
        public static string oneImageString(string imageid)
        {
            string oneStr =
                "var img = document.createElement('img');img.setAttribute('onmousedown','imageRightMenuMethod(event)');img.setAttribute('imageId','" +
                imageid + "');img.setAttribute('selfmethods','one');";
            return oneStr;
        }
        /// <summary>
        /// 单聊img左侧图片自定义菜单
        /// </summary>
        /// <param name="imageid"></param>
        /// <returns></returns>
        public static string oneLeftImageString(string imageid)
        {
            string oneStr =
                "var img = document.createElement('img');img.setAttribute('onmousedown','imageLeftMenuMethod(event)');img.setAttribute('imageId','" +
                imageid + "');img.setAttribute('selfmethods','one');";
            return oneStr;
        }
        /// <summary>
        /// 机器人图片右键
        /// </summary>
        /// <param name="imageid"></param>
        /// <returns></returns>
        public static string RobitImageString(string imageid)
        {
            string oneStr =
               "var img1 = document.createElement('img');img1.setAttribute('onmousedown','imageLeftMenuMethod(event)');img1.setAttribute('imageId','" +
               imageid + "');img1.setAttribute('selfmethods','one');";
            return oneStr;
        }
        /// <summary>
        /// 发送图片右键菜单
        /// </summary>
        /// <param name="imageid"></param>
        /// <returns></returns>
        public static string oneSentImageString(string imageid, string messageId)
        {
            string oneStr =
                "var img1 = document.createElement('img');img1.setAttribute('onmousedown','imageRightMenuMethod(event)');img1.setAttribute('imageId','" +
                imageid + "');img1.setAttribute('selfmethods','one');img1.setAttribute('imgMessageId','" + messageId + "');";
            return oneStr;
        }
        public static string oneSentFile(string messageId)
        {
            string oneStr =
                "first.setAttribute('onmousedown','fileRightMenuMethod(" + messageId + ")');first.setAttribute('selfmethods','one');first.setAttribute('imgMessageId','" + messageId + "');";
            return oneStr;
        }
        /// <summary>
        /// 复制div内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string divLeftCopyContent(string id)
        {
            string oneStr =
              "var node = document.createElement('div');node.setAttribute('onmousedown','copyContent(event)');node.setAttribute('copyid','" +
              id + "');node.setAttribute('selfmethods','copy');node.setAttribute('leftstyle','0');";
            return oneStr;
        }
        /// <summary>
        /// 复制内容、撤销
        /// </summary>
        /// <param name="id"></param>
        /// <param name="messageid"></param>
        /// <returns></returns>
        public static string divRightCopyContent(string id, string messageid)
        {
            string oneStr =
              "var node = document.createElement('div');node.setAttribute('onmousedown','copyContent(event)');node.setAttribute('copyid','" +
              id + "');node.setAttribute('selfmethods','copy');node.setAttribute('recallid','" + messageid + "');";
            return oneStr;
        }
        /// <summary>
        /// html格式化
        /// </summary>
        /// <param name="htmlFragment"></param>
        public static void CopyHtmlToClipBoard(string htmlFragment)
        {
            string headerFormat
              = "Version:0.9\r\nStartHTML:{0:000000}\r\nEndHTML:{1:000000}"
              + "\r\nStartFragment:{2:000000}\r\nEndFragment:{3:000000}\r\n";

            string htmlHeader
              = "<html>\r\n<head>\r\n"
              + "<meta http-equiv=\"Content-Type\""
              + " content=\"Text/html; charset=utf-8\">\r\n"
              + "<title>HTML clipboard</title>\r\n</head>\r\n<body>\r\n"
              + "<!--StartFragment-->";

            string htmlFooter = "<!--EndFragment-->\r\n</body>\r\n</html>\r\n";
            string headerSample = String.Format(headerFormat, 0, 0, 0, 0);

            Encoding encoding = Encoding.UTF8;
            int headerSize = encoding.GetByteCount(headerSample);
            int htmlHeaderSize = encoding.GetByteCount(htmlHeader);
            int htmlFragmentSize = encoding.GetByteCount(htmlFragment);
            int htmlFooterSize = encoding.GetByteCount(htmlFooter);

            string htmlResult
              = String.Format(
                  CultureInfo.InvariantCulture,
                  headerFormat,
                  /* StartHTML     */ headerSize,
                  /* EndHTML       */ headerSize + htmlHeaderSize + htmlFragmentSize + htmlFooterSize,
                  /* StartFragment */ headerSize + htmlHeaderSize,
                  /* EndFragment   */ headerSize + htmlHeaderSize + htmlFragmentSize)
              + htmlHeader
              + htmlFragment
              + htmlFooter;
            DataObject obj = new DataObject();
            obj.SetData(DataFormats.Html, new MemoryStream(encoding.GetBytes(htmlResult)));
            Clipboard.SetDataObject(obj, true);
        }
        /// <summary>
        /// 剪切板写内容
        /// </summary>
        /// <param name="content"></param>
        public static void SetDataToClipBoard(string content)
        {
            //MemoryStream vMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            try
            {
                System.Windows.Forms.Clipboard.SetData(DataFormats.UnicodeText, content);
            }
            catch (Exception)
            {
                System.Windows.Forms.Clipboard.SetData(DataFormats.UnicodeText, content);
            }

        }
        public static string ScrollOnceSendMsgDiv(string messageid)
        {
            string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
            string imageSendingId = "sending" + imageTipId;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var sendonce=document.createElement('div');");
            sb.AppendLine("sendonce.className='onceSend';");
            //发送过程img
            sb.AppendLine("var imgsending = document.createElement('img');");
            sb.AppendLine("imgsending.className='scrollSendingImg';");
            sb.AppendLine("imgsending.id='" + imageSendingId + "';");
            sb.AppendLine("imgsending.src='../images/loading.gif';");
            sb.AppendLine("imgsending.title='发送中';");
            sb.AppendLine("sendonce.appendChild(imgsending);");
            //重复img
            sb.AppendLine("var imgtip = document.createElement('img');imgtip.setAttribute('failId','" +
              messageid + "');imgtip.setAttribute('methods','sendText');");
            sb.AppendLine("imgtip.id='" + imageTipId + "';");
            sb.AppendLine("imgtip.addEventListener('click',clickfailshow);");
            sb.AppendLine("imgtip.className='onceSendFail';");
            sb.AppendLine("imgtip.src='../images/发送失败.png';");
            sb.AppendLine("imgtip.title='点击重新发送';");
            sb.AppendLine("sendonce.appendChild(imgtip);");
            sb.AppendLine("first.appendChild(sendonce);");
            return sb.ToString();
        }
        /// <summary>
        /// 发送过程div和重新发送div
        /// </summary>
        public static string OnceSendMsgDiv(string method, string messageid, string path, string imageTipId, string imageSendingId, string preValue, string atPreValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var sendonce=document.createElement('div');");
            sb.AppendLine("sendonce.className='onceSend';");
            //发送过程img
            sb.AppendLine("var imgsending = document.createElement('img');");
            sb.AppendLine("imgsending.className='onceSendingImg';");
            sb.AppendLine("imgsending.id='" + imageSendingId + "';");
            sb.AppendLine("imgsending.src='../images/loading.gif';");
            sb.AppendLine("imgsending.title='发送中';");
            sb.AppendLine("sendonce.appendChild(imgsending);");
            //重复img
            sb.AppendLine("var imgtip = document.createElement('img');imgtip.setAttribute('failId','" +
              messageid + "');imgtip.setAttribute('filepaths','" + path.Replace("\r\n", "<br/>").Replace("\n", "<br/>") + "');imgtip.setAttribute('methods','" + method + "');imgtip.setAttribute('preValue','" + preValue.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');imgtip.setAttribute('atPreValues','" + atPreValues + "');");
            sb.AppendLine("imgtip.id='" + imageTipId + "';");
            sb.AppendLine("imgtip.addEventListener('click',clickfailshow);");
            sb.AppendLine("imgtip.className='onceSendImg';");
            sb.AppendLine("imgtip.src='../images/发送失败.png';");
            sb.AppendLine("imgtip.title='点击重新发送';");
            sb.AppendLine("sendonce.appendChild(imgtip);");
            sb.AppendLine("first.appendChild(sendonce);");
            return sb.ToString();
        }
        public static string OnceSendHistoryMsgDiv(string method, string messageid, string path, string imageTipId, string imageSendingId, string preValue, string atPreValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var sendonce=document.createElement('div');");
            sb.AppendLine("sendonce.className='onceSendFail';");
            ////发送过程img
            //sb.AppendLine("var imgsending = document.createElement('img');");
            //sb.AppendLine("imgsending.className='onceSendingImg';");
            //sb.AppendLine("imgsending.id='" + imageSendingId + "';");
            //sb.AppendLine("imgsending.src='../images/loading.gif';");
            //sb.AppendLine("imgsending.title='发送中';");
            //sb.AppendLine("sendonce.appendChild(imgsending);");
            //重复img
            sb.AppendLine("var imgtip = document.createElement('img');imgtip.setAttribute('failId','" +
              messageid + "');imgtip.setAttribute('filepaths','" + path.Replace("\r\n", "<br/>").Replace("\n", "<br/>") + "');imgtip.setAttribute('methods','" + method + "');imgtip.setAttribute('preValue','" + preValue.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');imgtip.setAttribute('atPreValues','" + atPreValues + "');");
            sb.AppendLine("imgtip.id='" + imageTipId + "';");
            sb.AppendLine("imgtip.addEventListener('click',clickfailshow);");
            sb.AppendLine("imgtip.className='onceSendImg';");
            sb.AppendLine("imgtip.src='../images/发送失败.png';");
            sb.AppendLine("imgtip.title='点击重新发送';");
            sb.AppendLine("sendonce.appendChild(imgtip);");
            sb.AppendLine("first.appendChild(sendonce);");
            return sb.ToString();
        }
        public static string OnceSendAtHistoryMsgDiv(string method, string messageid, string path, string imageTipId, string imageSendingId, string preValue, string atPreValues)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var sendonce=document.createElement('div');");
            sb.AppendLine("sendonce.className='onceSendFail';");
            ////发送过程img
            //sb.AppendLine("var imgsending = document.createElement('img');");
            //sb.AppendLine("imgsending.className='onceSendingImg';");
            //sb.AppendLine("imgsending.id='" + imageSendingId + "';");
            //sb.AppendLine("imgsending.src='../images/loading.gif';");
            //sb.AppendLine("imgsending.title='发送中';");
            //sb.AppendLine("sendonce.appendChild(imgsending);");
            //重复img
            sb.AppendLine("var imgtip = document.createElement('img');imgtip.setAttribute('failId','" +
              messageid + "');imgtip.setAttribute('filepaths','" + path.Replace("\r\n", "<br/>").Replace("\n", "<br/>") + "');imgtip.setAttribute('methods','" + method + "');imgtip.setAttribute('preValue','" + preValue.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');imgtip.setAttribute('atPreValues','" + atPreValues + "');");
            sb.AppendLine("imgtip.id='" + imageTipId + "';");
            sb.AppendLine("imgtip.addEventListener('click',removeDivs);");
            sb.AppendLine("imgtip.className='onceSendImg';");
            sb.AppendLine("imgtip.src='../images/发送失败.png';");
            sb.AppendLine("imgtip.title='点击重新发送';");
            sb.AppendLine("sendonce.appendChild(imgtip);");
            sb.AppendLine("first.appendChild(sendonce);");
            return sb.ToString();
        }
        /// <summary>
        /// 消息构造开头
        /// </summary>
        /// <returns></returns>
        public static string topString()
        {
            string topStr = "function myFunction() {";
            return topStr;
        }
        /// <summary>
        /// 消息构造结尾
        /// </summary>
        /// <returns></returns>
        public static string endString()
        {
            string endStr = "} myFunction(); ";
            return endStr;
        }
        /// <summary>
        /// 个人消息批量解析
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string LeftOneToOneShowMessage(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg,
            AntSdkContact_User user, List<string> ImgId = null)
        {
            StringBuilder sbLRC = new StringBuilder();

            #region 获取接收者头像 New

            string pathImage = "";
            //获取接收者头像
            var listUser =
                GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
            if (listUser == null)
            {
                AntSdkContact_User cus =
                    AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
                if (cus == null)
                {
                    user = new AntSdkContact_User();
                    pathImage = "file:///" +
                                (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/")
                                    .Replace(" ", "%20");
                    user.userId = msg.sendUserId;
                    user.userName = "离职人员";
                }
                else
                {
                    if (string.IsNullOrEmpty(cus.picture + ""))
                    {
                        pathImage = "file:///" +
                                    (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/")
                                        .Replace(" ", "%20");
                    }
                    else
                    {
                        pathImage = cus.picture;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(listUser.Url + ""))
                {
                    pathImage = "file:///" +
                                (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/")
                                    .Replace(" ", "%20");
                }
                else
                {
                    pathImage = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
                }
            }

            #endregion

            switch (Convert.ToInt32( /*TODO:AntSdk_Modify:msg.MTP*/msg.MsgType))
            {
                case (int)AntSdkMsgType.ChatMsgText:

                    #region 文本消息显示

                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        if (msg.flag == 1)
                        {
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");

                            //头像显示层
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");
                            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            //{
                            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //}
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            //内容显示层
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");
                            string showMsg = PublicTalkMothed.talkContentReplace(msg.sourceContent);
                            sbLRC.AppendLine("three.innerHTML ='" + showMsg + "';");
                            sbLRC.AppendLine("first.appendChild(three);");

                            //阅后即焚图片显示外层
                            sbLRC.AppendLine("var imageOutDiv=document.createElement('div');");
                            sbLRC.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


                            //阅后即焚图片显示内层
                            sbLRC.AppendLine("var imageInDiv=document.createElement('img');");
                            sbLRC.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("imageOutDiv.appendChild(imageInDiv);");
                            sbLRC.AppendLine("first.appendChild(imageOutDiv);");

                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendBurnText", msg.messageId, showMsg,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                            }

                            sbLRC.AppendLine("document.body.appendChild(first);");
                        }
                        else
                        {
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");


                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");


                            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            //{
                            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //}
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            //sbRight.AppendLine("var three=document.createElement('div');");
                            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;

                            if (msg.IsRobot == true)
                            {
                                sbLRC.AppendLine(divLeftCopyContent(divid));
                            }
                            else
                            {
                                sbLRC.AppendLine(divRightCopyContent(divid, msg.messageId));
                            }
                            sbLRC.AppendLine("node.id='" + divid + "';");
                            sbLRC.AppendLine("node.className='speech right';");

                            string showMsg = PublicTalkMothed.talkContentReplace(msg.sourceContent);

                            sbLRC.AppendLine("node.innerHTML ='" + showMsg + "';");

                            sbLRC.AppendLine("first.appendChild(node);");
                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendText", msg.messageId, showMsg,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                            }
                            sbLRC.AppendLine("document.body.appendChild(first);");
                        }
                    }
                    else
                    {
                        if (msg.flag == 1)
                        {
                            #region 阅后即焚分支

                            //生成新的层ID
                            DivIdGather divId = getDivID(msg.chatIndex);

                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            //sbLeft.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");

                            //头像显示
                            sbLRC.AppendLine("var imgHead = document.createElement('img');");
                            sbLRC.AppendLine("imgHead.src='" + pathImage + "';");
                            sbLRC.AppendLine("imgHead.className='divcss5Left';");
                            sbLRC.AppendLine("second.appendChild(imgHead);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");

                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            //消息显示
                            sbLRC.AppendLine("var node=document.createElement('div');");
                            sbLRC.AppendLine("node.className='speech left';");

                            //单击显示外层
                            sbLRC.AppendLine(
                                "var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" +
                                divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" +
                                divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId +
                                "');outdiv.setAttribute('time','" + 15 + "');outdiv.setAttribute('hideMessageId','" +
                                msg.messageId + "');");
                            sbLRC.AppendLine("outdiv.id='" + divId.guid + "';");
                            sbLRC.AppendLine("outdiv.addEventListener('click',clickshow);");
                            sbLRC.AppendLine("node.appendChild(outdiv);");

                            //内容显示层
                            sbLRC.AppendLine("var contentDiv=document.createElement('div');");
                            sbLRC.AppendLine("contentDiv.id='" + divId.hideContentId + "';");
                            string showMsg =
                                PublicTalkMothed.talkContentReplace( /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                            sbLRC.AppendLine("contentDiv.innerHTML ='" + showMsg + "';");
                            sbLRC.AppendLine("contentDiv.className='contentHidden';");
                            //sbLeft.AppendLine("contentDiv.style.visibility='hidden;';");
                            sbLRC.AppendLine("outdiv.appendChild(contentDiv);");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");


                            //阅后即焚和倒计时层
                            sbLRC.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                            sbLRC.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            //阅后即焚显示图片层
                            sbLRC.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                            sbLRC.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                            sbLRC.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


                            //阅后即焚倒计时层
                            sbLRC.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                            sbLRC.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                            sbLRC.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                            sbLRC.AppendLine("outAfterTimeSpan.hidden='hidden';");
                            sbLRC.AppendLine("outAfterTimeSpan.innerHTML='" +
                                             getSeconds(selectType.text, /*TODO:AntSdk_Modify:msg.content*/
                                                 msg.sourceContent.Length) + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");

                            #endregion
                        }
                        else
                        {
                            #region 正常消息分支

                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");


                            sbLRC.AppendLine("var img = document.createElement('img');");


                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("img.id='" + user.userId + "';");
                            sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");

                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            //sbLeft.AppendLine("var node=document.createElement('div');");
                            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
                            sbLRC.AppendLine(divLeftCopyContent(divid));
                            sbLRC.AppendLine("node.id='" + divid + "';");
                            sbLRC.AppendLine("node.className='speech left';");


                            string showMsg =
                                PublicTalkMothed.talkContentReplace( /*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);


                            sbLRC.AppendLine("node.innerHTML ='" + showMsg + "';");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");

                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");

                            #endregion
                        }
                    }

                    #endregion

                    break;
                case (int)AntSdkMsgType.ChatMsgPicture:

                    #region 图片消息

                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        if (msg.flag == 1)
                        {
                            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(msg.sourceContent);
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");


                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");

                            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            //{
                            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //}

                            //头像显示层
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");

                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");


                            //图片显示层
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");

                            sbLRC.AppendLine("var img1 = document.createElement('img');");

                            sbLRC.AppendLine("img1.src='" + rimgDto.picUrl + "';");
                            sbLRC.AppendLine("img1.style.width='100%';");
                            sbLRC.AppendLine("img1.style.height='100%';");
                            sbLRC.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                            sbLRC.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
                            sbLRC.AppendLine("img1.title='双击查看原图';");
                            sbLRC.AppendLine("three.appendChild(img1);");

                            sbLRC.AppendLine("first.appendChild(three);");

                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                                string imgLocalPath = "file:///" + pathHtml;
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                            }

                            //阅后即焚图片显示外层
                            sbLRC.AppendLine("var imageOutDiv=document.createElement('div');");
                            sbLRC.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


                            //阅后即焚图片显示内层
                            sbLRC.AppendLine("var imageInDiv=document.createElement('img');");
                            sbLRC.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("imageOutDiv.appendChild(imageInDiv);");
                            sbLRC.AppendLine("first.appendChild(imageOutDiv);");

                            sbLRC.AppendLine("document.body.appendChild(first);");

                            sbLRC.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                            sbLRC.AppendLine(
                                "img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                        }
                        else
                        {
                            SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(msg.sourceContent);
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");


                            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            //{
                            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //}

                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");



                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");

                            //sbRight.AppendLine("var img1 = document.createElement('img');");
                            string imageid = "wlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
                            if (msg.IsRobot)
                            {
                                sbLRC.AppendLine(RobitImageString(imageid));
                            }
                            else
                            {
                                sbLRC.AppendLine(oneSentImageString(imageid, msg.messageId));
                            }

                            sbLRC.AppendLine("img1.src='" + rimgDto.picUrl + "';");
                            sbLRC.AppendLine("img1.style.width='100%';");
                            sbLRC.AppendLine("img1.style.height='100%';");
                            sbLRC.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
                            sbLRC.AppendLine("img1.id='" + imageid + "';");
                            sbLRC.AppendLine("img1.title='双击查看原图';");
                            sbLRC.AppendLine("three.appendChild(img1);");

                            sbLRC.AppendLine("first.appendChild(three);");
                            sbLRC.AppendLine("document.body.appendChild(first);");
                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                                //string imgLocalPath = "file:///" + pathHtml;
                                string imgLocalPath = pathHtml;
                                string isSucessOrFail = "0";
                                if (rimgDto.picUrl.StartsWith("file:///"))
                                {
                                    isSucessOrFail = "1";
                                }
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", isSucessOrFail, ""));
                            }

                            sbLRC.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                            sbLRC.AppendLine(
                                "img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                        }
                    }
                    else
                    {
                        if (msg.flag == 1)
                        {
                            #region 阅后即焚分支

                            //生成新的层ID
                            DivIdGather divId = getDivID(msg.chatIndex);

                            SendImageDto rimgDto =
                                JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);

                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            //sbLeftImage.AppendLine("nodeFirst.id='" + divId.hideDivId + "';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");

                            //头像显示
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");

                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            //图片消息层
                            sbLRC.AppendLine("var node=document.createElement('div');");
                            sbLRC.AppendLine("node.className='speech left';");

                            //图片最外层
                            sbLRC.AppendLine("var outImageDiv=document.createElement('div');");
                            sbLRC.AppendLine("node.appendChild(outImageDiv);");


                            //单击显示外层
                            sbLRC.AppendLine(
                                "var outdiv=document.createElement('div');outdiv.setAttribute('hideContentId','" +
                                divId.hideContentId + "');outdiv.setAttribute('countDownShowId','" +
                                divId.countDownShowId + "');outdiv.setAttribute('hideDivId','" + divId.hideDivId +
                                "');outdiv.setAttribute('time','" + 20 + "');outdiv.setAttribute('hideMessageId','" +
                                msg.messageId + "');");
                            sbLRC.AppendLine("outdiv.id='" + divId.guid + "';");
                            sbLRC.AppendLine("outdiv.addEventListener('click',clickshow);");
                            sbLRC.AppendLine("outImageDiv.appendChild(outdiv);");

                            //图片消息显示层
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.style.width='100%';");
                            sbLRC.AppendLine("img.style.height='100%';");
                            sbLRC.AppendLine("img.style.visibility='hidden';");
                            sbLRC.AppendLine("img.src='" + rimgDto.picUrl + "';");
                            sbLRC.AppendLine("img.id='" + divId.hideContentId + "';");
                            sbLRC.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
                            //sbLeftImage.AppendLine("img.id='rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                            sbLRC.AppendLine("img.title='双击查看原图';");
                            sbLRC.AppendLine("img.values='1';");

                            sbLRC.AppendLine("outdiv.appendChild(img);");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");



                            //阅后即焚和倒计时层
                            sbLRC.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                            sbLRC.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            //阅后即焚显示图片层
                            sbLRC.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                            sbLRC.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                            sbLRC.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");


                            //阅后即焚倒计时层
                            sbLRC.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                            sbLRC.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                            sbLRC.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                            sbLRC.AppendLine("outAfterTimeSpan.hidden='hidden';");
                            sbLRC.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.image, 20) + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                            sbLRC.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                            sbLRC.AppendLine(
                                "img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

                            #endregion
                        }
                        else
                        {
                            #region 正常消息分支;

                            SendImageDto rimgDto =
                                JsonConvert.DeserializeObject<SendImageDto>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);
                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");


                            sbLRC.AppendLine("var img = document.createElement('img');");

                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("img.id='" + user.userId + "';");
                            sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");


                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            sbLRC.AppendLine("var node=document.createElement('div');");
                            sbLRC.AppendLine("node.className='speech left';");

                            //sbLeftImage.AppendLine("var img = document.createElement('img');");
                            string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
                            sbLRC.AppendLine(oneLeftImageString(imageid));

                            sbLRC.AppendLine("img.style.width='100%';");
                            sbLRC.AppendLine("img.style.height='100%';");
                            sbLRC.AppendLine("img.src='" + rimgDto.picUrl + "';");
                            sbLRC.AppendLine("img.setAttribute('sid', '" + msg.messageId + "');");
                            sbLRC.AppendLine("img.id='" + imageid + "';");
                            sbLRC.AppendLine("img.title='双击查看原图';");

                            sbLRC.AppendLine("node.appendChild(img);");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");

                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                            sbLRC.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                            sbLRC.AppendLine(
                                "img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");

                            #endregion
                        }
                    }

                    #endregion

                    break;
                case (int)AntSdkMsgType.ChatMsgFile:

                    #region 文件消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        if (msg.flag == 1)
                        {
                            #region 阅后即焚
                            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                            //判断是否已经下载
                            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                            bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
                            receive.messageId = msg.messageId;
                            receive.chatIndex = msg.chatIndex;
                            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                            receive.seId = msg.sessionId;
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");
                            sbLRC.AppendLine("var seconds=document.createElement('div');");
                            sbLRC.AppendLine("seconds.className='rightimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("seconds.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(seconds);");
                            sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                            sbLRC.AppendLine("bubbleDiv.className='speech right';");
                            sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='fileDiv';");
                            sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='fileDivOne';");
                            sbLRC.AppendLine("second.appendChild(three);");
                            sbLRC.AppendLine("var four=document.createElement('div');");
                            sbLRC.AppendLine("four.className='fileImg';");
                            sbLRC.AppendLine("three.appendChild(four);");
                            //文件显示图片类型
                            sbLRC.AppendLine("var fileimage = document.createElement('img');");
                            if (receive.fileExtendName == null)
                            {
                                receive.fileExtendName =
                                    receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1),
                                        receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                            }
                            sbLRC.AppendLine("fileimage.src='" +
                                             fileShowImage.showImageHtmlPath(receive.fileExtendName,
                                                 receive.localOrServerPath) + "';");
                            sbLRC.AppendLine("four.appendChild(fileimage);");
                            sbLRC.AppendLine("var five=document.createElement('div');");
                            sbLRC.AppendLine("five.className='fileName';");
                            sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                            string fileName = receive.fileName.Length > 12
                                ? receive.fileName.Substring(0, 10) + "..."
                                : receive.fileName;
                            sbLRC.AppendLine("five.innerText='" + fileName + "';");
                            sbLRC.AppendLine("three.appendChild(five);");
                            sbLRC.AppendLine("var six=document.createElement('div');");
                            sbLRC.AppendLine("six.className='fileSize';");
                            sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                            sbLRC.AppendLine("three.appendChild(six);");
                            sbLRC.AppendLine("var seven=document.createElement('div');");
                            sbLRC.AppendLine("seven.className='fileProgressDiv';");
                            sbLRC.AppendLine("second.appendChild(seven);");
                            //进度条
                            sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                            sbLRC.AppendLine("sevenFist.className='processcontainer';");
                            sbLRC.AppendLine("seven.appendChild(sevenFist);");
                            sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                            sbLRC.AppendLine("sevenSecond.className='processbar';");
                            string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.progressId = progressId;
                            sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                            var txtShowSucessImg = "success";//接收上传状态图片
                            string txtShowDown = "上传成功";//接收上传状态文字
                            string txtOpenOrReceive = "打开";//打开接收文字
                            string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
                            //同一端发送
                            if (isOpenOrReceive)
                            {
                                if (msg.sendsucessorfail == 1)//发送成功
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                    txtShowSucessImg = "success";
                                    txtShowDown = "上传成功";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                                else//发送失败
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                    txtShowSucessImg = "fail";
                                    txtShowDown = "上传失败";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                            }
                            //不同端同步的消息
                            else
                            {
                                if (isDownFile)//已经下载
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                    txtShowSucessImg = "success";
                                    txtShowDown = "下载成功";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                                else//未下载
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                    txtShowSucessImg = "reveiving";
                                    txtShowDown = "未下载";
                                    txtOpenOrReceive = "接收";
                                    txtSaveAs = "另存为";
                                }
                            }
                            sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                            sbLRC.AppendLine("var eight=document.createElement('div');");
                            sbLRC.AppendLine("eight.className='fileOperateDiv';");
                            sbLRC.AppendLine("second.appendChild(eight);");
                            sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                            sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                            sbLRC.AppendLine("eight.appendChild(imgSorR);");
                            //接收图片添加
                            sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                            sbLRC.AppendLine("showSOFImg.className='onging';");
                            sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") +
                                             "';");
                            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileImgGuid = showImgGuid;
                            sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                            sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                            sbLRC.AppendLine("var night=document.createElement('div');");
                            sbLRC.AppendLine("night.className='fileRorS';");
                            sbLRC.AppendLine("eight.appendChild(night);");
                            //接收中添加文字
                            sbLRC.AppendLine("var nightButton=document.createElement('button');");
                            sbLRC.AppendLine("nightButton.className='fileUploadProgress';");
                            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                            sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                            receive.fileTextGuid = fileshowText;
                            sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                            sbLRC.AppendLine("night.appendChild(nightButton);");
                            sbLRC.AppendLine("var ten=document.createElement('div');");
                            sbLRC.AppendLine("ten.className='fileOpen';");
                            sbLRC.AppendLine("eight.appendChild(ten);");
                            //打开
                            sbLRC.AppendLine("var btnten=document.createElement('button');");
                            sbLRC.AppendLine("btnten.className='btnOpenFile';");
                            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                            sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                            receive.fileOpenGuid = fileOpenguid;
                            sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                            sbLRC.AppendLine("ten.appendChild(btnten);");
                            sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                            sbLRC.AppendLine("var eleven=document.createElement('div');");
                            sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                            sbLRC.AppendLine("eight.appendChild(eleven);");
                            //打开文件夹
                            sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                            sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileDirectoryGuid = fileDirectoryId;
                            sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                            sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                            string setValues = JsonConvert.SerializeObject(receive);
                            sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                            sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                            sbLRC.AppendLine("btnten.value='" + setValues + "';");
                            sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                            //阅后即焚图片显示外层
                            sbLRC.AppendLine("var imageOutDiv=document.createElement('div');");
                            sbLRC.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
                            //阅后即焚图片显示内层
                            sbLRC.AppendLine("var imageInDiv=document.createElement('img');");
                            sbLRC.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("imageOutDiv.appendChild(imageInDiv);");
                            sbLRC.AppendLine("first.appendChild(imageOutDiv);");
                            //发送失败提示
                            if (msg.sendsucessorfail == 0)
                            {
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("3", msg.messageId, msg.uploadOrDownPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                            }
                            sbLRC.AppendLine("document.body.appendChild(first);");
                            #endregion
                        }
                        else
                        {
                            #region 正常消息
                            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                            //判断是否已经下载
                            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                            bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
                            receive.messageId = msg.messageId;
                            receive.chatIndex = msg.chatIndex;
                            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                            receive.seId = msg.sessionId;
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            if (!msg.IsRobot)
                            {
                                sbLRC.AppendLine(oneSentFile(msg.messageId));
                            }
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");
                            sbLRC.AppendLine("var seconds=document.createElement('div');");
                            sbLRC.AppendLine("seconds.className='rightimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("seconds.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(seconds);");
                            sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                            sbLRC.AppendLine("bubbleDiv.className='speech right';");
                            sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='fileDiv';");
                            sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='fileDivOne';");
                            sbLRC.AppendLine("second.appendChild(three);");
                            sbLRC.AppendLine("var four=document.createElement('div');");
                            sbLRC.AppendLine("four.className='fileImg';");
                            sbLRC.AppendLine("three.appendChild(four);");
                            //文件显示图片类型
                            sbLRC.AppendLine("var fileimage = document.createElement('img');");
                            if (receive.fileExtendName == null)
                            {
                                receive.fileExtendName =
                                    receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1),
                                        receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                            }
                            sbLRC.AppendLine("fileimage.src='" +
                                             fileShowImage.showImageHtmlPath(receive.fileExtendName,
                                                 receive.localOrServerPath) + "';");
                            sbLRC.AppendLine("four.appendChild(fileimage);");
                            sbLRC.AppendLine("var five=document.createElement('div');");
                            sbLRC.AppendLine("five.className='fileName';");
                            sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                            string fileName = receive.fileName.Length > 12
                                ? receive.fileName.Substring(0, 10) + "..."
                                : receive.fileName;
                            sbLRC.AppendLine("five.innerText='" + fileName + "';");
                            sbLRC.AppendLine("three.appendChild(five);");
                            sbLRC.AppendLine("var six=document.createElement('div');");
                            sbLRC.AppendLine("six.className='fileSize';");
                            sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                            sbLRC.AppendLine("three.appendChild(six);");
                            sbLRC.AppendLine("var seven=document.createElement('div');");
                            sbLRC.AppendLine("seven.className='fileProgressDiv';");
                            sbLRC.AppendLine("second.appendChild(seven);");
                            //进度条
                            sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                            sbLRC.AppendLine("sevenFist.className='processcontainer';");
                            sbLRC.AppendLine("seven.appendChild(sevenFist);");
                            sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                            sbLRC.AppendLine("sevenSecond.className='processbar';");
                            string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.progressId = progressId;
                            sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                            var txtShowSucessImg = "success";//接收上传状态图片
                            string txtShowDown = "上传成功";//接收上传状态文字
                            string txtOpenOrReceive = "打开";//打开接收文字
                            string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
                            //同一端发送
                            if (isOpenOrReceive)
                            {
                                if (msg.sendsucessorfail == 1)//发送成功
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                    txtShowSucessImg = "success";
                                    txtShowDown = "上传成功";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                                else//发送失败
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                    txtShowSucessImg = "fail";
                                    txtShowDown = "上传失败";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                            }
                            //不同端同步的消息
                            else
                            {
                                if (isDownFile)//已经下载
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                    txtShowSucessImg = "success";
                                    txtShowDown = "下载成功";
                                    txtOpenOrReceive = "打开";
                                    txtSaveAs = "打开文件夹";
                                }
                                else//未下载
                                {
                                    sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                    txtShowSucessImg = "reveiving";
                                    txtShowDown = "未下载";
                                    txtOpenOrReceive = "接收";
                                    txtSaveAs = "另存为";
                                }
                            }
                            sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                            sbLRC.AppendLine("var eight=document.createElement('div');");
                            sbLRC.AppendLine("eight.className='fileOperateDiv';");
                            sbLRC.AppendLine("second.appendChild(eight);");
                            sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                            sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                            sbLRC.AppendLine("eight.appendChild(imgSorR);");
                            //接收图片添加
                            sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                            sbLRC.AppendLine("showSOFImg.className='onging';");
                            sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
                            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileImgGuid = showImgGuid;
                            sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                            sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                            sbLRC.AppendLine("var night=document.createElement('div');");
                            sbLRC.AppendLine("night.className='fileRorS';");
                            sbLRC.AppendLine("eight.appendChild(night);");
                            //接收中添加文字
                            sbLRC.AppendLine("var nightButton=document.createElement('button');");
                            sbLRC.AppendLine("nightButton.className='fileUploadProgress';");
                            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                            sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                            receive.fileTextGuid = fileshowText;
                            sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                            sbLRC.AppendLine("night.appendChild(nightButton);");
                            sbLRC.AppendLine("var ten=document.createElement('div');");
                            sbLRC.AppendLine("ten.className='fileOpen';");
                            sbLRC.AppendLine("eight.appendChild(ten);");
                            //打开
                            sbLRC.AppendLine("var btnten=document.createElement('button');");
                            sbLRC.AppendLine("btnten.className='btnOpenFile';");
                            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                            sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                            receive.fileOpenGuid = fileOpenguid;
                            sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                            sbLRC.AppendLine("ten.appendChild(btnten);");
                            sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                            sbLRC.AppendLine("var eleven=document.createElement('div');");
                            sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                            sbLRC.AppendLine("eight.appendChild(eleven);");
                            //打开文件夹
                            sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                            sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileDirectoryGuid = fileDirectoryId;
                            sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                            sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                            string setValues = JsonConvert.SerializeObject(receive);
                            sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                            sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                            sbLRC.AppendLine("btnten.value='" + setValues + "';");
                            sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                            //发送失败提示
                            if (msg.sendsucessorfail == 0)
                            {
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                            }
                            sbLRC.AppendLine("document.body.appendChild(first);");
                            #endregion
                        }
                    }
                    else
                    {
                        if (msg.flag == 1)
                        {
                            #region 阅后即焚分支
                            //生成新的层ID
                            DivIdGather divId = getDivID(msg.chatIndex);
                            ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                            //判断是否已经下载
                            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                            receive.messageId = msg.messageId;
                            receive.chatIndex = msg.chatIndex;
                            receive.seId = msg.sessionId;
                            receive.flag = "1";
                            //最外层DIV
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='leftd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            sbLRC.AppendLine("var seconds=document.createElement('div');");
                            sbLRC.AppendLine("seconds.className='leftimg';");
                            //头像显示
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("seconds.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(seconds);");
                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeshow);");
                            sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                            sbLRC.AppendLine("bubbleDiv.className='speech left';");
                            sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='fileDiv';");
                            sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='fileDivOne';");
                            sbLRC.AppendLine("second.appendChild(three);");
                            sbLRC.AppendLine("var four=document.createElement('div');");
                            sbLRC.AppendLine("four.className='fileImg';");
                            sbLRC.AppendLine("three.appendChild(four);");
                            //文件显示图片类型
                            sbLRC.AppendLine("var fileimage = document.createElement('img');");
                            if (receive.fileExtendName == null)
                            {
                                receive.fileExtendName =
                                    receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1),
                                        receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                            }
                            sbLRC.AppendLine("fileimage.src='" +
                                             fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                            sbLRC.AppendLine("four.appendChild(fileimage);");
                            sbLRC.AppendLine("var five=document.createElement('div');");
                            sbLRC.AppendLine("five.className='fileName';");
                            sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                            string fileName = receive.fileName.Length > 12
                                ? receive.fileName.Substring(0, 10) + "..."
                                : receive.fileName;
                            sbLRC.AppendLine("five.innerText='" + fileName + "';");
                            sbLRC.AppendLine("three.appendChild(five);");
                            sbLRC.AppendLine("var six=document.createElement('div');");
                            sbLRC.AppendLine("six.className='fileSize';");
                            sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                            sbLRC.AppendLine("three.appendChild(six);");
                            sbLRC.AppendLine("var seven=document.createElement('div');");
                            sbLRC.AppendLine("seven.className='fileProgressDiv';");
                            sbLRC.AppendLine("second.appendChild(seven);");
                            //进度条
                            sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                            sbLRC.AppendLine("sevenFist.className='processcontainer';");
                            sbLRC.AppendLine("seven.appendChild(sevenFist);");
                            sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                            sbLRC.AppendLine("sevenSecond.className='processbar';");
                            string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.progressId = progressId;
                            sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                            if (isDownFile == true)
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                            }
                            sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                            sbLRC.AppendLine("var eight=document.createElement('div');");
                            sbLRC.AppendLine("eight.className='fileOperateDiv';");
                            sbLRC.AppendLine("second.appendChild(eight);");
                            sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                            sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                            sbLRC.AppendLine("eight.appendChild(imgSorR);");
                            //接收图片添加
                            sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                            sbLRC.AppendLine("showSOFImg.className='onging';");
                            string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
                            sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") +
                                             "';");
                            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileImgGuid = showImgGuid;
                            sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                            sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                            sbLRC.AppendLine("var night=document.createElement('div');");
                            sbLRC.AppendLine("night.className='fileRorS';");
                            sbLRC.AppendLine("eight.appendChild(night);");
                            //接收中添加文字
                            sbLRC.AppendLine("var nightButton=document.createElement('button');");
                            sbLRC.AppendLine("nightButton.className='fileUploadProgressLeft';");
                            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                            sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                            receive.fileTextGuid = fileshowText;
                            string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
                            sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                            sbLRC.AppendLine("night.appendChild(nightButton);");
                            sbLRC.AppendLine("var ten=document.createElement('div');");
                            sbLRC.AppendLine("ten.className='fileOpen';");
                            sbLRC.AppendLine("eight.appendChild(ten);");
                            //打开
                            sbLRC.AppendLine("var btnten=document.createElement('button');");
                            sbLRC.AppendLine("btnten.className='btnOpenFileLeft';");
                            string fileOpenguid = divId.guid;
                            sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                            receive.fileOpenGuid = fileOpenguid;
                            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
                            sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                            sbLRC.AppendLine("ten.appendChild(btnten);");
                            sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                            sbLRC.AppendLine("var eleven=document.createElement('div');");
                            sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                            sbLRC.AppendLine("eight.appendChild(eleven);");
                            //打开文件夹
                            sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                            sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                            string fileDirectoryId = "dct" + divId.guid;
                            receive.fileDirectoryGuid = fileDirectoryId;
                            sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
                            sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                            string setValues = JsonConvert.SerializeObject(receive);
                            sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                            sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                            sbLRC.AppendLine("btnten.value='" + setValues + "';");
                            sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                            //阅后即焚和倒计时层
                            sbLRC.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                            sbLRC.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                            sbLRC.AppendLine("first.appendChild(outAfterBurnDiv);");
                            //阅后即焚显示图片层
                            sbLRC.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                            sbLRC.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                            sbLRC.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");
                            //阅后即焚倒计时层
                            sbLRC.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                            sbLRC.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                            sbLRC.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                            sbLRC.AppendLine("outAfterTimeSpan.hidden='hidden';");
                            sbLRC.AppendLine("outAfterTimeSpan.innerHTML='" + getSeconds(selectType.file, 20) + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");
                            sbLRC.AppendLine("first.appendChild(outAfterBurnDiv);");
                            sbLRC.AppendLine("document.body.appendChild(first);");
                            #endregion
                        }
                        else
                        {
                            #region 正常消息
                            ReceiveOrUploadFileDto receive =
                                JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                            //判断是否已经下载
                            bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                            receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                            receive.messageId = msg.messageId;
                            receive.chatIndex = msg.chatIndex;
                            receive.seId = msg.sessionId;
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='leftd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            sbLRC.AppendLine("var seconds=document.createElement('div');");
                            sbLRC.AppendLine("seconds.className='leftimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("img.id='" + user.userId + "';");
                            sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                            sbLRC.AppendLine("seconds.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(seconds);");
                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeshow);");
                            sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                            sbLRC.AppendLine("bubbleDiv.className='speech left';");
                            sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='fileDiv';");
                            sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='fileDivOne';");
                            sbLRC.AppendLine("second.appendChild(three);");
                            sbLRC.AppendLine("var four=document.createElement('div');");
                            sbLRC.AppendLine("four.className='fileImg';");
                            sbLRC.AppendLine("three.appendChild(four);");
                            //文件显示图片类型
                            sbLRC.AppendLine("var fileimage = document.createElement('img');");
                            if (receive.fileExtendName == null)
                            {
                                receive.fileExtendName =
                                    receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1),
                                        receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                            }
                            sbLRC.AppendLine("fileimage.src='" +
                                             fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                            sbLRC.AppendLine("four.appendChild(fileimage);");
                            sbLRC.AppendLine("var five=document.createElement('div');");
                            sbLRC.AppendLine("five.className='fileName';");
                            sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                            string fileName = receive.fileName.Length > 12
                                ? receive.fileName.Substring(0, 10) + "..."
                                : receive.fileName;
                            sbLRC.AppendLine("five.innerText='" + fileName + "';");
                            sbLRC.AppendLine("three.appendChild(five);");
                            sbLRC.AppendLine("var six=document.createElement('div');");
                            sbLRC.AppendLine("six.className='fileSize';");
                            sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                            sbLRC.AppendLine("three.appendChild(six);");
                            sbLRC.AppendLine("var seven=document.createElement('div');");
                            sbLRC.AppendLine("seven.className='fileProgressDiv';");
                            sbLRC.AppendLine("second.appendChild(seven);");
                            //进度条
                            sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                            sbLRC.AppendLine("sevenFist.className='processcontainer';");
                            sbLRC.AppendLine("seven.appendChild(sevenFist);");
                            sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                            sbLRC.AppendLine("sevenSecond.className='processbar';");
                            string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.progressId = progressId;
                            sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                            if (isDownFile == true)
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                            }
                            sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                            sbLRC.AppendLine("var eight=document.createElement('div');");
                            sbLRC.AppendLine("eight.className='fileOperateDiv';");
                            sbLRC.AppendLine("second.appendChild(eight);");
                            sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                            sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                            sbLRC.AppendLine("eight.appendChild(imgSorR);");
                            //接收图片添加
                            sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                            sbLRC.AppendLine("showSOFImg.className='onging';");
                            string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
                            sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") +
                                             "';");
                            string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileImgGuid = showImgGuid;
                            sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                            sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                            sbLRC.AppendLine("var night=document.createElement('div');");
                            sbLRC.AppendLine("night.className='fileRorS';");
                            sbLRC.AppendLine("eight.appendChild(night);");
                            //接收中添加文字
                            sbLRC.AppendLine("var nightButton=document.createElement('button');");
                            sbLRC.AppendLine("nightButton.className='fileUploadProgressLeft';");
                            string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                            sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                            receive.fileTextGuid = fileshowText;
                            string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
                            sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                            sbLRC.AppendLine("night.appendChild(nightButton);");
                            sbLRC.AppendLine("var ten=document.createElement('div');");
                            sbLRC.AppendLine("ten.className='fileOpen';");
                            sbLRC.AppendLine("eight.appendChild(ten);");
                            //打开
                            sbLRC.AppendLine("var btnten=document.createElement('button');");
                            sbLRC.AppendLine("btnten.className='btnOpenFileLeft';");
                            string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                            sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                            receive.fileOpenGuid = fileOpenguid;
                            string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
                            sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                            sbLRC.AppendLine("ten.appendChild(btnten);");
                            sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                            sbLRC.AppendLine("var eleven=document.createElement('div');");
                            sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                            sbLRC.AppendLine("eight.appendChild(eleven);");
                            //打开文件夹
                            sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                            sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                            string fileDirectoryId = "gxc" + Guid.NewGuid().ToString().Replace("-", "");
                            receive.fileDirectoryGuid = fileDirectoryId;
                            sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                            string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
                            sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                            string setValues = JsonConvert.SerializeObject(receive);
                            sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                            sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                            sbLRC.AppendLine("btnten.value='" + setValues + "';");
                            sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                            sbLRC.AppendLine("document.body.appendChild(first);");
                            #endregion
                        }
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgAudio:

                    #region 语音消息

                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        if (msg.flag == 1)
                        {
                            AntSdkChatMsg.Audio_content receive =
                                JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);

                            #region 圆形图片Right

                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");

                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            //点击播放
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");
                            sbLRC.AppendLine("three.id='M" + msg.messageId + "';");
                            sbLRC.AppendLine(
                                "three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" +
                                receive.audioUrl.Replace("\r\n", "<br/>")
                                    .Replace("\n", "<br/>")
                                    .Replace(@"\", @"\\")
                                    .Replace("'", "&#39") +
                                "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId +
                                "');three.setAttribute('isShowGif','" + isShowGifId +
                                "');three.setAttribute('isLeftOrRight','1');");

                            string musicImg = "file:///" +
                                              (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png")
                                                  .Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img_right';");
                            sbLRC.AppendLine("three.appendChild(imgmp3);");
                            sbLRC.AppendLine("var div3 = document.createElement('div');");
                            sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                            sbLRC.AppendLine("div3.className ='divmp3_contain_right';");
                            sbLRC.AppendLine("three.appendChild(div3);");
                            sbLRC.AppendLine("first.appendChild(three);");
                            //阅后即焚图片显示外层
                            sbLRC.AppendLine("var imageOutDiv=document.createElement('div');");
                            sbLRC.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");
                            //阅后即焚图片显示内层
                            sbLRC.AppendLine("var imageInDiv=document.createElement('img');");
                            sbLRC.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("imageOutDiv.appendChild(imageInDiv);");
                            sbLRC.AppendLine("first.appendChild(imageOutDiv);");
                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                                string imgLocalPath = "file:///" + pathHtml;
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendBurnVoice", msg.messageId, imgLocalPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
                            }
                            sbLRC.AppendLine("document.body.appendChild(first);");

                            #endregion
                        }
                        else
                        {
                            AntSdkChatMsg.Audio_content receive =
                                JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);

                            #region 圆形图片Right

                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");
                            sbLRC.AppendLine("three.id='M" + msg.messageId + "';");
                            if (msg.IsRobot == true)
                            {
                                sbLRC.AppendLine(
                                    "three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" +
                                    receive.audioUrl.Replace("\r\n", "<br/>")
                                        .Replace("\n", "<br/>")
                                        .Replace(@"\", @"\\")
                                        .Replace("'", "&#39") +
                                    "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" +
                                    isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId +
                                    "');three.setAttribute('isLeftOrRight','1');three.setAttribute('isRecallMenu','1');");
                            }
                            else
                            {
                                sbLRC.AppendLine(
                                    "three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" +
                                    receive.audioUrl.Replace("\r\n", "<br/>")
                                        .Replace("\n", "<br/>")
                                        .Replace(@"\", @"\\")
                                        .Replace("'", "&#39") +
                                    "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" +
                                    isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId +
                                    "');three.setAttribute('isLeftOrRight','1');");
                            }

                            string musicImg = "file:///" +
                                              (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png")
                                                  .Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img_right';");
                            sbLRC.AppendLine("three.appendChild(imgmp3);");



                            sbLRC.AppendLine("var div3 = document.createElement('div');");
                            sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                            sbLRC.AppendLine("div3.className ='divmp3_contain_right';");
                            sbLRC.AppendLine("three.appendChild(div3);");


                            sbLRC.AppendLine("first.appendChild(three);");

                            ////未读圆形提示
                            //string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/接受成功.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //sbRight.AppendLine("var divCircle = document.createElement('div');");
                            //sbRight.AppendLine("divCircle.className ='rightVoice';");
                            //sbRight.AppendLine("var imgCircle = document.createElement('img');");
                            //sbRight.AppendLine("imgCircle.src='" + CirCleImg + "';");
                            //sbRight.AppendLine("divCircle.appendChild(imgCircle);");
                            //sbRight.AppendLine("first.appendChild(divCircle);");
                            //是否失败
                            if (msg.sendsucessorfail == 0)
                            {
                                string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                                string imgLocalPath = "file:///" + pathHtml;
                                sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendVoice", msg.messageId, imgLocalPath,
                                    Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
                            }
                            sbLRC.AppendLine("document.body.appendChild(first);");

                            #endregion
                        }
                    }
                    else
                    {
                        if (msg.flag == 1)
                        {
                            AntSdkChatMsg.Audio_content receive =
                                JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);
                            //生成新的层ID
                            DivIdGather divId = getDivVoiceID(msg.chatIndex, receive.duration);
                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");
                            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                            //头像
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("img.id='" + user.userId + "';");
                            sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");


                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            sbLRC.AppendLine("var node=document.createElement('div');");
                            sbLRC.AppendLine("node.className='speech left';");
                            sbLRC.AppendLine("node.id='M" + msg.messageId + "';");
                            sbLRC.AppendLine(
                                "node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" +
                                receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" +
                                isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId +
                                "');node.setAttribute('divGuid','" + divId.guid +
                                "');node.setAttribute('divChatIndex','" + msg.chatIndex +
                                "');node.setAttribute('isBurn','0');");

                            string musicImg = "file:///" +
                                              (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png")
                                                  .Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img';");
                            sbLRC.AppendLine("node.appendChild(imgmp3);");

                            sbLRC.AppendLine("var div3 = document.createElement('div');");
                            sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                            sbLRC.AppendLine("div3.className ='divmp3_contain';");

                            sbLRC.AppendLine("node.appendChild(div3);");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");

                            //阅后即焚和倒计时层
                            sbLRC.AppendLine("var outAfterBurnDiv=document.createElement('div');");
                            sbLRC.AppendLine("outAfterBurnDiv.className='outafterBurnRead';");
                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            //阅后即焚显示图片层
                            sbLRC.AppendLine("var outAfterIsShowImage=document.createElement('img');");
                            sbLRC.AppendLine("outAfterIsShowImage.id='" + divId.imageIsShowId + "';");
                            sbLRC.AppendLine("outAfterIsShowImage.className='afterBurnImage';");
                            string afterBurnImagePath = "file:///" +
                                                        (AppDomain.CurrentDomain.BaseDirectory +
                                                         "Images\\burnAfterRead\\阅后即焚-消息-左.png").Replace(@"\", @"/")
                                                            .Replace(" ", "%20");
                            sbLRC.AppendLine("outAfterIsShowImage.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterIsShowImage);");

                            //阅后即焚倒计时层
                            sbLRC.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                            sbLRC.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                            sbLRC.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                            sbLRC.AppendLine("outAfterTimeSpan.hidden='hidden';");
                            sbLRC.AppendLine("outAfterTimeSpan.innerHTML='" +
                                             getSeconds(selectType.voice, /*TODO:AntSdk_Modify:msg.content*/
                                                 receive.duration) + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");

                            //阅后即焚倒计时层
                            sbLRC.AppendLine("var outAfterTimeSpan=document.createElement('span');");
                            sbLRC.AppendLine("outAfterTimeSpan.id='" + divId.countDownShowId + "';");
                            sbLRC.AppendLine("outAfterTimeSpan.className='countTimeValues';");
                            sbLRC.AppendLine("outAfterTimeSpan.hidden='hidden';");
                            sbLRC.AppendLine("outAfterTimeSpan.innerHTML='" +
                                             getSeconds(selectType.voice, /*TODO:AntSdk_Modify:msg.content*/
                                                 receive.duration) + "';");
                            sbLRC.AppendLine("outAfterBurnDiv.appendChild(outAfterTimeSpan);");

                            sbLRC.AppendLine("nodeFirst.appendChild(outAfterBurnDiv);");
                            if (msg.voiceread + "" == "")
                            {
                                //未读圆形提示
                                string CirCleImg = "file:///" +
                                                   (AppDomain.CurrentDomain.BaseDirectory +
                                                    "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/")
                                                       .Replace(" ", "%20");
                                sbLRC.AppendLine("var divCircle = document.createElement('div');");
                                sbLRC.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                                sbLRC.AppendLine("divCircle.className ='leftVoiceBurn';");
                                sbLRC.AppendLine("var imgCircle = document.createElement('img');");
                                sbLRC.AppendLine("imgCircle.src='" + CirCleImg + "';");
                                sbLRC.AppendLine("divCircle.appendChild(imgCircle);");
                                sbLRC.AppendLine("nodeFirst.appendChild(divCircle);");
                            }
                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                        }
                        else
                        {
                            AntSdkChatMsg.Audio_content receive =
                                JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>( /*TODO:AntSdk_Modify:msg.content*/
                                    msg.sourceContent);
                            sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                            sbLRC.AppendLine("nodeFirst.className='leftd';");
                            //sbLeft.AppendLine("nodeFirst.='" + receive.audioUrl+"';");
                            sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");
                            string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                            string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                            //头像
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("img.id='" + user.userId + "';");
                            sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("nodeFirst.appendChild(second);");

                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                            sbLRC.AppendLine("var node=document.createElement('div');");
                            sbLRC.AppendLine("node.className='speech left';");
                            sbLRC.AppendLine("node.id='M" + msg.messageId + "';");
                            sbLRC.AppendLine(
                                "node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" +
                                receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" +
                                isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

                            string musicImg = "file:///" +
                                              (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png")
                                                  .Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img';");
                            sbLRC.AppendLine("node.appendChild(imgmp3);");

                            sbLRC.AppendLine("var div3 = document.createElement('div');");
                            sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                            sbLRC.AppendLine("div3.className ='divmp3_contain';");

                            sbLRC.AppendLine("node.appendChild(div3);");

                            sbLRC.AppendLine("nodeFirst.appendChild(node);");
                            if (msg.voiceread + "" == "")
                            {
                                //未读圆形提示
                                string CirCleImg = "file:///" +
                                                   (AppDomain.CurrentDomain.BaseDirectory +
                                                    "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/")
                                                       .Replace(" ", "%20");
                                sbLRC.AppendLine("var divCircle = document.createElement('div');");
                                sbLRC.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                                sbLRC.AppendLine("divCircle.className ='leftVoice';");
                                sbLRC.AppendLine("var imgCircle = document.createElement('img');");
                                sbLRC.AppendLine("imgCircle.src='" + CirCleImg + "';");
                                sbLRC.AppendLine("divCircle.appendChild(imgCircle);");
                                sbLRC.AppendLine("nodeFirst.appendChild(divCircle);");
                            }

                            sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                        }
                    }

                    #endregion

                    break;
                case (int)AntSdkMsgType.Revocation:

                    #region 撤回消息

                    sbLRC.AppendLine("var firsts=document.createElement('div');");
                    sbLRC.AppendLine("firsts.className='speechCenterTips';");
                    sbLRC.AppendLine("firsts.id='" + msg.messageId + "';");
                    sbLRC.AppendLine("firsts.innerHTML ='" + msg.sourceContent + "';");
                    sbLRC.AppendLine("document.body.appendChild(firsts);");

                    #endregion

                    break;
                case (int)AntSdkMsgType.ChatMsgMixMessage:

                    #region 图文混排消息

                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        #region 右边展示

                        //显示内容解析
                        List<MixMessageObjDto> receive =
                            JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + PictureAndTextMixMethod.timeComparison(msg.sendTime) +
                                         "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //图文混合展示层
                        //sbLRC.AppendLine("var node = document.createElement('div')");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine(PublicTalkMothed.divRightCopyContent(divid, msg.messageId));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech right';");
                        int i = 0;
                        //图文混合内部构造
                        StringBuilder sbInside = new StringBuilder();
                        foreach (var list in receive)
                        {
                            switch (list.type)
                            {
                                //文本
                                case "1001":
                                    sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                                    break;
                                //图片
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix =
                                        JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(
                                            list.content?.ToString());
                                    sbInside.Append("<img id=\"" + ImgId[i] + "\" src=\"" + pictureAndTextMix.picUrl +
                                                    "\" class=\"imgRightProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                                    break;
                                //换行
                                case "0000":
                                    sbInside.Append("<br/>");
                                    break;
                            }
                        }
                        sbLRC.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
                        sbLRC.AppendLine("first.appendChild(node);");
                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            string guid = Guid.NewGuid().ToString().Replace("-", "");

                            sbLRC.AppendLine(PictureAndTextMixMethod.OnceSendHistoryPicDiv("sendMixPic", msg.messageId,
                                "", guid, "sending" + guid, "", ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");

                        #endregion
                    }
                    else
                    {
                        #region 左边展示

                        //显示内容解析
                        List<MixMessageObjDto> receive =
                            JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);

                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + pathImage + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + PictureAndTextMixMethod.timeComparison(msg.sendTime) +
                                         "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        //图文混合展示层
                        //sbLRC.AppendLine("var node = document.createElement('div');");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine(PublicTalkMothed.divLeftCopyContent(divid));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech left';");
                        int i = 0;
                        //图文混合内部构造
                        StringBuilder sbInside = new StringBuilder();
                        foreach (var list in receive)
                        {
                            switch (list.type)
                            {
                                //文本
                                case "1001":
                                    sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                                    break;
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix =
                                        JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(
                                            list.content?.ToString());
                                    sbInside.Append("<img id=\"" + ImgId[i] + "\" src=\"" + pictureAndTextMix.picUrl +
                                                    "\" class=\"imgLeftProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                                    break;
                                case "0000":
                                    sbInside.Append("<br/>");
                                    break;
                            }
                        }
                        sbLRC.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(node);");
                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");

                        #endregion
                    }

                    #endregion

                    break;
                case (int)AntSdkMsgType.PointAudioVideo:
                    {
                        #region 语音电话

                        if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                        {
                            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
                            if (string.IsNullOrEmpty(audioMsg.text))
                                audioMsg.text = "发起了语音电话";
                            sbLRC.AppendLine("{ var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            //头像显示层
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");
                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeshow);");

                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");
                            var musicImg = "file:///" +
                                           (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-右.png")
                                               .Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img_right';");
                            sbLRC.AppendLine("three.appendChild(imgmp3);");
                            sbLRC.AppendLine("var node = document.createElement('span');");
                            sbLRC.AppendLine("node.innerHTML='" + audioMsg.text + "&nbsp;&nbsp;';");
                            sbLRC.AppendLine("three.appendChild(node);");
                            sbLRC.AppendLine("first.appendChild(three);");
                            sbLRC.AppendLine("document.body.appendChild(first);}");
                        }
                        else
                        {
                            var audioMsg = JsonConvert.DeserializeObject<PointAudioVideo_content>(msg.sourceContent);
                            if (string.IsNullOrEmpty(audioMsg.text))
                                audioMsg.text = "发起了语音电话";
                            sbLRC.AppendLine("{ var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='leftd';");
                            sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                            //头像显示层
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='leftimg';");
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + pathImage + "';");
                            sbLRC.AppendLine("img.className='divcss5Left';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");
                            //时间显示
                            sbLRC.AppendLine("var timeshow = document.createElement('div');");
                            sbLRC.AppendLine("timeshow.className='leftTimeText';");
                            sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeshow);");

                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech left';");
                            var musicImg = "file:///" +
                                           (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音通话-气泡-左.png")
                                               .Replace(
                                                   @"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                            sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                            sbLRC.AppendLine("imgmp3.className ='divmp3_img';");
                            sbLRC.AppendLine("three.appendChild(imgmp3);");
                            sbLRC.AppendLine("var node = document.createElement('span');");
                            sbLRC.AppendLine("node.innerHTML='&nbsp;&nbsp;" + audioMsg.text + "';");
                            sbLRC.AppendLine("three.appendChild(node);");
                            sbLRC.AppendLine("first.appendChild(three);");
                            sbLRC.AppendLine("document.body.appendChild(first);}");
                        }

                        #endregion
                    }
                    break;
            }
            return sbLRC.ToString();
        }
        /// <summary>
        /// 个人消息滚动批量解析
        /// </summary>
        /// <returns></returns>
        public static string OneToOneScrollMessage(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, AntSdkContact_User user)
        {
            StringBuilder sbLRC = new StringBuilder();
            switch (Convert.ToInt32(/*TODO:AntSdk_Modify:msg.MTP*/msg.MsgType))
            {
                case (int)GlobalVariable.MsgType.Text:
                    #region 文本消息显示
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        #region 本人消息展示
                        if (msg.flag == 1)
                        {
                            #region 阅后即焚消息
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");
                            sbLRC.Append("first.id=" + msg.chatIndex + ";");

                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");

                            //头像显示层
                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");
                            ////string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            ////if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            ////{
                            ////    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            ////}
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            //内容显示层
                            sbLRC.AppendLine("var three=document.createElement('div');");
                            sbLRC.AppendLine("three.className='speech right';");
                            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                            sbLRC.AppendLine("three.innerHTML ='" + showMsg + "';");
                            sbLRC.AppendLine("first.appendChild(three);");

                            //阅后即焚图片显示外层
                            sbLRC.AppendLine("var imageOutDiv=document.createElement('div');");
                            sbLRC.AppendLine("imageOutDiv.className='rightOutAfterImageDiv';");


                            //阅后即焚图片显示内层
                            sbLRC.AppendLine("var imageInDiv=document.createElement('img');");
                            sbLRC.AppendLine("imageInDiv.className='rightInAfterImageDiv';");
                            string afterBurnImagePath = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images\\burnAfterRead\\阅后即焚-默认.png").Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("imageInDiv.src='" + afterBurnImagePath + "';");
                            sbLRC.AppendLine("imageOutDiv.appendChild(imageInDiv);");
                            sbLRC.AppendLine("first.appendChild(imageOutDiv);");

                            //获取body层
                            sbLRC.AppendLine("var listbody = document.getElementById('bodydiv');");
                            sbLRC.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
                            #endregion
                        }
                        else
                        {
                            #region 正常消息
                            sbLRC.AppendLine("var first=document.createElement('div');");
                            sbLRC.AppendLine("first.className='rightd';");

                            //时间显示层
                            sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                            sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                            sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                            sbLRC.AppendLine("first.appendChild(timeDiv);");


                            sbLRC.AppendLine("var second=document.createElement('div');");
                            sbLRC.AppendLine("second.className='rightimg';");


                            //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                            //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                            //{
                            //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                            //}
                            sbLRC.AppendLine("var img = document.createElement('img');");
                            sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                            sbLRC.AppendLine("img.className='divcss5';");
                            sbLRC.AppendLine("second.appendChild(img);");
                            sbLRC.AppendLine("first.appendChild(second);");

                            //sbRight.AppendLine("var three=document.createElement('div');");
                            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
                            sbLRC.AppendLine(divRightCopyContent(divid, msg.messageId));
                            sbLRC.AppendLine("node.id='" + divid + "';");
                            sbLRC.AppendLine("node.className='speech right';");

                            string showMsg = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                            sbLRC.AppendLine("node.innerHTML ='" + showMsg + "';");

                            sbLRC.AppendLine("first.appendChild(node);");

                            //获取body层
                            sbLRC.AppendLine("var listbody = document.getElementById('bodydiv');");
                            sbLRC.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);");
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region 非本人消息

                        #endregion
                    }
                    #endregion
                    break;
            }
            return sbLRC.ToString();
        }
        /// <summary>
        /// 群组消息批量解析
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="GroupMembers"></param>
        /// <returns></returns>
        public static string LeftGroupToGroupShowMessage(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers, List<string> ImgId = null)
        {
            #region 获取接收者头像 New
            string pathImages = "";
            //获取接收者头像
            //var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
            var user = getGroupMembersUser(GroupMembers, msg);
            //if (listUser == null)
            //{
            //if (user == null)
            //{
            //    AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == msg.sendUserId);
            //    if (cus == null)
            //    {
            //        user = new AntSdkGroupMember();
            //        pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/离职人员.png").Replace(@"\", @"/").Replace(" ", "%20");
            //        user.picture = pathImages;
            //        user.userName = "离职人员";
            //    }
            //    else
            //    {
            //        user = new AntSdkGroupMember();
            //        user.userNum = cus.userNum;
            //        user.userName = cus.userName;
            //        user.userId = cus.userId;
            //        user.position = cus.position;
            //        user.picture = cus.picture;
            //    }
            //}
            //}
            #endregion
            StringBuilder sbLRC = new StringBuilder();
            switch (Convert.ToInt32(/*TODO:AntSdk_Modify:msg.MTP*/msg.MsgType))
            {
                case (int)AntSdkMsgType.CreateActivity:
                    #region 活动
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        //显示内容解析
                        Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //活动内容展示层
                        sbLRC.AppendLine("var node = document.createElement('div')");
                        sbLRC.AppendLine("node.className='voteBgColor speech right';");

                        //设置活动Id
                        sbLRC.AppendLine("node.id='" + receive.activityId + "';");
                        //事件监听
                        sbLRC.AppendLine("node.addEventListener('click',clickActivityShow);");

                        //活动默认图片显示层
                        sbLRC.AppendLine("var imgVote=document.createElement('img');");
                        sbLRC.AppendLine("imgVote.className='activityImg';");
                        sbLRC.AppendLine("imgVote.src='" + receive.picture + "';");
                        sbLRC.AppendLine("node.appendChild(imgVote);");

                        //活动title显示层
                        sbLRC.AppendLine("var voteTitle = document.createElement('div');");
                        sbLRC.AppendLine("voteTitle.className='voteTitle';");
                        sbLRC.AppendLine("voteTitle.innerHTML ='" + receive.theme + "';");
                        sbLRC.AppendLine("node.appendChild(voteTitle);");

                        //换行1
                        sbLRC.AppendLine("var  newLineOne= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineOne);");

                        //活动时间
                        sbLRC.AppendLine("var voteInFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteInFirst.className='activityTP';");
                        sbLRC.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';");
                        sbLRC.AppendLine("node.appendChild(voteInFirst);");

                        //换行2
                        sbLRC.AppendLine("var  newLineTwo= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineTwo);");

                        //活动地点
                        sbLRC.AppendLine("var voteInSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteInSecond.className='activityTP';");
                        sbLRC.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
                        sbLRC.AppendLine("node.appendChild(voteInSecond);");

                        //换行4
                        sbLRC.AppendLine("var  newLineFour= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineFour);");

                        sbLRC.AppendLine("first.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(first);");
                    }
                    else
                    {
                        //显示内容解析
                        Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");


                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        //活动内容展示层
                        sbLRC.AppendLine("var node = document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");
                        //设置活动Id
                        sbLRC.AppendLine("node.id='" + receive.activityId + "';");
                        //事件监听
                        sbLRC.AppendLine("node.addEventListener('click',clickActivityShow);");

                        //活动默认图片显示层
                        sbLRC.AppendLine("var imgVote=document.createElement('img');");
                        sbLRC.AppendLine("imgVote.className='activityImg';");
                        sbLRC.AppendLine("imgVote.src='" + receive.picture + "';");
                        sbLRC.AppendLine("node.appendChild(imgVote);");

                        //活动title显示层
                        sbLRC.AppendLine("var voteTitle = document.createElement('div');");
                        sbLRC.AppendLine("voteTitle.className='voteTitle';");
                        sbLRC.AppendLine("voteTitle.innerHTML ='" + receive.theme + "';");
                        sbLRC.AppendLine("node.appendChild(voteTitle);");

                        //换行1
                        sbLRC.AppendLine("var  newLineOne= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineOne);");

                        sbLRC.AppendLine("var voteInFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteInFirst.className='voteTitle';");
                        sbLRC.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';"); ;
                        sbLRC.AppendLine("node.appendChild(voteInFirst);");

                        //换行2
                        sbLRC.AppendLine("var  newLineTwo= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineTwo);");

                        //活动地点
                        sbLRC.AppendLine("var voteInSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteInSecond.className='voteTitle';");
                        sbLRC.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
                        sbLRC.AppendLine("node.appendChild(voteInSecond);");

                        //换行4
                        sbLRC.AppendLine("var  newLineFour= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineFour);");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.CreateVote:
                    #region 投票
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        //显示内容解析
                        MsChatMsgCreateVote_content receive = JsonConvert.DeserializeObject<MsChatMsgCreateVote_content>(msg.sourceContent);

                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");


                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //投票内容展示层
                        sbLRC.AppendLine("var node = document.createElement('div')");
                        sbLRC.AppendLine("node.className='voteBgColor speech right';");

                        //设置活动Id
                        sbLRC.AppendLine("node.id='" + receive.id + "';");
                        //事件监听
                        sbLRC.AppendLine("node.addEventListener('click',clickVoteShow);");

                        //投票默认图片显示层
                        sbLRC.AppendLine("var imgVote=document.createElement('img');");
                        sbLRC.AppendLine("imgVote.className='baseFloatLeft';");
                        sbLRC.AppendLine("imgVote.src='" + PictureAndTextMixMethod.VoteImg + "';");
                        sbLRC.AppendLine("node.appendChild(imgVote);");

                        //投票title显示层
                        sbLRC.AppendLine("var voteTitle = document.createElement('div');");
                        sbLRC.AppendLine("voteTitle.className='voteTitle';");
                        sbLRC.AppendLine("voteTitle.innerHTML ='" + receive.title + "';");
                        sbLRC.AppendLine("node.appendChild(voteTitle);");


                        //换行1
                        sbLRC.AppendLine("var  newLineOne= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineOne);");

                        //投票第一项
                        sbLRC.AppendLine("var voteFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteFirst.className='divCircle';");


                        sbLRC.AppendLine("var voteInFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteInFirst.className='voteContent';");
                        sbLRC.AppendLine("voteInFirst.innerHTML ='" + receive.options[0].name + "';");
                        sbLRC.AppendLine("voteFirst.appendChild(voteInFirst);");
                        sbLRC.AppendLine("node.appendChild(voteFirst);");


                        //换行2
                        sbLRC.AppendLine("var  newLineTwo= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineTwo);");

                        //投票第二项
                        sbLRC.AppendLine("var voteSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteSecond.className='divCircle';");

                        sbLRC.AppendLine("var voteInSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteInSecond.className='voteContent';");
                        sbLRC.AppendLine("voteInSecond.innerHTML ='" + receive.options[1].name + "';");
                        sbLRC.AppendLine("voteSecond.appendChild(voteInSecond);");
                        sbLRC.AppendLine("node.appendChild(voteSecond);");

                        if (receive.options.Count > 3)
                        {
                            //换行3
                            sbLRC.AppendLine("var  newLineThree= document.createElement('br');");
                            sbLRC.AppendLine("node.appendChild(newLineThree);");

                            //投票第三项
                            sbLRC.AppendLine("var voteThree = document.createElement('div');");
                            sbLRC.AppendLine("voteThree.className='divCircle';");

                            sbLRC.AppendLine("var voteInThree = document.createElement('div');");
                            sbLRC.AppendLine("voteInThree.className='voteContent';");
                            sbLRC.AppendLine("voteInThree.innerHTML ='" + receive.options[2].name + "';");
                            sbLRC.AppendLine("voteThree.appendChild(voteInThree);");
                            sbLRC.AppendLine("node.appendChild(voteThree);");
                        }

                        //换行4
                        sbLRC.AppendLine("var  newLineFour= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineFour);");

                        sbLRC.AppendLine("first.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(first);");
                    }
                    else
                    {
                        //显示内容解析
                        MsChatMsgCreateVote_content receive = JsonConvert.DeserializeObject<MsChatMsgCreateVote_content>(msg.sourceContent);

                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        //投票内容展示层
                        sbLRC.AppendLine("var node = document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");

                        //设置活动Id
                        sbLRC.AppendLine("node.id='" + receive.id + "';");
                        //事件监听
                        sbLRC.AppendLine("node.addEventListener('click',clickVoteShow);");

                        //投票默认图片显示层
                        sbLRC.AppendLine("var imgVote=document.createElement('img');");
                        sbLRC.AppendLine("imgVote.className='baseFloatLeft';");
                        sbLRC.AppendLine("imgVote.src='" + PictureAndTextMixMethod.VoteImg + "';");
                        sbLRC.AppendLine("node.appendChild(imgVote);");

                        //投票title显示层
                        sbLRC.AppendLine("var voteTitle = document.createElement('div');");
                        sbLRC.AppendLine("voteTitle.className='voteTitle';");
                        sbLRC.AppendLine("voteTitle.innerHTML ='" + receive.title + "';");
                        sbLRC.AppendLine("node.appendChild(voteTitle);");

                        //换行1
                        sbLRC.AppendLine("var  newLineOne= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineOne);");

                        //投票第一项
                        sbLRC.AppendLine("var voteFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteFirst.className='divCircle';");

                        sbLRC.AppendLine("var voteInFirst = document.createElement('div');");
                        sbLRC.AppendLine("voteInFirst.className='voteContent';");
                        sbLRC.AppendLine("voteInFirst.innerHTML ='" + receive.options[0].name + "';");
                        sbLRC.AppendLine("voteFirst.appendChild(voteInFirst);");
                        sbLRC.AppendLine("node.appendChild(voteFirst);");


                        //换行2
                        sbLRC.AppendLine("var  newLineTwo= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineTwo);");

                        //投票第二项
                        sbLRC.AppendLine("var voteSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteSecond.className='divCircle';");

                        sbLRC.AppendLine("var voteInSecond = document.createElement('div');");
                        sbLRC.AppendLine("voteInSecond.className='voteContent';");
                        sbLRC.AppendLine("voteInSecond.innerHTML ='" + receive.options[1].name + "';");
                        sbLRC.AppendLine("voteSecond.appendChild(voteInSecond);");
                        sbLRC.AppendLine("node.appendChild(voteSecond);");
                        if (receive.options.Count > 3)
                        {
                            //换行3
                            sbLRC.AppendLine("var  newLineThree= document.createElement('br');");
                            sbLRC.AppendLine("node.appendChild(newLineThree);");

                            //投票第三项
                            sbLRC.AppendLine("var voteThree = document.createElement('div');");
                            sbLRC.AppendLine("voteThree.className='divCircle';");

                            sbLRC.AppendLine("var voteInThree = document.createElement('div');");
                            sbLRC.AppendLine("voteInThree.className='voteContent';");
                            sbLRC.AppendLine("voteInThree.innerHTML ='" + receive.options[2].name + "';");
                            sbLRC.AppendLine("voteThree.appendChild(voteInThree);");
                            sbLRC.AppendLine("node.appendChild(voteThree);");

                        }
                        //换行4
                        sbLRC.AppendLine("var  newLineFour= document.createElement('br');");
                        sbLRC.AppendLine("node.appendChild(newLineFour);");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgMixMessage:
                    #region 图文混合显示
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        //显示内容解析
                        List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);
                        #region 图文混合Right
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //图文混合展示层
                        //sbLRC.AppendLine("var node = document.createElement('div')");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine(PublicTalkMothed.divRightCopyContent(divid, msg.messageId));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech right';");
                        int i = 0;
                        //图文混合内部构造
                        StringBuilder sbInside = new StringBuilder();
                        foreach (var list in receive)
                        {
                            switch (list.type)
                            {
                                //文本
                                case "1001":
                                    sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                                    break;
                                //图片
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(list.content?.ToString());
                                    sbInside.Append("<img id=\"" + ImgId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgRightProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                                    i++;
                                    break;
                                //换行
                                case "0000":
                                    sbInside.Append("<br/>");
                                    break;
                                //@消息
                                case "1008":
                                    List<At_content> at = JsonConvert.DeserializeObject<List<At_content>>(list.content.ToString());
                                    string strAt = "";
                                    foreach (var atList in at)
                                    {
                                        if (atList.type == "1112")
                                        {
                                            foreach (var atName in atList.names)
                                            {
                                                strAt += "@" + atName;
                                            }
                                            sbInside.Append(strAt);
                                        }
                                        else
                                        {
                                            #region @全体成员
                                            sbInside.Append("@全体成员");
                                            #endregion
                                        }
                                    }
                                    break;
                            }
                        }
                        sbLRC.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
                        sbLRC.AppendLine("first.appendChild(node);");
                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            string guid = Guid.NewGuid().ToString().Replace("-", "");

                            sbLRC.AppendLine(PictureAndTextMixMethod.OnceSendHistoryPicDiv("sendMixPic", msg.messageId, "", guid, "sending" + guid, "", ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    else
                    {
                        //显示内容解析
                        List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);
                        sbLRC.AppendLine(" var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        //头像显示层
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + PictureAndTextMixMethod.getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        //图文混合展示层
                        //sbLRC.AppendLine("var node = document.createElement('div');");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine(PublicTalkMothed.divLeftCopyContent(divid));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech left';");
                        int i = 0;
                        //图文混合内部构造
                        StringBuilder sbInside = new StringBuilder();
                        foreach (var list in receive)
                        {
                            switch (list.type)
                            {
                                //文本
                                case "1001":
                                    sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                                    break;
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(list.content?.ToString());
                                    sbInside.Append("<img id=\"" + ImgId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgLeftProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                                    i++;
                                    break;
                                case "0000":
                                    sbInside.Append("<br/>");
                                    break;
                                //@消息
                                case "1008":
                                    List<At_content> at = JsonConvert.DeserializeObject<List<At_content>>(list.content.ToString());
                                    string strAt = "";
                                    foreach (var atList in at)
                                    {
                                        if (atList.type == "1112")
                                        {
                                            foreach (var atName in atList.names)
                                            {
                                                strAt += "@" + atName;
                                            }
                                            sbInside.Append(strAt);
                                        }
                                        else
                                        {
                                            #region @全体成员
                                            sbInside.Append("@全体成员");
                                            #endregion
                                        }
                                    }
                                    break;

                            }
                        }
                        sbLRC.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(node);");
                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgText:
                    #region 文本消息显示
                    #region 正常消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");


                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");


                        //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                        //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                        //{
                        //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //}
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //sbRight.AppendLine("var three=document.createElement('div');");
                        string divids = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
                        sbLRC.AppendLine(divRightCopyContent(divids, msg.messageId));
                        sbLRC.AppendLine("node.id='" + divids + "';");
                        sbLRC.AppendLine("node.className='speech right';");

                        string showMsgs = PublicTalkMothed.talkContentReplace(msg.sourceContent);

                        sbLRC.AppendLine("node.innerHTML ='" + showMsgs + "';");

                        sbLRC.AppendLine("first.appendChild(node);");
                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendText", msg.messageId, showMsgs, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                    }
                    else
                    {
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");
                        string useridText = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridText));
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridText + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");

                        //添加用户名称
                        //sbLeft.AppendLine("var username=document.createElement('div');");
                        //sbLeft.AppendLine("username.className='leftUsername';");
                        //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
                        //sbLeft.AppendLine("nodeFirst.appendChild(username);");

                        //sbLeft.AppendLine("var node=document.createElement('div');");
                        string dividText = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
                        sbLRC.AppendLine(divLeftCopyContent(dividText));
                        sbLRC.AppendLine("node.id='" + dividText + "';");
                        sbLRC.AppendLine("node.className='speech left';");

                        string showMsgText = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                        sbLRC.AppendLine("node.innerHTML ='" + showMsgText + "';");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgPicture:
                    #region 图片消息
                    #region 正常消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(msg.sourceContent);
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");


                        //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                        //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                        //{
                        //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //}

                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");



                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='speech right';");

                        //sbRight.AppendLine("var img1 = document.createElement('img');");
                        string imageid = "wlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
                        sbLRC.AppendLine(oneSentImageString(imageid, msg.messageId));

                        sbLRC.AppendLine("img1.src='" + rimgDto.picUrl + "';");
                        sbLRC.AppendLine("img1.style.width='100%';");
                        sbLRC.AppendLine("img1.style.height='100%';");
                        sbLRC.AppendLine("img1.setAttribute('sid', '" + msg.messageId + "');");
                        sbLRC.AppendLine("img1.id='" + imageid + "';");
                        sbLRC.AppendLine("img1.title='双击查看原图';");
                        sbLRC.AppendLine("three.appendChild(img1);");

                        sbLRC.AppendLine("first.appendChild(three);");
                        if (msg.sendsucessorfail == 0)
                        {
                            string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                            string imgLocalPath = pathHtml;
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("0", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                        }

                        sbLRC.AppendLine("document.body.appendChild(first);");

                        sbLRC.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                        sbLRC.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                    }
                    else
                    {
                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");


                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string useridPicture = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridPicture));
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridPicture + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");


                        ////添加用户名称
                        //sbLeftImage.AppendLine("var username=document.createElement('div');");
                        //sbLeftImage.AppendLine("username.className='leftUsername';");
                        //sbLeftImage.AppendLine("username.innerHTML ='" + user.userName + "';");
                        //sbLeftImage.AppendLine("nodeFirst.appendChild(username);");

                        sbLRC.AppendLine("var node=document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");

                        //sbLeftImage.AppendLine("var img = document.createElement('img');");
                        string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
                        sbLRC.AppendLine(oneLeftImageString(imageid));

                        sbLRC.AppendLine("img.style.width='100%';");
                        sbLRC.AppendLine("img.style.height='100%';");
                        sbLRC.AppendLine("img.src='" + rimgDto.picUrl + "';");
                        sbLRC.AppendLine("img.id='" + imageid + "';");
                        sbLRC.AppendLine("img.setAttribute('sid', '" + msg.chatIndex + "');");
                        sbLRC.AppendLine("img.title='双击查看原图';");

                        sbLRC.AppendLine("node.appendChild(img);");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                        sbLRC.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                        sbLRC.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                    }
                    #endregion
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgFile:
                    #region 文件消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                        //判断是否已经下载
                        bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                        bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
                        receive.messageId = msg.messageId;
                        receive.chatIndex = msg.chatIndex;
                        receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                        receive.seId = msg.sessionId;
                        #region 构造发送文件解析
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine(oneSentFile(msg.messageId));
                        sbLRC.Append("first.id='" + msg.messageId + "';");
                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");
                        sbLRC.AppendLine("var seconds=document.createElement('div');");
                        sbLRC.AppendLine("seconds.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("seconds.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(seconds);");
                        sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                        sbLRC.AppendLine("bubbleDiv.className='speech right';");
                        sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='fileDiv';");
                        sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='fileDivOne';");
                        sbLRC.AppendLine("second.appendChild(three);");
                        sbLRC.AppendLine("var four=document.createElement('div');");
                        sbLRC.AppendLine("four.className='fileImg';");
                        sbLRC.AppendLine("three.appendChild(four);");
                        //文件显示图片类型
                        sbLRC.AppendLine("var fileimage = document.createElement('img');");
                        if (receive.fileExtendName == null)
                        {
                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                        }
                        sbLRC.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
                        sbLRC.AppendLine("four.appendChild(fileimage);");
                        sbLRC.AppendLine("var five=document.createElement('div');");
                        sbLRC.AppendLine("five.className='fileName';");
                        sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                        sbLRC.AppendLine("five.innerText='" + fileName + "';");
                        sbLRC.AppendLine("three.appendChild(five);");
                        sbLRC.AppendLine("var six=document.createElement('div');");
                        sbLRC.AppendLine("six.className='fileSize';");
                        sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                        sbLRC.AppendLine("three.appendChild(six);");
                        sbLRC.AppendLine("var seven=document.createElement('div');");
                        sbLRC.AppendLine("seven.className='fileProgressDiv';");
                        sbLRC.AppendLine("second.appendChild(seven);");
                        //进度条
                        sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                        sbLRC.AppendLine("sevenFist.className='processcontainer';");
                        sbLRC.AppendLine("seven.appendChild(sevenFist);");
                        sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                        sbLRC.AppendLine("sevenSecond.className='processbar';");
                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.progressId = progressId;
                        sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                        var txtShowSucessImg = "success";//接收上传状态图片
                        string txtShowDown = "上传成功";//接收上传状态文字
                        string txtOpenOrReceive = "打开";//打开接收文字
                        string txtSaveAs = "打开文件夹";//打开文件夹另存为文字
                        //同一端发送
                        if (isOpenOrReceive)
                        {
                            if (msg.sendsucessorfail == 1)//发送成功
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                txtShowSucessImg = "success";
                                txtShowDown = "上传成功";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                            else//发送失败
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                txtShowSucessImg = "fail";
                                txtShowDown = "上传失败";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                        }
                        //不同端同步的消息
                        else
                        {
                            if (isDownFile)//已经下载
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                txtShowSucessImg = "success";
                                txtShowDown = "下载成功";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                            else//未下载
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                txtShowSucessImg = "reveiving";
                                txtShowDown = "未下载";
                                txtOpenOrReceive = "接收";
                                txtSaveAs = "另存为";
                            }
                        }
                        sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                        sbLRC.AppendLine("var eight=document.createElement('div');");
                        sbLRC.AppendLine("eight.className='fileOperateDiv';");
                        sbLRC.AppendLine("second.appendChild(eight);");
                        sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                        sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                        sbLRC.AppendLine("eight.appendChild(imgSorR);");
                        //接收图片添加
                        sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                        sbLRC.AppendLine("showSOFImg.className='onging';");
                        sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileImgGuid = showImgGuid;
                        sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                        sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                        sbLRC.AppendLine("var night=document.createElement('div');");
                        sbLRC.AppendLine("night.className='fileRorS';");
                        sbLRC.AppendLine("eight.appendChild(night);");
                        //接收中添加文字
                        sbLRC.AppendLine("var nightButton=document.createElement('button');");
                        sbLRC.AppendLine("nightButton.className='fileUploadProgress';");
                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                        receive.fileTextGuid = fileshowText;
                        sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                        sbLRC.AppendLine("night.appendChild(nightButton);");
                        sbLRC.AppendLine("var ten=document.createElement('div');");
                        sbLRC.AppendLine("ten.className='fileOpen';");
                        sbLRC.AppendLine("eight.appendChild(ten);");
                        //打开
                        sbLRC.AppendLine("var btnten=document.createElement('button');");
                        sbLRC.AppendLine("btnten.className='btnOpenFile';");
                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                        sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                        receive.fileOpenGuid = fileOpenguid;
                        sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                        sbLRC.AppendLine("ten.appendChild(btnten);");
                        sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                        sbLRC.AppendLine("var eleven=document.createElement('div');");
                        sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                        sbLRC.AppendLine("eight.appendChild(eleven);");
                        //打开文件夹
                        sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                        sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileDirectoryGuid = fileDirectoryId;
                        sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                        sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                        string setValues = JsonConvert.SerializeObject(receive);
                        sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                        sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                        sbLRC.AppendLine("btnten.value='" + setValues + "';");
                        sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                        //2017-04-06 添加 重发div
                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                        if (msg.sendsucessorfail == 0)
                        {
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("1", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    else
                    {
                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                        //判断是否已经下载
                        bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                        receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                        receive.messageId = msg.messageId;
                        receive.chatIndex = msg.chatIndex;
                        receive.seId = msg.sessionId;
                        #region 构造发送文件解析
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='leftd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        sbLRC.AppendLine("var seconds=document.createElement('div');");
                        sbLRC.AppendLine("seconds.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string useridFile = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridFile));
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridFile + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("seconds.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(seconds);");
                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeshow);");
                        sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                        sbLRC.AppendLine("bubbleDiv.className='speech left';");
                        sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='fileDiv';");
                        sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='fileDivOne';");
                        sbLRC.AppendLine("second.appendChild(three);");
                        sbLRC.AppendLine("var four=document.createElement('div');");
                        sbLRC.AppendLine("four.className='fileImg';");
                        sbLRC.AppendLine("three.appendChild(four);");
                        //文件显示图片类型
                        sbLRC.AppendLine("var fileimage = document.createElement('img');");
                        if (receive.fileExtendName == null)
                        {
                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                        }
                        sbLRC.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                        sbLRC.AppendLine("four.appendChild(fileimage);");
                        sbLRC.AppendLine("var five=document.createElement('div');");
                        sbLRC.AppendLine("five.className='fileName';");
                        sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                        sbLRC.AppendLine("five.innerText='" + fileName + "';");
                        sbLRC.AppendLine("three.appendChild(five);");
                        sbLRC.AppendLine("var six=document.createElement('div');");
                        sbLRC.AppendLine("six.className='fileSize';");
                        sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                        sbLRC.AppendLine("three.appendChild(six);");
                        sbLRC.AppendLine("var seven=document.createElement('div');");
                        sbLRC.AppendLine("seven.className='fileProgressDiv';");
                        sbLRC.AppendLine("second.appendChild(seven);");
                        //进度条
                        sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                        sbLRC.AppendLine("sevenFist.className='processcontainer';");
                        sbLRC.AppendLine("seven.appendChild(sevenFist);");
                        sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                        sbLRC.AppendLine("sevenSecond.className='processbar';");
                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.progressId = progressId;
                        sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                        if (isDownFile == true)
                        {
                            sbLRC.AppendLine("sevenSecond.style.width='100%';");
                        }
                        sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                        sbLRC.AppendLine("var eight=document.createElement('div');");
                        sbLRC.AppendLine("eight.className='fileOperateDiv';");
                        sbLRC.AppendLine("second.appendChild(eight);");
                        sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                        sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                        sbLRC.AppendLine("eight.appendChild(imgSorR);");
                        //接收图片添加
                        sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                        sbLRC.AppendLine("showSOFImg.className='onging';");
                        string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
                        sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileImgGuid = showImgGuid;
                        sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                        sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                        sbLRC.AppendLine("var night=document.createElement('div');");
                        sbLRC.AppendLine("night.className='fileRorS';");
                        sbLRC.AppendLine("eight.appendChild(night);");
                        //接收中添加文字
                        sbLRC.AppendLine("var nightButton=document.createElement('button');");
                        sbLRC.AppendLine("nightButton.className='fileUploadProgressLeft';");
                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                        receive.fileTextGuid = fileshowText;
                        string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
                        sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                        sbLRC.AppendLine("night.appendChild(nightButton);");
                        sbLRC.AppendLine("var ten=document.createElement('div');");
                        sbLRC.AppendLine("ten.className='fileOpen';");
                        sbLRC.AppendLine("eight.appendChild(ten);");
                        //打开
                        sbLRC.AppendLine("var btnten=document.createElement('button');");
                        sbLRC.AppendLine("btnten.className='btnOpenFileLeft';");
                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                        sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                        receive.fileOpenGuid = fileOpenguid;
                        string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
                        sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                        sbLRC.AppendLine("ten.appendChild(btnten);");
                        sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                        sbLRC.AppendLine("var eleven=document.createElement('div');");
                        sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                        sbLRC.AppendLine("eight.appendChild(eleven);");
                        //打开文件夹
                        sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                        sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileDirectoryGuid = fileDirectoryId;
                        sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                        string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
                        sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                        string setValues = JsonConvert.SerializeObject(receive);
                        sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                        sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                        sbLRC.AppendLine("btnten.value='" + setValues + "';");
                        sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgAudio:
                    #region 语音消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        #region 圆形图片Right

                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                        string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");

                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='speech right';");
                        sbLRC.AppendLine("three.id='M" + msg.messageId + "';");
                        sbLRC.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
                        sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                        sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                        sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                        sbLRC.AppendLine("imgmp3.className ='divmp3_img_right';");
                        sbLRC.AppendLine("three.appendChild(imgmp3);");

                        sbLRC.AppendLine("var div3 = document.createElement('div');");
                        sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                        sbLRC.AppendLine("div3.className ='divmp3_contain_right';");
                        sbLRC.AppendLine("three.appendChild(div3);");
                        sbLRC.AppendLine("first.appendChild(three);");
                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                            string imgLocalPath = "file:///" + pathHtml;
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendVoice", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    else
                    {
                        AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");
                        string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                        string isShowGifId = "g" + Guid.NewGuid().GetHashCode();

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");
                        string userid = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(userid));
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");

                        sbLRC.AppendLine("var node=document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");
                        sbLRC.AppendLine("node.id='M" + msg.messageId + "';");
                        sbLRC.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receive.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
                        sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                        sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                        sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                        sbLRC.AppendLine("imgmp3.className ='divmp3_img';");
                        sbLRC.AppendLine("node.appendChild(imgmp3);");

                        sbLRC.AppendLine("var div3 = document.createElement('div');");
                        sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                        sbLRC.AppendLine("div3.className ='divmp3_contain';");
                        sbLRC.AppendLine("node.appendChild(div3);");
                        sbLRC.AppendLine("nodeFirst.appendChild(node);");
                        if (msg.voiceread + "" == "")
                        {
                            //未读圆形提示
                            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var divCircle = document.createElement('div');");
                            sbLRC.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                            sbLRC.AppendLine("divCircle.className ='leftVoice';");
                            sbLRC.AppendLine("var imgCircle = document.createElement('img');");
                            sbLRC.AppendLine("imgCircle.src='" + CirCleImg + "';");
                            sbLRC.AppendLine("divCircle.appendChild(imgCircle);");
                            sbLRC.AppendLine("nodeFirst.appendChild(divCircle);");
                        }

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgAt:
                    #region At消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        //AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(msg.content);
                        var setlist = msg as AntSdkChatMsg.At;
                        string showContent = "";
                        int i = 0;
                        //构造展示消息
                        foreach (var str in setlist.content)
                        {
                            switch (str.type)
                            {
                                //文本
                                case "1001":
                                    showContent += PublicTalkMothed.talkContentReplace(str.content);
                                    break;
                                //@全体成员
                                case "1111":
                                    showContent += "@全体成员";
                                    break;
                                //@个人
                                case "1112":
                                    string strAt = "";
                                    if (str.ids.Count() > 1)
                                    {
                                        foreach (var name in str.names)
                                        {
                                            strAt += "@" + name[0];
                                        }
                                    }
                                    else
                                    {
                                        strAt += "@" + str.names[0];
                                    }
                                    showContent += strAt;
                                    break;
                                //换行
                                case "0000":
                                    showContent += "<br/>";
                                    break;
                                //图片
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(str.content);
                                    string ImgUrl = pictureAndTextMix.picUrl;
                                    showContent += "<img id=\"" + ImgId[i] + "\" src=\"" + ImgUrl + "\" class=\"imgRightProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>";
                                    i++;
                                    break;
                            }
                        }
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");


                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");



                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        //sbRight.AppendLine("var three=document.createElement('div');");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex;
                        sbLRC.AppendLine(divRightCopyContent(divid, msg.messageId));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech right';");

                        string showMsg = showContent;

                        sbLRC.AppendLine("node.innerHTML ='" + showMsg + "';");

                        sbLRC.AppendLine("first.appendChild(node);");

                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            sbLRC.AppendLine(OnceSendAtHistoryMsgDiv("sendAtMixPic", msg.messageId, showContent, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                        }

                        sbLRC.AppendLine("document.body.appendChild(first);");
                    }
                    else
                    {
                        //AtContentDto atDto = JsonConvert.DeserializeObject<AtContentDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        var setlist = msg as AntSdkChatMsg.At;
                        string showContent = "";
                        int i = 0;
                        //构造展示消息
                        foreach (var str in setlist.content)
                        {
                            switch (str.type)
                            {
                                //文本
                                case "1001":
                                    showContent += PublicTalkMothed.talkContentReplace(str.content);
                                    break;
                                //@全体成员
                                case "1111":
                                    showContent += "@全体成员";
                                    break;
                                //@个人
                                case "1112":
                                    string strAt = "";
                                    if (str.ids.Count() > 1)
                                    {
                                        foreach (var name in str.names)
                                        {
                                            strAt += "@" + name[0];
                                        }
                                    }
                                    else
                                    {
                                        strAt += "@" + str.names[0];
                                    }
                                    showContent += strAt;
                                    break;
                                //换行
                                case "0000":
                                    showContent += "<br/>";
                                    break;
                                //图片
                                case "1002":
                                    PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(str.content);
                                    string ImgUrl = pictureAndTextMix.picUrl;
                                    showContent += "<img id=\"" + ImgId[i] + "\" src=\"" + ImgUrl + "\" class=\"imgLeftProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>";
                                    i++;
                                    break;
                            }
                        }
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");


                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.id='" + msg.chatIndex + "';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string userid = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(userid));
                        sbLRC.AppendLine("img.src='" + getPathImage(GroupMembers, msg) + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + userid + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");

                        //添加用户名称
                        //sbLeft.AppendLine("var username=document.createElement('div');");
                        //sbLeft.AppendLine("username.className='leftUsername';");
                        //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
                        //sbLeft.AppendLine("nodeFirst.appendChild(username);");

                        //sbLeft.AppendLine("var node=document.createElement('div');");
                        string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "") + msg.chatIndex; ;
                        sbLRC.AppendLine(divLeftCopyContent(divid));
                        sbLRC.AppendLine("node.id='" + divid + "';");
                        sbLRC.AppendLine("node.className='speech left';");

                        string showMsg = showContent;

                        sbLRC.AppendLine("node.innerHTML ='" + showMsg + "';");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.Revocation:
                    sbLRC.AppendLine("var firsts=document.createElement('div');");
                    sbLRC.AppendLine("firsts.className='speechCenterTips';");
                    sbLRC.AppendLine("firsts.id='" + msg.messageId + "';");
                    sbLRC.AppendLine("firsts.innerHTML ='" + msg.sourceContent + "';");
                    sbLRC.AppendLine("document.body.appendChild(firsts);");
                    break;
            }
            return sbLRC.ToString();
        }
        /// <summary>
        /// 群组阅后即焚消息批量处理
        /// </summary>
        /// <returns></returns>
        public static string LeftGroupToGroupShowBurnMessage(SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            string pathImages = "";
            //获取接收者头像

            var user = getGroupMembersUser(GroupMembers, msg);
            if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId && msg.os != (int)GlobalVariable.OSType.PC)
                msg.sendsucessorfail = 1;
            StringBuilder sbLRC = new StringBuilder();
            switch (Convert.ToInt32(/*TODO:AntSdk_Modify:msg.MTP*/msg.MsgType))
            {
                case (int)AntSdkMsgType.ChatMsgText:
                    #region 阅后即焚消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");


                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");


                        //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                        //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                        //{
                        //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //}
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='speech right';");

                        string showMsg = PublicTalkMothed.talkContentReplace(msg.sourceContent);

                        sbLRC.AppendLine("three.innerHTML ='" + showMsg + "';");


                        sbLRC.AppendLine("first.appendChild(three);");
                        if (msg.sendsucessorfail == 0)
                        {
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendText", msg.messageId, showMsg, Guid.NewGuid().ToString().Replace("-", ""), "", msg.sourceContent, ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                    }
                    else
                    {
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");

                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string useridTextBurn = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridTextBurn));
                        sbLRC.AppendLine("img.src='" + defaultHeaderImage + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridTextBurn + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");

                        //添加用户名称
                        //sbLeft.AppendLine("var username=document.createElement('div');");
                        //sbLeft.AppendLine("username.className='leftUsername';");
                        //sbLeft.AppendLine("username.innerHTML ='" + user.userName + "';");
                        //sbLeft.AppendLine("nodeFirst.appendChild(username);");

                        sbLRC.AppendLine("var node=document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");

                        string showMsgText = PublicTalkMothed.talkContentReplace(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                        sbLRC.AppendLine("node.innerHTML ='" + showMsgText + "';");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgPicture:
                    #region 阅后即焚消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(msg.sourceContent);
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");

                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");

                        //string imgUrl = AntSdkService.AntSdkCurrentUserInfo.picture;
                        //if (imgUrl == "pack://application:,,,/AntennaChat;Component/Images/36-头像.png" || imgUrl.Contains("pack://application:,,,/AntennaChat"))
                        //{
                        //    imgUrl = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/36-头像-copy.png").Replace(@"\", @"/").Replace(" ", "%20");
                        //}

                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");



                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='speech right';");

                        sbLRC.AppendLine("var img1 = document.createElement('img');");


                        sbLRC.AppendLine("img1.src='" + rimgDto.picUrl + "';");

                        sbLRC.AppendLine("img1.style.width='100%';");
                        sbLRC.AppendLine("img1.style.height='100%';");

                        sbLRC.AppendLine("img1.id='wlc" + Guid.NewGuid().ToString().Replace("-", "") + "';");
                        sbLRC.AppendLine("img1.setAttribute('sid', '" + msg.chatIndex + "');");
                        sbLRC.AppendLine("img1.title='双击查看原图';");
                        sbLRC.AppendLine("three.appendChild(img1);");

                        sbLRC.AppendLine("first.appendChild(three);");
                        if (msg.sendsucessorfail == 0)
                        {
                            string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                            string imgLocalPath = pathHtml;
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("2", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", "", ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");

                        sbLRC.AppendLine("img1.addEventListener('dblclick',clickImgCall);");
                        sbLRC.AppendLine("img1.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                    }
                    else
                    {
                        SendImageDto rimgDto = JsonConvert.DeserializeObject<SendImageDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");


                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string useridPictureBurn = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridPictureBurn));
                        sbLRC.AppendLine("img.src='" + defaultHeaderImage + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridPictureBurn + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");


                        ////添加用户名称
                        //sbLeftImage.AppendLine("var username=document.createElement('div');");
                        //sbLeftImage.AppendLine("username.className='leftUsername';");
                        //sbLeftImage.AppendLine("username.innerHTML ='" + user.userName + "';");
                        //sbLeftImage.AppendLine("nodeFirst.appendChild(username);");

                        sbLRC.AppendLine("var node=document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");

                        //sbLeftImage.AppendLine("var img = document.createElement('img');");
                        string imageid = "rwlc" + Guid.NewGuid().ToString().Replace("-", "") + "";
                        sbLRC.AppendLine(oneImageString(imageid));

                        sbLRC.AppendLine("img.style.width='100%';");
                        sbLRC.AppendLine("img.style.height='100%';");
                        sbLRC.AppendLine("img.src='" + rimgDto.picUrl + "';");
                        sbLRC.AppendLine("img.id='" + imageid + "';");
                        sbLRC.AppendLine("img.setAttribute('sid', '" + msg.chatIndex + "');");
                        sbLRC.AppendLine("img.title='双击查看原图';");

                        sbLRC.AppendLine("node.appendChild(img);");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                        sbLRC.AppendLine("img.addEventListener('dblclick',clickImgCall);");
                        sbLRC.AppendLine("img.addEventListener('load',function(e){window.scrollTo(0,document.body.scrollHeight);})");
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgFile:
                    #region 阅后即焚消息
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(msg.sourceContent);
                        //判断是否已经下载
                        bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                        bool isOpenOrReceive = IsOpenOrReceive(msg.SENDORRECEIVE, msg.sendsucessorfail.ToString(), msg.uploadOrDownPath);
                        receive.messageId = msg.messageId;
                        receive.chatIndex = msg.chatIndex;
                        receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                        receive.seId = msg.sessionId;
                        receive.flag = "1";
                        #region 构造发送文件解析
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");
                        sbLRC.AppendLine("var seconds=document.createElement('div');");
                        sbLRC.AppendLine("seconds.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("seconds.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(seconds);");
                        sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                        sbLRC.AppendLine("bubbleDiv.className='speech right';");
                        sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='fileDiv';");
                        sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='fileDivOne';");
                        sbLRC.AppendLine("second.appendChild(three);");
                        sbLRC.AppendLine("var four=document.createElement('div');");
                        sbLRC.AppendLine("four.className='fileImg';");
                        sbLRC.AppendLine("three.appendChild(four);");
                        //文件显示图片类型
                        sbLRC.AppendLine("var fileimage = document.createElement('img');");
                        if (receive.fileExtendName == null)
                        {
                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                        }
                        sbLRC.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, receive.localOrServerPath) + "';");
                        sbLRC.AppendLine("four.appendChild(fileimage);");
                        sbLRC.AppendLine("var five=document.createElement('div');");
                        sbLRC.AppendLine("five.className='fileName';");
                        sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                        sbLRC.AppendLine("five.innerText='" + fileName + "';");
                        sbLRC.AppendLine("three.appendChild(five);");
                        sbLRC.AppendLine("var six=document.createElement('div');");
                        sbLRC.AppendLine("six.className='fileSize';");
                        sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                        sbLRC.AppendLine("three.appendChild(six);");
                        sbLRC.AppendLine("var seven=document.createElement('div');");
                        sbLRC.AppendLine("seven.className='fileProgressDiv';");
                        sbLRC.AppendLine("second.appendChild(seven);");
                        //进度条
                        sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                        sbLRC.AppendLine("sevenFist.className='processcontainer';");
                        sbLRC.AppendLine("seven.appendChild(sevenFist);");
                        sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                        sbLRC.AppendLine("sevenSecond.className='processbar';");
                        var progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.progressId = progressId;
                        sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                        var txtShowSucessImg = "success";//接收上传状态图片
                        var txtShowDown = "上传成功";//接收上传状态文字
                        var txtOpenOrReceive = "打开";//打开接收文字
                        var txtSaveAs = "打开文件夹";//打开文件夹另存为文字
                        //同一端发送
                        if (isOpenOrReceive)
                        {
                            if (msg.sendsucessorfail == 1)//发送成功
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                txtShowSucessImg = "success";
                                txtShowDown = "上传成功";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                            else//发送失败
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                txtShowSucessImg = "fail";
                                txtShowDown = "上传失败";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                        }
                        //不同端同步的消息
                        else
                        {
                            if (isDownFile)//已经下载
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='100%';");
                                txtShowSucessImg = "success";
                                txtShowDown = "下载成功";
                                txtOpenOrReceive = "打开";
                                txtSaveAs = "打开文件夹";
                            }
                            else//未下载
                            {
                                sbLRC.AppendLine("sevenSecond.style.width='0%';");
                                txtShowSucessImg = "reveiving";
                                txtShowDown = "未下载";
                                txtOpenOrReceive = "接收";
                                txtSaveAs = "另存为";
                            }
                        }
                        sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                        sbLRC.AppendLine("var eight=document.createElement('div');");
                        sbLRC.AppendLine("eight.className='fileOperateDiv';");
                        sbLRC.AppendLine("second.appendChild(eight);");
                        sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                        sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                        sbLRC.AppendLine("eight.appendChild(imgSorR);");
                        //接收图片添加
                        sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                        sbLRC.AppendLine("showSOFImg.className='onging';");
                        sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileImgGuid = showImgGuid;
                        sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                        sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                        sbLRC.AppendLine("var night=document.createElement('div');");
                        sbLRC.AppendLine("night.className='fileRorS';");
                        sbLRC.AppendLine("eight.appendChild(night);");
                        //接收中添加文字
                        sbLRC.AppendLine("var nightButton=document.createElement('button');");
                        sbLRC.AppendLine("nightButton.className='fileUploadProgress';");
                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                        receive.fileTextGuid = fileshowText;
                        sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                        sbLRC.AppendLine("night.appendChild(nightButton);");
                        sbLRC.AppendLine("var ten=document.createElement('div');");
                        sbLRC.AppendLine("ten.className='fileOpen';");
                        sbLRC.AppendLine("eight.appendChild(ten);");
                        //打开
                        sbLRC.AppendLine("var btnten=document.createElement('button');");
                        sbLRC.AppendLine("btnten.className='btnOpenFile';");
                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                        sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                        receive.fileOpenGuid = fileOpenguid;
                        sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                        sbLRC.AppendLine("ten.appendChild(btnten);");
                        sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                        sbLRC.AppendLine("var eleven=document.createElement('div');");
                        sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                        sbLRC.AppendLine("eight.appendChild(eleven);");
                        //打开文件夹
                        sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                        sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectory hover';");
                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileDirectoryGuid = fileDirectoryId;
                        sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                        string setValues = JsonConvert.SerializeObject(receive);
                        sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                        sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                        sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                        sbLRC.AppendLine("btnten.value='" + setValues + "';");
                        sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                        string imageTipId = Guid.NewGuid().ToString().Replace("-", "");
                        if (msg.sendsucessorfail == 0)
                        {
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("3", msg.messageId, msg.uploadOrDownPath, Guid.NewGuid().ToString().Replace("-", ""), "sending" + imageTipId, "", ""));
                        }
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    else
                    {
                        ReceiveOrUploadFileDto receive = JsonConvert.DeserializeObject<ReceiveOrUploadFileDto>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        //判断是否已经下载
                        bool isDownFile = IsReceiveDownFileMethod(msg.uploadOrDownPath);
                        receive.downloadPath = receive.localOrServerPath = msg.uploadOrDownPath;
                        receive.messageId = msg.messageId;
                        receive.chatIndex = msg.chatIndex;
                        receive.seId = msg.sessionId;
                        receive.flag = "1";
                        #region 构造发送文件解析
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='leftd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        sbLRC.AppendLine("var seconds=document.createElement('div');");
                        sbLRC.AppendLine("seconds.className='leftimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        string useridBurn = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(useridBurn));
                        sbLRC.AppendLine("img.src='" + defaultHeaderImage + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + useridBurn + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("seconds.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(seconds);");
                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeshow);");
                        sbLRC.AppendLine("var bubbleDiv=document.createElement('div');");
                        sbLRC.AppendLine("bubbleDiv.className='speech left';");
                        sbLRC.AppendLine("first.appendChild(bubbleDiv);");
                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='fileDiv';");
                        sbLRC.AppendLine("bubbleDiv.appendChild(second);");
                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='fileDivOne';");
                        sbLRC.AppendLine("second.appendChild(three);");
                        sbLRC.AppendLine("var four=document.createElement('div');");
                        sbLRC.AppendLine("four.className='fileImg';");
                        sbLRC.AppendLine("three.appendChild(four);");
                        //文件显示图片类型
                        sbLRC.AppendLine("var fileimage = document.createElement('img');");
                        if (receive.fileExtendName == null)
                        {
                            receive.fileExtendName = receive.fileName.Substring((receive.fileName.LastIndexOf('.') + 1), receive.fileName.Length - 1 - receive.fileName.LastIndexOf('.'));
                        }
                        sbLRC.AppendLine("fileimage.src='" + fileShowImage.showImageHtmlPath(receive.fileExtendName, "") + "';");
                        sbLRC.AppendLine("four.appendChild(fileimage);");
                        sbLRC.AppendLine("var five=document.createElement('div');");
                        sbLRC.AppendLine("five.className='fileName';");
                        sbLRC.AppendLine("five.title='" + receive.fileName + "';");
                        string fileName = receive.fileName.Length > 12 ? receive.fileName.Substring(0, 10) + "..." : receive.fileName;
                        sbLRC.AppendLine("five.innerText='" + fileName + "';");
                        sbLRC.AppendLine("three.appendChild(five);");
                        sbLRC.AppendLine("var six=document.createElement('div');");
                        sbLRC.AppendLine("six.className='fileSize';");
                        sbLRC.AppendLine("six.innerText='" + FileSize(receive.Size) + "';");
                        sbLRC.AppendLine("three.appendChild(six);");
                        sbLRC.AppendLine("var seven=document.createElement('div');");
                        sbLRC.AppendLine("seven.className='fileProgressDiv';");
                        sbLRC.AppendLine("second.appendChild(seven);");
                        //进度条
                        sbLRC.AppendLine("var sevenFist=document.createElement('div');");
                        sbLRC.AppendLine("sevenFist.className='processcontainer';");
                        sbLRC.AppendLine("seven.appendChild(sevenFist);");
                        sbLRC.AppendLine("var sevenSecond=document.createElement('div');");
                        sbLRC.AppendLine("sevenSecond.className='processbar';");
                        string progressId = "pro" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.progressId = progressId;
                        sbLRC.AppendLine("sevenSecond.id='" + progressId + "';");
                        if (isDownFile == true)
                        {
                            sbLRC.AppendLine("sevenSecond.style.width='100%';");
                        }
                        sbLRC.AppendLine("sevenFist.appendChild(sevenSecond);");
                        sbLRC.AppendLine("var eight=document.createElement('div');");
                        sbLRC.AppendLine("eight.className='fileOperateDiv';");
                        sbLRC.AppendLine("second.appendChild(eight);");
                        sbLRC.AppendLine("var imgSorR=document.createElement('div');");
                        sbLRC.AppendLine("imgSorR.className='fileRorSImage';");
                        sbLRC.AppendLine("eight.appendChild(imgSorR);");
                        //接收图片添加
                        sbLRC.AppendLine("var showSOFImg=document.createElement('img');");
                        sbLRC.AppendLine("showSOFImg.className='onging';");
                        string txtShowSucessImg = (isDownFile == true ? "success" : "reveiving");
                        sbLRC.AppendLine("showSOFImg.src='" + fileShowImage.showImageHtmlPath(txtShowSucessImg, "") + "';");
                        string showImgGuid = "zxy" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileImgGuid = showImgGuid;
                        sbLRC.AppendLine("showSOFImg.id='" + showImgGuid + "';");
                        sbLRC.AppendLine("imgSorR.appendChild(showSOFImg);");
                        sbLRC.AppendLine("var night=document.createElement('div');");
                        sbLRC.AppendLine("night.className='fileRorS';");
                        sbLRC.AppendLine("eight.appendChild(night);");
                        //接收中添加文字
                        sbLRC.AppendLine("var nightButton=document.createElement('button');");
                        sbLRC.AppendLine("nightButton.className='fileUploadProgressLeft';");
                        string fileshowText = "ldh" + Guid.NewGuid().ToString().Replace("-", "");
                        sbLRC.AppendLine("nightButton.id='" + fileshowText + "';");
                        receive.fileTextGuid = fileshowText;
                        string txtShowDown = (isDownFile == true ? "下载成功" : "未下载");
                        sbLRC.AppendLine("nightButton.innerHTML='" + txtShowDown + "';");
                        sbLRC.AppendLine("night.appendChild(nightButton);");
                        sbLRC.AppendLine("var ten=document.createElement('div');");
                        sbLRC.AppendLine("ten.className='fileOpen';");
                        sbLRC.AppendLine("eight.appendChild(ten);");
                        //打开
                        sbLRC.AppendLine("var btnten=document.createElement('button');");
                        sbLRC.AppendLine("btnten.className='btnOpenFileLeft';");
                        string fileOpenguid = "gxc" + Guid.NewGuid().ToString().Replace(" - ", "");
                        sbLRC.AppendLine("btnten.id='" + fileOpenguid + "';");
                        receive.fileOpenGuid = fileOpenguid;
                        string txtOpenOrReceive = (isDownFile == true ? "打开" : "接收");
                        sbLRC.AppendLine("btnten.innerHTML='" + txtOpenOrReceive + "';");
                        sbLRC.AppendLine("ten.appendChild(btnten);");
                        sbLRC.AppendLine("btnten.addEventListener('click',clickBtnCall);");
                        sbLRC.AppendLine("var eleven=document.createElement('div');");
                        sbLRC.AppendLine("eleven.className='fileOpenDirectory';");
                        sbLRC.AppendLine("eight.appendChild(eleven);");
                        //打开文件夹
                        sbLRC.AppendLine("var btnEleven=document.createElement('button');");
                        sbLRC.AppendLine("btnEleven.className='btnOpenFileDirectoryLeft hover';");
                        string fileDirectoryId = "dct" + Guid.NewGuid().ToString().Replace("-", "");
                        receive.fileDirectoryGuid = fileDirectoryId;
                        sbLRC.AppendLine("btnEleven.id='" + receive.fileDirectoryGuid + "';");
                        string txtSaveAs = (isDownFile == true ? "打开文件夹" : "另存为");
                        sbLRC.AppendLine("btnEleven.innerHTML='" + txtSaveAs + "';");
                        string setValues = JsonConvert.SerializeObject(receive);
                        sbLRC.AppendLine("btnEleven.value='" + setValues + "';");
                        sbLRC.AppendLine("eleven.appendChild(btnEleven);");
                        sbLRC.AppendLine("btnten.value='" + setValues + "';");
                        sbLRC.AppendLine("btnEleven.addEventListener('click',clickFilePathCall);");
                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    #endregion
                    break;
                case (int)AntSdkMsgType.ChatMsgAudio:
                    #region 阅后即焚语音
                    if (AntSdkService.AntSdkCurrentUserInfo.userId == msg.sendUserId)
                    {
                        AntSdkChatMsg.Audio_content receive = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);
                        #region 圆形图片Right
                        sbLRC.AppendLine("var first=document.createElement('div');");
                        sbLRC.AppendLine("first.className='rightd';");
                        sbLRC.AppendLine("first.id='" + msg.messageId + "';");
                        string isShowReadImgId = "v" + Guid.NewGuid().GetHashCode();
                        string isShowGifId = "g" + Guid.NewGuid().GetHashCode();
                        //时间显示层
                        sbLRC.AppendLine("var timeDiv=document.createElement('div');");
                        sbLRC.AppendLine("timeDiv.className='rightTimeStyle';");
                        sbLRC.AppendLine("timeDiv.innerHTML='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("first.appendChild(timeDiv);");

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='rightimg';");
                        sbLRC.AppendLine("var img = document.createElement('img');");
                        sbLRC.AppendLine("img.src='" + HeadImgUrl() + "';");
                        sbLRC.AppendLine("img.className='divcss5';");
                        sbLRC.AppendLine("second.appendChild(img);");
                        sbLRC.AppendLine("first.appendChild(second);");

                        sbLRC.AppendLine("var three=document.createElement('div');");
                        sbLRC.AppendLine("three.className='speech right';");
                        sbLRC.AppendLine("three.id='M" + msg.messageId + "';");
                        sbLRC.AppendLine("three.setAttribute('onmousedown','VoiceRightMenuMethod(event)');three.setAttribute('selfmethods','one');three.setAttribute('audioUrl','" + receive.audioUrl.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace(@"\", @"\\").Replace("'", "&#39") + "');three.setAttribute('isRead','0');three.setAttribute('isShowImg','" + isShowReadImgId + "');three.setAttribute('isShowGif','" + isShowGifId + "');three.setAttribute('isLeftOrRight','1');");

                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音right.png").Replace(@"\", @"/").Replace(" ", "%20");
                        sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                        sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                        sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                        sbLRC.AppendLine("imgmp3.className ='divmp3_img_right';");
                        sbLRC.AppendLine("three.appendChild(imgmp3);");

                        sbLRC.AppendLine("var div3 = document.createElement('div');");
                        sbLRC.AppendLine("div3.innerHTML ='" + receive.duration + "″" + "';");
                        sbLRC.AppendLine("div3.className ='divmp3_contain_right';");
                        sbLRC.AppendLine("three.appendChild(div3);");
                        sbLRC.AppendLine("first.appendChild(three);");
                        //是否失败
                        if (msg.sendsucessorfail == 0)
                        {
                            string pathHtml = msg.uploadOrDownPath.Replace(@"\", @"/");
                            string imgLocalPath = "file:///" + pathHtml;
                            sbLRC.AppendLine(OnceSendHistoryMsgDiv("sendVoice", msg.messageId, imgLocalPath, Guid.NewGuid().ToString().Replace("-", ""), "", msg.uploadOrDownPath, ""));
                        }

                        sbLRC.AppendLine("document.body.appendChild(first);");
                        #endregion
                    }
                    else
                    {
                        AntSdkChatMsg.Audio_content receives = JsonConvert.DeserializeObject<AntSdkChatMsg.Audio_content>(/*TODO:AntSdk_Modify:msg.content*/msg.sourceContent);

                        sbLRC.AppendLine("var nodeFirst=document.createElement('div');");
                        sbLRC.AppendLine("nodeFirst.className='leftd';");
                        sbLRC.AppendLine("nodeFirst.id='" + msg.messageId + "';");
                        string isShowReadImgId = "v" + Guid.NewGuid().ToString().GetHashCode();
                        string isShowGifId = "g" + Guid.NewGuid().ToString().GetHashCode();

                        sbLRC.AppendLine("var second=document.createElement('div');");
                        sbLRC.AppendLine("second.className='leftimg';");
                        sbLRC.AppendLine("nodeFirst.appendChild(second);");

                        //时间显示
                        sbLRC.AppendLine("var timeshow = document.createElement('div');");
                        sbLRC.AppendLine("timeshow.className='leftTimeText';");
                        sbLRC.AppendLine("timeshow.innerHTML ='" + timeComparison(msg.sendTime) + "';");
                        sbLRC.AppendLine("nodeFirst.appendChild(timeshow);");
                        string userid = NewUserIdString(user.userId);
                        sbLRC.AppendLine(groupImageString(userid));
                        sbLRC.AppendLine("img.src='" + defaultHeaderImage + "';");
                        sbLRC.AppendLine("img.className='divcss5Left';");
                        sbLRC.AppendLine("img.id='" + user.userId + "';");
                        sbLRC.AppendLine("img.addEventListener('click',clickImgCallUserId);");
                        sbLRC.AppendLine("second.appendChild(img);");

                        sbLRC.AppendLine("var node=document.createElement('div');");
                        sbLRC.AppendLine("node.className='speech left';");
                        sbLRC.AppendLine("node.id='M" + msg.messageId + "';");
                        sbLRC.AppendLine("node.setAttribute('onmousedown','VoiceRightMenuMethod(event)');node.setAttribute('selfmethods','one');node.setAttribute('audioUrl','" + receives.audioUrl + "');node.setAttribute('isRead','0');node.setAttribute('isShowImg','" + isShowReadImgId + "');node.setAttribute('isShowGif','" + isShowGifId + "');");

                        string musicImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/语音left.png").Replace(@"\", @"/").Replace(" ", "%20");
                        sbLRC.AppendLine("var imgmp3 = document.createElement('img');");
                        sbLRC.AppendLine("imgmp3.id ='" + isShowGifId + "';");
                        sbLRC.AppendLine("imgmp3.src='" + musicImg + "';");
                        sbLRC.AppendLine("imgmp3.className ='divmp3_img';");
                        sbLRC.AppendLine("node.appendChild(imgmp3);");

                        sbLRC.AppendLine("var div3 = document.createElement('div');");
                        sbLRC.AppendLine("div3.innerHTML ='" + receives.duration + "″" + "';");
                        sbLRC.AppendLine("div3.className ='divmp3_contain';");
                        sbLRC.AppendLine("node.appendChild(div3);");

                        sbLRC.AppendLine("nodeFirst.appendChild(node);");

                        if (msg.voiceread + "" == "")
                        {
                            //未读圆形提示
                            string CirCleImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/htmlImages/未读语音提示.png").Replace(@"\", @"/").Replace(" ", "%20");
                            sbLRC.AppendLine("var divCircle = document.createElement('div');");
                            sbLRC.AppendLine("divCircle.id ='" + isShowReadImgId + "';");
                            sbLRC.AppendLine("divCircle.className ='leftVoice';");
                            sbLRC.AppendLine("var imgCircle = document.createElement('img');");
                            sbLRC.AppendLine("imgCircle.src='" + CirCleImg + "';");
                            sbLRC.AppendLine("divCircle.appendChild(imgCircle);");
                            sbLRC.AppendLine("nodeFirst.appendChild(divCircle);");
                        }
                        sbLRC.AppendLine("document.body.appendChild(nodeFirst);");
                    }
                    #endregion
                    break;
            }
            return sbLRC.ToString();
        }
        /// <summary>
        /// 默认头像
        /// </summary>
        /// <returns></returns>
        public static string HeadImgUrl()
        {
            #region 获取接收者头像 New
            string pathImages = "";
            //获取接收者头像
            var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == AntSdkService.AntSdkCurrentUserInfo.userId);
            if (listUser == null)
            {
                AntSdkContact_User cus = AntSdkService.AntSdkListContactsEntity.users.SingleOrDefault(m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId);
                if (cus != null)
                {
                    if (string.IsNullOrEmpty(cus.picture + ""))
                    {
                        pathImages = "file:///" +
                                     (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/")
                                         .Replace(" ", "%20");
                    }
                    else
                    {
                        pathImages = cus.picture;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(listUser.Url))
                {
                    pathImages = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/默认头像.png").Replace(@"\", @"/").Replace(" ", "%20");
                }
                else
                {
                    pathImages = "file:///" + listUser.Url.Replace(@"\", @"/").Replace(" ", "%20");
                }
            }
            #endregion
            return pathImages;
        }
        /// <summary>
        /// 判断文件是否下载
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="uploadOrDownPath"></param>
        /// <returns></returns>
        public static bool IsDownFileMethod(string fileUrl, string uploadOrDownPath, string sendsucessorfail)
        {
            bool isDownFile = true && (fileUrl.StartsWith("http") && (uploadOrDownPath.StartsWith("file:///") || (uploadOrDownPath + "").Trim().Length > 0 || (sendsucessorfail == "1")) == true);
            return isDownFile;
        }
        public static bool LeftIsDownFileMethod(string uploadOrDownPath, string sendsucessorfail)
        {
            bool isDownFile = true && (sendsucessorfail == "1" && (uploadOrDownPath + "").Trim().Length > 0);
            return isDownFile;
        }
        /// <summary>
        /// 单人是否已经接收文件成功
        /// </summary>
        /// <param name="uploadOrDownPath"></param>
        /// <returns></returns>
        public static bool IsReceiveDownFileMethod(string uploadOrDownPath)
        {
            if (string.IsNullOrWhiteSpace(uploadOrDownPath)) return false;
            if (System.IO.File.Exists(uploadOrDownPath)) return true;
            return false;
        }
        /// <summary>
        /// 判断文件是否上传成功
        /// </summary>
        /// <param name="sendsucessorfail"></param>
        /// <returns></returns>
        public static bool IsUploadFileSucessOrFail(string sendsucessorfail, string uploadOrDownPath)
        {
            bool isUploadFile = true ? sendsucessorfail == "1" && (uploadOrDownPath + "").Trim().Length > 0 : sendsucessorfail == "0";
            return isUploadFile;
        }
        /// <summary>
        /// 发送文件上传是否成功
        /// </summary>
        /// <param name="sendsucessorfail"></param>
        /// <returns></returns>
        public static bool IsSendUploadFileSucessOrFail(string sendsucessorfail, string uploadOrDownPath)
        {
            bool isUploadFile = true ? sendsucessorfail == "1" && (uploadOrDownPath + "").Trim().Length > 0 : sendsucessorfail == "0";
            return isUploadFile;
        }
        /// <summary>
        /// 群组
        /// </summary>
        /// <param name="sendsucessorfail"></param>
        /// <param name="uploadOrDownPath"></param>
        /// <returns></returns>
        public static bool IsGroupUploadFileSucessOrFail(string sendsucessorfail, string uploadOrDownPath)
        {
            bool isUploadFile = true ? sendsucessorfail == "1" && (uploadOrDownPath + "").Trim().Length > 0 : sendsucessorfail == "0";
            return isUploadFile;
        }
        public static bool IsGroupBurnUpLoadFileSucessOrFail(string sendsucessorfail, string sendorreceive)
        {
            bool isUploadFile = false;
            if ((sendsucessorfail == "0" && sendorreceive == "1") || (sendsucessorfail == "1" && sendorreceive == "1"))
            {
                isUploadFile = true;
            }
            else
            {
                isUploadFile = false;
            }
            return isUploadFile;
        }
        /// <summary>
        /// 文件打开接收状态
        /// </summary>
        /// <param name="SendOrReceive">发送或者接收</param>
        /// <param name="SendSucessOrFail">发送或者接收是否成功</param>
        /// <param name="UploadOrDownPath">接收或者上传路径 接收的消息要赋值</param>
        /// <returns></returns>
        public static bool IsOpenOrReceive(string SendOrReceive, string SendSucessOrFail, string UploadOrDownPath = "")
        {
            bool isUploadFile = false;
            switch (SendOrReceive)
            {
                //接受消息判断
                case "0":
                    if (UploadOrDownPath.Length > 0)
                    {
                        isUploadFile = true;
                    }
                    else
                    {
                        isUploadFile = false;
                    }
                    break;
                //发送消息判断
                case "1":
                    if (SendSucessOrFail == "1" && SendOrReceive == "1" && (UploadOrDownPath + "").Length == 0)
                    {
                        isUploadFile = false;
                    }
                    else
                    {
                        isUploadFile = true;
                    }
                    break;
            }
            return isUploadFile;
        }


    }
}