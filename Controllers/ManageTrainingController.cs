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
using System.Data.Entity;


namespace DSRCManagementSystem.Controllers
{
    //[DSRCAuthorize(Roles = " Project Manager")]
    public class ManageTrainingController : Controller
    {
        //
        // GET: /ManageTraining/
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult index()
        {
            return View();
        }

        public ActionResult MTRaining(string ID)

        {
            if (ID == null)
            {
                ID = "false";
            }


            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            List<ManageTrainingModel> result = null;


            int roleId = int.Parse(Session["RoleID"].ToString());
            int userId = int.Parse(Session["UserID"].ToString());
            DateTime myDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            

            var toggle = (from u in db.TrainingPermissions
                          where u.UserID == userId
                          select new
                          {
                              u.UserID
                          }).FirstOrDefault();

            if (toggle != null)
            {
                ViewBag.toggle = toggle;
            }
            else
            {
            }


            

            if (ID == "true")

           //if(ID=="true")
           {
               //Session["TRUEORFALSE"] = "false";
               ViewBag.value = "false";

            //if ((roleId == 1 || userId == 44 || userId == 232))
            //{
            

                result = (from rc in db.Trainings
                          join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                          join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                          join i in db.Users on rc.InstructorId equals i.UserID
                          join s in db.Master_TrainingStatus on rc.StatusId equals s.StatusId                          
                          where rc.IsActive == true
                          select new ManageTrainingModel
                          {
                              TrainingID = rc.TrainingId,
                              TrainingName = rc.TrainingName,
                              Level = l.LevelName,
                              Technology = t.TechnologyName,
                              SeatingCapacity = rc.SeatingCapacity,
                              ScheduledDate = rc.ScheduledDate,
                              StatusID = rc.StatusId,
                              Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                              Nominations = rc.NumberOfNominated,
                              Status = s.StatusName,
                              NoNominations = "-",

                              IsToday = (rc.ScheduledDate <= myDate) ? true : false,
                              color = (rc.ScheduledDate == myDate) ? true : false,
                              MailDepartments=rc.MailDepartments,
                              NotifyUsers=rc.NotifyUsers
                          
                         }).OrderByDescending(o => o.ScheduledDate).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Year).ToList();
            }
            else
            {
                //Session["TRUEORFALSE"] = "true";
                ViewBag.value = "true";
                result = (from rc in db.Trainings
                          join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                          join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                          join i in db.Users on rc.InstructorId equals i.UserID
                          join s in db.Master_TrainingStatus on rc.StatusId equals s.StatusId
                          where rc.IsActive == true && rc.InstructorId == userId                         
                          select new ManageTrainingModel
                          {
                              TrainingID = rc.TrainingId,
                              TrainingName = rc.TrainingName,
                              Level = l.LevelName,
                              Technology = t.TechnologyName,
                              SeatingCapacity = rc.SeatingCapacity,
                              ScheduledDate = rc.ScheduledDate,
                              StatusID = rc.StatusId,
                              Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                              Nominations = rc.NumberOfNominated,
                              Status = s.StatusName,
                              NoNominations = "-",
                              IsToday = (rc.ScheduledDate <= myDate) ? true : false,
                              color = (rc.ScheduledDate == myDate) ? true : false,
                              MailDepartments = rc.MailDepartments,
                              NotifyUsers = rc.NotifyUsers
                          }).OrderByDescending(o => o.ScheduledDate).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Year).ToList();
            }
            return View(result);
        }


        public ActionResult Edit(int TrainingID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DateTime myDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            
            var obj = (from rc in db.Trainings
                       where rc.IsActive == true && rc.TrainingId == TrainingID
                       join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                       join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                       join s in db.Master_TrainingStatus on rc.StatusId equals s.StatusId

                       join i in db.Users on rc.InstructorId equals i.UserID

                       select new ManageTrainingModel()
                       {

                           TrainingID = rc.TrainingId,
                           SeatingCapacity = rc.SeatingCapacity,
                           ScheduledDate = rc.ScheduledDate,
                           Starttime = rc.StartTime,
                           TrainingName = rc.TrainingName,
                           Technology = t.TechnologyName,
                           Level = l.LevelName,
                           Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                           Nominations = rc.NumberOfNominated,
                           StatusID = rc.StatusId,
                           Status = s.StatusName,
                           Endtime = rc.EndTime,
                           IsToday = (rc.ScheduledDate <= myDate) ? true : false ,
                           MailDepartments = rc.MailDepartments,
                           NotifyUsers = rc.NotifyUsers
                           
                       }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).FirstOrDefault();
           
            return View(obj);

        }
        [HttpPost]

        public ActionResult Edit(ManageTrainingModel model)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var obj_Comp = db.Trainings.FirstOrDefault(o => o.TrainingId == model.TrainingID);

                if (obj_Comp.ScheduledDate != model.ScheduledDate || obj_Comp.StartTime != model.Starttime || obj_Comp.EndTime != model.Endtime || obj_Comp.SeatingCapacity != model.SeatingCapacity)
                {
                    var Message = "";

                    LDCourse1Controller obj = new LDCourse1Controller();
                    int ID = Convert.ToInt32(model.TrainingID);
                    string dt = model.ScheduledDate.ToString();
                    string time = Convert.ToString(model.Starttime);
                    string endtime = Convert.ToString(model.Endtime);
                    var message = obj.scheduletime(ID, dt, model.TrainingName, time, endtime);                    
                    
                    Message=Convert.ToString(message.Data);

                    if (Message == "availabletime")
                    {
                        return Json(Message, JsonRequestBehavior.AllowGet);
                    }

                    obj_Comp.SeatingCapacity = model.SeatingCapacity;
                    obj_Comp.ScheduledDate = model.ScheduledDate;
                    obj_Comp.StartTime = model.Starttime;
                    obj_Comp.EndTime = model.Endtime;
                    obj_Comp.StatusId = 7;

                    db.SaveChanges();

                    string Tid = obj_Comp.TrainingId.ToString();
                    string tname = obj_Comp.TrainingName;
                    DateTime d1 = Convert.ToDateTime(model.ScheduledDate);
                    string d = d1.ToShortDateString();
                    string stime = obj_Comp.StartTime;
                    string etime = obj_Comp.EndTime;
                    var userref = db.Users.FirstOrDefault(o => o.UserID == obj_Comp.InstructorId);
                    string username = userref.FirstName + " " + (userref.LastName ?? "");

                    List<string> MailID = new List<string>();
                    MailID.Add(userref.EmailAddress);


                    var EmpIDList = db.TrainingNominations.Where(o => o.TrainingId == model.TrainingID).Select(o => o.EmpId).ToList();

                    //foreach (int Eid in EmpIDList)
                    foreach (string Eid in EmpIDList)
                    {
                        model.EmpID = Eid.ToString();

                        if (model.EmpID.Length == 4)
                            model.EmpID = "0" + model.EmpID;

                        model.MailAddress = db.Users.FirstOrDefault(o => o.EmpID == model.EmpID).EmailAddress ?? "";
                        MailID.Add(model.MailAddress);
                    }

                    //string mailMessage = MailBuilder.TrainingScheduleChangeNotification(Tid, tname, d, stime, etime, username);

                     var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Training Schedule Change Notification").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Training Schedule Change Notification").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objTrainingScheduleChangeNotification = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Training Schedule Change Notification")
                                                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                      select new DSRCManagementSystem.Models.Email
                                                                      {
                                                                          To = p.To,
                                                                          CC = p.CC,
                                                                          BCC = p.BCC,
                                                                          Subject = p.Subject,
                                                                          Template = q.TemplatePath
                                                                      }).FirstOrDefault();
                         var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathTrainingScheduleChangeNotification = Server.MapPath(objTrainingScheduleChangeNotification.Template);
                         string htmlTrainingScheduleChangeNotification = System.IO.File.ReadAllText(TemplatePathTrainingScheduleChangeNotification);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#TrainingId", Tid);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#TrainingName", tname);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#ScheduledDate", d);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#start", stime);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#end", etime);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#Instructor", username);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#CompanyName", objcom);
                         htmlTrainingScheduleChangeNotification = htmlTrainingScheduleChangeNotification.Replace("#ServerName", ServerName);

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

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                             //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                             // DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleChangeNotification.Subject + " - Test Mail Please Ignore", null, htmlTrainingScheduleChangeNotification + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                             DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleChangeNotification.Subject + " - Test Mail Please Ignore", null, htmlTrainingScheduleChangeNotification + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));

                         }
                         else
                         {
                             foreach (string mail in MailID)
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleChangeNotification.Subject, null, htmlTrainingScheduleChangeNotification, "TRAINING@dsrc.co.in", mail, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleChangeNotification.Subject, null, htmlTrainingScheduleChangeNotification, "TRAINING@dsrc.co.in", mail, Server.MapPath(logo.ToString()));
                             }
                         }
                     }
                     else
                     {
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Training Schedule Change Notification", folder, ServerName);
                     }
                }
                return Json("Success", JsonRequestBehavior.AllowGet);               
            }
        }


        // [HttpGet]
        //public ActionResult TrainingPermission()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        //    int userIds = Convert.ToInt32(Session["UserID"]);
        //    int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userIds).BranchId;
        //    var UnAssignedEmployees = (from u in db.Users.Where(o => o.IsActive != false)
        //                               where u.BranchId == BranchId && u.UserStatus != 6 //By Default select only DSRC
        //                               select new DSRCManagementSystem.Models.TrainingsModel()
        //                               {
        //                                   UserID = u.UserID,
        //                                   UserName = u.FirstName + " " + u.LastName
        //                               }).ToList();

        //    ViewBag.UnAssignedEmployees = new MultiSelectList(UnAssignedEmployees, "UserID", "UserName");




        //    int roleId = int.Parse(Session["RoleID"].ToString());
        //    int userId = int.Parse(Session["UserID"].ToString());
            

        //    var AssignedEmployees = (from u in db.TrainingPermissions
        //                             join p in db.Users on u.UserID equals p.UserID
        //                             where p.IsActive == true
        //                             select new
        //                             {
        //                                 UserID1=p.UserID,
        //                                 UserName1 = p.FirstName + " " + p.LastName
        //                             }).Distinct().ToList();

        //    ViewBag.AssignedEmployees = new MultiSelectList(AssignedEmployees, "UserID1", "UserName1");


        //    return View();
        //}
        // [HttpPost]
        // public ActionResult TrainingPermission(TrainingsModel obj)
        // {
        //     DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            
        //     int userIds = Convert.ToInt32(Session["UserID"]);
        //     int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userIds).BranchId;
        //     var UnAssignedEmployees = (from u in db.Users.Where(o => o.IsActive != false)
        //                                where u.BranchId == BranchId && u.UserStatus != 6 //By Default select only DSRC
        //                                select new DSRCManagementSystem.Models.TrainingsModel()
        //                                {
        //                                    UserID = u.UserID,
        //                                    UserName = u.FirstName + " " + u.LastName
        //                                }).ToList();

        //     ViewBag.UnAssignedEmployees = new MultiSelectList(UnAssignedEmployees, "UserID", "UserName");


        //     int roleId = int.Parse(Session["RoleID"].ToString());
        //     int userId = int.Parse(Session["UserID"].ToString());

             

        //     var AssignedEmployees = (from u in db.TrainingPermissions
        //                              join p in db.Users on u.UserID equals p.UserID
        //                              where p.IsActive == true
        //                              select new TrainingsModel()
        //                              {
        //                                  UserID1 = p.UserID,
        //                                  UserName1 = p.FirstName + " " + p.LastName
        //                              }).Distinct().ToList();



        //     ViewBag.AssignedEmployees = new MultiSelectList(AssignedEmployees, "UserID1", "UserName1");

           
        //     int permission=1;


        //     var user = (from rc in db.Users
        //                       join p in db.UserRoles on rc.UserID equals p.UserID
        //                       where (rc.FirstName != null && rc.LastName != null)
        //                       select new AssignRole()
        //                       {
        //                           userid = rc.UserID,
        //                       }).Distinct().ToList();



        //     var UserIDs = obj.UserName.Split(',');

             
             

             


        //     for (int j = 0; j < obj.count.Count(); j++)
        //     {
        //         int check = obj.count[j];

                
        //             TrainingPermission permissions = new TrainingPermission();
        //             permissions.PermissionID = permission;
        //             permissions.UserID = check;
        //             permissions.IsActive = true;
        //             permissions.TrainingID = 1;
        //             db.TrainingPermissions.AddObject(permissions);
        //             db.SaveChanges();
                    
        //     }
             

        //     //return View();
        //     return Json("Success", JsonRequestBehavior.AllowGet);

        // }






        [HttpGet]
        public ActionResult TrainingPermission()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var check = db.TrainingPermissions.Select(x => x.UserID).ToList();
                List<int> LIST = new List<int>();
                foreach (var X in check)
                {
                    var list =
                        db.Users.Where(x => x.UserID == X && x.IsActive == true && x.UserStatus != 6)
                            .Select(o => o.UserID)
                            .FirstOrDefault();


                    if (list == 0)
                    {
                        var data = db.TrainingPermissions.Where(o => o.UserID == X).Select(x => x).ToList();
                        foreach (var y in data)
                        {
                            y.IsActive = false;
                            db.SaveChanges();

                        }
                        //LIST.Add(list);
                    }
                }


                var AuthUsers = (from ep in db.TrainingPermissions.Where(ep => ep.IsActive == true)
                                 join u in db.Users.Where(u => u.IsActive == true && u.UserStatus != 6) on ep.UserID equals u.UserID
                                     into evper
                                 from eventper in evper.DefaultIfEmpty()
                                 select new
                                 {
                                     userid = eventper.UserID,
                                     username = eventper.FirstName + " " + eventper.LastName
                                 }).ToList();

                ViewBag.AuthorizedUsers = new MultiSelectList(AuthUsers, "userid", "username");


                var FilteredUsers =
                        db.Users.Where(
                            u => u.IsActive == true && u.UserStatus != 6)
                            .Select(x => x.UserID)
                            .ToList()
                            .
                            Except(
                                db.TrainingPermissions.Where(ep => ep.IsActive == true || ep.IsActive.Value)
                                    .Select(x => x.UserID.Value)
                                    .ToList()).ToList();
                List<object> UnAuthUsers = new List<object>();
                foreach (int users in FilteredUsers)
                {
                    UnAuthUsers.AddRange(
                        db.Users.Where(u => u.UserID == users)
                            .Select(u => new { userid = u.UserID, username = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "") })
                            .ToList());
                }
                ViewBag.UnAuthorizedUsers = new MultiSelectList(UnAuthUsers, "userid", "username");
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
        public ActionResult TrainingPermission(List<int> From, List<int> To)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var deleteuser = db.TrainingPermissions.Where(x => x.IsActive == true).Select(o => o).ToList();
                foreach (var deluser in deleteuser)
                    db.TrainingPermissions.DeleteObject(deluser);
                db.SaveChanges();
                for (int j = 0; j < To.Count(); j++)
                {
                    DSRCManagementSystem.TrainingPermission objaccess = new DSRCManagementSystem.TrainingPermission();
                    objaccess.UserID = To[j];
                    objaccess.IsActive = true;
                    objaccess.PermissionID = 1;
                    db.AddToTrainingPermissions(objaccess);
                    db.SaveChanges();
                }
                return Json("Authorize", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }

        }
        
    
        [HttpGet]
        public ActionResult CalculateAggregate(int TrainingID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();            

            int? Id = TrainingID;
            db.Sp_FeedbackCalculation(Id);


            var AggreList = (from fa in db.TrainingFeedbackAggregates
                             join tfc in db.TrainingFeedBackCalcs on fa.TrainingId equals tfc.TrainingId
                             where fa.TrainingId == TrainingID
                             select new FeedbackAggregareModel
                             {
                                 ContentRating = fa.ContentRating,
                                 PresentRating = fa.PresentRating,
                                 FacultyRating = fa.FacultyQRating,
                                 OverallRating = fa.OverAllRating,
                                 learntinprog = tfc.LearntInPgm

                             }).FirstOrDefault();

            //AggreList.Comments = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == TrainingID).Select(o => o.Comments).ToList();
            AggreList.Comments = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == TrainingID && x.Comments != "Not given yet ").Select(o => o.Comments).ToList();
            AggreList.Learn = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == TrainingID && x.LearntInPgm != "Not given yet ").Select(o => o.LearntInPgm).ToList();

            return View(AggreList);
        }

        [HttpPost]
        public ActionResult Delete(int TrainingID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            ManageTrainingModel model = new ManageTrainingModel();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var obj = db.Trainings.Where(x => x.TrainingId == TrainingID).FirstOrDefault();
            obj.IsActive = false;
            obj.StatusId = 5;            

            var val = db.TrainingNominations.Where(x => x.TrainingId == TrainingID).Select(o => o).ToList();

            foreach (var item in val)
            {
                item.IsActive = false;
                db.SaveChanges();
            }

            db.SaveChanges();

            var MailPurposeObj = db.TrainingMailPurposes.Where(x => x.TrainingId == TrainingID).FirstOrDefault();
            MailPurposeObj.IsActive = false;
            db.SaveChanges();

            string Tid = obj.TrainingId.ToString();
            string tname = obj.TrainingName;
            DateTime d1 = Convert.ToDateTime(obj.ScheduledDate);
            string d = d1.ToShortDateString();
            string stime = obj.StartTime;
            string etime = obj.EndTime;
            var userref = db.Users.FirstOrDefault(o => o.UserID == obj.InstructorId);
            string username = userref.FirstName + userref.LastName;

            List<string> MailID = new List<string>();
            MailID.Add(userref.EmailAddress);

            var EmpIDList = db.TrainingNominations.Where(o => o.TrainingId == obj.TrainingId).Select(o => o.EmpId).ToList();

            foreach (string Eid in EmpIDList)
            {
                model.EmpID = Eid.ToString();
                model.EmpID = "0" + model.EmpID;
                model.MailAddress = db.Users.FirstOrDefault(o => o.EmpID == model.EmpID).EmailAddress;
                MailID.Add(model.MailAddress);
            }

           // string mailMessage = MailBuilder.TrainingScheduleCancelled(Tid, tname, d, stime, etime, username);

             var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Training Schedule Cancelled").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Training Schedule Cancelled").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var objTrainingScheduleCancelled = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Training Schedule Cancelled")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();
                         var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string TemplatePathTrainingScheduleCancelled = Server.MapPath(objTrainingScheduleCancelled.Template);
                         string htmlTrainingScheduleCancelled = System.IO.File.ReadAllText(TemplatePathTrainingScheduleCancelled);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#TrainingId", Tid);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#TrainingName", tname);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#ScheduledDate", d);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#start", stime);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#end", etime);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#Instructor", username);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#ServerName", ServerName);
                         htmlTrainingScheduleCancelled = htmlTrainingScheduleCancelled.Replace("#ComapanyName", objcom);
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

                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }

                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                             //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                             // DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleCancelled.Subject + " - Test Mail Please Ignore", null, htmlTrainingScheduleCancelled + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));

                             DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleCancelled.Subject + " - Test Mail Please Ignore", null, htmlTrainingScheduleCancelled + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));


                         }
                         else
                         {


                             foreach (string mail in MailID)
                             {
                                 //Task.Factory.StartNew(() =>
                                 //{
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 // DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleCancelled.Subject, null, htmlTrainingScheduleCancelled, "TRAINING@dsrc.co.in", mail, Server.MapPath(logo.AppValue.ToString()));
                                 DsrcMailSystem.MailSender.SendMail(null, objTrainingScheduleCancelled.Subject, null, htmlTrainingScheduleCancelled, "TRAINING@dsrc.co.in", mail, Server.MapPath(logo.ToString()));
                                 //DsrcMailSystem.MailSender.LDSendMail(null, "L & D - Training Schedule Cancelled", null, mailMessage, "TRAINING@dsrc.co.in", mail, Server.MapPath("~/Content/Template/images/logo.png"));

                                 //  });
                             }
                         }
                     }
                     else
                     {
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Training Schedule Cancelled", folder, ServerName);
                     }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult MailDepartments(string TrainingID)
        {
            int trainingid =Convert.ToInt32(TrainingID);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();


          

            var departments =(from p in  objdb.Departments.Where(x => x.IsActive== true)
                              select new 
                              {
                                  DepartmentId=p.DepartmentId,
                                  DepartmentName=p.DepartmentName

                              }).ToList();


            var NotifyUsers = (from p in objdb.Users.Where(x => x.IsActive == true)
                               select new
                               {
                                   Userid = p.UserID,
                                   UserName = p.FirstName + " " + p.LastName

                               }).ToList();

            List<int> objvalue = new List<int>();
            List<int>  objnotify = new List<int>();


           

            string values = objdb.Trainings.Where(x => x.TrainingId == trainingid).Select(o => o.MailDepartments).FirstOrDefault();

            string notify = objdb.Trainings.Where(x => x.TrainingId == trainingid).Select(o => o.NotifyUsers).FirstOrDefault();
       

           if(values!=null)
            {
                string[] tokens = values.Split(new string[] { "," }, StringSplitOptions.None);
                foreach (var k in tokens)
                {
                    int val;
                    int.TryParse(k, out val);
                    objvalue.Add(val);
                }
               
            }


           if (notify != null)
           {
               string[] tokens = notify.Split(new string[] { "," }, StringSplitOptions.None);
               foreach (var k in tokens)
               {
                   int val;
                   int.TryParse(k, out val);
                  objnotify.Add(val);
               }

           }


              //List<int> selectedEmail = new List<int>();
              //  if (dbvalue.Count()!= 0)
              //  {

              //      string[] tokens = dbvalue.Split(new string[] { "," }, StringSplitOptions.None);
              //      foreach (var i in tokens)
              //      {
              //          int val;
              //          int.TryParse(i, out val);
              //          selectedEmail.Add(val);
              //      }
              //  }

            ViewBag.Leaders = new MultiSelectList(departments, "DepartmentId", "DepartmentName", objvalue);

            ViewBag.users = new MultiSelectList(NotifyUsers, "Userid", "UserName",objnotify);
            return View();
        }



        [HttpGet]
        public ActionResult GetEmployee(int TrainingID)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();           

            GetEmployeeModel obj = new GetEmployeeModel();

            bool IsTimeLimit = db.TrainingNominations.Where(o => o.TrainingId == TrainingID).Select(o => o.CompletionFlag).Any(o => o == true);            

            if (IsTimeLimit)
            {
                obj.AttendeesResult = (from p in db.Trainings.Where(x => x.TrainingId == TrainingID)
                                       join t in db.TrainingNominations on p.TrainingId equals t.TrainingId
                                       join u in db.Users on t.UserId equals u.UserID
                                       join d in db.Departments on u.DepartmentId equals d.DepartmentId                                       
                                       where t.UserId == u.UserID && t.NominationFlag == true && t.IsActive == true //&& t.Score > 0
                                       select new DSRCManagementSystem.Models.GetEmployeesModel
                                       {
                                           EmployeeId = t.EmpId,
                                           EmployeeName = t.EmpName,
                                           Department = d.DepartmentName,
                                           score = t.Score,
                                           //status = p.StatusId,
                                           status = 0,
                                           FeedbackStatus = "-"
                                       }).ToList();

                obj.NonAttendeesResult = (from p in db.Trainings.Where(x => x.TrainingId == TrainingID)
                                          join t in db.TrainingNominations on p.TrainingId equals t.TrainingId
                                          join u in db.Users on t.UserId equals u.UserID
                                          join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                          where t.Score > 0
                                          //join f in db.TrainingFeedBackCalcs on p.TrainingId equals f.TrainingId
                                          //where f.UserId == u.UserID
                                          select new DSRCManagementSystem.Models.GetEmployeesModel
                                          {
                                              EmployeeId = t.EmpId,
                                              EmployeeName = t.EmpName,
                                              Department = d.DepartmentName,
                                              score = 0,
                                              status = 0,
                                              FeedbackStatus = "-"
                                          }).ToList();
            }
            else
            {


                obj.AttendeesResult = (from p in db.Trainings.Where(x => x.TrainingId == TrainingID)
                                       join t in db.TrainingNominations on p.TrainingId equals t.TrainingId
                                       join u in db.Users on t.UserId equals u.UserID
                                       join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                       join f in db.TrainingFeedBackCalcs on p.TrainingId equals f.TrainingId
                                       where f.UserId == u.UserID && t.NominationFlag == true && t.IsActive == true && t.Score > 0
                                       // join v in db.TrainingFeedBackCalcs on u.UserID equals v.UserId
                                       select new DSRCManagementSystem.Models.GetEmployeesModel
                                       {
                                           EmployeeId = t.EmpId,
                                           EmployeeName = t.EmpName,
                                           Department = d.DepartmentName,
                                           score = t.Score,
                                           status = p.StatusId,

                                           FeedbackStatus = f.Flag == true ? "Submitted" : "Pending"
                                       }).ToList();




                obj.NonAttendeesResult = (from p in db.Trainings.Where(x => x.TrainingId == TrainingID)
                                          join t in db.TrainingNominations on p.TrainingId equals t.TrainingId
                                          join u in db.Users on t.UserId equals u.UserID
                                          join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                          where t.Score <= 0 && t.NominationFlag == true && t.IsActive == true
                                          //join f in db.TrainingFeedBackCalcs on p.TrainingId equals f.TrainingId
                                          //where f.UserId == u.UserID
                                          select new DSRCManagementSystem.Models.GetEmployeesModel
                                          {
                                              EmployeeId = t.EmpId,
                                              EmployeeName = t.EmpName,
                                              Department = d.DepartmentName,
                                              score = 0,
                                              status = 0,
                                              FeedbackStatus = "-"
                                          }).ToList();
            }

                return View(obj);
        }
    }
}