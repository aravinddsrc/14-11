using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Skills
    {
        public int Id { get; set; }
        public int RequestStatusId { get; set; }
        public int UserId { get; set; }
        public string UName { get; set; }
        public string Technology { get; set; }
        public int? TechnologyID { get; set; }
        public List<SelectListItem> tech { get; set; }
        public List<SelectListItem> TechIDList { get; set; }

        public int? SpecificationId { get; set; }
        public string Specification { get; set; }
        public List<SelectListItem> speci { get; set; }
        public List<SelectListItem> speciIdList { get; set; }

        public string level { get; set; }
        public int LevelId { get; set; }
        public List<SelectListItem> LevelList { get; set; }
        public List<SelectListItem> LevelIDList { get; set; }
        public List<SelectListItem> ApprovedList { get; set; }

        [Display(Name = "Date Assessed")]
        public String DateAssessed { get; set; }

        public Boolean Primary { get; set; }
        public Boolean Secondary { get; set; }

        [Display(Name = "Last Used")]
        public int? LastUsed { get; set; }
        public int yearid { get; set; }
        public List<SelectListItem> year { get; set; }
        public List<SelectListItem> yearidlist { get; set; }

        public string experiance { get; set; }
        public List<int> objmodel { get; set; }
        public string Specialization { get; set; }

        public int? SkillId { get; set; }
        public string SkillName { get; set; }
        public int? SpecId { get; set; }
        public string SpecName { get; set; }

        public int? techId { get; set; }
        public string techname{get;set;}

        public int Status { get; set; }
        public int ApprovedBy { get; set; }
        public string ApprovedName { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class skilllist
    {
        public List<Skills> skilllists { get; set; }
        public Skills SKL { get; set; }
    }

    public class SearchResultModel
    {
        public string SearchKey { get; set; }
    }

    public class userdetails
    {
        public string empid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime? dob { get; set; }
        public DateTime? doj { get; set; }
        public string email { get; set; }
        public string ip { get; set; }
        public string machinename { get; set; }
        public int? permanentaddress { get; set; }
    }


    public class skilldetail
    {
        public string Technology { get; set; }
        public string Specification { get; set; }
        public int? LastUsed { get; set; }
        public string experiance { get; set; }

    }
}