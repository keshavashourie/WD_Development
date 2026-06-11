// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../../jquery/jquery.min.js" />
/// <reference path="../../jquery/jquery-ui.min.js" />
/// <reference path="../../jquery/jqGrid/jquery.jqGrid.min.js" />
/// <reference path="../CamstarPortal.WebControls/JQGridHelper.js" />

Type.registerNamespace("CamstarPortal.WebControls");

/// CamstarPortal.WebControls.GridResponseStatus 
CamstarPortal.WebControls.GridValidationStatus = function () 
{
    this.Code = -1;
    this.FieldName = null;
    this.Message = null;
}

CamstarPortal.WebControls.GridValidationStatus.prototype =
{
    get_Code: function() {return this.Code; },
    set_Code: function(value) {this.Code = value;},

    get_FieldName: function(){return this.FieldName; },
    set_FieldName: function(value) {this.FieldName = value;},

    get_Message: function(){return this.Message; },
    set_Message: function(value) {this.Message = value;}
};

CamstarPortal.WebControls.GridValidationStatus.descriptor =
{
    properties:
    [
        {name: 'Code', type: Number},
        {name: 'FieldName', type: String}, 
        {name: 'Message', type: String}
    ]
};
CamstarPortal.WebControls.GridValidationStatus.registerClass('CamstarPortal.WebControls.GridValidationStatus');

CamstarPortal.WebControls.JQGridBaseData = function (element)
{
    CamstarPortal.WebControls.JQGridBaseData.initializeBase(this, [element]);

    this._gridID = "";
    this._eventTargetId = "";

    // IDs for jQuery
    this.GridID = "";
    this.PagerID = "";
    this.wrapEverything = false;    // wrap all cells regardless visiblility
    this.wrapCellTimer;

    this._initialSettings = null;
    this._initialData = null;

    this._serverType = null;
    this._contextID = null;

    this._divEditorID = "";

    this._editorSettings = null;

    this._last_selectedRowID = null;
    this._subGridId = null;

    this._isSubGridInstance = false;
    this._parentGridID = null;
    this._collapseOtherOnExpanding = false;
    this._customEventHandlers = null;
    this._hidden = false;
    this._width = null;
    this._justSelect = false;
    this._controlCustomHandlers = [];
    this._subgridMarkIndex = -1;
    this.multiCheckBox = null;
    this._rowSelectionMode = null;
    this._shrinkColumns = null;
    this._expandGroupIfRowSelected = false;
    this._dataChanged = false;
    this._totalSelectedRows = 0;
    this._totalRows = 0;
    this._parentSelectsSubGrid = false;
    this._selectedCountFormat = null;
    this._columnModel = null;
    this._hideEmptySubGrid = false;
    this.locales = {};
    this._validation = null;
    this._vscroll = 0;
    this._addEnabled = false;
    this._templateContext = null;
    this._visibleRows = 0;
    this._rowHeight = 30;
    this._filter = '';
    this._isTree = false;
    this._sequenceMode = "None";
    this._sequenceColumn = null;
    this._seqClassseqClass = null;
    this.DataLoaded = null;
    this._showAllExpand = false;
    this._editingControls = [];
    this._multiTypes = false;
    this._thExtender = '';
    this._scrollWrapping = null;
    this._ajaxEntryUrl = null;
    this._lock = false;
    this._ignoreInlineEditing = true;
    this._customRowStyling = false;
    this._pagermode = null;
    this._statusToDisplay = null;
    this._durationUpdateInterval = null;
    this._responsiveWidth = null;
    this.IsResponsive = false;
    this._isHorizonStyle = false;
    this._oldWindowSize = {};
    this._navAddButtonClicks;

    // Const
    this.__subGridContextSeparator = "((ROW))";
    this.__emptyRowPrefix = '#empty#';
    this.__cellNotEnteredAttribute = "cell-not-entered";    
    this.__checked = "checked";
    this.__wrapCellTimeout = 400; // ms
}

