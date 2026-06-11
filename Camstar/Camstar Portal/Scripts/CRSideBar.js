// Copyright Siemens 2023

/*  
 *  Implements interface for pages to customize core SideBar using only client script
 */
(function CR_SideBar() {
    'use strict';

    var api = CR.GetNamespace('CR.SideBar');

    api.addMenuItem = addMenuItem;
    api.addSeparatorToLastItem = addSeparatorToLastItem;
    api.beginCustomize = beginCustomize;
    api.clearCustomization = clearCustomization;
    api.containsButton = containsButton;
    api.endCustomize = endCustomize;
    api.haveCustomItems = haveCustomItems;
    api.hideStandardItemActiveTimers = hideStandardItemActiveTimers;
    api.hideStandardItemAttachDocument = hideStandardItemAttachDocument;
    api.hideStandardItemContainerStatus = hideStandardItemContainerStatus;
    api.hideStandardItemContainerWorkflow = hideStandardItemContainerWorkflow;
    api.hideStandardItemDocuments = hideStandardItemDocuments;
    api.hideStandardItemMfgAuditTrail = hideStandardItemMfgAuditTrail;
    api.hideStandardItemEProcRecordProductionEvent = hideStandardItemEProcRecordProductionEvent;
    api.hideStandardItemEProcTaskList = hideStandardItemEProcTaskList;
    api.isVisible = isVisible;
    api.removeBadge = removeBadge;
    api.reset = reset;
    api.setContainerNameFunction = setContainerNameFunction;
    api.setCustomContainerStatusServerType = setCustomContainerStatusServerType;
    api.setupLayout = setupLayout;
    api.setSeparatorOnLastVisibleItem = setSeparatorOnLastVisibleItem;
    api.showButton = showButton;
    api.showSeparator = showSeparator;
    api.sideBarJQuery = getSideBarJQuery();
    api.submitEProc = submitEProc;
    api.update = update;
    api.updateAdditionalInfo = updateAdditionalInfo;
    api.updateBadge = updateBadge;
    api.updateButtonImage = updateButtonImage;

    var STANDARD_ITEM_ID = {
        CONTAINER_ACTIVE_TIMERS: 'ContainerActiveTimers',
        CONTAINER_ATTACH_DOCUMENT: 'ContainerAttachDocument',
        CONTAINER_DOCUMENTS: 'ContainerDocuments',
        CONTAINER_MFG_AUDIT_TRAIL: 'ContainerMfgAuditTrail',
        CONTAINER_STATUS: 'ContainerStatus',
        CONTAINER_WORKFLOW: 'ContainerWorkflow',
        EPROC_RECORD_PRODUCTION_EVENT: 'RecordProductionEvent',
        EPROC_TASK_LIST: 'EProcTaskList'
    };
    Object.freeze(STANDARD_ITEM_ID);

    var _sideBar;
    var _customCommandItems = [];
    var _showButtonTimeout;
    var _containerNameFunction = null;

    function haveCustomItems() {
        return _customCommandItems.length > 0;
    }
    /**
     * Verifies the SideBar has been initialized and logs a warning message if not.
     * 
     * @param {string} method - name of the calling function to add in the logged message;
     */
    function verifyInitialized(method) {
        var initialized = !!_sideBar;
        if (!_sideBar)
            console.warn('CR.SideBar.' + method + ': SideBar not initialized. Call "CR.SideBar.beginCustomize" first.');
        return initialized;
    }


    /**
     * This function provides a mechanism for a function to be used to get the container name dynamically
     * for any sidebar button or function that needs a container name. The sidebar buttons by default
     * require a container status webpart to be present on a page. Not all pages use one of these web 
     * parts. This function will take a function reference and assign it to the ContainerStatusWebPart_Deco object
     * From there it will be available to any functionality that needs it.
     * 
     * @param {function} [functionReference] - Function to use to get the container name.
     */
    function setContainerNameFunction(functionReference) {
        let sideBar = getSideBar();
        if (sideBar) {
            if (sideBar._commandSettings.containerNameFunction !== functionReference)
                _containerNameFunction = sideBar._commandSettings.containerNameFunction;
            sideBar._commandSettings.containerNameFunction = functionReference;
        }
    }

    /**
     * Override the standard data shown in Container Status slideout by specifying a custom server type to handle the processing.
     * This must be a class derived from Camstar.WebPortal.Helpers.ContainerStatusInquiry;
     * 
     * @param {string} serverType - The name of the server type class including namespace.
     */
    function setCustomContainerStatusServerType(serverType) {
        if (!verifyInitialized('setCustomContainerStatusServerType'))
            return;

        _sideBar._commandSettings.customContainerStatusServerType = serverType;
    }

    /**
     * Initializes the SideBar. 
     * Call this prior to adding any menu items.
     */
    function beginCustomize() {
        _sideBar = getSideBar();

        if (_sideBar) {
            console.log('found SideBar with Id: ' + _sideBar._controlId);
            clearCustomization();
            //_customCommandItems = [];
        } else {
            console.warn('SideBar not found!');
        }

        return !!_sideBar;
    }

    /**
     * Adjusts the layout of the SideBar by calling a function on the Ajax Library script object to position itself properly in the page.
     */
    function setupLayout() {
        getSideBar()._layoutSetup();
    }

    /**
     * Gets a reference to the Ajax Library script object defined in SideBar.js
     */
    function getSideBar() {
        var sideBar = $find('ctl00_SideBarRight');
        if (!sideBar && parent.$find) {
            sideBar = parent.$find('ctl00_SideBarRight');
        }
        return sideBar;
    }

    /**
     * Completes customization by adding all the custom menu items to the SideBar.
     * Call this after all calls to "addMenuItem" and "hideStandardItem*" functions.
     */
    function endCustomize() {
        if (!verifyInitialized('endCustomize'))
            return;

        _customCommandItems.forEach(function (item) {
            _sideBar._commandSettings.AddCustomCommandBarItem(item);
        });

        _sideBar.update(true);
    }

    /**
     * Removes all custom items from the SideBar and optionally refreshes display of the SideBar.
     * 
     * @param {boolean} update - If true, the SideBar display is updated. 
     */
    function clearCustomization(update) {
        if (!verifyInitialized('clearCustomization'))
            return;

        _customCommandItems = [];
        _sideBar._commandSettings.GetCustomCommandBarItems().length = 0;
        _sideBar._commandSettings.containerNameFunction = _containerNameFunction;
        if (update)
            _sideBar.update(true);
    }

    /**
     * Adds a menu item to the internal list of items
     * 
     * @param {string} id - Unique identifier for command item. To override a default item, use the same name as the default item. (see Control.js).
     * @param {string} [nameText] - Text to use for button caption.
     * @param {string} [nameLabel] - Label name to use for button caption. Ignored if nameText set. Standard SideBar processing looks up label value.
     * @param {string} [imageClass] - CSS class that defines image to use for button.
     * @param {function} [clickHandler] - Callback for handling button click.
     * @param {boolean} [primary] - If true, button rendered in primary styling, otherwise will be secondary.
     * @param {boolean} [visible] - Flag to control button visibility.
     * @param {number} [order] - Identifies the position of menu item from top down. If not set, order based on position inserted after items with a value set.
*/
    function addMenuItem(id, nameText, nameLabel, imageClass, clickHandler, primary, visible, order) {
        if (order === undefined || order === null)
            order = _customCommandItems.length + 1;

        var item = new CommandItem(id, nameText, nameLabel, imageClass, clickHandler, primary, visible, order);
        _customCommandItems.push(item);
        return item;
    }

    /**
     * Hides a standard command menu item defined for either the EProcedure page or a Container Status Web Part page.
     * Item is hidden by adding an override that has has visible = false.
     * 
     * @param {string} id - Id of the standard menu item to hide.
     */
    function hideStandardItem(id) {
        var item = new CommandItem(id, id, null, 'cr-button-scrap', null, false, false);
        _customCommandItems.push(item);
        return item;
    }

    function hideStandardItemActiveTimers() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_ACTIVE_TIMERS);
    }

    function hideStandardItemAttachDocument() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_ATTACH_DOCUMENT);
    }

    function hideStandardItemDocuments() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_DOCUMENTS);
    }

    function hideStandardItemMfgAuditTrail() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_MFG_AUDIT_TRAIL);
    }

    function hideStandardItemContainerStatus() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_STATUS);
    }

    function hideStandardItemContainerWorkflow() {
        return hideStandardItem(STANDARD_ITEM_ID.CONTAINER_WORKFLOW);
    }

    function hideStandardItemEProcRecordProductionEvent() {
        return hideStandardItem(STANDARD_ITEM_ID.EPROC_RECORD_PRODUCTION_EVENT);
    }

    function hideStandardItemEProcTaskList() {
        return hideStandardItem(STANDARD_ITEM_ID.EPROC_TASK_LIST);
    }


    /**
     * Adds a separator to the last item currently in the custom item list.
     */
    function addSeparatorToLastItem() {
        if (!verifyInitialized('addSeparatorToLastItem'))
            return;

        if (_customCommandItems.length > 0)
            _customCommandItems[_customCommandItems.length - 1].addSeparator();
    }

    function setSeparatorOnLastVisibleItem(skipIDs) {
        if (!verifyInitialized('setSeparatorOnLastVisibleItem'))
            return;

        // make list of all visible items not in the skip list
        var visibleItems = [];
        _customCommandItems.forEach(function (item) {
            var skipItem = skipIDs && skipIDs.some(function (id) { return id === item.Id; });
            if (!skipItem && item.isVisible())
                visibleItems.push(item);
        });

        var lastIndex = visibleItems.length - 1;
        for (var i = 0; i <= lastIndex; i++) {
            showSeparator(visibleItems[i].Id, i === lastIndex);
        }
    }

    /**
     * Causes the SideBar to refresh
     */
    function update() {
        if (!verifyInitialized('update'))
            return;

        _sideBar.update(true);
    }

    /**
     * Resets each custom command to initial settings and updates the side bar.
     */
    function reset() {
        if (!verifyInitialized('reset'))
            return;

        _sideBar._commandSettings.GetCustomCommandBarItems().forEach(function (cmdItem) {
            cmdItem.reset();
        });
        _sideBar.update(true);
    }

    /**
     * Returns true if the button is in the command bar and is visible
     *
     * @param {string} id - Unique Id of button.
     */
    function isVisible(id) {
        if (!verifyInitialized('isVisible'))
            return;
        let visible = false;
        _sideBar._commandSettings.GetCustomCommandBarItems().some(function (cmdItem) {
            if (cmdItem.Id === id) {
                visible = cmdItem.Action.Visible;
                return true;
            }
        });
        return visible;
    }

    /**
     * Set the Visible setting on a menu item and optionally redraw the menu.
     * 
     * @param {string} id - Unique Id of button.
     * @param {boolean} show - Set true to mark button as visible.
     */
    function showButton(id, show) {
        if (!verifyInitialized('showButton'))
            return;

        //make sure show arg is boolean
        show = !!show;

        var visibilityChanged = false;

        //TODO: couldn't I just iterate the local list?
        _sideBar._commandSettings.GetCustomCommandBarItems().some(function (cmdItem) {
            if (cmdItem.Id === id) {
                if (show !== cmdItem.Action.Visible) { 
                    visibilityChanged = true;
                    cmdItem.Action.Visible = show;
                }
                return true;
            }
        });

        // do update in timeout so only done once if multiple calls in a row.
        if (visibilityChanged) {
            if (_showButtonTimeout) 
                clearTimeout(_showButtonTimeout);
            _showButtonTimeout = setTimeout(function () { _sideBar.update(true); }, 150);
        }
    }

    /**
     * Update the additional info section of a menu item.
     * 
     * @param {string} id - Id of the menu item.
     * @param {string} text - Text to set.
     */
    function updateAdditionalInfo(id, text) {
        if (!verifyInitialized('updateAdditionalInfo'))
            return;

        _sideBar._updateAdditionalInfo(id, text);
    }

    /**
     * Updates the badge settings to show, hide, change count, update theme.
     * 
     * @param {string} id - Unique Id of button.
     * @param {number} count - Notification count to show in badge.
     * @param {string} theme - Controls badge color. Allowed options [blue, red, light]
     * @param {boolean} showEmpty - Set true to show a badge with no number when notification count is 0.
     */
    function updateBadge(id, count, theme, showEmpty) {
        if (!verifyInitialized('updateBadge'))
            return;

        _sideBar._updateBadge(id, count, theme, showEmpty);
    }

    /**
     * Removes badge from display on a button. Leaves button still marked as supporting a badge
     * 
     * @param {string} id - Unique Id of button.
     */
    function removeBadge(id) {
        updateBadge(id, 0, null, false);    // remove by setting count to 0 and not showing when empty.
    }

    function updateButtonImage(id, imageClass) {
        if (!verifyInitialized('updateButtonImage'))
            return;

        _sideBar._updateCommandImage(id, imageClass);
    }

    /**
     * Returns true if the menu item with the specified Id is in the custom item list.
     * 
     * @param {any} id - Unique Id of the menu item.
     */
    function containsButton(id) {
        if (!verifyInitialized('containsButton'))
            return;

        return _sideBar._commandSettings.GetCustomCommandBarItems().some(function (cmdItem) {
            return cmdItem.Id === id;
        });
    }

    /**
     * Updates the specified menu item to show or hide a separator bar beneath it.
     *
     * @param {string} id - Unique Id of the menu item to update.
     * @param {boolean} show - If true, then add a separator bar, otherwise remove it.
     */
    function showSeparator(id, show) {
        if (!verifyInitialized('showSeparator'))
            return;

        //make sure show arg is boolean
        show = !!show;

        return _sideBar._updateSeparator(id, show);
    }

    /**
     * Gets jQuery from the window containing the sidebar.
     */
    function getSideBarJQuery() {
        return CR.Page.isInEProcedurePage() ? parent.window.jQuery : window.jQuery;
    }

    /**
     * Click the Submit button in EProcedure page.
     */
    function submitEProc() {
        //TODO: logic is from ES.Utility, may be a better way or may simply need to update
        //      not sure about assuming parent would have __page and that would have submit.
        var $sideBar = getSideBarJQuery();
        var btn = $sideBar('#ctl00_WebPartManager_ActionsControl_ExecuteAction');
        if (btn.length)
            btn.click();
        else
            parent.__page.submit();
    }

    /**
     * Constructor function to create an instance of a CommandItem object
     *
     * @param {string} id - Unique identifier for command item. To override a default item, use the same name as the default item. (see Control.js).
     * @param {string} [nameText] - Text to use for button caption.
     * @param {string} [nameLabel] - Label name to use for button caption. Ignored if nameText set. Standard SideBar processing looks up label value.
     * @param {string} imageClass - CSS class that defines image to use for button.
     * @param {function} [clickHandler] - Callback for handling button click.
     * @param {boolean} [primary] - If true, button rendered in primary styling, otherwise will be secondary.
     * @param {boolean} [visible] - Flag to control button visibility.
     * @param {number} [order] - Identifies the position of menu item from top down. If not set, order based on position inserted after items with a value set.
     */
    function CommandItem(id, nameText, nameLabel, imageClass, clickHandler, primary, visible, order) {

        this.InitialState = {
            additionalInfoText: '',
            badge: false,
            clickHandler: clickHandler,
            hasSeparator: false,
            id: id,
            isActionWithSeparator: false,
            imageClass: imageClass,
            nameText: nameText,
            nameLabel: nameLabel,
            order: order > 0 ? order : 0,
            panelBuilder: null,
            primary: !!primary,
            showAdditionalInfo: false,
            showEmptyBadge: false,
            visible: !!visible
        }

        this.initialize(this.InitialState);
        //TODO: doing this to support ability to reset to initial state, but is it really needed?
    }

    CommandItem.prototype = {
        initialize: function (options) {
            this.Action = {
                OnClick: options.clickHandler,
                PanelBuilder: options.panelBuilder,
                Visible: !!options.visible
            };
            this.AdditionalInfo = {
                Text: options.additionalInfoText,
                Visible: options.showAdditionalInfo
            };
            this.CSS = {
                Bar: ['cmdbar-custom-button']
            };
            this.Icon = {
                CSS: options.imageClass || ''
            };
            this.Id = options.id;
            this.IsPrimary = !!options.primary;
            this.Name = {
                Label: options.nameLabel || '',
                Text: options.nameText || ''
            };
            this.Order = options.order > 0 ? options.order : 0;

            // the presence of the Badge obj indicates the command item supports a badge. so do not add unless specified.
            if (options.badge) {
                this.Badge = {
                    Count: 0,
                    Theme: 'light',
                    ShowEmpty: !!options.showEmptyBadge
                };
            }

            if (options.hasSeparator) {
                this.CSS.Bar.push('add-separator');
            }
        },

        /**
         * Resets command item properties to initial values when first created.
         */
        reset: function () {
            this.initialize(this.InitialState);
            return this;
        },

        /**
         * Marks the command item as possibly showing a badge. 
         * Call this when adding the menu item if it may show a badge even if not initially visible.
         * 
         * @param {boolean} showEmptyBadge
         */
        setBadge: function (showEmptyBadge) {
            this.Badge = {
                Count: 0,
                Theme: 'light',
                ShowEmpty: !!showEmptyBadge
            };

            this.InitialState.badge = true;
            this.InitialState.showEmptyBadge = !!showEmptyBadge;

            return this;
        },

        /**
         * Sets initial state for the Additional Info section
         * 
         * @param {boolean} [visible=false] - Set true to always show the section.
         * @param {string} [text=''] - Text to display.
         */
        setAdditionalInfo: function (visible, text) {
            this.AdditionalInfo.Visible = !!visible;
            this.AdditionalInfo.Text = text ? text : '';

            this.InitialState.showAdditionalInfo = this.AdditionalInfo.Visible;
            this.InitialState.additionalInfoText = this.AdditionalInfo.Text;

            return this;
        },

        /**
         * Sets initial state for Panel Builder
         * 
         * @param {function} callback - Function to call for getting contents of a slide out panel.
         */
        setPanelBuilder: function (callback) {
            this.Action.PanelBuilder = callback;

            this.InitialState.panelBuilder = callback;

            return this;
        },

        /**
         * Flag the menu item as needing a visible border line between it and lower items.
         * Use only when initially adding items to the SideBar.
         * For updating items after initialized, use "updateSeparator"
         */
        addSeparator: function () {
            this.CSS.Bar.push('add-separator');
            this.InitialState.hasSeparator = true;

            return this;
        },

        /**
         * Add or remove the class to show a separator below the menu item.
         * 
         * @param {boolean} show - Indicates if menu items should show the separator.
         */
        updateSeparator: function (show) {
            var index = this.CSS.Bar.indexOf('add-separator');

            if (show && index === -1)
                this.CSS.Bar.push('add-separator');
            else if(!show && index > -1)
                this.CSS.Bar.splice(index, 1);

            return this;
        },


        isVisible: function () {
            return this.Action.Visible;
        }

    };


})();
