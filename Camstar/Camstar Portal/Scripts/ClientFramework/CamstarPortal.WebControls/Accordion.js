// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
Type.registerNamespace('CamstarPortal.WebControls.Accordion');

CamstarPortal.WebControls.Accordion.AutoSize = function()
{
    throw Error.invalidOperation();
};
CamstarPortal.WebControls.Accordion.AutoSize.prototype = 
{
    None: 0,
    Fill: 1,
    Limit: 2
};
CamstarPortal.WebControls.Accordion.AutoSize.registerEnum("CamstarPortal.WebControls.Accordion.AutoSize", false);


CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs = function(oldIndex, selectedIndex) 
{
    CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs.initializeBase(this);

    this._oldIndex = oldIndex;
    this._selectedIndex = selectedIndex;
};
CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs.prototype = 
{
    get_oldIndex : function() {
        return this._oldIndex;
    },
    set_oldIndex : function(value) {
        this._oldIndex = value;
    },
    
    get_selectedIndex : function() {
        return this._selectedIndex;
    },
    set_selectedIndex : function(value) {
        this._selectedIndex = value;
    }
};

CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs.registerClass('CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs', Sys.CancelEventArgs);

CamstarPortal.WebControls.Accordion.GetExpandAllText = function () {
    // Get Labels for Page    
    __page.getLabel('Lbl_ExpandAll', function (response) {
        if ($.isArray(response)) {                        
            return response[0].Value;
        }
        else {
            alert(response.Error);
        }
    });
}

CamstarPortal.WebControls.Accordion.GetCollapseAllText = function () {
    __page.getLabel('Lbl_CollapseAll', function (response) {
        if ($.isArray(response)) {
            return response[0].Value;
        }
        else {
            alert(response.Error);
        }
    });
}

CamstarPortal.WebControls.Accordion.Accordion = function (element)
{    
    CamstarPortal.WebControls.Accordion.Accordion.initializeBase(this, [element]);

    this._PanelHeight = '';
    this._selectedIndex = 0;
    this._panes = [];
    this._duration = 0.25;
    this._autoSize = CamstarPortal.WebControls.Accordion.AutoSize.None;
    this._headersSize = 0;
    this._headerClickHandler = null;
    this._headerMouseOverHandler = null;
    this._headerMouseOutHandler = null;
    this._headerCssClass = '';
    this._headerSelectedCssClass = '';
    this._contentCssClass = '';
    this._resizeHandler = null;
    this._AllowExpandAll = false;
    this._ExpandedSections = null;
    this._DisplayExpandAllButton = false;
    this._HiddenFieldSelectedIndex = '';
    this._HiddenExpandedSections = null;
    this._expandedAll = false;
    this._controlId = null;
    this._expandAllText = this.GetExpandAllText;
    this._collapseAllText = this.GetCollapseAllText;    
    this._serverType = "CamstarPortal.WebControls.Accordion.Accordion, CamstarPortal.WebControls";
    this._resetPaneButton = true;
    this._isExtendedDesign = false;
    this._camstarControls = [];
};

