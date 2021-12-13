using System;
using System.Collections.Generic;
using System.Text;

namespace coo.Models.CImg
{
    public class UnReferencedImgDeleteModel
    {
        public enum DeleteResultEnum
        {
            DeleteSuccess = 0,
            DeleteFailure = 1,
            DeleteIgnore = 2,
        }

        public DeleteResultEnum DeleteResult { get; set; }

        public string ImgAbsolutePath { get; set; }

    }
}
