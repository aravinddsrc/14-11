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
using NPOI.DDF;
using NPOI.SS.Formula.Functions;

namespace DSRCManagementSystem.Controllers
{
    public class ManageAssessmentController : Controller
    {
        
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult ManageAssessment()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            List<DSRCManagementSystem.Models.ManageAssessment> objmodel = new List<Models.ManageAssessment>();
            try
            {
            List<ManageAssessment> Value = new List<ManageAssessment>();
            Value = (from a in db.Assessments
                     where (a.IsActive == true)
                     select new ManageAssessment()
                     {
                         AssessmentID = a.AssessmentID,
                         AssessmentName = a.AssessmentName,
                         AssessmentDescription = a.AssessmentDescription,
                         AssessmentDate = (DateTime)a.AssessmentDate,
                         TotalScore = a.TotalScore,
                         PassingScore = a.PassScore
                     }).OrderByDescending(o => o.AssessmentDate).ToList();
            foreach (var x in Value)
            {
                objmodel.Add(x);
            }
            int userId = int.Parse(Session["UserID"].ToString());
            var GetBran = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            var Getdept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            var BranchList = (from d in db.Master_Branches
                                    select new DSRCEmployees
                                    {
                                        Name = d.BranchName,
                                        UserId = d.BranchID,

                                    }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "UserId", "Name", GetBran);
           
            var DepartmentIdList = (from d in db.Departments
                                    where d.IsActive == true && d.BranchID == GetBran
                                    select new DSRCEmployees
                                    {
                                        Name = d.DepartmentName,
                                        UserId = d.DepartmentId,

                                    }).OrderBy(o =>o.Name).ToList();
            ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "UserId", "Name");




            var Group = (from d in db.Departments
                         join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                         join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                         where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Getdept
                         select new DSRCEmployees
                         {
                             Name = dg.GroupName,
                             UserId = dg.GroupID,

                         }).OrderBy(o => o.Name).ToList();
            ViewBag.Groups = new SelectList("", "UserId", "Name");
          
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult ManageAssessment(ManageAssessment Model, FormCollection form)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            List<DSRCManagementSystem.Models.ManageAssessment> objmodel = new List<Models.ManageAssessment>();
            List<ManageAssessment> Value = new List<ManageAssessment>();
            Value = (from a in db.Assessments
                     where (a.IsActive == true)
                     select new ManageAssessment()

                     {
                         AssessmentID = a.AssessmentID,
                         AssessmentName = a.AssessmentName,
                         AssessmentDescription = a.AssessmentDescription,
                         AssessmentDate = (DateTime)a.AssessmentDate,
                         TotalScore = a.TotalScore,
                         PassingScore = a.PassScore,
                         Idbranchname = a.BranchID,
                         Iddepartment = a.DepartmentID,
                         Idgroup = a.GroupID
                     }).OrderBy(o => o.AssessmentName).ToList();
            foreach (var x in Value)
            {
                objmodel.Add(x);
            }

            if (Model.Idgroup1 == null)
            {
                Model.Idgroup1 = 0;
            
            }

            List<DSRCManagementSystem.Models.ManageAssessment> Result = new List<Models.ManageAssessment>();

