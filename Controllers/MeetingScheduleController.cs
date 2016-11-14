using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Web.SessionState;
using System.Net.Mail;
using DSRCManagementSystem;
using System.Net;
using System.Web.Security;
using System.Text.RegularExpressions;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Management;
using System.Globalization;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Configuration;
using System.IO;
using System.Reflection;

namespace DSRCManagementSystem.Controllers
{
    public class MeetingScheduleController : Controller
    {
        //
        // GET: /MeetingSchedule/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProjectAgenda(int id)
        {
            DSRCManagementSystem.Models.AgandaForProject objagenda = new DSRCManagementSystem.Models.AgandaForProject();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string value = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.Agenda).FirstOrDefault();
            if (value != null)
            {
                DateTime? date = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.AgendaDate).FirstOrDefault();
                DateTime? current = System.DateTime.Now;
                TimeSpan difference = current.Value - date.Value;
                double k = difference.TotalDays;
                int j = Convert.ToInt32(k);
                if (id == 47)
                {
                    if (value != null && j >= 7)
                    {
                        value = null;
                        objagenda.ProjectAganda = value;
                    }
                    else if (value != null && j < 7)
                    {
                        objagenda.ProjectAganda = value;

                    }
                    else
                    {
                        objagenda.ProjectAganda = "";
                    }
                }
                else
                {
                    if (value != null && j >= 13)
                    {
                        value = null;
                        objagenda.ProjectAganda = value;
                    }
                    else if (value != null && j < 13)
                    {
                        objagenda.ProjectAganda = value;
                    }
                    else
                    {
                        objagenda.ProjectAganda = "";
                    }
                }
            }
            else
            {
                objagenda.ProjectAganda = "";
            }


            List<DSRCManagementSystem.Models.Historylist> objlist = new List<DSRCManagementSystem.Models.Historylist>();
            objlist = (from p in objdb.AgendaFeedbacks
                       select new DSRCManagementSystem.Models.Historylist
                       {
                           ProjectId = p.ProjectId,
                           agenda = p.Agenda,
                           feedback = p.Feedback
                       }).ToList();

            int i = objlist.Count();
            int? project = objlist.Select(o => o.ProjectId).FirstOrDefault();
            TempData["project"] = project;
            TempData["Count"] = i;

            System.Web.HttpContext.Current.Application["agenda"] = id;
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectAgenda(AgandaForProject objagenda)
        {
            objagenda.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Application["agenda"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var value = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.Feedback).FirstOrDefault();
            var agenda = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.Agenda).FirstOrDefault();
            var mom = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(x => x.MOM).FirstOrDefault();

