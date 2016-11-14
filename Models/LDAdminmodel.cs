using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class LDAdminmodel
    {
        [DisplayName("ID")]
        public int TrainingID { get; set; }
         [DisplayName("Training Name")]
        public string TrainingName { get; set; }
         [DisplayName("Technologies")]
        public string Technologies{ get; set; }
         public List<SelectListItem> Technology_list { get; set; }
         public Nullable<int> Technology_id { get; set; }
      
         [DisplayName("Instructor")]
        public string Instructor { get; set; }
         [DisplayName("Nomination")]
        public int? Nomination { get; set; }
         [DisplayName("Status")]
        public string Status { get; set; }
         public Nullable<int> Status_id { get; set; }
         public List<SelectListItem> Status_list{ get; set; }
         public int MonthId { get; set; }
         public string  Month { get; set; }
         [DisplayName("Schedule Date ")]
         public DateTime ?scheduledate { get; set; }
         public string From { get; set; }
         public string To { get; set; }
         public string  dateFrom { get; set; }
         public string dateTo { get; set; }
    }
    public class LDAdminmodelList
    {
      
        public LDAdminmodel LDA{ get; set; }


    }
}