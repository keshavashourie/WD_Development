// Copyright Siemens 2025

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../Camstar.WebPortal.PortalFramework/WebPartBase.js" />

Type.registerNamespace("Camstar.WebPortal.WebPortlets.Concierge");

Camstar.WebPortal.WebPortlets.Concierge = function (element) {
    Camstar.WebPortal.WebPortlets.Concierge.initializeBase(this, [element]);
    this._refreshButton = null;
    this._selectedSection = null;
    this._isRefreshInProcess = false;
    this._sections = new Array();
}

Camstar.WebPortal.WebPortlets.Concierge.prototype = {
    initialize: function () {
        Camstar.WebPortal.WebPortlets.Concierge.callBaseMethod(this, 'initialize');

        this._refreshEditDelegate = Function.createDelegate(this, this._onRefreshEdit);
        this._headerClickDelegate = Function.createDelegate(this, this._onHeaderClick);

        for (var i = 0; i < this._sections.length; i++) {
            this.wireElement($get(this._sections[i]), this._headerClickDelegate);
        }

        //TODO:get element ids dynamically
        this._refreshButton = $get('ctl00_WebPartManager_ConciergeControl_RefreshButton');

        if (this._refreshButton)
            this.wireElement(this._refreshButton, this._refreshEditDelegate);

        var $conciergeSection = $($(".ui-concierge-sectioncontent")[this.get_selectedSection()]);
        $conciergeSection.prop("selected", true);

        if (document.body.classList.contains("Horizon-theme")) {
            $("img", $conciergeSection.prev(".ui-concierge-section")).prop("src", "Themes/Horizon/Images/Icons/icon-collapse-16x16.svg");
            var availabilityItem = $conciergeSection.prev(".ui-concierge-section").attr("items");

            if (availabilityItem == "True") {
                $conciergeSection.addClass("expanded-table");
                $conciergeSection
                    .addClass("expanded-concierge-sectioncontent")
                    .prev(".ui-concierge-section")
                    .addClass("expanded-concierge-section");
            }
        }

        this.set_zoneElement($(this.get_element()).closest("table.zone")[0]);
    },

    wireElement: function (element, delegate) {
        $clearHandlers(element);
        $addHandlers(element, { 'click': delegate }, this);
    },

    _onRefreshEdit: function (e) {
        if (e && e.preventDefault) {
            //prevent postback in FF
            e.preventDefault();
        }

        e.rawEvent.cancelBubble = true;
        e.stopPropagation();

        for (var i = 0; i < this._sections.length; i++) {
            var section = $get(this._sections[i]);
            if (section)
                $(section).children()[1].innerHTML = "";
        }
        var selSectionElement = $get(this._sections[this._selectedSection]);

        if (selSectionElement)
            $(selSectionElement).children()[1].innerHTML = "";

        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
        transition.set_command("RefreshConciergeSection");
        transition.set_commandParameters(selSectionElement.attributes['SectionType'].value);
        transition.set_clientCallback("refreshSectionContent");

        var communicator = new Camstar.Ajax.Communicator(transition, this);
        communicator.syncCall();

        return false;
    },

    _onHeaderClick: function (e) {
        if (this._isRefreshInProcess)
            return false;

        var header = e.target;
        while (header && (header.className !== "ui-concierge-section")) {
            header = header.parentNode;
        }

        if (!header)
            return false;

        var selSectionElement = $get(this._sections[this._selectedSection]);

        e.stopPropagation();

        if (selSectionElement.id == header.parentNode.id)
            return false;

        if (selSectionElement) {
            $(selSectionElement).children()[1].innerHTML = "";
        }

        for (var i = 0; i < this._sections.length; i++) {
            if (header.parentNode.id == this._sections[i]) {
                this._selectedSection = i;
                var $conciergeSectioncontent = $($(".ui-concierge-sectioncontent")[i]);
                var $conciergeSectionS = $(".apollo-concierge .ui-flyout-panel span");
                var availabilityItem = $conciergeSectioncontent.prev(".ui-concierge-section").attr("items");

                $conciergeSectioncontent.attr("selected", true);

                if (document.body.classList.contains("Horizon-theme")) {
                    $("img", $(".ui-concierge-sectioncontent").prev(".ui-concierge-section")).prop("src", "Themes/Horizon/Images/Icons/icon-expand-16x16.svg");

                    $conciergeSectionS.each(function (index) {
                        $(this)
                            .children(".ui-concierge-section")
                            .eq(0).removeClass("expanded-concierge-section");
                        $(this)
                            .children(".ui-concierge-sectioncontent")
                            .eq(0).removeClass("expanded-concierge-sectioncontent");
                    });

                    if (availabilityItem == "True") {
                        $conciergeSectioncontent.addClass("expanded-table");
                        $conciergeSectioncontent.eq(0)
                            .addClass("expanded-concierge-sectioncontent")
                            .prev(".ui-concierge-section")
                            .addClass("expanded-concierge-section");
                    }

                    $("img", $conciergeSectioncontent.prev(".ui-concierge-section")).attr("src", "Themes/Horizon/Images/Icons/icon-collapse-16x16.svg");
                }
            }
            else
                $($(".ui-concierge-sectioncontent")[i]).attr("selected", false);
        }

        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
        transition.set_command("RefreshConciergeSection");
        transition.set_commandParameters(header.parentNode.attributes['SectionType'].value);
        transition.set_clientCallback("refreshSectionContent");

        this._isRefreshInProcess = true;
        var communicator = new Camstar.Ajax.Communicator(transition, this);
        communicator.syncCall();

        return false;
    },

    refreshSectionContent: function (responseSection) {
        if (responseSection && responseSection.Data) {
            var selSectionElement = $get(this._sections[this._selectedSection]);
            $(selSectionElement).children()[1].innerHTML = responseSection.Data.Message;
        }
        __page.hideModal();
        this._isRefreshInProcess = false;
        __page.doReload = false;
    },

    refreshContent: function () {
        this._refreshButton.click();
    },

    dispose: function () {
        Camstar.WebPortal.WebPortlets.Concierge.callBaseMethod(this, 'dispose');
    },

    doToggle: function (keepOpened) {
        if (keepOpened == true && this.get_opened() || keepOpened == false && !this.get_opened())
            return;

        var opened = this.get_opened();

        var time = opened ? 1000 : 500;
        var zone = $(this.get_zoneElement());

        zone.toggle("slide", { direction: "up" }, time);
    },

    toggle: function (keepOpened) {
        setTimeout("__toppage.getConcierge().doToggle(" + keepOpened + ");", 0);
    },

    close: function () {
        if (this.get_opened()) {
            //  calling toggle can actually result in opening the concierge.  simply hide it.
            $(this.get_zoneElement()).hide();
            //this.doToggle(false);
        }
    },

    get_ownershipSection: function () { return mOwnershipSection; },

    get_workItemSection: function () { return mWorkItemSection; },

    get_pendingSection: function () { return mPendingSection; },

    get_opened: function () {
        var zoneElement = this.get_zoneElement();
        if (zoneElement)
            return zoneElement.style.display !== "none";
        return false;
    },

    get_isStatic: function () { return true; },

    get_sections: function () { return this._sections; },
    set_sections: function (value) { this._sections = value; },

    get_selectedSection: function () { return this._selectedSection; },
    set_selectedSection: function (value) { this._selectedSection = value; }
}

