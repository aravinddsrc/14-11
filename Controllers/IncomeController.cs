using DSRCManagementSystem.DSRCLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class IncomeController : Controller
    {
        //
        // GET: /Income/
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult Income()
        {
            List<DSRCManagementSystem.Models.Income> UserList = new List<DSRCManagementSystem.Models.Income>();
            try
            {
            int IncomeID = Convert.ToInt32(Session["IncomeID"]);
            ModelState.Clear();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Income objUser = new DSRCManagementSystem.Models.Income();
            UserList = (from u in db.Incomes.Where(o => o.IsActive != false)
                        select new DSRCManagementSystem.Models.Income()
                        {
                           IncomeID=u.IncomeID,
                           IncomeDescription=u.IncomeDescription,
                           IncomeDate=u.IncomeDate,
                           IncomeAmount=u.IncomeAmount,
                           IsActive=u.IsActive,
                           CreatedAt=u.CreatedAt,
                        }).OrderByDescending(o => o.IncomeDate.Value.Year).ThenByDescending(o => o.IncomeDate.Value.Month).ThenByDescending(o => o.IncomeDate.Value.Day).ToList();
            return View(UserList);    
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(UserList);    
        }

        [HttpGet]
        public ActionResult AddIncome()
        {
            DSRCManagementSystem.Models.Income obj = new DSRCManagementSystem.Models.Income();
            try
            {
            obj.CreatedAt = System.DateTime.Now;
            DateTime d1 = Convert.ToDateTime(obj.CreatedAt);
            string d = d1.ToShortDateString();
            obj.ScheduleDate = d;
            //return View(obj);
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
        public ActionResult AddIncome(DSRCManagementSystem.Models.Income objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DSRCManagementSystem.Income obj = new DSRCManagementSystem.Income();
            try
            {
            obj.IncomeDescription = objmodel.IncomeDescription.Trim();
            obj.IncomeDate = objmodel.IncomeDate;
            obj.IncomeAmount = objmodel.IncomeAmount;
            obj.CreatedAt = Convert.ToDateTime(objmodel.ScheduleDate);
            obj.IsActive = true;
            objdb.AddToIncomes(obj);
            objdb.SaveChanges();

            var IncomeDes = objmodel.IncomeDescription.Trim();
            DateTime? Incomedate = objmodel.IncomeDate;
            var Income = objmodel.IncomeAmount;
            int userId = int.Parse(Session["UserID"].ToString());
            var createdby = objdb.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = objdb.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
            //foreach (var eids in GetUsersFromDept)
            //{
                var Getmailid = objdb.Users.Where(x => x.UserID == 1).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = objdb.Users.Where(x => x.UserID == 1).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Add Income").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Add Income").Select(o => o.TemplatePath).FirstOrDefault();
                if ((check != null)&&(check !=0))
                {
                var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Add Income")
                            join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                            select new DSRCManagementSystem.Models.Email
                            {
                                To = p.To,
                                CC = p.CC,
                                BCC = p.BCC,
                                Subject = p.Subject,
                                Template = q.TemplatePath
                            }).FirstOrDefault();
              
                    string TemplatePath = Server.MapPath(obj1.Template);
                    string html = System.IO.File.ReadAllText(TemplatePath);

                    //string Title = " " + objcom + "   calendar event Created";
                    string Subject = "Income Added on " + DateTime.Today.ToString("dd MMM yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Income Added";


                    html = html.Replace("#IncomeDescription", IncomeDes);
                    html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    html = html.Replace("#IncomeDate", Incomedate.ToString().Substring(0, 10));
                    html = html.Replace("#IncomeAmount", Income.ToString());
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName", ServerName);
                    List<string> MailIds = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();
                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }
                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                    if (ServerName  != "http://win2012srv:88/")
                    {

                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));

                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Add Income",folder, ServerName);

                }
            }

            
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]

        public ActionResult EditIncome(int Id)
        {
            DSRCManagementSystem.Models.Income obj = new DSRCManagementSystem.Models.Income();
            try
            {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var value = objdb.Incomes.Where(x => x.IncomeID == Id).Select(o => o).FirstOrDefault();
            obj.IncomeDescription = value.IncomeDescription;
            obj.IncomeAmount = value.IncomeAmount;
            obj.IncomeDate = value.IncomeDate;
            obj.ScheduleDate = Convert.ToString(value.CreatedAt);
            TempData["IncomeId"] = Id;
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
        public ActionResult EditIncome(DSRCManagementSystem.Models.Income objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
            objmodel.IncomeID = Convert.ToInt32(TempData["IncomeId"]);
            var value = objdb.Incomes.Where(x => x.IncomeID == objmodel.IncomeID).Select(o => o).FirstOrDefault();
            if (value != null)
            {
                value.IncomeDescription = objmodel.IncomeDescription;
                value.IncomeDate = objmodel.IncomeDate;
                value.IncomeAmount = objmodel.IncomeAmount;
                value.CreatedAt = Convert.ToDateTime(objmodel.ScheduleDate);
                objdb.SaveChanges();

                var IncomeDes = objmodel.IncomeDescription.Trim();
                DateTime? Incomedate = objmodel.IncomeDate;
                var Income = objmodel.IncomeAmount;
                int userId = int.Parse(Session["UserID"].ToString());
                var createdby = objdb.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                var FromEmailID = objdb.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
                //foreach (var eids in GetUsersFromDept)
                //{
                var Getmailid = objdb.Users.Where(x => x.UserID == 1).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = objdb.Users.Where(x => x.UserID == 1).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

              //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Edit Income").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Edit Income").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Edit Income")
                                join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                select new DSRCManagementSystem.Models.Email
                                {
                                    To = p.To,
                                    CC = p.CC,
                                    BCC = p.BCC,
                                    Subject = p.Subject,
                                    Template = q.TemplatePath
                                }).FirstOrDefault();

                    string TemplatePath = Server.MapPath(obj1.Template);
                    string html = System.IO.File.ReadAllText(TemplatePath);

                    //string Title = " " + objcom + "   calendar event Created";
                    string Subject = "Income Edited on " + DateTime.Today.ToString("dd MMM yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Income Edited";


                    html = html.Replace("#IncomeDescription", IncomeDes);
                    html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    html = html.Replace("#IncomeDate", Incomedate.ToString().Substring(0, 10));
                    html = html.Replace("#IncomeAmount", Income.ToString());
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName", ServerName);
                    List<string> MailIds = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();
                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }
                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                    if (ServerName  != "http://win2012srv:88/")
                    {

                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));

                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Edit Income",folder, ServerName);

                }

            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
            var value = objdb.Incomes.Where(x => x.IncomeID == Id).Select(o => o).FirstOrDefault();
            var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
            .Select(o => o.AppValue)
            .FirstOrDefault();

           // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var IncomeDes= objdb.Incomes.Where(x => x.IncomeID == Id).Select(o => o.IncomeDescription).FirstOrDefault();
                var Incomedate = objdb.Incomes.Where(x => x.IncomeID == Id).Select(o => o.IncomeDate).FirstOrDefault();
                var Income = objdb.Incomes.Where(x => x.IncomeID == Id).Select(o => o.IncomeAmount).FirstOrDefault();
                

            if (value != null)
            {
                value.IsActive = false;
                objdb.SaveChanges();

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Income").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                var folder= objdb.EmailTemplates.Where(o=> o.TemplatePurpose == "Delete Income").Select(x=> x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Income")
                                join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                select new DSRCManagementSystem.Models.Email
                                {
                                    To = p.To,
                                    CC = p.CC,
                                    BCC = p.BCC,
                                    Subject = p.Subject,
                                    Template = q.TemplatePath
                                }).FirstOrDefault();

                    string TemplatePath = Server.MapPath(obj1.Template);
                    string html = System.IO.File.ReadAllText(TemplatePath);

                    //string Title = " " + objcom + "   calendar event Created";
                    string Subject = "Income Deleted on " + DateTime.Today.ToString("dd MMM yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Income Edited";


                    html = html.Replace("#IncomeDescription", IncomeDes);
                    html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    html = html.Replace("#IncomeDate", Incomedate.ToString().Substring(0, 10));
                    html = html.Replace("#IncomeAmount", Income.ToString());
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName", ServerName);
                    List<string> MailIds = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();
                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }
                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                    if (ServerName  != "http://win2012srv:88/")
                    {

                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            string pathvalue = CommonLogic.getLogoPath();
                            DsrcMailSystem.MailSender.TaskMail(null, Subject, html, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Delete Income", folder, ServerName);

                }
            }
            
        }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }
    }
}
