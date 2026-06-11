// Copyright Siemens 2025 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../../jquery/jquery.min.js" />
/// <reference path="../../jquery/jquery-ui.min.js" />
/// <reference path="../../jquery/jqGrid/jquery.jqGrid.min.js" />


$.extend($.jgrid,
    {
        sort:
        {
            caption: "Sorting...",
            bSort: "Sort",
            bReset: "Reset",
            odata: ['ASC', 'DESC']
        },

        buildAjaxUrl: function () {
            // Create ajax url
            // should be like = "http://localhost/CamstarPortalT/AjaxEntry.axd?CallStackKey=Home";
            var l = window.location;
            var k;
            $.each(l.search.replace('?', '').split('&'), function (i, v) { if (v.startsWith('CallStackKey=')) k = v.split('=')[1]; });

            var p = l.pathname.substr(0, l.pathname.lastIndexOf('/') + 1);
            return l.protocol + '//' + l.host + p + 'AjaxEntry.axd?CallStackKey=' + k;
        },

        buildAjaxRequestData: function (g, serdata, action, rowid, callback) {
            var clientState =
            {
                Action: action,
                JQGridID: g._gridID,
                RowID: rowid,
                ContextID: g._contextID,
                CallStackKey: getParameterByName("CallStackKey"),
                GridCallBack: callback,
                RowDataObject: serdata
            };

            var serializedClientState = JSON.stringify(clientState);

            var transition =
            {
                ClientCallback: 'dummy',
                ID: g._gridID,
                Command: "ClientEntry",
                CommandParameters: serializedClientState,
                RequestType: 0,
                TargetType: "Camstar.WebPortal.FormsFramework.WebGridControls.JQDataGrid, Camstar.WebPortal.FormsFramework.WebGridControls"
            };

            return transition;
        },

        loadLocales: function (g, initialSettings) {
            // Override global locales and text
            if (initialSettings.locales) {
                for (var i = 0; i < initialSettings.locales.length; i++) {
                    var lcl = initialSettings.locales[i];
                    for (var p in lcl) {
                        if ($.jgrid[p] != undefined)
                            $.extend(true, $.jgrid[p], lcl[p]);
                    }
                }
            }

            g.locales = {};
            for (var j = 0; j < initialSettings.runtimeLocales.length; j++) {
                var rlcl = initialSettings.runtimeLocales[j];
                for (var r in rlcl) {
                    if (g.locales[r] != undefined)
                        $.extend(true, g.locales[r], rlcl[r]);
                    else
                        g.locales[r] = rlcl[r];
                }
            }

            g._validation = $.jgrid.camstar.Validation;
            if (g.locales && g.locales.camstar && g.locales.camstar.Validation)
                $.extend(true, g._validation, g.locales.camstar.Validation);
        },

        setupHeaderButtons: function (g, initialSettings) {
            var hbs = initialSettings.headerButtons || [];
            var theGrid = $(g.GridID);

            for (var i = 0; i < hbs.length; i++) {
                var it = hbs[i];
                var prms;
                if (it.ShowColumns === true) {
                    prms = $.jgrid.camstar.ColumnChooserDialog;
                    if (g.locales && g.locales.camstar && g.locales.camstar.ColumnChooserDialog)
                        $.extend(true, prms, g.locales.camstar.ColumnChooserDialog);

                    prms.onClickButton = function () {
                        g._onShowColumnChooser(prms);
                        if (g.IsResponsive) {
                            var $dialogWin = $(".cs-grid-dialog");
                            $dialogWin.width(530);
                            $("button.ui-column-chooser-button", $dialogWin).css("margin-top", "-14px");
                        }
                    };
                    $.jgrid.headerButtonAdd.call(theGrid, '#gview_' + g._gridID, prms);
                }

                else if (it.ShowMultiSort === true) {
                    prms = $.jgrid.camstar.MultiSortDialog;
                    if (g.locales && g.locales.camstar && g.locales.camstar.MultiSortDialog)
                        $.extend(true, prms, g.locales.camstar.MultiSortDialog);
                    prms.onClickButton = function () { g._onShowMultiSort(prms); };
                    $.jgrid.headerButtonAdd.call(theGrid, '#gview_' + g._gridID, prms);
                }
                else if (it.ShowFrozen === true) {
                    theGrid.setFrozenColumns();
                }
                else if (it.ShowSelectedRows === true) {
                    g._selectedCountFormat = $.jgrid.defaults.selectedCountFormat;
                }
                else if (it.ShowAllExpand === true) {
                    g._showAllExpand = true;
                }
                else if (it.Sequencing) {
                    var sm = it.Sequencing.split(':');
                    g._sequenceMode = sm[0];
                    g._sequenceColumn = sm[1];
                    if (g._sequenceMode.indexOf('Drag') != -1) {
                        theGrid.sortableRows(
                            {
                                items: '> :not([empty-row])',
                                cancel: '.cell-non-draggable',
                                update: function (ev, reorder) {
                                    g.reorderRows(reorder.item, 0);
                                }
                            });
                    }
                }
            }

            // set header css classes if defined
            Array.forEach(initialSettings.options.colModel, function (c) {
                if (c.headerclass)
                    $('th[id="' + g._gridID + '_' + c.name + '"]').addClass(c.headerclass);
            });
        },

        GetCompiledFunction: function (g, fnameOrText, key) {
            var fun = null;
            if (!$.isFunction(fnameOrText)) {
                if (fnameOrText.indexOf('{') != -1) {
                    // it's a inline function
                    fun = eval(g._gridID + '_' + key + '=' + fnameOrText);
                }
                else {
                    fun = window[fnameOrText];
                }
            }
            else {
                fun = fnameOrText;
            }
            return fun;
        },

        stripHtml: function (v) {
            v = String(v);
            var regexp = /<("[^"]*"|'[^']*'|[^'">])*>/gi;
            if (v) {
                v = v.replace(regexp, "");
                return (v && v !== '&nbsp;' && v !== '&#160;') ? v.replace(/\"/g, "&quot;") : "";
            }
            return v;
        },

        scrollbarWidth: function () {
            var div = $('<div style="width:50px;height:50px;overflow:hidden;position:absolute;top:-200px;left:-200px;"><div style="height:100px;"></div>');
            // Append our div, do our calculation and then remove it 
            $("body").append(div);
            var w1 = $("div", div).innerWidth();
            div.css("overflow-y", "scroll");
            var w2 = $("div", div).innerWidth();
            $(div).remove();
            return (w1 - w2);
        },

        closeFlyout: function () {
            var $f = $('#textflyout');
            $($f.data('chevron_element')).attr('chevron', 'closed');
            $f.fadeOut(500);
            $f.data('chevron_element', null);
            $('.footer .text', $f).text('');
            $f.width('');
            return false;
        },

        cutOverflowText: function () {
            var getTextNode = function ($el) {
                return $el.contents()
                    .filter(function () {
                        return this.nodeType == Node.TEXT_NODE;
                    })[0];
            };

            var $d = $(this);
            var $txt = $d;

            var correction = { width: 0, height: 0 };
            if ($d.is("th")) {
                $txt = $("#jqgh_" + $d.prop("id"), $d);
                if ($d.outerHeight() > 50) {
                    $d.addClass("big-hdr");
                    return;
                }
                else {
                    $d.removeClass("big-hdr");
                    correction.width = 4 + $(".s-ico", $txt).width();
                }
            }
            else if ($d.hasClass("instance-name")) {
                correction.width = 2;
            }
            else if ($d.parent().hasClass("jqgroup")) {
                correction.width = 0;
                var $tw = $(".jqgrid-custom-group-header.wrapper", $d);
                if ($tw.length) {
                    $txt = $(".instructionTextClass", $tw);
                    correction.width = $(".ui-icon-circlesmall-plus,.ui-icon-circlesmall-minus", $d).outerWidth() +
                        $(">div >b", $tw).outerWidth();
                }
            }

            var textNode = getTextNode($txt);
            if (!textNode) {
                return;
            }
            var txt = textNode.textContent;
            if ($d.hasClass("wrapped")) {
                txt = $d.attr("fulltext");
                $d.removeAttr("title").removeClass("wrapped");
                $("span[chevron]", $d).remove();
                textNode.textContent = txt;
            }

            if ($d.is(":hidden") || $d.is('.ui-sgcollapsed') || $d.hasClass('ui-jqgrid-sequence-cell') || $d.hasClass('ui-jqgrid-row-selector') || (txt.length == 0) /*|| (html == '&nbsp;')*/) {
                // if empty - not truncate
                return;
            }

            if ($d.is("[hastimers]")) {

                $d.prop("title", txt)
                    .addClass("wrapped")
                    .attr("fulltext", txt);

                var chevron = $("<span chevron=closed />")[0];
                if ($txt.hasClass("instructionTextClass")) {
                    $txt.parent().append(chevron);
                }
                else {
                    if ($txt[0].childNodes.length == 1)
                        $txt.append(chevron);
                    else
                        $txt[0].insertBefore(chevron, textNode.nextSibling);
                }

                var textSize = { width: $txt[0].scrollWidth, height: $txt[0].scrollHeight };
                var contSize = { width: Math.ceil($d.outerWidth()) - correction.width, height: Math.ceil($d.outerHeight()) - correction.height };
                while (textSize.width > contSize.width) {
                    textNode.textContent = textNode.textContent.substr(0, textNode.textContent.length - 1);
                    if (textNode.textContent.length == 0)
                        break;
                    textSize.width = $txt[0].scrollWidth;
                }
            }
        },

        // Call with debouncing
        wrapCells: function (g, columnname /*optional - wrapping just one column otherwise all visible columns*/) {

            if (g.wrapCellTimer)
                clearTimeout(g.wrapCellTimer);

            g.wrapCellTimer = setTimeout($.jgrid._wrapCells, g.__wrapCellTimeout, g, columnname);
        },

        // Internal 
        _wrapCells: function (g, columnname /*optional - wrapping just one column otherwise all visible columns*/) {
            $.jgrid.closeFlyout();

            if (!g._gridID) {
                // Wrong grid context - nothing to do
                return;
            }

            // Wrap headers            
            var tdId = columnname ? g._gridID + '_' + columnname : null;
            var rows = $("tr[role=row][id].jqgrow", g.get_element());

            var $viewPort = $('#gview_' + g._gridID + ' div.ui-jqgrid-bdiv');
            if (!$viewPort.length) {
                console.warn("viewport not found", g._gridID);
                return;
            }
            var viewPortTop = $viewPort[0].scrollTop;
            var viewPortBottom = $viewPort[0].scrollTop + $viewPort.height();

            // wrap
            var $r;
            for (var ir = 0; ir < rows.length; ir++) {
                $r = $(rows[ir]);
                if (!$r.is('[empty-row]')) {
                    if (g.wrapEverything || ($r[0].offsetTop + $r[0].offsetHeight) >= viewPortTop && $r[0].offsetTop < viewPortBottom) {
                        var cells = columnname ?
                            $('td[hastimers][role=gridcell][aria-describedby="' + tdId + '"]', $r) :
                            $('td[hastimers][role=gridcell]', $r);

                        // Wrap with adding chevron only for visible Timer/Duration cell
                        cells.each(function () {
                            var cell = $(this);
                            var style = window.getComputedStyle(cell[0]);
                            var isHidden = style.display === "none";
                            if (!isHidden)
                                $.jgrid.cutOverflowText.call(cell);
                        });
                    }
                }
            }

        },

        clickOnChevron: function (e, $chevron) {

            $chevron = $chevron || $(e.target);

            var $cell = $chevron.closest('td,th');

            // grid
            var $gridBox = $cell.closest('.ui-jqgrid');
            var gridId = $gridBox.prop("id").substr(5);     // remove prefix gbox_
            var grid = $find(gridId);

            if (!$chevron.is("[chevron]")) {
                if ($chevron.hasClass("ui-jqgrid-sortable") && $cell.prop("id")) {
                    var columnId = $cell.prop("id").substr(gridId.length + 1 /* 1 - underscore */);
                    var sortorder = $cell.attr("sorted");
                    if (!sortorder || sortorder === "desc")
                        sortorder = "asc";
                    else
                        sortorder = "desc";
                    var iCol = 0;               // index of column (doesn't care now)
                    grid._onColumnSorting(columnId, iCol, sortorder);
                }
                return;
            }

            var $fl = $('#textflyout');

            if ($chevron.is('[chevron=closed]')) {
                // Close opened chevron if any
                $('[chevron=open]', $gridBox).each(function () {
                    $(this).attr('chevron', 'closed');
                    $('.footer .text', $fl).text('');
                    $fl.data('chevron_element', null).width('');
                });

                $chevron.attr('chevron', 'open');

                $('div.content', $fl).css('height', '');
                var txt = $cell.attr("fulltext");

                var isDuration = $cell.is('[jqgrid-duration-autoupdate]');
                if (txt || isDuration) {
                    if (isDuration) {
                        var timers = $cell.data('timers');
                        if (timers.length) {
                            // Load more timers
                            // get container
                            var $tdContainer = $('td[aria-describedby="' + grid._gridID + '_Container' + '"]', $cell.parent());
                            var containerName = $tdContainer.text().trimEnd();

                            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), grid);
                            transition.set_command("GetContainerTimers");
                            transition.set_commandParameters(containerName);
                            transition.set_clientCallback("GetContainerTimers");
                            transition.set_noModalImage(true);
                            var oldServerType = grid._serverType;
                            var me = this;

                            grid.GetContainerTimers = function (r) {
                                var tmrs = eval(r.Data.HTML);
                                timers = timersWatcher.loadTimersFromContainer(tmrs);
                                $cell.data('timers', timers);
                                me.updateDurationFlyoutContent($fl, timers, grid);
                                me.UpdateDuration(false, grid);
                            }
                            grid._serverType = "Camstar.WebPortal.WebPortlets.TimersSupport,App_Code";

                            var communicator = new Camstar.Ajax.Communicator(transition, grid);
                            communicator.syncCall();
                            communicator.dispose();

                            grid._serverType = oldServerType;
                        }
                        this.updateDurationFlyoutContent($fl, timers, grid);
                    }
                    else {
                        $('.content', $fl).text(txt);
                    }
                }
                else {
                    var ht = $cell.text();
                    $('.content', $fl).html(ht);
                }

                var pos = { left: e.clientX, top: e.clientY };

                var width = $fl.width();

                var $parentBody = $cell.parents('body');
                var pointerShift = 18;
                var pointerLeft = 0;
                var horizShift = -pointerShift;            //on the right by the image

                if ((pos.left + width) > $parentBody.width()) {
                    horizShift = -width + pointerShift - 2; // on the left by the image 
                    pointerLeft = width - pointerShift * 2 + 3;
                }

                var top = pos.top + $cell.height();
                if ((top + $fl.height()) > $('body').height()) {
                    // The flyout is clipped - should be opened above the chevron
                    top -= ($fl.height() + 68);
                    $fl.attr('location', 'above');
                    $cell.addClass('up');
                }
                else {
                    $fl.attr('location', 'below');
                    $cell.removeClass('up');
                }

                var left = pos.left + horizShift + 1;

                $fl.css({ top: top + 'px', left: left + 'px' });
                $fl.data('chevron_element', $chevron[0]); // save associated chevron element

                // flyout close (x) image click definition 
                $('.footer img', $fl)
                    .unbind('click')
                    .bind('click', function () {
                        // close the flyout
                        var $f = $('#textflyout');
                        var $chv = $($f.data('chevron_element'));
                        $chv.attr('chevron', 'closed');
                        $f.fadeOut(500);
                        $f.data('chevron_element', null);
                        $('.footer .text', $f).text('');
                        $f.width('');
                        return false;
                    });

                $($fl.attr('location') == 'below' ? '.pointer-top' : '.pointer-bottom', $fl)
                    .show()
                    .css({ left: pointerLeft + 'px', width: $fl.width() + 'px', zIndex: '2' })
                    .unbind('click')
                    .bind('click', function () { return $('#textflyout .footer img').click(); });

                $('.pointer-bottom', $fl).css("top", $fl.height());

                // Calculate proper zIndex for the flyout panel
                var zIndex = 0;
                $($cell.parents('div')).each(function () {
                    if (zIndex == 0 && $(this).css('z-index') != 'auto')
                        zIndex = parseInt($(this).css('z-index'));
                });

                if (zIndex > 0)
                    $fl.css('z-index', (zIndex + 1).toString());

                $fl.fadeIn(500);
            }
            else {
                $chevron.attr('chevron', 'closed');
                $fl.fadeOut(500);
                $fl.data('chevron_element', null);
                $('.footer .text', $fl).text('');
                $fl.width('');
            }
        },

        updateDurationFlyoutContent: function ($fl, timers, grid) {
            // create a content
            var cont = '';
            var st;
            var isFirefox = navigator.userAgent.indexOf("Firefox") != -1;
            for (var i = 0; i < timers.length; i++) {
                var t = timers[i];
                timersWatcher.setTimerColors(t);
                var styleFix = isFirefox ? ' class=ff' : '';
                cont += ('<tr role=timerdata' + styleFix + '><td><div>' + t.name + '</div></td>');
                Array.forEach([t.min, t.max], function (tm) {
                    if (tm.actualColor)
                        st = ' style="background-color:' + tm.actualColor + '"';
                    else
                        st = '';
                    cont += ('<td><div' + st + '>' + (tm.active ? timersWatcher.formatDurationRel(tm.endTime) : '') + '</div></td>')
                });
                cont += '</tr>';
            }

            var $tbl = $('.content .jqgrid-timers-flyout table', $fl);
            if (!$tbl.length) {

                var lbls = $.jgrid.camstar.Timers;
                if (grid.locales)
                    if (grid.locales.camstar)
                        $.extend(true, lbls, grid.locales.camstar.Timers);

                // Create a owner table
                $tbl = $('.content', $fl).html(
                    '<div class=jqgrid-timers-flyout><table>' +
                    '<caption>' + lbls.captionTimers + '</caption>' +
                    '<tr role=timersheader><th><div>' + lbls.headerTimer + '</div></th>' +
                    '<th><div>' + lbls.headerTillMin + '</div></th>' +
                    '<th><div>' + lbls.headerTillMax + '</div></th>' +
                    '</tr>' +
                    cont +
                    '</table></div>');
                $tbl = $('table', $tbl);
                $('.footer .text', $fl).text(lbls.footerItems);
            }
            else {
                // Remove data rows
                $('tr[role=timerdata]', $tbl).remove();
                // Add data rows
                $('tr[role=timersheader]', $tbl).after(cont);
            }

            var txt = $('.footer .text', $fl).text();
            if (!txt)
                txt = $.jgrid.camstar.Timers.footerItems;
            txt = timers ? (timers.length.toString() + txt.substring(txt.indexOf(' '))) : '';
            $('.footer .text', $fl).text(txt);
        },

        UpdateDuration: function (initialization, g) {
            var me = this;

            // Set timeout for the next cycle
            if (window.gridTimers && window.gridTimers[g._gridID]) {
                window.clearTimeout(window.gridTimers[g._gridID]);
            }
            if (!window.gridTimers)
                window.gridTimers = {};

            window.gridTimers[g._gridID] = window.setTimeout(function () { me.UpdateDuration(false, g); }, g._durationUpdateInterval);

            // Update duration cell 
            var timeInterval = g._durationUpdateInterval / 1000;

            $("td[jqgrid-duration-autoupdate]", $(g.GridID)).each(
                function (ind, td) {
                    var $td = $(td);
                    var $tr = $td.parent();
                    var textNode = $td.contents().filter(function () { return this.nodeType == Node.TEXT_NODE; })[0];
                    var timers = [];
                    if (initialization) {
                        timersWatcher.interval = timeInterval;
                        var n = timersWatcher.getItem('TimersCount', g, $tr);
                        if (n) {
                            var cnt = parseInt(n);
                            if (cnt > 0) {
                                // We can take only first time from the query response
                                for (var i = 0; i < cnt; i++) {
                                    var t = { name: null, max: { active: false }, min: { active: false } };
                                    if (i == 0) {
                                        // First timer set. Other timers left empty
                                        t = timersWatcher.getTimerFromColumns(g, $tr);
                                    }
                                    timers.push(t);
                                }
                                timersWatcher.setTimerColors(timers[0]);
                            }
                        }

                        $td.data('timers', timers);

                        textNode.textContent = '';
                        if (timers.length) {
                            $td.attr('hasTimers', timers.length.toString());
                        }
                        else {
                            $td.removeAttr('hasTimers');
                        }
                        $td.attr('title', '');
                    }
                    else {
                        timers = $td.data('timers');
                        if (timers.length) {
                            timersWatcher.updateTimers(timers);
                            $td.data('timers', timers);
                        }
                    }

                    if (timers.length) {
                        // Find sooner timer to display in the grid cell
                        var t = timers[0];
                        var timerDisplayedInCell = t.max.active ? t.max : t.min; // This returns min/max timer

                        if (timerDisplayedInCell) {
                            textNode.textContent = timersWatcher.formatDurationRel(timerDisplayedInCell.endTime);
                            $td.css('background-color', timerDisplayedInCell.actualColor);
                        }
                    }
                    else {
                        textNode.textContent = ' ';
                    }

                    $td.attr("fulltext", textNode.textContent);

                    // Update flyout if its open
                    if ($td.is('[chevron=open]')) {
                        $.jgrid.updateDurationFlyoutContent($('#textflyout'), timers, g);
                    }
                }
            );
        },

        getLabelText: function (g, path) {
            var pp = path.split('.');
            var o = $.jgrid;
            Array.forEach(pp, function (p) {
                if (o[p])
                    o = o[p];
                else
                    o = path + ' not found';
            }, null);

            var ret = o;

            if (g.locales && g.locales.length > 0) {
                o = g.locales;
                Array.forEach(pp, function (p) {
                    if (o != null && o[p])
                        o = o[p];
                    else
                        o = null;
                }, null);
            }
            if (o != null)
                ret = o;
            return ret;
        },

        getUserAttributes: function (g, attrName, index) {
            var attrs = $('#gbox_' + g._gridID).attr(attrName).split(' ');
            return index == undefined ? attrs : attrs[index];
        },

        headerButtonAdd: function (elem, p) {
            p = $.extend({
                caption: "newButton",
                title: '',
                buttonicon: 'ui-icon-newwin',
                onClickButton: null,
                position: "last",
                cursor: 'pointer'
            }, p || {});
            return this.each(function () {
                if (!this.grid) { return; }
                if (elem.indexOf("#") !== 0) { elem = "#" + elem; }
                var findnav = $(".ui-jqgrid-titlebar", elem), $t = this;
                if (findnav) {
                    p.id = p.caption.replace(/[^a-zA-Z 0-9]|\s+/g, '');
                    // check is it already added
                    if (p.id && $("#" + p.id, findnav).length)
                        return;

                    var tbd = $("<div class=ui-jqgrid-titlebar-linkbutton><span class=button-text></span><span class=ui-icon></span></div>");
                    if (p.buttonicon.toString().toLowerCase() != "none")
                        $(">span.ui-icon", tbd).addClass(p.buttonicon);

                    $(">span.button-text", tbd).text(p.caption);

                    if (p.id) { $(tbd).attr("id", p.id); }
                    if (p.position == 'first') {
                        if (findnav.rows[0].cells.length === 0)
                            $("div", findnav).append(tbd);
                        else
                            $("div:eq(0)", findnav).before(tbd);
                    } else {
                        $(findnav).append(tbd);
                    }
                    $(tbd, findnav)
                        .attr("title", p.title || "")
                        .click(function (e) {
                            if (!$(this).hasClass('ui-state-disabled'))
                                if ($.isFunction(p.onClickButton)) { p.onClickButton.call($t, e); }

                            return false;
                        })
                        .hover(
                            function () {
                                if (!$(this).hasClass('ui-state-disabled')) {
                                    $(this).addClass('ui-state-hover');
                                }
                            },
                            function () { $(this).removeClass("ui-state-hover"); }
                        );
                }
            });
        },
        addCustomRowClass: function (cn, normalStyle, selectedStyle) {
            var exists = $('head #' + cn);
            if (exists.length == 0) {
                var cssrules = $("<style type='text/css' id=" + cn + "> </style>").appendTo("head");
                cssrules.append("." + cn + " {" + normalStyle + "}");
                if (selectedStyle)
                    cssrules.append("." + cn + ".ui-state-highlight {" + selectedStyle + "}");
            }
            return cn;
        },

        toggleRowStyle: function ($r, selected) {
            if (typeof $r == "number") {
                $r = $(this);
                selected = $r.hasClass('ui-state-highlight');
            }
            var cssFrom = $r.attr(selected ? 'rowNormalStyle' : 'rowSelectedStyle');
            var cssTo = $r.attr(!selected ? 'rowNormalStyle' : 'rowSelectedStyle');

            if (cssFrom && cssFrom.indexOf('class:') == 0)
                $r.removeClass(cssFrom.split(':')[1]);
            else
                $r.css('cssText', '');

            if (cssTo && cssTo.indexOf('class:') == 0)
                $r.addClass(cssTo.split(':')[1]);
            else if (cssTo)
                $r.css('cssText', cssTo);
        },
        preserveHtml: function (htmltext) {
            return htmltext.replace(/</g, '&lt;');
        },

        horizonStyling: function (g) {
            // Split title bar
            var $titleBar = $("#gview_" + g._gridID + " .ui-jqgrid-caption");
            var lblText = $(".ui-jqgrid-title", $titleBar).text();
            if (lblText) {
                $(".ui-jqgrid-title", $titleBar).hide();
                var $lbl = $("<div class='ui-jqgrid-title grid-label'></div>");
                $lbl.prop("id", "gtitle_" + g._gridID);
                $lbl.text(lblText);
                $lbl.toggleClass("subgrid-title", g._parentGridID ? true : false);

                if (g._hidden || $("#gbox_" + g._gridID).css("display") === "none")
                    $lbl.hide();
                else
                    $lbl.css("display", "");
                $("#gbox_" + g._gridID).before($lbl);
            }

            // Move action table
            var $actTbl = $("#gbox_" + g._gridID + " .ui-pg-table.navtable");
            var navBtns = $("td", $actTbl);
            var isNavEmpty = true;
            navBtns.each(function () {
                if ($(this).width() > 0) {
                    isNavEmpty = false;
                    return false;
                }
            });
            // add div for command table
            $('.ui-jqgrid-title', $titleBar).before("<div class='commands-table" + (isNavEmpty ? " empty" : "") + "'></div>");
            $actTbl.detach().appendTo($('.commands-table', $titleBar));
            $(g.PagerID + ' .ui-pg-button .ui-separator').parent().remove();
            if (!$titleBar.is(":visible")) {
                // Display title if it was hidden 
                $titleBar.css("display", "");
            }
        },

        setStyledCb: function ($cb) {

            if ($cb.length) {
                $cb.parent().attr("checked", $cb[0].checked ? $cb[0].checked : null)
                    .attr("disabled", $cb[0].disabled ? "disabled" : null);
                // set visibility of label
                $("label.lbl-checkbox", $cb.parent()).css("visibility", $cb.css("display") === "none" ? "hidden" : null);
            }
        },

        initHorizonCheckbox: function ($cb) {
            var me = this;
            var $$par = $cb.parent();
            var isFieldCheckbox = $cb.is("[offval]") || $cb.hasClass("editable");

            // Add label
            // set id for checkbox if it's not there
            if (!$cb.prop("id"))
                $cb.prop("id", $$par.attr("aria-describedby") + "_" + $$par.closest("tr").prop("id") + "_dcb");

            var $lbl = $("label.lbl-checkbox", $$par);
            if (!$lbl.length) {
                $$par.append("<label class=lbl-checkbox for=\"" + $cb.prop("id") + "\" />").addClass("styled-cb");
                $lbl = $("label.lbl-checkbox", $$par);
                $lbl.off().on('click', function () {
                    var $$cb = $(":checkbox", this.parentElement);
                    $$cb.click();

                    // If checkbox has callback function then its style will be updated in ClickCheckbox_Completed
                    if (!$$cb.hasClass("cb-callback"))
                        me.setStyledCb($$cb);
                    return false;
                });
            }

            $cb.on('focus', function (x) {
                $$par.children('label').first().addClass('focused');
            });

            $cb.on('blur', function (x) {
                $$par.children('label').first().removeClass('focused');
            });


            if (!isFieldCheckbox)
                $$par.addClass("selector-styled-cb").addClass("jqcboxrow");
            else
                $$par.addClass("data-styled-cb");


            $cb.on("change", function () {
                me.setStyledCb($(this));
            });

            this.setStyledCb($cb);
        },

        initHorizonSelectAllCheckBox: function (gview) {
            // Initilize header select-all checkbox

            // Set header cb
            var $header_cb = $('.ui-jqgrid-htable tr.ui-jqgrid-labels th[id$="_cb"] input.cbox', gview);

            if ($header_cb.length) {
                $header_cb.addClass("select-all");
                this.initHorizonCheckbox($header_cb);
                // set th width
            }
        },

        initHorizonSelectCheckboxes: function (gview) {

            // first row -- width template
            $(".ui-jqgrid-btable tr.jqgfirstrow > td.jqfirstcol", gview).addClass("selector-styled-cb");

            // set data checkboxes
            $(".ui-jqgrid-btable tr[role=row] td :checkbox", gview).each(function () {
                $.jgrid.initHorizonCheckbox($(this));
            });
        },

        updateHorizonCheckboxes: function (gview, rowid) {
            if (!rowid) {
                // set data checkboxes
                $(".ui-jqgrid-btable tr[role=row] td :checkbox", gview).each(function () {
                    $.jgrid.setStyledCb($(this));
                });

                var $header_cb = $('.ui-jqgrid-htable tr.ui-jqgrid-labels th[id$="_cb"] input.cbox', gview);
                $.jgrid.setStyledCb($header_cb);
            }
            else {

                // set data checkboxes for particular row
                $(".ui-jqgrid-btable tr[role=row]#" + rowid + " td :checkbox", gview).each(function () {
                    $.jgrid.setStyledCb($(this));

                    // Reinitialize checkbox change handler
                    $(this).one("change", function () {
                        $.jgrid.setStyledCb($(this));
                    });

                });
            }
        },

        disposeHorizonCheckboxes: function (gview) {
            // header cb
            $('.ui-jqgrid-htable tr.ui-jqgrid-labels th[id$="_cb"] input.cbox', gview).unbind();

            $(".ui-jqgrid-btable tr[role=row] td :checkbox", gview).unbind();
        },

        adjustPagerWidth: function ($pager, isSmallWdth) {
            var $msgAreaTD = $(".msg-area", $pager);
            var $tbl = $(">div > table", $pager);
            if (isSmallWdth) {
                if (!$(".small-width-row", $tbl).length) {
                    // create a new TR
                    $tbl.first().append("<tr class='small-width-row'></tr>");
                    $(".small-width-row", $tbl).append($msgAreaTD.detach());
                }
            }
            else {
                if ($(".small-width-row", $tbl).length) {
                    var $pager_td = $("> tbody > tr:first-child > td:first-child", $tbl);
                    $msgAreaTD.detach().insertBefore($pager_td);
                    $(".small-width-row", $tbl).remove();
                }
            }
        }

    }
);


