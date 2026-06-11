// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
/// <reference path="~/Scripts/Camstar.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.FormsFramework.WebControls.PagePanel = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.PagePanel.initializeBase(this, [element]);

    this._extensions = null;
    this._src = null;
}

Camstar.WebPortal.FormsFramework.WebControls.PagePanel.prototype =
{
    initialize: function () {
        this._element.firstElementChild.setAttribute("src", "about:blank");
        if (this._extensions != null)
            eval(this._extensions);
    },

    dispose: function ()
    {
        this._extensions = null;
        this._src = null;
        if (this._element) {
            removeIframe($(this._element.firstElementChild));
        }
        Camstar.WebPortal.FormsFramework.WebControls.PagePanel.callBaseMethod(this, 'dispose');
    },

    SetAttribute: function (name, val) {
        this._element.setAttribute(name, val);
    },

    SetStyle: function (name, val) {
        this._element.style[name] = val;
    },

    OpenSrc: function (value) {
        var randString = String((new Date()).getTime()).replace(/\D/gi, '');
        value += "&rand=" + randString;
        value += "&pagePanelOf=" + (typeof (__page) == 'undefined' ? "" : __page.get_virtualPageName());
        this._element.firstElementChild.setAttribute("src", value);
    },

    get_Hidden: function () { return this._element.style.display == 'none'; },
    set_Hidden: function (value) {
        if (value == true)
            this._element.style.display = 'none';
        else
            this._element.style.display = '';
    },

    get_Disabled: function () { return this._element.disabled; },
    set_Disabled: function (value) {
        this._element.disabled = value;
    },

    get_extensions: function () { return this._extensions; },
    set_extensions: function (value) { this._extensions = value; },

    get_Src: function () { return this._src; },
    set_Src: function (value) { this._src = value; this.OpenSrc(value); }
}

Camstar.WebPortal.FormsFramework.WebControls.PagePanel.registerClass('Camstar.WebPortal.FormsFramework.WebControls.PagePanel', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();


