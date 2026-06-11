// Copyright Siemens 2023  


function getTypeSelectionElementsId(viewRadio, tableRadio) {
    viewRadioFieldId = viewRadio;
    tableRadioFieldId = tableRadio;
}

function getTableElementsId(isViewField) {
    isViewFieldId = isViewField;

}

function getScheduleElementsId(hoursField, startingAtHourField, specificDaysOfWeekField, specificDaysOfMonthField, specificMonthsField, startingAtDayField, everyDayField) {

    hoursFieldId = startingAtHourField; //hoursField;
    startingAtHourFieldId = hoursField; //startingAtHourField;

    specificDaysOfWeekFieldId = specificDaysOfWeekField;
    specificDaysOfMonthFieldId = specificDaysOfMonthField;
    specificMonthsFieldId = specificMonthsField;

    startingAtDayFieldId = startingAtDayField;
    everyDayFieldId = everyDayField;
}

function getHiddenScheduleElementsId(scheduleHoursField, scheduleDaysOfWeekField, scheduleDaysOfMonthField, scheduleMonthsField) {
    scheduleHoursFieldId = scheduleHoursField;
    scheduleDaysOfWeekFieldId = scheduleDaysOfWeekField;
    scheduleDaysOfMonthFieldId = scheduleDaysOfMonthField;
    scheduleMonthsFieldId = scheduleMonthsField;
}

function loadInitialData() {
    loadHours();
    loadDaysOfWeek();
    loadMonths();
    loadDaysOfMonth();
    loadIsView();
}

function loadHours() {
    setDefaultsHoursControl();
    var hours = $("#" + scheduleHoursFieldId)[0].value;
    var stringHours = $("#" + hoursFieldId)[0].value;
    if (stringHours == "24" && hours == "") $("#" + scheduleHoursFieldId)[0].value = "0";

    if (hours.length == 0) return;

    var selectedHours = hours.split(",");
    if (selectedHours.length == 0) return;

    $("#" + startingAtHourFieldId)[0].value = selectedHours[0];

    if (selectedHours.length > 1) {
        var hourStep = 24;
        if (selectedHours[1].length > 0) {
            var startHr = parseInt(selectedHours[0]);
            var nextHr = parseInt(selectedHours[1]);
            hourStep = nextHr - startHr;
        }
        $("#" + hoursFieldId)[0].value = hourStep.toString();
    }
}

function loadDaysOfWeek() {

    var daysOfWeekControls = $(".summary-table-days-of-week");
    setHandlersForElements(daysOfWeekControls, daysOfWeek_Changed);

    var daysString = $("#" + scheduleDaysOfWeekFieldId)[0].value;
    convertStringToCheckBoxesValues(daysString, daysOfWeekControls, specificDaysOfWeekFieldId);
}

function convertStringToCheckBoxesValues(stringValue, checkBoxes, specificCheckBoxId) {
    defaultHideCheckBoxes(checkBoxes, specificCheckBoxId);

    if (stringValue.length == 0)
        return;
    var elements = stringValue.split(",");

    var showCheckBoxes = false;
    for (var i = 0; i < elements.length; i++) {
        if (elements[i].length == 0)
            continue;

        $("input", checkBoxes[elements[i]-1]).prop("checked", true);
        showCheckBoxes = true;
    }

    if (showCheckBoxes) {
        $('#' + specificCheckBoxId)[0].checked = true;
        checkBoxes.css('display', defaultDisplayStyle);
    }
}

function loadDaysOfMonth() {

    var daysOfmonthsControls = $(".summary-table-days-of-month");
    setDefaultsForDaysOfMonthsControls(daysOfmonthsControls);

    var daysString = $("#" + scheduleDaysOfMonthFieldId)[0].value;
    if (daysString.length == 0) return;

    var selectedDays = daysString.split(",");
    if (selectedDays.length == 0) return;

    $("#" + startingAtDayFieldId)[0].value = selectedDays[0];
    if (selectedDays.length > 1) {
        var dayStep = 30;
        if (selectedDays[1].length > 0) {
            var startDay = parseInt(selectedDays[0]);
            var nextDay = parseInt(selectedDays[1]);
            dayStep = nextDay - startDay;
        }
        $("#" + everyDayFieldId)[0].value = dayStep.toString();
    }

    $('#' + specificDaysOfMonthFieldId)[0].checked = true;
    daysOfmonthsControls.css('display', defaultDisplayStyle);
}

