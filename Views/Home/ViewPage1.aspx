<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title></title>
    <style>
        .panel-heading > .panel-title
        {
            float: left;
            padding: 10px 15px;
        }
        .panel-title
        {
            margin-top: 0;
            margin-bottom: 0;
            font-size: 14px;
        }
        *, *:before, *:after
        {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
        }
        user agent stylesheetdiv
        {
            display: block;
        }
        Inherited from .panel-default > .panel-heading
        {
            color: #373e4a;
            background-color: #f0f0f1;
            border-color: #ebebeb;
        }
        Inherited from body
        {
            font-family: "Noto Sans" , sans-serif, "Helvetica Neue" , Helvetica, Arial, sans-serif;
            font-size: 12px;
            line-height: 1.428571429;
            color: #282727;
            background-color: #ffffff;
        }
        body
        {
            font-family: "Noto Sans" , sans-serif;
        }
        Inherited from html
        {
            font-size: 62.5%;
            -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
        }
        html
        {
            font-family: sans-serif;
            -ms-text-size-adjust: 100%;
            -webkit-text-size-adjust: 100%;
        }
        Pseudo ::before element *, *:before, *:after
        {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
        }
        Pseudo ::after element *, *:before, *:after
        {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
        }
    </style>
</head>
<body>
    <div class="row">
        <div class="col-md-3">
        </div>
        <div class="col-md-6">
            <div class="panel panel-default panel-shadow" data-collapsed="0">
                <!-- panel head -->
                <div class="panel-heading">
                    <div class="panel-title" color="White">
                        Approve Leave Request</div>
                </div>
                <!-- panel body -->
                <div class="panel-body">
                    @if (ViewBag.IsalreadyApproved) {
                    <div style="text-align: center" class="">
                        You Already Approved,
                    </div>
                    } else {
                    <div style="text-align: center" class="success">
                        Sucessfully Leave Request Aprroved,
                    </div>
                    }
                </div>
            </div>
        </div>
        <div class="col-md-3">
        </div>
    </div>
</body>
</html>
