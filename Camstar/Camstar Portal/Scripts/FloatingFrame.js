// Copyright Siemens 2025  

/// <reference path="./ClientFramework/MicrosoftAjaxExt.js"/>
/// <reference path="./ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");

Camstars.Controls.FloatingFrame = function ()
{
    var me = this;
    var mContainer = null;
    var mTitle = "";
    var mMouseOffset = 0;
    var mWidth = 0;
    var mHeight = 0;
    var mTop = 0;
    var mLeft = 0;
    var mShowButtons = true;
    var mCloseButtonOnly = false;
    var mDisplayReset = false;
    var mAjaxFloating = false;
    var mCancelConfirmation = null;
    var mImgClose = null;
    this._restoredDimensions = [];
    var mOptionArgs = null;
    var mIsExternal = false;
    var flagLineAssignment = false;
    var propWidthViewPortPopUp = 0;
    var propHeightViewPortPopUp = 0;
    var FloatingFrameIFrame = null;
    var isCamstarClassicFlag = false;

    var defaultWidthLineAssignment = 660;
    var floatHeaderHeight = 40;
    var minHeightLineAssignment = 470;

    me.showAjax = function (url, title, height, width, top, left, showButtons, okButtonText, closeButtonText, element, closeOnCancel, optionArgs, cancelConfirmMsg, closeButtonOnly, displayReset, loadingTitle) {
        me.EnableParentTabPointerClicks(false);
        mAjaxFloating = true;
        me.showWin(url, title, height, width, top, left, showButtons, okButtonText, closeButtonText, element, closeOnCancel, optionArgs, cancelConfirmMsg, closeButtonOnly, displayReset, loadingTitle);
    },

    me.show = function (url, title, height, width, top, left, showButtons, okButtonText, closeButtonText, element, closeOnCancel, optionArgs, cancelConfirmMsg, closeButtonOnly, displayReset, loadingTitle) {
        mAjaxFloating = false;
        me.showWin(url, title, height, width, top, left, showButtons, okButtonText, closeButtonText, element, closeOnCancel, optionArgs, cancelConfirmMsg, closeButtonOnly, displayReset, loadingTitle);
    },

    me.showWin = function (url, title, height, width, top, left, showButtons, okButtonText, closeButtonText, element, closeOnCancel, optionArgs, cancelConfirmMsg, closeButtonOnly, displayReset, loadingTitle) {
        //check to see if the control is enabled
        if (element.childNodes != null && !IsControlEnabled(element))
            return;
       
        mContainer = $get("FloatingFrame_frame").parentNode.parentNode;
        mContainer.style.height = "";
        if (width && height)
        {
            // set width and height
            mContainer.style.width = width + "px";
            var innerContent = $(".content", mContainer)[0];

            mShowButtons = showButtons;
            mCloseButtonOnly = closeButtonOnly;
            innerContent.style.height = (height - floatHeaderHeight) + "px";
            var header = $(mContainer).find(".floating-frame .header")[0];
            if (header)
                header.style.width = (width - 80) + "px";
        }

        //wire events to handle resizing
        $addHandler(window, "resize", me.resize, false);
        $addHandler(window, "scroll", me.center, false);
        $addHandler(window, "DOMMouseScroll", me.center, false);
        $addHandler($get("FloatingFrame_frame"), "load", me.showFrame, false);

        mTitle = title;
        if (typeof (displayReset) != "undefined")
            mDisplayReset = displayReset;
        mTop = top;
        mLeft = left;
        mCancelConfirmation = null;
        mOptionArgs = optionArgs;
        if (mImgClose != null) {
            mImgClose.title = __page.get_CloseLabelText();
        }

        if (typeof (cancelConfirmMsg) != "undefined") {
            // cancel confirmation message
            mCancelConfirmation = cancelConfirmMsg;
        }

        if ((width && height) && (width > 0 && height > 0)) {
            this._modal = new CamstarPortal.WebControls.Modal();
            this._modal.show();

            me.center();
            mContainer.style.display = "block";
            oIframe().style.display = "block";
        }
        else {
            __page.showModal();
        }
        if (mOptionArgs) {
            if (mOptionArgs.isExternal)
                mIsExternal = mOptionArgs.isExternal;
            if (mOptionArgs.scrolling)
                oIframe().scrolling = mOptionArgs.scrolling;
            if (mOptionArgs.zIndex)
                $(".floating-frame").css("z-index", mOptionArgs.zIndex);
        }

        if (document.body.classList.contains("body-modeling") && !mIsExternal) {
            url += "&frameStyle=modeling";
        }
        // Get Loading Text for all Languages to remove hard coding of "Loading..."
        var labels = [{ Name: 'Lbl_PopupLoadingTitle' }];
        if (typeof (__page) !== 'undefined') {
            var loading = __page.getLabels(labels, function (response) {
                if ($.isArray(response)) {
                    var value;
                    $.each(response, function () {
                        var labelname = this.Name;
                        var labeltext = this.Value;
                        switch (labelname) {
                            case 'Lbl_PopupLoadingTitle':
                                value = labeltext;
                                break;
                            default:
                                break;
                        }
                    });
                    oIframe().src = url;
                    oTitle().innerHTML = loadingTitle ? loadingTitle : value;
                    return false;
                }
                else {
                    alert(response.Error);
                }
            });
        }
        else
        {
            oIframe().src = url;
            oTitle().innerHTML = loadingTitle ? loadingTitle : "Loading...";
            return false;
        }
    },

    me.showFrame = function () {
        var titleText;

        if (!mIsExternal) {
			try {
                FloatingFrameIFrame = $(mContainer).find("iframe:first").get(0).contentWindow.document.body;
			} catch(err) {
			}
			if (FloatingFrameIFrame)
				FloatingFrameIFrame.addEventListener("changeElement", function (event) {
					me.center();
					event.stopPropagation();
				}, true);
        }

        if (mTitle == "")
            titleText = oIframe().contentDocument ? oIframe().contentDocument.title : oIframe().document.title;
        else
            titleText = mTitle;
        oTitle().innerHTML = titleText;
        $(mContainer).attr('frame-title', titleText);

        if (!isCamstarClassicFlag)
            flagLineAssignment = me.getSetLineAssignment();

        hideLoadingDiv();
        $(mContainer).resizable({
            helper: "ui-resizable-helper",
            zIndex: 99999 + 1, // many popup pages has 99999 z-index specified.
            stop: function (event, ui) {
                var content = $('.content', $(ui.element));
                var header = $('.header-parent', $(ui.element));
                content.height(ui.size.height - header.outerHeight());
                $(ui.element).css({ height: 'auto' });
                me.center();
            },
        });

        if (typeof ($(mContainer).draggable) !== 'undefined') {
            if (typeof (__page) !== 'undefined' && __page.isMobilePage()) {
                // do not enable drag on mobile
                $(mContainer).draggable({
                    disabled: true
                });
            }
            else {
                $(mContainer).draggable();
            }
        }

        try {
            var frame = $(".floating-frame, .ui-dialog").filter(":visible");
            if (frame.length > 0) {
                var frameDoc = oIframe().contentWindow.document;
                if (document.body.classList.contains("reg-FontSize"))
                    frameDoc.body.classList.add("reg-FontSize");

                mWidth = $(frameDoc).width();
                if (mWidth > $(window).width())
                    frame.outerWidth('100%');
                var height = $(frameDoc).height() + floatHeaderHeight;
                if (height > $(window).height())
                    me.maximizeHeight(frame);
            }
        }
        catch (exc) {
        }
		me.center();
		me.EnableParentTabPointerClicks(true);

        propWidthViewPortPopUp = $(mContainer).width() / window.document.body.offsetWidth;
        propHeightViewPortPopUp = $(mContainer).height() / window.document.body.offsetHeight;
    },
    me.getSetLineAssignment = function (value) {
        if (!arguments.length) {
            flagLineAssignment = $(FloatingFrameIFrame).hasClass("line-assignment") ? true : false;
            return flagLineAssignment;
        }
        flagLineAssignment = value;
    },
    me.maximizeHeight = function (frame)
    {
        frame.outerHeight('100%');
        var content = frame.find('.content, .ui-dialog-content').filter(":visible");
        var header = frame.find('.header-parent, .ui-dialog-titlebar').filter(":visible");

        if (content.length > 0 && header.length > 0)
        {
            var buttonsPane = frame.find('.ui-dialog-buttonpane').filter(":visible");
            var buttonsPaneHeight = 0;
            if (buttonsPane.length > 0)
            {
                buttonsPaneHeight = buttonsPane.outerHeight();
            }
            content.outerHeight(frame.height() - header.outerHeight() - buttonsPaneHeight);
        }
    },

    me.setTitle = function (title)
    {
        mTitle = title;
    },

    me.getTitle = function ()
    {
        return mTitle;
    },

    me.hide = function ()
    {        
        var $activeTab = __page.$getActiveTabPanel();
        if ($activeTab.length) {
            $activeTab.removeAttr("popupHelpUrl");
        }

        $clearHandlers(window);
        $clearHandlers($get("FloatingFrame_frame"));

        mWidth = 1000;
        $(mContainer).css({ 'width': mWidth + 'px' });

        this.normalize();

        var internalFrame = oIframe();
        if (internalFrame.document) internalFrame.document.close();
        $(internalFrame).html('');

        if (internalFrame.contentDocument && internalFrame.contentDocument.forms[0]) 
            internalFrame.contentDocument.forms[0].style.display = "none";

        oTitle().innerHTML = '';

        if (mContainer !== null)
        {
            if (typeof mContainer.style !== "undefined") {
                mContainer.style.display = "none";
            }
            for (var i = 0; i < mContainer.length; i++) {
                if (mContainer[i].style !== null) {
                    mContainer[i].style.display = "none";
                }
            }
        }

        if (this._modal)
            this._modal.hide();

        if (mOptionArgs && mOptionArgs.callerId) {
            // instead of string pass complex object ({callerId:'', gridRowId: ''})
            __page.setFocus(mOptionArgs);
        }

        return false;
    },

    me.center = function () {
        var top = window.getScrollTop() + ((window.getViewportHeight() - $(mContainer).outerHeight()) / 2);
        if (top < 0)
            top = 0;
        var left = window.getScrollLeft() + ((window.getViewportWidth() - $(mContainer).width()) / 2);
        if (left < 0)
            left = 0;
        $(mContainer).css({ 'top': top, 'left': left }); //set position

        if (!mShowButtons)
            $(oIframe()).contents().find("#nonscrollablepanel").hide();

        if (flagLineAssignment) {
            //  uncomment this line to hide the X button to close the popup
            //$('.ui-floatingframe-close').hide();
            $(mContainer)
                .css("min-height", minHeightLineAssignment + "px")
                .find(".content:first")
                .css({ "min-height": (minHeightLineAssignment - 50) + "px", "width": "100%" });

            var $matrixFloatinFrame = $('.float-form-container .webpart .matrix:first-child', GetFloatingFrame().document);
            
            if ($matrixFloatinFrame && $(mContainer).width() < defaultWidthLineAssignment) { // fix bug where error occurs in IE if Set Line Assignment is selected first
                $matrixFloatinFrame.addClass("small-assignment");
            }
            else if ($matrixFloatinFrame) { // fix bug where error occurs in IE if Set Line Assignment is selected twice
                $matrixFloatinFrame.removeClass("small-assignment");
            }

        }

    };

    me.resize = function () {
        if (propWidthViewPortPopUp && propHeightViewPortPopUp && !isCamstarClassicFlag) {

            if (propWidthViewPortPopUp != $(mContainer).width() / window.document.body.offsetWidth || propHeightViewPortPopUp != $(mContainer).height() / window.document.body.offsetHeight) {
                $(mContainer).css({ height: (window.document.body.offsetHeight * propHeightViewPortPopUp) + "px", width: (window.document.body.offsetWidth * propWidthViewPortPopUp) + "px" });
                $(".content", mContainer).css("height", (window.document.body.offsetHeight * propHeightViewPortPopUp - 50) + "px");
            }

        }

        me.center();
    }

    var dimensions = this._restoredDimensions;

    me.maximize = function ()
    {
        var maximizeButton = $(".ui-floatingframe-normalize, .ui-floatingframe-max").filter(":visible");
        if (maximizeButton.length > 0)
        {
            var frame = $(".floating-frame, .ui-dialog").filter(":visible");
            if (frame.length > 0)
            {
                var content = frame.find('.content, .ui-dialog-content').filter(":visible");
                var header = frame.find('.header-parent, .ui-dialog-titlebar').filter(":visible");
                if (content.length > 0 && header.length > 0)
                {
                    var isNormalSize = maximizeButton.hasClass('ui-floatingframe-max');
                    if (isNormalSize)
                    {
                        dimensions[0] = frame.position();
                        dimensions[1] = frame.outerWidth();
                        dimensions[2] = content.height();
                        frame.outerWidth('100%');
                        frame.css({ left: 0, top: 0 });
                        me.maximizeHeight(frame);
                        
                        maximizeButton.toggleClass('ui-floatingframe-max ui-floatingframe-normalize');

                        var label = maximizeButton.attr('restoreLabel');
                        maximizeButton.attr('title', label);
                    }
                    else
                        this.normalize();
                    if (frame.find("iframe").length) {
                        const event = new Event('panelResized');
                        frame.find("iframe")[0].contentDocument.dispatchEvent(event);
                    }
                }
            }
        }
    };
    me.normalize = function ()
    {
        if (dimensions.length > 0)
        {
            var maximizeButton = $(".ui-floatingframe-normalize").filter(":visible");
            if (maximizeButton.length > 0)
            {
                var frame = $(".floating-frame, .ui-dialog").filter(":visible");
                if (frame)
                {
                    var content = frame.find('.content, .ui-dialog-content').filter(":visible");
                    if (content.length > 0)
                    {
                        frame.css({ height: 'auto', left: dimensions[0].left, top: dimensions[0].top });
                        frame.outerWidth(dimensions[1]);
                        content.height(dimensions[2]);
                        dimensions = [];
                        maximizeButton.toggleClass('ui-floatingframe-max ui-floatingframe-normalize');

                        var label = maximizeButton.attr('maximizeLabel');
                        maximizeButton.attr('title', label);
                    }
                }
            }
        }
    };

    me.onSubmitClick = function (e)
    {
        if (!e) e = window.event;

        var frame = GetFloatingFrame();
        if (frame != null)
        {
            frame.document.getElementById('__EVENTARGUMENT').value = "FloatingFrameSubmitPostbackArgument";
            if (typeof (frame.__page) == 'undefined')
            {
                frame.theForm.submit();
            }
            else
            {
                frame.__page._additionalInput = GetAdditionalInput(e.srcElement || e.target);
                frame.__page.submit();
            }
            frame.document.getElementById('__EVENTARGUMENT').value = '';
        }
        return false;
    },

    me.onResetAllClick = function ()
    {
        var frame = GetFloatingFrame();
        if (frame != null)
        {
            frame.document.getElementById('__EVENTARGUMENT').value = "FloatingFrameResetAllPostbackArgument";
            if (typeof (frame.__page) == 'undefined')
                frame.theForm.submit();
            else
                frame.__page.submit();
            frame.document.getElementById('__EVENTARGUMENT').value = '';
        }
        return false;
    },

    me.onCancelClick = function (e)
    {
        if (mCancelConfirmation)
        {
            var postbackFunction = "pop.PerformCancelClick()";
            JConfirmationShort(mCancelConfirmation, postbackFunction);
            oIframe().style.display = "block";
            return false;
        }
        me.PerformCancelClick();
        return true;
    },

    me.IsShowButtons = function (e)
    {
        return mShowButtons;
    },

    me.PerformCancelClick = function ()
    {
        var notify = false;
        var frame = GetFloatingFrame();
        if (frame != null && !mIsExternal)
        {
			try {
				if (typeof (frame.__page) != 'undefined')
				{
					notify = frame.__page.get_notifyParentOnClose();
				}
			} catch(err) {
			}
        }
        CloseFloatingFrame(notify);
    }

    me.GetIFrameVirtualPage = function ()
    {
        if (mIsExternal)
            return "";

        var frame = GetFloatingFrame();
        if (!frame || typeof (frame.__page) == 'undefined')
            return "";

        return frame.__page.get_virtualPageName();
    }

    me.IsCloseButtonOnly = function ()
    {
        return mCloseButtonOnly;
    }
    me.GetCallerPage = function ()
    {
        return GetFloatingFrame().parent.parent;
    }

    me.EnableParentTabPointerClicks = function (e)
    {
        var parentTopDocument = window.parent.document;
        let elements = parentTopDocument.querySelectorAll("div#tabContainerControl ul#tablist li[role='tab'] a[role='presentation");
        for (var i = 0; i < elements.length; i++) {
            var elem = elements[i];
            if (e) {
                elem.style.pointerEvents = 'auto';
            }
            else {
                elem.style.pointerEvents = 'none';
            }
        }
    }

    var hideLoadingDiv = function ()
    {
        $get("divLoading").style.display = "none";
    }

    var showLoadingDiv = function ()
    {
        $get("divLoading").style.display = "block";
    }

    var IsControlEnabled = function (controlElement)
    {
        var isEnabled = false;
        if (controlElement.childNodes.length > 0)
        {
            for (var i = 0; i < controlElement.childNodes.length; i++)
            {
                var eachChild = controlElement.childNodes[i];
                if (eachChild.childNodes.length > 0)
                {
                    isEnabled = IsControlEnabled(eachChild);
                }
                else
                {
                    if (eachChild.tagName != null)
                    {
                        if (eachChild.tagName.toLowerCase() == "input")
                        {
                            isEnabled = !eachChild.disabled;
                        }
                    }
                    else
                    {
                        isEnabled = true;
                    }
                }
            }
        }
        else if (controlElement.tagName.toLowerCase() == "input")
        {
            isEnabled = !controlElement.disabled;
        }
        return isEnabled;
    } //IsControlEnabled

    var load = function ()
    {
        isCamstarClassicFlag = $(getCEP_top().document.body).hasClass("Camstar-theme") 
        positionFrameInfoZoneContainer();
        mContainer = $("#ctl00_FloatingFrame").find("div:first");   

        var header = $(mContainer).find(".floating-frame .header")[0];
        if (header)
            $addHandler(header, "mousedown", makeDraggable, false);
    }

    var positionFrameInfoZoneContainer = function ()
    {
        var infoZoneContainer = $get("InfoZoneContainer");

        if (infoZoneContainer)
        {
            infoZoneContainer.className = "InfoZoneContainerWithOutTopStaticZone";
        }
    }

    var oTitle = function ()
    {
        return $(mContainer).find("span:first")[0];
    }

    var makeDraggable = function ()
    {
        $(document).mousemove(function (e) { mouseMove(e); });
        $(mContainer).mousedown(function (e) { mouseDown(e); });
        $(document).mouseup(function (e) { mouseUp(e); });
    }

    var mouseDown = function (e)
    {
        mMouseOffset = getMouseOffset(mContainer, e);

        showLoadingDiv();
    }

    var mouseMove = function (e)
    {
        e = e || window.event;

        $(mContainer).css({ 'position': 'absolute', 'top': e.pageY - mMouseOffset.y, 'margin-top': '0px', 'left': e.pageX - mMouseOffset.x, 'margin-left': '0px' });

        return false;
    }

    var mouseUp = function (e)
    {
        hideLoadingDiv();
        $(document).unbind();
        $(mContainer).unbind();
    }

    var getMouseOffset = function (target, e)
    {
        e = e || window.event;

        var docPos = getPosition(target);
        var mousePos = mouseCoords(e);
        return { x: mousePos.x - docPos.x, y: mousePos.y - docPos.y }
    }

    var getPosition = function (e)
    {
        e = e || window.event;
        var left = 0;
        var top = 0;

        while (e.offsetParent)
        {
            left += e.offsetLeft;
            top += e.offsetTop;
            e = e.offsetParent;
        }

        left += e.offsetLeft;
        top += e.offsetTop;
        return { x: left, y: top };
    }

    var oIframe = function ()
    {
        return $get('FloatingFrame_frame');
    }

    var GetFloatingFrame = function ()
    {
        return oIframe().contentWindow;

        //for (var i = 0; i < window.frames.length; i++)
        //{
        //    var eachFrame = window.frames[i];
        //    if (eachFrame.frameElement.id == 'FloatingFrame_frame')
        //    {
        //        return eachFrame;
        //    }
        //}
        //return null;
    }

    var setFirstEsigFocus = function ()
    {
        var inputs = oIframe().contentWindow.document.getElementsByTagName("input");
        for (i = 0; i < inputs.length; i++)
        {
            if (inputs[i].id.indexOf("ESigSignerField") >= 0)
            {
                var ctrlSigner = inputs[i];
                if (ctrlSigner && !ctrlSigner.getAttribute("readonly"))
                {
                    ctrlSigner.focus();
                    break;
                }
                else
                {
                    if (inputs[i + 1].name.indexOf("ESigPasswordField") >= 0)
                    {
                        var ctrlPassword = inputs[i + 1];
                        if (ctrlPassword)
                        {
                            ctrlPassword.focus();
                            break;
                        }
                    }
                }
            }
        }
    }
    $addHandler(window, "load", load);
}

