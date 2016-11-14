using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class ManageServer
    {
        public string empid { get; set; }
        public string name { get; set; }
        public int? ID { get; set; }
        public String ProjectName { get; set; }
        public string Location { get; set; }
        public List<SelectListItem> LocationList { get; set; }
        public List<string> NameOfProject { get; set; }        
        public string RackNo { get; set; }       
        public string AssetId { get; set; }      
        public string MachineName { get; set; }       
        public string ServerMake { get; set; }        
        public string Model { get; set; }       
        public string Processor { get; set; }      
        public string Memory { get; set; }       
        public string HardDisks { get; set; }  
        [Display(Name="InUse")]
        public bool? Raid { get; set; }       
        public string Configurationdetails { get; set; }       
        public string OperatingSystem { get; set; }       
        public string ServerEdition { get; set; }       
        public string NameofProjectsHosted { get; set; }
        // public List<string> ProjectList { get; set; }
        public int ProjectId { get; set; }
        public int LocationId { get; set; }
        public List<int> ProjectList { get; set; }
        public string project1 { get; set; }
        public string Assignedto { get; set; }
     //   public string Assignedto { get; set; }
        public List<SelectListItem> AssignedtoList { get; set; }
        public string temp { get; set; }
        public int ManageServersId { get; set; }
        // public string Project { get; set; }
        // public List<SelectListItem> ProjectList { get; set; }
        // public IEnumerable<SelectListItem> ProjectList { get; set; }
                public int LeaveRequestTo { get; set; }

                //public string OsName { get; set; }
                //public List<SelectListItem> OsList { get; set; }
                //public int Osid { get; set; }

                public string CpuName { get; set; }
                public List<SelectListItem> cpulist { get; set; }
                public int CpuIdNew { get; set; }

                public string ServerOsName { get; set; }
                public List<SelectListItem> OsList { get; set; }
                public int ServerOsid { get; set; }
                [DisplayName("Other Projects")]
                public string OtherProjects { get; set; }

    }

    public class ServerDetails
    {
        public string NameOfProjects { get; set; }
        public string AssignedTo { get; set; }
        public string MachineName { get; set; }

        public ServerDetails(string ProjectName, string UserName,string machinename)
        {
            NameOfProjects = ProjectName;
            AssignedTo = UserName;
            MachineName = machinename;
        }
    }


}