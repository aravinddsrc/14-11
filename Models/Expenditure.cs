using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Expenditure
    {
        public int ExpenditureID { get; set; }
        public string ExpenseDescription { get; set; }        
        public DateTime? ExpenseDate { get; set; }
        public double? ExpenseAmount { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ScheduleDate { get; set; }
    }
}