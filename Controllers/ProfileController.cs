using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DSRCManagementSystem.Models;
using DSRCManagementSystem;
using System.Web.SessionState;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.Web.Helpers;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using DSRCManagementSystem.DSRCLogic;



namespace DSRCManagementSystem.Controllers
{
    /***************Experience Months and Years Calculation************************/
    public struct DateTimeSpan
    {
        private readonly int years;
        private readonly int months;
        private readonly int days;
        //private readonly int hours;
        //private readonly int minutes;
        //private readonly int seconds;
        //private readonly int milliseconds;

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            this.years = years;
            this.months = months;
            this.days = days;
            //this.hours = hours;
            //this.minutes = minutes;
            //this.seconds = seconds;
            //this.milliseconds = milliseconds;
        }

        public int Years { get { return years; } }
        public int Months { get { return months; } }
        public int Days { get { return days; } }
        //public int Hours { get { return hours; } }
        //public int Minutes { get { return minutes; } }
        //public int Seconds { get { return seconds; } }
        //public int Milliseconds { get { return milliseconds; } }

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date1;
            int years = 0;
            int months = 0;
            int days = 0;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }
                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                        }
                        else
                        {
                            months++;
                        }
                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }
                        break;
                }
            }

            return span;
        }
    }
    /************************Ends Here**********************/


    public class ProfileController : Controller
    {
        //
        // GET: /Profile/

        public ActionResult ViewProfile()
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_group = CommonLogic.getLabelName(3).ToString();

            ViewBag.Lbl_branch = CommonLogic.getLabelName(1).ToString();

            
              using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())     
   


            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();
                ViewBag.Message = "";

                var Country = (from c in db.Master_Country
                               select new
                               {
                                   PermanentCountryID = c.CountryID,
                                   PermanentCountry = c.CountryName


                               }).ToList();
                ViewBag.permanentCountry = new SelectList(Country, "PermanentCountryID", "PermanentCountry");


                var state = (from s in db.Master_States
                             select new
                             {
                                 PermanentstateID = s.StateID,
                                 Permanentstate = s.States
                             }).ToList();

                ViewBag.permanentstate = new SelectList(state, "PermanentstateID", "Permanentstate");



                var city = (from c in db.Master_City
                            select new

                            {
                                PermanentCityID = c.CityID,
                                PermanentCity = c.CityName

                            }).ToList();

                ViewBag.permanentcity = new SelectList(city, "PermanentCityID", "PermanentCity");



                var tempCountry = (from c in db.Master_Country
                               select new
                               {
                                   PresentCountryID = c.CountryID,
                                   PresentCountry = c.CountryName


                               }).ToList();
                ViewBag.presentCountry = new SelectList(tempCountry, "PresentCountryID", "PresentCountry");


                var tempState = (from s in db.Master_States
                             select new
                             {
                                 PresentstateID = s.StateID,
                                 Presentstate = s.States
                             }).ToList();

                ViewBag.presentState = new SelectList(tempState, "PresentstateID", "Presentstate");



                var tempCity = (from c in db.Master_City
                            select new

                            {
                                PresentCityID = c.CityID,
                                PresentCity = c.CityName

                            }).ToList();

                ViewBag.presentCity = new SelectList(tempCity, "PresentCityID", "PresentCity");








                var RelationShip = (from r in db.Master_Relationship
                                    select new
                                    {
                                        RelationShipID = r.RelationshipID,
                                        RelationShipName = r.RelationshipName
                                    }).ToList();
                ViewBag.RelationShip = new SelectList(RelationShip, "RelationShipID ", "RelationShipName");


                var Nationality = (from r in db.Master_Nationality
                                   select new
                                   {
                                       NationalityID = r.NationalityID,
                                       Nationality = r.NationalityName
                                   }).ToList();
                ViewBag.Nationality = new SelectList(Nationality, "NationalityID ", "Nationality");

                var BloodGroup = (from b in db.Master_BloodGroup
                                  select new
                                  {
                                      BloodGroup = b.BloodGroupName,
                                      BloodGroupID = b.BloodGroupID
                                  }).ToList();

                ViewBag.BloodGroup = new SelectList(BloodGroup, "BloodGroupID", "BloodGroup");


                var Religious = (from r in db.Master_Religious
                                 select new
                                 {
                                     ReligiousID = r.ReligiousID,
                                     Religious = r.ReligiousName
                                 }).ToList();
                ViewBag.Religious = new SelectList(Religious, "ReligiousID ", "Religious");


                List<int> ChildCount = new List<int>();

                for (int i = 1; i < 10; i++)
                {
                    ChildCount.Add(i);
                }
                ViewBag.Child = new SelectList(ChildCount, "", "");

                
                {
                    Profile objProfile = new Profile();
                //   var user = db.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(userName) && x.IsActive == true);
                    var user = db.Users.Where(x => (x.UserName == userName) && x.IsActive == true).FirstOrDefault();
                    
                   var permanentAddress = db.PermanentAddresses.Where(x => x.PermanentAddressID == user.PermanentAddressID).Select(o => o).First(); 
                    string permanentAddressAddress_1 = Convert.ToString(permanentAddress.Address_1);
                    string permanentAddress2 = Convert.ToString(permanentAddress.Address_2);
                    string permanentAddress3 = Convert.ToString(permanentAddress.Address_3);
                    int permanentCountryId = Convert.ToInt32(permanentAddress.CountryID);
                    int permanentState = ((permanentAddress.State == "") || (permanentAddress.State == null)) ? 0 : Convert.ToInt32(permanentAddress.State);
                    int permanentCity = ((permanentAddress.City == "") || (permanentAddress.City == null)) ? 0 : Convert.ToInt32(permanentAddress.City);
                    int permanentPincode = Convert.ToInt32(permanentAddress.Zip);
                  
                    string presentAddressAddress_1;
                    string presentAddress2;
                    string presentAddress3;
                    int presentCountryId;
                    int presentState;
                    int presentCity;
                    int presentPincode;

                  
                    if (user.TemporaryAddressID == null)
                   {
                       presentAddressAddress_1 = "";  
                       presentAddress2  = "";
                       presentAddress3 = "";
                       presentCity = 0;
                       presentCountryId = 0;
                       presentState = 0;
                       presentPincode = 0;
                   }
                   else
                   {
                       var presentAddress = db.PresentAddresses.Where(x => x.PresentAddressID == user.TemporaryAddressID).FirstOrDefault();
                       presentAddressAddress_1 = Convert.ToString(presentAddress.Address_1);
                       presentAddress2  = Convert.ToString(presentAddress.Address_2);
                       presentAddress3 = Convert.ToString(presentAddress.Address_3);
                       presentCountryId = Convert.ToInt32(presentAddress.CountryID);
                       presentState = ((presentAddress.State == "") || (presentAddress.State == "null") )? 0 : Convert.ToInt32(presentAddress.State);
                       presentCity = ((presentAddress.City == "")|| (presentAddress.City == "null")) ? 0 : Convert.ToInt32(presentAddress.City);
                       presentPincode = Convert.ToInt32(presentAddress.Zip);

                       
                   }
                   
                    var skill = db.UserSkills.FirstOrDefault(x => x.UserID == user.UserID);
                    objProfile.EmpID = (user.EmpID == null) ? "" : user.EmpID;
                    objProfile.FirstName = (user.FirstName == null) ? "" : user.FirstName;
                    objProfile.MiddleName = (user.MiddleName == null) ? "" : user.MiddleName;
                    objProfile.LastName = (user.LastName == null) ? "" : user.LastName;
                    objProfile.UserName = userName;
                    objProfile.Password = user.Password;
                    objProfile.DateOfJoin = (user.DateOfJoin.HasValue) ? user.DateOfJoin.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";
                    objProfile.DateOfBirth = (user.DateOfBirth.HasValue) ? user.DateOfBirth.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";
                    objProfile.ContactNo = (user.ContactNo == null) ? "" : Convert.ToString(user.ContactNo);
                    objProfile.EmailAddress = user.EmailAddress;
                    objProfile.IPAddress = (user.IPAddress == null) ? "" : user.IPAddress;
                    objProfile.MachineName = (user.MachineName == null) ? "" : user.MachineName;
                    objProfile.IsUnderProbation = Convert.ToBoolean(user.IsUnderProbation);
                    //objProfile.ReportingPerson = user.mul
                    objProfile.IsUnderNoticePeriod = Convert.ToBoolean(user.IsUnderNoticePeriod);   
                    objProfile.Skills = (skill == null) ? "" : skill.Skills ?? "";            
                    objProfile.UserID = user.UserID;
                    objProfile.DepartmentId = user.DepartmentId;
                    objProfile.DepartmentName = GetDepartment(user.DepartmentId);
                    objProfile.PermanentAddress = permanentAddressAddress_1;
                    objProfile.PermanentAddress2 = permanentAddress2 ;
                    objProfile.PermanentAddress3 = permanentAddress3;
                    objProfile.PermanentCountryID = permanentCountryId;
                    objProfile.PermanentCityID = permanentCity;
                    objProfile.PermanentstateID = permanentState;

                   
                    objProfile.TemporaryAddress = presentAddressAddress_1;
                    objProfile.PresentAddress2 = presentAddress2;
                    objProfile.PresentAddress3 = presentAddress3;
                    objProfile.PresentCountryID = presentCountryId;
                    objProfile.PresentCityID = presentCity;
                    objProfile.PresentstateID = presentState;
                    objProfile.PresentPinCode = presentPincode;


                    objProfile.FatherName = user.FatherName;
                    objProfile.MotherName = user.MotherName;
                    objProfile.BirthPlace = user.BirthPlace;
                    objProfile.NationalityID = user.NationalityID;
                    objProfile.ReligiousID = user.ReligionID;
                    objProfile.BloodGroupID = ((user.BloodGroup == "") || (user.BloodGroup == null)) ? 0 : Convert.ToInt32(user.BloodGroup);
                    objProfile.Emergency = user.EmergencyContact;
                    objProfile.EmergencyContactName = user.Contactperson;
                    objProfile.RelationShipID = user.RelationshipID;
                    objProfile.SpouseName = user.SpouseName;
                    objProfile.NoOfChild = user.NoOfChild;
                    objProfile.AnniversaryDate = user.AnniversaryDate;


                    objProfile.HasImage = (db.UserProfiles.Any(x => x.UserID == user.UserID && x.IsDeleted == false));
                    objProfile.RoleID = db.UserRoles.FirstOrDefault(item => item.UserID == user.UserID).RoleID;
                    objProfile.OfficeSkypeId = user.OfficeSkypeId;
                    objProfile.WorkPlace = user.Workplace;
                    objProfile.extension =user.extension;
                  // objProfile.WorkPlace = user.Workplace;

                    objProfile.officeno = user.officeno;
                    objProfile.Marital =user.MaritalStatus;

                    objProfile.BranchID = user.BranchId;


                   // objProfile.BranchID =(int)user.BranchId;



                    objProfile.Previous = (user.Experience == null) ? "0.0" : user.Experience;
                    objProfile.GenderID = user.Gender;


                    var DesignationList = (from
                                r in db.Master_Designation
                                           select new
                                           {
                                               DesignationID = r.DesignationID,
                                               DesignationName = r.DesignationName
                                           }).ToList();


                    var GroupList = db.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                    {
                        groupid = g.GroupID,
                        groupname = g.GroupName
                    }).ToList();
                    var Zone = (from p in db.TimeZones
                                select new
                                {
                                    RegionId = p.Id,
                                    Region = p.Zone
                                }).ToList();
                    var BranchList = (from d in db.Master_Branches
                                      select new DSRCEmployees
                                      {
                                          Name = d.BranchName,
                                          BranchID = d.BranchID,

                                      }).ToList();
                    //added 29/9
                    //ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name");

                    ViewBag.BranchList = new SelectList(BranchList, "BranchID", "Name",user.BranchId );


                    var EmployeeList = (from p in db.Users.Where(x => x.IsActive == true && x.UserID != user.UserID)
                                        select new
                                        {
                                            ID1 = p.UserID,
                                            UserName1 = p.FirstName + " " + (p.LastName.Length > 0 ? p.LastName : "")
                                        }).ToList();

                    var GenderNameList = (from us in db.Master_Gender
                                          select new
                                          {
                                              GenderID = us.GenderID,
                                              GenderName = us.GenderName
                                          }).ToList();

                    ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", user.DesignationID);
                    ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", user.DepartmentGroup);
                    ViewBag.Region = new SelectList(Zone, "RegionId", "Region", user.Region);
                    ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName",user.Gender );



                    List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();

                    //objuser = (from p in db.UserReportings.Where(x => x.UserID == user.UserID)
                    //           select new DSRCManagementSystem.Models.ListReporting
                    //           {
                    //               Id = p.ReportingUserID

                    //           }).ToList();


                     objuser = (from p in db.UserReportings.Where(x => x.UserID == user.UserID)
                               select new DSRCManagementSystem.Models.ListReporting
                               {
                                   Id = p.ReportingUserID

                               }).ToList();
                    List<int> selectedAttendees = new List<int>();


                    for (int i = 0; i < objuser.Count(); i++)
                    {
                        selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                    }


                    ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);


                    if (objProfile.GenderID ==1)
                    {
                        ViewBag.GenderID = true;
                    }
                    else
                    {
                        ViewBag.GenderID = false;
                    }

                    if (objProfile.WorkPlace != null)
                    {
                        ViewBag.WorkPlaceId = true;
                    }
                    else
                    {
                        ViewBag.WorkPlaceId = false;
                    }

                    if (objProfile.Marital == 1)
                    {
                        ViewBag.IsMarried = true;
                    }
                    else
                    {
                        ViewBag.IsMarried = false;
                    }

                    DateTime DOJ = Convert.ToDateTime(user.DateOfJoin);
                    DateTime Today = DateTime.Now;
                    var curExp = DateTimeSpan.CompareDates(DOJ, Today);


                    var curyears = (int)curExp.Years;

                    var curmonths = (int)curExp.Months;

                    string prevexp = user.Experience ?? "0.0";

                    string[] split = prevexp.Split('.');


                    int peryears;
                    int permonths;

                    if (split.Length > 1)
                    {
                        peryears = Convert.ToInt32(split[0]);

                        permonths = Convert.ToInt32(split[1]);
                    }
                    else
                    {
                        peryears = Convert.ToInt32(split[0]);

                        permonths = 0;
                    }


                    int totalmonths = curmonths + permonths;

                    int totalyears =Convert.ToInt32(curyears + peryears);

                    if (totalmonths >= 12)
                    {
                        int additionalyears = (totalmonths) / 12;
                        int additionalmonths = (totalmonths) % 12;

                        totalyears += additionalyears;
                        totalmonths = additionalmonths;
                    }

                    string FinalExperience = totalyears + "." + totalmonths;

                    var dateSpan = DateTimeSpan.CompareDates(DOJ, Today);
                   

                    double months = 0.0;

                    if (dateSpan.Months >=10)
                    {
                        months = (double)dateSpan.Months /(double)100;
                    }
                    else
                    {
                        months = (double)dateSpan.Months / (double)10;
                    }

                  

                    double Current = (double)dateSpan.Years +(double)months;
                  //  double previousd = peryears + permonths;
                   //double OverAll = Convert.ToDouble(previousd + Current);
                    double OverAll = Convert.ToDouble(FinalExperience);


                    string Total = " ";
                    string CurrentExp = " ";

                    if (dateSpan.Months == 10)
                    {

                        Total = string.Format("{0}", OverAll);
                        CurrentExp = string.Format("{0}", Current);
                    }
                    else
                    {
                        CurrentExp = Current.ToString();
                        Total = OverAll.ToString();
                    }

                    objProfile.CurrentExperience = CurrentExp;
                    objProfile.OverallExperience = FinalExperience;

                    var WorkPlaceList = (from p in db.Master_WorkPlace
                                         select new
                                         {
                                             WorkplaceId = p.WorkPlaceID,
                                             Name = p.WorkPlaceName

                                         }).ToList();



                  //  ViewBag.WorkPlaceList = new SelectList(WorkPlaceList, "WorkplaceId", "Name");





                    // ViewBag.WorkPlaceList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "DSRC", Value = 1 }, new { Text = "DSRC Haddows Road", Value = 2 }, new { Text = "Client Place", Value = 3 }, new { Text = "Work From Home", Value = 4 } }, "Value", "Text", 0);
                    ViewBag.WorkPlaceList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "DSRC", Value = 1 }, new { Text = "DSRC Haddows Road", Value = 2 }, new { Text = "Client Place", Value = 3 }, new { Text = "Work From Home", Value = 4 } }, "Value", "Text", 0);
                    ViewBag.MaritalList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Married", Value = 1 }, new { Text = "Single", Value = 2 } }, "Value", "Text", 0);

                  //  ViewBag.BranchList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "DSRC Smith Road", Value = 1 }, new { Text = "DSRC Haddows Road", Value = 2 } }, "Value", "Text", user.BranchId);


                   //ViewBag.GenderList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Male", Value = 1 }, new { Text = "Female", Value = 2 } }, "Value", "Text", 0);
                    

                    if (user.EmailAddress == "" || user.EmpID == "" || user.FirstName == "" || user.LastName == "" || user.IPAddress == null || user.DateOfBirth == null || user.DateOfJoin == null || user.ContactNo == null || user.PermanentAddressID == 0 || user.Department == null || user.Workplace == null || user.MaritalStatus == null)
                    {
                        ViewBag.isalfiled=true;
                    }
                    else
                    {
                        ViewBag.isalfiled=false;
                    }
                    return View(objProfile);
                   
                }
                
            }
            
            return RedirectToAction("Login", "User");
        }

        public ActionResult ViewEmployeeProfile(string UserId)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                int userID = Convert.ToInt32(UserId);
                ViewBag.Message = "";

                Profile objProfile = new Profile();

                var user = db.Users.FirstOrDefault(x => x.UserID == userID);
                var skill = db.UserSkills.FirstOrDefault(x => x.UserID == user.UserID);

                objProfile.EmpID = (user.EmpID == null) ? "" : user.EmpID;
                objProfile.FirstName = (user.FirstName == null) ? "" : user.FirstName;
                objProfile.MiddleName = (user.MiddleName == null) ? "" : user.MiddleName;
                objProfile.LastName = (user.LastName == null) ? "" : user.LastName;
                objProfile.UserName = user.UserName;
                objProfile.Password = user.Password;
                objProfile.DateOfJoin = (user.DateOfJoin.HasValue) ? user.DateOfJoin.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";
                objProfile.DateOfBirth = (user.DateOfBirth.HasValue) ? user.DateOfBirth.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";
                objProfile.ContactNo = (user.ContactNo == null) ? "" : Convert.ToString(user.ContactNo);
                objProfile.EmailAddress = user.EmailAddress;
                objProfile.IPAddress = (user.IPAddress == null) ? "" : user.IPAddress;
                objProfile.MachineName = (user.MachineName == null) ? "" : user.MachineName;
                objProfile.IsUnderProbation = Convert.ToBoolean(user.IsUnderProbation);
                objProfile.IsUnderNoticePeriod = Convert.ToBoolean(user.IsUnderNoticePeriod);
                objProfile.Skills = (skill == null) ? "" : skill.Skills ?? "";
                objProfile.UserID = user.UserID;
                objProfile.DepartmentId = user.DepartmentId;
                objProfile.DepartmentName = GetDepartment(user.DepartmentId);
                objProfile.PermanentAddress =Convert.ToString( user.PermanentAddressID);
                objProfile.TemporaryAddress =Convert.ToString( user.TemporaryAddressID);
                objProfile.HasImage = (db.UserProfiles.Any(x => x.UserID == user.UserID && x.IsDeleted == false));
                objProfile.RoleID = db.UserRoles.FirstOrDefault(item => item.UserID == user.UserID).RoleID;
                objProfile.OfficeSkypeId = user.OfficeSkypeId;
                objProfile.WorkPlace = user.Workplace;
                objProfile.extension = user.extension;
                objProfile.officeno = user.officeno;
                objProfile.Marital = user.MaritalStatus;

                //if (user.Experience != "" && user.Experience != null)
                //{
                //    user.Experience= user.Experience + ".0";
                //}

                objProfile.Previous = (user.Experience == null) ? "0.0" : user.Experience;
                objProfile.GenderID = user.Gender;
                DateTime DOJ = Convert.ToDateTime(user.DateOfJoin);
                DateTime Today = DateTime.Now;

                var curExp = DateTimeSpan.CompareDates(DOJ, Today);

                var curyears = (int)curExp.Years;

                var curmonths = (int)curExp.Months;

                string prevexp = user.Experience ?? "0.0";

                string[] split = prevexp.Split('.');


                int peryears;
                int permonths;

                if (split.Length > 1)
                {
                    peryears = Convert.ToInt32(split[0]);

                    permonths = Convert.ToInt32(split[1]);
                }
                else
                {
                    peryears = Convert.ToInt32(split[0]);

                    permonths = 0;
                }

                int totalmonths = curmonths + permonths;

                int totalyears = Convert.ToInt32(curyears + peryears);

                if (totalmonths >= 12)
                {
                    int additionalyears = (totalmonths) / 12;
                    int additionalmonths = (totalmonths) % 12;

                    totalyears += additionalyears;
                    totalmonths = additionalmonths;
                }

                string FinalExperience = totalyears + "." + totalmonths;

                var dateSpan = DateTimeSpan.CompareDates(DOJ, Today);

                double months = 0.0;

                if (dateSpan.Months >= 10)
                {
                    months = (double)dateSpan.Months / (double)100;
                }
                else
                {
                    months = (double)dateSpan.Months / (double)10;
                }

                double Current = (double)dateSpan.Years + (double)months;

                double OverAll = Convert.ToDouble(objProfile.Previous) + Current;

                string Total = " ";
                string CurrentExp = " ";

                if (dateSpan.Months == 10)
                {
                    Total = string.Format("{0}", OverAll);
                    CurrentExp = string.Format("{0}", Current);
                }
                else
                {
                    CurrentExp = Current.ToString();
                    Total = OverAll.ToString();
                }

                objProfile.CurrentExperience = CurrentExp;
                objProfile.OverallExperience = FinalExperience;
                var GenderNameList = (from us in db.Master_Gender
                                      select new
                                      {
                                          GenderID = us.GenderID,
                                          GenderName = us.GenderName
                                      }).ToList();
                ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName",user.Gender);
               // ViewBag.GenderList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Male", Value = 1 }, new { Text = "Female", Value = 2 } }, "Value", "Text", 0);
                ViewBag.WorkPlaceList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "DSRC", Value = 1 }, new { Text = "DSRC Haddows Road", Value = 2 }, new { Text = "Client Place", Value = 3 }, new { Text = "Work From Home", Value = 4 } }, "Value", "Text", 0);
                ViewBag.MaritalList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Married", Value = 1 }, new { Text = "Single", Value = 2 } }, "Value", "Text", 0);

                if (user.EmailAddress == "" || user.EmpID == "" || user.FirstName == "" || user.LastName == "" || user.IPAddress == null || user.DateOfBirth == null || user.DateOfJoin == null || user.ContactNo == null || user.PermanentAddressID == 0 || user.Department == null || skill.Skills == null || user.Workplace == null )
                {
                    ViewBag.isalfiled = true;
                }
                else
                {
                    ViewBag.isalfiled = false;
                }

                return View(objProfile);
            }
        }
        private List<ReportingPerson> GetReportingPersons(int id = 0)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                if (id != 42)
                {
                    List<ReportingPerson> reportingPersons = (from r in db.Master_Roles
                                                              join ur in db.UserRoles on r.RoleID equals ur.RoleID
                                                              join u in db.Users on ur.UserID equals u.UserID
                                                              where r.RoleID == 4 || r.RoleID == 8 || r.RoleID == 44 || r.RoleID == 42
                                                              || r.RoleID == 40 || r.RoleID == 47 || r.RoleID == 60 || r.RoleID == 67
                                                              || r.RoleID == 59 || r.RoleID == 26 ||r.RoleID==30
                                                              select new ReportingPerson
                                                              {
                                                                  UserID = u.UserID,
                                                                  Name = u.FirstName + " " + (u.LastName.Length > 0 ? u.LastName : "")
                                                              }).OrderBy(o => o.Name).ToList();
                    reportingPersons.RemoveAll(x => x.UserID == userID);
                    return reportingPersons;

                }
                else
                {
                    return new List<ReportingPerson>();
                }
            }
        }
        
        private string GetDepartment(int? deptId = 0)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var departmentName = (from data in db.Departments
                                      where data.DepartmentId == deptId
                                      select data.DepartmentName).FirstOrDefault();
                return departmentName;
            }
        }
        [HttpPost]
        public ActionResult ViewProfile(Profile collection)
        {
            ViewBag.Lbl_department = CommonLogic.getLabelName(2).ToString();
            ViewBag.Lbl_depgroup = CommonLogic.getLabelName(3).ToString();
            DSRCManagementSystemEntities1 db1 = new DSRCManagementSystemEntities1();
           
            List<int> selectedAttendees = new List<int>();
            List<DSRCManagementSystem.Models.ListReporting> objuser = new List<DSRCManagementSystem.Models.ListReporting>();

            int userID = Convert.ToInt32(Session["UserID"]);
            string userName = Session["UserName"].ToString();
            

           
            
            
            collection.DepartmentName = GetDepartment(collection.DepartmentId);
            ViewBag.Message = "";
            
            var fileExtension = collection.Photo != null ? Path.GetExtension(collection.Photo.FileName) : "";
          
            var DesignationList = (from
                                 r in db1.Master_Designation
                                   select new
                                   {
                                       DesignationID = r.DesignationID,
                                       DesignationName = r.DesignationName
                                   }).ToList();
            
            var GroupList = db1.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
            {
                groupid = g.GroupID,
                groupname = g.GroupName
            }).ToList();


            var GenderNameList = (from us in db1.Master_Gender
                                  select new
                                  {
                                      GenderID = us.GenderID,
                                      GenderName = us.GenderName
                                  }).ToList();

            ViewBag.GenderList = new SelectList(GenderNameList, "GenderID ", "GenderName");
            //ViewBag.GenderList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Male", Value = 1 }, new { Text = "Female", Value = 2 } }, "Value", "Text", 0);

            var BranchList = (from b in db1.Master_Branches
                                          select new
                                          {
                                              BranchID = b.BranchID,
                                              BranchName = b.BranchName

                                          }).ToList();
            ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName");

            var Zone = (from p in db1.TimeZones
                        select new
                        {
                            RegionId = p.Id,
                            Region = p.Zone
                        }).ToList();
            var EmployeeList = (from p in db1.Users.Where(x => x.IsActive == true && x.UserID != collection.UserID)
                                select new
                                {
                                    ID1 = p.UserID,
                                    UserName1 = p.FirstName + " " + (p.LastName.Length > 0 ? p.LastName : "")
                                }).ToList();

            //added on 28/9
            
            //var workplace = (from m in db1.Users.Where(x => x.EmpID == collection.EmpID).Select(x => x.Workplace)
            //                 select new
            //                 {
            //                     ID=m
            //                 }).ToList();
          
            //                 //select new
            //                 //{
            //                 //    ID = m
            //                 //});

           
            ////int value;
            //var elemnt = workplace.ElementAt(0);

            var WorkPlaceList = (from p in db1.Master_WorkPlace
                                 select new
                                 {
                                     WorkplaceId = p.WorkPlaceID,
                                     Name = p.WorkPlaceName

                                 }).ToList();



            ViewBag.WorkPlaceList = new SelectList(WorkPlaceList, "WorkplaceId", "Name");


            ViewBag.MaritalList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Married", Value = 1 }, new { Text = "Single", Value = 2 } }, "Value", "Text", 0);
            ViewBag.WorkPlaceList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "DSRC", Value = 1 }, new { Text = "DSRC Haddows Road", Value = 2 }, new { Text = "Client Place", Value = 3 }, new { Text = "Work From Home", Value = 4 } }, "Value","Text");
            ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", collection.DesignationName);
            ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", collection.GroupName);
            ViewBag.Region = new SelectList(Zone, "RegionId", "Region", collection.RegionId);
            //ViewBag.BranchList = new SelectList(BranchList, "BranchID", "BranchName", 1);

         //   ViewBag.Nationality = new SelectList(Nationality, "", "", collection.Nationality);

           
            

            objuser = (from p in db1.UserReportings.Where(x => x.UserID == collection.UserID)
                       select new DSRCManagementSystem.Models.ListReporting
                       {
                           Id = p.ReportingUserID

                       }).ToList();

           


            for (int i = 0; i < objuser.Count(); i++)
            {
                selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
            }


            ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);

            if (ModelState.IsValid)
            {
                try
                {
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {

                        var user = db.Users.Where(x => x.UserID == collection.UserID).Select(o => o).FirstOrDefault();

                        var usersinfo = db.Users.FirstOrDefault(x => (x.UserName ?? "").Equals(userName) && x.IsActive == true);



                        int permanent = Convert.ToInt32(user.PermanentAddressID);
                        int present = Convert.ToInt32(user.TemporaryAddressID);

                        if (present != 0)
                        {
                            var presentAddress = db.PresentAddresses.Where(x => x.PresentAddressID == present).Select(o => o).First();
                            presentAddress.Address_1 = Convert.ToString(collection.TemporaryAddress);
                            presentAddress.Address_2 = Convert.ToString(collection.PresentAddress2);
                            presentAddress.Address_3 = Convert.ToString(collection.PresentAddress3);
                            presentAddress.CountryID = Convert.ToInt32(collection.PresentCountryID);
                            presentAddress.City = Convert.ToString(collection.PresentCityID);
                            presentAddress.State = Convert.ToString(collection.PresentstateID);
                            presentAddress.Zip = Convert.ToInt32(collection.PresentPinCode);
                        }

                        if (permanent != 0)
                        {
                            var permanentAddress = db.PermanentAddresses.Where(x => x.PermanentAddressID == permanent).Select(o => o).First();

                            permanentAddress.Address_1 = Convert.ToString(collection.PermanentAddress);
                            permanentAddress.Address_2 = Convert.ToString(collection.PermanentAddress2);
                            permanentAddress.Address_3 = Convert.ToString(collection.PermanentAddress3);
                            permanentAddress.CountryID = Convert.ToInt32(collection.PermanentCountryID);
                            permanentAddress.City = Convert.ToString(collection.PermanentCityID);
                            permanentAddress.State = Convert.ToString(collection.PermanentstateID);
                            permanentAddress.Zip = Convert.ToInt32(collection.PermanentPinCode);

                        }

                        user.MiddleName = collection.MiddleName;

                        user.DateOfJoin = Convert.ToDateTime(collection.DateOfJoin);
                        user.DateOfBirth = Convert.ToDateTime(collection.DateOfBirth);
                        user.ContactNo = Convert.ToInt64(collection.ContactNo);
                        user.IPAddress = collection.IPAddress;
                        user.UserName = userName;//added on 28/9
                        //user.UserName = collection.UserName;
                        user.MachineName = collection.MachineName;
                        user.IsUnderProbation = collection.IsUnderProbation;
                        user.IsUnderNoticePeriod = collection.IsUnderNoticePeriod;
                        //user.PermanentAddressID = collection.PermanentAddress;
                        //user.TemporaryAddressID = collection.TemporaryAddress;
                        user.OfficeSkypeId = collection.OfficeSkypeId;
                        //user.Workplace = collection.WorkPlace;
                        user.extension = collection.extension;
                        user.officeno = ((collection.officeno == null) ? null : collection.officeno);
                        user.MaritalStatus = collection.Marital;
                        if (collection.Previous != null || collection.Previous != "0")
                        {
                            string[] split = collection.Previous.Split('.');
                            if (split.Length <= 1)
                            {
                              collection.Previous = collection.Previous + " .0";
                            }
                            
                        }
                        else
                            collection.Previous = "0.0";
                        user.Experience = collection.Previous;
                        //user.BranchId = collection.BranchID;
                        //user.Gender = collection.GenderID;

                        user.BloodGroup =  Convert.ToString(collection.BloodGroupID);
                        user.MotherName = collection.MotherName;
                        user.FatherName = collection.FatherName;
                        user.NationalityID = collection.NationalityID;
                        user.ReligionID = collection.ReligiousID;
                        user.BirthPlace = collection.BirthPlace;
                        user.SpouseName = collection.SpouseName;
                        user.NoOfChild = collection.NoOfChild;
                        user.AnniversaryDate = collection.AnniversaryDate;
                        user.EmergencyContact = collection.Emergency;
                        user.RelationshipID = collection.RelationShipID;
                        user.Contactperson = collection.EmergencyContactName;
                        user.Gender = collection.GenderID;

                        db.SaveChanges();

                        DesignationList = (from
                                 r in db1.Master_Designation
                                           select new
                                           {
                                               DesignationID = r.DesignationID,
                                               DesignationName = r.DesignationName
                                           }).ToList();


                        GroupList = db1.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                        {
                            groupid = g.GroupID,
                            groupname = g.GroupName
                        }).ToList();

                        Zone = (from p in db1.TimeZones
                                select new
                                {
                                    RegionId = p.Id,
                                    Region = p.Zone
                                }).ToList();
                        ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", collection.GroupName);
                        ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", collection.DesignationName);

                        ViewBag.Region = new SelectList(Zone, "RegionId", "Region", collection.RegionId);

                        EmployeeList = (from p in db1.Users.Where(x => x.IsActive == true && x.UserID != collection.UserID)
                                        select new
                                        {
                                            ID1 = p.UserID,
                                            UserName1 = p.FirstName + " " + (p.LastName.Length > 0 ? p.LastName : "")
                                        }).ToList();


                        objuser = (from p in db1.UserReportings.Where(x => x.UserID == collection.UserID)
                                   select new DSRCManagementSystem.Models.ListReporting
                                   {
                                       Id = p.ReportingUserID

                                   }).ToList();


                        for (int i = 0; i < objuser.Count(); i++)
                        {
                            selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                        }


                        ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);


                        if (db.UserSkills.Where(x => x.UserID == collection.UserID).ToList().Count == 0)
                        {
                            var objUserSkill = new UserSkill()
                            {
                                UserID = Convert.ToInt32(collection.UserID),
                                Skills = collection.Skills,
                            };
                            db.UserSkills.AddObject(objUserSkill);
                            db.SaveChanges();

                        }
                        else
                        {
                            var updateRecord = db.UserSkills.Where(x => x.UserID == collection.UserID).FirstOrDefault();
                            updateRecord.Skills = collection.Skills;
                            db.SaveChanges();

                        }
                        //if (db.UserProfiles.Where(x => x.UserID == collection.UserID).ToList().Count == 0)
                        //{ 

                        //}

                        if (db.UserProfiles.Where(x => x.UserID == collection.UserID).ToList().Count == 0)
                        {
                            var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                            if (collection.Photo != null && collection.Photo.ContentLength > 0)
                            {
                                if ((fileExtension == ".jpg") || (fileExtension == ".jpeg") || (fileExtension == ".gif") || (fileExtension == ".png") || (fileExtension == ".jfif"))
                                {
                                    var objUserProfile = new UserProfile()
                                    {
                                        UserID = Convert.ToInt32(collection.UserID),
                                        Photo = ConvertToBytes(collection.Photo),

                                        IsDeleted = false
                                        //IsDeleted=true
                                    };
                                    db.UserProfiles.AddObject(objUserProfile);
                                    db.SaveChanges();

                                    if (fileExtension == ".gif" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jfif")
                                    {
                                        bool ImgExists = System.IO.File.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + objUserProfile.UserID.ToString() + ".jpg")));
                                        var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + objUserProfile.UserID.ToString() + ".jpg")));
                                        //var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")), fileName);
                                        if (ImgExists == true)
                                        {
                                            FileInfo file = new FileInfo(path);
                                            file.Delete();
                                            collection.Photo.SaveAs(path);
                                            var comimage = collection.Photo;
                                            Image image = Image.FromStream(comimage.InputStream, true, true);
                                            if (image.Width > 1024 || image.Height > 768)
                                            {
                                                WebImage img = new WebImage(collection.Photo.InputStream);
                                                if (img.Width > 1000)
                                                {
                                                    img.Resize(1000, 1000);
                                                }
                                                img.Save(path);
                                            }
                                            //added 29/9
                                            //   collection.Photo.SaveAs(path);
                                        }

                                        else
                                        {
                                            collection.Photo.SaveAs(path);
                                        }


                                        collection.Photo.SaveAs(path);
                                        objUserProfile.IsDeleted = false;
                                        collection.HasImage = true;

                                    }


                                }
                                else
                                {

                                    ModelState.AddModelError("Photo", "Choose valid image format only.");
                                    return View(collection);
                                }
                            }
                        }

                        else
                        {
                            if (collection.Photo != null && collection.Photo.ContentLength > 0)
                            {
                                if ((fileExtension == ".jpg") || (fileExtension == ".jpeg") || (fileExtension == ".gif") || (fileExtension == ".png") || (fileExtension == ".jfif"))
                                {

                                    var updateProfile = db.UserProfiles.FirstOrDefault(x => x.UserID == collection.UserID && x.IsDeleted == true);

                                    var updateProfile1 = db.UserProfiles.FirstOrDefault(x => x.UserID == collection.UserID && x.IsDeleted == false);
                                    if (updateProfile != null)
                                    {
                                        updateProfile.Photo = ConvertToBytes(collection.Photo);
                                        var fileName = Path.GetFileName(collection.Photo.FileName);

                                        var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                                    }
                                    else if (updateProfile1 != null)
                                    {
                                        updateProfile1.Photo = ConvertToBytes(collection.Photo);
                                        var fileName = Path.GetFileName(collection.Photo.FileName);

                                        var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                                    }




                                    if (fileExtension == ".gif" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jfif")
                                    {
                                        if (updateProfile != null)
                                        {
                                            bool ImgExists = System.IO.File.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")));
                                            var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")));


                                            if (ImgExists == true)
                                            {
                                                FileInfo file = new FileInfo(path);
                                                file.Delete();
                                                collection.Photo.SaveAs(path);



                                                var comimage = collection.Photo;
                                                Image image = Image.FromStream(comimage.InputStream, true, true);
                                                if (image.Width > 1024 || image.Height > 768)
                                                {

                                                    WebImage img = new WebImage(collection.Photo.InputStream);
                                                    if (img.Width > 1000)
                                                    {
                                                        img.Resize(1000, 1000);
                                                    }
                                                    img.Save(path);

                                                }
                                                else
                                                {

                                                    collection.Photo.SaveAs(path);
                                                }
                                            }

                                            else
                                            {
                                                collection.Photo.SaveAs(path);
                                            }



                                            updateProfile.IsDeleted = false;

                                            collection.HasImage = true;

                                        }

                                    }
                                    else
                                    {
                                        ViewBag.Photostatus = 1;
                                        ModelState.AddModelError("Photo", "Choose valid image format only.");
                                        collection.HasImage = false;
                                        return View(collection);
                                    }
                                }

                                var user1 = db1.Users.Where(x => x.UserID == collection.UserID).FirstOrDefault();
                                if (user1 != null)
                                {
                                    DateTime DOJ = Convert.ToDateTime(user1.DateOfJoin);
                                    DateTime Today = DateTime.Now;
                                    var curExp = DateTimeSpan.CompareDates(DOJ, Today);


                                    var curyears = (int)curExp.Years;

                                    var curmonths = (int)curExp.Months;

                                    string prevexp = user1.Experience ?? "0.0";
                                    string[] split = prevexp.Split('.');



                                    int peryears;
                                    int permonths;

                                    if (split.Length > 1)
                                    {
                                        peryears = Convert.ToInt32(split[0]);

                                        permonths = Convert.ToInt32(split[1]);
                                    }
                                    else
                                    {
                                        peryears = Convert.ToInt32(split[0]);

                                        permonths = 0;
                                    }


                                    int totalmonths = curmonths + permonths;

                                    int totalyears = curyears + peryears;

                                    if (totalmonths >= 12)
                                    {
                                        int additionalyears = (totalmonths) / 12;
                                        int additionalmonths = (totalmonths) % 12;

                                        totalyears += additionalyears;
                                        totalmonths = additionalmonths;
                                    }

                                    string FinalExperience = totalyears + "." + totalmonths;

                                    var dateSpan = DateTimeSpan.CompareDates(DOJ, Today);

                                    double months = 0.0;

                                    if (dateSpan.Months >= 10)
                                    {
                                        months = (double)dateSpan.Months / (double)100;
                                    }
                                    else
                                    {
                                        months = (double)dateSpan.Months / (double)10;
                                    }

                                    double Current = (double)dateSpan.Years + (double)months;

                                    double OverAll = Convert.ToDouble(collection.Previous) + Current;




                                    string Total = " ";
                                    string CurrentExp = " ";

                                    if (dateSpan.Months == 10)
                                    {

                                        Total = string.Format("{0}", OverAll);
                                        CurrentExp = string.Format("{0}", Current);
                                    }
                                    else
                                    {
                                        CurrentExp = Current.ToString();
                                        Total = OverAll.ToString();
                                    }

                                    collection.CurrentExperience = CurrentExp;
                                    collection.OverallExperience = FinalExperience;

                                    //collection.WorkPlace = null;
                                    if (collection.WorkPlace != null)
                                    {
                                        ViewBag.WorkPlaceId = true;
                                    }
                                    else
                                    {
                                        ViewBag.WorkPlaceId = false;
                                    }

                                    if (collection.GenderID == 1)
                                    {
                                        ViewBag.GenderID = true;
                                    }
                                    else
                                    {
                                        ViewBag.GenderID = false;
                                    }
                                    if (collection.Marital == 1)
                                    {
                                        ViewBag.IsMarried = true;
                                    }
                                    else
                                    {
                                        ViewBag.IsMarried = false;
                                    }
                                }
                                else
                                {

                                }


                            }
                            Session["IsRequired"] = true;
                            db.SaveChanges();
                            ViewBag.Message = "Success";

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    throw new Exception(Ex.Message);
                }
            }







