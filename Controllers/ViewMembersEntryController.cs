using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Globalization;

namespace DSRCManagementSystem.Controllers
{
    public class ViewMembersEntryController : Controller
    {
        //
        // GET: /ViewMembersEntry/MembersTimeEntry

        // [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment, Tech Lead,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer,Head - Quality")]
        public ActionResult MembersTimeEntry()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            TeamEntryData model = new TeamEntryData();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                model.BranchID = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                model.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                model.IsTeamData = true;
                model.DateFrom = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.DateTo = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: model.MemberList, Date: DateTime.Today.AddDays(-1), IsAscending: true, BranchId: model.BranchID);
                // model.EmployeeData = new List<HoursWorkData>();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model);
        }


        [HttpPost]
        //[DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment, Tech Lead,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer,Head - Quality")]
        public ActionResult MembersTimeEntry(TeamEntryData model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                model.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                model.IsTeamData = model.MemberId == "0";
                model.BranchID = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                //if (model.DateFrom != null)
                //{
                DateTime FromDate = DateTime.ParseExact(model.DateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (model.IsTeamData)
                    model.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: model.MemberList, Date: FromDate, IsAscending: model.IsSorting.Equals("true"), BranchId: model.BranchID);
                else
                {
                    DateTime ToDate = DateTime.ParseExact(model.DateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    model.EmployeeData = TimeEntryHelper.GetSingleMemberData(EmpId: model.MemberId, FromDate: FromDate, ToDate: ToDate, IsAscending: true, BranchId: model.BranchID);
                }
                //}
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model);
        }

        public ActionResult TimeEntry()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            TeamEntryData model = new TeamEntryData();
            model.MemberId = TimeEntryHelper.GetEmpId(userId: userId);
            model.DateFrom = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            model.DateTo = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return View(model);
        }

        [HttpPost]
        public ActionResult TimeEntry(TeamEntryData model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var userId = (int)Session["UserId"];

                model.BranchID = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                DateTime FromDate = DateTime.ParseExact(model.DateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime ToDate = DateTime.ParseExact(model.DateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.EmployeeData = TimeEntryHelper.GetSingleMemberData(EmpId: model.MemberId, FromDate: FromDate, ToDate: ToDate, IsAscending: true, BranchId: model.BranchID);
                DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1();
                var UserDetails = dbhrms.Users.Where(u => u.UserID == userId).First();
                var employeeId = UserDetails.EmpID;
                var BranchID = UserDetails.BranchId;

                var AcadamicEndMonth = dbhrms.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;


                //var today = DateTime.Today;
                //var startDate = new DateTime(today.Year, today.Month - 1, 01);
                //var endDate = new DateTime(today.Year, today.Month - 1, 01).AddMonths(1).AddSeconds(-1);
                var holidaysCount = 0;//dbhrms.Holidays.Where(holiday => holiday.Date >= FromDate && holiday.Date <= ToDate).Select(item => item.Date).ToList().Count;

                foreach (var item in dbhrms.Master_holiday.Where(holiday => holiday.Date >= FromDate && holiday.Date <= ToDate))
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


                double calcBusinessDays = 1 + ((ToDate - FromDate).TotalDays * 5 - (FromDate.DayOfWeek - ToDate.DayOfWeek) * 2) / 7;

                if ((int)ToDate.DayOfWeek == 6) calcBusinessDays--;
                if ((int)FromDate.DayOfWeek == 0) calcBusinessDays--;
                calcBusinessDays -= holidaysCount;

                ViewBag.WorkingHoursInMonth = Math.Floor(calcBusinessDays) * 8; //(ToDate.Subtract(FromDate).Days + 1) * 8;
                var totalDaysWorked =
                    dbhrms.TimeManagements.Where(
                        t =>
                            t.EmpID == employeeId && t.BranchId == BranchID && t.Date >= FromDate && t.Date <= ToDate).ToList();



                var totalHoursWorked = totalDaysWorked.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).Sum(tm => tm.TotalTime);
                var workingHour = Math.Floor(TimeSpan.FromMinutes(totalHoursWorked ?? 0).TotalHours);
                var blanceMinutes = (totalHoursWorked % 60) / 100.0;
                //ViewBag.TotalHoursWorked = Math.Round((double)(workingHour + (blanceMinutes ?? 0)) - totalDaysWorked.Count(), 2);
                ViewBag.TotalHoursWorked = Math.Round((double)(workingHour + (blanceMinutes ?? 0)), 2);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model);
        }

        [HttpGet]

        public ActionResult TeamTimeEntry(FormCollection form)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            ProjectMapping ObjPM = new ProjectMapping();
            TeamEntryData obj = new TeamEntryData();
            try
            {
                var curmonth = DateTime.Now.Month;
                var month = objdb.Master_TeamMonths.Where(o => o.Id < curmonth).ToList();
                var userId = (int)Session["UserId"];
                int BranchID = (int)objdb.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                ViewBag.Months = new SelectList(month, "Id", "Months", (curmonth - 1));
                ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName");
                ObjPM.Members = GetMembers(obj);
                obj.ProjectMembersDetails = GetMembers(obj);
                obj.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                obj.IsTeamData = true;
                obj.DateFrom = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                obj.DateTo = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                obj.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: obj.MemberList, Date: DateTime.Today.AddDays(-1), IsAscending: true, BranchId: BranchID);
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
        // [DSRCAuthorize(Roles = "Vice President, Project Manager, Tech Lead,Head - Quality")]
        public ActionResult TeamTimeEntry(ProjectMapping ObjPM,TeamEntryData teamobj, FormCollection form)   
        {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                ViewMembers obj1 = new ViewMembers();
                TeamEntryData obj = new TeamEntryData();
            try
            {
                int userId = (int)(Session["UserID"]);
                int BranchID = (int)objdb.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                string projectID = (form["ProjectList"] == "") ? "0" : form["ProjectList"].ToString();
                int PId = Convert.ToInt32(projectID);
                ViewBag.Projects = new SelectList(LoadProjects(), "ProjectID", "ProjectName", projectID);

                //if (Convert.ToInt32(teamobj.MemberId) == 0 || teamobj.MemberId==null)
                if (teamobj.MemberId == null || teamobj.MemberId == "0")
                {
                    obj.ProjectMembersDetails = GetMembers(teamobj, PId);
                   obj.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                    obj.IsTeamData = true;
                    obj.MemberId ="0";
                }
                else
                {
                    obj.ProjectMembersDetails = null;


                    obj.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                    obj.IsTeamData = obj.MemberId == "0";

                }
                if (Convert.ToString(form["DateFrom"]) != null)
                {
                    DateTime FDate = DateTime.ParseExact(Convert.ToString(form["DateFrom"]), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    if (obj.IsTeamData)
                    {
                        //obj.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: obj.MemberList, Date: FDate, IsAscending:true, BranchId: BranchID);
                                       
                        obj.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: obj.ProjectMembersDetails as List<TeamMember>, Date: FDate, IsAscending: true, BranchId: BranchID);
                    }
                    else
                    {
                        DateTime ToDate = DateTime.ParseExact(form["DateTo"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        obj.EmployeeData = null;
                        obj.EmployeeData = TimeEntryHelper.GetSingleMemberData(EmpId: teamobj.MemberId, FromDate: FDate, ToDate: ToDate, IsAscending: true, BranchId: BranchID);
                        if (ToDate == FDate)
                        {
                            int count = Convert.ToInt32((ToDate - FDate).TotalDays + 1);
                            ViewData["totaldays"] = count;
                        }
                        else
                        {
                            int count = Convert.ToInt32((ToDate - FDate).TotalDays + 1);
                            ViewData["totaldays"] = count;
                        }

                    }
                }

                var holidaysCount = 0;
                double businessDays = 0;
                using (var dbhrms = new DSRCManagementSystemEntities1())
                {
                    var FromDate = Convert.ToDateTime(obj.DateFrom);
                    var ToDate = Convert.ToDateTime(obj.DateTo);
                    foreach (var item in dbhrms.Master_holiday.Where(holiday => holiday.Date >= FromDate && holiday.Date <= ToDate))
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
                    businessDays = 1 + ((ToDate - FromDate).TotalDays * 5 - (FromDate.DayOfWeek - ToDate.DayOfWeek) * 2) / 7 - holidaysCount;
                }
                ViewBag.businessDays = businessDays;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return View(obj);
        }

        
        private IList<ViewMembers> GetMembers(TeamEntryData objval, int PId = 0, int mId = 0)
        {
            List<ViewMembers> Members = new List<ViewMembers>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    if (PId == 0 && mId == 0)
                    {
                        {

                        }
                    }
                    else
                    {
                        Members = (from a in db.UserProjects
                                   join b in db.Users
                                   on a.UserID equals b.UserID
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID

                                   join d in db.Master_MemberTypes
                                   on a.MemberTypeID equals d.MemberTypeID

                                   where a.ProjectID == PId && b.IsActive == true

                                   select new ViewMembers()
                                   {
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,

                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,


                                   }).OrderBy(x => x.EmployeeName).ToList();
                    }
                }

                foreach (var item in Members)
                {
                    int id = Convert.ToInt32(System.Web.HttpContext.Current.Application["Month"]);
                    item.TotalWorkingHours = GetTotalHours(item.UserId, objval.DateFrom, objval.DateTo);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Members.OrderBy(x => x.TotalWorkingHours).ToList();
        }

        public static double GetTotalHours(int userID, string StartDate,string EndDate)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var UserDetails= db.Users.Where(u => u.UserID == userID).First();
                var employeeId = UserDetails.EmpID;
                var BranchID = UserDetails.BranchId;
                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                var year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var today = DateTime.Today;
                
                var startDate = Convert.ToDateTime(StartDate);
                var endDate =Convert.ToDateTime(EndDate);
              
                var totalDaysWorked =
                  db.TimeManagements.Where(
                      t =>
                          t.EmpID == employeeId && t.BranchId==BranchID &&  t.Date >= startDate && t.Date <= endDate).ToList();

               
               //var totalHoursWorked = totalDaysWorked.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).Sum(tm => tm.TotalTime);
               var totalHoursWorked = totalDaysWorked.Sum(tm => tm.TotalTime);
                var workingHour = Math.Floor(TimeSpan.FromMinutes(totalHoursWorked ?? 0).TotalHours);
                var blanceMinutes = (totalHoursWorked % 60) / 100.0;
                return Math.Round((double)((workingHour>=totalDaysWorked.Count()?workingHour-totalDaysWorked.Count: workingHour)+ blanceMinutes),2) ;
            }
        }
        private List<Project> LoadProjects()
        {
            List<Project> Projects = new List<Project>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    int userId = int.Parse(Session["UserID"].ToString());

                    int roleId = db.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();

                    if (roleId == 44)
                    {
                        var projectId = db.UserProjects.Where(x => x.UserID == userId).Select(x => x.ProjectID).ToList();

                        Projects = (from data in db.Projects
                                    where data.IsActive == true && projectId.Contains(data.ProjectID)
                                    select data).OrderBy(x => x.ProjectName).ToList();
                    }
                    //else if (roleId == 4 || roleId == 42 || roleId == 70)
                    //{
                    //    Projects = (from data in db.Projects
                    //                join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectID
                    //                where data.IsActive == true
                    //                select data).OrderBy(x => x.ProjectName).ToList();
                    //}
                    else{
                        Projects = (from data in db.Projects
                                    join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectID
                                    where data.IsActive == true
                                    select data).OrderBy(x => x.ProjectName).ToList();
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return Projects;
        }

        public ActionResult NonTechTeamTimeEntry()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                TeamEntryData model = new TeamEntryData();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                int BranchId = (int)objdb.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                model.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                model.IsTeamData = true;
                model.DateFrom = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.DateTo = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.EmployeeData = TimeEntryHelper.GetTeamMemberData1(teamMembers: model.MemberList, Date: DateTime.Today.AddDays(-1), BranchId: BranchId);
                MonthList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return View(model);
       }

        
        [HttpPost]
        public ActionResult NonTechTeamTimeEntry(TeamEntryData model, FormCollection form)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                MonthList();
                int userId = Convert.ToInt32(Session["UserID"]);
                int BranchId = (int)objdb.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                model.MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                model.IsTeamData = model.MemberId == "0";
                DateTime FromDate = DateTime.ParseExact(model.DateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (model.ListMonth == null)
                {
                    if (model.IsTeamData)
                        model.EmployeeData = TimeEntryHelper.GetTeamMemberData1(teamMembers: model.MemberList, Date: FromDate, BranchId: BranchId);
                    else
                    {
                        DateTime ToDate = DateTime.ParseExact(model.DateTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        model.EmployeeData = TimeEntryHelper.GetSingleMemberData(EmpId: model.MemberId, FromDate: FromDate, ToDate: ToDate, IsAscending: true, BranchId: BranchId);
                    }
                }
                else
                {
                    DateTime time = new DateTime(2015, Convert.ToInt32(model.ListMonth), 1);
                    var dtfrm = FirstDayOfMonthFromDateTime(time).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var dtTo = LastDayOfMonthFromDateTime(time).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime FrmDt = DateTime.ParseExact(dtfrm, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime ToDt = DateTime.ParseExact(dtTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    model.EmployeeData = TimeEntryHelper.GetSingleMemberData(EmpId: model.MemberId, FromDate: FrmDt, ToDate: ToDt, IsAscending: true, BranchId: BranchId);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return View(model);
        }


        public DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }


        public DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }

        public void MonthList()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var month = objdb.Master_TeamMonths.ToList();
            IList<SelectListItem> mon = new List<SelectListItem>();
            foreach (var s in month)
            {
                mon.Add(new SelectListItem { Value = Convert.ToString(s.Id), Text = s.Months });
            }
            ViewBag.Month1 = mon;
        }
    }
}









