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
using System.Configuration;
using System.Data.SqlClient;

namespace DSRCManagementSystem.Controllers
{
    public class SkillsSpecificationController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult SkillsSpecification()
        {
            List<DSRCManagementSystem.Models.Skills> objmodel = new List<Models.Skills>();
            List<Skills> tech = new List<Skills>();
            tech = (from a in db.SkillsTechnologies
                    where (a.IsActive == true)
                    select new Skills()
                    {
                        techId=a.SkillId,
                        techname = a.SkillName
                    }).ToList();
            foreach (var b in tech)
            {
                objmodel.Add(b);
            }
            List<Skills> Value = new List<Skills>();
            Value = (from p in db.SkillsSpecifications
                     where (p.IsActive == true)
                     select new Skills()
                     {
                         Id = p.SpecificationId,
                         Technology = p.Specification
                     }).ToList();
            foreach (var x in Value)
            {

                objmodel.Add(x);
            }
            List<Skills> Values = new List<Skills>();
            Values = (from s in db.SkillSpecificationMappings
                      join m in db.SkillsTechnologies on s.SkillId equals m.SkillId
                      join ss in db.SkillsSpecifications on s.SpecificationId equals ss.SpecificationId
                      where (s.IsActive == true && m.IsActive == true && ss.IsActive == true)
                      select new Skills()
                      {
                          LevelId = s.SkillSpecificationMappingId,
                          SkillId = s.SkillId,
                          SkillName = m.SkillName,
                          SpecId = s.SpecificationId,
                          SpecName = ss.Specification

                      }).ToList();
            foreach (var y in Values)
            {

                objmodel.Add(y);
            }


            return View(objmodel);
        }


