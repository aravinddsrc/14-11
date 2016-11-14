using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Threading.Tasks;
using System.Data.Objects;
using System.Web.Configuration;


namespace DSRCManagementSystem.Controllers
{
    public class LDTrainingController : Controller
    {
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult NominationClosed()
        {
            return View();
        }

        [HttpGet]
        public ActionResult InstructorNomination()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AlreadyNominated()
        {

            return View();
        }
        [HttpGet]
        public ActionResult AlreadyCancelled()
        {

            return View();
        }


        [HttpGet]
        public ActionResult AlreadyFeedback()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FeedbackClosed()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Nomination(int Id)
        {
            try
            {
             
                int userId = int.Parse(Session["UserID"].ToString());
              
                int TrainingId = Id;

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var empid = db.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();
                //int? eid = Convert.ToInt32(empid);

                int? isInstructor = db.Trainings.FirstOrDefault(o => o.TrainingId == TrainingId).InstructorId;

                var ValidNomination = db.Trainings.FirstOrDefault(o => o.TrainingId == TrainingId);

                if (ValidNomination.ScheduledDate.Value.Date <= DateTime.Now.Date)
                {
                    return RedirectToAction("CalendarEvents", "LDR");
                }

                else if (isInstructor == userId)
                {
                    return RedirectToAction("InstructorNomination");
                }
                else
                {
                    var val = db.TrainingNominations.Where(x => x.EmpId == empid && x.TrainingId == Id && x.IsActive == true).FirstOrDefault();
                    if (val != null)
                    {
                        return RedirectToAction("AlreadyNominated");
                    }
                    else
                    {

                        var purposeList = db.Master_TrainingPurpose.ToList();

                        ViewBag.PurposeList = new SelectList(new[] { new Master_TrainingPurpose() { PurposeId = 0, Purpose = "---Select---" } }.Union(purposeList), "PurposeId", "Purpose", 0);

                        var viewobj = (from t in db.Trainings
                                       join u in db.Users on t.InstructorId equals u.UserID
                                       where t.TrainingId == TrainingId
                                       join tt in db.Master_TrainingTechnology on t.TechnologyId equals tt.TechnologyId
                                       select new Nomination()
                                       {
                                           TrainingID = t.TrainingId,
                                           CourseName = t.TrainingName,
                                           InstructorId = t.InstructorId,
                                           InstructorName = u.FirstName + " " + (u.LastName ?? " "),
                                           Technology = tt.TechnologyName,
                                           endtime = t.EndTime,
                                           starttime = t.StartTime
                                       }).FirstOrDefault();

                        var detailsobj = db.Users.FirstOrDefault(o => o.UserID == userId);

                       // viewobj.EmpId = Convert.ToInt32(detailsobj.EmpID);
                        viewobj.EmpId = detailsobj.EmpID;
                        viewobj.EmpName = detailsobj.FirstName + " " + detailsobj.LastName;
                        ///viewobj.Email = detailsobj.EmailAddress;

                        viewobj.NominationCount = db.TrainingNominations.Where(o => o.TrainingId == TrainingId).Count();
                        viewobj.SeatingCapacity = db.Trainings.FirstOrDefault(o => o.TrainingId == TrainingId).SeatingCapacity;
                        viewobj.AvaliableSeats = viewobj.SeatingCapacity - viewobj.NominationCount;

                        if (viewobj.AvaliableSeats == -2)
                        {
                            return RedirectToAction("NominationClosed");
                        }
                        else
                        {
                            return View(viewobj);
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
            return View();
        }

        [HttpPost]
        public ActionResult Nomination(Nomination nominee)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = int.Parse(Session["UserID"].ToString());
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var Techref = db.Trainings.FirstOrDefault(o => o.TrainingId == nominee.TrainingID);
                var Supervisorref = db.UserReportings.FirstOrDefault(o => o.UserID == userId);
                nominee.SupervisorId = Supervisorref.ReportingUserID.ToString();
                var SupervisorDetails = db.Users.FirstOrDefault(o => o.UserID == Supervisorref.ReportingUserID);
                nominee.SupervisorName = SupervisorDetails.FirstName;
                nominee.Email = SupervisorDetails.EmailAddress;
                nominee.ContactNo = SupervisorDetails.ContactNo.ToString();

                var ReqToNominate = new TrainingNomination()
                {
                    TrainingId = nominee.TrainingID,
                    TechnologyId = Techref.TechnologyId,
                    EmpId = nominee.EmpId,
                    EmpName = nominee.EmpName,
                    UserId = userId,
                    SupervisorId = nominee.SupervisorId,
                    SupervisorName = nominee.SupervisorName,
                    EmailId = nominee.Email,
                    MobileNo = nominee.ContactNo,
                    NominationFlag = true,
                    CompletionFlag = true,
                    IsActive = true
                };

                db.TrainingNominations.AddObject(ReqToNominate);
                db.SaveChanges();

                var No_Of_Nominees_Ref = db.Trainings.FirstOrDefault(o => o.TrainingId == nominee.TrainingID);

                int? No_Of_Nominations = No_Of_Nominees_Ref.NumberOfNominated;
                No_Of_Nominees_Ref.NumberOfNominated = No_Of_Nominations + 1;

                db.SaveChanges();

                string Tid = Techref.TrainingId.ToString();
                string tname = Techref.TrainingName;
                DateTime d1 = Convert.ToDateTime(Techref.ScheduledDate);
                string d = d1.ToShortDateString();
                string stime = Techref.StartTime;
                string etime = Techref.EndTime;

                string username = db.Users.Where(o => o.UserID == Techref.InstructorId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                var userdetails = db.Users.FirstOrDefault(o => o.UserID == userId);
                string MailID = userdetails.EmailAddress;

                nominee.EmpName = userdetails.FirstName + " " + userdetails.LastName ?? "";



                //string mailMessage = MailBuilder.NominationConfirmation(nominee.EmpName, Tid, tname, d, stime, etime, username);
                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Nomination Confirmation").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Nomination Confirmation").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {

                         var objNominationConfirmation = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Nomination Confirmation")
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
                         string TemplatePathNominationConfirmation = Server.MapPath(objNominationConfirmation.Template);
                         string htmlNominationConfirmation = System.IO.File.ReadAllText(TemplatePathNominationConfirmation);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Empname", nominee.EmpName);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingId", Tid);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingName", tname);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ScheduledDate", d);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#start", stime);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#end", etime);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Instructor", username);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ServerName",ServerName);
                         htmlNominationConfirmation = htmlNominationConfirmation.Replace("#CompanyName", company);
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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
                                 //  DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                             });


                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 // DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));
                                 //DsrcMailSystem.MailSender.LDSendMail(null, "L & D - Training Nomination Confirmation", null, mailMessage, "TRAINING@dsrc.co.in", MailID, Server.MapPath("~/Content/Template/images/logo.png"));

                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Nomination Confirmation", folder, ServerName);
                     }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Feedback(int id)
        {
            DSRCManagementSystem.Models.FeedbackModel model = new DSRCManagementSystem.Models.FeedbackModel();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = int.Parse(Session["UserID"].ToString());
                var val = db.TrainingFeedBackCalcs.Where(o => o.TrainingId == id && o.UserId == userId && o.Flag == true).FirstOrDefault();
                //var valFeedback = db.TrainingFeedBackCalcs.Where(o => o.TrainingId == id && o.UserId == userId && o.Flag == false).FirstOrDefault();

                //DateTime? Feedbacksenton = valFeedback.FeedBackSentOn;
                //DateTime FeedbackClosed = Convert.ToDateTime(Feedbacksenton);

                //FeedbackClosed = FeedbackClosed.AddDays(3);
                //DateTime Today = DateTime.Now.Date;

                if (val != null)
                {
                    return RedirectToAction("AlreadyFeedback");
                }
                //else if (Today > FeedbackClosed)
                //{
                //    return RedirectToAction("FeedbackClosed");
                //}
                else
                {
                    var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
                    int Id = roleID.FirstOrDefault();
                    TempData["ID"] = id;
                    var ObjTraining = db.Trainings.FirstOrDefault(o => o.TrainingId == id);
                    var ObjUserName = db.Users.FirstOrDefault(o => o.UserID == ObjTraining.InstructorId);
                    model.CourseName = ObjTraining.TrainingName;
                    model.TrainingId = id;
                    model.CourseType = db.Master_TrainingType.FirstOrDefault(o => o.TrainingTypeId == ObjTraining.TrainingTypeId).TypeName;
                    model.Instructor = ObjUserName.FirstName + " " + (ObjUserName.LastName ?? "");
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
        [HttpPost]
        public ActionResult Feedback(FeedbackModel model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            int UserID = Convert.ToInt32(Session["UserID"]);

            int Id = Convert.ToInt32(model.TrainingId);

            try
            {
               // var Req_Feedback = db.TrainingFeedBackCalcs.CreateObject();
                //var Req_Feedback = db.TrainingFeedBackCalcs.FirstOrDefault(o => o.TrainingId == Id && o.UserId == UserID);
                var Req_Feedback = db.TrainingFeedBackCalcs.Where(o => o.TrainingId == Id && o.UserId == UserID).Select(o => o).FirstOrDefault();
                Req_Feedback.TrainingId = Id;
                Req_Feedback.UserId = UserID;
                Req_Feedback.ChoiceTopic = model.Choice;
                Req_Feedback.PgmRelevance = model.Relevance;
                Req_Feedback.ContentAmt = model.AmountofCourse;
                Req_Feedback.AdequacyLearning = model.AdeqofLearning;
                Req_Feedback.AdequacyPreparation = model.AdeqofPreparation;
                Req_Feedback.ExampleDemo = model.EXpofConcepts;
                Req_Feedback.ContentPresented = model.contentPres;
                Req_Feedback.TimeMaintanence = model.Timemain;
                Req_Feedback.Flag = true;
                Req_Feedback.LearntInPgm = model.Learn;
                Req_Feedback.FacultyQuality = model.Quality;
                Req_Feedback.PgmUseful = (model.PrgUseful == 1) ? true : false;
                Req_Feedback.Comments = model.Suggestions;

                int? total = model.Choice + model.Relevance + model.AmountofCourse + model.AdeqofLearning + model.AdeqofPreparation + model.EXpofConcepts + model.contentPres + model.Timemain + model.Quality;
                float overall = Convert.ToSingle(total) / (9 * 5);
                float overallperc = overall * 100;

                Req_Feedback.Overall = overall;
                Req_Feedback.OverallPerc = overallperc;
               // db.TrainingFeedBackCalcs.AddObject(Req_Feedback);
                db.SaveChanges();

                int Feedback_Status = db.TrainingFeedBackCalcs.Count(o => o.Flag == false && o.TrainingId == Id);

                if (Feedback_Status == 0)
                {
                    var TrainingRef = db.Trainings.FirstOrDefault(o => o.TrainingId == Id);

                    TrainingRef.StatusId = 6;
                    db.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }



       [HttpGet]
        public ActionResult TrainingConducted(string Id)
        {
            int ID = Convert.ToInt32(Id);
           ManageTrainingModel AsgnList = new ManageTrainingModel();
            try
            {
            int UserId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            AsgnList.CompletionResult = (from rc in db.Trainings
                                         join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                         join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                         join i in db.Users on rc.InstructorId equals i.UserID
                                         join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                         where rc.IsActive == true && rc.InstructorId == UserId && rc.TrainingId == ID
                                         //join c in db.TrainingCompletions on rc.TrainingId equals c.TrainingId                                          
                                         //where rc.IsActive == true && rc.InstructorId == UserId && n.Score == 0 && n.CompletionFlag==true && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                         //where rc.IsActive == true && rc.InstructorId == UserId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date && rc.TrainingId == ID
                                         select new CompletionResultModel()
                                         {
                                             TrainingID = rc.TrainingId,
                                             TrainingName = rc.TrainingName,
                                             Technology = t.TechnologyName,
                                             ScheduledDate = rc.ScheduledDate,
                                             IsCompleted = n.CompletionFlag,
                                             Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                             Starttime = rc.StartTime,
                                             submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                             pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),
                                             Endtime = rc.EndTime,
                                             Nominations = rc.NumberOfNominated
                                             //Attendees = (int)rc.NumberOfNominated

                                         }).Distinct().ToList();
           
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(AsgnList);
        }

       [HttpGet]
       public ActionResult TrainingFeedback()
       {
           ManageTrainingModel AsgnList = new ManageTrainingModel();
           try
           {
               int UserId = int.Parse(Session["UserID"].ToString());
               DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
               AsgnList.CompletionResult = (from rc in db.Trainings
                                            join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                            join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                            join i in db.Users on rc.InstructorId equals i.UserID
                                            join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                            //join c in db.TrainingCompletions on rc.TrainingId equals c.TrainingId                                          
                                            //where rc.IsActive == true && rc.InstructorId == UserId && n.Score == 0 && n.CompletionFlag==true && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                            where rc.IsActive == true && rc.InstructorId == UserId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                            select new CompletionResultModel()
                                            {
                                                TrainingID = rc.TrainingId,
                                                TrainingName = rc.TrainingName,
                                                Technology = t.TechnologyName,
                                                ScheduledDate = rc.ScheduledDate,
                                                IsCompleted = n.CompletionFlag,
                                                Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                                Starttime = rc.StartTime,
                                                submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                                pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),
                                                Endtime = rc.EndTime,
                                                Nominations = rc.NumberOfNominated,
                                                //Attendees = (int)rc.NumberOfNominated

                                            }).Distinct().ToList();


               AsgnList.FeedBackResult = (from rc in db.Trainings
                                          join f in db.TrainingFeedBackCalcs on rc.TrainingId equals f.TrainingId
                                          where f.Flag == false && rc.IsActive == true && f.UserId == UserId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date //&& EntityFunctions.TruncateTime(EntityFunctions.AddDays(f.FeedBackSentOn, 3)) >= DateTime.Today.Date
                                          select new FeedBackResultModel()
                                          {
                                              TrainingID = rc.TrainingId,
                                              TrainingName = rc.TrainingName

                                          }).Distinct().ToList();
           }
           catch (Exception Ex)
           {
               string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
               string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
               ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
           }
           return View(AsgnList);
       }

        

        [HttpGet]
        public ActionResult WorkList()
        {
            ManageTrainingModel AsgnList = new ManageTrainingModel();
            Session["ServerName"] = AppValue.GetFromMailAddress("ServerName");
            try
            {
            int UserId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            AsgnList.CompletionResult = (from rc in db.Trainings
                                         join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                         join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                         join i in db.Users on rc.InstructorId equals i.UserID
                                         join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                         //join c in db.TrainingCompletions on rc.TrainingId equals c.TrainingId                                          
                                         //where rc.IsActive == true && rc.InstructorId == UserId && n.Score == 0 && n.CompletionFlag==true && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                         where rc.IsActive == true && rc.InstructorId == UserId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                         select new CompletionResultModel()
                                         {
                                             TrainingID = rc.TrainingId,
                                             TrainingName = rc.TrainingName,
                                             Technology = t.TechnologyName,
                                             ScheduledDate = rc.ScheduledDate,
                                             IsCompleted = n.CompletionFlag,
                                             Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                             Starttime = rc.StartTime,
                                             submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                             pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),
                                             Endtime = rc.EndTime,
                                             Nominations = rc.NumberOfNominated,
                                             //Attendees = (int)rc.NumberOfNominated

                                         }).Distinct().ToList();


            AsgnList.FeedBackResult = (from rc in db.Trainings
                                       join f in db.TrainingFeedBackCalcs on rc.TrainingId equals f.TrainingId
                                       where f.Flag == false && rc.IsActive == true && f.UserId == UserId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date //&& EntityFunctions.TruncateTime(EntityFunctions.AddDays(f.FeedBackSentOn, 3)) >= DateTime.Today.Date
                                       select new FeedBackResultModel()
                                       {
                                           TrainingID = rc.TrainingId,
                                           TrainingName = rc.TrainingName

                                       }).Distinct().ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(AsgnList);
        }
    }
}
