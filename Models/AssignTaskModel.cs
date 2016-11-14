using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class AssignTaskModel
    {
        public string Task { get; set; }
        public string ProjectName { get; set; }
        public string Start { get; set; }
        public int ID { get; set; }
        public string End { get; set; }
        public int ProjectId { get; set; }
        public int userid { get; set; }
        public int ProjectphaseId { get; set; }
        public int? Taskid { get; set; }
        public string TaskName { get; set; }
        public int? PhaseId { get; set; }
        public string PhaseName { get; set; } 
       public string Empid{get;set;}
       public List<int> SelectedEmpList { get; set; }
        public string Employees { get; set; }
        public string SelectedEmployees { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? NumberOfEfforts { get; set; }
        public int? assigntaskid { get; set; }
        
        public string taskstatus { get; set; }
        public int? tasktypeid { get; set; }
        public int unuserid { get; set; }
        public string multiselectemployees { get; set; }

        public string unemployees { get; set; }
    }
}