// Copyright Siemens 2024


(function CR_Root() {
    'use strict';

    var CR = GetNamespace('CR');
    CR.GetNamespace = GetNamespace;

    /**
     * Create a hierarchy of objects based on the specifed namespace parameter and returns the lowest level object.
     * The highest level object is placed on the global object (window).
     * At each level, an object will be created if it does not already exist.
     * 
     * e.g. the namespacew "fe.fi.fo" would create 
     *      window.fe = { fi: { fo: { } } }
     *      and return the object "fo".
     * 
     * @param {string} namespace - Represents namespace hierarchy to be created.
     */
    function GetNamespace(namespace) {

        var obj = window;
        var i = 0;
        var name = null;
        var nameArray = namespace.split('.');

        for (i = 0; i < nameArray.length; i++) {
            name = nameArray[i];
            obj[name] = obj[name] || {};
            obj = obj[name];
        }

        return obj;
    }

})();

/*
 * Utility functions related to paths and files
 */
(function CR_Path() {
    'use strict';

    var api = CR.GetNamespace('CR.Path');

    api.getFileExtension = getFileExtension;
    api.getFileTypeIconName = getFileTypeIconName;
    api.isJTFile = isJTFile;
    api.isPath = isPath;

    /**
     * Gets the name of a icon to use for file based on the files extension.
     * 
     * @param {string} file - Can be name or full path. Basically any string ending with ".xxx".
     */
    function getFileTypeIconName(file) {

        var fileTypeIconName;

        switch (getFileExtension(file).toLowerCase()) {
            case 'jt': fileTypeIconName = 'type3D48.svg'; break;
            case 'txt': fileTypeIconName = 'typeTxt48.svg'; break;
            case 'gif': fileTypeIconName = 'typeGif48.svg'; break;
            case 'jpg': fileTypeIconName = 'typeJpg48.svg'; break;  //TODO: this file not in assets/image. will work if ES installed.
            case 'png': fileTypeIconName = 'typePng48.svg'; break;
            //TODO: below image differs from previous used ES_Image-32x32.png but is more consistent with other images. verify if OK or should revert to old image?
            case 'bmp': fileTypeIconName = 'typeBMP48.svg'; break;  
            case 'doc': fileTypeIconName = 'typeComponentWord48.svg'; break;
            case 'docx': fileTypeIconName = 'typeComponentWord48.svg'; break;
            case 'xls': fileTypeIconName = 'typeMsExcel48.svg'; break;
            case 'xlsx': fileTypeIconName = 'typeMsExcel48.svg'; break;
            default: fileTypeIconName = 'cmdDocument24.svg'; break;
        }

        return fileTypeIconName;
    }

    /**
     * Returns true if extension of file is "jt".
     * 
     * @param {any} file
     */
    function isJTFile(file) {
        // ends with '.jt', 'i' indicates case insensitive
        return /\.jt$/i.test(file);
    }

    /**
     * Gets file extension of the specified file.
     * 
     * @param {string} file - File to get the name of. Can be full path
     */
    function getFileExtension(file) {
        var basename = file.split(/[\\/]/).pop();  // extract name if path. (supports `\\` and `/` separators)

        var lastDot = basename.lastIndexOf(".");
        if (basename === "" || lastDot < 1) {      // if file name is empty or `.` not found(-1) or comes first(0)
            return "";
        }

        return basename.slice(lastDot + 1);

        // interesting one liner to get file extension below, but above verbose version more clear.
        // return fname.slice((fname.lastIndexOf(".") - 1 >>> 0) + 2);
        // see explanation  here https://stackoverflow.com/questions/190852/how-can-i-get-file-extensions-with-javascript/1203361#1203361
    }

    /**
     * Checks the specified file to see if it includes a path
     * 
     * @param {string} file - File to check.
     */
    function isPath(file) {
        return file.split(/[\\/]/).length > 1;
    }

})();


/*
 * Utility functions for URLs
 */