var pop = new Camstars.Controls.FloatingFrame();

function mouseCoords(e)
{
    if (e.pageX || e.pageY)
    {
        return { x: e.pageX, y: e.pageY };
    }
    return {
        x: e.clientX + document.body.scrollLeft - document.body.clientLeft,
        y: e.clientY + document.body.scrollTop - document.body.clientTop
    };
}

function CloseFloatingFrame(notifyParent, keepParentTabOpen)
{ 
    pop.hide();
    var overridePage = pop.GetIFrameVirtualPage();
    if (notifyParent) 
    {
        $get('__EVENTARGUMENT').value = 'FloatingFrameSubmitParentPostBackArgument';
        __page._additionalInput = null;
        if (__page._isTabContainerPage == null && overridePage !== 'OverrideConfirmation_VP') {
            __page.submit();
        }
        else
            __page.refreshHeader();
        $get('__EVENTARGUMENT').value = '';
    }

    if (typeof (keepParentTabOpen) === 'undefined' || !keepParentTabOpen) {
        if (typeof (__page) !== 'undefined') {
            if (overridePage == 'OverrideConfirmation_VP') {
                __page.closeTab('');
            }
        }
    }
}

function CloseChildFrame(notifyParent) {
    if (notifyParent) {
        $get('__EVENTARGUMENT').value = 'ChildFrameSubmitParentPostBackArgument';
        __page._additionalInput = null;
        if (__page._isTabContainerPage == null)
            __page.submit();
        else
            __page.refreshHeader();
        $get('__EVENTARGUMENT').value = '';
    }
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

function RequestESigMode(e, eventArgument, eventTarget)
{
    if (!e) e = window.event;

    var element = e.srcElement || e.target;
    if (element.disabled)
        return;

    var additionalInput = GetAdditionalInput(element);

    __page.postback(eventTarget, eventArgument, additionalInput);
    __page._additionalInput = '';
}

function gridCompleted()
{
    var completed = ! jqgrid_lock();

    return completed;
}

function canvasCompleted() {
    var completed = true;
    $('.canvas').each(
        function () {
            if (completed) {
                var canvas = $find(this.id);
                if (canvas) {
                    if (canvas.get_inProcFuncCounter() != 0)
                        completed = false;
                }
            }
        }
    );

    return completed;
}

function WaitForGridOpCompletionOnSave(eventTarget, eventArgument, elemID)
{
    if (!gridCompleted() || !canvasCompleted() || __page._lock)
    {
        setTimeout(function () { WaitForGridOpCompletionOnSave(eventTarget, eventArgument, elemID); }, 250);
        return false;
    }
    var element = document.getElementById(elemID);
    
    if (element == null || element.disabled)
    {
        return false;
    }
    
    var additionalInput = GetAdditionalInput(element);

    __page._formSubmitElement = element.id;
    __page.postback(eventTarget, eventArgument);
    __page._additionalInput = '';
    __page._formSubmitElement = '';

    return false;
}

function DirtyFlagforNew(eventTarget, eventArgument, elemID)
{
    if (__page.isDirty())
    {
        jConfirmFloatingFrameSetText(eventTarget, eventArgument, elemID);
    }
    else
    {
        WaitForGridOpCompletionOnSave(eventTarget, eventArgument, elemID);
    }
       
}

function FireFloatingFrameConfirm(textLabel, eventTarget, eventArgument, elemID,warning)
{
    jConfirm(textLabel, null, function (r) {
        if (r == true) {
            __page.resetDirty();
            WaitForGridOpCompletionOnSave(eventTarget, eventArgument, elemID);
        }
    }, warning);
}
function jConfirmFloatingFrameSetText(eventTarget, eventArgument, elemID)
{
    var labels = [{ Name: 'Lbl_UnsavedChangesForInstances' }, { Name: 'Lbl_Warning'}];
    __page.getLabels(labels, function (response) {
        if ($.isArray(response)) {
            var value;
            var warning;
            $.each(response, function () {  
                var labelname = this.Name;
                var labeltext = this.Value;
                switch (labelname) {
                    case 'Lbl_UnsavedChangesForInstances':
                        value = labeltext;
                        break;
                    case 'Lbl_Warning':
                        warning = labeltext;
                        break;
                    default:
                        break;
                }
            });
            FireFloatingFrameConfirm(value, eventTarget, eventArgument, elemID,warning);
        }
        else {
            alert(response.Error);
        }
    });
}

function RequestESigModeWithElemID(eventArgument, eventTarget, elemID) {
    if (pop.GetCallerPage().pop.IsCloseButtonOnly())
    {
        pop.GetCallerPage().pop.hide();
        return false;
    }    

    if (!gridCompleted())
    {
        setTimeout(function () { RequestESigModeWithElemID(eventArgument, eventTarget, elemID); }, 250);
        return false;
    }

    var element = document.getElementById(elemID);
    if (element == null || element.disabled)
    {
        return false;
    }

    var additionalInput = GetAdditionalInput(element);

    __page._formSubmitElement = element.id;
    __page.postback(eventTarget, eventArgument, additionalInput);
    __page._additionalInput = '';
    __page._formSubmitElement = '';
    return false;
}

function CloseFrame(eventTarget, eventArgument)
{
    pop.onCancelClick(null);
    if (__page)
        __page.showHomePage();
}
