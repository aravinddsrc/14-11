       using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class LDCourseModel
    {
    
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

        public  string Scheduledate { get; set; }

        [DisplayName("Instructor")]
        public string Instructor { get; set; }
        public int InstructorID { get; set; }
        public List<SelectListItem> InstructorList { get; set; }
        public List<SelectListItem> InstructorIDList { get; set; }
        public int count { get; set; }
        [DisplayName("Upload File")]
        public string UploadFile { get; set; }
        [DisplayName("Sample File")]
        public string SampleFile { get; set; }
        public HttpPostedFileBase ExcelFile { get; set; }
        public string CreateFolderIfNeeded { get; set; }

     





    }
    public class LDCourseModelList
    {
        public List<LDCourseModel> ldmlist { get; set; }
        public LDCourseModel LDM { get; set; }


    }
}

  