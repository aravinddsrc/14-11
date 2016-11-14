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
using System.Configuration;
using System.Data.SqlClient;





namespace DSRCManagementSystem.Controllers
{
    public class QuickEnrollController : Controller
    {
        //
        // GET: /QuickEnroll/
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();
        public ActionResult QuickEnrole()
        {

            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


            var Enroldetails = (from u in db.QuickEnrolls
                                where u.IsActive == true 
                                join v in db.Master_Designation on u.RoleID equals (byte)v.DesignationID
                                into a
                                from b in a.DefaultIfEmpty()
                                select new QuickEnrollment()
                                {
                                    QuickEnroll = u.QuickEnroll1,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    Address = u.Address,
                                    Location = u.Location,
                                    PhoneNumber = u.PhoneNumber,
                                    DateOfEnquiry = (DateTime)u.DateOfEnquiry,
                                    GenderID = u.GenderID,
                                    RollID = (byte)u.RoleID,
                                    rollName = b.DesignationName,
                                    //DateOfBirth = (DateTime)u.DateOfBirth,
                                    DateOfJoin = (DateTime)u.DateOfJoin,
                                    Experience = u.Experience,
                                    PersonalEmailAddress = u.PersonalMailAddress,
                                    //DepartmentList = u.DepartmentID,
                                    //DepartmentGroup = u.DepartmentGroup,
                                    Comments = u.Comments


                                    //}).ToList();
                                }).OrderBy(u => u.DateOfJoin).ThenBy(u=>u.FirstName );

            return View(Enroldetails);



        }
        [HttpGet]
        public ActionResult AddNewRoll()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //var EmpID = db.QuickEnrolls.Select(o=>o).OrderByDescending(o => o.QuickEnroll1).First();
            Session["currDate"] = System.DateTime.Now.Date.ToShortDateString();

            try
            {
                var EmpID = (from d in db.QuickEnrolls select d.QuickEnroll1).Max();
                ViewBag.EmpID = Convert.ToInt32(EmpID) + 1;
            }
            catch (Exception ex)
            {
                ViewBag.EmpID = 1;
            }

            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();

            try
            {
                QuickEnrollment role = new QuickEnrollment();

                var DepartmentList = (from us in db.Departments
                                      orderby us.DepartmentName
                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).OrderBy(o => o.DepartmentName).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();


                var UserType = (from u in db.Master_Designation
                                select new
                                {
                                    RoleID = u.DesignationID,
                                    RoleName = u.DesignationName
                                }).OrderBy(o => o.RoleName).ToList();

                var GroupList = db.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                {
                    GroupId = g.GroupID,
                    GroupName = g.GroupName
                }).ToList();
                var BranchList = db.Master_Branches.ToList();
                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName");

