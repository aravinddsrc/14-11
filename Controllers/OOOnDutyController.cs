using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Data;
//using DSRCHRMSRemainderService;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Objects;

namespace DSRCManagementSystem.Controllers
{
    public class OOOnDutyController : Controller
    {

        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult OOODViewRequest()   
        {
            int UserId = Convert.ToInt32(Session["UserID"]);

            var Count = db.UserReportings.Where(o => o.ReportingUserID == UserId).Count();
            
            ViewBag.IsReportingPerson = Count > 0 ? true : false;

            ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", 0);

            var result = (from rc in db.OutOfOfficeDetails
                          join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                          join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                          join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                          join u in db.Users on rc.Userid equals u.UserID 
                          orderby rc.ODStartDate descending
                          where rc.Userid == UserId &&u.UserStatus!=6
                          select new OnDutyRequestModel
                          {
                              ODTypeID = rc.ODTypeID,
                              EmpName = u.FirstName + " " + (u.LastName ?? ""),                             
                              StartDate = rc.ODStartDate,
                              EndDate = rc.ODEndDate,
                              Workingdays = rc.ODWorkingDays,
                              AlternateNo = rc.ODAlternateNo,
                              ODType = t.ODTypeName,
                              ODPlace = p.ODLocation,
                              RequestStatus = l.Status,
                              ODComments = rc.ODComments,
                              ODID = rc.ODId,                             
                              Leaveid = l.LeaveStatusId,
                              SelectedUserStatusid=u.UserStatus
                          }).ToList();
            return View(result.ToList());
        }

        [HttpPost]
        public ActionResult OOODViewRequest(OnDutyRequestModel model, FormCollection form)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            string LeaveStatusId = (form["LeaveStatusId"] == "") ? "0" : form["LeaveStatusId"].ToString();
            int Id = Convert.ToInt32(LeaveStatusId);

            int UserId = Convert.ToInt32(Session["UserID"]);

            var Count = db.UserReportings.Where(o => o.ReportingUserID == UserId).Count();
            ViewBag.IsReportingPerson = Count > 0 ? true : false;

