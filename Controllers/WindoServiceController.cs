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

namespace DSRCManagementSystem.Controllers
{
    public class WindoServiceController : Controller
    {
        [HttpGet]
        public ActionResult Addschedule()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Addnewschedule()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                Windowservice model = new Windowservice();
                model.StartDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                model.EndDate = DateTime.Today.AddYears(5).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                var Mails = (from t in objdb.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 Id = t.UserID,
                                 UserName = t.UserName,

                             }).ToList();

                var schedules = (from p in objdb.Master_WindowServiceSchedules
                                 select new
                                 {
                                     ScheduleId = p.Id,
                                     ScheduleName = p.Schedules
                                 }).ToList();
                var services = (from p in objdb.Master_Windowsservices
                                select new
                                {
                                    Id = p.Id,
                                    Services = p.WindowServices
                                }).ToList();

                ViewBag.MailTO = new MultiSelectList(Mails, "Id", "UserName");
                ViewBag.Schedules = new SelectList(new List<string> { "Daily", "Weekly", "Monthly", "Instant" });
                ViewBag.services = new SelectList(services, "Id", "Services");
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
        public ActionResult Addnewschedule(Windowservice service)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                //Windowservice windows = new Windowservice();
                //windows.ServiceName = service.ServiceName;
                //windows.ServiceType = service.ServiceType;
                //windows.StartDate = service.StartDate;
                //windows.EndDate = service.EndDate;
                //windows.MailTO = service.MailTO;
                //db.Windowservicecontrols.AddObject(windows);
                //db.SaveChanges();
            }
            return View();
        }
























        [HttpGet]
        public ActionResult Editschedule()
        {
            //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //var Mails = (from t in objdb.Users.Where(x => x.IsActive == true)
            //             select new
            //             {
            //                 Id = t.UserID,
            //                 UserName = t.UserName,

            //             }).ToList();

            //var schedules = (from p in objdb.WindowServiceSchedules

            //                 select new
            //                 {
            //                     ScheduleId = p.Id,
            //                     ScheduleName = p.Schedules
            //                 }).ToList();
            //var services = (from p in objdb.Windowsservices
            //                select new
            //                {
            //                    Id = p.Id,
            //                    Services = p.WindowServices
            //                }).ToList();

            //ViewBag.MailTO = new MultiSelectList(Mails, "Id", "UserName");
            //ViewBag.Schedules = new SelectList(schedules, "ScheduleId", "ScheduleName");
            //ViewBag.services = new SelectList(services, "Id", "Services");


            return View();

        }
        [HttpGet]
        public ActionResult Deleteschedule()
        {
            //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            //var ProjectLead = (from t in objdb.Users.Where(x => x.IsActive == true)
            //                   select new
            //                   {
            //                       Id3 = t.UserID,
            //                       // Name=t.FirstName+t.LastName,
            //                       FirstName = t.FirstName,
            //                       LastName = t.LastName,


            //                   }).ToList();

            //ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id3", "FirstName", "LastName");
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);


            //return View();

        }

        [HttpGet]
        public ActionResult controlschedule()
        {
            //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            //var Mails = (from t in objdb.Users.Where(x => x.IsActive == true)
            //             select new
            //             {
            //                 Id = t.UserID,
            //                 UserName = t.UserName,

            //             }).ToList();

            //var schedules = (from p in objdb.WindowServiceSchedules
            //                 select new
            //                 {
            //                     ScheduleId = p.Id,
            //                     ScheduleName = p.Schedules
            //                 }).ToList();
            //var services = (from p in objdb.Windowsservices
            //                select new
            //                {
            //                    Id = p.Id,
            //                    Services = p.WindowServices
            //                }).ToList();

            //ViewBag.MailTO = new MultiSelectList(Mails, "Id", "UserName");
            //ViewBag.Schedules = new SelectList(schedules, "ScheduleId", "ScheduleName");
            //ViewBag.services = new SelectList(services, "Id", "Services");

            return View();
        }

        [HttpGet]
        public ActionResult Addcontrolschedule()
        {
            //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //var Mails = (from t in objdb.Users.Where(x => x.IsActive == true)
            //             select new
            //             {
            //                 Id = t.UserID,
            //                 UserName = t.UserName,

            //             }).ToList();

            //var schedules = (from p in objdb.WindowServiceSchedules

            //                 select new
            //                 {
            //                     ScheduleId = p.Id,
            //                     ScheduleName = p.Schedules
            //                 }).ToList();
            //var services = (from p in objdb.Windowsservices
            //                select new
            //                {
            //                    Id = p.Id,
            //                    Services = p.WindowServices
            //                }).ToList();

            //ViewBag.MailTO = new MultiSelectList(Mails, "Id", "UserName");
            //ViewBag.Schedules = new SelectList(schedules, "ScheduleId", "ScheduleName");
            //ViewBag.services = new SelectList(services, "Id", "Services");


            return View();

        }

        [HttpGet]
        public ActionResult Editcontrolschedule()
        {
            //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //var Mails = (from t in objdb.Users.Where(x => x.IsActive == true)
            //             select new
            //             {
            //                 Id = t.UserID,
            //                 UserName = t.UserName,

            //             }).ToList();

            //var schedules = (from p in objdb.WindowServiceSchedules

            //                 select new
            //                 {
            //                     ScheduleId = p.Id,
            //                     ScheduleName = p.Schedules
            //                 }).ToList();
            //var services = (from p in objdb.Windowsservices
            //                select new
            //                {
            //                    Id = p.Id,
            //                    Services = p.WindowServices
            //                }).ToList();

            //ViewBag.MailTO = new MultiSelectList(Mails, "Id", "UserName");
            //ViewBag.Schedules = new SelectList(schedules, "ScheduleId", "ScheduleName");
            //ViewBag.services = new SelectList(services, "Id", "Services");


            return View();

        }
       
    }
}
