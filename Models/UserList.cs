using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class UserList
    {
        public long userId { get; set; }

        public string userName { get; set; }

        public bool isSelected { get; set; }

        public int departmentId { get; set; }


    }

    public class DepartmentData
    {
        public int departmentId { get; set; }

        public string departmentName { get; set; }
    }

    public class Menu
    {
        public int FunctionID { get; set; }
        public string FunctionName { get; set; }
    }

    public class MenuList
    {
        public int FunctionId { get; set; }
        public int PageModuleId { get; set; }
        public string FunctionName { get; set; }
        public string ModuleName { get; set; }
    }

    public class RoleFunctionPrivilages
    {
        public byte? FunctionId { get; set; }
        public byte? PageModuleId { get; set; }
    }

    public class CheckVal
    {
        public int RoleId { get; set; }
        public int FunctionID { get; set; }
        public int? PageModuleId { get; set; }
    }

}