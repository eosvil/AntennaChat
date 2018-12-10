namespace SDK.Service.Models
{
    /// <summary>
    /// 发布活动Input对象
    /// </summary>
    public class ReleaseGroupActivityInput
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupId { get; set; }
        /// <summary>
        /// 创建人用户ID
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 活动主题
        /// </summary>
        public string theme { get; set; }
        /// <summary>
        /// 活动主题图片地址
        /// </summary>
        public string picture { get; set; }
        /// <summary>
        /// 地图经度
        /// </summary>
        public float latitude { get; set; }
        /// <summary>
        /// 地图纬度
        /// </summary>
        public float longitude { get; set; }
        /// <summary>
        /// 活动地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 开始时间: 2017-09-13 17:41:00
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 结束时间: 2017-09-14 17:41:00
        /// </summary>
        public string endTime { get; set; }
        /// <summary>
        /// 活动提醒时间:分钟
        /// </summary>
        public int remindTime { get; set; }
        /// <summary>
        /// 活动说明
        /// </summary>
        public string description { get; set; }
    }

    /// <summary>
    /// 获取活动列表Input对象
    /// </summary>
    public class GetGroupActivitysInput : IInputQuery
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupId { get; set; }

        /// <summary>
        /// 活动状态（1进行中 2已结束）
        /// </summary>
        public int activityStatus { get; set; }

        /// <summary>
        /// 页码，默认为0，从0开始
        /// </summary>
        public int pageNum { get; set; }

        /// <summary>
        /// 每页个数
        /// </summary>
        public int pageSize { get; set; }

        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?groupId={groupId}&activityStatus={activityStatus}&userId={userId}&pageNum={pageNum}&pageSize={pageSize}";
    }

    /// <summary>
    /// 获取活动详情Input对象
    /// </summary>
    public class GetGroupActivityDetailsInput : IInputQuery
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupId { get; set; }

        /// <summary>
        /// 活动ID
        /// </summary>
        public int activityId { get; set; }

        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?groupId={groupId}&activityId={activityId}&userId={userId}";
    }
    /// <summary>
    /// 删除活动Input对象
    /// </summary>
    public class DeleteGroupActivityInput : IInputQuery
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        public string activityId { get; set; }

        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?activityId={activityId}&userId={userId}";
    }
    /// <summary>
    /// 获取活动参与者列表Input对象
    /// </summary>
    public class GetGroupActivityParticipatorInput : IInputQuery
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupId { get; set; }
        /// <summary>
        /// 活动ID
        /// </summary>
        public int activityId { get; set; }

        /// <summary>
        /// 页码，默认为0，从0开始
        /// </summary>
        public int pageNum { get; set; }

        /// <summary>
        /// 每页个数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?groupId={groupId}&activityId={activityId}&pageNum={pageNum}&pageSize={pageSize}";
    }

    /// <summary>
    /// 参与活动Input对象
    /// </summary>
    public class ParticipateActivitiesInput : IInputQuery
    {
        /// <summary>
        /// 活动ID
        /// </summary>
        public int activityId { get; set; }

        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;
        /// <summary>
        /// 路径型参数必须实现路径合成
        /// </summary>
        string IInputQuery.GetQuery() => $"?activityId={activityId}&userId={userId}";
    }
}

