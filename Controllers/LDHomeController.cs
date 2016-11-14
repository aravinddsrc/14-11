using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.DSRCLogic;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Data.Objects;
using System.Data.Objects.SqlClient;


namespace DSRCManagementSystem.Controllers
{
    public class LDHomeController : Controller
    {
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult LDHome()
        {
            List<DSRCManagementSystem.Models.LDHomeModel> AsgnList = new List<DSRCManagementSystem.Models.LDHomeModel>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.LDHomeModel ObjAC = new DSRCManagementSystem.Models.LDHomeModel();
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
                int Id = roleID.FirstOrDefault();
                Session["LDMenu"] = DSRCLogic.StoredProcedures.GetUserMenuForLD(userId, Id);
                AsgnList = (from a in db.Trainings

                            join t in db.Master_TrainingType on a.TrainingTypeId equals t.TrainingTypeId
                            join o in db.Master_TrainingTechnology on a.TechnologyId equals o.TechnologyId
                            join i in db.Users on a.InstructorId equals i.UserID
                            // join i in db.TrainingInstructors on a.InstructorId equals i.InstructorId
                            select new DSRCManagementSystem.Models.LDHomeModel()
                            {
                                TrainingId = a.TrainingId,
                                TrainingName = t.TypeName,
                                TechnologyName = o.TechnologyName,
                                ScheduledDate = a.ScheduledDate,
                                Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : ""))

                            }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(AsgnList.ToList());
        }

