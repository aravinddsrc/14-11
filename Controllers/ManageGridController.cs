using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ManageGridController : Controller
    {
        //
        // GET: /ManageTab/
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult ControlGrid(string type = "users")
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Session["TabAssigned"] = null;
            var objModel = new ManageGrid();
            try
            {
                List<SelectListItem> TotalTerms = new List<SelectListItem>();
                TotalTerms.AddRange(new[]
                {
                    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                });
                if (type == "users")
                {
                    objModel = new ManageGrid { type = "0", typelist = TotalTerms };
                }
                else if (type == "roles")
                {
                    objModel = new ManageGrid { type = "1", typelist = TotalTerms };
                }
                objModel.Users = GetUsers();
                objModel.Roles = GetRoles();
                ViewBag.type = TotalTerms;
                //var Tabs = db.Master_Tab.Select(x =>
                //    new
                //    {
                //        tabid = x.TabID,
                //        tabname = x.TabName
                //    }).ToList();
                //ViewBag.TabList = new SelectList(Tabs, "tabid", "tabname");
                var Tabs = db.Master_Tab_Grids.Select(x =>
                    new
                    {
                        Gridid = x.GridID,
                        GridName = x.GridName
                    }).ToList();
                ViewBag.TabList = new SelectList(Tabs, "Gridid", "GridName");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            Dictionary<int, string> dict = new Dictionary<int, string>();
            var getGrid = db.Master_Tab_Grids.ToList();
            foreach (var item in getGrid)
            {
                dict.Add(item.GridID, item.GridName);
            }

            return View(objModel);
        }

        [HttpPost]
        public ActionResult ControlGrid(ManageGrid objModel)
        {
            try
            {
                Session["TabAssigned"] = null;
                if (ModelState.IsValid)
                {
                    if (objModel.Grids != null)
                    {
                        if (objModel.UserId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.Grids);
                            var oldTabs =
                                db.ManageTabGrids.Where(x => x.UserID == objModel.UserId)
                                    .Select(x => x.GridID)
                                    .ToList();

                            var toInsert = newTabs.Except(oldTabs).ToList();
                            var toDelete = oldTabs.Except(newTabs).ToList();

                            if (oldTabs.Count == 0)
                            {
                                foreach (var item in newTabs)
                                {
                                    var insertNew = new ManageTabGrid()
                                    {
                                        GridID = Convert.ToInt32(item),
                                        UserID = objModel.UserId,
                                        RoleID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabGrids.AddObject(insertNew);
                                }
                            }
                            else if (toInsert.Count > 0)
                            {
                                foreach (var item in toInsert)
                                {
                                    var insertChanged = new ManageTabGrid()
                                    {
                                        GridID = Convert.ToInt32(item),
                                        UserID = objModel.UserId,
                                        RoleID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabGrids.AddObject(insertChanged);
                                }
                            }
                            if (toDelete.Count > 0)
                            {
                                foreach (var item in toDelete)
                                {
                                    var data =
                                        db.ManageTabGrids.Where(
                                            x => x.UserID == objModel.UserId && x.GridID == item)
                                            .FirstOrDefault();
                                    db.ManageTabGrids.DeleteObject(data);
                                }
                            }
                        }
                        if (objModel.RoleId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.Grids);
                            var oldTabs =
                                db.ManageTabGrids.Where(x => x.RoleID == objModel.RoleId)
                                    .Select(x => x.GridID)
                                    .ToList();

                            var toInsert = newTabs.Except(oldTabs).ToList();
                            var toDelete = oldTabs.Except(newTabs).ToList();

                            if (oldTabs.Count == 0)
                            {
                                foreach (var item in newTabs)
                                {
                                    var insertNew = new ManageTabGrid()
                                    {
                                        GridID = Convert.ToInt32(item),
                                        RoleID = Convert.ToByte(objModel.RoleId),
                                        UserID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabGrids.AddObject(insertNew);
                                }
                            }
                            else if (toInsert.Count > 0)
                            {
                                foreach (var item in toInsert)
                                {
                                    var insertChanged = new ManageTabGrid()
                                    {
                                        GridID = Convert.ToInt32(item),
                                        RoleID = Convert.ToByte(objModel.RoleId),
                                        UserID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabGrids.AddObject(insertChanged);
                                }
                            }
                            if (toDelete.Count > 0)
                            {
                                foreach (var item in toDelete)
                                {
                                    var data =
                                        db.ManageTabGrids.Where(
                                            x => x.RoleID == objModel.RoleId && x.GridID == item)
                                            .FirstOrDefault();
                                    db.ManageTabGrids.DeleteObject(data);
                                }
                            }
                        }
                    }
                    else
                    {
                        
                        if (objModel.UserId != 0)
                        {
                            var oldTabs =
                                db.ManageTabGrids.Where(x => x.UserID == objModel.UserId)
                                    .Select(x => x.GridID)
                                    .ToList();
                            if (oldTabs.Count > 0)
                            {
                                foreach (var item in oldTabs)
                                {
                                    var data =
                                        db.ManageTabGrids.Where(
                                            x => x.UserID == objModel.UserId && x.GridID == item)
                                            .FirstOrDefault();
                                    db.ManageTabGrids.DeleteObject(data);
                                }
                            }
                        }
                        if (objModel.RoleId != 0)
                        {
                            var oldTabs =
                              db.ManageTabGrids.Where(x => x.RoleID == objModel.RoleId)
                                  .Select(x => x.GridID)
                                  .ToList();
                            if (oldTabs.Count > 0)
                            {
                                foreach (var item in oldTabs)
                                {
                                    var data =
                                        db.ManageTabGrids.Where(
                                            x => x.RoleID == objModel.RoleId && x.GridID == item)
                                            .FirstOrDefault();
                                    db.ManageTabGrids.DeleteObject(data);
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                    Session["TabAssigned"] = 1;
                }
                List<SelectListItem> TotalTerms = new List<SelectListItem>();
                TotalTerms.AddRange(new[]
                {
                    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                });
                if (objModel.RoleId != 0)
                {
                    objModel = new ManageGrid { type = "1", typelist = TotalTerms };

                }
                else
                {
                    objModel = new ManageGrid { type = "0", typelist = TotalTerms };
                }
                objModel.Roles = GetRoles();
                objModel.Users = GetUsers();
                var Tabs = db.Master_Tab_Grids.Select(x =>
                        new
                        {
                            tabid = x.GridID,
                            tabname = x.GridName
                        }).ToList();
                ViewBag.TabList = new SelectList(Tabs, "tabid", "tabname");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objModel);
        }

        public ActionResult GetAssignedTabs(int? Uid, int? Rid)
        {
            var selectedValues = new List<int?>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                if (Uid != null)
                {
                    selectedValues =
                       db.ManageTabGrids.Where(x => x.UserID == Uid).Select(x => x.GridID).ToList();
                }
                if (Rid != null)
                {
                    selectedValues =
                       db.ManageTabGrids.Where(x => x.RoleID == Rid).Select(x => x.GridID).ToList();
                }
            }
            return Json(selectedValues, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetUsers()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                var NameList = new List<SelectListItem>();

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    // int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 join report in db.ReportingUsers on data.UserID equals report.UserId
                                                 where
                                                     data.IsActive == true && data.UserStatus != 6
                                                 select new DSRCEmployees
                                                 {
                                                     Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                                     UserId = data.UserID,
                                                     EmployeeId = data.EmpID
                                                 }).OrderBy(x => x.Name).ToList();
                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.Name, Value = item.UserId.ToString() });
                    }

                    NameList.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
                }
                return NameList;
            }
            catch (Exception Ex)
            {
                string actionName = "GetNames";
                string controllerName = "ManageGrid";
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        public static List<SelectListItem> GetRoles()
        {
            try
            {
                var Roles = new List<SelectListItem>();

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<Roles> RoleList = (from mrole in db.Master_Roles
                                            join role in db.ReportingUsers on mrole.RoleID equals role.RoleID
                                            where mrole.IsActive == true
                                            select new Roles
                                            {
                                                RoleId = mrole.RoleID,
                                                RoleName = mrole.RoleName
                                            }).OrderBy(x => x.RoleName).ToList();
                    foreach (var item in RoleList)
                    {
                        Roles.Add(new SelectListItem { Text = item.RoleName, Value = item.RoleId.ToString() });
                    }
                    Roles.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
                }
                return Roles;
            }
            catch (Exception Ex)
            {
                string actionName = "GetRoles";
                string controllerName = "ManageGrid";
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }
    }
}

