using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using MvcPaging;

//using System.Data.SqlClient;
//using System.Data.Entity.Infrastructure;
//using PagedList.Mvc;
//using MvcPaging;


namespace DSRCManagementSystem.Controllers
{
    public class LoginStatusController : Controller
    {
        // private const int pageSize = 10;
        //
        // GET: /LoginStatus/
     
        DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
        public ActionResult LoginStatus(String Search, int? pagesize, int? page)
        {

            int pageSize1 = pagesize.HasValue ? pagesize.Value : 10;
            ViewBag.Search = Search;
            ViewBag.PageSize = pageSize1;
            int pageNumber = page.HasValue ? page.Value : 1;
            if (Search != null)
            {
                var result = (from als in objdb.Audit_LoginStatus
                              join mls in objdb.Master_LoginStatus on als.LoginStatusID equals mls.LoginStatusID
                              select new LoginStatus
                              {
                                  IPAddress = als.IPAddress,
                                  BrowserVersion = als.BrowserVersion,
                                  LogedInDate = als.LogedInDate,
                                  LoginStatuss = mls.LoginStatus
                              }).OrderByDescending(x => x.LogedInDate).Take(100).Where(als => als.LoginStatuss.Contains(Search) || als.IPAddress.Contains(Search) || als.BrowserVersion.Contains(Search) || als.IPAddress.Contains(Search) ).ToList().ToPagedList(pageNumber, (int)pageSize1);
                return View(result);
            }
            else
            {
                var result = (from als in objdb.Audit_LoginStatus
                              join mls in objdb.Master_LoginStatus on als.LoginStatusID equals mls.LoginStatusID
                              select new LoginStatus
                              {
                                  IPAddress = als.IPAddress,
                                  BrowserVersion = als.BrowserVersion,
                                  LogedInDate = als.LogedInDate,
                                  LoginStatuss = mls.LoginStatus
                              }).OrderByDescending(x => x.LogedInDate).Take(100).ToList().ToPagedList(pageNumber, (int)pageSize1);
                return View(result);
            }
        }
    }
}
