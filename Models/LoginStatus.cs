using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class LoginStatus
    {
         public int AuditLoginStatusID { get; set; }
         public string IPAddress { get; set; }
         public string BrowserVersion { get; set; }
         public DateTime? LogedInDate { get; set; }
         public string LoginStatuss { get; set; }
         public int LoginStatusID { get; set; }
    }
}