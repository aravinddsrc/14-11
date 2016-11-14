using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Text;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using System.Data.Linq.SqlClient;

namespace DSRCManagementSystem.Controllers
{
    public class AdministrationSetupController : Controller
    {
        //
        // GET: /AdministrationSetup/
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult RolesandPermission()
        {
            AdministrationSetup ObjAS = new AdministrationSetup();
            try
            {
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
        public ActionResult RolesandPermission(AdministrationSetup ADMSetUp)
        {
            try
            {
                int roleid = ADMSetUp.RoleTypeID;
                List<string> SelectedFunId = ADMSetUp.selectedFunctionID;
                List<string> SelectedPageModuleID = ADMSetUp.selectedPageModuleID;
                if (roleid != 0 && SelectedFunId != null && SelectedPageModuleID != null)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

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
                        int funid = Convert.ToInt32(functionid);
                        if (ADMSetUp.selectedPageModuleID != null)
                        {
                            foreach (string pagemoduleid in ADMSetUp.selectedPageModuleID)
                            {
                                int FunctionID = Convert.ToInt32(pagemoduleid.Split(',')[0]);
                                int PageModuleID = Convert.ToInt32(pagemoduleid.Split(',')[1]);
                                if (PageModuleID == 0)
                                {
                                    PageModuleID = Convert.ToByte(null);
                                }
                                if (FunctionID == Convert.ToInt32(functionid))
                                {
                                    if (db.RoleFunctionPrivileges.Any(R => R.RoleID == roleid && R.FunctionID == FunctionID && R.PageModuleID == PageModuleID))
                                    {
                                        //exists
                                    }
                                    else
                                    {
                                        //not exists
                                        RoleFunctionPrivilege role = new RoleFunctionPrivilege
                                        {
                                            RoleID = Convert.ToByte(roleid),
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
                            if (db.RoleFunctionPrivileges.Any(R => R.RoleID == roleid && R.FunctionID == funid))
                            {
                                //Exists Parant node
                            }
                            else
                            {
                                //No Parant Node
                                RoleFunctionPrivilege role = new RoleFunctionPrivilege
                                {
                                    RoleID = Convert.ToByte(roleid),
                                    FunctionID = Convert.ToByte(functionid),
                                    //PageModuleID = Convert.ToByte(PageModuleID),
                                    CanRead = Convert.ToBoolean(1),
                                    CanWrite = Convert.ToBoolean(1),
                                    CanDelete = Convert.ToBoolean(0)
                                };
                                db.RoleFunctionPrivileges.AddObject(role);
                                db.SaveChanges();
                            }
                        }

                    }

                }
                ADMSetUp.RoleList = GetRoles();
                ADMSetUp.Menu = GetMenu();
                ADMSetUp.MenuList = GetMenuList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(ADMSetUp);
        }
        private List<SelectListItem> GetRoles()
        {
            var RoleList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
            try
            {

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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return RoleList;
        }

        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
            var tmp = "";
            try
            {
                List<int> lst = new List<int>();
                foreach (var str in Attendee.Split(','))
                {
                    lst.Add(Convert.ToInt32(str));
                }
                var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.UserName).ToList();
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
                ExceptionHandlingController.ExceptionDetails(Ex, "GetUserString", "AdministrationSetup");
            }
            return tmp;
        }

        public static List<DSRCManagementSystem.Models.Menu> GetMenu()
        {
            List<DSRCManagementSystem.Models.Menu> menu = new List<DSRCManagementSystem.Models.Menu>();

            try
            {


                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    //menu = db.Functions.Where(x => x.IsActive == true).Select(x => new Menu()
                    //{
                    //    FunctionID = x.FunctionID,
                    //    FunctionName = x.FunctionName
                    //}).ToList();
                    menu = (from function in db.Functions
                            join defaultfunctionorder in db.DefaultFunctionPrecedanceOrders on function.FunctionID equals
                                defaultfunctionorder.FunctionID
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
                ExceptionHandlingController.ExceptionDetails(Ex, "GetMenu", "AdministrationSetup");
            }
            return menu;
        }

        public static List<MenuList> GetMenuList()
        {
            List<DSRCManagementSystem.Models.MenuList> menulist = new List<DSRCManagementSystem.Models.MenuList>();
            try
            {
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
                ExceptionHandlingController.ExceptionDetails(Ex, "GetMenuList", "AdministrationSetup");
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


        #region EmailPurpose Get

        public ActionResult EmailPurpose()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.EmailPurpose> obj = new List<Models.EmailPurpose>();
            try
            {

                obj = (from p in objdb.EmailPurposes.Where(x => x.IsActive == true)
                       join t in objdb.EmailTemplates on p.EmailTemplateID equals t.EmailTemplateID
                       select new DSRCManagementSystem.Models.EmailPurpose
                       {
                           Id = p.EmailPurposeID,
                           To = p.To == " " ? "-" : p.To,
                           CC = p.CC == " " ? "-" : p.CC,
                           BCC = p.BCC == " " ? "-" : p.BCC,
                           Purpose = p.EmailPurposeName,
                           Subject = p.Subject,
                           Template = t.TemplatePath
                       }).ToList();


                foreach (var meetingSchedule in obj)
                {
                    if (meetingSchedule.To != "-")
                    {
                        meetingSchedule.To = AdministrationSetupController.GetUserString(objdb, meetingSchedule.To);
                    }
                    if (meetingSchedule.CC != "-")
                    {
                        meetingSchedule.CC = AdministrationSetupController.GetUserString(objdb, meetingSchedule.CC);
                    }
                    if (meetingSchedule.BCC != "-")
                    {
                        meetingSchedule.BCC = AdministrationSetupController.GetUserString(objdb, meetingSchedule.BCC);
                    }

                }

                var Purpose = (from pi in objdb.Master_EmailTemplateCategory.Where(x => x.IsActive == true)
                               select new
                               {
                                   Id3 = pi.EmailTemplateCategoryID,
                                   Template = pi.CategoryName
                               }).ToList();

                ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template");

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }


            return View(obj);
        }

        #endregion



        #region EmailPurpose Post
        [HttpPost]
        public ActionResult EmailPurpose(FormCollection form)
        {
            DSRCManagementSystemEntities1 objdb1 = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.EmailPurpose> objEmail = new List<Models.EmailPurpose>();
            
            try
            {
                int EmailCategoryId = (form["Emailcategory"] == "") ? 0 : Convert.ToInt16(form["Emailcategory"]);
                string AssignValue="";

                if (EmailCategoryId == 0)
                {
                    objEmail = (from p in objdb.EmailPurposes.Where(x => x.IsActive == true)
                                join t in objdb.EmailTemplates on p.EmailTemplateID equals t.EmailTemplateID
                                select new DSRCManagementSystem.Models.EmailPurpose
                                {
                                    Id = p.EmailPurposeID,
                                    To = p.To == " " ? "-" : p.To,
                                    CC = p.CC == " " ? "-" : p.CC,
                                    BCC = p.BCC == " " ? "-" : p.BCC,
                                    Purpose = p.EmailPurposeName,
                                    Subject = p.Subject,
                                    Template = t.TemplatePath
                                }).ToList();


                    foreach (var meetingSchedule in objEmail)
                    {
                        if (meetingSchedule.To != "-")
                        {
                            meetingSchedule.To = AdministrationSetupController.GetUserString(objdb, meetingSchedule.To);
                        }
                        if (meetingSchedule.CC != "-")
                        {
                            meetingSchedule.CC = AdministrationSetupController.GetUserString(objdb, meetingSchedule.CC);
                        }
                        if (meetingSchedule.BCC != "-")
                        {
                            meetingSchedule.BCC = AdministrationSetupController.GetUserString(objdb, meetingSchedule.BCC);
                        }

                    }

                    var Purpose = (from pi in objdb.Master_EmailTemplateCategory.Where(x => x.IsActive == true)
                                   select new
                                   {
                                       Id3 = pi.EmailTemplateCategoryID,
                                       Template = pi.CategoryName
                                   }).ToList();

                    ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template");

                }
                else
                {
                    var CategoryFilter = (from e in objdb1.Master_EmailTemplateCategory.Where(a => a.EmailTemplateCategoryID == EmailCategoryId)
                                          select new DSRCManagementSystem.Models.Email
                                          {

                                              EmailCategoryName = e.CategoryName

                                          }
                                 );
                    foreach (var Category in CategoryFilter)
                    {
                        AssignValue = Category.EmailCategoryName;

                    }


                    objEmail = (from p in objdb1.EmailPurposes.Where(x => x.EmailTemplateCategoryID == EmailCategoryId && x.IsActive == true)
                                join t in objdb1.EmailTemplates on p.EmailTemplateID equals t.EmailTemplateID
                                select new DSRCManagementSystem.Models.EmailPurpose
                                {
                                    Id = p.EmailPurposeID,
                                    To = p.To == " " ? "-" : p.To,
                                    CC = p.CC == " " ? "-" : p.CC,
                                    BCC = p.BCC == " " ? "-" : p.BCC,
                                    Purpose = p.EmailPurposeName,
                                    Subject = p.Subject,
                                    Template = t.TemplatePath
                                }).ToList();

                    foreach (var meetingSchedule1 in objEmail)
                    {
                        if (meetingSchedule1.To != "-")
                        {
                            meetingSchedule1.To = AdministrationSetupController.GetUserString(objdb, meetingSchedule1.To);
                        }
                        if (meetingSchedule1.CC != "-")
                        {
                            meetingSchedule1.CC = AdministrationSetupController.GetUserString(objdb, meetingSchedule1.CC);
                        }
                        if (meetingSchedule1.BCC != "-")
                        {
                            meetingSchedule1.BCC = AdministrationSetupController.GetUserString(objdb, meetingSchedule1.BCC);
                        }

                    }

                    var Purpose = (from pi in objdb.Master_EmailTemplateCategory.Where(x => x.IsActive == true)
                                   select new
                                   {
                                       Id3 = pi.EmailTemplateCategoryID,
                                       Template = pi.CategoryName
                                   }).ToList();

                    ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", EmailCategoryId);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objEmail);
        }

        #endregion


        [HttpGet]
        public ActionResult Value(int value)
        {
            var Path = "";
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                DSRCManagementSystem.Models.Email obj = new DSRCManagementSystem.Models.Email();
                Path = objdb.EmailTemplates.Where(x => x.EmailTemplateID == value).Select(o => o.TemplatePath).FirstOrDefault();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Name = Path }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddNew()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            try
            {
                var EmailList = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
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

                var Purpose = (from pi in objdb.EmailTemplates.Where(x => x.IsActive == true )
                               select new
                               {
                                   Id3 = pi.EmailTemplateID,
                                   Template = pi.TemplatePurpose
                               }).ToList();


                var Emailcategory = (from pi in objdb.Master_EmailTemplateCategory.Where(x => x.IsActive == true)
                                     select new
                                     {
                                         Id4 = pi.EmailTemplateCategoryID,
                                         Template = pi.CategoryName
                                     }).ToList();

                ViewBag.Emailcategory = new SelectList(Emailcategory, "Id4", "Template");

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
            var ShowResult = "";

            try
            {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    DSRCManagementSystem.EmailPurpose objpurpose = new DSRCManagementSystem.EmailPurpose();
                    DSRCManagementSystem.Master_EmailTemplateCategory emailtemplate = new DSRCManagementSystem.Master_EmailTemplateCategory();

                
                    var already = db.EmailPurposes.Where(x => x.EmailPurposeName == objmail.Purpose && x.IsActive == true).Select(x => x).FirstOrDefault();
                   
                    var EmailCategoryId = db.EmailPurposes.Where(z => z.EmailPurposeName == objmail.Purpose).Select(m => m.EmailTemplateCategoryID).FirstOrDefault();

                    var GetID = db.Master_EmailTemplateCategory.Where(m => m.CategoryName == objmail.EmailCategoryName).Select(o => o.EmailTemplateCategoryID).FirstOrDefault();

                    if (EmailCategoryId == null)
                    {
                        var val = db.EmailTemplates.Where(x => x.TemplatePurpose == objmail.Purpose).Select(o => o.EmailTemplateID).FirstOrDefault();
                        var value1 = db.EmailTemplates.Where(x => x.TemplatePurpose == objmail.Purpose).Select(o => o.TemplatePath).FirstOrDefault();
                        objmail.Template = value1;
                        

                        objpurpose.EmailPurposeName = objmail.Purpose;
                        objpurpose.EmailTemplateID = val;
                        objpurpose.To = objmail.To != null ? objmail.To : "";
                        objpurpose.CC = objmail.CC != null ? objmail.CC : "";
                        objpurpose.BCC = objmail.BCC != null ? objmail.BCC : "";
                        objpurpose.Subject = objmail.Subject.Trim();
                        objpurpose.IsActive = true;
                        objpurpose.EmailTemplateCategoryID = GetID;
                        db.AddToEmailPurposes(objpurpose);
                        db.SaveChanges();
                        ShowResult = "Success";
                    }
                    else
                    {
                        ShowResult = "Already";
                    }


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

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

      


        [HttpGet]
        public ActionResult AddNewEdit(int Id, string Purpose)
         {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.Email obj = new DSRCManagementSystem.Models.Email();
            try
            {


                var GetEmailTempId = objdb.EmailPurposes.Where(z => z.EmailPurposeName == Purpose).Select(m => m.EmailTemplateCategoryID).FirstOrDefault();

                var EmailCategoryId = objdb.Master_EmailTemplateCategory.Where(e => e.EmailTemplateCategoryID == GetEmailTempId).Select(l => l.EmailTemplateCategoryID).FirstOrDefault();

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

                var EmailCategory1 = (from p in objdb.Master_EmailTemplateCategory.Where(x => x.IsActive==true)
                                      select new
                                      {
                                          Id11 = p.EmailTemplateCategoryID,
                                          Template1 = p.CategoryName,
                                      }).ToList();

                ViewBag.EmailCategory1 = new SelectList(EmailCategory1, "Id11", "Template1", EmailCategoryId);


                ViewBag.Email = new MultiSelectList(EmailList, "UserId", "UserName", selectedEmail);
                ViewBag.Email1 = new MultiSelectList(EmailList1, "Id1", "UserName1", selectedEmail1);
                ViewBag.Email2 = new MultiSelectList(EmailList2, "Id2", "UserName2", selectedEmail2);
                ViewBag.Purpose = new SelectList(value, "Id3", "Template", obj.PurposeId);

                if (obj.To != "")
                {
                    obj.To = AdministrationSetupController.GetUserString(objdb, obj.To);
                }
                if (obj.CC != "")
                {
                    obj.CC = AdministrationSetupController.GetUserString(objdb, obj.CC);
                }
                if (obj.BCC != "")
                {
                    obj.BCC = AdministrationSetupController.GetUserString(objdb, obj.BCC);
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
        public ActionResult AddNewEdit(Email objmail)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var emailid = db.EmailPurposes.Where(z => z.EmailPurposeName == objmail.Purpose).Select(m => m.EmailTemplateCategoryID).FirstOrDefault();

                

                var Ecategory = db.Master_EmailTemplateCategory.Where(c => c.EmailTemplateCategoryID == emailid).Select(l => l.CategoryName).FirstOrDefault();

                var ECid = db.Master_EmailTemplateCategory.Where(k => k.CategoryName == objmail.EmailCategoryName).Select(j => j.EmailTemplateCategoryID).FirstOrDefault();


                int UserId = int.Parse(Session["UserID"].ToString());

                

                DSRCManagementSystem.EmailPurposeLog obj = new DSRCManagementSystem.EmailPurposeLog();

                var fed = db.EmailPurposes.Where(x => x.EmailPurposeName == objmail.Purpose).Select(o => o).FirstOrDefault();

                if (fed.To != objmail.To && fed.CC != objmail.CC && fed.BCC != objmail.BCC && fed.EmailTemplateCategoryID!=emailid)
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
                fed.EmailTemplateCategoryID = ECid;
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
            try
            {
                int UserId = int.Parse(Session["UserID"].ToString());

                DSRCManagementSystem.EmailPurposeLog fed = new DSRCManagementSystem.EmailPurposeLog();

                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
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
            try
            {
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


        [HttpPost]
        public ActionResult Reset()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddFunction()
        {
            try
            {
                GetController();
                GetAction();
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var LoadFunctions = (from f in db.Functions
                                     where f.IsActive == true
                                     select new
                                     {
                                         FunctionID = f.FunctionID,
                                         FunctionName = f.FunctionName
                                     }).ToList();
                ViewBag.Functions = new SelectList(LoadFunctions, "FunctionID", "FunctionName", 0);
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
        public ActionResult AddFunction(MenuItems model)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                int sessionUserId = Convert.ToInt32(Session["UserID"]);
                var getRoleid = objdb.UserRoles.Where(u => u.UserID == sessionUserId).Select(r => r.RoleID).FirstOrDefault();

                var varFunctions = db.Functions.CreateObject();
                varFunctions.FunctionName = model.NewFunctionName;
                varFunctions.IsActive = true;
                db.Functions.AddObject(varFunctions);
                db.SaveChanges();

                var preceedenceDFP = db.DefaultFunctionPrecedanceOrders.Where(f => f.FunctionID == model.InsertAfterFunctionID).Select(f => f.PreceedanceOrder).FirstOrDefault();
                var preceedenceFPC = db.FunctionPrecedanceOrders.Where(f => f.FunctionID == model.InsertAfterFunctionID).Select(f => f.PreceedanceOrder).FirstOrDefault();

                PrecOrderIncr(varFunctions.FunctionID, preceedenceFPC, preceedenceDFP, getRoleid);

                if (model.ControllerName != "--- Select---" && model.ActionName != "--- Select---")
                {
                    var varPages = db.Pages.CreateObject();
                    varPages.PageName = model.NewFunctionName.ToString();
                    varPages.PageURL = "~/" + model.ControllerName.Remove(model.ControllerName.Length - 10) + "/" + model.ActionName;
                    varPages.DateCreated = DateTime.Now;
                    db.Pages.AddObject(varPages);
                    db.SaveChanges();
                }

                //int counter = 0;
                //string line;
                //// Read the file and display it line by line.
                //System.IO.StreamReader file = new System.IO.StreamReader("c:\\test.txt");
                //while ((line = file.ReadLine()) != null)
                //{
                //    if (line.Contains("word"))
                //    {
                //        Console.WriteLine(counter.ToString() + ": " + line);
                //    }
                //    counter++;
                //}
                //file.Close();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public void PrecOrderIncr(int functionid, int preceedenceFPC, int preceedenceDFP, int Roleid)
        {
            try
            {
                List<int> funcpreclist = objdb.FunctionPrecedanceOrders.Select(f => f.PreceedanceOrder).ToList();
                var maxvalue = funcpreclist.Max();
                var varDefaultFunction = objdb.DefaultFunctionPrecedanceOrders.CreateObject();
                varDefaultFunction.FunctionID = Convert.ToByte(functionid);
                varDefaultFunction.PreceedanceOrder = preceedenceDFP + 1;
                objdb.DefaultFunctionPrecedanceOrders.AddObject(varDefaultFunction);
                objdb.SaveChanges();

                var funcprec = objdb.DefaultFunctionPrecedanceOrders.Where(t => t.FunctionID == functionid).Select(t => t.PreceedanceOrder).FirstOrDefault();
                var selectfunc = objdb.DefaultFunctionPrecedanceOrders.Where(f => f.PreceedanceOrder >= funcprec).ToList();
                foreach (DefaultFunctionPrecedanceOrder prec in selectfunc)
                {
                    prec.PreceedanceOrder += 1;
                    objdb.SaveChanges();
                }

                var varFunctionPreceedence = objdb.FunctionPrecedanceOrders.CreateObject();
                varFunctionPreceedence.FunctionID = Convert.ToByte(functionid);
                varFunctionPreceedence.RoleID = Convert.ToByte(Roleid);
                varFunctionPreceedence.PreceedanceOrder = preceedenceFPC + 1;
                objdb.FunctionPrecedanceOrders.AddObject(varFunctionPreceedence);
                objdb.SaveChanges();

                var funcprecFPO = objdb.FunctionPrecedanceOrders.Where(t => t.FunctionID == functionid && t.RoleID == Roleid).Select(t => t.PreceedanceOrder).FirstOrDefault();
                var selectfuncFPO = objdb.FunctionPrecedanceOrders.Where(f => f.PreceedanceOrder >= funcprecFPO).ToList();

                foreach (FunctionPrecedanceOrder prec in selectfuncFPO)
                {
                    prec.PreceedanceOrder += 1;
                    objdb.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
        }

        [HttpGet]
        public ActionResult AddSubFunction()
        {
            try
            {
                GetController();
                GetAction();
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var LoadFunctions = (from f in db.Functions
                                     where f.IsActive == true
                                     select new
                                     {
                                         FunctionID = f.FunctionID,
                                         FunctionName = f.FunctionName
                                     }).ToList();

                var LoadSubFunctions = (from f in db.Modules
                                        where f.IsActive == true
                                        select new
                                        {
                                            PageModuleID = f.PageModuleID,
                                            ModuleName = f.ModuleName
                                        }).ToList();
                ViewBag.Functions = new SelectList(LoadFunctions, "FunctionID", "FunctionName", 0);
                ViewBag.SubFunctions = new SelectList(LoadSubFunctions, "PageModuleID", "ModuleName", 0);
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
        public ActionResult AddSubFunction(SubmenuItems model)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                int sessionUserId = Convert.ToInt32(Session["UserID"]);
                var getRoleid = Convert.ToByte(objdb.UserRoles.Where(u => u.UserID == sessionUserId).Select(r => r.RoleID).FirstOrDefault());

                var varModules = db.Modules.CreateObject();
                varModules.ModuleName = model.NewModuleName;
                varModules.IsActive = true;
                db.Modules.AddObject(varModules);
                db.SaveChanges();

                var varFunctionModules = db.FunctionModules.CreateObject();
                varFunctionModules.FunctionID = (byte)model.FunctionId;
                varFunctionModules.PageModuleID = varModules.PageModuleID;
                db.FunctionModules.AddObject(varFunctionModules);
                db.SaveChanges();


                var preceedenceDFP = db.DefaultModulePrecedanceOrders.Where(f => f.PageModuleID == model.InsertAfterPageModuleId).Select(f => f.PreceedanceOrder).FirstOrDefault();
                var preceedenceMPC = db.ModulePrecedanceOrders.Where(m => m.PageModuleID == model.InsertAfterPageModuleId).Select(m => m.PreceedanceOrder).FirstOrDefault();

                var varDefaultModule = db.DefaultModulePrecedanceOrders.CreateObject();
                varDefaultModule.PageModuleID = varModules.PageModuleID;
                varDefaultModule.PreceedanceOrder = preceedenceDFP + 1;
                db.DefaultModulePrecedanceOrders.AddObject(varDefaultModule);
                db.SaveChanges();

                var varModule = db.ModulePrecedanceOrders.CreateObject();
                varModule.RoleID = getRoleid;
                varModule.PageModuleID = varModules.PageModuleID;
                varModule.PreceedanceOrder = preceedenceMPC + 1;
                db.ModulePrecedanceOrders.AddObject(varModule);
                db.SaveChanges();


                var moduleCount = db.FunctionModules.Where(fm => fm.FunctionID == model.FunctionId).Select(fm => fm.PageModuleID).ToList();

                var insertedmoduleprec = db.ModulePrecedanceOrders.Where(m => m.PageModuleID == varModule.PageModuleID && m.RoleID == getRoleid).Select(m => m.PreceedanceOrder).FirstOrDefault();
                var inserteddefmoduleprec = db.DefaultModulePrecedanceOrders.Where(m => m.PageModuleID == varModule.PageModuleID).Select(m => m.PreceedanceOrder).FirstOrDefault();
                var listSubModules = new List<ModulePrecedanceOrder>();
                var listSubModuleDef = new List<DefaultModulePrecedanceOrder>();
                if (moduleCount != null)
                {
                    foreach (var item in moduleCount)
                    {
                        if (item != varModule.PageModuleID)
                        {
                            listSubModules = db.ModulePrecedanceOrders.Where(m => m.PageModuleID == item).ToList();
                            listSubModuleDef = db.DefaultModulePrecedanceOrders.Where(m => m.PageModuleID == item).ToList();
                        }
                    }
                    foreach (var item in listSubModules)
                    {
                        if (item.PreceedanceOrder >= insertedmoduleprec)
                        {
                            item.PreceedanceOrder += 1;
                            db.SaveChanges();
                        }
                    }
                    foreach (var item in listSubModuleDef)
                    {
                        if (item.PreceedanceOrder >= inserteddefmoduleprec)
                        {
                            item.PreceedanceOrder += 1;
                            db.SaveChanges();
                        }
                    }
                }
                var varPages = db.Pages.CreateObject();
                varPages.PageName = model.NewModuleName;
                varPages.PageURL = "~/" + model.ControllerName.Remove(model.ControllerName.Length - 10) + "/" + model.ActionName;
                varPages.DateCreated = DateTime.Now;
                db.Pages.AddObject(varPages);
                db.SaveChanges();

                var varModulePages = db.ModulePages.CreateObject();
                varModulePages.PageModuleID = varModules.PageModuleID;
                varModulePages.PageId = varPages.PageID;
                db.ModulePages.AddObject(varModulePages);
                db.SaveChanges();
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
        public ActionResult EditMenu(string text, int fid, int mid)
        {
            try
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                if (fid != 0)
                {
                    var objfun = db.Functions.Where(f => f.FunctionID == fid).Select(f => f).FirstOrDefault();
                    objfun.FunctionName = text;
                    db.SaveChanges();
                }
                if (mid != 0)
                {
                    var objmod = db.Modules.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                    objmod.ModuleName = text;
                    db.SaveChanges();
                }

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
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteFunction(int fid, int mid)
        {
            try
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                if (fid != 0)
                {
                    var objfun = db.Functions.Where(f => f.FunctionID == fid).Select(f => f).FirstOrDefault();
                    objfun.IsActive = false;
                    db.SaveChanges();
                }
                if (mid != 0)
                {
                    var objmod = db.Modules.Where(m => m.PageModuleID == mid).Select(m => m).FirstOrDefault();
                    objmod.IsActive = false;
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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSubModule(int FunctionId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterSubModule = new List<SelectListItem>();

            try
            {

                if (FunctionId != 0)
                {

                    List<byte> validSubModule = new List<byte>();

                    validSubModule = db.FunctionModules.Where(f => f.FunctionID == FunctionId).Select(f => f.PageModuleID).ToList();

                    FilterSubModule = (from lt in db.Modules.Where(fm => validSubModule.Contains(fm.PageModuleID) && fm.IsActive == true)
                                       select new MenuList()
                                       {
                                           PageModuleId = lt.PageModuleID,
                                           ModuleName = lt.ModuleName
                                       }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.PageModuleId), Text = m.ModuleName });
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new SelectList(FilterSubModule, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        public void GetController()
        {
            List<string> ctrl = new List<string>();
            var controllers = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)).Select(t => t);
            foreach (System.Type controller in controllers)
            {
                ctrl.Add(Convert.ToString(controller.Name));
            }
            ViewBag.GetController = new SelectList(ctrl.ToList());
        }
        public void GetAction()
        {
            List<string> act = new List<string>();
            var controllers = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)).Select(t => t);
            foreach (System.Type controller in controllers)
            {
                var actions = controller.GetMethods().Where(t => t.Name != "Dispose" && !t.IsSpecialName && t.DeclaringType.IsSubclassOf(typeof(ControllerBase)) && t.IsPublic && !t.IsStatic).ToList();
                foreach (var action in actions)
                {
                    var HasPostAttr = action.GetCustomAttributes(typeof(HttpPostAttribute), false).Any();
                    act.Add(action.Name.ToString() + (HasPostAttr ? " (Post) " : ""));
                }
                ViewBag.GetAction = new SelectList(act.ToList());
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult FilterAction(string ControllerName)
        {
            List<string> act = new List<string>();
            try
            {
                var controller = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t) && t.Name == ControllerName).Select(t => t).FirstOrDefault();
                var actions = controller.GetMethods().Where(t => t.Name != "Dispose" && !t.IsSpecialName && t.DeclaringType.IsSubclassOf(typeof(ControllerBase)) && t.IsPublic && !t.IsStatic).ToList();
                foreach (var action in actions)
                {
                    var HasPostAttr = action.GetCustomAttributes(typeof(HttpPostAttribute), false).Any();
                    act.Add(action.Name.ToString() + (HasPostAttr ? " (Post) " : ""));
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new SelectList(act.ToList()), JsonRequestBehavior.AllowGet);
        }
        

        [HttpGet]
        public ActionResult Manage()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.Lbl_LeaveType = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_DaysAllowed = CommonLogic.getLabelName(2).ToString();

            List<DSRCManagementSystem.Models.AddLeave> objmodel = new List<Models.AddLeave>();
            try
            {

                objmodel = (from p in db.LeaveTypes
                            select new DSRCManagementSystem.Models.AddLeave
                            {
                                LeaveTypeId = p.LeaveTypeId,
                                Name = p.Name,
                                DaysAllowed = p.DaysAllowed


                            }).ToList();


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }



            return View(objmodel);
           
        }

        //[HttpGet]
        //public ActionResult Add()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    ViewBag.Lbl_LeaveType = CommonLogic.getLabelName(1).ToString();
        //    ViewBag.Lbl_DaysAllowed = CommonLogic.getLabelName(2).ToString();

        //    try
        //    {
        //        var Name = db.Master_LeaveType.Select(c => new
        //        {
        //            LeaveTypeId = c.LeaveTypeId,
        //            Name = c.Name


        //        }).ToList();
        //        ViewBag.Name = new SelectList(Name, "LeaveTypeId", "Name");
        //    }
        //    catch (Exception Ex)
        //    {
        //        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
        //        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

        //    }
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Add(AddLeave model)
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    Session["Tab"] = "";
        //    try
        //    {
        //        var DaysAllowed = db.Master_LeaveType.Select(f => f.DaysAllowed);
        //        foreach (var day in DaysAllowed)
        //        {
        //            if (day != null)
        //            {


        //                return Json("success", JsonRequestBehavior.AllowGet);
        //            }

        //        }

        //        {
        //            var Assignobj = db.Master_LeaveType.CreateObject();
        //            db.Master_LeaveType.AddObject(Assignobj);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
        //        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

        //    }
        //    return Json("Success1", JsonRequestBehavior.AllowGet);

        //}
    
       [HttpGet]
       public ActionResult  Edit(string Id)
       {

           int leavetypeid = Convert.ToInt32(Id);
           DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           DSRCManagementSystem.Models.AddLeave objmodel = new DSRCManagementSystem.Models.AddLeave();
           var value = db.LeaveTypes.Where(x => x.LeaveTypeId == leavetypeid).Select(o => o).FirstOrDefault();
           objmodel.Name = value.Name;
           objmodel.DaysAllowed = value.DaysAllowed;
           objmodel.LeaveTypeId = leavetypeid;

           return View(objmodel);
       }
       [HttpPost]

       public ActionResult Edit(string LeaveTypeId, string DaysAllowed)
       {
           DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           int id = Convert.ToInt32(LeaveTypeId);
           var value = db.LeaveTypes.Where(x => x.LeaveTypeId == id).Select(o => o).FirstOrDefault();
           value.DaysAllowed = Convert.ToInt32(DaysAllowed);
           db.SaveChanges();
           return Json(new { Result = "Success"}, JsonRequestBehavior.AllowGet);
       }


    }
}
