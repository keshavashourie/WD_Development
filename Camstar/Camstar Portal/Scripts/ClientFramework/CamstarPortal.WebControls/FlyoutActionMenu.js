// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");

CamstarPortal.WebControls.FlyoutActionMenu = function (element, controlID, serverType)
{
    CamstarPortal.WebControls.FlyoutActionMenu.initializeBase(this, [element]);
    this._element = [element];
    this._controlID = controlID;
    this._serverType = serverType;
}

CamstarPortal.WebControls.FlyoutActionMenu.prototype =
{
    initialize: function ()
    {
        CamstarPortal.WebControls.FlyoutActionMenu.callBaseMethod(this, 'initialize');

        // bind the events
        var obj = $(this._element[0]);
    },

    dispose: function ()
    {
        CamstarPortal.WebControls.FlyoutActionMenu.callBaseMethod(this, 'dispose');
    },

    _populateFlyOutMenu: function (responseSection)
    {
        var jsonData = null;
        var tooltipHTML = "";
        if (responseSection.Data.IsSuccess)
        {
            jsonData = $.parseJSON(responseSection.Data.Message.toString());
        }
        else
        {
            tooltipHTML += "<b>" + responseSection.Data.Message + "</b>";
        }
        if (jsonData != null)
        {
            for (var i = 0; i < jsonData.length; i++)
            {
                if (jsonData[i].AllowBrowse)
                    tooltipHTML += "<a href='javascript:void(0);' onclick='__doPostBack(\"" + this.get_controlId() + "\", \"" + jsonData[i].VirtPageName + ":" + jsonData[i].Caption + ":" + jsonData[i].Caption + ":" + this._element[0].previousSibling.data + "\");'>" + jsonData[i].Caption + "</a>";
                else
                    tooltipHTML += "<a class='disabled'>" + jsonData[i].Caption + "</a>";
            }
        }
        var flMenu = document.getElementById(this.get_controlId());
        flMenu = $(flMenu);
        flMenu.tooltip().show();
        var flyOutDiv = flMenu.data("tooltip").getTip();
        flyOutDiv.html(tooltipHTML);

        __page.hideModal();
    },

    _sendRequest: function (callParameters, serverAction, callBackMethod, returnVal)
    {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
        transition.set_command(serverAction);

        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback(callBackMethod);
        var communicator = new Camstar.Ajax.Communicator(transition, this);
        communicator.syncCall();

    },

    get_serverType: function () { return this._serverType; },

    get_controlId: function () { return this._controlID; }
}

CamstarPortal.WebControls.FlyoutActionMenu.registerClass('CamstarPortal.WebControls.FlyoutActionMenu', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
