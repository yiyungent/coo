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
        public string AttachmentPath { get; set; }

        /// <summary>
        /// 是否内联保存图片，即在 markdown 文件内保存 base64 编码的图片，默认为 false
        /// </summary>
        public bool InlineImage { get; set; }

        /// <summary>
        /// 是否使用唯一的 GUID 作为 markdown 文件名，默认为 false，使用原始标题作为文件名
        /// </summary>
        public bool GuidFileName { get; set; }
        #endregion

        #region Ctor
        public MarkdownConfig()
        {
            TableWithoutHeaderRowHandling = TableWithoutHeaderRowHandlingOption.EmptyRow;
            GithubFlavored = true;
            //AttachmentPath = "files/";
            AttachmentPath = "";
            InlineImage = false;
            GuidFileName = false;
        } 
        #endregion
    }
}
