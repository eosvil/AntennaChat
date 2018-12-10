using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 系统用户消息接收:mtp为6
    /// </summary>
    /// 创建者：赵雪峰 20160921
    public class SysUserMsgReceive
    {
        public string type { get; set; }//内容类型
        #region 用户信息相关
        //type=101：在线登陆 102：离开登陆 103：忙碌登陆 104: 用户信息更新 105: 用户被踢出登陆 100：退出
        public string userId { get; set; }//用户Id
        public string sex { get; set; }//用户性别
        public string picture { get; set; }//用户头像
        public string signature { get; set; }//用户个性签名
        #endregion

        #region 讨论组新增、删除、成员更新、基本信息更新
        //type=201：讨论组新增 202：讨论组删除 203：讨论组成员更新 204: 讨论组基本信息更新  205:免打扰同步
        public string targetId { get; set; }//消息接收者ID（201,202,203,204）
        public string companyCode { get; set; }//公司代码（201,202,203,204）
        public string messageType { get; set; }//消息类型（201,202,203,204）
        public string sessionId { get; set; }//会话ID是为了维持Redis结构（201,202,203,204）
        public string groupId { get; set; }//讨论组ID（201,202,203,204）
        public string groupName { get; set; }//讨论组名称（201,204）
        public string groupPicture { get; set; }//讨论组头像（201,204）
        public string chatIndex { get; set; }//通知的游标 （201,202,204） 
        public string addMsg { get; set; }//有成员添加，添加成员的名称(203)
        public string delMsg { get; set; }//有成员删除，删除成员的名称(203)
        public string adminId { get; set; }//群主ID(群主退出讨论组，产生新的群主)(203)
        public string adminName { get; set; }//群主名称(群主退出讨论组，产生新的群主)(203)
        public string addIds { get; set; }//有成员添加，添加成员的ID	
        public string delIds { get; set; }//有成员删除，删除成员的ID
        public string operateId { get; set; }//操作者ID
        public int state { get; set; }//免打扰状态  1-接受消息并提醒 2-接受消息不提醒
        #endregion

        #region 组织架构更新
        //type=301：更新组织架构
        public string dataVersion { get; set; }//新的组织架构数据版本号

        //type=401：通知服务_登录推送 402：通知服务_添加推送 403：通知服务_删除推送 404: 通知服务_修改已读状态 
        public string sendUserId { get; set; }//发送者ID
        //public string content { get; set; }//
        public string notificationId { get; set; }
        #endregion
        #region 讨论组阅后即焚状态切换
        public string sendTime { get; set; }//消息发送时间
        public string maxIndex { get; set; }//小于等于maxIndex的阅后即焚消息全部删除
        #endregion
    }

    public class SysUserMsgReceive_Notice_Login : SysUserMsgReceive
    {
        public List<Notice_Login_content> content { get; set; }//
    }

    public class SysUserMsgReceive_Notice_Add : SysUserMsgReceive
    {
        public Notice_content content { get; set; }//
    }

    public class Notice_Login_content
    {
        public string targetId { get; set; }//讨论组ID
        public List<Notice_content> list { get; set; }//讨论组targetId的未读通知
    }

    public class Notice_content
    {
        public string notificationId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string targetId { get; set; }
        public string createTime { get; set; }
        public string createId { get; set; }//创建人ID
        public int hasAttach { get; set; }//是否有附件“0”没有，“1”有
        public string groupId { set; get; }//讨论组ID
    }
}
