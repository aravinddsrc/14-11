using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Globalization;
using System.Text;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using System.Data.SqlClient;
using NPOI.SS.Util;
using System.Data.OleDb;
using System.Data.Common;
using DSRCManagementSystem.DSRCLogic;
using System.Threading.Tasks;
using System.Web.Configuration;


namespace DSRCManagementSystem.Controllers
{
    public class SkillsController : Controller
    {

        public ActionResult Skills(string ID)
        {

            int INCOMING = 0;
            if (ID == null)
            {
                INCOMING = 2;
                ViewData["Phase"] = INCOMING;
            }
            else if (ID == "")
            {
                INCOMING = 0;
            }
            else
            {
                 INCOMING = Convert.ToInt32(ID);
                 ViewData["Phase"] = INCOMING;
            }
          

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Skills obj = new Skills();

            var TechList = db.SkillsTechnologies.Where(x => x.IsActive == true).ToList();

            int userId = Convert.ToInt32(Session["UserID"]);
            var LevelList = db.Master_TrainingLevel.ToList();
            var reportingPersonId = db.UserReportings.Where(x => x.UserID == userId).Select(x => x.ReportingUserID).ToList();


            List<ReportingPerson> reportingPersons = (from u in db.Users.Where(o => o.IsActive == true)
                                                      where reportingPersonId.Contains(u.UserID)
                                                      select new ReportingPerson
                                                      {
                                                          UserID = u.UserID,
                                                          Name = (u.FirstName + " " + (u.LastName ?? "")).Trim()
                                                      }).OrderBy(o => o.Name).ToList();

            //ViewBag.details = new SelectList(reportingPersons, "UserID", "Name");
            //var yearlist = db.Master_years.OrderByDescending(x => x.year1).ToList();
            var yearlist = db.Master_years.OrderByDescending(x => x.year).ToList();
           // var speclist = db.SkillsSpecifications.Where(x=>x.IsActive==true).ToList();
            

            string[] l = new string[LevelList.Count];
            for (int j = 0; j < LevelList.Count; j++)
            {
                l[j] = LevelList[j].LevelName;
            }

            string[] t = new string[TechList.Count];

            for (int k = 0; k < TechList.Count; k++)
            {
                t[k] = TechList[k].SkillName;
            }

            string[] y = new string[yearlist.Count];
            for (int i = 0; i < yearlist.Count; i++)
            {
               // y[i] = yearlist[i].year1.ToString();
                y[i] = yearlist[i].year.ToString();
            }

            //string[] s = new string[speclist.Count];
            //for (int r = 0; r < speclist.Count; r++)
            //{
            //    s[r] = speclist[r].Specification;
            //}



            DSRCManagementSystem.Models.Skills ObjLD = new DSRCManagementSystem.Models.Skills();
            ModelState.Clear();

            skilllist sk = new skilllist();
            List<Skills> skl = new List<Skills>();
            List<Skills> userSkl = new List<Skills>();
            List<Skills> userSkl1 = new List<Skills>();
            Skills lModel = new Skills();
         

               var skillSet = db.Skills.Where(skill => skill.UserId == userId && skill.status != 1 && skill.Isactive == true).Join(db.SkillsTechnologies, skill => skill.Technology, tech => tech.SkillId, (skill, tech) => new

            {
                skillMain = skill,
                techMain = tech
            }).Join(db.Master_TrainingLevel, skillLevel => skillLevel.skillMain.Level, level => level.LevelId, (allData, level) => new
            {
                skill = allData,
                level = level
            });

           foreach (var item in skillSet)
            {
                Skills lm = new Skills();
                lm.Id = item.skill.skillMain.Id;
                lm.Technology = item.skill.techMain.SkillName;
                lm.Specification = item.skill.skillMain.SkillsSpecification.Specification;
                lm.level = item.level.LevelName;
                lm.Primary = (bool)item.skill.skillMain.Primary;
                lm.Secondary = (bool)item.skill.skillMain.Secondary;
                lm.DateAssessed = ((DateTime)item.skill.skillMain.Date).ToString("dd/MM/yyyy");
                lm.LastUsed = item.skill.skillMain.LastUsed;
                lm.experiance = item.skill.skillMain.experiance;
                lm.Status = (int)item.skill.skillMain.status;
                if (INCOMING!=0)
                {
                    if (lm.Status == INCOMING)
                    {
                        userSkl.Add(lm);
                    }
                }
                //else if (INCOMING != null && INCOMING!=0)
                //{
                //    if (lm.Status == 2)
                //    {
                //        userSkl.Add(lm);
                    
                //    }
                
                //}
                else
                {

                    userSkl.Add(lm);  

                }
            }
            ViewBag.userSkills = userSkl;

            var skillSet1 = db.Skills.Where(skill => skill.UserId == userId && skill.status == 1 && skill.Isactive == true).Join(db.SkillsTechnologies, skill => skill.Technology, tech => tech.SkillId, (skill, tech) => new
            {
                skillMain = skill,
                techMain = tech
            }).Join(db.Master_TrainingLevel, skillLevel => skillLevel.skillMain.Level, level => level.LevelId, (allData, level) => new
            {
                skill = allData,
                level = level
            });

            foreach (var item in skillSet1)
            {

                Skills lm = new Skills();
               
                  
                lm.Id = item.skill.skillMain.Id;
                lm.Technology = item.skill.techMain.SkillName;
                lm.Specification = item.skill.skillMain.SkillsSpecification.Specification;
                lm.level = item.level.LevelName;
                lm.Primary = (bool)item.skill.skillMain.Primary;
                lm.Secondary = (bool)item.skill.skillMain.Secondary;
                lm.DateAssessed = ((DateTime)item.skill.skillMain.Date).ToString("dd/MM/yyyy");
                lm.LastUsed = item.skill.skillMain.LastUsed;
                lm.experiance = item.skill.skillMain.experiance;
                userSkl1.Add(lm);
               
            }
            ViewBag.userSkills1 = userSkl1;

            List<SelectListItem> LevelList1 = new List<SelectListItem>();
            List<SelectListItem> TechList1 = new List<SelectListItem>();
            List<SelectListItem> yearlist11 = new List<SelectListItem>();
            List<SelectListItem> specList1 = new List<SelectListItem>();
            List<SelectListItem> ApproveList1 = new List<SelectListItem>();

            //foreach (var list in speclist)
            //{
            //    specList1.Add(new SelectListItem { Text = list.Specification, Value = Convert.ToString(list.SpecificationId) });
            //}
            lModel.speciIdList = specList1;

            foreach (var list in LevelList)
            {
                LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
            }
            lModel.LevelIDList = LevelList1;

            foreach (var list in TechList)
            {
                TechList1.Add(new SelectListItem { Text = list.SkillName, Value = Convert.ToString(list.SkillId) });
            }
            lModel.TechIDList = TechList1;

            foreach (var list in yearlist)
            {
                //yearlist11.Add(new SelectListItem { Text = list.year1.ToString(), Value = Convert.ToString(list.year1) });
                yearlist11.Add(new SelectListItem { Text = list.year.ToString(), Value = Convert.ToString(list.year) });
            }
            lModel.yearidlist = yearlist11;


            foreach (var list in reportingPersons)
            {
                ApproveList1.Add(new SelectListItem { Text = Convert.ToString(list.Name), Value = Convert.ToString(list.UserID) });
            }
            lModel.ApprovedList = ApproveList1;


            for (int i = 0; i < 1; i++)
            {
                Skills lm = new Skills();
                lm.SpecificationId = 0;
                lm.LevelId = 0;
                lm.Id = 0;
                lm.DateAssessed = null;
                lm.LastUsed = 0;
                lm.Primary = false;
                lm.Secondary = false;
                skl.Add(lm);
            }

            sk.skilllists = skl;
            sk.SKL = lModel;

            var SkillStatus = db.Master_SkillStatus.Where(o => o.AssessmentStatusID == 2 || o.AssessmentStatusID == 3).ToList();
            ViewBag.skillstatus = new SelectList(SkillStatus, "AssessmentStatusID", "AssessmentStatus");
            return View("Skills", sk);
        }


    
        public ActionResult Details(int skillid)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.Skills ObjLD = new DSRCManagementSystem.Models.Skills();

