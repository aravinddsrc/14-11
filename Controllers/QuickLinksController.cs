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

namespace DSRCManagementSystem.Controllers
{
    public class QuickLinksController : Controller
    {
       
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult QuickLinks()
        {
            DSRCManagementSystem.Models.QuickLinks objmodel = new DSRCManagementSystem.Models.QuickLinks();
            var userid = Convert.ToInt32(Session["UserID"].ToString());
            var LinksDetails = (from p in db.ManageQuickLinks
                                join m in db.Modules on p.PageModuleID equals m.PageModuleID
                                where p.IsActive == true && p.UserID == userid
                                select new QuickLinks()
                             {
                                 path = p.IconPath,
                                 QuickLinkID = p.QuickLinkID,
                                 PageModuleID = m.PageModuleID,
                                 ModuleName = m.ModuleName,
                                 DisplayName = p.DisplayName

                             }).ToList();


            return View(LinksDetails);



        }
        [HttpGet]
        public ActionResult AddLinks()
        {

            DSRCManagementSystem.Models.QuickLinks objmodel = new DSRCManagementSystem.Models.QuickLinks();
            try
            {

                var userid = Convert.ToInt32(Session["UserID"].ToString());
                var RoleID = Session["RoleId"];
                var Functions = db.Functions.ToList();

                var modules = from p in db.Modules
                               join f in db.RoleFunctionPrivileges on p.PageModuleID equals f.PageModuleID
                               join q in db.UserRoles on f.RoleID equals q.RoleID
                               where q.UserID == userid
                                select p ;
                ViewBag.ModuleList = new SelectList(new[] { new Module() { PageModuleID = 0, ModuleName = "" } }.Union(modules.Distinct()).OrderBy(o=>o.ModuleName), "PageModuleID", "ModuleName");

                objmodel.path = "../../UsersData/Logo/Images/No_Image.png";
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return View(objmodel);
        }
        [HttpGet]
        public ActionResult EditLinks(int QuickLinkID)
        {


            var EditQuickLinks = new QuickLinks();

            var EditQuickLinkId = db.ManageQuickLinks.Where(q => q.QuickLinkID == QuickLinkID && q.IsActive ==true).Select(r => r).FirstOrDefault();
            try
            {
                var userid = Convert.ToInt32(Session["UserID"].ToString());

                var modules = db.Modules.ToList();
                ViewBag.ModuleList = new SelectList(new[] { new Module() { PageModuleID = 0, ModuleName = "---Select---" } }.Union(modules), "PageModuleID", "ModuleName", EditQuickLinkId.PageModuleID);

                if (EditQuickLinkId != null)
                {

                    EditQuickLinks.path = EditQuickLinkId.IconPath;

                    EditQuickLinks.PageModuleID = EditQuickLinkId.PageModuleID;
                    EditQuickLinks.ModuleName = db.Modules.FirstOrDefault(q => q.PageModuleID == EditQuickLinkId.PageModuleID).ModuleName;
                    EditQuickLinks.DisplayName = EditQuickLinkId.DisplayName;


                    if (EditQuickLinks.path != null)
                    {
                        string pathImage = HttpContext.Server.MapPath(EditQuickLinks.path.ToString().Replace("../../", "~/"));
                        EditQuickLinks.path = System.IO.File.Exists(pathImage) == true ? EditQuickLinks.path.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png"; ;

                    }
                    else
                    {
                        EditQuickLinks.path = @"..\..\UsersData\Logo\Images\No_Image.png";
                    }


                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return View(EditQuickLinks);
        }

        [HttpPost]
        public ActionResult AddLinks(QuickLinks Links)
        {
            try
            {
                var AddLinks = db.ManageQuickLinks.CreateObject();
                var userid = Convert.ToInt32(Session["UserID"].ToString());
                var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                string IconPath = ConfigurationManager.AppSettings["MenuIconPath"].ToString();

                var CoverPhotoName = Path.GetFileName(httpPostedFile.FileName);
                var FolderPath = Server.MapPath(IconPath);
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }


                var pageID = db.ManageQuickLinks.Where(x => x.PageModuleID == Links.PageModuleID && x.IsActive == true && x.UserID == userid).ToList();

    //2-9-16
                string AllImages = ".png;.jpg;.jpeg;.jfif;.bmp;.tif;.tiff;.gif;.icon";
                List<string> lstimgTypes = new List<string>();
                string[] imgtypes = AllImages.Split(';');    
                lstimgTypes.AddRange(imgtypes);
                if (lstimgTypes.Contains(Path.GetExtension(CoverPhotoName).ToLower()) == false)
                {
                    return Json(new { Result = "NotImage" }, JsonRequestBehavior.AllowGet);
                }
  


                Image image = Image.FromStream(httpPostedFile.InputStream, true, true);
                if (pageID.Count > 0)
                {
                    return Json(new { Result = "Exists" }, JsonRequestBehavior.AllowGet);

                }
                else if (image.Width < 30 || image.Height < 30)
                {
                    return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var path = Path.Combine(Server.MapPath(IconPath), httpPostedFile.FileName);
                    image.Save(path);
                    AddLinks.PageModuleID = Links.PageModuleID;
                    AddLinks.DisplayName = Links.DisplayName;
                    AddLinks.UserID = Convert.ToInt32(Session["UserID"].ToString());
                    AddLinks.IconPath = ConfigurationManager.AppSettings["MenuIconPath"].ToString() + CoverPhotoName;
                    AddLinks.CreatedBy = Convert.ToInt32(Session["UserID"].ToString());
                    AddLinks.CreatedDate = DateTime.Now;
                    AddLinks.IsActive = true;
                    db.ManageQuickLinks.AddObject(AddLinks);
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }



            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
           

        }

        public ActionResult EditLinks(QuickLinks Links)
        {
            try
            {
                var EditQuickLinks = db.ManageQuickLinks.Where(q => q.QuickLinkID == Links.QuickLinkID).Select(r => r).FirstOrDefault();
                var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                string IconPath = ConfigurationManager.AppSettings["MenuIconPath"].ToString();

                if (httpPostedFile != null)// added on 9/1/2016
                {
                    var CoverPhotoName = Path.GetFileName(httpPostedFile.FileName);
                    var FolderPath = Server.MapPath(IconPath);
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    //2-9-16
                    string AllImages = ".png;.jpg;.jpeg;.jfif;.bmp;.tif;.tiff;.gif;.icon";
                    List<string> lstimgTypes = new List<string>();
                    string[] imgtypes = AllImages.Split(';');
                    lstimgTypes.AddRange(imgtypes);
                    if (lstimgTypes.Contains(Path.GetExtension(CoverPhotoName).ToLower()) == false)
                    {
                        return Json(new { Result = "NotImage" }, JsonRequestBehavior.AllowGet);
                    }
  

                    var filepath = Server.MapPath(IconPath + "/" + CoverPhotoName);
                    Image image = Image.FromStream(httpPostedFile.InputStream, true, true);


                    if ((image.Width < 30 || image.Height < 30))
                    {
                        return Json(new { Result = "Invalid" }, JsonRequestBehavior.AllowGet);

                    }

                    else
                    {

                        var path = Path.Combine(Server.MapPath(IconPath), httpPostedFile.FileName);
                        image.Save(path);
                        EditQuickLinks.DisplayName = Links.DisplayName;
                        EditQuickLinks.UserID = Convert.ToInt32(Session["UserID"].ToString());
                        EditQuickLinks.PageModuleID = Links.PageModuleID;
                        EditQuickLinks.IconPath = ConfigurationManager.AppSettings["MenuIconPath"].ToString() + CoverPhotoName;
                        EditQuickLinks.ModBy = Convert.ToInt32(Session["UserID"].ToString());
                        EditQuickLinks.ModDate = DateTime.Now;
                        db.SaveChanges();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                   

                }
                    //added on 9/1/2016
                else
                {
                    EditQuickLinks.DisplayName = Links.DisplayName;
                    EditQuickLinks.UserID = Convert.ToInt32(Session["UserID"].ToString());
                    EditQuickLinks.PageModuleID = Links.PageModuleID;
                    //EditQuickLinks.IconPath = ConfigurationManager.AppSettings["MenuIconPath"].ToString() + CoverPhotoName;
                    EditQuickLinks.ModBy = Convert.ToInt32(Session["UserID"].ToString());
                    EditQuickLinks.ModDate = DateTime.Now;
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);// added
                }
                // ends

            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
           

        }
        [HttpPost]
        public ActionResult Delete(int QuickLinkID)
        {
            try
            {
                var DeleteQuickLinks = db.ManageQuickLinks.Where(q => q.QuickLinkID == QuickLinkID && q.IsActive ==true).Select(r => r).FirstOrDefault();
                DeleteQuickLinks.IsActive = false;
                DeleteQuickLinks.ModBy = Convert.ToInt32(Session["UserID"].ToString());
                DeleteQuickLinks.ModDate = DateTime.Now;
                db.SaveChanges();

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
            
        }


    
    }
}
