using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDK.Service.Models;
using SDK.Service.Properties;
using System.IO;
using SDK.Service.Internal.Http;

namespace SDK.Service
{
    /// <summary>
    /// 相关HTTP方法
    /// </summary>
    internal class SdkHttpService
    {
        public SdkHttpService()
        {
            if (SdkService.MdsdkhttpMethod == null)
            {
                SdkService.MdsdkhttpMethod = new Service.SdkHttpMethod();
            }
        }

        public event EventHandler TokenErrorEvent;

        /// <summary>
        /// SDK_HTTP请求统一入口
        /// </summary>
        /// <typeparam name="TIn"> 输入参数实体类型</typeparam>
        /// <typeparam name="TOut">输出参数实体类型</typeparam>
        /// <param name="methodName">方法名</param>
        /// <param name="input">输入参数</param>
        /// <param name="output">输出参数</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errMsg">错误信息</param>
        /// <param name="requestType">请求类型（默认为post）</param>
        /// <returns></returns>
        private bool HttpCommonMethod<TIn, TOut>(string methodName, TIn input, ref TOut output, ref int errorCode, ref string errMsg
            , SdkEnumCollection.RequestMethod requestType = SdkEnumCollection.RequestMethod.POST)
        {
            var httpCommon = new HttpCommon<TIn, TOut>();
            if (TokenErrorEvent != null)
            {
                httpCommon.TokenErrorEvent += (s, e) =>
                    TokenErrorEvent.Invoke(s, e);
            }
            //调用公共方法处理
            return httpCommon.HttpCommonMethod(methodName, input, ref output, ref errorCode, ref errMsg, requestType);
        }

        #region 具体的功能接口

