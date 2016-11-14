using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace DSRCManagementSystem.DSRCLogic
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    
    public class DSRCAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            IPrincipal user = httpContext.User;
            var sessionObj = httpContext.Session;

            return (user.Identity.IsAuthenticated
                && (String.IsNullOrEmpty(Roles) || Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Select(i => i.Trim()).Any(role => role.Equals(sessionObj["RoleName"].ToString(), StringComparison.OrdinalIgnoreCase)))
                && (String.IsNullOrEmpty(Users) || Users.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Select(i => i.Trim()).Any(u => u.Equals(sessionObj["RoleName"].ToString(), StringComparison.OrdinalIgnoreCase))));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var routeDictionary = new RouteValueDictionary { { "action", "Error" }, {"controller", "Home"}, { "message", "Authorization is restricted" } };
            filterContext.Result = new RedirectToRouteResult(routeDictionary);
        }
    }    
}