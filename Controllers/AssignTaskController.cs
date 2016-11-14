using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.Objects;

namespace DSRCManagementSystem.Controllers
{
    public class AssignTaskController : Controller
    {
        //
        // GET: /AssignTask/

        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AssignTask()
        {
            var result = new List<AssignTaskModel>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var userId = (int)Session["UserId"];

                result = (from rc in db.TS_AssignedTask
                              join p in db.Projects on rc.ProjectId equals p.ProjectID
                              join ps in db.ProjectPhases on rc.PhaseName equals ps.ProjectPhaseId
                              join t in db.TS_Task on rc.TaskName equals t.TaskId
                              join u in db.Users on rc.Employees equals u.UserID
                              join ts in db.PhaseTaskTypeMappings on rc.TaskStatus equals ts.TaskTypeID
                              where rc.IsActive == true
                              select new AssignTaskModel
                              {
                                  assigntaskid = rc.AssignedTaskId,
                                  ProjectName = p.ProjectName,
                                  PhaseName = ps.Phase,
                                  StartDate = rc.StartDate,
                                  EndDate = rc.EndDate,
                                  NumberOfEfforts = rc.NoOfEfforts,
                                  TaskName = t.TaskName,
                                  taskstatus = ts.TaskTypeName,
                                  Taskid = t.TaskId,
                                  Employees = u.FirstName + "" + u.LastName ?? ""
                              }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(result);
        }

        [HttpGet]
        public ActionResult AssignNewTask()
        {
            try{
            var userId = (int)Session["UserId"];
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            AssignTaskModel task = new AssignTaskModel();
            List<int> list = new List<int>();
            list = objdb.ProjectPhases.Where(x => x.IsACTIVE == true).Select(o => o.ProjectID).ToList();

            var obj = (from p in objdb.UserProjects.Where(x => x.UserID == userId)
                       join t in objdb.Projects.Where(x => list.Contains(x.ProjectID) && (x.IsDeleted == false || x.IsDeleted == null)) on p.ProjectID equals t.ProjectID
                       select new
                       {
                           ProjectId = p.ProjectID,
                           ProjectName = t.ProjectName
                       }).OrderBy(o => o.ProjectName).ToList();

            ViewBag.ProjectList = new SelectList(obj, "ProjectId", "ProjectName");

            int pid = task.ProjectId;
            var phase = (from p in objdb.ProjectPhases.Where(x => x.ProjectID == pid && x.IsACTIVE == true)

                         select new AssignTaskModel()
                         {
                             PhaseId = p.ProjectPhaseId,
                             PhaseName = p.Phase
                         }).ToList();
            ViewBag.PhaseList = new SelectList(phase, "PhaseId", "PhaseName");

            var employees = (from p in objdb.UserProjects.Where(x => x.ProjectID == pid)
                             join t in objdb.Users on p.UserID equals t.UserID

                             select new AssignTaskModel()
                             {
                                 userid = t.UserID,
                                 Employees = t.FirstName + " " + t.LastName
                             }).ToList();



            ViewBag.Employees = new MultiSelectList(employees, "userid", "Employees");
            var taskstatus = (from t in objdb.TS_Task.Where(x => x.ProjectName == pid)
                              join p in objdb.PhaseTaskTypeMappings on t.PhaseStatus equals p.PhaseStatusId

                              select new AssignTaskModel()
                              {
                                  tasktypeid = p.TaskTypeID,
                                  taskstatus = p.TaskTypeName
                              }).ToList();

            ViewBag.taskstatuslist = new SelectList(taskstatus, "tasktypeid", "taskstatus");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }



        [HttpGet]
        public JsonResult GetEmployeeName(int projectId)
        {
            List<int> userid = new List<int>();
            List<SelectListItem> empname = new List<SelectListItem>();
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            if (projectId != 0)
            {

                userid = db.UserProjects.Where(x => x.ProjectID == projectId).Select(x => x.UserID).ToList();
                foreach (var user in userid)
                {

                    var emp = db.Users.Where(o => o.UserID == user && o.IsActive == true).FirstOrDefault();
                    if (emp != null)
                    {
                        string empna = emp.FirstName + " " + (emp.LastName ?? "");
                        empname.Add(new SelectListItem { Text = empna, Value = Convert.ToString(emp.UserID) });
                    }
                }
            }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }


            return Json(empname, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult GetTaskstatus(int proid)
        {
            var tasktypeid =0;
            var taskst = "";
            try
            {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            tasktypeid = db.TS_Task.FirstOrDefault(x => x.TaskId == proid).TaskStatus;
            taskst = db.PhaseTaskTypeMappings.FirstOrDefault(o => o.TaskTypeID == tasktypeid).TaskTypeName;
            return Json(new { taskstatus = taskst, Tasktypeid = tasktypeid }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { taskstatus = taskst, Tasktypeid = tasktypeid }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PhaseName(int pid)
        {

            IEnumerable<SelectListItem> phasenamelist = new List<SelectListItem>();
            List<int> userid = new List<int>();
            try
            {
                if (Convert.ToString(pid) != "--Select--" || pid != 0)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    phasenamelist = (from p in db.ProjectPhases.Where(x => x.ProjectID == pid && x.IsACTIVE == true)

                                     select new AssignTaskModel()
                                     {
                                         PhaseId = p.ProjectPhaseId,
                                         PhaseName = p.Phase
                                     }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.PhaseId), Text = m.PhaseName });

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new SelectList(phasenamelist, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult TaskName(int phid)
        {
            IEnumerable<SelectListItem> tasknamelist = new List<SelectListItem>();
            try{
           
            List<int> userid = new List<int>();
            if (Convert.ToString(phid) != "--Select--")
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                tasknamelist = (from p in db.TS_Task.Where(x => x.PhaseNameId == phid && x.ISACTIVE == true)

                                select new AssignTaskModel()
                                {
                                    Taskid = p.TaskId,
                                    TaskName = p.TaskName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.Taskid), Text = m.TaskName });

            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new SelectList(tasknamelist, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AssignNewTask(AssignTaskModel model)
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var employeecount = model.SelectedEmpList.Count();
            var name = model.Taskid;
            var Name = db.TS_Task.Where(x => x.TaskId == name).Select(o => o.TaskName).FirstOrDefault();

            string UserName = "";
            DateTime startdate;
            DateTime enddate;
            foreach (int id in model.SelectedEmpList)
            {
                UserName = db.Users.Where(o => o.UserID == id).Select(o => o.FirstName + " " + o.LastName ?? "").FirstOrDefault();
                startdate = (DateTime)db.TS_Task.FirstOrDefault(x => x.TaskId == model.Taskid).StartDate;
                enddate = (DateTime)db.TS_Task.FirstOrDefault(x => x.TaskId == model.Taskid).EndDATE;
                string start = startdate.ToString("dd/MM/yyyy");
                string end = enddate.ToString("dd/MM/yyyy");
                string fdate = Convert.ToString(model.StartDate);
                string tdate = Convert.ToString(model.EndDate);
                var fromdate = DateTime.Parse(fdate);
                var todate = DateTime.Parse(tdate);

                bool overlapping = db.TS_Task.FirstOrDefault(x => EntityFunctions.TruncateTime(x.StartDate) <= EntityFunctions.TruncateTime(fromdate)
                         && EntityFunctions.TruncateTime(x.EndDATE) >= EntityFunctions.TruncateTime(todate) && x.TaskId == model.Taskid && x.ISACTIVE == true) != null;


                var helpfromassign = db.TS_AssignedTask.Where(x => x.TaskName == model.Taskid).Select(o => o).FirstOrDefault();

                var TaskName = db.TS_Task.Where(x => x.ProjectName == model.ProjectId && x.TaskName == model.Task).Select(o => o.TaskId).FirstOrDefault();


                //startdateassign =(DateTime) db.TS_AssignedTask.FirstOrDefault(x => x.ProjectId == model.ProjectId).StartDate;

                //enddateassign = (DateTime)db.TS_AssignedTask.FirstOrDefault(x => x.ProjectId == model.ProjectId).EndDate;

                //string Start = startdateassign.ToString("dd/MM/yyyy");
                //string End = enddateassign.ToString("dd/MM/yyyy");
                //string Fdate = Convert.ToString(model.StartDate);
                //string Tdate = Convert.ToString(model.EndDate);
                //var Fromdate = DateTime.Parse(Fdate);
                //var Todate = DateTime.Parse(Tda

                var value = db.TS_Task.Where(x => x.TaskId == model.Taskid && x.ISACTIVE == true).Select(o => o.NoOfEfforts).FirstOrDefault();

                int? y = 0;


                List<int?> Total = new List<int?>();

                Total = db.TS_AssignedTask.Where(x => x.TaskName == model.Taskid && x.IsActive == true).Select(o => o.NoOfEfforts).ToList();

                for (int z = 0; z < Total.Count(); z++)
                {

                    y += Total[z];

                }

                if ((employeecount * model.NumberOfEfforts) > value || (value < model.NumberOfEfforts) || (y + model.NumberOfEfforts) > value)
                {
                    return Json("Greater", JsonRequestBehavior.AllowGet);
                }

                bool overlap = db.TS_AssignedTask.FirstOrDefault(x =>
                         EntityFunctions.TruncateTime(x.StartDate) <= EntityFunctions.TruncateTime(fromdate)
                         && EntityFunctions.TruncateTime(x.EndDate) >= EntityFunctions.TruncateTime(todate) && x.Employees == id && x.IsActive == true && x.ProjectId == model.ProjectId && x.Task == model.Task) != null;

                if (!overlapping)
                {
                    var result = new { message = "timeexceed", sdate = start, edate = end };
                    return Json(new { message = "timeexceed", sdate = start, edate = end });
                }
                if (overlap)
                {
                    var result = new { message = "Already", name = UserName };
                    return Json(new { message = "Already", name = UserName });
                }
            }

            //foreach (int id in model.SelectedEmpList)
            //{
            //    var Assignobj = db.TS_AssignedTask.CreateObject();

            //    Assignobj.ProjectId = model.ProjectId;
            //    Assignobj.PhaseName = model.ProjectphaseId;
            //    Assignobj.TaskName = model.Taskid;
            //    Assignobj.NoOfEfforts = model.NumberOfEfforts;
            //    Assignobj.Employees = id;
            //    Assignobj.StartDate = model.StartDate;
            //    Assignobj.EndDate = model.EndDate;
            //    Assignobj.IsActive = true;
            //    Assignobj.ISdelete = false;
            //    Assignobj.TaskStatus= model.tasktypeid;
            //    Assignobj.Approved = false;
            //    Assignobj.Isreject = false;
            //    Assignobj.Task=model.Task;
            //    db.TS_AssignedTask.AddObject(Assignobj);
            //    db.SaveChanges();
            //}
            var Assignobj = db.TS_AssignedTask.CreateObject();

            Assignobj.ProjectId = model.ProjectId;
            Assignobj.PhaseName = model.ProjectphaseId;
            Assignobj.TaskName = model.Taskid;
            Assignobj.NoOfEfforts = model.NumberOfEfforts;
            Assignobj.SelectedEmployess = model.multiselectemployees;
            // Assignobj.Employees = Convert.ToInt32(model.SelectedEmpList[i]);
            Assignobj.StartDate = model.StartDate;
            Assignobj.EndDate = model.EndDate;
            Assignobj.IsActive = true;
            Assignobj.ISdelete = false;
            Assignobj.TaskStatus = model.tasktypeid;
            Assignobj.Approved = false;
            Assignobj.Isreject = false;
            Assignobj.Task = Name;
            Assignobj.flag = 0;
            Assignobj.Task = model.Task;

            db.TS_AssignedTask.AddObject(Assignobj);
            db.SaveChanges();

            for (int i = 0; i < model.SelectedEmpList.Count(); i++)
            {
                DSRCManagementSystem.TS_AssignedTask obj = new DSRCManagementSystem.TS_AssignedTask();

                obj.ProjectId = model.ProjectId;
                obj.PhaseName = model.ProjectphaseId;
                obj.TaskName = model.Taskid;
                obj.NoOfEfforts = model.NumberOfEfforts;
                obj.SelectedEmployess = model.multiselectemployees;
                obj.Employees = Convert.ToInt32(model.SelectedEmpList[i]);
                obj.StartDate = model.StartDate;
                obj.EndDate = model.EndDate;
                obj.Task = Name;
                obj.IsActive = true;
                obj.ISdelete = false;
                obj.TaskStatus = model.tasktypeid;
                obj.Approved = false;
                obj.Isreject = false;
                obj.flag = 1;
                obj.Task = model.Task;
                db.TS_AssignedTask.AddObject(obj);
                db.SaveChanges();
            }

            Taskmodel oj = new Taskmodel();
            oj.newassigntask = 1;
            TempData["newassigntask"] = 1;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
    }
}
