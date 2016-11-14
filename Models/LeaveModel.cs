using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{

    public class LeaveModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Type of Leave is required")]
        [Display(Name = "Type of Leave")]
        public byte LeaveType { get; set; }
        public string LeaveTypeName { get; set; }
        public int LeaveTypeId { get; set; }

        public int UserId { get; set; }
        [Display(Name = "Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy hh:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy hh:mm:ss}")]
        public DateTime EndDateTime { get; set; }


        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy Day}")]
        public DateTime Date { get; set; }
        [Display(Name = "Worked Date (Applicable for Comp Off)")]
        public string WorkedDate1 { get; set; }
        //[RegularExpression(@"^[a-zA-Z][a-zA-Z0-9\s\.\,\-\&\:\;\']*$", ErrorMessage = "Enter valid details")]
        public string Details { get; set; }
        [Display(Name = "Leave Request To")]
        public int LeaveRequestTo { get; set; }
        public string ReportingPersonName { get; set; }
        public string ReportingPersonEmail { get; set; }
        [Display(Name = "Employee Name")]
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string Comments { get; set; }
        public int LeaveStatusId { get; set; }

        [Display(Name = "Hours")]
        public double HoursTaken { get; set; }

        public int AvailableSickLeave { get; set; }
        public int AvailableCasualLeave { get; set; }
        public int AvailbaleEarnedLeave { get; set; }
        public int MyProperty { get; set; }
        public List<LevaeBalance> Balance { get; set; }
        public int LeaveRequestedId { get; set; }
        public int MaximumDays { get; set; }
        public double totalLeaveDays { get; set; }
        public int Minutes { get; set; }
        public double Dayss { get; set; }

        public string UserEmail { get; set; }
        // public double totalLeaveDays { get; set; }

        public bool HalfDay { get; set; }

        public double TotalAvailDays { get; set; }
        public double LOPdays { get; set; }

        public string FullName { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? ProcessedOn { get; set; }

        public int BranchID { get; set; }
        public string BranchName { get; set; }
    }

    public class LevaeBalance
    {
        public int LeaveTypeId { get; set; }
        public string Name { get; set; }
        public double DaysAllowed { get; set; }
        public double UsedDays { get; set; }
        public double RemainingDays
        {
            get
            {
                return DaysAllowed - UsedDays;
            }
        }
    }

    public class AvailLeaveTypes
    {
        public int LeaveTypeId { get; set; }
        public string Name { get; set; }
    }

    public class FilterDepartment
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? BranchId { get; set; }
    }

    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int? CountryId { get; set; }
    }
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int? StateId { get; set; }
    }

}

