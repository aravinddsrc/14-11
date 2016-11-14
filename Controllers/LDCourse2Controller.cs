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
    public class LDCourse2Controller : Controller
    {
        [HttpGet]
        public ActionResult AddCoursedetails()
        {
            LDCourse2List ld = new LDCourse2List();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                LDCourse2 obj = new LDCourse2();

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

                DSRCManagementSystem.Models.LDCourse2 ObjLD = new DSRCManagementSystem.Models.LDCourse2();
                ModelState.Clear();
                List<LDCourse2> ldm = new List<LDCourse2>();

                LDCourse2 lModel = new LDCourse2();
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

                ViewBag.Departments = new MultiSelectList(departments, "DepartmentId", "DepartmentName");

                var Users = (from p in db.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                             select new
                             {
                                 userid = p.UserID,
                                 username = p.FirstName + "" + p.LastName
                             }).ToList();


                ViewBag.Users = new MultiSelectList("", "userid", "username");



                for (int i = 0; i < 1; i++)
                {
                    LDCourse2 lm = new LDCourse2();
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
            return View(ld);
        }


        [HttpPost]
        public ActionResult AddCoursedetails(LDCourse2List model, FormCollection formData)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            LDCourse2List ld = new LDCourse2List();
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
                var LevelList = db.Master_TrainingLevel.ToList();
                var TechList = db.Master_TrainingTechnology.ToList();
                var query = db.Users.OrderBy(a => a.FirstName).Where(x => x.IsActive == true).ToList();

                List<LDCourse2> ldm = new List<LDCourse2>();

                LDCourse2 lModel = new LDCourse2();

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
                    LDCourse2 lm = new LDCourse2();
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

            return Json("Success", JsonRequestBehavior.AllowGet);
            //return RedirectToAction("MTRaining", "ManageTraining");  
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
                FilterUser = (from lt in db.Users.Where(x => !DepartID.Contains(x.DepartmentId) && x.UserName!=null)
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





