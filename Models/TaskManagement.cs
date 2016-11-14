using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace DSRCManagementSystem.Models
{
    public class TaskManagement
    {
        //[Required(ErrorMessageResourceName = "VR_TaskManagementModels_TaskNameExists", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string TaskDescription { get; set; }
       
        public string AssignedUser { get; set; }
        public int StatusID { get; set; }
        public int TaskID { get; set; }
        public string StatusName { get; set; }
        public int ActionID { get; set; }
        public DateTime AssignedDate { get; set; }
        public int? TaskAssignedToID { get; set; }
        public int AssignedTaskID { get; set; }
        public int? RecurringID { get; set; }
        public string RecurringName { get; set; }
        public string Comments { get; set; }
        public bool InActive { get; set; }
        public int? SelectedUserStatusid { get; set; }
        public string SelectedDays { get; set; }
        public List<string> EditDays { get; set; }

    }
}