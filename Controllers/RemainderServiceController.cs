//using DSRCManagementSystem.Models;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using DSRCHRMSRemainderService;
//using System.Configuration;


//namespace DSRCManagementSystem.Controllers
//{
//    public class RemainderServiceController : Controller
//    {
//        //
//        // GET: /RemainderService/

//       // string dateInString = ConfigurationManager.AppSettings["EndDate"];

//        string dateInString = "31-03-2017";

//        public ActionResult Index()
//        {
//            return View();
//        }

//        [HttpGet]
//        public ActionResult AddNewRemainder()
//        {
//            DSRCManagementSystemEntities1 servicedb = new DSRCManagementSystemEntities1();
//            try
//            {
//                var ServiceType = (from pi in servicedb.Master_RemainderServiceType.Where(x => x.IsActive == true)
//                                   select new
//                                   {
//                                       Id1 = pi.RemainderServiceID,
//                                       Type = pi.ServiceType
//                                   }).ToList();

//                ViewBag.ServiceType = new SelectList(ServiceType, "Id1", "Type");



//                var GetAllMethod = typeof(DSRCHRMSRemainderService.RemainderMethods);


//                List<SelectListItem> EventCall = new List<SelectListItem>();
//                int i = 1;

//                foreach (var m in GetAllMethod.GetMethods())
//                {
//                    if (m.ToString().Contains("Void"))
//                    {
//                        EventCall.Add(new SelectListItem { Value = i.ToString(), Text = m.ToString() });
//                        i++;
//                        //ViewBag.Event = m;
//                    }
//                    else { }
//                }
//                //}
//                ViewBag.Event = new SelectList(EventCall, "Value", "Text");


//                List<SelectListItem> HoursBind = new List<SelectListItem>();

//                HoursBind.Add(new SelectListItem { Value = "1", Text = "1" });

//                ViewBag.Hours = new SelectList(HoursBind, "Value", "Text");

//                List<SelectListItem> AddDays = new List<SelectListItem>();

//                AddDays.Add(new SelectListItem { Value = "1", Text = "Mon", Selected = true });
//                AddDays.Add(new SelectListItem { Value = "2", Text = "Tue", Selected = true });
//                AddDays.Add(new SelectListItem { Value = "3", Text = "Wed", Selected = true });
//                AddDays.Add(new SelectListItem { Value = "4", Text = "Thu", Selected = true });
//                AddDays.Add(new SelectListItem { Value = "5", Text = "Fri", Selected = true });


//                var IdVal = "";
//                foreach (var getval in AddDays)
//                {
//                        IdVal += getval.Value;
                    
//                }


//              //  ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text", IdVal); 

//                ViewBag.WeekDays = AddDays;

//                RemainderService remainder = new RemainderService();
//               remainder.DaysCheck=AddDays;


//                var EmailList = (from p in servicedb.Users.Where(x => x.IsActive == true)
//                                 select new
//                                 {
//                                     Id = p.UserID,
//                                     UserName = p.UserName
//                                 }).ToList();
//                var EmailList1 = (from p in servicedb.Users.Where(x => x.IsActive == true)
//                                  select new
//                                  {
//                                      Id1 = p.UserID,
//                                      UserName = p.UserName
//                                  }).ToList();

//                var EmailList2 = (from p in servicedb.Users.Where(x => x.IsActive == true)
//                                  select new
//                                  {
//                                      Id2 = p.UserID,
//                                      UserName = p.UserName
//                                  }).ToList();

//                ViewBag.Email = new MultiSelectList(EmailList, "Id", "UserName");
//                ViewBag.Email1 = new MultiSelectList(EmailList1, "Id1", "UserName");
//                ViewBag.Email2 = new MultiSelectList(EmailList1, "Id2", "UserName");
//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }
//            return View();
//        }


//        [HttpPost]
//        public ActionResult AddNewRemainder(RemainderPurpose remainderservice)
//        {
//            int userId = Convert.ToInt32(Session["UserID"]);

//            var ShowResult = "";

//            try
//            {
//                DSRCManagementSystem.RemainderServiceDetail objremainder = new DSRCManagementSystem.RemainderServiceDetail();

//                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

//                DSRCManagementSystemEntities1 objdbData = new DSRCManagementSystemEntities1();

//                var Servicetypecheck = remainderservice.ServiceType;

//                var getserviceId = objdb.Master_RemainderServiceType.Where(x => x.ServiceType == remainderservice.ServiceType && x.IsActive == true).Select(c => c.RemainderServiceID).FirstOrDefault();
//                //   DateTime validateDate = DateTime.ParseExact("24/01/2013", "dd/MM/yyyy", CultureInfo.InvariantCulture);

//                DateTime Moddate = Convert.ToDateTime("01-01-1900");

//                if (Servicetypecheck == "Hourly Service")
//                {

