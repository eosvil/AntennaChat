using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    /// <summary>
    /// 触角SDK：更新用户信息输入参数
    /// </summary>
    public class AntSdkUpdateUserInfoInput
    {
        /// <summary>
        /// 当前用户在app下的唯一ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 当前用户名称
        /// </summary>
        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 用户描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 用户备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal UpdateUserInfoInput GetSdk()
        {
            var sdk = new UpdateUserInfoInput
            {
                userId = userId,
                userName = userName,
                desc = desc,
                remark = remark,
                attr1 = attr1,
                attr2 = attr2,
                attr3 = attr3
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：创建聊天室输入信息
    /// </summary>
    public class AntSdkCreateChatRoomInput
    {
        /// <summary>
        /// 创建者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

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
        /// 是否开启机器人“0”未开启，”1”开启机器人
        /// </summary>
        public int robotFlag { get; set; }

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员 
        /// </summary>
        public List<AntSdkMember> members { get; set; }

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal CreateChatRoomInput GetSdk()
        {
            var sdkmember = new List<SdkMember>();
            if (members?.Count > 0)
            {
                sdkmember.AddRange(members.Select(c => new SdkMember
                {
                    userId = c?.userId,
                    userName = c?.userName
                }));
            }
            var sdk = new CreateChatRoomInput
            {
                operateId = operateId,
                roomName = roomName,
                desc = desc,
                remark = remark,
                attr1 = attr1,
                attr2 = attr2,
                attr3 = attr3,
                robotFlag = robotFlag,
                robotType = robotType,
                members = sdkmember
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：添加聊天室成员输入信息
    /// </summary>
    public class AntSdkAddChatRoomMembersInput
    {
        /// <summary>
        /// 聊天室ID 
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;

        /// <summary>
        /// 操作者账号，必须是创建者才可以操作
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<SdkMember> members { get; set; }

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal AddChatRoomMembersInput GetSdk()
        {
            var sdkmember = new List<SdkMember>();
            if (members?.Count > 0)
            {
                sdkmember.AddRange(members.Select(c => new SdkMember
                {
                    userId = c?.userId,
                    userName = c?.userName
                }));
            }
            var sdk = new AddChatRoomMembersInput
            {
                operateId = operateId,
                roomName = roomName,
                roomId = roomId,
                members = sdkmember
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：删除聊天室成员输入信息
    /// </summary>
    public class AntSdkDeleteChatRoomMembersInput
    {
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 操作者账号，必须是创建者才可以操作
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<SdkMember> members { get; set; }

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal DeleteChatRoomMembersInput GetSdk()
        {
            var sdkmember = new List<SdkMember>();
            if (members?.Count > 0)
            {
                sdkmember.AddRange(members.Select(c => new SdkMember
                {
                    userId = c?.userId,
                    userName = c?.userName
                }));
            }
            var sdk = new DeleteChatRoomMembersInput
            {
                operateId = operateId,
                roomId = roomId,
                members = sdkmember
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：更新成聊天室员属性输入信息
    /// </summary>
    public class AntSdkUpdateChatRoomMemberInput
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
        /// 操作者ID
        /// </summary>
        public string operateId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 用户描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 用户备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 用户其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal UpdateChatRoomMemberInput GetSdk()
        {
            var sdk = new UpdateChatRoomMemberInput
            {
                userId = userId,
                userName = userName,
                roomId = roomId,
                operateId = operateId,
                desc = desc,
                remark = remark,
                attr1 = attr1,
                attr2 = attr2,
                attr3 = attr3,
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：更新聊天室输入信息
    /// </summary>
    public class AntSdkUpdateChatRoomInput
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
        /// 聊天室描述
        /// </summary>
        public string desc { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr1 { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr2 { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室其他信息
        /// </summary>
        public string attr3 { get; set; } = string.Empty;

        /// <summary>
        /// “0”未开启，”1”开启机器人
        /// </summary>
        public string robotFlag { get; set; } = string.Empty;

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;

        /// <summary>
        /// 返回SDK需要参数
        /// </summary>
        /// <returns></returns>
        internal UpdateChatRoomInput GetSdk()
        {
            var sdk = new UpdateChatRoomInput
            {
                roomId = roomId,
                operateId = operateId,
                desc = desc,
                remark = remark,
                attr1 = attr1,
                attr2 = attr2,
                attr3 = attr3,
                roomName = roomName,
                robotFlag = robotFlag,
                robotType = robotType
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：查找聊天室输出信息
    /// </summary>
    public class AntSdkFindRoomsOutput
    {
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;
    }

    /// <summary>
    ///  触角SDK：查询单个聊天室详细输出信息
    /// </summary>
    public class AntSdkGetChatRoomInfoOutput
    {
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
        /// 聊天室名称
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
        /// 创建时间（时间戳）
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间（时间戳）
        /// </summary>
        public string updateTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public string updateBy { get; set; } = string.Empty;

        /// <summary>
        /// 是否开启机器人
        /// </summary>
        public string robotFlag { get; set; } = string.Empty;

        /// <summary>
        /// 机器人类型
        /// </summary>
        public string robotType { get; set; } = string.Empty;

        /// <summary>
        /// 一次最多拉200个成员
        /// </summary>
        public List<AntSdkMember> members { get; set; }
    }

    /// <summary>
    /// 触角SDK：查询聊天室所有成员输出基本信息
    /// </summary>
    public class AntSdkFindRoomMembersOutput
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
    /// 触角SDK：查询聊天室单个成员详细信息输出
    /// </summary>
    public class AntSdkGetRoomMemberInfoOutput
    {
        /// <summary>
        /// 用户名称
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

        /// <summary>
        /// 创建时间（时间戳）
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 修改时间（时间戳）
        /// </summary>
        public string updateTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// 邀请加入聊天室(输入参数）
    /// </summary>
    public class AntSdkInviteJoinRoomInput
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
        /// 聊天室ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string targetName { get; set; } = string.Empty;

        /// <summary>
        /// 被邀请者id
        /// </summary>
        public string handleId { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal InviteJoinRoomInput GetSdk()
        {
            var sdk = new InviteJoinRoomInput
            {
                userId = userId,
                userName = userName,
                targetId = targetId,
                targetName = targetName,
                handleId = handleId,
                remark = remark,
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：处理邀请输入信息
    /// </summary>
    public class AntSdkHandleInviteInput
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
        /// 聊天室ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string targetName { get; set; } = string.Empty;

        /// <summary>
        /// 邀请者id
        /// </summary>
        public string handleId { get; set; } = string.Empty;

        /// <summary>
        /// 处理方式：1为同意邀请，该人员被加入到聊天室；2为拒绝邀请
        /// </summary>
        public string state { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal HandleInviteInput GetSdk()
        {
            var sdk = new HandleInviteInput
            {
                userId = userId,
                userName = userName,
                targetId = targetId,
                targetName = targetName,
                handleId = handleId,
                state = state,
                remark = remark,
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：查询用户所在聊天室输出信息
    /// </summary>
    public class AntSdkFindIndividRoomsOutput
    {
        /// <summary>
        /// 聊天室ID
        /// </summary>
        public string roomId { get; set; } = string.Empty;

        /// <summary>
        /// 聊天室名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;

        /// <summary>
        /// 接收消息类型（1.接收并提醒【默认】；2.接收不提醒；3.屏蔽群消息）
        /// </summary>
        public int receiveType { get; set; }
    }

    /// <summary>
    ///  触角SDK：获取用户的讨论组列表，讨论组输出信息   
    /// </summary>
    public class AntSdkGroupInfo
    {
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

        /// <summary>
        /// 群成员数量
        /// </summary>
        public string memberCount { get; set; } = string.Empty;

        /// <summary>
        /// 群主ID
        /// </summary>
        public string groupOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 群管理员ID的集合
        /// </summary>
        public List<string> managerIds { get; set; } = new List<string>();

        /// <summary>
        /// 状态1:接受消息并提;2：接受消息不提醒
        /// </summary>
        public int state { get; set; }
    }

    /// <summary>
    /// 获取讨论组成员信息
    /// </summary>
    public class AntSdkGroupMember
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
        /// 工号
        /// </summary>
        public string userNum { get; set; } = string.Empty;

        /// <summary>
        /// 用户头像
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// 职位
        /// </summary>
        public string position { get; set; } = string.Empty;

        /// <summary>
        /// 0--普通成员 1--群主  2--管理员
        /// </summary>
        public int roleLevel { get; set; }
    }

    /// <summary>
    /// 触角SDK：群主转让输入参数
    /// </summary>
    public class AntSdkGroupOwnerChangeInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 新群主ID
        /// </summary>
        public string newOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal GroupOwnerChangeInput GetSdk()
        {
            var sdk = new GroupOwnerChangeInput
            {
                userId = userId,
                groupId = groupId,
                newOwnerId = newOwnerId
            };
            return sdk;
        }
    }
    /// <summary>
    /// 触角SDK：管理员设置输入参数
    /// </summary>
    public class AntSdkGroupManagerChangeInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        /// 新群主ID
        /// </summary>
        public string newOwnerId { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        public int roleLevel { get; set; } = 0;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal GroupManagerChangeInput GetSdk()
        {
            var sdk = new GroupManagerChangeInput
            {
                userId = userId,
                groupId = groupId,
                newOwnerId = newOwnerId,
                roleLevel = roleLevel
            };
            return sdk;
        }
    }
    /// <summary>
    /// 触角SDK：创建讨论组输入信息
    /// </summary>
    public class AntSdkCreateGroupInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组名称
        /// </summary>
        public string groupName { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组头像，初次创建时，可以不传头像
        /// </summary>
        public string groupPicture { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组成员ID列表
        /// </summary>
        public string[] userIds { get; set; }

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal CreateGroupInput GetSdk()
        {
            var sdk = new CreateGroupInput
            {
                userId = userId,
                groupName = groupName,
                groupPicture = groupPicture,
                userIds = userIds
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：创建讨论组输出信息
    /// </summary>
    public class AntSdkCreateGroupOutput
    {
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

        /// <summary>
        /// 群成员数量
        /// </summary>
        public string memberCount { get; set; } = string.Empty;

        /// <summary>
        /// 群主ID
        /// </summary>
        public string groupOwnerId { get; set; } = string.Empty;


    }

    /// <summary>
    /// 触角SDK：更新讨论组输入信息
    /// </summary>
    public class AntSdkUpdateGroupInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

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
        /// 如果是添加成员，这里存添加的讨论组成员Id集合
        /// </summary>
        public List<string> userIds { get; set; }

        /// <summary>
        /// 如果是添加成员，这里存添加的讨论组成员名称集合
        /// </summary>
        public List<string> userNames { get; set; }

        /// <summary>
        /// 如果是删除成员，这里存删除的讨论组成员Id集合
        /// </summary>
        public List<string> deleteUserIds { get; set; }

        /// <summary>
        /// 如果是删除成员，这里存删除的讨论组成员名称集合
        /// </summary>
        public List<string> delUserNames { get; set; }

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal UpdateGroupInput GetSdk()
        {
            var sdk = new UpdateGroupInput
            {
                userId = userId,
                groupId = groupId,
                groupName = groupName,
                groupPicture = groupPicture,
                userIds = userIds,
                userNames = userNames,
                deleteUserIds = deleteUserIds,
                delUserNames = delUserNames
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：更新用户在讨论组的设置输入信息
    /// </summary>
    public class AntSdkUpdateGroupConfigInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 讨论组ID
        /// </summary>
        public string groupId { get; set; } = string.Empty;

        /// <summary>
        ///  用户在讨论组的消息状态 1:接受并提醒 2:接受不提醒(免打扰) 3:拒绝接受(屏蔽)
        /// </summary>
        public string state { get; set; } = string.Empty;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal UpdateGroupConfigInput GetSdk()
        {
            var sdk = new UpdateGroupConfigInput
            {
                userId = userId,
                groupId = groupId,
                state = state
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：群组公告输出信息
    /// </summary>
    public class AntSdkGetGroupNotificationsOutput
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 公告标题
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 是否有附件，"1"表示有 "0"表示没有----String类型
        /// </summary>
        public string hasAttach { get; set; } = string.Empty;

        /// <summary>
        /// 群组ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 读取状态 0-未读，1-已读
        /// </summary>
        public int readState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 触角SDK：根据ID获取公告输出信息
    /// </summary>
    public class AntSdkGetNotificationsByIdOutput
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 公告标题
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 公告内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 附件
        /// </summary>
        public string attach { get; set; } = string.Empty;

        /// <summary>
        /// 群组ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 读取状态 0-未读，1-已读
        /// </summary>
        public int readState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public string updateBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 触角SDK：添加群组公告输入信息
    /// </summary>
    public class AntSdkAddNotificationsInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群公告的标题，最多32个字
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 群公告的内容，最多500个字
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 附件，Json格式的数组，数组里面的每个对象为，附件上传时返回的JSON对象。 
        /// </summary>
        public string attach { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal AddNotificationsInput GetSdk()
        {
            var sdk = new AddNotificationsInput
            {
                userId = userId,
                title = title,
                content = content,
                attach = attach,
                targetId = targetId
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：添加群组公告输出信息
    /// </summary>
    public class AntSdkAddNotificationsOutput
    {
        /// <summary>
        /// 公告ID
        /// </summary>
        public string notificationId { get; set; } = string.Empty;

        /// <summary>
        /// 公告标题
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 公告内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 附件
        /// </summary>
        public string attach { get; set; } = string.Empty;

        /// <summary>
        /// 群组ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// 读取状态 0-未读，1-已读
        /// </summary>
        public int readState { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public string updateBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 触角SDK：消息漫游输入信息
    /// </summary>
    public class AntSdkRoamMessageInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; }

        /// <summary>
        /// 查询的起始的chatIndex
        /// </summary>
        public int startChatIndex { get; set; }

        /// <summary>
        /// 查询的结束的chatIndex
        /// </summary>
        public int endChatIndex { get; set; }

        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal RoamMessageInput GetSdk()
        {
            var sdk = new RoamMessageInput
            {
                userId = userId,
                sessionId = sessionId,
                startChatIndex = startChatIndex,
                endChatIndex = endChatIndex
            };
            return sdk;
        }
    }

    /// <summary>
    /// 触角SDK：消息漫游输出信息
    /// </summary>
    public class AntSdkRoamMessageOutput
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string messageId { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送者ID
        /// </summary>
        public string sendUserId { get; set; } = string.Empty;

        /// <summary>
        /// 消息接收者ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;

        /// <summary>
        /// app的唯一标识
        /// </summary>
        public string appKey { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 消息发送时间
        /// </summary>
        public string sendTime { get; set; } = string.Empty;

        /// <summary>
        /// 消息游标
        /// </summary>
        public string chatIndex { get; set; } = string.Empty;

        /// <summary>
        /// 会话ID
        /// </summary>
        public string sessionId { get; set; } = string.Empty;
    }
}
