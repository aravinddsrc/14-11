using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Text;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using System.Data.Linq.SqlClient;
namespace DSRCManagementSystem.Controllers
{
    public class TransportationController : Controller
    {
        //
        // GET: /Transportation/
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ManageVehicle(string value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            List<DSRCManagementSystem.Models.AddVehicle> objmodel = new List<Models.AddVehicle>();
            try
            {
                if (value == null)
                {
                    objmodel = (from c in db.Vehicles.Where(x => x.IsActive == true)
                                select new DSRCManagementSystem.Models.AddVehicle()
                               {

                                   vehicleid = c.VehicleId,
                                   Vehicle_No = c.Vehicle_No,
                                   VehicleMake = c.VehicleMake,
                                   VehicleModel_Id = c.VehicleModel_Id
                               }).ToList();



                    var number = (from p in db.Vehicles.Where(x => x.IsActive == true)
                                  select new DSRCManagementSystem.Models.AddVehicle()

                                  {
                                      vehicleid = p.VehicleId,
                                      Vehicle_No = p.Vehicle_No,

                                  }).ToList();
                    ViewBag.Vehicles = new SelectList(number, "Vehicleid", "Vehicle_No");
                }
                else
                {
                    objmodel = (from c in db.Vehicles.Where(x => x.IsActive == true && x.Vehicle_No == value)
                                select new DSRCManagementSystem.Models.AddVehicle()
                                {

                                    vehicleid = c.VehicleId,
                                    Vehicle_No = c.Vehicle_No,
                                    VehicleMake = c.VehicleMake,
                                    VehicleModel_Id = c.VehicleModel_Id
                                }).ToList();



                    var number = (from p in db.Vehicles.Where(x => x.IsActive == true)
                                  select new DSRCManagementSystem.Models.AddVehicle()

                                  {
                                      vehicleid = p.VehicleId,
                                      Vehicle_No = p.Vehicle_No,

                                  }).ToList();
                    ViewBag.Vehicles = new SelectList(number, "Vehicleid", "Vehicle_No");
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objmodel);
        }






        [HttpGet]
        public ActionResult ManageExpense(string ExpenseId, string intexp)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.Transportation> objmodel = new List<DSRCManagementSystem.Models.Transportation>();
            int expid = 0;
            if (intexp != "")
            {
                expid = Convert.ToInt32(intexp);
            }

            var vehicles = (from p in objdb.Vehicles.Where(x => x.IsActive == true || x.IsActive == null)
                            select new
                            {
                                VehicleId = p.VehicleId,
                                VehicleNo = p.Vehicle_No,

                            }).Distinct().ToList();

            if (intexp == null || intexp == "")
            {
                ViewBag.Vehicles = new SelectList(vehicles.Distinct(), "VehicleId", "VehicleNo");
            }

            else
            {
                ViewBag.Vehicles = new SelectList(vehicles.Distinct(), "VehicleId", "VehicleNo", expid);
            }

            if (ExpenseId == null || intexp == "")
            {

                objmodel = (from p in objdb.Vehicles.Where(x => x.IsActive == true || x.IsActive == null)
                            join t in objdb.Master_VehicleModel on p.VehicleModel_Id equals t.VehicleModel_Id into y
                            from value in y.DefaultIfEmpty()
                            join v in objdb.ManageExpense_Mapping on p.VehicleId equals v.VehicleId into z
                            from abc in z.DefaultIfEmpty()
                            join x in objdb.ManageExpenses on abc.ManageExpenseId equals x.ManageExpenseId into value1 from abcd in value1.DefaultIfEmpty()
                           where abcd.Isactive == true
                            join c in objdb.Master_ExpenseType on abcd.ExpenseType equals c.ExpenseType_Id
                            select new DSRCManagementSystem.Models.Transportation
                            {
                                Id = p.VehicleId,
                                VehicleNumber = p.Vehicle_No,
                                VehicleModel = value.VehicleModel,
                                VehicleMake = p.VehicleMake,
                                ManageExpenseId = abc.ManageExpenseId,
                                //   ExpenseType = c.ExpenseType,
                                //Cost = abcd.Cost,
                                ExpenseId = abc.ManageExpenseId,
                                YearsofManufacturing = p.Model_Year
                            }).OrderByDescending(x => x.YearsofManufacturing).ToList();

                foreach (var item in objmodel)
                {
                    if (item.ManageExpenseId != null)
                    {
                        var values = objdb.ManageExpenses.Where(x => x.Isactive == true && x.ManageExpenseId == item.ManageExpenseId).Select(o => o).FirstOrDefault();
                        if (values != null)
                        {
                            var extype = objdb.Master_ExpenseType.Where(x => x.ExpenseType_Id == values.ExpenseType).Select(o => o.ExpenseType).FirstOrDefault();
                            if (extype != null)
                            {
                                item.ExpenseType = extype;
                            }
                            else
                            {
                                item.ExpenseType = null;
                            }
                            if (values.Cost != null)
                            {
                                item.Cost = values.Cost;
                            }
                            else
                            {
                                item.Cost = null;
                            }
                        }
                        else
                        {
                            item.ExpenseType = null;
                            item.Cost = null;

                        }
                    }
                    else
                    {
                        item.ManageExpenseId = null;
                        item.ExpenseType = null;
                        item.Cost = null;

                    }

                }


            }

            else
            {
                objmodel = (from p in objdb.Vehicles.Where(x => (x.IsActive == true || x.IsActive == null) && x.Vehicle_No == ExpenseId)
                            join t in objdb.Master_VehicleModel on p.VehicleModel_Id equals t.VehicleModel_Id into y
                            from value in y.DefaultIfEmpty()
                            join v in objdb.ManageExpense_Mapping on p.VehicleId equals v.VehicleId into z
                            from abc in z.DefaultIfEmpty()
                            join x in objdb.ManageExpenses on abc.ManageExpenseId equals x.ManageExpenseId into value1 from abcd in value1.DefaultIfEmpty()
                            where abcd.Isactive == true
                            join c in objdb.Master_ExpenseType on abcd.ExpenseType equals c.ExpenseType_Id
                            select new DSRCManagementSystem.Models.Transportation
                            {
                                Id = p.VehicleId,
                                VehicleNumber = p.Vehicle_No,
                                VehicleModel = value.VehicleModel,
                                VehicleMake = p.VehicleMake,
                                ManageExpenseId = abc.ManageExpenseId,
                                // ExpenseType = c.ExpenseType,
                                //Cost = abcd.Cost,
                                ExpenseId = abc.ManageExpenseId,
                                YearsofManufacturing = p.Model_Year
                            }).OrderByDescending(x => x.YearsofManufacturing).ToList();

                foreach (var item in objmodel)
                {
                    if (item.ManageExpenseId != null)
                    {
                        var values = objdb.ManageExpenses.Where(x => x.Isactive == true && x.ManageExpenseId == item.ManageExpenseId).Select(o => o).FirstOrDefault();
                        if (values != null)
                        {
                            var extype = objdb.Master_ExpenseType.Where(x => x.ExpenseType_Id == values.ExpenseType).Select(o => o.ExpenseType).FirstOrDefault();
                            if (extype != null)
                            {
                                item.ExpenseType = extype;
                            }
                            else
                            {
                                item.ExpenseType = null;
                            }
                            if (values.Cost != null)
                            {
                                item.Cost = values.Cost;
                            }
                            else
                            {
                                item.Cost = null;
                            }
                        }
                        else
                        {
                            item.ExpenseType = null;
                            item.Cost = null;

                        }
                    }
                    else
                    {
                        item.ManageExpenseId = null;
                        item.ExpenseType = null;
                        item.Cost = null;

                    }

                }

            }


            return View(objmodel);
        }



