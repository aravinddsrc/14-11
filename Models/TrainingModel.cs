using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class TrainingModel
    {
         [DisplayName("Event Name ")]
        public string EventName { get; set; }

        [DisplayName("Venue Location")]
         public string VenueLocation { get; set; }

        [DisplayName("Event Managed By")]
        public string EventManagedBy { get; set; }
        
        //[DisplayName("")]
    }
}