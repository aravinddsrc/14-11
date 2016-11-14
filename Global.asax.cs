using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Globalization;
using DSRCManagementSystem.Models.Domain_Models;
using System.Web.UI;
using System.Web.Security;

namespace DSRCManagementSystem
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Page_Init(object sender, EventArgs e)
        {

        }
        protected void Application_Start()
        {
            //Added for improve application Performance
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            //2 lines added

            Application["test"] = "ss";
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            CultureInfo info = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.ToString());
            info.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            System.Threading.Thread.CurrentThread.CurrentCulture = info;
        }
        void Application_Error(object sender, EventArgs e)
        {
            Exception obj = Server.GetLastError().GetBaseException();
            
            var code = (obj is HttpException) ? (obj as HttpException).GetHttpCode() : 500;

            if(obj is SqlException)
                Response.Redirect("~/user/sqlConnectivityError");

            else if (System.Web.HttpContext.Current.Session == null)
            {
                Response.Redirect("~/user/login");
            }
            else if (System.Web.HttpContext.Current.Session["UserID"] == null)
            {
                Session.Clear();
                Response.Redirect("~/User/SessionExpired");
            }
            else if (code == 404)
            {
                Response.Redirect("~/Exception/Error404?ErrorCode=" + code + "&ErrorMessage=" + obj.Message);
            }
            else
            {
                try
                {
                    string PageName = Request.RawUrl;
                    string MethodName = (obj.InnerException != null) ? (obj.InnerException.TargetSite.Name == null ? "" : obj.InnerException.TargetSite.Name) : (obj.TargetSite.Name == null ? "" : obj.TargetSite.Name);
                    string ExceptionMessage = (obj.InnerException != null) ? obj.InnerException.Message : obj.Message;
                    string Source = (obj.InnerException != null) ? obj.InnerException.Source : obj.Source;
                    string StackTrace = (obj.InnerException != null) ? obj.InnerException.StackTrace : obj.StackTrace;
                    long UserID = Convert.ToInt64(System.Web.HttpContext.Current.Session["UserID"]);
                    //DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                    //var dataobj = db.ExceptionLogs.CreateObject();
                    //dataobj.UserID = UserID;
                    //dataobj.MethodName = MethodName;
                    //dataobj.ExceptionDate = DateTime.Now;
                    //dataobj.ExceptionMessage = ExceptionMessage;
                    //dataobj.Source = Source;
                    //dataobj.StackTrace = StackTrace;
                    //db.ExceptionLogs.AddObject(dataobj);
                    //db.SaveChanges();
                }
                catch
                {

                }
                Response.Redirect("~/Exception/Error404?ErrorCode=" + 500 + "&ErrorMessage= Something wrong");
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {


            if (Context.Session != null && Session.IsNewSession && this.Request.IsAuthenticated)
            {
                if (Application["test"].ToString() == "end")
                {
                    Response.Redirect("~/user/SessionExpired");

                }
                else
                {
                    Response.Redirect("~/user/login");
                }
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {


        }
    }
}