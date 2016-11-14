using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class ManageDrivers
    {
        
         public int DriverId {get;set;}
         public string First_Name              {get;set;}
         public string Last_Name { get; set; }

        public HttpPostedFileBase Pictures { get; set; }
         public byte[] Picture { get; set; }

         public string DriverName { get; set; }
         public DateTime?  DOB { get; set; }
        public int ? Gender                      {get;set;}
        public string Genders { get; set; }
        public string Driver_Licence_No      {get;set;}
        public DateTime? Driver_Licence_Expire_Date { get; set; }
        public int ? DriverType_Id               {get;set;}
        public string DriverType { get; set; }
        public string Email_Id               {get;set;}
        public long? Contact_No                  {get;set;}
        public string Blood_Group            {get;set;}
        public string Communication_Address  {get;set;}
        public string Driver_Batch_No        {get;set;}

        public byte[] Document_Proof         {get;set;}
        public HttpPostedFileBase Documents { get; set; }
    }
}