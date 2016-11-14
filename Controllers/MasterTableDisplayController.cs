
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Remoting.Contexts;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Web.UI;
using System.Runtime.InteropServices;
using System.Configuration;


namespace DSRCManagementSystem.Controllers
{
    public class MasterTableDisplayController : Controller
    {
      
        [HttpGet]
        public ActionResult MasterDropTable()
        {


            DataTable dt = new DataTable();
            AllMail.MasterList(dt);

            List<DSRCManagementSystem.Models.MasterList.masterdroplist> objmodel = new List<Models.MasterList.masterdroplist>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DSRCManagementSystem.Models.MasterList.masterdroplist obj = new DSRCManagementSystem.Models.MasterList.masterdroplist();
                obj.Name = dt.Rows[i]["name"].ToString();
                objmodel.Add(obj);
            }
            SelectList list = new SelectList(objmodel, "name", "name");
            ViewBag.MasterTable = list;
            TempData["message"] = "Addeds";
            return View();
        }

        MasterList cs = new MasterList();      
        [HttpPost]
        public ActionResult MasterDropTable(DSRCManagementSystem.Models.MasterList Name)
        {
            
            if (Name._drpMasterName == null)
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var selectedTable = Name._drpMasterName;

                Session["selectmasterTableName"] = selectedTable;
                Session["masterTableName"] = Name._drpMasterName;
                var selectMaster = db.Sp_Master(selectedTable).ToList();
                var ColumnName = db.Sp_GetColumn(selectedTable).ToList();
                var columndatatype = db.Sp_MasterDataType(selectedTable).ToList();
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();

                ViewBag.columndatatype = columndatatype;
                ViewBag.ColumnName = ColumnName;

                for (int i = 0; i <= columndatatype.Count - 1; i++)
                {
                    cs.ColumnDataType.Add(columndatatype[i]);
                    Name.ColumnDataTypes.Add(new Addmasterjoin(columndatatype[i]));
                }

                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }

                Name._masterjoin = new List<masterjoin>();
                foreach (var s in selectMaster)
                {
                    var rs = s.Split('^').ToArray();
                    cs.id.Add(rs[0]);
                    cs.value.Add(rs[1]);
                    Name._masterjoin.Add(new masterjoin(rs[0].ToString(), rs[1].ToString()));
                }

                DataTable dt = new DataTable();
                AllMail.MasterList(dt);

