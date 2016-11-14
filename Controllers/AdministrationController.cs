using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Text;
using System.Reflection;

namespace DSRCManagementSystem.Controllers
{
    public class AdministrationController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        
         ////////
        
        public ActionResult MenuDown(int FunctionID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.DefaultFunctionPrecedanceOrders.Where(x => x.FunctionID == FunctionID).Select(o => o).FirstOrDefault();
            val.PreceedanceOrder = val.PreceedanceOrder + 1;
            var val1 = db.DefaultFunctionPrecedanceOrders.Where(x => x.PreceedanceOrder == val.PreceedanceOrder).Select(o => o).FirstOrDefault();
            val1.PreceedanceOrder = val1.PreceedanceOrder - 1;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult MenuUP(int FunctionID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.DefaultFunctionPrecedanceOrders.Where(x => x.FunctionID == FunctionID).Select(o => o).FirstOrDefault();
            val.PreceedanceOrder = val.PreceedanceOrder - 1;
            var val1 = db.DefaultFunctionPrecedanceOrders.Where(x => x.PreceedanceOrder == val.PreceedanceOrder).Select(o => o).FirstOrDefault();
            val1.PreceedanceOrder = val1.PreceedanceOrder + 1;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult SubMenuDown(int PageModuleID, int FunctionID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.TestRoles ts = new DSRCManagementSystem.Models.TestRoles();
            var v1 = db.DefaultModulePrecedanceOrders.Where(x => x.PageModuleID == PageModuleID).Select(o => o).FirstOrDefault();
            v1.PreceedanceOrder += 1;
            var va = v1.PreceedanceOrder;
            var flag = (from p in db.FunctionModules
                        join pt in db.DefaultModulePrecedanceOrders on p.PageModuleID equals pt.PageModuleID
                        where (p.FunctionID == FunctionID && pt.PreceedanceOrder == va)
                        select new TestRoles()
                        {
                            FunctionID = p.FunctionID,
                            PreceedanceOrder = pt.PreceedanceOrder,
                            PageModuleID = p.PageModuleID,
                        }).ToList();
            var ap = 0;
            foreach (var item in flag)
            {
                ap = item.PageModuleID;
            }
            var val1 = db.DefaultModulePrecedanceOrders.Where(x => x.PageModuleID == ap).Select(o => o).FirstOrDefault();
            val1.PreceedanceOrder -= 1;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubMenuUP(int PageModuleID, int FunctionID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.TestRoles ts = new DSRCManagementSystem.Models.TestRoles();
            var v1 = db.DefaultModulePrecedanceOrders.Where(x => x.PageModuleID == PageModuleID).Select(o => o).FirstOrDefault();
            v1.PreceedanceOrder -= 1;
            var va = v1.PreceedanceOrder;
            var flag = (from p in db.FunctionModules
                        join pt in db.DefaultModulePrecedanceOrders on p.PageModuleID equals pt.PageModuleID
                        where (p.FunctionID == FunctionID && pt.PreceedanceOrder == va)
                        select new TestRoles()
                        {
                            FunctionID = p.FunctionID,
                            PreceedanceOrder = pt.PreceedanceOrder,
                            PageModuleID = p.PageModuleID,
                        }).ToList();
            var ap = 0;
            foreach (var item in flag)
            {
                ap = item.PageModuleID;
            }
            var val1 = db.DefaultModulePrecedanceOrders.Where(x => x.PageModuleID == ap).Select(o => o).FirstOrDefault();
            val1.PreceedanceOrder += 1;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult TestRoles(string rolename,int roleid )
        {
            string RoleName = rolename;
            int RoleId = roleid;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            AdministrationSetup ObjAS = new AdministrationSetup();
            try
            {

                ObjAS.RoleTypeID = RoleId;
                ObjAS.RoleName = RoleName;
                TempData["RoleName"] = RoleName;
                var temp = db.Master_Roles.Where(r => r.RoleName == RoleName).Select(f => f.RoleName).FirstOrDefault();
                ViewBag.RoleNamedisplay1 = temp;
                ObjAS.RoleList = GetRoles();
                ObjAS.Menu = GetMenu();
                ObjAS.MenuList = GetMenuList();
                var tem = db.Master_Roles.Where(r => r.RoleName == RoleName).Select(f => f.RoleID).FirstOrDefault();
                ObjAS.CheckVal = GetCheckVal(tem);
                ViewBag.RoleId = RoleId;

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(ObjAS);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult UnselectMenu(int RoleId, int FunctionId, int PageModuleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.RoleFunctionPrivileges.Where(x => x.FunctionID == FunctionId && x.RoleID == RoleId && x.PageModuleID == PageModuleId).Select(o => o).FirstOrDefault();
            if (val != null)
            {
                db.DeleteObject(val);
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SelectMenu(int RoleId, int FunctionId, int PageModuleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.RoleFunctionPrivileges.Where(x => x.FunctionID == FunctionId && x.RoleID == RoleId && x.PageModuleID == PageModuleId).Select(o => o).FirstOrDefault();
            var val1 = db.FunctionModules.Where(x => x.FunctionID == FunctionId).Select(o => o.PageModuleID).ToList();

            if (val1.Count() != 0)
            {
                if (val != null)
                {
                    // Already 
                }
                else
                {
                    RoleFunctionPrivilege role = new RoleFunctionPrivilege
                    {
                        RoleID = Convert.ToByte(RoleId),
                        FunctionID = Convert.ToByte(FunctionId),
                        PageModuleID = Convert.ToByte(PageModuleId),
                        CanRead = Convert.ToBoolean(1),
                        CanWrite = Convert.ToBoolean(1),
                        CanDelete = Convert.ToBoolean(0)
                    };
                    db.RoleFunctionPrivileges.AddObject(role);
                    db.SaveChanges();
                }
            }
            else
            {
                RoleFunctionPrivilege role = new RoleFunctionPrivilege
                {
                    RoleID = Convert.ToByte(RoleId),
                    FunctionID = Convert.ToByte(FunctionId),
                    PageModuleID = null,
                    CanRead = Convert.ToBoolean(1),
                    CanWrite = Convert.ToBoolean(1),
                    CanDelete = Convert.ToBoolean(0)
                };
                db.RoleFunctionPrivileges.AddObject(role);
                db.SaveChanges();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SelectSubMenu(int RoleId, int FunctionId, int PageModuleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.RoleFunctionPrivileges.Where(x => x.FunctionID == FunctionId && x.RoleID == RoleId && x.PageModuleID == PageModuleId).Select(o => o).FirstOrDefault();
            if (val == null)
            {
                RoleFunctionPrivilege role = new RoleFunctionPrivilege
                {
                    RoleID = Convert.ToByte(RoleId),
                    FunctionID = Convert.ToByte(FunctionId),
                    PageModuleID = Convert.ToByte(PageModuleId),
                    CanRead = Convert.ToBoolean(1),
                    CanWrite = Convert.ToBoolean(1),
                    CanDelete = Convert.ToBoolean(0)
                };
                db.RoleFunctionPrivileges.AddObject(role);
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult UnselectSubMenu(int RoleId, int FunctionId, int PageModuleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.RoleFunctionPrivileges.Where(x => x.FunctionID == FunctionId && x.RoleID == RoleId && x.PageModuleID == PageModuleId).Select(o => o).FirstOrDefault();
            if (val != null)
            {
                db.DeleteObject(val);
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public static List<DSRCManagementSystem.Models.CheckVal> GetCheckVal(int tem)
        {
            List<DSRCManagementSystem.Models.CheckVal> checkval = new List<DSRCManagementSystem.Models.CheckVal>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    checkval = (from rfp in db.RoleFunctionPrivileges
                                where rfp.RoleID == tem
                                select new DSRCManagementSystem.Models.CheckVal
                                {
                                    RoleId = (int)rfp.RoleID,
                                    FunctionID = (int)rfp.FunctionID,
                                    PageModuleId = (int)rfp.PageModuleID,
                                }).ToList();

                }
            }
            catch (Exception Ex)
            {
                string actionName = null;
                string controllerName = null;
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return checkval;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetMenuListCount(int FunctionId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> mlc = new List<SelectListItem>();
            mlc = (from lt in db.FunctionModules.Where(o => o.FunctionID == FunctionId)
                   select new MenuList()
                   {
                       FunctionId = lt.FunctionID,
                       PageModuleId = lt.PageModuleID,
                   }).AsEnumerable().Select(m => new SelectListItem() { Value = m.PageModuleId.ToString(), Text = "" });
            return Json(new SelectList(mlc, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSubMenuListCount(int FunctionId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> smlc = new List<SelectListItem>();
            smlc = (from lt in db.FunctionModules.Where(o => o.FunctionID == FunctionId)
                    select new MenuList()
                    {
                        FunctionId = lt.FunctionID,
                        PageModuleId = lt.PageModuleID,
                    }).AsEnumerable().Select(m => new SelectListItem() { Value = m.PageModuleId.ToString(), Text = "" });
            return Json(new SelectList(smlc, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
 

        
        /// ////////
        
        
        
        public ActionResult Roles(string RoleName,int RoleId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            AdministrationSetup ObjAS = new AdministrationSetup();
            try
            {
            ObjAS.RoleTypeID = RoleId;
            ObjAS.RoleName = RoleName;
            TempData["RoleName"] = RoleName;
            var temp = db.Master_Roles.Where(r => r.RoleName == RoleName).Select(f => f.RoleName).FirstOrDefault();
            ViewBag.RoleNamedisplay1 = temp;
            ObjAS.RoleList = GetRoles();
            ObjAS.Menu = GetMenu();
            ObjAS.MenuList = GetMenuList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
           return View(ObjAS);
           
        }
        [HttpPost]
        public ActionResult Roles(AdministrationSetup ADMSetUp)
        {
            try
            {

            DSRCManagementSystemEntities1 db1 = new DSRCManagementSystemEntities1();
            int roleid = ADMSetUp.RoleTypeID;
            var temp = db1.Master_Roles.Where(r => r.RoleID == roleid).Select(f => f.RoleName).FirstOrDefault();
            ViewBag.RoleNamedisplay = temp;

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ADMSetUp.RoleID = Convert.ToByte(roleid);

            List<string> SelectedFunId = ADMSetUp.selectedFunctionID;
            List<string> SelectedPageModuleID = ADMSetUp.selectedPageModuleID;
            List<string> SelectedPreceedenceOrder = ADMSetUp.selectedPreceedenceorder;


            //if (ADMSetUp.RoleID != 0 && SelectedFunId != null && SelectedPageModuleID != null)
            if (ADMSetUp.RoleID != 0 && SelectedFunId != null)
            {
                ADMSetUp.PrecedanceOrder = 1;
                var value = SelectedFunId.Concat(SelectedPageModuleID).Distinct();
                List<string> Obj = new List<string>();

                var rolefunctionprivillege = (from rolefunction in db.RoleFunctionPrivileges
                                              where rolefunction.RoleID == roleid
                                              orderby rolefunction.FunctionID
                                              select rolefunction).ToList();


                foreach (var rolefunfunctionid in rolefunctionprivillege)
                {
                    string PageModuleID;
                    if (rolefunfunctionid.PageModuleID == null)
                    {
                        PageModuleID = string.Empty;
                    }
                    else
                    {
                        PageModuleID = rolefunfunctionid.PageModuleID.Value.ToString();
                    }
                    Obj.Add(rolefunfunctionid.FunctionID.Value.ToString() + (PageModuleID == "" ? "" : "," + PageModuleID));
                }
                List<string> ToDeleteFunction = Obj.Except(value).ToList();
                foreach (var ToDeleteID in ToDeleteFunction)
                {
                    if (ToDeleteID.Length > 0)
                    {
                        byte todeletefunid = Convert.ToByte(ToDeleteID.Split(',')[0]);
                        byte todeletepageid = 0;
                        if (ToDeleteID.Split(',').Length > 1)
                        {
                            todeletepageid = Convert.ToByte(ToDeleteID.Split(',')[1]);
                        }

                        if (todeletepageid == 0)
                        {
                            var RoleFunctionPrivillege = (from rolefunction in db.RoleFunctionPrivileges
                                                          where rolefunction.RoleID == roleid && rolefunction.FunctionID == todeletefunid
                                                          select rolefunction).FirstOrDefault();
                            if (RoleFunctionPrivillege != null)
                                db.DeleteObject(RoleFunctionPrivillege);
                        }
                        else
                        {
                            var RoleFunctionPrivillege = (from rolefunction in db.RoleFunctionPrivileges
                                                          where rolefunction.RoleID == roleid && rolefunction.FunctionID == todeletefunid && rolefunction.PageModuleID == todeletepageid
                                                          select rolefunction).FirstOrDefault();
                            if (RoleFunctionPrivillege != null)
                                db.DeleteObject(RoleFunctionPrivillege);
                        }
                        db.SaveChanges();
                    }
                }


               
                
                
                foreach (string functionid in ADMSetUp.selectedFunctionID)
                {
                    byte FuncID = Convert.ToByte(functionid);

                    var FunctionPrecedenceOrderObj = db.FunctionPrecedanceOrders.CreateObject();
                    FunctionPrecedenceOrderObj.RoleID = ADMSetUp.RoleID;
                    FunctionPrecedenceOrderObj.FunctionID = FuncID;
                    FunctionPrecedenceOrderObj.PreceedanceOrder = ADMSetUp.PrecedanceOrder;
                    db.FunctionPrecedanceOrders.AddObject(FunctionPrecedenceOrderObj);
                    db.SaveChanges();

                    
                    
                    if (ADMSetUp.selectedPageModuleID != null)
                    {
                        foreach (string pageid in ADMSetUp.selectedPageModuleID)
                        {
                            int FunctionID = Convert.ToInt32(pageid.Split(',')[0]);
                            byte PageModuleID = Convert.ToByte(pageid.Split(',')[1]);

                            if (PageModuleID == 0)
                            {
                                PageModuleID = Convert.ToByte(null);
                            }

                            if (FunctionID == Convert.ToInt32(functionid))
                            {
                                if (db.RoleFunctionPrivileges.Any(R => R.RoleID == ADMSetUp.RoleID && R.FunctionID == FunctionID && R.PageModuleID == PageModuleID))
                                {
                                    //Already Menu exists
                                }
                                else
                                {
                                    //Already Menu does not exists
                                    RoleFunctionPrivilege role = new RoleFunctionPrivilege
                                    {
                                        RoleID = Convert.ToByte(ADMSetUp.RoleID),
                                        FunctionID = Convert.ToByte(functionid),
                                        PageModuleID = Convert.ToByte(PageModuleID),
                                        CanRead = Convert.ToBoolean(1),
                                        CanWrite = Convert.ToBoolean(1),
                                        CanDelete = Convert.ToBoolean(0)
                                    };
                                    db.RoleFunctionPrivileges.AddObject(role);
                                    db.SaveChanges();
                                }
                            }
                        }
                        if (db.RoleFunctionPrivileges.Any(R => R.RoleID == ADMSetUp.RoleID && R.FunctionID == FuncID))
                        {
                            //Exists Parant node
                        }
                        else
                        {
                            //No Parant Node
                            RoleFunctionPrivilege role = new RoleFunctionPrivilege
                            {
                                RoleID = Convert.ToByte(ADMSetUp.RoleID),
                                FunctionID = Convert.ToByte(functionid),
                                CanRead = Convert.ToBoolean(1),
                                CanWrite = Convert.ToBoolean(1),
                                CanDelete = Convert.ToBoolean(0)
                            };
                            db.RoleFunctionPrivileges.AddObject(role);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (db.RoleFunctionPrivileges.Any(R => R.RoleID == ADMSetUp.RoleID && R.FunctionID == FuncID))
                        {
                            //Exists Parant node
                        }
                        else
                        {
                            //No Parant Node
                            RoleFunctionPrivilege role = new RoleFunctionPrivilege
                            {
                                RoleID = Convert.ToByte(ADMSetUp.RoleID),
                                FunctionID = Convert.ToByte(functionid),
                                CanRead = Convert.ToBoolean(1),
                                CanWrite = Convert.ToBoolean(1),
                                CanDelete = Convert.ToBoolean(0)
                            };
                            db.RoleFunctionPrivileges.AddObject(role);
                            db.SaveChanges();
                        }
                    }
                    ADMSetUp.PrecedanceOrder++;
                }

                ADMSetUp.PrecedanceOrder = 1;

                if (ADMSetUp.selectedPageModuleID != null)
                {
                    foreach (string pageid in ADMSetUp.selectedPageModuleID)
                    {
                        int FunctionID = Convert.ToInt32(pageid.Split(',')[0]);
                        byte PageModuleID = Convert.ToByte(pageid.Split(',')[1]);

                        if (PageModuleID == 0)
                        {
                            PageModuleID = Convert.ToByte(null);
                        }
                        else
                        {
                            var ModulePrecedenceOrderObj = db.ModulePrecedanceOrders.CreateObject();
                            ModulePrecedenceOrderObj.RoleID = ADMSetUp.RoleID;
                            ModulePrecedenceOrderObj.PageModuleID = PageModuleID;
                            ModulePrecedenceOrderObj.PreceedanceOrder = ADMSetUp.PrecedanceOrder;
                            db.ModulePrecedanceOrders.AddObject(ModulePrecedenceOrderObj);
                            db.SaveChanges();

                            ADMSetUp.PrecedanceOrder++;
                        }
                    }
                }
            }


           
            ADMSetUp.RoleList = GetRoles();
            ADMSetUp.Menu = GetMenu();
            ADMSetUp.MenuList = GetMenuList();
            int userId = Convert.ToInt32(Session["UserID"]);
            var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
            int Id = roleID.FirstOrDefault();
            Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)userId, Id);
             }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return RedirectToAction("ViewRoles","ManageRole");
        }
        private List<SelectListItem> GetRoles()
        {
            try
            {
                var RoleList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<RoleType> Names = (from data in db.Master_Roles
                                            select new RoleType
                                            {
                                                RoleID = data.RoleID,
                                                RoleName = data.RoleName,
                                            }).ToList();
                    foreach (var item in Names)
                    {
                        RoleList.Add(new SelectListItem { Text = item.RoleName, Value = item.RoleID.ToString() });
                    }
                }
                return RoleList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
           
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.UserName).ToList();
            var tmp = "";
            try
            {
            int len = obj.Count; int i = 0;
            foreach (var str in obj)
            {
                i++;
                tmp += str;
                if (i < len)
                {
                    tmp += ", ";
                }
            }
             }
            catch (Exception Ex)
            {
                string actionName = null;
                string controllerName = null;
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return tmp;
        }

        public static List<DSRCManagementSystem.Models.Menu> GetMenu()
        {
            List<DSRCManagementSystem.Models.Menu> menu = new List<DSRCManagementSystem.Models.Menu>();
            try{
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //menu = db.Functions.Where(x => x.IsActive == true).Select(x => new Menu()
                //{
                //    FunctionID = x.FunctionID,
                //    FunctionName = x.FunctionName
                //}).ToList();
                menu = (from function in db.Functions
                        join defaultfunctionorder in db.DefaultFunctionPrecedanceOrders on function.FunctionID equals defaultfunctionorder.FunctionID
                        where function.IsActive == true
                        orderby defaultfunctionorder.PreceedanceOrder ascending
                        select new DSRCManagementSystem.Models.Menu
                        {
                            FunctionID = function.FunctionID,
                            FunctionName = function.FunctionName
                        }).ToList();

            }
               }
            catch (Exception Ex)
            {
                string actionName = null;
                string controllerName = null;
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return menu;
        }

        public static List<MenuList> GetMenuList()
        {
            List<DSRCManagementSystem.Models.MenuList> menulist = new List<DSRCManagementSystem.Models.MenuList>();
            try{
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //menulist = (from function_module in db.FunctionModules
                //            join modules in db.Modules on function_module.PageModuleID equals modules.PageModuleID
                //            select new DSRCManagementSystem.Models.MenuList
                //            {
                //                FunctionId = function_module.FunctionID,
                //                PageModuleId = function_module.PageModuleID,
                //                ModuleName = modules.ModuleName
                //            }).OrderBy(x => x.FunctionId).ToList();
                menulist = (from function_module in db.FunctionModules
                            join modules in db.Modules on function_module.PageModuleID equals modules.PageModuleID
                            join defaultmodulesceorder in db.DefaultModulePrecedanceOrders on modules.PageModuleID equals defaultmodulesceorder.PageModuleID
                            where modules.IsActive == true
                            orderby defaultmodulesceorder.PreceedanceOrder ascending
                            select new DSRCManagementSystem.Models.MenuList
                            {
                                FunctionId = function_module.FunctionID,
                                PageModuleId = function_module.PageModuleID,
                                ModuleName = modules.ModuleName
                            }).ToList();
            }
               }
            catch (Exception Ex)
            {
                string actionName = null;
                string controllerName =null;
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return menulist;
        }

        [HttpGet]
        public ActionResult GetMenuForRole(AdministrationSetup ADMSetUp)
        {
            List<DSRCManagementSystem.Models.RoleFunctionPrivilages> RoleFunctionPrivilages = new List<DSRCManagementSystem.Models.RoleFunctionPrivilages>();
            try
            {
            int roleid = ADMSetUp.RoleTypeID;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            RoleFunctionPrivilages = db.RoleFunctionPrivileges.Where(R => R.RoleID == roleid).Select(x => new RoleFunctionPrivilages()
            {
                FunctionId = x.FunctionID,
                PageModuleId = x.PageModuleID
            }).ToList();
               }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(RoleFunctionPrivilages, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult NewRoles()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Newroles(RoleType rt)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //RoleType rt=new RoleType();
            try
            {
            var obj = db.Master_Roles.CreateObject();
            obj.RoleName = rt.RoleName;
            db.Master_Roles.AddObject(obj);
            db.SaveChanges();
            var id = db.Master_Roles.Where(s => s.RoleName == rt.RoleName).Select(s => s.RoleID).SingleOrDefault();
            var obj1 = db.RoleFunctionPrivileges.Where(x => x.RoleID == 74).ToList();
            foreach (var a in obj1)
            {
                var rf = db.RoleFunctionPrivileges.CreateObject();
                rf.RoleID = id;
                rf.FunctionID = a.FunctionID;
                rf.PageModuleID = a.PageModuleID;
                rf.CanRead = true;
                rf.CanWrite = true;
                rf.CanDelete = false;
                db.RoleFunctionPrivileges.AddObject(rf);
                db.SaveChanges();

            }

            var md = db.ModulePrecedanceOrders.Where(c => c.RoleID == 74).ToList();
            foreach (var b in md)
            {
                var mp = db.ModulePrecedanceOrders.CreateObject();
                mp.RoleID = id;
                mp.PageModuleID = b.PageModuleID;
                mp.PreceedanceOrder = b.PreceedanceOrder;
                db.ModulePrecedanceOrders.AddObject(mp);
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
            // return View();
        }

        [HttpGet]



        public ActionResult Value(int value)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.Email obj = new DSRCManagementSystem.Models.Email();
            var Path = objdb.EmailTemplates.Where(x => x.EmailTemplateID == value).Select(o => o.TemplatePath).FirstOrDefault();
            return Json(new { Name = Path }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddNew()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            try{
            var EmailList = (from p in objdb.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = p.UserID,
                                 UserName = p.UserName
                             }).ToList();
            var EmailList1 = (from p in objdb.Users.Where(x => x.IsActive == true)
                              select new
                              {
                                  Id1 = p.UserID,
                                  UserName = p.UserName
                              }).ToList();

            var EmailList2 = (from p in objdb.Users.Where(x => x.IsActive == true)
                              select new
                              {
                                  Id2 = p.UserID,
                                  UserName = p.UserName
                              }).ToList();

            var Purpose = (from pi in objdb.EmailTemplates.Where(x => x.IsActive == true)
                           select new
                           {
                               Id3 = pi.EmailTemplateID,
                               Template = pi.TemplatePurpose
                           }).ToList();


            ViewBag.Email = new MultiSelectList(EmailList, "Id", "UserName");
            ViewBag.Email1 = new MultiSelectList(EmailList1, "Id1", "UserName");
            ViewBag.Email2 = new MultiSelectList(EmailList1, "Id2", "UserName");
            ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View();
        }

        [HttpPost]
        public ActionResult AddNew(Email objmail)
        {
            try
            {


                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.EmailPurpose objpurpose = new DSRCManagementSystem.EmailPurpose();
                var already = db.EmailPurposes.Where(x => x.EmailPurposeName == objmail.Purpose && x.IsActive == true).Select(x => x).FirstOrDefault();
                if (already != null)
                {
                    return Json(new { Result = "Already", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    var val = db.EmailTemplates.Where(x => x.TemplatePurpose == objmail.Purpose).Select(o => o.EmailTemplateID).FirstOrDefault();
                    var val1 = db.EmailTemplates.Where(x => x.TemplatePurpose == objmail.Purpose).Select(o => o.TemplatePath).FirstOrDefault();
                    objmail.Template = val1;

                    //string temp = "";

                    //int count = objmail.To.Count();

                    //foreach (var num in objmail.To)
                    //{

                    //    temp += num + ",";
                    //}
                    //string temp1 = "";

                    //foreach (var num in objmail.CC)
                    //{
                    //    temp1 += num + ",";
                    //}
                    //string temp2 = "";

                    //foreach (var num in objmail.BCC)
                    //{
                    //    temp2 += num + ",";
                    //}
                    objpurpose.EmailPurposeName = objmail.Purpose;
                    objpurpose.EmailTemplateID = val;
                    objpurpose.To = objmail.To != null ? objmail.To : "";
                    objpurpose.CC = objmail.CC != null ? objmail.CC : "";
                    objpurpose.BCC = objmail.BCC != null ? objmail.BCC : "";
                    objpurpose.Subject = objmail.Subject;
                    objpurpose.IsActive = true;
                    db.AddToEmailPurposes(objpurpose);




                    db.SaveChanges();

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AddNewEdit(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.Email obj = new DSRCManagementSystem.Models.Email();

           try

           {
               var EmailList = (from p in objdb.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 UserId = p.UserID,
                                 UserName = p.UserName
                             }).ToList();

            var EmailList1 = (from p in objdb.Users.Where(x => x.IsActive == true)
                              select new
                              {
                                  Id1 = p.UserID,
                                  UserName1 = p.UserName
                              }).ToList();

            var EmailList2 = (from p in objdb.Users.Where(x => x.IsActive == true)
                              select new
                              {
                                  Id2 = p.UserID,
                                  UserName2 = p.UserName
                              }).ToList();

            obj = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeID == Id)
                   join t in objdb.EmailTemplates on p.EmailTemplateID equals t.EmailTemplateID
                   select new DSRCManagementSystem.Models.Email
                   {
                       PurposeId = t.EmailTemplateID,
                       Purpose = p.EmailPurposeName,
                       To = p.To,
                       CC = p.CC,
                       BCC = p.BCC,
                       Subject = p.Subject,
                       Template = t.TemplatePath
                   }).FirstOrDefault();

            List<int> selectedEmail = new List<int>();
            if (obj.To != null)
            {

                string[] tokens = obj.To.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var i in tokens)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedEmail.Add(val);
                }
            }
            List<int> selectedEmail1 = new List<int>();
            if (obj.CC != null)
            {

                string[] tokens1 = obj.CC.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var i in tokens1)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedEmail1.Add(val);
                }
            }

            List<int> selectedEmail2 = new List<int>();
            if (obj.BCC != null)
            {

                string[] tokens2 = obj.BCC.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var i in tokens2)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedEmail2.Add(val);
                }
            }



            var value = (from pi in objdb.EmailTemplates.Where(x => x.IsActive == true)
                         select new
                         {
                             Id3 = pi.EmailTemplateID,
                             Template = pi.TemplatePurpose
                         }).ToList();

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
        public ActionResult AddNewEdit(Email objmail)
        {
            int UserId = int.Parse(Session["UserID"].ToString());

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.EmailPurposeLog obj = new DSRCManagementSystem.EmailPurposeLog();

          try
          {
              var fed = db.EmailPurposes.Where(x => x.EmailPurposeName == objmail.Purpose).Select(o => o).FirstOrDefault();


            if (fed.To != objmail.To && fed.CC != objmail.CC && fed.BCC != objmail.BCC)
            {
                obj.EmailPurposeID = fed.EmailPurposeID;
                obj.Datetimestamp = System.DateTime.Now;
                obj.IsActive = true;
                obj.Content = fed.To + "/" + fed.CC + "/" + fed.BCC;
                obj.ParameterChanges = "TO/CC/BCC";
                obj.UpdatedUserID = UserId;
                db.AddToEmailPurposeLogs(obj);
                db.SaveChanges();
            }

            if (fed.To != objmail.To && fed.CC == objmail.CC && fed.BCC == objmail.BCC)
            {
                obj.EmailPurposeID = fed.EmailPurposeID;
                obj.Datetimestamp = System.DateTime.Now;
                obj.Content = fed.To;
                obj.IsActive = true;
                obj.ParameterChanges = "TO";
                obj.UpdatedUserID = UserId;
                db.AddToEmailPurposeLogs(obj);
                db.SaveChanges();
            }

            else if (fed.CC != objmail.CC && fed.To == objmail.To && fed.BCC == objmail.BCC)
            {
                obj.EmailPurposeID = fed.EmailPurposeID;
                obj.Datetimestamp = System.DateTime.Now;
                obj.IsActive = true;
                obj.Content = fed.CC;
                obj.ParameterChanges = "CC";
                obj.UpdatedUserID = UserId;
                db.AddToEmailPurposeLogs(obj);
                db.SaveChanges();
            }

            if (fed.BCC != null)
            {

                if (fed.BCC != objmail.BCC && fed.CC == objmail.CC && fed.To == objmail.To)
                {
                    obj.EmailPurposeID = fed.EmailPurposeID;
                    obj.Datetimestamp = System.DateTime.Now;
                    obj.IsActive = true;
                    obj.Content = fed.BCC;
                    obj.ParameterChanges = "BCC";
                    obj.UpdatedUserID = UserId;
                    db.AddToEmailPurposeLogs(obj);
                    db.SaveChanges();
                }
            }

            else if (fed.Subject != objmail.Subject && fed.CC == objmail.CC && fed.BCC == objmail.BCC)
            {
                obj.EmailPurposeID = fed.EmailPurposeID;
                obj.ParameterChanges = "Subject";
                obj.Content = "";
                obj.UpdatedUserID = UserId;
                obj.Datetimestamp = System.DateTime.Now;
                obj.IsActive = true;
                db.AddToEmailPurposeLogs(obj);
                db.SaveChanges();
            }
            fed.EmailPurposeName = objmail.Purpose;
            fed.To = objmail.To != null ? objmail.To : "";
            fed.CC = objmail.CC != null ? objmail.CC : "";
            fed.BCC = objmail.BCC != null ? objmail.BCC : "";
            fed.Subject = objmail.Subject;
            // fed.EmailTemplateID = objmail.Template;
            db.SaveChanges();
          }
          catch (Exception Ex)
          {
              string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
              string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
              ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

          }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteEmail(int Id)
        {
            int UserId = int.Parse(Session["UserID"].ToString());

            DSRCManagementSystem.EmailPurposeLog fed = new DSRCManagementSystem.EmailPurposeLog();

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {

            var ajenda = objdb.EmailPurposes.Where(x => x.EmailPurposeID == Id && x.IsActive == true).Select(o => o).FirstOrDefault();
            var log = objdb.EmailPurposeLogs.Where(x => x.EmailPurposeID == ajenda.EmailPurposeID).FirstOrDefault();
            if (log != null)
            {
                log.IsActive = false;
                log.UpdatedUserID = UserId;
                log.Datetimestamp = System.DateTime.Now;
                log.ParameterChanges = "IsActive";
                objdb.SaveChanges();
            }
            else
            {
                fed.Datetimestamp = System.DateTime.Now;
                fed.EmailPurposeID = ajenda.EmailPurposeID;
                fed.IsActive = false;
                fed.ParameterChanges = "Isactive";
                fed.UpdatedUserID = UserId;
                fed.Content = "1";
                objdb.AddToEmailPurposeLogs(fed);
                objdb.SaveChanges();
            }
            ajenda.IsActive = false;
            objdb.SaveChanges();

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }


            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ViewAllProjects()
        {
            var isReporting = (bool)Session["IsRerportingPerson"];

            ModelState.Clear();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
            List<Projects> projectData = new List<Projects>();
           try{
           

            if (isReporting)
            {
                projectData = (from p in db.Projects
                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                               where p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true
                               select new Projects()
                               {
                                   ProjectID = p.ProjectID,
                                   ProjectName = p.ProjectName,
                                   ProjectCode = p.ProjectCode,
                                   ProjectType = pt.ProjectTypeName,
                                   RAGStatus = p.RAGStatus,
                                   RAGComments = p.RAGComments ?? "Comments not added",
                                   CommentsCreated = p.CommentsCreated
                               }).OrderBy(x => x.RAGStatus).ToList();
            }
            else
            {
                projectData = (from p in db.Projects
                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                               where p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true
                               select new Projects()
                               {
                                   ProjectID = p.ProjectID,
                                   ProjectName = p.ProjectName,
                                   ProjectCode = p.ProjectCode,
                                   ProjectType = pt.ProjectTypeName,
                                   RAGStatus = p.RAGStatus,
                                   RAGComments = p.RAGComments ?? "Comments not added",
                                   CommentsCreated = p.CommentsCreated
                               }).OrderBy(x => x.RAGStatus).ToList();
            }

            ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");
           }
           catch (Exception Ex)
           {
               string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
               string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
               ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

           }
            return View(projectData);
        }

        [HttpPost]
        public ActionResult ViewAllProjects(FormCollection form)
        {
            
            List<Projects> projectData = new List<Projects>();
            try
            {
                if (ModelState.IsValid)
                {

                    var isReporting = (bool)Session["IsRerportingPerson"];
                    int userId = int.Parse(Session["UserID"].ToString());
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    string projectID = (form["ProjectTypeDL"] == "") ? "0" : form["ProjectTypeDL"].ToString();
                    int projectTypeID = int.Parse(projectID.ToString());
                    bool status = form["Inactive"].Contains("true");
                    DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                    ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName", projectID);

                    //List<Projects> datas = new List<Projects>();
                    if (projectTypeID == 0)
                    {
                        if (isReporting)
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                        }
                        else
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                        }
                    }
                    else
                    {
                        if (isReporting)
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                        }
                        else
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated
                                               }).OrderBy(x => x.RAGStatus).ToList();
                            }
                        }
                    }
                    return View(projectData);
                }
                else
                {
                    DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                    ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");
                    return View(projectData);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(projectData);
        }
       
        [HttpGet]
        public ActionResult EditMenu(string text, int fid, int mid,string Menutype)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try{
                if (Menutype == "F")
                {
                    if (fid != 0)
                    {
                        var varFunctionPages = (from f in db.Functions
                                                where f.IsActive == true && f.FunctionID == fid
                                                join p in db.Pages on f.FunctionName equals p.PageName
                                                select new
                                                {
                                                    Pageid = p.PageID
                                                }).FirstOrDefault();

                        var objfun = db.Functions.Where(f => f.FunctionID == fid).Select(f => f).FirstOrDefault();
                        objfun.FunctionName = text;
                        db.SaveChanges();

                        if (varFunctionPages != null)
                        {
                            var varPages = db.Pages.Where(p => p.PageID == varFunctionPages.Pageid).Select(p => p).FirstOrDefault();
                            varPages.PageName = text;
                            db.SaveChanges();
                        }
                    }
                }

                else if (Menutype == "S")
                {
                    if (mid != 0)
                    {
                        var varModulesPages = (from f in db.Modules
                                               where f.IsActive == true && f.PageModuleID == mid
                                               join p in db.Pages on f.ModuleName equals p.PageName
                                               select new
                                               {
                                                   Pageid = p.PageID
                                               }).FirstOrDefault();

                        var objmod = db.Modules.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                        objmod.ModuleName = text;
                        db.SaveChanges();
                        if (varModulesPages != null)
                        {
                            var varPages = db.Pages.Where(p => p.PageID == varModulesPages.Pageid).Select(p => p).FirstOrDefault();
                            varPages.PageName = text;
                            db.SaveChanges();
                        }
                    }
                }

            int userId = Convert.ToInt32(Session["UserID"]);
            var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
            int Id = roleID.FirstOrDefault();
            Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)userId, Id);
            }
            catch (Exception Ex)
            {
                //SendExceptionMailController.SendMail(Ex);
                MethodBase method = Ex.TargetSite;
                string smethod=method.Name;
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult EditMenuItem(int PageModuleId, int FunctionId,string menutype)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int function = FunctionId;

                if (menutype == "F")
                {
                    if (FunctionId != 0)
                    {
                        var varFunctionPages = (from f in db.Functions
                                                where f.IsActive == true && f.FunctionID == FunctionId
                                                join p in db.Pages on f.FunctionName equals p.PageName
                                                select new
                                                {
                                                    Pageid = p.PageID
                                                }).FirstOrDefault();

                        var objfun = db.Functions.Where(f => f.FunctionID == FunctionId).Select(f => f).FirstOrDefault();

                        if (varFunctionPages != null)
                        {
                            var varPages = db.Pages.Where(p => p.PageID == varFunctionPages.Pageid).Select(p => p).FirstOrDefault();

                        }

                        ViewBag.Modules = objfun.FunctionName;
                        ViewBag.PageModuleID = 0;
                        ViewBag.FunctionID = function;
                        ViewBag.Menu = menutype;
                    }
                }

                else if (menutype == "S")
                {
                    if (PageModuleId != 0)
                    {
                        var varModulesPages = (from f in db.Modules
                                               where f.IsActive == true && f.PageModuleID == PageModuleId
                                               join p in db.Pages on f.ModuleName equals p.PageName
                                               select new
                                               {
                                                   Pageid = p.PageID
                                               }).FirstOrDefault();

                        var objmod = db.Modules.Where(m => m.PageModuleID == PageModuleId).Select(m => m).FirstOrDefault();

                        ViewBag.Modules = objmod.ModuleName;
                        ViewBag.PageModuleID = objmod.PageModuleID;
                        ViewBag.FunctionID = function;
                        ViewBag.Menu = menutype;
                        //    if (varModulesPages != null)
                        //    {
                        //        var varPages = db.Pages.Where(p => p.PageID == varModulesPages.Pageid).Select(p => p).FirstOrDefault();

                        //    }
                        //    ViewBag.Modules = objmod.ModuleName;
                    }
                }

            }
            catch (Exception ex)
            {

            }

            return View();
        }
      
        
        [HttpPost]
        public ActionResult DeleteFunction(int fid, int mid,string menutype)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try{
                if (menutype == "F")
                {
                    if (fid != 0)
                    {

                        var objfun = db.Functions.Where(f => f.FunctionID == fid).Select(f => f).FirstOrDefault();
                        objfun.IsActive = false;
                        db.SaveChanges();
                    }
                }

                if (menutype == "S")
                {
                    if (mid != 0)
                    {
                        var functionmodules = db.FunctionModules.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                        db.DeleteObject(functionmodules);

                        var modulepage = db.ModulePages.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                        db.DeleteObject(modulepage);

                        var moduleprecedence = db.ModulePrecedanceOrders.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                        db.DeleteObject(moduleprecedence);

                        var objmod = db.Modules.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                        db.DeleteObject(objmod);
                        db.SaveChanges();
                    }
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


        [HttpPost]
        public ActionResult Reset()
        {
            return View();
        }
    }
}
