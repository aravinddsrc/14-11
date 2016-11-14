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
    public class AddExpenditureController : Controller
    {
        //
        // GET: /AddExpenditure/
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
    [HttpGet]
        public ActionResult AddExpenditure()
        {
            List<DSRCManagementSystem.Models.Expenditure> UserList = new List<DSRCManagementSystem.Models.Expenditure>();
        try
        {
            
            int ExpenditureId = Convert.ToInt32(Session["ExpenditureID"]);
            ModelState.Clear();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
             DSRCManagementSystem.Models.Expenditure objUser = new DSRCManagementSystem.Models.Expenditure();
           
            UserList = (from u in db.Expenditures.Where(o => o.IsActive != false)
                select new DSRCManagementSystem.Models.Expenditure()
                {
                    ExpenditureID = u.ExpenseID,
                    ExpenseDescription = u.ExpenseDescription,
                    ExpenseDate = u.ExpenseDate,
                    ExpenseAmount = u.ExpenseAmount,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                }).OrderByDescending(o => o.ExpenseDate.Value.Year)
                .ThenByDescending(o => o.ExpenseDate.Value.Month)
                .ThenByDescending(o => o.ExpenseDate.Value.Day)
                .ToList();
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
        public ActionResult Add()
        {
            DSRCManagementSystem.Models.Expenditure obj = new DSRCManagementSystem.Models.Expenditure();
           try
           {
               obj.CreatedAt = System.DateTime.Now;
               DateTime d1 = Convert.ToDateTime(obj.CreatedAt);
               string d = d1.ToShortDateString();
               obj.ScheduleDate = d;
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
       public ActionResult Add(DSRCManagementSystem.Models.Expenditure objmodel )
       {
           DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
           string ServerName = AppValue.GetFromMailAddress("ServerName");
           DSRCManagementSystem.Expenditure obj = new DSRCManagementSystem.Expenditure();
           try
           {
               obj.ExpenseDescription = objmodel.ExpenseDescription.Trim();
               obj.ExpenseDate = objmodel.ExpenseDate;
               obj.ExpenseAmount = objmodel.ExpenseAmount;
               obj.CreatedAt = Convert.ToDateTime(objmodel.ScheduleDate);
               obj.IsActive = true;
               objdb.AddToExpenditures(obj);
               objdb.SaveChanges();


                 var ExpenseDes =  objmodel.ExpenseDescription.Trim();
            DateTime? Expensedate = objmodel.ExpenseDate;
            var Expense = objmodel.ExpenseAmount;
            int userId = int.Parse(Session["UserID"].ToString());
            var createdby = objdb.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = objdb.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
              var Getmailid = objdb.Users.Where(x => x.UserID == 1).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = objdb.Users.Where(x => x.UserID == 1).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();
                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Add Expense").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Add Expense").Select(o => o.TemplatePath).FirstOrDefault();
                if ((check != null)&&(check !=0))
                {
                    var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Add Expense")
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

                    string Subject = "Expense Added on " + DateTime.Today.ToString("dd MMM yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Expense Added";


                    html = html.Replace("#ExpenseDescription", ExpenseDes);
                    html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    html = html.Replace("#ExpenseDate", Expensedate.ToString().Substring(0, 10));
                    html = html.Replace("#ExpenseAmount", Expense.ToString());
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName", ServerName);
                    List<string> MailIds = objdb.TestMailIDs.Select(o => o.MailAddress).ToList();
                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }
                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                    if (ServerName != "http://win2012srv:88/")
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

                    ExceptionHandlingController.TemplateMissing("Add Expense", folder, ServerName);

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

       public ActionResult EditExpenditure(int Id)
       {

           DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
           DSRCManagementSystem.Models.Expenditure obj = new DSRCManagementSystem.Models.Expenditure();
           try
           {


               var value = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o).FirstOrDefault();
               
               obj.ExpenseDescription = value.ExpenseDescription;
               obj.ExpenseAmount = value.ExpenseAmount;
               obj.ExpenseDate = value.ExpenseDate;
               obj.ScheduleDate = Convert.ToString(value.CreatedAt);
               TempData["ExpenditureId"] = Id;
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
       public ActionResult EditExpenditure(DSRCManagementSystem.Models.Expenditure objmodel)
       {
           DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
           string ServerName = AppValue.GetFromMailAddress("ServerName");
           try
           {
               objmodel.ExpenditureID = Convert.ToInt32(TempData["ExpenditureId"]);
               var value =
                   objdb.Expenditures.Where(x => x.ExpenseID == objmodel.ExpenditureID).Select(o => o).FirstOrDefault();
               if (value != null)
               {
                   value.ExpenseDescription = objmodel.ExpenseDescription;
                   value.ExpenseDate = objmodel.ExpenseDate;
                   value.ExpenseAmount = objmodel.ExpenseAmount;
                   value.CreatedAt = Convert.ToDateTime(objmodel.ScheduleDate);
                   objdb.SaveChanges();
                     var ExpenseDes =  objmodel.ExpenseDescription.Trim();
            DateTime? Expensedate = objmodel.ExpenseDate;
            var Expense = objmodel.ExpenseAmount;
            int userId = int.Parse(Session["UserID"].ToString());
            var createdby = objdb.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var FromEmailID = objdb.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
               var Getmailid = objdb.Users.Where(x => x.UserID == 1).Select(o => o.EmailAddress).FirstOrDefault();
                var GetName = objdb.Users.Where(x => x.UserID == 1).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                 .Select(o => o.AppValue)
                 .FirstOrDefault();

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Edit Expense").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Edit Expense").Select(o => o.TemplatePath).FirstOrDefault();
                if ((check != null)&&(check !=0))
                {
                    var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Edit Expense")
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

                    string Subject = "Expense Edited on " + DateTime.Today.ToString("dd MMM yyyy");
                    obj1.Subject = " " + objcom + " Management Portal-New Expense Edited";


                    html = html.Replace("#ExpenseDescription", ExpenseDes);
                    html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    html = html.Replace("#ExpenseDate", Expensedate.ToString().Substring(0, 10));
                    html = html.Replace("#ExpenseAmount", Expense.ToString());
                    html = html.Replace("#CompanyName", objcom.ToString());
                    html = html.Replace("#ServerName",ServerName);
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

                    ExceptionHandlingController.TemplateMissing("Edit Expense", folder, ServerName);

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
               var value = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o).FirstOrDefault();
               var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
               .Select(o => o.AppValue)
               .FirstOrDefault();

               var ExpenseDes = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o.ExpenseDescription).FirstOrDefault();
               var Expensedate = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o.ExpenseDate).FirstOrDefault();
               var Expense = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o.ExpenseAmount).FirstOrDefault();
               if (value != null)
               {
                   value.IsActive = false;
                   objdb.SaveChanges();
                   var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Expense").Select(o => o.EmailTemplateID).FirstOrDefault();
                   var folder = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Expense").Select(o => o.TemplatePath).FirstOrDefault();
                   if ((check != null) && (check != 0))
                   {
                       var obj1 = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Expense")
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

                       string Subject = "Expense Deleted on " + DateTime.Today.ToString("dd MMM yyyy");
                       obj1.Subject = " " + objcom + " Management Portal-Expense Deleted";


                       html = html.Replace("#ExpenseDescription", ExpenseDes);
                       html = html.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                       html = html.Replace("#ExpenseDate", Expensedate.ToString().Substring(0, 10));
                       html = html.Replace("#ExpenseAmount", Expense.ToString());
                       html = html.Replace("#CompanyName", objcom.ToString());
                       html = html.Replace("#ServerName",ServerName);
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

                       ExceptionHandlingController.TemplateMissing("Delete Expense", folder, ServerName);

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



        public ActionResult DeleteExpenditure()
        {
            return View();
        }
             

    }
}
