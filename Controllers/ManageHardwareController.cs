using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;

namespace DSRCManagementSystem.Controllers
{
    public class ManageHardwareController : Controller
    {

        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult ManageHardware()
        {
            Asset1();
            ManageHardwareModel rec = new ManageHardwareModel();


            rec.HardwareListCollection = (from a in db.Assets
                                          where a.InUse == true && a.ISDelete == false
                                          join b in db.locations on a.LocationID equals b.locationid
                                          into Bord1
                                          from tab2 in Bord1.DefaultIfEmpty()
                                          join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                                          join d in db.computermanagements on a.ComputerName equals d.managementid
                                          into Bord
                                          from tab1 in Bord.DefaultIfEmpty()
                                          select new HardwareList()
                                          {
                                              ID = a.AssetID,
                                              Hardware = c.AssetName,
                                              Component = a.Name_Model_No,
                                              Model = a.ModelNo,
                                              AssignedTo = a.ComputerName == 0 ? "Not Assigned" : tab1.ComputerName,
                                              Floor = a.LocationID == 0 ? "Not Allocated" : tab2.LocationName,
                                              Quantity = a.Quantity,
                                              Ip = a.ConnectedTo,
                                              InUse = a.InUse
                                          }).ToList();
            
            return View(rec);
        }        
        




        [HttpPost]
        public ActionResult ManageHardware(ManageHardwareModel HW,FormCollection form)
        {
            Asset1();

            bool status = form["NotInUse"].Contains("true");

            int DropHardware = Convert.ToInt32(HW.ID);
            if (DropHardware == 0)
            {
                if (status == true)
                {

                    HW.HardwareListCollection = (from a in db.Assets where a.InUse == false && a.ISDelete == false
                                                 join b in db.locations on a.LocationID equals b.locationid
                                                 into Bord1 from tab2 in Bord1.DefaultIfEmpty()
                                                 join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                                                 join d in db.computermanagements on a.ComputerName equals d.managementid
                                                 into Bord from tab1 in Bord.DefaultIfEmpty()
                                                 select new HardwareList()
                                                 {
                                                     ID = a.AssetID,
                                                     Hardware = c.AssetName,
                                                     Component = a.Name_Model_No,
                                                     Model = a.ModelNo,
                                                     AssignedTo = a.ComputerName == 0 ? "Not Assigned" : tab1.ComputerName,
                                                     Floor = a.LocationID == 0 ? "Not Allocated" : tab2.LocationName,
                                                     Quantity = a.Quantity,
                                                     Ip = a.ConnectedTo,
                                                     InUse = a.InUse
                                                 }).ToList();
                }

                else
                {
                    HW.HardwareListCollection = (from a in db.Assets
                                                 where a.InUse == true && a.ISDelete == false
                                                 join b in db.locations on a.LocationID equals b.locationid
                                                 into Bord1 from tab2 in Bord1.DefaultIfEmpty()
                                                 join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                                                 join d in db.computermanagements on a.ComputerName equals d.managementid
                                                 into Bord from tab1 in Bord.DefaultIfEmpty()
                                                 select new HardwareList()
                                                 {
                                                     ID = a.AssetID,
                                                     Hardware = c.AssetName,
                                                     Component = a.Name_Model_No,
                                                     Model = a.ModelNo,
                                                     AssignedTo = a.ComputerName == 0 ? "Not Assigned" : tab1.ComputerName,
                                                     Floor = a.LocationID == 0 ? "Not Allocated" : tab2.LocationName,
                                                     Quantity = a.Quantity,
                                                     Ip = a.ConnectedTo,
                                                     InUse = a.InUse
                                                 }).ToList();
                }
            }
            else
            {
                if (status == true)
                {
                    HW.HardwareListCollection = (from a in db.Assets
                                                 where a.InUse == false
                                                 join b in db.locations on a.LocationID equals b.locationid
                                                 into Bord1 from tab2 in Bord1.DefaultIfEmpty()
                                                 join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                                                 join d in db.computermanagements on a.ComputerName equals d.managementid
                                                 into Bord from tab1 in Bord.DefaultIfEmpty()
                                                 where (a.ISDelete == false && a.AssetTypeId == DropHardware)
                                                 select new HardwareList()
                                                 {
                                                     ID = a.AssetID,
                                                     Hardware = c.AssetName,
                                                     Component = a.Name_Model_No,
                                                     Model = a.ModelNo,
                                                     AssignedTo = a.ComputerName == 0 ? "Not Assigned" : tab1.ComputerName,
                                                     Floor = a.LocationID == 0 ? "Not Allocated" : tab2.LocationName,
                                                     Quantity = a.Quantity,
                                                     Ip = a.ConnectedTo,
                                                     InUse = a.InUse
                                                 }).ToList();
                }
                else
                {
                    HW.HardwareListCollection = (from a in db.Assets
                                                 where a.InUse == true
                                                 join b in db.locations on a.LocationID equals b.locationid
                                                 into Bord1 from tab2 in Bord1.DefaultIfEmpty()
                                                 join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                                                 join d in db.computermanagements on a.ComputerName equals d.managementid
                                                 into Bord from tab1 in Bord.DefaultIfEmpty()
                                                 where (a.ISDelete == false && a.AssetTypeId == DropHardware)
                                                 select new HardwareList()
                                                 {
                                                     ID = a.AssetID,
                                                     Hardware = c.AssetName,
                                                     Component = a.Name_Model_No,
                                                     Model = a.ModelNo,
                                                     AssignedTo = a.ComputerName == 0 ? "Not Assigned" : tab1.ComputerName,
                                                     Floor = a.LocationID == 0 ? "Not Allocated" : tab2.LocationName,
                                                     Quantity = a.Quantity,
                                                     Ip = a.ConnectedTo,
                                                     InUse = a.InUse
                                                 }).ToList();
                }
            }
            return View(HW);
        }



