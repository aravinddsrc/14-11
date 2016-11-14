using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class VisitorsController : Controller
    {
        //
        // GET: /Visitors/       
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult VisitorsDetails()
        {
            
                var auditlog = (from audit in db.AuditLogs
                                join users in db.Users on audit.LoginID equals users.UserName
                                select new DSRCManagementSystem.Models.AuditLogs()
                                {
                                    FirstName = users.FirstName + " " + (users.LastName ?? ""),
                                    Role = audit.Roles,
                                    UserName = audit.LoginID,
                                    LogInDate = audit.LogedInDate,
                                    LogOutDate = audit.LoggedOutDate,
                                    IpAddress = audit.IpAddress,
                                    BrowserName = audit.BrowserVersion,
                                    OSName = audit.OsVersion,
                                    Location = audit.Location
                                }).OrderByDescending(o => o.LogInDate).Take(500);
                //.OrderByDescending(o => o.LogInDate.Value.Year).ThenByDescending(o => o.LogInDate.Value.Month).ThenByDescending(o => o.LogInDate.Value.Day).Take(500);
                var RoleType = db.Master_Roles.ToList();
                ViewBag.RoleTypeList = new SelectList(RoleType, "RoleID", "RoleName");
                var months = db.Master_TSR_Month.ToList();
                ViewBag.MonthList = new SelectList(months, "Id", "Month");
            return View(auditlog);
        }

        [HttpPost]
        public ActionResult VisitorsDetails(FormCollection form)
        {
                List<DSRCManagementSystem.Models.AuditLogs> visitors = new List<DSRCManagementSystem.Models.AuditLogs>();
            try
            {
                string RoleType = (form["Roles"] == "") ? "0" : form["Roles"].ToString();
                int RoleTypeID = int.Parse(RoleType.ToString());
                string Month = (form["Month"] == "") ? "0" : form["Month"].ToString();
                int Months = int.Parse(Month.ToString());

                if (RoleTypeID == 0 && Months == 0)
                {
                    return RedirectToAction("VisitorsDetails", "Visitors");
                }
                else if (RoleTypeID != 0 && Months != 0)
                {
                    visitors = (from audit in db.AuditLogs
                                join roles in db.Master_Roles on audit.Roles equals roles.RoleName
                                join users in db.Users on audit.LoginID equals users.UserName
                                where roles.RoleID == RoleTypeID && audit.LogedInDate.Value.Month == Months
                                select new DSRCManagementSystem.Models.AuditLogs()
                                {
                                    FirstName = users.FirstName + " " + (users.LastName ?? ""),
                                    Role = audit.Roles,
                                    UserName = audit.LoginID,
                                    LogInDate = audit.LogedInDate,
                                    LogOutDate = audit.LoggedOutDate,
                                    IpAddress = audit.IpAddress,
                                    BrowserName = audit.BrowserVersion,
                                    OSName = audit.OsVersion,
                                    Location = audit.Location
                                }).OrderByDescending(x => x.LogInDate).ToList();
                    //.OrderByDescending(o =>o.LogInDate.Value.Year).ThenByDescending(o =>o.LogInDate.Value.Month).ThenByDescending(o=>o.LogInDate.Value.Day).ToList();


                }
                else
                    if (RoleTypeID != 0)
                    {
                        visitors = (from audit in db.AuditLogs
                                    join roles in db.Master_Roles on audit.Roles equals roles.RoleName
                                    join users in db.Users on audit.LoginID equals users.UserName
                                    where roles.RoleID == RoleTypeID
                                    select new DSRCManagementSystem.Models.AuditLogs()
                                    {
                                        FirstName = users.FirstName + " " + (users.LastName ?? ""),
                                        Role = audit.Roles,
                                        UserName = audit.LoginID,
                                        LogInDate = audit.LogedInDate,
                                        LogOutDate = audit.LoggedOutDate,
                                        IpAddress = audit.IpAddress,
                                        BrowserName = audit.BrowserVersion,
                                        OSName = audit.OsVersion,
                                        Location = audit.Location
                                    }).OrderByDescending(o => o.LogInDate).ToList();
                        //.OrderByDescending(o => o.LogInDate.Value.Year).ThenByDescending(o => o.LogInDate.Value.Month).ThenByDescending(o => o.LogInDate.Value.Day).ToList();

                    }
                    else if (Months != 0)
                    {
                        visitors = (from audit in db.AuditLogs
                                    join roles in db.Master_Roles on audit.Roles equals roles.RoleName
                                    join users in db.Users on audit.LoginID equals users.UserName
                                    where audit.LogedInDate.Value.Month == Months
                                    select new DSRCManagementSystem.Models.AuditLogs()
                                    {
                                        FirstName = users.FirstName + " " + (users.LastName ?? ""),
                                        Role = audit.Roles,
                                        UserName = audit.LoginID,
                                        LogInDate = audit.LogedInDate,
                                        LogOutDate = audit.LoggedOutDate,
                                        IpAddress = audit.IpAddress,
                                        BrowserName = audit.BrowserVersion,
                                        OSName = audit.OsVersion,
                                        Location = audit.Location
                                    }).OrderByDescending(o => o.LogInDate).ToList();
                        //.OrderByDescending(o => o.LogInDate.Value.Year).ThenByDescending(o => o.LogInDate.Value.Month).ThenByDescending(o => o.LogInDate.Value.Day).ToList();

                    }
                var RoleTypes = db.Master_Roles.ToList();
                ViewBag.RoleTypeList = new SelectList(RoleTypes, "RoleID", "RoleName", RoleTypeID);
                var months = db.Master_TSR_Month.ToList();
                ViewBag.MonthList = new SelectList(months, "Id", "Month", Month);
                visitors = visitors.Distinct().ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
           
            return View(visitors);
        }
        public ActionResult ChartData()
        {
                var list = new List<object>();
            try
            {
                var currentmonth = DateTime.Now.Month;
                var currentyear = DateTime.Now.Year;
                for (int i = 1; i <= currentmonth; i++)
                {
                    var Data = db.AuditLogs.Where(a => a.LogedInDate.Value.Month == i && a.LogedInDate.Value.Year == currentyear).Select(d => d.LogedInDate.Value.Month).Count();
                    var Val = new { m = currentyear + "-" + i, a = Data };
                    list.Add(Val);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