        [HttpGet]
        public ActionResult Mylearning()
        {
            DSRCManagementSystem.Models.LDHomeModel AsgnList = new DSRCManagementSystem.Models.LDHomeModel();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DateTime todays = DateTime.Today;
                int userId = Convert.ToInt32(Session["UserID"].ToString());
                var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
                //int? roleid = db.UserRoles.Where(x => x.UserID == userId).Select(o => o.RoleID).FirstOrDefault();
                //int? instructorid = db.Trainings.Where(x => x.InstructorId == userId).Select(o => o.InstructorId).FirstOrDefault();
                //int Id = roleID.FirstOrDefault();
                List<int?> obj = new List<int?>();
                int BId = GetBranch(userId);
                obj = db.TrainingNominations.Where(x => x.UserId == userId).Select(o => o.TrainingId).ToList();
                int j = obj.Count();
                int? Training = db.TrainingNominations.Where(x => x.UserId == userId).Select(o => o.TrainingId).FirstOrDefault();
                List<int?> list = new List<int?>();
                list = db.TrainingNominations.Where(x => x.UserId == userId && x.IsActive == true).Select(o => o.TrainingId).ToList();
                var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                //AsgnList.upcomingTrainings 
                var tempQuery = (from training in db.Trainings.Where(x => !list.Contains(x.TrainingId) && x.IsActive == true)
                                 join trainingType in db.Master_TrainingLevel on training.TrainingTypeId equals trainingType.LevelId
                                 join tech in db.Master_TrainingTechnology on training.TechnologyId equals tech.TechnologyId
                                 join user in db.Users on training.InstructorId equals user.UserID
                                 where training.ScheduledDate > today && user.BranchId == BId
                                 select new { training, trainingType, tech, user }).OrderBy(o => o.training.ScheduledDate.Value.Year).ThenBy(o => o.training.ScheduledDate.Value.Month).ThenBy(o => o.training.ScheduledDate.Value.Day);
                AsgnList.upcomingTrainings = new List<Models.UpcomingTraningModel>();


                foreach (var item in tempQuery)
                {
                    var schdDate = item.training.ScheduledDate ?? DateTime.Now.Date;
                    var strdStartTime = item.training.StartTime.ToUpper();
                    var splitter = new[] { "AM", "PM" };
                    var tempStartTime = strdStartTime.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    var startTime = DateTime.Now;
                    if (DateTime.TryParseExact(tempStartTime[0].Trim(), "h:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startTime) && strdStartTime.Contains("pm"))
                        if (startTime.Hour != 12)
                            startTime = startTime.AddHours(12);
                    startTime = schdDate.AddHours(startTime.Hour).AddMinutes(startTime.Minute);

                    if (startTime > DateTime.Now)
                    {
                        var training = new DSRCManagementSystem.Models.UpcomingTraningModel();
                        training.TrainingId = item.training.TrainingId;
                        training.TrainingName = item.training.TrainingName;
                        training.ScheduledDate = item.training.ScheduledDate;
                        training.TechnologyName = item.tech.TechnologyName;
                        // training.Instructor = ((item.user.FirstName.Length > 0 ? item.user.FirstName : "") + " " + (item.user.LastName.Length > 0 ? item.user.LastName : ""));
                        training.Instructor = item.user.FirstName + " " + (item.user.LastName ?? "");
                        training.Seatingcapacity = item.training.SeatingCapacity;
                        training.StartTime = item.training.StartTime;
                        training.EndTime = item.training.EndTime;
                        training.instructorid = item.training.InstructorId;
                        training.Nominations = item.training.NumberOfNominated;
                         training.submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == training.TrainingId && x.Flag == true).Count();
                         training.pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == training.TrainingId && x.Flag == false).Count();
                    
                        AsgnList.upcomingTrainings.Add(training);

                    }
                    //    if (item.training.NumberOfNominated == item.training.SeatingCapacity)
                    //    {
                    //        var training = new DSRCManagementSystem.Models.UpcomingTraningModel();
                    //        training.IsSeatingCapacity = true;
                    //    }
                    //    else
                    //    {
                    //        var training = new DSRCManagementSystem.Models.UpcomingTraningModel();
                    //        training.IsSeatingCapacity = false;
                    //    }
                }

                AsgnList.historyTrainings = (from a in db.Trainings
                                             join t in db.Master_TrainingType on a.TrainingTypeId equals t.TrainingTypeId
                                             join o in db.Master_TrainingTechnology on a.TechnologyId equals o.TechnologyId
                                             join i in db.Users on a.InstructorId equals i.UserID
                                             join tn in db.TrainingNominations on a.TrainingId equals tn.TrainingId
                                             //join tf in db.TrainingFeedBackCalcs on a.TrainingId equals tf.TrainingId
                                             where tn.UserId == userId && tn.Score > 0
                                             //where tn.UserId == userId && a.ScheduledDate <= today && tn.Score > 0 && i.BranchId == BId
                                             select new DSRCManagementSystem.Models.HistorytrainingModel()
                                             {
                                                 TrainingId = a.TrainingId,
                                                 TrainingName = a.TrainingName,
                                                 TechnologyName = o.TechnologyName,
                                                 ScheduledDate = a.ScheduledDate,
                                                 //flag=tf.Flag,
                                                 Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : ""))

                                             }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).ToList();




                foreach (var attneded in AsgnList.historyTrainings)
                {
                    // var val = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == item.TrainingId && o.Flag == false).Select(o => o).ToList();
                    var FeedbackFlag = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == attneded.TrainingId).Select(o => o).FirstOrDefault();
                    //var val = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == item.TrainingId && o.Flag == true).Select(o => o).ToList();
                    //int FeedbackCount = val.Count();
                    //attneded.FeedbackCount = FeedbackCount;
                    attneded.flag = FeedbackFlag.ChoiceTopic == 0 ? false : true;
                }


                AsgnList.unattendedTrainings = (from a in db.Trainings
                                                join t in db.Master_TrainingType on a.TrainingTypeId equals t.TrainingTypeId
                                                join o in db.Master_TrainingTechnology on a.TechnologyId equals o.TechnologyId
                                                join i in db.Users on a.InstructorId equals i.UserID
                                                join tn in db.TrainingNominations on a.TrainingId equals tn.TrainingId
                                                where tn.UserId == userId && a.ScheduledDate <= today && tn.Score < 0
                                                select new DSRCManagementSystem.Models.HistorytrainingModel()
                                                {
                                                    TrainingId = a.TrainingId,
                                                    TrainingName = a.TrainingName,
                                                    TechnologyName = o.TechnologyName,
                                                    ScheduledDate = a.ScheduledDate,
                                                    Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : ""))

                                                }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).ToList();

                var tempTimeQuery = (from a in db.TrainingNominations.Where(x => x.IsActive == true)
                                     join t in db.Trainings on a.TrainingId equals t.TrainingId
                                     join o in db.Master_TrainingTechnology on t.TechnologyId equals o.TechnologyId
                                     join u in db.Users on t.InstructorId equals u.UserID

                                     where a.UserId == userId && a.NominationFlag == true && u.BranchId == BId

                                     select new
                                     {
                                         a,
                                         t,
                                         o,
                                         u
                                     }).OrderBy(o => o.t.ScheduledDate.Value.Year).ThenBy(o => o.t.ScheduledDate.Value.Month).ThenBy(o => o.t.ScheduledDate.Value.Day).ToList();

                AsgnList.nominatedTrainings = new List<Models.NominatedTrainingModel>();

                foreach (var item in tempTimeQuery)
                {
                    var schdDate = item.t.ScheduledDate ?? DateTime.Now.Date;
                    var strdStartTime = item.t.StartTime.ToUpper();
                    var splitter = new[] { "AM", "PM" };
                    var tempStartTime = strdStartTime.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    DateTime startTime = DateTime.Now;
                    if (DateTime.TryParseExact(tempStartTime[0].Trim(), "h:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startTime) && strdStartTime.Contains("PM"))
                        if (startTime.Hour != 12)
                            startTime = startTime.AddHours(12);
                    startTime = schdDate.AddHours(startTime.Hour).AddMinutes(startTime.Minute);

                    if (startTime >= DateTime.Now)
                    {
                        var training = new DSRCManagementSystem.Models.NominatedTrainingModel();
                        training.NominationId = item.a.NominationId;
                        training.TrainingId = item.a.TrainingId;
                        training.TrainingName = item.t.TrainingName;
                        training.TechnologyName = item.o.TechnologyName;
                        training.ScheduledDate = item.t.ScheduledDate;
                        training.Instructor = ((item.u.FirstName.Length > 0 ? item.u.FirstName : "") + " " + (item.u.LastName.Length > 0 ? item.u.LastName : ""));
                        training.Starttime = item.t.StartTime;
                        training.IsCompleted = item.a.CompletionFlag;
                        if (training.ScheduledDate <= today)
                        {
                            training.test = true;
                        }
                        else
                        {
                            training.test = false;
                        }

                        AsgnList.nominatedTrainings.Add(training);
                    }
                }

                AsgnList.conductedtrainings = (from rc in db.Trainings
                                               join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                               join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                               join i in db.Users on rc.InstructorId equals i.UserID
                                               join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                               where rc.IsActive == true && rc.InstructorId == userId
                                               && (rc.StatusId == 3 || rc.StatusId == 4 || rc.StatusId == 6 || rc.StatusId == 7 || rc.StatusId == 10)
                                               && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                               && i.BranchId == BId
                                               //where rc.IsActive == true && rc.InstructorId == userId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date && i.BranchId == BId && n.CompletionFlag == false
                                               select new DSRCManagementSystem.Models.Conductedtrainingmodel()
                                               {
                                                   // TrainingId = rc.TrainingId,
                                                   TrainingName = rc.TrainingName,
                                                   TechnologyName = t.TechnologyName,
                                                   ScheduledDate = rc.ScheduledDate,
                                                   IsCompleted = n.CompletionFlag,
                                                   Starttime = rc.StartTime,
                                                   TrainingId = rc.TrainingId,
                                                   Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                                   submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                                   pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),
                                                   Endtime = rc.EndTime,
                                                   Nominations = rc.NumberOfNominated

                                               }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).Distinct().ToList();//OrderByDescending(o => o.ScheduledDate).ToList();
                                               //OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).Distinct().ToList();
                // var distinct = AsgnList.conductedtrainings.Distinct();
                //  var distinct = AsgnList.DistinctBy(x => x.trainingid);
                //var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
                int userID = Convert.ToInt32(Session["UserID"]);
                // var eid = db.Users.Where(x => x.UserID.Equals(userID)).Select(x => x.EmpID).FirstOrDefault();
                double totalTraineeScore = 0.0;
                double totalInstructorScore = 0.0;
                double totalabsentscore = 0.0;
                int totalCourseCompleted = 0, totalCourseConducted = 0, totalcourseabsent = 0;
                //,totalcourseabsented = 0
                // if (eid != null)

                if (userID != null)
                {
                    //int? eidTemp = Int32.Parse(eid);
                    var TraineeScore = db.TrainingNominations.Where(x => x.UserId == userID && x.Score > 0).Select(x => x.Score);
                    foreach (var score in TraineeScore)
                    {
                        if (score != 0)
                            totalTraineeScore += (double)score;
                    }

                    var TraineeScore1 = db.TrainingNominations.Where(x => x.UserId == userID && x.Score < 0).Select(x => x.Score);

                    foreach (var score1 in TraineeScore1)
                    {
                        if (score1 != 0)
                        {
                            totalabsentscore += (double)score1;
                            // NegativeScore = -totalabsentscore;
                        }
                    }
                    List<int?> Inslist = new List<int?>();
                    List<int> Inslist1 = new List<int>();
                    //Inslist = (from t in db.Trainings
                    //           join tn in db.TrainingNominations on t.TrainingId equals tn.TrainingId
                    //           where tn.Score != 0 && t.InstructorId==userId
                    //           select t.LevelId).ToList();

                    Inslist1 = db.Trainings.Where(o => o.InstructorId == userId).Select(o => o.TrainingId).ToList();

                    foreach (int i in Inslist1)
                    {
                        Inslist.Add(i);
                    }

                    var LevelTrainings = db.TrainingNominations.Where(o => o.Score != 0 && Inslist.Contains(o.TrainingId)).Select(o => o.TrainingId);

                    var InstructorLevel = db.Trainings.Where(o => LevelTrainings.Contains((int?)o.TrainingId)).Select(o => o.LevelId);

                    foreach (var levelid in InstructorLevel)
                    {
                        if (levelid != null)
                        {
                            var InsScore = db.TrainingWeightages.Where(o => o.LevelId == levelid).Select(o => o.Instructor).FirstOrDefault();
                            if (InsScore != null)
                            {
                                totalInstructorScore += Convert.ToDouble(InsScore);
                            }
                            else
                            {
                                totalInstructorScore += 0.0;
                            }
                        }
                    }
                    var statusIDs = new int[] { 3, 4, 6 };
                    totalCourseCompleted = //db.TrainingNominations.Where(x => x.EmpId == eidTemp) 
                        (from TN in db.TrainingNominations.Where(x => x.UserId == userId && x.Score > 0)
                         join T in db.Trainings on TN.TrainingId equals T.TrainingId
                         join TS in db.Master_TrainingStatus.Where(x => statusIDs.Contains(x.StatusId)) on T.StatusId equals TS.StatusId
                         select new { TN, T, TS }).ToList().Count;

                    //totalcourseabsented = (from TN in db.TrainingNominations.Where(x => x.EmpId == eidTemp && x.Score == 0)
                    //                       join T in db.Trainings on TN.TrainingId equals T.TrainingId
                    //                       join TS in db.TrainingStatus.Where(x => statusIDs.Contains(x.StatusId)) on T.StatusId equals TS.StatusId
                    //                       select new { TN, T, TS }).ToList().Count;
                    totalCourseConducted = //db.TrainingNominations.Where(x => x.EmpId == eidTemp) 
                       (from TN in db.Trainings.Where(x => x.InstructorId == userID)
                        join T in db.Trainings on TN.TrainingId equals T.TrainingId
                        join TS in db.Master_TrainingStatus.Where(x => statusIDs.Contains(x.StatusId)) on T.StatusId equals TS.StatusId
                        select new { TN, T, TS }).ToList().Count;
                    totalcourseabsent = (from n in db.TrainingNominations.Where(x => x.UserId == userID && x.Score < 0)
                                         select new { n }).ToList().Count;


                }

                ViewBag.totalCourseCompleted = totalCourseCompleted;
                ViewBag.totalCourseConducted = totalCourseConducted;
                ViewBag.totalcourseabsented = totalcourseabsent;
                ViewBag.CourseConductedScore = totalInstructorScore;
                ViewBag.CourseCompletedScore = totalTraineeScore;
                ViewBag.courseabsentscore = totalabsentscore;
                ViewBag.scor = totalInstructorScore + totalTraineeScore + totalabsentscore;

                db.Dispose();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(AsgnList);
        }
        [HttpGet]
        public ActionResult Instructor3()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteNominate(int NominationId)
        {
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var val = objdb.TrainingNominations.Where(x => x.NominationId == NominationId).Select(o => o).FirstOrDefault();
                val.IsActive = false;
                val.NominationFlag = false;
                objdb.SaveChanges();
                var trainee = objdb.Trainings.Where(x => x.TrainingId == val.TrainingId).Select(o => o).FirstOrDefault();
                int? No_Of_Nominations = trainee.NumberOfNominated;
                trainee.NumberOfNominated = No_Of_Nominations - 1;
                objdb.SaveChanges();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Mylearning(int Trainingid)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                int user = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var instructor = obj.Trainings.Where(x => x.TrainingId == Trainingid && x.InstructorId == user).Select(o => o.InstructorId).FirstOrDefault();
                var Training = obj.Trainings.Where(x => x.TrainingId == Trainingid).Select(o => o).FirstOrDefault();
                var NomitionOne = obj.TrainingNominations.Where(x => x.UserId == user && x.TrainingId == Trainingid).Select(x => x).ToList();
                int NomineeCount = NomitionOne.Count();
                if (Trainingid == 0)
                {
                    return Json(new { Result = "Null" }, JsonRequestBehavior.AllowGet);
                }
                else if (instructor != null)
                {

                    return Json(new { Result = "Instructor" }, JsonRequestBehavior.AllowGet);
                }
                else if (Training.NumberOfNominated == Training.SeatingCapacity + 2)
                {
                    return Json(new { Result = "NoSeat" }, JsonRequestBehavior.AllowGet);
                }
                else if (NomineeCount != 0)
                {
                    return Json(new { Result = "Already" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int userId = int.Parse(Session["UserID"].ToString());
                    var email = System.Web.HttpContext.Current.Application["UserName"].ToString();


                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    var empid = db.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();
                    //int emp = Convert.ToInt32(empid);
                    DSRCManagementSystemEntities1 obf = new DSRCManagementSystemEntities1();
                    //var EMP = obf.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();


                    int k = Trainingid;
                    DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                    //var id = db.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();
                    //int? value = Convert.ToInt32(id);
                    var empname = objdb.Users.Where(x => x.UserID == userId).Select(o => o.FirstName + " " + o.LastName ?? "").FirstOrDefault();
                    //  context.Authors.Where(a => a.Books.Any(b => b.BookID == bookID)).ToList();
                    var val = objdb.TrainingNominations.Where(x => x.UserId == userId && x.TrainingId == k).Select(o => o.TrainingId).FirstOrDefault();
                    int? instructorid = db.Trainings.Where(x => x.InstructorId == userId).Select(o => o.InstructorId).FirstOrDefault();
                    var technology = objdb.Trainings.Where(x => x.TrainingId == k).Select(o => o.TechnologyId).FirstOrDefault();
                    var LevelId = db.Trainings.Where(x => x.TrainingId == Trainingid).Select(x => x.LevelId).FirstOrDefault();
                    int? InsID = db.Trainings.Where(x => x.TrainingId == Trainingid).Select(o => o.InstructorId).FirstOrDefault();


                    //var trainwidgets = db.TrainingWeightages.CreateObject();
                    //trainwidgets.Trainee = user;
                    //trainwidgets.Instructor = InsID;
                    //trainwidgets.LevelId = LevelId;
                    //db.TrainingWeightages.AddObject(trainwidgets);
                    //db.SaveChanges();

                    DSRCManagementSystem.TrainingNomination nom = new DSRCManagementSystem.TrainingNomination();
                    nom.TrainingId = Trainingid;
                    nom.EmpId = empid;
                    nom.UserId = user;
                    nom.EmpName = empname;
                    nom.TechnologyId = technology;
                    nom.EmailId = email;
                    nom.NominationFlag = true;
                   // nom.CompletionFlag = true;
                    nom.CompletionFlag = true;
                    nom.IsActive = true;
                    objdb.AddToTrainingNominations(nom);
                    objdb.SaveChanges();


                    var Techref = db.Trainings.FirstOrDefault(o => o.TrainingId == k);

                    string Tid = k.ToString();
                    string tname = Techref.TrainingName;
                    DateTime d1 = Convert.ToDateTime(Techref.ScheduledDate);
                    string d = d1.ToShortDateString();
                    string stime = Techref.StartTime;
                    string etime = Techref.EndTime;

                    string username = db.Users.Where(o => o.UserID == Techref.InstructorId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                    var userdetails = db.Users.FirstOrDefault(o => o.UserID == userId);
                    string MailID = userdetails.EmailAddress;


                    string EmpName = userdetails.FirstName + " " + userdetails.LastName ?? "";


                    //string mailMessage = MailBuilder.NominationConfirmation(EmpName, Tid, tname, d, stime, etime, username);
                    var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Nomination Confirmation").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Nomination Confirmation").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objNominationConfirmation = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Nomination Confirmation")
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
                        string TemplatePathNominationConfirmation = Server.MapPath(objNominationConfirmation.Template);
                        string htmlNominationConfirmation = System.IO.File.ReadAllText(TemplatePathNominationConfirmation);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Empname", EmpName);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingId", Tid);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingName", tname);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ScheduledDate", d);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#start", stime);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#end", etime);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Instructor", username);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ServerName", ServerName);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#CompanyName", company);
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

                            Task.Factory.StartNew(() =>
                            {
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //  DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test Mail Please Ignore", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test Mail Please Ignore", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //  DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));
                                //DsrcMailSystem.MailSender.LDSendMail(null, "L & D - Training Nomination Confirmation", null, mailMessage, "TRAINING@dsrc.co.in", MailID, Server.MapPath("~/Content/Template/images/logo.png"));
                            });
                        }
                    }
                    else
                    {
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Nomination Confirmation", folder, ServerName);
                    }

                    //int k = Trainingid;

                    var value1 = obf.Trainings.Where(x => x.TrainingId == Trainingid).Select(o => o).FirstOrDefault();
                    value1.Flag = false;
                    obf.SaveChanges();

                    var No_Of_Nominees_Ref = db.Trainings.FirstOrDefault(o => o.TrainingId == Trainingid);

                    int? No_Of_Nominations = No_Of_Nominees_Ref.NumberOfNominated;
                    No_Of_Nominees_Ref.NumberOfNominated = No_Of_Nominations + 1;

                    db.SaveChanges();

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Nominate(string TrainingId, string UserId)
        {
            try
            {
                int User = Convert.ToInt32(Encrypter.Decode(UserId));
               // int User = 0;
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                int TId = Convert.ToInt32(Encrypter.Decode(TrainingId));
                DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
                var instructor = obj.Trainings.Where(x => x.TrainingId == TId && x.InstructorId == User).Select(o => o.InstructorId).FirstOrDefault();
                var Training = obj.Trainings.Where(x => x.TrainingId == TId).Select(o => o).FirstOrDefault();
                var NomitionOne = obj.TrainingNominations.Where(x => x.UserId == User && x.TrainingId == TId).Select(x => x).ToList();
                int NomineeCount = NomitionOne.Count();
                if (TId == 0)
                {
                    ViewBag.Null = "Null";
                }
                else if (instructor != null)
                {

                    ViewBag.Instructor = "Instructor";
                }
                else if (Training.NumberOfNominated == Training.SeatingCapacity + 2)
                {
                    ViewBag.NoSeat = "NoSeat";
                }
                else if (NomineeCount != 0)
                {
                    ViewBag.Already = "Already";
                }
                else
                {
                    //int userId = int.Parse(Session["UserID"].ToString());
                    var email = System.Web.HttpContext.Current.Application["UserName"].ToString();
                    
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    var empid = db.Users.Where(x => x.UserID == User).Select(o => o.EmpID).FirstOrDefault();
                    //int? emp = Convert.ToInt32(empid);
                    DSRCManagementSystemEntities1 obf = new DSRCManagementSystemEntities1();
                    //var EMP = obf.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();


                    int k = TId;
                    DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                    //var id = db.Users.Where(x => x.UserID == userId).Select(o => o.EmpID).FirstOrDefault();
                    //int? value = Convert.ToInt32(id);
                    var empname = objdb.Users.Where(x => x.UserID == User).Select(o => o.FirstName + " " + o.LastName ?? "").FirstOrDefault();
                    //  context.Authors.Where(a => a.Books.Any(b => b.BookID == bookID)).ToList();
                    var val = objdb.TrainingNominations.Where(x => x.UserId == User && x.TrainingId == k).Select(o => o.TrainingId).FirstOrDefault();
                    int? instructorid = db.Trainings.Where(x => x.InstructorId == User).Select(o => o.InstructorId).FirstOrDefault();
                    var technology = objdb.Trainings.Where(x => x.TrainingId == k).Select(o => o.TechnologyId).FirstOrDefault();
                    var LevelId = db.Trainings.Where(x => x.TrainingId == TId).Select(x => x.LevelId).FirstOrDefault();
                    int? InsID = db.Trainings.Where(x => x.TrainingId == TId).Select(o => o.InstructorId).FirstOrDefault();


                    //var trainwidgets = db.TrainingWeightages.CreateObject();
                    //trainwidgets.Trainee = user;
                    //trainwidgets.Instructor = InsID;
                    //trainwidgets.LevelId = LevelId;
                    //db.TrainingWeightages.AddObject(trainwidgets);
                    //db.SaveChanges();

                    DSRCManagementSystem.TrainingNomination nom = new DSRCManagementSystem.TrainingNomination();
                    nom.TrainingId = TId;
                    //nom.EmpId = Convert.ToInt32(empid);
                    nom.EmpId = empid;
                    nom.UserId = User;
                    nom.EmpName = empname;
                    nom.TechnologyId = technology;
                    nom.EmailId = email;
                    nom.NominationFlag = true;
                    nom.CompletionFlag = false;
                    nom.IsActive = true;
                    objdb.AddToTrainingNominations(nom);
                    objdb.SaveChanges();


                    var Techref = db.Trainings.FirstOrDefault(o => o.TrainingId == k);

                    string Tid = k.ToString();
                    string tname = Techref.TrainingName;
                    DateTime d1 = Convert.ToDateTime(Techref.ScheduledDate);
                    string d = d1.ToShortDateString();
                    string stime = Techref.StartTime;
                    string etime = Techref.EndTime;

                    string username = db.Users.Where(o => o.UserID == Techref.InstructorId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                    var userdetails = db.Users.FirstOrDefault(o => o.UserID == User);
                    string MailID = userdetails.EmailAddress;


                    string EmpName = userdetails.FirstName + " " + userdetails.LastName ?? "";


                    //string mailMessage = MailBuilder.NominationConfirmation(EmpName, Tid, tname, d, stime, etime, username);
                    var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Nomination Confirmation").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Nomination Confirmation").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objNominationConfirmation = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Nomination Confirmation")
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
                        string TemplatePathNominationConfirmation = Server.MapPath(objNominationConfirmation.Template);
                        string htmlNominationConfirmation = System.IO.File.ReadAllText(TemplatePathNominationConfirmation);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Empname", EmpName);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingId", Tid);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#TrainingName", tname);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ScheduledDate", d);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#start", stime);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#end", etime);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#Instructor", username);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#ServerName",ServerName);
                        htmlNominationConfirmation = htmlNominationConfirmation.Replace("#CompanyName", company);
                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        var logo = CommonLogic.getLogoPath();

                        if (ServerName != "http://win2012srv:88/")
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

                            Task.Factory.StartNew(() =>
                            {
                                //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //  DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test Mail Please Ignore", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject + " - Test Mail Please Ignore", null, htmlNominationConfirmation + " - Testing Plaese ignore", "Test-TRAINING@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //  DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objNominationConfirmation.Subject, null, htmlNominationConfirmation, "TRAINING@dsrc.co.in", MailID, Server.MapPath(logo.ToString()));
                                //DsrcMailSystem.MailSender.LDSendMail(null, "L & D - Training Nomination Confirmation", null, mailMessage, "TRAINING@dsrc.co.in", MailID, Server.MapPath("~/Content/Template/images/logo.png"));
                            });
                        }
                    }
                    else
                    {
                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Nomination Confirmation", folder, ServerName);
                    }

                    //int k = Trainingid;

                    var value1 = obf.Trainings.Where(x => x.TrainingId == TId).Select(o => o).FirstOrDefault();
                    value1.Flag = false;
                    obf.SaveChanges();

                    var No_Of_Nominees_Ref = db.Trainings.FirstOrDefault(o => o.TrainingId == TId);

                    int? No_Of_Nominations = No_Of_Nominees_Ref.NumberOfNominated;
                    No_Of_Nominees_Ref.NumberOfNominated = No_Of_Nominations + 1;

                    db.SaveChanges();

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

        public int GetBranch(int Id)
        {
            // int user = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
            var BranchId = obj.Users.Where(x => x.UserID == Id).Select(x => x.BranchId).FirstOrDefault();
            int BId = Convert.ToInt32(BranchId);
            return BId;
        }

    }
}
