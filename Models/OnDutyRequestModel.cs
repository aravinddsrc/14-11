using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class OnDutyRequestModel
    {
       //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yy}")]
        [DisplayFormat(DataFormatString = "{0:ddd, MMM d, yyyy}")]        
        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }
        //  [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yy}")]
        [DisplayFormat(DataFormatString = "{0:ddd, MMM d, yyyy}")]
        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        public int? ODTypeID { get; set; }
          [DisplayName("Out Of Office Type")]
        public string ODType { get; set; }

        public int ODPlaceID { get; set; }
        [DisplayName("Working Place")]
        public string ODPlace { get; set; }
        public int ODID { get; set; }
            [DisplayName("Working Days Count")]
        public int? Workingdays { get; set; }
            [DisplayName("Comments")]
        public string ODComments { get; set; }
        //[Display(Name = "DN_ProfileModel_ContactNo", ResourceType = typeof(Resources.Resource))]
        //[StringLength(10, ErrorMessageResourceName = "VR_ProfileModels_ContactNo_Length", ErrorMessageResourceType = typeof(Resources.Resource), MinimumLength = 10)]
        //[RegularExpression(@"^\d+$", ErrorMessageResourceName = "VR_ProfileModels_ContactNo_Invalid", ErrorMessageResourceType = typeof(Resources.Resource))]
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_ContactNo", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DisplayName("Alternate Contact Number")]
        public string AlternateNo { get; set; }

        public string EmpId { get; set; }
        public string EmpName { get; set; }

        public int ReportingPersonID { get; set; }
        public string ReportingPersonName { get; set; }
          [DisplayName("Request Status")]
        public string RequestStatus { get; set; }
        
        public List<DateTime> WorkingDates { get; set; }

        public int submittingUserId { get; set; }
        public string Employeename { get; set; }
        public int userid { get; set; }
        public int Leaveid { get; set; }
        public string others { get; set; }
        public string emailaddress { get; set; }

        public string reportingemail { get; set; }     

        public List<OnDutyRequestModel> List { get; set; }

        [DisplayName("Manager Comments")]
        public string ManagerComments { get; set; }

        public int? RequestStatusId { get; set; }


        public string ApproverName { get; set; }

        public int? SelectedUserStatusid { get; set; }
        public int? UnderNoticePeriod { get; set; }

    }

    public class OoaNotification
    {
        public int NotifyCount { get; set; }
        public List<OoaNotificationLeaveDetails> Values { get; set; }
    }
    public class OoaNotificationLeaveDetails
    {
        public string UserName { get; set; }
        public DateTime? RequestedDateTime { get; set; }
        public string Time { get; set; }

    }

    public class EmployeeOutDuty
    {
        public string UserName { get; set; }
        public string WorkedDate1 { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Details { get; set; }
        public int OutOfOfficeId { get; set; }
    }


    public class ApplyEmployeeOutDuty
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Comments { get; set; }

        public string Type { get; set; }

        public string EmployeeName { get; set; }
    }



    public class OutOfOfficeNotification
    {
      public int StatusId { get; set; }
      public string EmployeeName{get;set;}
      public string OutType { get; set; }
      public string OutStatus { get; set; }
      public DateTime? StartDate { get; set; }
      public  DateTime?  EndDate {get;set;}
      public string Details { get; set; }
      public int ID { get; set; }
      public int? NoofDays { get; set; }
      public bool IsApproved { get; set; }
    }
    public class NotificatioinReject
    {
        public string RejectedReason { get; set; }

    }


}