                List<DSRCManagementSystem.Models.MasterList.masterdroplist> objmodel = new List<Models.MasterList.masterdroplist>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DSRCManagementSystem.Models.MasterList.masterdroplist obj = new DSRCManagementSystem.Models.MasterList.masterdroplist();
                    obj.Name = dt.Rows[i]["name"].ToString();
                    objmodel.Add(obj);
                }
                SelectList list = new SelectList(objmodel, "name", "name");
                ViewBag.MasterTable = list;
                TempData["message"] = "Added";
               // return View("<script language='javascript' type='text/javascript'>alert('Select MasterTableName ');</script>");
               // return Content("<script language='javascript' type='text/javascript'>alert('Select MasterTableName ');</script>");
                return View();
            }
            else
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

                var selectedTable = Name._drpMasterName;

                Session["selectmasterTableName"] = selectedTable;
                Session["masterTableName"] = Name._drpMasterName;
                var selectMaster = db.Sp_Master(selectedTable).ToList();
                var ColumnName = db.Sp_GetColumn(selectedTable).ToList();
                var columndatatype = db.Sp_MasterDataType(selectedTable).ToList();
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();
                ViewBag.columndatatype = columndatatype;
                ViewBag.ColumnName = ColumnName;

                for (int i = 0; i <= columndatatype.Count - 1; i++)
                {
                    cs.ColumnDataType.Add(columndatatype[i]);
                    Name.ColumnDataTypes.Add(new Addmasterjoin(columndatatype[i]));
                }

                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }

                Name._masterjoin = new List<masterjoin>();
                foreach (var s in selectMaster)
                {
                    var rs = s.Split('^').ToArray();
                    cs.id.Add(rs[0]);
                    cs.value.Add(rs[1]);
                    Name._masterjoin.Add(new masterjoin(rs[0].ToString(), rs[1].ToString()));
                }

                DataTable dt = new DataTable();
                AllMail.MasterList(dt);

                List<DSRCManagementSystem.Models.MasterList.masterdroplist> objmodel = new List<Models.MasterList.masterdroplist>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DSRCManagementSystem.Models.MasterList.masterdroplist obj = new DSRCManagementSystem.Models.MasterList.masterdroplist();
                    obj.Name = dt.Rows[i]["name"].ToString();
                    objmodel.Add(obj);
                }
                SelectList list = new SelectList(objmodel, "name", "name");
                ViewBag.MasterTable = list;
                return View(Name);
            }
           
        }


        public ActionResult AddMaster(DSRCManagementSystem.Models.MasterList Name, string result,string z)
        {
             DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();

            if (Convert.ToString(Session["selectmasterTableName"]) == "--Select--" || Session["selectmasterTableName"] == null)
            {

               Response.Write("<script language=javascript>alert('Please Select the table Name');</script>");              
               return View("MasterView");
            
            }
            else
            {
                var count = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).Count();
                var ColumnName = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).ToList();
                var columnDatatype = db.Sp_MasterDataType(Session["selectmasterTableName"].ToString()).ToList();
                ViewBag.columndatatype=columnDatatype;
                ViewBag.ColumnName = ColumnName;
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();
                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }
                for (int i = 1; i <= columnDatatype.Count - 1; i++)
                {
                    cs.ColumnDataType.Add(columnDatatype[i]);
                    Name.ColumnDataTypes.Add(new Addmasterjoin(columnDatatype[i]));
                }
                ViewBag.count = count-1;
                return View(Name);
            }

        }

        [HttpPost]
        public ActionResult AddMaster(DSRCManagementSystem.Models.MasterList Name,string Column)
        {
            try
            {
                var values = "";

                List<string> objuser = new List<string>();
                string[] value = Column.Split(',');
                for (int k = 0; k < value.Count(); k++)
                {
                    if (value[k] != "")
                    {
                        objuser.Add(value[k]);
                    }
                }


                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var TableName = Session["selectmasterTableName"];


                var ColumnName = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).ToList();
                var columnDatatype = db.Sp_MasterDataType(Session["selectmasterTableName"].ToString()).ToList();
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();
                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }

                for (int i = 0; i < objuser.Count(); i++)
                {
                    
                        if (columnDatatype[i+1] == "datetime")
                        {
                            values = values + (values == string.Empty ? values.Trim() : ",") + "" + objuser[i].ToString() + "";
                        }
                        else
                        {
                            values = values + (values == string.Empty ? values.Trim() : ",") + "'" + objuser[i].ToString() + "'";
                        }
                   
                   
                }

                string cmdstr = string.Format("INSERT INTO " + TableName + " VALUES ({0})", values);
                string constr = ConfigurationManager.AppSettings["connstr"];

                SqlConnection objcon = new SqlConnection(constr);

                objcon.Open();
                SqlCommand cmd = new SqlCommand(cmdstr, objcon);
                cmd.ExecuteNonQuery();
                objcon.Close();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult EditMaster(DSRCManagementSystem.Models.MasterList Name,string uid,string z)
        {
            var col="";
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int id =Convert.ToInt16(uid);
            Session["id"] = Convert.ToInt16(uid);
            var TableName = Session["selectmasterTableName"];
            var ColumnName = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).ToList();
            var columnDatatype = db.Sp_MasterDataType(Session["selectmasterTableName"].ToString()).ToList();
            Name.ColumnNames = new List<Addmasterjoin>();
            Name.ColumnDataTypes = new List<Addmasterjoin>();
            for (int s = 0; s <= ColumnName.Count - 1; s++)
            {
                cs.ColumnName0.Add(ColumnName[s]);
                col = ColumnName[0];
                Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
            }
            for (int i = 1; i <= columnDatatype.Count - 1; i++)
            {
                cs.ColumnDataType.Add(columnDatatype[i]);               
                Name.ColumnDataTypes.Add(new Addmasterjoin(columnDatatype[i]));
            }

            string EditQuery = "select * from " + TableName + " where " + col +" = "+ id;
            string constr = ConfigurationManager.AppSettings["connstr"];

            SqlConnection objcon = new SqlConnection(constr);
            objcon.Open();
            SqlCommand cmd = new SqlCommand(EditQuery, objcon);
            cmd.ExecuteNonQuery();
            objcon.Close();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            Name.ColumnValues = new List<object>();
            DataSet ds = new DataSet();
            sda.Fill(ds,"dinesh");


            for (int i = 0; i < columnDatatype.Count; i++)
            {

            foreach (DataRow dr in ds.Tables["dinesh"].Rows)
            {
                
                    DSRCManagementSystem.Models.MasterList ob = new DSRCManagementSystem.Models.MasterList();

                    Array values = dr.ItemArray;
                    ob.val = values;

                    Name.ColumnValues.Add(ob.val.GetValue(i));
                }
            }

            return View(Name);
        }


        [HttpPost]
        public ActionResult EditMaster(DSRCManagementSystem.Models.MasterList Name, string ColumnValues)
        {

            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
               
                string values = string.Empty;
                List<string> column = new List<string>();
                List<string> objuser = new List<string>();

                

                int ids =Convert.ToInt16(Session["id"]);


                string[] value = ColumnValues.Split(',');
                for (int k = 0; k < value.Count(); k++)
                {
                    if (value[k] != "")
                    {
                        objuser.Add(value[k].Replace(",", "''"));
                    }
                }
                for (int i = 0; i < objuser.Count(); i++)
                {
                    values = values + (values == string.Empty ? values.Trim() : ",") + "'" + objuser[i].ToString() + "'";

                }
               
                var col = "";
                var TableName = Session["selectmasterTableName"];
                var ColumnName = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).ToList();



                for (int i = 1; i < ColumnName.Count; i++)
                {

                    //column = column + (column == string.Empty ? column.Trim() : ",") + "'" + ColumnName[i].ToString() + "'";
                    column.Add(ColumnName[i]);


                }

                string[] cols = column.ToArray();



                //string convToSingleData = String.Join(",", column.Select(x=>x.ToString()));



                var columnDatatype = db.Sp_MasterDataType(Session["selectmasterTableName"].ToString()).ToList();
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();
                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    col = ColumnName[0];
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }
                for (int i = 1; i <= columnDatatype.Count - 1; i++)
                {
                    cs.ColumnDataType.Add(columnDatatype[i]);
                    Name.ColumnDataTypes.Add(new Addmasterjoin(columnDatatype[i]));
                }
                string dd = string.Empty;
                for (int j = 0; j < ColumnName.Count-1; j++)
                {
                    try
                    {
                        if (j != ColumnName.Count - 2)
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], " " + Convert.ToInt32(objuser[j]) + " ") + ",";
                        }
                        else if (columnDatatype[j + 1] == "datetime")
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], " " + objuser[j] + " ") + "";
                        }
                        else
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], " " + Convert.ToInt32(objuser[j]) + " ") + "";
                        }
                    }
                    catch (Exception)
                    {
                        if (j != ColumnName.Count - 2)
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], "' " + objuser[j] + " '") + ",";
                        }
                        else if (columnDatatype[j + 1] == "datetime")
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], " " + objuser[j] + " ") + "";
                        }
                        else
                        {
                            dd += column[j] + " = " + objuser[j].Replace(objuser[j], "' " + objuser[j] + " '") + "";
                        }
                    }
                    
                    
                }


               // string replaced = "," + dd.Replace(",", "','") + "'";


                string UpdateQuery = "update " + TableName + " set "+ dd +" where " + col + " = " + ids;
               // string UpdateQuery = "update " + TableName + " set " + column + " = " + values + " where " + col + " = " + ids + "";
                //string UpdateQuery = " UPDATE " + TableName + " VALUES " + values + " WHERE " + col + " = " + ids;
                string constr = ConfigurationManager.AppSettings["connstr"];

                SqlConnection objcon = new SqlConnection(constr);
                objcon.Open();
                SqlCommand cmd = new SqlCommand(UpdateQuery, objcon);
                cmd.ExecuteNonQuery();
                objcon.Close();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);

            }

        }
        public ActionResult DeleteMaster(DSRCManagementSystem.Models.MasterList Name,string uid)
        {
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                var TableName = Session["selectmasterTableName"];
                var ColumnName = db.Sp_GetColumn(Session["selectmasterTableName"].ToString()).ToList();
                var columnDatatype = db.Sp_MasterDataType(Session["selectmasterTableName"].ToString()).ToList();
                Name.ColumnNames = new List<Addmasterjoin>();
                Name.ColumnDataTypes = new List<Addmasterjoin>();
                var col = "";
                for (int s = 0; s <= ColumnName.Count - 1; s++)
                {
                    cs.ColumnName0.Add(ColumnName[s]);
                    col = ColumnName[0];
                    Name.ColumnNames.Add(new Addmasterjoin(ColumnName[s]));
                }
                for (int i = 1; i <= columnDatatype.Count - 1; i++)
                {
                    cs.ColumnDataType.Add(columnDatatype[i]);
                    Name.ColumnDataTypes.Add(new Addmasterjoin(columnDatatype[i]));
                }



                string DeleteMasterTableRow = "delete from " + TableName + " where " + col + " = " + uid;
                string constr = ConfigurationManager.AppSettings["connstr"];

                SqlConnection objcon = new SqlConnection(constr);
                objcon.Open();
                SqlCommand cmd = new SqlCommand(DeleteMasterTableRow, objcon);
                cmd.ExecuteNonQuery();
                objcon.Close();
                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { Result = "Already", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
