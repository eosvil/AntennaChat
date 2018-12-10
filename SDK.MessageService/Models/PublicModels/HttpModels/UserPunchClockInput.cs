using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    /// <summary>
    /// 确认打卡输入对象
    /// </summary>
    public class ConfirmPunchInput
    {
        /// <summary>
        /// 打卡ID
        /// </summary>
        public string attendId { get; set; }

        /// <summary>
        /// 用户IP
        /// </summary>
        public string userIp { get; set; }
    }
    /// <summary>
    /// 获取打卡记录列表输入对象
    /// </summary>
    public class GetPunchClocksInput
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int pageNum { get; set; }

        /// <summary>
        /// 页面大小
        /// </summary>
        public int pageSize { get; set; }
    }
    /// <summary>
    /// 获取打卡记录列表输出对象
    /// </summary>
    public class GetPunchClocksOutput
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
        public List<PunchClockDetail> list { get; set; } = new List<PunchClockDetail>();
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
    /// 打卡记录详情
    /// </summary>
    public class PunchClockDetail
    {
        /// <summary>
        /// 打卡记录id
        /// </summary>
        public string attendId { get; set; }
        /// <summary>
        /// 打卡公司配置id
        /// </summary>
        public string configId { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public string attendTime { get; set; }
        /// <summary>
        /// 验证时间
        /// </summary>
        public string signTime { get; set; }
        /// <summary>
        /// 用户ip
        /// </summary>
        public string userIP { get; set; }
        /// <summary>
        /// 打卡位置
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// mac地址
        /// </summary>
        public string mac { get; set; }
        /// <summary>
        /// imei手机唯一标识码
        /// </summary>
        public string imei { get; set; }
        /// <summary>
        /// 精度
        /// </summary>
        public float longItude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public float latitude { get; set; }
        /// <summary>
        /// 打卡状态0:待验证1:验证失败2:成功
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }


}