//                    objremainder.ServiceName = remainderservice.ServiceName;
//                    objremainder.RemainderServiceTypeID = getserviceId;
//                    objremainder.HoursCheck = remainderservice.ServiceHour;
//                    objremainder.SelectedDays = remainderservice.Days;
//                    objremainder.StartTime = remainderservice.ServiceStartTime;
//                    objremainder.IsActive = true;
//                    objremainder.IsOnService = true;
//                    objremainder.CreatedBy = userId;
//                    objremainder.CreatedDate = DateTime.Now;
//                    objremainder.ModBy = 0;
//                    objremainder.ModDate = Moddate;
//                    objremainder.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.RemainderServiceDetails.AddObject(objremainder);
//                    objdb.SaveChanges();

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);

//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = GetServiceID;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(1);
//                    }
//                    ShowResult = "Success";
//                }
//                else if (Servicetypecheck == "Daily Service")
//                {
//                    objremainder.ServiceName = remainderservice.ServiceName;
//                    objremainder.RemainderServiceTypeID = getserviceId;
//                    objremainder.HoursCheck = remainderservice.ServiceHour;
//                    objremainder.SelectedDays = remainderservice.Days;
//                    objremainder.StartTime = remainderservice.ServiceStartTime;
//                    objremainder.IsActive = true;
//                    objremainder.IsOnService = true;
//                    objremainder.CreatedBy = userId;
//                    objremainder.CreatedDate = DateTime.Now;
//                    objremainder.ModBy = 0;
//                    objremainder.ModDate = Moddate;
//                    objremainder.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.RemainderServiceDetails.AddObject(objremainder);
//                    objdb.SaveChanges();

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);
//                    while (startDate <= EndDate)
//                    {

//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = GetServiceID;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(1);
//                    }
//                    ShowResult = "Success";
//                }
//                else if (Servicetypecheck == "Weekly Service")
//                {

//                    objremainder.ServiceName = remainderservice.ServiceName;
//                    objremainder.RemainderServiceTypeID = getserviceId;
//                    objremainder.HoursCheck = remainderservice.ServiceHour;
//                    objremainder.SelectedDays = remainderservice.Days;
//                    objremainder.StartTime = remainderservice.ServiceStartTime;
//                    objremainder.IsActive = true;
//                    objremainder.IsOnService = true;
//                    objremainder.CreatedBy = userId;
//                    objremainder.CreatedDate = DateTime.Now;
//                    objremainder.ModBy = 0;
//                    objremainder.ModDate = Moddate;
//                    objremainder.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.RemainderServiceDetails.AddObject(objremainder);
//                    objdb.SaveChanges();

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);
//                    while (startDate <= EndDate)
//                    {

//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = GetServiceID;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(7);
//                    }
//                    ShowResult = "Success";
//                }
//                else if (Servicetypecheck == "Monthly Twise Service")
//                {

//                    objremainder.ServiceName = remainderservice.ServiceName;
//                    objremainder.RemainderServiceTypeID = getserviceId;
//                    objremainder.HoursCheck = remainderservice.ServiceHour;
//                    objremainder.SelectedDays = remainderservice.Days;
//                    objremainder.StartTime = remainderservice.ServiceStartTime;
//                    objremainder.IsActive = true;
//                    objremainder.IsOnService = true;
//                    objremainder.CreatedBy = userId;
//                    objremainder.CreatedDate = DateTime.Now;
//                    objremainder.ModBy = 0;
//                    objremainder.ModDate = Moddate;
//                    objremainder.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.RemainderServiceDetails.AddObject(objremainder);
//                    objdb.SaveChanges();

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);
//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = GetServiceID;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(15);
//                    }
//                    ShowResult = "Success";
//                }
//                else if (Servicetypecheck == "Monthly Service")
//                {
//                    objremainder.ServiceName = remainderservice.ServiceName;
//                    objremainder.RemainderServiceTypeID = getserviceId;
//                    objremainder.HoursCheck = remainderservice.ServiceHour;
//                    objremainder.SelectedDays = remainderservice.Days;
//                    objremainder.StartTime = remainderservice.ServiceStartTime;
//                    objremainder.IsActive = true;
//                    objremainder.IsOnService = true;
//                    objremainder.CreatedBy = userId;
//                    objremainder.CreatedDate = DateTime.Now;
//                    objremainder.ModBy = 0;
//                    objremainder.ModDate = Moddate;
//                    objremainder.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.RemainderServiceDetails.AddObject(objremainder);
//                    objdb.SaveChanges();

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);
//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = GetServiceID;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(30);
//                    }
//                    ShowResult = "Success";
//                }

//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }

//            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

//        }

//        [HttpGet]
//        public ActionResult RemainderSetup()
//        {
//            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
//            List<DSRCManagementSystem.Models.RemainderPurpose> objTab = new List<Models.RemainderPurpose>();
//            List<DSRCManagementSystem.Models.RemainderPurpose> DataDetails = new List<Models.RemainderPurpose>();
//            //List<DSRCManagementSystem.Models.RemainderPurpose> objTab1 = new List<Models.RemainderPurpose>();
//            try
//            {
//                RemainderPurpose remainder = new RemainderPurpose();

               

