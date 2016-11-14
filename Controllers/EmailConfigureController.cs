using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.IO;
using DSRCManagementSystem.DSRCLogic;
using Utilities;
using System.Net.Mail;
using System.Data;
using System.Data.Objects;

namespace DSRCManagementSystem.Controllers
{
    public class EmailConfigureController : Controller
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        [HttpGet]
        public ActionResult SendMail()
        {

            EmailConfigure MailObj = new EmailConfigure();
           try{
            DSRCManagementSystemEntities1 db=new DSRCManagementSystemEntities1();
            var AppSettings = db.Master_ApplicationSettings.Select(o => o).ToList();
            MailObj.Host = AppSettings.FirstOrDefault(o => o.AppKey == "Host Name").AppValue;
            MailObj.Port =Convert.ToInt32(AppSettings.FirstOrDefault(o => o.AppKey == "Port").AppValue);
            MailObj.UserName =AppSettings.FirstOrDefault(o => o.AppKey == "UserName").AppValue;
            MailObj.Password = AppSettings.FirstOrDefault(o => o.AppKey == "Password").AppValue;
            MailObj.ConfirmPassword = AppSettings.FirstOrDefault(o => o.AppKey == "Password").AppValue;
            MailObj.From = AppSettings.FirstOrDefault(o => o.AppKey == "From").AppValue;
            MailObj.To = AppSettings.FirstOrDefault(o => o.AppKey == "To").AppValue;
           }
           catch (Exception Ex)
           {
               string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
               string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
               ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

           }
           

            return View(MailObj);
        }
        [HttpPost]
        public ActionResult SendMail(EmailConfigure data)
        {
            
                try
                {
                    DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    MailMessage mail = new MailMessage();
                    mail.To.Add(data.To);
                    mail.From = new MailAddress(data.From);
                    mail.Subject = "Test Mail";
                    mail.Body = "Test Mail on" + " " + indianTime + " " + "<br/><br/>This email has been automatically generated.</br>Please do not reply to this email address as all responses are directed to an unattended mailbox. <br/> You will not receive a response</p><br/><br/><p style=font-size:15px>Thanks,<br/>DSRC Management Portal</p>";
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = data.Host;
                    smtp.Port = data.Port;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential
                    (data.UserName, data.Password);
                    smtp.EnableSsl = false;
                    smtp.Send(mail);
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    return Json("failed", JsonRequestBehavior.AllowGet);
                }
              
        }
        
        public string SaveApplicationSetting(EmailConfigure data)
        {

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var Host = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "Host Name");
                Host.AppValue = data.Host;

                var Port = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "Port");
                Port.AppValue = data.Port.ToString();

                var UserName = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "UserName");
                UserName.AppValue = data.UserName.Trim();

                var Password = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "Password");
                Password.AppValue = data.Password;

                var From = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "From");
                From.AppValue = data.From.Trim();

                var To = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "To");
                To.AppValue = data.To.Trim();

                db.SaveChanges();
                return ("success");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return ("failed");
            }

        }

        
       
       

    }
}
               