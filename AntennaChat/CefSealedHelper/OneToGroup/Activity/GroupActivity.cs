using AntennaChat.CefSealedHelper.OneAndGroupCommon;
using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using SDK.AntSdk.AntModels;
using SDK.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDK.AntSdk.AntModels.AntSdkChatMsg;

namespace AntennaChat.CefSealedHelper.OneToGroup.Activity
{
    public class GroupActivity
    {
        public static void RightGroupSendActivity(ChromiumWebBrowser cef, ChatBase msg)
        {
            //显示内容解析
            Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

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

            //活动内容展示层
            sbRight.AppendLine("var node = document.createElement('div')");
            sbRight.AppendLine("node.className='voteBgColor speech right';");

            //设置活动Id
            sbRight.AppendLine("node.id='" + receive.activityId + "';");
            //事件监听
            sbRight.AppendLine("node.addEventListener('click',clickActivityShow);");

            //活动默认图片显示层
            sbRight.AppendLine("var imgVote=document.createElement('img');");
            sbRight.AppendLine("imgVote.className='activityImg';");
            sbRight.AppendLine("imgVote.src='" + PictureAndTextMixMethod.ImgUrlSplit(receive.picture) + "';");
            sbRight.AppendLine("node.appendChild(imgVote);");

            //活动title显示层
            sbRight.AppendLine("var voteTitle = document.createElement('div');");
            sbRight.AppendLine("voteTitle.className='voteTitle';");
            sbRight.AppendLine("voteTitle.innerHTML ='" + receive.theme + "';");
            sbRight.AppendLine("node.appendChild(voteTitle);");

            //换行1
            sbRight.AppendLine("var  newLineOne= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineOne);");

            //活动时间
            sbRight.AppendLine("var voteInFirst = document.createElement('div');");
            sbRight.AppendLine("voteInFirst.className='activityTP';");
            sbRight.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';");
            sbRight.AppendLine("node.appendChild(voteInFirst);");

            //换行2
            sbRight.AppendLine("var  newLineTwo= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineTwo);");

            //活动地点
            sbRight.AppendLine("var voteInSecond = document.createElement('div');");
            sbRight.AppendLine("voteInSecond.className='activityTP';");
            sbRight.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
            sbRight.AppendLine("node.appendChild(voteInSecond);");

            //换行4
            sbRight.AppendLine("var  newLineFour= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineFour);");

            sbRight.AppendLine("first.appendChild(node);");

            sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("}");
            sbRight.AppendLine("myFunction();");
            Task task = cef.EvaluateScriptAsync(sbRight.ToString());
            task.Wait();

        }
        public static void RightGroupScrollActivity(ChromiumWebBrowser cef, ChatBase msg)
        {
            //显示内容解析
            Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
            StringBuilder sbRight = new StringBuilder();
            sbRight.AppendLine("function myFunction()");

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

            //活动内容展示层
            sbRight.AppendLine("var node = document.createElement('div')");
            sbRight.AppendLine("node.className='voteBgColor speech right';");

            //设置活动Id
            sbRight.AppendLine("node.id='" + receive.activityId + "';");
            //事件监听
            sbRight.AppendLine("node.addEventListener('click',clickActivityShow);");

            //活动默认图片显示层
            sbRight.AppendLine("var imgVote=document.createElement('img');");
            sbRight.AppendLine("imgVote.className='activityImg';");
            sbRight.AppendLine("imgVote.src='" + receive.picture + "';");
            sbRight.AppendLine("node.appendChild(imgVote);");

            //活动title显示层
            sbRight.AppendLine("var voteTitle = document.createElement('div');");
            sbRight.AppendLine("voteTitle.className='voteTitle';");
            sbRight.AppendLine("voteTitle.innerHTML ='" + PictureAndTextMixMethod.ImgUrlSplit(receive.picture) + "';");
            sbRight.AppendLine("node.appendChild(voteTitle);");

            //换行1
            sbRight.AppendLine("var  newLineOne= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineOne);");

            //活动时间
            sbRight.AppendLine("var voteInFirst = document.createElement('div');");
            sbRight.AppendLine("voteInFirst.className='activityTP';");
            sbRight.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';");
            sbRight.AppendLine("node.appendChild(voteInFirst);");

            //换行2
            sbRight.AppendLine("var  newLineTwo= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineTwo);");

            //活动地点
            sbRight.AppendLine("var voteInSecond = document.createElement('div');");
            sbRight.AppendLine("voteInSecond.className='activityTP';");
            sbRight.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
            sbRight.AppendLine("node.appendChild(voteInSecond);");

            //换行4
            sbRight.AppendLine("var  newLineFour= document.createElement('br');");
            sbRight.AppendLine("node.appendChild(newLineFour);");

            sbRight.AppendLine("first.appendChild(node);");

            //获取body层
            sbRight.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbRight.AppendLine("listbody.insertBefore(first,listbody.childNodes[0]);}");
            //sbRight.AppendLine("document.body.appendChild(first);");
            sbRight.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbRight.ToString());
        }
        public static void LeftGroupShowActivity(ChromiumWebBrowser cef, ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = PictureAndTextMixMethod.getGroupMembersUser(GroupMembers, msg);
            //显示内容解析
            Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");

            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + PictureAndTextMixMethod.getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //活动内容展示层
            sbLeft.AppendLine("var node = document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");
            //设置活动Id
            sbLeft.AppendLine("node.id='" + receive.activityId + "';");
            //事件监听
            sbLeft.AppendLine("node.addEventListener('click',clickActivityShow);");

            //活动默认图片显示层
            sbLeft.AppendLine("var imgVote=document.createElement('img');");
            sbLeft.AppendLine("imgVote.className='activityImg';");
            sbLeft.AppendLine("imgVote.src='" + PictureAndTextMixMethod.ImgUrlSplit(receive.picture) + "';");
            sbLeft.AppendLine("node.appendChild(imgVote);");

            //活动title显示层
            sbLeft.AppendLine("var voteTitle = document.createElement('div');");
            sbLeft.AppendLine("voteTitle.className='voteTitle';");
            sbLeft.AppendLine("voteTitle.innerHTML ='" + receive.theme + "';");
            sbLeft.AppendLine("node.appendChild(voteTitle);");

            //换行1
            sbLeft.AppendLine("var  newLineOne= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineOne);");

            sbLeft.AppendLine("var voteInFirst = document.createElement('div');");
            sbLeft.AppendLine("voteInFirst.className='activityTP';");
            sbLeft.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';");;
            sbLeft.AppendLine("node.appendChild(voteInFirst);");

            //换行2
            sbLeft.AppendLine("var  newLineTwo= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineTwo);");

            //活动地点
            sbLeft.AppendLine("var voteInSecond = document.createElement('div');");
            sbLeft.AppendLine("voteInSecond.className='activityTP';");
            sbLeft.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
            sbLeft.AppendLine("node.appendChild(voteInSecond);");

            //换行4
            sbLeft.AppendLine("var  newLineFour= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineFour);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            sbLeft.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeft.AppendLine("}");
            sbLeft.AppendLine("myFunction();");
            Task task = cef.EvaluateScriptAsync(sbLeft.ToString());
            task.Wait();
        }
        public static void LeftGroupScrollActivity(ChromiumWebBrowser cef, ChatBase msg, List<AntSdkGroupMember> GroupMembers)
        {
            AntSdkGroupMember user = PictureAndTextMixMethod.getGroupMembersUser(GroupMembers, msg);
            //显示内容解析
            Activity_content receive = JsonConvert.DeserializeObject<Activity_content>(msg.sourceContent);
            StringBuilder sbLeft = new StringBuilder();
            sbLeft.AppendLine("function myFunction()");

            sbLeft.AppendLine("{ var nodeFirst=document.createElement('div');");
            sbLeft.AppendLine("nodeFirst.className='leftd';");
            sbLeft.AppendLine("nodeFirst.id='" + msg.messageId + "';");

            //头像显示层
            sbLeft.AppendLine("var second=document.createElement('div');");
            sbLeft.AppendLine("second.className='leftimg';");
            sbLeft.AppendLine("var img = document.createElement('img');");
            sbLeft.AppendLine("img.src='" + PictureAndTextMixMethod.getPathImage(GroupMembers, msg) + "';");
            sbLeft.AppendLine("img.className='divcss5Left';");
            sbLeft.AppendLine("img.id='" + user.userId + "';");
            sbLeft.AppendLine("img.addEventListener('click',clickImgCallUserId);");
            sbLeft.AppendLine("second.appendChild(img);");
            sbLeft.AppendLine("nodeFirst.appendChild(second);");


            //时间显示
            sbLeft.AppendLine("var timeshow = document.createElement('div');");
            sbLeft.AppendLine("timeshow.className='leftTimeText';");
            sbLeft.AppendLine("timeshow.innerHTML ='" + user.userNum + user.userName + "  " + PictureAndTextMixMethod.timeComparison(msg.sendTime) + "';");
            sbLeft.AppendLine("nodeFirst.appendChild(timeshow);");

            //活动内容展示层
            sbLeft.AppendLine("var node = document.createElement('div');");
            sbLeft.AppendLine("node.className='speech left';");

            //设置活动Id
            sbLeft.AppendLine("node.id='" + receive.activityId + "';");
            //事件监听
            sbLeft.AppendLine("node.addEventListener('click',clickActivityShow);");

            //活动默认图片显示层
            sbLeft.AppendLine("var imgVote=document.createElement('img');");
            sbLeft.AppendLine("imgVote.className='activityImg';");

            sbLeft.AppendLine("imgVote.src='" + PictureAndTextMixMethod.ImgUrlSplit(receive.picture) + "';");
            sbLeft.AppendLine("node.appendChild(imgVote);");

            //活动title显示层
            sbLeft.AppendLine("var voteTitle = document.createElement('div');");
            sbLeft.AppendLine("voteTitle.className='voteTitle';");
            sbLeft.AppendLine("voteTitle.innerHTML ='" + receive.theme + "';");
            sbLeft.AppendLine("node.appendChild(voteTitle);");

            //换行1
            sbLeft.AppendLine("var  newLineOne= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineOne);");

            sbLeft.AppendLine("var voteInFirst = document.createElement('div');");
            sbLeft.AppendLine("voteInFirst.className='activityTP';");
            sbLeft.AppendLine("voteInFirst.innerHTML ='时间：" + receive.startTime + "';"); ;
            sbLeft.AppendLine("node.appendChild(voteInFirst);");

            //换行2
            sbLeft.AppendLine("var  newLineTwo= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineTwo);");

            //活动地点
            sbLeft.AppendLine("var voteInSecond = document.createElement('div');");
            sbLeft.AppendLine("voteInSecond.className='activityTP';");
            sbLeft.AppendLine("voteInSecond.innerHTML ='地点：" + receive.address + "';");
            sbLeft.AppendLine("node.appendChild(voteInSecond);");

            //换行4
            sbLeft.AppendLine("var  newLineFour= document.createElement('br');");
            sbLeft.AppendLine("node.appendChild(newLineFour);");

            sbLeft.AppendLine("nodeFirst.appendChild(node);");

            //获取body层
            sbLeft.AppendLine("var listbody = document.getElementById('bodydiv');");
            sbLeft.AppendLine("listbody.insertBefore(nodeFirst,listbody.childNodes[0]);}");
            //sbLeft.AppendLine("document.body.appendChild(nodeFirst);");
            sbLeft.AppendLine("myFunction();");
            cef.ExecuteScriptAsync(sbLeft.ToString());
        }
    }
}
