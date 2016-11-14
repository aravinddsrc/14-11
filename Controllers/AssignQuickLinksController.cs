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
using System.Web.Helpers;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;



namespace DSRCManagementSystem.Controllers
{
    public class AssignQuickLinksController : Controller
    {
        
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AssignQuickLinks()
        {
            QuickLinks ObjAS = new QuickLinks();
            List<DSRCManagementSystem.Models.QuickLinks> Links = new List<DSRCManagementSystem.Models.QuickLinks>();
            try
            {

                var userid = Convert.ToInt32(Session["UserID"].ToString());
                Links = (from p in db.ManageQuickLinks

                         where p.IsActive == true && p.UserID == userid
                         select new QuickLinks()
                         {
                             path = p.IconPath,
                             QuickLinkID = p.QuickLinkID,
                             PageModuleID = p.PageModuleID,
                             DisplayName = p.DisplayName

                         }).ToList();

                ObjAS.links = Links;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return View(ObjAS);
        }
        [HttpPost]
        public ActionResult AssignQuickLinks(QuickLinks AssignQuickLinks)
        {
            QuickLinks ObjAS = new QuickLinks();
            try
            {


                List<DSRCManagementSystem.Models.QuickLinks> Links = new List<DSRCManagementSystem.Models.QuickLinks>();
                var AddLinks = db.AssignQuickLinks.CreateObject();
                int userId = Convert.ToInt32(Session["UserID"]);
                //var toInsert=null; 
                //var toDelete;

                var newLinks = new List<Int32?>();
                newLinks = AssignQuickLinks.selectedQuickLinkId;

                var oldQuickLinks = db.AssignQuickLinks.Where(x => x.UserID == userId).Select(x => x.QuickLinkID).ToList();

                if (newLinks != null)
                {
                    var toInsert = newLinks.Except(oldQuickLinks).ToList();
                    var toDelete = oldQuickLinks.Except(newLinks).ToList();


                    if (toInsert != null)
                    {
                        foreach (var item in toInsert.Distinct())
                        {
                            var insertNew = new AssignQuickLink()
                            {
                                QuickLinkID = Convert.ToInt32(item),
                                UserID = userId,
                                IsActive = true,
                                ModBy = userId,
                                ModDate = DateTime.Now,

                            };

                            db.AssignQuickLinks.AddObject(insertNew);
                        }


                    }
                    if (toDelete != null)
                    {
                        foreach (var item in toDelete)
                        {
                            int quicklinkid = Convert.ToInt32(item);
                            var data = (from d in db.AssignQuickLinks
                                        where d.UserID == userId && d.QuickLinkID == quicklinkid
                                        select d).FirstOrDefault();


                            db.AssignQuickLinks.DeleteObject(data);
                        }


                    }
                    db.SaveChanges();

                    Links = (from p in db.ManageQuickLinks

                             where p.IsActive == true && p.UserID == userId
                             select new QuickLinks()
                             {
                                 path = p.IconPath,
                                 QuickLinkID = p.QuickLinkID,
                                 PageModuleID = p.PageModuleID,
                                 DisplayName = p.DisplayName

                             }).ToList();

                    ObjAS.links = Links;


                }

                // added if selected list is null 
                if(newLinks == null) 
                {

                    var toDelete = oldQuickLinks.ToList();
                    if (toDelete != null)
                    {
                        foreach (var item in toDelete)
                        {
                            int quicklinkid = Convert.ToInt32(item);
                            var data = (from d in db.AssignQuickLinks
                                        where d.UserID == userId && d.QuickLinkID == quicklinkid
                                        select d).FirstOrDefault();


                            db.AssignQuickLinks.DeleteObject(data);
                        }


                    }
                    db.SaveChanges();

                    Links = (from p in db.ManageQuickLinks

                             where p.IsActive == true && p.UserID == userId
                             select new QuickLinks()
                             {
                                 path = p.IconPath,
                                 QuickLinkID = p.QuickLinkID,
                                 PageModuleID = p.PageModuleID,
                                 DisplayName = p.DisplayName

                             }).ToList();

                    ObjAS.links = Links;

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
           // return RedirectToAction("Index", "Home");
            return RedirectToAction("QuickLinks", "QuickLinks"); // redirect to quicklinks page

        }
        [HttpGet]
        public ActionResult GetMenuForRole(QuickLinks Links)
        {
            
            int userId = Convert.ToInt32(Session["UserID"]);

            var links = (from p in db.ManageQuickLinks
                         join q in db.AssignQuickLinks on p.QuickLinkID equals q.QuickLinkID
                         where p.IsActive == true && q.UserID == userId && q.IsActive == true && p.UserID == userId
                         select new
                         {
                             QuickLinkId = p.QuickLinkID,


                         }).ToList();


            return Json(links, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Reset()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            var links = (from p in db.ManageQuickLinks
                         join q in db.AssignQuickLinks on p.QuickLinkID equals q.QuickLinkID
                         where p.IsActive == true && q.UserID == userId && q.IsActive == true && p.UserID == userId
                         select new
                         {
                             QuickLinkId = p.QuickLinkID,


                         }).ToList();


            return Json(links, JsonRequestBehavior.AllowGet);
        }
    }
   }
