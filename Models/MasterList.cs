using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class MasterList
    {

        public List<masterjoin> _masterjoin { get; set; }
        public string _drpMasterName { get; set; }        
        public List<string> id = new List<string>();
        public List<string> value = new List<string>();
        public List<Addmasterjoin> ColumnNames { get; set; }
        public List<Addmasterjoin> ColumnDataTypes { get; set; }
        public List<object> ColumnValues { get; set; }
        public List<string> ColumnName0 = new List<string>();
        public List<string> ColumnName1 = new List<string>();
        public Array val { get; set; }
        public List<string> ColumnDataType = new List<string>();
        //public List<string> ColumnName3 = new List<string>();
        //public List<string> ColumnName4 = new List<string>();

        public class masterdroplist
        {
             public string Name { get; set; }
            
        }
       
    }

    public   class BindColums
    {
        public string col1 { get; set; }
        public string col2 { get; set; }
        public  BindColums(string c1, string c2)
        {
            this.col1 = c1;
            this.col2 = c2;
        }
    }


    public class masterjoin
    {
        public string id { get; set; }
        public string name { get; set; }
        public masterjoin(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
    
    public class Addmasterjoin
    {
       
        public string ColumnName0 { get; set; }
        public string ColumnName1 { get; set; }
        public string ColumnDataType { get; set; }
     
        //public string ColumnName3 { get; set; }
        //public string ColumnName4 { get; set; }

        public Addmasterjoin(string ColumnName0, [Optional] string ColumnName1, [Optional]string ColumnDataType, [Optional]string ColumnName3, [Optional]string ColumnName4)
        {
            this.ColumnName0 = ColumnName0;
            this.ColumnName1 = ColumnName1;
            this.ColumnDataType = ColumnDataType;
      
            //this.ColumnName3 = ColumnName3;
            //this.ColumnName4 = ColumnName4;
            
        }
    }




  
}