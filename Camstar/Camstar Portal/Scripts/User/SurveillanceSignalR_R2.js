/*
***************************************************************************
Copyright 2019 Siemens
This file should include all user-defined javascript functions. 
Functions defined in this file will over-ride any functions of the
same name written in any of the Camstar supplied javascript files.
***************************************************************************
*/

function SurveillanceStartup(ServerURL, correlationId, SurvCIOChannelAdapter) {
    $.getScript("scripts/jquery/jquery.signalR-2.2.0.min.js", function () {
        $.getScript(ServerURL + "hubs", function () {
            ConnectToHubScript(ServerURL, correlationId, SurvCIOChannelAdapter);
        }).fail(function () {
            showMessage("Unable Connect Hub");
        });
    });
}

function ConnectToHubScript(ServerURL, correlationId, SurvCIOChannelAdapter) {
    var eventSubscription =
        {
            correlationId: correlationId,
            messageTypeFilter: "*",
            adapterFilter: SurvCIOChannelAdapter
        }

    var hub = $.connection.cIOEventSignalRHub;

    if (hub == null) {
        showMessage("Unable to connect to server");

        return;
    }

    hub.connection.error(function (error) {
        if (error)
            showMessage("Error: " + error.message);
    });

    hub.connection.disconnected(function () {
        showMessage("Connection lost.", 4000, true);

        setTimeout(function () {
            $.connection.hub.start();
        }, 2000);
    });

    var resultGrid = jQuery('#ctl00_WebPartManager_SS_WIPSignalRSurvR2_ResultWP_SurveillanceStatusDetails');

    //event handler
    hub.client.newEvent = function (messageEvent, correlationIds) {
        var eventMessage = "name: " + messageEvent.name + " id: " + messageEvent.id + " request id: " + messageEvent.requestid + " status: " + messageEvent.statusmessage + " type: " + messageEvent.type;
        var MessageEventDataLength = messageEvent.data.length;
        resultGrid.empty();

        var RowCount = 0;
        var timeCount = 0;
        var tr = "";

        for (var i = 0; i < MessageEventDataLength; i++) {
            var jsonObj = $.parseJSON(messageEvent.data[i].value);
            if (i == 0) {
                var SignalRSurvStatuses = jsonObj.Resource;
                //var FilterCriteriaValue = ResourceCriteria;
            }
            else if (i == 1) {
                var SignalRSurvStatuses = jsonObj.ResourceGroup;
                //var FilterCriteriaValue = ResourceGroupCriteria;
            }
            else if (i == 2) {
                var SignalRSurvStatuses = jsonObj.Operation;
                //var FilterCriteriaValue = OperationCriteria;
            }
            else if (i == 3) {
                var SignalRSurvStatuses = jsonObj.WorkCenter;
                //var FilterCriteriaValue = WorkCenterCriteria;
            }

            if (SignalRSurvStatuses != "") {
                var itemCriteria = messageEvent.data[i].name;

                //if ( FilterCriteria == "" || FilterCriteria.replace(/\s+/, "").toLowerCase() === itemCriteria.toLowerCase()) {  
                if (SignalRSurvStatuses.SignalRSurvStatus.constructor === Array) {
                    var SignalRSurvStatus = SignalRSurvStatuses.SignalRSurvStatus;
                }
                else {
                    var SignalRSurvStatus = [SignalRSurvStatuses.SignalRSurvStatus];
                }

                for (var j = 0; j < SignalRSurvStatus.length ; j++) {
                    //if ( FilterCriteriaValue == "" || FilterCriteriaValue === SignalRSurvStatus[j].ss_Criteria) {
                    //calculate Remaining lot percentage
                    if (SignalRSurvStatus[j].ss_SurveillanceType == 'Lot Surveillance') {
                        var LotCount = SignalRSurvStatus[j].ss_LotCount;
                        var NextLotCountDue = SignalRSurvStatus[j].ss_NextLotCountDue;
                        var RemainingLotPercent = LotCount + '/' + NextLotCountDue + '\t(' + Math.round((LotCount / NextLotCountDue) * 100) + '%)';
                        var RemainingDays = "";
                    }
                    else {
                        var RemainingLotPercent = "";
                        var RemainingDays = SignalRSurvStatus[j].ss_NextDateDue;
                    }

                    var rowData =
                        {
                            _id_column: String("000000" + RowCount).slice(-6),
                            SurveillanceName: SignalRSurvStatus[j].ss_SurveillanceDisplayName,
                            SurveillanceType: SignalRSurvStatus[j].ss_SurveillanceType,
                            RemainingDays: RemainingDays,
                            RemainingLotPercent: RemainingLotPercent,
                            criteria: itemCriteria,
                            criteriavalue: SignalRSurvStatus[j].ss_Criteria
                        };

                    resultGrid.jqGrid('addRowData', rowData._id_column, rowData, 'last');

                    var selectedRow = document.getElementById(rowData._id_column);
                    var td = selectedRow.getElementsByTagName("td");
                    td[1].style.width = '330px';
                    td[2].style.width = '240px';
                    td[3].style.width = '240px';
                    td[4].style.width = '240px';

                    try {
                        var rowClass;
                        switch (SignalRSurvStatus[j].ss_SurveillanceState) {
                            case 'Past Due':
                                rowClass = 'red';
                                break;
                            case 'Due':
                                rowClass = 'orange';
                                break;
                            case 'Pending':
                                rowClass = 'yellow';
                                break;
                            default:
                                rowClass = '';
                        }

                        if (rowClass != '') {
                            var selectedRow = $('#' + rowData._id_column);
                            selectedRow.addClass('ui-jqgrid-row-' + rowClass);

                        }
                    } catch (ex) { }

                    if (rowData.SurveillanceType == "Time Surveillance") {
                        td[3].id = 'timecell' + timeCount;
                        initializeCountDown('timecell' + timeCount);
                        timeCount++;
                    }
                    RowCount++;
                    //}
                }
                //}
            }
        }

        filterGrid();

        return;
    }

    $.connection.hub.url = ServerURL;

    function subscribe() {
        var d = $.Deferred();
        hub.server.subscribeToEvents(eventSubscription);
        return d.promise();
    }

    function clearEvent() {
        var d = $.Deferred();
        return d.promise();
    }

    function clearStatus() {
        var d = $.Deferred();
        return d.promise();
    }

    function unsubscribe() {
        var d = $.Deferred();
        hub.server.unSubscribe();
        return d.promise();
    }

    // Start the connection.
    $.connection.hub.start()
        .done(function () {

            clearEvent().pipe(clearStatus()).pipe(unsubscribe()).pipe(subscribe()).pipe(showMessage("Subscribed"));

        }).fail(function () {
            alert("Could not connect!");
        });

    function getTimeRemaining(endtime) {
        var enddatearray = endtime.split(" ");
        var endtimearray = enddatearray[3].split(":");
        var d1= new Date();
        var t = Date.UTC(enddatearray[0], enddatearray[1], enddatearray[2], endtimearray[0], endtimearray[1], endtimearray[2]) - Date.UTC(d1.getUTCFullYear(), d1.getUTCMonth() + 1, d1.getUTCDate(), d1.getHours(), d1.getMinutes(), d1.getSeconds());
        if (t < 0) {
            t = 0;
        }

        var seconds = Math.floor((t / 1000) % 60);
        var minutes = Math.floor((t / 1000 / 60) % 60);
        var hours = Math.floor((t / (1000 * 60 * 60)) % 24);
        var days = Math.floor(t / (1000 * 60 * 60 * 24));
        return {
            'total': t,
            'days': days,
            'hours': hours,
            'minutes': minutes,
            'seconds': seconds
        };
    }

    function initializeCountDown(id) {
        var timeCellElement = document.getElementById(id);
        var dueDate = getDateTime(timeCellElement.title);

        function updateClock() {
            var t = getTimeRemaining(dueDate);

            timeCellElement.innerHTML = ('000' + t.days).slice(-3) + ' Days ' + ('0' + t.hours).slice(-2) + ': ' + ('0' + t.minutes).slice(-2) + ': ' + ('0' + t.seconds).slice(-2) + '';

            if (t.total <= 0) {
                clearInterval(timeinterval);
            }
        }

        function getDateTime(dateTime) {
            var a = dateTime.split(" ");
            var b = a[0].split("/");
            var result = b[0] + ' ' + b[1] + ' ' + b[2] + ' ' + a[1];

            return result;
        }

        updateClock();
        var timeinterval = setInterval(updateClock, 1000);
    }
}

