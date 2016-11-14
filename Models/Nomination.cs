using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class Nomination
    {
        [DisplayName("Training Name ")]
        public string CourseName { get; set; }
          [DisplayName("Employee ID ")]
        public string EmpId { get; set; }
        //public int? EmpId { get; set; }
          [DisplayName("Employee Name ")]
        public string EmpName { get; set; }
        public string ProjectWon { get; set; }
        public string SupervisorId { get; set; }
        //public int? SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public int? InstructorId { get; set; }
        [DisplayName("Instructor Name ")]
        public string InstructorName { get; set; }
        public string Email { get; set; }
        public string Purpose { get; set; }
        public int? TechnologyId { get; set; }
        public string Technology { get; set; }
        public string ProjectName { get; set; }
        public string ContactNo { get; set; }

        public string starttime { get; set; }
        public string endtime { get; set; }

        public int? TrainingID { get; set; }
        public int? NominationCount { get; set; }
        public int? SeatingCapacity { get; set; }
        public int? AvaliableSeats { get; set; }
    }

    public class FeedbackModel
    {
         [DisplayName("Training Name ")]
        public string CourseName { get; set; }

        public string Instructor { get; set; }

         [DisplayName("Training Type ")]

        public string CourseType { get; set; }

        public int? Choice { get; set; }
        public int? Relevance { get; set; }
        public int? AmountofCourse { get; set; }
        public int? AdeqofLearning { get; set; }
        public int? AdeqofPreparation { get; set; }
        public int TrainingId { get; set; }
        public int? EXpofConcepts{ get; set; }

        public int? contentPres { get; set; }
        public int? Timemain { get; set; }

        public int? Quality{ get; set; }
        public int? PrgUseful{ get; set; }
        public string Learn { get; set; }
        public string Suggestions { get; set; }



    }
}