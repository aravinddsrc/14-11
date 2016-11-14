using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{


    public class Department
    {
        public string BranchName { get; set; }
        public int DepartmentID { get; set; }
        public int? Idbranchname1 { get; set; }
        public int UID { get; set; }
        public int?  DPID { get; set; }
        public int? GPID { get; set; }
        public string DepartmentName { get; set; }
        public int? GroupID { get; set; }
        public string GroupName { get; set; }
        public string DPName { get; set; }
        public string OBUserName { get; set; }
        public int? ActivityID { get; set; }
        public string ActivityName { get; set; }
        public string ActivityLevel { get; set; }
        public string Activity { get; set; }
        public int? ActivityLevelID { get; set; }
        public string ActivityLevelName { get; set; }
        public string Comment { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ActivityDate { get; set; }
        public List<int> objmodel { get; set; }
        public int? SelectedUserStatusid { get; set; }
        public int GroupTab { get; set; }
        public int Users { get; set; }
        public int UserId { get; set; }
    }



}