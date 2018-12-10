using CefSharp.Wpf;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntennaChat.ViewModel.Talk
{
    public class SendOrReceiveMsgStateOperate
    {
        /// <summary>
        /// 单人聊天消息状态更改
        /// </summary>
        /// <param name="messageId">消息id</param>
        /// <returns>状态： 1成功,0失败</returns>
        public static int UpdateSendMsgPointStatus(string messageId,string chatIndex)
        {
            //更新状态
            T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
            var result = t_chat.UpdateReSendMsgState(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId,chatIndex);
            return result;
        }
        /// <summary>
        /// 群聊聊天消息状态更改
        /// </summary>
        /// <param name="isBurn">是否阅后即焚</param>
        /// <param name="messageId">消息id</param>
        public static int UpdateSendMsgGroup(AntSdkburnMsg.isBurnMsg isBurn, string messageId,string chatIndex)
        {
            int result = 0;
            switch (isBurn)
            {
                case AntSdkburnMsg.isBurnMsg.notBurn:
                    T_Chat_Message_GroupDAL t_chat = new T_Chat_Message_GroupDAL();
                    result=t_chat.UpdateReSendMsgState(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, chatIndex);
                    break;
                case AntSdkburnMsg.isBurnMsg.yesBurn:
                    T_Chat_Message_GroupBurnDAL t_chat_burn = new T_Chat_Message_GroupBurnDAL();
                    result=t_chat_burn.UpdateReSendMsgState(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, chatIndex);
                    break;
            }
            return result;
        }
        /// <summary>
        /// 更新消息内容
        /// </summary>
        /// <param name="isBurn"></param>
        /// <param name="messageId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int UpdateContent(AntSdkburnMsg.isBurnMsg isBurn, PointOrGroupFrom.PointOrGroup from, string messageId, string content)
        {
            int result = 0;
            switch (from)
            {
                case PointOrGroupFrom.PointOrGroup.Point:
                    T_Chat_MessageDAL t_chatPoint = new T_Chat_MessageDAL();
                    result=t_chatPoint.UpdateContent(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, content);
                    break;
                case PointOrGroupFrom.PointOrGroup.Group:
                    if(isBurn==AntSdkburnMsg.isBurnMsg.notBurn)
                    {
                        T_Chat_Message_GroupDAL t_chatGroup = new T_Chat_Message_GroupDAL();
                        t_chatGroup.UpdateContent(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, content);
                    }
                    else
                    {
                        T_Chat_Message_GroupBurnDAL t_chatGroupBurn = new T_Chat_Message_GroupBurnDAL();
                        t_chatGroupBurn.UpdateContent(messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, content);
                    }
                    break;
            }
            return result;
        }
        /// <summary>
        /// 更改界面消息显示状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="cef">cef</param>
        /// <param name="messageId">消息Id</param>
        public static void UpdateUiState(AntSdkburnMsg.isSendSucessOrFail state, ChromiumWebBrowser cef,string messageId)
        {
            switch(state)
            {
                case AntSdkburnMsg.isSendSucessOrFail.fail:
                    PublicTalkMothed.HiddenMsgDiv(cef, messageId);
                    break;
                case AntSdkburnMsg.isSendSucessOrFail.sucess:
                    PublicTalkMothed.HiddenMsgDiv(cef, messageId);
                    break;
            }
        }
    }
}