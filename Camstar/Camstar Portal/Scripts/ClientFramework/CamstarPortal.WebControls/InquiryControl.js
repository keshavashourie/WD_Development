// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.InquiryControl = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.InquiryControl.initializeBase(this, [element]);

    this._tooltip = null;
    this._trigger = null;
    this._header = null;
    this._content = null;

    this.tooltip = null;
    this.trigger = null;
    this.header = null;
    this.content = null;
};

Camstar.WebPortal.FormsFramework.WebControls.InquiryControl.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.InquiryControl.callBaseMethod(this, 'initialize');

        var me = this;

        this.tooltip = $(this._tooltip);
        this.trigger = $(this._trigger);
        this.header = $(this._header);
        this.content = $(this._content);

        var maxWidth = 100;
        if ($(this._element).css('max-width')) 
        {
            //if the element is hidden, NaN is returned
            maxWidth = parseInt($(this._element).css('max-width')) - 10;
        }

        if (isNaN(maxWidth) || this._element.scrollWidth <= maxWidth)
        {
            this.tooltip.hide();
        }
        else
        {
            this.trigger.addClass("arrow-left");
            this.trigger.bind('click', function (e) { return me.click(e); });
        }
    },

    dispose: function ()
    {
        if (this.trigger)
            this.trigger.unbind();

        Camstar.WebPortal.FormsFramework.WebControls.InquiryControl.callBaseMethod(this, 'dispose');
    },

    click: function ()
    {
        this.content.text($(this._element).text());
        if (this.content.is(":visible"))
            this.hideTooltip();
        else
            this.showTooltip();

        return false;
    },

    showTooltip: function ()
    {
        //close all other opened tooltips
        var tls = $('div.inquiry_tooltip').prev();
        for (var i = 0; i < tls.length; i++)
            $find(tls[i].id).hideTooltip();

        var pos = $(this._tooltip).position();
        this._header.style.top = pos.top + this.tooltip.height() + 'px';
        this._header.style.left = pos.left - this.tooltip.width() / 3 * 2 + 'px';
        this._content.style.top = this._header.style.top;
        this._content.style.left = this._header.style.left;

        this.trigger.removeClass("arrow-left");
        this.trigger.addClass("arrow-down");
        this.content.show().css('z-index', '999');
        this.header.show().css('z-index', '999');
    },

    hideTooltip: function ()
    {
        this.trigger.removeClass("arrow-down");
        this.trigger.addClass("arrow-left");
        this.content.fadeOut('fast');
        this.header.fadeOut('fast');
    },

    get_tooltip: function () { return this._tooltip; },
    set_tooltip: function (value) { this._tooltip = value; },

    get_trigger: function () { return this._trigger; },
    set_trigger: function (value) { this._trigger = value; },

    get_header: function () { return this._header; },
    set_header: function (value) { this._header = value; },

    get_content: function () { return this._content; },
    set_content: function (value) { this._content = value; }
},

Camstar.WebPortal.FormsFramework.WebControls.InquiryControl.registerClass('Camstar.WebPortal.FormsFramework.WebControls.InquiryControl', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
