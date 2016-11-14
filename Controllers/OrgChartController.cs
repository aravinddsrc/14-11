using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Web.Script.Serialization;


namespace DSRCManagementSystem.Controllers
{

    //[DSRCAuthorize(Roles = "Vice President, Project Manager, HR,Business Development Manager,Head - Quality,Vice President - Marketing,Coo/Executive Vice President,Manager - Engineer")]
    
    public class OrgChartController : Controller
    {
        //
        // GET: /OrgChart/

        public ActionResult Index()
        {
            return View();
        }

        public class ChartStructure
        {
            public int? RecordID { get; set; }
            public int? UserId { get; set; }
            public string UserName { get; set; }
            public int DepartmentID { get; set; }
            public string DepartmentName { get; set; }
            public int? PrecendenceOrder { get; set; }

        }

        //public ActionResult OrgChart()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    DSRCManagementSystem.Models.Users obj_User = new DSRCManagementSystem.Models.Users();
        //    //var OrgChart1 = from r in db.Roles
        //    //                join rh in db.RoleHierarchies on r.RoleID equals rh.RoleID
        //    //                join ur in db.UserRoles on r.RoleID equals ur.RoleID                            
        //    //var OrgChart = (from u in db.Users
        //    //                join ur in db.UserReportings on u.UserID equals ur.UserID
        //    //                select ur).GroupBy(i => i.UserID);

        //    var result = (from r in db.Roles
        //                  from rh in db.RoleHierarchies
        //                  from ur in db.UserRoles
        //                  from u in db.Users
        //                  join ure in db.UserReportings on u.UserID equals ure.UserID into ps
        //                  from ure in ps.DefaultIfEmpty()
        //                  from d in db.Departments
        //                  where r.RoleID == rh.RoleID && r.RoleID == ur.RoleID && ur.UserID == u.UserID && u.DepartmentId == d.DepartmentId
        //                  select new
        //                  {
        //                      parent = rh.Parent,
        //                      roleid = r.RoleID,
        //                      rolename = r.RoleName,
        //                      userid = u.UserID,
        //                      username = u.FirstName,
        //                      departmentname = d.DepartmentName,
        //                      reportinguserid = ure.ReportingUserID
        //                  }).OrderBy(i => i.parent);//.GroupBy(i => i.reportinguserid);
                
        //        //from u in db.Users
        //        //         from d in db.Departments
        //        //         where u.DepartmentId == d.DepartmentId
        //        //         select new {
        //        //             userid = u.UserID,
        //        //             username = u.FirstName,
        //        //             departmentid = u.DepartmentId,
        //        //             departmentname = d.DepartmentName
        //        //        };

        //    var resultList = new List<Org>();
        //    var urList = db.Users.ToList();
        //    foreach (var item in result)
        //    {
        //        //var current = item.First().ReportingUserID;
        //        resultList.Add(new Org
        //        {
        //            Name = item.,
        //            Format = "",
        //            ReportingName = urList.FirstOrDefault(i => i.UserID == current).FirstName
        //        });
        //    }
        //    return View(resultList);
        //}

        //public ActionResult OrgChart()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    //var result = (from r in db.Roles
        //    //              from rh in db.RoleHierarchies
        //    //              from ur in db.UserRoles
        //    //              from u in db.Users
        //    //              join ure in db.UserReportings on u.UserID equals ure.UserID into ps
        //    //              from ure in ps.DefaultIfEmpty()
        //    //              from d in db.Departments
        //    //              where r.RoleID == rh.RoleID && r.RoleID == ur.RoleID && ur.UserID == u.UserID && u.DepartmentId == d.DepartmentId
        //    //              select new
        //    //              {
        //    //                  parent = rh.Parent,
        //    //                  roleid = r.RoleID,
        //    //                  rolename = r.RoleName,
        //    //                  userid = u.UserID,
        //    //                  username = u.FirstName,
        //    //                  departmentname = d.DepartmentName,
        //    //                  reportinguserid = ure.ReportingUserID
        //    //              }).OrderBy(i => i.parent);//.GroupBy(i => i.reportinguserid);

        //    //List<OrgChart> resultSet = new List<OrgChart>();

        //    //foreach (var item in result)
        //    //{
        //    //    resultSet.Add(new OrgChart()
        //    //    {
        //    //        UserID = item.userid,
        //    //        Name = item.username,
        //    //        ReportingUserId = item.reportinguserid ?? null,
        //    //        Tooltip = item.rolename
        //    //    });
        //    //}

        //    //var result = from oc in db.OrgCharts
        //    //             join d in db.Departments on oc.DepartmentID equals d.DepartmentId
        //    //             select new
        //    //             {
        //    //                UserId = oc.UserID,
        //    //                DepartmentID = d.DepartmentId,
        //    //                DepartmentName = d.DepartmentName,
        //    //                PrecendenceOrder = oc.PrecedenceOrder
        //    //             };
        //    //List<OrgChart> resultset = new List<OrgChart>();

        //    //foreach (var item in result)
        //    //{
        //    //    resultset.Add(new OrgChart()
        //    //    {
        //    //        UserID = item.UserId,
        //    //        DepartmentID = item.DepartmentID,
        //    //        //DepartmentName = item.DepartmentName,
        //    //        PrecedenceOrder  = item.PrecendenceOrder

        //    //    });                    
        //    //}
        //    return View(result);
        //}


