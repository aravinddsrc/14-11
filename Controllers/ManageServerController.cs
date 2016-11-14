using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;

namespace DSRCManagementSystem.Controllers
{
    public class ManageServerController : Controller
    {

        public ActionResult ManageServer()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ManageServer ObjAC = new DSRCManagementSystem.Models.ManageServer();
            List<DSRCManagementSystem.Models.ManageServer> AsgnList = new List<DSRCManagementSystem.Models.ManageServer>();

            AsgnList = (from a in db.ManageServers
                        where a.ISDelete == false && a.Raid == true
                        join l in db.locations on a.LocationId equals l.locationid
                        join o in db.Master_ServerOs on a.OperatingSystem equals o.ServerOsName
                       // join c in db.Cpus on a.Processor equals c.CpuName
                        select new DSRCManagementSystem.Models.ManageServer()
                        {
                            RackNo = a.RackNo,
                            MachineName = a.MachineName,
                            Location = l.LocationName,
                            ServerMake = a.ServerMake,
                            Model = a.Model,
                            Processor = a.Processor,
                            Memory = a.Memory,
                            HardDisks = a.HardDisks,
                            Raid = a.Raid,
                            Configurationdetails = a.Configurationdetails,
                            OperatingSystem = o.ServerOsName,
                            ServerEdition = a.ServerEdition,
                            NameofProjectsHosted = a.NameofProjectsHosted,
                            ID = a.ManageServers_Id
                        }).ToList();

            return View(AsgnList.ToList());
        }

        [HttpPost]
        public ActionResult ManageServer(DSRCManagementSystem.Models.ManageServer model,FormCollection form)
        {
            List<DSRCManagementSystem.Models.ManageServer> AsgnList = new List<DSRCManagementSystem.Models.ManageServer>();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            bool status = form["Inactive"].Contains("true");
            if (status == true)
            {

                AsgnList = (from a in db.ManageServers
                            where a.ISDelete == false && a.Raid == false
                            join l in db.locations on a.LocationId equals l.locationid
                            join o in db.Master_ServerOs on a.OperatingSystem equals o.ServerOsName
                            //join c in db.Cpus on a.Processor equals c.CpuName

                            select new DSRCManagementSystem.Models.ManageServer()
                            {
                                RackNo = a.RackNo,
                                MachineName = a.MachineName,
                                Location = l.LocationName,
                                ServerMake = a.ServerMake,
                                Model = a.Model,
                                Processor = a.Processor,
                                Memory = a.Memory,
                                HardDisks = a.HardDisks,
                                Raid = a.Raid,
                                Configurationdetails = a.Configurationdetails,
                                OperatingSystem = o.ServerOsName,
                                ServerEdition = a.ServerEdition,
                                NameofProjectsHosted = a.NameofProjectsHosted,
                                ID = a.ManageServers_Id
                            }).ToList();
            }
            else
            {
                AsgnList = (from a in db.ManageServers
                            where a.ISDelete == false && a.Raid == true
                            join l in db.locations on a.LocationId equals l.locationid
                            join o in db.Master_ServerOs on a.OperatingSystem equals o.ServerOsName
                            //join c in db.Cpus on a.Processor equals c.CpuName

                            select new DSRCManagementSystem.Models.ManageServer()
                            {
                                RackNo = a.RackNo,
                                MachineName = a.MachineName,
                                Location = l.LocationName,
                                ServerMake = a.ServerMake,
                                Model = a.Model,
                                Processor = a.Processor,
                                Memory = a.Memory,
                                HardDisks = a.HardDisks,
                                Raid = a.Raid,
                                Configurationdetails = a.Configurationdetails,
                                OperatingSystem = o.ServerOsName,
                                ServerEdition = a.ServerEdition,
                                NameofProjectsHosted = a.NameofProjectsHosted,
                                ID = a.ManageServers_Id
                            }).ToList();
            }
            return View(AsgnList);

        }


        [HttpGet]


