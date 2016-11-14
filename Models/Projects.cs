using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class Projects
    {
        public int ProjectID { get; set; }

        public bool Isactiveorwhat { get; set; }

        public bool Isdelete { get; set; }

        public int? loginmemberid { get; set; }

        public List<string> MemberType { get; set; }
        public List<string> TechList { get; set; }
        public List<string> ORMList { get; set; }
        public List<string> DBList { get; set; }
        public List<string> ThirdPartyList { get; set; }
        public List<string> SourceControlList { get; set; }
        public List<string> ProjectPlan { get; set; }
        public List<string> Phases { get; set; }
        public string CompletedDates { get; set; }
        public int PhaseName {get;set;}
        [DisplayName("Project Name")]
        [Required(ErrorMessage = "Enter Project Name")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9\s_]*$", ErrorMessage = "Project Name Invalid")]
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "Enter Project Code")]
        [DisplayName("Project Code")]
        public string ProjectCode { get; set; }
        [DisplayName("Project Description")]
        public string ProjectDescription { get; set; }
        [DisplayName("Project Type")]
        [ExcludeZero(ErrorMessage = "Select Project Type.")]
        public string ProjectType { get; set; }
        [DisplayName("SVN Repository URL")]
        //[RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", ErrorMessage = "URL format is wrong")]
        //[Required(ErrorMessage = "Enter SVN Repository URL")]
        public string SvnRepositoryUrl { get; set; }
        [DisplayName("Date Created")]
        public DateTime? DateCreated { get; set; }
        [DisplayName("IsActive")]
        public bool? IsActive { get; set; }
        public IQueryable<ProjectMembers> Members { get; set; }
        public int? RAGStatus { get; set; }
        public string RAGComments { get; set; }
        public string Metrics { get; set; }
        public DateTime? CommentsCreated { get; set; }
        public List<string> ProjectTypeLIst { get; set; }

        public string ProjectTypeName { get; set; }
        public int? ProjectTypeID { get; set; }
        public string ManagedResources { get; set; }

        public string CreatedBy { get; set; }

        public int? MemberTypeID { get; set; }

        //[Required(ErrorMessage = "Enter Project Start Date")]
        [Display(Name = "Project Start Date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy}")]
        //[DataType(DataType.DateTime)]
        [DataType(DataType.Date)]
        public DateTime? StartDateTime { get; set; }

        [Display(Name = "Project End Date")]
        //[DataType(DataType.DateTime)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? EndDateTime { get; set; }

        public List<Master_ProjectTypes> GetProjectTypeList()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var projectTypes = from t in db.Master_ProjectTypes select t;

            return (projectTypes.ToList());
        }

        public class ExcludeZero : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    var valueAsString = value.ToString();
                    if (valueAsString.Equals("0"))
                    {
                        var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                        return new ValidationResult(errorMessage);
                    }
                }
                return ValidationResult.Success;
            }
        }
    }
    public class ProjectRAGStatus
    {
        public string ProjectName { get; set; }
        public int ProjectID { get; set; }
        public int? CurrentRAGStatus { get; set; }
        public List<SelectListItem> RAG { get; set; }
        public string RAGStatusComments { get; set; }        
        public string CommentedBy { get; set; }
    }
    public class ProjectMembers
    {
        public string FirstName { get; set; }
        public int MemberTypeID { get; set; }

        public string LastName { get; set; }
    }


    public class MetricHistory
    {
        public int? ProjectId { get; set; }
        public string Metrics { get; set; }
        public DateTime? Date { get; set; }     
    }

    public class ProjectStatus
    {
        public int ProjectID { get; set; }
        public int StatusID { get; set; }
        public string StatusComments { get; set; }
        public DateTime CommentsCreated { get; set; }
        public int CommnetedBy { get; set; }
        public string Commented { get; set; }
    }

    public class RAGHistory
    {
        public int ProjectID { get; set; }
        public int StatusID { get; set; }
        public string StatusComments { get; set; }
        public DateTime CommentsCreated { get; set; }
        public int CommnetedBy { get; set; }
        public string Commented { get; set; }

        public List<ProjectStatus> RAGHistoryList { get; set; }
    }

    public class MileStone
    {
        public int ProjectID { get; set; }
        public int MileStoneID { get; set; }
        public string MileStoneValue { get; set; }
        public DateTime? PhaseStartDate { get; set; }
        public DateTime? PhaseEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public int Numberofdays { get; set; }
        public int ActualNumberofdays { get; set; }

    }

}