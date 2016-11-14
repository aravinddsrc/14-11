using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using DsrcMailSystem;
using System.Text.RegularExpressions;
using DSRCManagementSystem;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Threading;
using System.Windows;

namespace DSRCManagementSystem.Controllers
{

    // [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Recruitment Specialist-RMG, Tech Lead,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer,Head - Quality,")]
    public class CommunicationController : Controller
    {
        //
        // GET: /Communication/

        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 

        #region "Messages"

        //[DSRCAuthorize(Roles = "Vice President, Project Manager, Tech Lead, Head - Quality,Business Development Manager,Recruitment Specialist-RMG,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer,Assistant Manager-Recruitment")]

        public ActionResult Messages(int? depid)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            int userID = Convert.ToInt32(Session["UserID"]);

            communicationModel commModel = new communicationModel();
            try
            {

                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

                commModel.dep = depid;


                commModel.dateFrom = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                commModel.dateTo = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                commModel.Message = string.Empty;

                commModel.users = communicationHelper.GetUsers();

                commModel.users.RemoveAll(x => x.userId == userID);

                commModel.messageType = communicationHelper.GetMessageTypes();

                commModel.departments = communicationHelper.GetDepartments();




                //    var DepartmentList = (from us in objdb.Departments

                //                          select new
                //                          {
                //                              DepartmentId = us.DepartmentId,
                //                              DepartmentName = us.DepartmentName
                //                          }).ToList();
                //    ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", depid);







                //    if (depid == null)
                //    {
                //        commModel.Groups = (from d in objdb.Departments
                //                            join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                //                            join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                //                            where d.IsActive == true && dg.IsActive == true
                //                            select new MailInvitesModel
                //                            {
                //                                gName = dg.GroupName,
                //                                gID = dg.GroupID,
                //                            }).ToList();
                //    }


                //    if (depid != null)
                //    {
                //        //MailInvitesModel commModel = new MailInvitesModel();

                //        commModel.selGroup = (from d in objdb.Departments
                //                              join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                //                              join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                //                              where d.IsActive == true && dg.IsActive == true && d.DepartmentId == depid
                //                              select new MailInvitesModel
                //                              {
                //                                  GroupName = dg.GroupName,
                //                                  GroupId = dg.GroupID,
                //                              }).ToList();


                //    }



                //    var UserList = (from lt in objdb.Users
                //                    join r in objdb.UserReportings.Where(o => o.ReportingUserID == userID) on lt.UserID equals r.UserID
                //                    where lt.IsActive == true && lt.UserStatus != 6
                //                    select new
                //                    {
                //                        Id3 = lt.UserID,
                //                        FirstName = lt.FirstName + " " + lt.LastName,

                //                        // }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.Id3), Text = m.FirstName });
                //                    }).ToList();


                //    ViewBag.Leaders = new MultiSelectList(UserList, "Id3", "FirstName");


                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = objdb.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                var Department = objdb.Departments.Where(o => o.IsActive == true && o.BranchID == GetBranch).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(c => c.DepartmentName).ToList();

                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");


                var Group = objdb.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
                {
                    GroupId = c.GroupID,
                    GroupName = c.GroupName
                }).ToList();
                ViewBag.Group = new SelectList("", "GroupId", "GroupName");


                var UserList = "";


                ViewBag.Users = new MultiSelectList(UserList, "Id3", "FirstName");

                //if (depid == null)
                //{
                //    commModel.Groups = (from d in objdb.Departments
                //                        join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                //                        join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                //                        where d.IsActive == true && dg.IsActive == true
                //                        select new MailInvitesModel
                //                        {
                //                            gName = dg.GroupName,
                //                            gID = dg.GroupID,
                //                        }).ToList();
                //}


                //if (depid != null)
                //{
                //    //MailInvitesModel commModel = new MailInvitesModel();

                //    commModel.selGroup = (from d in objdb.Departments
                //                          join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                //                          join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                //                          where d.IsActive == true && dg.IsActive == true && d.DepartmentId == depid
                //                          select new MailInvitesModel
                //                          {
                //                              GroupName = dg.GroupName,
                //                              GroupId = dg.GroupID,
                //                          }).ToList();




