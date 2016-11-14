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
using DSRCManagementSystem.Controllers;
using System.Web.Configuration;
using System.Threading.Tasks;


namespace DSRCManagementSystem.Models
{
    public class TimeEntryController : Controller
    {
        //
        // GET: /Attendance/UploadAttendance
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        // [DSRCAuthorize(Roles = "Attendant")]
        public ActionResult UploadAttendance()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            TimeEntry model = new TimeEntry();
            try
            {
                List<SelectListItem> Branches = GetBranches();
                Branches.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });
                model.BranchList = Branches;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model);
        }

        //[HttpPost]
        ////[DSRCAuthorize(Roles = "Attendant")]
        //public ActionResult UploadAttendance(TimeEntry model)
        //{

        //    int userId = Convert.ToInt32(Session["UserID"]);
        //    string folderPath = Server.MapPath("~/FileManager/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));
        //    var Excelfile = model.excelFile;
        //    List<SelectListItem> Branches = GetBranches();
        //    Branches.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });
        //    model.BranchList = Branches;
        //    // string errorMessage = string.Empty;
        //    if (model.BranchID != 0)
        //    {
        //        if (Excelfile != null)
        //        {
        //            //var Extension = Excelfile.FileName.Substring(Excelfile.FileName.LastIndexOf(".") + 1);
        //            var Extension = Path.GetExtension(Excelfile.FileName);
        //            if ((Extension == ".xlsx") || Extension == ".xls")
        //            {
        //                if (!Directory.Exists(folderPath))
        //                    Directory.CreateDirectory(folderPath);
        //                var fileName = Path.GetFileName(Excelfile.FileName);
        //                var path = Path.Combine(folderPath, fileName);
        //                Excelfile.SaveAs(path);
        //                ExcelUtility objExcelUtility = new ExcelUtility();
        //                try
        //                {
        //                   // objExcelUtility.ImportExcelToDatabase(path, model.BranchID);
        //                    communicationHelper.DeleteDuplicateTimeEntry();
        //                    model.ErrorSuccessMessage = "Excel file uploaded successfully.";
        //                    SendMail();
        //                }
        //                catch (Exception ex)
        //                {
        //                    ExceptionHandlingController.ExceptionDetails(ex);
        //                    model.ErrorSuccessMessage = "Error: " + ex.Message;
        //                    return View(model);
        //                }
        //            }
        //            else
        //            {
        //                model.ErrorSuccessMessage = "Please upload excel files only.";
        //            }
        //        }
        //        else
        //        {
        //            model.ErrorSuccessMessage = "Please upload the excel file.";
        //        }
        //    }
        //    else
        //    {
        //        model.ErrorSuccessMessage = "Please select branch";
        //    }
        //    return View(model);
        //}

        //private void SendMail(int BranchID)
        //{
          
        //        string ServerName = WebConfigurationManager.AppSettings["SeverName"];
        //        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //        var BranchName = db.Master_Branches.Where(x => x.BranchID == BranchID).Select(o => o.BranchName).FirstOrDefault();
        //        if (ServerName  != "http://win2012srv:88/")
        //        {
        //            MailMessage mail = new MailMessage();
        //            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

        //            string EmailAddres = "";

        //            //foreach (string mails in MailIds)
        //            //{
        //            //    EmailAddres += mails + ",";
        //            //}

        //            for (int i = 0; i < MailIds.Count(); i++)
        //            {
        //                if (i != MailIds.Count() - 1)
        //                {
        //                    EmailAddres += MailIds[i] + ",";
        //                }
        //                else
        //                {
        //                    EmailAddres += MailIds[i];
        //                }

        //            }


        //            mail.To.Add(EmailAddres);
        //            mail.From = new MailAddress("TestMail@dsrc.co.in");
        //            mail.Subject = "Test-Regarding Attendance upload";
        //            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

        //            mail.Body = "Thanks for uploading Attendance on" + " " + indianTime + " " + "<br>" + "for" + BranchName + " " + "branch" + "<br/><br/>This email has been automatically generated.</br>Please do not reply to this email address as all responses are directed to an unattended,<br/>mailbox, and you will not receive a response</p><br/><br/><p style=font-size:15px>Thanks,<br/>" + company + "Management Portal</p>";
        //            mail.IsBodyHtml = true;
        //            SmtpClient smtp = new SmtpClient();
        //            smtp.Host = "192.168.4.101";
        //            smtp.Port = 2525;
        //            smtp.UseDefaultCredentials = false;
        //            smtp.Credentials = new System.Net.NetworkCredential
        //            ("prasanthk@dsrc.co.in", "dsrc1234");// Enter senders User name and password                               
        //            smtp.EnableSsl = false;
        //            smtp.Send(mail);
        //        }
        //        else
        //        {
        //            MailMessage mail = new MailMessage();
        //            mail.To.Add("prasanthk@dsrc.co.in , umapathy@dsrc.co.in");
        //            mail.From = new MailAddress("HRMS@dsrc.co.in");
        //            mail.Subject = "Test-Regarding Attendance upload";
        //            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
        //            mail.Body = "Thanks for uploading Attendance on" + " " + indianTime + " " + "<br/><br/>This email has been automatically generated.</br>Please do not reply to this email address as all responses are directed to an unattended,<br/>mailbox, and you will not receive a response</p><br/><br/><p style=font-size:15px>Thanks,<br/>" + company + " Management Portal</p>";
        //            mail.IsBodyHtml = true;
        //            SmtpClient smtp = new SmtpClient();
        //            smtp.Host = "192.168.4.101";
        //            smtp.Port = 2525;
        //            smtp.UseDefaultCredentials = false;
        //            smtp.Credentials = new System.Net.NetworkCredential
        //            ("Mubarakbasha.S@dsrc.co.in", "dsrc12345");// Enter senders User name and password                               
        //            smtp.EnableSsl = false;
        //            smtp.Send(mail);
        //        }
        //    }
        
        //[HttpPost]
        //public FileResult SampleAttendanceFile()
        //{
        //    string filename = Server.MapPath(Url.Content("~/SampleAttendanceFile/Access 1st Mar to 31st Mar 2015.xls"));
        //    string contentType = "application/vnd.ms-excel";
        //    //var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleAttendanceFile");

        //    return new FilePathResult(filename, contentType) { FileDownloadName = "Sample_Attendance_File.xls" };
        //}

        //public ActionResult EnterAttendance()
        //{
        //    return View();
        //}

        
        //[DSRCAuthorize(Roles = "Attendant")]
        //public ActionResult EnterAttendance()
        //{
            //EditTimeEntry ObjPM = new EditTimeEntry();
            //ObjPM.IsSubmit = false;  
            ////SelectList EmpList = (SelectList)GetNames(1).Data;
            ////ObjPM.EmployeeList = EmpList.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();
            //ObjPM.EmployeeList = GetNames(1);
            //ObjPM.BranchList = GetBranches();
            //ObjPM.Date = null;
            //ObjPM.EmployeeId = null;

            //return View( ObjPM);
       // }

        //[HttpPost]
        ////[DSRCAuthorize(Roles = "Attendant")]
        //public ActionResult EnterAttendance(EditTimeEntry Model)
        //{
        //    DSRCManagementSystemEntities1 DBHRMS = new DSRCManagementSystemEntities1();
        //    var obj = (from tim_mng in DBHRMS.TimeManagements.Where(o => o.EmpID == Model.EmployeeId && o.BranchId == Model.BranchID && EntityFunctions.TruncateTime(o.Date) == EntityFunctions.TruncateTime(Model.Date))
        //               join usr in DBHRMS.Users on tim_mng.EmpID equals usr.EmpID
        //               select new
        //               {
        //                   userID = usr.UserID,
        //                   EmployeeName = usr.FirstName + " " + usr.LastName,
        //                   EmpID = usr.EmpID,
        //                   Date = tim_mng.Date,
        //                   InTime = tim_mng.InTime,
        //                   OutTime = tim_mng.OutTime

        //               }).FirstOrDefault();

        //    EditTimeEntry model = new EditTimeEntry();

        //    //SelectList EmpList = (SelectList)GetNames(Model.BranchID).Data;
        //    //model.EmployeeList = EmpList.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();

        //    model.BranchList = GetBranches();
        //    model.EmployeeList = GetNames(Model.BranchID);
        //    model.EmployeeId = Model.EmployeeId;

        //    if (obj != null)
        //    {
        //        model.IsRecordAvail = true;
        //        model.UserID = obj.userID;
        //        model.EmployeeName = obj.EmployeeName;
        //        model.EmployeeId = obj.EmpID;

        //        model.Date = obj.Date;
        //        model.InTime = obj.InTime;
        //        model.OutTime = obj.OutTime;
        //        model.TotalMin = 000;
        //        model.BranchID = Model.BranchID;

        //    }
        //    else
        //        model.IsRecordAvail = false;
        //    model.IsSubmit = true;

        //    return View(model);
        //}

        //private List<SelectListItem> GetBranches()
        //{
        //    var BranchesList = new List<SelectListItem>();

        //    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
        //    {
        //        List<Master_Branches> BranchList = db.Master_Branches.ToList();
        //        foreach (var item in BranchList)
        //        {
        //            BranchesList.Add(new SelectListItem { Text = item.BranchName, Value = item.BranchID.ToString() });
        //        }
        //    }

        //    return BranchesList;
        //}

        //[AcceptVerbs(HttpVerbs.Get)]
        //public ActionResult GetAvailEmployees(int BranchID)
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

        //    if (BranchID != 0)
        //    {

        //        Employees = (from data in db.Users
        //                      where data.IsActive == true && data.EmpID != null && data.BranchId == BranchID
        //                      select new DSRCEmployees
        //                      {
        //                          Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
        //                          UserId = data.UserID,
        //                          EmployeeId = data.EmpID
        //                      }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

        //    }
        //    return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}


        //private List<SelectListItem> GetNames(int BranchID)
        //{
        //    try
        //    {
        //        var NameList = new List<SelectListItem>();

        //        using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
        //        {

        //            List<DSRCEmployees> Names = (from data in db.Users
        //                                         where data.IsActive == true && data.EmpID != null && data.BranchId == BranchID&&data.UserStatus!=6
        //                                         select new DSRCEmployees
        //                                         {
        //                                             Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
        //                                             UserId = data.UserID,
        //                                             EmployeeId = data.EmpID
        //                                         }).OrderBy(x => x.Name).ToList();
        //            foreach (var item in Names)
        //            {
        //                NameList.Add(new SelectListItem { Text = item.Name, Value = item.EmployeeId.ToString() });
        //            }

        //            NameList.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
        //        }

        //        return NameList;
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionHandlingController.ExceptionDetails(ex);
        //        throw ex;
        //    }
        //}


        [HttpGet]
        public ActionResult EditTime(EditTimeEntry Model, string EmployeeId, string Date, int BranchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            EditTimeEntry model = new EditTimeEntry();

            try
            {
                var fromDate = DateTime.Parse(Date);
                var obj = (from tim_mng in db.TimeManagements.Where(o => o.EmpID == Model.EmployeeId && o.BranchId == Model.BranchID &&
                                                                         EntityFunctions.TruncateTime(o.Date) == EntityFunctions.TruncateTime(fromDate))
                           join usr in db.Users on tim_mng.EmpID equals usr.EmpID
                           select new
                           {
                               userID = usr.UserID,
                               EmployeeName = usr.FirstName + " " + usr.LastName,
                               EmpID = usr.EmpID,
                               Date = tim_mng.Date,
                               InTime = tim_mng.InTime,
                               OutTime = tim_mng.OutTime,
                               BranchId = tim_mng.BranchId

                           }).FirstOrDefault();

                

                if (obj != null)
                {
                    model.UserID = obj.userID;
                    model.EmployeeId = obj.EmpID;
                    model.Date = obj.Date;
                    model.EmployeeName = obj.EmployeeName;
                    model.InTime = obj.InTime;
                    model.OutTime = obj.OutTime;
                    model.BranchID = (int)obj.BranchId;

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
        public ActionResult EditTime(EditTimeEntry model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                if (ModelState.IsValid)
                {
                    var obj = (from tim_mng in db.TimeManagements
                               where tim_mng.EmpID == model.EmployeeId && tim_mng.BranchId == model.BranchID && EntityFunctions.TruncateTime(tim_mng.Date) == EntityFunctions.TruncateTime(model.Date)
                               select tim_mng
                               ).FirstOrDefault();
                    obj.InTime = model.InTime;
                    obj.OutTime = model.OutTime;
                    var inTime = DateTime.Parse(model.InTime);
                    var outTime = DateTime.Parse(model.OutTime);
                    obj.InTimeMin = (inTime.Hour * 60) + inTime.Minute;
                    obj.OutTimeMin = (outTime.Hour * 60) + outTime.Minute;
                    obj.TotalTime = obj.OutTimeMin - obj.InTimeMin;
                    db.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
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
        
        public ActionResult Attendance()
        {

            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            EditTimeEntry ObjPM = new EditTimeEntry();
            try
            {
                ObjPM.IsSubmit = false;

                ObjPM.EmployeeList = GetNames(1);
                ObjPM.BranchList = GetBranches();
                ObjPM.Date = null;
                ObjPM.EmployeeId = null;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(ObjPM);
        }
        [HttpPost]
        public ActionResult Attendance(EditTimeEntry Model)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            DSRCManagementSystemEntities1 DBHRMS = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            EditTimeEntry model = new EditTimeEntry();
            Model.EmployeeList = GetNames(1);
            try
            {
                if (Model.EmployeeId != null)
                {

                    
                    var obj = (from tim_mng in DBHRMS.TimeManagements.Where(o => o.EmpID == Model.EmployeeId && o.BranchId == Model.BranchID && EntityFunctions.TruncateTime(o.Date) == EntityFunctions.TruncateTime(Model.Date))
                               join usr in DBHRMS.Users on tim_mng.EmpID equals usr.EmpID
                               select new
                               {
                                   userID = usr.UserID,
                                   EmployeeName = usr.FirstName + " " + usr.LastName,
                                   EmpID = usr.EmpID,
                                   Date = tim_mng.Date,
                                   InTime = tim_mng.InTime,
                                   OutTime = tim_mng.OutTime

                               }).FirstOrDefault();

                    

                    //SelectList EmpList = (SelectList)GetNames(Model.BranchID).Data;
                    //model.EmployeeList = EmpList.Select(x => new SelectListItem { Text = x.Text, Value = x.Value }).ToList();

                    model.BranchList = GetBranches();
                    model.EmployeeList = GetNames(Model.BranchID);
                    model.EmployeeId = Model.EmployeeId;

                    if (obj != null)
                    {
                        model.IsRecordAvail = true;
                        model.UserID = obj.userID;
                        model.EmployeeName = obj.EmployeeName;
                        model.EmployeeId = obj.EmpID;

                        model.Date = obj.Date;
                        model.InTime = obj.InTime;
                        model.OutTime = obj.OutTime;
                        model.TotalMin = 000;
                        model.BranchID = Model.BranchID;
                        model.IsSubmit = true;


                    }
                    else
                    {
                        model.IsRecordAvail = false;
                        model.IsSubmit = true;

                    }


                    model.EmployeeList = GetNames(1);
                    //model.BranchList = GetBranches();
                    //model.Date = null;
                    //model.EmployeeId = null;
                    return View(model);
                }

    //TimeEntry Model
                else
                {
                    Model.EmployeeList = GetNames(1);
                    int userId = Convert.ToInt32(Session["UserID"]);
                    string folderPath = Server.MapPath("~/FileManager/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));
                    var Excelfile = Model.excelFile;
                    List<SelectListItem> Branches = GetBranches();
                    Branches.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });
                    Model.BranchList = Branches;
                    // string errorMessage = string.Empty;
                    if (Model.BranchID != 0)
                    {
                        if (Excelfile != null)
                        {
                            //var Extension = Excelfile.FileName.Substring(Excelfile.FileName.LastIndexOf(".") + 1);
                            var Extension = Path.GetExtension(Excelfile.FileName);
                            if ((Extension == ".xlsx") || Extension == ".xls")
                            {
                                if (!Directory.Exists(folderPath))
                                    Directory.CreateDirectory(folderPath);
                                var fileName = Path.GetFileName(Excelfile.FileName);
                                var path = Path.Combine(folderPath, fileName);
                                Excelfile.SaveAs(path);
                                ExcelUtility objExcelUtility = new ExcelUtility();
                                try
                                {                                   
                                    objExcelUtility.ImportExcelToDatabase(path, Model.BranchID);
                                    int BranchID = Model.BranchID;
                                    communicationHelper.DeleteDuplicateTimeEntry(Model.BranchID);
                                    Model.ErrorSuccessMessage = "Excel file uploaded successfully.";
                                    Model.EmployeeList = GetNames(1);
                                  //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];


                                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Upload Attendance").Select(o => o.EmailTemplateID).FirstOrDefault();
                                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Upload Attendance").Select(x => x.TemplatePath).FirstOrDefault();
                                    if ((check != null) && (check != 0))
                                    {
                                        var AttendanceUpload = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Upload Attendance")
                                                                join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                select new DSRCManagementSystem.Models.Email
                                                                {
                                                                    To = p.To,
                                                                    CC = p.CC,
                                                                    BCC = p.BCC,
                                                                    Subject = p.Subject,
                                                                    Template = q.TemplatePath
                                                                }).FirstOrDefault();
                                        var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();
                                        string TemplatePathAttendanceUpload = Server.MapPath(AttendanceUpload.Template);
                                        string Attendance = System.IO.File.ReadAllText(TemplatePathAttendanceUpload);  
                                        int LoginUser = (int)Session["UserId"];                                        
                                        var user = db.Users.Where(o => o.UserID == LoginUser && o.IsActive == true).Select(o => o).FirstOrDefault();
                                                                              
                                        Attendance = Attendance.Replace("#ServerName", ServerName);                                       
                                        Attendance = Attendance.Replace("#CompanyName", company.AppValue);
                                        string date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE).ToString("dd-mm-yyyy hh:mm:ss tt");
                                        Attendance = Attendance.Replace("#IndianTime", date);
                                        var BranchName = db.Master_Branches.Where(x => x.BranchID == BranchID).Select(o => o.BranchName).FirstOrDefault();                                 
                                        var logo = CommonLogic.getLogoPath();
                                        if (ServerName != "http://win2012srv:88/")
                                        {


                                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                            string EmailAddress = "";

                                            foreach (string mail in MailIds)
                                            {
                                                EmailAddress += mail + ",";
                                            }

                                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);                                            
                                            string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";

                                            Task.Factory.StartNew(() =>
                                            {

                                                DsrcMailSystem.MailSender.SendMail(null, AttendanceUpload.Subject + " - Test Mail Please Ignore", null, Attendance + " - Testing Please ignore", "Test-admin@dsrc.co.in", EmailAddress, "aruna.m@dsrc.co.in", BCCMailId, Server.MapPath(logo.ToString()));
                                            });
                                        }
                                        else
                                        {

                                            Task.Factory.StartNew(() =>
                                            {

                                                DsrcMailSystem.MailSender.SendMail(null, AttendanceUpload.Subject, null, Attendance, "admin@dsrc.co.in", user.EmailAddress,"umapathy@dsrc.co.in", null, null, Server.MapPath(logo.ToString()));

                                            });
                                        }
                                    }   


                                    //string Subject = "Regarding Attendance upload";
                                    //string pathvalue = CommonLogic.getLogoPath();
                                    //var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                    //DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                                    //var BranchName = db.Master_Branches.Where(x => x.BranchID == BranchID).Select(o => o.BranchName).FirstOrDefault();
                                    //var imagePath = new List<string>() { Server.MapPath("~/Content/Template/images/Circle_Red.png"), Server.MapPath("~/Content/Template/images/Circle_Orange.png"), Server.MapPath("~/Content/Template/images/Circle_Green.png"), Server.MapPath(pathvalue) };
                                    //string Message = "Thanks for uploading Attendance on" + " " + indianTime + " " + "<br>" + "for" + BranchName + " " + "branch" + "<br/><br/>This email has been automatically generated.</br>Please do not reply to this email address as all responses are directed to an unattended,<br/>mailbox, and you will not receive a response</p><br/><br/><p style=font-size:15px>Thanks,<br/>" + company + "Management Portal</p>";
                                 
                                    //if (ServerName != "http://win2012srv:88/")
                                    //{

                                    //    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                    //    string EmailAddress = string.Empty;

                                    //    foreach (string mail in MailIds)
                                    //    {
                                    //        EmailAddress += mail + ",";
                                    //    }

                                    //    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                    //    string CCMailId = "kirankumar@dsrc.co.in";
                                    //    string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";
                                        
                                    //    Task.Factory.StartNew(() =>
                                    //    {

                                    //        DsrcMailSystem.MailSender.SendMail(null, Subject + " - Test Mail Please Ignore", null, Message + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress,"aruna.m@dsrc.co.in", BCCMailId, Server.MapPath(pathvalue.ToString()));
                                    //    });
                                    //}
                                    //else
                                    //{
                                    //    Task.Factory.StartNew(() =>
                                    //    {

                                    //        DsrcMailSystem.MailSender.SendMail(null, Subject, null, Message, "admin@dsrc.co.in", "prasanthk@dsrc.co.in , umapathy@dsrc.co.in", null, null, Server.MapPath(pathvalue.ToString()));
                                    //    });
                                    //}


                                    //SendMail(BranchID);

                                }
                                catch (Exception Ex)
                                {
                                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                                    Model.ErrorSuccessMessage = "Error: " + Ex.Message;
                                    return View(Model);
                                }
                            }
                            else
                            {
                                Model.ErrorSuccessMessage = "Please upload excel files only.";
                            }
                        }
                        else
                        {
                            Model.ErrorSuccessMessage = "Please upload the excel file.";
                        }
                    }
                    else
                    {
                        Model.ErrorSuccessMessage = "Please select branch";
                    }

                    //Model.IsSubmit = false;
                    Model.EmployeeList = GetNames(1);
                    //Model.IsSubmit = false;

                    //Model.EmployeeList = GetNames(1);
                    //Model.BranchList = GetBranches();
                    //Model.Date = null;
                    //Model.EmployeeId = null;
                    
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Model);
        }

        private List<SelectListItem> GetBranches()
        {
            var BranchesList = new List<SelectListItem>();
            try
            {

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<Master_Branches> BranchList = db.Master_Branches.ToList();
                    foreach (var item in BranchList)
                    {
                        BranchesList.Add(new SelectListItem { Text = item.BranchName, Value = item.BranchID.ToString() });
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return BranchesList;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailEmployees(int BranchID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();
            try
            {
                if (BranchID != 0)
                {

                    Employees = (from data in db.Users
                                 where data.IsActive == true && data.EmpID != null && data.BranchId == BranchID
                                 select new DSRCEmployees
                                 {
                                     Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                     UserId = data.UserID,
                                     EmployeeId = data.EmpID
                                 }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

                }
                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        private List<SelectListItem> GetNames(int BranchID)
        {
            try
            {
                var NameList = new List<SelectListItem>();

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 where data.IsActive == true && data.EmpID != null && data.BranchId == BranchID && data.UserStatus != 6
                                                 select new DSRCEmployees
                                                 {
                                                     Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                                     UserId = data.UserID,
                                                     EmployeeId = data.EmpID
                                                 }).OrderBy(x => x.Name).ToList();
                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.Name, Value = item.EmployeeId.ToString() });
                    }

                    NameList.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });
                }

                return NameList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }


    }
}
  