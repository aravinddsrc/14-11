using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.Objects.SqlClient;
using System.Linq.Expressions;
using DSRCManagementSystem.DSRCLogic;

namespace DSRCManagementSystem.Controllers
{

    public class TemplateController : Controller
    {
        //
        // GET: /Template/
       

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ProjectColumns(int? ProjectId)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Template model = new Template();
            try
            {
                model.ColumnLIst = new TemplateColumnLIst();
                if (ProjectId != null)
                {
                    Session["projectID"] = ProjectId;
                }
                //get  ungrouped columns
                model.ColumnLIst.ColumnList = (

                                                           from dt in db.TimeSheetColumns
                                                           join
                                                               ct in db.Master_ColumnTypes on dt.ColumnTypeID equals ct.ColumnTypeID
                                                           where dt.GroupID == null && dt.ProjectID == ProjectId && dt.IsActive != false

                                                           select new Column()
                                                           {
                                                               ColumnId = dt.TimeSheetColumnID,
                                                               GroupName = "",
                                                               ColumnTypeName = ct.ColumnTypeName,
                                                               ColumnName = dt.ColumnNames,
                                                               ColumnDisplayName = dt.ColumnDisplayName,
                                                               IsActive = ct.IsActive
                                                           }).ToList();

                //get grouped columns
                var temp = (

                                              from dt in db.TimeSheetColumns
                                              join
                                                  ct in db.Master_ColumnTypes on dt.ColumnTypeID equals ct.ColumnTypeID
                                              join
                                                   gt in db.Groups on dt.GroupID equals gt.GroupID
                                              orderby gt.GroupName
                                              where dt.ProjectID == ProjectId && dt.IsActive != false


                                              select new Column()
                                              {
                                                  ColumnId = dt.TimeSheetColumnID,
                                                  GroupName = gt.GroupName,
                                                  ColumnTypeName = ct.ColumnTypeName,
                                                  ColumnName = dt.ColumnNames,
                                                  ColumnDisplayName = dt.ColumnDisplayName,
                                                  IsActive = ct.IsActive
                                              }).ToList();
                foreach (var col in temp)
                    model.ColumnLIst.ColumnList.Add(col);

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
             return Json(model, JsonRequestBehavior.AllowGet);
        }

        
        