(function CR_URL() {
    'use strict';

    var api = CR.GetNamespace('CR.URL');

    api.isHttpUrl = isHttpUrl;
    api.isNetworkFile = isNetworkFile;
    api.isWWW = isWWW;
    api.getDocViewerUrl = getDocViewerUrl;
    api.getJTViewerUrl = getJTViewerUrl;

    /**
     * Simple test if the specified string is a URL
     * 
     * @param {string} maybeUrl - The string to check.
     */
    function isHttpUrl(maybeUrl) {
        return maybeUrl.toLowerCase().indexOf('http://') !== -1 || maybeUrl.toLowerCase().indexOf('https://') !== -1;
        //TODO: this was ported from ES.Utility.isUrl. The check seems too simple to warrant that general name so I changed it.
        //      do we need a more general isUrl check?
    }

    function isWWW(maybeUrl) {
        return maybeUrl.toLowerCase().startsWith("www.");
    }

    function isNetworkFile(maybeUrl) {
        return maybeUrl.toLowerCase().startsWith("\\");
    }
    /**
     * Get a URL to view a document in the standard DocumentViewer page.
     * 
     * @param {string} docIdentifier
     * @param {string} docName
     * @param {string} docRevision
     */
    function getDocViewerUrl(docIdentifier, docName, docRevision) {
        var loc = window.location;
        var appPath = loc.pathname.substr(0, loc.pathname.indexOf("/", 1));
        var docViewerUrl = loc.protocol + "//" + loc.host + appPath + "/DocumentViewer.html";

        // First check if this document is a URL
        if (docIdentifier && isHttpUrl(docIdentifier)) {
            //  Since this is a URL, just set the URL so the browser will open the URL directly
            docViewerUrl = docIdentifier;
        } else if (docIdentifier && isWWW(docIdentifier)) {
            docViewerUrl = loc.protocol + "//" + docIdentifier;
        }
        else {
            docViewerUrl = docViewerUrl + "?name=" + docName;
            if (docRevision) {
                docViewerUrl = docViewerUrl +
                    "&revision=" +
                    docRevision;
            }
        }

        return docViewerUrl;
    }

    // Get URL for viewer for a JT file
    /**
     * Get a URL to view a JT document
     * 
     * @param {string} docName
     * @param {string} docRev
     */
    function getJTViewerUrl(docName, docRev) {
        docRev = docRev ? docRev : '';
        return location.href.substr(0, location.href.lastIndexOf("/")) +
            "/ModelViewerPopup.aspx?IsFloatingFrame=2&name=" + encodeURIComponent(docName) + "&revision=" + encodeURIComponent(docRev);
    }

    // test using anchor tag as way to get location info from a string url.
    //function testUrl(url) {
    //    var a = document.createElement('a');
    //    a.href = url;
    //    var href = a.href;
    //    console.log('testUrl - url: ' + url + ' turns into href: ' + href);

    //    var host = a.host;
    //    var hostName = a.hostname;
    //    var origin = a.origin;
    //    var search = a.search;
    //    var pathname = a.pathname;
    //    var port = a.port;
    //    var protocol = a.protocol;

    //    debugger;
    //    return a;
    //}


})();

