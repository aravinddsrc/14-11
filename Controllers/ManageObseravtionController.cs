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
    public class ManageObservationController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult Observation()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
            List<DSRCManagementSystem.Models.Department> Value = new List<DSRCManagementSystem.Models.Department>();
            Value = (from p in db.Observations
                     join a in db.Activities on p.ActivityID equals a.ActivityID
                     join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                     join u in db.Users on p.UserId equals u.UserID
                     join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                     from y1 in yr.DefaultIfEmpty()
                     join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                     from y2 in x.DefaultIfEmpty()
                     join ur in db.UserReportings on u.UserID equals ur.UserID
                     where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == GetBranch)

                     //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == GetBranch)
                     select new DSRCManagementSystem.Models.Department()
                     {
                         DPID = u.DepartmentId,
                         GPID = u.DepartmentGroup,
                         DepartmentID = p.ObservationID,
                         DepartmentName = y2.DepartmentName,
                         OBUserName = y1.GroupName,
                         ActivityDate = (DateTime)p.Date,
                         GroupID = p.UserId,
                         GroupName = u.FirstName + " " + u.LastName,
                         Activity = a.Activity1,
                         ActivityLevel = ac.ActivityLevel1,
                         Comment = p.Comments,
                         SelectedUserStatusid = u.UserStatus
                     }).OrderBy(o => o.ActivityDate).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
            }

            //return View(objmodel);

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  UserId = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "UserId", "Name", GetBranch);

            var Department = db.Departments.Where(o => o.IsActive == true && o .BranchID == 1).Select(c => new
            {
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentName
            }).OrderBy(x=>x.DepartmentName).ToList();
            ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

            var Group = (from dg in db.DepartmentGroups 
                         join dgm in db.DepartmentGroupMappings on dg.GroupID equals dgm.GroupID
                         join d in db.Departments on dgm.DepartmentID equals d.DepartmentId
                         where d.IsActive == true && d.BranchID == 1
                         select new DSRCEmployees
                         {
                       
                               GroupID = dg.GroupID,
                               GroupName = dg.GroupName
                      }).OrderBy(x=>x.GroupName).ToList();
            ViewBag.Group = new SelectList(Group, "GroupID", "GroupName");


            var Users = (from u in db.Users 
                         where u.IsActive == true && u.UserStatus != 6  && u.BranchId == 1
                         select new DSRCEmployees
                         {
                             Name = u.FirstName + " " + u.LastName,
                             UserId = u.UserID,

                         }).Distinct().OrderBy(x => x.Name).ToList();
            ViewBag.Users = new SelectList(Users, "UserId", "Name");

            return View(objmodel);
        }
        [HttpPost]
        public ActionResult Observation(DSRCManagementSystem.Models.Department Model)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
            string dpt = Model.DepartmentName;
            int dt = Convert.ToInt32(dpt);
           

            string gpt = Model.GroupName;
            int gp = Convert.ToInt32(gpt);
            int Date = 0;
            if (Model.ActivityDate == DateTime.MinValue)
            {

                Date = 0;
            }
            else
            {
                Date = 1;

            }

            List<DSRCManagementSystem.Models.Department> Result = new List<DSRCManagementSystem.Models.Department>();
            Result = (from p in db.Observations
                      join a in db.Activities on p.ActivityID equals a.ActivityID
                      join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                      join u in db.Users on p.UserId equals u.UserID
                      join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                      from y1 in yr.DefaultIfEmpty()
                      join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                      from y2 in x.DefaultIfEmpty()
                      join ur in db.UserReportings on u.UserID equals ur.UserID
                      where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                      //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                      select new DSRCManagementSystem.Models.Department()
                      {
                          DPID = u.DepartmentId,
                          GPID = u.DepartmentGroup,
                          DepartmentID = p.ObservationID,
                          DepartmentName = y2.DepartmentName,
                          OBUserName = y1.GroupName,
                          ActivityDate = (DateTime)p.Date,
                          GroupID = p.UserId,
                          GroupName = u.FirstName + " " + u.LastName,
                          Activity = a.Activity1,
                          ActivityLevel = ac.ActivityLevel1,
                          Comment = p.Comments,
                          SelectedUserStatusid = u.UserStatus,
                          Users = u.UserID
                      }).ToList();
            foreach (var x in Result)
            {

                objmodel.Add(x);
            }


            List<DSRCManagementSystem.Models.Department> Value =
                       new List<DSRCManagementSystem.Models.Department>();

            if (Convert.ToInt32(Model.DepartmentName) == 0)
            {
                Model.DepartmentName = null;
            }

            //NULL
            if ((Model.DepartmentName == null) && (gp == 0) && Date == 0)
            {


                Value = Result.ToList();

                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");

            }

            //DATE//
           else if (Model.DepartmentName == null && (gp == 0) && Date != 0)
            {

                Value = Result.Where(x => x.ActivityDate == Model.ActivityDate).ToList();

                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");

            }

            //DEPARTMENTNAME//
            else if (Model.DepartmentName != null && (gp == 0) && Date == 0)
            {

                Value = Result.Where(x => x.DPID == dt).ToList();

                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");
            }



            //DEPARTMENT & GROUP//
            else if (Model.DepartmentName != null && (gp != 0) && Date == 0)
            {
                Value = Result.Where(x => x.DPID == dt && x.GPID == gp).ToList();
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");

            }


            //DEPARTMENT & DATE//
            else if (Model.DepartmentName != null && (gp == 0) && Date != 0)
            {
                Value = Result.Where(x => x.DPID == dt && x.ActivityDate == Model.ActivityDate).ToList();
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");

            }

            //DEPARTMENT && GROUP & DATE//
            else if (Model.DepartmentName != null && (gp != 0) && Date != 0)
            {
                Value = Result.Where(x => x.DPID == dt && x.GPID == gp && x.ActivityDate == Model.ActivityDate).ToList();

                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == dt
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name");

            }


            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  UserId = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "UserId", "Name", Model.Idbranchname1);


            if (Model.Idbranchname1 != null)
            {
                var Department = db.Departments.Where(o => o.IsActive == true && o.BranchID == Model.Idbranchname1).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(x => x.DepartmentName).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

            }
            else
            {

                var Department = db.Departments.Where(o => o.IsActive == true && o.BranchID == GetBranch).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(x => x.DepartmentName).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");



            }

            if (Model.UserId != 0)
            {

                if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt == 0 && gp == 0)
                {

                    Value = Value.Where(x => x.Users == Model.UserId).ToList();
                    var Users = (from u in db.Users 
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name", Model.UserId);


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt != 0 && gp == 0)
                {

                    Value = Value.Where(x => x.Users == Model.UserId).ToList();
                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name", Model.UserId);


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt == 0 && gp != 0)
                {

                    Value = Value.Where(x => x.Users == Model.UserId).ToList();
                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name", Model.UserId);


                }

                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt != 0 && gp != 0 && Model.UserId != 0)
                {

                    Value = Value.Where(x => x.Users == Model.UserId).ToList();
                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.UserID == Model.UserId
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name", Model.UserId);


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt != 0 && gp != 0)
                {

                    Value = Value.Where(x => x.Users == Model.UserId).ToList();
                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name", Model.UserId);


                }

            }
            if (Model.UserId == 0)
            {
                if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt == 0 && gp == 0)
                {


                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name");


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt != 0 && gp == 0)
                {


                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name");


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt == 0 && gp != 0)
                {


                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x=> x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name");


                }
                else if ((Model.Idbranchname1 != 0 || Model.Idbranchname1 != null) && dt != 0 && gp != 0)
                {

                 
                    var Users = (from o in db.Observations
                                 join u in db.Users on o.UserId equals u.UserID
                                 where u.IsActive == true && u.UserStatus != 6 && o.IsActive == true && u.BranchId == Model.Idbranchname1 && u.DepartmentId == dt && u.DepartmentGroup==gp
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + u.LastName,
                                     UserId = u.UserID,

                                 }).Distinct().OrderBy(x => x.Name).ToList();
                    ViewBag.Users = new SelectList(Users, "UserId", "Name");


                }   

            }




            return View(Value);
        }


        [HttpGet]
        public ActionResult EditObservation(int UID, string Activity, string ActivityLevel, string Comment)
        {
            try
            {
                ViewBag.UID = UID;



                var ActivitiesID = db.Activities.Where(x => x.Activity1 == Activity).Select(x => x.ActivityID).FirstOrDefault();
                var ActivitiesLevelID = db.ActivityLevels.Where(x => x.ActivityLevel1 == ActivityLevel).Select(x => x.ActivityLevelID).FirstOrDefault();


                var Activities = db.Activities.Where(x => x.IsActive == true).Select(c => new
                {
                    ActivityName = c.Activity1,
                    ActivityId = c.ActivityID
                }).OrderBy(o =>o.ActivityName).ToList();

                ViewBag.Activity = new SelectList(Activities, "ActivityId", "ActivityName", ActivitiesID);

                var ActivityLevels = db.ActivityLevels.Where(x => x.IsActive == true).Select(c => new
                {
                    ActivityLevelName = c.ActivityLevel1,
                    ActivityLevelId = c.ActivityLevelID
                }).OrderBy(o => o.ActivityLevelName).ToList();

                ViewBag.ActivityLevels = new SelectList(ActivityLevels, "ActivityLevelId", "ActivityLevelName", ActivitiesLevelID);
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
        public ActionResult EditObservation(DSRCManagementSystem.Models.Department Model)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                int UID = Model.UID;
                int ActivityName = Convert.ToInt32(Model.ActivityName);
                int Level = Convert.ToInt32(Model.ActivityLevelName);
                string comment = (Model.Comment).Replace("-","\"");  //added

                var tempName = db.Activities.Where(r => r.ActivityID == ActivityName).Select(f => f.Activity1).FirstOrDefault();
                var tempLevel = db.ActivityLevels.Where(r => r.ActivityLevelID == Level).Select(f => f.ActivityLevel1).FirstOrDefault();


                var x = db.Observations.Where(o => o.ObservationID == UID).FirstOrDefault();
                x.ActivityID = ActivityName;
                x.ActivityLevelID = Level;
                x.Comments = comment;
                db.SaveChanges();
                //////////////////////
                var createdby = db.Users.Where(e => e.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var FromEmailID = db.Users.Where(e => e.UserID == userId).Select(o => o.UserName).FirstOrDefault();
                var ActivityNameEmail = db.Activities.Where(z => z.ActivityID == ActivityName).Select(o => o.Activity1).FirstOrDefault();
                var ActivityLevelEmail = db.ActivityLevels.Where(z => z.ActivityLevelID == Level).Select(o => o.ActivityLevel1).FirstOrDefault();

                var GetUserID = db.Observations.Where(u => u.ObservationID == UID).Select(o => o.UserId).FirstOrDefault();
                var getdate = db.Observations.Where(u => u.ObservationID == UID).Select(o => o.Date).FirstOrDefault();

                var Getmailid = db.Users.Where(e => e.UserID == GetUserID).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = db.Users.Where(e => e.UserID == GetUserID).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();





                var objcom = db.Master_ApplicationSettings.Where(e => e.AppKey == "Company Name")
                                            .Select(o => o.AppValue)
                                            .FirstOrDefault();

              //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

            var check = db.EmailTemplates.Where(a=> a.TemplatePurpose== "Edit Activity").Select(o=> o.EmailTemplateID).FirstOrDefault();
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Edit Activity").Select(c => c.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var obj1 = (from p in db.EmailPurposes.Where(e => e.EmailPurposeName == "Edit Activity")
                                     join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                     select new DSRCManagementSystem.Models.Email
                                     {
                                         To = p.To,
                                         CC = p.CC,
                                         BCC = p.BCC,
                                         Subject = p.Subject,
                                         Template = q.TemplatePath
                                     }).FirstOrDefault();

                         string TemplatePath = Server.MapPath(obj1.Template);
                         string html = System.IO.File.ReadAllText(TemplatePath);

                         //string Title = " " + objcom + "   calendar event Created";
                         string Subject = "Activity Created on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal-New Assessment Edited";


                         html = html.Replace("#UserName", GetName);
                         html = html.Replace("#ActivityName", ActivityNameEmail);
                         html = html.Replace("#Date", getdate.ToString().Substring(0, 10));
                         html = html.Replace("#Changedby", createdby);
                         html = html.Replace("#ActivityLevel", ActivityLevelEmail);
                         html = html.Replace("#Comment", comment);
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName", ServerName);
                         List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                         string EmailAddress = "";

                         foreach (string mail in MailIds)
                         {
                             EmailAddress += mail + ",";
                         }

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", "aravind.a@dsrc.co.in", Server.MapPath(pathvalue.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));

                             });
                         }

                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Edit Activity", folder, ServerName);
                     }


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }


            return Json("Success1", JsonRequestBehavior.AllowGet);


        }

        [HttpGet]
        public ActionResult Addobservation(DSRCManagementSystem.Models.Department Model, FormCollection form, string ActivityDate,string Branch1,string Department1,string Group1)
        {
            var chekdate =0;
             
            if(ActivityDate!=null && ActivityDate!="")
            {
                DateTime time = Convert.ToDateTime(ActivityDate);
                 chekdate = db.Observations.Where(x => x.Date == time &&x.IsActive==true).Select(o => o.ObservationID).FirstOrDefault();
            }

            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            List<DSRCManagementSystem.Models.Department> objmodel = new List<DSRCManagementSystem.Models.Department>();
            List<DSRCManagementSystem.Models.Department> DepartmentValue = new List<DSRCManagementSystem.Models.Department>();
            List<DSRCManagementSystem.Models.Department> Uservalue= new List<DSRCManagementSystem.Models.Department>();
            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();

            if ((Model.Idbranchname1 == 0) || (Model.Idbranchname1 == null)  && chekdate==0)
            {
                Model.Idbranchname1 = GetBranch;
            }
            if ((ActivityDate == null || ActivityDate == "") && chekdate == 0)
            {
                if (Model.DepartmentID == 0 && (Model.UID == 0 || Model.UID == null))
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                }

                if (Model.DepartmentID != 0 && (Model.UID == 0 || Model.UID == null) && chekdate == 0)
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.DepartmentId == Model.DepartmentID && u.BranchId == Model.Idbranchname1)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                    ViewBag.DepartmentID = Model.DepartmentID;
                }

                if (Model.DepartmentID != 0 && Model.UID != 0 && chekdate == 0)
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.DepartmentId == Model.DepartmentID && u.DepartmentGroup == Model.UID && u.BranchId == Model.Idbranchname1)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                    ViewBag.DepartmentID = Model.DepartmentID;
                    ViewBag.UID = Model.UID;
                }
            }
            if (ActivityDate != null && ActivityDate != ""  && chekdate == 0)
            {
                DateTime CUR = Convert.ToDateTime(ActivityDate);
                if (Model.DepartmentID == 0 && (Model.UID == 0 || Model.UID == null))
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DateOfJoin <= CUR)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                }

                if (Model.DepartmentID != 0 && (Model.UID == 0 || Model.UID == null) && chekdate == 0)
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.DepartmentId == Model.DepartmentID && u.BranchId == Model.Idbranchname1 && u.DateOfJoin <= CUR)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                    ViewBag.DepartmentID = Model.DepartmentID;
                }

                if (Model.DepartmentID != 0 && Model.UID != 0 && chekdate == 0)
                {
                    DepartmentValue = (from ur in db.UserReportings
                                       join u in db.Users on ur.UserID equals u.UserID
                                       where (ur.ReportingUserID == userId && u.IsActive == true && u.UserStatus != 6 && u.DepartmentId == Model.DepartmentID && u.DepartmentGroup == Model.UID && u.BranchId == Model.Idbranchname1 && u.DateOfJoin <= CUR)
                                       select new DSRCManagementSystem.Models.Department()
                                       {
                                           Users = u.UserID,
                                           OBUserName = u.FirstName + " " + u.LastName,
                                           SelectedUserStatusid = u.UserStatus
                                       }).ToList();
                    ViewBag.DepartmentID = Model.DepartmentID;
                    ViewBag.UID = Model.UID;
                }
            }

            List<DSRCManagementSystem.Models.Department> Result = new List<DSRCManagementSystem.Models.Department>();
            List<DSRCManagementSystem.Models.Department> MainResult = new List<DSRCManagementSystem.Models.Department>();
            if ( chekdate != 0)
            {
                DateTime CUR = Convert.ToDateTime(ActivityDate);
                if (Model.DepartmentID == 0 && (Model.UID == 0 || Model.UID == null))
                {
                    //List<DSRCManagementSystem.Models.Department> Result = new List<DSRCManagementSystem.Models.Department>();
                    Result = (from p in db.Observations
                              join a in db.Activities on p.ActivityID equals a.ActivityID
                              join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                              join u in db.Users on p.UserId equals u.UserID
                              join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                              from y1 in yr.DefaultIfEmpty()
                              join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                              from y2 in x.DefaultIfEmpty()
                              join ur in db.UserReportings on u.UserID equals ur.UserID
                              where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DateOfJoin <= CUR)
                              //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                              select new DSRCManagementSystem.Models.Department()
                              {
                                  DPID = u.DepartmentId,
                                  GPID = u.DepartmentGroup,
                                  DepartmentID = p.ObservationID,
                                  DepartmentName = y2.DepartmentName,
                                  OBUserName = u.FirstName + " " + u.LastName,
                                  ActivityDate = (DateTime)p.Date,
                                  GroupID = p.UserId,
                                  GroupName = y1.GroupName,
                                  Activity = a.Activity1,
                                  ActivityLevel = ac.ActivityLevel1,
                                  Comment = p.Comments,
                                  SelectedUserStatusid = u.UserStatus,
                                  Users = u.UserID
                              }).ToList();

                              MainResult  = (from u in db.Users 
                                             join  ur in db.UserReportings on u.UserID equals ur.UserID
                                             where (u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DateOfJoin<= CUR)
                                                                                         
                                                      
                              select new DSRCManagementSystem.Models.Department()
                              {
                                  DPID = u.DepartmentId,
                                  GPID = u.DepartmentGroup,
                                  OBUserName = u.FirstName + " " + u.LastName,
                                  SelectedUserStatusid = u.UserStatus,
                                  Users = u.UserID
                              }).ToList();



                }

                else if(Model.DepartmentID != 0 && (Model.UID == 0 || Model.UID == null))
                {
                   
                    Result = (from p in db.Observations
                              join a in db.Activities on p.ActivityID equals a.ActivityID
                              join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                              join u in db.Users on p.UserId equals u.UserID
                              join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                              from y1 in yr.DefaultIfEmpty()
                              join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                              from y2 in x.DefaultIfEmpty()
                              join ur in db.UserReportings on u.UserID equals ur.UserID
                              where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DepartmentId == Model.DepartmentID && u.DateOfJoin <= CUR)
                              //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                              select new DSRCManagementSystem.Models.Department()
                              {
                                  DPID = u.DepartmentId,
                                  GPID = u.DepartmentGroup,
                                  DepartmentID = p.ObservationID,
                                  DepartmentName = y2.DepartmentName,
                                  OBUserName = u.FirstName + " " + u.LastName,
                                  ActivityDate = (DateTime)p.Date,
                                  GroupID = p.UserId,
                                  GroupName = y1.GroupName,
                                  Activity = a.Activity1,
                                  ActivityLevel = ac.ActivityLevel1,
                                  Comment = p.Comments,
                                  SelectedUserStatusid = u.UserStatus,
                                  Users = u.UserID
                              }).ToList();

                    MainResult = (from u in db.Users
                                  join ur in db.UserReportings on u.UserID equals ur.UserID
                                  where (u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DepartmentId == Model.DepartmentID && u.DateOfJoin <= CUR)


                                  select new DSRCManagementSystem.Models.Department()
                                  {
                                      DPID = u.DepartmentId,
                                      GPID = u.DepartmentGroup,
                                      OBUserName = u.FirstName + " " + u.LastName,
                                      SelectedUserStatusid = u.UserStatus,
                                      Users = u.UserID
                                  }).ToList();



                    ViewBag.DepartmentID = Model.DepartmentID;
                }
                else if (Model.DepartmentID != 0 && Model.UID != 0)
                {
                    //List<DSRCManagementSystem.Models.Department> Result = new List<DSRCManagementSystem.Models.Department>();
                    Result = (from p in db.Observations
                              join a in db.Activities on p.ActivityID equals a.ActivityID
                              join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                              join u in db.Users on p.UserId equals u.UserID
                              join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                              from y1 in yr.DefaultIfEmpty()
                              join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                              from y2 in x.DefaultIfEmpty()
                              join ur in db.UserReportings on u.UserID equals ur.UserID
                              where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DepartmentId == Model.DepartmentID && u.DepartmentGroup == Model.UID && u.DateOfJoin <= CUR)
                              //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1)
                              select new DSRCManagementSystem.Models.Department()
                              {
                                  DPID = u.DepartmentId,
                                  GPID = u.DepartmentGroup,
                                  DepartmentID = p.ObservationID,
                                  DepartmentName = y2.DepartmentName,
                                  OBUserName = u.FirstName + " " + u.LastName,
                                  ActivityDate = (DateTime)p.Date,
                                  GroupID = p.UserId,
                                  GroupName = y1.GroupName,
                                  Activity = a.Activity1,
                                  ActivityLevel = ac.ActivityLevel1,
                                  Comment = p.Comments,
                                  SelectedUserStatusid = u.UserStatus,
                                  Users = u.UserID
                              }).ToList();










                    MainResult = (from u in db.Users
                                  join ur in db.UserReportings on u.UserID equals ur.UserID
                                  where (u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == Model.Idbranchname1 && u.DepartmentId == Model.DepartmentID && u.DepartmentGroup == Model.UID && u.DateOfJoin <= CUR)


                                  select new DSRCManagementSystem.Models.Department()
                                  {
                                      DPID = u.DepartmentId,
                                      GPID = u.DepartmentGroup,
                                      OBUserName = u.FirstName + " " + u.LastName,
                                      SelectedUserStatusid = u.UserStatus,
                                      Users = u.UserID
                                  }).ToList();


                    ViewBag.DepartmentID = Model.DepartmentID;
                    ViewBag.UID = Model.UID;
                }


               

                DepartmentValue = Result.Where(x => x.ActivityDate == CUR).ToList();
                 ViewBag.ActivityDate = 1;
                       
            }

            //List<int> Checks = new List<int>();
            //for (int i = 0; i < MainResult.Count(); i++)
            //{
            //    Checks.Add(MainResult[i].Users); 
            //}

            //for (int i = 0; i < Checks.Count(); i++)
            //{
            //    int j = Checks[i];

            //    Uservalue = DepartmentValue.Where(x => x.Users != j).ToList();
               

            //}

            foreach (var item in DepartmentValue)
            {
                objmodel.Add(item);
            }

                     foreach (var item in MainResult )
                  {

                      var res = DepartmentValue.Where(x => x.Users == item.Users).Select(o => o.Users).FirstOrDefault();
                      if (res ==0)
                      {
                          objmodel.Add(item);
                      }


                  }
  

                    
                




            var Activity = db.Activities.Where(x => x.IsActive == true).Select(c => new
            {
                ActivityName = c.Activity1,
                ActivityId = c.ActivityID
            }).OrderBy(x=>x.ActivityName).ToList();

            ViewBag.Activity = new SelectList(Activity, "ActivityId", "ActivityName");

            var ActivityLevel = db.ActivityLevels.Where(x => x.IsActive == true).Select(c => new
            {
                LevelOrders = c.LevelOrder,
                ActivityLevelName = c.ActivityLevel1,
                ActivityLevelId = c.ActivityLevelID
            }).OrderBy(o => o.LevelOrders).ToList();

            ViewBag.ActivityLevel = new SelectList(ActivityLevel, "ActivityLevelId", "ActivityLevelName");

            var GetBran = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  UserId = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "UserId", "Name", Model.Idbranchname1);

            if (Model.Idbranchname1 != null)
            {
                var Department = db.Departments.Where(o => o.IsActive == true && o.BranchID == Model.Idbranchname1).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(x => x.DepartmentName).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

            }
            else
            {

                var Department = db.Departments.Where(o => o.IsActive == true && o.BranchID == GetBranch).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(x => x.DepartmentName).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");



            }

            //var Group = db.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
            //{
            //    GroupId = c.GroupID,
            //    GroupName = c.GroupName
            //}).ToList();
            //ViewBag.Group = new SelectList(Group, "GroupId", "GroupName");

            var Group = (from d in db.Departments
                         join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                         join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                         where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Model.DepartmentID
                         select new DSRCEmployees
                         {
                             Name = dg.GroupName,
                             UserId = dg.GroupID,

                         }).ToList();
            ViewBag.Group = new SelectList(Group, "UserId", "Name");

            return View(objmodel);
        }


        [HttpPost]
        public ActionResult Addobservation(DSRCManagementSystem.Models.Department Model, string Column, string Column1, string Column2, string Column3)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");

                DateTime ActiveDate = Model.ActivityDate;
                //var check = db.Observations.Where(r => r.Date == ActiveDate).Select(f => f.ObservationID).ToList();

                //foreach (var x in check)
                //{
                //    if (x != 0)
                //    {
                //        return Json("Warning", JsonRequestBehavior.AllowGet);
                //    }
                //}

                int userId = int.Parse(Session["UserID"].ToString());
                List<string> objuser = new List<string>();
                string[] value = Column.Split(',');
                for (int k = 0; k < value.Count(); k++)
                {
                    if (value[k] != "")
                    {
                        objuser.Add(value[k].Replace(",", "''"));
                    }
                }

                List<string> objusers = new List<string>();
                string[] values = Column1.Split(',');
                for (int k = 0; k < values.Count(); k++)
                {
                    if (values[k] != "")
                    {
                        objusers.Add(values[k].Replace(",", "''"));
                    }
                }


                List<string> objuserColumn = new List<string>();
                string[] valuesColumn = Column2.Split(',');
                for (int k = 0; k < valuesColumn.Count(); k++)
                {
                    if (valuesColumn[k] != "")
                    {
                        objuserColumn.Add(valuesColumn[k].Replace(",", "''"));
                    }
                    if (valuesColumn[k] == "")
                    {
                        objuserColumn.Add(valuesColumn[k]);
                    }
                }


                List<string> USER = new List<string>();
                string[] valuesUSER = Column3.Split(',');
                for (int k = 0; k < valuesUSER.Count(); k++)
                {
                    if (valuesUSER[k] != "")
                    {
                        USER.Add(valuesUSER[k].Replace(",", "''"));
                    }
                }
                //List<string> USER = new List<string>();
                //string[] valuesUSER = Column4.Split(',');
                //for (int k = 0; k < valuesUSER.Count(); k++)
                //{
                //    if (valuesUSER[k] != "")
                //    {
                //        USER.Add(valuesUSER[k].Replace(",", "''"));
                //    }
                //}



                for (int i = 0; i < objuser.Count(); i++)
                {
                    var Assignobj = db.Observations.CreateObject();
                    Assignobj.ActivityID = Convert.ToInt32(objuser[i]);
                    for (int j = i; j < i + 1; j++)
                    {
                        Assignobj.ActivityLevelID = Convert.ToInt32(objusers[j]);
                        for (int k = j; k < j + 1; k++)
                        {

                            Assignobj.Comments = objuserColumn[k];

                            for (int l = k; l < k + 1; l++)
                            {
                                Assignobj.Date = Model.ActivityDate;

                                for (int m = l; m < l + 1; m++)
                                {
                                    Assignobj.UserId = Convert.ToInt32(USER[m]);
                                }
                            }
                        }
                    }

                    Assignobj.IsActive = true;
                    db.Observations.AddObject(Assignobj);
                    db.SaveChanges();
                }
                /////////////////////////////
                var createdby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();




                for (int e = 0; e < objuser.Count(); e++)
                {
                    int ActivityID = Convert.ToInt32(objuser[e]);
                    for (int j = e; j < e + 1; j++)
                    {
                        int ActivityLevelID = Convert.ToInt32(objusers[j]);
                        for (int k = j; k < j + 1; k++)
                        {
                            var Comments = objuserColumn[k];
                            for (int l = k; l < k + 1; l++)
                            {
                                for (int m = l; m < l + 1; m++)
                                {
                                    int UserIds = Convert.ToInt32(USER[m]);


                                    int ConvertIDS = Convert.ToInt32(UserIds);
                                    var Getmailid = db.Users.Where(x => x.UserID == ConvertIDS).Select(o => o.EmailAddress).FirstOrDefault();
                                    var GetName = db.Users.Where(x => x.UserID == ConvertIDS).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                                    var ActivityName = db.Activities.Where(x => x.ActivityID == ActivityID).Select(o => o.Activity1).FirstOrDefault();
                                    var ActivityLevel = db.ActivityLevels.Where(x => x.ActivityLevelID == ActivityLevelID).Select(o => o.ActivityLevel1).FirstOrDefault();






                                    var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                                     .Select(o => o.AppValue)
                                     .FirstOrDefault();

                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                     var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Add Activity").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Add Activity").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {
                         var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Add Activity")
                                     join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                     select new DSRCManagementSystem.Models.Email
                                     {
                                         To = p.To,
                                         CC = p.CC,
                                         BCC = p.BCC,
                                         Subject = p.Subject,
                                         Template = q.TemplatePath
                                     }).FirstOrDefault();

                         string TemplatePath = Server.MapPath(obj1.Template);
                         string html = System.IO.File.ReadAllText(TemplatePath);

                         //string Title = " " + objcom + "   calendar event Created";
                         string Subject = "Activity Created on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal-New Activity Created";


                         html = html.Replace("#UserName", GetName);
                         html = html.Replace("#ActivityName", ActivityName);
                         html = html.Replace("#Date", Model.ActivityDate.ToString("dd MMM yyyy"));
                         html = html.Replace("#Createdby", createdby);
                         html = html.Replace("#ActivityLevel", ActivityLevel);
                         html = html.Replace("#Comments", Comments);
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName", ServerName);
                         List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                         string EmailAddress = "";

                         foreach (string mail in MailIds)
                         {
                             EmailAddress += mail + ",";
                         }
                         EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                         if (ServerName  != "http://win2012srv:88/")
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));

                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Add Activity", folder, ServerName);
                     }
                                }
                            }
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


            return Json("Success", JsonRequestBehavior.AllowGet);

        }

        public ActionResult Delete(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                int userId = int.Parse(Session["UserID"].ToString());
                var data = db.Observations.Where(o => o.ObservationID == Id).Select(o => o).FirstOrDefault();
                data.IsActive = false;
                db.SaveChanges();

                ///////////////////////

                var getActID = db.Observations.Where(x => x.ObservationID == Id).Select(o => o.ActivityID).FirstOrDefault();
                var getLevID = db.Observations.Where(x => x.ObservationID == Id).Select(o => o.ActivityLevelID).FirstOrDefault();
                var getuserids = db.Observations.Where(x => x.ObservationID == Id).Select(o => o.UserId).FirstOrDefault();
                var ActivityNameEmail = db.Activities.Where(c => c.ActivityID == getActID).Select(o => o.Activity1).FirstOrDefault();
                var ActivityLevelEmail = db.ActivityLevels.Where(x => x.ActivityLevelID == getLevID).Select(o => o.ActivityLevel1).FirstOrDefault();
                var createdby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
                var getcom = db.Observations.Where(u => u.ObservationID == Id).Select(o => o.Comments).FirstOrDefault();
                var getdate = db.Observations.Where(u => u.ObservationID == Id).Select(o => o.Date).FirstOrDefault();
                var Getmailid = db.Users.Where(x => x.UserID == getuserids).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = db.Users.Where(x => x.UserID == getuserids).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Activity").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Delete Activity").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Activity")
                                     join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                     select new DSRCManagementSystem.Models.Email
                                     {
                                         To = p.To,
                                         CC = p.CC,
                                         BCC = p.BCC,
                                         Subject = p.Subject,
                                         Template = q.TemplatePath
                                     }).FirstOrDefault();

                         string TemplatePath = Server.MapPath(obj1.Template);
                         string html = System.IO.File.ReadAllText(TemplatePath);

                         //string Title = " " + objcom + "   calendar event Created";
                         string Subject = "Activity Created on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal-New Activity Deleted";


                         html = html.Replace("#UserName", GetName);
                         html = html.Replace("#ActivityName", ActivityNameEmail);
                         html = html.Replace("#Date", getdate.ToString().Substring(0, 10));
                         html = html.Replace("#Deletedby", createdby);
                         html = html.Replace("#ActivityLevel", ActivityLevelEmail);
                         html = html.Replace("#Comments", getcom);
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName",ServerName);
                         List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                         string EmailAddress = "";

                         foreach (string mail in MailIds)
                         {
                             EmailAddress += mail + ",";
                         }

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", "aravind.a@dsrc.co.in", Server.MapPath(pathvalue.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));

                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Delete Activity", folder, ServerName);
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
        public ActionResult GetAvailEmployees(int DepartmentName, int BranchID)
        {

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

            if (DepartmentName != 0)
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                //Employees = db.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
                //{
                //    GroupId = c.GroupID,
                //    GroupName = c.GroupName
                //}).OrderBy(x => x.GroupName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });

                Employees = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DepartmentName && d.BranchID == BranchID
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

            }
            return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

      


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDepartments(int BranchId)
        {

            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();

            if (BranchId != 0)
            {

                List<int> validDepart = new List<int>();

                validDepart = db.Departments.Where(d => d.BranchID == BranchId && d.IsActive == true).Select(d => d.DepartmentId).ToList();

                FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.DepartmentId,
                                    DepartmentName = lt.DepartmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }

            return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Data(int Id)
        {

            var list = new List<object>();

            //var currentmonth = DateTime.Now.Month;
            //var currentyear = DateTime.Now.Year;
            //for (int i = 1; i <= currentmonth; i++)
            //{
            //    var Data = db.AuditLogs.Where(a => a.LogedInDate.Value.Month == i && a.LogedInDate.Value.Year == currentyear).Select(d => d.LogedInDate.Value.Month).Count();
            //    var Val = new { m = currentyear + "-" + i, a = Data };
            //    list.Add(Val);
            //}
            try
            {
                var GetBranch = db.Users.Where(x => x.UserID == Id).Select(o => o.BranchId).FirstOrDefault();
                List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
                List<DSRCManagementSystem.Models.Department> Value = new List<DSRCManagementSystem.Models.Department>();
                Value = (from p in db.Observations
                         join a in db.Activities on p.ActivityID equals a.ActivityID
                         join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                         join u in db.Users on p.UserId equals u.UserID
                         join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                         from y1 in yr.DefaultIfEmpty()
                         join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                         from y2 in x.DefaultIfEmpty()
                         where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserStatus != 6 && u.BranchId == GetBranch && u.UserID == Id)


                         select new DSRCManagementSystem.Models.Department()
                         {
                             DPID = u.DepartmentId,
                             GPID = u.DepartmentGroup,
                             DepartmentID = p.ObservationID,
                             DepartmentName = y2.DepartmentName,
                             OBUserName = y1.GroupName,
                             ActivityDate = (DateTime)p.Date,
                             GroupID = p.UserId,
                             GroupName = u.FirstName + "" + u.LastName,
                             Activity = a.Activity1,
                             ActivityLevelID = ac.ActivityLevelID,
                             ActivityLevel = ac.ActivityLevel1,
                             Comment = p.Comments,
                             SelectedUserStatusid = u.UserStatus,
                             ActivityID = ac.LevelOrder
                         }).ToList();
                foreach (var x in Value)
                {
                    var d = x.Activity;
                    string result = "";
                    if (d.Length >= 10)
                    {
                        result = d.Substring(0, 10);
                    }
                    else {

                        result = d;
                    }
                        var COUNT = db.ActivityLevels.Where(o => o.IsActive == true).Count();
                    var LEVEL = (COUNT + 1) - (x.ActivityID);
                    var Val = new { m = result, Date = x.ActivityDate.Day + "-" + x.ActivityDate.Month + "-" + x.ActivityDate.Year, Level = LEVEL, LevelName = x.ActivityLevel };
                    list.Add(Val);
                    objmodel.Add(x);
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


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUsers(int BranchId, int DepartmentName,int GroupName)
        {

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

            if (BranchId != 0 && DepartmentName == 0 && GroupName ==0)
            {

                Employees = (from u in db.Users 
                             where u.IsActive == true && u.UserStatus != 6 && u.BranchId == BranchId
                             select new DSRCEmployees
                             {
                                 Name = u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,

                             }).Distinct().AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });



            }

            if (BranchId != 0 && DepartmentName != 0 && GroupName == 0)
            {

                Employees = (from u in db.Users 
                             where u.IsActive == true && u.UserStatus != 6 && u.BranchId == BranchId &&u.DepartmentId==DepartmentName
                             select new DSRCEmployees
                             {
                                 Name = u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,

                             }).Distinct().AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });



            }

            if (BranchId != 0 && DepartmentName != 0 && GroupName != 0)
            {

                Employees = (from u in db.Users
                             where u.IsActive == true && u.UserStatus != 6 && u.BranchId == BranchId && u.DepartmentId == DepartmentName && u.DepartmentGroup==GroupName
                             select new DSRCEmployees
                             {
                                 Name = u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,

                             }).Distinct().AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });



            }

            return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult MyObservation(string ID)
        {

            int FID = Convert.ToInt32(ID);
            int U = int.Parse(Session["UserID"].ToString());

            int userId = 0;

            if ((FID == null) || (FID == 0))
            {
                userId = int.Parse(Session["UserID"].ToString());

            }
            else
            {
                userId = FID;

            }



           /* List<SelectListItem> obj = new List<SelectListItem>();
            //var Users = (from o in db.Observations
            //             join u in db.Users on o.UserId equals u.UserID
            //             join ur in db.UserReportings on u.UserID equals ur.UserID
            //             where u.IsActive == true && ur.ReportingUserID == U && u.UserStatus != 6 && o.IsActive == true
            //             select new DSRCEmployees
            //             {
            //                 Name = u.FirstName + " " + u.LastName,
            //                 UserId = u.UserID,

            //             }).Distinct().ToList();

            var Users = (from u in db.Users
                         where u.UserID == userId && u.IsActive == true && u.UserStatus != 6
                         select new DSRCEmployees
                         {
                             Name = u.FirstName + " " + u.LastName,
                             UserId = u.UserID,

                         }).Distinct().ToList();



            foreach (var x in Users)
            {
                var Text = x.Name;
                var value = x.UserId;
                obj.Add(new SelectListItem { Text = Text, Value = value.ToString() });
            }


            ViewBag.Years = new SelectList(obj, "Value", "Text");*/




            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();

            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
            List<DSRCManagementSystem.Models.Department> Value = new List<DSRCManagementSystem.Models.Department>();
            Value = (from p in db.Observations
                     join a in db.Activities on p.ActivityID equals a.ActivityID
                     join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                     join u in db.Users on p.UserId equals u.UserID
                     join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                     from y1 in yr.DefaultIfEmpty()
                     join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                     from y2 in x.DefaultIfEmpty()
                
                     //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == GetBranch)
                     where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserID == userId && u.UserStatus != 6 && u.BranchId == GetBranch)
                  
                     select new DSRCManagementSystem.Models.Department()
                     {
                         DPID = u.DepartmentId,
                         GPID = u.DepartmentGroup,
                         DepartmentID = p.ObservationID,
                         DepartmentName = y2.DepartmentName,
                         OBUserName = y1.GroupName,
                         ActivityDate = (DateTime)p.Date,
                         GroupID = p.UserId,
                         GroupName = u.FirstName + "" + u.LastName,
                         Activity = a.Activity1,
                         ActivityLevel = ac.ActivityLevel1,
                         Comment = p.Comments,
                         SelectedUserStatusid = u.UserStatus
                     }).OrderBy(o=>o.ActivityDate).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
            }
            ViewData["LoginIDs"] = userId;
            return View(objmodel);


        }


        [HttpGet]
        public ActionResult UsersObservation(string ID)
        {

            int FID = Convert.ToInt32(ID);
            int U = int.Parse(Session["UserID"].ToString());

            int userId = 0;

            if ((FID == null) || (FID == 0))
            {
                userId = int.Parse(Session["UserID"].ToString());

            }
            else
            {
                userId = FID;

            }
            
            List<SelectListItem> obj = new List<SelectListItem>();
            var Users = (from o in db.Observations
                         join u in db.Users on o.UserId equals u.UserID
                         join ur in db.UserReportings on u.UserID equals ur.UserID
                         where u.IsActive == true && ur.ReportingUserID == U && u.UserStatus != 6 && o.IsActive == true
                         select new DSRCEmployees
                         {
                             Name = u.FirstName + " " + u.LastName,
                             UserId = u.UserID,

                         }).Distinct().ToList();
            

            foreach (var x in Users)
            {
                var Text = x.Name;
                var value = x.UserId;
                obj.Add(new SelectListItem { Text = Text, Value = value.ToString() });
            }


            ViewBag.Years = new SelectList(obj, "Value", "Text");


            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();

            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
            List<DSRCManagementSystem.Models.Department> Value = new List<DSRCManagementSystem.Models.Department>();
            Value = (from p in db.Observations
                     join a in db.Activities on p.ActivityID equals a.ActivityID
                     join ac in db.ActivityLevels on p.ActivityLevelID equals ac.ActivityLevelID
                     join u in db.Users on p.UserId equals u.UserID
                     join dg in db.DepartmentGroups on u.DepartmentGroup equals dg.GroupID into yr
                     from y1 in yr.DefaultIfEmpty()
                     join md in db.Departments on u.DepartmentId equals md.DepartmentId into x
                     from y2 in x.DefaultIfEmpty()

                     //where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 && u.BranchId == GetBranch)
                     where (y2.IsActive == true && p.IsActive == true && u.IsActive == true && u.UserID == userId && u.UserStatus != 6 && u.BranchId == GetBranch)

                     select new DSRCManagementSystem.Models.Department()
                     {
                         DPID = u.DepartmentId,
                         GPID = u.DepartmentGroup,
                         DepartmentID = p.ObservationID,
                         DepartmentName = y2.DepartmentName,
                         OBUserName = y1.GroupName,
                         ActivityDate = (DateTime)p.Date,
                         GroupID = p.UserId,
                         GroupName = u.FirstName + "" + u.LastName,
                         Activity = a.Activity1,
                         ActivityLevel = ac.ActivityLevel1,
                         Comment = p.Comments,
                         SelectedUserStatusid = u.UserStatus
                     }).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
            }
            ViewData["LoginIDs"] = userId;
            return View(objmodel);
        }



        [HttpGet]
        public ActionResult GetYear(string Year)
        {
            int year = 1;
            if (Year != "")
            {
                year = Convert.ToInt32(Year);
            }

            return Json(year, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetData(string GetData)
        {




            return Json(GetData, JsonRequestBehavior.AllowGet);
        }


    }



}

