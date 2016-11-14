using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.DSRCLogic
{
    public class DateComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return x.Date == y.Date;
        }

        public int GetHashCode(DateTime obj)
        {
            return obj.GetHashCode();
        }
    }
}