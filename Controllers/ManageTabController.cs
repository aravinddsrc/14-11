using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ManageTabController : Controller
    {
        //
        // GET: /ManageTab/
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult ControlTab(string type="users")
        {
            Session["TabAssigned"] = null;
            var objModel = new ManageTabModel();
            try
            {
                //List<SelectListItem> TotalTerms = new List<SelectListItem>();
                //TotalTerms.AddRange(new[]
                //{
                //    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                //});
                //if (type == "users")
                //{
                //    objModel = new ManageTabModel { type = "0", typelist = TotalTerms };
                //}
                //else if (type == "roles")
                //{
                //    objModel = new ManageTabModel { type = "1", typelist = TotalTerms };
                //}
                    objModel.Users = GetUsers();
                   // objModel.Roles = GetRoles();
                  //  ViewBag.type = TotalTerms;
                    var Tabs = db.Master_Tab.Where(x=>x.IsActive == true).Select(x =>
                        new
                        {
                            tabid = x.TabID,
                            tabname = x.TabName
                        }).ToList();
                    ViewBag.TabList = new SelectList(Tabs, "tabid", "tabname");
                    var TabGrids = db.Master_Tab_Grids.Where(x=>x.IsActive == true).Select(x =>
                           new
                           {
                               tabid = x.GridID,
                               tabname = x.GridName
                           }).ToList();
                    ViewBag.TabGridList = new SelectList(TabGrids, "tabid", "tabname");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            Dictionary<int, string> dict = new Dictionary<int, string>();
            var getTab = db.Master_Tab.Where(x=>x.IsActive == true).ToList();
            foreach (var item in getTab)
            {
                dict.Add(item.TabID,item.TabName);
            }
            
            return View(objModel);
        }

        [HttpPost]
        public ActionResult ControlTab(ManageTabModel objModel)
        {
            try
            {
                Session["TabAssigned"] = null;
                if (ModelState.IsValid)
                {
                    if (objModel.Tabs != null)
                    {
                        if (objModel.UserId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.Tabs);
                            var oldTabs =
                                db.ManageTabs.Where(x => x.UserID == objModel.UserId && x.IsActive == true)
                                    .Select(x => x.TabID)
                                    .ToList();

                            var toInsert = newTabs.Except(oldTabs).ToList();
                            var toDelete = oldTabs.Except(newTabs).ToList();

                            if (oldTabs.Count == 0)
                            {
                                foreach (var item in newTabs)
                                {
                                    var insertNew = new ManageTab()
                                    {
                                        TabID = Convert.ToInt32(item),
                                        UserID = objModel.UserId,
                                        RoleID=null,
                                        UserSelected = true,
                                        IsActive=true
                                    };
                                    db.ManageTabs.AddObject(insertNew);
                                }
                            }
                            else if (toInsert.Count > 0)
                            {
                                foreach (var item in toInsert)
                                {
                                    var insertChanged = new ManageTab()
                                    {
                                        TabID = Convert.ToInt32(item),
                                        UserID = objModel.UserId,
                                        RoleID = null,
                                        UserSelected = true,
                                        IsActive = true
                                    };
                                    db.ManageTabs.AddObject(insertChanged);
                                }
                            }
                            if (toDelete.Count > 0)
                            { 
                                foreach (var item in toDelete)
                                {
                                    var data =
                                        db.ManageTabs.Where(
                                            x => x.UserID == objModel.UserId && x.TabID == item && x.IsActive == true)
                                            .FirstOrDefault();
                                    db.ManageTabs.DeleteObject(data);
                                }
                            }
                        }
                        /*if (objModel.RoleId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.Tabs);
                            var oldTabs =
                                db.ManageTabs.Where(x => x.RoleID == objModel.RoleId)
                                    .Select(x => x.TabID)
                                    .ToList();

                            var toInsert = newTabs.Except(oldTabs).ToList();
                            var toDelete = oldTabs.Except(newTabs).ToList();

                            if (oldTabs.Count == 0)
                            {
                                foreach (var item in newTabs)
                                {
                                    var insertNew = new ManageTab()
                                    {
                                        TabID = Convert.ToInt32(item),
                                        RoleID = Convert.ToByte(objModel.RoleId),
                                        UserID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabs.AddObject(insertNew);
                                }
                            }
                            else if (toInsert.Count > 0)
                            {
                                foreach (var item in toInsert)
                                {
                                    var insertChanged = new ManageTab()
                                    {
                                        TabID = Convert.ToInt32(item),
                                        RoleID = Convert.ToByte(objModel.RoleId),
                                        UserID = null,
                                        IsActive = true
                                    };
                                    db.ManageTabs.AddObject(insertChanged);
                                }
                            }
                            if (toDelete.Count > 0)
                            {
                                foreach (var item in toDelete)
                                {
                                    var data =
                                        db.ManageTabs.Where(
                                            x => x.RoleID == objModel.RoleId && x.TabID == item)
                                            .FirstOrDefault();
                                    db.ManageTabs.DeleteObject(data);
                                }
                            }
                        }*/
                    }
                    else
                    {

                        if (objModel.UserId != 0)
                        {
                            var oldTabs =
                                 db.ManageTabs.Where(x => x.UserID == objModel.UserId && x.IsActive == true)
                                     .Select(x => x.TabID)
                                     .ToList();
                            if (oldTabs.Count > 0)
                            {
                                foreach (var item in oldTabs)
                                {
                                    var data =
                                        db.ManageTabs.Where(
                                            x => x.UserID == objModel.UserId && x.TabID == item && x.IsActive == true)
                                            .FirstOrDefault();
                                    db.ManageTabs.DeleteObject(data);
                                }
                            }
                        }
                        /*
                        if (objModel.RoleId != 0)
                        {
                            var oldTabs =
                               db.ManageTabs.Where(x => x.RoleID == objModel.RoleId)
                                   .Select(x => x.TabID)
                                   .ToList();

                            if (oldTabs.Count > 0)
                            {
                                foreach (var item in oldTabs)
                                {
                                    var data =
                                         db.ManageTabs.Where(
                                             x => x.RoleID == objModel.RoleId && x.TabID == item)
                                             .FirstOrDefault();
                                    db.ManageTabs.DeleteObject(data);
                                }
                            }
                        
                        }
                           */
                    }
                    db.SaveChanges();
                    if (objModel.TabGrids != null)
                    {
                        if (objModel.UserId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.TabGrids);
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
                                        UserSelected =true,
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
                                        UserSelected = true,
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
                        /*
                        if (objModel.RoleId != 0)
                        {
                            var newTabs = new List<Int32?>(objModel.TabGrids);
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
                        }*/
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
                        /*
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
                         */
                    }
                    db.SaveChanges();
                    Session["TabAssigned"] = 1;
                }
                List<SelectListItem> TotalTerms = new List<SelectListItem>();
                TotalTerms.AddRange(new[]
                {
                    new SelectListItem() {Text = "Users", Value = "0"},new SelectListItem(){Text = "Roles",Value = "1"}, 
                });
                /*
                if (objModel.RoleId != 0)
                {
                    objModel = new ManageTabModel { type = "1", typelist = TotalTerms };
                   
                }
                else
                {
                    objModel = new ManageTabModel { type = "0", typelist = TotalTerms };
                }
                 
                objModel.Roles = GetRoles();
                  */
                objModel.Users = GetUsers();
                var Tabs = db.Master_Tab.Where(x=>x.IsActive == true).Select(x =>
                        new
                        {
                            tabid = x.TabID,
                            tabname = x.TabName
                        }).ToList();
                ViewBag.TabList = new SelectList(Tabs, "tabid", "tabname");
                var TabGrids = db.Master_Tab_Grids.Where(x=>x.IsActive == true).Select(x =>
                       new
                       {
                           tabid = x.GridID,
                           tabname = x.GridName
                       }).ToList();
                ViewBag.TabGridList = new SelectList(TabGrids, "tabid", "tabname");
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
                        db.ManageTabs.Where(x => x.UserID == Uid && x.IsActive == true).Select(x => x.TabID).ToList();
                     
                }
                if (Rid != null)
                {
                     selectedValues =
                        db.ManageTabs.Where(x => x.RoleID == Rid && x.IsActive == true).Select(x => x.TabID).ToList();
                  
                }
            }
                    return Json(selectedValues, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAssignedTabsGrids(int? Uid, int? Rid)
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
                     int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 join report in db.ReportingUsers on data.UserID equals report.UserId
                                                 where
                                                     data.IsActive == true && data.UserStatus != 6 && data.BranchId == BranchId
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
                string controllerName = "ManageTab";
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

     [HttpGet]
     public ActionResult ManageTabWidget()
        {
            Session["TabAssigned"] = null;
            var objModel = new ManageTabModel();
         //DSRCManagementSystem.Models.ManageGrid objmodel = new DSRCManagementSystem.Models.ManageGrid();
            DSRCManagementSystem.Models.ManageTabWidget UserCounts = new DSRCManagementSystem.Models.ManageTabWidget();
           // DSRCManagementSystem.Models.ManageTabWidget TabUserCount = new DSRCManagementSystem.Models.ManageTabWidget();
            try
            {
                //objModel.Users = GetUsers();
                //var Tabs = db.Master_Tab.Select(x =>
                //    new
                //    {
                //        tabid = x.TabID,
                //        tabname = x.TabName
                //    }).ToList();
                //ViewBag.TabList = new SelectList(Tabs, "tabid", "tabname");
                //var TabGrids = db.Master_Tab_Grids.Select(x =>
                //       new
                //       {
                //           tabid = x.GridID,
                //           tabname = x.GridName
                //       }).ToList();
                //ViewBag.TabGridList = new SelectList(TabGrids, "tabid", "tabname");



                UserCounts.GridUsers = (from t in db.Master_Tab_Grids
                                        where t.IsActive == true
                                        select new values
                                        {
                                            GridName = t.GridName,
                                            GridId = t.GridID
                                        }).ToList();

             
                foreach (var item in UserCounts.GridUsers)
                {
                    var GridUsers = (from t in db.ManageTabGrids
                                        join m in db.Master_Tab_Grids on t.GridID equals m.GridID
                                        where m.IsActive == true && t.GridID == item.GridId && t.UserID != null
                                        select new values
                                       {
                                          UserId = t.UserID
                                       }).ToList();

                    int k = GridUsers.Count();
                    item.NoofUsers = k;
                }

                UserCounts.TabUsers = (from t in db.Master_Tab
                                       where t.IsActive == true
                                       select new values
                                       {
                                           TabName = t.TabName,
                                           TabId = t.TabID,
                                       }).ToList();

            
                foreach (var item in UserCounts.TabUsers)
                {
                    var TabUsers = (from t in db.ManageTabs
                                    join m in db.Master_Tab on t.TabID equals m.TabID
                                    where m.IsActive == true && t.TabID == item.TabId && t.UserID != null
                                    select new values
                                    {
                                        UserId = t.UserID
                                    }).ToList();

                    int k1 = TabUsers.Count();
                    item.NoofUsers = k1;
                }
             

               }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            Dictionary<int, string> dict = new Dictionary<int, string>();
            var getTab = db.Master_Tab.Where(x=>x.IsActive == true).ToList();
            foreach (var item in getTab)
            {
                dict.Add(item.TabID, item.TabName);
            }

            return View(UserCounts);
            
        }

     [HttpGet]
     public ActionResult TabUsers(string Id, string UserCount)
     {
         int TId = Convert.ToInt32(Id);
         int UCount = Convert.ToInt32(UserCount);
         ViewBag.TanId = TId;
         ViewBag.UserCount = UCount;
         Session["TabID"] = TId;
         DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
         var objmodel = new List<ManageTabs>();
         DSRCManagementSystem.Models.ManageTabs Users = new DSRCManagementSystem.Models.ManageTabs();
         DSRCManagementSystem.Models.ManageTabs obj = new DSRCManagementSystem.Models.ManageTabs();
         try
         {
             Users.TabUsers = (from t in db.Master_Tab
                               join m in db.ManageTabs on t.TabID equals m.TabID
                               join u in db.Users on m.UserID equals u.UserID
                               where t.IsActive == true && t.TabID == TId && m.IsActive == true && m.UserID != null
                               select new DSRCManagementSystem.Models.values
                             {
                                 TabName = t.TabName,
                                 TabId = t.TabID,
                                 UserName =(u.LastName == null) ? u.FirstName : u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,
                             }).ToList();


               objmodel = (from t in db.Master_Tab
                            join m in db.ManageTabs on t.TabID equals m.TabID
                            join u in db.Users on m.UserID equals u.UserID
                           where t.IsActive == true && t.TabID == TId && m.IsActive == true && m.UserID != null
                            select new ManageTabs
                            {
                                 UserName = u.UserName,
                                 UserId = u.UserID
                            }).ToList();
             int k = objmodel.Count();
             obj.Nofcount = k;
             foreach (var item in Users.TabUsers)
                {
                    var Assignedlist = db.ManageTabs.Where(x => x.UserID == item.UserId && x.IsActive == true).Select(x => x.TabID).ToList();
                    int Count = Assignedlist.Count;
                    if (Count != 0)
                    {
                        item.IsChecked = true;
                        item.Nofcount = k;
                    }
                    else
                    {
                        item.IsChecked = false;
                        item.Nofcount = k;
                    }

                }
            }
         catch (Exception Ex)
         {
             string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
             string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
             ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
         }

         return View(Users);

     }

     [HttpPost]
     public ActionResult TabUsers(string UserId,string TabId,string UserCount)
     {
         DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
         try
         {
             if (UserId == null || UserId == "")
             {
                 int NulTabId = Convert.ToInt32(Session["TabID"]);
                 var TotalUsers = db.ManageTabs.Where(q => q.TabID == NulTabId && q.IsActive == true).Select(q =>q.UserID).ToList();
                 foreach (var item in TotalUsers)
                 {
                     if (item != null)
                     {
                         var data = db.ManageTabs.Where(x => x.UserID == item && x.TabID == NulTabId).FirstOrDefault();
                         db.ManageTabs.DeleteObject(data);

                         //var ManTabGrid = db.ManageTabs.Where(q => q.UserID == item && q.TabID == NulTabId).Select(r => r).FirstOrDefault();
                         //ManTabGrid.UserSelected = false;
                     }
                 }
                 db.SaveChanges();
             }
             else
             {
                 int TId = Convert.ToInt32(Session["TabID"]);
                 string ManageUsers = UserId.Trim(new Char[] { ' ', ',' });

                 int userId = int.Parse(Session["UserID"].ToString());
                 Session["TabAssigned"] = null;

                 List<int?> Users = new List<int?>();
                 if (UserId != "")
                 {

                     string[] value = ManageUsers.Split(',');
                     for (int i = 0; i < value.Count(); i++)
                     {
                         var item = Convert.ToInt32(value[i]);
                         var ManTabs = db.ManageTabs.Where(q => q.UserID == item && q.TabID == TId && q.IsActive == true).Select(r => r).FirstOrDefault();
                         ManTabs.UserSelected = true;
                         Users.Add(Convert.ToInt32(value[i]));

                     }
                 }

                 var TotalUsers = db.ManageTabs.Where(q => q.TabID == TId && q.IsActive == true).Select(q => q.UserID).ToList();
                 var UnSelected = TotalUsers.Except(Users).ToList();

                 foreach (var item in UnSelected)
                 {
                     if (item != null)
                     {
                         var data = db.ManageTabs.Where(x => x.UserID == item && x.TabID == TId && x.IsActive == true).FirstOrDefault();
                         db.ManageTabs.DeleteObject(data);

                         //var ManTabGrid = db.ManageTabs.Where(q => q.UserID == item && q.TabID == TId).Select(r => r).FirstOrDefault();
                         //ManTabGrid.UserSelected = false;
                     }
                 }

                 db.SaveChanges();


             }
         }

         catch (Exception Ex)
         {
             string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
             string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
             ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
         }

         return Json("Success", JsonRequestBehavior.AllowGet);
         
     }

     [HttpGet]
     public ActionResult GridUsers(string GridName, string UserCount)
     {
         int GId = Convert.ToInt32(GridName);
         int UCount = Convert.ToInt32(UserCount);
         Session["GridID"] = GId;
         ViewBag.GridId = GId;
         ViewBag.UserCount = UCount;
         DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
         var objmodel = new List<ManageGrid>();
         DSRCManagementSystem.Models.ManageGrid Users = new DSRCManagementSystem.Models.ManageGrid();
         DSRCManagementSystem.Models.ManageGrid obj = new DSRCManagementSystem.Models.ManageGrid();
         try
         {
             Users.GridUsers = (from t in db.Master_Tab_Grids
                               join m in db.ManageTabGrids on t.GridID equals m.GridID
                               join u in db.Users on m.UserID equals u.UserID
                                where t.IsActive == true && t.GridID == GId && m.IsActive == true && m.UserID != null
                               select new DSRCManagementSystem.Models.values
                               {
                                   GridName = t.GridName,
                                   GridId = t.GridID,
                                   UserName = (u.LastName == null) ? u.FirstName : u.FirstName + " " + u.LastName,
                                   UserId = u.UserID
                               }).ToList();


             objmodel =  (from t in db.Master_Tab_Grids
                               join m in db.ManageTabGrids on t.GridID equals m.GridID
                               join u in db.Users on m.UserID equals u.UserID
                          where t.IsActive == true && t.GridID == GId && m.IsActive == true && m.UserID != null
                           select new ManageGrid
                           {
                             UserName = u.UserName,
                             UserId = u.UserID
                         }).ToList();
             int k = objmodel.Count();
             obj.Nofcount = k;
             foreach (var item in Users.GridUsers)
             {
                 var Assignedlist = db.ManageTabGrids.Where(x => x.UserID == item.UserId && x.IsActive == true).Select(x => x.GridID).ToList();
                 int Count = Assignedlist.Count;
                 if (Count != 0)
                 {
                     item.IsChecked = true;
                     item.Nofcount = k;
                 }
                 else
                 {
                     item.IsChecked = false;
                     item.Nofcount = k;
                 }

             }
         }
         catch (Exception Ex)
         {
             string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
             string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
             ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
         }

         return View(Users);
     }

       [HttpPost]
     public ActionResult GridUsers(string UserId,string GridId,string UserCount)
     {
         DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
         try
         {
             if (UserId == null || UserId == "")
             {
                 int NulgridId = Convert.ToInt32(Session["GridID"]);
                 var TotalUsers = db.ManageTabGrids.Where(q => q.GridID == NulgridId && q.IsActive == true).Select(q => q.UserID).ToList();
                 foreach (var item in TotalUsers)
                 {
                     if (item != null)
                     {
                         var data = db.ManageTabGrids.Where(x => x.UserID == item && x.GridID==NulgridId).FirstOrDefault();
                         db.ManageTabGrids.DeleteObject(data);

                        // var ManTabGrid = db.ManageTabGrids.Where(q => q.UserID == item && q.GridID == NulgridId).Select(r => r).FirstOrDefault();
                         //ManTabGrid.UserSelected = false;
                     }
                 }
                 db.SaveChanges();
             }
             else
             {

                 int GId = Convert.ToInt32(Session["GridID"]);
                 string ManageUsers = UserId.Trim(new Char[] { ' ', ',' });
                 int userId = int.Parse(Session["UserID"].ToString());
                 Session["TabAssigned"] = null;
                 List<int?> Users = new List<int?>();
                 if (UserId != "")
                 {

                     string[] value = ManageUsers.Split(',');
                     for (int i = 0; i < value.Count(); i++)
                     {
                         var item = Convert.ToInt32(value[i]);
                         var ManTabs = db.ManageTabGrids.Where(q => q.UserID == item && q.GridID == GId).Select(r => r).FirstOrDefault();

                         ManTabs.UserSelected = true;
                         Users.Add(Convert.ToInt32(value[i]));

                     }
                 }



                 var TotalUsers = db.ManageTabGrids.Where(q => q.GridID == GId && q.IsActive == true).Select(q => q.UserID).ToList();
                 var UnSelected = TotalUsers.Except(Users).ToList();

                 foreach (var item in UnSelected)
                 {
                     if (item != null)
                     {
                         var data = db.ManageTabGrids.Where(x => x.UserID == item && x.GridID == GId).FirstOrDefault();
                         db.ManageTabGrids.DeleteObject(data);

                        // var ManTabGrid = db.ManageTabGrids.Where(q => q.UserID == item && q.GridID == GId).Select(r => r).FirstOrDefault();
                       //  ManTabGrid.UserSelected = false;
                     }
                 }
                 db.SaveChanges();


             }
         }
         catch (Exception Ex)
         {
             string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
             string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
             ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
         }

         return Json("Success", JsonRequestBehavior.AllowGet);

     }


        /*
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
                                                     RoleName=mrole.RoleName
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
                string controllerName = "ManageTab";
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }
         */
    }
}
