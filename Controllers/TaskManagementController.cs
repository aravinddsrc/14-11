using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Web.Configuration;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Globalization;

namespace DSRCManagementSystem.Controllers
{
    public class TaskManagementController : Controller
    {
        private readonly DSRCManagementSystemEntities1 _db = new DSRCManagementSystemEntities1();
        private static readonly TimeZoneInfo IndianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult ManageTask()
        {
            var userid = Convert.ToInt32(Session["UserID"].ToString());
            var obj = (from p in _db.TaskMasters.Where(x => x.IsActive == true && x.CreatedBy == userid)
                join q in _db.Users.Where(x => x.IsActive == true) on p.AssignedTo equals q.UserID
                join tr in _db.Master_TaskRecurring on p.Recurring equals tr.TaskRecurringID
                select new TaskManagement
                {
                    TaskDescription = p.TaskDescription,
                    TaskID = p.TaskMasterID,
                    TaskAssignedToID = p.AssignedTo,
                    AssignedDate = (DateTime)p.AssignedDate,
                    RecurringName = tr.RecurringType,
                    AssignedUser = q.FirstName + " " + q.LastName,
                    SelectedUserStatusid=q.UserStatus
                }).OrderByDescending(o => o.AssignedDate).ToList();


            var curMonth = DateTime.Now.Month;

            var list = obj.Where(x => x.AssignedDate.Month >= curMonth).ToList();
            list.AddRange(obj.Where(x => x.AssignedDate.Month < curMonth));


            return View(list);
        }

        [HttpGet]
        public ActionResult CreateTask()
        {
            var userid = Convert.ToInt32(Session["UserID"].ToString());
            var reportingUsers = (from p in _db.UserReportings.Where(x => x.ReportingUserID == userid)
                                  join q in _db.Users.Where(x => x.IsActive == true && x.UserStatus != 6) on p.UserID equals q.UserID
                                  select new
                                  {
                                      UserID = q.UserID,
                                      UserName = q.FirstName + " " + q.LastName
                                  }).ToList();
            ViewBag.ReportingUsers = new SelectList(reportingUsers, "UserID", "UserName");
            var recurring = (from p in _db.Master_TaskRecurring
                             select new
                             {
                                 RecurringID = p.TaskRecurringID,
                                 RecurringType = p.RecurringType
                             }).ToList();
            ViewBag.Recurring = new SelectList(recurring, "RecurringID", "RecurringType");
            return View();
        }

        [HttpPost]
        public ActionResult CreateTask(TaskManagement task)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var inActive = task.InActive;
                int AssignToId = Convert.ToInt32(task.TaskAssignedToID);
                DateTime Assigneddate = Convert.ToDateTime(task.AssignedDate);
                DateTime Enddate = new DateTime();
                DateTime AssignedOn = Convert.ToDateTime(DateTime.Now);
                int TaskTypeId = Convert.ToInt32(task.RecurringID);
                string TaskName = Convert.ToString(task.TaskDescription);

                if (TaskTypeId == 1)
                {
                    Enddate = Assigneddate;
                }
                if (TaskTypeId == 2)
                {

                    Enddate = Assigneddate.AddDays(7);
                }
                if (TaskTypeId == 3)
                {

                    Enddate = Assigneddate.AddDays(15);
                }
                if (TaskTypeId == 4)
                {

                    Enddate = Assigneddate.AddDays(30);
                }
                string AssignedbyEmailId = System.Web.HttpContext.Current.Application["UserName"].ToString();

                var Assignedbydetails = _db.Users.Where(o => o.EmailAddress == AssignedbyEmailId).Select(o => o).FirstOrDefault();

                string Assignedby = Assignedbydetails.FirstName + " " + Assignedbydetails.LastName;

                var MailAddress = (from u in _db.Users.Where(x => x.UserID == AssignToId)
                                   select (u.EmailAddress)).FirstOrDefault();

                var objuser = _db.Users.Where(o => o.UserID == AssignToId).Select(o => o).FirstOrDefault();

                string UserName = objuser.FirstName + "  " + objuser.LastName;
                string EmailId = Convert.ToString(MailAddress);
                if (_db.TaskMasters.Any(R => R.TaskDescription == task.TaskDescription && R.AssignedDate == task.AssignedDate && R.IsActive == true))
                {
                    //ModelState.AddModelError("TaskDescription", "Task Description Already Exists");
                    //ViewBag.task = task.TaskDescription;
                    return Json("Already exists", JsonRequestBehavior.AllowGet);
                }

                var calendarDetails = _db.CalendarYears.FirstOrDefault();
                var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var calendarModel = new DSRCManagementSystem.Models.Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1, calendarDetails.EndingMonth ?? 12);
                var end = calendarModel.EndDate;

