// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../Camstar.WebPortal.PortalFramework/WebPartBase.js" />

Type.registerNamespace("Camstar.WebPortal.WebPortlets");

Camstar.WebPortal.WebPortlets.ButtonsBarControl = function(element)
{
    Camstar.WebPortal.WebPortlets.ButtonsBarControl.initializeBase(this, [element]);
}

Camstar.WebPortal.WebPortlets.ButtonsBarControl.prototype = {
	initialize: function ()
	{
		Camstar.WebPortal.WebPortlets.ButtonsBarControl.callBaseMethod(this, 'initialize');
	},

	dispose: function ()
    {
		Camstar.WebPortal.WebPortlets.ButtonsBarControl.callBaseMethod(this, 'dispose');
    },
    get_isStatic: function () {
        return true;
    }
}

Camstar.WebPortal.WebPortlets.ButtonsBarControl.registerClass('Camstar.WebPortal.WebPortlets.ButtonsBarControl', Camstar.WebPortal.PortalFramework.WebPartBase);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
