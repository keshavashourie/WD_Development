// Copyright Siemens 2024

/*
 * CR.SlideOut
 * Manages multiple generic slide out panels than can open from left or right
 */
(function CR_SlideOut () {
    'use strict';

    var api = CR.GetNamespace('CR.SlideOut');

    api.close = close;              // closeSlideOut        old ES_Utility.js names
    api.closeAll = closeAll;        // closeAllSlideOuts
    api.closeAllRightSlideOut = closeAllRightSlideOut; // closeAllRightSideSlideOuts
    api.getContent = getContent;    // getSlideOutContent
    api.isOpen = isOpen;            // isSlideOutOpen
    api.open = open;                // showSlideOut
    api.resizeAll = resizeAll;      // resizeSlideouts

    // globals
    var activeSlideouts = [];

    /**
     * Open a slide out panel.
     * 
     * @param {object} $owner - DOM element the slide-out is related to (ie the canvas container - NOT necessarily the parent).  Match its height.
     * @param {string} slideOutId - Uniquely identifies this slide out panel. Used in "id" attributes of DOM elements.
     * @param {string} title - Text to appear in header of slide out panel.
     * @param {object|string} content - DOM object or HTML that defines content of the panel.
     * @param {string|number} width - Defines panel width and assumed to be pixels. Can use ["300px", "300", 300]
     * @param {string} [side='right'] - Defines the side panel opens from.
     * @param {function} [closeCallback] - Optional function to call when panel closes.
     * @param {function} [resizeCallback] - Optional function to call when page resizes.
     * @param {boolean} [noPadding=false] - Set true to not use padding in panel.
     * @param {any} [$slideFrom=$owner.parent()] - Sets element panel html is appended to.
     */
    function open($owner, slideOutId, title, content, width, side, closeCallback, resizeCallback, noPadding, $slideFrom) {
        if (typeof side === 'undefined') {
            side = 'right';
        }
        if (['left', 'right'].indexOf(side) === -1) {
            console.warn('SlideOut - invalid side argument: ' + side + ', defaulting to right');
            side = 'right';
        }
        var otherSide = (side === 'right') ? 'left' : 'right';
        let contentClass = noPadding ? "cr-slide-out-content-nopadding" : "cr-slide-out-content";
        var slideOutHtml =
            '<div id="container-' + slideOutId + '" class="cr-slide-out-container">' +
            '<div id="' + slideOutId + '" class="cr-slide-out">' +
            '<div id="header-' + slideOutId + '" class="cr-slide-out-header">' +
            '<span class="cr-title">' + title + '</span>' +
            '<span class="cr-close-button">' +
            '<div class="item-icon cr-close"></div>' +
            '</span>' +
            '</div>' +
            '<div id="content-' + slideOutId + '" class="' + contentClass + '">' +
            '</div>' +
            '</div>'
            + '</div>';

        var containerSelector = '#container-' + slideOutId;
        var slideOutSelector = '#' + slideOutId;
        var headerSelector = '#header-' + slideOutId;

        // ensure element the panel is sliding out from is a positioned element.
        $slideFrom = $slideFrom ? $slideFrom : $owner.parent();
        var parentPosition = $slideFrom.css('position');
        if (parentPosition !== 'relative' && parentPosition !== 'absolute') {
            console.warn('Slideout - parent element ' + $slideFrom.get(0).id + ' position is ' + parentPosition + '.  Should be relative or absolute.');

            // Try to add relative positioning
            $slideFrom.css('position', 'relative');
        }

        // find max z-index of already open slide outs
        var maxExistingZIndex = 0;
        $slideFrom.find('.cr-slide-out-container').each(function () {
            var zIndex = parseInt($(this).css('z-index'), 10);
            if (zIndex > maxExistingZIndex) {
                maxExistingZIndex = zIndex;
            }
        });

        //  If not yet created, add content to the DOM
        var contentSelector = '#content-' + slideOutId;
        if ($(containerSelector).length === 0) {
            $slideFrom.append(slideOutHtml);
        } else {
            $(containerSelector + " span.cr-title").text(title); // Assemble panel may change title
        }

        // If given content, clear out any old objects and append the new stuff
        if (content) {
            $(contentSelector).empty();
            $(contentSelector).append($(content));
        }

        var parentBorderTopWidth = parseInt($slideFrom.css('border-top-width'), 10);
        var parentPaddingSide = parseInt($slideFrom.css('padding-' + side), 10);

        $(containerSelector)
            .css('z-index', maxExistingZIndex + 1)
            .css('top', $owner.offset().top - $slideFrom.offset().top - parentBorderTopWidth)
            .css(side, parentPaddingSide)
            .css('height', $owner.outerHeight(false) + 'px');  // innerHeight() include padding

        // width override
        var shadowSize = 10;
        if (width) {
            $(containerSelector).css('width', parseInt(width, 10) + shadowSize + 'px');
        }

        var soLeftBorderWidth = parseInt($(slideOutSelector).css('border-left-width'), 10);
        var soRightBorderWidth = parseInt($(slideOutSelector).css('border-right-width'), 10);

        // Start the slide-out all the way over (hidden)
        $(slideOutSelector)
            .css('height', $owner.height() + 'px')
            .width($(containerSelector).width() - shadowSize - soLeftBorderWidth - soRightBorderWidth)  // shrink the slideout by the border width so it fits inside the container
            .css(otherSide, $(containerSelector).width());

        var contentPaddingTop = parseInt($(contentSelector).css('padding-top'), 10);
        var contentPaddingBottom = parseInt($(contentSelector).css('padding-bottom'), 10);

        var selectorHeight = $(slideOutSelector).height();
        var sideHeight = $('#ctl00_sidebar').length > 0 ? $('#ctl00_sidebar').height() : selectorHeight;
        var maxHeight = Math.min(sideHeight, selectorHeight);
        // Set content container height so it can overflow for large amounts of content
        $(contentSelector).height(maxHeight - $(headerSelector).height() - contentPaddingTop - contentPaddingBottom);

        var openClass = 'open-from-' + side;
        $(slideOutSelector).addClass(openClass);

        // Handler for clicking 'X'
        // this will keep adding additional callbacks each time slideout is opened, resulting in any callback being called multiple times.
        // to prevent that, first remove any existing click handlers
        $(headerSelector + ' div.cr-close').off('click').click(closeClick);

        function closeClick() {
            // Slide back out of view
            $(slideOutSelector).removeClass(openClass);
            setTimeout(function () {
                $(containerSelector).css('height', '0');

                if (closeCallback)
                    closeCallback();
            }, 250);

            // Hide container div (wait until slide animation done)

            activeSlideouts = activeSlideouts.filter(function (s) { return s.id !== slideOutId; });
        }

        function resizeSlideout() {
            var $container = $(containerSelector);
            var $slideOut = $(slideOutSelector);
            var $content = $(contentSelector);
            var $header = $(headerSelector);

            $container.css('height', $owner.outerHeight(false) + 'px');
            $slideOut.css('height', $owner.height() + 'px');

            var paddingTop = parseInt($content.css('padding-top'), 10);
            var paddingBottom = parseInt($content.css('padding-bottom'), 10);

            $content.height($slideOut.height() - $header.height() - paddingTop - paddingBottom);

            if (resizeCallback)
                resizeCallback();
        }

        activeSlideouts.push({ id: slideOutId, resize: resizeSlideout });

        var slideOutReturn = {
            slideOutSelector: slideOutSelector,
            close: function () {
                $(headerSelector + ' div.cr-close').click();
            },
            enableClose: function (enable) {
                if (enable) {
                    $(headerSelector + ' span.cr-close-button')
                        .removeClass('cr-disabled')
                        .find('div.cr-close')
                        .click(closeClick);
                } else {
                    $(headerSelector + ' span.cr-close-button')
                        .addClass('cr-disabled')
                        .find('div.cr-close')
                        .off('click');
                }
            }
        };

        return slideOutReturn;
    }

    /**
     * Closes the specified panel
     * 
     * @param {string} slideOutId - Id of panel used when opening
     */
    function close(slideOutId) {
        if (isOpen(slideOutId)) {
            var slideOutSelector = '#' + slideOutId;
            $(slideOutSelector + ' .cr-slide-out-header .cr-close').click();
        }
    }

    /**
     * Simulate clicking the 'X' for all open panels
    */
    function closeAll() {
        $('.cr-slide-out-header .cr-close').click();
    }

    /**
     * Simulate clicking the 'X' for all open panels on right side slide out
     * Major usage on remain panel open for latest side bar button clicked
     * @param {string} id - side bar button id that wish to remain open
    */
    function closeAllRightSlideOut(id) {
        if (typeof id === 'undefined') {
            closeAll();
        }
        else {
            var slideOutSelector = '.open-from-right';
            var excludeElement = $(slideOutSelector).not('#' + id);
            var findElement = $(excludeElement).find('.cr-slide-out-header .cr-close');
            $(findElement).click();
        }
    }

    /**
     * Returns true if the requested panel is being displayed to the user.
     * @param {string} slideOutId 
     */
    function isOpen(slideOutId) {
        var slideOutSelector = '#' + slideOutId;
        var isDisplayed = $(slideOutSelector).hasClass("open-from-left") || $(slideOutSelector).hasClass("open-from-right");

        return isDisplayed;
    }

    /**
     * Get jQuery object for the DOM inside the slide out content area (not including the header)
     * 
     * @param {string} slideOutId
     */
    function getContent(slideOutId) {
        return $('div#content-' + slideOutId);
    }

    /**
     * Call resize handler for all active slide out panels.
    */
    function resizeAll() {
        activeSlideouts.forEach(function (s) { s.resize(); });
    }

})();

