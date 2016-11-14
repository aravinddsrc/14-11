using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Reflection;

namespace DSRCManagementSystem.Models
{
    public class Node
    {
        public string NodeId { get; set; }
        public string Name { get; set; }
        public string ParentNodeId { get; set; }
        public string Tooltip { get; set; }
    }


    public class UserModel
    {
        public int? RegionId { get; set; }
        public int? quick { get; set; }
        public bool flag { get; set; }
        public int IsNew { get; set; }
        public int UserId { get; set; }
        [DisplayName("Employee ID")]
        public string EmpID { get; set; }    
        [DisplayName("First Name")]
        [Required(ErrorMessage="Enter First Name")]
        public string FirstName { get; set; }
        [DisplayName("Middle Name")]
        public string MiddleName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayName("Date Of Birth")]
    //    [Required(ErrorMessage="Select Date of Birth")]
        public DateTime? DateOfBirth { get; set; }
        //[DataType(DataType.DateTime)]
        [DataType(DataType.Date)]
        [DisplayName("Date Of Join")]        
     //   [Required(ErrorMessage="Select Date of Join")]
        public DateTime? DateOfJoin { get; set; }
        [DisplayName("Mobile Number")]   
      //  [Required(ErrorMessage="Enter Contact No")]
        public long? ContactNo { get; set; }        
        public string Email { get; set; }
        [Display(Name = "Email Address")]
    //   [RegularExpression(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$", ErrorMessage = "Please Enter Correct Email Address")]
        [DataType(DataType.EmailAddress,ErrorMessage="hisdfdjhf")]
        [DisplayName("Email Address")]        
        public string EmailAddress { get; set; }
        [DisplayName("IP Address")]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$",ErrorMessage="Invalid IP Address")]
        public string IPAddress { get; set; }
        [DisplayName("Machine Name")]        
        public string MachineName{ get; set; }
        public string DirectReportingTo { get; set; }
        [DisplayName("Is UnderProbation")]        
        public bool IsUnderProbation { get; set; }
        [DisplayName("Is UnderNoticePeriod")]        
        public bool IsUnderNoticePeriod { get; set; }
        [DisplayName("Is FirstLogin")]        
        public bool IsFirstLogin { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastAccessed { get; set; }                
        public bool IsActive { get; set; }        
        public int? DepartmentId { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        public int? ID { get; set; }
        public string Tecnology { get; set; }        
        public int? RoleID { get; set; }
        public string RoleName { get; set; }
        [DisplayName("Designation")]
        public int? DesignationID { get; set; }
        [DisplayName("Designation")]
        public string DesignationName { get; set; }
        [DisplayName("Gender")]
        public int? GenderID { get; set; }
        public string Gender { get; set; }
        public int? PAddress{ get; set; }
        public int? TAddress { get; set; }
        [DisplayName("Resigned On")]
        public DateTime? ResignedOn { get; set; }
        [DisplayName("Last Working Date")]
        public DateTime? LastworkingDate { get; set; }
        public string Experience { get; set; }
        public Boolean IsBoarding { get; set; }
        public Boolean Block { get; set; }
        //public int ExperienceMonth { get; set; }
        //public int ExperienceYear { get; set; }
        public string ExperienceMonth { get; set; }
        public string ExperienceYear { get; set; }
        public bool InActive { get; set; }
        public List<SelectListItem> DeptList  { get; set; }
        public List<string> TechList { get; set; }
        [DisplayName("Work Place")]
        public int WorkplaceId { get; set; }
        public string WorkPlace { get; set; }        
        [DisplayName("Marital Status")]
        public string Marital { get; set; }
        public int? MaritalStatusId { get; set; }
        public bool NewJoinee { get; set; }
        public bool NoticePeriod { get; set; }
        public bool NotPerformingGood { get; set; }
        public List<int> EmployeeId { get; set; }
        public List<string> DepartmentList { get; set; }
        public int BranchID { get; set; }
        [DisplayName("Branch")]
        public string BranchName { get; set; }
        public int Employees { get; set; }
        public bool onboarding { get; set; }
        public string multiselectemployees { get; set; }
        public List<int> SelectedEmpList { get; set; }
        public string SelectedEmployees { get; set; }
        public List<SelectListItem> EmployeeList { get; set; }
        [DisplayName("Office Skype Id")]
        public string OfficeSkypeId { get; set; }
        [DisplayName("Office No")]
        public int? OfficeNo { get; set; }
        public int? Extension { get; set; }
        [DisplayName("Permanent Address")]
        public int? PermanentAddress { get; set; }
        public int? GroupId { get; set; }
        [DisplayName("Group")]
        public string GroupName { get; set; }
        public int? Attempts { get; set; }
        public string SearchString { get; set; }
        public string Flagsubmit { get; set; }
         [DisplayName("UserStatus")]
        public int? SelectedUserStatusid { get; set; }
        public int? SearchedUserStatusid { get; set; }

        //New Columns Added
        public string QAddress { get; set; }
        public int? RollID { get; set; }
        public string RollName { get; set; }
        [DisplayName("Blood Group")]
        public string BloodGroup { get; set; }
        public int BloodGroupID { get; set; }
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
        [DisplayName("Present Address 1")]
        public string PresentAddress1 { get; set; }
        [DisplayName("Present Address 2")]
        public string PresentAddress2 { get; set; }
        [DisplayName("Present Address 3")]
        public string PresentAddress3 { get; set; }
        [DisplayName("Present City")]
        public string PresentCity { get; set; }
        public int PresentCityID { get; set; }
        [DisplayName("Present Country")]
        public string PresentCountry { get; set; }
        public int? PresentCountryID { get; set; }
        [DisplayName("Present State")]
        public string Presentstate { get; set; }
        public int PresentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PresentPinCode { get; set; }
        [DisplayName("Permanent Address 1")]
        public string PermanentAddress1 { get; set; }
        [DisplayName("Permanent Address 2")]
        public string PermanentAddress2 { get; set; }
        [DisplayName("Permanent Address 3")]
        public string PermanentAddress3 { get; set; }
        [DisplayName("Permanent City")]
        public string PermanentCity { get; set; }
        public int PermanentCityID { get; set; }
        [DisplayName("Permanent Country")]
        public string PermanentCountry { get; set; }
        public int? PermanentCountryID { get; set; }
        [DisplayName("Permanent State")]
        public string Permanentstate { get; set; }
        public int? PermanentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PermanentPinCode { get; set; }
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

    public class UpdateUser
    {
        public int Region { get; set; }
        [DisplayName("Employee ID")]
        public string EmpID { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Middle Name")]
        public string MiddleName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DisplayName("Date Of Birth")]
        public string DateOfBirth { get; set; }
        [DisplayName("Date Of Join")] 
        public string DateOfJoin { get; set; }
        [DisplayName("Mobile Number")]   
        public long? ContactNo { get; set; }
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        public string Tecnology { get; set; }
        [DisplayName("Designation")]
        public string DesignationName { get; set; }
        public int? GenderID { get; set; }
        [DisplayName("Gender")]
        public string GenderName { get; set; }
        public string Experience { get; set; }
        public Boolean IsBoarding { get; set; }
        public Boolean Block { get; set; }
        public int ExperienceMonth { get; set; }
        public int ExperienceYear { get; set; }
        [DisplayName("Work Place")]
        public string WorkPlace { get; set; }
        public int WorkplaceId { get; set; }
        [DisplayName("Marital Status")]
        public string Marital { get; set; }
        public int MaritalStatusId { get; set; }
        public string Skills { get; set; }
        public int BranchID { get; set; }
        [DisplayName("Branch")]
        public string BranchName { get; set; }


        //New Columns Added
        public string QAddress { get; set; }
        public int? RollID { get; set; }
        public string RollName { get; set; }
        [DisplayName("Blood Group")]
        public string BloodGroup { get; set; }
        public int BloodGroupID { get; set; }
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
        [DisplayName("Present Address 1")]
        public string PresentAddress1 { get; set; }
        [DisplayName("Present Address 2")]
        public string PresentAddress2 { get; set; }
        [DisplayName("Present Address 3")]
        public string PresentAddress3 { get; set; }
        [DisplayName("Present City")]
        public string PresentCity { get; set; }
        public int PresentCityID { get; set; }
        [DisplayName("Present Country")]
        public string PresentCountry { get; set; }
        public int? PresentCountryID { get; set; }
        [DisplayName("Present State")]
        public string Presentstate { get; set; }
        public int PresentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PresentPinCode { get; set; }
        [DisplayName("Permanent Address 1")]
        public string PermanentAddress1 { get; set; }
        [DisplayName("Permanent Address 2")]
        public string PermanentAddress2 { get; set; }
        [DisplayName("Permanent Address 3")]
        public string PermanentAddress3 { get; set; }
        [DisplayName("Permanent City")]
        public string PermanentCity { get; set; }
        public int PermanentCityID { get; set; }
        [DisplayName("Permanent Country")]
        public string PermanentCountry { get; set; }
        public int? PermanentCountryID { get; set; }
        [DisplayName("Permanent State")]
        public string Permanentstate { get; set; }
        public int? PermanentstateID { get; set; }
        [DisplayName("PinCode")]
        public int? PermanentPinCode { get; set; }
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

    public class SaveUser
    {
        public int Region { get; set; }
        public string EmpID { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Middle Name")]
        public string MiddleName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DisplayName("Date Of Birth")]
        public string DateOfBirth { get; set; }
        [DisplayName("Date Of Join")] 
        public string DateOfJoin { get; set; }
        [DisplayName("Mobile Number")]   
        public long? ContactNo { get; set; }
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [DisplayName("Department Name")]
        public string DepartmentName { get; set; }
        public string Tecnology { get; set; }
        [DisplayName("Designation")]
        public string DesignationName { get; set; }
        public int? GenderID { get; set; }
        [DisplayName("Gender")]
        public string GenderName { get; set; }
        public string Experience { get; set; }
        public Boolean IsBoarding { get; set; }
        public Boolean Block { get; set; }
        public int ExperienceMonth { get; set; }
        public int ExperienceYear { get; set; }
        [DisplayName("Work Place")]
        public string WorkPlace { get; set; }
        public int WorkplaceId { get; set; }
        [DisplayName("Marital Status")]
        public string Marital { get; set; }
        public int MaritalStatusId { get; set; }
        public string Skills { get; set; }
        public int BranchID { get; set; }
        [DisplayName("Branch")]
        public string BranchName { get; set; }



        //New Columns Added
        public string QAddress { get; set; }
        public int? RollID { get; set; }
        public string RollName { get; set; }
        [DisplayName("Blood Group")]
        public string BloodGroup { get; set; }
        public int BloodGroupID { get; set; }
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
        [DisplayName("Present Address 1")]
        public string PresentAddress1 { get; set; }
        [DisplayName("Present Address 2")]
        public string PresentAddress2 { get; set; }
        [DisplayName("Present Address 3")]
        public string PresentAddress3 { get; set; }
        [DisplayName("Present City")]
        public string PresentCity { get; set; }
        public int PresentCityID { get; set; }
        [DisplayName("Present Country")]
        public string PresentCountry { get; set; }
        public int? PresentCountryID { get; set; }
        [DisplayName("Present State")]
        public string Presentstate { get; set; }
        public int PresentstateID { get; set; }
        [DisplayName("Pin Code")]
        public int? PresentPinCode { get; set; }
        [DisplayName("Permanent Address 1")]
        public string PermanentAddress1 { get; set; }
        [DisplayName("Permanent Address 2")]
        public string PermanentAddress2 { get; set; }
        [DisplayName("Permanent Address 3")]
        public string PermanentAddress3 { get; set; }
        [DisplayName("Permanent City")]
        public string PermanentCity { get; set; }
        public int PermanentCityID { get; set; }
        [DisplayName("Permanent Country")]
        public string PermanentCountry { get; set; }
        public int? PermanentCountryID { get; set; }
        [DisplayName("Permanent State")]
        public string Permanentstate { get; set; }
        public int? PermanentstateID { get; set; }
        [DisplayName("PinCode")]
        public int? PermanentPinCode { get; set; }
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


    public class  ListReporting
    {

        public int? Id { get; set; }
    }

    static class extentions
    {

        internal static List<Variance> DetailedCompare(UpdateUser UserUpdateModel, SaveUser UserSavedModel)
        {
            List<Variance> variances = new List<Variance>();
            //var prop = UserUpdateModel.GetType().GetProperties();
            //var prop1 = UserSavedModel.GetType().GetProperties().Select(x => x.Name);      
            //prop = prop.Where(x => prop1.Contains(x.Name)).ToArray();

            Type type_s = UserSavedModel.GetType();
            PropertyInfo[] properties_s = type_s.GetProperties();
           // bool hasMetaDataAttribute_s = false;


            //object item;
            Type type_u = UserUpdateModel.GetType();
            PropertyInfo[] properties_u = type_u.GetProperties();
            bool hasMetaDataAttribute_u = false;
            foreach (PropertyInfo f in properties_u)
            {
                string displayName = GetDisplayName(type_u,f, hasMetaDataAttribute_u);
                Variance v = new Variance();
                v.FieldName = displayName;

                //if (f.Name == "RoleName")
                //    v.FieldName = "Role Name";
                //else if (f.Name == "Marital")
                //    v.FieldName = " Marital Status";
                //else
                //    v.FieldName = f.Name;

                v.UserUpdateValue = UserUpdateModel.GetType().GetProperty(f.Name).GetValue(UserUpdateModel, null);
                v.UserSaveValue = UserSavedModel.GetType().GetProperty(f.Name).GetValue(UserSavedModel, null);
                if (v.UserUpdateValue != null && v.UserSaveValue != null)
                {
                    if (!v.UserUpdateValue.ToString().Equals(v.UserSaveValue.ToString()))
                        variances.Add(v);
                }
                else
                {
                    if (v.UserUpdateValue == null && v.UserSaveValue == null)
                    {
                    }
                    else
                    {
                        variances.Add(v);
                    }
                }
            }

            //Commented to avoid Content Repeat while sending mail

        //    Type type_s = UserSavedModel.GetType();
        //    PropertyInfo[] properties_s = type_s.GetProperties();
        //    bool hasMetaDataAttribute_s = false;
        //    foreach (PropertyInfo f in properties_s)
        //    {
        //        string displayName = GetDisplayName(type_s, f, hasMetaDataAttribute_s);
            
        //        Variance v = new Variance();

        //        v.FieldName = displayName;
        //        //if (f.Name == "RoleName")
        //        //    v.FieldName = "Role Name";
        //        //else if (f.Name == "Marital")
        //        //    v.FieldName = " Marital Status";
        //        //else
        //        //    v.FieldName = f.Name;

        //        v.UserUpdateValue = UserUpdateModel.GetType().GetProperty(f.Name).GetValue(UserUpdateModel, null);
        //        v.UserSaveValue = UserSavedModel.GetType().GetProperty(f.Name).GetValue(UserSavedModel, null);
        //        if (v.UserUpdateValue != null && v.UserSaveValue != null)
        //        {
        //            if (!v.UserUpdateValue.ToString().Equals(v.UserSaveValue.ToString()))
        //                variances.Add(v);
        //        }
        //        else
        //        {
        //            if (v.UserUpdateValue == null && v.UserSaveValue == null)
        //            {
        //            }
        //            else
        //            {
        //                variances.Add(v);
        //            }
        //        }

        //    }
            return variances;
        }
        private static String GetDisplayName(Type type, PropertyInfo info, bool hasMetaDataAttribute)
        {
            if (!hasMetaDataAttribute)
            {
                object[] attributes = info.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    var displayName = (DisplayNameAttribute)attributes[0];
                    return displayName.DisplayName;
                }
                return info.Name;
            }
            PropertyDescriptor propDesc = TypeDescriptor.GetProperties(type).Find(info.Name, true);
            DisplayNameAttribute displayAttribute =
                propDesc.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
            return displayAttribute != null ? displayAttribute.DisplayName : null;
        }
    }
    public class Variance
    {
        public string FieldName { get; set; }
        public object UserUpdateValue { get; set; }
        public object UserSaveValue { get; set; }
    }

    public class EmailPurpose
    {
        public int Id { get; set; }
        public string  EmailList { get; set; }
        public string Purpose { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public string EmailTemplateCategoryID { get; set; }
    }


    public class EmailCategory
    {
        public string purpose { get; set; }
    }


    public class Email
    {
        public int  PurposeId { get; set; }
        public string Purpose { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public string EmailCategoryName { get; set; }
    }
    public class Mycomputerdetails
    {
     
        public string ComputerName { get; set; }
        public string HardDisk { get; set; }
        public string os { get; set; }
        public string IPAddress { get; set; }
        public string Extra { get; set; }
        public string Location { get; set; }
        public string workstation { get; set; }
        public string pendrive { get; set; }
    }

    public class FilterGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DepartmentId { get; set; }
    }

    public class ResetPassword
    {
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public bool Reset { get; set; }
        public bool SendLink { get; set; }
        public bool ResetHere { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}