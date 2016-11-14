

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class AssignComputers
    {
        public int ID { get; set; }
        public string Empid { get; set; }
        public string name { get; set; }
        public int AssignId { get; set; }
        [DisplayName("Employee Name ")]
        public string EmployeeName { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        [DisplayName("Department Name")]
        public string Department { get; set; }
        public List<SelectListItem> DepartmentList { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int Managementid { get; set; }
        public bool ISDelete { get; set; }
        public int UserId { get; set; }
        public List<SelectListItem> DepartmentIdList { get; set; }
        [DisplayName("Location Name")]
        public string Location { get; set; }
        public List<SelectListItem> LocationList { get; set; }        
        [DisplayName("Computer Name")]
        public string ComputerName { get; set; }
        public List<SelectListItem> ComputerNameList { get; set; }
       // public List<SelectListItem> ManagementIdList { get; set; }
        [DisplayName("Pendrive Access")]
        public bool? PenDriveAcess { get; set; }
        public List<SelectListItem> PenDriveAcessList { get; set; }
        public string Pendrive { get; set; }
        public int PDAID { get; set; }
        [DisplayName("Extra Devices ")]
        public string ExtraDevices { get; set; }
        public string Actions { get; set; }
        [DisplayName("Workstation Number")]
        public string WorkstationNumber { get; set; }
        [DisplayName("Component Id")]
        public string ComponentId { get; set; }
        public List<int> ComponentList { get; set; }
        public int? CompId { get; set; }
        public List<SelectListItem> ComponentIdList { get; set; }
        public int? assetid { get; set; }
        public int AID { get; set; }
        public string DepartmentName { get; set; }
        public string Departmentvalue { get; set; }
        [DisplayName("UPS ID")]
        public string UPSID { get; set; }
        public int UpsId { get; set; }
        public List<int> UpsList { get; set; }
        public string Component { get; set; }
        public int Multiselect { get; set; }
    }


    public class AssignComputersNew {
        public int ID { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; }
        public string Departmentvalue { get; set; }
        public int LocationId { get; set; }
        public int Managementid { get; set; } 
        public string ComputerName { get; set; }
        public string Pendrive { get; set; }
        public string UPSID { get; set; }
        public string WorkstationNumber { get; set; }
        public string ComponentId { get; set; }
   
    }

    public class FilterComponent
    {
        public int ComponentId { get; set; }
        public string CompDescription { get; set; }
        public int? ComputerName { get; set; }
    }

}