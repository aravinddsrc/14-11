using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem;
using System.Web.SessionState;
using System.Net;
using DSRCManagementSystem.Models;
using System.Web.Script.Serialization;
using DSRCManagementSystem.DSRCLogic;

namespace DSRCManagementSystem.Controllers
{
    public class OrgPrecedenceController : Controller
    {
        //
        // GET: /OrgPrecedence/

//        public ActionResult Index()
//        {
//            return View();
//        }

        public ActionResult OrgPrecedence()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            List<OrgPrecedence> Orgstructure = new List<OrgPrecedence>();
            try
            {
                ModelState.Clear();
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
               
                var result = from oc in db.OrgCharts
                             join u in db.Users on oc.UserID equals u.UserID
                             join d in db.Departments on u.DepartmentId equals d.DepartmentId
                             select new
                             {
                                 UserID = u.UserID,
                                 EmpID = u.EmpID,
                                 FirstName = u.FirstName,
                                 DepartmentID = d.DepartmentId,
                                 DepartmentName = d.DepartmentName,
                                 PrecedenceOrder = oc.PrecedenceOrder
                             };
                foreach (var item in result)
                {
                    Orgstructure.Add(new DSRCManagementSystem.Models.OrgPrecedence { EmpID = item.EmpID, UserID = item.UserID, FirstName = item.FirstName, DepartmentID = item.DepartmentID, DepartmentName = item.DepartmentName, PrecedenceOrder = item.PrecedenceOrder });
                }
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_OrgPrecedence", Orgstructure);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
                                   
            return View(Orgstructure);
        }

        public ActionResult CreatePrecedence()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var DepartmentList = db.Departments.ToList();
                //var UserName = db.Users.ToList().FirstOrDefault().ToString();
                var FirstDepartmentId = (from d in db.Departments
                                         select new
                                         {
                                             DepartmentID = d.DepartmentId

                                         }).FirstOrDefault();
                var UserName = (from u in db.Users
                                where u.DepartmentId == FirstDepartmentId.DepartmentID
                                select new
                                {
                                    UserID = u.UserID,
                                    FirstName = u.FirstName + "   " + "(" + u.EmpID + ")",
                                }).ToList();
                ViewBag.DepartmentIdList = new SelectList(new[] { new Department { DepartmentId = 0, DepartmentName = "---Select---" } }.Union(DepartmentList), "DepartmentId", "DepartmentName", 0);
                //ViewBag.FirstNameList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" }}.Union(UserName), "UserID", "FirstName", 0);
                ViewBag.FirstNameList = new SelectList(new[] { new { UserID = 0, FirstName = "---Select---" } }, "UserID", "FirstName", 0);
                //ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                //ViewBag.FirstNameList = new SelectList(UserName, "UserID", "FirstName");
                //var UserNameFilteration = (from u in db.Users
                //                           join d in db.Departments on u.DepartmentId equals d.DepartmentId
                //                           where u.DepartmentId == DepartmentList.First(i => Convert.ToInt32(i.DepartmentId))
                //                           select u);
                //ViewBag.FirstNameList = new SelectList(UserNameFilteration, "FirstName", "FirstName");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }
        
        public ActionResult DropDownFilter(int id = 0)
        {
               DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var DepartmentList = db.Departments.ToList();
                var UserNameFilteration = (from u in db.Users
                                           join d in db.Departments on u.DepartmentId equals d.DepartmentId
                                           where u.DepartmentId == id
                                           select u).Select(i => new
                                           {
                                               dataValue = i.UserID,
                                               dataText = i.FirstName + "   " + "(" + i.EmpID + ")",
                                           }).ToList();
                UserNameFilteration.Insert(0, new { dataValue = 0, dataText = "---Select---" });
                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                ViewBag.FirstNameList = new SelectList(UserNameFilteration, "UserID", "FirstName");
                var sr = new JavaScriptSerializer().Serialize(UserNameFilteration.ToList());
            return Json(sr, JsonRequestBehavior.AllowGet);            
        }

