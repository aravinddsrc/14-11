using System;

using System.Linq;
using System.Web;

using System.Collections.Generic;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Globalization;


namespace DSRCManagementSystem.Controllers
{
    public class LDAdminController : Controller
    {
        //
        // GET: /L&DAdmin/

        [HttpGet]
        public ActionResult LDAdmin()
        {
            List<DSRCManagementSystem.Models.LDAdminmodel> LDList = new List<DSRCManagementSystem.Models.LDAdminmodel>();
            try
            {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.LDAdminmodel ObjAC = new DSRCManagementSystem.Models.LDAdminmodel();

            var technologylist = db.Master_TrainingTechnology.ToList();
            ViewBag.Technology_list = new SelectList(new[] { new Master_TrainingTechnology() { TechnologyId = 0, TechnologyName = "---Select---" } }.Union(technologylist), "TechnologyId", "TechnologyName", 0);
            var statuslist = db.Master_TrainingStatus.ToList();
            ViewBag.Status_list = new SelectList(new[] { new Master_TrainingStatus() { StatusId = 0, StatusName = "---Select---" } }.Union(statuslist), "StatusId", "StatusName", 0);

            var query = (from a in db.Trainings
                         join i in db.Users on a.InstructorId equals i.UserID
                         join s in db.Master_TrainingStatus on a.StatusId equals s.StatusId

                         join t in db.Master_TrainingTechnology on a.TechnologyId equals t.TechnologyId
                         where  a.IsActive == true

                         select new DSRCManagementSystem.Models.LDAdminmodel()
                         {
                             TrainingID = a.TrainingId,
                             Technology_id = t.TechnologyId,
                             TrainingName = a.TrainingName,
                             Status_id = s.StatusId,
                             Status = s.StatusName,
                             Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName :"")),
                             Technologies = t.TechnologyName,
                           
                             scheduledate = a.ScheduledDate
                         }).OrderByDescending(o => o.scheduledate).ToList();
            foreach (var s in query)
            {
                LDAdminmodel s1 = new LDAdminmodel();
                s1.TrainingID = s.TrainingID;
                s1.TrainingName = s.TrainingName;
                s1.Status = s.Status;
                s1.Instructor = s.Instructor;
                if (s.Status == "Scheduled" || s.Status == "InProgress" || s.Status == "Initiated" || s.Status == "Rescheduled")
                {
                    s1.Nomination = db.Trainings.Where(x => x.TrainingId == s.TrainingID).FirstOrDefault().NumberOfNominated;
                }
                else if (s.Status == "Completed" || s.Status == "Feedback Pending")
                {
                    bool exist;
                    exist = db.TrainingCompletions.Any(x => x.TrainingId == s.TrainingID);
                    if (exist)
                    {
                        s1.Nomination = db.TrainingCompletions.Where(x => x.TrainingId == s.TrainingID).FirstOrDefault().Count;
                    }
                    else
                        s1.Nomination = 0;
                }

                s1.scheduledate = s.scheduledate;
                s1.Technologies = s.Technologies;
                LDList.Add(s1);
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(LDList);
        }




