using System;
using System.Collections.Generic;
using System.Text;

namespace coo.Models.CImg
{
    public class CImgStatModel
    {
        /// <summary>
        /// (md,html,htm) 文件 数量
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// 本地图片  
        /// 绝对路径
        /// </summary>
        public List<string> AllImageList { get; set; }

        /// <summary>
        /// (md,html,htm) 文件 引用本地图片
        /// </summary>
        public List<string> ReferencedImgAbsolutePathList { get; set; }

        /// <summary>
        /// (md,html,htm) 文件 未引用本地图片
        /// </summary>
        public List<string> UnReferencedImgAbsolutePathList { get; set; }

    }
}
