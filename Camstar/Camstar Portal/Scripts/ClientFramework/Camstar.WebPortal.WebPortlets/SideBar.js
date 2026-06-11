// Copyright Siemens 2025

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.WebPortal.PortalFramework/WebPartPageBase.js" />

Type.registerNamespace("Camstar.WebPortal.WebPortlets");
Type.registerNamespace("Camstar.WebPortal.PortalFramework");

Camstar.WebPortal.WebPortlets.SideBar = function (element) {
    Camstar.WebPortal.WebPortlets.SideBar.initializeBase(this, [element]);

    this._controlId = null;
    this.cssIconTheme = "Horizon";
    this.actionsSelector = "#WebPart_ButtonsBar input, #WebPart_ButtonsBar button";
    this._commandSettings = null;
    this._haveHiddenButtons = false;
    this._hiddenButtons = [];
    this._txnLabels = [
        { Name: 'Lbl_ShopFloorTxns', DefaultValue: "Shop Floor Txns", Value: null },
        { Name: 'Lbl_ShopFloorTransactions', DefaultValue: "Shop Floor Transactions", Value: null },
        { Name: 'SubmitButton', DefaultValue: "Submit", Value: null },
        { Name: 'Lbl_MoreCommandBar', DefaultValue: "More...", Value: null },
        { Name: 'Lbl_LessCommandBar', DefaultValue: "Less...", Value: null }
    ];
}

