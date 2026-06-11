// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
Type.registerNamespace("Camstar.UI");

// Generic Client-Side UI Component Interface
Camstar.UI.IUIComponent = function () {
};
Camstar.UI.IUIComponent.Prototype = {
    get_isStatic: function () { },
    refresh: function (html) { }
};
Camstar.UI.IUIComponent.registerInterface('Camstar.UI.IUIComponent');

// Base class for all client-side objects
Camstar.UI.Control = function (element) {
    this._$parent = null;
    this.addToParent(element.id, element);
    Camstar.UI.Control.initializeBase(this, [element]);
};

Camstar.UI.Control.prototype = {
    initialize: function () {
        Camstar.UI.Control.callBaseMethod(this, 'initialize');

        if (this.getParent()) {
            var $wp = this.getParent().children(".webpart");
            if ($wp.length) {
                if ($wp.attr('HighlightRequiredFields')) {
                    this._hl_required_class = $wp.attr('HighlightRequiredFields');
                }
                else {
                    this._hl_required_class = null;
                }
            }
        }
    },

    dispose: function () {
        if (!this._element) {
            //console.log("no element", this);
        }
        else {
            this.disposeChildren();
        }

        Camstar.UI.Control.callBaseMethod(this, 'dispose');
    },

    disposeChildren: function () {
        var $elem = $(this.get_element());
        var childrenControls = $elem.data("controls");

        if (childrenControls && childrenControls.length) {
            childrenControls.forEach(function (id) {
                var c = $find(id);
                if (c) {
                    if (c.get_isStatic()) {
                        c.disposeChildren();
                    }
                    else {
                        c.dispose();
                    }
                }
            });
            $elem.data("controls", null);
        }
    },

    get_isStatic: function () { return false; },

    getParent: function (el) {
        if (!this.isWebPart) {
            if (!this._$parent) {
                this._$parent = $("#" + $(el).closest('.webpart').prop("id") + "_UIComponent");
            }
        }
        return this._$parent;
    },

    addToParent: function (id, element) {
        var $parentElement = this.getParent(element);
        if ($parentElement && $parentElement.length) {
            var ctls = $parentElement.data("controls") || [];
            if (ctls.indexOf(id) == -1)
                ctls.push(id);
            $parentElement.data("controls", ctls);
        }
    },

    refresh: function (html) {
        var internalWebPart = document.getElementById('WebPart_' + this._serverID); //$('#WebPart_' + this._serverID);
        var el = this.get_element();
        if (el && el.children.length == 0 && internalWebPart.length != 0) {
            // the internal web part inside the dialog
            var $i = $(internalWebPart);
            var d = $(html);
            if ($i.is(':visible')) {
                // if the dialog web part has been visible it will be set visible after rendering
                setTimeout(function () {
                    $i.show();
                }, 100);
            }
            $i.empty();
            $i.html(d.html());
            d.remove();        
        }
        else {
            this.get_element().innerHTML = html;
        }
        if (!this._initialized)
            this.initialize();
    },

    addChild: function (id) {
        if (this.childrenControls.indexOf(id) == -1)
            this.childrenControls.push(id);
    },

    directUpdate: function (value) { },

    get_hl_required: function () { return this._hl_required_class; }

};
Camstar.UI.Control.registerClass('Camstar.UI.Control', Sys.UI.Control, Camstar.UI.IUIComponent);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
