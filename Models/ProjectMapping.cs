//Model Name     :ProjectMapping
//Purpose        :Assign And View Project Members
//Date Created   :20-02-2015
//Created By     :Balaji.S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class ProjectMapping
    {
        public List<string> MembersList { get; set; }
        public string Project { get; set; }
        public int ProjectID { get; set; }
        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        public string EmployeeName { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        [Display(Name = "DN_General_ProjectName", ResourceType = typeof(Resources.Resource))]
        public string ProjectName { get; set; }
        public List<SelectListItem> ProjectList { get; set; }
        //[Display(Name = "DN_General_RoleName", ResourceType = typeof(Resources.Resource))]
        //public string RoleName { get; set; }
        [DisplayName("Resource Type")]
        public int MemberTypeID { get; set; }
        public List<SelectListItem> RoleList { get; set; }
        public IList<ViewMembers> Members { get; set; }
        [Display(Name = "DN_General_IsBillableResource", ResourceType = typeof(Resources.Resource))]
        public bool IsBillableResource { get; set; }
        public bool IsBuffer { get; set; }
        public bool IsUnassigned { get; set; }
        public bool OnBoarding { get; set; }
        public List<string> DepartmentList { get; set; }
        public string Resources { get; set; }
        public bool ManagedResources { get; set; }

        public bool BillableResources { get; set; }

        public bool AdditionalBufferResources { get; set; }

        public int UserProjectID { get; set; }

       // public List<int> MemberTypeList { get; set; }

        public string Multiselectedvalues { get; set; }

        public List<int> Id7 { get; set; }
        //public List<int> EmployeeList { get; set; }

        public int value { get; set; }

        public int multiselect { get; set; }

        public string SelectedResources { get; set; }

        public List<string> MemberTypeList { get; set; }

        public string SelectedProject { get; set; }

        public bool? InActive { get; set; }

    }
    public class DSRCEmployees
    {
        public int? UId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string EmployeeId { get; set; }
        public int BranchID { get; set; }
        public int DepartmentId { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
    }
    public class DSRCProjects
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
    public class Roles
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
    public class MemberTypes
    {
        public int MemberTypeID { get; set; }
        public string MemberType { get; set; }
    }
    public class AssignedMembers
    {
        public int ProjectId { get; set; }
        public List<string> UserId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
    public class ViewMembers
    {
        public int MonthId { get; set; }
        public string EmployeeName { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int UserProjectId { get; set; }
        public int? SelectedUserStatusid { get; set; }
        //public int MonthId { get; set; }


        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int MemberTypeID { get; set; }
        public string MemberType { get; set; }
        public bool IsBillable { get; set; }
        public bool? onboarding { get; set; }
        public bool? IsUnderNoticePeriod { get; set; }
        public double TotalWorkingHours { get; set; }
    }
    public class EditUser
    {
        public int UserProjectId { get; set; }
        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        public string EmployeeName { get; set; }
        public int UserId { get; set; }
        [Display(Name = "DN_General_ProjectName", ResourceType = typeof(Resources.Resource))]
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        /*[Display(Name = "DN_General_RoleName", ResourceType = typeof(Resources.Resource))]
        public string RoleName { get; set; }
         public int RoleId { get; set; }*/
        [DisplayName("Member Type")]
        public string MemberType { get; set; }
        public int MemberTypeID { get; set; }
        public List<SelectListItem> RoleList { get; set; }
        [Display(Name = "DN_General_IsBillableResource", ResourceType = typeof(Resources.Resource))]
        public bool IsBillableResource { get; set; }
    }
    public class Reporting
    {
        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        [Projects.ExcludeZero(ErrorMessage = "Select Employee Name.")]
        public int EmployeeId { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        [DisplayName("Reporting Person(s)")]
        [Required(ErrorMessageResourceName = "VR_ProfileModels_ReportingPerson", ErrorMessageResourceType = typeof(Resources.Resource))]
        public List<Int32?> ReportingPerson { get; set; }

        public string Reportingtype { get; set; }
        public IEnumerable<SelectListItem> Items { get; set; }
    }

    public class ProjectMeetingTime
    {
        public int? DayId { get; set; }
        public string Day { get; set; }
        public string Date { get; set; }
        public int? ProjectNameId { get; set; }
        public string ProjectName { get; set; }
        public string TimeSlotFrom { get; set; }
        public string TimeSlotTo { get; set; }
        public int? AttendeeId { get; set; }
        //public string Attendee { get; set; }
        public string Attendee { get; set; }
        public string Agenda { get; set; }
        //public List<int> Attendee { get; set; }
        public int? Week { get; set; }
        public string FeedBack { get; set; }
    }

    public class MeetingSchedule
    {
        public int? Id { get; set; }
        public string View { get; set; }
        public int DayId { get; set; }
        public int? WeekDropDown { get; set; }
        public int? Week { get; set; }
        public string Day { get; set; }
        public string Date { get; set; }
        public string Project { get; set; }
        public int? ProjectID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Attendees { get; set; }
        public string Agenda { get; set; }
        public string Feedback { get; set; }
        public string MOM { get; set; }

        public List<int> AttendeeList { get; set; }
    }

    public class ProjectFeedBack
    {
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public string Feedback { get; set; }
        public string Date { get; set; }
    }
    public class AgandaForProject
    {
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public string  ProjectAganda { get; set; }
        public string Date { get; set; }

        public int? id { get; set; }
    }
    public class ProjectMom
    {
        public string Name { get; set; }
        public string schedule { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? UserId { get; set; }
        public string UserIds { get; set; }
        public string ProjectMOM { get; set; }
        public string Date { get; set; }
        public string Attendee { get; set; }
    }
    public class Historylist
    {
        public int?  ProjectId { get; set; }
        public string agenda { get; set; }
        public string feedback { get; set; }
        public string MOM { get; set; }
    }
    public class AddAgenda
    {
        public string ProjectAgenda { get; set; }
        public string Date { get; set; }
    }

    public class AddFeedback
    {
        public string ProjectFeedback { get; set; }
        public string Date { get; set; }
    }
    public class History
    {
        public DateTime? Date { get; set; }
        public string Agenda { get; set; }
        public string Feedback { get; set; }
        public string MOM { get; set; }
    }
    public class  CompletedTraining
    {

        public int TrainingId { get;set; }
        public string Name  {get;set;  }
        public DateTime? Schedule {get;set; }
        public string  Nomination {get;set; }
        public string Attendess { get; set;  }
        public string Unattendess { get; set; }
        public string upload { get; set;  }
        public string ScheduleDate { get; set; }
    }
}

