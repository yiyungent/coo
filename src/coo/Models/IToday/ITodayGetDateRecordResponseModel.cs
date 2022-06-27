using System;
using System.Collections.Generic;
using System.Text;

namespace coo.Models.IToday
{
    public class ITodayGetDateRecordResponseModel
    {
        public DataModel data { get; set; }

        public string info { get; set; }

        public int status { get; set; }

        public class DataModel
        {
            public ItemModel[] items { get; set; }

            public class ItemModel
            {
                public long id { get; set; }
                public long userId { get; set; }
                public long goalId { get; set; }
                public long goalType { get; set; }
                public string startTime { get; set; }
                public long take { get; set; }
                public string stopTime { get; set; }
                public int isEnd { get; set; }
                public long isRecord { get; set; }
                public string remarks { get; set; }
                public int isDelete { get; set; }
                public string deleteTime { get; set; }
                public string endUpdateTime { get; set; }
            }
        }
    }
}