        //public ActionResult ChartData()
        //{
        //    DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        //    var result = (from r in db.Roles
        //                  from rh in db.RoleHierarchies
        //                  from ur in db.UserRoles
        //                  from u in db.Users
        //                  join ure in db.UserReportings on u.UserID equals ure.UserID into ps
        //                  from ure in ps.DefaultIfEmpty()
        //                  from d in db.Departments
        //                  where r.RoleID == rh.RoleID && r.RoleID == ur.RoleID && ur.UserID == u.UserID && u.DepartmentId == d.DepartmentId
        //                  select new
        //                  {
        //                      parent = rh.Parent,
        //                      roleid = r.RoleID,
        //                      rolename = r.RoleName,
        //                      userid = u.UserID,
        //                      username = u.FirstName,
        //                      departmentname = d.DepartmentName,
        //                      reportinguserid = ure.ReportingUserID
        //                  }).OrderBy(i => i.parent);//.GroupBy(i => i.reportinguserid);

        //    List<OrgChart> resultSet = new List<OrgChart>();

        //    foreach (var item in result)
        //    {
        //        resultSet.Add(new OrgChart()
        //        {
        //            UserID = item.userid,
        //            Name = item.username,
        //            ReportingUserId = (int)item.reportinguserid,
        //            Tooltip = item.rolename
        //        });
        //    }

        //    return Json(resultSet, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Node()
        {
            List<Node> Data = new List<Node>();
            try
            {
                Data.Add(new Node() { NodeId = "0", Name = "Rohan", ParentNodeId = "", Tooltip = "MD" });
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                //var resultList = (from d in db.Departments
                //                  join oc in db.OrgCharts on d.DepartmentId equals oc.DepartmentID into DeptName
                //                  from em in DeptName.DefaultIfEmpty()
                //                  join u in db.Users on em.UserID eq uals u.UserID into Resu
                //                  from final in resultList

                var resultList = (from d in db.Departments
                                  join oc in db.OrgCharts on d.DepartmentId equals oc.DepartmentID into DeptName
                                  from de in DeptName.DefaultIfEmpty()
                                  join u in db.Users on de.UserID equals u.UserID into res
                                  from final in res.DefaultIfEmpty()
                                  select new ChartStructure
                                {
                                    RecordID = de.ID,
                                    UserId = de.UserID,
                                    UserName = final.FirstName,
                                    DepartmentID = d.DepartmentId,
                                    DepartmentName = d.DepartmentName,
                                    PrecendenceOrder = de.PrecedenceOrder
                                }).GroupBy(i => i.DepartmentID);

                //var joins = (from d in db.Departments
                //             join oc in db.OrgCharts on d.DepartmentId equals oc.DepartmentID into DeptName
                //             from de in DeptName.DefaultIfEmpty()
                //             join u in db.Users on de.UserID equals u.UserID into res
                //             from final in res.DefaultIfEmpty()
                //             select new Temp
                //             {
                //                 RecordID = de.ID,
                //                 UserId = final.UserID,
                //                 DepartmentID = d.DepartmentId,
                //                 DepartmentName = d.DepartmentName,
                //                 PrecendenceOrder = de.PrecedenceOrder
                //             }).GroupBy(i => i.DepartmentID);



                foreach (var item in resultList)
                {
                    var deptId = item.Key;
                    Data.Add(new Node() { NodeId = deptId.ToString(), Name = item.FirstOrDefault().DepartmentName, ParentNodeId = "0", Tooltip = "Department" });
                    var childParentId = deptId;
                    var childNodeId = deptId * 10;

                    foreach (var userNode in item.OrderBy(i => i.PrecendenceOrder))
                    {
                        if (userNode.RecordID != null && userNode.UserId != null && userNode.UserName != null && userNode.PrecendenceOrder != null)
                        {
                            Data.Add(new Node() { NodeId = childNodeId.ToString(), Name = userNode.UserName, ParentNodeId = childParentId.ToString(), Tooltip = "Employee" });
                            childParentId = childNodeId;
                            childNodeId++;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(Data);
        }

        public ActionResult CompanyArchitecture()
        {
            return View();
        }
        public ActionResult CompanyArchitectureData()
        {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                List<Organizechart> model = new List<Organizechart>();                
                using (DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1())
                {
                    model = dbhrms.Master_CompanyArchitecture.Select(o => new Organizechart() { id = o.NodeId, name = o.Name, parent = o.Parent }).ToList();
                }
               
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CompanyArchitectureData(string jsondata)
        {
            try
            {
                var json_serializer = new JavaScriptSerializer();
                List<Organizechart> listdata = json_serializer.Deserialize<List<Organizechart>>(jsondata);
                //DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                //List<Organizechart> model = new List<Organizechart>();
                //model.Add(new Organizechart { id = 1, name = " DSRC ", parent = 0 });
                //int count = 2;
                //foreach (var item in db.Departments.ToList())
                //{
                //    model.Add(new Organizechart { id = count++, name = item.DepartmentName, parent = 1 });


                //}


                // return Json(model, JsonRequestBehavior.AllowGet);
                using (DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1())
                {
                    dbhrms.ExecuteStoreCommand("truncate table CompanyArchitecture");

                    foreach (var item in listdata)
                    {
                        var obj = dbhrms.Master_CompanyArchitecture.CreateObject();
                        obj.NodeId = item.id;
                        obj.Name = item.name;
                        obj.Parent = item.parent;
                        dbhrms.Master_CompanyArchitecture.AddObject(obj);
                        dbhrms.SaveChanges();

                    }

                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
} 
    
