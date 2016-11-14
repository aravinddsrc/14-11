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

namespace DSRCManagementSystem.Controllers
{
    public class DepartmentController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        public ActionResult Department()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            List<DSRCManagementSystem.Models.Department> objmodel = new List<Models.Department>();
            try{
           
            List<DSRCManagementSystem.Models.Department> DepartmentValue = new List<DSRCManagementSystem.Models.Department>();
            DepartmentValue = (from p in db.Departments
                               join mb in db.Master_Branches on p.BranchID equals mb.BranchID
                               where (p.IsActive == true)
                               select new DSRCManagementSystem.Models.Department()
                               {
                                   BranchName=mb.BranchName,
                                   DepartmentID = p.DepartmentId,
                                   DepartmentName = p.DepartmentName,

                               }).OrderBy(o=>o.DepartmentName).ToList();
            foreach (var x in DepartmentValue)
            {

                objmodel.Add(x);
            }


            List<DSRCManagementSystem.Models.Department> GroupValue = new List<DSRCManagementSystem.Models.Department>();
            GroupValue = (from dg in db.DepartmentGroups
                          join dgm in db.DepartmentGroupMappings on dg.GroupID equals dgm.GroupID
                          join md in db.Departments on dgm.DepartmentID equals md.DepartmentId
                          where (dg.IsActive == true && md.IsActive == true)
                          select new DSRCManagementSystem.Models.Department()
                          {
                              GroupID=dg.GroupID,
                              DepartmentID=md.DepartmentId,
                              DPName = md.DepartmentName,
                              GroupName = dg.GroupName,

                          }).OrderBy(o => o.GroupName).ToList();
            foreach (var y in GroupValue)
            {

                objmodel.Add(y);
            }
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
        public ActionResult AddDepartment()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            //ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try{
            var BranchName = db.Master_Branches.Select(c => new
            {
                BranchId = c.BranchID,
                BranchName = c.BranchName
            }).ToList();
            ViewBag.BranchName = new SelectList(BranchName, "BranchId", "BranchName");
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
        public ActionResult AddDepartment(AddDepartment model)
        {
            Session["Tab"] = "";
            try{
            var userId = (int)Session["UserId"];
            var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();


            var Name = model.DepartmentName.Trim();
            var DepartmentName = db.Departments.Where(r => r.DepartmentName == Name && r.IsActive == true && r.BranchID==model.BranchID).Select(f => f.DepartmentName);
            foreach (var dpt in DepartmentName)
            {
                if (dpt != null)
                {

                   
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

            }

            {
                var Assignobj = db.Departments.CreateObject();
                Assignobj.DepartmentName = Name;
                Assignobj.IsActive = true;
                Assignobj.BranchID = model.BranchID;
                db.Departments.AddObject(Assignobj);
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success1", JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult EditDepartment(int ID, string DepartmentName)
        {
            
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            try{
            ViewBag.EditDepartment = DepartmentName.Replace("-","\"");
            ViewBag.ID = ID;

            var ids = db.Departments.Where(x => x.DepartmentId==ID && x.IsActive == true).FirstOrDefault();

            //int i = Convert.ToInt32(ids);

            //List<int> selected = new List<int>();
            //selected.Add(i);

            var BranchName = db.Master_Branches.Select(c => new
            {
                BranchId = c.BranchID,
                BranchName = c.BranchName
            }).ToList();
            ViewBag.BranchNames = new SelectList(BranchName, "BranchId", "BranchName",ids.BranchID);
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
        public ActionResult EditDepartment(AddDepartment model)
        {
            Session["Tab"] = "";
            try{
            int ID = model.ID;
            var Name = model.DepartmentName.Trim();
        

            var temp = db.Departments.Where(r => r.DepartmentId != ID && r.IsActive == true).Select(f => f.DepartmentName);

            foreach (var Edit in temp)
            {

                if (Edit == Name)
                {
                    ModelState.AddModelError("RoleName", "Department Name Already Exists");
                    @ViewBag.ID = ID;
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

            }

            {
                var x = db.Departments.Where(o => o.DepartmentId == ID).FirstOrDefault();
                x.DepartmentName = Name;
                x.BranchID = model.BranchID;
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            // Session["Tab"] = 1;

            return Json("Success1", JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public ActionResult AddGroup()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try{

            var Department = db.Departments.Where(o => o.IsActive == true).Select(c => new
            {
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentName
            }).OrderBy(o => o.DepartmentName).ToList();
            ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");
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
        public ActionResult AddGroup(AddDepartment model)
        {
            try{
            int ID = model.DepartmentId;
            var GroupName = model.GroupName.Trim();

            var CheckGroupId = db.DepartmentGroupMappings.Where(r => r.DepartmentID == ID ).Select(f => f.GroupID);

            foreach (var x in CheckGroupId)
            {

                var CheckGroupName = db.DepartmentGroups.Where(r => r.GroupID == x && r.IsActive==true).Select(f => f.GroupName);
                foreach (var y in CheckGroupName)
                {
                    if (y == GroupName)
                    {
                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            {
                var Assignobj = db.DepartmentGroups.CreateObject();
                Assignobj.GroupName = GroupName;
                Assignobj.IsActive = true;
                db.DepartmentGroups.AddObject(Assignobj);
                db.SaveChanges();

                
                int id = Assignobj.GroupID;
                var Assignobjt = db.DepartmentGroupMappings.CreateObject();
                Assignobjt.GroupID = id;
                Assignobjt.DepartmentID = ID;
                db.DepartmentGroupMappings.AddObject(Assignobjt);
                db.SaveChanges();
                Session["Tab"] = "one";

            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success1", JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public ActionResult EditGroup(int UIDD,int UID,string DPName, string GroupName)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try{

            var dptname = db.Departments.Where(x => x.DepartmentId == UIDD && x.IsActive == true).Select(o => o.DepartmentName).FirstOrDefault();
            ViewBag.dptname = dptname;
            ViewBag.EditGroup = GroupName;
            ViewBag.ID = DPName;
            ViewBag.UID = UID;
            ViewBag.UIDD = UIDD;
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
        public ActionResult EditGroup(AddDepartment model)
        {
            try{
            string GroupID = model.GroupID;
            int UIDD = model.UIDD;

            var Name = model.GroupName.Trim();

            var CheckGroupId = db.DepartmentGroupMappings.Where(r => r.DepartmentID == UIDD).Select(f => f.GroupID);

            foreach (var x in CheckGroupId)
            {
                if (x != model.UID)
                {
                    var CheckGroupName = db.DepartmentGroups.Where(r => r.GroupID == x && r.IsActive == true).Select(f => f.GroupName).FirstOrDefault();

                    if (CheckGroupName == Name)
                    {

                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }
                
            }
            {
                var x = db.DepartmentGroups.Where(r => r.GroupName == GroupID).FirstOrDefault();
                x.GroupName = Name;
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            Session["Tab"] = "one";
            return Json("Success1", JsonRequestBehavior.AllowGet);

        }

        public ActionResult Delete(int ID)
        {
            try{
            var CheckDept = db.Users.Where(o => o.DepartmentId == ID && o.IsActive == true).Select(o => o.UserID);
            foreach (int x in CheckDept)
            {
                if (x != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
            }
            var data = db.Departments.Where(o => o.DepartmentId == ID).Select(o => o).FirstOrDefault();
            data.IsActive = false;
            db.SaveChanges();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            Session["Tab"] = "";
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteGroup(int DPName)
        {
            try{
            var Check = db.Users.Where(o => o.DepartmentGroup == DPName && o.IsActive == true).Select(o => o.UserID);
            foreach (int x in Check)
            {
                if (x != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
            }
            {
                var data = db.DepartmentGroups.Where(o => o.GroupID == DPName).Select(o => o).FirstOrDefault();
                data.IsActive = false;
                db.SaveChanges();
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            Session["Tab"] = "one";
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
    }
}
