/*
 * 触角SDK聊天室消息及转换（平台SDK聊天室消息->触角SDK聊天室消息）
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkReceivedRoomMsg
    {
        public class RoomBase : AntSdkMsBase
        {
            /// <summary>
            /// 时间戳
            /// </summary>
            public string sendTime { get; set; } = string.Empty;
        }

        /// <summary>
        /// 聊天室成员信息
        /// </summary>
        public class Member
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            public string userId { get; set; } = string.Empty;

            /// <summary>
            /// 用户名称
            /// </summary>
            public string userName { get; set; } = string.Empty;
        }

        /// <summary>
        /// 创建聊天室通知
        /// </summary>
        public class Create : RoomBase
        {
            /// <summary>
            /// 通知内容
            /// </summary>
            public Create_content content { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Create_content
        {
            /// <summary>
            /// 创建者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室ID 
            /// </summary>
            public string roomId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室名称
            /// </summary>
            public string roomName { get; set; } = string.Empty;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc { get; set; } = string.Empty;

            /// <summary>
            /// 备注
            /// </summary>
            public string remark { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr1 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr2 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr3 { get; set; } = string.Empty;

            /// <summary>
            /// 是否开启机器人
            /// </summary>
            public string robotFlag { get; set; } = string.Empty;

            /// <summary>
            /// 机器人类型
            /// </summary>
            public string robotType { get; set; } = string.Empty;
        }

        /// <summary>
        /// 聊天室信息变更
        /// </summary>
        public class Modify : RoomBase
        {
            /// <summary>
            /// 聊天室信息变更
            /// </summary>
            public Modify_content content { get; set; }
        }

        public class Modify_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室ID
            /// </summary>
            public string roomId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室名称
            /// </summary>
            public string roomName { get; set; } = string.Empty;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc { get; set; } = string.Empty;

            /// <summary>
            /// 备注
            /// </summary>
            public string remark { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr1 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr2 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr3 { get; set; } = string.Empty;

            /// <summary>
            /// 是否开启机器人
            /// </summary>
            public string robotFlag { get; set; } = string.Empty;

            /// <summary>
            /// 机器人类型
            /// </summary>
            public string robotType { get; set; } = string.Empty;
        }

        /// <summary>
        /// 解散聊天室（MQTT接收）
        /// </summary>
        public class Delete : RoomBase
        {
            /// <summary>
            /// 通知内容
            /// </summary>
            public Delete_content content { get; set; }
        }

        /// <summary>
        /// 删除聊天室
        /// </summary>
        public class Delete_content
        {
            /// <summary>
            /// 创建者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室ID 
            /// </summary>
            public string roomId { get; set; } = string.Empty;

            /// <summary>
            /// 聊天室名称
            /// </summary>
            public string roomName { get; set; } = string.Empty;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc { get; set; } = string.Empty;

            /// <summary>
            /// 备注
            /// </summary>
            public string remark { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr1 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr2 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr3 { get; set; } = string.Empty;

            /// <summary>
            /// 是否开启机器人
            /// </summary>
            public string robotFlag { get; set; } = string.Empty;

            /// <summary>
            /// 机器人类型
            /// </summary>
            public string robotType { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到添加聊天室成员消息
        /// </summary>
        public class AddMembers : RoomBase
        {
            public AddMembers_content content { get; set; }
        }

        /// <summary>
        /// MQTT收到添加聊天室成员结构
        /// </summary>
        public class AddMembers_content
        {
            /// <summary>
            /// 一次最多拉200个成员
            /// </summary>
            public List<Member> members { get; set; }

            /// <summary>
            /// 操作者账号，必须是创建者才可以操作
            /// </summary>
            public string operateId { get; set; } = string.Empty;
        }

        /// <summary>
        /// 删除聊天室成员(MQTT接收到的消息) 
        /// </summary>
        public class DeleteMembers : RoomBase
        {
            /// <summary>
            /// 
            /// </summary>
            public DeleteMembers_content content { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DeleteMembers_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 被删除成员
            /// </summary>
            public List<Member> members { get; set; }
        }

        /// <summary>
        /// 聊天室成员退出
        /// </summary>
        public class QuitMember : RoomBase
        {
            public Member content { get; set; }
        }

        /// <summary>
        /// 聊天室成员信息变更
        /// </summary>
        public class ModifyMember : RoomBase
        {
            public ModifMember_content content { get; set; }
        }

        /// <summary>
        /// 聊天室成员信息变更
        /// </summary>
        public class ModifMember_content
        {
            /// <summary>
            /// 操作者ID
            /// </summary>
            public string operateId { get; set; } = string.Empty;

            /// <summary>
            /// 成员ID
            /// </summary>
            public string userId { get; set; } = string.Empty;

            /// <summary>
            /// 成员名称
            /// </summary>
            public string userName { get; set; } = string.Empty;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc { get; set; } = string.Empty;

            /// <summary>
            /// 备注
            /// </summary>
            public string remark { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr1 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr2 { get; set; } = string.Empty;

            /// <summary>
            /// 扩展信息
            /// </summary>
            public string attr3 { get; set; } = string.Empty;
        }

        /// <summary>
        /// 方法说明：触角SDK聊天消息和SDK聊天消息转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static T GetReceiveAntSdkChatBaseInfo<T>(MsSdkMessageRoomBase entity)
            where T : RoomBase, new()
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
        internal static RoomBase GetReceiveAntSdkRoomInfo(MsSdkMessageRoomBase entity)
        {
            try
            {
                var sdkcreateObj = entity as MsCreateChatRoom;
                if (sdkcreateObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<Create>(sdkcreateObj);
                    antsdkchatroomMsg.content = new Create_content
                    {
                        operateId = sdkcreateObj.content?.operateId,
                        roomId = sdkcreateObj.content?.roomId,
                        roomName = sdkcreateObj.content?.roomName,
                        desc = sdkcreateObj.content?.desc,
                        remark = sdkcreateObj.content?.remark,
                        attr1 = sdkcreateObj.content?.attr1,
                        attr2 = sdkcreateObj.content?.attr2,
                        attr3 = sdkcreateObj.content?.attr3,
                        robotFlag = sdkcreateObj.content?.robotFlag,
                        robotType = sdkcreateObj.content?.robotType
                    };
                    return antsdkchatroomMsg;
                }
                var sdkmodifyObj = entity as MsModifyChatRoom;
                if (sdkmodifyObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<Modify>(sdkmodifyObj);
                    antsdkchatroomMsg.content = new Modify_content
                    {
                        operateId = sdkmodifyObj.content?.operateId,
                        roomId = sdkmodifyObj.content?.roomId,
                        roomName = sdkmodifyObj.content?.roomName,
                        desc = sdkmodifyObj.content?.desc,
                        remark = sdkmodifyObj.content?.remark,
                        attr1 = sdkmodifyObj.content?.attr1,
                        attr2 = sdkmodifyObj.content?.attr2,
                        attr3 = sdkmodifyObj.content?.attr3,
                        robotFlag = sdkmodifyObj.content?.robotFlag,
                        robotType = sdkmodifyObj.content?.robotType
                    };
                    return antsdkchatroomMsg;
                }
                var sdkdeleteObj = entity as MsDeleteChatRoom;
                if (sdkdeleteObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<Delete>(sdkdeleteObj);
                    antsdkchatroomMsg.content = new Delete_content
                    {
                        operateId = sdkdeleteObj.content?.operateId,
                        roomId = sdkdeleteObj.content?.roomId,
                        roomName = sdkdeleteObj.content?.roomName,
                        desc = sdkdeleteObj.content?.desc,
                        remark = sdkdeleteObj.content?.remark,
                        attr1 = sdkdeleteObj.content?.attr1,
                        attr2 = sdkdeleteObj.content?.attr2,
                        attr3 = sdkdeleteObj.content?.attr3,
                        robotFlag = sdkdeleteObj.content?.robotFlag,
                        robotType = sdkdeleteObj.content?.robotType
                    };
                    return antsdkchatroomMsg;
                }
                var sdkaddmembersObj = entity as MsAddChatRoomMembers;
                if (sdkaddmembersObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<AddMembers>(sdkaddmembersObj);
                    var addmemberList = new List<Member>();
                    if (sdkaddmembersObj.content?.members?.Count > 0)
                    {
                        addmemberList.AddRange(sdkaddmembersObj.content.members.Select(c => new Member
                        {
                            userId = c.userId,
                            userName = c.userName
                        }));
                    }
                    antsdkchatroomMsg.content = new AddMembers_content
                    {
                        operateId = sdkaddmembersObj.content?.operateId,
                        members = addmemberList
                    };
                    return antsdkchatroomMsg;
                }
                var sdkdeletemembersObj = entity as MsDeleteChatRoomMembers;
                if (sdkdeletemembersObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<DeleteMembers>(sdkdeletemembersObj);
                    var addmemberList = new List<Member>();
                    if (sdkdeletemembersObj.content?.members?.Count > 0)
                    {
                        addmemberList.AddRange(sdkdeletemembersObj.content.members.Select(c => new Member
                        {
                            userId = c.userId,
                            userName = c.userName
                        }));
                    }
                    antsdkchatroomMsg.content = new DeleteMembers_content
                    {
                        operateId = sdkdeletemembersObj.content?.operateId,
                        members = addmemberList
                    };
                    return antsdkchatroomMsg;
                }
                var sdkquitmembersObj = entity as MsQuitChatRoomMember;
                if (sdkquitmembersObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<QuitMember>(sdkquitmembersObj);
                    antsdkchatroomMsg.content = new Member
                    {
                        userId = sdkquitmembersObj.content?.userId,
                        userName = sdkquitmembersObj.content?.userName
                    };
                    return antsdkchatroomMsg;
                }
                var sdkmodifymembersObj = entity as MsModifyChatRoomMember;
                if (sdkmodifymembersObj != null)
                {
                    var antsdkchatroomMsg = GetReceiveAntSdkChatBaseInfo<ModifyMember>(sdkmodifymembersObj);
                    antsdkchatroomMsg.content = new ModifMember_content
                    {
                        operateId = sdkmodifymembersObj.content?.operateId,
                        userId = sdkmodifymembersObj.content?.userId,
                        userName = sdkmodifymembersObj.content?.userName,
                        desc = sdkmodifymembersObj.content?.desc,
                        remark = sdkmodifymembersObj.content?.remark,
                        attr1 = sdkmodifymembersObj.content?.attr1,
                        attr2 = sdkmodifymembersObj.content?.attr2,
                        attr3 = sdkmodifymembersObj.content?.attr3
                    };
                    return antsdkchatroomMsg;
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
