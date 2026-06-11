/*
***************************************************************************
© 2025 Siemens Product Lifecycle Management Software Inc.
This file should include all batchprocessing-defined javascript functions.
***************************************************************************
*/
var oldHub = null;

function scaleConnect(hubURL, subscriptionId, bpChannelAdapter, decimalSeparator) {
    if (typeof (oldHub) != 'undefined' && oldHub != null) {
        oldHub.server.unSubscribe();
    }

    $.getScript("scripts/jquery/jquery.signalR-2.2.0.min.js", function () {
        $.getScript(hubURL + "hubs", function () {
            connectToHubScript(hubURL, subscriptionId, bpChannelAdapter, decimalSeparator);
        });
    });
}

function connectToHubScript(hubURL, subscriptionId, bpChannelAdapter, decimalSeparator) {

    var eventSubscription =
    {
        correlationId: subscriptionId,
        messageTypeFilter: "*",
        adapterFilter: bpChannelAdapter
    };

    var hub = $.connection.cIOEventSignalRHub;

    oldHub = hub;

    if (hub == null) {
        showMessage("Couldn't connect to server");

        return;
    }

    hub.connection.error(function (error) {
        if (error)
            showMessage(error.message);
    });

    hub.connection.disconnected(function () {
        console.log("Connection lost.");

        setTimeout(function () {
            $.connection.hub.start()
                .done(function () {
                    clearEvent().pipe(clearStatus()).pipe(unsubscribe()).pipe(subscribe());
                    console.log("SignalR reconnected");
                }).fail(function () {
                    showMessage("Could not connect to SignalR hub.");
                }
                );
        }, 2000);
    });

    var scaleUOM = ""

    //event handler
    hub.client.newEvent = function (messageEvent, correlationIds) {

        var arrayLength = messageEvent.data.length;
        var weight = null;
        var sendType = null;
        var uom = null;
        var isTare = false;
        var tare = null;
        for (var i = 0; i < arrayLength; i++) {

            var obj = {};
            switch (messageEvent.data[i].name) {
                case "weight":
                    weight = messageEvent.data[i].value;
                    break;
                case "sendtype":
                    sendType = messageEvent.data[i].value;
                    break;
                case "uom":
                    uom = messageEvent.data[i].value;
                    break;
                case "tare":
                    isTare = true;
                    tare = messageEvent.data[i].value;
                    break;
            }

        }
        if (weight) {
            obj.Scale = weight;
            $("#ctl00_WebPartManager_MaterialsRequirementWP_ServiceDetails_IssueQty_ctl00").text(weight);
            if (sendType == "stable" && parseFloat(weight.toString().replace(decimalSeparator, ".")) > 0) {
                $("#ctl00_WebPartManager_MaterialsRequirementWP_ServiceDetails_IssueQty_ctl00").val(weight).change();
            }
            var scaleControl = $get("ctl00_WebPartManager_bpScaleWP_bpScaleDetail").control;
            if (scaleControl && uom) {
                scaleControl._uom = uom;
            }
            __page.setTagItemValues(obj);
        } else if (isTare) {
            $("#ctl00_WebPartManager_WeighInfoWP_HiddenTare_ctl00").val(tare + " " + uom).change();
            var reqUOM = $("#ctl00_WebPartManager_WeighInfoWP_IssueDetails_UOM").text();
            if (uom === null || reqUOM.toLowerCase() !== uom.toLowerCase()) {
                __page.getLabel('bpScaleUOMMisMatch', function (response) {
                    if ($.isArray(response))
                        __page.displayStatus(response[0].Value, "Warning"); // Scale UOM does not match requirement UOM.
                    else
                        __page.displayStatus("The Scales UOM is different than the material requirements UOM", "Warning");
                });
            }
        }
        return;
    }

    $.connection.hub.url = hubURL;

    function subscribe() {
        var d = $.Deferred();
        hub.server.subscribeToEvents(eventSubscription);
        return d.promise();
    }

    function unsubscribe() {
        var d = $.Deferred();

        hub.server.unSubscribe();
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

    function showMessage(messageText) {
        var d = $.Deferred();
        __page.displayStatus(messageText, "Warning", "Device Connection Status");
    }


    // Start the connection.
    $.connection.hub.start()
        .done(function () {
            clearEvent().pipe(clearStatus()).pipe(unsubscribe()).pipe(subscribe());
            console.log("SignalR connected");
        }).fail(function () {
            showMessage("Could not connect to SignalR hub.");
        }
        );

    $.connection.hub.start()
        .done(function () {
            clearEvent().pipe(clearStatus()).pipe(unsubscribe()).pipe(subscribe());
        }).fail(function () {
            showMessage("Could not connect!");
        }
        );


}