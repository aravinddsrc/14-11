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

namespace DSRCManagementSystem.Controllers
{
    public class LDRController : Controller
    {
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult Language()
        {
            List<DSRCManagementSystem.Models.Language> objler = new List<DSRCManagementSystem.Models.Language>();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.CompletedTraining objcom = new DSRCManagementSystem.Models.CompletedTraining();
            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            int? userid = objdb.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int? roleid = objdb.UserRoles.Where(x => x.UserID == userid).Select(o => o.RoleID).FirstOrDefault();
            int? instructorid = objdb.Trainings.Where(x => x.InstructorId == userid).Select(o => o.InstructorId).FirstOrDefault();
             var language = objdb.Trainings.Where(x => x.InstructorId == instructorid).Select(o => o.TrainingName).ToList();
            objler = (from p in objdb.Trainings.Where(x => x.InstructorId == instructorid)
                      select new DSRCManagementSystem.Models.Language
                      {
                          language = p.TrainingName
                     }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objler);
        }

        [HttpGet]
        public ActionResult Form(int id)
        {
            Session["TrainingId"] = id;
            DSRCManagementSystem.Models.CompletedTraining objcom = new DSRCManagementSystem.Models.CompletedTraining();
            try
            {
                
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int UserId = int.Parse(Session["UserID"].ToString());
            objcom = (from p in objdb.Trainings.Where(o => o.TrainingId == id)
                      select new DSRCManagementSystem.Models.CompletedTraining
                      {
                          TrainingId = p.TrainingId,
                          Name = p.TrainingName,
                          Schedule = p.ScheduledDate

                      }).FirstOrDefault();

            DateTime d1 = Convert.ToDateTime(objcom.Schedule);
            string d = d1.ToShortDateString();
            objcom.ScheduleDate = d;

            var categories = (from p in objdb.Trainings
                              join t in objdb.TrainingNominations on p.TrainingId equals t.TrainingId
                              where p.InstructorId == UserId && p.TrainingId==id && t.IsActive==true
                              select new
                              {
                                  //CategoryID = t.EmpId,
                                  CategoryID = t.UserId,
                                  CategoryName = t.EmpName
                              }).ToList();

            ViewBag.Categories = new MultiSelectList(categories, "CategoryID", "CategoryName");

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objcom);
        }


