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

namespace DSRCManagementSystem.Controllers
{
    [DSRCAuthorize]
    public class LeaveController : Controller
    {
        string[] day;
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult LeaveRequests()
        {
            var leaveRequestsData = new List<LeaveRequest>();
            try
            {
                var userId = (int)Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "All LeaveStatus" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", 0);

                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,
                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
                ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).OrderBy(x=>x.DepartmentName).ToList()), "DepartmentId", "DepartmentName", 0);
                ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
                ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
                var leaveRequestQuery = from leaveRequests in db.LeaveRequests
                                        where leaveRequests.UserId == userId
                                        orderby leaveRequests.StartDateTime descending
                                        select leaveRequests;
                leaveRequestsData = GetLeaveRequestsQuery(leaveRequestQuery, null, null).ToList();
                ViewBag.anyPendingLeaveRequests = leaveRequestsData.Any(item => item.LeaveStatusId == db.Master_LeaveStatus.First(i => i.Status.Equals("Pending", StringComparison.InvariantCultureIgnoreCase)).LeaveStatusId);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LeaveRequests", leaveRequestsData);
                }
                var userstatus = db.Users.Where(u => u.UserID == userId && u.IsActive == true).Select(u => u).FirstOrDefault();//(from u in db.Users.Where(u => u.UserID == userId && u.IsActive == true) select u.IsUnderNoticePeriod).FirstOrDefault();
                if (userstatus.UserStatus == 2)
                {
                    ViewBag.isUnderNoticePeriod = true;
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestsData);
        }
        [HttpPost]
        public ActionResult ApplyLeave(FormCollection collection)
        {
            int leaveStatusSearchId = Convert.ToInt32(collection["LeaveStatus"]) != 0 ? Convert.ToInt32(collection["LeaveStatus"]) : 0;

            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "All LeaveStatus" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", leaveStatusSearchId);

            var leaveRequestQuery = from leaveRequests in db.LeaveRequests
                                    where leaveRequests.UserId == userId
                                    orderby leaveRequests.StartDateTime descending
                                    select leaveRequests;

            var leaveRequestsData = GetLeaveRequestsQuery(leaveRequestQuery, null, leaveStatusSearchId).ToList();
            ViewBag.anyPendingLeaveRequests = leaveRequestsData.Any(item => item.LeaveStatusId == db.Master_LeaveStatus.First(i => i.Status.Equals("Pending", StringComparison.InvariantCultureIgnoreCase)).LeaveStatusId);
            ViewBag.isUnderNoticePeriod = (from u in db.Users.Where(u => u.UserID == userId && u.IsActive == true) select u.IsUnderNoticePeriod).FirstOrDefault();

            return View(leaveRequestsData);

        }

        [HttpPost]
        public ViewResult LeaveRequests(FormCollection collection)
        {
            var leaveRequestsData = new List<LeaveRequest>();
            try
            {
                int leaveStatusSearchId = Convert.ToInt32(collection["LeaveStatus"]) != 0 ? Convert.ToInt32(collection["LeaveStatus"]) : 0;
                var userId = (int)Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", leaveStatusSearchId);

                var leaveRequestQuery = from leaveRequests in db.LeaveRequests
                                        where leaveRequests.UserId == userId
                                        orderby leaveRequests.StartDateTime descending
                                        select leaveRequests;

                leaveRequestsData = GetLeaveRequestsQuery(leaveRequestQuery, null, leaveStatusSearchId).ToList();
                ViewBag.anyPendingLeaveRequests = leaveRequestsData.Any(item => item.LeaveStatusId == db.Master_LeaveStatus.First(i => i.Status.Equals("Pending", StringComparison.InvariantCultureIgnoreCase)).LeaveStatusId);
                ViewBag.isUnderNoticePeriod = (from u in db.Users.Where(u => u.UserID == userId && u.IsActive == true) select u.IsUnderNoticePeriod).FirstOrDefault();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestsData);
        }

        [HttpGet]
        public ViewResult CreateLeaveRequest()
        {
            try
            {
                DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
                var userId = (int)Session["UserId"];
                int gender = dbHrms.Users.First(item => item.UserID == userId).Gender ?? 1;
                string userGender = gender == 1 ? "Male" : "Female";
                ViewBag.Reporting = new SelectList(GetReportingPersons(userId), "UserId", "Name");
                var chk = dbHrms.Users.Where(x => x.UserID == userId).Select(o => o).FirstOrDefault();

                if (chk.Gender == MasterEnum.Genders.Female.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Single.GetHashCode())
                {
                    ViewBag.LeaveTypeList = new SelectList(dbHrms.LeaveTypes.Where(i => i.ApplicableEmployees.Equals("All", StringComparison.OrdinalIgnoreCase) || i.ApplicableEmployees.Equals(userGender, StringComparison.OrdinalIgnoreCase)).ToList().Where(i => LeaveTypeModel.IsEmployeeEligibleForLeave(i.Name, userId) && i.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()), "LeaveTypeId", "Name");
                }
                else if (chk.Gender == MasterEnum.Genders.Female.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    ViewBag.LeaveTypeList = new SelectList(dbHrms.LeaveTypes.Where(i => i.ApplicableEmployees.Equals("All", StringComparison.OrdinalIgnoreCase) || i.ApplicableEmployees.Equals(userGender, StringComparison.OrdinalIgnoreCase)).ToList().Where(i => LeaveTypeModel.IsEmployeeEligibleForLeave(i.Name, userId) && i.LeaveTypeId != MasterEnum.LeaveTypes.Marriage.GetHashCode()), "LeaveTypeId", "Name");
                }
                else if (chk.Gender == MasterEnum.Genders.Male.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    ViewBag.LeaveTypeList = new SelectList(dbHrms.LeaveTypes.Where(i => i.ApplicableEmployees.Equals("All", StringComparison.OrdinalIgnoreCase) || i.ApplicableEmployees.Equals(userGender, StringComparison.OrdinalIgnoreCase)).ToList().Where(i => LeaveTypeModel.IsEmployeeEligibleForLeave(i.Name, userId) && i.LeaveTypeId != MasterEnum.LeaveTypes.Marriage.GetHashCode() && i.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()), "LeaveTypeId", "Name");
                }
                else if (chk.Gender == MasterEnum.Genders.Male.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Single.GetHashCode())
                {
                    ViewBag.LeaveTypeList = new SelectList(dbHrms.LeaveTypes.Where(i => i.ApplicableEmployees.Equals("All", StringComparison.OrdinalIgnoreCase) || i.ApplicableEmployees.Equals(userGender, StringComparison.OrdinalIgnoreCase)).ToList().Where(i => LeaveTypeModel.IsEmployeeEligibleForLeave(i.Name, userId) && i.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()), "LeaveTypeId", "Name");
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
        public ActionResult CreateLeaveRequest(LeaveModel leaveRequest)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var LeaveType = new DSRCManagementSystemEntities1().LeaveTypes.ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
                int UserId = (int)Session["UserId"];
                ViewBag.Reporting = new SelectList(GetReportingPersons(UserId), "UserId", "Name");
                DateTime startdate = leaveRequest.StartDateTime;
                DateTime enddate = leaveRequest.EndDateTime;
                leaveRequest.WorkedDate1 = leaveRequest.WorkedDate1;
                leaveRequest.StartDateTime = leaveRequest.StartDateTime.AddHours(9);
                leaveRequest.EndDateTime = leaveRequest.EndDateTime.AddHours(18);
                if (leaveRequest.StartDateTime >= leaveRequest.EndDateTime)
                    ModelState.AddModelError("DateTime", "Start Date must be lower than End Date");

                int LeaveUser = (int)Session["UserId"];

                var UserRegion = db.Users.Where(x => x.UserID == LeaveUser).Select(o => o.Region).FirstOrDefault();

                var holidayList = db.AddHolidays.Where(x => x.ZoneId == UserRegion && x.Isactive == true && x.Date >= leaveRequest.StartDateTime.Date && x.Date <= leaveRequest.EndDateTime.Date).Select(x => x.Date).ToList();


                var HolidayName = db.AddHolidays.Where(x => x.ZoneId == UserRegion && x.Isactive == true).Select(x => x.Date).ToList();

                if (HolidayName.Contains(leaveRequest.StartDateTime.Date) || HolidayName.Contains(leaveRequest.EndDateTime.Date))
                    return Json(new { Result = "Holiday" }, JsonRequestBehavior.AllowGet);
                if (holidayList.Count > 0 && leaveRequest.StartDateTime.Date == leaveRequest.EndDateTime.Date)
                    return Json(new { Result = "Holiday" }, JsonRequestBehavior.AllowGet);

                int CancelledCode = MasterEnum.LeaveStatus.Cancelled.GetHashCode();
                int RejectedCode = MasterEnum.LeaveStatus.Rejected.GetHashCode();
                var cancelled = MasterEnum.RequestStatus.Cancelled.GetHashCode();
                var Rejected = MasterEnum.RequestStatus.Rejected.GetHashCode();
                bool Pending1 = Convert.ToBoolean(MasterEnum.LeaveStatus.Pending.GetHashCode());
                bool Approved1 = Convert.ToBoolean(MasterEnum.LeaveStatus.Approved.GetHashCode());
                byte Pending = Convert.ToByte(Pending1);
                byte Approved = Convert.ToByte(Approved1);
                int Marriage = MasterEnum.LeaveTypes.Marriage.GetHashCode();
                int Comp_Off = MasterEnum.LeaveTypes.Comp_Off.GetHashCode();
                int Maternity = MasterEnum.LeaveTypes.Maternity.GetHashCode();

                if (ModelState.IsValid)
                {
                    var name = db.Users.Where(o => o.UserID == leaveRequest.LeaveRequestTo && o.IsActive == true).FirstOrDefault();

                    leaveRequest.ReportingPersonName = (name.FirstName + " " + (name.LastName ?? "")).Trim();
                    leaveRequest.UserName = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == UserId).FirstName;
                    leaveRequest.LastName = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == UserId).LastName;
                    leaveRequest.ReportingPersonEmail = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequest.LeaveRequestTo).EmailAddress;

                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                    int i = leaveRequest.LeaveType;
                    var a = GetLeaveBalance(year, UserId);

                    leaveRequest.Balance = (from b in a
                                            select new LevaeBalance()
                                            {
                                                LeaveTypeId = b.LeaveTypeId,
                                                Name = b.LeaveType,
                                                DaysAllowed = (double)b.DaysAllowed,
                                                UsedDays = (double)b.UsedDays,

                                            }).ToList();

                    double j = leaveRequest.Balance.Where(o => o.LeaveTypeId == i).Select(o => o.RemainingDays).FirstOrDefault();
                    double w = (leaveRequest.EndDateTime.Date - leaveRequest.StartDateTime.Date).TotalDays;
                    int totaldays = Convert.ToInt32(w);

                    List<DateTime?> obj = new List<DateTime?>();
                    for (int z = 0; z < w; z++)
                    {
                        obj.Add(leaveRequest.StartDateTime.AddDays(z).Date);
                    }
                    if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= startdate && o.ODEndDate >= startdate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).Select(o => o).ToList().Count > 0)
                    {
                        return Json(new { Result = "OD" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= startdate && o.ODEndDate >= startdate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                    {
                        return Json(new { Result = "OD" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= enddate && o.ODEndDate >= enddate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                    {
                        return Json(new { Result = "OD" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate <= startdate && o.ODEndDate >= enddate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                    {
                        return Json(new { Result = "OD" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (db.OutOfOfficeDetails.Where(o => o.ODStartDate >= startdate && o.ODEndDate <= enddate).Where(o => o.Userid == UserId).Where(o => o.RequestStatusId != cancelled && o.RequestStatusId != Rejected).ToList().Count > 0)
                    {
                        return Json(new { Result = "OD" }, JsonRequestBehavior.AllowGet);
                    }
                    if (leaveRequest.HalfDay)
                    {
                        if (!(leaveRequest.LeaveType == MasterEnum.LeaveTypes.Sick.GetHashCode() || leaveRequest.LeaveType == MasterEnum.LeaveTypes.Casual.GetHashCode() || leaveRequest.LeaveType == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode()))
                        {
                            return Json(new { Result = "InvalidLeaveType" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (!leaveRequest.HalfDay)
                    {
                        if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Select(o => o).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.EndDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime >= leaveRequest.StartDateTime && o.EndDateTime <= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Where(o => o.LeaveDays != 0.5).Select(o => o).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Where(o => o.LeaveDays != 0.5).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.EndDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Where(o => o.LeaveDays != 0.5).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Where(o => o.LeaveDays != 0.5).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (db.LeaveRequests.Where(o => o.StartDateTime >= leaveRequest.StartDateTime && o.EndDateTime <= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Where(o => o.LeaveDays != 0.5).ToList().Count > 0)
                        {
                            return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }

                        int HalfDayCount = db.LeaveRequests.Where(o => o.StartDateTime == leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId == Pending || o.LeaveStatusId == Approved).Count();

                        if (HalfDayCount != 0)
                        {
                            if (HalfDayCount >= 2)
                                return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    leaveRequest.BranchID = (int)db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == UserId).BranchId;

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                    {
                        string[] splitCompoff = leaveRequest.WorkedDate1.ToString().Split(',');

                        string date = "";
                        bool chk = false, check = false;
                        int counttemp = 0, counttemp1 = 0;

                        for (int count = 0; count < splitCompoff.Count(); count++)
                        {
                            int LeaveUserId = (int)Session["UserId"];

                            var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();

                            int getIndex = splitCompoff[count].ToString().IndexOf('(');
                            string getDay = splitCompoff[count].ToString().Remove(splitCompoff[count].ToString().IndexOf(')')).ToString().Substring(getIndex + 1);
                            string replace = splitCompoff[count].ToString().Remove(getIndex);
                            DateTime d = Convert.ToDateTime(replace);
                            date = d.ToString("dd/MM/yyyy");
                            var holidays = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true).Select(x => x.Date).ToList();

                            foreach (var r in holidays)
                            {
                                if ((r.Value.ToString("dd/MM/yyyy") == date && Convert.ToDateTime(date) < DateTime.Now) || getDay == "Sunday" || getDay == "Saturday")
                                {
                                    counttemp++;
                                }

                                if (splitCompoff.Count() == counttemp)
                                {
                                    chk = true;
                                }
                            }

                            var time = (from us in db.Users.Where(x => x.IsActive == true)
                                        join lea in db.LeaveRequests on us.UserID equals lea.UserId into leave
                                        join tme in db.TimeManagements on us.EmpID equals tme.EmpID
                                        from leavereq in leave.DefaultIfEmpty()
                                        where us.IsActive == true && us.UserID == UserId && tme.BranchId == leaveRequest.BranchID
                                        select new LeaveModel()
                                        {
                                            Date = tme.Date,
                                            Minutes = (int)tme.TotalTime,

                                        }).Distinct().ToList();

                            foreach (var o in time)
                            {
                                if (o.Date.ToString("dd/MM/yyyy") == date && o.Minutes > 300)
                                {
                                    counttemp1++;
                                }
                                if (splitCompoff.Count() == counttemp1)
                                {
                                    check = true;
                                }
                            }
                        }
                        if (chk == false)
                        {
                            return Json(new { Result = "Not Holiday" }, JsonRequestBehavior.AllowGet);
                        }

                        if (check == false)
                        {
                            return Json(new { Result = "not applicable" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                    {
                        double cont = leaveRequest.WorkedDate1.ToString().Split(',').Count();
                        leaveRequest.Dayss = new LeaveBalance().CalculateLeaveDays(leaveRequest.StartDateTime, leaveRequest.EndDateTime, holidayList).LeaveDays;
                        double day = leaveRequest.Dayss;

                        if (cont != day)
                        {
                            return Json(new { Result = "not equal" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                    {

                        string[] splitCompoff = leaveRequest.WorkedDate1.ToString().Split(',');

                        string date = "", date1 = "";
                        bool chk = false;
                        int countemp = 0;
                        for (int count = 0; count < splitCompoff.Count(); count++)
                        {
                            int getIndex = splitCompoff[count].ToString().IndexOf('(');
                            string replace = splitCompoff[count].ToString().Remove(getIndex);
                            date = Convert.ToDateTime(replace).ToString("dd/MM/yyyy");
                            var Work = db.LeaveRequests.Where(x => x.UserId == UserId && x.LeaveTypeId == Comp_Off && x.LeaveStatusId < RejectedCode).Select(x => x.WorkedDate).ToList();
                            foreach (var p in Work)
                            {
                                if (p != null)
                                {
                                    day = p.Split(',');

                                    for (int x = 0; x < day.Count(); x++)
                                    {
                                        int get = day[x].ToString().IndexOf('(');
                                        string replac = "";
                                        if (get != -1)
                                        {
                                            replac = day[x].ToString().Remove(get);
                                        }
                                        else replac = day[x];
                                        date1 = Convert.ToDateTime(replac).ToString("dd/MM/yyyy");
                                        if (date1 == date)
                                        {
                                            countemp++;
                                        }
                                        if (splitCompoff.Count() == countemp)
                                        {
                                            chk = true;
                                        }
                                    }
                                }

                            }
                        }
                        if (chk == true)
                        {
                            return Json(new { Result = "already" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (leaveRequest.HalfDay)
                    {
                        leaveRequest.totalLeaveDays = 0.5;
                    }
                    else
                    {
                        leaveRequest.totalLeaveDays = 0.0;
                    }

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                    {
                        if (LeaveType.First(type => type.LeaveTypeId == MasterEnum.LeaveTypes.Comp_Off.GetHashCode()) != null)
                        {
                            int count = leaveRequest.WorkedDate1.ToString().Split(',').Count();
                            leaveRequest.totalLeaveDays = count;
                        }
                    }

                    else if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                    {
                        if (LeaveType.First(type => type.LeaveTypeId == MasterEnum.LeaveTypes.Maternity.GetHashCode()) != null)
                        {
                            int count = 90;
                            leaveRequest.totalLeaveDays = count;
                        }
                    }

                    else if (!(leaveRequest.HalfDay))
                    {
                        leaveRequest.totalLeaveDays = new LeaveBalance().CalculateLeaveDays(leaveRequest.StartDateTime, leaveRequest.EndDateTime, holidayList).LeaveDays;
                    }

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                    {
                        var leaobj = db.LeaveRequests.Where(x => x.LeaveTypeId == Maternity && x.UserId == UserId && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Count();

                        if (Convert.ToInt32(leaobj) >= 2)
                        {
                            return Json(new { Result = "Maternity already applied" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                    {
                        var matobj = db.LeaveRequests.Where(x => x.UserId == UserId && x.LeaveTypeId == 6 && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Select(o => o.EndDateTime).FirstOrDefault();
                        DateTime Dt = Convert.ToDateTime(matobj);
                        Dt = Dt.AddMonths(12);
                        DateTime startda = leaveRequest.StartDateTime;
                        if (startda < Dt)
                        {
                            return Json(new { Result = "unavailable" }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        var leaobj = db.LeaveRequests.Where(x => x.LeaveTypeId == Marriage && x.UserId == UserId && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Count();

                        if (Convert.ToInt32(leaobj) >= 1)
                        {
                            return Json(new { Result = "Marriage already applied" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (leaveRequest.LeaveType != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() && leaveRequest.LeaveType != MasterEnum.LeaveTypes.Maternity.GetHashCode())
                    {
                        leaveRequest.LOPdays = Convert.ToDouble(TempData["LOPdays"]);
                    }
                    else
                    {
                        leaveRequest.LOPdays = 0.0;
                    }

                    db.LeaveRequests.AddObject(new LeaveRequest()
                    {
                        LeaveTypeId = leaveRequest.LeaveType,
                        StartDateTime = leaveRequest.StartDateTime,
                        EndDateTime = leaveRequest.EndDateTime.AddSeconds(-1),
                        WorkedDate = leaveRequest.WorkedDate1,
                        UserId = (int)Session["UserId"],
                        LeaveStatusId = (byte)1,
                        Details = leaveRequest.Details,
                        LeaveDays = leaveRequest.totalLeaveDays,
                        ReportingTo = leaveRequest.LeaveRequestTo,
                        RequestedDate = DateTime.Now,
                        LOP = leaveRequest.LOPdays
                    });

                    db.SaveChanges();
                    leaveRequest.LeaveRequestedId = db.LeaveRequests.Where(o => o.UserId == UserId && EntityFunctions.TruncateTime(o.StartDateTime) == EntityFunctions.TruncateTime(leaveRequest.StartDateTime) && EntityFunctions.TruncateTime(o.EndDateTime) == EntityFunctions.TruncateTime(leaveRequest.EndDateTime) && o.ReportingTo == leaveRequest.LeaveRequestTo).Select(o => o.LeaveRequestId).FirstOrDefault();

                    leaveRequest.LeaveTypeName = db.LeaveTypes.Where(o => o.LeaveTypeId == leaveRequest.LeaveType).Select(o => o.Name).FirstOrDefault();

                    var doj = db.Users.Where(x => x.IsActive == true).FirstOrDefault(item => item.UserID == UserId).DateOfJoin;

                    bool isEligible;

                    if (doj == null)
                        isEligible = false;
                    else
                    {
                        var completedDays = (DateTime.Now - doj).Value.Days;

                        if (completedDays < 365)
                            isEligible = false;
                        else
                            isEligible = true;
                    }

                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Leave Request").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {

                         var objLeaveRequest = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request")
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
                         string TemplatePathLeaveRequest = Server.MapPath(objLeaveRequest.Template);
                         string htmlLeaveRequest = System.IO.File.ReadAllText(TemplatePathLeaveRequest);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#ReportingPersonName", leaveRequest.ReportingPersonName);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#EmployeeName", leaveRequest.UserName + " " + leaveRequest.LastName);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#LeaveTypeName", leaveRequest.LeaveTypeName);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#StartDateTime", leaveRequest.StartDateTime.ToString("ddd, MMM d, yyyy"));
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#EndDateTime", leaveRequest.EndDateTime.ToString("ddd, MMM d, yyyy"));
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#totalLeaveDays", leaveRequest.totalLeaveDays.ToString());
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#CompanyName", company);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#Balance[0]RemainingDays", leaveRequest.Balance[0].RemainingDays <= 0 ? "0" : leaveRequest.Balance[0].RemainingDays.ToString());
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#Balance[1]RemainingDays", leaveRequest.Balance[1].RemainingDays <= 0 ? "0" : leaveRequest.Balance[1].RemainingDays.ToString());

                         if (isEligible)
                         {
                             string EarnedLeave = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  Earned  Leave Available&nbsp;&nbsp;:&nbsp;<label style='color: Black;'>" + (leaveRequest.Balance[2].RemainingDays <= 0 ? "0" : leaveRequest.Balance[2].RemainingDays.ToString()) + "</label></p>";
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#Balance[2]RemainingDays", EarnedLeave);
                         }
                         else
                         {
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#Balance[2]RemainingDays", "");
                         }
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#LeaveRequestedId", Encrypter.Encode(leaveRequest.LeaveRequestedId.ToString()));
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#Details", leaveRequest.Details);
                         htmlLeaveRequest = htmlLeaveRequest.Replace("#ServerName", ServerName);
                         if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                         {
                             string workingdate = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  Worked Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp:<label style='color: Black;'>" + leaveRequest.WorkedDate1 + "</label></p>";
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#WorkingDate", workingdate);
                         }
                         else
                         {
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#WorkingDate", "");
                         }
                         if (leaveRequest.LOPdays > 0.0)
                         {
                             string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp:<label style='color: Black;'>" + leaveRequest.LOPdays + "</label></p>";
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#LOP", LOPDays);
                         }
                         else
                         {
                             htmlLeaveRequest = htmlLeaveRequest.Replace("#LOP", "");
                         }


                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
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
                                 // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequest.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequest.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //  DsrcMailSystem.MailSender.SendMail(null, objLeaveRequest.Subject, null, htmlLeaveRequest, "admin@dsrc.co.in", leaveRequest.ReportingPersonEmail, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequest.Subject, null, htmlLeaveRequest, "admin@dsrc.co.in", leaveRequest.ReportingPersonEmail, Server.MapPath(logo.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Leave Request", folder, ServerName);
                     }
                    return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequest);
        }

        [HttpGet]
        public ActionResult SubmittedLeaveRequests()
        {
            var leaveRequestsResult = new List<LeaveRequest>();
            try
            {
                var userId = (int)Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestQuery = from leaveRequestsData in db.LeaveRequests
                                        where leaveRequestsData.ReportingTo == userId
                                        orderby leaveRequestsData.StartDateTime descending
                                        select leaveRequestsData;
                ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", 0);
                leaveRequestsResult = GetLeaveRequestsQuery(leaveRequestQuery, null, 1).ToList();
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_SubmittedLeaveRequests", leaveRequestsResult);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model: leaveRequestsResult);
        }

        [HttpPost]
        public ViewResult SubmittedLeaveRequests(FormCollection collection)
        {
            var leaveRequestsResult = new List<LeaveRequest>();
            try
            {
                int leaveStatusSearchId = Convert.ToInt32(collection["LeaveStatus"]) != 0 ? Convert.ToInt32(collection["LeaveStatus"]) : 0;
                var userId = (int)Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestQuery = from leaveRequestsData in db.LeaveRequests
                                        where leaveRequestsData.ReportingTo == userId
                                        orderby leaveRequestsData.StartDateTime descending
                                        select leaveRequestsData;

                ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", leaveStatusSearchId);
                leaveRequestsResult = GetLeaveRequestsQuery(leaveRequestQuery, null, leaveStatusSearchId).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestsResult);
        }

        public ActionResult UpdateLeaveRequestStatus(int leaveRequestId)
        {
            var leaveRequestToUpdate = new LeaveModel();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                leaveRequestToUpdate = (from lr in db.LeaveRequests
                                        where lr.LeaveRequestId == leaveRequestId
                                        select new LeaveModel()
                                        {
                                            LeaveRequestedId = lr.LeaveRequestId,
                                            UserId = lr.UserId,
                                            LeaveTypeId = (byte)lr.LeaveTypeId,
                                            StartDateTime = (DateTime)lr.StartDateTime,
                                            EndDateTime = (DateTime)lr.EndDateTime,
                                            LeaveStatusId = (byte)lr.LeaveStatusId,
                                            totalLeaveDays = (double)lr.LeaveDays,
                                            UserName = lr.User.FirstName,
                                            LastName = lr.User.LastName,
                                            FullName = lr.User.FirstName + " " + (lr.User.LastName ?? ""),
                                            Details = lr.Details,
                                            Comments = lr.Comments,
                                            RequestedDate = lr.RequestedDate,
                                            ProcessedOn = lr.ProcessedOn,
                                            WorkedDate1 = lr.WorkedDate,
                                        }).FirstOrDefault();
                ViewBag.LeaveTypeList = new SelectList(db.LeaveTypes.ToList(), "LeaveTypeId", "Name", leaveRequestToUpdate.LeaveTypeId);
                ViewBag.LeaveStatusList = new SelectList(db.Master_LeaveStatus.ToList(), "LeaveStatusId", "Status", leaveRequestToUpdate.LeaveStatusId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestToUpdate);
        }

        [HttpPost]
        public ActionResult UpdateLeaveRequestStatus(int leaveRequestId, int leaveStatusId, int reportingUserId, string comments)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var leaveRequestToUpdate = (from leaveRequest in db.LeaveRequests
                                            join userReporting in db.UserReportings on leaveRequest.UserId equals userReporting.UserID
                                            where leaveRequest.LeaveRequestId == leaveRequestId && userReporting.ReportingUserID == reportingUserId
                                            select leaveRequest).First();

                if (leaveRequestToUpdate != null)
                {
                    leaveRequestToUpdate.LeaveStatusId = (byte)leaveStatusId;
                    leaveRequestToUpdate.ProcessedBy = (int)Session["UserId"];
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    leaveRequestToUpdate.Comments = comments;
                    db.SaveChanges();
                    return RedirectToAction("SuccessModalDialog", "Popup", new { modalTitle = "Success", modalContent = "Leave request status has been updated successfully" });
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

        public ActionResult ApproveLeaveRequestStatus(int leaveRequestId)
        {
            var leaveRequestToUpdate = new LeaveRequest();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                leaveRequestToUpdate = db.LeaveRequests.First(x => x.LeaveRequestId == leaveRequestId);
                ViewBag.LeaveTypeList = new SelectList(db.LeaveTypes.ToList(), "LeaveTypeId", "Name", leaveRequestToUpdate.LeaveTypeId);
                ViewBag.LeaveStatusList = new SelectList(db.Master_LeaveStatus.ToList(), "LeaveStatusId", "Status", leaveRequestToUpdate.LeaveStatusId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestToUpdate);
        }

        [HttpPost]
        public ActionResult ApproveLeaveRequestStatus(int leaveRequestId, int leaveStatusId, int reportingUserId, string comments)
        {
            try
            {

                string ServerName = AppValue.GetFromMailAddress("ServerName");
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int UserId = (int)Session["UserId"];

                var leaveRequestToUpdate = (from leaveRequest in db.LeaveRequests
                                            where leaveRequest.LeaveRequestId == leaveRequestId && leaveRequest.ReportingTo == reportingUserId
                                            select leaveRequest).Include(i => i.User).Include(item => item.User1).First();
                double LOPs = 0.0;

                if (leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() && leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    /** For Calculatin LOP calling this Mehtod if check availability is not called*****/
                    GetLOPDays(leaveRequestId);
                    LOPs = Convert.ToDouble(TempData["LOP"]);
                }

                if (leaveRequestToUpdate != null)
                {
                    leaveRequestToUpdate.LeaveStatusId = (byte)leaveStatusId;
                    leaveRequestToUpdate.ProcessedBy = (int)Session["UserId"];
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    leaveRequestToUpdate.Comments = comments;
                    leaveRequestToUpdate.LOP = LOPs;
                    db.SaveChanges();

                    //If Marriage Leave is Applied and Approved Marital Status has to be updated in Users Table
                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        var UpdateMaritalStatus = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                        UpdateMaritalStatus.MaritalStatus = MasterEnum.MaritialStatus.Married.GetHashCode(); /* 1-Married 2-UnMarried*/
                        db.SaveChanges();
                    }

                    var FromDate = leaveRequestToUpdate.StartDateTime;
                    var ToDate = leaveRequestToUpdate.EndDateTime;
                    var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                    bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                    LeaveModel leaveRequest = new LeaveModel();
                    var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                    var a = GetLeaveBalance(years, leaveRequestToUpdate.UserId);

                    leaveRequest.Balance = (from b in a
                                            select new LevaeBalance()
                                            {
                                                LeaveTypeId = b.LeaveTypeId,
                                                Name = b.LeaveType,
                                                DaysAllowed = (int)b.DaysAllowed,
                                                UsedDays = (int)b.UsedDays,

                                            }).ToList();
                    double RemainingDays = leaveRequest.Balance.Where(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Select(o => o.RemainingDays).FirstOrDefault();

                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                    {
                        var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                                  where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                                      leavebalance.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId
                                                  select leavebalance).FirstOrDefault();

                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = leaveRequestToUpdate.LeaveTypeId;
                            updateleavebalance.Value = leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();
                        }
                        else
                        {
                            updateleavebalance.Value = updateleavebalance.Value + leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();
                        }
                    }

                    UpdateLeaveBalance(leaveRequestToUpdate);


                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Name;
                    string StartTime = Convert.ToDateTime(leaveRequestToUpdate.StartDateTime).ToString("ddd, MMM d, yyyy");
                    string EndTime = Convert.ToDateTime(leaveRequestToUpdate.EndDateTime).ToString("ddd, MMM d, yyyy");

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Leave Request Approved").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objLeaveRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Approved")
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
                         string TemplatePathLeaveRequestApproved = Server.MapPath(objLeaveRequestApproved.Template);
                         string htmlLeaveRequestApproved = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LeaveTypeName", LeaveTyepName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#StartDateTime", StartTime);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#EndDateTime", EndTime);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#CompanyName", company);

                         if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()))
                         {
                             string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", LOPDays);

                             string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request has to be considered as LOP.</p>";
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", LOP);
                         }
                         else
                         {
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", "");
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", "");
                         }

                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ServerName", ServerName);

                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         var logo = CommonLogic.getLogoPath();

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
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
                                 // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "virupaksha.gaddad@dsrc.co.in ", "", Server.MapPath(logo.AppValue.ToString()));

                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "virupaksha.gaddad@dsrc.co.in ", "", Server.MapPath(logo.ToString()));
                             });
                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //  DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, objLeaveRequestApproved.CC, "", Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, objLeaveRequestApproved.CC, "", Server.MapPath(logo.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Leave Request Approved", folder, ServerName);
                     }
                    return Json(new { Result = "Success" });
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

        public ActionResult RejectLeaveRequestStatus(int leaveRequestId)
        {
            var leaveRequestToUpdate = new LeaveRequest();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                leaveRequestToUpdate = db.LeaveRequests.First(x => x.LeaveRequestId == leaveRequestId);
                ViewBag.LeaveTypeList = new SelectList(db.LeaveTypes.ToList(), "LeaveTypeId", "Name", leaveRequestToUpdate.LeaveTypeId);
                ViewBag.LeaveStatusList = new SelectList(db.Master_LeaveStatus.ToList(), "LeaveStatusId", "Status", leaveRequestToUpdate.LeaveStatusId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestToUpdate);
        }

        [HttpPost]
        public ActionResult RejectLeaveRequestStatus(int leaveRequestId, int leaveStatusId, int reportingUserId, string comments)
        {
            try
            {


                string ServerName = AppValue.GetFromMailAddress("ServerName");
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var leaveRequestToUpdate = (from leaveRequest in db.LeaveRequests
                                            where leaveRequest.LeaveRequestId == leaveRequestId && leaveRequest.ReportingTo == reportingUserId
                                            select leaveRequest).Include(i => i.User).Include(item => item.User1).First();

                if (leaveRequestToUpdate != null)
                {
                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Name;
                    string StartTime = Convert.ToDateTime(leaveRequestToUpdate.StartDateTime).ToString("ddd, MMM d, yyyy");
                    string EndTime = Convert.ToDateTime(leaveRequestToUpdate.EndDateTime).ToString("ddd, MMM d, yyyy");

                    if (leaveRequestToUpdate.LeaveStatusId == MasterEnum.LeaveStatus.Approved.GetHashCode() && leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode())
                    {
                        ////If Marriage Leave is Applied and Approved and Rejected then Marital Status has to be updated in Users Table again
                        if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                        {
                            if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                            {
                                var UpdateMaritalStatus = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                                UpdateMaritalStatus.MaritalStatus = MasterEnum.MaritialStatus.Single.GetHashCode(); /* 1-Married 2-UnMarried*/
                                db.SaveChanges();
                            }

                            var leaveBalanceCountToUpdate = (from leavebalcount in db.LeaveBalanceCounts
                                                             where leavebalcount.UserId == leaveRequestToUpdate.UserId &&
                                                             leavebalcount.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId
                                                             select leavebalcount).FirstOrDefault();
                            if (leaveBalanceCountToUpdate != null)
                            {
                                leaveBalanceCountToUpdate.Value -= leaveRequestToUpdate.LeaveDays;
                                db.SaveChanges();
                            }
                        }

                        if (leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode())
                        {
                            UpdateLeaveBalanceReject(leaveRequestToUpdate);
                        }

                        //Mail To HR For Intimation Purpose
                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Rejection After Approval").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Leave Rejection After Approval").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objLeaveRejectedApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Rejection After Approval")
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
                         string TemplatePathobjLeaveRejectedApproved = Server.MapPath(objLeaveRejectedApproved.Template);
                         string htmlLeaveRejectedApproved = System.IO.File.ReadAllText(TemplatePathobjLeaveRejectedApproved);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#HR", "Umapathy V");
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EmployeeName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LeaveTypeName", LeaveTyepName);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#StartDateTime", StartTime);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EndDateTime", EndTime);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#CompanyName", company);
                         if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()))
                         {
                             string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                             htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", LOPDays);
                             string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                             htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", LOP);
                         }
                         else
                         {
                             htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", "");
                             htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", "");
                         }

                         htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ServerName",ServerName);

                         string EmailAddress = "";
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

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             Task.Factory.StartNew(() =>
                             {
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 // DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRejectedApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRejectedApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {
                             int HRID = Convert.ToInt32(objLeaveRejectedApproved.To);
                             EmailAddress = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == HRID).EmailAddress;

                             Task.Factory.StartNew(() =>
                             {
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //  DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject, null, htmlLeaveRejectedApproved, "admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject, null, htmlLeaveRejectedApproved, "admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Leave Rejection After Approval", folder, ServerName);
                     }
                    }
                    leaveRequestToUpdate.LeaveStatusId = (byte)leaveStatusId;
                    leaveRequestToUpdate.ProcessedBy = (int)Session["UserId"];
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    leaveRequestToUpdate.Comments = comments;
                    db.SaveChanges();

                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folders= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Leave Request Rejected").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {
                         var objLeaveRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Rejected")
                                                        join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                        select new DSRCManagementSystem.Models.Email
                                                        {
                                                            To = p.To,
                                                            CC = p.CC,
                                                            BCC = p.BCC,
                                                            Subject = p.Subject,
                                                            Template = q.TemplatePath
                                                        }).FirstOrDefault();
                         var objcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathLeaveRequestRejected = Server.MapPath(objLeaveRequestRejected.Template);
                         string htmlLeaveRequestRejected = System.IO.File.ReadAllText(TemplatePathLeaveRequestRejected);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LeaveTypeName", LeaveTyepName);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#StartDateTime", StartTime);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#EndDateTime", EndTime);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#Comments", leaveRequestToUpdate.Comments);
                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#CompanyName", objcompany);
                         if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || leaveRequestToUpdate.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()))
                         {
                             string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                             htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", LOPDays);
                             string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                             htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", LOP);
                         }
                         else
                         {
                             htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", "");
                             htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", "");
                         }

                         htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ServerName",ServerName);


                         // var logo = CommonLogic.getLogoPath();
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
                                 var logo = CommonLogic.getLogoPath();
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //  DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = CommonLogic.getLogoPath();
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject, null, htmlLeaveRequestRejected, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject, null, htmlLeaveRequestRejected, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath(logo.ToString()));
                             });
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Leave Request Rejected", folders, ServerName);
                     }
                    return Json(new { Result = "Success" });
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

        public ActionResult CancelLeaveRequest(int leaveRequestId)
        {
            var leaveRequestToCancel = new LeaveRequest();
            try
            {
                var userId = Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                leaveRequestToCancel = db.LeaveRequests.First(x => x.LeaveRequestId == leaveRequestId && x.UserId == (int)userId);
                ViewBag.LeaveTypeList = new SelectList(db.LeaveTypes.ToList(), "LeaveTypeId", "Name", leaveRequestToCancel.LeaveTypeId);
                ViewBag.LeaveStatusList = new SelectList(db.Master_LeaveStatus.ToList(), "LeaveStatusId", "Status", leaveRequestToCancel.LeaveStatusId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveRequestToCancel);
        }

        [HttpPost]
        public ActionResult CancelLeaveRequest(int leaveRequestId, int submittingUserId)
        {
            try
            {
                var userId = Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestToCancel = db.LeaveRequests.First(x => x.LeaveRequestId == leaveRequestId && x.UserId == (int)userId);
                if (leaveRequestToCancel != null)
                {
                    leaveRequestToCancel.LeaveStatusId = Convert.ToByte(MasterEnum.LeaveStatus.Cancelled.GetHashCode());
                    db.SaveChanges();
                    return Json(new { Result = "Success" });
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

        public ActionResult ViewCalendarYear()
        {
            var calendarYears = CalendarYearRepository.GetCalendarYears();
            return View(calendarYears);
        }

        public ActionResult CalendarYear()
        {
            var calendarYears = CalendarYearRepository.GetCalendarYears();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_CalendarYear", calendarYears);
            }
            return View(calendarYears);
        }

        public ActionResult EditCalendar(int calendarYearId)
        {
            var calendarYear = new CalendarYear();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                calendarYear = db.CalendarYears.First(item => item.CalendarYearId == calendarYearId);

                var monthList = Enum.GetValues(typeof(Month))
                                                .Cast<Month>()
                                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() });

                ViewBag.MonthList = new SelectList(monthList, "Value", "Text");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(calendarYear);
        }

        [HttpPost]
        public ActionResult EditCalendar(CalendarYear calendarYear)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {
                        var calendarYearToUpdate = db.CalendarYears.First(item => item.CalendarYearId == calendarYear.CalendarYearId);
                        UpdateModel(calendarYearToUpdate);
                        db.SaveChanges();
                    }
                    return Json(new { Result = "Success" });
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(calendarYear);
        }

        [HttpGet]
        public ActionResult ViewHoliday()
        {
            var userholiday = new List<AddHoliday>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int LeaveUserId = (int)Session["UserId"];
                var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();
                //   var holidays = db..OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ToList();
                userholiday = db.AddHolidays.Where(x => x.ZoneId == UserRegionId && x.Isactive == true).OrderByDescending(x => x.Date.Value.Year).ThenBy(x => x.Date.Value.Month).ThenBy(x => x.Date.Value.Day).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(userholiday);
        }

        [ValidateInput(false)]
        public ActionResult Holiday()
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<AddHoliday> holidays = null;
            // var holidays = db.AddHolidays.Where(x => x.Isactive == true).OrderBy(x => x.Date).ToList();
            try
            {


                //DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                holidays = db.AddHolidays.Where(x => x.Isactive == true).OrderBy(x => x.Date).ToList();


                if (Request.IsAjaxRequest())
                {
                    return PartialView("_Holiday", holidays);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(holidays);
        }

        [ValidateInput(false)]
        public ViewResult CreateHoliday()
        {
            return View(new Master_holiday());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateHoliday(Master_holiday holiday)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string pattern = @"^[a-zA-Z][a-zA-Z0-9\s]*$";
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);
                    bool result = r.IsMatch(holiday.Detail);
                    if (result)
                    {
                        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                        var duplicate = db.Master_holiday.Where(x => x.Date.Value.Year == DateTime.Today.Year).Any(x => holiday.Detail.Replace(" ", "").Equals(x.Detail.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                        if (!duplicate)
                        {
                            db.Master_holiday.AddObject(new Master_holiday() { Date = holiday.Date, Detail = holiday.Detail });
                            db.SaveChanges();
                            return Json(new { Result = "Success" });
                        }
                        else
                        {
                            return Json(new { Result = "Fail" });
                        }
                    }
                }
                ModelState.AddModelError("Detail", "Detail Invalid");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(holiday);
        }

        [ValidateInput(false)]
        public ActionResult EditHoliday(int holidayId)
        {
            var holiday = new Master_holiday();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                holiday = db.Master_holiday.First(item => item.HolidayId == holidayId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(holiday);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditHoliday(Master_holiday holiday)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string pattern = @"^[a-zA-Z][a-zA-Z0-9\s]*$";
                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);
                    bool result = r.IsMatch(holiday.Detail);
                    if (result)
                    {
                        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                        if (!db.Master_holiday.Where(x => x.Date == holiday.Date && x.HolidayId == holiday.HolidayId).Any(x => holiday.Detail.Replace(" ", "").Equals(x.Detail.Replace(" ", ""), StringComparison.OrdinalIgnoreCase)))
                        {
                            var duplicate = db.Master_holiday.Where(x => x.Date.Value.Year == DateTime.Today.Year).Any(x => holiday.Detail.Replace(" ", "").Equals(x.Detail.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                            if (!duplicate)
                            {
                                var holidayToUpdate = db.Master_holiday.First(item => item.HolidayId == holiday.HolidayId);
                                UpdateModel(holidayToUpdate);
                                db.SaveChanges();

                                return Json(new { Result = "Success" });
                            }
                            else
                            {
                                return Json(new { Result = "Fail" });
                            }
                        }
                        else
                        {
                            return Json(new { Result = "Success" });
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
            return View(holiday);
        }

        public ActionResult DeleteHoliday(int holidayId)
        {
            var holiday = new Master_holiday();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                holiday = db.Master_holiday.First(item => item.HolidayId == holidayId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(holiday);
        }

        [HttpPost]
        public ActionResult DeleteHoliday(int holidayId, int submittingUserId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var holidayToDelete = db.Master_holiday.First(item => item.HolidayId == holidayId);
                db.DeleteObject(holidayToDelete);
                db.SaveChanges();
                return Json(new { Result = "Success" });
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json(new { Result = "Failure", Message = "Error occured while processing the request" });
            }
        }

        public ActionResult Department()
        {
            var Departmentypes = new List<Department>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Departmentypes = db.Departments.ToList();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_LeaveType", Departmentypes);
            }
            return View(Departmentypes);
        }

        public ActionResult LeaveType()
        {

            List<LeaveType> LeaveType = null;

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                LeaveType = db.LeaveTypes.ToList();

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LeaveType", LeaveType);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(LeaveType);
        }

        public ViewResult CreateLeaveType()
        {
            return View(new LeaveType());
        }

        [HttpPost]
        public ActionResult CreateLeaveType(LeaveType leaveType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    db.LeaveTypes.AddObject(new LeaveType() { Name = leaveType.Name, DaysAllowed = leaveType.DaysAllowed, CalculateLeaveDays = leaveType.CalculateLeaveDays, ApplicableEmployees = leaveType.ApplicableEmployees });
                    db.SaveChanges();
                    return Json(new { Result = "Success" });
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveType);
        }

        public ActionResult EditLeaveType(int leaveTypeId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveType = db.LeaveTypes.First(item => item.LeaveTypeId == leaveTypeId);
                return View(leaveType);
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
        public ActionResult EditLeaveType(LeaveType leaveType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                    var leaveTypeToUpdate = db.LeaveTypes.First(item => item.LeaveTypeId == leaveType.LeaveTypeId);

                    if (TryUpdateModel(leaveTypeToUpdate))
                    {
                        db.SaveChanges();
                        return Json(new { Result = "Success" });
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(leaveType);
        }

        public ActionResult DeleteLeaveType(int leaveTypeId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var holiday = db.LeaveTypes.First(item => item.LeaveTypeId == leaveTypeId);
                return View(holiday);
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
        public ActionResult DeleteLeaveType(int leaveTypeId, int submittingUserId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveTypeToDelete = db.LeaveTypes.First(item => item.LeaveTypeId == leaveTypeId);
                db.DeleteObject(leaveTypeToDelete);
                db.SaveChanges();
                return Json(new { Result = "Success" });
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json(new { Result = "Failure", Message = "Error occured while processing the request" });
            }
        }


        public ActionResult LeaveBalance(List<int> years, int userId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                if (years == null || years.Count == 0)
                {
                    years = new List<int>();
                    for (int year = DateTime.Now.Year - 1; year <= DateTime.Now.Year; year++)
                    {
                        years.Add(year);
                    }
                }

                var doj = db.Users.Where(x => x.IsActive == true).Where(x => x.IsActive == true).FirstOrDefault(item => item.UserID == userId).DateOfJoin;

                bool isEligible;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;

                    if (completedDays < 365)
                        isEligible = false;
                    else
                        isEligible = true;
                }

                if (isEligible)
                    ViewBag.isEligible = true;
                else
                    ViewBag.isEligible = false;

                var User = db.Users.Where(x => x.IsActive == true).FirstOrDefault(o => o.UserID == userId);
                int count = 0;

                if (User.Gender == MasterEnum.Genders.Female.GetHashCode() && User.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    var k = MasterEnum.LeaveTypes.Maternity.GetHashCode();
                    var j = MasterEnum.LeaveStatus.Approved.GetHashCode();

                    count = db.LeaveRequests.Where(o => o.LeaveTypeId == k && o.UserId == userId && o.LeaveStatusId == j).Count();
                    ViewBag.MaternityCount = count;

                    var DaysAllowed = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == k).DaysAllowed;

                    if (count == 1)
                    {
                        ViewBag.MaternityUsedDays = DaysAllowed / 2;
                        ViewBag.RemainingDays = DaysAllowed - ViewBag.MaternityUsedDays;
                    }
                    else if (count == 2)
                    {
                        ViewBag.MaternityUsedDays = DaysAllowed;
                        ViewBag.RemainingDays = DaysAllowed - ViewBag.MaternityUsedDays;
                    }
                    else if (count == 0)
                    {
                        ViewBag.MaternityUsedDays = 0;
                        ViewBag.RemainingDays = DaysAllowed;
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(GetLeaveBalance(DateTime.Now.Year, userId));
        }


        

        [HttpGet]
        public ActionResult ApplyLeave()
        {

            try
            {
                var userId = (int)Session["UserId"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                ViewBag.LeaveStatusList = new SelectList(new[] { new Master_LeaveStatus() { LeaveStatusId = 0, Status = "---Select---" } }.Union(db.Master_LeaveStatus.ToList()), "LeaveStatusId", "Status", 0);

                var leaveRequestQuery = from leaveRequests in db.LeaveRequests
                                        where leaveRequests.UserId == userId
                                        orderby leaveRequests.StartDateTime descending
                                        select leaveRequests;
                var leaveRequestsData = GetLeaveRequestsQuery(leaveRequestQuery, null, null).ToList();

                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
                ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
                ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
                ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
                ViewBag.anyPendingLeaveRequests = leaveRequestsData.Any(item => item.LeaveStatusId == db.Master_LeaveStatus.First(i => i.Status.Equals("Pending", StringComparison.InvariantCultureIgnoreCase)).LeaveStatusId);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LeaveRequests", leaveRequestsData);
                }

                ViewBag.isUnderNoticePeriod = (from u in db.Users.Where(u => u.UserID == userId && u.IsActive == true) select u.IsUnderNoticePeriod).FirstOrDefault();
                return View(leaveRequestsData);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        
        public ActionResult MyLeaveBalance()
        {
            var thisUserId = (int)Session["UserId"];
            int currentYear = DateTime.Today.Year;
            return RedirectToActionPermanent("LeaveBalance", new { years = new List<int>() { currentYear }, userId = thisUserId });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult TaskType(int phid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> tasktypelist = new List<SelectListItem>();
            int userId = (int)Session["UserId"];

            tasktypelist = (from p in db.Users.Where(x => x.UserID == phid && x.IsActive == true)
                            join t in db.Departments on p.DepartmentId equals t.DepartmentId
                            join v in db.DepartmentGroups on p.DepartmentGroup equals v.GroupID
                            select new
                            {
                                DepartmentId = p.DepartmentId,
                                DepartmentName = t.DepartmentName
                            }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });

            return Json(new SelectList(tasktypelist, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult FillCity(string state)
        {
            int stateid = Convert.ToInt32(state);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var cities = (from p in db.Users.Where(x => x.UserID == stateid && x.IsActive == true)
                          join t in db.Departments on p.DepartmentId equals t.DepartmentId
                          select new
                          {
                              DepartmentId = p.DepartmentId,
                              DepartmentName = t.DepartmentName

                          }).FirstOrDefault();

            return Json(cities, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]

        public ActionResult DataBind(string ProjectId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int ID = Convert.ToInt32(ProjectId);
            int? department = db.Users.Where(x => x.UserID == ID).Select(o => o.DepartmentId).FirstOrDefault();
            string Department = Convert.ToString(department);
            string Userid = ProjectId;
            ProjectId = Department;

            //  return Json(ProjectId,Userid,JsonRequestBehavior.AllowGet);

            return Json(new { Project = ProjectId, Uservalue = Userid, JsonRequestBehavior.AllowGet });

            // return RedirectToAction("EmployeeLeaveDetails", "Leave", new { Project = ProjectId, Uservalue=Userid });
        }


        [HttpGet]
        public ActionResult EmployeeLeaveDetails(string UserId)
        {
            Session["Start"] = "";
            Session["End"] = "";
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            var thisUserId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);

            //var startDateTime = calendarModel.StartDate;
            //var endDateTime = calendarModel.EndDate;

// added 26/9
            var startDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");

           

            var Department = db.Departments.Where(o => o.IsActive == true && o.BranchID == 1).Select(c => new
            {
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentName
            }).OrderBy(x=>x.DepartmentName).ToList();
            ViewBag.DepartmentList = new SelectList(Department, "DepartmentId", "DepartmentName");


            var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
            {
                LeaveTypeId = l.LeaveTypeId,
                Name = l.Name
            }).ToList();
            ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", thisUserId);

            var monthList = Enum.GetValues(typeof(Month))
                                            .Cast<Month>()
                                            .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList(); ;

            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(monthList, "Value", "Text", 0);



            //var leaveRequestQuery = from leaveRequests in db.LeaveRequests
            //                        where leaveRequests.LeaveStatusId == 2                                   
            //                        orderby leaveRequests.StartDateTime descending
            //                        select leaveRequests;

            int Hrrole = MasterEnum.Roles.HR.GetHashCode();

            int role = db.UserRoles.Where(x => x.UserID == thisUserId).Select(x => x.RoleID).FirstOrDefault();

            if (role == Hrrole)
            {
                if (thisUserId != 0)
                {
                    int Uservalue = Convert.ToInt32(thisUserId);
                    var User = (from p in db.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                                select new
                                {
                                    Id = p.UserID,
                                    UserName = p.FirstName + "" + p.LastName
                                }).ToList();


                    ViewBag.User = new SelectList(User, "Id", "UserName", Uservalue);
                }
                else
                {
                    var User = (from p in db.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                                select new
                                {
                                    Id = p.UserID,
                                    UserName = p.FirstName + "" + p.LastName
                                }).ToList();
                    ViewBag.User = new SelectList(User, "Id", "UserName");
                }

            }
            else
            {
                if (thisUserId != 0)
                {
                    int Uservalue = Convert.ToInt32(thisUserId);
                    var User = (from p in db.UserReportings.Where(x => x.ReportingUserID == thisUserId)
                                join t in db.Users on p.UserID equals t.UserID
                                where t.IsActive == true && t.UserStatus != 6
                                select new
                                {

                                    Id = t.UserID,
                                    UserName = t.FirstName + "" + t.LastName

                                }).ToList();
                    ViewBag.User = new SelectList(User, "Id", "UserName", Uservalue);
                }
                else
                {
                    var User = (from p in db.UserReportings.Where(x => x.ReportingUserID == thisUserId)
                                join t in db.Users on p.UserID equals t.UserID
                                where t.IsActive == true && t.UserStatus != 6
                                select new
                                {

                                    Id = p.UserID,
                                    UserName = t.FirstName + "" + t.LastName

                                }).ToList();
                    ViewBag.User = new SelectList(User, "Id", "UserName");
                }

            }



            //var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
            //                      select new
            //                      {
            //                          GroupId = p.GroupID,
            //                          GroupName = p.GroupName
            //                      }).ToList();


            if (thisUserId != 0)
            {
                int Group = Convert.ToInt32(UserId);
                var groupvalue = db.Users.Where(x => x.UserID == Group).Select(o => o.DepartmentGroup).FirstOrDefault();

                //var gropup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                //              select new
                //              {
                //                  GroupID = p.GroupID,
                //                  GroupName = p.GroupName
                //              }).OrderBy(o=>o.GroupName).ToList();

                ViewBag.Group = new SelectList("", "GroupID", "GroupName");

                //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", groupvalue);

                //ViewBag.Group = "";
            }
            else
            {

                //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
                //var gropup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                //              select new
                //              {
                //                  GroupID = p.GroupID,
                //                  GroupName = p.GroupName
                //              }).ToList();

                //ViewBag.Group = new SelectList(gropup, "GroupID", "GroupName");
                ViewBag.Group = new SelectList("", "GroupID", "GroupName");
            }
            //  ViewBag.User = new SelectList(User, "Id", "UserName");

            if (role == Hrrole)
            {

                var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                        join leave in db.LeaveRequests
                                        on users.UserID equals leave.UserId
                                        join depart in db.Departments
                                        on users.DepartmentId equals depart.DepartmentId
                                        where leave.LeaveStatusId == 2
                                        orderby leave.StartDateTime descending
                                        select leave;

                var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
                return View(leaveRequestsData);
            }

            else
            {
                // old code
                //var leaveRequestQuery = from p in db.UserReportings.Where(x => x.ReportingUserID == thisUserId)
                //                        join t in db.Users on p.UserID equals t.UserID
                //                        join leave in db.LeaveRequests
                //                        on t.UserID equals leave.UserId
                //                        join depart in db.Departments
                //                        on t.DepartmentId equals depart.DepartmentId
                //                        where leave.LeaveStatusId == 2 && t.IsActive == true
                //                        orderby t.UserID descending
                //                        select leave;

                // updated on 27/9
                var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                        join leave in db.LeaveRequests
                                        on users.UserID equals leave.UserId
                                        join depart in db.Departments
                                        on users.DepartmentId equals depart.DepartmentId
                                        where leave.LeaveStatusId == 2
                                        orderby leave.StartDateTime descending
                                        select leave;


                var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
                return View(leaveRequestsData);
            }

        }

        [ActionName("EmployeeLeaveDetailsForAMonth")]
        public ActionResult EmployeeLeaveDetailsFour(int leaveType, int Department, DateTime startDateTime, DateTime endDateTime,int Branch)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           
            int GroupID = 0;
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupID);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsFive(int leaveType, int Department, DateTime startDateTime, DateTime endDateTime, int Departmentgroup,int Branch)
        {
            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && dgm.DepartmentID == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
                                    orderby users.UserID descending
                                    select leave;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailseven(int leaveType, int Department, DateTime startDateTime, DateTime endDateTime,int Branch)
        {
            
            int Departmentgroup = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var Uservalue = (from p in db.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = p.UserID,
                                 UserName = p.FirstName + "" + p.LastName
                             }).Distinct().ToList();


            ViewBag.User = new SelectList(Uservalue, "Id", "UserName");


           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            //added 22/9/16
            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);

            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && users.IsActive == true
            //                        orderby users.UserID descending
            //                        select leave;

           //added on 22/9/2016

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    //join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;
          
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsSix(int leaveType, int Department, DateTime startDateTime, DateTime endDateTime, int Departmentgroup, int branchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
          //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

          



            //DateTime d2 = Convert.ToDateTime(endDateTime);
            //string d3 = d2.ToShortDateString();
            //taskobj.End = d3;

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();




            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }

            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Departmentgroup != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }

            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }


           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Departmentgroup);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }



// added for dept,branch,group,starttime,leavetype

        //public ActionResult EmployeeLeaveDetailsSixNew(int leaveType, int Department, DateTime startDateTime,int Departmentgroup, int branchId)
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
        //    //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);





        //    //DateTime d2 = Convert.ToDateTime(endDateTime);
        //    //string d3 = d2.ToShortDateString();
        //    //taskobj.End = d3;
        //    //var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
        //    //var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
        //    //calendarDetails.EndingMonth ?? 12);
        //    //DateTime endDateTime = Convert.ToDateTime(endDateTime);

        //    var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
        //                          select new
        //                          {
        //                              GroupId = p.GroupID,
        //                              GroupName = p.GroupName
        //                          }).ToList();

        //    var User = (from p in db.Users.Where(x => x.IsActive == true)
        //                select new
        //                {
        //                    Id = p.UserID,
        //                    UserName = p.FirstName + "" + p.LastName
        //                }).Distinct().ToList();




        //    ViewBag.User = new SelectList(User, "Id", "UserName");


        //    if (branchId != 0)
        //    {
        //        var BranchList = (from d in db.Master_Branches
        //                          select new DSRCEmployees
        //                          {
        //                              Name = d.BranchName,
        //                              BranchID = d.BranchID,

        //                          }).ToList();
        //        ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
        //    }
        //    else
        //    {
        //        var BranchList = (from d in db.Master_Branches
        //                          select new DSRCEmployees
        //                          {
        //                              Name = d.BranchName,
        //                              BranchID = d.BranchID,

        //                          }).ToList();
        //        ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
        //    }
        //    if (leaveType != 0)
        //    {
        //        var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
        //        {
        //            LeaveTypeId = l.LeaveTypeId,
        //            Name = l.Name
        //        }).ToList();
        //        ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
        //    }
        //    else
        //    {
        //        var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
        //        {
        //            LeaveTypeId = l.LeaveTypeId,
        //            Name = l.Name
        //        }).ToList();
        //        ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
        //    }

        //    if (Department != 0)
        //    {
        //        var DepartmentIdList = (from d in db.Departments
        //                                where d.IsActive == true && d.BranchID == branchId
        //                                select new DSRCEmployees
        //                                {
        //                                    Name = d.DepartmentName,
        //                                    DepartmentId = d.DepartmentId,

        //                                }).ToList();
        //        ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

        //    }
        //    else
        //    {
        //        var DepartmentIdList = (from d in db.Departments
        //                                where d.IsActive == true
        //                                select new DSRCEmployees
        //                                {
        //                                    Name = d.DepartmentName,
        //                                    DepartmentId = d.DepartmentId,

        //                                }).ToList();
        //        ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
        //    }



        //    if (Departmentgroup != 0)
        //    {
        //        var Group = (from d in db.Departments
        //                     join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
        //                     join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
        //                     where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
        //                     select new DSRCEmployees
        //                     {
        //                         Name = dg.GroupName,
        //                         GroupID = dg.GroupID,

        //                     }).ToList();
        //        ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
        //    }

        //    else
        //    {
        //        var Group = (from d in db.Departments
        //                     join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
        //                     join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
        //                     where d.IsActive == true && dg.IsActive == true
        //                     select new DSRCEmployees
        //                     {
        //                         Name = dg.GroupName,
        //                         GroupID = dg.GroupID,

        //                     }).ToList();
        //        ViewBag.Group = new SelectList(Group, "GroupID", "Name");
        //    }


        //    // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Departmentgroup);

        //    var monthList = Enum.GetValues(typeof(Month))
        //                        .Cast<Month>()
        //                        .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
        //    monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
        //    ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

        //    var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
        //                            join leave in db.LeaveRequests on users.UserID equals leave.UserId
        //                            join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
        //                            join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
        //                            where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
        //                            orderby users.UserID descending
        //                            select leave;

        //    var calendarDetails = db.CalendarYears.FirstOrDefault();
        //    var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
        //    var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
        //        calendarDetails.EndingMonth ?? 12);
        //    var endDateTime = calendarModel.EndDate;

        //    //  var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime,branchId).ToList();
        ////    var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery,null,startDateTime,endDateTime).ToList();

        //    return View("EmployeeLeaveDetails", leaveRequestQuery);
        //}



        public ActionResult EmployeeLeaveDetailsBranchDeptGroupSDate(int branchId,int Department,int Departmentgroup,int leaveType,DateTime startDateTime )
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);





            //DateTime d2 = Convert.ToDateTime(endDateTime);
            //string d3 = d2.ToShortDateString();
            //taskobj.End = d3;

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();




            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }

            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Departmentgroup != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && dgm.DepartmentID == Department && dgm.GroupID == Departmentgroup
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }

            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }


            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Departmentgroup);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime,null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsFour(int leaveType, int Department, int Departmentgroup, int branchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

           

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Departmentgroup);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Departmentgroup != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null,null,null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }


        public ActionResult EmployeeLeaveDetailsTwelth(int leaveType, int Department, DateTime startDateTime, int GroupId, int Branch)
        {
           
          //  int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name",GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == GroupId
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBrachDeptGroupSdate(int branchId, int Department, DateTime startDateTime, int GroupId)
        {
            //int branchId = 0;
            //int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where  depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == GroupId
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsBrachDeptGroupSdateEdate(int branchId, int Department,int GroupId, DateTime startDateTime,DateTime endDateTime)
        {
            //int branchId = 0;
            //int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID ==GroupId
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        
        public ActionResult EmployeeLeaveDetailsBranchSDate(int branchId, DateTime startDateTime)
        {
           // int branchId = 0;
            int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "UserId", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

  // old code
            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && users.BranchId == branchId
            //                        orderby users.UserID descending
            //                        select leave;

            // added on 26/9

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    //join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && users.BranchId == branchId
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsBranchEDate(int branchId, DateTime endDateTime)
        {
            // int branchId = 0;
            int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && users.BranchId == branchId
            //                        orderby users.UserID descending
            //                        select leave;

            // Added on 26/9
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                            //        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && users.BranchId == branchId
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsGroupSDate(int GroupId, DateTime startDateTime, int Branch)
        {
             
            //int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID ==GroupId 
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsGroupEDate(int GroupId, DateTime endDateTime, int Branch)
        {
           
            //int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == GroupId
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, null, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsLeaveSDate(int leaveType, DateTime startDateTime, int Branch)
        {
            // int branchId = 0;
            int GroupId = 0;
           
           // int leaveType = 0;
            int Department = 0;
           // int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees 
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2 && depgroup.IsActive == true && leave.StartDateTime >= startDateTime
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsLeaveEDate(int leaveType, DateTime EndDateTime, int Branch)
        {
            // int branchId = 0;
            int GroupId = 0;
          
            // int leaveType = 0;
            int Department = 0;
            // int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2 && depgroup.IsActive == true && leave.EndDateTime <= EndDateTime
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, null, EndDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsBranchSDateEDate(int branchId, DateTime startDateTime, DateTime endDateTime)
        {
            // int branchId = 0;
            int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
// old code
            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && users.BranchId == branchId
            //                        orderby users.UserID descending
            //                        select leave;

            //updated on added on 22/9/2016 
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    //join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && users.BranchId == branchId
                                    orderby leave.StartDateTime descending
                                    select leave;

               

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsGroupSDateEDate(int GroupId, DateTime startDateTime, DateTime endDateTime, int Branch)
        {
            
            //int GroupId = 0;
            int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId== Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == GroupId
                                    orderby users.UserID descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranchDeptSDate(int branchId, int Department, DateTime startDateTime)
        {
            // int branchId = 0;
            int GroupId = 0;
            int leaveType = 0;
            //int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

// old code
            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depart.DepartmentId == Department
            //                        orderby users.UserID descending
            //                        select leave;

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                   // join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.LeaveStatusId == 2 && depart.DepartmentId == Department
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null,startDateTime,null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranchDeptSDateEDate(int branchId, int Department, DateTime startDateTime,DateTime endDateTime)
        {
            // int branchId = 0;
            int GroupId = 0;
            int leaveType = 0;
            //int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depart.DepartmentId == Department
            //                        orderby users.UserID descending
            //                        select leave;

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    //join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2  && depart.DepartmentId == Department
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranchLeaveSDateEDate(int branchId, int leaveType, DateTime startDateTime, DateTime endDateTime)
        {
            // int branchId = 0;
            int GroupId = 0;
           // int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (branchId != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", branchId);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests on users.UserID equals leave.UserId
            //                        join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
            //                        join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
            //                        where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depart.BranchID == branchId
            //                        orderby users.UserID descending
            //                        select leave;

            //added on 22/9/2016
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    //join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2 && users.BranchId == branchId && leave.LeaveTypeId == leaveType
                                    orderby leave.StartDateTime descending
                                    select leave;


            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsThirteen(int leaveType, int Department, DateTime EndDateTime, int Departmentgroup, int Branch)
        {
           
            int GroupID=0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "All Department" } }.Union(db.Departments.Where(o => o.IsActive == true && o.BranchID == Branch).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Departmentgroup);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }
           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where leave.LeaveTypeId == leaveType && depart.DepartmentId == Department && leave.EndDateTime <= EndDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true && depgroup.GroupID == Departmentgroup
                                    orderby leave.StartDateTime descending
                                    select leave;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, EndDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsfourteen(int Department, int Group, DateTime StartDateTime, DateTime EndDateTime, int Branch)
        {
            
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
          //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var GroupList = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(GroupList, "GroupID", "Name", Group);
            }
            else
            {
                var GroupList = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(GroupList, "GroupID", "Name");
            }

            //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Group);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where depgroup.GroupID == Group && depart.DepartmentId == Department && leave.EndDateTime <= EndDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true
                                    orderby users.UserID descending
                                    select leave;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, StartDateTime, EndDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsGroupIdLeaveSDateEDate(int Group, int leaveType, DateTime StartDateTime, DateTime EndDateTime, int Branch)
        {
           
            //int leaveType = 0;
            int Department = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name = "All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var GroupList = (from d in db.Departments
                                 join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                 join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                 where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                                 select new DSRCEmployees
                                 {
                                     Name = dg.GroupName,
                                     GroupID = dg.GroupID,

                                 }).ToList();
                ViewBag.Group = new SelectList(GroupList, "GroupID", "Name", Group);
            }
            else
            {
                var GroupList = (from d in db.Departments
                                 join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                 join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                 where d.IsActive == true && dg.IsActive == true
                                 select new DSRCEmployees
                                 {
                                     Name = dg.GroupName,
                                     GroupID = dg.GroupID,

                                 }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", Group);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests on users.UserID equals leave.UserId
                                    join depart in db.Departments on users.DepartmentId equals depart.DepartmentId
                                    join depgroup in db.DepartmentGroups on users.DepartmentGroup equals depgroup.GroupID
                                    where depgroup.GroupID == Group &&leave.StartDateTime>= StartDateTime &&leave.EndDateTime <= EndDateTime && leave.LeaveStatusId == 2 && depgroup.IsActive == true
                                    orderby users.UserID descending
                                    select leave;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, StartDateTime, EndDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }


        public ActionResult EmployeeLeaveDetailsTenth(int userid)
        {
            int branchId = 0;
            int Department = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name  ="All LeaveType"} }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName  ="All Department"} }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName", userid);

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (branchId != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == branchId
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", Group);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.LeaveStatusId == 2 && leave.UserId == userid
                                    orderby users.UserID descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();



            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsEleventh(DateTime startDateTime, DateTime endDateTime, int Branch)
        {
            
            int Department = 0;
            int GroupId =0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;
            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);


            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();



            return View("EmployeeLeaveDetails", leaveRequestsData);
        }




        public ActionResult EmployeeLeaveDetailsThree(int leaveType, DateTime startDateTime, DateTime endDateTime, int Branch)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            
            int Department = 0;
            int GroupId = 0;
            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.LeaveTypeId == leaveType && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;




            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsThree1(int Department, DateTime startDateTime, DateTime endDateTime, int Branch)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           
            int GroupId = 0;
            int leaveType = 0;
            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name  ="All LeaveType"} }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;



            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsNine(int Department, DateTime startDateTime, DateTime endDateTime, int Branch)
        {
           
            int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);


           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName  ="All Department"} }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            var BranchList = (from d in db.Master_Branches
                              select new DSRCEmployees
                              {
                                  Name = d.BranchName,
                                  BranchID = d.BranchID,

                              }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsfifteen(int Department, int GruopId, DateTime StartDateTime,int Branch)
        {
           
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", GruopId);


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings 
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.StartDateTime >= StartDateTime && leave.LeaveStatusId == 2 && gp.GroupID == GruopId
                                    orderby users.UserID descending
                                    select leave;


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GruopId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, StartDateTime, null).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsDeptGroupLeave(int Department, int GruopId, int leaveType, int Branch)
        {
            
           // int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", GruopId);


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.LeaveStatusId == 2 && gp.GroupID == GruopId    
                                    orderby leave.StartDateTime descending
                                    select leave;


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GruopId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, leaveType, DateTime.MinValue, DateTime.MaxValue).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsGroupLeaveSDate(int GruopId, int leaveType, DateTime StartDateTime, int Branch)
        {
           
            int Department = 0;
            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", GruopId);


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.StartDateTime >= StartDateTime && leave.LeaveStatusId == 2 && gp.GroupID == GruopId && leave.LeaveTypeId == leaveType
                                    orderby leave.StartDateTime descending
                                    select leave;


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GruopId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, StartDateTime, null).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsGroupLeaveEDate(int GruopId, int leaveType, DateTime EndDateTime , int Branch)
        {
          
            int Department = 0;

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", GruopId);


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.EndDateTime <= EndDateTime && leave.LeaveStatusId == 2 && gp.GroupID == GruopId && leave.LeaveTypeId == leaveType
                                    orderby users.UserID descending
                                    select leave;


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GruopId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, null, EndDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailssixteen(int Department, int GruopId, DateTime EndDateTime, int Branch)
        {
          
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0 } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", GruopId);


           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);


           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            // old code
            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests
            //                        on users.UserID equals leave.UserId
            //                        join depart in db.Departments
            //                        on users.DepartmentId equals depart.DepartmentId
            //                        where depart.DepartmentId == Department && leave.LeaveStatusId == 2 && leave.EndDateTime <= EndDateTime
            //                        orderby leave.StartDateTime descending
            //                        select leave;


            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.LeaveStatusId == 2 && gp.GroupID == GruopId
                                    orderby leave.StartDateTime descending
                                    select leave;

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GruopId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, EndDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsThreeValues(int Department, int BranchID, int GroupId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            //int BranchID = 0;
         //   int GroupId = 0;
            int leaveType = 0;
            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);



            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            //  ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name  ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var Uservalue = (from p in db.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = p.UserID,
                                 UserName = p.FirstName + "" + p.LastName
                             }).Distinct().ToList();


            ViewBag.User = new SelectList(Uservalue, "Id", "UserName", User);


            if (BranchID != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", BranchID);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }


            //var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
            //                        join leave in db.LeaveRequests
            //                        on users.UserID equals leave.UserId
            //                        join depart in db.Departments
            //                        on users.DepartmentId equals depart.DepartmentId
            //                        where depart.DepartmentId == Department && leave.LeaveStatusId == 2
            //                        orderby leave.StartDateTime descending
            //                        select leave;

            // updated 
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    join gp in db.DepartmentGroupMappings
                                    on users.DepartmentGroup equals gp.GroupID
                                    where depart.DepartmentId == Department && leave.LeaveStatusId == 2 && gp.GroupID == GroupId
                                    orderby leave.StartDateTime descending
                                    select leave;


            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);

            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, DateTime.MaxValue).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }


        public ActionResult EmployeeLeaveDetailsEight(int Department, int BranchID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            //int BranchID = 0;
            int GroupId = 0;
            int leaveType = 0;
           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);



           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

          //  ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name  ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var Uservalue = (from p in db.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = p.UserID,
                                 UserName = p.FirstName + "" + p.LastName
                             }).Distinct().ToList();


            ViewBag.User = new SelectList(Uservalue, "Id", "UserName", User);


            if (BranchID != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", BranchID);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true 
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
          

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);

            var startDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsDepartandGroup(int Department, int GroupId, int Branch)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            
            
            int leaveType = 0;
            // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);



            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            //  ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name  ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var Uservalue = (from p in db.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = p.UserID,
                                 UserName = p.FirstName + "" + p.LastName
                             }).Distinct().ToList();


            ViewBag.User = new SelectList(Uservalue, "Id", "UserName", User);


            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }


            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && users.DepartmentGroup == GroupId && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);

            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsTime(DateTime startDateTime, DateTime endDateTime, int Branch)
        {
           
            int Department = 0;
            int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }

            ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName  ="All Department"} }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);
            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.StartDateTime >= startDateTime && leave.EndDateTime <= endDateTime && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;



            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();
            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsTwo(int Department, int leaveType,int Branch)
        {
           
            int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);
           
           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }


//added my code for branch,dept,enddate,leavetype

        public ActionResult EmployeeLeaveDetailsbranchDeptLeaveEdate(int Department, int leaveType, int Branch, DateTime endDateTime)
        {

            int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {  
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        //added my code for branch,dept,Startdate,leavetype
        public ActionResult EmployeeLeaveDetailsbranchDeptLeaveSdate(int Department, int leaveType, int Branch, DateTime startDateTime)
        {

            int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var endDateTime = calendarModel.EndDate;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsDeptSdate(int Department, DateTime startDate, int Branch)
        {
           
            int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.StartDateTime >= startDate && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, null).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsDeptEdate(int Department, DateTime EndDateTime, int Branch)
        {
           
            int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");
            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }


            //ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName  ="All Group"} }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.EndDateTime <= EndDateTime && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, DateTime.MinValue, EndDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsOne(int Department, int Branch)
        {
           
            int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

          //  ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            //var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
            //                      select new
            //                      {
            //                          GroupId = p.GroupID,
            //                          GroupName = p.GroupName
            //                      }).ToList();

            //ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department 
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }
            
            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.DepartmentId == Department && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;



            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranch(int BranchID)
        {
            int Department = 0;
            int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (BranchID != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name",BranchID);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.BranchID == BranchID && leave.LeaveStatusId == 2
                                    orderby leave.StartDateTime descending
                                    select leave;



            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            // old code
           // var startDateTime = calendarModel.StartDate;
          //  var endDateTime = calendarModel.EndDate;

            var startDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;

            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null,startDateTime,endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranchGroup(int BranchID,int GroupId)
        {
            int Department = 0;
           // int GroupId = 0;
            int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (BranchID != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", BranchID);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (GroupId != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && dg.GroupID == GroupId
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.BranchID == BranchID && leave.LeaveStatusId == 2 && users.DepartmentGroup== GroupId
                                    orderby users.UserID descending
                                    select leave;



            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsBranchLeave(int BranchID, int leaveType)
        {
            int Department = 0;
            int GroupId = 0;
            //int leaveType = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (BranchID != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", BranchID);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Department != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == BranchID
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where depart.BranchID == BranchID && leave.LeaveStatusId == 2 && leave.LeaveTypeId == leaveType
                                    orderby leave.StartDateTime descending
                                    select leave;



            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = DateTime.MinValue;
            var endDateTime = DateTime.MaxValue;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsGroup(int GroupId, int Branch)
        {
            int Department = 0;
            int leaveType = 0;
           
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", Department);

            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();

            ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);
            //ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name ="All LeaveType" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }
            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }



            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where users.DepartmentGroup == GroupId && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;



            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }
        public ActionResult EmployeeLeaveDetailsOne1(int leaveType, int Branch)
        {
           
            int Department = 0;
            int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }



           // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

           // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0,DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


           // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupId);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2
                                    orderby users.UserID descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        public ActionResult EmployeeLeaveDetailsGroupLeaveType(int GroupID, int leaveType, int Branch)
        {
            
            int Department = 0;
          //  int GroupId = 0;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();


            var User = (from p in db.Users.Where(x => x.IsActive == true)
                        select new
                        {
                            Id = p.UserID,
                            UserName = p.FirstName + "" + p.LastName
                        }).Distinct().ToList();


            ViewBag.User = new SelectList(User, "Id", "UserName");

            if (Branch != 0)
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name", Branch);
            }
            else
            {
                var BranchList = (from d in db.Master_Branches
                                  select new DSRCEmployees
                                  {
                                      Name = d.BranchName,
                                      BranchID = d.BranchID,

                                  }).ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");
            }



            // ViewBag.Group = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName ="All Group" } }.Union(db.DepartmentGroups.Where(o => o.IsActive == true).ToList()), "GroupID", "GroupName", 0);

            // ViewBag.DepartmentList = new SelectList(new[] { new Department() { DepartmentId = 0,DepartmentName ="All Department" } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);


            // ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", leaveType);
            var monthList = Enum.GetValues(typeof(Month))
                                .Cast<Month>()
                                .Select(item => new SelectListItem() { Text = item.ToString(), Value = ((int)item).ToString() }).ToList();
            monthList.Insert(0, new SelectListItem { Text = "All", Value = "0" });
            //ViewBag.MonthList = new SelectList(monthList, "Value", "Text", startDateTime.Value.Month);
            ViewBag.MonthList = new SelectList(new[] { new Department() { DepartmentId = 0 } }.Union(db.Departments.Where(o => o.IsActive == true).ToList()), "DepartmentId", "DepartmentName", 0);

            if (leaveType != 0)
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name", leaveType);
            }
            else
            {
                var LeaveType = db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").Select(l => new
                {
                    LeaveTypeId = l.LeaveTypeId,
                    Name = l.Name
                }).ToList();
                ViewBag.LeaveTypeList = new SelectList(LeaveType, "LeaveTypeId", "Name");
            }
            if (Branch != 0)
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true && d.BranchID == Branch
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name", Department);

            }
            else
            {
                var DepartmentIdList = (from d in db.Departments
                                        where d.IsActive == true
                                        select new DSRCEmployees
                                        {
                                            Name = d.DepartmentName,
                                            DepartmentId = d.DepartmentId,

                                        }).ToList();
                ViewBag.DepartmentList = new SelectList(DepartmentIdList, "DepartmentId", "Name");
            }


            if (Department != 0)
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == Department
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList(Group, "GroupID", "Name", GroupID);
            }
            else
            {
                var Group = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 GroupID = dg.GroupID,

                             }).ToList();
                ViewBag.Group = new SelectList("", "GroupID", "Name");
            }

            var leaveRequestQuery = from users in db.Users.Where(x => x.IsActive == true)
                                    join leave in db.LeaveRequests
                                    on users.UserID equals leave.UserId
                                    join depart in db.Departments
                                    on users.DepartmentId equals depart.DepartmentId
                                    where leave.LeaveTypeId == leaveType && leave.LeaveStatusId == 2 && users.DepartmentGroup== GroupID
                                    orderby users.UserID descending
                                    select leave;

            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var year = DateTime.Now.Month <= calendarDetails.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
            var startDateTime = calendarModel.StartDate;
            var endDateTime = calendarModel.EndDate;
            var leaveRequestsData = GetLeaveDetailsQuery(leaveRequestQuery, null, startDateTime, endDateTime).ToList();

            return View("EmployeeLeaveDetails", leaveRequestsData);
        }

        //[DSRCAuthorize(Roles = "Attendant")]
        //public ActionResult EmployeeLeaveDetails(int leaveType, int month)
        //{
        //    DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
        //    var acadamicEndMonth = dbHrms.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
        //    var year = DateTime.Now.Month <= acadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
        //    if (month == 0 && leaveType != 0)
        //    {
        //        var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
        //        var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
        //        calendarDetails.EndingMonth ?? 12);

        //        var startDateTime = calendarModel.StartDate;
        //        var endDateTime = calendarModel.EndDate;

        //        return EmployeeLeaveDetails(leaveType,startDateTime,endDateTime);
        //    }
        //    else if (month == 0) 
        //    { 
        //        return EmployeeLeaveDetails(); 
        //    }
        //    else 
        //    {                
        //        return EmployeeLeaveDetails(leaveType, new DateTime(year, month, 01), new DateTime(year, (month), 01).AddMonths((1)).AddSeconds((-1))); 
        //    }
        //}


        [HttpPost]
       // public ActionResult EmployeeLeaveDetails(int Branch, int leaveType, int Department, string StartDateTime, string EndDateTime, int GroupID)
      public ActionResult EmployeeLeaveDetails(FormCollection form)
        {
            
            //int Id = 0;
            int Branch = (form["BranchID"] == "") ? 0 : Convert.ToInt16(form["BranchID"]);
            int Department = (form["DepartmentId"] == "") ? 0 : Convert.ToInt16(form["DepartmentId"]);
            int GroupID = (form["GroupID"] == "") ? 0 : Convert.ToInt16(form["GroupID"]);
            int leaveType = (form["LeaveType"] == "") ? 0 : Convert.ToInt16(form["LeaveType"]);
            string StartDateTime = (form["StartDateTime"] == "") ? null : Convert.ToString(form["StartDateTime"]);
            string EndDateTime = (form["EndDateTime"] == "") ? null : Convert.ToString(form["EndDateTime"]);
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            DSRCManagementSystem.Models.LeaveDetails objmodel = new DSRCManagementSystem.Models.LeaveDetails();

            if (StartDateTime != null)
            {
                DateTime d1 = Convert.ToDateTime(StartDateTime);
                string z = d1.ToShortDateString();
                objmodel.startdate = z;
                Session["Start"] = z.ToString();
            }
            else
            {
                objmodel.startdate = "";
                Session["Start"] = "";
            }
            if (EndDateTime != null)
            {
                DateTime d2 = Convert.ToDateTime(EndDateTime);
                string z1 = d2.ToShortDateString();
                objmodel.enddate = z1.ToString();
                Session["End"] = z1.ToString();

            }
            else
            {
                objmodel.enddate = "";
                Session["End"] = "";
            }

         
            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            var acadamicEndMonth = dbHrms.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= acadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            if (Department != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null && GroupID != 0 && Branch != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int usergroup = GroupID;
                //int user = Id;
                return EmployeeLeaveDetailsSix(leaveType, Department, startDateTime, endDateTime, usergroup, Branch);
            }

            if (Department != 0 && leaveType != 0 && GroupID != 0 && Branch != 0 && StartDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                // DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int usergroup = GroupID;
                //int user = Id;
                return EmployeeLeaveDetailsBranchDeptGroupSDate(Branch, Department, usergroup, leaveType, startDateTime);
            }

            else if (leaveType != 0 && Department != 0 && EndDateTime != null && GroupID != 0 && Branch != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                //   DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsThirteen(leaveType, Department, endDateTime, group, Branch);
            }

            //else if (Department != 0 && Branch != 0 && GroupID != 0)
            //{
            //    return EmployeeLeaveDetailsThreeValues(Branch, Department, GroupID);
            //}

            else if (Department != 0 && GroupID != 0 && EndDateTime != null && Branch!=0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                // DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailssixteen(Department, group, endDateTime, Branch);

            }
// added for branch,group,dept,leave,sdate
            //if (Department != 0 && leaveType != 0 && StartDateTime != null && GroupID != 0 && Branch != 0)
            //{
            //    var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
            //    var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
            //    calendarDetails.EndingMonth ?? 12);
            //    DateTime startDateTime = Convert.ToDateTime(StartDateTime);
            //    DateTime endDateTime = Convert.ToDateTime(EndDateTime);
            //    int usergroup = GroupID;
            //    //int user = Id;
            //    return EmployeeLeaveDetailsSixNew(leaveType, Department, startDateTime,usergroup, Branch);
            //}

//added for branch,dept,leave,enddate

            if (Department != 0 && EndDateTime != null && Branch != 0 && leaveType != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int usergroup = GroupID;
                //int user = Id;
                return EmployeeLeaveDetailsbranchDeptLeaveEdate(Department,leaveType,Branch,endDateTime);
            }

            if (Department != 0 && StartDateTime != null && Branch != 0 && leaveType != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int usergroup = GroupID;
                //int user = Id;
                return EmployeeLeaveDetailsbranchDeptLeaveSdate(Department, leaveType, Branch, startDateTime);
            }


          
            else if (Department != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null && GroupID != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;

                return EmployeeLeaveDetailsFive(leaveType, Department, startDateTime, endDateTime, group,Branch);
             }
            else if (Department != 0 && GroupID != 0 && StartDateTime != null && Branch != 0 && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsBrachDeptGroupSdateEdate(Branch, Department, GroupID, startDateTime, endDateTime);
            }
                
            else if(Department != 0 && GroupID != 0 && leaveType != 0 && Branch != 0)
            {
                 var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                int usergroup = GroupID;
                //int user = Id;
                return EmployeeLeaveDetailsFour(leaveType, Department, usergroup, Branch);
            }
            else if (Department != 0 && GroupID != 0 && StartDateTime != null && Branch != 0)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsBrachDeptGroupSdate(Branch, Department, startDateTime, GroupID);
            }
            
            else if (leaveType != 0 && Department != 0 && StartDateTime != null && GroupID != 0)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                //  DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsTwelth(leaveType, Department, startDateTime, group,Branch);

            }
           

            else if (Department != 0 && GroupID != 0 && StartDateTime != null && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsfourteen(Department, group, startDateTime, endDateTime, Branch);
            }
            else if (GroupID != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsGroupIdLeaveSDateEDate(GroupID, leaveType, startDateTime, endDateTime,Branch);
            }

            else if (Department != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                //int user = Id;
                return EmployeeLeaveDetailseven(leaveType, Department, startDateTime, endDateTime,Branch);
            }

            else if (Department != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);

                return EmployeeLeaveDetailsFour(leaveType, Department, startDateTime, endDateTime,Branch);
            }
            else if (Branch != 0 && Department != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsBranchDeptSDateEDate(Branch, Department, startDateTime, endDateTime);
            }
            else if (Branch != 0 && leaveType != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsBranchLeaveSDateEDate(Branch, leaveType, startDateTime, endDateTime);
            }


            else if (Department != 0 && GroupID != 0 && StartDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                //  DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsfifteen(Department, group, startDateTime,Branch);
            }
            else if (Department != 0 && GroupID != 0 && leaveType != null)
            {
                int group = GroupID;
                return EmployeeLeaveDetailsDeptGroupLeave(Department, group, leaveType,Branch);
            }
            else if  (GroupID != 0 &&   leaveType != 0 && StartDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                //  DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsGroupLeaveSDate(group,leaveType,startDateTime,Branch);
            }
            else if (GroupID != 0 && leaveType != 0 && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
               // DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                 DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                int group = GroupID;
                return EmployeeLeaveDetailsGroupLeaveEDate(group, leaveType, endDateTime,Branch);
            }

            

            else if (Department != 0 && StartDateTime != null && EndDateTime != null)
            {
                int Dep = Department;
                //int User = Id;
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsNine(Dep, startDateTime, endDateTime,Branch);

            }
            else if (leaveType != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsThree(leaveType, startDateTime, endDateTime,Branch);
            }
            else if (Department != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsThree1(Department, startDateTime, endDateTime,Branch);
            }

            else if (Branch != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsBranchSDateEDate(Branch, startDateTime, endDateTime);
            }
            else if (Branch != 0 && Department != 0 && StartDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsBranchDeptSDate(Branch, Department, startDateTime);
            }
            else if (GroupID != 0 && StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsGroupSDateEDate(GroupID, startDateTime, endDateTime,Branch);
            }


            else if (StartDateTime != null && EndDateTime != null)
            {
                var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
                var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
                calendarDetails.EndingMonth ?? 12);
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                // int user = Id;
                return EmployeeLeaveDetailsEleventh(startDateTime, endDateTime,Branch);
            }
            else if (Department != 0 && leaveType != 0)
            {
                return EmployeeLeaveDetailsTwo(Department, leaveType,Branch);
            }
            else if (Department != 0 && StartDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsDeptSdate(Department, startDateTime,Branch);
            }
            else if (Department != 0 && EndDateTime != null)
            {
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsDeptEdate(Department, endDateTime,Branch);
            }
          
            else if (Department != 0 && Branch != 0)
            {
                return EmployeeLeaveDetailsEight(Department, Branch);
            }
            else if (Department != 0 && GroupID != 0)
            {
                return EmployeeLeaveDetailsDepartandGroup(Department, GroupID,Branch);
            }
            else if (GroupID != 0 && leaveType != 0)
            {
                return EmployeeLeaveDetailsGroupLeaveType(GroupID, leaveType,Branch);
            }
            else if (Branch != 0 && leaveType != 0)
            {
                return EmployeeLeaveDetailsBranchLeave(Branch, leaveType);
            }
            else if (StartDateTime != null && EndDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsTime(startDateTime, endDateTime,Branch);
            }
            else if (Branch != 0 && GroupID != 0)
            {
                return EmployeeLeaveDetailsBranchGroup(Branch, GroupID);
            }
            else if (Branch != 0 && StartDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsBranchSDate(Branch, startDateTime);
            }
            else if (Branch != 0 && EndDateTime != null)
            {
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsBranchEDate(Branch, endDateTime);
            }
            else if (GroupID != 0 && StartDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsGroupSDate(GroupID, startDateTime,Branch);
            }
            else if (GroupID != 0 && EndDateTime != null)
            {
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsGroupEDate(GroupID, endDateTime,Branch);
            }
            else if (leaveType != 0 && StartDateTime != null)
            {
                DateTime startDateTime = Convert.ToDateTime(StartDateTime);
                return EmployeeLeaveDetailsLeaveSDate(leaveType, startDateTime,Branch);
            }
            else if (leaveType != 0 && EndDateTime != null)
            {
                DateTime endDateTime = Convert.ToDateTime(EndDateTime);
                return EmployeeLeaveDetailsLeaveEDate(leaveType, endDateTime,Branch);
            }

            else if (Department != 0)
            {
                return EmployeeLeaveDetailsOne(Department,Branch);
            }
            else if (Branch != 0)
            {
                return EmployeeLeaveDetailsBranch(Branch);
            }
            else if (GroupID != 0)
            {
                return EmployeeLeaveDetailsGroup(GroupID,Branch);
            }
            else if (leaveType != 0)
            {
                return EmployeeLeaveDetailsOne1(leaveType,Branch);
            }
            else
            {
                return RedirectToAction("EmployeeLeaveDetails", "Leave");
            }
        }

        [HttpPost]
        //public ActionResult EmployeeLeaveDetails(int leaveType, int month)
        //{
        //    DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
        //    var acadamicEndMonth = dbHrms.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
        //    var year = DateTime.Now.Month <= acadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
        //    if (month == 0 && leaveType != 0)
        //    {
        //        var calendarDetails = dbHrms.CalendarYears.FirstOrDefault();
        //        var calendarModel = new Calendar().GetCalendarDetails(year, calendarDetails.StartingMonth ?? 1,
        //        calendarDetails.EndingMonth ?? 12);

        //        var startDateTime = calendarModel.StartDate;
        //        var endDateTime = calendarModel.EndDate;

        //        return EmployeeLeaveDetails(leaveType,startDateTime,endDateTime);
        //    }
        //    else if (month == 0) 
        //    { 
        //        return EmployeeLeaveDetails(); 
        //    }
        //    else 
        //    {                
        //        return EmployeeLeaveDetails(leaveType, new DateTime(year, month, 01), new DateTime(year, (month), 01).AddMonths((1)).AddSeconds((-1))); 
        //    }
        //}



        public ActionResult EmployeeLeaveBalance()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.LeaveTypesList =
                db.LeaveTypes.Select(item => new { LeaveTypeId = item.LeaveTypeId, Name = item.Name })
                    .ToDictionary(i => i.LeaveTypeId, i => i.Name);

            List<EmployeeLeaveBalance> employeeLeaveBalances = new List<EmployeeLeaveBalance>();

            var employees = db.Users.Where(o => o.IsActive != true && o.EmpID != null).Select(o => new { UserId = o.UserID, EmployeeName = o.FirstName + " " + o.LastName, EmployeeId = o.EmpID, BranchId = o.BranchId }).ToList();
            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            ViewBag.LeaveTypesList = db.LeaveTypes.ToDictionary(o => o.LeaveTypeId, o => o.Name);

            foreach (var employee in employees)
            {
                var empLeaveBalance = new EmployeeLeaveBalance();
                empLeaveBalance.UserID = employee.UserId;
                empLeaveBalance.EmployeeId = employee.EmployeeId.ToString();
                empLeaveBalance.EmployeeName = employee.EmployeeName;
                empLeaveBalance.Year = year;
                empLeaveBalance.LeaveTypeBalances = (from typ in db.LeaveTypes
                                                     join
                                                         count in db.LeaveBalanceCounts.Where(o => o.Year == year && o.UserId == employee.UserId) on
                                                         typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                                     from data in leftjoin.DefaultIfEmpty()
                                                     select new LeaveBalance()
                                                     {
                                                         LeaveTypeId = typ.LeaveTypeId,
                                                         LeaveType = typ.Name,
                                                         LeaveDaysUsed = data.Value ?? 0
                                                     }).ToList();
                employeeLeaveBalances.Add(empLeaveBalance);
            }

            return View(employeeLeaveBalances);
        }

        public ActionResult EditEmployeeLeaveBalanceDetails(string employeeId, int UseId)
        {

            int empid = Convert.ToInt32(employeeId);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            ViewBag.LeaveTypesList = db.LeaveTypes.ToDictionary(o => o.LeaveTypeId, o => o.Name);
            int userid = UseId;
            var t = (from typ in db.LeaveTypes
                     join
                         count in db.LeaveBalanceCounts.Where(o => o.Year == year && o.UserId == userid) on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                     from data in leftjoin.DefaultIfEmpty()
                     select new LeaveBalance()
                     {
                         LeaveTypeId = typ.LeaveTypeId,
                         LeaveType = typ.Name,
                         UsedDays = data.Value ?? 0
                     }).ToList();

            EmployeeLeaveBalance employeLeaveBalance = new EmployeeLeaveBalance();
            employeLeaveBalance.UserID = userid;
            employeLeaveBalance.EmployeeId = employeeId;
            employeLeaveBalance.EmployeeName = db.Users.Where(o => o.UserID == userid && o.IsActive == true).Select(o => o.FirstName).FirstOrDefault();
            employeLeaveBalance.LeaveTypeBalances = t;
            employeLeaveBalance.Year = year;

            if (employeLeaveBalance != null && employeLeaveBalance.EmployeeId.Equals(employeeId, StringComparison.InvariantCultureIgnoreCase))
            {
                ViewBag.MaxLength = employeLeaveBalance.LeaveTypeBalances.Max(item => item.LeaveType.Length) + 20;
                return View(employeLeaveBalance);
            }
            else
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult EditEmployeeLeaveBalanceDetails(EmployeeLeaveBalance model, int[] UsedDays, int[] Id)
        {
            DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1();
            int UserID = model.UserID;
            dynamic obj;
            for (int Count = 0; Count < Id.Length; Count++)
            {
                int tempid = Id[Count];
                obj = dbhrms.LeaveBalanceCounts.Where(o => o.UserId == UserID && o.LeaveTypeId == tempid).Select(o => o).FirstOrDefault();
                if (obj != null)
                {
                    obj.Value = UsedDays[Count];
                    dbhrms.SaveChanges();
                }
                else
                {
                    var obj1 = new LeaveBalanceCount();
                    obj1.UserId = UserID;
                    obj1.LeaveTypeId = Convert.ToByte(Id[Count]);
                    obj1.Year = model.Year;
                    obj1.Value = UsedDays[Count];
                    dbhrms.LeaveBalanceCounts.AddObject(obj1);
                    dbhrms.SaveChanges();
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeeLeaveBalanceDetails(string employeeId, int UserID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.LeaveTypesList =
                db.LeaveTypes.Select(item => new { LeaveTypeId = item.LeaveTypeId, Name = item.Name })
                    .ToDictionary(i => i.LeaveTypeId, i => i.Name);

            var employee = db.Users.Where(o => o.UserID == UserID && o.IsActive == true).Select(o => new { UserId = o.UserID, EmployeeName = o.FirstName + " " + o.LastName, EmployeeId = o.EmpID }).FirstOrDefault();
            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            ViewBag.LeaveTypesList = db.LeaveTypes.ToDictionary(o => o.LeaveTypeId, o => o.Name);

            var empLeaveBalance = new EmployeeLeaveBalance();
            empLeaveBalance.EmployeeId = employee.EmployeeId.ToString();
            empLeaveBalance.EmployeeName = employee.EmployeeName;
            empLeaveBalance.Year = year;
            empLeaveBalance.LeaveTypeBalances = (from typ in db.LeaveTypes
                                                 join
                                                     count in db.LeaveBalanceCounts.Where(o => o.Year == year && o.UserId == employee.UserId) on
                                                     typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                                 from data in leftjoin.DefaultIfEmpty()
                                                 select new LeaveBalance()
                                                 {
                                                     LeaveTypeId = typ.LeaveTypeId,
                                                     LeaveType = typ.Name,
                                                     DaysAllowed = typ.DaysAllowed ?? 0,
                                                     LeaveDaysUsed = data.Value ?? 0
                                                 }).ToList();
            ViewBag.MaxLength = empLeaveBalance.LeaveTypeBalances.Max(item => item.LeaveType.Length) + 20;
            return View(empLeaveBalance);
        }

        public ActionResult LeaveBalanceDashboard()
        {
            var userId = (int)Session["UserId"];

            DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1();

            var UserDetails = dbhrms.Users.FirstOrDefault(u => u.UserID == userId);
            var employeeId = UserDetails.EmpID;
            var BranchId = UserDetails.BranchId;
            var AcadamicEndMonth = dbhrms.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var leaveBalances1 = GetLeaveBalance1(DateTime.Now.Year, userId);

            var today = DateTime.Today;

            var CurMonth = today.Month;
            var Curyear = today.Year;

            Curyear = CurMonth == 1 ? Curyear - 1 : Curyear;
            CurMonth = CurMonth == 1 ? 12 : CurMonth - 1;

            var startDate = new DateTime(Curyear, CurMonth, 01);
            var endDate = new DateTime(Curyear, CurMonth, 01).AddMonths(1).AddSeconds(-1);
            var holidaysCount = 0;

            foreach (var item in dbhrms.Master_holiday.Where(holiday => holiday.Date >= startDate && holiday.Date <= endDate))
            {
                if (item.Date != null)
                {
                    var tempDate = ((DateTime)item.Date);
                    if (tempDate.DayOfWeek != DayOfWeek.Saturday && tempDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        holidaysCount++;
                    }
                }
            }

            double calcBusinessDays = 1 + ((endDate - startDate).TotalDays * 5 - (startDate.DayOfWeek - endDate.DayOfWeek) * 2) / 7;

            if ((int)endDate.DayOfWeek == 6) calcBusinessDays--;
            if ((int)startDate.DayOfWeek == 0) calcBusinessDays--;
            calcBusinessDays -= holidaysCount;

            ViewBag.WorkingHoursInMonth = Math.Floor(calcBusinessDays) * 8;
            var totalDaysWorked =
                dbhrms.TimeManagements.Where(
                    t =>
                        t.EmpID == employeeId && t.BranchId == BranchId && t.Date >= startDate && t.Date <= endDate).ToList();

            var totalHoursWorked = totalDaysWorked.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).Sum(tm => tm.TotalTime);
            var workingHour = Math.Floor(TimeSpan.FromMinutes(totalHoursWorked ?? 0).TotalHours);
            var blanceMinutes = (totalHoursWorked % 60) / 100.0;
            //ViewBag.TotalHoursWorked = Math.Round((double)(workingHour + (blanceMinutes ?? 0)) - totalDaysWorked.Count(), 2);
            ViewBag.TotalHoursWorked = Math.Round((double)(workingHour + (blanceMinutes ?? 0)), 2);

            var curMonth = DateTime.Now.Month;
            int LOPTypeId = dbhrms.LeaveTypes.FirstOrDefault(o => o.Name == "LOP").LeaveTypeId;

            int ApprovedStatus = Convert.ToInt32(MasterEnum.LeaveStatus.Approved.GetHashCode());

            var MonthlyLOP = dbhrms.LeaveRequests.Where(o => o.StartDateTime.Value.Month == curMonth && o.StartDateTime.Value.Year == year && o.LeaveStatusId == ApprovedStatus && o.UserId == userId).Sum(o => o.LOP);
            var YearlyLOP = dbhrms.LeaveBalanceCounts.FirstOrDefault(o => o.UserId == userId && o.Year == year && o.LeaveTypeId == LOPTypeId);

            ViewBag.MonthlyLOP = MonthlyLOP == null ? 0 : MonthlyLOP.Value;
            ViewBag.YearlyLOP = YearlyLOP == null ? 0 : YearlyLOP.Value;

            string FinancialYear = Convert.ToString(year);
            string FinancialYearEnd = Convert.ToString((year + 1));
            FinancialYearEnd = FinancialYearEnd.Substring(2, 2);
            ViewBag.year = year + "-" + FinancialYearEnd;

            return View(leaveBalances1);
        }

        public JsonResult GetLeaveBalance(int year, int leaveTypeId, int userId)
        {
            var balance = GetLeaveBalance1(year, userId);
            return Json(balance.FirstOrDefault(o => o.LeaveTypeId == leaveTypeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditEmployeeLeaveBalnce()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<EditLeaveBalanceModel> list = new List<EditLeaveBalanceModel>();
            var empIdLIst = db.Users.Where(o => o.IsActive == true && o.EmpID != null).Select(o => o.UserID).ToList();
            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            ViewBag.LeaveTypesList = db.LeaveTypes.ToDictionary(o => o.LeaveTypeId, o => o.Name);
            foreach (int empid in empIdLIst)
            {
                var temp = new EditLeaveBalanceModel();
                temp.UserName = db.Users.Where(o => o.UserID == empid && o.IsActive == true).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                temp.EmployeeId = db.Users.Where(o => o.UserID == empid && o.IsActive == true).Select(o => o.EmpID).FirstOrDefault();
                temp.SelectedUserStatusid = db.Users.Where(o => o.UserID == empid && o.IsActive == true && o.UserStatus != 6).Select(o => o.UserStatus).FirstOrDefault();
                temp.UseId = empid;
                temp.LeaveTypeBalanceValue = (from typ in db.LeaveTypes
                                              join
                                                  count in db.LeaveBalanceCounts.Where(o => o.Year == year && o.UserId == empid) on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                              from data in leftjoin.DefaultIfEmpty()
                                              select new EditLeaveBalanceModel()
                                              {
                                                  TypeID = typ.LeaveTypeId,
                                                  DaysAllowed = typ.DaysAllowed ?? 0,
                                                  UsedDays = data.Value ?? 0,
                                                  SelectedUserStatusid = temp.SelectedUserStatusid
                                              }).ToDictionary(o => o.TypeID, o => o.UsedDays);
                list.Add(temp);
            }
            return View(list);
        }



        public static List<LeaveBalance> GetLeaveBalance(int Year, int userId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            int userid = db.Users.Where(o => o.UserID == userId && o.IsActive == true).Select(o => o.UserID).FirstOrDefault();
            var doj = db.Users.FirstOrDefault(item => item.UserID == userId && item.IsActive == true).DateOfJoin;
            var chk = db.Users.Where(x => x.UserID == userid && x.IsActive == true).Select(o => o).FirstOrDefault();

            string FinancialYear = Convert.ToString(Year);
            string FinancialYearEnd = Convert.ToString((Year + 1));
            FinancialYearEnd = FinancialYearEnd.Substring(2, 2);
            FinancialYear = FinancialYear + "-" + FinancialYearEnd;

            List<LeaveBalance> LeaveTypeList = null;

            if (chk.Gender == MasterEnum.Genders.Female.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
            {
                LeaveTypeList = (from typ in db.LeaveTypes.Where(x => x.LeaveTypeId != 6 && x.LeaveTypeId != 5)
                                 join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == userid)
                                 on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                 from data in leftjoin.DefaultIfEmpty()
                                 select new LeaveBalance()
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     LeaveType = typ.Name,
                                     DaysAllowed = typ.DaysAllowed ?? 0,
                                     UsedDays = data.Value ?? 0,
                                     Year = Year,
                                     AcadamicYear = FinancialYear
                                 }).ToList();

                var MarMat = (from typ in db.LeaveTypes
                              join count in db.LeaveBalanceCounts.Where(o => o.UserId == userid)
                                on typ.LeaveTypeId equals count.LeaveTypeId
                              where typ.LeaveTypeId == 5 || typ.LeaveTypeId == 6
                              select new LeaveBalance()
                              {
                                  LeaveTypeId = typ.LeaveTypeId,
                                  LeaveType = typ.Name,
                                  DaysAllowed = typ.DaysAllowed ?? 0,
                                  UsedDays = count.Value ?? 0,
                                  Year = Year,
                                  AcadamicYear = FinancialYear
                              }).ToList();

                foreach (LeaveBalance marmat in MarMat)
                {
                    LeaveTypeList.Add(marmat);
                }
            }
            else
            {
                LeaveTypeList = (from typ in db.LeaveTypes.Where(x => x.LeaveTypeId != 6)
                                 join
                                     count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == userid) on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                 from data in leftjoin.DefaultIfEmpty()
                                 select new LeaveBalance()
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     LeaveType = typ.Name,
                                     DaysAllowed = typ.DaysAllowed ?? 0,
                                     UsedDays = data.Value ?? 0,
                                     Year = Year,
                                     AcadamicYear = FinancialYear
                                 }).ToList();
            }

            var t = LeaveTypeList.Where(o => o.LeaveTypeId == 3).Select(o => o).FirstOrDefault();

            return LeaveTypeList;
        }

        public ActionResult GetAvailBalance(int leaveRequestID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            LeaveModel LOPDetails = new LeaveModel();

            var LeaveTypeList = new List<LeaveBalance>();

            LOPDetails.LOPdays = 0.0;

            var leaveDetail = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestID);

            DateTime StartDateTime = Convert.ToDateTime(leaveDetail.StartDateTime).AddHours(9);
            DateTime EndDateTime = Convert.ToDateTime(leaveDetail.EndDateTime).AddHours(18);

            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

            bool isEligible;
            List<int> EligibleLeaveTypes = new List<int>();

            var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == leaveDetail.UserId).DateOfJoin;

            if (doj == null)
                isEligible = false;
            else
            {
                var completedDays = (DateTime.Now - doj).Value.Days;
                if (completedDays < 365)
                    isEligible = false;
                else
                    isEligible = true;
            }

            EligibleLeaveTypes.Add(1);
            EligibleLeaveTypes.Add(2);

            if (isEligible)
                EligibleLeaveTypes.Add(3);

            LeaveTypeList = (from typ in db.LeaveTypes
                             join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetail.UserId)
                             on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                             from data in leftjoin.DefaultIfEmpty()
                             select new LeaveBalance
                             {
                                 LeaveTypeId = typ.LeaveTypeId,
                                 DaysAllowed = typ.DaysAllowed.Value,
                                 LeaveType = typ.Name,
                                 UsedDays = data.Value ?? 0,
                                 CalculateLeave = true
                             }).ToList();

            LOPDetails.TotalAvailDays = LeaveTypeList.Where(o => EligibleLeaveTypes.Contains(o.LeaveTypeId) && o.RemainingDays > 0).Sum(o => o.RemainingDays);

            return Json(new { Result = LOPDetails.TotalAvailDays }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAvailLeaveBalance(LeaveModel leaveDetails)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            LeaveModel LOPDetails = new LeaveModel();

            var LeaveTypeList = new List<LeaveBalance>();

            LOPDetails.LOPdays = 0.0;

            DateTime StartDateTime = leaveDetails.StartDateTime.AddHours(9);
            DateTime EndDateTime = leaveDetails.EndDateTime.AddHours(18);


            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

            bool isEligible;
            List<int> EligibleLeaveTypes = new List<int>();

            var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == leaveDetails.UserId).DateOfJoin;

            if (doj == null)
                isEligible = false;
            else
            {
                var completedDays = (DateTime.Now - doj).Value.Days;
                if (completedDays < 365)
                    isEligible = false;
                else
                    isEligible = true;
            }

            EligibleLeaveTypes.Add(1);
            EligibleLeaveTypes.Add(2);

            if (isEligible)
                EligibleLeaveTypes.Add(3);

            LeaveTypeList = (from typ in db.LeaveTypes
                             join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetails.UserId)
                             on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                             from data in leftjoin.DefaultIfEmpty()
                             select new LeaveBalance
                             {
                                 LeaveTypeId = typ.LeaveTypeId,
                                 DaysAllowed = typ.DaysAllowed.Value,
                                 LeaveType = typ.Name,
                                 UsedDays = data.Value ?? 0,
                                 CalculateLeave = true
                             }).ToList();

            LOPDetails.TotalAvailDays = LeaveTypeList.Where(o => EligibleLeaveTypes.Contains(o.LeaveTypeId) && o.RemainingDays > 0).Sum(o => o.RemainingDays);

            return Json(new { Result = LOPDetails.TotalAvailDays }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTotalLeaveBalance(LeaveModel leaveDetails)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            LeaveModel LOPDetails = new LeaveModel();

            var LeaveTypeList = new List<LeaveBalance>();

            LOPDetails.LOPdays = 0.0;

            DateTime StartDateTime = leaveDetails.StartDateTime.AddHours(9);
            DateTime EndDateTime = leaveDetails.EndDateTime.AddHours(18);


            if (leaveDetails.LeaveTypeId != 4)
            {
                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                bool isEligible;
                List<int> EligibleLeaveTypes = new List<int>();

                var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == leaveDetails.UserId).DateOfJoin;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;
                    if (completedDays < 365)
                        isEligible = false;
                    else
                        isEligible = true;
                }

                EligibleLeaveTypes.Add(1);
                EligibleLeaveTypes.Add(2);

                if (isEligible)
                    EligibleLeaveTypes.Add(3);

                LeaveTypeList = (from typ in db.LeaveTypes
                                 join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetails.UserId)
                                 on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                 from data in leftjoin.DefaultIfEmpty()
                                 select new LeaveBalance
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     DaysAllowed = typ.DaysAllowed.Value,
                                     LeaveType = typ.Name,
                                     UsedDays = data.Value ?? 0,
                                     CalculateLeave = true
                                 }).ToList();

                LOPDetails.TotalAvailDays = LeaveTypeList.Where(o => EligibleLeaveTypes.Contains(o.LeaveTypeId) && o.RemainingDays > 0).Sum(o => o.RemainingDays);
                LOPDetails.totalLeaveDays = 0.0;

                var LeaveType = new DSRCManagementSystemEntities1().LeaveTypes.ToList();
                var UserRegion = db.Users.Where(x => x.UserID == leaveDetails.UserId).Select(o => o.Region).FirstOrDefault();

                var holidayList = db.AddHolidays.Where(holiday => holiday.Date >= StartDateTime.Date && holiday.Date <= EndDateTime.Date && holiday.ZoneId == UserRegion && holiday.Isactive == true).Select(item => item.Date).ToList();
                if (leaveDetails.HalfDay)
                {
                    LOPDetails.totalLeaveDays = 0.5;
                }
                else
                {
                    LOPDetails.totalLeaveDays = new LeaveBalance().CalculateLeaveDays(StartDateTime, EndDateTime, holidayList).LeaveDays;
                }

                if (LOPDetails.TotalAvailDays <= 0)
                {
                    LOPDetails.LOPdays = LOPDetails.totalLeaveDays;
                }
                else if (LOPDetails.totalLeaveDays > LOPDetails.TotalAvailDays)
                {
                    LOPDetails.LOPdays = LOPDetails.totalLeaveDays - LOPDetails.TotalAvailDays;
                }
            }

            TempData["LOPdays"] = LOPDetails.LOPdays;

            return Json(new { Result = LOPDetails.LOPdays }, JsonRequestBehavior.AllowGet);


        }


        public ActionResult GetLOPDays(int leaveRequestId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            LeaveModel LOPDetails = new LeaveModel();

            var leaveDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestId);

            var LeaveTypeList = new List<LeaveBalance>();

            LOPDetails.LOPdays = 0.0;

            DateTime StartDateTime = Convert.ToDateTime(leaveDetails.StartDateTime);
            DateTime EndDateTime = Convert.ToDateTime(leaveDetails.EndDateTime);

            StartDateTime = StartDateTime.AddHours(9);
            EndDateTime = EndDateTime.AddHours(18);

            if (leaveDetails.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || leaveDetails.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode())
            {
                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                bool isEligible;
                List<int> EligibleLeaveTypes = new List<int>();

                var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == leaveDetails.UserId).DateOfJoin;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;
                    if (completedDays < 365)
                        isEligible = false;
                    else
                        isEligible = true;
                }

                EligibleLeaveTypes.Add(1);
                EligibleLeaveTypes.Add(2);

                if (isEligible)
                    EligibleLeaveTypes.Add(3);

                LeaveTypeList = (from typ in db.LeaveTypes
                                 join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetails.UserId)
                                 on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                 from data in leftjoin.DefaultIfEmpty()
                                 select new LeaveBalance
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     DaysAllowed = typ.DaysAllowed.Value,
                                     LeaveType = typ.Name,
                                     UsedDays = data.Value ?? 0,
                                     CalculateLeave = true
                                 }).ToList();

                LOPDetails.TotalAvailDays = LeaveTypeList.Where(o => EligibleLeaveTypes.Contains(o.LeaveTypeId) && o.RemainingDays > 0).Sum(o => o.RemainingDays);
                LOPDetails.totalLeaveDays = 0.0;

                var UserRegion = db.Users.Where(x => x.UserID == leaveDetails.UserId).Select(o => o.Region).FirstOrDefault();

                var LeaveType = new DSRCManagementSystemEntities1().LeaveTypes.ToList();
                var holidayList = db.AddHolidays.Where(holiday => holiday.Date >= StartDateTime.Date && holiday.Date <= EndDateTime.Date && holiday.ZoneId == UserRegion && holiday.Isactive == true).Select(item => item.Date).ToList();


                LOPDetails.totalLeaveDays = (double)leaveDetails.LeaveDays;


                if (LOPDetails.TotalAvailDays <= 0)
                {
                    LOPDetails.LOPdays = LOPDetails.totalLeaveDays;
                }
                else if (LOPDetails.totalLeaveDays > LOPDetails.TotalAvailDays)
                {
                    LOPDetails.LOPdays = LOPDetails.totalLeaveDays - LOPDetails.TotalAvailDays;
                }
            }

            TempData["LOP"] = LOPDetails.LOPdays;

            return Json(new { Result = LOPDetails.LOPdays }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult IsLeaveTypeExists(string leaveTypeName)
        {

            if (String.IsNullOrEmpty(leaveTypeName))
            {
                return Json(new { Result = false });
            }

            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            bool leaveTypeExists =
                dbHrms.LeaveTypes.Any(item => item.Name.Equals(leaveTypeName.Trim(), StringComparison.OrdinalIgnoreCase));

            return Json(new { Result = leaveTypeExists }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult IsHolidayExists(DateTime holiday, string describtion)
        {
            if (holiday == null)
            {
                return Json(new { Result = false });
            }

            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            bool holidayExists =
                dbHrms.Master_holiday.Any(item => item.Date.Value.Equals(holiday) && item.Detail == describtion);

            return Json(new { Result = holidayExists });
        }

        public ActionResult DropDownFilter(string monthType, int id)
        {
            if (String.Equals(monthType, "StartingMonth", StringComparison.OrdinalIgnoreCase))
            {
                var endingMonth = id + 11 == 12 ? 12 : (id + 11) - 12;
                return Json(new { EndingMonth = endingMonth }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var startingMonth = id - 1 == 11 ? 1 : (id - 11) + 12;
                return Json(new { StartingMonth = startingMonth }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Calendar()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0 } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
            return View();
        }

        [HttpGet]
        public ActionResult CalendarEvents(int LeaveTypeId)
        {
            var userId = (int)Session["UserId"];
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                int leaveStatus = MasterEnum.LeaveStatus.Approved.GetHashCode();

                var recordsIQueryable = db.LeaveRequests.Where(o => o.LeaveStatusId == leaveStatus).Join(db.Users.Where(o => o.IsActive == true), l => l.UserId, u => u.UserID, (l, u) => new { l, u })
                        .Join(db.UserReportings, r => r.u.UserID, ur => ur.UserID, (r, ur) => new { r, ur }).Where(o => o.ur.ReportingUserID == userId).Select(v => v);
                if (LeaveTypeId != 0)
                {
                    recordsIQueryable = recordsIQueryable.Where(x => x.r.l.LeaveType.LeaveTypeId == LeaveTypeId);
                }
                var records = recordsIQueryable.ToList();
                var data = records.Select(x => new CalenderEvents()
                {
                    title = x.r.u.FirstName + " " + (x.r.u.LastName ?? "").Trim(),
                    start = x.r.l.StartDateTime.Value.ToString("ddd, MMM d, yyyy"),
                    end = x.r.l.EndDateTime.Value.AddDays(1).ToString("ddd, MMM d, yyyy"),
                    Detail = x.r.l.Details,
                    className = "colorClass" + x.r.l.LeaveType.LeaveTypeId
                }).ToList();
                return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult MonthCalendar()
        {
            var userId = (int)Session["UserId"];
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                int leaveStatus = MasterEnum.LeaveStatus.Approved.GetHashCode();

                var recordsIQueryable = db.LeaveRequests.Where(x => x.UserId == userId && x.LeaveStatusId == 2).ToList();

                //if (LeaveTypeId != 0)
                //{
                //    recordsIQueryable = recordsIQueryable.Where(x => x.r.l.LeaveType.LeaveTypeId == LeaveTypeId);
                //}
                var records = recordsIQueryable.ToList();
                var data = records.Select(x => new CalenderEvents()
                {

                    // title = x.r.u.FirstName + " " + (x.r.u.LastName ?? "").Trim(),
                    start = x.StartDateTime.Value.ToString("ddd, MMM d, yyyy")
                    // end = x.r.l.EndDateTime.Value.AddDays(1).ToString("ddd, MMM d, yyyy"),
                    // Detail = x.r.l.Details,
                    // className = "colorClass" 
                }).ToList();
                return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet]
        public ActionResult AddHolidays()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.Format> obj = new List<DSRCManagementSystem.Models.Format>();
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
            obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());

            foreach (var meetingSchedule in obj)
            {
                meetingSchedule.EnteredBy = LeaveController.GetUserString(objdb, meetingSchedule.EnteredBy);
            }

            return View(obj);
        }


        [HttpGet]
        public ActionResult AddNewDays()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.ExtraHolidays obj = new DSRCManagementSystem.Models.ExtraHolidays();

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

            obj.EnteredBy = LeaveController.GetUserString(objdb, obj.EnteredBy);

            return View(obj);
        }
        [HttpPost]
        public ActionResult AddNewDays(ExtraHolidays objzone)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.AddHoliday obj = new DSRCManagementSystem.AddHoliday();
            var val = objdb.TimeZones.Where(x => x.Zone == objzone.ZoneName).Select(o => o.Id).FirstOrDefault();
            int UserId = int.Parse(Session["UserID"].ToString());
            var already = objdb.AddHolidays.Where(x => x.Date == objzone.Date && x.ZoneId == val && x.Isactive == true && x.HolidayName == objzone.HolidayName).Select(o => o).FirstOrDefault();
            var same = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == val && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();
            if (already != null)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (same != null)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            if (val == 4)
            {

                var allalready1 = objdb.AddHolidays.Where(x => x.HolidayName == objzone.HolidayName && x.Date == objzone.Date && x.ZoneId == 1 && x.Isactive == true).Select(o => o).FirstOrDefault();

                var same1 = objdb.AddHolidays.Where(x => x.Date != objzone.Date && x.ZoneId == 1 && x.HolidayName == objzone.HolidayName && x.Isactive == true).Select(o => o).FirstOrDefault();

                if (allalready1 != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }

                else if (same1 != null)
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
                if (allalready2 != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
                else if (same2 != null)
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

                if (allalready3 != null)
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
                else if (same3 != null)
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
                objdb.SaveChanges();

                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                obj.HolidayName = objzone.HolidayName.Trim();
                obj.Date = objzone.Date;
                obj.ZoneId = val;
                obj.Isactive = true;
                obj.EnteredBy = UserId.ToString();
                objdb.AddToAddHolidays(obj);
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AddNewDaysEdit(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.ExtraHolidays obj = new DSRCManagementSystem.Models.ExtraHolidays();

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


            obj.EnteredBy = LeaveController.GetUserString(objdb, obj.EnteredBy);

            TempData["HolidayId"] = Id;

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
                var check = db.AddHolidays.Where(x => x.Date == obj.Date && x.HolidayName == obj.HolidayName).Select(o => o).FirstOrDefault();
                if (check != null)
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

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            int UserId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

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
        [HttpGet]
        public ActionResult DashBoard()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.HolidayDashBoard> obj = new List<DSRCManagementSystem.Models.HolidayDashBoard>();
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
                meetingSchedule.EnteredBy = LeaveController.GetUserString(objdb, meetingSchedule.EnteredBy);
            }
            obj.ForEach(x => x.Day = x.Date.Value.Date.DayOfWeek.ToString());
            return View(obj);
        }

        [HttpPost]
        public ActionResult DashBoard(HolidayDashBoard obj, FormCollection form)
        {
            string ZoneId = (form["Id3"] == "") ? "0" : form["Id3"].ToString();
            int Id = Convert.ToInt32(ZoneId);
            if (Id != 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                List<DSRCManagementSystem.Models.HolidayDashBoard> value = new List<DSRCManagementSystem.Models.HolidayDashBoard>();
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
                    meetingSchedule.EnteredBy = LeaveController.GetUserString(objdb, meetingSchedule.EnteredBy);
                }
                return View(value);
            }
            else
            {
                return RedirectToAction("DashBoard", "Leave");
            }

        }


        [HttpPost]
        public ActionResult Reset1()
        {
            return RedirectToAction("DashBoard", "Leave");
        }

        public ActionResult UpcomingLeaves()
        {
            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var leaveRequestQuery = from leaveRequestsData in db.LeaveRequests
                                    where leaveRequestsData.StartDateTime >= System.DateTime.Today
                                    select leaveRequestsData;

            var leaveRequestsResult = GetLeaveRequestsQuery(leaveRequestQuery, 0, 2).ToList();


            if (Request.IsAjaxRequest())
            {
                return PartialView("_UpcomingLeaves", leaveRequestsResult);
            }

            return View(model: leaveRequestsResult);
        }



        [HttpGet]
        public ActionResult EmployeeLeaveEntry()
        {
            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            var userId = (int)Session["UserId"];

            ViewBag.Reportable = new SelectList(GetReporatablePerson(), "UserId", "Name");
            ViewBag.LeaveTypeList = new SelectList(dbHrms.LeaveTypes.Where(i => i.ApplicableEmployees != "All" && i.ApplicableEmployees != "None").ToList(), "LeaveTypeId", "Name");

            return View();
        }

        [HttpPost]
        public ActionResult EmployeeLeaveEntry(LeaveModel leaveRequest)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            int CancelledCode = MasterEnum.LeaveStatus.Cancelled.GetHashCode();
            int RejectedCode = MasterEnum.LeaveStatus.Rejected.GetHashCode();
            int Comp_Off = MasterEnum.LeaveTypes.Comp_Off.GetHashCode();
            int Maternity = MasterEnum.LeaveTypes.Maternity.GetHashCode();
            int Marriage = MasterEnum.LeaveTypes.Marriage.GetHashCode();
            bool Pending1 = Convert.ToBoolean(MasterEnum.LeaveStatus.Pending.GetHashCode());
            bool Approved1 = Convert.ToBoolean(MasterEnum.LeaveStatus.Approved.GetHashCode());
            byte Pending = Convert.ToByte(Pending1);
            byte Approved = Convert.ToByte(Approved1);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           // int UserId = (int)Session["UserId"];

            int UserId = leaveRequest.UserId;
           
            var UserRegion = db.Users.Where(x => x.UserID == leaveRequest.UserId).Select(o => o.Region).FirstOrDefault();

            var LeaveType = new DSRCManagementSystemEntities1().LeaveTypes.ToList();

            leaveRequest.WorkedDate1 = leaveRequest.WorkedDate1;
           leaveRequest.StartDateTime = leaveRequest.StartDateTime.AddHours(9);
                     
           leaveRequest.EndDateTime = leaveRequest.EndDateTime.AddHours(18);
         
            if (leaveRequest.StartDateTime >= leaveRequest.EndDateTime)
                ModelState.AddModelError("DateTime", "Start Date must be lower than End Date");
            var holidayList =
                   db.AddHolidays.Where(holiday => holiday.Date >= leaveRequest.StartDateTime.Date && holiday.Date <= leaveRequest.EndDateTime.Date && holiday.ZoneId == UserRegion).Select(item => item.Date).ToList();
            if (holidayList.Count > 0 && leaveRequest.StartDateTime.Date == leaveRequest.EndDateTime.Date)
                return Json(new { Result = "Holiday" }, JsonRequestBehavior.AllowGet);

            if (ModelState.IsValid)
            {
                var name = db.Users.Where(o => o.UserID == UserId && o.IsActive == true).FirstOrDefault();

                leaveRequest.LeaveRequestTo = UserId;
                leaveRequest.ReportingPersonName = (name.FirstName + " " + (name.LastName ?? "")).Trim();
                leaveRequest.UserName = db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequest.UserId).FirstName;
                leaveRequest.LastName = db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequest.UserId).LastName;
                leaveRequest.ReportingPersonEmail = db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == UserId).EmailAddress;

                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();

                var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                int i = leaveRequest.LeaveType;

                var a = GetLeaveBalance(year, UserId);

                leaveRequest.Balance = (from b in a
                                        select new LevaeBalance()
                                        {
                                            LeaveTypeId = b.LeaveTypeId,
                                            Name = b.LeaveType,
                                            DaysAllowed = (int)b.DaysAllowed,
                                            UsedDays = (int)b.UsedDays,

                                        }).ToList();

                double j = leaveRequest.Balance.Where(o => o.LeaveTypeId == i).Select(o => o.DaysAllowed).FirstOrDefault();

                
              
                if (db.LeaveRequests.Where(o =>  o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Select(o => o).ToList().Count > 0)
                {
                    if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).Select(o => o).ToList().Count > 0)
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                    }

                    else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.StartDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                    }

                    else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.EndDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)


                    //else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.EndDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                    }

                    else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)

                    //else if (db.LeaveRequests.Where(o => o.StartDateTime <= leaveRequest.StartDateTime && o.EndDateTime >= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId !=CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                    }

                    else if (db.LeaveRequests.Where(o => o.StartDateTime >= leaveRequest.StartDateTime && o.EndDateTime <= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)

                    //else if (db.LeaveRequests.Where(o => o.StartDateTime >= leaveRequest.StartDateTime && o.EndDateTime <= leaveRequest.EndDateTime).Where(o => o.UserId == UserId).Where(o => o.LeaveStatusId != CancelledCode && o.LeaveStatusId != RejectedCode).ToList().Count > 0)
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);
                    }
                }

                leaveRequest.BranchID = (int)db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == UserId).BranchId;
                if (leaveRequest.LeaveType == Comp_Off)
                {
                    string[] splitCompoff = leaveRequest.WorkedDate1.ToString().Split(',');

                    string date = "";
                    bool chk = false, check = false;
                    int counttemp = 0, counttemp1 = 0;

                    for (int count = 0; count < splitCompoff.Count(); count++)
                    {
                        int getIndex = splitCompoff[count].ToString().IndexOf('(');
                        string getDay = splitCompoff[count].ToString().Remove(splitCompoff[count].ToString().IndexOf(')')).ToString().Substring(getIndex + 1);
                        string replace = splitCompoff[count].ToString().Remove(getIndex);
                        DateTime d = Convert.ToDateTime(replace);
                        date = d.ToString("dd/MM/yyyy");
                        var holidays = db.AddHolidays.Where(x => x.ZoneId == UserRegion && x.Isactive == true).Select(o => o.Date).ToList();
                        foreach (var r in holidays)
                        {
                            if ((r.Value.ToString("dd/MM/yyyy") == date && Convert.ToDateTime(date) < DateTime.Now) || getDay == "Sunday" || getDay == "Saturday")
                            {
                                counttemp++;
                            }

                            if (splitCompoff.Count() == counttemp)
                            {
                                chk = true;
                            }
                        }

                        var time = (from us in db.Users.Where(o => o.IsActive == true)
                                    join lea in db.LeaveRequests on us.UserID equals lea.UserId into leave
                                    join tme in db.TimeManagements on us.EmpID equals tme.EmpID
                                    from leavereq in leave.DefaultIfEmpty()
                                    where us.IsActive == true && us.UserID == UserId && tme.BranchId == leaveRequest.BranchID
                                    select new LeaveModel()
                                    {
                                        Date = tme.Date,
                                        Minutes = (int)tme.TotalTime,

                                    }).Distinct().ToList();

                        foreach (var o in time)
                        {
                            if (o.Date.ToString("dd/MM/yyyy") == date && o.Minutes > 300)
                            {
                                counttemp1++;
                            }
                            if (splitCompoff.Count() == counttemp1)
                            {
                                check = true;
                            }
                        }
                    }
                    if (chk == false)
                    {
                        return Json(new { Result = "Not Holiday" }, JsonRequestBehavior.AllowGet);
                    }

                    if (check == false)
                    {
                        return Json(new { Result = "not applicable" }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (leaveRequest.LeaveType == Comp_Off)
                {
                    double cont = leaveRequest.WorkedDate1.ToString().Split(',').Count();
                    leaveRequest.Dayss = new LeaveBalance().CalculateLeaveDays(leaveRequest.StartDateTime, leaveRequest.EndDateTime, holidayList).LeaveDays;
                    double day = leaveRequest.Dayss;

                    if (cont != day)
                    {
                        return Json(new { Result = "not equal" }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (leaveRequest.LeaveType == Comp_Off)
                {

                    string[] splitCompoff = leaveRequest.WorkedDate1.ToString().Split(',');

                    string date = "", date1 = "";
                    bool chk = false;
                    int countemp = 0;
                    for (int count = 0; count < splitCompoff.Count(); count++)
                    {
                        int getIndex = splitCompoff[count].ToString().IndexOf('(');
                        string replace = splitCompoff[count].ToString().Remove(getIndex);
                        date = Convert.ToDateTime(replace).ToString("dd/MM/yyyy");
                        var Work = db.LeaveRequests.Where(x => x.UserId == UserId && x.LeaveTypeId == 4 && x.LeaveStatusId < 3).Select(x => x.WorkedDate).ToList();
                        foreach (var p in Work)
                        {
                            if (p != null)
                            {
                                day = p.Split(',');
                                for (int x = 0; x < day.Count(); x++)
                                {
                                    int get = day[x].ToString().IndexOf('(');
                                    string replac = "";
                                    if (get != -1)
                                    {
                                        replac = day[x].ToString().Remove(get);
                                    }
                                    else replac = day[x];
                                    date1 = Convert.ToDateTime(replac).ToString("dd/MM/yyyy");
                                    if (date1 == date)
                                    {
                                        countemp++;
                                    }

                                    if (splitCompoff.Count() == countemp)
                                    {
                                        chk = true;
                                    }
                                }
                            }

                        }
                    }
                    if (chk == true)
                    {
                        return Json(new { Result = "already" }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (leaveRequest.HalfDay)
                {
                    leaveRequest.totalLeaveDays = 0.5;
                }
                else
                {
                    leaveRequest.totalLeaveDays = 0.0;
                }

                if (leaveRequest.LeaveType == Comp_Off)
                {
                    if (LeaveType.First(type => type.LeaveTypeId == 4) != null)
                    {
                        int count = leaveRequest.WorkedDate1.ToString().Split(',').Count();
                        leaveRequest.totalLeaveDays = count;
                    }
                }
                else if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    if (LeaveType.First(type => type.LeaveTypeId == MasterEnum.LeaveTypes.Maternity.GetHashCode()) != null)
                    {
                        int count = 90;
                        leaveRequest.totalLeaveDays = count;
                    }
                }
                else
                {
                    leaveRequest.totalLeaveDays = new LeaveBalance().CalculateLeaveDays(leaveRequest.StartDateTime, leaveRequest.EndDateTime, holidayList).LeaveDays;
                }

                if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    var leaobj = db.LeaveRequests.Where(x => x.LeaveTypeId == Maternity && x.UserId == UserId && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Count();

                    if (Convert.ToInt32(leaobj) >= 2)
                    {
                        return Json(new { Result = "Maternity already applied" }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    var matobj = db.LeaveRequests.Where(x => x.UserId == UserId && x.LeaveTypeId == Maternity && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Select(o => o.EndDateTime).FirstOrDefault();
                    DateTime Dt = Convert.ToDateTime(matobj);
                    Dt = Dt.AddMonths(12);
                    DateTime startda = leaveRequest.StartDateTime;
                    if (startda < Dt)
                    {
                        return Json(new { Result = "unavailable" }, JsonRequestBehavior.AllowGet);
                    }

                }

                if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                {
                    var leaobj = db.LeaveRequests.Where(x => x.LeaveTypeId == Marriage && x.UserId == UserId && (x.LeaveStatusId == Approved || x.LeaveStatusId == Pending)).Count();

                    if (Convert.ToInt32(leaobj) >= 1)
                    {
                        return Json(new { Result = "Marriage already applied" }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (leaveRequest.LeaveType != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() && leaveRequest.LeaveType != MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    leaveRequest.LOPdays = Convert.ToDouble(TempData["LOPdays"]);
                }
                else
                {
                    leaveRequest.LOPdays = 0.0;
                }

                double LOPs = 0.0;

                if (leaveRequest.LeaveType != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() && leaveRequest.LeaveType != MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {
                    /** For Calculatin LOP calling this Mehtod if check availability is not called*****/
                    GetTotalLeaveBalance(leaveRequest);
                    LOPs = Convert.ToDouble(TempData["LOPdays"]);
                }

                var leaveApproved = new LeaveRequest()
                {
                    LeaveTypeId = leaveRequest.LeaveType,
                    StartDateTime = leaveRequest.StartDateTime,
                    EndDateTime = leaveRequest.EndDateTime.AddSeconds(-1),
                    WorkedDate = leaveRequest.WorkedDate1,
                    UserId = leaveRequest.UserId,
                    LeaveStatusId = (byte)2,  //status self approved by manageer to user while applying
                    Details = leaveRequest.Details.Trim(),
                    LeaveDays = leaveRequest.totalLeaveDays,
                    ReportingTo = UserId,
                    ProcessedBy = UserId,
                    RequestedDate = DateTime.Now,
                    ProcessedOn = DateTime.Now,
                    LOP = LOPs,
                    Comments = leaveRequest.Details
                };
                db.LeaveRequests.AddObject(leaveApproved);
                db.SaveChanges();

                leaveRequest.LeaveRequestedId = leaveApproved.LeaveRequestId;
                leaveRequest.LeaveTypeName = db.LeaveTypes.Where(o => o.LeaveTypeId == leaveRequest.LeaveType).Select(o => o.Name).FirstOrDefault();

                //If Marriage Leave is Applied and Approved Marital Status has to be updated in Users Table
                if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                {
                    var UpdateMaritalStatus = db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == leaveRequest.UserId);
                    UpdateMaritalStatus.MaritalStatus = MasterEnum.MaritialStatus.Married.GetHashCode(); /* 1-Married 2-UnMarried*/
                    db.SaveChanges();
                }

                var FromDate = leaveRequest.StartDateTime;
                var ToDate = leaveRequest.EndDateTime;
                var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();

                var year1 = FromDate.Month <= 3 ? FromDate.Year - 1 : FromDate.Year;
                bool IsAcadamicYearEnd = (FromDate.Month == AcadamicEndMonth && ToDate.Month != AcadamicEndMonth);

                var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                a = GetLeaveBalance(years, leaveRequest.UserId);

                leaveRequest.Balance = (from b in a
                                        select new LevaeBalance()
                                        {
                                            LeaveTypeId = b.LeaveTypeId,
                                            Name = b.LeaveType,
                                            DaysAllowed = (int)b.DaysAllowed,
                                            UsedDays = (int)b.UsedDays,

                                        }).ToList();

                if (leaveRequest.LeaveType == MasterEnum.LeaveTypes.Marriage.GetHashCode() || leaveRequest.LeaveType == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                {

                    var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequest.UserId &&
                                                  leavebalance.LeaveTypeId == leaveRequest.LeaveType
                                              select leavebalance).FirstOrDefault();

                    if (updateleavebalance == null)
                    {
                        updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                        updateleavebalance.UserId = leaveRequest.UserId;
                        updateleavebalance.LeaveTypeId = leaveRequest.LeaveType;
                        updateleavebalance.Value = leaveApproved.LeaveDays;
                        updateleavebalance.Year = year;
                        db.LeaveBalanceCounts.AddObject(updateleavebalance);
                        db.SaveChanges();
                    }
                    else
                    {
                        updateleavebalance.Value = updateleavebalance.Value + leaveApproved.LeaveDays;
                        updateleavebalance.Year = year;
                        db.SaveChanges();
                    }
                }


                UpdateLeaveBalance(leaveApproved);

                var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Leave Request Approved").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {
                         var objLeaveRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Approved")
                                                        join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                        select new DSRCManagementSystem.Models.Email
                                                        {
                                                            To = p.To,
                                                            CC = p.CC,
                                                            BCC = p.BCC,
                                                            Subject = p.Subject,
                                                            Template = q.TemplatePath
                                                        }).FirstOrDefault();
                         leaveRequest.ReportingPersonName = db.Users.Where(o => o.IsActive == true).Where(o => o.UserID == leaveRequest.LeaveRequestTo).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                         var user = db.Users.Where(o => o.IsActive == true).Where(o => o.UserID == leaveRequest.UserId).Select(o => o).FirstOrDefault();
                         leaveRequest.UserName = user.FirstName + " " + (user.LastName ?? "");
                         leaveRequest.UserEmail = user.EmailAddress;

                         string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequest.LeaveType).Name;
                         string StartTime = Convert.ToDateTime(leaveRequest.StartDateTime).ToString("ddd, MMM d, yyyy");
                         string EndTime = Convert.ToDateTime(leaveRequest.EndDateTime).ToString("ddd, MMM d, yyyy");

                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathLeaveRequestApproved = Server.MapPath(objLeaveRequestApproved.Template);
                         string htmlLeaveRequestApproved = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#UserName", leaveRequest.UserName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ManagerName", leaveRequest.ReportingPersonName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#Comments", leaveRequest.Comments);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LeaveTypeName", LeaveTyepName);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#StartDateTime", StartTime);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#EndDateTime", EndTime);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#totalLeaveDays", leaveRequest.totalLeaveDays.ToString());
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#Comments", leaveRequest.Comments);
                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#CompanyName", company);
                         if (leaveApproved.LOP > 0 && (leaveApproved.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || leaveApproved.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()))
                         {
                             //FULL RED #FF0000
                             string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveApproved.LOP + "</label></p>";
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", LOPDays);

                             string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request has to be considered as LOP.</p>";
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", LOP);
                         }
                         else
                         {
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", "");
                             htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", "");
                         }

                         htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ServerName",ServerName);

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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
                                 //  var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //  DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequest.UserEmail, Server.MapPath(logo.ToString()));
                                 // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequest.UserEmail, Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Leave Request Approved", folder, ServerName);
                     }


                return Json(new { Result = "Success" });
            }
            return Json(new { Result = "Not Success" });
        }

        public static void UpdateLeaveBalance(LeaveRequest leaveRequestToUpdate)
        {
            if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var LeaveType = db.LeaveTypes.ToList();
                var FromDate = leaveRequestToUpdate.StartDateTime;
                var ToDate = leaveRequestToUpdate.EndDateTime;
                var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                LeaveModel leaveRequest = new LeaveModel();
                var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var a = GetLeaveBalance(years, leaveRequestToUpdate.UserId);

                leaveRequest.Balance = (from b in a
                                        select new LevaeBalance()
                                        {
                                            LeaveTypeId = b.LeaveTypeId,
                                            Name = b.LeaveType,
                                            DaysAllowed = (int)b.DaysAllowed,
                                            UsedDays = (int)b.UsedDays,
                                        }).ToList();

                var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == leaveRequestToUpdate.UserId).DateOfJoin;

                bool isEligible;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;

                    if (completedDays < 365)
                    {
                        isEligible = false;
                    }
                    else
                    {
                        isEligible = true;
                    }
                }

                int ExtendLeaveTypeId1 = 0;
                int ExtendLeaveTypeId2 = 0;
                int ExtendLeaveTypeId3 = 0;


                if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                {
                    ExtendLeaveTypeId1 = 2;
                    ExtendLeaveTypeId2 = 3;
                }
                else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                {
                    ExtendLeaveTypeId1 = 1;
                    ExtendLeaveTypeId2 = 3;
                }
                else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                {
                    ExtendLeaveTypeId1 = 2;
                    ExtendLeaveTypeId2 = 1;
                }
                else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                {
                    if (isEligible)
                    {
                        ExtendLeaveTypeId1 = 2;
                        ExtendLeaveTypeId2 = 1;
                        ExtendLeaveTypeId3 = 3;
                    }
                    else
                    {
                        ExtendLeaveTypeId1 = 1;
                        ExtendLeaveTypeId2 = 2;   // for earned unelligible this type wont be taken for count
                        ExtendLeaveTypeId3 = 2;
                    }
                }

                double detectionDays = 0.0;

                var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                          where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                          leavebalance.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId &&
                                          leavebalance.Year == year
                                          select leavebalance).FirstOrDefault();

                var updateLeaveDetectionDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestToUpdate.LeaveRequestId);

                double? UsedDays = 0.0;
                double? AllowedDays = LeaveType.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).DaysAllowed;
                double? TotalLeaveDays = leaveRequestToUpdate.LeaveDays;
                double? RemainingAvailLeaveDays = 0.0;

                if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                {
                    updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                          where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                          leavebalance.LeaveTypeId == ExtendLeaveTypeId3 &&
                                          leavebalance.Year == year
                                          select leavebalance).FirstOrDefault();

                    AllowedDays = LeaveType.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3).DaysAllowed;
                }

                if (updateleavebalance == null)
                {
                    updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                    updateleavebalance.UserId = leaveRequestToUpdate.UserId;

                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId3;
                    }
                    else
                    {
                        updateleavebalance.LeaveTypeId = db.LeaveRequests.Where(o => o.LeaveRequestId == leaveRequestToUpdate.LeaveRequestId).Select(o => o.LeaveType.LeaveTypeId).FirstOrDefault();
                    }

                    RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                    if (TotalLeaveDays <= RemainingAvailLeaveDays)
                    {
                        updateleavebalance.Value = TotalLeaveDays;
                        detectionDays = (double)TotalLeaveDays;
                        TotalLeaveDays = 0;
                    }
                    else
                    {
                        updateleavebalance.Value = RemainingAvailLeaveDays;
                        detectionDays = (double)RemainingAvailLeaveDays;
                        TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                    }
                    updateleavebalance.Year = year;
                    db.LeaveBalanceCounts.AddObject(updateleavebalance);
                    db.SaveChanges();

                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                    {
                        updateLeaveDetectionDetails.Sick = detectionDays;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                    {
                        updateLeaveDetectionDetails.Casual = detectionDays;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                    {
                        updateLeaveDetectionDetails.Earned = detectionDays;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        if (isEligible)
                        {
                            updateLeaveDetectionDetails.Earned = detectionDays;
                        }
                        else
                        {
                            updateLeaveDetectionDetails.Sick = detectionDays;
                        }
                    }
                    db.SaveChanges();
                }
                else
                {
                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        AllowedDays = LeaveType.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3).DaysAllowed;
                        UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3 &&
                                                                              o.UserId == leaveRequestToUpdate.UserId && o.Year == year).Value;
                    }
                    else
                    {
                        UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId &&
                                                                                 o.UserId == leaveRequestToUpdate.UserId && o.Year == year).Value;
                    }


                    //Detect From selected LeaveType

                    if (UsedDays < AllowedDays)
                    {
                        RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                        if (TotalLeaveDays <= RemainingAvailLeaveDays)
                        {
                            updateleavebalance.Value += TotalLeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();

                            detectionDays = (double)TotalLeaveDays;

                            TotalLeaveDays = 0;
                        }
                        else
                        {
                            updateleavebalance.Value += RemainingAvailLeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();

                            detectionDays = (double)RemainingAvailLeaveDays;

                            TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;

                        }

                        if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                        {
                            updateLeaveDetectionDetails.Sick = detectionDays;
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                        {
                            updateLeaveDetectionDetails.Casual = detectionDays;
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                        {
                            updateLeaveDetectionDetails.Earned = detectionDays;
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                        {
                            if (isEligible)
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                            else
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                        }
                        db.SaveChanges();
                    }
                }

                ////Detect From next LeaveType
                if (TotalLeaveDays > 0)
                {
                    updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                          where leavebalance.UserId == leaveRequestToUpdate.UserId && leavebalance.LeaveTypeId == ExtendLeaveTypeId1 &&
                                          leavebalance.Year == year
                                          select leavebalance).FirstOrDefault();

                    UsedDays = 0.0;
                    AllowedDays = LeaveType.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId1).DaysAllowed;

                    if (updateleavebalance == null)
                    {
                        updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                        updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                        updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId1;

                        RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                        if (TotalLeaveDays <= RemainingAvailLeaveDays)
                        {
                            updateleavebalance.Value = TotalLeaveDays;

                            detectionDays = (double)TotalLeaveDays;

                            TotalLeaveDays = 0;
                        }
                        else
                        {
                            updateleavebalance.Value = RemainingAvailLeaveDays;

                            detectionDays = (double)RemainingAvailLeaveDays;

                            TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                        }

                        updateleavebalance.Year = year;
                        db.LeaveBalanceCounts.AddObject(updateleavebalance);
                        db.SaveChanges();

                        if (ExtendLeaveTypeId1 == 1)
                        {
                            updateLeaveDetectionDetails.Sick = detectionDays;
                        }
                        else if (ExtendLeaveTypeId1 == 2)
                        {
                            updateLeaveDetectionDetails.Casual = detectionDays;
                        }
                        else if (ExtendLeaveTypeId1 == 3)
                        {
                            updateLeaveDetectionDetails.Earned = detectionDays;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId1 &&
                                                                             o.UserId == leaveRequestToUpdate.UserId && o.Year == year).Value;

                        if (UsedDays < AllowedDays)
                        {
                            RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                            if (TotalLeaveDays <= RemainingAvailLeaveDays)
                            {
                                updateleavebalance.Value += TotalLeaveDays;
                                updateleavebalance.Year = year;
                                db.SaveChanges();

                                detectionDays = (double)TotalLeaveDays;

                                TotalLeaveDays = 0;
                            }
                            else
                            {
                                updateleavebalance.Value += RemainingAvailLeaveDays;
                                updateleavebalance.Year = year;
                                db.SaveChanges();

                                detectionDays = (double)RemainingAvailLeaveDays;

                                TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                            }

                            if (ExtendLeaveTypeId1 == 1)
                            {
                                updateLeaveDetectionDetails.Sick = detectionDays;
                            }
                            else if (ExtendLeaveTypeId1 == 2)
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                            else if (ExtendLeaveTypeId1 == 3)
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                            db.SaveChanges();
                        }
                    }
                }
                //Detect from next leave type;
                if (TotalLeaveDays > 0)
                {
                    if (isEligible)
                    {
                        updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                                  leavebalance.LeaveTypeId == ExtendLeaveTypeId2 &&
                                                  leavebalance.Year == year
                                              select leavebalance).FirstOrDefault();
                        UsedDays = 0.0;
                        AllowedDays = LeaveType.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId2).DaysAllowed;


                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId2;

                            RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                            if (TotalLeaveDays <= RemainingAvailLeaveDays)
                            {
                                updateleavebalance.Value = TotalLeaveDays;

                                detectionDays = (double)TotalLeaveDays;

                                TotalLeaveDays = 0;
                            }
                            else
                            {
                                updateleavebalance.Value = RemainingAvailLeaveDays;

                                detectionDays = (double)RemainingAvailLeaveDays;

                                TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                            }

                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();

                            if (ExtendLeaveTypeId2 == 1)
                            {
                                updateLeaveDetectionDetails.Sick = detectionDays;
                            }
                            else if (ExtendLeaveTypeId2 == 2)
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                            else if (ExtendLeaveTypeId2 == 3)
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId2 &&
                                                                                    o.UserId == leaveRequestToUpdate.UserId && o.Year == year).Value;

                            if (UsedDays < AllowedDays)
                            {
                                RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                                if (TotalLeaveDays <= RemainingAvailLeaveDays)
                                {
                                    updateleavebalance.Value += TotalLeaveDays;
                                    updateleavebalance.Year = year;
                                    db.SaveChanges();

                                    detectionDays = (double)TotalLeaveDays;

                                    TotalLeaveDays = 0;
                                }
                                else
                                {
                                    updateleavebalance.Value += RemainingAvailLeaveDays;
                                    updateleavebalance.Year = year;
                                    db.SaveChanges();

                                    detectionDays = (double)RemainingAvailLeaveDays;

                                    TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                                }

                                if (ExtendLeaveTypeId2 == 1)
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (ExtendLeaveTypeId2 == 2)
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (ExtendLeaveTypeId2 == 3)
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                                db.SaveChanges();
                            }
                        }
                    }
                }
                //still leave days remains na then it has to take as LOP
                if (TotalLeaveDays > 0)
                {
                    byte LeaveTypeId = db.LeaveTypes.FirstOrDefault(o => o.Name == "LOP").LeaveTypeId;
                    //Extra Leaves than earned leaves will be taken as LOP
                    updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                          where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                          leavebalance.LeaveTypeId == LeaveTypeId &&
                                          leavebalance.Year == year
                                          select leavebalance).FirstOrDefault();

                    if (updateleavebalance == null)
                    {
                        updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                        updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                        updateleavebalance.LeaveTypeId = LeaveTypeId;
                        updateleavebalance.Value = TotalLeaveDays;
                        updateleavebalance.Year = year;
                        db.LeaveBalanceCounts.AddObject(updateleavebalance);
                        db.SaveChanges();

                        updateLeaveDetectionDetails.LOP = TotalLeaveDays;
                        db.SaveChanges();
                    }
                    else
                    {
                        updateleavebalance.Value += TotalLeaveDays;
                        updateleavebalance.Year = year;
                        db.SaveChanges();

                        updateLeaveDetectionDetails.LOP = TotalLeaveDays;
                        db.SaveChanges();

                        TotalLeaveDays = 0;
                    }
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailLeaveTypes(int UserId)
        {

            int Maternity = MasterEnum.LeaveTypes.Maternity.GetHashCode();
            int LOP = MasterEnum.LeaveTypes.LOP.GetHashCode();

            int Earned_Leave = MasterEnum.LeaveTypes.Earned_Leave.GetHashCode();
            int Marriage = MasterEnum.LeaveTypes.Marriage.GetHashCode();


            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> LeaveType = new List<SelectListItem>();

            if (UserId != 0)
            {

                List<byte> validLeaveTypes = new List<byte>();
                var UserDetails = db.Users.Where(o => o.IsActive == true).FirstOrDefault(o => o.UserID == UserId);
                var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == UserId).DateOfJoin;
                bool isEligible;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;

                    if (completedDays < 365)
                        isEligible = false;
                    else
                        isEligible = true;
                }

                if (UserDetails.Gender == null)
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
                else if (UserDetails.MaritalStatus == null)
                {
                    return Json("NoMarital", JsonRequestBehavior.AllowGet);
                }
                else if (UserDetails.Gender == MasterEnum.Genders.Female.GetHashCode() && UserDetails.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    validLeaveTypes = db.LeaveTypes.Where(o => o.LeaveTypeId != Earned_Leave && o.LeaveTypeId != LOP && o.LeaveTypeId != Marriage).Select(o => o.LeaveTypeId).ToList();

                    if (isEligible)
                        validLeaveTypes.Add(3);
                }
                else if (UserDetails.Gender == MasterEnum.Genders.Female.GetHashCode() && UserDetails.MaritalStatus == MasterEnum.MaritialStatus.Single.GetHashCode())
                {
                    validLeaveTypes = db.LeaveTypes.Where(o => o.LeaveTypeId != Earned_Leave && o.LeaveTypeId != LOP && o.LeaveTypeId != Maternity).Select(o => o.LeaveTypeId).ToList();

                    if (isEligible)
                        validLeaveTypes.Add(3);
                }
                else if (UserDetails.Gender == MasterEnum.Genders.Male.GetHashCode() && UserDetails.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    validLeaveTypes = db.LeaveTypes.Where(o => o.LeaveTypeId != Earned_Leave && o.LeaveTypeId != LOP && o.LeaveTypeId != Maternity && o.LeaveTypeId != Marriage).Select(o => o.LeaveTypeId).ToList();

                    if (isEligible)
                        validLeaveTypes.Add(3);
                }
                else if (UserDetails.Gender == MasterEnum.Genders.Male.GetHashCode() && UserDetails.MaritalStatus == MasterEnum.MaritialStatus.Single.GetHashCode())
                {
                    validLeaveTypes = db.LeaveTypes.Where(o => o.LeaveTypeId != Earned_Leave && o.LeaveTypeId != LOP && o.LeaveTypeId != Maternity).Select(o => o.LeaveTypeId).ToList();

                    if (isEligible)
                        validLeaveTypes.Add(3);
                }

                LeaveType = (from lt in db.LeaveTypes.Where(o => validLeaveTypes.Contains(o.LeaveTypeId))
                              select new AvailLeaveTypes()
                              {
                                  LeaveTypeId = lt.LeaveTypeId,
                                  Name = lt.Name
                              }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.LeaveTypeId), Text = m.Name });

            }
            return Json(new SelectList(LeaveType, "Value", "Text"), JsonRequestBehavior.AllowGet);
            

        }

        #region Other Methods

        private static IQueryable<LeaveRequest> GetLeaveRequestsQuery(IQueryable<LeaveRequest> leaveRequestQuery, int? leaveTypeId, int? leaveStatusId)
        {
            if (leaveTypeId != null && leaveTypeId != 0 && leaveStatusId != null && leaveStatusId != 0)
            {
                return
                    leaveRequestQuery.Where(
                        item =>
                            item.LeaveTypeId == leaveTypeId &&
                            item.LeaveStatusId == leaveStatusId).Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveTypeId != null && leaveTypeId != 0)
            {
                return
                    leaveRequestQuery.Where(item => item.LeaveTypeId == leaveTypeId)
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveStatusId != null && leaveStatusId != 0)
            {
                return
                    leaveRequestQuery.Where(item => item.LeaveStatusId == leaveStatusId)
                        .Include(x => x.User).Include(i => i.User1);
            }
            return leaveRequestQuery.Include(x => x.User).Include(i => i.User1);
        }

        private static IQueryable<LeaveRequest> GetLeaveDetailsQuery(IQueryable<LeaveRequest> leaveRequestQuery, int? leaveTypeId, DateTime? startDateTime, DateTime? endDateTime)
        {
            if (leaveTypeId != null && leaveTypeId != 0 && startDateTime != null && endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(
                        item =>
                            (item.LeaveTypeId == leaveTypeId) && ((item.StartDateTime <= startDateTime && item.EndDateTime >= startDateTime) || (item.StartDateTime <= endDateTime && item.EndDateTime >= endDateTime) || (item.StartDateTime >= startDateTime && item.EndDateTime <= endDateTime))).Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveTypeId != null && leaveTypeId != 0 && startDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => (item.LeaveTypeId == leaveTypeId) && (item.StartDateTime >= startDateTime))
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveTypeId != null && leaveTypeId != 0 && endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => (item.LeaveTypeId == leaveTypeId) && (item.EndDateTime <= endDateTime))
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveTypeId != null && leaveTypeId != 0)
            {
                return
                    leaveRequestQuery.Where(item => item.LeaveTypeId == leaveTypeId)
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (startDateTime != null && endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => (item.StartDateTime <= startDateTime && item.EndDateTime >= startDateTime) || (item.StartDateTime <= endDateTime && item.EndDateTime >= endDateTime) || (item.StartDateTime >= startDateTime && item.EndDateTime <= endDateTime))
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (startDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => item.StartDateTime >= startDateTime)
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => item.EndDateTime <= endDateTime)
                        .Include(x => x.User).Include(i => i.User1);
            }
            return leaveRequestQuery.Include(x => x.User).Include(i => i.User1);
        }

        private static IQueryable<LeaveRequest> GetEmployeeLeaveBalanceQuery(IQueryable<LeaveRequest> leaveRequestQuery, DateTime? startDateTime, DateTime? endDateTime)
        {
            if (startDateTime != null && endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => (item.StartDateTime <= startDateTime && item.EndDateTime >= startDateTime) || (item.StartDateTime <= endDateTime && item.EndDateTime >= endDateTime) || (item.StartDateTime >= startDateTime && item.EndDateTime <= endDateTime))
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (startDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => item.StartDateTime >= startDateTime)
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (endDateTime != null)
            {
                return
                    leaveRequestQuery.Where(item => item.EndDateTime <= endDateTime)
                        .Include(x => x.User).Include(i => i.User1);
            }
            return leaveRequestQuery.Include(x => x.User).Include(i => i.User1);
        }



        private List<ReportablePerson> GetReporatablePerson()
        {
            var userId = (int)Session["UserId"];

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                List<ReportablePerson> reportablepersons = null;
                var AllEmpLeaveEntryAccessUserList = db.AllEmpLeaveEntryPermissions.Select(o => o.UserID).ToList();

                if(AllEmpLeaveEntryAccessUserList.Contains(userId))
                {
                    reportablepersons = (from usr_ropt in db.Users.Where(o => o.IsActive == true && o.IsExclude == false && o.UserStatus != 6)

                                         select new ReportablePerson()
                                         {
                                             UserID = usr_ropt.UserID,
                                             Name = (usr_ropt.FirstName + " " + (usr_ropt.LastName ?? "")).Trim()
                                         }).OrderBy(o => o.Name).ToList();
                }
                else
                {
                    reportablepersons = (from usr_ropt in db.UserReportings
                                         join
                                             usr in db.Users.Where(o => o.IsActive != false && o.UserStatus != 6) on usr_ropt.UserID equals usr.UserID
                                         where usr_ropt.ReportingUserID == userId && usr_ropt.UserID != userId
                                         select new ReportablePerson()
                                         {
                                             UserID = usr.UserID,
                                             Name = (usr.FirstName + " " + (usr.LastName ?? "")).Trim()
                                         }).OrderBy(o => o.Name).ToList();
                }

                return reportablepersons;
            }

        }




        private List<ReportingPerson> GetReportingPersons(int id = 0)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var reportingPersonId = db.UserReportings.Where(x => x.UserID == userID).Select(x => x.ReportingUserID).ToList();


                List<ReportingPerson> reportingPersons = (from u in db.Users.Where(o => o.IsActive == true)
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

        private static Dictionary<int, List<LeaveBalance>> GetLeaveBalance(List<int> years, List<int?> leaveTypesParam, int userId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var a = db.LeaveTypes.ToList();

            var LeaveTypeList = (from typ in db.LeaveTypes
                                 join count in db.LeaveBalanceCounts on typ.LeaveTypeId equals count.LeaveTypeId
                                 select new LeaveBalance
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     DaysAllowed = (typ.DaysAllowed ?? 0) - count.Value ?? 0
                                 }).ToList();
            List<LeaveBalance> leaveBalancesList = new List<LeaveBalance>();
            Dictionary<int, List<LeaveBalance>> leaveBalances = new Dictionary<int, List<LeaveBalance>>();

            int gender = db.Users.Where(o => o.IsActive == true).First(item => item.UserID == userId).Gender ?? 1;
            string userGender = gender == 1 ? "Male" : "Female";

            var LeaveType =
                db.LeaveTypes.Where(
                    i =>
                        i.ApplicableEmployees.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                        i.ApplicableEmployees.Equals(userGender, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var calendarYear in years)
            {

                int Startingmonth = ((int)db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault());
                int EndMonth = ((int)db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault());
                var calendar = new Calendar().GetCalendarDetails(calendarYear, Startingmonth, EndMonth);

                List<LeaveBalance> leaveTypesList = new List<LeaveBalance>();
                foreach (var leaveType in LeaveType)
                {
                    leaveTypesList.Add(new LeaveBalance() { LeaveTypeId = leaveType.LeaveTypeId, CalculateLeave = leaveType.CalculateLeaveDays ?? false, LeaveType = leaveType.Name, Year = calendarYear, ApplicableEmployees = leaveType.ApplicableEmployees });
                }

                leaveBalances.Add(calendarYear, leaveTypesList);



                IQueryable<IGrouping<byte?, LeaveRequest>> leaveDays = null;

                if (leaveTypesParam != null && leaveTypesParam.Count != 0)
                {
                    //Gets the leave balance for the specific leave type.
                    leaveDays =
                        db.LeaveRequests.Where(
                            leaves =>
                                leaves.UserId == userId && leaves.LeaveStatusId == 2 &&
                                (leaveTypesParam.Count != 0 && leaveTypesParam.Contains(leaves.LeaveTypeId)))
                            .GroupBy(i => i.LeaveTypeId);
                }
                else
                {
                    //Gets the leave balance of an employee for all leave types.
                    leaveDays =
                        db.LeaveRequests.Where(
                            leaves => leaves.UserId == userId && leaves.LeaveStatusId == 2)
                            .GroupBy(i => i.LeaveTypeId);
                }
                var UserRegion = db.Users.Where(x => x.UserID == userId).Select(x => x.Region).FirstOrDefault();

                calendar.Holidays = db.AddHolidays.Where(
                    holiday =>
                        holiday.Date >= calendar.StartDate && holiday.Date <= calendar.EndDate && holiday.ZoneId == UserRegion && holiday.Isactive == true)
                    .Select(item => item.Date)
                    .ToList();

                foreach (var leaveDay in leaveDays)
                {
                    var totalDays = 0.0;
                    if (LeaveType.First(type => type.LeaveTypeId == leaveDay.Key).DaysAllowed != 0)
                    {
                        totalDays = new LeaveBalance().GetLeaveBalance(leaveDay.ToList(), calendar.Holidays,
                        calendar.StartDate, calendar.EndDate);
                    }
                    else
                    {
                        totalDays = 0.0;
                    }

                    leaveBalances[calendarYear].Find(i => i.LeaveTypeId == leaveDay.Key).LeaveDaysUsed = totalDays;
                }
            }

            return leaveBalances;
        }

        private static List<LeaveBalance> GetLeaveBalance1(int years, int userId)
        {

            int Earned_Leave = MasterEnum.LeaveTypes.Earned_Leave.GetHashCode();
            int Maternity1 = MasterEnum.LeaveTypes.Maternity.GetHashCode();

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            bool isEligible;
            var doj = db.Users.Where(o => o.IsActive == true).FirstOrDefault(item => item.UserID == userId).DateOfJoin;
            if (doj == null)
                isEligible = false;
            else
            {
                var completedDays = (DateTime.Now - doj).Value.Days;
                if (completedDays < 365)
                    isEligible = false;
                else
                    isEligible = true;
            }

            var LeaveTypeList = (from typ in db.LeaveTypes
                                 join count in db.LeaveBalanceCounts.Where(o => o.Year == years && o.UserId == userId)
                                 on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                 from data in leftjoin.DefaultIfEmpty()
                                 select new LeaveBalance
                                 {
                                     LeaveTypeId = typ.LeaveTypeId,
                                     DaysAllowed = typ.DaysAllowed.Value,
                                     LeaveType = typ.Name,
                                     UsedDays = data.Value ?? 0,
                                     CalculateLeave = true
                                 }).ToList();

            var t = LeaveTypeList.Where(o => o.LeaveTypeId == Earned_Leave).Select(o => o).FirstOrDefault();
            t.DaysAllowed = isEligible ? t.DaysAllowed : 0;

            var Maternity = LeaveTypeList.Where(o => o.LeaveTypeId == Maternity1).Select(o => o).FirstOrDefault();
            Maternity.DaysAllowed = Maternity.DaysAllowed / 2;

            return LeaveTypeList;
        }

        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(o => o.IsActive == true).Where(user => lst.Contains(user.UserID)) select user.FirstName + " " + (user.LastName ?? "")).ToList();
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

        public static void UpdateLeaveBalanceReject(LeaveRequest leaveRequestToUpdate)
        {


            int Sick = MasterEnum.LeaveTypes.Sick.GetHashCode();
            int Casual = MasterEnum.LeaveTypes.Casual.GetHashCode();
            int Earned_Leave = MasterEnum.LeaveTypes.Earned_Leave.GetHashCode();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var FromDate = leaveRequestToUpdate.StartDateTime;
            var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;

            var leaveDetectedDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestToUpdate.LeaveRequestId);

            if (leaveDetectedDetails != null)
            {

                if ((leaveDetectedDetails.Sick == 0 || leaveDetectedDetails.Sick == null) && (leaveDetectedDetails.Casual == 0 || leaveDetectedDetails.Casual == null) &&
                    (leaveDetectedDetails.Earned == 0 || leaveDetectedDetails.Earned == null) && (leaveDetectedDetails.LOP == 0 || leaveDetectedDetails.LOP == null))
                {
                    var updateLeaveBalance = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId &&
                                                                                       o.UserId == leaveRequestToUpdate.UserId && o.Year == year);
                    updateLeaveBalance.Value -= leaveRequestToUpdate.LeaveDays;
                    db.SaveChanges();
                }
                else
                {
                    var sickDays = leaveDetectedDetails.Sick ?? 0.0;
                    var casualDays = leaveDetectedDetails.Casual ?? 0.0;
                    var earnedDays = leaveDetectedDetails.Earned ?? 0.0;
                    var lopDays = leaveDetectedDetails.LOP ?? 0.0;

                    var LOPId = db.LeaveTypes.FirstOrDefault(o => o.Name == "LOP").LeaveTypeId;

                    var updateLeaveBalance = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == Sick && o.UserId == leaveRequestToUpdate.UserId && o.Year == year);

                    if (updateLeaveBalance != null)
                    {
                        updateLeaveBalance.Value -= sickDays;
                        db.SaveChanges();
                    }

                    updateLeaveBalance = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == Casual && o.UserId == leaveRequestToUpdate.UserId && o.Year == year);

                    if (updateLeaveBalance != null)
                    {
                        updateLeaveBalance.Value -= casualDays;
                        db.SaveChanges();
                    }

                    updateLeaveBalance = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == Earned_Leave && o.UserId == leaveRequestToUpdate.UserId && o.Year == year);

                    if (updateLeaveBalance != null)
                    {
                        updateLeaveBalance.Value -= earnedDays;
                        db.SaveChanges();
                    }

                    updateLeaveBalance = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == LOPId && o.UserId == leaveRequestToUpdate.UserId && o.Year == year);

                    if (updateLeaveBalance != null)
                    {
                        updateLeaveBalance.Value -= lopDays;
                        db.SaveChanges();
                    }
                }
            }
        }

        #endregion


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailEmployees(int DepartmentName)
        {

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
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
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DepartmentName && d.BranchID == GetBranch
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

            }
            return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDepartments(int BranchId)
        {
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
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
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            if (DepartmentId != 0)
            {
                var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == DepartmentId).Select(d => d.GroupID).ToList();

                FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                               where lt.IsActive == true
                               select new FilterGroup()
                               {
                                   GroupId = lt.GroupID,
                                   GroupName = lt.GroupName
                               }).AsEnumerable().OrderBy(o=>o.GroupName).Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });
            }
            else
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

        public List<Tuple<DateTime, string, string>> currentdates = new List<Tuple<DateTime, string,string>>();
        [HttpGet]
        public ActionResult LeaveCalender(DateTime Month)
        {



            var userId = (int)Session["UserId"];


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
                        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();

                        if (Holiday.Count > 0)
                        {
                            flagColor2 = "colorClass9";
                            currentdates.Add(Tuple.Create(objdate, flagColor2, HolidayName));
                        }

                        if (leaveRequestApproved != null)
                        {
                            flagColor1 = "colorClass7";
                            currentdates.Add(Tuple.Create(objdate, flagColor1, leaveRequestApproved));
                        }
                                     
                }
                else
                {
                             
                        DateTime objdate = Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i);
                        string HolidayName = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.HolidayName).FirstOrDefault();

                        var timemang = db.TimeManagements.Where(x => x.EmpID == EmpID && x.Date == objdate).Select(o => o.Date).ToList();
                        var leave = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                        string weekends = "Weekend";
                        if (leaveRequestApproved != null)
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
                                currentdates.Add(Tuple.Create(objdate, flagColor3, leaveRequestApproved));

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










                //if (DateTime.Now.Date < Convert.ToDateTime(Month))
                //{
                   
                    
                //   int day = DateTime.DaysInMonth(Month.Year, Month.Month);  
                //    for (int i = 1; i < day; i++)
                //    {
                //        string flagColor1 = "";
                //        string flagColor2 = "";
                //        DateTime objdate = Convert.ToDateTime(Month.AddDays(i));
                //        var Holiday = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                //        string HolidayName = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.HolidayName).FirstOrDefault();
                //        //var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();  'original
                //        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                        
                //        if (Holiday.Count > 0)
                //        {
                //            flagColor2 = "colorClass9";
                //            currentdates.Add(Tuple.Create(objdate, flagColor2, HolidayName));
                //        }

                //        if (leaveRequestApproved != null)
                //        {
                //            flagColor1 = "colorClass7";
                //            currentdates.Add(Tuple.Create(objdate, flagColor1, leaveRequestApproved));
                //        }
                //    }

                //    var HolidayData = (from t in currentdates
                //                       select new
                //                       {
                //                           start = t.Item1.AddDays(1),
                //                           end = t.Item1.AddDays(1),
                //                           className = t.Item2,
                //                           sample= t.Item3

                //                       }).ToArray();

                //    return Json(HolidayData.ToArray(), JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                   
                //    for (int i = 1; i <= day; i++)    //days replaced by day
                //    {

                //        string flagColor1 = "";
                //        string flagColor2 = "";
                //        string flagColor3 = "";

                //        DateTime objdate = Convert.ToDateTime(todaydate.AddDays(-1)).AddDays(i);

                //        string HolidayName = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.HolidayName).FirstOrDefault();

                //        var timemang = db.TimeManagements.Where(x => x.EmpID == EmpID && x.Date == objdate).Select(o => o.Date).ToList();
                //        var leave = db.AddHolidays.Where(x => x.Date == objdate).Select(o => o.Date).ToList();
                //        //var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(objdate)).Select(o => o).ToList();              'original
                //       // var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                //        var leaveRequestApproved = db.LeaveRequests.Where(x => x.UserId == userId && EntityFunctions.TruncateTime(x.StartDateTime) <= EntityFunctions.TruncateTime(objdate) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(objdate)).Select(o => o.Details).FirstOrDefault();
                //        string weekends = "Weekend";
                //        //if (leaveRequestApproved.Count > 0)
                //        //{
                //        //    flagColor3 = "colorClass7";
                //        //}
                //        if (leaveRequestApproved != null)
                //        {
                //            flagColor3 = "colorClass7";
                //            }
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

                //                currentdates.Add(Tuple.Create(objdate, flagColor1,HolidayName));
                //                flagColor1 = "";

                //            }
                //            else if (flagColor2 != "")
                //            {
                //                currentdates.Add(Tuple.Create(objdate, flagColor2,HolidayName));
                //                flagColor2 = "";
                //            }
                //            else if (flagColor3 != "" && leaveRequestApproved != null)
                //            {
                //                currentdates.Add(Tuple.Create(objdate, flagColor3, leaveRequestApproved));
                               
                //                flagColor3 = "";
                //            }
                //            else if (flagColor3 != "" && leaveRequestApproved == null)
                //            {
                //                currentdates.Add(Tuple.Create(objdate, flagColor3, HolidayName));
                //                flagColor3 = "";
                //            }
                //        }
                //        else
                //        {
                //            flagColor2 = "colorClass9";
                //            currentdates.Add(Tuple.Create(objdate, flagColor2, weekends));
                //            flagColor2 = "";
                //        }

                //    }
                if (DateTime.Now.Date < Convert.ToDateTime(Month))
                {
                var HolidayData = (from t in currentdates
                                   select new
                                   {
                                       start = t.Item1.AddDays(1),
                                       end = t.Item1.AddDays(1),
                                       className = t.Item2,
                                       sample = t.Item3

                                   }).ToArray();
               

                return Json(HolidayData.ToArray(), JsonRequestBehavior.AllowGet);

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
            }
        }

        [HttpGet]
        public ActionResult AllEmpLeaveAccess()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var FilteredUsers =
                       db.Users.Where(
                           u => u.IsActive == true && u.UserStatus != 6)
                           .Select(x => x.UserID)
                           .ToList()
                           .
                           Except(
                               db.AllEmpLeaveEntryPermissions.Where(ep => ep.UserID != null)
                                   .Select(x => x.UserID.Value)
                                   .ToList()).ToList();
            List<object> UnAuthUsers = new List<object>();
            foreach (int users in FilteredUsers)
            {
                UnAuthUsers.AddRange(
                    db.Users.Where(u => u.UserID == users)
                        .Select(u => new { userid = u.UserID, username = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "") })
                        .ToList());
            }
            ViewBag.Users = new MultiSelectList(UnAuthUsers, "userid", "username");

            var AuthUsers = (from ep in db.AllEmpLeaveEntryPermissions.Where(ep => ep.UserID != null)
                             join u in db.Users.Where(u => u.IsActive == true && u.UserStatus != 6) on ep.UserID equals u.UserID
                                 into evper
                             from eventper in evper.DefaultIfEmpty()
                             select new
                             {
                                 userid = eventper.UserID,
                                 username = eventper.FirstName + " " + eventper.LastName
                             }).ToList();

            ViewBag.Users1 = new MultiSelectList(AuthUsers, "userid", "username");

            return View();
        }

        

        [HttpPost]
        public ActionResult AllEmpLeaveAccess(List<int?> From, List<int?> To)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var deleteuser = db.AllEmpLeaveEntryPermissions.Select(o => o).ToList();
                foreach (var deluser in deleteuser)
                    db.AllEmpLeaveEntryPermissions.DeleteObject(deluser);
                db.SaveChanges();
                for (int j = 0; j < To.Count(); j++)
                {
                    DSRCManagementSystem.AllEmpLeaveEntryPermission objpermission = new DSRCManagementSystem.AllEmpLeaveEntryPermission();
                    objpermission.UserID = To[j];
                    db.AddToAllEmpLeaveEntryPermissions(objpermission);
                    db.SaveChanges();
                }
                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
            return Json("Authorize", JsonRequestBehavior.AllowGet);
        }
    }
}




