using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class AdministrationSetup
    {

        [DisplayName("Role Type")]
        public int RoleTypeID { get; set; }        
        public List<SelectListItem> RoleList { get; set; }
        //public List<SelectListItem> Menu { get; set; }
        public List<Menu> Menu { get; set; }
        //public List<SelectListItem> MenuName { get; set; }
        public List<MenuList> MenuList { get; set; }
        public string RoleName { get; set; }
        public byte RoleID { get; set; }
        public string isActive { get; set; }
        public string FunctionName { get; set; }
        public string SubFunctionName { get; set; }           

        //[Required(ErrorMessage = "RoleName alredy exists")]
        [Display(Name = "VR_AdministrationSetupModels_RoleNameExists", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "VR_AdministrationSetupModels_RoleNameExists", ErrorMessageResourceType = typeof(Resources.Resource))]
        

        //public int Role { get; set; }
        //public int Menus { get; set; }
        public List<string> selectedFunctionID { get; set; }
        public List<string> selectedPageModuleID { get; set; }
        public List<string> selectedPreceedenceorder { get; set; }
        public int PrecedanceOrder { get; set; }
        public string Roledesignation { get; set; }
        public int Rolefunctionprivillege { get; set; }
        public int RoleFunctionPrivileges { get; set; }

        public List<CheckVal> CheckVal { get; set; }
    }
    public class RoleType
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
    }

    public class TestRoles
    {
        public int FunctionID { get; set; }
        public int PageModuleID { get; set; }
        public int PreceedanceOrder { get; set; }
    }

    public class ExtraHolidays
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public int? HolidayId { get; set; }
        public string HolidayName { get; set; }
        public string ZoneName { get; set; }
        public DateTime? Date { get; set; }
        public string  EnteredBy { get; set; }
        public int? ApprovedBy { get; set; }
        public string HolidayDate { get; set; }     
    }


    public class Format
    {
        public string Day { get; set; }
        public int? Id { get; set; }
        public string HolidayName { get; set; }
        public string ZoneName { get; set; }
        public DateTime? Date { get; set; }
        public string EnteredBy { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class HolidayDashBoard
    {
        public string Day { get; set; }
        public int? ZoneId { get; set; }
        public int? Id { get; set; }
        public string HolidayName { get; set; }
        public string ZoneName { get; set; }
        public DateTime? Date { get; set; }
        public string EnteredBy { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class MenuItems
    {
        public int InsertAfterFunctionID { get; set; }
        public string InsertAfterFunctionName { get; set; }
        public string NewFunctionName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
    public class SubmenuItems
    {
        public int FunctionId { get; set; }
        public string FunctionName { get; set; }
        public int InsertAfterPageModuleId { get; set; }
        public string InsertAfterModuleName { get; set; }
        public string NewModuleName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }

    //public class MenuList
    //{
    //    public int FunctionId { get; set; }
    //    public int PageModuleId { get; set; }
    //    public string FunctionName { get; set; }
    //    public string ModuleName { get; set; }
    //}
}