using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace XQData
{
    class SQLLog
    {
        public static bool WriteLog(string message)
        {
            if (IsCanWrite())
            {
                return AddLogToData(message);
            }
            return true;
        }
        private static bool IsCanWrite()
        {
            bool IsCanWriteLog;
            bool.TryParse(Convert.ToString(ConfigurationManager.AppSettings["WriteLog"]), out IsCanWriteLog);
            return IsCanWriteLog;
        }
        private static bool AddLogToData(string message)
        {
            string pageUrl = System.Web.HttpContext.Current.Request.Url.ToString();
            SQLHelper helper = new SQLHelper(new SqlConnection(ConfigurationManager.ConnectionStrings["LogsConn"].ToString()));
            helper.WriteLog = false;
            try
            {
                helper.AddParameter("@PageUrl", pageUrl, System.Data.SqlDbType.NVarChar);
                helper.AddParameter("@ErrorMessage", message, System.Data.SqlDbType.NVarChar);
                helper.ExeNonQuery("insert into ErrorLogs(PageUrl,ErrorMessage) values(@PageUrl,@ErrorMessage)", false);
                helper.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
