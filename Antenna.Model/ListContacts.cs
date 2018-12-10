using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    /// <summary>
    /// 获取联系人信息，返回数组格式(输入参数)
    /// </summary>
    /// 创建者：赵雪峰 20160913
    public class ListContactsInput:BaseInput
    {
        public string dataVersion { get; set; }//客服端本地组织架构数据版本号，如果本地没有，表示第一次获取。可以传NULL
    }
    /// <summary>
    /// 获取联系人信息，返回数组格式(输出参数)
    /// </summary>
    public class ListContactsOutput: BaseOutput
    {
        public string dataVersion { get; set; }//本次数据版本号
        public Contacts contacts { get; set; }//所有的联系人和部门
    }
    public class Contacts
    {
        public List<Contact_User> users { get; set; }//所有联系人
        public List<Contact_Depart> departs { get; set; }//所有部门
    }
    public class Contact_User
    {
        public string departmentId { get; set; }//部门ID
        public string position { get; set; }//人员职位
        public string userId { get; set; }//人员id
        public string userName { get; set; }//人员名称
        public string picture { get; set; }//人员头像（有数据传值）

        public string userNum { get; set; }//工号
    }
    public class Contact_Depart
    {
        public string departName { get; set; }//部门名称
        public string departmentId { get; set; }//部门ID
        public string parentDepartId { get; set; }//上级部门ID，如果没有，值为”0”
    }
}
