using Antenna.Model;
using Antenna.Model.PictureAndTextMix;
using AntennaChat.CefSealedHelper.OneAndGroupCommon;
using AntennaChat.ViewModel.Talk;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Antenna.Model.ModelEnum;
using static SDK.AntSdk.AntModels.AntSdkChatMsg;

namespace AntennaChat.CefSealedHelper.OneToOne.PictureAndTextMix
{
    /// <summary>
    /// 发送图文混合类
    /// </summary>
    public class OneRightSendPicAndText
    {
        public static void RightSendPicAndTextMix(ChromiumWebBrowser cef, string messageId, List<MixMessageObjDto> obj, MessageStateArg arg, MixMsg mixMsgClass)
        {
            #region 图文混合Right
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + DateTime.Now.ToString("HH:mm:ss") + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            //头像显示层
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");

            //图文混合展示层
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbRight.AppendLine(PublicTalkMothed.divRightCopyContent(divid, messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            //sbRight.AppendLine("var node = document.createElement('div')");
            sbRight.AppendLine("node.className='speech right';");

            //图文混合内部构造
            StringBuilder sbInside = new StringBuilder();
            int i = 0;
            foreach (var list in obj)
            {
                //var type = list as MixMessageBase;
                switch (list.type)
                {
                    //文本
                    case "1001":
                        //var text = list as MixMessageDto;
                        sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                        break;
                    //图片
                    case "1002":
                        //var images = list as MixMessageDto;
                        var contentImg = JsonConvert.DeserializeObject<PictureDto>(list.content.ToString());
                        sbInside.Append("<img id=\""+ mixMsgClass.TagDto[i].PreGuid + "\" src=\""+ contentImg.picUrl+ "\" class=\"imgRightProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                        i++;
                        break;
                    //换行
                    case "0000":
                        sbInside.Append("<br/>");
                        break;
                }
            }
            sbRight.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
            sbRight.AppendLine("first.appendChild(node);");

            //重发div
            //sbRight.AppendLine(OnceSendMsgDiv("sendText", arg.MessageId, sendStr, imageTipId, imageSendingId, msgStr, ""));
            sbRight.AppendLine(PictureAndTextMixMethod.OnceSendMixPicDiv("sendMixPic", arg.MessageId, "",arg.RepeatId, arg.SendIngId, "", ""));
            sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("}");

            sbRight.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }
        public static void LeftShowPicAndTextMix(ChromiumWebBrowser cef, ChatBase msg,AntSdkContact_User user,List<string> imgId)
        {
            //显示内容解析
            List<MixMessage_content> receive = JsonConvert.DeserializeObject<List<MixMessage_content>>(msg.sourceContent);
            string pathImage = user.copyPicture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");

            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            //头像显示层
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
            sbLeft.AppendLine("timeshow.innerHTML ='" +user.userNum + user.userName + "  " +PictureAndTextMixMethod.timeComparison(msg.sendTime) +  "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //图文混合展示层
            //sbLeft.AppendLine("var node = document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbLeft.AppendLine(PublicTalkMothed.divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");
            int i = 0;
            //图文混合内部构造
            StringBuilder sbInside = new StringBuilder();
            foreach(var list in receive)
            {
                switch(list.type)
                {
                    //文本
                    case "1001":
                        sbInside.Append(PublicTalkMothed.talkContentReplace(list.content?.ToString()));
                        break;
                    case "1002":
                        PictureAndTextMixContentDto pictureAndTextMix = JsonConvert.DeserializeObject<PictureAndTextMixContentDto>(list.content?.ToString());
                        if (msg.IsSetImgLoadComplete)
                        {
                            sbInside.Append("<img id=\"" + imgId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgLeftProportion\" onload=\"scrollToend(event)\" ondblclick=\"myFunctions(event)\"/>");
                        }
                        else
                        {
                            sbInside.Append("<img id=\"" + imgId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgLeftProportion\"  ondblclick=\"myFunctions(event)\"/>");
                        }
                        i++;
                        break;
                    case "0000":
                        sbInside.Append("<br/>");
                        break;
                }
            }
            sbLeft.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(node);");
            sbLeft.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeft.AppendLine("}");
            sbLeft.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbLeft.ToString());

        }
        public static void RightScrollPicAndTextMix(ChromiumWebBrowser cef, ChatBase msg, List<string> imageId)
        {
            #region 右边展示
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");
            //显示内容解析
            List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);
            sbRight.AppendLine("{ var first=document.createElement('div');");
            sbRight.AppendLine("first.className='rightd';");
            sbRight.AppendLine("first.id='" + msg.messageId + "';");

            //时间显示层
            sbRight.AppendLine("var timeDiv=document.createElement('div');");
            sbRight.AppendLine("timeDiv.className='rightTimeStyle';");
            sbRight.AppendLine("timeDiv.innerHTML='" + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
            sbRight.AppendLine("first.appendChild(timeDiv);");

            //头像显示层
            sbRight.AppendLine("var second=document.createElement('div');");
            sbRight.AppendLine("second.className='rightimg';");
            sbRight.AppendLine("var img = document.createElement('img');");
            sbRight.AppendLine("img.src='" + PictureAndTextMixMethod.HeadImgUrl() + "';");
            sbRight.AppendLine("img.className='divcss5';");
            sbRight.AppendLine("second.appendChild(img);");
            sbRight.AppendLine("first.appendChild(second);");
            int i = 0;
            //图文混合展示层
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbRight.AppendLine(PublicTalkMothed.divRightCopyContent(divid, msg.messageId));
            sbRight.AppendLine("node.id='" + divid + "';");
            //sbRight.AppendLine("var node = document.createElement('div')");
            sbRight.AppendLine("node.className='speech right';");

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
                        sbInside.Append("<img id=\"" + imageId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgRightProportion\"  ondblclick=\"myFunctions(event)\"/>");
                        i++;
                        break;
                    //换行
                    case "0000":
                        sbInside.Append("<br/>");
                        break;
                }
            }
            sbRight.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
            sbRight.AppendLine("first.appendChild(node);");
            //发送失败判断
            if(msg.sendsucessorfail==0)
            {
                string guid = Guid.NewGuid().ToString().Replace("-", "");

                sbRight.AppendLine(PictureAndTextMixMethod.OnceSendHistoryPicDiv("sendMixPic", msg.messageId, "", guid, "sending" + guid, "", ""));
            }
            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            //sbRight.AppendLine("document.body.appendChild(first);");

            sbRight.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbRight.ToString());
            #endregion
        }
        public static void LeftScrollPicAndTextMix(ChromiumWebBrowser cef, ChatBase msg, AntSdkContact_User user, List<string> imageId)
        {
            #region 左边展示
            string pathImage = user.copyPicture;
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");
            //显示内容解析
            List<MixMessageObjDto> receive = JsonConvert.DeserializeObject<List<MixMessageObjDto>>(msg.sourceContent);

            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            //头像显示层
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
            sbLeft.AppendLine("timeshow.innerHTML ='" + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");
            int i = 0;
            //图文混合展示层
            //sbLeft.AppendLine("var node = document.createElement('div');");
            string divid = "copy" + Guid.NewGuid().ToString().Replace("-", "");
            sbLeft.AppendLine(PublicTalkMothed.divLeftCopyContent(divid));
            sbLeft.AppendLine("node.id='" + divid + "';");
            sbLeft.AppendLine("node.className='speech left';");

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
                        sbInside.Append("<img id=\"" + imageId[i] + "\" src=\"" + pictureAndTextMix.picUrl + "\" class=\"imgLeftProportion\" ondblclick=\"myFunctions(event)\"/>");
                        break;
                    case "0000":
                        sbInside.Append("<br/>");
                        break;
                }
            }
            sbLeft.AppendLine("node.innerHTML ='" + sbInside.ToString() + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(node);");
            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");
            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeft.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbLeft.ToString());
            #endregion
        }
    }
}