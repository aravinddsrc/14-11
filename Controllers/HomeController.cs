using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data;
using DSRCManagementSystem.DSRCLogic;
using System.Net.Mail;
using System.Threading.Tasks;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Web.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Globalization;





namespace DSRCManagementSystem.Controllers
{
    [CompressFilter]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        [HttpGet]

        public ActionResult Index(string id,string Tab)
        {
            if (id != null && id == "AssetManagement")
            {
                ViewBag.IsAssetManagement = true;
            }
            if (Tab != null && Tab == "Training")
            {
                ViewBag.Taining = true;
            }
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            int monthno = first.Month;
            var monthname = System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(monthno);
            Session["Lastmonth"] = monthname;
            return Index();
        }

        public ActionResult Index()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            Dashboard _dashboard = new Dashboard();
            
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                Session["AssignReportingPerson"] = 2;
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                /************************************************************************************************************************************/
                ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name = "All Leave Types" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
                var leaveRequestQuery = from leaveRequestsData in db.LeaveRequests
                                        where leaveRequestsData.ReportingTo == userId && leaveRequestsData.StartDateTime >= System.DateTime.Today//&& leaveRequestsData.LeaveStatusId==2
                                        select leaveRequestsData;

                var leaveRequestsResult = GetLeaveRequestsQuery(leaveRequestQuery, null, 2).ToList();

                _dashboard.leaveRequestsResult = leaveRequestsResult;
                /************************************************************************************************************************************/
                _dashboard.messages = communicationHelper.GetMessages(userId);
                _dashboard.timeWorked = communicationHelper.GetTimeWorked(userId).OrderBy(x => x.Date).ToList();
                GetJsonWorkingHours(_dashboard);
                var value = Convert.ToString(Session["IsRerportingPerson"]);
                GetMenuIcon(_dashboard);
                if (Convert.ToBoolean(Session["IsRerportingPerson"]))
                {
                    communicationHelper.RemoveResignedEmployees();

                    int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                    _dashboard.Propation = db.QuickEnrolls.Where(o => o.IsActive == true).OrderBy(o => o.DateOfJoin).Select(o => new Propation()
                                             {
                                                 Name = o.FirstName + " " + o.LastName,
                                                 JoiningDate = o.DateOfJoin,
                                                 Department = o.Department.DepartmentName,
                                                 Experience = o.Experience
                                             }).ToList();
                    _dashboard.Noticeperiod = db.Users.Where(o => o.UserStatus == 2 && o.IsActive == true && o.BranchId == BranchId).OrderByDescending(o => o.LastWorkingDate).Select(o => new NoticePeriod()
                        {
                            Name = o.FirstName + " " + o.LastName,
                            ResignedOn = o.ResignedOn,
                            Department = o.Department.DepartmentName,
                            LastWorkingDate = o.LastWorkingDate
                        }).OrderBy(o => o.LastWorkingDate.Value.Year).ThenBy(o => o.LastWorkingDate.Value.Month).ThenBy(o => o.LastWorkingDate.Value.Day).ToList();

                    var MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);

