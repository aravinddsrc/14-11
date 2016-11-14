using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Globalization;
using System.Text;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using System.Data.SqlClient;
using NPOI.SS.Util;
using System.Data.OleDb;
using System.Data.Common;

namespace DSRCManagementSystem.Controllers
{
    public class LDCourseController : Controller
    {
        [HttpGet]
        public ActionResult AddCoursedetails()
        {
            LDCourseModelList ld = new LDCourseModelList();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                var LevelList = db.Master_TrainingLevel.ToList();
                var TechList = db.Master_TrainingTechnology.ToList();
                var InstructorList = db.TrainingInstructors.ToList();

                string[] l = new string[LevelList.Count];
                for (int j = 0; j < LevelList.Count; j++)
                {
                    l[j] = LevelList[j].LevelName;

                }
                string result1 = string.Join("\n", l);
                TempData["res1"] = result1;
                string[] t = new string[TechList.Count];

                for (int k = 0; k < TechList.Count; k++)
                {

                    t[k] = TechList[k].TechnologyName;

                }

                string result2 = string.Join("\n", t);

                TempData["res2"] = result2;
                string[] s = new string[InstructorList.Count];

                for (int i = 0; i < InstructorList.Count; i++)
                {

                    s[i] = InstructorList[i].InstructorName;

                }
                string result = string.Join("\n", s);

                TempData["res"] = result;

                string Filepath = Server.MapPath("~/CourseTemplate/Course1.xlt");
                CellDataWriter1(1, 1, Filepath, "sheet1");

                DSRCManagementSystem.Models.LDCourseModel ObjLD = new DSRCManagementSystem.Models.LDCourseModel();
                ModelState.Clear();
                List<LDCourseModel> ldm = new List<LDCourseModel>();
                LDCourseModel lModel = new LDCourseModel();
                List<SelectListItem> LevelList1 = new List<SelectListItem>();
                List<SelectListItem> TechList1 = new List<SelectListItem>();
                foreach (var list in LevelList)
                {
                    LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
                }
                lModel.LevelIDList = LevelList1;
                foreach (var list in TechList)
                {
                    TechList1.Add(new SelectListItem { Text = list.TechnologyName, Value = Convert.ToString(list.TechnologyId) });
                }
                lModel.TechIDList = TechList1;
                //foreach (var list in InstructorList)
                //{
                //    InstructorList1.Add(new SelectListItem { Text = list.InstructorName, Value = Convert.ToString(list.InstructorId) });
                //}
                //lModel.InstructorIDList = InstructorList1;
                // ld.LDM.LevelIDList = new SelectList(new[] { new TraingingLevel() { LevelId = 0, LevelName = "" } }.Union(LevelList), "LevelId", "LevelName", 0);
                //    ViewBag.TechIDList = new SelectList(new[] { new TraingingTechnology() { TechnologyId = 0, TechnologyName = "" } }.Union(TechList), "TechnologyId", "TechnologyName", 0);
                ViewBag.InstructorIDList = new SelectList(new[] { new TrainingInstructor() { InstructorId = 0, InstructorName = "" } }.Union(InstructorList), "InstructorId", "InstructorName", 0);
                for (int i = 0; i < 10; i++)
                {
                    LDCourseModel lm = new LDCourseModel();
                    lm.Coursename = "";
                    lm.LevelId = 0;
                    lm.TechnologyId = 0;
                    //ldm.LDM.
                    lm.Scheduledate = null;
                    ldm.Add(lm);
                }

                ld.ldmlist = ldm;
                ld.LDM = lModel;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View("AddCoursedetails", ld);
        }

