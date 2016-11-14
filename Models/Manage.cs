using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class Manage
    {

        public int RoleID { get; set; }

        public bool Isactiveorwhat { get; set; }

        public bool Isdelete { get; set; }

        public List<string> MemberType { get; set; }
        public List<string> TechList { get; set; }
        public List<string> ORMList { get; set; }
        public List<string> DBList { get; set; }
        public string RoleName { get; set; }
        public string ProjectType { get; set; }
        [DisplayName("IsActive")]
        public bool? IsActive { get; set; }
        public IQueryable<ProjectMembers> Members { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
        





    }




}