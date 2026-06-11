// Copyright Siemens 2023

/// <reference path="../../MicrosoftAjaxExt.js"/>
/// <reference path="../../Camstar.UI/Control.js" />
/// <reference path="PickListCommon.js" />
/// <reference path="../../../jquery/jquery.min.js" />
/// <reference path="../../../jquery/jquery-ui.min.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls.PickLists");

var __oldCallbackRender = null;
let isSearchPerformed = false;

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.initializeBase(this, [element]);

    // Objects
    this._tree = null;

    // Elements
    this._parent = null;
    this._view = null;
    this._filter = null;
    this._filterTextBox = null;
    this._filterPrevValue = '';
    this._filterForceRequestData = null;
    this._filterCompleted = true;
    this._refresh = null;
    this._pager = null;
    this.Pager = null;
    this._close = null;
    this._shadow = null;
    this._status = null;
    this._itemCountLabel = null;
    this.itemCountLabelText = '';
    this.itemCountLabelTextFound = '';
    this._indent = 24;

    this._stretchLink = null;

    // Properties
    this._needpostback = false;
    this._viewId = '';
    this._panelId = null;
    this._disableAutoFiltering = false;

    this._totalPages = 0;
    this._currentPage = 0;
    this._totalRecords = 0;
    this._pagerDisposition = '';
    this._initialData = null;
    this._expandedNodesFieldID = null;
    this._webpartOwner = null;
};

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.prototype =
{
    initialize: function () {
            Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.callBaseMethod(this, 'initialize');
      
            this.Pager = $(this._pager);

            this.initializeView();
            this._webpartOwner = $(this.get_element()).parents('.webpart').first();
            var me = this;

        if (this._refresh) {
                $(this._refresh).unbind().click(function (e) {
                    return me.onFilter(e);
                });
            }

        if (this._close) {
                $(this._close).unbind().click(function (e) {
                    return me.onClose(e);
                });
            }

            if (this._stretchLink) {
                $(this._stretchLink).unbind()
                    .bind('mousedown', function (e) {
                        return me.onStretchMouseDown(e);
                    });
            }

        if (this._filter) {
                $('.filter', this._element).show();
                this._filterForceRequestData = $get(this._filter.id + '_Flhc', this._filter);
                this._filterForceRequestData.value = '0';
                this._filterTextBox = $get(this._filter.id + '_Fltc', this._filter);
            $(this._filterTextBox)
                .bind('click', function (e) { return me.onFilterBoxClick(e); })
                .bind('keyup', function (e) { return me.onFilterChanged(e); })
                .bind('keydown', function (e) { return me.onFilterChanging(e); });
                $(this._filterTextBox).attr('placeholder', 'Search');
            }
        else {
                $('.filter', this._element).hide();
            }

            if (this._pager) {
               
                // remove postback from the pager prev-next buttons
                var t = $('#' + this._pager.id + '_Prev, #' + this._pager.id + '_Next').each(
                    function () {
                        this.onclick = null;
                        $(this).click(function () {
                            if (this.disabled) {
                                return false;
                            }
                            return me.onPagerButtonClick($(this).attr('pgbtn'));
                        });
                    }
                );
            }

            if (this._status)
                this._status.checked = false;
        },

    onFilterBoxClick: function (e) {
        var filterID = $get(this._filter.id + '_Fltc', this._filter).id;
        var filterIDTrimmed = filterID.replace('_Panl_Filter_Fltc', '');

        const picklistList = document.getElementsByClassName('cs-picklist');
        const arrPickList = Array.from(picklistList);

        arrPickList.forEach(function (element) {
            if (element.childNodes[0].id.replace('_Edit', '') == (filterIDTrimmed)) {
                element.setAttribute('state', 'select');
            }
        });

        const picklistFilter = document.getElementsByClassName('filter');
        const arrPickListFilter = Array.from(picklistFilter);

        arrPickListFilter.forEach(function (element) {
            if (element.childNodes[0].id.replace('_Panl_Filter', '') == (filterIDTrimmed)) {
                element.setAttribute('state', 'select');
            }
        });
    },

    onFilterChanging: function (e) {
     
            if (!e)
                e = window.event;
        if (!this._filterCompleted) {
                e.returnValue = false;
        }
        },

    onFilterChanged: function (e) {
        if (this._filterTextBox.value) {
            isSearchPerformed = true;
        }
        else {
            isSearchPerformed = false;
        }
        if (this._filterCompleted) {
                if (this._filterPrevValue != this._filterTextBox.value) {
                    this._filterPrevValue = this._filterTextBox.value;
                    if (this.get_parent().get_useValue() && !this._disableAutoFiltering) {
                        // client filtering
                        this._totalRecords = this.filterItems();
                        this.setCurrentPage(1);
                        this.__displayTotalItems();
                        this._filterCompleted = true;
                    }
                    else {
                        this._filterForceRequestData.value = '1';
                        this._filterCompleted = false;
                        if (!this._disableAutoFiltering) {
                            this.updateInProgress = 'Started';
                            this.setCurrentPage(1);
                            this._filterPrevValue = this._filterTextBox.value;
                            this.requestData(true);
                        }
                    }
                }
        }
        },

    filterItems: function () {
        
            // provides in-place filtering in case of all items are loaded on the client side
            var items = $('.viewer li.jstree-leaf', this.get_element());
            var filterText = this._filterTextBox.value.toLocaleLowerCase();
            var totalFilterdItems = 0;

            items.each(function (index, li) {
                var txt = $('a', li).text().trimStart().toLocaleLowerCase();
                if (!txt.startsWith(filterText)) {
                    $(li).hide();
                }
                else {
                    $(li).show();
                    totalFilterdItems++;
                }
            }
            );

            return totalFilterdItems;
        },

        onStretchMouseDown: function () {
            var me = this;
            $(this._stretchLink).bind('mouseup', function (e) { return me.onStretchMouseUp(e); });

            $addHandlers(window,
                {
                    'mouseup': this.onStretchMouseUp,
                    'mousemove': this.onStretchMouseMove

                }, this);
        },

        onStretchMouseMove: function (ev) {
            ev = ev || window.event;

            var width = ev.clientX - $(this.get_element()).position().left;

            if (width > 200)
                $(this.get_element()).css('width', (width + 10) + 'px');
        },

        onStretchMouseUp: function () {
            $clearHandlers(window);
            $clearHandlers(this._stretchLink);
            $addHandlers(this._stretchLink,
                {
                    'mousedown': this.onStretchMouseDown
                }, this);
        },

    dispose: function () {
            //detach the panel from temporary node, if it's hasn't been done by handlers before. 
            //it helps to avoid of elements with dublicating ids.
            if (this._parent != null)
                $(this._element).detach();

            if (this._close)
                $clearHandlers(this._close);

            if (this._refresh)
                $clearHandlers(this._refresh);

            Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.callBaseMethod(this, 'dispose');
        },

        setup: function (parent) {
            this.set_parent(parent);
            if (parent.get_onPageLoad()) {
                if (this.get_initialData()) {
                    this._tree_load_callback(this.get_initialData(), this);
                }
            }
        },

        getTreeById: function (id) {
            return null;
        },

        _loadTree: function () {
            var args = {
                cmd: 'load',
                page: !this._filterCompleted ? 0 : this.getCurrentPage(),
                filterForceRequestData: this._filter ? this._filterForceRequestData.value : null,
                filterTextBox: this._filter ? this._filterTextBox.value : null
            };
            this._doCallBack(JSON.stringify(args), this._tree_load_callback);
        },

        _loadTreeNode: function (nodeLi) {
            var panel = $('.viewer', this.get_element());

            nodeLi.removeClass('jstree-last');

            var args = {
                cmd: 'loadNode',
                page: !this._filterCompleted ? 0 : this.getCurrentPage(),
                filterForceRequestData: this._filter ? this._filterForceRequestData.value : null,
                filterTextBox: this._filter ? this._filterTextBox.value : null,
                nodeTag: nodeLi.attr('tag')
            };
            this._doCallBack(JSON.stringify(args), this._node_load_callback, { panel: panel, node: nodeLi, me: this });
        },

        _doCallBack: function (arg, callBackFun, context) {
            if (!context)
                context = this;
            __theFormPostData = "";
            __theFormPostCollection = new Array();
            WebForm_InitCallback();

            if ($(this._webpartOwner).is('.ui-dialog-content')) {
                // Add manually control values for lite-popup webpart ( the controls are out of parent asp form )
                $(this._webpartOwner).find(':input').each(function () { WebForm_InitCallbackAddField(this.name, this.value); });
            }

            WebForm_DoCallback(this.get_panelId(), arg, callBackFun, context,
                function (e) {
                    if (typeof (e) === 'string')
                        alert(e);
                }
                , true);
        },

        initializeView: function () {
            if (this._view == null) {
                var resp = { records: 0, page: 0, pages: 0, items: 0, message: '' };
                this.processPagerStatus(resp);
            }
        },

        getNodeById: function (id) {
            return null;
        },

        // --- IPickListPanel Interface ---
        load: function (force) {
            this.requestData(force);
        },


        page: function () {
            this.requestData(true);
        },

    setFocus: function () {
            if (this.get_visible()) {
                if (this._filter != null && $(this._filter).is(':visible') && $('#' + this._filter.id + "_Fltc", this._filter).val() != '') {
                    $('#' + this._filter.id + "_Fltc", this._filter).focus();
                }
                else if (this.Pager.is(':visible')) {
                    var next = this.Pager.find('[pgbtn="Next"]', this._pager);
                    if (next.is(':disabled'))
                        next = this.Pager.find('[pgbtn="Prev"]', this._pager);
                    if (!next.is(':disabled'))
                        next.focus();
                }
                else {
                    this._element.focus();
                }
            }
        },
        // --- End of IPickListPanel ---

        // --- Event Handlers Section ---
        onClose: function (e) {
            if (this._parent != null)
                this._parent.close();
        },

    onFilter: function (e) {
            // server filtering
            this.updateInProgress = 'Started';
            this.setFocus(true);
            this._filterCompleted = false;
            this.setCurrentPage(1);
            this.requestData(true);
            if (e.preventDefault)
                e.preventDefault();
            return false;
        },

        onSelect: function (li) {
            var val = null;

            if (li != null) {
                // deselect previously selected
                li.parent().find('li[selected]').removeProp('selected');
                li.prop('selected', 'selected');
                var parentNodeText = '';
                var parentTags = [];
                var parentNodes = li.parents(".jstree-open").has('a');
                if (parentNodes.length > 0) {
                    parentNodeText = $.trim($(parentNodes[0]).children('a').text());
                    $.each(parentNodes, function (index, value) {
                        if ($(value).attr("tag") != null && $(value).children('ul').children('li').attr("tag") != null) {
                            var baseWorkflow = eval('(' + decodeURIComponent($(value).attr("tag")) + ')');
                            var initialWorkflow = eval('(' + decodeURIComponent($(value).children('ul').children('li').attr("tag")) + ')');

                            if ($(value).children('ul').children('li').attr("mode") == 'subwf')
                            { initialWorkflow = baseWorkflow }

                            var workflow = {
                                Name: baseWorkflow.Name,
                                Info: baseWorkflow.Info,
                                Revision: baseWorkflow.Revision,
                                Ror: baseWorkflow.Ror,
                                InitialName: initialWorkflow.Name
                            };
                            var changedNode = encodeURIComponent(JSON.stringify(workflow));
                            parentTags.push(changedNode);
                        }
                        else {
                            parentTags.push($(value).attr("tag"));
                        }
                    });
                }
                val = { key: li.attr('key'), text: $.trim(li.children('a').text()), tag: li.attr('tag'), parent: parentNodeText, parentTags: parentTags };
            }
            else {
                val = { key: '', text: '', tag: '', parent: '' };
            }

            if (this._parent) {
                if (!this._parent.setValue(val))
                    return false;
                if (this._parent._isInlineControl)
                    this._parent.reloadDependentInlineControls();
            }

            return !this._needpostback;
        },

        onPagerNumberClick: function (ev) {
            this.updateInProgress = 'Started';
            this.setFocus(true);
            this._currentPage = parseInt($(ev.target).text());
            this.requestData(true);
            return false;
        },

        onPagerButtonClick: function (toPage) {
            this.updateInProgress = 'Started';
            this.setFocus(true);

            switch (toPage) {
                case "First":
                    this._currentPage = 1;
                    break;
                case "Prev":
                    this._currentPage -= 1;
                    if (this._currentPage == 0)
                        this._currentPage -= 1;
                    break;
                case "Next":
                    this._currentPage += 1;
                    if (this._currentPage > this._totalPages)
                        this._currentPage = this._totalPages;
                    break;
                case "Last":
                    this._currentPage = this._totalPages;
                    break;
                default:
                    this._currentPage = parseInt(toPage);
                    if (isNaN(this._currentPage))
                        this._currentPage = 1;
            }

            this.requestData(true);
            return false;
        },
        // --- End of Event Handlers ---

        hide: function (useAnimation, callback) {
            if (!useAnimation)
                $(this._element).hide();
            else {
                $(this._element).slideUp(200, function () {
                    if (callback)
                        callback();
                });
            }
        },

        remove: function () {
            if (this._parent != null)
                $(this._parent.get_editor()).focus();
            $(this._element).remove();
        },

        show: function () {
            $.each($(document).find('.cs-picklist-panel'), function (i, val) {
                if (!$(val).hasClass('standAlone')) {
                    $(val).css('display', 'none');
                }
            });

            $(this._element).show();
        },

        get_visible: function () {
            return $(this._element).is(':visible');
        },

        // --- Pager Control Section ---
        setCurrentPage: function (pageNum) {
            this._currentPage = pageNum;
        },

        getCurrentPage: function () {
            if (this._currentPage == 0)
                this._currentPage++;
            return this._currentPage;
        },

        createLinkPages: function () {
            var me = this;
            var pagesControl = this.Pager.find('[id$= "_pagerPages"]');

            var createLinkFunction = function (index) {
                var pageElement = $('<span class="pagelink">' + index + '</span>');
                if (me._currentPage == index) {
                    pageElement.attr('selected', '');
                }
                else {
                    pageElement.click("click", function (ev) {
                        return me.onPagerNumberClick(ev);
                    });
                }
                return pageElement[0];
            };

            pagesControl.empty();

            var i = 0;
            // Create 4 links from current page to the last
            if ((this._currentPage + 4) <= this._totalPages) {
                for (i = this._currentPage; i <= this._currentPage + 4; i++)
                    pagesControl.append(createLinkFunction(i));
            }
            else {
                if (this._totalPages - 4 > 0) {
                    for (i = this._totalPages - 4; i <= this._totalPages; i++)
                        pagesControl.append(createLinkFunction(i));
                }
                else {
                    for (i = 1; i <= this._totalPages; i++)
                        pagesControl.append(createLinkFunction(i));
                }
            }
        },

        setPosition: function (top, left, orient) {
            $(this._element).css("top", top).css("left", left);
        },

        adjust: function () {
            var border = 4;
            var parent = this._parent;
            var width = 0;
            if (parent) {
                if (parent.get_isInlineControl())
                    width = $(parent._editor).parent('div').width();
                else
                    width = $(parent._element).width() - border * 2;
            }
            if (width > $(this._element).width())
                $(this._element).width(width);
        },

        adjustStandAlone: function () {
            var parEl = this._parent;
            if (parEl) {
                var panelDelta = $(this._element).outerHeight(true) - $(this._element).height() + 8 /*shadow*/;
                var viewerHeight = $(parEl._element).height() - $(parEl._label).outerHeight(true) -
                    $('.footer', this._element).outerHeight(true) - $('.filter', this._element).outerHeight(true) - panelDelta;
                if (viewerHeight > 0)
                    $('.viewer', this._element).height(viewerHeight);
            }
        },

        setPagerButton: function (direction, isEnabled) {
            var el = this.Pager.find('[pgbtn="' + direction + '"]');
            if (el.length > 0) {
                el[0].disabled = !isEnabled;
                el.css('cursor', isEnabled ? 'pointer' : 'default');
            }
        },

        // --- End of Pager Control ---

        // --- Data Processing Section ---
        get_loaded: function () {
            return this._status.checked;
        },
    set_loaded: function (value) {
            if (!value)
                this._filterCompleted = false;
        this._status.checked = value;
        },

        requestData: function (force) {
            if (force == undefined)
                force = false;

            var res = false;
            if (!this.get_loaded() || force) {
                if (this._view == null) {
                    this._loadTree();
                    res = true;
                }
                else {
                    if (!$.isFunction(this._view.control.DataLoaded)) {
                        var me = this;
                        this._view.control.DataLoaded = function () {
                            me.set_loaded(true);
                            me._filterCompleted = true;
                        };
                    }
                    if (this._filterTextBox)
                        this._view.control.SetFilter(this._filterTextBox.value);
                    this._view.control.Reload(null, null);
                    res = true;
                }
            }
            return res;
        },

        processPagerStatus: function (resp) {
            var leaveTotalRecords = false;
            //Status tag consists of number of pages, status message to display and number of the current page
            if (resp.pages > 0 && resp.page == 1) {
                this._totalPages = resp.pages;
            }
            else {
                // We don't request TotalCount on non-first page.
                leaveTotalRecords = true;
            }
            if (resp.records > 0) {
                this._totalRecords = resp.records;
            }
            else if (resp.rows != undefined)
                this._totalRecords = resp.rows.length;
            else {
                if (!leaveTotalRecords)
                    this._totalRecords = 0;
            }

            if (this._totalPages > 1) {
                this.Pager.show();
                this.setFocus();

                //Set pager buttons` styles according to the page position
                this._currentPage = resp.page;
                this.setPagerButton('First', this._currentPage > 1);
                this.setPagerButton('Prev', this._currentPage > 1);
                this.setPagerButton('Next', this._currentPage < this._totalPages);
                this.setPagerButton('Last', this._currentPage < this._totalPages);

                this.createLinkPages();
            }
            else {
                // hide pager if just one page
                this.Pager.hide();
            }

            this.__displayTotalItems();
            if (resp.message) {
                DisplayMessage(resp.message, false);
            }
        },

        __displayTotalItems: function () {
            // Set total number of records        
            if (isSearchPerformed){
            $('.footer .cs-label', this.get_element()).html(`${this._totalRecords} <i>${this.get_itemCountLabelTextFound()}</i>`).show();
        }
            else {
            $('.footer .cs-label', this.get_element()).html(`${this._totalRecords} <i>${this.get_itemCountLabelText()}</i>`).show();
        }
    },

    __newCallbackRender: function (payload, context) {
            if (payload == "SessionClosed") {
                document.parentWindow.eval("__page.redirect(\"Default.aspx\");");
                return;
            }

            // Invoke original callback handler
            __oldCallbackRender(payload, context);


            // find panel component by view id
            var panel = null;
            var allComponents = Sys.Application.getComponents();
            for (var i = 0; i < allComponents.length; i++) {
                if (Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.isInstanceOfType(allComponents[i]) &&
                    allComponents[i].get_viewId() == context.clientId) {
                    panel = allComponents[i];
                    break;
                }
            }

            if (panel) {
                panel.afterDemandLoad(context.clientId, context.nodeId);
                panel._filterCompleted = true;
            }
        },

        _tree_load_callback: function (resp, me, filter) {
            
            if (me._filterTextBox != null && me._filterPrevValue != me._filterTextBox.value) {
                me.requestData(true);
                me._filterPrevValue = me._filterTextBox.value;
            }
            if (typeof resp == "string") {
                me.set_initialData(resp);
                resp = me.get_initialData();
            }
            if (resp.exception) {
                alert(resp.exception);
                return;
            }
            if (resp.rows == undefined) {
                // Set empty data
                resp.rows = [];
            }

            var panel = $('.viewer', me.get_element());
            panel.attr('view', resp.view);
            panel
                .off("loaded.jstree refresh.jstree")
                .on("loaded.jstree refresh.jstree", function (e, data) {

                    var div = $('.viewer', me.get_element())[0];
                    $('li', div).each(function (i, l) { me._setupNode($(l)); });

                    me._filterCompleted = true;
                    var divJqueryObject = $(div);

                    // check if picklist control has default-expanded class.
                    // The class is applied from portal studio to DropDownList control
                    if (divJqueryObject.parents().eq(1).hasClass("default-expanded") == true && divJqueryObject.jstree) {
                        setTimeout(function () {
                            divJqueryObject.jstree("open_all");
                        });
                    }
                })
                .off("select_node.jstree")
                .on("select_node.jstree", function (e, data) {
                    var li = $('#' + data.selected);
                    var allowSelect = true;

                    if (resp.selectChildOnly) {
                        var parent = data.instance.get_parent(li);
                        allowSelect = parent && parent != '#';
                        if (allowSelect) {
                            if (resp.selectLeafOnly)
                                allowSelect = data.instance.is_leaf(li);
                        }
                    }
                    else {
                        if (resp.selectLeafOnly)
                            allowSelect = data.instance.is_leaf(li);
                    }

                    if (allowSelect)
                        return me.onSelect($('#' + data.selected));
                })
                .off("close_node.jstree")
                .on("close_node.jstree", function (e, data) {
                    if (data.node.parents[0] != '#')
                        me.StoreExpandedKeys($($('#' + data.node.parents[0])).attr('key'), false);
                    return true;
                })

                .off("open_node.jstree")
                .on("open_node.jstree", function (e, data) {
                    var div = $('.viewer', me.get_element())[0];
                    $('li', div).each(function (i, l) { me._setupNode($(l)); });

                    if (data.node.parents[0] != '#') {

                        if (data.node.children != undefined && data.node.children.length === 0)
                            if (data.node.li_attr.mode != undefined && data.node.li_attr.mode === 'ror' || 'subwf') {
                                me._loadTreeNode($('#' + data.node.id));
                                return false;
                            }
                        if (data.instance.get_children_dom($('#' + data.node.parents[0])).length) {
                            me.StoreExpandedKeys($($('#' + data.node.parents[0])).attr('key'), true);
                            return true;
                        }
                    }
                })
                .jstree(
                    {
                        core:
                        {
                            data: resp.rows,
                            check_callback: true,
                            themes: {
                            },
                        },
                        plugins: [
                            "themes",
                            "wholerow"
                        ]
                    });
           
            panel.jstree(true).refresh();
            panel.jstree(true).settings.core.data = resp.rows;
            panel.jstree(true).refresh();
            

            me.processPagerStatus(resp);
            me.set_loaded(true);
        },

    _node_load_callback: function (response, context) {
            var tmp = [], res;
            var resp = eval(response);
            resp = resp[0];

            if (resp.exception) {
                alert(resp.exception);
                return;
            }

            if (resp.rows == undefined) {
                // Set empty data
                resp.rows = [];
            }

            $.each(resp.rows, function (i, r) {
                r.li_attr.level += (parseInt(context.node.attr('level')) + 1);
                if (r.children == null)
                    r.children = [];
                res = context.panel.jstree('create_node', context.node, r, "last", false, true);
                tmp.push(res);
            }
            );
            //manually setup newly added nodes to add picklist icons for subWFs
            $.each(tmp, function (i, val) {
                context.me._setupNode($('#' + (val)));
            });
            context.panel.jstree('open_node', context.node);
            context.me.CleanUpExpandedKeys();
        },

        _iClickCorrection: function (event, correctionValue) {
            var $prev = $(event.target).prev();

            if ($prev.length > 0) {
                if ($(document.body).hasClass("mobile-device") && $prev.is('i')) {
                    $prev.click();
                    return false;
                }
            }
        },

    _setupNode: function (li) {
            var me = this;
            if (this.IsStandAlone()) {
                $(li).draggable({
                    revert: true,
                    helper: function () {
                        return $('<div></div>');
                    }
                });
                var key = li.attr('key');
                if (key) {
                    this.RestoreExpandedKeys(li, key);
                    if (this._parent && this._parent._value && this._parent._value.value == key) {
                        li.attr("selected", 'selected');

                        if (this.IsStandAlone()) {
                            var panel = $('.viewer', this.get_element());
                            setTimeout(function () {
                                var xScroll = $(li).offset().top - panel.offset().top + panel.scrollTop() - panel.height() / 2;
                                if (xScroll > 0)
                                    panel.scrollTop(xScroll);
                            }, 100);
                        }
                    }
                }
            }

            // setup padding
            if (li.length) {
                if (li.attr('description')) {
                    li.attr('title', li.attr('description'));
                }

                if (li.is('.jstree-leaf') && li.attr('leaf') === "false") {
                    li.removeClass('jstree-leaf jstree-last').addClass('jstree-closed');
                }

                // Click on LI calls a A click

                li.click(function (e) {
                    if (li.attr('filter_mode') == "Disable") {
                        return false;
                    }

                    if ($(e.target).is('i')) {
                        return true;
                    }
                    me._iClickCorrection(e, 5); // second parameter is value of correction in pixels    

                    if (!$(e.target).is('a')) {
                        li.children('a').click();
                        return false;
                    }
                    else {
                        return true;
                    }
                });
            }
        },

        IsStandAlone: function () {
            return this._parent && this._parent._displayPanelOnly;
        },

        StoreExpandedKeys: function (key, isPush) {
            if (this.IsStandAlone() && key) {
                var hiddenField = $("#" + this._expandedNodesFieldID);
                if (hiddenField.length > 0) {
                    var expandedKeys = hiddenField.val().split(';');
                    var ind = $.inArray(key, expandedKeys);
                    if (isPush) {
                        if (ind < 0)
                            expandedKeys.push(key);
                    }
                    else {
                        if (ind >= 0)
                            expandedKeys.splice(ind, 1);
                    }
                    hiddenField.val(expandedKeys.join(";"));
                }
            }
        },

        RestoreExpandedKeys: function (li, key) {
            var hiddenField = $("#" + this._expandedNodesFieldID);
            if (hiddenField.length > 0 && hiddenField.val()) {
                var expandedKeys = hiddenField.val().split(';');
                var ind = $.inArray(key, expandedKeys);
                if (ind >= 0) {
                    var panel = $('.viewer', this.get_element());
                    panel.jstree("open_node", li);
                }
            }
        },

        CleanUpExpandedKeys: function () {
            var hiddenField = $("#" + this._expandedNodesFieldID);
            if (hiddenField.length > 0)
                hiddenField.val('');
        },

        // --- End of Data Processing ---

        get_parent: function () { return this._parent; },
        set_parent: function (value) { this._parent = value; },

        get_view: function () { return this._view; },
        set_view: function (value) { this._view = value; },

        get_filter: function () { return this._filter; },
        set_filter: function (value) {
            this._filter = value;
        },

        get_refresh: function () { return this._refresh; },
        set_refresh: function (value) { this._refresh = value; },

        get_pager: function () { return this._pager; },
        set_pager: function (value) {
            this._pager = value;

            var allPagerLis = $($(this._pager).children()).children();
            var allPagerInputs = $(allPagerLis).children();

            $(allPagerInputs[4]).remove('*');
            $(allPagerInputs[0]).remove('*');
        },

        get_close: function () { return this._close; },
        set_close: function (value) { this._close = value; },

        get_itemCountLabel: function () { return this._itemCountLabel; },
        set_itemCountLabel: function (value) { this._itemCountLabel = value; },

        get_itemCountLabelText: function () { return this._itemCountLabelText; },
        set_itemCountLabelText: function (value) { this._itemCountLabelText = value; },

        get_itemCountLabelTextFound: function () { return this._itemCountLabelTextFound; },
        set_itemCountLabelTextFound: function (value) { this._itemCountLabelTextFound = value; },

        get_stretchLink: function () { return this._stretchLink; },
        set_stretchLink: function (value) { this._stretchLink = value; },

        get_shadow: function () { return this._shadow; },
        set_shadow: function (value) { this._shadow = value; },

        get_status: function () { return this._status; },
        set_status: function (value) { this._status = value; },

        get_viewId: function () { return this._viewId; },
        set_viewId: function (value) { this._viewId = value; },

        get_panelId: function () { return this._panelId; },
        set_panelId: function (value) { this._panelId = value; },

        get_needPostBack: function () { return this._needpostback; },
        set_needPostBack: function (value) { this._needpostback = value; },

        get_disableAutoFiltering: function () { return this._disableAutoFiltering; },
        set_disableAutoFiltering: function (value) { this._disableAutoFiltering = value; },

        get_initialData: function () { return this._initialData; },
        set_initialData: function (value) {
            var d = eval(value);
            if (d && d.length)
                this._initialData = d[0];
            else
                this._initialData = null;
        },

        get_expandedNodesFieldID: function () { return this._expandedNodesFieldID; },
        set_expandedNodesFieldID: function (value) { this._expandedNodesFieldID = value; }
    }

Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel.registerClass('Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListPanel', Camstar.UI.Control, Camstar.WebPortal.FormsFramework.WebControls.PickLists.IPickListPanel);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
