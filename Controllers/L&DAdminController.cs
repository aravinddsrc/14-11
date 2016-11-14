using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class L_DAdminController : Controller
    {
        //
        // GET: /L&DAdmin/


        public ActionResult LDAdmin()
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                DSRCManagementSystem.Models.LDAdminmodel ObjAC = new DSRCManagementSystem.Models.LDAdminmodel();
                List<DSRCManagementSystem.Models.LDAdminmodel> LDList = new List<DSRCManagementSystem.Models.LDAdminmodel>();
            }


            //var user = db.Users.FirstOrDefault(x => (x.EmailAddress ?? "").Equals(objUserLogin.UserName) && x.IsActive == true);
            //int userId = Convert.ToInt32(Session["UserID"].ToString());
            //var roleID = from c in db.UserRoles where c.UserID == userId select (int)c.RoleID;
            //int Id = roleID.FirstOrDefault();

            //Session["LDMenu"] = DSRCLogic.StoredProcedures.GetUserMenuForLD(userId, Id);

        //    LDList = (from a in db.Trainings

        //              join t in db.TrainingTypes on a.TrainingTypeId equals t.TrainingTypeId 
        //              join i in db.TrainingInstructors on a.InstructorId equals i.InstructorId
        //              join s in db.Status on a.StatusId equals s.Statusid
        //              select new DSRCManagementSystem.Models.LDHomeModel()
        //              {
        //                  TrainingId = a.TrainingId,
        //                  TrainingName = a.TrainingName,
        //                  //domain=a.
        //                  Instructor= i.InstructorName,
        //                  //Nomination=
        //                  // Status =s.StatusName
           
        //            //  }).ToList();
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

         return View();

        //    return View(LDList.ToList());
        }

      

    }
}
