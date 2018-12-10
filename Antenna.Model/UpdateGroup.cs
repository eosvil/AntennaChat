using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
   public  class UpdateGroupInput:BaseInput
    {
        public string groupId { get; set; }//讨论组id
        public string groupName { get; set; }//讨论组名称
        public string userIds { get; set; }//讨论组成员
        public string userNames { get; set; }//讨论组成员名称
        public string deleteUserIds { get; set; }//删除的讨论组成员Id
        public string delUserNames { get; set; }//删除成员名称
        public string groupPicture { get; set; }//讨论组图片
    }
    public class UpdateGroupConfigInput:BaseInput
    {
        public string groupId { get; set; }//讨论组id
        public int state { get; set; }//用户讨论组状态设置 1：接受消息并提醒 2：接受消息不提醒
    }
}
