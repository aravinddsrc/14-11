using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.Web.Script.Serialization;
using System.Data.Objects;

namespace DSRCManagementSystem.Controllers
{
    public class TimeSheetController : Controller
    {
        //
        // GET: /TimeSheet/

        public ActionResult Create()
        {
            //try
            //{
            //    int tyy = 0;
            //    tyy /= tyy;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            TimesheetModel objtimesheet = new TimesheetModel();
            try
            {
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



                ViewBag.ProjectList = new SelectList(result, "ProjectID", "ProjectName");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult GetProjectType(int ProjectId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                bool? type = (from t in db.TimesheetTypes where t.ProjectID == ProjectId select t.IsGrouped).SingleOrDefault();
                if (type != null)//true
                    return Json(type.Value, JsonRequestBehavior.AllowGet);
                else
                    return Json("empty", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection forms)
        {
            return View();
        }

        public ActionResult GroupedTimesheet(int ProjectId)
        {
            List<Template> templateData = new List<Template>();
            Template objtemplate = new Template();
            try
            {
                ModelState.Clear();
                Session["projectID"] = ProjectId;
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

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

                    objtemplate.columnName = item.ColumnName;
                    objtemplate.columnTypeName = item.ColumnTypeName;
                    objtemplate.ColumnDisplayName = item.ColumnDisplayName;
                    objtemplate.ColumnId = item.ColumnId;
                    objtemplate.groupName = item.GroupName;
                    templateData.Add(objtemplate);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(templateData);
        }
        [HttpPost]
        public ActionResult GroupedTimesheet(FormCollection form)
        {
            // Session["projectID"] = f;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            TimesheetModel objTimesheet = new TimesheetModel();
            try
            {
                int ProjectId = int.Parse(Session["projectID"].ToString());
                var ColumnNames = (from t in db.TimeSheetColumns where t.ProjectID == ProjectId select t.ColumnNames).ToList();

                int n = ColumnNames.Count();
                Dictionary<string, string> timeSheetValueDic = new Dictionary<string, string>();
                string values = ""; int TimesheetColumnID, UserID, ProjectID;
                string TimeSheetValue = "", Date;
                string timesheetColumnName = "";
                for (int i = 0; i < n; i++)
                {
                    string results = form[ColumnNames[i]];
                    timesheetColumnName = ColumnNames[i];

                    if (results != ",")
                    {
                        values = results.Remove(results.Length - 2, 2);

                        timeSheetValueDic.Add(timesheetColumnName, values);
                    }
                    else
                        timeSheetValueDic.Add(timesheetColumnName, null);
                }

                int s = 0;
                foreach (KeyValuePair<string, string> timesheet in timeSheetValueDic)
                {
                    //change
                    TimesheetColumnID = int.Parse(timesheet.Key);
                    TimeSheetValue = timesheet.Value;
                    UserID = int.Parse(Session["UserID"].ToString());
                    ProjectID = 27;
                    Date = DateTime.Now.ToString();
                    if (s == 0)
                        objTimesheet.InsertTimesheet(TimesheetColumnID, TimeSheetValue, UserID, ProjectID, Date);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return RedirectToAction("Success", "Popup");
        }

        public ActionResult ViewNonGroupedtimeSheet(int ProjectId, DateTime Date)
        {
            Template objtemplate = new Template();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            try
            {
                Session["projectID"] = ProjectId;
                int UserID = int.Parse(Session["UserID"].ToString());
                string Query;

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
                Query += " FROM TimesheetData WHERE UserId=" + UserID + " AND ProjectId=" + ProjectId + " AND DateOFSheet='" + Date.ToString("MM-dd-yyyy HH:mm:ss") + "'";

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

        public ActionResult ViewGroupedtimeSheet(int ProjectId, DateTime Date)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            Template objtemplate = new Template();
            try
            {
                int UserID = int.Parse(Session["UserID"].ToString());
                string Query;
                ModelState.Clear();
                Session["projectID"] = ProjectId;
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
                Query += " FROM TimesheetData WHERE UserId=" + UserID + " AND ProjectId=" + ProjectId + " AND DateOFSheet='" + Date.ToString("MM-dd-yyyy HH:mm:ss") + "'";

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


        public ActionResult NonGroupedTimesheet(int ProjectId)
        {
            Session["projectID"] = ProjectId;
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
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return View(templateData);
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

        public ActionResult SaveTimeSheet(List<string> ColumnName, List<string> ColumnValue)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int ProjectId = Convert.ToInt32(Session["projectID"]);
                int UserId = Convert.ToInt32(Session["UserID"]);
                string Query = "insert into TimesheetData (UserID,ProjectID,DateOFSheet,";
                foreach (var val in ColumnName)
                    Query += val + ",";
                Query = Query.Substring(0, Query.Length - 1);
                Query += ") values(" + UserId + "," + ProjectId + ",'" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "','";
                foreach (var val in ColumnValue)
                {
                    if (val.Equals(""))
                    {
                        Query = Query.Substring(0, Query.Length - 1);
                        Query += "null,'";
                    }
                    else
                        Query += val + "','";
                }

                Query = Query.Substring(0, Query.Length - 2);
                Query += ")";
                db.ExecuteStoreCommand(Query);
                var obj = db.TimesheetDatas.CreateObject();
                obj.UserID = UserId;
                obj.ProjectID = ProjectId;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return RedirectToAction("Success", "Popup");
        }
        private List<SelectListItem> GetProjects()
        {
            try
            {
                var ProjectList = new List<SelectListItem>();
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {

                    int UserID = int.Parse(Session["UserID"].ToString());
                    List<DSRCProjects> projects = (from usrproj in db.UserProjects
                                                   join
                                                      proj in db.Projects on usrproj.ProjectID equals proj.ProjectID
                                                   where usrproj.UserID == UserID
                                                   select new DSRCProjects
                                                   {
                                                       ProjectId = usrproj.ProjectID,
                                                       ProjectName = proj.ProjectName
                                                   }).ToList();
                    foreach (var item in projects)
                    {
                        ProjectList.Add(new SelectListItem { Text = item.ProjectName, Value = item.ProjectId.ToString() });
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

        public ActionResult ViewTimeSheet()
        {
            ViewTimeSheetModel obj = new ViewTimeSheetModel();
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {

                    obj.ProjectNames = GetProjects();
                    obj.EmployeeNames = new List<SelectListItem>();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return View(obj);
        }


        [HttpPost]
        public ActionResult ViewTimeSheet(ViewTimeSheetModel model)
        {
            // var selectedtimeSheetData = "";
            try
            {
                int uid = int.Parse(Session["UserID"].ToString());
                int pid = Convert.ToInt32(model.ProjectName);
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var selectedtimeSheetData = (from data in dbHrms.TimesheetDatas
                                                 join
                                               userdata in dbHrms.Users on data.UserID equals userdata.UserID
                                                 join
                                                     projectdata in dbHrms.Projects on data.ProjectID equals projectdata.ProjectID
                                                 where data.UserID == uid && data.ProjectID == pid && EntityFunctions.TruncateTime(data.DateOFSheet) >= model.StartDate && EntityFunctions.TruncateTime(data.DateOFSheet) <= model.EndDate
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
            return Json(JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewTimeSheetValues(string Value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {

                var json_serializer = new JavaScriptSerializer();
                SelectedTimeSheet val = json_serializer.Deserialize<SelectedTimeSheet>(Value);

                bool? type = (from t in db.TimesheetTypes where t.ProjectID == val.ProjectId select t.IsGrouped).SingleOrDefault();
                if (type.Value)
                    return RedirectToAction("ViewGroupedtimeSheet", new { ProjectId = val.ProjectId, Date = val.Date });
                else
                    return RedirectToAction("ViewNonGroupedtimeSheet", new { ProjectId = val.ProjectId, Date = val.Date });
              }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
    }
}