                var FirstName = role.FirstName;
                var LastName = role.LastName;
                var Address = role.Address;
                var Location = role.Location;
                var PhoneNumber = role.PhoneNumber;
                var DateOfBirth = role.DateOfBirth;
                var DateOfJoin = role.DateOfJoin;
                var Experience = role.Experience;
                var PersonalEmailAddress = role.PersonalEmailAddress;
                var Comments = role.Comments;
                var Department = role.DepartmentList;
                var DepartmentGroup = role.DepartmentGroup;
                var GenderID = role.GenderID;


                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year");
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month");
                //ViewBag.GroupList = new SelectList(GroupList, "GroupId", "GroupName");
                ViewBag.GroupList = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }, "GroupID", "GroupName", 0);
               // ViewBag.GroupList = new SelectList(GroupList, "GroupID", "GroupName");
                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                ViewBag.Gender = new SelectList(GenderNameList, "GenderID ", "GenderName");
                ViewBag.usertype = new SelectList(UserType, "RoleID", "RoleName");

            }

            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddNewRoll(QuickEnrollment role)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            string exp = "--";
            string Date = null;
            string Date1 = null;
            //DateTime todate = (Convert.ToDateTime(role.DateOfJoin));  // added
            var AddRoles = db.QuickEnrolls.CreateObject();
            AddRoles.FirstName = role.FirstName;
            AddRoles.LastName = role.LastName;
            AddRoles.Address = role.Address;
            AddRoles.Location = role.Location;
            AddRoles.PhoneNumber = role.PhoneNumber;
            //if (role.DateOfBirth != DateTime.MinValue)
            //{
            //    AddRoles.DateOfBirth = Convert.ToDateTime(role.DateOfBirth);
            //}
            AddRoles.BranchID = role.BranchID;
            if (role.DateOfJoin != null)
            {
                AddRoles.DateOfJoin = Convert.ToDateTime(role.DateOfJoin);
                DateTime todate = (Convert.ToDateTime(role.DateOfJoin));
                Date = todate.ToString("dd-MM-yyyy");
            }
            if (role.DateOfEnquiry != null)
            {
                AddRoles.DateOfEnquiry = Convert.ToDateTime(role.DateOfEnquiry);
                DateTime endate = (Convert.ToDateTime(role.DateOfEnquiry));
                Date1 = endate.ToString("dd-MM-yyyy");
            }
            //added on 9/9 
            if (role.Experience != "undefined.undefined")
            {
                exp = role.Experience;
            }
            else
            {
                exp = "0";
            }
            //ends
            AddRoles.Experience = role.Experience;
            AddRoles.PersonalMailAddress = role.PersonalEmailAddress;
            AddRoles.DepartmentID = role.DepartmentName;
            AddRoles.RoleID = (byte)role.RollID;
            AddRoles.GenderID = role.GenderID;
            AddRoles.DateOfEnquiry = role.DateOfEnquiry;
            if (role.DepartmentGroup != 0)
            {
                AddRoles.DepartmentGroup = role.DepartmentGroup;
            }
            AddRoles.Comments = role.Comments;
            AddRoles.Experience = role.Experience;
            AddRoles.IsActive = true;
            db.QuickEnrolls.AddObject(AddRoles);
            db.SaveChanges();

            if (role.QuickEnroll != null)
            {
                var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "AddEnroll").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "AddEnroll").Select(x => x.TemplatePath).FirstOrDefault();
                if ((checks != null) && (checks != 0))
                {

                    var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "AddEnroll")
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
                    string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                    string AddEnrollhtml = System.IO.File.ReadAllText(TemplatePathnewuser);

                    AddEnrollhtml = AddEnrollhtml.Replace("#Name", role.FirstName + "  " + role.LastName);
                    AddEnrollhtml = AddEnrollhtml.Replace("#JoiningDate",Date);
                    AddEnrollhtml = AddEnrollhtml.Replace("#Experience", exp); //role.Experience);
                    AddEnrollhtml = AddEnrollhtml.Replace("#Date", Date);

                    AddEnrollhtml = AddEnrollhtml.Replace("#ServerName", ServerName);
                    AddEnrollhtml = AddEnrollhtml.Replace("#CompanyName", company);
                    if (objnewuser.To != "")
                    {
                        objnewuser.To = QuickEnrollController.GetUserEmailAddress(db, objnewuser.To);
                    }
                    if (objnewuser.CC != "")
                    {
                        objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.CC);
                    }
                    if (objnewuser.BCC != "")
                    {
                        // objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.BCC);
                    }

                    // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    var logo = CommonLogic.getLogoPath();
                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    int userId = int.Parse(Session["UserID"].ToString());
                    var userdetails = db.Users.FirstOrDefault(o => o.UserID == userId);
                    string MailID = userdetails.EmailAddress;

                    if (ServerName != "http://win2012srv:88/")
                    {
                        List<string> MailIds = new List<string>();

                        MailIds.Add("boobalan.k@dsrc.co.in");
                        // MailIds.Add("shaikhakeel@dsrc.co.in");
                        MailIds.Add("ramesh.S@dsrc.co.in");
                        MailIds.Add("aruna.m@dsrc.co.in");
                        MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        MailIds.Add("kirankumar@dsrc.co.in");
                        MailIds.Add("dhanalakshmi.t@dsrc.co.in");
                        MailIds.Add("angayarkanni.s@dsrc.co.in");
                        //MailIds.Add("dineshkumar.d@dsrc.co.in");
                        // MailIds.Add("gopika.v@dsrc.co.in");

                        string EmailAddress = "";

                        foreach (string maiil in MailIds)
                        {
                            EmailAddress += maiil + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                        Task.Factory.StartNew(() =>
                        {

                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", AddEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", AddEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                        });

                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {

                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", AddEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));   
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", AddEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));

                        });
                    }
                }
                else
                {
                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    ExceptionHandlingController.TemplateMissing("AddEnroll", folder, ServerName);

                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult EditNewRoll(int QuickEnroll)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            QuickEnrollment EditRoll = new QuickEnrollment();
            var EditQuuickEnroll = new QuickEnrollment();

            try
            {


                var DepartmentList = (from us in db.Departments

                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).OrderBy(o => o.DepartmentName).ToList();
                var BranchList = db.Master_Branches.ToList();
                //var GroupList = db.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                //{
                //    GroupID = g.GroupID,
                //    Groupname = g.GroupName
                //}).OrderBy(o => o.Groupname).ToList();

               


                ViewBag.DepartmentId = EditRoll.DepartmentList;
                ViewBag.Branch = EditRoll.BranchID;


                var EditQuickEnroll = db.QuickEnrolls.Where(q => q.QuickEnroll1 == QuickEnroll).Select(r => r).FirstOrDefault();
                if (EditQuickEnroll.Experience != null)
                {
                    string exp = EditQuickEnroll.Experience.ToString();
                    string[] experience = exp.Split('.');
                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", experience[0]);
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", experience[1]);

                }
                else
                {
                    //string exp = EditQuickEnroll.Experience.ToString();

                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year");
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month");
                }

                if (EditQuickEnroll.DepartmentGroup != null)
                {

                    var GroupList = (from p in db.DepartmentGroups
                                     join q in db.DepartmentGroupMappings on p.GroupID equals q.GroupID
                                     where (q.DepartmentID == EditQuickEnroll.DepartmentID)
                                     select new
                                     {
                                         GroupID = p.GroupID,
                                         Groupname = p.GroupName
                                     }).OrderBy(o => o.Groupname).ToList();
                    ViewBag.GroupList = new SelectList(GroupList, "GroupID", "Groupname", EditQuickEnroll.DepartmentGroup);
                }
                else
                {
                    //List<DepartmentGroup> dept = new List<DepartmentGroup>();
                    //DepartmentGroup d = new DepartmentGroup();
                    //d.GroupID = 0;
                    //d.GroupName = "--Select--";
                    //dept.Add(d);
                    var GroupList = (from p in db.DepartmentGroups
                                     join q in db.DepartmentGroupMappings on p.GroupID equals q.GroupID
                                     where (q.DepartmentID == EditQuickEnroll.DepartmentID)
                                     select new
                                     {
                                         GroupID = p.GroupID,
                                         Groupname = p.GroupName
                                     }).OrderBy(o => o.Groupname).ToList();
                    ViewBag.GroupList = new SelectList(GroupList, "GroupID", "Groupname", 0);
                }

                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", EditQuickEnroll.BranchID);
                //ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", ViewBag.Branch);
                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", EditQuickEnroll.DepartmentID);
                var Date = db.QuickEnrolls.Where(o => o.QuickEnroll1 == QuickEnroll).Select(o => o.DateOfBirth).FirstOrDefault();
                if (Date != null)
                {


                    EditQuuickEnroll = (from Qe in db.QuickEnrolls
                                        where Qe.QuickEnroll1 == QuickEnroll
                                        //join v in db.Master_Roles on Qe.RoleID equals v.RoleID
                                        join v in db.Master_Designation on Qe.RoleID equals v.DesignationID
                                         into a
                                        from b in a.DefaultIfEmpty()
                                        select new QuickEnrollment
                                        {
                                            QuickEnroll = Qe.QuickEnroll1,
                                            FirstName = Qe.FirstName,
                                            LastName = Qe.LastName,
                                            PhoneNumber = Qe.PhoneNumber,
                                            Location = Qe.Location,
                                            Address = Qe.Address,
                                            DateOfBirth = ((DateTime)Qe.DateOfBirth),
                                            BranchID = Qe.BranchID,
                                            DateOfEnquiry = (DateTime)Qe.DateOfEnquiry,
                                            GenderID = Qe.GenderID,
                                            RollID = (byte)Qe.RoleID,
                                            //rollName=b.RoleName,
                                            rollName = b.DesignationName,
                                            // DateOfBirth = (string.IsNullOrEmpty(Qe.DateOfBirth)),
                                            DateOfJoin = (DateTime)Qe.DateOfJoin,
                                            Experience = Qe.Experience,
                                            PersonalEmailAddress = Qe.PersonalMailAddress,
                                            //DepartmentList = Qe.Department,
                                            //DepartmentGroup = Qe.DepartmentGroup,
                                            Comments = Qe.Comments
                                        }).FirstOrDefault();
                }
                else
                {

                    EditQuuickEnroll = (from Qe in db.QuickEnrolls
                                        where Qe.QuickEnroll1 == QuickEnroll
                                        //join v in db.Master_Roles on Qe.RoleID equals v.RoleID
                                        join v in db.Master_Designation on Qe.RoleID equals v.DesignationID
                                         into a
                                        from b in a.DefaultIfEmpty()
                                        select new QuickEnrollment
                                        {
                                            QuickEnroll = Qe.QuickEnroll1,
                                            FirstName = Qe.FirstName,
                                            LastName = Qe.LastName,
                                            PhoneNumber = Qe.PhoneNumber,
                                            Location = Qe.Location,
                                            Address = Qe.Address,
                                            BranchID = Qe.BranchID,
                                            DateOfEnquiry = (DateTime)Qe.DateOfEnquiry,
                                            GenderID = Qe.GenderID,
                                            RollID = (byte)Qe.RoleID,
                                            rollName = b.DesignationName,
                                            //rollName = b.RoleName,
                                            //DateOfBirth = ((DateTime)Qe.DateOfBirth),
                                            // DateOfBirth = (string.IsNullOrEmpty(Qe.DateOfBirth)),
                                            DateOfJoin = (DateTime)Qe.DateOfJoin,
                                            Experience = Qe.Experience,
                                            PersonalEmailAddress = Qe.PersonalMailAddress,
                                            //DepartmentList = Qe.Department,
                                            //DepartmentGroup = Qe.DepartmentGroup,
                                            Comments = Qe.Comments
                                        }).FirstOrDefault();
                }

                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();

                var UserType = (from u in db.Master_Designation
                                select new
                                {
                                    RoleID = u.DesignationID,
                                    RoleName = u.DesignationName
                                }).OrderBy(o => o.RoleName).ToList();


                ViewBag.Gender = new SelectList(GenderNameList, "GenderID ", "GenderName");
                ViewBag.usertype = new SelectList(UserType, "RoleID", "RoleName");
            }



            catch (Exception ex)
            {
                throw ex;
            }

            return View(EditQuuickEnroll);



        }

        [HttpPost]
        public ActionResult EditNewRoll(QuickEnrollment roll)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //var Department = db.Master_Department.ToList();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var DepartmentList = (from us in db.Departments

                                  select new
                                  {
                                      DepartmentId = us.DepartmentId,
                                      DepartmentName = us.DepartmentName
                                  }).OrderBy(o => o.DepartmentName).ToList();
            var GroupList = db.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
            {
                groupid = g.GroupID,
                groupname = g.GroupName
            }).OrderBy(o => o.groupname).ToList();



            var EditQuickEnroll = db.QuickEnrolls.Where(q => q.QuickEnroll1 == roll.QuickEnroll).Select(r => r).FirstOrDefault();

            EditQuickEnroll.FirstName = roll.FirstName;
            EditQuickEnroll.LastName = roll.LastName;
            EditQuickEnroll.Address = roll.Address;
            EditQuickEnroll.Location = roll.Location;
            EditQuickEnroll.PhoneNumber = roll.PhoneNumber;
            //EditQuickEnroll.DateOfJoin = Convert.ToDateTime(roll.DateOfJoin);
            EditQuickEnroll.DateOfJoin = roll.DateOfJoin;
            EditQuickEnroll.BranchID = roll.BranchName;
            EditQuickEnroll.GenderID = roll.GenderID;
            EditQuickEnroll.RoleID = (byte)roll.RollID;

            if (roll.DateOfBirth != DateTime.MinValue)
            {
                EditQuickEnroll.DateOfBirth = Convert.ToDateTime(roll.DateOfBirth);
            }
            // EditQuickEnroll.DateOfJoin = roll.DateOfJoin;
            // EditQuickEnroll.Experience = roll.Experience;
            EditQuickEnroll.PersonalMailAddress = roll.PersonalEmailAddress;

            EditQuickEnroll.DepartmentID = roll.DepartmentName;
            if (roll.DepartmentGroup != 0)
            {
                EditQuickEnroll.DepartmentGroup = roll.DepartmentGroup;
            }

            else
            {
                EditQuickEnroll.DepartmentGroup = null;
            
            }
            EditQuickEnroll.Comments = roll.Comments;
            EditQuickEnroll.Experience = roll.Experience;
           

            db.SaveChanges();

            ViewBag.GroupID = roll.DepartmentGroup;
            ViewBag.DepartmentId = roll.DepartmentList;
            ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", roll.Experience);
            ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", roll.ExperienceMonth);
            ViewBag.GroupList = new SelectList(GroupList, "ID", "GroupName", roll.DepartmentList);
            ViewBag.DepartmentList = new SelectList(DepartmentList, "ID", "DepartmentList", roll.DepartmentGroup);





            if (roll.QuickEnroll != null)
            {

                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "EditEnroll").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "EditEnroll").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "EditEnroll")
                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                      select new DSRCManagementSystem.Models.Email
                                      {
                                          To = p.To,
                                          CC = p.CC,
                                          BCC = p.BCC,
                                          Subject = p.Subject,
                                          Template = q.TemplatePath
                                      }).FirstOrDefault();
                    //var date = Convert.ToString(roll.DateOfJoin);
                    string Date1= null;
                    if (roll.DateOfJoin != null)
                    {
                        string Date = Convert.ToString(roll.DateOfJoin);
                        DateTime jdate = (Convert.ToDateTime(Date));
                        Date1 = jdate.ToString("dd-MM-yyyy");
                        
                    }

                    string editdate = DateTime.Now.ToString("dd-MM-yyyy");
                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                    string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                    string EditEnrollhtml = System.IO.File.ReadAllText(TemplatePathnewuser);

                    EditEnrollhtml = EditEnrollhtml.Replace("#Name", roll.FirstName + "  " + roll.LastName);
                    EditEnrollhtml = EditEnrollhtml.Replace("#JoiningDate", Date1);
                    EditEnrollhtml = EditEnrollhtml.Replace("#Experience", roll.Experience);
                    EditEnrollhtml = EditEnrollhtml.Replace("#Date", editdate);

                    EditEnrollhtml = EditEnrollhtml.Replace("#ServerName", ServerName);
                    EditEnrollhtml = EditEnrollhtml.Replace("#CompanyName", company);
                    if (objnewuser.To != "")
                    {
                        objnewuser.To = QuickEnrollController.GetUserEmailAddress(db, objnewuser.To);
                    }
                    if (objnewuser.CC != "")
                    {
                        objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.CC);
                    }
                    if (objnewuser.BCC != "")
                    {
                        // objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.BCC);
                    }

                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    var logo = CommonLogic.getLogoPath();
                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    int userId = int.Parse(Session["UserID"].ToString());
                    var userdetails = db.Users.FirstOrDefault(o => o.UserID == userId);
                    string MailID = userdetails.EmailAddress;

                    if (ServerName != "http://win2012srv:88/")
                    {
                        List<string> MailIds = new List<string>();

                        MailIds.Add("boobalan.k@dsrc.co.in");
                        //MailIds.Add("shaikhakeel@dsrc.co.in");
                        MailIds.Add("ramesh.S@dsrc.co.in");
                        MailIds.Add("aruna.m@dsrc.co.in");
                        MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        MailIds.Add("kirankumar@dsrc.co.in");
                        MailIds.Add("dhanalakshmi.t@dsrc.co.in");
                        // MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add("dineshkumar.d@dsrc.co.in");
                      
                        string EmailAddress = "";

                        foreach (string maiil in MailIds)
                        {
                            EmailAddress += maiil + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                        Task.Factory.StartNew(() =>
                        {

                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", EditEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", EditEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                        });

                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {

                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", EditEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", EditEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));
                        });
                    }
                }

                else
                {
                    //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    ExceptionHandlingController.TemplateMissing("EditEnroll", folder, ServerName);
                }
            }

            return Json("Success", JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int Quickenrollid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var DeleteQuickEnroll = db.QuickEnrolls.Where(o => o.QuickEnroll1 == Quickenrollid).Select(o => o).FirstOrDefault();
            DeleteQuickEnroll.IsActive = false;
            db.SaveChanges();




            if (Quickenrollid != null)
            {

                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "DeleteEnroll").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "DeleteEnroll").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "DeleteEnroll")
                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                      select new DSRCManagementSystem.Models.Email
                                      {
                                          To = p.To,
                                          CC = p.CC,
                                          BCC = p.BCC,
                                          Subject = p.Subject,
                                          Template = q.TemplatePath
                                      }).FirstOrDefault();
                    //var date = Convert.ToString(roll.DateOfJoin);

                    var Date1 = (from p in db.QuickEnrolls.Where(p => p.QuickEnroll1 == Quickenrollid)
                                 select (p.DateOfJoin)).FirstOrDefault();

                    var Name = (from p in db.QuickEnrolls.Where(p => p.QuickEnroll1 == Quickenrollid)
                                select (p.FirstName)).FirstOrDefault();

                    var Name1 = (from p in db.QuickEnrolls.Where(p => p.QuickEnroll1 == Quickenrollid)
                                 select (p.LastName)).FirstOrDefault();

                    var Exp = (from p in db.QuickEnrolls.Where(p => p.QuickEnroll1 == Quickenrollid)
                               select (p.Experience)).FirstOrDefault();

                    string Exp1 = Convert.ToString(Exp);
                    DateTime endate = (Convert.ToDateTime(Date1));
                    string Date = endate.ToString("dd-MM-yyyy");
                    string Deldate = DateTime.Now.ToString("dd-MM-yyyy");
                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                    string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                    string DeleteEnrollhtml = System.IO.File.ReadAllText(TemplatePathnewuser);

                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#Name", Name + "  " + Name1);
                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#JoiningDate", Date);
                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#Experience", Exp1);
                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#Date", Deldate);

                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#ServerName", ServerName);
                    DeleteEnrollhtml = DeleteEnrollhtml.Replace("#CompanyName", company);
                    if (objnewuser.To != "")
                    {
                        objnewuser.To = QuickEnrollController.GetUserEmailAddress(db, objnewuser.To);
                    }
                    if (objnewuser.CC != "")
                    {
                        objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.CC);
                    }
                    if (objnewuser.BCC != "")
                    {
                        // objnewuser.BCC = QuickEnrollController.GetUserEmailAddress(db, objnewuser.BCC);
                    }

                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    var logo = CommonLogic.getLogoPath();
                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    int userId = int.Parse(Session["UserID"].ToString());
                    var userdetails = db.Users.FirstOrDefault(o => o.UserID == userId);
                    string MailID = userdetails.EmailAddress;

                    if (ServerName != "http://win2012srv:88/")
                    {
                        List<string> MailIds = new List<string>();

                        MailIds.Add("boobalan.k@dsrc.co.in");
                        MailIds.Add("shaikhakeel@dsrc.co.in");
                        MailIds.Add("ramesh.S@dsrc.co.in");
                        MailIds.Add("aruna.m@dsrc.co.in");
                        MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        MailIds.Add("kirankumar@dsrc.co.in");
                        MailIds.Add("francispaul.k.c@dsrc.co.in");
                        MailIds.Add("dineshkumar.d@dsrc.co.in");
                        MailIds.Add("gopika.v@dsrc.co.in");

                        string EmailAddress = "";

                        foreach (string maiil in MailIds)
                        {
                            EmailAddress += maiil + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                        Task.Factory.StartNew(() =>
                        {
                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", DeleteEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", DeleteEnrollhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                        });

                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {

                            // DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", DeleteEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", DeleteEnrollhtml, "HRMS@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));
                        });
                    }
                }

                else
                {
                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    ExceptionHandlingController.TemplateMissing("DeleteEnroll", folder, ServerName);

                }
            }


            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public List<YearDropDown> YearsList()
        {
            List<YearDropDown> yearList = new List<YearDropDown>() { new YearDropDown() { Year = "", YearId = -1 } };
            foreach (int i in Enumerable.Range(0, 99))
            {
                yearList.Add(new YearDropDown() { Year = i.ToString(), YearId = i });
            }

            return yearList;
        }
        public List<MonthDropDown> MonthsList()
        {
            List<MonthDropDown> monthList = new List<MonthDropDown>() { new MonthDropDown() { Month = "", MonthId = -1 } };
            foreach (int i in Enumerable.Range(0, 12))
            {
                monthList.Add(new MonthDropDown() { Month = i.ToString(), MonthId = i });
            }

            return monthList;
        }
        //[HttpPost]
        // public ActionResult Converter(int QuickEnrollid)
        //     DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        //     return View();
        // }


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

                    validDepart = db.Departments.Where(d => d.BranchID == BranchId && d.IsActive == true).Select(d => d.DepartmentId).ToList();

                    FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                    select new FilterDepartment()
                                    {
                                        DepartmentId = lt.DepartmentId,
                                        DepartmentName = lt.DepartmentName
                                    }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
                }
                return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
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

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetGroups(int DepartmentId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();

            if (DepartmentId != 0)
            {
                var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == DepartmentId).Select(d => d.GroupID).ToList();

                FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                               where lt.IsActive == true
                               select new FilterGroup()
                               {
                                   GroupId = lt.GroupID,
                                   GroupName = lt.GroupName
                               }).AsEnumerable().OrderBy(o=>o.GroupName).Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });
            }
            return Json(new SelectList(FilterGroup, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


    }
}

