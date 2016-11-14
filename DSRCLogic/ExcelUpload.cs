using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;

namespace DSRCManagementSystem.DSRCLogic
{
    public class ExcelUpload
    {
        public static void ImportTimeEntryExcelToDatabase(string filePath)
        {
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {

                String excelConnString = "";
                string extension = Path.GetExtension(filePath);
                switch (extension)
                {
                    case ".xls": //Excel 97-03
                        excelConnString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                        break;
                    case ".xlsx": //Excel 07 or higher
                        excelConnString = ConfigurationManager.ConnectionStrings["Excel07+ConString"].ConnectionString;
                        break;

                }
                excelConnString = string.Format(excelConnString, filePath);
                //Create Connection to Excel work book 
                using (OleDbConnection excelConnection = new OleDbConnection(excelConnString))
                {
                    //Create OleDbCommand to fetch data from Excel 
                    using (OleDbCommand cmd = new OleDbCommand("select EmpId,Date,InTime,OutTime from [sheet1$] where EmpId is not null", excelConnection))
                    {
                        DataTable dt = new DataTable();
                        DataSet ds = new DataSet();
                        OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                        da.Fill(dt);
                        ds.Tables.Add(dt);
                       // excelConnection.Open();
                      

                        using (OleDbDataReader dReader = cmd.ExecuteReader())
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(dbHrms.Connection.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
                            {
                                sqlBulk.DestinationTableName = "TimeManagement";
                                sqlBulk.BatchSize = 2;
                                sqlBulk.ColumnMappings.Add("EmployeeId", "EmpID");
                                sqlBulk.ColumnMappings.Add("InTime", "InTime");
                                sqlBulk.ColumnMappings.Add("OutTime", "OutTime");
                                sqlBulk.WriteToServer(dReader);
                            }
                        }
                    }
                }
            }
        }
    }
}