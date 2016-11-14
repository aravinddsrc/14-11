using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class CalendarEventModel
    {
         public int EventId{get;set;}
        [Display(Name="Event Name")]
         public string EventName{get;set;}
        [Display(Name = "Event Description")]
         public string EventDescription{get;set;}
        [Display(Name = "Start Date")]
        public DateTime? StartDate{get;set;}
        [Display(Name = "End Date")]
         public DateTime? EndDate { get; set; }
        [Display(Name = "Start Time")]
         public string StartTime { get; set; }
        [Display(Name = "End Time")]
         public string EndTime { get; set; }
        [Display(Name = "Event Members")]
         public string Members { get; set; }
        [Display(Name = "Event Color")]
         public string ColorCode { get; set; }
         public string CreatedBy { get; set; }
         public string CreatedDate { get; set; }
         public int RecurringID { get; set; }

    }
}