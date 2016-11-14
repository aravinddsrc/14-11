using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace DSRCManagementSystem.Controllers
{
    public class ManageDriversController : Controller
    {
       
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                      //---------------View Drivers

        [HttpGet]
        public ActionResult Manage()
        {
            List<DSRCManagementSystem.Models.ManageDrivers> objmodel = new List<DSRCManagementSystem.Models.ManageDrivers>();


            objmodel = (from drivers in db.Drivers
                              join drivertype in db.Master_DriverType on drivers.DriverType_Id equals drivertype.DriverType_Id
                              join gender in db.Master_Gender on drivers.Gender equals gender.GenderID into x
                              from y in x.DefaultIfEmpty()
                        where drivers.IsActive != false
                              select new DSRCManagementSystem.Models.ManageDrivers()
                              {
                                  DriverId= drivers.DriverId,
                                  DriverType_Id = drivers.DriverId,
                                  DriverName = (drivers.First_Name) + " " + (drivers.Last_Name ?? " "),
                                  DriverType=drivertype.DriverType,
                                  Genders = y.GenderName,
                                  Contact_No = drivers.Contact_No
                              }).ToList();
           
            var DriverName = db.Master_DriverType.ToList();
            ViewBag.DriverTypeList = new SelectList(DriverName, "DriverType_Id", "DriverType");
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult Manage(FormCollection forms)            
        {   
            string DriverType = (forms["DriverType"] == "") ? "0" : forms["DriverType"].ToString();
            int DriverTypeID = int.Parse(DriverType.ToString());

            List<DSRCManagementSystem.Models.ManageDrivers> objmodel = new List<DSRCManagementSystem.Models.ManageDrivers>();

            if (DriverTypeID == 0)
            {
                return RedirectToAction("Manage", "ManageDrivers");
            }
            else
            {
                 objmodel= (from drivers in db.Drivers
                                  join drivertype in db.Master_DriverType on drivers.DriverType_Id equals drivertype.DriverType_Id
                                  join gender in db.Master_Gender on drivers.Gender equals gender.GenderID into x
                                  from y in x.DefaultIfEmpty()
                            where drivertype.DriverType_Id == DriverTypeID && drivers.IsActive != false
                                  select new DSRCManagementSystem.Models.ManageDrivers()
                                  {
                                      DriverId = drivers.DriverId,
                                      DriverType_Id = drivers.DriverId,
                                      DriverName = (drivers.First_Name) + " " + (drivers.Last_Name ?? " "),
                                      DriverType = drivertype.DriverType,
                                      Genders = y.GenderName,
                                      Contact_No = drivers.Contact_No
                                  }).ToList();

            }

            var DriverName = db.Master_DriverType.ToList();
            ViewBag.DriverTypeList = new SelectList(DriverName, "DriverType_Id", "DriverType", DriverTypeID);

            return View(objmodel);
        }

                    //---------------Add New Drivers

        [HttpGet]
        public ActionResult addnew()
        {
            var DriverName = db.Master_DriverType.ToList();
            ViewBag.DriverTypeList = new SelectList(DriverName, "DriverType_Id", "DriverType");

            var BloodGroup = db.Master_BloodGroup.ToList();
            ViewBag.BloodGroupList = new SelectList(BloodGroup, "BloodGroupID", "BloodGroupName");

            var Gender = db.Master_Gender.ToList();
            ViewBag.GenderList = new SelectList(Gender,"GenderID","GenderName");

            return View();
        }


        [HttpPost]
        public ActionResult addnew(DSRCManagementSystem.Models.ManageDrivers objmanage)
        {
            var DriverName = db.Master_DriverType.ToList();
            ViewBag.DriverTypeList = new SelectList(DriverName, "DriverType_Id", "DriverType");

            var BloodGroup = db.Master_BloodGroup.ToList();
            ViewBag.BloodGroupList = new SelectList(BloodGroup, "BloodGroupID", "BloodGroupName");

            var Gender = db.Master_Gender.ToList();
            ViewBag.GenderList = new SelectList(Gender, "GenderID", "GenderName");
           

            DSRCManagementSystem.Models.ConvertByte objcb = new Models.ConvertByte();
            var picture = objcb.ConvertToBytes(objmanage.Pictures);
            var Doc = objcb.ConvertToBytes(objmanage.Documents);


            if (ModelState.IsValid)
            {
               
                 var driver =  db.Drivers.CreateObject();

                     driver.First_Name=objmanage.First_Name;
                     driver.Last_Name = objmanage.Last_Name;
                    driver.DOB = objmanage.DOB;
                    driver.Gender = objmanage.Gender;
                    driver.Driver_Licence_No = objmanage.Driver_Licence_No;
                    driver.Driver_Licence_Expire_Date = objmanage.Driver_Licence_Expire_Date;
                    driver.DriverType_Id = objmanage.DriverType_Id;
                    driver.Email_Id = objmanage.Email_Id;
                    driver.Contact_No = objmanage.Contact_No;
                    driver.Blood_Group = objmanage.Blood_Group;
                    driver.Communication_Address = objmanage.Communication_Address;
                    driver.Driver_Batch_No = objmanage.Driver_Batch_No;
                    driver.IsActive = true;
                    driver.Photo = picture;
                    driver.Document_Proof = Doc;              
                
                db.Drivers.AddObject(driver);
                db.SaveChanges();
               
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Manage", "ManageDrivers");
        }

                     //---------------Edit Drivers

        [HttpGet]
        public ActionResult EditDriver(int Id)
        {
            //List<DSRCManagementSystem.Models.ManageDrivers> EditList = new List<DSRCManagementSystem.Models.ManageDrivers>();          
            var Edit = db.Drivers.Where(x => x.DriverId == Id).Select(o => o).FirstOrDefault();
          
            
           
            var EditList = (from d in db.Drivers
                            where d.DriverId == Id && d.IsActive != false
                            select new DSRCManagementSystem.Models.ManageDrivers
                            {
                               DriverId = d.DriverId,
                               First_Name = d.First_Name,
                               Last_Name = d.Last_Name,
                               DOB = d.DOB,
                               Gender = d.Gender,
                               Driver_Licence_No = d.Driver_Licence_No,
                               Driver_Licence_Expire_Date = d.Driver_Licence_Expire_Date,
                               DriverType_Id = d.DriverType_Id,
                               Email_Id = d.Email_Id,
                               Contact_No = d.Contact_No,
                               Blood_Group = d.Blood_Group,
                               Communication_Address = d.Communication_Address,
                               Driver_Batch_No = d.Driver_Batch_No ,
                               Picture = d.Photo
                               //Document_Proof = 
                            }).FirstOrDefault();

            var DriverName = db.Master_DriverType.ToList();
            ViewBag.DriverTypeList = new SelectList(DriverName, "DriverType_Id", "DriverType",Edit.DriverType_Id);

            var BloodGroup = db.Master_BloodGroup.ToList();
            ViewBag.BloodGroupList = new SelectList(BloodGroup, "BloodGroupID", "BloodGroupName",Edit.Blood_Group);

            var Gender = db.Master_Gender.ToList();
            ViewBag.GenderList = new SelectList(Gender, "GenderID", "GenderName",Edit.Gender);

            return View(EditList);
        }

        [HttpPost]
        public ActionResult EditDriver(DSRCManagementSystem.Models.ManageDrivers Values)
        {
            DSRCManagementSystem.Models.ConvertByte objcb = new Models.ConvertByte();
            var picture = objcb.ConvertToBytes(Values.Pictures);
            var Doc = objcb.ConvertToBytes(Values.Documents);

            var UpdateDriver = db.Drivers.Where(o => o.DriverId == Values.DriverId).Select(x => x).FirstOrDefault();

                UpdateDriver.DriverId = Values.DriverId;
                UpdateDriver.First_Name = Values.First_Name;
                UpdateDriver.Last_Name = Values.Last_Name;
                UpdateDriver.DOB = Values.DOB;
                UpdateDriver.Gender = Values.Gender;
                UpdateDriver.Driver_Licence_No = Values.Driver_Licence_No;
                UpdateDriver.Driver_Licence_Expire_Date = Values.Driver_Licence_Expire_Date;
                UpdateDriver.DriverType_Id = Values.DriverType_Id;
                UpdateDriver.Email_Id = Values.Email_Id;
                UpdateDriver.Contact_No = Values.Contact_No;
                UpdateDriver.Blood_Group = Values.Blood_Group;
                UpdateDriver.Communication_Address = Values.Communication_Address;
                UpdateDriver.Driver_Batch_No = Values.Driver_Batch_No;
                UpdateDriver.Document_Proof = Doc;
                UpdateDriver.Photo = picture;

                db.SaveChanges();
                return RedirectToAction("Manage", "ManageDrivers");
                //return Json("true", JsonRequestBehavior.AllowGet);    
        }
        
                    
                    //------------------Delete Drivers

        public ActionResult DeleteDriver(int Id)
        {
            var DeleteDriver = db.Drivers.Where(o => o.DriverId == Id).Select(x => x).FirstOrDefault();

            DeleteDriver.IsActive = false;
            db.SaveChanges();

            return Json("true", JsonRequestBehavior.AllowGet);    
        }
        

       
    }
}