            if (Id != 0)
            {
                List<DSRCManagementSystem.Models.OnDutyRequestModel> value = new List<DSRCManagementSystem.Models.OnDutyRequestModel>();

                var statuslist = db.Master_LeaveStatus.ToList();

                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              where rc.Userid == UserId && rc.RequestStatusId == Id
                              orderby rc.ODStartDate descending
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId,
                                  Leaveid = l.LeaveStatusId
                              }).OrderByDescending(o => o.StartDate.Value.Year).ThenByDescending(o => o.StartDate.Value.Month).ThenByDescending(o => o.StartDate.Value.Day).ToList();
                foreach (var item in result)
                {
                    ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(statuslist), "LeaveStatusId", "Status", item.Leaveid);
                }

                if (result.Count() == 0)
                {
                    result = new List<OnDutyRequestModel>();
                    ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(statuslist), "LeaveStatusId", "Status", Id);
                    return View(result.ToList());
                }
                return View(result.ToList());
            }
            else
            {
                return RedirectToAction("OOODViewRequest", "OOOnDuty");
            }
        }
       
        [HttpGet]
        public ActionResult ApplyOnDuty()
        {
            int UserId = Convert.ToInt32(Session["UserId"]);

            var ODTList = db.Master_OutOfOfficeTypes.ToList();
            var ODPList = db.Master_OutOfOfficePlaces.ToList();
            var ODRPList = GetReportingPersons(UserId);          

            if (ODRPList.Count == 0)
            {
                ViewBag.ODAssignedToEmpty = true;
                TempData["AssignedToEmpty"] = true;
                ViewBag.ODAssignedToList = new SelectList(new[] { new { UserId = 1, Name = "None" } }, "UserId", "Name");
            }
            else
            {
                ViewBag.ODAssignedToEmpty = false;
                TempData["AssignedToEmpty"] = false;
                ViewBag.ODAssignedToList = new SelectList(ODRPList, "UserId", "Name");
            }

            ViewBag.ODTypeList = new SelectList(ODTList, "ODTypeId", "ODTypeName");
            ViewBag.ODPlaceList = new SelectList(ODPList, "ODPlaceID", "ODLocation");

            return View();
        }

        [HttpGet]
        public ActionResult EmployeeOutEntry()
        {
            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            var userId = (int)Session["UserId"];

            var types = (from p in dbHrms.Master_OutOfOfficeTypes
                       select new 
                       {
                           outId =p.ODTypeId,
                           Outvalue=p.ODTypeName
                       }).ToList();

            ViewBag.Reportable = new SelectList(GetReporatablePerson(), "UserId", "Name");
          
            ViewBag.LeaveTypeList = new SelectList(types, "outId", "Outvalue");

            return View();
        }
    
        public ActionResult IsHolyDay(string date)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int LeaveUserId = (int)Session["UserId"];
            var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();
            DateTime? selectedDate = Convert.ToDateTime(date);
            //var holidays = db.Master_holiday.Select(x => x.Date).ToList();
            var holidays = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true).Select(x => x.Date).ToList();
            if (holidays.Contains(selectedDate))
                return Json("holiday", JsonRequestBehavior.AllowGet);
            else
                return Json("Noholiday", JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult ApplyOnDuty(OnDutyRequestModel model)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {

                int UserId = Convert.ToInt32(Session["UserId"]);

                var User=db.Users.FirstOrDefault(o=>o.UserID==UserId);
                DateTime start = (DateTime)model.StartDate;
                DateTime end = (DateTime)model.EndDate;
                DateTime startdate = start.AddHours(9);
                DateTime enddate = end.AddHours(18);
                string fdate = Convert.ToString(model.StartDate);
                string tdate = Convert.ToString(model.EndDate);

                var fromdate = DateTime.Parse(fdate);
                var todate = DateTime.Parse(tdate);
                var cancelled = MasterEnum.RequestStatus.Cancelled.GetHashCode();
                var Rejected = MasterEnum.RequestStatus.Rejected.GetHashCode();
                var Cancelled1 = MasterEnum.LeaveStatus.Cancelled.GetHashCode();
                var Rejected1 = MasterEnum.LeaveStatus.Rejected.GetHashCode();


                if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= model.StartDate && o.ODEndDate >= model.StartDate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).Select(o => o).ToList().Count > 0)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }
                else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= model.StartDate && o.ODEndDate >= model.StartDate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }
                else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= model.EndDate&& o.ODEndDate >= model.EndDate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }
                else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= model.StartDate && o.ODEndDate >= model.EndDate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }
                else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate >= model.StartDate && o.ODEndDate <= model.EndDate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }

                if (db.LeaveRequests.Where(o => o.StartDateTime <= startdate && o.EndDateTime >= startdate).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != Cancelled1 && o.LeaveStatusId != Rejected1).Select(o => o).ToList().Count > 0)
                {
                    return Json("InLeave", JsonRequestBehavior.AllowGet);
                }
                else if (db.LeaveRequests.Where(o => o.StartDateTime <= startdate && o.EndDateTime >= startdate).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != Cancelled1 && o.LeaveStatusId != Rejected1).ToList().Count > 0)
                {
                    return Json("InLeave", JsonRequestBehavior.AllowGet);
                }
                else if (db.LeaveRequests.Where(o => o.StartDateTime <= enddate && o.EndDateTime >= enddate).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != Cancelled1 && o.LeaveStatusId != Rejected1).ToList().Count > 0)
                {
                    return Json("InLeave", JsonRequestBehavior.AllowGet);
                }
                else if (db.LeaveRequests.Where(o => o.StartDateTime <= startdate && o.EndDateTime >=enddate).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != Cancelled1 && o.LeaveStatusId != Rejected1).ToList().Count > 0)
                {
                    return Json("InLeave", JsonRequestBehavior.AllowGet);
                }
                else if (db.LeaveRequests.Where(o => o.StartDateTime >= startdate && o.EndDateTime <= enddate).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != Cancelled1 && o.LeaveStatusId != Rejected1).ToList().Count > 0)
                {
                    return Json("InLeave", JsonRequestBehavior.AllowGet);
                }

                model.EmpName = User.FirstName + " " + (User.LastName ?? "");
                 var reportingemail= db.Users.Where(o => o.UserID == model.ReportingPersonID).Select(o => o.EmailAddress).FirstOrDefault();
                 model.ReportingPersonName = db.Users.Where(o => o.UserID == model.ReportingPersonID).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                model.emailaddress = db.Users.FirstOrDefault(o => o.UserID == UserId).EmailAddress;

                bool AssinedToList = Convert.ToBoolean(TempData["AssignedToEmpty"]);

                if (AssinedToList)
                    model.RequestStatusId = MasterEnum.RequestStatus.Approved.GetHashCode();
                else
                    model.RequestStatusId = MasterEnum.RequestStatus.Pending.GetHashCode();

                var RequestToODApply = new OutOfOfficeDetail
                {
                    ODStartDate = model.StartDate,
                    ODEndDate = model.EndDate,
                    ODWorkingDays = model.Workingdays,
                    ODAlternateNo = model.AlternateNo,
                    ODTypeID = model.ODTypeID,
                    ODPlaceID = model.ODPlaceID,
                    ODComments = model.ODComments,
                    Others=model.others,
                    Userid = UserId,
                    Ireject = 0,
                    RequestStatusId = (byte)model.RequestStatusId, //These people will self approve their On Duty.
                    ReportingPersonId = model.ReportingPersonID,
                    RequestedDate=DateTime.Now

                };
                db.OutOfOfficeDetails.AddObject(RequestToODApply);
                db.SaveChanges();
                if (AssinedToList)
                {
                    model.EmpId = db.Users.FirstOrDefault(o => o.UserID == UserId).EmpID;

                    var ReqToApproveOnDuty = db.Users.Where(o => o.UserID == UserId).FirstOrDefault();
                    ReqToApproveOnDuty.IsExclude = true;
                    db.SaveChanges();

                   // TimeManagementEntry(model.EmpId, model.StartDate, model.EndDate);
                }
                else
                {
                    var obj = db.Users.Where(o => o.UserID == model.userid).Select(o => o).FirstOrDefault();

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Apply OnDuty").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Apply OnDuty").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objApplyOnDuty = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Apply OnDuty")
                                               join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                               select new DSRCManagementSystem.Models.Email
                                               {
                                                   To = p.To,
                                                   CC = p.CC,
                                                   BCC = p.BCC,
                                                   Subject = p.Subject,
                                                   Template = q.TemplatePath
                                               }).FirstOrDefault();

                         var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathApplyOnDuty = Server.MapPath(objApplyOnDuty.Template);
                         string htmlApplyOnDuty = System.IO.File.ReadAllText(TemplatePathApplyOnDuty);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#EmpName", model.EmpName);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#ReportingPersonName", model.ReportingPersonName);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#ODType", model.ODType);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#ODPlace", model.ODPlace);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#StartDate", Convert.ToDateTime(model.StartDate).ToString("ddd, MMM d, yyyy"));
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#EndDate", Convert.ToDateTime(model.EndDate).ToString("ddd, MMM d, yyyy"));
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#Workingdays", model.Workingdays.ToString());
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#ODComments", model.ODComments);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#ServerName",ServerName);
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#Date", DateTime.Today.ToString("ddd, MMM d, yyyy"));
                         htmlApplyOnDuty = htmlApplyOnDuty.Replace("#CompanyName", objcom);
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             ////MailIds.Add("kirankumar@dsrc.co.in");
                             ////MailIds.Add("francispaul.k.c@dsrc.co.in");
                             //MailIds.Add("vennimalai.n@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objApplyOnDuty.Subject + " - Test Mail Please Ignore", null, htmlApplyOnDuty + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objApplyOnDuty.Subject, "", htmlApplyOnDuty, "HRMS@dsrc.co.in", reportingemail, model.emailaddress, "", Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Apply OnDuty", folder, ServerName);
                     }
                }
            }
            catch (Exception)
            {
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult OutOfOfficeNotification()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1();
            List<NotificationLeaveDetails> list = dbhrms.OutOfOfficeDetails.Where(o => o.ReportingPersonId == userId && o.RequestStatusId == 1) 
                                                      .Select(o =>
                                                        new NotificationLeaveDetails()
                                                        {
                                                            UserName = o.User.FirstName,
                                                            RequestedDateTime = o.RequestedDate
                                                        }).ToList();
            foreach (var item in list)
            {
                if (item.RequestedDateTime != null)
                {
                    if (DateTime.Now.Date == item.RequestedDateTime.Value.Date)
                    {
                        if ((DateTime.Now.Subtract(item.RequestedDateTime.Value).Hours == 0))
                            item.Time = (DateTime.Now.Subtract(item.RequestedDateTime.Value).Minutes) + "Minutes ago";
                        else
                            item.Time = (DateTime.Now.Subtract(item.RequestedDateTime.Value).Hours) + "Hours Ago ";
                    }

                    else
                        item.Time = DateTime.Now.Subtract(item.RequestedDateTime.Value).Days + "Days ago";
                }
            }
            Notification obj = new Notification();
            obj.NotifyCount = list.Count;
            obj.Values = list;
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult WorkingDaysCount(string startdate, string enddate)
        {
            double ODWorkingDays = 0;

            int LeaveUserId = (int)Session["UserId"];
            var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();
            if (startdate != "" && startdate != null && enddate != "" && enddate != null)
            {
                DateTime startDate = Convert.ToDateTime(startdate);
                DateTime endDate = Convert.ToDateTime(enddate);

                DateTime date = startDate;

                List<DateTime?> holidayList = db.AddHolidays.Where(holiday => holiday.Date >= startDate.Date && holiday.Date <= endDate.Date && holiday.ZoneId==UserRegionId && holiday.Isactive==true).Select(item => item.Date ).ToList();

                while (date <= endDate)
                {
                    if (!holidayList.Contains(date) && (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                        ODWorkingDays += 1;

                    date = date.Date.Add(new TimeSpan(24, 00, 00));
                }
            }

            return Json(new { dept = ODWorkingDays }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CancelLeaveRequest(int odid)
        {
            var userId = Session["UserId"];
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var leaveRequestToCancel = db.OutOfOfficeDetails.First(x => x.ODId == odid && x.Userid == (int)userId);

            if (leaveRequestToCancel != null)
            {
                leaveRequestToCancel.RequestStatusId = Convert.ToByte(MasterEnum.RequestStatus.Cancelled.GetHashCode());
                leaveRequestToCancel.StatusUpdatedBy = (int)userId;
                db.SaveChanges();
                return Json(new { Result = "Success" });
            }
            return View();
        }

        public ActionResult SubmittedOutOfOffice()
         {
             int userID = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            
            List<DSRCManagementSystem.Models.OutOfOfficeNotification> objnotification = new List<OutOfOfficeNotification>();

            var Status = (from k in objdb.Master_RequestStatus
                          select new
                          {
                              ID = k.RequestStatusId,
                              Status = k.RequestStatus
                          }).ToList();
            int pending = MasterEnum.RequestStatus.Pending.GetHashCode();
            int reject = MasterEnum.RequestStatus.Rejected.GetHashCode();
            objnotification = (from p in objdb.OutOfOfficeDetails.Where(x => x.ReportingPersonId == userID && x.RequestedDate <= DateTime.Now && (x.RequestStatusId == pending) && x.Ireject == 0 && x.RequestStatusId == reject)
                               join t in objdb.Master_RequestStatus on p.RequestStatusId equals t.RequestStatusId
                                   join b in objdb.Master_OutOfOfficeTypes on p.ODTypeID equals b.ODTypeId
                                   join v in objdb.Users on p.Userid equals v.UserID
                                   select new DSRCManagementSystem.Models.OutOfOfficeNotification
                                   {
                                       ID = p.ODId,
                                       EmployeeName = v.FirstName + "" + v.LastName,
                                       OutStatus = t.RequestStatus,
                                       OutType = b.ODTypeName,
                                       StartDate = p.ODStartDate,
                                       EndDate = p.ODEndDate,
                                       Details = p.ODComments
                                   }).ToList();

            foreach (var item in objnotification)
            {
                DateTime d1 =Convert.ToDateTime( item.StartDate);
                DateTime d2 = Convert.ToDateTime(item.EndDate);
                TimeSpan t = d2-d1;
                double NrOfDays = t.TotalDays;
                item.NoofDays = Convert.ToInt32(NrOfDays);
            }
            foreach (var item in objnotification)
            {
                if (item.OutStatus == "Approved")
                {
                    item.IsApproved = true;
                }
                else
                {
                    item.IsApproved = false;
                }
            }
              ViewBag.Status = new SelectList(Status, "ID", "Status",1);
            return View(objnotification);
        }

        [HttpPost]
        public ActionResult SubmittedOutOfOffice(OutOfOfficeNotification obj, FormCollection form)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ZoneId = (form["ID"] == "") ? "0" : form["ID"].ToString();
            int Id = Convert.ToInt32(ZoneId);
            int userID = Convert.ToInt32(Session["UserID"]);
            if (Id != 0)
            {
                List<DSRCManagementSystem.Models.OutOfOfficeNotification> objvalue = new List<OutOfOfficeNotification>();
                objvalue = (from p in objdb.OutOfOfficeDetails.Where(x => x.RequestStatusId == Id && x.RequestedDate <= DateTime.Now && x.ReportingPersonId==userID )
                            join t in objdb.Master_RequestStatus on p.RequestStatusId equals t.RequestStatusId
                            join b in objdb.Master_OutOfOfficeTypes on p.ODTypeID equals b.ODTypeId
                            join v in objdb.Users on p.Userid equals v.UserID
                            select new DSRCManagementSystem.Models.OutOfOfficeNotification
                            {
                                ID = p.ODId,
                                EmployeeName = v.FirstName + "" + v.LastName,
                                OutStatus = t.RequestStatus,
                                OutType = b.ODTypeName,
                                StartDate = p.ODStartDate,
                                EndDate = p.ODEndDate,
                                Details = p.ODComments,
                                StatusId=t.RequestStatusId
                            }).ToList();
                var Status = (from k in objdb.Master_RequestStatus
                              select new
                              {
                                  ID = k.RequestStatusId,
                                  Status = k.RequestStatus
                              }).ToList();
                if (objvalue.Count() == 0)
                {
                    objvalue = new List<OutOfOfficeNotification>();
                    ViewBag.Status = new SelectList(Status, "ID", "Status", Id);
                    return View(objvalue.ToList());
                }
                foreach (var item in objvalue)
                {

                    ViewBag.Status = new SelectList(Status, "ID", "Status",item.StatusId);

                }
                foreach (var item in objvalue)
                {
                    if (item.OutStatus == "Approved")
                    {
                        item.IsApproved = true;
                    }
                    else
                    {
                        item.IsApproved = false;
                    }
                }
                return View(objvalue);
            }
            else
            {
                return RedirectToAction("SubmittedOutOfOffice", "OOOnDuty");
            }
        }

        [HttpPost]
        public ActionResult ApproveOD(int ID)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
           
            var Update = objdb.OutOfOfficeDetails.Where(x => x.ODId == ID).Select(o => o).FirstOrDefault();
           if (Update !=  null)
            {
                Update.RequestStatusId=2;
                objdb.SaveChanges();
                return Json(new { Result = "Success", JsonRequestBehavior.AllowGet });
            }
          return View();
        }

        [HttpGet]
        public ActionResult RejectOD(int ID)
        {
            TempData["TaskId"] = ID;
            return View();
        }

        [HttpGet]
        public ActionResult ViewDetail(int odid)
        {
            var query = (from rc in db.OutOfOfficeDetails
                         join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                         join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                         join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                         join r in db.UserReportings on rc.ReportingPersonId equals r.ReportingUserID
                         join u in db.Users on r.ReportingUserID equals u.UserID
                         orderby rc.ODStartDate descending
                          where rc.ODId == odid
                         select new OnDutyRequestModel
                         {
                             ODTypeID = rc.ODTypeID,
                             StartDate = rc.ODStartDate,
                             EndDate = rc.ODEndDate,
                             Workingdays = rc.ODWorkingDays,
                             AlternateNo = rc.ODAlternateNo,
                             ApproverName=u.FirstName+" "+(u.LastName??""),
                             ODType = t.ODTypeName,
                             ODPlace = p.ODLocation,
                             RequestStatus = l.Status,
                             ODComments = rc.ODComments,
                             ODID = rc.ODId
                         }).FirstOrDefault();

            return View(query);
        }

        [HttpGet]
        public ActionResult worklist(string Pending)
        {
            
            int pending = MasterEnum.RequestStatus.Pending.GetHashCode();
            int userID = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var statuslist = db.Master_LeaveStatus.ToList();

            if (Pending=="true")
            {

                ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(statuslist), "LeaveStatusId", "Status", pending);
            }
            else
            {
                ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(statuslist), "LeaveStatusId", "Status", 0);
            }

            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            var User = db.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int UserId = Convert.ToInt32(User);
            
            if (Pending =="true")
            {
               var result = (from rc in db.OutOfOfficeDetails.Where(x => x.RequestStatusId == pending)
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userID
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId,
                                  UnderNoticePeriod=u.UserStatus

                              }).OrderByDescending(o => o.StartDate.Value.Year).ThenByDescending(o => o.StartDate.Value.Month).ThenByDescending(o => o.StartDate.Value.Day).ToList();
               return View(result.ToList());
            }
            else
            {
              var  result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userID
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId,
                                   UnderNoticePeriod=u.UserStatus
                              }).OrderByDescending(o => o.StartDate.Value.Year).ThenByDescending(o => o.StartDate.Value.Month).ThenByDescending(o => o.StartDate.Value.Day).ToList();
                return View(result.ToList());
            }
            
        }

        [HttpPost]
        public ActionResult worklist(OnDutyRequestModel model)
        {
            int userId = int.Parse(Session["UserID"].ToString());
            int roleId = int.Parse(Session["RoleID"].ToString());

            int ReqStatusSearchId = Convert.ToInt32(model.RequestStatusId) != 0 ? Convert.ToInt32(model.RequestStatusId) : 0;

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var RequestQuery = from od in db.OutOfOfficeDetails select od;

            var statuslist = db.Master_LeaveStatus.ToList();
            ViewBag.Status_list = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(statuslist), "LeaveStatusId", "Status", model.RequestStatusId);

            if (ReqStatusSearchId == 0)
            {
                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userId
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId
                              }).ToList();

                return View(result);
            }
            else if (ReqStatusSearchId == MasterEnum.RequestStatus.Pending.GetHashCode())
            {
                int pending = MasterEnum.RequestStatus.Pending.GetHashCode();
                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userId && rc.RequestStatusId == pending
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId
                              }).ToList();

                return View(result);
            }
            else if (ReqStatusSearchId == MasterEnum.RequestStatus.Approved.GetHashCode())
            {
                var approved = MasterEnum.RequestStatus.Approved.GetHashCode();
                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userId && rc.RequestStatusId == approved
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId
                              }).ToList();

                return View(result);
            }
            else if (ReqStatusSearchId == MasterEnum.RequestStatus.Rejected.GetHashCode())
            {
                var rejected = MasterEnum.RequestStatus.Rejected.GetHashCode();
                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userId && rc.RequestStatusId == rejected
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,

                                  ODID = rc.ODId
                              }).ToList();

                return View(result);
            }
            else 
            {
                var cancelled= MasterEnum.RequestStatus.Cancelled.GetHashCode();
                var result = (from rc in db.OutOfOfficeDetails
                              join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                              join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                              join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                              join u in db.Users on rc.Userid equals u.UserID
                              orderby rc.ODStartDate descending
                              where rc.ReportingPersonId == userId && rc.RequestStatusId ==cancelled
                              select new OnDutyRequestModel
                              {
                                  ODTypeID = rc.ODTypeID,
                                  EmpName = u.FirstName + " " + (u.LastName ?? ""),
                                  StartDate = rc.ODStartDate,
                                  EndDate = rc.ODEndDate,
                                  Workingdays = rc.ODWorkingDays,
                                  AlternateNo = rc.ODAlternateNo,
                                  ODType = t.ODTypeName,
                                  ODPlace = p.ODLocation,
                                  RequestStatus = l.Status,
                                  ODComments = rc.ODComments,
                                  ODID = rc.ODId
                              }).ToList();

                return View(result);
            }          
        }

        [HttpGet]
        public ActionResult ApproveOnDutyRequest(int ODID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();            

            var query = (from rc in db.OutOfOfficeDetails
                         join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                         join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                         join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                         where rc.ODId == ODID                         
                         select new OnDutyRequestModel
                         {
                             ODTypeID = rc.ODTypeID,
                             StartDate = rc.ODStartDate,
                             EndDate = rc.ODEndDate,
                             Workingdays = rc.ODWorkingDays,
                             AlternateNo = rc.ODAlternateNo,
                             ODType = t.ODTypeName,
                             ODPlace = p.ODLocation,
                             RequestStatus = l.Status,
                             ODComments = rc.ODComments,
                             ODID = rc.ODId
                         }).FirstOrDefault();

            return View(query);
        }
        
        [HttpPost]
        public ActionResult ApproveOnDutyRequest(OnDutyRequestModel model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var ReqToApprove = db.OutOfOfficeDetails.FirstOrDefault(o => o.ODId == model.ODID);

            var EmailAddress = db.Users.FirstOrDefault(o => o.UserID == ReqToApprove.Userid).EmailAddress;
            var Users = db.Users.FirstOrDefault(o => o.UserID == ReqToApprove.Userid) ;
            string EmpName = Users.FirstName + " " + Users.LastName ?? "";

            if (ReqToApprove != null)
            {

                ViewBag.IsalreadyApproved = ReqToApprove.RequestStatusId == 2 ? true : false;
                ViewBag.IsCanceled = ReqToApprove.RequestStatusId == 4 ? true : false;
                ViewBag.IsalreadyRejected = ReqToApprove.RequestStatusId == 3 ? true : false;

                if (ViewBag.IsalreadyApproved)
                {
                    return Json(new { Result = "Approved" }, JsonRequestBehavior.AllowGet);
                }
                else if (ViewBag.IsalreadyRejected)
                {
                    return Json(new { Result = "Rejected" }, JsonRequestBehavior.AllowGet);
                }
                else if (ViewBag.IsalreadyRejected)
                {
                    return Json(new { Result = "Cancelled" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
                    var User = db.Users.Where(x => x.UserName == email).Select(o => o).FirstOrDefault();
                    int UserId = Convert.ToInt32(User.UserID);
                    var reportingemail = db.Users.Where(o => o.UserID == ReqToApprove.ReportingPersonId).Select(o => o.EmailAddress).FirstOrDefault();
                    //Approved By assigned to person.
                    ReqToApprove.RequestStatusId = 2;
                    ReqToApprove.StatusUpdatedBy = UserId;
                    ReqToApprove.MngrComments = model.ManagerComments;
                    db.SaveChanges();
                    
                    model.emailaddress = db.Users.FirstOrDefault(o => o.UserID == UserId).EmailAddress;
                    model.EmpId = db.Users.FirstOrDefault(o => o.UserID == UserId).EmpID;
                    model.EmpName = User.FirstName + " " + (User.LastName ?? "");
                    model.StartDate = ReqToApprove.ODStartDate;
                    model.EndDate = ReqToApprove.ODEndDate;
                    model.ODType = db.Master_OutOfOfficeTypes.FirstOrDefault(o => o.ODTypeId == ReqToApprove.ODTypeID).ODTypeName;
                    model.ODPlace = db.Master_OutOfOfficePlaces.FirstOrDefault(o => o.ODPlaceID == ReqToApprove.ODPlaceID).ODLocation;
                    model.Workingdays = ReqToApprove.ODWorkingDays;
                    model.ODComments = ReqToApprove.ODComments;

                    model.ReportingPersonName = db.Users.Where(o => o.UserID == ReqToApprove.ReportingPersonId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                    var ReqToApproveOnDuty = db.Users.Where(o => o.UserID == ReqToApprove.Userid).FirstOrDefault();
                    ReqToApproveOnDuty.IsExclude = true;
                    db.SaveChanges();

                    //  TimeManagementEntry(model.EmpId, model.StartDate, model.EndDate);  
                    //mail sending coding part for approval

                    var obj = db.Users.Where(o => o.UserID == model.userid).Select(o => o).FirstOrDefault();

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Approve OnDuty").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Approve OnDuty").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objApproveOnDuty = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Approve OnDuty")
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
                         string TemplatePathApproveOnDuty = Server.MapPath(objApproveOnDuty.Template);
                         string htmlApproveOnDuty = System.IO.File.ReadAllText(TemplatePathApproveOnDuty);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#EmpName", EmpName);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#ReportingPersonName", model.ReportingPersonName);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#ODType", model.ODType);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#ODPlace", model.ODPlace);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#StartDate", Convert.ToDateTime(model.StartDate).ToString("ddd, MMM d, yyyy"));
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#EndDate", Convert.ToDateTime(model.EndDate).ToString("ddd, MMM d, yyyy"));
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#Workingdays", model.Workingdays.ToString());
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#Comments", model.ODComments);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#ServerName",ServerName);
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#Date", DateTime.Today.ToString("ddd, MMM d, yyyy"));
                         htmlApproveOnDuty = htmlApproveOnDuty.Replace("#CompanyName", company);
                       //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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

                             string EmailAddres = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddres += mail + ",";
                             }

                             EmailAddres = EmailAddres.Remove(EmailAddres.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objApproveOnDuty.Subject + " - Test Mail Please Ignore", null, htmlApproveOnDuty + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddres, Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objApproveOnDuty.Subject, "", htmlApproveOnDuty, "HRMS@dsrc.co.in", reportingemail, EmailAddress, "umapathy@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Approve OnDuty", folder, ServerName);
                     }

                    return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult RejectOnDutyRequest(int ODID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var query = (from rc in db.OutOfOfficeDetails
                         join t in db.Master_OutOfOfficeTypes on rc.ODTypeID equals t.ODTypeId
                         join p in db.Master_OutOfOfficePlaces on rc.ODPlaceID equals p.ODPlaceID
                         join l in db.Master_LeaveStatus on rc.RequestStatusId equals l.LeaveStatusId
                         where rc.ODId == ODID
                         select new OnDutyRequestModel
                         {
                             ODTypeID = rc.ODTypeID,
                             StartDate = rc.ODStartDate,
                             EndDate = rc.ODEndDate,
                             Workingdays = rc.ODWorkingDays,
                             AlternateNo = rc.ODAlternateNo,
                             ODType = t.ODTypeName,
                             ODPlace = p.ODLocation,
                             RequestStatus = l.Status,
                             ODComments = rc.ODComments,
                             ODID = rc.ODId
                         }).FirstOrDefault();

            return View(query);            
        }

        [HttpPost]
        public ActionResult RejectOnDutyRequest(OnDutyRequestModel model)
        {

            var ReqToReject = db.OutOfOfficeDetails.FirstOrDefault(o => o.ODId == model.ODID);
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var EmailAddress = db.Users.FirstOrDefault(o => o.UserID == ReqToReject.Userid).EmailAddress;
            var Users = db.Users.FirstOrDefault(o => o.UserID == ReqToReject.Userid);
            string EmpName = Users.FirstName + " " + Users.LastName ?? "";
            if (ReqToReject != null)
            {
                ViewBag.IsalreadyApproved = ReqToReject.RequestStatusId == 2 ? true : false;
                ViewBag.IsCanceled = ReqToReject.RequestStatusId == 4 ? true : false;
                ViewBag.IsalreadyRejected = ReqToReject.RequestStatusId == 3 ? true : false;

                if (ViewBag.IsalreadyApproved)
                {
                    return Json(new { Result = "Approved" }, JsonRequestBehavior.AllowGet);
                }
                else if (ViewBag.IsalreadyRejected)
                {
                    return Json(new { Result = "Rejected" }, JsonRequestBehavior.AllowGet);
                }
                else if (ViewBag.IsalreadyRejected)
                {
                    return Json(new { Result = "Cancelled" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
                    var User = db.Users.Where(x => x.UserName == email).Select(o => o).FirstOrDefault();
                    int UserId = Convert.ToInt32(User.UserID);
                   
                    var reportingemail = db.Users.Where(o => o.UserID == ReqToReject.ReportingPersonId).Select(o => o.EmailAddress).FirstOrDefault();
                    //Rejected By assigned to person.
                    ReqToReject.RequestStatusId = 3;
                    ReqToReject.StatusUpdatedBy = UserId;
                    ReqToReject.MngrComments = model.ManagerComments;
                    db.SaveChanges();
                    model.emailaddress = db.Users.FirstOrDefault(o => o.UserID == ReqToReject.Userid).EmailAddress;
                    model.EmpId = db.Users.FirstOrDefault(o => o.UserID == UserId).EmpID;
                    model.EmpName = User.FirstName + " " + (User.LastName ?? "");
                    model.StartDate = ReqToReject.ODStartDate;
                    model.EndDate = ReqToReject.ODEndDate;
                    model.ODType = db.Master_OutOfOfficeTypes.FirstOrDefault(o => o.ODTypeId == ReqToReject.ODTypeID).ODTypeName;
                    model.ODPlace = db.Master_OutOfOfficePlaces.FirstOrDefault(o => o.ODPlaceID == ReqToReject.ODPlaceID).ODLocation;
                    model.Workingdays = ReqToReject.ODWorkingDays;
                    model.ODComments = ReqToReject.ODComments;
                    model.ReportingPersonName = db.Users.Where(o => o.UserID == ReqToReject.ReportingPersonId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                    
                    //mail sending coding part for approval

                    var obj = db.Users.Where(o => o.UserID == model.userid).Select(o => o).FirstOrDefault();

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Reject OnDuty").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Reject OnDuty").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objRejectOnDuty = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Reject OnDuty")
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
                         string TemplatePathRejectOnDuty = Server.MapPath(objRejectOnDuty.Template);
                         string htmlRejectOnDuty = System.IO.File.ReadAllText(TemplatePathRejectOnDuty);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#EmpName", EmpName);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ReportingPersonName", model.ReportingPersonName);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ODType", model.ODType);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ODPlace", model.ODPlace);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#StartDate", Convert.ToDateTime(model.StartDate).ToString("ddd, MMM d, yyyy"));
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#EndDate", Convert.ToDateTime(model.EndDate).ToString("ddd, MMM d, yyyy"));
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#Workingdays", model.Workingdays.ToString());
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ODComments", model.ODComments);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ServerName",ServerName);
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#Date", DateTime.Today.ToString("ddd, MMM d, yyyy"));
                         htmlRejectOnDuty = htmlRejectOnDuty.Replace("#ComapnyName", company);
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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

                             string EmailAddres = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddres += mail + ",";
                             }

                             EmailAddres = EmailAddress.Remove(EmailAddres.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objRejectOnDuty.Subject + " - Test Mail Please Ignore", null, htmlRejectOnDuty + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddres, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {


                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objRejectOnDuty.Subject, "", htmlRejectOnDuty, "HRMS@dsrc.co.in", model.emailaddress, reportingemail, "umapathy@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Reject OnDuty", folder, ServerName);
                     }

                    return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                }
            }
            return View();
        }


        public ActionResult ApplyEmployeeonDuty()
        {
              int UserId = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            var Employeees = (from p in objdb.UserReportings.Where(x => x.ReportingUserID == UserId)
                       join t in objdb.Users on p.UserID equals t.UserID
                       select new
                       {
                           Id5 = p.UserID,
                           EmployeeName = t.FirstName + "" + t.LastName
                       }).ToList();

            var Type = (from p in objdb.Master_OutOfOfficeTypes
                        select new
                        {
                            Id6 = p.ODTypeId,
                            OdType = p.ODTypeName
                        }).ToList();

            var obj = (from p in objdb.UserProjects.Where(x => x.UserID == UserId)
                       join t in objdb.Projects.Where(x => x.IsDeleted == false || x.IsDeleted == null) on p.ProjectID equals t.ProjectID
                       select new
                       {
                           ProjectId = p.ProjectID,
                           ProjectName = t.ProjectName
                       }).OrderBy(x => x.ProjectName).ToList();

            ViewBag.ProjectList = new SelectList(obj, "ProjectId", "ProjectName");
            ViewBag.Users = new SelectList(Employeees, "Id5", "EmployeeName");
            ViewBag.Type = new SelectList(Type, "Id6", "OdType");

            return View();
        }

        [HttpPost]
        public ActionResult ApplyEmployeeonDuty(ApplyEmployeeOutDuty objduty)
        {
            int UserId = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.OutOfOfficeDetail officedetails = new DSRCManagementSystem.OutOfOfficeDetail();
            officedetails.ODStartDate = objduty.StartDate;
            officedetails.ODEndDate = objduty.EndDate;
            TimeSpan t =  objduty.EndDate - objduty.StartDate;
            double NrOfDays = t.TotalDays;
            officedetails.ODWorkingDays = Convert.ToInt32(NrOfDays);
            officedetails.ODTypeID = 1;
            officedetails.ODPlaceID = 1;
            officedetails.ODComments = objduty.Comments;
            officedetails.Userid = Convert.ToInt32(objduty.EmployeeName);
            officedetails.RequestStatusId = Convert.ToByte(MasterEnum.RequestStatus.Approved.GetHashCode());
            officedetails.ReportingPersonId = UserId;
            officedetails.Others = null;
            officedetails.StatusUpdatedBy = UserId;
            officedetails.MngrComments = null;
            officedetails.RequestedDate = System.DateTime.Now;
            officedetails.RejectedUserid = null;
            officedetails.RejectedReason = null;
            officedetails.RejectedDate = null;
            objdb.AddToOutOfOfficeDetails(officedetails);
            objdb.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult RejectOD(NotificatioinReject obj)
        {
             int UserId = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int RejectedODId = Convert.ToInt32(TempData["TaskId"]);
            var RejectedReason = objdb.OutOfOfficeDetails.Where(x => x.ODId == RejectedODId).Select(o => o).FirstOrDefault();
            if (RejectedReason != null)
            {
                RejectedReason.RejectedUserid = UserId;
                RejectedReason.RejectedDate = System.DateTime.Now;
                RejectedReason.RejectedReason = obj.RejectedReason;
                RejectedReason.Ireject = 1;
                objdb.SaveChanges();
            }
            return Json(new { Result = "Success", JsonRequestBehavior.AllowGet });
        }
       
        #region Other Methods

        public void Status()
        {
            var query = db.Master_LeaveStatus.ToList();
            IList<SelectListItem> dropcomp = new List<SelectListItem>();
            foreach (var s in query)
            {
                dropcomp.Add(new SelectListItem { Value = Convert.ToString(s.LeaveStatusId), Text = s.Status });
            }
            ViewBag.Details = dropcomp;
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

        private List<ReportingPerson> GetReportingPersons(int id = 0)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                var reportingPersonId = db.UserReportings.Where(x => x.UserID == userID).Select(x => x.ReportingUserID).ToList();


                List<ReportingPerson> reportingPersons = (from u in db.Users
                                                          where reportingPersonId.Contains(u.UserID)
                                                          select new ReportingPerson
                                                          {
                                                              UserID = u.UserID,
                                                              Name = (u.FirstName + " " + (u.LastName ?? "")).Trim()
                                                          }).OrderBy(o => o.Name).ToList();
                reportingPersons.RemoveAll(x => x.UserID == userID);
                return reportingPersons;

            }
        }

        private List<ReportablePerson> GetReporatablePerson()
        {
            var userId = (int)Session["UserId"];

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                List<ReportablePerson> reportablepersons = (from usr_ropt in db.UserReportings
                                                            join
                                                                usr in db.Users.Where(o => o.IsActive != false) on usr_ropt.UserID equals usr.UserID
                                                            where usr_ropt.ReportingUserID == userId && usr_ropt.UserID != userId
                                                            select new ReportablePerson()
                                                            {
                                                                UserID = usr.UserID,
                                                                Name = (usr.FirstName + " " + (usr.LastName ?? "")).Trim()
                                                            }).OrderBy(o => o.Name).ToList();
                return reportablepersons;
            }

        }

        public void TimeManagementEntry(string EmpId, DateTime? Start, DateTime? End)
        {
            using (DSRCManagementSystemEntities1 db1 = new DSRCManagementSystemEntities1())
            {
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();

                var strconn = ConfigurationManager.ConnectionStrings["DSRCManagementSystemEntitiesForLD"].ConnectionString;
               
                using (SqlConnection con = new SqlConnection(strconn))
                {
                    using (SqlCommand cmd = new SqlCommand("Sp_WorkingDaysCalculation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = Start;
                        cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = End;
                        cmd.Parameters.Add("@ZoneId", SqlDbType.Int).Value = 3;
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ds);

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            var TimeMngEntry = new OutOfOfficeTimeManagement
                            {
                                EmpID = EmpId,
                                InTime = "09:00",
                                OutTime = "18:00",
                                InTimeMin = 540,
                                OutTimeMin = 1080,
                                Date = (DateTime)row["AllDays"],
                                TotalTime = 1080 - 540
                            };

                            db.OutOfOfficeTimeManagements.AddObject(TimeMngEntry);
                            db.SaveChanges();
                        }
                        cmd.Cancel();
                        con.Close();
                    }
                }
            }
        }
        #endregion
    }
}