Camstar.WebPortal.WebPortlets.Concierge.registerClass('Camstar.WebPortal.WebPortlets.Concierge', Camstar.WebPortal.PortalFramework.WebPartBase);

Camstars.Controls.ConciergeSection = function (elementId) {
    var me = this;
    var mElement = null;
    var mItems = null;
    var mExpanded = false;
    var mMaxShown = 9;

    var load = function () {
        mElement = $get(elementId);
        if (!mElement) return;
        mItems = $($(mElement.rows[1].cells[0]).children()[0]).children()[0];
    }

    me.toggleHide = function () {
        load();
        if (mItems.style.display == "none")
            mItems.style.display = "";
        else
            mItems.style.display = "none";
    }

    me.toggleMore = function () {
        load();
        var mItemFirstElement;
        mItemFirstElement = $(mItems.rows[mItems.rows.length - 1].cells[0]).children()[0];

        if (mItemFirstElement.innerHTML == "More...")
            mExpanded = false;

        var className = mExpanded ? "trHidden" : "";
        var innerHTML = mExpanded ? "More..." : "Less...";
        if (mItems.rows.length > 5) {
            for (var x = 5; x < mItems.rows.length - 1; x++)
                mItems.rows[x].className = className;

            mItemFirstElement.innerHTML = innerHTML;
            mExpanded = !mExpanded;
            mItems.parentNode.className = mExpanded ? "divExpanded" : "";

            if (mItems.rows.length < 10)
                mItems.parentNode.style.overflow = "hidden";
        }
    }

    me.isExpanded = function () {
        return mExpanded;
    }

    Camstars.Event.addEvent(window, "load", load);
}

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();