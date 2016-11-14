using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class GetDropdownListForCourseDetails
    {
        public List<SelectListItem> LevelList { get; set; }
        public List<SelectListItem> TechList { get; set; }
        public List<SelectListItem> InsList { get; set; }
    }
}