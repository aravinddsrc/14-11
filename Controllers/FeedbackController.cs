using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data;
using DSRCManagementSystem.DSRCLogic;
using System.Net.Mail;
using System.Threading.Tasks;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Web.Configuration;
//using DSRCManagementSystem.ServiceReference1;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;


namespace DSRCManagementSystem.Controllers
{
    public class FeedbackController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShowFeedback()
        {
            var Result = new List<Feedback>();
            try
            {

            Result = (from fb in db.UsersFeedbacks
                          join us in db.Users on fb.UserID equals us.UserID
                          select new Feedback()
                          {
                              FeedbackId = fb.Id,
                              UserName = us.FirstName + " " + (us.LastName ?? ""),
                              Feedbacks = fb.Feedback,
                              FeedbackDate = fb.FeedbackDate,
                          }).OrderByDescending(x => x.FeedbackDate).ThenByDescending(x => x.FeedbackDate.Month).ThenByDescending(x => x.FeedbackDate.Year).ToList();
            return View(Result);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Result);
        }

        [HttpGet]
        public ActionResult Viewmail(string myParams)
        {
            var Result = new Feedback();
            try
            {
            
            var FeedbackID = Convert.ToInt32(myParams);

          
            Result = (from fb in db.UsersFeedbacks
                          join us in db.Users.Where(x => x.IsActive ==true) on fb.UserID equals us.UserID
                          where fb.Id == FeedbackID
                          select new Feedback()
                          {
                              //FeedbackId = fb.Id,http://localhost:5555/user/login
                              UserID =us.UserID,
                              UserName = us.FirstName + " " + (us.LastName ?? ""),
                              Feedbacks = fb.Feedback,
                              FeedbackDate = fb.FeedbackDate,
                          }).FirstOrDefault();


            Session["ProfileId"] = Result.UserID;

            return View(Result);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Result);
        }

        public ActionResult UserFeedbacks()
        {
            return View(new UserFeedback());
        }

        [HttpPost]
        public ActionResult UserFeedbacks(UserFeedback userFeedback)
        {
            try
            {
            if (ModelState.IsValid)
            {

                    DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                    //  var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    string ServerName = AppValue.GetFromMailAddress("ServerName");
                    int userID = Convert.ToInt32(Session["UserID"]);
                    string email = Convert.ToString(Session["UserName"]);

                    string pathvalue = CommonLogic.getLogoPath();



                    //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    //string[] words;

                    //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                    //string pathvalue = "~/" + words[1];



                    var obj = new UsersFeedback()
                    {
                        UserID = userID,
                        Feedback = userFeedback.Feedback,
                        FeedbackDate = DateTime.Now
                    };
                    db.UsersFeedbacks.AddObject(obj);
                    db.SaveChanges();


                    string MailMessage = MailBuilder.FeedBack(Session["FirstName"].ToString(), Session["LastName"].ToString(), userFeedback.Feedback.Replace(Environment.NewLine, "</br>"));

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Feedback").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Feedback").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objFeedback = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Feedback")
                                            join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                            select new DSRCManagementSystem.Models.Email
                                            {
                                                To = p.To,
                                                CC = p.CC,
                                                BCC = p.BCC,
                                                Subject = p.Subject,
                                                Template = q.TemplatePath
                                            }).FirstOrDefault();

                         string TemplatePathFeedback = Server.MapPath(objFeedback.Template);
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string htmlFeedback = System.IO.File.ReadAllText(TemplatePathFeedback);
                         htmlFeedback = htmlFeedback.Replace("#SenderName", Session["FirstName"].ToString() + " " + Session["LastName"].ToString());
                         htmlFeedback = htmlFeedback.Replace("#Comments", userFeedback.Feedback.Replace(Environment.NewLine, "</br>"));
                         htmlFeedback = htmlFeedback.Replace("#ServerName", ServerName);
                         htmlFeedback = htmlFeedback.Replace("#CompanyName", company);

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("dineshkumar.d@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in";
                             string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";

                             Task.Factory.StartNew(() =>
                             {

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objFeedback.Subject + " - Test Mail Please Ignore", null, htmlFeedback + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue.ToString()));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objFeedback.Subject, "", htmlFeedback, "HRMS@dsrc.co.in", objFeedback.To, objFeedback.CC, objFeedback.BCC, Server.MapPath(pathvalue.ToString()));

                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Feedback", folder, ServerName);

                     }
                    //DsrcMailSystem.MailSender.SendMail("", "Feedback", "", MailMessage, email, "boobalan.k@dsrc.co.in", Server.MapPath("~/Content/Template/images/logo.png")); });
                    //Task.Factory.StartNew(() => { DsrcMailSystem.MailSender.SendMail("", "Feedback", "", userFeedback.Feedback, email, "prasanthK@dsrc.co.in", Server.MapPath("~/Content/Template/images/logo.png")); });
                    userFeedback.StatusMessage = "Success";
                }
            return View(userFeedback);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(userFeedback);
        }
    }
}
