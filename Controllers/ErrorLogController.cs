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


namespace DSRCManagementSystem.Controllers
{
    public class ErrorLogController : Controller
    {
        //
        // GET: /ErrorLog/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ErrorLogPageDetails()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Result = (from error in db.ExceptionLogs
                          join user in db.Users on error.UserID equals user.UserID
                          orderby error.ExceptionDate descending
                          select new DSRCManagementSystem.Models.Error()
                          {
                              EmpID = user.EmpID,
                              Username = user.FirstName + " " + user.LastName,
                              ExecptionDate = error.ExceptionDate,
                              Method = error.MethodName,
                              Message=error.ExceptionMessage.Substring(0,20),
                              source=error.Source.Substring(0,20),
                              strck=error.StackTrace.Substring(0,20),
                              ExecptionLogID=error.ExceptionLogId
                              
                          }).Take(100);


            return View(Result);
        }
        
        [HttpGet]
        public ActionResult ErrorViewDetails(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var Result = (from error in db.ExceptionLogs
                          join user in db.Users on error.UserID equals user.UserID
                          where error.ExceptionLogId==ID
                          select new DSRCManagementSystem.Models.Error()
                          {
                              EmpID = user.EmpID,
                              Username = user.FirstName + " " + user.LastName,
                              ExecptionDate = error.ExceptionDate,
                              Method = error.MethodName,
                              Message = error.ExceptionMessage,
                              source = error.Source,
                              strck = error.StackTrace

                          }).FirstOrDefault();
            return View(Result);
        }
    }
}
