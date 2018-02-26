$(document).on("beforeAjaxSend.ic", function(event, ajaxSetup, elt) {
    delete ajaxSetup.data;
});