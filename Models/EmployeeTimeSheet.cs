using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class EmployeeTimeSheet
    {
        [Display(Name = "DN_General_ProjectName", ResourceType = typeof(Resources.Resource))]
        public string ProjectName { get; set; }
        public List<SelectListItem> ProjectNames { get; set; }
        public int EmployeeId { get; set; }
        [Display(Name = "DN_General_EmployeeName", ResourceType = typeof(Resources.Resource))]
        public string EmployeeName { get; set; }
        public List<SelectListItem> EmployeeNames { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
    }
    public class SelectedEmployeeTimeSheet
    {
        public string ProjectName { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? Date { get; set; }
    }
}