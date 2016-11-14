using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ManageProjectController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1(); //
        // GET: /ManageProject/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ProjectPermission()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var check = db.ViewallProjectPermissions.Select(x => x.UserId).ToList();
                List<int> LIST = new List<int>();
                foreach (var X in check)
                {
                    var list =
                        db.Users.Where(x => x.UserID == X && x.IsActive == true && x.UserStatus != 6)
                            .Select(o => o.UserID)
                            .FirstOrDefault();


                    if (list == 0)
                    {
                        var data = db.ViewallProjectPermissions.Where(o => o.UserId == X).Select(x => x).ToList();
                        foreach (var y in data)
                        {
                            y.IsAuthorized = false;
                            db.SaveChanges();

                        }
                        //LIST.Add(list);
                    }
                }


                var AuthUsers = (from ep in db.ViewallProjectPermissions.Where(ep => ep.IsAuthorized == true)
                                 join u in db.Users.Where(u => u.IsActive == true && u.UserStatus != 6) on ep.UserId equals u.UserID
                                     into evper
                                 from eventper in evper.DefaultIfEmpty()
                                 select new
                                 {
                                     userid = eventper.UserID,
                                     username = eventper.FirstName + " " + eventper.LastName
                                 }).ToList();

                ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "userid", "username");


                var FilteredUsers =
                        db.Users.Where(
                            u => u.IsActive == true  && u.UserStatus != 6)
                            .Select(x => x.UserID)
                            .ToList()
                            .
                            Except(
                                db.ViewallProjectPermissions.Where(ep => ep.IsAuthorized == true || ep.IsAuthorized.Value)
                                    .Select(x => x.UserId.Value)
                                    .ToList()).ToList();
                List<object> UnAuthUsers = new List<object>();
                foreach (int users in FilteredUsers)
                {
                    UnAuthUsers.AddRange(
                        db.Users.Where(u => u.UserID == users)
                            .Select(u => new { userid = u.UserID, username = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "") })
                            .ToList());
                }
                ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");
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
        public ActionResult ProjectPermission(List<int> From, List<int> To)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var deleteuser = db.ViewallProjectPermissions.Where(x => x.IsAuthorized == true).Select(o => o).ToList();
                foreach (var deluser in deleteuser)
                    db.ViewallProjectPermissions.DeleteObject(deluser);
                db.SaveChanges();
                for (int j = 0; j < To.Count(); j++)
                {
                    DSRCManagementSystem.ViewallProjectPermission objaccess = new DSRCManagementSystem.ViewallProjectPermission();
                    objaccess.UserId = To[j];
                    objaccess.IsAuthorized = true;
                    db.AddToViewallProjectPermissions(objaccess);
                    db.SaveChanges();
                }
                return Json("Authorize", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }

        }
        


    }
}