                    _dashboard.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: MemberList, Date: DateTime.Today.AddDays(-1), IsAscending: true, BranchId: BranchId).Take(5).ToList();

                    GetRAJStatus(_dashboard);
                    GetTraining(_dashboard);

                    UnInformedLeave(_dashboard);
                    //GetMenuIcon(_dashboard);



                    ViewBag.TotalComputers = db.computermanagements.Where(x => x.ISDelete == false).Count();
                    ViewBag.ActiveComputers = db.computermanagements.Where(o => o.ComputerStatusNew == "Active" && o.ISDelete == false).Count();
                    ViewBag.AssignedComputers = db.ComputerAssigneds.Where(o => o.ISDelete == false).Count();

                    ViewBag.TotalHardwares = db.Assets.Where(x => x.ISDelete == false).Count();
                    ViewBag.Inuse = db.Assets.Where(o => o.InUse == true && o.ISDelete == false).Count();
                    ViewBag.AssignedHardwares = db.Assets.Where(o => o.ISDelete == false && o.ComputerName != 0).Count();

                    ViewBag.TotalServers = db.ManageServers.Where(b => b.ISDelete == false).Count();
                    ViewBag.ActiveServer = db.ManageServers.Where(o => o.Raid == true && o.ISDelete == false).Count();
                    ViewBag.AssignedServer = db.ManageServers.Where(o => o.Raid == true && o.ISDelete == false && o.Assignedto != "0;" && o.NameofProjectsHosted != "0;").Count();
                
                    List<SelectListItem> Months = GetMonths();
                    _dashboard.Month = Months;
                    List<int?> obj = new List<int?>();
                    int BId = GetBranch(userId);                  
                     
                    return View("ManagerIndex", _dashboard);
                }

            }
            catch (Exception ex)
            {
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
                long UserID = Convert.ToInt64(System.Web.HttpContext.Current.Session["UserID"]);
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var dataobj = db.ExceptionLogs.CreateObject();
                dataobj.UserID = UserID;
                dataobj.MethodName = (ex.InnerException != null) ? (ex.InnerException.TargetSite.Name == null ? "" : ex.InnerException.TargetSite.Name) : (ex.TargetSite.Name == null ? "" : ex.TargetSite.Name); ;
                dataobj.ExceptionDate = DateTime.Now;
                dataobj.ExceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                dataobj.Source = (ex.InnerException != null) ? ex.InnerException.Source : ex.Source;
                dataobj.StackTrace = (ex.InnerException != null) ? ex.InnerException.StackTrace : ex.StackTrace;
                db.ExceptionLogs.AddObject(dataobj);
                db.SaveChanges();
                int LoginStatusId = MasterEnum.LoginStatus.LoginFailed.GetHashCode();
                var LoginStatus = db.Audit_LoginStatus.CreateObject();
                LoginStatus.IPAddress = IP;
                LoginStatus.BrowserVersion = BrowserVersion;
                LoginStatus.LogedInDate = LoginAt;
                LoginStatus.LoginStatusID = LoginStatusId;
                db.Audit_LoginStatus.AddObject(LoginStatus);
                db.SaveChanges();
            }
            return View(_dashboard);
        }


        private void GetRAJStatus(Dashboard _dashboard)
        {
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
            _dashboard.RAG = (from p in db.Projects
                              join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                              join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                              join u in db.Users on p.CommentedBy equals u.UserID into leftu
                              from usr in leftu.DefaultIfEmpty()
                              where p.IsDeleted != true || p.IsDeleted == null && p.IsActive == true && usr.BranchId == BranchId
                              select new ProjectRAGStatus()
                              {
                                  ProjectID = p.ProjectID,
                                  ProjectName = p.ProjectName,
                                  CurrentRAGStatus = p.RAGStatus,
                                  RAGStatusComments = p.RAGComments,
                                  CommentedBy = usr.FirstName + " " + (usr.LastName ?? "")

                              }).OrderBy(x => x.CurrentRAGStatus).ToList();
        }

        private void GetTraining(Dashboard _dashboard)
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
            _dashboard.upcomingTrainings = new List<Models.UpcomingTraningModel>();


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



                    _dashboard.upcomingTrainings.Add(training);

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

            _dashboard.historyTrainings = (from a in db.Trainings
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




            foreach (var attneded in _dashboard.historyTrainings)
            {
                // var val = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == item.TrainingId && o.Flag == false).Select(o => o).ToList();
                var FeedbackFlag = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == attneded.TrainingId).Select(o => o).FirstOrDefault();
                //var val = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == item.TrainingId && o.Flag == true).Select(o => o).ToList();
                //int FeedbackCount = val.Count();
                //attneded.FeedbackCount = FeedbackCount;
                attneded.flag = FeedbackFlag.ChoiceTopic == 0 ? false : true;
            }


            _dashboard.unattendedTrainings = (from a in db.Trainings
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

            _dashboard.nominatedTrainings = new List<Models.NominatedTrainingModel>();

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

                    _dashboard.nominatedTrainings.Add(training);
                }
            }

            _dashboard.conductedtrainings = (from rc in db.Trainings
                                           join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                           join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                           join i in db.Users on rc.InstructorId equals i.UserID
                                           join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                           where rc.IsActive == true && rc.InstructorId == userId && (rc.StatusId == 3 || rc.StatusId == 4 || rc.StatusId == 6 || rc.StatusId == 7 || rc.StatusId == 10)
                                            && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                            && i.BranchId == BId
                                           //where rc.IsActive == true && rc.InstructorId == userId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date && i.BranchId == BId && n.CompletionFlag == false
                                           select new DSRCManagementSystem.Models.Conductedtrainingmodel()
                                           {

                                               TrainingName = rc.TrainingName,
                                               TechnologyName = t.TechnologyName,
                                               ScheduledDate = rc.ScheduledDate,
                                               Starttime = rc.StartTime,
                                               TrainingId = rc.TrainingId,
                                               Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                               Endtime = rc.EndTime,
                                               Nominations = rc.NumberOfNominated,
                                               IsCompleted = n.CompletionFlag,
                                               submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                               pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),

                                           }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).Distinct().ToList();
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
        [HttpGet]
        public ActionResult DeleteNominate(int NominationId)
        {
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var val = objdb.TrainingNominations.Where(x => x.NominationId == NominationId).Select(o => o).FirstOrDefault();
                val.IsActive = false;
                val.NominationFlag = false;
                objdb.SaveChanges();
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


        public ActionResult LeaveRequestNotification()
        {
            Notification obj = new Notification();
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1();
                List<NotificationLeaveDetails> list = dbhrms.LeaveRequests.Where(o => o.ReportingTo == userId && o.LeaveStatusId == 1).Select(o =>
                                                            new NotificationLeaveDetails()
                                                            {
                                                                UserName = o.User1.FirstName,
                                                                RequestedDateTime = o.RequestedDate
                                                            }).ToList();

                foreach (var item in list)
                {
                    //if (DateTime.Now.Date == item.RequestedDateTime.Value.Date)

                    if (DateTime.Now.Date == item.RequestedDateTime.Value.Date)
                    {
                        if ((DateTime.Now.Subtract(item.RequestedDateTime.Value).Hours == 0))
                            item.Time = (DateTime.Now.Subtract(item.RequestedDateTime.Value).Minutes) + "Minutes ago";
                        else
                            item.Time = (DateTime.Now.Subtract(item.RequestedDateTime.Value).Hours) + "Hours Ago ";
                    }

                    else
                        item.Time = DateTime.Now.Subtract(item.RequestedDateTime.Value).Days + "Days ago";
                }
                obj.NotifyCount = list.Count;
                obj.Values = list;
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        static string DateFormat = "dd/MMM";
        static string LeaveFormat = "(L)";
        static string NoOutEntryFormat = "(OEM)";

        public void GetJsonWorkingHours(Dashboard _dashboard)
        {
            _dashboard.JsonWorkedData = new List<JsonData>();
            if (_dashboard.timeWorked != null)
            {

                foreach (var record in _dashboard.timeWorked)
                {

                    if (record.IsAbsent)
                    {
                        _dashboard.JsonWorkedData.Add(new DSRCManagementSystem.Models.JsonData()
                        {
                            date = record.Date.ToString(DateFormat) + LeaveFormat,
                            hoursWorked = 0
                        });
                    }
                    else
                        if (!record.IsOutEntry)
                        {
                            _dashboard.JsonWorkedData.Add(new DSRCManagementSystem.Models.JsonData()
                            {
                                date = record.Date.ToString(DateFormat) + NoOutEntryFormat,
                                hoursWorked = 0
                            });
                        }
                        else
                        {

                            /************** Less Hours for break who are all enter after 2 PM
                             * 
                             *  var workingHour = 0.0;

                           //If Employee Entered after 02:00 PM LUNCH BREAK wont detected.
                            if (record.InTmieMin >= 840 && record.InTmieMin <= 1080)
                            {
                                workingHour = Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours);
                            }
                            else
                            {
                                workingHour = Math.Round(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours, 0) >= 5 ? (Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours) - 1) : Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours);
                            }          
                             * 
                             * *************************/

                            //var workingHour = Math.Round(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours, 0) >= 5 ? (Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours) - 1) : Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours);
                            var workingHour = Math.Floor(TimeSpan.FromMinutes(record.minsWorked ?? 0).TotalHours);
                            var blanceMinutes = ((record.minsWorked % 60) / 100);
                            //var balance  =string.Format("{0:0.00;zero}", blanceMinutes);                           
                            ////int balanceminutes = Convert.ToInt32(blanceMinutes);
                            //var value = Convert.ToDecimal(blanceMinutes);
                            ////var i = Math.Floor(value);
                            //string value1 = Convert.ToString(blanceMinutes);
                            //string zero = "0";
                            //int count = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
                            //if (count == 1)
                            //{

                            //    var test = value1 + zero+"m";

                            //}

                            var totalTime = Math.Round((workingHour + (blanceMinutes ?? 0)), 2, MidpointRounding.AwayFromZero);
                            var JsonLeaveData = new DSRCManagementSystem.Models.JsonData()
                                {
                                    date = record.Date.ToString(DateFormat),
                                    hoursWorked = totalTime,
                                    Day = record.Date.ToString("dddd"),

                                    InTime = record.InTime,
                                    OutTime = record.OutTime
                                };
                            JsonLeaveData.hours = String.Format("{0:0.00}", totalTime).Replace('.', ':');

                            _dashboard.JsonWorkedData.Add(JsonLeaveData);



                        }
                }
            }
            /*_dashboard.JsonWorkedData.Add(new DSRCManagementSystem.Models.JsonData()
            {
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                hoursWorked = null
            });*/
        }


        [HttpPost]
        public ActionResult Index(Dashboard _dashboard)
        {
            bool IsMessage = _dashboard.ReplyOk != null;
            bool IsYesorno = _dashboard.ReplyYes != null || _dashboard.ReplyNo != null;
            bool? opinion = _dashboard.ReplyYes != null ? true : (_dashboard.ReplyNo != null ? false : false);
            try
            {




                int userId = Convert.ToInt32(Session["UserID"]);

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name = "All Leave Types" } }.Union(db.LeaveTypes.ToList()), "LeaveTypeId", "Name", 0);
                    var leaveRequestQuery = from leaveRequestsData in db.LeaveRequests
                                            where leaveRequestsData.ReportingTo == userId && leaveRequestsData.StartDateTime >= System.DateTime.Today//&& leaveRequestsData.LeaveStatusId==2
                                            select leaveRequestsData;

                    var leaveRequestsResult = GetLeaveRequestsQuery(leaveRequestQuery, null, 2).ToList();
                    //if (Request.IsAjaxRequest())
                    //{
                    //return PartialView("_UpcomingLeaves", leaveRequestsResult);
                    _dashboard.leaveRequestsResult = leaveRequestsResult;
                    //}
                }
                if (IsYesorno)
                {
                    int messageId = Convert.ToInt32(_dashboard.ReplyYes ?? _dashboard.ReplyNo);
                    communicationHelper.replyToMessage(messageId, _dashboard.Comments, _dashboard.ReplyYes != null, opinion, userId);
                    ViewBag.PageHeader = "Home";
                }
                else
                    if (IsMessage)
                    {
                        int messageId = Convert.ToInt32(_dashboard.ReplyOk);
                        communicationHelper.replyToMessage(messageId, _dashboard.Comments, true, null, userId);
                        ViewBag.PageHeader = "Home";
                    }
                ModelState.Clear();
                _dashboard.messages = communicationHelper.GetMessages(userId);
                _dashboard.timeWorked = communicationHelper.GetTimeWorked(userId).OrderBy(x => x.Date).ToList();
                GetJsonWorkingHours(_dashboard);
                _dashboard.Comments = string.Empty;
                if (Convert.ToBoolean(Session["IsRerportingPerson"]))
                {
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {
                        _dashboard.Propation = db.Users.Where(o => o.IsBoarding == true).OrderBy(o => o.DateOfJoin).Select(o => new Propation()
                        {
                            Name = o.FirstName,
                            JoiningDate = o.DateOfJoin,
                            Department = o.Department.DepartmentName,
                            Experience = o.Experience
                        }).ToList();
                        _dashboard.Noticeperiod = db.Users.Where(o => o.IsUnderNoticePeriod == true).OrderBy(o => o.LastWorkingDate).Select(o => new NoticePeriod()
                        {
                            Name = o.FirstName,
                            ResignedOn = o.ResignedOn,
                            Department = o.Department.DepartmentName,
                            LastWorkingDate = o.LastWorkingDate
                        }).ToList();
                         var monthId=DateTime.Now.Month;
                        int MID = Convert.ToInt32(monthId);
                        //_dashboard.Month = db.Master_TSR_Month.Where(m=> m.Id<=MID).Select(o => new Month()
                        //    {
                        //        MonthName = o.Month
                        //    }).ToList();
                        var MemberList = TimeEntryHelper.GetTeamMemberList(UserId: userId);
                        int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                        _dashboard.EmployeeData = TimeEntryHelper.GetTeamMemberData(teamMembers: MemberList, Date: DateTime.Today.AddDays(-1), IsAscending: true, BranchId: BranchId).Take(5).ToList();
                        GetRAJStatus(_dashboard);
                        UnInformedLeave(_dashboard);
                        return View("ManagerIndex", _dashboard);
                    }
                }
                return View(_dashboard);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(_dashboard);
        }
        /*Same as in LeaveController*/
        private static IQueryable<LeaveRequest> GetLeaveRequestsQuery(IQueryable<LeaveRequest> leaveRequestQuery, int? leaveTypeId, int? leaveStatusId)
        {
            if (leaveTypeId != null && leaveTypeId != 0 && leaveStatusId != null && leaveStatusId != 0)
            {
                return
                    leaveRequestQuery.Where(
                        item =>
                            item.LeaveTypeId == leaveTypeId &&
                            item.LeaveStatusId == leaveStatusId).Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveTypeId != null && leaveTypeId != 0)
            {
                return
                    leaveRequestQuery.Where(item => item.LeaveTypeId == leaveTypeId)
                        .Include(x => x.User).Include(i => i.User1);
            }
            else if (leaveStatusId != null && leaveStatusId != 0)
            {
                return
                    leaveRequestQuery.Where(item => item.LeaveStatusId == leaveStatusId)
                        .Include(x => x.User).Include(i => i.User1);
            }
            return leaveRequestQuery.Include(x => x.User).Include(i => i.User1);
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View("Error");
        }

        //public ActionResult UserFeedback()
        //{
        //    return View(new UserFeedback());
        //}

        //[HttpPost]
        //public ActionResult UserFeedback(UserFeedback userFeedback)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        int userID = Convert.ToInt32(Session["UserID"]);
        //        string email = Convert.ToString(Session["UserName"]); 

        //        using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
        //        {

        //            string pathvalue = CommonLogic.getLogoPath();



        //            var obj = new UsersFeedback() {
        //                UserID = userID, 
        //                Feedback = userFeedback.Feedback,
        //                FeedbackDate = DateTime.Now
        //            };
        //            db.UsersFeedbacks.AddObject(obj);
        //            db.SaveChanges();


        //            string MailMessage = MailBuilder.FeedBack(Session["FirstName"].ToString(), Session["LastName"].ToString(), userFeedback.Feedback.Replace(Environment.NewLine, "</br>"));

        //            var objFeedback = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Feedback")
        //                               join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
        //                               select new DSRCManagementSystem.Models.Email
        //                               {
        //                                   To = p.To,
        //                                   CC = p.CC,
        //                                   BCC = p.BCC,
        //                                   Subject = p.Subject,
        //                                   Template = q.TemplatePath
        //                               }).FirstOrDefault();

        //            string TemplatePathFeedback = Server.MapPath(objFeedback.Template);
        //            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
        //            string htmlFeedback = System.IO.File.ReadAllText(TemplatePathFeedback);
        //            htmlFeedback = htmlFeedback.Replace("#SenderName", Session["FirstName"].ToString() + " " + Session["LastName"].ToString());
        //            htmlFeedback = htmlFeedback.Replace("#Comments", userFeedback.Feedback.Replace(Environment.NewLine, "</br>"));
        //            htmlFeedback = htmlFeedback.Replace("#ServerName", WebConfigurationManager.AppSettings["SeverName"]);
        //            htmlFeedback = htmlFeedback.Replace("#CompanyName",company);

        //            objFeedback.To = HomeController.GetUserEmailAddress(db, objFeedback.To);
        //            objFeedback.CC = HomeController.GetUserEmailAddress(db, objFeedback.CC);
        //            if (objFeedback.BCC != "")
        //            {
        //                objFeedback.BCC = HomeController.GetUserEmailAddress(db, objFeedback.BCC);
        //            }

        //             string ServerName = WebConfigurationManager.AppSettings["SeverName"];

        //             if (ServerName  != "http://win2012srv:88/")
        //             {

        //                 List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

        //                 //MailIds.Add("boobalan.k@dsrc.co.in");
        //                 //MailIds.Add("shaikhakeel@dsrc.co.in");
        //                 //MailIds.Add("ramesh.S@dsrc.co.in");
        //                 //MailIds.Add("aruna.m@dsrc.co.in");
        //                 //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
        //                 //MailIds.Add("dineshkumar.d@dsrc.co.in");

        //                 string EmailAddress = "";

        //                 foreach (string mail in MailIds)
        //                 {
        //                     EmailAddress += mail + ",";
        //                 }

        //                 EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

        //                 string CCMailId = "kirankumar@dsrc.co.in";
        //                 string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";

        //                 Task.Factory.StartNew(() =>
        //                 {

        //                     DsrcMailSystem.MailSender.SendMailToALL(null, objFeedback.Subject + " - Test Mail Please Ignore", null, htmlFeedback + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue.ToString()));
        //                 });

        //             }
        //             else
        //             {

        //                 Task.Factory.StartNew(() =>
        //                 {

        //                     DsrcMailSystem.MailSender.SendMailToALL(null, objFeedback.Subject, "", htmlFeedback, "HRMS@dsrc.co.in", objFeedback.To, objFeedback.CC, objFeedback.BCC, Server.MapPath(pathvalue.ToString()));

        //                 });
        //             }
        //            //DsrcMailSystem.MailSender.SendMail("", "Feedback", "", MailMessage, email, "boobalan.k@dsrc.co.in", Server.MapPath("~/Content/Template/images/logo.png")); });
        //            //Task.Factory.StartNew(() => { DsrcMailSystem.MailSender.SendMail("", "Feedback", "", userFeedback.Feedback, email, "prasanthK@dsrc.co.in", Server.MapPath("~/Content/Template/images/logo.png")); });
        //            userFeedback.StatusMessage = "Success";
        //        }
        //    }
        //    return View(userFeedback);
        //}

        public void UnInformedLeave(Dashboard _dashboard)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Dashboard objmodel = new Dashboard();
            int UserID = Convert.ToInt32(Session["UserID"]);
            var BranchID = db.Users.FirstOrDefault(o => o.UserID == UserID).BranchId;
            //var ReportingID=db.UserReportings.FirstOrDefault(o=>o.ReportingUserID == UserID).ReportingUserID;
            var ReportingID = UserID;
            DateTime dt = System.DateTime.Now.Date.AddDays(-1);



            DateTime mondayOfLastWeek = dt.AddDays(-(int)dt.DayOfWeek - 6);

            DateTime fridayOfLastWeek = mondayOfLastWeek.AddDays(4);


            TimeSpan t = dt - mondayOfLastWeek;
            double NrOfDays = t.TotalDays;
            int noofdays = Convert.ToInt32(NrOfDays);

            List<DateTime?> dates = new List<DateTime?>();

            for (int i = 0; i <= noofdays; i++)
            {
                DateTime leavedate = mondayOfLastWeek.AddDays(i);
                if (i != 5 && i != 6)
                {
                    dates.Add(mondayOfLastWeek.AddDays(i));
                }
            }

            List<DateTime?> objuser = new List<DateTime?>();

            List<DateTime?> OBJ = new List<DateTime?>();

            OBJ = db.Master_holiday.Where(x => x.Date >= mondayOfLastWeek && x.Date <= fridayOfLastWeek).Select(o => o.Date).ToList();

            dates = dates.Except(OBJ).ToList();

            List<int> objuserid = new List<int>();

            objuserid = db.UserReportings.Where(o => o.ReportingUserID == ReportingID).Select(o => o.UserID).ToList();

            List<string> objEmpid = new List<string>();

            objEmpid = db.Users.Where(X => objuserid.Contains(X.UserID) && X.BranchId == BranchID && X.IsActive == true && X.EmpID != null).Select(O => O.EmpID).ToList();

            List<string> Timeentrynone = new List<string>();

            Timeentrynone = db.TimeManagements.Where(X => X.Date >= dt && (objEmpid.Contains(X.EmpID)) && X.BranchId == BranchID).Select(X => X.EmpID).ToList();

            objEmpid = objEmpid.Except(Timeentrynone).ToList();

            List<int> leaveuser = new List<int>();

            leaveuser = db.Users.Where(x => objEmpid.Contains(x.EmpID) && x.BranchId == BranchID && x.IsActive == true).Select(o => o.UserID).ToList();

            List<int> unapplyuserids = new List<int>();

            unapplyuserids = db.LeaveRequests.Where(x => leaveuser.Contains(x.UserId) && (x.StartDateTime >= mondayOfLastWeek) && (x.EndDateTime <= fridayOfLastWeek) && x.LeaveStatusId != 2).Select(X => X.UserId).ToList();

            leaveuser = leaveuser.Except(unapplyuserids).ToList();

            List<int> objint = new List<int>();

            for (int k = 0; k < leaveuser.Count(); k++)
            {
                objint.Add(Convert.ToInt32(leaveuser[k]));
            }


            List<DSRCManagementSystem.Models.Userid> OBJMODEL = new List<DSRCManagementSystem.Models.Userid>();

            List<DSRCManagementSystem.Models.AbsentDate> objabsentees = new List<DSRCManagementSystem.Models.AbsentDate>();


            var AbsentDates = new List<DateTime?>();
            var PresentDates = new List<DateTime?>();
            for (int i = 0; i < leaveuser.Count; i++)
            {
                var id = leaveuser[i];
                for (int j = 0; j < dates.Count(); j++)
                {
                    var Date = dates[j];
                    var EndDate = dates[dates.Count - 1];
                    var dbvalue = db.LeaveRequests.Where(x => EntityFunctions.TruncateTime(x.StartDateTime) >= Date || EntityFunctions.TruncateTime(x.StartDateTime) <= EndDate && x.UserId == id).Select(o => o).ToList();
                    var firstname = db.Users.Where(x => x.UserID == id).Select(o => o).FirstOrDefault();
                    if (dbvalue.Count() == 0)
                    {
                        Userid objuserids = new Userid();
                        objuserids.Userids = id;
                        objuserids.name = firstname.FirstName + " " + firstname.LastName;
                        objuserids.startdate = Convert.ToDateTime(dates[j]).ToString("dd/MMM/yyyy");
                        objuserids.days = 1;
                        OBJMODEL.Add(objuserids);

                    }


                }
            }




            //Date For One Week//
            //for (int d = 0; d < leaveuser.Count(); d++)
            //{
            //    List<string> objdatetime = new List<string>();

            //    AbsentDate objabsent = new AbsentDate();

            //    int absentuserid = leaveuser[d];
            //    var emp = db.Users.Where(x => x.UserID == absentuserid).Select(o => o.EmpID).FirstOrDefault();
            //    PresentDates = db.TimeManagements.Where(x => x.EmpID == emp && x.Date >= mondayOfLastWeek).Select(o => EntityFunctions.TruncateTime(o.Date)).ToList();
            //    AbsentDates = dates.Except(PresentDates).ToList();
            //    var absentname = db.Users.Where(x => x.UserID == absentuserid).Select(o => o).FirstOrDefault();
            //    var datevalues = OBJMODEL.Where(x => x.Userids == absentuserid).Select(o => o).ToList();
            //    var datacount = datevalues.Count();
            //    var datevalue = OBJMODEL.Where(x => x.Userids == absentuserid).Select(o => o).FirstOrDefault();
            //    objabsent.Name = absentname.FirstName + " " + absentname.LastName;

            //    var Datevalues = OBJMODEL.Where(x => x.Userids == absentuserid).Select(o => o.startdate).ToList();


            //    for (int g = 0; g < AbsentDates.Count; g++)
            //    {

            //        AbsentDate absentdatevalue = new AbsentDate();


            //        if (g != AbsentDates.Count - 1)
            //        {
            //            objdatetime.Add(Convert.ToDateTime(AbsentDates[g]).Date.ToString("dd/MMM/yyyy") + "    ,   ");
            //        }
            //        else
            //        {
            //            objdatetime.Add(Convert.ToDateTime(AbsentDates[g]).Date.ToString("dd/MMM/yyyy") + "");
            //        }

            //        objabsent.StartDate = objdatetime;

            //    }

            //    objabsentees.Add(objabsent);


            //}
            //Date For One Week//

            _dashboard.objuser = OBJMODEL;

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


        public ActionResult LeaveDetails(int userId, int leaveTypeId)
        {
            List<LeaveDetails> obj = new List<LeaveDetails>();
            obj = communicationHelper.LeaveDetails(userId: userId, leaveTypeId: leaveTypeId);
            ViewBag.Leavetypeid = leaveTypeId;
            try
            {


                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var doj = db.Users.FirstOrDefault(item => item.UserID == userId).DateOfJoin;

                bool isEligible;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;

                    if (completedDays < 365)
                    {
                        isEligible = false;
                    }
                    else
                    {
                        isEligible = true;
                    }
                }

                if (isEligible)
                    ViewBag.isEligible = true;
                else
                    ViewBag.isEligible = false;

                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        public ActionResult LOPLeaveDetails(int userId, bool Monthly)
        {
            List<LeaveDetails> obj = new List<LeaveDetails>();
            try
            {


                obj = communicationHelper.LOPLeaveDetails(userId: userId, Monthly: Monthly);

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var doj = db.Users.FirstOrDefault(item => item.UserID == userId).DateOfJoin;

                bool isEligible;

                if (doj == null)
                    isEligible = false;
                else
                {
                    var completedDays = (DateTime.Now - doj).Value.Days;

                    if (completedDays < 365)
                    {
                        isEligible = false;
                    }
                    else
                    {
                        isEligible = true;
                    }
                }

                if (isEligible)
                    ViewBag.isEligible = true;
                else
                    ViewBag.isEligible = false;
                return View(obj);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }
        public ActionResult HardwareDetails()
        {
            List<DSRCManagementSystem.Models.Assets> Details = new List<Assets>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                Details = (from p in db.Assets.Where(x => x.ISDelete == false)
                           join v in db.AssetTypes on p.AssetTypeId equals v.AssetTypeId
                           join r in db.computermanagements on p.ComputerName equals r.managementid
                           //join t in db.Users on r.Userid equals t.UserID
                           select new DSRCManagementSystem.Models.Assets
                           {
                               //Empid = t.EmpID,
                               Hardwarename = v.AssetName,
                               AssignedTo = r.ComputerName,
                               Status = p.InUse == true ? "InUse" : "Not InUse"
                               //name = t.FirstName + "." + t.LastName
                           }).Distinct().ToList();
                return View(Details);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Details);
        }
        public ActionResult ServerDetails()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.ManageServer> Details = new List<Models.ManageServer>();


            List<DSRCManagementSystem.Models.ServerDetails> FullDetails = new List<DSRCManagementSystem.Models.ServerDetails>();
            try
            {
                Details = (from p in db.ManageServers.Where(x => x.Raid == true && x.Assignedto != "0" && x.ISDelete == false)
                           select new DSRCManagementSystem.Models.ManageServer
                           {
                               NameofProjectsHosted = p.NameofProjectsHosted,
                               Assignedto = p.Assignedto,
                               MachineName = p.MachineName

                           }).ToList();
                string NameOfProjects = "";
                string UsersName = "";
                string machinename = "";

                foreach (DSRCManagementSystem.Models.ManageServer ms in Details)
                {
                    NameOfProjects = "";
                    UsersName = "";

                    var projects = ms.NameofProjectsHosted.Split(';');

                    foreach (string pid in projects)
                    {
                        if (pid != "")
                        {
                            int ProjectID = Convert.ToInt32(pid);

                            if (ProjectID != 0)
                                NameOfProjects += db.Projects.FirstOrDefault(o => o.ProjectID == ProjectID).ProjectCode + ", ";
                        }
                    }

                    NameOfProjects = NameOfProjects.Remove(NameOfProjects.Length - 2, 2);

                    var names = ms.Assignedto.Split(';');

                    foreach (string userid in names)
                    {
                        if (userid != "")
                        {
                            int UserId = Convert.ToInt32(userid);

                            UsersName += db.Users.Where(o => o.UserID == UserId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault() + ", ";
                        }
                    }
                    machinename = db.ManageServers.Where(o => o.MachineName == ms.MachineName).Select(o => o.MachineName).FirstOrDefault();
                    UsersName = UsersName.Remove(UsersName.Length - 2, 2);
                    FullDetails.Add(new ServerDetails(NameOfProjects, UsersName, machinename));
                }
                return View(FullDetails);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(FullDetails);
        }

        public ActionResult ComputerDetails()
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.AssignComputers> Details = new List<DSRCManagementSystem.Models.AssignComputers>();
            try
            {
                Details = (from p in db.ComputerAssigneds.Where(x => x.ISDelete != true)
                           join v in db.computermanagements on p.Managementid equals v.managementid
                           where v.ISDelete == false
                           join t in db.Users on p.Userid equals t.UserID
                           select new DSRCManagementSystem.Models.AssignComputers
                           {
                               Empid = t.EmpID,
                               ComputerName = v.ComputerName,
                               name = t.FirstName + "." + t.LastName
                           }).Distinct().ToList();
                return View(Details);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Details);
        }

        public ActionResult AuditLog()
        {
            var obj = new List<AuditLogs>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var reords = db.AuditLogs.Where(x => EntityFunctions.TruncateTime(x.LogedInDate) == DateTime.Today.Date).Select(x => x).ToList();
                    obj = reords.Select(r => new AuditLogs()
                    {
                        FirstName = r.FirstName,
                        UserName = r.LoginID,
                        Role = r.Roles,
                        LogInDate = r.LogedInDate,
                        LogOutDate = r.LoggedOutDate,
                        IpAddress = r.IpAddress,
                        BrowserName = r.BrowserVersion,
                        OSName = r.OsVersion
                    }).ToList();
                    return View(obj);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(obj);
        }

        public ActionResult LastLoggedOn(string userName)
        {
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var lastLoggedOn = "";

                    var lastLogged = db.AuditLogs.Where(x => x.LoginID.Equals(userName)).ToList();

                    if (lastLogged != null)
                    {
                        if (lastLogged.Count == 1)
                            lastLoggedOn = lastLogged.OrderByDescending(x => x.LogedInDate).FirstOrDefault().LogedInDate.ToString();
                    }
                    return Json(lastLoggedOn, JsonRequestBehavior.AllowGet);
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

        public ActionResult ApproveLeaveRequest(string RequestID)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");

            try
            {


                var Id = Convert.ToInt32(Encrypter.Decode(RequestID));
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestToUpdate = db.LeaveRequests.Where(o => o.LeaveRequestId == Id).Select(o => o).FirstOrDefault();
                ViewBag.IsalreadyApproved = leaveRequestToUpdate.LeaveStatusId == 2 ? true : false;
                ViewBag.IsCanceled = leaveRequestToUpdate.LeaveStatusId == 4 ? true : false;
                ViewBag.IsalreadyRejected = leaveRequestToUpdate.LeaveStatusId == 3 ? true : false;
                if (ViewBag.IsalreadyApproved == false && ViewBag.IsCanceled == false && ViewBag.IsalreadyRejected == false)
                {
                    double LOPs = 0.0;

                    if (leaveRequestToUpdate.LeaveTypeId != 4 && leaveRequestToUpdate.LeaveTypeId != 6)
                    {
                        GetLOPDays(Id);
                        LOPs = Convert.ToDouble(TempData["LOP"]);
                    }

                    leaveRequestToUpdate.LeaveStatusId = 2;
                    leaveRequestToUpdate.ProcessedBy = leaveRequestToUpdate.ReportingTo;
                    leaveRequestToUpdate.ProcessedOn = System.DateTime.Now;
                    leaveRequestToUpdate.LOP = LOPs;
                    db.SaveChanges();

                    ////If Marriage Leave is Applied and Approved Marital Status has to be updated in Users Table
                    if (leaveRequestToUpdate.LeaveTypeId == 5)
                    {
                        var UpdateMaritalStatus = db.Users.FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                        UpdateMaritalStatus.MaritalStatus = 1; /* 1-Married 2-UnMarried*/
                        db.SaveChanges();
                    }

                    var FromDate = leaveRequestToUpdate.StartDateTime;
                    var ToDate = leaveRequestToUpdate.EndDateTime;
                    var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                    bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                    LeaveModel leaveRequest = new LeaveModel();
                    var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                    var a = LeaveController.GetLeaveBalance(years, leaveRequestToUpdate.UserId);

                    leaveRequest.Balance = (from b in a
                                            select new LevaeBalance()
                                            {
                                                LeaveTypeId = b.LeaveTypeId,
                                                Name = b.LeaveType,
                                                DaysAllowed = (int)b.DaysAllowed,
                                                UsedDays = (int)b.UsedDays,

                                            }).ToList();

                    double RemainingDays = leaveRequest.Balance.Where(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Select(o => o.RemainingDays).FirstOrDefault();

                    if (leaveRequestToUpdate.LeaveTypeId == 5 || leaveRequestToUpdate.LeaveTypeId == 6)
                    {
                        var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                                  where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                                      leavebalance.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId
                                                  select leavebalance).FirstOrDefault();
                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = leaveRequestToUpdate.LeaveTypeId;
                            updateleavebalance.Value = leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();
                        }
                        else
                        {
                            updateleavebalance.Value = updateleavebalance.Value + leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();
                        }
                    }

                    LeaveController.UpdateLeaveBalance(leaveRequestToUpdate);

                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Name;
                    string StartTime = Convert.ToDateTime(leaveRequestToUpdate.StartDateTime).ToString("ddd, MMM d, yyyy");
                    string EndTime = Convert.ToDateTime(leaveRequestToUpdate.EndDateTime).ToString("ddd, MMM d, yyyy");

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {

                        var objLeaveRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Approved")
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
                        string TemplatePathLeaveRequestApproved = Server.MapPath(objLeaveRequestApproved.Template);
                        string htmlLeaveRequestApproved = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LeaveTypeName", LeaveTyepName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#StartDateTime", StartTime);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#EndDateTime", EndTime);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#CompanyName", company);
                        if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                        {
                            //FULL RED #FF0000
                            string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", LOPDays);

                            string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request has to be considered as LOP.</p>";
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", LOP);
                        }
                        else
                        {
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", "");
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", "");
                        }

                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ServerName",ServerName);
                        //string mailMessage = MailBuilder.LeaveRequestApproved(leaveRequestToUpdate.User1.FirstName, leaveRequestToUpdate.User1.LastName, leaveRequestToUpdate.User.FirstName, leaveRequestToUpdate.User.LastName, leaveRequestToUpdate.Comments);

                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                        if (ServerName  != "http://win2012srv:88/")
                        {

                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                            //MailIds.Add("boobalan.k@dsrc.co.in");
                            //MailIds.Add("ramesh.S@dsrc.co.in");
                            //MailIds.Add("aruna.m@dsrc.co.in");
                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                            //MailIds.Add("kirankumar@dsrc.co.in");
                            //MailIds.Add("francispaul.k.c@dsrc.co.in");
                            //MailIds.Add("dineshkumar.d@dsrc.co.in");


                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                var logo = CommonLogic.getLogoPath();

                                //DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "virupaksha.gaddad@dsrc.co.in", "", Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "virupaksha.gaddad@dsrc.co.in", "", Server.MapPath(logo.ToString()));

                            });

                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                                var logo = CommonLogic.getLogoPath();
                                // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, objLeaveRequestApproved.CC, "", Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, objLeaveRequestApproved.CC, "", Server.MapPath(logo.ToString()));
                            });
                        }
                    }
                    else
                    {
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Leave Request Approved", folder, ServerName);
                    }
                }
                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult RejectLeaveRequest(String RequestID)
        {
            try
            {

                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var Id = Convert.ToInt32(Encrypter.Decode(RequestID));
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestToUpdate = db.LeaveRequests.Where(o => o.LeaveRequestId == Id).Select(o => o).FirstOrDefault();
                ViewBag.IsCanceled = leaveRequestToUpdate.LeaveStatusId == 4 ? true : false;
                ViewBag.IsalreadyRejected = leaveRequestToUpdate.LeaveStatusId == 3 ? true : false;

              //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                if (ViewBag.IsalreadyRejected == false && ViewBag.IsCanceled == false)
                {
                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Name;
                    string StartTime = Convert.ToDateTime(leaveRequestToUpdate.StartDateTime).ToString("ddd, MMM d, yyyy");
                    string EndTime = Convert.ToDateTime(leaveRequestToUpdate.EndDateTime).ToString("ddd, MMM d, yyyy");

                    if (leaveRequestToUpdate.LeaveStatusId == 2 && leaveRequestToUpdate.LeaveTypeId != 4)
                    {
                        ////If Marriage Leave is Applied and Approved and Rejected then Marital Status has to be updated in Users Table again
                        if (leaveRequestToUpdate.LeaveTypeId == 5 || leaveRequestToUpdate.LeaveTypeId == 6)
                        {
                            if (leaveRequestToUpdate.LeaveTypeId == 5)
                            {
                                var UpdateMaritalStatus = db.Users.FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                                UpdateMaritalStatus.MaritalStatus = 2; /* 1-Married 2-UnMarried*/
                                db.SaveChanges();
                            }
                            var leaveBalanceCountToUpdate = (from leavebalcount in db.LeaveBalanceCounts
                                                             where leavebalcount.UserId == leaveRequestToUpdate.UserId &&
                                                             leavebalcount.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId
                                                             select leavebalcount).FirstOrDefault();
                            if (leaveBalanceCountToUpdate != null)
                            {
                                leaveBalanceCountToUpdate.Value -= leaveRequestToUpdate.LeaveDays;
                                db.SaveChanges();
                            }
                        }

                        if (leaveRequestToUpdate.LeaveTypeId != 6)
                        {
                            LeaveController.UpdateLeaveBalanceReject(leaveRequestToUpdate);
                        }

                        //Mail To HR For Intimation Purpose      
                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Rejection After Approval").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Rejection After Approval").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {

                            var objLeaveRejectedApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Rejection After Approval")
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
                            string TemplatePathobjLeaveRejectedApproved = Server.MapPath(objLeaveRejectedApproved.Template);
                            string htmlLeaveRejectedApproved = System.IO.File.ReadAllText(TemplatePathobjLeaveRejectedApproved);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#HR", "Umapathy V");
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EmployeeName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LeaveTypeName", LeaveTyepName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#StartDateTime", StartTime);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EndDateTime", EndTime);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#CompanyName", company);
                            if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                            {
                                //FULL RED #FF0000
                                string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", LOPDays);

                                string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", LOP);
                            }
                            else
                            {
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", "");
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", "");
                            }

                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ServerName",ServerName);

                            string EmailAddress = "";

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

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRejectedApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });

                            }
                            else
                            {
                                int HRID = Convert.ToInt32(objLeaveRejectedApproved.To);
                                EmailAddress = db.Users.FirstOrDefault(o => o.UserID == HRID).EmailAddress;

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject, null, htmlLeaveRejectedApproved, "admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("Leave Rejection After Approval", folder, ServerName);
                        }

                    }
                    leaveRequestToUpdate.LeaveStatusId = 3;
                    leaveRequestToUpdate.ProcessedBy = leaveRequestToUpdate.ReportingTo;
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    db.SaveChanges();


                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Request Rejected").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((checks != null) && (checks != 0))
                    {
                        var objLeaveRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Rejected")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();
                        var objcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                        string TemplatePathLeaveRequestRejected = Server.MapPath(objLeaveRequestRejected.Template);
                        string htmlLeaveRequestRejected = System.IO.File.ReadAllText(TemplatePathLeaveRequestRejected);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LeaveTypeName", LeaveTyepName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#StartDateTime", StartTime);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#EndDateTime", EndTime);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#Comments", leaveRequestToUpdate.Comments);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#CompanyName", objcompany);
                        if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                        {
                            //FULL RED #FF0000
                            string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", LOPDays);

                            string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", LOP);
                        }
                        else
                        {
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", "");
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", "");
                        }

                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ServerName",ServerName);

                        //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        // var logo = CommonLogic.getLogoPath();

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

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {


                            // string mailMessage = MailBuilder.LeaveRequestRejected(leaveRequestToUpdate.User1.FirstName, leaveRequestToUpdate.User1.LastName, leaveRequestToUpdate.User.FirstName, leaveRequestToUpdate.User.LastName, leaveRequestToUpdate.Comments);
                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                // DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject, null, htmlLeaveRequestRejected, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject, null, htmlLeaveRequestRejected, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath(logo.ToString()));
                                //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Leave request has been rejected", null, mailMessage, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath("~/Content/Template/images/logo.png"));
                            });
                        }
                    }
                    else
                    {

                        ExceptionHandlingController.TemplateMissing("Leave Request Rejected", folders, ServerName);
                    }
                }
                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult CommentLeaveRequest(String RequestID)
        {
            try
            {

                Session["ServerName"] = AppValue.GetFromMailAddress("ServerName");
                var Id = Convert.ToInt32(Encrypter.Decode(RequestID));
                ViewBag.Id = Id;
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestToUpdate = db.LeaveRequests.Where(o => o.LeaveRequestId == Id).Select(o => o).FirstOrDefault();
                ViewBag.IsCanceled = leaveRequestToUpdate.LeaveStatusId == 4 ? true : false;
                ViewBag.IsAlreadycommented = (leaveRequestToUpdate.LeaveStatusId != 1) ? true : false;
                return View();
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
        public ActionResult CommentLeaveRequest(int RequestID, string Comments, bool IsAccepted)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var leaveRequestToUpdate = db.LeaveRequests.Where(o => o.LeaveRequestId == RequestID).Select(o => o).FirstOrDefault();

                ViewBag.Id = RequestID;

                string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).Name;
                string StartTime = Convert.ToDateTime(leaveRequestToUpdate.StartDateTime).ToString("ddd, MMM d, yyyy");
                string EndTime = Convert.ToDateTime(leaveRequestToUpdate.EndDateTime).ToString("ddd, MMM d, yyyy");

                if (IsAccepted)
                {
                    double LOPs = 0.0;

                    if (leaveRequestToUpdate.LeaveTypeId != 4 && leaveRequestToUpdate.LeaveTypeId != 6)
                    {
                        GetLOPDays(RequestID);
                        LOPs = Convert.ToDouble(TempData["LOP"]);
                    }

                    leaveRequestToUpdate.LeaveStatusId = 2;
                    leaveRequestToUpdate.ProcessedBy = leaveRequestToUpdate.ReportingTo;
                    leaveRequestToUpdate.Comments = Comments;
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    leaveRequestToUpdate.LOP = LOPs;
                    db.SaveChanges();

                    ////If Marriage Leave is Applied and Approved Marital Status has to be updated in Users Table
                    if (leaveRequestToUpdate.LeaveTypeId == 5)
                    {
                        var UpdateMaritalStatus = db.Users.FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                        UpdateMaritalStatus.MaritalStatus = 1; /* 1-Married 2-UnMarried*/
                        db.SaveChanges();
                    }

                    var FromDate = leaveRequestToUpdate.StartDateTime;
                    var ToDate = leaveRequestToUpdate.EndDateTime;
                    var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                    bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                    if (leaveRequestToUpdate.LeaveTypeId == 5 || leaveRequestToUpdate.LeaveTypeId == 6)
                    {
                        var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                                  where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                                      leavebalance.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId
                                                  select leavebalance).FirstOrDefault();
                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = leaveRequestToUpdate.LeaveTypeId;
                            updateleavebalance.Value = leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();
                        }
                        else
                        {
                            updateleavebalance.Value = updateleavebalance.Value + leaveRequestToUpdate.LeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();
                        }
                    }

                    LeaveController.UpdateLeaveBalance(leaveRequestToUpdate);
                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objLeaveRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Approved")
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
                        string TemplatePathLeaveRequestApproved = Server.MapPath(objLeaveRequestApproved.Template);
                        string htmlLeaveRequestApproved = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LeaveTypeName", LeaveTyepName);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#StartDateTime", StartTime);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#EndDateTime", EndTime);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#CompanyName", company);
                        if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                        {
                            //FULL RED #FF0000
                            string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", LOPDays);


                            string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request has to be considered as LOP.</p>";
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", LOP);
                        }
                        else
                        {
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOPDays", "");
                            htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#LOP", "");
                        }
                        htmlLeaveRequestApproved = htmlLeaveRequestApproved.Replace("#ServerName",ServerName);
                        string mailMessage = MailBuilder.LeaveRequestApproved(leaveRequestToUpdate.User1.FirstName, leaveRequestToUpdate.User1.LastName, leaveRequestToUpdate.User.FirstName, leaveRequestToUpdate.User.LastName, leaveRequestToUpdate.Comments);
                        string n1 = leaveRequestToUpdate.User1.EmailAddress;

                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            });

                        }
                        else
                        {

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlLeaveRequestApproved, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, objLeaveRequestApproved.CC, "", Server.MapPath(logo.AppValue.ToString()));

                            });
                        }
                    }
                    else
                    {
                       // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Leave Request Approved", folder, ServerName);
                    }
                }
                else
                {
                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    if (leaveRequestToUpdate.LeaveStatusId == 2 && leaveRequestToUpdate.LeaveTypeId != 4)
                    {
                        if (leaveRequestToUpdate.LeaveTypeId == 5 || leaveRequestToUpdate.LeaveTypeId == 6)
                        {
                            if (leaveRequestToUpdate.LeaveTypeId == 5)
                            {
                                var UpdateMaritalStatus = db.Users.FirstOrDefault(o => o.UserID == leaveRequestToUpdate.UserId);
                                UpdateMaritalStatus.MaritalStatus = 2; /* 1-Married 2-UnMarried*/
                                db.SaveChanges();
                            }

                            //var FromDate = leaveRequestToUpdate.StartDateTime;
                            //var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;

                            var leaveBalanceCountToUpdate = (from leavebalcount in db.LeaveBalanceCounts
                                                             where leavebalcount.UserId == leaveRequestToUpdate.UserId &&
                                                             leavebalcount.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId //&&
                                                             //leavebalcount.Year == year
                                                             select leavebalcount).FirstOrDefault();
                            if (leaveBalanceCountToUpdate != null)
                            {
                                leaveBalanceCountToUpdate.Value -= leaveRequestToUpdate.LeaveDays;
                                db.SaveChanges();
                            }
                        }

                        if (leaveRequestToUpdate.LeaveTypeId != 6)
                        {
                            LeaveController.UpdateLeaveBalanceReject(leaveRequestToUpdate);
                        }


                        //Mail To HR For Intimation Purpose

                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Rejection After Approval").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Rejection After Approval").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objLeaveRejectedApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Rejection After Approval")
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
                            string TemplatePathobjLeaveRejectedApproved = Server.MapPath(objLeaveRejectedApproved.Template);
                            string htmlLeaveRejectedApproved = System.IO.File.ReadAllText(TemplatePathobjLeaveRejectedApproved);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#HR", "Umapathy V");
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EmployeeName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LeaveTypeName", LeaveTyepName);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#StartDateTime", StartTime);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#EndDateTime", EndTime);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#Comments", leaveRequestToUpdate.Comments);
                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#CompanyName", company);
                            if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                            {
                                //FULL RED #FF0000
                                string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", LOPDays);

                                string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", LOP);
                            }
                            else
                            {
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOPDays", "");
                                htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#LOP", "");
                            }

                            htmlLeaveRejectedApproved = htmlLeaveRejectedApproved.Replace("#ServerName", ServerName);

                            string EmailAddress = "";

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

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject + " - Test Mail Please Ignore", null, htmlLeaveRejectedApproved + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });

                            }
                            else
                            {
                                int HRID = Convert.ToInt32(objLeaveRejectedApproved.To);
                                EmailAddress = db.Users.FirstOrDefault(o => o.UserID == HRID).EmailAddress;

                                Task.Factory.StartNew(() =>
                                {
                                    var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    DsrcMailSystem.MailSender.SendMail(null, objLeaveRejectedApproved.Subject, null, htmlLeaveRejectedApproved, "admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                });
                            }
                        }
                        else
                        {

                            ExceptionHandlingController.TemplateMissing("Leave Rejection After Approval", folder, ServerName);
                        }

                    }



                    leaveRequestToUpdate.LeaveStatusId = 3;
                    leaveRequestToUpdate.Comments = Comments;
                    leaveRequestToUpdate.ProcessedBy = leaveRequestToUpdate.ReportingTo;
                    leaveRequestToUpdate.ProcessedOn = DateTime.Now;
                    db.SaveChanges();

                    var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Rejected").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Request Rejected").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((checks != null) && (checks != 0))
                    {

                        var objLeaveRequestRejected = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Rejected")
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
                        string TemplatePathLeaveRequestRejected = Server.MapPath(objLeaveRequestRejected.Template);
                        string htmlLeaveRequestRejected = System.IO.File.ReadAllText(TemplatePathLeaveRequestRejected);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#UserName", leaveRequestToUpdate.User1.FirstName + " " + leaveRequestToUpdate.User1.LastName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ManagerName", leaveRequestToUpdate.User.FirstName + " " + leaveRequestToUpdate.User.LastName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LeaveTypeName", LeaveTyepName);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#StartDateTime", StartTime);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#EndDateTime", EndTime);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#totalLeaveDays", leaveRequestToUpdate.LeaveDays.ToString());
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#Comments", leaveRequestToUpdate.Comments);
                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#CompanyName", objcom);
                        if (leaveRequestToUpdate.LOP > 0 && (leaveRequestToUpdate.LeaveTypeId != 4 || leaveRequestToUpdate.LeaveTypeId != 6))
                        {
                            //FULL RED #FF0000
                            string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + leaveRequestToUpdate.LOP + "</label></p>";
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", LOPDays);

                            string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request had been considered as LOP.</p>";
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", LOP);
                        }
                        else
                        {
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOPDays", "");
                            htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#LOP", "");
                        }

                        htmlLeaveRequestRejected = htmlLeaveRequestRejected.Replace("#ServerName", ServerName);

                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

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

                            string EmailAddress = "";

                            foreach (string mail in MailIds)
                            {
                                EmailAddress += mail + ",";
                            }

                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequestRejected + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                            });

                        }
                        else
                        {
                            //string mailMessage = MailBuilder.LeaveRequestRejected(leaveRequestToUpdate.User1.FirstName, leaveRequestToUpdate.User1.LastName, leaveRequestToUpdate.User.FirstName, leaveRequestToUpdate.User.LastName , leaveRequestToUpdate.Comments);
                            Task.Factory.StartNew(() =>
                            {
                                var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestRejected.Subject, null, htmlLeaveRequestRejected, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath(logo.AppValue.ToString()));
                                //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Leave request has been rejected", null, mailMessage, "admin@dsrc.co.in", leaveRequestToUpdate.User1.EmailAddress, Server.MapPath("~/Content/Template/images/logo.png"));
                            });
                        }
                    }
                    else
                    {

                        ExceptionHandlingController.TemplateMissing("Leave Request Rejected", folders, ServerName);
                    }
                }
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
        public ActionResult SuccessfullyApproved()
        {
            return View();
        }

        public ActionResult SuccessfullyRejected()
        {
            return View();
        }


        public double? totalTime { get; set; }

        [HttpGet]
        public JsonResult ClearTempSession()
        {
            Session["PaginationNumber"] = null;

            return Json(1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Default()
        {
            return View();
        }

        //[HttpGet]
        //public ActionResult wcfApprove(string RequestID)
        //{
        //    int ID = Convert.ToInt32(Encrypter.Decode(RequestID));
        //    bool IsalreadyApproved;
        //    bool IsalreadyRejected;
        //    bool Canceled;
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    ServiceLeaveApprovalClient approve = new ServiceLeaveApprovalClient();

        //    var AlreadyApproved = db.LeaveRequests.Where(x => x.LeaveStatusId == 2 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();
        //    if (AlreadyApproved != null)
        //    {

        //        IsalreadyApproved = true;
        //        ViewBag.IsalreadyApproved = true;
        //        ApproveLeaveRequest(RequestID);
        //    }
        //    else
        //    {
        //        IsalreadyApproved = false;
        //        ViewBag.IsalreadyApproved = false;
        //    }

        //    var AlreadyRejected = db.LeaveRequests.Where(x => x.LeaveStatusId == 3 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();
        //    if (AlreadyRejected != null)
        //    {
        //        IsalreadyRejected = true;
        //        ViewBag.IsalreadyRejected = true;
        //        RejectLeaveRequest(RequestID);
        //    }
        //    else
        //    {
        //        ViewBag.IsalreadyRejected = false;
        //        IsalreadyRejected = false;
        //    }

        //    var IsCanceled = db.LeaveRequests.Where(x => x.LeaveStatusId == 4 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();

        //    if (IsCanceled != null)
        //    {
        //        ViewBag.IsCanceled = true;
        //        Canceled = true;

        //    }
        //    else
        //    {
        //        Canceled = false;
        //        ViewBag.IsCanceled = false;
        //    }

        //    if (IsalreadyApproved == false && Canceled == false && IsalreadyRejected == false)
        //    {
        //        approve.Open();
        //        return View(approve.ApproveLeaveRequest(RequestID));

        //    }
        //    approve.Close();
        //    return View();
        //}

        //[HttpGet]
        //public ActionResult wcfReject(string RequestID)
        //{
        //    int ID = Convert.ToInt32(Encrypter.Decode(RequestID));
        //    bool IsalreadyApproved;
        //    bool IsalreadyRejected;
        //    bool Canceled;
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        //    ServiceLeaveApprovalClient approve = new ServiceLeaveApprovalClient();

        //    var AlreadyApproved = db.LeaveRequests.Where(x => x.LeaveStatusId == 2 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();
        //    if (AlreadyApproved != null)
        //    {

        //        IsalreadyApproved = false;
        //        ViewBag.IsalreadyApproved = false;

        //    }
        //    else
        //    {
        //        IsalreadyApproved = false;
        //        ViewBag.IsalreadyApproved = false;
        //    }

        //    var AlreadyRejected = db.LeaveRequests.Where(x => x.LeaveStatusId == 3 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();
        //    if (AlreadyRejected != null)
        //    {
        //        IsalreadyRejected = true;
        //        ViewBag.IsalreadyRejected = true;

        //    }
        //    else
        //    {
        //        ViewBag.IsalreadyRejected = false;
        //        IsalreadyRejected = false;
        //    }

        //    var IsCanceled = db.LeaveRequests.Where(x => x.LeaveStatusId == 4 && x.LeaveRequestId == ID).Select(o => o).FirstOrDefault();

        //    if (IsCanceled != null)
        //    {
        //        ViewBag.IsCanceled = true;
        //        Canceled = true;

        //    }
        //    else
        //    {
        //        Canceled = false;
        //        ViewBag.IsCanceled = false;
        //    }

        //    if (IsalreadyApproved == false && Canceled == false && IsalreadyRejected == false)
        //    {
        //        approve.Open();
        //        return View(approve.RejectLeaveRequest(RequestID));

        //    }
        //    approve.Close();
        //    return View();


        //}






        public ActionResult GetLOPDays(int leaveRequestId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {


                LeaveModel LOPDetails = new LeaveModel();

                var leaveDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestId);

                var LeaveTypeList = new List<LeaveBalance>();

                LOPDetails.LOPdays = 0.0;

                DateTime StartDateTime = Convert.ToDateTime(leaveDetails.StartDateTime);
                DateTime EndDateTime = Convert.ToDateTime(leaveDetails.EndDateTime);

                StartDateTime = StartDateTime.AddHours(9);
                EndDateTime = EndDateTime.AddHours(18);

                if (leaveDetails.LeaveTypeId != 4 || leaveDetails.LeaveTypeId != 6)
                {
                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                    bool isEligible;
                    List<int> EligibleLeaveTypes = new List<int>();

                    var doj = db.Users.FirstOrDefault(item => item.UserID == leaveDetails.UserId).DateOfJoin;

                    if (doj == null)
                        isEligible = false;
                    else
                    {
                        var completedDays = (DateTime.Now - doj).Value.Days;
                        if (completedDays < 365)
                            isEligible = false;
                        else
                            isEligible = true;
                    }

                    EligibleLeaveTypes.Add(1);
                    EligibleLeaveTypes.Add(2);

                    if (isEligible)
                        EligibleLeaveTypes.Add(3);

                    LeaveTypeList = (from typ in db.LeaveTypes
                                     join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetails.UserId)
                                     on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                     from data in leftjoin.DefaultIfEmpty()
                                     select new LeaveBalance
                                     {
                                         LeaveTypeId = typ.LeaveTypeId,
                                         DaysAllowed = typ.DaysAllowed.Value,
                                         LeaveType = typ.Name,
                                         UsedDays = data.Value ?? 0,
                                         CalculateLeave = true
                                     }).ToList();

                    LOPDetails.TotalAvailDays = LeaveTypeList.Where(o => EligibleLeaveTypes.Contains(o.LeaveTypeId) && o.RemainingDays > 0).Sum(o => o.RemainingDays);
                    LOPDetails.totalLeaveDays = 0.0;

                    var leaveTypes = new DSRCManagementSystemEntities1().LeaveTypes.ToList();

                    var UserRegion = db.Users.Where(x => x.UserID == leaveDetails.UserId).Select(o => o.Region).FirstOrDefault();

                    var holidayList = db.AddHolidays.Where(holiday => holiday.Date >= StartDateTime.Date && holiday.Date <= EndDateTime.Date && holiday.ZoneId == UserRegion && holiday.Isactive == true).Select(item => item.Date).ToList();

                    //if (leaveTypes.First(type => type.LeaveTypeId == leaveTypeId).DaysAllowed != 0 && !(leaveRequest.HalfDay))
                    //{
                    LOPDetails.totalLeaveDays = new LeaveBalance().CalculateLeaveDays(StartDateTime, EndDateTime, holidayList).LeaveDays;
                    //}

                    if (LOPDetails.TotalAvailDays <= 0)
                    {
                        LOPDetails.LOPdays = LOPDetails.totalLeaveDays;
                    }
                    else if (LOPDetails.totalLeaveDays > LOPDetails.TotalAvailDays)
                    {
                        LOPDetails.LOPdays = LOPDetails.totalLeaveDays - LOPDetails.TotalAvailDays;
                    }
                }

                TempData["LOP"] = LOPDetails.LOPdays;
                //Session["LOP"] = LOPDetails.LOPdays;

                return Json(new { Result = LOPDetails.LOPdays }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
            // return LOPDetails;
        }
        public ActionResult ChartAttendance()
        {
            List<object> Persentage = new List<object>();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int LoggedInId = (int)Session["UserId"];
            //var today = DateTime.Today;
            //var month = new DateTime(today.Year, today.Month, 1);
            //var first = month.AddMonths(-1);
            //var last = month.AddDays(-1);
            //int year = month.Year;
            //int mon = first.Month;
            //int Fday = first.Day;
            //int Lday = last.Day;

            //var EmpId = (from e in objdb.Users
            //             join u in objdb.TimeManagements on e.EmpID equals u.EmpID
            //             where e.IsActive == true && e.UserID == LoggedInId
            //             select (u.EmpID)).FirstOrDefault();

            //var Prasent = (from t in objdb.TimeManagements.Where(x => x.Date >= new DateTime(year, mon, Fday) && x.Date <= new DateTime(year, mon, Lday) && x.EmpID == EmpId) select (t.EmpID));
            //int PreCount = Prasent.Count();
            //var Obsent = (from l in objdb.LeaveRequests.Where(x => x.StartDateTime >= new DateTime(year, mon, Fday) && x.EndDateTime <= new DateTime(year, mon, Lday) && x.UserId == LoggedInId) select (l.UserId));
            //int ObCount = Obsent.Count();
            //Persentage.Add(PreCount);
            //Persentage.Add(ObCount);
            //return Json(Persentage, JsonRequestBehavior.AllowGet);

            var UserDetails = objdb.Users.FirstOrDefault(u => u.UserID == LoggedInId);
            var employeeId = UserDetails.EmpID;
            var BranchId = UserDetails.BranchId;
            var today = DateTime.Today;
            var CurMonth = today.Month;
            var Curyear = today.Year;

            Curyear = CurMonth == 1 ? Curyear - 1 : Curyear;
            CurMonth = CurMonth == 1 ? 12 : CurMonth - 1;

            var startDate = new DateTime(Curyear, CurMonth, 01);
            var endDate = new DateTime(Curyear, CurMonth, 01).AddMonths(1).AddSeconds(-1);
            var holidaysCount = 0;

            foreach (var item in objdb.Master_holiday.Where(holiday => holiday.Date >= startDate && holiday.Date <= endDate))
            {
                if (item.Date != null)
                {
                    var tempDate = ((DateTime)item.Date);
                    if (tempDate.DayOfWeek != DayOfWeek.Saturday && tempDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        holidaysCount++;
                    }
                }
            }
            double calcBusinessDays = 1 + ((endDate - startDate).TotalDays * 5 - (startDate.DayOfWeek - endDate.DayOfWeek) * 2) / 7;

            if ((int)endDate.DayOfWeek == 6) calcBusinessDays--;
            if ((int)startDate.DayOfWeek == 0) calcBusinessDays--;
            calcBusinessDays -= holidaysCount;
            double WorkingHoursInMonth = Math.Floor(calcBusinessDays) * 8;
            var totalDaysWorked =
                objdb.TimeManagements.Where(
                    t =>
                        t.EmpID == employeeId && t.BranchId == BranchId && t.Date >= startDate && t.Date <= endDate).ToList();

            var totalHoursWorked = totalDaysWorked.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).Sum(tm => tm.TotalTime);
            var workingHour = Math.Floor(TimeSpan.FromMinutes(totalHoursWorked ?? 0).TotalHours);
            var blanceMinutes = (totalHoursWorked % 60) / 100.0;
            //ViewBag.TotalHoursWorked = Math.Round((double)(workingHour + (blanceMinutes ?? 0)) - totalDaysWorked.Count(), 2);
            double Prasent = Math.Round((double)(workingHour + (blanceMinutes ?? 0)), 2);
            double Absent = WorkingHoursInMonth - Prasent;
            double PrasentPercentage = (Prasent / WorkingHoursInMonth) * 100;
            double AbsentPercentage = (Absent / WorkingHoursInMonth) * 100;
            Persentage.Add(PrasentPercentage);
            Persentage.Add(AbsentPercentage);
            return Json(Persentage, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ChartData()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List = new List<object>();
            try
            {
                var male = db.Users.Where(r => r.IsActive == true && (int)r.Gender == 1 && (int)r.UserID != 6).GroupBy(r => new { r.UserID, r.UserName }).Count();

                var Female = db.Users.Where(r => r.IsActive == true && (int)r.Gender == 2 && (int)r.UserID != 6).GroupBy(r => new { r.UserID, r.UserName }).Count();

                var Val = new { m = male, a = Female };
                List.Add(male);
                List.Add(Female);


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DepartmentPiechart1()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List = new List<object>();
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                var Department = db.Departments.Where(x => x.IsActive == true).Select(o => o.DepartmentId).ToList();

                foreach (var DPList in Department)
                {
                    var users = db.Users.Where(x => x.IsActive == true && x.UserStatus != 6 && x.DepartmentId == DPList).Select(o => o.UserID).Count();
                    var Val = new { m = DPList, a = users };
                    List.Add(users);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DepartmentPiechart2()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List1 = new List<object>();
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                var Department = db.Departments.Where(x => x.IsActive == true).Select(o => o.DepartmentId).ToList();

                foreach (var DPList in Department)
                {
                    var users = db.Users.Where(x => x.IsActive == true && x.UserStatus != 6 && x.DepartmentId == DPList).Select(o => o.UserID).Count();
                    var getDepartmentname = db.Departments.Where(x => x.DepartmentId == DPList).Select(o => o.DepartmentName).FirstOrDefault();
                    var Val = new { m = DPList, a = users };
                    List1.Add(getDepartmentname);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List1, JsonRequestBehavior.AllowGet);

        }


        public ActionResult DepartmentPiechart3()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List2 = new List<object>();
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                var Department = db.Departments.Where(x => x.IsActive == true ).Select(o => o.DepartmentId).ToList();
                Random rdm = new Random();
                string hexValue = string.Empty;
                int num;

                for (int i = 0; i < Department.Count; i++)
                {
                    num = rdm.Next(0, int.MaxValue);
                    hexValue = num.ToString("X6");
                    var x = hexValue.Substring(hexValue.Length - 6);
                    List2.Add("#" + x);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List2, JsonRequestBehavior.AllowGet);

        }



        private void GetMenuIcon(Dashboard _dashboard)
        {
            int userId = int.Parse(Session["UserID"].ToString());

            _dashboard.Pages = GetPages(userId);

        }

        public static List<DSRCManagementSystem.Models.PageUrl> GetPages(int userid)
        {
            List<DSRCManagementSystem.Models.PageUrl> Pages = new List<DSRCManagementSystem.Models.PageUrl>();
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    Pages = (from p in db.AssignQuickLinks
                             join a in db.ManageQuickLinks on p.QuickLinkID equals a.QuickLinkID
                             join mod in db.Modules on a.PageModuleID equals mod.PageModuleID
                             join q in db.ModulePages on a.PageModuleID equals q.PageModuleID
                             join m in db.Pages on q.PageId equals m.PageID
                             where p.IsActive == true && a.PageModuleID != null && p.UserID == userid && a.IsActive == true
                             && a.UserID == userid
                             orderby a.PageModuleID ascending
                             select new DSRCManagementSystem.Models.PageUrl
                             {
                                 PageModuleId = a.PageModuleID,
                                 URL = m.PageURL,
                                 ModuleName = a.DisplayName,
                                 path = a.IconPath

                             }).ToList();

                }
            }
            catch (Exception Ex)
            {
                string actionName = null;
                string controllerName = null;
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Pages;
        }
        [HttpGet]
        public ActionResult HoursChart()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List = new List<object>();

            Session["ChartValue"] = "1";
            try
            {
                var userId = (int)Session["UserId"];
                var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();
                var CurrentMonth = DateTime.Now.Month;
                var CurrentYear = DateTime.Now.Year;
                
                
                    var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == CurrentMonth && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().OrderBy(o => o).ToList();
                
                    foreach (var DATE in GetDate)
                {
                    

                    var TimeManagement = db.TimeManagements.Where(x => x.BranchId == BranchId  && x.Date == DATE).Select(o => o.TotalTime).ToList();
                    List<object> GREEN = new List<object>();
                    List<object> ORANGE = new List<object>();
                    List<object> RED = new List<object>();

                    foreach (var TIME in TimeManagement)
                    {

                        var HOURS = TIME / 60;
                        if (HOURS >= 8)
                        {
                            GREEN.Add(HOURS);
                        }
                        if (HOURS >= 7 && HOURS < 8)
                        {
                            ORANGE.Add(HOURS);
                        }
                        if (HOURS < 7)
                        {
                            RED.Add(HOURS);

                        }
                    }

                    var Val = new { Date1 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Green = GREEN.Count, Orange = ORANGE.Count, Red = RED.Count };
                    List.Add(Val);
                                      
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HoursChart(string value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List1 = new List<object>();
            try
            {
                var userId = (int)Session["UserId"];
                var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();
                var CurrentMonth = DateTime.Now.Month;
                var CurrentYear = DateTime.Now.Year;

                if (value == "0")
                {
                    var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == CurrentMonth && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().OrderBy(o => o).ToList();

                    foreach (var DATE in GetDate)
                    {


                        var TimeManagement = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE).Select(o => o.TotalTime).ToList();
                        List<object> GREEN = new List<object>();
                        List<object> ORANGE = new List<object>();
                        List<object> RED = new List<object>();

                        foreach (var TIME in TimeManagement)
                        {

                            var HOURS = TIME / 60;
                            if (HOURS >= 8)
                            {
                                GREEN.Add(HOURS);
                            }
                            if (HOURS >= 7 && HOURS < 8)
                            {
                                ORANGE.Add(HOURS);
                            }
                            if (HOURS < 7)
                            {
                                RED.Add(HOURS);

                            }
                        }

                        var Val = new { Date3 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Green = GREEN.Count, Orange = ORANGE.Count, Red = RED.Count };
                        List1.Add(Val);
                    }
                }
                else
                {
                    if (value != "" && value != null)
                    {
                        int Values = Convert.ToInt32(value);
                        var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == Values && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().OrderBy(o => o).ToList();
                        foreach (var DATE in GetDate)
                        {


                            var TimeManagement = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE).Select(o => o.TotalTime).ToList();
                            List<object> GREEN = new List<object>();
                            List<object> ORANGE = new List<object>();
                            List<object> RED = new List<object>();

                            foreach (var TIME in TimeManagement)
                            {

                                var HOURS = TIME / 60;
                                if (HOURS >= 8)
                                {
                                    GREEN.Add(HOURS);
                                }
                                if (HOURS >= 7 && HOURS < 8)
                                {
                                    ORANGE.Add(HOURS);
                                }
                                if (HOURS < 7)
                                {
                                    RED.Add(HOURS);

                                }
                            }

                            var Val = new { Date3 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Green = GREEN.Count, Orange = ORANGE.Count, Red = RED.Count };
                            List1.Add(Val);
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
            return Json(List1, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AttendanceBarchart()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List = new List<object>();
            try
            {
                var CurrentMonth = DateTime.Now.Month;
                var CurrentYear = DateTime.Now.Year;

                var userId = (int)Session["UserId"];
                var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();
               // var users = db.Users.Where(x => x.UserID != null && x.UserID == userId).Select(e => e.EmpID).ToList();

                var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == CurrentMonth && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().ToList();
                foreach (var DATE in GetDate)
                {
                    var GetAllUsers = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE).Select(o => o.EmpID).Count();
                    var CAME = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE && x.TotalTime != 0).Select(o => o.EmpID).Count();
                    var NOTCAME = GetAllUsers - CAME;
                    var Val = new { Date2 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Came = CAME, NotCame = NOTCAME };
                    List.Add(Val);
                }


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AttendanceBarchart(string value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<object> List = new List<object>();
            try
            {
                var CurrentMonth = DateTime.Now.Month;
                var CurrentYear = DateTime.Now.Year;

                var userId = (int)Session["UserId"];
                var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();
                // var users = db.Users.Where(x => x.UserID != null && x.UserID == userId).Select(e => e.EmpID).ToList();
                if (value == "0")
                {
                    var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == CurrentMonth && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().ToList();
                    foreach (var DATE in GetDate)
                    {
                        var GetAllUsers = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE).Select(o => o.EmpID).Count();
                        var CAME = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE && x.TotalTime != 0).Select(o => o.EmpID).Count();
                        var NOTCAME = GetAllUsers - CAME;
                        var Val = new { Date4 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Came = CAME, NotCame = NOTCAME };
                        List.Add(Val);
                    }

                }
                else
                {
                    if (value != "" && value != null)
                    {
                        int Values = Convert.ToInt32(value);
                        var GetDate = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date.Month == Values && x.Date.Year == CurrentYear).Select(o => o.Date).Distinct().ToList();
                        foreach (var DATE in GetDate)
                        {
                            var GetAllUsers = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE).Select(o => o.EmpID).Count();
                            var CAME = db.TimeManagements.Where(x => x.BranchId == BranchId && x.Date == DATE && x.TotalTime != 0).Select(o => o.EmpID).Count();
                            var NOTCAME = GetAllUsers - CAME;
                            var Val = new { Date4 = DATE.Day + "-" + DATE.Month + "-" + DATE.Year, Came = CAME, NotCame = NOTCAME };
                            List.Add(Val);
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
            return Json(List, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ManageTab()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var objmodel = new List<ManageTabs>();
            DSRCManagementSystem.Models.ManageTabs objvalue = new DSRCManagementSystem.Models.ManageTabs();
            int userId = int.Parse(Session["UserID"].ToString());
            try
            {
                
                objvalue.TaskName = (from t in db.Master_Tab
                                     join m in db.ManageTabs on t.TabID equals m.TabID
                                     where t.IsActive == true && m.UserID == userId && m.IsActive == true

                                     select new DSRCManagementSystem.Models.values
                                     {
                                         TabName = t.TabName,
                                         TabId = t.TabID
                                     }).ToList();

                objmodel = (from t in db.Master_Tab
                            join m in db.ManageTabs on t.TabID equals m.TabID
                            where t.IsActive == true && m.UserID == userId && m.IsActive == true
                            select new ManageTabs
                            {
                                TabName = t.TabName,
                                TabId = t.TabID,
                            }).ToList();


                int? k = objmodel.Count();

                DSRCManagementSystem.Models.ManageTabs obj = new DSRCManagementSystem.Models.ManageTabs();

                obj.Nofcount = k;


                foreach (var item in objvalue.TaskName)
                {
                    var Assignedlist = db.ManageTabs.Where(x => x.TabID == item.TabId && x.UserID == userId && x.UserSelected == true).Select(x => x.TabID).ToList();
                    int Count = Assignedlist.Count;
                    if (Count != 0)
                    {
                        item.IsChecked = true;
                        item.Nofcount = k;
                    }
                    else
                    {
                        item.IsChecked = false;
                        item.Nofcount = k;
                    }

                }
            }


            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objvalue);
        }
        [HttpPost]
        public ActionResult ManageTab(string TabIds)
        {
            string ManageTabs = TabIds.Trim(new Char[] { ' ', ',' });
             DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = int.Parse(Session["UserID"].ToString());
                Session["TabAssigned"] = null;
            try
            {
                List<int?> Tabs = new List<int?>();
                if (TabIds != "")
                {

                    string[] value = ManageTabs.Split(',');
                    for (int i = 0; i < value.Count(); i++)
                    {
                        var item=Convert.ToInt32(value[i]);
                        var ManTabs = db.ManageTabs.Where(q => q.TabID == item && q.UserID == userId).Select(r => r).FirstOrDefault();

                        ManTabs.UserSelected = true;
                        Tabs.Add(Convert.ToInt32(value[i]));

                    }
                }

               

                var TotalGrids = db.ManageTabs.Where(q => q.UserID ==userId && q.IsActive ==true ).Select(x => x.TabID).ToList();
                var UnSelected = TotalGrids.Except(Tabs).ToList();

                foreach(var item in UnSelected )
                {

                    var ManTabGrid = db.ManageTabs.Where(q => q.TabID == item && q.UserID == userId).Select(r => r).FirstOrDefault();

                     ManTabGrid.UserSelected = false ;
                }
                    db.SaveChanges();

                
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
            // return RedirectToAction("Index","Home");
        }
        [HttpGet]
        public ActionResult ManageWidget()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var objmodel = new List<ManageTabs>();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystem.Models.ManageTabs objvalue = new DSRCManagementSystem.Models.ManageTabs();
            try
            {
                objvalue.TaskName = (from t in db.Master_Tab_Grids
                                     join m in db.ManageTabGrids on t.GridID equals m.GridID
                                     where t.IsActive == true && m.UserID == userId
                                     select new DSRCManagementSystem.Models.values
                                     {
                                         GridName = t.GridName,
                                         GridId = t.GridID
                                     }).OrderBy(o => o.GridId).ToList();

                objmodel = (from t in db.Master_Tab_Grids
                            join m in db.ManageTabGrids on t.GridID equals m.GridID
                            where t.IsActive == true && m.UserID == userId

                            select new ManageTabs
                            {
                                GridName = t.GridName,
                                GridId = t.GridID
                            }).ToList();


                int? k = objmodel.Count();

                DSRCManagementSystem.Models.ManageTabs obj = new DSRCManagementSystem.Models.ManageTabs();

                obj.Nofcount = k;


                foreach (var item in objvalue.TaskName)
                {
                    var Assignedlist = db.ManageTabGrids.Where(x => x.GridID == item.GridId && x.UserID == userId && x.UserSelected == true).Select(x => x.GridID).ToList();
                    int Count = Assignedlist.Count;
                    if (Count != 0)
                    {
                        item.IsChecked = true;
                        item.Nofcount = k;
                    }
                    else
                    {
                        item.IsChecked = false;
                        item.Nofcount = k;
                    }

                }
            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objvalue);
        }
        [HttpPost]
        public ActionResult ManageWidget(string GridIds)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = int.Parse(Session["UserID"].ToString());
          
            try
            {

                if (GridIds == null || GridIds == "")
                {
                    var TotalGrids = db.ManageTabGrids.Where(q => q.UserID == userId && q.IsActive == true).Select(x => x.GridID).ToList();
                    foreach (var item in TotalGrids)
                    {
                        var ManTabGrid = db.ManageTabGrids.Where(q => q.GridID == item && q.UserID == userId).Select(r => r).FirstOrDefault();

                        ManTabGrid.UserSelected = false;
                    }
                    db.SaveChanges();
                }
                else
                {
                    string Widgets = GridIds.Trim(new Char[] { ' ', ',' });
                    List<int?> Grids = new List<int?>();

                    if (GridIds != "")
                    {
                        string[] value = Widgets.Split(',');
                        for (int i = 0; i < value.Count(); i++)
                        {
                            var item = Convert.ToInt32(value[i]);
                            var ManTabGrid = db.ManageTabGrids.Where(q => q.GridID == item && q.UserID == userId).Select(r => r).FirstOrDefault();

                            ManTabGrid.UserSelected = true;

                            Grids.Add(Convert.ToInt32(value[i]));
                        }
                    }

                    var TotalGrids = db.ManageTabGrids.Where(q => q.UserID == userId && q.IsActive == true).Select(x => x.GridID).ToList();
                    var UnSelected = TotalGrids.Except(Grids).ToList();

                    foreach (var item in UnSelected)
                    {

                        var ManTabGrid = db.ManageTabGrids.Where(q => q.GridID == item && q.UserID == userId).Select(r => r).FirstOrDefault();

                        ManTabGrid.UserSelected = false;
                    }


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
        public ActionResult Mylearning()
        {
            DSRCManagementSystem.Models.LDHomeModel AsgnList = new DSRCManagementSystem.Models.LDHomeModel();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
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
                                 select new { training, trainingType, tech, user }).OrderByDescending(o => o.training.ScheduledDate.Value.Year).ThenByDescending(o => o.training.ScheduledDate.Value.Month).ThenByDescending(o => o.training.ScheduledDate.Value.Day);
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
                                             where tn.UserId == userId && a.ScheduledDate <= today && tn.Score > 0 && i.BranchId == BId
                                             select new DSRCManagementSystem.Models.HistorytrainingModel()
                                             {
                                                 TrainingId = a.TrainingId,
                                                 TrainingName = a.TrainingName,
                                                 TechnologyName = o.TechnologyName,
                                                 ScheduledDate = a.ScheduledDate,
                                                 Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : ""))

                                             }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).ToList();



                foreach (var item in AsgnList.historyTrainings)
                {
                    var val = db.TrainingFeedBackCalcs.Where(o => o.UserId == userId && o.TrainingId == item.TrainingId && o.Flag == false).Select(o => o).ToList();
                    int FeedbackCount = val.Count();
                    item.FeedbackCount = FeedbackCount;
                }


                AsgnList.unattendedTrainings = (from a in db.Trainings
                                                join t in db.Master_TrainingType on a.TrainingTypeId equals t.TrainingTypeId
                                                join o in db.Master_TrainingTechnology on a.TechnologyId equals o.TechnologyId
                                                join i in db.Users on a.InstructorId equals i.UserID
                                                join tn in db.TrainingNominations on a.TrainingId equals tn.TrainingId
                                                where tn.UserId == userId && a.ScheduledDate <= today && tn.Score < 0 && i.BranchId == BId
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
                                     where a.UserId == userId && a.NominationFlag == true && u.BranchId == BId && a.CompletionFlag == false
                                     select new
                                     {
                                         a,
                                         t,
                                         o,
                                         u
                                     }).OrderByDescending(o => o.t.ScheduledDate.Value.Year).ThenByDescending(o => o.t.ScheduledDate.Value.Month).ThenByDescending(o => o.t.ScheduledDate.Value.Day).ToList();

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

                    // if (startTime >= DateTime.Now)
                    //   {
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
                    // }
                }

                //AsgnList.conductedtrainings = (from a in db.TrainingNominations.Where(x => x.IsActive == true)
                //                               join t in db.Trainings on a.TrainingId equals t.TrainingId
                //                               join o in db.TrainingTechnologies on t.TechnologyId equals o.TechnologyId
                //                               join u in db.Users on t.InstructorId equals u.UserID                                           
                //                               where t.InstructorId == userId && a.Score > 0
                //                               orderby t.ScheduledDate descending                                          
                //                               select new DSRCManagementSystem.Models.Conductedtrainingmodel()
                //                               {
                //                                   NominationId = a.NominationId,
                //                                   TrainingId = a.TrainingId,
                //                                   TrainingName = t.TrainingName,
                //                                   TechnologyName = o.TechnologyName,
                //                                   ScheduledDate = t.ScheduledDate,
                //                                   Starttime = t.StartTime,
                //                                   Instructor = ((u.FirstName.Length > 0 ? u.FirstName : "") + " " + (u.LastName.Length > 0 ? u.LastName : ""))

                //                               }).Distinct().OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).ToList();


                //from a in db.Trainings.Where(x=>x.IsActive==true)
                //  join o in db.Master_TrainingTechnology on a.TechnologyId equals o.TechnologyId
                // join u in db.Users on a.InstructorId equals u.UserID
                //  where a.InstructorId == userId && (a.StatusId == 3 || a.StatusId == 4 || a.StatusId == 6)



                AsgnList.conductedtrainings = (from rc in db.Trainings
                                               join l in db.Master_TrainingLevel on rc.LevelId equals l.LevelId
                                               join t in db.Master_TrainingTechnology on rc.TechnologyId equals t.TechnologyId
                                               join i in db.Users on rc.InstructorId equals i.UserID
                                               join n in db.TrainingNominations on rc.TrainingId equals n.TrainingId
                                               where rc.IsActive == true && rc.InstructorId == userId
                                              && (rc.StatusId == 3 || rc.StatusId == 4 || rc.StatusId == 6 || rc.StatusId == 7 || rc.StatusId == 10)
                                              && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                              && i.BranchId == BId
                                               //join c in db.TrainingCompletions on rc.TrainingId equals c.TrainingId                                          
                                               //where rc.IsActive == true && rc.InstructorId == UserId && n.Score == 0 && n.CompletionFlag==true && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date
                                               //where rc.IsActive == true && rc.InstructorId == userId && EntityFunctions.TruncateTime(rc.ScheduledDate) <= DateTime.Today.Date && i.BranchId == BId && n.CompletionFlag == false
                                               select new DSRCManagementSystem.Models.Conductedtrainingmodel()
                                               {

                                                   TrainingName = rc.TrainingName,
                                                   TechnologyName = t.TechnologyName,
                                                   ScheduledDate = rc.ScheduledDate,
                                                   Starttime = rc.StartTime,
                                                   TrainingId = rc.TrainingId,
                                                   Instructor = ((i.FirstName.Length > 0 ? i.FirstName : "") + " " + (i.LastName.Length > 0 ? i.LastName : "")),
                                                   Endtime = rc.EndTime,
                                                   Nominations = rc.NumberOfNominated,
                                                   IsCompleted = n.CompletionFlag,
                                                   submit = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == true).Count(),
                                                   pending = db.TrainingFeedBackCalcs.Where(x => x.TrainingId == rc.TrainingId && x.Flag == false).Count(),

                                               }).OrderByDescending(o => o.ScheduledDate.Value.Year).ThenByDescending(o => o.ScheduledDate.Value.Month).ThenByDescending(o => o.ScheduledDate.Value.Day).Distinct().ToList();
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
                    //int? emp = Convert.ToInt32(empid);
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
                    //nom.EmpId = Convert.ToInt32(empid);
                    nom.EmpId = empid;
                    nom.UserId = user;
                    nom.EmpName = empname;
                    nom.TechnologyId = technology;
                    nom.EmailId = email;
                    nom.NominationFlag = true;
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

        public int GetBranch(int Id)
        {
            // int user = Convert.ToInt32(Session["UserID"]);
            DSRCManagementSystemEntities1 obj = new DSRCManagementSystemEntities1();
            var BranchId = obj.Users.Where(x => x.UserID == Id).Select(x => x.BranchId).FirstOrDefault();
            int BId = Convert.ToInt32(BranchId);
            return BId;
        }
        private List<SelectListItem> GetMonths()
        {
            var MonthsList = new List<SelectListItem>();
            try
            {

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<Master_TSR_Month> MonthList = db.Master_TSR_Month.ToList();
                    foreach (var item in MonthList)
                    {
                        MonthsList.Add(new SelectListItem { Text = item.Month, Value = item.Id.ToString() });
                    }

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return MonthsList;
        }
    }
}