$.jgrid.extend(
    {
        /*Camstar Custom Function*/
        multisortGrid: function (p) {
            p = $.extend({
                recreateSort: false,
                drag: true,
                sField: 'sortField',
                sValue: 'sortString',
                sOper: 'sortOper',
                sSort: 'sorts',
                loadDefaults: true,
                beforeShowSort: null,
                afterShowSort: null,
                onInitializeSort: null,
                closeAfterSort: true,
                closeAfterReset: false,
                closeOnEscape: true,
                multipleGroup: false,
                top: 0,
                left: 0,
                jqModal: true,
                modal: false,
                resize: false,
                width: 450,
                height: 'auto',
                dataheight: 'auto',
                sopt: null,
                stringResult: undefined,
                onClose: null,
                onSort: null,
                onReset: null,
                toTop: true,
                overlay: 10,
                columns: [],
                tmplNames: null,
                tmplSorts: null,
                // translations - later in lang file
                tmplLabel: ' Template: ',
                showOnLoad: false,
                layer: null
            }, $.jgrid.sort, p || {});
            return this.each(function () {
                var $t = this;
                if (!$t.grid) { return; }
                var fid = "fbox_" + $t.p.id,
                    showFrm = true,
                    IDs = { themodal: 'sortmod' + fid, modalhead: 'sorthd' + fid, modalcontent: 'sortcnt' + fid, scrollelm: fid },
                    defaultSorts = $t.p.postData[p.sSort];
                if (typeof (defaultSorts) === "string") {
                    defaultSorts = $.jgrid.parse(defaultSorts);
                }
                if (p.recreateSort === true) {
                    $("#" + IDs.themodal).remove();
                }
                function showSort() {
                    if ($.isFunction(p.beforeShowSort)) {
                        showFrm = p.beforeShowSort($("#" + fid));
                        if (typeof (showFrm) === "undefined") {
                            showFrm = true;
                        }
                    }
                    if (showFrm) {
                        $.jgrid.viewModal("#" + IDs.themodal, { gbox: "#gbox_" + fid, jqm: p.jqModal, modal: p.modal, overlay: p.overlay, toTop: p.toTop });
                        if ($.isFunction(p.afterShowSort)) {
                            p.afterShowSort($("#" + fid));
                        }
                    }
                }
                if ($("#" + IDs.themodal).html() !== null) {
                    showSort();
                } else {
                    var fil = $("<span><div id='" + fid + "' class='sortFilter' style='overflow:auto'></div></span>").insertBefore("#gview_" + $t.p.id);
                    if ($.isFunction(p.onInitializeSort)) {
                        p.onInitializeSort($("#" + fid));
                    }
                    var columns = $.extend([], $t.p.colModel),
                        bS = "<a href='javascript:void(0)' id='" + fid + "_sort' class='fm-button ui-state-default ui-corner-all fm-button-icon-right ui-reset'><span class='ui-icon ui-icon-shuffle'></span>" + p.bSort + "</a>",
                        bC = "<a href='javascript:void(0)' id='" + fid + "_reset' class='fm-button ui-state-default ui-corner-all fm-button-icon-left ui-sort'><span class='ui-icon ui-icon-arrowreturnthick-1-w'></span>" + p.bReset + "</a>",
                        bQ = "", tmpl = "", colnm, found = false, bt;

                    if (!p.columns.length) {
                        $.each(columns, function (i, n) {
                            if (!n.label) {
                                n.label = $t.p.colNames[i];
                            }
                            // find first sortable column and set it if no default sort
                            if (!found) {
                                var sortable = (typeof n.sort === 'undefined') ? true : n.sort,
                                    hidden = (n.hidden === true),
                                    ignoreHiding = (n.sortoptions && n.sortoptions.sorthidden === true);
                                if ((ignoreHiding && sortable) || (sortable && !hidden)) {
                                    found = true;
                                    colnm = n.index || n.name;
                                }
                            }
                        });
                    } else {
                        columns = p.columns;
                    }

                    found = false;
                    if (p.tmplNames && p.tmplNames.length) {
                        found = true;
                        tmpl = p.tmplLabel;
                        tmpl += "<select class='ui-template'>";
                        tmpl += "<option value='default'>Default</option>";
                        $.each(p.tmplNames, function (i, n) {
                            tmpl += "<option value='" + i + "'>" + n + "</option>";
                        });
                        tmpl += "</select>";
                    }

                    bt = "<table class='EditTable' style='border:0px none;margin-top:5px' id='" + fid + "_2'><tbody><tr><td colspan='2'><hr class='ui-widget-content' style='margin:1px'/></td></tr><tr><td class='EditButton' style='text-align:left'>" + bC + tmpl + "</td><td class='EditButton'>" + bQ + bS + "</td></tr></tbody></table>";
                    $("#" + fid).jqSort({
                        columns: columns,
                        sort: p.loadDefaults ? defaultSorts : null,
                        _gridsopt: $.jgrid.sort.odata,
                        onChange: function (sp) {
                            //perform change event(s)
                        }
                    });
                    fil.append(bt);
                    if (found && p.tmplSorts && p.tmplSorts.length) {
                        $(".ui-template", fil).bind('change', function (e) {
                            var curtempl = $(this).val();
                            if (curtempl == "default") {
                                alert("default: " + window.SerializeObject(defaultSorts));
                                $("#" + fid).jqSort('addSort', defaultSorts);
                            } else {
                                alert("not default: " + window.SerializeObject(p.tmplSorts) + "\r\n" + p.tmplSorts[parseInt(curtempl, 10)]);
                                $("#" + fid).jqSort('addSort', p.tmplSorts[parseInt(curtempl, 10)]);
                            }
                            return false;
                        });
                    }
                    if ($.isFunction(p.onInitializeSort)) {
                        p.onInitializeSort($("#" + fid));
                    }
                    if (p.layer)
                        $.jgrid.createModal(IDs, fil, p, "#gview_" + $t.p.id, $("#gbox_" + $t.p.id)[0], "#" + p.layer, { position: "relative" });
                    else
                        $.jgrid.createModal(IDs, fil, p, "#gview_" + $t.p.id, $("#gbox_" + $t.p.id)[0]);
                    if (bQ) {
                        $("#" + fid + "_query").bind('click', function (e) {
                            $(".queryresult", fil).toggle();
                            return false;
                        });
                    }
                    $("#" + fid + "_sort").bind('click', function () {
                        var fl = $("#" + fid),
                            sdata = {}, res,
                            sorts = fl.jqSort('sortData');

                        if (p.useLocalData)//TODO: review this logic with local data
                        {
                            var data2sort = Array.clone($($t).getGridParam("data"));
                            var result = '';

                            data2sort.sort(function (a, b) {
                                for (var i = 0; i < sorts.rules.length; i++) {
                                    result += a[sorts.rules[i].field] + ' - ' + b[sorts.rules[i].field] + '\r\n\r\n';
                                    //return comparison only if current level has a definite winner
                                    if (a[sorts.rules[i].field] != b[sorts.rules[i].field]) {
                                        if (sorts.rules[i].op == 'ASC')
                                            return a[sorts.rules[i].field] > b[sorts.rules[i].field];
                                        else
                                            return a[sorts.rules[i].field] < b[sorts.rules[i].field];
                                    }
                                }
                            });

                            //reload
                            $($t).jqGrid("clearGridData", true);
                            $($t).jqGrid("setGridParam", { "data": data2sort }).trigger('reloadGrid', [{ page: 1 }]);
                        }

                        //perform post sort functions
                        if ($.isFunction(p.onSort)) {
                            p.onSort(sorts.rules);
                        }
                        //$($t).trigger("reloadGrid", [{ page: 1}]);
                        if (p.closeAfterSort) {
                            $.jgrid.hideModal("#" + IDs.themodal, { gb: "#gbox_" + $t.p.id, jqm: p.jqModal, onClose: p.onClose });
                        }

                        //reset form
                        sorts = fl.jqSort('resetSort');

                        return false;
                    });
                    $("#" + fid + "_reset").bind('click', function () {
                        var sdata = {},
                            fl = $("#" + fid);
                        $t.p.sort = false;
                        sdata[p.sSort] = "";
                        fl[0].resetSort();
                        if (found) {
                            $(".ui-template", fil).val("default");
                        }

                        $.extend($t.p.postData, sdata);
                        if ($.isFunction(p.onReset)) {
                            p.onReset();
                        }
                        $($t).trigger("reloadGrid", [{ page: 1 }]);
                        return false;
                    });
                    showSort();
                    $(".fm-button:not(.ui-state-disabled)", fil).hover(
                        function () { $(this).addClass('ui-state-hover'); },
                        function () { $(this).removeClass('ui-state-hover'); }
                    );
                }
            });
        },

        resizeGrid: function (timeout) {
            return this.each(function () {

                if (!$(this).hasClass("force-resize")) {
                    if ($(this).is(":hidden") /*|| !$(document.body).hasClass("cs-responsive") */ || !this.p)
                        return;
                }

                var id = this.p.id;

                var initWidth = this.p._initialWidth;
                var w = "";
                var $container = $(".form-container >.scrollable-panel");
                var isResponsive = $(document.body).hasClass("cs-responsive");
                var isIE = Camstars.Browser.IE;

                if (initWidth && (initWidth < $container.width() || !isResponsive)) {
                    // Keep initial width if the grid fits into the view
                    w = initWidth + "px";
                }

                var $gbox = $("#gbox_" + id);

                var gridElements = [
                    "#gtitle_" + id,
                    "#gbox_" + id,
                    "#gview_" + id,
                    "#gview_" + id + " .ui-jqgrid-hdiv",
                    "#gview_" + id + " .ui-jqgrid-bdiv",

                    "#gview_" + id + " .ui-jqgrid-bdiv > div > .ui-jqgrid-btable",
                    "#gview_" + id + " .ui-jqgrid-hdiv >.ui-jqgrid-hbox >.ui-jqgrid-htable",
                    "#" + id + "_pager",
                    "span#" + id
                ];

                var allGridElements = gridElements.join();

                $(allGridElements, $gbox.parent()).css("width", w);
                if (w != "")
                    $(".ui-jqgrid-htable, .ui-jqgrid-btable", $gbox).css("width", "");

                allGridElements = null;
                gridElements = null;

                // Sync width
                var $gview = $("#gview_" + id);
                var $hbox = $(".ui-jqgrid-hdiv .ui-jqgrid-hbox", $gview);
                var headers = $(".ui-jqgrid-htable >thead >tr:first-child >th", $hbox);
                var templateRowTDs = $(".ui-jqgrid-bdiv >div >.ui-jqgrid-btable >tbody >tr.jqgfirstrow > td", $gview);

                // Add extra header scrollbar filler
                if (!$(document.body).hasClass("mobile-device")) {
                    if (!$(".scroll-filler", $hbox).length) {
                        // Add filler to the grid header
                        $hbox.append("<div class=scroll-filler></div>");
                    }
                }
                else {
                    $(".ui-icon-excel", $gbox).closest(".ui-pg-button").addClass("ui-export-btn");
                }

                var cols = $("#" + id).getGridParam('colModel');
                var scrolling = { v: false, h: false };
                var $btableDiv = $(".ui-jqgrid-btable", $gview).parent();
                scrolling.h = $btableDiv[0].scrollWidth > $btableDiv.parent().width();

                var verticalContainer = $btableDiv.parent().height() - (scrolling.h ? 17 : 0);
                scrolling.v = $btableDiv[0].scrollHeight > verticalContainer;
                $gbox.toggleClass("scroll-v", scrolling.v).toggleClass("scroll-h", scrolling.h);

                var looseColIndex = -1;
                if (scrolling.h == false) {
                    cols.forEach(function (c, i) {
                        if (!(c.hidden || c.name == "cb" || c.index == "_leftSelector_column")) {
                            if (c.name == "_dummy_" || c.name == "_extender_") {
                                looseColIndex = i;
                                return false;
                            }
                            looseColIndex = i;
                        }
                    });

                    if (looseColIndex > -1) {
                        // make 1 data column loose
                        var ttd = templateRowTDs.get(looseColIndex);
                        var looseCol = cols[looseColIndex];
                        var widthOriginal = (looseCol.name == "_dummy_" || looseCol.name == "_extender_") ? 0 : looseCol.widthOrg;

                        if ($(ttd).width() >= widthOriginal) {
                            ttd.style.width = "100%";
                            headers.get(looseColIndex).style.width = "100%";
                        }
                        else {
                            ttd.style.width = widthOriginal + "px";
                        }
                    }
                }

                var addPixeToFirstColumn = true;
                cols.forEach(function (c, i) {
                    var templTD = templateRowTDs.get(i);
                    var hdr = headers.get(i);

                    if (c.hidden === true) {
                        // nothing to do
                        templTD.style.removeProperty("width");
                    }
                    else if (c.index == "_leftSelector_column") {
                        // keep fixed width 
                        if (c.widthOrg)
                            templTD.style.width = c.widthOrg + "px";
                        hdr.style.width = templTD.style.width;
                        addPixeToFirstColumn = false;
                    }
                    else if (c.index == "_dummy_" || c.index == "_extender_") {
                        // skip
                    }
                    else if ($(templTD).hasClass("selector-styled-cb")) {
                        $(hdr).width(27 + (isResponsive ? 0 : 1));
                        addPixeToFirstColumn = false;
                    }
                    else {
                        if (i == looseColIndex) {
                            hdr.style.width = isIE ? "auto" : null;
                        }
                        else {
                            var colWidth = $(templTD).width();
                            if (addPixeToFirstColumn) {
                                if (!isIE)
                                    if (!isResponsive)
                                        colWidth++;
                                addPixeToFirstColumn = false;
                            }
                            if (c.widthOrg && colWidth < 5) {
                                templTD.style.width = c.widthOrg + "px";
                                colWidth = c.widthOrg;
                            }
                            $(hdr).width(colWidth);
                        }
                    }

                    if ($(">div >input.cbox", hdr).length) {
                        $(hdr).addClass("hdr-checkbox");
                    }
                });


                var $pager = $(".ui-jqgrid-pager", $gview.parent()).toggleClass("small-width", $gview.width() <= 600);
                $.jgrid.adjustPagerWidth($pager, $gview.width() <= 600);

                if (this.control)
                    $.jgrid.wrapCells(this.control);
            });
        },

    });

