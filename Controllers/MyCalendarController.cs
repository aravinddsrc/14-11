using DSRCManagementSystem.DSRCLogic;
using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class MyCalendarController : Controller
    {
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        //
        // GET: /MyCalendar/
        [HttpGet]
        public ActionResult MyCalendar()
        {
            int sessionuserid = Convert.ToInt32(Session["UserID"]);

            var varReportingPersons = (from ur in objdb.UserReportings
                                       where ur.ReportingUserID == sessionuserid
                                       join u in objdb.Users on ur.UserID equals u.UserID
                                       where u.IsActive == true && u.UserStatus != 6
                                       select new
                                       {
                                           ReportingUserid = u.UserID,
                                           ReportingUsername = u.FirstName + " " + u.LastName
                                       }).ToList();
            ViewBag.ReportingPersons = new SelectList(varReportingPersons, "ReportingUserid", "ReportingUsername");

            var varUsers = (from u in objdb.Users.Where(u => u.IsActive == true && u.UserStatus != 6)
                            select new
                            {
                                Userid = u.UserID,
                                Username = u.FirstName + "" + u.LastName
                            }).ToList();


            List<int> SelectedUserid = new List<int>();
            SelectedUserid.Add(Convert.ToInt32(sessionuserid));

            ViewBag.Members = new MultiSelectList(varUsers, "Userid", "Username", SelectedUserid);
            var eventUser = objdb.EventPermissions.Where(e => e.UserID == sessionuserid).Select(p => p).FirstOrDefault();
            var eventCreator = objdb.CalenderEvents.Where(c => c.CreatedBy == sessionuserid).Select(c => c).FirstOrDefault();
            var eventMembers = (from c in objdb.CalenderEvents
                                where c.IsActive == true
                                join um in objdb.CalendarEventUserMappings on c.EventID equals um.EventID
                                where c.CreatedBy != um.UserId && um.UserId == sessionuserid
                                select new
                                {
                                    members = um.UserId
                                }).ToList();
            var EventTaskID = objdb.CalenderEvents.Where(x => x.EventTaskId == null).ToList();
            List<int> taskId = new List<int>();
            foreach (var item in EventTaskID)
            {

                taskId.Add(item.EventID);

            }
            ViewBag.calendar = taskId;
            var isReporting = objdb.UserReportings.Where(u => u.ReportingUserID == sessionuserid).FirstOrDefault();
            if (isReporting != null)
            {
                ViewBag.IsReportingPerson = true;
            }
            if (eventUser != null)
            {
                ViewBag.Aurthorized = eventUser.IsAurthorized;
            }
            if (eventCreator != null)
            {
                if (eventCreator.CreatedBy == sessionuserid)
                {
                    ViewBag.EventCreator = true;
                }
            }
            foreach (var user in eventMembers)
            {
                ViewBag.EventMember = true;
            }
            return View();
        }

        [HttpGet]
        public ActionResult CreateEvent()
        {
            var varUsers = (from u in objdb.Users.Where(u => u.IsActive == true && u.UserStatus != 6)
                            select new
                            {
                                Userid = u.UserID,
                                Username = u.FirstName + " " + u.LastName
                            }).ToList();

            int sessionuserid = Convert.ToInt32(Session["UserID"]);

            List<int> SelectedUserid = new List<int>();
            SelectedUserid.Add(Convert.ToInt32(sessionuserid));

            ViewBag.Members = new MultiSelectList(varUsers, "Userid", "Username", SelectedUserid);

            var Recurring = (from p in objdb.Master_TaskRecurring
                             select new
                             {
                                 RecurringID = p.TaskRecurringID,
                                 RecurringType = p.RecurringType
                             }).ToList();
            ViewBag.Recurring = new SelectList(Recurring, "RecurringID", "RecurringType");

            return View();
        }

        [HttpPost]
        public ActionResult CreateEvent(CalendarEventModel model)
        {

            //var varCalenderEvents = objdb.CalenderEvents.CreateObject();
            //varCalenderEvents.EventName = model.EventName;
            //varCalenderEvents.EventDescription = model.EventDescription;
            //varCalenderEvents.StartDate = model.StartDate;
            //varCalenderEvents.Enddate = model.EndDate;
            //varCalenderEvents.StartTime = model.StartTime;
            //varCalenderEvents.EndTime = model.EndTime;
            //varCalenderEvents.ColorCode = model.ColorCode;
            //varCalenderEvents.CreatedBy = Convert.ToInt32(Session["UserID"]);
            //varCalenderEvents.CreatedDate = DateTime.Now;
            //varCalenderEvents.IsActive = true;
            //varCalenderEvents.Entypo = "entypo-network";
            //objdb.CalenderEvents.AddObject(varCalenderEvents);
            //objdb.SaveChanges();

            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DateTime Assigneddate = Convert.ToDateTime(model.StartDate);
            int EventTypeId = Convert.ToInt32(model.RecurringID);
            DateTime Enddate = new DateTime();

            //if (EventTypeId == 1)
            //{
            //    Enddate = Assigneddate;
            //}
            //if (EventTypeId == 2)
            //{

            //    Enddate = Assigneddate.AddDays(7);
            //}
            //if (EventTypeId == 3)
            //{

            //    Enddate = Assigneddate.AddDays(15);
            //}
            //if (EventTypeId == 4)
            //{

            //    Enddate = Assigneddate.AddDays(30);
            //}

            var daily = MasterEnum.Recurring.Daily.GetHashCode();
                                                var Weekly = MasterEnum.Recurring.Weekly.GetHashCode();
            var monthlyTwice = MasterEnum.Recurring.FifteenDaysOnce.GetHashCode();
            var Monthly = MasterEnum.Recurring.Monthly.GetHashCode();



            var calendarDetails = objdb.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new DSRCManagementSystem.Models.Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1, calendarDetails.EndingMonth ?? 12);
            var end = calendarModel.EndDate;


           



            if (model.RecurringID == Monthly)//Monthly
            {
                var monthly = new List<DateTime>();
                for (var dt = model.StartDate; dt <= model.EndDate; dt = dt.Value.AddMonths(1))
                {
                    monthly.Add(dt.Value);
                }




                foreach (var data in monthly)
                {
                    var varCalenderEvents = objdb.CalenderEvents.CreateObject();
                    var sdate = data.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                    var edate = data.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                        varCalenderEvents.EventName = model.EventName;
                        varCalenderEvents.EventDescription = model.EventDescription;
                        varCalenderEvents.StartDate = Convert.ToDateTime(sdate);
                        //varCalenderEvents.Enddate = Convert.ToDateTime(edate);
                        varCalenderEvents.Enddate = model.EndDate;
                        varCalenderEvents.StartTime = model.StartTime;
                        varCalenderEvents.EndTime = model.EndTime;
                        varCalenderEvents.ColorCode = model.ColorCode;
                        varCalenderEvents.CreatedBy = Convert.ToInt32(Session["UserID"]);
                        varCalenderEvents.CreatedDate = DateTime.Now;
                        varCalenderEvents.IsActive = true;
                        varCalenderEvents.Entypo = "entypo-network";
                        varCalenderEvents.RecurringTypeID = model.RecurringID;
                        objdb.CalenderEvents.AddObject(varCalenderEvents);
                        objdb.SaveChanges();

                 

                       
                    List<int?> memberlist = new List<int?>();
                    string[] members = model.Members.Split(',');
                    for (int i = 0; i < members.Count(); i++)
                    {
                        memberlist.Add(Convert.ToInt32(members[i]));
                    }

                    foreach (int mlist in memberlist)
                    {
                        var varCEUM = objdb.CalendarEventUserMappings.CreateObject();
                        varCEUM.EventID = varCalenderEvents.EventID;
                        varCEUM.UserId = Convert.ToInt32(mlist);
                        objdb.CalendarEventUserMappings.AddObject(varCEUM);
                        objdb.SaveChanges();
                    }


                   }


                }




            if (model.RecurringID == Weekly)//weekly
            {
                var weekly = new List<DateTime>();
                for (var dt = model.StartDate; dt <= model.EndDate; dt = dt.Value.AddDays(7))
                {
                    weekly.Add(dt.Value);
                }
               




                    foreach (var dataweekly in weekly)
                    {
                        var varCalenderEvents = objdb.CalenderEvents.CreateObject();
                        var sdate = dataweekly.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                        var edate = dataweekly.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                        varCalenderEvents.EventName = model.EventName;
                        varCalenderEvents.EventDescription = model.EventDescription;
                        varCalenderEvents.StartDate = Convert.ToDateTime(sdate);
                        //varCalenderEvents.Enddate = Convert.ToDateTime(edate);
                        varCalenderEvents.Enddate = model.EndDate;
                        varCalenderEvents.StartTime = model.StartTime;
                        varCalenderEvents.EndTime = model.EndTime;
                        varCalenderEvents.ColorCode = model.ColorCode;
                        varCalenderEvents.CreatedBy = Convert.ToInt32(Session["UserID"]);
                        varCalenderEvents.CreatedDate = DateTime.Now;
                        varCalenderEvents.IsActive = true;
                        varCalenderEvents.Entypo = "entypo-network";
                        varCalenderEvents.RecurringTypeID = model.RecurringID;
                        objdb.CalenderEvents.AddObject(varCalenderEvents);
                        objdb.SaveChanges();




                        List<int?> memberlist = new List<int?>();
                        string[] members = model.Members.Split(',');
                        for (int i = 0; i < members.Count(); i++)
                        {
                            memberlist.Add(Convert.ToInt32(members[i]));
                        }

                        foreach (int mlist in memberlist)
                        {
                            var varCEUM = objdb.CalendarEventUserMappings.CreateObject();
                            varCEUM.EventID = varCalenderEvents.EventID;
                            varCEUM.UserId = Convert.ToInt32(mlist);
                            objdb.CalendarEventUserMappings.AddObject(varCEUM);
                            objdb.SaveChanges();
                        }


                    }


                }


            if (model.RecurringID == monthlyTwice)
                    {
                        var monthlytwice = new List<DateTime>();
                        for (var dt = model.StartDate; dt <= model.EndDate; dt = dt.Value.AddDays(15))
                        {
                            monthlytwice.Add(dt.Value);
                        }
                        foreach (var datamonthlytwice in monthlytwice)
                        {

                            var varCalenderEvents = objdb.CalenderEvents.CreateObject();
                            var sdate = datamonthlytwice.Date.ToString("dd-MM-yyyy") + " " + "09:00:00";
                            var edate = datamonthlytwice.Date.ToString("dd-MM-yyyy") + " " + "18:00:00";
                            varCalenderEvents.EventName = model.EventName;
                            varCalenderEvents.EventDescription = model.EventDescription;
                            varCalenderEvents.StartDate = Convert.ToDateTime(sdate);
                            //varCalenderEvents.Enddate = Convert.ToDateTime(edate);
                            varCalenderEvents.Enddate = model.EndDate;
                            varCalenderEvents.StartTime = model.StartTime;
                            varCalenderEvents.EndTime = model.EndTime;
                            varCalenderEvents.ColorCode = model.ColorCode;
                            varCalenderEvents.CreatedBy = Convert.ToInt32(Session["UserID"]);
                            varCalenderEvents.CreatedDate = DateTime.Now;
                            varCalenderEvents.IsActive = true;
                            varCalenderEvents.Entypo = "entypo-network";
                            varCalenderEvents.RecurringTypeID = model.RecurringID;
                            objdb.CalenderEvents.AddObject(varCalenderEvents);
                            objdb.SaveChanges();




                            List<int?> memberlist = new List<int?>();
                            string[] members = model.Members.Split(',');
                            for (int i = 0; i < members.Count(); i++)
                            {
                                memberlist.Add(Convert.ToInt32(members[i]));
                            }

                            foreach (int mlist in memberlist)
                            {
                                var varCEUM = objdb.CalendarEventUserMappings.CreateObject();
                                varCEUM.EventID = varCalenderEvents.EventID;
                                varCEUM.UserId = Convert.ToInt32(mlist);
                                objdb.CalendarEventUserMappings.AddObject(varCEUM);
                                objdb.SaveChanges();
                            }


                        }


                    }



            if (model.RecurringID == daily)
            {

                var varCalenderEvents = objdb.CalenderEvents.CreateObject();
                varCalenderEvents.EventName = model.EventName;
                varCalenderEvents.EventDescription = model.EventDescription;
                varCalenderEvents.StartDate = model.StartDate;
                varCalenderEvents.Enddate = model.EndDate;
                varCalenderEvents.StartTime = model.StartTime;
                varCalenderEvents.EndTime = model.EndTime;
                varCalenderEvents.ColorCode = model.ColorCode;
                varCalenderEvents.CreatedBy = Convert.ToInt32(Session["UserID"]);
                varCalenderEvents.CreatedDate = DateTime.Now;
                varCalenderEvents.IsActive = true;
                varCalenderEvents.Entypo = "entypo-network";
                varCalenderEvents.RecurringTypeID = model.RecurringID;
                objdb.CalenderEvents.AddObject(varCalenderEvents);
                objdb.SaveChanges();



                List<int?> memberlist = new List<int?>();
                string[] members = model.Members.Split(',');
                for (int i = 0; i < members.Count(); i++)
                {
                    memberlist.Add(Convert.ToInt32(members[i]));
                }

                foreach (int mlist in memberlist)
                {
                    var varCEUM = objdb.CalendarEventUserMappings.CreateObject();
                    varCEUM.EventID = varCalenderEvents.EventID;
                    varCEUM.UserId = Convert.ToInt32(mlist);
                    objdb.CalendarEventUserMappings.AddObject(varCEUM);
                    objdb.SaveChanges();
                }
            }

                
            




            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var objcom =
                db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                    .Select(o => o.AppValue)
                    .FirstOrDefault();

            string Title = " " + objcom + "   calendar event  created";
            string Subject = " event was created on " + DateTime.Today.ToString("dd MMM yyyy");

            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Create Event").Select(o => o.EmailTemplateID).FirstOrDefault();
            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Create Event").Select(x => x.TemplatePath).FirstOrDefault();
            if ((check != null) && (check != 0))
            {

                var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Create Event")
                           join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                           select new DSRCManagementSystem.Models.Email
                           {
                               To = p.To,
                               CC = p.CC,
                               BCC = p.BCC,
                               Subject = p.Subject,
                               Template = q.TemplatePath
                           }).FirstOrDefault();

                string TemplatePath = Server.MapPath(obj.Template);
                string html = System.IO.File.ReadAllText(TemplatePath);


                Title = " " + objcom + "   calendar event Created";
                Subject = " event was created on " + DateTime.Today.ToString("dd MMM yyyy");
                obj.Subject = " " + objcom + " Management Portal-new  event  Created";



                html = html.Replace("#Title", Title);
                html = html.Replace("#Subject", Subject);
                html = html.Replace("#EventName", model.EventName);
                html = html.Replace("#EventDescription", model.EventDescription);
                html = html.Replace("#StartDate", model.StartDate.ToString());
                html = html.Replace("#EndDate", model.EndDate.ToString());
                html = html.Replace("#CompanyName", objcom.ToString());


                html = html.Replace("#ServerName", ServerName);


                obj.To = MyCalendarController.GetUserEmailAddress(db, obj.To);
                obj.CC = MyCalendarController.GetUserEmailAddress(db, obj.CC);
                if (obj.BCC != "")
                {
                    obj.BCC = MyCalendarController.GetUserEmailAddress(db, obj.BCC);
                }


              //  string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];

                if (ServerName != "http://win2012srv:88/")
                {
                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                    //MailIds.Add("boobalan.k@dsrc.co.in");
                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                    //MailIds.Add("ramesh.S@dsrc.co.in");
                    //MailIds.Add("aruna.m@dsrc.co.in");
                    //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                    //MailIds.Add("dineshkumar.d@dsrc.co.in");
                    //MailIds.Add("gopika.v@dsrc.co.in");

                    //foreach (var item in membersemail)
                    //{
                    //    MailIds.Add(item.ToString());
                    //}


                    string EmailAddress = "";

                    foreach (string maiil in MailIds)
                    {
                        EmailAddress += maiil + ",";
                    }

                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                    string CCMailId = "kirankumar@dsrc.co.in";
                    string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                    Task.Factory.StartNew(() =>
                    {
                        //var logo =
                        //    db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];
                        string pathvalue = CommonLogic.getLogoPath();
                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null,
                            html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId,
                            Server.MapPath(pathvalue.ToString()));
                    });

                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        //var logo =
                        //    db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];
                        string pathvalue = CommonLogic.getLogoPath();

                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", obj.To,
                            obj.CC, obj.BCC, Server.MapPath(pathvalue.ToString()));

                    });
                }
            }
            else
            {
                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                ExceptionHandlingController.TemplateMissing("Create Event", folder, ServerName);

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
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
        public JsonResult GetEvents(int ReportpersonId = 0)
        {



            int SessionUserId = Convert.ToInt32(Session["UserID"]);
            int userid = ReportpersonId == 0 ? SessionUserId : ReportpersonId;
            var varEventDetail = (from eu in objdb.CalendarEventUserMappings
                                  where eu.UserId == userid
                                  join e in objdb.CalenderEvents on eu.EventID equals e.EventID
                                  into events

                                  from eventmap in events.DefaultIfEmpty()
                                  select new
                                  {
                                      id = eventmap.EventID,
                                      className = eventmap.Entypo,
                                      title = eventmap.EventName,
                                      description = eventmap.EventDescription,
                                      start = eventmap.StartDate,
                                      end = eventmap.Enddate,
                                      color = eventmap.ColorCode,
                                      name = objdb.Users.Where(u => u.UserID == eventmap.CreatedBy).Select(u => u.FirstName + "\n" + u.LastName).FirstOrDefault(),
                                      allDay = false
                                  }).ToArray();
            return Json(varEventDetail, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ViewEvent(int eventid)
        {
            var varEditEvent = (from e in objdb.CalenderEvents
                                where e.EventID == eventid
                                join em in objdb.CalendarEventUserMappings on e.EventID equals em.EventID
                                select new CalendarEventModel
                                {
                                    EventId = eventid,
                                    EventName = e.EventName,
                                    EventDescription = e.EventDescription,
                                    StartDate = e.StartDate,
                                    EndDate = e.Enddate,
                                    ColorCode = e.ColorCode,
                                }).FirstOrDefault();

            var varUsers = (from u in objdb.Users.Where(u => u.IsActive == true)
                            select new
                            {
                                Userid = u.UserID,
                                Username = u.FirstName + " " + u.LastName
                            }).ToList();

            var getselectedusers = objdb.CalendarEventUserMappings.Where(c => c.EventID == eventid).Select(ce => ce.UserId).ToList();
            List<int> SelectedUserid = new List<int>();
            foreach (int users in getselectedusers)
            {
                SelectedUserid.Add(users);
            }
            ViewBag.Members = new MultiSelectList(varUsers, "Userid", "Username", SelectedUserid);
            return View(varEditEvent);
        }
        [HttpGet]
        public ActionResult EditEvent(int eventid)
        {
            var varEditEvent = (from e in objdb.CalenderEvents
                                where e.EventID == eventid
                                join em in objdb.CalendarEventUserMappings on e.EventID equals em.EventID
                                
                                select new CalendarEventModel
                                {
                                    EventId = eventid,
                                    EventName = e.EventName,
                                    EventDescription = e.EventDescription,
                                    StartDate = e.StartDate,
                                    EndDate = e.Enddate,
                                    StartTime = e.StartTime,
                                    EndTime = e.EndTime,
                                    ColorCode = e.ColorCode,
                                    RecurringID =(int)e.RecurringTypeID
                                    
                                }).FirstOrDefault();

            var varUsers = (from u in objdb.Users.Where(u => u.IsActive == true && u.UserStatus != 6)
                            select new
                            {
                                Userid = u.UserID,
                                Username = u.FirstName + " " + u.LastName
                            }).ToList();

            var getselectedusers = objdb.CalendarEventUserMappings.Where(c => c.EventID == eventid).Select(ce => ce.UserId).ToList();
            List<int> SelectedUserid = new List<int>();
            foreach (int users in getselectedusers)
            {
                SelectedUserid.Add(users);
            }
            ViewBag.Members = new MultiSelectList(varUsers, "Userid", "Username", SelectedUserid);

            var Recurring = (from p in objdb.Master_TaskRecurring
                             select new
                             {
                                 RecurringID = p.TaskRecurringID,
                                 RecurringType = p.RecurringType
                             }).ToList();
            ViewBag.Recurring = new SelectList(Recurring, "RecurringID", "RecurringType",varEditEvent.RecurringID);


            return View(varEditEvent);
        }

        [HttpPost]
        public ActionResult EditEvent(CalendarEventModel model)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var varUpdateEvents = objdb.CalenderEvents.Where(t => t.EventID == model.EventId).Select(e => e).FirstOrDefault();
            varUpdateEvents.EventName = model.EventName;
            varUpdateEvents.EventDescription = model.EventDescription;
            varUpdateEvents.StartDate = model.StartDate;
            varUpdateEvents.Enddate = model.EndDate;
            varUpdateEvents.StartTime = model.StartTime;
            varUpdateEvents.EndTime = model.EndTime;
            varUpdateEvents.ColorCode = model.ColorCode;
            objdb.SaveChanges();

            var dbEventUsers =
                objdb.CalendarEventUserMappings.Where(c => c.EventID == model.EventId).Select(ce => ce).ToList();
            List<int?> modelMemberList = new List<int?>();
            string[] members = model.Members.Split(',');
            for (int i = 0; i < members.Count(); i++)
            {
                modelMemberList.Add(Convert.ToInt32(members[i]));
            }
            var insertUser = modelMemberList.Where(u => dbEventUsers.All(r => r.UserId != u.Value));
            var deleteUser = dbEventUsers.Where(u => modelMemberList.All(r => r.Value != u.UserId));

            var useInsertUser = insertUser.Select(i => i.Value).ToList();
            var useDeleteUser = deleteUser.Select(d => d.UserId).ToList();

            foreach (var users in useInsertUser)
            {
                var varInsertUser = objdb.CalendarEventUserMappings.CreateObject();
                varInsertUser.EventID = model.EventId;
                varInsertUser.UserId = users;
                objdb.CalendarEventUserMappings.AddObject(varInsertUser);
                objdb.SaveChanges();
            }
            foreach (var users in useDeleteUser)
            {
                var varDeleteUser =
                    objdb.CalendarEventUserMappings.Where(c => c.UserId == users && c.EventID == model.EventId).FirstOrDefault();
                objdb.CalendarEventUserMappings.DeleteObject(varDeleteUser);
                objdb.SaveChanges();
            }





            List<string> membersmailids = new List<string>();
            foreach (int item in modelMemberList)
            {
                var username = objdb.Users.Where(x => x.UserID == item).Select(o => o.UserName).FirstOrDefault();
                membersmailids.Add(username);
            }

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

            string Title = " " + objcom + "   calendar event  updated";
            string Subject = " event was updated on " + DateTime.Today.ToString("dd MMM yyyy");

            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Edit Event").Select(o => o.EmailTemplateID).FirstOrDefault();
            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Edit Event").Select(x => x.TemplatePath).FirstOrDefault();
            if ((check != null) && (check != 0))
            {

                var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Edit Event")
                           join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                           select new DSRCManagementSystem.Models.Email
                           {
                               To = p.To,
                               CC = p.CC,
                               BCC = p.BCC,
                               Subject = p.Subject,
                               Template = q.TemplatePath
                           }).FirstOrDefault();


                string TemplatePath = Server.MapPath(obj.Template);
                string html = System.IO.File.ReadAllText(TemplatePath);


                Title = " " + objcom + "   calendar event updated";
                Subject = " event  has changed , please refer the  below details";
                obj.Subject = " " + objcom + " Management Portal  event  Updated";
                html = html.Replace("#Title", Title);
                html = html.Replace("#Subject", Subject);
                html = html.Replace("#EventName", model.EventName);
                html = html.Replace("#EventDescription", model.EventDescription);
                html = html.Replace("#StartDate", model.StartDate.ToString());
                html = html.Replace("#EndDate", model.EndDate.ToString());
                html = html.Replace("#CompanyName", objcom.ToString());


                html = html.Replace("#ServerName", ServerName);


                obj.To = MyCalendarController.GetUserEmailAddress(db, obj.To);
                obj.CC = MyCalendarController.GetUserEmailAddress(db, obj.CC);

                if (obj.BCC != "")
                {
                    obj.BCC = MyCalendarController.GetUserEmailAddress(db, obj.BCC);
                }
                //string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];

                if (ServerName != "http://win2012srv:88/")
                {
                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                    //MailIds.Add("boobalan.k@dsrc.co.in");
                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                    //MailIds.Add("ramesh.S@dsrc.co.in");
                    //MailIds.Add("aruna.m@dsrc.co.in");
                    //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                    //MailIds.Add("dineshkumar.d@dsrc.co.in");
                    //MailIds.Add("gopika.v@dsrc.co.in");
                    //foreach (var item in  membersmailids)
                    //{
                    //    MailIds.Add(item.ToString());
                    //}
                    string EmailAddress = "";

                    foreach (string maiil in MailIds)
                    {
                        EmailAddress += maiil + ",";
                    }

                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                    string CCMailId = "kirankumar@dsrc.co.in";
                    string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                    Task.Factory.StartNew(() =>
                    {
                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];
                        string pathvalue = CommonLogic.getLogoPath();
                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue.ToString()));
                    });

                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];

                        string pathvalue = CommonLogic.getLogoPath();

                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", obj.To, obj.CC, obj.BCC, Server.MapPath(pathvalue.ToString()));

                    });

                }
            }

            else
            {
               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                ExceptionHandlingController.TemplateMissing("Edit Event", folder, ServerName);

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteEvent(int EventId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var varDeleteEvent = objdb.CalendarEventUserMappings.Where(ce => ce.EventID == EventId).Select(c => c.EventID).ToList();
            foreach (var events in varDeleteEvent)
            {
                var varDeleteAllEvent = objdb.CalendarEventUserMappings.Where(c => c.EventID == events).FirstOrDefault();
                objdb.CalendarEventUserMappings.DeleteObject(varDeleteAllEvent);
                objdb.SaveChanges();
            }


            var varCalenderEvents = objdb.CalenderEvents.Where(e => e.EventID == EventId).Select(e => e).FirstOrDefault();
            varCalenderEvents.IsActive = false;
            objdb.SaveChanges();


            var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

            string Title = " " + objcom + "calendar event  deleted";
            string Subject = " event  was  canceled ";
            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Event").Select(o => o.EmailTemplateID).FirstOrDefault();
            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Delete Event").Select(x => x.TemplatePath).FirstOrDefault();
            if ((check != null) && (check != 0))
            {
                var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Event")
                           join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                           select new DSRCManagementSystem.Models.Email
                           {
                               To = p.To,
                               CC = p.CC,
                               BCC = p.BCC,
                               Subject = p.Subject,
                               Template = q.TemplatePath
                           }).FirstOrDefault();


                string TemplatePath = Server.MapPath(obj.Template);
                string html = System.IO.File.ReadAllText(TemplatePath);


                Title = " " + objcom + "   calendar event  deleted";
                Subject = " event  has  been canceled,  please refer the below details";
                obj.Subject = " " + objcom + " Management Portal  event  Deleted";
                html = html.Replace("#Title", Title);
                html = html.Replace("#Subject", Subject);
                html = html.Replace("#EventName", varCalenderEvents.EventName);
                html = html.Replace("#EventDescription", varCalenderEvents.EventDescription);
                html = html.Replace("#StartDate", varCalenderEvents.StartDate.ToString());
                html = html.Replace("#EndDate", varCalenderEvents.Enddate.ToString());
                html = html.Replace("#CompanyName", objcom.ToString());


                html = html.Replace("#ServerName", ServerName);


                obj.To = MyCalendarController.GetUserEmailAddress(db, obj.To);
                obj.CC = MyCalendarController.GetUserEmailAddress(db, obj.CC);

                if (obj.BCC != "")
                {
                    obj.BCC = MyCalendarController.GetUserEmailAddress(db, obj.BCC);
                }
              //  string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];

                if (ServerName == "http://win2012srv:88/")
                {
                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                    //MailIds.Add("boobalan.k@dsrc.co.in");
                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                    //MailIds.Add("ramesh.S@dsrc.co.in");
                    //MailIds.Add("aruna.m@dsrc.co.in");
                    //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                    //MailIds.Add("dineshkumar.d@dsrc.co.in");
                    //MailIds.Add("gopika.v@dsrc.co.in");
                    //foreach (var item in  membersmailids)
                    //{
                    //    MailIds.Add(item.ToString());
                    //}
                    string EmailAddress = "";

                    foreach (string maiil in MailIds)
                    {
                        EmailAddress += maiil + ",";
                    }

                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                    string CCMailId = "kirankumar@dsrc.co.in";
                    string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                    Task.Factory.StartNew(() =>
                    {
                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];

                        string pathvalue = CommonLogic.getLogoPath();

                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue.ToString()));
                    });

                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        //string[] words;

                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                        //string pathvalue = "~/" + words[1];

                        string pathvalue = CommonLogic.getLogoPath();


                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", obj.To, obj.CC, obj.BCC, Server.MapPath(pathvalue.ToString()));

                    });
                }
            }
            else
            {
                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                ExceptionHandlingController.TemplateMissing("Delete Event", folder, ServerName);

            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

    }

}
