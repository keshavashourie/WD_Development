// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList.initializeBase(this, [element]);

    this._control = null;
};

Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList.prototype = {
    initialize: function () {
        Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList.callBaseMethod(this, 'initialize');

        $("input", this._control).each(function () {
            var td = $(this).parent();
            var label = $("label", td);
            var forAttr = this.id;

            if (label.length === 0) {
                label = $("<label>");
                label.attr("for", forAttr);
                $(this).after(label);
            }

            label.before("<label for='" + forAttr + "'></label>");
            label.addClass("cs-label").attr("pos", "right");
            $(td).wrapInner("<span class='cs-radio-button'></span>");
        });
    },

    dispose: function () {
        Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList.callBaseMethod(this, 'dispose');
    },

    get_control: function () { return this._control; },
    set_control: function (value) { this._control = value; }
};

Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList.registerClass('Camstar.WebPortal.FormsFramework.WebControls.RadioButtonList', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
