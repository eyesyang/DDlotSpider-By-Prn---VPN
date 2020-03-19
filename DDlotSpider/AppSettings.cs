using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CentaLine.Common;
using System.Configuration;

namespace DDlotSpider
{
    public static class AppSettings
    {
        public static string ConnStr
        {
            get { return ConvertUtility.Trim(ConfigurationManager.ConnectionStrings["connStr"]); }
        }

        public static string StartUrl
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["startUrl"]); }
        }

        public static string TableSearch
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["table_search"]); }
        }

        public static string TableResult
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["table_result"]); }
        }

        public static string UserName
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["userName"]); }
        }

        public static string Password
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["password"]); }
        }

        public static string Mode
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["appMode"]); }
        }

        public static string SearchUrl
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["searchUrl"]); }
        }

        public static int LoginOption
        {
            get { return ConvertUtility.ToInt(ConfigurationManager.AppSettings["loginOption"]); }
        }

        public static string StatusFileName
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["statusFileName"]); }
        }

        public static string TimeNum
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["TimeNum"]); }
        }

        public static string Machine
        {
            get
            {
                return ConvertUtility.Trim(ConfigurationManager.AppSettings["Machine"]);
            }
        }

        public static string VerifyImageName
        {
            get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["VerifyImageName"]); }
        }

        public static bool Dama2Enabled
        {
            get { return ConvertUtility.ToBoolean(ConfigurationManager.AppSettings["Dama2Enabled"]); }
        }

        public static int SleepTime
        {
            get
            {
                return ConvertUtility.ToInt(ConfigurationManager.AppSettings["SleepTime"]);
            }
        }

        public static string AppId { get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["app_id"]); } }
        public static string AppKey { get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["app_key"]); } }
        public static string PdId { get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["pd_id"]); } }

        public static string PdKey { get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["pd_key"]); } }

        public static string PredType { get { return ConvertUtility.Trim(ConfigurationManager.AppSettings["pred_type"]); } }
    }
}
