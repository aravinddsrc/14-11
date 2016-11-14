using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using DSRCManagementSystem.DSRCLogic;



namespace DSRCManagementSystem.Controllers
{

    public class AssetManagementController : Controller
    {

        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ManageComputers()
        {
            ModelState.Clear();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ManageComputers objcmp = new DSRCManagementSystem.Models.ManageComputers();
            List<DSRCManagementSystem.Models.ManageComputers> CmpList = new List<DSRCManagementSystem.Models.ManageComputers>();
            try{
           
            CmpList = (from c in db.computermanagements
                       where c.ISDelete == false && c.ComputerStatusNew=="Active"
                       join s in db.Master_Os on c.OsId equals s.OsId

                       select new DSRCManagementSystem.Models.ManageComputers()
                       {
                           ID = c.managementid,
                           Manufacturer = c.Manufacturer,
                           CPUID = c.CPUID,
                           MonitorID = c.MonitorID,
                           
                           OS = s.OsName,
                           Memory = c.Memory,
                           OSID = s.OsId,

                           CPU = c.cpu,
                           ComputerStatus = c.ComputerStatusNew,
                           ComputerName = c.ComputerName,
                           IP = c.IP,
                       }).ToList();
            var OSList = db.Master_Os.ToList();
           
            foreach (var item in CmpList)
            {
                var Assignedlist = db.ComputerAssigneds.Where(x => x.Managementid == item.ID).FirstOrDefault();
                if (Assignedlist != null)
                {
                    item.Alreadyassigned = 1;
                }
                else
                {

                   item.Alreadyassigned=0;
                }

            }
            ViewBag.OSIDList = new SelectList(OSList, "OsId", "OsName");
            var value = Convert.ToInt32(TempData["assigned"]);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(CmpList.ToList());
        }


        [HttpPost]
        public ActionResult ManageComputers(ManageComputers model, FormCollection form)
        {
            ModelState.Clear();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ManageComputers objcmp = new DSRCManagementSystem.Models.ManageComputers();
            List<DSRCManagementSystem.Models.ManageComputers> CmpList = new List<DSRCManagementSystem.Models.ManageComputers>();
            try{
            bool status = form["Inactive"].Contains("true");
            if (status == true)
            {
                CmpList = (from c in db.computermanagements
                           where c.ISDelete == false && c.ComputerStatusNew == "Inactive"
                           join s in db.Master_Os on c.OsId equals s.OsId


                           select new DSRCManagementSystem.Models.ManageComputers()
                           {
                               ID = c.managementid,
                               Manufacturer = c.Manufacturer,
                               CPUID = c.CPUID,
                               MonitorID = c.MonitorID,

                               OS = s.OsName,
                               Memory = c.Memory,
                               OSID = s.OsId,

                               CPU = c.cpu,
                               ComputerStatus = c.ComputerStatusNew,
                               ComputerName = c.ComputerName,
                               IP = c.IP,
                           }).ToList();
                var OSList = db.Master_Os.ToList();

                foreach (var item in CmpList)
                {
                    var Assignedlist = db.ComputerAssigneds.Where(x => x.Managementid == item.ID).FirstOrDefault();
                    if (Assignedlist != null)
                    {
                        item.Alreadyassigned = 1;
                    }
                    else
                    {

                        item.Alreadyassigned = 0;
                    }

                }
                ViewBag.OSIDList = new SelectList(OSList, "OsId", "OsName");
                var value = Convert.ToInt32(TempData["assigned"]);
            }
            else
            {
                CmpList = (from c in db.computermanagements
                           where c.ISDelete == false && c.ComputerStatusNew == "Active"
                           join s in db.Master_Os on c.OsId equals s.OsId


                           select new DSRCManagementSystem.Models.ManageComputers()
                           {
                               ID = c.managementid,
                               Manufacturer = c.Manufacturer,
                               CPUID = c.CPUID,
                               MonitorID = c.MonitorID,

                               OS = s.OsName,
                               Memory = c.Memory,
                               OSID = s.OsId,

                               CPU = c.cpu,
                               ComputerStatus = c.ComputerStatusNew,
                               ComputerName = c.ComputerName,
                               IP = c.IP,
                           }).ToList();
                var OSList = db.Master_Os.ToList();

                foreach (var item in CmpList)
                {
                    var Assignedlist = db.ComputerAssigneds.Where(x => x.Managementid == item.ID).FirstOrDefault();
                    if (Assignedlist != null)
                    {
                        item.Alreadyassigned = 1;
                    }
                    else
                    {

                        item.Alreadyassigned = 0;
                    }

                }
                ViewBag.OSIDList = new SelectList(OSList, "OsId", "OsName");
                var value = Convert.ToInt32(TempData["assigned"]);
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(CmpList.ToList());
        }

        [HttpGet]
        public ActionResult AddNewComputer()
        {
            try{
            ViewBag.ComputerStatusList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Active", Value = 1 }, new { Text = "Inactive", Value = 2 } }, "Value", "Text", 0);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var OSList = db.Master_Os.ToList();
            ViewBag.OSIDList = new SelectList(new[] { new Master_Os() { OsId = 0, OsName = "---Select---" } }.Union(OSList), "OsId", "OsName", 0);
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
        public ActionResult AddNewComputer(ManageComputers model,string cmpname)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var Already = db.computermanagements.Where(x => x.ComputerName == model.ComputerName && x.ISDelete == false).Select(o => o).FirstOrDefault();
                if (Already != null)
                {
                    return Json("Already", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        // DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                        if (model.ComputerStatus == "1")
                        {
                            model.ComputerStatus = "Active";
                        }
                        else
                        {
                            model.ComputerStatus = "Inactive";
                        }



                        var Compobj = db.computermanagements.CreateObject();
                        Compobj.Manufacturer = model.Manufacturer.Trim();
                        Compobj.CPUID = model.CPUID.Trim();
                        Compobj.MonitorID = model.MonitorID.Trim();
                        Compobj.Memory = model.Memory;
                        Compobj.ComputerStatusNew = model.ComputerStatus;
                        Compobj.ComputerName = model.ComputerName.Trim();
                        Compobj.ISDelete = false;
                        Compobj.IP = model.IP;
                        Compobj.cpu = model.CPU.Trim();
                        Compobj.OsId = model.OSID;
                        db.computermanagements.AddObject(Compobj);
                        db.SaveChanges();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return View();
                    }
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
        public ActionResult Edit(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var obj = new ManageComputers();
            try{

            obj = (from t in db.computermanagements
                       where t.ISDelete == false
                       join s in db.Master_Os on t.OsId equals s.OsId

                       where t.managementid == ID
                       select new ManageComputers()
                       {
                           OSID = s.OsId,

                           ID = t.managementid,
                           Manufacturer = t.Manufacturer,
                           CPUID = t.CPUID,
                           MonitorID = t.MonitorID,
                           OS = s.OsName,
                           Memory = t.Memory,
                           CPU = t.cpu,
                           ComputerStatus = t.ComputerStatusNew,
                           ComputerName = t.ComputerName,
                           IP = t.IP,
                       }).FirstOrDefault();
            


            if (obj.ComputerStatus == "Active")
            {
                ViewBag.ComputerStatusList = new SelectList(new[] { new { Text = "Active", Value = "Active" }, new { Text = "Inactive", Value = "Inactive" } }, "Text", "Value", obj.ComputerStatus);
            }
            else
            {
                ViewBag.ComputerStatusList = new SelectList(new[] { new { Text = "Inactive", Value = "Inactive" }, new { Text = "Active", Value = "Active" } }, "Text", "Value", obj.ComputerStatus);
            }

            var OSList = db.Master_Os.ToList();
            ViewBag.OSIDList = new SelectList(OSList, "OsId", "OsName", obj.OSID);
            return View(obj);
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

        public ActionResult Edit(ManageComputers model)
        {
            try
            {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                var obj_Comp = db.computermanagements.Where(o => o.managementid == model.ID).Select(o => o).FirstOrDefault();
                obj_Comp.Manufacturer = model.Manufacturer;
                obj_Comp.CPUID = model.CPUID;
                obj_Comp.MonitorID = model.MonitorID;
                obj_Comp.OsId = Convert.ToInt32(model.OS);
                obj_Comp.Memory = model.Memory;
                obj_Comp.cpu = model.CPU;
                obj_Comp.ComputerStatusNew = model.ComputerStatus;
                obj_Comp.ComputerName = model.ComputerName;
                obj_Comp.IP = model.IP;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public ActionResult Delete(int ID)
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var obj = db.computermanagements.Where(x => x.managementid == ID).FirstOrDefault();
            obj.ISDelete = true;
            
            var obj1 = db.ComputerAssigneds.Where(x => x.Managementid == ID).FirstOrDefault();
            if (obj1 != null)
            {
                if (obj.managementid == obj1.Managementid)
                {
                    obj1.ISDelete = true;
                }
            }
            

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
        public ActionResult SuggestMonid(string MonitorID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                bool Result = db.computermanagements.Where(x => x.MonitorID == MonitorID).FirstOrDefault() == null;
                string Message = "";
                if (!Result)
                {
                    Message = "Available";
                }
                else
                {

                }
                return Json(new { Name = Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        [HttpGet]
        public ActionResult SuggestCmpName(string ComputerName)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                bool Result = db.computermanagements.Where(x => x.ComputerName == ComputerName).FirstOrDefault() == null;
                string Message = "";
                if (!Result)
                {
                    Message = "Available";
                }
                else
                {

                }


                return Json(new { Name = Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        [HttpGet]
        public ActionResult SuggestCPUID(string CPUID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                bool Result = db.computermanagements.Where(x => x.CPUID == CPUID).FirstOrDefault() == null;
                string Message = "";
                if (!Result)
                {
                    Message = "Available";
                }
                else
                {

                }


                return Json(new { Name = Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }


        [HttpGet]
        public ActionResult AssignComputers()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.AssignComputers> AsgnList = new List<DSRCManagementSystem.Models.AssignComputers>();

            try{
       

            AsgnList = (from a in db.ComputerAssigneds where a.ISDelete == false
                        join d in db.Departments on a.Departmentid equals d.DepartmentId
                        join l in db.locations on a.Locationid equals l.locationid into leftjoin from left in leftjoin.DefaultIfEmpty()
                        join u in db.Users on a.Userid equals u.UserID where u.IsActive == true && u.UserStatus != 6
                        join e in db.UserAssetMappings on a.Assignid equals e.AssignId where e.IsActive == true
                        join c in db.computermanagements on a.Managementid equals c.managementid where c.ISDelete==false orderby u.FirstName ascending
                        join b in db.Assets on e.AssetID equals b.AssetID  into joinasset from jn in joinasset.DefaultIfEmpty() 
                        
                        select new DSRCManagementSystem.Models.AssignComputers()
                        {
                            EmployeeName = (u.FirstName + " " + (u.LastName ?? "")).Trim(),
                            Department = d.DepartmentName,
                            Location = a.Locationid==0?"Not Allocated":left.LocationName,
                            ComputerName = c.ComputerName,
                            WorkstationNumber = a.WorkStation,
                            PenDriveAcess = a.pendriveAccessnew,
                            ComponentId =jn.Name_Model_No,
                            UPSID = a.UPSID,
                            ID = a.Assignid,
                            assetid = e.AssetID,
                            Component=e.Component
                        }).Distinct().ToList();


            foreach (var item in AsgnList)
            {
                if (item.Component != "")
                {
                    item.Component = AssetManagementController.GetUserString(db, item.Component);
                }

                else
                {
                    item.Component = null;
                }
            }

            var value = db.computermanagements.Where(x => x.ISDelete == false).ToList();


            //var EmployeeList = db.Users.ToList();
            //ViewBag.EmployeeIDList = new SelectList(EmployeeList, "UserID", "FirstName");
            var DepartmentList = db.Departments.ToList();
            //var DepartmentList=Master_Department.Department();
            ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
            var LocationList = db.locations.ToList();
            ViewBag.LocationIDList = new SelectList(LocationList, "locationid", "LocationName");
            var ComputerNameList = db.computermanagements.Where(x => x.ISDelete == false).ToList();
            ViewBag.ManagementIdList = new SelectList(value, "managementid", "ComputerName");
            var Component = db.Assets.Where(v=>v.ISDelete==false).ToList();
            ViewBag.ComponentList = new SelectList(Component, "id", "name");
            var UpsList = db.ComputerAssigneds.ToList();
            ViewBag.UpsList = new SelectList(UpsList, "id1", "name1");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(AsgnList);
        }


        private static string GetUserString(DSRCManagementSystemEntities1 db, string Attendee)
        {
                 var tmp = "";
            try
            {
            List<int?> lst = new List<int?>();
                 if (Attendee != null)
                 {
                     foreach (var str in Attendee.Split(','))
                     {
                         lst.Add(Convert.ToInt32(str));
                     }
                     var obj = (from user in db.Assets.Where(user => lst.Contains(user.AssetID)) select user.Name_Model_No).ToList();

                     int len = obj.Count; int i = 0;
                     foreach (var str in obj)
                     {
                         i++;
                         tmp += str;
                         if (i < len)
                         {
                             tmp += ", ";
                         }
                     }

                 }
                 return tmp;
            }
            catch (Exception Ex)
            {
                ExceptionHandlingController.ExceptionDetails(Ex, "GetUserString", "AssetManagement");
            }
                return tmp;
        }


        [HttpPost]
        public ActionResult AssignComputers(AssignComputers model)
        {
            DSRCManagementSystem.Models.AssignComputers obj_Assign = new DSRCManagementSystem.Models.AssignComputers();
            return View(obj_Assign);
        }


        [HttpGet]
        public ActionResult AssignComputer()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            try{
            DSRCManagementSystem.Models.AssignComputers obj = new DSRCManagementSystem.Models.AssignComputers();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            UserDetail();
            var ComponentList = db.Assets.ToList();

            var s = (from at in db.AssetTypes
                     join ast in db.Assets on at.AssetTypeId equals ast.AssetTypeId
                     where at.AssetName != "UPS" && ast.ISDelete == false && ast.ComputerName==0
                     select new
                     {
                         name = ast.Name_Model_No,
                         id = ast.AssetID//at.AssetTypeId, by Boobalan
                     }).ToList();


            var UpsList = db.Assets.ToList();
            var w = (from at in db.AssetTypes
                     join ast in db.Assets on at.AssetTypeId equals ast.AssetTypeId
                     where at.AssetName == "UPS" && ast.ISDelete == false
                     select new
                     {
                         name1 = ast.Name_Model_No,
                         id1 = at.AssetTypeId
                     }).ToList();

            var computer = (from p in db.computermanagements.Where(x => x.ISDelete == false)
                                      select new
                                      {
                                          managementid = p.managementid,
                                          ComputerName = p.ComputerName


                                      }).ToList();


            ViewBag.PenDriveAcessList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "No", Value = 0 }, new { Text = "Yes", Value = 1 } }, "Value", "Text", 0);
            var LocationList = db.locations.ToList();
            var deleted = db.UserAssetMappings.Where(x => x.IsActive == false).Select(x => x.AssignId.Value).ToList();
            var assign = db.ComputerAssigneds.Select(x => x.Assignid).ToList();
            var LstCompAssigned = assign.Except(deleted);
            var Lst = db.ComputerAssigneds.Where(x => LstCompAssigned.Contains(x.Assignid)).Select(x => x.Managementid).ToList();
            var ComputerNameList = db.computermanagements.Where(x => x.ISDelete == false && x.ComputerStatusNew == "Active").ToList(); //|| !Lst.Contains(x.managementid)).ToList();
            //var ComputerNameList = db.computermanagements.Where(x => x.ISDelete != true).ToList();
            ViewBag.ComponentIdList = new MultiSelectList(s, "id", "name");
            ViewBag.UpsList = new SelectList(w, "id1", "name1");
            ViewBag.LocationIDList = new SelectList(new[] { new location() { locationid = 0, LocationName = "---select---" } }.Union(LocationList), "locationid", "LocationName", 0);
            ViewBag.ManagementIdList = new SelectList(new[] { new computermanagement() { managementid = 0, ComputerName = "---select---" } }.Union(ComputerNameList), "managementid", "ComputerName", 0);
           // ViewBag.ManagementIdList = new SelectList(computer, "managementid",);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();

        }
        public void Component()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var query = db.Assets.ToList();
            IList<SelectListItem> Cmpnt = new List<SelectListItem>();
            foreach (var a in query)
            {
                Cmpnt.Add(new SelectListItem { Value = Convert.ToString(a.AssetID), Text = a.Name_Model_No });
            }
            ViewBag.Detail1 = Cmpnt;
        }


        [HttpPost]
        public ActionResult AssignComputer(AssignComputersNew model)
        {
            try
            {

            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            //var AlreadyEmp = db.ComputerAssigneds.Where(x => x.Userid == model.UserId && x.ISDelete == false).Select(o => o).FirstOrDefault();

            var AlreadyComp = (from p in db.computermanagements.Where(x => x.ComputerName == model.ComputerName && x.ISDelete == false)
                              join t in db.ComputerAssigneds on p.managementid equals t.Managementid where t.ISDelete==false
                              select new
                                {
                                    ComputerName = p.ComputerName,
                                }).FirstOrDefault();
        //    var already = db.ComputerAssigneds.Where(x => x. == model.UserId).Select(o => o).FirstOrDefault();
            //if (AlreadyEmp != null)
            //{
            //    return Json("AlreadyEmp", JsonRequestBehavior.AllowGet);
            //}

            if (AlreadyComp != null)
            {

                return Json("AlreadyComp", JsonRequestBehavior.AllowGet);

            }


            else
            {
                var deptid = db.Departments.FirstOrDefault(o => o.DepartmentName == model.Departmentvalue).DepartmentId;
                ComputerAssigned Asgobj = null;
                
                ComputerAssigned repeatcheck = db.ComputerAssigneds.Where(r => r.Managementid == model.Managementid && r.ISDelete == true).FirstOrDefault();
                
                if(repeatcheck!=null)
                {
                    Asgobj = repeatcheck;
                    db.SaveChanges();
                }
                else{
                    Asgobj = db.ComputerAssigneds.CreateObject();
                    db.ComputerAssigneds.AddObject(Asgobj);
                }
                Asgobj.Userid = model.UserId;
                Asgobj.Locationid = model.LocationId;
                Asgobj.Managementid = model.Managementid;
                Asgobj.Departmentid = deptid;
                Asgobj.pendriveAccessnew = model.Pendrive.Equals("1") ? true : false;
                Asgobj.WorkStation = model.WorkstationNumber.Trim();
                Asgobj.ComponentId = model.ComponentId;
                Asgobj.UPSID = model.UPSID;
                Asgobj.ISDelete = false;
                db.SaveChanges();
                var item = db.ComputerAssigneds.OrderByDescending(i => i.Assignid).FirstOrDefault();

                        DSRCManagementSystem.UserAssetMapping obj = new DSRCManagementSystem.UserAssetMapping();
                    var Rcheck = db.UserAssetMappings.Where(r => r.AssignId == item.Assignid).FirstOrDefault();
                if (!string.IsNullOrEmpty(model.ComponentId))
                {
                    if (Rcheck == null)
                    {
                        obj.UseID = model.UserId;
                        obj.IsActive = true;
                        obj.AssignId = item.Assignid;
                        obj.Component = model.ComponentId;
                        db.UserAssetMappings.AddObject(obj);
                        db.SaveChanges();
                    }
                    else
                    {
                        string[] splitComponentId = model.ComponentId.ToString().Split(',');
                        foreach (var s in splitComponentId)
                        {
                            if (s.Length > 0)
                            {
                                obj.AssetID = Convert.ToInt32(s);
                            }
                        }
                        Rcheck.UseID = model.UserId;
                        Rcheck.IsActive = true;
                        Rcheck.AssignId = item.Assignid;
                        Rcheck.Component = model.ComponentId;


                        //obj.UseID = model.UserId;
                        //obj.IsActive = true;    
                        //obj.AssignId = item.Assignid;
                        //obj.Component = model.ComponentId;
                        //db.UserAssetMappings.AddObject(obj);
                        db.SaveChanges();
                    }
                }

                else
                {
                    if (Rcheck == null)
                    {
                        var UserAssetObj = db.UserAssetMappings.CreateObject();
                        UserAssetObj.AssignId = item.Assignid;
                        UserAssetObj.UseID = model.UserId;
                        UserAssetObj.AssetID =null;
                        UserAssetObj.Component = model.ComponentId;
                        UserAssetObj.IsActive = true;
                        db.UserAssetMappings.AddObject(UserAssetObj);
                    }
                }
                db.SaveChanges();
                ManageComputers val = new ManageComputers();
                val.Alreadyassigned = 1;

                TempData["Assigned"] = 1;

                return Json("Success", JsonRequestBehavior.AllowGet);
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
        public JsonResult GetDepartmentName(int userId)
        {
            try
            {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var deptid = db.Users.FirstOrDefault(o => o.UserID == userId).DepartmentId;
            var deptname = db.Departments.FirstOrDefault(o => o.DepartmentId == deptid).DepartmentName;
            return Json(new { dept = deptname }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json("",JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditCmptr(int ID)
        {
            var obj = new AssignComputers();
            try
            {

            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            //ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.AssignComputers AsgnList = new DSRCManagementSystem.Models.AssignComputers();
            UserDetail();
            obj = (from a in db.ComputerAssigneds where a.Assignid == ID && a.ISDelete == false
                       join e in db.UserAssetMappings on a.Assignid equals e.AssignId
                       // join b in db.Assets on e.AssetID equals b.AssetID
                       join d in db.Departments on a.Departmentid equals d.DepartmentId
                       join u in db.Users on a.Userid equals u.UserID
                       join l in db.locations on a.Locationid equals l.locationid
                       join c in db.computermanagements on a.Managementid equals c.managementid

                       join b in db.Assets on e.AssetID equals b.AssetID into joinasset from jn in joinasset.DefaultIfEmpty()
                       
                       select new DSRCManagementSystem.Models.AssignComputers()
                      {
                          UserId = u.UserID,
                          DepartmentId = d.DepartmentId,
                          LocationId = l.locationid,
                          Managementid = c.managementid,
                          EmployeeName = (u.FirstName + " " + (u.LastName ?? "")).Trim(),
                          Department = d.DepartmentName,
                          Location = l.LocationName,
                          ComputerName = c.ComputerName,
                          PenDriveAcess = a.pendriveAccessnew,
                          UPSID = a.UPSID,
                          ComponentId = jn.Name_Model_No,
                          Component=e.Component,
                          CompId = e.AssetID,
                          WorkstationNumber = a.WorkStation,
                          ID = a.Assignid,
                      }).FirstOrDefault();


            
             
                //var z = AssetManagementController.GetUserString(db, obj.Component);
             
            if (obj.PenDriveAcess == false)
            {
                ViewBag.PenDriveAcessList = new SelectList(new[] { new { Text = "No", Value = "No" }, new { Text = "Yes", Value = "Yes" } }, "Text", "Value", "No");
            }
            else
            {
                ViewBag.PenDriveAcessList = new SelectList(new[] { new { Text = "Yes", Value = "Yes" }, new { Text = "No", Value = "No" } }, "Text", "Value", "Yes");
            }

            var UpsRef = db.Assets.FirstOrDefault(o => o.Name_Model_No == obj.UPSID);

            var s = (from at in db.AssetTypes
                     join ast in db.Assets on at.AssetTypeId equals ast.AssetTypeId
                     where at.AssetName != "UPS" && ast.ISDelete == false && ast.ComputerName == 0
                     select new
                     {
                         name = ast.Name_Model_No,
                         id = ast.AssetID,  
                     }).ToList();

            var w = (from at in db.AssetTypes
                     join ast in db.Assets on at.AssetTypeId equals ast.AssetTypeId
                     where at.AssetName == "UPS" && ast.ISDelete == false
                     select new
                     {
                         Name_Model_No = ast.Name_Model_No,
                         AssetID = ast.AssetID,
                     }).ToList();

            if (UpsRef != null)
            {
                for (int i = 0; i < w.Count; i++)
                {
                    var item = w[i];
                    if (item.AssetID == UpsRef.AssetID)
                    {
                        w.Remove(item);
                        w.Insert(0, item);
                    }
                }
                ViewBag.abc = true;
                ViewBag.UpsList = new SelectList(w, "AssetID", "Name_Model_No");
            }
            else
            {
                ViewBag.abc = false;
                ViewBag.UpsList = new SelectList(w, "AssetID", "Name_Model_No");

            }

            List<int> list = new List<int>();
            list.Add(Convert.ToInt32(obj.CompId));
            ViewBag.ComponentIdList = new MultiSelectList(s, "id", "name", obj.Component);
            var LocationList = db.locations.ToList();
            ViewBag.LocationIDList = new SelectList(LocationList, "locationid", "LocationName", obj.LocationId);
            var LstCompAssigned = db.ComputerAssigneds.Select(x => x.Managementid).ToList();
            var ComputerNameList = db.computermanagements.Where(x=>x.ISDelete == false).ToList();
            ViewBag.ManagementIdList = new SelectList(ComputerNameList, "managementid", "ComputerName", obj.Managementid);
            return View(obj);
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

        public ActionResult EditCmptr(string ID, string UserId, string Location, string Managementid, string ComputerName, string Pendrive, string UPSID, string ComponentId, string WorkstationNumber, string DepartmentName, string locationid, string WorkstationName)
        {
            try{
            int? id = Convert.ToInt32(ID);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var Edobj = objdb.ComputerAssigneds.Where(o => o.Assignid == id ).FirstOrDefault();
            var deptid = objdb.Departments.FirstOrDefault(o => o.DepartmentName == DepartmentName).DepartmentId;

            Edobj.Userid = Convert.ToInt32(UserId);
            Edobj.Departmentid = deptid;
            Edobj.Locationid = Convert.ToInt32(locationid);
            Edobj.Managementid = Convert.ToInt32(Managementid);
            if (Pendrive == "Yes")
            {
                Edobj.pendriveAccessnew = false;
            }
            else
            {
                Edobj.pendriveAccessnew = true;

            }
            Edobj.UPSID = UPSID;
            Edobj.WorkStation = WorkstationNumber;
            objdb.SaveChanges();

            var item = db.UserAssetMappings.Where(i => i.AssignId == id ).ToList();
            string[] splitComponentId = ComponentId.ToString().Split(',');
            int count = 0;

            if (ComponentId != "")
            {
                foreach (var s in splitComponentId)
                {
                    int assetid = 0;
                    if (count < item.Count)
                    {
                        for (int j = 0; j < item.Count; j++)
                        {
                            assetid = Convert.ToInt32(item[count].AssetID);

                        }
                    }
                    if (s.Length > 0)
                    {

                        int updateAssetID = Convert.ToInt32(s);
                        var UserAssetObj = db.UserAssetMappings.Where(m => m.AssignId == id).FirstOrDefault();

                        UserAssetObj.AssetID = updateAssetID;
                        UserAssetObj.Component = ComponentId;
                        TryUpdateModel(UserAssetObj);
                        db.SaveChanges();
                    }
                    count++;
                }
            }

            else
            {

                foreach (var s in splitComponentId)
                {
                    int assetid = 0;
                    if (count < item.Count)
                    {
                        for (int j = 0; j < item.Count; j++)
                        {
                            assetid = Convert.ToInt32(item[count].AssetID);

                        }
                    }
                    

                        //int updateAssetID = Convert.ToInt32(s);
                        var UserAssetObj = db.UserAssetMappings.Where(m => m.AssignId == id).FirstOrDefault();

                        UserAssetObj.AssetID = assetid;
                        UserAssetObj.Component = ComponentId;
                        TryUpdateModel(UserAssetObj);
                        db.SaveChanges();
                   
                    count++;
                }


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

        public void UserDetail()
        {
            var NameList = new List<SelectListItem>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var query = db.Users.ToList();
                List<DSRCEmployees> Names = (from data in db.Users
                                             where data.IsActive == true && data.BranchId == 1 && data.UserStatus != 6
                                             select new DSRCEmployees
                                             {
                                                 Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                                 UserId = data.UserID,
                                                 EmployeeId = data.EmpID
                                             }).OrderBy(x => x.Name).ToList();

                foreach (var item in Names)
                {
                    NameList.Add(new SelectListItem { Text = item.Name, Value = item.UserId.ToString() });
                }


            }

            ViewBag.Detail3 = NameList;
        }






        //public void User()
        //{

        //    var query = db.Users.ToList();
        //    IList<SelectListItem> usr = new List<SelectListItem>();
        //    foreach (var a in query)
        //    {
        //        usr.Add(new SelectListItem { Value = Convert.ToString(a.UserID), Text = a.FirstName + " " + a.LastName });
        //    }
        //    ViewBag.Detail3 = usr;
        //}


        //public void User()
        //{
        //    var query = (from p in db.Users
        //                 select new
        //                 {
        //                     UserId = p.UserID,
        //                     UserName = p.FirstName + "." + p.LastName


        //                 }).ToList();
        //    ViewBag.Detail3 = new SelectList(query, "UserId", "UserName");
        //}




        [HttpPost]
        public ActionResult DeleteCmptr(int ID)
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var obj1 = db.ComputerAssigneds.Where(x => x.Assignid == ID).FirstOrDefault();
            obj1.ISDelete = true;
            TryUpdateModel(obj1);
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
    }

}
