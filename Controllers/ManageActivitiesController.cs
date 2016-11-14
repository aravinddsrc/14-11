using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Web.SessionState;
using System.Net.Mail;
using DSRCManagementSystem;
using System.Net;
using System.Web.Security;
using System.Text.RegularExpressions;
using DSRCManagementSystem.Models.Domain_Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Management;
using System.Globalization;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Configuration;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;

namespace DSRCManagementSystem.Controllers
{
    public class ManageActivitiesController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult ManageActivities()
        {
            List<DSRCManagementSystem.Models.ManageActivities> objmodel = new List<Models.ManageActivities>();
            try{
            List<ManageActivities> Value = new List<ManageActivities>();
            Value = (from a in db.Activities
                     where (a.IsActive == true)



                     select new ManageActivities()
                     {
                         ActivityId = a.ActivityID,
                         ActivityName = a.Activity1,
                         ActivityDescription = a.ActivityDescription,


                     }).OrderBy(o => o.ActivityName).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }
        [HttpPost]
        public ActionResult ManageActivities(ManageActivities model)
        {
            try{
            var ActivityName = model.ActivityName.Trim();
            var ActivityDescription = model.ActivityDescription.Trim();

            var temp = db.Activities.Where(r => r.Activity1 == ActivityName && r.IsActive == true).Select(f => f.Activity1);

            foreach (var check in temp)
            {

                if (check == ActivityName)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

            }

            {

                var Assignobj = db.Activities.CreateObject();
                Assignobj.Activity1 = ActivityName;
                Assignobj.ActivityDescription = ActivityDescription;
                db.Activities.AddObject(Assignobj);
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult EditActivities(int ActivityId, string ActivityName,string ActivityDescription)
        {
            try{
            ViewBag.ActivityId = ActivityId;
            ViewBag.ActivityName = ActivityName;
            ViewBag.ActivityDescription = ActivityDescription;
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
        public ActionResult EditActivities(ManageActivities Model)
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int ActivityId = Model.ActivityId;
            string ActivityName = Model.ActivityName.Trim();
            string ActivityDescription = "";
            if (Model.ActivityDescription != null)
            {
                ActivityDescription = Model.ActivityDescription.Trim();
            }
          

            var data = db.Activities.Where(o => o.ActivityID != ActivityId && o.IsActive == true).Select(o => o.Activity1);

            foreach (var check in data)
            {

                if (check == ActivityName)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

            }
            var datas = db.Activities.Where(o => o.ActivityID == ActivityId).Select(o => o).FirstOrDefault();
            {
                datas.Activity1 = ActivityName;
                datas.ActivityDescription = ActivityDescription;
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success1", JsonRequestBehavior.AllowGet);


        }

        public ActionResult Delete(int Id)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var data = db.Activities.Where(o => o.ActivityID == Id && o.IsActive == true).Select(o => o).FirstOrDefault();
                data.IsActive = false;
                db.SaveChanges();
            }
        catch (Exception Ex)
        {
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

        }
         return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult AddActivity()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddActivity(ManageActivities model)
        {
            try
            {
                var ActivityName = model.ActivityName.Trim();
                var ActivityDescription = "";
                if (model.ActivityDescription != null)
                {
                    ActivityDescription = model.ActivityDescription.Trim();
                }


                var temp = db.Activities.Where(r => r.Activity1 == ActivityName && r.IsActive == true).Select(f => f.Activity1);

                foreach (var check in temp)
                {

                    if (check == ActivityName)
                    {

                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }

                {

                    var Assignobj = db.Activities.CreateObject();
                    Assignobj.Activity1 = ActivityName;
                    Assignobj.ActivityDescription = ActivityDescription;
                    Assignobj.IsActive = true;
                    db.Activities.AddObject(Assignobj);
                    db.SaveChanges();
                }
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
