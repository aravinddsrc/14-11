using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Checklist
    {
        public int CheckListID { get; set; }
        public string CheckListName { get; set; }
        public int CategoryID { get; set; }
        public bool IsActive { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }

}