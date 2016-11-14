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
    public class ManageLevelController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult ManageLevel()
        {
            List<DSRCManagementSystem.Models.ManageLevel> objmodel = new List<Models.ManageLevel>();
            try
            {
                List<ManageLevel> Value = new List<ManageLevel>();
                Value = (from a in db.ActivityLevels
                         where (a.IsActive == true)


                         select new ManageLevel()
                         {
                             LevelOrder = a.LevelOrder,
                             LevelId = a.ActivityLevelID,
                             LevelName = a.ActivityLevel1,
                             LevelDescription = a.LevelDescription


                         }).OrderBy(o => o.LevelOrder).ToList();
                foreach (var x in Value)
                {

                    objmodel.Add(x);
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
        [HttpPost]
        public ActionResult ManageLevel(ManageLevel model)
        {
            try
            {
                var LevelName = model.LevelName.Trim();
                var LevelDescription = model.LevelDescription.Trim();

                var temp = db.ActivityLevels.Where(r => r.ActivityLevel1 == LevelName && r.IsActive == true).Select(f => f.ActivityLevel1);

                foreach (var check in temp)
                {

                    if (check == LevelName)
                    {

                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }

                {

                    var Assignobj = db.ActivityLevels.CreateObject();
                    Assignobj.ActivityLevel1 = LevelName;
                    Assignobj.LevelDescription = LevelDescription;
                    db.ActivityLevels.AddObject(Assignobj);
                    db.SaveChanges();


                }
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
        public ActionResult EditLevel(int LevelId, string LevelName, string LevelDescription)
        {
            try
            {
                ViewBag.LevelId = LevelId;
                ViewBag.LevelName = LevelName;
                ViewBag.LevelDescription = LevelDescription;
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
        public ActionResult EditLevel(ManageLevel Model)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int LevelId = Model.LevelId;
                string LevelName = Model.LevelName.Trim();
                string LevelDescription = "";
                if (Model.LevelDescription != null)
                {
                    LevelDescription = Model.LevelDescription.Trim();
                }
                var data = db.ActivityLevels.Where(o => o.ActivityLevelID != LevelId && o.IsActive == true).Select(o => o.ActivityLevel1);

                foreach (var check in data)
                {
                    
                    if (check.ToLower() == LevelName.ToLower())
                    {

                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }
                var datas = db.ActivityLevels.Where(o => o.ActivityLevelID == LevelId).Select(o => o).FirstOrDefault();
                {
                    datas.ActivityLevel1 = LevelName;
                    datas.LevelDescription = LevelDescription;
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

        public ActionResult Delete(int Id)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var data = db.ActivityLevels.Where(o => o.ActivityLevelID == Id && o.IsActive == true).Select(o => o).FirstOrDefault();
                data.IsActive = false;
                db.SaveChanges();
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
        public ActionResult AddLevel()
        {


            return View();
        }


        [HttpPost]
        public ActionResult AddLevel(ManageLevel model)
        {
            try
            {
                var LevelName = model.LevelName.Trim();
                var LevelDescription = "";
                if (model.LevelDescription != null)
                {
                    LevelDescription = model.LevelDescription.Trim();
                }
                var temp = db.ActivityLevels.Where(r => r.ActivityLevel1 == LevelName && r.IsActive == true).Select(f => f.ActivityLevelID).FirstOrDefault();

                //foreach (var check in temp)
                //{

                if (temp != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }


                //}

                {
                    var count = db.ActivityLevels.Where(x => x.IsActive == true).Select(o => o).Count();
                    var Assignobj = db.ActivityLevels.CreateObject();
                    Assignobj.ActivityLevel1 = LevelName;
                    Assignobj.IsActive = true;
                    Assignobj.LevelDescription = LevelDescription;
                    Assignobj.LevelOrder = count + 1;
                    db.ActivityLevels.AddObject(Assignobj);
                    db.SaveChanges();
                }
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
        public ActionResult LevelOrder(string Ids)
        {
            var CurrentOrder = Ids.Split(',');
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var FunctionOrder = "";
            // List<int> List = new List<int>();

            var OrderIncrement = 1;
            foreach (var x in CurrentOrder)
            {
                FunctionOrder = rgx.Replace(x, "");
                var OrderID = Convert.ToInt32(FunctionOrder);
                var order = db.ActivityLevels.Where(a => a.ActivityLevelID == OrderID).Select(o => o).FirstOrDefault();
                order.LevelOrder = OrderIncrement;
                db.SaveChanges();
                OrderIncrement++;
                //List.Add(FunctionOrder);
            }



            return Json("Success", JsonRequestBehavior.AllowGet);
        }



    }
}
