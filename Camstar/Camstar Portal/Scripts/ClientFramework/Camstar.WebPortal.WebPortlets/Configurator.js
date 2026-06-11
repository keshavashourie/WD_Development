// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
/// <reference path="~/Scripts/ClientFramework/Camstar.WebPortal.PortalFramework/WebPartBase.js" />
Type.registerNamespace("Camstar.WebPortal.WebPortlets");

Camstar.WebPortal.WebPortlets.Configurator = function(element)
{
    Camstar.WebPortal.WebPortlets.Configurator.initializeBase(this, [element]);

    this._opened = false;
}

Camstar.WebPortal.WebPortlets.Configurator.prototype =
{
    initialize: function()
    {
        Camstar.WebPortal.WebPortlets.Configurator.callBaseMethod(this, 'initialize');
    },

    dispose: function()
    {
        Camstar.WebPortal.WebPortlets.Configurator.callBaseMethod(this, 'dispose');
    },

    nodeSelected: function(eventTarget, eventArgument)
    {
        __page.postback(eventTarget, eventArgument);
    },

    close: function()
    {
        $get("ctl00_WebPartManager_ConfiguratorControl_CloseConfiguratorButton").click();
    },

    templateSelected: function(sender)
    {
        var iconsContainer = sender.parentElement.parentElement
        for (var i = 0; i < iconsContainer.children.length; i++)
            iconsContainer.children[i].className = iconsContainer.children[i].firstChild["normalClass"];
        sender.parentElement.className = sender["selectedClass"];
        $get("ctl00_WebPartManager_ConfiguratorControl_HiddenSelectedIcon").value = sender["iconValue"];
    },

    get_opened: function() { return this._opened; },
    set_opened: function(value)
    {
        this._opened = value;
        if (this._opened)
            __toppage.getConcierge().toggle(false);
    },

    get_contentPanel: function() { return $get('WebPart_ConfiguratorControl'); },

    get_isStatic: function() { return true; }
}

Camstar.WebPortal.WebPortlets.Configurator.registerClass('Camstar.WebPortal.WebPortlets.Configurator', Camstar.WebPortal.PortalFramework.WebPartBase);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
