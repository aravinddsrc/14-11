using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Data.Entity;
using System.Web.Configuration;


namespace DSRCManagementSystem.Controllers
{
    public class HarwareSoftwareRequestController : Controller
    {
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        private static IQueryable<RequestComponent> GetRequestsQuery(IQueryable<RequestComponent> RequestQuery, int? RoleId, int? ReqStatusId, int? UserId)
        {
            
            if (RoleId == 4 || RoleId == 42)
            {
                if (ReqStatusId != null && ReqStatusId != 0)
                {
                    if (ReqStatusId == 1)
                    {
                        return RequestQuery.Where(item => (item.FirststageApprovalid == ReqStatusId || item.FirststageApprovalid == 2) && item.SecondstageApprovalid == ReqStatusId && item.AssignedToId == UserId);
                    }
                    else if (ReqStatusId == 3)
                    {
                        return RequestQuery.Where(item => (item.FirststageApprovalid == ReqStatusId || item.SecondstageApprovalid == ReqStatusId) && item.AssignedToId == UserId);
                    }
                    else
                        return RequestQuery.Where(item => item.FirststageApprovalid == ReqStatusId && item.AssignedToId == UserId);
                }
            }
            else if (RoleId == 30)
            {
                if (ReqStatusId != null && ReqStatusId != 0)
                {
                    return RequestQuery.Where(item => item.SecondstageApprovalid == ReqStatusId && item.NetworkingHead == UserId && item.FirststageApprovalid == 2);
                }
            }
            else
            {
                if (ReqStatusId == 2)
                {
                    return RequestQuery.Where(item => item.FirststageApprovalid == ReqStatusId && item.SecondstageApprovalid == ReqStatusId && item.userid == UserId);
                }
                else
                {
                    return RequestQuery.Where(item => (item.FirststageApprovalid == ReqStatusId || item.SecondstageApprovalid == ReqStatusId) && item.userid == UserId);
                }
            }
            return RequestQuery;
        }

        [HttpGet]
        public ActionResult ViewRequests()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            List<DSRCManagementSystem.Models.Assets> objmail = new List<DSRCManagementSystem.Models.Assets>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var ReqStatusList = db.Master_RequestStatus.ToList();
                ViewBag.ReqStatusList = new SelectList(new[] { new Master_RequestStatus() { RequestStatusId = 0, RequestStatus = "---Select---" } }.Union(ReqStatusList), "RequestStatusId", "RequestStatus", 0);


                int userId = int.Parse(Session["UserID"].ToString());
                int roleId = int.Parse(Session["RoleID"].ToString());

                if (roleId == 1 || roleId == 42)
                {
                    var result = from rc in db.RequestComponents
                                 join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                 join l in db.locations on rc.LocationId equals l.locationid
                                 join s in db.Status on rc.StatusId equals s.Statusid
                                 where rc.AssignedToId == userId && rc.FirststageApprovalid == 1 && rc.SecondstageApprovalid == 1
                                 select new Assets()
                                 {
                                     RequestedId = rc.RequestId,
                                     EmpName = rc.Employee,
                                     Description = rc.Description,
                                     DepartmentID = rc.DepartmentId,
                                     DepartmentName = d.DepartmentName,
                                     LocationID = rc.LocationId,
                                     Location = l.LocationName,
                                     StatusID = rc.StatusId,
                                     Status = s.StatusName,
                                     FirstStageApprovalID = rc.FirststageApprovalid,
                                     EmpID = rc.userid
                                 };

                    objmail = result.ToList();
                }
                else if (roleId == 30)
                {
                    var result = from rc in db.RequestComponents
                                 join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                 join l in db.locations on rc.LocationId equals l.locationid
                                 join s in db.Status on rc.StatusId equals s.Statusid
                                 where rc.FirststageApprovalid == 2 && rc.SecondstageApprovalid == 1 && rc.NetworkingHead == userId
                                 select new Assets()
                                 {
                                     RequestedId = rc.RequestId,
                                     EmpName = rc.Employee,
                                     Description = rc.Description,
                                     DepartmentID = rc.DepartmentId,
                                     DepartmentName = d.DepartmentName,
                                     LocationID = rc.LocationId,
                                     Location = l.LocationName,
                                     StatusID = rc.StatusId,
                                     Status = s.StatusName,
                                     FirstStageApprovalID = rc.FirststageApprovalid,
                                     SecondStageApprovalID = rc.SecondstageApprovalid,
                                     EmpID = rc.userid
                                 };

                    objmail = result.ToList();
                }
                else
                {
                    var result = from rc in db.RequestComponents
                                 join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                 join l in db.locations on rc.LocationId equals l.locationid
                                 join s in db.Status on rc.StatusId equals s.Statusid
                                 where rc.userid == userId && rc.FirststageApprovalid == 1
                                 select new Assets()
                                 {
                                     RequestedId = rc.RequestId,
                                     EmpName = rc.Employee,
                                     Description = rc.Description,
                                     DepartmentID = rc.DepartmentId,
                                     DepartmentName = d.DepartmentName,
                                     LocationID = rc.LocationId,
                                     Location = l.LocationName,
                                     StatusID = rc.StatusId,
                                     Status = s.StatusName,
                                     FirstStageApprovalID = rc.FirststageApprovalid,
                                     EmpID = rc.userid
                                 };

                    objmail = result.ToList();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View(objmail);
        }

