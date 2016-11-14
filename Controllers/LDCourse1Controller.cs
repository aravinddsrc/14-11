using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Globalization;
using System.Text;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using System.Data.SqlClient;
using NPOI.SS.Util;
using System.Data.OleDb;
using System.Data.Common;



namespace DSRCManagementSystem.Controllers
{
    public class LDCourse1Controller : Controller
    {
        [HttpGet]
        public ActionResult AddCoursedetails()
        {
            LDCourse1List ld = new LDCourse1List();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                LDCourse1 obj = new LDCourse1();

                var LevelList = db.Master_TrainingLevel.ToList();
                var TechList = db.Master_TrainingTechnology.ToList();

                string[] l = new string[LevelList.Count];
                for (int j = 0; j < LevelList.Count; j++)
                {
                    l[j] = LevelList[j].LevelName;
                }
                string result1 = string.Join("\n", l);
                TempData["res1"] = result1;
                string[] t = new string[TechList.Count];

                for (int k = 0; k < TechList.Count; k++)
                {
                    t[k] = TechList[k].TechnologyName;
                }

                string result2 = string.Join("\n", t);

                TempData["res2"] = result2;
                // string[] s = new string[InstructorList.Count];

                //for (int i = 0; i < InstructorList.Count; i++)
                //{

                //    s[i] = InstructorList[i].FirstName + ' ' + InstructorList[i].LastName;

                //}
                //string result = string.Join("\n", s);

                //  TempData["res"] = result;

                //string Filepath = Server.MapPath("~/CourseTemplate/Course1.xls");
                // CellDataWriter1(1, 1, Filepath, "sheet1");

                DSRCManagementSystem.Models.LDCourse1 ObjLD = new DSRCManagementSystem.Models.LDCourse1();
                ModelState.Clear();
                List<LDCourse1> ldm = new List<LDCourse1>();

                LDCourse1 lModel = new LDCourse1();
                List<SelectListItem> LevelList1 = new List<SelectListItem>();
                List<SelectListItem> TechList1 = new List<SelectListItem>();
                IList<SelectListItem> usr = new List<SelectListItem>();
                foreach (var list in LevelList)
                {
                    LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
                }
                lModel.LevelIDList = LevelList1;
                foreach (var list in TechList)
                {
                    TechList1.Add(new SelectListItem { Text = list.TechnologyName, Value = Convert.ToString(list.TechnologyId) });
                }
                lModel.TechIDList = TechList1;
                var id = db.UserReportings.ToList();

                var query = db.Users.OrderBy(a => a.FirstName).Where(x => x.IsActive == true).ToList();

                int userId = Convert.ToInt32(Session["UserID"]);

                // int? reportingid= db.UserReportings.Where(x => x.ReportingUserID ==userId).Select(o => o.ReportingUserID).FirstOrDefault();
                var reportingid = db.UserReportings.Where(x => x.ReportingUserID == userId).Select(o => o).ToList();
                string userName = "";

                if (reportingid.Count > 0)
                {
                    userName = db.Users.Where(o => o.UserID == userId && o.IsActive == true).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                    usr.Add(new SelectListItem { Value = Convert.ToString(userId), Text = userName });

                    foreach (var user in reportingid)
                    {
                        if (user.UserID != userId)
                        {
                            userName = db.Users.Where(o => o.UserID == user.UserID && o.IsActive == true).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();


                            if (userName != null)
                                usr.Add(new SelectListItem { Value = Convert.ToString(user.UserID), Text = userName });
                        }
                        //usr.Add(new SelectListItem { Value = Convert.ToString(a.UserID), Text = a.FirstName + " " + a.LastName });
                    }
                }
                else
                {
                    var USER = db.Users.Where(x => x.UserID == userId && x.IsActive == true).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                    usr.Add(new SelectListItem { Value = Convert.ToString(userId), Text = USER });
                    //foreach (var a in query)
                    //{
                    //    usr.Add(new SelectListItem { Value = Convert.ToString(userId), Text = a.FirstName + " " + a.LastName });
                    //}   
                }

                lModel.InstructorIDList = usr.OrderBy(o => o.Text).ToList();

                // ViewBag.InstructorIDList = new SelectList(InstructorList, "UserID", "FirstName");


                var departments = (from p in db.Departments.Where(x => x.IsActive == true)
                                   select new
                                   {
                                       DepartmentId = p.DepartmentId,
                                       DepartmentName = p.DepartmentName

                                   }).ToList();

                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

                var Users = (from p in db.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                             select new
                             {
                                 userid = p.UserID,
                                 username = p.FirstName + "" + p.LastName
                             }).ToList();


                ViewBag.Users = new SelectList("", "userid", "username");



                for (int i = 0; i < 1; i++)
                {
                    LDCourse1 lm = new LDCourse1();
                    lm.Coursename = "";
                    lm.LevelId = 0;
                    lm.TechnologyId = 0;
                    lm.userid = 0;
                    lm.Scheduledate = null;
                    lm.Starttime = null;
                    lm.Endtime = null;
                    lm.SeatingCapacity = null;
                    //lm.MailDepartments=null;
                    //lm.NotifyUsers = null;
                    ldm.Add(lm);
                }

                ld.ldmlist = ldm;
                ld.LDM = lModel;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View("AddCoursedetails", ld);
        }


