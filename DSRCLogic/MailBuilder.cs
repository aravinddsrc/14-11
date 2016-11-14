using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DSRCManagementSystem.Models;
using System.Web.Configuration;


namespace DSRCManagementSystem.DSRCLogic
{
    public class MailBuilder
    {
        DsrcMailSystem.MailSender AppValue = new DsrcMailSystem.MailSender();
        public static string ServerName;
        public void GetServerName()
        {
            ServerName = AppValue.GetFromMailAddress("ServerName");
        }

        public static string LeaveRequest(LeaveModel model)
        {
            string MessageBody;
           
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Leave Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + model.ReportingPersonName + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Leave request has been sumbitted for approval by " + model.UserName + " " + model.LastName+ @".
                                        </p>
                                             <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                           Type of Leave&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                            <label style='color: Black;'>
                                                " + model.LeaveTypeName + @"
                                            </label>
                                        </p>";
            if (model.LeaveType == 4)
            {
                MessageBody += @" <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            Worked Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                            <label style='color: Black;'>
                                                " + model.WorkedDate1 + @"
                                            </label>
                                        </p>";
            }
                      MessageBody += @" <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            Start Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                            <label style='color: Black;'>
                                                " + model.StartDateTime.ToString("dd-MMM-yyyy") + @"
                                            </label>
                                        </p>
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            End Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                            <label style='color: Black;'>
                                                " + model.EndDateTime.ToString("dd-MMM-yyyy") + @"
                                            </label>
                                        </p>
                                           
                                         <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                           Leave Days&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                            <label style='color: Black;'>

                                                " + model.totalLeaveDays + @"

                                            </label>
                                        </p>";

            MessageBody += @"<p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                         Sick  Leave Available &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color: Black;'>&nbsp;" + model.Balance[0].RemainingDays + @"</label>
                                        </p>
                                         <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                         Casual  Leave Available &nbsp;&nbsp;:<label style='color: Black;'>&nbsp;" + model.Balance[1].RemainingDays + @"</label>
                                        </p>
                                         <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                         Earned  Leave Available &nbsp;&nbsp;:<label style='color: Black;'>&nbsp;" + model.Balance[2].RemainingDays + @"</label>
                                        </p>  
                                         <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                         Details &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color:Black;'>&nbsp;" + model.Details + @"</label></p>
                                        
                                         
                                           <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                         Actions&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:
                                          <a style='display: inline-block; margin-bottom: 0; font-weight: 400; text-align: center;
                                                vertical-align: middle; cursor: pointer; background-image: none; border: 1px solid transparent;
                                                border-radius: 3px; white-space: nowrap; padding: 6px 12px; font-size: 12px;
                                                -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none;
                                                user-select: none; color: #ffffff; background-color: #00a651; border-color: #00a651;' href='" + ServerName + @"Home/ApproveLeaveRequest?RequestID= " + Encrypter.Encode(model.LeaveRequestedId.ToString()) + @"'  >
                                         Approve</a>
                                            <a style='display: inline-block; margin-bottom: 0; font-weight: 400;
                                                    text-align: center; vertical-align: middle; cursor: pointer; background-image: none; 
                                                    border: 1px solid transparent; border-radius: 3px; white-space: nowrap; padding: 6px 12px;
                                                    font-size: 12px; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none;
                                                    -o-user-select: none; user-select: none; color: #ffffff; background-color: #cc2424;
                                                    border-color: #cc2424;' href='" + ServerName + @"Home/RejectLeaveRequest?RequestID= " + Encrypter.Encode(model.LeaveRequestedId.ToString()) + @"'>
                                         Reject</a>
                                            
                                            <a style='display: inline-block; margin-bottom: 0; font-weight: 400; text-align: center;
                                                        vertical-align: middle; cursor: pointer; background-image: none; border: 1px solid transparent;
                                                        border-radius: 3px; white-space: nowrap; padding: 6px 12px; font-size: 12px;
                                                        -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none;
                                                        user-select: none; color: #ffffff; background-color: #21a9e1; border-color: #21a9e1;' href='" + ServerName + @"Home/CommentLeaveRequest?RequestID= " + Encrypter.Encode(model.LeaveRequestedId.ToString()) + @"'>
                                         Comments</a>
                                        </p>
<br/>
                                         <p style='padding-left: 2%; font-weight:'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;margin: 0;'>
                                            " + model.UserName + " " + model.LastName+ @"</p>

                                    </td>
                                </tr>
                               
                               
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string LeaveRequestApproved(string UserName, string LastName, string Manager, string LastName1, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='50' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Leave Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + UserName + " " + LastName + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Your leave request has been approved by your manager " + Manager +" "+ LastName1 + @"
                                        </p>
                                        
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            Comments :<label style='color: Black; '>
                                                " + Comments + @"</label></p>
                                       
                                        <br />
                                        <p style='padding-left: 2%;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold; margin: 0;'>
                                            " + Manager +" "+ LastName1 + @"</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string LeaveRequestRejected(string UserName, string LastName, string Manager, string LastName1, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Leave Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + UserName + " " + LastName1 + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Your leave request has been Rejected by your manager " + Manager +" "+ LastName1 + @"
                                        </p>
                                       
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            Comments  :<label style='color: Black;'>
                                                " + Comments + @"</label></p>
                                        <br />
                                        <p style='padding-left: 2%;'>
                                             Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold; margin: 0'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                         " + Manager +" "+ LastName1 + @" </p>

                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string FeedBack(string senderFirstName, string senderLastName, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                   Feedback
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + "Prasanth" + @",</p>
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold;'>
                                            Feedback &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; :<label style='color: Black;'>
                                                " + Comments + @"</label></p>
                                        <br />
                                        <p style='padding-left: 2%; font-weight:;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            " + senderFirstName + ' ' + senderLastName + @"</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string NewUser(string UserName, string LoginID, string Password)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {

          padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                   WELCOME TO DSRC
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear " + UserName + @",</p>
                                                                                <br />
                                         <p style='padding-left: 2%; margin: 0;'>Your DSRC Management Portal user details are</p>
                                      
                                      
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold; margin: 0;'>
                                           Username &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color: Black;'>
                                                " + LoginID + @"</label></p>
                                     <p style='padding-left  : 2%; color: #006699; font-weight: bold; margin: 0; align='center';'>
                                            Password &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color: Black;'>
                                                " + Password + @"</label></p>
                                     
                                        <br />

                                        <p style='padding-left: 2%; font-weight: ;'>

                                        <p style='padding-left: 2%; margin: 0;'>

                                           Click on <a href='" + ServerName + @"' style='color: Blue; margin: 0;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold; margin:0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold; margin:0;'>
                                            DSRC Management</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string PasswordRecovery(string UserName, string LoginID, string Password)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
         padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                   Password Recovery
                                                </td>
                                            </tr>

                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + UserName + @",</p>
                                                                               
                                         <p style='padding-left: 2%; margin:0;'>Your DSRC Management Portal user details are</p>
                                   
                                       
                                        <p style='padding-left: 2%; color: #006699; font-weight: bold;margin: 0; padding: 0;'>
                                            Username &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color: Black;'>
                                                " + LoginID + @"</label></p>
                                     <p style='padding-left: 2%; color: #006699; font-weight: bold;margin: 0; padding: 0;'>
                                            Password &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<label style='color: Black;'>
                                                " + Password + @"</label></p>
                                       <br/>
                                       

                                        <p style='padding-left: 2%; font-weight: ;'>

                                        <p style='padding-left: 2%; margin:0;'>

                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold; margin:0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold; margin:0;'>
                                            DSRC Management</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string ProjectStatus(string projectName, string comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC Project Status
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        S.No
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Project Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Project Status
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Status Comments
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tr>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    1
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;'>
                                                    " + projectName + @"
                                                </td>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    @InlineImage
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;'>
                                                    " + comments + @"
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string ProjectSummary(List<string> projectName, List<string> comments, List<int?> pro_status)
        {
            string MessageBody1 = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC Project Status
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        S.No
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Project Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Project Status
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Status Comments
                                                    </th>
                                                </tr>
                                            </thead>";
            string MessageBody2 = @"</table>
                                        <br/>
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            string Tablebody = "";
            for (int i = 0; i < projectName.Count; i++)
            {
                string pro_color = ((pro_status[i] == 3) ? "Pro^Img^Green" : (pro_status[i] == 2) ? "Pro^Img^Orange" : "Pro^Img^Red");
                Tablebody += @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"
                + (i + 1) + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                + projectName[i] + @"</td><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                + pro_color + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>" + comments[i] + @"</td></tr>";
            }
            string MessageBody = MessageBody1 + Tablebody + MessageBody2;
            return MessageBody;
        }

        public static string InlineSignatureHtmlPage(string senderFirstName, string receiverName, string subject, string content, bool isSignature)
        {
            string messageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo>" +
@"<p style='padding-left: 2%;'>" + content + @"</p>@InlineImages " +
    ((isSignature) ? @"<p style='font-weight: bold;'><span style='color: #006699;'>Thanks,</span></br>" + senderFirstName + @"</p>" : "")
+ @"
</body>
</html>";
            return messageBody;
        }

        public static string BirthdayRemainder(string senderFirstName)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {

          padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                   Happy Birthday Greetings
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 130px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear " + senderFirstName + @",</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:50px; margin: 0;'>
                                           On behalf of DSRC we are delighted to wish you a<b style='font-style:italic; color:Blue; font-weight:bold;'> Happy Birthday</b> that blooms with beautiful surprises and happy moments.... 

                                               <br>
											   <br>
											   Have a wonderful day!!!
                                          </p>

                                     <br />
                                        <p style=' color: #006699; font-weight: bold; padding-bottom:10px; margin:0;'>
                                            Regards,</p>
                                        <p style='font-weight: bold; margin:0;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                                 
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string ForgotPassword(string UserName, string LastName)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {

          padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                Password Recovery
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 130px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear " + UserName + LastName + @",</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:50px; margin: 0;'>
                                            Please click on the below link to set a new password for your account.
                                             Click on <a href='" + ServerName + "User/ChangePasswordUsingGUIID/" + @"' style='color: Blue;'><u>" + ServerName + "User/ChangePasswordUsingGUIID/" + @"
                                            </u></a>to login to DSRC Management Portal </p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                                 
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }


        public static string Completion(string path,int trainingid,int userid,string trainingname,string scheduleddate)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {

          padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    L & D
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'> Training Feedback Required.
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 130px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear All " + @",</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:50px; margin: 0;'>


                                            Thanks for attending " + trainingname + " session on " + scheduleddate + @".<br />I hope everyone gain the knowledge.<br /><br /> Please provide your valuable feedback within 3 days.<br /><br />
                                            For feedback follow,<br /> Learning and Development -> My Training -> Worklist -> Feedback path in Management Portal.

                                            <br /><br /> </p>
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"                                            
                                            </u></a>to log in to DSRC Management Portal </p>
                                            <br /><br />
                                        <p style='color: #006699; font-weight: bold; margin:0'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;margin:0'>
                                            L & D Admin</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                                 
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string SendhwswRequest(Assets model)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2'  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + model.ReportingPersonName + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been sumbitted for approval by " + model.EmpName + @".
                                        </p>
                                    </td>
                                </tr>
                                
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                     Description
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Description + @"
                                    </label>
                                  </td>
                                </tr>
                                  
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                        Department Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.DepartmentName + @"
                                    </label>
                                  </td>
                                </tr>

<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Location
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Location + @"
                                            </label>
</td>
</tr>

<tr>
<td>                                       
                                           
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           Computer Name
</p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.ComputerName + @"
                                            </label>
</td>
</tr>
                                        ";

            MessageBody += @"
<tr>
<td>

<p style='padding: 10px; color: #006699; font-weight: bold;'>
                                         Category
</p>
</td>
<td>
<label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Category + @"</label>
</td>
</tr>
<tr><td>                                        
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                         Status</p>
</td>
<td>
<label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Status + @"</label>
</td></tr>
<tr><td>                                        
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                         Priority
</p>  
</td>
<td>
<label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Priority + @"</label>
</td></tr>
                                        
          <tr>
<td>                                 

                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                         Actions
</p>
</td>
<td style='padding: 10px;'>
                                          <b>:</b>&nbsp;&nbsp;<a style='display: inline-block; margin-bottom: 0; font-weight: 400; text-align: center;
                                                vertical-align: middle; cursor: pointer; background-image: none; border: 1px solid transparent;
                                                border-radius: 3px; white-space: nowrap; padding: 6px 12px; font-size: 12px;
                                                -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none;
                                                user-select: none; color: #ffffff; background-color: #00a651; border-color: #00a651;' href='" + ServerName + @"HarwareSoftwareRequest/ApproveVMail_Stage1?RequestID= " + Encrypter.Encode(model.RequestedId.ToString()) + @"'  >
                                         Approve</a>
                                            <a style='display: inline-block; margin-bottom: 0; font-weight: 400;
                                                    text-align: center; vertical-align: middle; cursor: pointer; background-image: none;
                                                    border: 1px solid transparent; border-radius: 3px; white-space: nowrap; padding: 6px 12px;
                                                    font-size: 12px; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none;
                                                    -o-user-select: none; user-select: none; color: #ffffff; background-color: #cc2424;
                                                    border-color: #cc2424;' href='" + ServerName + @"HarwareSoftwareRequest/RejectVMail_Stage1?RequestID= " + Encrypter.Encode(model.RequestedId.ToString()) + @"'>
                                         Reject</a>
                                            
                                            <a style='display: inline-block; margin-bottom: 0; font-weight: 400; text-align: center;
                                                        vertical-align: middle; cursor: pointer; background-image: none; border: 1px solid transparent;
                                                        border-radius: 3px; white-space: nowrap; padding: 6px 12px; font-size: 12px;
                                                        -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none;
                                                        user-select: none; color: #ffffff; background-color: #21a9e1; border-color: #21a9e1;' href='" + ServerName + @"HarwareSoftwareRequest/CommentVMail_Stage1?RequestID= " + Encrypter.Encode(model.RequestedId.ToString()) + @"'>
                                         Comments</a>
</td>
</tr>
<tr>
<td colspan='2'>
                                        
<br/>     
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>

                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold; margin: 0;'>
                                            " + model.EmpName + @" </p>
</td>
</tr>
                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;

        }

        public static string SendhwswRequest_Stage2(Assets model)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2'  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + model.Networkheadname + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been Approved by " + model.MngrName + " and forwarded for your Approval." + @".
                                         </p>
                                    </td>
                                </tr>

                                       
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                     Description
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Description + @"
                                    </label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                        Department Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.DepartmentName + @"
                                    </label>
                                  </td>
                                </tr>
                                            
                                        
                               <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                      Location
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Location + @"
                                     </label>
                                  </td>
                                </tr>  

                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                      Computer Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.ComputerName + @"
                                    </label>
                                  </td>
                                </tr>  
                                        ";

            MessageBody += @"
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                         Category
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Category + @"
                                    </label>
                                  </td>
                                </tr> 
       
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>                                         
                                         Status
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Status + @"
                                    </label>
                                  </td>
                                </tr> 

                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>   
                                         Priority
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + model.Priority + @"
                                    </label>
                                  </td>
                                </tr> 
                                           
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>   
                                         Actions&nbsp;&nbsp;
                                    </p>
                                  </td>
                                  <td>     <b>:</b>&nbsp;&nbsp;<a style='display: inline-block; margin-bottom: 0; font-weight: 400; text-align: center;
                                                vertical-align: middle; cursor: pointer; background-image: none; border: 1px solid transparent;
                                                border-radius: 3px; white-space: nowrap; padding: 6px 12px; font-size: 12px;
                                                -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; -o-user-select: none;
                                                user-select: none; color: #ffffff; background-color: #00a651; border-color: #00a651;' href='" + ServerName + @"HarwareSoftwareRequest/CommentVMail_Stage2?RequestID= " + Encrypter.Encode(model.RequestedId.ToString()) + "&choice=1" + @"'  >
                                         Approve</a>
                                            <a style='display: inline-block; margin-bottom: 0; font-weight: 400;
                                                    text-align: center; vertical-align: middle; cursor: pointer; background-image: none;
                                                    border: 1px solid transparent; border-radius: 3px; white-space: nowrap; padding: 6px 12px;
                                                    font-size: 12px; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none;
                                                    -o-user-select: none; user-select: none; color: #ffffff; background-color: #cc2424;
                                                    border-color: #cc2424;' href='" + ServerName + @"HarwareSoftwareRequest/CommentVMail_Stage2?RequestID= " + Encrypter.Encode(model.RequestedId.ToString()) + "&choice=0" + @"'>
                                         Reject</a>   
                                    </td>
                                </tr>                                          

        
<tr>
<td colspan='2'>
                                        
<br/>    
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            " + model.MngrName + @" </p>
 </td>
</tr>   

                               
                               
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;

        }

        public static string HwSwRequestApproved(int ReqID, string EmpName, string Description, string Manager, string NetworkingMngr, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='50' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2' style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + EmpName + "," + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been approved by " + Manager + " and <br>Networking Manager Is asked to resolve the issue. " + @"
                                        </p>
                                 </td>
                                </tr>
                                        
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                      Request ID
                                     </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ReqID + @"
                                    </label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                          Requested Employee
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + EmpName + @"
                                     </label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Comments
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Comments + @"
                                    </label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td colspan='2'>                                          
                                        <p style='padding: 10px;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin:0'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            " + Manager + @"</p>
                                  </td>
                                </tr>
                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;

        }

        public static string HwSwRequestRejected(int ReqID, string EmpName, string Description, string Manager, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td colspan='2' style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear Employee," + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been Rejected by " + Manager + @"
                                            <br>
                                        </p>
                                    </td>
                                </tr>
                                        
                                <tr>                                
                                  <td>
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Request ID
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ReqID + @"
                                    </label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Requested Employee</p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + EmpName + @"</label>
                                  </td>
                                </tr>
                                            
                                        
                               <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                     Comments</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Comments + @"</label> 
                                  </td>
                                </tr>

                                <tr>
                                    <td colspan='2'>
                                        <br />
                                        <p style='padding: 5px; font-weight:bold;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin:0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold;'>
                                            " + Manager + @"</p>
                                </td>
                                </tr>     
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string HwSwRequestApprovedStage2(int ReqID, string EmpName, string Description, string NetworkingMngr, string NetworkingEmp, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='50' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr> 
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2' style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + EmpName + "," + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been approved by " + NetworkingMngr + ".<br>Networking Employee Is asked to resolve the issue. " + @"
                                        </p>
                                    </td>
                                </tr>

                                <tr>                                
                                  <td>
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Request ID</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ReqID + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Requested Employee</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + EmpName + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Description</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Description + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Task Assigned To</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + NetworkingEmp + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Comments</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Comments + @"</label>
                                  </td>
                                </tr>

                                <tr>
                                <td colspan='2'>
                                        <br />
                                        <p style='padding: 5px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin:0'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold;'>
                                            " + NetworkingMngr + @"</p>
                                  </td>
                                </tr>  
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string HwSwRequestRejectedStage2(int ReqID, string EmpName, string Description, string NetworkingMngr, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='50' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr> 
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2' style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear Employee," + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been Rejected by " + NetworkingMngr + @"
                                        </p>
                                    </td>
                                </tr>
                                        
                                 <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Request ID</p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ReqID + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Requested Employee</p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + EmpName + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Description</p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Description + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Comments</p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Comments + @"</label>
                                  </td>
                                </tr>

                                <tr>
                                <td colspan='2'>

                                        <br />
                                        <p style='padding: 10px; font-weight:bold;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='padding: 5px; color: #006699; font-weight: bold;margin:0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold;'>
                                            " + NetworkingMngr + @"</p>
                                </td>
                                </tr>                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string HwSwRequestAssignTo(int ReqID, string EmpName, string Description, string NetworkingMngr, string NetworkingEmp, string Comments)
        {
            string MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='50' style='padding: 0 20px 10px 0;'>
                                         @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Hardware Request
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr> 
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td style='colspan='2' padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + NetworkingEmp + ", " + @"</p>
                                        <p style='padding-left: 15%;'>
                                            Hardware request has been Assigned by " + NetworkingMngr + ".<br>You are asked to resolve the issue. " + @"
                                        </p>
                                    </td>
                                </tr>
                                        
                                 <tr>                                
                                  <td>
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Request ID</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ReqID + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Requested Employee</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + EmpName + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Description</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Description + @"</label>
                                  </td>
                                </tr>
                                
                                 <tr>                                
                                  <td>      
                                    <p style='padding: 5px; color: #006699; font-weight: bold;'>
                                            Comments</p>
                                  </td>
                                  <td>
                                    <label style='padding: 5px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Comments + @"</label>
                                  </td>
                                </tr>
                                
                                <tr>
                                <td colspan='2'>
                                        <br />
                                        <p style='padding: 5px;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style='color: #006699; font-weight: bold;margin:0'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold;'>
                                            " + NetworkingMngr + @"</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string NewJoiningOnBoarding(List<string> Name, List<string> Department, List<DateTime?> JoiningDate, List<string> Experience)
        {
            string MessageBody1 = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC New Joining Employee List
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 0px 0 0px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                             Dear All,</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:0px; margin: 0;'>
                                            This mail is to inform you that the following is the list of new joining employees on " + DateTime.Today.ToString("dd MMM yyyy") + @" 
                                         </p>

                                     <br />
                                       
                                    </td>
                                </tr>
                            </table>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        S.No
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Department
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Joining Date
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Expericence
                                                    </th>
                                                </tr>
                                            </thead>";
            string MessageBody2 = @"
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            string Tablebody = "";
            for (int i = 0; i < Name.Count; i++)
            {
                //string pro_color = ((pro_status[i] == 3) ? "Pro^Img^Green" : (pro_status[i] == 2) ? "Pro^Img^Orange" : "Pro^Img^Red");
                Tablebody += @"<tr><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>"
                + (i + 1) + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;'>"
                + Name[i] + @"</td><td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;text-align: left;'>"
                + Department[i] + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb; text-align: center;'>"
                + DateTime.Parse((JoiningDate[i].ToString())).ToString("dd MMM yyyy") + "</td><td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top; border-top: 1px solid #ebebeb;text-align: center;'>"
                + Experience[i] + @"</td></tr>";
            }
            string MessageBody = MessageBody1 + Tablebody + MessageBody2;
            return MessageBody;
        }

        public static string DeleteEmployee(string Name, string Department, string JoiningDate, string Experience)
        {
            string MessageBody = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC Employee Deleted
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 0px 0 0px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                             Dear All,</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:0px; margin: 0;'>
                                            This mail is to inform you that the following employee was deleted on  " + DateTime.Today.ToString("dd MMM yyyy") + @"
                                         </p>

                                     <br />
                                       
                                    </td>
                                </tr>
                            </table>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Employee Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Department
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Joining Date
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Expericence
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tr>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                     " + Name + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: left;'>
                                                    " + Department + @"
                                                </td>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + JoiningDate + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: center;'>
                                                    " + Experience + @"
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string ResignedEmployeeDetails(string EmployeeName, string Department, string LastWorkingDate, string ResignedOn)
        {
            string MessageBody = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC Resigned Employee Details
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 0px 0 0px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                             Dear All,</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:0px; margin: 0;'>
                                           This mail is to inform you that the following is the list of resigned employees details on  " + DateTime.Today.ToString("dd MMM yyyy") + @"
                                         </p>

                                     <br />
                                       
                                    </td>
                                </tr>
                            </table>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Employee Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Department
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Resigned On 
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Last Working Date
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tr>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                     " + EmployeeName + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: left;'>
                                                    " + Department + @"
                                                </td>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + ResignedOn + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: center;'>
                                                    " + LastWorkingDate + @"
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string NewEmployeeAdd(string EmployeeName, string Department, string RoleName, DateTime? JoiningDate, string Experience)
        {
            string MessageBody = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC New Employee Added
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 0px 0 0px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                             Dear All,</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:0px; margin: 0;'>
                                            This mail is to inform you that the following employee was added on  " + DateTime.Today.ToString("dd MMM yyyy") + @"
                                         </p>

                                     <br />
                                       
                                    </td>
                                </tr>
                            </table>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Employee Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Department
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Role Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Joining Date
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Expericence
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tr>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                     " + EmployeeName + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: left;'>
                                                    " + Department + @"
                                                </td>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + RoleName + @"
                                                </td>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + JoiningDate.Value.ToString("dd MMM yyyy") + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: center;'>
                                                    " + Experience + @"
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string EmployeeDetailsUpdate(string MailBody, string EmpId, string EmployeeName, string Department)
        {
            string MessageBody = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                   DSRC Employee Details Updated 
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear ALL, </p>
                                            <br />
                                         <p style='padding-left: 2%; margin: 0;'>This mail is to inform you that the following  details of <label style='color: #006699; font-weight: bold;'>" + EmployeeName + "</label> with <label style='color: #006699; font-weight: bold;'>" + EmpId + " </label>has been updated on " + DateTime.Today.ToString("dd MMM yyyy") + @" </p>
                                         <br/>
                                         
                                          " + MailBody + @"
                                     
                                        <br />

                                        <p style='padding-left: 2%; font-weight: ;'>

                                        <p style='padding-left: 2%; margin: 0;'>

                                           Click on <a href='" + ServerName + @"' style='color: Blue; margin: 0;'><u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold; margin:0;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold; margin:0;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string EmployeeDetailsDeactivate(string EmployeeID, string EmployeeName, string EmailAddress, string Department, DateTime? JoiningDate, string Experience)
        {
            string MessageBody = @"<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding: 10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
        .table-bordered > thead > tr > th, .table-bordered > thead > tr > td
        {
            background-color: #303641;
            border-bottom-width: 1px;
            color: #F7F6F6;
        }
        .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td
        {
            border: 1px solid #ebebeb;
        }
        .table > thead > tr > th
        {
            vertical-align: bottom;
            border-bottom: 2px solid #ebebeb;
        }
        .table > thead > tr > th, .table > tbody > tr > th, .table > tfoot > tr > th, .table > thead > tr > td, .table > tbody > tr > td, .table > tfoot > tr > td
        {
            padding: 8px;
            line-height: 1.428571429;
            vertical-align: top;
            border-top: 1px solid #ebebeb;
        }
        th
        {
            text-align: left;
            font-weight: 400;
            color: #303641;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
        }
        #tblProjects td:nth-child(3)
        {
            word-wrap: break-word;
            word-break: break-all;
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' style='width: 100%; max-width: 600px;' align='center' cellpadding='0'
                    cellspacing='0' border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' style='padding: 10px 10px 0px 10px;'>
                            <table width='100%' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' width='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                    <td height='70'>
                                        <div style='font-size: 15px; color: #ffffff; font-family: sans-serif;
                                            letter-spacing: 10px; padding: 0 0 0 3px;'>
                                            Management Portal
                                        </div>
                                        <div style='color: #153643; font-family: sans-serif; font-size: 24px; line-height: 28px;
                                            font-weight: bold; padding: 5px 0 0 0;'>
                                            DSRC Employee Details Deactivated
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; border-bottom: 1px solid #f2eeed;'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td style='padding: 20px 0 30px 0;'>
                                        <br />
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 0px 0 0px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                             Dear All,</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:0px; margin: 0;'>
                                            This mail is to inform you that the following employee was deactivated on  " + DateTime.Today.ToString("dd MMM yyyy") + @"
                                         </p>

                                     <br />
                                       
                                    </td>
                                </tr>
                            </table>
                                        <br />
                                        <table style='border-collapse: collapse; border-spacing: 0;' id='tblProjects' width='100%'>
                                            <thead>
                                                <tr>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        EmployeeID
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Employee Name
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Department
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: left; font-weight: 400;'>
                                                        Email Address
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Joining Date
                                                    </th>
                                                    <th style='background-color: #303641; border-bottom-width: 1px; color: #F7F6F6; border: 1px solid #ebebeb;
                                                        vertical-align: bottom; border-bottom: 2px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                        vertical-align: top; border-top: 1px solid #ebebeb; text-align: center; font-weight: 400;'>
                                                        Expericence
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tr>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                     " + EmployeeID + @"
                                                </td>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                     " + EmployeeName + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: left;'>
                                                    " + Department + @"
                                                </td>
                                                <td style='text-align: left; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + EmailAddress + @"
                                                </td>
                                                <td style='text-align: center; border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429;
                                                    vertical-align: top; border-top: 1px solid #ebebeb;'>
                                                    " + JoiningDate.Value.ToString("dd MMM yyyy") + @"
                                                </td>
                                                <td style='border: 1px solid #ebebeb; padding: 8px; line-height: 1.428571429; vertical-align: top;
                                                    border-top: 1px solid #ebebeb;text-align: center;'>
                                                    " + Experience + @"
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <p style='padding-left: 2%; font-weight: ;'>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'>
                                                <u>" + ServerName + @"
                                            </u></a>to login to DSRC Management Portal</p>
                                        <br />
                                        <p style='color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px 30px 30px 30px; color: #153643; font-family: sans-serif;
                            font-size: 16px; line-height: 22px;'>
                            <p style='font-size: 12px; font-weight: bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }

        public static string ResetPassword(string UserName, string LastName, Int32? Key)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green
        {
            color: #ffffff;
            background-color: #00a651;
            border-color: #00a651;
        }
        .btn
        {
            display: inline-block;
            margin-bottom: 0;
            font-weight: 400;
            text-align: center;
            vertical-align: middle;
            cursor: pointer;
            background-image: none;
            border: 1px solid transparent;
            white-space: nowrap;
            padding: 6px 12px;
            font-size: 12px;
            line-height: 1.428571429;
            border-radius: 3px;
            -webkit-user-select: none;
            -moz-user-select: none;
            -ms-user-select: none;
            -o-user-select: none;
            user-select: none;
        }
        a
        {
            color: #373e4a;
            text-decoration: none;
        }
        .btn-danger
        {
            color: #ffffff;
            background-color: #cc2424;
            border-color: #cc2424;
        }
        .btn-info
        {
            color: #ffffff;
            background-color: #21a9e1;
            border-color: #21a9e1;
        }
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {

          padding: 10px 10px 0px 10px;

        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                        @Logo
                                    </td>
                                </tr>
                            </table>
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    Management Portal
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                 Employee Reset Password 
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                               
                                <tr>
                                    <td style='padding: 20px 0 130px 0;'>
                                        <p style='color: #006699; font-weight: bold; margin: 0;'>
                                            Dear " + UserName + LastName + @",</p>
                             <br />
                                         <p style='padding-left: 2%; padding-bottom:50px; margin: 0;'>
                                             We received a request to change your password on DSRC Management Portal. If you really did, click on the below link to reset your password <br /><br />
                                             <a href='" + ServerName + "User/ChangePasswordUsingGUIID?Key=" + Key + @"' style='color: Blue;'><u>click here to reset your password
                                            </u></a><br/><br />If you didnt mean to reset your password, then you can just ignore this email. Your password will not change. </p>
                                            <br />
                                        <p style=' color: #006699; font-weight: bold;'>
                                            Thanks,</p>
                                        <p style='font-weight: bold;'>
                                            DSRC Management Portal</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                            <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended mailbox and you will not receive a response.
                            </p>
                            </td>
                    </tr>
                    <tr>
                        <td style='padding: 20px 30px 15px 30px;' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='font-family: sans-serif; font-size: 14px; color: #ffffff;'>
                                        &reg; copyright 2016 - DSRC
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            return MessageBody;
        }        

        public static string NominationConfirmation(string Empname,string TrainingId, string TrainingName, string ScheduledDate,string start,string end, string Instructor)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    L&D
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Training Nomination Confirmation
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2'  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear " + Empname + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Your nomination has confirmed. Please refer the following details." + @".
                                        </p>
                                    </td>
                                </tr>
                                
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                     Training ID
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingId + @"
                                    </label>
                                  </td>
                                </tr>
                                  
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                        Training Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingName + @"
                                    </label>
                                  </td>
                                </tr>

<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Scheduled Date
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ScheduledDate + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Start Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + start + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           End Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + end + @"
                                            </label>
</td>
</tr>

<tr>
<td>                                       
                                           
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           Instructor
</p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Instructor + @"
                                            </label>
</td>
</tr>
                                        ";

            MessageBody += @"

<tr>
<td colspan='2'>
                                        
<br/>     
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to log in to L&D Portal</p>

                                            <br />
                                        <p style='color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold; margin: 0;'>
                                                 L & D Admin" + @" </p>
</td>
</tr>
                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";

            return MessageBody;
        }


        public static string TrainingScheduleChangeNotification(string TrainingId, string TrainingName, string ScheduledDate,string start,string end, string Instructor)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    L&D
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Training Schedule Changed
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2'  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear Employee" + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Your training schedule has changed. Please refer the following details." + @".
                                        </p>
                                    </td>
                                </tr>
                                
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                     Training ID
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingId + @"
                                    </label>
                                  </td>
                                </tr>
                                  
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                        Training Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingName + @"
                                    </label>
                                  </td>
                                </tr>

<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Scheduled Date
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ScheduledDate + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Start Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + start + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           End Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + end + @"
                                            </label>
</td>
</tr>

<tr>
<td>                                       
                                           
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           Instructor
</p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Instructor + @"
                                            </label>
</td>
</tr>
                                        ";

            MessageBody += @"

<tr>
<td colspan='2'>
                                        
<br/>     
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to log in to L&D Portal</p>

                                            <br />
                                        <p style='color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold; margin: 0;'>
                                                L & D Admin" + @" </p>
</td>
</tr>
                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";

            return MessageBody;
        }

        public static string TrainingScheduleCancelled(string TrainingId, string TrainingName, string ScheduledDate,string start,string end, string Instructor)
        {
            string MessageBody;
            MessageBody = @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
    <title></title>
    <style type='text/css'>
        .btn-success, .btn-green {
  color: #ffffff;
  background-color: #00a651;
  border-color: #00a651;
}
.btn {
  display: inline-block;
  margin-bottom: 0;
  font-weight: 400;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  background-image: none;
  border: 1px solid transparent;
  white-space: nowrap;
  padding: 6px 12px;
  font-size: 12px;
  line-height: 1.428571429;
  border-radius: 3px;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  -o-user-select: none;
  user-select: none;
}
a {
  color: #373e4a;
  text-decoration: none;
}
.btn-danger {
  color: #ffffff;
  background-color: #cc2424;
  border-color: #cc2424;
}
.btn-info {
  color: #ffffff;
  background-color: #21a9e1;
  border-color: #21a9e1;
}
        body
        {
            margin: 0;
            padding: 0;
            min-width: 100% !important;
        }
        img
        {
            height: auto;
        }
        .content
        {
            width: 100%;
            max-width: 600px;
        }
        .header
        {
            padding:10px 10px 0px 10px;
        }
        .innerpadding
        {
            padding: 30px 30px 30px 30px;
        }
        .borderbottom
        {
            border-bottom: 1px solid #f2eeed;
        }
        .subhead
        {
            font-size: 15px;
            color: #ffffff;
            font-family: sans-serif;
            letter-spacing: 10px;
        }
        .h1, .h2, .bodycopy
        {
            color: #153643;
            font-family: sans-serif;
        }
        .h1
        {
            font-size: 33px;
            line-height: 38px;
            font-weight: bold;
        }
        .h2
        {
            padding: 0 0 15px 0;
            font-size: 24px;
            line-height: 28px;
            font-weight: bold;
        }
        .bodycopy
        {
            font-size: 16px;
            line-height: 22px;
        }
        .button
        {
            text-align: center;
            font-size: 18px;
            font-family: sans-serif;
            font-weight: bold;
            padding: 0 30px 0 30px;
        }
        .button a
        {
            color: #ffffff;
            text-decoration: none;
        }
        .footer
        {
            padding: 20px 30px 15px 30px;
        }
        .footercopy
        {
            font-family: sans-serif;
            font-size: 14px;
            color: #ffffff;
        }
        .footercopy a
        {
            color: #ffffff;
            text-decoration: underline;
        }
        @media only screen and (max-width: 550px), screen and (max-device-width: 550px)
        {
            body[yahoo] .hide
            {
                display: none !important;
            }
            body[yahoo] .buttonwrapper
            {
                background-color: transparent !important;
            }
            body[yahoo] .button
            {
                padding: 0px !important;
            }
            body[yahoo] .button a
            {
                background-color: #e05443;
                padding: 15px 15px 13px !important;
            }
            body[yahoo] .unsubscribe
            {
                display: block;
                margin-top: 20px;
                padding: 10px 50px;
                background: #2f3942;
                border-radius: 5px;
                text-decoration: none !important;
                font-weight: bold;
            }
        }
     
    </style>
</head>
<body yahoo bgcolor='#f6f8f1'>
    <table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>
        <tr>
            <td>
              
                <table bgcolor='#ffffff' class='content' align='center' cellpadding='0' cellspacing='0'
                    border='0'>
                    <tr>
                        <td bgcolor='#c7d8a7' class='header'>
                            <table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td height='70' style='padding: 0 20px 10px 0;'>
                                       @Logo
                                    </td>
                                </tr>
                            </table>
                      
                            <table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%;
                                max-width: 425px;'>
                                <tr>
                                    <td height='70'>
                                        <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td class='subhead' style='padding: 0 0 0 3px;'>
                                                    L&D
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='h2' style='padding: 5px 0 0 0;'>
                                                    Training Schedule Cancelled
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class='innerpadding borderbottom'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                              
                                <tr>
                                    <td colspan='2'  style='padding: 20px 0 30px 0;' >
                                        <p style='color: #006699; font-weight: bold;'>
                                            Dear Employee" + @",</p>
                                        <p style='padding-left: 15%;'>
                                            Training schedule was cancelled. Please refer the training details." + @".
                                        </p>
                                    </td>
                                </tr>
                                
                                <tr>                                
                                  <td>
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                     Training ID
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingId + @"
                                    </label>
                                  </td>
                                </tr>
                                  
                                <tr>                                
                                  <td>      
                                    <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                        Training Name
                                    </p>
                                  </td>
                                  <td>
                                    <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + TrainingName + @"
                                    </label>
                                  </td>
                                </tr>

<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Scheduled Date
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + ScheduledDate + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                            Start Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + start + @"
                                            </label>
</td>
</tr>
<tr>
<td>
                                      
                                        <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           End Time
 </p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + end + @"
                                            </label>
</td>
</tr>

<tr>
<td>                                       
                                           
                                         <p style='padding: 10px; color: #006699; font-weight: bold;'>
                                           Instructor
</p>
</td>
<td>
                                            <label style='padding: 10px; color: Black; font-weight: bold;'>:&nbsp;&nbsp;" + Instructor + @"
                                            </label>
</td>
</tr>
                                        ";

            MessageBody += @"

<tr>
<td colspan='2'>
                                        
<br/>     
                                         <p style='padding: 10px; '>
                                            Click on <a href='" + ServerName + @"' style='color: Blue;'><u>" + ServerName + @"
                                            </u></a>to log in to L&D Portal</p>

                                            <br />
                                        <p style='color: #006699; font-weight: bold;margin: 0;'>
                                            Thanks,</p>
                                        <p style=' font-weight: bold; margin: 0;'>
                                                 L & D Admin" + @" </p>
</td>
</tr>
                                   
                            </table>
                        </td>
                    </tr>
                    <tr>
                    </tr>
                    <tr>
                        <td class='innerpadding bodycopy'>
                          <p style='font-size: 12px;font-weight:bold;'>
                                Please keep a copy of this mail for future reference, This email has been automatically
                                generated. Please do not reply to this email address as all responses are directed
                                to an unattended, mailbox, and you will not receive a response
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class='footer' bgcolor='#44525f'>
                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' class='footercopy'>
                                        &reg; copyright 2016 - DSRC <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
             
            </td>
        </tr>
    </table>
</body>
</html>";

            return MessageBody;
        }
    }
}