        [HttpPost]
        public ActionResult Form(CompletedTraining objcom, List<int> Attendess, List<int> Unattendess) //, List<string> LAttendess, List<string> LUnattendess)
        {
            try
            {
             string ServerName = AppValue.GetFromMailAddress("ServerName");
              DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int count = Attendess.Count();
            int count1 = Unattendess.Count();
            DSRCManagementSystem.TrainingCompletion obj = new DSRCManagementSystem.TrainingCompletion();

            List<string> MailIds = new List<string>();
            List<string> Unmailids = new List<string>();

            int? userid = int.Parse(Session["UserID"].ToString());
            int? instructorid = objdb.Trainings.Where(x => x.TrainingId == objcom.TrainingId).Select(o => o.InstructorId).FirstOrDefault();
            int? trainingid = objcom.TrainingId;
          //  var nominee = Null;


            Session["Instructorid"] = instructorid;

            int? level = objdb.Trainings.Where(x => x.InstructorId == instructorid).Select(o => o.LevelId).FirstOrDefault();

            var value = objdb.TrainingWeightages.Where(x => x.LevelId == level).Select(o => o.Trainee).FirstOrDefault();

            string Feedbacksenton = DateTime.Now.ToShortDateString();

            var nominee1 = objdb.TrainingNominations.Where(x => x.TrainingId == trainingid).Select(o => o).ToList();
            nominee1.ForEach(o => o.CompletionFlag = false);
            objdb.SaveChanges();

           foreach (int Eid in Attendess)            
            {
              // var EId1 = Convert.ToString(Eid);
            //    var EmpID ="";
               
            //        if (EId1.Length == 1)
            //        {
            //                EmpID = "0000" + EId1;
            //        }
            //       if (EId1.Length == 2)
            //        {
            //                EmpID = "000" + EId1;
            //        }
            //       if (EId1.Length == 3)
            //        {
            //                EmpID = "00" + EId1;
            //        }
            //      if (EId1.Length == 4)
            //        {
            //                EmpID = "0" + EId1;
            //        }
            //      if (EId1.Length == 5)
            //        {
            //                EmpID =  EId1;
            //        }
                  //if (EId1.Length == 1)
                  //  {
                  //          EmpID = "0000" + EId1;
                  //  }
                  // if (EId1.Length == 1)
                  //  {
                  //          EmpID = "0000" + EId1;
                  //  }
                 // var nominee = objdb.TrainingNominations.Where(x => x.TrainingId == trainingid && x.EmpId == EmpID).Select(o => o).FirstOrDefault();
                if (Eid != 0)
                {
                    var nominee = objdb.TrainingNominations.Where(x => x.TrainingId == trainingid && x.UserId == Eid).Select(o => o).FirstOrDefault();

                    if (value != null)
                    {
                        nominee.Score = Convert.ToInt32(value);
                        objdb.SaveChanges();
                    }

                    //  string EMPID = Convert.ToString(Eid);

                    //if (EMPID.Length == 4)
                    //{
                    //    EMPID = "0" + EMPID;
                    //}

                    //var user = objdb.Users.FirstOrDefault(o => o.EmpID == ).UserID;

                    //  var user = objdb.Users.Where(x => x.EmpID == EmpID).Select(x => x.UserID).FirstOrDefault();

                    var user = objdb.Users.Where(x => x.UserID == Eid).Select(x => x.UserID).FirstOrDefault();



                    // To avoid duplicate trainingID and UserID combination in training feedback calculation.
                    var CheckUser = objdb.TrainingFeedBackCalcs.FirstOrDefault(o => o.TrainingId == nominee.TrainingId && o.UserId == user);

                    if (CheckUser == null)
                    {
                        var RequestToFeedback = new TrainingFeedBackCalc
                        {
                            TrainingId = nominee.TrainingId,
                            UserId = user,
                            ChoiceTopic = 0,
                            PgmRelevance = 0,
                            ContentAmt = 0,
                            AdequacyLearning = 0,
                            AdequacyPreparation = 0,
                            ExampleDemo = 0,
                            ContentPresented = 0,
                            TimeMaintanence = 0,
                            FacultyQuality = 0,
                            PgmUseful = false,
                            LearntInPgm = "Not given yet",
                            Comments = "Not given yet",
                            FeedBackSentOn = Convert.ToDateTime(Feedbacksenton),
                            Flag = false
                        };
                        objdb.TrainingFeedBackCalcs.AddObject(RequestToFeedback);
                        objdb.SaveChanges();
                    }
                }
                else
                {


                }
            }

           foreach (int UEid in Unattendess)
           {
               //if (UEid != 0)
               //{
               //    var EmpID = Convert.ToString(UEid);

               //    if (EmpID.Length == 1)
               //    {
               //        EmpID = "0000" + EmpID;
               //    }
               //    if (EmpID.Length == 2)
               //    {
               //        EmpID = "000" + EmpID;
               //    }
               //    if (EmpID.Length == 3)
               //    {
               //        EmpID = "00" + EmpID;
               //    }
               //    if (EmpID.Length == 4)
               //    {
               //        EmpID = "0" + EmpID;
               //    }
               //if (EmpID.Length == 5)
               //{
               //    EmpID = EmpID;
               //}
               //var nominee2 = objdb.TrainingNominations.Where(x => x.TrainingId == trainingid && x.EmpId == UEid).Select(o => o).FirstOrDefault();
               if (UEid != 0)
               {
                   if (value != null)
                   {
                       var nominee2 = objdb.TrainingNominations.Where(x => x.TrainingId == trainingid && x.UserId == UEid).Select(o => o).FirstOrDefault();
                       nominee2.Score = -(Convert.ToInt32(value));
                       objdb.SaveChanges();
                   }

                   //}


                   int id = Convert.ToInt32(trainingid);

                   var RequestToFeedbackAgg = new TrainingFeedbackAggregate
                   {
                       TrainingId = id,
                       TopicRating = 0,
                       RelevanceRating = 0,
                       CntAmtRating = 0,
                       LearnRating = 0,
                       PrepareRating = 0,
                       EgDemoRating = 0,
                       CntPresentRating = 0,
                       TimeRating = 0,
                       QualityRating = 0,
                       ContentRating = 0,
                       PresentRating = 0,
                       FacultyQRating = 0,
                       OverAllRating = 0,
                       No_Of_Attendees = 0

                   };

                   objdb.TrainingFeedbackAggregates.AddObject(RequestToFeedbackAgg);
                   objdb.SaveChanges();

               }
           }
            /******In trainings after submitting completed the status is changed as Feedback pending not as completed. 
            dont change this status will effect on worklist mand Manage training*********/
            var valuefed = objdb.Trainings.Where(x => x.TrainingId == trainingid).Select(o => o).FirstOrDefault();
            valuefed.StatusId = 3;
            objdb.SaveChanges();

            string temp = "";

            foreach (var num in Attendess)
            {
                temp += num + ",";
            }
            string temp1 = "";
            foreach (var num1 in Unattendess)
            {
                temp1 += num1 + ",";
            }

            int u = Attendess.Count();
            int v = Unattendess.Count();

            DSRCManagementSystem.TrainingCompletion com = new DSRCManagementSystem.TrainingCompletion();
            com.TrainingId = objcom.TrainingId;
            com.TrainingName = objcom.Name;
            com.Date = DateTime.Now;
            com.Attendece = obj.Attendece;
            if (objcom.upload == null)
            {
                com.Material = "";
                objdb.SaveChanges();
            }
            else
            {
                com.Material = objcom.upload;
                objdb.SaveChanges();
            }
            com.Count = u;
            com.IsActive = false;
            objdb.AddToTrainingCompletions(com);
            objdb.SaveChanges();

            int empid;

            for (int i = 0; i < count; i++)
            {
               //empid = Convert.ToString(Attendess[i]);
               // empid = "0" + empid;

               // var Mail = objdb.Users.Where(x => x.EmpID == empid).Select(o => o.EmailAddress).FirstOrDefault();
                empid = Attendess[i];
                var Mail = objdb.Users.Where(x => x.UserID == empid).Select(o => o.EmailAddress).FirstOrDefault();

                MailIds.Add(Mail);
            }

            for (int j = 0; j < count1; j++)
            {
                empid = Unattendess[j];

               // if (empid.Length == 4)
                  //  empid = "0" + empid;

               // var Mail = objdb.Users.Where(x => x.EmpID == empid).Select(o => o.EmailAddress).FirstOrDefault();
                var Mail = objdb.Users.Where(x => x.UserID == empid).Select(o => o.EmailAddress).FirstOrDefault();

                Unmailids.Add(Mail);
            }
            int k = MailIds.Count();
            int l = Unmailids.Count();

            var TrainingDetails = objdb.Trainings.FirstOrDefault(o => o.TrainingId == trainingid);

            string Instructor = objdb.Users.Where(o => o.UserID == instructorid).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

            string completion = objcom.upload;

            System.Web.HttpContext.Current.Application["Training"] = trainingid.ToString();

            int c = Convert.ToInt32(System.Web.HttpContext.Current.Application["Training"]);

            DateTime? d1 = objdb.Trainings.FirstOrDefault(o => o.TrainingId == trainingid).ScheduledDate;
            DateTime d2 = Convert.ToDateTime(d1);
            string d = d2.ToShortDateString();
           // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
            var logo = CommonLogic.getLogoPath();

            if (Unmailids == null) // addedd on 10.10
            {
                

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Training Absentees").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Training Absentees").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {

                    var objUnAttendees = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Training Absentees")
                                          join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                          select new DSRCManagementSystem.Models.Email
                                          {
                                              To = p.To,
                                              CC = p.CC,
                                              BCC = p.BCC,
                                              Subject = p.Subject,
                                              Template = q.TemplatePath
                                          }).FirstOrDefault();
                    var company = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                    string TemplatePathUnAttendees = Server.MapPath(objUnAttendees.Template);
                    string htmlUnAttendees = System.IO.File.ReadAllText(TemplatePathUnAttendees);
                    htmlUnAttendees = htmlUnAttendees.Replace("#TrainingId", trainingid.ToString());
                    htmlUnAttendees = htmlUnAttendees.Replace("#TrainingName", TrainingDetails.TrainingName);
                    htmlUnAttendees = htmlUnAttendees.Replace("#ScheduledDate", d);
                    htmlUnAttendees = htmlUnAttendees.Replace("#start", TrainingDetails.StartTime);
                    htmlUnAttendees = htmlUnAttendees.Replace("#end", TrainingDetails.EndTime);
                    htmlUnAttendees = htmlUnAttendees.Replace("#Instructor", Instructor);
                    htmlUnAttendees = htmlUnAttendees.Replace("#ServerName", ServerName);
                    htmlUnAttendees = htmlUnAttendees.Replace("#CompanyName", company);



                    if (ServerName != "http://win2012srv:88/")
                    {

                        //List<string> MailIdss = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        List<string> MailIdss = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();

                        //MailIdss.Add("boobalan.k@dsrc.co.in");
                        //MailIdss.Add("shaikhakeel@dsrc.co.in");
                        //MailIdss.Add("ramesh.S@dsrc.co.in");
                        //MailIdss.Add("aruna.m@dsrc.co.in");
                        //MailIdss.Add("Virupaksha.Gaddad@dsrc.co.in");
                        //MailIdss.Add("kirankumar@dsrc.co.in");
                        //MailIdss.Add("francispaul.k.c@dsrc.co.in");

                        //string EmailAddress = "";

                        //foreach (string mail in MailIdss)
                        //{
                        //    EmailAddress += mail + ",";
                        //}

                        //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                        // var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();



                        // DsrcMailSystem.MailSender.LD(null, objUnAttendees.Subject + " - Test Mail Please Ignore", null, htmlUnAttendees + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", MailIdss, "Test-L&DAdmin@dsrc.co.in", "Test-Instructor@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                        DsrcMailSystem.MailSender.LD(null, objUnAttendees.Subject + " - Test Mail Please Ignore", null, htmlUnAttendees + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", MailIdss, "Test-L&DAdmin@dsrc.co.in", "Test-Instructor@dsrc.co.in", Server.MapPath(logo.ToString()));

                    }
                    else
                    {
                        //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                        // DsrcMailSystem.MailSender.LD(null, objUnAttendees.Subject, null, htmlUnAttendees, "TRAINING@dsrc.co.in", Unmailids, "L&DAdmin@dsrc.co.in", "Instructor@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                        DsrcMailSystem.MailSender.LD(null, objUnAttendees.Subject, null, htmlUnAttendees, "TRAINING@dsrc.co.in", Unmailids, "L&DAdmin@dsrc.co.in", "Instructor@dsrc.co.in", Server.MapPath(logo.ToString()));
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Training Absentees", folder, ServerName);
                }

            } // addede on 10/10

                var checks = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Completion").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folders= objdb.EmailTemplates.Where(o=> o.TemplatePurpose == "Completion").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {
                         var objCompletion = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Completion")
                                              join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                              select new DSRCManagementSystem.Models.Email
                                              {
                                                  To = p.To,
                                                  CC = p.CC,
                                                  BCC = p.BCC,
                                                  Subject = p.Subject,
                                                  Template = q.TemplatePath
                                              }).FirstOrDefault();
                         var objcompany = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathCompletion = Server.MapPath(objCompletion.Template);
                         string htmlCompletion = System.IO.File.ReadAllText(TemplatePathCompletion);
                         htmlCompletion = htmlCompletion.Replace("#trainingname", objcom.Name);
                         htmlCompletion = htmlCompletion.Replace("#scheduleddate", d);
                         htmlCompletion = htmlCompletion.Replace("#ServerName",ServerName);
                         htmlCompletion = htmlCompletion.Replace("#CompanyName", objcompany);


                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> Mailidss = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIdss.Add("boobalan.k@dsrc.co.in");
                             //MailIdss.Add("shaikhakeel@dsrc.co.in");
                             //MailIdss.Add("ramesh.S@dsrc.co.in");
                             //MailIdss.Add("aruna.m@dsrc.co.in");
                             //MailIdss.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIdss.Add("kirankumar@dsrc.co.in");
                             //MailIdss.Add("francispaul.k.c@dsrc.co.in");