//modelstate not valid

            else
            {
                try
                {
                    using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
                    {

                        var user = db.Users.Where(x => x.UserID == collection.UserID).FirstOrDefault();
                        int permanent = Convert.ToInt32(user.PermanentAddressID);
                        int present = Convert.ToInt32(user.TemporaryAddressID);
                        var permanentAddress = db.PermanentAddresses.Where(x => x.PermanentAddressID == permanent).Select(o => o).First();
                        var presentAddress = db.PresentAddresses.Where(x => x.PresentAddressID == present).Select(o => o).First();
                        permanentAddress.Address_1 = Convert.ToString(collection.PermanentAddress);
                        presentAddress.Address_1 = Convert.ToString(collection.TemporaryAddress);
                       
                        
                        user.MiddleName = collection.MiddleName;
                        user.DateOfJoin = Convert.ToDateTime(collection.DateOfJoin);
                        user.DateOfBirth = Convert.ToDateTime(collection.DateOfBirth);
                        user.ContactNo = Convert.ToInt64(collection.ContactNo);
                        user.IPAddress = collection.IPAddress;
                        user.MachineName = collection.MachineName;
                        user.IsUnderProbation = collection.IsUnderProbation;
                        user.IsUnderNoticePeriod = collection.IsUnderNoticePeriod;
                       // user.PermanentAddressID = Convert.ToInt32(collection.PermanentAddress);
                       // user.TemporaryAddressID =Convert.ToInt32( collection.TemporaryAddress);
                        user.OfficeSkypeId = collection.OfficeSkypeId;
                        user.Workplace = collection.WorkPlace;
                        user.extension = collection.extension;
                        user.officeno = collection.officeno;
                        user.MaritalStatus = collection.Marital;
                        user.Experience = collection.Previous;
                        user.Gender = collection.GenderID;
                        user.Workplace = collection.WorkPlace;
                        db.SaveChanges();
                        //db.SaveChanges();

                        DesignationList = (from
                                 r in db1.Master_Designation
                                           select new
                                           {
                                               DesignationID = r.DesignationID,
                                               DesignationName = r.DesignationName
                                           }).ToList();


                        GroupList = db1.DepartmentGroups.Where(dg => dg.IsActive == true).Select(g => new
                        {
                            groupid = g.GroupID,
                            groupname = g.GroupName
                        }).ToList();

                        Zone = (from p in db1.TimeZones
                                select new
                                {
                                    RegionId = p.Id,
                                    Region = p.Zone
                                }).ToList();
                        ViewBag.Groups = new SelectList(GroupList, "groupid", "groupname", collection.GroupName);
                        ViewBag.DesignationList = new SelectList(DesignationList, "DesignationID", "DesignationName", collection.DesignationName);
                      
                        ViewBag.GenderList = new SelectList(GenderNameList, "GenderID", "GenderName",collection.GenderName );
                       // ViewBag.GenderList = new SelectList(new[] { new { Text = "---Select---", Value = 0 }, new { Text = "Male", Value = 1 }, new { Text = "Female", Value = 2 } }, "Value", "Text", 0);
                        ViewBag.Region = new SelectList(Zone, "RegionId", "Region", collection.RegionId);

                        EmployeeList = (from p in db1.Users.Where(x => x.IsActive == true && x.UserID != collection.UserID)
                                        select new
                                        {
                                            ID1 = p.UserID,
                                            UserName1 = p.FirstName + " " + (p.LastName.Length > 0 ? p.LastName : "")
                                        }).ToList();


                        objuser = (from p in db1.UserReportings.Where(x => x.UserID == collection.UserID)
                                   select new DSRCManagementSystem.Models.ListReporting
                                   {
                                       Id = p.ReportingUserID

                                   }).ToList();


                        for (int i = 0; i < objuser.Count(); i++)
                        {
                            selectedAttendees.Add(Convert.ToInt32(objuser[i].Id));
                        }


                        ViewBag.ReportingPerson = new MultiSelectList(EmployeeList, "Id1", "UserName1", selectedAttendees);


                        if (db.UserSkills.Where(x => x.UserID == collection.UserID).ToList().Count == 0)
                        {
                            var objUserSkill = new UserSkill()
                            {
                                UserID = Convert.ToInt32(collection.UserID),
                                Skills = collection.Skills,
                            };
                            db.UserSkills.AddObject(objUserSkill);
                            db.SaveChanges();
                        }
                        else
                        {
                            var updateRecord = db.UserSkills.Where(x => x.UserID == collection.UserID).FirstOrDefault();
                            updateRecord.Skills = collection.Skills;
                            db.SaveChanges();

                        }

                        if (db.UserProfiles.Where(x => x.UserID == collection.UserID).ToList().Count == 0)
                        {
                            var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                            if (collection.Photo != null && collection.Photo.ContentLength > 0)
                            {
                                if ((fileExtension == ".jpg") || (fileExtension == ".jpeg") || (fileExtension == ".gif") || (fileExtension == ".png") || (fileExtension == ".jfif"))
                                {
                                    var objUserProfile = new UserProfile()
                                    {
                                        UserID = Convert.ToInt32(collection.UserID),
                                        Photo = ConvertToBytes(collection.Photo),

                                        IsDeleted = false
                                    };
                                    db.UserProfiles.AddObject(objUserProfile);

                                    var photoObj = db.UserProfiles.CreateObject();
                                    photoObj.UserID = collection.UserID;
                                    photoObj.Photo = ConvertToBytes(collection.Photo);
                                    photoObj.IsDeleted = false;

                                    db.UserProfiles.AddObject(photoObj);
                                    db.SaveChanges();

                                    if (fileExtension == ".gif" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jfif")
                                    {
                                        bool ImgExists = System.IO.File.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + objUserProfile.UserID.ToString() + ".jpg")));
                                        var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + objUserProfile.UserID.ToString() + ".jpg")));
                                        //var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")), fileName);
                                        if (ImgExists == true)
                                        {
                                            FileInfo file = new FileInfo(path);
                                            file.Delete();
                                            collection.Photo.SaveAs(path);
                                            var comimage = collection.Photo;
                                            
                                            Image image = Image.FromStream(comimage.InputStream, true, true);
                                            if (image.Width > 1024 || image.Height > 768)
                                            {
                                                WebImage img = new WebImage(collection.Photo.InputStream);
                                                if (img.Width > 1000)
                                                {
                                                    img.Resize(1000, 1000);
                                                }
                                                img.Save(path);
                                            }

                                        }

                                        else
                                        {
                                            collection.Photo.SaveAs(path);
                                        }


                                        collection.Photo.SaveAs(path);
                                        //objUserProfile.IsDeleted = false;
                                        objUserProfile.IsDeleted = true;
                                        collection.HasImage = true;

                                    }



                                }
                                else
                                {
                                    ViewBag.Photostatus = 1;
                                    ModelState.AddModelError("Photo", "Choose valid image format only.");
                                    collection.HasImage = false;
                                    return View(collection);
                                }
                            }
                        }

                        else
                        {
                            if (collection.Photo != null && collection.Photo.ContentLength > 0)
                            {
                                if ((fileExtension == ".jpg") || (fileExtension == ".jpeg") || (fileExtension == ".gif") || (fileExtension == ".png") || (fileExtension == ".jfif"))
                                {

                                    var updateProfile = db.UserProfiles.FirstOrDefault(x => x.UserID == collection.UserID && x.IsDeleted == true);

                                    var updateProfile1 = db.UserProfiles.FirstOrDefault(x => x.UserID == collection.UserID && x.IsDeleted == false);
                                    if (updateProfile != null)
                                    {
                                        updateProfile.Photo = ConvertToBytes(collection.Photo);
                                        var fileName = Path.GetFileName(collection.Photo.FileName);

                                        var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                                    }
                                    else if (updateProfile1 != null)
                                    {
                                        updateProfile1.Photo = ConvertToBytes(collection.Photo);
                                        var fileName = Path.GetFileName(collection.Photo.FileName);

                                        var httpPostedFile = HttpContext.Request.Files["UploadedImage"] as HttpPostedFileBase;
                                    }




                                    if (fileExtension == ".gif" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jfif")
                                    {
                                        if (updateProfile != null)
                                        {
                                            bool ImgExists = System.IO.File.Exists(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")));
                                            var path = Path.Combine(Server.MapPath(Url.Content("~/UsersData/Logo/Images/" + updateProfile.UserID.ToString() + ".jpg")));


                                            if (ImgExists == true)
                                            {
                                                FileInfo file = new FileInfo(path);
                                                file.Delete();
                                                collection.Photo.SaveAs(path);



                                                var comimage = collection.Photo;
                                                Image image = Image.FromStream(comimage.InputStream, true, true);
                                                if (image.Width > 1024 || image.Height > 768)
                                                {

                                                    WebImage img = new WebImage(collection.Photo.InputStream);
                                                    if (img.Width > 1000)
                                                    {
                                                        img.Resize(1000, 1000);
                                                    }
                                                    img.Save(path);

                                                }
                                                else
                                                {

                                                    collection.Photo.SaveAs(path);
                                                }
                                            }

                                            else
                                            {
                                                collection.Photo.SaveAs(path);
                                            }



                                            updateProfile.IsDeleted = true;

                                            collection.HasImage = true;

                                        }

                                    }
                                    else
                                    {

                                        ModelState.AddModelError("Photo", "Choose valid image format only.");
                                        collection.HasImage = false;
                                        return View(collection);
                                    }
                                }

                                var user1 = db1.Users.Where(x => x.UserID == collection.UserID).FirstOrDefault();
                                if (user1 != null)
                                {
                                    DateTime DOJ = Convert.ToDateTime(user1.DateOfJoin);
                                    DateTime Today = DateTime.Now;
                                    var curExp = DateTimeSpan.CompareDates(DOJ, Today);


                                    var curyears = (int)curExp.Years;

                                    var curmonths = (int)curExp.Months;

                                    string prevexp = user1.Experience ?? "0.0";
                                    string[] split = prevexp.Split('.');



                                    int peryears;
                                    int permonths;

                                    if (split.Length > 1)
                                    {
                                        peryears = Convert.ToInt32(split[0]);

                                        permonths = Convert.ToInt32(split[1]);
                                    }
                                    else
                                    {
                                        peryears = Convert.ToInt32(split[0]);

                                        permonths = 0;
                                    }


                                    int totalmonths = curmonths + permonths;

                                    int totalyears = curyears + peryears;

                                    if (totalmonths >= 12)
                                    {
                                        int additionalyears = (totalmonths) / 12;
                                        int additionalmonths = (totalmonths) % 12;

                                        totalyears += additionalyears;
                                        totalmonths = additionalmonths;
                                    }

                                    string FinalExperience = totalyears + "." + totalmonths;

                                    var dateSpan = DateTimeSpan.CompareDates(DOJ, Today);

                                    double months = 0.0;

                                    if (dateSpan.Months >= 10)
                                    {
                                        months = (double)dateSpan.Months / (double)100;
                                    }
                                    else
                                    {
                                        months = (double)dateSpan.Months / (double)10;
                                    }

                                    double Current = (double)dateSpan.Years + (double)months;

                                    double OverAll = Convert.ToDouble(collection.Previous) + Current;




                                    string Total = " ";
                                    string CurrentExp = " ";

                                    if (dateSpan.Months == 10)
                                    {

                                        Total = string.Format("{0}", OverAll);
                                        CurrentExp = string.Format("{0}", Current);
                                    }
                                    else
                                    {
                                        CurrentExp = Current.ToString();
                                        Total = OverAll.ToString();
                                    }

                                    collection.CurrentExperience = CurrentExp;
                                    collection.OverallExperience = FinalExperience;

                                    //collection.WorkPlace = null;
                                    if (collection.GenderID == 1)
                                    {
                                        ViewBag.GenderID = true;
                                    }
                                    else
                                    {
                                        ViewBag.GenderID = false;
                                    }


                                    if (collection.WorkPlace != null)
                                    {
                                        ViewBag.WorkPlaceId = true;
                                    }
                                    else
                                    {
                                        ViewBag.WorkPlaceId = false;
                                    }
                                    if (collection.Marital == 1)
                                    {
                                        ViewBag.IsMarried = true;
                                    }
                                    else
                                    {
                                        ViewBag.IsMarried = false;
                                    }
                                }
                                else
                                {
                                }



                            }
                            Session["IsRequired"] = true;
                            db.SaveChanges();
                            ViewBag.Message = "Success";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                catch (Exception Ex)
                {
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    ExceptionHandlingController.ExceptionDetails(Ex, actionName, controllerName);
                    throw new Exception(Ex.Message);
                }
            }

            //var user1 = db1.Users.Where(x => x.UserID == collection.UserID).FirstOrDefault();
            //if (user1 != null)
            //{
            //    DateTime DOJ = Convert.ToDateTime(user1.DateOfJoin);
            //    DateTime Today = DateTime.Now;
            //    var curExp = DateTimeSpan.CompareDates(DOJ, Today);


            //    var curyears = (int)curExp.Years;

            //    var curmonths = (int)curExp.Months;

            //    string prevexp = user1.Experience ?? "0.0";
            //    string[] split = prevexp.Split('.');



            //    int peryears;
            //    int permonths;

            //    if (split.Length > 1)
            //    {
            //        peryears = Convert.ToInt32(split[0]);

            //        permonths = Convert.ToInt32(split[1]);
            //    }
            //    else
            //    {
            //        peryears = Convert.ToInt32(split[0]);

            //        permonths = 0;
            //    }


            //    int totalmonths = curmonths + permonths;

            //    int totalyears = curyears + peryears;

            //    if (totalmonths >= 12)
            //    {
            //        int additionalyears = (totalmonths) / 12;
            //        int additionalmonths = (totalmonths) % 12;

            //        totalyears += additionalyears;
            //        totalmonths = additionalmonths;
            //    }

            //    string FinalExperience = totalyears + "." + totalmonths;

            //    var dateSpan = DateTimeSpan.CompareDates(DOJ, Today);

            //    double months = 0.0;

            //    if (dateSpan.Months >= 10)
            //    {
            //        months = (double)dateSpan.Months / (double)100;
            //    }
            //    else
            //    {
            //        months = (double)dateSpan.Months / (double)10;
            //    }

            //    double Current = (double)dateSpan.Years + (double)months;

            //    double OverAll = Convert.ToDouble(collection.Previous) + Current;




            //    string Total = " ";
            //    string CurrentExp = " ";

            //    if (dateSpan.Months == 10)
            //    {

            //        Total = string.Format("{0}", OverAll);
            //        CurrentExp = string.Format("{0}", Current);
            //    }
            //    else
            //    {
            //        CurrentExp = Current.ToString();
            //        Total = OverAll.ToString();
            //    }

            //    collection.CurrentExperience = CurrentExp;
            //    collection.OverallExperience = FinalExperience;

            //    collection.WorkPlace = null;

            //    if (collection.Marital == 1)
            //    {
            //        ViewBag.IsMarried = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsMarried = false;
            //    }
            //}
            //else
            //{
            //}
            Session["IsRequired"] = true;
            //db.SaveChanges();

            // added on 29/9
            ViewBag.Message = "Success";
           // return RedirectToAction("Index", "Home");
           //return View(collection);
           return RedirectToAction("Index", "Home");
        }
        
        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }
        public ActionResult ResetImage(int ID)
        {
            string fileName = Server.MapPath(Url.Content("~/Content/Template/images/profile.jpg"));
            byte[] defaultImage = System.IO.File.ReadAllBytes(fileName);
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var updateProfile = db.UserProfiles.FirstOrDefault(x => x.UserID == ID);
                updateProfile.Photo = defaultImage;
                updateProfile.IsDeleted = true;
                db.SaveChanges();
                
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public double months { get; set; }
    }
}
