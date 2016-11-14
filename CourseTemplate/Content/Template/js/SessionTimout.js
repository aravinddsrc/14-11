
$(document).ready(function () {


    $(window).focus(function () {
        checkSession();
    });

    function checkSession() {
        $.ajax({
            type: 'POST',
            url: rootDir + '/User/ValidateSession',
            datatype: "JSON",
            contentType: 'application/json; charset=utf-8',
            error: function (xhr, desc, error) {
                CheckAuthorization(xhr);
            }
        });
    }

    function CheckAuthorization(xhr) {

        console.log(rootDir + JSON.parse(xhr.responseText).RedirectURL);
        console.log(xhr.responseText);

        if (IsJsonString(xhr.responseText) && JSON.parse(xhr.responseText).Error == 'NotAuthorized') {
            window.location.href = rootDir + JSON.parse(xhr.responseText).RedirectURL;
        }
    }

    function IsJsonString(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }


});

