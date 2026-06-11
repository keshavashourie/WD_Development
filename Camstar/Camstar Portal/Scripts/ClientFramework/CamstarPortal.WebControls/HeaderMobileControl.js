// Copyright Siemens 2023  

Type.registerNamespace("CamstarPortal.WebControls");

CamstarPortal.WebControls.HeaderMobileControl = function(element) {
        CamstarPortal.WebControls.HeaderMobileControl.initializeBase(this, [element]);
        this._alertsContainer = null;
        this._alertPanel = null;
        this._alertButton = null;
        this._controlId = null;
        this._alerts = null;
        this._alertPanelTitleText = null;
        this._minutesLabel = null;
        this._serverType = "CamstarPortal.WebControls.HeaderMobileControl, CamstarPortal.WebControls";
    },

CamstarPortal.WebControls.HeaderMobileControl.prototype =
    {
        initialize: function() {
            CamstarPortal.WebControls.HeaderMobileControl.callBaseMethod(this, "initialize");
            this._alerts = [];
            this._alertPanelTitleText = "Last Saved Messages ({0}/5)";
            this._alertPanel = $(".wrapper", this._alertsContainer);
            this._alertButton = $("button[data-toggle]", this._alertsContainer);
            var me = this;

            $(this._alertsContainer).on("show.bs.dropdown",
                function() {
                    $(this).find(".dropdown-menu").first().stop(true, true).slideDown(300).css("display", "flex");
                });

            $(this._alertsContainer).on("hide.bs.dropdown",
                function() {
                    $(this).find(".dropdown-menu").first().stop(true, true).slideUp(200);
                });

            this._alertButton.click(function() {
                if (!$(me._alertsContainer).hasClass("show"))
                    me.refreshAlertPanel();
            });

            // close alert panel when click outside top window.
            $(window).on("blur",
                function() {
                    if (me._alertButton.is("[aria-expanded='true']"))
                        me._alertButton.dropdown("toggle");
                });
        },

        addAlert: function(message, messageType) {
            var lastItem = [{ message: message, messageType: messageType, date: new Date() }];
            this._alerts = lastItem.concat(this._alerts.slice(0, 4));
            this._alertButton.show();
        },

        refreshAlertPanel: function() {
            var title = $(".dropdown-panel-title", this._alertsContainer);
            if (title.length === 0) {
                this.addAlertPanelTitle();
                title = $(".dropdown-panel-title", this._alertsContainer);
            }
            var content = $(".dropdown-panel-content", this._alertsContainer);
            if (content.length === 0) {
                this.addAlertPanelContentDiv();
                content = $(".dropdown-panel-content", this._alertsContainer);
            }
            title.html(this._alertPanelTitleText.replace("{0}", this._alerts.length));
            this.buildAlertContent(content);
        },

        addAlertPanelTitle: function() {
            this._alertPanel.prepend($("<div class='dropdown-panel-title'>" + this._alertPanelTitleText.replace("{0}", "0") + "</div>"));
        },

        addAlertPanelContentDiv: function () {
            this._alertPanel.append($("<div class='dropdown-panel-content'></div>"));
        },

        buildAlertContent: function(contentDiv) {
            contentDiv.empty();
            var content = "";
            var currentDate = new Date();
            var me = this;
            this._alerts.forEach(function(alert) {
                content += "<div class='alert " + alert.messageType + "'>";
                content += "<div class='time'>" + me.getDeltaInMinutes(currentDate, alert.date) + " " + me._minutesLabel + " ago</div>";
                content += "<div class='msg'>" + alert.message + "</div>";
                content += "</div>";
            });
            contentDiv.html($(content));
        },

        getDeltaInMinutes: function(datetime1, datetime2) {
            var diff = Math.abs(datetime1 - datetime2); // milliseconds.
            return Math.floor((diff / 1000) / 60);
        },

        dispose: function () {
            this._alertPanel = null;
            this._alertButton = null;
            this._alertsContainer = null;
            this._controlId = null;
            this._alerts = null;
            this._alertPanelTitleText = null;
            this._minutesLabel = null;

            CamstarPortal.WebControls.HeaderMobileControl.callBaseMethod(this, "dispose");
        },

        get_alertsContainer: function () { return this._alertsContainer; },
        set_alertsContainer: function (value) { this._alertsContainer = value; },

        get_controlId: function () { return this._controlId; },
        set_controlId: function (value) { this._controlId = value; },

        get_minutesLabel: function () { return this._minutesLabel; },
        set_minutesLabel: function (value) { this._minutesLabel = value; },

        get_serverType: function () { return this._serverType; }
    },

CamstarPortal.WebControls.HeaderMobileControl.registerClass('CamstarPortal.WebControls.HeaderMobileControl', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