        [HttpPost]
        public ActionResult ViewRequests(Assets model)
        {
            try
            {

                ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
                int userId = int.Parse(Session["UserID"].ToString());
                int roleId = int.Parse(Session["RoleID"].ToString());

                int ReqStatusSearchId = Convert.ToInt32(model.RequestStatusId) != 0 ? Convert.ToInt32(model.RequestStatusId) : 0;

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var RequestQuery = from rc in db.RequestComponents select rc;

                var ReqStatusList = db.Master_RequestStatus.ToList();

                ViewBag.ReqStatusList = new SelectList(new[] { new Master_RequestStatus() { RequestStatusId = 0, RequestStatus = "---Select---" } }.Union(ReqStatusList), "RequestStatusId", "RequestStatus", 0);

                if (roleId == 4 || roleId == 42)
                {
                    if (ReqStatusSearchId == 0)
                    {
                        var result = from rc in db.RequestComponents
                                     join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                     join l in db.locations on rc.LocationId equals l.locationid
                                     join s in db.Status on rc.StatusId equals s.Statusid
                                     where rc.AssignedToId == userId && rc.FirststageApprovalid == 1 && rc.SecondstageApprovalid == 1
                                     select new Assets()
                                     {
                                         RequestedId = rc.RequestId,
                                         EmpName = rc.Employee,
                                         Description = rc.Description,
                                         DepartmentID = rc.DepartmentId,
                                         DepartmentName = d.DepartmentName,
                                         LocationID = rc.LocationId,
                                         Location = l.LocationName,
                                         StatusID = rc.StatusId,
                                         Status = s.StatusName,
                                         FirstStageApprovalID = rc.FirststageApprovalid,
                                         EmpID = rc.userid
                                     };

                        return View(result.ToList());
                    }
                    else
                    {
                        var RequestsResult = GetRequestsQuery(RequestQuery, roleId, ReqStatusSearchId, userId).
                            Join(db.Departments, rc => rc.DepartmentId, d => d.DepartmentId, (rc, d) => new { RequestComponents = rc, Departments = d }).
                            Join(db.locations, rc => rc.RequestComponents.LocationId, l => l.locationid, (rc, l) => new { RequestComponents = rc, locations = l }).
                            Join(db.Status, rc => rc.RequestComponents.RequestComponents.StatusId, s => s.Statusid, (rc, s) => new { RequestComponents = rc, Status = s }).
                            Select(x => new DSRCManagementSystem.Models.Assets
                            {
                                RequestedId = x.RequestComponents.RequestComponents.RequestComponents.RequestId,
                                EmpName = x.RequestComponents.RequestComponents.RequestComponents.Employee,
                                Description = x.RequestComponents.RequestComponents.RequestComponents.Description,
                                DepartmentID = x.RequestComponents.RequestComponents.Departments.DepartmentId,
                                DepartmentName = x.RequestComponents.RequestComponents.Departments.DepartmentName,
                                LocationID = x.RequestComponents.locations.locationid,
                                Location = x.RequestComponents.locations.LocationName,
                                StatusID = x.Status.Statusid,
                                Status = x.Status.StatusName,
                                FirstStageApprovalID = x.RequestComponents.RequestComponents.RequestComponents.FirststageApprovalid,
                                EmpID = x.RequestComponents.RequestComponents.RequestComponents.userid
                            }).ToList();

                        return View(RequestsResult);
                    }
                }
                else if (roleId == 30)
                {
                    if (ReqStatusSearchId == 0)
                    {
                        var result = from rc in db.RequestComponents
                                     join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                     join l in db.locations on rc.LocationId equals l.locationid
                                     join s in db.Status on rc.StatusId equals s.Statusid
                                     where rc.FirststageApprovalid == 2 && rc.SecondstageApprovalid == 1 && rc.NetworkingHead == userId
                                     select new Assets()
                                     {
                                         RequestedId = rc.RequestId,
                                         EmpName = rc.Employee,
                                         Description = rc.Description,
                                         DepartmentID = rc.DepartmentId,
                                         DepartmentName = d.DepartmentName,
                                         LocationID = rc.LocationId,
                                         Location = l.LocationName,
                                         StatusID = rc.StatusId,
                                         Status = s.StatusName,
                                         FirstStageApprovalID = rc.FirststageApprovalid,
                                         SecondStageApprovalID = rc.SecondstageApprovalid,
                                         EmpID = rc.userid
                                     };

                        return View(result.ToList());
                    }
                    else
                    {
                        var RequestsResult = GetRequestsQuery(RequestQuery, roleId, ReqStatusSearchId, userId).
                           Join(db.Departments, rc => rc.DepartmentId, d => d.DepartmentId, (rc, d) => new { RequestComponents = rc, Departments = d }).
                           Join(db.locations, rc => rc.RequestComponents.LocationId, l => l.locationid, (rc, l) => new { RequestComponents = rc, locations = l }).
                           Join(db.Status, rc => rc.RequestComponents.RequestComponents.StatusId, s => s.Statusid, (rc, s) => new { RequestComponents = rc, Status = s }).
                           Select(x => new DSRCManagementSystem.Models.Assets
                           {
                               RequestedId = x.RequestComponents.RequestComponents.RequestComponents.RequestId,
                               EmpName = x.RequestComponents.RequestComponents.RequestComponents.Employee,
                               Description = x.RequestComponents.RequestComponents.RequestComponents.Description,
                               DepartmentID = x.RequestComponents.RequestComponents.Departments.DepartmentId,
                               DepartmentName = x.RequestComponents.RequestComponents.Departments.DepartmentName,
                               LocationID = x.RequestComponents.locations.locationid,
                               Location = x.RequestComponents.locations.LocationName,
                               StatusID = x.Status.Statusid,
                               Status = x.Status.StatusName,
                               FirstStageApprovalID = x.RequestComponents.RequestComponents.RequestComponents.FirststageApprovalid,
                               SecondStageApprovalID = x.RequestComponents.RequestComponents.RequestComponents.SecondstageApprovalid,
                               EmpID = x.RequestComponents.RequestComponents.RequestComponents.userid
                           }).ToList();

                        return View(RequestsResult);
                    }
                }
                else
                {
                    if (ReqStatusSearchId == 0)
                    {
                        var result = from rc in db.RequestComponents
                                     join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                                     join l in db.locations on rc.LocationId equals l.locationid
                                     join s in db.Status on rc.StatusId equals s.Statusid
                                     where rc.userid == userId && rc.FirststageApprovalid == 1
                                     select new Assets()
                                     {
                                         RequestedId = rc.RequestId,
                                         EmpName = rc.Employee,
                                         Description = rc.Description,
                                         DepartmentID = rc.DepartmentId,
                                         DepartmentName = d.DepartmentName,
                                         LocationID = rc.LocationId,
                                         Location = l.LocationName,
                                         StatusID = rc.StatusId,
                                         Status = s.StatusName,
                                         FirstStageApprovalID = rc.FirststageApprovalid,
                                         EmpID = rc.userid
                                     };

                        return View(result.ToList());
                    }
                    else
                    {
                        var RequestsResult = GetRequestsQuery(RequestQuery, roleId, ReqStatusSearchId, userId).
                           Join(db.Departments, rc => rc.DepartmentId, d => d.DepartmentId, (rc, d) => new { RequestComponents = rc, Departments = d }).
                           Join(db.locations, rc => rc.RequestComponents.LocationId, l => l.locationid, (rc, l) => new { RequestComponents = rc, locations = l }).
                           Join(db.Status, rc => rc.RequestComponents.RequestComponents.StatusId, s => s.Statusid, (rc, s) => new { RequestComponents = rc, Status = s }).
                           Select(x => new DSRCManagementSystem.Models.Assets
                           {
                               RequestedId = x.RequestComponents.RequestComponents.RequestComponents.RequestId,
                               EmpName = x.RequestComponents.RequestComponents.RequestComponents.Employee,
                               Description = x.RequestComponents.RequestComponents.RequestComponents.Description,
                               DepartmentID = x.RequestComponents.RequestComponents.Departments.DepartmentId,
                               DepartmentName = x.RequestComponents.RequestComponents.Departments.DepartmentName,
                               LocationID = x.RequestComponents.locations.locationid,
                               Location = x.RequestComponents.locations.LocationName,
                               StatusID = x.Status.Statusid,
                               Status = x.Status.StatusName,
                               FirstStageApprovalID = x.RequestComponents.RequestComponents.RequestComponents.FirststageApprovalid,
                               EmpID = x.RequestComponents.RequestComponents.RequestComponents.userid
                           }).ToList();

                        return View(RequestsResult);
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        public ActionResult CreateNewRequest()
        {
            int userId = int.Parse(Session["UserID"].ToString());
            int roleId = int.Parse(Session["RoleID"].ToString());
            DSRCManagementSystem.Models.Assets obj_Users = new DSRCManagementSystem.Models.Assets();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var LocationList = db.locations.ToList();
            var CategoryList = db.Master_Category.ToList();
            var StatusList = db.Status.ToList();
            var PriorityList = db.Master_Priority.ToList();

            //Assign Computer Name
            obj_Users.ComputerName = (from cm in db.computermanagements join ca in db.ComputerAssigneds on cm.managementid equals ca.Managementid where ca.Userid == userId select cm.ComputerName).FirstOrDefault();

            if (obj_Users.ComputerName != null)
            {
                try
                {

                    var ManagerList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 4 select new { UserID = u.UserID, FirstName = u.FirstName }).OrderBy(o => o.FirstName).ToList();

                    if (roleId == 30)
                    {
                        ManagerList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 42 select new { UserID = u.UserID, FirstName = u.FirstName }).OrderBy(o => o.FirstName).ToList();
                    }

                    var NetworkEmpList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 36 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();

                    ViewBag.LocList = new SelectList(new[] { new location() { locationid = 0, LocationName = "---Select---" } }.Union(LocationList), "locationid", "LocationName", 0);
                    ViewBag.CatList = new SelectList(new[] { new Master_Category() { Categoryid = 0, CategoryName = "---Select---" } }.Union(CategoryList), "Categoryid", "CategoryName", 0);
                    ViewBag.StatusList = new SelectList(StatusList, "Statusid", "StatusName", 0);
                    ViewBag.PriorList = new SelectList(new[] { new Master_Priority() { Priorityid = 0, PriorityName = "---Select---" } }.Union(PriorityList), "Priorityid", "PriorityName", 0);
                    ViewBag.MngrList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" } }.Union(ManagerList), "UserID", "FirstName", 0);
                    ViewBag.NworkEmpList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" } }.Union(NetworkEmpList), "UserID", "FirstName", 0);

                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
                return View(obj_Users);
            }
            else
                return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateNewRequest(Assets sendrequest)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = int.Parse(Session["UserID"].ToString());
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            if (ModelState.IsValid)
            {

                try
                {
                    sendrequest.FirstStageApprovalID = 1;
                    sendrequest.SecondStageApprovalID = 1;
                    sendrequest.RequestedDate = DateTime.Now;

                    var resutls = db.ComputerAssigneds.Where(x => x.Userid == userId).Select(m => m.Managementid).FirstOrDefault();

                    var objRequest = new RequestComponent()
                    {
                        userid = userId,
                        Description = sendrequest.Description,
                        DepartmentId = db.Users.FirstOrDefault(o => o.UserID == userId).DepartmentId,
                        LocationId = sendrequest.LocationID,
                        Employee = db.Users.FirstOrDefault(o => o.UserID == userId).FirstName,
                        NewrequestDate = sendrequest.RequestedDate,
                        ComputerId = resutls,
                        CategoryId = sendrequest.CategoryID,
                        StatusId = sendrequest.StatusID,
                        PriorityId = sendrequest.PriorityID,
                        AssignedToId = sendrequest.MngrID,
                        FirststageApprovalid = sendrequest.FirstStageApprovalID,
                        SecondstageApprovalid = sendrequest.SecondStageApprovalID,
                        ISDelete = false

                    };
                    db.RequestComponents.AddObject(objRequest);

                    db.SaveChanges();

                    sendrequest.RequestedId = objRequest.RequestId;
                    sendrequest.ReportingPersonName = db.Users.FirstOrDefault(o => o.UserID == sendrequest.MngrID).FirstName;
                    sendrequest.EmpName = db.Users.FirstOrDefault(o => o.UserID == userId).FirstName;
                    sendrequest.ReportingPersonEmail = db.Users.FirstOrDefault(o => o.UserID == sendrequest.MngrID).EmailAddress;
                    sendrequest.DepartmentID = db.Users.FirstOrDefault(o => o.UserID == userId).DepartmentId;
                    sendrequest.DepartmentName = db.Departments.FirstOrDefault(o => o.DepartmentId == sendrequest.DepartmentID).DepartmentName;


                    //string mailMessage = MailBuilder.SendhwswRequest(sendrequest);

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Sendhwsw Request").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Sendhwsw Request").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objSendhwswRequest = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Sendhwsw Request")
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
                        string TemplatePathSendhwswRequest = Server.MapPath(objSendhwswRequest.Template);
                        string htmlSendhwswRequest = System.IO.File.ReadAllText(TemplatePathSendhwswRequest);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#ReportingPersonName", sendrequest.ReportingPersonName);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#Description", sendrequest.Description);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#DepartmentName", sendrequest.DepartmentName);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#Location", sendrequest.Location);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#ComputerName", sendrequest.ComputerName);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#Category", sendrequest.Category);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#Status", sendrequest.Status);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#Priority", sendrequest.Priority);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#EmpName", sendrequest.EmpName);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#RequestedId", Encrypter.Encode(sendrequest.RequestedId.ToString()));
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#ServerName",ServerName);
                        htmlSendhwswRequest = htmlSendhwswRequest.Replace("#CompanyName", company);

                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                        var logo = CommonLogic.getLogoPath();


                        if (ServerName  != "http://win2012srv:88/")
                        {

                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();


                            //MailIds.Add("boobalan.k@dsrc.co.in");
                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                            //MailIds.Add("ramesh.S@dsrc.co.in");
                            //MailIds.Add("aruna.m@dsrc.co.in");
                            //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                            //MailIds.Add("kirankumar@dsrc.co.in");
                            //MailIds.Add("francispaul.k.c@dsrc.co.in");

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });

                        }
                        else
                        {

                            //Sending Mail For Hardware Software Request Submitted to the First Stage Manager.
                            Task.Factory.StartNew(() =>
                            {
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest.Subject, null, htmlSendhwswRequest, "admin@dsrc.co.in", sendrequest.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest.Subject, null, htmlSendhwswRequest, "admin@dsrc.co.in", sendrequest.ReportingPersonEmail, Server.MapPath(logo.ToString()));
                                //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been submited", null, mailMessage, "admin@dsrc.co.in", sendrequest.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png"));                        
                            });
                        }
                        return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Sendhwsw Request", folder, ServerName);
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    Console.WriteLine(Ex.Message);
                }
            }
            return null;
        }

