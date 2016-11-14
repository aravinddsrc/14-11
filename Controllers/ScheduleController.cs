using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ScheduleController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        public ActionResult MS()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            List<DSRCManagementSystem.Models.ProjectMom> objmodel = new List<Models.ProjectMom>();

            try
            {
                objmodel = (from c in db.MeetingGuids
                            join u in db.Users on c.UserId equals u.UserID
                            select new DSRCManagementSystem.Models.ProjectMom()
                            {
                                UserId = c.UserId,
                                Name = u.FirstName + " " + (u.LastName ?? " ")
                            }).ToList();
                var attendee = db.MettingSchedules.Select(x => x.Attendees).ToList();
                List<int> ob = new List<int>();

                foreach (var x in attendee)
                {
                    if (x != null)
                    {
                        List<string> result = x.Split(new char[] { ',' }).ToList();
                        foreach (var y in result)
                        {
                            ob.Add(Convert.ToInt32(y));

                        }
                    }
                }
                ViewBag.aaa = ob;
                ViewData["list"] = ob;

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objmodel);
        }
        //List<ProjectMom> objmodel = new List<ProjectMom>();

        //List<int> attid = new List<int>();
        //List<object> New = new List<object>();
        //var att = db.MettingSchedules.Select(x => x.Attendees).ToList();
        //var a = (from m in db.MettingSchedules
        //         select new ProjectMom
        //         {
        //             UserIds = m.Attendees,
        //             ProjectId = m.ProjectID
        //         }).ToList();



        //foreach (var x in a)
        //{
        //    List<string> result = x.UserIds.Split(new char[] { ',' }).ToList();
        //    foreach (var y in result)
        //    {
        //        attid.Add(Convert.ToInt32(y));
        //        //var Val = new { UID = Convert.ToInt32(y), PID = x.ProjectId };
        //        //New.Add(Val);
        //        DSRCManagementSystem.Models.ProjectMom ob = new DSRCManagementSystem.Models.ProjectMom();
        //        int nameid = Convert.ToInt32(y);
        //        var getName = db.Users.Where(v => v.UserID == nameid).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
        //        ob.Name = getName;

        //        var getProjectName = db.Projects.Where(b => b.ProjectID == x.ProjectId).Select(o => o.ProjectName).FirstOrDefault();
        //        ob.ProjectName = getProjectName;

        //        ob.UserId = Convert.ToInt32(y);

        //        objmodel.Add(ob);




        //    }
        //}
        //IEnumerable<ProjectMom> obj = new List<ProjectMom>();
        ////obj=(from p in db.MettingSchedules
        ////    join q in db.Users.Where(x => x.UserID==) on p.AssignedTo equals q.UserID
        ////    join tr in db.Master_TaskRecurring on p.Recurring equals tr.TaskRecurringID
        ////    select new ProjectMom 
        ////    {
        ////              }).ToList();
        //return View(objmodel);


        [HttpPost]
        public ActionResult Delete(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int id = Convert.ToInt32(Id);
            var data = db.MeetingGuids.Where(o => o.UserId == id).Select(x => x).ToList();
            foreach (var y in data)
            {
                db.DeleteObject(y);
                db.SaveChanges();
            }
            DeleteAttendee(Id);
            //DeleteFuture(Id);

            return Json("success", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteAttendee(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var search = db.MettingSchedules.Select(o => o.Id).ToList();
            foreach (var y in search)
            {
                var Filter = db.MettingSchedules.Where(p => p.Id == y).Select(o => o.Attendees).ToList();
                foreach (var t in Filter)
                {
                    if (t != null)
                    {
                        List<string> result = t.Split(new char[] { ',' }).ToList();
                        foreach (var x in result)
                        {
                            int con = Convert.ToInt32(x);
                            if (con == Id)
                            {
                                List<string> result2 = new List<string>();
                                result2.Add(x);
                                var RE = result.Except(result2);
                                var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == y);
                                ReqToEdit.Attendees = null;
                                db.SaveChanges();
                                if (result.Count > 1)
                                {
                                    string combindedString = string.Join(",", RE.ToArray());
                                    var ReqToUpdate = db.MettingSchedules.FirstOrDefault(o => o.Id == y);
                                    ReqToUpdate.Attendees = combindedString;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }


        [HttpPost]
        public ActionResult DeleteFuture(int Id)
        {
            


            var date = DateTime.Now;

            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            var result = (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;


            var firstdateofweek = DateTime.Now;
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            Dictionary<string, DateTime> currentWeek = new Dictionary<string, DateTime>();
            Dictionary<string, DateTime> nextWeek = new Dictionary<string, DateTime>();

            var weekofYear = cal.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);
            firstdateofweek = MeetingScheduleController.FirstDateOfWeek(DateTime.Now.Year, weekofYear, CultureInfo.CurrentCulture);
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
                var objmail = (from metting_schedule in db.MettingSchedules
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


                    if (meetingSchedule.Attendees != null)
                    {
                        var CDATE = DateTime.Now;
                        DateTime MDATE = new DateTime();

                        if (result % 2 == meetingSchedule.Week / 2)
                            MDATE = Convert.ToDateTime(nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy"));
                        else
                            MDATE = Convert.ToDateTime(currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy"));



                        List<string> results = meetingSchedule.Attendees.Split(new char[] { ',' }).ToList();
                        foreach (var x in results)
                        {
                            int con = Convert.ToInt32(x);
                            if ((con == Id) && (MDATE > CDATE.Date))
                            {
                                List<string> result2 = new List<string>();
                                result2.Add(x);
                                var RE = results.Except(result2);
                                var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == meetingSchedule.Id);
                                ReqToEdit.Attendees = null;
                                db.SaveChanges();
                                if (results.Count > 1)
                                {
                                    string combindedString = string.Join(",", RE.ToArray());
                                    var ReqToUpdate = db.MettingSchedules.FirstOrDefault(o => o.Id == meetingSchedule.Id);
                                    ReqToUpdate.Attendees = combindedString;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    ////////////////


                }
            }
            else
            {
                var objmail = (from metting_schedule in db.MettingSchedules
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


                    if (meetingSchedule.Attendees != null)
                    {
                        var CDATE = DateTime.Now;
                        DateTime MDATE = new DateTime();

                        if (result % 2 == meetingSchedule.Week / 2)
                            MDATE = Convert.ToDateTime(nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy"));
                        else
                            MDATE = Convert.ToDateTime(currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy"));



                        List<string> results = meetingSchedule.Attendees.Split(new char[] { ',' }).ToList();
                        foreach (var x in results)
                        {
                            int con = Convert.ToInt32(x);
                            if ((con == Id) && (MDATE > CDATE.Date))
                            {
                                List<string> result2 = new List<string>();
                                result2.Add(x);
                                var RE = results.Except(result2);
                                var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == meetingSchedule.Id);
                                ReqToEdit.Attendees = null;
                                db.SaveChanges();
                                if (results.Count > 1)
                                {
                                    string combindedString = string.Join(",", RE.ToArray());
                                    var ReqToUpdate = db.MettingSchedules.FirstOrDefault(o => o.Id == meetingSchedule.Id);
                                    ReqToUpdate.Attendees = combindedString;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    ////////////////


                }
                
            }
            return null;
        }

        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
            var tmp = "";
            if (Attendee != null)
            {
                List<int> lst = new List<int>();
                foreach (var str in Attendee.Split(','))
                {
                    lst.Add(Convert.ToInt32(str));
                }
                var obj = (from user in db.Users.Where(user => user.IsActive == true && lst.Contains(user.UserID)) select user.FirstName + " " + (user.LastName ?? "")).ToList();

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
            return tmp;

        }


        public ActionResult Data(int Id)
        {
            List<object> List = new List<object>();
            try
            {

                //var Filter = db.MettingSchedules.Select(o => o.Attendees).ToList();


                var Filter = (from ms in db.MettingSchedules
                              select new ProjectMom()
                              {
                                  ProjectId = ms.ProjectID,
                                  UserIds = ms.Attendees
                              }).ToList();


                foreach (var t in Filter)
                {
                    if (t.UserIds != null)
                    {
                        List<string> result = t.UserIds.Split(new char[] { ',' }).ToList();
                        foreach (var x in result)
                        {

                            string s = Convert.ToString(Id);
                            if (x == s)
                            {
                                var PID = t.ProjectId;

                                var PNAME = db.Projects.Where(u => u.ProjectID == PID).Select(o => o.ProjectName).FirstOrDefault();
                                List.Add(PNAME);

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
            return Json(List, JsonRequestBehavior.AllowGet);
        }



    }
}
