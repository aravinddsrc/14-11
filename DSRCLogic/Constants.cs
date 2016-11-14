using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.DSRCLogic
{
    public enum Month
    {
        January = 1, February, March, April, May, June, July, August, September, October, November, December
    }

    public class Constants
    {

    }

    public class YearDropDown
    {
        public int YearId { get; set; }
        public string Year { get; set; }
    }

    public class MonthDropDown
    {
        public int MonthId { get; set; }
        public string Month { get; set; }
    }
}