            var Details = (from s in db.Skills
                           join ms in db.SkillsTechnologies on s.Technology equals ms.SkillId 
                           join u in db.Users on s.ApprovedBy equals u.UserID
                           where s.Id ==skillid 

                           select new Skills()
                           {
                              ApprovedName= u.FirstName+" "+((u.LastName).Length>0? u.LastName:" "),// s.User.Firstname_lastname
                              ApprovedDate = s.ApprovedDate
                               
                           }).FirstOrDefault();
           

            return View(Details);
        }

        [HttpPost]
        public ActionResult Skills(skilllist model, FormCollection formData)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();
            int UserId = (int)Session["UserId"];

            List<string> Technology = new List<string>();
            List<string> Specialization = new List<string>();
            List<string> Lastused = new List<string>();
            List<string> Experience = new List<string>();
            int t=0;
            int? s=0;
           
            string ServerName = AppValue.GetFromMailAddress("ServerName");

            var objcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                      
            for (int i = 0; i < model.skilllists.Count; i++)
            {
               
                var LDobj = db.Skills.CreateObject();
                if (model.skilllists[i].Id != 0)
                {
                    try
                    {
                        LDobj.Technology = model.skilllists[i].Id;
                         t = model.skilllists[i].Id;
                        var Techname = db.SkillsTechnologies.Where(o => o.SkillId == t).Select(o => o.SkillName).FirstOrDefault();
                        Technology.Add(Convert.ToString(Techname));
                       
                        LDobj.Specialization = model.skilllists[i].SpecificationId;
                         s = model.skilllists[i].SpecificationId;
                        var SpecializationName = db.SkillsSpecifications.Where(o => o.SpecificationId == s).Select(o => o.Specification).FirstOrDefault();
                        Specialization.Add(Convert.ToString(SpecializationName));
                       
                        LDobj.Level = model.skilllists[i].LevelId;
                        LDobj.Date = Convert.ToDateTime(model.skilllists[i].DateAssessed);
                       LDobj.LastUsed = model.skilllists[i].yearid;
                        Lastused.Add(Convert.ToString(model.skilllists[i].yearid));
                                               
                        LDobj.UserId = Convert.ToInt32(Session["UserID"]);
                        LDobj.Primary = Convert.ToBoolean(model.skilllists[i].Primary);
                        LDobj.Secondary = Convert.ToBoolean(model.skilllists[i].Secondary);
                                               
                        LDobj.experiance = model.skilllists[i].experiance.Trim();
                        Experience.Add(Convert.ToString(model.skilllists[i].experiance));
                        LDobj.Isactive = true;
                        LDobj.status = 2;
                        LDobj.ApprovedBy = model.skilllists[i].ApprovedBy;
                        db.Skills.AddObject(LDobj);
                        db.SaveChanges();

                    }
                    catch (Exception)
                    {
                    }

                }
                TempData["Success"] = "Skill details Saved Successfully";
            }

