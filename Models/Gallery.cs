using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DSRCManagementSystem.Models
{
    public class Gallery
    {
        
        public string AlbumTitle { get; set; }
        public string AlbumDescription { get; set; }
        public string CoverPhotoPath { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int AlbumID { get; set; }
        public string Album { get; set; }
        public int AlbumPhotoID { get; set; }
        public string AlbumVisibleTo { get; set; }
        public int Photocount { get; set; }
        public int? RoleID { get; set; }
        public int count { get; set; }
        public string IsAccess { get; set; }
        public string AlbumRole { get; set; }
        public string AlbumUser {get;set;}
        public List<int> AlbumAccessRoles { get; set; }
        public List<int> AlbumAccessUsers { get; set; }
        public HttpPostedFileBase Photo { get; set; }
        public IEnumerable<string> Images { get; set; }
        public string[] AlbumExist { get; set; }
        public string src { get; set; }
        public string title { get; set; }
        public DateTime EventDate { get; set; }
        [Display(Name="Tag Users")]
        public string TagUsers { get; set; }
    }
    public class AccessRoles
    {

        public int? RoleID { get; set; }
    }

    public class AccessUsers
    {

        public int? UserID { get; set; }
    }
    public class Albums
    {

        public int? albumid { get; set; }
    }
    

  
}