function loadMonths() {
    var monthsControls = $(".summary-table-months");

    setHandlersForElements(monthsControls, months_Changed);
    var monthsString = $("#" + scheduleMonthsFieldId)[0].value;
    convertStringToCheckBoxesValues(monthsString, monthsControls, specificMonthsFieldId);
}

function setHandlersForElements(elements, handlerFunction) {
    for (var i = 0; i < elements.length; ++i) {
        elements[i].onclick = handlerFunction;
    }
}

function defaultHideCheckBoxes(checkBoxes, specificCheckBoxId) {

    $('#' + specificCheckBoxId)[0].checked = false;
    checkBoxes.css('display', "none");
    checkBoxes.children().attr("checked", false);
}

function setDefaultsForDaysOfMonthsControls(daysOfmonthsControls) {

    $('#' + specificDaysOfMonthFieldId)[0].checked = false;
    daysOfmonthsControls.css('display', "none");

    var stDay = $('#' + startingAtDayFieldId)[0];
    var everyDay = $('#' + everyDayFieldId)[0];
    stDay.onchange = daysOfMonths_Changed;
    stDay.value = defaultDayValue;
    everyDay.onchange = daysOfMonths_Changed;
    everyDay.value = defaultDayValue;
}

function setDefaultsHoursControl() {
    var stHour = $('#' + startingAtHourFieldId)[0];
    var everyHour = $('#' + hoursFieldId)[0];
    stHour.onchange = hours_Changed;
    stHour.value = defaultStHourValue;
    everyHour.onchange = hours_Changed;
    everyHour.value = defaultHourValue;
}

function loadIsView() {

    var isView = $('#' + isViewFieldId)[0].checked;

    $('#' + viewRadioFieldId)[0].checked = !isView;
    $('#' + tableRadioFieldId)[0].checked = isView;

    setDisplayForScheduleControls(!isView);

}

function setIsView(isView) {
    var isViewControl = $('#' + isViewFieldId)[0];
    isViewControl.checked = !isView;
    setDisplayForScheduleControls(isView);

}

function setIsManual(val) {
    setDisplayForScheduleControls(val);
}

function setDisplayForScheduleControls(hide) {
    $(".summary-table-schedule-elements").css('display', (hide) ? "none" : defaultDisplayStyle)
        .css('visibility', (hide) ? "hidden" : defaultDisplayStyle);
    $(".reccurence-pattern-section").css('display', (hide) ? "none" : defaultDisplayStyle);
    $(".right-checkbox").css('display', (hide) ? "none" : defaultDisplayStyle);
    $(".hourly-pattern").css('display', (hide) ? "none" : defaultDisplayStyle);
    setDisplayForDaysOfWeek(!hide && $('#' + specificDaysOfWeekFieldId)[0].checked);
    setDisplayForDaysOfMonth(!hide && $('#' + specificDaysOfMonthFieldId)[0].checked);
    setDisplayForMonths(!hide && $('#' + specificMonthsFieldId)[0].checked);

    $(".resurrence-pattern").css('display', (hide) ? defaultDisplayStyle : "none");
    $(".cs-title").css('margin-top', (hide) ? "0px" : "15px");

    $("#ctl00_WebPartManager_SchedulingGroupWP_StartingAtHourControl").children(".cs-label").css('margin-left', '0px');
}

function setDisplayForDateControls(hide) {
    $('#' + startDateControlId)[0].parentElement.style["display"] = hide ? "none" : defaultDisplayStyle;
    $('#' + endDateControlId)[0].parentElement.style["display"] = hide ? "none" : defaultDisplayStyle;
}

function hours_Changed() {
    var startingHour = parseInt($('#' + startingAtHourFieldId)[0].value);
    var hours = parseInt($('#' + hoursFieldId)[0].value);

    if ($('#' + hoursFieldId)[0].value == "" && $('#' + startingAtHourFieldId)[0].value == "") {
        putValueIntoHiddenField("0", scheduleHoursFieldId);
        return false;
    }

    if (startingHour < 0) startingHour = 0;
    if (startingHour > 23) startingHour = 0;
    if (hours < 1) hours = 24;
    if (hours > 24) hours = 24;

    var hourStr = "";
    for (var hour = startingHour; hour < 24; hour += hours) {
        hourStr += hour.toString() + ",";
    }

    putValueIntoHiddenField(hourStr.substring(0, hourStr.length - 1), scheduleHoursFieldId);
}

