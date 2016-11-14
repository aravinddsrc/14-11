using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;
using System.Data.Objects.SqlClient;
using System.Web.UI;

namespace DSRCManagementSystem.Controllers
{

    // [DSRCAuthorize(Roles = "Vice President, Project Manager, Technical Lead")]
    public class ProjectsController : Controller
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();
        //
        // GET: /Projects/
        [HttpGet]
        public ActionResult ViewProjects(string ID)
        {

            var isReporting = (bool)Session["IsRerportingPerson"];
            ViewBag.InActive = false;
            ModelState.Clear();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
            List<Projects> projectData = new List<Projects>();
            int? membertypeid = db.UserProjects.Where(x => x.UserID == userId).Select(o => o.MemberTypeID).FirstOrDefault();
            ////////////////////////////

            if (ID == null)
            {
                ID = "false";
            }

            int roleId = int.Parse(Session["RoleID"].ToString());
            DateTime myDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            //var toggle = (from u in db.ViewallProjectPermissions
            //              where u.UserId == userId
            //              select new
            //              {
            //                  u.UserId
            //              }).FirstOrDefault();

            var toggles = db.ViewallProjectPermissions.Where(o => o.UserId == userId).FirstOrDefault();

            if (toggles != null)
            {
                ViewBag.toggles = toggles;
            }

            if (ID == "false")
            {
                ViewBag.value = "false";



                if (isReporting)
                {
                    projectData = (from p in db.Projects
                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID 
                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                   select new Projects()
                                   {
                                       ProjectID = p.ProjectID,
                                       ProjectName = p.ProjectName,
                                       ProjectCode = p.ProjectCode,
                                       ProjectType = pt.ProjectTypeName,
                                       RAGStatus = p.RAGStatus,
                                       RAGComments = p.RAGComments ?? "Comments not added",
                                       CommentsCreated = p.CommentsCreated,
                                      // MemberTypeID = up.MemberTypeID
                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                }
                else if (Convert.ToInt32(Session["RoleID"]) == 59)
                {
                    projectData = (from p in db.Projects
                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                   join up in db.UserProjects on p.ProjectID equals up.ProjectID
                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                   select new Projects()
                                   {
                                       ProjectID = p.ProjectID,
                                       ProjectName = p.ProjectName,
                                       ProjectCode = p.ProjectCode,
                                       ProjectType = pt.ProjectTypeName,
                                       RAGStatus = p.RAGStatus,
                                       RAGComments = p.RAGComments ?? "Comments not added",
                                       CommentsCreated = p.CommentsCreated,
                                       //MemberTypeID = up.MemberTypeID
                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                }

                else
                {
                    projectData = (from p in db.Projects
                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                   select new Projects()
                                   {
                                       ProjectID = p.ProjectID,
                                       ProjectName = p.ProjectName,
                                       ProjectCode = p.ProjectCode,
                                       ProjectType = pt.ProjectTypeName,
                                       RAGStatus = p.RAGStatus,
                                       RAGComments = p.RAGComments ?? "Comments not added",
                                       CommentsCreated = p.CommentsCreated,
                                     //  MemberTypeID = up.MemberTypeID
                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                }
            }



            else
            {
                ViewBag.value = "true";
                {

                    projectData = (from p in db.Projects
                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                   where p.IsActive == true && (p.IsDeleted == false || p.IsDeleted == null) 
                                   select new Projects()
                                   {
                                       ProjectID = p.ProjectID,
                                       ProjectName = p.ProjectName,
                                       ProjectCode = p.ProjectCode,
                                       ProjectType = pt.ProjectTypeName,
                                       RAGStatus = p.RAGStatus,
                                       RAGComments = p.RAGComments ?? "Comments not added",
                                       CommentsCreated = p.CommentsCreated,
                                      // MemberTypeID = up.MemberTypeID
                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();

                }
            }

            ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");

            if (ID == "false")
            {
                projectData = projectData.Distinct().ToList();

               

                int z = Convert.ToInt32(TempData["Isdelete"]);
                if (z == 1)
                {
                    foreach (var item in projectData)
                    {

                        item.Isdelete = true;
                    }
                }

                foreach (var item in projectData)
                {
                    item.loginmemberid = membertypeid;
                }
            }

            return View(projectData);
        }

        private List<SelectListItem> GetProjects()
        {
            try
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                var ProjectList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<DSRCProjects> projects = (from data in db.Projects
                                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on data.ProjectID equals up.ProjectID
                                                   where data.IsActive == true
                                                   select new DSRCProjects
                                                   {
                                                       ProjectId = data.ProjectID,
                                                       ProjectName = data.ProjectName
                                                   }).OrderBy(x => x.ProjectName).Distinct().ToList();
                    foreach (var item in projects)
                    {
                        ProjectList.Add(new SelectListItem { Text = item.ProjectName, Value = item.ProjectId.ToString() });
                    }
                }

                return ProjectList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }
        private List<SelectListItem> GetRoles()
        {
            try
            {
                var NameList = new List<SelectListItem>(new[] { new SelectListItem { Text = "---Select---", Value = "0" } });
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    /*List<Roles> Names = (from data in db.Roles
                                         where data.IsActive == true
                                         //&&
                                         //data.RoleID != 3 && data.RoleID != 11
                                         //&& data.RoleID != 12
                                         select new Roles
                                         {
                                             RoleId = data.RoleID,
                                             RoleName = data.RoleName,
                                         }).ToList();*/
                    //List<Master_MemberTypes> Names = (from data in db.Master_MemberTypes
                    //                                  select new Master_MemberTypes
                    //                           {
                    //                               MemberTypeID = data.MemberTypeID,
                    //                               MemberType = data.MemberType,
                    //                           }).ToList();

                    var Names = (from data in db.Master_MemberTypes
                                 select new
                                 {
                                     MemberTypeID = data.MemberTypeID,
                                     MemberType = data.MemberType,
                                 }).ToList();
                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.MemberType, Value = item.MemberTypeID.ToString() });
                    }
                }
                return NameList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        [HttpPost]
        public ActionResult ViewProjects(FormCollection form, TmpModel INP)//bool Switch, string ProjectTypeDL, string Inactive)
        {
            List<Projects> projectData = new List<Projects>();
            // INP.Switch = Request.Form["Switch"].ToString();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var toggles = db.ViewallProjectPermissions.Where(o => o.UserId == userId).FirstOrDefault();

            if (toggles != null)
            {
                ViewBag.toggles = toggles;
            }
            if (INP.Switch == null)
            {
                ViewBag.Value = "true";
            }
            else if (INP.Switch == "on")
            {
                ViewBag.Value = "false";
            }
            //if (form["ProjectTypeDL"] == null || form["ProjectTypeDL"] == "")
            //    ModelState.AddModelError("ProjectType", "Project Type is Not empty");
            if (ModelState.IsValid)
            {


                var isReporting = (bool)Session["IsRerportingPerson"];
                //  int userId = int.Parse(Session["UserID"].ToString());
                // DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                string projectID = (INP.ProjectTypeDL == "" || INP.ProjectTypeDL == null) ? "0" : INP.ProjectTypeDL.ToString();
                int projectTypeID = Convert.ToInt32(projectID);
                bool status = INP.Inactive.Contains("true");
                DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName", projectID);
                ViewBag.InActive = false;
                //List<Projects> datas = new List<Projects>();

                if (toggles == null)
                {
                    if (projectTypeID == 0)
                    {
                        if (Convert.ToInt32(Session["RoleID"]) == 59)
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           join up in db.UserProjects on p.ProjectID equals up.ProjectID
                                           where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated,
                                               //MemberTypeID = up.MemberTypeID
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                            foreach (var item in projectData)
                            {
                                if (item.ProjectID != null)
                                {
                                    var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                    if (values != null)
                                    {
                                        var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                        if (member != null)
                                        {
                                            item.MemberTypeID = member;
                                        }
                                        else
                                        {
                                            item.MemberTypeID = null;
                                        }
                                    }
                                }
                            }

                        }
                        else if (isReporting)
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                              // join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                                ViewBag.InActive = true;
                            }
                        }
                        else
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //  MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //  MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(Session["RoleID"]) == 59)
                        {

                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                           where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true)
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                               //DateCreated = p.DateCreated,
                                               //IsActive = p.IsActive,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated,
                                               // MemberTypeID = up.MemberTypeID
                                           }).OrderBy(x => x.RAGStatus).Distinct().Distinct().ToList();
                            foreach (var item in projectData)
                            {
                                if (item.ProjectID != null)
                                {
                                    var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                    if (values != null)
                                    {
                                        var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                        if (member != null)
                                        {
                                            item.MemberTypeID = member;
                                        }
                                        else
                                        {
                                            item.MemberTypeID = null;
                                        }
                                    }
                                }
                            }

                        }
                        else if (isReporting)
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                              
                                               where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   // MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID 
                                              
                                               where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //  MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (!status)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                
                                               where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false)
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //  MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

                else if (toggles != null)
                {

                    if (ViewBag.value == "true")
                    {
                        if (projectTypeID == 0)
                        {
                            if (INP.Inactive == "false")
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects on p.ProjectID equals up.ProjectID
                                               
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   // MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }

                            else if (INP.Inactive == "true")
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               //join up in db.UserProjects on p.ProjectID equals up.ProjectID                                              
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   // MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (INP.Inactive == "false")
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects on p.ProjectID equals up.ProjectID
                                               
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   // MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               //join up in db.UserProjects on p.ProjectID equals up.ProjectID                                               
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   // MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }


                        }

                    }
                    else
                    {
                        if (projectTypeID == 0)
                        {
                            if (Convert.ToInt32(Session["RoleID"]) == 59)
                            {
                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                               join up in db.UserProjects on p.ProjectID equals up.ProjectID
                                              
                                               where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else if (isReporting)
                            {
                                if (!status)
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID                                                  
                                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID 
                                                 
                                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();

                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                    ViewBag.InActive = true;
                                }
                            }
                            else
                            {
                                if (!status)
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                    join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                    
                                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       // MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                    join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                   
                                                   where (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //  MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(Session["RoleID"]) == 59)
                            {

                                projectData = (from p in db.Projects
                                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               
                                               where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                               select new Projects()
                                               {
                                                   ProjectID = p.ProjectID,
                                                   ProjectName = p.ProjectName,
                                                   ProjectCode = p.ProjectCode,
                                                   ProjectType = pt.ProjectTypeName,
                                                   //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                   //DateCreated = p.DateCreated,
                                                   //IsActive = p.IsActive,
                                                   RAGStatus = p.RAGStatus,
                                                   RAGComments = p.RAGComments ?? "Comments not added",
                                                   CommentsCreated = p.CommentsCreated,
                                                   //   MemberTypeID = up.MemberTypeID
                                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                foreach (var item in projectData)
                                {
                                    if (item.ProjectID != null)
                                    {
                                        var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                        if (values != null)
                                        {
                                            var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                            if (member != null)
                                            {
                                                item.MemberTypeID = member;
                                            }
                                            else
                                            {
                                                item.MemberTypeID = null;
                                            }
                                        }
                                    }
                                }

                            }
                            else if (isReporting)
                            {
                                if (!status)
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                               
                                                   where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                        //MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                  
                                                   where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false)
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //  MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                if (!status)
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                   
                                                   where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == true
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //  MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    projectData = (from p in db.Projects
                                                   join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                                   join up in db.UserProjects.Where(x => x.UserID == userId) on p.ProjectID equals up.ProjectID
                                                  
                                                   where pt.ProjectTypeID == projectTypeID && ((p.IsDeleted == false || p.IsDeleted == null) && p.IsActive == false)
                                                   select new Projects()
                                                   {
                                                       ProjectID = p.ProjectID,
                                                       ProjectName = p.ProjectName,
                                                       ProjectCode = p.ProjectCode,
                                                       ProjectType = pt.ProjectTypeName,
                                                       //SvnRepositoryUrl = p.SvnRepositoryUrl,
                                                       //DateCreated = p.DateCreated,
                                                       //IsActive = p.IsActive,
                                                       RAGStatus = p.RAGStatus,
                                                       RAGComments = p.RAGComments ?? "Comments not added",
                                                       CommentsCreated = p.CommentsCreated,
                                                       //  MemberTypeID = up.MemberTypeID
                                                   }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                                    foreach (var item in projectData)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var values = db.UserProjects.Where(x => x.ProjectID == item.ProjectID).Select(o => o).FirstOrDefault();
                                            if (values != null)
                                            {
                                                var member = db.UserProjects.Where(x => x.ProjectID == values.MemberTypeID).Select(o => o.MemberTypeID).FirstOrDefault();
                                                if (member != null)
                                                {
                                                    item.MemberTypeID = member;
                                                }
                                                else
                                                {
                                                    item.MemberTypeID = null;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }

                    }
                }






                //foreach (var item in datas)
                //{

                //    projectData.Add(new DSRCManagementSystem.Models.Projects { ProjectID = item.ProjectID, ProjectName = item.ProjectName, ProjectCode = item.ProjectCode, SvnRepositoryUrl = item.SvnRepositoryUrl, DateCreated = item.DateCreated, IsActive = Convert.ToBoolean(item.IsActive) });
                //}

                projectData = projectData.Distinct().ToList();

                return View(projectData);
            }


            else
            {
                DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");
                return View(projectData);
            }
        }


        //[HttpGet]
        public ActionResult ViewAllProjects()
        {
            var isReporting = (bool)Session["IsRerportingPerson"];

            ModelState.Clear();
            int userId = int.Parse(Session["UserID"].ToString());
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
            List<Projects> projectData = new List<Projects>();

            if (isReporting)
            {
                projectData = (from p in db.Projects
                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                               where p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true
                               select new Projects()
                               {
                                   ProjectID = p.ProjectID,
                                   ProjectName = p.ProjectName,
                                   ProjectCode = p.ProjectCode,
                                   ProjectType = pt.ProjectTypeName,
                                   RAGStatus = p.RAGStatus,
                                   RAGComments = p.RAGComments ?? "Comments not added",
                                   CommentsCreated = p.CommentsCreated
                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
            }
            else
            {
                projectData = (from p in db.Projects
                               join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                               where p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true
                               select new Projects()
                               {
                                   ProjectID = p.ProjectID,
                                   ProjectName = p.ProjectName,
                                   ProjectCode = p.ProjectCode,
                                   ProjectType = pt.ProjectTypeName,
                                   RAGStatus = p.RAGStatus,
                                   RAGComments = p.RAGComments ?? "Comments not added",
                                   CommentsCreated = p.CommentsCreated
                               }).OrderBy(x => x.RAGStatus).Distinct().ToList();
            }

            ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");
            return View(projectData);
        }

        [HttpPost]
        public ActionResult ViewAllProjects(FormCollection form)
        {
            List<Projects> projectData = new List<Projects>();

            if (ModelState.IsValid)
            {
                var isReporting = (bool)Session["IsRerportingPerson"];
                int userId = int.Parse(Session["UserID"].ToString());
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                string projectID = (form["ProjectTypeDL"] == "") ? "0" : form["ProjectTypeDL"].ToString();
                int projectTypeID = int.Parse(projectID.ToString());
                bool status = form["Inactive"].Contains("true");
                DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName", projectID);

                //List<Projects> datas = new List<Projects>();
                if (projectTypeID == 0)
                {
                    if (isReporting)
                    {
                        if (!status)
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                        else
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                    }
                    else
                    {
                        if (!status)
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                        else
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                    }
                }
                else
                {
                    if (isReporting)
                    {
                        if (!status)
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true)
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                        else
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false)
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                    }
                    else
                    {
                        if (!status)
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true)
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                        else
                        {
                            projectData = (from p in db.Projects
                                           join pt in db.Master_ProjectTypes on p.ProjectTypeID equals pt.ProjectTypeID
                                           where pt.ProjectTypeID == projectTypeID && (p.IsDeleted == true || p.IsDeleted == null && p.IsActive == false)
                                           select new Projects()
                                           {
                                               ProjectID = p.ProjectID,
                                               ProjectName = p.ProjectName,
                                               ProjectCode = p.ProjectCode,
                                               ProjectType = pt.ProjectTypeName,
                                               RAGStatus = p.RAGStatus,
                                               RAGComments = p.RAGComments ?? "Comments not added",
                                               CommentsCreated = p.CommentsCreated
                                           }).OrderBy(x => x.RAGStatus).Distinct().ToList();
                        }
                    }
                }

                return View(projectData);
            }
            else
            {
                DSRCManagementSystem.Models.Projects objproject = new DSRCManagementSystem.Models.Projects();
                ViewBag.ProjectTypeList = new SelectList(objproject.GetProjectTypeList(), "ProjectTypeID", "ProjectTypeName");
                return View(projectData);
            }
        }

        public ActionResult Metrics(int id)
        {
            DSRCManagementSystem.Models.Projects objmodel = new DSRCManagementSystem.Models.Projects();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            //var val = objdb.Projects.Where(x => x.ProjectID == id).Select(x => x.Metrix).FirstOrDefault();
            //if (val != null)
            //{
            //    objmodel.Metrics = val.ToString();
            //}
            //else
            //{
            objmodel.Metrics = "";
            //}
            System.Web.HttpContext.Current.Application["metrics"] = id;
            return View(objmodel);
        }

        [HttpPost]
        public ActionResult Metrics(Projects objproject)
        {
            Session["Tab"] = "Two";
            int ProjectId = Convert.ToInt32(System.Web.HttpContext.Current.Application["metrics"]);
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.ProjectMetric obj = new DSRCManagementSystem.ProjectMetric();
            obj.Metrics = objproject.Metrics;
            obj.ProjectId = ProjectId;
            obj.Date = System.DateTime.Now;
            objdb.AddToProjectMetrics(obj);
            var value = objdb.Projects.Where(x => x.ProjectID == ProjectId).Select(o => o).FirstOrDefault();
            value.Metrix = objproject.Metrics;
            objdb.SaveChanges();
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult ViewMetrics(int ProjectID, string v)
        {
            ViewBag.val4 = v;
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            List<DSRCManagementSystem.Models.MetricHistory> obj = new List<DSRCManagementSystem.Models.MetricHistory>();
            obj = (from P in objdb.ProjectMetrics.Where(x => x.ProjectId == ProjectID)
                   select new DSRCManagementSystem.Models.MetricHistory
                   {
                       //ProjectId = P.ProjectId,
                       Metrics = P.Metrics,
                       Date = P.Date
                   }).ToList();

            ViewBag.ProjectID = ProjectID;

            return View(obj);
        }


        public ActionResult ProjectDetails(int Id, string v)
        {
            ViewBag.v1 = v;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var ProjectDetails = db.Projects.Where(o => o.ProjectID == Id).Select(o =>
                 new Projects()
                 {
                     ProjectID = o.ProjectID,
                     ProjectName = o.ProjectName,
                     ProjectCode = o.ProjectCode,
                     ProjectType = o.Master_ProjectTypes.ProjectTypeName,
                     DateCreated = o.DateCreated,
                     ProjectDescription = (((o.ProjectDescription != null) && (o.ProjectDescription.Trim() != "")) ? o.ProjectDescription : "-"),
                     SvnRepositoryUrl = (((o.SvnRepositoryUrl != null) && (o.SvnRepositoryUrl.Trim() != "")) ? o.SvnRepositoryUrl : "-"),
                     StartDateTime = o.ProjectStartDate.Value,
                     EndDateTime = o.ProjectEndDate.Value
                 }).FirstOrDefault();
            ProjectDetails.ProjectDescription = ProjectDetails.ProjectDescription != null ? ProjectDetails.ProjectDescription.Replace(Environment.NewLine, "</br>") : "";
            ProjectDetails.Members = (
                /****Bug HRMS-4419 retrieves role from Actual role instead of project role 
                from usr in db.Users
                where usr.IsActive == true
                join
                    usrproj in db.UserProjects.Where(o => o.ProjectID == Id) on usr.UserID equals usrproj.UserID
                join usrrole in db.UserRoles on usr.UserID equals usrrole.UserID
                join rol in db.Roles on usrrole.RoleID equals rol.RoleID
                */
                                      from usrproj in db.UserProjects.Where(o => o.ProjectID == Id)
                                      //join rol in db.Roles on usrproj.RoleID equals rol.RoleID
                                      join rol in db.Master_MemberTypes on usrproj.MemberTypeID equals rol.MemberTypeID
                                      join usr in db.Users.Where(u => u.IsActive == true) on usrproj.UserID equals usr.UserID
                                      select new ProjectMembers()
                                      {
                                          FirstName = usr.FirstName,
                                          LastName = usr.LastName,
                                          MemberTypeID = rol.MemberTypeID
                                      }
                                          ).OrderBy(x => x.MemberTypeID).AsQueryable();
            ProjectDetails.TechList = (from techval in db.TechnologyValues.Where(t => t.ProjectId == Id) join tech in db.Master_Technologies on techval.TecnologyId equals tech.ID select tech.Tecnology).ToList();
            ProjectDetails.ORMList = (from techval in db.ORMValues.Where(t => t.ProjectID == Id) join tech in db.Master_ORM on techval.ORM_Tools_ID equals tech.ID select tech.ORM_Tools).Distinct().ToList();
            ProjectDetails.DBList = (from techval in db.DataBaseValues.Where(t => t.ProjectID == Id) join tech in db.Master_DataBaseTechnolgy on techval.Database_Tools_ID equals tech.ID select tech.Database_Tools).Distinct().ToList();
            ProjectDetails.ThirdPartyList = (from techval in db.ThirdPartyValues.Where(t => t.ProjectID == Id) join tech in db.Master_ThirdParty on techval.ThirdParty_Tools_ID equals tech.ID select tech.ThirdParty_Tools).Distinct().ToList();
            ProjectDetails.SourceControlList = (from techval in db.SourceControlValues.Where(t => t.ProjectID == Id) join tech in db.Master_SourceControl on techval.SourceControlID equals tech.ID select tech.SourceControl_Tools).Distinct().ToList();
            ProjectDetails.MemberType = (from mt in db.Master_MemberTypes.OrderBy(x => x.MemberTypeID) select mt.MemberType).ToList();
            ProjectDetails.ProjectPlan = (from Planval in db.MileStoneValues.Where(t => t.ProjectID == Id) join Plan in db.Master_MileStones on Planval.MileStoneID equals Plan.MileStoneID select Plan.MileStoneName).ToList();
            return View(ProjectDetails);
        }

        [HttpGet]
        public ActionResult Tab(int Id, int TypeID, string v1)
        {
            ViewBag.val = v1;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.ProjectMapping objmodel = new DSRCManagementSystem.Models.ProjectMapping();
            objmodel.ProjectID = Id;
            objmodel.MemberTypeID = 0;
            if (TypeID != 0)
            {
                objmodel.MemberTypeID = TypeID;
            }
            ViewBag.IsReportingPerson = (bool)Session["IsRerportingPerson"];
            ViewData["ProjectID"] = Id;
            ViewBag.MemberTypeID = TypeID;
            return View();
        }

        [HttpPost]
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


        [HttpPost]
        public ActionResult AssignProject(string Value)
        {
            Session["Tab"] = "one";
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                var json_serializer = new JavaScriptSerializer();
                AssignedMembers memberObj = json_serializer.Deserialize<AssignedMembers>(Value);
                List<string> newMembers = new List<string>(memberObj.UserId);
                List<string> splitedNewMembers = new List<string>();
                foreach (var item in newMembers)
                {
                    splitedNewMembers.Add(item.Split('+')[0]);
                }
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    List<int> oldMembersList = db.UserProjects.Where(x => x.ProjectID == memberObj.ProjectId).Select(x => x.UserID).ToList();
                    List<int> newMembersList = splitedNewMembers.ConvertAll(s => Int32.Parse(s));

                    var toInsert = newMembersList.Except(oldMembersList).ToList();
                    var toDelete = oldMembersList.Except(newMembersList).ToList();

                    if (memberObj.UserId != null && memberObj.ProjectId != 0)
                    {
                        //StringBuilder SB = new StringBuilder();
                        if (toInsert.Count > 0)
                        {
                            for (int i = 0; i < memberObj.UserId.Count; i++)
                            {
                                string[] RoleId = memberObj.UserId[i].Split('+');
                                UserProject UP = new UserProject();
                                UP.ProjectID = memberObj.ProjectId;
                                UP.UserID = Convert.ToInt32(RoleId[0]);
                                //UP.RoleID = Convert.ToByte(RoleId[1]);
                                UP.MemberTypeID = Convert.ToByte(RoleId[1]);
                                if (UP.RoleID != 13)
                                    UP.IsBillable = true;
                                else
                                    UP.IsBillable = false;

                                var data = db.UserProjects.FirstOrDefault(x => (x.UserID == UP.UserID) && (x.ProjectID == UP.ProjectID));

                                if (data == null)
                                {
                                    db.UserProjects.AddObject(UP);
                                    //db.SaveChanges();
                                    string MemberType = string.Empty;
                                    if (UP.MemberTypeID == 1)
                                    {
                                        MemberType = "Managed Resources";
                                    }
                                    else if (UP.MemberTypeID == 2)
                                    {
                                        MemberType = "Billable Resources";
                                    }
                                    else
                                    {
                                        MemberType = "Additional/Buffer Resources";
                                    }
                                    var Project = db.Projects.FirstOrDefault(P => P.ProjectID == UP.ProjectID);
                                    var User = db.Users.FirstOrDefault(U => U.UserID == UP.UserID);

                                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project").Select(o => o.EmailTemplateID).FirstOrDefault();
                                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Assign Project").Select(x => x.TemplatePath).FirstOrDefault();
                                    if ((check != null) && (check != 0))
                                    {
                                        var objAssignProject = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project")
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
                                        string TemplatePathAssignProject = Server.MapPath(objAssignProject.Template);
                                        string htmlAssignProject = System.IO.File.ReadAllText(TemplatePathAssignProject);
                                        htmlAssignProject = htmlAssignProject.Replace("#MemberType", MemberType);
                                        htmlAssignProject = htmlAssignProject.Replace("#EmployeeID", User.EmpID);
                                        htmlAssignProject = htmlAssignProject.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                                        htmlAssignProject = htmlAssignProject.Replace("#ProjectName", Project.ProjectName);
                                        htmlAssignProject = htmlAssignProject.Replace("#ServerName", ServerName);
                                        htmlAssignProject = htmlAssignProject.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                        htmlAssignProject = htmlAssignProject.Replace("#CompanyName", company);
                                        var ToEmail = (from p in db.UserProjects
                                                       join u in db.Users
                                                       on p.UserID equals u.UserID
                                                       where p.ProjectID == UP.ProjectID && p.MemberTypeID == 1
                                                       select new
                                                       {
                                                           mailaddress = u.EmailAddress,
                                                           FirstName = u.FirstName,
                                                           LastName = u.LastName
                                                       }).ToList();

                                        //List<string> ToEmailList = ToEmail.ConvertAll(s => s.mailaddress.ToString());
                                        List<string> ToEmailList = ToEmail.Where(s => s.mailaddress != null).Select(o => (o.mailaddress).ToString()).ToList();

                                        string objAssignProjectTo = string.Join(",", ToEmailList);
                                        // List<string> Name = ToEmail.ConvertAll(s => s.FirstName + " " + s.LastName.ToString());
                                        List<string> Name = ToEmail.Where(s => s.mailaddress != null).Select(o => (o.FirstName + "  " + o.LastName).ToString()).ToList();
                                        var ManagedResources = String.Join(", ", Name.ToArray());
                                        htmlAssignProject = htmlAssignProject.Replace("#ManagedResources", ManagedResources);

                                        objAssignProject.CC = ProjectsController.GetUserEmail(db, objAssignProject.CC);
                                        if (objAssignProject.BCC != "")
                                        {
                                            objAssignProject.BCC = ProjectsController.GetUserEmail(db, objAssignProject.BCC);
                                        }

                                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                        if (ServerName != "http://win2012srv:88/")
                                        {

                                            List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                            //MailIds.Add("boobalan.k@dsrc.co.in");
                                            //MailIds.Add("shaikhakeel@dsrc.co.in");
                                            //MailIds.Add("ramesh.S@dsrc.co.in");
                                            //MailIds.Add("aruna.m@dsrc.co.in");
                                            //MailIds.Add("kirankumar@dsrc.co.in");

                                            string EmailAddress = "";

                                            foreach (string mail in MailIds)
                                            {
                                                EmailAddress += mail + ",";
                                            }

                                            EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);


                                            string CCMailId = "kirankumar@dsrc.co.in ";
                                            string BCCMailId = "virupaksha.gaddad@dsrc.co.in";


                                            Task.Factory.StartNew(() =>
                                            {
                                                /// DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                                                //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                                //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                                //string[] words;

                                                //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                                //string pathvalue = "~/" + words[1];
                                                string pathvalue = CommonLogic.getLogoPath();
                                                // var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                                DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject + " - Test Mail Please Ignore", "", htmlAssignProject + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                                            });

                                        }
                                        else
                                        {
                                            Task.Factory.StartNew(() =>
                                            {
                                                //DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                                                //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                                //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                                //string[] words;

                                                //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                                //string pathvalue = "~/" + words[1];
                                                string pathvalue = CommonLogic.getLogoPath();
                                                //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                                DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProject.Subject, "", htmlAssignProject, "HRMS@dsrc.co.in", objAssignProjectTo, objAssignProject.CC, objAssignProject.BCC, Server.MapPath(pathvalue));
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                        ExceptionHandlingController.TemplateMissing("Assign Project", folder, ServerName);
                                    }


                                }


                            }
                        }
                        if (toDelete.Count > 0)
                        {
                            foreach (var item in toDelete)
                            {
                                var record = db.UserProjects.FirstOrDefault(x => x.UserID == item && x.ProjectID == memberObj.ProjectId);
                                db.UserProjects.DeleteObject(record);

                                string MemberType = string.Empty;

                                if (record.MemberTypeID == 1)
                                {
                                    MemberType = "Managed Resources";
                                }
                                else if (record.MemberTypeID == 2)
                                {
                                    MemberType = "Billable Resources";
                                }
                                else
                                {
                                    MemberType = "Additional/Buffer Resources";
                                }
                                var Project = db.Projects.FirstOrDefault(P => P.ProjectID == record.ProjectID);
                                var User = db.Users.FirstOrDefault(U => U.UserID == record.UserID);

                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Assign Project Delete").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Assign Project Delete").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objAssignProjectDelete = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Assign Project Delete")
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
                                    string TemplatePathAssignProject = Server.MapPath(objAssignProjectDelete.Template);
                                    string htmlAssignProjectDelete = System.IO.File.ReadAllText(TemplatePathAssignProject);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#MemberType", MemberType);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeID", User.EmpID);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#EmployeeName", User.FirstName + " " + User.LastName);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ProjectName", Project.ProjectName);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ServerName", ServerName);
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#CompanyName", company);

                                    var ToEmail = (from p in db.UserProjects
                                                   join u in db.Users
                                                   on p.UserID equals u.UserID
                                                   where p.ProjectID == record.ProjectID && p.MemberTypeID == 1
                                                   select new
                                                   {
                                                       u
                                                   }).ToList();
                                    List<string> ToEmailList = ToEmail.ConvertAll(s => s.u.EmailAddress.ToString());
                                    string objAssignProjectDeleteTo = string.Join(",", ToEmailList);
                                    List<string> Name = ToEmail.ConvertAll(s => s.u.FirstName + "  " + s.u.LastName.ToString());
                                    var ManagedResources = String.Join(", ", Name.ToArray());
                                    htmlAssignProjectDelete = htmlAssignProjectDelete.Replace("#ManagedResources", ManagedResources);

                                    objAssignProjectDelete.CC = ProjectsController.GetUserEmail(db, objAssignProjectDelete.CC);
                                    if (objAssignProjectDelete.BCC != "")
                                    {
                                        objAssignProjectDelete.BCC = ProjectsController.GetUserEmail(db, objAssignProjectDelete.BCC);
                                    }

                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    if (ServerName != "http://win2012srv:88/")
                                    {

                                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                        //MailIds.Add("boobalan.k@dsrc.co.in");
                                        //MailIds.Add("shaikhakeel@dsrc.co.in");
                                        //MailIds.Add("ramesh.S@dsrc.co.in");
                                        //MailIds.Add("aruna.m@dsrc.co.in");
                                        //MailIds.Add("kirankumar@dsrc.co.in");

                                        string EmailAddress = "";

                                        foreach (string mail in MailIds)
                                        {
                                            EmailAddress += mail + ",";
                                        }

                                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                        string CCMailId = "kirankumar@dsrc.co.in ";
                                        string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                                        Task.Factory.StartNew(() =>
                                        {
                                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                            //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //string[] words;

                                            //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                            //string pathvalue = "~/" + words[1];
                                            string pathvalue = CommonLogic.getLogoPath();
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject + " - Test Mail Please Ignore", null, htmlAssignProjectDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                                        });

                                    }
                                    else
                                    {

                                        Task.Factory.StartNew(() =>
                                        {
                                            //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                            //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //string[] words;

                                            //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                            //string pathvalue = "~/" + words[1];
                                            string pathvalue = CommonLogic.getLogoPath();
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objAssignProjectDelete.Subject, "", htmlAssignProjectDelete, "HRMS@dsrc.co.in", objAssignProjectDeleteTo, objAssignProjectDelete.CC, objAssignProjectDelete.BCC, Server.MapPath(pathvalue));
                                        });
                                    }
                                }
                                else
                                {
                                    //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Assign Project Delete", folder, ServerName);
                                }
                            }
                        }

                        db.SaveChanges();
                        return Json(true);
                    }
                    return Json(false);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw new Exception(Ex.Message);
            }
        }
        private static string GetUserEmail(DSRCManagementSystemEntities1 db, string Attendee)
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



        private IList<ViewMembers> GetMembers(int PId = 0, bool ManagedResources = true, bool BillableResources = true, bool AdditionalBufferResources = true, bool AccountManager = true, bool QA = true, bool TeamLead = true, bool Marketing = true)
        {

            List<int> objType = new List<int>();
            if (ManagedResources) { objType.Add(1); }
            if (BillableResources) { objType.Add(2); }
            if (AdditionalBufferResources) { objType.Add(3); }
            if (AccountManager) { objType.Add(4); }
            if (QA) { objType.Add(5); }
            if (TeamLead) { objType.Add(6); }
            if (Marketing) { objType.Add(7); }
            List<ViewMembers> Members = new List<ViewMembers>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                //int userId = int.Parse(Session["UserID"].ToString());
                if (PId == 0)
                {
                    {
                        Members = (from a in db.UserProjects
                                   join b in db.Users
                                   on a.UserID equals b.UserID
                                   join c in db.Projects
                                   on a.ProjectID equals c.ProjectID
                                   /*join d in db.Rolesf
                                   on a.RoleID equals d.RoleID*/
                                   join d in db.Master_MemberTypes
                                   on a.MemberTypeID equals d.MemberTypeID
                                   //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                                   where b.IsActive == true && objType.Contains(d.MemberTypeID)
                                   select new ViewMembers()
                                   {
                                       EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                       UserId = b.UserID,
                                       ProjectName = c.ProjectName,
                                       ProjectId = c.ProjectID,
                                       //RoleName = d.RoleName,
                                       //RoleId = d.RoleID,
                                       MemberType = d.MemberType,
                                       MemberTypeID = d.MemberTypeID,
                                       IsBillable = a.IsBillable ?? false,
                                       IsUnderNoticePeriod = b.IsUnderNoticePeriod
                                   }).OrderBy(x => x.EmployeeName).ToList();
                    }
                }
                else
                {
                    Members = (from a in db.UserProjects
                               join b in db.Users
                               on a.UserID equals b.UserID
                               join c in db.Projects
                               on a.ProjectID equals c.ProjectID
                               /*join d in db.Roles
                               on a.RoleID equals d.RoleID*/
                               join d in db.Master_MemberTypes
                               on a.MemberTypeID equals d.MemberTypeID
                               //join up in db.UserProjects.Where(x => x.UserID == userId) on a.ProjectID equals up.ProjectID
                               where a.ProjectID == PId && b.IsActive == true && objType.Contains(d.MemberTypeID)

                               select new ViewMembers()
                               {
                                   EmployeeName = (b.FirstName + " " + (b.LastName ?? "")).Trim(),
                                   UserId = b.UserID,
                                   ProjectName = c.ProjectName,
                                   ProjectId = c.ProjectID,
                                   //RoleName = d.RoleName,
                                   //RoleId = d.RoleID,
                                   //MemberType = d.MemberType1,
                                   MemberType = d.MemberType,
                                   MemberTypeID = d.MemberTypeID,
                                   IsBillable = a.IsBillable ?? false,
                                   IsUnderNoticePeriod = b.IsUnderNoticePeriod
                               }).OrderBy(x => x.EmployeeName).ToList();
                }
            }
            return Members;
        }

        public ActionResult GetAssignedProject(int pId)
        {
            List<SelectListItem> Members = new List<SelectListItem>();
            var assignedMembers = GetMembers(pId);
            foreach (var item in assignedMembers)
            {
                //Members.Add(new SelectListItem { Text = item.EmployeeName + '(' + item.RoleName + ')', Value = (item.UserId.ToString() + '+' + item.RoleId.ToString()) });
                Members.Add(new SelectListItem { Text = item.EmployeeName + '(' + item.MemberType + ')', Value = (item.UserId.ToString() + '+' + item.MemberTypeID.ToString()) });
            }
            return Json(Members, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult ResourcesTabWithAssignProject(int Id ,string v)
        {
            ViewBag.val2 = v;
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            ProjectMapping ObjPM = new ProjectMapping();
            ObjPM.EmployeeList = GetNames();
            ObjPM.ProjectList = GetProjects();
            ObjPM.RoleList = GetRoles();
            ObjPM.Members = GetMembers();
            var Project = objdb.Projects.Where(x => x.ProjectID == Id).Select(o => o.ProjectName).FirstOrDefault();
            ObjPM.ProjectName = Project;
            ObjPM.ProjectID = Id;
            var checkInactive = objdb.Projects.Where(p => p.ProjectID == Id).Select(p => p.IsActive).FirstOrDefault();
            if (checkInactive == false)
            {
                ObjPM.InActive = true;
            }
            return View(ObjPM);

        }


        private List<Master_Technologies> LoadTechnologies()
        {
            List<Master_Technologies> Technology = new List<Master_Technologies>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_Technologies
                              select data).OrderBy(x => x.Tecnology).ToList();
            }
            return Technology;
        }

        private List<Master_MileStones> LoadProjectPlan()
        {
            List<Master_MileStones> MileStone = new List<Master_MileStones>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                MileStone = (from data in db.Master_MileStones
                             select data).OrderBy(x => x.MileStoneName).ToList();
            }
            return MileStone;
        }



        private List<Master_ORM> LoadORM()
        {
            List<Master_ORM> Technology = new List<Master_ORM>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_ORM
                              select data).OrderBy(x => x.ORM_Tools).ToList();
            }
            return Technology;
        }
        private List<Master_DataBaseTechnolgy> LoadDB()
        {
            List<Master_DataBaseTechnolgy> Technology = new List<Master_DataBaseTechnolgy>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_DataBaseTechnolgy
                              select data).OrderBy(x => x.Database_Tools).ToList();
            }
            return Technology;
        }
        private List<Master_ThirdParty> LoadThirdParty()
        {
            List<Master_ThirdParty> Technology = new List<Master_ThirdParty>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_ThirdParty
                              select data).OrderBy(x => x.ThirdParty_Tools).ToList();
            }
            return Technology;
        }
        private List<Master_SourceControl> LoadSourceControl()
        {
            List<Master_SourceControl> Technology = new List<Master_SourceControl>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_SourceControl
                              select data).OrderBy(x => x.SourceControl_Tools).ToList();
            }
            return Technology;
        }
        public ActionResult AddNew()
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

            var ProjectLead = (from t in objdb.Users.Where(x => x.IsActive == true && x.UserStatus != 6)
                               select new
                               {
                                   Id3 = t.UserID,
                                   // Name=t.FirstName+t.LastName,
                                   FirstName = (t.FirstName)??"" + " " + (t.LastName)??"",
                                   LastName = t.LastName,


                               }).ToList();

            ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id3", "FirstName", "LastName");



            Projects objproject = new Projects();
            objproject.IsActive = true;
            //List<int> selected = new List<int>() { 15 };
            ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology");
            ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools");
            ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools");
            ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools");
            ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools");
            ViewBag.ProjectPlan = new SelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName");
            ViewData["Phasecount"] = LoadProjectPlan().Count();
            ViewBag.ProjectTypeList = new SelectList(new[] { new Master_ProjectTypes() { ProjectTypeID = 0, ProjectTypeName = "---Select---" } }.Union(objproject.GetProjectTypeList()), "ProjectTypeID", "ProjectTypeName");
            return View(objproject);
        }
        [HttpPost]
        public ActionResult AddNew(Projects projectmodel, List<int> TechList1, List<int> ORMList1, List<int> DBList1, List<int> ThirdPartyList1, List<int> SourceControlsList1)
        {
            //string TechnologyId = collection["TechList"].ToString();    
            DSRCManagementSystemEntities1 db1 = new DSRCManagementSystemEntities1();



            string ServerName = AppValue.GetFromMailAddress("ServerName");


            if (ModelState.IsValid)
            {
                var check =
            db1.Projects.Where(x => x.ProjectName == projectmodel.ProjectName)
                .Select(o => o.ProjectID)
                .FirstOrDefault();
                if (check != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var res = db.Projects.Any(x => x.ProjectName == projectmodel.ProjectName && x.IsActive == true);
                    if (!res)
                    {



                        var t = new DSRCManagementSystem.Project //Make sure you have a table called test in DB
                        {
                            ProjectName = projectmodel.ProjectName,
                            ProjectCode = projectmodel.ProjectCode,
                            ProjectDescription = projectmodel.ProjectDescription,
                            ProjectTypeID = int.Parse(projectmodel.ProjectType.ToString()),
                            SvnRepositoryUrl = projectmodel.SvnRepositoryUrl,
                            IsActive = projectmodel.IsActive,
                            DateCreated = DateTime.Now,
                            RAGStatus = 3,
                            ProjectStartDate = projectmodel.StartDateTime,
                            //ManagedResources = projectmodel.ManagedResources,
                            ProjectEndDate = projectmodel.EndDateTime,
                            IsDeleted = false
                        };
                        db.AddToProjects(t);
                        //db.Projects.AddObject(t);
                        db.SaveChanges();

                        string UsersName = "";
                        List<int> objuser = new List<int>();
                        if (projectmodel.ManagedResources != null)
                        {
                            string[] values = projectmodel.ManagedResources.Split(',');
                            for (int y = 0; y < values.Count(); y++)
                            {
                                objuser.Add(Convert.ToInt32(values[y]));
                            }

                            foreach (string userid in values)
                            {
                                if (userid != "")
                                {
                                    int UserId = Convert.ToInt32(userid);
                                    UsersName += db.Users.Where(o => o.UserID == UserId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault() + ", ";
                                }
                            }
                            UsersName = UsersName.Remove(UsersName.Length - 2, 2);
                        }

                        var ProjectId = Convert.ToInt32(projectmodel.ProjectID);

                        if (projectmodel.Phases != null)
                        {
                            foreach (string phase in projectmodel.Phases)
                            {
                                var SplittedPhase = phase.Split(',');
                                string PhaseName = SplittedPhase[0].Trim();
                                int MileStoneId;
                                if (SplittedPhase.Count() == 3)
                                {

                                    MileStoneId = db.Master_MileStones.FirstOrDefault(x => x.MileStoneName.Equals(PhaseName)).MileStoneID;

                                    db.MileStoneValues.AddObject(new MileStoneValue
                                    {
                                        ProjectID = t.ProjectID,
                                        MileStoneID = MileStoneId,
                                        StartDate = Convert.ToDateTime(SplittedPhase[1]),
                                        EndDate = Convert.ToDateTime(SplittedPhase[2])

                                    });

                                }
                            }
                            db.SaveChanges();
                        }

                        for (int z = 0; z < objuser.Count(); z++)
                        {
                            DSRCManagementSystem.UserProject Objvalue = new DSRCManagementSystem.UserProject();
                            var UserId = Convert.ToInt32(objuser[z]);
                            var RoleId = db.UserRoles.Where(x => x.UserID == UserId).Select(o => o.RoleID).FirstOrDefault();
                            Objvalue.ProjectID = t.ProjectID;
                            Objvalue.UserID = UserId;
                            Objvalue.RoleID = RoleId;
                            Objvalue.IsBillable = false;
                            Objvalue.MemberTypeID = 1;
                            db.AddToUserProjects(Objvalue);
                            db.SaveChanges();
                        }


                        int projectid = db.Projects.Where(o => o.ProjectName == projectmodel.ProjectName && o.ProjectCode == projectmodel.ProjectCode && o.IsActive == true).Select(o => o.ProjectID).FirstOrDefault();

                        int userId = int.Parse(Session["UserID"].ToString());

                        foreach (int id in TechList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new TechnologyValue();
                                obj_save.ProjectId = projectid;
                                obj_save.TecnologyId = id;
                                db.TechnologyValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in ORMList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new ORMValue();
                                obj_save.ProjectID = projectid;
                                obj_save.ORM_Tools_ID = id;
                                db.ORMValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in DBList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new DataBaseValue();
                                obj_save.ProjectID = projectid;
                                obj_save.Database_Tools_ID = id;
                                db.DataBaseValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        //foreach (int id in ProjectPlan1)
                        //{
                        //    if (id != 0)
                        //    {
                        //        var obj_save = new MileStoneValue();
                        //        obj_save.ProjectID = projectid;
                        //        obj_save.MileStoneID = id;
                        //        db.MileStoneValues.AddObject(obj_save);
                        //        db.SaveChanges();
                        //    }
                        //}
                        foreach (int id in ThirdPartyList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new ThirdPartyValue();
                                obj_save.ProjectID = projectid;
                                obj_save.ThirdParty_Tools_ID = id;
                                db.ThirdPartyValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in SourceControlsList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new SourceControlValue();
                                obj_save.ProjectID = projectid;
                                obj_save.SourceControlID = id;
                                db.SourceControlValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }


                        var objProjectStatus = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Add New Project")
                                                join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                select new DSRCManagementSystem.Models.Email
                                                {
                                                    To = p.To,
                                                    CC = p.CC,
                                                    BCC = p.BCC,
                                                    Subject = p.Subject,
                                                    Template = q.TemplatePath
                                                }).FirstOrDefault();

                        var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Add New Fixed Project").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "Add New Fixed Project").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((checks != null) && (checks != 0))
                        {
                            if (projectmodel.ProjectType.ToString() == "1")
                            {


                                objProjectStatus = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Add New Fixed Project")
                                                    join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                    select new DSRCManagementSystem.Models.Email
                                                    {
                                                        To = p.To,
                                                        CC = p.CC,
                                                        BCC = p.BCC,
                                                        Subject = p.Subject,
                                                        Template = q.TemplatePath
                                                    }).FirstOrDefault();
                            }

                            projectmodel.ProjectTypeID = int.Parse(projectmodel.ProjectType.ToString());
                            projectmodel.ProjectTypeName = db.Master_ProjectTypes.FirstOrDefault(o => o.ProjectTypeID == projectmodel.ProjectTypeID).ProjectTypeName;
                            projectmodel.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                            projectmodel.ManagedResources = UsersName;


                            string TemplatePathProjectStatus;

                            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();




                            TemplatePathProjectStatus = Server.MapPath(objProjectStatus.Template);
                            string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                            htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                            htmlProjectStatus = htmlProjectStatus.Replace("#ProjectCode", projectmodel.ProjectCode);
                            htmlProjectStatus = htmlProjectStatus.Replace("#ProjectName", projectmodel.ProjectName);
                            htmlProjectStatus = htmlProjectStatus.Replace("#ProjectType", projectmodel.ProjectTypeName);
                            htmlProjectStatus = htmlProjectStatus.Replace("#CreatedBy", projectmodel.CreatedBy);
                            htmlProjectStatus = htmlProjectStatus.Replace("#ManagedBy", UsersName);
                            //htmlProjectStatus = htmlProjectStatus.Replace("#ManagdBy", projectmodel.ManagedResources);
                            htmlProjectStatus = htmlProjectStatus.Replace("#ServerName", ServerName);
                            htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);
                           // if (projectmodel.ProjectType.ToString() == "1")
                           // {
                                htmlProjectStatus = htmlProjectStatus.Replace("#StartDateTime", projectmodel.StartDateTime.Value.ToString("dd MMM yyyy"));
                                htmlProjectStatus = htmlProjectStatus.Replace("#EndDateTime", projectmodel.EndDateTime.Value.ToString("dd MMM yyyy"));
                          //  }


                            // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                            if (ServerName != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("mariappan.j@dsrc.co.in");
                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                string CCMailId = "kirankumar@dsrc.co.in ";
                                string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                                Task.Factory.StartNew(() =>
                                {
                                    //   var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                    //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //string[] words;

                                    //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                    //string pathvalue = "~/" + words[1];
                                    string pathvalue = CommonLogic.getLogoPath();
                                    DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));

                                    //DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));

                                    //DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, attachments.ToArray());
                                });

                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                    ////var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //string[] words;
                                   
                                    //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                    //string pathvalue = "~/" + words[1];
                                    string pathvalue = CommonLogic.getLogoPath();
                                    DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", objProjectStatus.To, objProjectStatus.CC, objProjectStatus.BCC, Server.MapPath(pathvalue));

                                   
                                    //DsrcMailSystem.MailSender.SendMail(null, "Project RAG Status", "", MailBuilder.ProjectStatus(model.ProjectName, model.RAGStatusComments), "HRMS@dsrc.co.in", Email, attachments.ToArray());
                                });
                            }
                        }
                        else
                        {
                            // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                            ExceptionHandlingController.TemplateMissing("Add New Fixed Project", folders, ServerName);
                        }

                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                        ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", selectedValues: TechList1);
                        ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools", selectedValues: ORMList1);
                        ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools", selectedValues: DBList1);
                        ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools", selectedValues: ThirdPartyList1);
                        ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools", selectedValues: SourceControlsList1);
                        var ProjectLead = (from t in objdb.Users
                                           select new
                                           {
                                               Id = t.UserID,
                                               // Name=t.FirstName+t.LastName,
                                               FirstName = t.FirstName,
                                               LastName = t.LastName,


                                           }).ToList();

                        ViewBag.Leaders = new MultiSelectList(ProjectLead, "Id3", "FirstName", "LastName");
                        ViewBag.ProjectPlan = new MultiSelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName");


                        ViewBag.ProjectTypeList = new SelectList(new[] { new Master_ProjectTypes() { ProjectTypeID = 0, ProjectTypeName = "---Select---" } }.Union(projectmodel.GetProjectTypeList()), "ProjectTypeID", "ProjectTypeName", projectmodel.ProjectType);

                        ModelState.AddModelError("ProjectName", "Project name already exist.");
                        return View(projectmodel);
                    }
                }
            }
            else
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
                Projects objproject = new Projects();
                objproject.IsActive = true;
                //List<int> selected = new List<int>() { 15,40 };
                ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", TechList1);
                ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools", ORMList1);
                ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools", DBList1);
                ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools", ThirdPartyList1);
                ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools", SourceControlsList1);
                var ProjectLead = (from t in objdb.Users
                                   select new
                                   {
                                       Id = t.UserID,
                                       // Name=t.FirstName+t.LastName,
                                       FirstName = t.FirstName,
                                       LastName = t.LastName,


                                   }).ToList();

                ViewBag.Leaders = new MultiSelectList(ProjectLead, "FirstName", "LastName");
                ViewBag.ProjectPlan = new MultiSelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName");

                ViewBag.ProjectTypeList = new SelectList(new[] { new Master_ProjectTypes() { ProjectTypeID = 0, ProjectTypeName = "---Select---" } }.Union(objproject.GetProjectTypeList()), "ProjectTypeID", "ProjectTypeName");
                return View(objproject);
            }
        }
        public ActionResult EditProject(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var result = db.Projects.Where(o => o.ProjectID == ID)
                                    .Select(o => new Projects
                                    {
                                        ProjectID = o.ProjectID,
                                        ProjectName = o.ProjectName,
                                        ProjectCode = o.ProjectCode,
                                        DateCreated = o.DateCreated,
                                        IsActive = o.IsActive,
                                        ProjectDescription = o.ProjectDescription,

                                        ProjectType = o.Master_ProjectTypes.ProjectTypeName,
                                        SvnRepositoryUrl = o.SvnRepositoryUrl,
                                        StartDateTime = o.ProjectStartDate.Value,
                                        EndDateTime = o.ProjectEndDate.Value

                                    }).FirstOrDefault();

            if (result.IsActive == false)
            {
                result.Isactiveorwhat = true;
                TempData["Isdelete"] = 1;
            }




            var Milestonevalue = db.MileStoneValues.Where(x => x.ProjectID == ID).Select(x => x).ToList();
            List<string> phases = new List<string>();


            foreach (var Milestone in Milestonevalue)
                phases.Add(db.Master_MileStones.FirstOrDefault(x => x.MileStoneID == Milestone.MileStoneID).MileStoneName.ToString() + "," + Convert.ToDateTime(Milestone.StartDate).ToString("dd-MM-yyyy") + "," + Convert.ToDateTime(Milestone.EndDate).ToString("dd-MM-yyyy"));
            var disinct = phases.Distinct().ToList();
            result.Phases = disinct;
            result.ProjectTypeLIst = db.Master_ProjectTypes.Select(o => o.ProjectTypeName).ToList<string>();
            result.ProjectTypeLIst.Remove(result.ProjectType);
            result.ProjectTypeLIst.Insert(0, result.ProjectType);
            var TecnologyList = db.TechnologyValues.Where(o => o.ProjectId == ID).Select(o => o.TecnologyId).ToList();
            
            var ResourcesList = db.UserProjects.Where(x =>x.ProjectID == ID&&x.MemberTypeID==1).Select(o => o.UserID).ToList();
            

            var Users = (from u in db.Users.Where(x => x.IsActive == true)
                         select new
                         {
                             UserID = u.UserID,
                             Names = (u.FirstName)??"" + " " + (u.LastName)??""
                         }).ToList();


            ViewBag.Resources = new MultiSelectList(Users, "UserID", "Names", ResourcesList);
            ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", TecnologyList);
            var ORMList = db.ORMValues.Where(o => o.ProjectID == ID).Select(o => o.ORM_Tools_ID).ToList();
            ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools", ORMList);
            var DBList = db.DataBaseValues.Where(o => o.ProjectID == ID).Select(o => o.Database_Tools_ID).ToList();
            ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools", DBList);
            var ThirdPartyList = db.ThirdPartyValues.Where(o => o.ProjectID == ID).Select(o => o.ThirdParty_Tools_ID).ToList();
            ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools", ThirdPartyList);
            var SourceControlList = db.SourceControlValues.Where(o => o.ProjectID == ID).Select(o => o.SourceControlID).ToList();
            ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools", SourceControlList);
            var ProjectPlanList = db.MileStoneValues.Where(o => o.ProjectID == ID).Select(o => o.MileStoneID).ToList();
            ViewBag.ProjectPlan = new SelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName", ProjectPlanList);
            ViewData["Phasecount"] = LoadProjectPlan().Count();
            return View(result);
        }

        [HttpPost]
        public ActionResult EditProject(Projects data, List<int> TechList1, List<int> ORMList1, List<int> DBList1, List<int> ThirdPartyList1, List<int> SourceControlsList1, List<string> Phases)
        {
            int ID = data.ProjectID;
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            List<int> objvalue = new List<int>();

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var ORGResult = db.Projects.FirstOrDefault(o => o.ProjectID == data.ProjectID);
            bool boolProjectTime = false;
            if (ORGResult.ProjectStartDate != data.StartDateTime || ORGResult.ProjectEndDate != data.EndDateTime)
            {
                boolProjectTime = true;
            }


            //var Milestonevalue = db.MileStoneValues.Where(x => x.ProjectID == ID).Select(x => x).ToList();
            //var phases = new List<string>();
            //List<int> ProjectPlan1 = Milestonevalue.Select(x => x.MileStoneID).ToList();

            //foreach (var Milestone in Milestonevalue)
            //    phases.Add(db.MileStones.FirstOrDefault(x => x.MileStoneID == Milestone.MileStoneID).MileStoneName.ToString() + "," + Convert.ToDateTime(Milestone.StartDate).ToString("dd-MM-yyyy") + "," + Convert.ToDateTime(Milestone.EndDate).ToString("dd-MM-yyyy"));


            List<int> ORGProjectPlan = new List<int>();
            //ORGProjectPlan = db.MileStoneValues.Where(x => x.ProjectID == data.ProjectID).Select(o => o.MileStoneID).ToList();            
            //var firstNotSecond = ORGProjectPlan.Except(ProjectPlan1).ToList();
            //var secondNotFirst = ProjectPlan1.Except(ORGProjectPlan).ToList();
            //bool boolProjectPlan = false;
            //if (firstNotSecond.Count != 0 || secondNotFirst.Count != 0)
            //{
            //    boolProjectPlan = true;
            //}
            var result = db.Projects.Where(o => o.ProjectID == ID)
                                    .Select(o => new Projects
                                    {
                                        ProjectID = o.ProjectID,
                                        ProjectName = o.ProjectName,
                                        ProjectCode = o.ProjectCode,
                                        DateCreated = o.DateCreated,
                                        IsActive = o.IsActive,
                                        ProjectDescription = o.ProjectDescription,
                                        ProjectType = o.Master_ProjectTypes.ProjectTypeName,
                                        SvnRepositoryUrl = o.SvnRepositoryUrl,
                                        StartDateTime = o.ProjectStartDate.Value,
                                        EndDateTime = o.ProjectEndDate.Value
                                    }).FirstOrDefault();
            result.ProjectTypeLIst = db.Master_ProjectTypes.Select(o => o.ProjectTypeName).ToList<string>();
            result.ProjectTypeLIst.Remove(result.ProjectType);
            result.ProjectTypeLIst.Insert(0, result.ProjectType);
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                // DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                {
                    var obj = db.Projects.Where(o => o.ProjectID == data.ProjectID).Select(o => o).FirstOrDefault();
                    obj.ProjectName = data.ProjectName;
                    obj.SvnRepositoryUrl = data.SvnRepositoryUrl;
                    obj.ProjectStartDate = data.StartDateTime;
                    obj.ProjectEndDate = data.EndDateTime;
                    obj.ProjectCode = data.ProjectCode;
                    obj.ProjectDescription = data.ProjectDescription;
                    obj.ProjectTypeID = db.Master_ProjectTypes.Where(o => o.ProjectTypeName == data.ProjectType).Select(o => o.ProjectTypeID).FirstOrDefault();
                    obj.IsActive = data.IsActive;
                    db.SaveChanges();

                    if (data.IsActive == false)
                    {
                        TempData["IsDelete"] = 0;
                        obj.IsDeleted = false;
                    }
                    if (data.IsActive == true)
                    {
                        obj.IsDeleted = false;
                    }

                    if (!data.IsActive.Value)
                    {
                        var members = db.UserProjects.Where(x => x.ProjectID == data.ProjectID).Select(x => x).ToList();

                        for (int i = 0; i < members.Count(); i++)
                        {
                            objvalue.Add(Convert.ToInt32(members[i].UserID));
                        }

                        foreach (var item in members)
                        {
                            db.UserProjects.DeleteObject(item);
                        }
                        db.SaveChanges();


                        var remove = db.TechnologyValues.Where(o => o.ProjectId == ID).Select(o => o).ToList();
                        foreach (var item in remove)
                            db.TechnologyValues.DeleteObject(item);
                        db.SaveChanges();

                        var removeProjectPlan = db.MileStoneValues.Where(o => o.ProjectID == ID).Select(o => o).ToList();
                        foreach (var item in removeProjectPlan)
                            db.MileStoneValues.DeleteObject(item);
                        db.SaveChanges();

                    }

                    else
                    {

                        foreach (int id in TechList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new TechnologyValue();
                                obj_save.ProjectId = ID;
                                obj_save.TecnologyId = id;
                                db.TechnologyValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in ORMList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new ORMValue();
                                obj_save.ProjectID = ID;
                                obj_save.ORM_Tools_ID = id;
                                db.ORMValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in DBList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new DataBaseValue();
                                obj_save.ProjectID = ID;
                                obj_save.Database_Tools_ID = id;
                                db.DataBaseValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        //var delete = db.MileStoneValues.Where(x => x.ProjectID == ID).Select(o => o).ToList();


                        //foreach (var x in delete)
                        //{ 
                        


                        //}

                        var delete = (from y in db.MileStoneValues
                                 select y).Where(x=>x.ProjectID==ID).ToList();

                        foreach (var y in delete)
                        {
                            db.MileStoneValues.DeleteObject(y);
                            db.SaveChanges();
                        }

                        if (Phases != null)
                        {

                        foreach (string phase in Phases)
                        {
                           
                                var SplittedPhase = phase.Split(',');
                                string PhaseName = SplittedPhase[0].Trim();
                                int MileStoneId;
                                if (SplittedPhase.Count() == 3)
                                {
                                    MileStoneId = db.Master_MileStones.FirstOrDefault(x => x.MileStoneName.Equals(PhaseName)).MileStoneID;
                                    var obj_save = new MileStoneValue();

                                    obj_save.ProjectID = ID;
                                    obj_save.MileStoneID = MileStoneId;
                                    obj_save.StartDate = Convert.ToDateTime(SplittedPhase[1]);
                                    obj_save.EndDate = Convert.ToDateTime(SplittedPhase[2]);
                                    db.MileStoneValues.AddObject(obj_save);
                                    db.SaveChanges();
                                }
                            }
                        }


                        //    foreach (string phase in Phases)
                        //    {
                        //        if (phase != null)
                        //        {
                        //            var SplittedPhase = phase.Split(',');
                        //            string PhaseName = SplittedPhase[0].Trim();
                        //            int MileStoneId;
                        //            if (SplittedPhase.Count() == 3)
                        //            {

                        //                MileStoneId = db.Master_MileStones.FirstOrDefault(x => x.MileStoneName.Equals(PhaseName)).MileStoneID;

                        //                db.MileStoneValues.AddObject(new MileStoneValue
                        //                {
                        //                    ProjectID = ID,
                        //                    MileStoneID = MileStoneId,
                        //                    StartDate = Convert.ToDateTime(SplittedPhase[1]),
                        //                    EndDate = Convert.ToDateTime(SplittedPhase[2])

                        //                });

                        //            }
                        //            db.SaveChanges();
                        //        }
                        //}


                        foreach (int id in ThirdPartyList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new ThirdPartyValue();

                                obj_save.ProjectID = ID;
                                obj_save.ThirdParty_Tools_ID = id;
                                db.ThirdPartyValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }
                        foreach (int id in SourceControlsList1)
                        {
                            if (id != 0)
                            {
                                var obj_save = new SourceControlValue();
                                obj_save.ProjectID = ID;
                                obj_save.SourceControlID = id;
                                db.SourceControlValues.AddObject(obj_save);
                                db.SaveChanges();
                            }
                        }

                        string UsersName = "";
                               
                        List<int> objuser = new List<int>();
                        if (data.ManagedResources != null)
                        {
                            string[] values = data.ManagedResources.Split(',');
                            for (int y = 0; y < values.Count(); y++)
                            {
                                objuser.Add(Convert.ToInt32(values[y]));
                            }
                            foreach (string userid in values)
                            {
                                if (userid != "")
                                {
                                    int UserId = Convert.ToInt32(userid);
                                    UsersName += db.Users.Where(o => o.UserID == UserId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault() + ", ";
                                }
                            }
                            UsersName = UsersName.Remove(UsersName.Length - 2, 2);
                        }


                        //---------------------------------
                        var select = db.UserProjects.Where(x => x.ProjectID == ID).Select(o => o.UserID).ToList();

                        var ex = select.Except(objuser).ToList();

                        foreach (var t in ex)
                        {
                            var del = db.UserProjects.Where(x => x.UserID == t && x.ProjectID == ID).Select(o => o).FirstOrDefault();
                            db.UserProjects.DeleteObject(del);
                        }
                        db.SaveChanges();


                            //---------------------------------



                       



                        for (int c = 0; c < objuser.Count; c++)
                        {
                            var value = Convert.ToInt32(objuser[c]);

                            var dbrow = db.UserProjects.Where(x => x.ProjectID == data.ProjectID && x.UserID == value).Select(o => o).FirstOrDefault();

                            if (dbrow != null)
                            {
                                dbrow.UserID = value;
                                db.SaveChanges();
                            }
                            else
                            {
                                DSRCManagementSystem.UserProject objh = new DSRCManagementSystem.UserProject();
                                objh.UserID = value;
                                objh.ProjectID = data.ProjectID;
                                objh.IsBillable = null;
                                objh.MemberTypeID = 1;
                                db.AddToUserProjects(objh);
                                db.SaveChanges();
                            }

                        }

                        //Mail Part


                        if (boolProjectTime)
                        {

                            if (data.ProjectType.ToString() == "Fixed Price")
                            {
                                var objProjectStatus = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Schedule Change Fixed Price")
                                                        join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                        select new DSRCManagementSystem.Models.Email
                                                        {
                                                            To = p.To,
                                                            CC = p.CC,
                                                            BCC = p.BCC,
                                                            Subject = p.Subject,
                                                            Template = q.TemplatePath
                                                        }).FirstOrDefault();

                                int userId = int.Parse(Session["UserID"].ToString());
                                data.ProjectTypeID = 1;
                                data.ProjectTypeName = db.Master_ProjectTypes.FirstOrDefault(o => o.ProjectTypeID == data.ProjectTypeID).ProjectTypeName;
                                data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();


                                string TemplatePathProjectStatus;

                                var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                TemplatePathProjectStatus = Server.MapPath(objProjectStatus.Template);
                                string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                                htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectCode", data.ProjectCode);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectName", data.ProjectName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectType", data.ProjectTypeName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#CreatedBy", data.CreatedBy);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ManagedBy", UsersName);
                                //htmlProjectStatus = htmlProjectStatus.Replace("#ManagdBy", projectmodel.ManagedResources);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ServerName", ServerName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#StartDateTime", data.StartDateTime.Value.ToString("dd MMM yyyy"));
                                htmlProjectStatus = htmlProjectStatus.Replace("#EndDateTime", data.EndDateTime.Value.ToString("dd MMM yyyy"));

                                htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);

                                // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                if (ServerName != "http://win2012srv:88/")
                                {

                                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                    //MailIds.Add("boobalan.k@dsrc.co.in");
                                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                                    //MailIds.Add("ramesh.S@dsrc.co.in");
                                    //MailIds.Add("aruna.m@dsrc.co.in");
                                    //MailIds.Add("kirankumar@dsrc.co.in");
                                    //MailIds.Add("premkumaar.r@dsrc.co.in");
                                    string EmailAddress = "";

                                    foreach (string mail in MailIds)
                                    {
                                        EmailAddress += mail + ",";
                                    }

                                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                    string CCMailId = "kirankumar@dsrc.co.in ";
                                    string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                                    Task.Factory.StartNew(() =>
                                    {
                                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                        ////  var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //string[] words;

                                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                        //string pathvalue = "~/" + words[1];
                                        string pathvalue = CommonLogic.getLogoPath();
                                        DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                                        //DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, attachments.ToArray());
                                    });

                                }
                                else
                                {


                                    Task.Factory.StartNew(() =>
                                    {
                                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                        ////var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //string[] words;

                                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                        //string pathvalue = "~/" + words[1];
                                        string pathvalue = CommonLogic.getLogoPath();
                                        DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", objProjectStatus.To, objProjectStatus.CC, objProjectStatus.BCC, Server.MapPath(pathvalue));

                                        //  string html= System.IO.File.ReadAllText(TemplatePathProjectStatus);


                                        //DsrcMailSystem.MailSender.SendMail(null, "Project RAG Status", "", MailBuilder.ProjectStatus(model.ProjectName, model.RAGStatusComments), "HRMS@dsrc.co.in", Email, attachments.ToArray());
                                    });
                                }
                            }
                        }
                      
                            if (data.ProjectType.ToString() == "Fixed Price")
                            {

                                ORGProjectPlan = db.MileStoneValues.Where(x => x.ProjectID == data.ProjectID).Select(o => o.MileStoneID).ToList();

                                List<string> lstMileStoneNames = new List<string>();

                                lstMileStoneNames = (from MV in db.MileStoneValues
                                                     join M in db.Master_MileStones on MV.MileStoneID equals M.MileStoneID
                                                     where (MV.ProjectID == data.ProjectID)
                                                     orderby M.MileStoneName
                                                     select M.MileStoneName).ToList();

                                string strMileStoneNames = "";

                                foreach (string MileStoneNames in lstMileStoneNames)
                                {
                                    strMileStoneNames += MileStoneNames + ",";
                                }

                                if (strMileStoneNames != "")
                                {
                                    strMileStoneNames = strMileStoneNames.Remove(strMileStoneNames.LastIndexOf(","));
                                }

                                int userId = int.Parse(Session["UserID"].ToString());
                                data.ProjectTypeID = 1;
                                data.ProjectTypeName = db.Master_ProjectTypes.FirstOrDefault(o => o.ProjectTypeID == data.ProjectTypeID).ProjectTypeName;
                                data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                                var objProjectStatus = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Project Plan Change Fixed Price")
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



                                string TemplatePathProjectStatus;

                                TemplatePathProjectStatus = Server.MapPath(objProjectStatus.Template);
                                string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                                htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectCode", data.ProjectCode);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectName", data.ProjectName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectType", data.ProjectTypeName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#CreatedBy", data.CreatedBy);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ManagedBy", UsersName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ServerName", ServerName);
                                htmlProjectStatus = htmlProjectStatus.Replace("#ProjectPlans", strMileStoneNames);
                                htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);

                                htmlProjectStatus = htmlProjectStatus.Replace("#StartDateTime", data.StartDateTime.Value.ToString("dd MMM yyyy"));
                                htmlProjectStatus = htmlProjectStatus.Replace("#EndDateTime", data.EndDateTime.Value.ToString("dd MMM yyyy"));



                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                if (ServerName != "http://win2012srv:88/")
                                {

                                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                    //MailIds.Add("boobalan.k@dsrc.co.in");
                                    //MailIds.Add("shaikhakeel@dsrc.co.in");
                                    //MailIds.Add("ramesh.S@dsrc.co.in");
                                    //MailIds.Add("aruna.m@dsrc.co.in");
                                    //MailIds.Add("kirankumar@dsrc.co.in");
                                    //MailIds.Add("parthasarathi.u@dsrc.co.in");
                                    string EmailAddress = "";

                                    foreach (string mail in MailIds)
                                    {
                                        EmailAddress += mail + ",";
                                    }

                                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                    string CCMailId = "kirankumar@dsrc.co.in ";
                                    string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                                    Task.Factory.StartNew(() =>
                                    {
                                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                        //// var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //string[] words;

                                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                        //string pathvalue = "~/" + words[1];
                                        string pathvalue = CommonLogic.getLogoPath();
                                        DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                                        //DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, attachments.ToArray());
                                    });

                                }
                                else
                                {


                                    Task.Factory.StartNew(() =>
                                    {
                                        //  var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                        //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                        //string[] words;

                                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                        //string pathvalue = "~/" + words[1];
                                        string pathvalue = CommonLogic.getLogoPath();
                                        DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", objProjectStatus.To, objProjectStatus.CC, objProjectStatus.BCC, Server.MapPath(pathvalue));
                                        //DsrcMailSystem.MailSender.SendMail(null, "Project RAG Status", "", MailBuilder.ProjectStatus(model.ProjectName, model.RAGStatusComments), "HRMS@dsrc.co.in", Email, attachments.ToArray());
                                    });
                                }
                            }
                            //}

                        
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var ResourcesList = db.UserProjects.Where(x => x.ProjectID == ID).Select(o => o.UserID).ToList();

                var Users = (from u in db.Users.Where(x => x.IsActive == true)
                             select new
                             {
                                 UserID = u.UserID,
                                 Names = u.FirstName + "" + u.LastName
                             }).ToList();
                ViewBag.Resources = new MultiSelectList(Users, "UserID", "Names", ResourcesList);
                var TecnologyList = db.TechnologyValues.Where(o => o.ProjectId == ID).Select(o => o.TecnologyId).ToList();
                ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", TecnologyList);
                var ORMList = db.ORMValues.Where(o => o.ProjectID == ID).Select(o => o.ORM_Tools_ID).ToList();
                ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools", ORMList);
                var DBList = db.DataBaseValues.Where(o => o.ProjectID == ID).Select(o => o.Database_Tools_ID).ToList();
                ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools", DBList);
                var ThirdPartyList = db.ThirdPartyValues.Where(o => o.ProjectID == ID).Select(o => o.ThirdParty_Tools_ID).ToList();
                ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools", ThirdPartyList);
                var SourceControlList = db.SourceControlValues.Where(o => o.ProjectID == ID).Select(o => o.SourceControlID).ToList();
                ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools", SourceControlList);
                ViewBag.ProjectPlan = new SelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName");
                ViewData["Phasecount"] = LoadProjectPlan().Count();
                return View(result);
            }

        }
        public ActionResult DeleteProject(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            var data = db.Projects.Where(o => o.ProjectID == Id).Select(o => o).FirstOrDefault();
            data.IsActive = false;
            data.IsDeleted = true;
            //db.SaveChanges();
            var members = db.UserProjects.Where(x => x.ProjectID == Id).Select(x => x).ToList();
            foreach (var item in members)
            {
                db.UserProjects.DeleteObject(item);
            }
            db.SaveChanges();
            return RedirectToAction("Success", "Popup");
        }
        public ActionResult ChangeRAGStatus(int ID)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                DSRCManagementSystem.Models.ProjectRAGStatus objmodel = new DSRCManagementSystem.Models.ProjectRAGStatus();
                var Comments = db.ProjectStatus.Where(x => x.ProjectID == ID).Select(o => o.StatusComments).FirstOrDefault();
                objmodel.RAGStatusComments = Comments;
                var RAGList = new List<SelectListItem>();
                var result = db.Projects.Where(o => o.ProjectID == ID)
                                    .Select(o => new ProjectRAGStatus
                                    {
                                        ProjectID = o.ProjectID,
                                        ProjectName = o.ProjectName,
                                        CurrentRAGStatus = o.RAGStatus,
                                        RAGStatusComments = o.RAGComments
                                    }).FirstOrDefault();
                RAGList.Add(new SelectListItem { Text = "Red", Value = "1" });
                RAGList.Add(new SelectListItem { Text = "Amber", Value = "2" });
                RAGList.Add(new SelectListItem { Text = "Green", Value = "3" });
                RAGList.Select(r => new SelectListItem { Selected = r.Value == result.CurrentRAGStatus.ToString() });
                result.RAG = RAGList;
                return View(result);
            }
        }
        [HttpPost]
        public ActionResult UpdateRAGStatus(ProjectRAGStatus model)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            int Userid = (int)Session["UserId"];
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            //var logo = objdb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
            //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
            ////var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
            //string[] words;

            //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

            //string pathvalue = "~/" + words[1];
            string pathvalue = CommonLogic.getLogoPath();

            var attachments = new List<string>() { Server.MapPath(pathvalue) };
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                {
                    var result = db.Projects.Where(o => o.ProjectID == model.ProjectID).FirstOrDefault();
                    result.RAGStatus = model.CurrentRAGStatus;
                    result.RAGComments = model.RAGStatusComments;
                    result.CommentsCreated = indianTime;
                    result.CommentedBy = Userid;

                    // var already = db.Projects.Where(x => x.ProjectID == model.ProjectID).Select(o => o.RAGComments).FirstOrDefault();



                    var data = new ProjectStatu
                    {
                        ProjectID = model.ProjectID,
                        StatusID = model.CurrentRAGStatus ?? 0,
                        StatusComments = model.RAGStatusComments,
                        CommentsCreated = indianTime,
                        CommnetedBy = Userid
                    };
                    db.ProjectStatus.AddObject(data);

                    db.SaveChanges();
                    Session["PaginationNumber"] = 1;
                }

                model.CommentedBy = db.Users.Where(o => o.UserID == Userid).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                var image = (model.CurrentRAGStatus == 3) ? Server.MapPath("~/Content/Template/images/Circle_Green.png") : (model.CurrentRAGStatus == 2) ? Server.MapPath("~/Content/Template/images/Circle_Orange.png") : Server.MapPath("~/Content/Template/images/Circle_Red.png");

                attachments.Add(image);

                var Email = (from u in db.Users
                             join up in db.UserProjects on u.UserID equals up.UserID
                             where (up.ProjectID == model.ProjectID && u.IsActive == true)
                             orderby up.MemberTypeID
                             select u.EmailAddress).ToList();
                string objProjectStatusTo = string.Join(",", Email);

                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Project Status").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Project Status").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var objProjectStatus = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Project Status")

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


                    string TemplatePathProjectStatus = Server.MapPath(objProjectStatus.Template);
                    string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                    htmlProjectStatus = htmlProjectStatus.Replace("#ProjectName", model.ProjectName);
                    htmlProjectStatus = htmlProjectStatus.Replace("#Comments", model.RAGStatusComments.Replace("\n", "<br>"));
                    htmlProjectStatus = htmlProjectStatus.Replace("#Commentedby", model.CommentedBy);
                    htmlProjectStatus = htmlProjectStatus.Replace("#ServerName", ServerName);
                    htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);
                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                    if (ServerName != "http://win2012srv:88/")
                    {

                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                        //MailIds.Add("boobalan.k@dsrc.co.in");
                        //MailIds.Add("shaikhakeel@dsrc.co.in");
                        //MailIds.Add("ramesh.S@dsrc.co.in");
                        //MailIds.Add("aruna.m@dsrc.co.in");
                        //MailIds.Add("kirankumar@dsrc.co.in");

                        string EmailAddress = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        string CCMailId = "kirankumar@dsrc.co.in ";
                        string BCCMailId = "virupaksha.gaddad@dsrc.co.in";

                        Task.Factory.StartNew(() =>
                        {
                            DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, attachments.ToArray());
                            //DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, attachments.ToArray());
                        });

                    }
                    else
                    {


                        Task.Factory.StartNew(() =>
                        {
                            DsrcMailSystem.MailSender.SendMail(null, objProjectStatus.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", objProjectStatusTo, objProjectStatus.CC, objProjectStatus.BCC, attachments.ToArray());
                            //DsrcMailSystem.MailSender.SendMail(null, "Project RAG Status", "", MailBuilder.ProjectStatus(model.ProjectName, model.RAGStatusComments), "HRMS@dsrc.co.in", Email, attachments.ToArray());
                        });
                    }
                }
                else
                {
                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    ExceptionHandlingController.TemplateMissing("Project Status", folder, ServerName);
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BirthdayRemainder()
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var BirthdayList = db.Users.Where(b => ((DateTime)b.DateOfBirth).Month == System.DateTime.Now.Month && ((DateTime)b.DateOfBirth).Day == System.DateTime.Today.Day).ToList();

                if (BirthdayList.Count > 0)
                {
                    foreach (var obj in BirthdayList)
                    {
                        var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Birthday Remainder").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Birthday Remainder").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((check != null) && (check != 0))
                        {
                            var objBirthdayRemainder = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Birthday Remainder")
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
                            string TemplatePathBirthdayRemainder = Server.MapPath(objBirthdayRemainder.Template);
                            string htmlBirthdayRemainder = System.IO.File.ReadAllText(TemplatePathBirthdayRemainder);
                            htmlBirthdayRemainder = htmlBirthdayRemainder.Replace("#senderFirstName", obj.FirstName);
                            htmlBirthdayRemainder = htmlBirthdayRemainder.Replace("#CompanyName", company);
                            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                            if (ServerName != "http://win2012srv:88/")
                            {

                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                                //MailIds.Add("boobalan.k@dsrc.co.in");
                                //MailIds.Add("shaikhakeel@dsrc.co.in");
                                //MailIds.Add("ramesh.S@dsrc.co.in");
                                //MailIds.Add("aruna.m@dsrc.co.in");
                                //MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                                //MailIds.Add("kirankumar@dsrc.co.in");
                                //MailIds.Add("francispaul.k.c@dsrc.co.in");

                                string EmailAddress = "";

                                foreach (string mail in MailIds)
                                {
                                    EmailAddress += mail + ",";
                                }

                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                    ////var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //string[] words;

                                    //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                    //string pathvalue = "~/" + words[1];
                                    string pathvalue = CommonLogic.getLogoPath();
                                    DsrcMailSystem.MailSender.SendMail(null, objBirthdayRemainder.Subject + " - Test Mail Please Ignore", null, htmlBirthdayRemainder + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(pathvalue));
                                });

                            }
                            else
                            {

                                Task.Factory.StartNew(() =>
                                {
                                    //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                    ////    var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                    //string[] words;

                                    //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);

                                    //string pathvalue = "~/" + words[1];
                                    string pathvalue = CommonLogic.getLogoPath();
                                    DsrcMailSystem.MailSender.SendMail(null, objBirthdayRemainder.Subject, "", htmlBirthdayRemainder, "HRMS@dsrc.co.in", obj.EmailAddress, Server.MapPath(pathvalue));
                                    //DsrcMailSystem.MailSender.SendMail(null, "DSRC HRMS-Birthday Wishes", "", MailBuilder.BirthdayRemainder(obj.FirstName), "HRMS@dsrc.co.in", obj.EmailAddress, Server.MapPath("~/Content/Template/images/logo.png"));
                                });
                            }
                        }
                        else
                        {
                            // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                            ExceptionHandlingController.TemplateMissing("Birthday Remainder", folder, ServerName);
                        }
                    }
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ProjectSummary()
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var result = db.Projects.Where(p => p.IsDeleted == false || p.IsDeleted == null && p.IsActive == true).Select(x => x).ToList();

                List<string> projectName = result.OrderBy(x => x.RAGStatus).Select(x => x.ProjectName).ToList();
                List<string> comments = result.OrderBy(x => x.RAGStatus).Select(x => x.RAGComments).ToList();
                List<int?> Pro_Status = result.OrderBy(x => x.RAGStatus).Select(x => x.RAGStatus).ToList();
                List<int?> commentedlist = result.OrderBy(x => x.RAGStatus).Select(x => x.CommentedBy).ToList();

                List<string> CommentedBy = null;

                foreach (int? id in commentedlist)
                {
                    if (id != 0 && id != null)
                        CommentedBy.Add(db.Users.Where(o => o.UserID == id).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault());
                }

                string pathvalue = CommonLogic.getLogoPath();
                var imagePath = new List<string>() { Server.MapPath("~/Content/Template/images/Circle_Red.png"), Server.MapPath("~/Content/Template/images/Circle_Orange.png"), Server.MapPath("~/Content/Template/images/Circle_Green.png"), Server.MapPath(pathvalue) };

                string MailBody = "";
                for (int i = 0; i < projectName.Count; i++)
                {
                    string pro_color = ((Pro_Status[i] == 3) ? "Pro^Img^Green" : (Pro_Status[i] == 2) ? "Pro^Img^Orange" : "Pro^Img^Red");
                    MailBody += @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"
                    + (i + 1) + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                    + projectName[i] + @"</td><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                    + pro_color + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                    + comments[i] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb; white-space: pre-wrap;'>"
                    + ((CommentedBy == null || string.IsNullOrEmpty(CommentedBy[i])) ? "" : CommentedBy[i].Replace("\n", "<br>")) + @"</td></tr>";
                }
                //string MessageBody = MailBody + Tablebody + MessageBody2;


                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Project Summary").Select(o => o.EmailTemplateID).FirstOrDefault();
                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Project Summary").Select(x => x.TemplatePath).FirstOrDefault();
                if ((check != null) && (check != 0))
                {
                    var objProjectSummary = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Project Summary")
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
                    string TemplatePathProjectSummary = Server.MapPath(objProjectSummary.Template);
                    string htmlnewjoiningOnBoarding = System.IO.File.ReadAllText(TemplatePathProjectSummary);
                    htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#MailBody", MailBody);
                    htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#ServerName", ServerName);
                    htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                    htmlnewjoiningOnBoarding = htmlnewjoiningOnBoarding.Replace("#CompanyName", company);
                    objProjectSummary.To = ProjectsController.GetUserEmailAddress(db, objProjectSummary.To);
                    objProjectSummary.CC = ProjectsController.GetUserEmailAddress(db, objProjectSummary.CC);
                    if (objProjectSummary.BCC != "")
                    {
                        objProjectSummary.BCC = ProjectsController.GetUserEmailAddress(db, objProjectSummary.BCC);
                    }

                    if (ServerName != "http://win2012srv:88/")
                    {

                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                        string EmailAddress = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddress += mail + ",";
                        }

                        EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                        string CCMailId = "kirankumar@dsrc.co.in";
                        string BCCMailId = "Virupaksha.Gaddad@dsrc.co.in ";

                        Task.Factory.StartNew(() =>
                        {
                            DsrcMailSystem.MailSender.SendMail(null, objProjectSummary.Subject + " - Test Mail Please Ignore", null, htmlnewjoiningOnBoarding + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, imagePath.ToArray());
                        });

                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            DsrcMailSystem.MailSender.SendMail(null, objProjectSummary.Subject, "", htmlnewjoiningOnBoarding, "HRMS@dsrc.co.in", objProjectSummary.To, objProjectSummary.CC, objProjectSummary.BCC, imagePath.ToArray());                            
                        });
                    }
                }
                else
                {
                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                    ExceptionHandlingController.TemplateMissing("Project Summary", folder, ServerName);
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        //private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        //{
        //    List<int> lst = new List<int>();
        //    foreach (var str in Attendee.Split(','))
        //    {
        //        lst.Add(Convert.ToInt32(str));
        //    }
        //    var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.EmailAddress).ToList();
        //    var tmp = "";
        //    int len = obj.Count; int i = 0;
        //    foreach (var str in obj)
        //    {
        //        i++;
        //        tmp += str;
        //        if (i < len)
        //        {
        //            tmp += ", ";
        //        }
        //    }
        //    return tmp;
        //}

        // [DSRCAuthorize(Roles = "Vice President, Project Manager")]
        //public ActionResult AssignReportingPerson()
        //{
        //    int Userid = (int)Session["UserId"];
        //    Reporting reporting = new Reporting();
        //    reporting.EmployeeList = GetNames();
        //    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
        //    {
        //        ViewBag.ReportingPersons = new MultiSelectList(GetReportingPersons(), "UserId", "Name");
        //        return View(reporting);
        //    }
        //}
        //[HttpPost]

        //public ActionResult AssignReportingPerson(Reporting report)
        //{
        //    Session["AssignReportingPerson"] = null;
        //    if (ModelState.IsValid)
        //    {
        //        using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
        //        {

        //            if (report.ReportingPerson != null)

        //            {
        //                var newReporting = new List<Int32?>(report.ReportingPerson);
        //                var oldReporting = db.UserReportings.Where(x => x.UserID == report.EmployeeId).Select(x => x.ReportingUserID).ToList();

        //                var toInsert = newReporting.Except(oldReporting).ToList();
        //                var toDelete = oldReporting.Except(newReporting).ToList();

        //                if (oldReporting.Count == 0)
        //                {
        //                    foreach (var item in newReporting)
        //                    {
        //                        var insertNew = new UserReporting()
        //                        {
        //                            UserID = report.EmployeeId,
        //                            ReportingUserID = Convert.ToInt32(item)
        //                        };
        //                        db.UserReportings.AddObject(insertNew);
        //                    }
        //                }
        //                else if (toInsert.Count > 0)
        //                {
        //                    foreach (var item in toInsert)
        //                    {
        //                        var insertChanged = new UserReporting()
        //                        {
        //                            UserID = report.EmployeeId,
        //                            ReportingUserID = Convert.ToInt32(item)
        //                        };
        //                        db.UserReportings.AddObject(insertChanged);
        //                    }
        //                }
        //                if (toDelete.Count > 0)
        //                {
        //                    foreach (var item in toDelete)
        //                    {
        //                        var data = db.UserReportings.Where(x => x.UserID == report.EmployeeId && x.ReportingUserID == item).FirstOrDefault();
        //                        db.UserReportings.DeleteObject(data);
        //                    }
        //                }
        //            }
        //            db.SaveChanges();
        //            Session["AssignReportingPerson"] = 1;
        //        }
        //    }
        //    report.EmployeeList = GetNames();
        //    ViewBag.ReportingPersons = new MultiSelectList(GetReportingPersons(), "UserId", "Name");
        //    return View(report);
        //}
        public ActionResult AssignedReportingPersons(int id)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var selectedValues = db.UserReportings.Where(x => x.UserID == id).Select(x => x.ReportingUserID).ToList();


                return Json(selectedValues, JsonRequestBehavior.AllowGet);
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
                                                          }).OrderBy(o => o.Name).ToList();
                return reportingPersons;
            }
        }
        private List<SelectListItem> GetNames()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                var NameList = new List<SelectListItem>();

                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    // int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                    List<DSRCEmployees> Names = (from data in db.Users
                                                 where data.IsActive == true && data.BranchId == 1 && data.UserID != userId
                                                 select new DSRCEmployees
                                                 {
                                                     Name = (data.FirstName + " " + (data.LastName ?? "")).Trim(),
                                                     UserId = data.UserID,
                                                     EmployeeId = data.EmpID
                                                 }).OrderBy(x => x.Name).ToList();
                    foreach (var item in Names)
                    {
                        NameList.Add(new SelectListItem { Text = item.Name, Value = item.UserId.ToString() });
                    }

                    NameList.Insert(0, new SelectListItem { Text = "---Select---", Value = "0" });
                }
                return NameList;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

        public ActionResult CommentsSummary(int projectID , string v)

        {
            ViewBag.val3 = v;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            {
                //var obj = new RAGHistory
                //{
                //    RAGHistoryList = db.ProjectStatus.Where(x => x.ProjectID == projectID).OrderByDescending(x => x.CommentsCreated).ToList()
                //};

                var obj = (from ps in db.ProjectStatus
                           join u in db.Users on ps.CommnetedBy equals u.UserID into usrleft
                           from usr in usrleft.DefaultIfEmpty()
                           where ps.ProjectID == projectID
                           select new RAGHistory
                           {
                               ProjectID = ps.ProjectID,
                               StatusID = ps.StatusID,
                               CommentsCreated = ps.CommentsCreated,
                               StatusComments = ps.StatusComments,
                               CommnetedBy = (int)(ps.CommnetedBy ?? 0),
                               Commented = usr.FirstName + " " + (usr.LastName ?? "")
                           }).OrderByDescending(x => x.CommentsCreated).ToList();

                return View(obj);
            }
        }

        [HttpGet]
        public ActionResult Barprog(int ID)
        {
            var objdb = new DSRCManagementSystemEntities1();
            var obj = new List<DSRCManagementSystem.Models.MileStone>();

            obj = (from p in objdb.MileStoneValues.Where(x => x.ProjectID == ID)
                   join t in objdb.Projects on p.ProjectID equals t.ProjectID
                   join m in objdb.Master_MileStones on p.MileStoneID equals m.MileStoneID
                   select new DSRCManagementSystem.Models.MileStone
                   {
                       ProjectID = t.ProjectID,
                       MileStoneID = p.MileStoneID,
                       MileStoneValue = m.MileStoneName,
                       ProjectStartDate = t.ProjectStartDate,
                       ProjectEndDate = t.ProjectEndDate,
                       PhaseStartDate = p.StartDate,
                       PhaseEndDate = p.EndDate,
                       ActualEndDate = p.ActualEndDate
                   }).Distinct().ToList();


            foreach (var item in obj)
            {
                if (item.PhaseStartDate != null && item.ProjectEndDate != null)
                {
                    DateTime PhaseStartDate = Convert.ToDateTime(item.PhaseStartDate);
                    DateTime PhaseEndDate = Convert.ToDateTime(item.PhaseEndDate);
                    TimeSpan NoofDays = PhaseEndDate - PhaseStartDate;

                    ViewBag.Numberofdays = item.Numberofdays = Convert.ToInt32(NoofDays.TotalDays);

                    if (item.ActualEndDate != null)
                    {
                        DateTime ActualEndDate = Convert.ToDateTime(item.ActualEndDate);
                        TimeSpan ActualNoofDays = ActualEndDate - PhaseStartDate;

                        item.ActualNumberofdays = Convert.ToInt32(ActualNoofDays.TotalDays);
                    }
                    else
                    {
                        item.ActualNumberofdays = 0;
                    }
                }
                else
                {
                    ViewBag.Numberofdays = item.Numberofdays = 0;
                    item.ActualNumberofdays = 0;
                }

            }

            return View(obj);
        }

        [HttpGet]
        public ActionResult PhaseCompletion(int ID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var result = db.Projects.Where(o => o.ProjectID == ID)
                                    .Select(o => new Projects
                                    {
                                        ProjectID = o.ProjectID,
                                        ProjectName = o.ProjectName,
                                        ProjectCode = o.ProjectCode,
                                        DateCreated = o.DateCreated,
                                        IsActive = o.IsActive,
                                        ProjectDescription = o.ProjectDescription,
                                        ProjectType = o.Master_ProjectTypes.ProjectTypeName,
                                        SvnRepositoryUrl = o.SvnRepositoryUrl,
                                        StartDateTime = o.ProjectStartDate.Value,
                                        EndDateTime = o.ProjectEndDate.Value

                                    }).FirstOrDefault();

            //if (result.IsActive == false)
            //{
            //    result.Isactiveorwhat = true;
            //    TempData["Isdelete"] = 1;
            //}

            var Milestonevalue = db.MileStoneValues.Where(x => x.ProjectID == ID).Select(x => x).ToList();
            var phases = new List<string>();

            foreach (var Milestone in Milestonevalue)
                phases.Add(db.Master_MileStones.FirstOrDefault(x => x.MileStoneID == Milestone.MileStoneID).MileStoneName.ToString() + "," + Convert.ToDateTime(Milestone.StartDate).ToString("dd-MM-yyyy") + "," + Convert.ToDateTime(Milestone.EndDate).ToString("dd-MM-yyyy"));

            result.Phases = phases;
            result.ProjectTypeLIst = db.Master_ProjectTypes.Select(o => o.ProjectTypeName).ToList<string>();
            result.ProjectTypeLIst.Remove(result.ProjectType);
            result.ProjectTypeLIst.Insert(0, result.ProjectType);
            var TecnologyList = db.TechnologyValues.Where(o => o.ProjectId == ID).Select(o => o.TecnologyId).ToList();

            var ResourcesList = db.UserProjects.Where(x => x.ProjectID == ID).Select(o => o.UserID).ToList();

            var Users = (from u in db.Users.Where(x => x.IsActive == true)
                         select new
                         {
                             UserID = u.UserID,
                             Names = u.FirstName + "" + u.LastName
                         }).ToList();

            ViewData["Resources"] = new MultiSelectList(Users, "UserID", "Names", ResourcesList);
            //  ViewBag.Resources = new SelectList(Users, "UserID", "Names", ResourcesList);
            ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", TecnologyList);
            var ORMList = db.ORMValues.Where(o => o.ProjectID == ID).Select(o => o.ORM_Tools_ID).ToList();
            ViewBag.ORM = new MultiSelectList(LoadORM(), "ID", "orm_tools", ORMList);
            var DBList = db.DataBaseValues.Where(o => o.ProjectID == ID).Select(o => o.Database_Tools_ID).ToList();
            ViewBag.DB = new MultiSelectList(LoadDB(), "ID", "database_tools", DBList);
            var ThirdPartyList = db.ThirdPartyValues.Where(o => o.ProjectID == ID).Select(o => o.ThirdParty_Tools_ID).ToList();
            ViewBag.ThirdParty = new MultiSelectList(LoadThirdParty(), "ID", "thirdparty_tools", ThirdPartyList);
            var SourceControlList = db.SourceControlValues.Where(o => o.ProjectID == ID).Select(o => o.SourceControlID).ToList();
            ViewBag.SourceControl = new MultiSelectList(LoadSourceControl(), "ID", "SourceControl_Tools", SourceControlList);
            // var ProjectPlanList = db.MileStoneValues.Where(o => o.ProjectID == ID).Select(o => o.MileStoneID).ToList();
            var ProjectNameList = (from x in db.MileStoneValues
                                   join y in db.Master_MileStones on x.MileStoneID equals y.MileStoneID
                                   where x.ProjectID == ID
                                   select y.MileStoneName
                                  ).ToList();
            ViewBag.ProjectPlan = new SelectList(LoadProjectPlan(), "MileStoneID", "MileStoneName", ProjectNameList);
            ViewData["Phasecount"] = LoadProjectPlan().Count();
            return View(result);
        }

        [HttpPost]
        public JsonResult PhaseCompletion(Projects data)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            if (data.CompletedDates != null)
            {
                int MileStoneId;
                int selectedMilestoneId = Convert.ToInt32(data.PhaseName);
                MileStoneId = db.Master_MileStones.FirstOrDefault(x => x.MileStoneID == selectedMilestoneId).MileStoneID;
                db.MileStoneValues.FirstOrDefault(x => x.ProjectID == data.ProjectID && x.MileStoneID == MileStoneId).ActualEndDate = Convert.ToDateTime(data.CompletedDates);
                db.SaveChanges();

            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PhaseDates(Projects project)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var dates = (from msv in db.MileStoneValues
                         join ms in db.Master_MileStones on msv.MileStoneID equals ms.MileStoneID
                         where (msv.ProjectID == project.ProjectID && msv.MileStoneID == project.PhaseName)
                         select new DSRCManagementSystem.Models.MileStone
                         {
                             ProjectID = msv.ProjectID,
                             MileStoneID = msv.MileStoneID,
                             MileStoneValue = ms.MileStoneName,
                             PhaseStartDate = msv.StartDate,
                             PhaseEndDate = msv.EndDate,
                             ActualEndDate = msv.ActualEndDate
                         }).FirstOrDefault();

            if (dates != null)
            {
                return Json(new { StartDate = Convert.ToDateTime(dates.PhaseStartDate).ToString("dd-MM-yyyy"), EndDate = Convert.ToDateTime(dates.PhaseEndDate).ToString("dd-MM-yyyy") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { StartDate = "failed" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult ClearTempSession()
        {
            Session["PaginationNumber"] = null;

            return Json(1, JsonRequestBehavior.AllowGet);
        }
    }

    public class TmpModel
    {
        public string Switch { get; set; }
        public string ProjectTypeDL { get; set; }
        public string Inactive { get; set; }
    }
}