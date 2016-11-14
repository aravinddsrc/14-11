using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Calendar()
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            ViewBag.LeaveTypeList = new SelectList(new[] { new LeaveType() { LeaveTypeId = 0, Name = "All Leave Types" } }.Union(db.LeaveTypes.Where(o => o.ApplicableEmployees == "All").ToList()), "LeaveTypeId", "Name", 0);
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
