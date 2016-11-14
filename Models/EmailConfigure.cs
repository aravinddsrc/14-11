using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class EmailConfigure
    {
       
        
        public string Host { get; set; }  
        public int Port { get; set; }
     
        public string UserName { get; set; }
        
        public string Password { get; set; }
        
       
        public string ConfirmPassword { get; set; }
      
        public string From { get; set; }
     
        public string To { get; set; }
    }
}