using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DSRCManagementSystem.Models
{
   

    public class Manual
    {
        
        public int UserID { get; set; }
       
        public string UserName { get; set; }
        public int DeptID { get; set; }
        public string Department { get; set; }
        public string BranchID { get; set; }
        public string Branch { get; set; }
        public DateTime? CurrentDate { get; set; }
        public string getCurrentDate { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public string Comments { get; set; }
        public int ReportingID { get; set; }
        public string TimeEntryPresence { get; set; }
        public int LoginUserId { get; set; }
        public int LeaveRequestSubmitted { get; set; }
        public int IsCurrentDate { get; set; }
        public int LeaveStatusId { get; set; }
        public int LeaveDays { get; set; }
        public int GroupID { get; set; }
        public string chkOnOff { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string UnderNoticePeriod { get; set; }
        public List<int?> OutOffice { get; set; }

    }
    public class ManualAttendance
    {
       
      
        public List<Manual> AttendanceList { get; set; }
       
        
    }

    public class ManualLeave
    {

        public int UserID { get; set; }

        public string UserName { get; set; }
       
        public int BranchID { get; set; }
    
        public string CurrentDate { get; set; }
 
        public int LeaveTypeId { get; set; }
       
        public string Comments { get; set; }
      
        public int LoginUserId { get; set; }

        public int LeaveDays { get; set; }

        public string Presence { get; set; }

        public string chkOnOff { get; set; }

        public string InTime { get; set; }

        public string OutTime { get; set; }

    }

    public class check
    {
        public List<ManualLeave> UserNames { get; set; }
      
    }
   
   
}