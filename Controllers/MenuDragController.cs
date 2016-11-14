using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using System.Text.RegularExpressions;

namespace DSRCManagementSystem.Controllers
{
    public class MenuDragController : Controller
    {
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        [HttpGet]
        public ActionResult MenuDrag()
        {

            List<MenuDrag> Functions = new List<MenuDrag>();
            List<DSRCManagementSystem.Models.MenuDrag> objmodel = new List<Models.MenuDrag>();
            MenuDrag MainMenus = new MenuDrag();
            MainMenus.Children = new List<MenuListItem>();
            int userId = int.Parse(Session["UserID"].ToString());
            var RoleID = db.UserRoles.Where(x => x.UserID == userId).Select(o => o.RoleID).FirstOrDefault();

            MenuListItem MainMenu = new MenuListItem();
            var temp = db.GetMenuListByClientIdRoleID(userId, RoleID).ToList();
            MainMenu.Children = new List<MenuListItem>();
            foreach (MenuListByClientIdRoleID menu in temp)
            {
                MenuListItem Function;
                MenuListItem Module;
                if (String.IsNullOrEmpty(menu.PageUrl)) menu.PageUrl = "#";

                Function = MainMenu.Children.Where(x => x.MenuName.Equals(menu.FunctionName)).FirstOrDefault();
                if (Function == null) Function = new MenuListItem() { MenuName = menu.FunctionName, MenuIcon = menu.ModuleIcon, Children = new List<MenuListItem>() };

                if (String.IsNullOrEmpty(menu.ModuleName))
                {



                    Function.Url = menu.PageUrl;
                    Function.Children = null;
                }
                else
                {
                    Function.Url = "javascript:;";
                    Module = Function.Children.Where(x => x.MenuName.Equals(menu.ModuleName)).FirstOrDefault();
                    if (Module == null) Module = new MenuListItem() { MenuName = menu.ModuleName, MenuIcon = menu.SubModuleIcon, Children = new List<MenuListItem>() };

                    if (String.IsNullOrEmpty(menu.SubModuleName))
                    {
                        Module.Url = menu.PageUrl;
                        Module.Children = null;
                    }
                    else
                    {
                        Module.Url = "javascript:;";
                        Module.Children.Add(new MenuListItem() { MenuName = menu.SubModuleName, MenuIcon = menu.SubMenuIcon, Url = menu.PageUrl, Children = null });
                    }

                    if (Function.Children.Where(x => x.MenuName.Equals(Module.MenuName)).FirstOrDefault() == null) Function.Children.Add(Module);
                }

                if (MainMenu.Children.Where(x => x.MenuName.Equals(Function.MenuName)).FirstOrDefault() == null)
                {
                    MainMenu.Children.Add(Function);
                    MainMenus.Children.Add(Function);

                }
            }


            var FunctionName = new List<string>();
            List<string> Value = new List<string>();

            foreach (var x in MainMenus.Children)
            {
                DSRCManagementSystem.Models.MenuDrag ob = new DSRCManagementSystem.Models.MenuDrag();
                var Function = x.MenuName;
                ob.FunctionName = Function;
                var id = db.Functions.Where(o => o.FunctionName == Function).Select(a => a.FunctionID).FirstOrDefault();
                ob.FunctionID = id;
                objmodel.Add(ob);


            }
            return View(objmodel);
        }


