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
    public class ManageReportingController : Controller
    {
        //
        // GET: /ManageReporting/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ManageReportingPerson(string user)
        {
            var obj = new Reporting();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();



                var SuperAdmin = MasterEnum.Roles.SuperAdmin;
                int userId = Convert.ToInt32(Session["UserID"]);
                var GetRole = db.UserRoles.Where(o => o.UserID == userId).Select(o => o.RoleID).FirstOrDefault();
                var RoleName = db.Master_Roles.Where(o => o.RoleID == GetRole).Select(o => o.RoleName).FirstOrDefault();

                if (RoleName == Convert.ToString(SuperAdmin))
                {



                    int intusers = Convert.ToInt16(user);

                    List<SelectListItem> TotalTerms = new List<SelectListItem>();
                    TotalTerms.AddRange(new[]
                {
                    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                });
                    if (intusers == 0)
                    {
                        var AuthUsers = (from ep in db.ReportingUsers.Where(x => x.UserId != null)
                                         join u in db.Users.Where(u => u.IsActive == true) on ep.UserId equals u.UserID
                                         select new
                                         {
                                             userid = u.UserID,
                                             username = (u.FirstName)??"" + (u.LastName)??""
                                         }).ToList();
                        ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "userid", "username");

                        var FilteredUsers = db.Users.Where(u => u.IsActive == true).Select(x => x.UserID).ToList().
                            Except(
                                db.ReportingUsers.Where(x => x.UserId != null)
                                    .Select(x => x.UserId.Value)
                                    .ToList()).ToList();
                        List<object> UnAuthUsers = new List<object>();
                        foreach (int users in FilteredUsers)
                        {
                            UnAuthUsers.AddRange(
                                db.Users.Where(u => u.UserID == users )
                                    .Select(u => new { userid = u.UserID, username = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "") })
                                    .ToList());
                        }
                        ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");

                        obj = new Reporting { Reportingtype = "0", Items = TotalTerms };
                        return View(obj);
                    }
                    else
                    {
                        var AuthRoles = (from ep in db.ReportingUsers.Where(x => x.RoleID != null)
                                         join p in db.Master_Roles.Where(x => x.IsActive == true ) on ep.RoleID equals p.RoleID
                                         select new
                                         {
                                             roleid = p.RoleID,
                                             rolename = p.RoleName
                                         }).ToList();
                        ViewBag.AuthorizedUsers = new SelectList(AuthRoles, "roleid", "rolename");

                        var FilteredRoles = db.Master_Roles.Where(u => u.IsActive == true && u.RoleName != RoleName).Select(x => (int)x.RoleID).ToList().
                            Except(
                                db.ReportingUsers.Where(x => x.RoleID != null)
                                    .Select(x => x.RoleID.Value)
                                    .ToList()).ToList();
                        List<object> UnAuthRoles = new List<object>();
                        foreach (int role in FilteredRoles)
                        {
                            UnAuthRoles.AddRange(
                                db.Master_Roles.Where(u => u.RoleID == role)
                                    .Select(u => new { roleid = u.RoleID, rolename = u.RoleName })
                                    .ToList());
                        }
                        ViewBag.UnAuthorizedUsers = new SelectList(UnAuthRoles, "roleid", "rolename");

                        obj = new Reporting { Reportingtype = "1", Items = TotalTerms };
                        return View(obj);
                    }






                }
                else
                {

                    var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();

                    int intusers = Convert.ToInt16(user);

                    List<SelectListItem> TotalTerms = new List<SelectListItem>();
                    TotalTerms.AddRange(new[]
                {
                    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                });
                    if (intusers == 0)
                    {
                        var AuthUsers = (from ep in db.ReportingUsers.Where(x => x.UserId != null)
                                         join u in db.Users.Where(u => u.IsActive == true && u.BranchId == getBracnch)on ep.UserId equals u.UserID
                                         select new
                                         {
                                             userid = u.UserID,
                                             username = u.FirstName + "" + (u.LastName.Length > 0 ? u.LastName : ""),
                                         }).ToList();
                        ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "userid", "username");

                        var FilteredUsers = db.Users.Where(u => u.IsActive == true && u.BranchId == getBracnch).Select(x => x.UserID).ToList().
                            Except(
                                db.ReportingUsers.Where(x => x.UserId != null)
                                    .Select(x => x.UserId.Value)
                                    .ToList()).ToList();
                        List<object> UnAuthUsers = new List<object>();
                        foreach (int users in FilteredUsers)
                        {
                            UnAuthUsers.AddRange(
                                db.Users.Where(u => u.UserID == users)
                                    .Select(u => new { userid = u.UserID, username = u.FirstName + "" + (u.LastName.Length > 0 ? u.LastName : ""), })
                                    .ToList());
                        }
                        ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");

                        obj = new Reporting { Reportingtype = "0", Items = TotalTerms };
                        return View(obj);
                    }
                    else
                    {
                        var AuthRoles = (from ep in db.ReportingUsers.Where(x => x.RoleID != null )
                                         join p in db.Master_Roles.Where(x => x.IsActive == true ) on ep.RoleID equals p.RoleID
                                         select new
                                         {
                                             roleid = p.RoleID,
                                             rolename = p.RoleName
                                         }).ToList();
                        ViewBag.AuthorizedUsers = new SelectList(AuthRoles, "roleid", "rolename");

                        var FilteredRoles = db.Master_Roles.Where(u => u.IsActive == true && u.RoleName != RoleName).Select(x => (int)x.RoleID).ToList().
                            Except(
                                db.ReportingUsers.Where(x => x.RoleID != null)
                                    .Select(x => x.RoleID.Value)
                                    .ToList()).ToList();
                        List<object> UnAuthRoles = new List<object>();
                        foreach (int role in FilteredRoles)
                        {
                            UnAuthRoles.AddRange(
                                db.Master_Roles.Where(u => u.RoleID == role)
                                    .Select(u => new { roleid = u.RoleID, rolename = u.RoleName })
                                    .ToList());
                        }
                        ViewBag.UnAuthorizedUsers = new SelectList(UnAuthRoles, "roleid", "rolename");

                        obj = new Reporting { Reportingtype = "1", Items = TotalTerms };
                        return View(obj);
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        [HttpPost]
        public ActionResult ManageReportingPerson(List<int> From, List<int> To, string user)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


                var SuperAdmin = MasterEnum.Roles.SuperAdmin;
                int userId = Convert.ToInt32(Session["UserID"]);
                var GetRole = db.UserRoles.Where(o => o.UserID == userId).Select(o => o.RoleID).FirstOrDefault();
                var RoleName = db.Master_Roles.Where(o => o.RoleID == GetRole).Select(o => o.RoleName).FirstOrDefault();

                if (RoleName == Convert.ToString(SuperAdmin))
                {
                    if (user == "0")
                    {


                        //var deleteuser = db.ReportingUsers.Where(x => x.UserId != null).Select(o => o).ToList();
                        var delete = db.ReportingUsers.Where(x => x.UserId != null).Select(o => o.UserId).ToList();




                        foreach (var del in delete)
                        {
                            var deleteuser = db.Users.Where(x => x.UserID == del).Select(o => o.UserID).ToList();

                            if (deleteuser.Count != 0)
                            {
                                var deleteindb = db.ReportingUsers.Where(x => x.UserId == del).Select(o => o).ToList();
                                foreach (var deluser in deleteindb)
                                    db.ReportingUsers.DeleteObject(deluser);
                                db.SaveChanges();
                            }
                        }
                        for (int j = 0; j < To.Count(); j++)
                        {
                            DSRCManagementSystem.ReportingUser objaccess = new DSRCManagementSystem.ReportingUser();
                            objaccess.UserId = To[j];
                            db.AddToReportingUsers(objaccess);
                            db.SaveChanges();
                        }
                    }
                    if (user == "1")
                    {
                        var deleteuser = db.ReportingUsers.Where(x => x.RoleID != null).Select(o => o).ToList();
                        foreach (var deluser in deleteuser)
                            db.ReportingUsers.DeleteObject(deluser);
                        db.SaveChanges();
                        for (int j = 0; j < To.Count(); j++)
                        {
                            DSRCManagementSystem.ReportingUser objaccess = new DSRCManagementSystem.ReportingUser();
                            objaccess.RoleID = To[j];
                            db.AddToReportingUsers(objaccess);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {

                    if (user == "0")
                    {


                        //var deleteuser = db.ReportingUsers.Where(x => x.UserId != null).Select(o => o).ToList();
                        var delete = db.ReportingUsers.Where(x => x.UserId != null).Select(o => o.UserId).ToList();


                        var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();
                        var tab = db.Master_Tab.Where(x => x.IsActive == true).Select(o => o.TabID).ToList();
                        var grid = db.Master_Tab_Grids.Where(x => x.IsActive == true).Select(o => o.GridID).ToList();
                        foreach (var del in delete)
                        {
                            var deleteuser = db.Users.Where(x => x.UserID == del && x.BranchId == getBracnch).Select(o => o.UserID).ToList();

                            if (deleteuser.Count != 0)
                            {
                                var deleteindb = db.ReportingUsers.Where(x => x.UserId == del).Select(o => o).ToList();
                                foreach (var deluser in deleteindb)
                                    db.ReportingUsers.DeleteObject(deluser);
                                db.SaveChanges();
                            }
                        }
                        for (int j = 0; j < To.Count(); j++)
                        {
                            DSRCManagementSystem.ReportingUser objaccess = new DSRCManagementSystem.ReportingUser();
                            objaccess.UserId = To[j];
                            db.AddToReportingUsers(objaccess);
                            db.SaveChanges();
                            var id = To[j];
                            var tabs = db.ManageTabs.Where(x => x.UserID == id).Select(o => o).ToList();
                            if (tabs.Count() == 0)
                            {
                                for (int i = 0; i < tab.Count(); i++)
                                {
                                    DSRCManagementSystem.ManageTab objaccess1 = new DSRCManagementSystem.ManageTab();
                                    objaccess1.TabID = tab[i];
                                    objaccess1.UserID = To[j];
                                    objaccess1.IsActive = true;
                                    objaccess1.UserSelected = true;
                                    db.AddToManageTabs(objaccess1);
                                    db.SaveChanges();
                                }

                            }
                            var grids = db.ManageTabGrids.Where(x => x.UserID == id).Select(o => o).ToList();
                            if (grids.Count == 0)
                            {
                                for (int i = 0; i < grid.Count(); i++)
                                {
                                    DSRCManagementSystem.ManageTabGrid objaccess2 = new DSRCManagementSystem.ManageTabGrid();
                                    objaccess2.GridID = grid[i];
                                    objaccess2.UserID = To[j];
                                    objaccess2.RoleID = null;
                                    objaccess2.IsActive = true;
                                    objaccess2.UserSelected = true;
                                    db.AddToManageTabGrids(objaccess2);
                                    db.SaveChanges();
                                }

                            }
                        }
                    }
                    if (user == "1")
                    {
                        var deleteuser = db.ReportingUsers.Where(x => x.RoleID != null).Select(o => o).ToList();
                        foreach (var deluser in deleteuser)
                            db.ReportingUsers.DeleteObject(deluser);
                        db.SaveChanges();
                        for (int j = 0; j < To.Count(); j++)
                        {
                            DSRCManagementSystem.ReportingUser objaccess = new DSRCManagementSystem.ReportingUser();
                            objaccess.RoleID = To[j];
                            db.AddToReportingUsers(objaccess);
                            db.SaveChanges();
                        }
                    }
                }
                return Json("Authorize", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }

    }
}
