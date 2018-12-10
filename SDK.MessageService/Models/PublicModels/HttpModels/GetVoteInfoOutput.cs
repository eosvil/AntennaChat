using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class GetVoteInfoOutput
    {
        /// <summary>
        /// 投票标识
        /// </summary>
        public int id { get; set; }

        /// <summary>
        ///群组标识
        /// </summary>
        public string groupId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        ///投票选项数组
        /// </summary>
        public List<VoteOptionInfo> options { get; set; } = new List<VoteOptionInfo>();
        /// <summary>
        ///最大选择数
        /// </summary>
        public int maxChoiceNumber { get; set; }

        /// <summary>
        /// 截止时间，yyyy-MM-dd HH: mm:ss
        /// </summary>
        public string expiryTime { get; set; }

        /// <summary>
        /// 是否匿名发起
        /// </summary>
        public bool secret { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string createdBy { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public string createdDate { get; set; }

        /// <summary>
        /// 投票人数
        /// </summary>
        public int voters { get; set; }

        /// <summary>
        /// 是否已投票
        /// </summary>
        public bool voted { get; set; }
    }

    public class UserInfo
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }

        /// <summary>
        /// 	头像
        /// </summary>
        public string avatar { get; set; }
    }

    public class VoteOptionInfo
    {
        /// <summary>
        /// 选项标识
        /// </summary>
        public int id { get; set; }

        /// <summary>
        ///选项名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 票数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 当前用户所投选项
        /// </summary>
        public bool voted { get; set; }
    }
}