function daysOfWeek_Changed() {
    var dayStr = "";
    var daysOfWeekControls = $(".summary-table-days-of-week");
    for (var i = 0; i < daysOfWeekControls.length; i++) {
        if ($("input", daysOfWeekControls[i]).prop("checked"))
            dayStr += (i + 1) + ",";
    }

    putValueIntoHiddenField(dayStr.substring(0, dayStr.length - 1), scheduleDaysOfWeekFieldId);
}

function months_Changed() {
    var monthsStr = "";
    var monthsControls = $(".summary-table-months");
    for (var i = 0; i < monthsControls.length; i++) {
        if ($("input", monthsControls[i]).prop("checked"))
            monthsStr += (i + 1) + ",";
    }
    //remove the comma at the end of the line    
    putValueIntoHiddenField(monthsStr.substring(0, monthsStr.length - 1), scheduleMonthsFieldId);
}

function daysOfMonths_Changed() {

    var startingDay = parseInt($("#" + startingAtDayFieldId)[0].value);
    var days = parseInt($("#" + everyDayFieldId)[0].value);
    if (startingDay > 31) startingDay = 1;
    if (startingDay < 1) startingDay = 1;
    if (days > 31) days = 1;
    if (days < 1) days = 1;

    var daysStr = "";
    for (var day = startingDay; day <= 31; day += days)//31 days
    {
        daysStr += day.toString() + ",";
    }

    putValueIntoHiddenField(daysStr.substring(0, daysStr.length - 1), scheduleDaysOfMonthFieldId);
}

function putValueIntoHiddenField(string, fieldId) {

    var field = $('#' + fieldId)[0];
    if (field) {
        field.value = string;
    }
}

function setDisplayForDaysOfWeek(checked, control) {
    if (!checked && typeof control !== 'undefined') {
        //clear all checkboxes
        var daysOfWeekControls = $(".summary-table-days-of-week");
        for (var i = 0; i < daysOfWeekControls.length; i++) {
            daysOfWeekControls[i].childNodes[0].checked = false;
        }
        $("#" + scheduleDaysOfWeekFieldId)[0].value = "";
    }

    $(".summary-table-days-of-week").css('display', (checked) ? defaultDisplayStyle : "none");
    $(".days-recurrence").css('display', (checked) ? defaultDisplayStyle : "none");
}

function setDisplayForDaysOfMonth(checked, control) {
    if (!checked && typeof control !== 'undefined') {
        $(".summary-table-days-of-month").css('display', "none");
        $("#" + scheduleDaysOfMonthFieldId)[0].value = "";
        var daysOfmonthsControls = $(".summary-table-days-of-month");
        setDefaultsForDaysOfMonthsControls(daysOfmonthsControls);
        return false;
    }
    //save default value
    if (typeof control !== 'undefined') {
        var startingDay = parseInt($("#" + startingAtDayFieldId)[0].value);
        var days = parseInt($("#" + everyDayFieldId)[0].value);
        if (startingDay == 1 && days == 1)
            $("#" + scheduleDaysOfMonthFieldId)[0].value = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31";
    }
    $(".summary-table-days-of-month").css('display', (checked) ? defaultDisplayStyle : "none");
}

function setDisplayForMonths(checked, control) {
    if (!checked && typeof control !== 'undefined') {
        $("#" + scheduleMonthsFieldId)[0].value = "";
        //clear all checkboxes
        var monthsControls = $(".summary-table-months");
        for (var i = 0; i < monthsControls.length; i++) {
            monthsControls[i].childNodes[0].checked = false;
        }
    }

    $(".summary-table-months").css('display', (checked) ? defaultDisplayStyle : "none");
    $(".months-recurrence").css('display', (checked) ? defaultDisplayStyle : "none");
}

var viewRadioFieldId = null;
var tableRadioFieldId = null;
var isViewFieldId = null;

var hoursFieldId = null;
var startingAtHourFieldId = null;

var specificDaysOfWeekFieldId = null;
var dayFieldIds = null;

var specificDaysOfMonthFieldId = null;
var specificMonthsFieldId = null;

var startingAtDayFieldId = null;
var everyDayFieldId = null;

var scheduleHoursFieldId = null;
var scheduleDaysOfWeekFieldId = null;
var scheduleDaysOfMonthFieldId = null;
var scheduleMonthsFieldId = null;

var startDateControlId = null;
var endDateControlId = null;

var defaultDayValue = "1";
var defaultHourValue = "24";
var defaultStHourValue = "0";
var defaultDisplayStyle = "";
