using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Web.Configuration;
using System.Threading.Tasks;
using DSRCManagementSystem.DSRCLogic;

namespace DSRCManagementSystem.Controllers
{
    public class PaymentStructureController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender(); 
        public ActionResult ProjectPayment()
        {
            DateTime Today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var CurMonth = Today.Month;
            var Curyear = Today.Year;
            var startDate = new DateTime(Curyear, CurMonth, 01);
            var endDate = new DateTime(Curyear, CurMonth, 01).AddMonths(1).AddSeconds(-1);
            List<PaymentStructure> obj = new List<PaymentStructure>();
           
            obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true)
                   join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                   select new PaymentStructure
                   {
                       PaymentID = p.PaymentID,
                       CustomerName = p.CustomerName,
                       PaymentType = s.Terms,
                       Amount = (double)p.Amount,
                   }).ToList();
            return View(obj);
        }

        [HttpGet]
        public ActionResult CreateProjectPayment()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            var BranchList = db.Master_Branches.ToList();
            //var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
            var DepartmentList = communicationHelper.GetDepartments();

            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();

            ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), "BranchID", 

"BranchName", 0);
            ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, "DepartmentId", 

"DepartmentName", 0);
            ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }, "GroupID", "GroupName", 0);
            var PaymentType = (from p in db.Master_ProjectPaymentTerms
                               select new
                                  {
                                      PaymentTypeId = p.ProjectPaymentTermID,
                                      PaymentType = p.Terms
                                  }).ToList();
            ViewBag.PaymentType = new SelectList(PaymentType, "PaymentTypeId", "PaymentType");
            var TaxTypes = (from data in db.Master_ProjectPaymentTaxType
                            select new
                                {
                                    ProjectPaymentTaxTypeID = data.ProjectPaymentTaxTypeID,
                                    TaxType = data.TaxType
                                }).ToList();

            ViewBag.TaxTypes = new SelectList(TaxTypes,"ProjectPaymentTaxTypeID", "TaxType" );
            ViewData["TaxType"] = TaxTypes.Count();
            return View();
        }

        [HttpPost]
        public ActionResult CreateProjectPayment(PaymentStructure data)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                if (TempData["Values"] != null)
                {


                }


                int userId = int.Parse(Session["UserID"].ToString());
                var Payment = db.ProjectPayments.CreateObject();
               
                Payment.CustomerName = data.CustomerName;
                Payment.TermType = data.PaymentTypeID;
               // Payment.Branch = data.BranchId;
               // Payment.Department = data.DepartmentId;
               // Payment.DepartmentGroup = data.GroupId;
                if (data.PaymentTypeID == 1)
                {
                    Payment.Amount = data.OverAllAmount + (data.MonthlyAmount * 11);
                }
                else
                {
                    Payment.Amount = data.OverAllAmount;
                }
                Payment.IsActive = true;
                db.ProjectPayments.AddObject(Payment);
                db.SaveChanges();
                int PaymentID = Payment.PaymentID;
                List<int> Users = null;
                if (data.GroupId == 0)
                {
                    Users =
                        db.Users.Where(
                            x =>
                                x.IsActive == true && x.BranchId == data.BranchId && x.DepartmentId == data.DepartmentId)
                            .Select(o => o.UserID).ToList();
                }
                else if (data.GroupId != 0)
                {
                    Users =
                      db.Users.Where(
                          x =>
                              x.IsActive == true && x.BranchId == data.BranchId && x.DepartmentId == data.DepartmentId && x.DepartmentGroup==data.GroupId)
                          .Select(o => o.UserID).ToList();
                }
                if (data.PaymentTypeID == 1)
                {
                    if (data.MonthlyAdditionalAmount != null)
                    {

                        foreach (string Amount in data.MonthlyAdditionalAmount)
                        {
                            if (Amount != "   --Select--                             ,")
                            {
                                if (Amount != "   --Select--   ,")
                                {
                                    var SplittedAmount = Amount.Split(',');
                                    string AdditionalTaxName = SplittedAmount[0].Trim();
                                    int AdditionalTaxID =
                                        db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                            x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                    double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                    db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                    {
                                        PaymentID = PaymentID,
                                        AddionalTaxID = AdditionalTaxID,
                                        Amount = AdditionalAmount,
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    DateTime duedate = (DateTime) data.MonthlyDueDate.AddMonths(-1);
                        for (int i = 1; i <= 12; i++)
                        {
                            if (Users.Count() > 0)
                            {
                                foreach (var user in Users)
                                {
                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                    {
                                        PaymentID = PaymentID,
                                        DueDate = duedate.AddMonths(i),
                                        Amount = data.MonthlyAmount,
                                        PaidStatus = 2,
                                        UserId = user,
                                    });
                                }
                            }
                            else
                            {
                              
                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                    {
                                        PaymentID = PaymentID,
                                        DueDate = duedate.AddMonths(i),
                                        Amount = data.MonthlyAmount,
                                        PaidStatus = 2,
                                       
                                    });
                                

                            }
                            db.SaveChanges();
                        }
                }
                else if (data.TermAmount != null)
                {
                    foreach (string Amount in data.TermAmount)
                    {
                        var SplittedAmount = Amount.Split(',');
                        //string TermAmount = SplittedAmount[0].Trim();

                        double TermAmount = Convert.ToDouble(SplittedAmount[0]);
                        DateTime duedate = Convert.ToDateTime(SplittedAmount[1].ToString());
                        if (Users.Count() != 0)
                        {
                            foreach (var user in Users)
                            {
                                db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                {
                                    PaymentID = PaymentID,
                                    DueDate = duedate,
                                    Amount = TermAmount,
                                    PaidStatus = 2,
                                    UserId = user
                                });
                            }
                        }
                        else
                        {
                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                            {
                                PaymentID = PaymentID,
                                DueDate = duedate,
                                Amount = TermAmount,
                                PaidStatus = 2,
                                
                            });

                        }
                    }
                    db.SaveChanges();
                    foreach (string ExtraAmount in data.TermAdditionalAmount)
                    {
                        if (ExtraAmount != "   --Select--                             ,")
                        {
                            if (ExtraAmount != "   --Select--   ,")
                            {
                                var SplittedAmount = ExtraAmount.Split(',');
                                string AdditionalTaxName = SplittedAmount[0].Trim();
                                int AdditionalTaxID =
                                    db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                        x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                {
                                    PaymentID = PaymentID,
                                    AddionalTaxID = AdditionalTaxID,
                                    Amount = AdditionalAmount,
                                });
                            }
                        }
                    }
                    db.SaveChanges();

                }
                if (data.PaymentTypeID == 3)
                {
                    if (data.YearlyAdditionalAmount != null)
                    {
                        foreach (string Amount in data.YearlyAdditionalAmount)
                        {
                            if (Amount != "   --Select--                             ,")
                            {
                                if (Amount != "   --Select--   ,")
                                {
                                    var SplittedAmount = Amount.Split(',');
                                    string AdditionalTaxName = SplittedAmount[0].Trim();
                                    int AdditionalTaxID =
                                        db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                            x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                    double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                    db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                    {
                                        PaymentID = PaymentID,
                                        AddionalTaxID = AdditionalTaxID,
                                        Amount = AdditionalAmount,
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    DateTime duedate = (DateTime) data.YearlyDueDate;

                    if (Users.Count() != 0)
                    {
                        foreach (var user in Users)
                        {
                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                            {
                                PaymentID = PaymentID,
                                DueDate = duedate,
                                Amount = data.YearlyAmount,
                                PaidStatus = 2,
                                UserId = user
                            });
                        }
                    }

                    else
                    {
                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                        {
                            PaymentID = PaymentID,
                            DueDate = duedate,
                            Amount = data.YearlyAmount,
                            PaidStatus = 2
                          
                        });
                    }
                        db.SaveChanges();
                }
            


                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Create Project Payment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Create Project Payment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var paymentcreatemail = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Create Project Payment")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();
                         data.Amount = (double)db.ProjectPayments.FirstOrDefault(o => o.PaymentID == PaymentID).Amount;
                         data.PaymentType = db.Master_ProjectPaymentTerms.FirstOrDefault(o => o.ProjectPaymentTermID == data.PaymentTypeID).Terms;
                         data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                         string overallamount = data.Amount.ToString();
                         string TemplatePathProjectStatus;

                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                         TemplatePathProjectStatus = Server.MapPath(paymentcreatemail.Template);
                         string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                         htmlProjectStatus = htmlProjectStatus.Replace("#Company", company);
                         htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlProjectStatus = htmlProjectStatus.Replace("#CustomerName", data.CustomerName);
                         htmlProjectStatus = htmlProjectStatus.Replace("#PaymentType", data.PaymentType);
                         htmlProjectStatus = htmlProjectStatus.Replace("#OverAllAmount", overallamount);
                         htmlProjectStatus = htmlProjectStatus.Replace("#CreatedBy", data.CreatedBy);
                         htmlProjectStatus = htmlProjectStatus.Replace("#ServerName",ServerName);
                       //  htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);
                       //  htmlProjectStatus = htmlProjectStatus.Replace("#AssignedTo", );

                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }
                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                             string CCMailId = "virupaksha.gaddad@dsrc.co.in";
                             string BCCMailId = "Kirankumar@dsrc.co.in ";

                             Task.Factory.StartNew(() =>
                             {
                                 //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                 //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //string[] words;
                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.SendMail(null, paymentcreatemail.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + "Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                 //var words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.SendMail(null, paymentcreatemail.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", paymentcreatemail.To,

                                    paymentcreatemail.CC, paymentcreatemail.BCC, Server.MapPath(pathvalue));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Create Project Payment", folder, ServerName);
                     }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult EditProjectPayment(int PaymentID)
        {

            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            PaymentStructure obj = new PaymentStructure();
            obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID==PaymentID)
                   join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                   join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                   select new PaymentStructure
                   {
                       PaymentID = p.PaymentID,
                       CustomerName = p.CustomerName,
                       PaymentTypeID =(int)p.TermType,
                       PaymentType=s.Terms,
                       OverAllAmount = (double)p.Amount,
                       BranchId = p.Branch,
                       DepartmentId = p.Department,
                       GroupId = p.DepartmentGroup
                   }).FirstOrDefault();
            var branch = obj.BranchId;
            var depart = obj.DepartmentId;
            var group = obj.GroupId;
            var BranchList = db.Master_Branches.ToList();
            var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();

            ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), "BranchID", 

"BranchName",branch);
            ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }.Union(DepartmentList), 

"DepartmentId", "DepartmentName",depart);
            ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }.Union(GroupList), "GroupID", 

"GroupName",group);
            
                if (obj.PaymentTypeID == 1)
                {
                    ViewBag.Term = 0;
                   
                    obj.MonthlyAmount = (double)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o => o.Amount).FirstOrDefault();
                    obj.MonthlyDueDate = (DateTime)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o => o.DueDate).FirstOrDefault();
                    var MonthlyTax = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == PaymentID).Select(x => x).ToList();
                    Session["Counts"] =Convert.ToString(MonthlyTax.Count());
                    var MonthlyTaxes = new List<string>();
                    foreach (var tax in MonthlyTax)
                        MonthlyTaxes.Add(tax.AddionalTaxID.ToString() + "," + (tax.Amount).ToString());
                    obj.MonthlyAdditionalAmount = MonthlyTaxes;

                }
                else if (obj.PaymentTypeID == 2)
                {
                    ViewBag.Term = 1;
                    var due = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o=>o.DueDate).Distinct().ToList();
                    
                    var TermAmounts = new List<string>();
                    foreach (var date in due )
                    {
                        var term =db.ProjectPaymetTermDetails.Where(x=>x.PaymentID==PaymentID && x.DueDate==date).Select(o=>o).FirstOrDefault();
                            TermAmounts.Add(Convert.ToDateTime(term.DueDate).ToString("dd-MM-yyyy") + "," + (term.Amount).ToString());
                        
                    }
                  
                    obj.TermAmount = TermAmounts;

                    var TermTax = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == PaymentID).Select(x => x).ToList();
                    Session["Counts"] = Convert.ToString(TermTax.Count);
                    var TermTaxes = new List<string>();
                    foreach (var tax in TermTax)
                        TermTaxes.Add(tax.AddionalTaxID.ToString() + "," + (tax.Amount).ToString());
                    obj.TermAdditionalAmount = TermTaxes;

                }
                else if (obj.PaymentTypeID == 3)
                {
                    ViewBag.Term = 0;
                    obj.YearlyAmount = (double)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o => o.Amount).FirstOrDefault();
                    obj.YearlyDueDate = (DateTime)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o => o.DueDate).FirstOrDefault();
                    var YearlyTax = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == PaymentID).Select(x => x).ToList();
                    Session["Counts"] = Convert.ToString (YearlyTax.Count);
                    var YearlyTaxes = new List<string>();
                    foreach (var tax in YearlyTax)
                        YearlyTaxes.Add(tax.AddionalTaxID.ToString() + "," + (tax.Amount).ToString());
                    obj.YearlyAdditionalAmount = YearlyTaxes;
                }
    
            var PaymentType = (from p in db.Master_ProjectPaymentTerms
                               select new
                               {
                                   PaymentTypeId = p.ProjectPaymentTermID,
                                   PaymentType = p.Terms
                               }).ToList();
            ViewBag.PaymentType = new SelectList(PaymentType, "PaymentTypeId", "PaymentType");
            var TaxTypes = (from data in db.Master_ProjectPaymentTaxType
                            select new
                            {
                                ProjectPaymentTaxTypeID = data.ProjectPaymentTaxTypeID,
                                TaxType = data.TaxType
                            }).ToList(); 
            ViewBag.TaxTypes = TaxTypes;
            ViewBag.PaymentID = PaymentID;
            ViewData["TaxType"] = TaxTypes.Count();
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditProjectPayment(PaymentStructure data)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            var dbvalue1 = objdb.ProjectPayments.Where(x => x.PaymentID == data.PaymentID).Select(o => o).FirstOrDefault();
            var dbvalue = db.ProjectPayments.Where(x => x.PaymentID == data.PaymentID).Select(o => o).FirstOrDefault();
            var CustomerName = db.ProjectPayments.Where(x => x.PaymentID == data.PaymentID).Select(o => o.CustomerName).FirstOrDefault();
            var PaymentTypeID = db.ProjectPayments.Where(x => x.PaymentID == data.PaymentID).Select(t => t.TermType).FirstOrDefault();
            var PaymentType = db.Master_ProjectPaymentTerms.Where(x => x.ProjectPaymentTermID == PaymentTypeID).Select(r => r.Terms).FirstOrDefault();
            string CName = CustomerName.ToString();
            string PType = PaymentType.ToString();
            try
            {
                List<int> Users = null;
                if (data.GroupId == null)
                {
                    Users =
                        db.Users.Where(
                            x =>
                                x.IsActive == true && x.BranchId == data.BranchId && x.DepartmentId == data.DepartmentId)
                            .Select(o => o.UserID).ToList();
                }
                else if (data.GroupId != null)
                {
                    Users =
                      db.Users.Where(
                          x =>
                              x.IsActive == true && x.BranchId == data.BranchId && x.DepartmentId == data.DepartmentId && x.DepartmentGroup == data.GroupId)
                          .Select(o => o.UserID).ToList();
                }
                PaymentStructure obj = new PaymentStructure();
                obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID == data.PaymentID)
                       join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                       join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                       select new PaymentStructure
                       {
                           CustomerName = p.CustomerName,
                           OverAllAmount = (double)p.Amount,
                           BranchId = p.Branch,
                           DepartmentId = p.Department,
                           GroupId = p.DepartmentGroup
                       }).FirstOrDefault();
                 var taxes = (from p in db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == data.PaymentID)
                        join q in db.Master_ProjectPaymentTaxType on p.AddionalTaxID equals q.ProjectPaymentTaxTypeID
                        select new{q.TaxType,p.Amount}).ToList();
                    var taxtype = new List<string>();
                    var taxamount = new List<string>();

                    foreach (var addition in taxes)
                    {
                        taxtype.Add(addition.TaxType);
                        taxamount.Add(addition.Amount.ToString());
                    }
                    foreach (var dates in taxtype)
                    {
                        obj.TotalTaxType += dates + ",";
                    }
                    foreach (var money in taxamount)
                    {
                        obj.TotalTaxAmount += money + ",";
                    }
                if (obj.TotalTaxType  != null && obj.TotalTaxAmount != null)
                {
                    obj.TotalTaxType = obj.TotalTaxType.Remove(obj.TotalTaxType.Length - 1);
                    obj.TotalTaxAmount = obj.TotalTaxAmount.Remove(obj.TotalTaxAmount.Length - 1);
                }

                if (data.PaymentType == "Monthly")
                {
                    ViewBag.Term = 0;
                    obj.MonthlyAmount = (double)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o.Amount).FirstOrDefault();
                    obj.MonthlyDueDate = (DateTime)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => 