/*
 * CR.DocSlideOut
 * Manages a slide out panel that holds a list of document sets containing one or more documents.
 */
(function CR_DocSlideOut() {
    'use strict';

    var api = CR.GetNamespace('CR.DocSlideOut');

    api.close = close;          
    api.open = open;
    api.setDefaultIconPath = setDefaultIconPath;

    api.DocCollection = DocCollection;
    api.DocEntry = DocEntry;
    api.DocSet = DocSet;

    // globals
    const _slideOutId = 'DocumentsSlide';
    var _defaultIconPath = 'assets/image/';

    var TEMPLATE = {
        docSetItem:
            "<li>\
                <div class='dropdown-menu-doc-set' tabindex='-1'>\
                    <span class='cr-content-header-noHPad'>__DOC_SET_NAME__</span>\
                </div>\
            </li>",
        docEntryItem:
            "<li class= 'dropdown-menu-doc-item'>\
                <div class='dropdown-menu-doc-item-link'>\
                    <img class='cr-document-image' src='__ICON_SRC__'/>\
                    <span class='cr-doc-item-text'>__DOC_NAME__</span>\
                    __OPEN_NEW_TAB_SPAN__\
                    __OPEN_POPUP_SPAN__\
                    <div class='cr-document-description'>__DOC_DESC__</div>\
                </div>\
            </li>",
        openPopupSpan:
            "<span class='cr-open-popup'>&nbsp;</span>",
        openNewTabSpan:
            "<span class='cr-open-new-tab'>&nbsp;</span>"
    }

    $(initialize);

    function initialize() {
        CR.Page.localizeLabels(['Lbl_NoDataToDisplay', 'Web_OpenPopup','Web_OpenNewTab'], labelsLoaded);
    }

    function labelsLoaded() {
        TEMPLATE = {
            docSetItem:
                "<li>\
                <div class='dropdown-menu-doc-set' tabindex='-1'>\
                    <span class='cr-content-header-noHPad'>__DOC_SET_NAME__</span>\
                </div>\
            </li>",
            docEntryItem:
                "<li class= 'dropdown-menu-doc-item'>\
                <div class='dropdown-menu-doc-item-link'>\
                    <img class='cr-document-image' src='__ICON_SRC__'/>\
                    <span class='cr-doc-item-text'>__DOC_NAME__</span>\
                    __OPEN_NEW_TAB_SPAN__\
                    __OPEN_POPUP_SPAN__\
                    <div class='cr-document-description'>__DOC_DESC__</div>\
                </div>\
            </li>",
            openPopupSpan:
                "<span class='cr-open-popup' title='" + CR.Page.getLabelValue("Web_OpenPopup") + "'>&nbsp;</span>",
            openNewTabSpan:
                "<span class='cr-open-new-tab' title='" + CR.Page.getLabelValue("Web_OpenNewTab") + "'>&nbsp;</span>"
        };
    }
    /**
     * Close the doc slide out panel.
     */
    function close() {
        CR.SlideOut.close(_slideOutId);
    }

    /**
     * Open the document panel and render the specified list of document sets
     * 
     * @param {object[]} documents - An array of document set objects or an instance of DocCollection
     * @param {object} $matchHeight - jQuery object of element to use for setting the panel height. 
     * @param {object} $slideFrom - jQuery object of element that the slide out panel is appended to.
     */
    function open(documents, $matchHeight, $slideFrom, closeCallback = null) {
        var docSets;
        var content;

        if (Array.isArray(documents)) {
            // documents is an array of DocSets
            docSets = documents;
        } else {
            // documents is a DocCollection object. show any doc entries not in a set first.
            docSets = documents.sets;
            if (documents.entries && documents.entries.length > 0) {
                content = $("<ul class='slideout-menu'></ul>");
                documents.entries.forEach(function (docEntry) {
                    // assume doc in form of obj with Identifier, Name, Revision properties.
                    appendDocEntryContent(content, getDocEntryfromIdentifierNameRevision(docEntry));
                });
            }
        }

        if (content) {
            if (docSets && docSets.length > 0)
                appendContentForAllDocSets(content, docSets);
        } else if (docSets && docSets.length > 0) {
            content = $("<ul class='slideout-menu'></ul>");
            appendContentForAllDocSets(content, docSets);
        } else {
            content = '<div id="cr-empty-state-tools" class="cr-empty-state-txt">' + CR.Page.getLabelValue("Lbl_NoDataToDisplay") + '</div>';
        }

        if (CR.SlideOut.isOpen(_slideOutId))
            close();
        else {
            CR.SlideOut.open($matchHeight, _slideOutId, "Documents", content, 450, 'right', closeCallback, null, false, $slideFrom);
        }
    }

    /**
     * Appends content for all the specified DocSets to the parent <ul>
     * 
     * @param {object} $ul -  - jquery object of the parent <ul>.
     * @param {object[]} docSets - An array of document set objects that represent a serialized DocumentSet CDO, or the generic DocSet obj defined in this file.
     */
    function appendContentForAllDocSets($ul, docSets) {
        docSets.forEach(function (docSet) {
            if (docSet.hasOwnProperty('DocumentEntries') || docSet.hasOwnProperty('DocumentSet_DocumentEntries')) {
                // is a serialized DocumentSet CDO
                let set = getDocSetFromDocumentSetCDO(docSet);
                if(set)
                    appendDocSetContent($ul, set);
            } else if (docSet.name && docSet.entries && docSet.entries.length > 0) {
                // doc set already based on DocSet object defined in this file.
                appendDocSetContent($ul, docSet);
            } else {
                // ignore any other obj type
                console.warn('Unknown Document Set format.')
            }
        });

        return $ul;
    }

    /**
     * Append document set content to the specified parent object.
     * 
     * @param {object} $ul - jquery object of the parent <ul>.
     * @param {object} docSet - An instance of a DocSet object.
     */
    function appendDocSetContent($ul, docSet) {
        $ul.append(TEMPLATE.docSetItem.replace(/__DOC_SET_NAME__/, docSet.name));
        docSet.entries.forEach(function (docEntry) {
            // the entry may be an object with Identifier, Name, Revision properties rather than a DocEntry instance.
            if (docEntry.hasOwnProperty('Identifier') && docEntry.hasOwnProperty('Name'))
                docEntry = getDocEntryfromIdentifierNameRevision(docEntry);
            appendDocEntryContent($ul, docEntry);
        });
    }

    /**
     * Append document entry content as an <li> in the <ul>
     * 
     * @param {object} $ul - jQuery object of the parent <ul>
     * @param {object} docEntry - An instance of a DocEntry object.
     */
    function appendDocEntryContent($ul, docEntry) {
        // if icon is just a file name, use the default path.
        var iconSrc = CR.Path.isPath(docEntry.icon) ? docEntry.icon : _defaultIconPath + docEntry.icon;

        var $entry = $(TEMPLATE.docEntryItem
            .replace(/__ICON_SRC__/, iconSrc)
            .replace(/__DOC_NAME__/, docEntry.name)
            .replace(/__OPEN_POPUP_SPAN__/, docEntry.openPopupHandler ? TEMPLATE.openPopupSpan : '')
            .replace(/__OPEN_NEW_TAB_SPAN__/, docEntry.openTabHandler ? TEMPLATE.openNewTabSpan : '')
            .replace(/__DOC_DESC__/, docEntry.description ? docEntry.description : '')
        );

        $entry.find('div.dropdown-menu-doc-item-link').click(docEntry.openPopupHandler ? docEntry.openPopupHandler : docEntry.openTabHandler);
        if (docEntry.openPopupHandler)
            $entry.find('span.cr-open-popup').click(docEntry.openPopupHandler);
        if (docEntry.openTabHandler)
            $entry.find('span.cr-open-new-tab').click(docEntry.openTabHandler);

        $ul.append($entry);
    }

    //TODO: below function checks for a field ES_DocSetModelObjectTypeName defined in ES workspace.
    //      add DocSetModelObjectTypeName to csi workspace on DocumentSet CDO
    //      then update all CLFs that use ES_DocSetModelObjectTypeName to use the new csi field
    /**
     * Gets an instance of a DocSet object based on values from a serialized version of a DocumentSet CDO
     * 
     * @param {object} documentSet - An object holding the contents of a DocumentSet CDO instance.
     */
    function getDocSetFromDocumentSetCDO(documentSet) {
        var docSet, setName;

        // DocumentSet CDO may get serialized differently. handle each variation
        if (documentSet.hasOwnProperty('DocumentEntries')) {
            setName = getSetName(documentSet.ES_DocSetModelObjectTypeName, documentSet.DocSetModelObjectName, documentSet.Name, documentSet.Self);
            if (setName) {
                docSet = new DocSet(setName);
                documentSet.DocumentEntries.forEach(function (entry) {
                    var args = getEntryArgs(entry.DocumentIdentifier, entry.FileName, entry.Document, entry.Name, entry.Description, entry.DocumentBrowseMode);
                    if (args.Identifier && args.Name)
                        docSet.addEntry(getDocEntryfromIdentifierNameRevision(args));
                });
            }
        } else if (documentSet.hasOwnProperty('DocumentSet_DocumentEntries')) {
            setName = getSetName(documentSet.DocumentSet_ES_DocSetModelObjectTypeName, documentSet.DocumentSet_DocSetModelObjectName, documentSet.DocumentSet_Name, documentSet.Self);
            if (setName) {
                docSet = new DocSet(setName);
                documentSet.DocumentSet_DocumentEntries.forEach(function (entry) {
                    var args = getEntryArgs(entry.DocumentEntry_DocumentIdentifier, entry.DocumentEntry_FileName, entry.DocumentEntry_Document, entry.DocumentEntry_Name, entry.Description, entry.DocumentBrowseMode);
                    if (args.Identifier && args.Name)
                        docSet.addEntry(getDocEntryfromIdentifierNameRevision(args));
                });
            }
        }

        return docSet;

        function getSetName(typeNameField, objNameField, nameField, selfField) {
            var name;

            //TODO: FF scenario 52.82_04_23 wants name as <type> - <self>.
            //      making that top priority below, but should get clarity on how we want to set this.

            if (typeNameField && typeNameField.Value && selfField && selfField.Name && typeNameField.Value !== selfField.Name)
                name = typeNameField.Value + ' - ' + selfField.Name;
            else if (typeNameField && typeNameField.Value && objNameField && objNameField.Value)
                name = typeNameField.Value + ' - ' + objNameField.Value;
            else if (nameField && nameField.Value)
                name = nameField.Value;
            else if (selfField && selfField.Name)
                name = selfField.Name;

            return name;
        }

        function getEntryArgs(identifierField, fileNameField, documentField, nameField, descriptionField, browseMode) {
            var args = { Identifier: '', Name: '', Revision: '', DisplayName: '', Description: '', BrowseMode: '' };

            // use DocumentEntry.Identifier as priority over DocumentEntry.FileName
            if (identifierField && identifierField.Value)
                args.Identifier = identifierField.Value;
            else if (fileNameField && fileNameField.Value)
                args.Identifier = fileNameField.Value;

            // get Name and Revision of the DocumentEntry.Document
            if (documentField) {
                if (documentField.Name)
                    args.Name = documentField.Name;
                if (documentField.Revision)
                    args.Revision = documentField.Revision;
            }

            // use DocumentEntry.Name as display name if set, otherwise Document.Name used
            if (nameField && nameField.Value)
                args.DisplayName = nameField.Value;

            if (descriptionField && descriptionField.Value)
                args.Description = descriptionField.Value;

            if (browseMode && browseMode.Value) {
                if (browseMode.Value === 1)
                    args.BrowseMode = "httpfile";
                else if (browseMode.Value === 3)
                    args.BrowseMode = "url";
                else
                    args.BrowseMode = "local";
            }
            return args;
        }
    }

    /**
     * Gets a standard DocEntry object from one with properties of Identifier, Name and Revision.
     * 
     * @param {object} doc - An object containing the document properties.
     */
    function getDocEntryfromIdentifierNameRevision(doc) {
        var iconSrc;
        if (CR.URL.isHttpUrl(doc.Identifier))
            iconSrc = 'Themes/Horizon/Images/icons/HTML_32_32.svg'; //TODO: get a version of this into assets/image
        else
            iconSrc = _defaultIconPath + CR.Path.getFileTypeIconName(doc.Identifier);
        let docName = doc.Name ? encodeURIComponent(doc.Name) : "";
        let docRevision = doc.Revision ? encodeURIComponent(doc.Revision) : "";
        var docViewerUrl = doc.BrowseMode && (doc.BrowseMode.toLowerCase() === "url" || doc.BrowseMode.toLowerCase() === "httpfile") ? doc.Identifier : CR.URL.getDocViewerUrl(doc.Identifier, docName, docRevision);
        var displayName = doc.DisplayName ? doc.DisplayName : doc.Name;
        var openInNewTab;
        var openInPopup;
        let docRev = doc.Revisison ? doc.Revision : '';
        if (CR.Path.isJTFile(doc.Identifier)) {
            openInPopup = function () {
                console.log('open JT file: ' + doc.Identifier);
                CR.Page.openPopup(CR.URL.getJTViewerUrl(doc.Name, docRev), '3D Model Popup', false);
            };
        } else {
            openInNewTab = function () {
                console.log('open in new tab: ' + doc.Identifier);
                window.open(docViewerUrl, 'Document Viewer - ' + doc.Identifier);
                let event1 = arguments[0] || window.event;
                CR.Event.stopEvent(event1);
            };
            if (docViewerUrl.indexOf(window.location.origin) >= 0) {
                openInPopup = function () {
                    console.log('open in popup: ' + doc.Identifier);
                    CR.Page.openPopup(docViewerUrl, 'Document Popup', false);
                };
            }
        }

        return new DocEntry(displayName, iconSrc, openInPopup, openInNewTab, doc.Description);
    }

    /**
     * Sets the default icon path to use if an icon is specified without a path;
     * Adds an ending "/" if missing.
     * If not set "assets/image/" is used
     * 
     * @param {string} path - Icon path to use as a default.
     */
    function setDefaultIconPath(path) {
        if (path.lastIndexOf('/') !== path.length - 1)
            path += '/';

        _defaultIconPath = path;
    }

    /**
     * Creates an instance of a document collection
     */
    function DocCollection() {
        this.sets = [];
        this.entries = [];
    }

    DocCollection.prototype = {
        addSet: function (nameOrSet) {
            //support adding an existing doc set object or creating a new one with the specified name.
            if (typeof nameOrSet === 'string')
                this.sets.push(new DocSet(nameOrSet));
            else
                this.sets.push(nameOrSet);
        },

        insertEntry: function (entry) {
            this.entries.unshift(entry);
        },

        addEntry: function (entry) {
            this.entries.push(entry);
        },

        clear: function () {
            this.sets = [];
            this.entries = [];
        }
    };


    /**
     * Creates an instance of a document set
     * 
     * @param {string} name - Name for document set.
     * @param {object[]} entries - Array of documents to be included in the set.
     */
    function DocSet(name, entries) {
        this.name = name;
        this.entries = Array.isArray(entries) ? entries : [];
    }

    DocSet.prototype = {
        /**
         * Add a document to the list of entries
         * 
         * @param {object|string} nameOrEntry - An existing DocEntry object or just the name to use for the document.
         * @param {string} icon - Name of icon to use for document.
         * @param {function} openPopupHandler - Callback for clicking the button to open document in a popup.
         * @param {function} openTabHandler - Callback for clicking the button to open document in a new tab.
         */
        addEntry: function (nameOrEntry, icon, openPopupHandler, openTabHandler) {
            if (typeof nameOrEntry === 'string')
                this.entries.push(new DocEntry(nameOrEntry, icon, openPopupHandler, openTabHandler));
            else
                this.entries.push(nameOrEntry);
        }
    };

    /**
     * Creates an object to use in the list of document set entries.
     * 
     * @param {string} name - Name of document
     * @param {string} icon - Name of icon to use for document.
     * @param {any} openPopupHandler - Callback for clicking the button to open document in a popup.
     * @param {any} openTabHandler - Callback for clicking the button to open document in a new tab.
     */
    function DocEntry(name, icon, openPopupHandler, openTabHandler, description) {
        this.name = name;
        this.icon = icon;
        this.openPopupHandler = openPopupHandler;
        this.openTabHandler = openTabHandler;
        this.description = description;
    }

})();