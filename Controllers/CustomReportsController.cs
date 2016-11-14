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
using System.Data.SqlClient;
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Reflection;


namespace DSRCManagementSystem.Controllers
{
    public class CustomReportsController : Controller
    {
        [HttpGet]
        public ActionResult DashBoard()
        {

            try{
            int roleid = Convert.ToInt32(Session["RoleID"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.CustomReports> obj = new List<DSRCManagementSystem.Models.CustomReports>();
            ViewBag.List = TempData["Value"];
            ViewBag.Val = TempData["List"];
            if (ViewBag.List == null)
            {
                var Purpose = (from cr in objdb.CustomReports
                               join cru in objdb.CustomReports_UserMapping on cr.ReportID equals cru.ReportID
                               where (cru.RoleID == roleid && cr.IsActive==true)
                               select new
                               {
                                   ReportID = cr.ReportID,
                                   ReportName = cr.ReportName
                               }).ToList();
                ViewBag.Purpose = new SelectList(Purpose, "ReportID", "ReportName");
            }
            if (ViewBag.List != null)
            {
                var Purpose = (from cr in objdb.CustomReports
                               join cru in objdb.CustomReports_UserMapping on cr.ReportID equals cru.ReportID
                               where (cru.RoleID == roleid && cr.IsActive == true)
                               select new
                               {
                                   ReportID = cr.ReportID,
                                   ReportName = cr.ReportName
                               }).ToList();
                var ZoneId = TempData["ReportIDs"];
                int ID = Convert.ToInt32(ZoneId);
                var temp = objdb.CustomReports.Where(r => r.ReportID == ID).Select(f => f.ReportName).ToList();
                foreach (var list in temp)
                {
                    ViewBag.ReportName = list;
                }
                ViewBag.Purpose = new SelectList(Purpose, "ReportID", "ReportName", ZoneId);
            }
            var userid = Convert.ToInt32(Session["UserID"]);
            ViewBag.permission = (from p in objdb.ReportsPermissions
                                  where p.UserId == userid && p.IsAuthorized == true
                                  select p.UserId).SingleOrDefault();
            ViewBag.UserID = Convert.ToInt32(Session["UserID"]);
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
        public ActionResult DashBoard(CustomReports obj, FormCollection form, string Column)
        {
            
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                List<DSRCManagementSystem.Models.CustomReports> value = new List<DSRCManagementSystem.Models.CustomReports>();
                List<DSRCManagementSystem.Models.CustomReports> Report = new List<DSRCManagementSystem.Models.CustomReports>();
                List<DSRCManagementSystem.Models.CustomReports> ExportValue = new List<DSRCManagementSystem.Models.CustomReports>();
                TempData["Column"] = Column;
                ViewBag.Column = Column;
                int roleid = Convert.ToInt32(Session["RoleID"]);
            try
            {


                if (Column == null)
                {

                    string CusReportId = (form["Id3"] == "") ? "0" : form["Id3"].ToString();
                    int cusId = Convert.ToInt32(CusReportId);
                    var Id =
                        objdb.CustomReports.Where(x => x.ReportID == cusId && x.IsActive == true)
                            .Select(o => o.ReportQuery)
                            .FirstOrDefault();
                    string ZoneId = Id;
                    var temp = objdb.CustomReports.Where(r => r.ReportQuery == Id).Select(f => f.ReportName).ToList();
                    foreach (var list in temp)
                    {
                        ViewBag.ReportName = list;
                    }


                    if (CusReportId == Convert.ToString(0))
                    {
                        var Purpose = (from cr in objdb.CustomReports
                            join cru in objdb.CustomReports_UserMapping on cr.ReportID equals cru.ReportID
                            where (cru.RoleID == roleid && cr.IsActive==true)
                            select new
                            {
                                ReportID = cr.ReportID,
                                ReportName = cr.ReportName
                            }).ToList();
                        ViewBag.Purpose = new SelectList(Purpose, "ReportID", "ReportName", 0);
                        ModelState.AddModelError("RoleName", "Select Report Name");
                    }
                    TempData["cusId"] = cusId;
                    TempData["ReportID"] = ZoneId;

                    if (CusReportId != Convert.ToString(0))
                    {
                        List<string> Parameter = new List<string>();
                        List<string> value1 = objdb.SP_GetName(Id).ToList();
                        foreach (var s in value1)
                        {
                            DSRCManagementSystem.Models.CustomReports ob =
                                new DSRCManagementSystem.Models.CustomReports();
                            var trims = s.TrimStart('@');
                            var newValue = trims.Substring(trims.IndexOf('_') + 1);
                            var trim = Regex.Replace(newValue, "([a-z])([A-Z])", "$1 $2");
                            ob.CustomName = trim;
                            value.Add(ob);
                            Parameter.Add(ob.CustomName);
                        }
                        ViewData["Parameter"] = Parameter;
                        ViewBag.Id = value;
                        string id1 = Id;

                        if (value.Count() == 0)
                        {
                            var pose = (from cr in objdb.CustomReports
                                join cru in objdb.CustomReports_UserMapping on cr.ReportID equals cru.ReportID
                                        where (cru.RoleID == roleid && cr.IsActive == true)
                                select new
                                {
                                    ReportID = cr.ReportID,
                                    ReportName = cr.ReportName
                                }).ToList();
                            ViewBag.Purpose = new SelectList(pose, "ReportID", "ReportName", cusId);
                            string constr = ConfigurationManager.AppSettings["connstr"];
                            DataSet ds = new DataSet();
                            SqlConnection objcon = new SqlConnection(constr);
                            SqlCommand cmd = new SqlCommand(id1, objcon);
                            cmd.CommandText = id1;
                            cmd.CommandType = CommandType.StoredProcedure;
                            SqlDataAdapter adap = new SqlDataAdapter(cmd);
                            adap.Fill(ds);
                            List<object> chartData = new List<object>();
                            List<DSRCManagementSystem.Models.CustomReports> value5 =
                                new List<DSRCManagementSystem.Models.CustomReports>();
                            List<object> Listvalue = new List<object>();
                            List<object> Val = new List<object>();
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                DSRCManagementSystem.Models.CustomReports ob =
                                    new DSRCManagementSystem.Models.CustomReports();
                                Array x = dr.ItemArray;
                                ob.ReportName1 = x;
                                Listvalue.Add(ob.ReportName1);
                                chartData.Add(new object[]
                                {
                                    dr[0]
                                });
                                ExportValue.Add(ob);
                            }
                            foreach (DataColumn dr in ds.Tables[0].Columns)
                            {
                                DSRCManagementSystem.Models.CustomReports ob1 =
                                    new DSRCManagementSystem.Models.CustomReports();
                                ob1.CustomNameId = dr.ColumnName;
                                Val.Add(ob1.CustomNameId);
                            }
                            ViewBag.Val = Val;
                            ViewBag.ListValue = Listvalue;
                            ViewBag.data = chartData;
                            return View(ExportValue);
                        }
                        var Purpose = (from cr in objdb.CustomReports
                            join cru in objdb.CustomReports_UserMapping on cr.ReportID equals cru.ReportID
                                       where (cru.RoleID == roleid && cr.IsActive == true)
                            select new
                            {
                                ReportID = cr.ReportID,
                                ReportName = cr.ReportName
                            }).ToList();
                        foreach (var item in value)
                        {
                            ViewBag.Purpose = new SelectList(Purpose, "ReportID", "ReportName", cusId);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {

                //string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                //string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                //ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                //return Json("Warning", JsonRequestBehavior.AllowGet);
                ModelState.AddModelError("RoleName", "Stored Procedure Was Not Supported ");
            }

            try
                {
                   
                    if (Column != null)
                    {
                        string Id = Convert.ToString(TempData["ReportID"]);
                        List<string> value1 = objdb.SP_GetName(Id).ToList();
                        foreach (var s in value1)
                        {
                            DSRCManagementSystem.Models.CustomReports ob = new DSRCManagementSystem.Models.CustomReports();
                            ob.CustomName = s;
                            value.Add(ob);
                        }
                        ViewBag.Id = value;
                        string values = string.Empty;
                        List<string> objuser = new List<string>();
                        string[] value2 = Column.Split(',');
                        for (int k = 0; k < value2.Count(); k++)
                        {
                            if (value2[k] != "")
                            {
                                objuser.Add(value2[k].Replace(",", "''"));
                            }
                        }
                        var pose = (from pi in objdb.CustomReports
                                    select new
                                    {
                                        Id3 = pi.ReportQuery,
                                        Template = pi.ReportName
                                    }).ToList();
                        ViewBag.Purpose = new SelectList(pose, "Id3", "Template");
                        string constr1 = ConfigurationManager.AppSettings["connstr"];
                        DataSet ds1 = new DataSet();
                        SqlConnection objcon1 = new SqlConnection(constr1);
                        SqlCommand cmd1 = new SqlCommand(Id, objcon1);
                        TempData["ReportID"] = ds1;
                        int i = 0;
                        foreach (var s in value1)
                        {
                            cmd1.Parameters.Add(s, SqlDbType.VarChar).Value = value2[i];
                            i++;
                        }
                        cmd1.CommandText = Id; //  Stored procedure name
                        cmd1.CommandType = CommandType.StoredProcedure; // set it to stored proc           
                        SqlDataAdapter adap1 = new SqlDataAdapter(cmd1);
                        adap1.Fill(ds1);
                        TempData["ID"] = adap1;
                        List<object> chartData = new List<object>();
                        List<object> List = new List<object>();
                        List<object> valueList = new List<object>();
                        foreach (DataRow dr in ds1.Tables[0].Rows)
                        {
                            DSRCManagementSystem.Models.CustomReports ob = new DSRCManagementSystem.Models.CustomReports();
                            Array column = dr.ItemArray;
                            ob.ReportName1 = column;
                            List.Add(ob.ReportName1);
                            Report.Add(ob);
                        }
                        foreach (DataColumn dr in ds1.Tables[0].Columns)
                        {
                            DSRCManagementSystem.Models.CustomReports ob1 = new DSRCManagementSystem.Models.CustomReports();
                            ob1.CustomNameId = dr.ColumnName;
                            valueList.Add(ob1.CustomNameId);
                        }
                        ViewBag.List = Report.ToList();
                        TempData["Value"] = Report.ToList();
                        TempData["List"] = ViewBag.Val = valueList;
                        TempData["ReportIDs"] = TempData["cusId"];
                        ViewBag.Val = valueList;
                        ViewBag.ListValue = List;
                    }
                }
                catch (Exception Ex)
                {
                    
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
                var userid = Convert.ToInt32(Session["UserID"]);
                ViewBag.permission = (from p in objdb.ReportsPermissions
                                      where p.UserId == userid && p.IsAuthorized == true
                                      select p.UserId).SingleOrDefault();
                ViewBag.UserID = Convert.ToInt32(Session["UserID"]);
            
                return View(value);
        }
        [HttpGet]
        public ActionResult ManageRoll()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var rolldetails = (from u in db.CustomReports
                                    where u.IsActive == true
                                    select new ReportMOD()
                                    {
                                        ReportID = u.ReportID,
                                        Name = u.ReportName,
                                        Description = u.ReportDescription,
                                        sp = u.ReportQuery

                                    }).ToList();
                return View(rolldetails);



            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }

        }




        [HttpGet]
        public ActionResult EditRoll(int ReportID)
        {
            var ReportDetails = new ReportMOD();
            List<DSRCManagementSystem.Models.ReportMOD> objmodel = new List<Models.ReportMOD>();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var Role = objdb.CustomReports_UserMapping.Where(x => x.ReportID == ReportID).Select(o => o.RoleID).ToList();
                ViewBag.Role = Role;
                
                var categories = objdb.Master_Roles.Select(c => new
                {
                    CategoryID = c.RoleID,
                    CategoryName = c.RoleName
                }).ToList();
                ViewBag.Categories = new MultiSelectList(categories, "CategoryID", "CategoryName", Role);

                Session["ReportID"] = ReportID;

                //var RoleName=objdb.Master_Roles.Where(o=>o.RoleID==Role


                //var Role = (from u in objdb.CustomReports_UserMapping
                //            where u.ReportID == ReportID
                //            select new
                //            {
                //                u.RoleID
                //            }).FirstOrDefault();

           

            

                 ReportDetails = (from u in objdb.CustomReports
                                     where u.ReportID == ReportID
                                     select new ReportMOD
                                     {
                                         rollid=u.ReportID,
                                         Name=u.ReportName,
                                         Description=u.ReportDescription,
                                         sp=u.ReportQuery,
                                         

                                     }).FirstOrDefault();
                 
                
                



                string constr = ConfigurationManager.AppSettings["connstr"];
                DataTable dt = new DataTable();
                SqlConnection objcon = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("Sp_Names", objcon);
                cmd.CommandText = "Sp_Names";
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter adap = new SqlDataAdapter(cmd);
                adap.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DSRCManagementSystem.Models.ReportMOD obj = new DSRCManagementSystem.Models.ReportMOD();
                    obj.Name = dt.Rows[i]["SP"].ToString();
                    objmodel.Add(obj);
                }
                SelectList list = new SelectList(objmodel, "Name", "Name");
                list.OrderBy(a => a);
                ViewBag.Roles = list;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(ReportDetails);
        }
        [HttpPost]
        public ActionResult EditRoll(ReportMOD model)
        {
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                var Name = model.Name.Trim();
                int Report = int.Parse(Session["ReportID"].ToString());

                

                var data = objdb.CustomReports.Where(x => x.IsActive == true && x.ReportID != Report).Select(o => o.ReportName);

                foreach (var check in data)
                {

                    if (check.ToLower() == Name.ToLower())
                    {

                        return Json("Warning", JsonRequestBehavior.AllowGet);
                    }

                }
                var checkname = objdb.CustomReports.Where(x => x.ReportName == Name && x.IsActive == true && x.ReportID !=Report).Select(o => o.ReportID).FirstOrDefault();
                if (checkname != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
                
                var Desc = "";
                if (model.Description != null)
                {
                    Desc = model.Description.Trim();
                }
                
               

             var Role = objdb.CustomReports_UserMapping.Where(x => x.ReportID == Report).Select(o => o.RoleID).ToList();



                var sp = model.sp;
                var ReportID = (int)Session["UserId"];
                var ReturnDate = System.DateTime.Now;
                var roles = model.roles.Split(',');
                var Parameter = model.Parameter;
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var Editroll = db.CustomReports.Where(q => q.ReportID == Report).Select(r => r).FirstOrDefault();
                Editroll.ReportID = Report;
               
                Editroll.ReportName = model.Name;
                Editroll.ReportDescription = model.Description;
                Editroll.ReportQuery = model.sp;
                Editroll.CreatedBy = Convert.ToString(Report);
                Editroll.IsActive = true;
                db.SaveChanges();

                
                var roles1 = model.roles.Split(',');
                foreach (string userID in roles1)
                {
                    //var Function = objdb.CustomReports_UserMapping.CreateObject();
                    var Function = db.CustomReports_UserMapping.Where(q => q.ReportID == Report).Select(r => r).FirstOrDefault();
                    Function.ReportID = Report;
                    Function.RoleID = Convert.ToInt32(userID);
                    
                    db.SaveChanges();
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
        public ActionResult Delete(int ReportID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var Deleteroll = db.CustomReports.Where(o => o.ReportID == ReportID).Select(o => o).FirstOrDefault();
            Deleteroll.IsActive = false;
            db.SaveChanges();

            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Report()
        {
          try{
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var categories = objdb.Master_Roles.Select(c => new
            {
                CategoryID = c.RoleID,
                CategoryName = c.RoleName
            }).ToList();
            ViewBag.Categories = new MultiSelectList(categories, "CategoryID", "CategoryName");
            List<DSRCManagementSystem.Models.ReportMOD> objmodel = new List<Models.ReportMOD>();
            string constr = ConfigurationManager.AppSettings["connstr"];
            DataTable dt = new DataTable();
            SqlConnection objcon = new SqlConnection(constr);
            SqlCommand cmd = new SqlCommand("Sp_Names", objcon);
            cmd.CommandText = "Sp_Names";
            cmd.CommandType = CommandType.StoredProcedure;        
            SqlDataAdapter adap = new SqlDataAdapter(cmd);
            adap.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DSRCManagementSystem.Models.ReportMOD obj = new DSRCManagementSystem.Models.ReportMOD();
                obj.Name = dt.Rows[i]["SP"].ToString();
                objmodel.Add(obj);
            }
            SelectList list = new SelectList(objmodel, "Name", "Name");
            list.OrderBy(a => a);
            ViewBag.Roles = list;
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
        public ActionResult Report(ReportMOD model)
        {
          try{
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            var Name = model.Name.Trim();
            var checkname = objdb.CustomReports.Where(x => x.ReportName == Name && x.IsActive == true).Select(o => o.ReportID).FirstOrDefault();
            if (checkname != 0)
            {
                return Json("Warning", JsonRequestBehavior.AllowGet);
            }
            var Desc = "";
            if (model.Description != null)
            {
                Desc = model.Description.Trim();
            }

          
            var sp = model.sp;
            var userId = (int)Session["UserId"];
            var ReturnDate = System.DateTime.Now;
            var roles = model.roles.Split(',');
            var Parameter = model.Parameter;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var Assignobj = objdb.CustomReports.CreateObject();
            Assignobj.ReportName = Name;
            Assignobj.ReportDescription = Desc;
            Assignobj.ReportQuery = sp;
            Assignobj.CreatedBy = Convert.ToString(userId);
            Assignobj.CreatedDate = ReturnDate;
            Assignobj.IsActive = true;
            objdb.CustomReports.AddObject(Assignobj);
            objdb.SaveChanges();
            model.ReportID = Assignobj.ReportID;
            var roles1 = model.roles.Split(',');
            foreach (string userID in roles1)
            {
                var Function = objdb.CustomReports_UserMapping.CreateObject();
                Function.ReportID = model.ReportID;
                Function.RoleID = Convert.ToInt32(userID);
                objdb.CustomReports_UserMapping.AddObject(Function);
                objdb.SaveChanges();
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
        public ActionResult Copy()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ReportsPermission()
        {
            try{
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var check = db.ReportsPermissions.Select(x => x.UserId).ToList();
           List<int> LIST=new List<int>();
            foreach (var X in check)
            {
                var list =
                    db.Users.Where(x => x.UserID == X && x.IsActive == true && x.UserStatus != 6)
                        .Select(o => o.UserID)
                        .FirstOrDefault();
                
                
                if (list == 0)
                {
                    var data = db.ReportsPermissions.Where(o => o.UserId == X).Select(x => x).ToList();
                    foreach (var y in data)
                    {
                        y.IsAuthorized = false;
                        db.SaveChanges();

                    }
                    //LIST.Add(list);
                }
            }
          

                var AuthUsers = (from ep in db.ReportsPermissions.Where(ep => ep.IsAuthorized == true)
                    join u in db.Users.Where(u => u.IsActive == true && u.UserStatus != 6) on ep.UserId equals u.UserID
                        into evper
                    from eventper in evper.DefaultIfEmpty()
                    select new
                    {
                        userid = eventper.UserID,
                        username = eventper.FirstName + " " + eventper.LastName
                    }).ToList();

                ViewBag.AuthorizedUsers = new SelectList(AuthUsers, "userid", "username");
         

            var FilteredUsers =
                    db.Users.Where(
                        u => u.IsActive == true && u.FirstName != null && u.LastName != null && u.UserStatus != 6)
                        .Select(x => x.UserID)
                        .ToList()
                        .
                        Except(
                            db.ReportsPermissions.Where(ep => ep.IsAuthorized == true || ep.IsAuthorized.Value)
                                .Select(x => x.UserId.Value)
                                .ToList()).ToList();
                List<object> UnAuthUsers = new List<object>();
                foreach (int users in FilteredUsers)
                {
                    UnAuthUsers.AddRange(
                        db.Users.Where(u => u.UserID == users)
                            .Select(u => new {userid = u.UserID, username = u.FirstName + " " + u.LastName})
                            .ToList());
                }
                ViewBag.UnAuthorizedUsers = new SelectList(UnAuthUsers, "userid", "username");
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
        public ActionResult ReportsPermission(List<int> From, List<int> To)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var deleteuser = db.ReportsPermissions.Where(x => x.IsAuthorized == true).Select(o => o).ToList();
                foreach (var deluser in deleteuser)
                    db.ReportsPermissions.DeleteObject(deluser);
                db.SaveChanges();
                for (int j = 0; j < To.Count(); j++)
                {
                    DSRCManagementSystem.ReportsPermission objaccess = new DSRCManagementSystem.ReportsPermission();
                    objaccess.UserId = To[j];
                    objaccess.IsAuthorized = true;
                    db.AddToReportsPermissions(objaccess);
                    db.SaveChanges();
                }
                return Json("Authorize", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }

        }


        

    }
}