            if (value != null && Convert.ToInt32(TempData["Count"]) == 0)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                db.AddToAgendaFeedbacks(obj);
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda != null)
            {

                DSRCManagementSystemEntities1 oho = new DSRCManagementSystemEntities1();
                var valuefed = oho.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                valuefed.Agenda = objagenda.ProjectAganda.ToString();
                valuefed.AgendaDate = System.DateTime.Now;
                oho.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }



            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda == null && mom != null)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var val = db.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                val.ProjectId = objagenda.UserId;
                val.Agenda = objagenda.ProjectAganda.ToString();
                val.AgendaDate = System.DateTime.Now;
                db.SaveChanges();

                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && agenda == null && mom == null)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                db.AddToAgendaFeedbacks(obj);
                db.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 oh = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Agenda = objagenda.ProjectAganda;
                obj.AgendaDate = System.DateTime.Now;
                obj.ProjectId = objagenda.UserId;
                oh.AddToAgendaFeedbacks(obj);
                oh.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (value != null && Convert.ToInt32(TempData["Count"]) != 0)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var valuefed = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objagenda.UserId).Select(o => o).FirstOrDefault();
                valuefed.Agenda = objagenda.ProjectAganda.ToString();

                valuefed.AgendaDate = System.DateTime.Now;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }


            return View();
        }


        [HttpGet]
        public ActionResult ProjectFeedBack(int id)
        {
            DSRCManagementSystem.Models.ProjectFeedBack objagenda = new DSRCManagementSystem.Models.ProjectFeedBack();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string value = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.Feedback).FirstOrDefault();
            if (value != null)
            {
                DateTime? date = objdb.AgendaFeedbacks.Where(o => o.ProjectId == id).Select(o => o.FeedbackDate).FirstOrDefault();
                DateTime? current = System.DateTime.Now;
                TimeSpan difference = current.Value - date.Value;
                double k = difference.TotalDays;
                int j = Convert.ToInt32(k);
                if (id == 47)
                {
                    if (value != null && j >= 7)
                    {
                        value = null;
                        objagenda.Feedback = value;
                    }
                    else if (value != null && j < 7)
                    {
                        objagenda.Feedback = value;

                    }
                    else
                    {
                        objagenda.Feedback = "";
                    }
                }
                else
                {
                    if (value != null && j >= 13)
                    {
                        value = null;
                        objagenda.Feedback = value;
                    }
                    else if (value != null && j < 13)
                    {
                        objagenda.Feedback = value;
                    }
                    else
                    {
                        objagenda.Feedback = "";
                    }
                }
            }
            else
            {
                objagenda.Feedback = "";
            }


            List<DSRCManagementSystem.Models.Historylist> objlist = new List<DSRCManagementSystem.Models.Historylist>();
            objlist = (from p in objdb.AgendaFeedbacks
                       select new DSRCManagementSystem.Models.Historylist
                       {
                           agenda = p.Agenda,
                           feedback = p.Feedback
                       }).ToList();

            int i = objlist.Count();
            TempData["Count"] = i;

            System.Web.HttpContext.Current.Application["id"] = id;
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectFeedBack(ProjectFeedBack ovj, AgandaForProject objagenda)
        {
            ovj.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Application["id"]);


            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();

            var value = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.Agenda).FirstOrDefault();
            var feedback = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.Feedback).FirstOrDefault();
            var mom = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(x => x.MOM).FirstOrDefault();
            if (value != null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var valuefed = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                valuefed.Feedback = ovj.Feedback;
                valuefed.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback != null && mom != null)
            {

                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var val = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                val.Feedback = ovj.Feedback;
                val.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }


            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback == null && mom != null)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                var val = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.MOMDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                val.ProjectId = ovj.UserId;
                val.Feedback = ovj.Feedback;
                val.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) != 0 && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Feedback = ovj.Feedback;
                obj.FeedbackDate = System.DateTime.Now;
                obj.ProjectId = ovj.UserId;
                objdb.AddToAgendaFeedbacks(obj);
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value == null && Convert.ToInt32(TempData["Count"]) == 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback obj = new DSRCManagementSystem.AgendaFeedback();
                obj.Feedback = ovj.Feedback;
                obj.FeedbackDate = System.DateTime.Now;
                obj.ProjectId = ovj.UserId;
                objdb.AddToAgendaFeedbacks(obj);
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

            else if (value != null && Convert.ToInt32(TempData["Count"]) != 0)
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var valuefed = objdb.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == ovj.UserId).Select(o => o).FirstOrDefault();
                valuefed.Feedback = ovj.Feedback;
                valuefed.FeedbackDate = System.DateTime.Now;
                objdb.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }
            return View();

        }
        [HttpGet]
        public ActionResult MOM(int id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ProjectMom objmom = new DSRCManagementSystem.Models.ProjectMom();
            var value = objdb.AgendaFeedbacks.Where(x => x.ProjectId == id).Select(o => o.MOM).FirstOrDefault();

            if (value != null)
            {
                objmom.ProjectMOM = value.ToString();
                objmom.Date = Convert.ToString(System.DateTime.Now);
            }
            else
            {
                objmom.ProjectMOM = "";
            }
            System.Web.HttpContext.Current.Application["agenda"] = id;
            return View(objmom);
        }
        [HttpPost]
        public ActionResult MOM(ProjectMom objmom)
        {
            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();
            objmom.ProjectId = Convert.ToInt32(System.Web.HttpContext.Current.Application["agenda"]);
            var agenda = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.Agenda).FirstOrDefault();
            var feedback = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.Feedback).FirstOrDefault();
            var mom = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x.MOM).FirstOrDefault();
            if (agenda != null && feedback != null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var fed = ob.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                fed.MOM = objmom.ProjectMOM;
                fed.ProjectId = objmom.ProjectId;
                fed.MOMDate = System.DateTime.Now;
                ob.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda == null && feedback != null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var age = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.FeedbackDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                age.MOM = objmom.ProjectMOM;
                age.MOMDate = System.DateTime.Now;
                age.ProjectId = objmom.ProjectId;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda != null && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var age = obj.AgendaFeedbacks.Where(x => EntityFunctions.TruncateTime(x.AgendaDate) == DateTime.Today.Date && x.ProjectId == objmom.ProjectId).Select(x => x).FirstOrDefault();
                age.MOM = objmom.ProjectMOM;
                age.MOMDate = System.DateTime.Now;
                age.ProjectId = objmom.ProjectId;
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda == null && feedback == null && mom == null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback objd = new DSRCManagementSystem.AgendaFeedback();
                objd.MOM = objmom.ProjectMOM;
                objd.ProjectId = objmom.ProjectId;
                objd.MOMDate = System.DateTime.Now;
                obj.AddToAgendaFeedbacks(objd);
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda == null && feedback == null && mom != null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback objd = new DSRCManagementSystem.AgendaFeedback();
                objd.MOM = objmom.ProjectMOM;
                objd.ProjectId = objmom.ProjectId;
                objd.MOMDate = System.DateTime.Now;
                obj.AddToAgendaFeedbacks(objd);
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else if (agenda != null && feedback != null && mom != null)
            {
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.AgendaFeedback objd = new DSRCManagementSystem.AgendaFeedback();
                objd.MOM = objmom.ProjectMOM;
                objd.ProjectId = objmom.ProjectId;
                objd.MOMDate = System.DateTime.Now;
                obj.AddToAgendaFeedbacks(objd);
                obj.SaveChanges();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }
        [HttpGet]

        public ActionResult ScheduleAttendees()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = Convert.ToInt32(Session["UserID"]);
                var FilteredUsers =
                                      db.Users.Where(
                                          u => u.IsActive == true && u.UserStatus != 6)
                                          .Select(x => x.UserID)
                                          .ToList()
                                          .
                                          Except(
                                              db.MeetingGuids
                                                  .Select(x => x.UserId.Value)
                                                  .ToList()).ToList();
                List<object> UnAuthUsers = new List<object>();
                foreach (int users in FilteredUsers)
                {
                    UnAuthUsers.AddRange(
                        db.Users.Where(u => u.UserID == users)
                            .Select(u => new { userid = u.UserID, username = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "") })
                            .ToList());
                }
                ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");
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
        public ActionResult ScheduleAttendees(ProjectMom model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            
                int value = Convert.ToInt32(model.Attendee);
                var check = db.MeetingGuids.Where(x => x.UserId == value).Select(o => o.Id).FirstOrDefault();
                var asobj = db.MeetingGuids.CreateObject();
                if (check == 0)
                {
                    asobj.UserId = value;
                    var fname = db.Users.Where(x => x.UserID == value).Select(o => o.FirstName + ""+(o.LastName ?? "")).FirstOrDefault();
                    asobj.FirstName = fname;
                    db.MeetingGuids.AddObject(asobj);
                    db.SaveChanges();
                }
            
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProjectMeeting()
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //  var ProjectList = objdb.Projects.Where(x=>x.IsActive==true).ToList();
            int userId = Convert.ToInt32(Session["UserID"]);
            var ProjectList = (from p in objdb.Projects
                               join pt in objdb.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                               join up in objdb.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                               where p.IsActive == true && (p.IsDeleted != true || p.IsDeleted == null)
                               select new Projects()
                               {
                                   ProjectID = p.ProjectID,
                                   ProjectName = p.ProjectName,
                                   ProjectCode = p.ProjectCode,
                                   ProjectType = pt.ProjectTypeName,
                                   RAGStatus = p.RAGStatus,
                                   RAGComments = p.RAGComments ?? "Comments not added",
                                   CommentsCreated = p.CommentsCreated,
                                   //MemberTypeID = up.MemberTypeID
                               }).Distinct().ToList();

            ProjectMeetingTime objtime = new ProjectMeetingTime();



            var ProjectLead = (from t in objdb.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                               join atn in objdb.MeetingGuids on t.UserID equals atn.UserId

                               select new
                               {
                                   Id = t.UserID,

                                   FirstName = t.FirstName + " " + (t.LastName.Length > 0 ? t.LastName : ""),
                                   LastName = t.LastName,


                               }).ToList();


            var Days = objdb.Master_Days.ToList();


            ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id", "FirstName", "LastName");
            ViewBag.DayList = new SelectList(new[] { new Master_Days() { Id = 0, Days = "--Select--" } }.Union(Days), "Id", "Days", 0);
            ViewBag.Projects = new SelectList(ProjectList, "ProjectID", "ProjectName");



            ViewBag.Week = new SelectList(new[] { new { Text = "--Select--", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);



            return View();



        }

        [HttpPost]

        public ActionResult ProjectMeeting(ProjectMeetingTime objmeeting)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            var Message = "";

            var already = objdb.MettingSchedules.Where(o => o.Week == objmeeting.Week && o.Day == objmeeting.Day && o.TimeSlot == objmeeting.TimeSlotFrom).Select(i => i.ProjectID).FirstOrDefault();

            if (already != null)
            {
                return Json(new { Result = "AlreadyExist", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }


            var time = Convert.ToDateTime(objmeeting.TimeSlotFrom);
            var hour = Convert.ToInt32(time.Hour);
            var min = Convert.ToInt32(time.Minute);
            var tt = time.ToString("tt");



            //  List<string> obj = new List<string>();
            var obj = objdb.MettingSchedules.Where(o => o.Week == objmeeting.Week && o.Day == objmeeting.Day).Select(o => o).ToList();
            TimeSpan CurTime = new TimeSpan(hour, min, 0);


            foreach (var item in obj)
            {
                var dbtime = Convert.ToDateTime(item.TimeSlot);
                var hourdb = Convert.ToInt32(dbtime.Hour);
                var mindb = Convert.ToInt32(dbtime.Minute);
                var ttdb = dbtime.ToString("tt");

                TimeSpan Db_StartTime = new TimeSpan(hourdb, mindb, 0);


                var dbendtime = Convert.ToDateTime(item.EndTime);
                var hourenddb = Convert.ToInt32(dbendtime.Hour);
                var minenddb = Convert.ToInt32(dbendtime.Minute);
                var endttdb = dbendtime.ToString("tt");

                TimeSpan Db_EndTime = new TimeSpan(hourenddb, minenddb, 0);

                if (Db_StartTime == CurTime)
                {
                    Message = "availabletime";

                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }

                else if (Db_StartTime >= CurTime && CurTime < Db_EndTime)
                {
                    Message = "availabletime";
                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }
                else if (Db_StartTime <= CurTime && CurTime <= Db_EndTime)
                {
                    Message = "availabletime";
                    return Json(new { Result = Message }, JsonRequestBehavior.AllowGet);
                }
            }






            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.MettingSchedule objdetail = new DSRCManagementSystem.MettingSchedule();
            objdetail.ProjectID = Convert.ToInt32(objmeeting.ProjectNameId);
            objdetail.TimeSlot = objmeeting.TimeSlotFrom;
            objdetail.EndTime = objmeeting.TimeSlotTo;
            objdetail.Day = objmeeting.Day;

            objdetail.Week = objmeeting.Week;
            objdetail.Attendees = objmeeting.Attendee;

            db.AddToMettingSchedules(objdetail);
            db.SaveChanges();

            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);



        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear, System.Globalization.CultureInfo ci)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            DateTime firstWeekDay = jan1.AddDays(daysOffset);
            int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1 || firstWeek > 50)
            {
                weekOfYear -= 1;
            }
            return firstWeekDay.AddDays(weekOfYear * 7);
        }

        [HttpGet]
        public ActionResult EditAttendee(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.MeetingSchedule editobj = new DSRCManagementSystem.Models.MeetingSchedule();
            editobj.Id = ID;

            var ProjectLead = (from t in db.Users.Where(x => x.IsActive == true)
                               join atn in db.MeetingGuids on t.UserID equals atn.UserId

                               select new
                               {
                                   Id = t.UserID,
                                   FirstName = t.FirstName + " " + (t.LastName.Length > 0 ? t.LastName : "")
                               }).ToList();

            var AttendeeList = (from a in db.MettingSchedules
                                where a.Id == ID
                                select new MeetingSchedule()
                                {
                                    Attendees = a.Attendees
                                }).FirstOrDefault();

            List<int> selectedAttendees = new List<int>();

            if (AttendeeList.Attendees != null)
            {

                string[] tokens = AttendeeList.Attendees.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var i in tokens)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedAttendees.Add(val);
                }
            }

            ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id", "FirstName", selectedAttendees);

            return View(editobj);
        }

        [HttpPost]
        public ActionResult EditAttendee(int Id, string Attendee)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == Id);

            ReqToEdit.Attendees = Attendee;
            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult MeetingSchedule()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = Convert.ToInt32(Session["UserID"]);
            var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();

            var date = DateTime.Now;

            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            var result = (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;

            ViewBag.Week = new SelectList(new[] { new { Text = "--Select--", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);


            DSRCManagementSystem.Models.MeetingSchedule objmeeting = new DSRCManagementSystem.Models.MeetingSchedule();




            var firstdateofweek = DateTime.Now;
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            Dictionary<string, DateTime> currentWeek = new Dictionary<string, DateTime>();
            Dictionary<string, DateTime> nextWeek = new Dictionary<string, DateTime>();

            var weekofYear = cal.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);
            firstdateofweek = FirstDateOfWeek(DateTime.Now.Year, weekofYear, CultureInfo.CurrentCulture);
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

                List<DSRCManagementSystem.Models.MeetingSchedule> objmail = new List<DSRCManagementSystem.Models.MeetingSchedule>();

                if (getBracnch != 1)
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               where proj.ProjectID == 0
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
                        meetingSchedule.Attendees = MeetingScheduleController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    objmail = (from metting_schedule in db.MettingSchedules
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
                        meetingSchedule.Attendees = MeetingScheduleController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                return View(objmail);
            }
            else
            {

                List<DSRCManagementSystem.Models.MeetingSchedule> objmail = new List<DSRCManagementSystem.Models.MeetingSchedule>();

                if (getBracnch != 1)
                {
                    objmail = (from metting_schedule in db.MettingSchedules
                               join proj in db.Projects on metting_schedule.ProjectID equals proj.ProjectID

                               join days in db.Master_Days on metting_schedule.Day equals days.Days
                               where proj.ProjectID == 0
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
                        meetingSchedule.Attendees = MeetingScheduleController.GetUserString(db, meetingSchedule.Attendees);
                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    objmail = (from metting_schedule in db.MettingSchedules
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

                               }).OrderBy(x => x.Week).ThenBy(x => x.DayId).ThenBy(x => x.From).ToList();

                    foreach (var meetingSchedule in objmail)
                    {
                        meetingSchedule.Attendees = MeetingScheduleController.GetUserString(db, meetingSchedule.Attendees);

                        if (result % 2 == meetingSchedule.Week / 2)
                            meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        else
                            meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                    }
                }

                return View(objmail);
            }

        }




        [HttpGet]
        public ActionResult History(int ProjectId)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.History> objhis = new List<DSRCManagementSystem.Models.History>();

            var agendadate = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.AgendaDate).FirstOrDefault();

            var feeddate = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.FeedbackDate).FirstOrDefault();

            var mom = objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId).Select(x => x.MOMDate).FirstOrDefault();

            if (agendadate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.AgendaDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }
            else if (feeddate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.FeedbackDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }

            else if (feeddate != null && agendadate != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.FeedbackDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }

            else if (feeddate == null && agendadate == null && mom != null)
            {
                objhis = (from p in objdb.AgendaFeedbacks.Where(x => x.ProjectId == ProjectId)
                          select new DSRCManagementSystem.Models.History
                          {
                              Agenda = p.Agenda,
                              Feedback = p.Feedback,
                              Date = p.MOMDate,
                              MOM = p.MOM
                          }).OrderByDescending(x => x.Date).ToList();
            }
            return View(objhis);


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

        private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => user.IsActive == true && lst.Contains(user.UserID)) select user.EmailAddress).ToList();
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



        //[AcceptVerbs(HttpVerbs.Get)]
        //public ActionResult GetSearch(int Search)
        //{

        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        //    var check = db.MeetingGuids.Where(x => x.UserId == Search).Select(o => o.Id).FirstOrDefault();


        //    if (check != 0)
        //    {

        //        return Json("Success", JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {

        //        return Json("Failure", JsonRequestBehavior.AllowGet);
        //    }
        //}


    }
}
