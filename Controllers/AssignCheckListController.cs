using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.DSRCLogic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Text.RegularExpressions;
using DSRCManagementSystem;
using System.Web.Script.Serialization;
using System.Threading;
using System.Data;
using System.Data.Common.CommandTrees;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Web.Helpers;
using System.Web.Routing;
using DSRCManagementSystem.Models.Domain_Models;
using System.Configuration;
using NPOI.SS.Formula.Functions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Net.Mail;
using Utilities;


namespace DSRCManagementSystem.Controllers
{
    public class AssignCheckListController : Controller
    {
        private IEqualityComparer<AssignCheckList> CheckListID;
        
        [HttpGet]
        public ActionResult AsssignCheckList(int projectid, string v1)
        {
            
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ProjectID = projectid.ToString();
            Session["project"] = projectid;
            ViewBag.value = v1;
            var objmodel = new List<AssignCheckList>();
            var Gridlist = new List<AssignCheckList>();
            DSRCManagementSystem.Models.AssignCheckList Users = new DSRCManagementSystem.Models.AssignCheckList();
            try
            {
                Users.Projectname = db.Projects.Where(x => x.ProjectID == projectid).Select(o => o.ProjectName).FirstOrDefault();
         
                Users.catcheck = (from u in db.CheckLists
                                  join v in db.CheckListMappings.Where(x=>x.ProjectID==projectid) on u.CheckListID equals v.CheckListID into x
                                  from y2 in x.DefaultIfEmpty()
                                  where u.IsActive==true
                                  select new AssignCheckList()
                                  {
                                      CategoryID = u.CategoryID,
                                      CheckListID= u.CheckListID,
                                      CheckListItems= u.CheckListName,
                                      IsChecked= y2.IsChecked,
                                      project= y2.ProjectID,
                                      Checklistmapping = y2.CheckListMapping_ID
                                  }).ToList();



                Users.Gridlist = (
                             from ch in db.ProjectCategoryMappings
                             join clm in db.Categories  on ch.CategoryID equals clm.CategoryID
                             where ch.IsActive == true && clm.IsActive== true && ch.ProjectID==projectid
                             select new AssignCheckList()
                             {
                                 CategoryID = clm.CategoryID,
                                 CategoryName = clm.CategoryName,
                                 project = ch.ProjectID,
                                 ProjectId = ch.ProjectCategoryMapping_ID,
                             }).GroupBy(p => p.CategoryID).Select(g => g.FirstOrDefault()).ToList();


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }

            return View(Users);
        }

        [HttpGet]
        public ActionResult Assign()
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            int pID = Convert.ToInt32(Session["project"]);
            var Assign = (from u in db.ProjectCategoryMappings
                          where u.IsActive == true
                          select new AssignCheckList()
                          {
                              CategoryID = u.CategoryID,
                          }).ToList();

            var Categories = (from u in db.Categories.Where ( x=> x.IsActive==true)
                            select new 
                          {
                              CategoryID = u.CategoryID,
                              CategoryName = u.CategoryName
                          }).ToList();


            var CategoriesList = db.ProjectCategoryMappings.Where(x => x.ProjectID == pID && x.IsActive== true).Select(o => o.CategoryID).ToList();


            ViewBag.Cat = new MultiSelectList(Categories, "CategoryID", "CategoryName", CategoriesList);



            return View();
        }

        [HttpPost]
        public ActionResult Assign(Assign list)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int pID = Convert.ToInt32(Session["project"]);


          
            
            string Cname = list.CategoryName;
            var GName = Cname.Split(',');
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var GP = "";
            List<string> List = new List<string>();
            List<string> USERS = new List<string>();
            List<string> IDS = new List<string>();


            foreach (var x in GName)
            {
                GP = rgx.Replace(x, "");
                List.Add(GP);
            }