o.DueDate).FirstOrDefault();
                } 
                else if (data.PaymentType == "Term")
                {
                    ViewBag.Term = 1;

                    var due = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o.DueDate).Distinct().ToList();
                    var Amount = new List<string>();
                    var Duedate = new List<string>();

                    foreach (var date in due)
                    {
                        var term = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.DueDate == date).Select(o => o).FirstOrDefault

();
                        Duedate.Add(Convert.ToDateTime(term.DueDate).ToString("dd-MM-yyyy"));
                        Amount.Add((term.Amount).ToString());
                    }
                    foreach (var dates in Duedate )
                    {
                        obj.TotalTermDueDate += dates + ",";
                    }
                    foreach (var money in Amount )
                    {
                        obj.TotalTermAmount += money + ",";
                    }
                    obj.TotalTermDueDate = obj.TotalTermDueDate.Remove(obj.TotalTermDueDate.Length - 1);
                    obj.TotalTermAmount = obj.TotalTermAmount.Remove(obj.TotalTermAmount.Length - 1);
                }
                else if (data.PaymentType == "Yearly")
                {
                    ViewBag.Term = 0;
                    obj.YearlyAmount = (double)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o.Amount).FirstOrDefault();
                    obj.YearlyDueDate = (DateTime)db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => 

