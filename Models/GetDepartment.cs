using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{


    public class GetDepartment
    {

        public int DepartmentID { get; set; }
        public int UID { get; set; }
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
        public List<GetDepartment> Department { get; set; }
        public List<int> objmodel { get; set; }
    }



}