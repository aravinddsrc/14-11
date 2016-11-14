using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;


namespace DSRCManagementSystem.Models
{



    public class communicationModel
    {
        public string Message { get; set; }
        public int gID { get; set; }
        public int? dep { get; set; }

        public int GroupId { get; set; }
        public String GroupName { get; set; }
        public List<MailInvitesModel> selGroup { get; set; }
        public List<MailInvitesModel> Users1 { get; set; }
        public List<MailInvitesModel> Users2 { get; set; }


        public List<MailInvitesModel> Groups { get; set; }

        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
        //       ApplyFormatInEditMode = true)]

        public string dateFrom { get; set; }

        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}",
        //       ApplyFormatInEditMode = true)]
        //[DataType(DataType.Date)]

        public string dateTo { get; set; }

        public List<UserList> users { get; set; }

        public List<MessageType> _messageType = new List<MessageType>();
        public List<MessageType> messageType
        {
            get;
            set;
        }

        //public List<string> selectedUsers { get; set; }

        public List<string> selectedMembers { get; set; }


        public int messageTypeId { get; set; }


        public bool showToAll { get; set; }


        public bool showComments { get; set; }

        public List<DepartmentData> departments { get; set; }

        public string ErrorSuccessMessage { get; set; }

        public List<string> Department { get; set; }

        //public List<string> Group { get; set; }

        public List<string> UserList { get; set; }



    }
    public class GroupModel
    {
        public int gID { get; set; }

        public string gName { get; set; }
    }


    public class Inbox
    {
        public int id { get; set; }
        public string header { get; set; }
        public string subject { get; set; }
        public string From { get; set; }
        public string to { get; set; }
        public string attachement { get; set; }
        public string message { get; set; }
        public DateTime? senton { get; set; }
        public string UserName { get; set; }
        public string Attachment { get; set; }
        public int InboxCount { get; set; }
        public int SentBoxCount { get; set; }
        public int Touserid { get; set; }
        public string ToEmail { get; set; }
        public int Fromusereid { get; set; }
        public bool? Checkbox { get; set; }
    }


    public class sentbox
    {
        public int id { get; set; }
        public string header { get; set; }
        public string subject { get; set; }
        public string From { get; set; }
        public string to { get; set; }
        public string attachement { get; set; }
        public string message { get; set; }
        public DateTime? senton { get; set; }
        public string UserName { get; set; }
        public string Attachment { get; set; }
        public int InboxCount { get; set; }
        public int SentBoxCount { get; set; }
    }


    public class MailInvitesModel
    {

        public string Message { get; set; }

        public bool attachment { get; set; }


        public string Attachmentpath { get; set; }

        //public string Departmentsid { get; set; }

        //public string Groupsid { get; set; }
        //public string Department { get; set; }

        public List<string> Department { get; set; }

        public List<MailInvitesModel> Groups { get; set; }
        // 1public List<MailInvitesModel> Groupfil { get; set; }

        // public List<string> Group { get; set; }
        public int Userid { get; set; }
        public string UserName { get; set; }

        public int GroupId { get; set; }
        public String GroupName { get; set; }

        //public IEnumerable<SelectListItem> Groups { get; set; }

        // 1public List<string> Groups { get; set; }

        public List<UserList> users { get; set; }
        public List<MailInvitesModel> selGroup { get; set; }
        public List<MailInvitesModel> Users1 { get; set; }
        public List<MailInvitesModel> Users2 { get; set; }


        public string subject { get; set; }

        //public IEnumerable<SelectListItem> Groupss { get; set; }
        //public List<string> Groupss { get; set; }


        public int gID { get; set; }
        public int? dep { get; set; }
        public string depName { get; set; }

        public string gName { get; set; }

        //1public List<string> selectedMembers { get; set; }

        public List<DepartmentData> departments { get; set; }

        public bool defaultSignature { get; set; }

        public List<HttpPostedFileBase> fileList { get; set; }

        public int MailType { get; set; }

        public string ErrorSuccessMessage { get; set; }

        public bool IsActive { get; set; }
        //public string EmailIds { get; set; }

        public List<string> EmailIds { get; set; }

        public List<string> UserList { get; set; }

        //1public List<string> SelectedGroups { get; set; }

        //1public List<string> SelectedDep { get; set; }

        //1 public List<string> SelectedUsers { get; set; }
    }

    public class SendEmail
    {
        public string FromEmail { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public string Sign { get; set; }
        public int ID { get; set; }
        public Boolean isActive { get; set; }
        public DateTime? SentOn { get; set; }
        public string MailAdd { get; set; }

    }
    public class SendMailList
    {
        public string MailId { get; set; }
        public List<string> AllMailList { get; set; }
        public string EmailIds { get; set; }
        public string MailAddress { get; set; }
        public string MailList { get; set; }
        public string Mail { get; set; }
        public int Count { get; set; }
        public string selectedAddress { get; set; }
    }


    public class MailList
    {
        public List<string> EmailIds { get; set; }
        public string Mails { get; set; }
    }
    public class MailId
    {
        public string MailAdd { get; set; }
        public string Message { get; set; }
        public List<string> UserList { get; set; }
        public string UserLists { get; set; }
        public bool flag { get; set; }
        public List<string> sentUserList { get; set; }
        public List<string> UnsentUserList { get; set; }

        public string attachment { get; set; }
    }
    public class StatusMail
    {
        public string UserID { get; set; }
        public string Uname { get; set; }
    }
    public class checks
    {
        public List<StatusMail> UserID { get; set; }

    }
}