(function CR_Page() {
    'use strict'

    var api = CR.GetNamespace('CR.Page');

    api.formatString = formatString;
    api.getLabelCache = getLabelCache;
    api.getLabelValue = getLabelValue;
    api.getLabelValueAsync = getLabelValueAsync;
    api.isInEProcedurePage = isInEProcedurePage;
    api.isMobile = isMobile;
    api.isResponsive = isResponsive;
    api.localizeLabels = localizeLabels;
    api.localizePage = localizePage;
    api.openPopup = openPopup;
    api.setLabelsFromDictionary = setLabelsFromDictionary;
    api.setCheckBoxChecked = setCheckBoxChecked;
    api.keepSessionAlive = keepSessionAlive;
    api.startKeepSessionAlive = startKeepSessionAlive;
    api.getQueryVariable = getQueryVariable;
    api.extractQueryString = extractQueryString;
	api.getElement = getElement;
    api.blurPickList = blurPickList;

    var _labels = {};
    var _keepAliveTimer = null;

    /**
     * Will trigger a postback change on a picklist
     *
     * @param {object} event
     */
    function blurPickList() {
        if (event) {
            var key = (event.keyCode) ? event.keyCode : event.which;
            if (key != null) {
                key = parseInt(key, 10);
                if (key == 13) {
                    document.body.focus();
                    window.event.srcElement.blur();
                    window.event.srcElement.focus();
                }
            }
        }
    }

    /**
     * Finds an element in the page or in a parent document
     * 
     * @param {string} idOrSelector
     */
    function getElement(idOrSelector) {
        // if it's not a selector, assume it's an ID an prepend a '#'
        var selector = (idOrSelector[0] === '#' || idOrSelector[0] === '.') ? idOrSelector : '#' + idOrSelector;

        // return value
        var $elems = $(selector);

        // if not found, check parent
        if ($elems.length === 0 && parent)
            $elems = $(selector, parent.document);

        // if not found, check first iframe in document (?)
        if ($elems.length === 0) {
            var iframe = $($(document).find('iframe')[0]);
            if (iframe.length === 1)
                $elems = iframe.contents().find(selector);
        }
        return $elems;
    }

	
    /**
     * Checks for a query string variable and returns the value
     * 
     * @param {string} variable
     */
    function getQueryVariable(variable) {
        var query = window.location.search.substring(1);
        var vars = query.split('&');
        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split('=');
            if (decodeURIComponent(pair[0]) == variable) {
                return decodeURIComponent(pair[1]);
            }
        }
		return "";
    }

    /**
     * Checks for a query string variable and returns the value
     * 
     * @param {string} variable
     */
    function extractQueryString(variable) {
        var query = window.location.search.substring(1);
        let idx = query.indexOf(variable);
        if (idx >= 0)
            return query.substring(idx + variable.length + 1);
        else
            return query;
        
    }
    /**
     * Opens the specified url in a popup window.
     * 
     * @param {string} url
     * @param {string} title - Text to show in popup header
     * @param {boolean} closeButtonOnly
     */
    function openPopup(url, title, closeButtonOnly) {
        // "element" arg is used in showAjax to check if it has childNodes and is disabled. I think purpose is to do nothing if user is clicking on a disabled button.
        // This method always opens the popup. So we just need "element" to be a non-null object without a childNodes property.
        // if this method called like "CR.Page.openPopup(...), then 'this' will be CR.Page.
        // but if called from another function in this module like openPopup(...), then 'this' will be undefined(due to using strict mode). So make sure an object here.
        var element = this ? this : CR.Page;

        pop.showAjax(url, title,
            isResponsive() ? screen.availHeight - 20 : 900,    // height
            isResponsive() ? screen.availWidth - 80 : 1300,    // width
            0,          // top
            0,          // left
            true,       // showButtons
            "",         // okButtonText
            "",         // closeButtonText
            element,    // element
            true,       // closeOnCancel
            '',         // optionArgs
            null,       // cancelConfirmMsg
            closeButtonOnly,    
            false);     // display reset
    }

    //TODO: do we really need to check this anymore? seems like should always be true.
    /**
     * Returns true if the page is using the responsive "Horizon" theme.
     */
    function isResponsive() {
        //return true;
        var page = window.__page || parent.__page || getCEP_top().__page;
        return document.body.classList.contains("Horizon-theme") || page.get_isResponsive();
    }

    /**
     * Returns true if page is hosted in a mobile device.
     */
    function isMobile() {
        let mobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(window.navigator.userAgent);
        if (!mobile)
            mobile = getPage().isMobilePage();
        return mobile;
    }

    /**
     * Returns true if a page is being hosted in the EProcedure VP.
     */
    function isInEProcedurePage() {
        return $("form", window.parent.document).is('[action*="EProcedure"]');
    }

    /**
     * Gets the __page object that is the single instance of Camstar.WebPortal.PortalFramework.WebPartPageBase.
     */
    function getPage() {
        return window.__page || parent.__page || getCEP_top().__page;
    }

    /**
     * Localizes page elements that have 'labelid' attribute set.
     * 
     * @param {function} [successCallback] - Optional function to call after successful page update.
     * @param {object} [$localizeThis] - Optional jQuery object defining the parent of all elements to be localized. If not set, entire page is updated.
     */
    function localizePage(successCallback, $localizeThis) {
        var $allLabels = $localizeThis ? $localizeThis.find('[labelid]') : $('[labelid]');

        // ensure unique
        var obj = {};
        $allLabels.each(function () {
            var id = $(this).attr('labelid');
            if (id && !obj.hasOwnProperty(id))
                obj[id] = id;
        });

        // then turn unique list into form needed to get values
        var labels = Object.keys(obj).map(function (key) {
            return { Name: key };
        });

        var page = getPage();
        if (labels.length > 0 && page) {
            page.getLabels(labels, function (result) {
                if (Array.isArray(result)) {
                    cacheLabels(result);
                    updatePageWithLabelValues($localizeThis);
                    if (successCallback)
                        successCallback();
                } else {
                    console.error(result.Error);
                }
            });
        } else {
            successCallback();
        }
    }

    /**
     * Gets localized values for the specified list of label names and optionally updates page elements.
     *
     * @param {string[]} labelNames - Array of label names
     * @param {function} [callback] - function to call if update successful
     * @param {boolean} [updatePage] - Flag to update page elements with 'labelid' attributes matching the specified label names.
     */
    function localizeLabels(labelNames, callback, updatePage) {
        var labels = labelNames.map(function (labelName) { return { Name: labelName }; });
        var page = getPage();
        if (page) {
            page.getLabels(labels, function (result) {
                if (Array.isArray(result)) {
                    cacheLabels(result);
                    if (updatePage)
                        updatePageWithLabelValues();
                    if (callback)
                        callback(result);
                } else {
                    console.error(result.Error);
                }
            });
        } else if (callback)
            callback();
    }

    /**
     * Adds the list of localized labels to the cache
     * 
     * @param {object[]} labels - Array of objects with the form { Name: <label name>, Value: <label value> }.
     */
    function cacheLabels(labels) {
        if (Array.isArray(labels)) {
            labels.forEach(function (label) {
                //TODO: load dictionaries and test to determine correct way to handle case of having a DefaultValue, but not a Value.
                var labelValue = isNonEmptyString(label.Value) ? label.Value : isNonEmptyString(label.DefaultValue) ? label.DefaultValue : label.Name;
                _labels[label.Name] = labelValue;
            });
        }
    }

    function isNonEmptyString(str) {
        return typeof str === 'string' && str !== '';
    }

    /**
     * Sets the appropriate properties and attributes for a radio button/checkbox 
     */
    function setCheckBoxChecked(spanId, checked) {
        // Classic requires the nested <input> to also be marked checked
        var $checkElems = $(formatString("#{0}, #{0} > input", spanId));

        // fixed View Mode radio button option, replace attr with prop
        if (checked) {
            $checkElems.prop('checked', true);
            $checkElems.attr('checked', 'checked');
        } else {
            $checkElems.prop('checked', false);
            $checkElems.removeAttr('checked');
        }
    }

    function setLabelsFromDictionary(dictionary) {
        if (dictionary) {
            Object.keys(dictionary).forEach(function (key) {
                _labels[key] = dictionary[key];
            });
        }
    }

    /**
     * Updates display text of page elements that have 'labelid' attribute set using the cached label values.
     * 
     * @param {any} [$localizeThis] - Optional jQuery object defining the parent of all elements to be localized. If not set, entire page is updated.
     */
    function updatePageWithLabelValues($localizeThis) {
        var $localizeObjs = $localizeThis ? $localizeThis.find('[labelid]') : $('[labelid]');
        var labelName, labelValue, control;
        
        $localizeObjs.each(function () {
            labelName = $(this).attr('labelid');
            labelValue = getLabelValue(labelName);
            control = $(this)[0];

            if (control.tagName === "INPUT" && control.type === 'button') {
                control.value = labelValue;
            } else if (control.children.length === 0) {
                control.innerText = (control.innerText.indexOf(":") >= 0) ? labelValue + ":" : labelValue;  //TODO: why considering ":"? was in ES_Utility script.
            }

            if (labelValue !== labelName) {
                //TODO: bug in ES script would never do this, but was intended. check if any problems now due to actually removing the attribute.
                //      I believe the idea was to allow calling localizePage more than once but only updating elements that needed. like if something previously wasn't on the page.
				if (!$(this).attr("id"))
					$(this).attr('id', labelName);  
                $(this).removeAttr('labelid');  
            }
        });
    }

    /**
     * Perform C# style string replacement.
     * i.e. formatString("The customer's name is {0}.", custName);
     */
    function formatString() {
        var s = arguments[0];

        for (var i = 0; i < arguments.length - 1; i++) {
            var reg = new RegExp("\\{" + i + "\\}", "gm");
            s = s.replace(reg, arguments[i + 1]);
        }

        return s;
    }

    /**
     * Gets a full copy of the label cache 
     */
    function getLabelCache() {
        return $.extend({}, _labels);
        //return _labels; // if we don't return reference, the later updates won't be in the local copy.
    }

    /**
     * Gets the label value for the specified label name from label cache.
     * Use this when labels are already localized and cached.
     * If label not yet localized, the label name is returned as the value.
     * 
     * @param {string} labelName - Name of label to get value for.
     */
    function getLabelValue(labelName) {
        var labelValue = _labels[labelName];
        return labelValue ? labelValue : labelName;
    }

    /**
     * Gets the label value for the specified label name and returns the result via callback function.
     * If the label value is not already in the cache, the server is contacted to get the value.
     * Use this if label may not yet be localized.
     * 
     * @param {string} labelName - Name of label to get value for.
     * @param {function} callback - Function used to pass label value back to caller.
     */
    function getLabelValueAsync(labelName, callback) {
        if (!callback) {
            return "getLabelValueAsync() - callback required";
        }

        var labelValue = getLabelValue(labelName);
        var labelLoaded = labelValue !== labelName;

        // First check to see if we already have the label
        if (labelLoaded) {
            callback(labelValue);
        } else {

            // No label found. Get it from the server
            var labels = [{ Name: labelName }];

            getPage().getLabels(labels, function (result) {
                if (Array.isArray(result)) {
                    // success
                    cacheLabels(result);
                    labelValue = result[0].Value;
                } else {
                    // fail
                    console.error(result.Error);
                }
                callback(labelValue);
            });
        }
    }
    /*
     *  Method that will call the SessionHandler to keep the ASP.NET session alive,
     *  and will also reset the value used by the warning message handler to display
     *  a message about the session expiring
     */
    function keepSessionAlive(keepAlive) {
        $('#sessionTimeoutStartTime', getCEP_top().document).val($.now());
        $.ajax({
            url: keepAlive ? "SessionHandler.ashx?refresh=1&keepalive=1" : "SessionHandler.ashx?refresh=1",
            contentType: "html"
        });
    }

    function startKeepSessionAlive(minutes, keepAlive) {
        if (_keepAliveTimer)
            clearTimeout(_keepAliveTimer);
        var timeMS = minutes * 60 * 1000;
        _keepAliveTimer = setTimeout(function () { doKeepSessionAlive(keepAlive, timeMS); }, timeMS);
    }

    function doKeepSessionAlive(keepAlive, timeMS) {
        keepSessionAlive(keepAlive);
        _keepAliveTimer = setTimeout(function () { doKeepSessionAlive(keepAlive, timeMS); }, timeMS);
    }
})();

