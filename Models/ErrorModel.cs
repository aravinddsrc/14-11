using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class ErrorModel
    {
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public string Company { get; set; }
    }
}