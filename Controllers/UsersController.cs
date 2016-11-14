using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem.DSRCLogic;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Web.Configuration;
using System.Threading.Tasks;
using DSRCManagementSystem.Models.Domain_Models;
using System.Runtime.InteropServices;


namespace DSRCManagementSystem.Controllers
{
    public class UsersController : Controller
    {
        //
        // GET: /Users/
        DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();

        [HttpGet]
        public ActionResult ManageUsers(string BranchID)
        {
            int Branch = 0;
            if (BranchID != null)
            {
                Branch = Convert.ToInt32(BranchID);
            }
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_group = CommonLogic.getLabelName(3).ToString();

            ModelState.Clear();

            List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
            List<DSRCManagementSystem.Models.UserModel> UnblockUserList = new List<DSRCManagementSystem.Models.UserModel>();
            try
            {
                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = Convert.ToInt32(Session["UserID"]);
                var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();
                
                ModelState.Clear();


                int BranchId = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;
                DSRCManagementSystem.Models.UserModel objUser = new DSRCManagementSystem.Models.UserModel();
                List<DSRCManagementSystem.Models.UserModel> UserList = new List<DSRCManagementSystem.Models.UserModel>();
                UserList = (from u in db.Users.Where(o => o.IsActive != false)
                            join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                            join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                            from dn in DeptName.DefaultIfEmpty()
                            where u.BranchId == BranchId && u.UserStatus != 6 //By Default select only DSRC
                            select new DSRCManagementSystem.Models.UserModel()
                            {
                                UserId = u.UserID,
                                EmpID = u.EmpID,
                                FirstName = u.FirstName,
                                LastName = u.LastName ?? "-",
                                UserName = u.UserName,
                                Password = u.Password,
                                DepartmentName = dn.DepartmentName ?? "-",
                                EmailAddress = u.EmailAddress,
                                IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                IsUnderProbation = u.IsUnderNoticePeriod ?? false,
                                WorkPlace = u.Workplace,
                                LastworkingDate = u.LastWorkingDate,
                                SelectedUserStatusid = u.UserStatus,
                                Attempts = u.Attempts,
                                RollID = u.DesignationID,
                                RollName = v.DesignationName
                            }).OrderBy(x => x.EmpID).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();

                foreach (var item in UserList)
                {
                    if (item.LastworkingDate < DateTime.Now.Date)
                    {
                        var setStatus = db.Users.Where(c => c.UserID == item.UserId).FirstOrDefault();
                        setStatus.UserStatus = 6;
                    }
                    db.SaveChanges();
                    string WorkName;
                    if (Convert.ToInt32(item.WorkPlace) == 1)
                    {
                        WorkName = "DSRC";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 2)
                    {
                        WorkName = "DSRC Haddows Road";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 3)
                    {
                        WorkName = "Client Place";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 4)
                    {
                        WorkName = "Work From Home";
                    }
                    else
                        WorkName = "";

                    var um = new UserModel();
                    um.UserId = item.UserId;
                    um.EmpID = item.EmpID;
                    um.FirstName = item.FirstName;
                    um.LastName = item.LastName;
                    um.UserName = item.UserName;
                    um.Password = item.Password;
                    um.DepartmentName = item.DepartmentName;
                    um.EmailAddress = item.EmailAddress;
                    um.IsUnderNoticePeriod = item.IsUnderNoticePeriod;
                    um.WorkPlace = WorkName;
                    um.RoleName = item.RollName;
                    um.IsUnderProbation = item.IsUnderProbation;
                    um.SelectedUserStatusid = item.SelectedUserStatusid;
                    if (item.Attempts > 5) { um.Block = true; } else { um.Block = false; }
                    UserListNew.Add(um);
                }

                UnblockUserList = UserListNew.Where(x => x.Block == false).Select(x => x).ToList();
                var BranchList = db.Master_Branches.ToList();
                if (BranchID != null)
                {
                    var DepartmentList = db.Departments.Where(x => x.BranchID == Branch && x.IsActive == true).OrderBy(o=>o.DepartmentName).ToList();
                    var Status = (from p in db.Master_UserStatus.Where(u => u.IsActive == true)
                                  select new
                                  {
                                      userstatusid = p.UserStatusId,
                                      userstatus = p.UserStatus
                                  }).ToList();

                    ViewBag.MemberTypes = new SelectList(Status, "userstatusid", "userstatus");
                

                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                    ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", getBracnch);

                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = false;
                    ViewBag.IsResigned = false;
                    var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                    {
                        userstatusid = l.UserStatusId,
                        userstatusname = l.UserStatus
                    }).ToList();

                    ViewBag.Status = UserStatus;

                    var Department = db.Departments.Where(o => o.IsActive == true).Select(c => new
                    {
                        DepartmentId = c.DepartmentId,
                        DepartmentName = c.DepartmentName
                    }).ToList();
                    ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

                    var Group = db.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
                    {
                        GroupId = c.GroupID,
                        Groupname = c.GroupName
                    }).ToList();
                    ViewBag.Group = new SelectList(Group, "GroupId", "Groupname");
                    return View(UnblockUserList);
                }
                else
                {
                    var DepartmentList = db.Departments.Where(x => x.BranchID == 1 && x.IsActive == true).OrderBy(o=>o.DepartmentName).ToList();
                    var Status = (from p in db.Master_UserStatus.Where(u => u.IsActive == true)
                                  select new
                                  {
                                      userstatusid = p.UserStatusId,
                                      userstatus = p.UserStatus
                                  }).ToList();

                    ViewBag.MemberTypes = new SelectList(Status, "userstatusid", "userstatus");

                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");

                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                    ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", getBracnch);

                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("_UserProfile", UserListNew);
                    }
                    ViewBag.Inactive = false;
                    ViewBag.IsResigned = false;
                    var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                    {
                        userstatusid = l.UserStatusId,
                        userstatusname = l.UserStatus
                    }).ToList();

                    ViewBag.Status = UserStatus;

                    var Department = db.Departments.Where(o => o.IsActive == true).Select(c => new
                    {
                        DepartmentId = c.DepartmentId,
                        DepartmentName = c.DepartmentName
                    }).OrderBy(o=>o.DepartmentName).ToList();
                    ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

