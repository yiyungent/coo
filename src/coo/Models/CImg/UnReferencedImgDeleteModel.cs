using System;
using System.Collections.Generic;
using System.Text;

namespace coo.Models.CImg
{
    public class UnReferencedImgDeleteModel
    {
        public enum DeleteStatusEnum
        {
            /// <summary>
            /// 执行删除后, 此状态不再存在, 转为: DeleteSuccess 或 DeleteFailure
            /// </summary>
            NeedDelete = 0,
            DeleteSuccess = 1,
            DeleteFailure = 2,
            DeleteIgnore = 3,
        }

        public DeleteStatusEnum DeleteStatus { get; set; }

        public string ImgAbsolutePath { get; set; }

    }
}
