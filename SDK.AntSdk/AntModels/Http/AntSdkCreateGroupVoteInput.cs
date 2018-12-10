using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkCreateGroupVoteInput
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
        public List<VoteOptionContent> options { get; set; } = new List<VoteOptionContent>();

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


        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal CreateGroupVoteInput GetSdk()
        {
            var sdk = new CreateGroupVoteInput
            {
                title = title,
                maxChoiceNumber = maxChoiceNumber,
                expiryTime = expiryTime,
                secret = secret,
                createdBy = createdBy,
            };
            foreach (var content in options)
            {
                if (content == null) continue;
                sdk.options.Add(new Service.Models.VoteOptionContent { name = content.name });
            }

            return sdk;
        }
    }

    public class VoteOptionContent
    {
        /// <summary>
        /// 选项名称，范围1-30个字符
        /// </summary>
        public string name { get; set; }
    }

    public class AntSdkGroupVoteOptionInput
    {
        public List<VoteOption> votes { get; set; } = new List<VoteOption>();
        /// <summary>
        /// 获取平台SDK输入
        /// </summary>
        /// <returns></returns>
        internal GroupVoteOptionInput GetSdk()
        {
            var sdk = new GroupVoteOptionInput();
            foreach (var vote in votes)
            {
                if (vote == null) continue;
                sdk.votes.Add(new Service.Models.VoteOption
                {
                    createdBy = vote.createdBy,
                    votingOptionId = vote.votingOptionId
                });
            }
            return sdk;
        }
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