        public ActionResult AddNew()
        {
            var userId = (int)Session["UserId"];
            ViewBag.Raid = new SelectList(new[] { new { Text = "No", Value = 0 }, new { Text = "Yes", Value = 1 } }, "Value", "Text", 0);
            ViewBag.Reporting = new SelectList(GetReportingPersons(userId), "UserId", "Name");
            project();
            location();
            os();
           // cpu();

            return View();

        }
        public void project()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var query = db.Projects.ToList();
            IList<SelectListItem> pro = new List<SelectListItem>();
            foreach (var a in query)
            {
                pro.Add(new SelectListItem { Value = Convert.ToString(a.ProjectID), Text = a.ProjectName });
            }
            ViewBag.Detail2 = pro;
        }

        private List<ReportingPerson> GetReportingPersons(int id = 0)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                List<ReportingPerson> reportingPersons = (from r in db.Master_Roles
                                                          join ur in db.UserRoles on r.RoleID equals ur.RoleID
                                                          join u in db.Users on ur.UserID equals u.UserID
                                                          where ur.UserID==10 || ur.UserID== 49|| ur.UserID ==48
                                                          || ur.UserID==56|| ur.UserID==47|| ur.UserID==57
                                                          select new ReportingPerson
                                                          {
                                                              UserID = u.UserID,
                                                              Name = (u.FirstName + " " + (u.LastName ?? "")).Trim()
                                                          }).OrderBy(o => o.Name).ToList();
                //reportingPersons.RemoveAll(x => x.UserID == userID);
                return reportingPersons;
            }
        }
        public void os()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var query1 = db.Master_ServerOs.ToList();
            IList<SelectListItem> osn = new List<SelectListItem>();
            foreach (var a in query1)
            {
                osn.Add(new SelectListItem { Value = Convert.ToString(a.ServerOsId), Text = a.ServerOsName });
            }
            ViewBag.osn1 = osn;
        }
        //public void cpu()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    var query2 = db.Cpus.ToList();
        //    IList<SelectListItem> cpun = new List<SelectListItem>();
        //    foreach (var a in query2)
        //    {
        //        cpun.Add(new SelectListItem { Value = Convert.ToString(a.CpuIdNew), Text = a.CpuName });
        //    }
        //    ViewBag.cpu1 = cpun;
        //}
        public void location()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var query = db.locations.ToList();
            IList<SelectListItem> loc = new List<SelectListItem>();
            foreach (var a in query)
            {

                loc.Add(new SelectListItem { Value = Convert.ToString(a.locationid), Text = a.LocationName });
            }
            ViewBag.Detail1 = loc;
        }

        [HttpPost]
        public ActionResult AddNew(DSRCManagementSystem.Models.ManageServer modelObj, List<int> ProjectList, List<int> AssignedTo,int Raid)
        {
            DSRCManagementSystem.Models.ManageServer obj_Assign = new DSRCManagementSystem.Models.ManageServer();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var OsList = db.Master_ServerOs.ToList();
            var LocationList = db.locations.ToList();
          //  var cpulist = db.Cpus.ToList();
            // var ProjectList = db.Projects.ToList();
            // int j = ProjectList.Count();
            string temp = "";

            //for (int i = 0; i < j; i++)
            // {
            //   temp += ProjectList[i] + ";";
            // }

            foreach (int i in ProjectList)
            {
                temp += i;

            }

            string temp1 = "";

            foreach (var i in AssignedTo)
            {
                temp1 += i;
            }

            ViewBag.Raid = new SelectList(new[] { new { Text = "No", Value = 0 }, new { Text = "Yes", Value = 1 } }, "Value", "Text", 0);
            ViewBag.LocationList = new SelectList(new[] { new location() { locationid = 0, LocationName = "---Select---" } }.Union(LocationList), "locationid", "LocationName", 0);
            ViewBag.OsList = new SelectList(new[] { new Master_ServerOs() { ServerOsId = 0, ServerOsName = "---Select---" } }.Union(OsList), "ServerOsId", "ServerOsName", 0);
           // ViewBag.cpulist = new SelectList(new[] { new Cpu() { CpuIdNew = 0, CpuName = "---Select---" } }.Union(cpulist), "CpuIdNew", "CpuName", 0);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var already = objdb.ManageServers.Where(o => o.MachineName == modelObj.MachineName).Select(o => o).FirstOrDefault();
            var already1 = objdb.ManageServers.Where(o => o.OtherProjects == modelObj.OtherProjects).Select(o => o).FirstOrDefault();
            if (already != null)
            {
                return Json(new { Result = "AlreadyExist", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                
            }
            else if (already1 != null)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
                
              
            else
            {

                var Asgnobj = db.ManageServers.CreateObject();
                    Asgnobj.LocationId = modelObj.LocationId;
                    Asgnobj.MachineName = modelObj.MachineName.Trim();
                    Asgnobj.ServerMake = modelObj.ServerMake.Trim();
                    Asgnobj.Model = modelObj.Model;
                    Asgnobj.Processor = modelObj.Processor.Trim();
                    Asgnobj.Memory = modelObj.Memory;
                    Asgnobj.HardDisks = modelObj.HardDisks;
                    Asgnobj.Raid = Raid == 0 ? false : true;
                    Asgnobj.Configurationdetails = modelObj.Configurationdetails.Trim();
                    Asgnobj.OperatingSystem = modelObj.ServerOsName;
                    Asgnobj.ServerEdition = modelObj.ServerEdition.Trim();
                    //Asgnobj.NameofProjectsHosted = modelObj.NameofProjectsHosted;
                    Asgnobj.NameofProjectsHosted = temp;
                    Asgnobj.Assignedto = temp1;
                    //Asgnobj.AssignedtoUserId = Convert.ToInt32(temp1.Trim(new Char[] { ' ', '*', '.', ';' }));
                    Asgnobj.ISDelete = false;
                    Asgnobj.AssetId = modelObj.AssetId.Trim();
                    Asgnobj.RackNo = modelObj.RackNo;
                    Asgnobj.OtherProjects = modelObj.OtherProjects;
                    db.ManageServers.AddObject(Asgnobj);
                    db.SaveChanges();
                
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }




        [HttpGet]
        public ActionResult EditServer(int ID)
        {
             var userId = (int)Session["UserId"];
            ViewBag.Raid = new SelectList(new[] { new { Text = "No", Value = 0 }, new { Text = "Yes", Value = 1 } }, "Value", "Text", 0);
            project();

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var AsgnList = (from a in db.ManageServers
                            where a.ManageServers_Id== ID
                            join l in db.locations on a.LocationId equals l.locationid
                            join o in db.Master_ServerOs on a.OperatingSystem equals o.ServerOsName
                            //join c in db.Cpus on a.Processor equals c.CpuName

                            select new DSRCManagementSystem.Models.ManageServer()
                            {
                                ID = a.ManageServers_Id,
                                RackNo = a.RackNo,
                                MachineName = a.MachineName,
                                Location = l.LocationName,
                                LocationId = l.locationid,
                                ServerMake = a.ServerMake,
                                Model = a.Model,
                                Processor = a.Processor,
                               // CpuIdNew = c.CpuIdNew,
                                Memory = a.Memory,
                                HardDisks = a.HardDisks,
                                Raid = a.Raid,
                                Configurationdetails = a.Configurationdetails,
                                OperatingSystem = o.ServerOsName,
                                ServerOsid = o.ServerOsId,
                                ServerEdition = a.ServerEdition,
                                NameofProjectsHosted = a.NameofProjectsHosted,
                                OtherProjects=a.OtherProjects,
                                Assignedto = a.Assignedto,
                                AssetId = a.AssetId
                            }).FirstOrDefault();

            if (AsgnList.Raid == true)
            {
                ViewBag.InUse = new SelectList(new[] { new { Text = "Yes", Value = "Yes" }, new { Text = "No", Value = "No" } }, "Text", "Value", "Yes");
            }
            else
            {
                ViewBag.InUse = new SelectList(new[] { new { Text = "No", Value = "No" }, new { Text = "Yes", Value = "Yes" } }, "Text", "Value", "No");

            }

            var ProjectList = db.Projects.ToList();

            //var ProjectList = (from
            //               tl in db.Projects
            //                   select new
            //                   {
            //                       ProjectID = tl.ProjectID,
            //                       ProjectName = tl.ProjectName
            //                   }).ToList();
            // var ProjectList = db.Projects.ToList();
            //ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", AsgnList.ProjectId);
            List<int> selectedProjects = new List<int>();
            if (AsgnList.NameofProjectsHosted != null)
            {

                string[] tokens = AsgnList.NameofProjectsHosted.Split(new string[] { ";" }, StringSplitOptions.None);
                foreach (var i in tokens)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedProjects.Add(val);
                }
            }
            List<int> selectedAssigned = new List<int>();
            if (AsgnList.Assignedto != null)
            {

                string[] tokens1 = AsgnList.Assignedto.Split(new string[] { ";" }, StringSplitOptions.None);
                foreach (var i in tokens1)
                {
                    int val;
                    int.TryParse(i, out val);
                    selectedAssigned.Add(val);
                }
            }

            //   var t = db.Projects.Select(o=>o.ProjectID).Take(10).ToList();
            ViewBag.ProjectIDList = new MultiSelectList(ProjectList, "ProjectID", "ProjectName", selectedProjects);
            var LocationList = db.locations.ToList();
            ViewBag.Reporting = new MultiSelectList(GetReportingPersons(userId), "UserId", "Name", selectedAssigned);
            ViewBag.LocationIDList = new SelectList(LocationList, "locationid", "LocationName");
            var OsList = db.Master_ServerOs.ToList();
            ViewBag.OsIdList = new SelectList(OsList, "ServerOsId", "ServerOsName");
           // var cpulist = db.Cpus.ToList();
            //ViewBag.cpuidlist = new SelectList(cpulist, "CpuIdNew", "CpuName");
            return View(AsgnList);
        }





        [HttpPost]
        public ActionResult EditServer(DSRCManagementSystem.Models.ManageServer modelObj, List<int> ProjectList, List<int> AssignedTo)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var userId = (int)Session["UserId"];
            ViewBag.Raid = new SelectList(new[] { new { Text = "No", Value = 0 }, new { Text = "Yes", Value = 1 } }, "Value", "Text", 0);
            var LocationList = db.locations.ToList();
            ViewBag.LocationIDList = new SelectList(LocationList, "locationid", "LocationName");
            var OsList = db.Master_ServerOs.ToList();
            ViewBag.OsIdList = new SelectList(OsList, "ServerOsId", "ServerOsName");
            ViewBag.Reporting = new SelectList(GetReportingPersons(userId), "UserId", "Name");
           
            //project();
           
            //if (!ModelState.IsValid)
            //{

                string temp = "";

                //for (int i = 0; i < j; i++)
                // {
                //   temp += ProjectList[i] + ";";
                // }

                if (ProjectList != null)
                {
                    foreach (int i in ProjectList)
                    {
                        temp += i +";";

                    }
                }
                string temp1 = "";
                if (AssignedTo != null)
                {
                    foreach (var i in AssignedTo)
                    {
                        temp1 += i +";";
                    }
                }
                using (DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1())

                {
                    var obj_server = objdb.ManageServers.Where(o => o.ManageServers_Id== modelObj.ID).Select(o => o).FirstOrDefault();
                    obj_server.LocationId = modelObj.LocationId;
                    obj_server.RackNo = modelObj.RackNo;
                    obj_server.AssetId = modelObj.AssetId;
                    obj_server.MachineName = modelObj.MachineName;
                    obj_server.ServerMake = modelObj.ServerMake;
                    obj_server.Model = modelObj.Model;
                    obj_server.Processor = modelObj.Processor;
                    obj_server.Memory = modelObj.Memory;
                    obj_server.HardDisks = modelObj.HardDisks;
                    obj_server.Raid = Convert.ToInt32(modelObj.Raid) == 0 ? true : false;
                    obj_server.Configurationdetails = modelObj.Configurationdetails;
                    obj_server.OperatingSystem = modelObj.ServerOsName;
                    obj_server.ServerEdition = modelObj.ServerEdition;
                    obj_server.NameofProjectsHosted = temp;
                    obj_server.Assignedto = temp1;
                    obj_server.OtherProjects = modelObj.OtherProjects;
                    objdb.SaveChanges();
                    List<int> selectedProjects = new List<int>();
                    if (obj_server.NameofProjectsHosted != null)
                    {

                        string[] tokens = obj_server.NameofProjectsHosted.Split(new string[] { ";" }, StringSplitOptions.None);
                        foreach (var i in tokens)
                        {
                            int val;
                            int.TryParse(i, out val);
                            selectedProjects.Add(val);
                        }
                    }
                    ViewBag.ProjectIDList = new MultiSelectList(ProjectList, "ProjectID", "ProjectName", selectedProjects);
                }
               

                return Json("Success", JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return View();
            //}
        }







        [HttpPost]
        public ActionResult Delete(int ID)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var record = db.ManageServers.FirstOrDefault(x => x.ManageServers_Id == ID);
                record.ISDelete = true;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);



            }
        }


    }
}