CamstarPortal.WebControls.JQGridBaseData.prototype =
{
    initialize: function ()
    {
        CamstarPortal.WebControls.JQGridBaseData.callBaseMethod(this, 'initialize');

        $("#mod").show();
        this._isHorizonStyle = document.body.classList.contains("Horizon-theme");
        var me = this;
        var initialSettings = __json(this._initialSettings);
        this._assignEvents(initialSettings);

        if (initialSettings.options.refreshinterval)
            this._durationUpdateInterval = initialSettings.options.refreshinterval * 1000;

        var textFlyout = $('#textflyout');
        if (textFlyout.length == 0) {
            $('<div id=textflyout class="text-flyout' + (this._isHorizonStyle ? '">' : ' ui-jqgrid-row-yellow">') +
                    '<div class="pointer-top"></div>' +
                    '<div class="wrap ui-helper-clearfix">' +
                    '<div class="content"></div>' +
                '<div class="footer"><div class="text" /><div class="close"><img src="' + (this._isHorizonStyle ?
                    'Themes/Horizon/images/icons/flyout-close.svg"' :
                     'Themes/Camstar/images/icons/icon-close-16x16.png"') + ' /></div></div>' +
                    '</div>' +
                    '<div class="pointer-bottom"></div>' +
                    '</div>')
                .appendTo('body');

            $('.pointer-top, .pointer-bottom').attr('css3', '');
        }
        textFlyout.hide();
        if (this._isHorizonStyle) {
            // change some locales
            var defaultLocals = initialSettings.locales.filter(function (l) { return !!l.defaults; })

            defaultLocals.forEach(function (d) {
                if (d.defaults.recordtexthorizon)
                    d.defaults.recordtext = d.defaults.recordtexthorizon;
                else if (d.defaults.pgtexthorizon)
                    d.defaults.pgtext = "<span class='ui-pg-text'>" + d.defaults.pgtexthorizon + "</span>{0}";
            });
        }

        $.jgrid.loadLocales(this, initialSettings);

        if (initialSettings.options.loadError)
        {
            if( typeof __page !== 'undefined')
                __page.displayStatus(initialSettings.options.loadError, "Error");
        }

        if (this._columnModel)
        {
            initialSettings.options.colModel = this._columnModel;
            this._columnModel = null;
        }

        var fontSize = $(document.body).css("fontSize");
        if (this._isHorizonStyle && $(document.body).hasClass("desktop-device") && (fontSize && parseFloat(fontSize) >= 15)) {
            // Increase width in 1.5 times for horizon and 
            this.widthMultiplier = 1.5;
            var cm = [];
            initialSettings.options.colModel.forEach(function (c) {
                if (typeof c.width == "number" && c.width > 0) {
                    c.initialWidth = c.width;
                    c.width = c.width * me.widthMultiplier;
                }
                cm.push(c);
            });
            initialSettings.options.colModel = cm;
        }

        var theGrid = jQuery(this.GridID);
        this._isSubGridInstance = (this._contextID && this._contextID.indexOf(this.__subGridContextSeparator) != -1);
        this._hideEmptySubGrid = initialSettings.HideEmptySubGrid;
        this._parentGridID = initialSettings.ParentGridID;
        this._multipleChildAdding = initialSettings.MultipleChildAdding;
        this._templateContext = initialSettings.TemplateContextID;
        if (initialSettings.options.visibleRows != null)
            this._visibleRows = initialSettings.options.visibleRows;
        if (initialSettings.options.grouping === true)
            this._visibleRows = null;

        initialSettings.options.height = "100%";
        this.IsResponsive = initialSettings.options.responsiveEnv;

        this._ajaxEntryUrl = $.jgrid.buildAjaxUrl();

        if (initialSettings.options.treeGrid)
        {
            if (!initialSettings.options.sortname)
                initialSettings.options.sortname = "_id_column";
        }
        initialSettings.options.scrollOffset = $.jgrid.scrollbarWidth();

        theGrid.jqGrid(initialSettings.options);
        theGrid.setGridParam(
            {
                serializeRowData: function (postdata)
                {
                    var rowid;
                    var callback;
                    if (postdata)
                    {
                        if (postdata._rowid)
                        {
                            rowid = postdata._rowid;
                            delete postdata._rowid;
                        }
                        if (postdata._id_column)
                            delete postdata._id_column;
                        if (postdata.oper)
                            delete postdata.oper;

                        // correct value for enum
                        for(var p in postdata)
                        {
                            var cp = $(this).getColProp(p);
                            if (cp && cp.editoptions && cp.editoptions.editortype == "JQDropDownList")
                            {
                                var comp = me.GetInlineComponent(p);
                                if (comp)
                                {
                                    var newVal = comp.resolveTextToEnum(postdata[p]);
                                    postdata[p] = newVal;
                                }
                            }
                        }
                    }
                    var serializedPostData = JSON.stringify(postdata, me.jsonReplacer);
                    var rs = $.jgrid.buildAjaxRequestData(me, serializedPostData, 'SaveDataRow', rowid, callback);
                    return JSON.stringify(rs);
                },
                altRows: initialSettings.options.altRows,
                onSelectRow: function (r, s, e) { return me._onSelectRow(r, s, e); },
                _initialWidth: this._responsiveWidth ? undefined : initialSettings.options.width 
            });

        if (me.get_editingMode() == "Inline")
        {
            // Disable default mouse click 
            theGrid.setGridParam(
            {
                    beforeSelectRow: function(rowid, e)
                    {
                    var $t = $(e.target);
                    if ($t.is('[chevron]')) {
                        $.jgrid.clickOnChevron(e, $t);
                        return false;
                    }

                    if ($t.is("td.wrapped") && e.clientX > ($t.offset().left + $t.width() - 24)) {
                        $.jgrid.clickOnChevron(e, $("[chevron]", $t));
                        return false;
                    }

                    if ($t.hasClass('jqcboxrow')) {
                        // ignore click on cell with checkbox on the most left column
                        return false;
                    }

                    if (e.target && $(e.target).hasClass('ui-jqgrid-sequence-cell') || $(e.target).hasClass('cbox') || $(e.target).hasClass('ui-jqgrid-row-selector'))
                    {
                        // this is left selector image or selection chekbox . click allowed
                        if ($('tr[id="' + rowid + '"]', me.get_element()).attr('editable') == '1')
                            return false;
                        else
                            return !me.isRowEmpty(rowid);
                    }
                    else
                    {
                        // start inline edititng by click
                        me._onSelectRow(rowid, true, e);
                        return false;
                    }
                }
            });
        }
        else
        {
            // Disable default mouse click 
            theGrid.setGridParam(
            {
                beforeSelectRow: function (rowid, e) {
                    var $t = $(e.target);
                    if ($t.is("span[chevron]")) {
                        $.jgrid.clickOnChevron(e);
                        return false;
                    }

                    if ($t.is("td.wrapped") && e.clientX > ($t.offset().left + $t.width() - 24)) {
                        $.jgrid.clickOnChevron(e, $("[chevron]", $t));
                        return false;
                    }

                    if ($t.hasClass("jqgroup")) {
                        $.jgrid.clickOnChevron(e);
                        return false;
                    }

                    if ($t.is('div.treeclick'))
                    {
                        var record = theGrid.getRowData(rowid);
                        if (typeof record["expanded"] == "string")
                            record["expanded"] = (record["expanded"] == "true");
                        if (record["expanded"] === true)
                        {
                            theGrid.collapseRow(record);
                            theGrid.collapseNode(record);
                        }
                        else
                        {
                            theGrid.expandRow(record);
                            theGrid.expandNode(record);
                        }
                        record["expanded"] = (record["expanded"] ? "true" : "false");
                        theGrid.setRowData(rowid, record);
                        return false;
                    }

                    if (me.isRowEmpty(rowid))
                        return false;

                    if (me.get_rowSelectionMode() == "Disable")
                        return false;

                    if (me.get_rowSelectionMode() == "CheckBox")
                    {
                        if(!$t.is(':checkbox'))
                            return false;
                    }

                    return true;
                }
            });
        }

        if (initialSettings.nav.length > 0 && initialSettings.nav[0].add == true)
            this._addEnabled = true;

        var navr = Array.clone(initialSettings.nav);
        Array.insert(navr, 0, this.PagerID);
        Array.insert(navr, 0, theGrid);
        Function.call.apply(theGrid.navGrid, navr);
        delete navr;

        $.jgrid.setupHeaderButtons(this, initialSettings);

        if (initialSettings.navadds)
        {
            me._navAddButtonClicks = {};
            for (var i = 0; i < initialSettings.navadds.length; i++)
            {
                var it = initialSettings.navadds[i];
                if (it.itemtype == 'separator')
                    theGrid.navSeparatorAdd(this.PagerID);
                else if (it.itemtype == 'button') {
                    theGrid.navButtonAdd(this.PagerID, {
                        caption: it.caption,
                        buttonicon: it.buttonicon,
                        onClickButton: me.navButtonExecute,
                        position: it.position,
                        title: it.title,
                        cursor: it.cursor,
                        id: "navadd"+i
                    });
                    me._navAddButtonClicks["navadd" + i] = it.onClickButton;
                }
            }
        }

        var hiddenColumnCount = 0;

        // Replace celattr function name with function object
        var columnModel = theGrid.getGridParam('colModel');
        Array.forEach(columnModel,
            function (cm)
            {
                // assign custom handlers to checkboxes
                if (cm.CustomScriptHandlers)
                {
                    var fun = null;
                    if (cm.CustomScriptHandlers.click)
                    {
                        fun = $.jgrid.GetCompiledFunction(me, cm.CustomScriptHandlers.click, cm.name + 'click');
                        if(fun)
                            me._controlCustomHandlers[cm.name] = {click: fun};
                    }
                    if (cm.CustomScriptHandlers.change)
                    {
                        fun = $.jgrid.GetCompiledFunction(me, cm.CustomScriptHandlers.change, cm.name + 'change');
                        if(fun)
                            me._controlCustomHandlers[cm.name] = { change: fun};
                    }
                    if (cm.CustomScriptHandlers.onAddCellAttributes)
                    {
                        fun = $.jgrid.GetCompiledFunction(me, cm.CustomScriptHandlers.onAddCellAttributes, cm.name + 'cellattr');
                        if (fun)
                            me._controlCustomHandlers[cm.name] = { cellattr: fun };
                    }                           
                }
                if (cm.hidden)
                    hiddenColumnCount++;

                if (cm.editoptions != null && cm.edittype == "custom")
                {
                    var ed = cm.editoptions;
                    ed.custom_element = eval(ed.custom_element); //createPicklistControl;
                    ed.custom_value = eval(ed.custom_value); //getSetValue;
                    ed.ColumnName = cm.index;
                    ed.GridID = me._gridID;
                }
                cm.cellattr = function (rowid, val, rawObject, column, rdata)
                {
                    var attrs = new Array();
                    if (rowid && rowid.indexOf(me.__emptyRowPrefix) == 0)
                    {
                        // skip attributes for empty rows
                    }
                    else
                    {
                        if (column.editoptions)
                        {
                            if (column.editoptions.editortype == "JQFieldCheckBox")
                                attrs.push(column.editable ? "editable-checkbox" : "checkbox");

                            if (column.name != me._sequenceColumn && column.editoptions.required && (val == null || val == '' || val == undefined || val == '&#160;'))
                                attrs.push(me.__cellNotEnteredAttribute);   
                        }

                        if (me._controlCustomHandlers[column.name] && me._controlCustomHandlers[column.name].cellattr)
                        {
                            var a = me._controlCustomHandlers[column.name].cellattr(rowid, val, rawObject, column, rdata);
                            if (a)
                                attrs.push(a);
                        }
                    }
                    if (attrs.length)
                        return attrs.join(' ');
                    else
                        return null;
                }
           }, null);

        theGrid.setGridParam({ loadonce: false });
        this._isTree = theGrid.getGridParam('treeGrid');

        // Replace edit page number with select 
        var $pgInput = $(this.GridID + "_pager .ui-pg-input");
        if ($pgInput.length != 0) {
            $pgInput.after("<select class=ui-pg-input />");
            $pgInput.unbind();
            $pgInput.remove();
            var $s = $(this.GridID + "_pager .ui-pg-input");
            $s.parent().addClass("page-selector");
            $s.change(
                function (e) {
                    me._onPaging(e.target.value); 
                });
        }

        // fix pager - navigator layout
        var $pg = $('#pg_' + this._gridID + '_pager');
        $('> table', $pg).css('table-layout', '');
        $('td:first', $pg).width('100%');
        this._pagermode = initialSettings.options.pagermode;

        if (this._isHorizonStyle) {
            // Swap cells in the pager area
            var $pgTR = $('>.ui-pg-table >tbody >tr', $pg);
            if ($pgTR.length) {
                var msgTD = $(">td:last-child", $pgTR).detach();
                msgTD.addClass("msg-area");
                $(">td:eq(0)", $pgTR).addClass("pager-area");
                $(">td:eq(0)", $pgTR).before(msgTD);
            }
        }

        // The multiple check box in the header
        this.multiCheckBox = $('#gbox_' + this._gridID + ' [role=columnheader] :checkbox');
        if (this.multiCheckBox.length == 0)
            this.multiCheckBox = null;

        theGrid.setGridParam({ onClickGroup: function (t, h, s) { return me._onGroupClick(t, h, s); } });

        $(".ui-jqdialog-content").each(function ()
        {
            if ($('.ui-jqdialog-footer').length == 0)
            {
                $('<div class="ui-jqdialog-footer"><input type="button" id="popup_ok_button" value="OK" class="cs-button"></div>').insertAfter(".ui-jqdialog-content");
                $('#popup_ok_button').click(function () { $('[id^="alertmod"]').hide(); });
                $('#alertmod').css('position', 'absolute');
            }
        });

        var initData = __json(this.get_initialData());
        this.set_initialData(null); // dispose cached string

        if (initData.ParentGridID)
            this._parentGridID = initData.ParentGridID;

        if (this._showAllExpand)
        {
            // Insert as a first column
            var gview = $('#gview_' + this._gridID);
            var th1 = $('.ui-jqgrid-hdiv th:first-child', gview);
            var globallyCollapsed = theGrid.getGridParam('groupingView').groupCollapse;
            var td1 = $('tr.jqgfirstrow td:first-child', gview);

            $('<th><div titleOn="Expand All" titleOff="Collapse All" /></th>').insertBefore(th1);
            $('<td></td>').insertBefore(td1);

            th1 = $('.ui-jqgrid-hdiv th:first-child', gview)
                .attr('id', this._gridID + '_expandall')
                .width(20)
                .children('div')
                .attr('id', 'jqgh_' + this._gridID + '_expandall')
                .attr('expanded', (!globallyCollapsed).toString())
                .click(
                function ()
                {
                    var isExpanded = $(this).attr('expanded');
                    isExpanded = isExpanded == "false" ? "true" : "false";
                    $(this).attr('expanded', isExpanded);
                    $(isExpanded == "false" ? '.ui-icon-circlesmall-minus' : '.ui-icon-circlesmall-plus', theGrid)
                    .each(function (ind, g_span)
                    {
                        me.__ignoreGroupClick = true;
                        theGrid.groupingToggle($(g_span).parent().parent().attr('id'));
                        delete me.__ignoreGroupClick;
                    });
                });

            $('tr.jqgfirstrow td:first-child', gview).width(20);
        }

        if (this.get_editingMode() == "Inline")
        {
            // disable edit nav button
            $('#edit_' + this._gridID).remove();

            // Hide "No selected row" alert
            $('#del_' + this._gridID).mousedown(function ()
            {
                me.__old_jgrid_viewModal = $.jgrid.viewModal;
                $.jgrid.viewModal = function (id, options) { /*do nothing*/ };
            })
            .click(function ()
            {
                $.jgrid.viewModal = me.__old_jgrid_viewModal;
            });

            var edSettings = this.get_editorSettings();
            if (edSettings && edSettings.AddRowMode === 1 && this._addEnabled === true)
            {
                this._addEnabled = false;
                $('#add_' + this._gridID).hide();
            }
            theGrid.focus(function (e, p)
            {
                if (me.__ignoreFocus === true)
                {
                    delete me.__ignoreFocus;
                    return false;
                }
                else
                {
                    if (typeof p == "object") {
                        var $r = me.$getRow(p.rowid);
                        if ($r.length)
                            $r.click();
                    }
                    if (p === "tabbing") {
                        var $firstRow = $('tr.jqgrow', theGrid).first();
                        if ($firstRow.length && $firstRow.prop("id") != "#empty#0") {
                            $firstRow.click();
                            return true;
                        }
                        return false;
                    }
                    return true;
                }
            });

            theGrid.keydown(
                function (e) { return me._keyDownProcessing(e); }
            );
        }
        else if (this.get_editingMode() == "LitePopup")
        {
            this._divEditorID = "#WebPart_" + this._editorSettings.EditorContainer;
            if (!$(this._divEditorID).parent().hasClass("ui-dialog")) {
                // Hide if web part is not inside active dialog
                $(this._divEditorID).hide();
            }

            if (this._editorSettings.SubClassMap)
            {
                Array.forEach(this._editorSettings.SubClassMap, function (sc) {
                    // Do not hide if dialog is active
                    var $edWp = $("#WebPart_" + sc.EditorWebPart);
                    if (!$edWp.parent().hasClass("ui-dialog"))
                        $edWp.hide();
                    $edWp.addClass('litepopup-webpart');
                });
                this._multiTypes = (this._editorSettings.SubClassMap.length > 1);
            }
        }
        else if (this.get_editingMode() == "Popup")
        {
            if (this._editorSettings.SubClassMap)
            {
                this._multiTypes = (this._editorSettings.SubClassMap.length > 1);
            }
        }

        theGrid.unbind('contextmenu');

        if (this._isHorizonStyle)
            $.jgrid.initHorizonSelectAllCheckBox($get('gview_' + this._gridID));

        // Fix check box column
        $(this.GridID + "_cb").filter("th").css('padding-left', '0');
        var isCheckBox = (this.multiCheckBox != null && this.multiCheckBox.length > 0);
        if (isCheckBox) 
            $('tr.jqgfirstrow td:first-child', theGrid).addClass('jqfirstcol');

        this.Reload_complete(null, initData.ResponseData);
        initData.ResponseData = null;

        if (initData.BubbleMessage)
            this.showMessage(initData.BubbleMessage);

        if (isCheckBox)
            $(".jqgrow .cbox", theGrid).parent().addClass('jqcboxrow');

        // Hide subgrid template
        if (this._subGridId)
        {
            setTimeout(function ()
            {
                if ($("#gbox_" + me._subGridId).parent().children(":visible").length == 0)
                {
                    $("#gbox_" + me._subGridId).parent().addClass("empty");
                }
            }, 1);
        }

        if (this._parentGridID && !this._isSubGridInstance)
        {
            $('#gbox_' + this._gridID).hide();
        }

        if (this._hidden)
            $('#gbox_' + this._gridID).hide();

        if (initData.ForceReload != undefined && initData.ForceReload == true)
        {
            setTimeout(function () { me.Reload(); });
            initData.ForceReload = false;
        }
        
        // fix the column with expand/collapse button when subgrids exist
        $("#" + this._gridID + "_subgrid").css('width', theGrid[0].p.subGridWidth + "px");
        var expandImageColumnPos = isCheckBox ? 2 : 1;

        if ($("td[aria-describedby=\"" + this._gridID + "_subgrid\"]").length > 0)
            $("#" + this._gridID + " tr.jqgfirstrow td:nth-child(" + expandImageColumnPos + ")")
                .not("#" + this._gridID + " .ui-jqgrid-subgrid-child tr.jqgfirstrow td:nth-child(" + expandImageColumnPos + ")")
                .css('width', theGrid[0].p.subGridWidth + "px");

        // fix colspan for grouped row when hidden columns exist
        $(".jqgroup td", theGrid).attr("colspan", columnModel.length - hiddenColumnCount);

        // hide span for the grid 
        $('span[id=' + this._gridID + ']').hide();

        // fix separator that has hardcoded width
        $('.navtable td.ui-state-disabled').attr('style', 'width:0px');

        if (this._editingMode == 'LitePopup' && this._editorSettings != null && this._editorSettings.ColumnControlsMap != undefined)
        {
            $('body.body-main').children('.webpart')
                .each(function () { $(this).remove(); });
        }

        if (this._editingMode == 'FormControls')
        {
            this._setupActionHandler(false, false);
        }

        // add scrolling handler to viewport div
        $('#gview_' + this._gridID + ' div.ui-jqgrid-bdiv').scroll(function (){
            $.jgrid.wrapCells(me, null);
        });

        // hide popup when scrolling page
        $(".scrollable-panel").scroll(function () {
            $.jgrid.wrapCells(me, null);
        });

        $("#mod").hide();

        // attach to accordion handler
        var $accrd = $(this.get_element()).closest('div.webpart,span.accordion');       // (webpart selector added to limiting search in case of no accordion )
        if ($accrd.hasClass('accordion'))
        {
            $accrd[0].control.add_selectedIndexChanged(function (acc, p)
            {
                var pane = acc._panes[p.get_selectedIndex()];
                if ( $(pane.content).find(me.get_element()).length)
                {
                    // the grid belongs to expanded pane
                    if ($(pane.header).hasClass('accordionHeaderSelected'))
                    {
                        // the grid is refreshed on expanding                        
                        if (me._responsiveWidth)
                            me.resize();
                        else
                            $.jgrid.wrapCells(me, null);
                    }
                }
            });
        }

        // attach to the toggle container 
        var $tcont = $(this.get_element()).closest('div.toggle-container').parent(); // this will points to the <span> of toggle container
        if ($tcont.length)
        {
            var tc = $tcont[0].control;
            tc.add_onExpanded(function () {
                me.resize();
            });
        }

        jqGrid_instances[this._gridID] = this;

        if (this._responsiveWidth) {

            var contWindow =
                $(document.body).hasClass("body-float") ? window : getCEP_top(); 

            $(contWindow).on("resize", function (e, p) {
                // There is redirected event. It's ignored
                if (p && p === "force-top-resize")
                    return;

                if (me._resizeTimer)
                    clearTimeout(me._resizeTimer);
                me._resizeTimer = setTimeout(function () { me.resize(); }, 400);                
            });

            if ($(document.body).hasClass("body-float"))
                me._resizeTimer = setTimeout(function () { me.resize(); }, 600);
        }

        if (this._isHorizonStyle) 
            $.jgrid.horizonStyling(this);
    },

    dispose: function () {

        if (this.get_editorSettings() && this.get_editorSettings().ColumnControlsMap) {
            var ctrlIDs = this.get_editorSettings().ColumnControlsMap
                .map(function (c) {
                    return c.EditorClientId;
                });
            $(this.get_element()).data("controls", ctrlIDs);
        }

        $('tr[role=row] td :checkbox', this.get_element()).unbind();
        $('#del_' + this._gridID).unbind();
        $(this.GridID + "_pager td.ui-pg-button span.ui-icon").unbind();
        $(this.GridID + "_pager .ui-pg-input").unbind();
        $('#gview_' + this._gridID + ' div.ui-jqgrid-bdiv').unbind();
        $(".scrollable-panel").unbind('scroll');

        if (this._isHorizonStyle)
            $.jgrid.disposeHorizonCheckboxes($get("gview_" + this._gridID));

        $(this.GridID).GridDestroy();
        delete jqGrid_instances[this._gridID];

        this._gridID = null;
        this._eventTargetId = null;

        this.GridID = null;
        this.PagerID = null;
        this.wrapEverything = null;
        this.wrapCellTimer = null;

        this._initialSettings = null;
        this._initialData = null;

        this._serverType = null;
        this._contextID = null;

        this._divEditorID = null;

        this._editorSettings = null;

        this._last_selectedRowID = null;
        this._subGridId = null;

        this._isSubGridInstance = null;
        this._parentGridID = null;
        this._collapseOtherOnExpanding = null;
        this._customEventHandlers = null;
        this._hidden = null;
        this._width = null;
        this._justSelect = false;
        this._controlCustomHandlers = null;
        this._subgridMarkIndex = null;
        this.multiCheckBox = null;
        this._rowSelectionMode = null;
        this._shrinkColumns = null;
        this._expandGroupIfRowSelected = null;
        this._dataChanged = null;
        this._totalSelectedRows = null;
        this._totalRows = null;
        this._parentSelectsSubGrid = null;
        this._selectedCountFormat = null;
        this._columnModel = null;
        this._hideEmptySubGrid = null;
        this.locales = null;
        this._validation = null;
        this._vscroll = null;
        this._addEnabled = null;
        this._templateContext = null;
        this._visibleRows = null;
        this._rowHeight = null;
        this._filter = null;
        this._isTree = null;
        this._sequenceMode = null;
        this._sequenceColumn = null;
        this._seqClassseqClass = null;
        this.DataLoaded = null;
        this._showAllExpand = false;
        this._editingControls = null;
        this._multiTypes = null;
        this._thExtender = null;
        this._scrollWrapping = null;
        this._ajaxEntryUrl = null;
        this._lock = null;
        this._ignoreInlineEditing = null;
        this._customRowStyling = null;
        this._pagermode = null;
        this._statusToDisplay = null;
        this._durationUpdateInterval = null;
        this._responsiveWidth = null;
        this.IsResponsive = null;
        this._isHorizonStyle = null;
        this._oldWindowSize = null;
        this._navAddButtonClicks = null;

        // Const
        this.__subGridContextSeparator = null;
        this.__emptyRowPrefix = null;
        this.__cellNotEnteredAttribute = null;
        this.__checked = null;
        this.__wrapCellTimeout = null;

        Camstar.UI.Control.callBaseMethod(this, 'dispose');
    },

    Invoke: function (func)
    {
        setTimeout("$(\"" + this.GridID + "\")[0].control." + func, 1);
    },

    SetFilter: function (filter)
    {
        this._filter = filter;
    },

    Reload: function (force)
    {
        var p = {};
        if (typeof force === "boolean" && force === true)
            p.ActionID = "refr";
        this._sendRequest(p, "Reload", "Reload_complete");
    },

    Reload_complete: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        var me = this;

        var selectedRowAfterPostback;

        if (window["selectedGridRowAfterPostback"] && window["selectedGridRowAfterPostback"].grid === this._gridID) {
            selectedRowAfterPostback = window["selectedGridRowAfterPostback"].rowid || "*";
            delete window["selectedGridRowAfterPostback"];
        }

        if (responseData) {
            var visibleRows;
            var theGrid = jQuery(this.GridID);
            var $gview = $('#gview_' + this._gridID);

            theGrid.jqGrid("clearGridData", true);
            theGrid.setGridParam({ datatype: "json"});
            var respData = __json(responseData);
            jqgrid_lock(false);

            if (respData.total == 0)
                $("#edit_" + theGrid[0].id).addClass('ui-state-disabled');
            else
                $("#edit_" + theGrid[0].id).removeClass('ui-state-disabled');

            // Clear grouping settings (because they are duplicated after refreshing )
            if (theGrid[0].p && theGrid[0].p.groupingView) {
                theGrid[0].p.groupingView.sortitems[0] = [];
                theGrid[0].p.groupingView.sortnames[0] = [];
                theGrid[0].p.groupingView.groupOrder[0] = '';
                theGrid[0].p.groupingView.groups = [];
            }
            if (theGrid[0].p.treeGrid)
                theGrid[0].p.treeANode = -1;

            this.__adjustDateTimeCells(respData);

            theGrid[0].addJSONData(respData);
            theGrid.setGridParam({ datatype: "local" });

            if (theGrid.getGridParam('treeGrid'))
                this._adjustTree(theGrid);

            var pagerVisible = (this._pagermode == "AlwaysVisible" || (this._pagermode == "Auto" && respData.total > 1));
            var pgTableSelect = this.GridID + '_pager table.ui-pg-table[role="row"] td table.ui-pg-table:first';
            // Adjust the pager
            if (pagerVisible) {
                var pgSelect = $(this.GridID + "_pager select[class='ui-pg-input']");
                if (pgSelect.length) {
                    if (respData.total != $('option', pgSelect).length) {
                        pgSelect.children().remove();
                        for (var i = 1; i <= respData.total; i++) {
                            pgSelect.append($("<option value=" + i + ">" + i + "</option>"));
                        }
                    }
                    pgSelect[0].selectedIndex = respData.page - 1;
                    pgSelect[0].disabled = (respData.total < 2);

                    // disable Next and End buttons
                    if (respData.total < 2 || (respData.total >= 2 && respData.page == respData.total))
                        pgSelect.parent().nextAll('td.ui-corner-all').addClass('ui-state-disabled');
                    else
                        pgSelect.parent().nextAll('td.ui-corner-all').removeClass('ui-state-disabled');

                    // Disable click on disabled buttons
                    $(this.GridID + "_pager td.ui-pg-button span.ui-icon")
                        .unbind('click')
                        .bind('click', function () {
                            return !$(this).parent().hasClass('ui-state-disabled');
                        });
                    $(pgTableSelect).show();
                }
            }
            else {
                $(pgTableSelect).hide();
            }

            pgTableSelect = null;

            this._totalSelectedRows = respData.totalSelectedRows;
            this._totalRows = respData.records;

            if (respData.totalSelectedRows != null)
                this.displaySelectedCount();

            this._ignoreInlineEditing = true;
            var columnModel = theGrid.getGridParam('colModel');
            columnModel.forEach(function (c) {
                if (c.editable && !c.hidden)
                    me._ignoreInlineEditing = false;
            });

            if (this._showAllExpand) {
                var firstCells = $('td[role=gridcell][aria-describedby]:first-child', $gview);
                $('<td style="width:20px" aria-describedby="' + this._gridID + '_expandall' + '">&nbsp;</td>').insertBefore(firstCells);
                setTimeout(me.__fixGroupHeaders, 1, [theGrid, me]);
            }

            if (responseData != "([])") {
                // Update settings 
                if (respData.colNames) {
                    for (var i = 0; i < respData.colNames.length; i++)
                        theGrid.jqGrid('setLabel', i, respData.colNames[i], null, null);
                }

                if (respData.attributes) {
                    var gbox = $('#gbox_' + this._gridID);
                    for (var k in respData.attributes) {
                        // Set grid root div to specified custom style
                        gbox.attr(k, respData.attributes[k]);
                    }
                }
                // Rearrange altRow coloring in case of sorting or grouping
                visibleRows = $(this.GridID + " tr[role=row][id].jqgrow");
                if (theGrid.getGridParam('altRows') == true) {
                    visibleRows.filter(':even').removeClass('ui-priority-secondary');
                    visibleRows.filter(':odd').addClass('ui-priority-secondary');
                }
                // Set attribute for empty rows
                visibleRows.filter('[id^="' + this.__emptyRowPrefix + '"]').attr('empty-row', '');

                // if adding is enable the first row set empty-row='0'
                if (this._addEnabled)
                    $(this.GridID + " tr[role=row][empty-row].jqgrow").first().attr('empty-row', '0');

                // Restore selected
                var selectedCount = 0;
                // Number of rows minus #empty rows
                var totalActiveRowCount = 0;

                respData.rows.forEach(
                    function (rx) {
                        if (!rx.id.startsWith(me.__emptyRowPrefix))
                            totalActiveRowCount++;

                        if (rx.selected != undefined && rx.selected == true) {
                            if (me._editingMode == "FormControls")
                                theGrid.setSelection(rx.id, true);
                            else
                                theGrid.setSelection(rx.id, false);

                            if (me._expandGroupIfRowSelected == true) {
                                // expand the group
                                var grpTr = null;
                                var theRow = me.$getRow(rx.id);
                                if (theRow.length > 0) {
                                    var grpTr = theRow.prev();
                                    while (!grpTr.is('tr.jqgroup')) {
                                        grpTr = grpTr.prev();
                                        if (grpTr.is('table')) {
                                            grpTr = null;
                                            break;
                                        }
                                    }
                                }
                                if (grpTr != null) {
                                    var grpId = grpTr.attr('id');
                                    if (respData.grpExpanded) {
                                        var found = false;
                                        for (var i = 0; i < respData.grpExpanded.length; i++) {
                                            if (respData.grpExpanded[i][0] == grpId) {
                                                respData.grpExpanded[i][1] = true;
                                                found = true;
                                            }
                                        }
                                        if (!found)
                                            respData.grpExpanded.push([grpId, true]);
                                    }
                                    else {
                                        respData.grpExpanded = [[grpId, true]];
                                    }
                                }
                            }
                            selectedCount++;
                        }

                        // Set rowtype index in case of multi-class 
                        if (rx.rowTypeIndex)
                            me.$getRow(rx.id).attr('rowTypeIndex', rx.rowTypeIndex);

                        // Set calculated row style
                        if (rx.rowNormalStyle) {
                            me._customRowStyling = true;
                            var tr = me.$getRow(rx.id);
                            tr.attr('rowNormalStyle', rx.rowNormalStyle);
                            tr.attr('rowSelectedStyle', rx.rowSelectedStyle);

                            $.jgrid.toggleRowStyle(tr, false);
                            if (rx.selected)
                                $.jgrid.toggleRowStyle(tr, true);
                        }
                    }, null
                );

                // Set multiple selected checkbox
                if (this.multiCheckBox) {
                    this.multiCheckBox[0].checked = (selectedCount > 0 && selectedCount == totalActiveRowCount);
                    this.stylingCheckboxes(this.multiCheckBox);
                }

                // Restore expanded
                if (theGrid.is(':visible')) {
                    respData.rows.forEach(function (rx) {
                        if (!me._isTree) {
                            if (rx.expanded && rx.expanded == true)
                                window.setTimeout('$(\"' + me.GridID + '\").expandSubGridRow(\"' + rx.id + '\")', 1);
                             
                        }
                    });
                }

                // Cell shading
                $('div[cellbackgroundcolor]', this.get_element()).each(
                    function () {
                        if ($(this).parent().parent().attr('aria-selected') != "true")
                            $(this).parent().css("background-color", $(this).attr("cellbackgroundcolor"));
                    });

                $('div[cellClass]', this.get_element()).each(
                    function () {
                        $(this).parent().addClass($(this).attr("cellClass"));
                    });


                // checkbox
                $('tr[role=row] td :checkbox', this.get_element()).each(
                    function () {
                        if ($(this).attr('role') == 'checkbox') {
                            $(this).click(
                                function () {
                                    var $r = $(this).closest('tr');
                                    if ($r.is('[empty-row]') || $r.is('[editable=1]'))
                                        return false;

                                    me._justSelect = true;
                                    if (this.checked == false) {
                                        // uncheck multiCheckBox if any row gets deselected
                                        if (me.multiCheckBox) {
                                            me.multiCheckBox[0].checked = false;
                                            me.stylingCheckboxes(me.multiCheckBox);
                                        }
                                    }
                                    return true;
                                });
                            $(this).css('cursor', 'pointer');
                        }
                        else if ($(this).parent().is('[editable-checkbox]') && me._editingMode != "LitePopup") {
                            $(this).removeAttr('disabled').click(
                                function () {
                                    if ($(this).parent().parent().is('[empty-row]'))
                                        return false;

                                    var rowDataObj = $(this).parent().attr('aria-describedBy').substr(me._gridID.length + 1);
                                    if (rowDataObj != '') {
                                        var callParameters = {
                                            RowID: $(this).parents('tr[role=row]').attr('id'),
                                            IsRowSelected: this.checked,
                                            RowDataObject: rowDataObj
                                        };
                                        me._sendRequest(callParameters, "ClickCheckbox", "ClickCheckbox_Completed");
                                    }
                                    $(this).toggleClass("cb-callback", rowDataObj != '');
                                    return true;
                                }
                            );
                        }
                    });

                if (respData.grpExpanded)
                    me.expandGroups(respData.grpExpanded);

                if (respData.autoSelectedRowID != undefined)
                    window.setTimeout(function () { me.autoSelectRow(respData.autoSelectedRowID); }, 1);

                // Height can be in pixels, percentag, points...
                if (respData.height) {
                    theGrid.setGridHeight(respData.height);
                }
                else if (me._visibleRows) // can be undefined
                {
                    var rowClearance = theGrid.hasClass('ui-jqgrow-parent') ? 1 : 0;
                    theGrid.setGridHeight(me._visibleRows * (me._rowHeight + rowClearance) + 1); // plus the firsts row's height - this is 1px.
                }

                var isResisable = $(".is-not-resizible-on-reload").find(this.GridID).length === 0;

                if (respData.width && respData.width > 0 && isResisable) {
                    if (this._width != respData.width) {
                        this._shrinkColumns = (respData.shrinkColumnWidthToFit && respData.shrinkColumnWidthToFit === true) ? true : false;
                        this._width = respData.width;

                        if (this._responsiveWidth && !document.body.classList.contains("desktop-device"))
                            theGrid.setGridWidth(100, this._shrinkColumns);
                        else
                            theGrid.setGridWidth(this._width, this._shrinkColumns);
                    }
                }
                else {
                    // get calculated width
                    this._width = theGrid.getGridParam('width');
                }

                // make the _extender_ column loosed ( without width )
                this._thExtender = $('th[id$="_extender_"]', $gview);
                if (this._thExtender.length) {
                    this._thExtender.width('auto');
                    var indexExtender = this._thExtender.index();
                    if (indexExtender != -1) {
                        $($('tr.jqgfirstrow td', theGrid)[indexExtender]).width('auto');
                    }
                }

                // fix frozen height
                if ($('div.frozen-div', $gview).length) {
                    var h = $(".ui-jqgrid-htable", $gview).height();
                    $('div.frozen-div', $gview).height(h);
                    var t = theGrid[0];
                    var top = t.p.caption ? $(t.grid.cDiv).outerHeight() : 0;
                    if (t.p.toppager) {
                        top += $($t.grid.topDiv).outerHeight();
                    }
                    if (t.p.toolbar[0] === true) {
                        if (t.p.toolbar[1] != "bottom") {
                            top += $(t.grid.uDiv).outerHeight();
                        }
                    }

                    $('div.frozen-bdiv', $gview)[0].style.top = (top + h) + 'px';
                }

                // Fix subgrid collapsed/expanded column
                if (this._subgridMarkIndex != -1) {
                    $('#gbox_' + this._gridID + ' .jqgfirstrow ').children()[this._subgridMarkIndex].style.width = '28px';
                }
            }

            // restore vertical row position
            var $viewDiv = $('.ui-jqgrid-bdiv', $gview);
            if ($viewDiv.length > 0 && !$viewDiv.is(':hidden'))
                $viewDiv.scrollTop(this._vscroll);

            if (this._isHorizonStyle)
                $.jgrid.initHorizonSelectCheckboxes($gview);

            if (visibleRows) {
                // Adjust "View {} Records" without empty rows
                //Moved this code out from the emptyRows.length>0 in order to set the tooltip 
                // regardless of the number of empty rows.
                var emptyRows = visibleRows.filter('[id^="' + this.__emptyRowPrefix + '"]');
                var rowsPerPage = theGrid.getGridParam('rowNum');
                var currentRecordsCount = theGrid.getGridParam('reccount');
                var from = (respData.records > 0) ? (rowsPerPage * (parseInt(respData.page) - 1) + 1) : 0;
                var tot = respData.records;
                var to = (respData.records > 0) ? (from + currentRecordsCount - emptyRows.length - 1) : 0;
                var recordTxtStr = $.jgrid.format(theGrid.getGridParam('recordtext'), from, to, tot);
                var viewRec = $(this.PagerID + ' .ui-paging-info');

                if (emptyRows.length > 0) {
                    if (viewRec.length > 0) {
                        if (tot > 0) {
                            viewRec.html(recordTxtStr);
                        }
                        else {
                            var emptyrecTextFormat = theGrid.getGridParam('emptyrecords');
                            viewRec.text(emptyrecTextFormat);
                            recordTxtStr = theGrid.getGridParam('emptyrecords');
                        }
                    }
                    emptyRows.children('td').removeAttr('cell-not-entered');
                    emptyRows.find('[role=checkbox]').hide();
                    emptyRows.find('.ui-icon-collapsed').parent().hide();
                }
                $(viewRec).attr('title', recordTxtStr);
                if (this._sequenceMode != "None") {
                    this.setRowPoinerCell(theGrid, columnModel);
                }

                // Set original data and title
                $('td[role=gridcell]', theGrid).each(function () {
                    var txt = $(this).text();
                    $(this).attr('title', txt).data('val', txt);
                });
            }

            this.setNavButtonsState();

            // Identify rows with subgrids
            $(".ui-sgcollapsed", $("#" + this._gridID)).parent().addClass("ui-jqgrow-parent");

            this.RunCustomHandler('renderCompleted', true, null, null);

            if ($.isFunction(this.DataLoaded)) {
                this.DataLoaded();
            }

            if (respData.permutation != null) {
                // now permutation is a list of columns
                var perm = [];

                Array.forEach(respData.permutation, function (cname) {
                    perm.push(me.__getColIndex(columnModel, cname));
                }, null);
                theGrid.remapColumns(perm, true, false);
            }


            var firstSelected = $('tr[aria-selected=true]', theGrid[0]);
            if (firstSelected.length > 0) {
                var container = $('.ui-jqgrid-bdiv', $gview);
                if (container.length > 0)
                    // scroll selected row to view.
                    container.scrollTop(firstSelected.offset().top - container.offset().top + container.scrollTop() - container.height() / 2);
            }

            var $gbox = $('#gbox_' + this._gridID);
            $('.grid-cell-action-button, .grid-cell-link-action', $gbox)
                .on('click', function () {
                    me._onCellActionClick(this);
            });

            $('.grid-cell-flyout-action', $gbox)
                .filter(function () { return typeof window[$(this).attr("flyoutMethod")] == "function"; })
                .on('click', function () {
                    return window[$(this).attr("flyoutMethod")](this);
                });

            $.jgrid.wrapCells(me);

            if (this._multiTypes)
            {
                // reassign the add button
                var $addBtnContainer = $('#add_' + this._gridID);
                $addBtnContainer.unbind('click');
                $addBtnContainer.click(function () { me._multiTypesAdd($addBtnContainer); return false; });
            }

            var durationCells = $("td[jqgrid-duration-autoupdate]", $(this.GridID));
            if(durationCells.length)
            {
                $.jgrid.UpdateDuration(true, this);
            }

            if (this.isMultiPickList)
                $(this.get_parent().get_element()).toggleClass('pager-footer', (theGrid.getGridParam('rowNum') >= this._totalRows));

            this.resize();

            if (typeof selectedRowAfterPostback != "undefined") {
                if (Camstars.Browser.IE) {
                    setTimeout(function () {
                        theGrid.triggerHandler("focus", { rowid: selectedRowAfterPostback });
                    }, 500);
                }
                else {
                    theGrid.triggerHandler("focus", { rowid: selectedRowAfterPostback });                // Just set focus
                }
            }
        }
    },

    RunCustomHandler: function (eventName, isAtTheEndOfProcessing, prm1, prm2)
    {
        if (this._customEventHandlers)
        {
            for (var i = 0; i < this._customEventHandlers.length; i++)
            {
                if (this._customEventHandlers[i].Name == eventName)
                {
                    var fun = this._customEventHandlers[i].Binary;
                    if (fun == undefined)
                    {
                        var handler = this._customEventHandlers[i].Handler;
                        fun = $.jgrid.GetCompiledFunction(this, handler, eventName);
                        this._customEventHandlers[i].Binary = fun;
                    }
                    if (fun != undefined)
                    {
                        return fun.call(this, isAtTheEndOfProcessing, prm1, prm2);
                    }
                }
            }
        }
        return true;
    },

    getValue: function ()
    {
        return null;
    },

    setValue: function (val)
    {
        if (val != null && val != "")
            this.Reload_complete(null, val);
    },

    GetWidth: function () {
        return $("#gbox_" + this._gridID).width();
    },

    SetWidth: function (w) {
        return $(this.GridID).setGridWidth(w, this._shrinkColumns);
    },

    GetInlineComponent: function (id)
    {
        var idComp = this.get_controlId() + '_' + id + '_InlineEditorControl';
        return $find(idComp);
    },

    isRowEmpty: function (rowid_or_row)
    {
        var rowid="";
        if ($.isArray(rowid_or_row))
            rowid = rowid_or_row.prop('id');
        else
            rowid = rowid_or_row;

        return rowid.startsWith(this.__emptyRowPrefix);
    },

    setNavButtonsState : function()
    {
        var $del = $('#del_' + this._gridID);

        if ($del.is(':visible'))
        {
            // disable Delete buttons if no rows
            var selectedOnCurrentPage = $('tr[aria-selected=true]', this.get_element()); //$('td input.cbox:checked', this.get_element());
            if (selectedOnCurrentPage.length == 0 || this._onStartInlineEditing)
                $del.addClass('ui-state-disabled');
            else
                $del.removeClass('ui-state-disabled');
        }
    },

    navButtonExecute: function (e) {
        if (this.id) {
            var grid = $find(this.id);
            if (grid) {
                var idNav = $(e.target).closest("td").prop("id");
                if (idNav) {
                    var navFunctionName = grid._navAddButtonClicks[idNav];
                    var fun = navFunctionName && grid[navFunctionName];
                    if (fun)
                        return fun.call(grid, e);
                    else
                        console.error("Unknown grid function " + navFunctionName)
                }
            }
        }
        return false;
    },

    $getRow: function(rowid)
    {
        return $('tr[role="row"][id="' + rowid + '"]', this.get_element());
    },

    reorderRows: function ($row, direction)
    {
        // find new index of the row
        var newid = $row.next().prop('id');
        if ($row.next().length == 0 || newid.substr(0, this.__emptyRowPrefix.length) == this.__emptyRowPrefix)
        {
            // empty row =  insert at the end
            newid = 1;
        }

        if (direction != 0)
        {
            newid = direction;
        }

        var callParameters = { ActionID: 'dnd', RowID: $row.prop('id'), RowDataObject: JSON.stringify(newid, this.jsonReplacer) };
        this._sendRequest(callParameters, "ReorderRows", "Reload_complete");
    },

    _assignEvents: function (initialSettings) {
        var me = this;
        var opt = initialSettings.options;

        opt.onPaging = function (b) { return me._onPaging(b); };
        opt.onSortCol = function (colId, iCol, sortord) { return me._onColumnSorting(colId, iCol, sortord); };
        opt.resizeStop = function (newwidth, ind) { return me._onResizeColumn(newwidth, ind); };
        if (opt.onSelectAll)
            opt.onSelectAll = function (rids, stat) { return me._onSelectAll(rids, stat); };

        opt.subGridBeforeExpand = function (subgrid_id, row_id) { return me._onSubGridBeforeExpand(subgrid_id, row_id); };
        opt.subGridRowExpanded = function (subgrid_id, row_id) { return me._onSubGridExpanded(subgrid_id, row_id); };
        opt.subGridRowColapsed = function (subgrid_id, row_id) { return me._onSubGridCollapsed(subgrid_id, row_id); };

        initialSettings.nav.forEach(function (n) {
            if (n.beforeRefresh)
                n.beforeRefresh = function () { return me._beforeRefresh() };
            if (n.addfunc)
                n.addfunc = function () { return me._onAddRow() };
            if (n.editfunc)
                n.editfunc = function (rowId) { return me._onEditRow(rowId); };
            if (n.delfunc)
                n.delfunc = function (rowId) { return me._onRowDelete(rowId); };
        });
    },

    _addDataRow: function (typeIndex, $r)
    {
        var $editingRow = [];
        var me = this;

        if (typeof typeIndex == "undefined")
            typeIndex = 0;

        // find if some row is in editing then it should be saved
        if ($r && $r.is('[editable=1]'))
        {
            $editingRow = $r;
        }
        else
        {
            $editingRow = $('tr[editable=1]', this.get_element());
        }
        if ($editingRow.length)
        {
            this.saveRowData($editingRow, function () { me._addDataRow(typeIndex); });
        }
        else
        {
            // Add empty line and get new row id
            if (this.get_editingMode() == "Inline" || this.get_editingMode() == "LitePopup")
                this._sendRequest({ RowID: '', TypeIndex: typeIndex, AddRow: true }, "SaveDataRow", "AddDataRow_completed");
            else
                this.saveControlData('');
        }
    },

    _prepRowToEdit: function (rowid)
    {
        var $r = $(this.GridID + " tr[role=row][empty-row=0].jqgrow");

        // modify left checkbox
        $('td[aria-describedby="' + this._gridID + '_cb"] :checkbox', $r).show()
            .prop('name', 'jqg_' + this._gridID + '_' + rowid)
            .prop('id', 'jqg_' + this._gridID + '_' + rowid);

        $r.removeAttr('empty-row')
            .prop('id', rowid);

        // Modify left pointer-triangle cell
        this.setRowPoinerCell($r);

        this._normalizeEmptyRows();
    },

    _normalizeEmptyRows: function()
    {
        var $r = $(this.GridID + " tr[role=row][empty-row].jqgrow").first();
        var i = 0;
        while ($r.length)
        {
            $r.prop('id', this.__emptyRowPrefix + i.toString())
            if (i == 0)
                $r.attr('empty-row', 0);
            i++;
            $r = $r.next();
        }
    },

    autoSelectRow: function (rowid) {
        $(this.GridID).setSelection(rowid, true);
        this.stylingCheckboxes(this.$getRow(rowid));
    },
    
    _onRowDelete: function (rowids)
    {
        if (rowids)
        {
            var src = event.srcElement;
            if (src && $(src).is("span.ui-icon-trash")) {
                if ($(src).attr("prevent-deletion")) {
                    $(src).removeAttr("prevent-deletion");
                    return false;
                }
            }
            if (!$.isArray(rowids))
                rowids = [rowids];

            if (!this.RunCustomHandler('rowDelete', false, rowids, ''))
                return;

            this._sendRequest( { SelectedRowIDs: rowids, RowID: rowids[0] }, "Delete", "_onRowDelete_completed");
        }
        return false;
    },

    _onRowDelete_completed: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        if (!this.RunCustomHandler('rowDelete', true, rowid, ''))
            return;
        if (this.get_editingMode() == "FormControls")
        {
            // Reset selections after update			
			this._onSelectRow(rowid, false);
        }
        setDirtyFlag();
        if (postbackRequested)
        {
            this._do_postback(this._eventTargetId, "RowDeleted:" + rowid);
            return;
        }
        else
        {
            this._last_selectedRowID = null;
            this._dataChanged = true;
            if (this.get_editingMode() == "FormControls")
                this._populateRowData(true, rowid);
            this.Reload_complete(null, responseData);
        }

        jqgrid_lock(false);
    },

    _loadComplete: function (data)
    {
    },

    _onHeaderClick: function ()
    {
    },

    _do_postback: function (eventTargetId, eventSource)
    {
        __page.postback(eventTargetId, eventSource);
    },

    _onEditRow_Started: /*void*/ function(rowid) 
    {
        // fix checkboxes
        var $r = this.$getRow(rowid);

        this.stylingCheckboxes($r);

        this._assignEditorHandlers(rowid);

        // clear cell-not-entered attribute for editing row
        $('td[cell-not-entered]', $r).removeAttr('cell-not-entered'); 

        // disable navigator buttons: paging, del, add. To avoid unplanned click
        this.setNavButtonsState();

        this.RunCustomHandler('editingStarted', true, rowid, null);
        var me = this;

        // Move focus to clicked cell
        setTimeout(function () {
            var $cellInput;
            var $rr = me.$getRow(rowid);
            if(window.LastClickedGridCell)
            {
                $cellInput = $('td[aria-describedby="' + window.LastClickedGridCell + '"] :input', $rr);
                if (!$cellInput.length)
                    $cellInput = $('td :input', $rr);                    
            }
            else {
                $cellInput = $('td :input', $rr);
            }
            if ($cellInput.length)
                $cellInput.first().focus();            
        }, 0);
    },

    _onEditRow_GetData: /*bool*/ function(serverData)
    {
        return true;
    },

    _onEditRow_AfterSave: /*void*/ function (rowid, serverResponse, ev)
    {
    },

    _onEditRow_AfterSaveError: /*void*/ function (rowid, serverResponse)
    {
    },

    _onEditRow_AfterRestore: /*void*/ function (rowid)
    {
    },

    _startInlineEditRow: function (rowid)
    {
        var me = this;
        if (this._ignoreInlineEditing == true)
        {
            return;
        }

        var theGrid = $(this.GridID);
        this._lockNavButtons(true);

        if (jqgrid_lock())
            return;

        jqgrid_lock(true);

        this._onStartInlineEditing = true;

        theGrid.jqGrid('editRow', rowid,
            false    /*use Enter/ESQ to save/cancel keys*/,
                /*oneditfunc: fires after successfully accessing the row for editing, prior to allowing user access to the input fields. The row's id is passed as a parameter to this function.*/
            function (rid) { me._onEditRow_Started(rid); delete me._onStartInlineEditing; },
                /*succesfunc: the function is called immediately after the request is successful. This function is passed the data returned from the server. 
                               Depending on the data from server; this function should return true or false.*/
            function (data) { return me._onEditRow_GetData(data); },
                /* url: the parameter replaces the editurl parameter from the options array. 
                    If set to 'clientArray', the data is not posted to the server but rather is saved only to the grid (presumably for later manual saving */
            me._ajaxEntryUrl, // 'clientArray',
                /* extraparam: an array of type name: value. When set these values are posted along with the other values to the server.*/
            null,
                /* aftersavefunc: this function is called after the data is saved to the server. 
                    Parameters passed to this function are the rowid and the response from the server request. Also the event is called too when the url is set to 'clientArray'*/
            function (rid, serverResponse, ev) { me._onEditRow_AfterSave(rid, serverResponse, ev); },
                /* errorfunc: this function is called after the data is saved to the server. 
                        Parameters passed to this function are the rowid and the the response from the server request. */
            function (rid, serverResponse) { me._onEditRow_AfterSaveError(rid, serverResponse); },
                /* afterrestorefunc: this function is called in restoreRow (in case the row is not saved with success) method after restoring the row. 
                To this function we pass the rowid */
            function (rid) { me._onEditRow_AfterRestore(rid); }
        );

        this._last_selectedRowID = rowid;
    },

    _lockNavButtons: function (do_lock)
    {
        if (do_lock)
        {
            var $btns = $('td[id="del_' + this._gridID + '"], td[id="refresh_' + this._gridID + '"], table.ui-pg-table td.ui-pg-button', this.PagerID);
            $btns.each(function (i, u)
            {
                if (u.id && u.id.startsWith('add_')) {
                    // ignore add button because it's available to add new row while editing
                }
                else
                {
                    // Disable click on locked buttons
                    $(u).addClass('ui-state-locked');
                    $('span.ui-icon', u)
                        .bind('click.locked', function () {
                            return false;
                        });
                }
            });

        }
        else
        {
            $(this.PagerID + ' td.ui-state-locked span.ui-icon').unbind('click.locked');
            $(this.PagerID + ' td.ui-pg-button').removeClass('ui-state-locked');
            $('#del_' + this._gridID + ' span.ui-icon').unbind('click.locked');
            $('#del_' + this._gridID).removeClass('ui-state-locked');
            $('#refresh_' + this._gridID + ' span.ui-icon').unbind('click.locked');
            $('#refresh_' + this._gridID).removeClass('ui-state-locked');
            $('#navadd1').unbind('click.locked');
            $('#navadd1').removeClass('ui-state-locked');
        }
    },

    _onAddRow: function (prm, onEmptyRowClick)
    {
        // Ignore click if incompleted transactions
        var theGrid = $(this.GridID);
        if (this._editingMode == "Inline")
        {
            if (this._ignoreInlineEditing == true)
            {
                return;
            }

            // Ignore add new row if addNewRow mode set
            var edSettings = this.get_editorSettings();
            if (edSettings) {
                if (onEmptyRowClick === true )
                {
                    if( edSettings.AddRowMode == 0 /*AddButton*/)
                        return;           // Ignore
                }
                else
                {
                    // User clicks Add button
                    if (edSettings.AddRowMode == 1 /*EmptyRowClick*/)
                        return;           // Ignore
                }
            }

            // lock nav buttons
            this._lockNavButtons(true);

            // find last editing row
            var $r = $('tr[editable=1]', theGrid);
            // Create new row on the server
            this._addDataRow(0, $r);
        }
        else if (this._editingMode == "FormControls")
        {
            theGrid.editGridRow("new", { reloadAfterSubmit: false });
        }
        else if (this._editingMode == "LitePopup")
        {
            // deselect all rows
            theGrid.resetSelection();
            this._addDataRow(0);
        }
        else if (this._editingMode == "Popup")
        {
            // deselect all rows
            theGrid.resetSelection();
            // get type by type index
            if (typeof prm == "undefined")
                prm = '0';
            var typeName = this._editorSettings.SubClassMap[parseInt(prm)].SubClassName;
            this._do_postback(this._eventTargetId, 'PopupEdit:Add:' + '' + ':' + typeName);
        }
        setDirtyFlag();
    },

    _litePopupEditing: function (isAdding, rowid)
    {
        this._divEditorID = "#WebPart_" + this._editorSettings.EditorContainer;
        if (this._editorSettings.SubClassMap)
        {
            var rowIndexStr = this.$getRow(rowid).attr('rowTypeIndex');
            var typeIndex = rowIndexStr ? parseInt(rowIndexStr) : 0;
            this._divEditorID = "#WebPart_" + this._editorSettings.SubClassMap[typeIndex].EditorWebPart;

            //clear potential 'required' fileds, which were set at runtime (by property conditions for example)
            var columnsMap = this._editorSettings.SubClassMap[typeIndex].ColumnControlsMap;
            if(typeof(columnsMap !== "undefined") && columnsMap.length > 0)
                Array.forEach(columnsMap, function (colMap) {
                    if (!colMap.Required)
                        $("#" + colMap.EditorClientId).children("span").removeAttr("required");
                });
        }
        var me = this;
        // Find the web part
        var wpEditor = $(this._divEditorID);
        wpEditor.data("isAdding", isAdding);
        wpEditor.data("_gridID", this._gridID);

        var body = wpEditor.parents('body');

        var wpEditorSize = { width: wpEditor.width(), height: wpEditor.height() };
        if (!wpEditorSize.width || wpEditorSize.width > (body.width() - 50 * 2))
        {
            wpEditor.width(body.width() - 50 * 2);
        }

        if (!wpEditorSize.height || wpEditorSize.height > (body.height() - 50 * 2))
        {
            wpEditor.height(body.height() - 50 * 2);
        }
        wpEditorSize = { width: wpEditor.width(), height: wpEditor.height() };
        wpEditor.css('overflow', 'auto');

        var prms = $.jgrid.camstar.EditingDialog;
        if (this.locales && this.locales.camstar && this.locales.camstar.EditingDialog)
            $.extend(true, prms, this.locales.camstar.EditingDialog);

        var wpEditorExists = false;
        if (wpEditor != null)
        {
            if (wpEditor.is('.ui-dialog-content'))
            {
                wpEditor.dialog('open');
                wpEditor.animate({ scrollTop: 0 }, 0);
                wpEditorExists = true;
            }
            if (wpEditor.find("input[type='file']").length > 0)
            {
                fileUpload = true;
                $('input.cs-fileuploadtext', wpEditor).val('');
            }

            wpEditor.data('current-rowid', rowid);
            if (!wpEditorExists)
            {
                var dialogButtons = [
                        {
                            text: prms.OkButton,
                            'class': prms.OkButtonCSS,
                            id: 'light_popup_dialog_button_OK',
                            click: function () {
                                var gr = $find($(this).data("_gridID"));
                                gr.saveControlData($(this).data('current-rowid'));
                            }
                        },
                        {
                            text: prms.CloseButton,
                            'class': prms.CloseButtonCSS,
                            id: 'light_popup_dialog_button_Close',
                            click: function ()
                            {
                                var gr = $find($(this).data('_gridID'));
                                if ($(this).data('bubbleMessage')) {
                                    gr.showMessage(dialog.data('bubbleMessage'));
                                    $(this).data('bubbleMessage', null);
                                }

                                if ($(this).data("isAdding") === true) {
                                    gr._onRowDelete($(this).data('current-rowid'));
                                    $(this).data('current-rowid', null);
                                }

                                // Reset data 
                                var selectedRowId = $(gr.GridID).getGridParam('selrow');
                                if (selectedRowId != null)
                                    gr._sendRequest({ RowID: selectedRowId, Options: "reset" }, "GetRowData", "GetRowData_completed");

                                $(this).dialog('close');
                            }
                        }
                ];
                if (!isAdding && prms.ResetButton)
                    dialogButtons.push(
                            {
                                text: prms.ResetButton,
                                'class': prms.ResetButtonCSS,
                                id: 'light_popup_dialog_button_Reset',
                                click: function (e)
                                {
                                    var gr = $find($(this).data("_gridID"));
                                    var selectedRowId = $(gr.GridID).getGridParam('selrow');
                                    if (selectedRowId != null) {
                                        var callParameters = { RowID: selectedRowId, Options: "reset" };
                                        setTimeout(function () {
                                            $find(gr._gridID)._sendRequest(callParameters, "GetRowData", "GetRowData_completed");
                                        }, 0);
                                        $(e.target).blur();
                                    }
                                }
                            });

                // 

                wpEditor.dialog(
                {
                    resizable: false,
                    title: (isAdding ? prms.addTitle : prms.editTitle),
                    modal: true,
                    zIndex: 2,
                    width: wpEditorSize.width,
                    height: wpEditorSize.height + 50,                    
                    beforeClose:
                        function (event, ui) {
                            $('.ui-dialog').css('z-index', null);
                            $('#fileupload  input').val('');

                            // Destroy Rich Text Box
                            $('.cs-textbox > textarea', wpEditor).each(function (i) {
                                var ctl = $find(this.id);
                                if (ctl) 
                                    ctl.richEditorRemove();
                            });

                            _modal.hide();

                            if (Camstars.Browser.IE)
                                $("input[type='file']").replaceWith($("input[type='file']").clone(true));
                            else
                                $("input[type='file']").val('');

                            // restore list
                            __page.makeTabstopsForDialog(null);
                        },
                    close: function () {
                        //prevents multiple dialog instances creation & reclaims resources
                        $(this).dialog('destroy');
                        var wpid = this.id;
                        window.setTimeout(function () {
                            // When dialog is closed the lite edit webpart should be hidden
                            $("#" + wpid).hide();
                        }, 100);    // use a much smaller timeout (was 1000)
                    },
                    open: function ()
                    {
                        _modal = new CamstarPortal.WebControls.Modal();
                        _modal.show();
                        $('.ui-dialog').css('z-index', 10);
                        $(this).addClass("litepopup-webpart");
                        $('td.empty', this).each(
                        function (cn, c)
                        {
                            if ($(c).children(':visible').length > 0)
                                $(c).removeClass('empty');
                        });

                        // Prepare toggle-container
                        $('.toggle-container', this).each(
                            function (ind, ui)
                            {
                                var toggleContainer = $find($(ui).parent().attr('id'));
                                if (toggleContainer)
                                {
                                    $clearHandlers(toggleContainer._collapseImage);
                                    $(toggleContainer._collapseImage).unbind('click').bind('click',
                                    function ()
                                    {
                                        // hide/show
                                        toggleContainer.toggle();
                                    });
                                    // collapse the toggle container when popup is opened.
                                    if (toggleContainer._state)
                                        toggleContainer.toggle();
                                }
                            });
                        // Modify tabbing list
                        __page.makeTabstopsForDialog($(this));

                        // Initialize Rich Text Box
                        $('.cs-textbox > textarea', this).each(function (i) {
                            var ctl = $find(this.id);
                            if (ctl)
                                ctl.richEditorInit(true);
                        });

                        // Start data loading and populating
                        var gr = $find($(this).data("_gridID"));
                        var selectedRowId = $(gr.GridID).getGridParam('selrow');
                        if (selectedRowId != null)
                        {
                            var callParameters = { RowID: selectedRowId, Options: "store" };
                            gr._sendRequest(callParameters, "GetRowData", "GetRowData_completed");
                        }
                        $(this).dialog("option", "title", ($(this).data("isAdding") === true ? prms.addTitle : prms.editTitle));
                    },
                    buttons: dialogButtons,
                    _modal: null
                });

                var dialog = wpEditor.parents('.ui-dialog');
                dialog.off("mousedown");
                if (dialog.length > 0)
                {
                    dialog.resizable({
                        helper: "ui-resizable-helper",
                        stop: function (event, ui)
                        {
                            var deltaY = ui.size.height - ui.originalSize.height;
                            $(this).height($(wpEditor).height() + deltaY);
                            ui.element.height('auto');
                        }
                    });
                    var titlebar = dialog.find('.ui-dialog-titlebar');
                    if (titlebar.length > 0)
                    {
                        $('<a class="ui-floatingframe-max"></a>')
                        .appendTo(titlebar)
                        .click(function () {
                            pop.maximize();
                        });
                    }
                    // Redirect upper cross button to the Close
                    $('.ui-dialog-titlebar-close', dialog).unbind().bind("click", function()
                    {
                        $('#light_popup_dialog_button_Close', dialog).click();
                    });
                }
            }
        }
    },

    GetRowData_completed: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        var dataObject = __json(responseData);
        this._populateRowData(false, rowid, dataObject);
    },

    // populate row data
    _populateRowData: function (clearControls, rowid, rowdata)
    {
        var currentRowData = this.getRowData(rowid);
        var theGrid = $(this.GridID);
        var $row = this.$getRow(rowid);
        var typeIndex = $row.attr('rowTypeIndex');
        if (!typeIndex)
            typeIndex = '0';

        var wp = $(this._divEditorID);
        var isAdding = (wp.length && wp.data("isAdding") === true);

        // load from selected row
        if (!clearControls)
        {
            if (rowdata == undefined || rowdata == null)
                rowdata = theGrid.getRowData(rowid);
        }
        else
        {
            // In case of clear data
            rowdata = null;
        }

        if (this._editorSettings != null)
        {
            var noPopulate = this._editorSettings.NoPopulate == true;
            if (clearControls)
                noPopulate = false;

            var columnControlsMap = this._editorSettings.ColumnControlsMap;

            if (this._editorSettings.SubClassMap && this._editorSettings.SubClassMap.length > 0 && typeIndex)
            {
                if (this._editorSettings.SubClassMap[parseInt(typeIndex)].ColumnControlsMap.length > 0)
                {
                    columnControlsMap = this._editorSettings.SubClassMap[parseInt(typeIndex)].ColumnControlsMap;
                }
            }

            if ( columnControlsMap && !noPopulate)
            {
                // new settings
                Array.forEach(columnControlsMap,
                function (colMap)
                {
                    if (!colMap.IsNotCleared)
                    {
                        var val = rowdata != null && rowdata[colMap.ColumnName] != null ? rowdata[colMap.ColumnName] : '';
                        var tdVal = $('td[role=gridcell][aria-describedBy$="_' + colMap.ColumnName + '"]', $row[0]);
                        if (tdVal.is('[chevron]'))
                        {
                            val = tdVal.data('val');
                        }
                        if ($('#' + colMap.EditorClientId).hasClass('hasDatepicker'))
                        {
                            Camstars.Controls.setValueById(colMap.EditorClientId, currentRowData[colMap.ColumnName], isAdding);
                        }
                        else
                        {
                            Camstars.Controls.setValueById(colMap.EditorClientId, val, isAdding);

                            var $input = $('#' + colMap.EditorClientId);

                            if ($input.is('input[type="checkbox"]'))
                            {
                                $input.trigger('change');
                            };
                        };
                    }
                }, null);
            }

            if (this._editorSettings != null)
                this._editorSettings.NoPopulate = false;
        }
    },

    _testUploadingCompletion: function (trans_id)
    {
        var uploadingCompleted = false;
        var errorUploading = null;

        var fileInp = $("input[type='file']");
        var ifr = $('iframe[name="dummyIframe_' + fileInp.attr("id") + '"]'); if (ifr.length > 0 && ifr[0].contentDocument && ifr[0].contentDocument.body)
        {
            var response = ifr[0].contentDocument.body.innerHTML;
            var valFile = $(this._divEditorID + ' input[type=file]').val();
            if (valFile == "")
            {
                uploadingCompleted = true;
            }
            else
            {
                uploadingCompleted = response.indexOf("Completed:") != -1;
                if (uploadingCompleted && response.indexOf("Failed:") != -1)
                {
                    errorUploading = response.substr(response.indexOf("Failed:") + 7);
                    errorUploading = errorUploading.substr(0, errorUploading.indexOf('EOF')).replace('\n', ' ');
                }
            }
        }
        else
        {
        }

        if (uploadingCompleted)
        {
            if (errorUploading == null)
            {
                var rowid = $(this._divEditorID).data('current-rowid');
                this.saveControlData(rowid);
            }
            else
            {
                this.showMessage(errorUploading);
                var editorDiv = "#WebPart_" + this._editorSettings.EditorContainer;
                var wpEditor = $(editorDiv);
                if (wpEditor.length > 0)
                {
                    wpEditor.dialog('close');
                    __page.hideModal();
                }
            }
            return true;
        }

        return false;
    },

    _getControlsData: function (rowId)
    {
        var datax = new Object();
        var validations = [];
        var controlsMap = this._editorSettings.ColumnControlsMap;

        if (this._editorSettings && this._editorSettings.SubClassMap)
        {
            var rowTypeIndex = 0;
            if (rowId)
            {
                var rowIndexStr = $('tr[role="row"][id="' + rowId + '"]', this._element).attr('rowTypeIndex');
                rowTypeIndex = rowIndexStr ? parseInt(rowIndexStr) : 0;
            }
            if (this._editorSettings.SubClassMap[rowTypeIndex].ColumnControlsMap.length > 0)
            {
                controlsMap = this._editorSettings.SubClassMap[rowTypeIndex].ColumnControlsMap;
            }
        }

        if (this._editorSettings && controlsMap)
        {
            Array.forEach(controlsMap, 
                function (colMap)
                {
                    var val = Camstars.Controls.getValueById(colMap.EditorClientId);
                    //field can be made required at runtime (property condition for example), so 'required' attr should also be checked
                    if ((colMap.Required || $('#' + colMap.EditorClientId).children('span').attr('required') === 'required') && (val == null || val == ''))
                    {
                        var valid = new CamstarPortal.WebControls.GridValidationStatus();
                        valid.set_Code(1);
                        valid.set_FieldName(colMap.ColumnName);
                        validations.push(valid);
                    }

                    datax[colMap.ColumnName] = val;
                });
        }

        return [datax, validations];
    },
    
    _updateFormValues: function (isAdding)
    {
    },

    SaveDataRow_completed: function (trans_id, responseData, action, postbackRequested, rowid, dataObject)
    {
        // set saved row selected without select event
        if (this.get_editingMode() == "LitePopup")
        {
            // Close dialog window with lite popup form
            var editorDiv = "#WebPart_" + this._editorSettings.EditorContainer;
            var wpEditor = $(editorDiv);
            if (wpEditor.length > 0 && wpEditor.is(":visible"))
            {
                wpEditor.dialog('close');
                __page.hideModal();
            }
        }

        //try to focus the first input control
        var controlsMap = this._editorSettings.ColumnControlsMap;
        if (controlsMap && controlsMap.length > 0)
        {
            if (!$('input', $("#" + controlsMap[0].EditorClientId)).is(":disabled"))
            {
                $('input', $("#" + controlsMap[0].EditorClientId)).focus();
            }
        }

        if (responseData != null)
            this.Reload_complete(null, responseData);

        this._populateRowData(true, rowid);

        if (this.get_editingMode() == "FormControls")
        {
            // Reset selections after update
            this._setupActionHandler(false, false);

            this._onSelectRow(rowid, false);
            if (action && action == "MultipleChildAdd")
            {
                var topGridId = dataObject;
                jqGrid_getGrid(topGridId).Reload();
            }
        }

        if (postbackRequested)
            this._do_postback(this._eventTargetId, "SaveDataRow_completed");

        this._dataChanged = true;
        this._lockNavButtons(false);
    },

    AddDataRow_completed: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        var em = this.get_editingMode();

        if ( em == "LitePopup")
        {
            if (responseData)
                this.Reload_complete(null, responseData);

            // assign new row id to empty string
            this._prepRowToEdit(rowid);
            this._litePopupEditing(true, rowid);
        }
        else if (em == "FormControls")
        {
            if (responseData)
                this.Reload_complete(null, responseData);
            else
                // assign new row id to empty string
                this._prepRowToEdit(rowid);
            // Save data
            $(this.GridID).setSelection(rowid);
            this.SaveFormControlData(false);
        }
        else
        {
            if (responseData)
            {
                this.Reload_complete(null, responseData);
            }
            else
            {
                // assign new row id to empty string
                this._prepRowToEdit(rowid);
            }
            this._dataChanged = true;
            this._startInlineEditRow(rowid);
        }
    },

    _onEditRow: function (rowid)
    {
        setDirtyFlag();
        if (this._editingMode == "Inline")
        {
            this._startInlineEditRow(rowid);
        }
        else if (this._editingMode == "LitePopup")
        {
            if (rowid && rowid.indexOf(this.__emptyRowPrefix) == -1)
            {
                this._litePopupEditing(false, rowid);
            }
            else
            {
                return false;
            }
        }
        else if (this._editingMode == "Popup")
        {
            if (rowid && rowid.indexOf(this.__emptyRowPrefix) == -1)
            {
                var typeIndexStr = $(this.GridID + ' tr[role="row"][id="' + rowid + '"]').attr('rowTypeIndex');
                var typeIndex = typeIndexStr ? parseInt(typeIndexStr) : 0;
                var typeName = this._editorSettings.SubClassMap[parseInt(typeIndex)].SubClassName;
                this._do_postback(this._eventTargetId, 'PopupEdit:Edit:' + rowid + ':' + typeName);
            }
            else
            {
                return false;
            }
        }
    },

    getRowData: function(rowid)
    {
        var res = {};
        var theGrid = $(this.GridID);
        var cols = theGrid.getGridParam('colModel');
        var controlsMap = this._editorSettings.ColumnControlsMap;
        if (this._editorSettings && this._editorSettings.SubClassMap)
        {
            var rowTypeIndex = 0;
            if (rowid)
            {
                var rowTypeIndexStr = $(this.GridID + ' tr[role="row"][id="' + rowid + '"]').attr('rowTypeIndex');
                rowTypeIndex = rowTypeIndexStr ? parseInt(rowTypeIndexStr) : 0;
            }
            if (this._editorSettings.SubClassMap[rowTypeIndex].ColumnControlsMap.length > 0)
            {
                controlsMap = this._editorSettings.SubClassMap[rowTypeIndex].ColumnControlsMap;
            }
        }

        var gridRows = $('>tbody >tr.jqgrow[id="' + rowid + '"] td[role="gridcell"]', theGrid);
        var me = this;
        gridRows.each(function (i)
        {
            var $td = $(this);
            if (i > cols.length - 1)
                return;
            var nameCol = cols[i].name;
            var val = null;
            if (nameCol !== 'cb')
            {
                if (cols[i].edittype == "checkbox")
                {
                    val = $td.find( ':checkbox' )[0].checked;
                }
                else if ($td.is('[chevron]'))
                {
                    val = $td.data('val');
                }
                else if ($td.data('val') && nameCol != me._sequenceColumn)
                {
                    val = $td.data('val');
                    if (val == '&#160;') //temp fix
                        val = '';
                }
                else
                {
                    val = $td.text();
                }

                if (controlsMap)
                {
                    var inlineCtlId = null;
                    Array.forEach(controlsMap, function (c)
                    {
                        if (inlineCtlId == null && c.ColumnName == nameCol)
                            inlineCtlId = c.EditorClientId;
                    });
                    if (inlineCtlId)
                    {
                        var inlineControl = $find(inlineCtlId);
                        if (inlineControl)
                        {
                            if (inlineControl.isDropDown)
                            {
                                val = inlineControl.getValue();
                                if (val == "NaN")
                                    val = null;
                            }
                        }
                    }
                }

                if (String(val).trim() === "")
                    val = "";

                res[nameCol] = val;
            }
        });

        return res;
    },

    SaveFormControlData: function (isNew)
    {
        if( isNew )
        {
            this._addDataRow();
        }
        else
        {
            this.saveControlData($(this.GridID).getGridParam('selrow'));
        }
    },

    ClearDataRow: function (rowId)
    {
        var callParameters = {};
        callParameters.RowID = rowId;
        this._sendRequest(callParameters, "ClearDataRow", "ClearDataRow_completed");
    },

    ClearDataRow_completed: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        this.Reload_complete(null, responseData);
        this._populateRowData(true, rowid);
        this._dataChanged = true;
    },

    jsonReplacer: function (k, v)
    {
        if (k == "" || typeof v != "string")
            return v;       // Root node or not string
        else
            return escape(v);
    },

    _savePrevRow: function (callback)
    {
        var $row = $('tr[editable=1]', this.get_element());
        if($row.length)
        {
            this.saveRowData($row[0].id, callback);
            return true;
        }
        else
        {
            return false;
        }
    },

    __getColEditable: function(colmodel, $cell)
    {
        var aria = $cell.attr('aria-describedby');
        if( aria )
        {
            var colName = aria.substr(this._gridID.length + 1);
            var col = null;
            Array.forEach( colmodel, function(c){if( c.name == colName) col = c ; });
            return col.editable;
        }
        return false;
    },

    __adjustDateTimeCells: function (respData) {

        if (respData.dateTimeColumnIdxs && respData.dateTimeColumnIdxs.length) {
            var inputFormat = Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern;
            var inputFormatMillisec = Sys.CultureInfo.InvariantCulture.dateTimeFormat.SortableDateTimePattern + "'.'fff";

            var p = getCEP_top().__page;
            var cult = p.get_pageCulture();
            if (p.get_LongTimePattern())
                cult.LongTimePattern = p.get_LongTimePattern();
            if (p.get_ShortDatePattern())
                cult.ShortDatePattern = p.get_ShortDatePattern();

            var outputFormat = cult.ShortDatePattern + ' ' + cult.LongTimePattern;

            respData.rows.forEach(function (rx) {
                respData.dateTimeColumnIdxs.forEach(function (di) {    
                    if (rx.cell[di]) {
                        var datestr = rx.cell[di];
                        var v = Date.parseLocale(datestr, /\.\d\d\d$/.test(datestr) ? inputFormatMillisec : inputFormat);
                        if (v) 
                            rx.cell[di] = new Date(v).format(outputFormat);
                        else
                            console.error('Datetime parsing error', di, rx.cell[di]);
                    }
                });
            });
        }
    },

    __findCellTofocus: function ($t, forward) {
        // find next element at the current row
        if (forward) 
            // find next 
            $t = $t.closest('td').next();
        else
            // find previous
            $t = $t.closest('td').prev();

        while ($t.length) {
            var $f = $('input:visible:enabled, textarea:visible:enabled', $t);
            if ($f.length && !$f.hasClass('cbox')) {
                // found accessible next/prev input
                $f.focus();
                return true;
            }
            $t = forward ? $t.next() : $t.prev();
        }
    },

    _keyDownProcessing: function (e, rowid)
    {
        var k = e.which;
        if (k === 9 /*TAB*/)
        {
            var tabDirection = e.shiftKey ? -1 : 1;
            var $t = $(e.target);
            var $r = $t.closest('tr');
            var $nextEditableRow = [];
            var isLastRow = false;

            // find next element at the current row
            if (this.__findCellTofocus($t, tabDirection == 1))
                return false;

            if (tabDirection == 1)
            {
                // no more focusable inputs in the row
                $nextEditableRow = $r.next('tr[role=row]');
                if ($nextEditableRow.length == 0)
                    isLastRow = true;
            }
            else if (tabDirection == -1)
            {
                // no more focusable inputs in the row - select previous row
                $nextEditableRow = $r.prev('tr[role=row]');
            }

            if ($nextEditableRow.length || isLastRow)
            {
                var me = this;
                if ($r.length)
                    this.saveRowData($r);
                this.__ignoreFocus = true;
                if (!$nextEditableRow.hasClass('jqgfirstrow') && !isLastRow)
                {
                    var nextEditableRowID = $nextEditableRow.prop("id");
                    setTimeout(
                            function ()
                            {
                                var $nr = me.$getRow(nextEditableRowID);
                                if ($nr.is('[empty-row]'))
                                {
                                    // Adding new line only if adding is enabled. if adding is not enabled  there is no empty-row === 0 and nothing to do
                                    if ($nr.attr('empty-row') === '0')
                                        me._onAddRow($nr);
                                }
                                else
                                {
                                    setTimeout(function ()
                                    {
                                        var $nrow = me.$getRow(nextEditableRowID);
                                        // Set first column of the next row
                                        var $nextEditableCell;
                                        if (tabDirection == 1) {
                                            $nextEditableCell = $('td:visible:not(.jqcboxrow):first', $nrow);
                                        }
                                        else {
                                            $nextEditableCell = $('td:visible:not(.jqcboxrow):last', $nrow);
                                            var colName = $nextEditableCell.attr('aria-describedby');
                                            if (/_dummy_$/.test(colName) || /_extender_$/.test(colName))
                                                $nextEditableCell = $nextEditableCell.prev();
                                        }

                                        if ($nextEditableCell.length) {
                                            window.LastClickedGridCell = $nextEditableCell.attr('aria-describedby');
                                        }
                                        $nrow.click();
                                    }, 0);
                                }
                            }, 0);
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
        else if (k === 13 /*CR-Enter*/)
        {
            var textbox = $('input[name="' + e.target.name + '"]');
            window.idrow = $(e.target).closest('tr').prop('id');
            if (typeof textbox.blur !== "undefined")
               textbox.blur();
            this.saveRowData($(e.target).closest('tr'));
            return false;
        }
        else if (k === 27 /*ESC*/)
        {
            var rid = $(e.target).closest('tr').prop('id');
            var me = this;
            $(this.GridID).restoreRow(rid, function()
            {
                jqgrid_lock(false);
                me._lockNavButtons(false);
            });
            return false;
        }
        return;
    },

    _assignEditorHandlers: function (rowid)
    {
        var $currentRow = this.$getRow(rowid);
        var me = this;
        if ($currentRow.length)
        {
            // assign custom handlers to checkboxes
            $('.editable[role=checkbox]', $currentRow).each(
                function ()
                {
                    if (me._controlCustomHandlers[this.name] != null && me._controlCustomHandlers[this.name].click)
                    {
                        $(this).click(me._controlCustomHandlers[this.name].click);
                    }
                }
            );
            $currentRow.removeAttr("state");
        }
    },

    _onSaveRow_getData: /*bool*/ function (data)
    {
        if (data && data.responseText)
        {
            var resp = __json(data.responseText);
            if (resp && resp.Response && resp.Response.length > 0)
            {
                // postponed 
                this.__directUpdateResponseData = resp.Response[0].Data;
            }
        }

        jqgrid_lock(false);

        // recalculate total rows
        this._totalRows = $('tr[role=row].jqgrow', this.get_element()).length;
        this._totalRows -= $('tr[empty-row]', this.get_element()).length;

        // allow delete button if it's visible
        this.setNavButtonsState();
        this._lockNavButtons(false);

        return true;
    },

    saveRowData: function (rowid, callback)
    {
        var me = this;

        jqgrid_lock(true);
        if (typeof rowid != 'string')
        {
            rowid = rowid[0].id;
        }
        var savePrm = {
            successfunc: function (p) { return jqGrid_getGrid(this.id)._onSaveRow_getData(p); },
            url: this._ajaxEntryUrl, 
            extraparam: { _rowid: rowid },
            aftersavefunc: function (row_id)
            {
                me.setNavButtonsState();
                if (callback && $.isFunction(callback))
                    callback.call(me);

                if (me.get_rowSelectionMode() == "CheckBox")
                {
                }
                else if (me.get_rowSelectionMode().startsWith("Single"))
                {
                    // Restore recently selected rows
                    //$(me.GridID).resetSelection(row_id);
                }

                if (me.__directUpdateResponseData)
                {
                    me.directUpdate(me.__directUpdateResponseData);
                    delete me.__directUpdateResponseData;
                }

                me.stylingCheckboxes(me.$getRow(row_id));

                // run wrapping 
                $.jgrid.wrapCells(me);
                me._lockNavButtons(false);
                me.RunCustomHandler('renderCompleted', false, row_id, 'aftersavefunc');

            },
            errorfunc: null,
            afterrestorefunc: null,
            restoreAfterError: true,
            mtype: "POST"
        };
        $(this.GridID).jqGrid('saveRow', rowid, savePrm);
    },


    saveControlData: function(rowid)
    {
        var inpData = this._getControlsData(rowid);

        // Submit data
        var inpDataSerialized = JSON.stringify(inpData[0], this.jsonReplacer);
        var callParameters = { RowID: rowid, RowDataObject: inpDataSerialized, AddRow: (rowid == '') };
        if (this._multipleChildAdding && rowid === "") {
            this._sendRequest(callParameters, "MultipleChildAdd", "SaveDataRow_completed");
        }
        else {
            if (inpData[1].length > 0 && CamstarPortal.WebControls.GridValidationStatus.isInstanceOfType(inpData[1][0]))
            {
                if (typeof (notDisplayWarningFromGrid) == 'undefined')//Disabling validation in the grid, only server-side validation.
                    //Setting a flag in ClientScript.js

                    // Error message 
                    this.processStatusData({ Message: null, ValidationItems: inpData[1] });

            }
            this._sendRequest(callParameters, "SaveDataRow", "SaveDataRow_completed");
        }

    },

    cellaction: function ()
    {
        $.ajax(this._ajaxEntryUrl);
    },

    _onSelectRow: function (rowid, status, e)
    {
        var clickedTD = null;

        if (e && e.target)
        {
            var $target = $(e.target);
            if ($target.is('td[role=gridcell]'))
            {
                clickedTD = $target;
            }
            else if ($target.is(':checkbox'))
            {
                clickedTD = $target.parent();
            }
            else if ($target.is("div[class='customelement']"))
            {
                clickedTD = $target.parents('td[role=gridcell]');
            }

            if ($target.is('div.treeclick'))
            {
                return false;
            }
        }

        var columnID = clickedTD ? this.getColumnOfTD(clickedTD).name : "";
        var isEditing = false;
       
        if ($('tr[id="' + rowid + '"]', this.get_element()).attr('editable') == "1")
           return false;
        
        if (this._editingMode == "Inline")
        {
            if (this._justSelect)
            {
                if (this.isRowEmpty(rowid))
                    return false;
            }
            else
            {
                if (clickedTD != null && (clickedTD.hasClass('jqcboxrow') || clickedTD.hasClass('ui-jqgrid-sequence-cell') || clickedTD.hasClass('ui-jqgrid-row-selector')))
                {
                    // is a check box or triangle on the left has been clicked
                    isEditing = false;
                    this._savePrevRow(rowid);
                    if (clickedTD.hasClass('ui-jqgrid-sequence-cell') || clickedTD.hasClass('ui-jqgrid-row-selector'))
                    {
                        status = true;
                        $(this.GridID).setSelection(rowid, false);
                    }
                }
                else
                {
                    isEditing = true;
                    if (this.isRowEmpty(rowid))
                    {
                        this._onAddRow(undefined, true);
                    }
                    else
                    {
                        this._savePrevRow(rowid);
                        if (this.get_rowSelectionMode().startsWith("SingleRowSelect"))
                            $(this.GridID).setSelection(rowid, false);
                        this._startInlineEditRow(rowid);
                    }
                }
            }
        }
        else if (this._editingMode == "FormControls")
        {
            var controlsMap = this._editorSettings.ColumnControlsMap;
            if (controlsMap && controlsMap.length > 0)
            {
                //focus only visible elements, since controls can have hidden inputs
                $('input:visible', $("#" + controlsMap[0].EditorClientId)).focus().select();
            }

            if (this.isRowEmpty(rowid))
                return false;

            var clearData = false;
            if (this._editorSettings.HideInactiveButtons == true)
            {
                clearData = !status;
            }
            if (this._editorSettings.NoPopulatingOnSelect == false)
            {
                this._populateRowData(clearData, rowid);
            }
            this._setupActionHandler(false, !clearData);
            if (clearData)
            {
                // Clear unselected highlighting
                if (this.get_rowSelectionMode().startsWith("SingleRowSelect"))
					$(this.GridID).resetSelection();
            else
                $(this.GridID).resetSelection(rowid);
                this._last_selectedRowID = null;
            }
            else
            {
                this._last_selectedRowID = rowid;
            }
        }
        else
        {

            if (this.get_rowSelectionMode().startsWith("SingleRowSelect") && status === false)
            {
                $(this.GridID).resetSelection();
            }
        }

        if (this._justSelect)
        {
            // keep _last_selectedRowID unchanged
            this._justSelect = false;
        }
        else
        {
            if (this._editingMode != "FormControls")
                this._last_selectedRowID = rowid;
        }

        if (!isEditing)
        {
            if (!(typeof status == "boolean"))
            {
                status = $('tr[id="' + rowid + '"]', this.get_element()).attr('aria-selected') == 'true';
            }
            var callParameters = { RowID: rowid, IsRowSelected: status, SelectedColumnID: columnID};
            this._sendRequest(callParameters, "OnRowSelected", "OnRowSelected_completed");
            return false;
        }

        return true;
    },

    _onSelectAll: function (aRowids, status)
    {
        var callParameters =
            {
                "RowID": null,
                "IsRowSelected": status,
                "SelectedRowIDs": aRowids
            };
        this._sendRequest(callParameters, "SelectAllRows", "_selectAll_completed");
    },

    _selectAll_completed: function (trans_id, responseData, action, postbackRequested, rowid)
    {
        if (this._isHorizonStyle)
            $.jgrid.updateHorizonCheckboxes($get("gview_" + this._gridID));

        // response data contains total selected 
        this.set_selectedCount(responseData);
        this.setNavButtonsState();

        // reload subgrids
        var theGrid = $(this.GridID);
        var ids = theGrid.getDataIDs();

        for (var i = 0; i < ids.length; i++)
        {
            this.reloadSubgridOnSelect(ids[i]);
        }

        this.displaySelectedCount();

        // Clear selected state for #empty# rows
        $("tr[role=row][empty-row].jqgrow", theGrid).removeClass('ui-state-highlight');

        if (this._customRowStyling)
        {
            $("tr[role=row].jqgrow", theGrid).each($.jgrid.toggleRowStyle);
        }

        this.RunCustomHandler('renderCompleted', true, null, null);

        if (postbackRequested)
        {
            this._do_postback(this._eventTargetId, action);
            return true;    // show modal
        }
        else
        {
            return false;   // hide modal
        }
    },

    getRowIndex: function (rowID)
    {
        return $(this.GridID).getInd(rowID) - 1;
    },

    getRowIndexes: function (rowIDs)
    {
        var selectedIndexes = [];
        if (rowIDs.length)
        {
            for (var i = 0; i < rowIDs.length; i++)
            {
                selectedIndexes.push(this.getRowIndex(rowIDs[i]));
            }
        }

        return selectedIndexes;
    },

    _getInstanceGrid: function ()
    {
        return $(this.GridID);
    },

    _sendRequest: function (callParameters, serverAction, callBackMethod, returnVal, trans_id) {
        if (!__page._lock) {

            var theGrid = $(this.GridID);
            var viewDiv = $('#gview_' + this._gridID + ' .ui-jqgrid-bdiv');
            if (viewDiv.length > 0)
                this._vscroll = viewDiv.scrollTop();

            var gridParam = theGrid.getGridParam();
            callParameters.Action = serverAction;
            callParameters.JQGridID = this._gridID;
            callParameters.ContextID = this._contextID;
            callParameters.GridCallBack = callBackMethod;
            callParameters.VScroll = Math.round(this._vscroll);
            callParameters.Filter = escape(this._filter);
            callParameters.trans_id = trans_id;
            if (callParameters.toPage) {
                callParameters.CurrentPage = callParameters.toPage;
                callParameters.toPage = undefined;
            }
            else {
                callParameters.CurrentPage = parseInt(gridParam.page);
            }
            callParameters.CallStackKey = getParameterByName("CallStackKey");

            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("ClientEntry");

            var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
            transition.set_commandParameters(callParamsString);
            transition.set_clientCallback(callBackMethod);
            transition.set_noModalImage(true);
            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
            communicator.dispose();
        }
        else {
            var gridId = this._gridID;
            // page locked: recall request
            window.setTimeout(function () {
                var gridComp = $find(gridId);
                if (gridComp) {
                    // grid component found
                    gridComp._sendRequest(callParameters, serverAction, callBackMethod, returnVal, trans_id);
                }
                else {
                    // grid component not found
                }
            }, 100);
        }
        return returnVal;
    },

    _onSaveReorderedColumns: function (newColOrder, permutation)
    {
        var theGrid = $(this.GridID);
        theGrid.remapColumns(permutation, true, false);

        if (!this._responsiveWidth && this._width)
            theGrid.setGridWidth(this._width, this._shrinkColumns);

        var cols = theGrid.getGridParam('colModel');
        var colState = [];
        var templateCols = this._responsiveWidth ? $("#gview_" + this._gridID + " .ui-jqgrid-bdiv table.ui-jqgrid-btable tr.jqgfirstrow > td") : null;
        var wmr = typeof this.widthMultiplier != "undefined" ? this.widthMultiplier : 1.0;
        cols.forEach(function (c, i) {
            var cw = c.width / wmr;
            if (c.index != null)
                colState.push([c.index, cw, !c.hidden]);

            if (templateCols) {
                var td = templateCols.get(i);
                if (td && td.style.display !== "none" && !td.style.width) {
                    td.style.width = (cw * wmr) + "px";
                }
            }
        });

        var serialized = JSON.stringify([newColOrder, colState], this.jsonReplacer);
        var callParameters = { "RowDataObject": serialized };
        this._sendRequest(callParameters, "SaveReorderedColumns", "SavedReorderedColumns");
    },

    SavedReorderedColumns: function () {
        if (this._responsiveWidth)
            this.resize();
    },

    __getColIndex: function(cols, id)
    {
        for (var i = 0; i < cols.length; i++)
        {
            if (cols[i].name == id)
                return i;
        }

        return -1;
    },

    _onShowColumnChooser: function (prms)
    {
        var me = this;
        var theGrid = $(this.GridID);
        var doneFunction = function (permutation)
        {
            if (permutation)
            {
                var cols = theGrid.getGridParam('colModel');
                var colOrder = [];
                if (cols[0].fixed && cols[0].name == 'cb')
                {
                    // Checkbox - must have fixed position
                    colOrder.push(cols[0].name);
                }
                // Show/hide columns
                Array.forEach(me.__savedSelectedColumnList_visible, function (t)
                {
                    theGrid.showCol(t.id);
                    colOrder.push(t.id);
                }, null);
                Array.forEach(me.__savedSelectedColumnList_hidden, function (t)
                {
                    theGrid.hideCol(t.id);
                    colOrder.push(t.id);
                }, null);

                permutation = [];
                // Add non existing column numbers
                for (var ic = 0; ic < cols.length; ic++)
                {
                    if (!Array.contains(colOrder, cols[ic].name))
                        colOrder.push(cols[ic].name);
                }
                Array.forEach(colOrder, function (cname)
                {
                    permutation.push(me.__getColIndex(cols, cname));
                }, null);
                me._onSaveReorderedColumns(colOrder, permutation);
            }
        };

        // Change behavior of the select control. The column is toggled 
        var me = this;
        var multiselect_completed = function (phase)
        {
            var selControl = this;

            if (phase != 'destroy')
            {
                // Create 
                me.__savedSelectedColumnList_visible = [];
                me.__savedSelectedColumnList_hidden = [];

                var cols = theGrid.getGridParam('colModel');

                $('option', selControl).each(function ()
                {
                    var c = cols[this.value];
                    if (this.text != '_id_column' && this.innerHTML != '&nbsp;')
                    {
                        var item = { name: this.text, index: parseInt(this.value), id: c.index, active: false };
                        if (this.selected)
                            me.__savedSelectedColumnList_visible.push(item);
                        else
                            me.__savedSelectedColumnList_hidden.push(item);
                    }
                });

                var chooserPanel = $($(selControl).parent()[0]);
                chooserPanel.height('100%');
                chooserPanel.width('100%');
                $(chooserPanel.parent().parent())
                    .addClass('cs-grid-dialog')
                    .find('.ui-icon-closethick').html('');

                var selControlHeight = $(selControl).height();

                var updown = '<table class="ui-column-chooser-side-buttons"><tr>' +
                                '<td width="50%" style="text-align:center"><div><button cmd=up class=ui-column-chooser-mbutton></button></div></td>' +
                                '<td width="50%" style="text-align:center"><div><button cmd=down class=ui-column-chooser-mbutton></button></div></td>' +
                            '</table>';

                $(selControl).replaceWith($(
                            '<div class="ui-column-chooser-panels">' +
                                '<table height="80%">' +
                                    '<tr>' +
                                        '<td><div class="ui-column-chooser-panel" panel="visible"></div></td>' +
                                        '<td><div class="ui-column-chooser-middle-buttons"></div></td>' +
                                        '<td><div class="ui-column-chooser-panel" panel="hidden"></div></td>' +
                                    '</tr>' +
                                    '<tr>' +
                                        '<td panel="visible">' + updown + '</td>' +
                                        '<td>&nbsp;</td>' +
                                        '<td panel="hidden">' + updown + '</td>' +
                                    '</tr>' +
                                '</table>' +
                             '</div>'));

                var visPanel = $('div.ui-column-chooser-panel[panel="visible"]', chooserPanel);
                var hidPanel = $('div.ui-column-chooser-panel[panel="hidden"]', chooserPanel);

                hidPanel.css('float', 'right');
                visPanel.css('float', 'left');

                // Define bottom buttons
                var bottomButtonPanel = $('.ui-dialog-buttonpane', $(chooserPanel).parent().parent()[0]).children().first();
                // Hide existing buttons
                bottomButtonPanel.children().hide();
                // Add new buttons
                bottomButtonPanel.append('<button mode=apply></button><button mode=close></button>');
                var bottomButtons = bottomButtonPanel.children('[mode]');
                bottomButtons.addClass('cs-button ui-column-chooser-button');

                bottomButtons.first().css('float', 'right').text(prms.ApplyButton).bind('click', function () {
                    if (me.__savedSelectedColumnList_visible.length == 0)
                        alert(prms.oneColumnReqiredMsg);
                    else
                        $("button:first-child", bottomButtonPanel).click(/*OK*/);
                });
                bottomButtons.last().css('float', 'right').text(prms.CloseButton).bind('click', function () {
                    $("button:nth-child(2)", bottomButtonPanel).click(/*Cancel*/);
                });
                bottomButtons.css('margin-left', '10px').css('margin-right', '10px');

                var middleButtonsPanel = $('div.ui-column-chooser-middle-buttons', chooserPanel);
                middleButtonsPanel.html(
                    '<table>' +
                        '<tr><td class="ui-column-chooser-middle-buttons"><div><span cmd=hideOne class=ui-column-chooser-mbutton></span></div></td></tr>' +
                        '<tr><td class="ui-column-chooser-middle-buttons"><div><span cmd=hideAll class=ui-column-chooser-mbutton></span></div></td></tr>' +
                        '<tr><td class="ui-column-chooser-middle-buttons"><div><span cmd=showOne class=ui-column-chooser-mbutton></span></div></td></tr>' +
                        '<tr><td class="ui-column-chooser-middle-buttons"><div><span cmd=showAll class=ui-column-chooser-mbutton></span></div></td></tr>' +
                    '</table>'
                 );
                // Setup middle buttons
                var mbuttons = $(chooserPanel).find('.ui-column-chooser-mbutton');
                mbuttons.bind('click', function (b)
                {
                    var cmd = $(this).attr('cmd');
                    if (cmd == 'up' || cmd == 'down')
                        me.__chooseColumnButtonReorder($(this), visPanel, hidPanel, cmd);
                    else
                        me.__chooseColumnButtonMove($(this), visPanel, hidPanel, cmd);
                });

                // Add closing dialog on next click
                var closeDialogFunc = function ()
                {
                    bottomButtons.last().click();
                    $(this).unbind('click', closeDialogFunc);
                    return false;
                };
                $('#ShowColumns', $('#gbox_' + me._gridID)[0]).bind('click', closeDialogFunc);

                // Setup headers
                $(visPanel.parent()).prepend('<div class="ui-column-chooser-header">' + prms.visibleCaption + '</div>');
                $(hidPanel.parent()).prepend('<div class="ui-column-chooser-header">' + prms.hiddenCaption + '</div>');

                me.__rebuiltColumnChooser(visPanel, hidPanel);

                $('.ui-column-chooser-panel').droppable({
                    drop: function (ev, ui)
                    {
                        var colName = ui.draggable.text();
                        var fromPanel = ui.draggable.parent();
                        var toPanel = $(this);

                        if (fromPanel.attr('panel') != toPanel.attr('panel'))
                        {
                            me.__moveItem(colName, fromPanel, toPanel, ev.pageY);
                        }
                        else
                        {
                            // Reorder in the same panel
                            me.__moveItem(colName, fromPanel, toPanel, ev.pageY);
                        }
                        me.__rebuiltColumnChooser(visPanel, hidPanel);
                    }
                });

                // Initial sizes
                $('.ui-column-chooser-panel').width(220).height(240);

                $('div[role=dialog].ui-dialog').bind("dialogresize",
                    function (ev, ui)
                    {
                        $('.ui-column-chooser-panel').width((ui.size.width - 110) / 2)
                            .height(ui.size.height - 180);
                    }
                );
            }
            else
            {
                // Destroy
                delete me.__savedSelectedColumnList_visible;
                delete me.__savedSelectedColumnList_hidden;
            }
        };

        theGrid.jqGrid('columnChooser', { done: doneFunction, msel: multiselect_completed });
    },

    __moveItem: function (itemName, fromPanel, toPanel, mouseY)
    {
        var fromList = fromPanel.is('[panel=visible]') ? this.__savedSelectedColumnList_visible : this.__savedSelectedColumnList_hidden;
        var toList = toPanel.is('[panel=visible]') ? this.__savedSelectedColumnList_visible : this.__savedSelectedColumnList_hidden;

        var items = toPanel.find('.ui-column-chooser-item');
        var droppedOnItemIndex = -1;
        for (var i = 0; i < items.length; i++)
        {
            var d = $(items[i]);
            var top = d.offset().top - 7;
            var bottom = d.offset().top + d.height() + 7;

            if ((mouseY >= top) && (mouseY < bottom))
            {
                droppedOnItemIndex = i;
                break;
            }
        }

        var listItem = null;

        Array.forEach(fromList, function (t) { if (t.name == itemName) listItem = t; }, null);
        Array.remove(fromList, listItem);
        if (droppedOnItemIndex == -1 || droppedOnItemIndex == (items.length - 1))
            Array.add(toList, listItem);
        else
            Array.insert(toList, droppedOnItemIndex + 1, listItem);
    },

    __rebuiltColumnChooser: function (visPanel, hidPanel)
    {
        visPanel.children('.ui-column-chooser-item').remove();
        hidPanel.children('.ui-column-chooser-item').remove();
        var me = this;

        Array.forEach(this.__savedSelectedColumnList_visible, function (sp) { visPanel.append('<div class="ui-column-chooser-item"' + (sp.active ? ' active=true' : '') + '>' + sp.name + '</div>'); }, null);
        Array.forEach(this.__savedSelectedColumnList_hidden, function (sp) { hidPanel.append('<div class="ui-column-chooser-item"' + (sp.active ? ' active=true' : '') + '>' + sp.name + '</div>'); }, null);
        $('.ui-column-chooser-item').draggable({
            helper: function ()
            {
                return $(this).clone().css('width', this.offsetWidth)[0];
            },
            opacity: 0.8
        });

        var selectFn = function () { me.__selectPanelItem($(this)); };
        hidPanel.find('.ui-column-chooser-item').click(selectFn);
        visPanel.find('.ui-column-chooser-item').click(selectFn);
    },

    __selectPanelItem: function (item)
    {
        var itemName = item.text();
        var isActive = item.attr('active') != undefined;
        var findItemFun = function (t) { t.active = (t.name == itemName && !t.active); };
        Array.forEach(this.__savedSelectedColumnList_visible, findItemFun, null);
        Array.forEach(this.__savedSelectedColumnList_hidden, findItemFun, null);

        // clear all selection for current panel
        var panel = item.parents('[panel]').find('[active=true]').removeAttr('active');
        if (!isActive)
            item.attr('active', 'true');
    },

    __chooseColumnButtonReorder: function (b, vp, hp, cmd)
    {
        var panel = $(b).parents('td[panel]').first().attr('panel') == 'visible' ? vp : hp;
        var cl = panel.is('[panel=visible]') ? this.__savedSelectedColumnList_visible : this.__savedSelectedColumnList_hidden;
        if (cl.length < 2)
            return; // too less elements - mo move

        var activePos = -1;
        for (var i = 0; i < cl.length; i++)
        {
            if (cl[i].active) { activePos = i; break; }
        }
        if (activePos == -1)
            return; // no active items

        if (cl[(cmd == 'up' ? 0 : cl.length - 1)].active)
            return; // do nothing - no space to move

        var swapShift = (cmd == 'up') ? -1 : 1;
        var it = cl[activePos + swapShift]; cl[activePos + swapShift] = cl[activePos]; cl[activePos] = it;
        this.__rebuiltColumnChooser(vp, hp);
    },

    __chooseColumnButtonMove: function (b, vp, hp, cmd)
    {
        var isHide = cmd.indexOf('hide') != -1;
        var isAll = cmd.indexOf('All') != -1;

        var fromList = isHide ? this.__savedSelectedColumnList_visible : this.__savedSelectedColumnList_hidden;
        var toList = !isHide ? this.__savedSelectedColumnList_visible : this.__savedSelectedColumnList_hidden;
        if (fromList.length == 0)
            return;

        if (!isAll)
        {
            // find active item on fromPanel
            var activePos = -1;
            for (var i = 0; i < fromList.length; i++)
            {
                if (fromList[i].active) { activePos = i; break; }
            }
            if (activePos == -1)
                return; // no active items

            var it = fromList[activePos];
            Array.removeAt(fromList, activePos);
            it.active = false;
            toList.push(it);
        }
        else
        {
            Array.addRange(toList, fromList);
            Array.forEach(toList, function (item) { item.active = false; }, null);
            if (isHide)
                this.__savedSelectedColumnList_visible = [];
            else
                this.__savedSelectedColumnList_hidden = [];
        }

        this.__rebuiltColumnChooser(vp, hp);
    },

    _onShowMultiSort: function (prms)
    {
        var me = this;
        (this.GridID).jqGrid('multisortGrid', { onSort: function (rules) { me._onSortByRules(rules); }, useLocalData: false });
    },

    _onColumnSorting: function (columnId, iCol/*index of column*/, sortorder /*can be 'asc' or 'desc'*/)
    {
        this._sendRequest({ "SortOrder": [columnId, sortorder].join(' ') }, "ColumnSorting", "ColumnSorting_complete");
        return 'stop';
    },


    ColumnSorting_complete: function (trans_id, responseData, action, isNeedPostBack, rowId, rowdata)
    {
        // rowdata contains sorting order
        var d = rowdata.split(' ');
        var $hdiv = $('.ui-jqgrid-hdiv', $(this.get_element()).parent().parent().parent());

        // reset all headers
        $('th[role=columnheader]', $hdiv).removeAttr('sorted');

        // set current header
        $('[id="' + this._gridID + '_' + d[0] + '"]', $hdiv).attr('sorted', d[1]);

        return this.Reload_complete(trans_id, responseData, action, isNeedPostBack, rowId);
    },

    /// - 'first','last','prev','next' in case of button click, 
    /// - 'records' in case when a number of requested rows is changed and 
    /// - 'user' when the user change the number of the requested page
    _onPaging: function (pgButton)
    {
        if (!$('#' + pgButton).hasClass('ui-state-disabled'))
        {
            // lock paging to prevent double click
            $('td.ui-pg-button', $('#' + pgButton).parent()).addClass('ui-state-disabled');
            this._sendRequest({ ActionID: pgButton.substr(0, 4) }, "Reload", "Reload_complete");
        }
        return 'stop';
    },

    _beforeRefresh: function ()
    {
        var me = this;
        if (!this._savePrevRow(function () { me.Reload(true); }))
            me.Reload(true);
        return false;
    },

    processStatusData: function (statusData)
    {
        var msgText = '';
        var msgLen = 0;
        var theGrid = $(this.GridID);
        var columnModel = theGrid.getGridParam('colModel');
        var me = this;

        if (statusData.Message)
        {
            msgText += statusData.Message;
            msgLen = statusData.Message.length;
        }
        else
        {
            Array.forEach(statusData.ValidationItems,
                function (v)
                {
                    if (v.Code == 0)
                    {
                        msgLen += v.Message.length;
                    }
                    else if (v.Code == 1)
                    {
                        // Find column by key
                        var colCaption = v.FieldName;
                        Array.forEach(columnModel, function (c) { if (c.index == v.FieldName) colCaption = c.label; }, null);
                        v.Message = $.jgrid.format(me._validation.fieldRequired, colCaption);
                    }
                    else if (v.Code == 2)
                    {
                        // display a label
                        v.Message = $.jgrid.getLabelText(me, v.Message);
                    }
                    msgLen += v.Message.length;
                    msgText += v.Message + "\n";
                },
                null);
        }
        var isSuccess = true;
        if (msgLen > 0) {
            __page.displayStatus(msgText, "Warning", me._validation.warning);
            isSuccess = false;
        }

        var wpEditor = $(this._divEditorID);
        if (wpEditor && wpEditor.dialog('isOpen')){
             wpEditor.dialog('close');
            __page.hideModal();           
        }
        return isSuccess;
    },

    showMessage: function (msgText, showTime)
    {
        if (showTime == undefined)
            showTime = 8000;

        if (msgText)
        {

            var style = window.getComputedStyle($(this.GridID)[0]);
            var isHidden = style.display === "none";

            if (isHidden)
            {
                if (this._parentGridID)
                {
                    var parGrid = $find(this._parentGridID);
                    if (parGrid)
                    {
                        parGrid.showMessage(msgText, showTime);
                    }
                    else
                    {
                        alert(msgText);
                    }
                }
                return;
            }

            var bubbleMessageBoxId = this._gridID + "_messagebox";
            var msgBox = $("#" + bubbleMessageBoxId);
            if (msgBox.length == 0)
            {
                // Add message box div 
                var pager = $("#pg_" + this._gridID + "_pager");
                pager.before('<div id=\'' + bubbleMessageBoxId + '\' class=\'ui-jqgrid-status ui-corner-all\'></div>');
                msgBox = $("#" + bubbleMessageBoxId);
            }

            msgBox.html(msgText).show().fadeIn();
            setTimeout('$(\'#' + bubbleMessageBoxId + '\').fadeOut().hide();', showTime);
        }
    },

    OnRowSelected_completed: function (trans_id, responseData, action, isNeedPostBack, rowId)
    {
        this.set_selectedCount(responseData);
        this.displaySelectedCount();
        this.setNavButtonsState();

        var $r = this.$getRow(rowId);
        this.stylingCheckboxes($r);

        if (this._customRowStyling){
            if ($r.length)
                $.jgrid.toggleRowStyle($r, $r.is('.ui-state-highlight'));
        }

        this.RunCustomHandler('renderCompleted', true, rowId, 'OnRowSelected_completed');

        this.reloadSubgridOnSelect(rowId);

        this._lockNavButtons(false);

        if (this.get_editingMode() == "FormControls")
        {
            // the button settings 
            var idSelected = $(this.GridID).getGridParam('selrow');
            if (idSelected && idSelected == rowId)
                this._setupActionHandler(false, true);
            else
                this._setupActionHandler(false, false);
        }

        if (isNeedPostBack)
        {
            this._do_postback(this._eventTargetId, action + ":" + this._contextID);
            return true;    // show modal
        }
        else
        {
            return false;   // hide modal
        }
    },

    reloadSubgridOnSelect: function (rowId)
    {
        // Reload sub grid if necessary
        var subGridComponent = $find(this._gridID + '_' + rowId + '_t');
        if (subGridComponent && $(subGridComponent.GridID).length > 0)
        {
            if (subGridComponent._parentSelectsSubGrid)
            {
                subGridComponent.Reload();
            }
        }
        else
        {
            if (this._subGridId)
            {
                // try to find a template
                subGridComponent = $find(this._subGridId);
                if (subGridComponent && subGridComponent._parentSelectsSubGrid)
                {
                    window.setTimeout('$(\"' + this.GridID + '\").expandSubGridRow(\"' + rowId + '\")', 1);
                }
            }
        }
    },

    mouseClick : function(e)
    {
        var $t = $(e.target);

        if ($t.is('td[chevron]') && this.get_editingMode() != 'Inline')
        {
            if (e.clientX > ($t.offset().left + $t.width() - 20))
            {
                // jusr ignore click on chevron
                return false;
            }
        }

        if ($t.is('div.treeclick'))
        {
            // tree expand/collapse click 
            return false;
        }
        if ($t[0].parentElement.classList.contains('jqcboxrow') || $t.hasClass('cbox') || $t.hasClass('ui-jqgrid-sequence-cell-draggable'))
        {
            // Save grid row if editable
            var $edRow = $('tr[editable=1]', this.get_element());
            if ($edRow.length)
            {
                this.saveRowData($edRow);
                return false;
            }
        }    
    },

    IsClickInside: function (e) {
        var $clickedElement = $(e.target);
        var $gview = $('#gview_' + this._gridID + ' > div.ui-jqgrid-bdiv');
        if ($gview.length && $gview.is(':visible')) {
            if ($clickedElement.closest($gview).length > 0) {
                return true;
            }
        }
        return false;
    },

    directUpdate: function (directUpdateData)
    {
        var showModal = false;
        CamstarPortal.WebControls.JQGridBaseData.callBaseMethod(this, 'directUpdate');
        var directUpdateResponsedata = __json(directUpdateData.PropertyValue);
        if (directUpdateResponsedata)
        {
            if (directUpdateResponsedata.ResponseStatus)
            {
                if (directUpdateResponsedata.GridCallBack)
                {
                    var callBackFun = this[directUpdateResponsedata.GridCallBack];
                    if (callBackFun != undefined)
                    {
                        showModal = callBackFun.call(this, directUpdateResponsedata.trans_id, directUpdateResponsedata.ResponseData, directUpdateResponsedata.Action, directUpdateResponsedata.PostBackRequested, directUpdateResponsedata.RowID, directUpdateResponsedata.RowDataObject);
                    }
                    else if (directUpdateResponsedata.GridCallBack != "dummyEvent")
                    {
                        this.showMessage("callback function " + directUpdateResponsedata.GridCallBack + " is not found in the JQGridBaseData");
                    }
                    
                }
                else if (directUpdateResponsedata.PostBackRequested)
                {
                    this._do_postback(this._eventTargetId, directUpdateResponsedata.Action + ':' + directUpdateResponsedata.RowID);
                }
                if (directUpdateResponsedata.ResponseStatus != "ok")
                {
                    this.showMessage(directUpdateResponsedata.ResponseStatus);
                }
                else if (directUpdateResponsedata.BubbleMessage)
                {
                    this.showMessage(directUpdateResponsedata.BubbleMessage, 5000);
                }
            }
        }

        if (!showModal) {
            __page.directUpdateShowsModal = false;
            __page.hideModal();
        }
        else {
            __page.directUpdateShowsModal = true;
            __page.showModal();
        }
    },

    _onCellActionClick: function (actionElement, actionId, handler)
    {
        actionId = actionId || $(actionElement).attr("actionMapId");
        handler = handler || $(actionElement).attr("actionHandler");

        if (handler) {
            // call custom function
            return window[handler](this, actionElement, actionId);
        }

        // Possible we should simplyfy this with jQuery 
        var row = $(actionElement).parentsUntil("TR").parent();
        if (row.is('[empty-row]'))
        {
            // Ignore click for empty row
        }
        else
        {
            if (row.attr("editable") == "1")
            {
                // Finish editing to save the values
                var me = this;
                this.saveRowData(row.attr("id"), function()
                {
                    me._sendRequest({ ActionID: actionId, RowID: row.attr("id") }, "CellActionClick", "CellActionClickCallBack");
                });
            }
            else
            {
                var subGridContextId = this._parentGridID + this.__subGridContextSeparator + row.parents(".ui-subgrid").prev().attr("id");
                var callParameters = { ActionID: actionId, RowID: row.attr("id"), SubContextID: subGridContextId};
                this._sendRequest(callParameters, "CellActionClick", "CellActionClickCallBack");
            }
        }
        return false;
    },

    CellActionClickCallBack: function (trans_id, data, action, postbackRequested)
    {
        var resp = __json(data);
        if (resp.CellActionArgs)
        {
            if (postbackRequested)
            {
                this._do_postback(this._eventTargetId, 'OnCellActionClick:' + resp.CellActionArgs /* "fieldID:cellActionID:rowid" */);
            }
            else
            {
                eval(resp.PageToRedirectPopup);
            }
            return false;
        }
        else if (resp.PopupActionArgs)
        {
            if (postbackRequested)
            {
                var rowid = resp.PopupActionArgs.split(':')[2];
                var typeIndexStr = $(this.GridID + ' tr[role="row"][id="' + rowid + '"]').attr('rowTypeIndex');
                var typeIndex = typeIndexStr ? parseInt(typeIndexStr) : 0;

                this._do_postback(this._eventTargetId, 'PopupEdit:' + resp.PopupActionArgs +':' + typeIndex);
            }
            else
            {
                eval(resp.PageToRedirectPopup);
            }
            return false;
        }
        else
        {
            return false;    // Hide Modal         
        }
    },

    _onSubGridBeforeExpand: function (subgridId, rowid)
    {
        if (rowid.substr(0, this.__emptyRowPrefix.length) == this.__emptyRowPrefix)
        {
            return false;
        }
        else
        {
            if (!$(this.GridID + ' tr[id="' + rowid + '"] td.ui-sgcollapsed').is(':visible'))
                return false;
        }
    },

    _onSubGridExpanded: function (subgridId, rowid)
    {
        var tempGridId = this.GridID;
        if (this._collapseOtherOnExpanding)
        {
            // find expanded rows
            var expRows = $(".sgexpanded").each(
                function (index, el)
                {
                    // el is TD
                    var expandedRowid = $(el).parent().attr("id");
                    window.setTimeout('$(\"' + tempGridId + '\").collapseSubGridRow(\"' + expandedRowid + '\")', 1);
                }
            );
        }

        var callParameters = { "RowID": rowid };

        // First the new context wll be created
        var subGrid = $find(this._subGridId);
        if (subGrid)
        {
            callParameters.RowDataObject = subGrid._contextID;
        }
        this._sendRequest(callParameters, "ExpandRow", "ExpandRow_completed");
    },

    ExpandRow_completed: function (trans_id, responseData, action, isPostBack, rowid)
    {
        var subgridId = this._gridID + '_' + rowid;
        var subgrid_table_id = subgridId + "_t";

        var subGrid = $find(this._subGridId);
        if (subGrid)
        {
            var subgridInit = __json(responseData);
            if (subgridInit.data && subgridInit.data.records == 0 && subGrid._hideEmptySubGrid)
            {
                // If subgrid is empty - do not expand
                // Click on expand button to get it back to not expanded
                $(this.GridID + ' tr[id="' + rowid + '"] td.ui-sgcollapsed > a').click();
                return;
            }
            
            if (typeof subgridInit.data === 'undefined')
                subgridInit.data = responseData;
            
            jQuery("#" + subgridId).html("<table id='" + subgrid_table_id + "' class='scroll'></table><div id='" + subgrid_table_id + "_pager'></div>");
            var rx = new RegExp(this._subGridId, 'g');
            var init = subGrid._initialSettings.replace(rx, subgrid_table_id);
            var prms =
            {
                contextId: this.get_contextId() + this.__subGridContextSeparator + rowid,
                initialSettings: init,
                serverType: subGrid.get_serverType(),
                controlId: subgrid_table_id,
                editorSettings: subGrid.get_editorSettings(),
                editingMode: subGrid.get_editingMode(),
                eventTargetId: subGrid.get_eventTargetId(),
                customEventHandlers: subGrid.get_customEventHandlers(),
                subGridId: subGrid.get_subGridId(),
                parentSelectsSubGrid: subGrid.get_parentSelectsSubGrid(),
                rowSelectionMode: subGrid.get_rowSelectionMode(),
                initialData: { ResponseData: subgridInit.data, ForceReload: false, ParentGridID: this._gridID },
                columnModel: subgridInit.newColModel
            };

            var subGridComponent = $create(CamstarPortal.WebControls.JQGridBaseData, prms, null, null, $get(subgrid_table_id));
            // fix margin for the subgrid instance
            var subGridDiv = $("#gbox_" + subgrid_table_id);
            subGridDiv.addClass("ui-jqgrid-subgrid-child");
            subGridDiv.find('.ui-jqgrid-pager').addClass("ui-jqgrid-subgrid-child");
            subGridComponent._templateContext = this._subGridId;

            // fix vertical bar 
            if ($(this.GridID).getGridParam("multiboxonly"))
            {
                var subGridFill = $(this.GridID + ' tr[id="' + rowid + '"]:first').next().children('td').first();
                if (subGridFill.length > 0 && subGridFill.attr('colspan') == '1')
                {
                    // enlarge next
                    var nextTd = subGridFill.next();
                    nextTd.attr('colspan', '2');
                    // remove 1st td
                    subGridFill.remove();
                }
            }
        }
    },

    ClickCheckbox_Completed: function (trans_id, responseData, action, isPostBack, rowid, rowDataObject)
    {
        if (this._controlCustomHandlers[rowDataObject] != null)
            this._controlCustomHandlers[rowDataObject].click(this, rowid);

        if (this._isHorizonStyle)
            $.jgrid.updateHorizonCheckboxes(this, rowid);

    },

    _onSubGridCollapsed: function (subgridId, rowid)
    {
        var subgrid_table_id = subgridId + "_t";
        var subGridComponent = $find(subgrid_table_id);
        if (subGridComponent)
        {
            subGridComponent._setupActionHandler(true, false);
            subGridComponent.dispose();
        }

        if (rowid.substr(0, this.__emptyRowPrefix.length) != this.__emptyRowPrefix)
        {
            var callParameters =
            {
                "RowID": rowid /*prms.contextId*/
            };
            this._sendRequest(callParameters, "CollapseRow", "dummyEvent");
        }
    },

    _exportToExcelSelector: function ()
    {

        var prms = $.jgrid.camstar.ExcelExport;
        if (this.locales && this.locales.camstar && this.locales.camstar.ExcelExport)
            $.extend(true, prms, this.locales.camstar.ExcelExport);

        var divId = this._gridID + '_excelSelectorDiv';
        var excelFlyout = $('#' + divId);
        if (excelFlyout.length == 1)
            excelFlyout.closest('.flyout.grid-flyout').remove();


        // create the excel selector div
        var button = $(this.PagerID + ' .grid-flyout' + ' .ui-icon-excel');
        if (button.length == 0) {
            button = $(this.PagerID + ' .ui-icon-excel');
            if (button.length == 0)
                button = $("#gbox_" + this._gridID + ' .ui-icon-excel');
            $(button).prop("id", this.PagerID.substr(1) + '_excelSelectorTrigger');
            var divHtml =
            '<div id="' + divId + '" trigger-id="excelSelector-trigger">' +
                '<ul>' +
                    '<span class=ui-export-submenu>' + prms.excelExportCaption + '</span>' +
                    '<li mode=excel_all_all><span class="ui-icon ui-icon-excel-all"> </span><span>' + prms.allRecOption + '</span></li>' +
                    '<li mode=excel_cur_all><span class="ui-icon ui-icon-excel-current-allcols"></span><span>' + prms.currRecAllColsOption + '</span></li>' +
                    '<li mode=excel_cur_cur><span class="ui-icon ui-icon-excel-current"></span><span>' + prms.currRecCurColsOption + '</span></li>' +
                    '<span class=ui-export-submenu>' + prms.csvExportCaption + '</span>' +
                    '<li mode=csv_all_all><span class="ui-icon ui-icon-excel-all"> </span><span>' + prms.allRecOption + '</span></li>' +
                    '<li mode=csv_cur_all><span class="ui-icon ui-icon-excel-current-allcols"></span><span>' + prms.currRecAllColsOption + '</span></li>' +
                    '<li mode=csv_cur_cur><span class="ui-icon ui-icon-excel-current"></span><span>' + prms.currRecCurColsOption + '</span></li>' +
                '</ul>' +
                '</div>';

            var opt = {
                trigger: button,
                show: true,
                className: 'grid-flyout',
                location: 'above',
                offset: { left: 50, top: 0 },
                header: { text: prms.caption }
            };

            if (this._isHorizonStyle) {
                opt.location = 'below';
                opt.offset.left = 0;
            }

            var flyoutWidget = $(divHtml).flyout(opt);

            $('td.ui-state-hover').addClass('ui-pg-button-active'); //add active style to excel button

            $(document).one('mousedown', function (e) {
                $('td.ui-pg-button-active').removeClass('ui-pg-button-active'); //remove active style from excel button
                if ($('.grid-flyout').length > 0 && $(e.target).filter('div.grid-flyout *').length == 0) {
                    $('.grid-flyout').css('display', 'none');
                }
            })

            excelFlyout = $('#' + divId);
            var panelZIndex = button.parents("div.cs-picklist-panel").css("z-index");
            if (panelZIndex) {
                var flyoutZIndex = parseInt(panelZIndex) + 1;
                excelFlyout.parents("div.grid-flyout").css("z-index", flyoutZIndex);
            }
            var me = this;
            excelFlyout.find('li').bind('click', function (e)
            {
                flyoutWidget.flyout('close');
                me._exportToExcel(e.currentTarget);
            });
        }
    },

    _exportToExcel: function (t)
    {
        var callParameters = {};
        callParameters.RowDataObject = $(t).attr('mode');
        this._sendRequest(callParameters, "ExportToExcel", "ExportToExcel_completed");
    },

    ExportToExcel_completed: function (trans_id, responseData, action, isPostBack, rowid)
    {
        var fileName = responseData;
        var versioning = 'randomCode';
        StartDownloadFile(fileName, versioning, 0);
    },

    _onResizeColumn: function (newwidth, index)
    {
        // Hide text flyout if it's opened
        $('td[chevron=open] img').click();

        var theGrid = $(this.GridID);
        var cols = theGrid.getGridParam('colModel');
        if (typeof this.widthMultiplier != "undefined")
            newwidth = newwidth / this.widthMultiplier;

        var colState = [cols[index].index, newwidth];

        var serialized = JSON.stringify(colState, this.jsonReplacer);
        var callParameters = { "RowDataObject": serialized };

        // setup chevron for overflowed cells
        $.jgrid.wrapCells(this, cols[index].name);

        // Save grid row if editable
        var $editingRow = $('tr[editable=1]', this.get_element());
        if ($editingRow.length) 
        {
            this.saveRowData($editingRow);
            this._sendRequest(callParameters, "ColumnResized");
        }
        else
            this._sendRequest(callParameters, "ColumnResized");
    },

    _setupActionHandler: function (release, editing /*no adding*/)
    {
        if (this._editorSettings && this._editorSettings.ActionsMap)
        {
            var me = this;
            // attach handler to action controls
            Array.forEach(this._editorSettings.ActionsMap,
            function (actMap)
            {
                var ctl = $("#" + actMap.EditorClientId);
                if (ctl.length > 0)
                {
                    ctl.unbind('click');
                    if (!release)
                    {
                        ctl.bind('click', function (e)
                        {
                            me._formControlAction(actMap.Action);
                            return false;
                        });
                    }
                    if (me._editorSettings.HideInactiveButtons == true)
                    {
                        if ((actMap.Action == 0 /*Add*/ && !editing) || (actMap.Action == 1 /*Update*/ && editing))
                            ctl.show();
                        else
                            ctl.hide();
                    }
                    else
                    {
                        if ((actMap.Action == 0 /*Add*/ && !editing) || (actMap.Action == 1 /*Update*/ && editing))
                            ctl.removeAttr('disabled');
                        else
                            ctl.attr('disabled', 'disabled');
                    }
                }
            }, null);
            var element;
            if (editing)
            {
                var columnModel = $(this.GridID).getGridParam("colModel");
                var editableColums = [];
                for (var j = 0; j < columnModel.length; j++)
                {
                    if (columnModel[j].editable)
                        editableColums.push(columnModel[j].index);
                }
                for (var k = 0; k < this._editorSettings.ColumnControlsMap.length; k++)
                {
                    if ($.inArray(this._editorSettings.ColumnControlsMap[k].ColumnName, editableColums) == -1)
                    {
                        element = $("#" + this._editorSettings.ColumnControlsMap[k].EditorClientId);
                        if (element[0].tagName == "SPAN")
                            $(":input", element).attr('disabled', 'disabled');
                        else if (element[0].tagName == "INPUT")
                            element.attr('disabled', 'disabled');
                    }
                }
            }
            else
            {
                for (var i = 0; i < this._editorSettings.ColumnControlsMap.length; i++)
                {
                    element = $("#" + this._editorSettings.ColumnControlsMap[i].EditorClientId);
                    if (element.length > 0 && element[0].tagName == "SPAN")
                        $(":input", element).removeAttr('disabled');
                    else if (element.length > 0 && element[0].tagName == "INPUT")
                        element.removeAttr('disabled');
                }
            }
        }
    },

    _formControlAction: function (action)
    {
        switch (action)
        {
            case 0 /*Add*/:
                this.SaveFormControlData(true);
                return false;
                break;
            case 1 /*Save*/:
                this.SaveFormControlData(false);
                break;
                return false;
        }
    },

    _onGroupClick: function (grpid, stateCollapsedAfterClick)
    {
        if (this.__ignoreGroupClick && this.__ignoreGroupClick == true)
        {
            return;
        }

        var callParameters =
        {
            "RowDataObject": stateCollapsedAfterClick,
            "RowID": grpid
        };

        this._sendRequest(callParameters, "GroupClick", "dummyEvent");

        if (stateCollapsedAfterClick == false)
        {
            // setup chevron for overflowed cells
            $.jgrid.wrapCells(this);
        }
    },

    expandGroups: function (grps)
    {
        this.__ignoreGroupClick = true;
        var theGrid = $(this.GridID);
        var globallyCollapsed = theGrid.getGridParam('groupingView').groupCollapse;
        Array.forEach(grps, function (g)
        {
            if (g[1] /*isExpanded*/ == globallyCollapsed)
                theGrid.groupingToggle(g[0]);
        }, null);
        delete this.__ignoreGroupClick;
    },

    isDataChanged: function ()
    {
        return this._dataChanged;
    },

    displaySelectedCount: function ()
    {
        if (this._selectedCountFormat && (this.get_rowSelectionMode() === "MultiRowSelect" || this.get_rowSelectionMode() === "CheckBox"))
        {
            var selcount = $(this.PagerID + ' .ui-selected-count');
            if (selcount.length == 0) {
                // Create a box for selected count (at the left of Display Records .. )
                var $pgInfo = $(this.PagerID + ' .ui-paging-info');
                if (this._isHorizonStyle) 
                    $pgInfo.after("<div class=selected-rows><span class=ui-selected-count>0</span><span>item(s) selected</span></div>");
                else
                    $('<div class=ui-selected-count></div>').insertBefore($pgInfo);

                selcount = $(this.PagerID + ' .ui-selected-count');
            }

            selcount.text(this._totalSelectedRows.toString());
            selcount.parent().prop('title', $.jgrid.format(this._selectedCountFormat, this._totalSelectedRows));
            selcount.parent().attr("count", this._totalSelectedRows);
        }
    },

    _adjustTree: function(theGrid)
    {
        var me = this;
        theGrid.setGridParam({
            beforeRequest: function (ts)
            {
                var callParameters = { RowDataObject: this.p.postData.nodeid, ExpandedIndexs: [] };
		        // keep expanded indexes for tree mode.
                $("div.tree-minus", "#" + $.jgrid.jqID(this.p.id)).each(
                    function()
                    {
                        var id = $(this).parents('tr[role=row]').attr('id');
                        if (id)
                        {
                            callParameters.ExpandedIndexs.push(id);
                        }
                    });
                me._sendRequest(callParameters, "LoadTreeNode", "Reload_complete");
                return false;
            }
        });
    },

    _moveRow: function(e)
    {
        // get selected row
        var theGrid = $(this.GridID);
        var nowSelection = $('tr[aria-selected=true]', theGrid);

        if (nowSelection.length == 0)
        {
            alert("Select a row to move");
        }
        else if (nowSelection.length > 1)
        {
            alert("Select a just one row to move");
        }
        else
        {
            if (e && e.target)
            {
                var up = $(e.target).hasClass('icon-row-up');
                var $r = nowSelection.first();

                var moveDirection = 0;

                if (up)
                {
                    if ($r.prev().length && $r.prev().hasClass('jqgfirstrow') == false)
                    {
                        $r.insertBefore($r.prev());
                    }
                    else
                    {
                        moveDirection = -1;
                    }
                }
                else // down
                {
                    if (! $r.is(':last-child') && ! $r.next().is('[id^="'+ this.__emptyRowPrefix+ '"]')) 
                    {
                        $r.insertAfter($r.next());
                    }
                    else if ($r.is(':last-child'))
                    {
                        moveDirection = 2;  // to the next page
                    }
                    else
                    {
                        moveDirection = 1; 
                    }
                }

                this.reorderRows($r, moveDirection);
            }
        }

    },

    __fixGroupHeaders: function(args)
    {
        var theGrid = args[0];
        var me = args[1];

        // Group header modification
        var oldColspan = $('tr.jqgroup', theGrid).first().children('td:first-child').prop('colSpan');

        $('tr.jqgroup', theGrid).each(function ()
        {
            var $row = $(this);
            $row.attr('advanced', '');
            var sp = $row.find('span').detach();

            var grpHtml = $row.html();
            $row.html('<td role=expandercell />' + grpHtml);
            $(this.cells[0]).append(sp);

            // set group checkbox checked if selected all items in its group
            var selectedGroup = true;
            $row.nextUntil('tr.jqgroup').each(function ()
            {
                // if any item is not selected the group is not checked
                if (!$(this).is('[aria-selected=true]'))
                {
                    selectedGroup = false;
                }
            });
        });
    },

    UpdateRowStyles: function(trans_id, responseData, action, postbackRequested, rowid)
    {
        var rx = __json(responseData);
        var $tr = this.$getRow(rowid);
        if ($tr.length)
        {
            if (rx.rowNormalStyle)
                $tr.attr('rowNormalStyle', rx.rowNormalStyle);
            else
                $tr.removeAttr('rowNormalStyle');
            if (rx.rowSelectedStyle)
                $tr.attr('rowSelectedStyle', rx.rowSelectedStyle);
            else
            {
                $tr.removeAttr('rowSelectedStyle');
            }
        }
        this._customRowStyling = true;
        $.jgrid.toggleRowStyle($tr, false);
        if ($tr.hasClass('.ui-state-highlight'))
            $.jgrid.toggleRowStyle($tr, true);
    },

    _multiTypesAdd: function ($addBtn)
    {
        var divId = this._gridID + '_multiTypesAddSelectorDiv';
        var flyoutDiv = $('#' + divId);
        if (flyoutDiv.length)
            $('#' + divId).closest('.flyout.grid-flyout').remove();

        var me = this;
        var flyoutDiv = $('#' + divId);
        if (flyoutDiv.length == 0)
        {
            // create the multiadd selector div
            var divHtml = '<div id="' + divId + '" trigger-id="multiTypesAdd-trigger"><ul>';
            Array.forEach(this._editorSettings.SubClassMap,
                function (m, i)
                {
                    divHtml += ('<li typeIndex=' + i + ' typeClass=' + m.SubClassName + '><span class="ui-icon ui-icon-excel"> </span><span>' + m.ClassLabelText + '</span></li>');
                });
            divHtml += ('</ul></div>');

            var flyoutWidget = $(divHtml).flyout({
                trigger: $addBtn,
                show: true,
                className: 'grid-flyout',
                location: this._isHorizonStyle ? 'below' : 'above',
                offset: !this._isHorizonStyle ? { left: 50, top: 0 } : { left: 0, top: -10 }, 
                header: { text: (me.locales && me.locales.camstar && me.locales.camstar.EditingDialog && me.locales.camstar.EditingDialog.AddMultiTypeTitle) ? me.locales.camstar.EditingDialog.AddMultiTypeTitle : $.jgrid.camstar.EditingDialog.AddMultiTypeTitle }
            });

            flyoutDiv = $('#' + divId);
            flyoutDiv.find('li').bind('click', function (e)
            {
                flyoutWidget.flyout('close');
                // get type index
                me._onAddRow(parseInt($(e.currentTarget).attr('typeIndex')));
            });
        }
    },

    setRowPoinerCell: function($parentElement, columnModel)
    {
        var seqClass = null;
        var sequenceColumn = this._sequenceColumn;
        var gridId = this._gridID;

        if (this._sequenceMode.indexOf("Drag") != -1)
        {
            if (columnModel)
            {
                Array.forEach(columnModel, function(c, ind)
                {
                    if (c.editable && !c.hidden)
                    {
                        $('td[aria-describedby="' + gridId + '_' + c.name + '"]', $parentElement).addClass('cell-non-draggable');
                    }
                    if (c.name == sequenceColumn)
                    {
                        seqClass = c.classes;
                    }
                });
            }
            else
            {
                seqClass = this._sequenceStyleClass;
            }
        }

        $('td[aria-describedby="' + gridId + '_' + sequenceColumn + '"]', $parentElement).css('color', 'transparent');
        if (seqClass)
        {
            if ($parentElement.is('tr'))
            {
                $('td[aria-describedby="' + gridId + '_' + sequenceColumn + '"]', $parentElement)
                    .addClass(seqClass);
            }
            else
            {
                $('tr[id^="' + this.__emptyRowPrefix + '"] td[aria-describedby="' + gridId + '_' + sequenceColumn + '"]', $parentElement)
                    .removeClass(seqClass);
            }
            this._sequenceStyleClass = seqClass;
        }

        if (this._sequenceMode == "ArrowMoving")
        {
            var tooltip = $.jgrid.camstar.Sequencing.selectorTooltip;
            if (this.locales && this.locales.camstar && this.locales.camstar.Sequencing && this.locales.camstar.Sequencing.selectorTooltip)
                tooltip = this.locales.camstar.Sequencing.selectorTooltip;
            if ($.trim(tooltip).length)
                $('td[aria-describedby="' + gridId + '_' + sequenceColumn + '"]', $parentElement).prop('title', tooltip);
        }
    },

    getColumnOfTD: function($td)
    {
        var columnID = $td.attr('aria-describedby').substr(this._gridID.length + 1);
        return  $(this.GridID).getColProp(columnID);
    },

    resize: function () {
        $(this.GridID).jqGrid("resizeGrid");
    },

    stylingCheckboxes: function ($cb) {
        if (this._isHorizonStyle) {
            if ($cb.hasClass("jqgrow")) {
                // If the paramneter is a grid row 
                // fix left selector checkbox
                $.jgrid.setStyledCb($('.styled-cb input.cbox', $cb));
                // fix data checkboxes
                $("td[editable-checkbox] :checkbox", $cb).each(function () {
                    // If initialization is lost - initialize again
                    if ($("label", this.parentElement).length == 0)
                        $.jgrid.initHorizonCheckbox($(this));
                    else
                        $.jgrid.setStyledCb($(this));
                });

            }
            else {
                $.jgrid.setStyledCb($cb);
            }
        }
    },

    // Dll name should be specified
    get_serverType: function () { return this._serverType; },
    set_serverType: function (value) { this._serverType = value; },

    get_initialSettings: function () { return this._initialSettings; },
    set_initialSettings: function (value) { this._initialSettings = value; },

    get_initialData: function () { return this._initialData; },
    set_initialData: function (value) { this._initialData = value; },

    get_controlId: function () { return this._gridID; },
    set_controlId: function (value) { this._gridID = value; this.GridID = "#" + value; this.PagerID = "#" + value + "_pager" },

    get_contextId: function () { return this._contextID; },
    set_contextId: function (value) { this._contextID = value; },

    get_eventTargetId: function () { return this._eventTargetId; },
    set_eventTargetId: function (value) { this._eventTargetId = value; },

    get_editingMode: function () { return this._editingMode; },
    set_editingMode: function (value) { this._editingMode = value; },

    get_editorSettings: function () { return this._editorSettings; },
    set_editorSettings: function (value)
    {
        if (value)
            this._editorSettings = __json(value);
    },

    get_subGridId: function () { return this._subGridId; },
    set_subGridId: function (value) { this._subGridId = value; },

    get_hidden: function () { return this._hidden; },
    set_hidden: function (value) { this._hidden = value; },

    get_customEventHandlers: function () { return this._customEventHandlers; },
    set_customEventHandlers: function (value) { this._customEventHandlers = value; },

    get_collapseOtherOnExpanding: function () { return this._collapseOtherOnExpanding; },
    set_collapseOtherOnExpanding: function (value) { this._collapseOtherOnExpanding = value; },

    get_expandGroupIfRowSelected: function () { return this._expandGroupIfRowSelected; },
    set_expandGroupIfRowSelected: function (value) { this._expandGroupIfRowSelected = value; },

    get_rowSelectionMode: function () { return this._rowSelectionMode; },
    set_rowSelectionMode: function (value) { this._rowSelectionMode = value; },

    get_selectedCount: function () { return this._totalSelectedRows; },
    set_selectedCount: function (value)
    {
        if (value.substring(0, 1) == '(')
        {
            value = value.replace('(', '').replace(')', '');
        }
        this._totalSelectedRows = parseInt(value, 10);
    },

    get_columnModel: function () { return this._columnModel; },
    set_columnModel: function (value) { this._columnModel = value; },

    get_vscroll: function () { return this._vscroll; },
    set_vscroll: function (value) { this._vscroll = value; },

    get_statusToDisplay: function () { return this._statusToDisplay; },
    set_statusToDisplay: function (value) { this._statusToDisplay = value; },

    get_parentSelectsSubGrid: function () { return this._parentSelectsSubGrid; },
    set_parentSelectsSubGrid: function (value) { this._parentSelectsSubGrid = value; },

    get_responsiveWidth: function () { return this._responsiveWidth; },
    set_responsiveWidth: function (value) { this._responsiveWidth = value; }
}

$.extend($.fn.fmatter,
{
    checkBoxFormatter: function (cellvalue, options, rowObject)
    {
        var id = "closeWhenEmpty" + options.rowId;
        var name = "closeWhenEmpty" + options.rowId;
        return "<input type='checkbox' offval='yes' id='" + id + "' name='" + name + "'/>";
    }
});

$.extend($.jgrid.defaults, { recordtext: "Displaying {0} to {1} of {2} items", selectedCountFormat: '{0} selected'}); 
$.extend($.jgrid.col, { bSubmit: "OK" });

// Camstar locales extension
$.jgrid.camstar =
    {
        EditingDialog:
        {
            addTitle: 'Add',
            editTitle: 'Edit',
            ResetButton: 'Reset',
            OkButton: 'OK',
            CloseButton: 'Close',
            OkButtonCSS: 'cs-button',
            ResetButtonCSS: 'cs-button-secondary',
            CloseButtonCSS: 'cs-button-secondary',
            AddMultiTypeTitle: 'Choose class'
        },
        ColumnChooserDialog: 
        {
            caption: "Show Columns",
            title: "Show Columns",
            buttonicon: "ui-icon-triangle-1-s",
            showAllButton: "Show All",
            hideAllButton: "Hide All",
            visibleCaption: "Visible Columns",
            hiddenCaption: "Hidden Columns",
            oneColumnReqiredMsg: "At least one column must be visible",
            ApplyButton: "Apply",
            CloseButton: "Close"
        },
        MultiSortDialog: 
        {
            caption: "Show MultiSort",
            title: "Show MultiSort",
            buttonicon: "ui-icon-triangle-1-s"
        },
        ExcelExport:
        {
            caption: "Export Data",
            excelExportCaption: "Export To Excel",
            csvExportCaption: "Export To CSV",
            allRecOption: "All Records",
            currRecCurColsOption: "Current Records and Displayed Columns",
            currRecAllColsOption: "Current Records and All Columns"
        },
        Validation:
        {
            warning: "Warning",
            fieldRequired: "Field {0} is required<br>"
        },
        Sequencing:
        {
            selectorTooltip: "Select Row"
        },
        Timers:
        {
            headerTimer: 'Process Timer',
            headerTillMin: 'Till Minimum',
            headerTillMax: 'Till Maximum',
            footerItems: '{0} Items',
            captionTimers: 'Active Timers'
        }
    }
;

/********************************************** end JQuery Extensions for all grids **********************************************/

// Optional descriptor for JSON serialization.
CamstarPortal.WebControls.JQGridBaseData.descriptor =
{
    properties:
    [
        { name: 'controlId', type: String }, // server ID
        { name: 'contextId', type: String }, // grid context ID
        { name: 'eventTargetId', type: String }, // unique ID  - need for postback
        { name: 'serverType', type: String },
        { name: 'editingMode', type: String },
        { name: 'initialSettings', type: String },
        { name: 'editorSettings', type: String },
        { name: 'initialData', type: String },
        { name: 'subGridId', type: String },
        { name: 'hidden', type: Boolean },
        { name: 'collapseOtherOnExpanding', type: Boolean },
        { name: 'customEventHandlers', type: Array },
        { name: 'rowSelectionMode', type: String },
        { name: 'columnModel', type: Object },
        { name: 'vscroll', type: Number },
        { name: 'parentSelectsSubGrid', type: Boolean }
    ]
}

CamstarPortal.WebControls.JQGridBaseData.registerClass('CamstarPortal.WebControls.JQGridBaseData', Camstar.UI.Control);

function createInlineControl(value, options)
{
    var editorControlID = getInlineEditorControlID(options);
    var inpElem;
    var divObj;

    // get actual value in case of chevron cut
    var rowId = options.id.substr(0, options.id.lastIndexOf(options.ColumnName) - 1);
    var $td = $('#' + options.GridID + ' tr[id="' + rowId + '"] td[aria-describedby="' + options.GridID + '_' + options.ColumnName + '"]');
    if ($td.hasClass("wrapped"))
        value = $td.data('val');

    var inlineComponent = $find(editorControlID);
    if (inlineComponent)
    {
        if (inlineComponent._panel) {
            var panel = inlineComponent._panel;
            if (panel._filter) {
                panel._filterTextBox = $get(panel._filter.id + '_Fltc', panel._filter);
                $(panel._filterTextBox).val("");
            }
        }
        if (inlineComponent.get_element() == undefined)
        {
            log('component ' + editorControlID + ' not found');
            return null;
        }
        else
        {
            divObj = $('[id="' + editorControlID + "_Div" + '"]');
            if (divObj.length == 0)
                divObj = $(inlineComponent.get_element()).children("div").first();
            var grid = $find(options.GridID);
            if (divObj.length == 0)
            {
                //editing control is stored for restoration;
                if (grid._editingControls[editorControlID])
                    $(inlineComponent.get_element()).prepend(grid._editingControls[editorControlID]);
                grid._editingControls[editorControlID] = divObj.clone(true);
            }
            divObj.show();
            divObj.attr('id', editorControlID + "_Div");

            // Copy cs-* classes from the custom control
            var cl = inlineComponent.get_element().classList;
            for (var i = 0; i < cl.length; i++) {
                if (cl.item(i).startsWith("cs-"))
                    divObj.addClass(cl.item(i));
            }

            if (inlineComponent.directUpdate)
            {
                var prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Data), PropertyValue: value };
                if ($.isFunction(inlineComponent.isDropDown))
                    prm.PropertyKey = eval(Camstar.Ajax.DirectUpdateParameterKeys.DataText);
                inlineComponent.directUpdate(prm);

                prm = { PropertyKey: eval(Camstar.Ajax.DirectUpdateParameterKeys.Enable), PropertyValue: 'True' };
                inlineComponent.directUpdate(prm);
            }
            else
            {
                inpElem = divObj.children().first();
                if (!inpElem.is('input'))
                {
                    inpElem = inpElem.children().first();
                }
                inpElem.val(value);
                inpElem.removeAttr('state');
            }

            if (inlineComponent._editor)
            {
                // attach event handler if presented for the current field/column
                var grid = $find(options.GridID);
                if (grid && grid._controlCustomHandlers)
                {
                    var handlr = grid._controlCustomHandlers[options.ColumnName];
                    if (handlr && handlr.change)
                    {
                        $(inlineComponent._editor).bind('change', handlr.change);
                    }
                }
            }
        }
    }
    else
    {
        var dateChooser = $('#' + editorControlID);
        divObj = dateChooser.parent();
        if (divObj.is("span"))
        {
            divObj.show();
            divObj.attr('id', editorControlID + "_Div");
            divObj.addClass("cs-date");
            dateChooser.val(value);
        }
    }

    options.id = divObj.attr('id');

    return divObj[0];
}

