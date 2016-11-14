using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class MenuListItem
    {
        public String MenuName { get; set; }
        public List<MenuListItem> Children { get; set; }
        public String MenuIcon { get; set; }
        public String Url { get; set; }
    }
}