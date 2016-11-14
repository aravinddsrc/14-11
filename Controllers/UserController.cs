using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using System.Text.RegularExpressions;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Configuration;
using System.Reflection;
using System.Data.SqlClient;


namespace DSRCManagementSystem.Controllers
{
    [HandleError]
    public class UserController : Controller
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]
        public ActionResult sqlConnectivityError()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NewLogin()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            LoginModel objUserLogin = new LoginModel();
            try
            {
                
                HttpCookie cookie = Request.Cookies["Login"];
                var dbhost = objdb.Master_ApplicationSettings.Where(x => x.AppID == 1).Select(o => o.AppValue).FirstOrDefault();
                Session["Host"] = dbhost;
                var dbhostmail = objdb.Master_ApplicationSettings.Where(x => x.AppID == 2).Select(o => o.AppValue).FirstOrDefault();
                Session["HostMail"] = dbhostmail;
                var Port = objdb.Master_ApplicationSettings.Where(x => x.AppID == 3).Select(o => o.AppValue).FirstOrDefault();
                Session["Port"] = Port;
                var UserName = objdb.Master_ApplicationSettings.Where(x => x.AppID == 4).Select(o => o.AppValue).FirstOrDefault();
                Session["EmailUserName"] = UserName;
                var Password = objdb.Master_ApplicationSettings.Where(x => x.AppID == 5).Select(o => o.AppValue).FirstOrDefault();
                Session["EmailPassword"] = Password;
                var dbcolor = objdb.Master_ApplicationSettings.Where(x => x.AppID == 6).Select(o => o.AppValue).FirstOrDefault();
                Session["Color"] = dbcolor;
                //var dblogo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o.AppValue).FirstOrDefault();
                var dblogo = CommonLogic.getLogoPath();
                Session["NewLoginLogo"] = dblogo;
                var dbfrom = objdb.Master_ApplicationSettings.Where(x => x.AppID == 8).Select(o => o.AppValue).FirstOrDefault();
                Session["FromMail"] = dbfrom;
                var dbto = objdb.Master_ApplicationSettings.Where(x => x.AppID == 9).Select(o => o.AppValue).FirstOrDefault();
                Session["ToMail"] = dbto;
                if (dbcolor != null)
                {
                    objUserLogin.color = dbcolor;
                }

                //if (dblogo != null)
                //{
                //    string[] words;

                //    words = dblogo.Split(new string[] { "~" }, StringSplitOptions.None);

                //if (dblogo != null)
                //{
                //    objUserLogin.path = dblogo;
                //}

                //    string pathvalue = "../.." + words[1];

                //    objUserLogin.path = pathvalue;
                //}
                objUserLogin.path = dblogo;
                if (cookie != null)
                {
                    objUserLogin.UserName = cookie.Values[0];
                    objUserLogin.Password = cookie.Values[1];
                }
                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View("NewLogin", objUserLogin);
        }

        [HttpGet]
        public ActionResult Login()
        {
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                LoginModel objUserLogin = new LoginModel();
                HttpCookie cookie = Request.Cookies["Login"];
                //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                var logo = CommonLogic.getLogoPath();
                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                //string vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string vers = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Version").Select(o => o.AppValue).FirstOrDefault();

                objUserLogin.version = vers;
                objUserLogin.company = objcom.ToString();
                string[] words;
                words = logo.Split(new string[] { "~/" }, StringSplitOptions.None);
                string path = "../../" + words[1];
               string pathImage = HttpContext.Server.MapPath(path.ToString().Replace("../../", "~/"));
              objUserLogin.path = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png"; 
               Session["LoginLogo"] = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
          
                if (cookie != null)
                {
                    objUserLogin.UserName = cookie.Values[0];
                    objUserLogin.Password = cookie.Values[1];
                }
                return View("login", objUserLogin);
            }
            catch (SqlException Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return RedirectToAction("sqlConnectivityError", "User");
            }
        }

        [HttpPost]
        public ActionResult NewLogin(LoginModel objUserLogin)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                int l = Convert.ToInt32(System.Web.HttpContext.Current.Application["Feedback"]);
                int j = Convert.ToInt32(System.Web.HttpContext.Current.Application["Training"]);

                objUserLogin.color = Convert.ToString(Session["Color"]);
                objUserLogin.path = Convert.ToString(Session["Logo"]);

                if (objUserLogin.UserName == null && objUserLogin.Password == null)
                {
                    
                    ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_BothInvalid);
                }
                else
                {
                    System.Web.HttpContext.Current.Application["UserName"] = objUserLogin.UserName.ToString();
                    if (ModelState.IsValid)
                    {
                        using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                        {
                            String password = DSRCLogic.Hashing.Create_SHA256(objUserLogin.Password);

                            var user = db.Users.FirstOrDefault(x => (x.EmailAddress ?? "").Equals(objUserLogin.UserName) && x.IsActive == true);


                            if (user != null && user.Attempts != null && user.Attempts <= 5)
                            {
                                var skill = db.UserSkills.FirstOrDefault(x => x.UserID == user.UserID);
                                if (user.Password.Equals(password))
                                {

                                    Session["IsRerportingPerson"] = db.ReportingPersonsRollIDs.Select(o => o.ReportingPersonRollID).ToList().Contains(db.UserRoles.Where(o => o.UserID == user.UserID).Select(o => o.RoleID).FirstOrDefault());

                                    if (user.IsFirstLogin != true)
                                    {
                                        user.Attempts = 0;
                                        int userId = user.UserID;
                                        Session["UserID"] = user.UserID;
                                        Session["UserName"] = user.EmailAddress;
                                        Session["FirstName"] = user.FirstName;
                                        Session["LastName"] = user.LastName;
                                        Session["BranchID"] = user.BranchId;
                                        //  var roleID = from c in db.Master_Designation where c.DesignationID == user.DesignationID select (int)c.DesignationID;
                                        var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;


                                        int Id = roleID.FirstOrDefault();



                                        Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)user.UserID, Id);

                                        //validate attempts
                                        var attempts =
                                        (from ord in db.Users
                                         where ord.UserID == user.UserID && ord.IsActive == true
                                         select ord).FirstOrDefault();
                                        attempts.Attempts = user.Attempts;
                                        db.SaveChanges();
                                        //var themelogo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();
                                        var themelogo = CommonLogic.getLogoPath();
                                        //Session["LoginLogo"] = themelogo.AppValue;
                                        Session["LoginLogo"] = themelogo;

                                        // Gets the role privilege of the logged in user and sets in session to avoid multiple DB hits during authorization..

                                        //    var roleObj = db.Master_Designation.First(item => item.DesignationID == 1);
                                        var roleobj = db.UserRoles.First(item => item.RoleID == Id);

                                        Session["RoleId"] = Id;
                                        Session["RoleName"] = db.Master_Roles.Where(x => x.RoleID == Id).Select(o => o.RoleName).FirstOrDefault();
                                        FormsAuthentication.SetAuthCookie(user.EmpID, false);
                                        Session["LoggedIn"] = InsertAuditValues(Request.Browser.Browser + " " + Request.Browser.Version, Convert.ToString(Session["FirstName"]), Convert.ToString(Session["UserName"]), Id);


                                        if (user.EmailAddress == "" || user.EmpID == "" || user.FirstName == "" || user.LastName == "" || user.IPAddress == null || user.DateOfBirth == null || user.DateOfJoin == null || user.ContactNo == null || user.PermanentAddressID == 0 || user.Department == null || skill.Skills == null || user.Workplace == null || user.MaritalStatus == null)
                                        {
                                            //Session["IsRequired"] = false;
                                            return RedirectToAction("Index", "Home");
                                            // return RedirectToAction("ViewProfile", "Profile");
                                        }
                                        else
                                        {
                                            //Session["IsRequired"] = true;
                                            return RedirectToAction("Index", "Home");
                                        }

                                    }
                                    else
                                    {
                                        user.Attempts = 0;
                                        int userId = user.UserID;
                                        Session["UserID"] = user.UserID;
                                        Session["UserName"] = user.UserName;
                                        Session["FirstName"] = user.FirstName;
                                        Session["LastName"] = user.LastName;
                                        Session["BranchID"] = user.BranchId;
                                        //    var roleID = from c in db.Master_Designation where c.DesignationID == userId select (int)c.DesignationID;
                                        var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;

                                        int Id = roleID.FirstOrDefault();

                                        // int Id = 1;
                                        Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)user.UserID, Id);

                                        Session["Host Name"] = (from r in db.ComputerAssigneds
                                                                join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                                join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                                select rrr.ComputerName).FirstOrDefault();
                                        Session["Memory"] = (from r in db.ComputerAssigneds
                                                             join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                             join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                             select rrr.Memory).FirstOrDefault();
                                        Session["OS"] = (from r in db.ComputerAssigneds
                                                         join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                         join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                         join or in db.Master_Os on rrr.OsId equals or.OsId
                                                         select or.OsName).FirstOrDefault();
                                        Session["IP"] = (from r in db.ComputerAssigneds
                                                         join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                         join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                         select rrr.IP).FirstOrDefault();
                                        Session["Extra"] = (from r in db.ComputerAssigneds
                                                            join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                            join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                            select rrr.IP).FirstOrDefault();



                                        //Validate Attempts
                                        var attempts =
                                      (from ord in db.Users
                                       where ord.UserID == user.UserID && ord.IsActive == true
                                       select ord).FirstOrDefault();
                                        attempts.Attempts = user.Attempts;
                                        db.SaveChanges();


                                        // Gets the role privilege of the logged in user and sets in session to avoid multiple DB hits during authorization..

                                        var roleObj = db.Master_Roles.First(item => item.RoleID == Id);

                                        Session["RoleId"] = Id;
                                        Session["RoleName"] = roleObj.RoleName;
                                        FormsAuthentication.SetAuthCookie(user.EmpID, false);
                                        user.IsFirstLogin = false;
                                        db.SaveChanges();
                                        Session["LoggedIn"] = InsertAuditValues(Request.Browser.Browser + " " + Request.Browser.Version, Convert.ToString(Session["FirstName"]), Convert.ToString(Session["UserName"]), Id);
                                        return RedirectToAction("ChangePassword", "User");
                                    }
                                    //}
                                }
                                else
                                {



                                    if (user.Attempts == null)
                                    {
                                        user.Attempts = 1;
                                        var query =
                                        (from ord in db.Users
                                         where ord.UserID == user.UserID && ord.IsActive == true
                                         select ord).FirstOrDefault();

                                        query.Attempts = user.Attempts;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        user.Attempts = user.Attempts + 1;
                                        var query =
                                        (from ord in db.Users
                                         where ord.UserID == user.UserID && ord.IsActive == true
                                         select ord).FirstOrDefault();
                                        query.Attempts = user.Attempts;
                                        db.SaveChanges();
                                    }

                                    if (user.Attempts == 3)
                                    {
                                        ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongPassword);
                                    }

                                    else
                                    {
                                        if (user.Attempts > 3 && user.Attempts <= 5)
                                        {
                                            ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongPassword);
                                        }
                                        else if (user.Attempts > 5)
                                        {
                                            ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongAttempt);
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Password", Resources.Resource.VE_UserModels_Login_InvalidPassword);
                                        }
                                    }
                                    db.SaveChanges();
                                    return View(objUserLogin);
                                }
                            }
                            else if (user == null)
                            {


                                ModelState.AddModelError("UserName", Resources.Resource.VE_UserModels_Login_InvalidUserName);

                            }


                            else
                            {
                                if (user.Attempts > 5)
                                {
                                    ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongAttempt);
                                }
                                else
                                {
                                    ModelState.AddModelError("UserName", Resources.Resource.VE_UserModels_Login_InvalidUserName);
                                }

                            }
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
        
            return View(objUserLogin);
        }


        [HttpPost]
        public ActionResult Login(LoginModel objUserLogin)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            var objuser = objdb.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(objUserLogin.UserName) && x.IsActive == true);
           
            var logo = CommonLogic.getLogoPath();
            var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            string vers = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Version").Select(o => o.AppValue).FirstOrDefault();
            objUserLogin.version = vers;
            objUserLogin.company = objcom.ToString();
            // objUserLogin.path = logo.ToString();
            string[] words;
            words = logo.Split(new string[] { "~/" }, StringSplitOptions.None);
            string path = "../../" + words[1];
            string pathImage = HttpContext.Server.MapPath(path.ToString().Replace("../../", "~/"));
            objUserLogin.path = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
            Session["LoginLogo"] = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
            
            //ends
           

            if (objuser == null)
            {
                //added on 9/9
               

              //  var logo = CommonLogic.getLogoPath();
              //  var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
              //  string vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
             //   objUserLogin.version = vers;
             //   objUserLogin.company = objcom.ToString();

              //  var logo = CommonLogic.getLogoPath();
               // var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
               // string vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
             //   string vers = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Version").Select(o => o.AppValue).FirstOrDefault();
              //  objUserLogin.version = vers;
             //   objUserLogin.company = objcom.ToString();

               // objUserLogin.path = logo.ToString();
              //  string[] words;
              //  words = logo.Split(new string[] { "~/" }, StringSplitOptions.None);
              //  string path = "../../" + words[1];
             //   string pathImage = HttpContext.Server.MapPath(path.ToString().Replace("../../", "~/"));
               // objUserLogin.path = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
              //  Session["LoginLogo"] = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
          
            
                //ends
                ModelState.AddModelError("UserName", Resources.Resource.VE_UserModels_Login_InvalidUserName);
            }
                 
            else
            {
        
                try
                {
                    //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                   // var logo = CommonLogic.getLogoPath();
                    var dbcolor = objdb.Master_ApplicationSettings.Where(x => x.AppID == 6).Select(o => o.AppValue).FirstOrDefault();
                    string BrowserVersion = Request.Browser.Browser + " " + Request.Browser.Version;
                    DateTime LoginAt = DateTime.Now;
                    String strHostName = default(string);
                    strHostName = Dns.GetHostName();
                    IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                    IPAddress[] addr = ipEntry.AddressList;
                    string IP = default(string);
                    for (int i = 0; i < addr.Length; i++)
                    {
                        IP = addr[i].ToString();
                    }
                    Session["Color"] = dbcolor;
                   // string vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                   // objUserLogin.version = vers;
                    int l = Convert.ToInt32(System.Web.HttpContext.Current.Application["Feedback"]);
                    int j = Convert.ToInt32(System.Web.HttpContext.Current.Application["Training"]);



                    if (objUserLogin.UserName == null && objUserLogin.Password == null)
                    {
                        int LoginStatusId = MasterEnum.LoginStatus.UserNameandPassWordareInvalid.GetHashCode();
                        var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                        LoginStatus.IPAddress = IP;
                        LoginStatus.BrowserVersion = BrowserVersion;
                        LoginStatus.LogedInDate = LoginAt;
                        LoginStatus.LoginStatusID = LoginStatusId;
                        objdb.Audit_LoginStatus.AddObject(LoginStatus);
                        objdb.SaveChanges();
                        ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_BothInvalid);

                    }
                    else
                    {
                        if (ModelState.IsValid)
                        {
                            System.Web.HttpContext.Current.Application["UserName"] = objUserLogin.UserName.ToString();
                            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                            {
                                String password = DSRCLogic.Hashing.Create_SHA256(objUserLogin.Password);

                                var user = db.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(objUserLogin.UserName) && x.IsActive == true);

                                var masterApplicationSettings = db.Master_ApplicationSettings.FirstOrDefault(o => o.AppKey == "Facebook");

                                if (masterApplicationSettings != null)
                                    Session["FacebookURL"] = masterApplicationSettings.AppValue;

                                if (user != null && user.Attempts != null && user.Attempts <= 5)
                                {
                                    var skill = db.UserSkills.FirstOrDefault(x => x.UserID == user.UserID);
                                    if (user.Password.Equals(password))
                                    {

                                        int LoginStatusId = MasterEnum.LoginStatus.LoginSuccess.GetHashCode();
                                        var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                        LoginStatus.IPAddress = IP;
                                        LoginStatus.BrowserVersion = BrowserVersion;
                                        LoginStatus.LogedInDate = LoginAt;
                                        LoginStatus.LoginStatusID = LoginStatusId;
                                        objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                        objdb.SaveChanges();
                                        Session["IsRerportingPerson"] = db.ReportingUsers.Select(o => o.UserId).ToList().Contains(db.UserRoles.Where(o => o.UserID == user.UserID).Select(o => o.UserID).FirstOrDefault());
                                        //Session["IsRerportingPerson"] = db.ReportingPersonsRollIDs.Select(o => o.ReportingPersonRollID).ToList().Contains(db.UserRoles.Where(o => o.UserID == user.UserID).Select(o => o.RoleID).FirstOrDefault());
                                        if (user.IsFirstLogin != true)
                                        {
                                            user.Attempts = 0;
                                            int userId = user.UserID;
                                            Session["UserID"] = user.UserID;
                                            Session["UserName"] = user.UserName;
                                            Session["FirstName"] = user.FirstName;
                                            Session["LastName"] = user.LastName;
                                            Session["BranchID"] = user.BranchId;
                                            var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
                                            int Id = roleID.FirstOrDefault();
                                            Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)user.UserID, Id);
                                              int userID = Convert.ToInt32(Session["UserID"]);
                                              var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                                            int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                                            int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                                            Session["MailSentCount"] = sentcount;
                                            Session["MailCount"] = inboxcount;
                                            //validate attempts
                                            var attempts =
                                            (from ord in db.Users
                                             where ord.UserID == user.UserID && ord.IsActive == true
                                             select ord).FirstOrDefault();
                                            attempts.Attempts = user.Attempts;
                                            db.SaveChanges();


                                            // Gets the role privilege of the logged in user and sets in session to avoid multiple DB hits during authorization..
                                            var roleObj = db.Master_Roles.First(item => item.RoleID == Id);
                                            Session["RoleId"] = Id;
                                            Session["RoleName"] = roleObj.RoleName;
                                            FormsAuthentication.SetAuthCookie(user.EmpID, false);
                                            Session["LoggedIn"] = InsertAuditValues(Request.Browser.Browser + " " + Request.Browser.Version, Convert.ToString(Session["FirstName"]), Convert.ToString(Session["UserName"]), Id);


                                            // if (user.EmailAddress == "" || user.EmpID == "" || user.FirstName == "" || user.LastName == "" || user.IPAddress == null || user.DateOfBirth == null || user.DateOfJoin == null || user.ContactNo == null || user.PermanentAddress == null || user.Department == null || skill.Skills == null || user.Workplace == null||user.MaritalStatus==null)
                                            if (user.UserName == "" || user.EmpID == "" || user.FirstName == "" || user.LastName == "" || user.IPAddress == null || user.DateOfBirth == null || user.DateOfJoin == null || user.ContactNo == null || user.PermanentAddressID == 0 || user.Department == null || user.Workplace == null || user.MaritalStatus == null)
                                            {
                                                //Session["IsRequired"] = false;
                                                //return RedirectToAction("ViewProfile", "Profile");
                                                return RedirectToAction("Index", "Home");
                                            }
                                            else
                                            {
                                                //Session["IsRequired"] = true;
                                                return RedirectToAction("Index", "Home");
                                            }

                                        }
                                        else
                                        {
                                            user.Attempts = 0;
                                            int userId = user.UserID;
                                            int userID = Convert.ToInt32(Session["UserID"]);
                                            var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                                            int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                                            int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                                            Session["MailSentCount"] = sentcount;
                                            Session["MailCount"] = inboxcount;
                                            Session["UserID"] = user.UserID;
                                            Session["UserName"] = user.UserName;
                                            Session["FirstName"] = user.FirstName;
                                            Session["LastName"] = user.LastName;
                                            Session["BranchID"] = user.BranchId;
                                            var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
                                            int Id = roleID.FirstOrDefault();
                                            Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)user.UserID, Id);

                                            Session["Host Name"] = (from r in db.ComputerAssigneds
                                                                    join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                                    join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                                    select rrr.ComputerName).FirstOrDefault();
                                            Session["Memory"] = (from r in db.ComputerAssigneds
                                                                 join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                                 join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                                 select rrr.Memory).FirstOrDefault();
                                            Session["OS"] = (from r in db.ComputerAssigneds
                                                             join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                             join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                             join or in db.Master_Os on rrr.OsId equals or.OsId
                                                             select or.OsName).FirstOrDefault();
                                            Session["IP"] = (from r in db.ComputerAssigneds
                                                             join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                             join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                             select rrr.IP).FirstOrDefault();
                                            Session["Extra"] = (from r in db.ComputerAssigneds
                                                                join rr in db.Users.Where(o => o.UserID == userId) on r.Userid equals rr.UserID
                                                                join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                                                select rrr.IP).FirstOrDefault();



                                            //Validate Attempts
                                            var attempts =
                                          (from ord in db.Users
                                           where ord.UserID == user.UserID && ord.IsActive == true
                                           select ord).FirstOrDefault();
                                            attempts.Attempts = user.Attempts;
                                            db.SaveChanges();
                                            // Gets the role privilege of the logged in user and sets in session to avoid multiple DB hits during authorization..
                                            var roleObj = db.Master_Roles.First(item => item.RoleID == Id);
                                            Session["RoleId"] = Id;
                                            Session["RoleName"] = roleObj.RoleName;
                                            FormsAuthentication.SetAuthCookie(user.EmpID, false);
                                            user.IsFirstLogin = false;
                                            db.SaveChanges();
                                            Session["LoggedIn"] = InsertAuditValues(Request.Browser.Browser + " " + Request.Browser.Version, Convert.ToString(Session["FirstName"]), Convert.ToString(Session["UserName"]), Id);
                                            return RedirectToAction("ChangePassword", "User");
                                        }

                                    }
                                    else
                                    {
                                        if (user.Attempts == null)
                                        {
                                            user.Attempts = 1;
                                            var query =
                                            (from ord in db.Users
                                             where ord.UserID == user.UserID && ord.IsActive == true
                                             select ord).FirstOrDefault();
                                            query.Attempts = user.Attempts;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            user.Attempts = user.Attempts + 1;
                                            var query =
                                            (from ord in db.Users
                                             where ord.UserID == user.UserID && ord.IsActive == true
                                             select ord).FirstOrDefault();
                                            query.Attempts = user.Attempts;
                                            db.SaveChanges();
                                        }

                                        if (user.Attempts == 3)
                                        {
                                            int LoginStatusId = MasterEnum.LoginStatus.InCorrectPassword.GetHashCode();
                                            var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                            LoginStatus.IPAddress = IP;
                                            LoginStatus.BrowserVersion = BrowserVersion;
                                            LoginStatus.LogedInDate = LoginAt;
                                            LoginStatus.LoginStatusID = LoginStatusId;
                                            objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                            objdb.SaveChanges();
                                            ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongPassword);
                                        }

                                        else
                                        {
                                            if (user.Attempts > 3 && user.Attempts <= 5)
                                            {
                                                int LoginStatusId = MasterEnum.LoginStatus.InCorrectPassword.GetHashCode();
                                                var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                                LoginStatus.IPAddress = IP;
                                                LoginStatus.BrowserVersion = BrowserVersion;
                                                LoginStatus.LogedInDate = LoginAt;
                                                LoginStatus.LoginStatusID = LoginStatusId;
                                                objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                                objdb.SaveChanges();
                                                ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongPassword);
                                            }
                                            else if (user.Attempts > 5)
                                            {
                                                int LoginStatusId = MasterEnum.LoginStatus.WrongAttempt.GetHashCode();
                                                var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                                LoginStatus.IPAddress = IP;
                                                LoginStatus.BrowserVersion = BrowserVersion;
                                                LoginStatus.LogedInDate = LoginAt;
                                                LoginStatus.LoginStatusID = LoginStatusId;
                                                objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                                objdb.SaveChanges();
                                                ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongAttempt);
                                            }
                                            else
                                            {
                                                int LoginStatusId = MasterEnum.LoginStatus.InCorrectPassword.GetHashCode();
                                                var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                                LoginStatus.IPAddress = IP;
                                                LoginStatus.BrowserVersion = BrowserVersion;
                                                LoginStatus.LogedInDate = LoginAt;
                                                LoginStatus.LoginStatusID = LoginStatusId;
                                                objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                                objdb.SaveChanges();
                                                ModelState.AddModelError("Password", Resources.Resource.VE_UserModels_Login_InvalidPassword);
                                            }
                                        }
                                        db.SaveChanges();
                                        return View(objUserLogin);
                                    }
                                }
                                else if (user == null)
                                {
                                    int LoginStatusId = MasterEnum.LoginStatus.InCorrectUserName.GetHashCode();
                                    var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                    LoginStatus.IPAddress = IP;
                                    LoginStatus.BrowserVersion = BrowserVersion;
                                    LoginStatus.LogedInDate = LoginAt;
                                    LoginStatus.LoginStatusID = LoginStatusId;
                                    objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                    objdb.SaveChanges();
                                    ModelState.AddModelError("UserName", Resources.Resource.VE_UserModels_Login_InvalidUserName);

                                }

                                else
                                {
                                    if (user.Attempts > 5)
                                    {
                                        int LoginStatusId = MasterEnum.LoginStatus.WrongAttempt.GetHashCode();
                                        var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                        LoginStatus.IPAddress = IP;
                                        LoginStatus.BrowserVersion = BrowserVersion;
                                        LoginStatus.LogedInDate = LoginAt;
                                        LoginStatus.LoginStatusID = LoginStatusId;
                                        objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                        objdb.SaveChanges();
                                        ModelState.AddModelError("Password", Resources.Resource.VR_UserModels_WrongAttempt);
                                    }
                                    else
                                    {
                                        int LoginStatusId = MasterEnum.LoginStatus.InCorrectUserName.GetHashCode();
                                        var LoginStatus = objdb.Audit_LoginStatus.CreateObject();
                                        LoginStatus.IPAddress = IP;
                                        LoginStatus.BrowserVersion = BrowserVersion;
                                        LoginStatus.LogedInDate = LoginAt;
                                        LoginStatus.LoginStatusID = LoginStatusId;
                                        objdb.Audit_LoginStatus.AddObject(LoginStatus);
                                        objdb.SaveChanges();
                                        ModelState.AddModelError("UserName", Resources.Resource.VE_UserModels_Login_InvalidUserName);
                                    }

                                }
                            }

                        }
                        int LoginStatussId = MasterEnum.LoginStatus.UserNameOrPassWordareNull.GetHashCode();
                        var LoginStatuss = objdb.Audit_LoginStatus.CreateObject();
                        LoginStatuss.IPAddress = IP;
                        LoginStatuss.BrowserVersion = BrowserVersion;
                        LoginStatuss.LogedInDate = LoginAt;
                        LoginStatuss.LoginStatusID = LoginStatussId;
                        objdb.Audit_LoginStatus.AddObject(LoginStatuss);
                        objdb.SaveChanges();
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }

            }
            return View(objUserLogin);
        }

        public ActionResult MyComputerDetails()
        {

                Mycomputerdetails objdetails = new Mycomputerdetails();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                int userID = Convert.ToInt32(Session["UserID"].ToString());



                List<string> obj = new List<string>();



                objdetails.ComputerName = (from r in db.ComputerAssigneds

                                           join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                           where r.Userid == userID
                                           select rrr.ComputerName).FirstOrDefault();
                objdetails.HardDisk = (from r in db.ComputerAssigneds

                                       join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                       where r.Userid == userID
                                       select rrr.Memory).FirstOrDefault();
                objdetails.os = (from r in db.ComputerAssigneds

                                 join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                 where r.Userid == userID
                                 join or in db.Master_Os on rrr.OsId equals or.OsId
                                 select or.OsName).FirstOrDefault();
                objdetails.IPAddress = (from r in db.ComputerAssigneds

                                        join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                        where r.Userid == userID
                                        select rrr.IP).FirstOrDefault();
                objdetails.Location = (from r in db.ComputerAssigneds

                                       join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                       where r.Userid == userID
                                       join rr in db.locations on r.Locationid equals rr.locationid
                                       select rr.LocationName).FirstOrDefault();

                objdetails.workstation = (from r in db.ComputerAssigneds

                                          join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                          where r.Userid == userID
                                          select r.WorkStation).FirstOrDefault();
                objdetails.pendrive = (from r in db.ComputerAssigneds

                                       join rrr in db.computermanagements on r.Managementid equals rrr.managementid
                                       where r.Userid == userID
                                       select r.pendriveAccess).FirstOrDefault().ToString();

                var components = db.ComputerAssigneds.Where(x => x.Userid == userID).Select(x => x.ComponentId).ToList();

                List<int> AssetTypeIds = new List<int>();

                foreach (var component in components)
                {
                    if (component != null)
                    {
                        AssetTypeIds.AddRange(component.Split(',').Select(int.Parse).ToArray());
                    }
                }

                var AssetTypes = db.AssetTypes.Where(x => AssetTypeIds.Contains(x.AssetTypeId)).Select(x => x.AssetName).ToArray();




                objdetails.Extra = string.Join(",", AssetTypes);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

                return View(objdetails);
        }

        public static DateTime InsertAuditValues(string Browsername, string FirstName, string LoginID, int RolID)
        {
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            String strHostName = default(string);


            strHostName = Dns.GetHostName();

            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            string IP = default(string);
            for (int i = 0; i < addr.Length; i++)
            {
                IP = addr[i].ToString();
            }
            string Browser = Browsername;
            string OS = Environment.OSVersion.Platform + " " + Environment.OSVersion.Version;
            string name = RegionInfo.CurrentRegion.DisplayName;
            var Auditobj = db.AuditLogs.CreateObject();
            Auditobj.FirstName = FirstName;
            Auditobj.LoginID = LoginID;
            Auditobj.Roles = db.Master_Roles.Where(o => o.RoleID == RolID).Select(o => o.RoleName).FirstOrDefault();
            Auditobj.LogedInDate = indianTime;
            Auditobj.IpAddress = IP;
            Auditobj.BrowserVersion = Browser;
            Auditobj.OsVersion = OS;
            Auditobj.Location = name;
            db.AuditLogs.AddObject(Auditobj);
            db.SaveChanges();

            return Auditobj.LogedInDate.Value;

        }

        public ActionResult ForgotPassword()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            Session["ServerName"] = AppValue.GetFromMailAddress("ServerName");
            UserLoginModel model = new UserLoginModel();
            LoginModel objUserLogin = new LoginModel();
            try
            {
               
                var logo = CommonLogic.getLogoPath();
                            
                var company = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                var objcom = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                string vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                model.company = objcom.ToString();
                var newpath = logo.ToString();
                string[] words;
                words = newpath.Split(new string[] { "~/" }, StringSplitOptions.None);
                string path = "../../" + words[1];
                string pathImage = HttpContext.Server.MapPath(path.ToString().Replace("../../", "~/"));
               var newpath1 = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";
               Session["LoginLogo"] = System.IO.File.Exists(pathImage) == true ? path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png";

              // Session["LoginLogo"] = logo;
           
               if (company != "" && company != null)
               {

                   model.company = company.ToString();
               }
               else
               {

                   model.company = "DSRC";
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
        public ActionResult ForgotPasswordMessage()
        {
            return View();
        }

        public JsonResult BlockUsers(string email)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string Attempt = "";
            var objuser = objdb.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(email) && x.IsActive == true);

            var user = objdb.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(email) && x.IsActive == true);
            if (user != null)
            {
                if ((user.Attempts > 5))
                {

                    Attempt = "OverAttempts";

                }
                else
                {
                    Attempt = "LessAttempts";
                }
            }
           else
            {
                Attempt = "Empty";
            }
            return Json(Attempt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetPasswordMessage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(UserLoginModel obj,string UserName)
        {
            Guid id = Guid.NewGuid();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var selectedcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                obj.company = selectedcompany;

                var userobj = db.Users.Where(i => i.UserName == UserName).FirstOrDefault();
              //  var EmailId = (from u in db.Users.Where(i => i.UserName == obj.Email) select(u.EmailAddress).FirstOrDefault());
                var MailAddress = (from u in db.Users.Where(x => x.UserName == UserName)
                                   select (u.EmailAddress)).FirstOrDefault();

                string ToEmailId = Convert.ToString(MailAddress);

                if (userobj == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email Address");
                }

                else
                {

                    userobj.Key = id.ToString();
                    db.SaveChanges();

                    if (UserName != null)
                    {
                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Forgot Password").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Forgot Password").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objForgetPassword = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Forgot Password")
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
                            string TemplatePathForgetPassword = Server.MapPath(objForgetPassword.Template);
                            string htmlForgetPassword = System.IO.File.ReadAllText(TemplatePathForgetPassword);
                            htmlForgetPassword = htmlForgetPassword.Replace("#UserName", userobj.FirstName + "  " + userobj.LastName);
                            htmlForgetPassword = htmlForgetPassword.Replace("#Guiid", userobj.Key);
                            htmlForgetPassword = htmlForgetPassword.Replace("#UserId", userobj.UserID.ToString());
                            htmlForgetPassword = htmlForgetPassword.Replace("#ServerName",ServerName);
                            htmlForgetPassword = htmlForgetPassword.Replace("#CompanyName", company);
                           // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                            //string[] words;

                            //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                            //string pathvalue = "~/" + words[1];

                            string pathvalue = CommonLogic.getLogoPath();

                            if (ServerName  != "http://win2012srv:88/")
                            {

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");
                                //MailIds.Add("dineshkumar.d@dsrc.co.in");
                                //MailIds.Add("gopika.v@dsrc.co.in");

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                                DsrcMailSystem.MailSender.SendMail(null, objForgetPassword.Subject + " - Test Mail Please Ignore", null, htmlForgetPassword + " - Testing Please ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue));

                            }
                            else
                            {
                                DsrcMailSystem.MailSender.SendMail(null, objForgetPassword.Subject, null, htmlForgetPassword, "HRMS@dsrc.co.in", ToEmailId, Server.MapPath(pathvalue));
                            }
                            ModelState.AddModelError("Email", Resources.Resource.VR_UserModels_Message);
                        }

                        else
                        {
                            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                            ExceptionHandlingController.TemplateMissing("Forgot Password", folder, ServerName);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Invalid Email Address");
                    }

                    System.Web.HttpContext.Current.Application["Email"] = UserName;
                   // return RedirectToAction("ForgotPasswordMessage","User");
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

          // return RedirectToAction("ForgotPasswordMessage","User");
           return Json("Success",JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ChangePasswordUsingGUIID(string Key, string UserId)
        {

            ChangePasswordGUIID objmodel = new ChangePasswordGUIID();
            try
            {
                if (objmodel.Email != null)
                {
                    return View(objmodel);
                }
                else
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                    int UserID = Convert.ToInt32(UserId);

                    var UserObj = db.Users.Where(o => o.UserID == UserID).Select(R => R).FirstOrDefault();

                    if (UserObj.Key == Key)
                    {
                        objmodel.PasswordKey = Key;
                        objmodel.UserId = UserID;
                    }
                    else
                        return RedirectToAction("ResetPasswordMessage", "User");
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }
        [HttpPost]
        public ActionResult ChangePasswordUsingGUIID(ChangePasswordGUIID model)
        {
            try
            {
                string key = model.PasswordKey;
                if (key != null)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                    var KeyCheck = db.Users.Where(o => o.UserID == model.UserId).Select(i => i).FirstOrDefault();

                    if (KeyCheck.Key == key)
                    {
                        if (model.NewPassword == null || model.ReEnterPassword == null)
                        {
                            ModelState.AddModelError("Conform Password", Resources.Resource.VR_UserLoginModels_ChangePassword_NewPassword);
                        }

                        else
                        {
                            if (model.NewPassword.Count() > 4 && model.NewPassword.Count() < 50 || model.ReEnterPassword.Count() > 4 && model.ReEnterPassword.Count() < 50)
                            {
                                if (model.NewPassword != null && model.ReEnterPassword != null)
                                {
                                    if (model.NewPassword == model.ReEnterPassword)
                                    {
                                        var userobj = db.Users.Where(i => i.EmailAddress == KeyCheck.EmailAddress).FirstOrDefault();
                                        if (userobj == null)
                                        {
                                            ModelState.AddModelError("Password", "Invalid Email Address");
                                        }
                                        else
                                        {
                                            userobj.Password = DSRCLogic.Hashing.Create_SHA256(model.NewPassword);
                                            userobj.IsReseted = 1;

                                            userobj.Key = null;

                                            if (userobj.IsFirstLogin == true)
                                                userobj.IsFirstLogin = false;
                                        }
                                        db.SaveChanges();
                                        return RedirectToAction("Login");
                                    }
                                }
                                else if (model.NewPassword != model.ReEnterPassword)
                                {
                                    ModelState.AddModelError("Key", "MisMatchKey");
                                }
                                else
                                {
                                    ModelState.AddModelError("NewPassWord", Resources.Resource.VR_UserLoginModels_ChangePassword_NewPassword);
                                }
                            }
                            else if ((model.ReEnterPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50) && model.NewPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                            {
                                ModelState.AddModelError("ReEnterPassword", Resources.Resource.VR_UserModels_MaxLength);
                            }
                            else if (model.NewPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                            {
                                ModelState.AddModelError("NewPassword", Resources.Resource.VR_UserModels_MaxLength);
                            }
                            else if (model.ReEnterPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                            {
                                ModelState.AddModelError("ReEnterPassword", Resources.Resource.VR_UserModels_MaxLength);
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("ForgotPasswordMessage", "User");
                    }
                }
                else
                {
                    string username = System.Web.HttpContext.Current.Application["Email"].ToString();
                    if (model.NewPassword == null || model.ReEnterPassword == null)
                    {
                        ModelState.AddModelError("Conform Password", Resources.Resource.VR_UserLoginModels_ChangePassword_NewPassword);
                    }

                    else
                    {
                        if (model.NewPassword.Count() > 4 && model.NewPassword.Count() < 50 || model.ReEnterPassword.Count() > 4 && model.ReEnterPassword.Count() < 50)
                        {
                            if (model.NewPassword != null && model.ReEnterPassword != null)
                            {
                                if (model.NewPassword == model.ReEnterPassword)
                                {
                                    //change passwor here
                                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                                    var userobj = db.Users.Where(i => i.EmailAddress == username).FirstOrDefault();
                                    if (userobj == null)
                                    {
                                        ModelState.AddModelError("Password", "Invalid Email Address");
                                    }
                                    else
                                    {
                                        userobj.Password = DSRCLogic.Hashing.Create_SHA256(model.NewPassword);

                                        userobj.Key = null;

                                        if (userobj.IsFirstLogin == true)
                                            userobj.IsFirstLogin = false;
                                    }
                                    db.SaveChanges();
                                    return RedirectToAction("Login");
                                }
                            }
                            else if (model.NewPassword != model.ReEnterPassword)
                            {
                                ModelState.AddModelError("Key", "MisMatchKey");
                            }
                            else
                            {
                                ModelState.AddModelError("NewPassWord", Resources.Resource.VR_UserLoginModels_ChangePassword_NewPassword);
                            }
                        }
                        else if ((model.ReEnterPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50) && model.NewPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                        {
                            ModelState.AddModelError("ReEnterPassword", Resources.Resource.VR_UserModels_MaxLength);
                        }
                        else if (model.NewPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                        {
                            ModelState.AddModelError("NewPassword", Resources.Resource.VR_UserModels_MaxLength);
                        }
                        else if (model.ReEnterPassword.Count() <= 4 || model.ReEnterPassword.Count() > 50)
                        {
                            ModelState.AddModelError("ReEnterPassword", Resources.Resource.VR_UserModels_MaxLength);
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

        public ActionResult ChangePassword()
        {
            try
            {
                if (Session["UserName"] != null)
                {
                    ChangePassword obj = new ChangePassword();
                    obj.UserName = Session["UserName"].ToString();
                    return View(obj);
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
        public ActionResult ChangePassword(ChangePassword model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ViewBag.Message = "";
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {
                        String password = DSRCLogic.Hashing.Create_SHA256(model.OldPassword);
                        string ChangedPassword = DSRCLogic.Hashing.Create_SHA256(model.NewPassword);
                        var record = db.Users.FirstOrDefault(x => x.UserName == model.UserName && x.Password == password);
                        if (record != null)
                        {
                            record.Password = ChangedPassword;
                            db.SaveChanges();

                            var skill = db.UserSkills.FirstOrDefault(x => x.UserID == record.UserID);

                            if (record.EmailAddress == "" || record.EmpID == "" || record.FirstName == "" || record.LastName == "" || record.IPAddress == null || record.DateOfBirth == null || record.DateOfJoin == null || record.ContactNo == null || record.PermanentAddressID == 0 || record.Department == null || skill == null || record.Workplace == null || record.MaritalStatus == null)
                            {
                                //Session["IsRequired"] = false;
                                return RedirectToAction("Index", "Home");
                                // return RedirectToAction("ViewProfile", "Profile");
                            }
                            else
                            {
                                //Session["IsRequired"] = true;
                                return RedirectToAction("Index", "Home");
                            }


                        }
                        ViewBag.Message = "Fail";
                        return View();
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

        public ActionResult RetrieveImage(string id)
        {
            int UserId = Convert.ToInt32(id);
            try
            {
                byte[] cover = GetImageFromDataBase(UserId);
                if (cover != null)
                {
                    return File(cover, "image/jpg");

                }
                else
                {
                    string filename = Server.MapPath(Url.Content("~/Content/Template/images/profile.jpg"));
                    return File(filename, "image/jpg");
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

        public byte[] GetImageFromDataBase(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var image = from temp in db.UserProfiles where temp.UserID == Id select temp.Photo;
            byte[] cover = image.FirstOrDefault();
            return cover;
        }

        public ActionResult Logout()
        {
            Session["Tab"] = "";
            try
            {
                string LoginID = Convert.ToString(Session["UserName"]);
                if (LoginID == "") Response.Redirect("~/User/SessionExpired");
                DateTime LoggedIn = Convert.ToDateTime(Session["LoggedIn"]);
                LoggedIn = LoggedIn.AddMilliseconds(-LoggedIn.Millisecond);

                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var obj = db.AuditLogs.Where(o => o.LoginID == LoginID).OrderByDescending(o => o.LogedInDate).Select(o => o).FirstOrDefault();
                obj.LoggedOutDate = indianTime;
                db.SaveChanges();

                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();
                HttpContext.Application["IsLogout"] = true;

                Glopal.IslogoutPressed = true;
                Glopal.IsSesionExpired = false;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            
            return RedirectToAction("Login", "User");
        }


        public ActionResult UserProfile()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                ModelState.Clear();
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                DSRCManagementSystem.Models.UserModel objUser = new DSRCManagementSystem.Models.UserModel();
                List<DSRCManagementSystem.Models.UserModel> UserList = new List<DSRCManagementSystem.Models.UserModel>();
                UserList = (from u in db.Users.Where(o => o.IsActive != false)
                            join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                            from dn in DeptName.DefaultIfEmpty()
                            where u.BranchId == BranchId  //By Default select only DSRC
                            select new DSRCManagementSystem.Models.UserModel()
                            {
                                UserId = u.UserID,
                                EmpID = u.EmpID,
                                FirstName = u.FirstName,
                                LastName = (u.LastName == null) ? "-" : u.LastName,
                                UserName = u.UserName,
                                Password = u.Password,
                                DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                EmailAddress = u.EmailAddress,
                                IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                IsUnderProbation = u.IsUnderNoticePeriod ?? false,
                                WorkPlace = u.Workplace,


                            }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();


                foreach (var item in UserList)
                {
                    string WorkName;
                    if (Convert.ToInt32(item.WorkPlace) == 1)
                    {
                        WorkName = "DSRC";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 2)
                    {
                        WorkName = "DSRC Haddows Road";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 3)
                    {
                        WorkName = "Client Place";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 4)
                    {
                        WorkName = "Work From Home";
                    }
                    else
                        WorkName = "";




                    UserModel um = new UserModel();
                    um.UserId = item.UserId;
                    um.EmpID = item.EmpID;
                    um.FirstName = item.FirstName;
                    um.LastName = item.LastName;
                    um.UserName = item.UserName;
                    um.Password = item.Password;
                    um.DepartmentName = item.DepartmentName;
                    um.EmailAddress = item.EmailAddress;
                    um.IsUnderNoticePeriod = item.IsUnderNoticePeriod;
                    um.WorkPlace = WorkName;
                    um.IsUnderProbation = item.IsUnderProbation;

                    UserListNew.Add(um);


                }
                var DepartmentList = db.Departments.ToList();
                var BranchList = db.Master_Branches.ToList();


                var List = (from p in db.Master_TypesofEmployees
                            select new
                            {
                                Employees = p.ID,
                                Types = p.Types

                            }).ToList();

                ViewBag.MemberTypes = new SelectList(List, "Employees", "Types");

                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", 1);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_UserProfile", UserListNew);
                }
                ViewBag.Inactive = false;
                ViewBag.IsResigned = false;

            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(UserListNew);
        }


        [HttpPost]
        public ActionResult UserProfile(UserModel model)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            List<DSRCManagementSystem.Models.UserModel> UserList = new List<DSRCManagementSystem.Models.UserModel>();
            List<DSRCManagementSystem.Models.UserModel> ProbaUser = new List<DSRCManagementSystem.Models.UserModel>();
            List<DSRCManagementSystem.Models.UserModel> NoticeUser = new List<DSRCManagementSystem.Models.UserModel>();
            List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();

                if (getBracnch != 1)
                    model.BranchID = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                var BranchList = db.Master_Branches.ToList();

                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", model.BranchID);

                ViewBag.IsResigned = false;

                var List = (from p in db.Master_TypesofEmployees
                            select new
                            {
                                Employees = p.ID,
                                Types = p.Types

                            }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();
                ViewBag.MemberTypes = new SelectList(List, "Employees", "Types");
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                if (model.Employees == 1)
                {
                    model.NewJoinee = true;
                }


                else if (model.Employees == 2)
                {
                    model.NoticePeriod = true;
                }

                else if (model.Employees == 3)
                {
                    model.onboarding = true;
                }

                else if (model.Employees == 4)
                {
                    model.InActive = true;
                }


                if (model.InActive == true)
                {
                    ViewBag.IsResigned = true;
                    UserList = (from u in db.Users.Where(o => o.IsActive == false && o.IsUnderNoticePeriod == true)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    LastworkingDate = u.LastWorkingDate,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    WorkPlace = u.Workplace,


                                }).OrderByDescending(o => o.LastworkingDate.Value.Year).ThenByDescending(o => o.LastworkingDate.Value.Month).ThenByDescending(o => o.LastworkingDate.Value.Day).ToList();

                   // List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();

                    foreach (var item in UserList)
                    {
                        string WorkName;
                        if (Convert.ToInt32(item.WorkPlace) == 1)
                        {
                            WorkName = "DSRC";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 2)
                        {
                            WorkName = "DSRC Haddows Road";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 3)
                        {
                            WorkName = "Client Place";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 4)
                        {
                            WorkName = "Work From Home";
                        }
                        else
                            WorkName = "";


                        UserModel um = new UserModel();
                        um.UserId = item.UserId;
                        um.EmpID = item.EmpID;
                        um.FirstName = item.FirstName;
                        um.LastName = item.LastName;
                        um.UserName = item.UserName;
                        um.Password = item.Password;
                        um.DepartmentName = item.DepartmentName;
                        um.EmailAddress = item.EmailAddress;
                        um.WorkPlace = WorkName;
                        UserListNew.Add(um);

                    }


                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                    var DepartmentList = db.Departments.ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = true;
                    ViewBag.Block = true;
                    return View(UserList);
                }
                else if (model.NewJoinee == true)
                {
                    ProbaUser = (from u in db.Users.Where(o => o.IsActive == true && o.IsUnderProbation == true)
                                 join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                 where u.BranchId == model.BranchID
                                 orderby u.DateOfJoin
                                 select new DSRCManagementSystem.Models.UserModel()
                                 {
                                     UserId = u.UserID,
                                     EmpID = u.EmpID,
                                     FirstName = u.FirstName,
                                     LastName = (u.LastName == null) ? "-" : u.LastName,
                                     UserName = u.UserName,
                                     DepartmentName = (d.DepartmentName == null) ? "-" : d.DepartmentName,
                                     EmailAddress = u.EmailAddress,
                                     WorkPlace = u.Workplace,


                                 }).ToList();
                    //List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
                    foreach (var item in ProbaUser)
                    {
                        string WorkName;
                        if (Convert.ToInt32(item.WorkPlace) == 1)
                        {
                            WorkName = "DSRC";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 2)
                        {
                            WorkName = "DSRC Haddows Road";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 3)
                        {
                            WorkName = "Client Place";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 4)
                        {
                            WorkName = "Work From Home";
                        }
                        else
                            WorkName = "";


                        UserModel um = new UserModel();
                        um.UserId = item.UserId;
                        um.EmpID = item.EmpID;
                        um.FirstName = item.FirstName;
                        um.LastName = item.LastName;
                        um.UserName = item.UserName;
                        um.Password = item.Password;
                        um.DepartmentName = item.DepartmentName;
                        um.EmailAddress = item.EmailAddress;
                        um.WorkPlace = WorkName;
                        UserListNew.Add(um);

                    }

                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = true;
                    ViewBag.Block = true;
                    return View(ProbaUser);
                }

                else if (model.NoticePeriod == true)
                {
                    NoticeUser = (from u in db.Users.Where(o => o.IsActive == true && o.IsUnderNoticePeriod == true)
                                  join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                  where u.BranchId == model.BranchID
                                  orderby u.ResignedOn
                                  select new DSRCManagementSystem.Models.UserModel()
                                  {
                                      UserId = u.UserID,
                                      EmpID = u.EmpID,
                                      FirstName = u.FirstName,
                                      LastName = (u.LastName == null) ? "-" : u.LastName,
                                      UserName = u.UserName,
                                      ResignedOn = u.ResignedOn,
                                      DepartmentName = (d.DepartmentName == null) ? "-" : d.DepartmentName,
                                      EmailAddress = u.EmailAddress,
                                      WorkPlace = u.Workplace,


                                  }).ToList();
                    //List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
                    foreach (var item in NoticeUser)
                    {
                        string WorkName;
                        if (Convert.ToInt32(item.WorkPlace) == 1)
                        {
                            WorkName = "DSRC";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 2)
                        {
                            WorkName = "DSRC Haddows Road";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 3)
                        {
                            WorkName = "Client Place";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 4)
                        {
                            WorkName = "Work From Home";
                        }
                        else
                            WorkName = "";


                        UserModel um = new UserModel();
                        um.UserId = item.UserId;
                        um.EmpID = item.EmpID;
                        um.FirstName = item.FirstName;
                        um.LastName = item.LastName;
                        um.UserName = item.UserName;
                        um.Password = item.Password;
                        um.DepartmentName = item.DepartmentName;
                        um.EmailAddress = item.EmailAddress;
                        um.WorkPlace = WorkName;
                        UserListNew.Add(um);

                    }


                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = true;
                    ViewBag.Block = true;
                    return View(NoticeUser);

                }


                else if (model.onboarding == true)
                {
                    NoticeUser = (from u in db.Users.Where(o => o.IsActive == true && o.IsBoarding == true)
                                  join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                  where u.BranchId == model.BranchID
                                  orderby u.ResignedOn
                                  select new DSRCManagementSystem.Models.UserModel()
                                  {
                                      UserId = u.UserID,
                                      EmpID = u.EmpID,
                                      FirstName = u.FirstName,
                                      LastName = (u.LastName == null) ? "-" : u.LastName,
                                      UserName = u.UserName,
                                      ResignedOn = u.ResignedOn,

                                      DepartmentName = (d.DepartmentName == null) ? "-" : d.DepartmentName,
                                      EmailAddress = u.EmailAddress,
                                      WorkPlace = u.Workplace,

                                  }).ToList();
                    
                    foreach (var item in NoticeUser)
                    {
                        string WorkName;
                        if (Convert.ToInt32(item.WorkPlace) == 1)
                        {
                            WorkName = "DSRC";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 2)
                        {
                            WorkName = "DSRC Haddows Road";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 3)
                        {
                            WorkName = "Client Place";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 4)
                        {
                            WorkName = "Work From Home";
                        }
                        else
                            WorkName = "";


                        UserModel um = new UserModel();
                        um.UserId = item.UserId;
                        um.EmpID = item.EmpID;
                        um.FirstName = item.FirstName;
                        um.LastName = item.LastName;
                        um.UserName = item.UserName;
                        um.Password = item.Password;
                        um.DepartmentName = item.DepartmentName;
                        um.EmailAddress = item.EmailAddress;
                        um.WorkPlace = WorkName;

                        UserListNew.Add(um);

                    }


                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = true;
                    ViewBag.Block = true;
                    return View(NoticeUser);

                }







                else
                {

                    UserList = (from u in db.Users.Where(o => o.IsActive != false)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,

                                }).ToList();
                   // List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
                    foreach (var item in UserList)
                    {
                        string WorkName;
                        if (Convert.ToInt32(item.WorkPlace) == 1)
                        {
                            WorkName = "DSRC";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 2)
                        {
                            WorkName = "DSRC Haddows Road";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 3)
                        {
                            WorkName = "Client Place";
                        }
                        else if (Convert.ToInt32(item.WorkPlace) == 4)
                        {
                            WorkName = "Work From Home";
                        }
                        else
                            WorkName = "";




                        UserModel um = new UserModel();
                        um.UserId = item.UserId;
                        um.EmpID = item.EmpID;
                        um.FirstName = item.FirstName;
                        um.LastName = item.LastName;
                        um.UserName = item.UserName;
                        um.Password = item.Password;
                        um.DepartmentName = item.DepartmentName;
                        um.EmailAddress = item.EmailAddress;
                        um.IsUnderNoticePeriod = item.IsUnderNoticePeriod;
                        um.WorkPlace = WorkName;

                        UserListNew.Add(um);


                    }


                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                    var DepartmentList = db.Departments.ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = false;
                    
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(UserListNew);
        }


        public ActionResult NewUser(List<int> TechList1)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            //int userId = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            try
            {

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType

                                      }).ToList();

                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();


                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();

                var EmployeeList = (from p in db.Users.Where(x => x.IsActive == true && x.UserID != obj_Users.UserId)
                                    select new
                                    {
                                        ID1 = p.UserID,
                                        UserName1 = p.FirstName + " " + p.LastName

                                    }).ToList();

                var DepartmentList = db.Departments.ToList();
                var Projects = db.Projects.ToList();
                var RoleNameList = db.Master_Roles.ToList();
                var DesignationList = db.Master_Designation.ToList();
                var BranchList = db.Master_Branches.ToList();

                ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }.Union(DepartmentList), "DepartmentId", "DepartmentName", 0);

                ViewBag.RoleIdList = new SelectList(new[] { new Master_Roles() { RoleID = 0, RoleName = "---Select---" } }.Union(RoleNameList), "RoleID", "RoleName", 0);
                ViewBag.Designation = new SelectList(new[] { new Master_Designation() { DesignationID = 0, DesignationName = "---Select---" } }.Union(DesignationList), "DesignationID", "DesignationName", 0);
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");
                ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value");
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", -1);
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", -1);
                ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology");
                ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), "BranchID", "BranchName", 0);
                ViewBag.Email1 = new MultiSelectList(EmployeeList, "Id1", "UserName1");
                ViewBag.Region = new SelectList(Zone, "RegionId", "Region", 3);

                obj_Users.IsBoarding = true;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj_Users);
        }

        public List<YearDropDown> YearsList()
        {
            List<YearDropDown> yearList = new List<YearDropDown>() { new YearDropDown() { Year = "---Select---", YearId = -1 } };
            foreach (int i in Enumerable.Range(0, 99))
            {
                yearList.Add(new YearDropDown() { Year = i.ToString(), YearId = i });
            }

            return yearList;
        }
        private List<Master_Technologies> LoadTechnologies()
        {
            List<Master_Technologies> Technology = new List<Master_Technologies>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_Technologies
                              select data).OrderBy(x => x.Tecnology).ToList();
            }
            return Technology;
        }
        private List<Master_ORM> LoadORM()
        {
            List<Master_ORM> Technology = new List<Master_ORM>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_ORM
                              select data).OrderBy(x => x.ORM_Tools).ToList();
            }
            return Technology;
        }
        private List<Master_DataBaseTechnolgy> LoadDB()
        {
            List<Master_DataBaseTechnolgy> Technology = new List<Master_DataBaseTechnolgy>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_DataBaseTechnolgy
                              select data).OrderBy(x => x.Database_Tools).ToList();
            }
            return Technology;
        }

        public List<MonthDropDown> MonthsList()
        {
            List<MonthDropDown> monthList = new List<MonthDropDown>() { new MonthDropDown() { Month = "---Select---", MonthId = -1 } };
            foreach (int i in Enumerable.Range(0, 12))
            {
                monthList.Add(new MonthDropDown() { Month = i.ToString(), MonthId = i });
            }

            return monthList;
        }

        [HttpGet]
        public ActionResult EditUser(String Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                //if(userId!=(Session["UserID"]))

                

                var empid = Id.ToString();
                var DepartmentList = (from d in db.Departments

                                      select new
                                      {
                                          DepartmentId = d.DepartmentId,
                                          DepartmentName = d.DepartmentName
                                      });
                var RoleNameList = (from us in db.Users
                                    join ur in db.UserRoles on us.UserID equals ur.UserID
                                    join r in db.Master_Designation on ur.RoleID equals r.DesignationID

                                    select new
                                    {
                                        RoleID = r.DesignationID,
                                        RoleName = r.DesignationName
                                    }).ToList();



                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();


                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();




                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                ViewBag.RoleIdList = new SelectList(RoleNameList, "RoleID", "RoleName");
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");


                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join ur in db.UserRoles on u.UserID equals ur.UserID
                                     join r in db.Master_Roles on ur.RoleID equals r.RoleID
                                     from dn in DeptID.DefaultIfEmpty()
                                     where u.EmpID == empid
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         DepartmentName = dn.DepartmentName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         LastName = u.LastName,
                                         UserName = u.UserName,

                                         Password = u.Password,
                                         DateOfBirth = u.DateOfBirth ?? DateTime.Now,
                                         DateOfJoin = u.DateOfJoin ?? DateTime.Now,
                                         ContactNo =u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         IPAddress = u.IPAddress,
                                         MachineName = u.MachineName,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,

                                     }).First();
                return View(userViewModel);
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
        public ActionResult EditUser(UserModel model)
        {
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var obj1 = db.Users.Where(o => o.UserID == model.UserId).Select(o => o).FirstOrDefault();
                    int userId = Convert.ToInt32(Session["UserID"]);
                    //if (model.UserId != (Session["UserID"]))

                    var obj = db.Users.Where(o => o.EmpID == model.EmpID).Select(o => o).FirstOrDefault();
                    obj.FirstName = model.FirstName;
                    obj.LastName = model.LastName;
                    obj.DepartmentId = model.DepartmentId;
                    obj.Gender = model.Gender == "Male" ? 1 : 2;
                    obj.DateOfJoin = model.DateOfJoin;
                    db.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult UserDetails(int Id)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            //ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            try
            {
                obj_Users.UserId = Id;

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType


                                      }).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();
                var DepartmentList = (from us in db.Departments

                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).ToList();

                var DesignationList = (from
                                    r in db.Master_Designation
                                       select new
                                       {
                                           DesignationID = r.DesignationID,
                                           DesignationName = r.DesignationName
                                       }).ToList();

                var TechList = (from
                                tl in db.Master_Technologies
                                select new
                                {
                                    ID = tl.ID,
                                    Tecnology = tl.Tecnology
                                }).ToList();

                var EmployeeList = (from p in db.Users.Where(x => x.IsActive == true && x.UserID != obj_Users.UserId)
                                    select new
                                    {
                                        ID1 = p.UserID,
                                        UserName1 = p.FirstName + " " + p.LastName
                                    }).ToList();


                var BranchesList = (from b in db.Master_Branches
                                    select new
                                    {
                                        BranchID = b.BranchID,
                                        BranchName = b.BranchName
                                    }).ToList();


                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();


                obj_Users.UserId = Id;

                var Isblock = db.Users.Where(x => x.UserID == obj_Users.UserId && x.Attempts > 5).Select(x => x.UserID).FirstOrDefault();

                List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();

                objuser = (from p in db.UserReportings.Where(x => x.UserID == Id)
                           select new DSRCManagementSystem.Models.ListReporting
                            {
                                Id = p.ReportingUserID

                            }).ToList();

                List<int> selectedAttendees = new List<int>();


                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                }


                ViewBag.Email1 = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);

                var regionid = db.Users.Where(x => x.UserID == Id).Select(o => o.Region).FirstOrDefault();

                ViewBag.Region = new SelectList(Zone, "RegionId", "Region", regionid);




                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID
                                     join us in db.UserSkills.DefaultIfEmpty() on u.UserID equals us.UserID into check
                                     from user in check.DefaultIfEmpty()
                                     from dn in DeptID.DefaultIfEmpty()

                                     where u.UserID == Id
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         DepartmentName = dn.DepartmentName,
                                         DepartmentId = u.DepartmentId,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         LastName = u.LastName,
                                         UserName = u.UserName,
                                         Gender = g.GenderName,
                                         GenderID = u.Gender,
                                         Password = u.Password,
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth),
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin),
                                         ContactNo =u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         ResignedOn = u.ResignedOn,
                                         LastworkingDate = u.LastWorkingDate,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         onboarding = u.IsBoarding ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         Block = Isblock > 0 ? true : false,
                                         IsBoarding = u.IsBoarding ?? false,
                                         Experience = u.Experience,
                                         ID = user.ID,
                                         Tecnology = user.Skills,
                                         WorkPlace = u.Workplace,
                                         Marital = m.MaritalStatusType,
                                         MaritalStatusId = (int)u.MaritalStatus,
                                         BranchID = (int)u.BranchId,

                                         BranchName = b.BranchName

                                     }).FirstOrDefault();



                if (userViewModel != null)
                {

                    userViewModel.DateOfJoin = userViewModel.DateOfJoin == null ? userViewModel.DateOfJoin : userViewModel.DateOfJoin.Value.Date;


                    var selected = from d in db.Departments where d.DepartmentId == userViewModel.DepartmentId select new { DepartmentId = d.DepartmentId };

                    var temp = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);



                    string[] experience = null;

                    if (userViewModel.Experience != null)
                    {
                        experience = userViewModel.Experience.Split('.');
                    }
                    else
                    {
                        experience = "0.0".Split('.');
                    }

                    if (userViewModel.WorkPlace != null)
                    {
                        userViewModel.WorkplaceId = Convert.ToInt32(userViewModel.WorkPlace);
                    }

                    else
                    {
                        userViewModel.WorkplaceId = 1;
                    }

                    userViewModel.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == userViewModel.WorkplaceId).WorkPlaceName;

                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(experience[0]));
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(experience[1]));
                    ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", userViewModel.Tecnology);
                    ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", userViewModel.BranchID);
                    ViewBag.DepartmentIdList = temp;
                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name", userViewModel.WorkPlace);
                    ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName", userViewModel.Gender);
                    ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", userViewModel.Marital);
                    ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", userViewModel.DesignationID);

                }

                else
                {
                    DSRCManagementSystem.Models.UserModel objmodel = new DSRCManagementSystem.Models.UserModel();

                    string[] experience = null;
                    DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                    var join = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DateOfJoin).FirstOrDefault();
                    //   userViewModel.DateOfJoin = Convert.ToDateTime(join)
                    objmodel.DateOfJoin = join.Value.Date;
                    objmodel.MiddleName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MiddleName).FirstOrDefault();
                    objmodel.LastName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.LastName).FirstOrDefault();
                    objmodel.EmailAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.EmailAddress).FirstOrDefault();
                    objmodel.UserName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.UserName).FirstOrDefault();
                    objmodel.DateOfBirth = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DateOfJoin).FirstOrDefault();
                    var No = objdb.Users.Where(x => x.UserID == Id).Select(o => o.ContactNo).FirstOrDefault();
                    objmodel.ContactNo = No;
                    objmodel.IPAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IPAddress).FirstOrDefault();
                    objmodel.MachineName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MachineName).FirstOrDefault();
                    objmodel.DirectReportingTo = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DirectReportingTo).FirstOrDefault();
                    bool? notice = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsUnderNoticePeriod).FirstOrDefault();
                    objmodel.IsUnderNoticePeriod = Convert.ToBoolean(notice);
                    bool? propation = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsUnderProbation).FirstOrDefault();
                    objmodel.IsUnderProbation = Convert.ToBoolean(propation);
                    bool? first = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsFirstLogin).FirstOrDefault();
                    objmodel.IsFirstLogin = Convert.ToBoolean(first);
                    bool? isactive = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsActive).FirstOrDefault();
                    objmodel.IsActive = Convert.ToBoolean(isactive);
                    objmodel.PAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.PermanentAddressID).FirstOrDefault();
                    objmodel.TAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.TemporaryAddressID).FirstOrDefault();
                    objmodel.ResignedOn = objdb.Users.Where(x => x.UserID == Id).Select(o => o.ResignedOn).FirstOrDefault();
                    objmodel.LastworkingDate = objdb.Users.Where(x => x.UserID == Id).Select(o => o.LastWorkingDate).FirstOrDefault();
                    bool? board = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsBoarding).FirstOrDefault();
                    objmodel.IsBoarding = Convert.ToBoolean(board);
                    var emp = objdb.Users.Where(x => x.UserID == Id).Select(o => o).FirstOrDefault();

                    if (emp.EmpID == null)
                    {
                        objmodel.EmpID = "";
                    }

                    var Technology = objdb.UserSkills.Where(x => x.UserID == Id).Select(o => o.Skills).FirstOrDefault();
                    var BranchId = objdb.Users.Where(x => x.UserID == Id).Select(o => o.BranchId).FirstOrDefault();
                    var WorkPlace = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Workplace).FirstOrDefault();
                    var Gender = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Gender).FirstOrDefault();
                    var Maritial = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MaritalStatus).FirstOrDefault();
                    var Designation = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DesignationID).FirstOrDefault();
                    var Exp = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Experience).FirstOrDefault();
                    if (Exp != null)
                    {
                        experience = Exp.Split('.');
                    }
                    else
                    {
                        experience = "0.0".Split('.');
                    }
                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(experience[0]));
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(experience[1]));
                    ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", Technology);
                    ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", BranchId);
                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name", WorkPlace);
                    ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName", Gender);
                    ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", Maritial);
                    ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", Designation);
                    return View(objmodel);

                }
                return View(userViewModel);
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
        public ActionResult UserDetails(UserModel model)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            //ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var Isblock = db.Users.Where(x => x.UserID == model.UserId && x.Attempts > 5).Select(x => x.UserID).FirstOrDefault();


                List<int?> objuser = new List<int?>();

                string[] value = model.multiselectemployees.Split(',');

                for (int i = 0; i < value.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(value[i]));
                }



                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID
                                     join us in db.UserSkills.DefaultIfEmpty() on u.UserID equals us.UserID into check
                                     from user in check.DefaultIfEmpty()
                                     from dn in DeptID.DefaultIfEmpty()

                                     where u.UserID == model.UserId
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         DepartmentName = dn.DepartmentName,
                                         DepartmentId = u.DepartmentId,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         RegionId = u.Region,
                                         LastName = u.LastName,
                                         UserName = u.UserName,
                                         Gender = u.Gender == 1 ? "Male" : "Female",
                                         GenderID = u.Gender,
                                         Password = u.Password,
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth),
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin),
                                         ContactNo =u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         ResignedOn = u.ResignedOn,
                                         LastworkingDate = u.LastWorkingDate,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         Block = Isblock > 0 ? true : false,
                                         IsBoarding = u.IsBoarding ?? false,
                                         Experience = u.Experience,
                                         ID = user.ID,
                                         Tecnology = user.Skills,
                                         WorkPlace = u.Workplace,
                                         Marital = u.MaritalStatus == 1 ? "Married" : "Single",
                                         MaritalStatusId = u.MaritalStatus,
                                         BranchID = (int)u.BranchId,
                                         BranchName = b.BranchName

                                         // BranchName = u.BranchId == 1 ? db.Master_Branches.FirstOrDefault(o => o.BranchID == 1).BranchName : db.Master_Branches.FirstOrDefault(o => o.BranchID == 2).BranchName                                     

                                     }).FirstOrDefault();





                var DepartmentList = (from
                                      d in db.Departments
                                      select new
                                      {
                                          DepartmentId = d.DepartmentId,
                                          DepartmentName = d.DepartmentName
                                      });

                var DesignationList = (from
                                    r in db.Master_Designation

                                       select new
                                       {
                                           DesignationID = r.DesignationID,
                                           DesignationName = r.DesignationName
                                       }).ToList();

                var TechList = (from
                                tl in db.Master_Technologies
                                select new
                                {
                                    ID = tl.ID,
                                    Tecnology = tl.Tecnology
                                }).ToList();
                //var WorkList = (from c in db.Users
                //                select new
                //                {
                //                    WorkPlace = c.Workplace
                //                }).ToList();

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType

                                      }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     WorkPlace = p.WorkPlaceName

                                 }).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();



                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", model.DepartmentName);
                ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", model.DesignationName);
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName", model.Gender);
                ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", model.Tecnology);
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name", model.WorkPlace);
                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", model.ExperienceYear);
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", model.ExperienceMonth);
                ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", model.Marital);

                DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();

                var a = Regex.Matches(model.EmpID, @"[a-zA-Z]").Count;

                string EmployeeID = model.EmpID;

                if (model.EmpID.Length <= 5 && a == 0 || a != 0)
                {

                    if (model.EmpID.Length == 1 && a == 0)
                    {
                        model.EmpID = "0000" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 2 && a == 0)
                    {
                        model.EmpID = "000" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 3 && a == 0)
                    {
                        model.EmpID = "00" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 4 && a == 0)
                    {
                        model.EmpID = "0" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (a != 0)
                    {
                        model.EmpID = model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                }


                model.BranchID = Convert.ToInt32(model.BranchName);

                var empidCheck = db.Users.Where(o => o.UserID != model.UserId).FirstOrDefault(x => x.EmpID == model.EmpID && x.BranchId == model.BranchID);
                var emailAddressCheck = db.Users.Where(o => o.UserID != model.UserId).FirstOrDefault(x => x.EmailAddress == model.EmailAddress);

                if (empidCheck == null)
                {
                    if (emailAddressCheck == null)
                    {

                        if ((model.EmailAddress.Trim().EndsWith("@dsrc.co.in", StringComparison.OrdinalIgnoreCase)) || (model.EmailAddress.Trim().EndsWith("@ford.com", StringComparison.OrdinalIgnoreCase)) || (model.EmailAddress.Trim().EndsWith("@dsrc.com", StringComparison.OrdinalIgnoreCase)) || (model.EmailAddress.Trim().EndsWith("@dsrc-cid.in", StringComparison.OrdinalIgnoreCase)))
                        {

                            Glopal.IsSesionExpired = false;


                            var obj = db.Users.Where(o => o.UserID == model.UserId).Select(o => o).FirstOrDefault();
                            var objreporting = db.UserReportings.Where(x => x.UserID == model.UserId).Select(o => o).ToList();
                            obj.DepartmentId = Convert.ToInt32(model.DepartmentName);

                            if (model.EmpID != null && model.EmailAddress != null)
                            {
                                obj.EmpID = model.EmpID;
                                obj.EmailAddress = model.EmailAddress;
                                obj.UserName = model.EmailAddress;
                                obj.IsBoarding = model.IsBoarding;
                            }

                            obj.EmpID = model.EmpID;
                            obj.FirstName = model.FirstName;
                            obj.MiddleName = model.MiddleName;
                            obj.LastName = model.LastName;
                            obj.DateOfBirth = model.DateOfBirth;
                            obj.EmailAddress = model.EmailAddress;
                            obj.DateOfJoin = model.DateOfJoin;
                            obj.Region = model.RegionId;
                            obj.ContactNo = model.ContactNo == null ? (long?)null : Convert.ToInt64(model.ContactNo);
                            obj.Gender = Convert.ToInt32(model.GenderID);
                            obj.MachineName = model.MachineName;
                            obj.IsUnderProbation = model.IsUnderProbation;
                            obj.IsUnderNoticePeriod = model.IsUnderNoticePeriod;
                            obj.IsActive = model.IsActive;
                            obj.Experience = model.ExperienceYear + "." + model.ExperienceMonth;
                            var Workplace = Convert.ToString(model.WorkplaceId);

                            obj.Workplace = Convert.ToString(model.WorkplaceId);

                            // obj.Workplace = db.WorkPlaces.Where(x => x.WorkPlaceID == model.WorkplaceId).Select(x => x.WorkPlaceName).FirstOrDefault();

                            obj.MaritalStatus = model.MaritalStatusId;
                            obj.BranchId = model.BranchID;
                            obj.DesignationID = Convert.ToInt32(model.DesignationName);

                            if (model.IsUnderNoticePeriod)
                            {
                                obj.ResignedOn = model.ResignedOn;
                                obj.LastWorkingDate = model.LastworkingDate;
                            }
                            else
                            {
                                obj.ResignedOn = null;
                                obj.LastWorkingDate = null;
                            }

                            if (model.IsActive)
                            {
                                if (model.LastworkingDate.HasValue && model.LastworkingDate.Value.Date < DateTime.Today.Date)
                                {
                                    obj.IsActive = false;
                                }
                                else
                                {
                                    obj.IsActive = true;
                                }
                            }
                            if (model.Block)
                            {
                                obj.Attempts = 6;
                            }
                            else
                            {
                                obj.Attempts = 0;
                            }
                            db.SaveChanges();


                            var skillobj = db.UserSkills.Where(o => o.UserID == model.UserId).Select(o => o).FirstOrDefault();

                            if (skillobj != null)
                            {
                                skillobj.Skills = model.Tecnology;
                                db.SaveChanges();
                            }
                            else
                            {
                                var skill = db.UserSkills.CreateObject();
                                skill.UserID = model.UserId;
                                skill.Skills = model.Tecnology;

                                db.UserSkills.AddObject(skill);
                                db.SaveChanges();
                            }

                            var FirstNametrim = obj.FirstName.Trim();
                            var LastNametrim = obj.LastName.Trim();


                            int WorkplaceId = Convert.ToInt32(Workplace);

                            UpdateUser updateuser = new Models.UpdateUser();
                            {

                                updateuser.EmpID = obj.EmpID;
                                updateuser.DepartmentName = obj.Department.DepartmentName;
                                updateuser.DesignationName = db.Master_Designation.FirstOrDefault(o => o.DesignationID == obj.DesignationID).DesignationName;
                                updateuser.FirstName = FirstNametrim;
                                updateuser.MiddleName = obj.MiddleName;
                                updateuser.LastName = LastNametrim;
                                updateuser.DateOfBirth = obj.DateOfBirth != null ? obj.DateOfBirth.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                updateuser.DateOfJoin = obj.DateOfJoin != null ? obj.DateOfJoin.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                updateuser.ContactNo = obj.ContactNo;
                                updateuser.EmailAddress = obj.EmailAddress;
                                updateuser.Region = Convert.ToInt32(obj.Region);
                                updateuser.GenderName = obj.Gender == 1 ? "Male" : "Female";
                                updateuser.Experience = obj.Experience;
                                //updateuser.WorkPlace = db.WorkPlaces.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                // updateuser.WorkPlace = db.WorkPlaces.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                updateuser.WorkPlace = Convert.ToString(Workplace);
                                updateuser.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                updateuser.Marital = db.Master_MaritalStatus.FirstOrDefault(o => o.MaritalStatusID == obj.MaritalStatus).MaritalStatusType;
                                updateuser.Skills = model.Tecnology == "Skills" ? "" : model.Tecnology;
                                updateuser.BranchID = (int)obj.BranchId;

                            }


                            SaveUser saveuser = new SaveUser();
                            {
                                if (userViewModel != null)
                                {
                                    saveuser.Region = Convert.ToInt32(userViewModel.RegionId);
                                    saveuser.EmpID = userViewModel.EmpID;
                                    saveuser.DepartmentName = userViewModel.DepartmentName;
                                    saveuser.DesignationName = userViewModel.DesignationName;
                                    saveuser.FirstName = userViewModel.FirstName;
                                    saveuser.MiddleName = userViewModel.MiddleName;
                                    saveuser.LastName = userViewModel.LastName;
                                    saveuser.DateOfBirth = userViewModel.DateOfBirth != null ? userViewModel.DateOfBirth.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                    saveuser.DateOfJoin = userViewModel.DateOfJoin != null ? userViewModel.DateOfJoin.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                    saveuser.ContactNo = userViewModel.ContactNo == 0 ? 0 : userViewModel.ContactNo;
                                    saveuser.EmailAddress = userViewModel.EmailAddress;
                                    saveuser.GenderName = userViewModel.Gender;
                                    saveuser.Experience = userViewModel.Experience;
                                    saveuser.Marital = userViewModel.Marital ?? "";
                                    saveuser.Skills = userViewModel.Tecnology == "Skills" ? "" : userViewModel.Tecnology;
                                    saveuser.BranchID = userViewModel.BranchID;
                                    //  saveuser.WorkPlace = userViewModel.WorkPlaceName;
                                    //saveuser.WorkPlace = userViewModel.WorkPlace;
                                    // saveuser.WorkPlace = db.WorkPlaces.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;                                    
                                    userViewModel.WorkplaceId = Convert.ToInt32(userViewModel.WorkPlace);
                                    //userViewModel.Marital = Convert.ToString(userViewModel.Marital);
                                    saveuser.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == userViewModel.WorkplaceId).WorkPlaceName;
                                }
                                else
                                {
                                    saveuser.EmpID = "";
                                    saveuser.DepartmentName = "";
                                    saveuser.DesignationName = "";
                                    saveuser.FirstName = "";
                                    saveuser.MiddleName = "";
                                    saveuser.LastName = "";
                                    saveuser.DateOfBirth = "";
                                    saveuser.DateOfJoin = "";
                                    saveuser.ContactNo = 0;
                                    saveuser.EmailAddress = "";
                                    saveuser.GenderName = "";
                                    saveuser.Experience = "";
                                    saveuser.Marital = "";
                                    saveuser.Skills = "";
                                    saveuser.WorkPlace = "";
                                }


                            }

                            string MailBody = "";
                            var UserUpdateModel = updateuser;
                            var UserSavedModel = saveuser;
                            List<Variance> diff = extentions.DetailedCompare(UserUpdateModel, UserSavedModel);

                            int i;

                            if (diff.Count > 0)
                            {
                                MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Old Values</p> <br />";

                                for (i = 0; i < diff.Count; i++)
                                {
                                    if (diff[i].UserUpdateValue != null)
                                    {
                                        if (diff[i].UserSaveValue == null)
                                            diff[i].UserSaveValue = "";
                                        //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p>" + System.Environment.NewLine;

                                        MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff[i].FieldName == "ContactNo" ? "MobileNumber" : diff[i].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p><br/>" + System.Environment.NewLine;
                                    }

                                }

                                MailBody += "<br /><p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Updated Values</p>  <br />";

                                for (i = 0; i < diff.Count; i++)
                                {
                                    if (diff[i].UserUpdateValue != null)
                                    {
                                        //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p>" + System.Environment.NewLine;
                                        MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff[i].FieldName == "ContactNo" ? "MobileNumber" : diff[i].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p><br/>" + System.Environment.NewLine;
                                    }
                                }
                            }


                            if (model.IsUnderNoticePeriod == false && model.IsActive != false && diff.Count > 0)
                            {

                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Update").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Update").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objEmpDetailsUpdate = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Update")
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
                                    string TemplatePathEmpDetailUpdate = Server.MapPath(objEmpDetailsUpdate.Template);
                                    string htmlEmpDetailUpdate = System.IO.File.ReadAllText(TemplatePathEmpDetailUpdate);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmpId", Convert.ToString(obj.EmpID));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#CompanyName", company);

                                    objEmpDetailsUpdate.To = db.Users.FirstOrDefault(o => o.UserID == model.UserId).EmailAddress;
                                    objEmpDetailsUpdate.CC = UserController.GetUserEmailAddress(db, objEmpDetailsUpdate.CC);
                                    if (objEmpDetailsUpdate.BCC != null)
                                    {
                                        objEmpDetailsUpdate.BCC = UserController.GetUserEmailAddress(db, objEmpDetailsUpdate.BCC);
                                    }

                                 //   string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    var logo = CommonLogic.getLogoPath();

                                    if (ServerName  != "http://win2012srv:88/")
                                    {

                                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                        //MailIds.Add("boobalan.k@dsrc.co.in");
                                        //MailIds.Add("shaikhakeel@dsrc.co.in");
                                        //MailIds.Add("ramesh.S@dsrc.co.in");
                                        //MailIds.Add("aruna.m@dsrc.co.in");
                                        //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                        //MailIds.Add("dineshkumar.d@dsrc.co.in");
                                        //MailIds.Add("gopika.v@dsrc.co.in");
                                        //MailIds.Add("vennimalai.n@dsrc.co.in");

                                        string EmailAddress = "";

                                        foreach (string mail in MailIds)
                                        {
                                            EmailAddress += mail + ",";
                                        }

                                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                        string CCMailId = "kirankumar@dsrc.co.in";
                                        string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";


                                        //var path = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();
                                        var path = CommonLogic.getLogoPath();


                                        //Session["LogoPath"] = path.AppValue;
                                        Session["LogoPath"] = path;
                                        Task.Factory.StartNew(() =>
                                          {
                                              //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                              // DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                              DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                          });

                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                         {
                                             //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                             //DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", objEmpDetailsUpdate.To, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.AppValue.ToString()));
                                             DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", objEmpDetailsUpdate.To, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.ToString()));

                                         });
                                    }
                                }
                            }
                            else
                            {
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Update").Select(x => x.TemplatePath).FirstOrDefault();
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("Employee Details Update", folder, ServerName);
                            }

                            if (model.IsUnderNoticePeriod == true && userViewModel.IsUnderNoticePeriod == true && model.IsActive != false && diff.Count > 0)
                            {
                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Update").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Update").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var updatedetailsofundernoticeperiod = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Update")
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
                                    string TemplatePathEmpDetailUpdate = Server.MapPath(updatedetailsofundernoticeperiod.Template);
                                    string htmlEmpDetailUpdate = System.IO.File.ReadAllText(TemplatePathEmpDetailUpdate);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmpId", Convert.ToString(obj.EmpID));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ComapanyName", company);


                                    updatedetailsofundernoticeperiod.To = db.Users.FirstOrDefault(o => o.UserID == model.UserId).EmailAddress;
                                    updatedetailsofundernoticeperiod.CC = UserController.GetUserEmailAddress(db, updatedetailsofundernoticeperiod.CC);
                                    if (updatedetailsofundernoticeperiod.BCC != null)
                                    {
                                        updatedetailsofundernoticeperiod.BCC = UserController.GetUserEmailAddress(db, updatedetailsofundernoticeperiod.BCC);
                                    }

                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    var logo = CommonLogic.getLogoPath();

                                    if (ServerName  != "http://win2012srv:88/")
                                    {

                                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                        //List<string> MailIds = new List<string>();

                                        //MailIds.Add("boobalan.k@dsrc.co.in");
                                        //MailIds.Add("shaikhakeel@dsrc.co.in");
                                        //MailIds.Add("ramesh.S@dsrc.co.in");
                                        //MailIds.Add("aruna.m@dsrc.co.in");
                                        //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                        //MailIds.Add("dineshkumar.d@dsrc.co.in");
                                        //MailIds.Add("gopika.v@dsrc.co.in");
                                        string EmailAddress = "";

                                        foreach (string mail in MailIds)
                                        {
                                            EmailAddress += mail + ",";
                                        }

                                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                        string CCMailId = "kirankumar@dsrc.co.in";
                                        string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";


                                        Task.Factory.StartNew(() =>
                                        {
                                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                        });

                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", updatedetailsofundernoticeperiod.To, updatedetailsofundernoticeperiod.CC, updatedetailsofundernoticeperiod.BCC, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", updatedetailsofundernoticeperiod.To, updatedetailsofundernoticeperiod.CC, updatedetailsofundernoticeperiod.BCC, Server.MapPath(logo.ToString()));

                                        });
                                    }
                                }
                                else
                                {
                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Employee Details Update", folder, ServerName);
                                }
                            }


                            else if (model.IsUnderNoticePeriod == true && userViewModel.IsUnderNoticePeriod == false && model.IsActive != false)
                            {
                                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Resigned Employee Details").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Resigned Employee Details").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objResignedEmpDetails = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Resigned Employee Details")
                                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                      select new DSRCManagementSystem.Models.Email
                                                      {
                                                          To = p.To,
                                                          CC = p.CC,
                                                          BCC = p.BCC,
                                                          Subject = p.Subject,
                                                          Template = q.TemplatePath
                                                      }).FirstOrDefault();

                         var projectname = (from u in db.UserProjects join p in db.Projects on u.ProjectID equals p.ProjectID where u.UserID == obj.UserID select new { p.ProjectName }).FirstOrDefault();
                         string ProjectName = projectname == null ? "N/A" : projectname.ProjectName;

                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathResignedEmpDetails = Server.MapPath(objResignedEmpDetails.Template);
                         string htmlResignedEmpDetails = System.IO.File.ReadAllText(TemplatePathResignedEmpDetails);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#EmployeeID", obj.EmpID);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#Department", obj.Department.DepartmentName);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ResignedOn", obj.ResignedOn.Value.ToString("dd MMM yyyy"));
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#LastWorkingDate", obj.LastWorkingDate.Value.ToString("dd MMM yyyy"));
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ProjectName", ProjectName);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ServerName",ServerName);
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#CompanyName", company);
                         objResignedEmpDetails.To = UserController.GetUserEmailAddress(db, objResignedEmpDetails.To);
                         objResignedEmpDetails.CC = UserController.GetUserEmailAddress(db, objResignedEmpDetails.CC);
                         if (objResignedEmpDetails.BCC != "")
                         {
                             objResignedEmpDetails.BCC = UserController.GetUserEmailAddress(db, objResignedEmpDetails.BCC);
                         }

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         var logo = CommonLogic.getLogoPath();

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                             //MailIds.Add("boobalan.k@dsrc.co.in");
                             //MailIds.Add("shaikhakeel@dsrc.co.in");
                             //MailIds.Add("ramesh.S@dsrc.co.in");
                             //MailIds.Add("aruna.m@dsrc.co.in");
                             //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                             //MailIds.Add("dineshkumar.d@dsrc.co.in");
                             //MailIds.Add("gopika.v@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in";

                             string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject + " - Test Mail Please Ignore", null, htmlResignedEmpDetails + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));

                                 DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject + " - Test Mail Please Ignore", null, htmlResignedEmpDetails + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject, "", htmlResignedEmpDetails, "HRMS@dsrc.co.in", objResignedEmpDetails.To, objResignedEmpDetails.CC, objResignedEmpDetails.BCC, Server.MapPath(logo.ToString()));
                                 // DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject, "", htmlResignedEmpDetails, "HRMS@dsrc.co.in", objResignedEmpDetails.To, objResignedEmpDetails.CC, objResignedEmpDetails.BCC, Server.MapPath(logo.AppValue.ToString()));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Resigned Employee Details", folder, ServerName);
                     }
                            }
                            if (model.IsActive == false)
                            {
                                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Deactivate").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Employee Details Deactivate").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objEmpDeactivated = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Deactivate")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppValue == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathEmpDeactivated = Server.MapPath(objEmpDeactivated.Template);
                         string htmlEmpDeactivated = System.IO.File.ReadAllText(TemplatePathEmpDeactivated);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#EmployeeName", obj.FirstName + "  " + obj.LastName);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Department", obj.Department.DepartmentName);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#EmailAddress", obj.EmailAddress);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#JoiningDate", obj.DateOfJoin.Value.ToString("dd MMM yyyy"));
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Experience", obj.Experience);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#ServerName", ServerName);
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlEmpDeactivated = htmlEmpDeactivated.Replace("#CompanyName", company);
                         objEmpDeactivated.To = UserController.GetUserEmailAddress(db, objEmpDeactivated.To);
                         objEmpDeactivated.CC = UserController.GetUserEmailAddress(db, objEmpDeactivated.CC);
                         if (objEmpDeactivated.BCC != "")
                         {
                             objEmpDeactivated.BCC = UserController.GetUserEmailAddress(db, objEmpDeactivated.BCC);
                         }

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
                             //MailIds.Add("dineshkumar.d@dsrc.co.in");
                             //MailIds.Add("gopika.v@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "kirankumar@dsrc.co.in";
                             string BCCMailId = "Kirankumar@dsrc.co.in ";

                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject + " - Test Mail Please Ignore", null, htmlEmpDeactivated + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject + " - Test Mail Please Ignore", null, htmlEmpDeactivated + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {


                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject, "", htmlEmpDeactivated, "HRMS@dsrc.co.in", objEmpDeactivated.To, objEmpDeactivated.CC, objEmpDeactivated.BCC, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject, "", htmlEmpDeactivated, "HRMS@dsrc.co.in", objEmpDeactivated.To, objEmpDeactivated.CC, objEmpDeactivated.BCC, Server.MapPath(logo.ToString()));

                             });
                         }
                     }
                     else
                     {
                        //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                          ExceptionHandlingController.TemplateMissing("Employee Details Deactivate", folder, ServerName);
                     }
                            }




                            for (int y = 0; y < objuser.Count(); y++)
                            {
                                TempData["Null"] = "0";
                                var Value = Convert.ToInt32(objuser[y]);
                                var id = model.UserId;
                                var alreadyvalue = db.UserReportings.Where(x => x.UserID == model.UserId).Select(o => o).ToList();

                                if (alreadyvalue.Count() > objuser.Count())
                                {
                                    if (objuser.Count() == 1)
                                    {

                                        var vps = db.UserReportings.Where(x => x.UserID == id).Select(o => o).ToList();
                                        foreach (var vp in vps)
                                            db.UserReportings.DeleteObject(vp);
                                        db.SaveChanges();

                                        DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                        objc.UserID = model.UserId;
                                        objc.ReportingUserID = Value;
                                        db.AddToUserReportings(objc);
                                        db.SaveChanges();
                                        TempData["message"] = "Added";
                                        return Json(true, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (alreadyvalue.Count() > objuser.Count())
                                {
                                    if (y == 0)
                                    {
                                        var vps = db.UserReportings.Where(x => x.UserID == id).Select(o => o).ToList();
                                        foreach (var vp in vps)
                                            db.UserReportings.DeleteObject(vp);
                                        db.SaveChanges();
                                        TempData["Null"] = "Deleted";
                                    }


                                }

                                if (alreadyvalue.Count == 0)
                                {
                                    for (int d = 0; d < objuser.Count(); d++)
                                    {
                                        DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                        objc.UserID = model.UserId;
                                        objc.ReportingUserID = objuser[d];
                                        db.AddToUserReportings(objc);
                                        db.SaveChanges();
                                    }
                                    TempData["message"] = "Added";
                                    return Json(true, JsonRequestBehavior.AllowGet);
                                }

                                if (TempData["Null"].ToString() == "0")
                                {
                                    if (TempData["Null"].ToString() != "Deleted")
                                    {
                                        if (y < alreadyvalue.Count())
                                        {
                                            alreadyvalue[y].ReportingUserID = Value;
                                            db.SaveChanges();

                                        }
                                        else
                                        {
                                            DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                            objc.UserID = model.UserId;
                                            objc.ReportingUserID = Value;
                                            db.AddToUserReportings(objc);
                                        }
                                    }
                                }
                            }

                            db.SaveChanges();
                            TempData["message"] = "Added";
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        return Json("EmailAddressExisting", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {

                    return Json("EmpIDExisting", JsonRequestBehavior.AllowGet);
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


        [HttpGet]
        public ActionResult UserValidation(UserModel profilemodel)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                if (profilemodel.EmpID != null)
                {
                    DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();
                    var a = Regex.Matches(profilemodel.EmpID, @"[a-zA-Z]").Count;
                    if (profilemodel.EmpID.Length <= 5 && a == 0 || a != 0)
                    {
                        string EmployeeID = profilemodel.EmpID;
                        if (profilemodel.EmpID.Length == 1 && a == 0)
                        {
                            profilemodel.EmpID = "0000" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 2 && a == 0)
                        {
                            profilemodel.EmpID = "000" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 3 && a == 0)
                        {
                            profilemodel.EmpID = "00" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 4 && a == 0)
                        {
                            profilemodel.EmpID = "0" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (a != 0)
                        {
                            profilemodel.EmpID = profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        var empidCheck = db.Users.FirstOrDefault(x => x.EmpID == profilemodel.EmpID && x.BranchId == profilemodel.BranchID);
                        var emailAddressCheck = db.Users.FirstOrDefault(x => x.EmailAddress == profilemodel.EmailAddress);
                        if (empidCheck == null)
                        {
                            if (emailAddressCheck == null)
                            {
                                if (profilemodel.EmailAddress != null)
                                {
                                    if (profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in"))
                                    {

                                        return Json("Success", JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    return Json("EmailAddressNULL", JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("EmailAddressExisting", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json("EmpIDCharc", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json("EmpIDNULL", JsonRequestBehavior.AllowGet);

                return Json("EmpIDExisting", JsonRequestBehavior.AllowGet);
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
        public ActionResult NewUser(UserModel profilemodel)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                string EmployeeID;

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();

                string password = GenerateRandomPassword(10);
                string newpassword = DSRCLogic.Hashing.Create_SHA256(password);
                List<int> objuser = new List<int>();
                string[] value = profilemodel.multiselectemployees.Split(',');

                for (int i = 0; i < value.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(value[i]));
                }

                if (profilemodel.EmpID != null)
                {
                    var a = Regex.Matches(profilemodel.EmpID, @"[a-zA-Z]").Count;
                    if (profilemodel.EmpID.Length <= 5 && a == 0 || a != 0)
                    {
                        if (profilemodel.EmpID.Length == 1 && a == 0)
                        {
                            profilemodel.EmpID = "0000" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 2 && a == 0)
                        {
                            profilemodel.EmpID = "000" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 3 && a == 0)
                        {
                            profilemodel.EmpID = "00" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (profilemodel.EmpID.Length == 4 && a == 0)
                        {
                            profilemodel.EmpID = "0" + profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                        if (a != 0)
                        {
                            profilemodel.EmpID = profilemodel.EmpID;
                            EmployeeID = profilemodel.EmpID;
                        }
                    }
                    var empidCheck = db.Users.FirstOrDefault(x => x.EmpID == profilemodel.EmpID && x.BranchId == profilemodel.BranchID);
                }

                dynamic emailAddressCheck;

                if (profilemodel.EmailAddress != null)
                    emailAddressCheck = db.Users.FirstOrDefault(x => x.EmailAddress == profilemodel.EmailAddress);
                EmployeeID = profilemodel.EmpID;
                var FirstNametrim = profilemodel.FirstName.Trim();
                var LastNametrim = profilemodel.LastName.Trim();
                try
                {
                    MailMessage mail = new MailMessage();

                    var t = new User
                    {
                        CreatedUserID = userId,
                        EmpID = profilemodel.EmpID,
                        DepartmentId = profilemodel.DepartmentId,
                        FirstName = FirstNametrim,
                        LastName = LastNametrim,
                        UserName = profilemodel.EmailAddress,
                        Password = newpassword,
                        DateOfJoin = profilemodel.DateOfJoin,
                        Gender = profilemodel.GenderID,
                        EmailAddress = profilemodel.EmailAddress,
                        IsFirstLogin = true,
                        Region = profilemodel.RegionId,
                        Attempts = 0,
                        IsActive = true,
                        IsBoarding = profilemodel.IsBoarding,
                        Experience = profilemodel.ExperienceYear + "." + profilemodel.ExperienceMonth,
                        Workplace = Convert.ToString(profilemodel.WorkplaceId),
                        MaritalStatus = profilemodel.MaritalStatusId,
                        IsUnderProbation = true,
                        IsExclude = false,
                        BranchId = profilemodel.BranchID,
                        DesignationID = profilemodel.DesignationID
                    };




                    if (profilemodel.EmailAddress != null)
                    {
                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "New User").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "New User").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "New User")
                                              join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                              select new DSRCManagementSystem.Models.Email
                                              {
                                                  To = p.To,
                                                  CC = p.CC,
                                                  BCC = p.BCC,
                                                  Subject = p.Subject,
                                                  Template = q.TemplatePath
                                              }).FirstOrDefault();
                            var company = db.Master_ApplicationSettings.Where(x => x.AppValue == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                            string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                            string newuserhtml = System.IO.File.ReadAllText(TemplatePathnewuser);
                            newuserhtml = newuserhtml.Replace("#UserName", profilemodel.FirstName + "  " + profilemodel.LastName);
                            newuserhtml = newuserhtml.Replace("#LoginID", profilemodel.EmailAddress);
                            newuserhtml = newuserhtml.Replace("#Password", password);
                            newuserhtml = newuserhtml.Replace("#ServerName",ServerName);
                            newuserhtml = newuserhtml.Replace("#CompanyName", company);
                            if (objnewuser.To != "")
                            {
                                objnewuser.To = UserController.GetUserEmailAddress(db, objnewuser.To);
                            }
                            if (objnewuser.CC != "")
                            {
                                objnewuser.BCC = UserController.GetUserEmailAddress(db, objnewuser.CC);
                            }
                            if (objnewuser.BCC != "")
                            {
                                objnewuser.BCC = UserController.GetUserEmailAddress(db, objnewuser.BCC);
                            }

                            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
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
                                //MailIds.Add("dineshkumar.d@dsrc.co.in");
                                //MailIds.Add("gopika.v@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string maiil in MailIds)
                                {
                                    EmailAddress += maiil + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", newuserhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", newuserhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                });

                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", newuserhtml, "HRMS@dsrc.co.in", profilemodel.EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", newuserhtml, "HRMS@dsrc.co.in", profilemodel.EmailAddress, Server.MapPath(logo.ToString()));

                                });
                            }
                        }
                        else
                        {
                            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                            ExceptionHandlingController.TemplateMissing("New User", folder, ServerName);
                        }
                    }
                    db.Users.AddObject(t);
                    db.SaveChanges();
                    // db.SaveChanges();
                    profilemodel.UserId = t.UserID;
                    profilemodel.BranchName = db.Master_Branches.FirstOrDefault(o => o.BranchID == profilemodel.BranchID).BranchName;
                    var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                    string Title = " " + objcom + " New Employee Added";
                    string Subject = " employee was added on " + DateTime.Today.ToString("dd MMM yyyy");


                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "New Employee Add").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "New Employee Add").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((checks != null) && (checks != 0))
                    {
                        var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "New Employee Add")
                                   join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                   select new DSRCManagementSystem.Models.Email
                                   {
                                       To = p.To,
                                       CC = p.CC,
                                       BCC = p.BCC,
                                       Subject = p.Subject,
                                       Template = q.TemplatePath
                                   }).FirstOrDefault();

                        string TemplatePath = Server.MapPath(obj.Template);
                        string html = System.IO.File.ReadAllText(TemplatePath);

                        if (profilemodel.IsBoarding)
                        {
                            Title = " " + objcom + " Onboarding Employee Added";
                            Subject = " onboarding employee was added on " + DateTime.Today.ToString("dd MMM yyyy");
                            obj.Subject = " " + objcom + " Management Portal-Onboarding Employee Added";
                        }


                        html = html.Replace("#Title", Title);
                        html = html.Replace("#Subject", Subject);
                        html = html.Replace("#EmployeeID", profilemodel.EmpID);
                        html = html.Replace("#UserId", profilemodel.UserId.ToString());
                        html = html.Replace("#EmployeeName", profilemodel.FirstName + " " + profilemodel.LastName);
                        html = html.Replace("#BranchName", profilemodel.BranchName);
                        html = html.Replace("#Department", profilemodel.DepartmentName);
                        html = html.Replace("#DesignationName", profilemodel.RoleName);
                        html = html.Replace("#JoiningDate", profilemodel.DateOfJoin.Value.ToString("dd MMM yyyy"));
                        html = html.Replace("#Experience", profilemodel.ExperienceYear + "." + profilemodel.ExperienceMonth);
                        html = html.Replace("#ServerName", ServerName);


                        obj.To = UserController.GetUserEmailAddress(db, obj.To);
                        obj.CC = UserController.GetUserEmailAddress(db, obj.CC);
                        if (obj.BCC != "")
                        {
                            obj.BCC = UserController.GetUserEmailAddress(db, obj.BCC);
                        }


                        //string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];
                        //var logo = CommonLogic.getLogoPath();

                        if (ServerName != "http://win2012srv:88/")
                        {

                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                            //MailIds.Add("boobalan.k@dsrc.co.in");
                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                            //MailIds.Add("ramesh.S@dsrc.co.in");
                            //MailIds.Add("aruna.m@dsrc.co.in");
                            //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                            //MailIds.Add("dineshkumar.d@dsrc.co.in");
                            //MailIds.Add("gopika.v@dsrc.co.in");

                            string EmailAddress = "";

                            foreach (string maiil in MailIds)
                            {
                                EmailAddress += maiil + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            string CCMailId = "kirankumar@dsrc.co.in";
                            string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                            Task.Factory.StartNew(() =>
                            {
                                var logo = CommonLogic.getLogoPath();
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                var logo = CommonLogic.getLogoPath();
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                // DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", obj.To, obj.CC, obj.BCC, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", obj.To, obj.CC, obj.BCC, Server.MapPath(logo.ToString()));
                            });
                        }

                        var user = db.UserRoles.CreateObject();
                        user.UserID = profilemodel.UserId;
                        user.RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == MasterEnum.NewuserRole.NewEmployeeRole).RoleID;
                        db.UserRoles.AddObject(user);
                        db.SaveChanges();

                        var r = (from u in db.Users
                                 where u.DepartmentId == profilemodel.DepartmentId && u.FirstName == profilemodel.FirstName &&
                                       u.DateOfJoin == profilemodel.DateOfJoin
                                 select u.UserID).FirstOrDefault();

                        for (int i = 0; i < objuser.Count(); i++)
                        {
                            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                            DSRCManagementSystem.UserReporting objreporting = new DSRCManagementSystem.UserReporting();
                            objreporting.UserID = objdb.Users.Where(x => x.EmpID == profilemodel.EmpID).Select(o => o.UserID).FirstOrDefault();
                            objreporting.ReportingUserID = Convert.ToInt32(objuser[i]);
                            objdb.AddToUserReportings(objreporting);
                            objdb.SaveChanges();
                        }

                        //Lines added to add data into user skills table.
                        db.UserSkills.AddObject(new UserSkill { UserID = r, Skills = profilemodel.Tecnology });
                        db.SaveChanges();

                    }
                    else
                    {
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("New Employee Add", folders, ServerName);
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                }

                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", profilemodel.ExperienceYear);
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", profilemodel.ExperienceMonth);
                ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", profilemodel.Tecnology);

                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult UpdateValidation(UserModel profilemodel)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();
            try
            {
                var a = Regex.Matches(profilemodel.EmpID, @"[a-zA-Z]").Count;
                if (profilemodel.EmpID.Length <= 5 && a == 0 || a != 0)
                {
                    string EmployeeID = profilemodel.EmpID;
                    if (profilemodel.EmpID.Length == 1 && a == 0)
                    {
                        profilemodel.EmpID = "0000" + profilemodel.EmpID;
                        EmployeeID = profilemodel.EmpID;
                    }
                    if (profilemodel.EmpID.Length == 2 && a == 0)
                    {
                        profilemodel.EmpID = "000" + profilemodel.EmpID;
                        EmployeeID = profilemodel.EmpID;
                    }
                    if (profilemodel.EmpID.Length == 3 && a == 0)
                    {
                        profilemodel.EmpID = "00" + profilemodel.EmpID;
                        EmployeeID = profilemodel.EmpID;
                    }
                    if (profilemodel.EmpID.Length == 4 && a == 0)
                    {
                        profilemodel.EmpID = "0" + profilemodel.EmpID;
                        EmployeeID = profilemodel.EmpID;
                    }
                    if (a != 0)
                    {
                        profilemodel.EmpID = profilemodel.EmpID;
                        EmployeeID = profilemodel.EmpID;
                    }
                    var emailAddressCheck = db.Users.FirstOrDefault(x => x.EmailAddress == profilemodel.EmailAddress);
                    if (emailAddressCheck == null)
                    {
                        if (profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in"))
                        {
                            if (ModelState.IsValid)
                            {
                                return Json("Update", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json("EmailAddress", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("EmailAddressExisting", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("EmpIDExisting", JsonRequestBehavior.AllowGet);
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
        public ActionResult UpdateUser(UserModel profilemodel)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    return View("UserDetails", profilemodel);
                }

                else
                {
                    return View();
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

        [HttpGet]
        public ActionResult DeleteUser(int Id = 0)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var DeleteUsers = db.Users.FirstOrDefault(x => x.UserID == Id);
                if (Id != 0)
                {

                    if (DeleteUsers != null)
                    {

                        DeleteUsers.IsActive = false;
                        db.SaveChanges();
                    }
                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Employee").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Delete Employee").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objDelete = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Employee")
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
                         string TemplatePathDelete = Server.MapPath(objDelete.Template);
                         string htmlDelete = System.IO.File.ReadAllText(TemplatePathDelete);
                         htmlDelete = htmlDelete.Replace("#EmpID", DeleteUsers.EmpID);
                         htmlDelete = htmlDelete.Replace("#Name", DeleteUsers.FirstName + " " + DeleteUsers.LastName);
                         htmlDelete = htmlDelete.Replace("#Department", DeleteUsers.Department.DepartmentName);
                         htmlDelete = htmlDelete.Replace("#JoiningDate", ((DateTime)DeleteUsers.DateOfJoin).ToString("dd MMM yyyy"));
                         htmlDelete = htmlDelete.Replace("#Experience", DeleteUsers.Experience);
                         htmlDelete = htmlDelete.Replace("#ServerName", ServerName);
                         htmlDelete = htmlDelete.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlDelete = htmlDelete.Replace("#CompanyName", company);
                         objDelete.To = UserController.GetUserEmailAddress(db, objDelete.To);
                         objDelete.CC = UserController.GetUserEmailAddress(db, objDelete.CC);
                         if (objDelete.BCC != "")
                         {
                             objDelete.BCC = UserController.GetUserEmailAddress(db, objDelete.BCC);
                         }


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
                             //MailIds.Add("dineshkumar.d@dsrc.co.in");
                             //MailIds.Add("gopika.v@dsrc.co.in");

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "Kirankumar@dsrc.co.in ";
                             string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject + " - Test Mail Please Ignore", null, htmlDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject + " - Test Mail Please Ignore", null, htmlDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                             });

                         }
                         else
                         {

                             Task.Factory.StartNew(() =>
                             {
                                 // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject, "", htmlDelete, "HRMS@dsrc.co.in", objDelete.To, objDelete.CC, objDelete.BCC, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject, "", htmlDelete, "HRMS@dsrc.co.in", objDelete.To, objDelete.CC, objDelete.BCC, Server.MapPath(logo.ToString()));

                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Delete Employee", folder,ServerName);
                     }
                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
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

        private string GenerateRandomPassword(int length)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-*&#+";
            char[] chars = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }

        public ActionResult SessionExpired()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ErrorModel objmodel = new DSRCManagementSystem.Models.ErrorModel();
            //objmodel.Company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            return View(objmodel);
        }


        public string employeeidNO { get; set; }

        [HttpPost]
        public ActionResult OnBoardingUpdateUser(UserModel model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var obj = db.Users.Where(o => o.UserID == model.UserId).Select(o => o).FirstOrDefault();
                obj.DateOfJoin = model.DateOfJoin;
                obj.IsBoarding = model.IsBoarding;
                obj.IsActive = model.IsActive;
                obj.Region = model.RegionId;
                obj.Gender = model.GenderID;
                //obj.Region = model.RegionId;
                obj.Experience = model.ExperienceYear + "." + model.ExperienceMonth;
                obj.MaritalStatus = model.MaritalStatusId;

                obj.Workplace = Convert.ToString(model.WorkplaceId);
                obj.BranchId = Convert.ToInt32(model.BranchName);

                List<int?> objuser = new List<int?>();

                string[] value = model.multiselectemployees.Split(',');

                for (int i = 0; i < value.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(value[i]));
                }

                for (int y = 0; y < objuser.Count(); y++)
                {
                    TempData["Null"] = "0";
                    var Value = Convert.ToInt32(objuser[y]);
                    var id = model.UserId;
                    var alreadyvalue = db.UserReportings.Where(x => x.UserID == id).Select(o => o).ToList();

                    if (alreadyvalue.Count() > objuser.Count())
                    {
                        if (objuser.Count() == 1)
                        {
                            var vps = db.UserReportings.Where(a => a.UserID == id).ToList();
                            foreach (var vp in vps)
                                db.UserReportings.DeleteObject(vp);
                            db.SaveChanges();

                            DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                            objc.UserID = model.UserId;
                            objc.ReportingUserID = Value;
                            db.AddToUserReportings(objc);
                            db.SaveChanges();
                            TempData["message"] = "Added";
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (alreadyvalue.Count() > objuser.Count())
                    {
                        if (y == 0)
                        {
                            var vps = db.UserReportings.Where(a => a.UserID == id).ToList();
                            foreach (var vp in vps)
                                db.UserReportings.DeleteObject(vp);
                            db.SaveChanges();
                            TempData["Null"] = "Deleted";
                        }


                    }

                    if (alreadyvalue.Count == 0)
                    {
                        for (int d = 0; d < objuser.Count(); d++)
                        {
                            DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                            objc.UserID = model.UserId;
                            objc.ReportingUserID = objuser[d];
                            db.AddToUserReportings(objc);
                            db.SaveChanges();
                        }
                        TempData["message"] = "Added";
                        return Json(true, JsonRequestBehavior.AllowGet);


                    }


                    if (TempData["Null"].ToString() == "0")
                    {

                        if (TempData["Null"].ToString() != "Deleted")
                        {
                            if (y < alreadyvalue.Count())
                            {
                                alreadyvalue[y].ReportingUserID = Value;
                                db.SaveChanges();

                            }
                            else
                            {
                                DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                objc.UserID = model.UserId;
                                objc.ReportingUserID = Value;
                                db.AddToUserReportings(objc);
                            }
                        }
                    }
                }

                db.SaveChanges();
                var userSkills = db.UserSkills.Select(us => us).FirstOrDefault();
                userSkills.UserID = model.UserId;
                userSkills.Skills = model.Tecnology;
                db.SaveChanges();
                TempData["message"] = "Added";
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [HttpGet]
        public ActionResult ProjectAgenda(int id)
        {
            DSRCManagementSystem.Models.AgandaForProject objagenda = new DSRCManagementSystem.Models.AgandaForProject();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
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
                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectAgenda(AgandaForProject objagenda)
        {
            try
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

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [HttpGet]
        public ActionResult ProjectFeedBack(int id)
        {
            DSRCManagementSystem.Models.ProjectFeedBack objagenda = new DSRCManagementSystem.Models.ProjectFeedBack();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objagenda);
        }

        [HttpPost]
        public ActionResult ProjectFeedBack(ProjectFeedBack ovj, AgandaForProject objagenda)
        {
           
            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();
            try
            {
                ovj.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Application["id"]);
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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();

        }
        [HttpGet]
        public ActionResult MOM(int id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ProjectMom objmom = new DSRCManagementSystem.Models.ProjectMom();
            try
            {
                var value = objdb.AgendaFeedbacks.Where(x => x.ProjectId == id).Select(o => o.MOM).FirstOrDefault();
                if (value != null)
                {
                    objmom.ProjectMOM = value.ToString();

                }
                else
                {
                    objmom.ProjectMOM = "";
                }
                System.Web.HttpContext.Current.Application["agenda"] = id;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmom);
        }
        [HttpPost]
        public ActionResult MOM(ProjectMom objmom)
        {
            DSRCManagementSystemEntities1 ob = new DSRCManagementSystemEntities1();
            try
            {
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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult ProjectMeeting()
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                var ProjectList = objdb.Projects.ToList();

                ProjectMeetingTime objtime = new ProjectMeetingTime();



                var ProjectLead = (from t in objdb.Users
                                   join atn in objdb.MeetingGuids on t.UserID equals atn.UserId

                                   select new
                                   {
                                       Id = t.UserID,

                                       FirstName = t.FirstName,
                                       LastName = t.LastName,


                                   }).ToList();


                var Days = objdb.Master_Days.ToList();


                ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id", "FirstName", "LastName");
                ViewBag.DayList = new SelectList(new[] { new Master_Days() { Id = 0, Days = "------Select--------" } }.Union(Days), "Id", "Days", 0);
                ViewBag.Projects = new SelectList(new[] { new Project() { ProjectID = 0, ProjectName = "----Select------" } }.Union(ProjectList), "ProjectID", "ProjectName", 0);



                ViewBag.Week = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);

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

        public ActionResult ProjectMeeting(ProjectMeetingTime objmeeting)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
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
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();

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
            try
            {
                editobj.Id = ID;

                var ProjectLead = (from t in db.Users
                                   join atn in db.MeetingGuids on t.UserID equals atn.UserId

                                   select new
                                   {
                                       Id = t.UserID,
                                       FirstName = t.FirstName
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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(editobj);
        }

        [HttpPost]
        public ActionResult EditAttendee(int Id, string Attendee)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var ReqToEdit = db.MettingSchedules.FirstOrDefault(o => o.Id == Id);

                ReqToEdit.Attendees = Attendee;
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [HttpGet]
        public ActionResult MeetingSchedule()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = Convert.ToInt32(Session["UserID"]);
                var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();


                var date = DateTime.Now;

                DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);

                while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                    date = date.AddDays(1);

                var result = (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;

                ViewBag.Week = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "1", Value = 1 }, new { Text = "2", Value = 2 } }, "Value", "Text", 0);


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
                            meetingSchedule.Attendees = UserController.GetUserString(db, meetingSchedule.Attendees);
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
                            meetingSchedule.Attendees = UserController.GetUserString(db, meetingSchedule.Attendees);
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
                            meetingSchedule.Attendees = UserController.GetUserString(db, meetingSchedule.Attendees);
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
                            meetingSchedule.Attendees = UserController.GetUserString(db, meetingSchedule.Attendees);

                            if (result % 2 == meetingSchedule.Week / 2)
                                meetingSchedule.Date = nextWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                            else
                                meetingSchedule.Date = currentWeek[meetingSchedule.Day].ToString("dd/MM/yyyy");
                        }
                    }

                    return View(objmail);
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


        [HttpGet]
        public ActionResult ViewUser(int Id)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            try
            {

                var DepartmentList = (from us in db.Departments

                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).ToList();
                var RoleNameList = (from


                                    r in db.Master_Designation


                                    select new
                                    {
                                        DesignationID = r.DesignationID,
                                        DesignationName = r.DesignationName
                                    }).ToList();



                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID

                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID
                                     join us in db.UserSkills.DefaultIfEmpty() on u.UserID equals us.UserID into check
                                     from user in check.DefaultIfEmpty()

                                     from dn in DeptID.DefaultIfEmpty()
                                     where u.UserID == Id
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         DepartmentName = dn.DepartmentName,
                                         DepartmentId = u.DepartmentId,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         LastName = u.LastName,
                                         UserName = u.UserName,
                                         Gender = u.Gender == 1 ? "Male" : "Female",
                                         GenderID = u.Gender,
                                         Password = u.Password,
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth),
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin),
                                         ContactNo =u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         ResignedOn = u.ResignedOn,
                                         LastworkingDate = u.LastWorkingDate,

                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         IsBoarding = u.IsBoarding ?? false

                                     }).FirstOrDefault();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();

                userViewModel.DateOfJoin = userViewModel.DateOfJoin == null ? userViewModel.DateOfJoin : userViewModel.DateOfJoin.Value.Date;
                var selected = from d in db.Departments where d.DepartmentId == userViewModel.DepartmentId select new { DepartmentId = d.DepartmentId };

                var temp = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);

                ViewBag.DepartmentIdList = temp;

                ViewBag.RoleIdList = new SelectList(RoleNameList, "DesignationID", "DesignationName", userViewModel.DesignationID);

                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");
                return View(userViewModel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpGet]
        public ActionResult History(int ProjectId)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.History> objhis = new List<DSRCManagementSystem.Models.History>();

            try
            {
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
                                  Date = p.FeedbackDate,
                                  MOM = p.MOM
                              }).OrderByDescending(x => x.Date).ToList();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objhis);
        }
        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.FirstName + " " + (user.LastName ?? "")).ToList();
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

        private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.EmailAddress).ToList();
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

        [HttpGet]
        public ActionResult ResetPassword(int UserID, string EmailAddress)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                int UID = UserID;
                string MailAddress = EmailAddress;
                int PasswordKey = Convert.ToInt32(GenerateRandomPasswordKey(5));
                var UpdateKey = db.Users.FirstOrDefault(x => x.UserID == UID);

                Guid id = Guid.NewGuid();

                if (UID != 0)
                {
                    var obj = db.Users.Where(o => o.UserID == UID).Select(o => o).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.PasswordKey = PasswordKey;
                        obj.Key = id.ToString();
                        db.SaveChanges();

                        if (MailAddress != null && MailAddress != "")
                        {

                            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Reset Password").Select(o => o.EmailTemplateID).FirstOrDefault();
                            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Reset Password").Select(x => x.TemplatePath).FirstOrDefault();
                            if ((check != null) && (check != 0))
                            {
                                var objResetPwd = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Reset Password")
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
                                string TemplatePathResetPwd = Server.MapPath(objResetPwd.Template);
                                string htmlResetPwd = System.IO.File.ReadAllText(TemplatePathResetPwd);
                                htmlResetPwd = htmlResetPwd.Replace("#UserName", obj.FirstName + " " + obj.LastName);
                                htmlResetPwd = htmlResetPwd.Replace("#UserID", UserID.ToString());
                                htmlResetPwd = htmlResetPwd.Replace("#Guiid", obj.Key);
                                htmlResetPwd = htmlResetPwd.Replace("#Key", Convert.ToString(PasswordKey));
                                htmlResetPwd = htmlResetPwd.Replace("#ServerName", ServerName);
                                htmlResetPwd = htmlResetPwd.Replace("#CompanyName", company);
                               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                if (ServerName  != "http://win2012srv:88/")
                                {
                                    //List<string> MailIds = new List<string>();

                                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                    //MailIds.Add("boobalan.k@dsrc.co.in");
                                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                                    //MailIds.Add("ramesh.S@dsrc.co.in");
                                    //MailIds.Add("aruna.m@dsrc.co.in");
                                    //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                    //MailIds.Add("kirankumar@dsrc.co.in");
                                    //MailIds.Add("francispaul.k.c@dsrc.co.in");
                                    //MailIds.Add("dineshkumar.d@dsrc.co.in");
                                    //MailIds.Add("gopika.v@dsrc.co.in");



                                    string EmailAddres = "";

                                    foreach (string mail in MailIds)
                                    {
                                        EmailAddres += mail + ",";
                                    }

                                    // EmailAddres = EmailAddres.Remove(EmailAddres.Length - 1);

                                    DsrcMailSystem.MailSender.SendMail(null, objResetPwd.Subject + " - Test Mail Please Ignore", null, htmlResetPwd + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddres, null);


                                }
                                else
                                {
                                    DsrcMailSystem.MailSender.SendMail(null, objResetPwd.Subject, null, htmlResetPwd, "HRMS@dsrc.co.in", MailAddress, Server.MapPath(Session["LoginLogo"].ToString()));
                                }



                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                              //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("Reset Password", folder, ServerName);
                            }
                        }
                        else
                        {
                            return Json(new { Result = "Failer", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
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

        private string GenerateRandomPasswordKey(int length)
        {
            string AllowedNo = "0123456789";
            char[] Key = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                Key[i] = AllowedNo[rd.Next(0, AllowedNo.Length)];
            }
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string strkey = new string(Key);
            Int32 pwkey = Convert.ToInt32(strkey);
            if (db.Users.Where(o => o.PasswordKey == pwkey).Any())
            {
                //key exists
                GenerateRandomPasswordKey(5);
            }
            return new string(Key);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDepartments(int BranchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
            try
            {

                if (BranchId != 0)
                {

                    List<int> validDepart = new List<int>();

                    validDepart = db.Departments.Where(d => d.BranchID == BranchId).Select(d => d.DepartmentId).ToList();

                    FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                    select new FilterDepartment()
                                    {
                                        DepartmentId = lt.DepartmentId,
                                        DepartmentName = lt.DepartmentName
                                    }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
                }
                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidateSession()
        {
            return View();
        }
        public ActionResult ClearTempSession()
        {
            return View();
        }
    }
}
