// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("Camstar.WebPortal.Personalization");

/******************* Camstar.WebPortal.Personalization.CollapsableState *******************/
if (Camstar.WebPortal.Personalization.CollapsableState == undefined) 
{
    Camstar.WebPortal.Personalization.CollapsableState = function() { };

    Camstar.WebPortal.Personalization.CollapsableState.prototype =
    {
        Collapsed: 0,
        Expanded: 1
    };
    Camstar.WebPortal.Personalization.CollapsableState.registerEnum("Camstar.WebPortal.Personalization.CollapsableState", false);    
}

/******************* CamstarPortal.WebControls.SectionDropDown *******************/
CamstarPortal.WebControls.SectionDropDown = function (element) {
    CamstarPortal.WebControls.SectionDropDown.initializeBase(this, [element]);
    this._collapsableTableId = null;
    this._collapseImageId = null;
    this._collapseImage = null;
    this._collapseTable = null;
    this._controlID = null;
    this._enabled = true;
    this._resizable = false;
    this._hidden = false;
    this._hiddenFieldId = null;
    this._hiddenField = null;
    this._width = null;
    this._height = null;
    this._panel = null;
    this._hasAltText = false;
}

CamstarPortal.WebControls.SectionDropDown.prototype =
{
    initialize: function () {
        CamstarPortal.WebControls.SectionDropDown.callBaseMethod(this, 'initialize');

        var me = this;
        this._collapseImage = $get(this.get_collapseImageId());

        if (this._enabled && this._collapseImage) {
            $(this._collapseImage).click(function (e) { return me._onToggleImage(e); });
        }

        let disabled = $(this._collapseImage).is('[disabled=disabled]');
        if (disabled && this._state === 1) {
            $(".ui-flyout").hide();
            this._state = 0;
            $get(this.get_hiddenFieldId()).value = 0;
        }

        var $altTextSpan = $('.cs-section-alttext', $(this._collapseImage).parent().parent());
        if ($altTextSpan.length)
        {
            this._hasAltText = true;
            if (!$altTextSpan.is('[disabled=disabled]'))
            {
                $altTextSpan.css('cursor', 'pointer');
                $altTextSpan.click(function (e) {
                    return me._onToggleImage(e);
                });
            }
            else
            {
                $altTextSpan.css('cursor', 'default');
            }
        }

        //comment-out to fix Bug 294401
        //$(".ui-flyout").css({ 'min-height': '300px' });
        $(".ui-flyout").css({ 'position': '' });

        this._collapseTable = $get(this.get_collapsableTableId());
        if (this._resizable) 
        {
            var height = $(".ui-flyout-container").height();
            var width = $(".ui-flyout-container").width();
            $(".ui-flyout-container").resizable({ minHeight: height, minWidth: width,
                helper: "ui-resizable-helper",
                stop: function (event, ui)
                {
                    var deltaY = ui.size.height - ui.originalSize.height;
                    var panel = $(".ui-flyout-panel", $(ui.element));
                    var pHeight = panel.height() + deltaY;
                    if (pHeight > 0)
                        panel.height(pHeight);
                    $(ui.element).trigger('resized');
                    $(ui.element).height('auto');
                }
            });
        }

        $(this._element).find("a.cs-button-image").each(function () {
            var link = $(this);
            link.click(function (e) {
                $(".ui-flyout").hide();
                me._state = 0;
                $get(me.get_hiddenFieldId()).value = 0;
                return true;
            });
        });

        // Parent container size is not ready at the initial. Then panel adjusting is queued by timer
        setTimeout(function () { me.movePanel(); }, 0);
    },

    movePanel: function () {
        var inputElement = this._collapseImage.parentNode;

        var tempControl = inputElement;
        var top = inputElement.offsetHeight + $(inputElement).parent().offset().top;
        if (top + $(".ui-flyout-container").height() + $(".pointer").height() > $(window).height()) {
            top = $(inputElement).parent().offset().top - $(".ui-flyout-container").height() - $(".pointer").height();
        }

        var left = 0;
        if (inputElement.clientLeft)
            left = inputElement.clientLeft;

        while (tempControl != null) {
            left += tempControl.offsetLeft;
            // scrollLeft is initialized in IE when textbox contains long value, so PickList appeares too far on the left.
            if (tempControl.tagName.toLowerCase() != 'input')
                left -= tempControl.scrollLeft;
            tempControl = tempControl.offsetParent;
        }
        this._panel = this._collapseTable.parentNode.parentNode.parentNode;
        var leftPosition = left - 300 + inputElement.offsetWidth + 20;
        if (this._hasAltText)
        {
            // Move to the center of image horizontally
            var $altText = $('.cs-section-alttext', this.get_element());
            var rect = $altText[0].getBoundingClientRect();
            leftPosition = rect.left + (rect.width / 2) - 260 /* triangle pointer left */ + 20 /*triangle pointer half-width*/;
            if ($(this._panel).hasClass('leftshift'))
            {
                leftPosition -= 20;
            }

            // and a little lower
            top += 10;
        }

        if (leftPosition < 10)
            leftPosition = 10;

        if ($(document.body).hasClass("Horizon-theme")) {
            if (this._controlID == "ctl00$WebPartManager$MaintMgmtDetailsWP$DocumentDropdown") {
                var pointer = this._panel.firstElementChild;
                top = inputElement.offsetHeight + pointer.offsetHeight / 2;
                var inputElementMidpoint = this._element.offsetLeft + inputElement.offsetWidth / 2;
                leftPosition = inputElementMidpoint - pointer.offsetLeft - pointer.offsetWidth / 2;

                var webpartRect = $(inputElement).closest('.webpart').get(0).getBoundingClientRect();
                var panelBottom = this._panel.lastElementChild.getBoundingClientRect().bottom;
                var panelTop = pointer.getBoundingClientRect().top;
                if (panelBottom > webpartRect.bottom) {
                    bottomDifference = panelBottom - webpartRect.bottom;
                    if (panelTop - bottomDifference >= webpartRect.top) {
                        top -= bottomDifference;
                    }
                    else {
                        top = top - (panelTop - webpartRect.top);
                    }
                }
            } else {
                var $pointer = $(".pointer", this._panel);
                var $btn = $(inputElement);
                top = $btn.offset().top + $btn.height() + $pointer.height() - 10;
                leftPosition = ($btn.offset().left + $btn.width()) - $(".ui-flyout-container", this._panel).width() + 20;
            }
        }

        this._panel.style.left = leftPosition + "px";
        this._panel.style.top = top + "px";
        this._panel.style.width = "100%";
        this._button = $('.cs-button', this._panel);
        var me = this;
        if (this._button) {
            $(this._button).click(function (e) { return me._onToggleImage(e); });
        }

        $(this._button).removeClass("cs-button");
        $(".ui-flyout-footer", $(this._panel).parent()).append(this._button);
    },

    dispose: function () {
        this._collapsableTableId = null;
        this._collapseImageId = null;
        this._collapseImage = null;
        this._collapseTable = null;
        this._controlID = null;
        CamstarPortal.WebControls.SectionDropDown.callBaseMethod(this, 'dispose');
    },

    _onToggleImage: function () {
        if (this._state == 0) {
            this.expand();
            $("#scrollablepanel").animate({ scrollTop: 0 }, 0);
        }
        else {
            this.collapse();
        }
    },

    collapse: function () {
        this._state = 0;
        $get(this.get_hiddenFieldId()).value = 0;

        __page.postback(this.get_controlID(), this._state, GetAdditionalInput(this));
        __page._additionalInput = '';
    },

    expand: function () {
        this._state = 1;
        $get(this.get_hiddenFieldId()).value = 1;

        __page.postback(this.get_controlID(), this._state, GetAdditionalInput(this));
        __page._additionalInput = '';
    },

    get_width: function () { return this._width; },
    set_width: function (value) { this._width = value; },

    get_height: function () { return this._height; },
    set_height: function (value) { this._height = value; },

    get_collapsableTableId: function () { return this._collapsableTableId; },
    set_collapsableTableId: function (value) { this._collapsableTableId = value; },

    get_collapseImageId: function () { return this._collapseImageId; },
    set_collapseImageId: function (value) { this._collapseImageId = value; },

    get_hiddenFieldId: function () { return this._hiddenFieldId; },
    set_hiddenFieldId: function (value) { this._hiddenFieldId = value; },

    get_state: function () { return this._state; },
    set_state: function (value) { this._state = value; },

    get_controlID: function () { return this._controlID; },
    set_controlID: function (value) { this._controlID = value; },

    get_enabled: function () { return this._enabled; },
    set_enabled: function (value) { this._enabled = value; },

    get_resizable: function () { return this._resizable; },
    set_resizable: function (value) { this._resizable = value; }
}

function GetAdditionalInput(element)
{
    var additionalInput = null;
    if (element.name)
    {
        if (element.tagName === 'INPUT')
        {
            var type = element.type;
            if (type === 'submit')
            {
                additionalInput = element.name + '=' + encodeURIComponent(element.value);
            }
        }
        else if ((element.tagName === 'BUTTON') && (element.name.length !== 0) && (element.type === 'submit'))
        {
            additionalInput = element.name + '=' + encodeURIComponent(element.value);
        }
    }
    return additionalInput;
}

// Optional descriptor for JSON serialization.
CamstarPortal.WebControls.SectionDropDown.descriptor =
{
    properties:
    [
        { name: 'collapsableTableId', type: String }, 
        { name: 'collapseImageId', type: String },
        { name: 'hiddenFieldId', type: String },
        { name: 'state', type: Number },
        { name: 'controlID', type: String },
        { name: 'enabled', type: Boolean }
    ]
}

CamstarPortal.WebControls.SectionDropDown.registerClass('CamstarPortal.WebControls.SectionDropDown', Camstar.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
