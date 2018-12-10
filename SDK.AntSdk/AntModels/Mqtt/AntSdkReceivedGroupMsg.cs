using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.AntSdk.Properties;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkReceivedGroupMsg
    {
        /// <summary>
        /// SDK消息基类Model
        /// </summary>
        public class GroupBase : AntSdkMsBase
        {
            /// <summary>
            /// 时间戳
            /// </summary>
            public string sendTime { get; set; } = string.Empty;
        }

        /// <summary>
        /// 创建讨论组
        /// </summary>
        public class Create : GroupBase
        {
            /// <summary>
            /// 创建讨论组信息
            /// </summary>
            public Create_content content { get; set; }
        }

        /// <summary>
        /// 创建讨论组内容
        /// </summary>
        public class Create_content
        {
            /// <summary>
            /// 讨论组ID
            /// </summary>
            public string groupId { get; set; } = string.Empty;

            /// <summary>
            /// 讨论组名称
            /// </summary>
            public string groupName { get; set; } = string.Empty;

            /// <summary>
            /// 讨论组头像，初次创建时，可以不传头像
            /// </summary>
            public string groupPicture { get; set; } = string.Empty;

            /// <summary>
            /// 群成员数量
            /// </summary>
            public int memberCount { get; set; }

            /// <summary>
            /// 群主ID
            /// </summary>
            public string groupOwnerId { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到讨论组信息变更
        /// </summary>
        public class Modify : GroupBase
        {
            /// <summary>
            /// 讨论组信息变更
            /// </summary>
            public Modify_content content { get; set; }
        }

        /// <summary>
        /// 讨论组信息变更
        /// </summary>
        public class Modify_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 群ID
            /// </summary>
            public string groupId { get; set; } = string.Empty;

            /// <summary>
            /// 群名称
            /// </summary>
            public string groupName { get; set; } = string.Empty;

            /// <summary>
            /// 群头像地址
            /// </summary>
            public string groupPicture { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到解散聊天室消息
        /// </summary>
        public class Delete : GroupBase
        {
            /// <summary>
            /// 解散聊天室信息
            /// </summary>
            public Delete_content content { get; set; }
        }

        /// <summary>
        /// 解散聊天室信息
        /// </summary>
        public class Delete_content
        {
            /// <summary>
            /// 群ID
            /// </summary>
            public string groupId { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到讨论组添加成员信息
        /// </summary>
        public class AddMembers : GroupBase
        {
            /// <summary>
            /// 聊天室添加成员
            /// </summary>
            public AddMembers_content content { get; set; }
        }

        /// <summary>
        /// 讨论组添加成员
        /// </summary>
        public class AddMembers_content
        {
            /// <summary>
            /// 添加成员信息
            /// </summary>
            public List<AntSdkMember> members { get; set; }

            /// <summary>
            /// 操作人ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到群组删除成员信息
        /// </summary>
        public class DeleteMembers : GroupBase
        {
            /// <summary>
            /// 删除群组成员消息
            /// </summary>
            public DeleteMembers_content content { get; set; }
        }

        /// <summary>
        /// 删除群组成员信息
        /// </summary>
        public class DeleteMembers_content
        {
            /// <summary>
            /// 删除成员信息
            /// </summary>
            public List<AntSdkMember> members { get; set; }

            /// <summary>
            /// 操作人ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到成员退出讨论组
        /// </summary>
        public class QuitMember : GroupBase
        {
            /// <summary>
            /// 成员退出讨论组
            /// </summary>
            public QuitMember_content content { get; set; }
        }

        /// <summary>
        /// 成员退出讨论组
        /// </summary>
        public class QuitMember_content
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            public string userId { get; set; } = string.Empty;

            /// <summary>
            /// 用户名称
            /// </summary>
            public string userName { get; set; } = string.Empty;

            /// <summary>
            /// 群主ID，如果群主退出有值
            /// </summary>
            public string groupOwnerId { get; set; } = string.Empty;

            /// <summary>
            /// 群主名称，如果群主退出有值
            /// </summary>
            public string groupOwnerName { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到讨论组成员信息变更
        /// </summary>
        public class ModifyMember : GroupBase
        {
            /// <summary>
            /// 讨论组成员信息变更
            /// </summary>
            public ModifyMember_content content { get; set; }
        }

        /// <summary>
        /// 讨论组成员信息变更
        /// </summary>
        public class ModifyMember_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 被改变的用户ID
            /// </summary>
            public string userId { get; set; } = string.Empty;

            /// <summary>
            /// 被改变的用户名称
            /// </summary>
            public string userName { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到群主切换为阅后即焚模式
        /// </summary>
        public class OwnerBurnMode : GroupBase
        {
            public OwnerBurnMode_content content { get; set; }
        }

        /// <summary>
        /// 群主切换为阅后即焚模式
        /// </summary>
        public class OwnerBurnMode_content
        {
            /// <summary>
            /// 删除chatIndex小于maxIndex的所有阅后即焚消息
            /// </summary>
            public int maxIndex { get; set; }
        }

        /// <summary>
        /// MQTT收到群主在阅后即焚模式下删除消息
        /// </summary>
        public class OwnerBurnDelete : GroupBase
        {
            public OwnerBurnDelete_content content { get; set; }
        }

        /// <summary>
        /// 群主在阅后即焚模式下删除消息
        /// </summary>
        public class OwnerBurnDelete_content
        {
            /// <summary>
            /// 删除chatIndex小于maxIndex的所有阅后即焚消息
            /// </summary>
            public int maxIndex { get; set; }
        }

        /// <summary>
        /// MQTT接收到群主切换为普通消息模式
        /// </summary>
        public class OwnerNormal : GroupBase
        {
            /// <summary>
            /// 群主切换为普通消息模式
            /// </summary>
            public string content { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT 收到群组群主变更通知
        /// </summary>
        public class OwnerChanged : GroupBase
        {
            /// <summary>
            /// 群主变更通知内容
            /// </summary>
            public OwnerChanged_content content { get; set; }
        }

        /// <summary>
        /// 群主变更通知内容
        /// </summary>
        public class OwnerChanged_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string oldOwnerId { get; set; } = string.Empty;

            /// <summary>
            /// 群ID
            /// </summary>
            public string newOwnerId { get; set; } = string.Empty;

        }

        /// <summary>
        /// 管理员授权
        /// </summary>
        public class AdminSet : GroupBase
        {
            /// <summary>
            /// 管理员授权内容
            /// </summary>
            public AdminSet_content content { get; set; }
        }

        /// <summary>
        /// 管理员授权内容
        /// </summary>
        public class AdminSet_content
        {
            /// <summary>
            /// 管理员ID
            /// </summary>
            public string manageId { get; set; } = string.Empty;

            /// <summary>
            /// 0表示普通成员，2表示管理员
            /// </summary>
            public int roleLevel { get; set; }
        }

        /// <summary>
        /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static T GetReceiveAntSdkGroupBaseInfo<T>(MsSdkMessageGroupBase entity)
            where T : GroupBase, new()
        {
            var sdkreceivemsgtypeValue = (long)entity.MsgType;
            var antsdkreceivemsgType = (AntSdkMsgType)sdkreceivemsgtypeValue;
            var result = new T
            {
                MsgType = antsdkreceivemsgType,
                sessionId = entity.sessionId,
                chatIndex = entity.chatIndex,
                sendTime = entity.sendTime
            };
            return result;
        }

        /// <summary>
        /// 方法说明：获取接收到的平台SDK聊天消息，转化为触角SDK聊天消息
        /// </summary>
        /// <param name="entity">SDK聊天信息</param>
        /// <returns>触角SDK聊天信息</returns>
        internal static GroupBase GetReceiveAntSdkGroupInfo(MsSdkMessageGroupBase entity)
        {
            try
            {
                var sdkcreateObj = entity as CreateGroup;
                if (sdkcreateObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<Create>(sdkcreateObj);
                    antsdkchatgroupMsg.content = new Create_content
                    {
                        groupId = sdkcreateObj.content?.groupId,
                        groupName = sdkcreateObj.content?.groupName,
                        groupPicture = sdkcreateObj.content?.groupPicture,
                        memberCount = sdkcreateObj.content?.memberCount ?? 0,
                        groupOwnerId = sdkcreateObj.content?.groupOwnerId
                    };
                    //收到创建讨论组通知，必须订阅讨论组主题收消息
                    if (!string.IsNullOrEmpty(sdkcreateObj.content?.groupId))
                    {
                        //连接成功需要订阅的默认主题
                        var topics = new List<string> {sdkcreateObj.content.groupId};
                        //订阅默认主题
                        var temperrorMsg = string.Empty;
                        if (!SdkService.Subscribe(topics.ToArray(), ref temperrorMsg))
                        {
                            //记录收到创建讨论组通知后订阅讨论组主题失败日志
                            LogHelper.WriteError($"Received Create Group Message Subscribe Group Topic,{Resources.AntSdkSubscribeGroupTopicsError}：{temperrorMsg}");
                        }
                    }
                    return antsdkchatgroupMsg;
                }
                var sdkmodifyObj = entity as MsModifyGroup;
                if (sdkmodifyObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<Modify>(sdkmodifyObj);
                    antsdkchatgroupMsg.content = new Modify_content
                    {
                        operateId = sdkmodifyObj.content?.operateId,
                        groupId = sdkmodifyObj.content?.groupId,
                        groupName = sdkmodifyObj.content?.groupName,
                        groupPicture = sdkmodifyObj.content?.groupPicture
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkdeleteObj = entity as MsDeleteGroup;
                if (sdkdeleteObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<Delete>(sdkdeleteObj);
                    antsdkchatgroupMsg.content = new Delete_content
                    {
                        groupId = sdkdeleteObj.content?.groupId
                    };
                    //收到删除讨论组通知，必须取消订阅讨论组主题收消息
                    if (!string.IsNullOrEmpty(sdkdeleteObj.content?.groupId))
                    {
                        //连接成功需要订阅的默认主题
                        var topics = new List<string> {sdkdeleteObj.content?.groupId};
                        //取消订阅主题
                        var temperrorMsg = string.Empty;
                        if (!SdkService.UnSubscribe(topics.ToArray(), ref temperrorMsg))
                        {
                            //记录收到删除讨论组通知后取消订阅讨论组主题失败日志
                            LogHelper.WriteError($"Received Delete Group Message UnSubscribe Group Topic,{Resources.AntSdkSubscribeDissolveGroupTopicsError}：{temperrorMsg}");
                        }
                    }
                    return antsdkchatgroupMsg;
                }
                var sdkaddmemberObj = entity as MsAddGroupMembers;
                if (sdkaddmemberObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<AddMembers>(sdkaddmemberObj);
                    var addmemberList = new List<AntSdkMember>();
                    if (sdkaddmemberObj.content?.members?.Count > 0)
                    {
                        addmemberList.AddRange(sdkaddmemberObj.content.members.Select(c => new AntSdkMember
                        {
                            userId = c.userId,
                            userName = c.userName
                        }));
                        //判断被删除的人中存在当前用户，则当前用户的该组主题需要取消订阅
                        var delcurrentUser =
                            addmemberList.FirstOrDefault(u => u.userId == AntSdkService.AntSdkLoginOutput.userId);
                        if (delcurrentUser != null && !string.IsNullOrEmpty(antsdkchatgroupMsg.sessionId))
                        {
                            //当前客户被群主删除，则订阅此群主题
                            var topics = new List<string> { antsdkchatgroupMsg.sessionId };
                            //订阅主题
                            var temperrorMsg = string.Empty;
                            if (!SdkService.Subscribe(topics.ToArray(), ref temperrorMsg))
                            {
                                //收到群主删除组员包含当前用户则取消订阅讨论组主题失败日志
                                LogHelper.WriteError($"Received Delete Group Member Message Contains Self UnSubscribe Group Topic,{Resources.AntSdkSubscribeDeleteGroupMemberTopicsError}：{temperrorMsg}");
                            }
                        }
                    }
                    antsdkchatgroupMsg.content = new AddMembers_content
                    {
                        operateId = sdkaddmemberObj.content?.operateId,
                        members = addmemberList
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkdeletememberObj = entity as MsDeleteGroupMembers;
                if (sdkdeletememberObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<DeleteMembers>(sdkdeletememberObj);
                    var addmemberList = new List<AntSdkMember>();
                    if (sdkdeletememberObj.content?.members?.Count > 0)
                    {
                        addmemberList.AddRange(sdkdeletememberObj.content.members.Select(c => new AntSdkMember
                        {
                            userId = c.userId,
                            userName = c.userName
                        }));
                        //判断被删除的人中存在当前用户，则当前用户的该组主题需要取消订阅
                        var delcurrentUser =
                            addmemberList.FirstOrDefault(u => u.userId == AntSdkService.AntSdkLoginOutput.userId);
                        if (delcurrentUser != null && !string.IsNullOrEmpty(antsdkchatgroupMsg.sessionId))
                        {
                            //当前客户被群主删除，则取消订阅此群主题
                            var topics = new List<string> { antsdkchatgroupMsg.sessionId };
                            //取消订阅主题
                            var temperrorMsg = string.Empty;
                            if (!SdkService.UnSubscribe(topics.ToArray(), ref temperrorMsg))
                            {
                                //收到群主删除组员包含当前用户则取消订阅讨论组主题失败日志
                                LogHelper.WriteError($"Received Delete Group Member Message Contains Self UnSubscribe Group Topic,{Resources.AntSdkSubscribeDeleteGroupMemberTopicsError}：{temperrorMsg}");
                            }
                        }
                    }
                    antsdkchatgroupMsg.content = new DeleteMembers_content
                    {
                        operateId = sdkdeletememberObj.content?.operateId,
                        members = addmemberList
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkquitmembersObj = entity as MsQuitGroupMember;
                if (sdkquitmembersObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<QuitMember>(sdkquitmembersObj);
                    antsdkchatgroupMsg.content = new QuitMember_content
                    {
                        userId = sdkquitmembersObj.content?.userId,
                        userName = sdkquitmembersObj.content?.userName,
                        groupOwnerId = sdkquitmembersObj.content?.groupOwnerId,
                        groupOwnerName = sdkquitmembersObj.content?.groupOwnerName
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkmodifymembersObj = entity as MsModifyGroupMember;
                if (sdkmodifymembersObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<ModifyMember>(sdkmodifymembersObj);
                    antsdkchatgroupMsg.content = new ModifyMember_content
                    {
                        operateId = sdkmodifymembersObj.content?.operateId,
                        userId = sdkmodifymembersObj.content?.userId,
                        userName = sdkmodifymembersObj.content?.userName,

                    };
                    return antsdkchatgroupMsg;
                }
                var sdkownerburnmodeObj = entity as MsGroupOwnerBurnMode;
                if (sdkownerburnmodeObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<OwnerBurnMode>(sdkownerburnmodeObj);
                    antsdkchatgroupMsg.content = new OwnerBurnMode_content
                    {
                        maxIndex = sdkownerburnmodeObj.content?.maxIndex ?? 0
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkownerburndeleteObj = entity as MsGroupOwnerBurnDelete;
                if (sdkownerburndeleteObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<OwnerBurnDelete>(sdkownerburndeleteObj);
                    antsdkchatgroupMsg.content = new OwnerBurnDelete_content
                    {
                        maxIndex = sdkownerburndeleteObj.content?.maxIndex ?? 0
                    };
                    return antsdkchatgroupMsg;
                }
                var sdkownerburnnomalObj = entity as MsGroupOwnerNormal;
                if (sdkownerburnnomalObj != null)
                {
                    var antsdkchatgroupMsg = GetReceiveAntSdkGroupBaseInfo<OwnerNormal>(sdkownerburnnomalObj);
                    antsdkchatgroupMsg.content = sdkownerburnnomalObj.content;
                    return antsdkchatgroupMsg;
                }
                var sdkgroupownerchangObj = entity as MsGroupOwnerChanged;
                if (sdkgroupownerchangObj != null)
                {
                    var antsdkownerchangedMsg = GetReceiveAntSdkGroupBaseInfo<OwnerChanged>(sdkgroupownerchangObj);
                    antsdkownerchangedMsg.content = new OwnerChanged_content
                    {
                        newOwnerId = sdkgroupownerchangObj.content?.newOwnerId,
                        oldOwnerId = sdkgroupownerchangObj.content?.oldOwnerId
                    };
                    return antsdkownerchangedMsg;
                }
                var sdkgroupadminsetObj = entity as MsGroupAdminSet;
                if (sdkgroupadminsetObj != null)
                {
                    var antsdkowneradminsetMsg = GetReceiveAntSdkGroupBaseInfo<AdminSet>(sdkgroupadminsetObj);
                    antsdkowneradminsetMsg.content = new AdminSet_content
                    {
                        manageId = sdkgroupadminsetObj.content?.manageId,
                        roleLevel = sdkgroupadminsetObj.content?.roleLevel ?? 0
                    };
                    return antsdkowneradminsetMsg;
                }
                //返回空
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkChatRoomMsg.GetReceiveAntSdkChatRoomInfo]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }
    }
}
