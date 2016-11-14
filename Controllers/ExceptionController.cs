using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
namespace DSRCManagementSystem.Controllers
{
    public class ExceptionController : Controller
    {
        //
        // GET: /Exception/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Error404(int ErrorCode, string ErrorMessage)
        {
            
                ErrorModel model = new ErrorModel();
                try
                {
                model.ErrorCode = ErrorCode;
                model.ErrorMessage = ErrorMessage;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(model);
        }

    }
}