Camstar.WebPortal.WebPortlets.SideBar.prototype = {
    initialize: function () {
        Camstar.WebPortal.WebPortlets.SideBar.callBaseMethod(this, 'initialize');

        this.isResponsive = this.$bar().parent().is(".cs-responsive");

        this.$pnl = $(".cs-sidebar-panel", this.get_element());
        this.$pnl.hide();

        this._isDesktop = $(document.body).hasClass("desktop-device");
        this._useDelay = false;

        var me = this;

        $(getCEP_top()).on("resize",
            function () {
                me._hiddenButtons = [];
                me._layoutSetup();
            });

        // Panel close ( for desktop - top right cross, for responsive top-left "back" )
        $('.close-button', me.$pnl).click(function () {
            me.panelToggle(false);
        });

        // add (...) button to the right of page title for responsive env. For the iPad/landscape teh bar is fixed as for the desktop
        var $pm = $('.pageTitleMobile');

        if (!$pm.is('.menu-btn')) {
            $pm.append("<span class=menu-btn></span>");
            $('.pageTitleMobile .menu-btn').click(function () {
                me.barToggle();    // show
            });

            // Add closing 
            $(".scrollable-panel").bind("mousedown", function (e) {
                if (!$(e.target).is(".menu-btn") && me.$bar().is(":visible"))
                    me.barToggle(); // hide
            });
        }
        this._layoutSetup();
        this._deferredUpdate = false;
        top_resize();
        var widthSideBar = $(document).find(".cs-command-sidebar")[0].offsetWidth;
        var $concierge = $(window.parent.document).find(".conciergeIcon");
        var conciergeMarginRight = (parseInt(widthSideBar) - ($concierge.width())) / 2;
        conciergeMarginRight = conciergeMarginRight >= 0 ? conciergeMarginRight : 22;
        $concierge.css("margin-right", conciergeMarginRight);
    },

    dispose: function () {
        Camstar.WebPortal.WebPortlets.SideBar.callBaseMethod(this, 'dispose');
    },

    barToggle: function () {
        if (this.isResponsive && !this.$bar().parent().parent().is(".cmdbar-sticky"))
            this.$bar().parent().toggle('slide', { direction: 'right' }, 500);
    },

    togglePopup: function ($item) {
        if (this.$pnl.is(":visible")) {
            this._destroyPanel();
        }

        var mItem = $item.data("menuitem");
        var cmdObj = $item.data("cmdObj");

        var cont = this._getPropValue(mItem.Action.PanelBuilder, cmdObj, mItem.Action.Parameters);

        var $pnlContent = $(".sidebar-panel-container .content", this.$pnl);
        $pnlContent.data("cmdObj", cmdObj);
        $pnlContent.data("menuitem", mItem);
        $pnlContent.html(cont || "<div>Not implemented!</div>");
    },

    panelToggle: function (par) {
        var me = this;
        var closePanel = this.$pnl.css("display") !== "none";
        var $item;
        var d = $.Deferred();

        if (typeof par != "undefined") {
            if (typeof par == "boolean" && par == false) {
                // unconditionally close
                closePanel = true;
            }
            else {
                if (typeof par == "string") {
                    // item is a class - find menu item by class
                    $item = $('.item.' + par, this.$bar());
                }
                else {
                    $item = par;        // par is jQuery object
                }
            }
        }
		
		// Adjust sidebar panel after pin clicked.
		me.$pinPanel = $(".cs-sidebar-panel");
        me.$div = $(".sidebar-pin-panel");
        if (me.$pinPanel.hasClass("cs-pinned-panel")) {
            if (closePanel) {
                me.$div.removeClass("sidebar-pinned-panel");
                me.$div.addClass("sidebar-unpinned-panel");
                $(".form-container .cs-command-sidebar").css({ 'height': 'calc((100 % - 66px) - 1px)', 'top': '67px' });
            }
            else {
                me.$div.removeClass("sidebar-unpinned-panel");
                me.$div.addClass("sidebar-pinned-panel");
                $("#ctl00_SideBarRight").css({ 'height': '100%', 'top': '0' });
            }
        }

        if (closePanel) {
            // If panel already closed -- nothing to do
            if (this.$pnl.css("display") == "none") {
                d.resolve();
                return d.promise();
            }

            this._destroyPanel();
        }

        if (!closePanel && this.$pnl.css("display") == "none") {
            if (!($item && $item.length)) {
                d.resolve();
                return d.promise();
            }
        }

        this.$pnl.addClass("toggling-progress");

        this.$pnl.toggle('slide', { direction: "right" }, 500, function () {
            // Complete
            $(this).removeClass("toggling-progress");
            if ($(this).css("display") !== "none") {
                // do when the panel is opened
                if ($item && $item.length) {
                    me._buildPanel($item);
                    // Lock main screen
                    $(this).closest(".scrollable-panel").addClass("noscroll");

                    // Hide bottom panel if presented
                    $(".bottom-panel").addClass("bottom-panel-hidden");
                }
            }
            else {
                // panel is closed
                $(this).closest(".scrollable-panel").removeClass("noscroll");
                $(".bottom-panel").removeClass("bottom-panel-hidden");
            }
            d.resolve();
        });

        return d.promise();
    },

    update: function (useDelay) {
        var me = this;
        if (useDelay)
            me._useDelay = true;
        var $b = this.$bar();
        if ($b.length) {
            if (this.$pnl.hasClass("toggling-progress")) {
                this._deferredUpdate = true;
                return;
            }

            this.panelToggle(false).done(function () {
                me._update.call(me);
                if (me._deferredUpdate) {
                    me._deferredUpdate = false;
                    me.update();
                }
            });
        }
    },

    _update: function () {
        var me = this;
        var $b = this.$bar();
        $b.unbind("click keydown").bind("click", function (e) {
            return me.cmdClick(e);
        }).bind("keydown", function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                return me.cmdClick(e);
            }
            });
        this._loadCommands();
        this.$pnl.closest(".scrollable-panel").removeClass("noscroll");

        // Trigger onUpdated by all cmds
        this.triggerEvent("onUpdated", $b.toArray());
    },

    triggerEvent: function (evName, prmArray) {
        var ret;
        // Trigger onUpdated by all cmds
        var glob = this._commandSettings.GetCommandBarGlobals();
        if (typeof glob[evName] === "function") {
            ret = glob[evName].apply(this._commandSettings, prmArray);
            if (ret === false)
                return;
        }
        return ret;
    },

    redirectClick: function ($item) {
        this.$pnl.closest(".scrollable-panel").removeClass("noscroll");

        var menuItem = $item.data("menuitem");

        var clickTo = this._getPropValue(menuItem.Action.RedirectClick, $item.data("cmdObj"));
        if (clickTo) {
            // Ignore if false
            if (this.triggerEvent("onRedirectClick", [$item[0], menuItem]) === false)
                return;

            this.panelToggle(false).done(function () {
                $(clickTo).click();
            });

        }
        else
            console.error("Redirect Click. Clickable item is not found", menuItem.Action);

    },

    cmdClick: function (ev) {
        var $item = $(ev.target);
        
        if (!$item.hasClass("item"))
            $item = $item.closest(".item");

        var menuItem = $item.data("menuitem");

        // Ignore if false
        if (this.triggerEvent("onCmdClick", [$item[0], menuItem]) === false)
            return;

        if (menuItem && menuItem.Action && this._getPropValue(menuItem.Action.Disabled, $item.data("cmdObj")) === true)
            return;

        if (menuItem && menuItem.Action && menuItem.Action.OnClick) {
            menuItem.Action.OnClick();
            return;
        }

        var closingItem = $(".item.selectedItem", this.$bar()).data("menuitem");
        var me = this;

        if ($item.hasClass("item")) {
            this.barToggle();         // hide ( for responsive)
            // Hide panel
            this.panelToggle(false).done(function () {

                // If closing item (or "More/Less" button) is clicked then just close it
                if (!menuItem || closingItem == menuItem)
                    return;

                var openMode = "redirect";
                var menuAction = menuItem.Action;
                if (!menuAction.OpenMode) {
                    if (menuAction.PanelBuilder)
                        openMode = "slideout";
                    else if (menuAction.Page)
                        openMode = "newtab";
                }
                else {
                    openMode = me._getPropValue(menuAction.OpenMode, $item.data("cmdObj"));
                }

                if (openMode == "slideout") {
                    // show with build content
                    me.panelToggle($item).done(function () {
                        $item.addClass("selectedItem");
                        setTimeout(()=>{ $item.blur()}, 0);             
                    });
                }
                else if (openMode == "popup") {
                    me.togglePopup($item);
                }
                else if (openMode == "newtab") {
                    var name = $item[0].title;
                    var queryString;
                    var dataContracts = me._getPropValue(menuAction.GetDataContracts, $item.data("cmdObj"));
                    if (dataContracts)
                        queryString = "DataContracts=" + encodeURIComponent(dataContracts);
                    __page.openInNewTab(menuAction.Page + ".aspx", queryString, name, null, null);
                }
                else /* "redirect" */ {
                    me.redirectClick($item);
                }
               
            });
        }
    },

    isLandscape: function () {
        return !this.isDesktop() && (window.orientation === 90 || window.orientation === -90);
    },

    isDesktop: function () {
        return this._isDesktop;
    },

    isTablet: function () {
        return Math.max(screen.availHeight, screen.availWidth) >= 950;
    },

    _layoutSetup: function () {
        var isSticky = (this.isTablet() && this.isLandscape()) || this.isDesktop();
        $(this.get_element()).parent().toggleClass("cmdbar-sticky", isSticky);

        if (!isSticky && !this.$pnl.parent().is(".scrollable-panel")) {
            // move panel to scrollable panel 
            this.$pnl.detach().appendTo($(".scrollable-panel"));
            this.$pnl = $(".scrollable-panel .cs-sidebar-panel");
            this.$bar().parent().css("display", "none");
        }
        else if (isSticky && !this.$pnl.parent().is(".cs-command-sidebar")) {
            // When switching from portrait to landscape and the slideout panel is displayed 
            //   the noscroll should be off and then restored to position the cmd-bar correctly
            var noScrollActive = this.$pnl.closest(".scrollable-panel").is(".noscroll");
            if (noScrollActive)
                this.$pnl.closest(".scrollable-panel").removeClass("noscroll");

            // move panel out of scrollable panel
            this.$pnl.detach().appendTo($(".cs-command-sidebar"));
            this.$pnl = $(".cs-command-sidebar .cs-sidebar-panel");
            this.$bar().parent().css("display", "block");
            if (noScrollActive)
                this.$pnl.closest(".scrollable-panel").addClass("noscroll");
        }
        else if (!isSticky) {
            this.$bar().parent().css("display", "none");
        }

        // toggle class in iframes
        if (window.frames) {
            for (var i = 0; i < window.frames.length; i++) {
                var fr = window.frames[i];
                if (fr.frameElement.src && fr.frameElement.src.indexOf("&IsChild=true") != -1) {
                    $(".form-container, .float-form-container", fr.document).toggleClass("cmdbar-sticky", isSticky);
                }
            }
        }

        if (!this.isDesktop())
            $(document.body).toggleClass("wide", this.isTablet());

        this.headerAdjustment();

        if (this.isDesktop())
            this._replaceSideBarBtns();

    },

    _appendMenuItem: function ($b, menuItem, cmdObj, order) {
        var $itm = menuItem.Id ? $("<div id='" + menuItem.Id + "' class='item'></div>") : $("<div class='item'></div>");
        var nameText = this._getPropValue(menuItem.Name.Text, cmdObj);
        $itm.prop("title", nameText);
        $itm.prop("tabindex", "-1");
        if (menuItem.Icon) {
            var iconSrc = this._getPropValue(menuItem.Icon.Src, cmdObj);
            if (iconSrc) {
                $itm.append("<img class='item-icon' />");
                $("img.item-icon", $itm).prop("src", "Themes/" + this.cssIconTheme + "/images/" + iconSrc);
            }
            else {
                // Use class and span as a button
                var iconClass = this._getPropValue(menuItem.Icon.CSS, cmdObj);
                if (iconClass) {
                    $itm.append("<span class=item-icon></span>");
                    $(".item-icon", $itm).addClass(iconClass);
                }

                // just text
                var iconTxt = this._getPropValue(menuItem.Icon.Text, cmdObj);
                if (iconTxt) {
                    $itm.append("<div class=button-text></div>");
                    $('.button-text', $itm).text(iconTxt);
                }
            }

            // Add text below the icon
            $itm.append("<div class=caption-text></div>");
            $(".caption-text", $itm).text(nameText);
        }
        else {
            // just text
            $itm.append("<div class=button-text></div>");
            $('.button-text', $itm).text(nameText);
        }

        var menuAction = menuItem.Action;
        if (!menuAction.OpenMode) {
            if (menuAction.PanelBuilder) {
                $itm.addClass('add-separator');
            }
        }

        $itm.toggleClass("hidden", menuItem.Action && this._getPropValue(menuItem.Action.Visible, cmdObj) === false);
        $itm.prop("disabled", menuItem.Action && this._getPropValue(menuItem.Action.Disabled, cmdObj) === true);


        $itm.data("cmdObj", cmdObj)
            .data("menuitem", menuItem);

        if (menuItem.CSS) {
            var cssBar = this._getPropValue(menuItem.CSS.Bar, cmdObj);
            if (cssBar)
                $itm.addClass(cssBar.join(' '));
        }

        if (menuItem.Badge) {
            if (menuItem.Badge.Count > 0 || menuItem.Badge.ShowEmpty) {
                $itm.prepend(this._getBadgeHtml(menuItem.Badge, menuItem.Id));
            } else {
                $itm.addClass('cr-cmdbar-button-badge');
            }
        }

        if (menuItem.AdditionalInfo) {
            this._updateAdditionaInfoElement($itm, menuItem);
        }

        if (order !== undefined || menuItem.Order > 0)
            $itm.attr("order", order ? order : menuItem.Order);

        // avoid duplicate action buttons.
        // only add if title and class of current item does not match the values of an item already in the bar.
        var bButtons = $b.children(".action-redirect-button,.transaction-action").toArray();
        var hashButtons = bButtons.map(function (el) {
            return {
                title: el.title, class: el.className.replace(' add-separator', '')
            };
        });

        if (hashButtons.length != 0) {
            var withoutSeparatorBth = $itm[0].className.replace(' add-separator', '');
            if (hashButtons.filter(function (v) { return v.title == nameText && v.class == withoutSeparatorBth }).length == 0) {
                $b.append($itm);
            }
        }
        else
            $b.append($itm);

        //  Do this here to update any buttons maintained in hidden list that have been re-created.
		if (typeof this._hiddenButtons !== 'undefined' && this._hiddenButtons.length && $itm[0].id) {
			for (var i = 0; i < this._hiddenButtons.length; i++) {
				if (this._hiddenButtons[i].id === $itm[0].id) {
                    this._hiddenButtons[i] = $itm[0];
                    break;
				}
			}
		}
    },

    /**
     * Gets the html to insert into the DOM for showing a badge on a button.
     * 
     * @param {object} badge - The Badge property object from menuItem data.
     */
    _getBadgeHtml: function (badge, id) {
        var html = '';

        if (badge.Count > 0 || badge.ShowEmpty) {
            //TODO: "operational" badge size standard for shopfloor but styles exist for "standard". determine if we need to support that and what to check as a flag.
            let badgeSize = 'operational';
            let digitCount = badge.Count > 0 ? badge.Count.toString().length : 0;
            let classAttribute = 'cr-badge cr-notification-icon-' + badgeSize + ' cr-notification-' + badge.Theme + '-' + badgeSize + '-' + digitCount;
            let iconText = badge.Count > 0 ? badge.Count.toString() : '';
            html = '<div class="' + classAttribute + '" id="' + id + '-badge">' + iconText + '</div>';
        }

        return html;
    },

    /**
     * Updates the specified menu item to show or hide a separator bar beneath it.
     * 
     * @param {string} id - Unique Id of the menu item to update.
     * @param {boolean} show - If true, then add a separator bar, otherwise remove it.
     */
    _updateSeparator: function (id, show) {
        var $item = this.$bar().find('div#' + id);
        var menuItem = $item.data('menuitem');

        if (menuItem) {
            menuItem.updateSeparator(show);
            $item.toggleClass('add-separator', show);
        } else {
            console.warn('SideBar._updateSeparator: could not find item with Id: ' + id);
        }
    },

    /**
     * Updates values in the Badge property of menuItem data and refreshes the badge shown on the button.
     * 
     * @param {string} id - Unique Id for the menu item.
     * @param {number} count - Integer value for number of notifications to show in the badge
     * @param {string} [theme] - Controls badge color. Allowed options [blue, red, light]
     * @param {boolean} [showEmpty] - Set true to show the badge with no number when count is zero.
     */
    _updateBadge: function (id, count, theme, showEmpty) {
        var $item = this.$bar().find('div#' + id);

        // update data for badge in atached menu item
        var menuItem = $item.data('menuitem');
        if (menuItem && menuItem.Badge) {
            menuItem.Badge.Count = count;
            if (theme)
                menuItem.Badge.Theme = theme;
            if (showEmpty !== undefined && showEmpty !== null)
                menuItem.Badge.ShowEmpty = showEmpty;

            // remove any existing badge <div> and class with margin
            $item.removeClass('cr-cmdbar-button-badge');
            $item.find('div.cr-badge').remove();

            // then add new badge <div> if needed.
            let html = this._getBadgeHtml(menuItem.Badge, id);
            if (html) {
                $item.prepend(html);
            } else {
                $item.addClass('cr-cmdbar-button-badge');
            }
        } else {
            console.warn('_updateBadge: menuItem with id "' + id + '" not found or does not have Badge set.')
        }
    },

    /**
     * Makes the badge not visible on a button.
     * 
     * @param {string} id - Unique Id for the menu item.
     */
    _removeBadge: function (id) {
        this._updateBadge(id, 0, null, false);
    },

    /**
     * Update valued displayed in Additional Info element.
     * 
     * @param {object} $item - jQuery object wrapping the menu button
     * @param {object} menuItem - Data object attached the button element
     */
    _updateAdditionaInfoElement: function ($item, menuItem) {
        let additionalSection = $item.find('div.cr-sidebar-additional-information-section');//.remove();
        if (menuItem.AdditionalInfo.Visible) {
            let text = menuItem.AdditionalInfo.Text ? menuItem.AdditionalInfo.Text : "&nbsp;";
            if (additionalSection.length === 0)
                $item.append("<div class='cr-sidebar-additional-information-section'>" + text + "</div>");
            else
                additionalSection.html(text);
        } else {
            additionalSection.remove();
        }
    },

    /**
     * Update Additional Information section of a menu items data object and refresh display of the element.
     * 
     * @param {string} id - Unique Id for menu item.
     * @param {string} [text=''] - Text to display
     */
    _updateAdditionalInfo: function (id, text) {
        var $item = this.$bar().find('div#' + id);

        var menuItem = $item.data('menuitem');
        if (menuItem && menuItem.AdditionalInfo) {
            menuItem.AdditionalInfo.Text = text;

            this._updateAdditionaInfoElement($item, menuItem);
        } else {
            console.warn('_updateAdditionalInfo: menuItem with id "' + id + '" not found or does not have AdditionalInfo set.')
        }
    },

    /**
     * Updates a command button image by updating the CSS class.
     * 
     * @param {string} id - Unique Id for menu item.
     * @param {string} imageClass - CSS class that renders button image.
     */
    _updateCommandImage: function (id, imageClass) {
        var $item = this.$bar().find('div#' + id);
        var menuItem = $item.data('menuitem');

        if (menuItem && menuItem.Icon) {
            if (menuItem.Icon.CSS !== imageClass) {
                $item.find('span.item-icon')
                    .removeClass(menuItem.Icon.CSS)
                    .addClass(imageClass);

                menuItem.Icon.CSS = imageClass;
            }
        } else {
            console.warn('_updateCommandImage: menuItem with id "' + id + '" not found or does not have Icon set.')
        }
    },

    _loadCommands: function () {
        var $b = this.$bar();
        var me = this;
        var labelsToLoad = [];
        var cmds = [];
         
        var loadingFinalize = function (_this, $$b, labels) {
            if (labels) {
                _this._populateLabels(labels);
            }
            if (labels && _this._commandSettings.PopulateLabels)
                _this._commandSettings.PopulateLabels(labels);

            _this._commandSettings.GetCommandBarItems().forEach(function (cmdItem, i) {
                let order = cmdItem.Order > 0 ? cmdItem.Order : i * 5 + 10000;
                me._appendMenuItem($b, cmdItem, _this._commandSettings, order);
                //me._appendMenuItem($b, cmdItem, _this._commandSettings, i*5 + 10000);
            });

            setTimeout(function () {
                _this._hideMobileMenu();
                _this._buildTransactions();
                _this._sortCmdItems($$b);
                _this._layoutSetup();
            },  me._useDelay ? 250 : 1);    //  Need a longer delay here to make sure that any transaction page has fully loaded
            _this.triggerEvent("onCommandsLoaded", [$$b.children("div.item").toArray()]);
        };
        $b.children(".item").remove();

        // find the menu settings in the page
        if ($(document.body).attr("sideMenu")) {
            var pageCmd = $(document.body).data();
            if (pageCmd && pageCmd.commandBarSettings) {
                cmds.push(pageCmd.commandBarSettings);
            }
            else {
                var cmdClass = window[$(document.body).attr("sideMenu")];
                $(document.body).data({ commandBarSettings: new cmdClass() });
                cmds.push($(document.body).data().commandBarSettings);
            }
        }

        if (cmds && cmds.length) {
            // Take first -> assumed just one (for now)
            this._commandSettings = cmds[0];
            this._commandSettings.initialize(me);
            if (this._commandSettings.CollectLabels)
                this._commandSettings.CollectLabels(labelsToLoad);
        }

        this._collectLabels(labelsToLoad, this._txnLabels);

        if (labelsToLoad.length) {
            getCEP_top().__page.getLabels(labelsToLoad, function (labels) {
                if (labels && labels.Error) {
                    console.error(labels.Error);
                }
                loadingFinalize(me, $b, labels);
            });
        }
        else {
            loadingFinalize(me, $b, null);
        }
    },

    _hideMobileMenu: function () {
        var showMobileMenu = true;
        var isContainerSearch = $(document.body).hasClass("common-search-r2");
        var glob = this._commandSettings.GetCommandBarGlobals();
        if (glob)
            showMobileMenu = this._getPropValue(glob.showMobileMenu, this._commandSettings);

        if (showMobileMenu == false && isContainerSearch == false) {
            $('.pageTitleMobile .menu-btn').addClass('hideMobileMenu');
        }
    },

    _getClasses: function ($btn, itemClasses, iconClasses) {

        for (var i = 0; i < $btn[0].classList.length; i++)
            itemClasses.push("cmdbar-" + $btn[0].classList.item(i));

        if ($btn.attr("itemCSS"))
            $btn.attr("itemCSS").split(' ').forEach(function (c) { itemClasses.push(c); });

        if ($btn.attr("imageCSS"))
            $btn.attr("imageCSS").split(' ').forEach(function (c) { iconClasses.push(c); });

        if ($btn.attr("isPrimary") === "true")
            itemClasses.push("cmd-primary");

        if ($btn.css("display") === "none") {
            itemClasses.push("origin-invisible");
        }
        else {
            // Check whether the parent webpart is display:none (in inline style) 
            var $wp = $btn.closest(".webpart");
            if ($wp.length) {
                if ($wp[0].style.display === "none")
                    itemClasses.push("origin-invisible");
                if ($wp.is(".webpart-actions"))
                    itemClasses.push("transaction-button");
            }
        }
        if (iconClasses.length == 0)
            iconClasses.push("action-icon");
    },

    makeCommandBarUnique: function (commandBar) {
        // Remove duplicate children ...
        commandBar.children().each(function (firstIndex, firstValue) {
            commandBar.children().each(function (secondIndex, secondValue) {
                if (firstIndex < secondIndex) {
                    if (firstValue.className == secondValue.className && firstValue.title == secondValue.title && firstValue.className == 'item') {
                        commandBar[0].removeChild(secondValue);
                    }
                }
            });
        });
        return commandBar;

    },

    _removeItemAll: function (arr, value) {
        var i = 0;
        while (i < arr.length) {
            if (arr[i] === value) {
                arr.splice(i, 1);
            } else {
                ++i;
            }
        }
        return arr;
    },

    _buildTransactions: function () {
        var me = this;
        // IR 10361483.  This removes duplicate page action buttons from the action panel ...
        //var $b = this.$bar();
        var $b = me.makeCommandBarUnique(this.$bar());

        // clear all actions
        //$(".action-redirect-button, .transaction-action", $b).remove();   //TODO: verify if update needed and undo change if not.
        $(".action-redirect-button, .transaction-action", $b).not('.cmdbar-custom-button').remove();

        var $wpButtonBar = $(".bottom-panel-container #WebPart_ButtonsBar");
        $wpButtonBar.children("button").remove();

        // Create duplicates of action panel buttons
        $("#WebPart_ActionsControl.webpart-actions .action input").each(function () {
            var $rbtn = $("<button />")
                .attr({
                    "ref-id": this.id,
                    "displayMode": $(this).attr("displayMode"),
                    "order": $(this).attr("order"),
                    "group": $(this).attr("group")
                })
                .addClass(this.className)
                .text(this.value)
                .prop("title", this.title);

            $wpButtonBar.append($rbtn);
            $("[ref-id=" + this.id + "]", $wpButtonBar).unbind("click").click(function () {
                var $origButton = $("#" + $(this).attr("ref-id"));
                if ($origButton.length)
                    $origButton.click();
                else
                    console.warn("Original action button not found ", $(this).attr("ref-id"));
            });
        });

        $("#WebPart_ActionsControl").addClass("hidden");

        // Collect all action over the page
        var $actions = $(this.actionsSelector);

        if (window.frames) {
            for (var i = 0; i < window.frames.length; i++) {
                try {
                    if (window.frames[i].frameElement.src) {
                        $.merge($actions, $(".page-panel #WebPart_ButtonsBar input", window.frames[i].document));
                        // IR 10361483 (Fixes missing Submit/Reset page action buttons on the action panel)
                        $.merge($actions, $(".pagepanel-eproc #WebPart_ButtonsBar input", window.frames[i].document));
                    }
                } catch { }
            }
        }

        var showTransactionSlideout = false;
        var transSlideoutMenu = [];
        var groups = [];

        // get count of visible actions to use in check for submit/reset overrides
        var $visibleActions = $actions.filter(function () {
            var $item = $(this);
            return $item.attr('displayMode') !== 'Hidden' && $item.css("display") !== "none";
        });

        $actions.each(function () {
            var $btn = $(this);

            if ($btn.attr("displayMode") === "Hidden" ||
                me._commandSettings.hasActionOverride($btn, $visibleActions.length))
                return true;

            if ($btn.is("button[ref-id]"))
                $btn = $("#" + $btn.attr("ref-id"));

            var txt = $btn.prop("title") || $btn.val();

            var cssClasses = ["action-redirect-button"];
            var iconClasses = [];
            me._getClasses($btn, cssClasses, iconClasses); // classes as string[]

            var actionMenuItem = {
                Name: { Text: txt },
                Action: {
                    RedirectClick: $btn[0],
                    Caption: txt,
                    Disabled: $btn[0].disabled
                },
                Icon: { CSS: iconClasses },
                CSS: { Transaction: cssClasses, Bar: cssClasses },
                DisplayMode: $btn.attr("displayMode") || "Bar",
                IsPrimary: $btn.attr("isPrimary") === "true",
                CustomMethod: $btn.attr("customMethod"),
                Order: parseInt($btn.attr("actionOrder") || "0"),
                Index: parseInt($btn.attr("actionIndex") || "0"),
                Group: $btn.attr("actionGroup")
            };

            if (actionMenuItem.Group) {
                if (groups.indexOf(actionMenuItem.Group) == -1)
                    groups.push(actionMenuItem.Group);
            }

            if (actionMenuItem.DisplayMode == "Bar") {
                // Avoid duplications !
                me._appendMenuItem($b, actionMenuItem);
            }
            else if (actionMenuItem.DisplayMode == "TransPanel") {
                if (actionMenuItem.CSS && actionMenuItem.CSS.Transaction && actionMenuItem.CSS.Transaction.indexOf("origin-invisible") == -1) {
                    showTransactionSlideout = true;
                    transSlideoutMenu.push(actionMenuItem);
                }
            }

            // Mark web part
            $btn.closest(".webpart").addClass("cmd-buttons");
        });

        // The class available-transactions-buttons can be assigned to the ActionsControl. 
        $(".available-transactions-buttons").addClass("cmd-buttons");

        // Sort transactions for slide-out panel
        transSlideoutMenu = transSlideoutMenu.sort(function (a, b) {
            return (a.Order === 0 ? 999999 : a.Order) - (b.Order === 0 ? 999999 : b.Order);
        });

        // Actions are in slide-out panel           
        this._appendMenuItem($b, {
            Name: {
                Text: function () {
                    return me.getLabel("Lbl_ShopFloorTxns");
                }
            },
            Action: {
                PanelBuilder: function () { return me._transactionPanel(); },
                PanelTitle: function () { return me.getLabel('Lbl_ShopFloorTransactions'); },
            },
            Icon: { CSS: 'transaction-panel' },
            CSS: { Bar: ["transaction-action"] },
            IsPrimary: false,
            Order: 9999,
            Index: 0,
            TransactionMenu: transSlideoutMenu
        });

        $b.toggleClass("show-transaction-panel", showTransactionSlideout);
        $(".transaction-action", $b).toggleClass("transaction-button", $('.transaction-button', $b).length != 0);

        var transactionButtons = $b.children(".action-redirect-button,.transaction-action");
        var primaryCount = 0;
        var secondaryCount = 0;
        var groupIndex = [];

        if (groups.length > 0) {
            // sort groups
            groups = groups.sort();
            groupIndex = groups.map(function (g) { return { name: g, lastIndex: -1, btn: undefined } });
        }

        // ordering
        transactionButtons.each(function (i) {
            var order = 0;
            var $a = $(this);
            var menu = $a.data("menuitem");

            if (menu.Order !== undefined && menu.Order > 0) {
                order = menu.Order;
            }
            else {
                if (menu.IsPrimary === true) {
                    if (menu.Index !== undefined && menu.Index > 0)
                        order = 20 + menu.Index;
                    else
                        order = 20 + primaryCount;
                    primaryCount += 5;
                }
                else {
                    if (menu.Index !== undefined && menu.Index > 0)
                        order = menu.Index;
                    else
                        order = secondaryCount;
                    if (!menu.Group)
                        order += 100;

                    secondaryCount += 5;
                }
            }

            if (menu.Group) {
                order = order + 200 + groups.indexOf(menu.Group) * 100;
                $a.attr("group", menu.Group);
                var grp = groupIndex.filter(function (g) { return g.name == menu.Group; });
                if (grp.length) {
                    var gr = grp[0];
                    if (order > gr.lastIndex) {
                        gr.lastIndex = order;
                        gr.btn = this;
                    }
                }
            }
            $a.attr("order", order);
            // next line could be used for debugging ordering
            //      $a.prop("title", order.toString() + " : " + (menu.Group || "-"));
        });

        // Add separator at the end of group
        groupIndex.forEach(function (g) {
            if (g.btn)
                $(g.btn).addClass("add-separator");
        });

        this.triggerEvent("onTransactionsLoaded", [transactionButtons]);
        this._addMoreBlock();
    },

    _addMoreBlock: function () {
        var $b = this.$bar();
        var me = this;

        //  Because we may have already built the 'more' panel with the buttons, we only want to re-create the hidden-buttons-wrapper DIV if it does not exist
        let hid = $(".hidden-buttons-wrapper", $b);
        if (typeof this._hiddenButtons === 'undefined' || !this._hiddenButtons.length || hid.length === 0) {
            $(".hidden-buttons-wrapper", $b).remove();
            $b.append("<div class='item hidden-buttons-wrapper hidden' order='100000'></div>");
            $(".hidden-buttons-wrapper", $b).append(
                "<div class='hidden-buttons hidden'></div>" +
                "<div class='more-button more item'>" + this.getLabel("Lbl_MoreCommandBar") + "</div>"
            );
        }

        $(".more-button", $b).unbind("click").bind("click", function (ev) {
            var $btn = $(ev.currentTarget);
            var isCurrentlyMore = $btn.hasClass("more");

            $btn.toggleClass("more")
                .toggleClass("less")
                .siblings(".hidden-buttons")
                .eq(0)
                .toggleClass("hidden");

            if (isCurrentlyMore) {
                $btn.text(me.getLabel("Lbl_LessCommandBar"));
            } else {
                $btn.text(me.getLabel("Lbl_MoreCommandBar"));
            }

            if ($(".hidden-buttons", $b).children(".item").length != 0)
                me._updatePositionMoreBtn($b);
        });
    },

    _sortCmdItems: function ($b) {

        var $elements = $b.children(".item");

        $elements.sort(function (a, b) {
            if (parseInt($(a).attr("order")) < parseInt($(b).attr("order")))
                return -1;
            else if (parseInt($(a).attr("order")) > parseInt($(b).attr("order")))
                return 1;
            else
                return 0;
        }).appendTo($b);

        // Add separator to the last transaction btn
        $b.children(".item.action-redirect-button:visible:not([group])").last().addClass("add-separator");

        //fix not normal behaviour of IE
        if (Camstars.Browser.IE) {
            $elements.each(function (index, value) {
                $(value).bind("mousedown",
                    function (ev) {
                        $(ev.currentTarget).addClass("active-mode");
                    });
                $(value).bind("mouseup",
                    function (ev) {
                        $(ev.currentTarget).removeClass("active-mode");
                    });
                $(value).bind("mouseleave",
                    function (ev) {
                        $(ev.currentTarget).removeClass("active-mode");
                    });
            });
        }

    },

    _getThresholdCount: function () {
        var ret = 2;
        var glob = this._commandSettings.GetCommandBarGlobals();
        if (glob)
            ret = this._getPropValue(glob.transactionThreshold, this._commandSettings);
        return ret;
    },

    _buildPanel: function ($item) {
        var $pnlContent = $(".sidebar-panel-container .content", this.$pnl);
        var $$pnl = this.$pnl;
        var cont;
        var mItem = $item.data("menuitem");
        var cmdObj = $item.data("cmdObj");
        if (mItem) {
            if (mItem.Action.PanelBuilder) {
                cont = this._getPropValue(mItem.Action.PanelBuilder, cmdObj, mItem.Action.Parameters);
                $pnlContent.data("cmdObj", cmdObj);
                $pnlContent.data("menuitem", mItem);
            }
            if (mItem.CSS && mItem.CSS.Panel)
                $$pnl.addClass(mItem.CSS.Panel.join(' '));
        }

        var caption = this._getPropValue(mItem.Action.PanelTitle, cmdObj) || $item.prop('title');
        $('.header .title', $pnlContent.parent()).text(caption);
        $pnlContent.html(cont || "<div></div>");
    },

    _destroyPanel: function () {
        var $pnlContent = $(".sidebar-panel-container .content", this.$pnl);
        var $$pnl = this.$pnl;
        var mItem = $pnlContent.data("menuitem");
        if (mItem) {
            this._getPropValue(mItem.Action.DestroyPanel, $pnlContent.data("cmdObj"));
            if (mItem.CSS && mItem.CSS.Panel)
                $$pnl.removeClass(mItem.CSS.Panel.join(' '));
        }

        $('.header .title', $pnlContent.parent()).text("");
        $(".content", this.$pnl).children().remove();
        $('.item.selectedItem', this.$bar()).removeClass("selectedItem");
    },

    _renderPanel: function (list$) {
        var $pnlCont = $(".sidebar-panel-container .content", this.$pnl);
        $pnlCont.empty();
        Array.forEach(list$, function (el) { $pnlCont.append(el) });
    },

    _transactionPanel: function () {
        var me = this;
        var div_list = [];
        var actions = $(".transaction-action", this.$bar()).data("menuitem").TransactionMenu;

        actions.forEach(function (menuItem) {
            if (menuItem.CSS && menuItem.CSS.Transaction && menuItem.CSS.Transaction.indexOf("origin-invisible") != -1)
                return true;

            var targetElement = menuItem.Action.RedirectClick;
            var isDisabled = targetElement.disabled;
            if (!isDisabled && targetElement.classList.contains("aspNetDisabled")) {
                isDisabled = true;
            }

            var ac = menuItem.Action;
            var $d = $("<div class='content-row content-btn-row'><input type='submit' class=action-button /></div>");
            var $actBtn = $('.action-button', $d);
            $actBtn.prop("value", ac.Caption).prop("title", ac.Caption)
                .data("cmdObj", me)
                .data("menuitem", menuItem)
                .addClass(menuItem.CSS && menuItem.CSS.Transaction && menuItem.CSS.Transaction.join(' '));
            $d.attr("order", menuItem.Order);
            if (menuItem.Group)
                $d.attr("group", menuItem.Group);

            if (!isDisabled)
                $actBtn.click(function () {
                    me.redirectClick($(this));
                    return false;
                });
            else {
                $actBtn.prop("disabled", "disabled");
            }
            div_list.push($d);
        });
        return div_list;
    },

    setCallbackData: function (jsondata, c) {

        var objData = JSON.parse(jsondata);
        var sp = $find(c.id);
        if (objData.__fun == "openDocument") {
            if (objData.Error) {
                alert(objData.Error);
            }
            else {
                var di = objData.DocInfo;
                sp.openDocument(di.DocumentRef, di.BrowseMode, di.URI, di.FileName, di.IsRemote, di.AuthenticationType);
            }
        }
        else {
            var $pnlContent = $(".sidebar-panel-container .content", this.$pnl);
            var cmdObj = $pnlContent.data("cmdObj");
            if (cmdObj) {
                sp._renderPanel(cmdObj.setCallbackData(objData, c));
                sp.triggerEvent("onPanelRendered", $pnlContent.toArray());
            }
        }
    },

    openDocument: function (docRef, browseMode, uri, fileName, isRemote, authType) {
        var me = this;
        this.panelToggle(false).done(function () {
            if (browseMode == "Url") {
                getCEP_top().pop.showAjax(uri, docRef.Name,
                    me.isResponsive ? screen.availHeight - 20 : 700,
                    me.isResponsive ? screen.availWidth - 80 : 1045,
                    0, 0, true, null, null,
                    me, true, { isExternal: true, scrolling: 'yes', zIndex: 99999 }, null, true, false);
            }
            else {
                if (/\.jt$/.test(fileName)) {
                    var url = location.href.substr(0, location.href.lastIndexOf("/")) +
                        "/ModelViewerPopup.aspx?IsFloatingFrame=2&name=" +
                        docRef.Name + "&revision=" + docRef.Revision;
                    me._openPopup(url, "3D Model Popup", false);
                }
                else if (isRemote) {
                    OpenDocumentUrl(uri, authType, '');
                }
                else
                    window.open("DownloadFile.aspx?viewdocfile=" + fileName + "&origfilename=" + uri);
            }
        });
    },

    _processing: function () {
        return [$("<div>processing...</div>")];
    },

    _openPopup: function (url, title, closeButtonOnly) {
        pop.showAjax(url, title,
            this.isResponsive ? screen.availHeight : 900/*height*/,
            this.isResponsive ? screen.availWidth : 1300 /*width*/,
            0/*top*/, 0/*left*/, true /*showButtons*/,
            "" /*okButtonText*/, ""/*closeButtonText*/,
            this /*element*/, true /*closeOnCancel*/,
            ''/*optionArgs*/, null /*cancelConfirmMsg*/, closeButtonOnly, false /*display reset*/);
    },

    $bar: function () {
        return $(".cs-sidebar", this.get_element());
    },

    _getPropValue: function (prop, cmdObj, optionalParameters) {
        var v;
        if (typeof prop == "function")
            v = prop.apply(cmdObj, optionalParameters);
        else
            v = prop;

        return v;
    },

    _isSticky: function () {
        return $(this.get_element()).parent().is(".cmdbar-sticky");
    },

    headerAdjustment: function (forceRestoring) {
        var $headerStatus;
        if (typeof forceRestoring == "undefined")
            forceRestoring = false;

        if (!forceRestoring && (this.isDesktop() || (this.isTablet() && this.isLandscape()))) {
            if (!$(document.body).hasClass("header-adjusted")) {
                // correct Container status / Resource header for desktop mode
                $headerStatus = $(".webpart-containerstatus-m").parent();
                if (!$headerStatus.length)
                    $headerStatus = $(".webpart.ui-webpart-resource-status").parent();
                if ($headerStatus.length) {
                    this._headerStatusParentId = $headerStatus.parent().prop("id");
                    $("#TemplateContentDiv").before($headerStatus.detach());
                    $(document.body).addClass("header-adjusted");
                }
            }
        }
        else {
            // Move back container/resource status webpart
            if ($(document.body).hasClass("header-adjusted")) {
                // correct Container status for desktop mode
                $headerStatus = $(".webpart-containerstatus-m").parent();
                if (!$headerStatus.length)
                    $headerStatus = $(".webpart.ui-webpart-resource-status").parent();

                if ($headerStatus.length) {
                    $("#" + this._headerStatusParentId).append($headerStatus.detach());
                    $(document.body).removeClass("header-adjusted");
                    delete this._headerStatusParentId;
                }
            }
        }
    },

    _replaceSideBarBtns: function () {

        var $b = this.$bar();
        var moreBtnHeight = 48;
        var $hiddenBarPanel = $b.find(".hidden-buttons");

        $(".hidden-buttons-wrapper", $b).before($hiddenBarPanel.children(".item"));

        this._sortCmdItems($b);

        var btns = $(".item", $b);

        if (!btns.length)
            return;

        var barHeight = $b.outerHeight();
        var availableHeight = barHeight - moreBtnHeight;

        var listOverceeded = [];
        var isBtnHidden = function ($btn) {
            if ($btn.hasClass("hidden") ||
                $btn.hasClass("hidden-buttons-wrapper") ||
                $btn.hasClass("more-button") ||
                $btn.hasClass("origin-invisible") ||
                (($btn.hasClass("transaction-button") || $btn.hasClass("transaction-action")) && $btn.is(":hidden")))
                return true;
            else
                return false;
        };

        var totalBtnHeight = 0;

        btns.each(function () {

            if (isBtnHidden($(this)))
                return true;

            totalBtnHeight += $(this).outerHeight(true);

            if (totalBtnHeight > availableHeight) {
                listOverceeded.push(this);
            }
        });
        if (totalBtnHeight === 0)   // clear any hiddenButtons when total buttons displayed is 0
			this._hiddenButtons = [];
		else if (totalBtnHeight < barHeight && listOverceeded.length > 0)	// if actual buttons to show height is less than bar height, no need for More button
			listOverceeded = [];
        var showMore = totalBtnHeight > availableHeight;
        if (listOverceeded.length > 0) {
            for (var i = 0; i < listOverceeded.length; i++) {
                let isFound = this._hiddenButtons.some(element => {
                    if (element.id === listOverceeded[i].id) {
                        return true;
                    }
                    return false;
                });
                if (!isFound)
                    this._hiddenButtons.push(listOverceeded[i]);
            }
        } else if (this._hiddenButtons.length > 0) {
            this._hiddenButtons.forEach(function (it) {
				let isFound = listOverceeded.some(element => {
                    if (element.id === it.id) {
                        return true;
                    }
                    return false;
                });
                if (!isFound)
					listOverceeded.push(it);
            });
        }

        if (showMore || listOverceeded.length) {
            listOverceeded.forEach(function (it) {
                $hiddenBarPanel.append(it);
            });
        } 

        this._changeStateMoreBtn($b, this, $hiddenBarPanel, moreBtnHeight);
    },

    _updatePositionMoreBtn: function ($b) {
        var heightHidBtns = 0;
        var $moreBtn = $(".more-button", $b);
        var $hiddenBtns = $(".hidden-buttons", $b);

        if ($moreBtn.hasClass("less")) {
            var allVisibleBtns = this._findVisibleBtns($b);
            var heightAllowdBtns = 0;
            var hiddenBtnsOffsetTop;

            allVisibleBtns.each(function () {
                heightAllowdBtns += $(this).outerHeight(true);
            });

            $hiddenBtns.children(".item:visible").each(function () {
                heightHidBtns += $(this).outerHeight();
            });

            var $containerStatus = $("#WebPart_ContainerStatusWP_R2");
            var hasContainerStatus = $containerStatus.length > 0;
            var containerHeight = hasContainerStatus ? $containerStatus.outerHeight() : 0;

            $hiddenBtns.addClass("hidden-block-border");

            if (hasContainerStatus) {
                $hiddenBtns.css({
                    'top': containerHeight + 'px',
                    'bottom': '0px'
                });
            } else {
                $hiddenBtns.css({
                    'top': '0px',
                    'bottom': '0px'
                });
            }

            if (heightAllowdBtns >= heightHidBtns) {
                hiddenBtnsOffsetTop = -heightHidBtns;
                $moreBtn.css("background-position", "8px center");
                $hiddenBtns.removeClass("scrollable-block");
            }
            else {
                hiddenBtnsOffsetTop = -($b.outerHeight() - $moreBtn.outerHeight());
                $hiddenBtns.addClass("scrollable-block");
            }

            setTimeout(function () {
                $hiddenBtns.addClass("show");
            }, 50);

            $moreBtn.attr("title", this.getLabel("Lbl_LessCommandBar"))
                .text(this.getLabel("Lbl_LessCommandBar"));
        }
        else {
            $hiddenBtns.removeClass("show");
            $hiddenBtns.removeClass("hidden-block-border scrollable-block");

            $(".hidden-buttons-wrapper", $b).removeAttr("style");
            $moreBtn.attr("title", this.getLabel("Lbl_MoreCommandBar"))
                .text(this.getLabel("Lbl_MoreCommandBar"))
                .css({
                    "margin-left": "",
                    "background-position": "8px center"
                });
        }
    },

    _changeStateMoreBtn: function ($b, me, $hiddenBarPanel, moreBtnHeight) {
        var $moreButton = $(".more-button", $b);
        var hiddenBtnsFlag = $hiddenBarPanel.children().length == 0 ? false : true;

        if (!hiddenBtnsFlag) {
            $moreButton.removeClass("less").addClass("more");
            $moreButton.prev(".hidden-buttons").addClass("hidden");
            $moreButton.parent().addClass("hidden");
        }
        else {
            $moreButton.css({ "height": moreBtnHeight });
            $moreButton.parent().removeClass("hidden");
            me._updatePositionMoreBtn($b);
        }
    },

    _findVisibleBtns: function ($b) {
        var allSideBarBtns = $b.children(".item").not(".hidden-buttons-wrapper");

        return allSideBarBtns.filter(function () {
            return !($(this).hasClass("hidden") ||
                $(this).hasClass("action-redirect-button") ||
                $(this).hasClass("origin-invisible")) ||
                !($(this).hasClass("transaction-button") && $(this).is(":hidden"));
        });

    },

    _collectLabels: function (labels, lbls) {
        if (lbls) {
            Object.keys(lbls).forEach(function (k) {
                if (!lbls[k].Value)
                    labels.push(lbls[k]);
            });
        }
    },

    _populateLabels: function (labels) {
        var lbls = this._txnLabels;

        Object.keys(lbls).forEach(function (k) {
            if (labels.Error) {
                if (lbls[k].DefaultValue)
                    lbls[k].Value = lbls[k].DefaultValue;
            }
            else {
                var lblName = lbls[k].Name;
                var lbl = labels.filter(function (l) { return l.Name == lblName; });
                if (lbl.length)
                    lbls[k].Value = lbl[0].Value;
            }
        });
    },

    getLabel: function (labelName) {
        var lbl = this._txnLabels.filter(function (l) { return l.Name == labelName; });
        if (lbl.length)
            return lbl[0].Value || lbl[0].DefaultValue;
        else
            return labelName;
    },

    get_controlId: function () { return this._controlId; },
    set_controlId: function (value) { this._controlId = value; }
}

Camstar.WebPortal.WebPortlets.SideBar.registerClass('Camstar.WebPortal.WebPortlets.SideBar', Sys.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

