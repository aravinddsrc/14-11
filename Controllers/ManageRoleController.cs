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
    public class ManageRoleController : Controller
    {
        public ActionResult ViewRoles()
        {
            var isReporting = (bool)Session["IsRerportingPerson"];

            ModelState.Clear();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Manage obj = new DSRCManagementSystem.Models.Manage();
            List<Manage> RoleValue = new List<Manage>();

            var RoleName = MasterEnum.NewuserRole.NewEmployeeRole;
            ViewBag.RoleName = RoleName;

            var RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == RoleName).RoleID;

            RoleValue = (from p in db.Master_Roles
                         where (p.IsActive == true)
                         select new Manage()
                         {
                             RoleID = p.RoleID,
                             RoleName = p.RoleName
                         }).ToList();
            return View(RoleValue);
        }
        [HttpPost]
        public ActionResult ViewRoles(FormCollection form)
        {
            List<Manage> RoleValue = new List<Manage>();
            if (ModelState.IsValid)
            {
                var isReporting = (bool)Session["IsRerportingPerson"];
                int userId = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                bool status = form["Inactive"].Contains("true");
                DSRCManagementSystem.Models.Manage obj = new DSRCManagementSystem.Models.Manage();
                if (Convert.ToInt32(Session["RoleID"]) == 59)
                {
                    RoleValue = (from p in db.Master_Roles
                                 select new Manage()
                                 {
                                     RoleID = p.RoleID,
                                     RoleName = p.RoleName
                                 }).ToList();
                }
                RoleValue = RoleValue.Distinct().ToList();
                return View(RoleValue);
            }
            else
            {
                DSRCManagementSystem.Models.Manage obj = new DSRCManagementSystem.Models.Manage();
                return View(RoleValue);
            }
        }

        [HttpGet]
        public JsonResult GetEmployeeName(int RoleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<int> userid = new List<int>();
            List<SelectListItem> empname = new List<SelectListItem>();
            if (RoleId != 0)
            {
                userid = db.UserRoles.Where(x => x.RoleID == RoleId).Select(x => x.UserID).ToList();
                foreach (var user in userid)
                {
                    var emp = db.Users.Where(o => o.UserID == user && o.IsActive == true).FirstOrDefault();
                    if (emp != null)
                    {
                        string empna = emp.FirstName + " " + (emp.LastName ?? "");
                        empname.Add(new SelectListItem { Text = empna, Value = Convert.ToString(emp.UserID) });
                    }
                }
            }
            return Json(empname, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AssignRole()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var userId = (int)Session["UserId"];
            return View();
        }

        [HttpGet]
        public ActionResult AssignNewRole(int RoleId)
        {
            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int BranchId = (int)objdb.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
            List<int> list = new List<int>();
            //TempData["RoleId"] = RoleId;
            string Role = Convert.ToString(RoleId);
            var temp = objdb.Master_Roles.Where(r => r.RoleID == RoleId).Select(f => f.RoleName).FirstOrDefault();
            ViewBag.RoleNamedisplay = temp;
            ViewBag.Role = RoleId;
            var RoleName = MasterEnum.NewuserRole.NewEmployeeRole;
            var RoleID = objdb.Master_Roles.FirstOrDefault(o => o.RoleName == RoleName).RoleID;
            ViewBag.DisplayRoleName = RoleName;
            var UnAssignedEmployees = from u in objdb.Users
                                      where (u.BranchId == BranchId) && !
                            (from ur in objdb.UserRoles
                             join r in objdb.Master_Roles on ur.RoleID equals r.RoleID
                             where r.RoleID != RoleID
                             select ur.UserID).Contains(u.UserID)
                                      select new AssignRole()
                                      {
                                          unuserid = u.UserID,
                                          Employees = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "")

                                      };
            var AssignedEmployees = (from rc in objdb.Users
                                     join p in objdb.UserRoles on rc.UserID equals p.UserID
                                     join r in objdb.Master_Roles on p.RoleID equals r.RoleID
                                   //  where (rc.FirstName != null && rc.LastName != null && rc.RoleID == RoleId && r.RoleID != RoleID && rc.BranchId == BranchId)
                                     where (p.RoleID == RoleId && r.RoleID != RoleID && rc.BranchId == BranchId)
                                     select new AssignRole()
                                     {
                                         userid = rc.UserID,
                                         unemployees = rc.FirstName + " " + (rc.LastName.Length > 0 ? rc.LastName : "")
                                     }).Distinct().ToList();



            ViewBag.UnAssignedEmployees = new MultiSelectList(UnAssignedEmployees, "unuserid", "Employees");
            ViewBag.AssignedEmployees = new MultiSelectList(AssignedEmployees, "userid", "unemployees");
            return View();
        }

        [HttpPost]
        public ActionResult AssignNewRole(AssignRole model)
        {
            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
            var userIDs1 = model.unuserid;
            var UserIDs = model.multiselectemployees.Split(',');
            var DataId = model.RoleID;
            var Data = Convert.ToInt32(DataId);
            List<int> UserID = model.SelectedEmpList;
            var RoleName = MasterEnum.NewuserRole.NewEmployeeRole;
            var RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == RoleName).RoleID;
            var employees1 = (from rc in db.Users
                              join p in db.UserRoles on rc.UserID equals p.UserID
                              join r in db.Master_Roles on p.RoleID equals r.RoleID
                              //where (rc.FirstName != null && rc.LastName != null && p.RoleID == Data && r.RoleID != RoleID && rc.BranchId == BranchId)
                              where (p.RoleID == Data && r.RoleID != RoleID && rc.BranchId == BranchId)
                              select new AssignRole()
                              {
                                  userid = rc.UserID,
                              }).Distinct().ToList();
            List<int> ex = UserID.Except(employees1.Select(e => e.userid)).ToList();
            foreach (int userID1 in ex)
            {
                var x = db.UserRoles.Where(o => o.UserID == userID1).ToList();
                foreach (var y in x)
                {
                    db.UserRoles.DeleteObject(y);
                    db.SaveChanges();
                }
            }

            foreach (int userID in ex)
            {
                var Assignobj = db.UserRoles.CreateObject();
                Assignobj.RoleID = Convert.ToByte(Data);
                Assignobj.UserID = Convert.ToInt32(userID);
                db.UserRoles.AddObject(Assignobj);
                db.SaveChanges();

                var UpdateRoleID = db.Users.Where(o => o.UserID == userID).Select(o => o).FirstOrDefault();               
                if (UpdateRoleID != null)
                UpdateRoleID.RoleID = Convert.ToByte(Data);
                db.SaveChanges();

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AssignNewRoles(int RoleId)
        {
            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<int> list = new List<int>();
            string Role = Convert.ToString(RoleId);
            var temp = objdb.Master_Roles.Where(r => r.RoleID == RoleId).Select(f => f.RoleName).FirstOrDefault();
            return View();
        }


        [HttpPost]
        public ActionResult AssignNewRoles(AssignRole model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var UserIDs = model.unemployees.Split(',');
            var DataId = model.RoleID;
            var Data = Convert.ToInt32(DataId);
            foreach (string userID in UserIDs)
            {
                int RoleID = Convert.ToByte(Data);
                int UserID = Convert.ToInt32(userID);
                var x = db.UserRoles.Where(o => o.UserID == UserID).ToList();
                foreach (var y in x)
                {
                    db.UserRoles.DeleteObject(y);
                    db.SaveChanges();
                    var user = db.UserRoles.CreateObject();
                    int z = Convert.ToInt32(y.UserID);
                    user.UserID = z;
                    user.RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == MasterEnum.NewuserRole.NewEmployeeRole).RoleID;
                    db.UserRoles.AddObject(user);
                    db.SaveChanges();
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Role = db.UserRoles.Where(o => o.RoleID == Id).Select(o => o).FirstOrDefault();
            var RoleName = db.Master_Roles.Where(o => o.RoleID == Id).Select(o => o.RoleName).FirstOrDefault();
            var RoleNames = MasterEnum.NewuserRole.NewEmployeeRole;
            var admin = MasterEnum.Roles.Admin;
            if (RoleName == RoleNames )
            {
                return Json("Warning1", JsonRequestBehavior.AllowGet);
            }
            if ( RoleName == Convert.ToString(admin))
            {
                return Json("Warning2", JsonRequestBehavior.AllowGet);
            }
            if (Role != null)
            {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = db.Master_Roles.Where(o => o.RoleID == Id).Select(o => o).FirstOrDefault();
                var dat = "UserRoles";
                string DeleteMasterTableRow = "delete from " + dat + " where RoleID = " + Id;
                string constr = ConfigurationManager.AppSettings["connstr"];
                SqlConnection objcon = new SqlConnection(constr);
                objcon.Open();
                SqlCommand cmd = new SqlCommand(DeleteMasterTableRow, objcon);
                cmd.ExecuteNonQuery();
                objcon.Close();
                data.IsActive = false;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult EditRole(int ID, string RoleName)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            ViewBag.EditRole = RoleName;
            ViewBag.ID = ID;
            var RoleNames = MasterEnum.NewuserRole.NewEmployeeRole;
            if (RoleName == RoleNames)
            {
                return Json("DeleteWarning", JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditRole(Manage model)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var Name = model.Name.Trim();
            int ID = model.ID;
            var temp = objdb.Master_Roles.Where(r => r.RoleID != ID && r.IsActive == true).Select(f => f.RoleName);

            foreach (var Edit in temp)

            {

                if (Edit == Name)
                {
                    ModelState.AddModelError("RoleName", "Role Name Already Exists");
                    @ViewBag.ID = ID;
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
            
            }

            //if (temp == Name)
            //{
            //    ModelState.AddModelError("RoleName", "Role Name Already Exists");
            //    @ViewBag.ID = ID;
            //    return Json("Warning", JsonRequestBehavior.AllowGet);
            //}
            //else
            {
                var x = objdb.Master_Roles.Where(o => o.RoleID == ID).FirstOrDefault();
                x.RoleName = Name;
                objdb.SaveChanges();
            }
            return Json("Success1", JsonRequestBehavior.AllowGet);
        }
    }
}