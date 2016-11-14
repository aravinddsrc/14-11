using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models.Domain_Models
{
    public class LeaveTypeModel
    {
        public static bool IsEmployeeEligibleForLeave(string leaveType, int userId)
        {
            DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1();
            bool isEligible = true;

            if (leaveType.Equals("Earned", StringComparison.OrdinalIgnoreCase))
            {
                var doj = dbHrms.Users.FirstOrDefault(item => item.UserID == userId).DateOfJoin;
                if (doj == null)
                    return false;
                var completedDays = (DateTime.Now - doj).Value.Days;
                if (completedDays < 365)
                {
                    isEligible = false;
                }
            }
            return isEligible;
        }
    }
}