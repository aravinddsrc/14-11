using System;
using System.Collections.Generic;
using System.Linq; 
using System.Web;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.Web.Mvc;


namespace DSRCManagementSystem.DSRCLogic
{
    public class CommonLogic
    {
        public static List<DSRCManagementSystem.Models.DepartmentData> GetDepartments()
        {
            List<DSRCManagementSystem.Models.DepartmentData> department = new List<DSRCManagementSystem.Models.DepartmentData>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            department = (List < DSRCManagementSystem.Models.DepartmentData>)db.Departments.Where(x => x.IsActive == true).Select(c => new
            {
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentName
            });
            department.Insert(0, new DepartmentData { departmentId = 0, departmentName = "---Select---" });

            return department.ToList();
        }
        public static List<DSRCManagementSystem.Models.DepartmentData> GetDepartments1()
        {
            List<DSRCManagementSystem.Models.DepartmentData> department = new List<DSRCManagementSystem.Models.DepartmentData>();
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                int userId = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

                int BranchId = (int)dbHrms.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                
                department = dbHrms.Departments.Where(x => x.BranchID == BranchId).Distinct().Select(x => new DepartmentData()
                {
                    departmentId = x.DepartmentId,
                    departmentName = x.DepartmentName
                }).ToList();
                department.Insert(0, new DepartmentData { departmentId = 0, departmentName = "---Select---" });
              
            }
            return department;
        }

        public static string getLabelName(int lblid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string name = db.Dictionaries.FirstOrDefault(x => x.Id == lblid).Name;
                return name;
            
        }

        public static string getLogoPath()
        {
            DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
            var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
            string[] words;
            words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
            string pathvalue = "~/" + words[1];
            return pathvalue;
        }

    }
}