using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Template
    {
        [DisplayName("Select Project Name")]
        public int? projectID { get; set; }
        public int? groupID { get; set; }
        public int? UserId { get; set; }
        [Required]
        [DisplayName("Group Name")]
        public string groupName { get; set; }
           public int ColumnId { get; set; }
          [DisplayName("Column Name")]
        public string columnName { get; set; }
        public string ColumnValue { get; set; }
        public string ColumnDisplayName { get; set; }
        [DisplayName("Column Type Id")]
        public int ? columnTypeID { get; set; }
        public string columnTypeName{get;set;}
        public bool IsDefaultTemplate { get; set; }
        public bool IsCustomisedTemplate { get; set; }
        [DefaultValue(true)]
        public bool? IsActive { get; set; }
        public int TimeSheetColumnID { get; set; }
        public string TimeSheetValue { get; set; }
        public TemplateColumnLIst ColumnLIst { get; set; }




        public List<Project> getProjectList()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
          
           var result = from t in db.Projects where t.IsActive == true select t;
            return (result.ToList());
         
        }

        public List<Group> getGroupID(int ? pid)
        {
             DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var result = from t in db.Groups where t.IsActive == true && t.ProjectID==pid select t;
            return (result.ToList());
        }
        public List<Master_ColumnTypes> getColumnTypeID()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var result = from t in db.Master_ColumnTypes select t;
            return (result.ToList());
        }
    }
    public class TemplateColumnLIst
    {
        public List<Column> ColumnList { get; set; }
        public List<Groups> Groups { get; set; }
    }

    public class Column
    {

        public int? ColumnId { get; set; }
        public int? ColumnTypeId { get; set; }
        public string ColumnName { get; set; }
        public string GroupName { get; set; }
        public string ColumnDisplayName { get; set; }
        public string ColumnTypeName { get; set; }
        public bool? IsActive { get; set; }
    }
    public class Groups
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<Column> Columns { get; set; }
    }


    public class EmailTemplateModules
    {
        public int Id { get; set; }
        public string EmailTemplates { get; set; }
    }


}