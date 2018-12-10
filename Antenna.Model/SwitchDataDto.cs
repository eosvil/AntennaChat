using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Antenna.Model
{
    public class SwitchDataDto
    {
        /// <summary>
        /// 回话index
        /// </summary>
        public string chatIndex { get; }
        /// <summary>
        /// 上传路径
        /// </summary>
        public string uploadPath { get; }
    }
}