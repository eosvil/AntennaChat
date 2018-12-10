using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDK.Service.Models
{
    public class AddNotificationsInput
    {
        /// <summary>
        /// 当前登陆用户ID
        /// </summary>
        public string userId { get; set; } = string.Empty;

        /// <summary>
        /// 群公告的标题，最多32个字
        /// </summary>
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 群公告的内容，最多500个字
        /// </summary>
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 附件，Json格式的数组，数组里面的每个对象为，附件上传时返回的JSON对象。 
        /// </summary>
        public string attach { get; set; } = string.Empty;

        /// <summary>
        /// 群ID
        /// </summary>
        public string targetId { get; set; } = string.Empty;
    }
}
