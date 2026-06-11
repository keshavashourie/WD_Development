// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
Type.registerNamespace('CamstarPortal.WebControls.SimpleSection');

CamstarPortal.WebControls.SimpleSection.AutoSize = function ()
{
    throw Error.invalidOperation();
}
CamstarPortal.WebControls.SimpleSection.AutoSize.prototype = {
    None: 0,
    Fill: 1,
    Limit: 2
}
CamstarPortal.WebControls.SimpleSection.AutoSize.registerEnum("CamstarPortal.WebControls.SimpleSection.AutoSize", false);


CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs = function (oldIndex, selectedIndex)
{
    CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs.initializeBase(this);

    this._oldIndex = oldIndex;
    this._selectedIndex = selectedIndex;
}
CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs.prototype = {
    get_oldIndex: function ()
    {
        return this._oldIndex;
    },
    set_oldIndex: function (value)
    {
        this._oldIndex = value;
    },

    get_selectedIndex: function ()
    {
        return this._selectedIndex;
    },
    set_selectedIndex: function (value)
    {
        this._selectedIndex = value;
    }
}

CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs.registerClass('CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs', Sys.CancelEventArgs);



CamstarPortal.WebControls.SimpleSection.SimpleSection = function (element)
{
    CamstarPortal.WebControls.SimpleSection.SimpleSection.initializeBase(this, [element]);

    this._PanelHeight = '';
    this._selectedIndex = 0;
    this._panes = [];
    this._duration = 0.25;
    this._autoSize = CamstarPortal.WebControls.SimpleSection.AutoSize.None;
    this._headersSize = 0;
    this._headerClickHandler = null;
    this._headerMouseOverHandler = null;
    this._headerMouseOutHandler = null;
    this._headerCssClass = '';
    this._headerSelectedCssClass = '';
    this._contentCssClass = '';
    this._resizeHandler = null;
    this._AllowExpandAll = false;
    this._HiddenFieldSelectedIndex = '';
    this._HiddenFieldIndexes = '';
}
CamstarPortal.WebControls.SimpleSection.SimpleSection.prototype = {
    initialize: function ()
    {
        CamstarPortal.WebControls.SimpleSection.SimpleSection.callBaseMethod(this, 'initialize');

        this._headerClickHandler = Function.createDelegate(this, this._onHeaderClick);
        this._headerMouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._headerMouseOutHandler = Function.createDelegate(this, this._onMouseOut);
        var nodes = this.get_element().childNodes;
        var index = {};
        for (index.value = 0; index.value < nodes.length; index.value++)
        {
            var header = this._getNextDiv(nodes, index);
            if (!header)
            {
                break;
            }
            var content = this._getNextDiv(nodes, index);
            if (content)
            {
                this.addPane(header, content);
                index.value--;
            }
        }

        // Ensure we have an opened pane if we're required to (and use the first
        // pane if we don't have a valid selected index)
        if (!this.get_Pane() && this._panes.length > 0)
        {
            this._changeSelectedIndex(0, false, true);
        }

        // Setup the layout for the given AutoSize mode
        this._initializeLayout();
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
        if (header.className == "SimpleSectionHeaderSelected")
        {
            var img = header.getElementsByTagName("img")[0];
            img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Minus.png";
        }

        header._index = this._panes.length;
        $addHandler(header, "click", this._headerClickHandler);
        $addHandler(header, "mouseover", this._headerMouseOverHandler);
        $addHandler(header, "mouseout", this._headerMouseOutHandler);

        // Wrap the content in a new element
        var simplesection = this.get_element();
        var wrapper = document.createElement('div');
        simplesection.insertBefore(wrapper, content);
        wrapper.appendChild(content);
        wrapper._original = content;
        pane.content = wrapper;

        wrapper.style.border = '';
        wrapper.style.margin = '';
        wrapper.style.padding = '';

        // Add the new pane at the bottom of the simplesection
        Array.add(this._panes, pane);

        this._initializePane(header._index);

        content.style.display = 'block';

        return pane;
    },

    _initializeLayout: function ()
    {
        // Cache the initial size of the simplesection
        var simplesection = this.get_element();
        this._initialHeight = simplesection.offsetHeight;
        var style = simplesection.style;


        // Initialize the simplesection itself
        if (this._autoSize === CamstarPortal.WebControls.SimpleSection.AutoSize.None)
        {
            // Remove the window resizing handler
            this._disposeResizeHandler();

            style.height = 'auto';
            style.overflowX = 'auto';
            style.overflowY = 'hidden';
        } else
        {
            // Add the window's resizing handler
            this._addResizeHandler();

            style.height = simplesection.offsetHeight + 'px';
            // style.overflow = 'hidden';
        }

        style.overflowX = 'auto';
        style.overflowY = 'hidden';


        // Setup the layout attributes for the pane so that it will be in a proper opened
        // or closed state
        for (var i = 0; i < this._panes.length; i++)
        {
            this._initializePane(i);
        }

        // Resize the selected pane so (depending on the AutoSize mode) it will fill the
        // available remaining space after the headers have been laid out.
        this._resizeSelectedPane();
    },


    _initializePane: function (index)
    {
        var pane = this.get_Pane(index);
        if (!pane)
        {
            return;
        }
        var wrapper = pane.content;
        var original = wrapper._original;
        var indexes = this._HiddenFieldIndexes.value.split(':');
        var opened = indexes[index] == 0 ? false : true;
        if (opened)
        {
            var img = pane.header.getElementsByTagName("img")[0];
            img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Minus.png";
        }

        wrapper.style.height = (opened || (this._autoSize === CamstarPortal.WebControls.SimpleSection.AutoSize.Fill)) ? 'auto' : '0px';
        wrapper.style.overflowX = 'auto';
        wrapper.style.overflowY = 'hidden';
        //wrapper.style.overflow = opened ? 'auto' : 'hidden';
        wrapper.style.display = opened ? 'block' : 'none';
        original.style.height = 'auto';
        original.style.maxHeight = '';
        //        original.style.overflow = opened ? 'auto' : 'hidden';
        original.style.overflowX = 'auto';
        original.style.overflowY = 'hidden';

    },

    _addResizeHandler: function ()
    {
        if (!this._resizeHandler)
        {
            this._resizeHandler = Function.createDelegate(this, this._resizeSelectedPane);
            $addHandler(window, "resize", this._resizeHandler);
        }
    },

    dispose: function ()
    {
        // Remove the window resizing handler
        this._disposeResizeHandler();

        // Wipe the _panes collection.  We're careful to wipe any expando properties
        // which could cause memory leaks in IE6.
        for (var i = this._panes.length - 1; i >= 0; i--)
        {
            var pane = this._panes[i];
            if (pane)
            {
                if (pane.header)
                {
                    pane.header._index = null;
                    $removeHandler(pane.header, "click", this._headerClickHandler);
                    pane.header = null;
                }
                if (pane.content)
                {
                    pane.content._original = null;
                    pane.content = null;
                }
                this._panes[i] = null;
                delete this._panes[i];
            }
        }
        this._panes = null;
        this._headerClickHandler = null;

        CamstarPortal.WebControls.SimpleSection.SimpleSection.callBaseMethod(this, 'dispose');
    },

    _disposeResizeHandler: function ()
    {
        if (this._resizeHandler)
        {
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
            case CamstarPortal.WebControls.SimpleSection.AutoSize.None:
                original.style.height = 'auto';
                original.style.maxHeight = '';
                break;
            case CamstarPortal.WebControls.SimpleSection.AutoSize.Limit:
                var remaining = this._getRemainingHeight(false);
                original.style.height = 'auto';
                original.style.maxHeight = remaining + 'px';
                break;
            case CamstarPortal.WebControls.SimpleSection.AutoSize.Fill:
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
            {
                header.className = "SimpleSectionHeaderDCSel";
            }
            else
            {
                header.parentNode.className = "SimpleSectionHeaderDCSel";
            }
        }
    },

    _onMouseOut: function (evt)
    {
        var header = evt.target;
        if (header != null)
        {
            if (header.tagName.toLowerCase() != 'span')
            {
                header.className = "SimpleSectionHeaderDC";
            }
            else
            {
                header.parentNode.className = "SimpleSectionHeaderDC";
            }
        }
    },

    _onHeaderClick: function (evt)
    {
        var header = evt.target;
        var simplesection = this.get_element();
        while (header && (header.parentNode !== simplesection))
        {
            header = header.parentNode;
        }

        evt.stopPropagation();

        var index = header._index;
        if (index === this._selectedIndex)
        {
            index = -1;
        }
        this._changeSelectedIndex(index, true);
    },

    _changeSelectedIndex: function (index, animate, force)
    {
        var indexes = this._HiddenFieldIndexes.value.split(':');
        var lastIndex = this._selectedIndex;
        var currentPane = this.get_Pane(index);
        var lastPane = this.get_Pane(lastIndex);
        if (!force && (currentPane == lastPane))
        {
            return;
        }

        // Raise the selectedIndexChanging event but don't change the selected index
        // if the handler set the cancel property to true
        var eventArgs = new CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs(lastIndex, index);
        this.raiseSelectedIndexChanging(eventArgs);
        if (eventArgs.get_cancel())
        {
            return;
        }

        //This sets the header CSS class to the non-selected case.
        if (lastPane)
        {
            lastPane.header.className = this._headerCssClass;
            if (currentPane == lastPane || currentPane == null)
            {

                var img = lastPane.header.getElementsByTagName("img")[0];
                if (this._AllowExpandAll && lastPane.content.style.display == "block")
                {

                    indexes[lastIndex] = "0";
                    this._startPaneChange(lastPane, false);
                    lastPane.content.style.display = "none";
                    img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Plus.png";
                }
                else if (this._AllowExpandAll && lastPane.content.style.display == "none")
                {
                    indexes[lastIndex] = "1";
                    this._startPaneChange(lastPane, true);
                    lastPane.content.style.display = "block";
                    img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Minus.png";
                    this._resizeSelectedPane();
                }
            }

        }

        //This sets the selected header CSS class if available.
        if (currentPane)
        {
            currentPane.header.className = (this._headerSelectedCssClass == '') ?
                this._headerCssClass : this._headerSelectedCssClass;


            var img = currentPane.header.getElementsByTagName("img")[0];
            if (this._AllowExpandAll && currentPane.content.style.display == "block")
            {
                indexes[index] = "0";
                this._startPaneChange(currentPane, false);
                currentPane.content.style.display = "none";
                img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Plus.png";
            }
            else if (this._AllowExpandAll && currentPane.content.style.display == "none")
            {
                indexes[index] = "1";
                this._startPaneChange(currentPane, true);
                currentPane.content.style.display = "block";
                img.src = img.src.substring(0, img.src.lastIndexOf('-')) + "-Minus.png";
                this._resizeSelectedPane();
            }

        }

        this._selectedIndex = index;

        if (!this._AllowExpandAll)
        {
            this._changePanes(lastIndex);
        }

        var newIndexes = "";
        for (var j = 0; j < indexes.length; j++)
        {
            newIndexes += indexes[j]
            if (j < indexes.length - 1)
                newIndexes += ":";
        }

        this._HiddenFieldIndexes.value = newIndexes;
        this._HiddenFieldSelectedIndex.value = index;
        this.raiseSelectedIndexChanged(new CamstarPortal.WebControls.SimpleSection.SimpleSectionSelectedIndexChangeEventArgs(lastIndex, index));
        this.raisePropertyChanged('SelectedIndex');
    },

    _changePanes: function (lastIndex)
    {
        if (!this.get_isInitialized())
        {
            return;
        }

        var open = null;
        var close = null;
        for (var i = 0; i < this._panes.length; i++)
        {
            var pane = this._panes[i];

            if (i == this._selectedIndex)
            {
                pane.content.style.display = "block";
                this._startPaneChange(pane, true);
                pane.header.className = this._headerSelectedCssClass;
                this._resizeSelectedPane();
            }
            else if (i == lastIndex)
            {
                pane.content.style.display = "none";
                this._startPaneChange(pane, false);
                pane.header.className = this._headerCssClass;
            } else
            {
                continue;
            }
        }
    },

    _startPaneChange: function (pane, opening)
    {
        var wrapper = pane.content;
        var original = wrapper._original;

        if (opening)
        {
            // Make the hidden panes visible so we can see them animate
            wrapper.style.height = "auto";
            wrapper.style.display = 'block';
        } else
        {
            // Hide any overflow because we'll be shrinking the wrapper div down to 0px and
            // we don't want content leaking out the bottom
            wrapper.style.overflowX = 'auto';
            wrapper.style.overflowY = 'hidden';

            // Turn off overflow on the original div because it's content doesn't grow during
            // the animation and leaving it on slows the animation down
            original.style.overflowX = 'auto';
            original.style.overflowY = 'hidden';

            // Remove any explicit height off the original content section but manually set
            // the wrapper to the initial height (since it will be shrunk from this height
            // to zero)
            if (this._autoSize === CamstarPortal.WebControls.SimpleSection.AutoSize.Limit)
            {
                wrapper.style.height = this._getTotalSize(original).height + 'px';
                original.style.maxHeight = '';
            }
        }
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

        if (this._autoSize === CamstarPortal.WebControls.SimpleSection.AutoSize.None)
        {
            // If the AutoSize mode is "None", then we use the size of the pane
            if (pane)
            {
                height = this._getTotalSize(pane.content._original).height;
            }
        }
        else
        {
            // Compute the amount of space used
            height = this._headersSize;
            if (includeGutter && pane)
            {
                height += this._getGutterSize(pane.content._original).height;
            }

            // Determine how much of the remaining space to use
            // (if AutoSize is "Fill", use the rest of the available space)
            var simplesection = this.get_element();
            height = Math.max(simplesection.offsetHeight - height, 0);

            // If AutoSize is "Limit", then the size of the pane should be either its
            // actual size, or the rest of the available space.
            if (pane && (this._autoSize === CamstarPortal.WebControls.SimpleSection.AutoSize.Limit))
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


    add_selectedIndexChanging: function (handler)
    {
        this.get_events().addHandler('selectedIndexChanging', handler);
    },

    remove_selectedIndexChanging: function (handler)
    {
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
        this.get_events().addHandler('selectedIndexChanged', handler);
    },

    remove_selectedIndexChanged: function (handler)
    {
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
        if (Sys.Browser.agent === Sys.Browser.InternetExplorer && value === CamstarPortal.WebControls.SimpleSection.AutoSize.Limit)
        {
            value = CamstarPortal.WebControls.SimpleSection.AutoSize.Fill;
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

    get_HiddenFieldIndexes: function ()
    {
        return this._HiddenFieldIndexes;
    },

    set_HiddenFieldIndexes: function (value)
    {
        this._HiddenFieldIndexes = value;
        this.raisePropertyChanged('HiddenFieldIndexes');
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

    get_SelectedIndex: function ()
    {
        return this._selectedIndex;
    },

    set_SelectedIndex: function (value)
    {
        this._selectedIndex = value;
        this._changeSelectedIndex(value, true);
        this.raisePropertyChanged('AutoSize');
    }
}

CamstarPortal.WebControls.SimpleSection.SimpleSection.registerClass('CamstarPortal.WebControls.SimpleSection.SimpleSection', Camstar.UI.Control);
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