        [HttpPost]
        public ActionResult LDAdmin(LDAdminmodel model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            List<DSRCManagementSystem.Models.LDAdminmodel> LDList = new List<DSRCManagementSystem.Models.LDAdminmodel>();
            try
            {
            var statuslist = db.Master_TrainingStatus.ToList();
            ViewBag.Status_list = new SelectList(new[] { new Master_TrainingStatus() { StatusId = 0, StatusName = "---Select---" } }.Union(statuslist), "StatusId", "StatusName", 0);
            var technologylist = db.Master_TrainingTechnology.ToList();
            ViewBag.Technology_list = new SelectList(new[] { new Master_TrainingTechnology() { TechnologyId = 0, TechnologyName = "---Select---" } }.Union(technologylist), "TechnologyId", "TechnologyName", 0);

            var qq2 = (from e in db.Trainings
                       join i in db.Users on e.InstructorId equals i.UserID
                       join s in db.Master_TrainingStatus on e.StatusId equals s.StatusId
                       join d in db.Master_TrainingTechnology on e.TechnologyId equals d.TechnologyId
                       where e.IsActive == true
                       select new DSRCManagementSystem.Models.LDAdminmodel()
                       {
                           TrainingID = e.TrainingId,
                           Technology_id = d.TechnologyId,
                           TrainingName = e.TrainingName,
                           Status = s.StatusName,
                           Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                           Technologies = d.TechnologyName,
                           //Nomination = e.TrainingId,
                           scheduledate = e.ScheduledDate
                       }).ToList();
                model.From = model.From ?? "";
                model.To = model.To ?? "";
                var f1 = model.From.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                var f2 = model.To.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                DateTime? FDate = null;
                DateTime? TDate = null;

                if (f1.Length == 2)
                    FDate = GetFirstDayOfMonth(Convert.ToInt32(f1[0].ToString()), Convert.ToInt32(f1[1].ToString()));
                if (f2.Length == 2)
                    TDate = GetLastDayOfMonth(Convert.ToInt32(f2[0].ToString()), Convert.ToInt32(f2[1].ToString()));

                if (model.Status_id == 5 || model.Status_id == 6)
                {
                    qq2 = (from e in db.Trainings
                           join i in db.Users on e.InstructorId equals i.UserID
                           join s in db.Master_TrainingStatus on e.StatusId equals s.StatusId
                           join d in db.Master_TrainingTechnology on e.TechnologyId equals d.TechnologyId
                           where e.ScheduledDate >= (FDate ?? e.ScheduledDate) && e.ScheduledDate <= (TDate ?? e.ScheduledDate) && e.IsActive == false
                          && e.TechnologyId == (model.Technology_id == null || model.Technology_id == 0 ? e.TechnologyId : model.Technology_id)
                          && e.StatusId == (model.Status_id == null || model.Status_id == 0 ? e.StatusId : model.Status_id)

                           select new DSRCManagementSystem.Models.LDAdminmodel()
                           {
                               TrainingID = e.TrainingId,
                               Technology_id = d.TechnologyId,
                               TrainingName = e.TrainingName,
                               Status = s.StatusName,
                               Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                               Technologies = d.TechnologyName,
                              // Nomination = e.NumberOfNominated,
                               scheduledate = e.ScheduledDate
                           }).ToList();
                }

                else
                {
                    qq2 = (from e in db.Trainings
                           join i in db.Users on e.InstructorId equals i.UserID
                           join s in db.Master_TrainingStatus on e.StatusId equals s.StatusId
                           join d in db.Master_TrainingTechnology on e.TechnologyId equals d.TechnologyId
                           where e.ScheduledDate >= (FDate ?? e.ScheduledDate) && e.ScheduledDate <= (TDate ?? e.ScheduledDate) && e.IsActive == true
                          && e.TechnologyId == (model.Technology_id == null || model.Technology_id == 0 ? e.TechnologyId : model.Technology_id)
                          && e.StatusId == (model.Status_id == null || model.Status_id == 0 ? e.StatusId : model.Status_id)

                           select new DSRCManagementSystem.Models.LDAdminmodel()
                           {
                               TrainingID = e.TrainingId,
                               Technology_id = d.TechnologyId,
                               TrainingName = e.TrainingName,
                               Status = s.StatusName,
                               Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                               Technologies = d.TechnologyName,
                              // Nomination = e.NumberOfNominated,
                               scheduledate = e.ScheduledDate
                           }).ToList();
                }
                foreach (var s in qq2)
                {

                    LDAdminmodel s1 = new LDAdminmodel();
                    s1.TrainingID = s.TrainingID;
                    s1.TrainingName = s.TrainingName;
                    s1.Status = s.Status;
                    s1.Instructor = s.Instructor;
                    if (s.Status == "Scheduled" || s.Status == "InProgress" || s.Status == "Initiated" || s.Status == "Rescheduled" || s.Status == "Cancelled")
                    {
                        s1.Nomination = db.Trainings.Where(x => x.TrainingId == s.TrainingID).FirstOrDefault().NumberOfNominated;
                    }
                    else if (s.Status == "Completed" || s.Status == "Feedback Pending")
                    {
                        bool exist;
                        exist = db.TrainingCompletions.Any(x => x.TrainingId == s.TrainingID);
                        if (exist)
                        {
                            s1.Nomination = db.TrainingCompletions.Where(x => x.TrainingId == s.TrainingID).FirstOrDefault().Count;
                        }
                        else
                            s1.Nomination = 0;
                    }

                    s1.scheduledate = s.scheduledate;
                    s1.Technologies = s.Technologies;
                    LDList.Add(s1);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(LDList);
        }

        private DateTime GetFirstDayOfMonth(int iMonth, int Year)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime dtFrom = new DateTime(Year, iMonth, 1);

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));

            // return the first day of the month
            return dtFrom;
        }

        private DateTime GetLastDayOfMonth(int iMonth, int Year)
        {

            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime dtTo = new DateTime(Year, iMonth, 1);

            // overshoot the date by a month
            dtTo = dtTo.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the
            // previous month
            dtTo = dtTo.AddDays(-(dtTo.Day));

            // return the last day of the month
            return dtTo;

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





    }
}
