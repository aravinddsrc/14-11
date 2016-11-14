using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Windowservice
    {
        public string SNO { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string Template { get; set; }
        public string Timing { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string MailTO { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
    }
}