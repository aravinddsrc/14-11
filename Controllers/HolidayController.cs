using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using DSRCManagementSystem.DSRCLogic;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.Models.Domain_Models;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Globalization;


namespace DSRCManagementSystem.Controllers
{
    [DSRCAuthorize]
    public class HolidayController : Controller
    {
        //
        // GET: /Holiday/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddHolidays(string Year, string Financial, string Id, string Date, string HolidayCountry, string DateId)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.Format> obj = new List<DSRCManagementSystem.Models.Format>();
            try
            {


                int Zone = Convert.ToInt32(Year);
                int holidayid = 0;

                if (HolidayCountry != null)
                {
                    holidayid = Convert.ToInt32(HolidayCountry);
                }

                else if (Financial != null && Financial != "")
                {
                    holidayid = Convert.ToInt32(Financial);
                }
                var financialstartdate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial Start Date").Select(o => o.AppValue).FirstOrDefault();
                var financialenddate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial End Date").Select(o => o.AppValue).FirstOrDefault();
                var calendaryear = objdb.CalendarYears.Select(o => o).FirstOrDefault();
                var startingmonth = calendaryear.StartingMonth;
                var endingmonth = calendaryear.EndingMonth;
                List<string> dropdownvalues = new List<string>();
                List<SelectListItem> objyear = new List<SelectListItem>();
                DSRCManagementSystem.Models.Format objmodel = new DSRCManagementSystem.Models.Format();
                int UserDate = 0;
                int objholiday = 0;
                int HolidayYear = Convert.ToInt32(Date);
                if (Financial != null && Financial != "")
                {
                    objholiday = Convert.ToInt32(Financial);
                }
                if (HolidayYear != null && HolidayYear != 0)
                {
                    UserDate = (HolidayYear == null) ? 0 : HolidayYear;
                }
                if (objholiday != null && objholiday != 0)
                {
                    UserDate = (objholiday == null) ? 0 : objholiday;
                }
                int holidaycountry = Convert.ToInt32(HolidayCountry);
                for (int i = 0; i < 2; i++)
                {
                    string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(startingmonth));
                    string strEndingMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(endingmonth));
                    var currentyear = 0;
                    var previousyear = 0;
                    var year1 = 0;
                    if (i == 0)
                    {
                        currentyear = DateTime.Now.Year;
                        previousyear = Convert.ToInt32(currentyear) - 1;
                        year1 = Convert.ToInt32(previousyear) + 1;
                    }
                    else
                    {
                        currentyear = DateTime.Now.Year;
                        previousyear = Convert.ToInt32(currentyear);

                        if (i == 1)
                        {
                            year1 = Convert.ToInt32(previousyear) + i;
                        }

                        else
                        {
                            year1 = Convert.ToInt32(previousyear) + (i - 1);

                        }

                    }
                    string substring1 = strMonthName.Remove(strMonthName.Length - 2);
                    string substring2 = strEndingMonthName.Remove(strEndingMonthName.Length - 2);
                    string startingstring = substring1 + ", " + previousyear + " " + " - " + substring2 + "," + year1;
                    string valuei = Convert.ToString(i);
                    objyear.Add(new SelectListItem { Text = startingstring, Value = valuei });
                }
                var financialpurpose1 = (from p in objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial Start Date")
                                         select new
                                         {
                                             startdate = p.AppValue
                                         }).ToList();
                var financialpurpose2 = (from p in objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial End Date")
                                         select new
                                         {
                                             enddate = p.AppValue
                                         }).ToList();
                ViewBag.financialstartdate = new SelectList(financialpurpose1, "startdate ");
                ViewBag.financialenddate = new SelectList(financialpurpose2, "enddate");
                DateTime startdate = Convert.ToDateTime(financialstartdate);
                DateTime enddate = Convert.ToDateTime(financialenddate);
                string FinancialStartDate = startdate.ToString("dd MMM yyyy");
                string FinancialEndDate = enddate.ToString("dd MMM yyyy");
                if (Date != null && HolidayCountry != null)
                {
                    if (Zone == 5)
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.Date.Value.Year == UserDate)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                     //   obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());

                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }



                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                    else
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.Date.Value.Year == UserDate && x.ZoneId == holidaycountry)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                     //   obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());


                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }


                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                }
                else if (Year != null && Financial != null && Financial != "")
                {
                    if (Zone == 5)
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.Date.Value.Year == UserDate)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                     //   obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());


                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }


                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                    else
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == Zone && x.Date.Value.Year == UserDate)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();


                       // obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());



                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }

                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                }
                else if (Year != null && (Financial == null || Financial == ""))
                {
                    if (Zone == 5)
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();

                       // obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());

                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }

                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                    else
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == Zone)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                    //    obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());

                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }


                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }

                }
                else if (Date != null && HolidayCountry == null)
                {
                    if (Zone == 5)
                    {
                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.Date.Value.Year == UserDate)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                       // obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());


                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }

                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }

                    }
                    else
                    {

                        obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.Date.Value.Year == UserDate)
                               join t in objdb.TimeZones on p.ZoneId equals t.Id
                               select new DSRCManagementSystem.Models.Format
                               {
                                   Id = p.HolidayId,
                                   HolidayName = p.HolidayName,
                                   ZoneName = t.Zone,
                                   Date = p.Date,
                                   EnteredBy = p.EnteredBy,
                               }).OrderBy(x => x.Date).ToList();
                     //   obj.ForEach(x => x.Day =x.Date.Value.Date.DayOfWeek.ToString());


                        foreach (var objvalue in obj)
                        {
                            if (objvalue.Date != null)
                            {
                                objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                            }
                            else
                            {
                                objvalue.Day = null;
                            }
                        }


                        foreach (var meetingSchedule in obj)
                        {
                            meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                        }
                    }
                }
                else
                {
                    if (Year == null || Year == "")
                    {
                        if (Zone == 5)
                        {
                            obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true)
                                   join t in objdb.TimeZones on p.ZoneId equals t.Id
                                   select new DSRCManagementSystem.Models.Format
                                   {
                                       Id = p.HolidayId,
                                       HolidayName = p.HolidayName,
                                       ZoneName = t.Zone,
                                       Date = p.Date,
                                       EnteredBy = p.EnteredBy,
                                   }).OrderBy(x => x.Date).ToList();
                           // obj.ForEach(x => x.Day =  x.Date.Value.Date.DayOfWeek.ToString());



                            foreach (var objvalue in obj)
                            {
                                if (objvalue.Date != null)
                                {
                                    objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                                }
                                else
                                {
                                    objvalue.Day = null;
                                }
                            }

                            foreach (var meetingSchedule in obj)
                            {
                                meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                            }
                        }
                        else
                        {
                            obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == 3)
                                   join t in objdb.TimeZones on p.ZoneId equals t.Id
                                   select new DSRCManagementSystem.Models.Format
                                   {
                                       Id = p.HolidayId,
                                       HolidayName = p.HolidayName,
                                       ZoneName = t.Zone,
                                       Date = p.Date,
                                       EnteredBy = p.EnteredBy,
                                   }).OrderBy(x => x.Date).ToList();
                           // obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());
                            foreach (var objvalue in obj)
                            {
                                if (objvalue.Date != null)
                                {
                                    objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                                }
                                else
                                {
                                    objvalue.Day = null;
                                }
                            }


                            foreach (var meetingSchedule in obj)
                            {
                                meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                            }
                        }
                    }
                    else
                    {
                        if (Zone == 5)
                        {
                            obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true)
                                   join t in objdb.TimeZones on p.ZoneId equals t.Id
                                   select new DSRCManagementSystem.Models.Format
                                   {
                                       Id = p.HolidayId,
                                       HolidayName = p.HolidayName,
                                       ZoneName = t.Zone,
                                       Date = p.Date,
                                       EnteredBy = p.EnteredBy,
                                   }).OrderBy(x => x.Date).ToList();
                           // obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());
                            foreach (var objvalue in obj)
                            {
                                if (objvalue.Date != null)
                                {
                                    objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                                }
                                else
                                {
                                    objvalue.Day = null;
                                }
                            }

                            foreach (var meetingSchedule in obj)
                            {
                                meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                            }
                        }

                        else
                        {
                            obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == Zone)
                                   join t in objdb.TimeZones on p.ZoneId equals t.Id
                                   select new DSRCManagementSystem.Models.Format
                                   {
                                       Id = p.HolidayId,
                                       HolidayName = p.HolidayName,
                                       ZoneName = t.Zone,
                                       Date = p.Date,
                                       EnteredBy = p.EnteredBy,
                                   }).OrderBy(x => x.Date).ToList();


                          //  obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());

                            foreach (var objvalue in obj)
                            {
                                if (objvalue.Date != null)
                                {
                                    objvalue.Day = objvalue.Date.Value.DayOfWeek.ToString();
                                }
                                else
                                {
                                    objvalue.Day = null;
                                }
                            }
                            foreach (var meetingSchedule in obj)
                            {
                                meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                                meetingSchedule.Day = meetingSchedule.Date.Value.DayOfWeek.ToString();
                            }

                        }
                    }

                }
                var Purpose = (from pi in objdb.TimeZones
                               select new
                               {
                                   Id3 = pi.Id,
                                   Template = pi.Zone
                               }).ToList();

                if (Zone != null || holidaycountry != null)
                {
                    if (Zone != null && Zone != 0)
                    {
                        ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", Zone);
                    }
                    else if (holidaycountry != null && holidaycountry != 0)
                    {
                        // int holidayid = Convert.ToInt32(holidaycountry);
                        ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", holidayid);
                    }
                    else
                    {
                        ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", 3);
                    }
                }
                else
                {
                    ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template");

                }
                if (DateId != "" && DateId != null)
                {
                    ViewBag.financialyears = new SelectList(objyear, "Value", "Text", DateId);
                }
                else
                {
                    ViewBag.financialyears = new SelectList(objyear, "Value", "Text");
                }

                if (Id != null && Id != "")
                {
                    int financialid = Convert.ToInt32(Id);
                    ViewBag.financialyears = new SelectList(objyear, "Value", "Text", financialid);
                }
                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
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
        [HttpGet]
        public ActionResult AddNewDays()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.ExtraHolidays obj = new DSRCManagementSystem.Models.ExtraHolidays();
            try
            {
                var UserId = int.Parse(Session["UserID"].ToString());
                obj.EnteredBy = UserId.ToString();

                var EmailList = (from p in objdb.Users.Where(x => x.IsActive == true)
                                 select new
                                 {
                                     Id = p.UserID,
                                     UserName = p.FirstName + " " + (p.LastName ?? "")
                                 }).ToList();

                var EmailList1 = (from p in objdb.Users.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      Id1 = p.UserID,
                                      UserName = p.FirstName + "" + (p.LastName ?? "")
                                  }).ToList();

                var EmailList2 = (from p in objdb.Users.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      Id2 = p.UserID,
                                      UserName = p.FirstName + "" + (p.LastName ?? "")
                                  }).ToList();

                var Purpose = (from pi in objdb.TimeZones
                               select new
                               {
                                   Id3 = pi.Id,
                                   Template = pi.Zone
                               }).ToList();

                ViewBag.Email = new SelectList(EmailList, "Id", "UserName");
                ViewBag.Email3 = new MultiSelectList(EmailList, "Id", "UserName");
                ViewBag.Email1 = new MultiSelectList(EmailList1, "Id1", "UserName");
                ViewBag.Email2 = new MultiSelectList(EmailList1, "Id", "UserName");
                ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template");

                obj.EnteredBy = HolidayController.GetUserString(objdb, obj.EnteredBy);
                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }


        [HttpPost]
        public ActionResult AddNewDays(ExtraHolidays objzone)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.AddHoliday obj = new DSRCManagementSystem.AddHoliday();
            try
            {


                var val = objdb.TimeZones.Where(x => x.Zone == objzone.ZoneName).Select(o => o.Id).FirstOrDefault();
                int UserId = int.Parse(Session["UserID"].ToString());
                var already = objdb.AddHolidays.Where(x => x.Date == objzone.Date && x.ZoneId == val && x.Isactive == true && x.HolidayName == objzone.HolidayName).Select(o => o).FirstOrDefault();
                var same = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == val && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                var check2 = objdb.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();

                if (already != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }

                else if (same != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }

                else if (check2 != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

                }

                if (val == 5)
                {
                    var allalready1 = objdb.AddHolidays.Where(x => x.HolidayName == objzone.HolidayName && x.Date == objzone.Date && x.ZoneId == 1 && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var same1 = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == 1 && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var check3 = objdb.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();

                    if (allalready1 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                    else if (same1 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                    else if (check3 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

                    }


                    else
                    {
                        DSRCManagementSystem.AddHoliday db = new DSRCManagementSystem.AddHoliday();
                        db.HolidayName = objzone.HolidayName;
                        db.Date = objzone.Date;
                        db.ZoneId = 1;
                        db.Isactive = true;
                        db.EnteredBy = UserId.ToString();
                        objdb.AddToAddHolidays(db);
                    }

                    var allalready2 = objdb.AddHolidays.Where(x => x.HolidayName == objzone.HolidayName && x.Date == objzone.Date && x.ZoneId == 2 && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var same2 = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == 2 && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var check4 = objdb.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();



                    if (allalready2 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else if (same2 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                    else if (check4 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        DSRCManagementSystem.AddHoliday odb = new DSRCManagementSystem.AddHoliday();
                        odb.HolidayName = objzone.HolidayName;
                        odb.Date = objzone.Date;
                        odb.ZoneId = 2;
                        odb.Isactive = true;
                        odb.EnteredBy = UserId.ToString();
                        objdb.AddToAddHolidays(odb);
                    }

                    var allalready3 = objdb.AddHolidays.Where(x => x.HolidayName == objzone.HolidayName && x.Date == objzone.Date && x.ZoneId == 3 && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var same3 = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == 3 && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var check5 = objdb.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();


                    if (allalready3 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else if (same3 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else if (check5 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        DSRCManagementSystem.AddHoliday data = new DSRCManagementSystem.AddHoliday();
                        data.HolidayName = objzone.HolidayName;
                        data.Date = objzone.Date;
                        data.ZoneId = 3;
                        data.Isactive = true;
                        data.EnteredBy = UserId.ToString();
                        objdb.AddToAddHolidays(data);

                    }


                    var allalready4 = objdb.AddHolidays.Where(x => x.HolidayName == objzone.HolidayName && x.Date == objzone.Date && x.ZoneId == 4 && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var same4 = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == 4 && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                    var check6 = objdb.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();


                    if (allalready4 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else if (same4 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else if (check6 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        DSRCManagementSystem.AddHoliday data = new DSRCManagementSystem.AddHoliday();
                        data.HolidayName = objzone.HolidayName;
                        data.Date = objzone.Date;
                        data.ZoneId = 4;
                        data.Isactive = true;
                        data.EnteredBy = UserId.ToString();
                        objdb.AddToAddHolidays(data);

                    }
                    objdb.SaveChanges();

                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    obj.HolidayName = objzone.HolidayName;
                    obj.Date = objzone.Date;
                    obj.ZoneId = val;
                    obj.Isactive = true;
                    obj.EnteredBy = UserId.ToString();
                    objdb.AddToAddHolidays(obj);
                    objdb.SaveChanges();
                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
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




        [HttpGet]
        public ActionResult AddNewDaysEdit(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.ExtraHolidays obj = new DSRCManagementSystem.Models.ExtraHolidays();
            try
            {
                obj = (from p in objdb.AddHolidays.Where(x => x.HolidayId == Id)
                       join t in objdb.TimeZones on p.ZoneId equals t.Id
                       select new DSRCManagementSystem.Models.ExtraHolidays
                       {
                           ZoneId = t.Id,
                           HolidayName = p.HolidayName,
                           Date = p.Date,
                           EnteredBy = p.EnteredBy,
                           ZoneName = t.Zone
                       }).FirstOrDefault();

                obj.Id = Id;

                DateTime d1 = Convert.ToDateTime(obj.Date);
                string d = d1.ToShortDateString();
                obj.HolidayDate = d;


                var EmailList = (from p in objdb.Users.Where(x => x.IsActive == true)
                                 select new
                                 {
                                     UserId = p.UserID,
                                     UserName = p.FirstName + "" + (p.LastName ?? "")
                                 }).ToList();

                var EmailList1 = (from p in objdb.Users.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      UserId = p.UserID,
                                      UserName1 = p.FirstName + "" + (p.LastName ?? "")
                                  }).ToList();

                var EmailList2 = (from p in objdb.Users.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      Id2 = p.UserID,
                                      UserName2 = p.FirstName + "" + (p.LastName ?? "")
                                  }).ToList();

                var Purpose = (from pi in objdb.TimeZones
                               select new
                               {
                                   Id3 = pi.Id,
                                   Template = pi.Zone
                               }).ToList();

                ViewBag.Email = new SelectList(EmailList, "UserId", "UserName", obj.EnteredBy);
                ViewBag.Email1 = new SelectList(EmailList1, "UserId", "UserName1", obj.ApprovedBy);
                ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", obj.ZoneId);


                obj.EnteredBy = HolidayController.GetUserString(objdb, obj.EnteredBy);

                TempData["HolidayId"] = Id;
                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }


        [HttpPost]
        public ActionResult AddNewDaysEdit(ExtraHolidays obj)
        {
            int User = int.Parse(Session["UserID"].ToString());
            int HolidayId = Convert.ToInt32(TempData["HolidayId"]);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var val = db.TimeZones.Where(x => x.Zone == obj.ZoneName).Select(o => o.Id).FirstOrDefault();
            int UserId = int.Parse(Session["UserID"].ToString());
            var value = db.AddHolidays.Where(x => x.HolidayId == obj.Id).Select(o => o).FirstOrDefault();
            DSRCManagementSystem.AddHolidayLog log = new DSRCManagementSystem.AddHolidayLog();
            try
            {
                if (value.Date != obj.Date && value.HolidayName != obj.HolidayName)
                {
                    log.ColumnChanged = "Date / HolidayName";
                    log.TableId = 1485248346;
                    log.ChangedBy = UserId;
                    log.PeviousValue = value.Date.ToString() + " ," + value.HolidayName;
                    log.CurrentValue = obj.Date.ToString() + " ," + value.HolidayName;
                    log.date = System.DateTime.Now;
                    db.AddToAddHolidayLogs(log);
                    db.SaveChanges();
                }

                else if (value.Date != obj.Date)
                {
                    log.ColumnChanged = "Date";
                    log.TableId = 1485248346;
                    log.ChangedBy = UserId;
                    log.PeviousValue = value.Date.ToString();
                    log.CurrentValue = obj.Date.ToString();
                    log.date = System.DateTime.Now;
                    db.AddToAddHolidayLogs(log);
                    db.SaveChanges();
                }
                else if (value.HolidayName != obj.HolidayName)
                {
                    log.ColumnChanged = "HolidayName";
                    log.TableId = 1485248346;
                    log.ChangedBy = UserId;
                    log.PeviousValue = value.HolidayName;
                    log.CurrentValue = obj.HolidayName;
                    log.date = System.DateTime.Now;
                    db.AddToAddHolidayLogs(log);
                    db.SaveChanges();
                }

                if (value.Date != obj.Date && value.HolidayName != obj.HolidayName)
                {
                    var check = db.AddHolidays.Where(x => x.Date == obj.Date && x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();
                    var check2 = db.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();
                    if (check != null || check2 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        value.HolidayName = obj.HolidayName;
                        value.ZoneId = val;
                        value.Isactive = true;
                        value.EnteredBy = UserId.ToString();
                        value.Date = obj.Date;
                        db.SaveChanges();
                    }
                }

                else if (value.Date == obj.Date && value.HolidayName != obj.HolidayName)
                {
                    var check = db.AddHolidays.Where(x => x.Date == obj.Date && x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();
                    var check2 = db.AddHolidays.Where(x => x.HolidayName == obj.HolidayName && x.ZoneId == val).Select(o => o).FirstOrDefault();
                    if (check != null || check2 != null)
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        value.HolidayName = obj.HolidayName;
                        value.ZoneId = val;
                        value.Isactive = true;
                        value.EnteredBy = UserId.ToString();
                        value.Date = obj.Date;
                        db.SaveChanges();
                    }

                }
                value.HolidayName = obj.HolidayName;
                value.ZoneId = val;
                value.Isactive = true;
                value.EnteredBy = UserId.ToString();
                value.Date = obj.Date;
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
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
        public ActionResult Delete(int Id)
        {
            int UserId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                DSRCManagementSystem.AddHolidayLog log = new AddHolidayLog();
                var value = db.AddHolidays.Where(x => x.HolidayId == Id).Select(o => o).FirstOrDefault();
                value.Isactive = false;
                log.ChangedBy = UserId;
                log.ColumnChanged = "IsActive";
                log.PeviousValue = "1";
                log.CurrentValue = "0";
                log.date = System.DateTime.Now;
                log.TableId = 1485248346;
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpGet]
        public ActionResult DashBoard(string Year)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.HolidayDashBoard> obj = new List<DSRCManagementSystem.Models.HolidayDashBoard>();
            try
            {
                obj = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == 3)
                       join t in objdb.TimeZones on p.ZoneId equals t.Id
                       select new DSRCManagementSystem.Models.HolidayDashBoard
                       {
                           ZoneId = p.ZoneId,
                           Id = p.HolidayId,
                           HolidayName = p.HolidayName,
                           ZoneName = t.Zone,
                           Date = p.Date,
                           EnteredBy = p.EnteredBy,
                       }).OrderBy(x => x.Date).ToList();

                var Purpose = (from pi in objdb.Master_DifferentZone
                               select new
                               {
                                   Id3 = pi.Id,
                                   Template = pi.Zone
                               }).ToList();

                obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());

                ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", 3);

                foreach (var meetingSchedule in obj)
                {
                    meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                }
                obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());
                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        [HttpPost]
        public ActionResult DashBoard(HolidayDashBoard obj, FormCollection form)
        {
            string ZoneId = (form["Id3"] == "") ? "0" : form["Id3"].ToString();
            int Id = Convert.ToInt32(ZoneId);
            List<DSRCManagementSystem.Models.HolidayDashBoard> value = new List<DSRCManagementSystem.Models.HolidayDashBoard>();
            try
            {


                if (Id != 0)
                {
                    DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                    value = (from p in objdb.AddHolidays.Where(x => x.Isactive == true && x.ZoneId == Id)
                             join t in objdb.TimeZones on p.ZoneId equals t.Id
                             select new DSRCManagementSystem.Models.HolidayDashBoard
                             {
                                 ZoneId = p.ZoneId,
                                 Id = p.HolidayId,
                                 HolidayName = p.HolidayName,
                                 ZoneName = t.Zone,
                                 Date = p.Date,
                                 EnteredBy = p.EnteredBy,
                             }).OrderBy(x => x.Date).ToList();

                    if (value.Count() == 0)
                    {
                        return RedirectToAction("DashBoard", "Leave");
                    }

                    value.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());
                    var Purpose = (from pi in objdb.Master_DifferentZone
                                   select new
                                   {
                                       Id3 = pi.Id,
                                       Template = pi.Zone
                                   }).ToList();
                    value.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());

                    foreach (var item in value)
                    {

                        ViewBag.Purpose = new SelectList(Purpose, "Id3", "Template", item.ZoneId);

                    }

                    foreach (var meetingSchedule in value)
                    {
                        meetingSchedule.EnteredBy = HolidayController.GetUserString(objdb, meetingSchedule.EnteredBy);
                    }
                    return View(value);
                }
                else
                {
                    return RedirectToAction("DashBoard", "Leave");
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
        public ActionResult Reset1()
        {
            return RedirectToAction("DashBoard", "Leave");
        }

        [HttpGet]
        public ActionResult ViewHoliday(string year)
        {
            try
            {
            var currentyears = DateTime.Now.Year;
            int HolidayYear = Convert.ToInt32(year);
            int Year = (HolidayYear == null) ? 0 : HolidayYear;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.Format> obj = new List<DSRCManagementSystem.Models.Format>();
            List<DSRCManagementSystem.Models.Format> objt = new List<DSRCManagementSystem.Models.Format>();
            var calendaryear = db.CalendarYears.Select(o => o).FirstOrDefault();
            var startingmonth = calendaryear.StartingMonth;
            var endingmonth = calendaryear.EndingMonth;
            List<SelectListItem> objyear = new List<SelectListItem>();
            for (int i = 0; i < 2; i++)
            {
                string strMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(startingmonth));
                string strEndingMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(endingmonth));
                var currentyear = 0;
                var previousyear = 0;
                var year1 = 0;
                if (i == 0)
                {
                    currentyear = DateTime.Now.Year;
                    previousyear = Convert.ToInt32(currentyear) - 1;
                    year1 = Convert.ToInt32(previousyear) + 1;
                }
                else
                {
                    currentyear = DateTime.Now.Year;
                    previousyear = Convert.ToInt32(currentyear);

                    if (i == 1)
                    {
                        year1 = Convert.ToInt32(previousyear) + i;
                    }

                    else
                    {
                        year1 = Convert.ToInt32(previousyear) + (i - 1);

                    }

                }
                string substring1 = strMonthName.Remove(strMonthName.Length - 2);
                string substring2 = strEndingMonthName.Remove(strEndingMonthName.Length - 2);
                string startingstring = substring1 + ", " + previousyear + " " + " - " + substring2 + "," + year1;
                string valuei;
                if (i == 0)
                {
                    valuei = Convert.ToString(currentyears - (i + 1));
                }
                else
                {
                    valuei = Convert.ToString(currentyears);
                }
                objyear.Add(new SelectListItem { Text = startingstring, Value = valuei });
            }



            int LeaveUserId = (int)Session["UserId"];
            var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();
            if (HolidayYear != null && HolidayYear != 0 && HolidayYear != 1)
            {

                int xyear = Year + 1;

                var userholiday = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true && (x.Date.Value.Year == Year && x.Date.Value.Month > 3)).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ToList();
                var xuserholiday = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true && (x.Date.Value.Year == xyear && x.Date.Value.Month < 4)).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ToList();
                //List<object> ob = new List<object>();

                foreach (var u in userholiday)
                {
                    DSRCManagementSystem.Models.Format ob = new DSRCManagementSystem.Models.Format();
                    ob.Date = u.Date;
                    ob.HolidayName = u.HolidayName;
                    objt.Add(ob);



                }
                foreach (var x in xuserholiday)
                {
                    DSRCManagementSystem.Models.Format ob1 = new DSRCManagementSystem.Models.Format();
                    ob1.Date = x.Date;
                    ob1.HolidayName = x.HolidayName;
                    objt.Add(ob1);


                }



                var Years = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).Select(o => (int?)o.Date.Value.Year).Distinct().ToList();




                ViewBag.Years = new SelectList(objyear, "Value", "Text");
                return View(objt);

            }
            else
            {
                int currentYear = DateTime.Today.Year;
                var yearholiday = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true && x.Date.Value.Year == currentYear).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ToList();
                var Years = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).Select(o => (int?)o.Date.Value.Year).Distinct().ToList();
                ViewBag.Years = new SelectList(objyear, "Value", "Text");


                foreach (var u in yearholiday)
                {
                    DSRCManagementSystem.Models.Format ob = new DSRCManagementSystem.Models.Format();
                    ob.Date = u.Date;
                    ob.HolidayName = u.HolidayName;
                    objt.Add(ob);
                }
                return View(objt);
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
        public ActionResult GetZone(string Year, string Financial, string Id)
        {
            int year = 0;
            if (Year != "")
            {
                year = Convert.ToInt32(Year);
            }

            //int country = Convert.ToInt32(Financial);

            string sub = "";

            if (Financial != "--Select--" && Financial != "")
            {
                int count = 4;
                sub = Financial.Substring(Financial.Length - count, count);
            }


            return Json(new { year = year, financial = sub, Id = Id }, JsonRequestBehavior.AllowGet);

            //  return Json(year, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetYear1(string Date, string Country, string DateId)
        {
            int country = Convert.ToInt32(Country);

            int count = 4;
            string sub = Date.Substring(Date.Length - count, count);

            return Json(new { date = sub, holidaycountry = country, dateid = DateId }, JsonRequestBehavior.AllowGet);
        }



    }
}
