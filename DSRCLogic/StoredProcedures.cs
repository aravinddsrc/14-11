//Developer: Animesh Agarwal
//Purpose: To Encapsulate the functionality of the Stored Procedures
//Date Modified: 24/10/2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DSRCManagementSystem.Models;
using DSRCManagementSystem;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace DSRCManagementSystem.DSRCLogic
{
    public class StoredProcedures
    {


        public static MenuListItem GetUserMenu(int UserID, int RoleID)
        {
            MenuListItem MainMenu = new MenuListItem();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var temp = db.GetMenuListByClientIdRoleID(UserID, RoleID).ToList();
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

                    if (MainMenu.Children.Where(x => x.MenuName.Equals(Function.MenuName)).FirstOrDefault() == null) MainMenu.Children.Add(Function);
                }
            }
            return MainMenu;
        }


        public static MenuListItem GetUserMenuForLD(int UserID, int RoleID)
        {

            MenuListItem MainMenu = new MenuListItem();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {

                DataTable dt = new DataTable();
                List<LDLeftMenuList> LDLeftMenuList = new List<LDLeftMenuList>();

                var strconn = ConfigurationManager.ConnectionStrings["DSRCManagementSystemEntitiesForLD"].ConnectionString;

                using (SqlConnection con = new SqlConnection(strconn))
                {
                    using (SqlCommand cmd = new SqlCommand("udpGetMenuListByClientIdForLD", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ClientUserID", SqlDbType.Int).Value = UserID;
                        cmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = RoleID;
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            var tempItem = new LDLeftMenuList();
                            tempItem.FunctionName = (string)row["FunctionName"];
                            tempItem.ModuleName = (string)row["ModuleName"];
                            tempItem.SubModuleName = (string)row["SubModuleName"];
                            tempItem.PageUrl = (string)row["PageUrl"];
                            tempItem.AccessLevel = (int)row["AccessLevel"];
                            tempItem.ModuleIcon = (string)row["ModuleIcon"];
                            tempItem.SubModuleIcon = (string)row["SubModuleIcon"];
                            tempItem.SubMenuIcon = (string)row["SubMenuIcon"];
                            tempItem.PreceedanceOrder = (Int64)row["PreceedanceOrder"];
                            tempItem.ModulePreceedanceOrder = (Int32)row["ModulePreceedanceOrder"];
                            LDLeftMenuList.Add(tempItem);
                        }
                        //LDLeftMenuList = dt.AsEnumerable().Select(r => new LDLeftMenuList() { FunctionName = (string)r["FunctionName"], ModuleName = (string)r["ModuleName"], SubModuleName = (string)r["SubModuleName"], PageUrl = (string)r["PageUrl"], AccessLevel = (int)r["AccessLevel"], ModuleIcon = (string)r["ModuleIcon"], SubModuleIcon = (string)r["SubModuleIcon"], SubMenuIcon = (string)r["SubMenuIcon"], PreceedanceOrder = (int)r["PreceedanceOrder"], ModulePreceedanceOrder = (int)r["ModulePreceedanceOrder"] }).ToList();
                    }
                }


                var temp = LDLeftMenuList.ToList();

                MainMenu.Children = new List<MenuListItem>();
                foreach (LDLeftMenuList menu in temp)
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

                    if (MainMenu.Children.Where(x => x.MenuName.Equals(Function.MenuName)).FirstOrDefault() == null) MainMenu.Children.Add(Function);
                }
            }
            return MainMenu;
        }

    }
}