using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace DSRCManagementSystem.Models
{
    public class AllMail
    {
        public static void GetTotalworkinghours(int month, int ProjectId, int Week, DataTable dt)
        {
            SqlParameter[] objParam = new SqlParameter[3];
            objParam[0] = new SqlParameter("@Month", month);
            objParam[1] = new SqlParameter("@ProjectId", ProjectId);
            objParam[2] = new SqlParameter("@Week", Week);
            dt.Fill("SP_GetTotalworkinghours", objParam);

        }
        public static void MasterList(DataTable dt)
        {
            dt.Fill("SP_MasterList");
        }

        public static void Names(DataTable dt)
        {
            dt.Fill("SP_Names");

        }
        public static void GetReportingPerson(DataTable dt)
        {
            dt.Fill("SP_GetReportingPerson");
        }
        public static void GetProjectMapping(DataTable dt)
        {
            dt.Fill("SP_GetProjectMapping");
        }
    }
}