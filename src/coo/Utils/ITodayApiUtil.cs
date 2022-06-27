using System;
using System.Collections.Generic;
using System.Text;

namespace coo.Utils
{
    /// <summary>
    /// https://api.timefriend.vip/ApiV2/
    /// </summary>
    public class ITodayApiUtil
    {
        public void Login(string userName, string password)
        {

        }

        public Models.IToday.ITodayGetDateRecordResponseModel GetDateRecord(DateTime time)
        {
            Models.IToday.ITodayGetDateRecordResponseModel responseModel = new Models.IToday.ITodayGetDateRecordResponseModel();
            try
            {
                string tokenStr = "";
                string activity_flagStr = "112207048";
                string timeStr = time.ToString("yyyy-MM-dd 00:00:00");

            }
            catch (Exception ex)
            {
                LogUtil.Exception(ex);
            }

            return responseModel;
        }
    }
}
