using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace DSRCManagementSystem.Models
{
    public class ManageAssessment
    {
        public int UserID { get; set; }
        public int? AssessmentID { get; set; }
        public string AssessmentName { get; set; }
        public string AssessmentDescription { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime AssessmentDate { get; set; }
        public int? TotalScore { get; set; }
        public int? PassingScore { get; set; }
        public string UserName { get; set; }
        public int? Attendance { get; set; }
        public int? Score { get; set; }
        public string Status { get; set; }
        public List<int> SelectedEmpList { get; set; }
        public List<string> AuthUsers { get; set; }
        public bool InActive { get; set; }
        public int? Idbranchname { get; set; }
        public int? Iddepartment { get; set; }
        public int? Idgroup { get; set; }
        public int? Idbranchname1 { get; set; }
        public int? Iddepartment1 { get; set; }
        public int? Idgroup1 { get; set; }
        public int UID { get; set; }
        public string Branch { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public string UIDS { get; set; }
        public int UserAssessmentID { get; set; }
        
       




    }
}