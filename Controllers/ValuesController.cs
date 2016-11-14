using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class ValuesController : Controller
    {
        //
        // GET: /Values/

        public string Get()
        {

            return Guid.NewGuid().ToString();
        }

    }
}
