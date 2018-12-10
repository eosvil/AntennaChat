using Antenna.Framework;
using Antenna.Model.PictureAndTextMix;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Antenna.Model.ModelEnum;

namespace AntennaChat.CefSealedHelper.OneAndGroupCommon
{
    public class PictureAndTextMixMethod
    {
        /// <summary>
        /// 投票默认图片
        /// </summary>
        public static string VoteImg = "file:///" + (AppDomain.CurrentDomain.BaseDirectory + "Images/vote70X70.png").Replace(@"\", @"/").Replace(" ", "%20");
        /// <summary>
        /// 接收者头像
        /// </summary>
        /// <returns></returns>
        public static string HeadImgUrl()
        {
            #region 获取接收者头像
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
        /// <summary>
        /// 图文混合重发
        /// </summary>
        /// <param name="method"></param>
        /// <param name="messageid"></param>
        /// <param name="path"></param>
        /// <param name="imageTipId"></param>
        /// <param name="imageSendingId"></param>
        /// <param name="preValue"></param>
        /// <param name="atPreValues"></param>
        /// <returns></returns>
        public static string OnceSendMixPicDiv(string method, string messageid, string path, string imageTipId, string imageSendingId, string preValue, string atPreValues)
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
            sb.AppendLine("imgtip.addEventListener('click',removeDivs);");
            sb.AppendLine("imgtip.className='onceSendImg';");
            sb.AppendLine("imgtip.src='../images/发送失败.png';");
            sb.AppendLine("imgtip.title='点击重新发送';");
            sb.AppendLine("sendonce.appendChild(imgtip);");
            sb.AppendLine("first.appendChild(sendonce);");
            return sb.ToString();
        }
        public static string OnceSendHistoryPicDiv(string method, string messageid, string path, string imageTipId, string imageSendingId, string preValue, string atPreValues)
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
        /// 获取获取对应群员信息
        /// </summary>
        /// <param name="GroupMembers"></param>
        /// <returns></returns>
        public static AntSdkGroupMember getGroupMembersUser(List<AntSdkGroupMember> GroupMembers, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
        {
            string pathImages = "";
            //获取接收者头像
            var listUser = GlobalVariable.ContactHeadImage.UserHeadImages.SingleOrDefault(m => m.UserID == msg.sendUserId);
            //var users = GroupMembers != null && GroupMembers.Count > 0 ? GroupMembers.SingleOrDefault(m => m.userId == msg.sendUserId) : null;
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
                    userName = cuss.status == 0 && cuss.state == 0 ? cuss.userName + "（停用）":cuss.userName ,
                    userNum = cuss.userNum,
                    picture = cuss.picture,
                    position = cuss.position
                };

            }
            //}
            return users;
        }
        /// <summary>
        /// 获取群聊个人头像
        /// </summary>
        /// <param name="GroupMembers"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string getPathImage(List<AntSdkGroupMember> GroupMembers, SDK.AntSdk.AntModels.AntSdkChatMsg.ChatBase msg)
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
        /// 图文混合构造
        /// </summary>
        /// <param name="mixEnum"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static PictureAndTextMixDto PictureAndTextStruct(PictureAndTextMixEnum mixEnum, string content = "", string bGuid = "", string imgPath = "", string imgId = "", object obj = null)
        {
            PictureAndTextMixDto mixDto = new PictureAndTextMixDto();
            mixDto.type = mixEnum;
            mixDto.content = content;
            mixDto.ImgGuid = bGuid;
            mixDto.ImgPath = imgPath;
            mixDto.ImgId = imgId;
            mixDto.obj = obj;
            return mixDto;
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
        /// <summary>
        /// 图片地址拼接
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string ImgUrlSplit(string Url)
        {
            int split = Url.LastIndexOf(".");
            string subString = Url.Substring(0, split);
            string suffix = Url.Substring(split, Url.Length - split);
            string imgUrl = subString + "_80x80" + suffix;
            return imgUrl;
        }
    }
}
