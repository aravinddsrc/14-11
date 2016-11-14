using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class AssignCheckList
    {
        public string GridID { get; set; }
        public string CheckID { get; set; }
        public int? ProjectId { get; set; }
        public Boolean? IsChecked { get; set; }
        public string CategoryName { get; set; }
        public int? project { get; set; }
        public int? Checklistmapping { get; set; }
        public string CheckListItems { get; set; }
        public int? CategoryID { get; set; }
        public int CheckListID { get; set; }
        public string UnProjectId { get; set; }
        public int? UnGridID { get; set; }
        public int UnCheckID { get; set; }
        // public List<string> GridList { get; set; }
        public List<string> WholeList { get; set; }
        public string CheckList { get; set; }
        public List<AssignCheckList> Gridlist { get; set; }
        public List<AssignCheckList> catcheck { get; set; }
        public List<AssignCheckList> objmodel { get; set; }
        public List<int?> checkedlist { get; set; }
        public string Projectname { get; set; }
         

    }
    public class Assign
    {
        public string CategoryName { get; set; }
       
    }
   
    public class AssignCheckList1
    {
        public string GridID { get; set; }
        public string CheckID { get; set; }
        public string UnGridID { get; set; }
        public string UnCheckID { get; set; }
    }
    public class Listing
    {
        public List<AssignCheckList1> Check { get; set; }
        public List<AssignCheckList1> UnCheck { get; set; }

    }
   
}