        [HttpPost]
        public ActionResult AddCoursedetails(LDCourse1List model, FormCollection formData)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            LDCourse1List ld = new LDCourse1List();


                 //string tname="", stim="", etim="", instr="";
                 //DateTime sdate = DateTime.Now;

            try
            {

                List<string> ListMailDepartments = new List<string>();
                string MailDepartments = "";
                string NotifyUsers = "";
                if (model.MailDepartments != null)
                {
                    foreach (var MailDepartment in model.MailDepartments)
                    {
                        ListMailDepartments.Add(MailDepartment);
                    }
                    MailDepartments = string.Join(",", ListMailDepartments.ToArray());
                }
                List<string> MailNotifyUsers = new List<string>();
                if (model.NotifyUsers != null)
                {
                    foreach (var NotifyUser in model.NotifyUsers)
                    {
                        MailNotifyUsers.Add(NotifyUser);
                    }
                    NotifyUsers = string.Join(",", MailNotifyUsers.ToArray());
                }


                string UserId = Session["UserID"].ToString();
                for (int i = 0; i < model.ldmlist.Count; i++)
                {
                    var LDobj = db.Trainings.CreateObject();
                    var LDMPobj = db.TrainingMailPurposes.CreateObject();
                    var TNobj = db.TrainingNominations.CreateObject();
                    if (model.ldmlist[i].TechnologyId == 0 || model.ldmlist[i].LevelId == 0 || model.ldmlist[i].Scheduledate == "" || model.ldmlist[i].Coursename == "" || model.ldmlist[i].Starttime == "" || model.ldmlist[i].Endtime == "" || model.ldmlist[i].SeatingCapacity == null || model.ldmlist[i].userid == 0)
                    {
                        //return Json(new { Result = "Validation", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                        TempData["validation"] = "Please provide values for all mandatory fields.";
                    }
                    else
                    {
                        if (model.ldmlist[i].TechnologyId != 0)
                        {



                            //tname=model.ldmlist[i].Coursename.Trim();
                            //sdate = Convert.ToDateTime(model.ldmlist[i].Scheduledate);
                            //stim=model.ldmlist[i].Starttime;
                            //etim=model.ldmlist[i].Endtime;
                            //instr = db.Users.Where(x => x.UserID == uid).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();


                            LDobj.MailDepartments = MailDepartments;
                            LDobj.NotifyUsers = NotifyUsers;
                            LDobj.TechnologyId = model.ldmlist[i].TechnologyId;
                            LDobj.LevelId = model.ldmlist[i].LevelId;
                            LDobj.ScheduledDate = Convert.ToDateTime(model.ldmlist[i].Scheduledate);
                            LDobj.TrainingName = model.ldmlist[i].Coursename.Trim();
                            LDobj.IsActive = true;
                            LDobj.TrainingTypeId = 2;
                            LDobj.InitiatedOn = DateTime.Now;
                            LDobj.NumberOfNominated = 0;
                            LDobj.InitiatedBy = UserId;
                            LDobj.StartTime = model.ldmlist[i].Starttime;
                            LDobj.EndTime = model.ldmlist[i].Endtime;
                            LDobj.SeatingCapacity = model.ldmlist[i].SeatingCapacity;

                            if (LDobj.ScheduledDate.Value.Date == DateTime.Now.Date)
                            {
                                LDobj.StatusId = 1;

                            }
                            else if (LDobj.ScheduledDate.Value.Date > DateTime.Now.Date)
                            {
                                LDobj.StatusId = 10;

                            }
                            else if (LDobj.ScheduledDate.Value.Date < DateTime.Now.Date && TNobj.Score > 0)
                            {
                                LDobj.StatusId = 3;

                            }

                            else if (LDobj.ScheduledDate.Value.Date < DateTime.Now.Date && TNobj.Score == 0)
                            {
                                LDobj.StatusId = 2;
                            }
                            LDobj.InstructorId = model.ldmlist[i].userid;




                            db.Trainings.AddObject(LDobj);
                            db.SaveChanges();
                        }



                            //tname=model.ldmlist[i].Coursename.Trim();
                            //sdate = Convert.ToDateTime(model.ldmlist[i].Scheduledate);
                            //stim=model.ldmlist[i].Starttime;
                            //etim=model.ldmlist[i].Endtime;
                            //instr = db.Users.Where(x => x.UserID == uid).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();


                        LDMPobj.TechnologyId = model.ldmlist[i].TechnologyId;
                        LDMPobj.ScheduledDate = Convert.ToDateTime(model.ldmlist[i].Scheduledate);
                        LDMPobj.TrainingName = model.ldmlist[i].Coursename;
                        LDMPobj.IsActive = true;
                        LDMPobj.InitiatedOn = DateTime.Now;
                        LDMPobj.StartTime = model.ldmlist[i].Starttime;
                        LDMPobj.EndTime = model.ldmlist[i].Endtime;
                        LDMPobj.InstructorId = model.ldmlist[i].userid;
                        LDMPobj.GroupEmailFlag = false;
                        db.TrainingMailPurposes.AddObject(LDMPobj);
                        db.SaveChanges();

                        TempData["Success"] = "Training details Saved Successfully";



                    }

                }

                /*      ////////////////
                    //  var GetUsersFromDept = new List<int>();
                      List<string> ListMailDepartments1 = new List<string>();

                      foreach (var MailDepartment1 in model.MailDepartments)
                      {
                          var flag=0;
                          if (MailDepartment1 != null)
                          {
                              flag = Convert.ToInt32(MailDepartment1);
                          }
                              var user = db.Users.Where(x => x.DepartmentId == flag && x.IsActive == true).Select(o => o.UserID).ToList();

                          foreach (var GetUsersFromDept in user)
                          {
                              DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
                              string ServerName = AppValue.GetFromMailAddress("ServerName");
                              int userId = int.Parse(Session["UserID"].ToString());
                              var createdby = db.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                              var FromEmailID = db.Users.Where(x => x.UserID == userId).Select(o => o.UserName).FirstOrDefault();
                              var Getmailid = db.Users.Where(x => x.UserID == GetUsersFromDept).Select(o => o.EmailAddress).FirstOrDefault();
                              var GetName = db.Users.Where(x => x.UserID == GetUsersFromDept).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                                  var objcom = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name")
                                   .Select(o => o.AppValue)
                                   .FirstOrDefault();
                                  var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Add New Training").Select(o => o.EmailTemplateID).FirstOrDefault();
                                  var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Add New Training").Select(x => x.TemplatePath).FirstOrDefault();
                                  if ((check != null) && (check != 0))
                                  {
                                      var obj1 = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Add New Training")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
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
                                      string Subject = "Training Created on " + DateTime.Today.ToString("dd MMM yyyy");
                                      obj1.Subject = " " + objcom + " New training schedule has been created";
                                     // string tname;
                                      html = html.Replace("#TrainingName", tname);
                                      html = html.Replace("#ScheduledDate", sdate.ToString("dd-MM-yyyy"));
                                      html = html.Replace("#start", stim);
                                      html = html.Replace("#end", etim);
                                      html = html.Replace("#Instructor", instr);
                                      html = html.Replace("#ServerName", ServerName);
                                      html = html.Replace("#CompanyName", objcom.ToString());
                                      List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
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
                                      ExceptionHandlingController.TemplateMissing("Create Assessment", folder, ServerName);
                                  }
                          }
                      }
                      */
                /////////////
                var LevelList = db.Master_TrainingLevel.ToList();
                var TechList = db.Master_TrainingTechnology.ToList();
                var query = db.Users.OrderBy(a => a.FirstName).Where(x => x.IsActive == true).ToList();
                List<LDCourse1> ldm = new List<LDCourse1>();
                LDCourse1 lModel = new LDCourse1();
                List<SelectListItem> LevelList1 = new List<SelectListItem>();
                List<SelectListItem> TechList1 = new List<SelectListItem>();
                IList<SelectListItem> usr = new List<SelectListItem>();
                foreach (var list in LevelList)
                {
                    LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
                }
                lModel.LevelIDList = LevelList1;
                foreach (var list in TechList)
                {
                    TechList1.Add(new SelectListItem { Text = list.TechnologyName, Value = Convert.ToString(list.TechnologyId) });
                }
                lModel.TechIDList = TechList1;
                foreach (var a in query)
                {
                    usr.Add(new SelectListItem { Value = Convert.ToString(a.UserID), Text = a.FirstName + " " + a.LastName });
                }
                lModel.InstructorIDList = usr.ToList();
                // ViewBag.InstructorIDList = new SelectList(InstructorList, "UserID", "FirstName" );
                for (int i = 0; i < 1; i++)
                {
                    LDCourse1 lm = new LDCourse1();
                    lm.Coursename = "";
                    lm.LevelId = 0;
                    lm.TechnologyId = 0;
                    lm.userid = 0;
                    lm.Scheduledate = null;
                    lm.Starttime = null;
                    lm.Endtime = null;
                    ldm.Add(lm);
                }
                ld.ldmlist = ldm;
                ld.LDM = lModel;
                // return RedirectToAction("MTRaining", "ManageTraining");
                var departments = (from p in db.Departments.Where(x => x.IsActive == true)
                                   select new
                                   {
                                       DepartmentId = p.DepartmentId,
                                       DepartmentName = p.DepartmentName
                                   }).ToList();
                ViewBag.Departments = new MultiSelectList(departments, "DepartmentId", "DepartmentName");
                var Users = (from p in db.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                             select new
                             {
                                 userid = p.UserID,
                                 username = p.FirstName + "" + p.LastName
                             }).ToList();
                ViewBag.Users = new MultiSelectList("", "userid", "username");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(ld);
        }

        
        [HttpGet]
        public ActionResult schedule(int ID, string scheduledate, string trainingname)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string Message = "";
            try
            {
                if (scheduledate != null || scheduledate != "")
                {
                    var dt = Convert.ToDateTime(scheduledate);

                    var result = db.Trainings.Where(x => x.ScheduledDate == dt && x.TrainingName == trainingname && x.TrainingId != ID).FirstOrDefault();

                    if (result != null)
                    {
                        Message = "available";
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Name = Message }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult NotifyUser(string Value)
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    IEnumerable<SelectListItem> FilterUser = new List<SelectListItem>();
        //    List<string> result = Value.Split(new char[] { ',' }).ToList();
        //    List<SelectListItem> Filter = new List<SelectListItem>();
        //    try
        //    {
        //        List<int?> DepartID = new List<int?>();

        //        foreach (var y in result)
        //        {
        //            DepartID.Add(Convert.ToInt32(y));
        //        }
        //        FilterUser = (from lt in db.Users.Where(x => !DepartID.Contains(x.DepartmentId))
        //                      select new
        //                      {
        //                          UserID = lt.UserID,
        //                          UserName = lt.UserName
        //                      }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserID), Text = m.UserName });
        //        foreach (var z in FilterUser)
        //        {
        //            Filter.Add(z);
        //        }
        //        //ViewBag.Users = new SelectList(Filter, "userid", "username");
        //        return Json(new SelectList(Filter.Distinct(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception Ex)
        //    {
        //        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
        //        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
        //    }
        //    return View();
        //}




        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult NotifyUser(string Value)
        {
            IEnumerable<SelectListItem> FilterUser = new List<SelectListItem>();

            if (Value != null)
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                List<int?> DepartID = new List<int?>();
                List<string> result = Value.Split(new char[] { ',' }).ToList();
                foreach (var y in result)
                {
                    DepartID.Add(Convert.ToInt32(y));
                }
                FilterUser = (from lt in db.Users.Where(x => !DepartID.Contains(x.DepartmentId))
                              select new
                              {
                                  UserID = lt.UserID,
                                  UserName = lt.UserName
                              }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserID), Text = m.UserName });
            }