        [HttpGet]
        public ActionResult ApproveHwSwRequest(int RequestId)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj_Request = new DSRCManagementSystem.Models.Assets();

                int RoleID = int.Parse(Session["RoleID"].ToString());

                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == RequestId).Select(o => o).FirstOrDefault();

                if (RoleID == 4 || RoleID == 42)
                {
                    ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;
                }
                else if (RoleID == 30)
                {
                    ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;
                }

                if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                {
                    obj_Request.RequestedId = RequestId;

                    var DepartmentList = db.Departments.ToList();
                    var LocationList = db.locations.ToList();
                    var CategoryList = db.Master_Category.ToList();
                    var StatusList = db.Status.ToList();
                    var PriorityList = db.Master_Priority.ToList();
                    var ApprovalStatusList = db.Master_RequestStatus.ToList();

                    var ManagerList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 4 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();
                    var EmpList = (from u in db.Users select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();
                    var NetworkEmpList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 36 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();

                    var RequestViewModel = (from rc in db.RequestComponents
                                            where rc.RequestId == RequestId
                                            select new Assets
                                            {
                                                RequestedId = rc.RequestId,
                                                Description = rc.Description,
                                                DepartmentID = rc.DepartmentId,
                                                LocationID = rc.LocationId,
                                                EmpName = rc.Employee,
                                                EmpID = rc.userid,
                                                RequestedDate = rc.NewrequestDate,
                                                ComputerID = rc.ComputerId,
                                                CategoryID = rc.CategoryId,
                                                StatusID = rc.StatusId,
                                                PriorityID = rc.PriorityId,
                                                ApprovalStatusID = rc.FirststageApprovalid,
                                                FirstStageApprovalID = rc.FirststageApprovalid,
                                                SecondStageApprovalID = rc.SecondstageApprovalid

                                            }).FirstOrDefault();


                    ViewBag.DeptList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", RequestViewModel.DepartmentID);
                    ViewBag.LocList = new SelectList(LocationList, "locationid", "LocationName", RequestViewModel.LocationID);
                    ViewBag.EmpList = new SelectList(EmpList, "UserID", "FirstName", RequestViewModel.EmpID);
                    ViewBag.CatList = new SelectList(CategoryList, "Categoryid", "CategoryName", RequestViewModel.CategoryID);
                    ViewBag.StatusList = new SelectList(StatusList, "Statusid", "StatusName", RequestViewModel.StatusID);
                    ViewBag.PriorList = new SelectList(PriorityList, "Priorityid", "PriorityName", RequestViewModel.PriorityID);
                    ViewBag.NworkEmpList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" } }.Union(NetworkEmpList), "UserID", "FirstName", 0);
                    ViewBag.ApprovalStatusList = new SelectList(ApprovalStatusList, "RequestStatusId", "RequestStatus", RequestViewModel.ApprovalStatusID);

                    RequestViewModel.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                    RequestViewModel.ComputerName = db.computermanagements.FirstOrDefault(o => o.managementid == RequestViewModel.ComputerID).ComputerName;
                    RequestViewModel.FirstStageApproveDate = DateTime.Now;

                    return View(RequestViewModel);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
        }


        //Sending Confirmation Mail For Hardware Software First Stage Approval When the Manager Clicks APPROVE button.
        [HttpPost]
        public ActionResult ApproveHwSwRequest(Assets model)
        {
            var Id = Convert.ToInt32(model.RequestedId);
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            if (ModelState.IsValid)
            {
                try
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                    model.FirstStageApprovalID = 2;
                    //currently the networking head only Sundar G Daniel and his user id is inserted for refernce
                    model.NetworkheadID = 10;

                    var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();
                    ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        RequestToUpdate.FirstStageApprovalDate = DateTime.Now;
                        RequestToUpdate.FirstStageApprovedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        RequestToUpdate.FirststageApprovalid = model.FirstStageApprovalID;
                        RequestToUpdate.AcceptedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        RequestToUpdate.FirststageApproval = model.FirstStageApproval;
                        RequestToUpdate.NetworkingHead = model.NetworkheadID;
                        db.SaveChanges();

                        string NetworkingMngrMailID = db.Users.FirstOrDefault(o => o.UserID == 10).EmailAddress;
                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        model.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        model.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                        var logo = CommonLogic.getLogoPath();
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objHwSwRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Approved")
                                                          join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                          select new DSRCManagementSystem.Models.Email
                                                          {
                                                              To = p.To,
                                                              CC = p.CC,
                                                              BCC = p.BCC,
                                                              Subject = p.Subject,
                                                              Template = q.TemplatePath
                                                          }).FirstOrDefault();
                            var Company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                            string TemplatePathHwSwRequestApproved = Server.MapPath(objHwSwRequestApproved.Template);
                            string htmlHwSwRequestApproved = System.IO.File.ReadAllText(TemplatePathHwSwRequestApproved);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#EmpName", RequestToUpdate.Employee);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Manager", model.MngrName);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Comments", RequestToUpdate.FirststageApproval);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ServerName", ServerName);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#CompanyName", Company);
                            //Approval Mail will been sent to Requested Person.
                            //string mailMessage = MailBuilder.HwSwRequestApproved(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, model.MngrName, model.Networkheadname, RequestToUpdate.FirststageApproval);





                            if (ServerName  != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();



                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                });

                            }
                            else
                            {


                                Task.Factory.StartNew(() =>
                                {
                                    // DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath("~/Content/Template/images/logo.png"));
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //var logo = CommonLogic.getLogoPath();
                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject, null, htmlHwSwRequestApproved, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath(logo.ToString()));

                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("HwSw Request Approved", folder, ServerName);
                        }

                        // Approval Mail will sent to Network head by Assigned TO Manager for Second stage Approval.

                        // string mailMessage1 = MailBuilder.SendhwswRequest_Stage2(model);


                        var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Sendhwsw Request_Stage2").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Sendhwsw Request_Stage2").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((checks != null) && (checks != 0))
                        {
                            var objSendhwswRequest_Stage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Sendhwsw Request_Stage2")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();

                            var companyname = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                            string TemplatePathSendhwswRequest_Stage2 = Server.MapPath(objSendhwswRequest_Stage2.Template);
                            string htmlSendhwswRequest_Stage2 = System.IO.File.ReadAllText(TemplatePathSendhwswRequest_Stage2);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Networkheadname", model.Networkheadname);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#MngrName", model.MngrName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Description", model.Description);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#DepartmentName", model.DepartmentName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Location", model.Location);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ComputerName", model.ComputerName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Category", model.Category);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Status", model.Status);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Priority", model.Priority);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#RequestedId", Encrypter.Encode(model.RequestedId.ToString()));
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ServerName", ServerName); 
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#CompanyName", companyname);


                            if (ServerName  != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    // var logo = CommonLogic.getLogoPath();
                                    //DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest_Stage2 + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest_Stage2 + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                });

                            }
                            else
                            {

                                Task.Factory.StartNew(() =>
                                {
                                    //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage1, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath("~/Content/Template/images/logo.png"));     
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    // var logo = CommonLogic.getLogoPath();
                                    //DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject, null, htmlSendhwswRequest_Stage2, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject, null, htmlSendhwswRequest_Stage2, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath(logo.ToString()));
                                });
                            }

                            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("Sendhwsw Request_Stage2", folders, ServerName);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

                }
            }
            return Json(new { Result = "Already Approved/Rejected/Cancelled" }, JsonRequestBehavior.AllowGet);
        }

        //Sending Confirmation Mail For Hardware Software Second Stage Approval When the Networking Head Clicks APPROVE button.
        [HttpPost]
        public ActionResult ApproveHwSwRequest_Stage2(Assets model)
        {
            //int userId = int.Parse(Session["UserID"].ToString());
            var Id = Convert.ToInt32(model.RequestedId);
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            Session["ServerName"]=ServerName;
            if (ModelState.IsValid)
            {
                try
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    DSRCManagementSystem.Models.Assets obj_Approve = new DSRCManagementSystem.Models.Assets();

                    model.SecondStageApprovalID = 2;
                    model.NetworkheadID = 10;

                    var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();
                    ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        RequestToUpdate.SecondStageApprovalDate = DateTime.Now;
                        RequestToUpdate.SecondStageApprovedBy = db.Users.FirstOrDefault(o => o.UserID == model.NetworkheadID).FirstName;
                        RequestToUpdate.SecondstageApprovalid = model.SecondStageApprovalID;
                        //In the second stage accepted by name is updated from Manager to networking head name.
                        RequestToUpdate.AcceptedBy = db.Users.FirstOrDefault(o => o.UserID == model.NetworkheadID).FirstName;
                        RequestToUpdate.SecondstageApproval = model.SecondStageApproval;
                        RequestToUpdate.NetworkingId = model.NwEmpID;
                        db.SaveChanges();

                        string MngrMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).EmailAddress;
                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        obj_Approve.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        obj_Approve.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                        obj_Approve.NwEmpName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingId).FirstName;

                        List<string> MailIDs = new List<string>();

                        MailIDs.Add(MngrMailID);
                        MailIDs.Add(RequestedPersonMailID);
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                        var logo = CommonLogic.getLogoPath();
                        //Approval Mail will been sent to both Requested Person and Approved Manager by Networking Head
                        //string mailMessage = MailBuilder.HwSwRequestApprovedStage2(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj_Approve.Networkheadname, obj_Approve.NwEmpName, RequestToUpdate.SecondstageApproval);

                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Approved Stage2").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request Approved Stage2").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objHwSwRequestApprovedStage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Approved Stage2")
                                                                join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                select new DSRCManagementSystem.Models.Email
                                                                {
                                                                    To = p.To,
                                                                    CC = p.CC,
                                                                    BCC = p.BCC,
                                                                    Subject = p.Subject,
                                                                    Template = q.TemplatePath
                                                                }).FirstOrDefault();

                            var companyname = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                            string TemplatePathHwSwRequestApprovedStage2 = Server.MapPath(objHwSwRequestApprovedStage2.Template);
                            string htmlHwSwRequestApprovedStage2 = System.IO.File.ReadAllText(TemplatePathHwSwRequestApprovedStage2);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#EmpName", RequestToUpdate.Employee);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#Description", RequestToUpdate.Description);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#NetworkingMngr", obj_Approve.Networkheadname);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#NetworkingEmp", obj_Approve.NwEmpName);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#ServerName", ServerName);
                            htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#CompanyName", companyname);

                            if (ServerName  != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                //string EmailAddress = "";

                                //foreach (string mail in MailIds)
                                //{
                                //    EmailAddress += mail + ",";
                                //}

                                //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                 {
                                     // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                                     // DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestApprovedStage2 + " - Testing Plaese ignore", null, null, "Test-admin@dsrc.co.in", MailIds, Server.MapPath(logo.AppValue.ToString()), false);
                                     DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestApprovedStage2 + " - Testing Plaese ignore", null, null, "Test-admin@dsrc.co.in", MailIds, Server.MapPath(logo.ToString()), false);
                                 });

                            }
                            else
                            {

                                Task.Factory.StartNew(() =>
                                {
                                    //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, "DSRC HRMS-Hardware request has been approved", mailMessage,null,null, "admin@dsrc.co.in", MailIDs, Server.MapPath("~/Content/Template/images/logo.png"), false); 
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                                    //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject, htmlHwSwRequestApprovedStage2, null, null, "admin@dsrc.co.in", MailIDs, Server.MapPath(logo.AppValue.ToString()), false);
                                    DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject, htmlHwSwRequestApprovedStage2, null, null, "admin@dsrc.co.in", MailIDs, Server.MapPath(logo.ToString()), false);

                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("HwSw Request Approved Stage2", folder, ServerName);
                        }


                        obj_Approve.ReportingPersonEmail = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingId).EmailAddress;

                        //Task Assigned mail sent to assigned networking Employee.
                        // mailMessage = MailBuilder.HwSwRequestAssignTo(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj_Approve.Networkheadname, obj_Approve.NwEmpName, RequestToUpdate.SecondstageApproval);

                        var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request AssignTo").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request AssignTo").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((checks != null) && (checks != 0))
                        {
                            var objHwSwRequestAssignTo = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request AssignTo")
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
                            string TemplatePathHwSwRequestAssignTo = Server.MapPath(objHwSwRequestAssignTo.Template);
                            string htmlHwSwRequestAssignTo = System.IO.File.ReadAllText(TemplatePathHwSwRequestAssignTo);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#EmpName", RequestToUpdate.Employee);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#Description", RequestToUpdate.Description);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#NetworkingMngr", obj_Approve.Networkheadname);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#NetworkingEmp", obj_Approve.NwEmpName);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#ServerName", ServerName);
                            htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#CompanyName", company);
                            if (ServerName  != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                                    //DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestAssignTo + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestAssignTo + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                });

                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", obj_Approve.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png")); 
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                                    //DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject, null, htmlHwSwRequestAssignTo, "admin@dsrc.co.in", obj_Approve.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject, null, htmlHwSwRequestAssignTo, "admin@dsrc.co.in", obj_Approve.ReportingPersonEmail, Server.MapPath(logo.ToString()));
                                });
                            }
                            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("HwSw Request AssignTo", folders, ServerName);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
            }
            return Json(new { Result = "Already Approved/Rejected/Cancelled" }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult RejectHwSwRequest(int RequestID)
        {
            int roleId = int.Parse(Session["RoleID"].ToString());
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj_Reject = new DSRCManagementSystem.Models.Assets();

                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == RequestID).Select(o => o).FirstOrDefault();

                if (roleId == 4 || roleId == 42)
                {
                    ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        obj_Reject.RequestedId = RequestID;
                        return View(obj_Reject);
                    }
                    else
                        return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
                }
                else if (roleId == 30)
                {
                    ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        obj_Reject.RequestedId = RequestID;
                        return View(obj_Reject);
                    }
                    else
                        return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    //First Stage 
                    ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;
                    //Second Stage
                    ViewBag.IsalreadyApproved1 = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled1 = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected1 = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false && ViewBag.IsalreadyApproved1 == false && ViewBag.IsCanceled1 == false && ViewBag.IsalreadyRejected1 == false)
                    {
                        RequestToUpdate.RejectedDate = DateTime.Now;
                        RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).FirstName;
                        RequestToUpdate.FirststageApprovalid = 4;
                        RequestToUpdate.SecondstageApprovalid = 4;
                        db.SaveChanges();

                        return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        //Sending Mail For Hardware Software First Stage Rejection.
        [HttpPost]
        public ActionResult RejectHwSwRequest(Assets model)
        {
            var Id = Convert.ToInt32(model.RequestedId);

            int roleId = int.Parse(Session["RoleID"].ToString());

            if (ModelState.IsValid)
            {
                try
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    DSRCManagementSystem.Models.Assets obj_Reject = new DSRCManagementSystem.Models.Assets();
                    string ServerName = AppValue.GetFromMailAddress("ServerName");
                    var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();

                    if (roleId == 4 || roleId == 42)
                    {
                        ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                        ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                        ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;

                        model.FirstStageApprovalID = 3;
                        model.SecondStageApprovalID = 3;

                        if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                        {
                            RequestToUpdate.RejectedDate = DateTime.Now;
                            RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                            RequestToUpdate.FirststageApprovalid = model.FirstStageApprovalID;
                            RequestToUpdate.SecondstageApprovalid = model.SecondStageApprovalID;
                            RequestToUpdate.FirststageApproval = model.FirstStageApproval;

                            db.SaveChanges();

                            string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                            obj_Reject.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                            obj_Reject.ReportingPersonEmail = RequestedPersonMailID;

                            //string mailMessage = MailBuilder.HwSwRequestRejected(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj_Reject.MngrName, RequestToUpdate.FirststageApproval);

                            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault();
                            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request Rejected").Select(x => x.TemplatePath).FirstOrDefault();
                            if ((check != null) && (check != 0))
                            {
                                var objHwSwRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Rejected")
                                                              join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                              select new DSRCManagementSystem.Models.Email
                                                              {
                                                                  To = p.To,
                                                                  CC = p.CC,
                                                                  BCC = p.BCC,
                                                                  Subject = p.Subject,
                                                                  Template = q.TemplatePath
                                                              }).FirstOrDefault();


                                var companyname = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                string TemplatePathHwSwRequestRejected = Server.MapPath(objHwSwRequestRejected.Template);
                                string htmlHwSwRequestRejected = System.IO.File.ReadAllText(TemplatePathHwSwRequestRejected);
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#EmpName", RequestToUpdate.Employee);
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Manager", obj_Reject.MngrName);
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Comments", RequestToUpdate.FirststageApproval);
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ServerName", ServerName);
                                htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#CompanyName", companyname);
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                if (ServerName  != "http://win2012srv:88/")
                                {

                                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                    //MailIds.Add("boobalan.k@dsrc.co.in");
                                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                                    //MailIds.Add("ramesh.S@dsrc.co.in");
                                    //MailIds.Add("aruna.m@dsrc.co.in");
                                    //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                    //MailIds.Add("kirankumar@dsrc.co.in");
                                    //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                    string EmailAddress = "";

                                    foreach (string mail in MailIds)
                                    {
                                        EmailAddress += mail + ",";
                                    }

                                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                    Task.Factory.StartNew(() =>
                                    {
                                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        var logo = CommonLogic.getLogoPath();

                                        // DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                        DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                    });

                                }
                                else
                                {


                                    Task.Factory.StartNew(() =>
                                    {
                                        // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        var logo = CommonLogic.getLogoPath();
                                        //DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject, null, htmlHwSwRequestRejected, "admin@dsrc.co.in", obj_Reject.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                        DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject, null, htmlHwSwRequestRejected, "admin@dsrc.co.in", obj_Reject.ReportingPersonEmail, Server.MapPath(logo.ToString()));
                                        // DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been rejected", null, mailMessage, "admin@dsrc.co.in", obj_Reject.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png"));                                
                                    });
                                }

                                return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("HwSw Request Rejected", folder, ServerName);
                            }
                        }
                        else
                            return Json(new { Result = "Already Approved/Rejected/Cancelled" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (roleId == 30)
                    {
                        ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                        ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                        ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                        model.SecondStageApprovalID = 3;

                        if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                        {
                            RequestToUpdate.RejectedDate = DateTime.Now;
                            RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingHead).FirstName;
                            RequestToUpdate.SecondstageApprovalid = model.SecondStageApprovalID;
                            RequestToUpdate.SecondstageApproval = model.SecondStageApproval;
                            db.SaveChanges();

                            string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                            string MngrMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).EmailAddress;

                            List<string> MailIDs = new List<string>();

                            MailIDs.Add(MngrMailID);
                            MailIDs.Add(RequestedPersonMailID);

                            // string mailMessage = MailBuilder.HwSwRequestRejectedStage2(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj_Reject.MngrName, RequestToUpdate.SecondstageApproval);

                             var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Rejected Stage2").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request Rejected Stage2").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objHwSwRequestRejectedStage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Rejected Stage2")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();
                         var companyname = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathHwSwRequestRejectedStage2 = Server.MapPath(objHwSwRequestRejectedStage2.Template);
                         string htmlHwSwRequestRejectedStage2 = System.IO.File.ReadAllText(TemplatePathHwSwRequestRejectedStage2);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#NetworkingMngr", obj_Reject.MngrName);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#Description", obj_Reject.Description);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#ServerName", ServerName);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#CompanyName", companyname);
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             //string EmailAddress = "";

                             //foreach (string mail in MailIds)
                             //{
                             //    EmailAddress += mail + ",";
                             //}

                             //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 var logo = CommonLogic.getLogoPath();
                                 //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestRejectedStage2 + " - Testing Plaese ignore", null, null, "Test-admin@dsrc.co.in", MailIds, Server.MapPath(logo.AppValue.ToString()), false);
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestRejectedStage2 + " - Testing Plaese ignore", null, null, "Test-admin@dsrc.co.in", MailIds, Server.MapPath(logo.ToString()), false);
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = CommonLogic.getLogoPath();
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject, htmlHwSwRequestRejectedStage2, null, null, "admin@dsrc.co.in", MailIDs, Server.MapPath(logo.ToString()), false);
                                 // DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject, htmlHwSwRequestRejectedStage2, null, null, "admin@dsrc.co.in", MailIDs, Server.MapPath(logo.AppValue.ToString()), false);
                                 //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, "DSRC HRMS-Hardware request has been rejected", mailMessage,null,null, "admin@dsrc.co.in", MailIDs, Server.MapPath("~/Content/Template/images/logo.png"), false);                                
                             });
                         }
                     }
                     else
                     {
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("HwSw Request Rejected Stage2", folder, ServerName);
                     }

                            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                            return Json(new { Result = "Already Approved/Rejected/Cancelled" }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
            }
            return null;
        }

        public ActionResult HwSwRequestDetail(int RequestID)
        {
            var RequestViewModel = new Assets();
            try
            {
                ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj_Request = new DSRCManagementSystem.Models.Assets();

                obj_Request.RequestedId = RequestID;

                var DepartmentList = db.Departments.ToList();
                var LocationList = db.locations.ToList();
                var CategoryList = db.Master_Category.ToList();
                var StatusList = db.Status.ToList();
                var PriorityList = db.Master_Priority.ToList();
                var ApprovalStatusList = db.Master_RequestStatus.ToList();

                var ManagerList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 4 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();
                var EmpList = (from u in db.Users select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();
                var NetworkEmpList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 36 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();

                RequestViewModel = (from rc in db.RequestComponents
                                    where rc.RequestId == RequestID
                                    select new Assets
                                    {
                                        RequestedId = rc.RequestId,
                                        Description = rc.Description,
                                        DepartmentID = rc.DepartmentId,
                                        LocationID = rc.LocationId,
                                        EmpID = rc.userid,
                                        EmpName = rc.Employee,
                                        ComputerID = rc.ComputerId,
                                        CategoryID = rc.CategoryId,
                                        StatusID = rc.StatusId,
                                        PriorityID = rc.PriorityId,
                                        FirstStageApprovalID = rc.FirststageApprovalid,
                                        ApprovalStatusID = rc.FirststageApprovalid,
                                        RequestedDate = rc.NewrequestDate,
                                        FirstStageApproveDate = rc.FirstStageApprovalDate

                                    }).FirstOrDefault();

                ViewBag.DeptList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", RequestViewModel.DepartmentID);
                ViewBag.LocList = new SelectList(LocationList, "locationid", "LocationName", RequestViewModel.LocationID);
                ViewBag.EmpList = new SelectList(EmpList, "UserID", "FirstName", RequestViewModel.EmpID);
                ViewBag.CatList = new SelectList(CategoryList, "Categoryid", "CategoryName", RequestViewModel.CategoryID);
                ViewBag.StatusList = new SelectList(StatusList, "Statusid", "StatusName", RequestViewModel.StatusID);
                ViewBag.PriorList = new SelectList(PriorityList, "Priorityid", "PriorityName", RequestViewModel.PriorityID);
                ViewBag.ApprovalStatusList = new SelectList(ApprovalStatusList, "RequestStatusId", "RequestStatus", RequestViewModel.ApprovalStatusID);

                RequestViewModel.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                return View(RequestViewModel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(RequestViewModel);
        }

        public Assets GetModelData(int id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var GetData = new Assets();
            try
            {
                GetData = (from rc in db.RequestComponents
                           join d in db.Departments on rc.DepartmentId equals d.DepartmentId
                           join l in db.locations on rc.LocationId equals l.locationid
                           join s in db.Status on rc.StatusId equals s.Statusid
                           join c in db.Master_Category on rc.CategoryId equals c.Categoryid
                           join ca in db.ComputerAssigneds on rc.userid equals ca.Userid
                           join p in db.Master_Priority on rc.PriorityId equals p.Priorityid
                           join cm in db.computermanagements on ca.Managementid equals cm.managementid
                           where rc.RequestId == id
                           select new Assets()
                           {
                               RequestedId = rc.RequestId,
                               EmpName = rc.Employee,
                               Description = rc.Description,
                               DepartmentID = rc.DepartmentId,
                               DepartmentName = d.DepartmentName,
                               ComputerName = cm.ComputerName,
                               LocationID = rc.LocationId,
                               Location = l.LocationName,
                               Category = c.CategoryName,
                               StatusID = rc.StatusId,
                               Status = s.StatusName,
                               Priority = p.PriorityName,
                               EmpID = rc.userid
                           }).FirstOrDefault();
                return GetData;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return GetData;
        }

        public ActionResult ApproveVMail_Stage1(string RequestID)
        {
            var Id = Convert.ToInt32(Encrypter.Decode(RequestID));

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj = new DSRCManagementSystem.Models.Assets();
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                obj = GetModelData(Id);

                obj.FirstStageApprovalID = 2;
                //currently the networking head only Sundar G Daniel and his user id is inserted for refernce
                obj.NetworkheadID = 10;

                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();
                ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;

                if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                {
                    RequestToUpdate.FirstStageApprovalDate = DateTime.Now;
                    RequestToUpdate.FirstStageApprovedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                    RequestToUpdate.FirststageApprovalid = obj.FirstStageApprovalID;
                    RequestToUpdate.AcceptedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                    RequestToUpdate.FirststageApproval = "Approved Via Mail";
                    RequestToUpdate.NetworkingHead = obj.NetworkheadID;
                    db.SaveChanges();

                    string NetworkingMngrMailID = db.Users.FirstOrDefault(o => o.UserID == 10).EmailAddress;
                    string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;

                    //assigning values to pass to mail
                    obj.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                    obj.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;

                    //Approval Mail will been sent to Requested Person.
                    //string mailMessage = MailBuilder.HwSwRequestApproved(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.MngrName, obj.Networkheadname, RequestToUpdate.FirststageApproval);
                   // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objHwSwRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Approved")
                                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                      select new DSRCManagementSystem.Models.Email
                                                      {
                                                          To = p.To,
                                                          CC = p.CC,
                                                          BCC = p.BCC,
                                                          Subject = p.Subject,
                                                          Template = q.TemplatePath
                                                      }).FirstOrDefault();

                        var companyname = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                        string TemplatePathHwSwRequestApproved = Server.MapPath(objHwSwRequestApproved.Template);
                        string htmlHwSwRequestApproved = System.IO.File.ReadAllText(TemplatePathHwSwRequestApproved);
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#EmpName", RequestToUpdate.Employee);
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Manager", obj.MngrName);
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Comments", RequestToUpdate.FirststageApproval);
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ServerName", ServerName);
                        htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#CompanyName", companyname);


                        if (ServerName  != "http://win2012srv:88/")
                        {

                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                            //MailIds.Add("boobalan.k@dsrc.co.in");
                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                            //MailIds.Add("ramesh.S@dsrc.co.in");
                            //MailIds.Add("aruna.m@dsrc.co.in");
                            //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                            //MailIds.Add("kirankumar@dsrc.co.in");
                            //MailIds.Add("francispaul.k.c@dsrc.co.in");

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                var logo = CommonLogic.getLogoPath();
                                // DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {


                            Task.Factory.StartNew(() =>
                            {
                                //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath("~/Content/Template/images/logo.png"));
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                var logo = CommonLogic.getLogoPath();
                                //DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject, null, htmlHwSwRequestApproved, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject, null, htmlHwSwRequestApproved, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath(logo.ToString()));
                            });
                        }
                    }
                    else
                    {

                        ExceptionHandlingController.TemplateMissing("HwSw Request Approved", folder, ServerName);
                    }

                    // Approval Mail will sent to Network head by Assigned TO Manager for Second stage Approval.

                    //mailMessage = MailBuilder.SendhwswRequest_Stage2(obj);

                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Sendhwsw Request_Stage2").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Sendhwsw Request_Stage2").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((checks != null) && (checks != 0))
                    {
                        var objSendhwswRequest_Stage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Sendhwsw Request_Stage2")
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
                        string TemplatePathSendhwswRequest_Stage2 = Server.MapPath(objSendhwswRequest_Stage2.Template);
                        string htmlSendhwswRequest_Stage2 = System.IO.File.ReadAllText(TemplatePathSendhwswRequest_Stage2);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Networkheadname", obj.Networkheadname);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#MngrName", obj.MngrName);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Description", obj.Description);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#DepartmentName", obj.DepartmentName);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Location", obj.Location);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ComputerName", obj.ComputerName);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Category", obj.Category);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Status", obj.Status);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Priority", obj.Priority);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#RequestedId", Encrypter.Encode(obj.RequestedId.ToString()));
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ServerName", ServerName);
                        htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#CompanyName", company);


                        if (ServerName  != "http://win2012srv:88/")
                        {

                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                            //MailIds.Add("boobalan.k@dsrc.co.in");
                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                            //MailIds.Add("ramesh.S@dsrc.co.in");
                            //MailIds.Add("aruna.m@dsrc.co.in");
                            //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                            //MailIds.Add("kirankumar@dsrc.co.in");
                            //MailIds.Add("francispaul.k.c@dsrc.co.in");

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest_Stage2 + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            });

                        }
                        else
                        {

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath("~/Content/Template/images/logo.png"));                                               
                                DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject, null, htmlSendhwswRequest_Stage2, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath(logo.AppValue.ToString()));
                            });
                        }


                    }
                    else
                    {

                        ExceptionHandlingController.TemplateMissing("Sendhwsw Request_Stage2", folders, ServerName);
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult RejectVMail_Stage1(string RequestID)
        {
            var Id = Convert.ToInt32(Encrypter.Decode(RequestID));

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj_Reject = new DSRCManagementSystem.Models.Assets();
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();

                ViewBag.IsalreadyApproved = RequestToUpdate.FirststageApprovalid == 2 ? true : false;
                ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                ViewBag.IsalreadyRejected = RequestToUpdate.FirststageApprovalid == 3 ? true : false;

                obj_Reject.FirstStageApprovalID = 3;
                obj_Reject.SecondStageApprovalID = 3;

                if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                {
                    RequestToUpdate.RejectedDate = DateTime.Now;
                    RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                    RequestToUpdate.FirststageApprovalid = obj_Reject.FirstStageApprovalID;
                    RequestToUpdate.SecondstageApprovalid = obj_Reject.SecondStageApprovalID;
                    RequestToUpdate.FirststageApproval = "Rejected Without Comments";

                    db.SaveChanges();

                    string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                    obj_Reject.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                    obj_Reject.ReportingPersonEmail = RequestedPersonMailID;

                    //string mailMessage = MailBuilder.HwSwRequestRejected(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj_Reject.MngrName, RequestToUpdate.FirststageApproval);

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request Rejected").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objHwSwRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Rejected")
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
                         string TemplatePathHwSwRequestRejected = Server.MapPath(objHwSwRequestRejected.Template);
                         string htmlHwSwRequestRejected = System.IO.File.ReadAllText(TemplatePathHwSwRequestRejected);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Manager", obj_Reject.MngrName);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Comments", RequestToUpdate.FirststageApproval);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ServerName",ServerName);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#CompanyName", company);

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject, null, htmlHwSwRequestRejected, "admin@dsrc.co.in", obj_Reject.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                 //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been rejected", null, mailMessage, "admin@dsrc.co.in", obj_Reject.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png"));                        
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("HwSw Request Rejected", folder, ServerName);
                     }
                }
                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult CommentVMail_Stage1(string RequestID)
        {
            try
            {
                Session["ServerName"] = AppValue.GetFromMailAddress("ServerName");
                var Id = Convert.ToInt32(Encrypter.Decode(RequestID));
                ViewBag.Id = Id;
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();
                ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                ViewBag.IsAlreadycommented = (RequestToUpdate.FirststageApprovalid != 1) ? true : false;
                return View();
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
        public ActionResult CommentVMail_Stage1(string RequestID, string Comments, bool IsAccepted)
        {
            var Id = Convert.ToInt32(RequestID);
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Assets obj = new DSRCManagementSystem.Models.Assets();
            var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();

            ViewBag.Id = RequestID;

            obj = GetModelData(Id);

            if (IsAccepted)
            {
                try
                {
                    obj.FirstStageApprovalID = 2;
                    //currently the networking head only Sundar G Daniel and his user id is inserted for refernce
                    obj.NetworkheadID = 10;


                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsAlreadycommented = (RequestToUpdate.FirststageApprovalid != 1) ? true : false;

                    if (ViewBag.IsAlreadycommented == false && ViewBag.IsCanceled == false)
                    {
                        RequestToUpdate.FirstStageApprovalDate = DateTime.Now;
                        RequestToUpdate.FirstStageApprovedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        RequestToUpdate.FirststageApprovalid = obj.FirstStageApprovalID;
                        RequestToUpdate.AcceptedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        RequestToUpdate.FirststageApproval = Comments;
                        RequestToUpdate.NetworkingHead = obj.NetworkheadID;
                        db.SaveChanges();

                        string NetworkingMngrMailID = db.Users.FirstOrDefault(o => o.UserID == 10).EmailAddress;
                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        obj.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        obj.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        //Approval Mail will been sent to Requested Person.
                        //string mailMessage = MailBuilder.HwSwRequestApproved(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.MngrName, obj.Networkheadname, RequestToUpdate.FirststageApproval);

                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "HwSw Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objHwSwRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Approved")
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
                            string TemplatePathHwSwRequestApproved = Server.MapPath(objHwSwRequestApproved.Template);
                            string htmlHwSwRequestApproved = System.IO.File.ReadAllText(TemplatePathHwSwRequestApproved);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#EmpName", RequestToUpdate.Employee);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Manager", obj.MngrName);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#Comments", RequestToUpdate.FirststageApproval);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#ServerName",ServerName);
                            htmlHwSwRequestApproved = htmlHwSwRequestApproved.Replace("#CompanyName", company);


                            if (ServerName  != "http://win2012srv:88/")
                            {


                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });

                            }
                            else
                            {


                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestApproved.Subject, null, htmlHwSwRequestApproved, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath(logo.AppValue.ToString()));
                                    //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", RequestedPersonMailID, Server.MapPath("~/Content/Template/images/logo.png"));                                                       
                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("HwSw Request Approved ", folder, ServerName);
                        }

                        // Approval Mail will sent to Network head by Assigned TO Manager for Second stage Approval.

                        //mailMessage = MailBuilder.SendhwswRequest_Stage2(obj);

                        var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Sendhwsw Request_Stage2").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Sendhwsw Request_Stage2").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((checks != null) && (checks != 0))
                        {
                            var objSendhwswRequest_Stage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Sendhwsw Request_Stage2")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();

                            var comp = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                            string TemplatePathSendhwswRequest_Stage2 = Server.MapPath(objSendhwswRequest_Stage2.Template);
                            string htmlSendhwswRequest_Stage2 = System.IO.File.ReadAllText(TemplatePathSendhwswRequest_Stage2);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Networkheadname", obj.Networkheadname);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#MngrName", obj.MngrName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Description", obj.Description);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#DepartmentName", obj.DepartmentName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Location", obj.Location);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ComputerName", obj.ComputerName);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Category", obj.Category);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Status", obj.Status);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#Priority", obj.Priority);
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#RequestedId", Encrypter.Encode(obj.RequestedId.ToString()));
                            htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#ServerName", ServerName); htmlSendhwswRequest_Stage2 = htmlSendhwswRequest_Stage2.Replace("#CompanyName", comp);

                            if (ServerName  != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject + " - Test Mail Please Ignore", null, htmlSendhwswRequest_Stage2 + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });

                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objSendhwswRequest_Stage2.Subject, null, htmlSendhwswRequest_Stage2, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath(logo.AppValue.ToString()));
                                    //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", NetworkingMngrMailID, Server.MapPath("~/Content/Template/images/logo.png"));                                                       
                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("Delete Income", folders, ServerName);
                        }
                    }
                    
                    
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }

            }
            else
            {
                try
                {
                    ViewBag.IsCanceled = RequestToUpdate.FirststageApprovalid == 4 ? true : false;
                    ViewBag.IsAlreadycommented = (RequestToUpdate.FirststageApprovalid != 1) ? true : false;

                    obj.FirstStageApprovalID = 3;
                    obj.SecondStageApprovalID = 3;

                    if (ViewBag.IsAlreadycommented == false && ViewBag.IsCanceled == false)
                    {
                        RequestToUpdate.RejectedDate = DateTime.Now;
                        RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        RequestToUpdate.FirststageApprovalid = obj.FirstStageApprovalID;
                        RequestToUpdate.SecondstageApprovalid = obj.SecondStageApprovalID;
                        RequestToUpdate.FirststageApproval = Comments;

                        db.SaveChanges();

                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        obj.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        obj.ReportingPersonEmail = RequestedPersonMailID;

                        //string mailMessage = MailBuilder.HwSwRequestRejected(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.MngrName, RequestToUpdate.FirststageApproval);

                         var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request Rejected").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objHwSwRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Rejected")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();

                         string TemplatePathHwSwRequestRejected = Server.MapPath(objHwSwRequestRejected.Template);
                         string htmlHwSwRequestRejected = System.IO.File.ReadAllText(TemplatePathHwSwRequestRejected);

                         var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Manager", obj.MngrName);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#Comments", RequestToUpdate.FirststageApproval);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#ServerName", ServerName);
                         htmlHwSwRequestRejected = htmlHwSwRequestRejected.Replace("#CompanyName", objcom);
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestRejected.Subject, null, htmlHwSwRequestRejected, "admin@dsrc.co.in", obj.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                 //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been rejected", null, mailMessage, "admin@dsrc.co.in", obj.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png"));                           
                             });
                         }
                     }

                     else
                     {
                          //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                          ExceptionHandlingController.TemplateMissing("HwSw Request Rejected", folder, ServerName);
                     }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CommentVMail_Stage2(string RequestID, string choice)
        {
            try
            {
                Session["ServerName"] = AppValue.GetFromMailAddress("ServerName");
                var Id = Convert.ToInt32(Encrypter.Decode(RequestID));
                ViewBag.Id = Id;
                ViewBag.Approve = choice == "1" ? true : false;

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Assets obj_Request = new DSRCManagementSystem.Models.Assets();

                var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();

                ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                {

                    var NetworkEmpList = (from u in db.Users join r in db.UserRoles on u.UserID equals r.UserID where r.RoleID == 36 select new { UserID = u.UserID, FirstName = u.FirstName }).ToList();
                    ViewBag.NworkEmpList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" } }.Union(NetworkEmpList), "UserID", "FirstName", 0);
                }
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
        public ActionResult CommentVMail_Stage2(string RequestID, string NwEmpID, string Comments, bool IsAccepted)
        {
            var Id = Convert.ToInt32(RequestID);
            var NwId = Convert.ToInt32(NwEmpID);
            ViewBag.Id = RequestID;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DSRCManagementSystem.Models.Assets obj = new DSRCManagementSystem.Models.Assets();
            var RequestToUpdate = db.RequestComponents.Where(o => o.RequestId == Id).Select(o => o).FirstOrDefault();

            if (IsAccepted)
            {
                try
                {
                    obj.SecondStageApprovalID = 2;
                    obj.NetworkheadID = 10;
                    obj.NwEmpID = NwId;

                    ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        RequestToUpdate.SecondStageApprovalDate = DateTime.Now;
                        RequestToUpdate.SecondStageApprovedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingHead).FirstName;
                        RequestToUpdate.SecondstageApprovalid = obj.SecondStageApprovalID;
                        //In the second stage accepted by name is updated from Manager to networking head name.
                        RequestToUpdate.AcceptedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingHead).FirstName;
                        RequestToUpdate.SecondstageApproval = Comments;
                        RequestToUpdate.NetworkingId = obj.NwEmpID;
                        db.SaveChanges();

                        string MngrMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).EmailAddress;
                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        obj.MngrName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).FirstName;
                        obj.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;
                        obj.NwEmpName = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingId).FirstName;

                        List<string> MailIDs = new List<string>();

                        MailIDs.Add(MngrMailID);
                        MailIDs.Add(RequestedPersonMailID);
                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        //Approval Mail will been sent to both Requested Person and Approved Manager by Networking Head
                        //string mailMessage = MailBuilder.HwSwRequestApprovedStage2(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.Networkheadname, obj.NwEmpName, RequestToUpdate.SecondstageApproval);

                         var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Approved Stage2").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request Approved Stage2").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objHwSwRequestApprovedStage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Approved Stage2")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();

                         string TemplatePathHwSwRequestApprovedStage2 = Server.MapPath(objHwSwRequestApprovedStage2.Template);
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string htmlHwSwRequestApprovedStage2 = System.IO.File.ReadAllText(TemplatePathHwSwRequestApprovedStage2);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#Description", RequestToUpdate.Description);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#NetworkingMngr", obj.Networkheadname);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#NetworkingEmp", obj.NwEmpName);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#ServerName", ServerName);
                         htmlHwSwRequestApprovedStage2 = htmlHwSwRequestApprovedStage2.Replace("#CompanyName", company);

                       

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             //string EmailAddress = "";

                             //foreach (string mail in MailIds)
                             //{
                             //    EmailAddress += mail + ",";
                             //}

                             //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestApprovedStage2 + " - Testing Plaese ignore", null, null, "Test-admin@dsrc.co.in", MailIds, Server.MapPath(logo.AppValue.ToString()), false);

                             });
                         }
                         else
                         {


                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestApprovedStage2.Subject, htmlHwSwRequestApprovedStage2, null, null, "admin@dsrc.co.in", MailIDs, Server.MapPath(logo.AppValue.ToString()), false);
                                 //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, "DSRC HRMS-Hardware request has been approved", mailMessage,null,null, "admin@dsrc.co.in", MailIDs, Server.MapPath("~/Content/Template/images/logo.png"), false);                           
                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("HwSw Request Approved Stage2", folder, ServerName);
                     }


                        obj.ReportingPersonEmail = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingId).EmailAddress;

                        //Task Assigned mail sent to assigned networking Employee.
                        //mailMessage = MailBuilder.HwSwRequestAssignTo(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.Networkheadname, obj.NwEmpName, RequestToUpdate.SecondstageApproval);

                         var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request AssignTo").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folders= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request AssignTo").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {
                         var objHwSwRequestAssignTo = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request AssignTo")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();
                         var Company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                         string TemplatePathHwSwRequestAssignTo = Server.MapPath(objHwSwRequestAssignTo.Template);
                         string htmlHwSwRequestAssignTo = System.IO.File.ReadAllText(TemplatePathHwSwRequestAssignTo);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#Description", RequestToUpdate.Description);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#NetworkingMngr", obj.Networkheadname);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#NetworkingEmp", obj.NwEmpName);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#ServerName",ServerName);
                         htmlHwSwRequestAssignTo = htmlHwSwRequestAssignTo.Replace("#CompanyName", Company);
                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject + " - Test Mail Please Ignore", null, htmlHwSwRequestAssignTo + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {


                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objHwSwRequestAssignTo.Subject, null, htmlHwSwRequestAssignTo, "admin@dsrc.co.in", obj.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                 //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Hardware request has been approved", null, mailMessage, "admin@dsrc.co.in", obj.ReportingPersonEmail, Server.MapPath("~/Content/Template/images/logo.png"));                           
                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("HwSw Request AssignTo", folders, ServerName);
                     }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
            }
            else
            {
                try
                {


                    ViewBag.IsalreadyApproved = RequestToUpdate.SecondstageApprovalid == 2 ? true : false;
                    ViewBag.IsCanceled = RequestToUpdate.SecondstageApprovalid == 4 ? true : false;
                    ViewBag.IsalreadyRejected = RequestToUpdate.SecondstageApprovalid == 3 ? true : false;

                    obj.SecondStageApprovalID = 3;

                    if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                    {
                        RequestToUpdate.RejectedDate = DateTime.Now;
                        RequestToUpdate.RejectedBy = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.NetworkingHead).FirstName;
                        RequestToUpdate.SecondstageApprovalid = obj.SecondStageApprovalID;
                        RequestToUpdate.SecondstageApproval = Comments;
                        db.SaveChanges();

                        string RequestedPersonMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.userid).EmailAddress;
                        string MngrMailID = db.Users.FirstOrDefault(o => o.UserID == RequestToUpdate.AssignedToId).EmailAddress;

                        obj.Networkheadname = db.Users.FirstOrDefault(o => o.UserID == 10).FirstName;

                        List<string> MailIDs = new List<string>();

                        MailIDs.Add(MngrMailID);
                        MailIDs.Add(RequestedPersonMailID);

                        //string mailMessage = MailBuilder.HwSwRequestRejectedStage2(RequestToUpdate.RequestId, RequestToUpdate.Employee, RequestToUpdate.Description, obj.Networkheadname, RequestToUpdate.SecondstageApproval);

                         var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "HwSw Request Rejected Stage2").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "HwSw Request Rejected Stage2").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objHwSwRequestRejectedStage2 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "HwSw Request Rejected Stage2")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();
                         var Company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathHwSwRequestRejectedStage2 = Server.MapPath(objHwSwRequestRejectedStage2.Template);
                         string htmlHwSwRequestRejectedStage2 = System.IO.File.ReadAllText(TemplatePathHwSwRequestRejectedStage2);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#ReqID", RequestToUpdate.RequestId.ToString());
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#EmpName", RequestToUpdate.Employee);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#NetworkingMngr", obj.MngrName);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#Description", RequestToUpdate.Description);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#Comments", RequestToUpdate.SecondstageApproval);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#ServerName", ServerName);
                         htmlHwSwRequestRejectedStage2 = htmlHwSwRequestRejectedStage2.Replace("#CompanyName", Company);
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("kirankumar@dsrc.co.in");
                             //MailIds.Add("francispaul.k.c@dsrc.co.in");

                             //string EmailAddress = "";

                             //foreach (string mail in MailIds)
                             //{
                             //    EmailAddress += mail + ",";
                             //}

                             //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject + " - Test Mail Please Ignore", htmlHwSwRequestRejectedStage2 + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", null, null, MailIds, Server.MapPath(logo.AppValue.ToString()), false);
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, objHwSwRequestRejectedStage2.Subject, htmlHwSwRequestRejectedStage2, "admin@dsrc.co.in", null, null, MailIDs, Server.MapPath(logo.AppValue.ToString()), false);
                                 //DsrcMailSystem.MailSender.SendInlineAttachmentMail(null, "DSRC HRMS-Hardware request has been rejected", mailMessage, "admin@dsrc.co.in", null,null,MailIDs, Server.MapPath("~/Content/Template/images/logo.png"), false);                       
                             });
                         }
                     }
                     else
                     {
                       //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("HwSw Request Rejected Stage2", folder, ServerName);
                     }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SuccessfullyApproved()
        {
            return View();
        }

        public ActionResult SuccessfullyRejected()
        {
            return View();
        }




    }
}
