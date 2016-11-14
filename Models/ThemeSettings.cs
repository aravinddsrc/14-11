using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;
using DSRCManagementSystem.Models;

namespace DSRCManagementSystem.Models
{
    public class ThemeSettings
    {
   List<string> _images = new List<string>();

     public ThemeSettings()
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
        public int? WorkplaceId { get; set; }
        public int? Marital { get; set; }
        public int? Colors { get; set; }

        public string path { get; set; }

        [Required]
        //(ErrorMessage = "Enter AcademicStartdate")]
        //[Display(AcademicStartdate = "Enter AcademicStartdate")]
        public string AcademicStartdate { get; set; }

        [Required]
        //[Display( AcademicEnddate = "Enter AcademicStartdate")]
        public string AcademicEnddate { get; set; }

        [Required]
        //(ErrorMessage = "Enter FaceBook URL")]
        public string Facebook { get; set; }

        [Required]
        //(ErrorMessage = "Enter CompanyName")]
        public string CompanyName { get; set; }

        [Required]
        //(ErrorMessage = "Enter VersionNumber")]
        public string VersionNumber { get; set; }
          [Required]
        public string InTime { get; set; }
         [Required]
        public string OutTime { get; set; }

         
    }


    public class ImageModel
    {
        List<string> _images = new List<string>();

        public ImageModel()
        {
            _images = new List<string>();
        }

        public List<string> Images
        {
            get { return _images; }
            set { _images = value; }
        }
    }
     

    }

