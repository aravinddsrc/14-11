using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class CheckListController : Controller
    {
        //
        // GET: /CheckList/

        public ActionResult Index()
        {
            return View();
        }



        public ActionResult CheckList()
        {
            List<DSRCManagementSystem.Models.Category> Category = new List<DSRCManagementSystem.Models.Category>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                Category = (from u in db.Categories.Where(o => o.IsActive != false)
                            select new DSRCManagementSystem.Models.Category()
                            {
                                CategoryID = u.CategoryID,
                                CategoryName = u.CategoryName
                            }).OrderBy(x=> x.CategoryName).ToList();

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(Category);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Add(DSRCManagementSystem.Models.Category objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Category obj = new DSRCManagementSystem.Category();
            try
            {


                obj.CategoryName = objmodel.CategoryName.Trim();
                obj.IsActive = true;
                objdb.AddToCategories(obj);
                if (!objdb.Categories.Any(cobj => cobj.IsActive == true && cobj.CategoryName.Trim() == obj.CategoryName.Trim()))
                {
                    objdb.SaveChanges();
                }
                else
                {
                    return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
                

            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]

        public ActionResult EditCategory(int Id)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Category obj = new DSRCManagementSystem.Models.Category();
            try
            {


                var value = objdb.Categories.Where(x => x.CategoryID == Id).Select(o => o).FirstOrDefault();

                obj.CategoryName = value.CategoryName;

                Session["CategoryId"] = Id;
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
        public ActionResult EditCategory(DSRCManagementSystem.Models.Category objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var res= "";
            try
            {
                objmodel.CategoryID = Convert.ToInt32(Session["CategoryId"]);
                var value = objdb.Categories.Where(x => x.CategoryID == objmodel.CategoryID).Select(o => o).FirstOrDefault();
                
                
                if (!objdb.Categories.Any(cobj => cobj.IsActive==true && cobj.CategoryName.Trim() == objmodel.CategoryName.Trim()))
                {
                    value.CategoryName = objmodel.CategoryName;
                    objdb.SaveChanges();
                    res = "Success";
                }
                else
                {
                   // return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    res = "Already";
                }
               

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new { Result = res, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]

        public ActionResult Delete(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                var value = objdb.Categories.Where(x => x.CategoryID == Id).Select(o => o).FirstOrDefault();
               // var Expense = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o.ExpenseAmount).FirstOrDefault();
                if (value != null)
                {
                    value.IsActive = false;
                    objdb.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);


        }


        public ActionResult ViewCheckList(string Id)
        {

            int id = Convert.ToInt32(Id);
            Session["Viewcategory"] = Id;

            List<DSRCManagementSystem.Models.Checklist> Checklist = new List<DSRCManagementSystem.Models.Checklist>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                Checklist = (from u in db.Categories.Where(o => o.IsActive != false)
                             join p in db.CheckLists.Where(x => x.IsActive != false) on u.CategoryID equals p.CategoryID where u.CategoryID==id
                             select new DSRCManagementSystem.Models.Checklist()
                            {
                                CheckListName = p.CheckListName,
                                CheckListID= p.CheckListID
                            }).OrderBy(x=> x.CheckListName).ToList();


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(Checklist);
        }


        [HttpGet]
        public ActionResult AddCheckList()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddCheckList(DSRCManagementSystem.Models.Checklist objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            objmodel.CategoryID = Convert.ToInt32(Session["Viewcategory"]);
            var res = "";
            DSRCManagementSystem.CheckList obj = new DSRCManagementSystem.CheckList();
            try
            {
                obj.CheckListName = objmodel.CheckListName.Trim();
                obj.IsActive = true;
                obj.CategoryID = objmodel.CategoryID;
                objdb.AddToCheckLists(obj);
                if (!objdb.CheckLists.Any(cobj => cobj.IsActive==true && cobj.CheckListName.Trim() == objmodel.CheckListName.Trim()))
                {
                    objdb.SaveChanges();
                    res = "Success";
                }
                else
                {
                   // return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    res = "Already";
                }

            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return Json(new { Result = res, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]

        public ActionResult EditCheckList(int Id)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Checklist obj = new DSRCManagementSystem.Models.Checklist();
            try
            {


                var value = objdb.CheckLists.Where(x => x.CheckListID == Id).Select(o => o).FirstOrDefault();

                obj.CheckListName = value.CheckListName;

                Session["CategoryId"] = Id;
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
        public ActionResult EditCheckList(DSRCManagementSystem.Models.Checklist objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var res = "";
            try
            {
                objmodel.CheckListID = Convert.ToInt32(Session["CategoryId"]);
                var value =
                    objdb.CheckLists.Where(x => x.CheckListID == objmodel.CheckListID).Select(o => o).FirstOrDefault();

              
                if (!objdb.CheckLists.Any(cobj => cobj.IsActive == true && cobj.CheckListName.Trim() == objmodel.CheckListName.Trim()))
                {
                    value.CheckListName = objmodel.CheckListName;
                    objdb.SaveChanges();
                    res = "Success";
                }
                else
                {
                   // return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    res = "Already";
                }
               
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new { Result = res, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]

        public ActionResult DeleteCheckList(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {

                var value = objdb.CheckLists.Where(x => x.CheckListID == Id).Select(o => o).FirstOrDefault();
                // var Expense = objdb.Expenditures.Where(x => x.ExpenseID == Id).Select(o => o.ExpenseAmount).FirstOrDefault();
                if (value != null)
                {
                    value.IsActive = false;
                    objdb.SaveChanges();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);


        }

    }
}