        [HttpGet]
        public ActionResult AddExpense()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var vehicles = (from p in objdb.Vehicles.Where(x => x.IsActive == true || x.IsActive == null)
                            select new
                            {
                                VehicleId = p.VehicleId,
                                VehicleNo = p.Vehicle_No,

                            }).Distinct().ToList();

            var expense = (from p in objdb.Master_ExpenseType
                           select new
                           {
                               ExpenseId = p.ExpenseType_Id,
                               Expense = p.ExpenseType
                           }).ToList();

            ViewBag.Vehicles = new SelectList(vehicles.Distinct(), "VehicleId", "VehicleNo");
            ViewBag.Expense = new SelectList(expense.Distinct(), "ExpenseId", "Expense");



            return View();
        }


        [HttpPost]
        public ActionResult AddExpense(DSRCManagementSystem.Models.Transportation objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.ManageExpens objexp = new DSRCManagementSystem.ManageExpens();

            DSRCManagementSystem.ManageExpense_Mapping objmap = new DSRCManagementSystem.ManageExpense_Mapping();

            //var num =Convert.ToInt32(objmodel.VehicleNumber);

            var vehicleid = objdb.Vehicles.Where(x => x.Vehicle_No == objmodel.VehicleNumber && x.IsActive == true).Select(o => o.VehicleId).FirstOrDefault();

            var objalready = objdb.ManageExpense_Mapping.Where(x => x.VehicleId == vehicleid).Select(o => o.ManageExpenseId).ToList();


            if (objalready != null)
            {

                for (int i = 0; i < objalready.Count(); i++)
                {
                    var value = Convert.ToInt32(objalready[i]);
                    int? already = objdb.ManageExpenses.Where(x => x.ManageExpenseId == value && x.Isactive == true).Select(o => o.ExpenseType).FirstOrDefault();

                    if (already == Convert.ToInt32(objmodel.ExpenseType))
                    {
                        return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                    }

                }
            }


            objexp.Date = DateTime.Now;
            objexp.Cost = objmodel.Cost;
            objexp.ExpenseType = Convert.ToInt32(objmodel.ExpenseType);
            objexp.Isactive = true;
            objdb.AddToManageExpenses(objexp);
            objdb.SaveChanges();


            objmap.ManageExpenseId = objexp.ManageExpenseId;
            objmap.VehicleId = vehicleid;
            objdb.AddToManageExpense_Mapping(objmap);
            objdb.SaveChanges();



            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditExpense(string Id, string vehicleno)
        {
            int id = 0;
            int objvehicleno = 0;
            if (Id != "" && Id != null)
            {
                id = Convert.ToInt32(Id);
            }
            else
            {
                id = 0;
            }




            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Transportation objmodel = new DSRCManagementSystem.Models.Transportation();


            var vehicles = (from p in objdb.Vehicles.Where(x => x.IsActive == true || x.IsActive == null)
                            select new
                            {
                                VehicleId = p.VehicleId,
                                VehicleNo = p.Vehicle_No,

                            }).ToList();

            var expense = (from p in objdb.Master_ExpenseType
                           select new
                           {
                               Id6 = p.ExpenseType_Id,
                               Expense = p.ExpenseType
                           }).ToList();



            var num = objdb.ManageExpense_Mapping.Where(x => x.ManageExpenseId == id).Select(o => o.VehicleId).FirstOrDefault();
            var objvalue = objdb.ManageExpenses.Where(x => x.ManageExpenseId == id && x.Isactive == true).Select(o => o).FirstOrDefault();

            if (objvalue != null)
            {
                objmodel.Cost = objvalue.Cost;
            }
            else
            {
                objmodel.Cost = null;
            }

            if (objvalue != null)
            {

                if (objvalue.Date != null)
                {

                    DateTime d1 = Convert.ToDateTime(objvalue.Date);
                    string d = d1.ToShortDateString();
                    objmodel.date = d;
                    objmodel.dateofyear = objvalue.Date;
                }
                else
                {
                    DateTime d1 = Convert.ToDateTime(DateTime.Now.ToString());
                    string d = d1.ToShortDateString();
                    objmodel.date = d;
                    objmodel.dateofyear = objvalue.Date;
                }
            }

            else
            {
                objmodel.date = null;

            }


            objmodel.ExpenseId = id;


            if (num != null)
            {
                ViewBag.Vehicles = new SelectList(vehicles, "VehicleId", "VehicleNo", num);
            }
            else
            {
                if (vehicleno != "" && Id == "")
                {
                    int vechi = Convert.ToInt32(vehicleno);
                    var objnum = objdb.Vehicles.Where(x => x.VehicleId == vechi).Select(o => o.VehicleId).FirstOrDefault();
                    ViewBag.Vehicles = new SelectList(vehicles, "VehicleId", "VehicleNo", objnum);
                }
                else
                {
                    ViewBag.Vehicles = new SelectList(vehicles, "VehicleId", "VehicleNo");
                }
            }

            int? expensetype = 0;

            if (objvalue != null)
            {
                expensetype = objvalue.ExpenseType;
            }


            if (objvalue != null)
            {
                ViewBag.Expense = new SelectList(expense, "Id6", "Expense", expensetype);
            }
            else
            {

                ViewBag.Expense = new SelectList(expense, "Id6", "Expense");

            }


            return View(objmodel);
        }

        [HttpPost]
        public ActionResult EditExpense(DSRCManagementSystem.Models.Transportation objmodel)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Transportation obj = new DSRCManagementSystem.Models.Transportation();
            var vehicle_num = objmodel.VehicleNumber;
            var dbvalue = objdb.Vehicles.Where(x => x.Vehicle_No == vehicle_num).Select(o => o.VehicleId).FirstOrDefault();
           // var dbvalue = objdb.Vehicles.Where(x => x.Vehicle_No == vehicle_num).Select(o => o).FirstOrDefault();
            var objmanageexpmap = objdb.ManageExpense_Mapping.Where(x => x.VehicleId == dbvalue).Select(o => o.ManageExpenseId).FirstOrDefault();

            if (objmodel.ExpenseId != null)
            {

                var manageid = objdb.ManageExpenses.Where(x => x.ManageExpenseId == objmanageexpmap && x.Isactive == true).Select(o => o).FirstOrDefault();
                var listvalue = objdb.ManageExpense_Mapping.Where(x => x.VehicleId == dbvalue).Select(o => o.ManageExpenseId).ToList();

                if (manageid.ExpenseType != Convert.ToInt32(objmodel.ExpenseType))
                {
                    if (listvalue != null)
                    {

                        for (int k = 0; k < listvalue.Count(); k++)
                        {
                            var dbvalue1 = Convert.ToInt32(listvalue[k]);
                            var alreadyvale = objdb.ManageExpenses.Where(x => x.ManageExpenseId == dbvalue1 && x.Isactive == true).Select(o => o.ExpenseType).FirstOrDefault();
                            if (alreadyvale == Convert.ToInt32(objmodel.ExpenseType))
                            {
                                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                }


                manageid.Date = objmodel.dateofyear;
                manageid.Cost = objmodel.Cost;
                manageid.ExpenseType = Convert.ToInt32(objmodel.ExpenseType);
                objdb.SaveChanges();
            }

            else
            {
                DSRCManagementSystem.ManageExpens objexp = new DSRCManagementSystem.ManageExpens();
                DSRCManagementSystem.ManageExpense_Mapping objmap = new DSRCManagementSystem.ManageExpense_Mapping();
                objexp.Date = objmodel.dateofyear;
                objexp.Cost = objmodel.Cost;
                objexp.ExpenseType = Convert.ToInt32(objmodel.ExpenseType);
                objexp.Isactive = true;
                objdb.AddToManageExpenses(objexp);
                objdb.SaveChanges();



                objmap.ManageExpenseId = objexp.ManageExpenseId;

                //objmap.VehicleId = Convert.ToInt32(dbvalue.VehicleId);

                objmap.VehicleId =Convert.ToInt32( dbvalue);

                objdb.AddToManageExpense_Mapping(objmap);
                objdb.SaveChanges();

            }

            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string Id)
        {
            int id = Convert.ToInt32(Id);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Transportation objmodel = new DSRCManagementSystem.Models.Transportation();

            //var value = objdb.ManageExpense_Mapping.Where(x => x.VehicleId == id).Select(o => o.ManageExpenseId).FirstOrDefault();

            //var objvalue = objdb.ManageExpenses.Where(x => x.ManageExpenseId == id && x.Isactive == true).Select(o => o).FirstOrDefault();
            //// var type = objdb.Master_ExpenseType.Where(x => x.ExpenseType_Id == objvalue.ExpenseType).Select(o => o.ExpenseType).FirstOrDefault();

            //objvalue.Isactive = false;



            var objvalue = objdb.ManageExpenses.Where(x => x.ManageExpenseId ==id && x.Isactive == true).Select(o => o).FirstOrDefault();
           // var type = objdb.Master_ExpenseType.Where(x => x.ExpenseType_Id == objvalue.ExpenseType).Select(o => o.ExpenseType).FirstOrDefault();        
            var values = objdb.ManageExpense_Mapping.Where(x => x.ManageExpenseId == objvalue.ManageExpenseId).Select(o => o).FirstOrDefault();
            objdb.ManageExpense_Mapping.DeleteObject(values);
          objvalue.Isactive = false;           
         
                
           

            objdb.SaveChanges();



            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddVehicle()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            DSRCManagementSystem.Models.AddVehicle objmodel = new DSRCManagementSystem.Models.AddVehicle();


            //var vehi = (from a in db.Vehicles
            //            join b in db.Master_VehicleModel on a.VehicleModel_Id equals b.VehicleModel_Id
            //            join c in db.Master_VehicleBrand on a.VehicleBrand_Id equals c.VehicleBrand_Id
            //            join d in db.Master_VehicleType on a.VehicleType_Id equals d.VehicleType_Id
            //            select new DSRCManagementSystem.Models.AddVehicle()
            //            {
            //                VehicleModel_Id = a.VehicleModel_Id,
            //                VehicleBrand_Id = a.VehicleBrand_Id,
            //                VehicleType_Id = a.VehicleType_Id,


            //            }).ToList();


            var model = (from p in db.Master_VehicleModel
                         select new
                         {
                             ModelId = p.VehicleModel_Id,
                             Model = p.VehicleModel

                         }).ToList();


            var brand = (from p in db.Master_VehicleBrand
                         select new
                         {
                             BrandId = p.VehicleBrand_Id,
                             Brand = p.VehicleBrand

                         }).ToList();

            var type = (from P in db.Master_VehicleType
                        select new
                        {
                            Typeid = P.VehicleType_Id,
                            Type = P.VehicleType

                        }).ToList();


            ViewBag.VehicleModel = new SelectList(model, "ModelId", "Model");
            ViewBag.VehicleBrand = new SelectList(brand, "BrandId", "Brand");
            ViewBag.VehicleType = new SelectList(type, "Typeid", "Type");
            ViewBag.Years = new SelectList(GetYears(), "", "");
            //GetYears();
            return View();

        }




        [HttpPost]
        public ActionResult AddVehicle(AddVehicle model, string Contact_No)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


            // DSRCManagementSystem.Models.AddsVehicle objvehicle = new DSRCManagementSystem.Models.AddsVehicle();
            DSRCManagementSystem.Vehicle objvehicle = new DSRCManagementSystem.Vehicle();


            try
            {


                if (db.Vehicles.Any(R => R.Vehicle_No == model.Vehicle_No))
                {
                    ModelState.AddModelError("Vehicle_No", "Vehicle Number Already Exists");
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }
                else if (model.Vehicle_No == null)
                {
                    ModelState.AddModelError("Vehicle_No", "Vehicle_No");

                }
                else
                {

                    var value = db.Vehicles.Where(x => x.VehicleId == model.vehicleid).Select(o => o).FirstOrDefault();

                    objvehicle.Vehicle_No = model.Vehicle_No;
                    objvehicle.VehicleMake = model.VehicleMake;
                    objvehicle.Vehicle_Remarks = model.Remarks;
                    objvehicle.VehicleBrand_Id = Convert.ToInt32(model.VehicleBrand);
                    objvehicle.VehicleType_Id = Convert.ToInt32(model.VehicleType);
                    objvehicle.VehicleModel_Id = Convert.ToInt32(model.VehicleModel);
                    objvehicle.Model_Year = model.Model_Year;
                    objvehicle.No_of_Seat = model.No_of_Seat;
                    objvehicle.No_of_Trip = model.Trip;

                    objvehicle.IsActive = true;

                    if (model.Vehicle_Photo != null)
                    {
                        objvehicle.Vehicle_Photo = true;
                    }

                    objvehicle.Contact_No = model.Contact_No;

                    db.AddToVehicles(objvehicle);
                    db.SaveChanges();


                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
              
            }

            return Json("Success1", JsonRequestBehavior.AllowGet);
        }

        //Edit

        [HttpGet]
        public ActionResult EditVehicle(string Id)
        {

            int vehicleid = Convert.ToInt32(Id);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.AddVehicle objmodel = new DSRCManagementSystem.Models.AddVehicle();

            var value = db.Vehicles.Where(x => x.VehicleId == vehicleid).Select(o => o).FirstOrDefault();

            var model = (from p in db.Master_VehicleModel
                         select new
                         {
                             ModelId = p.VehicleModel_Id,
                             Model = p.VehicleModel

                         }).ToList();


            var brand = (from p in db.Master_VehicleBrand
                         select new
                         {
                             BrandId = p.VehicleBrand_Id,
                             Brand = p.VehicleBrand

                         }).ToList();

            var type = (from P in db.Master_VehicleType
                        select new
                        {
                            Typeid = P.VehicleType_Id,
                            Type = P.VehicleType
                        }).ToList();


            ViewBag.VehicleModel = new SelectList(model, "ModelId", "Model", value.VehicleModel_Id);
            ViewBag.VehicleBrand = new SelectList(brand, "BrandId", "Brand", value.VehicleBrand_Id);
            ViewBag.VehicleType = new SelectList(type, "Typeid", "Type", value.VehicleType_Id);
            ViewBag.Years = new SelectList(GetYears(), "", "", value.Model_Year);




            objmodel.Vehicle_No = value.Vehicle_No;
            objmodel.VehicleMake = value.VehicleMake;
            objmodel.Remarks = value.Vehicle_Remarks;
            objmodel.VehicleBrand = Convert.ToString(value.VehicleBrand_Id);
            objmodel.VehicleType = Convert.ToString(value.VehicleType_Id);
            objmodel.VehicleModel = Convert.ToString(value.VehicleModel_Id);
            objmodel.Model_Year = value.Model_Year;
            objmodel.No_of_Seat = value.No_of_Seat;
            objmodel.Trip = value.No_of_Trip;
            objmodel.Contact_No = value.Contact_No;
            objmodel.vehicleid = Convert.ToInt32(vehicleid);
            //objmodel.Vehicle_Photo = value.Vehicle_Photo;

            //List<int> Years = new List<int>();
            //ViewBag.Years = new SelectList(Years, "", "", value.Model_Year);
            return View(objmodel);

        }
        [HttpPost]
        public ActionResult EditVehicle(AddVehicle model, string Contact_No, string vehicleid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            if (vehicleid != null)
            {
                int vechid = Convert.ToInt32(vehicleid);
            }

            var value = db.Vehicles.Where(x => x.VehicleId == model.vehicleid).Select(o => o).FirstOrDefault();
            value.VehicleBrand_Id = Convert.ToInt32(model.VehicleBrand);
            value.VehicleType_Id = Convert.ToInt32(model.VehicleType);
            value.VehicleModel_Id = Convert.ToInt32(model.VehicleModel);
            value.VehicleMake = model.VehicleMake;
            value.Vehicle_Remarks = model.Remarks;
            value.Model_Year = model.Model_Year;
            value.No_of_Seat = model.No_of_Seat;
            value.No_of_Trip = model.Trip;


            if (value.Vehicle_Photo != null)
            {
                value.Vehicle_Photo = true;
            }


            db.SaveChanges();

            return Json("Success1", JsonRequestBehavior.AllowGet);
        }

        //delete

        [HttpPost]
        public ActionResult DeleteVehicle(int Id)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //if (Id != null)
            //{
            //    int vehicleid = Convert.ToInt32(Id);
            //}
            //if (vehicleid != null)
            //{
            //    int vechid = Convert.ToInt32(vehicleid);
            //}            
            //  var value = objdb.Vehicles.Where(x => x.VehicleId == vechid).Select(o => o).FirstOrDefault();

            var objvalue = objdb.Vehicles.Where(x => x.VehicleId == Id).Select(o => o).FirstOrDefault();
            objvalue.IsActive = false;
            objdb.SaveChanges();
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

        }


        //year drop down
        public List<int> GetYears()
        {
            List<int> Years = new List<int>();
            int i = DateTime.Now.Year;
            for (i = 1990; i <= DateTime.Now.Year; i++)
            {
                Years.Add(i);

            }

            //DateTime startYear = DateTime.Now;

            //while (startYear.Year >= DateTime.Now.AddYears(3).Year)
            //{
            //    Years.Add(startYear.Year);
            //    startYear = startYear.AddYears(1);
            //}
            //ViewBag.Years = Years;
            return Years;
        }

        [HttpGet]
        public ActionResult AssignDriver(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.AddVehicle objmodel = new DSRCManagementSystem.Models.AddVehicle();
            try
            {
                //var vehicles = (from p in db.Vehicles.Where(x => x.IsActive == true || x.IsActive == null)
                //                select new
                //                {
                //                    VehicleId = p.VehicleId,
                //                    VehicleNo = p.Vehicle_No,

                //                }).ToList();
                var num = db.Vehicles.Where(x => x.VehicleId == Id).Select(x=>x).FirstOrDefault();
                //var Vehicle = (from x in db.Vehicles
                //               select new
                //               {
                //                   VehileId = x.VehicleId,
                //                   VehicleName = x.Vehicle_No

                //               }).ToList();
                objmodel.Vehicle_No = num.Vehicle_No;
                var Driver = (from x in db.Drivers
                              where x.IsActive == true
                              select new
                              {
                                  DriverId = x.DriverId,
                                  DriverName = x.First_Name
                              }
                    ).ToList();

                //ViewBag.Vehicle = new SelectList(vehicles, "VehicleId", "VehicleNo", num);
                ViewBag.Driver = new SelectList(Driver, "DriverId", "DriverName");
                ViewBag.Co_Driver = new SelectList("", "", "");
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult AssignDriver(AddVehicle model)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                DSRCManagementSystem.DriverVehicle_Mapping obj = new DSRCManagementSystem.DriverVehicle_Mapping();

                obj.VehicleId = db.Vehicles.Where(x => x.Vehicle_No == model.Vehicle_No).Select(x => x.VehicleId).FirstOrDefault();
                obj.DriverId = Convert.ToInt32(model.DriverName);
                obj.Co_DriverId = Convert.ToInt32(model.Co_DriverName);
                db.AddToDriverVehicle_Mapping(obj);
                db.SaveChanges();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json("Success1", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AssignCoDriver(string Value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


            int DriverID = Convert.ToInt32(Value);
            var Driver = (from D in db.Drivers.Where(x => x.DriverId != DriverID)
                          where D.IsActive == true
                          select new
                          {
                              DriverId = D.DriverId,
                              DriverName = D.First_Name

                          }).OrderBy(x => x.DriverName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DriverId), Text = m.DriverName });

            ViewBag.Co_Driver = new SelectList(Driver, "DriverId", "DriverName");
            return Json(new SelectList(Driver, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        // Route functions Start

        [HttpGet]
        public ActionResult RouteDetails()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var routeDetails = (from route in db.Routes
                                join routeMap in db.RoutesStops_Mapping
                                    on route.RouteId equals routeMap.RouteId
                                join vehicle in db.Vehicles
                                    on routeMap.VehicleId equals vehicle.VehicleId
                                join stops in db.Stops
                                    on routeMap.StopId equals stops.StopId
                                select new
                                {
                                    RouteName = route.Route_Name,
                                    Stopping = stops.Stop_Name,
                                    vehicleNo = vehicle.Vehicle_No,
                                    RouteId = route.RouteId,
                                    vehicleId = vehicle.VehicleId,
                                    stopId = stops.StopId
                                }).ToList();

            List<ManageRoute> manageRouteList = new List<ManageRoute>();

            ManageRoute manageRoute;
            foreach (var routeDetail in routeDetails)
            {
                manageRoute = new ManageRoute();
                manageRoute.Stops = routeDetail.Stopping;
                manageRoute.RouteName = routeDetail.RouteName;
                manageRoute.VehicleNumber = routeDetail.vehicleNo;
                manageRoute.RouteId = routeDetail.RouteId;
                manageRoute.StopId = routeDetail.stopId;
                manageRoute.VehicleId = routeDetail.vehicleId;
                manageRouteList.Add(manageRoute);
            }

            return View(manageRouteList);
        }

        [HttpGet]
        public ActionResult AddRoute(int stopId = 0, ManageStops obj = null)
        {
            ManageRoute route = new ManageRoute();

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                if (Request.IsAjaxRequest())
                {
                    //stopId = Convert.ToInt32(Request.QueryString["stopId"].ToString());

                    List<SelectListItem> item = new List<SelectListItem>();

                    var stopsDetails = db.Stops.ToList();
                    string stopval = "";

                    foreach (var stops in stopsDetails)
                        if (stops.StopId == stopId)
                        {
                            stopval = stops.StopId.ToString();
                            item.Add(new SelectListItem { Text = stops.Stop_Name, Value = stops.StopId.ToString() });
                        }

                    route.StopItemList = item.ToList();


                    return Json(item, JsonRequestBehavior.AllowGet);
                }

                ViewData["VehicleList"] = db.Vehicles.ToList().Select(x => new SelectListItem { Value = x.VehicleId.ToString(), Text = x.Vehicle_No });
                //route.StopItemList = db.Stops.ToList().Where(x => !string.IsNullOrEmpty(x.Stop_Name)).Select(X => new SelectListItem { Value = X.StopId.ToString(), Text = X.Stop_Name });
            }

            return View(route);
        }

        [HttpGet]
        public ActionResult ViewDetail(string routeId, string vehicleId, string stopId)
        {
            ManageRoute route = SetModelObject(routeId, vehicleId, stopId);
            return View(route);
        }

        [HttpPost]
        public ActionResult AddRoute(ManageRoute obj, string[] StopArray)
        {
            using (var db = new DSRCManagementSystemEntities1())
            {

                //Adding Route Name to Routes Table
                Route route = new Route();
                route.Route_Name = obj.RouteName;
                db.Routes.AddObject(route);

                foreach (var stopId in StopArray)
                {
                    //Adding Details to RouteMapping Table
                    RoutesStops_Mapping route_StopMapping = new RoutesStops_Mapping();
                    route_StopMapping.RouteId = route.RouteId;
                    route_StopMapping.VehicleId = obj.VehicleId;
                    route_StopMapping.StopId = Convert.ToInt32(stopId);
                    db.RoutesStops_Mapping.AddObject(route_StopMapping);
                }

                db.SaveChanges();
            }

            return RedirectToAction("RouteDetails", "Transportation");
        }

        [HttpGet]
        public ActionResult EditRoute(string routeId, string vehicleId, string stopId)
        {
            ManageRoute route = SetModelObject(routeId, vehicleId, stopId);
            return View(route);
        }

        public ManageRoute SetModelObject(string routeId, string vehicleId, string stopId)
        {
            ManageRoute route = new ManageRoute();

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                route.RouteId = Convert.ToInt32(routeId);
                route.VehicleId = Convert.ToInt32(vehicleId);
                route.StopId = Convert.ToInt32(stopId);
                route.RouteName = db.Routes.Where(x => x.RouteId == route.RouteId).FirstOrDefault().Route_Name;

                var stopItemList = (from routes in db.Routes join routeMap in db.RoutesStops_Mapping on routes.RouteId equals routeMap.RouteId join vehicle in db.Vehicles on routeMap.VehicleId equals vehicle.VehicleId select new { stopId = routeMap.StopId, StopName = routeMap.Stop }).ToList()
                    .Select(x => new SelectListItem { Value = x.stopId.ToString(), Text = x.StopName.Stop_Name });//db.Stops.ToList().Where(x => !string.IsNullOrEmpty(x.Stop_Name)).Select(X => new SelectListItem { Value = X.StopId.ToString(), Text = X.Stop_Name });

                //SelectListItem StopItem = stopItemList.Where(x => x.Value == stopId).FirstOrDefault();
                //if (StopItem != null)
                //    stopItemList = new SelectList(stopItemList, "value", "text", StopItem.Value);
                //route.StopItemList = stopItemList;

                var vehicleList = db.Vehicles.ToList().Select(x => new SelectListItem { Value = x.VehicleId.ToString(), Text = x.Vehicle_No });
                SelectListItem vehicleItem = vehicleList.Where(x => x.Value == vehicleId).FirstOrDefault();
                if (vehicleItem != null)
                    vehicleList = new SelectList(vehicleList, "value", "text", vehicleItem.Value);

                ViewData["VehicleList"] = vehicleList;
            }
            return route;
        }

        [HttpPost]
        public ActionResult EditRoute(ManageRoute obj)
        {
            Route route = new Route();
            RoutesStops_Mapping routeMap = new RoutesStops_Mapping();

            using (var db = new DSRCManagementSystemEntities1())
            {
                route = db.Routes.ToList().Where(x => x.RouteId == obj.RouteId).FirstOrDefault();
                route.Route_Name = obj.RouteName;

                routeMap = db.RoutesStops_Mapping.ToList().Where(x => x.RouteId == obj.RouteId).FirstOrDefault();
                routeMap.VehicleId = obj.VehicleId;
                routeMap.StopId = obj.StopId;

                db.SaveChanges();
            }

            return RedirectToAction("RouteDetails", "Transportation");
        }

        [HttpGet]
        public ActionResult AddStops(string VehicleId)
        {
            ManageStops stopDetail = new ManageStops();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                int vehichleId = string.IsNullOrEmpty(VehicleId) ? 0 : Convert.ToInt32(VehicleId);

                ViewData["Stops"] = db.Stops.ToList().Select(x => new SelectListItem { Text = x.Stop_Name, Value = x.StopId.ToString() });


                var driverList = (from vehicle in db.Vehicles
                                  join vehicleMap in db.DriverVehicle_Mapping on vehicle.VehicleId equals vehicleMap.VehicleId
                                  join drivers in db.Drivers on vehicleMap.DriverId equals drivers.DriverId
                                  where vehicle.VehicleId == vehichleId
                                  select new { DriverName = drivers.First_Name + " " + drivers.Last_Name, driverId = drivers.DriverId, VehicleId = vehicle.VehicleId }).ToList().Select(x => new SelectListItem { Text = x.DriverName, Value = x.driverId.ToString() });

                ViewData["DriverName"] = driverList;

                var vehicleList = db.Vehicles.ToList().Select(x => new SelectListItem { Value = x.VehicleId.ToString(), Text = x.Vehicle_No });

                stopDetail.vehicleId = vehichleId;

                ViewData["VehicleList"] = vehicleList;

            }
            return View(stopDetail);
        }

        [HttpGet]
        public ActionResult DeleteRoute(string Id)
        {
            if (!String.IsNullOrEmpty(Id))
            {
                int routeId = Convert.ToInt32(Id);

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    db.RoutesStops_Mapping.Where(c => c.RouteId == routeId).ToList().ForEach(p => db.RoutesStops_Mapping.DeleteObject(p));

                    var route = db.Routes.Where(c => c.RouteId == routeId).FirstOrDefault();

                    db.Routes.DeleteObject(route);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("RouteDetails", "Transportation");
        }

    }
}
