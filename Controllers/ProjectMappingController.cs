//Controller Name:ProjectMappingController
//Purpose        :Assign And View Project Members
//Date Created   :20-02-2015
//Created By     :Balaji.S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;
using System.Web.Configuration;
using System.Threading.Tasks;
using System.Data.Objects.SqlClient;

namespace DSRCManagementSystem.Controllers
{

    //[DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Recruitment Specialist-RMG, Tech Lead,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer,Head - Quality,")]
    public class ProjectMappingController : Controller
    {
        //
        // GET: /ProjectMapping/

        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 


        public ActionResult AssignProject()
        {
            ProjectMapping ObjPM = new ProjectMapping();
            ObjPM.EmployeeList = GetNames();
            ObjPM.ProjectList = GetProjects();
            ObjPM.RoleList = GetRoles();
            //ObjPM.Members = GetMembers();
            return View(ObjPM);
        }
        private List<SelectListItem> GetRoles()
        {
            try
            {
                var NameList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    /*List<Roles> Names = (from data in db.Roles
                                         where data.IsActive == true
                                         //&&
                                         //data.RoleID != 3 && data.RoleID != 11
                                         //&& data.RoleID != 12
                                         select new Roles
                                         {
                                             RoleId = data.RoleID,
                                             RoleName = data.RoleName,
                                         }).ToList();*/
                    //List<Master_MemberTypes> Names = (from data in db.Master_MemberTypes
                    //                           select new Master_MemberTypes
                    //                           {
                    //                               MemberTypeID = data.MemberTypeID,
                    //                               MemberType = data.MemberType,
                    //                           }).ToList();

                    var Names = (from data in db.Master_MemberTypes
                                                      select new                                                       {
                                                          MemberTypeID = data.MemberTypeID,
                                                          MemberType = data.MemberType,
                                                      }).ToList();

                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.MemberType, Value = item.MemberTypeID.ToString() });
                    }
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
        private List<SelectListItem> GetProjects()
        {
            try
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                var ProjectList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<DSRCProjects> projects = (from data in db.Projects
                                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectID
                                                   where data.IsActive == true
                                                   select new DSRCProjects
                                                   {
                                                       ProjectId = data.ProjectID,
                                                       ProjectName = data.ProjectName
                                                   }).OrderBy(x => x.ProjectName).ToList();
                    foreach (var item in projects)
                    {
                        ProjectList.Add(new SelectListItem { Text = item.ProjectName, Value = item.ProjectId.ToString() });
                    }
                }

