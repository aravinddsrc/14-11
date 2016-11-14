using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ReportMOD
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public string sp { get; set; }
        public string roles { get; set; }
        public int rollid { get; set; }
        
        public int SPID { get; set; }
        public List<SelectListItem> SPLIST { get; set; }

        public List<SelectListItem> tipo { get; set; }
        public int RoleID { get; set; }

        public string RoleName { get; set; }
        public int ReportID { get; set; }
        public int MappingID { get; set; }
        public string Parameter { get; set; }
        

     
     
    }
}