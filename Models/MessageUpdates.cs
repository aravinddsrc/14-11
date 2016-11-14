using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class MessageUpdates
    {


        public int messageId { get; set; }

        public string messageText { get; set; }

        public bool showComments { get; set; }
        public string Comments { get; set; }

        public commMessageType messageType { get; set; }

    }




    public class ViewMessage
    {
        public int UserId { get; set; }
        public int MessageId { get; set; }
        public string UserName { get; set; }
        public DateTime? MessageinitiatesDate { get; set; }
        public DateTime? MessageValidUpto { get; set; }
        public string Message { get; set; }
        public string Comment { get; set; }
        public bool Isreplyable { get; set; }
        public bool? IsYesOrNo { get; set; }
        public int MessageType { get; set; }

    }


    public class Dashboard
    {
        public List<Userid> objuser { get; set; }

        public List<MessageUpdates> messages { get; set; }
        public List<AbsentDate> AbsentDates { get; set; }
        public List<cleardates> cleardata { get; set; }

        public string Comments { get; set; }

        public string ReplyYes { get; set; }

        public string ReplyNo { get; set; }

        public string ReplyOk { get; set; }
        public IList<Propation> Propation { get; set; }
        public List<NoticePeriod> Noticeperiod { get; set; }
        public List<WorkData> timeWorked { get; set; }
        public List<HoursWorkData> EmployeeData { get; set; }
        public List<JsonData> JsonWorkedData { get; set; }
        public List<ProjectRAGStatus> RAG { get; set; }
        public IEnumerable<LeaveRequest> leaveRequestsResult { get; set; }

        public List<UpcomingTraningModel> upcomingTrainings;
        public List<NominatedTrainingModel> nominatedTrainings;
        public List<HistorytrainingModel> historyTrainings;
        public List<Conductedtrainingmodel> conductedtrainings;
        public List<HistorytrainingModel> unattendedTrainings;
        public List<LDHomeModel> MYL { get; set; }
        public List<SelectListItem> Month { get; set; }


        public List<PageUrl> Pages { get; set; }


    }


    public class Userid
    {
        public int Userids { get; set; }
        public string name { get; set; }
        public string startdate { get; set; }
        public DateTime enddate { get; set; }
        public int days { get; set; }
        //public  List<Data> values { get; set; }
        public List<AbsentDate> AbsentDates { get; set; }
    }

    public class AbsentDate
    {

        public List<string> StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public int days { get; set; }
        public List<cleardates> cleardata { get; set; }
    }

    public class cleardates
    {
        public DateTime? Dates { get; set; }
    }

    //public class Data
    //{
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public int? UserId { get; set; }
    //    public string Name { get; set; }
    //}
    public class NoticePeriod
    {
        public string Name { get; set; }
        public DateTime? ResignedOn { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public string Department { get; set; }
    }

    public class Propation
    {
        public string Name { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string Department { get; set; }
        public string Experience { get; set; }
    }
    public class JsonData
    {
        public string date { get; set; }
        public double? hoursWorked { get; set; }

        public string Day { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string hours { get; set; }
    }
    public class WorkData
    {
        public string empName { get; set; }

        public DateTime Date { get; set; }

        public string EmpId { get; set; }

        public double? hoursWorked { get; set; }

        public double? minsWorked { get; set; }

        public bool IsOutEntry { get; set; }

        public bool IsAbsent { get; set; }

        public string InTime { get; set; }

        public string OutTime { get; set; }

        public int? InTmieMin { get; set; }

        public int? BranchID { get; set; }
    }

    public class UserFeedback
    {
        [AllowHtml]
        [Required(ErrorMessage = "Enter Feedback")]
        public string Feedback { get; set; }

        public string StatusMessage { get; set; }
    }

    public class CalenderEvents
    {
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string Detail { get; set; }
        public string className { get; set; }
    }

    public class AuditLogs
    {
        public string FirstName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime? LogInDate { get; set; }
        public DateTime? LogOutDate { get; set; }
        public string IpAddress { get; set; }
        public string BrowserName { get; set; }
        public string OSName { get; set; }
        public string Location { get; set; }

    }
    public class PageUrl
    {
        public byte? PageModuleId { get; set; }
        public string URL { get; set; }
        public string ModuleName { get; set; }
        public string path { get; set; }
    }
}