o.DueDate).FirstOrDefault();
                }


                UpdatePaymentStructure updatepayment = new UpdatePaymentStructure();
                {
                    if (data.PaymentType == "Monthly")
                    {
                        ViewBag.Term = 0;
                        updatepayment.CustomerName = data.CustomerName;
                        updatepayment.OverAllAmount = data.OverAllAmount;
                        updatepayment.MonthlyAmount = data.MonthlyAmount;
                        updatepayment.MonthlyDueDate = data.MonthlyDueDate;
                    }
                    if (data.PaymentType == "Yearly")
                    {
                        ViewBag.Term = 0;
                        updatepayment.CustomerName = data.CustomerName;
                        updatepayment.OverAllAmount = data.OverAllAmount;
                        updatepayment.YearlyAmount = data.YearlyAmount;
                        updatepayment.YearlyDueDate = data.YearlyDueDate;
                    }
                    if (data.PaymentType == "Term")
                    {
                        ViewBag.Term = 1;
                        updatepayment.CustomerName = data.CustomerName;
                        updatepayment.OverAllAmount = data.OverAllAmount;
                        var Amount = new List<string>();
                        var Duedate = new List<string>();
                        foreach (string money in data.TermAmount)
                        {
                            var SplittedAmount = money.Split(',');
                            double TermAmount = Convert.ToDouble(SplittedAmount[0]);
                            DateTime duedate = Convert.ToDateTime(SplittedAmount[1].ToString());
                            Duedate.Add(duedate.ToShortDateString());
                            Amount.Add(TermAmount.ToString());
                        }
                        foreach (var dates in Duedate)
                        {
                            updatepayment.TotalTermDueDate += dates + ",";
                        }
                        foreach (var money in Amount)
                        {
                            updatepayment.TotalTermAmount += money + ",";
                        }

                        updatepayment.TotalTermDueDate =
                            updatepayment.TotalTermDueDate.Remove(updatepayment.TotalTermDueDate.Length - 1);
                        updatepayment.TotalTermAmount =
                            updatepayment.TotalTermAmount.Remove(updatepayment.TotalTermAmount.Length - 1);
                    }

              

                    if (data.BranchId != null)
                    {
                        updatepayment.BranchId = (int)data.BranchId;
                    }
                    else
                    {
                        updatepayment.BranchId =dbvalue.Branch ;

                    }
                    if (data.DepartmentId != null)
                    {

                        updatepayment.DepartmentId = (int)data.DepartmentId;
                    }
                    else
                    {

                        updatepayment.DepartmentId = dbvalue.Department;
                    }

                    if (data.GroupId != null)
                    {
                        updatepayment.GroupId = (int)data.GroupId;
                    }
                    else
                    {
                        updatepayment.GroupId =dbvalue.DepartmentGroup;
                    }
                  
                    if (data.MonthlyAdditionalAmount != null || data.TermAdditionalAmount != null ||
                        data.YearlyAdditionalAmount != null)
                    {
                        if (data.MonthlyAdditionalAmount != null)
                        {
                            foreach (string Amount in data.MonthlyAdditionalAmount)
                            {
                                if (Amount != "   --Select--                             ,")
                                {
                                    if (Amount != "   --Select--   ,")
                                    {
                                        var SplittedAmount = Amount.Split(',');
                                        string AdditionalTaxName = SplittedAmount[0].Trim();
                                        double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                        updatepayment.TotalTaxAmount += AdditionalAmount + ",";
                                        updatepayment.TotalTaxType += AdditionalTaxName + ",";
                                    }
                                }
                            }
                        }
                        if (data.TermAdditionalAmount != null)
                        {
                            foreach (string ExtraAmount in data.TermAdditionalAmount)
                            {
                                if (ExtraAmount != "   --Select--                             ,")
                                {
                                    if (ExtraAmount != "   --Select--   ,")
                                    {
                                        var SplittedAmount = ExtraAmount.Split(',');
                                        string AdditionalTaxName = SplittedAmount[0].Trim();
                                        double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                        updatepayment.TotalTaxAmount += AdditionalAmount + ",";
                                        updatepayment.TotalTaxType += AdditionalTaxName + ",";
                                    }
                                }
                            }
                       }
                        if (data.YearlyAdditionalAmount != null)
                        {
                            foreach (string ExtraAmount in data.YearlyAdditionalAmount)
                            {
                                if (ExtraAmount != "   --Select--                             ,")
                                {
                                    if (ExtraAmount != "   --Select--   ,")
                                    {
                                        var SplittedAmount = ExtraAmount.Split(',');
                                        string AdditionalTaxName = SplittedAmount[0].Trim();
                                        double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                        updatepayment.TotalTaxAmount += AdditionalAmount + ",";
                                        updatepayment.TotalTaxType += AdditionalTaxName + ",";
                                    }
                                }
                            }
                        }
                        if (updatepayment.TotalTaxAmount != null && updatepayment.TotalTaxType != null)
                        {
                            updatepayment.TotalTaxAmount =
                                updatepayment.TotalTaxAmount.Remove(updatepayment.TotalTaxAmount.Length - 1);
                            updatepayment.TotalTaxType =
                                updatepayment.TotalTaxType.Remove(updatepayment.TotalTaxType.Length - 1);
                        }

                    }
               }


          
                //var Project = db.ProjectPayments.Where(x => x.PaymentID == data.PaymentID && x.IsActive == true).Select(o => o).FirstOrDefault();
                //db.ProjectPayments.DeleteObject(Project);
                //db.SaveChanges();
                var ProjectTax = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();
                foreach (var tax in ProjectTax)
                {
                    db.ProjectPaymetTaxDetails.DeleteObject(tax);
                }
                db.SaveChanges();
                //var ProjectTaxTerm = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();

                //foreach (var term in ProjectTaxTerm)
                //{
                //    db.ProjectPaymetTermDetails.DeleteObject(term);
                //}

                db.SaveChanges();
               
                var paymentTypeId = db.Master_ProjectPaymentTerms.Where(x => x.Terms == data.PaymentType).Select(o => o.ProjectPaymentTermID).FirstOrDefault();
                //var Payment = db.ProjectPayments.CreateObject();

            

               dbvalue.CustomerName = data.CustomerName;
               dbvalue.TermType = paymentTypeId;

            

                if (dbvalue1.Branch != null)
                {
                    dbvalue.Branch = dbvalue1.Branch;
                }
                else
                {
                    dbvalue.Branch =data.BranchId;

                }

                if (dbvalue1.Department != null)
                {
                    dbvalue.Department = dbvalue1.Department;
                }
                else
                {
                    dbvalue.Department = data.DepartmentId;
                }

                if (dbvalue1.DepartmentGroup != null)
                {
                    dbvalue.DepartmentGroup = dbvalue1.DepartmentGroup;
                }

                else
                {
                    dbvalue.DepartmentGroup = data.GroupId;
                }
              
               dbvalue.IsActive = true;
               
                int PaymentID = dbvalue.PaymentID;
                if (paymentTypeId == 1)
                {
                    if (data.MonthlyAdditionalAmount != null)
                    {
                        foreach (string Amount in data.MonthlyAdditionalAmount)
                        {
                            if (Amount != "   --Select--                             ,")
                            {
                                if (Amount != "   --Select--   ,")
                                {
                                    var SplittedAmount = Amount.Split(',');
                                    string AdditionalTaxName = SplittedAmount[0].Trim();
                                    int AdditionalTaxID =
                                        db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                            x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                    double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                    db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                    {
                                        PaymentID = PaymentID,
                                        AddionalTaxID = AdditionalTaxID,
                                        Amount = AdditionalAmount,
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    var ProjectTaxs = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();
                    var Userid = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.UserId != 0).Select(o => o.UserId).Distinct().ToList();
                    if (data.PaymentType == "Monthly")
                    {
                        
                        for (int i = 0; i < ProjectTaxs.Count(); i++)
                        {
                            if (ProjectTaxs.Count() == 2)
                            {
                                if (ProjectTaxs[i].Amount != 0 && ProjectTaxs[1].Amount != 0)
                                {
                                    dbvalue.Amount = (data.MonthlyAmount * 12) + ProjectTaxs[i].Amount + ProjectTaxs[1].Amount;
                                }
                                else if (ProjectTaxs[i].Amount != 0)
                                {
                                    dbvalue.Amount = (data.MonthlyAmount * 12) + ProjectTaxs[i].Amount;
                                }
                                else if (ProjectTaxs[1].Amount != 0)
                                {
                                    dbvalue.Amount = (data.MonthlyAmount * 12) + ProjectTaxs[1].Amount;
                                }
                            }
                            else if (ProjectTaxs.Count() == 1)
                            {
                                dbvalue.Amount = (data.MonthlyAmount * 12) + ProjectTaxs[i].Amount;
                            }
                           
                        }
                        if (ProjectTaxs.Count() == 0)
                        {
                            dbvalue.Amount = (data.MonthlyAmount * 12);
                        }

                        obj.OverAllAmount = (int)dbvalue.Amount;
                       
                    }
                    else
                    {
                        dbvalue.Amount = data.OverAllAmount;
                    }
                    
                    db.SaveChanges();
                    
                        for (int i = 0; i < 12; i++)
                        {
                            DateTime duedate = (DateTime)data.MonthlyDueDate.AddMonths(i);
                            var RemoveMonthTerm = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();

                            var comments = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Comments != null).Select(o => o).ToList();                         
                            var Pending = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Pending != null).Select(o => o).ToList();
                            var paidAmount = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.PaidAmount != null).Select(o => o).ToList();

                            if (Userid.Count() > 0)
                            {
                                if (i == 0)
                                {
                                    for (int k = 0; k < RemoveMonthTerm.Count(); k++)
                                    {

                                        db.ProjectPaymetTermDetails.DeleteObject(RemoveMonthTerm[k]);
                                        db.SaveChanges();
                                    }
                                }
                                foreach (var user in Userid)
                                {
                                    int k = 0;
                                    if (comments.Count() != 0 || Pending.Count() != 0 || paidAmount.Count() != 0)
                                    {
                                        for (int j = 0; j < paidAmount.Count(); j++)
                                        {
                                            if (j == 0)
                                            {
                                                k = j;
                                            }
                                            else
                                            {
                                                k = paidAmount.Count() - (paidAmount.Count()-j);
                                            }
                                           
                                            if (paidAmount[k].UserId == user && paidAmount[k].DueDate == duedate)
                                            {
                                                if (j == 0 && i == 0)
                                                {
                                                    try
                                                    {
                                                        if (comments[k].Comments != null && comments[k].DueDate == duedate && comments[k].UserId == user)
                                                        {
                                                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                            {
                                                                PaymentID = PaymentID,
                                                                DueDate = duedate,
                                                                Amount = data.MonthlyAmount,
                                                                PaidStatus = 2,
                                                                UserId = user,
                                                                Comments = comments[i].Comments,
                                                                PaidAmount = paidAmount[i].PaidAmount,
                                                                Pending = Pending[i].Pending
                                                            });
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                            {
                                                                PaymentID = PaymentID,
                                                                DueDate = duedate,
                                                                Amount = data.MonthlyAmount,
                                                                PaidStatus = 2,
                                                                UserId = user,
                                                                //Comments = comments[i].Comments,
                                                                PaidAmount = paidAmount[i].PaidAmount,
                                                                Pending = Pending[i].Pending
                                                            });
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                        {
                                                            PaymentID = PaymentID,
                                                            DueDate = duedate,
                                                            Amount = data.MonthlyAmount,
                                                            PaidStatus = 2,
                                                            UserId = user,
                                                            //Comments = comments[i].Comments,
                                                            PaidAmount = paidAmount[i].PaidAmount,
                                                            Pending = Pending[i].Pending
                                                        });
                                                        db.SaveChanges();

                                                    }
                                                }
                                            }                                            
                                            else
                                            {
                                                if (j == 0)
                                                {

                                                    try
                                                    {
                                                        int z = j + 1;
                                                        if (paidAmount[z].UserId == user && paidAmount[z].DueDate == duedate)
                                                        {
                                                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                            {
                                                                PaymentID = PaymentID,
                                                                DueDate = duedate,
                                                                Amount = data.MonthlyAmount,
                                                                PaidStatus = 2,
                                                                UserId = user,
                                                               // Comments = comments[i].Comments,
                                                                PaidAmount = paidAmount[i].PaidAmount,
                                                                Pending = Pending[i].Pending
                                                            });
                                                            db.SaveChanges();
                                                        }
                                                        else if (comments[j].Comments != null && comments[j].DueDate == duedate && comments[j].UserId == user)
                                                        {
                                                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                            {
                                                                PaymentID = PaymentID,
                                                                DueDate = duedate,
                                                                Amount = data.MonthlyAmount,
                                                                PaidStatus = 2,
                                                                UserId = user,
                                                                Comments = comments[i].Comments,
                                                                PaidAmount = paidAmount[i].PaidAmount,
                                                                Pending = Pending[i].Pending
                                                            });
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                            {
                                                                PaymentID = PaymentID,
                                                                DueDate = duedate,
                                                                Amount = data.MonthlyAmount,
                                                                PaidStatus = 2,
                                                                UserId = user,
                                                                //Comments = comments[i].Comments,
                                                                PaidAmount = paidAmount[i].PaidAmount,
                                                                Pending = Pending[i].Pending
                                                            });
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                        {
                                                            PaymentID = PaymentID,
                                                            DueDate = duedate,
                                                            Amount = data.MonthlyAmount,
                                                            PaidStatus = 2,
                                                            UserId = user,
                                                            //Comments = comments[i].Comments,
                                                            //PaidAmount = paidAmount[i].PaidAmount,
                                                            //Pending = Pending[i].Pending
                                                        });
                                                        db.SaveChanges();

                                                    }
                                                }
                                            }



                                        }
                                    }

                                    else
                                    {
                                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                        {
                                            PaymentID = PaymentID,
                                            DueDate = duedate,
                                            Amount = data.MonthlyAmount,
                                            PaidStatus = 2,
                                            UserId = user
                                           
                                        });
                                        db.SaveChanges();
                                    }
                                    
                                }
                            }

                            else
                            {
                               
                               
                               if (i == 0)
                               {
                                   for (int j = 0; j < RemoveMonthTerm.Count(); j++)
                                   {

                                       db.ProjectPaymetTermDetails.DeleteObject(RemoveMonthTerm[j]);
                                       db.SaveChanges();
                                   }
                               }

                                db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                {
                                    PaymentID = PaymentID,
                                    DueDate = duedate,
                                    Amount = data.MonthlyAmount,
                                    PaidStatus = 2
                                    //  UserId = user
                                });
                                db.SaveChanges();
                            }
                        }
                }
                else if (data.TermAmount != null)
                {

                    var Userid = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.UserId != 0).Select(o => o.UserId).Distinct().ToList();

                    

                    var RemoveTerm = db.ProjectPaymetTermDetails.Where(x=>x.PaymentID == data.PaymentID).Select(o=>o).ToList();

                    var comments = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Comments != null).Select(o => o).ToList();
                   // var commentsCount = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Comments != null).Select(o => o).Count();
                    var Pending = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Pending != null).Select(o => o).ToList();
                    var paidAmount = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.PaidAmount != null).Select(o => o).ToList();

                    var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o.DueDate).Distinct().ToList();



                    for (int i = 0; i < RemoveTerm.Count(); i++)
                    {
                        db.ProjectPaymetTermDetails.DeleteObject(RemoveTerm[i]);
                        db.SaveChanges();
                    }


                   
                    foreach (string Amount in data.TermAmount)
                    {

                        if (Userid.Count() > 0)
                        {


                            foreach (var user in Userid)
                            {
                                for (int i = 0; i < data.TermAmount.Count(); i++)
                                {

                                     var SplittedAmount = Amount.Split(',');
                      

                        double TermAmount = Convert.ToDouble(SplittedAmount[0]);
                        DateTime duedate = Convert.ToDateTime(SplittedAmount[1].ToString());
                        
                            if (comments.Count() != 0 || Pending.Count() != 0 || paidAmount.Count() != 0)
                            {
                                for (int j = 0; j < paidAmount.Count(); j++)
                                {
                                    if (paidAmount[j].UserId == user && paidAmount[j].DueDate == duedate)
                                    {
                                       
                                        if (j == 0 && i == 0 )
                                        {
                                            try
                                            {
                                                if (comments[i].Comments != null && comments[i].DueDate == duedate && comments[i].UserId == user)
                                                {
                                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                    {
                                                        PaymentID = PaymentID,
                                                        DueDate = duedate,
                                                        Amount = TermAmount,
                                                        PaidStatus = 2,
                                                        Comments = comments[i].Comments,
                                                        PaidAmount = paidAmount[i].PaidAmount,
                                                        Pending = Pending[i].Pending,
                                                        UserId = user
                                                    });
                                                }
                                                else
                                                {
                                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                    {
                                                        PaymentID = PaymentID,
                                                        DueDate = duedate,
                                                        Amount = TermAmount,
                                                        PaidStatus = 2,
                                                        //Comments = comments[i].Comments,
                                                        PaidAmount = paidAmount[i].PaidAmount,
                                                        Pending = Pending[i].Pending,
                                                        UserId = user
                                                    });
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                                db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                {
                                                    PaymentID = PaymentID,
                                                    DueDate = duedate,
                                                    Amount = TermAmount,
                                                    PaidStatus = 2,
                                                    //Comments = comments[i].Comments,
                                                    PaidAmount = paidAmount[i].PaidAmount,
                                                    Pending = Pending[i].Pending,
                                                    UserId = user
                                                });

                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (j == 0 && i == 0)
                                        {

                                            try
                                            {
                                                if (comments[i].Comments != null && comments[i].DueDate == duedate && comments[i].UserId == user)
                                                {
                                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                    {
                                                        PaymentID = PaymentID,
                                                        DueDate = duedate,
                                                        Amount = TermAmount,
                                                        PaidStatus = 2,
                                                        Comments = comments[i].Comments,
                                                       // PaidAmount = paidAmount[i].PaidAmount,
                                                        //Pending = Pending[i].Pending,
                                                        UserId = user
                                                    });
                                                }
                                                else
                                                {
                                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                    {
                                                        PaymentID = PaymentID,
                                                        DueDate = duedate,
                                                        Amount = TermAmount,
                                                        PaidStatus = 2,
                                                        //Comments = comments[i].Comments,
                                                       // PaidAmount = paidAmount[i].PaidAmount,
                                                       // Pending = Pending[i].Pending,
                                                        UserId = user
                                                    });
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                                db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                {
                                                    PaymentID = PaymentID,
                                                    DueDate = duedate,
                                                    Amount = TermAmount,
                                                    PaidStatus = 2,
                                                    //Comments = comments[i].Comments,
                                                   // PaidAmount = paidAmount[i].PaidAmount,
                                                   // Pending = Pending[i].Pending,
                                                    UserId = user
                                                });

                                            }
                                        }
                                        
                                    }

                                  



                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                    {
                                        PaymentID = PaymentID,
                                        DueDate = duedate,
                                        Amount = TermAmount,
                                        PaidStatus = 2,
                                        UserId = user
                                    });

                                }
                            }
                        
                                }
                            }

                        }


                        else
                        {
                            var SplittedAmount = Amount.Split(',');


                            double TermAmount = Convert.ToDouble(SplittedAmount[0]);
                            DateTime duedate = Convert.ToDateTime(SplittedAmount[1].ToString());

                            if (Users.Count() > 0)
                            {
                                foreach (var user in Users)
                                {
                                    
                                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                        {
                                            PaymentID = PaymentID,
                                            DueDate = duedate,
                                            Amount = TermAmount,
                                            PaidStatus = 2,
                                            UserId = user
                                        });
                                    }
                                
                            }
                            else
                            {
                                db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                {
                                    PaymentID = PaymentID,
                                    DueDate = duedate,
                                    Amount = TermAmount,
                                    PaidStatus = 2

                                });
                            }
                        }
                    }
                    db.SaveChanges();
                    if (data.TermAdditionalAmount != null)
                    {
                        foreach (string ExtraAmount in data.TermAdditionalAmount)
                        {
                            if (ExtraAmount != "   --Select--                             ,")
                            {
                                if (ExtraAmount != "   --Select--   ,")
                                {
                                    var SplittedAmount = ExtraAmount.Split(',');
                                    string AdditionalTaxName = SplittedAmount[0].Trim();
                                    int AdditionalTaxID =
                                        db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                            x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                    double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                    db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                    {
                                        PaymentID = PaymentID,
                                        AddionalTaxID = AdditionalTaxID,
                                        Amount = AdditionalAmount,
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    if (data.OverAllAmount > 0)
                    {
                        var alreadyvalue = db.ProjectPayments.Where(x => x.PaymentID == data.PaymentID).Select(o => o).FirstOrDefault();

                        if (alreadyvalue != null)
                        {
                            alreadyvalue.Amount = data.OverAllAmount;

                        }
                        db.SaveChanges();
                    }
              
                }
                if (paymentTypeId == 3)
                {
                    var ProjectTaxsYear = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();
                    var Userid = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.UserId != 0).Select(o => o.UserId).Distinct().ToList();
                    var RemoveMonthTerm = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();

                    var comments = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Comments != null).Select(o => o).ToList();
                    var Pending = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.Pending != null).Select(o => o).ToList();
                    var paidAmount = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID && x.PaidAmount != null).Select(o => o).ToList();
                    if (data.YearlyAdditionalAmount != null)
                    {

                        for (int i = 0; i < ProjectTaxsYear.Count(); i++)
                        {
                            if (ProjectTaxsYear.Count() == 2)
                            {
                                if (ProjectTaxsYear[i].Amount != 0 && ProjectTaxsYear[1].Amount != 0)
                                {
                                    dbvalue.Amount = data.OverAllAmount + ProjectTaxsYear[i].Amount + ProjectTaxsYear[1].Amount;
                                }
                                else if (ProjectTaxsYear[i].Amount != 0)
                                {
                                    dbvalue.Amount = data.OverAllAmount + ProjectTaxsYear[i].Amount;
                                }
                                else if (ProjectTaxsYear[1].Amount != 0)
                                {
                                    dbvalue.Amount = data.OverAllAmount + ProjectTaxsYear[1].Amount;
                                }
                            }
                            else if (ProjectTaxsYear.Count() == 1)
                            {
                                dbvalue.Amount = data.OverAllAmount + ProjectTaxsYear[i].Amount;
                            }

                        }
                        if (ProjectTaxsYear.Count() == 0)
                        {
                            dbvalue.Amount = data.OverAllAmount;
                        }
                        db.SaveChanges();

                        

                        foreach (string Amount in data.YearlyAdditionalAmount)
                        {
                            if (Amount != "   --Select--                             ,")
                            {
                                if (Amount != "   --Select--   ,")
                                {
                                    var SplittedAmount = Amount.Split(',');
                                    string AdditionalTaxName = SplittedAmount[0].Trim();
                                    int AdditionalTaxID =
                                        db.Master_ProjectPaymentTaxType.FirstOrDefault(
                                            x => x.TaxType.Equals(AdditionalTaxName)).ProjectPaymentTaxTypeID;
                                    double AdditionalAmount = Convert.ToDouble(SplittedAmount[1]);
                                    db.ProjectPaymetTaxDetails.AddObject(new ProjectPaymetTaxDetail
                                    {
                                        PaymentID = PaymentID,
                                        AddionalTaxID = AdditionalTaxID,
                                        Amount = AdditionalAmount,
                                    });
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    DateTime duedate = (DateTime) data.YearlyDueDate;
                      for (int k = 0; k < RemoveMonthTerm.Count(); k++)
                        {

                            db.ProjectPaymetTermDetails.DeleteObject(RemoveMonthTerm[k]);
                            db.SaveChanges();
                        }
                    

                    if (Userid.Count() > 0)
                        {
                            foreach (var user in Userid)
                             {
                            if (comments.Count() != 0 || Pending.Count() != 0 || paidAmount.Count() != 0)
                                    {
                                        for (int j = 0; j < paidAmount.Count(); j++)
                                        {
                                            if (paidAmount[j].UserId == user && paidAmount[j].DueDate == duedate)
                                            {
                                                if (j == 0)
                                                {
                                                    try
                                                    {
                                                        if (comments[0].Comments != null && comments[0].DueDate == duedate && comments[0].UserId == user)
                                                        {
                                                             db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                       Comments = comments[0].Comments,
                                                                       PaidAmount = paidAmount[0].PaidAmount,
                                                                       Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                           db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                       //Comments = comments[0].Comments,
                                                                       PaidAmount = paidAmount[0].PaidAmount,
                                                                       Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                       db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                      // Comments = comments[0].Comments,
                                                                       PaidAmount = paidAmount[0].PaidAmount,
                                                                       Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (j == 0)
                                                {

                                                    try
                                                    {
                                                        if (comments[0].Comments != null && comments[0].DueDate == duedate && comments[0].UserId == user)
                                                        {
                                                           db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                       Comments = comments[0].Comments,
                                                                       //PaidAmount = paidAmount[0].PaidAmount,
                                                                      // Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                           db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                       //Comments = comments[0].Comments,
                                                                      // PaidAmount = paidAmount[0].PaidAmount,
                                                                       //Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                       db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                                                 {
                                                                       PaymentID = PaymentID,
                                                                       DueDate = duedate,
                                                                       Amount = data.YearlyAmount,
                                                                       PaidStatus = 2,
                                                                       UserId = user,
                                                                       //Comments = comments[0].Comments,
                                                                       //PaidAmount = paidAmount[0].PaidAmount,
                                                                       //Pending = Pending[0].Pending
                                                                     });
                                                            db.SaveChanges();

                                                    }
                                                }
                                            }

                                        
                                        }
                                        
                                    }

                                    else
                                    {
                                        db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                                        {
                                            PaymentID = PaymentID,
                                            DueDate = duedate,
                                            Amount = data.YearlyAmount,
                                            PaidStatus = 2,
                                            UserId = user,
                                           
                                        });
                                        db.SaveChanges();
                                    }
                                    
                                }
                            }
                        
                        else
                        {

                            var RemoveYear = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == data.PaymentID).Select(o => o).ToList();
                            if (RemoveYear != null)
                            {
                                for (int i = 0; i < RemoveYear.Count(); i++)
                                {
                                    db.ProjectPaymetTermDetails.DeleteObject(RemoveYear[i]);
                                    db.SaveChanges();
                                }
                            }
                            db.ProjectPaymetTermDetails.AddObject(new ProjectPaymetTermDetail
                            {
                                PaymentID = PaymentID,
                                DueDate = duedate,
                                Amount = data.YearlyAmount,
                                PaidStatus = 2,
                                // UserId = user
                            });
                        }
                    }
                    db.SaveChanges();
                

                string MailBody = "";
                var updated = updatepayment;
                var saved = obj;
                List<Variance> diff = ext.DetailedCompare(updated, saved);
                int a;

                if (diff.Count > 0)
                {
                    MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Old Values</p> <br />";

                    MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "CustomerName :" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + CName + "</label></p><br/>" + "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "PaymentType :" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + PaymentType + "</label></p><br/>";

                    for (a = 0; a < diff.Count; a++)
                    {
                        if (diff[a].UserUpdateValue != null)
                        {
                            if (diff[a].UserSaveValue == null)
                                diff[a].UserSaveValue = "";
                            //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p>" + System.Environment.NewLine;

                            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff

[a].FieldName == "ContactNo" ? "MobileNumber" : diff[a].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff

[a].UserSaveValue + @"</label></p><br/>" + System.Environment.NewLine;
                        }

                    }

                    MailBody += "<br /><p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Updated Values</p>  <br />";

                    MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "CustomerName :" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + CName + "</label></p><br/>" + "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "PaymentType :" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + PaymentType + "</label></p><br/>";

                    for (a = 0; a < diff.Count; a++)
                    {
                        if (diff[a].UserUpdateValue != null)
                        {
                            //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p>" + System.Environment.NewLine;
                            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff

[a].FieldName == "ContactNo" ? "MobileNumber" : diff[a].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff

[a].UserUpdateValue.ToString() + @"</label></p><br/>" + System.Environment.NewLine;
                        }
                    }

                     var check = objdb.EmailTemplates.Where(x => x.TemplatePurpose == "Update Project Payment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= objdb.EmailTemplates.Where(o=> o.TemplatePurpose == "Update Project Payment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var projectpaymentupdate = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Update Project Payment")
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
                         data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string Templateprojectpaymentupdate = Server.MapPath(projectpaymentupdate.Template);
                         string htmlEmpDetailUpdate = System.IO.File.ReadAllText(Templateprojectpaymentupdate);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Company", company);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#PaymentID", Convert.ToString(data.PaymentID));
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#CustomerName", data.CustomerName);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", data.CreatedBy);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {
                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }
                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                             string CCMailId = "virupaksha.gaddad@dsrc.co.in";
                             string BCCMailId = "Kirankumar@dsrc.co.in ";


                             var path = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                             Session["LogoPath"] = path.AppValue;
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMailToALL(null, projectpaymentupdate.Subject + " - Test Mail Please Ignore", null,

     htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMailToALL(null, projectpaymentupdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in",

     projectpaymentupdate.To, projectpaymentupdate.CC, projectpaymentupdate.BCC, Server.MapPath(logo.AppValue.ToString()));

                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Update Project Payment", folder, ServerName);
                     }
                }


                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteProjectPayment(int PaymentID)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                int userId = int.Parse(Session["UserID"].ToString());
                PaymentStructure data = new PaymentStructure();
                var Project = db.ProjectPayments.Where(x => x.PaymentID == PaymentID && x.IsActive == true).Select(o => o).FirstOrDefault();
                data.CustomerName = Project.CustomerName;
                data.Amount = (double)Project.Amount;
                data.PaymentType = db.Master_ProjectPaymentTerms.FirstOrDefault(o => o.ProjectPaymentTermID == Project.TermType).Terms;
                data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();

                db.ProjectPayments.DeleteObject(Project);
                db.SaveChanges();
                var ProjectTax = db.ProjectPaymetTaxDetails.Where(x => x.PaymentID == PaymentID).Select(o => o).ToList();
                foreach (var tax in ProjectTax)
                {
                    db.ProjectPaymetTaxDetails.DeleteObject(tax);
                }
                db.SaveChanges();
                var ProjectTaxTerm = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == PaymentID).Select(o => o).ToList();
                foreach (var term in ProjectTaxTerm)
                {
                    db.ProjectPaymetTermDetails.DeleteObject(term);
                }
                db.SaveChanges(); 

                 var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Project Payment").Select(o => o.EmailTemplateID).FirstOrDefault(); 
                     var folder= db.EmailTemplates.Where(o=> o.TemplatePurpose == "Delete Project Payment").Select(x=> x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var paymentdeletemail = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Project Payment")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();


                         string overallamount = data.Amount.ToString();
                         string TemplatePathProjectStatus;

                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                         TemplatePathProjectStatus = Server.MapPath(paymentdeletemail.Template);
                         string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                         htmlProjectStatus = htmlProjectStatus.Replace("#Company", company);
                         htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlProjectStatus = htmlProjectStatus.Replace("#PaymentID", PaymentID.ToString());
                         htmlProjectStatus = htmlProjectStatus.Replace("#CustomerName", data.CustomerName);
                         htmlProjectStatus = htmlProjectStatus.Replace("#PaymentType", data.PaymentType);
                         htmlProjectStatus = htmlProjectStatus.Replace("#OverAllAmount", overallamount);
                         htmlProjectStatus = htmlProjectStatus.Replace("#DeletedBy", data.CreatedBy);
                         htmlProjectStatus = htmlProjectStatus.Replace("#ServerName", ServerName);
                         htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);

                         //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {

                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
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
                                 //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                 //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //string[] words;
                                 //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.SendMail(null, paymentdeletemail.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + "Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                                 //var words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                                 //string pathvalue = "~/" + words[1];
                                 string pathvalue = CommonLogic.getLogoPath();
                                 DsrcMailSystem.MailSender.SendMail(null, paymentdeletemail.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", paymentdeletemail.To,

         paymentdeletemail.CC, paymentdeletemail.BCC, Server.MapPath(pathvalue));
                             });
                         }
                     }
                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Delete Project Payment", folder, ServerName);
                     }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult AssignPaymentGroup(int paymentId)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            var obj = new PaymentStructure();
            var data = db.ProjectPayments.Where(x => x.PaymentID == paymentId && x.IsActive == true).Select(o => o).FirstOrDefault();
            obj.CustomerName = data.CustomerName;
            ViewBag.PaymentId = paymentId;
            var BranchList = db.Master_Branches.ToList();
            var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();
            if (data.Branch != null && data.Department != null && data.DepartmentGroup != null)
            {
                ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), 

