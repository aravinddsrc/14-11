using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class RemainderService
    {

        public DateTime SelectDate { get; set; }
        public string Days { get; set; }
        public int RemainderServiceID { get; set; }

        public int ServiceIDPost { get; set; }

        public string TypePost { get; set; }

        public string DataPost { get; set; }

        public string NextDateCalculation { get; set; }

        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Template { get; set; }

        public string Subject { get; set; }

        public string EventMethodName { get; set; }

        public IEnumerable<SelectListItem> DaysCheck { get; set; }
        
        [Display(Name = "Select Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy hh:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime ServiceCalendarFrom { get; set; }


        [Display(Name = "Select Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy hh:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime ServiceCalendarTo { get; set; }

        [Display(Name = "Select Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy hh:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime ServiceCalendar { get; set; }

        public string ServiceStartTime { get; set; }
        public string ServiceStatus { get; set; }

        public string TimeSlotFrom { get; set; }

        public string ServiceName { get; set; }

        public int ID{get;set;}

        public string Name{get;set;}

        public bool IsChecked { get; set; }

        public string ServiceTypeGet { get; set; }

               
    }

    public class RemainderPurpose
    {  
        public int Id { get; set; }
       // public string ServiceType { get; set; }
        public string ServiceData { get; set; }

        public bool CheckDays { get; set; }

        public string ServiceType { get; set; }
        public int ServiceHour { get; set; }
                
        public DateTime SelectDate { get; set; }
        public string Days { get; set; }

        public string EventMethodName { get; set; }
        
        //[ReadOnly(true)]
        public bool CheckHistory { get; set; }

        [ReadOnly(true)]
        public bool checkMonday { get; set;}
        [ReadOnly(true)]
        public bool checkTuesday { get; set; }
        [ReadOnly(true)]
        public bool checkWednesday { get; set; }
        [ReadOnly(true)]
        public bool checkThursday { get; set; }
        [ReadOnly(true)]
        public bool checkFriday { get; set; }
        [ReadOnly(true)]
        public bool checkSaturday { get; set; }
        [ReadOnly(true)]
        public bool checkSunday { get; set; }

        public string ServiceName { get; set; }

        public bool OnServiceStatus { get; set; }

        public string ServiceStartTime { get; set; }

        public string ServiceStatus { get; set; }

        public int Mas_RemainderServiceID { get; set; }
        public string Mas_ServiceType { get; set; }

      //  public List<RemainderPurpose> ActiveDays { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Template { get; set; }
        public string Subject { get; set; }
    
    }

    public class ReminderServiceProcess
    {
        public int Id { get; set; }
        // public string ServiceType { get; set; }
        public string ServiceData { get; set; }

        public bool CheckDays { get; set; }

        public string ServiceType { get; set; }
        public int ServiceHour { get; set; }

        public DateTime SelectDate { get; set; }
        public string Days { get; set; }

        public string EventMethodName { get; set; }

        //[ReadOnly(true)]
        public bool CheckHistory { get; set; }

        [ReadOnly(true)]
        public bool checkMonday { get; set; }
        [ReadOnly(true)]
        public bool checkTuesday { get; set; }
        [ReadOnly(true)]
        public bool checkWednesday { get; set; }
        [ReadOnly(true)]
        public bool checkThursday { get; set; }
        [ReadOnly(true)]
        public bool checkFriday { get; set; }
        [ReadOnly(true)]
        public bool checkSaturday { get; set; }
        [ReadOnly(true)]
        public bool checkSunday { get; set; }

        public string ServiceName { get; set; }

        public bool OnServiceStatus { get; set; }

        public string ServiceStartTime { get; set; }

        public string ServiceStatus { get; set; }

        public int Mas_RemainderServiceID { get; set; }
        public string Mas_ServiceType { get; set; }

        //  public List<RemainderPurpose> ActiveDays { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Template { get; set; }
        public string Subject { get; set; }
    
        //BirthdayReminder
        public DateTime BirthdayDate { get; set; }
        public string BirthdayUserName { get; set; }
        public string LastName { get; set; }
        public string BirthdayEmailId { get; set; }
        public string EmpID { get; set; }


        //ProjectSummary
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public string StatusComments { get; set; }


        //EmailHistory

        public string EmailEmpID { get; set; }
        public string EmailServiceType { get; set; }
        public string EmailUserName { get; set; }
        public string Email_EmailAddress { get; set; }
        public string EmailStatus { get; set; }

    }


}
