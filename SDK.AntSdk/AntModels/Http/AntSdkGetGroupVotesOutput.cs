using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkGetGroupVotesOutput
    {
        /// <summary>
        /// 投票内容
        /// </summary>
        public List<GroupVoteContent> content { get; set; } = new List<GroupVoteContent>();
        /// <summary>
        /// 总计个数
        /// </summary>
        public int totalElements { get; set; }
        /// <summary>
        /// 当前页个数
        /// </summary>
        public int numberOfElements { get; set; }
        /// <summary>
        /// 每页个数
        /// </summary>
        public int size { get; set; }
        /// <summary>
        ///总计页数
        /// </summary>
        public int totalPages { get; set; }
        /// <summary>
        /// 页码，从0开始
        /// </summary>
        public int number { get; set; }
        /// <summary>
        /// 是否第一页
        /// </summary>
        public bool first { get; set; }
        /// <summary>
        ///是否最后一页
        /// </summary>
        public bool last { get; set; }
    }

  

    public class GroupVoteContent
    {
        /// <summary>
        /// 投票标识    
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 最大选择数
        /// </summary>
        public int maxChoiceNumber { get; set; }

        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool secret { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string expiryTime { get; set; }
        /// <summary>
        /// 发起人信息
        /// </summary>
        public string createdBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createdDate { get; set; }
        /// <summary>
        ///是否已经投票
        /// </summary>
        public bool voted { get; set; }
    }
    public class AntSdkGroupAbstentionVoteOutput
    {
        public string[] voters { get; set; }
    }
}
