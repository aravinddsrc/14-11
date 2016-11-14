using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class Assets
    {
       
        public string Empid { get; set; }
        public string name { get; set; }
        public string Hardwarename { get; set; }
        public List<string> DeptList { get; set; }
        public List<string> LocList { get; set; }
        public List<string> CatList { get; set; }
        public List<string> StatusList { get; set; }
        public List<string> PriorList { get; set; }
        public List<string> MngrList { get; set; }
        public List<string> EmpList { get; set; }
        public List<string> NetworkEmpList { get; set; }
        public List<string> ApprovalStatusList { get; set; }

        public string Description { get; set; }
        public int? DepartmentID { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        public int? LocationID { get; set; }
        public string Location { get; set; }
        public int? EmpID { get; set; }
        [DisplayName("Employee Name")]
        public string EmpName { get; set; }
        [DisplayName("Computer Name")]
        public int? ComputerID { get; set; }
        public string ComputerName { get; set; }
        public int? CategoryID { get; set; }
        public string Category { get; set; }
        public int? StatusID { get; set; }
        public string Status { get; set; }
        public int? ApprovalStatusID { get; set; }
        public string ApprovalStatus { get; set; }
        public int? MngrID { get; set; }
        public string MngrName { get; set; }
        public int? NwEmpID { get; set; }
        public string NwEmpName { get; set; }
        public int? PriorityID { get; set; }
        public string Priority { get; set; }
        [DisplayName("Assigned To")]
        public string AssignedTo { get; set; }
        [DisplayName("Assign To")]
        public string AssignTo { get; set; }
        public int? FirstStageApprovalID { get; set; }
        [DisplayName("First Stage Approval Comments")]
        public string FirstStageApproval { get; set; }
        public int? SecondStageApprovalID { get; set; }
        [DisplayName("Second Stage Approval Comments")]
        public string SecondStageApproval { get; set; }
        public int? NetworkheadID { get; set; }
        [DisplayName("Networking Head")]
        public string Networkheadname { get; set; }

        [DisplayName("Requested Date")]
        public DateTime? RequestedDate { get; set; }

        [DisplayName("Manager Approved Date")]
        public DateTime? FirstStageApproveDate { get; set; }

        [DisplayName("NetworkingHead Approved Date")]
        public DateTime? SecondStageApproveDate { get; set; }

        public int? RequestStatusId { get; set; }


        //Mail Purpose
        public string ReportingPersonName { get; set; }
        public string ReportingPersonEmail { get; set; }


        public int? RequestedId { get; set; }
    }
}