//                var rowcount = _db.RemainderServiceDetails.Where(x => x.IsActive == true).Count();

//             //   var dateget = _db.ServiceDataDetails.Where(x => x.ServiceDate > DateTime.Now).Select(m => m.ServiceDate).Take(1).FirstOrDefault();
//                var status = string.Empty;
//                objTab = (from r in objdb.RemainderServiceDetails.Where(x => x.IsActive == true)
//                          join m in objdb.Master_RemainderServiceType on r.RemainderServiceTypeID equals m.RemainderServiceID
//                          join s in objdb.ServiceDataDetails.Where(a => a.ServiceStatus == status && a.ServiceDate >= DateTime.Now ) on r.ServiceID equals s.RemainderServiceID into g
//                         // where s.ServiceStatus == status && DateTime.Now >= s.ServiceDate 
                         
//                          select new DSRCManagementSystem.Models.RemainderPurpose
//                          {
//                              Id = r.ServiceID,
//                              ServiceName = r.ServiceName,
//                              ServiceType = m.ServiceType,
//                              //SelectDate = (DateTime)s.ServiceDate,
//                              SelectDate = (DateTime)g.Min(s => s.ServiceDate),
//                              Days = r.SelectedDays,
//                              checkMonday = true,
//                              checkTuesday = true,
//                              checkWednesday = true,
//                              checkThursday = true,
//                              checkFriday = true,
//                              checkSaturday = true,
//                              checkSunday = true,
//                              OnServiceStatus = (bool)r.IsOnService,
//                              ServiceStartTime = r.StartTime
//                              // CheckHistory = false
//                          }).ToList();



//                var ServiceType = (from pi in objdb.Master_RemainderServiceType.Where(x => x.IsActive == true)
//                                   select new
//                                   {
//                                       Id3 = pi.RemainderServiceID,
//                                       Service = pi.ServiceType
//                                   }).ToList();

//                ViewBag.ServiceType = new SelectList(ServiceType, "Id3", "Service");
                
//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

//            }
//            return View(objTab);

//        }

//        [HttpPost]
//        public ActionResult RemainderSetup(FormCollection form)
//        {
//            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
//            List<DSRCManagementSystem.Models.RemainderPurpose> objTab = new List<Models.RemainderPurpose>();

//            try
//            {
//                var ServiceTypeId = (form["ServiceType"] == "") ? 0 : Convert.ToInt16(form["ServiceType"]);

//                var dateget = _db.ServiceDataDetails.Where(x => x.ServiceDate > DateTime.Now).Select(m => m.ServiceDate).Take(1).FirstOrDefault();
//                var status=string.Empty;

//                if (ServiceTypeId == 0)
//                {
//                    objTab = (from r in objdb.RemainderServiceDetails.Where(x => x.IsActive == true)
//                              join m in objdb.Master_RemainderServiceType on r.RemainderServiceTypeID equals m.RemainderServiceID
//                              join s in objdb.ServiceDataDetails.Where(a => a.ServiceStatus == status && a.ServiceDate >= DateTime.Now) on r.ServiceID equals s.RemainderServiceID into g
//                              // where s.ServiceStatus == status && DateTime.Now >= s.ServiceDate 

//                              select new DSRCManagementSystem.Models.RemainderPurpose
//                              {
//                                  Id = r.ServiceID,
//                                  ServiceName = r.ServiceName,
//                                  ServiceType = m.ServiceType,
//                                  //SelectDate = (DateTime)s.ServiceDate,
//                                  SelectDate = (DateTime)g.Min(s => s.ServiceDate),
//                                  Days = r.SelectedDays,
//                                  checkMonday = true,
//                                  checkTuesday = true,
//                                  checkWednesday = true,
//                                  checkThursday = true,
//                                  checkFriday = true,
//                                  checkSaturday = true,
//                                  checkSunday = true,
//                                  OnServiceStatus = (bool)r.IsOnService,
//                                  ServiceStartTime = r.StartTime
//                                  // CheckHistory = false
//                              }).ToList();

//                }
//                else
//                {
//                    objTab = (from r in objdb.RemainderServiceDetails.Where(m => m.RemainderServiceTypeID == ServiceTypeId)
//                              join m in objdb.Master_RemainderServiceType on r.RemainderServiceTypeID equals m.RemainderServiceID
//                              where (r.IsActive == true)
//                              join s in objdb.ServiceDataDetails.Where(m => m.ServiceStatus == status && m.ServiceDate >= DateTime.Now) on r.ServiceID equals s.RemainderServiceID into g
//                              // where dateget == s.ServiceDate
//                              select new DSRCManagementSystem.Models.RemainderPurpose
//                              {
//                                  Id = r.ServiceID,
//                                  ServiceName = r.ServiceName,
//                                  ServiceType = m.ServiceType,
//                                  // SelectDate = (DateTime)s.ServiceDate,
//                                  SelectDate = (DateTime)g.Min(s => s.ServiceDate),
//                                  Days = r.SelectedDays,
//                                  checkMonday = true,
//                                  checkTuesday = true,
//                                  checkWednesday = true,
//                                  checkThursday = true,
//                                  checkFriday = true,
//                                  checkSaturday = true,
//                                  checkSunday = true,
//                                  OnServiceStatus = (bool)r.IsOnService,
//                                  ServiceStartTime = r.StartTime
//                              }).ToList();
//                }

