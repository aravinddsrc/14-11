using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class ManageServers
    {
        [DisplayName("Location")]
        //public int locationid { get; set; }
        public IList<Viewlocation> location { get; set; }
        public int LocationId { get; set; }
        //public string location { get; set; }
        public string LocationName { get; set; }


        public int ManageServersId { get; set; }
        //public int LocationId { get; set; }
        public int RackNo { get; set; }
        public int AssetId { get; set; }
        public string MachineName { get; set; }
        public string ServerMake { get; set; }
        public string Model { get; set; }
        public string Processor { get; set; }
        public int Memory { get; set; }
        public int HardDisks { get; set; }
        public string Raid { get; set; }
        public string Configurationdetails { get; set; }
        public string OperatingSystem { get; set; }
        public string ServerEdition { get; set; }
        public string NameofProjectsHosted { get; set; }
        //public int locationid { get; set; }
        

    }

    public class Viewlocation
    {
        public int locationid { get; set; }
        public string LocationName { get; set; }
    }
}