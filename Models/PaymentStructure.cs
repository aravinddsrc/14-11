using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class PaymentStructure
    {
        public string groupicon { get; set; }
        public string CustomerName { get; set; }
        public string UserName { get; set; }
        public int? Userid { get; set; }
        public string Branch { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public int? GroupId { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public string CreatedBy { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string comments { get; set; }
        public int PaymentTypeID { get; set; } 
        public int? PaymentID { get; set; }
        public int PaymentTermID { get; set; }
        public double Amount { get; set; }
        public double? PaidAmount { get; set; }
        public double? PendingAmount { get; set; }
        public double AdditionalAmount { get; set; }
        public double OverAllAmount { get; set; }
        public double MonthlyAmount { get; set; }
        public double YearlyAmount { get; set; }
        public DateTime MonthlyDueDate { get; set; }
        public DateTime YearlyDueDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> TermAmount { get; set; }
        //public List<string> TotalTermAmount { get; set; }
        //public List<string> TotalTermDueDate { get; set; }
        public string TotalTermAmount { get; set; }
        public string TotalTermDueDate { get; set; }
        public string TotalTaxType { get; set; }
        public string TotalTaxAmount { get; set; }
        public List<string>MonthlyAdditionalAmount { get; set; }
        public List<string> YearlyAdditionalAmount { get; set; }
        public List<string> TermAdditionalAmount { get; set; }
        public List<PaymentStructure> Employeedetails { get; set; }
        public List<string> UserList { get; set; }
        public int dbvalue { get; set; }

    }

    
    public class UpdatePaymentStructure
    {
        public string CustomerName { get; set; }
        public string UserName { get; set; }
        public int? Userid { get; set; }
        public string Branch { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public int? GroupId { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public string CreatedBy { get; set; }
        public int StatusID { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string comments { get; set; }
        public int PaymentTypeID { get; set; }
        public int? PaymentID { get; set; }
        public int PaymentTermID { get; set; }
        public double Amount { get; set; }
        public double? PaidAmount { get; set; }
        public double? PendingAmount { get; set; }
        public double AdditionalAmount { get; set; }
        public double OverAllAmount { get; set; }
        public double MonthlyAmount { get; set; }
        public double YearlyAmount { get; set; }
        public DateTime MonthlyDueDate { get; set; }
        public DateTime YearlyDueDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> TermAmount { get; set; }
        //public List<string> TotalTermAmount { get; set; }
        //public List<string> TotalTermDueDate { get; set; }
        public string TotalTermAmount { get; set; }
        public string TotalTermDueDate { get; set; }
        public string TotalTaxType { get; set; }
        public string TotalTaxAmount { get; set; }
        public List<string> MonthlyAdditionalAmount { get; set; }
        public List<string> YearlyAdditionalAmount { get; set; }
        public List<string> TermAdditionalAmount { get; set; }

    }
    public class FilterCustomer
    {
        public int PaymentId { get; set; }
        public string CustomerName { get; set; }
        public int? DepartmentId { get; set; }
        public int? GroupId { get; set; }
    }
    static class ext
    {

        internal static List<Variance> DetailedCompare(UpdatePaymentStructure updated, PaymentStructure saved)
        {
            List<Variance> variances = new List<Variance>();
            var prop = updated.GetType().GetProperties();
            var prop1 = saved.GetType().GetProperties().Select(x => x.Name);
            prop = prop.Where(x => prop1.Contains(x.Name)).ToArray();
            foreach (PropertyInfo f in prop)
            {
                Variance v = new Variance();

                if (f.Name == "RoleName")
                    v.FieldName = "Role Name";
                else if (f.Name == "Marital")
                    v.FieldName = " Marital Status";
                else
                    v.FieldName = f.Name;

                v.UserUpdateValue = updated.GetType().GetProperty(f.Name).GetValue(updated, null);
                v.UserSaveValue = saved.GetType().GetProperty(f.Name).GetValue(saved, null);
                //if (f.Name == "TotalTermAmount" || f.Name == "TotalTermDueDate")
                //{
                //    if (updated.TotalTermAmount != null && updated.TotalTermDueDate != null &&
                //        saved.TotalTermAmount != null && saved.TotalTermDueDate != null)
                //    {
                //        string[] old = updated.TermAmount.ToArray();
                //        string[] upda = saved.TermAmount.ToArray();
                //        foreach (var a in old)
                //        {
                //            if (!(upda).Contains(a))
                //            {
                //                variances.Add(v);
                //            }
                //        }
                //    }
                //}

                if (v.UserUpdateValue != null && v.UserSaveValue != null)
                {
                    if (!v.UserUpdateValue.ToString().SequenceEqual(v.UserSaveValue.ToString()))
                        //if (!v.UserUpdateValue.ToString().Equals(v.UserSaveValue.ToString()))
                            variances.Add(v);
                    
                }
                
                else
                {
                    if (v.UserUpdateValue == null && v.UserSaveValue == null)
                    {
                    }
                    else
                    {
                        variances.Add(v);
                    }
                }

            }
            return variances;
        }
    }

}