        [HttpGet]
        public ActionResult EditSkills(int Id, string Technology)
        {
            ViewBag.Id = Id;
            ViewBag.Technology = Technology;





            return View();
        }
        [HttpPost]
        public ActionResult EditSkills(Skills Model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int Id = Model.Id;
            string Technology = Model.Technology.Trim();

            var data = db.SkillsSpecifications.Where(o => o.SpecificationId != Id && o.IsActive == true).Select(o => o.Specification);

            foreach (var check in data)
            {

                if (check == Technology)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

                if (check.ToLower() == Technology.ToLower())
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }

                if (check.ToUpper() == Technology.ToUpper())
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }

            }
            var datas = db.SkillsSpecifications.Where(o => o.SpecificationId == Id).Select(o => o).FirstOrDefault();
            {
                datas.Specification = Technology;
                db.SaveChanges();
            }

            return Json("Success1", JsonRequestBehavior.AllowGet);


        }


        [HttpGet]
        public ActionResult AddSkills()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddSkills(Skills Model)
        {

            var Id = Model.Id;
            var Technology = Model.Technology.Trim();


            var temp = db.SkillsSpecifications.Where(r => r.Specification == Technology && r.IsActive == true).Select(f => f.Specification);

            foreach (var check in temp)
            {

                if (check == Technology)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

                if (check.ToLower() == Technology.ToLower())
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }

                if (check.ToUpper() == Technology.ToUpper())
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }

            }

            {

                var Assignobj = db.SkillsSpecifications.CreateObject();
                Assignobj.Specification = Technology;
                Assignobj.IsActive = true;
                db.SkillsSpecifications.AddObject(Assignobj);
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);

        }



        [HttpGet]
        public ActionResult AddMap()
        {

            var Technology = db.SkillsTechnologies.Where(o => o.IsActive == true).Select(c => new
            {
                LevelId = c.SkillId,
                level = c.SkillName
            }).ToList();
            ViewBag.Technology = new SelectList(Technology, "LevelId", "level");

            var Specialization = db.SkillsSpecifications.Where(o => o.IsActive == true).Select(c => new
            {
                SpecificationId = c.SpecificationId,
                Specification = c.Specification
            }).ToList();
            ViewBag.Specialization = new SelectList(Specialization, "SpecificationId", "Specification");




            return View();

        }


        [HttpPost]
        public ActionResult AddMap(Skills Model)
        {

            var Id = Model.TechnologyID;
            var Specification = Model.Specialization.Split(',');
            foreach (string SID in Specification)
            {
                int checkskillid = Convert.ToInt32(SID);
                var check = db.SkillSpecificationMappings.Where(x => x.SkillId == Id && x.SpecificationId == checkskillid && x.IsActive == true).Select(o => o.SkillSpecificationMappingId).FirstOrDefault();
               
                if (check != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
                var Function = db.SkillSpecificationMappings.CreateObject();
                Function.SkillId = Id;
                Function.SpecificationId = Convert.ToInt32(SID);
                Function.IsActive = true;
                db.SkillSpecificationMappings.AddObject(Function);
                db.SaveChanges();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult Delete(string IDs)
        {
            int Id = Convert.ToInt32(IDs);
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var CheckDept = db.Skills.Where(o => o.Specialization == Id && o.Isactive == true).Select(o => o.UserId);

            foreach (int x in CheckDept)
            {
                if (x != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
            }


            {
                var data = db.SkillsSpecifications.Where(o => o.SpecificationId == Id).Select(o => o).FirstOrDefault();
                data.IsActive = false;
                db.SaveChanges();

            }
            return Json("Success", JsonRequestBehavior.AllowGet);

        }





        public ActionResult DeleteMap(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var data = db.SkillSpecificationMappings.Where(o => o.SkillSpecificationMappingId == Id).Select(o => o).FirstOrDefault();
            data.IsActive = false;
            db.SaveChanges();



            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditMap(int LevelId, int SkillId, int SpecId)
        {
            ViewBag.Id = LevelId;
            ViewBag.SpecId = SpecId;
            var Technology = db.SkillsTechnologies.Where(o => o.IsActive == true).Select(c => new
            {
                LevelId = c.SkillId,
                level = c.SkillName
            }).ToList();
            ViewBag.Technology = new SelectList(Technology, "LevelId", "level", SkillId);

            //List<int> selectedRoles = new List<int>();
            //for (int i = 0; i < objrole.Count(); i++)
            //{
            //    selectedRoles.Add(Convert.ToInt32(objrole[i].RoleID));
            //}

            //ViewBag.AlbumAccessRoles = new MultiSelectList(AlbumAccessRoles, "RoleID", "RoleDescription", selectedRoles);

            //List<int> selected = new List<int>();
            //selected.Add(SpecId);
            var Specialization = db.SkillsSpecifications.Where(o => o.IsActive == true).Select(c => new
            {
                SpecificationId = c.SpecificationId,
                Specification = c.Specification
            }).ToList();
            ViewBag.Specializations = new SelectList(Specialization, "SpecificationId", "Specification", SpecId);





            return View();
        }
        [HttpPost]
        public ActionResult EditMap(Skills Model)
        {
            var Id = Model.Id;
            var SkillId = Model.TechnologyID;
            var Specification = Model.SpecificationId;
            var SpecId = Model.SpecId;

           
          
              
                var check = db.SkillSpecificationMappings.Where(y => y.SkillSpecificationMappingId != Id &&y.SkillId==SkillId && y.SpecificationId == Specification && y.IsActive == true).Select(o => o.SkillSpecificationMappingId).FirstOrDefault();

                if (check != 0)
                {
                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }
              
         


            var x = db.SkillSpecificationMappings.Where(o => o.SkillSpecificationMappingId == Id && o.IsActive==true).FirstOrDefault();
                x.SkillId = SkillId;
                x.SpecificationId = Specification;
                x.IsActive = true;
                db.SaveChanges();

            
            return Json("Success", JsonRequestBehavior.AllowGet);


        }
        [HttpGet]
        public ActionResult AddTechnology()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddTechnology(Skills Model)
        {

            var Id = Model.techId;
            var Technology = Model.techname;


            var temp = db.SkillsTechnologies.Where(r => r.SkillId == Id && r.IsActive == true).Select(f => f.SkillName);

            foreach (var check in temp)
            {

                if (check == Technology)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }

               

            }

            {


                var Assignobj = db.SkillsTechnologies.CreateObject();
                Assignobj.SkillName = Technology;
                Assignobj.IsActive = true;
                db.SkillsTechnologies.AddObject(Assignobj);
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);

        }


        public ActionResult DeleteTechnology(int Id)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var data = db.SkillsTechnologies.Where(o => o.SkillId == Id).Select(o => o).FirstOrDefault();
            data.IsActive = false;
            db.SaveChanges();



            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult EditTechnology(int techId, string techname)
        {
            ViewBag.Id = techId;
            ViewBag.Technology = techname;

            return View();
        }

        [HttpPost]
        public ActionResult EditTechnology(Skills Model)
        {

            var Id = Model.techId;
            var Technology = Model.techname;


            var temp = db.SkillsTechnologies.Where(r => r.SkillId == Id && r.IsActive == true).Select(f => f.SkillName);

            foreach (var check in temp)
            {

                if (check == Technology)
                {

                    return Json("Warning", JsonRequestBehavior.AllowGet);
                }



            }

            var datas = db.SkillsTechnologies.Where(o => o.SkillId == Id).Select(o => o).FirstOrDefault();
            {
                datas.SkillName = Technology;
                db.SaveChanges();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);

        }


    }
}