//                var Service = (from pi in objdb.Master_RemainderServiceType.Where(x => x.IsActive == true)
//                               select new
//                               {
//                                   Id3 = pi.RemainderServiceID,
//                                   SType = pi.ServiceType
//                               }).ToList();

//                ViewBag.ServiceTypeData = new SelectList(Service, "Id3", "SType", ServiceTypeId);
//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }

//            return View(objTab);
//        }


//        public static int IdData;
//        [HttpGet]
//        public ActionResult AddNewEdit(int Id, string ServiceType1, string SelectedDate1)
//        {
//            DSRCManagementSystemEntities1 servicedb = new DSRCManagementSystemEntities1();
//            try
//            {
//                var getID = servicedb.Master_RemainderServiceType.Where(m => m.ServiceType == ServiceType1).Select(m => m.RemainderServiceID).FirstOrDefault();

//                var ServiceType = (from pi in servicedb.Master_RemainderServiceType.Where(x => x.IsActive == true)
//                                   select new
//                                   {
//                                       Id3 = pi.RemainderServiceID,
//                                       SType = pi.ServiceType
//                                   }).ToList();
//                IdData = Id;

//                ViewBag.ServiceTypeData = new SelectList(ServiceType, "Id3", "SType", getID);

//                DSRCManagementSystem.Models.RemainderPurpose obj = new DSRCManagementSystem.Models.RemainderPurpose();


//                var getserviceId = servicedb.RemainderServiceDetails.Where(x => x.ServiceID == Id && x.IsActive == true).Select(c => c.ServiceID).FirstOrDefault();
//                var hour = "";
//                var date1 = "";
//                var Time = "";

//                obj = (from p in servicedb.RemainderServiceDetails.Where(x => x.ServiceID == getserviceId)
//                       select new DSRCManagementSystem.Models.RemainderPurpose
//                       {
//                           Id = p.ServiceID,
//                           ServiceType = ServiceType1,
//                           ServiceName = p.ServiceName,
//                           ServiceHour = (Int32)p.HoursCheck,
//                           Days = p.SelectedDays,
//                           ServiceStartTime = p.StartTime,
//                           EventMethodName = p.ServiceMethod
//                       }).FirstOrDefault();


//                hour = obj.ServiceHour.ToString();
//                date1 = SelectedDate1;
//                Time = obj.ServiceStartTime.ToString();
//                var date = date1.Remove(date1.Length - 9);
//                var selectdays = obj.Days;
//                var servicename = obj.ServiceName;
//                var method = obj.EventMethodName;

                

//                List<SelectListItem> list = new List<SelectListItem>();

//               // list.Add(new SelectListItem { Value = "1", Text = "Sun", Selected = true });
//                list.Add(new SelectListItem { Value = "1", Text = "Mon", Selected = true });
//                list.Add(new SelectListItem { Value = "2", Text = "Tue", Selected = true });
//                list.Add(new SelectListItem { Value = "3", Text = "Wed", Selected = true });
//                list.Add(new SelectListItem { Value = "4", Text = "Thu", Selected = true });
//                list.Add(new SelectListItem { Value = "5", Text = "Fri", Selected = true });
//              //  list.Add(new SelectListItem { Value = "7", Text = "Sat", Selected = true });


//                List<SelectListItem> HoursBind = new List<SelectListItem>();

//                HoursBind.Add(new SelectListItem { Value = "1", Text = "1" });
//                //HoursBind.Add(new SelectListItem { Value = "2", Text = "2" });
//                //HoursBind.Add(new SelectListItem { Value = "3", Text = "3" });
//                //HoursBind.Add(new SelectListItem { Value = "4", Text = "4" });
//                //HoursBind.Add(new SelectListItem { Value = "5", Text = "5" });
//                //HoursBind.Add(new SelectListItem { Value = "6", Text = "6" });
//                //HoursBind.Add(new SelectListItem { Value = "7", Text = "7" });
//                //HoursBind.Add(new SelectListItem { Value = "8", Text = "8" });
//                //HoursBind.Add(new SelectListItem { Value = "9", Text = "9" });
//                //HoursBind.Add(new SelectListItem { Value = "10", Text = "10" });
//                //HoursBind.Add(new SelectListItem { Value = "11", Text = "11" });
//                //HoursBind.Add(new SelectListItem { Value = "12", Text = "12" });



//                var GetAllMethod = typeof(DSRCHRMSRemainderService.RemainderMethods);

