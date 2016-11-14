using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class LoginModel
    {
        private string _UserName;
        private string _Password;
        [Required(ErrorMessageResourceName = "VR_UserModels_UserName", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_UserModels_UserName", ResourceType = typeof(Resources.Resource))]
        [StringLength(250, ErrorMessageResourceName = "V_General_Length_250", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }

        [Required(ErrorMessageResourceName = "VR_UserModels_Password", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_UserModels_Password", ResourceType = typeof(Resources.Resource))]
        [StringLength(250, ErrorMessageResourceName = "V_General_Length_250", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }
        public string color {get;set;}
        public string path { get; set; }
        public string version { get; set; }
        public string company { get; set; }
       // [DisplayName("Remember Me")]
       // public bool RememberMe { get; set; }

    }
    public class ChangePasswordGUIID
    {
        public int UserId { get; set; }
        public Guid Key { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage="Enter Key Here")]
       // [Compare("KeyEntered")]
        public string KeyEntered { get; set; }
        [Required(ErrorMessage="Enter New Password")]
        public string NewPassword { get; set; }
        //[Compare("NewPassword",ErrorMessage="Missmatch Password")]
        [Required(ErrorMessage = "Enter Confirm Password")]
        public string ReEnterPassword { get; set; }
        public Int32? ResetPasswordKey { get; set; }
        public string PasswordKey { get; set; }
        public string Guiid { get; set; }
    }

    public class UserLoginModel
    {
        public string company { get; set; }
        public int UserId { get; set; }
        private string _UserName;
        private string _Password;
        public string Name { get; set; }
        public Guid Key { get; set; }
        public Guid KeyEntered { get; set; }
        public string NewPassword { get; set; }
        public string ReEnterPassword { get; set; }
        [Required(ErrorMessage="Enter Email Address ")]
        public string Email { get; set; }
        public Int32? ResetPasswordKey { get; set; }

        [Required(ErrorMessageResourceName = "VR_UserModels_UserName", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_UserModels_UserName", ResourceType = typeof(Resources.Resource))]
        [StringLength(250, ErrorMessageResourceName = "V_General_Length_250", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }

        [Required(ErrorMessageResourceName = "VR_UserModels_Password", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = "DN_UserModels_Password", ResourceType = typeof(Resources.Resource))]
        [StringLength(250, ErrorMessageResourceName = "V_General_Length_250", ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }
    }
    public class ChangePassword
    {
        //[Required(ErrorMessage = "Enter Username")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceName = "VR_UserLoginModels_ChangePassword_OldPassword", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceName = "VR_UserLoginModels_ChangePassword_NewPassword", ErrorMessageResourceType = typeof(Resources.Resource))]
        [StringLength(50, ErrorMessageResourceName = "V_General_Length_50", ErrorMessageResourceType = typeof(Resources.Resource), MinimumLength = 5)]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        //[Compare("NewPassword", ErrorMessageResourceName = "VR_UserLoginModels_ChangePassword_PasswordMisMatch", ErrorMessageResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "VR_UserLoginModels_ChangePassword_ConfirmPassword", ErrorMessageResourceType = typeof(Resources.Resource))]
        [StringLength(50, ErrorMessageResourceName = "V_General_Length_50", ErrorMessageResourceType = typeof(Resources.Resource), MinimumLength = 5)]
        public string ConfirmPassword { get; set; }
    }
}