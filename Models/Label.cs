using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Label
    {
        public int id { get; set; }
        public string LabelName { get; set; }
        public string PreviousName { get; set; }
    }
}