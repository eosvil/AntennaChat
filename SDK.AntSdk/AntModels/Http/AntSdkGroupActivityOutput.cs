using System.Collections.Generic;

namespace SDK.AntSdk.AntModels
{

    /// <summary>
    /// 获取活动列表Output对象
    /// </summary>
    public class AntSdkGetGroupActivitysOutput
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int pageNum { get; set; }
        /// <summary>
        /// 页容量
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 记录总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 活动详情集合
        /// </summary>
        public List<AntSdkGroupActivityDetailOutput> list { get; set; } = new List<AntSdkGroupActivityDetailOutput>();
        /// <summary>
        /// 是否是第一页
        /// </summary>
        public bool isFirstPage { get; set; }
        /// <summary>
        /// 是否是最后一页
        /// </summary>
        public bool isLastPage { get; set; }
    }
    /// <summary>
    /// 活动列表内容对象
    /// </summary>
    public class AntSdkGroupActivityDetailOutput
    {
        /// <summary>
        /// 活动标识
        /// </summary>
        public int activityId { get; set; }
        /// <summary>
        /// 群标识
        /// </summary>
        public string groupId { get; set; }
        /// <summary>
        /// 创建者ID
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 活动主题
        /// </summary>
        public string theme { get; set; }
        /// <summary>
        /// 活动主题图片
        /// </summary>
        public string picture { get; set; }
        /// <summary>
        /// 活动地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 地图经度
        /// </summary>
        public float longitude { get; set; }
        /// <summary>
        /// 地图纬度
        /// </summary>
        public float latitude { get; set; }
        /// <summary>
        /// 活动开始时间
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 活动结束时间
        /// </summary>
        public string endTime { get; set; }
        /// <summary>
        /// 活动提示时间:分钟
        /// </summary>
        public int remindTime { get; set; }
        /// <summary>
        /// 活动介绍
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 进行中（1进行中 2已结束）
        /// </summary>
        public int activityStatus { get; set; }
        /// <summary>
        /// 活动创建时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 是否已参与标识
        /// </summary>
        public bool voteFlag { get; set; }

        /// <summary>
        /// 参与活动人数
        /// </summary>
        public int? voteCount { get; set; }
    }

    /// <summary>
    /// 获取活动参与者列表Output对象
    /// </summary>
    public class AntSdkGetGroupActivityParticipatorsOutput
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int pageNum { get; set; }
        /// <summary>
        /// 页容量
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 记录总数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 参与人详情集合
        /// </summary>
        public List<AntSdkGroupActivityParticipator> list { get; set; } = new List<AntSdkGroupActivityParticipator>();

    }
    /// <summary>
    /// 参与者信息
    /// </summary>
    public class AntSdkGroupActivityParticipator
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string picture { get; set; }
        /// <summary>
        /// 用户编号
        /// </summary>
        public string userNum { get; set; }
    }

    /// <summary>
    /// 活动默认背景图
    /// </summary>
    public class AntSdkGetGroupActivityDefaultImageOutput
    {
        /// <summary>
        /// 户外
        /// </summary>
        public string outdoor { get; set; }
        /// <summary>
        /// 游戏
        /// </summary>
        public string game { get; set; }
        /// <summary>
        /// 唱K
        /// </summary>
        public string sing { get; set; }
        /// <summary>
        /// 聚餐
        /// </summary>
        public string dinner { get; set; }
        /// <summary>
        /// 体育
        /// </summary>
        public string sport { get; set; }
    }

}