// Replace oridinal method with own to support inline editing

$.extend($.fn.jqGrid, {
    setRowData: function (rowid, data, cssp) {
        var nm, success = true, title;
        
        function sanitizeHtmlInput(input) {
            if (!input || typeof input !== 'string') {
                return input;
            }
            var result = input;
            result = result.replace(/<\s*script\b[^>]*>[\s\S]*?<\/\s*script\s*>/gi, '');
            result = result.replace(/<\s*style\b[^>]*>[\s\S]*?<\/\s*style\s*>/gi, '');

            result = result.replace(/<[^>]+>/gi, function (t) {
                return t.replace(/\s+on[a-z0-9_-]+\s*=\s*(?:"[^"]*"|'[^']*'|[^"'\s><`][^\s><`]*)/gi, '');
            });

            return result;
        }

        for (key in data) {
            if (data[key] && typeof data[key] === 'string') {
                data[key] = sanitizeHtmlInput(data[key]);
            }
        }

        this.each(function () {
            if (!this.grid) { return false; }
            var t = this, vl, ind, cp = typeof cssp, lcdata = {};
            ind = t.rows.namedItem(rowid);
            if (!ind) { return false; }
            if (data) {
                try {
                    $(this.p.colModel).each(function (i) {
                        nm = this.name;
                        if (data[nm] !== undefined) {
                            lcdata[nm] = this.formatter && typeof (this.formatter) === 'string' && this.formatter == 'date' ? $.unformat.date.call(t, data[nm], this) : data[nm];
                            vl = t.formatter(rowid, data[nm], i, data, 'edit');
                            title = this.title ? { "title": $.jgrid.stripHtml(vl) } : {};
                            if (t.p.treeGrid === true && nm == t.p.ExpandColumn) {
                                $("td:eq(" + i + ") > span:first", ind).html(vl).attr(title).data('val', vl);

                            } else {
                                // Restore inline editing control
                                var customEl = $("td:eq(" + i + ")", ind).find('.customelement');
                                if (customEl.length > 0) {
                                    customEl.hide();
                                    var idPicklist = customEl.attr('id').substring(0, customEl.attr('id').length - 4);
                                    var picklist = $find(idPicklist);
                                    if (picklist != null) {
                                        picklist._element.appendChild(customEl[0]);
                                    }
                                    else {
                                        document.body.appendChild(customEl[0]);
                                    }
                                }
                                $("td:eq(" + i + ")", ind).html(vl).attr(title).data('val', vl);
                            }
                        }
                    });
                    if (t.p.datatype == 'local') {
                        var id = $.jgrid.stripPref(t.p.idPrefix, rowid),
                            pos = t.p._index[id];
                        if (t.p.treeGrid) {
                            for (var key in t.p.treeReader) {
                                if (lcdata.hasOwnProperty(t.p.treeReader[key])) {
                                    delete lcdata[t.p.treeReader[key]];
                                }
                            }
                        }
                        if (typeof (pos) != 'undefined') {
                            t.p.data[pos] = $.extend(true, t.p.data[pos], lcdata);
                        }
                        lcdata = null;
                    }
                } catch (e) {
                    success = false;
                }
            }
            if (success) {
                if (cp === 'string') { $(ind).addClass(cssp); } else if (cp === 'object') { $(ind).css(cssp); }
                $(t).triggerHandler("jqGridAfterGridComplete");
            }
        });
        return success;
    }

});
