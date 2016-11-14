using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem;
using System.Web.SessionState;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Web.Helpers;

namespace DSRCManagementSystem.Controllers
{
    public class ThemeSettingsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ChangeColor(string ColorId)
        {
            int ID = 0;
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {

                if (ColorId == "")
                {
                    ID = 1;
                }
                else
                {
                    ID = Convert.ToInt32(ColorId);
                }
                var Themecolor = objdb.Master_ApplicationSettings.Where(x => x.AppID == 6).Select(o => o).FirstOrDefault();
                if (Themecolor != null)
                {
                    Themecolor.AppValue = objdb.Master_ThemeColors.Where(x => x.ColorId == ID).Select(o => o.ColorName).FirstOrDefault();
                    objdb.SaveChanges();
                    Session["Theme"] = Themecolor.AppValue;
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(ID, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult StartDateChange(string Date)
        {
            string strEndDate = "";
            try
            {
                DateTime startDate = Convert.ToDateTime(Date);
                DateTime endDate = startDate.AddDays(-1).AddYears(1);
                strEndDate = endDate.ToString("dd-MM-yyyy");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(strEndDate, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult DateChange(string StartDate)
        {
            return Json(StartDate, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ThemeSettings(string ColorId)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ThemeSettings objmodel = new DSRCManagementSystem.Models.ThemeSettings();
            try
            {
                var Themecolor = (from p in objdb.Master_ThemeColors
                                  select new
                                  {
                                      colorid = p.ColorId,
                                      colorname = p.ColorName,
                                      colorcode = p.ColorCode

                                  }).ToList();
                var startdate = "";
                var enddate = "";
                var InTime = "";
                var OutTime = "";
                
                //  var save = Convert.ToInt32(Session["Save"]);
                var value1 = Convert.ToString(TempData["startdate"]);
                var value2 = Convert.ToString(TempData["enddate"]);
                var Timein = Convert.ToString(TempData["InTime"]);
                var Timeout= Convert.ToString(TempData["OutTime"]);
                //var VersionNo= Convert.ToString(TempData["Version"]);

                if (value1 != "")
                {
                    startdate = value1;
                }
                else
                {
                    startdate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial Start Date").Select(o => o.AppValue).FirstOrDefault();
                }

                if (value2 != "")
                {
                    enddate = value2;
                }
                else
                {
                    enddate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial End Date").Select(o => o.AppValue).FirstOrDefault();
                }
                if (Timein != "")
                {
                    InTime = Timein;
                }
                else
                {
                    InTime = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "In Time").Select(o => o.AppValue).FirstOrDefault();
                }
                if (Timeout != "")
                {
                    OutTime = Timeout;
                }
                else
                {
                    OutTime = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Out Time").Select(o => o.AppValue).FirstOrDefault();
                }
              
                   
                

                DateTime d1 = Convert.ToDateTime(startdate);
                string d = d1.ToShortDateString();
                objmodel.AcademicStartdate = d;
                DateTime d2 = Convert.ToDateTime(enddate);
                string d3 = d2.ToShortDateString();
                objmodel.AcademicEnddate = d3;
                if (ColorId != null)
                {
                    int ID = Convert.ToInt32(ColorId);
                    objmodel.Colors = ID;
                    ViewBag.Colors = new SelectList(Themecolor, "colorid", "colorname", objmodel.Colors);
                }
                var dblogo = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Log").Select(o => o.AppValue).FirstOrDefault();
                var dbcolor = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "ThemeColor").Select(o => o.AppValue).FirstOrDefault();
                var colorid = objdb.Master_ThemeColors.Where(x => x.ColorName == dbcolor).Select(o => o.ColorId).FirstOrDefault();
                objmodel.Colors = colorid;
                Session["Color"] = dbcolor;
                string pathImage = HttpContext.Server.MapPath(dblogo.ToString().Replace("../../", "~/"));
                //if (save == 1)
                //{
                    objmodel.path = System.IO.File.Exists(pathImage) == true ? dblogo.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png"; ;
                    ViewBag.Colors = new SelectList(Themecolor, "colorid", "colorname", objmodel.Colors);
                //}
                //else
                //{
                //    objmodel.path = System.IO.File.Exists(pathImage) == true ? dblogo.ToString() : @"..\..\UsersData\Logo\Images\No_Image.png"; ;
                //    ViewBag.Colors = new SelectList(Themecolor, "colorid", "colorname", objmodel.Colors);
                //}
                    string defImage = "../../UsersData/Logo/Images/No_Image.png";
                    if (dblogo != defImage)
                    {
                        objmodel.HasImage = true;
                    }
                    else
                    {
                        objmodel.HasImage = false;
                    }
                var logo = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Log").Select(o => o).FirstOrDefault();
                Session["Logo"] = logo.AppValue;
                var facebook = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Facebook").Select(o => o).FirstOrDefault();                
                var companyName = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();
               var VersionNumber = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Version").Select(o => o).FirstOrDefault();
                objmodel.InTime = InTime;
                objmodel.OutTime = OutTime;
                objmodel.Facebook = facebook.AppValue;
                objmodel.CompanyName = companyName.AppValue;
                objmodel.VersionNumber = VersionNumber.AppValue;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult ThemeSettings(DSRCManagementSystem.Models.ThemeSettings collection)
        {
            ViewBag.Message = "";
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Master_ApplicationSettings obj = new DSRCManagementSystem.Master_ApplicationSettings();
            try
            {
                var Photopath = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Log").Select(o => o).FirstOrDefault();
                var fileExtension = collection.Photo != null ? Path.GetExtension(collection.Photo.FileName) : "";
                if (fileExtension == ".gif" || fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == "")
                {
                    if (collection.Photo != null)
                    {
                        string _filePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                        FileInfo fileInfo = new FileInfo(_filePath);
                        string directoryFullPath = fileInfo.DirectoryName + @"\DSRCHRMSRemainderService\Templates\logo.png";
                        bool ImgExists = System.IO.File.Exists(directoryFullPath);
                        var valpath = Path.Combine(directoryFullPath);
                        if (ImgExists == true)
                        {
                            FileInfo file = new FileInfo(directoryFullPath);
                            collection.Photo.SaveAs(valpath);
                        }
                        string FilePath = collection.Photo.FileName;
                        var fileName = Guid.NewGuid().ToString() + Path.GetFileName(collection.Photo.FileName);
                        var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/")), fileName);
                        //Create Directory if not created, Mostly happens during first execution
                        //Added by Prasanth k
                        if (!Directory.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/"))))
                        {
                            Directory.CreateDirectory(Server.MapPath(Url.Content("~/UsersData/Logo/Images/")));
                        }
                        try
                        {
                            WebImage img = new WebImage(collection.Photo.InputStream);
                            if (img.Width > 1000)
                            {
                                img.Resize(1000, 1000);
                            }
                            img.Save(path);

                            //     collection.Photo.SaveAs(path);
                        }
                        catch (Exception)
                        {

                        }
                        var Extension = collection.Photo != null ? Path.GetExtension(collection.Photo.FileName) : "";
                        if (Extension != null)
                        {
                            var Themelogo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();
                            string ImgPath = ConfigurationManager.AppSettings["ImgPath"].ToString() + fileName;
                            Themelogo.AppValue = ImgPath;
                            objdb.SaveChanges();
                            Session["Save"] = 1;
                        }

                    }
                    var Themecolor = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "ThemeColor").Select(o => o).FirstOrDefault();
                    var startdate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial Start Date").Select(o => o).FirstOrDefault();
                    var enddate = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Financial End Date").Select(o => o).FirstOrDefault();
                    var facebook = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Facebook").Select(o => o).FirstOrDefault();
                    var companyname = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();
                    var VersionNumber = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Version").Select(o => o).FirstOrDefault();
                    var InTime = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "In Time").Select(o => o).FirstOrDefault();
                    var OutTime = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Out Time").Select(o => o).FirstOrDefault();
                    DateTime dt1 = Convert.ToDateTime(collection.AcademicStartdate);
                    DateTime dt2 = Convert.ToDateTime(collection.AcademicEnddate);

                    if (dt1.Date > dt2.Date)
                    {
                        TempData["message"] = "Startdate";

                    }

                    else
                    {

                        if (startdate != null)
                        {
                            startdate.AppValue = collection.AcademicStartdate;
                            objdb.SaveChanges();
                        }
                        if (enddate != null)
                        {
                            enddate.AppValue = collection.AcademicEnddate;
                            objdb.SaveChanges();
                        }
                        if (facebook != null)
                        {
                            if (collection.Facebook == "" || collection.Facebook == null)
                            {
                                facebook.AppValue = facebook.AppValue;
                                objdb.SaveChanges();
                            }
                            else
                            {
                                facebook.AppValue = collection.Facebook;
                                objdb.SaveChanges();
                            }
                        }

                        if (companyname != null)
                        {
                            if (collection.CompanyName == "" || collection.CompanyName == null)
                            {
                                companyname.AppValue = companyname.AppValue;
                                objdb.SaveChanges();
                            }
                            else
                            {
                                companyname.AppValue = collection.CompanyName;
                                objdb.SaveChanges();
                            }
                        }

                        if (VersionNumber != null)
                        {
                            if (collection.VersionNumber == "" || collection.VersionNumber == null)
                            {
                                VersionNumber.AppValue  = VersionNumber.AppValue ;
                                objdb.SaveChanges();
                            }
                            else
                            {
                                VersionNumber.AppValue = collection.VersionNumber;
                                objdb.SaveChanges();
                            }
                        }

                        if (collection.Colors != null)
                        {
                            Themecolor.AppValue = objdb.Master_ThemeColors.Where(x => x.ColorId == collection.Colors).Select(o => o.ColorName).FirstOrDefault();
                            objdb.SaveChanges();
                            Session["Save"] = 1;
                        }
                        if (InTime != null)
                        {
                            InTime.AppValue = collection.InTime;
                            objdb.SaveChanges();
                        }
                        if (OutTime != null)
                        {
                            OutTime.AppValue = collection.OutTime;
                            objdb.SaveChanges();
                        }
                        TempData["startdate"] = collection.AcademicStartdate.ToString();
                        TempData["enddate"] = collection.AcademicEnddate.ToString();
                        TempData["InTime"] = collection.InTime.ToString();
                        TempData["OutTime"] = collection.OutTime.ToString();
                        objdb.SaveChanges();
                        var themelogo = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Log").Select(o => o).FirstOrDefault();
                        Session["LoginLogo"] = themelogo.AppValue;
                        TempData["doc"] = themelogo.AppValue;
                        TempData["message"] = "Added";
                        ViewBag.Message = "Success";
                    }
                }
                else
                {
                    TempData["message"] = "Wrongformat";

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return RedirectToAction("ThemeSettings", "ThemeSettings");
        }

        public ActionResult ResetImage(int ID)
        {
            //string fileName = Server.MapPath(@"..\..\UsersData\Logo\Images\No_Image.png");
            string fileName = "../../UsersData/Logo/Images/No_Image.png";

            byte[] defaultImage = System.IO.File.ReadAllBytes(Server.MapPath(fileName));
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
           DSRCManagementSystem.Master_ApplicationSettings obj = new DSRCManagementSystem.Master_ApplicationSettings();
           try
           {
               var dbImageLogo = db.Master_ApplicationSettings.FirstOrDefault(x => x.AppID == 7);
               dbImageLogo.AppValue = fileName;              
               db.SaveChanges();
           }
           catch(Exception Ex)
           {
               string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
               string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
               ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
           }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