// Functions to help with responsive functionality
//      - Resizing a given DOM element to fill remaing vertical space
(function CR_Rad () {
	var api = CR.GetNamespace('CR.Rad');

	api.addVFillContainer = addVFillContainer;
	api.fillVerticalSpaceAll = fillVerticalSpaceAll;
	api.fillParentVertical = fillParentVertical;
	
    var _verticalContainers = [];
    var _domReady = false;

    $(function () {
        _domReady = true;

        // add observers
        _verticalContainers.forEach(function (container) {
            addObserver(container);
        });

        setTimeout(fillVerticalSpaceAll, 1000);
//        fillVerticalSpaceAll();
    });

    // Add a new handler function to watch for height attribute changes
    function addObserver(container) {
        var observerConfig = {
            attributes: true,
            attributeOldValue: true,
            childList: false,
            subtree: false,
            attributeFilter: ['height']
        };

        // Just use this technique instead?
        $(window).resize(function () {
            if (CR.Page.getElement('#WebPart_TaskInfoWP_UIComponent').length === 0)
                setTimeout(fillVerticalSpaceAll, 0);
        });

        // When changing portrait/landscape the iframe's height property is updated.  Watch for changes on that property so we can resize.
        if (!!MutationObserver) {
            var observer = new MutationObserver(function (mutations, observer) { heightChanged(mutations, observer, container.callback); });
            observer.observe(window.frameElement, observerConfig);
        } else {
            console.error('MutationObserver not supported.  Make sure you are not running IE in compatibility mode.');
        }
    }

    // For each vertical container, resize it to take up any available space in the containing iFrame
    function fillVerticalSpaceAll() {

        _verticalContainers.forEach(function (container) {
            fillVerticalSpace(container);
        });
    }

    // Resize only the given container to fill available vertical space
    function fillVerticalSpace(container) {
        // Make this fill available vertical height
        var $container = $(container.selector);
        var containerHeight = $container.outerHeight(true);
        var containerOff = $container.offset().top;

        // scrollable-panel has a fixed height.  DynamicContentDiv can be pushed bigger by the amount of empty space under it

        // Mobile:
        // <div class='scrollable-panel'>...
        //    <div id='DynamicContentDiv'> ...

        // Desktop:
        // <div id='scrollablepanel') ..
        //    <div id='DynamicContentDiv'> ...
        
        var $desktopScrollablePanel = $container.closest('#scrollablepanel');
        var $mobileScrollablePanel = $container.closest('.scrollable-panel');

        var $scrollablePanel = ($desktopScrollablePanel.length > 0) ? $desktopScrollablePanel : $mobileScrollablePanel;

        var $floatingFormContainer = $container.closest('.float-form-container');

        if ($scrollablePanel.length === 0) {
            console.error('Could not find scrollable panel!');
            return;
        }

        var taskWebPart = CR.Page.getElement('#WebPart_TaskInfoWP_UIComponent');
        var taskHeight = 0;
        var emptyHeight = 0;
        if (taskWebPart.length)
            taskHeight = taskWebPart.outerHeight(true) - taskWebPart.offset().top;
        var $dynamicContentDiv = $('#DynamicContentDiv');
        var panelHeight = $scrollablePanel.outerHeight();
        var dynamicHeight = $dynamicContentDiv.outerHeight(true);
        var off = $dynamicContentDiv.offset().top;
        if (CR.Page.isResponsive()) {
            //  This is great when really in a mobile device-  but messes up when in a desktop that still detects this panel
            //if ($mobileScrollablePanel.length)
            //    emptyHeight = $dynamicContentDiv.outerHeight(true) - $dynamicContentDiv.offset().top;
            //else
            //emptyHeight = panelHeight - (dynamicHeight + off);
            if (panelHeight > dynamicHeight)
                emptyHeight = panelHeight - (dynamicHeight + off);
            else
                emptyHeight = dynamicHeight - off - containerHeight;

            if (taskHeight > 0)
                emptyHeight = taskHeight - (containerOff - containerHeight);
            else if (emptyHeight === 0 && CR.Page.isMobile())// $mobileScrollablePanel.length)
                emptyHeight = dynamicHeight - off - containerHeight;                
        } else {
            // This "static zone" is empty in desktop, but still taking up some space.  Also assuming this will go away...
            $('div#StaticZoneBottom_Content').hide();

            var $outerTable = $('#TemplateContentDiv > table');
            if ($outerTable.length !== 0) {
                // Argh, floating frame containing table height is off by 1 px
                let containerHeight = ($floatingFormContainer.length > 0) ? $floatingFormContainer.outerHeight() - 1 : $scrollablePanel.outerHeight();
                emptyHeight = containerHeight - ($outerTable.outerHeight(true) + $outerTable.offset().top);
            }
        }

        if ($floatingFormContainer.length !== 0 && taskHeight > 0) {
            $container.height(containerHeight + emptyHeight);
        } else {
            $container.outerHeight(containerHeight + emptyHeight, true);
        }

        if (container.callback) {
            container.callback();
        }
    }

    // Mutation observer handler
    function heightChanged(mutations, observer, callback) {
        var heightAttrChanged = false;
        mutations.forEach(function (mutation) {
            // This check shouldn't be necessary - due to attributeFilter set below
            if (mutation.type === "attributes" && mutation.attributeName === "height") {
                //console.log("mutation obs");
                heightAttrChanged = true;
            }
        });

        if (heightAttrChanged) {
            setTimeout(fillVerticalSpaceAll, 0);
        }
    }

    // Make the given element fill up remaining vertical space and call the callback after resized
    function addVFillContainer(selector, callback) {
        var newContainer = {
            selector: selector,
            callback: callback
        };

        _verticalContainers.push(newContainer);

        if (_domReady) {
            // Not sure if this use case is needed
            console.warn("If possible, call addVFillContainer() before DOM is ready.");

            // Resize the container NOW and call the callback.  
            addObserver(newContainer);
            fillVerticalSpace(newContainer);
        }
    }

    //---------------------------------------------------------------------------
    // Take the elem with the given ID and make it fill all available height
    // in the parent
    function fillParentVertical(fillElemId, accountForSiblings) {
        if (typeof accountForSiblings === 'undefined') {
            accountForSiblings = true;
        }

        var $fillElem = $('#' + fillElemId);
        var $parent = $fillElem.parent();

        var parentPosition = $parent.css('position');
        if (parentPosition !== 'relative' && parentPosition !== 'absolute') {
            console.warn('Parent element ' + $parent.get(0).id + ' position is ' + parentPosition + '.  Should be relative or absolute.');

            return;
        }

        var $lowestElem = $fillElem;
        // if there is a lower sibling, do we offset from that?
        if (accountForSiblings) {
            let $lastSib = $parent.children().last();
            $lowestElem = $lastSib.offset().top > $fillElem.offset().top ? $lastSib : $fillElem;
        }

        // $lowestElem.position.top will be the offset from the parent's top padding
        // We want offset from the content box
        var lowestElemTop = $lowestElem.position().top - parseInt($parent.css('padding-top'), 10);

        // fillElem height set to all the leftover height in the parent
        var unusedHeight = $parent.height() - (lowestElemTop + $lowestElem.outerHeight(true));
        $fillElem.outerHeight($fillElem.outerHeight(true) + unusedHeight, true);
    }
})();