            var LevelList = db.Master_TrainingLevel.ToList();

            var TechList = db.SkillsTechnologies.ToList();

            var yearlist = db.Master_years.ToList();
            var specList = db.SkillsSpecifications.ToList();

            skilllist sk = new skilllist();
            List<Skills> skl = new List<Skills>();
            Skills lModel = new Skills();

            List<SelectListItem> LevelList1 = new List<SelectListItem>();
            List<SelectListItem> TechList1 = new List<SelectListItem>();
            List<SelectListItem> yearlist11 = new List<SelectListItem>();
            List<SelectListItem> specList1 = new List<SelectListItem>();

            foreach (var list in specList)
            {
                specList1.Add(new SelectListItem { Text = list.Specification, Value = Convert.ToString(list.SpecificationId) });
            }
            lModel.speciIdList = specList1;

            foreach (var list in LevelList)
            {
                LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
            }
            lModel.LevelIDList = LevelList1;

            foreach (var list in TechList)
            {
                TechList1.Add(new SelectListItem { Text = list.SkillName, Value = Convert.ToString(list.SkillId) });
            }
            lModel.TechIDList = TechList1;

            foreach (var list in yearlist)
            {
                //yearlist11.Add(new SelectListItem { Text = list.year1.ToString(), Value = Convert.ToString(list.ID) });
                yearlist11.Add(new SelectListItem { Text = list.year.ToString(), Value = Convert.ToString(list.ID) });
            }
            lModel.yearidlist = yearlist11;

            for (int i = 0; i < 1; i++)
            {
                Skills lm = new Skills();
                lm.SpecificationId = 0;
                lm.LevelId = 0;
                lm.Id = 0;
                lm.DateAssessed = null;
                lm.LastUsed = 0;
                lm.Primary = false;
                lm.Secondary = false;
                skl.Add(lm);
            }

            sk.skilllists = skl;
            sk.SKL = lModel;
           
