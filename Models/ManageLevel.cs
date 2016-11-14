using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageLevel
    {
        public int? LevelOrder { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string LevelDescription { get; set; }

    }
}