//                List<SelectListItem> EventCall = new List<SelectListItem>();
//                int i = 1;
//                foreach (var m in GetAllMethod.GetMethods())
//                {
//                    EventCall.Add(new SelectListItem { Value = i.ToString(), Text = m.ToString() });
//                    i++;
//                    //ViewBag.Event = m;
//                }
//                var IdVal = "";
//                foreach (var getval in EventCall)
//                {
//                    if (method == getval.Text)
//                    {
//                        IdVal = getval.Value;
//                    }
//                }

//                ViewBag.Event = new SelectList(EventCall, "Value", "Text", IdVal);


//                if (ServiceType1 == "Hourly Service")
//                {
//                    var IdGet1 = "";
//                    foreach (var val1 in list)
//                    {
//                        if (selectdays.Contains(val1.Text))
//                        {
//                            IdGet1 += val1.Value;
//                        }
//                    }

//                   // ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text", IdGet1);
//                    ViewBag.WeekDays = selectdays;
//                    var IdGet = "";
//                    foreach (var val in HoursBind)
//                    {
//                        if (hour.Contains(val.Text))
//                        {
//                            IdGet = val.Value;
//                            ViewBag.Hours = new SelectList(HoursBind, "Value", "Text", IdGet);
//                        }
//                    }
//                    ViewBag.PicDate = date;
//                    ViewBag.PicTime = Time;
//                    ViewBag.PicServiceName = servicename;

//                    TempData["ValidDays"] = selectdays;
//                    ViewBag.ValidDays = selectdays;

//                }
//                else if (ServiceType1 == "Daily Service")
//                {
//                    var IdGet1 = "";
//                    foreach (var val1 in list)
//                    {
//                        if (selectdays.Contains(val1.Text))
//                        {
//                            IdGet1 += val1.Value;
//                        }
//                    }
//                   // ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text", IdGet1);
//                    ViewBag.WeekDays = selectdays;
//                    ViewBag.PicDate = date;
//                    ViewBag.PicTime = Time;
//                    ViewBag.Hours = new SelectList(HoursBind, "Value", "Text");
//                    ViewBag.PicServiceName = servicename;

//                    TempData["ValidDays"] = selectdays;
//                    ViewBag.ValidDays = selectdays;
//                }
//                else if (ServiceType1 == "Weekly Service")
//                {
//                   // ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text");
//                    ViewBag.WeekDays = selectdays;
//                    ViewBag.Hours = new SelectList(HoursBind, "Value", "Text");
//                    ViewBag.PicDate = date;
//                    ViewBag.PicTime = Time;
//                    ViewBag.PicServiceName = servicename;

//                    TempData["ValidDays"] = selectdays;
//                    ViewBag.ValidDays = selectdays;
//                }
//                else if (ServiceType1 == "Monthly Twise Service")
//                {
//                    //ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text");
//                    ViewBag.WeekDays = selectdays;
//                    ViewBag.Hours = new SelectList(HoursBind, "Value", "Text");
//                    ViewBag.PicDate = date;
//                    ViewBag.PicTime = Time;
//                    ViewBag.PicServiceName = servicename;
//                }
//                else if (ServiceType1 == "Monthly Service")
//                {
//                    //ViewBag.WeekDays = new MultiSelectList(list, "Value", "Text");
//                    ViewBag.WeekDays = selectdays;
//                    ViewBag.Hours = new SelectList(HoursBind, "Value", "Text");
//                    ViewBag.PicDate = date;
//                    ViewBag.PicTime = Time;
//                    ViewBag.PicServiceName = servicename;
//                }
//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }

//            return View();
//        }

//        [HttpPost]
//        public ActionResult AddNewEdit(RemainderPurpose remainderservice)
//        {
//            var ShowResult = "";

//            int userId = Convert.ToInt32(Session["UserID"]);

//            try
//            {
//                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                
//                    DSRCManagementSystemEntities1 objdbData = new DSRCManagementSystemEntities1();


//                var Serviceypecheck = remainderservice.ServiceType;

//                var getserviceId = objdb.Master_RemainderServiceType.Where(x => x.ServiceType == remainderservice.ServiceType && x.IsActive == true).Select(c => c.RemainderServiceID).FirstOrDefault();

//                var GetID = objdb.RemainderServiceDetails.Where(x => x.ServiceID == IdData).Select(o => o).FirstOrDefault();

//                var status=string.Empty;

//                var ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                //var getServiceId = objdb.Master_RemainderServiceType.Where(x => x.ServiceType == remainderservice.ServiceType).Select(x => x.RemainderServiceID).FirstOrDefault();


//                var ServiceTblcheck = (from p in objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status)
//                                  select new 
//                                  {
//                                      Id=p.ServiceDataID
//                                  }).ToList();

//                var ServcieStartDate = remainderservice.SelectDate;  //c

//                var fed = objdb.RemainderServiceDetails.Where(x => x.RemainderServiceTypeID == getserviceId).Select(o => o).FirstOrDefault();


