using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.App_Start
{
    public class SessionExpireFilterAttribute : AuthorizeAttribute
    {
        
        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            context.HttpContext.Response.TrySkipIisCustomErrors = true;

            if (context.HttpContext.Request.IsAjaxRequest())
            {
                var urlHelper = new UrlHelper(context.RequestContext);
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult
                {
                    Data = new
                    {
                        Error = "NotAuthorized",
                        RedirectURL = "/User/SessionExpired"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

        }


    }
}