using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.DSRCLogic
{
    public class Encrypter
    {
        public static string Encode(string Id)
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(Id);
            return Convert.ToBase64String(encoded);
        }

        public  static string Decode(string Id)
        {
            byte[] encoded = Convert.FromBase64String(Id);
            return System.Text.Encoding.UTF8.GetString(encoded);
        }





    }
}