(function CR_PBIReports() {
    'use strict'

    var api = CR.GetNamespace('CR.PBIReports');

    api.initialize = initialize;
	
	function initialize() {
		let id = '#ctl00_WebPartManager_ReportsWP_IFrame_Reports';
		CR.Rad.addVFillContainer('#ctl00_WebPartManager_ReportsWP_IFrame_Reports');
        
		let iFrame = $(id).children("iframe").eq(0);
		let src = iFrame.attr('src');
		if (src) {
			if (!src.endsWith("/"))
                src += "/";
            var report = CR.Page.extractQueryString("Report");
            if (report.toLowerCase().startsWith("rdlembed")) {
                src = src + "rdlEmbed?" + report.substring(8);
            }
            else if (report.toLowerCase().startsWith("reportid")) {
                src = src + "reportEmbed?" + report;            
            } else if (report.toLowerCase().startsWith("groups") || report.indexOf("rdlreports")) {
                src = src + report + "?rs:embed=true&experience=power-bi";
            }
            else if (src.toLowerCase().endsWith("reports/")) {
                if (report) {
                    src = src + report + "?rs:embed=true";
                } else {
                    src = src + "browse/?rs:embed=true";
                }
            } else if (report)
                src = src + report;

			iFrame.attr('src', src);
		}
		doResize();
		$(window).resize(function () {            
			setTimeout(doResize, 0);
        });
	}
	
	function doResize() {
		let elem = $('#WebPart_ReportsWP');
		elem.children().first().width(elem.width() - 16);
	}
})();

