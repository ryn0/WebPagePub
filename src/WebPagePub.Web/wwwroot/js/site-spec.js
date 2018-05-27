$('.stackable-table').stacktable();
   

$("input[data-limit-input]").keyup(function () {
    var charLength = $(this).val().length;
    var charLimit = $(this).attr("data-limit-input");
    // Displays count
    $(this).next("span").html(charLength + " of " + charLimit + " characters used");
    // Alert when max is reached
    if ($(this).val().length > charLimit) {
        $(this).next("span").html("<strong>You may only have up to " + charLimit + " characters.</strong>");
    }
});