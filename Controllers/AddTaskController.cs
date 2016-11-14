using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.Objects;

namespace DSRCManagementSystem.Controllers
{
    public class AddTaskController : Controller
    {
        //
        // GET: /AddTask/
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult AddTask()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                var userId = (int)Session["UserId"];
                AddTaskModel atm = new AddTaskModel();
                List<int> list = new List<int>();
                list = objdb.ProjectPhases.Where(x => x.IsACTIVE == true).Select(o => o.ProjectID).ToList();

                var obj = (from p in db.UserProjects.Where(x => x.UserID == userId)
                           join t in db.Projects.Where(x => list.Contains(x.ProjectID) && (x.IsDeleted == false || x.IsDeleted == null)) on p.ProjectID equals t.ProjectID
                           select new
                           {
                               ProjectId = p.ProjectID,
                               ProjectName = t.ProjectName
                           }).OrderBy(x => x.ProjectName).ToList();

                ViewBag.ProjectList = new SelectList(obj, "ProjectId", "ProjectName");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PhaseName(int pid)
        {
            IEnumerable<SelectListItem> phasenamelist = new List<SelectListItem>();
            try
            {

                if (Convert.ToString(pid) != "--Select--" || pid != 0)
                {
                    phasenamelist = (from p in db.ProjectPhases.Where(x => x.ProjectID == pid && x.IsACTIVE == true)

                                     select new AddTaskModel()
                                         {
                                             PhaseNameId = p.ProjectPhaseId,
                                             PhaseName = p.Phase
                                         }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.PhaseNameId), Text = m.PhaseName });

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
        public ActionResult TaskType(int phid)
        {
            IEnumerable<SelectListItem> tasktypelist = new List<SelectListItem>();
            try
            {
                int userId = (int)Session["UserId"];
                var tid = db.ProjectPhases.FirstOrDefault(o => o.UserID == userId && o.ProjectPhaseId == phid && o.IsACTIVE == true).PhaseStatus;
                if (Convert.ToString(phid) != "--Select--")
                {
                    tasktypelist = (from p in db.PhaseTaskTypeMappings.Where(x => x.PhaseStatusId == tid)
                                    //join ps in db.PhaseStatus on p.PhaseID equals ps.PhaseStatusName
                                    select new AddTaskModel()
                                    {
                                        TaskStatusId = p.TaskTypeID,
                                        Taskstatus = p.TaskTypeName
                                    }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.TaskStatusId), Text = m.Taskstatus });

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName); 

            }
            return Json(new SelectList(tasktypelist, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public JsonResult PhaseStatus(int PhasId)
        {

            int userId = (int)Session["UserId"];
            var phid = db.ProjectPhases.FirstOrDefault(o => o.UserID == userId && o.ProjectPhaseId == PhasId && o.IsACTIVE == true).PhaseStatus;
            var phasename = db.Master_PhaseStatus.FirstOrDefault(o => o.PhaseStatusId == phid).PhaseStatusName;

            return Json(new { phase = phasename, phaseid = phid }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddTask(int ProjectId, int PhaseId, string PhaseName, int PhaseStatus, string TaskName, int Taskstatus, DateTime StartDate, DateTime EndDate, int Efforts)
        {
            try
            {


                DateTime startdate;
                DateTime enddate;

                //startdate = (DateTime)db.ProjectPhases.FirstOrDefault(x => x.Phase == PhaseName).StartDate;
                //enddate = (DateTime)db.ProjectPhases.FirstOrDefault(x => x.Phase == PhaseName).EndDate;

                startdate = (DateTime)db.ProjectPhases.Where(x => x.ProjectPhaseId == PhaseId).Select(o => o.StartDate).FirstOrDefault();
                enddate = (DateTime)db.ProjectPhases.Where(x => x.ProjectPhaseId == PhaseId).Select(o => o.EndDate).FirstOrDefault();

                string start = startdate.ToString("dd/MM/yyyy");
                string end = enddate.ToString("dd/MM/yyyy");

                var userId = (int)Session["UserId"];
                var already = db.TS_Task.Where(x => x.StartDate == StartDate && x.EndDATE == EndDate && x.ProjectName == ProjectId && x.TaskName == TaskName && (x.IsDelete == false || x.IsDelete == null)).Select(o => o).FirstOrDefault();
                var alreadyname = db.TS_Task.Where(x => x.ProjectName == ProjectId && x.TaskName == TaskName).Select(o => o).FirstOrDefault();

                var exist = db.TS_Task.Where(x => (x.StartDate == StartDate || x.EndDATE == EndDate) && x.ProjectName == ProjectId && x.TaskName == TaskName && (x.IsDelete == false || x.IsDelete == null)).Select(o => o).FirstOrDefault();

                var avail = db.TS_Task.Where(x => x.ProjectName == ProjectId && x.TaskName == TaskName && (x.IsDelete == false || x.IsDelete == null)).Select(o => o).FirstOrDefault();

                bool overlapping = db.ProjectPhases.FirstOrDefault(x => EntityFunctions.TruncateTime(x.StartDate) <= EntityFunctions.TruncateTime(StartDate)
                             && EntityFunctions.TruncateTime(x.EndDate) >= EntityFunctions.TruncateTime(EndDate) && x.Phase == PhaseName && x.IsACTIVE == true) != null;

                int y = 0;


                List<int> Total = new List<int>();

                Total = db.TS_Task.Where(x => x.PhaseNameId == PhaseId && x.ISACTIVE == true).Select(o => o.NoOfEfforts).ToList();

                for (int z = 0; z < Total.Count(); z++)
                {

                    y += Total[z];

                }


                if (!overlapping)
                {
                    var result = new { message = "timeexceed", sdate = start, edate = end };
                    return Json(new { message = "timeexceed", sdate = start, edate = end });
                }
                if (already != null || alreadyname != null)
                {
                    return Json("AlreadyAssigned", JsonRequestBehavior.AllowGet);
                }

                if (exist != null)
                {
                    return Json("exist", JsonRequestBehavior.AllowGet);
                }

                if (avail != null)
                {
                    return Json("avail", JsonRequestBehavior.AllowGet);
                }

                var value = db.ProjectPhases.Where(x => x.ProjectPhaseId == PhaseId && x.IsACTIVE == true).Select(o => o.NoOfEfforts).FirstOrDefault();
                if ((y + Efforts) > value)
                {
                    return Json("greater", JsonRequestBehavior.AllowGet);
                }


                else
                {
                    var objtsk = db.TS_Task.CreateObject();
                    objtsk.UserId = userId;
                    objtsk.ProjectName = ProjectId;
                    objtsk.PhaseNameId = PhaseId;
                    objtsk.PhaseName = PhaseName;
                    objtsk.PhaseStatus = PhaseStatus;
                    objtsk.TaskName = TaskName;
                    objtsk.TaskStatus = Taskstatus;
                    objtsk.StartDate = StartDate;
                    objtsk.EndDATE = EndDate;
                    objtsk.NoOfEfforts = Efforts;
                    objtsk.ISACTIVE = true;
                    objtsk.IsDelete = false;
                    objtsk.IsOpened = true;
                    db.TS_Task.AddObject(objtsk);
                    db.SaveChanges();
                }
                Taskmodel obj = new Taskmodel();
                obj.newtask = 1;
                TempData["newtask"] = 1;
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
