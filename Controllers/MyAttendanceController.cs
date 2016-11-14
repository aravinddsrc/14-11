using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class MyAttendanceController : Controller
    {
        //
        // GET: /MyAttendance/

        public List<Tuple<DateTime, string, string>> currentdates = new List<Tuple<DateTime, string, string>>();
        [HttpGet]
        public ActionResult LeaveCalender(DateTime Month, int userId)
        {

            if (userId == 0)
                userId = (int)Session["UserId"];


            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {


                int leaveStatus = MasterEnum.LeaveStatus.Approved.GetHashCode();

                var recordsIQueryable = db.LeaveRequests.Where(x => x.UserId == userId && x.LeaveStatusId == 2).ToList();


                var records = recordsIQueryable.ToList();
                var EmpID = db.Users.Where(x => x.IsActive == true && x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();

                var recordsPresent = db.TimeManagements.Where(x => x.EmpID == EmpID).ToList();


                var DOJ = db.Users.Where(x => x.IsActive == true && x.UserID == userId).Select(o => o.DateOfJoin).FirstOrDefault();


                TimeSpan difference;
                DateTime todaydate;
                if (DateTime.Now.Date != Convert.ToDateTime(Month))
                {
                    difference = DateTime.Now.Date.AddDays(1) - Convert.ToDateTime(Month);
                    todaydate = Month;
                }
                else
                {
                    todaydate = Convert.ToDateTime((Month.Day + 1) - Month.Day + "-" + Month.Month + "-" + Month.Year);
                    difference = DateTime.Now.Date.AddDays(1) - Convert.ToDateTime((Month.Day + 1) - Month.Day + "-" + Month.Month + "-" + Month.Year);

                }
                var days = difference.Days;



                int day = DateTime.DaysInMonth(Month.Year, Month.Month); // new one

                for (int i = 1; i <= day; i++)    //days replaced by day
                {
                    string flagColor1 = "";
                    string flagColor2 = "";
                    string flagColor3 = "";

                    string s = "";

                    if (DateTime.Now.Date < Convert.ToDateTime(Month) || DateTime.Now.Date < Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i))
                    {

                        DateTime objdate;
                        if (DateTime.Now.Date < Convert.ToDateTime(Month))
                        {
                            objdate = Convert.ToDateTime(Month.AddDays(-1).AddDays(i));

                        }
                        else
                        {
                            objdate = Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i);
                        }

                        var Holiday = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                        string HolidayName = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.HolidayName).FirstOrDefault();
                        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) 
                            && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                      
                        if (leaveRequestApproved != null) {
                        // To get LeaveType Name
                        var a = from lb in db.LeaveRequests join lt in db.LeaveTypes on lb.LeaveTypeId equals lt.LeaveTypeId 
                                where lb.UserId == userId && EntityFunctions.TruncateTime(lb.StartDateTime) <= EntityFunctions.TruncateTime(objdate) 
                                && EntityFunctions.TruncateTime(lb.EndDateTime) >= EntityFunctions.TruncateTime(objdate) select lt.Name;
                       s = a.FirstOrDefault().ToString();
                    }
                        
                        if (Holiday.Count > 0)
                        {
                            flagColor2 = "colorClass9";
                            currentdates.Add(Tuple.Create(objdate, flagColor2, HolidayName));
                        }

                        if (leaveRequestApproved != null)
                        {

                            flagColor1 = "colorClass7";
                            
                            currentdates.Add(Tuple.Create(objdate, flagColor1, leaveRequestApproved + " - " + s + " Leave "));
                        }
                                     

                    }
                    else
                    {

                        DateTime objdate = Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i);
                        string HolidayName = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.HolidayName).FirstOrDefault();

                        var timemang = db.TimeManagements.Where(x => x.EmpID == EmpID && x.Date == objdate).Select(o => o.Date).ToList();
                        var leave = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                       
                        if (leaveRequestApproved != null){
                        //To get LeaveTypeName
                        var a = from lb in db.LeaveRequests
                                join lt in db.LeaveTypes on lb.LeaveTypeId equals lt.LeaveTypeId
                                where lb.UserId == userId && EntityFunctions.TruncateTime(lb.StartDateTime) <= EntityFunctions.TruncateTime(objdate)
                                && EntityFunctions.TruncateTime(lb.EndDateTime) >= EntityFunctions.TruncateTime(objdate)
                                select lt.Name;
                            s = a.FirstOrDefault().ToString();
                        }
                        string weekends = "Weekend";
                        //if (leaveRequestApproved.Count >0)
                        if(leaveRequestApproved != null)
                        {
                            flagColor3 = "colorClass7";
                        }
                        else if (timemang.Count > 0)
                        {
                            flagColor1 = "colorClass8";
                        }
                        else if (leave.Count > 0)
                        {
                            flagColor2 = "colorClass9";
                        }
                        else
                        {
                            flagColor3 = "colorClass7";
                        }

                        string weekend = objdate.Date.DayOfWeek.ToString();


                        if (weekend != "Saturday" && weekend != "Sunday")
                        {


                            if (flagColor1 != "")
                            {

                                currentdates.Add(Tuple.Create(objdate, flagColor1, HolidayName));
                                flagColor1 = "";

                            }
                            else if (flagColor2 != "")
                            {
                                currentdates.Add(Tuple.Create(objdate, flagColor2, HolidayName));
                                flagColor2 = "";
                            }
                            else if (flagColor3 != "" && leaveRequestApproved != null)
                            {
                                currentdates.Add(Tuple.Create(objdate, flagColor3, leaveRequestApproved + " - " + s + " Leave "));
                               

                                flagColor3 = "";
                            }
                            else if (flagColor3 != "" && leaveRequestApproved == null)
                            {
                                currentdates.Add(Tuple.Create(objdate, flagColor3, HolidayName));
                                flagColor3 = "";
                            }
                        }
                        else
                        {
                            flagColor2 = "colorClass9";
                            currentdates.Add(Tuple.Create(objdate, flagColor2, weekends));
                            flagColor2 = "";
                        }

                    }

                }


                if (DateTime.Now.Date < Convert.ToDateTime(Month))
                {
                    var data = (from t in currentdates
                                select new
                                {
                                    start = t.Item1.AddDays(1),
                                    end = t.Item1.AddDays(1),
                                    className = t.Item2,
                                     sample = t.Item3
                                     
                                }).ToArray();
                    return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
                }
                else
                {

                    List<DateTime?> holidaysdates = new List<DateTime?>();

                    holidaysdates = db.AddHolidays.Select(o => o.Date).ToList();

                    List<DateTime?> Remainingdate = new List<DateTime?>();



                    List<DateTime> tmgremaining = new List<DateTime>();

                    tmgremaining = db.TimeManagements.Where(x => x.EmpID == EmpID).Select(o => o.Date).ToList();

                    List<DateTime?> tmgdates = new List<DateTime?>();

                    List<DateTime?> Remaining = new List<DateTime?>();
                    foreach (var date in tmgremaining)
                    {
                        Remaining.Add(date);
                    }

                    var data = (from t in currentdates
                                select new
                                {
                                    start = t.Item1.AddDays(1),
                                    end = t.Item1.AddDays(1),
                                    className = t.Item2,
                                    sample = t.Item3

                                }).ToArray();
                    return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
                  

                }

                //string flagColor1 = "";
                //string flagColor2 = "";
                //string flagColor3 = "";
                //if (DateTime.Now.Date < Convert.ToDateTime(Month))
                //{
                //    int day = DateTime.DaysInMonth(Month.Year, Month.Month);
                //    for (int i = 1; i < day; i++)
                //    {
                //        DateTime objdate = Convert.ToDateTime(Month.AddDays(i));
                //        var Holiday = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                //        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                //        if (Holiday.Count > 0)
                //        {
                //            flagColor2 = "colorClass9";
                //            currentdates.Add(Tuple.Create(objdate, flagColor2));
                //        }
                //        if (leaveRequestApproved != null) 
                //        {
                //            flagColor1 = "colorClass7";
                //            currentdates.Add(Tuple.Create(objdate, flagColor1));
                //        }

                //    }

                //    var HolidayData = (from t in currentdates
                //                       select new
                //                       {
                //                           start = t.Item1.AddDays(1),
                //                           end = t.Item1.AddDays(1),
                //                           className = t.Item2

                //                       }).ToArray();

                //    return Json(HolidayData.ToArray(), JsonRequestBehavior.AllowGet);
                //}
                //else
                //{


                //    for (int i = 1; i <= days; i++)
                //    {
                //        DateTime objdate = Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i);

                //        var timemang = db.TimeManagements.Where(x => x.EmpID == EmpID && x.Date == objdate).Select(o => o.Date).ToList();
                //        var leave = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                //        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(objdate)).Select(o => o).ToList();


                //        if (leaveRequestApproved.Count > 0)
                //        {
                //            flagColor3 = "colorClass7";
                //        }
                //        else if (timemang.Count > 0)
                //        {
                //            flagColor1 = "colorClass8";
                //        }
                //        else if (leave.Count > 0)
                //        {
                //            flagColor2 = "colorClass9";
                //        }
                //        else
                //        {
                //            flagColor3 = "colorClass7";
                //        }

                //        string weekend = objdate.Date.DayOfWeek.ToString();


                //        if (weekend != "Saturday" && weekend != "Sunday")
                //        {


                //            if (flagColor1 != "")
                //            {

                //                currentdates.Add(Tuple.Create(objdate, flagColor1));
                //                flagColor1 = "";

                //            }
                //            else if (flagColor2 != "")
                //            {
                //                currentdates.Add(Tuple.Create(objdate, flagColor2));
                //                flagColor2 = "";
                //            }
                //            else if (flagColor3 != "")
                //            {
                //                currentdates.Add(Tuple.Create(objdate, flagColor3));
                //                flagColor3 = "";
                //            }
                //        }
                //        else
                //        {
                //            flagColor2 = "colorClass9";
                //            currentdates.Add(Tuple.Create(objdate, flagColor2));
                //            flagColor2 = "";
                //        }

                //    }


                //    List<DateTime?> holidaysdates = new List<DateTime?>();

                //    holidaysdates = db.AddHolidays.Select(o => o.Date).ToList();

                //    List<DateTime?> Remainingdate = new List<DateTime?>();



                //    List<DateTime> tmgremaining = new List<DateTime>();

                //    tmgremaining = db.TimeManagements.Where(x => x.EmpID == EmpID).Select(o => o.Date).ToList();

                //    List<DateTime?> tmgdates = new List<DateTime?>();

                //    List<DateTime?> Remaining = new List<DateTime?>();
                //    foreach (var date in tmgremaining)
                //    {
                //        Remaining.Add(date);
                //    }


                //    var data = (from t in currentdates
                //                select new
                //                {
                //                    start = t.Item1.AddDays(1),
                //                    end = t.Item1.AddDays(1),
                //                    className = t.Item2

                //                }).ToArray();
                //    return Json(data.ToArray(), JsonRequestBehavior.AllowGet);

                //}
            }
        }



        public ActionResult AttendancesCalendar()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var userid = Convert.ToInt32(Session["UserID"].ToString());
            var reportingUsers = (from p in objdb.UserReportings.Where(x => x.ReportingUserID == userid)
                                  join q in objdb.Users.Where(x => x.IsActive == true && x.UserStatus != 6) on p.UserID equals q.UserID
                                  select new
                                  {
                                      UserID = q.UserID,
                                      UserName = q.FirstName + " " + q.LastName
                                  }).ToList();

            ViewBag.LeaveEmpCalender = new SelectList(reportingUsers, "UserID", "UserName");
            return View();

        }
     

    }
}
