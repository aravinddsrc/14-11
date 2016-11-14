using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class LDCourse2
    {

        public DateTime? ScheduledDate { get; set; }


        [DisplayName("Training Name")]
        [Required(ErrorMessage = "Enter Training Name")]

        public string Coursename { get; set; }
        [DisplayName("Level")]


        public string Level { get; set; }
        [Required(ErrorMessage = "Select Level")]
        public int LevelId { get; set; }
        public List<SelectListItem> LevelList { get; set; }
        public List<SelectListItem> LevelIDList { get; set; }
        [DisplayName("Technology")]


        public string Technology { get; set; }
        [Required(ErrorMessage = "Select Technology")]
        public int TechnologyId { get; set; }
        public List<SelectListItem> TechList { get; set; }
        public List<SelectListItem> TechIDList { get; set; }
        [DisplayName("Schedule Date")]
        [Required(ErrorMessage = "Select Schedule Date")]
        public string Scheduledate { get; set; }

        [DisplayName("Instructor")]

        public string Instructor { get; set; }

        [Required(ErrorMessage = "Select Instructor")]
        public int userid { get; set; }
        public List<SelectListItem> InstructorList { get; set; }
        public List<SelectListItem> InstructorIDList { get; set; }
        public int count { get; set; }
        [DisplayName("Upload File")]
        public string UploadFile { get; set; }
        [DisplayName("Sample File")]
        public string SampleFile { get; set; }
        public HttpPostedFileBase ExcelFile { get; set; }
        [DisplayName("Seats")]
        [Required(ErrorMessage = "Enter Seats")]

        public int? SeatingCapacity { get; set; }
        [DisplayName("Start Time")]
        [Required(ErrorMessage = "Select Start Time")]

        public string Starttime { get; set; }
        public string Timeslotst { get; set; }
        [DisplayName("End Time ")]
        [Required(ErrorMessage = "Select End Time")]

        public string Endtime { get; set; }
        public int EndtimeId { get; set; }
        public List<SelectListItem> EndList { get; set; }
        public List<SelectListItem> EndIDList { get; set; }
        public string Timeslotet { get; set; }

        public int? TrainingID { get; set; }

        public string FirstNameAsc { get; set; }





    }
    public class LDCourse2List
    {
        public List<LDCourse2> ldmlist { get; set; }
        public LDCourse2 LDM { get; set; }


        public List<string> NotifyUsers { get; set; }
        public List<string> MailDepartments { get; set; }


    }
}

