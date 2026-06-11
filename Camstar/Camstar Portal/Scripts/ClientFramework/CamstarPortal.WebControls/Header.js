// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");

/******************* CamstarPortal.WebControls.CollapsableSection *******************/
CamstarPortal.WebControls.Header = function(element)
{
    CamstarPortal.WebControls.Header.initializeBase(this, [element]);
    this._resource = null;
    this._workstation = null;
    this._workcenter = null;
    this._operation = null;
    this._spec = null;
    this._resourceValue = null;
    this._workstationValue = null;
    this._workcenterValue = null;
    this._operationValue = null;
    this._specValue = null;
    this._controlId = null;
    this._serverType = "CamstarPortal.WebControls.Header, CamstarPortal.WebControls";
},

CamstarPortal.WebControls.Header.prototype =
{
    initialize: function()
    {
        CamstarPortal.WebControls.Header.callBaseMethod(this, 'initialize');
    },

    dispose: function()
    {
        this._resource = null;
        this._workstation = null;
        this._workcenter = null;
        this._operation = null;
        this._spec = null;
        this._resourceValue = null;
        this._workstationValue = null;
        this._workcenterValue = null;
        this._operationValue = null;
        this._specValue = null;
        this._controlId = null;
        this._serverType = null;
        CamstarPortal.WebControls.Header.callBaseMethod(this, 'dispose');
    },
    
    refresh: function () 
    {
        var self = this;
        var recallSendRequest = function ()
        {
            self.refresh();
        };
        if (!__page._lock)
        {
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("RefreshHeaderSection");
            transition.set_commandParameters('');
            transition.set_clientCallback("refreshSectionContent");
            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
            communicator.dispose();
        }
        else 
        {
            window.setTimeout(recallSendRequest, 100);
        }
    },
    
    refreshSectionContent: function (responseSection) 
    {
        if (responseSection && responseSection.Data != null)
        {
            var separator = String.fromCharCode(2);
            var items = responseSection.Data.Message.split(separator);
            if (items[0])
            {
                this.get_resource().innerHTML = items[0];
                this.set_resourceValue(items[0]);
            }
            else
                this.get_resource().innerHTML = '&nbsp';
            if (items[1])
            {
                this.get_workstation().innerHTML = items[1];
                this.set_workstationValue(items[1]);
            }
            else
                this.get_workstation().innerHTML = '&nbsp';
            if (items[2])
            {
                this.get_workcenter().innerHTML = items[2];
                this.set_workcenterValue(items[2]);
            }
            else
                this.get_workcenter().innerHTML = '&nbsp;';
            if (items[3])
            {
                this.get_operation().innerHTML = items[3];
                this.set_operationValue(items[3]);
            }
            else
                this.get_operation().innerHTML = '&nbsp;';
            if (items[4]) {
                this.get_spec().innerHTML = items[4];
                this.set_specValue(items[4]);
            }
            else
                this.get_spec().innerHTML = '&nbsp;';
        }
        __page.hideModal();
    },

    get_resource: function() { return this._resource; },
    set_resource: function(value) { this._resource = value; },

    get_workstation: function() { return this._workstation; },
    set_workstation: function(value) { this._workstation = value; },

    get_workcenter: function() { return this._workcenter; },
    set_workcenter: function(value) { this._workcenter = value; },

    get_operation: function() { return this._operation; },
    set_operation: function (value) { this._operation = value; },

    get_spec: function () { return this._spec; },
    set_spec: function (value) { this._spec = value; },

    get_resourceValue: function () { return this._resourceValue; },
    set_resourceValue: function (value) { this._resourceValue = value; },

    get_workstationValue: function () { return this._workstationValue; },
    set_workstationValue: function (value) { this._workstationValue = value; },

    get_workcenterValue: function () { return this._workcenterValue; },
    set_workcenterValue: function (value) { this._workcenterValue = value; },

    get_operationValue: function () { return this._operationValue; },
    set_operationValue: function (value) { this._operationValue = value; },
    
    get_specValue: function () { return this._specValue; },
    set_specValue: function (value) { this._specValue = value; },

    get_controlId: function () { return this._controlId; },
    set_controlId: function(value) { this._controlId = value; },
    
    get_serverType: function() { return this._serverType;}

},

CamstarPortal.WebControls.Header.registerClass('CamstarPortal.WebControls.Header', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
