using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Feedback
    {
    
        public int FeedbackId { get; set; }
       
        public int UserID { get; set; }

        public string UserName { get; set; }
      
        public string Feedbacks { get; set; }
      
        public DateTime FeedbackDate { get; set; }

        public int count { get; set; }
    }
}