                //}



            }











            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View(commModel);
        }

        [HttpPost]
        // [DSRCAuthorize(Roles = "Vice President,Assistant Manager-Recruitment, Project Manager,Tech Lead,Head - Quality,Recruitment Specialist-RMG,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
        public ActionResult Messages(communicationModel commModel, string dateTo, string dateFrom)
        {

            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();



            try
            {

                int userID = Convert.ToInt32(Session["UserID"]);
                //String Date = DateTime.Now.ToShortTimeString();
                String Date = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                if (commModel.dateFrom == Date && commModel.dateTo == Date)
                {
                    DateTime validFrom = DateTime.ParseExact(commModel.dateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    DateTime validTo = DateTime.ParseExact(commModel.dateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    communicationHelper.SendMessage(commModel, validFrom, validTo, userID);
                }
                else
                {
                    DateTime validFrom = Convert.ToDateTime(dateFrom);
                    DateTime validTo = Convert.ToDateTime(dateTo);
                    communicationHelper.SendMessage(commModel, validFrom, validTo, userID);
                }
                try
                {
                    //communicationHelper.SendMessage(commModel, validFrom, validTo, userID);
                    commModel = new communicationModel();
                    ModelState.Clear();
                    if (commModel.dateFrom == Date && commModel.dateTo == Date)
                    {
                        commModel.dateFrom = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                        commModel.dateTo = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        DateTime validFrom = Convert.ToDateTime(dateFrom);
                        DateTime validTo = Convert.ToDateTime(dateTo);
                        //communicationHelper.SendMessage(commModel, validFrom, validTo, userID);
                    }
                    //commModel.dateFrom = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    //commModel.dateTo = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    commModel.ErrorSuccessMessage = "Message sent successfully.";

                    var Department = (from us in objdb.Departments

                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).ToList();
                    ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

                    //commModel.Groups = (from d in objdb.Departments
                    //                    join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                    //                    join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                    //                    where d.IsActive == true && dg.IsActive == true
                    //                    select new MailInvitesModel
                    //                    {
                    //                        gName = dg.GroupName,
                    //                        gID = dg.GroupID,
                    //                    }).OrderBy(dg=>dg.GroupName).ToList();

                    var UserList = (from lt in objdb.Users.Where(o=>o.UserName!=null)
                                    join r in objdb.UserReportings.Where(o => o.ReportingUserID == userID) on lt.UserID equals r.UserID
                                    where lt.IsActive == true && lt.UserStatus != 6
                                    select new
                                    {
                                        Id3 = lt.UserID,
                                        FirstName = lt.FirstName + " " + lt.LastName,

                                        // }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.Id3), Text = m.FirstName });
                                    }).ToList();


                    ViewBag.Users = new MultiSelectList(UserList, "Id3", "FirstName");


                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    commModel.ErrorSuccessMessage = "Error in sending message";
                }
                commModel.users = communicationHelper.GetUsers();
                //commModel.departments = communicationHelper.GetDepartments();

                commModel.messageType = communicationHelper.GetMessageTypes();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(commModel);
        }
        public ActionResult Delete(int DepartmentId, int msgid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Delete = db.UserMessages.Where(o => o.MessageId == msgid).Select(o => o).FirstOrDefault();
            db.DeleteObject(Delete);
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);

        }







        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUser(int DepartmentId)
        {

            //int DepartmentId = Convert.ToInt32(DepartmentIds);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();
            int userID = Convert.ToInt32(Session["UserID"]);


            if (DepartmentId != 0)
            {
                var validGroup = db.Users.Where(d => d.DepartmentId == DepartmentId).Select(d => d.UserID).ToList();

                FilterGroup = (from lt in db.Users.Where(o => validGroup.Contains(o.UserID))

                               join r in db.UserReportings.Where(o => o.ReportingUserID == userID) on lt.UserID equals r.UserID

                               where lt.IsActive == true && lt.UserStatus != 6
                               select new
                               {
                                   UserId = lt.UserID,
                                   UserName = lt.FirstName + " " + lt.LastName,

                               }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.UserName });




            }

            var UserIDs = (from lt in db.Users
                           join r in db.UserReportings on lt.UserID equals r.UserID
                           where lt.IsActive == true && lt.UserStatus != 6 && r.ReportingUserID == userID && lt.DepartmentId == DepartmentId
                           select lt.UserID
                         ).ToList();



            return Json(UserIDs, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Mail Invites"


        //[DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead,Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
        [HttpGet]
        public ActionResult mailInvites(int? depid)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            MailInvitesModel commModel = new MailInvitesModel();
            try
            {
                List<string> AllMailIds = new List<string>();
                ModelState.Clear();
                ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
                //var DepartmentList = (from us in objdb.Departments

                //                      select new
                //                      {
                //                          DepartmentId = us.DepartmentId,
                //                          DepartmentName = us.DepartmentName
                //                      }).ToList();

                //ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", depid);

              
                Session["Back1"] = "mailInvites";

                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = objdb.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();
                var Department = objdb.Departments.Where(o => o.IsActive == true && o.BranchID == GetBranch).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).OrderBy(x=>x.DepartmentName).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");



                var Group = objdb.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
                {
                    GroupId = c.GroupID,
                    GroupName = c.GroupName
                }).ToList();
                ViewBag.Group = new SelectList("", "GroupId", "GroupName");


                //var UserList = "";


                //ViewBag.Users = new MultiSelectList(UserList, "Id3", "FirstName");


                int userIDs = Convert.ToInt32(Session["UserID"]);


                var UserList = (from lt in objdb.Users.Where(o=>o.UserName!=null)
                                join r in objdb.UserReportings.Where(o => o.ReportingUserID == userIDs) on lt.UserID equals r.UserID
                                where lt.IsActive == true && lt.UserStatus != 6
                                select new
                                {
                                    Id3 = lt.UserID,
                                    FirstName = lt.FirstName + " " + lt.LastName,

                                }).ToList();


                ViewBag.Users = new MultiSelectList(UserList, "Id3", "FirstName");






                var GroupList = objdb.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                {
                    groupid = g.GroupID,
                    groupname = g.GroupName
                }).ToList();

                commModel.dep = depid;
                //commModel.depName = objdb.Departments.Where(x => x.DepartmentId == depid).Select(x => x.DepartmentName));

                if (depid == null)
                {
                    commModel.Groups = (from d in objdb.Departments
                                        join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                        join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                        where d.IsActive == true && dg.IsActive == true
                                        select new MailInvitesModel
                                        {
                                            gName = dg.GroupName,
                                            gID = dg.GroupID,
                                        }).ToList();
                }


                if (depid != null)
                {
                    //MailInvitesModel commModel = new MailInvitesModel();

                    commModel.selGroup = (from d in objdb.Departments
                                          join dgm in objdb.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                          join dg in objdb.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                          where d.IsActive == true && dg.IsActive == true && d.DepartmentId == depid
                                          select new MailInvitesModel
                                          {
                                              GroupName = dg.GroupName,
                                              GroupId = dg.GroupID,
                                          }).ToList();


                    //var validGroup = objdb.DepartmentGroupMappings.Where(d => d.DepartmentID == depid).Select(d => d.GroupID).ToList();

                    //commModel.selGroup = (from lt in objdb.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                    //                      where lt.IsActive == true
                    //                      select new MailInvitesModel()
                    //                      {
                    //                          GroupId = lt.GroupID,
                    //                          GroupName = lt.GroupName
                    //                      }).ToList();

                }
                //var DepartmentList = (from us in objdb.Departments

                //                      select new
                //                      {
                //                          DepartmentId = us.DepartmentId,
                //                          DepartmentName = us.DepartmentName
                //                      }).ToList();

                //ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", depid);





                int userID = Convert.ToInt32(Session["UserID"]);


                //var UserList = (from lt in objdb.Users
                //                join r in objdb.UserReportings.Where(o => o.ReportingUserID == userID) on lt.UserID equals r.UserID
                //                where lt.IsActive == true && lt.UserStatus != 6
                //                select new
                //                {
                //                    Id3 = lt.UserID,
                //                    FirstName = lt.FirstName + " " + lt.LastName,

                //                }).ToList();


                // ViewBag.Leaders = new MultiSelectList(UserList, "Id3", "FirstName");

                //}





                commModel.Message = string.Empty;

                commModel.users = communicationHelper.GetUsers();

                commModel.departments = communicationHelper.GetDepartments();
                return View(commModel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(commModel);
        }
        //[AcceptVerbs(HttpVerbs.Get)]
        [HttpGet]
        public ActionResult GetGroups(int Depid)
        {
            MailInvitesModel commModel = new MailInvitesModel();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();
                int userId = Convert.ToInt32(Session["UserID"]);

                ViewBag.Dept = Depid;

                if (Depid != 0)
                {





                    var validGroup = objdb.DepartmentGroupMappings.Where(d => d.DepartmentID == Depid).Select(d => d.GroupID).ToList();

                    commModel.selGroup = (from lt in objdb.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                                          where lt.IsActive == true
                                         
                                          select new MailInvitesModel()
                                          {
                                              GroupId = lt.GroupID,
                                              GroupName = lt.GroupName
                                          }).ToList();
                    var validGroup1 = objdb.Users.Where(d => d.DepartmentId == Depid).Select(d => d.UserID).ToList();

//                    var sorted = commModel.selGroup.Sort();

                    //commModel.Users2 = (from lt in objdb.Users.Where(o => validGroup1.Contains(o.UserID))
                    //                    join r in objdb.UserReportings on lt.UserID equals r.UserID
                    //                    where lt.IsActive == true && lt.UserStatus != 6 && r.ReportingUserID == userId && lt.DepartmentId == Depid 
                    //                    select new MailInvitesModel()
                    //                    {
                    //                        Userid = lt.UserID,
                    //                        UserName = lt.FirstName + " " + lt.LastName,
                    //                    }).ToList();

                    commModel.Users2 = (from lt in objdb.Users.Where(o => validGroup1.Contains(o.UserID))
                                        join r in objdb.UserReportings on lt.UserID equals r.UserID
                                        where lt.IsActive == true && lt.UserStatus != 6
                                        select new MailInvitesModel()
                                        {
                                            Userid = lt.UserID,
                                            UserName = lt.FirstName + " " + lt.LastName,
                                        }).ToList();




                    var DepartmentList = (from us in objdb.Departments

                                          select new
                                          {
                                              DepartmentId = us.DepartmentId,
                                              DepartmentName = us.DepartmentName
                                          }).ToList();

                    ViewBag.DepartmentList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", Depid);

                }
                //return Json(commModel.selGroup, JsonRequestBehavior.AllowGet);
                return Json(new { FirstList = commModel.selGroup, SecondList = commModel.Users2 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { FirstList = commModel.selGroup, SecondList = commModel.Users2 }, JsonRequestBehavior.AllowGet);
        }






        //[AcceptVerbs(HttpVerbs.Get)]
        //public ActionResult GetUsers(int DepartmentId, int? groupid)
        //{
        //    MailInvitesModel commModel = new MailInvitesModel();
        //    try
        //    {
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    int userId = Convert.ToInt32(Session["UserID"]);
        //    if (DepartmentId != 0 && groupid == 0)
        //    {
        //        var validGroup = db.Users.Where(d => d.DepartmentId == DepartmentId).Select(d => d.UserID).ToList();

        //        commModel.Users1 = (from lt in db.Users.Where(o => validGroup.Contains(o.UserID))
        //                            join r in db.UserReportings on lt.UserID equals r.UserID
        //                            where lt.IsActive == true && lt.UserStatus != 6 && r.ReportingUserID != userId && lt.DepartmentId == DepartmentId
        //                            select new MailInvitesModel()
        //                            {
        //                                Userid = lt.UserID,
        //                                UserName = lt.FirstName + " " + lt.LastName,
        //                            }).ToList();
        //    }

        //    else if (DepartmentId != 0 && groupid != 0)
        //    {

        //        var validGroup = db.Users.Where(d => d.DepartmentId == DepartmentId || d.DepartmentGroup == groupid).Select(d => d.UserID).ToList();
        //        //var validGroup = db.Users.Where(d => d.DepartmentGroup == groupid).Select(d => d.UserID).ToList();

        //        //commModel.Users1 = (from lt in db.Users.Where(o => validGroup.Contains(o.UserID))
        //        //                    join r in db.UserReportings on lt.UserID equals r.UserID
        //        //                    where lt.IsActive == true && lt.UserStatus != 6 && r.ReportingUserID != userId && lt.DepartmentId == DepartmentId && lt.DepartmentGroup == groupid
        //        //                    select new MailInvitesModel()
        //        //                    {
        //        //                        Userid = lt.UserID,
        //        //                        UserName = lt.FirstName + " " + lt.LastName,
        //        //                    }).ToList();
        //        commModel.Users1 = (from lt in db.Users.Where(o => validGroup.Contains(o.UserID))
        //                            join r in db.UserReportings on lt.UserID equals r.UserID
        //                            where lt.IsActive == true && lt.UserStatus != 6
        //                            select new MailInvitesModel()
        //                            {
        //                                Userid = lt.UserID,
        //                                UserName = lt.FirstName + " " + lt.LastName,
        //                            }).ToList();
        //    }
        //    return Json(commModel.Users1, JsonRequestBehavior.AllowGet);
        //}
        //    catch (Exception Ex)
        //    {
        //        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
        //        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
        //    }
        //    return Json(commModel.Users1, JsonRequestBehavior.AllowGet);
        //}



        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailEmployees(int DepartmentName)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

            if (DepartmentName != 0)
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();




                Employees = (from d in db.Departments
                             join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                             join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                             where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DepartmentName && d.BranchID == GetBranch 
                             orderby dg.GroupName
                             select new DSRCEmployees
                             {
                                 Name = dg.GroupName,
                                 UserId = dg.GroupID,

                             }).Union(from dg in db.DepartmentGroups
                                      where dg.GroupName == "UnGrouped"
                                      select new DSRCEmployees
                           {
                               Name = dg.GroupName,
                               UserId = dg.GroupID,
                           }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

            }
            return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUsers(string GroupName, int DepartmentName)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var GName = GroupName.Split(',');
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var GP = "";
            List<string> List = new List<string>();

          // if(GroupName !=null)
            foreach (var x in GName)
            {
                GP = rgx.Replace(x, "");
                List.Add(GP);
            }
            IEnumerable<SelectListItem> Employeess = new List<SelectListItem>();

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

            List<object> USERS = new List<object>();

            int userId = int.Parse(Session["UserID"].ToString());
            var GetBranch = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();



            foreach (var GRPID in List)
            {

                //if ( (GRPID != "")&& !(GRPID.Any()) )
                if ((GRPID != "") && (GRPID.Any()))
                {
                    if (GRPID != Convert.ToString(0) ) //|| GRPID != "" || GRPID != null )
                    {
                        int GID = Convert.ToInt32(GRPID);

                        var Users = (from u in db.Users
                                     where u.IsActive == true && u.UserStatus != 6 && u.BranchId == GetBranch && u.DepartmentId == DepartmentName && u.DepartmentGroup == GID// && u.FirstName != null && u.LastName != null
                                     select new DSRCEmployees
                                     {
                                        // Name = u.FirstName+ "" +u.LastName,
                                         Name = u.FirstName + " " + (u.LastName.Length>0 ? u.LastName : "" ),
                                         //u.LastName.HasValue ? "u.LastName" : (e.col2.HasValue ? "" : null)
                                         UserId = u.UserID,

                                     }).ToList();
                    
                        foreach (var x in Users)
                        {
                            USERS.Add(x);
                        }
                    }

                    if (GRPID == "195")
                    {
                        var Users = (from u in db.Users
                                     where u.IsActive == true && u.UserStatus != 6 && u.BranchId == GetBranch && u.DepartmentId == DepartmentName && u.DepartmentGroup == null// && u.FirstName != null && u.LastName != null
                                     select new DSRCEmployees
                                     {
                                         //Name =u.FirstName +(u.LastName)??"",
                                         Name = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                                         UserId = u.UserID,

                                     }).ToList();

                        foreach (var x in Users)
                        {
                            USERS.Add(x);
                        }



                    }
                    
                }

                // added on 9/7
                else
                {

                    int userIdd = int.Parse(Session["UserID"].ToString());
                    var GetBranchd = db.Users.Where(x => x.UserID == userId).Select(o => o.BranchId).FirstOrDefault();

                    var Users = (from u in db.Users
                                 where u.IsActive == true && u.UserStatus != 6 && u.BranchId == GetBranch && u.DepartmentId == DepartmentName //&& u.DepartmentGroup==null && u.FirstName != null && u.LastName != null
                                 select new DSRCEmployees
                                 {
                                     Name = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""),
                                     UserId = u.UserID,

                                 }).ToList();

                    foreach (var x in Users)
                    {
                        USERS.Add(x);
                    }
                    
                }

            }

       

           
          
           return Json(new SelectList(USERS, "UserId", "Name"), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Status(List<string> UserName)
        {
            List<StatusMail> Mails = new List<StatusMail>();
            var s = Session["status1"];
            var t = Session["status2"];
            if (s != null)
            {
                ViewBag.status1 = s;
            }
            if (t != null)
            {
                ViewBag.status2 = t;
            }
            List<string> List = new List<string>();

            if (UserName != null)
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var json_serializer = new JavaScriptSerializer();
                checks memberObj = json_serializer.Deserialize<checks>(UserName[0]);
                List<StatusMail> newMembers = new List<StatusMail>(memberObj.UserID);
                foreach (var item in newMembers)
                {
                    int user = Convert.ToInt32(item.UserID);
                    StatusMail name = new StatusMail();
                    var FirstName = (from u in db.Users
                                     where u.UserID == user
                                     select (u.FirstName != null ? u.FirstName : "") + " " + (u.LastName != null ? u.LastName : "")).FirstOrDefault();

                    name.Uname = FirstName;

                    Mails.Add(name);


                    List.Add(FirstName);

                }





                ViewBag.count = newMembers.Count();
                ViewData["count"] = newMembers.Count();

                //AllMailLists.Add(name);
            }
            //return View();

            ViewBag.Name = Mails;
            Session["COunt"] = Mails;
            ViewData["Names"] = List;
            return View();
        }



        [HttpPost]
        public ActionResult SendMailList(MailInvitesModel commModel)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            Thread.Sleep(6000);
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            MailInvitesModel model = new MailInvitesModel();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            Session["Department"] = commModel.Department;

            //Session["Group"] = commModel.Group;

            int userId = Convert.ToInt32(Session["UserID"]);
            var FirstName = (from u in objdb.Users
                             where u.UserID == userId
                             select (u.FirstName != null ? u.FirstName : "") + " " + (u.LastName != null ? u.LastName : "")).FirstOrDefault();

            commModel.UserList = communicationHelper.GetMailIdFromUsersList(commModel.UserList.Select(int.Parse).ToList());
            List<string> UserMail = commModel.UserList;


            //commModel.EmailIds = communicationHelper.GetMailIdFromUsersList(commModel.selectedMembers.Select(int.Parse).ToList());

            int j = commModel.UserList.Count();
            string EmailAddress = communicationHelper.GetMailId(userId);


            ViewEmail objmail = new ViewEmail();


            List<MailId> AllMailList = new List<MailId>();

            List<string> AllMailIds = new List<string>();

            var fileList = commModel.fileList.Where(x => x != null).ToList();
            Session["attachment"] = commModel.fileList.Where(x => x != null).ToList();

            if (fileList.Count() < 1)
            {
                commModel.attachment = false;
            }
            else
            {
                commModel.attachment = true;
            }

            string folderPath = Server.MapPath("~/EmailImages/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));


            string temp = "";

            for (int i = 0; i < j; i++)
            {

                temp += commModel.UserList[i].ToString() + ",";
            }




            //string Toadd = commModel.UserList[0].ToString();
            //string tomailid=UserMail[0]+""+UserMail[1]+""+UserMail[2]+""+UserMail[3]; 

            objmail = new ViewEmail();
            objmail.FromEmail = EmailAddress;
            objmail.ToEmail = temp.Remove(temp.Length - 1).ToString();

            objmail.Subject = commModel.subject;
            Session["Subject"] = commModel.subject;
            objmail.Attachment = commModel.attachment.ToString();
            objmail.Message = commModel.Message;
            Session["Message"] = commModel.Message;
            objmail.Sign = commModel.defaultSignature.ToString();
            Session["Sign"] = commModel.defaultSignature.ToString();
            if (commModel.defaultSignature == true)
            {
                objmail.Message = commModel.Message + Environment.NewLine + Environment.NewLine + "Thanks," + Environment.NewLine + Environment.NewLine + FirstName;
                Session["Message"] = commModel.Message + Environment.NewLine + Environment.NewLine + "Thanks," + Environment.NewLine + Environment.NewLine + FirstName;
            }
            else
            {
                objmail.Message = commModel.Message;
                Session["Message"] = commModel.Message;
            }
            objmail.IsActive = true;
            objmail.SentOn = DateTime.Now;
            objmail.CheckBox = true;
            objdb.AddToViewEmails(objmail);
            objdb.SaveChanges();


            if (fileList != null && fileList.Count > 0)
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                foreach (var file in fileList)
                {
                    try
                    {
                        if (file != null)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(folderPath, fileName);
                            file.SaveAs(path);
                        }
                    }
                    catch
                    {

                    }
                }
            }

            else
            {

            }

            try
            {
                foreach (var MailAddress in commModel.UserList)
                {
                    //for(int i=0;i<mail.Count();i++)
                    //{

                    string subject = commModel.Message + " " + MailAddress;

                    string EmailAddress1 = MailAddress;

                    string Signature = string.Empty;
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();



                    //var FirstName = (from u in db.Users
                    //                 where u.UserID == userId
                    //                 select (u.FirstName != null ? u.FirstName : "") + " " + (u.LastName != null ? u.LastName : "")).FirstOrDefault();

                  //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    List<string> TestMailIds = new List<string>();

                    //TestMailIds.Add(Convert.ToString(mail));
                    TestMailIds.Add(Convert.ToString(MailAddress));
                    //string folderPath = Server.MapPath("~/EmailImages/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));

                    if (ServerName  != "http://win2012srv:88/")
                    {

                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                        Task.Factory.StartNew(() =>
                        {
                            // DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, commModel.subject + " - Test Mail Please Ignore", MailBuilder.InlineSignatureHtmlPage(FirstName, "", commModel.subject + " - Test Mail Please Ignore", commModel.Message.Replace("\n", "<br/>"), commModel.defaultSignature), "Test-HRMS@dsrc.co.in", null, null, MailIds, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));
                            DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, commModel.subject + " - Test Mail Please Ignore", MailBuilder.InlineSignatureHtmlPage(FirstName, "", commModel.subject + " - Test Mail Please Ignore", subject.Replace("\n", "<br/>"), commModel.defaultSignature), "Test-HRMS@dsrc.co.in", null, null, MailIds, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));

                        });
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, commModel.subject, MailBuilder.InlineSignatureHtmlPage(FirstName, "", commModel.subject, commModel.Message.Replace("\n", "<br/>"), commModel.defaultSignature), EmailAddress1, null, null, TestMailIds, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));

                        });
                    }



                    MailId MailDetails = new MailId();
                    //foreach (var file in commModel.UserList)
                    //{
                    //    List<string> UserMails = commModel.UserList;
                    //}

                    if (MailSender.SentSuccess == false)
                    {
                        MailDetails.UserLists = MailAddress;

                        //MailDetails.MailAdd = Convert.ToString(mail);
                        MailDetails.Message = "Mail sent successfully";

                        ViewBag.flag = true;
                        Session["status1"] = MailAddress;


                        //   int userID = Convert.ToInt32(Session["UserID"]);


                        int userID = Convert.ToInt32(Session["UserID"]);
                        var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                        int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                        int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                        Session["MailSentCount"] = sentcount;
                        Session["MailCount"] = inboxcount;


                    }


                    //else{

                    //    MailDetails.MailAdd = MailAddress;
                    //    MailDetails.Message = "Message sent successfully";
                    //}
                    else if (MailSender.SentSuccess == true)
                    {
                        if (commModel.UserList != null)
                        {
                            MailDetails.UserLists = MailAddress;
                            //MailDetails.MailAdd = Convert.ToString(mail);
                            MailDetails.Message = "Mail sent successfully";
                            int userID = Convert.ToInt32(Session["UserID"]);
                            ViewBag.flag = false;
                            Session["status2"] = MailAddress;


                            var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                            int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                            Session["MailCount"] = inboxcount;
                        }

                        else
                        {

                            commModel.ErrorSuccessMessage = "Error in sending mail";
                        }
                    }

                    AllMailList.Add(MailDetails);
                    //i = i + mail.Count();
                }

                commModel.ErrorSuccessMessage = "Mail sent successfully.";


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                commModel.ErrorSuccessMessage = "Error in sending mail";

            }
            return View(AllMailList);
            // return View();
        }

        [HttpGet]
        public ActionResult Resend(List<string> MailAdd)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");

                MailInvitesModel commModel = new MailInvitesModel();
                var attach = Session["attachment"];

                var Subject = Session["subject"];

                // List<string> MailAdds =(MailAdd[0]);


                string Message = Session["Message"].ToString() + "," + MailAdd[0];
                //commModel.Message = Message;

                //if (attach.Count()) < 1))
                //{
                //    attach = false;
                //}
                //else
                //{
                //    attach = true;
                //}

                int userId = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                string username = System.Web.HttpContext.Current.Application["UserName"].ToString();
                //if (MailSender.SentSuccess == false)
                //{

                string EmailAddress = communicationHelper.GetMailId(userId);
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();


                string folderPath = Server.MapPath("~/EmailImages/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));

                string Signature = string.Empty;

                bool sign = Convert.ToBoolean(Session["Sign"]);


                var FirstName = (from u in db.Users
                                 where u.UserID == userId
                                 select (u.FirstName != null ? u.FirstName : "") + " " + (u.LastName != null ? u.LastName : "")).FirstOrDefault();

               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                var Subject1 = Session["subject"];
                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();



                if (ServerName  != "http://win2012srv:88/")
                {
                    Task.Factory.StartNew(() =>
                    {
                        // DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, Subject + " - Test Mail Please Ignore", MailBuilder.InlineSignatureHtmlPage(FirstName, "", Subject + " - Test Mail Please Ignore", Message, commModel.defaultSignature), "Test-HRMS@dsrc.co.in", null, null, MailAdd, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));
                        DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, Subject + " - Test Mail Please Ignore", MailBuilder.InlineSignatureHtmlPage(FirstName, "", Subject + " - Test Mail Please Ignore", Message, sign), "Test-HRMS@dsrc.co.in", null, null, MailIds, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));
                    });
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {


                        DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, Subject + " - Test Mail Please Ignore", MailBuilder.InlineSignatureHtmlPage(FirstName, "", Subject + " - Test Mail Please Ignore", Message, commModel.defaultSignature), "Test-HRMS@dsrc.co.in", null, null, MailAdd, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));
                        //DsrcMailSystem.MailSender.SendInlineAttachmentMail(folderPath, Subject, MailBuilder.InlineSignatureHtmlPage(FirstName, "", Subject, Message, commModel.defaultSignature), EmailAddress, null, null, MailAdd, Server.MapPath("~/Content/Template/images/logo.png"), (commModel.MailType == 2));


                    });
                }

                // }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return Json("success", JsonRequestBehavior.AllowGet);
            //return View();
        }




        [HttpGet]
        //   [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead, HR,Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]        
        public ActionResult ViewMail()
        {
            List<DSRCManagementSystem.Models.SendEmail> objmail = new List<DSRCManagementSystem.Models.SendEmail>();
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                string username = System.Web.HttpContext.Current.Application["UserName"].ToString();


                objmail = db.ViewEmails.Where(o => o.FromEmail == username).Where(n => n.IsActive == true).Select(x => new DSRCManagementSystem.Models.SendEmail { FromEmail = x.FromEmail, Message = x.Message, Subject = x.Subject, To = x.ToEmail, Attachment = x.Attachment, Sign = x.Sign, ID = x.Id, SentOn = (DateTime)x.SentOn }).OrderByDescending(x => x.SentOn).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objmail);
        }


        [HttpGet]
        //   [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead, HR,Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]  
        public ActionResult DeleteMail(int id)
        {
            try
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var obj = db.ViewEmails.Where(x => x.Id == id).FirstOrDefault();
                obj.IsActive = false;
                db.SaveChanges();
                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        //[HttpPost]
        //public ActionResult DeleteM(int id)
        //{
        //    try
        //    {

        //        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //        var obj = db.ViewEmails.Where(x => x.Id == id).FirstOrDefault();
        //        obj.IsActive = false;
        //        db.SaveChanges();
        //        return View();
        //    }
        //    catch (Exception Ex)
        //    {
        //        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
        //        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
        //    }
        //    return View();
        //}


        #endregion


        #region "View Messages"

        //[DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead, Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]

        public ActionResult ViewMessage()
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            communicationHelper.RemoveExpiredMessage();
            List<ViewMessage> messageLIst = communicationHelper.ViewMessage(UserID: userID);
            try
            {
                List<ViewMessage> messageIdlist = communicationHelper.GetMessageLIst();
                DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
                var list = dbHrms.communicationMessages.Where(o => o.UserID == userID).Select(o => new
                {
                    MessageId = o.MessageId,
                    Message = o.MessageText.Replace("<br />", "\n"),
                    ValidFrom = o.Valid_From
                }).ToList();
                //}).OrderByDescending(o => o.ValidFrom).ToList();

                ViewBag.MessageList = new SelectList(list, "MessageId", "Message");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(messageLIst);
        }

        [HttpPost]
        // [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead, Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
        public ActionResult ViewMessage(int? MessageId)
        {

            int userID = Convert.ToInt32(Session["UserID"]);
            communicationHelper.RemoveExpiredMessage();
            List<ViewMessage> messageLIst;
            if (MessageId == null)
            {
                messageLIst = communicationHelper.ViewMessage(userID);
            }
            else
            {
                messageLIst = communicationHelper.ViewMessage(userID, MessageId ?? 0);
            }

            List<ViewMessage> messageIdlist = communicationHelper.GetMessageLIst();
            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            var list = dbHrms.communicationMessages.Where(o => o.UserID == userID).Select(o => new
            {
                MessageId = o.MessageId,
                Message = o.MessageText.Replace("<br />", "\n"),
                ValidFrom = o.Valid_From

                //}).ToList();
            }).OrderByDescending(o => o.ValidFrom).ToList();
            //}).OrderByDescending(o=>o.MessageId).ToList();
            //}).OrderByDescending(o=>o.Message).ToList();
            ViewBag.MessageList = new SelectList(list, "MessageId", "Message");

            return View(messageLIst);
        }

        // [DSRCAuthorize(Roles = "Vice President, Project Manager,Assistant Manager-Recruitment,Tech Lead,Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
        public ActionResult MessageGrid(int MessageId)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            List<ViewMessage> messageLIst = communicationHelper.ViewMessage(userID, MessageId);
            return Json(messageLIst, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region "Delete Message"
        //[DSRCAuthorize(Roles = "Vice President,Tech Lead, Project Manager,Assistant Manager-Recruitment,Head - Quality,Business Development Manager,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
        public ActionResult DeleteMessage(int MessageID)
        {
            communicationHelper.DeleteMessage(MessageID);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion




        public ActionResult ViewEmail()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Inbox()
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();
                Session["Back"] = "Inbox";
                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;


                List<DSRCManagementSystem.Models.sentbox> sentmodel = new List<DSRCManagementSystem.Models.sentbox>();
                objmodel = (from p in objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true)
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                //message = p.Message,
                                senton = p.SentOn,
                                Checkbox = p.CheckBox,
                                Attachment = p.Attachmentpath
                            }).OrderByDescending(x => x.senton).ToList();



                if (TempData["Mail"] == "Success")
                {
                    TempData["Compose"] = "Sent";
                }

                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }

        public ActionResult ComposeMail(string myParams, string memberid, string message)
        {
            DSRCManagementSystem.Models.Inbox objbox = new DSRCManagementSystem.Models.Inbox();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int id = Convert.ToInt32(myParams);
                Session["Message"] = message;
                int fromid = Convert.ToInt32(memberid);

                var reporing = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                select new
                                {
                                    FromMailID = p.UserID,
                                    UserName = p.EmailAddress
                                }).ToList().Distinct();



                var reporting1 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                  select new
                                  {
                                      Id1 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();

                var reporting2 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                  select new
                                  {
                                      Id2 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();

                var to = objdb.Users.Where(x => x.UserID == fromid).Select(o => o.UserID).ToList();

                ViewBag.EmailMail = new MultiSelectList(reporing, "FromMailID", "UserName", to);

                ViewBag.Email4 = new MultiSelectList(reporting1, "Id1", "UserName");

                ViewBag.Email5 = new MultiSelectList(reporting2, "Id2", "UserName");

                var ToMail = objdb.Users.Where(x => x.UserID == fromid).Select(o => o.UserName).FirstOrDefault();
                var FromMail = objdb.Users.Where(x => x.UserID == id).Select(o => o.UserName).FirstOrDefault();
                objbox.ToEmail = ToMail;
                objbox.From = FromMail;
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;


                return View(objbox);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objbox);
        }



        [HttpGet]
        public ActionResult ForwardMail(string myParams, string memberid, string message)
        {

            DSRCManagementSystem.Models.Inbox objbox = new DSRCManagementSystem.Models.Inbox();

            Session["ForwardMessage"] = message;
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;


                int id = Convert.ToInt32(myParams);

                var reporing = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                select new
                                {
                                    Id = p.UserID,
                                    UserName = p.EmailAddress
                                }).ToList().Distinct();



                var reporting1 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                  select new
                                  {
                                      Id1 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();

                var reporting2 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                  select new
                                  {
                                      Id2 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();

                ViewBag.Email3 = new MultiSelectList(reporing, "Id", "UserName");

                ViewBag.Email4 = new MultiSelectList(reporting1, "Id1", "UserName");

                ViewBag.Email5 = new MultiSelectList(reporting2, "Id2", "UserName");


                int fromid = Convert.ToInt32(memberid);
                var ToMail = string.Empty;
                var FromMail = objdb.Users.Where(x => x.UserID == fromid).Select(o => o.UserName).FirstOrDefault();
                objbox.ToEmail = ToMail;
                objbox.From = FromMail;
                objbox.message = message;
                return View(objbox);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objbox);
        }

        [HttpPost]
        public ActionResult ForwardMail(string From, string To, string CC, string BCC, string Subject, string Message)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;


                DSRCManagementSystem.Models.Inbox objbox = new DSRCManagementSystem.Models.Inbox();
              //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                List<string> TestMailIds = new List<string>();
                //string folderPath = Server.MapPath("~/EmailImages/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));
                if (From != "")
                {
                    bool isEmail = Regex.IsMatch(From, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                    if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                    {
                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                    }
                }
                List<int> toemailContains = new List<int>();
                if (To != "")
                {
                    if (To.Count() == 1)
                    {
                        int to = Convert.ToInt32(To);
                        toemailContains.Add(Convert.ToInt32(to));
                        var tomail = objdb.Users.Where(x => x.UserID == to).Select(o => o.EmailAddress).FirstOrDefault();
                        DSRCManagementSystem.ViewEmail objmail = new DSRCManagementSystem.ViewEmail();
                        objmail.FromEmail = From;
                        objmail.ToEmail = tomail;
                        objmail.IsActive = true;
                        objmail.Subject = Subject;
                        objmail.SentOn = DateTime.Now;
                        objmail.Sign = null;
                        objmail.Message = Message;
                        objmail.Attachment = null;
                        objmail.Attachmentpath = null;
                        objmail.CheckBox = true;
                        objdb.AddToViewEmails(objmail);
                        objdb.SaveChanges();

                        bool isEmail = Regex.IsMatch(tomail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        string[] value = To.Split(',');

                        List<string> objuser = new List<string>();
                        for (int i = 0; i < value.Count(); i++)
                        {
                            toemailContains.Add(Convert.ToInt32(value[i]));
                            int objto = Convert.ToInt32(value[i]);
                            var toemail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                            string[] email = value[i].Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            DSRCManagementSystem.ViewEmail objmail = new DSRCManagementSystem.ViewEmail();
                            objmail.FromEmail = From;
                            objmail.ToEmail = toemail;
                            objmail.IsActive = true;
                            objmail.Subject = Subject;
                            objmail.SentOn = DateTime.Now;
                            objmail.Sign = null;
                            objmail.Message = Message;
                            objmail.Attachment = null;
                            objmail.Attachmentpath = null;
                            objmail.CheckBox = true;
                            objdb.AddToViewEmails(objmail);
                            objdb.SaveChanges();

                            bool isEmail = Regex.IsMatch(toemail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                            if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                            {
                                return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                            }

                        }

                    }

                }

                if (CC != "")
                {
                    List<int> ccemailContains = new List<int>();


                    string[] value = CC.Split(',');
                    List<string> objuser = new List<string>();
                    for (int i = 0; i < value.Count(); i++)
                    {
                        ccemailContains.Add(Convert.ToInt32(value[i]));

                    }

                    ccemailContains.Except(toemailContains);
                    for (int j = 0; j < ccemailContains.Count(); j++)
                    {
                        int objto = Convert.ToInt32(value[j]);
                        var CCEmail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                        bool isEmail = Regex.IsMatch(CCEmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }

                }

                if (BCC != "")
                {
                    List<int> bccemailContains = new List<int>();


                    string[] value = BCC.Split(',');
                    List<string> objuser = new List<string>();
                    for (int i = 0; i < value.Count(); i++)
                    {
                        bccemailContains.Add(Convert.ToInt32(value[i]));

                    }

                    bccemailContains.Except(toemailContains);
                    for (int j = 0; j < bccemailContains.Count(); j++)
                    {
                        int objto = Convert.ToInt32(value[j]);
                        var BCCEmail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                        bool isEmail = Regex.IsMatch(BCCEmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }

    //                int objto = Convert.ToInt32(BCC);
    //                var BCCEmail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
    //                bool isEmail = Regex.IsMatch(BCCEmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
    //@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
    //                if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
    //                {
    //                    return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
    //                }

                }
                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Communication").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Communication").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {

                    var objWorkinghoursRejectedApproved = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Communication")
                                                           join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                           select new DSRCManagementSystem.Models.Email
                                                           {
                                                               To = p.To,
                                                               CC = p.CC,
                                                               BCC = p.BCC,
                                                               Subject = p.Subject,
                                                               Template = q.TemplatePath
                                                           }).FirstOrDefault();



                    var employeename = objdb.Users.Where(x => x.EmailAddress == From).Select(o => o).FirstOrDefault();


                    // var end = objdb.TS_AssignedTask.Where(x => x.TaskName == id).Select(o => o).FirstOrDefault();

                    //DateTime StartDate = EntityFunctions.TruncateTime(start);
                    //var EndDate = EntityFunctions.TruncateTime(end);

                    string TemplatePathobjworkinghoursRejected = Server.MapPath(objWorkinghoursRejectedApproved.Template);

                    string htmlworkinghours = System.IO.File.ReadAllText(TemplatePathobjworkinghoursRejected);


                    var companyname = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();
                    Font font = new Font("family", 16.0f,
                FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);






                    htmlworkinghours = htmlworkinghours.Replace("#EmployeeName", employeename.FirstName + "  " + employeename.LastName);
                    htmlworkinghours = htmlworkinghours.Replace("#Content", Message);
                    htmlworkinghours = htmlworkinghours.Replace("#ServerName", ServerName);
                    htmlworkinghours = htmlworkinghours.Replace("#ManagerName", employeename.FirstName + "  " + employeename.LastName);
                    htmlworkinghours = htmlworkinghours.Replace("#CompanyName", companyname.AppValue);
                    //htmlworkinghours = htmlworkinghours.Replace("#EmployeeName", Name.FirstName + "" + Name.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#ManagerName", Manager.FirstName + " " + Manager.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#Task", TaskName);
                    //htmlworkinghours = htmlworkinghours.Replace("#StartDateTime", StartTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#EndDateTime", EndTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#Comments", Reason);
                    //htmlworkinghours = htmlworkinghours.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    //htmlworkinghours = htmlworkinghours.Replace("#ServerName", WebConfigurationManager.AppSettings["SeverName"]);

                    string EmailAddress = "";
                    //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    if (ServerName  != "http://win2012srv:88/")
                    {
                        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                        // MailIds.Add("boobalan.k@dsrc.co.in");
                        // MailIds.Add("shaikhakeel@dsrc.co.in");




                        // MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add(objWorkinghoursRejectedApproved.To);
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add("ramesh.s@dsrc.co.in");
                        //MailIds.Add("aruna.m@dsrc.co.in");
                        //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        //MailIds.Add("kirankumar@dsrc.co.in");
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        Task.Factory.StartNew(() =>
                        {
                            var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();

                            //  DsrcMailSystem.MailSender.LDSendMail(null, Subject, null, htmlworkinghours, "Test -" + From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, Subject, null, htmlworkinghours, "Test -" + From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));

                            //   DsrcMailSystem.MailSender.SendMail(null, Subject, null, htmlworkinghours, From, EmailAddress, null);
                        });
                    }
                    else
                    {
                        List<string> MailIds = new List<string>();
                        if (To.Count() > 1)
                        {

                            string[] value = To.Split(',');

                            for (int i = 0; i < value.Count(); i++)
                            {
                                MailIds.Add(value[i]);

                            }
                        }

                        Task.Factory.StartNew(() =>
                        {
                            var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                            //DsrcMailSystem.MailSender.LDSendMail(null, Subject, null, htmlworkinghours, From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, Subject, null, htmlworkinghours, From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Communication", folder, ServerName);

                }

                TempData["Compose"] = "Sent";
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            //    return Json(new { Result = "Success", JsonRequestBehavior.AllowGet });
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult ComposeMail(string From, string To, string CC, string BCC, string Subject, string Message, string Attachment)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Inbox objbox = new DSRCManagementSystem.Models.Inbox();
                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];



                List<string> TestMailIds = new List<string>();

                if (From != "")
                {
                    bool isEmail = Regex.IsMatch(From, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                    if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                    {
                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                    }
                }
                List<int> toemailContains = new List<int>();
                if (To != "")
                {


                    if (To.Count() == 1)
                    {

                        int to = Convert.ToInt32(To);
                        toemailContains.Add(Convert.ToInt32(to));
                        var tomail = objdb.Users.Where(x => x.UserID == to).Select(o => o.EmailAddress).FirstOrDefault();
                        DSRCManagementSystem.ViewEmail objmail1 = new DSRCManagementSystem.ViewEmail();
                        objmail1.FromEmail = From;
                        objmail1.ToEmail = tomail;
                        objmail1.IsActive = true;
                        objmail1.Subject = Subject;
                        objmail1.SentOn = DateTime.Now;
                        objmail1.Sign = null;
                        objmail1.Message = Message;
                        objmail1.Attachment = null;
                        objmail1.Attachmentpath = Attachment;
                        objmail1.CheckBox = true;
                        objdb.AddToViewEmails(objmail1);
                        objdb.SaveChanges();

                        bool isEmail = Regex.IsMatch(tomail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        string[] value = To.Split(',');
                        List<string> objuser = new List<string>();
                        for (int i = 0; i < value.Count(); i++)
                        {
                            toemailContains.Add(Convert.ToInt32(value[i]));
                            int objto = Convert.ToInt32(value[i]);
                            var toemail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                            string[] email = value[i].Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            DSRCManagementSystem.ViewEmail objmail1 = new DSRCManagementSystem.ViewEmail();
                            objmail1.FromEmail = From;
                            objmail1.ToEmail = toemail;
                            objmail1.IsActive = true;
                            objmail1.Subject = Subject;
                            objmail1.SentOn = DateTime.Now;
                            objmail1.Sign = null;
                            objmail1.Message = Message;
                            objmail1.Attachment = null;
                            objmail1.Attachmentpath = Attachment;
                            objmail1.CheckBox = true;
                            objdb.AddToViewEmails(objmail1);
                            objdb.SaveChanges();

                            bool isEmail = Regex.IsMatch(toemail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                            if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                            {
                                return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                            }

                        }

                    }
                }


                if (CC != "")
                {
                    List<int> ccemailContains = new List<int>();


                    string[] value = CC.Split(',');
                    List<string> objuser = new List<string>();
                    for (int i = 0; i < value.Count(); i++)
                    {
                        ccemailContains.Add(Convert.ToInt32(value[i]));

                    }

                    ccemailContains.Except(toemailContains);
                    for (int j = 0; j < ccemailContains.Count(); j++)
                    {
                        int objto = Convert.ToInt32(value[j]);
                        var CCEmail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                        bool isEmail = Regex.IsMatch(CCEmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }

                }

                if (BCC != "")
                {
                    int objto = Convert.ToInt32(BCC);
                    var BCCEmail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                    bool isEmail = Regex.IsMatch(BCCEmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                    if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                    {
                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                    }

                }
                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Communication").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Communication").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var Mail = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Communication")
                                join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                select new DSRCManagementSystem.Models.Email
                                {
                                    To = p.To,
                                    CC = p.CC,
                                    BCC = p.BCC,
                                    Subject = p.Subject,
                                    Template = q.TemplatePath
                                }).FirstOrDefault();



                    var employeename = objdb.Users.Where(x => x.EmailAddress == From).Select(o => o).FirstOrDefault();


                    // var end = objdb.TS_AssignedTask.Where(x => x.TaskName == id).Select(o => o).FirstOrDefault();

                    //DateTime StartDate = EntityFunctions.TruncateTime(start);
                    //var EndDate = EntityFunctions.TruncateTime(end);

                    string mailRejected = Server.MapPath(Mail.Template);

                    string MailView = System.IO.File.ReadAllText(mailRejected);


                    var companyname = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();

                    MailView = MailView.Replace("#EmployeeName", employeename.FirstName + "  " + employeename.LastName);
                    MailView = MailView.Replace("#Content", Message);
                    MailView = MailView.Replace("#ServerName",ServerName);
                    MailView = MailView.Replace("#ManagerName", employeename.FirstName + "   " + employeename.LastName);
                    MailView = MailView.Replace("#CompanyName", companyname.AppValue);
                    //htmlworkinghours = htmlworkinghours.Replace("#EmployeeName", Name.FirstName + "" + Name.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#ManagerName", Manager.FirstName + " " + Manager.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#Task", TaskName);
                    //htmlworkinghours = htmlworkinghours.Replace("#StartDateTime", StartTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#EndDateTime", EndTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#Comments", Reason);
                    //htmlworkinghours = htmlworkinghours.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    //htmlworkinghours = htmlworkinghours.Replace("#ServerName", WebConfigurationManager.AppSettings["SeverName"]);

                    string EmailAddress = "";
                    //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    if (ServerName  != "http://win2012srv:88/")
                    {
                        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();


                        // MailIds.Add("boobalan.k@dsrc.co.in");
                        // MailIds.Add("shaikhakeel@dsrc.co.in");
                        // MailIds.Add("ramesh.s@dsrc.co.in");
                        // MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add(objWorkinghoursRejectedApproved.To);
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add("ramesh.s@dsrc.co.in");
                        //MailIds.Add("aruna.m@dsrc.co.in");
                        //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        //MailIds.Add("kirankumar@dsrc.co.in");
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        Task.Factory.StartNew(() =>
                        {
                            var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                            List<string> toemail = new List<string>();
                            toemail.Add(EmailAddress);
                            //  DsrcMailSystem.MailSender.LDSendMail(null, Subject, null, htmlworkinghours, "Test - " + From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, Subject, null, MailView, From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()), Attachment);

                            //   DsrcMailSystem.MailSender.SendMail(null, Subject, null, htmlworkinghours, From, EmailAddress, null);
                        });
                    }
                    else
                    {

                        List<string> MailIds = new List<string>();
                        if (To.Count() > 1)
                        {

                            string[] value = To.Split(',');

                            for (int i = 0; i < value.Count(); i++)
                            {
                                MailIds.Add(value[i]);

                            }
                        }


                        Task.Factory.StartNew(() =>
                        {
                            var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                            // DsrcMailSystem.MailSender.LDSendMail(null, Subject, null, htmlworkinghours, From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                            DsrcMailSystem.MailSender.SendMail(null, Subject, null, MailView, From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Communication", folder, ServerName);

                }
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();
                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;
                TempData["Compose"] = "Sent";
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult SentEmailsView()
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();
                List<DSRCManagementSystem.Models.sentbox> sentmodel = new List<DSRCManagementSystem.Models.sentbox>();

                objmodel = (from p in objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true)
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                message = p.Message,
                                senton = p.SentOn
                            }).OrderBy(x => x.senton).ToList();

                ViewData["SentCount"] = sentmodel.Count();

                ViewData["InboxCount"] = objmodel.Count();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = inboxcount;
                Session["MailCount"] = sentcount;

                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }

        public ActionResult ViewMailPage(string myParams)
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;

                int contentid = Convert.ToInt32(myParams);
                var updaterecord = objdb.ViewEmails.Where(x => x.Id == contentid && x.IsActive == true).Select(o => o).FirstOrDefault();
                // int userID = Convert.ToInt32(Session["UserID"]);
                if (updaterecord != null)
                {
                    updaterecord.CheckBox = false;
                    objdb.SaveChanges();
                }
                else
                {
                    updaterecord.CheckBox = true;
                    objdb.SaveChanges();
                }


                objmodel = (from p in objdb.ViewEmails.Where(x => x.Id == contentid && x.IsActive == true)
                            //join t in objdb.Users.Where(x=>x.IsActive == true) on p.ToEmail equals t.EmailAddress
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                message = p.Message,
                                senton = p.SentOn,
                                Attachment = p.Attachmentpath,
                                Checkbox = p.CheckBox
                            }).OrderBy(x => x.senton).ToList();


                foreach (var item in objmodel)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    var uservalue = db.Users.Where(x => x.EmailAddress == item.From).Select(o => o).FirstOrDefault();
                    if (uservalue == null)
                    {
                        item.UserName = "";
                    }
                    else
                    {

                        item.UserName = uservalue.FirstName + " " + uservalue.LastName;
                    }
                    var uservalue1 = db.Users.Where(x => x.EmailAddress == item.to && x.IsActive == true).Select(o => o).FirstOrDefault();
                    var uservalue2 = db.Users.Where(x => x.EmailAddress == item.From && x.IsActive == true).Select(o => o).FirstOrDefault();
                    item.Fromusereid = uservalue2.UserID;
                    item.Touserid = uservalue1.UserID;
                }
                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);

        }

        [HttpGet]
        public ActionResult Compose()
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Inbox objmodel = new DSRCManagementSystem.Models.Inbox();

            try
            {
                int userID = Convert.ToInt32(Session["UserID"]);
                objmodel.From = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                //  int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;


                var reporing = (from p in objdb.Users .Where(x => x.IsActive == true && x.UserName!=null)
                             select new 
                                {
                                    Id = p.UserID,
                                    UserName = p.EmailAddress
                                }).ToList().Distinct();



                var reporting1 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!= null)
                                  select new
                                  {
                                      Id1 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();

                var reporting2 = (from p in objdb.Users.Where(x => x.IsActive == true && x.UserName!=null)
                                  select new
                                  {
                                      Id2 = p.UserID,
                                      UserName = p.EmailAddress
                                  }).ToList().Distinct();


                ViewBag.Email3 = new MultiSelectList(reporing, "Id", "UserName");

                ViewBag.Email4 = new MultiSelectList(reporting1, "Id1", "UserName");

                ViewBag.Email5 = new MultiSelectList(reporting2, "Id2", "UserName");
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
        public ActionResult Compose(string From, string To, string CC, string BCC, string Subject, string Message)
        {

            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.Inbox objbox = new DSRCManagementSystem.Models.Inbox();
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;

                List<string> TestMailIds = new List<string>();
                if (From != "")
                {
                    bool isEmail = Regex.IsMatch(From, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                    if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                    {
                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                    }
                }

                if (To != "")
                {
                    if (To.Count() == 1)
                    {
                        int to = Convert.ToInt32(To);
                        var tomail = objdb.Users.Where(x => x.UserID == to).Select(o => o.EmailAddress).FirstOrDefault();
                        DSRCManagementSystem.ViewEmail objmail1 = new DSRCManagementSystem.ViewEmail();
                        objmail1.FromEmail = From;
                        objmail1.ToEmail = tomail;
                        objmail1.IsActive = true;
                        objmail1.Subject = Subject;
                        objmail1.SentOn = DateTime.Now;
                        objmail1.Sign = null;
                        objmail1.Message = Message;
                        objmail1.Attachment = null;
                        objmail1.Attachmentpath = null;
                        objmail1.CheckBox = true;
                        objdb.AddToViewEmails(objmail1);
                        objdb.SaveChanges();

                        bool isEmail = Regex.IsMatch(tomail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        DSRCManagementSystem.ViewEmail objmail1 = new DSRCManagementSystem.ViewEmail();               
                        string[] value = To.Split(',');
                        int j = value.Count();
                        string email = "";
                        
                        for (int k = 0; k <j ; k++)
                        {
                            string temp = value[k];
                            int tem = Convert.ToInt32(temp);
                            var EMailI = objdb.Users.Where(x => x.UserID == tem).Select(o => o.EmailAddress).FirstOrDefault();

                            bool isEmail = Regex.IsMatch(EMailI, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                            if (!isEmail)
                            {
                                return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                            }
                            email += EMailI + ',';
                        }                        
                        objmail1.FromEmail = From;
                        objmail1.ToEmail = email.Remove(email.Length - 1).ToString();
                        objmail1.IsActive = true;
                        objmail1.Subject = Subject;
                        objmail1.SentOn = DateTime.Now;
                        objmail1.Sign = null;
                        objmail1.Message = Message;
                        objmail1.Attachment = null;
                        objmail1.Attachmentpath = null;
                        objmail1.CheckBox = true;
                        objdb.AddToViewEmails(objmail1);
                        objdb.SaveChanges();
                         
                    }

                    //string[] value = To.Split(',');
                    //List<string> objuser = new List<string>();
      //                  for (int i = 0; i < value.Count(); i++)
      //                  {                            
      //                      int objto = Convert.ToInt32(value[i]);
      //                      var toemail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
      //                      string[] email = value[i].Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
      //                      DSRCManagementSystem.ViewEmail objmail1 = new DSRCManagementSystem.ViewEmail();
      //                      objmail1.FromEmail = From;
      //                      objmail1.ToEmail = toemail;
      //                      objmail1.IsActive = true;
      //                      objmail1.Subject = Subject;
      //                      objmail1.SentOn = DateTime.Now;
      //                      objmail1.Sign = null;
      //                      objmail1.Message = Message;
      //                      objmail1.Attachment = null;
      //                      objmail1.Attachmentpath = null;
      //                      objmail1.CheckBox = true;
      //                      objdb.AddToViewEmails(objmail1);
      //                      objdb.SaveChanges();

      //                      bool isEmail = Regex.IsMatch(toemail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      //@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
      //                      if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
      //                      {
      //                          return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
      //                      }

      //                  }

                //    }
                }


                if (CC != "")
                {
                    if (CC.Count() == 1)
                    {
                        int objcc = Convert.ToInt32(CC);
                        var objccmail = objdb.Users.Where(x => x.UserID == objcc).Select(o => o.EmailAddress).FirstOrDefault();
                        
                        bool isEmail = Regex.IsMatch(objccmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        string[] value = CC.Split(',');
                        List<string> objuser = new List<string>();
                        for (int i = 0; i < value.Count(); i++)
                        {
                            string[] email = value[i].Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            int objcc = Convert.ToInt32(value[i]);

                            var objccmail = objdb.Users.Where(x => x.UserID == objcc).Select(o => o.EmailAddress).FirstOrDefault();

                            bool isEmail = Regex.IsMatch(objccmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                            if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                            {
                                return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                            }

                        }

                    }
                }

                if (BCC != "")
                {
                    if (BCC.Count() == 1)
                    {
                        int objbcc = Convert.ToInt32(BCC);
                        var objbccmail = objdb.Users.Where(x => x.UserID == objbcc).Select(o => o.EmailAddress).FirstOrDefault();

                        bool isEmail = Regex.IsMatch(objbccmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
        @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                        if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                        {
                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        string[] value = BCC.Split(',');
                        List<string> objuser = new List<string>();
                        for (int i = 0; i < value.Count(); i++)
                        {
                            int objbcc = Convert.ToInt32(value[i]);
                            var objbccmail = objdb.Users.Where(x => x.UserID == objbcc).Select(o => o.EmailAddress).FirstOrDefault();
                            string[] email = value[i].Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            bool isEmail = Regex.IsMatch(objbccmail, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
      @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                            if (!isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                            {
                                return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                            }

                        }

                    }

                }

                var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Communication").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = objdb.EmailTemplates.Where(o => o.TemplatePurpose == "Communication").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {

                    var objWorkinghoursRejectedApproved = (from p in objdb.EmailPurposes.Where(x => x.EmailPurposeName == "Communication")
                                                           join q in objdb.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                           select new DSRCManagementSystem.Models.Email
                                                           {
                                                               To = p.To,
                                                               CC = p.CC,
                                                               BCC = p.BCC,
                                                               Subject = p.Subject,
                                                               Template = q.TemplatePath
                                                           }).FirstOrDefault();



                    var employeename = objdb.Users.Where(x => x.EmailAddress == From).Select(o => o).FirstOrDefault();


                    // var end = objdb.TS_AssignedTask.Where(x => x.TaskName == id).Select(o => o).FirstOrDefault();

                    //DateTime StartDate = EntityFunctions.TruncateTime(start);
                    //var EndDate = EntityFunctions.TruncateTime(end);

                    string TemplatePathobjworkinghoursRejected = Server.MapPath(objWorkinghoursRejectedApproved.Template);

                    string htmlworkinghours = System.IO.File.ReadAllText(TemplatePathobjworkinghoursRejected);


                    var companyname = objdb.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o).FirstOrDefault();

                    htmlworkinghours = htmlworkinghours.Replace("#EmployeeName", employeename.FirstName + "  " + employeename.LastName);
                    htmlworkinghours = htmlworkinghours.Replace("#Content", Message);
                    htmlworkinghours = htmlworkinghours.Replace("#ServerName", ServerName);
                    htmlworkinghours = htmlworkinghours.Replace("#ManagerName", employeename.FirstName + "   " + employeename.LastName);
                    htmlworkinghours = htmlworkinghours.Replace("#CompanyName", companyname.AppValue);
                    //htmlworkinghours = htmlworkinghours.Replace("#EmployeeName", Name.FirstName + "" + Name.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#ManagerName", Manager.FirstName + " " + Manager.LastName);
                    //htmlworkinghours = htmlworkinghours.Replace("#Task", TaskName);
                    //htmlworkinghours = htmlworkinghours.Replace("#StartDateTime", StartTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#EndDateTime", EndTime);
                    //htmlworkinghours = htmlworkinghours.Replace("#Comments", Reason);
                    //htmlworkinghours = htmlworkinghours.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    //htmlworkinghours = htmlworkinghours.Replace("#ServerName", WebConfigurationManager.AppSettings["SeverName"]);

                    string EmailAddress = "";
                    //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    if (ServerName  != "http://win2012srv:88/")
                    {
                        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        // MailIds.Add("boobalan.k@dsrc.co.in");
                        // MailIds.Add("shaikhakeel@dsrc.co.in");
                        // MailIds.Add("ramesh.s@dsrc.co.in");
                        // MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add(objWorkinghoursRejectedApproved.To);
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");
                        //MailIds.Add("ramesh.s@dsrc.co.in");
                        //MailIds.Add("aruna.m@dsrc.co.in");
                        //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                        //MailIds.Add("kirankumar@dsrc.co.in");
                        //MailIds.Add("francispaul.k.c@dsrc.co.in");

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }




                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                       // string CcMail = "";
                       // string BccMail = "";
                        List<string> CCMailIds = new List<string>();
                        List<string> BCCMailIds = new List<string>();
                      

                       if (CC.Count() >= 1)
                       {
                           string[] value = CC.Split(',');

                           for (int i = 0; i < value.Count(); i++)
                           {
                               int objcc = Convert.ToInt32(value[i]);
                               var objccmail = db.Users.Where(x => x.UserID == objcc).Select(o => o.EmailAddress).FirstOrDefault();
                               CCMailIds.Add(objccmail);

                           }

                       }

                       if (BCC.Count() >= 1)
                       {
                           string[] value = BCC.Split(',');

                           for (int i = 0; i < value.Count(); i++)
                           {
                               int objbcc = Convert.ToInt32(value[i]);
                               var objbccmail = objdb.Users.Where(x => x.UserID == objbcc).Select(o => o.EmailAddress).FirstOrDefault();
                               BCCMailIds.Add(objbccmail);
                           }

                       }
                       //   foreach (string ccmail in CCMailIds)
                       // {
                       //     CcMail += ccmail + ",";
                       // }
                       // CcMail = CcMail.Remove(CcMail.Length - 1);
                       // foreach (string bccmail in BCCMailIds)
                       // {
                       //     BccMail += bccmail + ",";
                       // }
                       // BccMail = BccMail.Remove(BccMail.Length - 1);
                       
                        //Task.Factory.StartNew(() =>
                        //{
                        //    var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                        //    //  DsrcMailSystem.MailSender.LDSendMail(null, Subject, null, htmlworkinghours, "Test - " + From, EmailAddress, CC, BCC, Server.MapPath(logo.AppValue.ToString()));
                        //    DsrcMailSystem.MailSender.SendMail(null, Subject, null, htmlworkinghours, "Test - " + From, EmailAddress, Server.MapPath(logo.AppValue.ToString()));

                        //});
                       Task.Factory.StartNew(() =>
                       {
                           var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                           DsrcMailSystem.MailSender.Inbox(null, Subject, null, htmlworkinghours, "Test--" + From, MailIds, CCMailIds, BCCMailIds, Server.MapPath(logo.AppValue.ToString()));

                       });
                    }

                    else
                    {
                        List<string> MailIds = new List<string>();
                        List<string> CCMailIds = new List<string>();
                        List<string> BCCMailIds = new List<string>();
                        if (To.Count() > 1)
                        {
                            string[] value = To.Split(',');

                            for (int i = 0; i < value.Count(); i++)
                            {
                                int objto = Convert.ToInt32(value[i]);
                                var toemail = objdb.Users.Where(x => x.UserID == objto).Select(o => o.EmailAddress).FirstOrDefault();
                                MailIds.Add(toemail);

                            }
                        }

                        if (CC.Count() > 1)
                        {
                            string[] value = CC.Split(',');

                            for (int i = 0; i < value.Count(); i++)
                            {
                                int objcc = Convert.ToInt32(value[i]);
                                var objccmail = objdb.Users.Where(x => x.UserID == objcc).Select(o => o.EmailAddress).FirstOrDefault();
                                CCMailIds.Add(objccmail);

                            }

                        }

                        if (BCC.Count() > 1)
                        {
                            string[] value = BCC.Split(',');

                            for (int i = 0; i < value.Count(); i++)
                            {
                                int objbcc = Convert.ToInt32(value[i]);
                                var objbccmail = objdb.Users.Where(x => x.UserID == objbcc).Select(o => o.EmailAddress).FirstOrDefault();
                                BCCMailIds.Add(objbccmail);

                            }

                        }

                        Task.Factory.StartNew(() =>
                        {
                            var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                            DsrcMailSystem.MailSender.Inbox(null, Subject, null, htmlworkinghours, "Test--" + From, MailIds, CCMailIds, BCCMailIds, Server.MapPath(logo.AppValue.ToString()));

                        });
                    }
                }
                else
                {

                    ExceptionHandlingController.TemplateMissing("Communication", folder, ServerName);

                }




                TempData["Mail"] = "Success";

                return Json("Success", JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult SentInbox()
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();
                int inboxcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = inboxcount;
                
                List<DSRCManagementSystem.Models.sentbox> sentmodel = new List<DSRCManagementSystem.Models.sentbox>();
                objmodel = (from p in objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true)
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                //message = p.Message,
                                senton = p.SentOn,
                                Checkbox = p.CheckBox
                            }).OrderByDescending(x => x.senton).ToList();

                if (TempData["Mail"] == "Success")
                {
                    TempData["Compose"] = "Sent";
                }

                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);


        }
        [HttpGet]
        public ActionResult SentMailInbox()
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();
                int inboxcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = inboxcount;

                List<DSRCManagementSystem.Models.sentbox> sentmodel = new List<DSRCManagementSystem.Models.sentbox>();
                objmodel = (from p in objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true)
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                //message = p.Message,
                                senton = p.SentOn,
                                Checkbox = p.CheckBox
                            }).OrderByDescending(x => x.senton).ToList();

                if (TempData["Mail"] == "Success")
                {
                    TempData["Compose"] = "Sent";
                }

                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);


        }


        [HttpGet]
        public ActionResult ViewSentBox(string myParams)
        {
            List<DSRCManagementSystem.Models.Inbox> objmodel = new List<DSRCManagementSystem.Models.Inbox>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                int contentid = Convert.ToInt32(myParams);
                var updaterecord = objdb.ViewEmails.Where(x => x.Id == contentid && x.IsActive == true).Select(o => o).FirstOrDefault();

                if (updaterecord != null)
                {
                    updaterecord.CheckBox = false;
                    objdb.SaveChanges();
                }
                else
                {
                    updaterecord.CheckBox = true;
                    objdb.SaveChanges();
                }

                int userID = Convert.ToInt32(Session["UserID"]);
                var useremail = objdb.Users.Where(x => x.UserID == userID).Select(o => o.EmailAddress).FirstOrDefault();

                int inboxcount = objdb.ViewEmails.Where(x => x.ToEmail == useremail && x.IsActive == true && x.CheckBox == true).Count();
                int sentcount = objdb.ViewEmails.Where(x => x.FromEmail == useremail && x.IsActive == true).Count();
                Session["MailSentCount"] = sentcount;
                Session["MailCount"] = inboxcount;

                objmodel = (from p in objdb.ViewEmails.Where(x => x.Id == contentid && x.IsActive == true)
                            //join t in objdb.Users on p.ToEmail equals t.EmailAddress
                            select new DSRCManagementSystem.Models.Inbox
                            {
                                id = p.Id,
                                header = p.Subject,
                                From = p.FromEmail,
                                to = p.ToEmail,
                                attachement = p.Attachment,
                                subject = p.Subject,
                                message = p.Message,
                                senton = p.SentOn,
                                Attachment = p.Attachmentpath,
                                Checkbox = p.CheckBox
                            }).OrderBy(x => x.senton).ToList();


                foreach (var item in objmodel)
                {
                    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    var uservalue = db.Users.Where(x => x.EmailAddress == item.From).Select(o => o).FirstOrDefault();
                    if (uservalue == null)
                    {
                        item.UserName = "";
                    }
                    else
                    {

                        item.UserName = uservalue.FirstName + " " + uservalue.LastName;
                    }
                    var uservalue1 = db.Users.Where(x => x.EmailAddress == item.to).Select(o => o).FirstOrDefault();
                    item.Fromusereid = uservalue1.UserID;
                    item.Touserid = uservalue1.UserID;
                }
                return View(objmodel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(objmodel);
        }


        public ActionResult UnreadEmailMessage(string Id)
        {
            int userid = 0;
            try
            {
                userid = Convert.ToInt32(Id);
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var EmailRead = db.ViewEmails.Where(x => x.Id == userid).Select(o => o).FirstOrDefault();
                if (EmailRead != null)
                {
                    EmailRead.CheckBox = false;
                    db.SaveChanges();
                }
                return Json(userid, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(userid, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string MailID)
        {
            int id = Convert.ToInt32(MailID);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var RemoveMail = db.ViewEmails.Where(x => x.Id == id).Select( x => x).FirstOrDefault();


            RemoveMail.IsActive = false;

            db.SaveChanges();

            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }


        public List<string> objccmail { get; set; }

        public List<string> objbccmail { get; set; }
    }
}








