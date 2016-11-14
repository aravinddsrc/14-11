using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;
namespace DSRCManagementSystem.Models
{
    public class TimesheetModel
    {
        public int TimeSheetColumnID { get; set; }
        public string TimeSheetColumn { get; set; }
        [DisplayName("Project Name")]
        public string ProjectName { get; set; }
        public List<string> ProjectNames { get; set; }


        public void InsertTimesheet(int TimesheetColumnID, string TimeSheetValue, int UserID, int ProjectID, string Date)
        {
            using (var db = new DSRCManagementSystemEntities1())
            {
                TimesheetModel lstTimeshhetData = new TimesheetModel();
                lstTimeshhetData = db.ExecuteStoreQuery<TimesheetModel>("exec SP_InsertTimesheetData @TimesheetColumnID={0},@TimeSheetValue={1},@UserID={2},@ProjectID={3},@SubmittedDate={4}", TimesheetColumnID, TimeSheetValue, UserID, ProjectID, Date).FirstOrDefault();
            }
        }
    }


    public class ViewTimeSheetModel
    {
        [Display(Name = "DN_General_ProjectName", ResourceType = typeof(Resources.Resource))]
        public string ProjectName { get; set; }
        public List<SelectListItem> ProjectNames { get; set; }
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
    public class SelectedTimeSheet
    {
        public string ProjectName { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? Date { get; set; }
    }
   

}