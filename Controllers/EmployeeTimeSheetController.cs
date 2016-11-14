using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.Web.Script.Serialization;

namespace DSRCManagementSystem.Controllers
{
    public class EmployeeTimeSheetController : Controller
    {
        //
        // GET: /EmployeeTimeSheet/

        public ActionResult EmployeeTimeSheet()
        {
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                EmployeeTimeSheet obj = new EmployeeTimeSheet();
                try
                {
                    obj.ProjectNames = GetProjects();
                    obj.EmployeeNames = new List<SelectListItem>();
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

                }
                return View(obj);
            }
        }

        private List<SelectListItem> GetProjects()
        {
            try
            {
                var ProjectList = new List<SelectListItem>();
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    //List<DSRCProjects> projects = (from a in db.UserProjects
                    //                               join b in db.
                    //                               where a.ProjectID == b.pro
                    //                               select new DSRCProjects
                    //                               {
                    //                                   ProjectId = a.ProjectID,
                    //                                   ProjectName = a.ProjectName
                    //                               }).ToList();
                    int userID = int.Parse(Session["UserID"].ToString());
                    List<string> groupID = new List<string>();
                    var result = (from UP in db.UserProjects
                                  join
                                      P in db.Projects on UP.ProjectID equals P.ProjectID
                                  where P.IsActive == true && UP.UserID == userID
                                  select new
                                  {
                                      ProjectID = P.ProjectID,
                                      ProjectName = P.ProjectName
                                  }).ToList();
                    foreach (var item in result)
                    {
                        ProjectList.Add(new SelectListItem { Text = item.ProjectName, Value = item.ProjectID.ToString() });
                    }
                }
                return ProjectList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        public ActionResult GetProjectMembers(int id)
        {
            try
            {
                var EmployeeList = new List<SelectListItem>();
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var Employees = (from a in db.UserProjects
                                     where a.ProjectID == id
                                     join b in db.Users
                                     on a.UserID equals b.UserID
                                     select new
                                     {
                                         EmployeeId = b.UserID,
                                         EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim()
                                     }).ToList();
                    foreach (var item in Employees)
                    {
                        EmployeeList.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeId.ToString() });
                    }
                }
                return Json(EmployeeList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        [HttpPost]
        public ActionResult EmployeeTimeSheet(EmployeeTimeSheet model)
        {
            var selectedtimeSheetData = new List<SelectedTimeSheet>();
            try
            {
                //int uid = int.Parse(Session["UserID"].ToString());
                int pid = Convert.ToInt32(model.ProjectName);
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {

                    selectedtimeSheetData = (from data in dbHrms.TimesheetDatas
                                             join
                                           userdata in dbHrms.Users on data.UserID equals userdata.UserID
                                             join
                                                 projectdata in dbHrms.Projects on data.ProjectID equals projectdata.ProjectID
                                             where data.UserID == model.EmployeeId && data.ProjectID == pid && EntityFunctions.TruncateTime(data.DateOFSheet) >= model.StartDate && EntityFunctions.TruncateTime(data.DateOFSheet) <= model.EndDate
                                             select new SelectedTimeSheet()
                                             {
                                                 EmployeeName = ((userdata.FirstName) + " " + (userdata.LastName ?? "")).Trim(),
                                                 ProjectName = projectdata.ProjectName,
                                                 Date = data.DateOFSheet,
                                                 ProjectId = data.ProjectID,
                                                 UserId = data.UserID

                                             }).ToList();
                    return Json(selectedtimeSheetData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(selectedtimeSheetData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewTimeSheetValues(string Value)
        {
            bool? type = null;
            var json_serializer = new JavaScriptSerializer();
            SelectedTimeSheet val = json_serializer.Deserialize<SelectedTimeSheet>(Value);
            try
            {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            type = (from t in db.TimesheetTypes where t.ProjectID == val.ProjectId select t.IsGrouped).SingleOrDefault();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            if (type.Value)
                return RedirectToAction("ViewGroupedtimeSheet", new { ProjectId = val.ProjectId, Date = val.Date });
            else
                return RedirectToAction("ViewNonGroupedtimeSheet", new { ProjectId = val.ProjectId, Date = val.Date });
        }

        public ActionResult ViewGroupedtimeSheet(int ProjectId, DateTime Date)
        {
            int UserID = int.Parse(Session["UserID"].ToString());
            string Query;
            ModelState.Clear();
            Session["projectID"] = ProjectId;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            try
            {
                var datas = from g in db.Groups
                            join t in db.TimeSheetColumns on g.GroupID equals t.GroupID
                            join ct in db.Master_ColumnTypes on t.ColumnTypeID equals ct.ColumnTypeID
                            where t.ProjectID == ProjectId
                            where t.IsActive != false
                            select new
                            {
                                ColumnName = t.ColumnNames,
                                ColumnDisplayName = t.ColumnDisplayName,
                                ColumnTypeName = ct.ColumnTypeName,
                                GroupName = g.GroupName,
                                ColumnId = t.TimeSheetColumnID
                            };

                foreach (var item in datas)
                {
                    Template objtemplate = new Template();
                    objtemplate.columnName = item.ColumnName;
                    objtemplate.columnTypeName = item.ColumnTypeName;
                    objtemplate.ColumnDisplayName = item.ColumnDisplayName;
                    objtemplate.ColumnId = item.ColumnId;
                    objtemplate.groupName = item.GroupName;
                    templateData.Add(objtemplate);
                }

                Query = "SELECT  ";
                foreach (var item in templateData)
                    Query += "ISNULL(CONVERT(NVARCHAR(100)," + item.columnName + ",103),'')+'$'+";
                Query = Query.Substring(0, Query.Length - 5);
                Query += " FROM TimesheetData WHERE UserId=" + UserID + " AND ProjectId=" + ProjectId + " AND Date='" + Date.ToString("MM-dd-yyyy HH:mm:ss") + "'";

                var details = db.SP_GetTimeSheetData(Query).FirstOrDefault();
                List<string> ColumnValues = details.Split(new char[] { '$' }).ToList();
                int counter = 0;
                foreach (var item in templateData)
                    item.ColumnValue = ColumnValues[counter++];

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View(templateData);

        }

        public ActionResult ViewNonGroupedtimeSheet(int ProjectId, DateTime Date)
        {
            Session["projectID"] = ProjectId;
            int UserID = int.Parse(Session["UserID"].ToString());
            string Query;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            try
            {
                var data = from t in db.Projects select t;
                var datas = from t in db.TimeSheetColumns
                            join ct in db.Master_ColumnTypes on t.ColumnTypeID equals ct.ColumnTypeID
                            where t.GroupID == null && t.ProjectID == ProjectId && t.IsActive != false
                            select new
                            {
                                Project = t.ProjectID,
                                ColumnName = t.ColumnNames,
                                ColumnDisplayName = t.ColumnDisplayName,
                                ColumnId = t.TimeSheetColumnID,
                                ColumnTypeName = ct.ColumnTypeName,
                                ColumnTypeID = t.ColumnTypeID
                            };


                foreach (var item in datas)
                {
                    Template objtemplate = new Template();
                    objtemplate.projectID = item.Project;
                    objtemplate.columnName = item.ColumnName;
                    objtemplate.ColumnId = item.ColumnId;
                    objtemplate.ColumnDisplayName = item.ColumnDisplayName;
                    objtemplate.columnTypeName = item.ColumnTypeName;
                    objtemplate.columnTypeID = item.ColumnTypeID;
                    templateData.Add(objtemplate);
                }

                Query = "SELECT  ";
                foreach (var item in templateData)
                    Query += "ISNULL(CONVERT(NVARCHAR(100)," + item.columnName + ",103),' ')+'$'+";
                Query = Query.Substring(0, Query.Length - 5);
                Query += " FROM TimesheetData WHERE UserId=" + UserID + " AND ProjectId=" + ProjectId + " AND Date='" + Date.ToString("MM-dd-yyyy HH:mm:ss") + "'";

                var details = db.SP_GetTimeSheetData(Query).FirstOrDefault();
                List<string> ColumnValues = details.Split(new char[] { '$' }).ToList();
                int counter = 0;
                foreach (var item in templateData)
                    item.ColumnValue = ColumnValues[counter++];
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View(templateData);

        }


    }
}
