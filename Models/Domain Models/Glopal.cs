using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models.Domain_Models
{
    public class Glopal
    {
        public static bool IslogoutPressed { get; set; }
        public static bool IsSesionExpired { get; set; }
        public static bool IsPasswordsend { get; set; }
        public static int RollId { get; set; }
        public static bool IsReportingPerson { get; set; }
    }
}