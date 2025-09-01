$(document).ready(function() {
    $(".form-group .datepicker").each(
        function() {
            var dateFormat = $(this).data("date-format");
            console.log("Date format: " + dateFormat);
            if ( dateFormat != undefined ) {
                $(this).datetimepicker({
                    format: dateFormat
                });
            }
            else {
                console.log("Using default date format...");
                $(this).datetimepicker({
                    format: "YYYY-MM-DD LT" // TODO: Find a better way to define default format so it works for all locales
                });
            }

        }
    );
    $("form[data-toggle='validator']").each(
        function() {
            $(this).submit(function(event) {

                $(this).find(".datepicker").each(
                    function() {
                        var dateField = $(this).find("input").first();
                        if ( dateField.val() ) {
                            var date = $(this).data("DateTimePicker").date();
                            var isoDate;
                            if (date % 10000 == 0) {

                                function pad(number) {
                                    var r = String(number);
                                    if (r.length === 1) {
                                        r = '0' + r;
                                    }
                                    return r;
                                }

                                // No time specified
                                //
                                var d = new Date(date);
                                isoDate = d.getFullYear()
                                    + '-' + pad(d.getMonth() + 1)
                                    + '-' + pad(d.getDate())
                                    + 'T00:00:00.000Z';
                            }
                            else {
                                isoDate = date.toISOString();
                            }
                            dateField.val(isoDate);
                            $(this).find("input").css("color", "#ecebeb");
                        }
                    }
                );
                // TODO: Rename to .integration-form-submit-button
                $(this).find(".crm-form-submit-button").append("&nbsp;<span class='glyphicon glyphicon-refresh spinning'></span>");

                return true;
            });
        }
    );

    setTimeout(function() {
        if (typeof Tridion != "undefined") {
            if (Tridion.Type.resolveNamespace("Tridion.Web.UI.SiteEdit.Page") || Tridion.Type.resolveNamespace("Tridion.Web.UI.Editors.XPMCore.Controls.Page")) {

                // Disable validations in XPM mode
                //
                $("form[data-toggle='validator']").each(
                    function() {
                        $(this).validator('destroy');
                        $(this).removeAttr("action");
                    }
                );
                $("input.form-control[required]").each(
                    function() {
                        $(this).removeAttr("required");
                    }
                );
                $("button[type='submit']").each(
                    function() {
                        $(this).hide();
                        $(this).next().show();
                    }
                );
            }
        }
    },1000);

});
