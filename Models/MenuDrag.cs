using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class MenuDrag
    {
        public int FunctionID { get; set; }
        public string FunctionName { get; set; }
        public List<MenuListItem> Children { get; set; }
        public int PreceedanceOrder { get; set; }

     
    }
}