        public ActionResult CreateTemplate(int ? projectID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Template data = new Template();
            try
            {
                Session["projectID"] = null;
                Template objTemplate = new Template();
                int UserID = int.Parse(Session["UserID"].ToString());
                var test = Session["UserID"];

                List<Project> projlist = ((from usrproj in db.Projects
                                           //join
                                           // proj in db.Projects on usrproj.ProjectID equals proj.ProjectID
                                           // where usrproj.UserID==UserID
                                           where usrproj.IsActive == true
                                           select usrproj).ToList());
                //ViewBag.ProjectList

                ViewBag.ProjectList = new SelectList(projlist, "ProjectID", "ProjectName");
                if (projectID != null)
                {
                    Session["projectID"] = projectID;
                }

                
                data.ColumnLIst = new TemplateColumnLIst();
                data.ColumnLIst.ColumnList = (from dt in db.DefaultTemplates
                                              join
                                                  ct in db.Master_ColumnTypes on dt.ColumnTypeID equals ct.ColumnTypeID
                                              select new Column()
                                              {
                                                  ColumnId = dt.ColumnTypeID,
                                                  ColumnTypeName = dt.ColumnName,
                                                  IsActive = ct.IsActive

                                              }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(data);

        }

      
        public ActionResult AddNewGroup()
        {
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            Template objtemplate = new Template();
            objtemplate.IsActive = true;
            return View(objtemplate);

        }

        [HttpPost]
        public ActionResult AddNewGroup(Template objTemplate)
        {
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try
            {
                if (ModelState.IsValid)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    int? ID;
                    var pID = Session["projectID"];
                    if (pID == null)
                        ID = null;
                    else
                        ID = int.Parse(Session["projectID"].ToString());

                    if (db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).ToList().Count == 0)
                    {
                        var typobj = db.TimesheetTypes.CreateObject();
                        typobj.IsGrouped = true;
                        typobj.ProjectID = ID;
                        db.TimesheetTypes.AddObject(typobj);

                    }
                    else
                    {
                        var typobj = db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).FirstOrDefault();
                        typobj.IsGrouped = true;
                        db.SaveChanges();
                    }

                    if (!db.Groups.Any(o => o.GroupName == objTemplate.groupName))
                    {
                        Template objTemplates = new Template();
                        var t = new Group()
                 {
              GroupName = objTemplate.groupName.Replace(" ", ""),
              ProjectID = ID,
              IsActive = objTemplate.IsActive
                   };
                         db.Groups.AddObject(t);
                        db.SaveChanges();
                        return RedirectToAction("Success", "Popup");
                    }
                    else
                        return Json(false, JsonRequestBehavior.AllowGet); ;
                }
                else
                    return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        public ActionResult AddNewSubGroup()
        {
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            Template objTemplate = new Template();
            try
            {
                int? ID;
                var pID = Session["projectID"];
                if (pID == null)
                    ID = null;
                else
                    ID = int.Parse(Session["projectID"].ToString());
                
                ViewBag.GroupID = new SelectList(objTemplate.getGroupID(ID), "GroupID", "GroupName");
                ViewBag.columnTypeID = new SelectList(objTemplate.getColumnTypeID(), "ColumnTypeID", "ColumnTypeName");
                objTemplate.IsActive = true;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objTemplate);
        }

        public ActionResult DeleteColumn(int ColumnId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var obj = db.TimeSheetColumns.Where(o => o.TimeSheetColumnID == ColumnId).Select(o => o).FirstOrDefault();
                obj.IsActive = false;    
                db.SaveChanges();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditColumn(int ColumnId)
        {
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Template objTemplate = new Template();
            try
            {
                int ID = int.Parse(Session["projectID"].ToString());

                objTemplate = (

                                            from dt in db.TimeSheetColumns
                                            join
                                                ct in db.Master_ColumnTypes on dt.ColumnTypeID equals ct.ColumnTypeID
                                            where dt.TimeSheetColumnID == ColumnId

                                            select new Template()
                                            {
                                                ColumnId = dt.TimeSheetColumnID,
                                                columnName = dt.ColumnNames,
                                                groupName = dt.Group.GroupName,
                                                IsActive = dt.IsActive,
                                                columnTypeName = ct.ColumnTypeName

                                            }).FirstOrDefault();



                var grouplist = db.Groups.Where(o => o.IsActive == true && o.ProjectID == ID).Select(o => new SelectListItem { Text = o.GroupName, Value = SqlFunctions.StringConvert((double)o.GroupID), Selected = o.GroupName == objTemplate.groupName ? true : false }).ToList();
                if (objTemplate.groupName == null || objTemplate.groupName == "")
                    grouplist.Add(new SelectListItem { Text = "", Value = "", Selected = true });
                //var grouplist= from t in db.Groups where t.IsActive == true && t.ProjectID==ID select t;   
                ViewBag.GroupID = grouplist;
                var list = db.Master_ColumnTypes.Select(o => new SelectListItem { Text = o.ColumnTypeName, Value = SqlFunctions.StringConvert((double)o.ColumnTypeID), Selected = o.ColumnTypeName == objTemplate.columnTypeName ? true : false }).ToList();
                ViewBag.columnTypeID = list;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objTemplate);
        }
        [HttpPost]
        public ActionResult EditColumn(Template data)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var obj = db.TimeSheetColumns.Where(o => o.TimeSheetColumnID == data.ColumnId).Select(o => o).FirstOrDefault();
                string columnname = data.columnName.Contains(" ") ? data.columnName.Replace(" ", "") : data.columnName;
                obj.ColumnNames = columnname;
                obj.ColumnDisplayName = data.columnName;
                obj.ColumnTypeID = data.columnTypeID;
                obj.GroupID = Convert.ToInt32(data.groupName) == 0 ? null : Convert.ToInt32(data.groupName) as int?;
                obj.IsActive = data.IsActive;
                db.SaveChanges();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return RedirectToAction("Success", "Popup");
        }

        [HttpPost]
        public ActionResult AddNewSubGroup(Template objTemplate)
        {
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
           DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           try
           {
               int? ID;
               var pID = Session["projectID"];
               if (pID == null)
                   ID = null;
               else
                   ID = int.Parse(Session["projectID"].ToString());
               int? GroupID;
               if (objTemplate.groupName != null)
                   GroupID = int.Parse(objTemplate.groupName.ToString());
               else
                   GroupID = null;

               string columnname = objTemplate.columnName.Contains(" ") ? objTemplate.columnName.Replace(" ", "") : objTemplate.columnName;
               if (!db.TimeSheetColumns.Any(o => o.ProjectID == ID && o.ColumnNames == columnname && o.IsActive != false))
               {
                   var t = new TimeSheetColumn()
                   {
                       ColumnNames = columnname,
                       ColumnTypeID = objTemplate.columnTypeID,
                       ColumnDisplayName = objTemplate.columnName,
                       GroupID = GroupID,
                       ProjectID = ID,
                       IsActive = objTemplate.IsActive
                   };
                   db.TimeSheetColumns.AddObject(t);
                   db.SaveChanges();
                   db.Dispose();
                   db = new DSRCManagementSystemEntities1();
                   int ColumnId;
                   if (GroupID != null)
                       ColumnId = db.TimeSheetColumns.Where(o => o.ProjectID == ID && o.ColumnNames == columnname && o.GroupID == GroupID).Select(o => o.TimeSheetColumnID).FirstOrDefault();
                   else
                       ColumnId = db.TimeSheetColumns.Where(o => o.ProjectID == ID && o.ColumnNames == columnname && o.GroupID == null).Select(o => o.TimeSheetColumnID).FirstOrDefault();
                   db.ExecuteStoreQuery<Template>("exec SP_AddTimesheetDataColumn @ColumnName={0} ,@ColumnType= {1}", columnname, (objTemplate.columnTypeID == 1 ? "DATETIME" : "nvarchar(MAX)")).FirstOrDefault();
                   return Json(ColumnId, JsonRequestBehavior.AllowGet);
               }
               else
                   return Json(false, JsonRequestBehavior.AllowGet);
           }
           catch (Exception Ex)
           {
               string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
               string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
               ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
           }
           return View();
        }
        public ActionResult AddNewColumn()
        {
            Template objTemplate = new Template();
            try
            {
                int? ID;
                var pID = Session["projectID"];
                if (pID == null)
                    ID = null;
                else
                    ID = int.Parse(Session["projectID"].ToString());
                
                ViewBag.GroupID = new SelectList(objTemplate.getGroupID(ID), "GroupID", "GroupName");
                ViewBag.columnTypeID = new SelectList(objTemplate.getColumnTypeID(), "ColumnTypeID", "ColumnTypeName");
                objTemplate.IsActive = true;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objTemplate);
        }

        [HttpPost]
        public ActionResult AddNewColumn(Template objTemplate)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Template objTemplates = new Template();
            try
            {
                if (ModelState.IsValid)
                {
                    int? ID;
                    var pID = Session["projectID"];
                    if (pID == null)
                        ID = null;
                    else
                        ID = int.Parse(Session["projectID"].ToString());

                    if (db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).ToList().Count == 0)
                    {
                        var typobj = db.TimesheetTypes.CreateObject();
                        typobj.IsGrouped = true;
                        typobj.ProjectID = ID;
                        db.TimesheetTypes.AddObject(typobj);

                    }
                    else
                    {
                        var typobj = db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).FirstOrDefault();
                        typobj.IsGrouped = true;
                        db.SaveChanges();
                    }

                    if (!db.Groups.Any(o => o.GroupName == objTemplate.groupName))
                    {

                        var t = new Group()
                        {
                            GroupName = objTemplate.groupName.Replace(" ", ""),
                            ProjectID = ID,
                            IsActive = objTemplate.IsActive
                        };
                        db.Groups.AddObject(t);
                        db.SaveChanges();
                        return RedirectToAction("Success", "Popup");
                    }
                    else
                        return Json(false, JsonRequestBehavior.AllowGet); ;
                }
                else
                    return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        public ActionResult DefaultTemplate()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> data = new List<Template>();
            try
            {
                data = (from dc in db.DefaultTemplates
                        join ct in db.Master_ColumnTypes on dc.ColumnTypeID equals ct.ColumnTypeID
                        select new Template()
                        {
                            columnName = dc.ColumnName,
                            columnTypeName = ct.ColumnTypeName,
                            columnTypeID = ct.ColumnTypeID
                        }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(data);
        }
        public ActionResult ViewNonGroupTemplate()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            try
            {
                int ID = int.Parse(Session["projectID"].ToString());
                var data = from t in db.Projects select t;
                var datas = from t in db.TimeSheetColumns
                            join ct in db.Master_ColumnTypes on t.ColumnTypeID equals ct.ColumnTypeID
                            where t.GroupID == null && t.ProjectID == ID && t.IsActive != false
                            select new
                            {
                                ColumnName = t.ColumnNames,
                                ColumnTypeName = ct.ColumnTypeName,
                                ColumnTypeID = t.ColumnTypeID
                            };

                foreach (var item in datas)
                {
                    Template objtemplate = new Template();
                    objtemplate.columnName = item.ColumnName;
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

        public ActionResult ViewGroupedTemplate()
        {
            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<Template> templateData = new List<Template>();
            Template objtemplate = new Template();
            try
            {
                ModelState.Clear();
                int? ID;
                var pID = Session["projectID"];
                if (pID == null)
                    ID = null;
                else
                    ID = int.Parse(Session["projectID"].ToString());
                

                var datas = from g in db.Groups
                            join t in db.TimeSheetColumns on g.GroupID equals t.GroupID
                            join ct in db.Master_ColumnTypes on t.ColumnTypeID equals ct.ColumnTypeID
                            where t.ProjectID == ID
                            where t.IsActive != false
                            select new
                            {
                                ColumnName = t.ColumnNames,
                                ColumnDisplayName = t.ColumnDisplayName,
                                ColumnTypeName = ct.ColumnTypeName,
                                GroupName = g.GroupName,
                                ColumnId = t.TimeSheetColumnID
                            };

                string columnname;
                foreach (var item in datas)
                {
                    
                    columnname = item.ColumnName;
                    objtemplate.columnName = item.ColumnName;
                    objtemplate.columnTypeName = item.ColumnTypeName;
                    objtemplate.ColumnDisplayName = item.ColumnDisplayName;
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
        //public ActionResult FinalizeDefaultTemplate(int ProjectID)
        //{

        //    return Json(true, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult FinalizeDefaultTemplate()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var ProjectId = Convert.ToInt32(Session["projectID"].ToString());
                var typobj = db.TimesheetTypes.Where(o => o.ProjectID == ProjectId).Select(o => o).FirstOrDefault();
                if (typobj == null)
                {
                    var dataobj = new TimesheetType();
                    dataobj.ProjectID = ProjectId;
                    dataobj.IsGrouped = false;
                    db.TimesheetTypes.AddObject(dataobj);
                }
                else
                    typobj.IsGrouped = false;
                db.SaveChanges();
                List<Column> defaultcolumnlist = db.DefaultTemplates.Select(o =>
                                                  new Column
                                                  {
                                                      ColumnName = o.ColumnName,
                                                      ColumnTypeId = o.ColumnTypeID
                                                  }).ToList();
                string columnname;
                foreach (var item in defaultcolumnlist)
                {
                    columnname = item.ColumnName.Contains(" ") ? item.ColumnName.Replace(" ", "") : item.ColumnName;
                    var timesheetcolumnobj = db.TimeSheetColumns.CreateObject();
                    timesheetcolumnobj.ProjectID = ProjectId;
                    timesheetcolumnobj.ColumnNames = columnname;
                    timesheetcolumnobj.ColumnDisplayName = item.ColumnName;
                    timesheetcolumnobj.ColumnTypeID = item.ColumnTypeId;
                    timesheetcolumnobj.IsActive = true;
                    db.TimeSheetColumns.AddObject(timesheetcolumnobj);
                    db.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FinalizeTemplate()
        {
            try
            {
                var ProjectId = Convert.ToInt32(Session["projectID"].ToString());
                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTimeSheetType(bool val)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int? ID;
                var pID = Session["projectID"];
                if (pID == null)
                    ID = null;
                else
                    ID = int.Parse(Session["projectID"].ToString());
                if (val)
                {
                    if (db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).ToList().Count == 0)
                    {
                        var typobj = db.TimesheetTypes.CreateObject();
                        typobj.IsGrouped = true;
                        typobj.ProjectID = ID;
                        db.TimesheetTypes.AddObject(typobj);

                    }
                    else
                    {
                        var typobj = db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).FirstOrDefault();
                        if (typobj == null)
                        {
                            var dataobj = new TimesheetType();
                            dataobj.ProjectID = ID;
                            dataobj.IsGrouped = true;
                            db.TimesheetTypes.AddObject(dataobj);
                        }
                        else
                            typobj.IsGrouped = true;


                        db.SaveChanges();
                    }
                }
                else
                {
                    var typobj = db.TimesheetTypes.Where(o => o.ProjectID == ID).Select(o => o).FirstOrDefault();
                    if (typobj == null)
                    {
                        var dataobj = new TimesheetType();
                        dataobj.ProjectID = ID;
                        dataobj.IsGrouped = false;
                        db.TimesheetTypes.AddObject(dataobj);
                    }
                    else
                        typobj.IsGrouped = false;
                    db.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
