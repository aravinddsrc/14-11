using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;

namespace DSRCManagementSystem.DSRCLogic
{
    public class TimeEntryHelper
    {
        static string DateFormat = "dd/MMM";
        static string LeaveFormat = "(L)";
        static string NoOutEntryFormat = "(OEM)";
        #region Team member list

        public static string GetEmpId(int userId)
        {
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                var EmpId = dbHrms.Users.FirstOrDefault(x => x.UserID == userId).EmpID;
                return EmpId;
            }
        }

        public static List<TeamMember> GetTeamMemberList(int UserId)
        {
            DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1();
            int BranchId = (int) db.Users.FirstOrDefault(o => o.UserID == UserId).BranchId;
            List<TeamMember> memberList = new List<TeamMember>();
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {

                 memberList = (from usr_ropt  in dbHrms.UserReportings
                                join
                                    usr in dbHrms.Users.Where(o=>o.IsActive!=false) on usr_ropt.UserID equals usr.UserID
                                    where usr_ropt.ReportingUserID==UserId && usr_ropt.UserID != UserId && usr.BranchId==BranchId
                                select new TeamMember()
                                {
                                    MemberId=usr.EmpID,
                                    MemberName = ((usr.FirstName + " " + usr.LastName) ?? "").Trim() // added on 9/12
                                }).Where(usr => usr.MemberName != null && usr.MemberName != "").OrderBy(o=>o.MemberName).ToList();


                 memberList.Insert(0, new TeamMember()
                 {
                     MemberId = "0",
                     MemberName = "All"
                 }
                 );
                
                
                return memberList;
            }
        }



        //public static List<TeamMember> GetTeamMemberList(int UserId)
        //{
        //    List<TeamMember> memberList = new List<TeamMember>();
        //    using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
        //    {
        //        var ProjectIds = dbHrms.UserProjects.Where(x => x.UserID == UserId).Select(x => x.ProjectID).ToList();
        //        var UserList = dbHrms.UserProjects.Where(x => ProjectIds.Contains(x.ProjectID)).Select(x => x.UserID).Distinct().ToList();
        //        memberList = dbHrms.Users.Where(x => UserList.Contains(x.UserID)).Select(x => new TeamMember()
        //             {
        //                 MemberId = x.EmpID,
        //                 MemberName = (x.FirstName + " " + (x.LastName ?? "")).Trim()
        //             }).OrderBy(x => x.MemberName).ToList();
        //        memberList.Insert(0, new TeamMember()
        //        {
        //            MemberId = "0",
        //            MemberName = "All"
        //        }
        //        );
        //        return memberList;
        //    }
        //}
        #endregion




        #region Individual work entry

