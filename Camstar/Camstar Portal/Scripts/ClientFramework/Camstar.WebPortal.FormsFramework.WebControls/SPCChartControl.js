// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../../jquery/jquery.min.js" />
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl.initializeBase(this, [element]);
    //this._element = [element];
    this._controlId = null;
    this._controlTime = null;
    this._intervalId = null;
    this._refreshTime = null;
    this._newSpcChartId = null;
    this._refreshInterval = 5000; //Refresh interval is set to 5 sec.
    this._serverType = "Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl, Camstar.WebPortal.FormsFramework.WebControls";
};

Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl.prototype =
{

    initialize: function () {
        var me = this;
        this._controlId = this.get_id();
        this._refreshTime = parseInt($("#" + this._controlId + "_AutoRefreshTime").val());

        var controlId = this._controlId;
        if (this._refreshTime > 0) {
            //Create SPC Timer
            if ($('#hdnSPCTimer').length == 0) { //if SPC Timer doesn't exists than add, fire every 3 secs
                var $hiddenSPCTimer = $('<input/>', { type: 'hidden', id: 'hdnSPCTimer', value: $.now() });
                $hiddenSPCTimer.appendTo('form');
            }
        
            //Set SPC Timer to fire at refresh interval.
            this._intervalId = setInterval(function () {
                $("#hdnSPCTimer").triggerHandler("change");
            }, this._refreshInterval);

            this._controlTime = parseInt($.now()) + this._refreshTime;
            
            $("#hdnSPCTimer").change(function () {
                me.refresh(controlId);
            });
        }
        var annotationRequiredMessage = $("#" + this._controlId + "_AnnotationMsg").val();
        me.setAnnotationMessage(annotationRequiredMessage);
        
        Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl.callBaseMethod(me, 'initialize');
    },

    dispose: function () {
        clearInterval(this._intervalId);
        this._controlId = null;
        this._controlTime = null;
        this._intervalId = null;
        this._refreshTime = null;
        this._refreshInterval = null;
        this._serverType = null;
        this._newSpcChartId = null;
        Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl.callBaseMethod(this, 'dispose');
    },

    refresh: function (controlId) {
        if (this._controlTime != null && parseInt($.now()) > this._controlTime) { //If control time has been elapsed, then refresh control
            //Make AJAX Call
            var callParameters = {
                ControlId: controlId,
                CallStackKey: __page.get_CallStackKey()
            };
            var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("RefreshSPCChartControl");
            transition.set_commandParameters(callParamsString);
            transition.set_clientCallback("refreshSPCChart");
            transition.set_noModalImage(true);
            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
        }
    },

    refreshSPCChart: function (responseSection) {
        //If data is returned, display
        
        if (responseSection) {
            var annotationText = responseSection.ResponseID;
            var chartHtml = responseSection.Data.Message;
            if (annotationText)
                this.setAnnotationMessage(annotationText);
            if (chartHtml) {
                var newSpc = $find(this._newSpcChartId);
                if (newSpc) {
                    newSpc.refreshSPCChart(responseSection);
                }
                else
                    $("div.spcChartPage", "#" + this.get_controlId()).replaceWith(responseSection.Data.Message);
            }
                
        }
        //Update control time
        this._controlTime = parseInt($.now()) + this._refreshTime;
    },

    setAnnotationMessage: function (message) {
        var closeBtn = parent.window.$("#ctl00_CloseLink");
        if (closeBtn) {
            closeBtn.unbind();
            if (message) {
                closeBtn.click(function() { alert(message, 'Warning!', null); });
            } else {
                closeBtn.click(function () { window.parent.CloseFloatingFrame(true); });
            }
        }
    },
    get_controlId: function () { return this._controlId; },
    set_controlId: function (value) { return this._controlId = value; },

    get_newSpcChartId: function () { return this._newSpcChartId; },
    set_newSpcChartId: function (value) { return this._newSpcChartId = value; },

    get_serverType: function () { return this._serverType; },
    set_serverType: function (value) { return this._serverType = value; }
},

Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl.registerClass('Camstar.WebPortal.FormsFramework.WebControls.SPCChartControl', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
