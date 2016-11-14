using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class Transportation
    {
        public int? Id { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleMake { get; set; }
        public  int ?YearsofManufacturing { get; set; }
        public string ExpenseType { get; set; }
        public int? Cost { get; set; }
        public int? ExpenseId { get; set; }
        public string date { get; set; }
        public DateTime? dateofyear { get; set; }
        public int? ManageExpenseId { get; set; }
    }
}