using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 创建讨论组输入参数实体
    /// </summary>
    public  class CreateGroupInput:BaseInput 
    {
        public string groupName { get; set; }//讨论组名称
        public string groupPicture { get; set; }//讨论组图片
        public string userIds { get; set; }//讨论组成员
    }
    /// <summary>
    /// 创建讨论组输出参数实体
    /// </summary>
    public class CreateGroupOutput:BaseOutput
    {
        public CreateGroupOutput_Group group { get; set; }
    }
    public class CreateGroupOutput_Group
    {
        public string groupId { get; set; }//创建成功后的讨论组ID
        public string groupName { get; set; }//创建成功后的讨论组名称
        public string groupPicture { get; set; }//讨论组的url
    }
}
