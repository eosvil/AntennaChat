using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 获取讨论组信息输出参数（输入参数为BaseInput实体）
    /// </summary>
    public class GetGroupsOutput:BaseOutput
    {
        public string version { get; set; }//用户讨论组版本号
        public List<GroupInfo> groups { get; set; }
    }

    public class GroupInfo
    {
        public string groupId { get; set; }//讨论组ID
        public string groupName { get; set; }//讨论组名称
        public string groupPicture { get; set; }//讨论组图片地址
        public int state { get; set; }//1:接受消息并提醒，2：接受消息不提醒
        //public int members { get; set; }//
    }
}
