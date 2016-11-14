using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class TimeEntry
    {
        public HttpPostedFileBase excelFile { get; set; }

        public string ErrorSuccessMessage { get; set; }

        public string EmpID { get; set; }
        public DateTime Date { get; set; }

        public List<SelectListItem> BranchList { get; set; }
       
        public int BranchID { get; set; }
        public string BranchName { get; set; }
    }

   
}