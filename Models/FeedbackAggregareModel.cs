using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class FeedbackAggregareModel
    {
        public int? No_Of_Feedbacks { get; set; }    
        public double? ContentRating { get; set; }
        public double? PresentRating { get; set; }
        public double? FacultyRating { get; set; }
        public List<string> Comments { get; set; }
        public List<string> Learn { get; set; }
        public string learntinprog { get; set; }
        public double? OverallRating { get; set; }

    }
}