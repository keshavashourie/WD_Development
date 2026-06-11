// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../Camstar.WebPortal.PortalFramework/WebPartBase.js" />

Type.registerNamespace("Camstar.WebPortal.WebPortlets");

Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons = function(element)
{
    this.isWebPart = true;
    Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons.initializeBase(this, [element]);
}

Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons.prototype =
{
    initialize: function () {

        Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons.callBaseMethod(this, 'initialize');

        // re-order buttons if needed
        $(':submit', this._element).each(function (a,btn)
        {
            if ($(btn).is('[position=rightmost]') && $(btn).parent().hasClass('left'))
            {
                // Move the button to the right panel
                var leftDiv = $(btn).parent();
                var rightDiv = leftDiv.next();
                var b = $(btn).detach();
                b.appendTo(rightDiv);
            }
        }
        );
    },

    get_isStatic: function () {
        return true;
    },

    dispose: function () {
        var buttons = $(".cs-button", this.get_element()).toArray().map(function (b) { return b.id; });

        if (buttons.length) {
            $(this.get_element()).data("controls", buttons);
        }
        Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons.callBaseMethod(this, 'dispose');
    }
}

Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons.registerClass('Camstar.WebPortal.WebPortlets.WorkflowNavigationButtons', Camstar.WebPortal.PortalFramework.WebPartBase);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
