using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 获取讨论组成员信息输入参数
    /// </summary>
    public class GetGroupMembersInput:BaseInput
    {
        public string groupId { get; set; }//讨论组ID
    }
    /// <summary>
    /// 获取讨论组成员信息输入参数
    /// </summary>
    public class GetGroupMembersOutput : BaseOutput
    {
        public List<GetGroupMembers_User> users { get; set; }
    }
    public class GetGroupMembers_User
    {
        public string userId { get; set; }//用户ID
        public string userName { get; set; }//成员名字
        public string picture { get; set; }//成员头像地址
        public int roleLevel { get; set; }//成员角色等级，0:普通成员，1:管理员
        public string position { get; set; }//职位
        public string userNum { get; set; } //工号
    }
}