                var createdBy = Convert.ToInt32(Session["UserID"].ToString());
                var createdOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndianZone);
                var daily = MasterEnum.Recurring.Daily.GetHashCode();
                var Weekly = MasterEnum.Recurring.Weekly.GetHashCode();
                var monthlyTwice = MasterEnum.Recurring.FifteenDaysOnce.GetHashCode();
                var Monthly = MasterEnum.Recurring.Monthly.GetHashCode();

                //var check = db.TaskMasters.Where(x => x.AssignedDate == task.AssignedDate && x.Recurring == task.RecurringID && x.AssignedTo==task.TaskAssignedToID).Select(o => o.TaskMasterID).FirstOrDefault();

                //if (check != 0 && task.TaskID != check)
                //{

                //    return Json("Warning", JsonRequestBehavior.AllowGet);
                //}

                try
                {
                    var create = _db.TaskMasters.CreateObject();
                    create.TaskDescription = task.TaskDescription;
                    create.AssignedTo = task.TaskAssignedToID;
                    create.AssignedDate = task.AssignedDate;
                    create.CreatedBy = createdBy;
                    create.CreatedOn = createdOn;
                    create.Recurring = task.RecurringID;
                    create.IsActive = true;
                    create.InCalendar = inActive;
                    _db.TaskMasters.AddObject(create);
                    _db.SaveChanges();
                    var type = _db.Master_TaskRecurring.Where(x => x.TaskRecurringID == task.RecurringID).Select(o => o).FirstOrDefault();
                    var createdtaskid = _db.TaskMasters.Where(x => x.TaskDescription == task.TaskDescription && x.AssignedDate == task.AssignedDate && x.AssignedTo == task.TaskAssignedToID).Select(o => o).FirstOrDefault();

                    if (type.TaskRecurringID == daily)
                    {
                        var dates = new List<DateTime>();
                        for (var dt = task.AssignedDate; dt <= end; dt = dt.AddDays(1))
                        {
                            if (task.SelectedDays.Contains(dt.DayOfWeek.ToString()))
                            {
                                dates.Add(dt);
                            }
                        }
                        foreach (var data in dates)
                        {
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = createdtaskid.TaskMasterID;
                            obj.AssignedDate = data;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            if (inActive == true)
                            {

                                obj.InActive = true;
                            }
                            _db.TaskAssigneds.AddObject(obj);
                            var sdate = data.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var edate = data.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            var inActiveobj = _db.CalenderEvents.CreateObject();
                            var inActiveobjMap = _db.CalendarEventUserMappings.CreateObject();
                            inActiveobj.EventName = task.TaskDescription;
                            inActiveobj.EventDescription = task.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(sdate);
                            inActiveobj.Enddate = Convert.ToDateTime(edate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            inActiveobj.IsActive = inActive;
                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            inActiveobj.Entypo = "entypo-newspaper";
                            _db.CalenderEvents.AddObject(inActiveobj);
                            _db.SaveChanges();
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = task.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                            _db.SaveChanges();
                        }
                    }
                    if (type.TaskRecurringID == Weekly)
                    {
                        var weekly = new List<DateTime>();
                        for (var dt = task.AssignedDate; dt <= end; dt = dt.AddDays(7))
                        {
                            weekly.Add(dt);
                        }
                        foreach (var data in weekly)
                        {
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = createdtaskid.TaskMasterID;
                            obj.AssignedDate = data;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            if (inActive == true)
                            {

                                obj.InActive = true;
                            }
                            _db.TaskAssigneds.AddObject(obj);
                            var sdate = data.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var edate = data.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            var inActiveobj = _db.CalenderEvents.CreateObject();
                            var inActiveobjMap = _db.CalendarEventUserMappings.CreateObject();
                            inActiveobj.EventName = task.TaskDescription;
                            inActiveobj.EventDescription = task.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(sdate);
                            inActiveobj.Enddate = Convert.ToDateTime(edate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            inActiveobj.IsActive = inActive;
                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            inActiveobj.Entypo = "entypo-newspaper";
                            _db.CalenderEvents.AddObject(inActiveobj);
                            _db.SaveChanges();
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = task.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                            _db.SaveChanges();
                        }
                    }
                    if (type.TaskRecurringID == monthlyTwice)
                    {
                        var monthlytwice = new List<DateTime>();
                        for (var dt = task.AssignedDate; dt <= end; dt = dt.AddDays(15))
                        {
                            monthlytwice.Add(dt);
                        }
                        foreach (var data in monthlytwice)
                        {
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = createdtaskid.TaskMasterID;
                            obj.AssignedDate = data;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            if (inActive == true)
                            {

                                obj.InActive = true;
                            }
                            _db.TaskAssigneds.AddObject(obj);
                            var sdate = data.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var edate = data.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            var inActiveobj = _db.CalenderEvents.CreateObject();
                            var inActiveobjMap = _db.CalendarEventUserMappings.CreateObject();
                            inActiveobj.EventName = task.TaskDescription;
                            inActiveobj.EventDescription = task.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(sdate);
                            inActiveobj.Enddate = Convert.ToDateTime(edate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            inActiveobj.IsActive = inActive;
                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            inActiveobj.Entypo = "entypo-newspaper";
                            _db.CalenderEvents.AddObject(inActiveobj);
                            _db.SaveChanges();
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = task.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                            _db.SaveChanges();
                        }
                    }
                    if (type.TaskRecurringID == Monthly)
                    {
                        var monthly = new List<DateTime>();
                        for (var dt = task.AssignedDate; dt <= end; dt = dt.AddMonths(1))
                        {
                            monthly.Add(dt);
                        }
                        foreach (var data in monthly)
                        {
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = createdtaskid.TaskMasterID;
                            obj.AssignedDate = data;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            if (inActive == true)
                            {

                                obj.InActive = true;
                            }
                            _db.TaskAssigneds.AddObject(obj);
                            var sdate = data.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var edate = data.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            var inActiveobj = _db.CalenderEvents.CreateObject();
                            var inActiveobjMap = _db.CalendarEventUserMappings.CreateObject();
                            inActiveobj.EventName = task.TaskDescription;
                            inActiveobj.EventDescription = task.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(sdate);
                            inActiveobj.Enddate = Convert.ToDateTime(edate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            inActiveobj.IsActive = inActive;
                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            inActiveobj.Entypo = "entypo-newspaper";
                            _db.CalenderEvents.AddObject(inActiveobj);
                            _db.SaveChanges();
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = task.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                            _db.SaveChanges();

                        }
                    }
                    var objcom = _db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                    .Select(o => o.AppValue)
                    .FirstOrDefault();
                   // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                     var check = _db.EmailTemplates.Where(x => x.TemplatePurpose == "Task Assignment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= _db.EmailTemplates.Where(o=> o.TemplatePurpose == "Task Assignment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                    {
                    var obj1 = (from p in _db.EmailPurposes.Where(x => x.EmailPurposeName == "Task Assignment")
                                join q in _db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
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
                    string Subject = "Task Assign on " + DateTime.Today.ToString("ddd, MMM d, yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Task Assignment";



                    html = html.Replace("#Taskname", TaskName);
                    html = html.Replace("#Subject", Subject);
                    html = html.Replace("#UserName", UserName);
                    html = html.Replace("#Assignedby", Assignedby);
                    html = html.Replace("#Date", AssignedOn.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#Start", Assigneddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#End", Enddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName", ServerName);
                  
                    if (ServerName  != "http://win2012srv:88/")
                    {
                        List<string> MailIds = _db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        string EmailAddress = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }
                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailId, Server.MapPath(pathvalue.ToString()));

                        });
                    }
                    }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Task Assignment", folder, ServerName);

                     }
                    
                }
                catch(Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    return Json("Failed", JsonRequestBehavior.AllowGet);
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
        public ActionResult EditTask(int TaskID)
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            TaskManagement obj = new TaskManagement();

            var Value = _db.TaskMasters.Where(x => x.TaskMasterID == TaskID && x.IsActive == true).Select(o => o).FirstOrDefault();
            obj.AssignedDate = (DateTime)Value.AssignedDate;
            obj.TaskDescription = Value.TaskDescription;
            obj.TaskAssignedToID = Value.AssignedTo;
            obj.RecurringID = Value.Recurring;
            obj.TaskID = TaskID;
            obj.InActive = Convert.ToBoolean(Value.InCalendar);
                obj.EditDays = new List<string>();
                var Date =
                    (_db.TaskAssigneds.Where(x => x.TaskMasterID == TaskID).Select(o => o.AssignedDate).Take(7)).ToList();
                var Days = new List<string>();
                foreach (var date in Date)
                {
                    Days.Add(date.Value.DayOfWeek.ToString());
                }
                obj.EditDays = Days.Distinct().ToList();
            var ReportingUsers = (from p in _db.UserReportings.Where(x => x.ReportingUserID == userid)
                                  join q in _db.Users.Where(x => x.IsActive == true && x.UserStatus != 6) on p.UserID equals q.UserID
                                  select new
                                  {
                                      UserID = q.UserID,
                                      UserName = q.FirstName + " " + q.LastName
                                  }).ToList();
            ViewBag.ReportingUsers = new SelectList(ReportingUsers, "UserID", "UserName");
            var Recurring = (from p in _db.Master_TaskRecurring
                             select new
                             {
                                 RecurringID = p.TaskRecurringID,
                                 RecurringType = p.RecurringType
                             }).ToList();
            ViewBag.Recurring = new SelectList(Recurring, "RecurringID", "RecurringType");
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditTask(TaskManagement data)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var calendarDetails = _db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
           var calendarModel = new  DSRCManagementSystem.Models.Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1, calendarDetails.EndingMonth ?? 12);
            DateTime End = calendarModel.EndDate;
            var inActive = data.InActive;
            int TaskID = data.TaskID;

            int Daily = MasterEnum.Recurring.Daily.GetHashCode();
            int Weekly = MasterEnum.Recurring.Weekly.GetHashCode();
            int MonthlyTwice = MasterEnum.Recurring.FifteenDaysOnce.GetHashCode();
            int Monthly = MasterEnum.Recurring.Monthly.GetHashCode();


            //var check = db.TaskMasters.Where(x => x.AssignedDate == data.AssignedDate && x.Recurring == data.RecurringID && x.AssignedTo==data.TaskAssignedToID).Select(o => o.TaskMasterID).FirstOrDefault();

            //if (check != 0 && data.TaskID!=check)
            //{

            //    return Json("Warning", JsonRequestBehavior.AllowGet);
            //}

            var sdate = data.AssignedDate.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
            var edate = data.AssignedDate.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";

            var check = _db.TaskMasters.Where(x => x.TaskMasterID == TaskID).Select(o => o).FirstOrDefault();

            if (check.TaskMasterID != 0)
            {
                check.InCalendar = inActive;
                _db.SaveChanges();

                if (inActive == false)
                {

                    var checkEvents = _db.CalenderEvents.Where(x => x.EventTaskId == TaskID).Select(o => o).ToList();
                    foreach (var x in checkEvents)
                    {
                        x.IsActive = inActive;
                        _db.SaveChanges();
                    }
                }
                if (inActive == true)
                    {

                        var checkEvent = _db.CalenderEvents.Where(x => x.EventTaskId == TaskID).Select(o => o).ToList();
                        foreach (var x in checkEvent)
                        {
                            x.IsActive = inActive;
                            _db.SaveChanges();
                        }
                    }


                }

            var value = _db.TaskMasters.Where(x => x.TaskMasterID == data.TaskID && x.IsActive == true).Select(o => o).FirstOrDefault();
            try
            {

                var checkCal = _db.CalenderEvents.Where(x => x.EventTaskId == TaskID ).Select(o => o).ToList();

                foreach (var x in checkCal)
                {
                    x.EventName = data.TaskDescription;
                    x.EventDescription = data.TaskDescription;
                    _db.SaveChanges();
                }
                var checkCalMap = _db.CalenderEvents.Where(x => x.EventTaskId == TaskID).Select(o => o.EventID).ToList();
                foreach (var x in checkCal)
                {
                    var map = _db.CalendarEventUserMappings.Where(y=>y.EventID==x.EventID).Select(o => o).ToList();
                    foreach(var mapid in map)
                    {
                    mapid.UserId = data.TaskAssignedToID;
                    _db.SaveChanges();
                    }
                  
                }
                
                


                if (value.AssignedDate != data.AssignedDate && value.Recurring == data.RecurringID)
                {
                    TimeSpan datediffer = (TimeSpan)(data.AssignedDate - value.AssignedDate);
                    var d = datediffer.TotalDays;
                    
                    var olddate = _db.TaskAssigneds.Where(x => x.IsActive == true && x.TaskMasterID == data.TaskID).Select(o => o.AssignedDate).ToList();
                    foreach (var date in olddate)
                    {
                        // DateTime StartDate = Convert.ToDateTime(date.Value.Date.ToString("dd-MM-yyyy") + " " + "09:00:00");
                        DateTime StartDate = Convert.ToDateTime(date.Value.Date.ToString("dd-MM-yyyy"));
                        var newdate = _db.TaskAssigneds.Where(x => x.IsActive == true && x.TaskMasterID == data.TaskID && x.AssignedDate == date).Select(o => o).FirstOrDefault();
                        var checkEvent = _db.CalenderEvents.Where(e => e.EventTaskId == TaskID && e.IsActive == true).Select(et => et).FirstOrDefault();
                        if (checkEvent != null)
                        {
                            newdate.AssignedDate = newdate.AssignedDate.Value.AddDays(d);
                            DateTime dates = Convert.ToDateTime(newdate.AssignedDate);
                            checkEvent.StartDate = Convert.ToDateTime(dates.ToString("dd-MM-yyyy") + " " + "09:00:00");
                            checkEvent.Enddate = Convert.ToDateTime(dates.ToString("dd-MM-yyyy") + " " + "18:00:00"); ;
                            _db.SaveChanges();
                        }
                    }
                }
               // if (value.Recurring != data.RecurringID)
               // {
                    var deletetask = _db.TaskAssigneds.Where(x => x.TaskMasterID == data.TaskID).Select(o => o).ToList();
                    foreach (var deltask in deletetask)
                    {
                        _db.TaskAssigneds.DeleteObject(deltask);
                        _db.SaveChanges();
                    }
                   
                    var delete = _db.CalenderEvents.Where(x => x.EventTaskId == data.TaskID).Select(o => o.EventID).ToList();
                    foreach (var del in delete)
                    {
                        var deleteMap = _db.CalendarEventUserMappings.Where(x => x.EventID == del).Select(o => o).FirstOrDefault();
                        _db.CalendarEventUserMappings.DeleteObject(deleteMap);
                        _db.SaveChanges();
                    }
                        
                 
                    var deletetasks = _db.CalenderEvents.Where(x => x.EventTaskId == data.TaskID).Select(o => o).ToList();
                    foreach (var deltasks in deletetasks)
                    {
                       
                        _db.CalenderEvents.DeleteObject(deltasks);
                        _db.SaveChanges();
                    }

                    if (data.RecurringID == Daily)
                    {
                        var dates = new List<DateTime>();
                        for (var dt = data.AssignedDate; dt <= End; dt = dt.AddDays(1))
                        {
                            if (data.SelectedDays.Contains(dt.DayOfWeek.ToString()))
                            {
                                dates.Add(dt);
                            }
                        }
                        foreach (var task in dates)
                        {
                            var tsddate = task.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var teddate = task.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = data.TaskID;
                            obj.AssignedDate = task;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            _db.TaskAssigneds.AddObject(obj);
                            _db.SaveChanges();
                            DSRCManagementSystem.CalenderEvent inActiveobj = new DSRCManagementSystem.CalenderEvent();
                            DSRCManagementSystem.CalendarEventUserMapping inActiveobjMap = new DSRCManagementSystem.CalendarEventUserMapping();
                            inActiveobj.EventName = data.TaskDescription;
                            inActiveobj.EventDescription = data.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(tsddate);
                            inActiveobj.Enddate = Convert.ToDateTime(teddate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            if (inActive == false)
                            {
                                inActiveobj.IsActive = false;
                            }
                            if (inActive == true)
                            {
                                inActiveobj.IsActive = true;
                            }

                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            _db.CalenderEvents.AddObject((inActiveobj));
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = data.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                        }
                    }
                    if (data.RecurringID == Weekly)
                    {
                        var weekly = new List<DateTime>();
                        for (var dt = data.AssignedDate; dt <= End; dt = dt.AddDays(7))
                        {
                            weekly.Add(dt);
                        }
                        foreach (var task in weekly)
                        {
                            var tsddate = task.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var teddate = task.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = data.TaskID;
                            obj.AssignedDate = task;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            _db.TaskAssigneds.AddObject(obj);
                            _db.SaveChanges();
                            DSRCManagementSystem.CalenderEvent inActiveobj = new DSRCManagementSystem.CalenderEvent();
                            DSRCManagementSystem.CalendarEventUserMapping inActiveobjMap = new DSRCManagementSystem.CalendarEventUserMapping();
                            inActiveobj.EventName = data.TaskDescription;
                            inActiveobj.EventDescription = data.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(tsddate);
                            inActiveobj.Enddate = Convert.ToDateTime(teddate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            if (inActive == false)
                            {
                                inActiveobj.IsActive = false;
                            }
                            if (inActive == true)
                            {
                                inActiveobj.IsActive = true;
                            }

                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            _db.CalenderEvents.AddObject((inActiveobj));
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = data.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                        }
                    }
                    if (data.RecurringID == MonthlyTwice)
                    {
                        var monthlytwice = new List<DateTime>();
                        for (var dt = data.AssignedDate; dt <= End; dt = dt.AddDays(15))
                        {
                            monthlytwice.Add(dt);
                        }
                        foreach (var task in monthlytwice)
                        {
                            var tsddate = task.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var teddate = task.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = data.TaskID;
                            obj.AssignedDate = task;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            _db.TaskAssigneds.AddObject(obj);
                            _db.SaveChanges();
                            DSRCManagementSystem.CalenderEvent inActiveobj = new DSRCManagementSystem.CalenderEvent();
                            DSRCManagementSystem.CalendarEventUserMapping inActiveobjMap = new DSRCManagementSystem.CalendarEventUserMapping();
                            inActiveobj.EventName = data.TaskDescription;
                            inActiveobj.EventDescription = data.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(tsddate);
                            inActiveobj.Enddate = Convert.ToDateTime(teddate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            if (inActive == false)
                            {
                                inActiveobj.IsActive = false;
                            }
                            if (inActive == true)
                            {
                                inActiveobj.IsActive = true;
                            }

                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            _db.CalenderEvents.AddObject((inActiveobj));
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = data.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                        }
                    }
                    if (data.RecurringID == Monthly)
                    {
                        var monthly = new List<DateTime>();
                        for (var dt = data.AssignedDate; dt <= End; dt = dt.AddMonths(1))
                        {
                            monthly.Add(dt);
                        }
                        foreach (var task in monthly)
                        {
                            var tsddate = task.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var teddate = task.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            DSRCManagementSystem.TaskAssigned obj = new DSRCManagementSystem.TaskAssigned();
                            obj.TaskMasterID = data.TaskID;
                            obj.AssignedDate = task;
                            obj.RecurringStatus = 3;
                            obj.IsActive = true;
                            _db.TaskAssigneds.AddObject(obj);
                            _db.SaveChanges();
                            DSRCManagementSystem.CalenderEvent inActiveobj = new DSRCManagementSystem.CalenderEvent();
                            DSRCManagementSystem.CalendarEventUserMapping inActiveobjMap = new DSRCManagementSystem.CalendarEventUserMapping();
                            inActiveobj.EventName = data.TaskDescription;
                            inActiveobj.EventDescription = data.TaskDescription;
                            inActiveobj.StartDate = Convert.ToDateTime(tsddate);
                            inActiveobj.Enddate = Convert.ToDateTime(teddate);
                            inActiveobj.StartTime = "09:00";
                            inActiveobj.EndTime = "18:00";
                            inActiveobj.ColorCode = "#2d56ba";
                            inActiveobj.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            inActiveobj.CreatedDate = DateTime.Now;
                            if (inActive == false)
                            {
                                inActiveobj.IsActive = false;
                            }
                            if (inActive == true)
                            {
                                inActiveobj.IsActive = true;
                            }

                            inActiveobj.EventTaskId = obj.TaskMasterID;
                            _db.CalenderEvents.AddObject((inActiveobj));
                            inActiveobjMap.EventID = inActiveobj.EventID;
                            inActiveobjMap.UserId = data.TaskAssignedToID;
                            _db.CalendarEventUserMappings.AddObject(inActiveobjMap);
                        }
                    }
               // }
                value.TaskDescription = data.TaskDescription;
                value.AssignedTo = data.TaskAssignedToID;
                value.AssignedDate = data.AssignedDate;
                value.Recurring = data.RecurringID;
                _db.SaveChanges();
                var objcom = _db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                .Select(o => o.AppValue)
                .FirstOrDefault();
                int AssignToId = Convert.ToInt32(data.TaskAssignedToID);
                DateTime Assigneddate = Convert.ToDateTime(data.AssignedDate);
                DateTime Enddate = new DateTime();
                DateTime AssignedOn = Convert.ToDateTime(DateTime.Now);
                int TaskTypeId = Convert.ToInt32(data.RecurringID);
                string TaskName = Convert.ToString(data.TaskDescription);

                if (TaskTypeId == 1)
                {
                    Enddate = Assigneddate;
                }
                if (TaskTypeId == 2)
                {

                    Enddate = Assigneddate.AddDays(7);
                }
                if (TaskTypeId == 3)
                {

                    Enddate = Assigneddate.AddDays(15);
                }
                if (TaskTypeId == 4)
                {

                    Enddate = Assigneddate.AddDays(30);
                }
                string AssignedbyEmailId = System.Web.HttpContext.Current.Application["UserName"].ToString();

                var Assignedbydetails = _db.Users.Where(o => o.EmailAddress == AssignedbyEmailId).Select(o => o).FirstOrDefault();

                string Assignedby = Assignedbydetails.FirstName + " " + Assignedbydetails.LastName;

                var MailAddress = (from u in _db.Users.Where(x => x.UserID == AssignToId)
                                   select (u.EmailAddress)).FirstOrDefault();

                var objuser = _db.Users.Where(o => o.UserID == AssignToId).Select(o => o).FirstOrDefault();

                string UserName = objuser.FirstName + "  " + objuser.LastName;
                string EmailId = Convert.ToString(MailAddress);
               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];


                 var checks = _db.EmailTemplates.Where(x => x.TemplatePurpose == "Task ReAssignment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                var folder= _db.EmailTemplates.Where(o=> o.TemplatePurpose == "Task ReAssignment").Select(x=> x.TemplatePath).FirstOrDefault();
                if ((checks != null) && (checks != 0))
                {
                    var obj1 = (from p in _db.EmailPurposes.Where(x => x.EmailPurposeName == "Task ReAssignment")
                                join q in _db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
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
                    string Subject = "Task Modified on " + DateTime.Today.ToString("ddd, MMM d, yyyy");
                    obj1.Subject = " " + objcom + " Management Portal- Task Modified";



                    html = html.Replace("#Taskname", TaskName);
                    html = html.Replace("#Subject", Subject);
                    html = html.Replace("#UserName", UserName);
                    html = html.Replace("#Assignedby", Assignedby);
                    html = html.Replace("#Date", AssignedOn.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#Start", Assigneddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#End", Enddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#CompanyName", objcom.ToString());

                    html = html.Replace("#ServerName", ServerName);
                    if (ServerName  != "http://win2012srv:88/")
                    {
                        List<string> MailIds = _db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        string EmailAddress = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }
                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailId, Server.MapPath(pathvalue.ToString()));

                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Task ReAssignment", folder, ServerName);

                }

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }



        }

        [HttpPost]
        public ActionResult DeleteTask(int TaskID)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var value = _db.TaskMasters.Where(x => x.TaskMasterID == TaskID && x.IsActive == true).Select(o => o).FirstOrDefault();
                value.IsActive = false;
                _db.SaveChanges();
                var data = _db.TaskAssigneds.Where(x => x.TaskMasterID == TaskID && x.IsActive == true).Select(o => o).ToList();
                foreach (var task in data)
                {
                    task.IsActive = false;
                    _db.SaveChanges();
                }
                var dataEvents = _db.CalenderEvents.Where(x => x.EventTaskId == TaskID).Select(o => o).ToList();
                foreach (var task in dataEvents)
                {
                    task.IsActive = false;
                    _db.SaveChanges();
                }
                var objcom = _db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
               .Select(o => o.AppValue)
               .FirstOrDefault();
                int AssignToId = Convert.ToInt32(value.AssignedTo);
                DateTime Assigneddate = Convert.ToDateTime(value.AssignedDate);
                DateTime Enddate = new DateTime();
                DateTime AssignedOn = Convert.ToDateTime(DateTime.Now);
                string TaskName = Convert.ToString(value.TaskDescription);

                
                string AssignedbyEmailId = System.Web.HttpContext.Current.Application["UserName"].ToString();

                var Assignedbydetails = _db.Users.Where(o => o.EmailAddress == AssignedbyEmailId).Select(o => o).FirstOrDefault();

                string Assignedby = Assignedbydetails.FirstName + " " + Assignedbydetails.LastName;

                var MailAddress = (from u in _db.Users.Where(x => x.UserID == AssignToId)
                                   select (u.EmailAddress)).FirstOrDefault();

                var objuser = _db.Users.Where(o => o.UserID == AssignToId).Select(o => o).FirstOrDefault();

                string UserName = objuser.FirstName + "  " + objuser.LastName;
                string EmailId = Convert.ToString(MailAddress);
                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                 var check = _db.EmailTemplates.Where(x => x.TemplatePurpose == "Task UnAssignment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                var folder= _db.EmailTemplates.Where(o=> o.TemplatePurpose == "Task UnAssignment").Select(x=> x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {

                    var obj1 = (from p in _db.EmailPurposes.Where(x => x.EmailPurposeName == "Task UnAssignment")
                                join q in _db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
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
                    string Subject = "Task Deleted on " + DateTime.Today.ToString("ddd, MMM d, yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-Task Deleted ";



                    html = html.Replace("#Taskname", TaskName);
                    html = html.Replace("#Subject", Subject);
                    html = html.Replace("#UserName", UserName);
                    html = html.Replace("#Assignedby", Assignedby);
                    html = html.Replace("#Date", AssignedOn.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#Start", Assigneddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#End", Enddate.ToString("ddd, MMM d, yyyy"));
                    html = html.Replace("#CompanyName", objcom.ToString());

                    html = html.Replace("#ServerName", ServerName);

                    if (ServerName  != "http://win2012srv:88/")
                    {
                        List<string> MailIds = _db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        string EmailAddress = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }
                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, AssignedbyEmailId, EmailId, Server.MapPath(pathvalue.ToString()));

                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Task UnAssignment", folder, ServerName);

                }

               
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AssignTask()
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            List<TaskManagement> obj = new List<TaskManagement>();
            obj = (from p in _db.TaskMasters.Where(x => x.IsActive == true && x.AssignedTo == userid)
                   join q in _db.TaskAssigneds.Where(x => x.IsActive == true) on p.TaskMasterID equals q.TaskMasterID
                   select new TaskManagement
                   {
                       TaskDescription = p.TaskDescription,
                       AssignedTaskID = q.TaskAssignedID,
                       AssignedDate = (DateTime)q.AssignedDate,
                       ActionID = (int)q.RecurringStatus,
                       Comments = q.Comments
                   }).OrderByDescending(o => o.AssignedDate).ToList();

            var StatusList = (from p in _db.Actions
                              select new { ActionID = p.ActionID, ActionStatus = p.ActionName }).ToList();
            ViewBag.ActionList = StatusList;

            return View(obj);
        }

        [HttpGet]
        public ActionResult UpdateComments(int TaskID, int StatusID, string AssignedDate)
        {
            TaskManagement editobj = new TaskManagement();
            editobj.Comments = _db.TaskAssigneds.Where(x => x.IsActive == true && x.TaskAssignedID == TaskID).Select(o => o.Comments).FirstOrDefault();
            editobj.AssignedDate = Convert.ToDateTime(AssignedDate);
            ViewBag.Status = _db.Actions.Where(x => x.ActionID == StatusID).Select(o => o.ActionName).FirstOrDefault();
            ViewBag.AssignedTaskID = TaskID;
            return View(editobj);
        }

        [HttpPost]
        public ActionResult UpdateComments(TaskManagement data)
        {
            try
            {
                var value = _db.TaskAssigneds.Where(x => x.TaskAssignedID == data.AssignedTaskID && x.IsActive == true).Select(o => o).FirstOrDefault();
                value.Comments = data.Comments;
                if (data.AssignedDate != null && data.AssignedDate != DateTime.MinValue)
                {
                    value.AssignedDate = data.AssignedDate;
                }
                _db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateStatus(int AssignedTaskID, int StatusID)
        {
            try
            {
                var value = _db.TaskAssigneds.Where(x => x.TaskAssignedID == AssignedTaskID && x.IsActive == true).Select(o => o).FirstOrDefault();
                value.RecurringStatus = StatusID;
                _db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ViewAssignedTask()
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            DateTime Upto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndianZone);
            List<TaskManagement> obj = new List<TaskManagement>();
            //obj = (from p in _db.TaskMasters.Where(x => x.IsActive == true && x.CreatedBy == userid)
            //       join q in _db.TaskAssigneds.Where(x => x.IsActive == true && x.AssignedDate <= Upto) on p.TaskMasterID equals q.TaskMasterID
            //       join r in _db.Actions on q.RecurringStatus equals r.ActionID
            //       join s in _db.Users.Where(x => x.IsActive == true && x.UserStatus != 6) on p.AssignedTo equals s.UserID
            //       select new TaskManagement
            //       {
            //           TaskDescription = p.TaskDescription,
            //           AssignedTaskID = q.TaskAssignedID,
            //           AssignedDate = (DateTime)q.AssignedDate,
            //           ActionID = (int)q.RecurringStatus,
            //           StatusName = r.ActionName,
            //           Comments = q.Comments,
            //           AssignedUser = s.FirstName + " " + s.LastName,
            //           SelectedUserStatusid = s.UserStatus
            //       }).OrderByDescending(o => o.AssignedDate).ToList();

            //var StatusList = (from p in _db.Actions
            //                  select new { ActionID = p.ActionID, ActionStatus = p.ActionName }).ToList();
            //ViewBag.ActionList = StatusList;

            //List<TaskManagement> List = new List<TaskManagement>();
            //var curMonth = DateTime.Now.Month;

            //foreach (var x in obj)
            //{
            //    if (x.AssignedDate.Month >= curMonth)
            //    {
            //        List.Add(x);
            //    }

            //}

            //foreach (var x in obj)
            //{
            //    if (x.AssignedDate.Month < curMonth)
            //    {
            //        List.Add(x);
            //    }

            //}
            // return View(List);

            obj = (from p in _db.TaskMasters.Where(x => x.IsActive == true && (x.CreatedBy == userid && x.AssignedTo == userid) || x.CreatedBy == userid)
                   join q in _db.TaskAssigneds.Where(x => x.IsActive == true) on p.TaskMasterID equals q.TaskMasterID
                   join r in _db.Actions on q.RecurringStatus equals r.ActionID
                   join s in _db.Users.Where(x => x.IsActive == true && x.UserStatus != 6) on p.AssignedTo equals s.UserID
                   select new TaskManagement
                   {
                       TaskDescription = p.TaskDescription,
                       AssignedTaskID = q.TaskAssignedID,
                       AssignedDate = (DateTime)q.AssignedDate,
                       ActionID = (int)q.RecurringStatus,
                       StatusName = r.ActionName,
                       AssignedUser = s.FirstName + " " + s.LastName,
                       Comments = q.Comments,
                       SelectedUserStatusid = s.UserStatus
                   }).OrderByDescending(o => o.AssignedDate).ToList();
            var StatusList = (from p in _db.Actions
                              select new { ActionID = p.ActionID, ActionStatus = p.ActionName }).ToList();
            ViewBag.ActionList = StatusList;

            List<TaskManagement> List = new List<TaskManagement>();
            var curMonth = DateTime.Now.Month;

            foreach (var x in obj)
            {
                if (x.AssignedDate.Month >= curMonth)
                {
                    List.Add(x);
                }

            }

            foreach (var x in obj)
            {
                if (x.AssignedDate.Month < curMonth)
                {
                    List.Add(x);
                }

            }

            return View(obj);

            
        }



        [HttpGet]
        public ActionResult DashBoardAssignTask()
        {
            int userid = Convert.ToInt32(Session["UserID"].ToString());
            List<TaskManagement> obj = new List<TaskManagement>();
            obj = (from p in _db.TaskMasters.Where(x => x.IsActive == true && x.AssignedTo == userid)
                   join q in _db.TaskAssigneds.Where(x => x.IsActive == true) on p.TaskMasterID equals q.TaskMasterID
                   select new TaskManagement
                   {
                       TaskDescription = p.TaskDescription,
                       AssignedTaskID = q.TaskAssignedID,
                       AssignedDate = (DateTime)q.AssignedDate,
                       ActionID = (int)q.RecurringStatus,
                       Comments = q.Comments
                   }).OrderByDescending(o => o.AssignedDate).ToList();

            var StatusList = (from p in _db.Actions
                              select new { ActionID = p.ActionID, ActionStatus = p.ActionName }).ToList();
            ViewBag.ActionList = StatusList;

            return View(obj);
        }

    }
}
