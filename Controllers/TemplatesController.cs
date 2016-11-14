using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class TemplatesController : Controller
    {
        //
        // GET: /Templates/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EmailTemplates()
        {


            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            List<DSRCManagementSystem.Models.EmailTemplateModules> obj = new List<Models.EmailTemplateModules>();

            obj = (from p in objdb.EmailTemplates.Where(x => x.IsActive == true)
                   select new DSRCManagementSystem.Models.EmailTemplateModules
                   {

                       Id = p.EmailTemplateID,
                       EmailTemplates = p.TemplatePurpose

                   }).ToList();

            return View(obj);

        }


        [HttpGet]
        public ActionResult CKEditor(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.AgandaForProject objmodel = new DSRCManagementSystem.Models.AgandaForProject();
            TempData["EmailTemplateId"] = Id;
            var Template = objdb.EmailTemplates.Where(x => x.EmailTemplateID == Id && x.IsActive == true).Select(o => o.TemplatePath).FirstOrDefault();
            var html =System.IO.File.ReadAllText(Server.MapPath(Template));
            objmodel.ProjectAganda = html.ToString();
            objmodel.id = Id;
            return View(objmodel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CKEditor(string commentText,string Tid)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int TemplateId = Convert.ToInt32(Tid);
            var alreadytemplate = objdb.EmailTemplates.Where(x => x.EmailTemplateID == TemplateId && x.IsActive == true).Select(o => o.TemplatePath).FirstOrDefault();
            //if (alreadytemplate != null || alreadytemplate == null)
            //{
            //    alreadytemplate = commentText.ToString();
            //    objdb.SaveChanges();
            //}

            string text = System.IO.File.ReadAllText(Server.MapPath(alreadytemplate));
            text = text.Replace(text, commentText);
            System.IO.File.WriteAllText(Server.MapPath(alreadytemplate), text);

            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }
    }
}