(function CR_Event() {
    'use strict'

    var api = CR.GetNamespace('CR.Event');

    api.isEnterKey = isEnterKey;
    api.isShiftTabKey = isShiftTabKey;
    api.isTabOrEnterKey = isTabOrEnterKey;
    api.stopEvent = stopEvent;
    api.sendEnterKeyDown = sendEnterKeyDown;

    /**
     * Stop propagation of an event.
     * 
     * @param {object} ev - Event object to stop.
     */
    function stopEvent(ev) {
        ev.preventDefault();
        if (ev.stopPropagation)
            ev.stopPropagation();
        else
            ev.cancelBubble = true;
    }

    /**
     * Returns true if event is for "Enter" key press.
     * 
     * @param {object} ev - Event object to check.
     */
    function isEnterKey(ev) {
        var isEnterKey = false;
        var enterKeyValue = 13;

        if (ev.which || ev.keyCode)
            isEnterKey = ev.which === enterKeyValue || ev.keyCode === enterKeyValue;

        return isEnterKey;
    }

    /**
     * Returns true if event is for "Tab" or "Enter" or "End of Transmission" key press.
     * 
     * @param {object} ev - Event object to check.
     */
    function isTabOrEnterKey(ev) {
        return ((ev.which || ev.keyCode)
            && (ev.which === 13) || (ev.keyCode === 13) || (ev.which === 9) || (ev.keyCode === 9) || (ev.which === 4) || (ev.keyCode === 4));
    }

    /**
     * Returns true if event is for "Shift" and "Tab" key press.
     * 
     * @param {object} ev - Event object to check.
     */
    function isShiftTabKey(ev) {
        var isShiftTab = false;
        if (ev.which || ev.keyCode)
            if ((ev.which === 9 || ev.keyCode === 9) && ev.shiftKey)
                isShiftTab = true;

        return isShiftTab;
    }

    /**
     * Sends an "Enter" keydown event to the specified element
     * 
     * @param {string} id - ID of the element to send the event to
     */
    function sendEnterKeyDown(id) {
        const event = new KeyboardEvent('keydown', {
            key: 'Enter',
            code: 'Enter',
            which: 13,
            keyCode: 13,
        });
        document.getElementById(id).dispatchEvent(event);
    }

})();