CamstarPortal.WebControls.Accordion.Accordion.prototype = {
    initialize: function ()
    {
        CamstarPortal.WebControls.Accordion.Accordion.callBaseMethod(this, 'initialize');

        if (this.get_isExtendedDesign()) {
            $(this.get_element()).addClass("cs-accordion-extended").removeClass("accordion").attr("autosize", this._autoSize);

            this._initExtendedDesign();
            return;
        }

        this._headerClickHandler = Function.createDelegate(this, this._onHeaderClick);
        this._headerMouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._headerMouseOutHandler = Function.createDelegate(this, this._onMouseOut);


        var nodes = this.get_element().childNodes;
        var index = {};
        for (index.value = 0; index.value < nodes.length; index.value++)
        {
            var header = this._getNextDiv(nodes, index);
            if (!header)
                break;

            var content = this._getNextDiv(nodes, index);
            if (content)
            {
                this.addPane(header, content);
                index.value--;
            }
        }

        if (this._AllowExpandAll && this._HiddenExpandedSections.value)
            this._ExpandedSections = this._HiddenExpandedSections.value;

        // Setup the layout for the given AutoSize mode
        this._initializeLayout();

        //this._changeSelectedIndex(this._selectedIndex, true);
        //this.raisePropertyChanged('AutoSize');

        var me = this;

        setTimeout(function () { me._initTrackValueChanges(); }, 1);

    },

    _getNextDiv: function (nodes, index)
    {
        var div = null;
        while (index.value < nodes.length && (div = nodes[index.value++]))
        {
            if (div.tagName && (div.tagName.toLowerCase() === 'div'))
            {
                break;
            }
        }
        return div;
    },

    addPane: function (header, content)
    {
        // Create the new pane object
        var pane = {};
        pane.animation = null;

        // Initialize the header
        pane.header = header;
        header._index = this._panes.length;
        $addHandler(header, "click", this._headerClickHandler);
        $addHandler(header, "mouseover", this._headerMouseOverHandler);
        $addHandler(header, "mouseout", this._headerMouseOutHandler);

        // Wrap the content in a new element
        var accordion = this.get_element();
        var wrapper = document.createElement('div');
        accordion.insertBefore(wrapper, content);
        wrapper.appendChild(content);
        wrapper._original = content;
        pane.content = wrapper;    
        wrapper.style.border = '';
        wrapper.style.margin = '';
        wrapper.style.padding = '';

        // Add the new pane at the bottom of the accordion
        Array.add(this._panes, pane);

        this._initializePane(header._index);

        $(content).css('display', 'block');

        return pane;
    },

    _initializeLayout: function ()
    {
        // Cache the initial size of the accordion
        var accordion = this.get_element();
        this._initialHeight = accordion.offsetHeight;
        var style = accordion.style;


        // Initialize the accordion itself
        if (this._autoSize === CamstarPortal.WebControls.Accordion.AutoSize.None)
        {
            // Remove the window resizing handler
            this._disposeResizeHandler();

            style.height = 'auto';
            style.overflow = 'auto';
        } 
        else
        {
            // Add the window's resizing handler
            this._addResizeHandler();

            style.height = accordion.offsetHeight + 'px';
            style.overflow = 'hidden';
        }

        // Setup the layout attributes for the pane so that it will be in a proper opened
        // or closed state
        for (var i = 0; i < this._panes.length; i++)
        {
            this._initializePane(i);
        }

        // Resize the selected pane so (depending on the AutoSize mode) it will fill the
        // available remaining space after the headers have been laid out.
        this._resizeSelectedPane();
        if (this._AllowExpandAll && this._ExpandedSections != null)
            this.restoreExpandedPanels();
    },

    _initializePane: function(index)
    {
        var pane = this.get_Pane(index);
        if (!pane)
            return;

        if (index == 0 && this._DisplayExpandAllButton) 
        {
            var header = pane.header;
            var imageContainer = $(header).children('div.headerImageContainer');
            if ($(imageContainer).children('#expandAllButton').length == 0)
            {
                var me = this;
                $('<span/>').click(function (e)
                    {
                        me._expandAll(this, e);
                    })
                    .attr({ id: 'expandAllButton' })
                    .html(this._expandAllText).addClass("expandAllButton")
                    .insertBefore($(imageContainer).children('span.arrow'));
            }
        }

        var wrapper = pane.content;
        var original = wrapper._original;

        var opened = (index === this._selectedIndex);
        wrapper.style.height = (opened || (this._autoSize === CamstarPortal.WebControls.Accordion.AutoSize.Fill)) ? 'auto' : '0px';
        $(wrapper).css('overflow', opened ? 'auto' : 'hidden');
        $(wrapper).css('display', opened ? 'block' : 'none');
        original.style.height = 'auto';
        original.style.maxHeight = '';
        original.style.overflow = opened ? 'auto' : 'hidden';

        pane.controls = this._getPaneControls(index);
        var me = this;

        if (!this._resetPaneButton) {
            $(".reset-pane-btn", pane.header).hide();
        }
        else {
            $(".reset-pane-btn", pane.header)
                .show()
                .click(function () { me._resetPane(pane); return false; });
        }
    },

    _getPaneControls: function (paneIndex) {
        var ctls = [];

        this._camstarControls
            .filter(function (c1) { return c1.paneInd == paneIndex; })
            .forEach(function (c) {
                ctls.push({ id: c.ctlId, ctl: null, pIndex: paneIndex });
            });
        return ctls;
    },

    _addResizeHandler: function ()
    {
        if (!this._resizeHandler)
        {
            this._resizeHandler = Function.createDelegate(this, this._resizeSelectedPane);
            $addHandler(window, "resize", this._resizeHandler);
        }
    },
    
    _expandAll: function (obj, event)
    {
        for (var i = 0; i < this._panes.length; i++)
        {
            var currentPane = this.get_Pane(i);
            //This sets the selected header CSS class if available.
            if (currentPane)
            {
                var content = $(currentPane.content);
                if (this._expandedAll) 
                {
                    if (this._AllowExpandAll && content.css('display') == "block") // collapse panel.
                    {
                        this._startPaneChange(currentPane, false, false, i);
                        $(currentPane.header).removeClass(this._headerSelectedCssClass);
                        this.raiseSelectedIndexChanged(new CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs(i, i));
                    }
                } 
                else 
                {
                    if (this._AllowExpandAll && content.css('display') == "none") // expand panel.
                    {
                        this._startPaneChange(currentPane, true, false, i);
                        $(currentPane.header).addClass(this._headerSelectedCssClass);
                        this.raiseSelectedIndexChanged(new CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs(i, i));
                        this._resizeSelectedPane();
                    }
                }
            }
        }
        if (this._expandedAll) 
            $(obj).html(this._expandAllText);
        else
            $(obj).html(this._collapseAllText);
        this._expandedAll = !this._expandedAll;
        
        event.cancelBubble = true; //IE
        if (event.stopPropagation) 
            event.stopPropagation(); //other browsers
    },

    dispose: function ()
    {
        // Remove the window resizing handler
        this._disposeResizeHandler();

        // Wipe the _panes collection.  We're careful to wipe any expando properties
        // which could cause memory leaks in IE6.
        if (this._panes) {
            for (var i = this._panes.length - 1; i >= 0; i--) {
                var pane = this._panes[i];
                if (pane) {
                    if (pane.header) {
                        pane.header._index = null;
                        if (!this._isExtendedDesign)
                            $removeHandler(pane.header, "click", this._headerClickHandler);
                        pane.header = null;
                    }
                    if (pane.content) {
                        pane.content._original = null;
                        pane.content = null;
                    }
                    this._panes[i] = null;
                    delete this._panes[i];
                }
            }
        }
        this._panes = null;
        this._headerClickHandler = null;
        this._controlId = null;
        this._ExpandedSections = null;
        this._HiddenExpandedSections = null;

        if (this._camstarControls) {
            var ctls = [];
            this._camstarControls.forEach(function (c) {
                if (ctls.indexOf(c.ctlId) == -1)
                    ctls.push(c.ctlId);
            });
            if (ctls.length)
                $(this.get_element()).data("controls", ctls);
        }

        CamstarPortal.WebControls.Accordion.Accordion.callBaseMethod(this, 'dispose');
    },

    _disposeResizeHandler: function ()
    {
        if (this._resizeHandler)
        {
            if (!this._isExtendedDesign)
                $removeHandler(window, "resize", this._resizeHandler);
            this._resizeHandler = null;
        }
    },

    _resizeSelectedPane: function ()
    {
        var pane = this.get_Pane();
        if (!pane)
        {
            return;
        }

        // Cache the header size so it only gets looked up when the window resizes
        this._headersSize = this._getHeadersSize().height;

        var original = pane.content._original;
        switch (this._autoSize)
        {
            case CamstarPortal.WebControls.Accordion.AutoSize.None:
                original.style.height = 'auto';
                original.style.maxHeight = '';
                break;
            case CamstarPortal.WebControls.Accordion.AutoSize.Limit:
                var remaining = this._getRemainingHeight(false);
                original.style.height = 'auto';
                original.style.maxHeight = remaining + 'px';
                break;
            case CamstarPortal.WebControls.Accordion.AutoSize.Fill:
                var remaining = this._getRemainingHeight(true);
                if (this._PanelHeight != '')
                {
                    original.style.height = this._PanelHeight;
                    original.style.overflow = 'auto';
                }
                else
                {
                    original.style.height = remaining + 'px';
                }

                original.style.maxHeight = '';
                break;
        }
    },

    _onMouseOver: function (evt)
    {
        var header = evt.target;
        if (header != null)
        {
            if (header.tagName.toLowerCase() != 'span')
                $(header).addClass("accordionHeaderSel");
            else
                $(header).parent().addClass("accordionHeaderSel");
        }
    },

    _onMouseOut: function (evt)
    {
        var header = evt.target;
        if (header != null)
        {
            if (header.tagName.toLowerCase() != 'span')
                $(header).removeClass("accordionHeaderSel");
            else
                $(header).parent().removeClass("accordionHeaderSel");
        }
    },

    _onHeaderClick: function (evt)
    {
        var header = evt.target;
        var accordion = this.get_element();
        while (header && (header.parentNode !== accordion))
        {
            header = header.parentNode;
        }

        evt.cancelBubble = true; //IE
        if (evt.stopPropagation)
            evt.stopPropagation(); //other browsers

        var index = header._index;
        this._changeSelectedIndex(index, true);
    },

    _changeSelectedIndex: function (index, animate, force)
    {
        if (this._isExtendedDesign) {
            return;
        }
        var currentPane = this.get_Pane(index);
        var lastIndex = this._selectedIndex;

        // Raise the selectedIndexChanging event but don't change the selected index
        // if the handler set the cancel property to true
        var eventArgs = new CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs(lastIndex, index);
        this.raiseSelectedIndexChanging(eventArgs);
        if (eventArgs.get_cancel())
        {
            return;
        }

        //This sets the selected header CSS class if available.
        if (currentPane)
        {
            var content = $(currentPane.content);
            var opening = false;
            if (this._AllowExpandAll && content.css('display') == "block")
            {
                opening = false;
                this._startPaneChange(currentPane, false, animate, index);
                $(currentPane.header).removeClass(this._headerSelectedCssClass);
                
            }
            else if (this._AllowExpandAll && content.css('display') == "none")
            {
                opening = true;
                this._startPaneChange(currentPane, true, animate, index);
                $(currentPane.header).addClass(this._headerSelectedCssClass);
                this._resizeSelectedPane();
            }
            
            if (this._DisplayExpandAllButton)
            {
                var allExpanded = opening;
                var allCollapsed = !opening;
                for (var i = 0; i < this._panes.length; i++)
                {
                    var pane = this._panes[i];
                    if (pane === currentPane)
                        continue;
                    
                    var paneContent = $(pane.content);
                    if (paneContent.css('display') == 'block') 
                    {
                        if (allCollapsed) 
                        {
                            allCollapsed = false;
                            break;
                        }
                        else
                            allExpanded = true;
                    }
                    else 
                    {
                        if (allExpanded) 
                        {
                            allExpanded = false;
                            break;    
                        }
                        else
                            allCollapsed = true;
                    }
                }
                if (allExpanded) 
                {
                    this._expandedAll = true;
                    $('#expandAllButton', this._panes[0].header).html(this._collapseAllText);
                }
                else if (allCollapsed) 
                {
                    this._expandedAll = false;
                    $('#expandAllButton', this._panes[0].header).html(this._expandAllText);
                }
            }
        }

        this._selectedIndex = index;
        this._HiddenFieldSelectedIndex.value = index;

        if (!this._AllowExpandAll)
            this._changePanes(lastIndex);
        
        this.raiseSelectedIndexChanged(new CamstarPortal.WebControls.Accordion.AccordionSelectedIndexChangeEventArgs(lastIndex, index));
        this.raisePropertyChanged('SelectedIndex');
    },

    _changePanes: function (lastIndex)
    {
        if (!this.get_isInitialized())
        {
            return;
        }

        for (var i = 0; i < this._panes.length; i++)
        {
            var pane = this._panes[i];
            var content = pane.content;

            if (i == this._selectedIndex)
            {
                $(content).css('display', 'block');
                this._startPaneChange(pane, true);
                if (!$(pane.header).hasClass(this._headerSelectedCssClass))
                    $(pane.header).addClass(this._headerSelectedCssClass);
                this._resizeSelectedPane();
            }
            else if (i == lastIndex)
            {
                $(content).css('display', 'none');
                this._startPaneChange(pane, false);
                $(pane.header).removeClass(this._headerSelectedCssClass);
            } 
            else
            {
                continue;
            }
        }
    },

    _startPaneChange: function (pane, opening, animate, paneIndex)
    {
        var wrapper = pane.content;
        var original = wrapper._original;
        var delay = 200;
        if (!animate)
            delay = 0;

        if (paneIndex)
        {
            var expandedIndexes = this._HiddenExpandedSections.value.split(';');
            var ind = $.inArray(paneIndex.toString(), expandedIndexes);
            if (ind >= 0)
            {
                if (!opening)
                    expandedIndexes.splice(ind, 1); // remove item from the array.
            } 
            else
            {
                if (opening)
                    expandedIndexes.push(paneIndex);
            }
            this._HiddenExpandedSections.value = expandedIndexes.join(";");
        }

        if (opening)
        {
            $(wrapper).css('height', 'auto');
            $(wrapper).css('overflow', 'auto');
            $(original).css('overflow', 'auto');
            $(wrapper).show(delay);
            $(wrapper).children().trigger('expanded');
        } 
        else
        {
            $(wrapper).css('overflow', 'hidden');
            $(original).css('overflow', 'hidden');
            if (this._autoSize === CamstarPortal.WebControls.Accordion.AutoSize.Limit)
            {
                wrapper.style.height = this._getTotalSize(original).height + 'px';
                original.style.maxHeight = '';
            }
            $(wrapper).hide(delay);
        }
        setTimeout(function ()
        {
        }, delay);
    },

    _getHeadersSize: function ()
    {
        var total = { width: 0, height: 0 };
        for (var i = 0; i < this._panes.length; i++)
        {
            var size = this._getTotalSize(this._panes[i].header);
            total.width = Math.max(total.width, size.width);
            total.height += size.height;
        }
        return total;
    },

    _getRemainingHeight: function (includeGutter)
    {
        var height = 0;
        var pane = this.get_Pane();

        if (this._autoSize === CamstarPortal.WebControls.Accordion.AutoSize.None)
        {
            // If the AutoSize mode is "None", then we use the size of the pane
            if (pane)
            {
                height = this._getTotalSize(pane.content._original).height;
            }
        } else
        {
            // Compute the amount of space used
            height = this._headersSize;
            if (includeGutter && pane)
            {
                height += this._getGutterSize(pane.content._original).height;
            }

            // Determine how much of the remaining space to use
            // (if AutoSize is "Fill", use the rest of the available space)
            var accordion = this.get_element();
            height = Math.max(accordion.offsetHeight - height, 0);

            // If AutoSize is "Limit", then the size of the pane should be either its
            // actual size, or the rest of the available space.
            if (pane && (this._autoSize === CamstarPortal.WebControls.Accordion.AutoSize.Limit))
            {
                var required = this._getTotalSize(pane.content._original).height;
                // Ensure we return a number greater than or equal to zero
                if (required > 0)
                {
                    height = Math.min(height, required);
                }
            }
        }

        return height;
    },

    _getTotalSize: function (element)
    {
        var size = this._getSize(element);
        var box = this._getMarginBox(element);
        size.width += box.horizontal;
        size.height += box.vertical;
        return size;
    },

    _getGutterSize: function (element)
    {
        var gutter = { width: 0, height: 0 };

        try
        {
            var box = this._getPaddingBox(element);
            gutter.width += box.horizontal;
            gutter.height += box.vertical;
        } catch (ex) { }

        try
        {
            var box = this._getBorderBox(element);
            gutter.width += box.horizontal;
            gutter.height += box.vertical;
        } catch (ex) { }

        var box = this._getMarginBox(element);
        gutter.width += box.horizontal;
        gutter.height += box.vertical;

        return gutter;
    },


    _getSize: function (element)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }
        return {
            width: element.offsetWidth,
            height: element.offsetHeight
        };
    },


    _getMargin: function (element, boxSide)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }

        var styleName = 'margin' + boxSide;
        var styleValue = this._getCurrentStyle(element, styleName);
        try { return this._parsePadding(styleValue); } catch (ex) { return 0; }
    },

    _getMarginBox: function (element)
    {

        if (!element)
        {
            throw Error.argumentNull('element');
        }
        var box = {
            top: this._getMargin(element, 'Top'),
            right: this._getMargin(element, 'Right'),
            bottom: this._getMargin(element, 'Bottom'),
            left: this._getMargin(element, 'Left')
        };
        box.horizontal = box.left + box.right;
        box.vertical = box.top + box.bottom;
        return box;
    },

    _getBorderBox: function (element)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }
        var box = {
            top: this._getBorderWidth(element, 'Top'),
            right: this._getBorderWidth(element, 'Right'),
            bottom: this._getBorderWidth(element, 'Bottom'),
            left: this._getBorderWidth(element, 'Left')
        };
        box.horizontal = box.left + box.right;
        box.vertical = box.top + box.bottom;
        return box;
    },

    _getPaddingBox: function (element)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }

        var box = {
            top: this._getPadding(element, 'Top'),
            right: this._getPadding(element, 'Right'),
            bottom: this._getPadding(element, 'Bottom'),
            left: this._getPadding(element, 'Left')
        };
        box.horizontal = box.left + box.right;
        box.vertical = box.top + box.bottom;
        return box;
    },

    _getBorderWidth: function (element, boxSide)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }

        if (!this.isBorderVisible(element, boxSide))
        {
            return 0;
        }
        var styleName = 'border' + boxSide;
        var styleValue = this._getCurrentStyle(element, styleName);
        return this._parseBorderWidth(styleValue);
    },

    _parseBorderWidth: function (borderWidth)
    {
        if (borderWidth)
        {
            var unit = this._parseUnit(borderWidth);
            return unit.size;
        }
        return 0;
    },

    _getPadding: function (element, boxSide)
    {
        if (!element)
        {
            throw Error.argumentNull('element');
        }

        var styleName = "padding" + boxSide;
        var styleValue = this._getCurrentStyle(element, styleName);
        return this._parsePadding(styleValue);
    },

    _getCurrentStyle: function (element, attribute, defaultValue)
    {

        var currentValue = null;
        if (element)
        {
            if (element.currentStyle)
            {
                currentValue = element.currentStyle[attribute];
            } else if (document.defaultView && document.defaultView.getComputedStyle)
            {
                var style = document.defaultView.getComputedStyle(element, null);
                if (style)
                {
                    currentValue = style[attribute];
                }
            }

            if (!currentValue && element.style.getPropertyValue)
            {
                currentValue = element.style.getPropertyValue(attribute);
            }
            else if (!currentValue && element.style.getAttribute)
            {
                currentValue = element.style.getAttribute(attribute);
            }
        }

        if ((!currentValue || currentValue == "" || typeof (currentValue) === 'undefined'))
        {
            if (typeof (defaultValue) != 'undefined')
            {
                currentValue = defaultValue;
            }
            else
            {
                currentValue = null;
            }
        }
        return currentValue;
    },

    _parsePadding: function (padding)
    {

        if (padding)
        {
            if (padding == 'inherit')
            {
                return 0;
            }
            var unit = this._parseUnit(padding);
            return unit.size;
        }
        return 0;
    },

    _parseUnit: function (value)
    {
        if (!value)
        {
            throw Error.argumentNull('value');
        }

        value = value.trim().toLowerCase();
        var l = value.length;
        var s = -1;
        for (var i = 0; i < l; i++)
        {
            var ch = value.substr(i, 1);
            if ((ch < '0' || ch > '9') && ch != '-' && ch != '.' && ch != ',')
            {
                break;
            }
            s = i;
        }
        if (s == -1)
        {
            throw Error.argumentNull('element');
        }
        var type;
        var size;
        if (s < (l - 1))
        {
            type = value.substring(s + 1).trim();
        } else
        {
            type = 'px';
        }
        size = parseFloat(value.substr(0, s + 1));
        if (type == 'px')
        {
            size = Math.floor(size);
        }
        return {
            size: size,
            type: type
        };
    },

    restoreExpandedPanels: function ()
    {
        var expandedIndexes = this._ExpandedSections.split(';');
        for (var i = 0; i < this._panes.length; i++)
        {
            var currentPane = this.get_Pane(i);
            if (currentPane)
            {
                var content = $(currentPane.content);
                if ($.inArray(i.toString(), expandedIndexes) >= 0)
                {
                    if (content.css('display') == "none")
                    {
                        this._startPaneChange(currentPane, true, false, i);
                        $(currentPane.header).addClass(this._headerSelectedCssClass);
                        this._resizeSelectedPane();
                    }
                }
                else
                {
                    if (content.css('display') == "block")
                    {
                        this._startPaneChange(currentPane, false, false, i);
                        $(currentPane.header).removeClass(this._headerSelectedCssClass);
                    }
                }
            }
        }
    },

    _initTrackValueChanges: function () {

        var me = this;

        $(this).on("toggleValues", function (ev, pane) {
            $(pane.header).toggleClass("has-values", pane.hasValues);
        });

        for (var i = 0; i < this._panes.length; i++) {
            var p = this.get_Pane(i);
            p.controls.forEach(function (cc) {
                if( !cc.ctl )
                    cc.ctl = $find(cc.id);

                if (!cc.ctl) {
                    cc.ctl = $("#" + cc.id);
                    if (!cc.ctl.length) {
                        console.warn('accordion._initTrackValueChanges', cc.id + ' not found');
                        return;
                    }
                    else {
                        cc.ctl = cc.ctl[0];
                    }
                }
                var $ctl = $(cc.ctl);
                $ctl.data("pane-index", i);
                if ($ctl.is("input") && $ctl.parent().hasClass("cs-date")) {
                    cc.isDate = true;
                }

                if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl.isInstanceOfType(cc.ctl) || 
                    Camstar.WebPortal.FormsFramework.WebControls.TextBox.isInstanceOfType(cc.ctl) || 
                    Camstar.WebPortal.FormsFramework.WebControls.CheckBox.isInstanceOfType(cc.ctl)
                ) {
                    $ctl.on("changed", function () {
                        me._trackValueChanges(this);
                    });
                }
                // Date
                else if (cc.isDate === true) {
                    $ctl.bind("change", function () {
                        me._trackValueChanges(this);
                    });
                }
            });
        }

    },

    _trackValueChanges: function (ctl) {
        var pi = $(ctl).data("pane-index");
        var pane = this.get_Pane(pi);
        var hasVal = false;
        if (pane) {
            pane.controls.forEach(function (c) {
                var val;
                if (c.ctl.getValue) {
                    val = c.ctl.getValue();
                }
                else if (c.isDate) {
                    val = $(c.ctl).val();
                }
                if (!!val)                    
                    hasVal = true;
            });
            if (pane.hasValues !== hasVal) {
                pane.hasValues = hasVal;
                $(this).trigger("toggleValues", [pane]);
            }
        }

    },

    _resetPane: function (pane) {
        pane.controls.forEach(function (cc) {
            if (cc.ctl) {
                if (cc.ctl.clearValue)
                    cc.ctl.clearValue();
                else if (cc.isDate)
                    $(cc.ctl).val("");
            }
        });
        pane.hasValues = false;
        $(this).trigger("toggleValues", [pane]);
    },

    // Special implementation for Horizon theme
    _initExtendedDesign: function () {

        var togglePanel = function (hdr, open) {
            $(hdr)
                .toggleClass("opened", open)
                .next(".accordionContent").toggleClass("opened", open);
        };

        var me = this;

        $(".accordionHeader", this.get_element()).click(function (e) {
            var header = this;
            var accordion = header.parentElement;
            var expandedIndexes = accordion.control._HiddenExpandedSections.value.split(';');
            var ind = header.attributes.index.value;

            if (header.className.indexOf("opened") == -1) {
                expandedIndexes.push(ind);
            } else {
                var idx = expandedIndexes.indexOf(ind);
                expandedIndexes.splice(idx, 1); // remove item from the array.
            }
            if (accordion.control._HiddenExpandedSections.value == '')
                accordion.control._HiddenExpandedSections.value = expandedIndexes[1];
            else
                accordion.control._HiddenExpandedSections.value = expandedIndexes.join(";");
            if ($(e.target).hasClass("reset-pane-btn")) {
                var h = this;
                var p = me._panes.filter(function (p) { return p.header == h; });
                if (p.length)
                    me._resetPane(p[0]);
                return false;
            }
            togglePanel(this, !$(this).hasClass("opened"));
        });

        // Initially opened pane
        this._panes = [];
        $(".accordionHeader", this.get_element()).each(function (i, h) {
            if (me._HiddenExpandedSections.value != '') {
                var expandedIndexes = me._HiddenExpandedSections.value.split(';');
                expandedIndexes.forEach(function(index) {
                    if (i == index)
                        togglePanel(h, true);
                });
            }

            var $h = $(h);
            var $p = $(h).next(".accordionContent");
            $h.attr("index", i);
            $p.attr("index", i);

            var pane = {
                animation: null,
                header: h,
                content: $p[0]
            };

            pane.controls = me._getPaneControls(i);

            // Initialize the header
            me._panes.push(pane);
        });

        setTimeout(function () { me._initTrackValueChanges(); });
    },

    add_selectedIndexChanging: function (handler)
    {
        if (!this._isExtendedDesign)
            this.get_events().addHandler('selectedIndexChanging', handler);
    },
    remove_selectedIndexChanging: function (handler)
    {
        if (!this._isExtendedDesign)
            this.get_events().removeHandler('selectedIndexChanging', handler);
    },
    raiseSelectedIndexChanging: function (eventArgs)
    {
        var handler = this.get_events().getHandler('selectedIndexChanging');
        if (handler)
        {
            handler(this, eventArgs);
        }
    },
    add_selectedIndexChanged: function (handler)
    {
        if (!this._isExtendedDesign)
            this.get_events().addHandler('selectedIndexChanged', handler);
    },
    remove_selectedIndexChanged: function (handler)
    {
        if (!this._isExtendedDesign)
            this.get_events().removeHandler('selectedIndexChanged', handler);
    },
    raiseSelectedIndexChanged: function (eventArgs)
    {
        var handler = this.get_events().getHandler('selectedIndexChanged');
        if (handler)
        {
            handler(this, eventArgs);
        }
    },

    get_Pane: function (index)
    {
        if (index === undefined || index === null)
        {
            index = this._selectedIndex;
        }
        return (this._panes && index >= 0 && index < this._panes.length) ? this._panes[index] : null;
    },

    get_Count: function ()
    {
        return this._panes ? this._panes.length : 0;
    },

    get_HeaderCssClass: function ()
    {
        return this._headerCssClass;
    },

    set_HeaderCssClass: function (value)
    {
        this._headerCssClass = value;
        this.raisePropertyChanged('HeaderCssClass');
    },

    get_HeaderSelectedCssClass: function ()
    {
        return this._headerSelectedCssClass;
    },

    set_HeaderSelectedCssClass: function (value)
    {
        this._headerSelectedCssClass = value;
        this.raisePropertyChanged('HeaderSelectedCssClass');
    },

    get_PanelHeight: function ()
    {
        return this._contentCssClass;
    },

    set_PanelHeight: function (value)
    {
        this._PanelHeight = value;
        this.raisePropertyChanged('PanelHeight');
    },

    get_ContentCssClass: function ()
    {
        return this._contentCssClass;
    },

    set_ContentCssClass: function (value)
    {
        this._contentCssClass = value;
        this.raisePropertyChanged('ContentCssClass');
    },

    get_AutoSize: function ()
    {
        return this._autoSize;
    },
    set_AutoSize: function (value)
    {
        if (Sys.Browser.agent === Sys.Browser.InternetExplorer && value === CamstarPortal.WebControls.Accordion.AutoSize.Limit)
        {
            value = CamstarPortal.WebControls.Accordion.AutoSize.Fill;
        }

        if (this._autoSize != value)
        {
            this._autoSize = value;
            this._initializeLayout();
            this.raisePropertyChanged('AutoSize');
        }
    },

    get_HiddenFieldSelectedIndex: function ()
    {
        return this._HiddenFieldSelectedIndex;
    },

    set_HiddenFieldSelectedIndex: function (value)
    {
        this._HiddenFieldSelectedIndex = value;
        this.raisePropertyChanged('HiddenFieldSelectedIndex');
    },

    get_HiddenExpandedSections: function ()
    {
        return this._HiddenExpandedSections;
    },

    set_HiddenExpandedSections: function (value)
    {
        this._HiddenExpandedSections = value;
    },

    get_AllowExpandAll: function ()
    {
        return this._AllowExpandAll;
    },

    set_AllowExpandAll: function (value)
    {
        this._AllowExpandAll = value;
        this.raisePropertyChanged('AllowExpandAll');
    },
    
    get_DisplayExpandAllButton: function ()
    {
        return this._DisplayExpandAllButton;
    },

    set_DisplayExpandAllButton: function (value)
    {
        this._DisplayExpandAllButton = value;
        this.raisePropertyChanged('DisplayExpandAllButton');
    },

    get_SelectedIndex: function ()
    {
        return this._selectedIndex;
    },

    get_serverType: function () { return this._serverType; },

    get_controlId: function () { return this._controlId; },
    set_controlId: function (value) { this._controlId = value; },

    get_expandAllText: function () { return this._expandAllText; },
    set_expandAllText: function (value) { this._expandAllText = value; },

    get_collapseAllText: function () { return this._collapseAllText; },
    set_collapseAllText: function (value) { this._collapseAllText = value; },

    get_isExtendedDesign: function () { return this._isExtendedDesign; },
    set_isExtendedDesign: function (value) { this._isExtendedDesign = value; },

    get_camstarControls: function () { return this._camstarControls; },
    set_camstarControls: function (value) { this._camstarControls = value; },

    set_SelectedIndex: function (value)
    {
        this._selectedIndex = value;

        //// This should not be here because not all the  parameters are available at this moment
        //this._changeSelectedIndex(value, true);
        //this.raisePropertyChanged('AutoSize');
    }
};

CamstarPortal.WebControls.Accordion.Accordion.registerClass('CamstarPortal.WebControls.Accordion.Accordion', Camstar.UI.Control);
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
