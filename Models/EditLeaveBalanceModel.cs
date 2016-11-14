using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class EditLeaveBalanceModel
    {
        public int UseId { get; set; }
        public string EmployeeId { get; set; }
        public string UserName { get; set; }
        public string TypeName { get; set; }
        //public Dictionary<byte,int> LeaveTypeBalanceValue { get; set; }
        public Dictionary<byte, double> LeaveTypeBalanceValue { get; set; }
        public Dictionary<int, string> LeaveTypeName { get; set; }
        public byte TypeID { get; set; }
        //public int DaysAllowed { get; set; }
        public double DaysAllowed { get; set; }
        //public int UsedDays { get; set; }
        public double UsedDays { get; set; }
        public int? SelectedUserStatusid { get; set; }
        //public int RemainingDays
        public double RemainingDays
        {
            get
            {
                return (DaysAllowed) - UsedDays;
            }
        } 
    }
}

