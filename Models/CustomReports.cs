using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    

    public class CustomReports
    {

        public int? CustomId { get; set; }
        public int? Id { get; set; }
        public string CustomName { get; set; }
        public string ZoneName { get; set; }
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public List<SelectListItem> RoleList { get; set; }
        public Array ReportName1 { get; set; }
        public string CustomNameId { get; set; }
        public list ZoneName2 { get; set; }
         public list a { get; set; }
         public string CustomNameEx { get; set; }
         public int Description { get; set; }
    }


  
}