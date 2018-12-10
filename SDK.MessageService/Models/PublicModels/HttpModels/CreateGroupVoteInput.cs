using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class CreateGroupVoteInput
    {
        ///// <summary>
        ///// 群ID
        ///// </summary>
        //public string id { get; set; }

        /// <summary>
        /// 投票的主题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 投票选项(选项数组，范围2-50个元素)
        /// </summary>
        public List<VoteOptionContent> options { get; set; }=new List<VoteOptionContent>();

        /// <summary>
        /// 多选还是单选
        /// </summary>
        public int maxChoiceNumber { get; set; }

        /// <summary>
        /// 截止时间，默认为1天之后，最小为一分钟之后，最大为一年之后，yyyy-MM-dd HH: mm:ss
        /// </summary>
        public string expiryTime { get; set; }

        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool secret { get; set; }
        /// <summary>
        /// 创建者用户标识
        /// </summary>
        public string createdBy { get; set; }
    }
    /// <summary>
    ///删除投票
    /// </summary>
    internal class DeteleGroupVoteInput : IInputQuery
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 获取Query型参数对应的路径信息
        /// </summary>
        string IInputQuery.GetQuery() => $"?userId={userId}";
    }
    public class VoteOptionContent
    {
        /// <summary>
        /// 选项名称，范围1-30个字符
        /// </summary>
        public string name { get; set; }
    }

    public class GroupVoteOptionInput
    {
        public List<VoteOption> votes { get; set; } = new List<VoteOption>();
    }

    public class VoteOption
    {
        /// <summary>
        /// 选项标识
        /// </summary>
        public int votingOptionId { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string createdBy { get; set; }

    }
}


