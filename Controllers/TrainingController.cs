using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class TrainingController : Controller
    {
        //
        // GET: /Training/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Events()
        {
            return View();    
        }
        public ActionResult AddEvent()
        {
            return View();
        }


    }
}
