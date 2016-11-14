using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;
using System.Data.Objects.SqlClient;
using System.Web.UI;
namespace DSRCManagementSystem.Controllers
{
    public class AssignReportingController : Controller
    {
        //
        // GET: /AssignReporting/
        public static string RoleName = MasterEnum.NewuserRole.NewEmployeeRole;
        public ActionResult AssignReportingPerson()
        {
            Reporting reporting = new Reporting();
            try
            {
                int Userid = (int)Session["UserId"];
                reporting.EmployeeList = GetNames();
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    ViewBag.ReportingPersons = new MultiSelectList(GetReportingPersons(), "UserId", "Name");
                    return View(reporting);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(reporting);
        }

        [HttpPost]

        public ActionResult AssignReportingPerson(Reporting report)
        {
            try
            {
                Session["AssignReportingPerson"] = null;
                if (ModelState.IsValid)
                {
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {

                        if (report.ReportingPerson != null)
                        {
                            var newReporting = new List<Int32?>(report.ReportingPerson);
                            var oldReporting =
                                db.UserReportings.Where(x => x.UserID == report.EmployeeId)
                                    .Select(x => x.ReportingUserID)
                                    .ToList();

                            var toInsert = newReporting.Except(oldReporting).ToList();
                            var toDelete = oldReporting.Except(newReporting).ToList();

                            if (oldReporting.Count == 0)
                            {
                                foreach (var item in newReporting)
                                {
                                    var insertNew = new UserReporting()
                                    {
                                        UserID = report.EmployeeId,
                                        ReportingUserID = Convert.ToInt32(item)
                                    };
                                    db.UserReportings.AddObject(insertNew);
                                }
                            }
                            else if (toInsert.Count > 0)
                            {
                                foreach (var item in toInsert)
                                {
                                    var insertChanged = new UserReporting()
                                    {
                                        UserID = report.EmployeeId,
                                        ReportingUserID = Convert.ToInt32(item)
                                    };
                                    db.UserReportings.AddObject(insertChanged);
                                }
                            }
                            if (toDelete.Count > 0)
                            {
                                foreach (var item in toDelete)
                                {
                                    var data = new UserReporting();                                    
                                    
                                    if (item == null)
                                    {
                                        data =
                                           db.UserReportings.Where(
                                               x => x.UserID == report.EmployeeId && x.ReportingUserID == null)
                                               .FirstOrDefault();
                                    }
                                    else
                                    {
                                        data =
                                          db.UserReportings.Where(
                                              x => x.UserID == report.EmployeeId && x.ReportingUserID == item)
                                              .FirstOrDefault();
                                    }
                                    db.UserReportings.DeleteObject(data);
                                }
                            }
                        }
                        db.SaveChanges();
                        Session["AssignReportingPerson"] = 1;
                    }
                }
                report.EmployeeList = GetNames();
                ViewBag.ReportingPersons = new MultiSelectList(GetReportingPersons(), "UserId", "Name");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(report);
        }

        public ActionResult AssignedReportingPersons(int id)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var selectedValues =
                    db.UserReportings.Where(x => x.UserID == id).Select(x => x.ReportingUserID).ToList();
                return Json(selectedValues, JsonRequestBehavior.AllowGet);
            }

        }



        private List<ReportingPerson> GetReportingPersons()
        {
            int Userid = (int)Session["UserId"];
            var EmployeeList = new List<ReportingPerson>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    int userId = Convert.ToInt32(Session["UserID"]);
                    var getbranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                    int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == Userid).BranchId;
                    //List<ReportingPerson> reportingPersons = (from r in db.Master_Roles
                    //    //where r.RoleID == 4 || r.RoleID == 8 || r.RoleID == 44 || r.RoleID == 42
                    //    //|| r.RoleID == 40 || r.RoleID == 47 || r.RoleID == 60 || r.RoleID == 67
                    //    //|| r.RoleID == 59 || r.RoleID == 26 || r.RoleID == 30 || r.RoleID == 70 || r.RoleID == 62
                    //    join ur in db.UserRoles on r.RoleID equals ur.RoleID
                    //    join u in db.Users on ur.UserID equals u.UserID
                    //    where u.IsActive == true && u.BranchId == BranchId && u.UserStatus != 6
                    //    //where u.IsActive == true && u.UserID != 282 && u.BranchId == BranchId

                    //    select new ReportingPerson
                    //    {
                    //        UserID = u.UserID,
                    //        Name = (u.FirstName + " " + (u.LastName ?? "")).Trim()
                    //    }).OrderBy(o => o.Name).ToList();

                    var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                      join q in db.UserRoles on p.RoleID equals q.RoleID
                                      select q.UserID).ToList();
                    var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                    foreach (var id in userbyid)
                    {
                        userbyrole.Add(id);
                    }
                    var fullusers = userbyrole.Distinct().ToList();


                    foreach (var userid in fullusers)
                    {
                        var name =
                            db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == getbranch) //&& (x.FirstName != null && x.LastName != null))
                                .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                                .FirstOrDefault();
                        var Val = new ReportingPerson() { UserID = userid, Name = name };
                        if (name != null)
                        {

                            EmployeeList.Add(Val);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return EmployeeList;
        }

        private List<SelectListItem> GetNames()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                var NameList = new List<SelectListItem>();

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var getbranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                    // int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 where
                                                     data.IsActive == true && data.BranchId == getbranch  && data.UserStatus != 6
                                                 select new DSRCEmployees
                                                 {
                                                    // Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                                     Name = data.FirstName + " " + (data.LastName.Length > 0 ? data.LastName : ""),
                                                     UserId = data.UserID,
                                                     EmployeeId = data.EmpID
                                                 }).OrderBy(x => x.Name).ToList();
                    foreach (var item in Names)
                    {
                        if (item.Name != null)
                        {
                            NameList.Add(new SelectListItem { Text = item.Name, Value = item.UserId.ToString() });
                        }
                    }
                    NameList.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
                }
                return NameList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

      
    }
}