        /// <summary>
        /// 登录接口
        /// </summary>
        public bool GetToken(GetTokenInput input, ref string output, ref int errorCode, ref string errMsg)
        {
            input.appSecret = SdkService.SdkSysParam.Appsecret;
            input.sdkType = (int)SdkEnumCollection.OSType.PC;
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetToken
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        public bool GetMqttConnectConfig(GetConnectConfigInput input, ref GetConnectConfigOutput output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetMqttParam
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        public bool UpdateUserInfo(UpdateUserInfoInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateUserInfo
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 添加用户黑名单
        /// </summary>
        public bool AddBlacklist(AddBlacklistInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.AddBlacklist
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 查询黑名单ID列表
        /// </summary>
        public bool FindBlacklists(FindBlacklistsInput input, ref List<string> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.FindBlacklists
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 移除用户黑名单
        /// </summary>
        public bool DelBlacklist(DelBlacklistInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.DelBlacklist
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }


        /// <summary>
        /// 删除聊天室
        /// </summary>
        public bool DeleteChatRoom(DeleteChatRoomInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.DeleteChatRoom
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        ///创建聊天室
        /// </summary>
        public bool CreateChatRoom(CreateChatRoomInput input, ref string output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.CreateChatRoom
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///添加聊天室成员
        /// </summary>
        public bool AddChatRoomMembers(AddChatRoomMembersInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.AddChatRoomMembers
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///删除聊天室成员
        /// </summary>
        public bool DeleteChatRoomMembers(DeleteChatRoomMembersInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.DeleteChatRoomMembers
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///更新成聊天室员属性
        /// </summary>
        public bool UpdateChatRoomMembers(UpdateChatRoomMemberInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateChatRoomMember
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///修改聊天室信息
        /// </summary>
        public bool UpdateChatRoom(UpdateChatRoomInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateChatRoom
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///成员退出聊天室
        /// </summary>
        public bool ExitChatRoom(ExitChatRoomInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.ExitChatRoom
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        ///查询用户所在聊天室，返回聊天室列表
        /// </summary>
        public bool FindRooms(FindRoomsInput input, ref List<FindRoomsOutput> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.FindRooms
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 查询单个聊天室详细信息
        /// </summary>
        public bool GetChatRoomInfo(GetChatRoomInfoInput input, ref GetChatRoomInfoOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetChatRoomInfo
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 查询聊天室所有成员基本信息
        /// </summary>
        public bool FindRoomMembers(FindRoomMembersInput input, ref List<FindRoomMembersOutput> output,
            ref int errorCode, ref string errMsg)
        {
            return
                HttpCommonMethod(SdkService.MdsdkhttpMethod.FindRoomMembers
                    , input, ref output, ref errorCode, ref errMsg
                    , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 查询聊天室单个成员详细信息
        /// </summary>
        public bool GetRoomMemberInfo(GetRoomMemberInfoInput input, ref GetRoomMemberInfoOutput output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetRoomMemberInfo
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 查询app下的所有聊天室
        /// </summary>
        public bool FindAllRooms(ref List<FindRoomsOutput> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod<object, List<FindRoomsOutput>>(SdkService.MdsdkhttpMethod.FindAllRooms
                , null, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 邀请加入聊天室
        /// </summary>
        public bool InviteJoinRoom(InviteJoinRoomInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.InviteJoinRoom
                , input, ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 处理邀请
        /// </summary>
        public bool HandleInvite(HandleInviteInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.HandleInvite
                , input, ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 查询用户所在聊天室，返回聊天室列表以及用户个性化设置
        /// </summary>
        public bool FindIndividRooms(FindIndividRoomsInput input, ref List<FindIndividRoomsOutput> output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.FindIndividRooms
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 设置聊天室的接收消息类型
        /// </summary>
        public bool SetRoomReceiveType(SetRoomReceiveTypeInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.SetRoomReceiveType
                , input, ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 设置用户的接收消息类型
        /// </summary>
        public bool SetUserReceiveType(SetUserReceiveTypeInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.SetUserReceiveType
                , input, ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 查询离线消息(客户端上线调用http接口)
        /// </summary>
        public bool QueryOfflineMsg(QueryOfflineMsgInput input, ref object output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.QueryOfflineMsg
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 查询离线消息(客户端上线调用http接口)
        /// </summary>
        public bool SynchronusMsgs(SynchronusMsgInput input, ref List<SynchronusMsgOutput> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.SynchronusMsgs
                , input, ref output, ref errorCode, ref errMsg
                , SdkEnumCollection.RequestMethod.POST);
        }
        /// <summary>
        /// 手机端切换运行状态[POST]
        /// </summary>
        /// <param name="input">手机端切换运行状态输入</param>
        /// <param name="output">手机端切换运行状态输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool ChangeAppRunStatus(ChangeAppRunStatusInput input,
            ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.ChangeAppRunStatus, input,
                ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 更新小米、华为、魅族、信鸽的信息[POST]
        /// </summary>
        /// <param name="input">更新小米、华为、魅族、信鸽的信息输入</param>
        /// <param name="output">更新小米、华为、魅族、信鸽的信息输出</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>更新小米、华为、魅族、信鸽的信息输出</returns>
        public bool UpdatePushDeviceToken(UpdatePushDeviceTokenInput input, ref BaseOutput output, ref int errorCode,
            ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdatePushDeviceToken, input,
                ref output, ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 获取用户的讨论组列表[GET]
        /// </summary>
        /// <param name="input">获取用户的讨论组列表输入</param>
        /// <param name="output">获取用户的讨论组列表输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool GetGroupList(GetGroupListInput input, ref List<GetGroupListOutput> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetGroupList, input,
                ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 获取讨论组成员信息[GET]
        /// </summary>
        /// <param name="input">获取讨论组成员信息输入</param>
        /// <param name="output">获取讨论组成员信息输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool GetGroupMembers(GetGroupMembersInput input, ref List<GetGroupMembersOutput> output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetGroupMembers, input,
                ref output, ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 退出讨论组[POST]
        /// </summary>
        /// <param name="input">退出讨论组输入</param>
        /// <param name="output">退出讨论组输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool GroupExitor(GroupExitorInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GroupExitor, input, ref output,
                ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 群主转让[PUT]
        /// </summary>
        /// <param name="input">群主转让输入</param>
        /// <param name="output">群主转让输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool GroupOwnerChange(GroupOwnerChangeInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GroupOwnerChange, input,
                ref output, ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.PUT);
        }
        /// <summary>
        /// 群设置管理员[PUT]
        /// </summary>
        /// <param name="input">管理员设置输入</param>
        /// <param name="output">管理员设置输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool GroupManagerSet(GroupManagerChangeInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GroupManagerSet, input,
               ref output, ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 创建讨论组[POST]
        /// </summary>
        /// <param name="input">创建讨论组输入</param>
        /// <param name="output">创建讨论组输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool CreateGroup(CreateGroupInput input, ref CreateGroupOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.CreateGroup, input, ref output,
                ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 更新讨论组信息[PUT]
        /// </summary>
        /// <param name="input">更新讨论组信息输入</param>
        /// <param name="output">更新讨论组信息输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool UpdateGroup(UpdateGroupInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateGroup, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.PUT);
        }

        /// <summary>
        /// 更新用户在讨论组的设置[PUT]
        /// </summary>
        /// <param name="input">更新用户在讨论组的设置输入</param>
        /// <param name="output">更新用户在讨论组的设置输出</param>
        /// <param name="errMsg">错误欣喜</param>
        /// <returns>是否成功</returns>
        public bool UpdateGroupConfig(UpdateGroupConfigInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateGroupConfig, input,
                ref output, ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.PUT);
        }

        /// <summary>
        /// 解散讨论组[DELETE]
        /// </summary>
        /// <param name="input">解散讨论组输入</param>
        /// <param name="output">解散讨论组输出</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool DissolveGroup(DissolveGroupInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.DissolveGroup, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.DELETE);
        }

        /// <summary>
        /// 获取群的所有公告
        /// </summary>
        public bool GetGroupNotifications(GetGroupNotificationsInput input, ref List<GetGroupNotificationsOutput> output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetGroupNotifications, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 通过ID获取群公告
        /// </summary>
        public bool GetNotificationsById(GetNotificationsByIdInput input, ref GetNotificationsByIdOutput output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetNotificationsById, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 添加群公告（群主才有权限）[POST]
        /// </summary>
        public bool AddNotifications(AddNotificationsInput input, ref AddNotificationsOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.AddNotifications, input, ref output,
                ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 用户修改公告状态为已读
        /// </summary>
        public bool UpdateNotificationsState(UpdateNotificationsStateInput input, ref BaseOutput output,
            ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateNotificationsState, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.PUT);
        }

        /// <summary>
        /// 删除群公告(群主才有权限)
        /// </summary>
        public bool DeleteNotificationsById(DeleteNotificationsByIdInput input, ref BaseOutput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.UpdateNotificationsState, input, ref output,
                ref errorCode, ref errMsg, SdkEnumCollection.RequestMethod.DELETE);
        }

        #region 投票
        /// <summary>
        /// 创建群投票
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CreateGroupVoting(CreateGroupVoteInput input,ref GetVoteInfoOutput output, string groupId, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod(string.Format(SdkService.MdsdkhttpMethod.CreateGroupVotings, groupId), input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 获取投票详情
        /// </summary>
        /// <param name="output"></param>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetVoteInfo(ref GetVoteInfoOutput output, int voteId,string userId, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GetVoteInfoOutput>(
                    string.Format(SdkService.MdsdkhttpMethod.GetVoteInfo, voteId,userId), null, ref output,
                    ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 获取所有弃权票信息
        /// </summary>
        /// <param name="output"></param>
        /// <param name="voteId">投票活动ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetGroupAbstentionVote(ref GroupAbstentionVoteOutput output, int voteId, ref int errorCode, ref string errorMsg)
        {
            //var output = new AntSdkBaseOutput();
            return HttpCommonMethod<object, GroupAbstentionVoteOutput>(
                string.Format(SdkService.MdsdkhttpMethod.GetGroupAbstentionVote, voteId), null, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 根据群ID获取群的所有投票活动
        /// </summary>
        /// <param name="output"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetGroupVotes(GetGroupVotesInput input, ref GetGroupVotesOutput output, string groupId, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GetGroupVotesOutput>(
                string.Format(SdkService.MdsdkhttpMethod.GetGroupVotes, groupId), input, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 根据ID删除投票活动
        /// </summary>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool DeleteGroupVote(DeteleGroupVoteInput input, int voteId, ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(string.Format(SdkService.MdsdkhttpMethod.DeleteGroupVote, voteId), input, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.DELETE);
        }

        /// <summary>
        /// 提交群投票选项
        /// </summary>
        /// <param name="input"></param>
        /// <param name="voteId"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool SubmitGroupVoteOptions(GroupVoteOptionInput input, int voteId, ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(string.Format(SdkService.MdsdkhttpMethod.SubmitGroupVoteOptions, voteId), input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.POST);
        }
        #endregion

        #region 活动
        /// <summary>
        /// 发布活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupId">群ID</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ReleaseGroupActivity(ReleaseGroupActivityInput input, string groupId, ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.ReleaseGroupActivity, input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 根据群ID获取群的有活动
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetGroupActivitys(GetGroupActivitysInput input, ref GetGroupActivitysOutput output, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GetGroupActivitysOutput>(SdkService.MdsdkhttpMethod.GetGroupActivitys, input, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 获取活动详情
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetActivityInfo(GetGroupActivityDetailsInput input, ref GroupActivityDetail output, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GroupActivityDetail>(SdkService.MdsdkhttpMethod.GetGroupActivityDetails, input, ref output,
                    ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }
        /// <summary>
        /// 根据ID删除活动
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool DeleteGroupActivity(DeleteGroupActivityInput input,ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.DeleteGroupActivity, input, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.DELETE);
        }

        /// <summary>
        /// 活动参与者列表
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input">input</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool GetGroupActivityParticipators(GetGroupActivityParticipatorInput input, ref GetGroupActivityParticipatorsOutput output, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GetGroupActivityParticipatorsOutput>(SdkService.MdsdkhttpMethod.GetGroupActivityParticipators, input, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        /// <summary>
        /// 提交参与活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public bool ParticipateActivities(ParticipateActivitiesInput input,ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.ParticipateActivities, input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.POST);
        }

        /// <summary>
        /// 获取活动默认主题图片
        /// </summary>
        /// <param name="output"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetActivityImages(ref GetGroupActivityDefaultImageOutput output, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod<object, GetGroupActivityDefaultImageOutput>(SdkService.MdsdkhttpMethod.GetActivityImages, null, ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }

        #endregion

        #region 打卡
        /// <summary>
        /// 确认打卡
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool ConfirmVerify(ConfirmPunchInput input,  ref int errorCode, ref string errorMsg)
        {
            var output = new BaseOutput();
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.ConfirmVerify, input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.POST);
        }
        /// <summary>
        /// 发布活动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output">打卡记录数据</param>
        /// <param name="errorCode">错误代码</param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GetPunchClocks(GetPunchClocksInput input, ref GetPunchClocksOutput output, ref int errorCode, ref string errorMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetPunchClocks, input,
                ref output, ref errorCode, ref errorMsg, SdkEnumCollection.RequestMethod.GET);
        }
        #endregion

        /// <summary>
        /// 消息漫游[POST]
        /// </summary>
        /// <param name="input">消息漫游输入</param>
        /// <param name="output">消息漫游输出</param>
        /// <param name="errorCode">错误代码（特定情况）</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public bool RoamMessage(RoamMessageInput input, ref List<RoamMessageOutput> output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.RoamMessage, input, ref output,
                ref errorCode, ref errMsg);
        }

        /// <summary>
        /// 获取系统当前时间[GET]
        /// </summary>
        /// <param name="output">时间输出</param>
        /// <param name="errorCode">错误代码（特定情况）</param>
        /// <param name="errMsg">错误提示</param>
        /// <returns>是否成功</returns>
        public bool GetCurrentSysTime(ref QuerySystemDateOuput output, ref int errorCode, ref string errMsg)
        {
            return HttpCommonMethod(SdkService.MdsdkhttpMethod.GetSysTime, (string)null, ref output, ref errorCode, ref errMsg,
                SdkEnumCollection.RequestMethod.GET);
        }
        /// <summary>
        /// 方法说明：文件上传
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="scid">上传信息</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>文件上传结果信息</returns>
        public SdkFileUpLoadOutput FileUpload(SdkSendFileInput scid, ref int errorCode, ref string errorMsg)
        {
            var transId = Guid.NewGuid();
            var url = string.Empty;
            var parm = string.Empty;
            var responseText = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(scid?.file))
                {
                    errorMsg += Resources.SdkHttpUploadFileNoExist;
                    return null;
                }
                if (!System.IO.File.Exists(scid.file))
                {
                    errorMsg += Resources.SdkHttpUploadFileNoExist;
                    return null;
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                url = $"{SdkService.SdkSysParam.FileUpload}{SdkService.MdsdkhttpMethod.FileUploadAdress}";
                parm = $"?&cmpcd={scid.cmpcd}&fileFileName={scid.fileFileName}";
                var client = new WebClient { Encoding = Encoding.UTF8 };
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.Headers.Add(HttpRequestHeader.Authorization, SdkService.SdkSysParam.Token);
                client.Headers.Add("sourceId", $"{(int)SdkEnumCollection.OSType.PC}{SdkService.SdkSysParam.Appkey}");
                client.Headers.Add("transId", $"{transId}");
                client.Headers.Add("sourceInsId", $"{GetComputeInfo.GetMacAddressByNetworkInformation()}");
                string result = new HttpUploadCommon().HttpUploadFile(url + parm, scid.file);
                //var responseResult = client.UploadFile(url + parm, "POST", scid.file);
                //var responseText = System.Text.Encoding.UTF8.GetString(responseResult);
                responseText = result;
                stopWatch.Stop();
                var temperrorCode = string.Empty;
                var errMsg = string.Empty;
                var output = new SdkFileUpLoadOutput();
                if (JsonCoder.GetValueByJsonKey("errorCode", responseText, ref temperrorCode, ref errMsg))
                {
                    if (temperrorCode != "0")
                    {
                        var baseout = new BaseOutput();
                        var temperrMsg = string.Empty;
                        if (!JsonCoder.DeserializeJson(responseText, ref baseout, ref temperrMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaError}{temperrMsg}";
                        }
                        else
                        {
                            if (baseout == null)
                            {
                                return null;
                            }
                            if (string.IsNullOrEmpty(baseout.errorMsg))
                            {
                                baseout.errorMsg = SetResources.Resources.GetString($"E_{temperrorCode}");
                            }
                        }
                        //返回错误
                        return null;
                    }
                    else
                    {
                        var baseout = new BaseOutput();
                        var temperrMsg = string.Empty;
                        if (!JsonCoder.DeserializeJson(responseText, ref baseout, ref temperrMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaError}{temperrMsg}";
                            return null;
                        }
                        else if (baseout.data == null)
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaNull}:errorCode = 0,data = null";
                            return null;
                        }
                        else if (!JsonCoder.DeserializeJson(baseout.data.ToString(), ref output, ref errMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonOutSchemaError}";
                            LogHelper.WriteDebug(
                                $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{url}?{parm}{Environment.NewLine}{responseText} DealWith = {typeof(SdkFileUpLoadOutput).ToString()};Value={baseout.data.ToString()}");
                            return null;
                        }
                        //返回
                        return output;
                    }
                }
                else
                {
                    //如果请求返回的信息没有errorCode则直接反序列为输出实体
                    var getReturnInfo = JsonCoder.DeserializeJson(responseText, ref output, ref errMsg);
                    if (!getReturnInfo)
                    {
                        LogHelper.WriteDebug(
                            $"[HTTPService.SDK_HttpCommonMethod({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{url}?{parm}{Environment.NewLine}{responseText}");
                        return null;
                    }
                }

                //return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(responseText));
                return null;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    errorCode = SdkErrorCodes.Instanece.HTTP_REQUREST_TIMEOUT;
                    errorMsg = Resources.SdkHttpRequestTimeOut;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status.ToString()}),Transid:{transId}]:{url}?{parm}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                else
                {
                    errorMsg = Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status.ToString()}),Transid:{transId}]:{url}?{parm}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                //返回
                return null;
            }
            catch (Exception ex)
            {
                errorMsg += ex.Message;
                LogHelper.WriteError("[HTTPService.FileUpload,Transid:{transid}]:" + SdkService.SdkSysParam.FileUpload + ex.Message + "," +
                                     ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 方法说明：文件上传MD5
        /// 完成时间：2017-06-03
        /// </summary>
        /// <param name="msgMd5">md5信息</param>
        /// <param name="fileName">文件物理路径</param>
        /// <param name="errorCode">错误代码（特殊情况）</param>
        /// <param name="errorMsg">错误提示</param>
        /// <returns>MD5校验信息</returns>
        public SdkFileUpLoadOutput CompareFileMd5(string msgMd5, string fileName, ref int errorCode, ref string errorMsg)
        {
            var transId = Guid.NewGuid();
            var strUrl = string.Empty;
            var parm = string.Empty;
            var responseText = string.Empty;
            StreamReader myreader = null;
            HttpWebResponse response = null;
            Stream streams = null;
            try
            {
                var stopWatch = new Stopwatch();
                parm = msgMd5;
                stopWatch.Start();
                strUrl = $"{SdkService.SdkSysParam.FileUpload}{SdkService.MdsdkhttpMethod.FileMd5Adress}?fileMD5={msgMd5}";
                var request = (HttpWebRequest)WebRequest.Create(strUrl);
                //设置超时时间(先设置为5秒)
                request.Timeout = 5000;
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                request.Headers.Add(HttpRequestHeader.Authorization, SdkService.SdkSysParam.Token);
                request.Headers.Add("sourceId", $"{(int)SdkEnumCollection.OSType.PC}{SdkService.SdkSysParam.Appkey}");
                request.Headers.Add("transId", $"{transId}");
                request.Headers.Add("sourceInsId", $"{GetComputeInfo.GetMacAddressByNetworkInformation()}");
                response = (HttpWebResponse)request.GetResponse();

                streams = response.GetResponseStream();

                if (streams == null)
                {
                    stopWatch.Stop();
                    errorMsg += Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[HTTPService.SDK_CompareFileMd5({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl},Param:{parm}{Environment.NewLine}Response's ResponseStream Is Null");
                    return null;
                }
                myreader = new System.IO.StreamReader(streams, Encoding.UTF8);

                responseText = myreader.ReadToEnd();
                stopWatch.Stop();
                var temperrorCode = string.Empty;
                var errMsg = string.Empty;
                var output = new SdkFileUpLoadOutput();
                if (JsonCoder.GetValueByJsonKey("errorCode", responseText, ref temperrorCode, ref errMsg))
                {
                    if (temperrorCode != "0")
                    {
                        var baseout = new BaseOutput();
                        var temperrMsg = string.Empty;
                        if (!JsonCoder.DeserializeJson(responseText, ref baseout, ref temperrMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaError}{temperrMsg}";
                        }
                        else
                        {
                            if (baseout == null)
                            {
                                return null;
                            }
                            if (string.IsNullOrEmpty(baseout.errorMsg))
                            {
                                baseout.errorMsg = SetResources.Resources.GetString($"E_{temperrorCode}");
                            }
                        }
                        //返回错误
                        return null;
                    }
                    else
                    {
                        var baseout = new BaseOutput();
                        var temperrMsg = string.Empty;
                        if (!JsonCoder.DeserializeJson(responseText, ref baseout, ref temperrMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaError}{temperrMsg}";
                            return null;
                        }
                        else if (baseout.data == null)
                        {
                            errMsg += $"{Resources.SdkHttpCommonBaseSchemaNull}:errorCode = 0,data = null";
                            return null;
                        }
                        else if (!JsonCoder.DeserializeJson(baseout.data.ToString(), ref output, ref errMsg))
                        {
                            errMsg += $"{Resources.SdkHttpCommonOutSchemaError}";
                            LogHelper.WriteDebug(
                                $"[HTTPService.SDK_CompareFileMd5({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl}?{parm}{Environment.NewLine}{responseText} DealWith = {typeof(SdkFileUpLoadOutput).ToString()};Value={baseout.data.ToString()}");
                            return null;
                        }
                        //返回
                        return output;
                    }
                }
                else
                {
                    //如果请求返回的信息没有errorCode则直接反序列为输出实体
                    var getReturnInfo = JsonCoder.DeserializeJson(responseText, ref output, ref errMsg);
                    if (!getReturnInfo)
                    {
                        LogHelper.WriteDebug(
                            $"[HTTPService.SDK_CompareFileMd5({stopWatch.Elapsed.TotalMilliseconds}毫秒,Transid:{transId})]:{strUrl}?{parm}{Environment.NewLine}{responseText}");
                        return null;
                    }
                }

                //return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(responseText));
                return null;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    myreader?.Dispose();
                    response?.Dispose();
                    streams?.Dispose();
                    errorCode = SdkErrorCodes.Instanece.HTTP_REQUREST_TIMEOUT;
                    errorMsg = Resources.SdkHttpRequestTimeOut;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status.ToString()}),Transid:{transId}]:{strUrl}?{parm}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                else
                {
                    errorMsg = Resources.SdkHttpRequestFail;
                    LogHelper.WriteError(
                        $"[SDK_HttpService.HttpCommonMethod({webEx.Status.ToString()}),Transid:{transId}]:{strUrl}?{parm}{Environment.NewLine} responseText:{responseText}{webEx.Message}{webEx.StackTrace}");
                }
                //返回
                return null;
            }
            catch (Exception ex)
            {
                myreader?.Dispose();
                response?.Dispose();
                streams?.Dispose();
                errorMsg += ex.Message;
                LogHelper.WriteError("[HTTPService.SDK_CompareFileMd5,Transid:{transId}]:" + SdkService.SdkSysParam.FileUpload + ex.Message + "," +
                                     ex.StackTrace);
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// SDK_Http方法名
    /// </summary>
    internal class SdkHttpMethod
    {
        /// <summary>
        /// 1、获取token接口:{company}/{appKey}/oauth/token
        /// </summary>
        public string GetToken = $"{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/oauth/token";

        /// <summary>
        /// 2、获取MQTT 连接参数:core/{companyCode}/{appkey}/Config/mqtts
        /// </summary>
        public string GetMqttParam = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/config/mqtts";

        /// <summary>
        /// 3、更新用户信息[POST]:{companyCode}/{appkey}/user/update
        /// </summary>
        public string UpdateUserInfo = $"{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/update";

        /// <summary>
        /// 4、获取当前连接的用户列表[GET]PC客户端不使用:{companyCode}/{appkey}/user/clients
        /// </summary>
        public string GetCurrUserList = $"{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/clients";

        /// <summary>
        /// 5、添加用户黑名单[POST]:core/{companyCode}/{appkey}/user/addBlacklist
        /// </summary>
        public string AddBlacklist = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/addBlacklist";

        /// <summary>
        /// 6、查询黑名单ID列表[GET]:core/{companyCode}/{appkey}/user/findBlacklists
        /// </summary>
        public string FindBlacklists = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/findBlacklists";

        /// <summary>
        /// 7、移除用户黑名单[POST]:core/{companyCode}/{appkey}/user/delBlacklist
        /// </summary>
        public string DelBlacklist = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/delBlacklist";

        /// <summary>
        /// 8、删除聊天室:core/{companyCode}/{appKey}/room/delete
        /// </summary>
        public string DeleteChatRoom = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/delete";

        /// <summary>
        /// 9、创建聊天室:core/{companyCode}/{appKey}/room/add
        /// </summary>
        public string CreateChatRoom = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/add";

        /// <summary>
        /// 10、添加聊天室成员:core/{companyCode}/{appKey}/room/addMembers
        /// </summary>
        public string AddChatRoomMembers = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/addMembers";

        /// <summary>
        /// 11、删除聊天室成员:core/{companyCode}/{appKey}/room/deleteMembers
        /// </summary>
        public string DeleteChatRoomMembers = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/deleteMembers";

        /// <summary>
        /// 12、改变成员属性:core/{companyCode}/{appKey}/room/updateMember
        /// </summary>
        public string UpdateChatRoomMember = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/updateMember";

        /// <summary>
        /// 13、修改聊天室信息:core/{companyCode}/{appKey}/room/update
        /// </summary>
        public string UpdateChatRoom = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/update";

        /// <summary>
        /// 14、成员退出聊天室:core/{companyCode}/{appKey}/room/exitRoom
        /// </summary>
        public string ExitChatRoom = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/exitRoom";

        /// <summary>
        /// 15、查询用户所在聊天室，返回聊天室列表:core/{companyCode}/{appKey}/room/findRooms
        /// </summary>
        public string FindRooms = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/findRooms";

        /// <summary>
        /// 16、查询单个聊天室详细信息:core/{companyCode}/{appKey}/room/get
        /// </summary>
        public string GetChatRoomInfo = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/get";

        /// <summary>
        /// 17、查询聊天室所有成员基本信息:core/{companyCode}/{appKey}/room/findMembers
        /// </summary>
        public string FindRoomMembers = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/findMembers";

        /// <summary>
        /// 18、查询聊天室单个成员详细信息:core/{companyCode}/{appKey}/room/getMember
        /// </summary>
        public string GetRoomMemberInfo = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/getMember";

        /// <summary>
        /// 19、查询app下的所有聊天室:core/{companyCode}/{appKey}/room/findAllRooms
        /// </summary>
        public string FindAllRooms = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/findAllRooms";

        /// <summary>
        /// 20、邀请加入聊天室:core/{companyCode}/{appKey}/room/invite
        /// </summary>
        public string InviteJoinRoom = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/invite";

        /// <summary>
        /// 21、处理邀请:core/{companyCode}/{appKey}/room/inviteHandle
        /// </summary>
        public string HandleInvite = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/inviteHandle";

        /// <summary>
        /// 22、查询用户所在聊天室，返回聊天室列表以及用户个性化设置[GET]:core/{companyCode}/{appKey}/room/findIndividRooms
        /// </summary>
        public string FindIndividRooms = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/findIndividRooms";

        /// <summary>
        /// 23、设置聊天室的接收消息类型[POST]:core/{companyCode}/{appKey}/room/setReceiveType
        /// </summary>
        public string SetRoomReceiveType = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/room/setReceiveType";

        /// <summary>
        /// 24、设置用户的接收消息类型[POST]:core/{companyCode}/{appKey}/user/setReceiveType
        /// </summary>
        public string SetUserReceiveType = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/user/setReceiveType";

        /// <summary>
        /// 25、查询离线消息: core/{companyCode}/{appKey}/queryOfflineMsg
        /// </summary>
        public string QueryOfflineMsg = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/queryOfflineMsg";

        /// <summary>
        /// 26、手机端切换运行状态:core/{companyCode}/{appKey}/changeAppRunStatus
        /// </summary>
        public string ChangeAppRunStatus = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/changeAppRunStatus";

        //#下面为新加接口[按照文档顺序：http://192.168.10.227/api/cj_http.html]
        /// <summary>
        /// [手机端使用]更新小米、华为、魅族、信鸽的信息: core/{companyCode}/{appKey}/updatePushDeviceToken
        /// </summary>
        public string UpdatePushDeviceToken = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/updatePushDeviceToken";

        //[放到客户端]获取联系人信息，返回数组格式: core/{companyCode}/{appKey}/{version}/{userId}/listContacts
        //public  string GetContactorInfos = $"core/{SysParam.AntPartner}/{SysParam.AntAppkey}/{version}/{userId}/listContacts";
        //[放到客户端]获取联系人信息—-增量信息(返回值区分组织架构变化和不变化两种情况):core/{companyCode}/{appKey}/{version}/{userId}/incrementContacts?dataVersion={dataVersion}

        /// <summary>
        /// 27、获取用户的讨论组列表[GET]: core/{companyCode}/{appKey}/{version}/{userId}/groupList
        /// </summary>
        public string GetGroupList = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupList";

        /// <summary>
        /// 28、获取讨论组成员信息[GET]:core/{companyCode}/{appKey}/{version}/{userId}/{groupId}/groupMembers
        /// </summary>
        public string GetGroupMembers = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupMembers";

        /// <summary>
        /// 29、退出讨论组[POST]:core/{companyCode}/{appKey}/groupExitor
        /// </summary>
        public string GroupExitor = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupExit";

        /// <summary>
        /// 30、群主转让:core/{companyCode}/{appKey}/groupOwner
        /// </summary>
        public string GroupOwnerChange = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupOwner";

        /// <summary>
        /// 31、创建讨论组[POST]: core/{companyCode}/{appKey}/group
        /// </summary>
        public string CreateGroup = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/group";

        /// <summary>
        /// 32、更新讨论组信息[PUT]:core/{companyCode}/{appKey}/group
        /// </summary>
        public string UpdateGroup = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/group";

        /// <summary>
        /// 33、更新用户在讨论组的设置[PUT]: core/{companyCode}/{appKey}/groupConfig
        /// </summary>
        public string UpdateGroupConfig = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupConfig";

        /// <summary>
        /// 34、解散讨论组[DELETE]: core/{companyCode}/{appKey}/group/{groupId}/user/{userId}
        /// </summary>
        public string DissolveGroup = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/group";

        /// <summary>
        /// 35、获取群的所有公告[GET]：core/{companyCode}/{appKey}/notifications?userId={userId} & targetId={targetId}
        /// </summary>
        public string GetGroupNotifications = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/notifications";

        /// <summary>
        /// 36、通过ID获取群公告[GET]：core/{companyCode}/{appKey}/notifications?notificationId={notificationId}
        /// </summary>
        public string GetNotificationsById = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/notifications";

        /// <summary>
        /// 37、添加群公告（群主才有权限）[POST]：core/{companyCode}/{appKey}/notifications
        /// </summary>
        public string AddNotifications = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/notifications";

        /// <summary>
        /// 38、用户修改公告状态为已读[PUT]：core/{companyCode}/{appKey}/notifications
        /// </summary>
        public string UpdateNotificationsState = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/notifications";

        /// <summary>
        /// 39、删除群公告(群主才有权限)[DELETE]：core/{companyCode}/{appKey}/notifications/{notificationId}
        /// </summary>
        public string DeleteNotificationsById = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/notifications";

        /// <summary>
        /// 40、消息漫游:core/{companyCode}/{appKey}/roamMessage
        /// </summary>
        public string RoamMessage = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/roamMessage";

        /// <summary>
        /// 41、文件上传地址
        /// </summary>
        public string FileUploadAdress = $"v1/file/upload";

        /// <summary>
        /// 42、文件上传MD5验证
        /// </summary>
        public string FileMd5Adress = $"v1/file/file";

        /// <summary>
        /// 43、获取系统当前时间 GET
        /// </summary>
        public string GetSysTime =
            $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/systemCurrentTime";
        /// <summary>
        /// 44、设置群组管理员
        /// </summary>
        public string GroupManagerSet = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/groupManager";

        /// <summary>
        /// 45、创建群投票[POST]:v1/groups/{id}/votings
        /// </summary>
        public string CreateGroupVotings = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/groups/{0}/votings";

        /// <summary>
        /// 46、查看投票详情[GET]:v1/votings/{0}
        /// </summary>
        public string GetVoteInfo = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/votings/{0}/?userId={1}";

        /// <summary>
        /// 47、获取所有弃权票信息[GET]:v1/votings/{id}/abstention
        /// </summary>
        public string GetGroupAbstentionVote = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/votings/{0}/votes/abstentions";

        /// <summary>
        /// 48、获取群投票[GET]:v1/groups/{id}/votings
        /// </summary>
        public string GetGroupVotes = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/groups/{0}/votings";

        /// <summary>
        /// 49、删除某个投票活动[GET]
        /// </summary>
        public string DeleteGroupVote = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/votings/{0}";

        /// <summary>
        ///50、提交群投票选项[POST]
        /// </summary>
        public string SubmitGroupVoteOptions = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}" + "/v1/votings/{0}/votes";

        /// <summary>
        ///51、发布活动[POST]
        /// </summary>
        public string ReleaseGroupActivity = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}"+"/activity/create";

        /// <summary>
        ///52、获取群活动列表[GET]
        /// </summary>
        public string GetGroupActivitys = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/pagelist";

        /// <summary>
        /// 53、获取群活动详情[GET]
        /// </summary>
        public string GetGroupActivityDetails = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/detail";

        /// <summary>
        /// 54、删除活动[DELETE]
        /// </summary>
        public string DeleteGroupActivity = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/delete";

        /// <summary>
        /// 55、活动参与者列表[GET]
        /// </summary>
        public string GetGroupActivityParticipators = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/voter/pagelist";

        /// <summary>
        /// 56、参与活动[POST]
        /// </summary>
        public string ParticipateActivities = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/voter/add";

        /// <summary>
        /// 57、活动主题图片[GET]
        /// </summary>
        public string GetActivityImages = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/activity/images";

        /// <summary>
        /// 确认打卡[POST]
        /// </summary>
        public string ConfirmVerify = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/attend/confirm";

        /// <summary>
        /// 获取打卡记录列表[GET]
        /// </summary>
        public string GetPunchClocks = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/attends";

        /// <summary>
        /// 58、历史消息同步[POST]:core/{companyCode}/{appKey}/synchronusMsg
        /// </summary>
        public string SynchronusMsgs = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/synchronusMsg";

        /// <summary>
        /// 59、群组无痕模式切换[POST]:core/{companyCode}/{appKey}/burnAfterRead
        /// </summary>
        public string SwitchBurnAfterRead = $"core/{SdkService.SdkSysParam.Companycode}/{SdkService.SdkSysParam.Appkey}/burnAfterRead";


    }
}
