using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageGrid
    {
       
            [Display(Name = "User Name")]
            public int UserId { get; set; }
            [Display(Name = "Role Name")]
            public int RoleId { get; set; }
            [Display(Name = "Users/Roles")]
            public string User_Role { get; set; }
            public List<Int32?> Grids { get; set; }
            public List<SelectListItem> Users { get; set; }
            public List<SelectListItem> Roles { get; set; }
            public string type { get; set; }
            public IEnumerable<SelectListItem> typelist { get; set; }
            public List<values> GridUsers { get; set; }
            public string UserName { get; set; }
            public int? Nofcount { get; set; }
            public int? NoofUsers { get; set; }
            

    }
    public class ManageTabs
    {
        public string TabName { get; set; }
        public Boolean IsActive { get; set; }
        public int TabId { get; set; }
        public Boolean IsChecked { get; set; }
        public List<values> TaskName { get; set; }
        public List<values> TabUsers { get; set; }
        public int GridId { get; set; }
        public string GridName { get; set; }
        public int? Nofcount { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int? NoofUsers { get; set; }
       
    }
    public class values
    {
        public string TabName { get; set; }
        public Boolean IsActive { get; set; }
        public int? TabId { get; set; }
        public Boolean IsChecked { get; set; }
       // public List<Taskname> TaskName { get; set; }
        public int? GridId { get; set; }
        public string GridName { get; set; }
        public int? Nofcount { get; set; }
        public string UserName { get; set; }
        public int? UserId { get; set; }
        public int? NoofUsers { get; set; }

    }

    public class ManageTabWidget
    {
      
        public string TabName { get; set; }
        public Boolean IsActive { get; set; }
        public int TabId { get; set; }
        public Boolean IsChecked { get; set; }
        public List<values> TaskName { get; set; }
        public List<values> TabUsers { get; set; }
        public int GridId { get; set; }
        public string GridName { get; set; }
        public int? Nofcount { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int? NoofUsers { get; set; }
        public int RoleId { get; set; }
        public List<values> GridUsers { get; set; }
       
    }
}