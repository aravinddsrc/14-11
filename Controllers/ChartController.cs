using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ChartController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult Chart()
        {
            return View();
        }
        public ActionResult ChartData()
        {
            List<object> List = new List<object>();
            try{
            var currentmonth = DateTime.Now.Month;
            var currentyear = DateTime.Now.Year;
            for (int i = 1; i <= 12; i++)
            {
                var Data = db.AuditLogs.Where(a => a.LogedInDate.Value.Month == i && a.LogedInDate.Value.Year == currentyear).Select(d => d.LogedInDate.Value.Month).Count();
                var Val = new { m = currentyear + "-" + i, a = Data };
                List.Add(Val);
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }
    }
}
