using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnexLib
{
    public class MarkdownConfig : ReverseMarkdown.Config
    {
        #region Properties
        /// <summary>
        /// 附件路径，默认为 ""
        /// </summary>
        /// <remarks>
        /// 1. 附件路径 为 ""
        /// hello.md
        /// hello/image-a.png
        /// 2. 附件路径 为 "files"
        /// hello.md
        /// hello/files/image-a.png
        /// </remarks>
        public string AttachmentPath { get; set; }

        /// <summary>
        /// 是否 内联保存图片，即在 markdown 文件内保存 base64 编码的图片，默认为 false
        /// </summary>
        public bool UseInlineImage { get; set; }

        /// <summary>
        /// 是否使用 唯一标识 作为 markdown 文件名，默认为 false, 使用 原始标题 作为文件名
        /// </summary>
        public bool UseUniqueIdFileName { get; set; }
        #endregion

        #region Ctor
        public MarkdownConfig()
        {
            TableWithoutHeaderRowHandling = TableWithoutHeaderRowHandlingOption.EmptyRow;
            GithubFlavored = true;
            //AttachmentPath = "files/";
            AttachmentPath = "";
            UseInlineImage = false;
            UseUniqueIdFileName = false;
        } 
        #endregion
    }
}
