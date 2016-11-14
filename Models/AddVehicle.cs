using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class AddVehicle 
    {
        public int vehicleid { get; set; }
        public string Vehicle_No { get; set; }
        public string VehicleModel { get; set; }
        public int? VehicleModel_Id { get; set; }
        public int? VehicleBrand_Id { get; set; }
        public string VehicleBrand { get; set; }
        public int ?Model_Year{get;set;}
        public int ?No_of_Seat{get;set;}
        public int ?Trip{get;set;}
        public string Remarks{get;set;}
        public string VehicleType { get; set; }
        public int ?VehicleType_Id { get; set; }
        public long? Contact_No { get; set; }
        public string VehicleMake { get; set; }
        public string Vehicle_Photo { get; set; }
        public bool IsActive { get; set; }
        public string path { get; set; }
       
        public string DriverName { get; set; }
        public string Co_DriverName { get; set; }
    }
    //public class AddsVehicle
    //{
    //    public int vehicleid { get; set; }
    //    public string Vehicle_No { get; set; }
    //    public string VehicleModel { get; set; }
    //    public int? VehicleModel_Id { get; set; }
    //    public int? VehicleBrand_Id { get; set; }
    //    public string VehicleBrand { get; set; }
    //    public int? Model_Year { get; set; }
    //    public int? No_of_Seat { get; set; }
    //    public int? Trip { get; set; }
    //    public string Remarks { get; set; }
    //    public string VehicleType { get; set; }
    //    public int? VehicleType_Id { get; set; }
    //    public long? Contact_No { get; set; }
    //    public string VehicleMake { get; set; }
    //    public string Vehicle_Photo { get; set; }
    //    public bool IsActive { get; set; }

    //}
}
