using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Data;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Reflection;
using DSRCManagementSystem.Models;





namespace DSRCManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        //
        // GET: /Report/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ExcelReport()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var reportnamelist = db.Reports1.ToList();
            ViewBag.reportname_list = new SelectList(new[] { new Report1() { ReportId = 0, ReportName = "---Select---" } }.Union(reportnamelist), "ReportId", "ReportName", 0);

  
     
            DataTable dt=new DataTable();
           AllMail.GetReportingPerson(dt);

           string sourceFile = Server.MapPath("~/Template/EmployeeReportingPerson.xlt");
           string ExcelFolder = Server.MapPath("~/Employee-ReportingPerson/");
           //string ExcelFolder = @"D:\HRMS Latest\DSRCManagementSystem\Employee-ReportingPerson";
              // @"C:\ShareInventoryReports\ExcelSheets\\";
               //Path.GetTempPath();
          // string ExcelFolder = @"C:\ShareInventoryReports\ExcelSheets\\";
        string FileName ="Employee-Reporting Person.xls";
        string destFile = ExcelFolder + FileName;

        if (System.IO.File.Exists(sourceFile))
        {
            if (!Directory.Exists(ExcelFolder))
            {
                Directory.CreateDirectory(ExcelFolder);
            }
            System.IO.File.Copy(sourceFile, destFile, true);
            FileInfo fileInfo = new FileInfo(destFile);
            fileInfo.IsReadOnly = false;
            fileInfo.Refresh();
            CellDataWriterFirstAndOthers(2, 0, dt, destFile, "Employee-Reporting Person");

            
        }

    
            
           DataTable dt1=new DataTable();
           AllMail.GetProjectMapping(dt1);

           string sourceFile1 = Server.MapPath("~/Template/EmployeeProject-Mapping.xlt");
           //string sourceFile1 = Server.MapPath("~/Template/EmployeeProject Mapping.xlt");

          // string ExcelFolder1 = Path.GetTempPath();
           string ExcelFolder1 = Server.MapPath("~/Employee-ProjectMapping/");
         //  string ExcelFolder1 = @"D:\HRMS Latest\DSRCManagementSystem\Employee-ProjectMapping";
        string FileName1 ="Employee-Project Mapping.xls";
        string destFile1 = ExcelFolder1 + FileName1;

        if (System.IO.File .Exists(sourceFile1))
           
        {
            if (!Directory.Exists(ExcelFolder1))
            {
                Directory.CreateDirectory(ExcelFolder1);
            }
            System.IO.File.Copy(sourceFile1, destFile1, true);
            FileInfo fileInfo1 = new FileInfo(destFile1);
            fileInfo1.IsReadOnly = false;
            fileInfo1.Refresh();
            CellDataWriterFirstAndOthers(2, 0, dt1, destFile1, "EmployeeProjectMapping");

           
         
        }
        return View();
        }

        private void CellDataWriterFirstAndOthers(int row, int col, DataTable dt, string FilePath, string SheetName)
        {
              
            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs);
            HSSFSheet sheet = (HSSFSheet)templateWorkbook.GetSheet(SheetName);
            fs.Close();
            int i = 0;
            int r = row;
            foreach (DataRow dr in dt.Rows)
            {
                HSSFRow headerRow4 = (HSSFRow)sheet.CreateRow(r);
       


                int j = 0;

                int c = col;
                foreach (DataColumn dc in dt.Columns)
                {         
                    
                    HSSFCell cell1 = (HSSFCell)headerRow4.CreateCell(c);
                    string value = dt.Rows[i][j].ToString();
                    if (value == "0" || value == "0.00")
                    {
                        value = string.Empty;
                    }
                    sheet.GetRow(r).GetCell(c).SetCellValue(value);
                    j++;
                    c++;
                }
                i++;
                r++;
            }

            
            fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            templateWorkbook.Write(fs);
            fs.Close();
            dt.Columns.Clear();
            dt.Rows.Clear();
        }



        [HttpPost]
        public FileResult ExcelReport(ReportModel model)
        {
            //if (ModelState.IsValid)
            //{
                //var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleAttendanceFile");
                if (model.ReportName == "2")
                {
                    string filename = Server.MapPath(Url.Content("~/Employee-ReportingPerson/Employee-Reporting Person.xls"));

                    string contentType = "application/vnd.ms-excel";
                    return new FilePathResult(filename, contentType)
                    {
                        FileDownloadName = "Employee-Reporting Person" + DateTime.Now.Day.ToString()
                            + '-' + DateTime.Now.Month.ToString()
                            + '-' + DateTime.Now.Year
                            + "_" + DateTime.Now.Hour.ToString()
                            + "-" + DateTime.Now.Minute.ToString()
                            + "-" + DateTime.Now.Second.ToString() + ".xls"
                    };
                }
                else
                //if (model.ReportName == "1")
                {
                    string filename1 = Server.MapPath(Url.Content("~/Employee-ProjectMapping/Employee-Project Mapping.xls"));

                    string contentType1 = "application/vnd.ms-excel";


                    return new FilePathResult(filename1, contentType1)
                    {
                        FileDownloadName = "Employee-Project Mapping" + DateTime.Now.Day.ToString()
                            + '-' + DateTime.Now.Month.ToString()
                            + '-' + DateTime.Now.Year
                            + "_" + DateTime.Now.Hour.ToString()
                            + "-" + DateTime.Now.Minute.ToString()
                            + "-" + DateTime.Now.Second.ToString() + ".xls"
                    };
                }
                // return Json("null", JsonRequestBehavior.AllowGet);

            }



        [HttpGet]
        public ActionResult AddNewReport()
        {

           
            return View();
        }


        [HttpPost]
        public ActionResult AddNewReport( ReportModel model)
        {

            if (ModelState.IsValid)
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

               
                var obj = db.Reports1.CreateObject();
                obj.ReportName = model.ReportName;
                obj.ReportQuery = model.ReportQuery;

                obj.IsActive =true;

                db.Reports1.AddObject(obj);
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
                return View();

        }


    }
}
