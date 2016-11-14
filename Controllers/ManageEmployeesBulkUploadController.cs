using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.IO;
using DSRCManagementSystem.DSRCLogic;
using Utilities;
using System.Net.Mail;
using System.Data;
using System.Data.Objects;
using DSRCManagementSystem.Controllers;
using System.Web.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Configuration;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using System.Text.RegularExpressions;


namespace DSRCManagementSystem.Controllers
{
    public class ManageEmployeesBulkUploadController : Controller
    {

        HSSFWorkbook wb;
        HSSFSheet sh;
        DataTable table = new DataTable();
        DataTable PresAddtable = new DataTable();
        DataTable PermanentAddtable = new DataTable();
        DataTable UserSkillsTable = new DataTable();
        DataTable UserRoleTable = new DataTable();
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ManageEmployeesBulkUpload()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ManageEmployeesBulkUpload model = new ManageEmployeesBulkUpload();
            try
            {
                List<SelectListItem> Branches = GetBranches();
                Branches.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });


                model.BranchList = Branches;
                model.ErrorSuccessMessage = null;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return View(model);

        }
        private List<SelectListItem> GetBranches()
        {
            var BranchesList = new List<SelectListItem>();
            try
            {

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<Master_Branches> BranchList = db.Master_Branches.ToList();
                    foreach (var item in BranchList)
                    {
                        BranchesList.Add(new SelectListItem { Text = item.BranchName, Value = item.BranchID.ToString() });
                    }

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return BranchesList;
        }
        [HttpPost]
        public ActionResult ManageEmployeesBulkUpload(ManageEmployeesBulkUpload Model)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                string folderPath = Server.MapPath("~/FileManager/" + userId + Session.SessionID + "/" + userId + DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss"));

                var Excelfile = Model.excelFile;
                List<SelectListItem> Branches = GetBranches();
                Branches.Insert(0, new SelectListItem { Text = "--Select--", Value = "0" });
                Model.BranchList = Branches;


                if (Excelfile != null)
                {
                    var Extension = Path.GetExtension(Excelfile.FileName);

                    if ((Extension == ".xlsx") || Extension == ".xls")
                    {
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);
                        var fileName = Path.GetFileName(Excelfile.FileName);

                        var path = Path.Combine(folderPath, fileName);
                        Excelfile.SaveAs(path);
                        ExcelUtility objExcelUtility = new ExcelUtility();
                        try
                        {
                            //    OleDbConnection OleDbcon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;");
                            //    string sqlConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0";
                            //    OleDbCommand cmd = new OleDbCommand("SELECT [First Name],[Last Name],[Mobile Number],[Email Address] FROM [Sheet1$]", OleDbcon);
                            //    OleDbDataAdapter objAdapter1 = new OleDbDataAdapter(cmd);
                            //    DataSet ds = new DataSet();
                            //    objAdapter1.Fill(ds);
                            //    DataTable dt = new DataTable();
                            //    dt = ds.Tables[0];

                            //    OleDbcon.Open();
                            //    OleDbDataReader dReader;
                            //    dReader = cmd.ExecuteReader();

                            //    SqlBulkCopy sqlBulk = new SqlBulkCopy(ConfigurationManager.AppSettings["connstr"]);
                            //    //Give your Destination table name
                            //    sqlBulk.DestinationTableName = "users";;
                            //    sqlBulk.WriteToServer(dReader);
                            //    OleDbcon.Close();

                            string result = ImportEmployeeExcelToDatabase(path, Model.BranchID);
                            if (result == "")
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(ConfigurationManager.AppSettings["connstr"], SqlBulkCopyOptions.KeepIdentity))
                                {

                                    sqlBulk.DestinationTableName = "users";
                                    sqlBulk.WriteToServer(table);

                                    sqlBulk.DestinationTableName = "PERMANENTADDRESS";
                                    sqlBulk.WriteToServer(PermanentAddtable);

                                    sqlBulk.DestinationTableName = "Presentaddress";
                                    sqlBulk.WriteToServer(PresAddtable);

                                    sqlBulk.DestinationTableName = "UserRoles";
                                    sqlBulk.WriteToServer(UserRoleTable);

                                    sqlBulk.DestinationTableName = "UserSkills";
                                    sqlBulk.WriteToServer(UserSkillsTable);

                                    Model.ErrorSuccessMessage = "Excel Uploaded Successfully.";
                                }

                            }
                            else
                            {
                                Model.ErrorSuccessMessage = result;

                            }

                        }
                        catch (Exception Ex)
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                            ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                            Model.ErrorSuccessMessage = "Error: " + Ex.Message;
                            return View(Model);
                        }
                    }
                    else
                    {
                        Model.ErrorSuccessMessage = "Please upload excel files only.";
                    }
                }
                else
                {
                    Model.ErrorSuccessMessage = "Please upload the excel file.";
                }



            }


            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Model);
        }

        public string ImportEmployeeExcelToDatabase(string filePath, int BranchID)
        {

            string ServerName = AppValue.GetFromMailAddress("ServerName");
            ManageEmployeesBulkUpload Model = new ManageEmployeesBulkUpload();
            int userId = Convert.ToInt32(Session["UserID"]);
            string result = "";
            int PermanentID = 0;
            int PresentID = 0;
            List<string> bulkempid = new List<string>();
            List<string> bulkuid = new List<string>();
            List<string> bulkfname = new List<string>();
            List<string> bulklname = new List<string>();
            List<string> bulkbranch = new List<string>();
            List<string> bulkdept = new List<string>();
            List<string> bulkjdate = new List<string>();
            List<string> bulkexp = new List<string>();
            List<string> bulkdesg = new List<string>();
            string MailBody = "";

            try
            {
                var PermanentMax = db.PermanentAddresses.Where(x => x.IsActive == true).Select(x => x.PermanentAddressID).Max();
                PermanentID = Convert.ToInt32(PermanentMax) + 1;
            }
            catch (Exception ex)
            {
                PermanentID = 1;
            }
            try
            {
                var PersentMax = db.PresentAddresses.Where(x => x.IsActive == true).Select(x => x.PresentAddressID).Max();
                PresentID = Convert.ToInt32(PersentMax) + 1;
            }
            catch (Exception ex)
            {
                PresentID = 1;
            }


            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);
            List<string> sheets = new List<string>();

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                wb = new HSSFWorkbook(fs);

                for (int i = 0; i < wb.Count; i++)
                {
                    sheets.Add(wb.GetSheetAt(i).SheetName);
                }
            }

            if (!IsValidExcel(wb))
            {
                throw new InvalidDataException("The uploaded document does not contain the valid data.");
            }

            //DataTable excelTable = GetExcelTable();

            foreach (string sheetName in sheets)
            {
                sh = (HSSFSheet)wb.GetSheet(sheetName);
                IRow headerRow = sh.GetRow(0);
                string password = GenerateRandomPassword(10);
                string newpassword = DSRCLogic.Hashing.Create_SHA256(password);
                //var EmpID = db.Users.OrderByDescending(x => x.UserID).Select(x => x.EmpID).First();
                String NewEmpID = "";
                //if (EmpID != null)
                //{
                //    NewEmpID = (Convert.ToInt32(EmpID) + 1).ToString();
                //}
                //else
                //{
                //    NewEmpID = "1";
                //}


                var EmpID = db.Users.Select(x => x.EmpID).Distinct().OrderByDescending(x => x).Skip(1).ToList();
                string Empid = (EmpID.Max());

                try
                {
                    
                    if (db.Users.Any(R => R.EmpID == Model.EmpID))
                    {
                        ModelState.AddModelError("EmpID", "EmpID  Already Exists");
                        throw new ArgumentNullException("EmpID", "EmpID  Already Exists");

                    }
                    else if (Empid == null)
                    {
                        ModelState.AddModelError("EmpID", "EmpID");

                    }
                    else
                    {
                        if (EmpID != null)
                        {

                            if (Empid.Length == 1)
                            {
                                NewEmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                            }
                            else if (Empid.Length == 2)
                            {
                                NewEmpID = "000" + (Convert.ToInt32(Empid) + 1);
                            }
                            else if (Empid.Length == 3)
                            {
                                NewEmpID = "00" + (Convert.ToInt32(Empid) + 1);
                            }
                            else if (Empid.Length == 4)
                            {
                                NewEmpID = "0" + (Convert.ToInt32(Empid) + 1);
                            }
                            else
                            {
                                string displayString = string.Empty;

                                //var temp=Empid.Split('/');
                                //string tempid=temp[2];
                                //int id = Convert.ToInt32(tempid);
                                //id = id + 1;
                                //string autoId = "DSRC" + String.Format("{0:0000}", id);

                                //ViewBag.EmpID = "" + (Convert.ToInt32(tempid) + 1);
                                ////if (string.IsNullOrEmpty(Empid))
                                ////{
                                ////{
                                ////    jid = "AM0000";//This string value has to increment at every time, but it is getting increment only one time.
                                ////}
                                int len = Empid.Length;
                                string split = Empid.Substring(4, len - 4);
                                int num = Convert.ToInt32(split);
                                num++;
                                displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                //ViewBag.EmpId = displayString;
                                NewEmpID = displayString;

                            }
                            if (db.Users.Any(R => R.EmpID == Empid))
                            {
                                string displayString = string.Empty;
                                int len = Empid.Length;
                                string split = Empid.Substring(4, len - 4);
                                int num = Convert.ToInt32(split);
                                num++;
                                displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                //ViewBag.EmpId = displayString;
                                NewEmpID = displayString;
                            }
                        }


                        //}
                        else
                        {
                            NewEmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException("EmpID", "EmpID  Already Exists");
                }



                var UserID = db.Users.OrderByDescending(x => x.UserID).First();
                int NewUserID = 0;
                if (UserID != null)
                {
                    NewUserID = Convert.ToInt32(UserID.UserID) + 1;
                }
                else
                {
                    NewUserID = 1;
                }
                var UserRole = db.UserRoles.OrderByDescending(x => x.UserRoleID).First();
                int NewUserRoleID = 0;
                if (UserRole != null)
                {
                    NewUserRoleID = Convert.ToInt32(UserRole.UserRoleID) + 1;
                }
                else
                {
                    NewUserRoleID = 1;
                }
                var UserSkills = db.UserSkills.OrderByDescending(x => x.ID).First();
                int NewUserSkillID = 0;
                if (UserSkills != null)
                {
                    NewUserSkillID = Convert.ToInt32(UserSkills.ID) + 1;
                }
                else
                {
                    NewUserSkillID = 1;
                }
                //string EmpCode;
                //var a = Regex.Matches(EmpID, @"[a-zA-Z]").Count;
                //if (NewEmpID.Length <= 5 && a == 0 || a != 0)
                //{
                //    if (NewEmpID.Length == 1 && a == 0)
                //    {
                //        NewEmpID = "0000" + NewEmpID;

                //    }
                //    if (NewEmpID.Length == 2 && a == 0)
                //    {
                //        NewEmpID = "000" + NewEmpID;

                //    }
                //    if (EmpID.Length == 3 && a == 0)
                //    {
                //        EmpID = "00" + EmpID;
                //        NewEmpID = EmpID;


                //    }
                //    if (NewEmpID.Length == 4 && a == 0)
                //    {
                //        NewEmpID = "0" + NewEmpID;

                //    }

                //}
                List<User> EmpList = new List<User>();
                int colCount = headerRow.LastCellNum;
                int rowCount = sh.LastRowNum  ;
                if (String.IsNullOrWhiteSpace(Convert.ToString(sh.GetRow(rowCount).GetCell(1))) == false || Convert.ToString(sh.GetRow(rowCount).GetCell(1)) != null || Convert.ToString(sh.GetRow(rowCount).GetCell(1)) != string.Empty)
                    {
                        rowCount = rowCount + 1;
                    }
              
                table = GetUserTable();
                PresAddtable = GetPresAddTable();
                PermanentAddtable = GetPermAddTable();
                UserSkillsTable = GetUserSkillsTable();
                UserRoleTable = GetUserRoleTable();
                for (int i = 4; i < rowCount; i++)
                {

                   
                    var listGetValues = new List<string>();
                                        
                    for (int j = 1; j <= colCount; j++)
                    {

                        listGetValues.Add(Convert.ToString(sh.GetRow(i).GetCell(j)));
                    }
                    if (listGetValues[0] != "" || listGetValues[0] != string.Empty)
                    {
                        
                        result = CheckValidEmployees(listGetValues);
                        if (result != "")
                        {
                            Model.ErrorSuccessMessage = result;
                            goto Loop;
                        }

                        else
                        {


                            DataRow dr = table.NewRow();
                            //dr["UserID"] = NewUserID;

                            //dr["FirstName"] = listGetValues[0];
                            //dr["MiddleName"] = null;
                            //dr["LastName"] = listGetValues[1];
                            //dr["UserName"] = listGetValues[2];
                            //dr["Password"] = newpassword;
                            //dr["DateOfBirth"] = listGetValues[3];
                            //dr["DateOfJoin"] = listGetValues[4];
                            //dr["ContactNo"] = listGetValues[5];
                            //dr["EmailAddress"] = listGetValues[6];
                            //dr["IPAddress"] = null;
                            //dr["MachineName"] = null;
                            //string Reportingto = listGetValues[7];
                            //dr["DirectReportingTo"] = db.Users.Where(x => x.FirstName == Reportingto).Select(o => o.UserID).FirstOrDefault();
                            //dr["IsUnderProbation"] = true;
                            //dr["IsUnderNoticePeriod"] = true;
                            //dr["IsFirstLogin"] = true;
                            //dr["CreatedAt"] = DBNull.Value;
                            //dr["LastAccessed"] = DBNull.Value;
                            //dr["IsActive"] = true;
                            //string DepartmentName = listGetValues[8];

                            //dr["DepartmentId"] = Convert.ToInt32(db.Departments.Where(x => x.DepartmentName == DepartmentName).Select(o => o.DepartmentId).FirstOrDefault());
                            //string Gender = listGetValues[9];
                            //dr["Gender"] = Convert.ToInt32(db.Master_Gender.Where(x => x.GenderName == Gender).Select(o => o.GenderID).FirstOrDefault());
                            //dr["PermanentAddressID"] = PermanentID;
                            //dr["TemporaryAddressID"] = PresentID;

                            //dr["ResignedOn"] = DBNull.Value;
                            //dr["LastWorkingDate"] = DBNull.Value;
                            //dr["Experience"] = listGetValues[10] + "." + listGetValues[11];
                            //dr["IsBoarding"] = 0;
                            //dr["OfficeSkypeId"] = DBNull.Value;
                            //dr["Attempts"] = 0;
                            //dr["Key"] = DBNull.Value;
                            //dr["PasswordKey"] = DBNull.Value;
                            //dr["IsReseted"] = DBNull.Value;
                            //string workplace = listGetValues[12];
                            //dr["Workplace"] = db.Master_WorkPlace.Where(x => x.WorkPlaceName == workplace).Select(x => x.WorkPlaceID).FirstOrDefault();
                            //dr["IsPasswordReseted"] = DBNull.Value;
                            //dr["officeno"] = 0;
                            //dr["extension"] = DBNull.Value;
                            //string Branch = listGetValues[13];

                            //dr["IsExclude"] = Convert.ToInt16(db.Master_Branches.Where(x => x.BranchName == Branch).Select(x => x.BranchID).FirstOrDefault()) != 1 ? true : false;
                            //dr["MaritalStatus"] = listGetValues[23] != "" ? true : false; ;
                            //dr["CreatedUserID"] = userId;
                            //dr["BranchId"] = Convert.ToInt16(Convert.ToInt16(db.Master_Branches.Where(x => x.BranchName == Branch).Select(x => x.BranchID).FirstOrDefault()));
                            //string designation = listGetValues[14];
                            //dr["DesignationID"] = Convert.ToInt32(db.Master_Designation.Where(x => x.DesignationName.Contains(designation)).Select(o => o.DesignationID).FirstOrDefault());
                            //int DesignationID = Convert.ToInt32(db.Master_Designation.Where(x => x.DesignationName.Contains(designation)).Select(o => o.DesignationID).FirstOrDefault());
                            //dr["UserStatus"] = 3;
                            //string Region = listGetValues[15];
                            //dr["Region"] = Convert.ToInt32(db.TimeZones.Where(x => x.Zone == Region).Select(o => o.Id).FirstOrDefault());
                            //string Groups = listGetValues[16];
                            //dr["DepartmentGroup"] = db.DepartmentGroups.Where(x => x.GroupName == Groups).Select(x => x.GroupID).FirstOrDefault();
                            //dr["FatherName"] = listGetValues[17];
                            //dr["MotherName"] = listGetValues[18];
                            //string bloodgroup = listGetValues[19];
                            //dr["BloodGroup"] = db.Master_BloodGroup.Where(x => x.BloodGroupName == bloodgroup).Select(x => x.BloodGroupID).FirstOrDefault();
                            //dr["BirthPlace"] = listGetValues[20];
                            //string Nationality = listGetValues[21];
                            //dr["NationalityID"] = db.Master_Nationality.Where(x => x.NationalityName == Nationality).Select(x => x.NationalityID).FirstOrDefault();
                            //string Religion = listGetValues[22];
                            //dr["ReligionID"] = db.Master_Religious.Where(x => x.ReligiousName == Religion).Select(x => x.ReligiousID).FirstOrDefault();
                            //dr["SpouseName"] = listGetValues[23];
                            //if (listGetValues[24] != "")
                            //{

                            //    dr["NoOfChild"] = Convert.ToInt32(listGetValues[24]);
                            //}
                            //else
                            //{
                            //    dr["NoOfChild"] = DBNull.Value;
                            //}
                            //dr["AnniversaryDate"] = listGetValues[25];
                            //dr["EmergencyContact"] = listGetValues[26];
                            //string Relationship = listGetValues[27];
                            //dr["RelationshipID"] = db.Master_Relationship.Where(x => x.RelationshipName == Relationship).Select(x => x.RelationshipID).FirstOrDefault();
                            //dr["Contactperson"] = listGetValues[28];
                            //string Role = listGetValues[29];
                            //dr["RoleID"] = db.Master_Roles.Where(x => x.RoleName == Role).Select(x => x.RoleID).FirstOrDefault();
                            //var DesigEmp = db.Master_Designation.Where(x => x.DesignationID == DesignationID).Select(x => x.DesignationDescription).First();


                            dr["UserID"] = NewUserID;
                           
                            dr["FirstName"] = listGetValues[0];
                            bulkfname.Add(listGetValues[0]);
                            dr["MiddleName"] = null;
                            dr["LastName"] = listGetValues[1];
                            bulklname.Add(listGetValues[1]);
                            dr["UserName"] = listGetValues[10];
                            bulkuid.Add(listGetValues[10]);
                            dr["Password"] = newpassword;
                            dr["DateOfBirth"] = listGetValues[15];
                            dr["DateOfJoin"] = listGetValues[11];
                            bulkjdate.Add(listGetValues[11]);
                            dr["ContactNo"] = listGetValues[2];
                            dr["EmailAddress"] = listGetValues[3];
                            dr["IPAddress"] = null;
                            dr["MachineName"] = null;
                            string Reportingto = listGetValues[12];
                            dr["DirectReportingTo"] = db.Users.Where(x => (x.FirstName+" "+x.LastName) == Reportingto).Select(o => o.UserID).FirstOrDefault();
                            dr["IsUnderProbation"] = true;
                            dr["IsUnderNoticePeriod"] = true;
                            dr["IsFirstLogin"] = true;
                            dr["CreatedAt"] = DBNull.Value;
                            dr["LastAccessed"] = DBNull.Value;
                            dr["IsActive"] = true;
                            string DepartmentName = listGetValues[6];
                            bulkdept.Add(listGetValues[6]);
                            dr["DepartmentId"] = Convert.ToInt32(db.Departments.Where(x => x.DepartmentName == DepartmentName).Select(o => o.DepartmentId).FirstOrDefault());
                            string Gender = listGetValues[4];
                            dr["Gender"] = Convert.ToInt32(db.Master_Gender.Where(x => x.GenderName == Gender).Select(o => o.GenderID).FirstOrDefault());
                            dr["PermanentAddressID"] = PermanentID;
                            dr["TemporaryAddressID"] = PresentID;

                            dr["ResignedOn"] = DBNull.Value;
                            dr["LastWorkingDate"] = DBNull.Value;
                            dr["Experience"] = listGetValues[39] + "." + listGetValues[40];
                            bulkexp.Add(listGetValues[39] + "." + listGetValues[40]);
                            dr["IsBoarding"] = 0;
                            dr["OfficeSkypeId"] = DBNull.Value;
                            dr["Attempts"] = 0;
                            dr["Key"] = DBNull.Value;
                            dr["PasswordKey"] = DBNull.Value;
                            dr["IsReseted"] = DBNull.Value;
                            string workplace = listGetValues[8];
                            dr["Workplace"] = db.Master_WorkPlace.Where(x => x.WorkPlaceName == workplace).Select(x => x.WorkPlaceID).FirstOrDefault();
                            dr["IsPasswordReseted"] = DBNull.Value;
                            dr["officeno"] = 0;
                            dr["extension"] = DBNull.Value;
                            string Branch = listGetValues[5];
                            bulkbranch.Add(listGetValues[5]);

                            dr["IsExclude"] = Convert.ToInt16(db.Master_Branches.Where(x => x.BranchName == Branch).Select(x => x.BranchID).FirstOrDefault()) != 1 ? true : false;
                            dr["MaritalStatus"] = listGetValues[22] != "" ? 1 : 2; ;
                            dr["CreatedUserID"] = userId;
                            dr["BranchId"] = Convert.ToInt16(Convert.ToInt16(db.Master_Branches.Where(x => x.BranchName == Branch).Select(x => x.BranchID).FirstOrDefault()));
                            string designation = listGetValues[9];
                            bulkdesg.Add(listGetValues[9]);
                            
                            dr["DesignationID"] = Convert.ToInt32(db.Master_Designation.Where(x => x.DesignationName.Contains(designation)).Select(o => o.DesignationID).FirstOrDefault());
                            int DesignationID = Convert.ToInt32(db.Master_Designation.Where(x => x.DesignationName.Contains(designation)).Select(o => o.DesignationID).FirstOrDefault());
                            dr["UserStatus"] = 3;
                            string Region = listGetValues[14];
                            dr["Region"] = Convert.ToInt32(db.TimeZones.Where(x => x.Zone == Region).Select(o => o.Id).FirstOrDefault());
                            string Groups = listGetValues[7];
                            dr["DepartmentGroup"] = db.DepartmentGroups.Where(x => x.GroupName == Groups).Select(x => x.GroupID).FirstOrDefault();
                            dr["FatherName"] = listGetValues[18];
                            dr["MotherName"] = listGetValues[17];
                            string bloodgroup = listGetValues[16];
                            dr["BloodGroup"] = db.Master_BloodGroup.Where(x => x.BloodGroupName == bloodgroup).Select(x => x.BloodGroupID).FirstOrDefault();
                            dr["BirthPlace"] = listGetValues[19];
                            string Nationality = listGetValues[20];
                            dr["NationalityID"] = db.Master_Nationality.Where(x => x.NationalityName == Nationality).Select(x => x.NationalityID).FirstOrDefault();
                            string Religion = listGetValues[21];
                            dr["ReligionID"] = db.Master_Religious.Where(x => x.ReligiousName == Religion).Select(x => x.ReligiousID).FirstOrDefault();
                            dr["SpouseName"] = listGetValues[22];
                            if (listGetValues[22] != "")
                            {
                                if (listGetValues[23] != "")
                                {

                                    dr["NoOfChild"] = Convert.ToInt32(listGetValues[23]);
                                }
                                else
                                {
                                    dr["NoOfChild"] = DBNull.Value;
                                }
                                if (listGetValues[24] != "")
                                {
                                    dr["AnniversaryDate"] = listGetValues[24];
                                }
                                else
                                {
                                    dr["AnniversaryDate"] = DBNull.Value;
                                }
                            }
                            dr["EmergencyContact"] = listGetValues[42];
                            string Relationship = listGetValues[43];
                            dr["RelationshipID"] = db.Master_Relationship.Where(x => x.RelationshipName == Relationship).Select(x => x.RelationshipID).FirstOrDefault();
                            dr["Contactperson"] = listGetValues[44];
                            string Role = listGetValues[13];
                            dr["RoleID"] = db.Master_Roles.Where(x => x.RoleName == Role).Select(x => x.RoleID).FirstOrDefault();
                            var DesigEmp = db.Master_Designation.Where(x => x.DesignationID == DesignationID).Select(x => x.DesignationDescription).First();



                            //if (NewEmpID.Length <= 5 && a == 0 || a != 0)
                            //{
                            //    if (NewEmpID.Length == 1 && a == 0)
                            //    {
                            //        NewEmpID = "0000" + NewEmpID;

                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //    if (NewEmpID.Length == 2 && a == 0)
                            //    {
                            //        NewEmpID = "000" + NewEmpID;

                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //    if (NewEmpID.Length == 3 && a == 0)
                            //    {
                            //        NewEmpID = "00" + NewEmpID;

                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //    if (NewEmpID.Length == 4 && a == 0)
                            //    {
                            //        NewEmpID = "0" + NewEmpID;

                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //    if (NewEmpID.Length == 5 && a == 0)
                            //    {


                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //    if (a != 0)
                            //    {


                            //        EmpCode = DesigEmp + NewEmpID;

                            //    }
                            //}
                            dr["EmpID"] = NewEmpID;
                            bulkempid.Add(NewEmpID);
                            dr["EmpCode"] = DesigEmp + NewEmpID;
                            table.Rows.Add(dr);



                            DataRow dr1 = PresAddtable.NewRow();
                            dr1["PresentAddressID"] = PresentID;
                            dr1["UserID"] = NewUserID;
                            //dr1["Address_1"] = listGetValues[30];
                            //dr1["Address_2"] = listGetValues[31];
                            //dr1["Address_3"] = listGetValues[32];
                            dr1["Address_1"] = listGetValues[25];
                            dr1["Address_2"] = listGetValues[26];
                            dr1["Address_3"] = listGetValues[27];
                            string Country = listGetValues[28];
                            dr1["CountryID"] = db.Master_Country.Where(x => x.CountryName == Country).Select(x => x.CountryID).FirstOrDefault();
                            string State = listGetValues[29];
                            dr1["State"] = db.Master_States.Where(x => x.States == State).Select(x => x.StateID).FirstOrDefault();
                            string City = listGetValues[30];
                            dr1["City"] = db.Master_City.Where(x => x.CityName == City).Select(x => x.CityID).FirstOrDefault();
                            dr1["Zip"] = listGetValues[31];
                            dr1["DateCreated"] = DateTime.Now;
                            dr1["IsActive"] = true;

                            PresAddtable.Rows.Add(dr1);


                            DataRow dr2 = PermanentAddtable.NewRow();
                            dr2["PermanentAddressID"] = PermanentID;
                            dr2["UserID"] = NewUserID;
                            //dr2["Address_1"] = listGetValues[37];
                            //dr2["Address_2"] = listGetValues[38];
                            //dr2["Address_3"] = listGetValues[39];
                            dr2["Address_1"] = listGetValues[32];
                            dr2["Address_2"] = listGetValues[33];
                            dr2["Address_3"] = listGetValues[34];
                            string PerCountry = listGetValues[35];
                            dr2["CountryID"] = db.Master_Country.Where(x => x.CountryName == PerCountry).Select(x => x.CountryID).FirstOrDefault();
                            string PerState = listGetValues[36];
                            dr2["State"] = db.Master_States.Where(x => x.States == PerState).Select(x => x.StateID).FirstOrDefault();
                            string PerCity = listGetValues[37];
                            dr2["City"] = db.Master_City.Where(x => x.CityName == PerCity).Select(x => x.CityID).FirstOrDefault();
                            dr2["Zip"] = listGetValues[38];
                            dr2["DateCreated"] = DateTime.Now;
                            dr2["IsActive"] = true;

                            PermanentAddtable.Rows.Add(dr2);


                            DataRow dr3 = UserRoleTable.NewRow();
                            dr3["UserRoleID"] = NewUserRoleID;
                            dr3["UserID"] = NewUserID;
                            dr3["RoleID"] = db.Master_Roles.FirstOrDefault(o => o.RoleName == MasterEnum.NewuserRole.NewEmployeeRole).RoleID;
                            UserRoleTable.Rows.Add(dr3);


                            DataRow dr4 = UserSkillsTable.NewRow();
                            dr4["ID"] = NewUserSkillID;
                            dr4["UserID"] = NewUserID;
                            dr4["Skills"] = listGetValues[41];
                            UserSkillsTable.Rows.Add(dr4);

                            NewUserID = NewUserID + 1;
                            PresentID = PresentID + 1;
                            PermanentID = PermanentID + 1;
                            //NewEmpID = (Convert.ToInt32(NewEmpID) + 1).ToString();
                            NewUserRoleID = NewUserRoleID + 1;
                            NewUserSkillID = NewUserSkillID + 1;

                            string Empid1 = NewEmpID;

                            if (NewEmpID != null)
                            {

                                if (Empid1.Length == 1)
                                {
                                    NewEmpID = "0000" + (Convert.ToInt32(Empid1) + 1);
                                }
                                else if (Empid1.Length == 2)
                                {
                                    NewEmpID = "000" + (Convert.ToInt32(Empid1) + 1);
                                }
                                else if (Empid1.Length == 3)
                                {
                                    NewEmpID = "00" + (Convert.ToInt32(Empid1) + 1);
                                }
                                else if (Empid1.Length == 4)
                                {
                                    NewEmpID = "0" + (Convert.ToInt32(Empid1) + 1);
                                }
                                else
                                {
                                    string displayString = string.Empty;

                                    //var temp=Empid.Split('/');
                                    //string tempid=temp[2];
                                    //int id = Convert.ToInt32(tempid);
                                    //id = id + 1;
                                    //string autoId = "DSRC" + String.Format("{0:0000}", id);

                                    //ViewBag.EmpID = "" + (Convert.ToInt32(tempid) + 1);
                                    ////if (string.IsNullOrEmpty(Empid))
                                    ////{
                                    ////{
                                    ////    jid = "AM0000";//This string value has to increment at every time, but it is getting increment only one time.
                                    ////}
                                    int len = Empid1.Length;
                                    string split = Empid1.Substring(4, len - 4);
                                    int num = Convert.ToInt32(split);
                                    num++;
                                    displayString = Empid1.Substring(0, 4) + num.ToString("0000");
                                    //ViewBag.EmpId = displayString;
                                    NewEmpID = displayString;

                                }
                                if (db.Users.Any(R => R.EmpID == Empid1))
                                {
                                    string displayString = string.Empty;
                                    int len = Empid1.Length;
                                    string split = Empid1.Substring(4, len - 4);
                                    int num = Convert.ToInt32(split);
                                    num++;
                                    displayString = Empid1.Substring(0, 4) + num.ToString("0000");
                                    //ViewBag.EmpId = displayString;
                                    NewEmpID = displayString;
                                }
                            }


                            //}
                            else
                            {
                                NewEmpID = "0000" + (Convert.ToInt32(Empid1) + 1);
                            }

                    }
                    }

                }
                var logo = CommonLogic.getLogoPath();
                // added on 29/9
              //  List<string> ecount = new List<string>();
               // var chk ="";
                //for (int z = 0; z <bulkempid.Count(); z++)
               // {
              //  User chk = db.Users.Where(x => x.EmpID.Equals(bulkempid[0].ToString())).FirstOrDefault();
                  // ecount.Add(chk);
             //  }

                 // if ((chk != null))
              //  {
                    for (int z = 0; z < bulkempid.Count(); z++)
                    {

                        if (bulkempid.Any())
                        {
                            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "New User").Select(o => o.EmailTemplateID).FirstOrDefault();
                            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "New User").Select(x => x.TemplatePath).FirstOrDefault();
                            if ((check != null) && (check != 0))
                            {
                                var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "New User")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();

                                var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                                string newuserhtml = System.IO.File.ReadAllText(TemplatePathnewuser);
                                newuserhtml = newuserhtml.Replace("#UserName", bulkfname[z] + "  " + bulklname[z]);
                                newuserhtml = newuserhtml.Replace("#LoginID", Convert.ToString(bulkuid[z]));
                                newuserhtml = newuserhtml.Replace("#Password", password);
                                newuserhtml = newuserhtml.Replace("#ServerName", ServerName);
                                newuserhtml = newuserhtml.Replace("#CompanyName", company);
                                if (objnewuser.To != "")
                                {
                                    objnewuser.To = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, objnewuser.To);
                                }
                                if (objnewuser.CC != "")
                                {
                                    objnewuser.BCC = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, objnewuser.CC);
                                }
                                if (objnewuser.BCC != "")
                                {
                                    objnewuser.BCC = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, objnewuser.BCC);
                                }

                                // string ServerName = WebConfigurationManager.AppSettings["SeverName"];


                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                string EmailAddress = "";

                                foreach (string mails in MailIds)
                                {
                                    EmailAddress += mails + ",";
                                }
                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                                if (ServerName != "http://win2012srv:88/")
                                {
                                    Task.Factory.StartNew(() =>
                                    {

                                        DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", newuserhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                    });

                                }
                                else
                                {
                                    Task.Factory.StartNew(() =>
                                    {

                                        DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", newuserhtml, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));

                                    });
                                }
                            }
                            else
                            {
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("Bulk Upload", folder, ServerName);
                            }
                        }
                    }

                            TempData["message"] = "Added";

                            var objcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                            string Title = " " + objcompany + " New Employee Added";
                            string Subject = " Employees was added on " + DateTime.Today.ToString("dd MMM yyyy");


                            var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Bulk Upload").Select(o => o.EmailTemplateID).FirstOrDefault();
                            var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Bulk Upload").Select(x => x.TemplatePath).FirstOrDefault();
                            if ((checks != null) && (checks != 0))
                            {
                                var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Manage Employees Upload")
                                           join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                           select new DSRCManagementSystem.Models.Email
                                           {
                                               To = p.To,
                                               CC = p.CC,
                                               BCC = p.BCC,
                                               Subject = p.Subject,
                                               Template = q.TemplatePath
                                           }).FirstOrDefault();

                                string TemplatePath = Server.MapPath(obj.Template);
                                string html = System.IO.File.ReadAllText(TemplatePath);

                              //  foreach (var newmember in bulkempid)
                                for (int z1 = 0; z1 < bulkempid.Count(); z1++)
                                {
                                    //if (newmember.Any())
                                    //{

                                        MailBody += @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"

                                       + bulkempid[z1] + @"</td><td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                       + bulkfname[z1] + " " + bulklname[z1] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                       + bulkbranch[z1] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb; white-space: pre-wrap;'>"
                                       + bulkdept[z1] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                       + bulkdesg[z1] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                       + bulkjdate[z1] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                                       + bulkexp[z1] + @"</td></tr>";


                                   // }
                                }

                                html = html.Replace("#Title", Title);
                                html = html.Replace("#Subject", Subject);
                                html = html.Replace("#MailBody", MailBody);
                                html = html.Replace("#ServerName", ServerName);
                                html = html.Replace("#CompanyName", objcompany);

                                obj.To = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, obj.To);
                                obj.CC = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, obj.CC);

                                if (obj.BCC != "")
                                {
                                    obj.BCC = DSRCManagementSystem.Controllers.ManageEmployeesBulkUploadController.GetUserEmailAddress(db, obj.BCC);
                                }

                                //string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];


                                List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                string EmailAddress1 = "";

                                foreach (string mails in MailIds1)
                                {
                                    EmailAddress1 += mails + ",";
                                }

                                EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);

                                if (ServerName != "http://win2012srv:88/")
                                {


                                    string CCMailId = "aruna.m@dsrc.co.in";
                                    string BCCMailId = "Kirankumar@dsrc.co.in ";

                                    Task.Factory.StartNew(() =>
                                    {
                                        // DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                    });

                                }
                                else
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        // DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", EmailAddress1, obj.CC, obj.BCC, Server.MapPath(logo.AppValue.ToString()));
                                        DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", EmailAddress1, obj.CC, obj.BCC, Server.MapPath(logo.ToString()));

                                    });
                                }
                            }
                            else
                            {
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("Bulk Upload", folders, ServerName);
                            }

                        
                    
              //  }

                //ends


            }
        Loop:
            return result;
         

        }
        private bool IsValidExcel(HSSFWorkbook workbook)
        {

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                if (!IsValidWorksheet(workbook.GetSheetAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsValidWorksheet(ISheet workSheet)
        {
            return IsValidWorksheet(workSheet as HSSFSheet);
        }

        private bool IsValidWorksheet(HSSFSheet workSheet)
        {
            try
            {
                var rowEnumerator = workSheet.GetRowEnumerator();
                while (rowEnumerator.MoveNext())
                {
                    var row = rowEnumerator.Current as IRow;
                    bool isRowValid = true;
                    if (row != null)
                    {
                        switch (row.RowNum)
                        {
                            case 0:
                                isRowValid = IsFirstRowValid(row);
                                break;
                            case 1:
                                isRowValid = IsSecondRowValid(row);
                                break;
                            case 2:
                                isRowValid = IsThirdRowValid(row);
                                break;
                            case 3:
                                isRowValid = IsFourthRowValid(row);
                                break;

                            default:
                                return isRowValid;
                        }
                    }

                    if (!isRowValid)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsFirstRowValid(IRow firstRow)
        {

            try
            {
                if (firstRow == null)
                {
                    throw new ArgumentNullException("firstRow", "First row does not contain the expected data");
                }

                ICell cell = firstRow.GetCell(1);
                string cellData = cell.StringCellValue.Trim();
                if (cell.IsMergedCell && (String.IsNullOrEmpty(cellData) || !cellData.Equals("Data Software Research Co Pvt Ltd", StringComparison.OrdinalIgnoreCase)) && (String.IsNullOrEmpty(cellData) || !cellData.Equals("Organization-1", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        private bool IsSecondRowValid(IRow secondRow)
        {
            try
            {
                if (secondRow == null)
                {
                    throw new ArgumentNullException("secondRow", "Second row does not contain the expected data");
                }

                ICell cell = secondRow.GetCell(1);
                string cellData = cell.StringCellValue.Trim();
                if (cell.IsMergedCell && (String.IsNullOrEmpty(cellData) || !cellData.Equals("Organization-Wise Employees List", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        private bool IsThirdRowValid(IRow thirdRow)
        {
            if (thirdRow == null)
            {
                throw new ArgumentNullException("thirdRow", "Third row does not contain the expected data");
            }

            ICell cell = thirdRow.GetCell(1);
            string cellData = cell.StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Run by:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cell = thirdRow.GetCell(2);
            cellData = cell.StringCellValue.Trim();
            if (cell.IsMergedCell && (String.IsNullOrEmpty(cellData) || !cellData.Equals("System Admin", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        private bool IsFourthRowValid(IRow fourthRow)
        {
            if (fourthRow == null)
            {
                throw new ArgumentNullException("fourthRow", "Fourth row does not contain the expected data");
            }

            ICell cell = fourthRow.GetCell(1);
            string cellData = cell.StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("First Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(2).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Last Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(3).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("ContactNo", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(4).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Email Address", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(5).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Gender", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            cellData = fourthRow.GetCell(6).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Branch", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(7).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Department", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

                           

            cellData = fourthRow.GetCell(8).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Group", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(9).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Workplace", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            cellData = fourthRow.GetCell(10).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Designation", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(11).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("User Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            cellData = fourthRow.GetCell(12).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("DateOfJoin", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }



            cellData = fourthRow.GetCell(13).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Reporting Person", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            cellData = fourthRow.GetCell(14).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Role", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(15).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Region", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(16).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(17).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Blood Group", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(18).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Mother Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(19).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Father Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }



            cellData = fourthRow.GetCell(20).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Birth Place", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            cellData = fourthRow.GetCell(21).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Nationality", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cellData = fourthRow.GetCell(22).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Religion", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(23).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Spouse Name", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(24).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("NoOfChild", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(25).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Anniversary Date", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(26).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present Address1", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(27).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present Address2", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(28).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present Address3", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(29).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present country", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(30).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present State", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(31).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present City", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(32).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Present Pincode", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(33).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent Address1", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(34).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent Address2", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(35).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent Address3", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(36).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent country", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(37).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent State", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(38).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent City", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(39).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Permanent Pincode", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            cellData = fourthRow.GetCell(40).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Experience in Years", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(41).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Experience in Months", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(42).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Skills", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
                 
                     
            cellData = fourthRow.GetCell(43).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Emergency Contact", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(44).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Relationship", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            cellData = fourthRow.GetCell(45).StringCellValue.Trim();
            if (String.IsNullOrEmpty(cellData) || !cellData.Equals("Contact person", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        
           
           
            return true;
        }
        private string GenerateRandomPassword(int length)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-*&#+";
            char[] chars = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }

 private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.EmailAddress).ToList();
            var tmp = "";
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
            return tmp;
        }
        private String CheckValidEmployees(List<string> listGetValues)
        {
            string result = "";

            string FirstName = listGetValues[0];
            string LastName = listGetValues[1];
            string ContactNo = listGetValues[2];
            string EMAIL = listGetValues[3];
            string Gender = listGetValues[4];
            string Branch = listGetValues[5];
            string Department = listGetValues[6];
            string Group = listGetValues[7];
            string Workplace = listGetValues[8];
            string Designation = listGetValues[9];
            string UserName = listGetValues[10];
            string DOJ = listGetValues[11];
            string ReportingPerson = listGetValues[12];
            string Role = listGetValues[13];
            string Region = listGetValues[14];
            string DOB = listGetValues[15];
            string BloodGroup = listGetValues[16];
            string MotherName = listGetValues[17];
            string FatherName = listGetValues[18];
            string BirthPlace = listGetValues[19];
            string Nationality = listGetValues[20];
            string Religion = listGetValues[21];
            string SpouseName = listGetValues[22];
            string NoOfChild = listGetValues[23];
            string AnniversaryDate = listGetValues[24];
            string PresentAdd1 = listGetValues[25];
            string PresentCountry = listGetValues[28];

            string PresentState = listGetValues[29];
            string PresentCity = listGetValues[30];
            string PresentPin = listGetValues[31];
            string PremanentAdd1 = listGetValues[32];
            string PermanentCountry = listGetValues[35];
            string PermanentState = listGetValues[36];
            string PermanentCity = listGetValues[37];
            string Permanentpin = listGetValues[38];
            string Experience = listGetValues[39];           
            string EmergencyContact = listGetValues[42];
            string Relationship = listGetValues[43];
            string Contactperson = listGetValues[44];
            
            

            string ValidEmail = db.Users.Where(x => x.EmailAddress.Contains(EMAIL)).Select(x => x.EmailAddress).FirstOrDefault();

            if (FirstName == string.Empty || LastName == string.Empty || ContactNo == string.Empty || EMAIL == string.Empty || ReportingPerson == string.Empty || Department == string.Empty || Gender == string.Empty ||
                UserName == string.Empty || Designation == string.Empty || Branch == string.Empty || DOJ == string.Empty || DOB == string.Empty || BloodGroup == string.Empty || Role == string.Empty ||
                FatherName == string.Empty || MotherName == string.Empty || EmergencyContact == string.Empty || Relationship == string.Empty || Religion == string.Empty || Contactperson == string.Empty
                || PresentAdd1 == string.Empty || PresentCountry == string.Empty || PresentState == string.Empty || PresentCity == string.Empty || PresentPin == string.Empty || PremanentAdd1 == string.Empty ||
                 PermanentCountry == string.Empty || PermanentState == string.Empty || PermanentCity == string.Empty || Permanentpin == string.Empty)
            {
                result = "Please Enter Mandatory Column";
            }
            else if (Regex.Match(ContactNo, @"^[0-9]{1,999}$").Success == false || Regex.Match(EmergencyContact, @"^[0-9]{1,999}$").Success == false)
            {
                result = "Enter a Valid Phone Number";
            }
            else if (Regex.Match(EMAIL, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z").Success == false)
            {

                result = "Enter a Valid Email Address";
            }
            else if (db.Departments.Where(x => x.DepartmentName.Contains(Department)).Select(x => x.DepartmentName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.Master_Gender.Where(x => x.GenderName.Contains(Gender)).Select(x => x.GenderName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.Master_WorkPlace.Where(x => x.WorkPlaceName.Contains(Workplace)).Select(x => x.WorkPlaceName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.Master_Branches.Where(x => x.BranchName.Contains(Branch)).Select(x=>x.BranchName).FirstOrDefault() == null)
            {
                result = "Error";
            }

            else if (db.Master_Designation.Where(x => x.DesignationName.Contains(Designation)).Select(x=>x.DesignationName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.DepartmentGroups.Where(x => x.GroupName.Contains(Group)).Select(x => x.GroupName).FirstOrDefault() == null)
            {
                result = "Error";
            }

            else if (db.Master_BloodGroup.Where(x => x.BloodGroupName.Contains(BloodGroup)).Select(x=>x.BloodGroupName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.Master_Nationality.Where(x => x.NationalityName.Contains(Nationality)).Select(x=>x.NationalityName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if ((db.Master_States.Where(x => x.States.Contains(PresentState)).Select (x=>x.States).FirstOrDefault() ==null) ||
                (db.Master_States.Where(x => x.States.Contains(PermanentState)).Select(x=>x.States).FirstOrDefault() == null))
            {
                result = "Error";
            }
            else if ((db.Master_City.Where(x => x.CityName.Contains(PresentCity)).Select(x=>x.CityName).FirstOrDefault() == null) ||
                (db.Master_City.Where(x => x.CityName.Contains(PermanentCity)).Select(x=>x.CityName).FirstOrDefault() ==null))
            {
                result = "Error";
            }
            else if ((db.Master_Country.FirstOrDefault(x => x.CountryName.Contains(PermanentCountry)).CountryName.ToString() == null) ||
                (db.Master_Country.Where(x => x.CountryName.Contains(PresentCountry)).Select(x=>x.CountryName).FirstOrDefault() == null))
            {
                result = "Error";
            }
            else if (db.Master_Religious.Where(x => x.ReligiousName.Contains(Religion)).Select(x=>x.ReligiousName).FirstOrDefault() == null)
            {
                result = "Error";
            }
            else if (db.Master_Relationship.Where(x => x.RelationshipName.Contains(Relationship)).Select(x=>x.RelationshipName).FirstOrDefault() ==null)
            {
                result = "Error";
            }
            else if (db.Master_Country.Where(x => x.CountryName.Contains(PresentCountry)).Select(x=>x.CountryName).FirstOrDefault() ==null)
            {
                result = "Error";
            }
            else if (ValidEmail != null)
            {
                result = "Email Address is already Available";

            }
            return result;
        }
        DataTable GetUserTable()
        {

            DataColumn UserID = new DataColumn();
            UserID.ColumnName = "UserID";
            UserID.DataType = System.Type.GetType("System.Int32");
            UserID.AutoIncrement = true;

            table.Columns.Add(UserID);
            table.Columns.Add("EmpID", typeof(string));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("MiddleName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            DataColumn Password = table.Columns.Add("Password", typeof(string));

            Password.MaxLength = -1;

            table.Columns.Add("DateOfBirth", typeof(DateTime));
            table.Columns.Add("DateOfJoin", typeof(DateTime));
            table.Columns.Add("ContactNo", typeof(Int64));
            table.Columns.Add("EmailAddress", typeof(string));
            table.Columns.Add("IPAddress", typeof(string));
            table.Columns.Add("MachineName", typeof(string));
            table.Columns.Add("DirectReportingTo", typeof(string));
            table.Columns.Add("IsUnderProbation", typeof(Boolean));
            table.Columns.Add("IsUnderNoticePeriod", typeof(Boolean));
            table.Columns.Add("IsFirstLogin", typeof(Boolean));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("LastAccessed", typeof(DateTime));
            table.Columns.Add("IsActive", typeof(Boolean));
            table.Columns.Add("DepartmentId", typeof(Int32));
            table.Columns.Add("Gender", typeof(Int32));
            table.Columns.Add("PermanentAddressID", typeof(Int32));
            table.Columns.Add("TemporaryAddressID", typeof(Int32));
            table.Columns.Add("ResignedOn", typeof(DateTime));
            table.Columns.Add("LastWorkingDate", typeof(DateTime));
            table.Columns.Add("Experience", typeof(string));
            table.Columns.Add("IsBoarding", typeof(Boolean));
            table.Columns.Add("OfficeSkypeId", typeof(string));
            table.Columns.Add("Attempts", typeof(Int32));
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("PasswordKey", typeof(int));
            table.Columns.Add("IsReseted", typeof(int));
            table.Columns.Add("Workplace", typeof(string));
            table.Columns.Add("IsPasswordReseted", typeof(int));
            table.Columns.Add("officeno", typeof(int));
            table.Columns.Add("extension", typeof(int));
            table.Columns.Add("IsExclude", typeof(Boolean));
            table.Columns.Add("MaritalStatus", typeof(int));
            table.Columns.Add("CreatedUserID", typeof(int));
            table.Columns.Add("BranchId", typeof(int));
            table.Columns.Add("DesignationID", typeof(int));
            table.Columns.Add("UserStatus", typeof(int));
            table.Columns.Add("Region", typeof(int));
            table.Columns.Add("DepartmentGroup", typeof(int));
            table.Columns.Add("FatherName", typeof(string));
            table.Columns.Add("MotherName", typeof(string));
            table.Columns.Add("BloodGroup", typeof(string));
            table.Columns.Add("BirthPlace", typeof(string));
            table.Columns.Add("NationalityID", typeof(int));
            table.Columns.Add("ReligionID", typeof(int));
            table.Columns.Add("SpouseName", typeof(string));
            table.Columns.Add("NoOfChild", typeof(int));
            table.Columns.Add("AnniversaryDate", typeof(DateTime));
            table.Columns.Add("EmergencyContact", typeof(Int64));
            table.Columns.Add("RelationshipID", typeof(int));
            table.Columns.Add("Contactperson", typeof(string));
            table.Columns.Add("RoleID", typeof(byte));
            table.Columns.Add("EmpCode", typeof(String));
            return table;
        }
        DataTable GetPresAddTable()
        {
            DataColumn PresentAddressID = new DataColumn();
            PresentAddressID.ColumnName = "PresentAddressID";
            PresentAddressID.DataType = System.Type.GetType("System.Int32");
            PresentAddressID.AutoIncrement = true;

            PresAddtable.Columns.Add(PresentAddressID);
            PresAddtable.Columns.Add("UserID", typeof(int));
            PresAddtable.Columns.Add("Address_1", typeof(string));
            PresAddtable.Columns.Add("Address_2", typeof(string));
            PresAddtable.Columns.Add("Address_3", typeof(string));
            PresAddtable.Columns.Add("City", typeof(string));
            PresAddtable.Columns.Add("State", typeof(string));
            PresAddtable.Columns.Add("Zip", typeof(int));
            PresAddtable.Columns.Add("DateCreated", typeof(DateTime));
            PresAddtable.Columns.Add("IsActive", typeof(Boolean));
            PresAddtable.Columns.Add("CountryID", typeof(int));
            return PresAddtable;
        }

        DataTable GetPermAddTable()
        {
            DataColumn PermanentAddressID = new DataColumn();
            PermanentAddressID.ColumnName = "PermanentAddressID";
            PermanentAddressID.DataType = System.Type.GetType("System.Int32");
            PermanentAddressID.AutoIncrement = true;

            PermanentAddtable.Columns.Add(PermanentAddressID);
            PermanentAddtable.Columns.Add("UserID", typeof(int));
            PermanentAddtable.Columns.Add("Address_1", typeof(string));
            PermanentAddtable.Columns.Add("Address_2", typeof(string));
            PermanentAddtable.Columns.Add("Address_3", typeof(string));
            PermanentAddtable.Columns.Add("City", typeof(string));
            PermanentAddtable.Columns.Add("State", typeof(string));
            PermanentAddtable.Columns.Add("Zip", typeof(int));
            PermanentAddtable.Columns.Add("DateCreated", typeof(DateTime));
            PermanentAddtable.Columns.Add("IsActive", typeof(Boolean));
            PermanentAddtable.Columns.Add("CountryID", typeof(int));

            return PermanentAddtable;
        }
        DataTable GetUserRoleTable()
        {
            DataColumn UserRoleID = new DataColumn();
            UserRoleID.ColumnName = "UserRoleID";
            UserRoleID.DataType = System.Type.GetType("System.Int32");
            UserRoleID.AutoIncrement = true;

            UserRoleTable.Columns.Add(UserRoleID);
            UserRoleTable.Columns.Add("UserID", typeof(int));
            UserRoleTable.Columns.Add("RoleID", typeof(byte));

            return UserRoleTable;
        }
        DataTable GetUserSkillsTable()
        {
            DataColumn ID = new DataColumn();
            ID.ColumnName = "ID";
            ID.DataType = System.Type.GetType("System.Int32");
            ID.AutoIncrement = true;

            UserSkillsTable.Columns.Add(ID);
            UserSkillsTable.Columns.Add("UserID", typeof(int));
            UserSkillsTable.Columns.Add("Skills", typeof(string));

            return UserSkillsTable;
        }

    }
}
