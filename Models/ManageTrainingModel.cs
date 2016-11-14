using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class ManageTrainingModel
    {

        public List<CompletionResultModel> CompletionResult;
        public List<FeedBackResultModel> FeedBackResult;
            [DisplayName("Schedule Date ")]
 
        public DateTime? ScheduledDate { get; set; }

        public int ID { get; set; }
        [DisplayName("Training Name")]
 
        public string TrainingName{ get; set; }
        [DisplayName("Level")]
        public string Level { get; set; }
        public int LevelId { get; set; }
       
        [DisplayName("Technology")]
        public string Technology { get; set; }
        public int TechnologyId { get; set; }
      
        [DisplayName("Schedule Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public string Scheduledate { get; set; }

        [DisplayName("Instructor")]
        public string Instructor { get; set; }
        public int userid { get; set; }
          [DisplayName("Start time")]
        public string Starttime { get; set; }
          public bool test { get; set; }
          [DisplayName("End time")]
          public string Endtime { get; set; }
        [DisplayName("Seats")]
        public int? SeatingCapacity { get; set; }
               [DisplayName("ID")]
        public int? TrainingID { get; set; }

        public string EmpID { get; set; }
        public int? Nominations { get; set; }
        public string MailAddress { get; set; }

        public int? StatusID { get; set; }
          [DisplayName("Status")]
        public string Status { get; set; }
        public string NoNominations { get; set; }
        [Display(Name = "Attendees")]
        public int Attendees { get; set; }

        public bool? IsToday { get; set; }
        public bool? color { get; set; }

        public string NotifyUsers { get; set; }

        public string MailDepartments { get; set; }

    }

    public class CompletionResultModel
    {
          [DisplayName("Training ID")]
        public int? TrainingID { get; set; }
        public string TrainingName { get; set; }
        public string Technology { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
        public int submit { get; set; }
        [Display(Name = "Attendees")]
        public int Attendees { get; set; }
        public int pending { get; set; }
        public bool? IsCompleted { get; set; }

        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public int? Nominations { get; set; }

    }

    public class FeedBackResultModel
    {
        public int? TrainingID { get; set; }
        public string TrainingName { get; set; }
        public string Technology { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
    }
    public class GetEmployeeModel
    {
        public List<GetEmployeesModel> AttendeesResult { get; set; }
        public List<GetEmployeesModel> NonAttendeesResult { get; set; }
    }

    public class GetEmployeesModel
    {
        public int Id { get; set; }
        public int? TrainingId { get; set; }
        [DisplayName("Employee ID")]
        public string EmployeeId { get; set; }
        //public int? EmployeeId { get; set; }
        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; }
        [DisplayName("Department Name")]
        public string Department { get; set; }
        public int  score { get; set; }
        public int? status { get; set; }

        public string FeedbackStatus { get; set; }

    }

    public class TrainingsModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

        public int UserID1 { get; set; }
        public string UserName1 { get; set; }

        public List<int> count { get; set; }
        
    }

}