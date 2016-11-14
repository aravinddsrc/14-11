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
    public class StopsController : Controller
    {
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
        //
        // GET: /Stops/

        [HttpGet]
        public ActionResult AddStops() {
            return View();
        }

        [HttpPost]
        public ActionResult AddStops(DSRCManagementSystem.Models.StopList objmodel) {
            DSRCManagementSystem.Stop obj = new DSRCManagementSystem.Stop();
            DSRCManagementSystem.Models.StopList obs = new DSRCManagementSystem.Models.StopList();
            var val = objdb.Stops.Where(x => x.Stop_Name == objmodel.Stop_Name ).Select(o => o).FirstOrDefault();
            if (val == null) {
                obj.Stop_Name = objmodel.Stop_Name;
                obj.IsActive = true;
                objdb.AddToStops(obj);
                objdb.SaveChanges();
            }
            else {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageStops() {
            var StopDetails = (from s in objdb.Stops
                               where s.IsActive==true 
                               select new StopList() {
                                   StopId = s.StopId,
                                   Stop_Name = s.Stop_Name
                               });
            return View(StopDetails);
        }

        [HttpGet]
        public ActionResult EditStops(int StopId) {
            
            DSRCManagementSystem.Models.StopList obs = new DSRCManagementSystem.Models.StopList();
            var val = objdb.Stops.Where(x => x.StopId == StopId).Select(o => o).FirstOrDefault();
            obs.StopId = val.StopId;
            obs.Stop_Name = val.Stop_Name;            
            return View(obs);
        }

        [HttpPost]
        public ActionResult EditStops(DSRCManagementSystem.Models.StopList objmodel)
        {
            DSRCManagementSystem.Stop objs = new DSRCManagementSystem.Stop();
            DSRCManagementSystem.Models.StopList objsl = new DSRCManagementSystem.Models.StopList();
            var val = objdb.Stops.Where(x => x.StopId == objmodel.StopId).Select(o => o).FirstOrDefault();
            var res = objdb.Stops.Where(x => x.Stop_Name == objmodel.Stop_Name).Select(o => o).FirstOrDefault();
            if (res == null) {
                val.Stop_Name = objmodel.Stop_Name;
                objdb.SaveChanges();
            }
            else {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int StopId) {            
            DSRCManagementSystem.Models.StopList obs = new DSRCManagementSystem.Models.StopList();
            var val = objdb.Stops.Where(x => x.StopId == StopId).Select(o => o).FirstOrDefault();
            val.IsActive = false;
            objdb.SaveChanges();
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

    }
}