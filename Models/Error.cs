using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Error
    {
         [DisplayName("Employee Name")]
        public string Username { get; set; }
         [DisplayName("Employee ID")]
        public string EmpID { get; set; }
         [DisplayName("ExecptionLog ID")]
        public int ExecptionLogID { get; set; }
         [DisplayName("Date")]
        public DateTime ExecptionDate { get; set; }
         [DisplayName("Exception Message")]
         public string Message { get; set; }
          [DisplayName("Method Name")]
        public string Method {get;set;}
          [DisplayName("Source")]
          public string source { get; set; }
          [DisplayName("StrackTrace")]
          public string strck { get; set; }
          public int userid { get; set; }
       
        
    }
}