//                if (Serviceypecheck == "Hourly Service")
//                {
//                    GetID.RemainderServiceTypeID = getserviceId;
//                    GetID.ServiceName = remainderservice.ServiceName;
//                    GetID.HoursCheck = remainderservice.ServiceHour;
//                    GetID.SelectedDays = remainderservice.Days;
//                    GetID.StartTime = remainderservice.ServiceStartTime;
//                    GetID.ModBy = userId;
//                    GetID.ModDate = DateTime.Now;
//                    GetID.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.SaveChanges();


                    


//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                   // DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);

//                    DateTime startDate = new DateTime(ServcieStartDate.Year, ServcieStartDate.Month, ServcieStartDate.Day);

//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);




//                    foreach (var deletedata in ServiceTblcheck)
//                    {
//                        if (deletedata.Id != null)
//                        {
//                            objdbData.ServiceDataDetails.DeleteObject(ServiceTbl);
//                            objdbData.SaveChanges();
//                            ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                        }
//                    }


//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = IdData;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(1);
//                    }
//                    ShowResult = "Success";
//                }
//                else if (Serviceypecheck == "Daily Service")
//                {
//                    GetID.RemainderServiceTypeID = getserviceId;
//                    GetID.ServiceName = remainderservice.ServiceName;
//                    GetID.HoursCheck = remainderservice.ServiceHour;
//                    GetID.SelectedDays = remainderservice.Days;
//                    GetID.StartTime = remainderservice.ServiceStartTime;
//                    GetID.ModBy = userId;
//                    GetID.ModDate = DateTime.Now;
//                    GetID.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.SaveChanges();



//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    //DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);
//                    DateTime startDate = new DateTime(ServcieStartDate.Year, ServcieStartDate.Month, ServcieStartDate.Day);
//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);


//                    foreach (var deletedata in ServiceTblcheck)
//                    {
//                        if (deletedata.Id != null)
//                        {
//                            objdbData.ServiceDataDetails.DeleteObject(ServiceTbl);
//                            objdbData.SaveChanges();
//                            ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                        }
//                    }


//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = IdData;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(1);
//                    }

//                    ShowResult = "Success";
//                }
//                else if (Serviceypecheck == "Weekly Service")
//                {
//                    GetID.RemainderServiceTypeID = getserviceId;
//                    GetID.ServiceName = remainderservice.ServiceName;
//                    GetID.HoursCheck = remainderservice.ServiceHour;
//                    GetID.SelectedDays = remainderservice.Days;
//                    GetID.StartTime = remainderservice.ServiceStartTime;
//                    GetID.ModBy = userId;
//                    GetID.ModDate = DateTime.Now;
//                    GetID.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.SaveChanges();


                   

//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                   // DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);
//                    DateTime startDate = new DateTime(ServcieStartDate.Year, ServcieStartDate.Month, ServcieStartDate.Day);
//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);



//                    foreach (var deletedata in ServiceTblcheck)
//                    {
//                        if (deletedata.Id != null)
//                        {
//                            objdbData.ServiceDataDetails.DeleteObject(ServiceTbl);
//                            objdbData.SaveChanges();
//                            ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                        }
//                    }


//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = IdData;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(7);
//                    }


//                    ShowResult = "Success";
//                }
//                else if (Serviceypecheck == "Monthly Twise Service")
//                {
//                    GetID.RemainderServiceTypeID = getserviceId;
//                    GetID.ServiceName = remainderservice.ServiceName;
//                    GetID.HoursCheck = remainderservice.ServiceHour;
//                    GetID.SelectedDays = remainderservice.Days;
//                    GetID.StartTime = remainderservice.ServiceStartTime;
//                    GetID.ModBy = userId;
//                    GetID.ModDate = DateTime.Now;
//                    GetID.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.SaveChanges();



//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();



//                    //DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);
//                    DateTime startDate = new DateTime(ServcieStartDate.Year, ServcieStartDate.Month, ServcieStartDate.Day);
//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);


//                    foreach (var deletedata in ServiceTblcheck)
//                    {
//                        if (deletedata.Id != null)
//                        {
//                            objdbData.ServiceDataDetails.DeleteObject(ServiceTbl);
//                            objdbData.SaveChanges();
//                            ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                        }
//                    }

//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = IdData;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(15);
//                    }

//                    ShowResult = "Success";
//                }
//                else if (Serviceypecheck == "Monthly Service")
//                {
//                    GetID.RemainderServiceTypeID = getserviceId;
//                    GetID.ServiceName = remainderservice.ServiceName;
//                    GetID.HoursCheck = remainderservice.ServiceHour;
//                    GetID.SelectedDays = remainderservice.Days;
//                    GetID.StartTime = remainderservice.ServiceStartTime;
//                    GetID.ModBy = userId;
//                    GetID.ModDate = DateTime.Now;
//                    GetID.ServiceMethod = remainderservice.EventMethodName;
//                    objdb.SaveChanges();



//                    var GetServiceID = objdb.RemainderServiceDetails.OrderByDescending(p => p.ServiceID).Select(r => r.ServiceID).FirstOrDefault();