"BranchID", "BranchName", data.Branch);
                ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }.Union

(DepartmentList), "DepartmentId", "DepartmentName", data.Department);
                
                ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }.Union(GroupList), "GroupID", 

"GroupName", data.DepartmentGroup);
            }
            else
            {
                ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), 

"BranchID", "BranchName", 0);
                ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, "DepartmentId", 

"DepartmentName", 0);
                ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }, "GroupID", "GroupName", 0);
            }
            return View(obj);
        }

        [HttpPost]
        public ActionResult AssignPaymentGroup(PaymentStructure data)
        {
            try
            {
                var obj = db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID == data.PaymentID).Select(o => o).FirstOrDefault();
                obj.Branch = data.BranchId;
                obj.Department = data.DepartmentId;
                obj.DepartmentGroup = data.GroupId;
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ViewPayment(string value)
        {
            var userid = Convert.ToInt32(Session["UserID"]);
            int Values = Convert.ToInt32(value);
            var detail = db.Users.Where(x => x.IsActive == true && x.UserID == userid).Select(o => o).FirstOrDefault();
            List<PaymentStructure> obj = new List<PaymentStructure>();
            if (value == null)
            {
                obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.Branch == detail.BranchId && x.Department == detail.DepartmentId && x.DepartmentGroup == detail.DepartmentGroup || x.DepartmentGroup == 0)
                       join r in db.ProjectPaymetTermDetails.Where(x => x.UserId == userid) on p.PaymentID equals r.PaymentID
                       join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                       join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                       select new PaymentStructure
                       {
                           PaymentID = p.PaymentID,
                           CustomerName = p.CustomerName,
                           UserName = detail.FirstName + " " + detail.LastName,
                           DueDate = (DateTime)r.DueDate,
                           PaymentType = s.Terms,
                           Amount = (double)r.Amount,
                           Status = t.PaymentStatus,
                           PaidAmount = r.PaidAmount,
                           PendingAmount = r.Pending,
                           GroupId = p.DepartmentGroup
                       }).OrderByDescending(x=>x.DueDate).ToList();
            }
            else if (Values == 0)
            {
                obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.Branch == detail.BranchId && x.Department == detail.DepartmentId && x.DepartmentGroup == detail.DepartmentGroup || x.DepartmentGroup == 0)
                       join r in db.ProjectPaymetTermDetails.Where(x => x.UserId == userid) on p.PaymentID equals r.PaymentID
                       join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                       join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                       select new PaymentStructure
                       {
                           PaymentID = p.PaymentID,
                           CustomerName = p.CustomerName,
                           UserName = detail.FirstName + " " + detail.LastName,
                           DueDate = (DateTime)r.DueDate,
                           PaymentType = s.Terms,
                           Amount = (double)r.Amount,
                           Status = t.PaymentStatus,
                           PaidAmount = r.PaidAmount,
                           PendingAmount = r.Pending,
                           GroupId = p.DepartmentGroup
                       }).OrderByDescending(x => x.DueDate).ToList();
            }
            else
            {
                obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.Branch == detail.BranchId && x.Department == detail.DepartmentId && x.DepartmentGroup == detail.DepartmentGroup || x.DepartmentGroup == 0 && x.PaymentID == Values)
                       join r in db.ProjectPaymetTermDetails.Where(x => x.UserId == userid) on p.PaymentID equals r.PaymentID
                       join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                       join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                       select new PaymentStructure
                       {
                           PaymentID = p.PaymentID,
                           CustomerName = p.CustomerName,
                           UserName = detail.FirstName + " " + detail.LastName,
                           DueDate = (DateTime)r.DueDate,
                           PaymentType = s.Terms,
                           Amount = (double)r.Amount,
                           Status = t.PaymentStatus,
                           PaidAmount = r.PaidAmount,
                           PendingAmount = r.Pending,
                           GroupId = p.DepartmentGroup
                       }).OrderByDescending(x => x.DueDate).ToList();
            }


            var Filter = (from lt in db.Users.Where(d => d.IsActive == true && d.BranchId == 1 && d.UserID == userid)
                          select new
                          {
                              UserID = lt.UserID,
                              UserName = lt.FirstName + " " + lt.LastName
                          }).ToList();
            var CustomerList = db.ProjectPayments.Where(d => d.IsActive == true && d.Branch == 1).ToList();
            var UserList = db.Users.Where(d => d.IsActive == true && d.BranchId == 1 && d.UserID == userid).ToList();
            if (value == null)
            {
                ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), "PaymentID", "CustomerName", 0);
            }
            else if (Values == 0)
            {
                ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), "PaymentID", "CustomerName", 0);
            }
            else
            {
                ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), "PaymentID", "CustomerName", Values);
            }
           
                ViewBag.user = new SelectList(Filter, "UserID", "UserName", 0);
            
            //var result= new List<PaymentStructure>();
            //result = obj.Where(x => x.GroupId == detail.DepartmentGroup).ToList();
            return View(obj);
        }




        //[HttpPost]
        //public ActionResult ViewPayment()
        //{
        //    var userid = Convert.ToInt32(Session["UserID"]);
        //    int Values = Convert.ToInt32(value);
        //    var detail = db.Users.Where(x => x.IsActive == true && x.UserID == userid).Select(o => o).FirstOrDefault();
        //    List<PaymentStructure> obj = new List<PaymentStructure>();
        //    if (value == null)
        //    {
        //        obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.Branch == detail.BranchId && x.Department == detail.DepartmentId && x.DepartmentGroup == detail.DepartmentGroup || x.DepartmentGroup == 0)
        //               join r in db.ProjectPaymetTermDetails.Where(x => x.UserId == userid) on p.PaymentID equals r.PaymentID
        //               join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
        //               join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
        //               select new PaymentStructure
        //               {
        //                   PaymentID = p.PaymentID,
        //                   CustomerName = p.CustomerName,
        //                   UserName = detail.FirstName + " " + detail.LastName,
        //                   DueDate = (DateTime)r.DueDate,
        //                   PaymentType = s.Terms,
        //                   Amount = (double)r.Amount,
        //                   Status = t.PaymentStatus,
        //                   PaidAmount = r.PaidAmount,
        //                   PendingAmount = r.Pending,
        //                   GroupId = p.DepartmentGroup
        //               }).ToList();
        //    }
        //    else
        //    {
        //        obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.Branch == detail.BranchId && x.Department == detail.DepartmentId && x.DepartmentGroup == detail.DepartmentGroup || x.DepartmentGroup == 0 && x.PaymentID == Values)
        //               join r in db.ProjectPaymetTermDetails.Where(x => x.UserId == userid) on p.PaymentID equals r.PaymentID
        //               join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
        //               join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
        //               select new PaymentStructure
        //               {
        //                   PaymentID = p.PaymentID,
        //                   CustomerName = p.CustomerName,
        //                   UserName = detail.FirstName + " " + detail.LastName,
        //                   DueDate = (DateTime)r.DueDate,
        //                   PaymentType = s.Terms,
        //                   Amount = (double)r.Amount,
        //                   Status = t.PaymentStatus,
        //                   PaidAmount = r.PaidAmount,
        //                   PendingAmount = r.Pending,
        //                   GroupId = p.DepartmentGroup
        //               }).ToList();
        //    }


        //    var Filter = (from lt in db.Users.Where(d => d.IsActive == true && d.BranchId == 1 && d.UserID == userid)
        //                  select new
        //                  {
        //                      UserID = lt.UserID,
        //                      UserName = lt.FirstName + " " + lt.LastName
        //                  }).ToList();
        //    var CustomerList = db.ProjectPayments.Where(d => d.IsActive == true && d.Branch == 1).ToList();
        //    var UserList = db.Users.Where(d => d.IsActive == true && d.BranchId == 1 && d.UserID == userid).ToList();
        //    ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), "PaymentID", "CustomerName", 0);
        //    ViewBag.user = new SelectList(Filter, "UserID", "UserName", 0);
        //    //var result= new List<PaymentStructure>();
        //    //result = obj.Where(x => x.GroupId == detail.DepartmentGroup).ToList();
        //    return View(obj);
        //}


        [HttpGet]
        public ActionResult EditPayment()
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            List<PaymentStructure> obj = new List<PaymentStructure>();
            obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true)
                   join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                   join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                   join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                   join u in db.Users.Where(x=>x.IsActive==true) on r.UserId equals u.UserID
                   select new PaymentStructure
                   {
                       PaymentID = p.PaymentID,
                       CustomerName=p.CustomerName,
                       UserName = u.FirstName+" "+u.LastName,
                       DueDate = (DateTime)r.DueDate,
                       PaymentType = s.Terms,
                       Amount = (double)r.Amount,
                       PaymentTermID=(int)r.PaymentTermID,
                       Status=t.PaymentStatus,
                       StatusID=(int)r.PaidStatus,
                       comments=r.Comments,
                       PaidAmount = (double)r.PaidAmount,
                       PendingAmount = (double)r.Pending
                   }).OrderByDescending(x=>x.DueDate).ToList();
            var list = (from a in db.ProjectPaymetTaxDetails
                        group a.Amount by a.PaymentID into b
                        select new
                        {
                            paymentid = b.Key,
                            additionalamount = b.Sum()
                        }).ToList();

            foreach (var a in obj)
            {
                foreach (var b in list)
                {
                    if (a.PaymentID == b.paymentid)
                    {
                        a.AdditionalAmount = (double)b.additionalamount;
                    }
                }
            }


            var Filter = (from lt in db.Users.Where(d => d.IsActive == true && d.BranchId == 1)
                          select new
                          {
                              UserID = lt.UserID,
                              UserName = lt.FirstName + " " + lt.LastName
                          }).ToList();

            var BranchList = db.Master_Branches.ToList();
            var DepartmentList = db.Departments.Where(d => d.IsActive == true && d.BranchID==1).ToList();
            var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();
            var CustomerList = db.ProjectPayments.Where(d => d.IsActive == true && d.Branch==1).ToList();
            var UserList = db.Users.Where(d => d.IsActive == true && d.BranchId == 1).ToList();
            ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 1 } }.Union(BranchList), "BranchID", "BranchName", 1);
            ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }.Union(DepartmentList), 

