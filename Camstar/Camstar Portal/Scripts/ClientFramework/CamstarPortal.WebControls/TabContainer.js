// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.Personalization");

/******************* CamstarPortal.WebControls.TabContainer *******************/
CamstarPortal.WebControls.TabContainer = function(element)
{
    CamstarPortal.WebControls.TabContainer.initializeBase(this, [element]);

    this._controlID = null;
    this._loadAllTabs = false;
    this._webParts = [];
}

CamstarPortal.WebControls.TabContainer.prototype =
{
    initialize: function()
    {
        CamstarPortal.WebControls.TabContainer.callBaseMethod(this, 'initialize');

        var wpUI = this.get_webParts().map(function (w) { return w + "_UIComponent" });
        $(this.get_element()).data("controls", wpUI);

        $(this.get_element()).scrollableTabs({
            overflow: "wrap",
            removable: false,
            hideIfEmpty: false,
            activate: function (event, ui) {
                //If workflow control (which uses jsPlumb) is present on hidden tab,
                //it may incorretly calculate items' & connections' dimensions, and therefore needs to be redrawn
                if (typeof (jsPlumb) === "object")
                    jsPlumb.repaintEverything();
                if (!this.control.get_loadAllTabs())
                {
                    __page.postback(this.control.get_controlID(), ui.newTab.index(), null);
                    return false;
                }
                return true;
            }
        });
    },

    dispose: function()
    {
        CamstarPortal.WebControls.TabContainer.callBaseMethod(this, 'dispose');
    },

    get_controlID: function() { return this._controlID; },
    set_controlID: function(value) { this._controlID = value; },

    get_webParts: function () { return this._webParts; },
    set_webParts: function (value) { this._webParts = value; },

    get_loadAllTabs: function() { return this._loadAllTabs; },
    set_loadAllTabs: function(value) { this._loadAllTabs = value; }
};

CamstarPortal.WebControls.TabContainer.registerClass('CamstarPortal.WebControls.TabContainer', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