//                    DateTime startDate = new DateTime(remainderservice.SelectDate.Year, remainderservice.SelectDate.Month, remainderservice.SelectDate.Day);
//                    //DateTime startDate = new DateTime(ServcieStartDate.Year, ServcieStartDate.Month, ServcieStartDate.Day);
//                    DateTime EndDate1 = DateTime.Parse(dateInString);
//                    DateTime EndDate = new DateTime(EndDate1.Year, EndDate1.Month, EndDate1.Day);

                   
//                    foreach (var deletedata in ServiceTblcheck)
//                    {
//                        if (deletedata.Id != null)
//                        {
//                            objdbData.ServiceDataDetails.DeleteObject(ServiceTbl);
//                            objdbData.SaveChanges();
//                            ServiceTbl = objdbData.ServiceDataDetails.Where(x => x.RemainderServiceID == IdData && x.ServiceStatus == status).Select(o => o).FirstOrDefault();
//                        }
//                    }

//                    while (startDate <= EndDate)
//                    {
//                        DSRCManagementSystem.ServiceDataDetail objremainderData = new DSRCManagementSystem.ServiceDataDetail();

//                        objremainderData.RemainderServiceID = IdData;
//                        objremainderData.ServiceDate = startDate;
//                        objremainderData.ServiceStatus = "";
//                        objdbData.ServiceDataDetails.AddObject(objremainderData);
//                        objdbData.SaveChanges();

//                        startDate = startDate.AddDays(30);
//                    }

//                    ShowResult = "Success";
//                }

//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }


//            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
//        }

//        private readonly DSRCManagementSystemEntities1 _db = new DSRCManagementSystemEntities1();

//        [HttpPost]
//        public ActionResult DeleteRemainderService(int Id)
//        {
//            var ShowResult = "";
//            var value = _db.RemainderServiceDetails.Where(x => x.ServiceID == Id && x.IsActive == true).Select(o => o).FirstOrDefault();
//            value.IsActive = false;
//            _db.SaveChanges();
//            if (value.IsActive == false)
//            {
//                ShowResult = "Success";
//            }
//            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
//        }


//        [HttpPost]
//        public ActionResult StartStop(int Id)
//        {
//            var ShowResult = "";
//            var Servicecheck = _db.RemainderServiceDetails.Where(x => x.ServiceID == Id && x.IsActive == true).Select(y => y.IsOnService).FirstOrDefault();
//            var ServiceChange = _db.RemainderServiceDetails.Where(x => x.ServiceID == Id && x.IsActive == true).Select(o => o).FirstOrDefault();

//            if (Servicecheck == true)
//            {
//                ServiceChange.IsOnService = false;
//                _db.SaveChanges();
//                ShowResult = "Stop";
//            }
//            else
//            {
//                ServiceChange.IsOnService = true;
//                _db.SaveChanges();
//                ShowResult = "Start";
//            }
//            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
//        }

//        public static bool historycheck = false;
//        public static int GetIdCheck = 0;
//        [HttpPost]
//        public ActionResult RemainderSetupViewHistory(int Id)
//        {
//            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
//            List<DSRCManagementSystem.Models.RemainderPurpose> objTab = new List<Models.RemainderPurpose>();
//            var ShowResult = "View";

//            try
//            {
//                GetIdCheck = Id;


//                var ServiceTypeId = objdb.RemainderServiceDetails.Where(x => x.IsActive == true && x.ServiceID == Id).Select(m => m.RemainderServiceTypeID).FirstOrDefault();


//                objTab = (from r in objdb.RemainderServiceDetails.Where(m => m.RemainderServiceTypeID == ServiceTypeId)
//                          join m in objdb.Master_RemainderServiceType on r.RemainderServiceTypeID equals m.RemainderServiceID
//                          where (r.IsActive == true)
//                          join s in objdb.ServiceDataDetails on r.ServiceID equals s.RemainderServiceID
//                          select new DSRCManagementSystem.Models.RemainderPurpose
//                          {
//                              Id = r.ServiceID,
//                              ServiceName = r.ServiceName,
//                              ServiceType = m.ServiceType,
//                              SelectDate = (DateTime)s.ServiceDate,
//                              Days = r.SelectedDays,
//                              checkMonday = true,
//                              checkTuesday = true,
//                              checkWednesday = true,
//                              checkThursday = true,
//                              checkFriday = true,
//                              checkSaturday = true,
//                              checkSunday = true,
//                              OnServiceStatus = (bool)r.IsOnService,
//                              ServiceStartTime = r.StartTime,
//                              ServiceStatus = s.ServiceStatus
//                          }).ToList();

//                var Service = (from pi in objdb.Master_RemainderServiceType.Where(x => x.IsActive == true)
//                               select new
//                               {
//                                   Id3 = pi.RemainderServiceID,
//                                   SType = pi.ServiceType
//                               }).ToList();

//                ViewBag.ServiceTypeData = new SelectList(Service, "Id3", "SType", ServiceTypeId);

//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }
           
//            return Json(new { Result = ShowResult, URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
//        }
        
