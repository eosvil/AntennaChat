﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class GetGroupAbstentionVoteInfoOutput
    {
        /// <summary>
        /// 票数
        /// </summary>
        public int votes { get; set; }
        /// <summary>
        /// 投票人数组
        /// </summary>
        public List<UserInfo> voters { get; set; }=new List<UserInfo>();
    }
}