function showMessage(messageText) {
    var d = $.Deferred();
    $("#ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_SubscriptionStatus_ctl00").val(messageText);
}

function filterGrid() {
    var FilterCriteria = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_FilterCriteria_Edit').value;
    var Status = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_SubscriptionStatus_ctl00').value;
    var RowCount = 0;

    if (Status != "") {
        if (FilterCriteria == "Resource")
            var selectedCriteriaValue = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_ss_GetSurveillanceStatuses_ss_Resource_Edit').value;
        else if (FilterCriteria == "Resource Group")
            var selectedCriteriaValue = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_ss_GetSurveillanceStatuses_ss_ResourceGroup_Edit').value;
        else if (FilterCriteria == "Operation")
            var selectedCriteriaValue = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_ss_GetSurveillanceStatuses_ss_Operation_Edit').value;
        else if (FilterCriteria == "Work Center")
            var selectedCriteriaValue = document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_ss_GetSurveillanceStatuses_ss_WorkCenter_Edit').value;

        var resultGrid = jQuery('#ctl00_WebPartManager_SS_WIPSignalRSurvR2_ResultWP_SurveillanceStatusDetails');

        resultGrid.find('tr').each(function (rowIndex, r) {
            if (FilterCriteria != "") {
                if ($(this).find('td')[5].innerHTML == FilterCriteria.replace(/\s+/, "")) {
                    if ($(this).find('td')[6].innerHTML == selectedCriteriaValue || selectedCriteriaValue == "") {
                        $(this)[0].style.display = "";
                        RowCount++;
                    }
                    else
                        $(this)[0].style.display = "none";
                }
                else {
                    $(this)[0].style.display = "none";
                }
            }
            else {
                $(this)[0].style.display = "";
                RowCount++;
            }
        });

		/*maintain visibleRow
		while (RowCount < 7) {
			var rowData = {
				_id_column: String("#empty#" + RowCount).slice(-6),
				SurveillanceName: "",
				SurveillanceType: "",
				RemainingDays: "",
				RemainingLotPercent: ""
			};
			resultGrid.jqGrid('addRowData', rowData._id_column, rowData, 'last');
			var selectedRow = document.getElementById(rowData._id_column);
			var td = selectedRow.getElementsByTagName("td");
			td[1].style.width = '209px';
			RowCount++;
		}*/
    }
}

function clearAllBtnClick() {
    document.getElementById('ctl00_WebPartManager_SS_WIPSignalRSurvR2_SearchWP_FilterCriteria_Edit').value = "";
    filterGrid();
}