//        [HttpGet]
//        public ActionResult HistoryData()
//        {           
//            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
//            List<DSRCManagementSystem.Models.RemainderPurpose> objTab = new List<Models.RemainderPurpose>();

//            try
//            {
//                var ServiceTypeId = objdb.RemainderServiceDetails.Where(x => x.IsActive == true && x.ServiceID == GetIdCheck).Select(m => m.RemainderServiceTypeID).FirstOrDefault();

//                objTab = (from r in objdb.RemainderServiceDetails.Where(m => m.RemainderServiceTypeID == ServiceTypeId)
//                          join m in objdb.Master_RemainderServiceType on r.RemainderServiceTypeID equals m.RemainderServiceID
//                          where (r.IsActive == true)
//                          join s in objdb.ServiceDataDetails on r.ServiceID equals s.RemainderServiceID
//                          select new DSRCManagementSystem.Models.RemainderPurpose
//                          {
//                              Id = r.ServiceID,
//                              ServiceName = r.ServiceName,
//                              ServiceType = m.ServiceType,
//                              SelectDate = (DateTime)s.ServiceDate,
//                              Days = r.SelectedDays,
//                              checkMonday = true,
//                              checkTuesday = true,
//                              checkWednesday = true,
//                              checkThursday = true,
//                              checkFriday = true,
//                              checkSaturday = true,
//                              checkSunday = true,
//                              OnServiceStatus = (bool)r.IsOnService,
//                              ServiceStartTime = r.StartTime,
//                              ServiceStatus = s.ServiceStatus,
//                              CheckHistory = true
//                          }).ToList();                
//            }
//            catch (Exception Ex)
//            {
//                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
//                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
//                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
//            }

//            return View(objTab);            
//        }

//        [HttpPost]
//        public ActionResult HistoryData(FormCollection form)
//        {
//            return RedirectToAction("RemainderSetup");
//        }


//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult GetDays(int BranchId)
//        {
//            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
//            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            
//                List<SelectListItem> DaysCheck = new List<SelectListItem>();

//                DaysCheck.Add(new SelectListItem { Value = "1", Text = "Mon", Selected = false});
//                DaysCheck.Add(new SelectListItem { Value = "2", Text = "Tue", Selected = false});
//                DaysCheck.Add(new SelectListItem { Value = "3", Text = "Wed", Selected = false});
//                DaysCheck.Add(new SelectListItem { Value = "4", Text = "Thu", Selected = false});
//                DaysCheck.Add(new SelectListItem { Value = "5", Text = "Fri", Selected = false});
//                // list.Add(new SelectListItem { Value = "6", Text = "Sat", Selected = true });
//                // list.Add(new SelectListItem { Value = "7", Text = "Sun", Selected = true });


               
//            return Json(new SelectList(DaysCheck, "Value", "Text"), JsonRequestBehavior.AllowGet);
//        }


//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult GetDaysCheck(int BranchId)
//        {
//            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
//            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

//            List<SelectListItem> DaysCheck = new List<SelectListItem>();

//            DaysCheck.Add(new SelectListItem { Value = "1", Text = "Mon", Selected=true });
//            DaysCheck.Add(new SelectListItem { Value = "2", Text = "Tue", Selected=true });
//            DaysCheck.Add(new SelectListItem { Value = "3", Text = "Wed", Selected=true });
//            DaysCheck.Add(new SelectListItem { Value = "4", Text = "Thu", Selected=true });
//            DaysCheck.Add(new SelectListItem { Value = "5", Text = "Fri", Selected=true });
//            // list.Add(new SelectListItem { Value = "6", Text = "Sat", Selected = true });
//            // list.Add(new SelectListItem { Value = "7", Text = "Sun", Selected = true });



//            return Json(new SelectList(DaysCheck, "Value", "Text"), JsonRequestBehavior.AllowGet);
//        }


//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult GetHour(int BranchId)
//        {
//            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();
//            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();



//            List<SelectListItem> HoursBind = new List<SelectListItem>();

//            HoursBind.Add(new SelectListItem { Value = "1", Text = "1" });

//            return Json(new SelectList(HoursBind, "Value", "Text"), JsonRequestBehavior.AllowGet);
//        }


//        [AcceptVerbs(HttpVerbs.Get)]
//        public ActionResult GetMethodName(int DepartmentId)
//        {
//            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
//            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

//            var GetAllMethod = typeof(DSRCHRMSRemainderService.RemainderMethods);


//            List<SelectListItem> EventCall = new List<SelectListItem>();
//            int i = 1;

//            foreach (var m in GetAllMethod.GetMethods())
//            {
//                if (m.ToString().Contains("Void"))
//                {
//                    EventCall.Add(new SelectListItem { Value = i.ToString(), Text = m.ToString() });
//                    i++;
//                }
//                else { }
//            }
//            return Json(new SelectList(EventCall, "Value", "Text"), JsonRequestBehavior.AllowGet);
//        }

//    }
//}
