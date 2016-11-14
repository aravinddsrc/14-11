using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class ReportModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        [DisplayName("Report Name")]
        public string ReportName { get; set; }
        public int reportid {get;set;}

        public string ReportQuery { get; set; }
    }
}