                    var Group = db.DepartmentGroups.Where(o => o.IsActive == true).Select(c => new
                    {
                        GroupId = c.GroupID,
                        Groupname = c.GroupName
                    }).OrderBy(o=>o.Groupname).ToList();
                    ViewBag.Group = new SelectList("", "GroupId", "Groupname");
                }

                return View(UnblockUserList);


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(UnblockUserList);
        }
        [HttpPost]
        public ActionResult ManageUsers(UserModel model)
        {
            List<DSRCManagementSystem.Models.UserModel> UserListNew = new List<DSRCManagementSystem.Models.UserModel>();
            List<DSRCManagementSystem.Models.UserModel> Result = new List<DSRCManagementSystem.Models.UserModel>();
            try
            {

                DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
                int userId = Convert.ToInt32(Session["UserID"]);
                var getBracnch = db.Users.Where(o => o.UserID == userId).Select(x => x.BranchId).FirstOrDefault();


                int DepartmentId = Convert.ToInt32(model.DepartmentName);
                int GroupId = Convert.ToInt32(model.GroupName);
                List<DSRCManagementSystem.Models.UserModel> UserList = new List<DSRCManagementSystem.Models.UserModel>();
                List<DSRCManagementSystem.Models.UserModel> ProbaUser = new List<DSRCManagementSystem.Models.UserModel>();
                List<DSRCManagementSystem.Models.UserModel> NoticeUser = new List<DSRCManagementSystem.Models.UserModel>();


                if (getBracnch != 1)
                    model.BranchID = (int)db.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                var BranchList = db.Master_Branches.ToList();

                ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", model.BranchID);

                ViewBag.IsResigned = false;

                var List = (from p in db.Master_TypesofEmployees
                            select new
                            {
                                Employees = p.ID,
                                Types = p.Types

                            }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();
                ViewBag.MemberTypes = new SelectList(List, "Employees", "Types");
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                var Status = (from p in db.Master_UserStatus.Where(u => u.IsActive == true)
                              select new
                              {
                                  userstatusid = p.UserStatusId,
                                  userstatus = p.UserStatus
                              }).ToList();

                ViewBag.MemberTypes = new SelectList(Status, "userstatusid", "userstatus");

                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();

                ViewBag.Status = UserStatus;

                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");
                if (model.BranchID != 0 || model.BranchID != null)
                {
                    var DepartmentList = db.Departments.Where(x => x.BranchID == model.BranchID).ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                }
                else
                {
                    var DepartmentList = db.Departments.Where(x => x.BranchID == model.BranchID).ToList();
                    ViewBag.DepartmentIdList = new SelectList(DepartmentList, "DepartmentId", "DepartmentName");
                }
                if (model.Employees == 1)
                {
                    model.NewJoinee = true;
                }


                else if (model.Employees == 2)
                {
                    model.NoticePeriod = true;
                }

                else if (model.Employees == 3)
                {
                    model.onboarding = true;
                }

                else if (model.Employees == 4)
                {
                    model.InActive = true;
                }
                //else if (model.Employees == 11) 
                //{
                //    model.NotPerformingGood = true;
                //}

                if (model.SearchedUserStatusid == null && model.Block == false)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false && o.UserStatus != 6)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID || u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).OrderBy(x => x.EmpID).ToList();
                }
                else if (model.SearchedUserStatusid == null && model.Block == true)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false && o.UserStatus != 6 && o.Attempts == 6)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                //where u.BranchId == model.BranchID || u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).OrderBy(x => x.EmpID).ToList();
                }

                if (model.SearchedUserStatusid == 6)
                {
                    UserList = (from u in db.Users.Where(o => o.UserID != userId)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID && u.UserStatus == model.SearchedUserStatusid && u.IsActive == false
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    LastworkingDate = u.LastWorkingDate,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).OrderByDescending(x => x.LastworkingDate).ToList();
                }
                else if (model.SearchedUserStatusid == 3)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID && u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    DateOfJoin = u.DateOfJoin,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).OrderBy(x => x.DateOfJoin).ToList();
                }
                else if (model.SearchedUserStatusid == 2)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID && u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    ResignedOn = u.ResignedOn,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).OrderBy(x => x.ResignedOn).ToList();
                }
                else if (model.SearchedUserStatusid == 4)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID && u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).ToList();
                }
                else if (model.SearchedUserStatusid == 11)
                {
                    UserList = (from u in db.Users.Where(o => o.IsActive != false)
                                join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptName
                                join v in db.Master_Designation on u.DesignationID equals v.DesignationID
                                from dn in DeptName.DefaultIfEmpty()
                                where u.BranchId == model.BranchID && u.UserStatus == model.SearchedUserStatusid
                                select new DSRCManagementSystem.Models.UserModel()
                                {
                                    UserId = u.UserID,
                                    EmpID = u.EmpID,
                                    FirstName = u.FirstName,
                                    LastName = (u.LastName == null) ? "-" : u.LastName,
                                    UserName = u.UserName,
                                    Password = u.Password,
                                    DepartmentName = (dn.DepartmentName == null) ? "-" : dn.DepartmentName,
                                    EmailAddress = u.EmailAddress,
                                    IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                    WorkPlace = u.Workplace,
                                    SelectedUserStatusid = u.UserStatus,
                                    GroupId = u.DepartmentGroup,
                                    Attempts = u.Attempts,
                                    RollID = u.DesignationID,
                                    RollName = v.DesignationName
                                }).ToList();
                }

                foreach (var item in UserList)
                {
                    string WorkName;
                    if (Convert.ToInt32(item.WorkPlace) == 1)
                    {
                        WorkName = "DSRC";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 2)
                    {
                        WorkName = "DSRC Haddows Road";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 3)
                    {
                        WorkName = "Client Place";
                    }
                    else if (Convert.ToInt32(item.WorkPlace) == 4)
                    {
                        WorkName = "Work From Home";
                    }
                    else
                        WorkName = "";

                    UserModel um = new UserModel();
                    um.UserId = item.UserId;
                    um.EmpID = item.EmpID;
                    um.FirstName = item.FirstName;
                    um.LastName = item.LastName;
                    um.UserName = item.UserName;
                    um.Password = item.Password;
                    um.DepartmentName = item.DepartmentName;
                    um.EmailAddress = item.EmailAddress;
                    um.IsUnderNoticePeriod = item.IsUnderNoticePeriod;
                    um.SelectedUserStatusid = item.SelectedUserStatusid;
                    um.WorkPlace = WorkName;
                    um.RoleName = item.RollName;
                    um.GroupId = item.GroupId;
                    um.Attempts = item.Attempts;
                    if (item.Attempts > 5) { um.Block = true; } else { um.Block = false; }
                    UserListNew.Add(um);

                }


                ViewBag.Inactive = false;



                var DepartmentName =
                    db.Departments.Where(x => x.DepartmentId == DepartmentId)
                        .Select(o => o.DepartmentName)
                        .FirstOrDefault();

                if (DepartmentId == 0)
                {
                    Result = UserListNew;
                }
                if (DepartmentId != 0 && GroupId == 0)
                {
                    Result = UserListNew.Where(x => x.DepartmentName == DepartmentName).ToList();
                }

                if (DepartmentId != 0 && GroupId != 0)
                {
                    Result = UserListNew.Where(x => x.DepartmentName == DepartmentName && x.GroupId == GroupId).ToList();
                }
                if (model.Block == true && DepartmentId == 0)
                {
                    Result = UserListNew.Where(x => x.Attempts == 6).ToList();
                }

                if (model.Block == true && DepartmentId != 0)
                {
                    Result = UserListNew.Where(x => x.Attempts == 6 && x.DepartmentName == DepartmentName).ToList();
                }
                if (model.Block == false && DepartmentId == 0)
                {
                    Result = UserListNew.Where(x => x.Attempts != 6).ToList();
                }
                var Department = db.Departments.Where(o => o.IsActive == true).Select(c => new
                {
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName
                }).ToList();
                ViewBag.Department = new SelectList(Department, "DepartmentId", "DepartmentName");

                if (model.DepartmentId != 0 || model.DepartmentId != null)
                {
                    var Group = (from d in db.Departments
                                 join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                 join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                 where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DepartmentId
                                 select new DSRCEmployees
                                 {
                                     Name = dg.GroupName,
                                     UserId = dg.GroupID,

                                 }).ToList();
                    ViewBag.Group = new SelectList(Group, "UserId", "Name");
                }
                else
                {

                    ViewBag.Group = new SelectList("", "UserId", "Name");
                }
                //}


            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(Result);
        }
        [HttpGet]
        public ActionResult AddUser(int? quick, UserModel profilemodel)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            UserModel obj_Users = new UserModel();

            List<string> EmpIDs = new List<string>();
            try
            {
              
                    int QuickEnrollid = Convert.ToInt32(quick);

                    if (QuickEnrollid == 0)
                    {


                        try
                        {
                           // var EmpID = db.Users.OrderByDescending(r => r.EmpID).Distinct().Skip(1).FirstOrDefault();
                           var EmpID = db.Users.Select(x => x.EmpID).Distinct().OrderByDescending(x=>x).Skip(1).ToList();
                            //for (int i = 0; i < EmpID.Count; i++)
                            //{
                            //    EmpIDs.Add((EmpID[i]));
                            //}
                           
                  //          var Empid = db.Users.OrderByDescending(p => p.EmpID)
                  //.Distinct(new EqualityComparer()).Skip(1).First();
                           //string Empid = db.Users.Select(x => x.EmpID.Max()).ToList();
                            //var EmpID = db.Users.GroupBy(e => e.EmpID).OrderByDescending(g => g.Key).Skip(1).First();
                            string Empid =(EmpID.Max());
                if (db.Users.Any(R => R.EmpID == profilemodel.EmpID))
                {
                    ModelState.AddModelError("EmpID", "EmpID  Already Exists");
                    return Json("Warning", JsonRequestBehavior.AllowGet);

                }
                else if (Empid == null)
                {
                    ModelState.AddModelError("EmpID", "EmpID");

                }
                else
                {
                            if (EmpID != null)
                            {

                                if (Empid.Length == 1)
                                {
                                    ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                                }
                                else if (Empid.Length == 2)
                                {
                                    ViewBag.EmpID = "000" + (Convert.ToInt32(Empid) + 1);
                                }
                                else if (Empid.Length == 3)
                                {
                                    ViewBag.EmpID = "00" + (Convert.ToInt32(Empid) + 1);
                                }
                                else if (Empid.Length == 4)
                                {
                                    ViewBag.EmpID = "0" + (Convert.ToInt32(Empid) + 1);
                                }
                                else
                                {
                                    string displayString = string.Empty;

                                    //var temp=Empid.Split('/');
                                    //string tempid=temp[2];
                                    //int id = Convert.ToInt32(tempid);
                                    //id = id + 1;
                                    //string autoId = "DSRC" + String.Format("{0:0000}", id);
                                    
                                    //ViewBag.EmpID = "" + (Convert.ToInt32(tempid) + 1);
                                 ////if (string.IsNullOrEmpty(Empid))
                                 ////{
                                    ////{
                                    ////    jid = "AM0000";//This string value has to increment at every time, but it is getting increment only one time.
                                    ////}
                                    int len = Empid.Length;
                                    string split = Empid.Substring(4, len - 4);
                                    int num = Convert.ToInt32(split);
                                    num++;
                                    displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                    ViewBag.EmpId = displayString;
                                   
                            }
                                if (db.Users.Any(R => R.EmpID == Empid))
                                {
                                    string displayString = string.Empty;
                                    int len = Empid.Length;
                                    string split = Empid.Substring(4, len - 4);
                                    int num = Convert.ToInt32(split);
                                    num++;
                                    displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                    ViewBag.EmpId = displayString;
                                }
                                }
                                
                               
                            //}
                            else
                            {
                                ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                            }
                        }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.EmpID = "0000" + 1;
                        }



                        var MaritalStautus = (from p in db.Master_MaritalStatus
                                              select new
                                              {
                                                  MaritalStatusId = p.MaritalStatusID,
                                                  Value = p.MaritalStatusType

                                              }).ToList();

                        var workplace = (from p in db.Master_WorkPlace
                                         select new
                                         {
                                             WorkplaceId = p.WorkPlaceID,
                                             Name = p.WorkPlaceName

                                         }).OrderBy(x => x.Name).ToList();
                        var GenderNameList = (from us in db.Master_Gender
                                              select new
                                              {
                                                  GenderID = us.GenderID,
                                                  GenderName = us.GenderName
                                              }).ToList();

                        var RelationShip = (from r in db.Master_Relationship
                                            select new
                                            {
                                                RelationShipID = r.RelationshipID,
                                                RelationShipName = r.RelationshipName
                                            }).ToList();
                        ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName");

                        var Roles = (from role in db.Master_Roles
                                     select new
                                     {
                                         RollName = role.RoleName,
                                         RollID = role.RoleID
                                     });
                        ViewBag.Roles = new SelectList(Roles, "RollID ", "RollName");

                        var Religious = (from r in db.Master_Religious
                                         select new
                                         {
                                             ReligiousID = r.ReligiousID,
                                             ReligiousName = r.ReligiousName
                                         }).ToList();
                        ViewBag.Religious = new SelectList(Religious, "ReligiousID ", "ReligiousName");

                        var Nationality = (from r in db.Master_Nationality
                                           select new
                                           {
                                               NationalityID = r.NationalityID,
                                               NationalityName = r.NationalityName
                                           }).ToList();
                        ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName");

                        var BloogGroup = (from b in db.Master_BloodGroup
                                          select new
                                          {
                                              BloogGroup = b.BloodGroupName,
                                              BloodGroupID = b.BloodGroupID
                                          }).ToList();

                        ViewBag.BloogGroup = new SelectList(BloogGroup, "BloodGroupID", "BloogGroup");

                        var Country = (from c in db.Master_Country
                                       select new
                                       {
                                           CountryID = c.CountryID,
                                           CountryName = c.CountryName


                                       }).ToList();
                        ViewBag.Country = new SelectList(Country, "CountryID", "CountryName");


                        var state = (from s in db.Master_States
                                     select new
                                     {
                                         StateID = s.StateID,
                                         StateName = s.States
                                     }).ToList();

                        ViewBag.state = new SelectList("", "StateID", "StateName");



                        var city = (from c in db.Master_City
                                    select new

                                    {
                                        CityID = c.CityID,
                                        CityName = c.CityName

                                    }).ToList();

                        ViewBag.city = new SelectList("", "CityID", "CityName");


                        var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                          join q in db.UserRoles on p.RoleID equals q.RoleID
                                          select q.UserID).ToList();
                        List<int> ChildCount = new List<int>();

                        for (int i = 1; i < 10; i++)
                        {
                            ChildCount.Add(i);
                        }
                        ViewBag.Child = new SelectList(ChildCount, "", "");

                        var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                        foreach (var id in userbyid)
                        {
                            userbyrole.Add(id);
                        }
                        var fullusers = userbyrole.Distinct().ToList();


                        //Added Branch//



                        List<object> EmployeeList = new List<object>();

                        foreach (var userid in fullusers)
                        {
                            var name =
                                db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6)
                                    .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                                    .FirstOrDefault();

                            if (name != null)
                            {
                                var Val = new { ID1 = userid, UserName1 = name };
                                EmployeeList.Add(Val);
                            }

                        }


                        var Zone = (from p in db.TimeZones
                                    select new
                                    {
                                        RegionId = p.Id,
                                        Region = p.Zone
                                    }).ToList();
                        var txt = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, "DepartmentId", "DepartmentName", 0);

                        var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
                        var Projects = db.Projects.Where(p => p.IsActive == true).ToList();
                        var RoleNameList = db.Master_Roles.Where(mr => mr.IsActive == true).ToList();
                        var DesignationList = db.Master_Designation.Where(md => md.IsActive == true).OrderBy(x => x.DesignationName).ToList();
                        var BranchList = db.Master_Branches.ToList();
                        var Branchcnt = db.Master_Branches.ToList().Count;
                        int branch = Convert.ToInt32(Session["BranchID"]);
                        var DepartmentIdList = (from d in db.Departments
                                                where d.IsActive == true && d.BranchID == branch
                                                select new DSRCEmployees
                                                {
                                                    Name = d.DepartmentName,
                                                    DepartmentId = d.DepartmentId,

                                                }).OrderBy(x => x.Name).ToList();

                        var department = (from d in db.QuickEnrolls
                                          where d.QuickEnroll1 == QuickEnrollid
                                          select new
                                          {
                                              d.Department
                                          }).FirstOrDefault();

                        //var DesignationList = (from
                        //            r in db.Master_Designation
                        //                       select new
                        //                       {
                        //                           DesignationID = r.DesignationID,
                        //                           DesignationName = r.DesignationName
                        //                       }).OrderBy(x => x.DesignationName).ToList();
                        ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "DepartmentId", "Name");

                        ViewBag.Groups = new SelectList(new[] { new DepartmentGroup() { GroupID = 0, GroupName = "--Select--" } }, "GroupID", "GroupName", 0);
                        //ViewBag.DepartmentIdList = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, "DepartmentId", "DepartmentName", 0);
                        //ViewBag.Designation = new SelectList(DesignationList, "DesignationID", "DesignationName",0);// Enroll.DesignationID);
                        ViewBag.RoleIdList = new SelectList(new[] { new Master_Roles() { RoleID = 0, RoleName = "--Select--" } }.Union(RoleNameList), "RoleID", "RoleName", 0);
                        ViewBag.Designation = new SelectList(new[] { new Master_Designation() { DesignationID = 0, DesignationName = "--Select--" } }.Union(DesignationList), "DesignationID", "DesignationName", 0);
                        ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");
                        ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value");
                        ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                        ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", 0);
                        ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", 0);
                        ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology");
                        ViewBag.BranchList = new SelectList(new[] { new Master_Branches() }.Union(BranchList), "BranchID", "BranchName", 1);
                        ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1");
                        ViewBag.Region = new SelectList(Zone, "RegionId", "Region", 3);

                        if (Branchcnt == 1)
                        {
                            var Manageemployees = new UserModel();


                            Manageemployees.BranchID = (int)db.Master_Branches.FirstOrDefault().BranchID;
                            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", Manageemployees.BranchID);

                            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();

                            List<int> validDepart = new List<int>();

                            validDepart = db.Departments.Where(d => d.BranchID == Manageemployees.BranchID && d.IsActive == true).Select(d => d.DepartmentId).ToList();

                            FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                            select new FilterDepartment()
                                            {
                                                DepartmentId = lt.DepartmentId,
                                                DepartmentName = lt.DepartmentName
                                            }).OrderBy(x => x.DepartmentName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });


                            ViewBag.DepartmentIdList = new SelectList(FilterDepart, "Value", "Text", 0);


                            var role = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                        join q in db.UserRoles on p.RoleID equals q.RoleID
                                        select q.UserID).ToList();
                            var UserRoleId = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                            foreach (var id in UserRoleId)
                            {
                                userbyrole.Add(id);
                            }
                            var AllUsers = role.Distinct().ToList();




                            foreach (var userid in AllUsers)
                            {
                                var name =
                                    db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == Manageemployees.BranchID)
                                        .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                                        .FirstOrDefault();
                                if (name != null)
                                {
                                    var Val = new { ID1 = userid, UserName1 = name };

                                    EmployeeList.Add(Val);
                                }

                            }

                        }
                        else
                        {
                            ViewBag.BranchList = new SelectList(new[] { new Master_Branches() }.Union(BranchList), "BranchID", "BranchName", 1);
                        }


                        return View();
                    }
                    else
                    {
                        try
                        {
                            // var EmpID = db.Users.Select(x => x.EmpID).ToList();
                            var EmpID = db.Users.Select(x => x.EmpID).Distinct().OrderByDescending(x => x).Skip(1).ToList();
                            ////for (int i = 0; i < EmpID.Count; i++)
                            ////{
                            ////    EmpIDs.Add(Convert.ToString(EmpID[i]));
                            ////}
                            string Empid = Convert.ToString(EmpID.Max());

                            if (db.Users.Any(R => R.EmpID == profilemodel.EmpID))
                            {
                                ModelState.AddModelError("EmpID", "EmpID  Already Exists");
                                return Json("Warning", JsonRequestBehavior.AllowGet);

                            }
                            else if (Empid == null)
                            {
                                ModelState.AddModelError("EmpID", "EmpID");

                            }
                            else
                            {
                                if (EmpID != null)
                                {

                                    if (Empid.Length == 1)
                                    {
                                        ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                                    }
                                    else if (Empid.Length == 2)
                                    {
                                        ViewBag.EmpID = "000" + (Convert.ToInt32(Empid) + 1);
                                    }
                                    else if (Empid.Length == 3)
                                    {
                                        ViewBag.EmpID = "00" + (Convert.ToInt32(Empid) + 1);
                                    }
                                    else if (Empid.Length == 4)
                                    {
                                        ViewBag.EmpID = "0" + (Convert.ToInt32(Empid) + 1);
                                    }


                                    else
                                    {
                                        //ViewBag.EmpID = "0000" + 1;
                                        string displayString = string.Empty;

                                        //var temp=Empid.Split('/');
                                        //string tempid=temp[2];
                                        //int id = Convert.ToInt32(tempid);
                                        //id = id + 1;
                                        //string autoId = "DSRC" + String.Format("{0:0000}", id);

                                        //ViewBag.EmpID = "" + (Convert.ToInt32(tempid) + 1);
                                        ////if (string.IsNullOrEmpty(Empid))
                                        ////{
                                        ////{
                                        ////    jid = "AM0000";//This string value has to increment at every time, but it is getting increment only one time.
                                        ////}
                                        int len = Empid.Length;
                                        string split = Empid.Substring(4, len - 4);
                                        int num = Convert.ToInt32(split);
                                        num++;
                                        displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                        ViewBag.EmpId = displayString;
                                    }

                                    if (db.Users.Any(R => R.EmpID == Empid))
                                    {
                                        string displayString = string.Empty;
                                        int len = Empid.Length;
                                        string split = Empid.Substring(4, len - 4);
                                        int num = Convert.ToInt32(split);
                                        num++;
                                        displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                        ViewBag.EmpId = displayString;
                                    }

                                }

                            //}
                                else
                                {
                                    ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.EmpID = "0000" + 1;
                        }

                        QuickEnrollment roll = new QuickEnrollment();
                        UserModel obj = new UserModel();
                        obj.flag = true;
                        obj.quick = quick;

                        var Enroll = (from us in db.QuickEnrolls
                                      where us.QuickEnroll1 == QuickEnrollid
                                      select new UserModel
                                      {

                                          FirstName = us.FirstName,
                                          LastName = us.LastName,
                                          DateOfBirth = us.DateOfBirth,
                                          DateOfJoin = us.DateOfJoin,
                                          ContactNo = us.PhoneNumber,
                                         // EmailAddress=us.PersonalMailAddress,
                                          //PersonalMailAddress=us.PersonalMailAddress,
                                          Experience = us.Experience,
                                          DepartmentId = us.Department.DepartmentId,
                                          GenderID=us.GenderID,
                                          DesignationID=us.RoleID,

                                          GroupId = us.DepartmentGroup,
                                          QAddress = us.Address

                                      }).FirstOrDefault();
                        if (Enroll.Experience != "" && Enroll.Experience != null)
                        {
                            var ex1 = Enroll.Experience.Split('.');
                            Enroll.ExperienceYear = ex1[0];
                            Enroll.ExperienceMonth = ex1[1];
                        }


                        var MaritalStautus = (from p in db.Master_MaritalStatus
                                              select new
                                              {
                                                  MaritalStatusId = p.MaritalStatusID,
                                                  Value = p.MaritalStatusType

                                              }).ToList();

                        var workplace = (from p in db.Master_WorkPlace
                                         select new
                                         {
                                             WorkplaceId = p.WorkPlaceID,
                                             Name = p.WorkPlaceName

                                         }).OrderBy(x => x.Name).ToList();
                        var GenderNameList = (from us in db.Master_Gender
                                              select new
                                              {
                                                  GenderID = us.GenderID,
                                                  GenderName = us.GenderName
                                              }).ToList();

                        var RelationShip = (from r in db.Master_Relationship
                                            select new
                                            {
                                                RelationShipID = r.RelationshipID,
                                                RelationShipName = r.RelationshipName
                                            }).ToList();
                        ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName");

                        var Roles = (from role in db.Master_Roles
                                     select new
                                     {
                                         RollName = role.RoleName,
                                         RollID = role.RoleID
                                     });
                        ViewBag.Roles = new SelectList(Roles, "RollID ", "RollName");

                        var Religious = (from r in db.Master_Religious
                                         select new
                                         {
                                             ReligiousID = r.ReligiousID,
                                             ReligiousName = r.ReligiousName
                                         }).ToList();
                        ViewBag.Religious = new SelectList(Religious, "ReligiousID ", "ReligiousName");

                        var Nationality = (from r in db.Master_Nationality
                                           select new
                                           {
                                               NationalityID = r.NationalityID,
                                               NationalityName = r.NationalityName
                                           }).ToList();
                        ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName");

                        var BloogGroup = (from b in db.Master_BloodGroup
                                          select new
                                          {
                                              BloogGroup = b.BloodGroupName,
                                              BloodGroupID = b.BloodGroupID
                                          }).ToList();

                        ViewBag.BloogGroup = new SelectList(BloogGroup, "BloodGroupID", "BloogGroup");

                        var Country = (from c in db.Master_Country
                                       select new
                                       {
                                           CountryID = c.CountryID,
                                           CountryName = c.CountryName


                                       }).ToList();
                        ViewBag.Country = new SelectList(Country, "CountryID", "CountryName");


                        var state = (from s in db.Master_States
                                     select new
                                     {
                                         StateID = s.StateID,
                                         StateName = s.States
                                     }).ToList();

                        ViewBag.state = new SelectList("", "StateID", "StateName");



                        var city = (from c in db.Master_City
                                    select new

                                    {
                                        CityID = c.CityID,
                                        CityName = c.CityName

                                    }).ToList();

                        ViewBag.city = new SelectList("", "CityID", "CityName");
                        List<int> ChildCount = new List<int>();

                        for (int i = 1; i < 10; i++)
                        {
                            ChildCount.Add(i);
                        }
                        ViewBag.Child = new SelectList(ChildCount, "", "");
                        var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                          join q in db.UserRoles on p.RoleID equals q.RoleID
                                          select q.UserID).ToList();
                        var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                        foreach (var id in userbyid)
                        {
                            userbyrole.Add(id);
                        }
                        var fullusers = userbyrole.Distinct().ToList();

                        List<object> EmployeeList = new List<object>();

                        foreach (var userid in fullusers)
                        {
                            var name =
                                db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6)
                                    .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                                    .FirstOrDefault();
                            var Val = new { ID1 = userid, UserName1 = name };
                            EmployeeList.Add(Val);
                        }
                        //var EmployeeList = (from p in db.Users.Where(x => x.IsActive == true /*&& x.UserID != obj_Users.UserId*/)
                        //                    select new
                        //                    {
                        //                        ID1 = p.UserID,
                        //                        UserName1 = p.FirstName + " " + p.LastName
                        var txt = new SelectList(new[] { new Department() { DepartmentId = 0, DepartmentName = "---Select---" } }, "DepartmentId", "DepartmentName", Enroll.DepartmentId);
                        //                    }).ToList();
                        var Zone = (from p in db.TimeZones
                                    select new
                                    {
                                        RegionId = p.Id,
                                        Region = p.Zone
                                    }).ToList();

                        var DepartmentList = db.Departments.Where(d => d.IsActive == true).ToList();
                        var Projects = db.Projects.Where(p => p.IsActive == true).ToList();
                        var RoleNameList = db.Master_Roles.Where(mr => mr.IsActive == true).ToList();
                       // var DesignationList = db.Master_Designation.Where(md => md.IsActive == true).OrderBy(x => x.DesignationName).ToList();
                        var BranchList = db.Master_Branches.ToList();
                        var GroupList = db.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                        {
                            groupid = g.GroupID,
                            groupname = g.GroupName
                        }).OrderBy(o => o.groupname).ToList();

                        var DepartmentIdList = (from d in db.Departments
                                                where d.IsActive == true && d.BranchID == 1
                                                select new DSRCEmployees
                                                {
                                                    Name = d.DepartmentName,
                                                    DepartmentId = d.DepartmentId,

                                                }).OrderBy(x => x.Name).ToList();

                        var DesignationList = (from
                                   r in db.Master_Designation
                                               select new
                                               {
                                                   DesignationID = r.DesignationID,
                                                   DesignationName = r.DesignationName
                                               }).OrderBy(x => x.DesignationName).ToList();

                        ViewBag.DepartmentIdList = new SelectList(DepartmentIdList, "DepartmentId", "Name");


                        ViewBag.Designation = new SelectList(DesignationList, "DesignationID", "DesignationName", Enroll.DesignationID);
                        ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", Enroll.GroupId);

                        ViewBag.RoleIdList = new SelectList(new[] { new Master_Roles() { RoleID = 0, RoleName = "---Select---" } }.Union(RoleNameList), "RoleID", "RoleName", 0);
                       // ViewBag.Designation = new SelectList(new[] { new Master_Designation() { DesignationID = 0, DesignationName = "---Select---" } }.Union(DesignationList), "DesignationID", "DesignationName", 0);
                        ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");
                        ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value");
                        ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name");

                        ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", Enroll.ExperienceYear);

                        ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", Enroll.ExperienceMonth);
                        ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology");
                        ViewBag.BranchList = new SelectList(new[] { new Master_Branches() }.Union(BranchList), "BranchID", "BranchName", 1);
                        ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1");
                        ViewBag.Region = new SelectList(Zone, "RegionId", "Region", 3);
                        ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName");
                        obj_Users.IsBoarding = true;


                        return View(Enroll);

                    }

                }
            
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddUser(UserModel profilemodel)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            //profilemodel.EmergencyContactNo = Emergency;
            DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();
            try
            {
                             
                    int userId = Convert.ToInt32(Session["UserID"]);
                    string EmployeeID;
                    string password = GenerateRandomPassword(10);
                    string newpassword = DSRCLogic.Hashing.Create_SHA256(password);
                    List<int> objuser = new List<int>();
                    string[] value = profilemodel.multiselectemployees.Split(',');

                    for (int i = 0; i < value.Count(); i++)
                    {
                        objuser.Add(Convert.ToInt32(value[i]));
                    }

                    var DesigEmp = db.Master_Designation.Where(x => x.DesignationID == profilemodel.DesignationID).Select(o => o.DesignationDescription).First();
                    
                if (profilemodel.EmpID == null)
                {
                    ModelState.AddModelError("EmpID", "EmpID");

                }
                     else
                     {
                    if (profilemodel.EmpID != null)
                    {
                       // var a = Regex.Matches(profilemodel.EmpID, @"[a-zA-Z]").Count;
                        if (profilemodel.EmpID.Length <= 10 )
                        {
                            if (profilemodel.EmpID.Length == 1)
                            {
                                profilemodel.EmpID = "0000" + profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            if (profilemodel.EmpID.Length == 2)
                            {
                                profilemodel.EmpID = "000" + profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            if (profilemodel.EmpID.Length == 3)
                            {
                                profilemodel.EmpID = "00" + profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            if (profilemodel.EmpID.Length == 4 )
                            {
                                profilemodel.EmpID = "0" + profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            if (profilemodel.EmpID.Length == 5  )
                            {
                                profilemodel.EmpID = profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            else
                            {
                                 profilemodel.EmpID = profilemodel.EmpID;
                                EmployeeID = profilemodel.EmpID;
                                profilemodel.EMPCODE = DesigEmp + EmployeeID;
                                ViewBag.EmpCode = profilemodel.EMPCODE;
                            }
                            //if (a != 0)
                            //{
                            //    profilemodel.EmpID = profilemodel.EmpID;
                            //    EmployeeID = profilemodel.EmpID;
                            //    profilemodel.EMPCODE = DesigEmp + EmployeeID;
                            //    ViewBag.EmpCode = profilemodel.EMPCODE;
                            //}
                        }
                        if (db.Users.Any(R => R.EmpID == profilemodel.EmpID))
                        {
                            ModelState.AddModelError("EmpID", "EmpID  Already Exists");
                            return Json("Warning", JsonRequestBehavior.AllowGet);

                        }
                        var empidCheck = db.Users.FirstOrDefault(x => x.EmpID == profilemodel.EmpID && x.BranchId == profilemodel.BranchID);
                    }
                }
                    dynamic emailAddressCheck;

                    if (profilemodel.EmailAddress != null)
                        emailAddressCheck = db.Users.FirstOrDefault(x => x.EmailAddress == profilemodel.EmailAddress);
                    EmployeeID = profilemodel.EmpID;
                    var FirstNametrim = profilemodel.FirstName.Trim();
                    var LastNametrim = profilemodel.LastName.Trim();
                    var MiddleNametrim = "";
               
                if(profilemodel.MiddleName != null)
                     MiddleNametrim = profilemodel.MiddleName.Trim();

                    try
                    {
                        MailMessage mail = new MailMessage();
                        try
                        {
                            var permanentAddCount = db.PermanentAddresses.OrderByDescending(x => x).First();

                            if (permanentAddCount != null)
                            {
                                profilemodel.PermanentAddress = Convert.ToInt32(permanentAddCount.PermanentAddressID) + 1;
                            }
                            else
                            {
                                profilemodel.PermanentAddress = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            profilemodel.PermanentAddress = 1;
                        }
                        int PermanentID = 0;
                        int PresentID = 0;
                        try
                        {
                            var PermanentMax = (from d in db.PermanentAddresses select d.PermanentAddressID).Max();
                            PermanentID = Convert.ToInt32(PermanentMax) + 1;
                        }
                        catch (Exception ex)
                        {
                            PermanentID = 1;
                        }
                        try
                        {
                            var PersentMax = (from d in db.PresentAddresses select d.PresentAddressID).Max();
                            PresentID = Convert.ToInt32(PersentMax) + 1;
                        }
                        catch (Exception ex)
                        {
                            PresentID = 1;
                        }





                        var t = new User
                        {


                            CreatedUserID = userId,
                            EmpID = profilemodel.EmpID,
                            EmpCode = profilemodel.EMPCODE,
                            DepartmentId = profilemodel.DepartmentId,
                            FirstName = FirstNametrim,
                            MiddleName = MiddleNametrim,
                            LastName = LastNametrim,
                            UserName = profilemodel.UserName,
                            Password = newpassword,
                            DateOfJoin = profilemodel.DateOfJoin,
                            DateOfBirth = profilemodel.DateOfBirth,
                            Gender = profilemodel.GenderID,
                            EmailAddress = profilemodel.EmailAddress,
                            IsFirstLogin = true,
                            Attempts = 0,
                            IsActive = true,
                            IsBoarding = profilemodel.IsBoarding,
                            Experience = profilemodel.ExperienceYear + "." + profilemodel.ExperienceMonth,
                            Workplace = Convert.ToString(profilemodel.WorkplaceId),
                            MaritalStatus = profilemodel.MaritalStatusId,
                            IsUnderProbation = true,
                            IsExclude = profilemodel.BranchID != 1 ? true : false,
                            BranchId = profilemodel.BranchID,
                            DesignationID = profilemodel.DesignationID,
                            UserStatus = 3,
                            DirectReportingTo = profilemodel.multiselectemployees,
                            DepartmentGroup = profilemodel.GroupId,
                            Region = profilemodel.RegionId,
                            ContactNo = Convert.ToInt64(profilemodel.ContactNo),
                            IPAddress = profilemodel.IPAddress,
                            extension = profilemodel.Extension,
                            officeno = profilemodel.OfficeNo,
                            OfficeSkypeId = profilemodel.OfficeSkypeId,
                            MachineName = profilemodel.MachineName,
                            PermanentAddressID = PermanentID,
                            TemporaryAddressID = PresentID,
                            FatherName = profilemodel.FatherName,
                            MotherName = profilemodel.MotherName,
                            BloodGroup = profilemodel.BloodGroup,
                            BirthPlace = profilemodel.BirthPlace,
                            NationalityID = profilemodel.NationalityID,
                            ReligionID = profilemodel.ReligiousID,
                            SpouseName = profilemodel.SpouseName,
                            NoOfChild = profilemodel.NoOfChild,
                            AnniversaryDate = profilemodel.AnniversaryDate,
                            EmergencyContact = profilemodel.Emergency==0?null:profilemodel.Emergency,
                            RelationshipID = profilemodel.RelationShipID,
                            Contactperson = profilemodel.EmergencyContactName,
                            RoleID = (byte)profilemodel.RollID


                        };
                        db.Users.AddObject(t);

                        var user = db.UserRoles.CreateObject();
                        user.UserID = profilemodel.UserId;
                     //   user.RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == MasterEnum.NewuserRole.NewEmployeeRole).RoleID;
                        user.RoleID = Convert.ToByte(t.RoleID);
                        db.UserRoles.AddObject(user);
                        db.SaveChanges();




                        int userID = db.Users.Where(x => x.EmpID == profilemodel.EmpID).Select(o => o.UserID).FirstOrDefault();

                        int PresentCountryID = db.Master_Country.Where(x => x.CountryName == profilemodel.PresentCountry).Select(o => o.CountryID).FirstOrDefault();

                        var PresentAdd = db.PresentAddresses.CreateObject();
                        PresentAdd.UserID = userID;
                        PresentAdd.Address_1 = profilemodel.PresentAddress1;
                        PresentAdd.Address_2 = profilemodel.PresentAddress2;
                        PresentAdd.Address_3 = profilemodel.PresentAddress3;
                        PresentAdd.City = Convert.ToString(profilemodel.PresentCityID);
                        PresentAdd.State = Convert.ToString(profilemodel.PresentstateID);
                        PresentAdd.Zip = profilemodel.PresentPinCode;
                        PresentAdd.DateCreated = System.DateTime.Now;
                        PresentAdd.IsActive = true;
                        PresentAdd.CountryID = PresentCountryID;
                        db.PresentAddresses.AddObject(PresentAdd);
                        db.SaveChanges();


                        int permanentCountry = db.Master_Country.Where(x => x.CountryName == profilemodel.PermanentCountry).Select(o => o.CountryID).FirstOrDefault();
                        var PermanentAdd = db.PermanentAddresses.CreateObject();
                        PermanentAdd.UserID = userID;
                        PermanentAdd.Address_1 = profilemodel.PermanentAddress1;
                        PermanentAdd.Address_2 = profilemodel.PermanentAddress2;
                        PermanentAdd.Address_3 = profilemodel.PermanentAddress3;
                        PermanentAdd.City = Convert.ToString(profilemodel.PermanentCityID);
                        PermanentAdd.State = Convert.ToString(profilemodel.PermanentstateID);
                        PermanentAdd.Zip = profilemodel.PermanentPinCode;
                        PermanentAdd.DateCreated = System.DateTime.Now;
                        PermanentAdd.IsActive = true;
                        PermanentAdd.CountryID = Convert.ToInt32(profilemodel.PermanentCountry);
                        db.PermanentAddresses.AddObject(PermanentAdd);
                        db.SaveChanges();




                        for (int i = 0; i < objuser.Count(); i++)
                        {
                            DSRCManagementSystem.UserReporting objreporting = new DSRCManagementSystem.UserReporting();
                            objreporting.UserID = db.Users.Where(x => x.EmpID == profilemodel.EmpID).Select(o => o.UserID).FirstOrDefault();
                            objreporting.ReportingUserID = Convert.ToInt32(objuser[i]);
                            db.AddToUserReportings(objreporting);
                        }
                        db.SaveChanges();

                        db.UserSkills.AddObject(new UserSkill { UserID = t.UserID, Skills = profilemodel.Tecnology });
                        db.SaveChanges();
                        if (profilemodel.quick != null)
                        {

                            var DeleteQuickEnroll = db.QuickEnrolls.Where(o => o.QuickEnroll1 == profilemodel.quick).Select(o => o).FirstOrDefault();
                            DeleteQuickEnroll.IsActive = false;
                            db.SaveChanges();
                        }


                        var logo = CommonLogic.getLogoPath();

                        if (profilemodel.EmailAddress != null)
                        {
                            var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "New User").Select(o => o.EmailTemplateID).FirstOrDefault();
                            var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "New User").Select(x => x.TemplatePath).FirstOrDefault();
                            if ((check != null) && (check != 0))
                            {
                                var objnewuser = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "New User")
                                                  join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                  select new DSRCManagementSystem.Models.Email
                                                  {
                                                      To = p.To,
                                                      CC = p.CC,
                                                      BCC = p.BCC,
                                                      Subject = p.Subject,
                                                      Template = q.TemplatePath
                                                  }).FirstOrDefault();

                                var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                string TemplatePathnewuser = Server.MapPath(objnewuser.Template);
                                string newuserhtml = System.IO.File.ReadAllText(TemplatePathnewuser);
                                newuserhtml = newuserhtml.Replace("#UserName", profilemodel.FirstName + "  " + profilemodel.LastName);
                                newuserhtml = newuserhtml.Replace("#LoginID", profilemodel.UserName);
                                newuserhtml = newuserhtml.Replace("#Password", password);
                                newuserhtml = newuserhtml.Replace("#ServerName", ServerName);
                                newuserhtml = newuserhtml.Replace("#CompanyName", company);
                                if (objnewuser.To != "")
                                {
                                    objnewuser.To = UsersController.GetUserEmailAddress(db, objnewuser.To);
                                }
                                if (objnewuser.CC != "")
                                {
                                    objnewuser.BCC = UsersController.GetUserEmailAddress(db, objnewuser.CC);
                                }
                                if (objnewuser.BCC != "")
                                {
                                    objnewuser.BCC = UsersController.GetUserEmailAddress(db, objnewuser.BCC);
                                }

                                // string ServerName = WebConfigurationManager.AppSettings["SeverName"];


                                List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                string EmailAddress = "";

                                foreach (string mails in MailIds)
                                {
                                    EmailAddress += mails + ",";
                                }
                                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);
                                if (ServerName != "http://win2012srv:88/")
                                {
                                    Task.Factory.StartNew(() =>
                                    {

                                        DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject + " - Test Mail Please Ignore", "", newuserhtml + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));
                                    });

                                }
                                else
                                {
                                    Task.Factory.StartNew(() =>
                                    {

                                        DsrcMailSystem.MailSender.SendMail(null, objnewuser.Subject, "", newuserhtml, "Test-HRMS@dsrc.co.in", EmailAddress, Server.MapPath(logo.ToString()));

                                    });
                                }
                            }
                            else
                            {
                                //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                ExceptionHandlingController.TemplateMissing("New User", folder, ServerName);
                            }
                        }

                        TempData["message"] = "Added";

                        profilemodel.UserId = t.UserID;
                        profilemodel.BranchName = db.Master_Branches.FirstOrDefault(o => o.BranchID == profilemodel.BranchID).BranchName;

                        var objcompany = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                        string Title = " " + objcompany + " New Employee Added";
                        string Subject = " employee was added on " + DateTime.Today.ToString("dd MMM yyyy");


                        var checks = db.EmailTemplates.Where(x => x.TemplatePurpose == "New Employee Add").Select(o => o.EmailTemplateID).FirstOrDefault();
                        var folders = db.EmailTemplates.Where(o => o.TemplatePurpose == "New Employee Add").Select(x => x.TemplatePath).FirstOrDefault();
                        if ((checks != null) && (checks != 0))
                        {
                            var obj = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "New Employee Add")
                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                       select new DSRCManagementSystem.Models.Email
                                       {
                                           To = p.To,
                                           CC = p.CC,
                                           BCC = p.BCC,
                                           Subject = p.Subject,
                                           Template = q.TemplatePath
                                       }).FirstOrDefault();

                            string TemplatePath = Server.MapPath(obj.Template);
                            string html = System.IO.File.ReadAllText(TemplatePath);




                            if (profilemodel.IsBoarding)
                            {
                                Title = " " + objcompany + " Onboarding Employee Added";
                                Subject = " " + objcompany + " onboarding employee was added on " + DateTime.Today.ToString("dd MMM yyyy");
                                obj.Subject = "" + objcompany + " Management Portal-Onboarding Employee Added";
                            }

                            html = html.Replace("#Title", Title);
                            html = html.Replace("#Subject", Subject);
                            html = html.Replace("#EmployeeID", profilemodel.EmpID);
                            html = html.Replace("#UserId", profilemodel.UserId.ToString());
                            html = html.Replace("#EmployeeName", profilemodel.FirstName + " " + profilemodel.LastName);
                            html = html.Replace("#BranchName", profilemodel.BranchName);
                            html = html.Replace("#Department", profilemodel.DepartmentName);
                            html = html.Replace("#DesignationName", profilemodel.RoleName);
                            html = html.Replace("#JoiningDate", profilemodel.DateOfJoin.Value.ToString("dd MMM yyyy"));
                            html = html.Replace("#Experience", profilemodel.ExperienceYear + "." + profilemodel.ExperienceMonth);
                            html = html.Replace("#ServerName", ServerName);
                            html = html.Replace("#CompanyName", objcompany);

                            obj.To = UsersController.GetUserEmailAddress(db, obj.To);
                            obj.CC = UsersController.GetUserEmailAddress(db, obj.CC);

                            if (obj.BCC != "")
                            {
                                obj.BCC = UsersController.GetUserEmailAddress(db, obj.BCC);
                            }

                            //string ServerName1 = WebConfigurationManager.AppSettings["SeverName"];


                            List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                            string EmailAddress1 = "";

                            foreach (string mails in MailIds1)
                            {
                                EmailAddress1 += mails + ",";
                            }

                            EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);

                            if (ServerName != "http://win2012srv:88/")
                            {


                                string CCMailId = "aruna.m@dsrc.co.in";
                                string BCCMailId = "Kirankumar@dsrc.co.in ";

                                Task.Factory.StartNew(() =>
                                {
                                    // DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject + " - Test Mail Please Ignore", null, html + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                });

                            }
                            else
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    // DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", EmailAddress1, obj.CC, obj.BCC, Server.MapPath(logo.AppValue.ToString()));
                                    DsrcMailSystem.MailSender.SendMailToALL(null, obj.Subject, "", html, "HRMS@dsrc.co.in", EmailAddress1, obj.CC, obj.BCC, Server.MapPath(logo.ToString()));

                                });
                            }
                        }
                        else
                        {
                            //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                            ExceptionHandlingController.TemplateMissing("New Employee Add", folders, ServerName);
                        }

                    }
                    catch (Exception Ex)
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                        ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    }

                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", profilemodel.ExperienceYear);
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", profilemodel.ExperienceMonth);
                    ViewBag.Tech = new MultiSelectList(LoadTechnologies(), "ID", "Tecnology", profilemodel.Tecnology);

                }
            
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult EditUser(int Id)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            DSRCManagementSystemEntities1 objdb = new DSRCManagementSystemEntities1();
            try
            {
                obj_Users.UserId = Id;
                var departmentid = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DepartmentId).FirstOrDefault();

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType
                                      }).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).OrderBy(x => x.Name).ToList();
                var DepartmentList = (from dept in db.Departments

                                      select new
                                      {
                                          DepartmentId = dept.DepartmentId,
                                          DepartmentName = dept.DepartmentName
                                      }).ToList();

                var DesignationList = (from
                                    r in db.Master_Designation
                                       select new
                                       {
                                           DesignationID = r.DesignationID,
                                           DesignationName = r.DesignationName
                                       }).OrderBy(x => x.DesignationName).ToList();



                var TechList = (from
                                tl in db.Master_Technologies
                                select new
                                {
                                    ID = tl.ID,
                                    Tecnology = tl.Tecnology
                                }).ToList();

                var RelationShip = (from r in db.Master_Relationship
                                    select new
                                    {
                                        RelationShipID = r.RelationshipID,
                                        RelationShipName = r.RelationshipName
                                    }).ToList();

                var Religious = (from r in db.Master_Religious
                                 select new
                                 {
                                     ReligiousID = r.ReligiousID,
                                     ReligiousName = r.ReligiousName
                                 }).ToList();

                var Nationality = (from r in db.Master_Nationality
                                   select new
                                   {
                                       NationalityID = r.NationalityID,
                                       NationalityName = r.NationalityName
                                   }).ToList();
                var BloogGroup = (from b in db.Master_BloodGroup
                                  select new
                                  {
                                      BloogGroup = b.BloodGroupName,
                                      BloodGroupID = b.BloodGroupID
                                  }).ToList();
                var Roles = (from role in db.Master_Roles
                             select new
                             {
                                 RollName = role.RoleName,
                                 RollID = role.RoleID
                             });


                var Country = (from c in db.Master_Country
                               select new
                               {
                                   CountryID = c.CountryID,
                                   CountryName = c.CountryName


                               }).ToList();

                var state = (from s in db.Master_States
                             select new
                             {
                                 StateID = s.StateID,
                                 StateName = s.States
                             }).ToList();





                var city = (from c in db.Master_City
                            select new

                            {
                                CityID = c.CityID,
                                CityName = c.CityName

                            }).ToList();


                var BranchesList = (from b in db.Master_Branches
                                    select new
                                    {
                                        BranchID = b.BranchID,
                                        BranchName = b.BranchName
                                    }).ToList();
                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();

                List<int> ChildCount = new List<int>();
                for (int i = 1; i < 10; i++)
                {
                    ChildCount.Add(i);
                }
                ViewBag.Child = new SelectList(ChildCount, "", "");

                obj_Users.UserId = Id;

                var Isblock = db.Users.Where(x => x.UserID == obj_Users.UserId && x.Attempts > 5).Select(x => x.UserID).FirstOrDefault();

                List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();

                objuser = (from p in db.UserReportings.Where(x => x.UserID == Id)
                           select new DSRCManagementSystem.Models.ListReporting
                           {
                               Id = p.ReportingUserID

                           }).ToList();

                List<int> selectedAttendees = new List<int>();


                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                }

                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join n in db.Master_Nationality on u.NationalityID equals n.NationalityID into country
                                     from National in country.DefaultIfEmpty()
                                     join Roll in db.Master_Roles on u.RoleID equals Roll.RoleID into Rolls
                                     from rol in Rolls.DefaultIfEmpty()
                                     join r in db.Master_Relationship on u.RelationshipID equals r.RelationshipID into relation
                                     from Relation in relation.DefaultIfEmpty()
                                     join persentAddress in db.PresentAddresses on u.TemporaryAddressID equals persentAddress.PresentAddressID into Temp
                                     from Tem in Temp.DefaultIfEmpty()
                                     join Permanent in db.PermanentAddresses on u.PermanentAddressID equals Permanent.PermanentAddressID into Terma
                                     from Perm in Terma.DefaultIfEmpty()
                                     join s in db.Master_Religious on u.ReligionID equals s.ReligiousID into Religion
                                     from Rel in Religion.DefaultIfEmpty()
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID into status
                                     from v in status.DefaultIfEmpty()
                                     join us in db.UserSkills on u.UserID equals us.UserID into value
                                     from user in value.DefaultIfEmpty()
                                     from dn in DeptID.DefaultIfEmpty()

                                     where u.UserID == Id
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         EMPCODE = u.EmpCode,
                                         DepartmentName = dn.DepartmentName,
                                         DepartmentId = u.DepartmentId,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         LastName = u.LastName,
                                         UserName = u.UserName,
                                         Gender = g.GenderName,
                                         GenderID = u.Gender,
                                         Password = u.Password,
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth),
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin),
                                         ContactNo = u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         ResignedOn = u.ResignedOn,
                                         LastworkingDate = u.LastWorkingDate,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         onboarding = u.IsBoarding ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         Block = Isblock > 0 ? true : false,
                                         IsBoarding = u.IsBoarding ?? false,
                                         Experience = u.Experience,
                                         ID = user.ID,
                                         Tecnology = user.Skills,
                                         WorkPlace = u.Workplace,
                                         Marital = v.MaritalStatusType,
                                         MaritalStatusId = (int)u.MaritalStatus,
                                         BranchID = (int)u.BranchId,
                                         PermanentAddress = u.PermanentAddressID,
                                         IPAddress = u.IPAddress,
                                         BranchName = b.BranchName,
                                         RegionId = u.Region,
                                         GroupId = u.DepartmentGroup,
                                         MachineName = u.MachineName,

                                         OfficeNo = u.officeno,
                                         OfficeSkypeId = u.OfficeSkypeId,
                                         Extension = u.extension,
                                         SelectedUserStatusid = u.UserStatus,
                                         Religious = Rel.ReligiousName,
                                         ReligiousID = Rel.ReligiousID,
                                         RelationShipID = Relation.RelationshipID,
                                         RelationShipName = Relation.RelationshipName,
                                         Nationality = National.NationalityName,
                                         NationalityID = National.NationalityID,
                                         BloodGroup = u.BloodGroup,
                                         FatherName = u.FatherName,
                                         MotherName = u.MotherName,
                                         BirthPlace = u.BirthPlace,
                                         SpouseName = u.SpouseName,
                                         NoOfChild = u.NoOfChild,
                                         AnniversaryDate = u.AnniversaryDate,
                                         EmergencyContactName = u.Contactperson,
                                         Emergency = u.EmergencyContact,
                                         PermanentAddress1 = Perm.Address_1,
                                         PermanentAddress3 = Perm.Address_3,
                                         PermanentAddress2 = Perm.Address_2,
                                         PermanentCity = Perm.City,
                                         Permanentstate = Perm.State,
                                         PermanentCountryID = Perm.CountryID,
                                         PermanentPinCode = Perm.Zip,
                                         PresentAddress1 = Tem.Address_1,
                                         PresentAddress2 = Tem.Address_2,
                                         PresentAddress3 = Tem.Address_3,
                                         PresentCity = Tem.City,
                                         Presentstate = Tem.State,
                                         PresentCountryID = Tem.CountryID,
                                         PresentPinCode = Tem.Zip,
                                         RollID = u.RoleID,
                                         RollName = rol.RoleName,
                                         DirectReportingTo=u.DirectReportingTo
                                     }).FirstOrDefault();


                ViewBag.EmpID = userViewModel.EmpID;
                ViewBag.EmpCode = userViewModel.EMPCODE;

                var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                  join q in db.UserRoles on p.RoleID equals q.RoleID
                                  select q.UserID).ToList();
                var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                foreach (var id in userbyid)
                {
                    userbyrole.Add(id);
                }
                var fullusers = userbyrole.Distinct().ToList();

                List<object> EmployeeList = new List<object>();
                int Branch = userViewModel.BranchID;

                foreach (var userid in fullusers)
                {
                    var name =
                        db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == Branch)
                            .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                            .FirstOrDefault();
                    if (name != null)
                    {
                        var Val = new { ID1 = userid, UserName1 = name };
                        EmployeeList.Add(Val);
                    }
                }

                ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);

                var GroupList = (from dg in db.DepartmentGroups.Where(dg => dg.IsActive == true)
                                 join dgm in db.DepartmentGroupMappings.Where(dgm => dgm.DepartmentID == userViewModel.DepartmentId) on dg.GroupID equals dgm.GroupID
                                 select new
                                 {
                                     groupid = dg.GroupID,
                                     groupname = dg.GroupName
                                 }).OrderBy(x => x.groupname).ToList();

                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();



                var objdepartment = (from p in objdb.Departments

                                     select new
                                     {
                                         DepartmentId = p.DepartmentId,
                                         DepartmentName = p.DepartmentName,

                                     }).OrderBy(x => x.DepartmentName).ToList();

                // var temp = new SelectList(db.Departments.OrderBy(d => d.DepartmentName), "DepartmentId", "DepartmentName", Userdetails.DepartmentId);
                var selected = from d in db.Departments where d.DepartmentId == userViewModel.DepartmentId select new { DepartmentId = d.DepartmentId };

                if (userViewModel != null)
                {

                    userViewModel.DateOfJoin = userViewModel.DateOfJoin == null ? userViewModel.DateOfJoin : userViewModel.DateOfJoin.Value.Date;

                    string[] experience = null;

                    if ((userViewModel.Experience != null) && (userViewModel.Experience != "0"))
                    {
                        experience = userViewModel.Experience.Split('.');
                    }
                    else
                    {
                        experience = "0.0".Split('.');
                    }

                    if (userViewModel.WorkPlace != null)
                    {
                        userViewModel.WorkplaceId = Convert.ToInt32(userViewModel.WorkPlace);
                    }

                    else
                    {
                        userViewModel.WorkplaceId = 1;

                    }

                    int presentcity, presentState, presentCountry = 0;
                    if (userViewModel.PresentCity != null && userViewModel.PresentCity != "")
                        presentcity = Convert.ToInt32(userViewModel.PresentCity);
                    else
                        presentcity = 0;

                    if (userViewModel.Presentstate != null && userViewModel.Presentstate != "")
                        presentState = Convert.ToInt32(userViewModel.Presentstate);
                    else
                        presentState = 0;

                    if (userViewModel.PresentCountryID != null)// || userViewModel.PresentCountryID != "")
                        presentCountry = Convert.ToInt32(userViewModel.PresentCountryID);
                    else
                        presentCountry = 0;
                    var PresentcountryID = db.Master_Country.Where(x => x.CountryID == presentCountry).Select(o => o.CountryID).FirstOrDefault();
                    var PresentcityID = db.Master_City.Where(x => x.CityID == presentcity).Select(o => o.CityID).FirstOrDefault();
                    var PresentStateID = db.Master_States.Where(x => x.StateID == presentState).Select(o => o.StateID).FirstOrDefault();


                    int permanentCity, permanentSate, permanentCountry = 0;

                    if (userViewModel.PermanentCity != null && userViewModel.PermanentCity != "")
                        permanentCity = Convert.ToInt32(userViewModel.PermanentCity);
                    else
                        permanentCity = 0;

                    if (userViewModel.Permanentstate != null && userViewModel.Permanentstate != "") 
                        permanentSate = Convert.ToInt32(userViewModel.Permanentstate);
                    else
                        permanentSate = 0;

                    if (userViewModel.PermanentCountryID != null)// || userViewModel.PermanentCountryID != "")
                        permanentCountry = Convert.ToInt32(userViewModel.PermanentCountryID);
                    else
                        permanentCountry = 0;
                    
                    var PermanentcountryID = db.Master_Country.Where(x => x.CountryID == permanentCountry).Select(o => o.CountryID).FirstOrDefault();
                    var PermanentcityID = db.Master_City.Where(x => x.CityID == permanentCity).Select(o => o.CityID).FirstOrDefault();
                    var PermanentStateID = db.Master_States.Where(x => x.StateID == permanentSate).Select(o => o.StateID).FirstOrDefault();

                    //Added on 22/09
                    var Technology = db.UserSkills.Where(x => x.UserID == Id).Select(o => o.Skills).FirstOrDefault();

                    //Added on 19/09

                   

                    ViewBag.Roles = new SelectList(Roles, "RollID ", "RollName", userViewModel.RollID);
                    ViewBag.PermanentCountry = new SelectList(Country, "CountryID", "CountryName", permanentCountry);
                    ViewBag.BloogGroup = new SelectList(BloogGroup, "BloodGroupID", "BloogGroup", userViewModel.BloodGroup);
                    ViewBag.Permanentstate = new SelectList(state, "StateID", "StateName", PermanentStateID);
                    ViewBag.Permanentcity = new SelectList(city, "CityID", "CityName", PermanentcityID);
                    ViewBag.PresentCountry = new SelectList(Country, "CountryID", "CountryName", presentCountry);
                    ViewBag.Presentstate = new SelectList(state, "StateID", "StateName", PresentStateID);
                    ViewBag.Presentcity = new SelectList(city, "CityID", "CityName", PresentcityID);
                    userViewModel.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == userViewModel.WorkplaceId).WorkPlaceName;
                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(experience[0]));
                                   
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(experience[1]));
                     ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", userViewModel.Tecnology);
                    ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", userViewModel.BranchID);

                    ViewBag.Department = new SelectList(objdepartment, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name", userViewModel.WorkplaceId);
                    ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", userViewModel.MaritalStatusId);
                    ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", userViewModel.DesignationID);
                    ViewBag.Region = new SelectList(Zone, "RegionId", "Region", userViewModel.RegionId);
                    ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", userViewModel.GroupId);
                    ViewBag.Status = new SelectList(UserStatus, "userstatusid", "userstatusname", userViewModel.SelectedUserStatusid);
                    ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName", userViewModel.GenderID);
                    ViewBag.Religious = new SelectList(Religious, "ReligiousID", "ReligiousName", userViewModel.ReligiousID);
                    ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID", "RelationShipName", userViewModel.RelationShipID);
                    ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName", userViewModel.NationalityID);
                }

                else
                {
                    DSRCManagementSystem.Models.UserModel objmodel = new DSRCManagementSystem.Models.UserModel();

                    string[] experience = null;
                    DSRCManagementSystemEntities1 odb = new DSRCManagementSystemEntities1();
                    var join = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DateOfJoin).FirstOrDefault();

                    objmodel.DateOfJoin = join.Value.Date;
                    objmodel.MiddleName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MiddleName).FirstOrDefault();
                    objmodel.LastName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.LastName).FirstOrDefault();
                    objmodel.EmailAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.EmailAddress).FirstOrDefault();
                    objmodel.UserName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.UserName).FirstOrDefault();
                    objmodel.DateOfBirth = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DateOfJoin).FirstOrDefault();
                    var No = objdb.Users.Where(x => x.UserID == Id).Select(o => o.ContactNo).FirstOrDefault();
                    objmodel.ContactNo = No;
                    objmodel.IPAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IPAddress).FirstOrDefault();
                    objmodel.MachineName = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MachineName).FirstOrDefault();
                    objmodel.DirectReportingTo = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DirectReportingTo).FirstOrDefault();
                    bool? notice = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsUnderNoticePeriod).FirstOrDefault();
                    objmodel.IsUnderNoticePeriod = Convert.ToBoolean(notice);
                    bool? propation = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsUnderProbation).FirstOrDefault();
                    objmodel.IsUnderProbation = Convert.ToBoolean(propation);
                    bool? first = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsFirstLogin).FirstOrDefault();
                    objmodel.IsFirstLogin = Convert.ToBoolean(first);
                    bool? isactive = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsActive).FirstOrDefault();
                    objmodel.IsActive = Convert.ToBoolean(isactive);
                    objmodel.PAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.PermanentAddressID).FirstOrDefault();
                    objmodel.TAddress = objdb.Users.Where(x => x.UserID == Id).Select(o => o.TemporaryAddressID).FirstOrDefault();
                    objmodel.ResignedOn = objdb.Users.Where(x => x.UserID == Id).Select(o => o.ResignedOn).FirstOrDefault();
                    objmodel.LastworkingDate = objdb.Users.Where(x => x.UserID == Id).Select(o => o.LastWorkingDate).FirstOrDefault();
                    bool? board = objdb.Users.Where(x => x.UserID == Id).Select(o => o.IsBoarding).FirstOrDefault();
                    objmodel.IsBoarding = Convert.ToBoolean(board);
                    var emp = objdb.Users.Where(x => x.UserID == Id).Select(o => o).FirstOrDefault();

                    if (emp.EmpID == null)
                    {
                        objmodel.EmpID = "";
                    }

                    var Technology = objdb.UserSkills.Where(x => x.UserID == Id).Select(o => o.Skills).FirstOrDefault();
                    var BranchId = objdb.Users.Where(x => x.UserID == Id).Select(o => o.BranchId).FirstOrDefault();
                    var WorkPlace = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Workplace).FirstOrDefault();
                    var Gender = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Gender).FirstOrDefault();
                    var Maritial = objdb.Users.Where(x => x.UserID == Id).Select(o => o.MaritalStatus).FirstOrDefault();
                    var Designation = objdb.Users.Where(x => x.UserID == Id).Select(o => o.DesignationID).FirstOrDefault();
                    var Exp = objdb.Users.Where(x => x.UserID == Id).Select(o => o.Experience).FirstOrDefault();
                    if (Exp != null)
                    {
                        experience = Exp.Split('.');
                    }
                    else
                    {
                        experience = "0.0".Split('.');
                    }
                    ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(experience[0]));
                    ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(experience[1]));
                    ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", Technology);
                    ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", BranchId);

                    ViewBag.DepartmentNameList = new SelectList(db.Departments.OrderBy(d => d.DepartmentName).Where(d => d.DepartmentId == userViewModel.DepartmentId), "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                    ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "Name", WorkPlace);
                    ViewBag.Department = new SelectList(objdepartment, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                    ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName", Gender);
                    ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", Maritial);
                    ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", Designation);
                    ViewBag.Status = new SelectList(UserStatus, "userstatusid", "userstatusname", userViewModel.SelectedUserStatusid);
                    ViewBag.Religious = new SelectList(Religious, "ReligiousID ", "ReligiousName", userViewModel.ReligiousID);
                    ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName", userViewModel.RelationShipID);
                    ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName", userViewModel.NationalityID);
                    return View(objmodel);
                }
                return View(userViewModel);
            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditUser(UserModel model)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var Isblock = db.Users.Where(x => x.UserID == model.UserId && x.Attempts > 5).Select(x => x.UserID).FirstOrDefault();


                List<int?> objuser = new List<int?>();

                string[] value = model.multiselectemployees.Split(',');

                for (int i = 0; i < value.Count(); i++)
                {
                    objuser.Add(Convert.ToInt32(value[i]));
                }


                var RelationShip = (from r in db.Master_Relationship
                                    select new
                                    {
                                        RelationShipID = r.RelationshipID,
                                        RelationShipName = r.RelationshipName
                                    }).ToList();

                var Religious = (from r in db.Master_Religious
                                 select new
                                 {
                                     ReligiousID = r.ReligiousID,
                                     ReligiousName = r.ReligiousName
                                 }).ToList();

                var Nationality = (from r in db.Master_Nationality
                                   select new
                                   {
                                       NationalityID = r.NationalityID,
                                       NationalityName = r.NationalityName
                                   }).ToList();
                var objdepartment = (from p in db.Departments

                                     select new
                                     {
                                         DepartmentId = p.DepartmentId,
                                         DepartmentName = p.DepartmentName,

                                     }).OrderBy(x => x.DepartmentName).ToList();
                List<int> ChildCount = new List<int>();
                for (int i = 1; i < 10; i++)
                {
                    ChildCount.Add(i);
                }
            
                ViewBag.Child = new SelectList(ChildCount, "", "");
                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();
                var Empid = db.Users.Where(x => x.EmpID == model.EmpID).Select(o => o).ToList();
                int userID;

                if (Empid.Any())
                    userID = Convert.ToInt32(Empid[0].UserID);
                else
                    userID = 0;


                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join n in db.Master_Nationality on u.NationalityID equals n.NationalityID into country
                                     from National in country.DefaultIfEmpty()
                                     join r in db.Master_Relationship on u.RelationshipID equals r.RelationshipID into relation
                                     from Relation in relation.DefaultIfEmpty()
                                     join Marital in db.Master_MaritalStatus on u.MaritalStatus equals Marital.MaritalStatusID into MaritalID
                                     from Maritals in MaritalID.DefaultIfEmpty()
                                     join persentAddress in db.PresentAddresses on u.TemporaryAddressID equals persentAddress.PresentAddressID into Temp
                                     from Tem in Temp.DefaultIfEmpty()
                                     join Permanent in db.PermanentAddresses on u.PermanentAddressID equals Permanent.PermanentAddressID into Terma
                                     from Perm in Terma.DefaultIfEmpty()
                                     join s in db.Master_Religious on u.ReligionID equals s.ReligiousID into Religion
                                     from Rel in Religion.DefaultIfEmpty()
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID into status
                                     from v in status.DefaultIfEmpty()
                                     join us in db.UserSkills on u.UserID equals us.UserID into value1
                                     from user in value1.DefaultIfEmpty()
                                     from dn in DeptID.DefaultIfEmpty()
                                     join roll in db.Master_Roles on u.RoleID equals roll.RoleID into val
                                     from use in val.DefaultIfEmpty()
                                     where u.UserID == userID
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID??"",
                                         EMPCODE = u.EmpCode??"",
                                         DepartmentName = dn.DepartmentName??"",
                                         DepartmentId = u.DepartmentId??0,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName??"",
                                         FirstName = u.FirstName??"",
                                         MiddleName = u.MiddleName??"",
                                         LastName = u.LastName??"",
                                         UserName = u.UserName??"",
                                         Gender = g.GenderName??"",
                                         GenderID = u.Gender??0,
                                         Password = u.Password??"",
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth)??null,
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin)??null,
                                         ContactNo = u.ContactNo??0,
                                         EmailAddress = u.EmailAddress??"",
                                         ResignedOn = u.ResignedOn??null,
                                         LastworkingDate = u.LastWorkingDate??null,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         onboarding = u.IsBoarding ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         // Block = Isblock > 0 ? true : false,
                                         RoleID = (byte)u.RoleID,
                                         IsBoarding = u.IsBoarding ?? false,
                                         Experience = u.Experience??"",
                                         ID = user.ID,
                                         Tecnology = user.Skills??"",
                                         WorkPlace = u.Workplace??"",
                                         Marital = Maritals.MaritalStatusType??"",
                                         MaritalStatusId = Maritals.MaritalStatusID,
                                         BranchID = (int)u.BranchId,
                                         PermanentAddress = u.PermanentAddressID??0,
                                         IPAddress = u.IPAddress??"",
                                         BranchName = b.BranchName??"",
                                         RegionId = u.Region??0,
                                         GroupId = u.DepartmentGroup??0,
                                         MachineName = u.MachineName??"",

                                         OfficeNo = u.officeno??0,
                                         OfficeSkypeId = u.OfficeSkypeId??"",
                                         Extension = u.extension??0,
                                         SelectedUserStatusid = u.UserStatus??0,
                                         Religious = Rel.ReligiousName??"",
                                         ReligiousID = Rel.ReligiousID,
                                         RelationShipID = Relation.RelationshipID,
                                         RelationShipName = Relation.RelationshipName??"",
                                         Nationality = National.NationalityName??"",
                                         NationalityID = National.NationalityID,
                                         BloodGroup = u.BloodGroup??"",
                                         FatherName = u.FatherName??"",
                                         MotherName = u.MotherName??"",
                                         BirthPlace = u.BirthPlace??"",
                                         SpouseName = u.SpouseName??"",
                                         NoOfChild = u.NoOfChild??0,
                                         AnniversaryDate = u.AnniversaryDate??null,
                                         EmergencyContactName = u.Contactperson??"",
                                         Emergency = u.EmergencyContact??0,
                                         PermanentAddress1 = Perm.Address_1??"",
                                         PermanentAddress3 = Perm.Address_3??"",
                                         PermanentAddress2 = Perm.Address_2??"",
                                         PermanentCity = Perm.City??"",
                                         Permanentstate = Perm.State??"",
                                         PermanentPinCode = Perm.Zip??0,
                                         PresentAddress1 = Tem.Address_1??"",
                                         PresentAddress2 = Tem.Address_2??"",
                                         PresentAddress3 = Tem.Address_3??"",
                                         PresentCity = Tem.City??"",
                                         Presentstate = Tem.State??"",
                                         PresentPinCode = Tem.Zip??0,


                                     }).FirstOrDefault();


                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();



                var DepartmentList = (from
                                      d in db.Departments
                                      select new
                                      {
                                          DepartmentId = d.DepartmentId,
                                          DepartmentName = d.DepartmentName
                                      });

                var DesignationList = (from
                                    r in db.Master_Designation

                                       select new
                                       {
                                           DesignationID = r.DesignationID,
                                           DesignationName = r.DesignationName
                                       }).ToList();

                var TechList = (from
                                tl in db.Master_Technologies
                                select new
                                {
                                    ID = tl.ID,
                                    Tecnology = tl.Tecnology
                                }).ToList();
                //var skills = (from p in db.UserSkills
                //                 select new
                //                 {
                //                     ID = p.ID,
                //                     Skills = p.Skills
                //                 }).ToList();

                //var WorkList = (from c in db.Users
                //                select new
                //                {
                //                    WorkPlace = c.Workplace
                //                }).ToList();

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType

                                      }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     WorkPlace = p.WorkPlaceName

                                 }).ToList();
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();

                var GroupList = (from dg in db.DepartmentGroups.Where(dg => dg.IsActive == true)
                                 join dgm in db.DepartmentGroupMappings.Where(dgm => dgm.DepartmentID == userViewModel.DepartmentId) on dg.GroupID equals dgm.GroupID
                                 select new
                                 {
                                     groupid = dg.GroupID,
                                     groupname = dg.GroupName
                                 }).OrderBy(x => x.groupname).ToList();


                List<DSRCManagementSystem.Models.ListReporting> objuser1 = new List<DSRCManagementSystem.Models.ListReporting>();

                objuser1 = (from p in db.UserReportings.Where(x => x.UserID == userID)
                            select new DSRCManagementSystem.Models.ListReporting
                            {
                                Id = p.ReportingUserID

                            }).ToList();

                List<int> selectedAttendees = new List<int>();


                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedAttendees.Add(Convert.ToInt32(objuser[i]));
                }
                List<object> EmployeeList = new List<object>();

                var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                  join q in db.UserRoles on p.RoleID equals q.RoleID
                                  select q.UserID).ToList();
                var fullusers = userbyrole.Distinct().ToList();
                int Branch = userViewModel.BranchID;
                foreach (var userid in fullusers)
                {
                    var name =
                        db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == Branch)
                            .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                            .FirstOrDefault();
                    var Val = new { ID1 = userid, UserName1 = name };
                    EmployeeList.Add(Val);
                }
                var BranchesList = (from b in db.Master_Branches
                                    select new
                                    {
                                        BranchID = b.BranchID,
                                        BranchName = b.BranchName
                                    }).ToList();
                ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", userViewModel.GroupId);
                ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);
                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(model.ExperienceYear));
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(model.ExperienceYear));
                ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", userViewModel.Tecnology);
                //ViewBag.Tech = new MultiSelectList(skills, "ID", "Tecnology", userViewModel.Tecnology);
                ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", userViewModel.BranchID);

                ViewBag.Department = new SelectList(objdepartment, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "WorkPlace ", userViewModel.WorkplaceId);

                ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", userViewModel.Marital);
                ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", userViewModel.DesignationID);
                ViewBag.Region = new SelectList(Zone, "RegionId", "Region", userViewModel.RegionId);
                ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", userViewModel.GroupId);
                ViewBag.Status = new SelectList(UserStatus, "userstatusid", "userstatusname", userViewModel.SelectedUserStatusid);
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName", userViewModel.GenderID);
                ViewBag.Religious = new SelectList(Religious, "ReligiousID ", "ReligiousName", userViewModel.ReligiousID);
                ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName", userViewModel.RelationShipID);
                ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName", userViewModel.NationalityID);
                DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();

                var a = Regex.Matches(model.EmpID, @"[a-zA-Z]").Count;

                string EmployeeID = model.EmpID;

                if (model.EmpID.Length <= 5 && a == 0 || a != 0)
                {

                    if (model.EmpID.Length == 1 && a == 0)
                    {
                        model.EmpID = "0000" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 2 && a == 0)
                    {
                        model.EmpID = "000" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 3 && a == 0)
                    {
                        model.EmpID = "00" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (model.EmpID.Length == 4 && a == 0)
                    {
                        model.EmpID = "0" + model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                    if (a != 0)
                    {
                        model.EmpID = model.EmpID;
                        EmployeeID = model.EmpID;
                    }
                }

                if (model.BranchID != null)
                model.BranchID = Convert.ToInt32(model.BranchName);

                var empidCheck = db.Users.Where(o => o.UserID != userID).FirstOrDefault(x => x.EmpID == model.EmpID && x.BranchId == model.BranchID);
                var emailAddressCheck = db.Users.Where(o => o.UserID != userID).FirstOrDefault(x => x.EmailAddress == model.EmailAddress);

                if (empidCheck == null)
                {
                    if (emailAddressCheck == null)
                    {
                        if (true)
                        {
                            Glopal.IsSesionExpired = false;



                            var obj = db.Users.Where(o => o.UserID == userID).Select(o => o).FirstOrDefault();


                            var per = db.PermanentAddresses.Where(x => x.PermanentAddressID == obj.PermanentAddressID).FirstOrDefault();
                            var temp = db.PresentAddresses.Where(x => x.PresentAddressID == obj.TemporaryAddressID).FirstOrDefault();



                            var objreporting = db.UserReportings.Where(x => x.UserID == model.UserId).Select(o => o).ToList();
                           
                            if(model.DepartmentId != null)
                            obj.DepartmentId = Convert.ToInt32(model.DepartmentId);

                            if (model.EmpID != null && model.EmailAddress != null && model.EMPCODE != null)
                            {
                                obj.EmpID = model.EmpID;
                                obj.EmpCode = model.EMPCODE;
                                obj.EmailAddress = model.EmailAddress;
                                obj.UserName = model.UserName;
                                obj.IsBoarding = model.IsBoarding;
                            }
                            obj.IsExclude = model.BranchID != 1 ? true : false;
                            obj.BranchId = model.BranchID; // addded on 30/9
                            obj.EmpID = model.EmpID;
                            obj.FirstName = model.FirstName;
                            obj.MiddleName = model.MiddleName;
                            obj.LastName = model.LastName;
                            obj.DateOfBirth = model.DateOfBirth;
                            obj.EmailAddress = model.EmailAddress;
                            obj.DateOfJoin = model.DateOfJoin;
                            obj.ContactNo = model.ContactNo == null ? (long?)null : Convert.ToInt64(model.ContactNo);
                           
                            if (model.GenderID !=null)
                            obj.Gender = Convert.ToInt32(model.GenderID);
                           
                           // obj.MachineName = model.MachineName;
                            obj.IsUnderProbation = model.IsUnderProbation;
                            obj.IsUnderNoticePeriod = model.IsUnderNoticePeriod;
                            //obj.IsActive = model.IsActive;
                            if (obj.LastWorkingDate > DateTime.Now.Date)
                            {
                                obj.IsActive = true;
                            }
                            else
                            {
                                obj.IsActive = false;
                                if (obj.LastWorkingDate == null)
                                {
                                    obj.IsActive = true;
                                }
                            }
                            obj.Experience = model.ExperienceYear + "." + model.ExperienceMonth;
                            var Workplace = Convert.ToString(model.WorkplaceId);

                            obj.Workplace = Convert.ToString(model.WorkplaceId);
                            //obj.IPAddress = model.IPAddress;
                            //obj.PermanentAddressID = model.PermanentAddress;
                            //obj.officeno = model.OfficeNo;
                            //obj.extension = model.Extension;
                           // obj.OfficeSkypeId = model.OfficeSkypeId;

                            obj.MaritalStatus = model.MaritalStatusId;

                            if (model.DesignationID != null)
                            obj.DesignationID = Convert.ToInt32(model.DesignationID);

                            obj.DepartmentGroup = model.GroupId;
                            obj.Region = model.RegionId;

                            obj.FatherName = model.FatherName;
                            obj.MotherName = model.MotherName;
                            obj.BirthPlace = model.BirthPlace;  //added on 26/9
                            obj.BloodGroup = model.BloodGroup;
                            obj.NationalityID = model.NationalityID;
                            obj.NoOfChild = model.NoOfChild;
                            obj.SpouseName = model.SpouseName;
                            obj.DirectReportingTo =model.multiselectemployees ;
                           // obj.DirectReportingTo = Convert.ToString(objuser1);
                            obj.AnniversaryDate = model.AnniversaryDate;
                            obj.ReligionID = model.ReligiousID;
                           // obj.RelationshipID = model.RelationShipID;
                            obj.RoleID = (byte)model.RollID;
                            obj.EmergencyContact = model.Emergency;
                            obj.RelationshipID = model.RelationShipID;
                            obj.Contactperson = model.EmergencyContactName;

                            int CID = Convert.ToInt32(model.PermanentCountry);
                            int PermanentCountryID = db.Master_Country.Where(x => x.CountryID == CID).Select(o => o.CountryID).FirstOrDefault();
                            per.Address_1 = model.PermanentAddress1;
                            per.Address_2 = model.PermanentAddress2;
                            per.Address_3 = model.PermanentAddress3;
                            per.City = Convert.ToString(model.PermanentCityID);
                            per.State = Convert.ToString(model.PermanentstateID);
                            per.CountryID = PermanentCountryID;// Int32.Parse(model.PermanentCountry);
                            per.Zip = model.PermanentPinCode;

                           int PresentCountryID = db.Master_Country.Where(x => x.CountryName == model.PresentCountry).Select(o => o.CountryID).FirstOrDefault();
                            temp.Address_1 = model.PresentAddress1;
                            temp.Address_2 = model.PresentAddress2;
                            temp.Address_3 = model.PresentAddress3;
                            temp.City = Convert.ToString(model.PresentCityID);
                            temp.State = Convert.ToString(model.PresentstateID);
                            temp.CountryID = PresentCountryID;
                            temp.Zip = model.PresentPinCode;

                            var user = db.UserRoles.Where(o => o.UserID == model.UserId).FirstOrDefault();
                           // user.UserID = model.UserId;
                            //   user.RoleID = db.Master_Roles.FirstOrDefault(o => o.RoleName == MasterEnum.NewuserRole.NewEmployeeRole).RoleID;
                            user.RoleID = Convert.ToByte(obj.RoleID);
//db.UserRoles.AddObject(user);                           
                            db.SaveChanges();






                            if (model.IsUnderNoticePeriod)
                            {
                                obj.ResignedOn = model.ResignedOn;
                                obj.LastWorkingDate = model.LastworkingDate;
                            }
                            else
                            {
                                obj.ResignedOn = null;
                                obj.LastWorkingDate = null;
                            }

                            if (model.IsActive)
                            {
                                if (model.LastworkingDate.HasValue && model.LastworkingDate.Value.Date < DateTime.Today.Date)
                                {
                                    obj.IsActive = false;
                                }
                                else
                                {
                                    obj.IsActive = true;
                                }
                            }
                            if (model.Block)
                            {
                                obj.Attempts = 6;
                            }
                            else
                            {
                                obj.Attempts = 0;
                            }
                            db.SaveChanges();


                            var skillobj = db.UserSkills.Where(o => o.UserID == model.UserId).Select(o => o).FirstOrDefault();

                            if (skillobj != null)
                            {
                                skillobj.Skills = model.Tecnology;
                                db.SaveChanges();
                            }
                            else
                            {
                                var skill = db.UserSkills.CreateObject();
                                skill.UserID = userID;
                                skill.Skills = model.Tecnology;

                                db.UserSkills.AddObject(skill);
                                db.SaveChanges();
                            }
                            var FirstNametrim = obj.FirstName.Trim();
                            var LastNametrim = obj.LastName.Trim();
                            int WorkplaceId =0;
                           
                            if(Workplace != null)
                             WorkplaceId = Convert.ToInt32(Workplace);

                            UpdateUser updateuser = new Models.UpdateUser();
                            {

                                updateuser.EmpID = obj.EmpID;
                                updateuser.DepartmentName = obj.Department.DepartmentName;
                                updateuser.DesignationName = db.Master_Designation.FirstOrDefault(o => o.DesignationID == obj.DesignationID).DesignationName;
                                updateuser.FirstName = FirstNametrim;
                                updateuser.MiddleName = obj.MiddleName;
                                updateuser.LastName = LastNametrim;
                                updateuser.DateOfBirth = obj.DateOfBirth != null ? obj.DateOfBirth.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                updateuser.DateOfJoin = obj.DateOfJoin != null ? obj.DateOfJoin.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                updateuser.ContactNo = obj.ContactNo;
                                updateuser.EmailAddress = obj.EmailAddress;
                                updateuser.GenderName = obj.Gender == 1 ? "Male" : "Female";
                                updateuser.Experience = obj.Experience;
                                // updateuser.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                // updateuser.WorkPlace = db.WorkPlaces.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                updateuser.WorkPlace = Convert.ToString(Workplace);
                                updateuser.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName;
                                // updateuser.Marital = db.Master_MaritalStatus.FirstOrDefault(o => o.MaritalStatusID == obj.MaritalStatus).MaritalStatusType;
                                updateuser.Skills = model.Tecnology == "Skills" ? "" : model.Tecnology;
                                updateuser.BranchName = db.Master_Branches.FirstOrDefault(x => x.BranchID == obj.BranchId).BranchName;
                                updateuser.FatherName = model.FatherName;
                                updateuser.MotherName = model.MotherName;
                                updateuser.BloodGroup = model.BloodGroup;
                                updateuser.NationalityID = model.NationalityID;
                                updateuser.NoOfChild = model.NoOfChild;
                                updateuser.AnniversaryDate = model.AnniversaryDate;
                                updateuser.ReligiousID = model.ReligiousID;
                                updateuser.RelationShipID = model.RelationShipID;
                                updateuser.RollID = model.RollID;







                            }


                            SaveUser saveuser = new SaveUser();
                            {
                                if (userViewModel != null)
                                {
                                    saveuser.EmpID = userViewModel.EmpID;
                                    saveuser.DepartmentName = userViewModel.DepartmentName;
                                    saveuser.DesignationName = userViewModel.DesignationName;
                                    saveuser.FirstName = userViewModel.FirstName;
                                    saveuser.MiddleName = userViewModel.MiddleName;
                                    saveuser.LastName = userViewModel.LastName;
                                    saveuser.DateOfBirth = userViewModel.DateOfBirth != null ? userViewModel.DateOfBirth.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                    saveuser.DateOfJoin = userViewModel.DateOfJoin != null ? userViewModel.DateOfJoin.Value.Date.ToString("dd MMM yyyy") : string.Empty;
                                    saveuser.ContactNo = userViewModel.ContactNo == 0 ? 0 : userViewModel.ContactNo;
                                    saveuser.EmailAddress = userViewModel.EmailAddress;
                                    saveuser.GenderName = userViewModel.Gender;
                                    saveuser.Experience = userViewModel.Experience;
                                    saveuser.Marital = userViewModel.Marital ?? "";

                                    //saveuser.Skills = userViewModel.Tecnology == "Skills" ? "" : userViewModel.Tecnology;
                                    //added on 22/09
                                    saveuser.Skills = db.UserSkills.FirstOrDefault(o => o.UserID == model.UserId).Skills;

                                    //added on 19/09
                                    saveuser.Skills = db.UserSkills.FirstOrDefault(o => o.UserID == model.UserId).Skills;
                                    //saveuser.Skills = userViewModel.Tecnology == "Skills" ? "" : userViewModel.Tecnology;


                                    //  saveuser.BranchName = db.Master_Branches.FirstOrDefault(x => x.BranchID == userViewModel.BranchID).BranchName;
                                    //  saveuser.WorkPlace = userViewModel.WorkPlaceName;
                                    //saveuser.WorkPlace = userViewModel.WorkPlace;
                                    // saveuser.WorkPlace = db.WorkPlaces.FirstOrDefault(o => o.WorkPlaceID == WorkplaceId).WorkPlaceName; 
                                    if (userViewModel.WorkPlace != null)
                                    userViewModel.WorkplaceId = Convert.ToInt32(userViewModel.WorkPlace);

                                    //userViewModel.Marital = Convert.ToString(userViewModel.Marital);
                                    saveuser.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == userViewModel.WorkplaceId).WorkPlaceName;
                                    saveuser.FatherName = model.FatherName;
                                    saveuser.MotherName = model.MotherName;
                                    saveuser.BloodGroup = model.BloodGroup;
                                    saveuser.NationalityID = model.NationalityID;
                                    saveuser.NoOfChild = model.NoOfChild;
                                    saveuser.AnniversaryDate = model.AnniversaryDate;
                                    saveuser.ReligiousID = model.ReligiousID;
                                    saveuser.RelationShipID = model.RelationShipID;
                                    saveuser.RollID = model.RollID;

                                }
                                else
                                {
                                    saveuser.EmpID = "";
                                    saveuser.DepartmentName = "";
                                    saveuser.DesignationName = "";
                                    saveuser.FirstName = "";
                                    saveuser.MiddleName = "";
                                    saveuser.LastName = "";
                                    saveuser.DateOfBirth = "";
                                    saveuser.DateOfJoin = "";
                                    saveuser.ContactNo = 0;
                                    saveuser.EmailAddress = "";
                                    saveuser.GenderName = "";
                                    saveuser.Experience = "";
                                    saveuser.Marital = "";
                                    saveuser.Skills = "";
                                    saveuser.WorkPlace = "";
                                    saveuser.FatherName = "";
                                    saveuser.MotherName = "";
                                    saveuser.BloodGroup = "";
                                    saveuser.NationalityID = null;
                                    saveuser.NoOfChild = null;
                                    saveuser.AnniversaryDate = null;
                                    saveuser.ReligiousID = null;
                                    saveuser.RelationShipID = null;
                                    saveuser.RollID = null;
                                }
                            }

                            string MailBody = "";
                            var UserUpdateModel = updateuser;
                            var UserSavedModel = saveuser;
                            List<Variance> diff = extentions.DetailedCompare(UserUpdateModel, UserSavedModel);

                            int i;

                            if (diff.Count > 0)
                            {
                                MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Old Values</p> <br />";

                                for (i = 0; i < diff.Count; i++)
                                {
                                    if (diff[i].UserUpdateValue != null)
                                    {
                                        if (diff[i].UserSaveValue == null)
                                            diff[i].UserSaveValue = "";
                                        //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p>" + System.Environment.NewLine;

                                        MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff[i].FieldName == "ContactNo" ? "MobileNumber" : diff[i].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserSaveValue + @"</label></p><br/>" + System.Environment.NewLine;
                                    }

                                }

                                MailBody += "<br /><p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Updated Values</p>  <br />";

                                for (i = 0; i < diff.Count; i++)
                                {
                                    if (diff[i].UserUpdateValue != null)
                                    {
                                        //MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + diff[i].FieldName + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p>" + System.Environment.NewLine;
                                        MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + (diff[i].FieldName == "ContactNo" ? "MobileNumber" : diff[i].FieldName) + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + diff[i].UserUpdateValue.ToString() + @"</label></p><br/>" + System.Environment.NewLine;
                                    }
                                }
                            }


                            if (/*model.IsUnderNoticePeriod == false && model.IsActive != false &&*/ diff.Count > 0)
                            {

                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Update").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Update").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objEmpDetailsUpdate = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Update")
                                                               join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                               select new DSRCManagementSystem.Models.Email
                                                               {
                                                                   To = p.To,
                                                                   CC = p.CC,
                                                                   BCC = p.BCC,
                                                                   Subject = p.Subject,
                                                                   Template = q.TemplatePath
                                                               }).FirstOrDefault();
                                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                    string TemplatePathEmpDetailUpdate = Server.MapPath(objEmpDetailsUpdate.Template);
                                    string htmlEmpDetailUpdate = System.IO.File.ReadAllText(TemplatePathEmpDetailUpdate);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmpId", Convert.ToString(obj.EmpID));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#CompanyName", company);
                                    //  htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#FathersName", obj.FatherName);
                                    objEmpDetailsUpdate.To = db.Users.FirstOrDefault(o => o.UserID == userID).EmailAddress;
                                    objEmpDetailsUpdate.CC = UsersController.GetUserEmailAddress(db, objEmpDetailsUpdate.CC);
                                    if (objEmpDetailsUpdate.BCC != null)
                                    {
                                        objEmpDetailsUpdate.BCC = UsersController.GetUserEmailAddress(db, objEmpDetailsUpdate.BCC);
                                    }

                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                    string EmailAddress1 = "";

                                    foreach (string mails in MailIds1)
                                    {
                                        EmailAddress1 += mails + ",";
                                    }
                                    EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);
                                    var logo = CommonLogic.getLogoPath();

                                    if (ServerName != "http://win2012srv:88/")
                                    {

                                        string CCMailId = "aruna.m@dsrc.co.in";
                                        string BCCMailId = "Kirankumar@dsrc.co.in ";


                                        var path = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();

                                        Session["LogoPath"] = path.AppValue;
                                        Task.Factory.StartNew(() =>
                                        {
                                            //  var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                        });

                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //  DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", EmailAddress1, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", EmailAddress1, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.ToString()));

                                        });
                                    }
                                }
                                else
                                {
                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Employee Details Update", folder, ServerName);
                                }
                            }

                            if (model.IsUnderNoticePeriod == true && userViewModel.IsUnderNoticePeriod == true && model.IsActive != false && diff.Count > 0)
                            {
                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Update").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Update").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var updatedetailsofundernoticeperiod = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Update")
                                                                            join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                            select new DSRCManagementSystem.Models.Email
                                                                            {
                                                                                To = p.To,
                                                                                CC = p.CC,
                                                                                BCC = p.BCC,
                                                                                Subject = p.Subject,
                                                                                Template = q.TemplatePath
                                                                            }).FirstOrDefault();

                                    string TemplatePathEmpDetailUpdate = Server.MapPath(updatedetailsofundernoticeperiod.Template);
                                    string htmlEmpDetailUpdate = System.IO.File.ReadAllText(TemplatePathEmpDetailUpdate);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmpId", Convert.ToString(obj.EmpID));
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();

                                    updatedetailsofundernoticeperiod.To = db.Users.FirstOrDefault(o => o.UserID == userID).EmailAddress;
                                    updatedetailsofundernoticeperiod.CC = UsersController.GetUserEmailAddress(db, updatedetailsofundernoticeperiod.CC);

                                    htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#CompanyName", company);

                                    if (updatedetailsofundernoticeperiod.BCC != null)
                                    {
                                        updatedetailsofundernoticeperiod.BCC = UsersController.GetUserEmailAddress(db, updatedetailsofundernoticeperiod.BCC);
                                    }

                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                    string EmailAddress1 = "";

                                    foreach (string mails in MailIds1)
                                    {
                                        EmailAddress1 += mails + ",";
                                    }
                                    EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);

                                    var logo = CommonLogic.getLogoPath();

                                    if (ServerName != "http://win2012srv:88/")
                                    {

                                        string CCMailId = "aruna.m@dsrc.co.in";
                                        string BCCMailId = "Kirankumar@dsrc.co.in ";


                                        Task.Factory.StartNew(() =>
                                        {
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            //  DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                        });

                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", EmailAddress1, updatedetailsofundernoticeperiod.CC, updatedetailsofundernoticeperiod.BCC, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, updatedetailsofundernoticeperiod.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", EmailAddress1, updatedetailsofundernoticeperiod.CC, updatedetailsofundernoticeperiod.BCC, Server.MapPath(logo.ToString()));
                                        });
                                    }
                                }
                                else
                                {
                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Employee Details Update", folder, ServerName);
                                }
                            }

                            else if (model.IsUnderNoticePeriod == true && userViewModel.IsUnderNoticePeriod == false && model.IsActive != false)
                            {
                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Resigned Employee Details").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Resigned Employee Details").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objResignedEmpDetails = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Resigned Employee Details")
                                                                 join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                                 select new DSRCManagementSystem.Models.Email
                                                                 {
                                                                     To = p.To,
                                                                     CC = p.CC,
                                                                     BCC = p.BCC,
                                                                     Subject = p.Subject,
                                                                     Template = q.TemplatePath
                                                                 }).FirstOrDefault();

                                    var projectname = (from u in db.UserProjects join p in db.Projects on u.ProjectID equals p.ProjectID where u.UserID == obj.UserID select new { p.ProjectName }).FirstOrDefault();
                                    string ProjectName = projectname == null ? "N/A" : projectname.ProjectName;

                                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                    string TemplatePathResignedEmpDetails = Server.MapPath(objResignedEmpDetails.Template);
                                    string htmlResignedEmpDetails = System.IO.File.ReadAllText(TemplatePathResignedEmpDetails);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#EmployeeID", obj.EmpID);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#EmployeeName", obj.FirstName + " " + obj.LastName);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#Department", obj.Department.DepartmentName);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ResignedOn", obj.ResignedOn.Value.ToString("dd MMM yyyy"));
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#LastWorkingDate", obj.LastWorkingDate.Value.ToString("dd MMM yyyy"));
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ProjectName", ProjectName);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#ServerName", ServerName);
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlResignedEmpDetails = htmlResignedEmpDetails.Replace("#CompanyName", company);
                                    objResignedEmpDetails.To = UsersController.GetUserEmailAddress(db, objResignedEmpDetails.To);
                                    objResignedEmpDetails.CC = UsersController.GetUserEmailAddress(db, objResignedEmpDetails.CC);
                                    if (objResignedEmpDetails.BCC != "")
                                    {
                                        objResignedEmpDetails.BCC = UsersController.GetUserEmailAddress(db, objResignedEmpDetails.BCC);
                                    }

                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                    string EmailAddress1 = "";

                                    foreach (string mails in MailIds1)
                                    {
                                        EmailAddress1 += mails + ",";
                                    }

                                    EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);
                                    var logo = CommonLogic.getLogoPath();
                                    if (ServerName != "http://win2012srv:88/")
                                    {

                                        string CCMailId = "aruna.m@dsrc.co.in";
                                        string BCCMailId = "Kirankumar@dsrc.co.in ";

                                        Task.Factory.StartNew(() =>
                                        {
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject + " - Test Mail Please Ignore", null, htmlResignedEmpDetails + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject + " - Test Mail Please Ignore", null, htmlResignedEmpDetails + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                        });

                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject, "", htmlResignedEmpDetails, "HRMS@dsrc.co.in", EmailAddress1, objResignedEmpDetails.CC, objResignedEmpDetails.BCC, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objResignedEmpDetails.Subject, "", htmlResignedEmpDetails, "HRMS@dsrc.co.in", EmailAddress1, objResignedEmpDetails.CC, objResignedEmpDetails.BCC, Server.MapPath(logo.ToString()));

                                        });
                                    }
                                }
                                else
                                {
                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Resigned Employee Details", folder, ServerName);
                                }
                            }
                            if (model.IsActive == false)
                            {
                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Employee Details Deactivate").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Employee Details Deactivate").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objEmpDeactivated = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Deactivate")
                                                             join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                             select new DSRCManagementSystem.Models.Email
                                                             {
                                                                 To = p.To,
                                                                 CC = p.CC,
                                                                 BCC = p.BCC,
                                                                 Subject = p.Subject,
                                                                 Template = q.TemplatePath
                                                             }).FirstOrDefault();
                                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                    string TemplatePathEmpDeactivated = Server.MapPath(objEmpDeactivated.Template);
                                    string htmlEmpDeactivated = System.IO.File.ReadAllText(TemplatePathEmpDeactivated);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#EmployeeName", obj.FirstName + "  " + obj.LastName);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Department", obj.Department.DepartmentName);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#EmailAddress", obj.EmailAddress);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#JoiningDate", obj.DateOfJoin.Value.ToString("dd MMM yyyy"));
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Experience", obj.Experience);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#ServerName", ServerName);
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                                    htmlEmpDeactivated = htmlEmpDeactivated.Replace("#CompanyName", company);
                                    objEmpDeactivated.To = UsersController.GetUserEmailAddress(db, objEmpDeactivated.To);
                                    objEmpDeactivated.CC = UsersController.GetUserEmailAddress(db, objEmpDeactivated.CC);
                                    if (objEmpDeactivated.BCC != "")
                                    {
                                        objEmpDeactivated.BCC = UsersController.GetUserEmailAddress(db, objEmpDeactivated.BCC);
                                    }

                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    List<string> MailIds1 = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                    string EmailAddress1 = "";

                                    foreach (string mails in MailIds1)
                                    {
                                        EmailAddress1 += mails + ",";
                                    }
                                    EmailAddress1 = EmailAddress1.Remove(EmailAddress1.Length - 1);
                                    var logo = CommonLogic.getLogoPath();

                                    if (ServerName != "http://win2012srv:88/")
                                    {


                                        string CCMailId = "aruna.m@dsrc.co.in";
                                        string BCCMailId = "Kirankumar@dsrc.co.in ";

                                        Task.Factory.StartNew(() =>
                                        {
                                            // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            // DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject + " - Test Mail Please Ignore", null, htmlEmpDeactivated + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject + " - Test Mail Please Ignore", null, htmlEmpDeactivated + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress1, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                                        });

                                    }
                                    else
                                    {


                                        Task.Factory.StartNew(() =>
                                        {
                                            //var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                            DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject, "", htmlEmpDeactivated, "HRMS@dsrc.co.in", EmailAddress1, objEmpDeactivated.CC, objEmpDeactivated.BCC, Server.MapPath(logo.ToString()));
                                            //DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDeactivated.Subject, "", htmlEmpDeactivated, "HRMS@dsrc.co.in", EmailAddress1, objEmpDeactivated.CC, objEmpDeactivated.BCC, Server.MapPath(logo.AppValue.ToString()));
                                        });
                                    }
                                }
                                else
                                {
                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Employee Details Deactivate", folder, ServerName);
                                }
                            }




                            for (int y = 0; y < objuser.Count(); y++)
                            {
                                TempData["Null"] = "0";
                                var Value = Convert.ToInt32(objuser[y]);
                                var id = userID;
                                var alreadyvalue = db.UserReportings.Where(x => x.UserID == userID).Select(o => o).ToList();

                                if (alreadyvalue.Count() > objuser.Count())
                                {
                                    if (objuser.Count() == 1)
                                    {

                                        var vps = db.UserReportings.Where(x => x.UserID == userID).Select(o => o).ToList();
                                        foreach (var vp in vps)
                                            db.UserReportings.DeleteObject(vp);
                                        db.SaveChanges();

                                        DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                        objc.UserID = userID;
                                        objc.ReportingUserID = Value;
                                        db.AddToUserReportings(objc);
                                        db.SaveChanges();
                                        TempData["message"] = "Edited";
                                        return Json(true, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (alreadyvalue.Count() > objuser.Count())
                                {
                                    if (y == 0)
                                    {
                                        var vps = db.UserReportings.Where(x => x.UserID == userID).Select(o => o).ToList();
                                        foreach (var vp in vps)
                                            db.UserReportings.DeleteObject(vp);
                                        db.SaveChanges();
                                        TempData["Null"] = "Deleted";
                                    }


                                }

                                if (alreadyvalue.Count == 0)
                                {
                                    for (int d = 0; d < objuser.Count(); d++)
                                    {
                                        DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                        objc.UserID = userID;
                                        objc.ReportingUserID = objuser[d];
                                        db.AddToUserReportings(objc);

                                    }
                                    db.SaveChanges();
                                    TempData["message"] = "Edited";
                                    return Json(true, JsonRequestBehavior.AllowGet);
                                }


                                if (TempData["Null"].ToString() == "0")
                                {
                                    if (TempData["Null"].ToString() != "Deleted")
                                    {
                                        if (y < alreadyvalue.Count())
                                        {
                                            alreadyvalue[y].ReportingUserID = Value;
                                        }
                                        else
                                        {
                                            DSRCManagementSystem.UserReporting objc = new DSRCManagementSystem.UserReporting();
                                            objc.UserID = userID;
                                            objc.ReportingUserID = Value;
                                            db.AddToUserReportings(objc);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            TempData["message"] = "Edited";
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        return Json("EmailAddressExisting", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {

                    return Json("EmpIDExisting", JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpGet]
        public ActionResult ViewUser(int Id, string SearchString)
        {
            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();
            ViewBag.Lbl_Group = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_department = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            DSRCManagementSystem.Models.UserModel obj_Users = new DSRCManagementSystem.Models.UserModel();
            try
            {
                var DepartmentList = (from us in db.Departments

                                      select new
                                      {
                                          DepartmentId = us.DepartmentId,
                                          DepartmentName = us.DepartmentName
                                      }).ToList();
                var RoleNameList = (from


                                    r in db.Master_Designation


                                    select new
                                    {
                                        DesignationID = r.DesignationID,
                                        DesignationName = r.DesignationName
                                    }).ToList();

                var Isblock = db.Users.Where(x => x.UserID == Id && x.Attempts > 5).Select(x => x.UserID).FirstOrDefault();


                var Roles = (from role in db.Master_Roles
                             select new
                             {
                                 RollName = role.RoleName,
                                 RollID = role.RoleName
                             }).ToList();


                var userViewModel = (from u in db.Users
                                     join d in db.Departments on u.DepartmentId equals d.DepartmentId into DeptID
                                     join de in db.Master_Designation on u.DesignationID equals de.DesignationID
                                     join g in db.Master_Gender on u.Gender equals g.GenderID
                                     join b in db.Master_Branches on u.BranchId equals b.BranchID
                                     join n in db.Master_Nationality on u.NationalityID equals n.NationalityID into country
                                     from National in country.DefaultIfEmpty()
                                     join r in db.Master_Relationship on u.RelationshipID equals r.RelationshipID into relation
                                     from Relation in relation.DefaultIfEmpty()
                                     join persentAddress in db.PresentAddresses on u.TemporaryAddressID equals persentAddress.PresentAddressID into Temp
                                     from Tem in Temp.DefaultIfEmpty()
                                     join Permanent in db.PermanentAddresses on u.PermanentAddressID equals Permanent.PermanentAddressID into Terma
                                     from Perm in Terma.DefaultIfEmpty()
                                     join s in db.Master_Religious on u.ReligionID equals s.ReligiousID into Religion
                                     from Rel in Religion.DefaultIfEmpty()
                                     join m in db.Master_MaritalStatus on u.MaritalStatus equals m.MaritalStatusID into status
                                     from v in status.DefaultIfEmpty()
                                     join us in db.UserSkills on u.UserID equals us.UserID into value
                                     from user in value.DefaultIfEmpty()
                                     from dn in DeptID.DefaultIfEmpty()
                                     join Roll in db.Master_Roles on u.RoleID equals Roll.RoleID into Rolls
                                     from rol in Rolls.DefaultIfEmpty()

                                    
                                     where u.UserID == Id
                                     select new UserModel
                                     {
                                         UserId = u.UserID,
                                         EmpID = u.EmpID,
                                         DepartmentName = dn.DepartmentName,
                                         DepartmentId = u.DepartmentId,
                                         DesignationID = de.DesignationID,
                                         DesignationName = de.DesignationName,
                                         FirstName = u.FirstName,
                                         MiddleName = u.MiddleName,
                                         LastName = u.LastName,
                                         UserName = u.UserName,
                                         Gender = g.GenderName,
                                         GenderID = u.Gender,
                                         Password = u.Password,
                                         DateOfBirth = EntityFunctions.TruncateTime(u.DateOfBirth),
                                         DateOfJoin = EntityFunctions.TruncateTime(u.DateOfJoin),
                                         ContactNo = u.ContactNo,
                                         EmailAddress = u.EmailAddress,
                                         ResignedOn = u.ResignedOn,
                                         RollID=u.RoleID,
                                         RollName=rol.RoleName,
                                         
                                         LastworkingDate = u.LastWorkingDate,
                                         IsUnderProbation = u.IsUnderProbation ?? false,
                                         onboarding = u.IsBoarding ?? false,
                                         IsUnderNoticePeriod = u.IsUnderNoticePeriod ?? false,
                                         IsActive = u.IsActive ?? false,
                                         Block = Isblock > 0 ? true : false,
                                         IsBoarding = u.IsBoarding ?? false,
                                         Experience = u.Experience,
                                         ID = user.ID,
                                         Tecnology = user.Skills,
                                         WorkPlace = u.Workplace,
                                         Marital = v.MaritalStatusType,
                                         MaritalStatusId = (int)u.MaritalStatus,
                                         BranchID = (int)u.BranchId,
                                         PermanentAddress = u.PermanentAddressID,
                                         IPAddress = u.IPAddress,
                                         BranchName = b.BranchName,
                                         RegionId = u.Region,
                                         GroupId = u.DepartmentGroup,
                                         MachineName = u.MachineName,

                                         OfficeNo = u.officeno,
                                         OfficeSkypeId = u.OfficeSkypeId,
                                         Extension = u.extension,
                                         SelectedUserStatusid = u.UserStatus,
                                         Religious = Rel.ReligiousName,
                                         ReligiousID = Rel.ReligiousID,
                                         RelationShipID = Relation.RelationshipID,
                                         RelationShipName = Relation.RelationshipName,
                                         Nationality = National.NationalityName,
                                         NationalityID = National.NationalityID,
                                         BloodGroup = u.BloodGroup,
                                         FatherName = u.FatherName,
                                         MotherName = u.MotherName,
                                         BirthPlace = u.BirthPlace,
                                         SpouseName = u.SpouseName,
                                         NoOfChild = u.NoOfChild,
                                         AnniversaryDate = u.AnniversaryDate,
                                         EmergencyContactName = u.Contactperson,
                                         Emergency = u.EmergencyContact,
                                         PermanentAddress1 = Perm.Address_1,
                                         PermanentAddress3 = Perm.Address_3,
                                         PermanentAddress2 = Perm.Address_2,
                                         PermanentCity = Perm.City,
                                         Permanentstate = Perm.State,
                                         PermanentCountryID = Perm.CountryID,
                                         PermanentPinCode = Perm.Zip,
                                         PresentAddress1 = Tem.Address_1,
                                         PresentAddress2 = Tem.Address_2,
                                         PresentAddress3 = Tem.Address_3,
                                         PresentCity = Tem.City,
                                         Presentstate = Tem.State,
                                         PresentCountryID = Tem.CountryID,
                                         PresentPinCode = Tem.Zip,


                                     }).FirstOrDefault();



                ViewBag.EmpID = userViewModel.EmpID;

                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();
                var BranchesList = (from b in db.Master_Branches
                                    select new
                                    {
                                        BranchID = b.BranchID,
                                        BranchName = b.BranchName
                                    }).ToList();
                var GroupList = (from dg in db.DepartmentGroups.Where(dg => dg.IsActive == true)
                                 join dgm in db.DepartmentGroupMappings.Where(dgm => dgm.DepartmentID == userViewModel.DepartmentId) on dg.GroupID equals dgm.GroupID
                                 select new
                                 {
                                     groupid = dg.GroupID,
                                     groupname = dg.GroupName
                                 }).ToList();
                var DesignationList = (from
                                    r in db.Master_Designation
                                       select new
                                       {
                                           DesignationID = r.DesignationID,
                                           DesignationName = r.DesignationName
                                       }).ToList();
                var workplace = (from p in db.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     WorkplaceName = p.WorkPlaceName

                                 }).ToList();

                var MaritalStautus = (from p in db.Master_MaritalStatus
                                      select new
                                      {
                                          MaritalStatusId = p.MaritalStatusID,
                                          Value = p.MaritalStatusType
                                      }).ToList();


                var TechList = (from
                                tl in db.Master_Technologies
                                select new
                                {
                                    ID = tl.ID,
                                    Tecnology = tl.Tecnology
                                }).ToList();

                var RelationShip = (from r in db.Master_Relationship
                                    select new
                                    {
                                        RelationShipID = r.RelationshipID,
                                        RelationShipName = r.RelationshipName
                                    }).ToList();

                var Religious = (from r in db.Master_Religious
                                 select new
                                 {
                                     ReligiousID = r.ReligiousID,
                                     ReligiousName = r.ReligiousName
                                 }).ToList();

                var Nationality = (from r in db.Master_Nationality
                                   select new
                                   {
                                       NationalityID = r.NationalityID,
                                       NationalityName = r.NationalityName
                                   }).ToList();
                var BloogGroup = (from b in db.Master_BloodGroup
                                  select new
                                  {
                                      BloogGroup = b.BloodGroupName,
                                      BloodGroupID = b.BloodGroupID
                                  }).ToList();

                var Country = (from c in db.Master_Country
                               select new
                               {
                                   CountryID = c.CountryID,
                                   CountryName = c.CountryName


                               }).ToList();

                var state = (from s in db.Master_States
                             select new
                             {
                                 StateID = s.StateID,
                                 StateName = s.States
                             }).ToList();





                var city = (from c in db.Master_City
                            select new

                            {
                                CityID = c.CityID,
                                CityName = c.CityName

                            }).ToList();


                var Zone = (from p in db.TimeZones
                            select new
                            {
                                RegionId = p.Id,
                                Region = p.Zone
                            }).ToList();

                List<int> ChildCount = new List<int>();
                for (int i = 1; i < 10; i++)
                {
                    ChildCount.Add(i);
                }
                ViewBag.Child = new SelectList(ChildCount, "", "");






                var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                  join q in db.UserRoles on p.RoleID equals q.RoleID
                                  select q.UserID).ToList();
                var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                foreach (var id in userbyid)
                {
                    userbyrole.Add(id);
                }
                var fullusers = userbyrole.Distinct().ToList();

                List<object> EmployeeList = new List<object>();
                int Branch = userViewModel.BranchID;
                foreach (var userid in fullusers)
                {
                    var name =
                        db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == Branch)
                            .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                            .FirstOrDefault();
                    var Val = new { ID1 = userid, UserName1 = name };
                    EmployeeList.Add(Val);
                }



                userViewModel.DateOfJoin = userViewModel.DateOfJoin == null ? userViewModel.DateOfJoin : userViewModel.DateOfJoin.Value.Date;
                var selected = from d in db.Departments where d.DepartmentId == userViewModel.DepartmentId select new { DepartmentId = d.DepartmentId };

                ViewBag.DepartmentIdList = new SelectList(db.Departments.OrderBy(d => d.DepartmentName).Where(d => d.DepartmentId == userViewModel.DepartmentId), "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                // var temp = new SelectList(DepartmentList, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);

                //ViewBag.DepartmentIdList = temp;
                ViewBag.RoleIdList = new SelectList(RoleNameList, "DesignationID", "DesignationName", userViewModel.DesignationID);

                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");

                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();

                List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();

                objuser = (from p in db.UserReportings.Where(x => x.UserID == Id)
                           select new DSRCManagementSystem.Models.ListReporting
                           {
                               Id = p.ReportingUserID

                           }).ToList();

                List<int> selectedAttendees = new List<int>();


                for (int i = 0; i < objuser.Count(); i++)
                {
                    selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                }
                string[] experience = null;

                if ((userViewModel.Experience != null) && (userViewModel.Experience != "0"))
                {
                    experience = userViewModel.Experience.Split('.');
                }
                else
                {
                    experience = "0.0".Split('.');
                }
                userViewModel.SearchString = SearchString;
                var objdepartment = (from p in db.Departments

                                     select new
                                     {
                                         DepartmentId = p.DepartmentId,
                                         DepartmentName = p.DepartmentName,

                                     }).OrderBy(x => x.DepartmentName).ToList();
                ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);


                //int presentcity = Convert.ToInt32(userViewModel.PresentCity);
                //int presentSate = Convert.ToInt32(userViewModel.Presentstate);
                //int presentCountry = Convert.ToInt32(userViewModel.PresentCountryID);
                int presentcity, presentState, presentCountry = 0;
                if (userViewModel.PresentCity != null && userViewModel.PresentCity != "")
                    presentcity = Convert.ToInt32(userViewModel.PresentCity);
                else
                    presentcity = 0;

                if (userViewModel.Presentstate != null && userViewModel.Presentstate != "")
                    presentState = Convert.ToInt32(userViewModel.Presentstate);
                else
                    presentState = 0;

                if (userViewModel.PresentCountryID != null)// || userViewModel.PresentCountryID != "")
                    presentCountry = Convert.ToInt32(userViewModel.PresentCountryID);
                else
                    presentCountry = 0;
                var PresentcountryID = db.Master_Country.Where(x => x.CountryID == presentCountry).Select(o => o.CountryID).FirstOrDefault();
                var PresentcityID = db.Master_City.Where(x => x.CityID == presentcity).Select(o => o.CityID).FirstOrDefault();
                var PresentStateID = db.Master_States.Where(x => x.StateID == presentState).Select(o => o.StateID).FirstOrDefault();

                //int permanentCity = Convert.ToInt32(userViewModel.PermanentCity);
                //int permanentSate = Convert.ToInt32(userViewModel.Permanentstate);
                //int permanentCountry = Convert.ToInt32(userViewModel.PermanentCountryID);

                int permanentCity, permanentSate, permanentCountry = 0;

                if (userViewModel.PermanentCity != null && userViewModel.PermanentCity != "")
                    permanentCity = Convert.ToInt32(userViewModel.PermanentCity);
                else
                    permanentCity = 0;

                if (userViewModel.Permanentstate != null && userViewModel.Permanentstate != "")
                    permanentSate = Convert.ToInt32(userViewModel.Permanentstate);
                else
                    permanentSate = 0;

                if (userViewModel.PermanentCountryID != null)// || userViewModel.PermanentCountryID != "")
                    permanentCountry = Convert.ToInt32(userViewModel.PermanentCountryID);
                else
                    permanentCountry = 0;

                var PermanentcountryID = db.Master_Country.Where(x => x.CountryID == permanentCountry).Select(o => o.CountryID).FirstOrDefault();
                var PermanentcityID = db.Master_City.Where(x => x.CityID == permanentCity).Select(o => o.CityID).FirstOrDefault();
                var PermanentStateID = db.Master_States.Where(x => x.StateID == permanentSate).Select(o => o.StateID).FirstOrDefault();




                int workPlaceID = Convert.ToInt32(userViewModel.WorkPlace);
                ViewBag.Roles = new SelectList(Roles, "RollID ", "RollName", userViewModel.RollID);
                userViewModel.WorkplaceId = workPlaceID;
                ViewBag.PermanentCountry = new SelectList(Country, "CountryID", "CountryName", permanentCountry);
                ViewBag.BloogGroup = new SelectList(BloogGroup, "BloodGroupID", "BloogGroup", userViewModel.BloodGroup);
                ViewBag.Permanentstate = new SelectList(state, "StateID", "StateName", PermanentStateID);
                ViewBag.Permanentcity = new SelectList(city, "CityID", "CityName", PermanentcityID);
                ViewBag.PresentCountry = new SelectList(Country, "CountryID", "CountryName", presentCountry);
                ViewBag.Presentstate = new SelectList(state, "StateID", "StateName", PresentStateID);
                ViewBag.Presentcity = new SelectList(city, "CityID", "CityName", PresentcityID);
                //int WorkPlace = db.Master_WorkPlace.Where(x => x.WorkPlaceName == userViewModel.WorkPlace).Select(o => o.WorkPlaceID).FirstOrDefault();
                //userViewModel.WorkPlace = db.Master_WorkPlace.FirstOrDefault(o => o.WorkPlaceID == workPlaceID).WorkPlaceName;
                userViewModel.WorkPlace = db.Master_WorkPlace.Where(o => o.WorkPlaceID == workPlaceID).Select(o => o.WorkPlaceName).FirstOrDefault();
                ViewBag.YearsList = new SelectList(YearsList(), "YearId", "Year", int.Parse(experience[0]));
                ViewBag.MonthList = new SelectList(MonthsList(), "MonthId", "Month", int.Parse(experience[1]));
                ViewBag.Tech = new MultiSelectList(TechList, "ID", "Tecnology", userViewModel.Tecnology);
                ViewBag.BranchList = new SelectList(BranchesList, "BranchID", "BranchName", userViewModel.BranchID);
                //ViewBag.DepartmentNameList = new SelectList(db.Departments.OrderBy(d => d.DepartmentName).Where(d => d.DepartmentId == userViewModel.DepartmentId), "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                ViewBag.Department = new SelectList(objdepartment, "DepartmentId", "DepartmentName", userViewModel.DepartmentId);
                ViewBag.WorkPlaceList = new SelectList(workplace, "WorkplaceId", "WorkplaceName", userViewModel.WorkplaceId);

                ViewBag.Marital = new SelectList(MaritalStautus, "MaritalStatusId", "Value", userViewModel.Marital);
                ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", userViewModel.DesignationID);
                ViewBag.Region = new SelectList(Zone, "RegionId", "Region", userViewModel.RegionId);
                ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", userViewModel.GroupId);
                ViewBag.Status = new SelectList(UserStatus, "userstatusid", "userstatusname", userViewModel.SelectedUserStatusid);
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName", userViewModel.GenderID);
                ViewBag.Religious = new SelectList(Religious, "ReligiousID", "ReligiousName", userViewModel.ReligiousID);
                ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName", userViewModel.RelationShipID);
                ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "NationalityName", userViewModel.NationalityID);
                return View(userViewModel);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpGet]
        public ActionResult ResetPassword(int UserID, string EmailAddress)
        {
            ResetPassword model = new ResetPassword();
            try
            {
                model.UserId = UserID;
                model.EmailAddress = EmailAddress;
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult ResetPassword(ResetPassword model)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                if (model.SendLink == true)
                {
                    int UID = model.UserId;
                    string MailAddress = model.EmailAddress;
                    int PasswordKey = Convert.ToInt32(GenerateRandomPasswordKey(5));
                    var UpdateKey = db.Users.FirstOrDefault(x => x.UserID == UID);

                    Guid id = Guid.NewGuid();

                    if (UID != 0)
                    {
                        var obj = db.Users.Where(o => o.UserID == UID).Select(o => o).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.PasswordKey = PasswordKey;
                            obj.Key = id.ToString();
                            db.SaveChanges();

                            if (MailAddress != null && MailAddress != "")
                            {
                                var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Reset Password").Select(o => o.EmailTemplateID).FirstOrDefault();
                                var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Reset Password").Select(x => x.TemplatePath).FirstOrDefault();
                                if ((check != null) && (check != 0))
                                {
                                    var objResetPwd = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Reset Password")
                                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                                       select new DSRCManagementSystem.Models.Email
                                                       {
                                                           To = p.To,
                                                           CC = p.CC,
                                                           BCC = p.BCC,
                                                           Subject = p.Subject,
                                                           Template = q.TemplatePath
                                                       }).FirstOrDefault();

                                    var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                                    string TemplatePathResetPwd = Server.MapPath(objResetPwd.Template);
                                    string htmlResetPwd = System.IO.File.ReadAllText(TemplatePathResetPwd);
                                    htmlResetPwd = htmlResetPwd.Replace("#UserName", obj.FirstName + " " + obj.LastName);
                                    htmlResetPwd = htmlResetPwd.Replace("#UserID", model.UserId.ToString());
                                    htmlResetPwd = htmlResetPwd.Replace("#Guiid", obj.Key);
                                    htmlResetPwd = htmlResetPwd.Replace("#Key", Convert.ToString(PasswordKey));
                                    htmlResetPwd = htmlResetPwd.Replace("#ServerName", ServerName);

                                    htmlResetPwd = htmlResetPwd.Replace("#CompanyName", company);
                                    //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                                    List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                                    string EmailAddres = "";

                                    foreach (string mail in MailIds)
                                    {
                                        EmailAddres += mail + ",";
                                    }
                                    EmailAddres = EmailAddres.Remove(EmailAddres.Length - 1);

                                    if (ServerName != "http://win2012srv:88/")
                                    {


                                        DsrcMailSystem.MailSender.SendMail(null, objResetPwd.Subject + " - Test Mail Please Ignore", null, htmlResetPwd + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddres, null);
                                    }
                                    else
                                    {
                                        DsrcMailSystem.MailSender.SendMail(null, objResetPwd.Subject, null, htmlResetPwd, "HRMS@dsrc.co.in", EmailAddres, Server.MapPath(Session["LoginLogo"].ToString()));
                                    }
                                }
                                else
                                {
                                    // string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                                    ExceptionHandlingController.TemplateMissing("Reset Password", folder, ServerName);
                                }
                                return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { Result = "Failer", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                if (model.ResetHere == true)
                {
                    string ChangedPassword = DSRCLogic.Hashing.Create_SHA256(model.Password);
                    var record = db.Users.FirstOrDefault(x => x.UserID == model.UserId);
                    if (record != null)
                    {
                        record.Password = ChangedPassword;
                        db.SaveChanges();
                    }
                    return Json(new { Result = "ResetHereSuccess", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        //[HttpGet]
        //public ActionResult DelUser() 
        //{


        //    var UserStatus12 = db.Master_UserStatus.Where(s => s.IsActive == true && (s.UserStatusId == 8 || s.UserStatusId == 9 || s.UserStatusId == 10)).Select(l => new
        //    {
        //        StatusId1 = l.UserStatusId,
        //        userstatusname = l.UserStatus
        //    }).ToList();
        //    ViewBag.Status1 = UserStatus12;
        //    return View();
        //}

        [HttpGet]
        public ActionResult DeleteUser(int Id = 0)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string ServerName = AppValue.GetFromMailAddress("ServerName");
            try
            {
                var DeleteUsers = db.Users.FirstOrDefault(x => x.UserID == Id);
                if (Id != 0)
                {

                    if (DeleteUsers != null)
                    {

                        DeleteUsers.IsActive = false;
                        db.SaveChanges();
                    }

                    var check = db.EmailTemplates.Where(x => x.TemplatePurpose == "Delete Employee").Select(o => o.EmailTemplateID).FirstOrDefault();
                    var folder = db.EmailTemplates.Where(o => o.TemplatePurpose == "Delete Employee").Select(x => x.TemplatePath).FirstOrDefault();
                    if ((check != null) && (check != 0))
                    {
                        var objDelete = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Delete Employee")
                                         join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                         select new DSRCManagementSystem.Models.Email
                                         {
                                             To = p.To,
                                             CC = p.CC,
                                             BCC = p.BCC,
                                             Subject = p.Subject,
                                             Template = q.TemplatePath
                                         }).FirstOrDefault();

                        var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
                        string TemplatePathDelete = Server.MapPath(objDelete.Template);
                        string htmlDelete = System.IO.File.ReadAllText(TemplatePathDelete);
                        htmlDelete = htmlDelete.Replace("#EmpID", DeleteUsers.EmpID);
                        htmlDelete = htmlDelete.Replace("#Name", DeleteUsers.FirstName + " " + DeleteUsers.LastName);
                        htmlDelete = htmlDelete.Replace("#Department", DeleteUsers.Department.DepartmentName);
                        htmlDelete = htmlDelete.Replace("#JoiningDate", ((DateTime)DeleteUsers.DateOfJoin).ToString("dd MMM yyyy"));
                        htmlDelete = htmlDelete.Replace("#Experience", DeleteUsers.Experience);
                        htmlDelete = htmlDelete.Replace("#ServerName", ServerName);
                        htmlDelete = htmlDelete.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
                        htmlDelete = htmlDelete.Replace("#CompanyName", company);
                        objDelete.To = UsersController.GetUserEmailAddress(db, objDelete.To);
                        objDelete.CC = UsersController.GetUserEmailAddress(db, objDelete.CC);
                        if (objDelete.BCC != "")
                        {
                            objDelete.BCC = UsersController.GetUserEmailAddress(db, objDelete.BCC);
                        }


                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];

                        List<string> MailIds = db.TestMailIDs.Select(o => o.MailAddress).ToList();
                        string EmailAddres = "";

                        foreach (string mail in MailIds)
                        {
                            EmailAddres += mail + ",";
                        }
                        EmailAddres = EmailAddres.Remove(EmailAddres.Length - 1);

                        var logo = CommonLogic.getLogoPath();
                        if (ServerName != "http://win2012srv:88/")
                        {


                            string CCMailId = "aruna.m@dsrc.co.in";
                            string BCCMailId = "Kirankumar@dsrc.co.in ";


                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                //DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject + " - Test Mail Please Ignore", null, htmlDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", "Gowtham.r@dsrc.co.in", CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject + " - Test Mail Please Ignore", null, htmlDelete + " - Testing Plaese ignore", "Test-HRMS@dsrc.co.in", EmailAddres, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                            });

                        }
                        else
                        {

                            Task.Factory.StartNew(() =>
                            {
                                // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                                // DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject, "", htmlDelete, "HRMS@dsrc.co.in", EmailAddres, objDelete.CC, objDelete.BCC, Server.MapPath(logo.AppValue.ToString()));
                                DsrcMailSystem.MailSender.SendMailToALL(null, objDelete.Subject, "", htmlDelete, "HRMS@dsrc.co.in", EmailAddres, objDelete.CC, objDelete.BCC, Server.MapPath(logo.ToString()));

                            });
                        }
                    }
                    else
                    {
                        //string ServerName = WebConfigurationManager.AppSettings["SeverName"];
                        ExceptionHandlingController.TemplateMissing("Delete Employee", folder, ServerName);
                    }
                    return Json(new { Result = "Success", URL = @Url.Action("AlertPopUp", "Popup") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpGet]
        public ActionResult NoticePeriod(int usersid, int statusid)
        {
            try
            {
                var user = (from u in db.Users.Where(u => u.IsActive == true && u.UserID == usersid)
                            select new UserModel()
                            {
                                UserId = usersid,
                                ResignedOn = u.ResignedOn,
                                LastworkingDate = u.LastWorkingDate,
                                SelectedUserStatusid = u.UserStatus
                            }).FirstOrDefault();
                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();
                ViewBag.Status = UserStatus;
                return View(user);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpPost]
        public ActionResult NoticePeriod(UserModel model)
        {
            try
            {
                
                var noticed = db.Users.Where(u => u.UserID == model.UserId).Select(u => u).FirstOrDefault();
                if (noticed.UserStatus == 2 && model.SelectedUserStatusid != 2)
                {
                    noticed.ResignedOn = null;
                    noticed.LastWorkingDate = null;
                }
                else
                {
                    noticed.ResignedOn = model.ResignedOn;
                    noticed.LastWorkingDate = model.LastworkingDate;
                }
                noticed.UserStatus = model.SelectedUserStatusid;
                if (noticed.LastWorkingDate < DateTime.Now.Date)
                {
                    var setStatus = db.Users.Where(c => c.UserID == model.UserId).FirstOrDefault();
                    setStatus.UserStatus = 6;
                }
                db.SaveChanges();
                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();

                ViewBag.Status = UserStatus;
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpGet]
        public ActionResult NotPerformingGood(int usersid, int statusid)
        {
            try
            {
                var user = (from u in db.Users.Where(u => u.IsActive == true && u.UserID == usersid)
                            select new UserModel()
                            {
                                UserId = usersid,
                                LastworkingDate = u.LastWorkingDate,
                                SelectedUserStatusid = u.UserStatus
                            }).FirstOrDefault();
                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();
                ViewBag.Status = UserStatus;
                return View(user);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpPost]
        public ActionResult NotPerformingGood(UserModel model)
        {
            try
            {

                var NotPerformingGood = db.Users.Where(u => u.UserID == model.UserId).Select(u => u).FirstOrDefault();
                if (NotPerformingGood.UserStatus == 11 && model.SelectedUserStatusid != 11)
                {
                    NotPerformingGood.ResignedOn = null;
                    NotPerformingGood.LastWorkingDate = null;
                }
                else
                {
                    NotPerformingGood.ResignedOn = model.ResignedOn;
                    NotPerformingGood.LastWorkingDate = model.LastworkingDate;
                }
                NotPerformingGood.UserStatus = model.SelectedUserStatusid;
                if (NotPerformingGood.LastWorkingDate < DateTime.Now.Date)
                {
                    var setStatus = db.Users.Where(c => c.UserID == model.UserId).FirstOrDefault();
                    setStatus.UserStatus = 6;
                }
                db.SaveChanges();
                var UserStatus = db.Master_UserStatus.Where(s => s.IsActive == true).Select(l => new
                {
                    userstatusid = l.UserStatusId,
                    userstatusname = l.UserStatus
                }).ToList();

                ViewBag.Status = UserStatus;
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [HttpPost]
        public ActionResult StatusChange(int statusid, int userid)
        {
            try
            {
                var checkuserstatus = db.Users.Where(u => u.UserID == userid).Select(u => u.UserStatus).FirstOrDefault();
                if (checkuserstatus == 2 && statusid != 2 )
                {
                    return Json("Alert", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var user = db.Users.Where(u => u.UserID == userid).Select(u => u).FirstOrDefault();
                    user.UserStatus = statusid;
                    if (statusid == 6) { user.IsActive = false; }
                    user.LastWorkingDate = statusid == 6 ? user.LastWorkingDate = DateTime.Now : null;
                    StatusChange_MailTrigger(statusid, userid);
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetDepartments(int BranchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
            try
            {
                if (BranchId != 0)
                {

                    List<int> validDepart = new List<int>();

                    validDepart = db.Departments.Where(d => d.BranchID == BranchId && d.IsActive == true).Select(d => d.DepartmentId).ToList();

                    FilterDepart = (from lt in db.Departments.Where(o => validDepart.Contains(o.DepartmentId))
                                    select new FilterDepartment()
                                    {
                                        DepartmentId = lt.DepartmentId,
                                        DepartmentName = lt.DepartmentName
                                    }).OrderBy(x => x.DepartmentName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.DepartmentId), Text = m.DepartmentName });
                }
                return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPersons(int BranchId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();
            List<object> EmployeeList = new List<object>();
            try
            {
                if (BranchId != 0)
                {
                    var userbyrole = (from p in db.ReportingUsers.Where(x => x.RoleID != null)
                                      join q in db.UserRoles on p.RoleID equals q.RoleID
                                      select q.UserID).ToList();
                    var userbyid = db.ReportingUsers.Where(x => x.UserId != null).Select(o => (int)o.UserId).ToList();
                    foreach (var id in userbyid)
                    {
                        userbyrole.Add(id);
                    }
                    var fullusers = userbyrole.Distinct().ToList();




                    foreach (var userid in fullusers)
                    {
                        var name =
                            db.Users.Where(x => x.UserID == userid && x.IsActive == true && x.UserStatus != 6 && x.BranchId == BranchId)
                                .Select(u => u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : ""))
                                .FirstOrDefault();
                        if (name != null)
                        {
                            var Val = new { ID1 = userid, UserName1 = name };

                            EmployeeList.Add(Val);
                        }

                    }

                }






                return Json(new SelectList(EmployeeList, "ID1", "UserName1"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetGroups(int DepartmentId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterGroup = new List<SelectListItem>();
            try
            {

                if (DepartmentId != 0)
                {
                    var validGroup = db.DepartmentGroupMappings.Where(d => d.DepartmentID == DepartmentId).Select(d => d.GroupID).ToList();

                    FilterGroup = (from lt in db.DepartmentGroups.Where(o => validGroup.Contains(o.GroupID))
                                   where lt.IsActive == true
                                   select new FilterGroup()
                                   {
                                       GroupId = lt.GroupID,
                                       GroupName = lt.GroupName
                                   }).OrderBy(x => x.GroupName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.GroupId), Text = m.GroupName });
                }
               
                return Json(new SelectList(FilterGroup, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }

            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        [HttpGet]
        public ActionResult UserValidation(int? quick,UserModel profilemodel)
        {

            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            try
            {
                int QuickEnrollid = Convert.ToInt32(quick);

                if (QuickEnrollid == 0)
                {


                    //try
                    //{
                    // var EmpID = db.Users.OrderByDescending(r => r.EmpID).Distinct().Skip(1).FirstOrDefault();
                    //var EmpID = db.Users.Select(x => x.EmpID).ToList();
                    //for (int i = 0; i < EmpID.Count; i++)
                    //{
                    //    EmpIDs.Add((EmpID[i]));
                    //}

                    //          var Empid = db.Users.OrderByDescending(p => p.EmpID)
                    //.Distinct(new EqualityComparer()).Skip(1).First();
                    //string Empid = db.Users.Select(x => x.EmpID.Max()).ToList();
                    //var EmpID = db.Users.GroupBy(e => e.EmpID).OrderByDescending(g => g.Key).Skip(1).First();
                    //  string Empid =(EmpID.Max());
                    if (db.Users.Any(R => R.EmpID == profilemodel.EmpID))
                    {
                        ModelState.AddModelError("EmpID", "EmpID  Already Exists");
                        return Json("Warning", JsonRequestBehavior.AllowGet);

                    }
                    else if (profilemodel.EmpID == null)
                    {
                        ModelState.AddModelError("EmpID", "EmpID");

                    }
                    else
                    {
                        //if (EmpID != null)
                        //{

                        //    if (Empid.Length == 1)
                        //    {
                        //        ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                        //    }
                        //    else if (Empid.Length == 2)
                        //    {
                        //        ViewBag.EmpID = "000" + (Convert.ToInt32(Empid) + 1);
                        //    }
                        //    else if (Empid.Length == 3)
                        //    {
                        //        ViewBag.EmpID = "00" + (Convert.ToInt32(Empid) + 1);
                        //    }
                        //    else if (Empid.Length == 4)
                        //    {
                        //        ViewBag.EmpID = "0" + (Convert.ToInt32(Empid) + 1);
                        //    }
                        //    else
                        //    {
                        //        string displayString = string.Empty;

                        //var temp=Empid.Split('/');
                        //string tempid=temp[2];
                        //int id = Convert.ToInt32(tempid);
                        //id = id + 1;
                        //string autoId = "DSRC" + String.Format("{0:0000}", id);

                        //ViewBag.EmpID = "" + (Convert.ToInt32(tempid) + 1);
                        ////if (string.IsNullOrEmpty(Empid))
                        ////{
                        ////{
                        ////    jid = "AM0000";//This string value has to increment at every time, but it is getting increment only one time.
                        ////}
                        //            int len = Empid.Length;
                        //            string split = Empid.Substring(4, len - 4);
                        //            int num = Convert.ToInt32(split);
                        //            num++;
                        //            displayString = Empid.Substring(0, 4) + num.ToString("0000");
                        //            ViewBag.EmpId = displayString;

                        //    }
                        //        }

                        //    //}
                        //    else
                        //    {
                        //        ViewBag.EmpID = "0000" + (Convert.ToInt32(Empid) + 1);
                        //    }
                        //}
                        //}
                        //catch (Exception ex)
                        //{
                        //    ViewBag.EmpID = "0000" + 1;
                        //}


                        if (profilemodel.EmpID != null)
                        {
                            //int EMPID = Convert.ToInt32(profilemodel.EmpID);
                           // profilemodel.EmpID = Convert.ToString(EMPID);
                            DSRCManagementSystem.Models.UserModel obj_UserModel = new DSRCManagementSystem.Models.UserModel();
                            //var a = Regex.Matches(profilemodel.EmpID, @"[a-zA-Z]").Count;
                            if (profilemodel.EmpID.Length <= 10 )
                            {
                                string EmployeeID = profilemodel.EmpID;
                                if (profilemodel.EmpID.Length == 1 )
                                {
                                    profilemodel.EmpID = profilemodel.EmpID;
                                    EmployeeID = profilemodel.EmpID;
                                }
                                if (profilemodel.EmpID.Length == 2 )
                                {
                                    profilemodel.EmpID = profilemodel.EmpID;
                                    EmployeeID = profilemodel.EmpID;
                                }
                                if (profilemodel.EmpID.Length == 3 )
                                {
                                    profilemodel.EmpID = profilemodel.EmpID;
                                    EmployeeID = profilemodel.EmpID;
                                }
                                if (profilemodel.EmpID.Length == 4)
                                {
                                    profilemodel.EmpID = profilemodel.EmpID;
                                    EmployeeID = profilemodel.EmpID;
                                }
                                //if (a != 0)
                                //{
                                //    profilemodel.EmpID = profilemodel.EmpID;
                                //    EmployeeID = profilemodel.EmpID;
                                //}
                                else
                                {
                                    profilemodel.EmpID = profilemodel.EmpID;
                                    EmployeeID = profilemodel.EmpID;
                                    ////string displayString = string.Empty;

                                }
                                    //int len = Empid.Length;
                                    //string split = Empid.Substring(4, len - 4);
                                    //int num = Convert.ToInt32(split);
                                    //num++;
                                    //displayString = Empid.Substring(0, 4) + num.ToString("0000");
                                    //ViewBag.EmpId = displayString;

                                }
                                var empidCheck = db.Users.FirstOrDefault(x => x.EmpID == profilemodel.EmpID && x.BranchId == profilemodel.BranchID);
                                var emailAddressCheck = db.Users.FirstOrDefault(x => x.EmailAddress == profilemodel.EmailAddress);
                                long contactno = Convert.ToInt64(profilemodel.ContactNo);
                                var checkContactNo = db.Users.FirstOrDefault(x => x.ContactNo == contactno);
                                var checkUsername = db.Users.FirstOrDefault(x => x.UserName == profilemodel.UserName);

                                if (empidCheck == null)
                                {
                                    if (checkUsername == null)
                                    {
                                        if (emailAddressCheck == null)
                                        {
                                            if (checkContactNo == null)
                                            {

                                                if (profilemodel.EmailAddress != null)
                                                {
                                                    bool isEmail = Regex.IsMatch(profilemodel.EmailAddress, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                             @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnoreCase);
                                                    if (isEmail/*profilemodel.EmailAddress.EndsWith("@dsrc.co.in") || profilemodel.EmailAddress.EndsWith("@dsrc-cid.in")*/)
                                                    {

                                                        return Json("Success", JsonRequestBehavior.AllowGet);

                                                    }
                                                    else
                                                    {
                                                        return Json("MailProcessingFailed", JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                                else
                                                {
                                                    return Json("EmailAddressNULL", JsonRequestBehavior.AllowGet);
                                                }
                                            }
                                            else
                                            {
                                                return Json("ContactNoExisting", JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            return Json("EmailAddressExisting", JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json("UsernameExisting", JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {
                                return Json("EmpIDCharc", JsonRequestBehavior.AllowGet);
                            }




                        }
                    }
                }
            
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        public ActionResult ClearTempSession()
        {
            return View();
        }

        #region MailTriggers

        public void StatusChange_MailTrigger(int statusid, int userid)
        {
            string ServerName = AppValue.GetFromMailAddress("ServerName");

            var objUser = db.Users.Where(u => u.UserID == userid).Select(u => u).FirstOrDefault();

            var objOldStatus = (from u in db.Users.Where(u => u.IsActive == true && u.UserID == userid)
                                join us in db.Master_UserStatus on u.UserStatus equals us.UserStatusId
                                select new
                                {
                                    oldstatusname = us.UserStatus
                                }).FirstOrDefault();

            var objNewStatus = db.Master_UserStatus.Where(u => u.UserStatusId == statusid).Select(u => u.UserStatus).FirstOrDefault();

            string MailBody = "";
            //var UserUpdateModel = updateuser;
            //var UserSavedModel = saveuser;
            //List<Variance> diff = extentions.DetailedCompare(UserUpdateModel, UserSavedModel);

            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Old Values</p> <br />";

            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "UserStatus" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + objOldStatus.oldstatusname + @"</label></p><br/>" + System.Environment.NewLine;


            MailBody += "<br /><p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>Updated Values</p>  <br />";


            MailBody += "<p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'><label style='width:25%'>" + "UserStatus" + "</label>:<label style='color: Black;'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + objNewStatus + @"</label></p><br/>" + System.Environment.NewLine;

            var objEmpDetailsUpdate = (from p in db.EmailPurposes.Where(x => x.EmailPurposeName == "Employee Details Update")
                                       join q in db.EmailTemplates on p.EmailTemplateID equals q.EmailTemplateID
                                       select new DSRCManagementSystem.Models.Email
                                       {
                                           To = p.To,
                                           CC = p.CC,
                                           BCC = p.BCC,
                                           Subject = p.Subject,
                                           Template = q.TemplatePath
                                       }).FirstOrDefault();

            var company = db.Master_ApplicationSettings.Where(x => x.AppKey == "Company Name").Select(o => o.AppValue).FirstOrDefault();
            string TemplatePathEmpDetailUpdate = Server.MapPath(objEmpDetailsUpdate.Template);
            string htmlEmpDetailUpdate = System.IO.File.ReadAllText(TemplatePathEmpDetailUpdate);
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmpId", Convert.ToString(objUser.EmpID));
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#EmployeeName", objUser.FirstName + " " + objUser.LastName);
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#MailBody", MailBody);
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ServerName", ServerName);
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#Date", DateTime.Today.ToString("dd MMM yyyy"));
            htmlEmpDetailUpdate = htmlEmpDetailUpdate.Replace("#ComapanyName", company);
            objEmpDetailsUpdate.To = db.Users.FirstOrDefault(o => o.UserID == objUser.UserID).EmailAddress;
            objEmpDetailsUpdate.CC = UsersController.GetUserEmailAddress(db, objEmpDetailsUpdate.CC);
            if (objEmpDetailsUpdate.BCC != null)
            {
                objEmpDetailsUpdate.BCC = UsersController.GetUserEmailAddress(db, objEmpDetailsUpdate.BCC);
            }

            //  string ServerName = WebConfigurationManager.AppSettings["SeverName"];
            var logo = CommonLogic.getLogoPath();

            if (ServerName != "http://win2012srv:88/")
            {
                List<string> MailIds = new List<string>();

                MailIds.Add("boobalan.k@dsrc.co.in");
                MailIds.Add("shaikhakeel@dsrc.co.in");
                MailIds.Add("ramesh.S@dsrc.co.in");
                MailIds.Add("aruna.m@dsrc.co.in");
                MailIds.Add("Virupaksha.Gaddad@dsrc.co.in");
                MailIds.Add("dineshkumar.d@dsrc.co.in");
                MailIds.Add("gopika.v@dsrc.co.in");
                MailIds.Add("vennimalai.n@dsrc.co.in");

                string EmailAddress = "";

                foreach (string mail in MailIds)
                {
                    EmailAddress += mail + ",";
                }

                EmailAddress = EmailAddress.Remove(EmailAddress.Length - 1);

                string CCMailId = "francispaul.k.c@dsrc.co.in";
                string BCCMailId = "Kirankumar@dsrc.co.in ";
                


                //var path = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(o => o).FirstOrDefault();
                var path = CommonLogic.getLogoPath();

                Session["LogoPath"] = path;
                Task.Factory.StartNew(() =>
                {
                    // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    // DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.AppValue.ToString()));
                    DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject + " - Test Mail Please Ignore", null, htmlEmpDetailUpdate + " - Testing Plaese ignore", "Test-admin@dsrc.co.in", EmailAddress, CCMailId, BCCMailId, Server.MapPath(logo.ToString()));
                });

            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    // var logo = db.Master_ApplicationSettings.Where(x => x.AppID == 7).Select(x => x).FirstOrDefault();
                    //  DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", objEmpDetailsUpdate.To, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.AppValue.ToString()));
                    DsrcMailSystem.MailSender.SendMailToALL(null, objEmpDetailsUpdate.Subject, "", htmlEmpDetailUpdate, "HRMS@dsrc.co.in", objEmpDetailsUpdate.To, objEmpDetailsUpdate.CC, objEmpDetailsUpdate.BCC, Server.MapPath(logo.ToString()));
                });
            }
        }
        #endregion

        #region Helper Methods
        public List<YearDropDown> YearsList()
        {
            List<YearDropDown> yearList = new List<YearDropDown>() { new YearDropDown() { Year = "---Select---", YearId = -1 } };
            foreach (int i in Enumerable.Range(0, 99))
            {
                yearList.Add(new YearDropDown() { Year = i.ToString(), YearId = i });
            }

            return yearList;
        }
        private List<Master_Technologies> LoadTechnologies()
        {
            List<Master_Technologies> Technology = new List<Master_Technologies>();
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                Technology = (from data in db.Master_Technologies
                              select data).OrderBy(x => x.Tecnology).ToList();
            }
            return Technology;
        }
        public List<MonthDropDown> MonthsList()
        {
            List<MonthDropDown> monthList = new List<MonthDropDown>() { new MonthDropDown() { Month = "---Select---", MonthId = -1 } };
            foreach (int i in Enumerable.Range(0, 12))
            {
                monthList.Add(new MonthDropDown() { Month = i.ToString(), MonthId = i });
            }

            return monthList;
        }
        private string GenerateRandomPassword(int length)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-*&#+";
            char[] chars = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            return new string(chars);
        }
        private static string GetUserEmailAddress(DSRCManagementSystemEntities1 db, string Attendee)
        {
            List<int> lst = new List<int>();
            foreach (var str in Attendee.Split(','))
            {
                lst.Add(Convert.ToInt32(str));
            }
            var obj = (from user in db.Users.Where(user => lst.Contains(user.UserID)) select user.EmailAddress).ToList();
            var tmp = "";
            int len = obj.Count; int i = 0;
            foreach (var str in obj)
            {
                i++;
                tmp += str;
                if (i < len)
                {
                    tmp += ", ";
                }
            }
            return tmp;
        }
        private string GenerateRandomPasswordKey(int length)
        {
            string AllowedNo = "0123456789";
            char[] Key = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                Key[i] = AllowedNo[rd.Next(0, AllowedNo.Length)];
            }
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            string strkey = new string(Key);
            Int32 pwkey = Convert.ToInt32(strkey);
            if (db.Users.Where(o => o.PasswordKey == pwkey).Any())
            {
                //key exists
                GenerateRandomPasswordKey(5);
            }
            return new string(Key);
        }
        #endregion


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAvailEmployees(int DepartmentName)
        {

            IEnumerable<SelectListItem> Employees = new List<SelectListItem>();

            try
            {
                if (DepartmentName != 0)
                {


                    Employees = (from d in db.Departments
                                 join dgm in db.DepartmentGroupMappings on d.DepartmentId equals dgm.DepartmentID
                                 join dg in db.DepartmentGroups on dgm.GroupID equals dg.GroupID
                                 where d.IsActive == true && dg.IsActive == true && d.DepartmentId == DepartmentName
                                 select new DSRCEmployees
                                 {
                                     Name = dg.GroupName,
                                     UserId = dg.GroupID,

                                 }).OrderBy(x => x.Name).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.UserId), Text = m.Name });

                }
                return Json(new SelectList(Employees, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStates(int CountryId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
            try
            {
                if (CountryId != 0)
                {

                    List<int> validDepart = new List<int>();
                    FilterDepart = (from lt in db.Master_States.Where(x => x.CountryID == CountryId)
                                    select new State
                                    {
                                        StateId = lt.StateID,
                                        StateName = lt.States
                                    }).OrderBy(x => x.StateName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.StateId), Text = m.StateName });
                }
                return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }

        //StateId

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCity(int StateId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            IEnumerable<SelectListItem> FilterDepart = new List<SelectListItem>();
            try
            {
                if (StateId != 0)
                {

                    List<int> validDepart = new List<int>();
                    FilterDepart = (from lt in db.Master_City.Where(x => x.StateID == StateId)
                                    select new City
                                    {
                                        CityId = lt.CityID,
                                        CityName = lt.CityName
                                    }).OrderBy(x => x.CityName).AsEnumerable().Select(m => new SelectListItem() { Value = Convert.ToString(m.CityId), Text = m.CityName });
                }
                return Json(new SelectList(FilterDepart, "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
            }
            return View();
        }


    }
}

