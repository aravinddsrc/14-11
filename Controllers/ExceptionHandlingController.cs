using DSRCManagementSystem.DSRCLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Globalization;
using DSRCManagementSystem.Models;
using System.Net.Mail;


namespace DSRCManagementSystem.Controllers
{
    public class ExceptionHandlingController : Controller
    {
        //
        // GET: /ExceptionHandling/
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
       

        public static void ExceptionDetails(Exception ex, string actionName, string controllerName)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            long UserID = Convert.ToInt64(System.Web.HttpContext.Current.Session["UserID"]);
            var dataobj = db.ExceptionLogs.CreateObject();
            dataobj.UserID = UserID;
            dataobj.MethodName = (ex.InnerException != null) ? (ex.InnerException.TargetSite.Name == null ? "" : ex.InnerException.TargetSite.Name) : (ex.TargetSite.Name == null ? "" : ex.TargetSite.Name); ;
            dataobj.ExceptionDate = DateTime.Now;
            dataobj.ExceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
            dataobj.Source = (ex.InnerException != null) ? ex.InnerException.Source : ex.Source;
            dataobj.StackTrace = (ex.InnerException != null) ? ex.InnerException.StackTrace : ex.StackTrace;
            db.ExceptionLogs.AddObject(dataobj);
            db.SaveChanges();
            Mail_Trigger(ex, actionName, controllerName);
        }

        public static void Mail_Trigger(Exception ex, string actionName, string controllerName)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
             DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
             string ServerName = AppValue.GetFromMailAddress("ServerName");
            string AssignedbyEmailId = System.Web.HttpContext.Current.Application["UserName"].ToString();
            // var userid = Convert.ToInt32(Session["UserID"].ToString());
            var objuser = db.Users.Where(o => o.UserName == AssignedbyEmailId).Select(o => o).FirstOrDefault();
            string UserName = objuser.FirstName + "  " + objuser.LastName;
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var MethodName = (ex.InnerException != null) ? (ex.InnerException.TargetSite.Name == null ? "" : ex.InnerException.TargetSite.Name) : (ex.TargetSite.Name == null ? "" : ex.TargetSite.Name); ;
            var ExceptionDate = DateTime.Now;
            var ExceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
            var Source = (ex.InnerException != null) ? ex.InnerException.Source : ex.Source;
            var StackTrace = (ex.InnerException != null) ? ex.InnerException.StackTrace : ex.StackTrace;
            var url = ServerName + controllerName + "/" + actionName;
            var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            //var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Error Message")
                        join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                        select new DSRCManagementSystem.Models.Email
                        {
                            To = p.To,
                            CC = p.CC,
                            BCC = p.BCC,
                            Subject = p.Subject,
                            Template = q.TemplatePath
                        }).FirstOrDefault();
            var path = System.Web.Hosting.HostingEnvironment.MapPath(obj1.Template);

            //var file = server.MapPath("~/SomeDirectory/Somefile");
            //string TemplatePath = Server.MapPath(obj1.Template);
            string html = System.IO.File.ReadAllText(path);
            string FromMailId = "TestMail@dsrc.co.in";
            //string Title = " " + objcom + "   calendar event Created";
            string Subject = "Management Portal : Error Report" + " " + "on" + " " + DateTime.Today.ToString("ddd, MMM d, yyyy");
            obj1.Subject = " " + objcom + " " + "Management Portal : Error Report";

            html = html.Replace("#Error", ExceptionMessage);
            html = html.Replace("#Subject", Subject);
            html = html.Replace("#Date", DateTime.Today.ToString("dd/MM/yyyy"));
            html = html.Replace("#url", url);
            html = html.Replace("#UserName", UserName);
            html = html.Replace("#CompanyName", objcom.ToString());
            html = html.Replace("#ServerName", ServerName);

            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
            string EmailAddres = "";
            for (int i = 0; i < MailIds.Count(); i++)
            {
                if (i != MailIds.Count() - 1)
                {
                    EmailAddres += MailIds[i] + ",";
                }
                else
                {
                    EmailAddres += MailIds[i];
                }
            }
            if (ServerName  != "http://win2012srv:88/")
            {


                Task.Factory.StartNew(() =>
                {
                    string pathvalue = CommonLogic.getLogoPath();
                    DsrcMailSystem.MailSender.TaskMail(null, Subject, html, FromMailId, EmailAddres, System.Web.Hosting.HostingEnvironment.MapPath(pathvalue.ToString()));
                });

            }
            else
            {

                Task.Factory.StartNew(() =>
                {
                    string pathvalue = CommonLogic.getLogoPath();
                    DsrcMailSystem.MailSender.TaskMail(null, Subject, html, FromMailId, EmailAddres, System.Web.Hosting.HostingEnvironment.MapPath(pathvalue.ToString()));
                });


            }
        }



        public static void TemplateMissing(string template, string folder, string servername)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
              DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var UserID = Convert.ToInt64(System.Web.HttpContext.Current.Session["UserID"]);
            var username = db.Users.Where(x => x.UserID == UserID).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            DateTime currentdate = DateTime.Now;

            var Assignobj = db.MissingTemplates.CreateObject();
            Assignobj.TemplateName = template;
            Assignobj.MissedProcess = folder;
            Assignobj.ProcessedBy = username;
            Assignobj.ProcessedDate = currentdate;
            db.MissingTemplates.AddObject(Assignobj);
            db.SaveChanges();

            MailMessage mail = new System.Net.Mail.MailMessage();
            string EmailAddres = "aruna.m@dsrc.co.in";
            mail.To.Add(EmailAddres);
            mail.From = new MailAddress("TemplateExceptionMail@dsrc.co.in");
            mail.Subject = "HRMS-Test Template Missing Exception";
            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            mail.Body = "Template Missing Exception: " + "  " + template + " <br/><br/>ServerName: " + " " + servername + " <br/><br/>Date: " + "  " + indianTime + " " + "<br/><br/>This email has been automatically generated.</br>Please do not reply to this email address as all responses are directed to an unattended,<br/>mailbox, and you will not receive a response.</p><br/><p style=font-size:15px>Thanks,<br/>" + company + "HRMS</p>";
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            string port = db.Master_ApplicationSettings.Where(x => x.AppKey == "Port").Select(o => o.AppValue).FirstOrDefault();
            smtp.Host = db.Master_ApplicationSettings.Where(x => x.AppKey == "Host Name").Select(o => o.AppValue).FirstOrDefault();
            smtp.Port = Convert.ToInt32(port.ToString());
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential
            ("prasanthk@dsrc.co.in", "dsrc1234");// Enter senders User name and password                               
            smtp.EnableSsl = false;
            smtp.Send(mail);
        }

        

        }
    }

        