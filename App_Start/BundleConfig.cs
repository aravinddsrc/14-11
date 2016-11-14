using System.Web;
using System.Web.Optimization;

namespace DSRCManagementSystem
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            //#region HRMS_Styles
            //bundles.Add(new StyleBundle("~/Content/Layout/css").Include(
            //"~/Content/Template/css/neon.css",
            //"~/Content/Template/css/neon-core.css",
            //"~/Content/Template/css/neon-forms.css",
            //"~/Content/Template/css/neon-theme.css",
            //    //NEON
            //"~/Content/Template/js/jquery-ui/css/no-theme/jquery-ui-1.10.3.custom.min.css",
            //"~/Content/Template/css/bootstrap.css",
            //"~/Content/Template/js/select2/select2-bootstrap.css",
            //"~/Content/Template/js/select2/select2.css",
            //"~/Content/Plugins/bootstrap-datetimepicker-master/css/bootstrap-datetimepicker.min.css",
            //"~/Content/Plugins/bootstrap-datetimepicker/css/bootstrap-datepicker.css",
            //"~/Content/ContextMenu/screen.css"
            //));
            //#endregion
            

            //#region HRMS_Scripts
            //bundles.Add(new ScriptBundle("~/Content/Layout/js").Include(
            //"~/Scripts/jquery-1.7.1.min.js",
            //"~/Content/Template/js/jquery-1.11.2.min.js",
            //"~/Content/Template/js/bootstrap.min.js",
            //"~/Content/Plugins/bootstrap-datetimepicker/js/moment.js",
            //"~/Content/Template/js/select2/select2.min.js",
            //"~/Content/Plugins/bootstrap-datetimepicker/js/bootstrap-datepicker.js",
            //"~/Content/bootstrap/bootbox.min.js",
            //"~/Content/Template/js/toastr.js",
            //    //NEON
            //"~/Content/Template/js/TweenMax.min.js",
            //"~/Content/Template/js/jquery-ui/js/jquery-ui-1.10.3.minimal.min.js",
            //"~/Content/Template/js/neon-custom.js",
            //"~/Content/Template/js/joinable.js",
            //"~/Content/Template/js/resizeable.js",
            //"~/Content/Template/js/neon-api.js",
            //"~/Content/Template/js/cookies.min.js",
            //"~/Content/Template/js/jquery.validate.min.js",
            //"~/Content/Template/js/neon-login.js",
            //"~/Content/Template/js/neon-demo.js",
            //"~/Content/Template/js/cookies.min.js",
            //"~/Content/Template/js/bootstrap-switch.min.js",
            //"~/Content/Template/js/neon-chat.js",
            //"~/Content/Template/js/neon-demo.js",
            //"~/Content/Template/js/neon-skins.js"));
            //#endregion
            
            ////DoNot Delete The Below Line
            //BundleTable.EnableOptimizations = true;
        }
    }
}