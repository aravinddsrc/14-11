using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Income
    {
        public int IncomeID { get; set; }
        public string IncomeDescription { get; set; }     
        public DateTime? IncomeDate { get; set; }
        public double? IncomeAmount { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ScheduleDate { get; set; }

    }
}