using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Web;
using DSRCManagementSystem.DSRCLogic;

namespace DSRCManagementSystem.Models.Domain_Models
{
    public class CalendarYearModel
    {
        [Required]
        public byte CalendarYearId { get; set; }
        [Required]
        public Month StartingMonth { get; set; }
        [Required]
        public Month EndingMonth { get; set; }

        public CalendarYearModel()
        {
        }
    }

    public class CalendarYearRepository
    {
        public static List<CalendarYear> GetCalendarYears()
        {
            return new DSRCManagementSystemEntities1().CalendarYears.ToList();
        }
        public static CalendarYear GetCalendarYear(int calendarYearId)
        {
            return new DSRCManagementSystemEntities1().CalendarYears.First(item => item.CalendarYearId == calendarYearId);
        }
    }




}