"DepartmentId", "DepartmentName", 0);
            ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }, "GroupID", "GroupName", 0);
            ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), 

"PaymentID", "CustomerName", 0);
            ViewBag.user = new SelectList(new[] { new User() { UserID = 0, UserName = "---Select---" } }, "UserID", "UserName", 0);
            var StatusList = (from p in db.PaymentStatus
                              select new { StatusId = p.StatusID, PaymentStatus = p.PaymentStatus }).ToList();
            ViewBag.StatusList = StatusList;
            return View(obj);
        }

        [HttpPost]
        public ActionResult EditPayment(FormCollection form)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
                int deptmnt = (form["DepartmentId"] == "") ? 0 : Convert.ToInt16(form["DepartmentId"]);
                int branch = (form["BranchID"] == "") ? 0 : Convert.ToInt16(form["BranchID"]);
                int groups = (form["GroupID"] == "") ? 0 : Convert.ToInt16(form["GroupID"]);
                var customer = (form["PaymentID"] == "") ? 0 : Convert.ToInt16(form["PaymentID"]);
                var user = (form["UserID"] == "") ? 0 : Convert.ToInt16(form["UserID"]);

                var validUser = (from p in db.ProjectPayments
                                 where (p.IsActive == true && p.PaymentID == customer)
                                 join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                                 select r.UserId).ToList();

                var userlist = (from lt in db.Users.Where(x => x.IsActive == true && validUser.Contains(x.UserID))
                                select new
                                {
                                    UserID = lt.UserID,
                                    UserName = lt.FirstName + " " + lt.LastName
                                }).ToList();


                List<PaymentStructure> obj = new List<PaymentStructure>();
                if (user != 0)
                {
                    obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID == customer)
                           join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                           join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                           join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                           join u in db.Users.Where(x => x.IsActive == true && x.UserID == user) on r.UserId equals u.UserID
                           select new PaymentStructure
                           {
                               PaymentID = p.PaymentID,
                               CustomerName = p.CustomerName,
                               UserName = u.FirstName + " " + u.LastName,
                               DueDate = (DateTime)r.DueDate,
                               PaymentType = s.Terms,
                               Amount = (double)r.Amount,
                               StatusID = (int)r.PaidStatus,
                               DepartmentId = p.Department,
                               GroupId = p.DepartmentGroup,
                               PaymentTermID = (int)r.PaymentTermID,
                               Status = t.PaymentStatus,
                               Userid = r.UserId,
                               comments = r.Comments,
                               PaidAmount = (double)r.PaidAmount,
                               PendingAmount = (double)r.Pending
                           }).OrderByDescending(x=>x.DueDate).ToList();
                }
                else
                {
                    obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID == customer)
                           join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                           join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                           join t in db.PaymentStatus on r.PaidStatus equals t.StatusID
                           join u in db.Users.Where(x => x.IsActive == true) on r.UserId equals u.UserID
                           select new PaymentStructure
                           {
                               PaymentID = p.PaymentID,
                               CustomerName = p.CustomerName,
                               UserName = u.FirstName + " " + u.LastName,
                               DueDate = (DateTime)r.DueDate,
                               PaymentType = s.Terms,
                               Amount = (double)r.Amount,
                               StatusID = (int)r.PaidStatus,
                               DepartmentId = p.Department,
                               GroupId = p.DepartmentGroup,
                               PaymentTermID = (int)r.PaymentTermID,
                               Status = t.PaymentStatus,
                               Userid = r.UserId,
                               comments = r.Comments,
                               PaidAmount = (double)r.PaidAmount,
                               PendingAmount = (double)r.Pending
                           }).OrderByDescending(x => x.DueDate).ToList();
                }
                var BranchList = db.Master_Branches.ToList();
                var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
                var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();
                var CustomerList = db.ProjectPayments.Where(d => d.IsActive == true).ToList();
                var UserList = db.Users.Where(d => d.IsActive == true).ToList();


                

            var result = new List<PaymentStructure>();
            if (branch != 0 && customer == 0 && user == 0)
            {
                result = obj;
            }
            if (branch != 0 && customer != 0 && user == 0)
            {
                result = obj.Where(x=>x.PaymentID==customer).ToList();
            }
            if (branch != 0 && customer == 0 && user != 0)
            {
                result = obj.Where(x => x.Userid == user).ToList();
            }
            if (branch != 0 && customer != 0 && user != 0)
            {
                result = obj.Where(x => x.Userid == user && x.PaymentID==customer).ToList();
            }
            if (branch != 0 && deptmnt != 0 && groups == 0)
            {
                result = obj.Where(x => x.DepartmentId == deptmnt).ToList();
            }
            if (branch != 0 && deptmnt != 0 && groups != 0)
            {
                result = obj.Where(x => x.DepartmentId == deptmnt && x.GroupId==groups).ToList();


            }


            var Uservalues = (from p in db.Users.Where(x => x.IsActive == true)
                              select new
                              {
                                  UserID = p.UserID,
                                  UserName = p.FirstName + "" + p.LastName

                              }).ToList();


             //   ViewBag.user = new SelectList(new[] { new User() { UserID = 0, UserName = "---Select---" } }.Union(UserList), "UserID", "UserName", user);
            ViewBag.user = new SelectList(userlist, "UserID", "UserName", user);

                ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), 

"BranchID", "BranchName", branch);
                ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }.Union

(DepartmentList), "DepartmentId", "DepartmentName", deptmnt);
                ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }.Union(GroupList), "GroupID", 