function getSetValue(divObj, operation, value)
{
    if (divObj.length > 0)
    {
        var inpElem = divObj.find('input,textarea').first();
        if (inpElem.length > 0)
        {
            if (operation === 'get')
            {
                var v = inpElem.val();
                return v;
            }
            else if (operation === 'set')
            {
                inpElem.val(value);
            }
        }
    }
    else
        return "";
}

function getSetRevisionedValue(elem, operation, value)
{
    if (elem != null && elem.length > 0)
    {
        var elemId = elem.attr('id').replace(/\_div$/i, '');
        var inlineComponent = $find(elemId);
        if (inlineComponent != null)
        {
            if (operation === 'get')
            {
                return inlineComponent.getValue();
            }
            else if (operation === 'set')
            {
                inlineComponent.setRDOValue(value);
            }
        }
        else
        {
            log('getSetRevisionedValue: inlineComponent not found');
        }

    }
    else
    {
        log('getSetRevisionedValue: element empty!');
    }
    return "";
}

function getSetNamedSubentityValue(elem, operation, value)
{
    if (elem != null && elem.length > 0)
    {
        var elemId = elem.attr('id').replace(/\_div$/i, '');
        var inlineComponent = $find(elemId);
        if (inlineComponent != null)
        {
            if (operation === 'get')
            {
                return inlineComponent.getValue();
            }
        }
        else
        {
            log('getSetNamedSubentityValue: inlineComponent not found');
        }
    }
    else
    {
        log('getSetNamedSubentityValue: element empty!');
    }
    return "";
}

