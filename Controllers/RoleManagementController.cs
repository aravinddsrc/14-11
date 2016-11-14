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
    public class RoleManagementController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult RolesandPermissions()
        {
            AdministrationSetup ObjAS = new AdministrationSetup();
            ObjAS.RoleList = GetRoles();
            ObjAS.Menu = GetMenu();
            ObjAS.MenuList = GetMenuList();
            return View(ObjAS);
        }

        [HttpPost]
        public ActionResult RolesandPermissions(AdministrationSetup ADMSetUp)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
               // var CompExist = db.Assets.Where(a => a.Name_Model_No == Component).Select(a => a.AssetID).FirstOrDefault();
               // var rolename = db.Roles.Where(x=>x.RoleName == ADMSetUp.RoleName).Select(x=>x.RoleID).FirstOrDefault();
                //if(rolename >0)
              
                if (db.Master_Roles.Any(R => R.RoleName == ADMSetUp.RoleName && R.IsActive == true))
                {
                    ModelState.AddModelError("RoleName", "Role Name Already Exists");
                   
                }
                
                else if ( ADMSetUp.RoleName == null )
                {
                    ModelState.AddModelError("RoleName", "Enter Role Name");

                }

                else if (ADMSetUp.RoleName.Trim().Count() == 0)
                {
                    ModelState.AddModelError("RoleName", "Enter Role Name");

                }
                    
                else
                {

                /**** First Add Roles into Roles Table ****/
                var AddRoleObj = db.Master_Roles.CreateObject();
                AddRoleObj.RoleName = ADMSetUp.RoleName.Trim();
                AddRoleObj.IsActive = true;
                db.Master_Roles.AddObject(AddRoleObj);
                db.SaveChanges();


                ADMSetUp.RoleID = AddRoleObj.RoleID;

                List<string> SelectedFunId = ADMSetUp.selectedFunctionID;
                List<string> SelectedPageModuleID = ADMSetUp.selectedPageModuleID;
                List<string> SelectedPreceedenceOrder = ADMSetUp.selectedPreceedenceorder;


                //if (ADMSetUp.RoleID != 0 && SelectedFunId != null && SelectedPageModuleID != null)
                if (ADMSetUp.RoleID != 0 && SelectedFunId != null)
                {
                    ADMSetUp.PrecedanceOrder = 1;

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

                    return Redirect("/ManageRole/AssignNewRole?RoleId=" + ADMSetUp.RoleID);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            ADMSetUp.RoleList = GetRoles();
            ADMSetUp.Menu = GetMenu();
            ADMSetUp.MenuList = GetMenuList();
            return View(ADMSetUp);
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
            catch (Exception ex)
            {
                throw ex;
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
            return tmp;
        }

        public static List<DSRCManagementSystem.Models.Menu> GetMenu()
        {
            List<DSRCManagementSystem.Models.Menu> menu = new List<DSRCManagementSystem.Models.Menu>();
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
            return menu;
        }

        public static List<MenuList> GetMenuList()
        {
            List<DSRCManagementSystem.Models.MenuList> menulist = new List<DSRCManagementSystem.Models.MenuList>();
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
            return menulist;
        }

        [HttpGet]
        public ActionResult GetMenuForRole(AdministrationSetup ADMSetUp)
        {
          
            List<DSRCManagementSystem.Models.RoleFunctionPrivilages> RoleFunctionPrivilages = new List<DSRCManagementSystem.Models.RoleFunctionPrivilages>();
            foreach(int ID in Enum.GetValues(typeof(MasterEnum.DefaultFunctionId)))
            {
                var PMID = db.FunctionModules.Where(x => x.FunctionID == ID).Select(o => o.PageModuleID).ToList();
                if (PMID.Count() != 0)
                {
                    foreach (var y in PMID)
                    {
                        RoleFunctionPrivilages.Add(new RoleFunctionPrivilages() { FunctionId = Convert.ToByte(ID), PageModuleId = Convert.ToByte(y) });
                    }
                }
                else
                {
                    RoleFunctionPrivilages.Add(new RoleFunctionPrivilages() { FunctionId = Convert.ToByte(ID) });
                }
                
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


            return View();
        }

        [HttpPost]
        public ActionResult AddNew(Email objmail)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.EmailPurpose objpurpose = new DSRCManagementSystem.EmailPurpose();
            var already = db.EmailPurposes.Where(x => x.EmailPurposeName == objmail.Purpose && x.IsActive == true ).Select(x => x).FirstOrDefault();
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




                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult AddNewEdit(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.Email obj = new DSRCManagementSystem.Models.Email();

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


            return View(obj);
        }
    
        [HttpPost]
        public ActionResult AddNewEdit(Email objmail)
        {
            int UserId = int.Parse(Session["UserID"].ToString());

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.EmailPurposeLog obj = new DSRCManagementSystem.EmailPurposeLog();

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
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteEmail(int Id)
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
            return View(projectData);
        }

        [HttpPost]
        public ActionResult ViewAllProjects(FormCollection form)
        {
            List<Projects> projectData = new List<Projects>();

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


        [HttpPost]
        public ActionResult Reset()
        {
            return View();
        }
    }
}


    


    

