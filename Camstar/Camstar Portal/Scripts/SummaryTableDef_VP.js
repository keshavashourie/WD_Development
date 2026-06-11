// Copyright Siemens 2023  

function getTypeSelectionElementsId(viewRadio, tableRadio, viewControl, tableControl) {
    viewRadioFieldId = viewRadio;
    tableRadioFieldId = tableRadio;
    viewControlId = viewControl;
    tableControlId = tableControl;
}

function getTableElementsId(isViewField, appendToTableField, isManuallyExecutedField, tableNameTypeLabel) {
    isViewFieldId = isViewField;
    appendToTableControlId = appendToTableField;
    isManuallyExecutedControlId = isManuallyExecutedField;
    tableNameTypeLabelId = tableNameTypeLabel;
}

function getScheduleElementsId(isEnabledField, hoursField, startingAtHourField, specificDaysOfWeekField, specificDaysOfMonthField, specificMonthsField, startingAtDayField, everyDayField) {
    isEnabledControlId = isEnabledField;
    hoursFieldId = hoursField;
    startingAtHourFieldId = startingAtHourField;

    specificDaysOfWeekFieldId = specificDaysOfWeekField;
    specificDaysOfMonthFieldId = specificDaysOfMonthField;
    specificMonthsFieldId = specificMonthsField;

    startingAtDayFieldId = startingAtDayField;
    everyDayFieldId = everyDayField;
}

function getDateElementsId(startDateField, endDateField) {
    startDateControlId = startDateField;
    endDateControlId = endDateField;
}

function getHiddenScheduleElementsId(scheduleHoursField, scheduleDaysOfWeekField, scheduleDaysOfMonthField, scheduleMonthsField) {
    scheduleHoursFieldId = scheduleHoursField;
    scheduleDaysOfWeekFieldId = scheduleDaysOfWeekField;
    scheduleDaysOfMonthFieldId = scheduleDaysOfMonthField;
    scheduleMonthsFieldId = scheduleMonthsField;
}

function loadInitialData() {
    defaultDisplayStyle = $('#' + isEnabledControlId)[0].style["display"];
    loadHours();
    loadDaysOfWeek();
    loadMonths();
    loadDaysOfMonth();
    loadIsView();
    loadIsManuallyExecuted();
    setTablePrefix();
}