            if (Model.Idbranchname1 != 0 && Model.Iddepartment1 == 0 && Model.Idgroup1 == 0)
            {

                Result = objmodel.Where(x => x.Idbranchname == Model.Idbranchname1).ToList();

            }
            if (Model.Idbranchname1 != 0 && Model.Iddepartment1 != 0 && Model.Idgroup1 == 0)
            {

                Result = objmodel.Where(x => x.Idbranchname == Model.Idbranchname1 && x.Iddepartment == Model.Iddepartment1).ToList();

            }
            if (Model.Idbranchname1 != 0 && Model.Iddepartment1 != 0 && Model.Idgroup1 != 0)
            {

                Result = objmodel.Where(x => x.Idbranchname == Model.Idbranchname1 && x.Iddepartment == Model.Iddepartment1 && x.Idgroup == Model.Idgroup1).ToList();


            }
            //var BranchList = db.Master_Branches.ToList();
            //var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
            //var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();
            //ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "--Select--" } }.Union(BranchList), "BranchID", "BranchName");
            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  UserId = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "UserId", "Name", Model.Idbranchname1);
            //ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "--Select--" } }.Union(DepartmentList), "DepartmentId", "DepartmentName", 0);
            //ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "--Select--" } }.Union(GroupList), "GroupID", "GroupName", 0);
             var DepartmentIdList = (from d in db.Departments
                                     where d.IsActive == true && d.BranchID == Model.Idbranchname1
                                            select new DSRCEmployees
                                            {
                                                Name = d.DepartmentName,
                                                UserId = d.DepartmentId,

                                            }).OrderBy(o=>o.Name).ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "UserId", "Name");
                
            
            
            
            var Group = (from d in db.Departments
                         join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                         join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                         where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Model.Iddepartment1
                         select new DSRCEmployees
                         {
                             Name = dg.GroupName,
                             UserId = dg.GroupID,

                         }).OrderBy(o=>o.Name).ToList();
            ViewBag.Groups = new SelectList(Group, "UserId", "Name");
            return View(Result);
        }

        [HttpGet]
        public ActionResult AddAssessment(ManageAssessment Model, FormCollection form)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            var GetBran = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            var GetDept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            //var AuthUsers = (from u in db.Users.Where(u => u.IsActive == true && u.DepartmentId == GetDept)

            //                 select new
            //                 {
            //                     userid = u.UserID,
            //                     username = u.FirstName + " " + u.LastName
            //                 }).ToList();
            //ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "userid", "username");

            //var FilteredUsers = db.Users.Where(u => u.IsActive == true && u.FirstName != null && u.LastName != null).Select(x => x.UserID).ToList().
            //    Except(db.ReportsPermissions.Where(ep => ep.IsAuthorized == true || ep.IsAuthorized.Value).Select(x => x.UserId.Value).ToList()).ToList();
            //List<object> UnAuthUsers = new List<object>();
            //foreach (int users in FilteredUsers)
            //{
            //    UnAuthUsers.AddRange(db.Users.Where(u => u.UserID == users).Select(u => new { userid = u.UserID, username = u.FirstName + " " + u.LastName }).ToList());
            //}
            //ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");
            var BranchList = db.Master_Branches.ToList();
            var DepartmentList = db.Departments.Where(d => d.IsActive == true).OrderBy(o=>o.DepartmentName).ToList();
            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).OrderBy(o=>o.GroupName).ToList();

            ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "--Select--" } }.Union(BranchList), "BranchID", "BranchName", GetBran);
            ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "--Select--" } }.Union(DepartmentList), "DepartmentId", "DepartmentName", 0);
            ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "--Select--" } }, "GroupID", "GroupName", 0);

            return View();
        }
        [HttpPost]
        public ActionResult AddAssessment(ManageAssessment Model)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            int userId = int.Parse(Session["UserID"].ToString());
            var GetDept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            //var GetGrp = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentGroup).FirstOrDefault();

            var GetUsersFromDept = new List<int>();
            if (Model.Idbranchname != 0 && Model.Iddepartment == 0 && Model.Idgroup == 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.BranchId == Model.Idbranchname && x.IsActive == true ).Select(o => o.UserID).OrderBy(o => o).ToList();
            }
            if (Model.Idbranchname == 0 && Model.Iddepartment != 0 && Model.Idgroup == 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.DepartmentId == Model.Iddepartment && x.IsActive == true ).Select(o => o.UserID).OrderBy(o => o).ToList();
            }
            if (Model.Idbranchname == 0 && Model.Iddepartment == 0 && Model.Idgroup != 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.DepartmentGroup == Model.Idgroup && x.IsActive == true ).Select(o => o.UserID).OrderBy(o => o).ToList();
            }


            if (Model.Idbranchname != 0 && Model.Iddepartment != 0 && Model.Idgroup == 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.BranchId == Model.Idbranchname && x.DepartmentId == Model.Iddepartment && x.IsActive == true ).Select(o => o.UserID).OrderBy(o => o).ToList();
            }
            if (Model.Idbranchname != 0 && Model.Iddepartment == 0 && Model.Idgroup != 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.BranchId == Model.Idbranchname && x.DepartmentGroup == Model.Idgroup && x.IsActive == true ).Select(o => o.UserID).OrderBy(o => o).ToList();
            }
            if (Model.Idbranchname == 0 && Model.Iddepartment != 0 && Model.Idgroup != 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.BranchId == Model.Iddepartment && x.DepartmentGroup == Model.Idgroup && x.IsActive == true && (x.FirstName != null || x.LastName != null)).Select(o => o.UserID).OrderBy(o => o).ToList();
            }


            if (Model.Idbranchname != 0 && Model.Iddepartment != 0 && Model.Idgroup != 0)
            {
                GetUsersFromDept = db.Users.Where(x => x.BranchId == Model.Idbranchname && x.DepartmentId == Model.Iddepartment && x.DepartmentGroup == Model.Idgroup && x.IsActive == true && (x.FirstName != null || x.LastName != null)).Select(o => o.UserID).OrderBy(o => o).ToList();
            }



            var AssessmentName = Model.AssessmentName.Trim();
            var AssessmentDate = Model.AssessmentDate;
            var TotalScore = Model.TotalScore;
            var PassingScore = Model.PassingScore;
            var AssessmentDescription = "";
            if (Model.AssessmentDescription != null)
            {
                AssessmentDescription = Model.AssessmentDescription.Trim();
            }
            var temp = db.Assessments.Where(x => x.AssessmentName == AssessmentName && x.IsActive == true).Select(o => o.AssessmentID).FirstOrDefault();

            if (temp != 0)
            {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            {
                var Assignobj = db.Assessments.CreateObject();
                Assignobj.AssessmentName = AssessmentName;
                Assignobj.AssessmentDescription = AssessmentDescription;
                Assignobj.AssessmentDate = AssessmentDate;
                Assignobj.TotalScore = TotalScore;
                Assignobj.PassScore = PassingScore;
                Assignobj.BranchID = Model.Idbranchname;
                Assignobj.DepartmentID = Model.Iddepartment;
                Assignobj.GroupID = Model.Idgroup;
                Assignobj.IsActive = true;
                db.Assessments.AddObject(Assignobj);
                db.SaveChanges();

                foreach (var DptUser in GetUsersFromDept)
                {
                    DSRCManagementSystem.UserAssessment obj = new DSRCManagementSystem.UserAssessment();
                    obj.AssessmentID = Assignobj.AssessmentID;
                    obj.UserID = DptUser;
                    obj.AssessmentStatus = 3;
                    obj.Score = 0;
                    obj.IsActive = true;
                    db.UserAssessments.AddObject(obj);
                    db.SaveChanges();
                }
            }
          
            var createdby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
            foreach (var eids in GetUsersFromDept)
            {
                var Getmailid = db.Users.Where(x => x.UserID == eids).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = db.Users.Where(x => x.UserID == eids).Select(o => o.FirstName+" "+o.LastName).FirstOrDefault();               

                var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();
              
               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Create Assessment").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Create Assessment").Select(x => x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {

                         var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Create Assessment")
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
                         string Subject = "Assessment Created on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal-New Assessment Created";


                         html = html.Replace("#UserName", GetName);
                         html = html.Replace("#AssesmentName", AssessmentName);
                         html = html.Replace("#Date", AssessmentDate.ToString("dd MMM yyyy"));
                         html = html.Replace("#Createdby", createdby);
                         html = html.Replace("#TotalScore", TotalScore.ToString());
                         html = html.Replace("#PassingScore", PassingScore.ToString());
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName",ServerName);
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

                         ExceptionHandlingController.TemplateMissing("Create Assessment", folder, ServerName);

                     }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AssessmentEntry(ManageAssessment Model, FormCollection form)
        {
            
            int BranchId = Convert.ToInt32(Model.Idbranchname);
            int DeptId = Convert.ToInt32(Model.Iddepartment);
            int GroupId = Convert.ToInt32(Model.Idgroup);
            int AsstId = Convert.ToInt32(Model.AssessmentID);
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            int AssessmentID = Convert.ToInt32(Model.AssessmentID);
            List<DSRCManagementSystem.Models.ManageAssessment> objmodel = new List<Models.ManageAssessment>();
            List<ManageAssessment> Value = new List<ManageAssessment>();


            if (AssessmentID == 0)
            {
                Value = (from us in db.UserAssessments
                         join u in db.Users on us.UserID equals u.UserID
                         join a in db.Assessments on us.AssessmentID equals a.AssessmentID
                         join ur in db.UserReportings on u.UserID equals ur.UserID
                         where ( u.IsActive == true && us.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 &&a.IsActive==true)
                         select new ManageAssessment()
                         {
                             AssessmentName = a.AssessmentName,
                             AssessmentDate = (DateTime)a.AssessmentDate,
                             UserAssessmentID = us.UserAssessmentID,
                             TotalScore = a.TotalScore,
                             PassingScore = a.PassScore,
                             Idbranchname = u.BranchId,
                             Iddepartment = u.DepartmentId,
                             Idgroup = u.DepartmentGroup,
                             UserID = u.UserID,
                             AssessmentID = us.AssessmentID,
                             UserName = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                             Attendance = us.AssessmentStatus,
                             Score = us.Score,
                             Status = null

                         }).OrderByDescending(o => o.AssessmentDate).ToList();
                foreach (var x in Value)
                {
                    objmodel.Add(x);
                }

            }
            if (AssessmentID != 0)
            {
                Value = (from us in db.UserAssessments
                         join u in db.Users on us.UserID equals u.UserID
                         join a in db.Assessments on us.AssessmentID equals a.AssessmentID
                         join ur in db.UserReportings on u.UserID equals ur.UserID
                         where ( u.IsActive == true && us.IsActive == true && ur.ReportingUserID == userId && u.UserStatus != 6 &&a.IsActive==true)


                         select new ManageAssessment()
                         {
                             AssessmentName = a.AssessmentName,
                             AssessmentDate = (DateTime)a.AssessmentDate,
                             UserAssessmentID = us.UserAssessmentID,
                             TotalScore = a.TotalScore,
                             PassingScore = a.PassScore,
                             Idbranchname = u.BranchId,
                             Iddepartment = u.DepartmentId,
                             Idgroup = u.DepartmentGroup,
                             UserID = u.UserID,
                             AssessmentID = us.AssessmentID,
                             UserName = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                             Attendance = us.AssessmentStatus,
                             Score = us.Score,
                             Status = null
                         }).OrderByDescending(o => o.AssessmentDate).ToList();
                foreach (var x in Value)
                {
                    objmodel.Add(x);
                }


            }
            var Attendance = db.Master_AssessmentStatus.Select(c => new
        {
            AssessmentID = c.AssessmentStatusID,
            AssessmentName = c.AssessmentStatus
        }).ToList();
            ViewBag.Attendance = new SelectList(Attendance, "AssessmentID", "AssessmentName");







         var AssessmentName = db.Assessments.Where(o=>o.IsActive==true && o.GroupID == GroupId).Select(c => new
       {
           AssessmentID = c.AssessmentID,
           AssessmentName = c.AssessmentName
       }).ToList();
            ViewBag.AssessmentName = new SelectList(AssessmentName, "AssessmentID", "AssessmentName",GroupId);

            var GetBran = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
            var BranchList = db.Master_Branches.ToList();
            var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();

           // ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 1 } }.Union(BranchList), "BranchID", "BranchName", GetBran);
            ViewBag.BranchList = new SelectList(BranchList, "BranchId", "BranchName", GetBran);
            //ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "--Select--" } }.Union(DepartmentList), "DepartmentId", "DepartmentName", 0);
            //ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "--Select--" } }, "GroupID", "GroupName", 0);


            List<DSRCManagementSystem.Models.ManageAssessment> Result = new List<Models.ManageAssessment>();
            List<int> Ulist = new List<int>();
            var USERID = db.UserAssessments.Where(x => x.IsActive == true).Select(o => o.UserID).ToList();
            //List<int> pass = new List<int>();

            Model.Idbranchname=Model.Idbranchname == 0 ? null : Model.Idbranchname;
            Model.Iddepartment = Model.Iddepartment == 0 ? null : Model.Iddepartment;
            Model.Idgroup = Model.Idgroup == 0 ? null : Model.Idgroup;
            Model.AssessmentID = Model.AssessmentID == 0 ? null : Model.AssessmentID;
          
            if (Model.Idbranchname != null && Model.Iddepartment == null && Model.Idgroup == null &&
                    Model.AssessmentID == null)
                {
                    Result =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname)
                            .OrderBy(x => x.AssessmentDate)
                            .ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname).Select(o => o.Attendance).ToList();
                    ViewBag.UID = pass;


                }
                if (Model.Idbranchname != null && Model.Iddepartment != null && (Model.Idgroup == null) &&
                    Model.AssessmentID == null)
                {
                    Result =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment)
                            .OrderByDescending(x => x.AssessmentDate)
                            .ToList();
                    var pass = Result.Select(o => o.AssessmentID).Distinct().ToList();
                    ViewData["ID"] =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment)
                            .Select(o => o.Attendance)
                            .ToList();
                    ViewBag.UIDS = pass;

                }
                if (Model.Idbranchname != null && Model.Iddepartment != null && (Model.Idgroup != null) &&
                    Model.AssessmentID == null)
                {
                    Result =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.Idgroup == Model.Idgroup).OrderByDescending(x => x.AssessmentDate).ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.Idgroup == Model.Idgroup).Select(o => o.Attendance).ToList();
                    ViewBag.UID = pass;

                }
                if (Model.Idbranchname != null && Model.Iddepartment == null && Model.Idgroup == null &&
                    Model.AssessmentID != null)
                {
                    Result =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname && x.AssessmentID == Model.AssessmentID)
                            .OrderByDescending(x => x.AssessmentDate)
                            .ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] =
                        objmodel.Where(x => x.Idbranchname == Model.Idbranchname && x.AssessmentID == Model.AssessmentID)
                            .Select(o => o.Attendance)
                            .ToList();
                    ViewBag.UID = pass;

                }
                if (Model.Idbranchname != null && Model.Iddepartment != null && Model.Idgroup == null &&
                    Model.AssessmentID != null)
                {
                    Result =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.AssessmentID == Model.AssessmentID).OrderByDescending(x => x.AssessmentDate).ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.AssessmentID == Model.AssessmentID).Select(o => o.Attendance).ToList();
                    ViewBag.UID = pass;

                }
                if (Model.Idbranchname != null && Model.Iddepartment != null && Model.Idgroup != null &&
                    Model.AssessmentID != null)
                {
                    Result =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.Idgroup == Model.Idgroup && x.AssessmentID == Model.AssessmentID)
                            .OrderByDescending(x => x.AssessmentDate)
                            .ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] =
                        objmodel.Where(
                            x =>
                                x.Idbranchname == Model.Idbranchname && x.Iddepartment == Model.Iddepartment &&
                                x.Idgroup == Model.Idgroup && x.AssessmentID == Model.AssessmentID)
                            .Select(o => o.Attendance)
                            .ToList();
                    ViewBag.UID = pass;

                }
                if (Model.Idbranchname == null && Model.Iddepartment == null && Model.Idgroup == null &&
                    Model.AssessmentID == null)
                {
                    Result =
                        objmodel.Where(x => x.Idbranchname == GetBran).OrderByDescending(x => x.AssessmentDate).ToList();
                    var pass = Result.Select(o => o.AssessmentID).FirstOrDefault();
                    var passvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.PassScore);
                    var totalvalue = db.Assessments.Where(x => x.AssessmentID == pass).Select(o => o.TotalScore);
                    ViewData["Pass"] = passvalue;
                    ViewData["ASSID"] = totalvalue;
                    ViewData["ID"] = objmodel.Where(x => x.Idbranchname == GetBran).Select(o => o.Attendance).ToList();
                    ViewBag.UID = pass;

                }
            
            if (Model.Idbranchname != null)
            {
                var DepartmentIdList = (from d in db.Departments
                    where d.IsActive == true && d.BranchID == Model.Idbranchname && d.BranchID == BranchId
                    select new DSRCEmployees
                    {
                        Name = d.DepartmentName,
                        UserId = d.DepartmentId,

                    }).OrderBy(o=>o.Name).ToList();
                ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "UserId", "Name",BranchId);
            }else
            {
               
                    var DepartmentIdList = (from d in db.Departments
                                            where d.IsActive == true 
                                            select new DSRCEmployees
                                            {
                                                Name = d.DepartmentName,
                                                UserId = d.DepartmentId,

                                            }).OrderBy(o=>o.Name).ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "UserId", "Name");
                
            
            
            }
            var Group = (from d in db.Departments
                         join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                         join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                         where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DeptId
                         select new DSRCEmployees
                         {
                             Name = dg.GroupName,
                             UserId = dg.GroupID,

                         }).OrderBy(o=>o.Name).ToList();
            ViewBag.Groups = new SelectList(Group, "UserId", "Name",DeptId);

            return View(Result);
        }

        [HttpPost]
        public ActionResult AssessmentEntry(ManageAssessment Model, string Column, string Column1, string Column2)
        {

            int AssessmentID = Convert.ToInt32(Model.AssessmentID);
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

            List<string> objuserss = new List<string>();
            string[] valuess = Column2.Split(',');
            for (int k = 0; k < valuess.Count(); k++)
            {
                if (valuess[k] != "")
                {
                    objuserss.Add(valuess[k].Replace(",", "''"));
                }
            }


            var userassessmentid = db.UserAssessments.Where(o => o.AssessmentID == Model.UID).Select(y => y.UserAssessmentID).ToList();

            for (int k = 0; k < objuserss.Count(); k++)
            {
                var ID = Convert.ToInt32(objuserss[k]);
                var x = db.UserAssessments.Where(o => o.UserAssessmentID == ID).FirstOrDefault();
                for (int i = 0; i < k + 1; i++)
                {
                    x.AssessmentStatus = Convert.ToInt32(objuser[i]);
                    for (int j = i; j < i + 1; j++)
                    {
                        x.Score = Convert.ToInt32(objusers[j]);
                    }

                }
                db.SaveChanges();
            }
            var Attendance = db.Master_AssessmentStatus.Select(c => new
            {
                AssessmentID = c.AssessmentStatusID,
                AssessmentName = c.AssessmentStatus
            }).ToList();
            ViewBag.Attendance = new SelectList(Attendance, "AssessmentID", "AssessmentName", AssessmentID);

            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult MyAssessment()
        {
            int userId = int.Parse(Session["UserID"].ToString());
            List<DSRCManagementSystem.Models.ManageAssessment> objmodel = new List<Models.ManageAssessment>();
            List<ManageAssessment> Value = new List<ManageAssessment>();
           //List<int?> AssessId = new List<int?>();
            Session["Test"] = db.UserAssessments.Select(o => o.AssessmentStatus).ToList();
           // int? dropdownid = 0;
            Value = (from us in db.UserAssessments
                     join u in db.Users on us.UserID equals u.UserID
                     join a in db.Assessments on us.AssessmentID equals a.AssessmentID
                     where ( u.IsActive == true && us.IsActive == true && u.UserID == userId)
                     select new ManageAssessment()
                     {

                         AssessmentName = a.AssessmentName,
                         AssessmentDate = (DateTime)a.AssessmentDate,
                         UserAssessmentID = us.UserAssessmentID,
                         TotalScore = a.TotalScore,
                         PassingScore = a.PassScore,
                         Idbranchname = u.BranchId,
                         Iddepartment = u.DepartmentId,
                         Idgroup = u.DepartmentGroup,
                         UserID = u.UserID,
                         AssessmentID = us.AssessmentID,
                         UserName = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                         Attendance = us.AssessmentStatus,
                         Score = us.Score,
                         Status = null
                     }).OrderByDescending(o => o.AssessmentDate).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
              // AssessId.Add(x.Attendance);

            }
            //var Purpose = (from us in db.UserAssessments
            //               join ms in db.Master_AssessmentStatus on us.UserID equals userId
            //               join a in db.Assessments on us.AssessmentID equals a.AssessmentID
            //               where (us.AssessmentID == a.AssessmentID && us.AssessmentStatus == ms.AssessmentStatusID)
            //               select new
            //               {
            //                   AssetmentId = us.AssessmentStatus,
            //                   Template = ms.AssessmentStatus
            //               }).ToList();

          //  ViewBag.Attendance = new SelectList(Purpose, "AssetmentId", "Template", AssessId);
            
           
            ViewData["ID"] = db.UserAssessments.Select(o => o.AssessmentStatus).ToList();
            return View(objmodel);
        }


       
               
        
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailEmployees(bool DepartmentName)
        {

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();
            int userId = int.Parse(Session["UserID"].ToString());
            var GetDept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            var GetGrp = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentGroup).FirstOrDefault();

            if (DepartmentName == true)
            {


                Employees = (from u in db.Users.Where(u => u.IsActive == true && u.DepartmentId == GetDept && u.DepartmentGroup == GetGrp)
                             select new DSRCEmployees
                             {
                                 Name = u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,

                             }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

            }
            if (DepartmentName == false)
            {


                Employees = (from u in db.Users.Where(u => u.IsActive == true && u.DepartmentId == GetDept)
                             select new DSRCEmployees
                             {
                                 Name = u.FirstName + " " + u.LastName,
                                 UserId = u.UserID,

                             }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetGroups(int DepartmentId)
        {
            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();

            if (DepartmentId != 0)
            {
                var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == DepartmentId).Select(d => d.GroupID).ToList();

                FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                               where lt.IsActive == true
                               select new FilterGroup()
                               {
                                   GroupId = lt.GroupID,
                                   GroupName = lt.GroupName
                               }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });
            }
            return Json(new SelectList(FilterGroup, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

 
            
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAssessmentName(int BranchId, int DepartmentId, int GroupId)
        {
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();

            if (BranchId != 0 && DepartmentId == 0 && GroupId==0)
            {

                List<int> validDepart = new List<int>();


                FilterDepart = (from lt in db.Assessments.Where(o => o.BranchID == BranchId && o.IsActive==true)
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.AssessmentID,
                                    DepartmentName = lt.AssessmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }
            if (BranchId == 0 && DepartmentId != 0 && GroupId == 0)
            {

                List<int> validDepart = new List<int>();


                FilterDepart = (from lt in db.Assessments.Where(o => o.BranchID == DepartmentId && o.IsActive == true)
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.AssessmentID,
                                    DepartmentName = lt.AssessmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }
            if (BranchId != 0 && DepartmentId != 0 && GroupId == 0)
            {

                List<int> validDepart = new List<int>();



                FilterDepart = (from lt in db.Assessments.Where(o => o.BranchID == BranchId && o.DepartmentID == DepartmentId && o.IsActive == true)
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.AssessmentID,
                                    DepartmentName = lt.AssessmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }
            if (BranchId != 0 && DepartmentId != 0 && GroupId != 0)
            {

                List<int> validDepart = new List<int>();



                FilterDepart = (from lt in db.Assessments.Where(o => o.BranchID == BranchId && o.DepartmentID == DepartmentId &&  o.GroupID == GroupId && o.IsActive == true)
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.AssessmentID,
                                    DepartmentName = lt.AssessmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }
            return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditAssessment(int UID, string Name, string Desc, string date, int Total, int Pass)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            var Branch = db.Assessments.Where(x => x.AssessmentID == UID).Select(o => o.BranchID).FirstOrDefault();
            var Dept = db.Assessments.Where(x => x.AssessmentID == UID).Select(o => o.DepartmentID).FirstOrDefault();
            var Group = db.Assessments.Where(x => x.AssessmentID == UID).Select(o => o.GroupID).FirstOrDefault();
            var BranchName = db.Master_Branches.Where(x => x.BranchID == Branch).Select(o => o.BranchName).FirstOrDefault();
            var DeptName = db.Departments.Where(x => x.DepartmentId == Dept).Select(o => o.DepartmentName).FirstOrDefault();
            var GroupName = db.DepartmentGroups.Where(x => x.GroupID == Group).Select(o => o.GroupName).FirstOrDefault();
            ViewBag.BranchName = BranchName;
            ViewBag.DeptName = DeptName;
            ViewBag.GroupName = GroupName;


            //ViewBag.date = date;
            ViewBag.UID = UID;


            ManageAssessment Value = new ManageAssessment();
            Value = (from a in db.Assessments.Where(x => x.AssessmentID == UID)
                     select new ManageAssessment()
                     {
                         AssessmentID = a.AssessmentID,
                         AssessmentName = a.AssessmentName,
                         AssessmentDescription = a.AssessmentDescription,
                         AssessmentDate = (DateTime)a.AssessmentDate,
                         TotalScore = a.TotalScore,
                         PassingScore = a.PassScore
                     }).OrderBy(o => o.AssessmentName).FirstOrDefault();

            Value.Branch = BranchName;
            Value.Department = DeptName;
            Value.Group = GroupName;



            return View(Value);
        }

        [HttpPost]
        public ActionResult EditAssessment(ManageAssessment Model)
        {
            int userId = int.Parse(Session["UserID"].ToString());
            var GetDept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            int UID = Model.UID;
            ViewBag.UID = UID;
            var AssessmentName = Model.AssessmentName.Trim();
            var AssessmentDate = Model.AssessmentDate;
            var TotalScore = Model.TotalScore;
            var PassingScore = Model.PassingScore;
            var AssessmentDescription = "";
            if (Model.AssessmentDescription != null)
            {
                AssessmentDescription = Model.AssessmentDescription.Trim();
            }
            var temp = db.Assessments.Where(x => x.AssessmentName == AssessmentName && x.AssessmentID != UID && x.IsActive == true).Select(o => o.AssessmentID).FirstOrDefault();

            if (temp != 0)
            {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            {
                var Update = db.Assessments.Where(x => x.AssessmentID == UID).Select(o => o).FirstOrDefault();
                Update.AssessmentName = AssessmentName;
                Update.AssessmentDescription = AssessmentDescription;
                Update.AssessmentDate = AssessmentDate;
                Update.TotalScore = TotalScore;
                Update.PassScore = PassingScore;
                db.SaveChanges();


                var getdepart = db.Assessments.Where(x => x.AssessmentID == UID).Select(o => o.DepartmentID).FirstOrDefault();
                var GetUsersFromDept = db.UserAssessments.Where(x => x.AssessmentID == UID).Select(o => o.UserID).ToList();

                var Editedby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
                foreach (var eids in GetUsersFromDept)
                {
                    var Getmailid = db.Users.Where(x => x.UserID == eids).Select(o => o.EmailAddress).FirstOrDefault();
                    var GetName = db.Users.Where(x => x.UserID == eids).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();





                    var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                     .Select(o => o.AppValue)
                     .FirstOrDefault();

                   // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Edit Assessment").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Edit Assessment").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Edit Assessment")
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
                        string Subject = "Assessment Changed on " + DateTime.Today.ToString("dd MMM yyyy");
                        obj1.Subject = " " + objcom + " Management Portal- Assessment Changed";


                        html = html.Replace("#UserName", GetName);
                        html = html.Replace("#AssesmentName", AssessmentName);
                        html = html.Replace("#Date", AssessmentDate.ToString("dd MMM yyyy"));
                        html = html.Replace("#Createdby", Editedby);
                        html = html.Replace("#TotalScore", TotalScore.ToString());
                        html = html.Replace("#PassingScore", PassingScore.ToString());
                        html = html.Replace("#CompanyName", objcom.ToString());
                        html = html.Replace("#ServerName",ServerName);
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

                        ExceptionHandlingController.TemplateMissing("Edit Assessment", folder, ServerName);

                    }
                }


                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Delete(int Id)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var data = db.Assessments.Where(o => o.AssessmentID == Id).Select(o => o).FirstOrDefault();
            data.IsActive = false;
            db.SaveChanges();

            var deleteusersassement = db.UserAssessments.Where(x => x.AssessmentID == Id).Select(o => o.UserAssessmentID).ToList();

            foreach (var x in deleteusersassement)
            {
                var deleteuser = db.UserAssessments.Where(y => y.UserAssessmentID == x).Select(o => o).FirstOrDefault();
                deleteuser.IsActive = false;
                db.SaveChanges();
            }

            int userId = int.Parse(Session["UserID"].ToString());
            var GetDept = db.Users.Where(x => x.UserID == userId).Select(o => o.DepartmentId).FirstOrDefault();
            var AssessmentName = db.Assessments.Where(o => o.AssessmentID == Id).Select(o => o.AssessmentName).FirstOrDefault();
            var AssessmentDate = db.Assessments.Where(o => o.AssessmentID == Id).Select(o => o.AssessmentDate).FirstOrDefault();
            
            var GetUsersFromDept = db.UserAssessments.Where(x => x.AssessmentID == Id).Select(o => o.UserID).ToList();
            var Deletedby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
            foreach (var eids in GetUsersFromDept)
            {
                var Getmailid = db.Users.Where(x => x.UserID == eids).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = db.Users.Where(x => x.UserID == eids).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();





                var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Assessment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Delete Assessment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Assessment")
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
                         string todate = DateTime.Today.ToString("dd MMM yyyy");

                         //string Title = " " + objcom + "   calendar event Created";
                         string Subject = "Assessment Deleted on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal- Assessment Deleted";


                         html = html.Replace("#UserName", GetName);
                         html = html.Replace("#AssesmentName", AssessmentName);
                         html = html.Replace("#Date", AssessmentDate.ToString().Substring(0, 10));
                         html = html.Replace("#Cancelledby", Deletedby);
                         html = html.Replace("#Currentdate", todate);
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName",ServerName);
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

                         ExceptionHandlingController.TemplateMissing("Delete Assessment", folder, ServerName);

                     }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AssessmentEntryTO(int TotalScore, int UserAssessmentID, int Attendance)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            if (Attendance == 2)
            {
                var x =
                    db.UserAssessments.Where(o => o.UserAssessmentID == UserAssessmentID)
                        .Select(o => o)
                        .FirstOrDefault();
                x.AssessmentStatus = Attendance;
                x.Score = null;
                db.SaveChanges();
            }

            if (Attendance == 3)
                {
                    var x =
                        db.UserAssessments.Where(o => o.UserAssessmentID == UserAssessmentID)
                            .Select(o => o)
                            .FirstOrDefault();
                    x.AssessmentStatus = Attendance;
                    x.Score = null;
                    db.SaveChanges();

                }

            else
            {
                var x =
                    db.UserAssessments.Where(o => o.UserAssessmentID == UserAssessmentID)
                        .Select(o => o)
                        .FirstOrDefault();
                x.AssessmentStatus = Attendance;
                x.Score = TotalScore;
                db.SaveChanges();
            }

            //////////////////////
            var uAssessmentID = db.UserAssessments.Where(x => x.UserAssessmentID == UserAssessmentID).Select(o => o.AssessmentID).FirstOrDefault();
            var uAssessmentName = db.Assessments.Where(x => x.AssessmentID == uAssessmentID).Select(o => o.AssessmentName).FirstOrDefault();
            var uAssessmentDate = db.Assessments.Where(x => x.AssessmentID == uAssessmentID).Select(o => o.AssessmentDate).FirstOrDefault();
            var uTotalScore = db.Assessments.Where(x => x.AssessmentID == uAssessmentID).Select(o => o.TotalScore).FirstOrDefault();
            var uPassScore = db.Assessments.Where(x => x.AssessmentID == uAssessmentID).Select(o => o.PassScore).FirstOrDefault();
            var uUserID = db.UserAssessments.Where(x => x.UserAssessmentID == UserAssessmentID).Select(o => o.UserID).FirstOrDefault();
            var uUserName = db.Users.Where(x => x.UserID == uUserID).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var Getmailid = db.Users.Where(x => x.UserID == uUserID).Select(o => o.EmailAddress).FirstOrDefault();

            var PassORFail=" ";

            if (TotalScore >= uPassScore)
            {
                PassORFail = "Pass";

            }
            else
            {
                PassORFail = "Fail";
            
            }

            int userId = int.Parse(Session["UserID"].ToString());
            var Updatedby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();

                var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Update Assessment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Update Assessment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Update Assessment")
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
                         string Subject = "Assessment Updated on " + DateTime.Today.ToString("dd MMM yyyy");
                         obj1.Subject = " " + objcom + " Management Portal- Assessment Updated";


                         html = html.Replace("#UserName", uUserName);
                         html = html.Replace("#AssesmentName", uAssessmentName);
                         html = html.Replace("#Date", uAssessmentDate.ToString().Substring(0, 10));
                         html = html.Replace("#Createdby", Updatedby);
                         html = html.Replace("#TotalScore", uTotalScore.ToString());
                         html = html.Replace("#PassingScore", uPassScore.ToString());
                         html = html.Replace("#Scoreobtained", TotalScore.ToString());
                         html = html.Replace("#Status", PassORFail);
                         html = html.Replace("#CompanyName", objcom.ToString());
                         html = html.Replace("#ServerName",ServerName);

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

                         ExceptionHandlingController.TemplateMissing("Update Assessment", folder, ServerName);

                     }

            return Json("Success",JsonRequestBehavior.AllowGet);
        }

    }
}