            foreach (var GRPID in List)
            {
                if (GRPID != Convert.ToString(0))
                {
                    int GID = Convert.ToInt32(GRPID);
                    var del = db.ProjectCategoryMappings.Where(o => o.ProjectID == pID).Select(p => p).ToList();

                    foreach (var delete in del)
                    {
                        db.ProjectCategoryMappings.DeleteObject(delete);
                        db.SaveChanges();
                    }

                    var Users = (from u in db.Categories
                                 where u.IsActive == true && u.CategoryID==GID
                                 select new Assign
                                 {
                                     CategoryName = u.CategoryName,
                                 }).ToList();   
                    
                        IDS.Add(Convert.ToString(GID));
                    
                    foreach (var x in Users)
                    {
                        USERS.Add(Convert.ToString(x.CategoryName));
                    }


                }
            }
          
            foreach (var cname in IDS)
            {
                int CId = Convert.ToInt32(cname);
                {
                    var Insert = db.ProjectCategoryMappings.CreateObject();
                    Insert.ProjectID = pID;
                    Insert.CategoryID = Convert.ToInt32(cname);
                    Insert.IsActive = true;
                    db.ProjectCategoryMappings.AddObject(Insert);
                    db.SaveChanges();
                }
            }



            var Categories = (from u in db.Categories.Where(x => x.IsActive == true)
                              select new
                              {
                                  CategoryID = u.CategoryID,
                                  CategoryName = u.CategoryName
                              }).ToList();


            var CategoriesList = db.ProjectCategoryMappings.Where(x => x.ProjectID == pID && x.IsActive == true).Select(o => o.CategoryID).ToList();


            ViewBag.Cat = new MultiSelectList(Categories, "CategoryID", "CategoryName", CategoriesList);
            

            return View();
        }
        [HttpPost]
        public ActionResult SaveCheck(List<string> List, int pid, List<string> UnList)
        {
//Check
            var json_serializer = new JavaScriptSerializer();
            Listing memberObj = json_serializer.Deserialize<Listing>(List[0]);
            List<AssignCheckList1> newMembers = new List<AssignCheckList1>(memberObj.Check);
//Uncheck
            var Unjson_serializer = new JavaScriptSerializer();
            Listing UnmemberObj = Unjson_serializer.Deserialize<Listing>(UnList[0]);
            List<AssignCheckList1> UnnewMembers = new List<AssignCheckList1>(UnmemberObj.UnCheck);


            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var del = db.CheckListMappings.Where(o => o.ProjectID == pid).Select(o => o).ToList();
            foreach (var delete in del)
            {

                db.CheckListMappings.DeleteObject(delete);
                db.SaveChanges();
            }

            foreach (var items in newMembers)
            {

                if(items !=null)
                {
                   var values = items.CheckID;
                    string values1 = Convert.ToString(values);
                    string split = values1.Trim(new Char[] { ' ', ',' });
                    string[] Ids = split.Split(',');

                   
                    for (int j = 0; j < Ids.Count(); j++)
                    {                 
                        int ChecklistId = Convert.ToInt32(Ids[j]);
                        int Categoryid = Convert.ToInt32(items.GridID);
                        var Insert = db.CheckListMappings.CreateObject();
                            Insert.CategoryID = Categoryid;
                            Insert.CheckListID = ChecklistId;
                            Insert.ProjectID = pid;
                            Insert.IsChecked = true;
                            db.CheckListMappings.AddObject(Insert);
                            db.SaveChanges();

                        }
                    }
                    }
                  
                
            foreach (var item in UnnewMembers)
            {
                if (item != null)
                {
                    var values22 = item.UnCheckID;
                    string values122 = Convert.ToString(values22);
                    string split22 = values122.Trim(new Char[] { ' ', ',' });
                    string[] Ids22 = split22.Split(',');


                    for (int j = 0; j < Ids22.Count(); j++)
                    {
                        int ChecklistId = Convert.ToInt32(Ids22[j]);
                        int Categoryid = Convert.ToInt32(item.UnGridID);
                        var Insert = db.CheckListMappings.CreateObject();
                       
                            Insert.CategoryID = Categoryid;
                            Insert.CheckListID = ChecklistId;
                            Insert.ProjectID = pid;
                            Insert.IsChecked = false;
                            db.CheckListMappings.AddObject(Insert);
                            db.SaveChanges();
                        }
                    }
                }

            
            
            
            return Json("success", JsonRequestBehavior.AllowGet);

        }


        public IEqualityComparer<AssignCheckList> CheckListName { get; set; }
    }
}
