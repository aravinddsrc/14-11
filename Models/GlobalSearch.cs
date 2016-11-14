using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class GlobalSearch
    {
        public string SearchString { get; set; }
        public string SearchResults { get; set; }
    }
    public class SearchListItemModel
    {
        public int UserId { get; set; }
        public FileContentResult Photo { get; set; }
        public int Region { get; set; }
        public string EmpID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfJoin { get; set; }
        public string ContactNo { get; set; }
        public string EmailAddress { get; set; }
        public string DepartmentName { get; set; }
        public string Technology { get; set; }
        public string DesignationName { get; set; }
        public int? GenderID { get; set; }
        public string GenderName { get; set; }
        public string Experience { get; set; }
        public Boolean IsBoarding { get; set; }
        public Boolean Block { get; set; }
        public int ExperienceMonth { get; set; }
        public int ExperienceYear { get; set; }
        public string WorkPlace { get; set; }
        public int WorkplaceId { get; set; }
        public string Marital { get; set; }
        public int MaritalStatusId { get; set; }
        public string Skills { get; set; }
        public int BranchID { get; set; }
        public string Role { get; set; }
        public string ProjectId { get; set; }        
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectType { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }

    }
}