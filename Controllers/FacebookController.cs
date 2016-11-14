using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class FacebookController : Controller
    {
        //
        // GET: /Facebook/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DsrcFacebook()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ThemeSettings objsettings = new DSRCManagementSystem.Models.ThemeSettings();
            try{
            var facebookurl = objdb.Master_ApplicationSettings.Where(x => x.AppID == 13).Select(o => o).FirstOrDefault();
            var url = facebookurl.AppValue.ToString();
            objsettings.Facebook = url;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objsettings);
        }
       
    }
}