            return Json(new SelectList(FilterUser, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public JsonResult scheduletime(int trainingID, string scheduledate, string trainingname, string trainingtime, string endtime)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string Message = "";
            try
            {
                var dt = Convert.ToDateTime(scheduledate);
                var time = Convert.ToDateTime(trainingtime);
                var hour = Convert.ToInt32(time.Hour);

                if (hour > 12)
                    hour -= 12;
                var min = Convert.ToInt32(time.Minute);
                var tt = time.ToString("tt");

                TimeSpan CurStartTime = new TimeSpan(hour, min, 0);

                var obj = db.Trainings.Where(x => x.ScheduledDate == dt && x.IsActive == true && x.TrainingId != trainingID).Select(o => o).ToList();

                foreach (var item in obj)
                {
                    var dbtime = Convert.ToDateTime(item.StartTime);
                    int hourdb = Convert.ToInt32(dbtime.Hour);
                    int mindb = Convert.ToInt32(dbtime.Minute);
                    var ttdb = dbtime.ToString("tt");

                    if (hourdb > 12)
                        hourdb -= 12;

                    TimeSpan Db_StartTime = new TimeSpan(hourdb, mindb, 0);

                    var dbendtime = Convert.ToDateTime(item.EndTime);
                    var hourenddb = Convert.ToInt32(dbendtime.Hour);
                    var minenddb = Convert.ToInt32(dbendtime.Minute);
                    var endttdb = dbendtime.ToString("tt");

                    if (hourenddb > 12)
                        hourenddb -= 12;

                    TimeSpan Db_EndTime = new TimeSpan(hourenddb, minenddb, 0);

                    if (endtime != "")
                    {
                        var entime = Convert.ToDateTime(endtime);
                        var enhour = Convert.ToInt32(entime.Hour);

                        if (enhour > 12)
                            enhour -= 12;
                        var enmin = Convert.ToInt32(entime.Minute);
                        var entt = entime.ToString("tt");

                        TimeSpan CurEndTime = new TimeSpan(enhour, enmin, 0);

                        if (Db_StartTime == CurStartTime || Db_StartTime == CurEndTime || Db_EndTime == CurStartTime || Db_EndTime == CurEndTime)
                        {
                            Message = "availabletime";
                            return Json(Message, JsonRequestBehavior.AllowGet);
                        }
                        else if (Db_StartTime < CurStartTime && CurStartTime < Db_EndTime)
                        {
                            Message = "availabletime";
                            return Json(Message, JsonRequestBehavior.AllowGet);
                        }
                        else if (Db_StartTime < CurEndTime && CurEndTime < Db_EndTime)
                        {
                            Message = "availabletime";
                            return Json(Message, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (Db_StartTime == CurStartTime || Db_EndTime == CurStartTime)
                        {
                            Message = "availabletime";
                            return Json(Message, JsonRequestBehavior.AllowGet);
                        }
                        else if (Db_StartTime < CurStartTime && CurStartTime < Db_EndTime)
                        {
                            Message = "availabletime";
                            return Json(Message, JsonRequestBehavior.AllowGet);
                        }
                        //else if (Db_StartTime <= CurTime && CurTime <= Db_EndTime)
                        //{
                        //    Message = "availabletime";
                        //    return Json(Message, JsonRequestBehavior.AllowGet);
                        //}
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(Message, JsonRequestBehavior.AllowGet);
        }
    }
}