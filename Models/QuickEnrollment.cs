using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class QuickEnrollment
    {
        public int QuickEnroll { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public long? PhoneNumber { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayName("Date Of Birth")]
        public DateTime DateOfBirth { get; set; }
        //[DataType(DataType.DateTime)]
        [DataType(DataType.Date)]
        [DisplayName("DateOfJoin")]
        public DateTime? DateOfJoin { get; set; }
        public string Experience { get; set; }
        public string ExperienceYear { get; set; }
        public string ExperienceMonth { get; set; }
        public int quick { get; set; }
        public bool flag { get; set; }
        public int? BranchID { get; set; }
       
        public int? BranchName { get; set; }
        public string rollName { get; set; }
        public string PersonalEmailAddress { get; set; }
        public int? RollID { get; set; }
        public int? GenderID { get; set; }
        public DateTime? DateOfEnquiry{get;set;}
        public List<string> DepartmentList { get; set; }
        //public List<SelectListItem> DeptList { get; set; }
        public int DepartmentName { get; set; }
        public int? DepartmentGroup { get; set; }
        //public List<SelectListItem> GroupList { get; set; }
        //public int? GroupId { get; set; }
        [DisplayName("Group")]
        //public string GroupName { get; set; }
        public string Comments { get; set; }
        

        
        
    }
}