function getInlineEditorControlID(options)
{
    var gridID = options.GridID;
    var grid = $find(gridID);
    var editorControlID;
    if (grid != null)
    {
        var editorSettings = grid.get_editorSettings();
        var ColumnName = options.ColumnName;
        for (var i = 0; i < editorSettings.ColumnControlsMap.length; i++)
        {
            if (editorSettings.ColumnControlsMap[i].ColumnName == ColumnName)
            {
                editorControlID = editorSettings.ColumnControlsMap[i].EditorClientId;
                break;
            }
        }
    }

    return editorControlID;
}


function log(msg)
{
    // disable it in check-in
    //console.log(msg);
}

var jqGrid_locking = false;

function jqgrid_lock(v)
{
    if (v !== undefined)
    {
        jqGrid_locking = v;
    }
    return jqGrid_locking;
}

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

$(document).ready(function ($)
{
    $('body').mousedown(jqGrid_GeneralOnClick);
});

var jqGrid_instances = [];

function jqGrid_GeneralOnClick(e) {
    var idClickGrid = null; // keep id of grid which was clicked
    var idEditGrid = null; // keep id of grid which has editing row

    var $lastClicked = $(e.target).closest('td[role=gridcell]');
    window.LastClickedGridCell = $lastClicked.length ? $lastClicked.attr('aria-describedby') : null;

    for (var id in jqGrid_instances) // for all grid on the page
    {
        // return true if grid was clicked
        var isInside = jqGrid_instances[id].IsClickInside(e);
        var editingRow = $('tr[editable=1]', jqGrid_getGrid(id).get_element());
        if (isInside)
            idClickGrid = id;
        if (editingRow.length)
            idEditGrid = id;
    }
    // if we clicked on the grid with editing row
    if (idClickGrid && idEditGrid && idClickGrid == idEditGrid) {
        jqGrid_instances[idClickGrid].mouseClick(e);
    }
    else // if we clicked on other grid or place
    {
        if (idEditGrid && jqGrid_instances[idEditGrid].get_editingMode() == 'Inline')
        {
            var isInlineControl = false;
            var $t = $(e.target);

            while ($t.length)
            {
                if ($t.is('[id$="_InlineEditorControl"]') || $t.is('[id$="_InlineEditorControl_Panl"]') || $t.is('[class*="ui-datepicker"]'))
                {
                    isInlineControl = true;
                    break;
                }
                $t = $t.parent();
            }

            if (!isInlineControl)
            {
                // Save grid row if editable
                var $editingRow = $('tr[editable=1]', jqGrid_instances[idEditGrid].get_element());
                if ($editingRow.length)
                {
                    var textbox = document.activeElement;                  
                    window.idrow = $editingRow[0].id;
                    if (typeof textbox.blur !== "undefined")
                        textbox.blur();

                    if (e.target && $(e.target).is("span.ui-icon-trash")) 
                        $(e.target).attr("prevent-deletion", "true");

                    jqGrid_instances[idEditGrid].saveRowData($editingRow);
                }
            }
            else
            {
            }
        }
        else
        {
        }

    }
    return true;
}

function jqGrid_getGrid(id)
{
    return jqGrid_instances[id];
}

function __json(j) {
    if (typeof j == "string") 
        return j ? JSON.parse(j) : {};
    
    else 
        return j;
    
}
