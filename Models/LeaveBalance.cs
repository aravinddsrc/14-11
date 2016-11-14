using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DSRCManagementSystem.DSRCLogic;

namespace DSRCManagementSystem.Models
{
    public class LeaveBalance
    {
        public string Academicyear { get; set; }
        public int Year { get; set; }
        public string AcadamicYear { get; set; }
        public int count { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        //public int DaysAllowed { get; set; }    /*All Integer variables are changed for half day leave management*/
        public double DaysAllowed { get; set; }
        public double LeaveDaysUsed { get; set; }
        public bool CalculateLeave { get; set; }
        public string ApplicableEmployees { get; set; }
        //public int UsedDays { get; set; }
        public double UsedDays { get; set; }
        public int Availabledays { get; set; }
        //public int RemainingDays

        public double RemainingDays
        {
            get
            {
                return DaysAllowed - UsedDays;
            }
        }    

        public double GetLeaveBalance(List<LeaveRequest> leaveRequests, List<DateTime?> holidays, DateTime calendarStartDate, DateTime calendarEndDate)
        {
            var totalLeaveDays = 0.0;

            var calculateLeaveDays = false;
            
            foreach (var leaveDay in leaveRequests.AsEnumerable())
            {
                DateTime calStartDate;
                
                if (leaveDay.StartDateTime < calendarStartDate && leaveDay.EndDateTime > calendarStartDate)
                {
                    calStartDate = calendarStartDate;
                    calculateLeaveDays = true;
                }
                else
                {
                    calStartDate = leaveDay.StartDateTime ?? DateTime.Now;
                }

                DateTime calEndDate;

                if (leaveDay.EndDateTime > calendarEndDate && leaveDay.StartDateTime < calendarEndDate)
                {
                    calEndDate = calendarEndDate;
                    calculateLeaveDays = true;
                }
                else
                {
                    calEndDate = leaveDay.EndDateTime ?? DateTime.Now;
                }

                if (calculateLeaveDays)
                {
                    totalLeaveDays += CalculateLeaveDays(calStartDate, calEndDate, holidays).LeaveDays;
                }
                else
                {
                    totalLeaveDays = totalLeaveDays + leaveDay.LeaveDays ?? 0.0;
                }
            }

            return totalLeaveDays;
        }

        public LeaveDaysDetails CalculateLeaveDays(DateTime startDate, DateTime endDate, List<DateTime?> holidayList)
        {
            DateTime endDateFinal = new DateTime();
            
            double leaveDays = 0;
            DateTime date = startDate;

            while (date <= endDate)
            {
                if (!holidayList.Contains(date) && (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    if (date.Date != startDate.Date)
                    {
                        date = date.Date.Add(new TimeSpan(9, 00, 00));
                    }
                    
                    var hoursTaken = new TimeSpan(18, 00, 00);
                    hoursTaken = date.Date == endDate.Date ? endDate.TimeOfDay.Subtract(date.TimeOfDay) : hoursTaken.Subtract(date.TimeOfDay);

                    leaveDays += hoursTaken.Hours > 5 ? 1 : 0.5;
                    endDateFinal = date;
                }
                date = date.Date.Add(new TimeSpan(24, 00, 00));
            }
            //return leaveDays;
            return new LeaveDaysDetails() { LeaveDays = leaveDays, EndDate = endDateFinal };
        }

    }


    public class Calendar
    {
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DateTime?> Holidays { get; set; }

        public Calendar GetCalendarDetails(int year, int startingMonth, int endingMonth)
        {
            int endingYear = startingMonth == 1 ? year : year+1;
            return new Calendar() { Year = year, StartDate = new DateTime(year, startingMonth, 01), EndDate = new DateTime(endingYear, (endingMonth), 01).AddMonths((1)).AddSeconds((-1)) };
        }
    }

    public class EmployeeLeaveBalance
    {
        public int Year { get; set; }
        public string EmployeeId { get; set; }
        public int UserID { get; set; }
        public string EmployeeName { get; set; }
        public List<LeaveBalance> LeaveTypeBalances { get; set; }
    }

    public class LeaveDaysDetails
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double LeaveDays { get; set; }
    }

    public class LeaveDetails
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Detail { get; set; }
        public int Leavetypeid { get; set; }
        public string WorkedDate { get; set; }
        public double? Sick { get; set; }
        public double? Casual { get; set; }
        public double? Earned { get; set; }
        public double? LeaveDays { get; set; }
        public double? LOP { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
    }
    public class Notification
    {
        public int NotifyCount { get; set; }
        public List<NotificationLeaveDetails> Values { get; set; }
    }
    public class NotificationLeaveDetails
    {
        public string UserName { get; set; }
        public DateTime?  RequestedDateTime { get; set; }
        public string Time { get; set; }

    }
}