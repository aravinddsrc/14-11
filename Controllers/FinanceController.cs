using DSRCManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Controllers
{
    public class FinanceController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

        public ActionResult Finance()
        {
            try{
            var currentyear = DateTime.Now.Year;
            var calendarDetails = db.CalendarYears.FirstOrDefault();
            var calendarModel = new Calendar().GetCalendarDetails(currentyear, calendarDetails.StartingMonth ?? 1, calendarDetails.EndingMonth ?? 12);
            DateTime Start = calendarModel.StartDate;
            DateTime End = calendarModel.EndDate;
            var Income = db.Incomes.Where(x => x.IsActive == true && x.IncomeDate >= Start && x.IncomeDate <= End).Select(o => o.IncomeAmount).Sum();
            var Expenditure = db.Expenditures.Where(x => x.IsActive == true && x.ExpenseDate >= Start && x.ExpenseDate <= End).Select(o => o.ExpenseAmount).Sum();
           // var ProjectIncome = db.ProjectPayments.Where(x => x.IsActive == true).Select(o => o.Amount).Sum();
            var ProjectIncome = (from pp in db.ProjectPayments.Where(x => x.IsActive == true)
                                 join ppd in db.ProjectPaymetTermDetails.Where(x => x.DueDate.Value.Year == currentyear) on pp.PaymentID equals ppd.PaymentID into j
                                 select new
                                 {
                                     Amount = j.Sum(a => a.Amount)
                                 }).ToList();
            double sum = 0;
            foreach (var x in ProjectIncome)
            {
                sum += Convert.ToInt32(x.Amount);
            }
            
            var Income1 = Income == null ? 0 : Income;
            var Expenditure1 = Expenditure == null ? 0 : Expenditure;
            var ProjectIncome1 = sum == 0 ? 0 : sum;
            var Profit = (Income1 + ProjectIncome1 - Expenditure1);
            ViewBag.Profit = Profit == null ? 0 : Profit;

            string FinancialYear = Convert.ToString(currentyear);
            string FinancialYearEnd = Convert.ToString((currentyear + 1));
            FinancialYearEnd = FinancialYearEnd.Substring(2, 2);
            ViewBag.year = currentyear + "-" + FinancialYearEnd;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return View();
        }

        public ActionResult ChartData()
        {
            List<object> List = new List<object>();
            try{
            var MinDateIncome = (from d in db.Incomes select d.IncomeDate).ToList();
            var MaxDateIncome = (from d in db.Incomes select d.IncomeDate).ToList();
            var MinDateExpenditure= (from d in db.Expenditures select d.ExpenseDate).ToList();
            var MaxDateExpenditure = (from d in db.Expenditures select d.ExpenseDate).ToList();
            var MinDateIncomes = MinDateIncome.Count>0 ?MinDateIncome.Min().Value.Year :0 ;
            var MaxDateIncomes = MaxDateIncome.Count > 0 ? MaxDateIncome.Max().Value.Year :0 ;
            var MinDateExpenditures = MinDateExpenditure.Count > 0 ? MinDateExpenditure.Min().Value.Year :0 ;
            var MaxDateExpenditures = MaxDateExpenditure.Count > 0 ? MaxDateExpenditure.Max().Value.Year : 0 ;


            List<int> DateList = new List<int>();
            if (MinDateIncomes != 0)
            {
                DateList.Add(MinDateIncomes);
            }
            if (MaxDateIncomes != 0)
            {
                DateList.Add(MaxDateIncomes);
            }
            if (MinDateExpenditures != 0)
            {
                DateList.Add(MinDateExpenditures);
            }
            if (MaxDateExpenditures != 0)
            {
                DateList.Add(MaxDateExpenditures);
            }

            
            if (DateList.Count==0)
            {
                var MinDate = DateTime.Now.Year;
                var MaxDate = MinDate;
                for (var min = MinDate; min <= MaxDate; min++)
                {
                    var calendarDetails = db.CalendarYears.FirstOrDefault();
                    var calendarModel = new Calendar().GetCalendarDetails(min, calendarDetails.StartingMonth ?? 1,
                        calendarDetails.EndingMonth ?? 12);
                    DateTime Start = calendarModel.StartDate;
                    DateTime End = calendarModel.EndDate;
                    var Income =
                        db.Incomes.Where(x => x.IsActive == true && x.IncomeDate >= Start && x.IncomeDate <= End)
                            .Select(o => o.IncomeAmount)
                            .Sum();
                    var Expenditure =
                        db.Expenditures.Where(x => x.IsActive == true && x.ExpenseDate >= Start && x.ExpenseDate <= End)
                            .Select(o => o.ExpenseAmount)
                            .Sum();
                    var ProjectIncome = db.ProjectPayments.Where(x => x.IsActive == true).Select(o => o.Amount).Sum();
                    var Income1 = Income == null ? 0 : Income;
                    var Expenditure1 = Expenditure == null ? 0 : Expenditure;
                    var ProjectIncome1 = ProjectIncome == null ? 0 : ProjectIncome;
                    var Profit = (Income1 + ProjectIncome1 - Expenditure1);
                    var Val = new { y = min, b = Profit };
                    List.Add(Val);
                }
            
            }

            else
        
                if (DateList.Count != 0)
                {
                    var MinDate = DateList.Min();
                    var MaxDate = DateList.Max();
                    if (MinDate == 0 || MaxDate == 0)
                    {
                        MinDate = DateTime.Now.Year;
                        MaxDate = MinDate;
                        for (var min = MinDate; min <= MaxDate; min++)
                        {
                            var calendarDetails = db.CalendarYears.FirstOrDefault();
                            var calendarModel = new Calendar().GetCalendarDetails(min, calendarDetails.StartingMonth ?? 1,
                                calendarDetails.EndingMonth ?? 12);
                            DateTime Start = calendarModel.StartDate;
                            DateTime End = calendarModel.EndDate;
                            var Income =
                                db.Incomes.Where(x => x.IsActive == true && x.IncomeDate >= Start && x.IncomeDate <= End)
                                    .Select(o => o.IncomeAmount)
                                    .Sum();
                            var Expenditure =
                                db.Expenditures.Where(x => x.IsActive == true && x.ExpenseDate >= Start && x.ExpenseDate <= End)
                                    .Select(o => o.ExpenseAmount)
                                    .Sum();
                            var ProjectIncome = db.ProjectPayments.Where(x => x.IsActive == true).Select(o => o.Amount).Sum();
                            var Income1 = Income == null ? 0 : Income;
                            var Expenditure1 = Expenditure == null ? 0 : Expenditure;
                            var ProjectIncome1 = ProjectIncome == null ? 0 : ProjectIncome;
                            var Profit = (Income1 + ProjectIncome1 - Expenditure1);
                            var Val = new { y = min, b = Profit };
                            List.Add(Val);
                        }

                    }
                    else
                    {
                        for (var min = MinDate; min <= MaxDate; min++)
                        {
                            var calendarDetails = db.CalendarYears.FirstOrDefault();
                            var calendarModel = new Calendar().GetCalendarDetails(min, calendarDetails.StartingMonth ?? 1,
                                calendarDetails.EndingMonth ?? 12);
                            DateTime Start = calendarModel.StartDate;
                            DateTime End = calendarModel.EndDate;
                            var Income =
                                db.Incomes.Where(x => x.IsActive == true && x.IncomeDate >= Start && x.IncomeDate <= End)
                                    .Select(o => o.IncomeAmount)
                                    .Sum();
                            var Expenditure =
                                db.Expenditures.Where(x => x.IsActive == true && x.ExpenseDate >= Start && x.ExpenseDate <= End)
                                    .Select(o => o.ExpenseAmount)
                                    .Sum();
                            // var ProjectIncome = db.ProjectPayments.Where(x => x.IsActive == true).Select(o => o.Amount).Sum();
                            var ProjectIncome = (from pp in db.ProjectPayments.Where(x => x.IsActive == true)
                                                 join ppd in db.ProjectPaymetTermDetails.Where(x => x.DueDate.Value.Year == min) on pp.PaymentID equals ppd.PaymentID into j
                                                 select new
                                                 {
                                                     Amount = j.Sum(a => a.Amount)
                                                 }).ToList();
                            double sum = 0;
                            foreach (var x in ProjectIncome)
                            {
                                sum += Convert.ToInt32(x.Amount);
                            }
                            var Income1 = Income == null ? 0 : Income;
                            var Expenditure1 = Expenditure == null ? 0 : Expenditure;
                            var ProjectIncome1 = sum == null ? 0 : sum;
                            var Profit = (Income1 + ProjectIncome1 - Expenditure1);
                            var Val = new { y = min, b = Profit };
                            List.Add(Val);

                        }
                    }
            }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);

            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }

    }
}
