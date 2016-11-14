using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DSRCManagementSystem.Models
{
    public class QuickLinks
    {
        List<string> _images = new List<string>();

        public QuickLinks()
    {
        _images = new List<string>();
        
    }
     public List<string> Images
     {
         get { return _images; }
         set { _images = value; }
     }

        public HttpPostedFileBase Photo { get; set; }
        public byte[] Image { get; set; }
        public bool HasImage { get; set; }
        public int QuickLink { get; set; }
        public string path { get; set; }
        [Display(Name = "Menu Name")]
        public byte? FunctionID { get; set; }
        public List<byte?> Functions { get; set; }
        public string FunctionName { get; set; }
        public string DisplayName { get; set; }
        public string ModuleName { get; set; }
        public int QuickLinkID { get; set; }
        public int userId { get; set; }
        public byte? PageModuleID { get; set; }
        public string pathName { get; set; }

        public List<Int32?> selectedQuickLinkId { get; set; }
        public List<string> selectedPageModuleId{ get; set; }
        public List<MenuList> MenuList { get; set; }
        public List<QuickLinks> Menu { get; set; }
        public List<QuickLinks> links { get; set; }
    }
    public class Usermenu
    {
        public byte? FunctionId { get; set; }
        public byte? PageModuleId { get; set; }
        public int QuickLinkId { get; set; }
    
    }

   
}