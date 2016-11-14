using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common.CommandTrees;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
//using DSRCHRMSRemainderService;
using DSRCManagementSystem.DSRCLogic;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.Models.Domain_Models;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Configuration;
using System.Globalization;
using NPOI.SS.Formula.Functions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Net.Mail;
using Utilities;


namespace DSRCManagementSystem.Controllers
{
    public class ManualAttendanceController : Controller
    {
        // GET: /ManualAttendance/

        public DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult ManualAttendance()
        {

            Session["InTimeEntry"] = MasterEnum.ManualAttendance.InTime.ToString();
            Session["OutTimeEntry"] = MasterEnum.ManualAttendance.OutTime.ToString();

            Session["Holiday"] = 0;
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try
            {
                ManualAttendance objtask = new ManualAttendance();

                DateTime strdate = DateTime.Now;

                string LeaveType = string.Empty;

                ViewBag.CurrentDate = DateTime.Now.ToString("dd/MM/yyyy").Replace("-", "/");



                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                              select new
                              {
                                  DepartmentId = p.DepartmentId,
                                  DepartmentName = p.DepartmentName

                              }).OrderBy(x => x.DepartmentName).ToList();


                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");


                ViewBag.LeaveTypeList =
                                    new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }.Union(
                                            db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");




                var Deparmentgroup = (from d in db.Departments
                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                      where d.IsActive == true && dg.IsActive == true
                                      select new
                                      {
                                          GroupID = dg.GroupID,
                                          GroupName = dg.GroupName
                                      }).OrderBy(x=>x.GroupName).ToList();


                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");


                ViewBag.BranchList =
                    new SelectList(
                        new[] { new Master_Branches() { BranchID = 0, BranchName = "All Branch" } }.Union(
                            db.Master_Branches.ToList()), "BranchID", "BranchName", 0);

               
              


                return BindGrid(0, 1, strdate, 0);

            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
        }



        [HttpPost]
        public ActionResult ManualAttendance(FormCollection form)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            Session["Holiday"] = 0;
            try
            {
                ManualAttendance objAttend = new ManualAttendance();

                int deptmnt = (form["Department"] == "") ? 0 : Convert.ToInt16(form["Department"]);
                int branch = (form["Branch"] == "") ? 0 : Convert.ToInt16(form["Branch"]);
                int group = (form["GroupName"] == "") ? 0 : Convert.ToInt16(form["GroupName"]);


                DateTime strdate = (form["CurrentDate"] == "") ? DateTime.Now : Convert.ToDateTime(form["CurrentDate"].ToString().Replace(",", ""));

                ViewBag.CurrentDate = strdate.ToString("dd/MM/yyyy").Replace("-", "/");


                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                      select new
                                      {
                                          DepartmentId = p.DepartmentId,
                                          DepartmentName = p.DepartmentName

                                      }).OrderBy(x => x.DepartmentName).ToList();


                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                ViewBag.LeaveTypeList =
                    new SelectList(
                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }.Union(
                        db.LeaveTypes.Take(3).Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);



                var Deparmentgroup = (from d in db.Departments
                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                      select new
                                      {
                                          GroupID = dg.GroupID,
                                          GroupName = dg.GroupName
                                      }).OrderBy(x => x.GroupName).ToList();

                if (deptmnt == 0)
                {
                    ViewBag.DepartmentGroup = null;
                }
                else
                {
                    ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);

                }

