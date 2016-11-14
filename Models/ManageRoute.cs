using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class ManageRoute
    {
        [HiddenInput(DisplayValue = false)]
        public int RouteId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int VehicleId { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int StopId { get; set; }

        [DisplayName("Route Name")]
        public string RouteName { get; set; }
        [DisplayName("Vehicle No")]
        public string VehicleNumber { get; set; }
        [DisplayName("Stops")]
        public string Stops { get; set; }        

        public IEnumerable<SelectListItem> StopItemList { get; set; }
        public IEnumerable<string> stopList { get; set; }
        public ManageStops stopsmodel { get; set; }

    }

    public class ManageStops
    {
        [DisplayName("Location(Stop Name)")]
        public string StopName { get; set; }

        public int StopId { get; set; }
        public int vehicleId { get; set; }
        public int driverId { get; set; }

        [DisplayName("Vehicle No")]
        public string VehicleNo { get; set; }
        [DisplayName("Waiting time")]
        public string Waitingtime { get; set; }
        [DisplayName("Driver Name")]
        public string DriverName { get; set; }
        [DisplayName("Fees")]
        public string Fees { get; set; }
        [DisplayName("Trip No/count")]
        public string TripCount { get; set; }
    }
}