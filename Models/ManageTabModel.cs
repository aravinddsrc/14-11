using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageTabModel
    {
        [Display(Name = "User Name")]
        public int UserId { get; set; }
        [Display(Name = "Role Name")]
        public int RoleId { get; set; }
        [Display(Name = "Users/Roles")]
        public string User_Role { get; set; }
        public List<Int32?> Tabs { get; set; }
        public List<Int32?> TabGrids { get; set; }
        public List<SelectListItem> Users { get; set; }
        public List<SelectListItem> Roles { get; set; }

        public string type { get; set; }
        public IEnumerable<SelectListItem> typelist { get; set; }
    }
}