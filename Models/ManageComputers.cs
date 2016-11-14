using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageComputers
    {
        public int ID { get; set; }
         [DisplayName("Manufacturer ")]
      
         
          public string Manufacturer { get; set; }
         
         [DisplayName("CPU ID")]
        // [Remote("CheckExistingCPUID", "AssetManagement", ErrorMessage = "CPUID already exists!")]
       public string CPUID { get; set; }
         
        [DisplayName("Monitor ID")]
    
        public string MonitorID { get; set; }
         
        //[DisplayName("UPS ID")]
        
        //public string UPSID { get; set; }

         [DisplayName("Workstation Operating System")]
        public string OS { get; set; }
         public List<SelectListItem> OSList { get; set; }
         public int OSID { get; set; }
       
    [DisplayName ("Memory (GB)")]
        public string Memory { get; set; }

       
        [DisplayName(" Processor")]
        public string CPU { get; set; }
        public List<SelectListItem> CPUList { get; set; }
        public int CID { get; set; }
       
        public List<SelectListItem> ComputerStatusList { get; set; }
          public int ComputerStatusID { get; set; }
         [DisplayName("Computer Status")]     
        public string ComputerStatus { get; set; }
          
        [DisplayName("Computer Name")]
        
       
        public string ComputerName { get; set; }
        [DisplayName("IP Address")]
       //[RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage="Enter Valid IP Address")]
        public string IP { get; set; }
        public string Actions { get; set; }

        public int Alreadyassigned { get; set; }
       
      
    }
}