        [HttpPost]
        public ActionResult MenuDrag(MenuDrag Model, string Ids, string Ids1, int RoleIDS, string idsSUB, string ids1SUB)
        {
            try
            {
                int userId = int.Parse(Session["UserID"].ToString());
                var RoleID = db.UserRoles.Where(x => x.UserID == userId).Select(o => o.RoleID).FirstOrDefault();
               
                
                //////////////////MODULE
                List<string> ListTRIM = new List<string>();
                var SUB = ids1SUB.Split('$');

                foreach (var trim in SUB)
                {
                    var TRIM = trim.TrimEnd(',');
                    var TRIMFINAL = TRIM.TrimStart(',');
                    ListTRIM.Add(TRIMFINAL);

                }
                List<int> ICOUNT = new List<int>();
                List<int> OCOUNT = new List<int>();

                foreach (var sub in ListTRIM)
                {
                    if (sub != "")
                    {
                        string input = sub.Substring(0, sub.LastIndexOf(","));
                        int I = Convert.ToInt32(input);
                        string output = sub.Substring(sub.IndexOf(',') + 1);
                        int O = Convert.ToInt32(output);
                        ICOUNT.Add(I);
                        OCOUNT.Add(O);
                    }
                }

                var q = ICOUNT.GroupBy(x => x)
                            .Select(g => new { Value = g.Key, Count = g.Count() });

                Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
                List<int> myList = new List<int>();
                int p = 0;
                int u = 0;
                foreach (var v in q)
                {
                    int j;
                    int val = v.Value;
                    int con = v.Count;
                    List<int> ins = new List<int>();
                    for (j = 0; j < con; j++)
                    {

                        ins.Add(OCOUNT[j + u]);
                    }
                    u = u + con;
                    dictionary.Add(p, ins);
                    p++;
                }

                foreach (List<int> listw in dictionary.Values)
                {
                    for (int k = 0; k < listw.Count(); k++)
                    {
                        var ID = Convert.ToInt32(listw[k]);
                       // var Module = db.ModulePrecedanceOrders.Where(o => o.PageModuleID == ID && o.RoleID == RoleIDS).Select(o => o).ToList();
                        var Module = db.ModulePrecedanceOrders.Where(o => o.PageModuleID == ID ).Select(o => o).ToList();

                        var Order = k + 1;
                        foreach (var ListUpdate in Module)
                        {
                            ListUpdate.PreceedanceOrder = Order;
                            db.SaveChanges();
                        }
                    }

                }

                //foreach (List<int> listw in dictionary.Values)
                //{
                //    for (int k = 0; k < listw.Count(); k++)
                //    {
                //        var ID = Convert.ToInt32(listw[k]);
                //        var Module = db.DefaultModulePrecedanceOrders.Where(o => o.PageModuleID == ID).Select(o => o).ToList();

                //        var Order = k + 1;
                //        foreach (var ListUpdate in Module)
                //        {
                //            ListUpdate.PreceedanceOrder = Order;
                //            db.SaveChanges();
                //        }
                //    }

                //}
             

                //////////////////






                        //////////////////uncheck
                List<string> ListTRIM1 = new List<string>();
                var SUB1 = idsSUB.Split('$');

                foreach (var trim in SUB1)
                {
                    var TRIM = trim.TrimEnd(',');
                    var TRIMFINAL = TRIM.TrimStart(',');
                    ListTRIM1.Add(TRIMFINAL);

                }
                List<int> ICOUNT1 = new List<int>();
                List<int> OCOUNT1 = new List<int>();

                foreach (var sub in ListTRIM1)
                {
                    if (sub != "")
                    {
                        string input = sub.Substring(0, sub.LastIndexOf(","));
                        int I = Convert.ToInt32(input);
                        string output = sub.Substring(sub.IndexOf(',') + 1);
                        int O = Convert.ToInt32(output);
                        ICOUNT1.Add(I);
                        OCOUNT1.Add(O);
                    }
                }

                var q1 = ICOUNT1.GroupBy(x => x)
                            .Select(g => new { Value = g.Key, Count = g.Count() });

                Dictionary<int, List<int>> dictionary1 = new Dictionary<int, List<int>>();
                List<int> myList1 = new List<int>();
                int p1 = 0;
                int u1 = 0;
                foreach (var v in q1)
                {
                    int j;
                    int val = v.Value;
                    int con = v.Count;
                    List<int> ins = new List<int>();
                    for (j = 0; j < con; j++)
                    {

                        ins.Add(OCOUNT1[j + u1]);
                    }
                    u1 = u1 + con;
                    dictionary1.Add(p1, ins);
                    p1++;
                }

                foreach (List<int> listw in dictionary1.Values)
                {
                    for (int k = 0; k < listw.Count(); k++)
                    {
                        var ID = Convert.ToInt32(listw[k]);
                        var Module = db.DefaultModulePrecedanceOrders.Where(o => o.PageModuleID == ID).Select(o => o).ToList();

                        var Order = k + 1;
                        foreach (var ListUpdate in Module)
                        {
                            ListUpdate.PreceedanceOrder = Order;
                            db.SaveChanges();
                        }
                    }

                }

                        //////////////////



                var FunctionName = Ids1.Split(',');


                var FunctionCurrentOrder = Ids.Split(',');
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                var FunctionOrder = "";
                List<string> List = new List<string>();


                foreach (var x in FunctionCurrentOrder)
                {
                    FunctionOrder = rgx.Replace(x, "");
                    List.Add(FunctionOrder);
                }



                var FunctionCurrentOrder1 = Ids1.Split(',');
                Regex rgx1 = new Regex("[^a-zA-Z0-9 -]");
                var FunctionOrder1 = "";
                List<string> List1 = new List<string>();


                foreach (var x1 in FunctionCurrentOrder1)
                {
                    FunctionOrder1 = rgx1.Replace(x1, "");
                    List1.Add(FunctionOrder1);
                }




                for (int k = 0; k < List1.Count(); k++)
                {
                    var ID = Convert.ToInt32(List1[k]);
                   // var FunctionIDList = db.FunctionPrecedanceOrders.Where(o => o.FunctionID == ID && o.RoleID == RoleIDS).Select(o => o).ToList();
                    var FunctionIDList = db.FunctionPrecedanceOrders.Where(o => o.FunctionID == ID ).Select(o => o).ToList();

                    var Order = k + 1;
                    foreach (var ListUpdate in FunctionIDList)
                    {
                        ListUpdate.PreceedanceOrder = Order;
                        db.SaveChanges();
                    }



                }

                for (int k = 0; k < List.Count(); k++)
                {
                    var ID = Convert.ToInt32(List[k]);

                    var Default = db.DefaultFunctionPrecedanceOrders.Where(o => o.FunctionID == ID).Select(o => o).ToList();
                    var Order = k + 1;

                    foreach (var ListUpdateDefault in Default)
                    {
                        ListUpdateDefault.PreceedanceOrder = Order;
                        db.SaveChanges();
                    }

                }


                Session["Menu"] = DSRCLogic.StoredProcedures.GetUserMenu((int)userId, RoleID);
            }
            catch (Exception)
            {
                return Json("Warning", JsonRequestBehavior.AllowGet);

            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

    }
}
