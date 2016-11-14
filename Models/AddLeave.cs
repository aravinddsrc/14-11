using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class AddLeave 
    {
        public int LeaveTypeId { get; set; }        
        public string Name { get; set; }
        public int? DaysAllowed { get; set; }

    }
}