"GroupName", groups);
                ViewBag.Customer = new SelectList(new[] { new ProjectPayment() { PaymentID = 0, CustomerName = "---Select---" } }.Union(CustomerList), 

"PaymentID", "CustomerName", customer);
                return View(obj);
        }

        [HttpGet]
        public ActionResult UpdatePayment(int PaymentID,int PaymenttermId)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            PaymentStructure obj = new PaymentStructure();
            obj = (from p in db.ProjectPayments.Where(x => x.IsActive == true && x.PaymentID==PaymentID)
                         join r in db.ProjectPaymetTermDetails.Where(x=>x.PaymentTermID==PaymenttermId) on p.PaymentID equals r.PaymentID
                         join s in db.Master_ProjectPaymentTerms on p.TermType equals s.ProjectPaymentTermID
                         join q in db.Master_Branches on p.Branch equals q.BranchID
                         join t in db.Departments.Where(x=>x.IsActive==true) on p.Department equals t.DepartmentId
                         join v in db.Users on r.UserId equals  v.UserID
                         join u in db.DepartmentGroups.Where(x=>x.IsActive==true) on p.DepartmentGroup equals u.GroupID into yr from y in yr.DefaultIfEmpty()
                         select new PaymentStructure
                         {
                             PaymentID = p.PaymentID,
                             CustomerName = p.CustomerName,
                             UserName = v.FirstName+" "+v.LastName,
                             Branch=q.BranchName,
                             Department=t.DepartmentName,
                             Group=y.GroupName,
                             DueDate = (DateTime)r.DueDate,
                             PaymentType = s.Terms,
                             StatusID=(int)r.PaidStatus,
                             Amount = (double)r.Amount,
                             PaymentTermID=(int)r.PaymentTermID,
                             PaidAmount = (double)r.PaidAmount,
                             PendingAmount = (double)r.Pending,
                             comments=r.Comments
                         }).FirstOrDefault();
            var StatusList = (from p in db.PaymentStatus
                              select new { StatusId = p.StatusID, PaymentStatus = p.PaymentStatus }).ToList();
            ViewBag.StatusList = StatusList;
            ViewBag.PaymentId = obj.PaymentID;
            ViewBag.PaymentTermId = obj.PaymentTermID;
            return View(obj);
        }

        [HttpPost]
        public ActionResult UpdatePayment(PaymentStructure data)
        {
            try
            {
                string ServerName = AppValue.GetFromMailAddress("ServerName");
                PaymentStructure saved=new PaymentStructure();
                saved = (from p in db.ProjectPaymetTermDetails
                    where p.PaymentID == data.PaymentID && p.PaymentTermID == data.PaymentTermID
                    join q in db.PaymentStatus on p.PaidStatus equals q.StatusID
                    select new PaymentStructure()
                    {
                        Status = q.PaymentStatus,
                        comments = p.Comments,
                        PaidAmount = p.PaidAmount,
                        PendingAmount = p.Pending

                    }).FirstOrDefault();

                var obj = db.ProjectPaymetTermDetails.Where( x=>x.PaymentID == data.PaymentID && x.PaymentTermID==data.PaymentTermID).Select(o => 

o).FirstOrDefault();
                obj.PaidStatus = data.StatusID;
                obj.Comments = data.comments;
                obj.PaidAmount = data.PaidAmount;
                obj.Pending = data.PendingAmount;
                db.SaveChanges();

                UpdatePaymentStructure updatepayment = new UpdatePaymentStructure();
                {
                    updatepayment.Status = db.PaymentStatus.Where(x=>x.StatusID==data.StatusID).Select(o=>o.PaymentStatus).FirstOrDefault();
                    updatepayment.comments = data.comments;
                    updatepayment.PaidAmount = data.PaidAmount;
                    updatepayment.PendingAmount = data.PendingAmount;

                }
                string MailBody = "";
                var updated = updatepayment;
                List<Variance> diff = ext.DetailedCompare(updated, saved);
                int a;

                if (diff.Count > 0)
                {
                    MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Old Values</p> <br />";

                    for (a = 0; a < diff.Count; a++)
                    {
                        if (diff[a].UserUpdateValue != null)
                        {
                            if (diff[a].UserSaveValue == null)
                                diff[a].UserSaveValue = "";
                            //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p>" + System.Environment.NewLine;

                            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff

[a].FieldName == "ContactNo" ? "MobileNumber" : diff[a].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff

[a].UserSaveValue + @"</label></p><br/>" + System.Environment.NewLine;
                        }

                    }

                    MailBody += "<br /><p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Updated Values</p>  <br />";

                    for (a = 0; a < diff.Count; a++)
                    {
                        if (diff[a].UserUpdateValue != null)
                        {
                            //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p>" + System.Environment.NewLine;
                            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff

[a].FieldName == "ContactNo" ? "MobileNumber" : diff[a].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff

[a].UserUpdateValue.ToString() + @"</label></p><br/>" + System.Environment.NewLine;
                        }
                    }

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Update Project Payment").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Update Project Payment").Select(x => x.TemplatePath).FirstOrDefault();
                     if ((check != null) && (check != 0))
                     {
                         var projectpaymentupdate = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Update Project Payment")
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
                         data.CreatedBy = db.Users.Where(o => o.UserID == userId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                         var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                         string Templateprojectpaymentupdate = Server.MapPath(projectpaymentupdate.Template);
                         string htmlEmpDetailUpdate = System.IO.File.ReadAllText(Templateprojectpaymentupdate);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#PaymentID", Convert.ToString(data.PaymentID));
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#CustomerName", data.CustomerName);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", data.CreatedBy);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                         htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Company", company);
                       //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                         if (ServerName  != "http://win2012srv:88/")
                         {
                             List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                             string EmailAddress = "";

                             foreach (string mail in MailIds)
                             {
                                 EmailAddress += mail + ",";
                             }
                             EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                             string CCMailId = "virupaksha.gaddad@dsrc.co.in";
                             string BCCMailId = "Kirankumar@dsrc.co.in ";

                             var path = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                             Session["LogoPath"] = path.AppValue;
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMailToALL(null, projectpaymentupdate.Subject + " - Test Mail Please Ignore", null,

     htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                             });

                         }
                         else
                         {
                             Task.Factory.StartNew(() =>
                             {
                                 var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                 DsrcMailSystem.MailSender.SendMailToALL(null, projectpaymentupdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in",

     projectpaymentupdate.To, projectpaymentupdate.CC, projectpaymentupdate.BCC, Server.MapPath(logo.AppValue.ToString()));

                             });
                         }
                     }

                     else
                     {
                        // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                         ExceptionHandlingController.TemplateMissing("Update Project Payment", folder, ServerName);
                     }

                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("failed", JsonRequestBehavior.AllowGet);
            }

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDepartments(int BranchId)
        {
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();

            if (BranchId != 0)
            {

                List<int> validDepart = new List<int>();

                validDepart = db.Departments.Where(d => d.BranchID == BranchId && d.IsActive == true).Select(d => d.DepartmentId).ToList();

                FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                select new FilterDepartment()
                                {
                                    DepartmentId = lt.DepartmentId,
                                    DepartmentName = lt.DepartmentName
                                }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
            }
            return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetGroups(int DepartmentId)
        {
            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();

            if (DepartmentId != 0)
            {
                var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == DepartmentId).Select(d => d.GroupID).ToList();

                FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                               where lt.IsActive == true
                               select new FilterGroup()
                               {
                                   GroupId = lt.GroupID,
                                   GroupName = lt.GroupName
                               }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });
            }
            return Json(new SelectList(FilterGroup, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

  

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCustomer(int DepartmentId,int GroupId)
        {
            IEnumerable<SelectListItem> FilterCustomer = new List<SelectListItem>();

            if (DepartmentId != 0 && GroupId !=0)
            {
                var validCustomer = db.ProjectPayments.Where(d => d.Department == DepartmentId && d.DepartmentGroup==GroupId).Select(d => 

d.PaymentID).ToList();

                FilterCustomer = (from lt in db.ProjectPayments.Where(o => validCustomer.Contains(o.PaymentID))
                               where lt.IsActive == true
                               select new FilterCustomer()
                               {
                                   PaymentId = lt.PaymentID,
                                   CustomerName = lt.CustomerName
                               }).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.PaymentId), Text = m.CustomerName });
            }
            return Json(new SelectList(FilterCustomer, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }



        



        [HttpGet]
        public ActionResult GetEmployees(int DepartmentId,int BranchId)
        {
          //  PaymentStructure objmodel = new PaymentStructure();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
              
                int userId = Convert.ToInt32(Session["UserID"]);

                if (DepartmentId != 0 && BranchId != 0)
                {
                    var  Employeedetails = (from p in db.Users.Where(x => x.IsActive == true && x.DepartmentId == DepartmentId && x.BranchId == BranchId)
                                        select new 
                                        {
                                            Userid = p.UserID,
                                            UserName = p.FirstName + "  " + p.LastName
                                        }).ToList();
                    return Json(new { ListValue = Employeedetails }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetUsers(int BranchId ,int DepartmentId,int GroupId)
        {
            //  PaymentStructure objmodel = new PaymentStructure();
            try
            {
                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

                int userId = Convert.ToInt32(Session["UserID"]);

                if (DepartmentId != 0 && BranchId != 0 && GroupId !=0)
                {
                    var Employeedetails = (from p in db.Users.Where(x => x.IsActive == true && x.DepartmentId == DepartmentId && x.BranchId == BranchId && x.DepartmentGroup==GroupId)
                                           select new
                                           {
                                               Userid = p.UserID,
                                               UserName = p.FirstName + "  " + p.LastName
                                           }).ToList();
                    return Json(new { ListValue = Employeedetails }, JsonRequestBehavior.AllowGet);
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
        public ActionResult Assign(string PaymentID)
        {
            DSRCManagementSystem.Models.PaymentStructure objmodel = new DSRCManagementSystem.Models.PaymentStructure();
            try
            {

                DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();

                int id = Convert.ToInt32(PaymentID);

                var UserList = (from p in objdb.Users.Where(x => x.IsActive == true)
                                select new
                                {
                                    Userid = p.UserID,
                                    Employees = p.FirstName + "  " + p.LastName
                                }).ToList();


                int? branchid = objdb.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.Branch).FirstOrDefault();

                int? depid = objdb.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.Department).FirstOrDefault();

                int? groupid = objdb.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.DepartmentGroup).FirstOrDefault();

                var userlist = (from p in objdb.Users.Where(x => x.IsActive == true && x.DepartmentId == depid && x.BranchId == branchid && x.DepartmentGroup == groupid)
                                select new
                                {
                                    Userid = p.UserID,
                                    Employees = p.FirstName + "  " + p.LastName
                                }).ToList();




                var list = (from p in objdb.Users.Where(x => x.IsActive == true && x.DepartmentId == depid && x.BranchId == branchid)
                            select new
                            {
                                Userid = p.UserID,
                                Employees = p.FirstName + "  " + p.LastName
                            }).ToList();
               
                 

                List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();


                objuser = (from p in db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id)
                           select new DSRCManagementSystem.Models.ListReporting
                           {
                               Id = p.UserId

                           }).Distinct().ToList();

                List<int> selectedAttendees = new List<int>();


                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                }



                if (objuser.Count() > 0 && branchid != 0 && depid != 0)
                {
                    objmodel.dbvalue = 1;
                    if (groupid != 0 && groupid != null)
                    {
                        ViewBag.Employees = new MultiSelectList(userlist, "Userid", "Employees", selectedAttendees);
                    }
                    else
                    {
                        ViewBag.Employees = new MultiSelectList(list, "Userid", "Employees", selectedAttendees);
                    }
                    var BranchList = db.Master_Branches.ToList();
                    //var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
                    var DepartmentList = communicationHelper.GetDepartments();

                    var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();

                    ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), 

"BranchID", "BranchName", branchid);

                    var department = (from p in objdb.Departments.Where(x => x.BranchID == branchid) orderby p.DepartmentName
                                      select new
                                      {
                                          departmentid = p.DepartmentId,
                                          departmentname = p.DepartmentName
                                      }).ToList();

                    ViewBag.DepartmentIdList = new SelectList(department, "departmentid", "departmentname", depid);


                    var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == depid).Select(d => d.GroupID).ToList();

                    var FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                                       where lt.IsActive == true orderby lt.GroupName
                                       select new
                                       {
                                           groupid = lt.GroupID,
                                           groupname = lt.GroupName
                                       }).ToList();

                    if (groupid != 0)
                    {
                        objmodel.dbvalue = 1;
                        ViewBag.Groups = new SelectList(FilterGroup, "groupid", "  groupname", groupid);
                    }
                    else
                    {
                  
                        ViewBag.Groups = new SelectList(FilterGroup, "groupid", "  groupname", 0);
                    }


                }

                else
                {
                    objmodel.dbvalue = 0;
                    ViewBag.Employees = new MultiSelectList(UserList, "Userid", "Employees");



                    //  ViewBag.ReportingPerson = new MultiSelectList(objuser, "Id1", "UserName1", selectedAttendees);




                    var BranchList = db.Master_Branches.ToList();
                    //var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
                    var DepartmentList = communicationHelper.GetDepartments();

                    var GroupList = db.DepartmentGroups.Where(d => d.IsActive == true).ToList();

                    ViewBag.BranchList = new SelectList(new[] { new Master_Branches() { BranchID = 0, BranchName = "---Select---" } }.Union(BranchList), 

"BranchID", "BranchName", 0);
                    ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, 

"DepartmentId", "DepartmentName", 0);
                    ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "---Select---" } }, "GroupID", "GroupName", 0);
                    objmodel.PaymentID = Convert.ToInt32(PaymentID);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }

            return View(objmodel);
        }


        [HttpPost]
        public ActionResult Assign(string PaymentId, string BranchId, string DepartmentId, string GroupId, string Employees)
        {
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.PaymentStructure objmodel = new DSRCManagementSystem.Models.PaymentStructure();
            int id = Convert.ToInt32(PaymentId);
            int objvalue1 =Convert.ToInt32(   Session["UserID"]);
            int Branch = Convert.ToInt32(BranchId);
            int Department = Convert.ToInt32(DepartmentId);
            int Group=0;
            if (GroupId != "" && GroupId != null)
            {
                Group = Convert.ToInt32(GroupId);
            }

            var record = objdb.ProjectPayments.Where(x => x.PaymentID == id && x.IsActive == true).Select(o => o).FirstOrDefault();

            var uservalues = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.UserId).Distinct().ToList();

            TempData["Values"] = Employees;


            //1
            if (record.Branch == null && record.Department == null && record.DepartmentGroup == null)
            {

                record.Department = Department;
                record.Branch = Branch;
                record.DepartmentGroup = Group;
                objdb.SaveChanges();

                var Userrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == 0).Select(o => o).ToList();

                var objrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).FirstOrDefault();

                var objrecordvalue = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == objrecord.UserId).Select(o => o).ToList();

                string[] objuser = Employees.Split(',');

                if (Userrecord.Count() != 0)
                {
                    if (objuser.Count() == 1)
                    {
                        for (int i = 0; i < Userrecord.Count(); i++)
                        {
                            foreach (var user in objuser)
                            {
                                int User = Convert.ToInt32(user);
                                Userrecord[i].UserId = Convert.ToInt32(user);
                            }

                            objdb.SaveChanges();
                        }
                    }

                    else if (objuser.Count() > 1)
                    {
                        for (int k = 0; k < objuser.Count(); k++)
                        {
                            if (k == 0)
                            {
                                for (int l = 0; l < Userrecord.Count(); l++)
                                {
                                    Userrecord[l].UserId = Convert.ToInt32(objuser[k]);
                                    objdb.SaveChanges();
                                }
                            }
                            else
                            {
                                for (int a = 0; a < Userrecord.Count(); a++)
                                {
                                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                    objdetail.PaymentID = Userrecord[a].PaymentID;
                                    objdetail.DueDate = Userrecord[a].DueDate;
                                    objdetail.Amount = Userrecord[a].Amount;
                                    objdetail.PaidStatus = Userrecord[a].PaidStatus;
                                    objdetail.Comments = Userrecord[a].Comments;
                                    objdetail.PaidAmount = Userrecord[a].PaidAmount;
                                    objdetail.UserId = Convert.ToInt32(objuser[k]);
                                    objdb.AddToProjectPaymetTermDetails(objdetail);
                                    objdb.SaveChanges();
                                }

                            }
                        }
                    }

                }





                else if (Userrecord.Count() == 0 && objrecordvalue.Count() != 0)
                {
                    for (int w = 0; w < objuser.Count(); w++)
                    {
                        int userid = Convert.ToInt32(objuser[w]);
                        var isrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == userid).Select(o => o).ToList();

                        if (isrecord.Count == 0)
                        {
                            for (int a = 0; a < objrecordvalue.Count(); a++)
                            {
                                DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                objdetail.PaymentID = objrecordvalue[a].PaymentID;
                                objdetail.DueDate = objrecordvalue[a].DueDate;
                                objdetail.Amount = objrecordvalue[a].Amount;
                                objdetail.PaidStatus = objrecordvalue[a].PaidStatus;
                                objdetail.Comments = objrecordvalue[a].Comments;
                                objdetail.PaidAmount = objrecordvalue[a].PaidAmount;
                                objdetail.UserId = Convert.ToInt32(objuser[w]);
                                objdb.AddToProjectPaymetTermDetails(objdetail);
                                objdb.SaveChanges();
                            }
                        }

                    }

                }







            }






                //2



            else if (record.Branch == Branch && record.Department != Department && record.DepartmentGroup != Group)
            {

                record.Department = Department;
                record.Branch = Branch;
                record.DepartmentGroup = Group;
                objdb.SaveChanges();

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();


                var dates = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var delRecords = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var termcount = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                var vps = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();
                var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();


                foreach (var tax in delRecords)
                {
                    db.ProjectPaymetTermDetails.DeleteObject(tax);
                    db.SaveChanges();
                }

                string[] objuser = Employees.Split(',');

                if (termcount == 1)
                {

                    for (int k = 0; k < objuser.Count(); k++)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps[i].DueDate;
                            objdetail.Amount = vps[i].Amount;
                            objdetail.PaidStatus = vps[i].PaidStatus;
                            objdetail.Comments = vps[i].Comments;
                            objdetail.PaidAmount = vps[i].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser[k]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                        }

                    }
                }

                if (termcount == 2)
                {
                    var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                    var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                    for (int d = 0; d < objuser.Count(); d++)
                    {

                        for (int i = 0; i < vps3.Count(); i++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps3[i].Value;
                            objdetail.Amount = vps[i].Amount;
                            objdetail.PaidStatus = vps[i].PaidStatus;
                            objdetail.Comments = vps[i].Comments;
                            objdetail.PaidAmount = vps[i].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser[d]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                            TempData["message"] = "Edited";
                        }
                    }

                }

                if (termcount == 3)
                {

                    for (int i = 0; i < objuser.Count(); i++)
                    {
                        for (int k = 0; k < 1; k++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps[k].DueDate;
                            objdetail.Amount = vps[k].Amount;
                            objdetail.PaidStatus = vps[k].PaidStatus;
                            objdetail.Comments = vps[k].Comments;
                            objdetail.PaidAmount = vps[k].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser[i]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                        }
                        TempData["message"] = "Edited";
                    }


                }


                int? term = record.TermType;


                   int id2 = Convert.ToInt32(PaymentId);
                   email(id2, objvalue1, Employees);


                 //  return RedirectToAction("email", "PaymentStructure", new { PaymentId = id2, UserId = objvalue1, Assigned = Employees });

                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }


