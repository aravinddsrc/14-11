using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DSRCManagementSystem.Models
{
   
   public class EditTimeEntry
    {
        public bool IsSubmit { get; set; }
        public int UserID { get; set; }
        public string EmployeeId { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public DateTime? Date { get; set; }

        //[Display(Description = "InTime")]
        //[RegularExpression("^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Invalid Time")]        
        public string InTime { get; set; }

        //[Display(Description = "OutTime")]
        //[RegularExpression("^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Invalid Time")]
        public string OutTime { get; set; }

        public int InTimeMin { get; set; }
        public int OutTimeMin { get; set; }
        public int TotalMin { get; set; }
        public List<TeamMember> MemberList { get; set; }
        public string MemberId { get; set; }
        public bool IsTeamData { get; set; }
        public string DateFrom { get; set; }
        public List<HoursWorkData> EmployeeData { get; set; }
        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        public string EmployeeName { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public List<SelectListItem> BranchList { get; set; }
        public IList<ViewMembers> Members { get; set; }

        public bool IsRecordAvail { get; set; }

        public HttpPostedFileBase excelFile { get; set; }

        public string ErrorSuccessMessage { get; set; }

        public string EmpID { get; set; }
        public DateTime Dates { get; set; }

        public List<SelectListItem> BranchLists { get; set; }

        public int BranchIDs { get; set; }
        public string BranchNames { get; set; }
       
    }
   

}
