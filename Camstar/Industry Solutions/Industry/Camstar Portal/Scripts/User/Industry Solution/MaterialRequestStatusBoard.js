// Copyright Siemens 2020  

var isMaterialRequestStatusBoard = (function () {
    'use strict';

    var _oldHub = null;
    var _eventIds = [];
    var _errorCount = 0;
    var SELECTORS = {
        BUTTON_UPDATE: '#ctl00_WebPartManager_BlankWP_ButtonUpdate'
    };
    Object.freeze(SELECTORS);

    var MESSAGE_TYPES = {
        ACKNOWLEDGE: "is_materialrequestacknowledge",
        ES_ACKNOWLEDGE: "es_materialrequestacknowledge",
        ES_REQUEST: "es_materialrequest",
        REQUEST: "is_materialrequest",
        STATUS: "is_materialrequeststatus"
    };
    Object.freeze(MESSAGE_TYPES);

    //  THis is the function that is called when an event is triggered
    function handleRequest(messageEvent, correlationIds) {
        var arrayLength = messageEvent.data.length;
        if (_eventIds.includes(messageEvent.id) === false) {
            var obj = {};
            
            for (var i = 0; i < arrayLength; i++) {

                switch (messageEvent.data[i].name) {
                    case "Qty":
                        var qty = messageEvent.data[i].value;

                        break;
                    case "UOM":
                        var uom = messageEvent.data[i].value;
                        break;

                    case "Product":
                        var producct = messageEvent.data[i].value;

                        break;
                    case "MaterialQueue":
                        var materialQueue = messageEvent.data[i].value;

                        break;

                    case "MaterialRequestId":
                        var requestId = messageEvent.data[i].value;

                        break;

                    case "Resource":
                        var materialQueue = messageEvent.data[i].value;

                        break;

                    case "RequestQty":
                        var materialQueue = messageEvent.data[i].value;

                        break;
                    case "ReceivedQty":
                        var materialQueue = messageEvent.data[i].value;

                        break;
                    case "Container":
                        var materialQueue = messageEvent.data[i].value;

                        break;
                }
            }
            __page.setTagItemValues(obj);
            _eventIds.push(messageEvent.id);

            //  This will trigger the click on a button to update the page - this would only be done if the tiles can't be modified/created in Javascript
            $(SELECTORS.BUTTON_UPDATE).click();
        }
    }

    function connectToSignalRHub(signalRHostUrl, subscriptionId) {
        signalRHostUrl = signalRHostUrl || "https://localhost:8096/ChannelSourceCtl/miochannelsource/SignalR/";
        signalRConnect(signalRHostUrl, subscriptionId, "is_valorpost,is_valorinbound,es_valorpost,es_valorinbound", handleRequest);
    }

    function signalRConnect(hubURL, subscriptionId, channelAdapter, callback) {
        if (typeof (_oldHub) != 'undefined' && _oldHub != null) {
            _oldHub.server.unSubscribe();
        }

        $.getScript("scripts/jquery/jquery.signalR-2.2.0.min.js", function () {
            $.getScript(hubURL + "hubs", function () {
                connectToHubScript(hubURL, subscriptionId, channelAdapter, callback);
            }).fail(function (jqxhr, settings, exception) {
                __page.displayStatus("Could not connect to SignalR hub.", "Warning", "Device Connection Status");
            });
        });
    }

    function connectToHubScript(hubURL, subscriptionId, channelAdapter, callback) {

        let list = channelAdapter.split(",");
        var eventSubscription =
                {
                    correlationId: subscriptionId,
                    messageTypeFilter: "*",
                    adapterFilter: channelAdapter
                };

        var hub = $.connection.cIOEventSignalRHub;

        _oldHub = hub;

        if (hub == null) {
            showMessage("Couldn't connect to server");

            return;
        }

        hub.connection.error(function (error) {
            if (error && _errorCount === 0)
                showMessage(error.message);
        });

        hub.connection.disconnected(function () {
            console.log("Connection lost.");

            setTimeout(function () {
                $.connection.hub.start()
                    .done(function () {
                        clearEvent().pipe(clearStatus()).pipe(unsubscribe()).pipe(subscribe());
                        console.log("SignalR reconnected");
                        if (_errorCount > 5)
                            showMessage("SignalR reconnected");
                        _errorCount = 0;
                    }).fail(function () {
                        _errorCount++;
                        if (_errorCount % 5 === 0)
                            showMessage("Could not connect to SignalR hub.");
                    }
                    );
            }, 2000);
        });

        //event handler
        hub.client.newEvent = function (messageEvent, correlationIds) {
            callback(messageEvent, correlationIds);
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


    }

	function setResource(objectTypeId, instanceId, wp_id, swacComponentName) {
		var objectTypeControl = $find(objectTypeId);
		var instanceIdControl = $find(instanceId);

		var instanceName = instanceIdControl._editor.value;
		if (instanceName) {
			var assetNumber = objectTypeControl._value.value + "-" + instanceIdControl._value.value;
			displaySwacWP(wp_id, true);

			var cur = SWAC.Container.get({ name: swacComponentName || "FactoryItemComponent" });
			cur.proxy.setResource(assetNumber, instanceName, "IPL");
		}
		return false;
	}

	function displaySwacWP(wp_id, isShow) {
		var wp = document.getElementById(wp_id);    
		if (!isShow)
			wp.style.display = "none";
		else
			wp.style.display = "block";
	}

	function valorSwac(containerId, options, url, componentName) {
		$.getScript("scripts/user/Industry Solutions/SWAC/min/swac-base.min.js", function() {
			$.getScript("scripts/user/Industry Solutions/SWAC/min/swac-container.min.js", function() {
				var parent = document.getElementById(containerId);
				options = options || {};
				options.flavor = "ui";

				SWAC.Container.onCreated.subscribe(function (event) {
					var name = event.data.name,
						cur = SWAC.Container.get({ name: name });
					console.log("Component created: " + name);
					cur.onReady.subscribe(function (event) {
						console.log("Component ready: " + name);

						if (cur.hasUI()) {
							cur.beginShow(true);
						}
					});
				});
				SWAC.Container.beginCreate([{
					name: componentName || "FactoryItemComponent",
					source: url,
					type: "",
					parent: parent,
					settings: options
				}]).then(
					function (value) { window.console.log("Components successfully created"); },
					function (reason) {
						window.console.log("Not all components could be created");
					});
			});
		});
	}

    var materialRequestStatusBoardInterface = {
        connectToSignalRHub: connectToSignalRHub,
		setResource: setResource,
		valorSwac: valorSwac,
		displaySwacWP: displaySwacWP
    };

    return materialRequestStatusBoardInterface;

})();