//3

            else if (Branch != record.Branch && Department != record.Department && Group != record.DepartmentGroup)
            {

                if (record != null)
                {
                    record.Department = Department;
                    record.Branch = Branch;
                    record.DepartmentGroup = Group;
                    objdb.SaveChanges();
                }


                DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();


                var dates = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var delRecords = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var termcount = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                var vps = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();
                var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();

                foreach (var tax in delRecords)
                {
                    db.ProjectPaymetTermDetails.DeleteObject(tax);
                    db.SaveChanges();
                }

                string[] objuser1 = Employees.Split(',');


                if (termcount == 1)
                {
                    for (int k = 0; k < objuser1.Count(); k++)
                    {
                        for (int i = 0; i < 12; i++)
                        {

                            for (int j = 0; j < 1; k++)
                            {
                                DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                objdetail.PaymentID = id;
                                objdetail.DueDate = vps[j].DueDate;
                                objdetail.Amount = vps[j].Amount;
                                objdetail.PaidStatus = vps[j].PaidStatus;
                                objdetail.Comments = vps[j].Comments;
                                objdetail.PaidAmount = vps[j].PaidAmount;
                                objdetail.UserId = Convert.ToInt32(objuser1[k]);
                                objdb.AddToProjectPaymetTermDetails(objdetail);
                                objdb.SaveChanges();
                            }
                        }

                    }
                }

                if (termcount == 2)
                {
                    var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                    var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                    for (int d = 0; d < objuser1.Count(); d++)
                    {

                        for (int i = 0; i < vps3.Count(); i++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps3[i].Value;
                            objdetail.Amount = vps[i].Amount;
                            objdetail.PaidStatus = vps[i].PaidStatus;
                            objdetail.Comments = vps[i].Comments;
                            objdetail.PaidAmount = vps[i].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser1[d]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                            TempData["message"] = "Edited";
                        }
                    }

                }

                if (termcount == 3)
                {
                    var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                    var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                    for (int i = 0; i < objuser1.Count(); i++)
                    {
                        for (int k = 0; k < 1; k++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps[k].DueDate;
                            objdetail.Amount = vps[k].Amount;
                            objdetail.PaidStatus = vps[k].PaidStatus;
                            objdetail.Comments = vps[k].Comments;
                            objdetail.PaidAmount = vps[k].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser1[i]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                        }
                        TempData["message"] = "Edited";
                    }

                    int id2 = Convert.ToInt32(PaymentId);
                    email(id2, objvalue1, Employees);
                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

                }

            }


               // int? term = record.TermType;


            else if (Branch == record.Branch && Department == record.Department && Group != record.DepartmentGroup)
            {

                DSRCManagementSystemEntities1 oldb = new DSRCManagementSystemEntities1();
                var record1 = objdb.ProjectPayments.Where(x => x.PaymentID == id && x.IsActive == true).Select(o => o).FirstOrDefault();
                record1.Branch = Branch;
                record1.Department = Department;
                record1.DepartmentGroup = Group;
                objdb.SaveChanges();






                var objdates = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var objdelRecords = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var objtermcount = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                var objvps = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();

                foreach (var tax in objdelRecords)
                {
                    db.ProjectPaymetTermDetails.DeleteObject(tax);
                    db.SaveChanges();
                }

                string[] objuser2 = Employees.Split(',');


                if (objtermcount == 1)
                {
                    for (int k = 0; k < objuser2.Count(); k++)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = objvps[i].DueDate;
                            objdetail.Amount = objvps[i].Amount;
                            objdetail.PaidStatus = objvps[i].PaidStatus;
                            objdetail.Comments = objvps[i].Comments;
                            objdetail.PaidAmount = objvps[i].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser2[k]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                        }

                    }
                }

                if (objtermcount == 2)
                {
                    var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                    var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                    for (int d = 0; d < objuser2.Count(); d++)
                    {

                        for (int i = 0; i < vps3.Count(); i++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = vps3[i].Value;
                            objdetail.Amount = objvps[i].Amount;
                            objdetail.PaidStatus = objvps[i].PaidStatus;
                            objdetail.Comments = objvps[i].Comments;
                            objdetail.PaidAmount = objvps[i].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser2[d]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                            TempData["message"] = "Edited";
                        }
                    }

                }

                if (objtermcount == 3)
                {
                    var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                    var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                    for (int i = 0; i < objuser2.Count(); i++)
                    {

                        for (int k = 0; k < 1; k++)
                        {
                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                            objdetail.PaymentID = id;
                            objdetail.DueDate = objvps[k].DueDate;
                            objdetail.Amount = objvps[k].Amount;
                            objdetail.PaidStatus = objvps[k].PaidStatus;
                            objdetail.Comments = objvps[k].Comments;
                            objdetail.PaidAmount = objvps[k].PaidAmount;
                            objdetail.UserId = Convert.ToInt32(objuser2[i]);
                            objdb.AddToProjectPaymetTermDetails(objdetail);
                            objdb.SaveChanges();
                        }
                        TempData["message"] = "Edited";
                    }

                    int id2 = Convert.ToInt32(PaymentId);
                    email(id2, objvalue1, Employees);

                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);




                }


            }




            else
            {
                record.Department = Department;
                record.Branch = Branch;
                record.DepartmentGroup = Group;
                objdb.SaveChanges();

                List<int?> objuser = new List<int?>();

                string[] value = Employees.Split(',');

                for (int i = 0; i < value.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(value[i]));
                }

                for (int y = 0; y < objuser.Count(); y++)
                {
                    TempData["Null"] = "0";
                    var Value = Convert.ToInt32(objuser[y]);
                    var pid = Convert.ToInt32(PaymentId);
                    var alreadyvalue = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == pid).Select(o => o).ToList();

                    if (alreadyvalue.Count() > objuser.Count())
                    {
                        var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                        if (objuser.Count() == 1)
                        {
                            var objtermcount1 = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                            var objvps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();


                            foreach (var vp in objvps1)
                                db.ProjectPaymetTermDetails.DeleteObject(vp);
                            db.SaveChanges();

                            if (objtermcount1 == 1)
                            {
                                for (int i = 0; i < 12; i++)
                                {
                                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                    objdetail.PaymentID = id;
                                    objdetail.DueDate = objvps1[i].DueDate;
                                    objdetail.Amount = objvps1[i].Amount;
                                    objdetail.PaidStatus = objvps1[i].PaidStatus;
                                    objdetail.Comments = objvps1[i].Comments;
                                    objdetail.PaidAmount = objvps1[i].PaidAmount;
                                    objdetail.UserId = Value;
                                    objdb.AddToProjectPaymetTermDetails(objdetail);
                                    objdb.SaveChanges();
                                    TempData["message"] = "Edited";
                                }

                                int id2 = Convert.ToInt32(PaymentId);
                                email(id2, objvalue1, Employees);
                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }

                            else if (objtermcount1 == 2)
                            {
                                var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                for (int k = 0; k < objuser.Count(); k++)
                                {
                                    for (int i = 0; i < vps3.Count(); i++)
                                    {
                                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                        objdetail.PaymentID = id;
                                        objdetail.DueDate = vps3[i].Value;
                                        objdetail.Amount = objvps1[i].Amount;
                                        objdetail.PaidStatus = objvps1[i].PaidStatus;
                                        objdetail.Comments = objvps1[i].Comments;
                                        objdetail.PaidAmount = objvps1[i].PaidAmount;
                                        objdetail.UserId = Convert.ToInt32(objuser[k]);
                                        objdb.AddToProjectPaymetTermDetails(objdetail);
                                        objdb.SaveChanges();
                                        TempData["message"] = "Edited";
                                    }

                                    int id2 = Convert.ToInt32(PaymentId);
                                    email(id2, objvalue1, Employees);
                                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                                }



                            }


                            else if (objtermcount1 == 3)
                            {
                                var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                for (int i = 0; i < objuser.Count(); i++)
                                {

                                    for (int k = 0; k < 1; k++)
                                    {
                                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                        objdetail.PaymentID = id;
                                        objdetail.DueDate = objvps1[k].DueDate;
                                        objdetail.Amount = objvps1[k].Amount;
                                        objdetail.PaidStatus = objvps1[k].PaidStatus;
                                        objdetail.Comments = objvps1[k].Comments;
                                        objdetail.PaidAmount = objvps1[k].PaidAmount;
                                        objdetail.UserId = Convert.ToInt32(objuser[i]);
                                        objdb.AddToProjectPaymetTermDetails(objdetail);
                                        objdb.SaveChanges();
                                    }
                                    TempData["message"] = "Edited";
                                }
                                int id2 = Convert.ToInt32(PaymentId);
                                email(id2, objvalue1, Employees);
                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);



                            }






                        }










                        else
                        {
                            var objtermcount1 = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                            var objvps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();
                            var vps5 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();

                            foreach (var vp in objvps1)
                                db.ProjectPaymetTermDetails.DeleteObject(vp);
                            db.SaveChanges();

                            if (objtermcount1 == 1)
                            {
                                for (int k = 0; k < objuser.Count(); k++)
                                {
                                    for (int i = 0; i < 12; i++)
                                    {
                                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                        objdetail.PaymentID = id;
                                        objdetail.DueDate = objvps1[i].DueDate;
                                        objdetail.Amount = objvps1[i].Amount;
                                        objdetail.PaidStatus = objvps1[i].PaidStatus;
                                        objdetail.Comments = objvps1[i].Comments;
                                        objdetail.PaidAmount = objvps1[i].PaidAmount;
                                        objdetail.UserId = Convert.ToInt32(objuser[k]);
                                        objdb.AddToProjectPaymetTermDetails(objdetail);
                                        objdb.SaveChanges();
                                        TempData["message"] = "Edited";
                                    }
                                }
                                int id2 = Convert.ToInt32(PaymentId);
                                email(id2, objvalue1, Employees);
                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }

                            else if (objtermcount1 == 2)
                            {
                                var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                for (int k = 0; k < objuser.Count(); k++)
                                {
                                    for (int i = 0; i < vps3.Count(); i++)
                                    {
                                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                        objdetail.PaymentID = id;
                                        objdetail.DueDate = vps5[i].Value;
                                        objdetail.Amount = objvps1[i].Amount;
                                        objdetail.PaidStatus = objvps1[i].PaidStatus;
                                        objdetail.Comments = objvps1[i].Comments;
                                        objdetail.PaidAmount = objvps1[i].PaidAmount;
                                        objdetail.UserId = Convert.ToInt32(objuser[k]);
                                        objdb.AddToProjectPaymetTermDetails(objdetail);
                                        objdb.SaveChanges();
                                        TempData["message"] = "Edited";
                                    }
                                }



                            }


                            else if (objtermcount1 == 3)
                            {
                                var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                for (int i = 0; i < objuser.Count(); i++)
                                {

                                    for (int k = 0; k < 1; k++)
                                    {
                                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                        objdetail.PaymentID = id;
                                        objdetail.DueDate = objvps1[k].DueDate;
                                        objdetail.Amount = objvps1[k].Amount;
                                        objdetail.PaidStatus = objvps1[k].PaidStatus;
                                        objdetail.Comments = objvps1[k].Comments;
                                        objdetail.PaidAmount = objvps1[k].PaidAmount;
                                        objdetail.UserId = Convert.ToInt32(objuser[i]);
                                        objdb.AddToProjectPaymetTermDetails(objdetail);
                                        objdb.SaveChanges();
                                    }
                                    TempData["message"] = "Edited";
                                }
                                int id2 = Convert.ToInt32(PaymentId);
                                email(id2, objvalue1, Employees);
                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);



                            }
                        }

                    }

                    if (alreadyvalue.Count == 0)
                    {
                        var objtermcount1 = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                        var objvps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();
                        var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                        if (objtermcount1 == 1)
                        {

                            for (int d = 0; d < objuser.Count(); d++)
                            {
                                for (int k = 0; k < 12; k++)
                                {
                                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                    objdetail.PaymentID = id;
                                    objdetail.DueDate = objvps1[k].DueDate;
                                    objdetail.Amount = objvps1[k].Amount;
                                    objdetail.PaidStatus = objvps1[k].PaidStatus;
                                    objdetail.Comments = objvps1[k].Comments;
                                    objdetail.PaidAmount = objvps1[k].PaidAmount;
                                    objdetail.UserId =Convert.ToInt32(objuser[d]);
                                    objdb.AddToProjectPaymetTermDetails(objdetail);
                                    objdb.SaveChanges();
                                }

                            }

                        }


                        if (objtermcount1 == 2)
                        {
                            var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                            var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                            for (int d = 0; d < objuser.Count(); d++)
                            {

                                for (int i = 0; i < vps3.Count(); i++)
                                {
                                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                    objdetail.PaymentID = id;
                                    objdetail.DueDate = vps3[i].Value;
                                    objdetail.Amount = objvps1[i].Amount;
                                    objdetail.PaidStatus = objvps1[i].PaidStatus;
                                    objdetail.Comments = objvps1[i].Comments;
                                    objdetail.PaidAmount = objvps1[i].PaidAmount;
                                    objdetail.UserId = Convert.ToInt32(objuser[d]);
                                    objdb.AddToProjectPaymetTermDetails(objdetail);
                                    objdb.SaveChanges();
                                    TempData["message"] = "Edited";
                                }
                            }


                        }


                        if (objtermcount1 == 3)
                        {
                            var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                            var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                            for (int i = 0; i < objuser.Count(); i++)
                            {

                                for (int k = 0; k < 1; k++)
                                {
                                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                    objdetail.PaymentID = id;
                                    objdetail.DueDate = objvps1[k].DueDate;
                                    objdetail.Amount = objvps1[k].Amount;
                                    objdetail.PaidStatus = objvps1[k].PaidStatus;
                                    objdetail.Comments = objvps1[k].Comments;
                                    objdetail.PaidAmount = objvps1[k].PaidAmount;
                                    objdetail.UserId = Convert.ToInt32(objuser[i]);
                                    objdb.AddToProjectPaymetTermDetails(objdetail);
                                    objdb.SaveChanges();
                                }
                                TempData["message"] = "Edited";
                            }
                            db.SaveChanges();
                            int id2 = Convert.ToInt32(PaymentId);
                            email(id2, objvalue1, Employees);
                            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);



                        }


                        db.SaveChanges();
                        TempData["message"] = "Edited";
                        int id7 = Convert.ToInt32(PaymentId);
                        email(id7, objvalue1, Employees);
                        return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);


                    }



                    if (TempData["Null"].ToString() == "0")
                    {
                        if (TempData["Null"].ToString() != "Deleted")
                        {
                            if (y < alreadyvalue.Count())
                            {
                                alreadyvalue[y].PaymentID = Value;
                            }
                            else
                            {

                                var objtermcount2 = db.ProjectPayments.Where(x => x.PaymentID == id).Select(o => o.TermType).FirstOrDefault();
                                var objvps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();
                                var vps3 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                if (objtermcount2 == 1)
                                {

                                    for (int d = 0; d < objuser.Count(); d++)
                                    {
                                        for (int k = 0; k < 12; k++)
                                        {
                                            DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                            objdetail.PaymentID = id;
                                            objdetail.DueDate = objvps1[k].DueDate;
                                            objdetail.Amount = objvps1[k].Amount;
                                            objdetail.PaidStatus = objvps1[k].PaidStatus;
                                            objdetail.Comments = objvps1[k].Comments;
                                            objdetail.PaidAmount = objvps1[k].PaidAmount;
                                            objdetail.UserId = Value;
                                            objdb.AddToProjectPaymetTermDetails(objdetail);
                                            objdb.SaveChanges();
                                        }

                                    }


                                    if (objtermcount2 == 2)
                                    {
                                        var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                        var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                        for (int d = 0; d < objuser.Count(); d++)
                                        {

                                            for (int i = 0; i < vps3.Count(); i++)
                                            {
                                                DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                                objdetail.PaymentID = id;
                                                objdetail.DueDate = vps3[i].Value;
                                                objdetail.Amount = objvps1[i].Amount;
                                                objdetail.PaidStatus = objvps1[i].PaidStatus;
                                                objdetail.Comments = objvps1[i].Comments;
                                                objdetail.PaidAmount = objvps1[i].PaidAmount;
                                                objdetail.UserId =Convert.ToInt32(objuser[d]);
                                                objdb.AddToProjectPaymetTermDetails(objdetail);
                                                objdb.SaveChanges();
                                                TempData["message"] = "Edited";
                                            }
                                        }


                                    }


                                    if (objtermcount2 == 3)
                                    {
                                        var vps1 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o.DueDate).Distinct().ToList();
                                        var vps2 = db.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).ToList();

                                        for (int i = 0; i < objuser.Count(); i++)
                                        {

                                            for (int k = 0; k < 1; k++)
                                            {
                                                DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                                                objdetail.PaymentID = id;
                                                objdetail.DueDate = objvps1[k].DueDate;
                                                objdetail.Amount = objvps1[k].Amount;
                                                objdetail.PaidStatus = objvps1[k].PaidStatus;
                                                objdetail.Comments = objvps1[k].Comments;
                                                objdetail.PaidAmount = objvps1[k].PaidAmount;
                                                objdetail.UserId = Convert.ToInt32(objuser[i]);
                                                objdb.AddToProjectPaymetTermDetails(objdetail);
                                                objdb.SaveChanges();
                                            }
                                            TempData["message"] = "Edited";
                                        }
                                        db.SaveChanges();
                                        int id2 = Convert.ToInt32(PaymentId);
                                        email(id2, objvalue1, Employees);
                                        return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);



                                    }


                                    db.SaveChanges();



                                    TempData["message"] = "Edited";
                                    int id3 = Convert.ToInt32(PaymentId);
                                    email(id3, objvalue1, Employees);
                                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);


                                }
                            }
                        }
                        TempData["message"] = "Edited";
                        int id4 = Convert.ToInt32(PaymentId);
                        email(id4, objvalue1, Employees);
                        return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

                    }








                    //else if (Branch == record.Branch && Department == record.Department && Group == record.DepartmentGroup)
                    //{
                    //    var Userrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == 0).Select(o => o).ToList();

                    //    var objrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id).Select(o => o).FirstOrDefault();

                    //    var objrecordvalue = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == objrecord.UserId).Select(o => 