        [HttpPost]
        public ActionResult CreatePrecedence(OrgPrecedence OrgChart)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var DepartmentList = db.Departments.ToList();
                var UserNameFilteration = db.Users.ToList();
                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                ViewBag.FirstNameList = new SelectList(UserNameFilteration, "UserID", "FirstName");
                var NameAndPrecedenceCheck = db.OrgCharts.FirstOrDefault(x => x.UserID == OrgChart.UserID && x.PrecedenceOrder == OrgChart.PrecedenceOrder);
                var DepartmentPrecedenceCheck = db.OrgCharts.FirstOrDefault(x => x.DepartmentID == OrgChart.DepartmentID && x.PrecedenceOrder == OrgChart.PrecedenceOrder);
                var UserNameCheck = db.OrgCharts.FirstOrDefault(x => x.UserID == OrgChart.UserID);
                if (OrgChart.UserID != 0)
                {
                    if (UserNameCheck == null)
                    {
                        if (DepartmentPrecedenceCheck == null)
                        {
                            if (NameAndPrecedenceCheck == null)
                            {
                                if (ModelState.IsValid)
                                {
                                    var t = new OrgChart
                                    {
                                        UserID = OrgChart.UserID,
                                        DepartmentID = OrgChart.DepartmentID,
                                        PrecedenceOrder = OrgChart.PrecedenceOrder
                                    };
                                    db.OrgCharts.AddObject(t);
                                    db.SaveChanges();
                                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json("ExistingPrecedenceOrder", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("ExistingDepartmentPrecedenceCheck", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("UserNameCheck", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("EmployeeCheck", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }

        [HttpGet]
        public ActionResult DeletePrecedence(int Id = 0)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var DeleteOrgChartPrecedence = db.OrgCharts.FirstOrDefault(x => x.UserID == Id);
                if (Id != 0)
                {
                    if (DeleteOrgChartPrecedence != null)
                    {
                        db.OrgCharts.DeleteObject(DeleteOrgChartPrecedence);
                        db.SaveChanges();
                    }
                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }

        [HttpGet]
        public ActionResult EditPrecedence(int id = 0 )
        {
            
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.OrgPrecedence obj_OrgPrecedence = new DSRCManagementSystem.Models.OrgPrecedence();
                var DepartmentList = db.Departments.ToList();
                //var UserName = (from u in db.Users
                //                where u.DepartmentId == id
                //                select new
                //                {
                //                    UserId = u.UserID,
                //                    FirstName = u.FirstName,

                //                });
                //var UserNameFilteration = db.Users.ToList();
                ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                //ViewBag.FirstNameList = new SelectList(UserName, "UserID", "FirstName");  
                var OrgChartEdit = (from oc in db.OrgCharts
                                    join d in db.Departments on oc.DepartmentID equals d.DepartmentId
                                    join u in db.Users on oc.UserID equals u.UserID
                                    where oc.UserID == id
                                    select new OrgPrecedence
                                    {
                                        UserID = oc.UserID,
                                        DepartmentID = oc.DepartmentID,
                                        DepartmentName = d.DepartmentName,
                                        FirstName = u.FirstName,
                                        PrecedenceOrder = oc.PrecedenceOrder
                                    }).FirstOrDefault();
            return View(OrgChartEdit);
        }

        [HttpPost]
        public ActionResult EditPrecedence(OrgChart Org)
        {
            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var UpdatePrecedenceOrder = db.OrgCharts.FirstOrDefault(x => x.UserID == Org.UserID);            
            var PrecedenceOrderCheck = db.OrgCharts.FirstOrDefault(x => x.DepartmentID == Org.DepartmentID && x.PrecedenceOrder == Org.PrecedenceOrder);
            //var ExistingPrecedenceOrder = db.OrgCharts.FirstOrDefault(x => x.PrecedenceOrder == Org.PrecedenceOrder);
            if (PrecedenceOrderCheck  == null)
            {
                if (TryUpdateModel(UpdatePrecedenceOrder))
                {
                    db.SaveChanges();
                }
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Result = "PrecedenceOrderCheck", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }              
        }
    }
}
