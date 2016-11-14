using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Organizechart
    {
        public int? id { get; set; }
        public string name { get; set; }
        public int? parent { get; set; }
        public List<Organisationchart> studentlist { get; set; }
    }
    public class Organisationchart
    {
        public byte[] cover { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ReportingUserID { get; set; }
        public string ReporterName { get; set; }
        public string Photo { get; set; }
    }
}