                             //string EmailAddress = "";

                             //foreach (string mail in MailIdss)
                             //{
                             //    EmailAddress += mail + ",";
                             //}

                             //EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                             //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                             // DsrcMailSystem.MailSender.LD(null, objCompletion.Subject + " - Test Mail Please Ignore", null, htmlCompletion + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", MailIds , "Test-L&DAdmin@dsrc.co.in", "Test-Instructor@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                             DsrcMailSystem.MailSender.LD(null, objCompletion.Subject + " - Test Mail Please Ignore", null, htmlCompletion + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", Mailidss, "Test-L&DAdmin@dsrc.co.in", "Test-Instructor@dsrc.co.in", Server.MapPath(logo.ToString()));

                         }
                         else
                         {
                             //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                             // DsrcMailSystem.MailSender.LD(null, objCompletion.Subject, null, htmlCompletion, "TRAINING@dsrc.co.in", MailIds, "L&DAdmin@dsrc.co.in", "Instructor@dsrc.co.in", Server.MapPath(logo.AppValue.ToString()));
                             DsrcMailSystem.MailSender.LD(null, objCompletion.Subject, null, htmlCompletion, "TRAINING@dsrc.co.in", MailIds, "L&DAdmin@dsrc.co.in", "Instructor@dsrc.co.in", Server.MapPath(logo.ToString()));
                         }
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Completion", folders, ServerName);
                     }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Null()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Enrollment()
        {
            List<DSRCManagementSystem.Models.Enrollment> obj = new List<DSRCManagementSystem.Models.Enrollment>();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            int? userid = objdb.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int? roleid = objdb.UserRoles.Where(x => x.UserID == userid).Select(o => o.RoleID).FirstOrDefault();
            obj = (from p in objdb.Trainings.Where(x => x.roleid == roleid && x.ScheduledDate > DateTime.Now && x.StatusId != 3)
                   join u in objdb.Master_TrainingStatus on p.StatusId equals u.StatusId
                   select new DSRCManagementSystem.Models.Enrollment
                   {
                       TrainingName = p.TrainingName,
                       TrainingId = p.TrainingId,
                       RegOn = p.ScheduledDate,
                       Status = u.StatusName
                   }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        [HttpGet]
        public ActionResult Learning()
        {
            List<DSRCManagementSystem.Models.Learning> objler = new List<DSRCManagementSystem.Models.Learning>();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            int? userid = objdb.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int? roleid = objdb.UserRoles.Where(x => x.UserID == userid).Select(o => o.RoleID).FirstOrDefault();
          
            objler = (from p in objdb.Trainings.Where(x => x.roleid == roleid && x.StatusId == 3 && x.ScheduledDate < DateTime.Now)
                      join t in objdb.Master_TrainingStatus on p.StatusId equals t.StatusId
                      join u in objdb.Master_TrainingLevel on p.LevelId equals u.LevelId
                      select new DSRCManagementSystem.Models.Learning
                      {
                          TrainingName = p.TrainingName,
                          TrainingId = p.TrainingId,
                          Level = u.LevelName,
                          RegisteredOn = p.ScheduledDate,
                          CompletedOn = p.ScheduledDate,
                          Status = t.StatusName,
                          Score = p.LevelId
                      }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objler);
        }

        [HttpGet]
        public ActionResult CalendarEvents()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();           
            return View();
        }
        [HttpGet]
        public ActionResult TrainingCalendar()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.Tab = "Training";
            return View();
        }

        [HttpGet]
        public ActionResult CalendarEventsResult()
        {
            var data = new List<LDCalendar>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var records = db.Trainings.Where(x => x.IsActive == true).ToList();
                    int userid = Convert.ToInt32(Session["UserID"].ToString());

                    DateTime myDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                    data = records.Select(x => new LDCalendar()
                    {
                        TrainingId = x.TrainingId,
                        title = x.TrainingName,
                        Detail = x.TrainingName,
                        start = x.ScheduledDate.Value.ToString("yyyy/MM/dd"),
                        className = "colorClass",
                        IsCompleted = db.TrainingNominations.Any(o => o.TrainingId == x.TrainingId && o.CompletionFlag == false) == true ? true : false,
                        IsNominated = db.TrainingNominations.Where(o => o.IsActive == true && o.TrainingId == x.TrainingId && o.UserId == userid).Count() > 0 ? true : false,
                        IsAttended = db.TrainingNominations.Where(o => o.IsActive == true && o.TrainingId == x.TrainingId && o.UserId == userid && o.CompletionFlag == false).Count() > 0 ?
                                     ((db.TrainingNominations.FirstOrDefault(o => o.IsActive == true && o.TrainingId == x.TrainingId && o.UserId == userid && o.CompletionFlag == false).Score) > 0 ?
                                     true : false) : false
                    }).ToList();

                    ViewBag.list = data.Select(o => o.TrainingId).ToList();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
                return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult NewTraining()
        {
            List<DSRCManagementSystem.Models.NewTraining> obj = new List<DSRCManagementSystem.Models.NewTraining>();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            int? userid = objdb.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int? roleid = objdb.UserRoles.Where(x => x.UserID == userid).Select(o => o.RoleID).FirstOrDefault();
            var  type = objdb.Trainings.Where( x => x.roleid == roleid ).Select( o => o.Master_TrainingType).FirstOrDefault();
         
            obj = (from p in objdb.Trainings.Where(x => x.roleid == roleid)
                   join t in objdb.Master_TrainingStatus on p.StatusId equals t.StatusId
                   join u in objdb.Master_TrainingLevel on p.LevelId equals u.LevelId
                   join w in objdb.Master_TrainingType on p.TrainingTypeId equals w.TrainingTypeId
                   select new DSRCManagementSystem.Models.NewTraining
                   {
                       TrainingName = p.TrainingName,
                       TrainingId = p.TrainingId,
                       Level = u.LevelName,
                       type = w.TypeName 
                   }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        [HttpGet]
        public ActionResult Detail(string type)
        {
            List<DSRCManagementSystem.Models.Detail> obj = new List<DSRCManagementSystem.Models.Detail>();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
            int? userid = objdb.Users.Where(x => x.UserName == email).Select(o => o.UserID).FirstOrDefault();
            int? roleid = objdb.UserRoles.Where(x => x.UserID == userid).Select(o => o.RoleID).FirstOrDefault();
            int? instructorid = objdb.Trainings.Where(x => x.roleid == roleid).Select(o => o.InstructorId).FirstOrDefault();
            var language = objdb.Trainings.Where(x => x.InstructorId == instructorid).Select(o => o.TrainingName).ToList();
            obj = (from P in objdb.Trainings.Where(x => x.ScheduledDate > DateTime.Now && x.roleid == roleid)
                   join t in objdb.UserRoles on P.roleid equals t.RoleID
                   join u in objdb.Users on t.UserID equals u.UserID
                   join w in objdb.Master_TrainingType on P.TrainingTypeId equals w.TrainingTypeId
                   select new DSRCManagementSystem.Models.Detail
                   {
                       Instructor = u.FirstName,
                       Type = w.TypeName,
                       Date = P.ScheduledDate
                   }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }
    }
}


