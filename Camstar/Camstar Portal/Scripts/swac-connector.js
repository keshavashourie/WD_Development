// Copyright Siemens 2023


var swacConnector = (function () {
    'use strict';

    var TEMPLATE = {
        swacContainer:
            "<div id='swac-empty-state-container-_WEB_PART_NAME_' class='swac-empty-state-container'>\
                <div id='swac-empty-state-_WEB_PART_NAME_' class='swac-empty-state'>\
                    <canvas id='swac-empty-state-img-_WEB_PART_NAME_' class='swac-empty-state-img'></canvas>\
                    <div id='swac-empty-state-text-_WEB_PART_NAME_' class='swac-empty-state-text'></div>\
                </div>\
            </div>\
            <div>\
                <span id='swac-container-parent-_WEB_PART_NAME_' class='swac-container-parent'>\
            </div>"
    };
    Object.freeze(TEMPLATE);

    var SELECTOR = {
        getContainerParent: function (wpName) { return '#swac-container-parent-' + wpName; },
        getContainerParentIframe: function (wpName) { return '#swac-container-parent-' + wpName + ' > iframe'; },
        getEmptyStateContainer: function (wpName) { return '#swac-empty-state-container-' + wpName; },
        getEmptyStateImage: function (wpName) { return '#swac-empty-state-img-' + wpName; },
        getEmptyStateText: function (wpName) { return '#swac-empty-state-text-' + wpName; },
        getSwacButtonDiv: function (wpName) { return '#WebPart_' + wpName + ' .swac-button-cell'; },
        POPUP_CONTENT: '#WebPart_SwacWPForPopup_UIComponent'
    };
    Object.freeze(SELECTOR);

    var _labelStore = labelStore();

    function logMessage(func, message) {
        var time = new Date();
        var hour = time.getHours();
        var min = time.getMinutes();
        var sec = time.getSeconds();
        var mil = time.getMilliseconds();
        var timeString = (hour > 9 ? hour : '0' + hour) + ':' + (min > 9 ? min : '0' + min) + ':' + (sec > 9 ? sec : '0' + sec) + ':' + (mil < 10 ? '00' + mil : mil < 100 ? '0' + mil : mil);

        console.log(timeString + ' ' + func + ": " + message);
    }

    // Initializes all embedded swac containers
    // swacConfig [Array] Configuration data from the web part for swac containers to be embedded in a page.
    function initializeSwacConnections(swacConfig) {

        var connections = [];

        // most implementations will have only one swac container, but no need to require that.
        swacConfig.forEach(function (wpConfig) {
            logMessage('initializeSwacConnections', 'Initialize swac connection for web part: ' + wpConfig.WebPartName);
            var connection = new SwacConnection(wpConfig);
            connection.initialize();
            connections.push(connection);
        });

        // handle resizing
        $(document).on('panelResized', function () { doResize('panelResized'); });

        var resizeTimeout = null;
        $(window).bind('resize', function () {
            if (resizeTimeout)
                clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function () { doResize('windowResized'); }, 100);
        });

        function doResize(source) {
            logMessage('doResize', 'resizing due to ' + source);
            connections.forEach(function (c) { c.resize(); });
        }
    }

    // constructor function to create instance of a swac connection
    function SwacConnection(webPartConfig) {
        this.config = webPartConfig;
    }

    SwacConnection.prototype.initialize = function () {
        var connection = this;

        // if popup, try to maximize it
        if (this.config.IsPopup) {
            try {
                pop.GetCallerPage().pop.maximize();
            } catch (ex) {
                console.error(ex);
            }
        }

        // remove the button from the web part and add markup for swac container
        var $buttonDiv = $(SELECTOR.getSwacButtonDiv(this.config.WebPartName));
        $buttonDiv.find('input').hide();
        $buttonDiv.append(TEMPLATE.swacContainer.replace(/_WEB_PART_NAME_/g, this.config.WebPartName));

        $(SELECTOR.getEmptyStateContainer(this.config.WebPartName)).hide();

        var componentName = this.config.ComponentName || 'emptyComponent'; // if no component name, use the empty component which does nothing
        var url = this.config.ComponentUrl;

        // verify an interface exists for this component name
        var componentInterface = getSwacComponentInterface(componentName);
        if (!componentInterface) {
            this.showNoData(true);
            return;
        }

        // verify component args are valid
        if (!componentInterface.validArgs(this.config.ComponentArgs)) {
            this.showNoData(true);
            return;
        }

        // if initializing via url params, update url now
        // the base url is set in Portal Studio on SwacWP. it is up to the component interface to know how to append anything additional to that base.
        if (componentInterface.initializeByUrl()) {
            let params = componentInterface.getQueryParams(this.config.ComponentArgs);
            console.log('initializing component by url with params: ' + params);
            url += params;
        }

        var parent = $(SELECTOR.getContainerParent(this.config.WebPartName))[0];
        var options = this.getSize();
        options.flavor = "ui";

        // just logging a message on successful component creation
        SWAC.Container.onCreated.subscribe(function (event) {
            logMessage('SWAC.Container.onCreated', event.data.name);
        });


        SWAC.Container.onReady.subscribe(function (event) {
            var name = event.data.name;
            logMessage('SWAC.Container.onReady', name);
            //TODO: do we need a delay here?
            setTimeout(showComponent, 0)

            function showComponent() {
                //console.log("showComponent: " + name);
                logMessage('showComponent', 'calling SWAC.Container.get for component ' + name);
                var component = SWAC.Container.get({ name: name });
                if (component.hasUI()) {
                    logMessage('showComponent', 'calling component.beginShow(true) for component ' + name);
                    component.beginShow(true);
                    //TODO: do we need the delay here?
                    setTimeout(finishShowComponent, 0);
                }

                function finishShowComponent() {
                    logMessage('finishShowComponent', name);
                    componentInterface.setComponent(component);
                    if (!componentInterface.initializeByUrl()) {
                        logMessage('finishShowComponent', 'initializing component by function with args: ' + connection.config.ComponentArgs);
                        componentInterface.initialize(connection.config.ComponentArgs);
                    }
                    componentInterface.setSubmitHandler(onSubmit, connection.config);
                    setTimeout(function () { connection.resize(); }, 100);
                }
            }

        });

        logMessage('', 'Calling SWAC.Container.beginCreate for component "' + componentName + '" and url "' + url + '".')
        SWAC.Container.beginCreate([{
            name: componentName,
            source: url,
            type: "",
            parent: parent,
            settings: options
        }]).then(
            function (value) {
                logMessage('SWAC.Container.beginCreate', 'Components successfully created: ' + value)
            },
            function (reason) {
                logMessage('SWAC.Container.beginCreate', 'Component creation failed with reason: ' + reason)
                connection.showNoData(true);
                console.error(reason);
                showMessage(reason, "Error", true);
            });

    };

    // Resets the dimensions of the swac container
    SwacConnection.prototype.resize = function () {
        var size = this.getSize();

        $(SELECTOR.getContainerParent(this.config.WebPartName)).height(size.height).width(size.width);
        $(SELECTOR.getContainerParentIframe(this.config.WebPartName)).height(size.height).width(size.width);
        $(SELECTOR.getEmptyStateContainer(this.config.WebPartName)).height(size.height).width(size.width);

        logMessage('SwacConnection.prototype.resize', 'resize web part "' + this.config.WebPartName + '" to height ' + size.height + ', width ' + size.width)
    };

    // Gets height/width to use as dimensions for swac container
    SwacConnection.prototype.getSize = function () {
        var height, width, margin, cellAdjust, matrixAdjust, totalHeightAdjust, $scrollablePanel, $popupContent, $swacWPContainer;

        // some fudging
        margin = 16;
        cellAdjust = 10;
        matrixAdjust = 32;
        totalHeightAdjust = (margin * 2) + cellAdjust + matrixAdjust;

        if (this.config.IsPopup) {
            // shwoing swac in dedicated popup page

            $scrollablePanel = $('#scrollablepanel');
            height = $scrollablePanel.height() - totalHeightAdjust;

            $popupContent = $(SELECTOR.POPUP_CONTENT);
            width = $popupContent.width() - (margin * 2);

            //logMessage('getSize', 'srollablepanel (w:' + $scrollablePanel.width() + ', h:' + $scrollablePanel.height() + ')');
            //logMessage('getSize', 'popup content (w:' + $popupContent.width() + ', h:' + $popupContent.height() + ')');

        } else if (this.config.FixedHeight > 0 && this.config.FixedWidth > 0) {
            // user specified size on SwacWP. use whatever was set as is.

            height = this.config.FixedHeight;
            width = this.config.FixedWidth;

        } else {
            // embedded in some page, but user didn't specify a size. try to use all space

            $scrollablePanel = $('.scrollable-panel');
            height = $scrollablePanel.height() - totalHeightAdjust;

            $swacWPContainer = $('#WebPart_' + this.config.WebPartName + '_UIComponent');
            width = $swacWPContainer.width();
        }

        logMessage('getSize', 'returning (w:' + width + ', h:' + height + ')');
        return { width: width, height: height };
    };

    // Shows or hides the 'no data' indicator.
    // noData [Boolean] If true, shows the 'no data' indicator, else shows the swac container.
    SwacConnection.prototype.showNoData = function (noData) {
        var wpName = this.config.WebPartName;

        if (noData) {
            _labelStore.getLabel('Mom_EmptyComponentText', function (noDataMessage) {
                $(SELECTOR.getEmptyStateContainer(wpName)).show();
                $(SELECTOR.getContainerParent(wpName)).hide();
                $(SELECTOR.getEmptyStateText(wpName)).text(noDataMessage);

                let emptyStateImg = $(SELECTOR.getEmptyStateImage(wpName))[0];
                let ctx = emptyStateImg.getContext('2d');
                let image = new Image(192, 192);
                image.src = 'Themes/Horizon/images/icons/typeComputer48.svg';

                image.onload = function () {
                    ctx.drawImage(image, 0, 0, emptyStateImg.width, emptyStateImg.height);
                };
            });
        } else {
            $(SELECTOR.getEmptyStateContainer(wpName)).hide();
            $(SELECTOR.getContainerParent(wpName)).show();
        }
    };

    // manages getting and saving labels
    //TODO: should use new CR.Page utility
    function labelStore() {
        var loadedLabels = {};
        var page = window.__page || parent.__page || getCEP_top().__page;

        return {
            getLabel: getLabel
        }

        function getLabel(labelName, callback) {
            if (loadedLabels[labelName]) {
                callback(loadedLabels[labelName]);
            } else {
                let labels = [{ Name: labelName }];
                let labelVal = labelName;
                page.getLabels(labels, function (result) {
                    if ($.isArray(result)) {
                        // success
                        labelVal = result[0].Value;
                        loadedLabels[labelName] = labelVal;
                    } else {
                        // fail
                        console.error(result.Error);
                        labelVal = labelId;
                    }

                    callback(labelVal);
                });
            }
        }
    }

    // Handler for submit event from swac component. 
    function onSubmit(result, config) {
        var messageType = result.isSuccess ? 'Success' : 'Error';
        if (result.message) {
            showMessage(result.message, messageType);
        }

        if (config.IsPopup && result.isSuccess && result.autoClose) {
            pop.GetCallerPage().pop.hide();
        }
    }

    // show a message in the standard portal popup
    //TODO: most of this copied from an Electronics utility script. not sure if a better way.
    function showMessage(message, messageType, noForce) {

        var statusBar = $find("WebPart_StatusBar_UIComponent");

        _labelStore.getLabel('StatusMessage_' + messageType, finishShowMessage)

        function finishShowMessage(title) {
            // Apollo renders messages a little different
            //if (isApollo()) {
            title += '!&nbsp;';
            //}
            // Set values used by the ShowPopupMessage function
            $("div[messagetype]").attr("messagetype", messageType);
            $('span.messageType').text(title);
            $('span.instruction').text(message);

            try {
                // Display the message
                ShowPopupMessage('WebPart_StatusBar_UIComponent', false);
            } catch (ex) {
                console.error(ex);
            }
            // During testing on one user's computer, the opacity would be set to 0 on a message based element in the core portal code
            // when a message was displayed. This would prevent the message from being visible. This only happened from what I can tell 
            // in production client after a serial number was selected. We were not able to reproduce the issue on anyone else's VM. 
            // Therefore, just to be safe, if it ever shows up we will remove the opacity style attribute from the offending element right here.
            $("#WebPart_StatusBar").css("opacity", "");

            // If a 'banner' message at the top of the page is being displayed then we need to show it's parent.
            // Also, do cleanup as needed.
            if ((typeof noForce === 'undefined' || !noForce) && $('#ctl00_StaticZoneTop_UIComponent').length > 0) {
                $('#ctl00_StaticZoneTop_UIComponent').css('z-index', '9999');
                $('#ctl00_StaticZoneTop_UIComponent').show();

                if (messageType === "Success") {
                    if (statusBar._displaySuccessPopup) {
                        // The Success message fades in then out. Show the message's parent as long as needed. Then clear it out.
                        var minimumTimeout = 3000;
                        var defaultTimeout = statusBar._successPopupFadeOutTime * 2;
                        var timeoutDuration = defaultTimeout >= minimumTimeout ? defaultTimeout : minimumTimeout

                        setTimeout(hideStaticZoneTopUIComponent, timeoutDuration);
                    }
                } else {
                    //A warning or error message has an X box to close the message. Add an event handler to clear the parent
                    //div of the message
                    $("#ctl00_WebPartManager_StatusBar_CloseStatusButton").bind("click.showMessage", function () {
                        hideStaticZoneTopUIComponent();
                    });
                }

                // For QA testing framework
                $('#ctl00_WebPartManager_StatusBar_StatusMessageExists').attr('value', '1');
            }
        }

        // Hide the parent wrapper of the message display
        function hideStaticZoneTopUIComponent() {
            if ($('#ctl00_StaticZoneTop_UIComponent').length > 0) {
                $('#ctl00_StaticZoneTop_UIComponent').css('z-index', '0');
                $('#ctl00_StaticZoneTop_UIComponent').hide();

                // Message Handler set in the showMessage() function. Handle it here instead of in that function
                // because the message might be cleared instead of closed with a button click.
                $("#ctl00_WebPartManager_StatusBar_CloseStatusButton").unbind("click.showMessage");
            }
        }

    }

    // Get an interface object that implements required functions for interacting with a swac component.
    // name [String] Name of the interface to get.
    // component [Object] Reference to the successfully created swac component
    /**
     * Get an interface object that implements required functions for interacting with a swac component.
     * When creating a new interface, add "else if" to construct your own interface obj. 
     * for example:
     *      else if (name === 'myComponentName')
     *          ci = myComponentInterface(name, component);
     * 
     * @param {string} name - Name of interface to get
     * @param {object} [component] - Optional reference to the successfully created swac component. Can be set later.
     */
    function getSwacComponentInterface(name, component) {
        var ci;

        if (name === 'testComponent')
            ci = testInterface(name, component);
        else if (name === 'SRCComponent')
            ci = srcComponent(name, component);
        else if (name === 'SRCSettingsTest')
            ci = srcSettingsTest(name, component);
        else if (name === 'SRCConfiguration')
            ci = srcConfiguration(name, component);
        else
            ci = emptyInterface();

        if (ci) {
            //console.log('Successfully found component interface for component: ' + name);
            logMessage('getSwacComponentInterface', 'Successfully found component interface for component: ' + name);
        } else {
            //console.error('Failed to find component interface for component: ' + name);
            logMessage('getSwacComponentInterface', 'Failed to find component interface for component: ' + name);
            _labelStore.getLabel('SWAC_ErrorNoComponentInterface', function (message) {
                showMessage(message, 'Error');
            });
        }

        return ci;
    }

    ///////////////////////////////////////////////////////////////////////////
    //
    //  Interfaces for remote components.
    //
    //  Add new interfaces below. (see "testInterface" for example.)
    //  Update function above to get the interface by name.
    //
    ///////////////////////////////////////////////////////////////////////////

    /**
     * Gets object that is interface to a test component used during initial construction of this script
     * @param {string} compName
     * @param {object} compReference
     */
    function testInterface(compName, compReference) {

        var componentName = compName;
        var component = compReference;
        var initByUrl = false;  // set true to indicate component is initialized by URL rather than SWAC function

        return {
            initialize: initialize,
            initializeByUrl: initializeByUrl,
            getQueryParams: getQueryParams,
            setComponent: setComponent,
            setSubmitHandler: setSubmitHandler,
            validArgs: validArgs
        };

        // An optional component "initialization" function passing args in order needed.
        // if component doesn't need initialization, implement empty function here that does nothing.
        function initialize(args) {
            if (component)
                component.proxy.initialize(args[0], args[1]);
            else
                console.log('Attempt to initialize component "' + componentName + '", before component set.')
        }

        // return true if component will be initialized by URL.
        function initializeByUrl() {
            return initByUrl;
        }

        // return true if the argument list passes validation checks
        function validArgs(args) {
            return true;
        }

        // build the query param string to append to the component URL
        function getQueryParams(args) {
            var paramString = '';

            if (initByUrl) {
                let paramArray = ['?factory=', args[0], '&enterprise=', args[1]];
                paramString = paramArray.join('');
            }

            return paramString;
        }

        // set the component reference
        function setComponent(comp) {
            component = comp;
        }

        // provide an optional handler for a component's "submit" event.
        // callback [Function] function to call in response to submit event
        // config [Object] configuration of connnector responding to event. must be passed to callback along with translated result.
        //
        // if event name on component differs from submit, change "onSubmit" in "component.proxy.onSubmit.subscribe" below to correct value.
        //
        // the handler must translate event data to standard format described below.
        //  {
        //      isSuccess: bool,    // indicates remote operation success or failure.
        //      message: string,    // if value is set, then display it in the standard message popup. messageType determined by isSuccess
        //      autoClose: bool     // flag to automatically close the swac popup. value is ignored for swac containers not in a popup.
        //  }
        function setSubmitHandler(callback, config) {
            if (component)
                component.proxy.onSubmit.subscribe(onSubmit);
            else
                console.log('Attempt to subsribe to event of component "' + componentName + '", before component set.')

            function onSubmit(result) {
                if (callback) {
                    var translatedResult = {
                        isSuccess: result.data.isSuccess,
                        message: result.data.message,
                        autoClose: result.data.autoClose
                    };
                    callback(translatedResult, config);
                }

            }
        }
    }

    /**
     * Gets object that is interface to a any component where no initialization or event handling is required.
     * @param {string} compName
     * @param {object} compReference
     */
    function emptyInterface(compName, compReference) {

        return {
            initialize: initialize,
            initializeByUrl: initializeByUrl,
            getQueryParams: getQueryParams,
            setComponent: setComponent,
            setSubmitHandler: setSubmitHandler,
            validArgs: validArgs
        };

        function initialize() {
            console.log('No component interface provided. Not initializing component.');
        }

        function initializeByUrl() {
            return false;
        }

        function validArgs(args) {
            return true;
        }

        // never called for empty component
        function getQueryParams() {
            console.log('No component interface provided. Not initializing component.');
            return '';
        }

        // set the component reference
        function setComponent(comp) {
        }

        function setSubmitHandler() {
            console.log('No component interface provided. Not subscribing to submit event');
        }
    }

    // Implements an interface for SRC component
    /**
     * Gets object that is interface to the SRC component
     * @param {string} compName
     * @param {object} compReference
     */
    function srcComponent(compName, compReference) {

        var componentName = compName;
        var component = compReference;
        var initByUrl = true;  // set true to indicate component is initialized by URL rather than SWAC function

        return {
            initialize: initialize,
            initializeByUrl: initializeByUrl,
            getQueryParams: getQueryParams,
            setComponent: setComponent,
            setSubmitHandler: setSubmitHandler,
            validArgs: validArgs
        };

        // An optional component "initialization" function passing args in order needed.
        // if component doesn't need initialization, implement empty function here that does nothing.
        function initialize(args) {
            if (component)
                component.proxy.displayResourceSettings(args[0], args[1], args[2]);
            else
                console.log('Attempt to initialize component "' + componentName + '", before component set.')
        }

        function validArgs(args) {
            var valid = false;

            switch (args[2]) {
                case 'ENTERPRISE':
                case 'SITE':
                case 'AREA':
                case 'LINE':
                case 'EQUIPMENT':
                    valid = true;
                    break;
                default:
                    break;
            };

            return valid;
        }

        // return true if component will be initialized by URL.
        function initializeByUrl() {
            return initByUrl;
        }

        // build the query param string to append to the component URL
        function getQueryParams(args) {
            var paramString = '';

            if (initByUrl) {

                let id = encodeURIComponent(args[0]);
                let name = encodeURIComponent(args[1]);
                let factoryLevel;
                let accessPermission = encodeURIComponent(args[3]);

                switch (args[2]) {
                    case 'ENTERPRISE': factoryLevel = 1; break;
                    case 'SITE': factoryLevel = 2; break;
                    case 'AREA': factoryLevel = 4; break;
                    case 'LINE': factoryLevel = 8; break;
                    case 'EQUIPMENT': factoryLevel = 16; break;
                    default: factoryLevel = 0; break;
                };

                paramString = '/ResourceSettings/' + id + '/' + name + '/' + factoryLevel;
                if (accessPermission)
                    paramString += '/' + accessPermission;

                paramString += "?bearertoken=" + encodeURIComponent(args[4]);
            }

            return paramString;
        }

        // set the component reference
        function setComponent(comp) {
            component = comp;
        }

        function setSubmitHandler(callback, config) {
        }
    }

    // Implements an interface for SRC Configuration
    /**
     * Gets object that is interface to the SRC Configuration
     * @param {string} compName
     * @param {object} compReference
     */
    function srcConfiguration(compName, compReference) {

        var componentName = compName;
        var component = compReference;
        var initByUrl = true;  // set true to indicate component is initialized by URL rather than SWAC function

        return {
            initialize: initialize,
            initializeByUrl: initializeByUrl,
            getQueryParams: getQueryParams,
            setComponent: setComponent,
            setSubmitHandler: setSubmitHandler,
            validArgs: validArgs
        };

        function validArgs(args) {
            var valid = true;

            return valid;
        }

        // An optional component "initialization" function passing args in order needed.
        // if component doesn't need initialization, implement empty function here that does nothing.
        function initialize(args) {
            if (component)
                component.proxy.displayResourceSettings(args[0], args[1], args[2]);
            else
                console.log('Attempt to initialize component "' + componentName + '", before component set.')
        }

        // return true if component will be initialized by URL.
        function initializeByUrl() {
            return initByUrl;
        }

        // build the query param string to append to the component URL
        function getQueryParams(args) {
            var paramString = '';

            if (initByUrl) {

                let accessPermission = encodeURIComponent(args[0]);

                if (accessPermission)
                    paramString = '/' + accessPermission;

                paramString += "?bearertoken=" + encodeURIComponent(args[1]);
            }

            return paramString;
        }

        // set the component reference
        function setComponent(comp) {
            component = comp;
        }

        function setSubmitHandler(callback, config) {
        }
    }


    // public api for the swacConnector.
    var api = {
        //showMessage: showMessage,
        initializeSwacConnections: initializeSwacConnections
    };
    return api;


})();