/*
 * Utility functions for grid table
 */
(function CR_Grid() {
    'use strict'

    var api = CR.GetNamespace('CR.Grid');

    api.makeScrollable = makeScrollable;
    api.resizeCols = resizeCols;
    api.fillParentVertical = fillParentVertical;


    //---------------------------------------------------------------------------
    function makeScrollable(tableId, fitToElemId, fitToWidth, resizedCallback) {
        var $fitToElem = $('#' + fitToElemId);
        var $table = $('#' + tableId);

        if ($fitToElem.length === 0 || $table.length === 0)
            return;

        if (typeof fitToWidth === 'undefined') {
            fitToWidth = true;
        }

        convertToScrollable(tableId, fitToElemId, fitToWidth);

        // If adding many rows, we don't want to resize after each addition
        var resizeColsDebounced = getDebouncedFunction(function () { resizeCols(tableId, fitToElemId, fitToWidth, resizedCallback); }, 100);

        // responds to any child list changes in the tbody
        const tableMutated = function (mutations, observer) {
            mutations.forEach(function (mutation) {
                if (mutation.type === 'childList') {
                    resizeColsDebounced();
                }
            });
        };

        const tableObserver = new MutationObserver(tableMutated);

        // watch for any changes in the children of the tbody so we can resize columns
        const observerConfig = { childList: true };
        tableObserver.observe($('#' + tableId + ' > tbody')[0], observerConfig);
    }

    //---------------------------------------------------------------------------
    // Take table with the given ID and make the body scrollable
    // fitToElemId - fit the table to the dimensions of DOM element with this ID
    // fitToWidth - bool: fit the table to the width of fitToElem?
    function convertToScrollable(tableId, fitToElemId, fitToWidth) {
        var $fitToElem = $('#' + fitToElemId);
        var $table = $('#' + tableId);

        if ($fitToElem.length === 0 || $table.length === 0)
            return;

        var $tbody = $('#' + tableId + ' > tbody');
        var $thead = $('#' + tableId + ' > thead');

        // Find index of last visble column
        let lastVisibleColIndex = -1;
        $tbody.find('tr').first().find('td').each(function () {
            var $td = $(this);
            if ($td.is(':visible') && $td.outerWidth(true) > 0) {
                lastVisibleColIndex = $td.index();
            }
        });

        // Loop through the <td>s in the first body row
        let $bodyRows = $('#' + tableId + ' > tbody > tr');
        $bodyRows.first().find('td').each(function () {
            var $td = $(this);

            var colWidthPx = getColWidth($td) + 'px';

            // let the last visible column take up remaining width
            if ($td.index() !== lastVisibleColIndex) {
                // Set exact width as a CSS prop
                $td.css('min-width', colWidthPx)
                    .css('max-width', colWidthPx)
                    .css('width', colWidthPx);
            }
        });

        // Now that columns have a set width, we can change display of tbody to block (adds scroll bar)
        // Can't set header column widths if there's no body column widths
        if ($bodyRows.length > 0) {
            $thead.addClass('header-scrollable');
        }

        if (fitToWidth) {
            var fitToElemWidth = $fitToElem.width();//[0].offsetWidth - ($table.outerWidth(true) - $table.width());
            $tbody.outerWidth(fitToElemWidth);
            $thead.outerWidth(fitToElemWidth);
        }

        if ($bodyRows.length > 0) {
            $tbody.addClass('body-scrollable');
        }

        // col widths are set, horizontal scroll will be present or not.  Now we can set table body height
        $tbody.css('max-height',
            visibleHeightNoScroll($fitToElem)//.height()
            - $thead.outerHeight(true)                          // account for head height
            - ($table.outerHeight(true) - $table.height())      // margin/padding/border on the table
            - ($tbody.outerHeight(true) - $tbody.height())      // margin/padding/border on the tbody
            + 'px');

        var colWidthSum = 0;

        // The scroll bar may have changed col widths.  now we can set related header widths
        $tbody.find('tr').first().find('td').each(function () {
            var $td = $(this);
            var $th = $thead.find('th').eq($td.index());
            var colWidth = getColWidth($td);

            if ($td.index() === lastVisibleColIndex) {
                // last column - make it take up the remaining space (table may or may not have scroll bar)
                colWidth = $tbody.width() - colWidthSum;
                // clientWidth does NOT include scroll bar
                var widthNoScroll = $tbody[0].clientWidth;

                if (window.document.documentMode) {
                    // IE 11 specific
                    colWidth -= 1;
                    widthNoScroll -= 1;
                }

                $td.css('min-width', widthNoScroll - colWidthSum + 'px')
                    .css('max-width', widthNoScroll - colWidthSum + 'px');

                // table wider than container?  Can happen when browser renders columns wider than the set width
                let tableWiderThanFitTo = $table.outerWidth(true) - $fitToElem.width();
                if (tableWiderThanFitTo > 0) {
                    // reduce <td> width so table fits
                    $td.css('min-width', widthNoScroll - colWidthSum - tableWiderThanFitTo + 'px')
                        .css('max-width', widthNoScroll - colWidthSum - tableWiderThanFitTo + 'px');
                    colWidth -= tableWiderThanFitTo;
                }
            } else {
                colWidthSum += colWidth;
            }

            var colWidthPx = colWidth + 'px';

            // if is last column, need to re-adjust thead column also
            if ($td.index() === lastVisibleColIndex) {
                var colBodyWidth = $tbody.width() - colWidthSum;
                var verticalScrollWidth = $tbody[0].offsetWidth - $tbody[0].clientWidth;
                let tdWidth = colBodyWidth - verticalScrollWidth + 'px';

                // if thead column is larger than tbody column, tbody column will use thead column width
                if (colBodyWidth > colWidth) {
                    $td.outerWidth(tdWidth)
                        .css('min-width', tdWidth)
                        .css('max-width', tdWidth);
                }

                // since vertical scroll bar will take up some space in tbody, thead need to add the space to make both width same
                $th.css('width', colBodyWidth + verticalScrollWidth + 'px');

            } else {
                $th.outerWidth(colWidthPx)
                    .css('min-width', colWidthPx)
                    .css('max-width', colWidthPx);
            }

        });

        $table.addClass('table-scrollable');

        // :visible is true for any element that takes up space.  No space === 0 width
        function getColWidth($td) {
            return $td.is(':visible') ? $td.outerWidth(false) : 0;
        }
    }

    // Get the visible height of the given element WITHOUT any scroll bar
    function visibleHeightNoScroll($elem) {
        let height =
            $elem[0].clientHeight                           // clientHeight includes padding...
            - parseInt($elem.css('padding-top'), 10)        // ... so we have to subtract it
            - parseInt($elem.css('padding-bottom'), 10)     // could be like "10px" so use parseInt to get just the integer
            ;

        return height;
    }

    // Return a function that can be called no more often than the given wait time
    function getDebouncedFunction(func, waitMS) {
        var timeout;
        return function () {
            var context = this;
            var args = arguments;

            var later = function () {
                timeout = null;
                func.apply(context, args);
            };

            clearTimeout(timeout);
            timeout = setTimeout(later, waitMS);
        };
    }

    //---------------------------------------------------------------------------
    function resizeCols(tableId, fitToElemId, fitToWidth, resizedCallback) {
        if (typeof fitToWidth === 'undefined') {
            fitToWidth = true;
        }

        // revert to letting the browser figure out best column width
        revertScrollable(tableId);

        if (!hasBodyRows(tableId)) {
            return;
        }

        // set column widths explicitly
        convertToScrollable(tableId, fitToElemId, fitToWidth);

        if (resizedCallback) {
            resizedCallback();
        }
    }

    //---------------------------------------------------------------------------
    // Remove any styling we added to make the table body scrollable
    function revertScrollable(tableId) {
        $('#' + tableId + ' > thead').removeClass('header-scrollable').css('width', '');
        $('#' + tableId + ' > tbody').removeClass('body-scrollable').css('width', '').css('max-height', '');
        $('#' + tableId).removeClass('table-scrollable');

        $('#' + tableId + ' > tbody > tr').first().find('td').each(function () {
            var $td = $(this);
            $td.css('width', '')
                .css('max-width', '')
                .css('min-width', '');
        });

        $('#' + tableId + ' > thead th').each(function () {
            $(this).css('width', '')
                .css('max-width', '')
                .css('min-width', '');
        });
    }

    // Table with the given ID has any body rows?
    function hasBodyRows(tableId) {
        return $('#' + tableId + ' tbody > tr').length > 0;
    }

    //---------------------------------------------------------------------------
    // Take the elem with the given ID and make it fill all available height
    // in the parent
    function fillParentVertical(fillElemId, accountForSiblings) {
        if (typeof accountForSiblings === 'undefined') {
            accountForSiblings = true;
        }

        var $fillElem = $('#' + fillElemId);
        var $parent = $fillElem.parent();
        if ($fillElem.length === 0 || $parent.length === 0)
            return;

        var parentPosition = $parent.css('position');
        if (parentPosition !== 'relative' && parentPosition !== 'absolute') {
            console.warn('Parent element ' + $parent.get(0).id + ' position is ' + parentPosition + '.  Should be relative or absolute.');

            return;
        }

        var $lowestElem = $fillElem;
        // if there is a lower sibling, do we offset from that?
        if (accountForSiblings) {
            let $lastSib = $parent.children().last();
            $lowestElem = $lastSib.offset().top > $fillElem.offset().top ? $lastSib : $fillElem;
        }

        // $lowestElem.position.top will be the offset from the parent's top padding
        // We want offset from the content box
        var lowestElemTop = $lowestElem.position().top - parseInt($parent.css('padding-top'), 10);

        // fillElem height set to all the leftover height in the parent
        var unusedHeight = $parent.height() - (lowestElemTop + $lowestElem.outerHeight(true));
        $fillElem.outerHeight($fillElem.outerHeight(true) + unusedHeight, true);
    }

})();