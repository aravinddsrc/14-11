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

namespace DSRCManagementSystem.Controllers
{
    public class OrganisationChartController : Controller
    {
        //
        // GET: /OrganisationChart/      
        [HttpGet]
        public ActionResult OrganisationChart()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ChangeUser(string UserId)
        {

            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int ID;
            if (UserId == "")
            {
                ID = 0;
            }

            else
            {
                ID = Convert.ToInt32(UserId);
            }

         
            return Json(ID, JsonRequestBehavior.AllowGet);



        }


      

        [HttpGet]
        public ActionResult GetChartData(string UserId)
        {
          List<DSRCManagementSystem.Models.Organisationchart> objmodel = new  List <DSRCManagementSystem.Models.Organisationchart>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                // DataTable dt = new DataTable();
                int ID = Convert.ToInt32(UserId);
                int Userid = (int)Session["UserId"];
                string constr = ConfigurationManager.AppSettings["connstr"];
                DataSet ds = new DataSet();
                SqlConnection objcon = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("SP_GetOrganizationChart2", objcon);
                if (ID != 0)
                {
                    cmd.Parameters.Add("@ReportingID", SqlDbType.Int).Value = ID;
                }
                else if (ID == 0 && UserId != "0")
                {
                    cmd.Parameters.Add("@ReportingID", SqlDbType.Int).Value = Userid;
                }
                else  if(ID ==0 && UserId == "0")
                {
                    cmd.Parameters.Add("@ReportingID", SqlDbType.Int).Value = 0;
                }
                cmd.CommandText = "SP_GetOrganizationChart2"; //  Stored procedure name
                cmd.CommandType = CommandType.StoredProcedure; // set it to stored proc           
                SqlDataAdapter adap = new SqlDataAdapter(cmd);
                adap.Fill(ds);
                List<object> chartData = new List<object>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    bool ImgExists = System.IO.File.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + dr["UserId"].ToString()+".jpg")));
                    chartData.Add(new object[]
                    {
                          dr["UserId"], dr["UserName"], dr ["DesignationName"], dr["ReportingUserID"] , dr["ReporterName"], ImgExists
                    });
                   // var tes = dr["UserId"];
                    //var ID = db.UserProfiles.Where(x => x.UserID == tes).Select(x => x.Photo);
                }
                var photos = db.UserProfiles.Select(o => o).ToList();
                 objmodel =(from  DataRow dr in ds.Tables[0].Rows
                            select new DSRCManagementSystem.Models.Organisationchart
                            {
                                UserId=dr["UserId"].ToString(),
                                UserName=dr["UserName"].ToString(),
                                ReportingUserID=dr["ReportingUserID"].ToString(),
                                ReporterName=dr["ReporterName"].ToString()
                               
                            }).ToList();

                 
               
                 using (DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1())
                 {
                     ViewBag.ReportingPersons = new  SelectList (GetReportingPersons(), "UserId", "Name",Userid);
                  
                 }

                 ViewBag.Week = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "All", Value = 1 }, new { Text = "PrasanthKrishnan", Value = 2 } }, "Value", "Text", 0);






                ViewBag.orgChart = chartData;


                foreach (var item in objmodel)
                {
                    int id = Convert.ToInt32(item.UserId);
                    DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                    var image = from temp in objdb.UserProfiles where temp.UserID == id select temp.Photo;
                   // byte[] cover = image.FirstOrDefault();
                    item.cover = image.FirstOrDefault();
                 //   item.Photo = objdb.UserProfiles.Where(x => x.UserID == id).Select(o => o.Photo).FirstOrDefault();

                }

                


             
                return View(objmodel);
            }
        }

        private List<ReportingPerson> GetReportingPersons()
        {
            int Userid = (int)Session["UserId"];
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == Userid).BranchId;
                List<ReportingPerson> reportingPersons = (from r in db.Master_Roles
                                                          //where r.RoleID == 4 || r.RoleID == 8 || r.RoleID == 44 || r.RoleID == 42
                                                          //|| r.RoleID == 40 || r.RoleID == 47 || r.RoleID == 60 || r.RoleID == 67
                                                          //|| r.RoleID == 59 || r.RoleID == 26 || r.RoleID == 30 || r.RoleID == 70 || r.RoleID == 62
                                                          join ur in db.UserRoles on r.RoleID equals ur.RoleID
                                                          join u in db.Users on ur.UserID equals u.UserID
                                                          where u.IsActive == true && u.BranchId == BranchId
                                                          //where u.IsActive == true && u.UserID != 282 && u.BranchId == BranchId

                                                          select new ReportingPerson
                                                          {
                                                              UserID = u.UserID,
                                                              Name = (u.FirstName + " " + (u.LastName ?? "")).Trim()
                                                          }).OrderBy(o => o.UserID).ToList();
                return reportingPersons;
            }
        }


        [HttpGet]

        public ActionResult Chart()
        {
            return View();
        }

        //    [HttpGet]

        //    public ActionResult GetChartData()
        //    {
        //        string query = "select a.UserID,b.FirstName as UserName,a.ReportingUserID,c.FirstName as ReporterName from UserReporting a join Users b on .UserID=b.UserID join Users c on c.UserID=a.ReportingUserID";


        //        string constr = ConfigurationManager.AppSettings["connstr"];
        //        using (SqlConnection con = new SqlConnection(constr))
        //        {

        //            using (SqlCommand cmd = new SqlCommand(query))
        //            {

        //                List<object> chartData = new List<object>();

        //                cmd.CommandType = CommandType.Text;

        //                cmd.Connection = con;

        //                con.Open();

        //                using (SqlDataReader sdr = cmd.ExecuteReader())
        //                {

        //                    while (sdr.Read())
        //                    {

        //                        chartData.Add(new object[]

        //                {

        //                     sdr["UserID"], sdr["UserName"], sdr["ReportingUserID"] , sdr["ReporterName"]

        //                });

        //                    }

        //                }
        //                ViewBag.orgChart = chartData;
        //                con.Close();



        //            }

        //        }

        //        return View();


        //    }
        //}
    }
}
