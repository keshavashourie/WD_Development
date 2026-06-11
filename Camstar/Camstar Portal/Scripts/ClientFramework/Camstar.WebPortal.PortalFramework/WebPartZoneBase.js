// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />

Type.registerNamespace("Camstar.WebPortal.PortalFramework");

Camstar.WebPortal.PortalFramework.WebPartBaseZone = function(element) 
{
    Camstar.WebPortal.PortalFramework.WebPartBaseZone.initializeBase(this, [element]);

    this._isStatic = false;
    this.isZone = true;
    this._webPartClientIds = new Array();

}

Camstar.WebPortal.PortalFramework.WebPartBaseZone.prototype = 
{
    initialize: function() 
    {
        Camstar.WebPortal.PortalFramework.WebPartBaseZone.callBaseMethod(this, 'initialize');
    },
    
    dispose: function() 
    {
        this.disposeChildren();    
        Camstar.WebPortal.PortalFramework.WebPartBaseZone.callBaseMethod(this, 'dispose');
    },

    disposeChildren: function () {
        var wpUI = this.get_webPartClientIds().map(function (w) { return w + "_UIComponent" });
        var added = [];
        var removed = [];
        wpUI.forEach(function (w, i) {
            var wp = $find(w);
            if (!wp) {
                // Load controls
                ($("#" + w).data("controls") || [])
                    .forEach(function (c) {
                        added.push(c);
                    });
                removed.push(i);
            }
        });

        removed.forEach(function (i) { wpUI.splice(i, 1); });
        wpUI = wpUI.concat(added);

        $(this.get_element()).data("controls", wpUI);

        Camstar.WebPortal.PortalFramework.WebPartBaseZone.callBaseMethod(this, 'disposeChildren');
    },
    
    get_isStatic: function() { return this._isStatic; },
    set_isStatic: function(value) { this._isStatic = value; },

    get_webPartClientIds: function() { return this._webPartClientIds; },
    set_webPartClientIds: function(value) { this._webPartClientIds = value;}
}

// Optional descriptor for JSON serialization.
Camstar.WebPortal.PortalFramework.WebPartBaseZone.descriptor =
{
    properties:
    [
        { name: "webPartClientIds", type: Array }
    ]
}

Camstar.WebPortal.PortalFramework.WebPartBaseZone.registerClass('Camstar.WebPortal.PortalFramework.WebPartBaseZone', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
