using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class LDModel
    {

        public int TrainingId { get; set; }
        public int TrainingTypeId { get; set; }
        public string TrainingName { get; set; }
       // public int TechnologyId { get; set; }
        public string TechnologyName { get; set; }
        public DateTime? ScheduledDate { get; set; }


          [DisplayName("Course Name")]
      
          public string Coursename { get; set; }
         [DisplayName("Level")]
          public string Level { get; set; }
         public int LevelId { get; set; }
         public List<SelectListItem> LevelList { get; set; }
         public List<SelectListItem> LevelIDList { get; set; }
          [DisplayName("Technology")]
          public string Technology { get; set; }
          public int TechnologyId { get; set; }
          public List<SelectListItem> TechList { get; set; }
          public List<SelectListItem> TechIDList { get; set; }
          [DisplayName("Schedule Date")]
          public string Scheduledate{ get; set; }
          [DisplayName("Instructor")]
          public string Instructor { get; set; }
          public int InstructorID { get; set; }
          public List<SelectListItem> InstructorList { get; set; }
          public List<SelectListItem> InstructorIDList { get; set; }
          public int count { get; set; }

    }
    public class mylearning
    {

    }

    public class Learning
    {
        public string TrainingName { get; set; }
        public int? TrainingId { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string  Status { get; set; }
        public int? Score { get; set; }
        public string Level { get; set; }
    }
    public class LDCalendar
    {
        public int? TrainingId { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string className { get; set; }
        public string Detail { get; set; }
        public bool IsNominated { get; set; }
        public bool IsAttended { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class Enrollment
    {
        public int? TrainingId { get; set; }
        public string TrainingName { get; set;}
        
        public DateTime? RegOn { get; set; }
        public string Status { get; set; }
    }

    public class NewTrainings
    {
        public int? TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string Purpose { get; set; }
        public string Type { get; set; }
        public string Level { get; set; }
    }

    public class Language
    {
        public string language { get; set; }
    }

    public class NewTraining
    {
        public int? TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string purpose { get; set; }
        public string type { get; set; }
        public string Level { get; set; }
    }

    public class Detail
    {
        public string Instructor { get; set; }
        public string Venue { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
    }
}