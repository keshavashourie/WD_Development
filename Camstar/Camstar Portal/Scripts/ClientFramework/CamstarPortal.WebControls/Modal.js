// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");

CamstarPortal.WebControls.Modal = function (element)
{
    CamstarPortal.WebControls.Modal.initializeBase(this, [element]);

    this._showCount = null;
    this._mask = null;

    this._mask = $get("divModal");
    if (!this._mask)
    {
    this._mask = document.createElement("div");
    this._mask.id = "divModal";

    Sys.UI.DomElement.addCssClass(this._mask, "divModal");

    document.body.appendChild(this._mask);
    }
    this._showCount = 0;

    this._windowChangeDelegate = Function.createDelegate(this, this.center);

    $addHandlers(window,
    {
        'resize': this._windowChangeDelegate,
        'scroll': this._windowChangeDelegate,
        'DOMMouseScroll': this._windowChangeDelegate
    }, this);
}

CamstarPortal.WebControls.Modal.prototype =
{
    dispose: function ()
    {
        CamstarPortal.WebControls.Modal.callBaseMethod(this, 'dispose');
    },

    show: function ()
    {
        this._showCount++;

        window.focus();
        this.center();

        Sys.UI.DomElement.setVisible(this._mask, true);

        return false;
    },

    hide: function ()
    {
        Sys.UI.DomElement.setVisible(this._mask, false);

        this._showCount--;
    },

    center: function ()
    {
        this._mask.style.height = window.getViewportHeight() + "px";
		this._mask.style.width = window.getViewportWidth() + "px";
		this._mask.style.top = window.getScrollTop() + "px";
		this._mask.style.left = window.getScrollLeft() + "px";
    },

    get_isStatic: function () { return true; }
}

CamstarPortal.WebControls.Modal.registerClass('CamstarPortal.WebControls.Modal', null, Sys.IDisposable, Camstar.UI.IUIComponent);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
