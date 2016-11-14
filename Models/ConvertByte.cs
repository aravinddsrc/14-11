using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace DSRCManagementSystem.Models
{
    public class ConvertByte
    {
        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {

            if (image != null)
            {
                byte[] imageBytes = null;
                BinaryReader reader = new BinaryReader(image.InputStream);
                imageBytes = reader.ReadBytes((int)image.ContentLength);
                return imageBytes;
            }
            else
            {
                return null;

            }
        }
    }
}