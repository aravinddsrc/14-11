using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageHardwareModel
    {
        public int ID { get; set; }
        [Display(Name = "Hardware")]
        public string Hardware { get; set; }
        public List<HardwareList> HardwareListCollection { get; set; }
        public HardwareList HardwareListDetails { get; set; }
    }

    public class HardwareList
    {
        [Display(Name = "Hardware")]
        public string Hardware { get; set; }
        [Display(Name = "Floor")]
        public string Floor { get; set; }
        [Display(Name = "Component Id")]
        public string Component { get; set; }
        [Range(1,2,ErrorMessage="Quantity must be Lesser")]
        public int? Quantity { get; set; }
        [Display(Name = "IP Address")]
        public string Ip { get; set; }
        [Display(Name = "In  Use")]
        public bool? InUse { get; set; }
        public int uid { get; set; }
        public Boolean isdelete { get; set; }
        public int? ID { get; set; }
        public string location { get; set; }
        public int? lid { get; set; }
        public int Id { get; set; }
        [Display(Name = "Model No")]
        public string Model { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }
        public int? mid { get; set; }
        public int? AssignedToId { get; set; }
    }


}