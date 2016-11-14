using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class AddDepartment
    {
        public int ID { get; set; }
        public int UID { get; set; }
        public int UIDD { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string GroupName { get; set; }
        public string GroupID { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
     

    }
}