            var skillid = db.Skills.Where(o => o.UserId == UserId && o.Technology ==t  && o.Specialization == s).Select(o=>o.Id).FirstOrDefault();
            var empname = db.Users.Where(o => o.UserID == UserId).Select(o => o.FirstName + " " + ((o.LastName).Length>0? o.LastName:" ")).FirstOrDefault();
            var reporting = db.Skills.Where(o => o.UserId == UserId && o.Technology ==t  && o.Specialization == s).Select(o => o.ApprovedBy).FirstOrDefault();
            int reportingid = (int)reporting;
            var reportingname = db.Users.Where(o => o.UserID == reportingid).Select(o => o.FirstName + " " + ((o.LastName).Length > 0 ? o.LastName : " ")).FirstOrDefault();
            var reportingemail = db.Users.Where(o => o.UserID == reportingid).Select(o => o.EmailAddress).FirstOrDefault();
            var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "Skill Added").Select(o => o.EmailTemplateID).FirstOrDefault();
            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Skill Added").Select(x => x.TemplatePath).FirstOrDefault();
                     if ((checks != null) && (checks != 0))
                     {

                         var objRequest = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Skill Added")
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
                         string TemplatePath = Server.MapPath(objRequest.Template);
                         string html = System.IO.File.ReadAllText(TemplatePath);
                         string Title = " Following Skills were Added by " +" "+empname +" "+"for Approval";
                         html = html.Replace("#Title", Title);
                         html = html.Replace("#Technology",Technology[0]);
                         html = html.Replace("#Specialization",Specialization[0]);
                         html = html.Replace("#LastUsed",Lastused[0]);
                         html = html.Replace("#Experience",Experience[0]);
                         html = html.Replace("#RequestedId", Convert.ToString(skillid));
                         html=html.Replace("#ReportingPersonName",Convert.ToString(reportingname));
                         html = html.Replace("#ServerName", ServerName);
                         html = html.Replace("#CompanyName", objcompany);
                         html = html.Replace("#ReqUser", empname);


                          var logo = CommonLogic.getLogoPath();

                          if (ServerName != "http://win2012srv:88/")
                          {

                              List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();

                              string EmailAddress = "";

                              foreach (string mail in MailIds)
                              {
                                  EmailAddress += mail + ",";
                              }

                              EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                              Task.Factory.StartNew(() =>
                              {
                                  DsrcMailSystem.MailSender.SendMail(null, objRequest.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                              });
                          }
                         //else
                         //{
                         //    Task.Factory.StartNew(() =>
                         //    {

                         //        DsrcMailSystem.MailSender.SendMail(null, objRequest.Subject, null, html, "admin@dsrc.co.in", Convert.ToString(reportingemail), Server.MapPath(logo.ToString()));
                         //    });
                         //}
                     }
                     else
                     {

                         ExceptionHandlingController.TemplateMissing("Skill Added", folder, ServerName);
                     }
                        
                      
             

            return RedirectToAction("Skills", sk);
        }

        [HttpGet]
        public ActionResult EditSkill(int Id)
        {
            System.Web.HttpContext.Current.Application["Id"] = Id;
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = Convert.ToInt32(Session["UserID"]);


            var skillSet = db.Skills.Where(skill => skill.Id == Id && skill.Isactive == true).Join(db.SkillsTechnologies, skill => skill.Technology, tech => tech.SkillId, (skill, tech) => new

            {
                skillMain = skill,
                techMain = tech
            }).Join(db.Master_TrainingLevel, skillLevel => skillLevel.skillMain.Level, level => level.LevelId, (allData, level) => new
            {
                skill = allData,
                level = level
            }).ToList();

            var lm = new Skills();
            var item = skillSet.FirstOrDefault();
            if (item != null)
            {
                lm.Technology = item.skill.techMain.SkillName;
                lm.TechnologyID = item.skill.skillMain.Technology;
                lm.SpecificationId = item.skill.skillMain.SkillsSpecification.SpecificationId;
                lm.level = item.level.LevelName;
                lm.LevelId = item.level.LevelId;
                lm.Primary = (bool)item.skill.skillMain.Primary;
                lm.Secondary = (bool)item.skill.skillMain.Secondary;
                lm.DateAssessed = ((DateTime)item.skill.skillMain.Date).ToString("dd/MM/yyyy");
                lm.LastUsed = item.skill.skillMain.LastUsed;
                lm.experiance = item.skill.skillMain.experiance;
            }

            var TechnologyList = db.SkillsTechnologies.ToList();

            ViewBag.TechnologyIDList = new SelectList(TechnologyList, "SkillId", "SkillName", lm.TechnologyID);

            var levellist = db.Master_TrainingLevel.ToList();
            ViewBag.Level = new SelectList(levellist, "LevelId", "LevelName", lm.LevelId);

            var yearid = db.Master_years.Where(x => x.year == lm.LastUsed).Select(o => o.ID).FirstOrDefault();
            //var yearlist = db.Master_years.ToList().OrderBy(x => x.year1);
            var yearlist = db.Master_years.ToList().OrderBy(x => x.year);
            ViewBag.year2 = new SelectList(yearlist, "ID", "year", yearid);

            var speclist = db.SkillsSpecifications.ToList();
            ViewBag.specdrop = new SelectList(speclist, "SpecificationId", "Specification", lm.SpecificationId);

            return View(lm);
        }

        [HttpPost]
        public ActionResult EditSkill(Skills modelObj)
        {
            int Id = Convert.ToInt32(System.Web.HttpContext.Current.Application["Id"]);
            var userId = (int)Session["UserId"];

            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var obj_server = db.Skills.Where(o => o.Id == Id).Select(o => o).FirstOrDefault();
                obj_server.Technology = modelObj.Id;
                obj_server.Specialization = modelObj.SpecificationId;
                obj_server.Level = modelObj.LevelId;
                obj_server.Date = Convert.ToDateTime(modelObj.DateAssessed);
                obj_server.Primary = modelObj.Primary;
                obj_server.Secondary = modelObj.Secondary;
                obj_server.LastUsed = modelObj.LastUsed;
                obj_server.experiance = modelObj.experiance;
                obj_server.status = 2;

                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropDown(int? id)
        {
            id = (id == 0 ? null : id);
            var specializations = new List<Tuple<int?, string>>();
            if (id != null)
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {


                    List<Skills> Values = new List<Skills>();
                    Values = (from s in db.SkillSpecificationMappings
                              join ss in db.SkillsSpecifications on s.SpecificationId equals ss.SpecificationId
                              where (s.SkillId == id &&s.IsActive == true && ss.IsActive == true)
                              select new Skills()
                              {
                                  
                                  SpecId = s.SpecificationId,
                                  SpecName = ss.Specification

                              }).ToList();

                    specializations.Add(new Tuple<int?, string>(0, "--Select--"));

                    foreach (var item in Values)
                    {
                        specializations.Add(new Tuple<int?, string>(item.SpecId, item.SpecName));
                    }
                }
            }
            return Json(specializations, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Delete(int Id)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var record = db.Skills.FirstOrDefault(x => x.Id == Id);
                if (record != null)
                {
                    record.Isactive = false;
                    record.status = 4;
                    db.SaveChanges();
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SearchSkill()
        {
            List<SearchResult> search = new List<SearchResult>();
            return View(search);
        }

        [HttpPost]
        public ActionResult SearchSkill(SearchResultModel model)
        {
            List<SearchResult> search = new List<SearchResult>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                string srchString = model.SearchKey;
                search = db.GetSearchResult(srchString).ToList();
            }
            return View(search);
        }

        public ActionResult ShowDetails(string EmpID)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            var AsgnList = (from a in db.Users
                            where a.EmpID == EmpID
                            select new DSRCManagementSystem.Models.userdetails()
                            {
                                empid = a.EmpID,
                                firstname = a.FirstName,
                                lastname = a.LastName,
                                dob = a.DateOfBirth,
                                doj = a.DateOfJoin,
                                email = a.EmailAddress,
                                ip = a.IPAddress,
                                machinename = a.MachineName,
                                permanentaddress = a.PermanentAddressID

                            }).FirstOrDefault();

            return View(AsgnList);
        }

        public ActionResult SkillDetails(int UserId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


            var AsgnList = (from b in db.Skills
                            join t in db.Users on b.UserId equals t.UserID

                            join s in db.SkillsTechnologies on b.Technology equals s.SkillId

                            join ss in db.SkillsSpecifications on b.Specialization equals ss.SpecificationId
                            where b.Isactive == true && UserId == t.UserID
                            select new DSRCManagementSystem.Models.skilldetail()

                            {
                                Technology = s.SkillName,
                                Specification = ss.Specification,
                                LastUsed = b.LastUsed,
                                experiance = b.experiance

                            }).ToList();


            return View(AsgnList);
        }
        [HttpGet]
        public ActionResult EmployeeSkills(string value)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Skills obj = new Skills();
            var TechList = db.SkillsTechnologies.Where(x => x.IsActive == true).ToList();
            var LevelList = db.Master_TrainingLevel.ToList();
            var yearlist = db.Master_years.OrderByDescending(x => x.year).ToList();
            string[] l = new string[LevelList.Count];
            for (int j = 0; j < LevelList.Count; j++)
            {
                l[j] = LevelList[j].LevelName;
            }

            string[] t = new string[TechList.Count];

            for (int k = 0; k < TechList.Count; k++)
            {
                t[k] = TechList[k].SkillName;
            }

            string[] y = new string[yearlist.Count];
            for (int i = 0; i < yearlist.Count; i++)
            {
                // y[i] = yearlist[i].year1.ToString();
                y[i] = yearlist[i].year.ToString();
            }

            DSRCManagementSystem.Models.Skills ObjLD = new DSRCManagementSystem.Models.Skills();
            ModelState.Clear();

            skilllist sk = new skilllist();
            List<Skills> skl = new List<Skills>();
            List<Skills> userSkl = new List<Skills>();
            List<Skills> userSkl1 = new List<Skills>();
            Skills lModel = new Skills();

            int userId = Convert.ToInt32(Session["UserID"]);
          
            var users = (from c in db.UserReportings
                         join b in db.Users on c.UserID equals b.UserID
                         where c.ReportingUserID == userId
                         select new
                            {
                                Name = b.FirstName + " " + (b.LastName ?? " "),
                                id = c.UserID
                            }).OrderBy(o => o.Name).ToList();

            var requestStatus = (from rs in db.Master_SkillStatus
                                select new
                                    {
                                        RequestStatusId=rs.AssessmentStatusID,
                                        RequestStatus = rs.AssessmentStatus
                                    }).ToList();

            ViewBag.requestStatus = new SelectList(requestStatus, "RequestStatusId", "RequestStatus");
            ViewBag.UsersREP = new SelectList(users, "id", "Name"); 

            //if (value == null)
            //{
            List<Skills> modelObj = new List<Skills>();
            //var skillSet
            modelObj = (from s in db.Skills
                                join ms in db.SkillsTechnologies on s.Technology equals ms.SkillId
                                join tl in db.Master_TrainingLevel on s.Level equals tl.LevelId
                                join ss in db.SkillsSpecifications on s.Specialization equals ss.SpecificationId
                                join ur in db.UserReportings on s.UserId equals ur.UserID
                                join u in db.Users on s.UserId equals u.UserID
                                where ur.ReportingUserID == userId// && s.status == 2
                                select new Skills()
                                {
                                    Id = s.Id,
                                    UName = u.FirstName + " " + (u.LastName ?? ""),
                                    TechnologyID = s.Technology,
                                    Technology = ms.SkillName,
                                    SpecificationId = s.Specialization,
                                    Specification = ss.Specification,
                                    LevelId = s.Level,
                                    level = tl.LevelName,
                                    Primary = (bool)s.Primary,
                                    Secondary = (bool)s.Secondary,
                                    //  DateAssessed = (s.Date).ToString("dd/MM/yyyy"),
                                    LastUsed = s.LastUsed,
                                    experiance = s.experiance,
                                    Status=(int)s.status
                                }).ToList();

                //ViewBag.userSkills = skillSet;

                

                //modelObj = skillSet;


            //}
            //else 
            //{
            //    var skillSet = (from s in db.Skills
            //                    join ms in db.SkillsTechnologies on s.Technology equals ms.SkillId
            //                    join tl in db.Master_TrainingLevel on s.Level equals tl.LevelId
            //                    join ss in db.SkillsSpecifications on s.Specialization equals ss.SpecificationId
            //                    join ur in db.UserReportings on s.UserId equals ur.UserID
            //                    join u in db.Users on s.UserId equals u.UserID
            //                    where ur.ReportingUserID == userId && (u.FirstName + " " + (u.LastName ?? "")) == value //&& s.status == 2
            //                    select new Skills()
            //                    {
            //                        Id = s.Id,
            //                        UName = u.FirstName + " " + (u.LastName ?? ""),
            //                        TechnologyID = s.Technology,
            //                        Technology = ms.SkillName,
            //                        SpecificationId = s.Specialization,
            //                        Specification = ss.Specification,
            //                        LevelId = s.Level,
            //                        level = tl.LevelName,
            //                        Primary = (bool)s.Primary,
            //                        Secondary = (bool)s.Secondary,
            //                        //  DateAssessed = (s.Date).ToString("dd/MM/yyyy"),
            //                        LastUsed = s.LastUsed,
            //                        experiance = s.experiance,
            //                        Status = (int)s.status
            //                    }).ToList();

            //    ViewBag.userSkills = skillSet;

            //}
            List<SelectListItem> LevelList1 = new List<SelectListItem>();
            List<SelectListItem> TechList1 = new List<SelectListItem>();
            List<SelectListItem> yearlist11 = new List<SelectListItem>();
            List<SelectListItem> specList1 = new List<SelectListItem>();

            
            lModel.speciIdList = specList1;

            foreach (var list in LevelList)
            {
                LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
            }
            lModel.LevelIDList = LevelList1;

            foreach (var list in TechList)
            {
                TechList1.Add(new SelectListItem { Text = list.SkillName, Value = Convert.ToString(list.SkillId) });
            }
            lModel.TechIDList = TechList1;

            foreach (var list in yearlist)
            {                
                yearlist11.Add(new SelectListItem { Text = list.year.ToString(), Value = Convert.ToString(list.year) });
            }
            lModel.yearidlist = yearlist11;
                     

            for (int i = 0; i < 1; i++)
            {
                Skills lm = new Skills();
                lm.SpecificationId = 0;
                lm.LevelId = 0;
                lm.Id = 0;
                lm.DateAssessed = null;
                lm.LastUsed = 0;
                lm.Primary = false;
                lm.Secondary = false;
                skl.Add(lm);
            }

            sk.skilllists = skl;
            sk.SKL = lModel;



            return View(modelObj);
        }

        [HttpPost]
        public ActionResult EmployeeSkills(Skills model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            Skills obj = new Skills();
            var TechList = db.SkillsTechnologies.Where(x => x.IsActive == true).ToList();
            var LevelList = db.Master_TrainingLevel.ToList();
            var yearlist = db.Master_years.OrderByDescending(x => x.year).ToList();
            string[] l = new string[LevelList.Count];
            for (int j = 0; j < LevelList.Count; j++)
            {
                l[j] = LevelList[j].LevelName;
            }

            string[] t = new string[TechList.Count];

            for (int k = 0; k < TechList.Count; k++)
            {
                t[k] = TechList[k].SkillName;
            }

            string[] y = new string[yearlist.Count];
            for (int i = 0; i < yearlist.Count; i++)
            {
                // y[i] = yearlist[i].year1.ToString();
                y[i] = yearlist[i].year.ToString();
            }

            DSRCManagementSystem.Models.Skills ObjLD = new DSRCManagementSystem.Models.Skills();
            ModelState.Clear();

            skilllist sk = new skilllist();
            List<Skills> skl = new List<Skills>();
            List<Skills> userSkl = new List<Skills>();
            List<Skills> userSkl1 = new List<Skills>();
            Skills lModel = new Skills();

            int userId = Convert.ToInt32(Session["UserID"]);

            var users = (from c in db.UserReportings
                         join b in db.Users on c.UserID equals b.UserID
                         where c.ReportingUserID == userId
                         select new
                         {
                             Name = b.FirstName + " " + (b.LastName ?? " "),
                             id = c.UserID
                         }).OrderBy(o => o.Name).ToList();

            var requestStatus = (from rs in db.Master_SkillStatus
                                 select new
                                 {
                                     RequestStatusId = rs.AssessmentStatusID,
                                     RequestStatus = rs.AssessmentStatus
                                 }).ToList();

            ViewBag.requestStatus = new SelectList(requestStatus, "RequestStatusId", "RequestStatus",model.RequestStatusId);
            ViewBag.UsersREP = new SelectList(users, "id", "Name",model.UserId);

            List<Skills> modelObj = new List<Skills>();
            //var skillSet
            modelObj = (from s in db.Skills
                            join ms in db.SkillsTechnologies on s.Technology equals ms.SkillId
                            join tl in db.Master_TrainingLevel on s.Level equals tl.LevelId
                            join ss in db.SkillsSpecifications on s.Specialization equals ss.SpecificationId
                            join ur in db.UserReportings on s.UserId equals ur.UserID
                            join u in db.Users on s.UserId equals u.UserID  
                            where ur.ReportingUserID == userId
                            select new Skills()
                            {
                                Id = s.Id,
                                UserId = u.UserID,
                                UName = u.FirstName + " " + (u.LastName ?? ""),
                                TechnologyID = s.Technology,
                                Technology = ms.SkillName,
                                SpecificationId = s.Specialization,
                                Specification = ss.Specification,
                                LevelId = s.Level,
                                level = tl.LevelName,
                                Primary = (bool)s.Primary,
                                Secondary = (bool)s.Secondary,
                                //  DateAssessed = (s.Date).ToString("dd/MM/yyyy"),
                                LastUsed = s.LastUsed,
                                experiance = s.experiance,
                                Status = (int)s.status
                            }).ToList();

            if (model.RequestStatusId != 0 && model.UserId != 0)
                modelObj = modelObj.Where(o => o.Status == model.RequestStatusId && o.UserId == model.UserId).ToList();
            else if (model.RequestStatusId != 0)
                modelObj = modelObj.Where(o => o.Status == model.RequestStatusId).ToList();
            else if (model.UserId != 0)
                modelObj = modelObj.Where(o => o.UserId == model.UserId).ToList();

            ViewBag.userSkills = modelObj;



           

            List<SelectListItem> LevelList1 = new List<SelectListItem>();
            List<SelectListItem> TechList1 = new List<SelectListItem>();
            List<SelectListItem> yearlist11 = new List<SelectListItem>();
            List<SelectListItem> specList1 = new List<SelectListItem>();


            lModel.speciIdList = specList1;

            foreach (var list in LevelList)
            {
                LevelList1.Add(new SelectListItem { Text = list.LevelName, Value = Convert.ToString(list.LevelId) });
            }
            lModel.LevelIDList = LevelList1;

            foreach (var list in TechList)
            {
                TechList1.Add(new SelectListItem { Text = list.SkillName, Value = Convert.ToString(list.SkillId) });
            }
            lModel.TechIDList = TechList1;

            foreach (var list in yearlist)
            {
                yearlist11.Add(new SelectListItem { Text = list.year.ToString(), Value = Convert.ToString(list.year) });
            }
            lModel.yearidlist = yearlist11;


            for (int i = 0; i < 1; i++)
            {
                Skills lm = new Skills();
                lm.SpecificationId = 0;
                lm.LevelId = 0;
                lm.Id = 0;
                lm.DateAssessed = null;
                lm.LastUsed = 0;
                lm.Primary = false;
                lm.Secondary = false;
                skl.Add(lm);
            }

            sk.skilllists = skl;
            sk.SKL = lModel;



            return View(modelObj);
        }

        public ActionResult ApproveRequestStatus(int Id)
        {

            int UId = Convert.ToInt32(System.Web.HttpContext.Current.Application["Id"]);
            var userId = (int)Session["UserId"];
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var obj_server = db.Skills.Where(o => o.Id == Id).Select(o => o).FirstOrDefault();
                     ViewBag.IsApproved = obj_server.status == 1 ? true : false;
                    ViewBag.IsRejected = obj_server.status == 3 ? true : false;
                    ViewBag.IsCancelled = obj_server.status == 4 ? true : false;

                    if (ViewBag.IsApproved == false && ViewBag.IsRejected == false && ViewBag.IsCancelled == false)
                    {
                        obj_server.status = 1;
                        obj_server.ApprovedBy = userId;
                        obj_server.ApprovedDate = DateTime.Now;

                        db.SaveChanges();
                    }
                }
                return Json(new { Result = "Success" });
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult ApproveRequestEmail(int Id)
        {

            int UId = Convert.ToInt32(System.Web.HttpContext.Current.Application["Id"]);
            var userId = (int)Session["UserId"];
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var obj_server = db.Skills.Where(o => o.Id == Id).Select(o => o).FirstOrDefault();
                    
                    ViewBag.IsApproved = obj_server.status == 1 ? true : false;
                    ViewBag.IsRejected = obj_server.status == 3 ? true : false;
                    ViewBag.IsCancelled = obj_server.status == 4 ? true : false;

                    if (ViewBag.IsApproved == false && ViewBag.IsRejected == false && ViewBag.IsCancelled == false)
                    {
                        obj_server.status = 1;
                        obj_server.ApprovedBy = userId;
                        obj_server.ApprovedDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                

                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }



        public ActionResult RejectRequestStatus(int Id)
        {
            int UId = Convert.ToInt32(System.Web.HttpContext.Current.Application["Id"]);
            var userId = (int)Session["UserId"];
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var obj_server = db.Skills.Where(o => o.Id == Id).Select(o => o).FirstOrDefault();
                     ViewBag.IsApproved = obj_server.status == 1 ? true : false;
                    ViewBag.IsRejected = obj_server.status == 3 ? true : false;
                    ViewBag.IsCancelled = obj_server.status == 4 ? true : false;

                    if (ViewBag.IsApproved == false && ViewBag.IsRejected == false && ViewBag.IsCancelled == false)
                    {
                        obj_server.status = 3;
                        obj_server.ApprovedBy = userId;
                        obj_server.ApprovedDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                return Json(new { Result = "Success" });
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult RejectRequestEmail(int Id)
        {
            int UId = Convert.ToInt32(System.Web.HttpContext.Current.Application["Id"]);
            var userId = (int)Session["UserId"];
            try
            {
                using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                {
                    var obj_server = db.Skills.Where(o => o.Id == Id).Select(o => o).FirstOrDefault();
                    ViewBag.IsApproved = obj_server.status == 1 ? true : false;
                    ViewBag.IsRejected = obj_server.status == 3 ? true : false;
                    ViewBag.IsCancelled = obj_server.status == 4 ? true : false;

                    if (ViewBag.IsApproved == false && ViewBag.IsRejected == false && ViewBag.IsCancelled == false)
                    {
                        obj_server.status = 3;
                        obj_server.ApprovedBy = userId;
                        obj_server.ApprovedDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                return View();
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [HttpGet]
        public ActionResult schedule(int Specialization)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int userId = Convert.ToInt32(Session["UserID"]);
            try
            {
           
                string Message = "";

              
                //var tech = Convert.ToInt32(Technology);

                var result = db.Skills.Where(x => x.Specialization == Specialization && x.Isactive == true && x.UserId == userId).FirstOrDefault();

                if (result == null)
                {
                   
                }
                else
                {
                    Message = "available";
                }

                return Json(new { Name = Message }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                throw Ex;
            }
        }

    }
}

