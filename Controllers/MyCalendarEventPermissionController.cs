using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;

namespace DSRCManagementSystem.Controllers
{
    public class MyCalendarEventPermissionController : Controller
    {
        //
        // GET: /MyCalendarEventPermission/
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

        [HttpGet]
        public ActionResult CreateEventPermission()
        {
            try
            {
            var AuthUsers = (from ep in objdb.EventPermissions.Where(ep => ep.IsAurthorized == true)
                                 join u in objdb.Users.Where(u => u.IsActive == true && u.UserStatus != 6) on ep.UserID equals u.UserID
                             into evper
                             from eventper in evper.DefaultIfEmpty()
                             select new DSRCEmployees
                             {
                                 UId = eventper.UserID,
                                 Name = (eventper.FirstName + " " + eventper.LastName)??""
                             }).Where(u => u.Name != null && u.Name != "").ToList();
            ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "UId", "Name");

            var FilteredUsers = objdb.Users.Where(u => u.IsActive == true && u.UserStatus != 6).Select(x => x.UserID).ToList().
                Except(objdb.EventPermissions.Where(ep => ep.IsAurthorized == true || ep.IsAurthorized.Value).Select(x => x.UserID.Value).ToList()).ToList();
            List<object> UnAuthUsers = new List<object>();
            foreach (int users in FilteredUsers)
            {
                UnAuthUsers.AddRange(objdb.Users.Where(u => u.UserID == users).Select(u => new { userid = u.UserID, username = (u.FirstName + " " + u.LastName)??"" }).Where(u => u.username != null && u.username != "").ToList());
            }
            ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");

            TempData["UnAuthorize"] = UnAuthUsers.ToList();
            TempData["Authorize"] = AuthUsers.ToList();
            }
            catch(Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreateEventPermission(List<int> From, List<int> To)
        {
            var UnAuthUsers = TempData["UnAuthorize"];
            var AuthUsers = TempData["Authorize"];
            int Check = 0;
            if (To != null)
            {
                foreach (int users in To)
                {
                    if (users != 0)
                    {

                        var repeatcount = objdb.EventPermissions.Where(ep => ep.UserID == users).FirstOrDefault();
                        
                        if (repeatcount == null)
                        {
                            var varEventPermissions = objdb.EventPermissions.CreateObject();
                            varEventPermissions.UserID = users;
                            varEventPermissions.IsAurthorized = true;
                            objdb.EventPermissions.AddObject(varEventPermissions);
                            objdb.SaveChanges();
                            Check = Check+1;
                        }
                        else if(repeatcount.IsAurthorized==false)
                        {
                            repeatcount.IsAurthorized = true;
                            objdb.SaveChanges();
                            Check = Check + 1;
                        }
                    }
                }
            }
            if (From != null)
            {
                foreach (int users in From)
                {
                    if (users != 0)
                    {
                        var repeatcount = objdb.EventPermissions.Where(ep => ep.UserID == users).Select(u => u).FirstOrDefault();
                        if (repeatcount != null)
                        {
                            repeatcount.IsAurthorized = false;
                            objdb.SaveChanges();
                            Check = Check+2;
                        }
                    }
                }
            }
            if (Check == 1)
            {
                return Json("Authorize", JsonRequestBehavior.AllowGet);
            }
            else if (Check == 2)
            {
                return Json("UnAuthorize", JsonRequestBehavior.AllowGet);
            }
            else
            {
            return CreateEventPermission();
            }
        }
    }
}