        [HttpPost]
        public ActionResult AddCoursedetails(LDCourseModelList model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            LDCourseModelList ld = new LDCourseModelList();
            try
            {
                for (int i = 0; i < model.ldmlist.Count; i++)
                {
                    var LDobj = db.Trainings.CreateObject();
                    if (model.ldmlist[i].TechnologyId != 0)
                    {
                        LDobj.TechnologyId = model.ldmlist[i].TechnologyId;
                        LDobj.LevelId = model.ldmlist[i].LevelId;
                        LDobj.ScheduledDate = Convert.ToDateTime(model.ldmlist[i].Scheduledate);
                        LDobj.TrainingName = model.ldmlist[i].Coursename;
                        LDobj.IsActive = true;

                        LDobj.InstructorId = model.ldmlist[i].InstructorID;
                        db.Trainings.AddObject(LDobj);
                        db.SaveChanges();
                    }
                }
                var LevelList = db.Master_TrainingLevel.ToList();
                var TechList = db.Master_TrainingTechnology.ToList();
                var InstructorList = db.TrainingInstructors.ToList();
                List<LDCourseModel> ldm = new List<LDCourseModel>();
                LDCourseModel lModel = new LDCourseModel();
                List<SelectListItem> LevelList1 = new List<SelectListItem>();
                List<SelectListItem> TechList1 = new List<SelectListItem>();
                foreach (var list in LevelList)
                {
                    LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
                }
                lModel.LevelIDList = LevelList1;
                foreach (var list in TechList)
                {
                    TechList1.Add(new SelectListItem { Text = list.TechnologyName, Value = Convert.ToString(list.TechnologyId) });
                }
                lModel.TechIDList = TechList1;
                //foreach (var list in InstructorList)
                //{
                //    InstructorList1.Add(new SelectListItem { Text = list.InstructorName, Value = Convert.ToString(list.InstructorId) });
                //}
                //lModel.InstructorIDList = InstructorList1;
                // ld.LDM.LevelIDList = new SelectList(new[] { new TraingingLevel() { LevelId = 0, LevelName = "" } }.Union(LevelList), "LevelId", "LevelName", 0);
                //    ViewBag.TechIDList = new SelectList(new[] { new TraingingTechnology() { TechnologyId = 0, TechnologyName = "" } }.Union(TechList), "TechnologyId", "TechnologyName", 0);
                ViewBag.InstructorIDList = new SelectList(new[] { new TrainingInstructor() { InstructorId = 0, InstructorName = "" } }.Union(InstructorList), "InstructorId", "InstructorName", 0);

                for (int i = 0; i < 10; i++)
                {
                    LDCourseModel lm = new LDCourseModel();
                    lm.Coursename = "";
                    lm.LevelId = 0;
                    lm.TechnologyId = 0;
                    //ldm.LDM.
                    lm.Scheduledate = null;
                    ldm.Add(lm);
                }
                ld.ldmlist = ldm;
                ld.LDM = lModel;

                // var LDobj = db.Trainings.CreateObject();
                // LDobj.TechnologyId = model.TechnologyId;
                // LDobj.LevelId = model.LevelId;
                // LDobj.ScheduledDate = Convert.ToDateTime(model.Scheduledate);
                //LDobj.CourseName = model.Coursename;
                // LDobj.IsActive = true;

                // LDobj.InstructorId = model.InstructorID;
                // db.Trainings.AddObject(LDobj);
                // db.SaveChanges();
                // return Json("Success", JsonRequestBehavior.AllowGet);
                //}
                //else
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(ld);
        }

        //[HttpGet]
        //public ActionResult AddNewRow()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    DSRCManagementSystem.Models.LDModel ObjLD1 = new DSRCManagementSystem.Models.LDModel();
        ////bjLD1.Scheduledate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        //    var LevelList = db.TrainingLevels.ToList();
        //   var InstructorList = db.TrainingInstructors.ToList();
        //    var TechList = db.TrainingTechnologies.ToList();
        //    ViewBag.LevelIDList = new SelectList(new[] { new TrainingLevel() { LevelId = 0, LevelName = "" } }.Union(LevelList), "LevelId", "LevelName", 0);
        //    ViewBag.TechIDList = new SelectList(new[] { new TrainingTechnology() { TechnologyId = 0, TechnologyName = "" } }.Union(TechList), "TechnologyId", "TechnologyName", 0);
        //   ViewBag.InstructorIDList = new SelectList(new[] { new TrainingInstructor() { InstructorId = 0, InstructorName = "" } }.Union(InstructorList), "InstructorId", "InstructorName", 0);
        //    return PartialView("_AddCourseDetails");


        //}

        [HttpPost]
        public ActionResult UploadFile()
        {
            try
            {
                string folderPath = Server.MapPath("~/CourseTemplate/Course1.xls");
                OleDbConnection OleDbcon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + folderPath + ";Extended Properties=Excel 12.0;");
                OleDbCommand command = new OleDbCommand("SELECT * FROM [Sheet1$]", OleDbcon);
                OleDbDataAdapter objAdapter1 = new OleDbDataAdapter(command);
                OleDbcon.Open();
                // Create DbDataReader to Data Worksheet
                DbDataReader dr = command.ExecuteReader();
                string constr = @"Data Source=DSRCMCSP16;Initial Catalog=DSRCHRMS1;Integrated Security=True;user id=rebar;password=rebar@123";

                // Bulk Copy to SQL Server
                SqlBulkCopy bulkInsert = new SqlBulkCopy(constr);
                bulkInsert.DestinationTableName = "Trainings";
                bulkInsert.WriteToServer(dr);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }




        private void CellDataWriter1(int row, int col, string FilePath, string SheetName)
        {
            var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            var templateWorkbook = new HSSFWorkbook(fs);
            var sheet = (HSSFSheet)templateWorkbook.GetSheet(SheetName);
            int r = row;
            string result1 = TempData["res1"].ToString();
            string result2 = TempData["res2"].ToString();
            string result = TempData["res"].ToString();
            int c = col;
            CellRangeAddressList addressList1 = new CellRangeAddressList(1, 50, 1, 1);
            DVConstraint dvConstraint1 = DVConstraint.CreateExplicitListConstraint(
            new String[] { result1 });
            HSSFDataValidation dataValidation1 = new HSSFDataValidation(addressList1, dvConstraint1);
            dataValidation1.SuppressDropDownArrow = false;
            ((HSSFSheet)sheet).AddValidationData(dataValidation1);

            CellRangeAddressList addressList2 = new CellRangeAddressList(1, 50, 2, 2);
            DVConstraint dvConstraint2 = DVConstraint.CreateExplicitListConstraint(
            new String[] { result2 });
            HSSFDataValidation dataValidation2 = new HSSFDataValidation(addressList2, dvConstraint2);
            dataValidation2.SuppressDropDownArrow = false;
            ((HSSFSheet)sheet).AddValidationData(dataValidation2);
            CellRangeAddressList addressList = new CellRangeAddressList(1, 50, 4, 4);
            DVConstraint dvConstraint = DVConstraint.CreateExplicitListConstraint(
            new String[] { result });
            HSSFDataValidation dataValidation = new HSSFDataValidation(addressList, dvConstraint);
            dataValidation.SuppressDropDownArrow = false;
            ((HSSFSheet)sheet).AddValidationData(dataValidation);
            fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            templateWorkbook.Write(fs);
            fs.Close();
        }
    }
}