        public ActionResult AddComponent()
        {
            try
            {
                Asset1();
                location1();
                ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = 0 }, new { Text = "No", Value = 1 } }, "Value", "Text", 0);
                var ComputerNameList = db.computermanagements.Where(x => x.ISDelete == false && x.ComputerStatusNew == "Active").ToList();
                ViewBag.ManagementIdList = new SelectList(ComputerNameList, "managementid", "ComputerName");
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
        public ActionResult AddComponent(string Hardware, string Floor, string Component, string Model, string AssignedTo, int Quantity, string Ip, int InUse)
        {
            
                ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = 0 }, new { Text = "No", Value = 1 } }, "Value", "Text", 0);

                var ComputerNameList = db.computermanagements.ToList();
                ViewBag.ManagementIdList = new SelectList(new[] { new computermanagement() { managementid = 0, ComputerName = "---select---" } }.Union(ComputerNameList), "managementid", "ComputerName", 0);
                int hardwa = Convert.ToInt32(Hardware);
                int assign = 0, getAsset = 0;
                if (AssignedTo != "")
                {
                    assign = Convert.ToInt32(AssignedTo);
                    getAsset = db.Assets.Where(s => s.AssetTypeId == hardwa && s.ComputerName == assign && s.ISDelete == false).Select(s => s.AssetID).FirstOrDefault();
                }
                else
                {
                    AssignedTo = null;
                }
                var CompExist = db.Assets.Where(a => a.Name_Model_No == Component).Select(a => a.AssetID).FirstOrDefault();
                if (CompExist > 0)
                {
                    return Json("ComponentExist", JsonRequestBehavior.AllowGet);
                }
                if (getAsset > 0)
                {
                    return Json("AlreadyAssigned", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var compobjaset = db.Master_Component.CreateObject();
                    var CompoID = db.Master_Component.Where(x => x.Description == Component).Select(x => x.ID).FirstOrDefault();
                    if (CompoID == 0)
                    {
                        compobjaset.Description = Component;

                        db.Master_Component.AddObject(compobjaset);
                        db.SaveChanges();
                        CompoID = db.Master_Component.Where(x => x.Description == Component).Select(x => x.ID).FirstOrDefault();
                    }

                    var objaset = db.Assets.CreateObject();
                    objaset.AssetTypeId = Convert.ToInt32(Hardware);
                    objaset.Name_Model_No = Component;
                    objaset.LocationID = Floor == "" ? 0 : Convert.ToInt32(Floor);
                    objaset.Quantity = Quantity;
                    objaset.ComputerName = Convert.ToInt32(AssignedTo);
                    objaset.ModelNo = Model;
                    objaset.ConnectedTo = Ip;
                    objaset.ISDelete = false;
                    objaset.InUse = InUse == 0 ? true : false;
                    objaset.ComponentID = CompoID;
                    db.Assets.AddObject(objaset);
                    db.SaveChanges();
                    return View(objaset);
                }
      
          
            //return RedirectToAction("ManageHardware", "ManageHardware");
            
        }




        public ActionResult EditComponent(int ID)
        {

            location1();
            ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = 0 }, new { Text = "No", Value = 1 } }, "Value", "Text", 0);

            var obj = (from a in db.Assets.Where(o => o.AssetID == ID)
                join b in db.locations on a.LocationID equals b.locationid
                into Bord1 from tab2 in Bord1.DefaultIfEmpty()
                join c in db.AssetTypes on a.AssetTypeId equals c.AssetTypeId
                join d in db.computermanagements on a.ComputerName equals d.managementid
                into Bord from tab1 in Bord.DefaultIfEmpty()
                       select new HardwareList()
                       {
                           Hardware = c.AssetName,
                           lid = tab2.locationid,
                           AssignedTo = tab1.ComputerName,
                           Floor = tab2.LocationName,
                           Component = a.Name_Model_No,
                           Model = a.ModelNo,
                           mid = tab1.managementid,
                           Quantity = a.Quantity,
                           Ip = a.ConnectedTo,
                           InUse = a.InUse,
                           isdelete = true,
                           Id = ID,
                           AssignedToId = a.ComputerName
                       }).FirstOrDefault();

             var Deparmentgroup = (from p in db.DepartmentGroups.Where(x => x.IsActive == true)
                                  select new
                                  {
                                      GroupId = p.GroupID,
                                      GroupName = p.GroupName
                                  }).ToList();
             var query = (from l in db.locations
                                      select new
                                  {
                                      locationid = l.locationid,
                                      LocationName = l.LocationName

                                  }).ToList();
             ViewBag.Detail1 = new SelectList(query, "locationid", "LocationName",obj.lid);


            for (int i = 0; i < query.Count; i++)
            {
                var item = query[i];
                if (item.locationid == obj.lid)
                {
                    query.Remove(item);
                    query.Insert(0, item);
                }
            }

            var ComputerNameList = (from m in db.computermanagements.Where(z => z.ISDelete == false && z.ComputerStatusNew == "Active")
                                    select new
                                    {
                                        managementid = m.managementid,
                                        ComputerName = m.Manufacturer

                                    }).ToList();
            ViewBag.ManagementIdList = new SelectList(ComputerNameList, "managementid", "ComputerName", obj.AssignedToId);

            if (obj.InUse == true)
            {
                ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = "Yes" }, new { Text = "No", Value = "No" } }, "Text", "Value", "Yes");
            }
            else
            {
                ViewBag.InUse = new SelectList(new[] { new { Text = "No", Value = "No" }, new { Text = "Yes", Value = "Yes" } }, "Text", "Value", "No");

            }

            return View(obj);
        }


        [HttpPost]
        public ActionResult EditComponent(int Floor, int Id, string Component, string Model, int AssignedTo, int Quantity, string Ip, string InUse)
        {

            ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = 0 }, new { Text = "No", Value = 1 } }, "Value", "Text", 0);


            var record = db.Assets.FirstOrDefault(x => (x.AssetID == Id));
            try
            {
                if (record != null)
                {
                    record.LocationID = Floor;
                    record.Name_Model_No = Component;
                    record.ModelNo = Model;
                    record.Quantity = Quantity;
                    record.ComputerName = AssignedTo;
                    record.ConnectedTo = Ip;
                    record.InUse = InUse == "Yes" ? true : false;
                    db.SaveChanges();
                }
                var query = db.locations.ToList();
                ViewBag.Detail1 = new SelectList(query, "locationid", "LocationName");
                var ComputerNameList = db.computermanagements.Where(z => z.ISDelete == false && z.ComputerStatusNew == "Active").ToList();
                ViewBag.ManagementIdList = new SelectList(ComputerNameList, "managementid", "ComputerName");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(record);

        }






        public void Asset1()
        {
            try
            {
                var query = db.AssetTypes.ToList();
                IList<SelectListItem> dropcomp = new List<SelectListItem>();
                foreach (var s in query)
                {
                    dropcomp.Add(new SelectListItem { Value = Convert.ToString(s.AssetTypeId), Text = s.AssetName });
                }
                ViewBag.Details = dropcomp;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
        }


        public void location1()
        {
            try
            {
                var query = db.locations.ToList();
                IList<SelectListItem> loc = new List<SelectListItem>();
                foreach (var a in query)
                {
                    loc.Add(new SelectListItem { Value = Convert.ToString(a.locationid), Text = a.LocationName });
                }
                ViewBag.Detail1 = loc;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
        }


        [HttpGet]
        public ActionResult Delete(int ID)
        {

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var obj = db.Assets.FirstOrDefault(x => x.AssetID == ID);
                obj.ISDelete = true;
                db.SaveChanges();
                var obj1 = db.Assets.Where(x1 => x1.AssetID == ID).Select(s => s.Name_Model_No).FirstOrDefault();
                string va = Convert.ToString(obj1);
                var obj2 = db.Assets.Where(c => c.Name_Model_No == va).ToList();
                foreach (var items in obj2)
                {
                    var update = db.Assets.Where(x => x.AssetID == items.AssetID).FirstOrDefault();
                    update.Name_Model_No = null;
                    TryUpdateModel(update);
                    db.SaveChanges();
                }
                bool exis = db.ComputerAssigneds.Any(s => s.UPSID == obj1);
                if (exis)
                {
                    var updateComputer = db.ComputerAssigneds.Where(m => m.UPSID == obj1).ToList();
                    foreach (var items in updateComputer)
                    {
                        var update = db.ComputerAssigneds.Where(m => m.Assignid == items.Assignid).FirstOrDefault();
                        update.UPSID = null;
                        TryUpdateModel(update);
                        db.SaveChanges();

                    }
                }
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }

        }

    }
}

