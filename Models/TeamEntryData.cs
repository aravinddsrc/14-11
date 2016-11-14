using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class TeamEntryData
    {
        public int Ndays { get; set; }
        public string DateTo { get; set; }

        public string DateFrom { get; set; }

        public string MemberId { get; set; }

        public string IsSorting { get; set; }

        public List<HoursWorkData> EmployeeData { get; set; }

        public bool IsTeamData { get; set; }
        public IList<ViewMembers>  ProjectMembersDetails { get; set; }
        public List<TeamMember> MemberList { get; set; }

        public string ListMonth { get; set; }
        public string ProjectID { get; set; }
        //public string ProjectName { get; set; }
        public List<Project> ProjectNames { get; set; }
        //public string UserID { get; set; }
        //public string FirstName { get; set; }
        [Display(Name = "DN_General_ProjectName", ResourceType = typeof(Resources.Resource))]
        public string ProjectName { get; set; }
        public List<SelectListItem> ProjectList { get; set; }
        public IList<ViewMembers> Members { get; set; }

        public int MemberTypeID { get; set; }
        public string MemberType { get; set; }

        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        public string EmployeeName { get; set; }
       public IList<ViewMembers> NonTechMemberDatails { get; set; }

       public int BranchID { get; set; }

    }


    public class HoursWorkData
    {
        public string empName { get; set; }

        public double? hoursWorked { get; set; }

        public string Date { get; set; }

        public string Day { get; set; }

        public string InTime { get; set; }

        public string OutTime { get; set; }
    }


      
    public class TeamMember
    {
        public string MemberId { get; set; }

        public string MemberName { get; set; }
    }
}
