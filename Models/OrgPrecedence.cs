using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class OrgPrecedence
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        [DisplayName("Employee ID")]
        public string EmpID { get; set; }
        [DisplayName("Employee")]
        public string FirstName { get; set; }
        [DisplayName("Department Name")]
        public int DepartmentID { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        [DisplayName("Precedence Order")]
        public int PrecedenceOrder { get; set; }
    }
}