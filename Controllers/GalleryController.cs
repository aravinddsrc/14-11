using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using DSRCManagementSystem.Models;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.Web.Helpers;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace DSRCManagementSystem.Controllers
{
    public class GalleryController : Controller
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Album(string year)
        {
            List<DSRCManagementSystem.Models.Gallery> objphoto = new List<DSRCManagementSystem.Models.Gallery>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int AlbumYear = Convert.ToInt32(year);
                int Year = (AlbumYear == null) ? 0 : AlbumYear;
                int LeaveUserId = (int)Session["UserId"];
                var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();


                var userid = Convert.ToInt32(Session["UserID"]);
                var roleid = Convert.ToInt32(Session["RoleId"]);

                ViewBag.CreateAlbum = (from p in db.AlbumPermissions
                                       where p.UserId == userid && p.IsAuthorized == true
                                       select p.UserId).SingleOrDefault();
                ViewBag.UserID = Convert.ToInt32(Session["UserID"]);

                List<Albums> AccessUsers = new List<Albums>();

                AccessUsers = (from a in db.AlbumAccesses
                               where a.UserId == userid || a.RoleID == roleid || a.RoleID == null && a.UserId == null
                               select new Albums
                               {
                                   albumid = a.AlbumID
                               }).ToList();

                List<Albums> DefaultUser = new List<Albums>();
                List<int?> obj = new List<int?>();
                DefaultUser = (from a in db.DefaultAlbumUsers
                               where a.UserID == userid && a.IsActive == true
                               select new Albums
                               {
                                   albumid = a.AlbumID
                               }).ToList();

                var checkTag = db.TaggedAlbums.Select(x => x.AlbumId).ToList();

                for (int i = 0; i < AccessUsers.Count(); i++)
                {
                    obj.Add(Convert.ToInt32(AccessUsers[i].albumid));
                }
                for (int i = 0; i < DefaultUser.Count(); i++)
                {
                    obj.Add(Convert.ToInt32(DefaultUser[i].albumid));
                }
                if (Year != null && Year != 0 && Year != 1)
                {

                    var Years1 = db.Albums.Where(x => x.IsActive == true && x.EventDate != null).OrderByDescending(x => x.EventDate.Value.Year).ThenBy(x => x.EventDate.Value.Month).ThenBy(x => x.EventDate.Value.Day).Select(o => o.EventDate.Value.Year).Distinct().ToList();
                    ViewBag.Years = new SelectList(Years1, Year);

                    objphoto = (from p in db.Albums.Where(x => x.IsActive == true && obj.Contains(x.AlbumID) && x.EventDate.Value.Year == Year /*&& !checkTag.Contains(x.AlbumID)*/)
                                join t in db.AlbumPhotos.Where(x => x.IsActive == true) on p.AlbumID equals t.AlbumID into j
                                select new DSRCManagementSystem.Models.Gallery
                                {
                                    Photocount = j.Count(a => a.AlbumID == p.AlbumID),
                                    AlbumID = p.AlbumID,
                                    AlbumTitle = p.AlbumTitle,
                                    AlbumDescription = p.AlbumDescription,
                                    CoverPhotoPath = p.AlbumCoverPhotoPath,
                                    CreatedBy = p.CreatedBy,
                                    EventDate = (DateTime)p.EventDate

                                }).ToList();
                    return View(objphoto);
                }
                else
                {
                    int currentYear = DateTime.Today.Year;
                    var Years = db.Albums.Where(x => x.IsActive == true && x.EventDate != null).OrderByDescending(x => x.EventDate.Value.Year).ThenBy(x => x.EventDate.Value.Month).ThenBy(x => x.EventDate.Value.Day).Select(o => o.EventDate.Value.Year).Distinct().ToList();
                    ViewBag.Years1 = new SelectList(Years, currentYear);

                    objphoto = (from p in db.Albums.Where(x => x.IsActive == true && obj.Contains(x.AlbumID) && x.EventDate.Value.Year == currentYear /*&& !checkTag.Contains(x.AlbumID)*/)
                                join t in db.AlbumPhotos.Where(x => x.IsActive == true) on p.AlbumID equals t.AlbumID into j
                                select new DSRCManagementSystem.Models.Gallery
                                {
                                    Photocount = j.Count(a => a.AlbumID == p.AlbumID),
                                    AlbumID = p.AlbumID,
                                    AlbumTitle = p.AlbumTitle,
                                    AlbumDescription = p.AlbumDescription,
                                    CoverPhotoPath = p.AlbumCoverPhotoPath,
                                    CreatedBy = p.CreatedBy,
                                    EventDate = (DateTime)p.EventDate

                                }).ToList();

                    return View(objphoto);

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objphoto);

        }


        [HttpGet]
        public ActionResult CreateAlbum()
        {
            Gallery obj = new Gallery();
            try
            {
                int sessionuserid = Convert.ToInt32(Session["UserID"]);
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                ViewBag.AlbumExist = (from p in db.Albums
                                      where p.IsActive == true
                                      select p.AlbumTitle.ToUpper()).ToArray();

                var AlbumAccessRoles = (from p in db.Master_Roles.Where(x => x.IsActive == true)
                                        select new
                                        {
                                            RoleID = p.RoleID,
                                            RoleDescription = p.RoleName

                                        }).ToList();

                ViewBag.AlbumAccessRoles = new MultiSelectList(AlbumAccessRoles, "RoleID", "RoleDescription");

                var AlbumAccessUsers = (from p in db.Users.Where(x => x.IsActive == true)
                                        select new
                                        {
                                            UserID = p.UserID,
                                            UserDescription = p.FirstName + " " + (p.LastName.Length > 0 ? p.LastName : "")
                                        }).ToList();

                ViewBag.AlbumAccessUsers = new MultiSelectList(AlbumAccessUsers, "UserID", "UserDescription");
                ViewBag.TagUsers = new MultiSelectList((from u in db.Users.Where(x => x.IsActive == true)
                                                        select new
                                                        {
                                                            UserId = u.UserID,
                                                            FullName = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                                                        }).ToList(), "UserId", "FullName");


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(obj);
        }

        [HttpPost]
        public ActionResult CreateAlbum(Gallery collection)
        {
            try
            {
                int sessionuserid = Convert.ToInt32(Session["UserID"]);
                var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var obj = objdb.Albums.CreateObject();
                var obj1 = objdb.DefaultAlbumUsers.CreateObject();
                
                int AlbumID;
                string AlbumPath = ConfigurationManager.AppSettings["AlbumPath"].ToString() + collection.AlbumTitle;

                if (httpPostedFile != null)
                {
                    var AlbumTitle = collection.AlbumTitle;
                    var CoverPhotoName = Path.GetFileName(httpPostedFile.FileName);
                    var FolderPath = Server.MapPath(AlbumPath);
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }

                    var filepath = Server.MapPath(AlbumPath + "/" + CoverPhotoName);
                    Image image = Image.FromStream(httpPostedFile.InputStream, true, true);
                    if (image.Width > 1024 || image.Height > 768)
                    {
                        int newwidthimg = 1024;
                        float AspectRatio = (float)image.Size.Width / (float)image.Size.Height;
                        int newHeight = 768;
                        Image thumbNail = new Bitmap(newwidthimg, newHeight, image.PixelFormat);
                        Graphics g = Graphics.FromImage(thumbNail);
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        Rectangle rect = new Rectangle(0, 0, newwidthimg, newHeight);
                        g.DrawImage(image, rect);
                        thumbNail.Save(Server.MapPath(AlbumPath + "/" + httpPostedFile.FileName + ".jpg"), ImageFormat.Jpeg);
                    }
                    else
                    {
                        image.Save(Server.MapPath(AlbumPath + "/" + httpPostedFile.FileName + ".jpg"), ImageFormat.Jpeg);
                    }

                    string CreatedBy = Convert.ToString(Session["FirstName"]) + " " + Convert.ToString(Session["LastName"]);
                    DateTime CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                    obj.AlbumTitle = AlbumTitle;
                    obj.AlbumDescription = collection.AlbumDescription;
                    obj.AlbumCoverPhotoPath = ConfigurationManager.AppSettings["AlbumPath"].ToString() + AlbumTitle + "/" + CoverPhotoName + ".jpg";
                    obj.CreatedDate = CreatedOn;
                    obj.CreatedBy = CreatedBy;
                    obj.EventDate = collection.EventDate;
                    obj.IsActive = true;
                    objdb.Albums.AddObject(obj);
                    objdb.SaveChanges();

                    AlbumID = (from p in objdb.Albums
                               where p.AlbumTitle == collection.AlbumTitle && p.IsActive == true
                               select p.AlbumID).SingleOrDefault();
                    
                    obj1.AlbumID = AlbumID;
                    obj1.UserID = Convert.ToInt32(Session["UserID"]);
                    obj1.IsActive = true;
                    objdb.DefaultAlbumUsers.AddObject(obj1);
                    objdb.SaveChanges();

                    if (collection.TagUsers != null && collection.TagUsers != "" )
                    {
                        string[] value = collection.TagUsers.Split(',');

                        for (int i = 0; i < value.Count(); i++)
                        {
                            var objTagAlbum = objdb.TaggedAlbums.CreateObject();
                            objTagAlbum.AlbumId = AlbumID;
                            objTagAlbum.UserId = Convert.ToInt32(value[i]);
                            objdb.TaggedAlbums.AddObject(objTagAlbum);
                            objdb.SaveChanges();
                        }
                    }

                    if (collection.AlbumRole != null)
                    {
                        List<int?> objrole = new List<int?>();

                        string[] role = collection.AlbumRole.Split(',');

                        for (int i = 0; i < role.Count(); i++)
                        {
                            objrole.Add(Convert.ToInt32(role[i]));
                        }

                        for (int i = 0; i < objrole.Count(); i++)
                        {
                            DSRCManagementSystem.AlbumAccess objaccess = new DSRCManagementSystem.AlbumAccess();
                            objaccess.AlbumID = AlbumID;
                            objaccess.RoleID = objrole[i];
                            objdb.AddToAlbumAccesses(objaccess);
                            objdb.SaveChanges();
                        }

                    }
                    else if (collection.AlbumUser != null)
                    {
                        List<int?> objuser = new List<int?>();

                        string[] user = collection.AlbumUser.Split(',');

                        for (int i = 0; i < user.Count(); i++)
                        {
                            objuser.Add(Convert.ToInt32(user[i]));
                        }
                        for (int i = 0; i < objuser.Count(); i++)
                        {
                            DSRCManagementSystem.AlbumAccess objaccess1 = new DSRCManagementSystem.AlbumAccess();
                            objaccess1.AlbumID = AlbumID;
                            objaccess1.UserId = objuser[i];
                            objdb.AddToAlbumAccesses(objaccess1);
                            objdb.SaveChanges();
                        }
                    }
                    else
                    {
                        DSRCManagementSystem.AlbumAccess objaccess2 = new DSRCManagementSystem.AlbumAccess();
                        objaccess2.AlbumID = AlbumID;
                        objdb.AddToAlbumAccesses(objaccess2);
                        objdb.SaveChanges();
                    }
                    //var tagusers = (from u in objdb.Users.Where(x => x.IsActive == true && x.UserID != sessionuserid)
                    //                select new
                    //                {
                    //                    UserId = u.UserID,
                    //                    FullName = u.FirstName + " " + u.LastName,
                    //                }).ToList();
                    ViewBag.TagUsers = new MultiSelectList((from u in objdb.Users.Where(x => x.IsActive == true && x.UserID != sessionuserid)
                                                            select new
                                                            {
                                                                UserId = u.UserID,
                                                                FullName = u.FirstName + " " + u.LastName,
                                                            }).ToList(), "UserId", "FullName");
                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SingleAlbum(int AlbumID)
        {
            List<DSRCManagementSystem.Models.Gallery> objphoto = new List<DSRCManagementSystem.Models.Gallery>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userid = Convert.ToInt32(Session["UserID"]);
                ViewBag.UserID = userid;
                ViewBag.CreateAlbum = (from p in db.AlbumPermissions
                                       where p.UserId == userid && p.IsAuthorized == true
                                       select p.UserId).SingleOrDefault();

                ViewBag.AlbumID = AlbumID;
                ViewBag.Albumtitle = (from p in db.Albums
                                      where p.AlbumID == AlbumID
                                      select p.AlbumTitle).SingleOrDefault();



                objphoto = (from p in db.AlbumPhotos
                            where p.IsActive == true && p.AlbumID == AlbumID
                            select new DSRCManagementSystem.Models.Gallery
                            {
                                AlbumPhotoID = p.AlbumPhotoID,
                                AlbumTitle = p.AlbumPhotoTitle,
                                AlbumDescription = p.AlbumPhotoDescription,
                                CoverPhotoPath = p.AlbumPhotoPath
                            }).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objphoto);
        }


        [HttpGet]
        public ActionResult EditSingleAlbum(int AlbumID, int AlbumPhotoID)
        {
            DSRCManagementSystem.Models.Gallery objphoto = new DSRCManagementSystem.Models.Gallery();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var value = db.AlbumPhotos.Where(x => x.AlbumID == AlbumID && x.AlbumPhotoID == AlbumPhotoID && x.IsActive == true).Select(o => o).FirstOrDefault();
                objphoto.AlbumID = AlbumID;
                objphoto.AlbumPhotoID = AlbumPhotoID;
                objphoto.AlbumTitle = value.AlbumPhotoTitle;
                objphoto.AlbumDescription = value.AlbumPhotoDescription;
                objphoto.CoverPhotoPath = value.AlbumPhotoPath;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objphoto);
        }

        [HttpPost]
        public ActionResult EditSingleAlbum(Gallery Collection)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var singleAlbum = db.AlbumPhotos.Where(x => x.AlbumID == Collection.AlbumID && x.AlbumPhotoID == Collection.AlbumPhotoID).Select(o => o).FirstOrDefault();
                singleAlbum.AlbumPhotoTitle = Collection.AlbumTitle;
                singleAlbum.AlbumPhotoDescription = Collection.AlbumDescription;
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteSingleAlbum(int AlbumID, int AlbumPhotoID)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Gallery objphoto = new DSRCManagementSystem.Models.Gallery();
                var value = db.AlbumPhotos.Where(x => x.AlbumID == AlbumID && x.AlbumPhotoID == AlbumPhotoID).Select(o => o).FirstOrDefault();
                value.IsActive = false;
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult EditAlbum(int Id)
        {
            DSRCManagementSystem.Models.Gallery objphoto = new DSRCManagementSystem.Models.Gallery();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var value = db.Albums.Where(x => x.AlbumID == Id).Select(o => o).FirstOrDefault();

                objphoto.AlbumID = Id;
                objphoto.AlbumTitle = value.AlbumTitle;
                objphoto.AlbumDescription = value.AlbumDescription;
                objphoto.CoverPhotoPath = value.AlbumCoverPhotoPath;
                objphoto.EventDate = (DateTime)value.EventDate;

                var AlbumAccessRoles = (from p in db.Master_Roles.Where(x => x.IsActive == true)
                                        select new
                                        {
                                            RoleID = p.RoleID,
                                            RoleDescription = p.RoleName

                                        }).ToList();

                List<DSRCManagementSystem.Models.AccessRoles> objrole = new List<DSRCManagementSystem.Models.AccessRoles>();
                objrole = (from a in db.AlbumAccesses
                           where a.AlbumID == Id && a.RoleID != null

                           select new AccessRoles
                            {
                                RoleID = a.RoleID
                            }).ToList();


                List<int> selectedRoles = new List<int>();
                for (int i = 0; i < objrole.Count(); i++)
                {
                    selectedRoles.Add(Convert.ToInt32(objrole[i].RoleID));
                }

                ViewBag.AlbumAccessRoles = new MultiSelectList(AlbumAccessRoles, "RoleID", "RoleDescription", selectedRoles);



                var AlbumAccessUsers = (from p in db.Users.Where(x => x.IsActive == true)
                                        select new
                                        {
                                            UserID = p.UserID,
                                            UserDescription = p.FirstName + " " + p.LastName

                                        }).ToList();
                List<DSRCManagementSystem.Models.AccessUsers> objuser = new List<DSRCManagementSystem.Models.AccessUsers>();
                objuser = (from a in db.AlbumAccesses
                           where a.AlbumID == Id && a.UserId != null
                           select new AccessUsers
                                   {
                                       UserID = a.UserId
                                   }).ToList();


                List<int> selectedUsers = new List<int>();
                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedUsers.Add(Convert.ToInt32(objuser[i].UserID));
                }


                ViewBag.AlbumAccessUsers = new MultiSelectList(AlbumAccessUsers, "UserID", "UserDescription", selectedUsers);


                objphoto.IsAccess = "all";
                if (objrole.Count > 0)
                {
                    objphoto.IsAccess = "role";
                }
                else if (objuser.Count > 0)
                {
                    objphoto.IsAccess = "user";
                }
                ViewBag.AlbumExist = (from p in db.Albums
                                      where p.IsActive == true && p.AlbumID != Id
                                      select p.AlbumTitle).ToArray();

                var getTagAlbumId = db.TaggedAlbums.Where(x => x.AlbumId == Id).Select(x=>x.UserId).ToList();
                List<int?> tagAlbumId = new List<int?>();
                foreach(var item in getTagAlbumId)
                {
                    tagAlbumId.Add(item);
                }
                var tagusers=(from u in db.Users.Where(x => x.IsActive == true)
                                                        select new
                                                        {
                                                            UserId = u.UserID,
                                                            FullName = u.FirstName + " " + u.LastName,
                                                        }).ToList();

                ViewBag.TagUsers = new MultiSelectList(tagusers, "UserId", "FullName", tagAlbumId);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objphoto);
        }
        [HttpPost]
        public ActionResult EditAlbum(Gallery collection)
        {
            try
            {
                var httpPostedFile = HttpContext.Request.Files["UploadedImage"];
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var value = db.Albums.Where(x => x.AlbumID == collection.AlbumID).Select(o => o).FirstOrDefault();
                value.AlbumTitle = collection.AlbumTitle;
                value.AlbumDescription = collection.AlbumDescription;
                value.EventDate = collection.EventDate;
                string AlbumPath = ConfigurationManager.AppSettings["AlbumPath"].ToString() + collection.AlbumTitle;
                if (httpPostedFile != null)
                {

                    var AlbumTitle = collection.AlbumTitle;
                    var CoverPhotoName = Path.GetFileName(httpPostedFile.FileName);
                    var FolderPath = Server.MapPath(AlbumPath);

                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }
                    string FilePath = Server.MapPath(AlbumPath + "/" + CoverPhotoName);
                    httpPostedFile.SaveAs(FilePath);


                    value.AlbumCoverPhotoPath = ConfigurationManager.AppSettings["AlbumPath"].ToString() + AlbumTitle + "/" + CoverPhotoName;
                }
                db.SaveChanges();

                if (collection.AlbumRole != null)
                {
                    List<int?> objrole = new List<int?>();

                    string[] role = collection.AlbumRole.Split(',');

                    for (int i = 0; i < role.Count(); i++)
                    {
                        objrole.Add(Convert.ToInt32(role[i]));
                    }
                    var deleterole = db.AlbumAccesses.Where(x => x.AlbumID == collection.AlbumID).Select(o => o).ToList();
                    foreach (var delrole in deleterole)
                        db.AlbumAccesses.DeleteObject(delrole);
                    db.SaveChanges();
                    for (int j = 0; j < objrole.Count(); j++)
                    {
                        DSRCManagementSystem.AlbumAccess objaccess = new DSRCManagementSystem.AlbumAccess();
                        objaccess.AlbumID = collection.AlbumID;
                        objaccess.RoleID = objrole[j];
                        db.AddToAlbumAccesses(objaccess);
                        db.SaveChanges();
                    }
                }
                else if (collection.AlbumUser != null)
                {
                    List<int?> objuser = new List<int?>();

                    string[] user = collection.AlbumUser.Split(',');

                    for (int i = 0; i < user.Count(); i++)
                    {
                        objuser.Add(Convert.ToInt32(user[i]));
                    }
                    var deleteuser = db.AlbumAccesses.Where(x => x.AlbumID == collection.AlbumID).Select(o => o).ToList();
                    foreach (var deluser in deleteuser)
                        db.AlbumAccesses.DeleteObject(deluser);
                    db.SaveChanges();
                    for (int j = 0; j < objuser.Count(); j++)
                    {
                        DSRCManagementSystem.AlbumAccess objaccess = new DSRCManagementSystem.AlbumAccess();
                        objaccess.AlbumID = collection.AlbumID;
                        objaccess.UserId = objuser[j];
                        db.AddToAlbumAccesses(objaccess);
                        db.SaveChanges();
                    }
                }
                else
                {
                    var deleteuser = db.AlbumAccesses.Where(x => x.AlbumID == collection.AlbumID).Select(o => o).ToList();
                    foreach (var deluser in deleteuser)
                        db.AlbumAccesses.DeleteObject(deluser);
                    db.SaveChanges();
                    DSRCManagementSystem.AlbumAccess objaccess = new DSRCManagementSystem.AlbumAccess();
                    objaccess.AlbumID = collection.AlbumID;
                    db.AddToAlbumAccesses(objaccess);
                    db.SaveChanges();
                }

                var deleteTag = db.TaggedAlbums.Where(x => x.AlbumId == collection.AlbumID).Select(o => o).ToList();
                foreach (var deluser in deleteTag)
                {
                    db.TaggedAlbums.DeleteObject(deluser);
                    db.SaveChanges();
                }
                string[] tagvalue = collection.TagUsers.Split(',');
                for (int i = 0; i < tagvalue.Count(); i++)
                {
                    var objTagAlbum = db.TaggedAlbums.CreateObject();
                    objTagAlbum.AlbumId = collection.AlbumID;
                    objTagAlbum.UserId = Convert.ToInt32(tagvalue[i]);
                    db.TaggedAlbums.AddObject(objTagAlbum);
                    db.SaveChanges();
                }
                ViewBag.TagUsers = new MultiSelectList((from u in db.Users.Where(x => x.IsActive == true)
                                                        select new
                                                        {
                                                            UserId = u.UserID,
                                                            FullName = u.FirstName + " " + u.LastName,
                                                        }).ToList(), "UserID", "FullName");
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("failed", JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult DeleteAlbum(int AlbumID)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Gallery objphoto = new DSRCManagementSystem.Models.Gallery();
                var value = db.Albums.Where(x => x.AlbumID == AlbumID && x.IsActive == true).Select(o => o).FirstOrDefault();
                var data = db.DefaultAlbumUsers.Where(x => x.AlbumID == AlbumID && x.IsActive == true).Select(o => o).FirstOrDefault();
                value.IsActive = false;
                data.IsActive = false;
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddPhotos(int AlbumID)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var obj = db.AlbumPhotos.CreateObject();
                string AlbumTitle = (from p in db.Albums
                                     where p.AlbumID == AlbumID
                                     select p.AlbumTitle).SingleOrDefault();


                string AlbumPath = ConfigurationManager.AppSettings["AlbumPath"].ToString();
                string FileName = "";

                {
                    var FolderPath = Server.MapPath(AlbumPath + AlbumTitle);
                    if (!Directory.Exists(FolderPath))
                    {
                        Directory.CreateDirectory(FolderPath);
                    }
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[fileName];

                        FileName = file.FileName;
                        if (file != null && file.ContentLength > 0)
                        {

                            string FilePath = Server.MapPath(AlbumPath + AlbumTitle + "/" + FileName);
                            Image image = Image.FromStream(file.InputStream, true, true);
                            if (image.Width > 1024 || image.Height > 768)
                            {
                                int newwidthimg = 1024;
                                float AspectRatio = (float)image.Size.Width / (float)image.Size.Height;
                                int newHeight = 768;
                                Image thumbNail = new Bitmap(newwidthimg, newHeight, image.PixelFormat);
                                Graphics g = Graphics.FromImage(thumbNail);
                                g.CompositingQuality = CompositingQuality.HighQuality;
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                Rectangle rect = new Rectangle(0, 0, newwidthimg, newHeight);
                                g.DrawImage(image, rect);
                                thumbNail.Save(Server.MapPath(AlbumPath + AlbumTitle + "/" + file.FileName + ".jpg"), ImageFormat.Jpeg);
                            }
                            else
                            {
                                image.Save(Server.MapPath(AlbumPath + AlbumTitle + "/" + file.FileName + ".jpg"), ImageFormat.Jpeg);
                            }

                            string CreatedBy = Convert.ToString(Session["FirstName"]) + " " + Convert.ToString(Session["LastName"]);
                            DateTime CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            obj.AlbumID = AlbumID;
                            obj.AlbumPhotoPath = ConfigurationManager.AppSettings["AlbumPath"].ToString() + AlbumTitle + "/" + FileName + ".jpg";
                            obj.CreatedDate = CreatedOn;
                            obj.CreatedBy = CreatedBy;
                            obj.IsActive = true;
                            db.AlbumPhotos.AddObject(obj);
                            db.SaveChanges();

                        }

                    }

                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult AlbumPermission()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var AuthUsers = (from ep in db.AlbumPermissions.Where(ep => ep.IsAuthorized == true)
                                 join u in db.Users.Where(u => u.IsActive == true) on ep.UserId equals u.UserID
                                 into evper
                                 from eventper in evper.DefaultIfEmpty()
                                 select new DSRCEmployees
                                 {
                                     UId = eventper.UserID,
                                     Name = (eventper.FirstName + " " + eventper.LastName) ?? ""
                                 }).Where(x => x.Name != null && x.Name != "").ToList();
               // AuthUsers.RemoveAll(x => x.Name == "");
                ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "UId", "Name");

                var FilteredUsers = db.Users.Where(u => u.IsActive == true).Select(x => x.UserID).ToList().
                    Except(db.AlbumPermissions.Where(ep => ep.IsAuthorized == true || ep.IsAuthorized.Value).Select(x => x.UserId.Value).ToList()).ToList();
                List<object> UnAuthUsers = new List<object>();
                foreach (int users in FilteredUsers)
                {
                    UnAuthUsers.AddRange(db.Users.Where(u => u.UserID == users).Select(u => new { userid = u.UserID, username = (u.FirstName + " " + u.LastName) ?? "" }).Where(u => u.username != null && u.username != "").ToList());
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
        public ActionResult AlbumPermission(List<int> From, List<int> To)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var deleteuser = db.AlbumPermissions.Where(x => x.IsAuthorized == true).Select(o => o).ToList();
                foreach (var deluser in deleteuser)
                    db.AlbumPermissions.DeleteObject(deluser);
                db.SaveChanges();
                for (int j = 0; j < To.Count(); j++)
                {
                    DSRCManagementSystem.AlbumPermission objaccess = new DSRCManagementSystem.AlbumPermission();
                    objaccess.UserId = To[j];
                    objaccess.IsAuthorized = true;
                    db.AddToAlbumPermissions(objaccess);
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



        [HttpGet]
        public ActionResult AlbumSlider(int AlbumID)
        {
            List<Gallery> files = new List<Gallery>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                List<DSRCManagementSystem.Models.Gallery> objphoto = new List<DSRCManagementSystem.Models.Gallery>();
                objphoto = (from p in db.AlbumPhotos
                            where p.IsActive == true && p.AlbumID == AlbumID
                            select new DSRCManagementSystem.Models.Gallery
                            {
                                AlbumPhotoID = p.AlbumPhotoID,
                                AlbumTitle = p.AlbumPhotoTitle,
                                AlbumDescription = p.AlbumPhotoDescription,
                                CoverPhotoPath = p.AlbumPhotoPath
                            }).ToList();


                foreach (var filePath in objphoto)
                {
                    string fileName = Path.GetFileName(filePath.CoverPhotoPath);
                    files.Add(new DSRCManagementSystem.Models.Gallery
                    {
                        title = fileName.Split('.')[0].ToString(),
                        src = filePath.CoverPhotoPath,
                        count = objphoto.Count()

                    });
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(files);
        }

        [HttpGet]
        public ActionResult TaggedAlbums(string year)
        {
            List<DSRCManagementSystem.Models.Gallery> objphoto = new List<DSRCManagementSystem.Models.Gallery>();
            int sessionuserid = Convert.ToInt32(Session["UserID"]);
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int AlbumYear = Convert.ToInt32(year);
                int Year = (AlbumYear == null) ? 0 : AlbumYear;
                int LeaveUserId = (int)Session["UserId"];
                var UserRegionId = db.Users.Where(x => x.UserID == LeaveUserId).Select(o => o.Region).FirstOrDefault();


                var userid = Convert.ToInt32(Session["UserID"]);
                var roleid = Convert.ToInt32(Session["RoleId"]);

                ViewBag.CreateAlbum = (from p in db.AlbumPermissions
                                       where p.UserId == userid && p.IsAuthorized == true
                                       select p.UserId).SingleOrDefault();
                ViewBag.UserID = Convert.ToInt32(Session["UserID"]);

                List<Albums> AccessUsers = new List<Albums>();

                AccessUsers = (from a in db.AlbumAccesses
                               where a.UserId == userid || a.RoleID == roleid || a.RoleID == null && a.UserId == null
                               select new Albums
                               {
                                   albumid = a.AlbumID
                               }).ToList();

                List<Albums> DefaultUser = new List<Albums>();
                List<int?> obj = new List<int?>();
                DefaultUser = (from a in db.DefaultAlbumUsers
                               where a.UserID == userid && a.IsActive == true
                               select new Albums
                               {
                                   albumid = a.AlbumID
                               }).ToList();
                for (int i = 0; i < AccessUsers.Count(); i++)
                {
                    obj.Add(Convert.ToInt32(AccessUsers[i].albumid));
                }
                for (int i = 0; i < DefaultUser.Count(); i++)
                {
                    obj.Add(Convert.ToInt32(DefaultUser[i].albumid));
                }
                if (Year != null && Year != 0 && Year != 1)
                {

                    var Years1 = db.Albums.Where(x => x.IsActive == true && x.EventDate != null).OrderByDescending(x => x.EventDate.Value.Year).ThenBy(x => x.EventDate.Value.Month).ThenBy(x => x.EventDate.Value.Day).Select(o => o.EventDate.Value.Year).Distinct().ToList();
                    ViewBag.Years = new SelectList(Years1, Year);

                    objphoto = (from p in db.Albums.Where(x => x.IsActive == true && obj.Contains(x.AlbumID) && x.EventDate.Value.Year == Year)
                                join t in db.AlbumPhotos.Where(x => x.IsActive == true) on p.AlbumID equals t.AlbumID into j
                                join tg in db.TaggedAlbums.Where(x=>x.UserId==sessionuserid&&obj.Contains(x.AlbumId)) on p.AlbumID equals tg.AlbumId
                                select new DSRCManagementSystem.Models.Gallery
                                {
                                    Photocount = j.Count(a => a.AlbumID == p.AlbumID),
                                    AlbumID = p.AlbumID,
                                    AlbumTitle = p.AlbumTitle,
                                    AlbumDescription = p.AlbumDescription,
                                    CoverPhotoPath = p.AlbumCoverPhotoPath,
                                    CreatedBy = p.CreatedBy,
                                    EventDate = (DateTime)p.EventDate
                                }).ToList();
                    return View(objphoto);
                }
                else
                {
                    int currentYear = DateTime.Today.Year;
                    var Years = db.Albums.Where(x => x.IsActive == true && x.EventDate != null).OrderByDescending(x => x.EventDate.Value.Year).ThenBy(x => x.EventDate.Value.Month).ThenBy(x => x.EventDate.Value.Day).Select(o => o.EventDate.Value.Year).Distinct().ToList();
                    ViewBag.Years1 = new SelectList(Years, currentYear);

                    objphoto = (from p in db.Albums.Where(x => x.IsActive == true && obj.Contains(x.AlbumID) && x.EventDate.Value.Year == currentYear)
                                join t in db.AlbumPhotos.Where(x => x.IsActive == true) on p.AlbumID equals t.AlbumID into j
                                join tg in db.TaggedAlbums.Where(x => x.UserId == sessionuserid && obj.Contains(x.AlbumId)) on p.AlbumID equals tg.AlbumId
                                select new DSRCManagementSystem.Models.Gallery
                                {
                                    Photocount = j.Count(a => a.AlbumID == p.AlbumID),
                                    AlbumID = p.AlbumID,
                                    AlbumTitle = p.AlbumTitle,
                                    AlbumDescription = p.AlbumDescription,
                                    CoverPhotoPath = p.AlbumCoverPhotoPath,
                                    CreatedBy = p.CreatedBy,
                                    EventDate = (DateTime)p.EventDate
                                }).ToList();
                    return View(objphoto);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objphoto);
        }
    }
}
