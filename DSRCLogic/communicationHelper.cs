using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DSRCManagementSystem.Models;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;


namespace DSRCManagementSystem.DSRCLogic
{
    public class communicationHelper
    {
        public static List<DSRCManagementSystem.Models.MessageType> GetMessageTypes()
        {
            List<DSRCManagementSystem.Models.MessageType> _messageType = new List<DSRCManagementSystem.Models.MessageType>();
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                _messageType = dbHrms.Master_MessageType.Select(x => new DSRCManagementSystem.Models.MessageType()
                {
                    description = x.Description,
                    typeId = x.MessageTypeId,
                    isSelected = (x.MessageTypeId == 2)
                }).ToList();
            }
            return _messageType;
        }

        public static List<DSRCManagementSystem.Models.DepartmentData> GetDepartments()
        {
            List<DSRCManagementSystem.Models.DepartmentData> department = new List<DSRCManagementSystem.Models.DepartmentData>();
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                int userId = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

                int BranchId = (int)dbHrms.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                department = dbHrms.Departments.Where(x => x.BranchID == BranchId).Distinct().Select(x => new DepartmentData()
                {
                    departmentId = x.DepartmentId,
                    departmentName = x.DepartmentName
                }).ToList();
            }
            return department;
        }
        //public static List<DSRCManagementSystem.Models.GroupData> GetGroups()
        //{
        //    List<DSRCManagementSystem.Models.GroupData> group = new List<DSRCManagementSystem.Models.GroupData>();
        //    using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
        //    {
        //        int userId = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

        //        int gpId = (int)dbHrms.Users.FirstOrDefault(o => o.UserID == userId).DepartmentId;

        //        group = dbHrms.DepartmentGroups.Where(x => x.GroupID == gpId).Distinct().Select(x => new GroupData()
        //        {
        //            GroupId = x.GroupID,
        //            GroupName = x.GroupName
        //        }).ToList();
        //    }
        //    return group;
        //}

        public static List<UserList> GetUsers()
        {
            List<DSRCManagementSystem.Models.UserList> _users = new List<DSRCManagementSystem.Models.UserList>();
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                int userId = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

                int BranchId = (int)dbHrms.Users.FirstOrDefault(o => o.UserID == userId).BranchId;

                _users = dbHrms.Users.Where(x => x.IsActive == true && x.BranchId == BranchId).Select(x => new UserList
                {
                    userId = (int)x.UserID,
                    userName = (x.FirstName + " " + (x.LastName ?? "")).Trim(),
                    departmentId = x.DepartmentId ?? 0
                }).ToList();
            }
            return _users;
        }
        
        public static void SendMessage(communicationModel commModel, DateTime validFrom, DateTime validTo,int UserID)
        {
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                communicationMessage commMessage = new communicationMessage();
                commMessage.MessageText = commModel.Message.Replace(System.Environment.NewLine, "<br />");
                commMessage.UserID = UserID;
                commMessage.ShowComments = commModel.showComments;
                commMessage.ShowToAll = commModel.showToAll;
                commMessage.Valid_From = validFrom /*commModel.dateFrom*/;
                commMessage.Valid_To = validTo /*commModel.dateTo*/;
                commMessage.MessageType = commModel.messageTypeId;
                dbHrms.AddTocommunicationMessages(commMessage);
                dbHrms.SaveChanges();

                //commModel.selectedMembers.ForEach(x =>
                commModel.UserList.ForEach(x =>
                {
                    UserMessage userMessage = new UserMessage();
                    userMessage.EmployeeId = Convert.ToInt32(x);
                    userMessage.MessageId = commMessage.MessageId;
                    dbHrms.AddToUserMessages(userMessage);
                    dbHrms.SaveChanges();
                });
            }            
        }

        public static void DeleteDuplicateTimeEntry(int BranchID)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var BranchParamID = new SqlParameter("@BranchID", SqlDbType.Int);
                    BranchParamID.Value = BranchID;                    
                    dbHrms.ExecuteStoreCommand("uspDeleteDuplicateTimeEntry @BranchID", BranchParamID);
                }
            }
            catch(Exception e)
            {
                
            }
        }

        public static List<WorkData> GetTimeWorked(int userId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var UserDetails=dbHrms.Users.FirstOrDefault(x => x.UserID == userId);
                    var EmpId = UserDetails.EmpID;
                    var BranchId = UserDetails.BranchId;

                    DateTime dt1 = DateTime.Parse(TimeSpan.FromMinutes(330).ToString());
                    DateTime dt2 = DateTime.Parse(TimeSpan.FromMinutes(330).ToString());

                    var WorkingHoursData = dbHrms.TimeManagements.Where(x => x.EmpID == EmpId && x.BranchId==BranchId).OrderByDescending(x => x.Date).Take(10).Select(x => new WorkData
                    {                        
                        Date = EntityFunctions.TruncateTime(x.Date) ?? DateTime.Now,
                        //minsWorked = x.OutTimeMin == 0 ? null : x.OutTimeMin - x.InTimeMin,
                        minsWorked = x.TotalTime == 0 ? null : x.TotalTime,
                        //Convert.ToInt32(Convert.ToDateTime(InTime).TimeOfDay.TotalMinutes);
                        IsOutEntry = (x.OutTimeMin != 0),
                        IsAbsent = (x.OutTimeMin == 0 && x.InTimeMin == 0),
                        InTime = x.InTime,
                        OutTime = x.OutTime,
                        InTmieMin=x.InTimeMin
                    }).ToList();
                    return WorkingHoursData;
                }
            }
            catch
            {
                return new List<WorkData>();
            }
        }

        public static List<MessageUpdates> GetMessages(int userId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    DateTime currentDate = DateTime.Now;

                    //EntityFunctions.TruncateTime(x.DateTimeStart) == currentDate.Date
                    var Messages = dbHrms.UserMessages.Where(user => user.EmployeeId == userId && user.Status == null).Join(dbHrms.communicationMessages.
                        Where(x => (EntityFunctions.TruncateTime(x.Valid_From) <= currentDate.Date && EntityFunctions.TruncateTime(x.Valid_To) >= currentDate.Date)),
                        x => x.MessageId, y => y.MessageId, (x, y) =>
                            new MessageUpdates()
                            {
                                messageId = y.MessageId,
                                messageText = y.MessageText.Replace("<br />", "\n"),
                                messageType = (commMessageType)y.MessageType,
                                showComments = y.ShowComments,
                                Comments=x.Comments??""
                             
                            }
                        ).ToList();

                    return Messages;
                }
            }
            catch
            {
                return new List<MessageUpdates>();
            }

        }

        public static void RemoveExpiredMessage()
        {
            using (DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1())
            {
                List<int> MessageIdLIst = dbhrms.communicationMessages.Where(o => EntityFunctions.TruncateTime(o.Valid_To) < EntityFunctions.TruncateTime (DateTime.Now)).Select(o => o.MessageId).ToList();
                foreach (int id in MessageIdLIst)
                {
                    List<int> usermessageIdLIst = dbhrms.UserMessages.Where(o => o.MessageId == id).Select(o => o.CommunicationId).ToList();
                    foreach (int msgid in usermessageIdLIst)
                    {
                        var obj = dbhrms.UserMessages.Where(o => o.CommunicationId == msgid).Select(o => o).FirstOrDefault();
                        dbhrms.UserMessages.DeleteObject(obj);
                        dbhrms.SaveChanges();
                    }
                    var communicationobj = dbhrms.communicationMessages.Where(o => o.MessageId == id).Select(o => o).FirstOrDefault();
                    dbhrms.communicationMessages.DeleteObject(communicationobj);
                    dbhrms.SaveChanges();
                }

            }
        }

        public static void DeleteMessage(int MessageID)
        {
            using (DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1())
            {
               // List<int> MessageIdLIst = dbhrms.communicationMessages.Where(o => EntityFunctions.TruncateTime(o.Valid_To) < EntityFunctions.TruncateTime(DateTime.Now)).Select(o => o.MessageId).ToList();
                //foreach (int id in MessageIdLIst)
                //{
                    List<int> usermessageIdLIst = dbhrms.UserMessages.Where(o => o.MessageId == MessageID).Select(o => o.CommunicationId).ToList();
                    foreach (int msgid in usermessageIdLIst)
                    {
                        var obj = dbhrms.UserMessages.Where(o => o.CommunicationId == msgid).Select(o => o).FirstOrDefault();
                        dbhrms.UserMessages.DeleteObject(obj);
                        dbhrms.SaveChanges();
                    }
                    var communicationobj = dbhrms.communicationMessages.Where(o => o.MessageId == MessageID).Select(o => o).FirstOrDefault();
                    dbhrms.communicationMessages.DeleteObject(communicationobj);
                    dbhrms.SaveChanges();
                //}

            }
        }

        public static List<ViewMessage> ViewMessage(int UserID)
        {
            dynamic messages;
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {

              //  var id = (from message in dbHrms.communicationMessages.Where(o=>o.UserID==UserID) select message.MessageId).FirstOrDefault() ;
                messages = (from message in dbHrms.communicationMessages.Where(o => o.UserID == UserID)
                            join
                                usermessage in dbHrms.UserMessages.Where(o => o.Comments != null || o.Comments == null) on message.MessageId equals usermessage.MessageId
                            join
                                 user in dbHrms.Users on usermessage.EmployeeId equals user.UserID
                            select new ViewMessage
                            {
                                UserId = user.UserID,
                                MessageId = message.MessageId,
                                UserName = user.FirstName + " " + user.LastName,
                                Message = message.MessageText.Replace("<br />", "\n"),
                                Comment = usermessage.Comments,
                                MessageinitiatesDate = EntityFunctions.TruncateTime(message.Valid_From),
                                MessageValidUpto = EntityFunctions.TruncateTime(message.Valid_To),
                                Isreplyable = message.ShowComments,
                                IsYesOrNo = usermessage.IsYesPressed,
                                MessageType = message.MessageType
                                //}) .ToList();
                            }).OrderByDescending(o => o.MessageinitiatesDate).ToList();
            }
            return messages;

        }

        public static List<ViewMessage> ViewMessage(int UserId,int messageid)
        {
            dynamic messages;
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                messages = (from message in dbHrms.communicationMessages.Where(o=>o.UserID==UserId)
                            join
                                usermessage in dbHrms.UserMessages.Where(o => o.Comments != null || o.Comments == null) on message.MessageId equals usermessage.MessageId
                            join
                                 user in dbHrms.Users on usermessage.EmployeeId equals user.UserID
                            select new ViewMessage
                            {
                                UserId = user.UserID,
                                MessageId = message.MessageId,
                                UserName = user.FirstName+" "+user.LastName,
                                Message = message.MessageText.Replace("<br />", "\n"),
                                Comment = usermessage.Comments,
                                MessageinitiatesDate = EntityFunctions.TruncateTime(message.Valid_From),
                                MessageValidUpto = EntityFunctions.TruncateTime(message.Valid_To),
                                Isreplyable = message.ShowComments,
                                IsYesOrNo = usermessage.IsYesPressed,
                                MessageType = message.MessageType
                            //}).Where(o => o.MessageId == messageid).ToList();
                            }).OrderByDescending(o => o.MessageinitiatesDate).ToList();
            }
            return messages;

        }

        public static List<ViewMessage> GetMessageLIst()
        {
            List<ViewMessage> messages;
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {

                messages = (from message in dbHrms.communicationMessages
                            join
                                usermessage in dbHrms.UserMessages on message.MessageId equals usermessage.MessageId
                            join
                                 user in dbHrms.Users on usermessage.EmployeeId equals user.UserID
                            select new ViewMessage
                            {
                                UserId = user.UserID,
                                MessageId = message.MessageId,
                                UserName = user.FirstName+" "+user.LastName,
                                Message = message.MessageText.Replace("<br />", "\n"),
                                Comment = usermessage.Comments,
                                MessageinitiatesDate = EntityFunctions.TruncateTime(message.Valid_From),
                                MessageValidUpto = EntityFunctions.TruncateTime(message.Valid_To),
                                Isreplyable = message.ShowComments,
                                IsYesOrNo = usermessage.IsYesPressed,
                                MessageType = message.MessageType
                            //}).ToList();
                            }).OrderByDescending(o => o.MessageinitiatesDate).ToList();


                return messages;
            }
        }
        
        public static List<ViewMessage> GetNewMessageOnly()
       // public static ViewMessage GetNewMessageOnly()    
    {
            dynamic messages;
            using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
            {
                messages = (from message in dbHrms.communicationMessages
                           join
                               usermessage in dbHrms.UserMessages on message.MessageId equals usermessage.MessageId
                           join
                                user in dbHrms.Users on usermessage.EmployeeId equals user.UserID
                              //  where usermessage.time>= EntityFunctions.AddSeconds(DateTime.Now,-3)
                           select new ViewMessage
                           {
                               UserId = user.UserID,
                               UserName = user.FirstName+" "+user.LastName,
                               Message = message.MessageText.Replace("<br />", "\n"),
                               Comment = usermessage.Comments,
                               MessageinitiatesDate = EntityFunctions.TruncateTime( message.Valid_From).Value,
                               MessageValidUpto =EntityFunctions.TruncateTime( message.Valid_To),
                               Isreplyable = message.ShowComments,
                               IsYesOrNo = usermessage.IsYesPressed,
                               MessageType = message.MessageType
                           //}).ToList();
                           }).OrderByDescending(o => o.MessageinitiatesDate).ToList();
            }
            return messages;
        }
        public static void replyToMessage(int MessageId, string Comments, bool status,bool? opinion,int UserId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var Message = dbHrms.UserMessages.Where(x => x.MessageId == MessageId && x.EmployeeId == UserId).FirstOrDefault();
                    //var Message = dbHrms.UserMessages.FirstOrDefault(x => x.MessageId == MessageId);
                    if (Message != null)
                    {
                        Message.Comments = Comments;
                        Message.Status = status;
                        Message.IsYesPressed = opinion;
                        Message.time = DateTime.Now;
                        dbHrms.SaveChanges();
                    }


                }

            }
            catch
            {

            }
        }

        public static string GetMailId(int userId)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var MailId = dbHrms.Users.FirstOrDefault(x => x.UserID == userId);
                    if (MailId != null)
                    {
                        return MailId.EmailAddress ?? string.Empty;
                    }


                }

            }
            catch
            {

            }
            return string.Empty;
        }
       



        public static List<string> GetMailIdFromUsersList(List<int> userIds)
        {
            try
            {
                using (DSRCManagementSystemEntities1 dbHrms = new DSRCManagementSystemEntities1())
                {
                    var MailIds = dbHrms.Users.Where(x => userIds.Contains(x.UserID) && x.EmailAddress != null).Select(x => x.EmailAddress);
                    if (MailIds != null)
                    {
                        return MailIds.ToList();
                    }


                }

            }
            catch
            {
                
            }
            return null;
        }

        public static List<LeaveDetails> LeaveDetails(int userId, int leaveTypeId)
        {
            using(DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var acadamicYear = db.CalendarYears.FirstOrDefault();
                var year = DateTime.Now.Month <= acadamicYear.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var calendar = new Calendar().GetCalendarDetails(year, acadamicYear.StartingMonth ?? 1, acadamicYear.EndingMonth ?? 12);
                DateTime startDate, endDate;
                startDate = calendar.StartDate;
                endDate = calendar.EndDate;

                List<LeaveDetails> Details = null;

                //| (item.StartDateTime <= endDateTime && item.EndDateTime >= endDateTime) || (item.StartDateTime >= startDateTime && item.EndDateTime <= endDateTime)
                if (leaveTypeId == 5 || leaveTypeId == 6)
                {
                    Details = db.LeaveRequests.Where(item => item.UserId == userId && item.LeaveTypeId == leaveTypeId && item.LeaveStatusId == 2).Select(x => new LeaveDetails
                    {
                        StartDate = x.StartDateTime,
                        EndDate = x.EndDateTime,
                        Detail = x.Details,
                        WorkedDate = x.WorkedDate,
                        Leavetypeid = leaveTypeId,
                        LeaveDays=x.LeaveDays,
                        Casual = x.Casual != null ? x.Casual : 0,
                        Earned = x.Earned != null ? x.Earned : 0,
                        Sick = x.Sick != null ? x.Sick : 0,
                        LOP = x.LOP != null ? x.LOP : 0
                    }).ToList();
                }
                else
                {
                    Details = db.LeaveRequests.Where(item => item.UserId == userId && item.LeaveTypeId == leaveTypeId && item.LeaveStatusId == 2 && 
                        ((item.StartDateTime <= startDate && item.EndDateTime >= startDate) || (item.StartDateTime <= endDate && item.EndDateTime >= endDate) || (item.StartDateTime >= startDate && item.EndDateTime <= endDate))).Select(x => new LeaveDetails
                    {
                        StartDate = x.StartDateTime,
                        EndDate = x.EndDateTime,
                        Detail = x.Details,
                        WorkedDate = x.WorkedDate,
                        Leavetypeid = leaveTypeId,
                        LeaveDays = x.LeaveDays,
                        Casual = x.Casual != null ? x.Casual : 0,
                        Earned = x.Earned != null ? x.Earned : 0,
                        Sick = x.Sick != null ? x.Sick : 0,
                        LOP = x.LOP != null ? x.LOP : 0
                    }).ToList();
                }


               return Details;
            }
        }

        public static List<LeaveDetails> LOPLeaveDetails(int userId, bool Monthly)
        {
            using (DSRCManagementSystemEntities1 db = new DSRCManagementSystemEntities1())
            {
                var acadamicYear = db.CalendarYears.FirstOrDefault();
                var month=DateTime.Now.Month;
                var year = DateTime.Now.Month <= acadamicYear.EndingMonth ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var calendar = new Calendar().GetCalendarDetails(year, acadamicYear.StartingMonth ?? 1, acadamicYear.EndingMonth ?? 12);
                DateTime startDate, endDate;
                startDate = calendar.StartDate;
                endDate = calendar.EndDate;

                List<LeaveDetails> Details = null;

                if (Monthly)
                {
                    Details = db.LeaveRequests.Where(item => item.UserId == userId  && 
                                                             item.LeaveStatusId == 2 &&
                                                             item.StartDateTime.Value.Month == month &&
                                                             ((item.StartDateTime <= startDate && item.EndDateTime >= startDate) || (item.StartDateTime <= endDate && item.EndDateTime >= endDate) || (item.StartDateTime >= startDate && item.EndDateTime <= endDate)) && 
                                                             item.LOP>0).Select(x => new LeaveDetails
                    {
                        StartDate = x.StartDateTime,
                        EndDate = x.EndDateTime,
                        Detail = x.Details,
                        WorkedDate = x.WorkedDate,
                        Leavetypeid = (int)x.LeaveTypeId,
                        LeaveDays = x.LeaveDays,
                        Casual = x.Casual!=null? x.Casual : 0,
                        Earned = x.Earned != null ? x.Earned : 0,
                        Sick = x.Sick != null ? x.Sick : 0,
                        LOP = x.LOP != null ? x.LOP : 0
                    }).ToList();
                }
                else
                {
                    Details = db.LeaveRequests.Where(item => item.UserId == userId &&
                                                             item.LeaveStatusId == 2 &&
                                                             ((item.StartDateTime <= startDate && item.EndDateTime >= startDate) || (item.StartDateTime <= endDate && item.EndDateTime >= endDate) || (item.StartDateTime >= startDate && item.EndDateTime <= endDate)) &&
                                                             item.LOP > 0).Select(x => new LeaveDetails
                                                             {
                                                                 StartDate = x.StartDateTime,
                                                                 EndDate = x.EndDateTime,
                                                                 Detail = x.Details,
                                                                 WorkedDate = x.WorkedDate,
                                                                 Leavetypeid = (int)x.LeaveTypeId,
                                                                 LeaveDays = x.LeaveDays,
                                                                 Casual = x.Casual != null ? x.Casual : 0,
                                                                 Earned = x.Earned != null ? x.Earned : 0,
                                                                 Sick = x.Sick != null ? x.Sick : 0,
                                                                 LOP = x.LOP != null ? x.LOP : 0
                                                             }).ToList();

                }       
                return Details;
            }
        }

        public static void RemoveResignedEmployees()
        {
            using (DSRCManagementSystemEntities1 dbhrms = new DSRCManagementSystemEntities1())
            {
                var resignedEmployees = dbhrms.Users.Where(o => (EntityFunctions.TruncateTime(o.LastWorkingDate) < EntityFunctions.TruncateTime(DateTime.Now)) && o.IsActive == true).Select(o => o.UserID).ToList();
                foreach (int Id in resignedEmployees)
                {
                    //var DeleteUserRoles = dbhrms.UserRoles.FirstOrDefault(x => x.UserID == Id);
                    //var DeleteUserProjects = dbhrms.UserProjects.FirstOrDefault(x => x.UserID == Id);
                    //var DeleteUserProfile = dbhrms.UserProfiles.FirstOrDefault(x => x.UserID == Id);
                    //var DeleteUserSkills = dbhrms.UserSkills.FirstOrDefault(x => x.UserID == Id);
                    //var DeleteLeaveReq = (from lq in dbhrms.LeaveRequests
                    //                      where lq.UserId == Id || lq.ReportingTo == Id || lq.ProcessedBy == Id
                    //                      select lq);
                    //var DeleteUserReporting = (from ur in dbhrms.UserReportings
                    //                           where ur.UserID == Id
                    //                           select ur);
                    //var DeleteUsers = dbhrms.Users.FirstOrDefault(x => x.UserID == Id);
                    //if (Id != 0)
                    //{
                    //    if (DeleteUserRoles != null)
                    //    {
                    //        dbhrms.UserRoles.DeleteObject(DeleteUserRoles);
                    //        dbhrms.SaveChanges();
                    //    }
                    //    if (DeleteUserProfile != null)
                    //    {
                    //        dbhrms.UserProfiles.DeleteObject(DeleteUserProfile);
                    //        db].SaveChanges();
                    //    }
                    //    if (DeleteUserProjects != null)
                    //    {
                    //        dbhrms.UserProjects.DeleteObject(DeleteUserProjects);
                    //        dbhrms.SaveChanges();
                    //    }
                    //    if (DeleteUserSkills != null)
                    //    {
                    //        dbhrms.UserSkills.DeleteObject(DeleteUserSkills);
                    //        dbhrms.SaveChanges();
                    //    }
                    //    if (DeleteLeaveReq != null)
                    //    {
                    //        foreach (var item in DeleteLeaveReq.ToList())
                    //        {
                    //            dbhrms.LeaveRequests.DeleteObject(item);
                    //            dbhrms.SaveChanges();
                    //        }
                    //    }
                    //    if (DeleteUserReporting != null)
                    //    {
                    //        foreach (var item in DeleteUserReporting.ToList())
                    //        {
                    //            dbhrms.UserReportings.DeleteObject(item);
                    //            dbhrms.SaveChanges();
                    //        }
                    //    }
                    //    if (DeleteUsers != null)
                    //    {
                    //        dbhrms.Users.DeleteObject(DeleteUsers);
                    //        dbhrms.SaveChanges();
                    //    }
                    //}

                    var resignedUser = dbhrms.Users.FirstOrDefault(x => x.UserID == Id);
                    resignedUser.IsActive = false;
                    dbhrms.SaveChanges();
                }
            }
        }
    }
}
