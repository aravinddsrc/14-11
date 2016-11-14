using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Taskmodel
    {
        public List<AddTaskModel> addtask;
        public List<Assigntaskmodel> assigntask;
        public int newtask { get; set; }
        public int newassigntask { get; set; }
        public int newtaskedit { get; set; }
        public int newassigntaskedit { get; set; }
        public int deletetask { get; set; }
        public int deleteassigntask { get; set; }
       
    }


    public class AddTaskModel
    {
        public bool? IsOpen { get; set; }
        public int alreadyassigned { get; set; }
        public int ID { get; set; }
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }

        [Display(Name = "Phase Status")]
        public string PhaseStatus { get; set; }
        public int? PhaseId { get; set; }

        [Display(Name = "Phase Name")]
        public string PhaseName { get; set; }
        public int? PhaseNameId { get; set; }

        [Display(Name = "Task Name")]
        public string TaskName { get; set; }

        [Display(Name = "Task Status")]
        public string Taskstatus { get; set; }
        public int TaskStatusId { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "No.Of Efforts")]
        public int Efforts { get; set; }

        public string Start { get; set; }
        public string End { get; set; }
        public int Isopened { get; set; }

    }



    public class Assigntaskmodel
    {
        public int ID { get; set; }
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public int? ProjectId { get; set; }
        [Display(Name = "Phase Name")]
        public string PhaseName { get; set; }
        public int PhaseNameId { get; set; }

        [Display(Name = "Task Name")]
        public string TaskName { get; set; }
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "No.Of Efforts")]
        public int Efforts { get; set; }
        public int ProjectphaseId { get; set; }
        public int PhaseId { get; set; }
        public int Taskid { get; set; }
        public int assigntaskid { get; set; }
        public string Empid { get; set; }
        public List<int> SelectedEmpList { get; set; }
        public string Employees { get; set; }
        public string SelectedEmployees { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        public int? NumberOfEfforts { get; set; }

        public string multiselectemployess { get; set; }

        public string taskstatus { get; set; }
        public int? tasktypeid { get; set; }


        public int? userid { get; set; }

      
      
      
       
    }




}





    