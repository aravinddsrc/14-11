using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Web.SessionState;
using System.Net.Mail;
using DSRCManagementSystem;
using System.Net;
using System.Web.Security;
using System.Text.RegularExpressions;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Management;
using System.Globalization;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Configuration;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;

namespace DSRCManagementSystem.Controllers
{
    public class DailyReportController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult DailyReport()
        {
            List<object> EmployeeList = new List<object>();
            var users = db.Users.Where(x => x.IsActive == true && x.UserStatus != 6).Select(o => o.UserID).ToList();

            foreach (var x in users)
            {
                var name = db.Users.Where(p => p.UserID == x && p.IsActive == true && p.UserStatus != 6)
            .Select(o => o.FirstName + " " + o.LastName)
            .FirstOrDefault();
                var Val = new { ID1 = x, UserName1 = name };
                EmployeeList.Add(Val);
            }


            ViewBag.To = new MultiSelectList(EmployeeList, "Id1", "UserName1");


            var StatusList = db.Actions.Select(c => new
            {
                ActionID = c.ActionID,
                ActionStatus = c.ActionName
            }).ToList();
            ViewBag.ActionList = new SelectList(StatusList, "ActionID", "ActionStatus");
            return View();
        }


        [HttpPost]
        public ActionResult DailyReport(string DES, string SDate, string EDate, string EXDate,string Status)
        {

            List<string> DESCRIPTION = new List<string>();
            string[] value = DES.Split(',');
            for (int k = 0; k < value.Count(); k++)
            {
               
                    DESCRIPTION.Add(value[k].Replace(",", "''"));
                
            }


            List<string> STARTDATE = new List<string>();
            string[] value1 = SDate.Split(',');
            for (int k = 0; k < value1.Count(); k++)
            {
                
                    STARTDATE.Add(value1[k].Replace(",", "''"));
                
            }


            List<string> ENDDATE = new List<string>();
            string[] value2 = EDate.Split(',');
            for (int k = 0; k < value2.Count(); k++)
            {
                
                    ENDDATE.Add(value2[k].Replace(",", "''"));
                
            }

            List<string> EXDATE = new List<string>();
            string[] value3 = EXDate.Split(',');
            for (int k = 0; k < value3.Count(); k++)
            {
                
                    EXDATE.Add(value3[k].Replace(",", "''"));
                
            }

            List<string> STATUS = new List<string>();
            string[] value4 = Status.Split(',');
            for (int k = 0; k < value4.Count(); k++)
            {

                STATUS.Add(value4[k].Replace(",", "''"));

            }

            var CurDate = DateTime.Now.Day;
            var CurMonth = DateTime.Now.Month;
            var CurYear = DateTime.Now.Year;
            var CurHour = DateTime.Now.Hour;
            var CurMinute = DateTime.Now.Minute;
            var CurSec = DateTime.Now.Second;
            string path = @"C:\" + "Excel";  // Give the specific path
            if (!(Directory.Exists(path)))
            {

                Directory.CreateDirectory(path);
            }
            var paths = "C:\\Excel\\Status on" + " " + CurDate + "-" + CurMonth + "-" + CurYear + " " + CurHour + "." + CurMinute + "." + CurSec + ".xls";
            StreamWriter wr = new StreamWriter(paths);



            //wr.Write("BugID"+ "\t");
            //wr.Write("Description" + "\t");
            //wr.Write("Start Date" + "\t");
            //wr.Write("End Date" + "\t");
            //wr.Write("Expected Date" + "\t");
            //wr.Write("Status" + "\t");
            //wr.WriteLine();


            //foreach (var item1 in objuser)
            //    {
            //        wr.Write("" + "\t");
            //        wr.Write(Convert.ToString(item1) + "\t");
            //        wr.Write( CurDate+"-"+CurMonth+"-"+CurYear+ "\t");
            //        wr.Write(CurDate + "-" + CurMonth + "-" + CurYear  +"\t");
            //        wr.Write("" + "\t");
            //        wr.Write("" + "\t");
            //        wr.WriteLine();
            //    }



            string str = string.Empty;

            str += "<Table border=1><TR><TD bgcolor='#b3b3b3'>BugID</TD><TD bgcolor='#b3b3b3'>Description</TD><TD bgcolor='#b3b3b3'>Start Date</TD><TD bgcolor='#b3b3b3'>End Date</TD><TD bgcolor='#b3b3b3'>Expected Date</TD><TD bgcolor='#b3b3b3'>Status</TD></TR>";


            //foreach (var item1 in DESCRIPTION)
            //{
            //    str += "<TR>";
            //    str += "<TD >" + "" + "</TD>";
            //    str += "<TD >" + item1 + "</TD>";
            //    str += "<TD >" + CurDate + "-" + CurMonth + "-" + CurYear + "</TD>";
            //    str += "<TD >" + CurDate + "-" + CurMonth + "-" + CurYear + "</TD>";
            //    str += "<TD >" + "" + "</TD>";
            //    str += "<TD >" + "" + "</TD>";
            //    str += "</TR>";
            //}


            for (int i = 0; i < DESCRIPTION.Count(); i++)
            {
                
                

                str += "<TR>";
                str += "<TD >" +" "+"</TD>";
                str += "<TD >" + DESCRIPTION[i]+"</TD>";
                for (int j = i; j < i + 1; j++)
                {
                    str += "<TD >" + STARTDATE[j] + "</TD>";
                    for (int k = j; k < j + 1; k++)
                    {

                        str += "<TD >" + ENDDATE[k] + "</TD>";

                        for (int l = k; l < k + 1; l++)
                        {
                            str += "<TD >" + EXDATE[l] + "</TD>";

                            for (int m = l; m < l + 1; m++)
                            {
                                str += "<TD >" + STATUS[m] + "</TD>";   
                            }
                        }
                        
                    }
                }
             
                str += "</TR>";
            }




            str += "</TABLE>";


            wr.WriteLine(str);



       
            //string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //return File(file, contentType, Path.GetFileName(file));

        
            wr.Close();
           
            ViewData["paths"] = paths;
            ViewBag.paths = paths;
            TempData["paths"] = paths;


            //Download(paths);




            return View(ViewBag.paths);
        }

        public ActionResult Download(string paths)
        {
            string Paths =Convert.ToString( TempData["paths"]);
            string file = Paths;
            //string file = @"c:\Excel\Report.xls";

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(file, contentType, Path.GetFileName(file));

            
            //String strRequest = Request.QueryString["file"];
            //FileInfo file = new FileInfo(paths);
            //if (file.Exists)
            //{
            //    Response.Clear();
            //    Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
            //    Response.AddHeader("Content-Length", file.Length.ToString());
            //    Response.ContentType = "application/octet-stream";
            //    Response.WriteFile(file.FullName);
            //    Response.End();
            //}
            //else
            //{
            //    Response.Write("This file does not exist.");
            //}
            //return Json("Success", JsonRequestBehavior.AllowGet);

    }


    }

}