        public static List<HoursWorkData> GetSingleMemberData(string EmpId, DateTime FromDate, DateTime ToDate, bool IsAscending,int BranchId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {

                    var WorkingHoursData = dbHrms.TimeManagements.Where(x => x.EmpID == EmpId && x.BranchId==BranchId && x.Date >= FromDate && x.Date <= ToDate).
                        OrderBy(x => x.Date).Select(x => new WorkData
                    {
                        Date = EntityFunctions.TruncateTime(x.Date) ?? DateTime.Now,
                        minsWorked = x.TotalTime,
                        BranchID=x.BranchId,
                        //    minsWorked = x.OutTimeMin == 0 ? null : x.OutTimeMin - x.InTimeMin,
                        IsOutEntry = (x.OutTimeMin != 0),
                        IsAbsent = (x.OutTimeMin == 0 && x.InTimeMin == 0),
                        InTime = x.InTime,
                        OutTime = x.OutTime
                    }).ToList();
                    ///var lll = dbHrms.TimeManagements.Where(x => x.EmpID.Contains(EmpId));
                    List<HoursWorkData> JsonWorkedData = new List<HoursWorkData>();
                    if (IsAscending)
                    {
                        JsonWorkedData = WorkingHoursData.Select(x => new DSRCManagementSystem.Models.HoursWorkData()
                          {
                              Date = (x.IsAbsent) ? x.Date.ToString(DateFormat) + LeaveFormat : (!x.IsOutEntry) ? x.Date.ToString(DateFormat) + NoOutEntryFormat : x.Date.ToString(DateFormat),
                              //hoursWorked = Math.Round((double)((x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Round(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours, 0) >= 5 ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours) - 1) : Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)), 2),
                              hoursWorked = Math.Round((double)((x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)), 2),
                              Day = x.Date.ToString("dddd"),
                              InTime = x.InTime,
                              OutTime = x.OutTime
                          }).ToList();
                    }
                    else
                    {
                        JsonWorkedData = WorkingHoursData.Select(x => new DSRCManagementSystem.Models.HoursWorkData()
                        {
                            Date = (x.IsAbsent) ? x.Date.ToString(DateFormat) + LeaveFormat : (!x.IsOutEntry) ? x.Date.ToString(DateFormat) + NoOutEntryFormat : x.Date.ToString(DateFormat),
                            hoursWorked = Math.Round((double)((x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)), 2),
                            Day = x.Date.ToString("dddd"),
                            InTime = x.InTime,
                            OutTime = x.OutTime
                        }).ToList();
                    }
                    return JsonWorkedData;
                }
            }
            catch
            {
                return new List<HoursWorkData>();
            }
        }

        #endregion

        #region Team work entry

        public static List<HoursWorkData> GetTeamMemberData1(List<TeamMember> teamMembers, DateTime? Date, int BranchId)
        {
           
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    if (Date == null)
                    {
                        var SatSunList = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };
                        var LastThreeDates =
                            dbHrms.TimeManagements.OrderByDescending(x => x.Date).Where(x => !dbHrms.Master_holiday.Select(y =>
                                EntityFunctions.TruncateTime(y.Date)).Contains(x.Date)).Take(3);
                        foreach (var data in LastThreeDates)
                        {
                            if (!SatSunList.Contains(data.Date.DayOfWeek))
                            {
                                Date = data.Date;
                                break;
                            }
                        }

                      
                    }
                    var filter = teamMembers.Select(y => y.MemberId);

                    //teamMembers.Select(y => y.MemberId).Contains(x.EmpID) &&
                    //filter.Any() && filter.Contains(x.EmpID) &&
                    var WorkingHoursData = dbHrms.TimeManagements.Where(x => filter.Contains(x.EmpID) && x.Date == Date)
                        .Select(x => new WorkData
                        {
                            // empName = teamMembers.FirstOrDefault(z => z.MemberId == x.EmpID).MemberName,
                            EmpId = x.EmpID,
                            Date = EntityFunctions.TruncateTime(x.Date) ?? DateTime.Now,
                            minsWorked = x.TotalTime,
                            //  minsWorked = x.OutTimeMin == 0 ? null : x.OutTimeMin - x.InTimeMin,
                            IsOutEntry = (x.OutTimeMin != 0),
                            IsAbsent = (x.OutTimeMin == 0 && x.InTimeMin == 0)
                        }).ToList();
                }

                return new List<HoursWorkData>();
            }
        


        public static List<HoursWorkData> GetTeamMemberData(List<TeamMember> teamMembers, DateTime? Date, bool IsAscending,int BranchId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    if (Date == null)
                    {
                        var SatSunList = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };
                        var LastThreeDates =
                            dbHrms.TimeManagements.OrderByDescending(x => x.Date).Where(x => !dbHrms.Master_holiday.Select(y =>
                                EntityFunctions.TruncateTime(y.Date)).Contains(x.Date)).Take(3);
                        foreach (var data in LastThreeDates)
                        {
                            if (!SatSunList.Contains(data.Date.DayOfWeek))
                            {
                                Date = data.Date;
                                break;
                            }
                        }
                    }
                    var filter = teamMembers.Select(y => y.MemberId);

                    //teamMembers.Select(y => y.MemberId).Contains(x.EmpID) &&
                    //filter.Any() && filter.Contains(x.EmpID) &&
                    var WorkingHoursData = dbHrms.TimeManagements.Where(x => filter.Contains(x.EmpID) && x.BranchId == BranchId && x.Date == Date)
                        .Select(x => new WorkData
                        {
                            // empName = teamMembers.FirstOrDefault(z => z.MemberId == x.EmpID).MemberName,
                            EmpId = x.EmpID,
                            Date = EntityFunctions.TruncateTime(x.Date) ?? DateTime.Now,
                            BranchID = x.BranchId,
                            minsWorked = x.TotalTime,
                            //  minsWorked = x.OutTimeMin == 0 ? null : x.OutTimeMin - x.InTimeMin,
                            IsOutEntry = (x.OutTimeMin != 0),
                            IsAbsent = (x.OutTimeMin == 0 && x.InTimeMin == 0)
                        }).ToList();



                    List<HoursWorkData> JsonWorkedData = new List<HoursWorkData>();
                    if (IsAscending)
                        JsonWorkedData = WorkingHoursData.Select(x => new DSRCManagementSystem.Models.HoursWorkData()
                         {

                             empName = teamMembers.FirstOrDefault(z => z.MemberId == x.EmpId).MemberName,
                             Date = (x.IsAbsent) ? x.Date.ToString(DateFormat) + LeaveFormat : (!x.IsOutEntry) ? x.Date.ToString(DateFormat) + NoOutEntryFormat : x.Date.ToString(DateFormat),
                            // hoursWorked = (x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Round(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours, 0) >= 5 ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours) - 1) : Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)
                             hoursWorked = (x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)
                         }).OrderBy(i => i.hoursWorked).ToList();
                    else
                        JsonWorkedData = WorkingHoursData.Select(x => new DSRCManagementSystem.Models.HoursWorkData()
                        {
                            empName = teamMembers.FirstOrDefault(z => z.MemberId == x.EmpId).MemberName,
                            Date = (x.IsAbsent) ? x.Date.ToString(DateFormat) + LeaveFormat : (!x.IsOutEntry) ? x.Date.ToString(DateFormat) + NoOutEntryFormat : x.Date.ToString(DateFormat),
                            //hoursWorked = (x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Round(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours, 0) >= 5 ? Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours) - 1 : Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)
                            hoursWorked = (x.minsWorked == null ? 0 : x.IsOutEntry ? (Math.Floor(TimeSpan.FromMinutes(x.minsWorked ?? 0).TotalHours)) : 0) + ((x.minsWorked % 60) / 100)
                        }).OrderByDescending(i => i.hoursWorked).ToList();

                    return JsonWorkedData;
                }
            }
            catch
            {
                return new List<HoursWorkData>();
            }
        }

        public static void GetTeamTotalTimeEntry(int projectId, bool IsAscending)
        {

        }
        #endregion
    }
}