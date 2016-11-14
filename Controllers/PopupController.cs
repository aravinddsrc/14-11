using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class PopupController : Controller
    {
        //
        // GET: /Popup/

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Faliure()
        {
            return View();
        }

        public ActionResult SuccessModalDialog(string modalTitle, string modalContent)
        {
            ViewBag.ModalTitle = modalTitle;
            ViewBag.ModalContent = modalContent;
            return View();
        }

        public ActionResult FailureModalDialog()
        {
            return View();
        }

        public ActionResult AlertPopUp(string modalContent)
        {
            ViewBag.ModalContent = modalContent;
            return View();
        }
    }
}
