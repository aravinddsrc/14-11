using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class AssignRole
    {
        public string RoleID { get; set; }
        public int ID { get; set; }
        public int userid { get; set; }
        public string Empid { get; set; }
        public List<int> SelectedEmpList { get; set; }
        public string Employees { get; set; }
        public string SelectedEmployees { get; set; }
        public int unuserid { get; set; }
        public string multiselectemployees { get; set; }
        public string unemployees { get; set; }

    }
}