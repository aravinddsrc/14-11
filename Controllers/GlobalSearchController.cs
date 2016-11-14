using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Controllers;
using DSRCManagementSystem.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace DSRCManagementSystem.Controllers
{
    public class GlobalSearchController : Controller
    {
        //
        // GET: /GlobalSearch/
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult SearchResults(GlobalSearch model)
        {
            try
            {
                if (model.SearchString != null)
                {
                   
                    char[] charsToTrim = { ' ', '\t' };
                    model.SearchString = model.SearchString.Trim(charsToTrim);

                }


                if (model.SearchString == null)
                {
                    model.SearchString = "";

                }
                int userId = int.Parse(Session["UserID"].ToString());



                if (model.SearchString != null)
                {
                    DataSet ds = new DataSet();
                    string constr = ConfigurationManager.AppSettings["connstr"];
                    SqlConnection con = new SqlConnection(constr);
                    SqlCommand cmd = new SqlCommand();
                    SqlParameter[] param = new SqlParameter[1];
                    cmd.Connection = con;
                    cmd.CommandText = "SP_GlobalSearch";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@srchString", SqlDbType.VarChar).Value = model.SearchString;
                    cmd.Parameters.Add("@UserID", SqlDbType.VarChar).Value = userId;
                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    adap.Fill(ds);
                    var searchlist = ds.Tables[0].AsEnumerable().Select(actRow =>
                                        new SearchListItemModel
                                        {
                                            UserId = (int)actRow["UserId"],
                                            FirstName = actRow["FirstName"] + " " + actRow["LastName"],
                                            Role = actRow["RoleName"].ToString() == null ? string.Empty : actRow["RoleName"].ToString(),
                                            DesignationName = actRow["DesignationName"].ToString() == null ? string.Empty : actRow["DesignationName"].ToString(),
                                            Skills = actRow["Skills"].ToString() == null ? string.Empty : actRow["Skills"].ToString(),
                                            ProjectId = actRow["ProjectId"].ToString() == null ? string.Empty : actRow["ProjectId"].ToString(),
                                            ProjectName = actRow["ProjectName"].ToString() == null ? string.Empty : actRow["ProjectName"].ToString()
                                        }).ToList();
                    ViewBag.SearchString = model.SearchString;
                    return View(searchlist);
                }
              
                else
                {
                    return View();
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex.GetBaseException();
            }
        }

        [HttpGet]
        public ActionResult SearchedProjectsDetails(int? UserId, string ProjectId)
        {
            List<Projects> showProjectDetails = new List<Projects>();
            try{
            if (ProjectId == "")
            {
                return View();
            }
            List<int> projectList=new List<int>();
            string[] singleProjectId=ProjectId.Split(',');
            for (int i = 0; i < singleProjectId.Count(); i++)
            {
                projectList.Add(Convert.ToInt32(singleProjectId[i]));
            }
                
                showProjectDetails = (from p in db.Projects.Where(x => projectList.Contains(x.ProjectID))
                                      join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                      select new DSRCManagementSystem.Models.Projects
                                      {
                                          ProjectCode = p.ProjectCode,
                                          ProjectName = p.ProjectName,
                                          ProjectType = pt.ProjectTypeName,
                                          RAGStatus = p.RAGStatus,
                                      }).OrderBy(x => x.RAGStatus).ToList();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View(showProjectDetails);
        }
    }
}
