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
    public class LabelController : Controller
    {
        //
        // GET: /Label/
        //public ActionResult Index()
        //{
        //    return View();
        //}

        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        public ActionResult ViewLabels()
        {
            //List<DSRCManagementSystem.Models.Label> objmodel = new List<Models.Label>();
            List<DSRCManagementSystem.Models.Label> LabelValue = new List<DSRCManagementSystem.Models.Label>();
            try
            {


                LabelValue = (from p in db.Dictionaries
                              select new DSRCManagementSystem.Models.Label()
                              {
                                  id = p.Id,
                                  LabelName = p.Name,
                                  PreviousName = p.Previous_Name,

                              }).OrderBy(o => o.LabelName).ToList();
                //foreach (var x in LabelValue)
                //{

                //    objmodel.Add(x);
                //}

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(LabelValue);
        }
        [HttpGet]
        public ActionResult AddLabel()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddLabel(Label model)
        {
            try
            {
                var userId = (int)Session["UserId"];
                var BranchId = db.Users.Where(r => r.UserID == userId).Select(f => f.BranchId).FirstOrDefault();


                var Name = model.LabelName.Trim();
                var LabelName = db.Dictionaries.Where(r => r.Name == Name && r.IsActive == true).Select(f => f.Name);
                foreach (var dpt in LabelName)
                {
                    if (dpt != null)
                    {


                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }

                {
                    var Assignobj = db.Dictionaries.CreateObject();
                    Assignobj.Name = Name;
                    Assignobj.IsActive = true;
                    db.Dictionaries.AddObject(Assignobj);
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
        public ActionResult EditLabel(int ID, string LabelName)
        {

            try
            {
                ViewBag.EditLabel = LabelName;
                ViewBag.ID = ID;

                var ids = db.Dictionaries.Where(x => x.Id == ID && x.IsActive == true).FirstOrDefault();

                //int i = Convert.ToInt32(ids);

                //List<int> selected = new List<int>();
                //selected.Add(i);


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
        public ActionResult EditLabel(Label model)
        {

            try
            {
                int ID = model.id;
                var Name = model.LabelName.Trim();


                var temp = db.Dictionaries.Where(r => r.Id != ID && r.IsActive == true).Select(f => f.Name);

                foreach (var Edit in temp)
                {

                    if (Edit == Name)
                    {
                        ModelState.AddModelError("RoleName", "Label Name Already Exists");
                        @ViewBag.ID = ID;
                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }

                {
                    var x = db.Dictionaries.Where(o => o.Id == ID).FirstOrDefault();
                    if (x.Name != Name)
                    {

                        x.Previous_Name = x.Name;
                        x.Name = Name;
                        db.SaveChanges();
                    }
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

    }
}