//o).ToList();

                    //    string[] objuser = Employees.Split(',');

                    //    if (Userrecord.Count() != 0)
                    //    {
                    //        if (objuser.Count() == 1)
                    //        {
                    //            for (int i = 0; i < Userrecord.Count(); i++)
                    //            {
                    //                foreach (var user in objuser)
                    //                {
                    //                    int User = Convert.ToInt32(user);
                    //                    Userrecord[i].UserId = Convert.ToInt32(user);
                    //                }

                    //                objdb.SaveChanges();
                    //            }
                    //        }

                    //        else if (objuser.Count() > 1)
                    //        {
                    //            for (int k = 0; k < objuser.Count(); k++)
                    //            {
                    //                if (k == 0)
                    //                {
                    //                    for (int l = 0; l < Userrecord.Count(); l++)
                    //                    {
                    //                        Userrecord[l].UserId = Convert.ToInt32(objuser[k]);
                    //                        objdb.SaveChanges();
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    for (int a = 0; a < Userrecord.Count(); a++)
                    //                    {
                    //                        DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                    //                        objdetail.PaymentID = Userrecord[a].PaymentID;
                    //                        objdetail.DueDate = Userrecord[a].DueDate;
                    //                        objdetail.Amount = Userrecord[a].Amount;
                    //                        objdetail.PaidStatus = Userrecord[a].PaidStatus;
                    //                        objdetail.Comments = Userrecord[a].Comments;
                    //                        objdetail.PaidAmount = Userrecord[a].PaidAmount;
                    //                        objdetail.UserId = Convert.ToInt32(objuser[k]);
                    //                        objdb.AddToProjectPaymetTermDetails(objdetail);
                    //                        objdb.SaveChanges();
                    //                    }

                    //                }
                    //            }
                    //        }

                    //    }

                    //    else if (Userrecord.Count() == 0 && objrecordvalue.Count() != 0)
                    //    {
                    //        for (int w = 0; w < objuser.Count(); w++)
                    //        {
                    //            int userid = Convert.ToInt32(objuser[w]);
                    //            var isrecord = objdb.ProjectPaymetTermDetails.Where(x => x.PaymentID == id && x.UserId == userid).Select(o => o).ToList();

                    //            if (isrecord.Count == 0)
                    //            {
                    //                for (int a = 0; a < objrecordvalue.Count(); a++)
                    //                {
                    //                    DSRCManagementSystem.ProjectPaymetTermDetail objdetail = new DSRCManagementSystem.ProjectPaymetTermDetail();
                    //                    objdetail.PaymentID = objrecordvalue[a].PaymentID;
                    //                    objdetail.DueDate = objrecordvalue[a].DueDate;
                    //                    objdetail.Amount = objrecordvalue[a].Amount;
                    //                    objdetail.PaidStatus = objrecordvalue[a].PaidStatus;
                    //                    objdetail.Comments = objrecordvalue[a].Comments;
                    //                    objdetail.PaidAmount = objrecordvalue[a].PaidAmount;
                    //                    objdetail.UserId = Convert.ToInt32(objuser[w]);
                    //                    objdb.AddToProjectPaymetTermDetails(objdetail);
                    //                    objdb.SaveChanges();
                    //                }
                    //            }

                    //        }


                  


                }
            }



            int id5 = Convert.ToInt32(PaymentId);
            email(id5, objvalue1, Employees);

            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUser(int CustomerId)
        {
            IEnumerable<SelectListItem> Filter = new List<SelectListItem>();

            if (CustomerId != 0)
            {
                var validUser = (from p in db.ProjectPayments
                                 where (p.IsActive == true && p.PaymentID == CustomerId)
                                 join r in db.ProjectPaymetTermDetails on p.PaymentID equals r.PaymentID
                                 select r.UserId).ToList();

                Filter = (from lt in db.Users.Where(x => x.IsActive == true && validUser.Contains(x.UserID))
                          select new
                          {
                              UserID = lt.UserID,
                              UserName = lt.FirstName + "  " + lt.LastName
                          }).AsEnumerable()
                    .Select(m => new SelectListItem() { Value = Convert.ToString(m.UserID), Text = m.UserName });
            }
            return Json(new SelectList(Filter, "Value", "Text"), JsonRequestBehavior.AllowGet);

        }

           public  ActionResult email(int PaymentId,int UserId,string Assigned)
           {       
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            List<string> objuser = new List<string>();
            string obj = "";
            List<int> objvalue = new List<int>();

            string[] values = Assigned.Split(',');

            for (int k = 0; k < values.Count(); k++)
            {
                objvalue.Add(Convert.ToInt32( values[k]));
            }

            for (int i = 0; i < objvalue.Count(); i++)
            {
              //  DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int uservalue = Convert.ToInt32(objvalue[i]);
                var username = db.Users.Where(o => o.UserID == uservalue).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                objuser.Add(username);
            }
            obj = string.Join(",", objuser);
            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Create Project Payment").Select(o => o.EmailTemplateID).FirstOrDefault();
            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Create Project Payment").Select(x => x.TemplatePath).FirstOrDefault();
            if ((check != null) && (check != 0))
            {
                var paymentcreatemail = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Create Project Payment")
                                         join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                         select new DSRCManagementSystem.Models.Email
                                         {
                                             To = p.To,
                                             CC = p.CC,
                                             BCC = p.BCC,
                                             Subject = p.Subject,
                                             Template = q.TemplatePath
                                         }).FirstOrDefault();




                var  Amount = (double)db.ProjectPayments.FirstOrDefault(o => o.PaymentID == PaymentId).Amount;
                var customer=db.ProjectPayments.FirstOrDefault( o => o.PaymentID == PaymentId).CustomerName;
                var paymenttype = db.ProjectPayments.Where(x => x.PaymentID == PaymentId).Select( o => o.TermType).FirstOrDefault();
                var  PaymentType = db.Master_ProjectPaymentTerms.FirstOrDefault(o => o.ProjectPaymentTermID ==paymenttype).Terms;
                var  CreatedBy = db.Users.Where(o => o.UserID == UserId).Select(o => o.FirstName + " " + (o.LastName ?? "")).FirstOrDefault();
                string overallamount =Amount.ToString();
                string TemplatePathProjectStatus;

                var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

               TemplatePathProjectStatus = Server.MapPath(paymentcreatemail.Template);
         

                string htmlProjectStatus = System.IO.File.ReadAllText(TemplatePathProjectStatus);
                htmlProjectStatus = htmlProjectStatus.Replace("#Company", company);
                htmlProjectStatus = htmlProjectStatus.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                htmlProjectStatus = htmlProjectStatus.Replace("#CustomerName", customer);
                htmlProjectStatus = htmlProjectStatus.Replace("#PaymentType", PaymentType);
                htmlProjectStatus = htmlProjectStatus.Replace("#OverAllAmount", overallamount);
                htmlProjectStatus = htmlProjectStatus.Replace("#CreatedBy", CreatedBy);
                htmlProjectStatus = htmlProjectStatus.Replace("#ServerName",ServerName);
                //  htmlProjectStatus = htmlProjectStatus.Replace("#CompanyName", company);
                htmlProjectStatus = htmlProjectStatus.Replace("#AssignedTo",obj );

                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                if (ServerName  != "http://win2012srv:88/")
                {

                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                    string EmailAddress = "";

                    foreach (string mail in MailIds)
                    {
                        EmailAddress += mail + ",";
                    }
                    EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                    string CCMailId = "virupaksha.gaddad@dsrc.co.in";
                    string BCCMailId = "Kirankumar@dsrc.co.in ";

                    Task.Factory.StartNew(() =>
                    {
                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                        //var logo = odb.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                        //string[] words;
                        //words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                        //string pathvalue = "~/" + words[1];
                        string pathvalue = CommonLogic.getLogoPath();
                        DsrcMailSystem.MailSender.SendMail(null, paymentcreatemail.Subject + " - Test Mail Please Ignore", null, htmlProjectStatus + "Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(pathvalue));
                    });
                    return View();
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                        //DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                        //var words = logo.AppValue.Split(new string[] { "../../" }, StringSplitOptions.None);
                        //string pathvalue = "~/" + words[1];
                        string pathvalue = CommonLogic.getLogoPath();
                        DsrcMailSystem.MailSender.SendMail(null, paymentcreatemail.Subject, "", htmlProjectStatus, "HRMS@dsrc.co.in", paymentcreatemail.To,

                           paymentcreatemail.CC, paymentcreatemail.BCC, Server.MapPath(pathvalue));
                    });
                }
            }
            else
            {
               // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                ExceptionHandlingController.TemplateMissing("Create Project Payment", folder, ServerName);
            }
            return View();
        }
      


    }
}
