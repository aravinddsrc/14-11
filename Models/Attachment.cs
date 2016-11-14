using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace DSRCManagementSystem.Models
{
    public class Attachment
    {
        public Stream fileStream { get; set; }

        public string fileName { get; set; }
    }
}