                if (deptmnt == 0 && branch == 0)
                {
                    return RedirectToAction("ManualAttendance", "ManualAttendance");
                }
                else
                {
                    return BindGrid(deptmnt, branch, strdate, group);
                }


            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
        }

        [HttpPost]
        public ActionResult AddAttendanceManually(List<string> objmodel)
        {
            Session["Holiday"] = 0;
            var json_serializer = new JavaScriptSerializer();
            check memberObj = json_serializer.Deserialize<check>(objmodel[0]);
            List<ManualLeave> newMembers = new List<ManualLeave>(memberObj.UserNames);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
          //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
            var logo = CommonLogic.getLogoPath();
            string[] words;

            //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
            words = logo.Split(new string[] { "../../" }, StringSplitOptions.None);
            string pathvalue = CommonLogic.getLogoPath();
            var imagePath = new List<string>() { Server.MapPath("~/Content/Template/images/Circle_Red.png"), Server.MapPath("~/Content/Template/images/Circle_Orange.png"), Server.MapPath("~/Content/Template/images/Circle_Green.png"), Server.MapPath(pathvalue) };

            var ManualAttendance = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Manual Attendance")
                                    join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                    select new DSRCManagementSystem.Models.Email
                                    {
                                        To = p.To,
                                        CC = p.CC,
                                        BCC = p.BCC,
                                        Subject = p.Subject,
                                        Template = q.TemplatePath
                                    }).FirstOrDefault();
            string ManualLeaveRequest = "";
            var objLeaveRequestApproved = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Manual Attendance Leave Request")
                                           join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                           select new DSRCManagementSystem.Models.Email
                                           {
                                               To = p.To,
                                               CC = p.CC,
                                               BCC = p.BCC,
                                               Subject = p.Subject,
                                               Template = q.TemplatePath
                                           }).FirstOrDefault();
            string MailBody = "";
            List<string> MailLeaveRequest = new List<string>();
            string TemplatePathLeaveRequestApproved = Server.MapPath(objLeaveRequestApproved.Template);
            string htmlLeaveRequestApproved = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);

            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            try
            {
                double LOPs = 0.0;
                var LeaveStatusId = MasterEnum.RequestStatus.Approved.GetHashCode();
                int LoggedInId = (int)Session["UserId"];
                string ManagerName = db.Users.FirstOrDefault(o => o.UserID == LoggedInId).FirstName + " " + db.Users.FirstOrDefault(o => o.UserID == LoggedInId).LastName;
                //Insert records to LeaveRequest Tables

                if (newMembers.Count == 0)
                {
                    return Json(new { Result = "UnSuccess", JsonRequestBehavior.AllowGet });
                }
                var objLeaveRequest = new LeaveRequest();
                var ObjInsert = new LeaveBalanceCount();
                var EmpId = "";
                foreach (var newmember in newMembers)
                {

                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(newmember.CurrentDate);
                    string weekend = dt.DayOfWeek.ToString();

                    string userid = newmember.UserID.ToString();
                    //string intime = Convert.ToString(newmember.InTime);
                    // string outtime = Convert.ToString(newmember.OutTime);
                    int UserID = Convert.ToInt32(userid);
                    EmpId = db.Users.Where(x => x.UserID == UserID).Select(o => o.EmpID).FirstOrDefault();
                    var leave = db.AddHolidays.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt)).Select(o => o.Date).ToList();
                    var OUtOfOfficeUSerID = db.OutOfOfficeDetails.Where(x => (EntityFunctions.TruncateTime(x.ODStartDate) <= EntityFunctions.TruncateTime(dt) || EntityFunctions.TruncateTime(x.ODEndDate) >= EntityFunctions.TruncateTime(dt)) && x.Userid == newmember.UserID && x.RequestStatusId ==2).Select(o => o.Userid).ToList();
                if (OUtOfOfficeUSerID.Count() == 0)
                    {

                    if (newmember != null)
                    {
                        if (weekend == "Saturday" || weekend == "Sunday" || leave.Count > 0)
                        {
                            if (leave.Count > 0)
                            {
                                Session["Holiday"] = 1;
                            }
                            if (newmember.chkOnOff == "1")
                            {
                                if (leave.Count > 0)
                                {
                                    Session["Holiday"] = 1;
                                }
                                DateTime date = Convert.ToDateTime(newmember.CurrentDate);
                                var BranchID = newmember.BranchID;
                                var checkManualwithBio = db.TimeManagements.Where(x => x.EmpID == EmpId && EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(date) && x.BranchId == BranchID).Select(o => o).FirstOrDefault();
                                int UserIDs = Convert.ToInt32(userid);
                                var RemoveLeaveRequest = db.LeaveRequests.Where(x => x.UserId == UserIDs && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(date) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(date)).FirstOrDefault();
                                if (checkManualwithBio != null)
                                {
                                    db.TimeManagements.DeleteObject(checkManualwithBio);
                                    db.SaveChanges();
                                }

                                DSRCManagementSystem.TimeManagement objmanagement = new DSRCManagementSystem.TimeManagement();


                                string InTime = Convert.ToString(newmember.InTime);
                                string OuTtime = Convert.ToString(newmember.OutTime);
                                
                                int index = InTime.IndexOf(":") + 1;
                                int piece = Convert.ToInt32(InTime.Substring(index));
                                int piece1 = Convert.ToInt32(InTime.Substring(0, (index - 1)));

                                int index1 = OuTtime.IndexOf(":") + 1;
                                int piece2 = Convert.ToInt32(OuTtime.Substring(index));
                                int piece3 = Convert.ToInt32(OuTtime.Substring(0, (index - 1)));


                                if ((InTime == "") || (OuTtime == ""))
                                {
                                    objmanagement.BranchId = newmember.BranchID;
                                    objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                    objmanagement.EmpID = EmpId;                                   
                                    objmanagement.InTime = MasterEnum.ManualAttendance.InTime;
                                    objmanagement.OutTime = MasterEnum.ManualAttendance.OutTime;
                                    objmanagement.InTimeMin = 560;
                                    objmanagement.OutTimeMin = 1100;
                                    objmanagement.TotalTime = 550;
                                    db.AddToTimeManagements(objmanagement);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    if (checkManualwithBio != null)
                                    {                                      
                                        objmanagement.BranchId = newmember.BranchID;
                                        objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                        objmanagement.EmpID = EmpId;
                                        objmanagement.InTime = InTime;
                                        objmanagement.OutTime = OuTtime;
                                        //objmanagement.InTimeMin = 560;
                                        //objmanagement.OutTimeMin = 1100;
                                        //objmanagement.TotalTime = 550;
                                        objmanagement.InTimeMin = Convert.ToInt32((piece1 * 60) + piece);
                                        objmanagement.OutTimeMin = Convert.ToInt32((piece3 * 60) + piece2);
                                        objmanagement.TotalTime = Convert.ToInt32(objmanagement.OutTimeMin - objmanagement.InTimeMin);
                                        db.AddToTimeManagements(objmanagement);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                      
                                        //string InTimeEntry = ConfigurationManager.AppSettings["ManualInTime"];
                                        //string OutTimeEntry = ConfigurationManager.AppSettings["ManualOutTime"];
                                        string InTimeEntry = AppValue.GetFromMailAddress("In Time");
                                        string OutTimeEntry = AppValue.GetFromMailAddress("Out Time");
                                        objmanagement.BranchId = newmember.BranchID;
                                        objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                        objmanagement.EmpID = EmpId;
                                        objmanagement.InTime = InTime;
                                        objmanagement.OutTime = OuTtime;
                                        //objmanagement.InTimeMin = 560;
                                        //objmanagement.OutTimeMin = 1100;
                                        //objmanagement.TotalTime = 550;
                                        objmanagement.InTimeMin = Convert.ToInt32((piece1 * 60) + piece);
                                        objmanagement.OutTimeMin = Convert.ToInt32((piece3 * 60) + piece2);
                                        objmanagement.TotalTime = Convert.ToInt32(objmanagement.OutTimeMin - objmanagement.InTimeMin);
                                        db.AddToTimeManagements(objmanagement);
                                        db.SaveChanges();
                                    }
                                }



                            }
                            else
                            {
                                if (leave.Count > 0)
                                {
                                    Session["Holiday"] = 1;
                                }
                                DateTime date = Convert.ToDateTime(newmember.CurrentDate);
                                var BranchID = newmember.BranchID;
                                int UserIDs = Convert.ToInt32(userid);
                                var checkManualwithBio = db.TimeManagements.Where(x => x.EmpID == EmpId && EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(date) && x.BranchId == BranchID).Select(o => o).FirstOrDefault();
                                if (checkManualwithBio != null)
                                {
                                    db.TimeManagements.DeleteObject(checkManualwithBio);
                                    db.SaveChanges();
                                }
                            }

                            
                        }
                        else if (newmember.chkOnOff == "0")
                        {

                            var BranchID = newmember.BranchID;
                            DateTime date = Convert.ToDateTime(newmember.CurrentDate);
                            userid = newmember.UserID.ToString();
                            DSRCManagementSystem.TimeManagement objmanagement = new DSRCManagementSystem.TimeManagement();
                            var RemoveEntryFromTimeManagement = db.TimeManagements.Where(x => x.EmpID == EmpId && x.BranchId == BranchID && EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(date)).FirstOrDefault();
                            if (RemoveEntryFromTimeManagement != null)
                            {
                                db.TimeManagements.DeleteObject(RemoveEntryFromTimeManagement);
                                db.SaveChanges();
                            }
                            int ID;
                            if (userid == "")
                            {
                                ID = 1;
                            }
                            else
                            {
                                ID = Convert.ToInt32(userid);
                            }
                            var index = "";
                            var noofdays = "";


                            var AcadamicEndMonths = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                            int Year = DateTime.Now.Month <= AcadamicEndMonths ? DateTime.Now.Year - 1 : DateTime.Now.Year;

                            bool isEligible;
                            List<int> EligibleLeaveTypes = new List<int>();

                            var dateOfJoin = objdb.Users.Where(x => x.UserID == ID).Select(o => o.DateOfJoin).FirstOrDefault();

                            if (dateOfJoin == null)
                                isEligible = false;
                            else
                            {
                                var completedDays = (DateTime.Now - dateOfJoin).Value.Days;
                                if (completedDays < 365)
                                    isEligible = false;
                                else
                                    isEligible = true;
                            }


                            var earnedleave = "";
                            var sickleave = "";
                            var casualLeave = "";
                            int SickLeaveCount = 0;
                            int CasualLeaveCount = 0;
                            int EarnedLeaveCount = 0;
                            //int x1 = 0;
                            if (isEligible)
                            {
                                sickleave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                                if (sickleave != "")
                                {
                                    SickLeaveCount = Convert.ToInt32(sickleave);
                                   // SickLeaveCount = int.TryParse(sickleave,out x1);
                                    if (SickLeaveCount < 6)
                                    {
                                        index = "1";
                                        noofdays = Convert.ToString(sickleave);
                                    }
                                    else
                                    {
                                        casualLeave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                                        if (casualLeave != "")
                                        {
                                            CasualLeaveCount = Convert.ToInt32(casualLeave);
                                            if (CasualLeaveCount < 6)
                                            {
                                                index = "2";
                                                noofdays = Convert.ToString(casualLeave);
                                            }
                                            else
                                            {
                                                earnedleave = objdb.LeaveTypes.Where(x => x.LeaveTypeId == 3).Select(o => o.DaysAllowed).FirstOrDefault().ToString();
                                                if (earnedleave != "")
                                                {
                                                    EarnedLeaveCount = Convert.ToInt32(earnedleave);
                                                    if (EarnedLeaveCount < 12)
                                                    {
                                                        index = "3";
                                                        noofdays = Convert.ToString(earnedleave);
                                                    }
                                                    else
                                                    {
                                                        //index = "7";
                                                        index = "1";
                                                    }
                                                }
                                                else
                                                {
                                                    ObjInsert = new LeaveBalanceCount()

                                                    {

                                                        UserId = ID,
                                                        LeaveTypeId = 3,
                                                        Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                                        Value = Convert.ToInt32(0)
                                                    };

                                                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                                                    db.SaveChanges();
                                                    index = "3";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ObjInsert = new LeaveBalanceCount()

                                            {

                                                UserId = ID,
                                                LeaveTypeId = 2,
                                                Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                                Value = Convert.ToInt32(0)
                                            };

                                            db.LeaveBalanceCounts.AddObject(ObjInsert);
                                            db.SaveChanges();
                                            index = "2";
                                        }
                                    }
                                }
                                else
                                {
                                    ObjInsert = new LeaveBalanceCount()

                                    {

                                        UserId = ID,
                                        LeaveTypeId = 1,
                                        Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                        Value = Convert.ToInt32(0)
                                    };

                                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                                    db.SaveChanges();
                                    index = "1";
                                }

                            }
                            else
                            {
                                sickleave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                                if (sickleave != "")
                                {
                                    SickLeaveCount = Convert.ToInt32(sickleave);
                                    if (SickLeaveCount < 6)
                                    {
                                        index = "1";
                                        noofdays = Convert.ToString(sickleave);
                                    }
                                    else
                                    {
                                        casualLeave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                                        if (casualLeave != "")
                                        {
                                            CasualLeaveCount = Convert.ToInt32(casualLeave);
                                            if (CasualLeaveCount < 6)
                                            {
                                                index = "2";
                                                noofdays = Convert.ToString(casualLeave);
                                            }
                                            else
                                            {
                                                index = "1";
                                                //index= "7";
                                            }
                                        }
                                        else
                                        {
                                            ObjInsert = new LeaveBalanceCount()

                                            {

                                                UserId = ID,
                                                LeaveTypeId = 2,
                                                Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                                Value = Convert.ToInt32(0)
                                            };

                                            db.LeaveBalanceCounts.AddObject(ObjInsert);
                                            db.SaveChanges();
                                            index = "2";
                                        }
                                    }
                                }
                                else
                                {
                                    ObjInsert = new LeaveBalanceCount()

                                   {

                                       UserId = ID,
                                       LeaveTypeId = 1,
                                       Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                       Value = Convert.ToInt32(0)
                                   };

                                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                                    db.SaveChanges();
                                    index = "1";
                                }

                            }

                            DateTime currentdate = Convert.ToDateTime(newmember.CurrentDate);
                            int LeaveRequestCheck = db.LeaveRequests.Where(x => x.UserId == newmember.UserID && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(currentdate)).Select(o => o.UserId).Count();
                            if (LeaveRequestCheck == 0)
                            {

                                objLeaveRequest = new LeaveRequest()
                               {
                                   UserId = newmember.UserID,
                                   LeaveTypeId = Convert.ToByte(index),
                                   StartDateTime = Convert.ToDateTime(newmember.CurrentDate),
                                   EndDateTime = Convert.ToDateTime(newmember.CurrentDate),
                                   Comments = newmember.Comments,
                                   ProcessedOn = DateTime.Now,
                                   ProcessedBy = LoggedInId,
                                   LeaveDays = Convert.ToInt32(newmember.LeaveDays),
                                   LeaveStatusId = Convert.ToByte(LeaveStatusId),
                                   RequestedDate =DateTime.Now ,
                                   ReportingTo = LoggedInId
                               };

                                db.LeaveRequests.AddObject(objLeaveRequest);
                                db.SaveChanges();
                                int LeaveRequestId = Convert.ToInt32(objLeaveRequest.LeaveRequestId);

                                if (LeaveStatusId.ToString() != null)
                                {
                                    /** For Calculatin LOP calling this Mehtod if check availability is not called*****/
                                    GetLOPDays(LeaveRequestId); //pass incremental ID from LeaveRequest Table
                                    LOPs = Convert.ToDouble(TempData["LOP"]);

                                    objLeaveRequest.LeaveStatusId = (byte)LeaveStatusId;
                                    objLeaveRequest.LOP = LOPs;
                                    db.SaveChanges();

                                    //If Marriage Leave is Applied and Approved Marital Status has to be updated in Users Table
                                    if (objLeaveRequest.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                                    {
                                        var UpdateMaritalStatus = db.Users.FirstOrDefault(o => o.UserID == objLeaveRequest.UserId);
                                        UpdateMaritalStatus.MaritalStatus = MasterEnum.MaritialStatus.Married.GetHashCode(); /* 1-Married 2-UnMarried*/
                                        db.SaveChanges();
                                    }

                                    var FromDate = objLeaveRequest.StartDateTime;
                                    var ToDate = objLeaveRequest.EndDateTime;
                                    var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                                    var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                                    bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                                    LeaveModel leaveRequest = new LeaveModel();
                                    var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                                    var a = GetLeaveBalance(years, objLeaveRequest.UserId);

                                    leaveRequest.Balance = (from b in a
                                                            select new LevaeBalance()
                                                            {
                                                                LeaveTypeId = b.LeaveTypeId,
                                                                Name = b.LeaveType,
                                                                DaysAllowed = (int)b.DaysAllowed,
                                                                UsedDays = (int)b.UsedDays,
                                                            }).ToList();
                                    double RemainingDays = leaveRequest.Balance.Where(o => o.LeaveTypeId == objLeaveRequest.LeaveTypeId).Select(o => o.RemainingDays).FirstOrDefault();

                                    if (objLeaveRequest.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode() || objLeaveRequest.LeaveTypeId == MasterEnum.LeaveTypes.Maternity.GetHashCode())
                                    {
                                        var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                                                  where leavebalance.UserId == objLeaveRequest.UserId &&
                                                                      leavebalance.LeaveTypeId == objLeaveRequest.LeaveTypeId
                                                                  select leavebalance).FirstOrDefault();

                                        if (updateleavebalance == null)
                                        {
                                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                                            updateleavebalance.UserId = objLeaveRequest.UserId;
                                            updateleavebalance.LeaveTypeId = objLeaveRequest.LeaveTypeId;
                                            updateleavebalance.Value = objLeaveRequest.LeaveDays;
                                            updateleavebalance.Year = year;
                                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            updateleavebalance.Value = updateleavebalance.Value + objLeaveRequest.LeaveDays;
                                            updateleavebalance.Year = year;
                                            db.SaveChanges();
                                        }
                                    }

                                    UpdateLeaveBalance(objLeaveRequest);



                                    MailBody = "";
                                    //string pathvalue = "~/" + words[1];
                                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == objLeaveRequest.LeaveTypeId).Name;
                                    string StartTime = Convert.ToDateTime(objLeaveRequest.StartDateTime).ToString("dd MMM yyyy");
                                    string EndTime = Convert.ToDateTime(objLeaveRequest.EndDateTime).ToString("dd MMM yyyy");

                                    string Empid = db.Users.FirstOrDefault(o => o.UserID == objLeaveRequest.UserId).EmpID;
                                    string EmployeeName = db.Users.FirstOrDefault(o => o.EmpID == Empid).FirstName + " " + db.Users.FirstOrDefault(o => o.EmpID == Empid).LastName;

                                    var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Leave Request Approved").Select(o => o.EmailTemplateID).FirstOrDefault();
                                    var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Leave Request Approved").Select(x => x.TemplatePath).FirstOrDefault();
                                    if ((check != null) && (check != 0))
                                    {
                                        var objLeaveRequestApprove = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Leave Request Approved")
                                                                      join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                      select new DSRCManagementSystem.Models.Email
                                                                      {
                                                                          To = p.To,
                                                                          CC = p.CC,
                                                                          BCC = p.BCC,
                                                                          Subject = p.Subject,
                                                                          Template = q.TemplatePath
                                                                      }).FirstOrDefault();

                                        string TemplatePathLeaveRequest = Server.MapPath(objLeaveRequestApprove.Template);

                                        string htmlLeaveRequest = System.IO.File.ReadAllText(TemplatePathLeaveRequest);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#UserName", objLeaveRequest.User1.FirstName + " " + objLeaveRequest.User1.LastName);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#ManagerName", objLeaveRequest.User.FirstName + " " + objLeaveRequest.User.LastName);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#LeaveTypeName", LeaveTyepName);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#StartDateTime", StartTime);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#EndDateTime", EndTime);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#totalLeaveDays", objLeaveRequest.LeaveDays.ToString());
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#Comments", objLeaveRequest.Comments);
                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#CompanyName", company);

                                        MailBody = @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                         + Empid + @"</td><td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                         + EmployeeName + @"</td><td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                         + LeaveTyepName + @"</td><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                         + StartTime + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; text-align: center; border-top: 1px solid #ebebeb;'>"
                                         + EndTime + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;  text-align: center;border-top: 1px solid #ebebeb; white-space: pre-wrap;'>"
                                         + objLeaveRequest.LeaveDays.ToString() + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; text-align: left; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                         + objLeaveRequest.Comments + @"</td></tr>";

                                        ManualLeaveRequest += MailBody;

                                        if (objLeaveRequest.LOP > 0 && (objLeaveRequest.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() || objLeaveRequest.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode()))
                                        {
                                            string LOPDays = "<p style='padding-left: 2%; color: #006699; font-weight: bold;'>  No.of LOP Days&nbsp;&nbsp;:<label style='color: Black;'>" + objLeaveRequest.LOP + "</label></p>";
                                            htmlLeaveRequest = htmlLeaveRequest.Replace("#LOPDays", LOPDays);

                                            string LOP = "<p style='padding-left: 2%; color: #FF0000; font-weight: bold;'>*This leave request has to be considered as LOP.</p>";
                                            htmlLeaveRequest = htmlLeaveRequest.Replace("#LOP", LOP);
                                        }
                                        else
                                        {
                                            htmlLeaveRequest = htmlLeaveRequest.Replace("#LOPDays", "");
                                            htmlLeaveRequest = htmlLeaveRequest.Replace("#LOP", "");
                                        }


                                        htmlLeaveRequest = htmlLeaveRequest.Replace("#ServerName", ServerName);


                                        if (ServerName  != "http://win2012srv:88/")
                                        {

                                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                            string EmailAddress = string.Empty;

                                            foreach (string mail in MailIds)
                                            {
                                                EmailAddress += mail + ",";
                                            }

                                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                            string CCMailId = "kirankumar@dsrc.co.in";
                                            string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";
                                            Task.Factory.StartNew(() =>
                                            {

                                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApprove.Subject + " - Test Mail Please Ignore", null, htmlLeaveRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "Lakshminarsimhan.R@dsrc.co.in ", BCCMailId, Server.MapPath(pathvalue.ToString()));
                                            });
                                        }
                                        else
                                        {
                                            Task.Factory.StartNew(() =>
                                            {

                                                DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApprove.Subject, null, htmlLeaveRequest, "admin@dsrc.co.in", objLeaveRequest.User1.EmailAddress, objLeaveRequestApproved.CC, objLeaveRequestApproved.BCC, Server.MapPath(pathvalue.ToString()));
                                            });
                                        }

                                    }

                                    else
                                    {

                                        ExceptionHandlingController.TemplateMissing("Leave Request Approved", folder, ServerName);
                                    }
                                }
                            }

                        }
                        else
                        {

                            DateTime date = Convert.ToDateTime(newmember.CurrentDate);
                            var BranchID = newmember.BranchID;
                            var checkManualwithBio = db.TimeManagements.Where(x => x.EmpID == EmpId && EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(date) && x.BranchId == BranchID).Select(o => o).FirstOrDefault();
                            int UserIDs = Convert.ToInt32(userid);
                            var RemoveLeaveRequest = db.LeaveRequests.Where(x => x.UserId == UserIDs && EntityFunctions.TruncateTime(x.StartDateTime) == EntityFunctions.TruncateTime(date) && EntityFunctions.TruncateTime(x.EndDateTime) == EntityFunctions.TruncateTime(date)).FirstOrDefault();
                            if (RemoveLeaveRequest != null)
                            {
                                db.LeaveRequests.DeleteObject(RemoveLeaveRequest);
                                db.SaveChanges();
                            }

                            DSRCManagementSystem.TimeManagement objmanagement = new DSRCManagementSystem.TimeManagement();


                           string InTime = Convert.ToString(newmember.InTime);
                           // double InTime = Convert.ToDouble(newmember.InTime);
                            string OuTtime = Convert.ToString(newmember.OutTime);
                            //double OuTtime = Convert.ToDouble(newmember.OutTime);

                            int index = InTime.IndexOf(":") + 1;
                            int piece = Convert.ToInt32(InTime.Substring(index));
                            int piece1 = Convert.ToInt32(InTime.Substring(0, (index - 1)));

                            int index1 = OuTtime.IndexOf(":") + 1;
                            int piece2 = Convert.ToInt32(OuTtime.Substring(index));
                            int piece3 = Convert.ToInt32(OuTtime.Substring(0, (index - 1)));

                            if ((InTime == "") || (OuTtime == ""))
                            {

                                objmanagement.BranchId = newmember.BranchID;
                                objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                objmanagement.EmpID = EmpId;
                               // objmanagement.InTime = MasterEnum.ManualAttendance.InTime.GetHashCode().ToString();
                              //  objmanagement.OutTime = MasterEnum.ManualAttendance.OutTime.GetHashCode().ToString();
                                objmanagement.InTime = MasterEnum.ManualAttendance.InTime;
                                objmanagement.OutTime = MasterEnum.ManualAttendance.OutTime;
                                objmanagement.InTimeMin = 560;
                                objmanagement.OutTimeMin = 1100;
                                objmanagement.TotalTime = 550;
                                db.AddToTimeManagements(objmanagement);
                                db.SaveChanges();
                            }
                            else
                            {
                                if (checkManualwithBio != null)
                                {
                                    db.TimeManagements.DeleteObject(checkManualwithBio);
                                    db.SaveChanges();


                                    objmanagement.BranchId = newmember.BranchID;
                                    objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                    objmanagement.EmpID = EmpId;
                                    objmanagement.InTime = InTime;
                                    objmanagement.OutTime = OuTtime;
                                    //objmanagement.InTimeMin = 560;
                                    //objmanagement.OutTimeMin = 1100;
                                    //objmanagement.TotalTime = 550;
                                    objmanagement.InTimeMin = Convert.ToInt32((piece1 * 60) + piece);
                                    objmanagement.OutTimeMin = Convert.ToInt32((piece3 * 60) + piece2);
                                    objmanagement.TotalTime =Convert.ToInt32(objmanagement.OutTimeMin - objmanagement.InTimeMin);
                                    db.AddToTimeManagements(objmanagement);
                                    db.SaveChanges();
                                }
                                else
                                {

                                    objmanagement.BranchId = newmember.BranchID;
                                    objmanagement.Date = Convert.ToDateTime(newmember.CurrentDate);
                                    objmanagement.EmpID = EmpId;
                                    objmanagement.InTime = InTime;
                                    objmanagement.OutTime = OuTtime;
                                    //objmanagement.InTimeMin = 560;
                                    //objmanagement.OutTimeMin = 1100;
                                   // objmanagement.TotalTime = 550;
                                    objmanagement.InTimeMin = Convert.ToInt32((piece1*60)+piece);
                                    objmanagement.OutTimeMin = Convert.ToInt32((piece3*60)+piece2);
                                    objmanagement.TotalTime = Convert.ToInt32(objmanagement.OutTimeMin - objmanagement.InTimeMin);
                                    db.AddToTimeManagements(objmanagement);
                                    db.SaveChanges();
                                }
                            }

                            
                        }

                    }

                }

                }

             
                string TemplateManualAttendance = Server.MapPath(ManualAttendance.Template);
                var companyName = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                string htmlAttendance = System.IO.File.ReadAllText(TemplateManualAttendance);
                htmlAttendance = htmlAttendance.Replace("#SenderName", Session["FirstName"].ToString() + " " + Session["LastName"].ToString());
                htmlAttendance = htmlAttendance.Replace("#ServerName", ServerName);
                htmlAttendance = htmlAttendance.Replace("#CompanyName", companyName);
                htmlAttendance = htmlAttendance.Replace("#UserName", "All");
                htmlAttendance = htmlAttendance.Replace("#Body", "Manual Attendance has been approved by "+ManagerName+" for the following Employees.");
                if (objLeaveRequest.User != null)
                {
                    htmlAttendance = htmlAttendance.Replace("#ManagerName", objLeaveRequest.User.FirstName + " " + objLeaveRequest.User.LastName);
                }
                MailBody = "";
                foreach (var newmember in newMembers)
                {
                    if (newmember.chkOnOff =="1")
                    {
                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(newmember.CurrentDate);
                    string EmployeeName = db.Users.FirstOrDefault(o => o.UserID == newmember.UserID).FirstName + " " + db.Users.FirstOrDefault(o => o.UserID == newmember.UserID).LastName;
                    string EmpID = db.Users.FirstOrDefault(o => o.UserID == newmember.UserID).EmpID;

                    MailBody += @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"

                  + EmpID + @"</td><td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                  + EmployeeName + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                  + dt.ToString("dd MMM yyyy") + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb; white-space: pre-wrap;'>"
                  + newmember.InTime + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                  + newmember.OutTime + @"</td></tr>";


                    }
                }
               
                htmlAttendance = htmlAttendance.Replace("#MailBody", MailBody);
                htmlAttendance = htmlAttendance.Replace("#CompanyName", companyName);
                
                if (ServerName  != "http://win2012srv:88/")
                {

                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();


                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }

                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                    string CCMailId = "kirankumar@dsrc.co.in";
                    string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in";


                    Task.Factory.StartNew(() =>
                    {

                        DsrcMailSystem.MailSender.SendMail(null, ManualAttendance.Subject + " - Test Mail Please Ignore", null, htmlAttendance + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue.ToString()));
                    });
                }
                else
                {

                    Task.Factory.StartNew(() =>
                    {

                        DsrcMailSystem.MailSender.SendMail(null, ManualAttendance.Subject, "", htmlAttendance, "HRMS@dsrc.co.in", ManualAttendance.To, ManualAttendance.CC, ManualAttendance.BCC, Server.MapPath(pathvalue.ToString()));

                    });
                }
                               
                if (ManualLeaveRequest != "")
                {
                   
                    MailBody = "";
                    //string pathvalue = "~/" + words[1];
                    string LeaveTyepName = db.LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == objLeaveRequest.LeaveTypeId).Name;
                    string StartTime = Convert.ToDateTime(objLeaveRequest.StartDateTime).ToString("dd MMM yyyy");
                    string EndTime = Convert.ToDateTime(objLeaveRequest.EndDateTime).ToString("dd MMM yyyy");

                    string Empid = db.Users.FirstOrDefault(o => o.UserID == objLeaveRequest.UserId).EmpID;
                    string EmployeeName = db.Users.FirstOrDefault(o => o.EmpID == Empid).FirstName + " " + db.Users.FirstOrDefault(o => o.EmpID == Empid).LastName;


                    string htmlManualAttendanceLeaveRequest = System.IO.File.ReadAllText(TemplatePathLeaveRequestApproved);
                    htmlManualAttendanceLeaveRequest = htmlManualAttendanceLeaveRequest.Replace("#UserName", "All");
                    htmlManualAttendanceLeaveRequest = htmlManualAttendanceLeaveRequest.Replace("#Body","Leave has been Approved by "+ objLeaveRequest.User.FirstName + " " + objLeaveRequest.User.LastName +" for the following employees via Manual Attendance.");
                    htmlAttendance = htmlAttendance.Replace("#ManagerName", objLeaveRequest.User.FirstName + " " + objLeaveRequest.User.LastName);
                    MailBody=ManualLeaveRequest;
                   
                  
                    htmlManualAttendanceLeaveRequest = htmlManualAttendanceLeaveRequest.Replace("#MailBody", MailBody);
                    htmlManualAttendanceLeaveRequest = htmlManualAttendanceLeaveRequest.Replace("#CompanyName", company);
                    htmlManualAttendanceLeaveRequest = htmlManualAttendanceLeaveRequest.Replace("#ServerName", ServerName);


                    if (ServerName  != "http://win2012srv:88/")
                    {

                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                        string EmailAddress = string.Empty;

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        string CCMailId = "kirankumar@dsrc.co.in";
                        string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";
                        Task.Factory.StartNew(() =>
                        {

                            DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject + " - Test Mail Please Ignore", null, htmlManualAttendanceLeaveRequest + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, "Lakshminarsimhan.R@dsrc.co.in ", BCCMailId, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {

                            DsrcMailSystem.MailSender.SendMail(null, objLeaveRequestApproved.Subject, null, htmlManualAttendanceLeaveRequest, "admin@dsrc.co.in", objLeaveRequest.User1.EmailAddress, objLeaveRequestApproved.CC, objLeaveRequestApproved.BCC, Server.MapPath(pathvalue.ToString()));
                        });
                    }
                }

                if (objLeaveRequest.UserId == 0)
                {
                    return Json(new { Result = "UnSuccess", JsonRequestBehavior.AllowGet });
                }
                return Json(new { Result = "Success", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
        }

        public static void UpdateLeaveBalance(LeaveRequest leaveRequestToUpdate)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var ObjInsert = new LeaveBalanceCount();
            var index = "";
            bool isEligible;
            //LeaveTypeSelection

            int ID;
            if (leaveRequestToUpdate.UserId == null)
            {
                ID = 1;
            }
            else
            {
                ID = Convert.ToInt32(leaveRequestToUpdate.UserId);
            }

            var noofdays = "";


            var AcadamicEndMonths = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            int Year = DateTime.Now.Month <= AcadamicEndMonths ? DateTime.Now.Year - 1 : DateTime.Now.Year;

            //bool isEligible;
            List<int> EligibleLeaveTypes = new List<int>();

            var dateOfJoin = db.Users.Where(x => x.UserID == ID).Select(o => o.DateOfJoin).FirstOrDefault();

            if (dateOfJoin == null)
                isEligible = false;
            else
            {
                var completedDays = (DateTime.Now - dateOfJoin).Value.Days;
                if (completedDays < 365)
                    isEligible = false;
                else
                    isEligible = true;
            }

            var earnedleave = "";
            var sickleave = "";
            var casualLeave = "";
            int SickLeaveCount = 0;
            int CasualLeaveCount = 0;
            int EarnedLeaveCount = 0;
            if (isEligible)
            {
                sickleave = db.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                if (sickleave != "")
                {
                    SickLeaveCount = Convert.ToInt32(sickleave);
                    if (SickLeaveCount < 6)
                    {
                        index = "1";
                        noofdays = Convert.ToString(sickleave);
                    }
                    else
                    {
                        casualLeave = db.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                        if (casualLeave != "")
                        {
                            CasualLeaveCount = Convert.ToInt32(casualLeave);
                            if (CasualLeaveCount < 6)
                            {
                                index = "2";
                                noofdays = Convert.ToString(casualLeave);
                            }
                            else
                            {
                                earnedleave = db.LeaveTypes.Where(x => x.LeaveTypeId == 3).Select(o => o.DaysAllowed).FirstOrDefault().ToString();
                                if (earnedleave != "")
                                {
                                    EarnedLeaveCount = Convert.ToInt32(earnedleave);
                                    if (EarnedLeaveCount < 12)
                                    {
                                        index = "3";
                                        noofdays = Convert.ToString(earnedleave);
                                    }
                                    else
                                    {
                                        //index = "7";
                                        index = "1";
                                    }
                                }
                                else
                                {
                                    ObjInsert = new LeaveBalanceCount()

                                    {

                                        UserId = ID,
                                        LeaveTypeId = 3,
                                        Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                        Value = Convert.ToInt32(0)
                                    };

                                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                                    db.SaveChanges();
                                    index = "3";
                                }
                            }
                        }
                        else
                        {
                            ObjInsert = new LeaveBalanceCount()

                            {

                                UserId = ID,
                                LeaveTypeId = 2,
                                Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                Value = Convert.ToInt32(0)
                            };

                            db.LeaveBalanceCounts.AddObject(ObjInsert);
                            db.SaveChanges();
                            index = "2";
                        }
                    }
                }
                else
                {
                    ObjInsert = new LeaveBalanceCount()

                    {

                        UserId = ID,
                        LeaveTypeId = 1,
                        Year = Convert.ToInt32(DateTime.Now.Date.Year),
                        Value = Convert.ToInt32(0)
                    };

                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                    db.SaveChanges();
                    index = "1";
                }

            }
            else
            {
                sickleave = db.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                if (sickleave != "")
                {
                    SickLeaveCount = Convert.ToInt32(sickleave);
                    if (SickLeaveCount < 6)
                    {
                        index = "1";
                        noofdays = Convert.ToString(sickleave);
                    }
                    else
                    {
                        casualLeave = db.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                        if (casualLeave != "")
                        {
                            CasualLeaveCount = Convert.ToInt32(casualLeave);
                            if (CasualLeaveCount < 6)
                            {
                                index = "2";
                                noofdays = Convert.ToString(casualLeave);
                            }
                            else
                            {
                                //index = "7";
                                index = "1";
                            }
                        }
                        else
                        {
                            ObjInsert = new LeaveBalanceCount()

                            {

                                UserId = ID,
                                LeaveTypeId = 2,
                                Year = Convert.ToInt32(DateTime.Now.Date.Year),
                                Value = Convert.ToInt32(0)
                            };

                            db.LeaveBalanceCounts.AddObject(ObjInsert);
                            db.SaveChanges();
                            index = "2";
                        }
                    }
                }
                else
                {
                    ObjInsert = new LeaveBalanceCount()

                    {

                        UserId = ID,
                        LeaveTypeId = 1,
                        Year = Convert.ToInt32(DateTime.Now.Date.Year),
                        Value = Convert.ToInt32(0)
                    };

                    db.LeaveBalanceCounts.AddObject(ObjInsert);
                    db.SaveChanges();
                    index = "1";
                }

            }





            //LeaveTypeSelection

            try
            {
                if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode() || leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                {

                    var LeaveTypes = db.LeaveTypes.ToList();
                    var FromDate = leaveRequestToUpdate.StartDateTime;
                    var ToDate = leaveRequestToUpdate.EndDateTime;
                    var AcadamicStartMonth = db.CalendarYears.Select(o => o.StartingMonth).FirstOrDefault();
                    var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                    var year = FromDate.Value.Month <= 3 ? FromDate.Value.Year - 1 : FromDate.Value.Year;
                    bool IsAcadamicYearEnd = (FromDate.Value.Month == AcadamicEndMonth && ToDate.Value.Month != AcadamicEndMonth);

                    LeaveModel leaveRequest = new LeaveModel();
                    var years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                    var a = GetLeaveBalance(years, leaveRequestToUpdate.UserId);

                    leaveRequest.Balance = (from b in a
                                            select new LevaeBalance()
                                            {
                                                LeaveTypeId = b.LeaveTypeId,
                                                Name = b.LeaveType,
                                                DaysAllowed = (int)b.DaysAllowed,
                                                UsedDays = (int)b.UsedDays,
                                            }).ToList();

                    var doj = db.Users.FirstOrDefault(item => item.UserID == leaveRequestToUpdate.UserId).DateOfJoin;



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

                    int ExtendLeaveTypeId1 = 0;
                    int ExtendLeaveTypeId2 = 0;
                    int ExtendLeaveTypeId3 = 0;

                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                    {
                        ExtendLeaveTypeId1 = 2;
                        ExtendLeaveTypeId2 = 3;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                    {
                        ExtendLeaveTypeId1 = 1;
                        ExtendLeaveTypeId2 = 3;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                    {
                        ExtendLeaveTypeId1 = 2;
                        ExtendLeaveTypeId2 = 1;
                    }
                    else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        if (isEligible)
                        {
                            ExtendLeaveTypeId1 = 2;
                            ExtendLeaveTypeId2 = 1;
                            ExtendLeaveTypeId3 = 3;
                        }
                        else
                        {
                            ExtendLeaveTypeId1 = 1;
                            ExtendLeaveTypeId2 = 2;   // for earned unelligible this type wont be taken for count
                            ExtendLeaveTypeId3 = 2;
                        }
                    }

                    double detectionDays = 0.0;

                    var updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                              leavebalance.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId &&
                                              leavebalance.Year == year
                                              select leavebalance).FirstOrDefault();

                    var updateLeaveDetectionDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestToUpdate.LeaveRequestId);

                    double? UsedDays = 0.0;
                    double? AllowedDays = LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId).DaysAllowed;
                    double? TotalLeaveDays = leaveRequestToUpdate.LeaveDays;
                    double? RemainingAvailLeaveDays = 0.0;

                    if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                    {
                        updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                              leavebalance.LeaveTypeId == ExtendLeaveTypeId3 &&
                                              leavebalance.Year == year
                                              select leavebalance).FirstOrDefault();

                        AllowedDays = LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3).DaysAllowed;
                    }

                    if (updateleavebalance == null)
                    {
                        updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                        updateleavebalance.UserId = leaveRequestToUpdate.UserId;

                        if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                        {
                            updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId3;
                        }
                        else
                        {
                            updateleavebalance.LeaveTypeId = db.LeaveRequests.Where(o => o.LeaveRequestId == leaveRequestToUpdate.LeaveRequestId).Select(o => o.LeaveType.LeaveTypeId).FirstOrDefault();
                        }

                        RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                        if (TotalLeaveDays <= RemainingAvailLeaveDays)
                        {
                            updateleavebalance.Value = TotalLeaveDays;
                            detectionDays = (double)TotalLeaveDays;
                            TotalLeaveDays = 0;
                        }
                        else
                        {
                            updateleavebalance.Value = RemainingAvailLeaveDays;
                            detectionDays = (double)RemainingAvailLeaveDays;
                            TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                        }
                        updateleavebalance.Year = year;
                        db.LeaveBalanceCounts.AddObject(updateleavebalance);
                        db.SaveChanges();

                        if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                        {
                            if (index == "1")
                            {
                                updateLeaveDetectionDetails.Sick = detectionDays;
                            }
                            else if (index == "2")
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                            else if (index == "3")
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                        {
                            if (index == "1")
                            {
                                updateLeaveDetectionDetails.Sick = detectionDays;
                            }
                            else if (index == "2")
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                            else if (index == "3")
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                        {
                            if (index == "1")
                            {
                                updateLeaveDetectionDetails.Sick = detectionDays;
                            }
                            else if (index == "2")
                            {
                                updateLeaveDetectionDetails.Casual = detectionDays;
                            }
                            else if (index == "3")
                            {
                                updateLeaveDetectionDetails.Earned = detectionDays;
                            }
                        }
                        else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                        {
                            if (isEligible)
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                        {
                            AllowedDays = LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3).DaysAllowed;
                            UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId3 &&
                                                                                  o.UserId == leaveRequestToUpdate.UserId).Value;
                        }
                        else
                        {
                            UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == leaveRequestToUpdate.LeaveTypeId &&
                                                                                     o.UserId == leaveRequestToUpdate.UserId).Value;
                        }

                        //Detect From selected LeaveType

                        if (UsedDays < AllowedDays)
                        {
                            RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                            if (TotalLeaveDays <= RemainingAvailLeaveDays)
                            {
                                updateleavebalance.Value += TotalLeaveDays;
                                updateleavebalance.Year = year;
                                db.SaveChanges();

                                detectionDays = (double)TotalLeaveDays;
                                TotalLeaveDays = 0;
                            }
                            else
                            {
                                updateleavebalance.Value += RemainingAvailLeaveDays;
                                updateleavebalance.Year = year;
                                db.SaveChanges();

                                detectionDays = (double)RemainingAvailLeaveDays;
                                TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                            }




                            if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Sick.GetHashCode())
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Casual.GetHashCode())
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Earned_Leave.GetHashCode())
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else if (leaveRequestToUpdate.LeaveTypeId == MasterEnum.LeaveTypes.Marriage.GetHashCode())
                            {
                                if (isEligible)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                else
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                            }
                            db.SaveChanges();
                        }
                    }

                    ////Detect From next LeaveType
                    if (TotalLeaveDays > 0)
                    {
                        updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequestToUpdate.UserId && leavebalance.LeaveTypeId == ExtendLeaveTypeId1 &&
                                              leavebalance.Year == year
                                              select leavebalance).FirstOrDefault();

                        UsedDays = 0.0;
                        AllowedDays = LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId1).DaysAllowed;

                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId1;

                            RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                            if (TotalLeaveDays <= RemainingAvailLeaveDays)
                            {
                                updateleavebalance.Value = TotalLeaveDays;
                                detectionDays = (double)TotalLeaveDays;
                                TotalLeaveDays = 0;
                            }
                            else
                            {
                                updateleavebalance.Value = RemainingAvailLeaveDays;
                                detectionDays = (double)RemainingAvailLeaveDays;
                                TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                            }

                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();

                            if (ExtendLeaveTypeId1 == 1)
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else if (ExtendLeaveTypeId1 == 2)
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            else if (ExtendLeaveTypeId1 == 3)
                            {
                                if (index == "1")
                                {
                                    updateLeaveDetectionDetails.Sick = detectionDays;
                                }
                                else if (index == "2")
                                {
                                    updateLeaveDetectionDetails.Casual = detectionDays;
                                }
                                else if (index == "3")
                                {
                                    updateLeaveDetectionDetails.Earned = detectionDays;
                                }
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId1 &&
                                                                                 o.UserId == leaveRequestToUpdate.UserId).Value;

                            if (UsedDays < AllowedDays)
                            {
                                RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                                if (TotalLeaveDays <= RemainingAvailLeaveDays)
                                {
                                    updateleavebalance.Value += TotalLeaveDays;
                                    updateleavebalance.Year = year;
                                    db.SaveChanges();

                                    detectionDays = (double)TotalLeaveDays;
                                    TotalLeaveDays = 0;
                                }
                                else
                                {
                                    updateleavebalance.Value += RemainingAvailLeaveDays;
                                    updateleavebalance.Year = year;
                                    db.SaveChanges();

                                    detectionDays = (double)RemainingAvailLeaveDays;
                                    TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                                }

                                if (ExtendLeaveTypeId1 == 1)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                else if (ExtendLeaveTypeId1 == 2)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                else if (ExtendLeaveTypeId1 == 3)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                db.SaveChanges();
                            }
                        }
                    }
                    //Detect from next leave type;
                    if (TotalLeaveDays > 0)
                    {
                        if (isEligible)
                        {
                            updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                                  where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                                      leavebalance.LeaveTypeId == ExtendLeaveTypeId2 &&
                                                      leavebalance.Year == year
                                                  select leavebalance).FirstOrDefault();
                            UsedDays = 0.0;
                            AllowedDays = LeaveTypes.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId2).DaysAllowed;

                            if (updateleavebalance == null)
                            {
                                updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                                updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                                updateleavebalance.LeaveTypeId = (byte)ExtendLeaveTypeId2;

                                RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                                if (TotalLeaveDays <= RemainingAvailLeaveDays)
                                {
                                    updateleavebalance.Value = TotalLeaveDays;
                                    detectionDays = (double)TotalLeaveDays;
                                    TotalLeaveDays = 0;
                                }
                                else
                                {
                                    updateleavebalance.Value = RemainingAvailLeaveDays;
                                    detectionDays = (double)RemainingAvailLeaveDays;
                                    TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                                }

                                updateleavebalance.Year = year;
                                db.LeaveBalanceCounts.AddObject(updateleavebalance);
                                db.SaveChanges();

                                if (ExtendLeaveTypeId2 == 1)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                else if (ExtendLeaveTypeId2 == 2)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                else if (ExtendLeaveTypeId2 == 3)
                                {
                                    if (index == "1")
                                    {
                                        updateLeaveDetectionDetails.Sick = detectionDays;
                                    }
                                    else if (index == "2")
                                    {
                                        updateLeaveDetectionDetails.Casual = detectionDays;
                                    }
                                    else if (index == "3")
                                    {
                                        updateLeaveDetectionDetails.Earned = detectionDays;
                                    }
                                }
                                db.SaveChanges();
                            }
                            else
                            {
                                UsedDays = db.LeaveBalanceCounts.FirstOrDefault(o => o.LeaveTypeId == ExtendLeaveTypeId2 &&
                                                                                        o.UserId == leaveRequestToUpdate.UserId).Value;
                                if (UsedDays < AllowedDays)
                                {
                                    RemainingAvailLeaveDays = (double)AllowedDays - (double)UsedDays;

                                    if (TotalLeaveDays <= RemainingAvailLeaveDays)
                                    {
                                        updateleavebalance.Value += TotalLeaveDays;
                                        updateleavebalance.Year = year;
                                        db.SaveChanges();

                                        detectionDays = (double)TotalLeaveDays;
                                        TotalLeaveDays = 0;
                                    }
                                    else
                                    {
                                        updateleavebalance.Value += RemainingAvailLeaveDays;
                                        updateleavebalance.Year = year;
                                        db.SaveChanges();

                                        detectionDays = (double)RemainingAvailLeaveDays;
                                        TotalLeaveDays = TotalLeaveDays - RemainingAvailLeaveDays;
                                    }

                                    if (ExtendLeaveTypeId2 == 1)
                                    {
                                        if (index == "1")
                                        {
                                            updateLeaveDetectionDetails.Sick = detectionDays;
                                        }
                                        else if (index == "2")
                                        {
                                            updateLeaveDetectionDetails.Casual = detectionDays;
                                        }
                                        else if (index == "3")
                                        {
                                            updateLeaveDetectionDetails.Earned = detectionDays;
                                        }
                                    }
                                    else if (ExtendLeaveTypeId2 == 2)
                                    {
                                        if (index == "1")
                                        {
                                            updateLeaveDetectionDetails.Sick = detectionDays;
                                        }
                                        else if (index == "2")
                                        {
                                            updateLeaveDetectionDetails.Casual = detectionDays;
                                        }
                                        else if (index == "3")
                                        {
                                            updateLeaveDetectionDetails.Earned = detectionDays;
                                        }
                                    }
                                    else if (ExtendLeaveTypeId2 == 3)
                                    {
                                        if (index == "1")
                                        {
                                            updateLeaveDetectionDetails.Sick = detectionDays;
                                        }
                                        else if (index == "2")
                                        {
                                            updateLeaveDetectionDetails.Casual = detectionDays;
                                        }
                                        else if (index == "3")
                                        {
                                            updateLeaveDetectionDetails.Earned = detectionDays;
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    //still leave days remains na then it has to take as LOP
                    if (TotalLeaveDays > 0)
                    {
                        byte LeaveTypeId = db.LeaveTypes.FirstOrDefault(o => o.Name == "LOP").LeaveTypeId;
                        //Extra Leaves than earned leaves will be taken as LOP
                        updateleavebalance = (from leavebalance in db.LeaveBalanceCounts
                                              where leavebalance.UserId == leaveRequestToUpdate.UserId &&
                                              leavebalance.LeaveTypeId == LeaveTypeId &&
                                              leavebalance.Year == year
                                              select leavebalance).FirstOrDefault();

                        if (updateleavebalance == null)
                        {
                            updateleavebalance = db.LeaveBalanceCounts.CreateObject();
                            updateleavebalance.UserId = leaveRequestToUpdate.UserId;
                            updateleavebalance.LeaveTypeId = LeaveTypeId;
                            updateleavebalance.Value = TotalLeaveDays;
                            updateleavebalance.Year = year;
                            db.LeaveBalanceCounts.AddObject(updateleavebalance);
                            db.SaveChanges();

                            updateLeaveDetectionDetails.LOP = TotalLeaveDays;
                            db.SaveChanges();
                        }
                        else
                        {
                            updateleavebalance.Value += TotalLeaveDays;
                            updateleavebalance.Year = year;
                            db.SaveChanges();

                            updateLeaveDetectionDetails.LOP = TotalLeaveDays;
                            db.SaveChanges();

                            TotalLeaveDays = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        public ActionResult GetLOPDays(int leaveRequestId)
        {
            try
            {
                LeaveModel LOPDetails = new LeaveModel();
                var index = "";
                var noofdays = "";
                var leaveDetails = db.LeaveRequests.FirstOrDefault(o => o.LeaveRequestId == leaveRequestId);

                var LeaveTypeList = new List<LeaveBalance>();

                LOPDetails.LOPdays = 0.0;

                DateTime StartDateTime = Convert.ToDateTime(leaveDetails.StartDateTime);
                DateTime EndDateTime = Convert.ToDateTime(leaveDetails.EndDateTime);

                StartDateTime = StartDateTime.AddHours(9);
                EndDateTime = EndDateTime.AddHours(18);

                if (leaveDetails.LeaveTypeId != MasterEnum.LeaveTypes.Comp_Off.GetHashCode() ||
                    leaveDetails.LeaveTypeId != MasterEnum.LeaveTypes.Maternity.GetHashCode())
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
                                     join count in
                                         db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == leaveDetails.UserId)
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


                    var earnedleave = "";
                    var sickleave = "";
                    var casualLeave = "";
                    int SickLeaveCount = 0;
                    int CasualLeaveCount = 0;
                    int EarnedLeaveCount = 0;
                    if (isEligible)
                    {
                        sickleave = db.LeaveBalanceCounts.Where(x => x.UserId == leaveDetails.UserId && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                        if (sickleave != "")
                        {
                            SickLeaveCount = Convert.ToInt32(sickleave);
                            if (SickLeaveCount < 6)
                            {
                                index = "1";
                                noofdays = Convert.ToString(sickleave);
                            }
                            else
                            {
                                casualLeave = db.LeaveBalanceCounts.Where(x => x.UserId == leaveDetails.UserId && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                                if (casualLeave != "")
                                {
                                    CasualLeaveCount = Convert.ToInt32(casualLeave);
                                    if (CasualLeaveCount < 6)
                                    {
                                        index = "2";
                                        noofdays = Convert.ToString(casualLeave);
                                    }
                                    else
                                    {
                                        earnedleave = db.LeaveTypes.Where(x => x.LeaveTypeId == 3).Select(o => o.DaysAllowed).FirstOrDefault().ToString();
                                        if (earnedleave != "")
                                        {
                                            EarnedLeaveCount = Convert.ToInt32(earnedleave);
                                            if (EarnedLeaveCount < 12)
                                            {
                                                index = "3";
                                                noofdays = Convert.ToString(earnedleave);
                                            }
                                            else
                                            {
                                                //index = "7";
                                                index = "1";
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        sickleave = db.LeaveBalanceCounts.Where(x => x.UserId == leaveDetails.UserId && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();
                        if (sickleave != "")
                        {
                            SickLeaveCount = Convert.ToInt32(sickleave);
                            if (SickLeaveCount < 6)
                            {
                                index = "1";
                                noofdays = Convert.ToString(sickleave);
                            }
                            else
                            {
                                casualLeave = db.LeaveBalanceCounts.Where(x => x.UserId == leaveDetails.UserId && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                                if (casualLeave != "")
                                {
                                    CasualLeaveCount = Convert.ToInt32(casualLeave);
                                    if (CasualLeaveCount < 6)
                                    {
                                        index = "2";
                                        noofdays = Convert.ToString(casualLeave);
                                    }
                                    else
                                    {
                                        //index = "7";
                                        index = "1";
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < EligibleLeaveTypes.Count(); i++)
                    {
                        if (EligibleLeaveTypes[i] == 1)
                        {

                            int leavetypeID = Convert.ToInt32(index);
                            LOPDetails.TotalAvailDays =
                                LeaveTypeList.Where(o => o.LeaveTypeId == leavetypeID && o.RemainingDays > 0)
                                    .Sum(o => o.RemainingDays);
                            LOPDetails.totalLeaveDays = 0.0;



                            var leaveTypes = new DSRCManagementSystemEntities1().LeaveTypes.ToList();

                            if (leaveDetails.LeaveDays != null)
                            {
                                LOPDetails.totalLeaveDays = (double)leaveDetails.LeaveDays;
                            }
                            else
                            {
                                LOPDetails.totalLeaveDays = 0.0;
                            }

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

                    }

                }
                return Json(new { Result = LOPDetails.LOPdays }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
        }

        public static List<LeaveBalance> GetLeaveBalance(int Year, int userId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                int userid = db.Users.Where(o => o.UserID == userId).Select(o => o.UserID).FirstOrDefault();
                var doj = db.Users.FirstOrDefault(item => item.UserID == userId).DateOfJoin;
                var chk = db.Users.Where(x => x.UserID == userid).Select(o => o).FirstOrDefault();

                string FinancialYear = Convert.ToString(Year);
                string FinancialYearEnd = Convert.ToString((Year + 1));
                FinancialYearEnd = FinancialYearEnd.Substring(2, 2);
                FinancialYear = FinancialYear + "-" + FinancialYearEnd;

                List<LeaveBalance> LeaveTypeList = null;

                if (chk.Gender == MasterEnum.Genders.Female.GetHashCode() && chk.MaritalStatus == MasterEnum.MaritialStatus.Married.GetHashCode())
                {
                    LeaveTypeList = (from typ in db.LeaveTypes.Where(x => x.LeaveTypeId != 6 && x.LeaveTypeId != 5)
                                     join count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == userid)
                                     on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                     from data in leftjoin.DefaultIfEmpty()
                                     select new LeaveBalance()
                                     {
                                         LeaveTypeId = typ.LeaveTypeId,
                                         LeaveType = typ.Name,
                                         DaysAllowed = typ.DaysAllowed ?? 0,
                                         UsedDays = data.Value ?? 0,
                                         Year = Year,
                                         AcadamicYear = FinancialYear
                                     }).ToList();

                    var MarMat = (from typ in db.LeaveTypes
                                  join count in db.LeaveBalanceCounts.Where(o => o.UserId == userid)
                                    on typ.LeaveTypeId equals count.LeaveTypeId
                                  where typ.LeaveTypeId == 5 || typ.LeaveTypeId == 6
                                  select new LeaveBalance()
                                  {
                                      LeaveTypeId = typ.LeaveTypeId,
                                      LeaveType = typ.Name,
                                      DaysAllowed = typ.DaysAllowed ?? 0,
                                      UsedDays = count.Value ?? 0,
                                      Year = Year,
                                      AcadamicYear = FinancialYear
                                  }).ToList();

                    foreach (LeaveBalance marmat in MarMat)
                    {
                        LeaveTypeList.Add(marmat);
                    }
                }
                else
                {


                    LeaveTypeList = (from typ in db.LeaveTypes.Where(x => x.LeaveTypeId != 6)
                                     join
                                         count in db.LeaveBalanceCounts.Where(o => o.Year == Year && o.UserId == userid) on typ.LeaveTypeId equals count.LeaveTypeId into leftjoin
                                     from data in leftjoin.DefaultIfEmpty()
                                     select new LeaveBalance()
                                     {
                                         LeaveTypeId = typ.LeaveTypeId,
                                         LeaveType = typ.Name,
                                         DaysAllowed = typ.DaysAllowed ?? 0,
                                         UsedDays = data.Value ?? 0,
                                         Year = Year,
                                         AcadamicYear = FinancialYear
                                     }).ToList();
                }
                var t = LeaveTypeList.Where(o => o.LeaveTypeId == 3).Select(o => o).FirstOrDefault();
                return LeaveTypeList;
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        private static List<LeaveBalance> GetLeaveBalance1(int years, int userId)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                int Earned_Leave = MasterEnum.LeaveTypes.Earned_Leave.GetHashCode();
                int Maternity1 = MasterEnum.LeaveTypes.Maternity.GetHashCode();

                var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
                years = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                bool isEligible;
                var doj = db.Users.FirstOrDefault(item => item.UserID == userId).DateOfJoin;
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

                var LeaveTypeList = (from typ in db.LeaveTypes
                                     join count in db.LeaveBalanceCounts.Where(o => o.Year == years && o.UserId == userId)
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

                var t = LeaveTypeList.Where(o => o.LeaveTypeId == Earned_Leave).Select(o => o).FirstOrDefault();
                t.DaysAllowed = isEligible ? t.DaysAllowed : 0;

                var Maternity = LeaveTypeList.Where(o => o.LeaveTypeId == Maternity1).Select(o => o).FirstOrDefault();
                Maternity.DaysAllowed = Maternity.DaysAllowed / 2;

                return LeaveTypeList;
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        public ActionResult BindGrid(int deptmnt, int branch, DateTime strdate, int group)
        {

            var LeaveTypeName = db.LeaveTypes.Take(3).ToList();
            int LoggedInId = (int)Session["UserId"];
            ManualAttendance objAttend = new ManualAttendance();


            DataSet ds = new DataSet();
            try
            {
                var brnch = db.Master_Branches.ToList();
                ViewBag.BranchList = new SelectList(brnch, "BranchID", "BranchName", branch);
                var TimeCount = getTimeCount(strdate.ToString("MM-dd-yyyy"));
                if (TimeCount == 0)
                {

                    ds = getData(strdate.ToString("MM-dd-yyyy"), LoggedInId, branch, deptmnt, group);

                    if (ds != null)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {

                            objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                        select new Manual()
                                                        {
                                                            UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                            UserName = dr["UserName"].ToString(),
                                                            BranchID = Convert.ToString(branch),
                                                            CurrentDate = strdate,
                                                            getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                            IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                            Comments = dr["Comments"].ToString(),
                                                            LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                            LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                            LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                        }).Distinct().ToList();
                        }
                        ds.Dispose();
                    }

                    string LeaveType = "";
                    int count = 0;


                   
                    // var depart = db.Departments.ToList();
                    if (deptmnt == 0)
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                    }
                    else
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                    }

                    if (group == 0)
                    {
                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();
                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                    }
                    else
                    {

                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt
                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();

                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                    }

                    if (objAttend.AttendanceList == null)
                    {
                        return View("ManualAttendance", objAttend);
                    }

                    foreach (var attendee in objAttend.AttendanceList)
                    {
                        LeaveType = Convert.ToString(count++);
                        if (attendee.LeaveTypeId == 0)
                        {
                            ViewBag.LeaveTypeList =
                                new SelectList(
                                    new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                        .Union(
                                            db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                        }
                        else
                        {
                            ViewData[LeaveType] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                attendee.LeaveTypeId);
                        }
                    }
                }


                else if (deptmnt != 0 && branch != 0)
                {

                    ds = getData(strdate.ToString("MM-dd-yyyy"), LoggedInId, branch, deptmnt, group);
                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(strdate);

                    var leave = db.AddHolidays.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt)).Select(o => o.Date).ToList();
                    string weekend = dt.DayOfWeek.ToString();
                    if (weekend == "Saturday" || weekend == "Sunday" || leave.Count > 0)
                    {
                        var timemanagement = db.TimeManagements.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt) && x.BranchId == branch).Select(o => o).ToList();
                        if (leave.Count > 0)
                        {
                            Session["Holiday"] = 1;
                        }
                        if (timemanagement.Count == 0)
                        {
                            if (ds != null)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {

                                    objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                                select new Manual()
                                                                {
                                                                    UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                    UserName = dr["UserName"].ToString(),
                                                                    BranchID = Convert.ToString(branch),
                                                                    CurrentDate = strdate,
                                                                    getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                    IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                    Comments = dr["Comments"].ToString(),
                                                                    LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                    LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                    LeaveTypeName = dr["LeaveTypeNames"].ToString(),

                                                                }).Distinct().ToList();
                                }
                                ds.Dispose();
                            }

                            string LeaveTypeHoliday = "";
                            int countholiday = 0;

                            var departholiday = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                            // var depart = db.Departments.ToList();
                            if (deptmnt == 0)
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                            }
                            else
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                            }

                            if (group == 0)
                            {
                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                            }
                            else
                            {

                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                            }

                            if (objAttend.AttendanceList == null)
                            {
                                return View("ManualAttendance", objAttend);
                            }

                            foreach (var attendee in objAttend.AttendanceList)
                            {
                                LeaveTypeHoliday = Convert.ToString(countholiday++);
                                if (attendee.LeaveTypeId == 0)
                                {
                                    ViewBag.LeaveTypeList =
                                        new SelectList(
                                            new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                                .Union(
                                                    db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                                }
                                else
                                {
                                    ViewData[LeaveTypeHoliday] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                        attendee.LeaveTypeId);
                                }
                            }
                        }
                        else if (ds != null)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                            select new Manual()
                                                            {
                                                                UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                UserName = dr["UserName"].ToString(),
                                                                BranchID = Convert.ToString(branch),
                                                                CurrentDate = strdate,
                                                                getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                Comments = dr["Comments"].ToString(),
                                                                LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                                InTime = System.Convert.ToString(dr["InTime"]),
                                                                OutTime = System.Convert.ToString(dr["OutTime"]),
                                                            }).Distinct().ToList();
                            }
                            ds.Dispose();
                        }

                        string LeaveType = "";
                        int count = 0;

                        var depart = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();


                        if (deptmnt == 0)
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                        }
                        else
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                        }

                        if (group == 0)
                        {
                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();
                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                        }
                        else
                        {

                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt

                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();
                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                        }

                        if (objAttend.AttendanceList == null)
                        {
                            return View("ManualAttendance", objAttend);
                        }

                        foreach (var attendee in objAttend.AttendanceList)
                        {
                            LeaveType = Convert.ToString(count++);
                            if (attendee.LeaveTypeId == 0)
                            {
                                ViewBag.LeaveTypeList =
                                    new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                            .Union(
                                                db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                            }
                            else
                            {
                                ViewData[LeaveType] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                    attendee.LeaveTypeId);
                            }
                        }


                    }

                    else if (ds != null)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {

                            objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                        select new Manual()
                                                        {
                                                            UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                            UserName = dr["UserName"].ToString(),
                                                            BranchID = Convert.ToString(branch),
                                                            CurrentDate = strdate,
                                                            getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                            IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                            Comments = dr["Comments"].ToString(),
                                                            LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                            LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                            LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                            InTime = System.Convert.ToString(dr["InTime"]),
                                                            OutTime = System.Convert.ToString(dr["OutTime"]),
                                                        }).Distinct().ToList();
                        }
                        ds.Dispose();
                    }

                    string LeaveTypeholiday = "";
                    int countholidays = 0;

                    var departholidays = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();


                    if (deptmnt == 0)
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                        ViewBag.DepartmentList = new SelectList(departholidays, "DepartmentId", "DepartmentName");
                    }
                    else
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);
                        //ViewBag.DepartmentList = new SelectList(departholidays, "DepartmentId", "DepartmentName", deptmnt);
                    }

                    if (group == 0)
                    {
                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();
                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                    }
                    else
                    {

                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();
                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                    }

                    if (objAttend.AttendanceList == null)
                    {
                        return View("ManualAttendance", objAttend);
                    }

                    foreach (var attendee in objAttend.AttendanceList)
                    {
                        LeaveTypeholiday = Convert.ToString(countholidays++);
                        if (attendee.LeaveTypeId == 0)
                        {
                            ViewBag.LeaveTypeList =
                                new SelectList(
                                    new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                        .Union(
                                            db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                        }
                        else
                        {
                            ViewData[LeaveTypeholiday] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                attendee.LeaveTypeId);
                        }
                    }
                }
                else if (deptmnt != 0 && branch == 0)
                {


                    ds = getData(strdate.ToString("MM-dd-yyyy"), LoggedInId, branch, deptmnt, group);
                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(strdate);

                    var leave = db.AddHolidays.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt)).Select(o => o.Date).ToList();
                    string weekend = dt.DayOfWeek.ToString();
                    if (weekend == "Saturday" || weekend == "Sunday" || leave.Count > 0)
                    {
                        var timemanagement = db.TimeManagements.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt) && x.BranchId == branch).Select(o => o).ToList();
                        if (leave.Count > 0)
                        {
                            Session["Holiday"] = 1;
                        }
                        if (timemanagement.Count == 0)
                        {
                            if (ds != null)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {

                                    objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                                select new Manual()
                                                                {
                                                                    UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                    UserName = dr["UserName"].ToString(),
                                                                    BranchID = Convert.ToString(branch),
                                                                    CurrentDate = strdate,
                                                                    getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                    IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                    Comments = dr["Comments"].ToString(),
                                                                    LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                    LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                    LeaveTypeName = dr["LeaveTypeNames"].ToString(),

                                                                }).Distinct().ToList();
                                }
                                ds.Dispose();
                            }

                            string LeaveTypeHoliday = "";
                            int countholiday = 0;

                            var departholiday = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                            // var depart = db.Departments.ToList();
                            if (deptmnt == 0)
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                            }
                            else
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                            }

                            if (group == 0)
                            {
                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                            }
                            else
                            {

                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                            }

                            if (objAttend.AttendanceList == null)
                            {
                                return View("ManualAttendance", objAttend);
                            }

                            foreach (var attendee in objAttend.AttendanceList)
                            {
                                LeaveTypeHoliday = Convert.ToString(countholiday++);
                                if (attendee.LeaveTypeId == 0)
                                {
                                    ViewBag.LeaveTypeList =
                                        new SelectList(
                                            new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                                .Union(
                                                    db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                                }
                                else
                                {
                                    ViewData[LeaveTypeHoliday] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                        attendee.LeaveTypeId);
                                }
                            }
                        }
                        else if (ds != null)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                            select new Manual()
                                                            {
                                                                UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                UserName = dr["UserName"].ToString(),
                                                                BranchID = Convert.ToString(branch),
                                                                CurrentDate = strdate,
                                                                getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                Comments = dr["Comments"].ToString(),
                                                                LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                                InTime = System.Convert.ToString(dr["InTime"]),
                                                                OutTime = System.Convert.ToString(dr["OutTime"]),
                                                            }).Distinct().ToList();
                            }
                            ds.Dispose();
                        }

                        string LeaveType = "";
                        int count = 0;
                        var depart = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                        // var depart = db.Departments.ToList();
                        if (deptmnt == 0)
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                            ;
                        }
                        else
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                        }

                        if (group == 0)
                        {
                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();
                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                        }
                        else
                        {

                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt
                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();

                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                        }

                        if (objAttend.AttendanceList == null)
                        {
                            return View("ManualAttendance", objAttend);
                        }

                        foreach (var attendee in objAttend.AttendanceList)
                        {
                            LeaveType = Convert.ToString(count++);
                            if (attendee.LeaveTypeId == 0)
                            {
                                ViewBag.LeaveTypeList =
                                    new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                            .Union(
                                                db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                            }
                            else
                            {
                                ViewData[LeaveType] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                    attendee.LeaveTypeId);
                            }
                        }


                    }
                    else if (ds != null)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {

                            objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                        select new Manual()
                                                        {
                                                            UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                            UserName = dr["UserName"].ToString(),
                                                            BranchID = Convert.ToString(branch),
                                                            CurrentDate = strdate,
                                                            getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                            IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                            Comments = dr["Comments"].ToString(),
                                                            LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                            LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                            LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                            InTime = System.Convert.ToString(dr["InTime"]),
                                                            OutTime = System.Convert.ToString(dr["OutTime"]),
                                                        }).Distinct().ToList();
                        }
                        ds.Dispose();
                    }

                    string LeaveTypes = "";
                    int counts = 0;
                    var departs = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                    // var depart = db.Departments.ToList();
                    if (deptmnt == 0)
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                    }
                    else
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                    }

                    if (group == 0)
                    {
                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();
                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                    }
                    else
                    {

                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt
                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();

                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                    }

                    if (objAttend.AttendanceList == null)
                    {
                        return View("ManualAttendance", objAttend);
                    }

                    foreach (var attendee in objAttend.AttendanceList)
                    {
                        LeaveTypes = Convert.ToString(counts++);
                        if (attendee.LeaveTypeId == 0)
                        {
                            ViewBag.LeaveTypeList =
                                new SelectList(
                                    new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                        .Union(
                                            db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                        }
                        else
                        {
                            ViewData[LeaveTypes] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                attendee.LeaveTypeId);
                        }
                    }
                }
                else if (deptmnt == 0 && branch != 0)
                {

                    ds = getData(strdate.ToString("MM-dd-yyyy"), LoggedInId, branch, deptmnt, group);
                    DateTime dt = new DateTime();
                    dt = Convert.ToDateTime(strdate);

                    var leave = db.AddHolidays.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt)).Select(o => o.Date).ToList();
                    string weekend = dt.DayOfWeek.ToString();
                    if (weekend == "Saturday" || weekend == "Sunday" || leave.Count > 0)
                    {
                        var timemanagement = db.TimeManagements.Where(x => EntityFunctions.TruncateTime(x.Date) == EntityFunctions.TruncateTime(dt) && x.BranchId == branch).Select(o => o).ToList();
                        if (leave.Count > 0)
                        {
                            Session["Holiday"] = 1;
                        }
                        if (timemanagement.Count == 0)
                        {
                            if (ds != null)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {

                                    objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                                select new Manual()
                                                                {
                                                                    UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                    UserName = dr["UserName"].ToString(),
                                                                    BranchID = Convert.ToString(branch),
                                                                    CurrentDate = strdate,
                                                                    getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                    IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                    Comments = dr["Comments"].ToString(),
                                                                    LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                    LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                    LeaveTypeName = dr["LeaveTypeNames"].ToString(),

                                                                }).Distinct().ToList();
                                }
                                ds.Dispose();
                            }

                            string LeaveTypeHoliday = "";
                            int countholiday = 0;

                            var departholiday = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                            // var depart = db.Departments.ToList();
                            if (deptmnt == 0)
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                            }
                            else
                            {
                                var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                      select new
                                                      {
                                                          DepartmentId = p.DepartmentId,
                                                          DepartmentName = p.DepartmentName

                                                      }).OrderBy(x => x.DepartmentName).ToList();


                                ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                            }

                            if (group == 0)
                            {
                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                            }
                            else
                            {

                                var Deparmentgroup = (from d in db.Departments
                                                      join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                      join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                      where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                      select new
                                                      {
                                                          GroupID = dg.GroupID,
                                                          GroupName = dg.GroupName
                                                      }).OrderBy(x => x.GroupName).ToList();
                                ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                            }

                            if (objAttend.AttendanceList == null)
                            {
                                return View("ManualAttendance", objAttend);
                            }

                            foreach (var attendee in objAttend.AttendanceList)
                            {
                                LeaveTypeHoliday = Convert.ToString(countholiday++);
                                if (attendee.LeaveTypeId == 0)
                                {
                                    ViewBag.LeaveTypeList =
                                        new SelectList(
                                            new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }
                                                .Union(
                                                    db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                                }
                                else
                                {
                                    ViewData[LeaveTypeHoliday] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                        attendee.LeaveTypeId);
                                }
                            }
                        }
                        else if (ds != null)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                if (objAttend.AttendanceList != null)
                                {
                                    objAttend.AttendanceList.Clear();
                                }
                                objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                            select new Manual()
                                                            {
                                                                UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                                UserName = dr["UserName"].ToString(),
                                                                BranchID = Convert.ToString(branch),
                                                                CurrentDate = strdate,
                                                                getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                                IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                                Comments = dr["Comments"].ToString(),
                                                                LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                                LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                                LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                                InTime = System.Convert.ToString(dr["InTime"]),
                                                                OutTime = System.Convert.ToString(dr["OutTime"]),
                                                            }).Distinct().ToList();
                            }
                            ds.Dispose();
                        }

                        string LeaveType = "";
                        int count = 0;
                        var depart = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                        // var depart = db.Departments.ToList();
                        if (deptmnt == 0)
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                        }
                        else
                        {
                            var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                                  select new
                                                  {
                                                      DepartmentId = p.DepartmentId,
                                                      DepartmentName = p.DepartmentName

                                                  }).OrderBy(x => x.DepartmentName).ToList();


                            ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                        }

                        if (group == 0)
                        {
                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();
                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                        }
                        else
                        {

                            var Deparmentgroup = (from d in db.Departments
                                                  join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                                  join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                                  where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt
                                                  select new
                                                  {
                                                      GroupID = dg.GroupID,
                                                      GroupName = dg.GroupName
                                                  }).OrderBy(x => x.GroupName).ToList();

                            ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                        }

                        if (objAttend.AttendanceList == null)
                        {
                            return View("ManualAttendance", objAttend);
                        }

                        foreach (var attendee in objAttend.AttendanceList)
                        {
                            LeaveType = Convert.ToString(count++);
                            if (attendee.LeaveTypeId == 0)
                            {
                                ViewBag.LeaveTypeList =
                                    new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }.Union(
                                            db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                            }
                            else
                            {
                                ViewData[LeaveType] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                    attendee.LeaveTypeId);
                            }
                        }

                    }

                    else if (ds != null)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            if (objAttend.AttendanceList != null)
                            {
                                objAttend.AttendanceList.Clear();
                            }
                            objAttend.AttendanceList = (from DataRow dr in ds.Tables[0].Rows
                                                        select new Manual()
                                                        {
                                                            UserID = Convert.ToInt32(dr["UserID"].ToString()),
                                                            UserName = dr["UserName"].ToString(),
                                                            BranchID = Convert.ToString(branch),
                                                            CurrentDate = strdate,
                                                            getCurrentDate = DateTime.Now.ToString("dd/MM/yyyy"),
                                                            IsCurrentDate = strdate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy") ? 1 : 0,
                                                            Comments = dr["Comments"].ToString(),
                                                            LeaveTypeId = Convert.ToInt32(dr["LeaveTypeID"].ToString()),
                                                            LeaveStatusId = Convert.ToInt32(dr["LeaveStatusId"].ToString()),
                                                            LeaveTypeName = dr["LeaveTypeNames"].ToString(),
                                                            InTime = System.Convert.ToString(dr["InTime"]),
                                                            OutTime = System.Convert.ToString(dr["OutTime"]),
                                                        }).Distinct().ToList();
                        }
                        ds.Dispose();
                    }






                    string LeaveTypess = "";
                    int countss = 0;
                    var departss = db.Departments.Where(x => x.IsActive == true && x.BranchID == branch).ToList();

                    // var depart = db.Departments.ToList();
                    if (deptmnt == 0)
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                    }
                    else
                    {
                        var DepartmentList = (from p in db.Departments.Where(x => x.IsActive == true)
                                              select new
                                              {
                                                  DepartmentId = p.DepartmentId,
                                                  DepartmentName = p.DepartmentName

                                              }).OrderBy(x => x.DepartmentName).ToList();


                        ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName",deptmnt);

                    }

                    if (group == 0)
                    {
                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt


                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();
                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName");
                    }
                    else
                    {

                        var Deparmentgroup = (from d in db.Departments
                                              join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                              join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == deptmnt
                                              select new
                                              {
                                                  GroupID = dg.GroupID,
                                                  GroupName = dg.GroupName
                                              }).OrderBy(x => x.GroupName).ToList();

                        ViewBag.DepartmentGroup = new SelectList(Deparmentgroup, "GroupID", "GroupName", group);
                    }

                    if (objAttend.AttendanceList == null)
                    {
                        return View("ManualAttendance", objAttend);
                    }

                    foreach (var attendee in objAttend.AttendanceList)
                    {
                        LeaveTypess = Convert.ToString(countss++);
                        if (attendee.LeaveTypeId == 0)
                        {
                            ViewBag.LeaveTypeList =
                                new SelectList(
                                    new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types" } }.Union(
                                        db.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
                        }
                        else
                        {
                            ViewData[LeaveTypess] = new SelectList(LeaveTypeName, "LeaveTypeId", "Name",
                                attendee.LeaveTypeId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
            string Leavedetails = "";
            ViewData[Leavedetails] = objAttend.AttendanceList;

           
            foreach (var item in objAttend.AttendanceList)
            {
                var OUtOfOfficeUSerID = db.OutOfOfficeDetails.Where(x => (EntityFunctions.TruncateTime(x.ODStartDate) <= strdate || EntityFunctions.TruncateTime(x.ODEndDate) >= strdate)  && x.RequestStatusId == 2).Select(o => o.Userid).ToList();
                if (OUtOfOfficeUSerID != null)
                {
                    item.OutOffice = OUtOfOfficeUSerID;
                }
            }

         
               
          

                foreach (var item in objAttend.AttendanceList)
                {
                    int userstatus = MasterEnum.ManualAttendance.UserStatus.UnderNoticePeriod.GetHashCode();
                    var undernotice = db.Users.Where(x => x.UserID == item.UserID && x.UserStatus == userstatus).Select(o => o.UserStatus).FirstOrDefault();

                    if (undernotice == userstatus)
                    {
                        item.UnderNoticePeriod = "true";
                    }



                }

            return View("ManualAttendance", objAttend);

        }

        [HttpGet]
        public ActionResult dynamiconchange(string Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int ID;
            if (Id == "")
            {
                ID = 1;
            }
            else
            {
                ID = Convert.ToInt32(Id);
            }
            var index = "";
            var noofdays = "";


            var AcadamicEndMonth = db.CalendarYears.Select(o => o.EndingMonth).FirstOrDefault();
            int Year = DateTime.Now.Month <= AcadamicEndMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;

            bool isEligible;
            List<int> EligibleLeaveTypes = new List<int>();

            var dateOfJoin = objdb.Users.Where(x => x.UserID == ID).Select(o => o.DateOfJoin).FirstOrDefault();

            if (dateOfJoin == null)
                isEligible = false;
            else
            {
                var completedDays = (DateTime.Now - dateOfJoin).Value.Days;
                if (completedDays < 365)
                    isEligible = false;
                else
                    isEligible = true;
            }


            var earnedleave = "";
            var sickleave = "";
            var casualLeave = "";
            if (isEligible)
            {


                earnedleave = objdb.LeaveTypes.Where(x => x.LeaveTypeId == 3).Select(o => o.DaysAllowed).FirstOrDefault().ToString();
                index = "3";
                noofdays = Convert.ToString(earnedleave);
            }
            else
            {
                sickleave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 1).Select(o => o.Value).FirstOrDefault().ToString();

                index = "1";
                noofdays = Convert.ToString(sickleave);

                if (sickleave == "")
                {
                    casualLeave = objdb.LeaveBalanceCounts.Where(x => x.UserId == ID && x.LeaveTypeId == 2).Select(o => o.Value).FirstOrDefault().ToString();
                    index = "2";
                    noofdays = Convert.ToString(casualLeave);
                }

            }




            ViewBag.LeaveTypeList =
                                    new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 0, Name = "Select Leave Types1" } }.Union(
                                            objdb.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");
            ViewBag.ActivityName = new SelectList(
                                        new[] { new LeaveType() { LeaveTypeId = 3, Name = "Select Leave Types1" } }.Union(
                                            objdb.LeaveTypes.Take(3).ToList()), "LeaveTypeId", "Name");


            return Json(new { value1 = index, value2 = noofdays }, JsonRequestBehavior.AllowGet);
            //  return Json(ID, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult LeaveCount(string userid)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int UserId = Convert.ToInt32(userid);
            int userids = UserId - 1;
            int LeaveType = Convert.ToInt32(1);
            int leavetypes = 3;

            var value = objdb.LeaveTypes.Where(x => x.LeaveTypeId == LeaveType).Select(o => o.DaysAllowed).FirstOrDefault();

            var leavetaken = objdb.LeaveRequests.Where(x => x.LeaveTypeId == LeaveType && x.UserId == userids).Select(o => o.LeaveDays).ToList();

            int k = 0;
            for (int i = 0; i < leavetaken.Count(); i++)
            {
                k = k + Convert.ToInt32(leavetaken[i]);
            }

            int daysallowed = Convert.ToInt32(value - k);

            return Json(daysallowed, JsonRequestBehavior.AllowGet);

        }

        public DataSet getData(string strdate, int LoggedInId, int branch, int deptmnt, int group)
        {
            try
            {
                DataSet ds = new DataSet();

                SqlConnection con = new SqlConnection();
                con.ConnectionString = ConfigurationManager.AppSettings["connstr"];
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con; //database connection
                cmd.CommandText = "sp_GetManualAttendance"; //  Stored procedure name
                cmd.CommandType = CommandType.StoredProcedure; // set it to stored proc     

                SqlParameter[] objParam = new SqlParameter[5];
                objParam[0] = new SqlParameter("@StrDate", strdate);
                objParam[1] = new SqlParameter("@ReportingUserID", LoggedInId);
                objParam[2] = new SqlParameter("@BranchId", branch);
                objParam[3] = new SqlParameter("@Department", deptmnt);
                objParam[4] = new SqlParameter("@Group", group);
                cmd.Parameters.AddRange(objParam);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                ds.Clear();
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }
        }

        public int getTimeCount(string strdate)
        {
            var count = 0;
            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = ConfigurationManager.AppSettings["connstr"];
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con; //database connection
                cmd.CommandText = "sp_GetManualAttendance"; //  Stored procedure name
                cmd.CommandType = CommandType.StoredProcedure;


                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@Action", "Count");
                objParam[1] = new SqlParameter("@StrDate", strdate);
                cmd.Parameters.AddRange(objParam);
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }


            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                throw ex.GetBaseException();
            }

            return count;
        }




    }
}