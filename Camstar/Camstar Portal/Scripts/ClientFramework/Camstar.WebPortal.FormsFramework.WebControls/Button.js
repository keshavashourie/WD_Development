// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.FormsFramework.WebControls.Button = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.Button.initializeBase(this, [element]);

    this._extensions = null;
}

Camstar.WebPortal.FormsFramework.WebControls.Button.prototype =
{
    initialize: function ()
    {
        if (this._extensions != null)
            eval(this._extensions);
    },

    dispose: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.Button.callBaseMethod(this, 'dispose');
    },

    Push: function (name, val)
    {
        this._element.click();
    },

    SetAttribute: function (name, val)
    {
        this._element.setAttribute(name, val);
    },

    SetStyle: function (name, val)
    {
        this._element.style[name] = val;
    },

    Click: function (method)
    {
        $(this._element).bind('click', method);
    },

    get_Hidden: function () { return this._element.style.display == 'none'; },
    set_Hidden: function (value)
    {
        if (value == true)
            $(this._element).hide();
        else
            $(this._element).show();
    },

    get_Disabled: function ()
    {
        return this._element.disabled;
    },
    set_Disabled: function (value)
    {
        this._element.disabled = value;
    },

    get_extensions: function () { return this._extensions; },
    set_extensions: function (value) { this._extensions = value; }

}

Camstar.WebPortal.FormsFramework.WebControls.Button.registerClass('Camstar.WebPortal.FormsFramework.WebControls.Button', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();


