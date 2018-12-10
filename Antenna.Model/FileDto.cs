using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Antenna.Model
{
    /// <summary>
    /// 处理文件上传类 content
    /// </summary>
    public class FileDto : BaseBoolDto
    {
        public string fileUrl { set; get; }
        public string size { set; get; }
        public string fileName { set; get; }
        public string fileExtendName { set; get; }
        public string dowmnloadUrl { set; get; }
        public string thumbnailUrl { set; get; }
    }
    public class ImageDto
    {
        /// <summary>
        /// 图片路径
        /// </summary>
        public string picUrl { set; get; }
    }
    /// <summary>
    /// 处理批量图片上传类
    /// </summary>
    public class BatchImage
    {
        /// <summary>
        /// 析构
        /// </summary>
        ~BatchImage() { GC.Collect(); }
        /// <summary>
        /// 图片类
        /// </summary>
        public Image image { set; get; }
        /// <summary>
        /// 消息Id
        /// </summary>
        public string messageId { set; get; }
    }
}
