
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antenna.Model
{
    public class AddImgUrlDto
    {
        /// <summary>
        /// 图片插入前或者后
        /// </summary>
        public enum InsertPreOrEnd
        {
            /// <summary>
            /// 前
            /// </summary>
            Pre = 0,
            /// <summary>
            /// 后
            /// </summary>
            End
        }
        public InsertPreOrEnd PreOrEnd { set; get; }
        public string ChatIndex { set; get; }
        public string ImageUrl { set; get; }
        public string ImageId { set; get; }
        public string messageId { set; get; }
        public burnMsg.isBurnMsg IsBurn { set; get; }
        /// <summary>
        /// 是否阅后即焚图片已经读了
        /// </summary>
        public burnMsg.IsReadImg IsRead { set; get; }
        /// <summary>
        /// 阅后即焚图片是否有效
        /// </summary>
        public burnMsg.IsEffective IsEffective { set; get; }
    }

    public class BurnAndNotBurnImgList
    {
        public List<AddImgUrlDto> NotBurn { set; get; }
        public List<AddImgUrlDto> YesBurn { set; get; }
    }
}
