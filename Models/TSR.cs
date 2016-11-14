using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Reflection;

namespace DSRCManagementSystem.Models
{

    public class AddPhase
    {
        public string PhaseStatus { get; set; }
        public int? PhaseId { get; set; }
        public string ProjectName { get; set; }
        public string PhaseName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndTime { get; set; }
        public int NumberOfEfforts { get; set; }
        public int alreadyassigned { get; set; }
    }

    public class AddPhaseEdit
    {
        public bool? open { get; set; }
        public int IsOpen { get; set; }
        public int StatusId { get; set; }
        public string ProjectId { get; set; }
        public int PhaseStatus { get; set; }
        public int? PhaseId { get; set; }
        public int ProjectName { get; set; }
        public string PhaseName { get; set; }
        public DateTime? StartDate { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public DateTime? EndTime { get; set; }
        public int NumberOfEfforts { get; set; }
    }

    public class StudentModel
    {
        public bool Issave { get; set; }
        public int? Count { get; set; }
        public string Name { get; set; }
        public List<Taskname> TaskName { get; set; }
        public List<Student> StudentList { get; set; }
        public int? Weekhours { get; set; }
        public string User { get; set; }
        public string StartDate { get; set; }
        public bool Isreject { get; set; }
    }

    public class WeekDetails
    {
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Total { get; set; }
    }
    public class Mulitiselect
    {
        public int AssigntaskPrimary { get; set; }
        public string Employees { get; set; }
    }

   

    public class Student
    {
        // public bool Issave { get; set; }
        public string TaskName { get; set; }
        public int MondayValue { get; set; }
        public DateTime? MondayDate { get; set; }
        public string Monday { get; set; }
        public string Mondayday { get; set; }
        public bool mondayholiday { get; set; }
        public string Tuesdayday { get; set; }
        public bool Tuesdayholiday { get; set; }
        public bool Wednesdayholiday { get; set; }
        public bool Thursdayholiday { get; set; }
        public bool Fridayholiday { get; set; }
        public string Wednesdayday { get; set; }
        public string Thursdayday { get; set; }
        public string Fridayday { get; set; }
        public string Saturdayday { get; set; }
        public string Sundayday { get; set; }
        public int TuesdayValue { get; set; }
        public DateTime? TuesdayDate { get; set; }
        public string Tuesday { get; set; }
        public int WednesdayValue { get; set; }
        public DateTime? WednesDayDate { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public DateTime? ThursDayDate { get; set; }
        public int FridayValue { get; set; }
        public int ThursdayValue { get; set; }
        public DateTime? FridayDate { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
        public DateTime? SaturDayDate { get; set; }
        public string Sunday { get; set; }
        public DateTime? Sundaydate { get; set; }
        public int SaturdayValue { get; set; }
        public int SundayValue { get; set; }
        public int? Total { get; set; }
        public bool MondayLeave { get; set; }
        public int? MondayLeave1 { get; set; }
        public bool TuesDayLeave { get; set; }
        public int? TuesDayLeave1 { get; set; }
        public bool WednesdayLeave { get; set; }
        public int? WednesdayLeave1 { get; set; }
        public bool ThursdayLeave { get; set; }
        public int? ThursdayLeave1 { get; set; }
        public bool FridayLeave { get; set; }
        public int? FridayLeave1 { get; set; }
        public bool MondayOut { get; set; }
        public int? MondayOutValue { get; set; }
        public bool TuesdayOut { get; set; }
        public int? TuesdayOutValue { get; set; }
        public bool WednesdayOut { get; set; }
        public int? WednesdayOutValue { get; set; }
        public bool ThursadyOut { get; set; }
        public int? ThursdayOutValue { get; set; }
        public bool FridayOut { get; set; }
        public int? FridayOutValue { get; set; }
       

    }

    public class Taskname
    {
        public string ProjectName { get; set; }
        public string PhaseName { get; set; }
        public int? id { get; set; }
        public string Task { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Total { get; set; }
        public string UserName { get; set; }
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
        public bool? Isreject { get; set; }
        public bool? Issave { get; set; }
        public int? Approved { get; set; }
        public string ProjectCompleteDetails { get; set; }

    }


    public class taskName
    {
        public int Totalworkingprimarykey { get; set; }
        public string ProjectName { get; set; }
        public string PhaseName { get; set; }
        public int? id { get; set; }
        public string taskname { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Total { get; set; }
        public int? ProjectId { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public bool? Isreject { get; set; }
        public bool? Issave { get; set; }
        public int? Approved { get; set; }
        public string ProjectCompleteDetails { get; set; }

    }



    public class AddTask
    {
        public string User { get; set; }
        public string GeneralTask { get; set; }
    }


    public class ApprovedTimeSheet
    {
        public string EmployeeName { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public string ProjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndTime { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int Multiselect { get; set; }
        public List<User> Userlist { get; set; }
    }

    public class excelsheet
    {
        public string Projects { get; set; }
        public string Months { get; set; }
        public string Weeks { get; set; }
    }
    public class list
    {

        public DateTime? Monday { get; set; }
        public DateTime? Tuesday { get; set; }
        public DateTime? Wednesday { get; set; }
        public DateTime? Thursday { get; set; }
        public DateTime? Friday { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
    }

    public class Rejected
    {
        public string Reason { get; set; }
    }

    public class Isreject
    {
        public string Reason { get; set; }
    }
    public class MultiselectEmployees
    {

        public string Multiselect { get; set; }
    }
   
}