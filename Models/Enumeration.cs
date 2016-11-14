using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSRCManagementSystem.Models
{
    public class MasterEnum
    {
        public enum Roles
        {
            SuperAdmin=6,
            Admin = 1,
            ProjectManager,
            Unassigned = 2,
            TeamLead = 3,
            Common = 4,
            NewRole = 5,
            HR
        };
       
        public static class ManualAttendance
        {
            public const string
            
                InTime = "09:00",
                OutTime = "18:00";
            public enum UserStatus
            {
                UnderNoticePeriod = 2,
               
            };
        }

        public enum LeaveTypes
        {
            Sick = 1,
            Casual = 2,
            Earned_Leave = 3,
            Comp_Off = 4,
            Marriage = 5,
            Maternity = 6,
            LOP = 7
            
        };
        public enum Genders
        {
            Male = 1,
            Female
        };
        public enum MaritialStatus
        {
            Married = 1,
            Single
        };
        public enum LeaveStatus
        {
            Pending = 1,
            Approved,
            Rejected,
            Cancelled
        };
        public enum RequestStatus
        {
            Pending = 1,
            Approved,
            Rejected,
            Cancelled
        };

        public enum DefaultFunctionId
        {

            DashBoard = 13,
            ProjectManagementSystem = 4,
            LeaveManagement = 11,
            Timeentry = 14,
            TimesheetManagement = 25,
            LearningandDevelopment = 22,
            Skills = 23,
            MyProfile = 1,
            ChangePassword = 12,
            Feedback = 16,
        };


        public class NewuserRole
        {
            public static string NewEmployeeRole = "Unassigned";
        };

        public class NewUnassignedUser
        {
            public static string NewEmployeeRole = " NewUnassigned";
        };
        public enum Recurring
        {
            Daily = 1,
            Weekly,
            FifteenDaysOnce,
            Monthly,
        };

        public enum PaymentType
        {
            Monthly = 1,
            Term,
            Yearly,
        };

        public enum LoginStatus
        {
            InCorrectUserName = 1,
            InCorrectPassword = 2,
            LoginSuccess = 3,
            UserNameandPassWordareInvalid = 4,
            WrongAttempt = 5,
            LoginFailed = 6,
            UserNameOrPassWordareNull = 7,

        };

        public enum Master_Tab
        {
            DashBoard = 1,
            AbsenceCalendar = 2,
            Finance = 3,
            MyTask = 4,
            RAGStatus =5,
            Training=6,
            AssetManagement=7,
            MyDashBoard=8,
            

        };
        public enum Master_Grid
        {
            	
        }
        public enum Master_Tab_Grid
        {
        MyAttendance=3,
        NewJoiningEmployee=4,
        UnInformedLeave=5,
        UnderNoticePeriod=6,
        LeaveBalanceDashboard=1,
        WorkingHours=2,
        GenderRatio=8,
        MyAttendanceRatioForLastMonth =7,
        DepartmentRatio=9,
        HoursChart=10,
        AttendanceBarchart=11,
        EmployeesUpcomingLeave = 12,
        TeamWorkingHours = 13,

      
        };
       
    }
}