function loadHours() {
    var hours = $("#" + scheduleHoursFieldId)[0].value;
    var stringHours = $("#" + hoursFieldId)[0].value;

    if (hours == "")
        setDefaultHours();

    if (stringHours == "24" && hours == "") $("#" + scheduleHoursFieldId)[0].value = "0,";

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
        
        $("input", checkBoxes[elements[i] - 1]).prop("checked", true);
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

    $("#" + everyDayFieldId)[0].value = selectedDays[0];
    if (selectedDays.length > 1) {
        var dayStep = 31;
        if (selectedDays[1].length > 0) {
            var startDay = parseInt(selectedDays[0]);
            var nextDay = parseInt(selectedDays[1]);
            dayStep = nextDay - startDay;
        }
        $("#" + startingAtDayFieldId)[0].value = dayStep.toString();
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

function setDefaultHours()
{
    var stHour = $('#' + startingAtHourFieldId)[0];
    var everyHour = $('#' + hoursFieldId)[0];
    stHour.value = defaultStartingAtHourValue;
    stHour.onchange = hours_Changed;
    everyHour.value = defaultEveryHourValue;
    everyHour.onchange = hours_Changed;
    hours_Changed();
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

function loadIsView() {
    var isView = $('#' + isViewFieldId)[0].checked;
    
    $('#' + viewRadioFieldId)[0].checked = isView;
    $('#' + tableRadioFieldId)[0].checked = !isView;
    
    setDisplayForTableControls(isView);
    
    setTablePrefix();
}

function loadIsManuallyExecuted() {
    var manualChecked = $("input", '#' + isManuallyExecutedControlId).prop("checked");
    var viewChecked = $('#' + isViewFieldId)[0].checked;

    setDisplayForScheduleControls((viewChecked || manualChecked));
}

function setIsView(isView) {
    var isViewControl = $('#' + isViewFieldId)[0];
    isViewControl.checked = isView;
    CR.Page.setCheckBoxChecked(viewControlId, isView);
    CR.Page.setCheckBoxChecked(tableControlId, !isView);

    setDisplayForTableControls(isView);

    setTablePrefix();
}

function formatString() {
    var s = arguments[0];

    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }

    return s;
}

function setCheckBoxChecked(spanId, checked) {
    // Classic requires the nested <input> to also be marked checked
    var $checkElems = $(formatString("#{0}, #{0} > input", spanId));

    // fixed View Mode radio button option, replace attr with prop
    if (checked) {
        $checkElems.prop('checked', true);
        $checkElems.attr('checked', 'checked');
    } else {
        $checkElems.prop('checked', false);
        $checkElems.removeAttr('checked');
    }
}

function setIsManual(val) {
    setDisplayForScheduleControls(val);
}

function setTablePrefix() {
    var isView = $('#' + isViewFieldId)[0].checked;
    var label = $('#' + tableNameTypeLabelId)[0].lastChild;
    if (isView) {
        label.data = "csiView_";
    }
    else {
        label.data = " csiTbl_";
    }
}

function setDisplayForTableControls(isView) {
    var manualChecked = $("input", '#' + isManuallyExecutedControlId).prop("checked");

    $('#' + appendToTableControlId).css('cssText', (isView) ? "display:none !important" : defaultDisplayStyle);
    $('#' + isManuallyExecutedControlId).css('cssText', (isView) ? "display:none !important" : defaultDisplayStyle);
    $('#' + isEnabledControlId).css('cssText', (isView) ? "display:none !important" : defaultDisplayStyle);
    
    setDisplayForScheduleControls(isView || manualChecked);
}

function setDisplayForScheduleControls(hide) {
    $(".summary-table-schedule-elements").css('display', (hide) ? "none" : defaultDisplayStyle)
        .css('visibility', (hide) ? "hidden" : defaultDisplayStyle);
    $(".reccurence-pattern-section").css('display', (hide) ? "none" : defaultDisplayStyle);
    $(".right-checkbox").css('display', (hide) ? "none" : defaultDisplayStyle);
    
    setDisplayForDaysOfWeek(!hide && $('#' + specificDaysOfWeekFieldId)[0].checked);
    setDisplayForDaysOfMonth(!hide && $('#' + specificDaysOfMonthFieldId)[0].checked);
    setDisplayForMonths(!hide && $('#' + specificMonthsFieldId)[0].checked);

    setDisplayForDateControls(hide);
}

function setDisplayForDateControls(hide) {
    $('#' + startDateControlId)[0].parentElement.style["display"] = hide ? "none" : defaultDisplayStyle;
    $('#' + endDateControlId)[0].parentElement.style["display"] = hide ? "none" : defaultDisplayStyle;
}

function hours_Changed() {
    var startingHour = parseInt($('#' + startingAtHourFieldId)[0].value);
    var hours = parseInt($('#' + hoursFieldId)[0].value);
    
    if (startingHour < 0 || startingHour > 23) startingHour = 0;
    if (hours < 1 || hours > 24) hours = 24;

    var hourStr = "";
    for (var hour = startingHour; hour < 24; hour += hours) {
        hourStr += hour.toString() + ",";
    }

    putValueIntoHiddenField(hourStr, scheduleHoursFieldId);
}

function daysOfWeek_Changed() {
    var dayStr = "";
    var daysOfWeekControls = $(".summary-table-days-of-week"); 
    for (var i = 0; i < daysOfWeekControls.length; i++) {
        if ($("input", daysOfWeekControls[i]).prop("checked"))
            dayStr += (i+1) + ",";
    }

    putValueIntoHiddenField(dayStr, scheduleDaysOfWeekFieldId);
}

function months_Changed() {
    var monthsStr = "";
    var monthsControls = $(".summary-table-months");
    for (var i = 0; i < monthsControls.length; i++) {
        if ($("input", monthsControls[i]).prop("checked"))
            monthsStr += (i+1) + ",";
        
    }

    putValueIntoHiddenField(monthsStr, scheduleMonthsFieldId);
}

function daysOfMonths_Changed() {
    var startingDay = parseInt($("#" + startingAtDayFieldId)[0].value);
    var days = parseInt($("#" + everyDayFieldId)[0].value);
    if (startingDay > 31) startingDay = 31;
    if (startingDay < 1) startingDay = 1;
    if (days > 31) days = 31;
    if (days < 1) days = 1;

    var daysStr = "";
    for (var day = days; day <= 31; day += startingDay) {
        daysStr += day.toString() + ",";
    }
    
    putValueIntoHiddenField(daysStr, scheduleDaysOfMonthFieldId);
}

function putValueIntoHiddenField(string, fieldId) {
    var field = $('#' + fieldId)[0];
    if (field) {
        field.value = string;
    }
}

function setDisplayForDaysOfWeek(checked) {
    $(".summary-table-days-of-week").css('display', (checked) ? defaultDisplayStyle : "none");
    $(".days-recurrence").css('display', (checked) ? defaultDisplayStyle : "none");
    if ($('#' + specificDaysOfWeekFieldId)[0].checked == false)
    {
        $("#" + scheduleDaysOfWeekFieldId)[0].value = "";      
    }
}

function setDisplayForDaysOfMonth(checked) {
    $(".summary-table-days-of-month").css('display', (checked) ? defaultDisplayStyle : "none");
    if ($('#' + specificDaysOfMonthFieldId)[0].checked)
    {
        daysOfMonths_Changed();
    }
    else
    {
        $("#" + scheduleDaysOfMonthFieldId)[0].value = "";
    }

}

function setDisplayForMonths(checked) {
    $(".summary-table-months").css('display', (checked) ? defaultDisplayStyle : "none");
    $(".months-recurrence").css('display', (checked) ? defaultDisplayStyle : "none");
    if ($('#' + specificMonthsFieldId)[0].checked == false)
    {        
        $("#" + scheduleMonthsFieldId)[0].value = "";
    }
 }

var viewRadioFieldId = null;
var tableRadioFieldId = null;
var isViewFieldId = null;
var viewControlId = null;
var tableControlId = null;

var tableNameTypeLabelId = null;
var appendToTableControlId = null;
var isManuallyExecutedControlId = null;

var isEnabledControlId = null;
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
var defaultEveryHourValue = "24";
var defaultStartingAtHourValue = "0";

var defaultDisplayStyle = "inline";
