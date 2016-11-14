using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;
using DSRCManagementSystem.Models;

namespace DSRCManagementSystem.Models
{
    public class Profile
    {
        [Display(Name = "DN_ProfileModel_EmpID", ResourceType = typeof(Resources.Resource))]
        public string EmpID { get; set; }
        public int UserID { get; set; }
       // [Required(ErrorMessageResourceName = "VR_ProfileModels_FirstName", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_ProfileModel_FirstName", ResourceType = typeof(Resources.Resource))]
        public string FirstName { get; set; }
        [Display(Name = "DN_ProfileModel_MiddleName", ResourceType = typeof(Resources.Resource))]
        public string MiddleName { get; set; }
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_LastName", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_ProfileModel_LastName", ResourceType = typeof(Resources.Resource))]
        public string LastName { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Display(Name = "DN_ProfileModel_DateofBirth", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "VR_ProfileModels_DateOfBirth", ErrorMessageResourceType = typeof(Resources.Resource))]
        //[DataType(DataType.Date)]
        public string DateOfBirth { get; set; }
        [Display(Name = "DN_ProfileModel_DateofJoin", ResourceType = typeof(Resources.Resource))]
        //public DateTime? DateOfJoin { get; set; }
        [Required(ErrorMessageResourceName = "VR_ProfileModels_DateOfJoin", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string DateOfJoin { get; set; }
        [Display(Name = "DN_ProfileModel_ContactNo", ResourceType = typeof(Resources.Resource))]
      // [StringLength(10, ErrorMessageResourceName = "VR_ProfileModels_ContactNo_Length", ErrorMessageResourceType = typeof(Resources.Resource), MinimumLength = 10)]
       // [RegularExpression(@"^\d+$", ErrorMessageResourceName = "VR_ProfileModels_ContactNo_Invalid", ErrorMessageResourceType = typeof(Resources.Resource))]
      // [Required(ErrorMessageResourceName = "VR_ProfileModels_ContactNo", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ContactNo { get; set; }
        [Display(Name = "DN_ProfileModel_EmailAddress", ResourceType = typeof(Resources.Resource))]
        public string EmailAddress { get; set; }
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_IPAddress", ErrorMessageResourceType = typeof(Resources.Resource))]
        //[Display(Name = "DN_ProfileModel_IPAddress", ResourceType = typeof(Resources.Resource))]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessageResourceName = "VR_ProfileModels_IPAddress_Invalid", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string IPAddress { get; set; }
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_MachineName", ErrorMessageResourceType = typeof(Resources.Resource))]
        //[Display(Name = "DN_ProfileModel_MachineName", ResourceType = typeof(Resources.Resource))]
        public string MachineName { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        //[Projects.ExcludeZero(ErrorMessage = "Select Department Name.")]
        public int? DepartmentId { get; set; }
        //public SelectList DepartmentList { get; set; }  
        [AllowHtml]
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_Skills", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Skills { get; set; }
        public bool IsUnderProbation { get; set; }
        public bool IsUnderNoticePeriod { get; set; }
        //[Required(ErrorMessageResourceName = "VR_ProfileModels_ReportingPerson", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DisplayName("Reporting Person(s)")]
        public List<Int32?> ReportingPerson { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        //public HttpPostedFileBase Photos { get; set; }
        public byte[] Image { get; set; }

        [AllowHtml]
        [DisplayName("Permanent Address 1")]
        [Required(ErrorMessageResourceName = "VR_ProfileModels_Address", ErrorMessageResourceType = typeof(Resources.Resource))]
        public String PermanentAddress { get; set; }
        [DisplayName("Permanent Address 2")]
        public string PermanentAddress2 { get; set; }
        [DisplayName("Permanent Address 3")]
        public string PermanentAddress3 { get; set; }
        [DisplayName("Permanent City")]
        public string PermanentCity { get; set; }
        public int? PermanentCityID { get; set; }
        [DisplayName("Permanent Country")]
        public string PermanentCountry { get; set; }
        public int? PermanentCountryID { get; set; }
        [DisplayName("Permanent State")]
        public string Permanentstate { get; set; }
        public int? PermanentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PermanentPinCode { get; set; }
        
        [AllowHtml]
        [DisplayName("Present Address 1")]
        public string TemporaryAddress { get; set; }
        [DisplayName("Present Address 2")]
        public string PresentAddress2 { get; set; }
        [DisplayName("Present Address 3")]
        public string PresentAddress3 { get; set; }
        [DisplayName("Present City")]
        public string PresentCity { get; set; }
        public int? PresentCityID { get; set; }
        [DisplayName("Present Country")]
        public string PresentCountry { get; set; }
        public int? PresentCountryID { get; set; }
        [DisplayName("Present State")]
        public string Presentstate { get; set; }
        public int? PresentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PresentPinCode { get; set; }




        public bool HasImage { get; set; }
        public int RoleID { get; set; }
        [DisplayName("Office Skype Id")]
        [StringLength(100)]
        public string OfficeSkypeId { get; set; }
       //[Required(ErrorMessageResourceName = "VR_ProfileModels_WorkPlace", ErrorMessageResourceType = typeof(Resources.Resource))]
        public int? WorkplaceId { get; set; }
        
        [DisplayName("Work Place")]
      //[Required(ErrorMessageResourceName = "VR_ProfileModels_WorkPlace", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string WorkPlace { get; set; }
        public int? officeno { get; set; }
        public int? extension { get; set; }

        [DisplayName("Marital Status")]
        public int? Marital { get; set; }

        [DisplayName("Blood Group")]
        public string BloodGroup { get; set; }
        public int? BloodGroupID { get; set; }
        [DisplayName("Father Name")]
        public string FatherName { get; set; }
        [DisplayName("Mother Name")]
        public string MotherName { get; set; }
        [DisplayName("Birth Place")]
        public string BirthPlace { get; set; }
        public string Nationality { get; set; }
        public int? NationalityID { get; set; }
        public string Religious { get; set; }
        public int? ReligiousID { get; set; }
        [DisplayName("Spouse Name")]
        public string SpouseName { get; set; }
        [DisplayName("Number Of Child")]
        public int? NoOfChild { get; set; }
        [DisplayName("Anniversary Date")]
        public DateTime? AnniversaryDate { get; set; }

       // public string work { get; set; }
        [DisplayName("Previous Organization Experience")]
       // [Required(ErrorMessageResourceName = "VR_ProfileModels_Previous_Invalid", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Previous { get; set; }
        [DisplayName("Current Experience")]
        public string CurrentExperience { get; set; }
        [DisplayName("Overall Experience")]
        public string OverallExperience { get; set; }
        [DisplayName("Designation")]
        public string DesignationName { get; set; }
        [DisplayName("Group")]
        public string GroupName { get; set; }
        public int? RegionId { get; set; }
        public int? BranchID { get; set; }
        public int? GenderID { get; set; }
        public string GenderName { get; set; }
        [DisplayName("Branch")]
        public string BranchName { get; set; }
        public string multiselectemployees { get; set; }
        [DisplayName("Emergency Contact Name")]
        public string EmergencyContactName { get; set; }
        [DisplayName("Relationship Name")]
        public int? RelationShipID { get; set; }
        [DisplayName("Relationship Name")]
        public string RelationShipName { get; set; }
        [DisplayName("Emergency Contact Number")]
        public long? Emergency { get; set; }
        public string EMPCODE { get; set; }

    }
    public class ReportingPerson
    {
        public int UserID { get; set; }
        public string Name { get; set; }
    }
    public class ReportablePerson
    {
        public int UserID { get; set; }
        public string Name { get; set; }
    }

}