                return ProjectList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }
        private List<SelectListItem> GetNames()
        {
            try
            {
                var NameList = new List<SelectListItem>();
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    var list = db.Users.ToList();

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 where data.IsActive == true && data.BranchId==1&&data.UserStatus!=6
                                                 select new DSRCEmployees
                                                 {
                                                     Name = (data.FirstName + " " + data.LastName)??"",
                                                     UserId = data.UserID,
                                                     EmployeeId = data.EmpID
                                                 }).Where(x => x.Name != null && x.Name != "").OrderBy(x => x.Name).ToList();
                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.Name, Value = item.UserId.ToString() });
                    }

                    //NameList.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
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
        [HttpPost]
        public ActionResult AssignProject(string Value)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var json_serializer = new JavaScriptSerializer();
                AssignedMembers memberObj = json_serializer.Deserialize<AssignedMembers>(Value);
                List<string> newMembers = new List<string>(memberObj.UserId);
                //List<string> splitedNewMembers = new List<string>();
                //foreach (var item in newMembers)
                //{
                //    splitedNewMembers.Add(item.Split('+')[0]);
                //}
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    //List<int> oldMembersList = db.UserProjects.Where(x => x.ProjectID == memberObj.ProjectId).Select(x => x.UserID).ToList();
                    //List<int> newMembersList = splitedNewMembers.ConvertAll(s => Int32.Parse(s));
                    
                    List<UserProject> oldMembersList = db.UserProjects.Where(x => x.ProjectID == memberObj.ProjectId).ToList();                                    

                    List<string> UnsplitedOldMembers = new List<string>();

                    foreach (var item in oldMembersList)
                    {
                        UnsplitedOldMembers.Add(Convert.ToString(item.UserID) + "+" + Convert.ToString(item.MemberTypeID));
                    }

                    //var toInsert = newMembersList.Except(oldMembersList).ToList();
                    //var toDelete = oldMembersList.Except(newMembersList).ToList();

                    var toInsert = newMembers.Except(UnsplitedOldMembers).ToList();
                    var toDelete = UnsplitedOldMembers.Except(newMembers).ToList();

                    if (memberObj.UserId != null && memberObj.ProjectId != 0)
                    {
                        //StringBuilder SB = new StringBuilder();
                        if (toInsert.Count > 0)
                        {
                            for (int i = 0; i < memberObj.UserId.Count; i++)
                            {
                                string[] RoleId = memberObj.UserId[i].Split('+');
                                UserProject UP = new UserProject();
                                UP.ProjectID = memberObj.ProjectId;
                                UP.UserStartDate = memberObj.StartDateTime;
                                UP.UserEndDate = memberObj.EndDateTime;
                                UP.UserID = Convert.ToInt32(RoleId[0]);
                                //UP.RoleID = Convert.ToByte(RoleId[1]);
                                UP.MemberTypeID = Convert.ToByte(RoleId[1]);
                                if (UP.RoleID != 13)
                                    UP.IsBillable = true;
                                else
                                    UP.IsBillable = false;

                                var data = db.UserProjects.FirstOrDefault(x => (x.UserID == UP.UserID) && (x.ProjectID == UP.ProjectID) && (x.MemberTypeID == UP.MemberTypeID));

                                if (data == null)
                                {
                                    db.UserProjects.AddObject(UP);
                                    //db.SaveChanges();

                                    string MemberType = db.Master_MemberTypes.FirstOrDefault(o => o.MemberTypeID == UP.MemberTypeID).MemberType;
                                   
                                    var Project = db.Projects.FirstOrDefault(P => P.ProjectID == UP.ProjectID);
                                    var User = db.Users.FirstOrDefault(U => U.UserID == UP.UserID);

                                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Assign Project").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {

                         var objAssignProject = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project")
                                                 join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                 select new DSRCManagementSystem.Models.Email
                                                 {
                                                     To = p.To,
                                                     CC = p.CC,
                                                     BCC = p.BCC,
                                                     Subject = p.Subject,
                                                     Template = q.TemplatePath
                                                 }).FirstOrDefault();

                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathAssignProject = Server.MapPath(objAssignProject.Template);
                         string htmlAssignProject = System.IO.File.ReadAllText(TemplatePathAssignProject);
                         htmlAssignProject = htmlAssignProject.Replace("#MemberType", MemberType);
                         htmlAssignProject = htmlAssignProject.Replace("#EmployeeID", User.EmpID);
                         htmlAssignProject = htmlAssignProject.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                         htmlAssignProject = htmlAssignProject.Replace("#ProjectName", Project.ProjectName);
                         htmlAssignProject = htmlAssignProject.Replace("#ServerName", ServerName);
                         htmlAssignProject = htmlAssignProject.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlAssignProject = htmlAssignProject.Replace("#CompanyName", company);
                         var ToEmail = (from p in db.UserProjects
                                        join u in db.Users
                                        on p.UserID equals u.UserID
                                        where p.ProjectID == UP.ProjectID && p.MemberTypeID == 1
                                        select new
                                        {
                                            mailaddress = u.EmailAddress,
                                            FirstName = u.FirstName,
                                            LastName = u.LastName
                                        }).ToList();

                         //List<string> ToEmailList = ToEmail.ConvertAll(s => s.mailaddress.ToString());
                         ToEmail.RemoveAll(o => o.mailaddress == null || o.mailaddress == "");
                         List<string> ToEmailList = ToEmail.Where(s => s.mailaddress != null).Select(o => (o.mailaddress).ToString()).ToList();

                         string objAssignProjectTo = string.Join(",", ToEmailList);
                         List<string> Name = ToEmail.Where(s => s.mailaddress != null).Select(o => (o.FirstName + " " + o.LastName ?? "")).ToList();
                         var ManagedResources = String.Join(", ", Name.ToArray());
                         htmlAssignProject = htmlAssignProject.Replace("#ManagedResources", ManagedResources);

                         objAssignProject.CC = ProjectMappingController.GetUserEmailAddress(db, objAssignProject.CC);

                         if (objAssignProject.CC != null && objAssignProject.CC != "")
                             objAssignProject.CC += ",";

                         byte MemTypeId = 0;
                         int Userid = 0;
                         string Emailaddress = null;

                         for (int j = 0; j < memberObj.UserId.Count; j++)
                         {
                             string[] RoleIds = memberObj.UserId[j].Split('+');

                             Userid = Convert.ToInt32(RoleIds[0]);
                             MemTypeId = Convert.ToByte(RoleIds[1]);

                             if (MemTypeId == 4 || MemTypeId == 5)
                             {
                                 Emailaddress = db.Users.FirstOrDefault(o => o.UserID == Userid).EmailAddress ?? "";

                                 if (Emailaddress != "")
                                     objAssignProject.CC += Emailaddress + ",";
                             }
                         }
                         objAssignProject.CC.TrimEnd(',');

                         if (objAssignProject.BCC != "")
                         {
                             objAssignProject.BCC = ProjectMappingController.GetUserEmailAddress(db, objAssignProject.BCC);
                         }

                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //List<string> mails = new List<string>();
                             //for (int k = 0;k < MailIds.Count(); k++)
                             //{
                             //    mails.Add(MailIds[k]);
                             //}


                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 //for (int k = 0; k <= MailIds.Count; k++)
                                 // {
                                 EmailAddress += mail + ",";
                                 // MailIds[k] = EmailAddress;
                                 //}
                             }
                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in";
                             string BCCMailId = "virupaksha.gaddad@dsrc.co.in";
                             //string CCMailId = "";
                             //string BCCMailId = "";


                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();



                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject + " - Test Mail Please Ignore", "", htmlAssignProject + " - Testing Please ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();
                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 //string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject, "", htmlAssignProject, "HRMS@dsrc.co.in", objAssignProjectTo, objAssignProject.CC, objAssignProject.BCC, Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Assign Project", folder, ServerName);
                     }


                                }


                            }
                        }
                        if (toDelete.Count > 0)
                        {
                            int UserId;
                            foreach (var item in toDelete)
                            {
                                UserId = Convert.ToInt32(item.Split('+')[0]);
                                var record = db.UserProjects.FirstOrDefault(x => x.UserID == UserId && x.ProjectID == memberObj.ProjectId);                                db.UserProjects.DeleteObject(record);

                                string MemberType = db.Master_MemberTypes.FirstOrDefault(o => o.MemberTypeID == record.MemberTypeID).MemberType;
                               
                                var Project = db.Projects.FirstOrDefault(P => P.ProjectID == record.ProjectID);
                                var User = db.Users.FirstOrDefault(U => U.UserID == record.UserID);

                                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project Delete").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Assign Project Delete").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objAssignProjectDelete = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project Delete")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathAssignProject = Server.MapPath(objAssignProjectDelete.Template);
                         string htmlAssignProjectDelete = System.IO.File.ReadAllText(TemplatePathAssignProject);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#MemberType", MemberType);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeID", User.EmpID);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ProjectName", Project.ProjectName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ServerName",ServerName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#CompanyName", company);

                         var ToEmail = (from p in db.UserProjects
                                        join u in db.Users
                                        on p.UserID equals u.UserID
                                        where p.ProjectID == record.ProjectID && p.MemberTypeID == 1
                                        select new
                                        {
                                            u
                                        }).ToList();

                         ToEmail.RemoveAll(o => o.u.EmailAddress == null || o.u.EmailAddress == "");
                         List<string> ToEmailList = ToEmail.ConvertAll(s => s.u.EmailAddress.ToString());
                         string objAssignProjectDeleteTo = string.Join(",", ToEmailList);
                         //List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + s.u.LastName.ToString());
                         List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + (s.u.LastName ?? ""));
                         var ManagedResources = String.Join(", ", Name.ToArray());
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ManagedResources", ManagedResources);

                         objAssignProjectDelete.CC = ProjectMappingController.GetUserEmailAddress(db, objAssignProjectDelete.CC);

                         if (objAssignProjectDelete.CC != null && objAssignProjectDelete.CC != "")
                             objAssignProjectDelete.CC += ",";

                         byte MemTypeId = 0;
                         int Userid = 0;
                         string Emailaddress = null;

                         for (int j = 0; j < memberObj.UserId.Count; j++)
                         {
                             string[] RoleIds = memberObj.UserId[j].Split('+');

                             Userid = Convert.ToInt32(RoleIds[0]);
                             MemTypeId = Convert.ToByte(RoleIds[1]);

                             if (MemTypeId == 4 || MemTypeId == 5)
                             {
                                 Emailaddress = db.Users.FirstOrDefault(o => o.UserID == Userid).EmailAddress ?? "";

                                 if (Emailaddress != "")
                                     objAssignProjectDelete.CC += Emailaddress + ",";
                             }
                         }
                         objAssignProjectDelete.CC.TrimEnd(',');

                         if (objAssignProjectDelete.BCC != "")
                         {
                             objAssignProjectDelete.BCC = ProjectMappingController.GetUserEmailAddress(db, objAssignProjectDelete.BCC);
                         }

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("gopika.v@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in ";
                             string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();


                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject + " - Test Mail Please Ignore", null, htmlAssignProjectDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject, "", htmlAssignProjectDelete, "HRMS@dsrc.co.in", objAssignProjectDeleteTo, objAssignProjectDelete.CC, objAssignProjectDelete.BCC, Server.MapPath(pathvalue));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Assign Project Delete", folder, ServerName);
                     }
                            }
                        }

                        db.SaveChanges();
                        return Json(true);
                    }
                    return Json(false);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw new Exception(Ex.Message);
            }
        }

        private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.EmailAddress).ToList();
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



        public ActionResult ViewProjectMappings()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName");
            ProjectMapping ObjPM = new ProjectMapping();
            ObjPM.Members = GetMembers();

            //List<int> objvalue = new List<int>();
            //for (int i = 2; i <=3; i++)
            //{
            //    objvalue.Add(i);
            //}


            var MemberTypeList = (from p in db.Master_MemberTypes
                                  select new
                                  {
                                      MemberTypeID = p.MemberTypeID,
                                      MemberType = p.MemberType
                                  }).ToList();

            ViewBag.Members = new MultiSelectList(LoadMembers(), "TypeId", "MemberTypes");

            //ObjPM.AdditionalBufferResources = true;
            //ObjPM.ManagedResources = false;
            //ObjPM.BillableResources = true;
            return View(ObjPM);
        }


        private List<Master_Types> LoadMembers()
        {
            List<Master_Types> Members = new List<Master_Types>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                Members = (from data in db.Master_Types
                           //join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectI
                           select data).ToList();
            }
            return Members;
        }


        [HttpPost]        
        public ActionResult ViewProjectMappings(FormCollection form)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string projectID = (form["ProjectList"] == "") ? "0" : form["ProjectList"].ToString();
            int PId = Convert.ToInt32(projectID);
            string Member=null;

            if (form["MembersList"] == null)
            {
                Member = null;
            }
            else if ((form["MembersList"].ToString() != null))
            {
               Member = (form["MembersList"] == "") ? "0" : form["MembersList"].ToString();
            }
            ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName", projectID);
            ProjectMapping ObjPM = new ProjectMapping();
            List<int> objuser = new List<int>();
           
        

            //for (int j = 0; j < objuser.Count(); j++)
            //{
            //    ObjPM.MemberTypeList.Add(Convert.ToInt32(objuser[j]));
            //}

            //ProjectMapping ObjPM = new ProjectMapping();
            
            //ObjPM.Members = GetMembers(PId, ObjPM.ManagedResources, ObjPM.BillableResources, ObjPM.AdditionalBufferResources);

            if (Member != null)
            {
                string[] values = Member.Split(',');

                for (int i = 0; i < values.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(values[i]));
                }

            }

            if (Member == null)
            {
                objuser = null;
            }

            if (objuser == null)
               objuser = db.Master_MemberTypes.Where(o => o.MemberTypeID == 2 || o.MemberTypeID == 3).Select(o => o.MemberTypeID).ToList();

            ObjPM.Members = GetMembers(objuser, PId);

            //var MemberTypeList = (from p in db.MemberTypes
            //                      select new
            //                      {
            //                          MemberTypeID = p.MemberTypeID,
            //                          MemberType = p.MemberType1
            //                      }).ToList();

            ViewBag.Members = new MultiSelectList(LoadMembers(), "TypeId", "MemberTypes", objuser);

            return View(ObjPM);
        }
        private IList<ViewMembers> GetMembers(int PId)
        {           

            List<ViewMembers> Members = new List<ViewMembers>();
         
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {                
                if (PId == 0)
                {
                    {
                        Members = (from a in db.UserProjects
                                   join b in db.Users
                                   on a.UserID equals b.UserID
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID                                   
                                   join d in db.Master_MemberTypes
                                   on a.MemberTypeID equals d.MemberTypeID                                   
                                   where b.IsActive == true && c.IsActive==true
                                   select new ViewMembers()
                                   {
                                       UserProjectId = a.UserProjectID,
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,                                       
                                       //MemberType = d.MemberType1,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,    
                                          SelectedUserStatusid = b.UserStatus,
                                       IsUnderNoticePeriod = b.IsUnderNoticePeriod
                                   //}).OrderBy(x => x.EmployeeName).ToList();
                                   }).OrderBy(x => x.ProjectName).ToList();
                    }
                }
                else
                {
                    Members = (from a in db.UserProjects
                               join b in db.Users
                               on a.UserID equals b.UserID
                               join c in db.Projects
                               on a.ProjectID equals c.ProjectID
                               join d in db.Master_MemberTypes
                               on a.MemberTypeID equals d.MemberTypeID                               
                               where a.ProjectID == PId && b.IsActive == true && c.IsActive == true

                               select new ViewMembers()
                               {
                                   UserProjectId = a.UserProjectID,
                                   EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                   UserId = b.UserID,
                                   ProjectName = c.ProjectName,
                                   ProjectId = c.ProjectID,                                   
                                   //MemberType = d.MemberType1,
                                   MemberType = d.MemberType,
                                   MemberTypeID = d.MemberTypeID,       
                                     
                                   IsUnderNoticePeriod = b.IsUnderNoticePeriod
                              // }).OrderBy(x => x.EmployeeName).ToList();
                               }).OrderBy(x => x.ProjectName).ToList();

                }
            }
            return Members;
        }

        private IList<ViewMembers> GetMembers(List<int> MemberTypeList, int PId = 0)
        {
            DSRCManagementSystemEntities1 db1 = new DSRCManagementSystemEntities1();

            List<int> objType = new List<int>();
            //if (ManagedResources) { objType.Add(1); }
            //if (BillableResources) { objType.Add(2); }
            //if (AdditionalBufferResources) { objType.Add(3); }

            if (MemberTypeList == null)
                MemberTypeList = db1.Master_MemberTypes.Select(o => o.MemberTypeID).ToList();
           
            foreach (int MemberTypeId in MemberTypeList)
            {
                objType.Add(MemberTypeId);
            }

            List<ViewMembers> Members = new List<ViewMembers>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                if (PId == 0)
                {
                    {
                        Members = (from a in db.UserProjects
                                   join b in db.Users
                                   on a.UserID equals b.UserID
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID
                                   join d in db.Master_MemberTypes
                                   on a.MemberTypeID equals d.MemberTypeID
                                   where b.IsActive == true && objType.Contains(d.MemberTypeID) && c.IsActive == true
                                   select new ViewMembers()
                                   {
                                       UserProjectId = a.UserProjectID,
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,
                                          SelectedUserStatusid = b.UserStatus,
                                       IsUnderNoticePeriod = b.IsUnderNoticePeriod
                                   //}).OrderBy(x => x.EmployeeName).ToList();
                                   }).OrderBy(x => x.ProjectName).ToList();
                    }
                }
                else
                {
                    Members = (from a in db.UserProjects
                               join b in db.Users
                               on a.UserID equals b.UserID
                               join c in db.Projects
                               on a.ProjectID equals c.ProjectID
                               join d in db.Master_MemberTypes
                               on a.MemberTypeID equals d.MemberTypeID
                               where a.ProjectID == PId && b.IsActive == true && objType.Contains(d.MemberTypeID) && c.IsActive ==true

                               select new ViewMembers()
                               {
                                   UserProjectId = a.UserProjectID,
                                   EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                   UserId = b.UserID,
                                   ProjectName = c.ProjectName,
                                   ProjectId = c.ProjectID,
                                   MemberType = d.MemberType,
                                   MemberTypeID = d.MemberTypeID,
                                   SelectedUserStatusid = b.UserStatus,
                                   IsUnderNoticePeriod = b.IsUnderNoticePeriod
                               //}).OrderBy(x => x.EmployeeName).ToList();
                               }).OrderBy(x => x.ProjectName).ToList();
                }
            }
            return Members;
        }

        private IList<ViewMembers> GetMembers(int PId = 0, bool ManagedResources = false, bool BillableResources = true, bool AdditionalBufferResources = true, bool AccountManager = true, bool QA = true, bool TeamLead = true, bool Marketing=true)
        {

            List<int> objType = new List<int>();
            if (ManagedResources) { objType.Add(1); }
            if (BillableResources) { objType.Add(2); }
            if (AdditionalBufferResources) { objType.Add(3); }
            if(AccountManager) {objType.Add(4) ;}
            if(QA) {objType.Add(5) ;}
            if(TeamLead) {objType.Add(6) ;}
            if (Marketing) { objType.Add(7); }
            List<ViewMembers> Members = new List<ViewMembers>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                if (PId == 0)
                {
                    {
                        Members = (from a in db.UserProjects
                                   join b in db.Users
                                   on a.UserID equals b.UserID
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID
                                   /*join d in db.Rolesf
                                   on a.RoleID equals d.RoleID*/
                                   join d in db.Master_MemberTypes
                                   on a.MemberTypeID equals d.MemberTypeID
                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                                   where b.IsActive == true&&b.UserStatus!=6 && objType.Contains(d.MemberTypeID) && c.IsActive==true
                                   select new ViewMembers()
                                   {
                                       UserProjectId=a.UserProjectID,
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,
                                       //RoleName = d.RoleName,
                                       //RoleId = d.RoleID,
                                       //MemberType = d.MemberType1,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,
                                       //IsBillable = a.IsBillable ?? false,
                                       SelectedUserStatusid = b.UserStatus
                                   //}).OrderBy(x => x.EmployeeName).ToList();
                                   }).OrderBy(x => x.ProjectName).ToList();
                    }
                }
                else
                {
                    Members = (from a in db.UserProjects
                               join b in db.Users
                               on a.UserID equals b.UserID
                               join c in db.Projects
                               on a.ProjectID equals c.ProjectID
                               /*join d in db.Roles
                               on a.RoleID equals d.RoleID*/
                               join d in db.Master_MemberTypes
                               on a.MemberTypeID equals d.MemberTypeID
                               //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                               where a.ProjectID == PId && b.IsActive == true && objType.Contains(d.MemberTypeID) && c.IsActive==true 

                               select new ViewMembers()
                               {
                                   UserProjectId = a.UserProjectID,
                                   EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                   UserId = b.UserID,
                                   ProjectName = c.ProjectName,
                                   ProjectId = c.ProjectID,
                                   //RoleName = d.RoleName,
                                   //RoleId = d.RoleID,
                                   MemberType = d.MemberType,
                                   MemberTypeID = d.MemberTypeID,
                                  // IsBillable = a.IsBillable ?? false,
                                   IsUnderNoticePeriod = b.IsUnderNoticePeriod
                               //}).OrderBy(x => x.EmployeeName).ToList();
                               }).OrderBy(x => x.ProjectName).ToList();

                }
            }
            return Members;
        }
        private List<Project> LoadProjects()
        {
            List<Project> Projects = new List<Project>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                Projects = (from data in db.Projects
                            //join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectID
                            where data.IsActive == true
                            select data).OrderBy(x => x.ProjectName).ToList();
            }
            return Projects;
        }
        private List<Department> LoadDepartments()
        {
            List<Department> Departments = new List<Department>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Departments = (from data in db.Departments.Where(x=>x.IsActive == true)
                               select data).OrderBy(x => x.DepartmentName).ToList();
            }
            return Departments;
        }
        public ActionResult DeleteUser(int userID, int memberTypeID, int projectId)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var record = db.UserProjects.FirstOrDefault(x => x.UserID == userID && /*x.RoleID == roleID*/x.MemberTypeID == memberTypeID && x.ProjectID == projectId);
                if (record != null)
                {
                    db.UserProjects.DeleteObject(record);
                    db.SaveChanges();
                    string MemberType = string.Empty;
                    if (record.MemberTypeID == 1)
                    {
                        MemberType = "Managed Resources";
                    }
                    else if (record.MemberTypeID == 2)
                    {
                        MemberType = "Billable Resources";
                    }
                    else
                    {
                        MemberType = "Additional/Buffer Resources";
                    }
                    var Project = db.Projects.FirstOrDefault(P => P.ProjectID == record.ProjectID);
                    var User = db.Users.FirstOrDefault(U => U.UserID == record.UserID);

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project Delete").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Assign Project Delete").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objAssignProjectDelete = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project Delete")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathAssignProject = Server.MapPath(objAssignProjectDelete.Template);
                         string htmlAssignProjectDelete = System.IO.File.ReadAllText(TemplatePathAssignProject);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#MemberType", MemberType);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeID", User.EmpID);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ProjectName", Project.ProjectName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ServerName",ServerName);
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#CompanyName", company);

                         var ToEmail = (from p in db.UserProjects
                                        join u in db.Users
                                        on p.UserID equals u.UserID
                                        where p.ProjectID == record.ProjectID && p.MemberTypeID == 1
                                        select new
                                        {
                                            u
                                        }).ToList();

                         ToEmail.RemoveAll(o => o.u.EmailAddress == null || o.u.EmailAddress == "");
                         List<string> ToEmailList = ToEmail.ConvertAll(s => s.u.EmailAddress.ToString());
                         string objAssignProjectDeleteTo = string.Join(",", ToEmailList);
                         //List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + s.u.LastName.ToString());
                         List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + (s.u.LastName ?? ""));
                         var ManagedResources = String.Join(", ", Name.ToArray());
                         htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ManagedResources", ManagedResources);

                         objAssignProjectDelete.CC = ProjectMappingController.GetUserEmailAddress(db, objAssignProjectDelete.CC);
                         if (objAssignProjectDelete.BCC != "")
                         {
                             objAssignProjectDelete.BCC = ProjectMappingController.GetUserEmailAddress(db, objAssignProjectDelete.BCC);
                         }

                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in ";
                             string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject + " - Test Mail Please Ignore", null, htmlAssignProjectDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();


                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject, "", htmlAssignProjectDelete, "HRMS@dsrc.co.in", objAssignProjectDeleteTo, objAssignProjectDelete.CC, objAssignProjectDelete.BCC, Server.MapPath(pathvalue));
                             });
                         }
                     }
                     else
                     {
                       //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Assign Project Delete", folder, ServerName);
                     }

                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddUser(int ProjectID = 0)
        {
            ProjectMapping ObjProjectMapping = new ProjectMapping();
            ObjProjectMapping.ProjectList = GetProjects();
            if (ProjectID != 0)
            {
                ObjProjectMapping.ProjectName = ProjectID.ToString();
                ViewBag.ProjectName = ObjProjectMapping.ProjectList.Where(x => x.Value == ProjectID.ToString()).Select(x => x.Text).FirstOrDefault();
            }

            ObjProjectMapping.EmployeeList = GetNames();
            ObjProjectMapping.RoleList = GetRoles();
            return View(ObjProjectMapping);
        }
        [HttpPost]
        public ActionResult AddUser(ProjectMapping Collection)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                StringBuilder stringBuilder = new StringBuilder();
                UserProject ObjUserProject = new UserProject();
                ObjUserProject.ProjectID = Convert.ToInt32(Collection.ProjectName);
                ObjUserProject.UserID = Convert.ToInt32(Collection.EmployeeName);
                //ObjUserProject.RoleID = Convert.ToByte(Collection.RoleName);
                ObjUserProject.MemberTypeID = Convert.ToByte(Collection.MemberTypeID);
                //ObjUserProject.IsBillable = Collection.IsBillableResource;
                //var record = db.UserProjects.Where(x => (x.UserID == ObjUserProject.UserID) && (x.ProjectID == ObjUserProject.ProjectID)).ToList().Count;
                var record = db.UserProjects.FirstOrDefault(x => (x.UserID == ObjUserProject.UserID) && (x.ProjectID == ObjUserProject.ProjectID) && (x.MemberTypeID==ObjUserProject.MemberTypeID));
                if (record == null)
                {
                    db.UserProjects.AddObject(ObjUserProject);
                    db.SaveChanges();
                    string MemberType = string.Empty;
                    if (ObjUserProject.MemberTypeID == 1)
                    {
                        MemberType = "Managed Resources";
                    }
                    else if (ObjUserProject.MemberTypeID == 2)
                    {
                        MemberType = "Billable Resources";
                    }
                    else
                    {
                        MemberType = "Additional/Buffer Resources";
                    }
                    var Project = db.Projects.FirstOrDefault(P => P.ProjectID == ObjUserProject.ProjectID);
                    var User = db.Users.FirstOrDefault(U => U.UserID == ObjUserProject.UserID);

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Assign Project").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objAssignProject = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project")
                                                 join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                 select new DSRCManagementSystem.Models.Email
                                                 {
                                                     To = p.To,
                                                     CC = p.CC,
                                                     BCC = p.BCC,
                                                     Subject = p.Subject,
                                                     Template = q.TemplatePath
                                                 }).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathAssignProject = Server.MapPath(objAssignProject.Template);
                         string htmlAssignProject = System.IO.File.ReadAllText(TemplatePathAssignProject);
                         htmlAssignProject = htmlAssignProject.Replace("#MemberType", MemberType);
                         htmlAssignProject = htmlAssignProject.Replace("#EmployeeID", User.EmpID);
                         htmlAssignProject = htmlAssignProject.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                         htmlAssignProject = htmlAssignProject.Replace("#ProjectName", Project.ProjectName);
                         htmlAssignProject = htmlAssignProject.Replace("#ServerName",ServerName);
                         htmlAssignProject = htmlAssignProject.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlAssignProject = htmlAssignProject.Replace("#CompanyName", company);



                         var ToEmail = (from p in db.UserProjects
                                        join u in db.Users
                                        on p.UserID equals u.UserID
                                        where p.ProjectID == ObjUserProject.ProjectID && p.MemberTypeID == 1
                                        select new
                                        {
                                            u
                                        }).ToList();

                         ToEmail.RemoveAll(o => o.u.EmailAddress == null || o.u.EmailAddress == "");
                         List<string> ToEmailList = ToEmail.ConvertAll(s => s.u.EmailAddress.ToString());
                         string objAssignProjectTo = string.Join(",", ToEmailList);
                         //List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + s.u.LastName.ToString());
                         List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + " " + (s.u.LastName ?? ""));
                         var ManagedResources = String.Join(", ", Name.ToArray());
                         htmlAssignProject = htmlAssignProject.Replace("#ManagedResources", ManagedResources);

                         objAssignProject.CC = ProjectMappingController.GetUserEmailAddress(db, objAssignProject.CC);
                         if (objAssignProject.BCC != "")
                         {
                             objAssignProject.BCC = ProjectMappingController.GetUserEmailAddress(db, objAssignProject.BCC);
                         }

                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");                     
                             //MailIds.Add("kirankumar@dsrc.co.in");                        

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in ";
                             string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject + " - Test Mail Please Ignore", null, htmlAssignProject + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject, "", htmlAssignProject, "HRMS@dsrc.co.in", objAssignProjectTo, objAssignProject.CC, objAssignProject.BCC, Server.MapPath(pathvalue));
                             });
                         }
                     }
                     else
                     {
                       //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Assign Project", folder, ServerName);
                     }
                }
                else
                {
                    var existingMember = (record.User.FirstName + " " + record.User.LastName).Trim();
                    stringBuilder.Append(existingMember + ",");
                }
                string result = stringBuilder.ToString();
                return Json(result.TrimEnd(','));
                //return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewBufferResources()
        {
            ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName");
            List<int> selected = new List<int>() { 0 /*15*/ };
            ViewBag.Departments = new MultiSelectList(LoadDepartments(), "DepartmentId", "DepartmentName", selected);
            ProjectMapping objPM = new ProjectMapping();
            objPM.Members = GetBufferResources(DepartmentId: selected);
            objPM.IsBuffer = true;
            objPM.IsUnassigned = false;
            objPM.OnBoarding = false;
            return View(objPM);
        }

        [HttpPost]
        public ActionResult ViewBufferResources(FormCollection form)
        {
            string projectID = (form["ProjectList"] == "") ? "0" : form["ProjectList"].ToString();
            int PId = Convert.ToInt32(projectID);
            string deptID = (form["DepartmentList"] ?? "0");
            List<int> result = deptID.Split(',').Select(int.Parse).ToList();
            ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName", projectID);
            ViewBag.Departments = new MultiSelectList(LoadDepartments(), "DepartmentId", "DepartmentName", result);
            ProjectMapping ObjPM = new ProjectMapping();
            ObjPM.IsUnassigned = form["IsUnassigned"].Contains("true");
            ObjPM.IsBuffer = form["IsBuffer"].Contains("true");
            ObjPM.OnBoarding = form["OnBoarding"].Contains("true");
            ObjPM.Members = GetBufferResources(DepartmentId: result, pId: PId, Unassigned: ObjPM.IsUnassigned, Isbuffer: ObjPM.IsBuffer, onboarding: ObjPM.OnBoarding);

            return View(ObjPM);
        }
        private IList<ViewMembers> GetBufferResources(List<int> DepartmentId, int pId = 0, bool Isbuffer = true, bool Unassigned = false, bool onboarding = false)
        {
            //if (Isbuffer == false && Unassigned == false && onboarding == false)
            //{
            //    Isbuffer = true;
            //    Unassigned = true;
            //    onboarding = false;
            //}

            List<ViewMembers> Members = new List<ViewMembers>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                if (pId == 0)
                {

                    if (Isbuffer)
                    {

                        Members = (from a in db.UserProjects
                                   join b in db.Users.Where(x=>x.IsActive == true&&x.UserStatus!=6)
                                   on a.UserID equals b.UserID
                                   where b.IsActive == true
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID
                                   /*join d in db.Roles
                                    on a.RoleID equals d.RoleID*/
                                   join d in db.Master_MemberTypes.Where(x => x.MemberTypeID == 3)
                                   on a.MemberTypeID equals d.MemberTypeID
                                   // where b.IsUnderNoticePeriod == false
                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                                   //where d.RoleName == "Buffer Resource"
                                   select new ViewMembers()
                                   {
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,
                                       //RoleName = d.RoleName,
                                       //RoleId = d.RoleID,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,
                                       DepartmentId = b.DepartmentId ?? 0,
                                       DepartmentName = b.Department.DepartmentName,
                                       onboarding = b.IsBoarding,
                                       SelectedUserStatusid=b.UserStatus
                                   //}).OrderBy(x => x.EmployeeName).ToList();
                                   }).OrderBy(x => x.ProjectName).ToList();
                    }

                    if (Unassigned)
                    {
                        var ActiveProjects = (from proj in db.Projects
                                              where proj.IsActive == true
                                              select proj.ProjectID).ToList();

                        var withProject = (from user in db.UserProjects
                                           where ActiveProjects.Contains(user.ProjectID)
                                           select user.UserID).ToList();

                        //var withProject = (from user in db.UserProjects
                        //                   select user.UserID).ToList();

                        var withoutProject = (from a in db.Users
                                              where a.IsActive == true && a.UserStatus != 6
                                              select a.UserID).ToList();

                        var otherEmployees = withoutProject.Except(withProject).ToList();

                        foreach (int i in otherEmployees)
                        {
                            Members.Add((db.Users.Where(o => o.UserID == i).Select(o => new ViewMembers()
                            {
                                EmployeeName = (o.FirstName + " " + (o.LastName ?? "")).Trim(),
                                UserId = o.UserID,
                                ProjectName = "",
                                RoleName = "Unassigned",
                                MemberType = "Unassigned",
                                DepartmentId = o.DepartmentId ?? 0,
                                DepartmentName = o.Department.DepartmentName,
                                onboarding = o.IsBoarding,
                                SelectedUserStatusid = o.UserStatus
                            })).FirstOrDefault());

                        }
                    }
                }
                else
                {
                    if (Isbuffer)
                    {

                        Members = (from a in db.UserProjects
                                   join b in db.Users.Where(x=>x.IsActive==true&&x.UserStatus!=6)
                                   on a.UserID equals b.UserID
                                   //where a.IsBillable == false
                                   join c in db.Projects.Where(x => x.ProjectID == pId)
                                   on a.ProjectID equals c.ProjectID
                                   /*join d in db.Roles
                                    on a.RoleID equals d.RoleID*/
                                   join d in db.Master_MemberTypes.Where(x => x.MemberTypeID == 3)
                                   on a.MemberTypeID equals d.MemberTypeID
                                   // where b.IsUnderNoticePeriod == false
                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                                   //where a.ProjectID == pId && d.RoleName == "Buffer Resource"
                                   select new ViewMembers()
                                   {
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,
                                       //RoleName = d.RoleName,
                                       //RoleId = d.RoleID,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,
                                       DepartmentId = b.DepartmentId ?? 0,
                                       DepartmentName = b.Department.DepartmentName,
                                       onboarding = b.IsBoarding,
                                       IsUnderNoticePeriod = b.IsUnderNoticePeriod
                                   //}).OrderBy(x => x.EmployeeName).ToList();
                                   }).OrderBy(x => x.ProjectName).ToList();
                    }
                }
                if (onboarding)
                {
                    Members = Members.Where(o => o.onboarding != true).Select(o => o).ToList();
                    if (pId == 0)
                    {
                        var list = db.Users.Where(o => o.IsBoarding == true && o.IsActive == true && o.UserStatus != 6).Select(o => new ViewMembers()
                        {
                            EmployeeName = (o.FirstName + " " + (o.LastName ?? "")).Trim(),
                            UserId = o.UserID,
                            ProjectName = "",
                            RoleName = "On Boarding",
                            DepartmentId = o.DepartmentId ?? 0,
                            DepartmentName = o.Department.DepartmentName,
                            onboarding = o.IsBoarding,
                            IsUnderNoticePeriod = o.IsUnderNoticePeriod,
                            MemberType = "-",
                        }).ToList();
                        foreach (var item in list)
                            Members.Add(item);
                    }
                }
                else
                {
                    Members = Members.Where(o => o.onboarding != true).Select(o => o).ToList();
                }
            }

            //compare with department id

            if (DepartmentId[0] != 0)
                Members = Members.Where(o => DepartmentId.Contains(o.DepartmentId)).Select(o => o).ToList();

            //if (onboarding == true)
            //    if (Unassigned == true || Isbuffer == true)
            //        Members = Members.Select(o => o).ToList();
            //    else

            //        Members = GetBufferResources(DepartmentId, pId, true, true, true).Where(o => o.onboarding == true).ToList();
            //else
            //    Members = Members.Where(o => o.onboarding != true).Select(o => o).ToList();

            //foreach (var item in Members.Where(o => o.onboarding == true))
            //{
            //    item.RoleName = " On Boarding";
            //}

            return Members;

        }
        public ActionResult EditUser(string empName, string projectName, int userID, int memberTypeID, int projectId, int UserProjectId)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //var record = db.UserProjects.FirstOrDefault(x => x.UserID == userID && x.MemberTypeID == memberTypeID && x.ProjectID == projectId);
                var record = db.UserProjects.FirstOrDefault(x => x.UserProjectID == UserProjectId);
                EditUser objEditUser = new EditUser();
                objEditUser.EmployeeName = empName;
                objEditUser.ProjectName = projectName;
                objEditUser.UserId = record.UserID;
                objEditUser.ProjectId = record.ProjectID;
                objEditUser.RoleList = GetRoles();
                objEditUser.MemberTypeID = memberTypeID;
                objEditUser.UserProjectId = record.UserProjectID;
                //objEditUser.IsBillableResource = (record.IsBillable ?? false);
                return View(objEditUser);
            }
        }
        [HttpPost]
        public ActionResult EditUser(EditUser Collection)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //var record = db.UserProjects.FirstOrDefault(x => (x.UserID == Collection.UserId) && (x.ProjectID == Collection.ProjectId));               

                var ExistingMemberTypes = db.UserProjects.Where(x => (x.UserID == Collection.UserId) && (x.ProjectID == Collection.ProjectId)).ToList();
                bool alreadyMemberType = ExistingMemberTypes.Any(o => o.MemberTypeID == Collection.MemberTypeID);

                if (alreadyMemberType)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }

                var record = db.UserProjects.FirstOrDefault(x => x.UserProjectID == Collection.UserProjectId);
                var oldmembertypeobj = record.MemberTypeID;

                if (record != null)
                {
                    //record.RoleID = Convert.ToByte(Collection.RoleId);
                    record.MemberTypeID = Convert.ToByte(Collection.MemberTypeID);
                    //record.IsBillable = Collection.IsBillableResource;
                    db.SaveChanges();
                }

                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "On Change Resource").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "On Change Resource").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objProjectSummary = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "On Change Resource")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();

                         var newresourcetype = db.Master_MemberTypes.Where(o => o.MemberTypeID == record.MemberTypeID).Select(o => o.MemberType).FirstOrDefault();
                         var oldresourcetype = db.Master_MemberTypes.Where(o => o.MemberTypeID == oldmembertypeobj).Select(o => o.MemberType).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathProjectSummary = Server.MapPath(objProjectSummary.Template);
                         string htmlnewjoiningOnBoarding = System.IO.File.ReadAllText(TemplatePathProjectSummary);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#EmployeeId", record.User.EmpID);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#ProjectName", record.Project.ProjectName);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#ProjectCode", record.Project.ProjectCode);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#EmployeeName", record.User.FirstName + record.User.LastName);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#OldResourceType", oldresourcetype);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#NewResourceType", newresourcetype);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#ServerName", ServerName);
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#CompanyName", company);
                         var temp = db.UserReportings.Where(ur => ur.UserID == Collection.UserId).Select(ur => ur.ReportingUserID).FirstOrDefault();
                         var CCReportingPerson = db.Users.Where(u => u.UserID == temp).Select(u => u.EmailAddress).FirstOrDefault();

                         List<string> CCMailIds = new List<string>();

                         CCMailIds.Add("kirankumar@dsrc.co.in");
                         CCMailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                         //CCMailIds.Add("daniel@dsrc.com");
                         //CCMailIds.Add("jali@dsrc.co.in");

                         string CCEmailAddress = "";

                         foreach (string mail in CCMailIds)
                         {
                             CCEmailAddress += mail + ",";
                         }

                         CCEmailAddress = CCEmailAddress.Remove(CCEmailAddress.Length - 1);

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "Kirankumar@dsrc.co.in";
                             string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();

                                 DsrcMailSystem.MailSender.SendMail(null, objProjectSummary.Subject + " - Test Mail Please Ignore", null, htmlnewjoiningOnBoarding + " - Testing Please ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                 //string[] words;

                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();


                                 DsrcMailSystem.MailSender.SendMail(null, objProjectSummary.Subject, "", htmlnewjoiningOnBoarding, "HRMS@dsrc.co.in", record.User.EmailAddress, CCEmailAddress + CCReportingPerson, "Kirankumar@dsrc.co.in", Server.MapPath(pathvalue));
                                 //DsrcMailSystem.MailSender.SendMail(null, "Project RAG Status", "", MailBuilder.ProjectSummary(projectName, comments, Pro_Status), "HRMS@dsrc.co.in", new List<string>() { "boobalan.k@dsrc.co.in" }, imagePath.ToArray());
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("On Change Resource", folder, ServerName);
                     }

                //return RedirectToAction("Notification", "ProjectMapping");
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }


        

       
        [HttpGet]
        public ActionResult Weekdropdown()
        {
            List<DSRCManagementSystem.Models.MeetingSchedule> meeting = new List<DSRCManagementSystem.Models.MeetingSchedule>();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            MeetingSchedule obj = new MeetingSchedule();

            meeting = (from t in objdb.MettingSchedules
                       //join user in objdb.Users on t.Attendees equals user.UserID
                       where t.Week == obj.WeekDropDown
                       select new DSRCManagementSystem.Models.MeetingSchedule
                       {
                           Day = t.Day,
                           From = t.TimeSlot,
                           To = t.EndTime,
                           Week = t.Week,
                           Attendees = t.Attendees,
                           ProjectID = t.ProjectID
                       }).ToList();
            return View(meeting);
        }



        [HttpGet]
        public ActionResult ProjectAgenda(int id)
        {
            DSRCManagementSystem.Models.AgandaForProject objagenda = new DSRCManagementSystem.Models.AgandaForProject();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string value = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.Agenda).FirstOrDefault();
            if (value != null)
            {
                DateTime? date = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.AgendaDate).FirstOrDefault();
                DateTime? current = System.DateTime.Now;
                TimeSpan difference = current.Value - date.Value;
                double k = difference.TotalDays;
                int j = Convert.ToInt32(k);
                if (id == 47)
                {
                    if (value != null && j >= 7)
                    {
                        value = null;
                        objagenda.ProjectAganda = value;
                    }
                    else if (value != null && j < 7)
                    {
                        objagenda.ProjectAganda = value;

                    }
                    else
                    {
                        objagenda.ProjectAganda = "";
                    }
                }
                else
                {
                    if (value != null && j >= 13)
                    {
                        value = null;
                        objagenda.ProjectAganda = value;
                    }
                    else if (value != null && j < 13)
                    {
                        objagenda.ProjectAganda = value;
                    }
                    else
                    {
                        objagenda.ProjectAganda = "";
                    }
                }
            }
            else
            {
                objagenda.ProjectAganda = "";
            }


            List<DSRCManagementSystem.Models.Historylist> objlist = new List<DSRCManagementSystem.Models.Historylist>();
            objlist = (from p in objdb.AgendaFeedbacks
                       select new DSRCManagementSystem.Models.Historylist
                       {
                           ProjectId = p.ProjectId,
                           agenda = p.Agenda,
                           feedback = p.Feedback
                       }).ToList();

            int i = objlist.Count();
            int? project = objlist.Select(o => o.ProjectId).FirstOrDefault();
            TempData["project"] = project;
            TempData["Count"] = i;

            System.Web.HttpContext.Current.Application["agenda"] = id;
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectAgenda(AgandaForProject objagenda)
        {
            objagenda.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Application["agenda"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var value = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.Feedback).FirstOrDefault();
            var agenda = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.Agenda).FirstOrDefault();
            var mom = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.MOM).FirstOrDefault();

            if (value != null && Convert.ToInt32(TempData["Count"]) == 0)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                db.AddToAgendaFeedbacks(obj);
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda != null)
            {

                DSRCManagementSystemEntities1 oho = new DSRCManagementSystemEntities1();
                var valuefed = oho.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                valuefed.Agenda = objagenda.ProjectAganda.ToString();
                valuefed.AgendaDate = System.DateTime.Now;
                oho.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }



            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda == null && mom != null)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var val = db.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                val.ProjectId = objagenda.UserId;
                val.Agenda = objagenda.ProjectAganda.ToString();
                val.AgendaDate = System.DateTime.Now;
                db.SaveChanges();

                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda == null && mom == null)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                db.AddToAgendaFeedbacks(obj);
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 oh = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                oh.AddToAgendaFeedbacks(obj);
                oh.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (value != null && Convert.ToInt32(TempData["Count"]) != 0)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var valuefed = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                valuefed.Agenda = objagenda.ProjectAganda.ToString();

                valuefed.AgendaDate = System.DateTime.Now;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }


            return View();
        }


        [HttpGet]
        public ActionResult ProjectFeedBack(int id)
        {
            DSRCManagementSystem.Models.ProjectFeedBack objagenda = new DSRCManagementSystem.Models.ProjectFeedBack();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string value = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.Feedback).FirstOrDefault();
            if (value != null)
            {
                DateTime? date = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.FeedbackDate).FirstOrDefault();
                DateTime? current = System.DateTime.Now;
                TimeSpan difference = current.Value - date.Value;
                double k = difference.TotalDays;
                int j = Convert.ToInt32(k);
                if (id == 47)
                {
                    if (value != null && j >= 7)
                    {
                        value = null;
                        objagenda.Feedback = value;
                    }
                    else if (value != null && j < 7)
                    {
                        objagenda.Feedback = value;

                    }
                    else
                    {
                        objagenda.Feedback = "";
                    }
                }
                else
                {
                    if (value != null && j >= 13)
                    {
                        value = null;
                        objagenda.Feedback = value;
                    }
                    else if (value != null && j < 13)
                    {
                        objagenda.Feedback = value;
                    }
                    else
                    {
                        objagenda.Feedback = "";
                    }
                }
            }
            else
            {
                objagenda.Feedback = "";
            }


            List<DSRCManagementSystem.Models.Historylist> objlist = new List<DSRCManagementSystem.Models.Historylist>();
            objlist = (from p in objdb.AgendaFeedbacks
                       select new DSRCManagementSystem.Models.Historylist
                       {
                           agenda = p.Agenda,
                           feedback = p.Feedback
                       }).ToList();

            int i = objlist.Count();
            TempData["Count"] = i;

            System.Web.HttpContext.Current.Application["id"] = id;
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectFeedBack(ProjectFeedBack ovj, AgandaForProject objagenda)
        {
            ovj.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Application["id"]);


            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();

            var value = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.Agenda).FirstOrDefault();
            var feedback = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.Feedback).FirstOrDefault();
            var mom = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.MOM).FirstOrDefault();
            if (value != null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var valuefed = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                valuefed.Feedback = ovj.Feedback;
                valuefed.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback != null && mom != null)
            {

                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var val = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                val.Feedback = ovj.Feedback;
                val.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }


            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback == null && mom != null)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                var val = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                val.ProjectId = ovj.UserId;
                val.Feedback = ovj.Feedback;
                val.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Feedback = ovj.Feedback;
                obj.FeedbackDate = System.DateTime.Now;
                obj.ProjectId = ovj.UserId;
                objdb.AddToAgendaFeedbacks(obj);
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Feedback = ovj.Feedback;
                obj.FeedbackDate = System.DateTime.Now;
                obj.ProjectId = ovj.UserId;
                objdb.AddToAgendaFeedbacks(obj);
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value != null && Convert.ToInt32(TempData["Count"]) != 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var valuefed = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                valuefed.Feedback = ovj.Feedback;
                valuefed.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }
            return View();

        }
        [HttpGet]
        public ActionResult MOM(int id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ProjectMom objmom = new DSRCManagementSystem.Models.ProjectMom();
            var value = objdb.AgendaFeedbacks.Where(x => x.ProjectId == id).Select(o => o.MOM).FirstOrDefault();
            if (value != null)
            {
                objmom.ProjectMOM = value.ToString();

            }
            else
            {
                objmom.ProjectMOM = "";
            }
            System.Web.HttpContext.Current.Application["agenda"] = id;
            return View(objmom);
        }
        [HttpPost]
        public ActionResult MOM(ProjectMom objmom)
        {
            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();
            objmom.ProjectId = Convert.ToInt32(System.Web.HttpContext.Current.Application["agenda"]);
            var agenda = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.Agenda).FirstOrDefault();
            var feedback = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.Feedback).FirstOrDefault();
            var mom = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.MOM).FirstOrDefault();
            if (agenda != null && feedback != null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var fed = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                fed.MOM = objmom.ProjectMOM;
                fed.ProjectId = objmom.ProjectId;
                fed.MOMDate = System.DateTime.Now;
                ob.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda == null && feedback != null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var age = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                age.MOM = objmom.ProjectMOM;
                age.MOMDate = System.DateTime.Now;
                age.ProjectId = objmom.ProjectId;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda != null && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var age = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                age.MOM = objmom.ProjectMOM;
                age.MOMDate = System.DateTime.Now;
                age.ProjectId = objmom.ProjectId;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda == null && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback objd = new DSRCManagementSystem.AgendaFeedback();
                objd.MOM = objmom.ProjectMOM;
                objd.ProjectId = objmom.ProjectId;
                objd.MOMDate = System.DateTime.Now;
                obj.AddToAgendaFeedbacks(objd);
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        public ActionResult ProjectMeeting()
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var ProjectList = objdb.Projects.ToList();

            ProjectMeetingTime objtime = new ProjectMeetingTime();



            var ProjectLead = (from t in objdb.Users
                               join atn in objdb.MeetingGuids on t.UserID equals atn.UserId

                               select new
                               {
                                   Id = t.UserID,

                                   FirstName = t.FirstName,
                                   LastName = t.LastName,


                               }).ToList();


            var Days = objdb.Master_Days.ToList();


            ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id", "FirstName", "LastName");
            ViewBag.DayList = new SelectList(new[] { new Master_Days() { Id = 0, Days = "------Select--------" } }.Union(Days), "Id", "Days", 0);
            ViewBag.Projects = new SelectList(new[] { new Project() { ProjectID = 0, ProjectName = "----Select------" } }.Union(ProjectList), "ProjectID", "ProjectName", 0);



            ViewBag.Week = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);



            return View();



        }

        [HttpPost]

        public ActionResult ProjectMeeting(ProjectMeetingTime objmeeting)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            var Message = "";

            var already = objdb.MettingSchedules.Where(o => o.Week == objmeeting.Week && o.Day == objmeeting.Day && o.TimeSlot == objmeeting.TimeSlotFrom).Select(i => i.ProjectID).FirstOrDefault();

            if (already != null)
            {
                return Json(new { Result = "AlreadyExist", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }


            var time = Convert.ToDateTime(objmeeting.TimeSlotFrom);
            var hour = Convert.ToInt32(time.Hour);
            var min = Convert.ToInt32(time.Minute);
            var tt = time.ToString("tt");



            //  List<string> obj = new List<string>();
            var obj = objdb.MettingSchedules.Where(o => o.Week == objmeeting.Week && o.Day == objmeeting.Day).Select(o => o).ToList();
            TimeSpan CurTime = new TimeSpan(hour, min, 0);


            foreach (var item in obj)
            {
                var dbtime = Convert.ToDateTime(item.TimeSlot);
                var hourdb = Convert.ToInt32(dbtime.Hour);
                var mindb = Convert.ToInt32(dbtime.Minute);
                var ttdb = dbtime.ToString("tt");

                TimeSpan Db_StartTime = new TimeSpan(hourdb, mindb, 0);


                var dbendtime = Convert.ToDateTime(item.EndTime);
                var hourenddb = Convert.ToInt32(dbendtime.Hour);
                var minenddb = Convert.ToInt32(dbendtime.Minute);
                var endttdb = dbendtime.ToString("tt");

                TimeSpan Db_EndTime = new TimeSpan(hourenddb, minenddb, 0);

                if (Db_StartTime == CurTime)
                {
                    Message = "availabletime";

                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }

                else if (Db_StartTime >= CurTime && CurTime < Db_EndTime)
                {
                    Message = "availabletime";
                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }
                else if (Db_StartTime <= CurTime && CurTime <= Db_EndTime)
                {
                    Message = "availabletime";
                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }
            }






            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.MettingSchedule objdetail = new DSRCManagementSystem.MettingSchedule();
            objdetail.ProjectID = Convert.ToInt32(objmeeting.ProjectNameId);
            objdetail.TimeSlot = objmeeting.TimeSlotFrom;
            objdetail.EndTime = objmeeting.TimeSlotTo;
            objdetail.Day = objmeeting.Day;

            objdetail.Week = objmeeting.Week;
            objdetail.Attendees = objmeeting.Attendee;

            db.AddToMettingSchedules(objdetail);
            db.SaveChanges();

            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);



        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            DateTime firstWeekDay = jan1.AddDays(daysOffset);
            int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1 || firstWeek > 50)
            {
                weekOfYear -= 1;
            }
            return firstWeekDay.AddDays(weekOfYear * 7);
        }

        [HttpGet]
        public ActionResult EditAttendee(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.MeetingSchedule editobj = new DSRCManagementSystem.Models.MeetingSchedule();
            editobj.Id = ID;

            var ProjectLead = (from t in db.Users
                               join atn in db.MeetingGuids on t.UserID equals atn.UserId

                               select new
                               {
                                   Id = t.UserID,
                                   FirstName = t.FirstName
                               }).ToList();

            var AttendeeList = (from a in db.MettingSchedules
                                where a.Id == ID
                                select new MeetingSchedule()
                                {
                                    Attendees = a.Attendees
                                }).FirstOrDefault();

            List<int> selectedAttendees = new List<int>();

            if (AttendeeList.Attendees != null)
            {

                string[] tokens = AttendeeList.Attendees.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var i in tokens)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedAttendees.Add(val);
                }
            }

            ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id", "FirstName", selectedAttendees);

            return View(editobj);
        }

        [HttpPost]
        public ActionResult EditAttendee(int Id, string Attendee)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == Id);

            ReqToEdit.Attendees = Attendee;
            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult MeetingSchedule()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = Convert.ToInt32(Session["UserID"]);
            var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();

            var date = DateTime.Now;

            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            var result = (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;

            ViewBag.Week = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);


            DSRCManagementSystem.Models.MeetingSchedule objmeeting = new DSRCManagementSystem.Models.MeetingSchedule();




            var firstdateofweek = DateTime.Now;
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            Dictionary<string, DateTime> currentWeek = new Dictionary<string, DateTime>();
            Dictionary<string, DateTime> nextWeek = new Dictionary<string, DateTime>();

            var weekofYear = cal.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);
            firstdateofweek = FirstDateOfWeek(DateTime.Now.Year, weekofYear, CultureInfo.CurrentCulture);
            int i = 0;
            while (i != 12) // skiped weeekend days...
            {
                if (i < 5)
                    currentWeek.Add(firstdateofweek.AddDays(i).DayOfWeek.ToString(), firstdateofweek.AddDays(i).Date);
                else if (i >= 7)
                    nextWeek.Add(firstdateofweek.AddDays(i).DayOfWeek.ToString(), firstdateofweek.AddDays(i).Date);

                i++;
            }

            if (result % 2 == 0)
            {
               
                List<DSRCManagementSystem.Models.MeetingSchedule> objmail = new List<DSRCManagementSystem.Models.MeetingSchedule>();

                if (getBracnch != 1)
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               where proj.ProjectID == 0
                               select new DSRCManagementSystem.Models.MeetingSchedule
                               {
                                   Id = metting_schedule.Id,
                                   Project = proj.ProjectName,
                                   ProjectID = metting_schedule.ProjectID,
                                   Day = metting_schedule.Day,
                                   DayId = days.Id,
                                   Week = metting_schedule.Week ?? 0,
                                   Attendees = metting_schedule.Attendees,

                                   From = metting_schedule.TimeSlot,
                                   To = metting_schedule.EndTime,

                               }).OrderByDescending(x => x.Week).ThenBy(x => x.DayId).ThenBy(x => x.From).ToList();


                    foreach (var meetingSchedule in objmail)
                    {
                        meetingSchedule.Attendees = ProjectMappingController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               select new DSRCManagementSystem.Models.MeetingSchedule
                               {
                                   Id = metting_schedule.Id,
                                   Project = proj.ProjectName,
                                   ProjectID = metting_schedule.ProjectID,
                                   Day = metting_schedule.Day,
                                   DayId = days.Id,
                                   Week = metting_schedule.Week ?? 0,
                                   Attendees = metting_schedule.Attendees,

                                   From = metting_schedule.TimeSlot,
                                   To = metting_schedule.EndTime,

                               }).OrderByDescending(x => x.Week).ThenBy(x => x.DayId).ThenBy(x => x.From).ToList();


                    foreach (var meetingSchedule in objmail)
                    {
                        meetingSchedule.Attendees = ProjectMappingController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                return View(objmail);
            }
            else
            {
                
                List<DSRCManagementSystem.Models.MeetingSchedule> objmail = new List<DSRCManagementSystem.Models.MeetingSchedule>();

                if (getBracnch != 1)
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               where proj.ProjectID == 0
                               select new DSRCManagementSystem.Models.MeetingSchedule
                               {
                                   Id = metting_schedule.Id,
                                   Project = proj.ProjectName,
                                   ProjectID = metting_schedule.ProjectID,
                                   Day = metting_schedule.Day,
                                   DayId = days.Id,
                                   Week = metting_schedule.Week ?? 0,
                                   Attendees = metting_schedule.Attendees,

                                   From = metting_schedule.TimeSlot,
                                   To = metting_schedule.EndTime,

                               }).OrderByDescending(x => x.Week).ThenBy(x => x.DayId).ThenBy(x => x.From).ToList();


                    foreach (var meetingSchedule in objmail)
                    {
                        meetingSchedule.Attendees = ProjectMappingController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               select new DSRCManagementSystem.Models.MeetingSchedule
                               {
                                   Id = metting_schedule.Id,
                                   Project = proj.ProjectName,
                                   ProjectID = metting_schedule.ProjectID,
                                   Day = metting_schedule.Day,
                                   DayId = days.Id,
                                   Week = metting_schedule.Week ?? 0,
                                   Attendees = metting_schedule.Attendees,

                                   From = metting_schedule.TimeSlot,
                                   To = metting_schedule.EndTime,

                               }).OrderBy(x => x.Week).ThenBy(x => x.DayId).ThenBy(x => x.From).ToList();

                    foreach (var meetingSchedule in objmail)
                    {
                        meetingSchedule.Attendees = ProjectMappingController.GetUserString(db, meetingSchedule.Attendees);

                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }

                return View(objmail);
            }

        }


        
        [HttpGet]
        public ActionResult History(int ProjectId)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.History> objhis = new List<DSRCManagementSystem.Models.History>();

            var agendadate = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.AgendaDate).FirstOrDefault();

            var feeddate = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.FeedbackDate).FirstOrDefault();

            var mom = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.MOMDate).FirstOrDefault();

            if (agendadate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.AgendaDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }
            else if (feeddate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.FeedbackDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }

            else if (feeddate != null && agendadate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.FeedbackDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }

            else if (feeddate == null && agendadate == null && mom != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.FeedbackDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }
            return View(objhis);


        }
        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.FirstName + " " + (user.LastName ?? "")).ToList();
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

      


        public ActionResult GetAssignedProject(int pId)
        {
            List<SelectListItem> Members = new List<SelectListItem>();
            var assignedMembers = GetMembers(pId);
            foreach (var item in assignedMembers)
            {                
                Members.Add(new SelectListItem { Text = item.EmployeeName + '(' + item.MemberType + ')', Value = (item.UserId.ToString() + '+' + item.MemberTypeID.ToString()) });
            }
            return Json(Members, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProjectStartDateEndDate(int Pid)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var ProjectStartDate = (Convert.ToString((db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectStartDate)) == "") ? "" : Convert.ToDateTime(db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectStartDate).ToString("dd-MM-yyyy");
            var ProjectEndDate = (Convert.ToString((db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectEndDate)) == "") ? "" : Convert.ToDateTime(db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectEndDate).ToString("dd-MM-yyyy");            
            //var ProjectStartDate = Convert.ToDateTime(db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectStartDate).ToString();
            //var ProjectEndDate = Convert.ToDateTime(db.Projects.FirstOrDefault(x => x.ProjectID == Pid).ProjectEndDate).ToString();
            return Json(new { StartDate = ProjectStartDate, EndDate = ProjectEndDate }, JsonRequestBehavior.AllowGet);
        }

        public int? ProjectId { get; set; }

        public string thisYear { get; set; }

        public int weeknum { get; set; }

        public object Project_Guids { get; set; }
    }
}