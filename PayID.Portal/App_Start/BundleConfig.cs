using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace PayID.Portal.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;
            var styleCdnPath = "http://fonts.googleapis.com/css?family=Open+Sans:400,600,400italic,700,600italic,700italic,800,800italic,300italic,300";

            //bundles.Add(new ScriptBundle("~/Content/font", styleCdnPath).Include(
            //    "~/Content/font-google.css"
            //    ));

            bundles.Add(new StyleBundle("~/Content/StylePns").Include(
                "~/Content/assets/css/styles.css",
                "~/Content/assets/plugins/form-daterangepicker/daterangepicker-bs3.css",
                "~/Content/validationEngine.jquery.css",
                "~/Content/assets/plugins/pines-notify/pnotify.css",
                "~/Content/assets/plugins/form-tokenfield/bootstrap-tokenfield.css",
                "~/Content/assets/js/jqueryui.css",
                "~/Content/style-loading.css"
                ));

            bundles.Add(new ScriptBundle("~/Script/ScriptPns").Include(
                "~/Content/assets/js/jquery-1.10.2.min.js",
                "~/Content/assets/js/jqueryui-1.9.2.min.js",
                "~/Content/assets/js/bootstrap.min.js",
                "~/Content/assets/plugins/jquery-slimscroll/jquery.slimscroll.js",
                "~/Content/assets/js/enquire.min.js",
                "~/Content/assets/plugins/form-tokenfield/bootstrap-tokenfield.js",
                "~/Content/assets/js/application.js",
                "~/Content/assets/plugins/form-colorpicker/js/bootstrap-colorpicker.min.js",
                "~/Content/assets/plugins/form-daterangepicker/moment.min.js",
                "~/Content/assets/plugins/form-daterangepicker/daterangepicker.js",
                "~/Content/assets/plugins/bootstrap-datepicker/bootstrap-datepicker.js",
                "~/Content/assets/plugins/bootstrap-timepicker/bootstrap-timepicker.js",
                "~/Content/assets/plugins/bootstrap-datetimepicker/js/bootstrap-datetimepicker.js",
                "~/Content/assets/plugins/clockface/js/clockface.js",
                "~/Content/assets/plugins/pines-notify/pnotify.min.js",
                "~/Scripts/jquery.validationEngine.js",
                "~/Scripts/jquery.validationEngine-vi.js",
                "~/Content/assets/plugins/form-inputmask/jquery.inputmask.bundle.min.js",
                "~/Scripts/EMS/common.js",
                "~/Scripts/EMS/url.js",
                "~/Scripts/jquery.isloading.min.js",
                "~/Scripts/EMS/filter.js",
                "~/Scripts/EMS/Lading.js",
                "~/Scripts/EMS/Po.js",
                "~/Scripts/EMS/Order.js",
                "~/Scripts/EMS/Customers.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/EMS/Shipment.js",
                "~/Scripts/EMS/Report.js"
                ));

            //BundleTable.EnableOptimizations = true;
        }
    }
}