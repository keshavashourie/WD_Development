// Copyright Siemens 2025 

// JScript File
// Set the focus on the control with a tab index of 1
function SetFocusOnFirstTabIndex() {
    if (document.forms[0] != null) {
        var tables = document.forms[0].getElementsByTagName("table");
        var tableID = "";
        for (var i = 0; i < tables.length; i++) {
            if (tables[i]) {
                if (tables[i].id.indexOf("WebPartTable") >= 0 && tables[i].id.indexOf("ConciergeControl") == -1 &&
                    tables[i].id.indexOf("ButtonsBar") == -1) {
                    WebPartBase_OnActivate(document.getElementById(tables[i].id));
                    tableID = tables[i].id;
                    break;
                }

            }
        }

        var Elements = document.forms[0].elements;
        for (var j = 0; j < Elements.length; j++) {
            if (Elements[j].name != null &&
                Elements[j].name.indexOf(tableID.split("_")[1]) >= 0 &&
                Elements[j].tagName != "table" &&
                Elements[j].tagName != "div") {

                try {
                    Elements[j].focus();
                }
                catch (e) { }
                break;
            }
        } // for        
    } // if
} // SetFocusOnFirstTabIndex


//Checks if "parent" element is a parent of the "child" element
function IsElementChildOf(child, parent) {
    if (child.parentNode != null) {
        if (child.parentNode == parent) {
            return true;
        } //if
        else {
            return IsElementChildOf(child.parentNode, parent);
        } //else
    } //if
    else {
        return false;
    } //if
} //IsElementChildOf

// Given an element id check to see if it is  valid element on the page 
function IsValidElement(doc, elementId) {
    var elem = $(doc).find("#" + elementId);

    if (elem != null && elem.length > 0 && elem[0] != null) {
        return true;
    }
    else {
        return false;
    } // if else
} // IsValidElement

// Upload control
// Hides button from inputLevel1 file control so that user could see our image
function AdjustFileInputWidth(ctrlId, fileCtrlSuffix, childTblLevel3Id, relativeCellID, buttonType) {
    var divLevel1 = document.getElementById(ctrlId + "_DivLevel1");
    var inputLevel1 = document.getElementById(ctrlId + "_" + fileCtrlSuffix);
    var divLevel2 = document.getElementById(ctrlId + "_DivLevel2");
    var inputLevel3Empty = document.getElementById(ctrlId + "_" + fileCtrlSuffix + "_Level3");

    if (divLevel1 == null || inputLevel1 == null || inputLevel3Empty == null)
        return false;

    var percStart = Math.round((inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth) / inputLevel1.offsetWidth * 100 - 1);
    var percEnd = percStart + 1;
    if (buttonType == "Image") {
        var strFilter = "alpha(style=1, opacity=100, finishOpacity=0, startX=" + percStart.toString() + ", startY=0, finishX=" + percEnd.toString() + ", finishY=0)";
        inputLevel1.style.filter = strFilter;
        divLevel1.style.width = inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth + inputLevel3Empty.offsetHeight + 4;
        divLevel2.style.width = inputLevel1.offsetWidth - inputLevel3Empty.offsetWidth;
        var relativeCell = document.getElementById(relativeCellID);
        if (relativeCell != null)
            relativeCell.style.width = divLevel1.offsetWith + 5;
    }
    else {
        divLevel1.style.width = inputLevel1.offsetWidth;
    }

    divLevel1.style.height = inputLevel3Empty.offsetHeight + 1;

    if (childTblLevel3Id != "") {
        var childTblLevel3 = document.getElementById(childTblLevel3Id);
        if (childTblLevel3 == null)
            return false;
        childTblLevel3.style.width = inputLevel1.offsetWidth + 3;
    }

} // AdjustFileInputWidth

// Copies values from one text control to another only if the values are different
function CopyControlValues(controlFromId, controlToId) {
    if ($get([controlFromId]) != null)
        if ($get([controlToId]) != null)
            if ($get([controlToId]).value != $get([controlFromId]).value)
                $get([controlToId]).value = $get([controlFromId]).value;
} //CopyControlValues

//Displays a confirmation message with a given text
function Confirmation(confirmMessage) {
    if (confirm(confirmMessage)) {
        return true;
    }
    else {
        return false;
    }
} //Confirmation

function JConfirmationLong(confirmationMessage, dialogTitle, trueCallFuncion, trueButtonText, falseCallFunction, falseButtonText, alertType) {
    var defaultTrueCaption;
    var defaultFalseCaption;
    var callbackFunction;

    //change buttons` captions
    if (trueButtonText != null) {
        defaultTrueCaption = $.alerts.okButton;
        $.alerts.okButton = '&nbsp;' + trueButtonText + '&nbsp;';
    }//if trueButtonText
    if (falseButtonText != null) {
        defaultFalseCaption = $.alerts.cancelButton;
        $.alerts.cancelButton = '&nbsp;' + falseButtonText + '&nbsp;';
    }// if falseButtonText

    //create callbackFunction
    if (falseCallFunction != null) {
        callbackFunction = function (r) {
            if (r == true) {
                $.isFunction(trueCallFuncion) ? trueCallFuncion() : eval(trueCallFuncion);
            }
            else {
                $.isFunction(falseCallFunction) ? falseCallFunction() : eval(falseCallFunction);
            }
        }
    }//if
    else {
        callbackFunction = function (r) {
            if (r == true) {
                $.isFunction(trueCallFuncion) ? trueCallFuncion() : eval(trueCallFuncion);
            }
        }
    }//else

    if (dialogTitle === null)
        dialogTitle = alertType;

    jConfirm(confirmationMessage, dialogTitle, callbackFunction, alertType);

    //restore buttons` captions
    if (trueButtonText != null) {
        defaultTrueCaption = $.alerts.okButton;
        $.alerts.okButton = '&nbsp;' + trueButtonText + '&nbsp;';
    }//if trueButtonText

    if (falseButtonText != null) {
        defaultFalseCaption = $.alerts.cancelButton;
        $.alerts.cancelButton = '&nbsp;' + falseButtonText + '&nbsp;';
    }// if falseButtonText
}//JConfirmationLong

function JConfirmationShort(confirmationMessage, trueCallFuncion) {
    var callback = function (r) { if (r == true) eval(trueCallFuncion); };
    jConfirm(confirmationMessage, null, callback);
}//JConfirmationShort

// Set the value for the password control
function SetPasswordValue(controlName, controlValue, clientID) {
    var control = document.getElementById(clientID);
    $("input:first", control).val(controlValue);
} //SetPasswordValue


function SwitchImages(event, trueImageSrc, falseImageSrc, checkControlId, autoPostBack) {
    if (IsValidElement(document, checkControlId)) {
        var checkControl = $get([checkControlId]);

        if (autoPostBack != 'true') {
            if (checkControl.checked) {
                event.srcElement.src = falseImageSrc;
            } //if
            else {
                event.srcElement.src = trueImageSrc;
            } //else
        } //if
        checkControl.click();
    } //if
} //SwitchImages

function ViewDocumentHandler(grid, sender, action) {
    var url = sender.parentNode.previousSibling.innerHTML;

    return OpenDocument(url, 'Cannot find document', 'Empty document URI');
}

// Views document's content.
function OpenDocument(docURL, cantFindLabel, emptyURLLabel) {
    try {
        if (docURL != null && docURL != '') {
            var newwindow = window.open(docURL);
            if (newwindow != null)
                newwindow.focus();
        }
        else {
            if (emptyURLLabel != '')
                alert(emptyURLLabel);
        }
    }
    catch (exception) {
        if (cantFindLabel != '')
            alert(exception.description + " " + cantFindLabel + " " + docURL + "");
    }
    return false;
}

function radioClick(clickedElement, radioButtonId) {
    if (!$(clickedElement).is('[clicked]')) {
        $(clickedElement).attr('clicked', '');
        $('#' + radioButtonId).click();
        $(clickedElement).removeAttr('clicked');
    }
}


function OpenDocumentRef(docName, docRev) {
    var iframe = $("#DownloadIframe");
    if (iframe.length < 1)
        iframe = $("<iframe id=\"DownloadIframe\"/>");
    iframe.attr("src", "DownloadFile.aspx?docMaintName=" + encodeURIComponent(docName) + "&docMaintRev=" + encodeURIComponent(docRev));
    iframe.hide();
    iframe.appendTo(document.body);
}

function OpenDocumentUrl(docURL, auth, cantFindLabel, emptyURLLabel) {
    if (!auth || auth.toLowerCase() === "none")
        OpenDocument(docURL, "", cantFindLabel, emptyURLLabel);
    else // authentication was specified.
    {
        var iframe = $("#DownloadIframe");
        if (iframe.length < 1)
            iframe = $("<iframe id=\"DownloadIframe\"/>");
        iframe.attr("src", "DownloadFile.aspx?docMaintURL=" + encodeURIComponent(docURL) + "&docMaintAuth=" + encodeURIComponent(auth));
        iframe.hide();
        iframe.appendTo(document.body);
    }
}

function DurationTimer() {
    this.MAXTIME = 3153600000;
}

DurationTimer.prototype = {

    initialize: function (durationControlId) {
        this.durationControlId = durationControlId;
        this.timersGridId = 'ctl00_WebPartManager_TimersView_WP_TimersGrid';
        this.timers = null;
        this.interval = null;
        this._timeout = null;
        this.isGridInit = false;
    },

    dispose: function () {
        this.timers = null;
        this.interval = null;
        clearTimeout(this._timeout);
        this._timeout = null;

        var $dc = $('.container-timer > input');
        if ($dc.length) {
            if ($('#ctl00_WebPartManager_ContainerStatus_WP_TimerViewer').css('display') == 'none')
                $dc.hide();
        }
    },

    update: function () {
        var me = this;
        if (this._timeout)
            clearTimeout(this._timeout);
        this._timeout = setTimeout(function () { me.update(); }, me.interval * 1000);

        var timerValue = '';
        var timerColor = '';
        if (this.timers && this.timers.length) {
            this.updateTimers();
            var t = this.timers[0];
            var timerToDisplay = t.max.active ? t.max : t.min;
            if (timerToDisplay) {
                timerValue = this.formatDurationRel(timerToDisplay.endTime);
                timerColor = timerToDisplay.actualColor;
            }
        }
        else {
            timerValue = '';
            timerColor = null;
        }

        var dc = $find(this.durationControlId);
        if (dc) {
            dc.setValue(timerValue);
            $('input', dc.get_element()).css('background-color', timerColor);
        }

        if (!timerValue) {
            this.dispose();
            return;
        }

        var timersGrid = $find(this.timersGridId);
        if (!timersGrid) {
            this.timersGridId = $('[id$="TimersGrid"]').prop('id');
            if (this.timersGridId)
                timersGrid = $find(this.timersGridId);
        }
        if (timersGrid) {
            this.updateTimersGrid(timersGrid, this.timers);
        }
    },

    loadTimersFromContainer: function (tmrs) {

        this.timers = [];
        var tmrsLength = tmrs.length;
        for (var i = 0; i < tmrsLength; i++) {
            this.timers.push(this.getTimerFromContainer(tmrs[i]));
        }

        this.sortTimers(this.timers);
        return this.timers;
    },

    getTimerFromColumns: function (g, $tr) {
        var dtFormat = this.cellDateTimeFormat();
        var parseTime = function (s) {
            var v;
            if (s) {
                v = Date.parseLocale(s, dtFormat);
                if (v == null)
                    console.error("Error dateTime parsing", s);
            }
            return v;
        };

        return {
            name: this.getItem('ProcessTimerName', g, $tr) + getRevisionDelimiter() + this.getItem('ProcessTimerRevision', g, $tr),
            min: {
                active: this.getItem('MinEndTimeGMT', g, $tr) ? true : false,
                endTime: parseTime(this.getItem('MinEndTimeGMT', g, $tr)),
                endColor: this.getItem('MinTimeColor', g, $tr),
                warning: this.getItem('MinEndWarningTimeGMT', g, $tr) ? true : false,
                warningTime: parseTime(this.getItem('MinEndWarningTimeGMT', g, $tr)),
                warningColor: this.getItem('MinWarningTimeColor', g, $tr),
                actualColor: ''
            },

            max: {
                active: this.getItem('MaxEndTimeGMT', g, $tr) ? true : false,
                endTime: parseTime(this.getItem('MaxEndTimeGMT', g, $tr)),
                endColor: this.getItem('MaxTimeColor', g, $tr),
                warning: this.getItem('MaxEndWarningTimeGMT', g, $tr) ? true : false,
                warningTime: parseTime(this.getItem('MaxEndWarningTimeGMT', g, $tr)),
                warningColor: this.getItem('MaxWarningTimeColor', g, $tr),
                actualColor: ''
            }
        };
    },

    getItem: function (colName, grid, $tr) {
        return $('td[aria-describedby="' + grid._gridID + '_' + colName + '"]', $tr).text().trimEnd();
    },

    getItemTime: function (colName, grid, $tr) {
        var dateTimeFormat = this.cellDateTimeFormat();
        // get time from column
        var s = this.getItem(colName, grid, $tr);
        if (s) {
            var d = Date.parseLocale(s, dateTimeFormat);
            return Math.floor(((d - (new Date(d).getTimezoneOffset() /* offset in minutes */ * 1000 * 60)) - new Date()) / 1000);
        }
        return null;
    },

    getTimerFromContainer: function (tc) {
        var cc = Sys.CultureInfo.CurrentCulture.dateTimeFormat;
        var self = this;
        var parseTime = function (s) {
            if (s && s.Value) {
                return (typeof s.Value == "string") ? Date.parseLocale(s.Value.substring(0, 19), cc.SortableDateTimePattern) : self.convertLocalDateToUTCDate(s.Value) /*Date*/;
            }
            else
                return NaN;
        };
        return {
            name: tc.ProcessTimerName.Value + getRevisionDelimiter() + tc.ProcessTimerRevision.Value,
            min: {
                active: tc.MinEndTimeGMT ? true : false,
                endTime: parseTime(tc.MinEndTimeGMT),
                endColor: tc.MinTimeColor ? tc.MinTimeColor.Value : null,
                warning: tc.MinEndWarningTimeGMT ? true : false,
                warningTime: parseTime(tc.MinEndWarningTimeGMT),
                warningColor: tc.MinWarningTimeColor ? tc.MinWarningTimeColor.Value : null,
                actualColor: ''
            },

            max: {
                active: tc.MaxEndTimeGMT ? true : false,
                endTime: parseTime(tc.MaxEndTimeGMT),
                endColor: tc.MaxTimeColor ? tc.MaxTimeColor.Value : '',
                warning: tc.MaxEndWarningTimeGMT ? true : false,
                warningTime: parseTime(tc.MaxEndWarningTimeGMT),
                warningColor: tc.MaxWarningTimeColor ? tc.MaxWarningTimeColor.Value : null,
                actualColor: ''
            }
        };
    },

    getSoonerTimer: function (timers) {
        var timerToDisplay = null;
        var rest = this.MAXTIME; // 100 years

        // Assuming the timers are sorted correctly
        if (timers && timers.length) {
            var tx = timers[0];
            Array.forEach([tx.min, tx.max], function (tm) {
                if (tm.active) {
                    if (tm.endTime < rest) {
                        rest = tm.endTime;
                        timerToDisplay = tm;
                    }
                }
            });
        }
        return timerToDisplay;
    },

    setTimerColors: function (t) {
        var now = new Date(this.getNowGMT());
        // Get actual color
        Array.forEach([t.min, t.max], function (tm) {
            if (tm.active) {
                if (tm.warning) {
                    if (now >= tm.warningTime)
                        tm.actualColor = tm.warningColor;   // warning time overdue
                }
                if (now >= tm.endTime)
                    tm.actualColor = tm.endColor;           // end time overdue
            }
        });
    },

    formatDurationRel: function (t /* milliseconds */) {
        if (!isNaN(t)) {
            var n = new Date(this.getNowGMT());
            var tillTimeSeconds = (t - n) / 1000;
            return this.formatDuration(tillTimeSeconds);
        }
        else
            return '';
    },

    formatDuration: function (seconds) {
        if (seconds == null)
            return '';

        var plus = ' ';
        if (seconds < 0) {
            plus = '+';
            seconds = -seconds;
        }
        var days = Math.floor(seconds / 86400);
        var hours = Math.floor(seconds / 3600) % 24;

        return plus + this.zeroPad(days, 2) + '.' +
            this.zeroPad(hours, 2) + ':' +
            this.zeroPad(Math.floor((seconds % 3600) / 60), 2) + ':' +
            this.zeroPad(Math.floor(seconds % 60), 2);
    },

    zeroPad: function (num, places) {
        var zero = places - num.toString().length + 1;
        return Array(+(zero > 0 && zero)).join("0") + num;
    },

    getNowGMT: function () {
        var n = Date.now();
        return n + new Date().getTimezoneOffset() * 60000;
    },

    updateTimers: function (tmrs) {
        if (!tmrs)
            tmrs = this.timers;

        var now = new Date(this.getNowGMT());
        Array.forEach(tmrs, function (tx) {
            Array.forEach([tx.min, tx.max], function (tm) {
                if (tm.active) {
                    if (tm.warning) {
                        if (now >= tm.warningTime)
                            tm.actualColor = tm.warningColor;
                    }
                    if (now >= tm.endTime)
                        tm.actualColor = tm.endColor;
                }
            });
        });
    },

    sortTimers: function (tmrs) {
        var endless = new Date(9999, 12);
        var getEndTime = function (tm) {
            return tm.active ? tm.endTime : endless;
        };
        // The timers are sorted by Time to Max first (with the longest timer over max at the top), 
        tmrs.sort(function (tm1, tm2) {

            if (getEndTime(tm1.max) < getEndTime(tm2.max))
                return -1;
            else if (getEndTime(tm1.max) == getEndTime(tm2.max)) {
                if (getEndTime(tm1.min) < getEndTime(tm2.min))
                    return -1;
                else if (getEndTime(tm1.min) > getEndTime(tm2.min))
                    return 1;
                else
                    return 0;
            }
            else
                return 1;
        });
    },

    updateTimersGrid: function (grid, timers) {
        if (!grid)
            grid = $find(this.timersGridId);

        if (!timers)
            timers = this.timers;

        timers = timers || [];

        if (!grid)
            return;

        var theGrid = jQuery(grid.GridID);
        if (!theGrid.length)
            return;

        if (!this.isGridInit)
            var refreshBtn = $('#refresh_' + grid.GridID.substring(1) + " .ui-icon-refresh")
        if (refreshBtn) {
            var me = this;

            refreshBtn.off('click').on('click', function () {
                me.refreshTimers();
            });
            this.isGridInit = true;
        }

        for (var i = 0; i < timers.length; i++) {
            var t = timers[i];
            var rowid = this.zeroPad(i, 6);
            theGrid.setCell(rowid, 'MinTime', this.formatDurationRel(t.min.endTime), t.min.actualColor ? { 'backgroundColor': t.min.actualColor } : null);
            theGrid.setCell(rowid, 'MaxTime', this.formatDurationRel(t.max.endTime), t.max.actualColor ? { 'backgroundColor': t.max.actualColor } : null);
            theGrid.setCell(rowid, 'Name', t.name);
        }

        // Add total items into the footer
        var $ftr = $('#ctl00_WebPartManager_ContainerStatus_WP_TimerViewer .ui-flyout .ui-flyout-container .ui-flyout-footer');
        if ($ftr.length) {
            if (!$('.total-items', $ftr).length) {
                // Add total items 
                var inp = $('input', $ftr).detach();
                $ftr.prepend('<div class="total-items"><span class="value"></span></div><div class="close-button"></div>');
                inp.appendTo($('.close-button', $ftr));
                var $lblItems = $('#ctl00_WebPartManager_TimersView_WP_ItemsLabel');
                $('span.value', $ftr).text($lblItems.text());
            }

            var $totalSpan = $('span.value', $ftr);
            var txt = $totalSpan.text();
            // Replace first word with total number
            txt = timers.length.toString() + txt.substring(txt.indexOf(' '));
            $totalSpan.text(txt);
        }
    },

    refreshTimers: function () {
        var grid = $find(this.timersGridId);
        grid.DataLoaded = function () {
            var theGrid = jQuery(grid.GridID);
            var gridRecords = theGrid.getRowData();
            if (timersWatcher.timers && gridRecords.length != timersWatcher.timers.length) {
                // Remove stopped timers
                var timersNew = [];
                for (var i = 0; i < gridRecords.length; i++) {
                    var n = gridRecords[i].Name;
                    var t = null;
                    timersWatcher.timers.forEach(function (tx) {
                        if (tx.name === n) { t = tx; return false; }
                    });
                    if (t)
                        timersNew.push(t);
                }
                timersWatcher.sortTimers(timersNew);
                timersWatcher.timers = timersNew;
            }

            if (timersWatcher) {
                setTimeout(function () { timersWatcher.updateTimersGrid(); }, 100);
            }
        }
        grid.Reload(true);
        return;
    },

    forceTimersUpdate: function (durationClass) {
        var $durationCtl = $('.' + durationClass);
        if ($durationCtl.length) {
            // Fix ui-flyout-header - add refresh icon
            var $ui = $('#ctl00_WebPartManager_ContainerStatus_WP_TimerViewer .ui-flyout');
            if ($ui.length && $ui.css('display') != 'none') {
                var $hdr = $('.ui-flyout-container .ui-flyout-header', $ui);
                if (!$('.refresh-button', $hdr).length) {
                    var $sp = $('span', $hdr);
                    $sp.after('<div class="delimiter"></div><span class="refresh-button" onclick="timersWatcher.refreshTimers()"> </span>');
                }
            }

            // This is for correction of left offset in case of EProcedure
            var $prodEvBtn = $('#ctl00_WebPartManager_ContainerStatus_WP_CreateProdEventButton:visible', $ui.closest('table'));
            if ($prodEvBtn.length)
                $ui.addClass('leftshift');
            else
                $ui.removeClass('leftshift');

            if (timersWatcher) {
                setTimeout(function () { timersWatcher.updateTimersGrid(); }, 10);
            }
        }
    },

    convertLocalDateToUTCDate: function (dt) {
        var localTime = dt.getTime();
        var localOffset = dt.getTimezoneOffset() * 60000;
        return new Date(localTime + localOffset);
    },

    cellDateTimeFormat: function () {
        var cult = getCEP_top().__page.get_pageCulture();
        return cult.ShortDatePattern + ' ' + cult.LongTimePattern;
    }
}

timersWatcher = new DurationTimer();

function CommandBarBase_Deco() {
    this._customCommandBarItems = [];
    this.containerNameFunction = null;
    this.customContainerStatusServerType = null;
    return this;
}

CommandBarBase_Deco.prototype = {
    initialize: function (commandBarObj) {
    },

    get_instance: function () {
        return this;
    },

    /* public virtual */
    GetCommandBarItems: function () {
        return this.GetCustomCommandBarItems();
    },

    AddCustomCommandBarItem: function (item) {
        this._customCommandBarItems.push(item);
    },

    GetCustomCommandBarItems: function () {
        if (!Array.isArray(this._customCommandBarItems))
            this._customCommandBarItems = [];
        return this._customCommandBarItems;
    },

    /**
     * Merges default items defined in classes that extend this base class with custom items.
     * 
     * @param {object[]} defaultItems - Command items to be merged with the custom items
     */
    GetMergedCommandBarItems: function (defaultItems) {
        if (!Array.isArray(this._customCommandBarItems) || this._customCommandBarItems.length === 0)
            return defaultItems;

        var me = this;
        var mergedItems = [];
        var overrides = [];

        defaultItems.forEach(function (defaultItem) {
            // if override exists in custom items, use that. otherwise use default item
            if (!me._customCommandBarItems.some(function (customItem, index) {
                var isOverride = defaultItem.Id === customItem.Id;
                if (isOverride) {
                    mergedItems.push(customItem);
                    overrides.push(index);    // mark override custom items so we only add new ones later.
                }
                return isOverride;
            })) {
                mergedItems.push(defaultItem);
            }
        });

        // add any custom items that did not override default items
        this._customCommandBarItems.forEach(function (customItem, index) {
            if (overrides.indexOf(index) === -1)
                mergedItems.push(customItem);
        });

        return mergedItems;
    },

    GetCommandBarGlobals: function () {
        return this._commandBarGlobals;
    },

    _commandBarGlobals: {
        transactionThreshold: 7,
        showMobileMenu: false,
        //onUpdated: function (commandBarElement) { return; },
        //onCommandsLoaded: function (cmdItemElements) { return true; },
        onTransactionsLoaded: function (transactionArray) {
            return this.transactionsLoad(transactionArray);
        },
        getLabels: function (cmdObj) {
            return cmdObj.labels;
        },
        //onCmdClick: function (clickedElement, menuItem) { return true; },
        //onRedirectClick: function (clickedElement, menuItem) { return true; },
    },

    labels: {
        submitLbl: { Name: "SubmitButton", Value: null }
    },

    /* public virtual */
    CollectLabels: function (labels) {
        var addedCount = 0;
        this.GetCommandBarItems().forEach(function (m) {
            if (!m.Name.Text && labels.filter(function (l) { return l.Name == m.Name.Label; }).length == 0) {
                labels.push({ Name: m.Name.Label });
                addedCount++;
            }
        });

        // Add extra labels
        var gbl = this.GetCommandBarGlobals();
        if (typeof gbl.getLabels == "function") {
            var lbls = gbl.getLabels(this);
            if (lbls) {
                Object.keys(lbls).forEach(function (k) {
                    if (!lbls[k].Value) {
                        labels.push(lbls[k]);
                        addedCount++;
                    }
                });
            }
        }
        return addedCount;
    },

    /* public virtual */
    PopulateLabels: function (labels) {
        var getDefaultLabel = labels.Error || false;
        this.GetCommandBarItems().forEach(function (m) {
            if (!getDefaultLabel) {
                var lbl = labels.filter(function (l) { return l.Name == m.Name.Label; });
                if (lbl.length)
                    m.Name.Text = lbl[0].Value;
            }
        });

        // Save extra labels
        var gbl = this.GetCommandBarGlobals();
        if (typeof gbl.getLabels == "function") {
            var lbls = gbl.getLabels(this);
            if (lbls) {
                Object.keys(lbls).forEach(function (k) {
                    var lblName = lbls[k].Name;
                    if (!getDefaultLabel) {
                        var lbl = labels.filter(function (l) { return l.Name == lblName; });
                        if (lbl.length)
                            lbls[k].Value = lbl[0].Value;
                    }
                    else {
                        if (lbls[k].DefaultValue)
                            lbls[k].Value = lbls[k].DefaultValue;
                    }
                });
            }
        }
    },

    /**
     * Checks if the Submit or Reset action command items have an override by a custom command item.
     * (logic here based on checks done in transactionsLoad)
     * 
     * @param {object} $action - jQuery element of action item
     * @param {number} visibleActionCount - Count of total number of visible action items.
     */
    hasActionOverride: function ($action, visibleActionCount) {
        if (this._customCommandBarItems.length === 0)
            return false;

        var hasOverride = false;
        var me = this;

        if ($action.attr('displaymode') === 'Bar' && $action.css("display") !== "none") {
            if ($action.attr('isprimary') === 'true' || $action.hasClass('cmd-submit')) {
                hasOverride = haveCustomCommandItemWithId('Submit');
            } else if ($action.hasClass('cmd-reset') || $action.attr('custommethod') === 'ShopfloorReset' || (visibleActionCount === 2 && $action.attr('isprimary') != 'true')) {
                hasOverride = haveCustomCommandItemWithId('Reset');
            }
        }

        return hasOverride;

        function haveCustomCommandItemWithId(id) {
            return me._customCommandBarItems.some(function (item) {
                return item.Id === id;
            });
        }

    },

    transactionsLoad: function (trButtons) {
        var submitText = this.labels.submitLbl.Value;
        var btnFiltered = trButtons.filter(".action-redirect-button:not(.origin-invisible)");

        btnFiltered.each(function () {
            var $btn = $(this);
            if (!$btn.hasClass("cmdbar-custom-button")) {
                var menu = $btn.data("menuitem");
                if (menu) {
                    if ($btn.hasClass("cmd-submit") || $btn.hasClass("cmd-primary")) {
                        $(".caption-text", $btn).text(submitText);
                        $btn.addClass("cmd-submit");
                    }
                    else {
                        if ($btn.hasClass("cmd-reset") || menu.CustomMethod === "ShopfloorReset" || (btnFiltered.length == 2 && !menu.IsPrimary)) {
                            $btn.addClass("cmd-reset");
                            if ($btn.attr("order") >= 100)
                                $btn.attr("order", 30);
                        }
                    }
                }
            }
        });
    }
}

function ContainerStatusWebPart_Deco() {
    CommandBarBase_Deco.call(this);
}

ContainerStatusWebPart_Deco.prototype = $.extend(Object.create(CommandBarBase_Deco.prototype), {

    initialize: function (commandBarObj) {
        CommandBarBase_Deco.prototype.initialize.apply(this, commandBarObj);
    },

    /* public virtual */
    GetCommandBarItems: function () {
        var me = this;
        this._commandBarItems.forEach(function (c) {
            if (c.Action && c.Action.Visible === undefined) {
                c.Action.Visible = me._isContainerDefined;
            }
        });
        return this.GetMergedCommandBarItems(this._commandBarItems);
    },

    GetCommandBarGlobals: function () {
        var me = this;
        return $.extend(CommandBarBase_Deco.prototype.GetCommandBarGlobals.call(this),
            {
                transactionThreshold: function () { return 2; },
                onTransactionsLoaded: function (transactionArray) {
                    return this.transactionsLoaded(transactionArray);
                },
                getLabels: function (cmdObj) {
                    var baseLabels = CommandBarBase_Deco.prototype.get_instance().labels || {};
                    $.extend(cmdObj._labels, baseLabels);
                    return cmdObj._labels;
                },

                showMobileMenu: true
            });
    },

    _commandBarItems: [
        {
            Id: "ContainerStatus",
            Action: {
                PanelBuilder: function () { return this._getContainerInfo('all'); },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "container-status" },
            Name: { Label: "Constants_ContainerStatus" }
        },
        {
            Id: "ContainerDocuments",
            Action: {
                PanelBuilder: function () { return this._getContainerInfo('documents'); },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "container-documents" },
            Name: { Label: "DocumentSet_DocumentEntries" },
        },
        {
            Id: "ContainerAttachDocument",
            Action: {
                PanelBuilder: function () { return this._attachDocument(); },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "container-attachments" },
            Name: { Label: "Lbl_AttachDocument" },
            CSS: { Bar: [], Panel: [] }
        },
        {
            Id: "ContainerMfgAuditTrail",
            Action: {
                Page: "MfgAuditTrailVP_R2",
                PanelBuilder: function () { return this._mfgAudit(); },
                GetDataContracts: function () { return this._getContainerNameObj(); },
                OpenMode: function () {
                    if ($(document.body).hasClass("mobile-device"))
                        return $(document.body).is(".mobile-device:not(.wide)") ? "slideout" : "popup";
                    return "newtab";
                },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "container-audit-trail" },
            Name: { Label: "LblMenuMfgAuditTrail" }
        },
        {
            Id: "ContainerActiveTimers",
            Action: { PanelBuilder: function () { return this._viewTimers(); }, Visible: false },
            Name: { Label: "ContainerStatus_ActiveTimers" },
            CSS: { Bar: ["active-timers"], Panel: ["extended-width"] },
        },
        {
            Id: "ContainerWorkflow",
            Action: {
                PanelBuilder: function () { return this._getContainerInfo('workflow'); },
                OpenMode: function () { return $(document.body).is(".mobile-device:not(.wide)") ? "slideout" : "popup"; },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "container-workflow" },
            Name: { Label: "Container_Workflow" },
            CSS: { Bar: [], Panel: ["workflow"] }
        }
    ],

    _labels: {
        attributeColumnLbl: { Name: "AttributeTitle", Value: null },
        valueColumnLbl: { Name: "SelVal_Value", Value: null }
    },

    /* public virtual */
    setCallbackData: function (objData) {
        switch (objData.__fun) {
            case "getContainerInfo":
            case "getCarrierContainersInfo":
                return this._renderContainerStatusInfo(objData);
            case "openDocument":
                return this._openDocument(objData);
            case "attachDocument":
                return this._attachDocument(objData);
            case "viewTimers":
                return this._viewTimers(objData);
            default:
                break;
        }
    },

    getContainer: function () {
        return this._getContainer();
    },

    _isContainerDefined: function () {
        return !!this._getContainer();
    },

    transactionsLoaded: function (trButtons) {
        // Call base method
        CommandBarBase_Deco.prototype.transactionsLoad.apply(CommandBarBase_Deco.prototype.get_instance(), [trButtons]);

        var containerValue = this.getContainer();
        var isHidden = !containerValue;

        if (!containerValue && $(document.body).hasClass("common-search-r2") && trButtons.length > 0)
            isHidden = false;

        trButtons.each(function () {
            var $btn = $(this);
            if (isHidden && !$btn.hasClass('cmdbar-custom-button'))
                $btn.toggleClass("hidden", !$btn.hasClass("transaction-button"));
        });
        return containerValue;
    },

    _getContainerInfo: function (filter) {
        // ContainerList control
        var containerName = this._getContainer();
        var isCheckSheet = $('form')[0].action.indexOf("scsCheckSheetVPR2") > -1;
        var isSS_ContainerSearch = $('form')[0].action.indexOf("scsContainerSearchVP_R2") > -1;
        if (!containerName) {
            return this._containerIsNotSelected();
        }
        let tmpPage = getCEP_top().__page || window.__page || parent.__page;
        let serverType = tmpPage ? tmpPage.get_containerStatusInquiry() : "Camstar.WebPortal.Helpers.ContainerStatusInquiry";

        // default args
        let serviceArgs =
        {
            serverType: this.customContainerStatusServerType ? this.customContainerStatusServerType : serverType,
            clientType: "ContainerStatusWebPart_Deco",
            fun: "getContainerInfo",
            containerName: containerName,
            filter: filter
        };

        if (!this.customContainerStatusServerType && serverType === "Camstar.WebPortal.Helpers.ContainerStatusInquiry") {
            // adjust service args
            if ($get("ctl00_WebPartManager_CarrierWP_ControlCarrier_Edit") && filter == "documents")  // Carrier Documents
            {
                serviceArgs.fun = "getCarrierContainersInfo";
                serviceArgs.serverType = "Camstar.WebPortal.Helpers.isContainerStatusInquiry";
            }

            if (typeof (isIndustryDefect) !== 'undefined') {
                serviceArgs.serverType = "Camstar.WebPortal.Helpers.isContainerStatusInquiry";
                serviceArgs.tag = isIndustryDefect.getSelectedDefectId();
            }

            if (isCheckSheet) {
                serviceArgs.serverType = "Camstar.WebPortal.Helpers.scsContainerStatusInquiry";
            }

            if (isSS_ContainerSearch) {
                serviceArgs.serverType = "Camstar.WebPortal.Helpers.scsContainerStatusInquiry";
            }
        }

        CallServer(JSON.stringify(serviceArgs), null);

        return this._processing();
    },

    _containerIsNotSelected: function () {
        return [$("<div class=error> Container is not selected </div>")];
    },

    _processing: function () {
        return [$("<div>processing...</div>")];
    },

    _getContainer: function () {
        if (this.containerNameFunction) {
            // CR.Sidebar can attach this object a function that can get a container name.
            // When rendering sidebar buttons that require a container name on pages that do not
            // use a standard web part, this function can get the container name it needs. So if a function 
            // reference is available use that instead of searching for a standard web part control.
            return this.containerNameFunction();
        } else {
            var containerControl = $get("ctl00_WebPartManager_ContainerStatusWP_ContainerStatus_ContainerName_Edit") ||
                $get("ctl00_WebPartManager_ContainerStatus_WP_ContainerStatus_ContainerName_Edit") ||
                $get("ctl00_WebPartManager_ContainerStatusWP_R2_ContainerStatus_ContainerName_Edit") ||
                $get("ctl00_WebPartManager_QuickLinksWP_ContainerStatus_ContainerName_Edit") ||
                $get("ctl00_WebPartManager_CarrierWP_ControlCarrier_Edit");

            if (containerControl) {
                return containerControl.value;
            }
        }

        return null;
    },

    _renderContainerStatusInfo: function (data) {
        var div_list = [];
        //var defectExists = document.getElementById("defectButtonRepairIcon");
        if (data.Error) {
            div_list.push($("<div class=error>" + data.Error + "</div>"));
        }

        else if (data["DocumentSets"] || data["CarrierDocumentSets"]) {
            var isCarrier = data["CarrierDocumentSets"];
            var dss = isCarrier ? data["CarrierDocumentSets"] : data["DocumentSets"];
            var getIcon = this._getDocIconPath;

            var loc = window.location;
            //var appPath = loc.pathname.substr(0, loc.pathname.indexOf("/", 1));
            //var sPage = loc.pathname.substring(loc.pathname.lastIndexOf('/') + 1);

            Array.forEach(dss, function (d) {
                var isCAD = d.Name === "NPI Job CAD";
                // Document Set name
                var $hdr = $("<div class='header-content-row cs-docset'></div>");
                $hdr.text(d.Name);
                div_list.push($hdr);

                // Doc entries
                Array.forEach(d.DocumentEntries, function (de) {
                    var $entry = $(
                        "<div class='content-row doc-entry'>" +
                        "<img class='icon'></img>" +
                        "<div class=info><span class='name'></span>" +
                        "<span class='cr-open-new-tab'>&nbsp;</span>" +
                        "<span class='cr-open-popup' >&nbsp;</span>" +
                        "<div class=desc></div></div > " +
                        "</div>");

                    var docIdentifier = de.DocumentIdentifier && de.BrowseMode && (de.BrowseMode.toLowerCase() === "url" || de.BrowseMode.toLowerCase() === "httpfile") ? de.DocumentIdentifier : (de.Document.Name + de.FileType);
                    let docName = de.Document.Name ? encodeURIComponent(de.Document.Name) : "";
                    let docRevision = de.Document.Revision ? encodeURIComponent(de.Document.Revision) : "";
                    var docViewerUrl = de.BrowseMode && (de.BrowseMode.toLowerCase() === "url" || de.BrowseMode.toLowerCase() === "httpfile") ? docIdentifier : CR.URL.getDocViewerUrl(docIdentifier, docName, docRevision);
                    var protocolMatch = true;
                    try {
                        var testURL = new URL(docViewerUrl);
                        protocolMatch = testURL.protocol === loc.protocol;
                    } catch (err) {
                    }

                    $entry.attr("browse-mode", de.BrowseMode);
                    $entry.prop("title", de.Description);
                    $('.icon', $entry).prop("src", getIcon(de.FileType));
                    $('.name', $entry).text(de.Name);
                    $('.desc', $entry).text(de.Description);
                    $('.name', $entry).prop("style", "max-width:160px;cursor:pointer;overflow: hidden;text-overflow:ellipsis;");
                    if (isCAD) {
                        $('.cr-open-popup', $entry).click(function () {
                            console.log('open in CAD Viewer: ' + docIdentifier);
                        });
                        $('.cr-open-popup', $entry).prop("title", CR.Page.getLabelValue("Web_OpenPopup"));
                        $('.cr-open-new-tab', $entry).hide();
                    } else if (CR.Path.isJTFile(docIdentifier)) {
                        $('.cr-open-popup', $entry).click(function () {
                            console.log('open in 3D Viewer: ' + docIdentifier);
                            CR.Page.openPopup(CR.URL.getJTViewerUrl(de.Document.Name, de.Document.Revision), '3D Model Popup', false);
                        });
                        $('.cr-open-popup', $entry).prop("title", CR.Page.getLabelValue("Web_OpenPopup"));
                        $('.cr-open-new-tab', $entry).hide();
                    } else {
                        $('.cr-open-new-tab', $entry).click(function () {
                            console.log('open in new tab: ' + docIdentifier);
                            window.open(docViewerUrl, 'Document Viewer - ' + docIdentifier);
                            let event1 = arguments[0] || window.event;
                            CR.Event.stopEvent(event1);
                        });
						if (protocolMatch) {
                        $('.cr-open-popup', $entry).click(function () {
                            console.log('open in popup: ' + docIdentifier);
                            CR.Page.openPopup(docViewerUrl, 'Document Popup', false);
                            let event1 = arguments[0] || window.event;
                            CR.Event.stopEvent(event1);
                        });
                        $('.cr-open-popup', $entry).prop("title", CR.Page.getLabelValue("Web_OpenPopup"));
						} else
                            $('.cr-open-popup', $entry).hide();
                        $('.cr-open-new-tab', $entry).prop("title", CR.Page.getLabelValue("Web_OpenNewTab"));
                    }
                    $entry.click(function () {
                        var isCheckSheet = $('form')[0].action.indexOf("scsCheckSheetVPR2") > -1;
                        //  Open document in new tab
                        if (CR.Path.isJTFile(docIdentifier)) {
                            console.log('open JT file: ' + docIdentifier);
                            CR.Page.openPopup(CR.URL.getJTViewerUrl(de.Document.Name, de.Document.Revision), '3D Model Popup', false);
                        }
                        else if (isCarrier) {
                            console.log('open in new tab: ' + docIdentifier);
                            window.open(docViewerUrl, 'Document Viewer - ' + docIdentifier);
                            let event1 = arguments[0] || window.event;
                            CR.Event.stopEvent(event1);
                        } else if (!isCAD) {
							if (protocolMatch) {
                            let tmpPage = getCEP_top().__page || window.__page || parent.__page;
                            let serverType = tmpPage ? tmpPage.get_containerStatusInquiry() : "Camstar.WebPortal.Helpers.ContainerStatusInquiry";
                            if (isCheckSheet) {
                                serverType = "Camstar.WebPortal.Helpers.scsContainerStatusInquiry";
                            }
                            CallServer(JSON.stringify({
                                fun: 'openDocument',
                                serverType: serverType,
                                clientType: "ContainerStatusWebPart_Deco",
                                documentName: de.Document.Name,
                                documentRev: de.Document.Revision
                            }), null);
                            //console.log('open in popup: ' + docIdentifier);
                            //CR.Page.openPopup(docViewerUrl, 'Document Popup', false);
							} else {
                                console.log('open in new tab: ' + docIdentifier);
                                window.open(docViewerUrl, 'Document Viewer - ' + docIdentifier);
                                let event1 = arguments[0] || window.event;
                                CR.Event.stopEvent(event1);
							}
						}
                    });
                    div_list.push($entry);
                });
            });
            // if (defectExists !== null) {
                // if (defectExists.style.display !== "none") {
                    // div_list.push(isDefect.getSelectedDefectList());
                // }
            // }
        }
        else if (data["WorkflowRef"]) {
            var wf = data["WorkflowRef"];
            var title = wf.Name + getRevisionDelimiter() + wf.Revision;
			var encodeTitle = encodeURIComponent(title);
            var step = data["StepName"];
            var dc = encodeURIComponent(JSON.stringify({ "WorkflowCtl": encodeTitle }));
            var url = $(document.body).is(".mobile-device:not(.wide)") ?
                "WorkflowViewPopup_VP.aspx?IsFloatingFrame=2&responsive=true&StepName=" + step + "&DataContracts=" + dc :
                location.href.substr(0, location.href.lastIndexOf("/")) +
                "/WorkflowViewPopup_VP.aspx?IsFloatingFrame=2&IsChild=true&StepName=" + step + "&DataContracts=" + dc;

            if (!(div_list = this._buildPanelContent(url, div_list, name = "WorkflowSlideOut", slideOutAttr = "workflow", 600, 800 )))
                div_list = [];
        }
        else {
            var $root = $("<div class=content-tbl></div>");
            var nodes = [];
            Object.keys(data).forEach(function (k) {
                if (!k.startsWith("__") && k.toLowerCase() !== 'attributes') {
                    var d = data[k];
                    if (d)
                        d = d.Value || "";
                    else
                        d = "";

                    var $r = $("<div class='content-row'><span class='name'></span><span class='val'></span></div>");
                    $(".name", $r).text(k);
                    $(".val", $r).text(d);
                    nodes.push($r);
                }
            });
            $root.html(nodes);
            div_list.push($root);

            if (data["Attributes"]) {
                var attrs = data["Attributes"];
                var $attHdr = $("<div class='header-content-row'><span class='header-name'></span><span class='header-value'></span></div>");
                $(".header-name", $attHdr).text(this._labels.attributeColumnLbl.Value);
                $(".header-value", $attHdr).text(this._labels.valueColumnLbl.Value);

                div_list.push($attHdr);
                var $column = $("<div class=content-tbl></div>");
                var rows = [];
                attrs.forEach(function (ap) {
                    var d = $("<div class='attribute-row content-row'><span class='attribute-name'>" + ap.Name + "</span><span class='attribute-val'>" + ap.AttributeValue + "</span></div>");
                    $(".attribute-name", d).text(ap.Name);
                    $('.attribute-val', d).text(ap.AttributeValue);
                    //  The code below could be used to style the attribute values as "hyperlinks"
                    /*
                    let low = ap.AttributeValue.toLowerCase();
                    if (low.startsWith("\\"))
                        d = $("<div class='attribute-row content-row'><span class='attribute-name'>" + ap.Name + "</span><span class='attribute-val'><a href='file:" + ap.AttributeValue + "' target='_blank'>" + ap.AttributeValue + "</a></span></div>");
                    else if (low.startsWith("www."))
                        d = $("<div class='attribute-row content-row'><span class='attribute-name'>" + ap.Name + "</span><span class='attribute-val'><a href='https://" + ap.AttributeValue + "' target='_blank'>" + ap.AttributeValue + "</a></span></div>");
                    else if ( low.startsWith("http"))
                        d = $("<div class='attribute-row content-row'><span class='attribute-name'>" + ap.Name + "</span><span class='attribute-val'><a href='" + ap.AttributeValue + "' target='_blank'>" + ap.AttributeValue + "</a></span></div>");
                    */
                    rows.push(d);
                });
                $column.html(rows);
                div_list.push($column);
            }
        }
        return div_list;
    },

    _attachDocument: function () {
        var div_list = [];
        var $di = $("<div class=iframe-container></div>");
        $di.append("<iframe></iframe>");
        var dc = encodeURIComponent(JSON.stringify({ "SelectedContainerNameDM": this.getContainer() }));
        var url = "AttachDocument_VP.aspx?IsFloatingFrame=2&responsive=true&DataContracts=" + dc;

        $('iframe', $di)
            .prop("src", url)
            .prop("name", "AttachDocumentSlideOut")
            .attr("slideout", "attachDocument")
            .on("load", function () {
                // "this" is an iframe
                $('body', this.contentDocument)
                    .addClass("commandbar-panel")
                    .addClass("cs-responsive");
            });

        $('iframe', $di).css('width', "100%").css("height", "100%");
        div_list.push($di);
        return div_list;
    },

    _viewTimers: function () {
        var div_list = [];
        var dc = encodeURIComponent(JSON.stringify({ "SelectedContainerNameDM": this.getContainer() }));
        var url = "TimersListPopup_VP.aspx?IsFloatingFrame=2&IsChild=true&responsive=true&slideout=timers&DataContracts=" + dc;
        var $di = $("<div class=iframe-container></div>");
        $di.append("<iframe></iframe>");
        $('iframe', $di)
            .prop("src", url)
            .prop("name", "TimersSlideOut")
            .attr("slideout", "timers")
            .css('width', "100%").css("height", "100%");
        div_list.push($di);
        return div_list;
    },

    _getDocIconPath: function (fileType) {
        var theme = "Camstar";
        var extension = ".png";
        if ($("body").hasClass("Horizon-theme")) {
            theme = "Horizon";
            extension = ".svg";
        }

        var path = "Themes/" + theme + "/images/icons/";
        var icon;
        switch (fileType.toLowerCase()) {
            case ".bmp":
                icon = "typeBMP48"; break;
            case ".rtf":
                icon = "typeRTF48"; break;
            case ".pdf":
                icon = "icon-pdf-32x32"; break;
            case ".docx":
            case ".doc":
                icon = "icon-word-32x32"; break;
            case ".xml":
                icon = "typeXml48"; break;
            case ".exe":
                icon = "typeExe48"; break;
            case ".exel":
                icon = "typeMsExcel48"; break;
            case ".gif":
                icon = "typeGif48"; break;
            case ".jpg":
            case ".jpeg":
                icon = "icon-jpg-32x32"; break;
            case ".txt":
                icon = "typeTxt48"; break;
            case ".zip":
                icon = "typeZipFile48"; break;
            case ".png":
                icon = "typePng48"; break;
            case ".pptx":
            case ".pptm":
            case ".ppt":
                icon = "typeMsPowerpoint48"; break;
            case ".html":
            case ".htm":
                icon = "HTML_32_32"; break;

            case ".jt":
                switch (theme.toLowerCase()) {
                    case "camstar":
                        icon = "3D_32_32"; break;
                    case "horizon":
                        icon = "Type3D48"; break;
                }
                break;
            case "url":
                icon = "HTML_32_32"; break;
            default:
                icon = "icon-unknown-32x32"; break;
        }
        path += icon + extension;

        return path;
    },

    _mfgAudit: function () {
        var div_list = [];
        var dc = JSON.stringify({ "AddContainerDM": this.getContainer() });
        var dcEncoded;

        if (dc) {
            dcEncoded = "DataContracts=" + encodeURI(dc);
        }

        var url = "MfgAuditTrailVP.aspx?IsFloatingFrame=2&IsChild=true&responsive=true&" + dcEncoded + "&CallStackKey=" + __page.generateQuickGuid();

        return this._buildPanelContent(url, div_list, "MfgAuditTrailSlideOut", "mfgaudit", null, null);
    },

    _buildPanelContent: function (url, div_list, name, slideOutAttr, height, width) {

        if ($(document.body).is(".mobile-device:not(.wide)")) {
            // slideout
            var $di = $("<div class=iframe-container></div>");
            $di.append("<iframe></iframe>");

            $('iframe', $di)
                .prop("src", url)
                .prop("name", name)
                .attr("slideout", slideOutAttr)
                .on("load", function () {
                    // "this" is an iframe
                    $('body', this.contentDocument)
                        .addClass("commandbar-panel")
                        .addClass("cs-responsive");
                });

            $('iframe', $di).css('width', "100%").css("height", "100%");
            div_list.push($di);
        }
        else {
            // Popup

            pop.showAjax(url, null,
                this.isResponsive = height || (screen.availHeight - 20)/*height*/,
                this.isResponsive = width || (screen.availWidth - 80)/*width*/,
                this.isResponsive ? 0 : 100/*top*/,
                this.isResponsive ? 0 : 100/*left*/, true /*showButtons*/,
                "" /*okButtonText*/, ""/*closeButtonText*/,
                this /*element*/, true /*closeOnCancel*/,
                ''/*optionArgs*/, null /*cancelConfirmMsg*/, false, false /*display reset*/);
        }

        return div_list;
    },

    _getContainerNameObj: function () {
        var containerName = this.getContainer();
        if (containerName)
            return "{\"AddContainerDM\":\"" + encodeURIComponent(containerName) + "\"}";

        return "";
    }

});


function EProcedureCommandBar() {
    ContainerStatusWebPart_Deco.call(this);
}

EProcedureCommandBar.prototype = $.extend(Object.create(ContainerStatusWebPart_Deco.prototype), {

    initialize: function (commandBarObj) {
        ContainerStatusWebPart_Deco.prototype.initialize.apply(this, commandBarObj);
        if (this._commandBarItems.length < 8) {
            var baseCmdItems = this._commandBarItems;
            // Solid copy
            this._commandBarItems = $.merge([], this._eproc_commandBarItems);

            // Eproc buttons should be first
            $.merge(this._commandBarItems, baseCmdItems);

            // consider custom buttons
            this._commandBarItems = this.GetMergedCommandBarItems(this._commandBarItems);
        }
    },

    GetCommandBarGlobals: function () {
        var me = this;
        return $.extend(ContainerStatusWebPart_Deco.prototype.GetCommandBarGlobals.call(this),
            {
                onTransactionsLoaded: function (transactionArray) {
                    return this._onTransactions(transactionArray);
                }
            });
    },

    _onTransactions: function (trButtons) {

        var containerValue = ContainerStatusWebPart_Deco.prototype.transactionsLoaded.call(this, trButtons);

        $(document.body)
            .addClass("page-eproc-m")
            .toggleClass("empty-container", !containerValue)
            .toggleClass("eproc-panel-page", $(".body-eproc-txn").length != 0);     // if the transaction page is displayed
    },

    _eproc_commandBarItems: [
        {
            Id: "EProcTaskList",
            Action: {
                PanelBuilder: function () {
                    return this._openTasklistSummary();
                },
                Visible: function () {
                    return this._isEprocDefined();
                }
            },
            Icon: { CSS: 'eproc-tasklist-summary' }, // TODO: Update when we get icon from product management
            Name: { Label: 'TaskListSummary_Title' }
        },
        {
            Id: "RecordProductionEvent",
            Action: {
                Page: "ProductionEventRecord_VPR2",
                GetDataContracts: function () { return this._getContainerNameObj(); },
                PanelBuilder: function () { return this._recordProdEvent(); },
                OpenMode: function () {
                    if ($(document.body).hasClass("mobile-device"))
                        return $(document.body).is(".mobile-device:not(.wide)") ? "slideout" : "popup";
                    return "newtab";
                },
                Visible: function () {
                    return this._isEprocDefined();
                }
            },
            Icon: { CSS: 'record-prod-event' },
            Name: { Label: 'Lbl_RecordProductionEvent' }
        }
    ],

    _openTasklistSummary: function () {

        var taskObj = this._getTxnTask();
        var isCheckSheet = $('form')[0].action.indexOf("scsCheckSheetVPR2") > -1;

        if (isCheckSheet) {
            CallServer(JSON.stringify(
                {
                    serverType: "Camstar.WebPortal.Helpers.scsContainerStatusInquiry",
                    clientType: "EProcedureCommandBar",
                    containerName: this.getContainer(),
                    eProcedure: this.getEProcedure(),
                    tasklistName: taskObj.taskList,
                    tasklistRevision: taskObj.taskListRevision,
                    tasklistId: taskObj.taskListId,
                    taskName: taskObj.task,
                    fun: "openCheckSheetTasklistSummary"
                })
            );
        } else {
            CallServer(JSON.stringify(
                {
                    serverType: "Camstar.WebPortal.Helpers.ContainerStatusInquiry",
                    clientType: "EProcedureCommandBar",
                    containerName: this.getContainer(),
                    tasklistName: taskObj.taskList,
                    tasklistRevision: taskObj.taskListRevision,
                    tasklistId: taskObj.taskListId,
                    taskName: taskObj.task,
                    fun: "openTasklistSummary"
                })
            );
        }
    },

    _getTxnTask: function () {
        var $tasklistItem = $('#ctl00_WebPartManager_TaskListWP_TaskListMenu_AccordionMenu_Div .active');
        var $taskItem = $('#ctl00_WebPartManager_TaskMenuWP_TaskMenu_ScrollableMenu_Container_Div.selected .taskitem-m');

        return { taskList: $tasklistItem.text(), task: $taskItem.text().replace('*', ''), taskListId: $tasklistItem.attr("tasklist-id"), taskListRevision: $tasklistItem.attr("tasklist-revision") };
    },

    _isEprocDefined: function () {
        return $('#ctl00_WebPartManager_TaskListWP_TaskListMenu_AccordionMenu_Div').length > 0;
    },

    _getContainerInfo: function (filter) {
        // ContainerList control
        var containerName = this._getContainer();

        var taskObj = this._getTxnTask();

        var isCheckSheet = $('form')[0].action.indexOf("scsCheckSheetVPR2") > -1;
        if (containerName) {
            if (isCheckSheet) {
                CallServer(JSON.stringify(
                    {
                        serverType: "Camstar.WebPortal.Helpers.scsContainerStatusInquiry",
                        clientType: "EProcedureCommandBar",
                        fun: "getContainerInfo",
                        containerName: containerName,
                        tasklistName: taskObj.taskList,
                        taskListRevision: taskObj.taskListRevision,
                        taskListId: taskObj.taskListId,
                        taskName: taskObj.task,
                        filter: filter
                    }), null);
            }
            else {
                CallServer(JSON.stringify(
                    {
                        serverType: "Camstar.WebPortal.Helpers.ContainerStatusInquiry",
                        clientType: "EProcedureCommandBar",
                        fun: "getContainerInfo",
                        containerName: containerName,
                        tasklistName: taskObj.taskList,
                        taskListRevision: taskObj.taskListRevision,
                        taskListId: taskObj.taskListId,
                        taskName: taskObj.task,
                        filter: filter
                    }), null);
            }
        }
        else
            return this._containerIsNotSelected();

        return this._processing();
    },

    _recordProdEvent: function () {
        var div_list = [];
        var dcEncoded = "DataContracts=" + encodeURI(JSON.stringify({ "AddContainerDM": this.getContainer() }));

        var url = "ProductionEventRecord_VPR2.aspx?IsFloatingFrame=2&IsChild=true&responsive=true&" +
            dcEncoded + "&CallStackKey=" + __page.generateQuickGuid();

        return this._buildPanelContent(url, div_list, name = "RecordProdEvntSlideOut", slideOutAttr = "recprodevnt", null, null);
    },

    setCallbackData: function (objData) {

        var baseData = ContainerStatusWebPart_Deco.prototype.setCallbackData.call(this, objData);
        if (baseData)
            return baseData;

        switch (objData.__fun) {
            case "openTasklistSummary":
                return this._viewTasklistSummary(objData);
            case "openCheckSheetTasklistSummary":
                return this._viewCheckSheetTasklistSummary(objData);
            default:
                return null;
                break;
        }
    },

    _viewTasklistSummary: function (objData) {
        var div_list = [];

        var tasklistSummaryDM = objData.OpenTaskListSummary;
        var taskCDO = tasklistSummaryDM.TaskCDO;
        //var popupTitle = tasklistSummaryDM.TasklistSummaryLabel;

        var containerRef = taskCDO.ExecuteTask_Container;
        //Bug 287719 - Fix Task List Summary not reading '#' in container name
        var dc = JSON.stringify({ "cdmTaskListSummary": encodeURIComponent(containerRef.Name), "cdmIsResponsive": 'true' });
        var url = location.href.substr(0, location.href.lastIndexOf("/")) +
            "/EProcTaskListSummaryVP.aspx?IsFloatingFrame=2&responsive=true&DataContracts=" + dc;

        return this._buildPanelContent(url, div_list, "EProcTaskListSummary", "eproc-tasklist-summary", null, null);
    },

    _viewCheckSheetTasklistSummary: function (objData) {
        var div_list = [];

        var tasklistSummaryDM = objData.OpenCheckSheetTasklistSummary;
        var taskCDO = tasklistSummaryDM.TaskCDO;
        var popupTitle = tasklistSummaryDM.TasklistSummaryLabel;
        var eProcedureRef = taskCDO.ExecuteCheckSheet_ElectronicProcedure;
        var containerRef = taskCDO.ExecuteCheckSheet_Container;
        var dc = JSON.stringify({ "cdmTaskListSummary": encodeURIComponent(containerRef.Name), "cdmIsResponsive": 'true', "cdmHiddenElectronicProcedureDM": eProcedureRef.Name });
        var url = location.href.substr(0, location.href.lastIndexOf("/")) +
            "/SS_CheckSheetTaskListSummaryVP.aspx?IsFloatingFrame=2&responsive=true&DataContracts=" + dc;

        return this._buildPanelContent(url, div_list, popupTitle, "eproc-tasklist-summary", null, null);
    },

    _getEProcedure: function () {
        var eProcControl = $find("ctl00_WebPartManager_HiddenSelectedContainer_WP_HiddenElectronicProcedure");

        if (eProcControl) {
            return eProcControl.get_Data();
        }
        return null;
    },

    getEProcedure: function () {
        return this._getEProcedure();
    }


});

// Handles creation of the commandbar/sidebar action buttons on the landing page
function scsLandingPageCommandBar() {
    CommandBarBase_Deco.call(this);
}

scsLandingPageCommandBar.prototype = $.extend(Object.create(CommandBarBase_Deco.prototype), {

    initialize: function (commandBarObj) {
        CommandBarBase_Deco.prototype.initialize.apply(this, commandBarObj);
    },

    /* public virtual */
    GetCommandBarItems: function () {
        var me = this;
        this._commandBarItems.forEach(function (c) {
            if (c.Action && c.Action.Visible === undefined) {
                c.Action.Visible = me._isContainerDefined;
            }
        });
        return this.GetMergedCommandBarItems(this._commandBarItems);
    },

    GetCommandBarGlobals: function () {
        var me = this;
        return $.extend(CommandBarBase_Deco.prototype.GetCommandBarGlobals.call(this),
            {
                transactionThreshold: function () { return 2; },
                //onTransactionsLoaded: function (transactionArray) {
                //    return this.transactionsLoaded(transactionArray);
                //},
                getLabels: function (cmdObj) {
                    var baseLabels = CommandBarBase_Deco.prototype.get_instance().labels || {};
                    $.extend(cmdObj._labels, baseLabels);
                    return cmdObj._labels;
                },

                showMobileMenu: true
            });
    },

    _commandBarItems: [
        {
            Id: "RefreshAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_RefreshButton"); },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "landing-page-refresh" },
            Name: { Label: "Lbl_Refresh" }
        },
        {
            Id: "WIPAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_WIPButton"); },
                Visible: function () { return this._isContainerDefined(); }
            },
            Icon: { CSS: "landing-page-wip" },
            Name: { Label: "Lbl_WIP" }
        },
        {
            Id: "LotDetailsAction",
            Action: {
                PanelBuilder: function () { return this._getContainerInfo('all'); },
                Visible: function () { return this._isContainerDefined(); },
                Disabled: function () { return $("#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00")[0].value == "" ? true : false; }
            },
            Icon: { CSS: "landing-page-lotdetails" },
            Name: { Label: "UIWebPart_LotDetails" },
            CSS: { Bar: function () { return $("#ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00")[0].value == "" ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "EqpDetailsAction",
            Action: {
                PanelBuilder: function () { return this._getEqpInfo(); },
                Visible: function () { return this._isContainerDefined(); },
                PanelTitle: function () { return $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value == "" ? "Eqp Details" : $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value + " Details"; },
                Disabled: function () {
                    let eqpname = $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value;
                    if (eqpname != "" && $("#" + eqpname + "_EqpStatusWP_Tile_LotCount").length) {
                        return false;
                    }
                    return true;
                }
            },
            Icon: { CSS: "landing-page-eqp-details" },
            Name: { Text: "Eqp Details" },
            CSS: {
                Bar: function () {
                    let eqpname = $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value;
                    if (eqpname != "" && $("#" + eqpname + "_EqpStatusWP_Tile_LotCount").length) {
                        return null;
                    }
                    return ["cmdbar-aspNetDisabled"];
                }
            }
        },
        {
            Id: "PMAction",
            Action: {
                PanelBuilder: function () { return this._getPMInfo(); },
                Visible: function () { return this._isContainerDefined(); },
                PanelTitle: function () { return $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value == "" ? "PMs" : "PMs of " + $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value; },
                Disabled: function () {
                    let eqpname = $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value;
                    if (eqpname != "" && $("#" + eqpname + "_EqpStatusWP_Tile_PM").length) {
                        return false;
                    }
                    return true;
                }
            },
            Icon: { CSS: "landing-page-pm" },
            Name: { Label: "SS_JobTypeEnum_PM" },
            CSS: {
                Bar: function () {
                    let eqpname = $("#ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00")[0].value;
                    if (eqpname != "" && $("#" + eqpname + "_EqpStatusWP_Tile_PM").length) {
                        return null;
                    }
                    return ["cmdbar-aspNetDisabled"];
                }
            }
        }
    ],

    _getContainerNameObj: function () {
        var containerName = this.getContainer();
        if (containerName)
            return "{\"WIPMain_RedirectSelectedLot\":\"" + containerName + "\"}";
        return "";
    },

    _labels: {
        attributeColumnLbl: { Name: "AttributeTitle", Value: null },
        valueColumnLbl: { Name: "SelVal_Value", Value: null }
    },

    /* public virtual */
    setCallbackData: function (objData) {
        switch (objData.__fun) {
            case "getContainerInfo":
                return this._renderContainerStatusInfo(objData);
            case "getPMInfo":
                return this._renderPMStatusInfo(objData);
            case "getEqpInfo":
                return this._renderEqpInfo(objData);
            default:
                break;
        }
    },

    getContainer: function () {
        return this._getContainer();
    },

    _isContainerDefined: function () {
        return true;
    },

    _getContainerInfo: function (filter) {
        var containerName = this._getContainer();
        var fun = "getContainerInfo";
        var serverType = "Camstar.WebPortal.Helpers.scsDashboardSlideOutHelper";

        if (containerName) {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "LandingPageVP_Deco",
                    fun: fun,
                    containerName: containerName,
                    filter: filter
                }), null);
        }
        else
            return this._containerIsNotSelected();

        return this._processing();
    },

    _getEqpInfo: function () {
        var fun = "getEqpInfo";
        var serverType = "Camstar.WebPortal.Helpers.scsDashboardSlideOutHelper";

        if ($get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value != "") {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "LandingPageVP_Deco",
                    fun: fun,
                    containerName: $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value,
                }), null);
        } else
            return [$("<div class='error'> There is no equipment selected. </div>")];

        return this._processing();
    },

    _getPMInfo: function () {
        var fun = "getPMInfo";
        var serverType = "Camstar.WebPortal.Helpers.scsDashboardSlideOutHelper";

        if ($get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value != "") {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "LandingPageVP_Deco",
                    fun: fun,
                    containerName: $get("ctl00_WebPartManager_scsEqpStatusWP_HiddenSelectedEqp_ctl00").value,
                }), null);
        } else
            return [$("<div class='error'> There is no equipment selected. </div>")];

        return this._processing();
    },

    _containerIsNotSelected: function () {
        return [$("<div class='error'> There is no container selected. </div>")];
    },

    _processing: function () {
        return [$("<div>loading...</div>")];
    },

    _getContainer: function () {
        var containerControl = $get("ctl00_WebPartManager_scsDispatchListWP_HiddenSelectedContainer_ctl00");
        if (containerControl) {
            return containerControl.value;
        }
        return null;
    },

    _renderContainerStatusInfo: function (data) {
        var div_list = [];
        if (data.Error) {
            div_list.push($("<div class=error>" + data.Error + "</div>"));
        } else {
            var $root = $("<div class=content-tbl></div>");
            var nodes = [];
            Object.keys(data).forEach(function (k) {
                if (!k.startsWith("__") && k.toLowerCase() !== 'attributes' && !k.toLowerCase().match("color")) {
                    var d = data[k];
                    if (d && d != ":")
                        d = d || "";
                    else
                        d = "";

                    var $r = $("<div class='content-row'><span class='name'></span><span class='val'></span></div>");
                    $(".name", $r).text(k);
                    $(".val", $r).text(d);
                    nodes.push($r);
                }
            });
            $root.html(nodes);
            div_list.push($root);
        }
        return div_list;
    },

    _renderEqpInfo: function (data) {
        if (data.Empty) {
            return [("<div class='error'> There is no data to display. </div>")];
        }
        var state = data.TileContext;
        var mainNode;
        var div_list = [];
        if (state && state.Columns) {
            if (Array.isArray(state.Columns)) {
                for (var i = 0; i < state.Columns.length; i++) {
                    var col = state.Columns[i];
                    if (col.Visible) {
                        var node = document.createElement("div");
                        node.className = "tileColumn";
                        node.setAttribute("data-name", col.Name);
                        node.setAttribute("data-ind", i);

                        if (col.CssClass)
                            node.classList.add(col.CssClass);
                        if (col.ColumnStyle)
                            node.style.cssText = col.ColumnStyle;
                        if (col.TileStyle)
                            this._columnTileCustomStyle[col.Name] = col.TileStyle;

                        var title = document.createElement("span");
                        title.className = "title-label";
                        title.innerHTML = col.Title;

                        //node.appendChild(title);
                        mainNode = node;
                    }
                }
            }
        }

        if (state && state.Columns && state.Tiles) {
            if (Array.isArray(state.Tiles)) {
                var sectionchk = "Lot";

                //DIV for Lot Section
                var sectionLot = document.createElement("div");
                sectionLot.id = 'sectionLot';
                sectionLot.className = 'accordionHeader opened';
                sectionLot.innerHTML = "<div class='headerImageContainer'><span class='arrow'></span></div>";
                var lotLbl = document.createElement('span');
                lotLbl.className = 'accordionHeaderLabel';
                lotLbl.innerHTML = 'Lot Details';
                sectionLot.append(lotLbl);
                sectionLot.onclick = function()
                {
                    this.classList.toggle("opened");
                    var content = this.nextElementSibling;
                    if (!content.style.display || content.style.display === "block") {
                        content.style.display = "none";
                    } else{
                        content.style.display = "block";
                    }
                }
                var sectionLotContent = document.createElement("div");
                sectionLotContent.id = 'sectionLotContent';
                sectionLotContent.className = 'accordionContent opened';


                ////DIV for Material Section
                var sectionMat = document.createElement("div");
                sectionMat.id = 'sectionMat';
                sectionMat.className = 'accordionHeader opened';
                sectionMat.innerHTML = "<div class='headerImageContainer'><span class='arrow'></span></div>";
                var matLbl = document.createElement('span');
                matLbl.className = 'accordionHeaderLabel';
                matLbl.innerHTML = 'Material Details';
                sectionMat.append(matLbl);
                sectionMat.onclick = function()
                {
                    this.classList.toggle("opened");
                    var content = this.nextElementSibling;
                    if (!content.style.display || content.style.display === "block") {
                        content.style.display = "none";
                    } else{
                        content.style.display = "block";
                    }
                }
                var sectionMatContent = document.createElement("div");
                sectionMatContent.id = 'sectionMatContent';
                sectionMatContent.className = 'accordionContent opened';

                ////DIV for Tool Section
                var sectionTool = document.createElement("div");
                sectionTool.id = 'sectionTool';
                sectionTool.className = 'accordionHeader opened';
                sectionTool.innerHTML = "<div class='headerImageContainer'><span class='arrow'></span></div>";
                var toolLbl = document.createElement('span');
                toolLbl.className = 'accordionHeaderLabel';
                toolLbl.innerHTML = 'Tool Details';
                sectionTool.append(toolLbl);

                sectionTool.onclick = function()
                {
                    this.classList.toggle("opened");
                    var content = this.nextElementSibling;
                    if (!content.style.display || content.style.display === "block") {
                        content.style.display = "none";
                    } else{
                        content.style.display = "block";
                    }
                }
                var sectionToolContent = document.createElement("div");
                sectionToolContent.id = 'sectionToolContent';
                sectionToolContent.className = 'accordionContent opened';

                for (var i = 0; i < state.Tiles.length; i++) {
                    var tempString = state.Tiles[i];
                    sectionchk = "Lot";
                    if (tempString.Text) {
                        if (Array.isArray(tempString.Text)) {
                            for (var j = 0; j < tempString.Text.length; j++) {
                                if (tempString.Text[j].includes("Material Type"))
                                {
                                    sectionchk = "Mat";
                                    break;
                                }
                                else if (tempString.Text[j].includes("Object Category"))
                                {
                                    sectionchk = "Tool";
                                    break;
                                }
                            }
                        }
                    }

                    // Pass in DefaultImage defined in Portal Page

                    //this._addTile(mainNode, state.Tiles[i], state);
                    if (sectionchk == "Lot")
                    {
                        this._addTile(sectionLotContent, state.Tiles[i], state);
                    }
                    else if (sectionchk == "Mat")
                    {
                        this._addTile(sectionMatContent, state.Tiles[i], state);
                    }
                    else if (sectionchk == "Tool")
                    {
                        this._addTile(sectionToolContent, state.Tiles[i], state);
                    }
                }

            }
            else
                console.error("Invalid Tiles value: array is expected.");
            div_list.push(sectionLot);
            div_list.push(sectionLotContent);
            div_list.push(sectionMat);
            div_list.push(sectionMatContent);
            div_list.push(sectionTool);
            div_list.push(sectionToolContent);
        }

        // Add qty image
        $(div_list).find(".textArea span:nth-child(2)").each(function () {
            var qtyHTML = $(this).html()
                .replace("_QTY_ICON_", "<img src='assets/image/indicatorContainsInnerMismatches16.svg' style='width:16px;' title='Qty'/>")
                .replace("_QTY2_ICON_", "<img src='assets/image/indicatorPartiallyAssignedByDescendants16.svg' style='width:16px;' title='Qty2'/>");
            $(this).html(qtyHTML);
        })

        return div_list;
    },

    _renderPMStatusInfo: function (data) {
        if (data.Empty) {
            return [("<div class='error'> There is no data to display. </div>")];
        }
        var state = data.TileContext;
        var mainNode;
        var div_list = [];
        if (state && state.Columns) {
            if (Array.isArray(state.Columns)) {
                for (var i = 0; i < state.Columns.length; i++) {
                    var col = state.Columns[i];
                    if (col.Visible) {
                        var node = document.createElement("div");
                        node.className = "tileColumn";
                        node.setAttribute("data-name", col.Name);
                        node.setAttribute("data-ind", i);

                        if (col.CssClass)
                            node.classList.add(col.CssClass);
                        if (col.ColumnStyle)
                            node.style.cssText = col.ColumnStyle;
                        if (col.TileStyle)
                            this._columnTileCustomStyle[col.Name] = col.TileStyle;

                        var title = document.createElement("span");
                        title.className = "title-label";
                        title.innerHTML = col.Title;

                        //node.appendChild(title);
                        mainNode = node;
                    }
                }
            }
        }

        if (state && state.Columns && state.Tiles) {
            if (Array.isArray(state.Tiles)) {
                for (var i = 0; i < state.Tiles.length; i++) {
                    // Pass in DefaultImage defined in Portal Page
                    this._addTile(mainNode, state.Tiles[i], state);
                }
                div_list.push(mainNode);
            }
            else
                console.error("Invalid Tiles value: array is expected.");
        }

        div_list.push("<div class='right'><input type='submit' onclick='$(\".close-button\").click();$(\"#ctl00_WebPartManager_ButtonsBar_PMPage\").click();' title='Open in Maintenance Management page.' class='cs-button'  value='Go to Maintenance' style='float: right;'/></div>");
        return div_list;
    },

    _addTile: function (_element, tileObj, state) {
        if (tileObj.ColumnName) {
            var colDiv = _element;
            if (colDiv) {
                var tileDiv = document.createElement("div");
                tileDiv.className = "item";
                tileDiv.setAttribute("data-ind", colDiv.querySelectorAll(".item").length);
                if (tileObj.CustomData)
                    tileDiv.setAttribute("data-custom", tileObj.CustomData);

                var body = document.createElement("div");
                body.className = "textArea";

                var titleArea = document.createElement("div");
                titleArea.className = "titleArea";

                var iconLeft = document.createElement("span");
                var image = tileObj.Image;
                if (!image) {
                    var column = null;
                    state.Columns.forEach(function (f) {
                        if (f.Name === tileObj.ColumnName) {
                            column = f;
                            return true;
                        }
                    });
                    if (column && column.DefaultImage)
                        image = column.DefaultImage;
                    else
                        image = state.DefaultImage;
                }
                if (image) {
                    var site = window.location.pathname.split("/");
                    if (site.length > 1)
                        iconLeft.style.backgroundImage = "url(/" + site[1] + "/assets/image/" + image + ")";
                }
                iconLeft.className = "icon";

                var bodyTitle = document.createElement("span");
                bodyTitle.className = "title";
                bodyTitle.innerText = tileObj.Title;
                //titleArea.appendChild(iconLeft);
                titleArea.appendChild(bodyTitle);

                body.appendChild(titleArea);

                var typechk = null;

                if (tileObj.Text) {
                    if (Array.isArray(tileObj.Text)) {
                        for (var i = 0; i < tileObj.Text.length; i++) {
                            if (tileObj.Text[i].includes("Material Type"))
                            {
                                typechk = "mat";
                            }
                            else if (tileObj.Text[i].includes("Object Category"))
                            {
                                typechk = "tool";
                            }

                            var textSpan = document.createElement("span");
                            textSpan.innerText = tileObj.Text[i];
                            body.appendChild(textSpan);
                        }
                    }
                    else
                        console.error("Invalid Text value: array of strings is expected.");
                }

                var iconRight = document.createElement("span");

                iconRight.className = "icon-last";
                var icon = document.createElement("span");
                if (typechk == "mat")
                    icon.className = "mat-icon-first";
                else if (typechk == "tool")
                    icon.className = "tool-icon-first";
                else
                    icon.className = "icon-first";
                tileDiv.appendChild(icon);
                tileDiv.appendChild(body);
                //tileDiv.appendChild(iconRight);
                colDiv.appendChild(tileDiv);

                return colDiv;
            }
        }

        return $("<div class='error'> There is no data to display. </div>");
    }
});

function getTabsByVPName(vpName) {
    var $result = [];
    var $tabs = $("div#tabContainerControl ul#tablist li[role='tab'] a[role='presentation", window.parent.document);
    $tabs.each(function () {
        var item = $(this);
        let id = item[0].hash;
        var $t = $(id, window.parent.document);
        var $iframe = $("iframe", $t);
        let regex = /\w+\.aspx/;
        let vp = $iframe[0].src.match(regex);
        if (vp == $.trim(vpName).replace('*', '') + ".aspx") {
            $result.push(item);
        }
    });
    return $result;
}

// Handles creation of the commandbar/sidebar action buttons on the new WIP Main page
function scsSimplePageCommandBar() {
    CommandBarBase_Deco.call(this);
}

scsSimplePageCommandBar.prototype = $.extend(Object.create(CommandBarBase_Deco.prototype), {

    initialize: function (commandBarObj) {
        CommandBarBase_Deco.prototype.initialize.apply(this, commandBarObj);
    },

    /* public virtual */
    GetCommandBarItems: function () {
        var me = this;
        this._commandBarItems.forEach(function (c) {
            if (c.Action && c.Action.Visible === undefined) {
                c.Action.Visible = me._isContainerDefined;
            }
        });
        return this.GetMergedCommandBarItems(this._commandBarItems);
    },

    GetCommandBarGlobals: function () {
        var me = this;
        return $.extend(CommandBarBase_Deco.prototype.GetCommandBarGlobals.call(this),
            {
                transactionThreshold: function () { return 2; },
                //onTransactionsLoaded: function (transactionArray) {
                //    return this.transactionsLoaded(transactionArray);
                //},
                getLabels: function (cmdObj) {
                    var baseLabels = CommandBarBase_Deco.prototype.get_instance().labels || {};
                    $.extend(cmdObj._labels, baseLabels);
                    return cmdObj._labels;
                },

                showMobileMenu: true
            });
    },

    _commandBarItems: [
        {
            Id: "SubmitAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_Submit"); },
                Visible: true
            },
            Icon: { CSS: "simple-wipmain-submit" },
            Name: { Label: "SubmitButton" }
        },
        {
            Id: "ResetAction",
            Action: {
                RedirectClick: function () { $get('__EVENTARGUMENT').value = 'ResetSubmitPostBackArgument'; return $("#ctl00_WebPartManager_ButtonsBar_Reset"); },
                Visible: true
            },
            Icon: { CSS: "simple-wipmain-reset" },
            Name: { Label: "ResetButton" },
        },
        {
            Id: "CarrierAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_CarrierAction"); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-carrier" },
            Name: { Label: "CSICDOName_Carrier" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "WIPMessage",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_WIPMsgAction"); },
                Visible: true,
                Disabled: function () { return !this._isWIPMsgExist(); }
            },
            Icon: { CSS: "simple-wipmain-wipmsg" },
            Name: { Label: "CSICDOName_WIPMsg" },
            CSS: { Bar: function () { return !this._isWIPMsgExist() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "Details",
            Action: {
                PanelBuilder: function () { return this._getLotDetails(); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-details" },
            Name: { Text: "Details" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "Equipment",
            Action: {
                PanelBuilder: function () { return this._getEqpStatus(); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-equipment" },
            Name: { Text: "Equipment" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "UndoTrackIn",
            Action: {
                RedirectClick: function () { $get('__EVENTARGUMENT').value = 'UndoTrackInPostBackArgument'; return $("#ctl00_WebPartManager_ButtonsBar_UndoTrackIn"); },
                Visible: function () { return this._isWIPState("2"); },
            },
            Icon: { CSS: "simple-wipmain-undotrackin" },
            Name: { Text: "Undo Track In" }
        },
        {
            Id: "OnlineTraveler",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_OnlineTravelerAction"); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-onlinetraveler" },
            Name: { Text: "Online Traveler" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "ContainerDocuments",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_DocumentSetAction"); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "container-documents" },
            Name: { Label: "Web_Documents" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "WIPFailures",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_FailuresAction"); },
                Visible: true,
                Disabled: function () { return !this._isWIPState("3"); }
            },
            Icon: { CSS: "simple-wipmain-failure" },
            Name: { Label: "WebUI_Failures" },
            CSS: { Bar: function () { return !this._isWIPState("3") ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "LotInfo",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_LotInfoAction"); },
                Visible: true,
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-lotinfo" },
            Name: { Text: "Lot Info" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }

        },
        {
            Id: "ResourceInfoAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_ResourceInfoAction"); },
                Visible: true,
                Disabled: function () { return !this._isResourceAvailable(); }
            },
            Icon: { CSS: "simple-wipmain-resourceinfo" },
            Name: { Label: "UIInSiteResourcesDetailsMenu" },
            CSS: { Bar: function () { return !this._isResourceAvailable() ? ["cmdbar-aspNetDisabled"] : null; } }

        },
        {
            Id: "DisplaySPC",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_LastSPCDisplayAction"); },
                Visible: true,
                Disabled: function () { return !this._isSPCDataAvailable(); }
            },
            Icon: { CSS: "simple-wipmain-spcchart" },
            Name: { Label: "CSICDOName_SPCChart" },
            CSS: { Bar: function () { return !this._isSPCDataAvailable() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "SurveillAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_SurveillAction"); },
                Visible: true,
            },
            Icon: { CSS: "simple-wipmain-surveill" },
            Name: { Label: "CSICDOName_ss_Surveillance" }
        },
        {
            Id: "ProductionEvent",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_ProductionEventAction"); },
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "simple-wipmain-pe" },
            Name: { Label: "ProductionEventInquiry_ProductionEvent" },
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "PreTrackIn",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_ButtonsBar_PreTrackIn"); },
                Visible: function () { return this._isWIPState("1"); }
            },
            Icon: { CSS: "simple-wipmain-pretrackin" },
            Name: { Text: "Pre-Track In" },
        },
        {
            Id: "ContainerWorkflow",
            Action: {
                PanelBuilder: function () { return this._getWorkflowInfo(); },
                OpenMode: function () { return $(document.body).is(".mobile-device:not(.wide)") ? "slideout" : "popup"; },
                Disabled: function () { return !this._isContainerDefined(); }
            },
            Icon: { CSS: "container-workflow" },
            Name: { Label: "Container_Workflow" },
            // CSS: { Bar: [], Panel: ["workflow"] }
            CSS: { Bar: function () { return !this._isContainerDefined() ? ["cmdbar-aspNetDisabled"] : null; }, Panel: ["workflow"] }
        }
    ],

    _getContainerNameObj: function () {
        var containerName = this.getContainer();
        if (containerName)
            return "{\"WIPMain_RedirectSelectedLot\":\"" + containerName + "\"}";
        return "";
    },

    _isContainerDefined: function () {
        return this.getContainer() ? true : false;
    },

    _isSPCDataAvailable: function () {
        let spc = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_SPCDataAvailable_ctl00");
        let attr = spc.attr("checked");
        if (attr === "checked")
            return true;
        else
            return false;
    },

    _isResourceAvailable: function () {
        var resourceControl = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_Equipment_Edit");
        if (resourceControl)
            return resourceControl.val();
        return "";
    },

    _labels: {
        attributeColumnLbl: { Name: "AttributeTitle", Value: null },
        valueColumnLbl: { Name: "SelVal_Value", Value: null }
    },

    getContainer: function () {
        return this._getContainer();
    },

    _getContainer: function () {
        var containerControl = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_LotItem_ContainerId_ctl00");
        if (containerControl) {
            return containerControl.val();
        }
        return null;
    },

    _isWIPState: function (wipflag) {
        return this.getWIPFlag() == wipflag ? true : false;
    },

    getWIPFlag: function () {
        return this._getWIPFlag();
    },

    _getWIPFlag: function () {
        var wipFlagControl = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_HiddenSelectedState_ctl00");
        if (wipFlagControl) {
            return wipFlagControl.val();
        }
        return null;
    },

    _isWIPMsgExist: function () {
        var wipMsgControl = $("#ctl00_WebPartManager_scsWIPMain_ControllerWP_WIPMain_WIPInstructions");
        if (wipMsgControl) {
            return wipMsgControl.val().length > 0 ? true : false;
        }
        return null;
    },

    _containerIsNotSelected: function () {
        return [$("<div class='error'> There is no container selected. </div>")];
    },

    _processing: function () {
        return [$("<div>loading...</div>")];
    },

    _getWorkflowInfo: function () {

        var containerName = this._getContainer();
        var fun = "getContainerInfo";
        var serverType = "Camstar.WebPortal.Helpers.ContainerStatusInquiry";

        if (containerName) {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "scsSimplePageCommandBar",
                    fun: fun,
                    containerName: containerName,
                    filter: "workflow"
                }), null);
        }
        else
            return this._containerIsNotSelected();

        return this._processing();
    },

    /* public virtual */
    _getLotDetails: function (filter) {
        var containerName = this._getContainer();
        var fun = "getLotDetails";
        var serverType = "Camstar.WebPortal.Helpers.scsSimpleWipMainSlideOutHelper";

        if (containerName) {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "SimpleWIPMainVP_Deco",
                    fun: fun,
                    containerName: containerName,
                    filter: filter
                }), null);
        }
        else
            return this._containerIsNotSelected();

        return this._processing();
    },

    _getEqpStatus: function () {
        var containerName = this._getContainer();
        var fun = "getEqpStatus";
        var serverType = "Camstar.WebPortal.Helpers.scsSimpleWipMainSlideOutHelper";

        if (containerName) {
            CallServer(JSON.stringify(
                {
                    serverType: serverType,
                    clientType: "SimpleWIPMainVP_Deco",
                    fun: fun,
                    containerName: null,
                }), null);
        } else
            return [$("<div class='error'> There is no equipment selected. </div>")];

        return this._processing();
    },

    /* public virtual */
    setCallbackData: function (objData) {
        switch (objData.__fun) {
            case "getLotDetails":
                return this._renderLotDetails(objData);
            case "getEqpStatus":
                return this._renderEqpStatus(objData);
            case "getContainerInfo":
                return this._renderWorkflowInfo(objData);
            default:
                break;
        }
    },

    _renderWorkflowInfo: function (data) {
        var div_list = [];
        if (data.Error) {
            div_list.push($("<div class=error>" + data.Error + "</div>"));
        }

        if (data["WorkflowRef"]) {
            var wf = data["WorkflowRef"];
            var title = wf.Name + ":" + wf.Revision;
            var step = data["StepName"];
            var dc = JSON.stringify({ "WorkflowCtl": title });
            var url = $(document.body).is(".mobile-device:not(.wide)") ?
                "WorkflowViewPopup_VP.aspx?IsFloatingFrame=2&responsive=true&StepName=" + step + "&DataContracts=" + dc :
                location.href.substr(0, location.href.lastIndexOf("/")) +
                "/WorkflowViewPopup_VP.aspx?IsFloatingFrame=2&IsChild=true&StepName=" + step + "&DataContracts=" + dc;

            if (!(div_list = this._buildPanelContent(url, div_list, name = "WorkflowSlideOut", slideOutAttr = "workflow", 600, 800)))
                div_list = [];
        }

        return div_list;
    },

    _buildPanelContent: function (url, div_list, name, slideOutAttr, height, width) {

        if ($(document.body).is(".mobile-device:not(.wide)")) {
            // slideout
            var $di = $("<div class=iframe-container></div>");
            $di.append("<iframe></iframe>");

            $('iframe', $di)
                .prop("src", url)
                .prop("name", name)
                .attr("slideout", slideOutAttr)
                .on("load", function () {
                    // "this" is an iframe
                    $('body', this.contentDocument)
                        .addClass("commandbar-panel")
                        .addClass("cs-responsive");
                });

            $('iframe', $di).css('width', "100%").css("height", "100%");
            div_list.push($di);
        }
        else {
            // Popup

            pop.showAjax(url, null,
                this.isResponsive = height || (screen.availHeight - 20)/*height*/,
                this.isResponsive = width || (screen.availWidth - 80)/*width*/,
                this.isResponsive ? 0 : 100/*top*/,
                this.isResponsive ? 0 : 100/*left*/, true /*showButtons*/,
                "" /*okButtonText*/, ""/*closeButtonText*/,
                this /*element*/, true /*closeOnCancel*/,
                ''/*optionArgs*/, null /*cancelConfirmMsg*/, false, false /*display reset*/);
        }

        return div_list;
    },

    _renderLotDetails: function (data) {
        var div_list = [];
        if (data.Error) {
            div_list.push($("<div class=error>" + data.Error + "</div>"));
        } else {
            var $root = $("<div class=content-tbl></div>");
            var nodes = [];
            Object.keys(data).forEach(function (k) {
                if (!k.startsWith("__") && k.toLowerCase() !== 'attributes' && !k.toLowerCase().match("color")) {
                    var d = data[k];
                    if (d && d != ":")
                        d = d || "";
                    else
                        d = "";

                    var $r = $("<div class='content-row'><span class='name'></span><span class='val'></span></div>");
                    $(".name", $r).text(k);
                    $(".val", $r).text(d);
                    nodes.push($r);
                }
            });
            $root.html(nodes);
            div_list.push($root);
        }
        return div_list;
    },

    _renderEqpStatus: function (data) {
        if (data.Empty) {
            return [("<div class='error'> There is no data to display. </div>")];
        }

        var eqpStatus = data.TileContext;
        var div_list = [];
        var TEMPLATE = {
            lotCountHTML: '<span class="eqp-lot-count" title="Lot Count">_LOT_COUNT_VALUE_</span>',
            imgEqpUnavailable: '<img src="./assets/image/indicatorStatusStopped16.svg" class="eqp-status-icon" title="Unavailable">',
            imgEqpAvailable: '<img src="./assets/image/indicatorStatusReady16.svg" class="eqp-status-icon" title="Available">',
            imgPMDue: '<img src="./assets/image/indicatorStatusMaintenance16.svg" class="eqp-status-icon" title="PM Required">'
        };

        var mainDiv = document.createElement("div");
        mainDiv.className = "tileContainer nowrap multiColumn";

        var subDiv = document.createElement("div");
        subDiv.className = "tileColumn";
        subDiv.setAttribute("data-name", "EqpStatus");
        subDiv.setAttribute("data-ind", 0);

        for (var i = 0; i < eqpStatus.length; i++) {
            var itemDiv = document.createElement("div");
            itemDiv.className = "item";
            itemDiv.setAttribute("data-ind", subDiv.querySelectorAll(".item").length);
            itemDiv.setAttribute("data-custom", eqpStatus[i].EquipmentName);

            var iconFirst = document.createElement("span");
            iconFirst.className = "icon-first";
            itemDiv.appendChild(iconFirst);

            var textArea = document.createElement("div");
            textArea.className = "textArea";

            var titleArea = document.createElement("div");
            titleArea.className = "titleArea";

            var titleDiv = document.createElement("span");
            titleDiv.className = "title";
            titleDiv.innerText = eqpStatus[i].EquipmentName;

            titleArea.appendChild(titleDiv);

            if (eqpStatus[i].LotCount > 0) {
                var countDiv = document.createElement("span");
                countDiv.innerHTML = TEMPLATE.lotCountHTML.replace("_LOT_COUNT_VALUE_", eqpStatus[i].LotCount);

                titleArea.appendChild(countDiv);
            }

            textArea.appendChild(titleArea);

            var textDesc = document.createElement("span");
            textDesc.innerText = "Description: " + eqpStatus[i].EquipmentDescription;
            textArea.appendChild(textDesc);

            var textStatus = document.createElement("span");
            textStatus.innerText = "Status: " + eqpStatus[i].EquipmentStatus;
            textArea.appendChild(textStatus);

            var iconLast = document.createElement("span");
            var iconLastHTML = "";
            if (eqpStatus[i].EquipmentAvailability == "2") {
                iconLastHTML += TEMPLATE.imgEqpUnavailable;
            }
            else {
                iconLastHTML += TEMPLATE.imgEqpAvailable;
            }

            if (eqpStatus[i].RequiredPM)
                iconLastHTML += TEMPLATE.imgPMDue;

            iconLast.innerHTML = iconLastHTML;

            textArea.appendChild(iconLast);
            itemDiv.appendChild(textArea);

            subDiv.appendChild(itemDiv);
        }

        mainDiv.appendChild(subDiv);
        div_list.push(mainDiv);

        //if (state && state.Columns) {
        //    if (Array.isArray(state.Columns)) {
        //        for (var i = 0; i < state.Columns.length; i++) {
        //            var col = state.Columns[i];
        //            if (col.Visible) {
        //                var node = document.createElement("div");
        //                node.className = "tileColumn";
        //                node.setAttribute("data-name", col.Name);
        //                node.setAttribute("data-ind", i);

        //                if (col.CssClass)
        //                    node.classList.add(col.CssClass);
        //                if (col.ColumnStyle)
        //                    node.style.cssText = col.ColumnStyle;
        //                if (col.TileStyle)
        //                    this._columnTileCustomStyle[col.Name] = col.TileStyle;

        //                var title = document.createElement("span");
        //                title.className = "title-label";
        //                title.innerHTML = col.Title;

        //                //node.appendChild(title);
        //                subNode = node;
        //            }
        //        }
        //    }
        //}

        //if (state && state.Columns && state.Tiles) {
        //    if (Array.isArray(state.Tiles)) {
        //        for (var i = 0; i < state.Tiles.length; i++) {
        //            // Pass in DefaultImage defined in Portal Page
        //            this._addTile(subNode, state.Tiles[i], state);
        //        }
        //        mainNode.appendChild(subNode);
        //        div_list.push(mainNode);
        //    }
        //    else
        //        console.error("Invalid Tiles value: array is expected.");
        //}

        return div_list;
    },

    _addTile: function (_element, tileObj, state) {
        if (tileObj.ColumnName) {
            var colDiv = _element;
            if (colDiv) {
                var tileDiv = document.createElement("div");
                tileDiv.className = "item";
                tileDiv.setAttribute("data-ind", colDiv.querySelectorAll(".item").length);
                if (tileObj.CustomData)
                    tileDiv.setAttribute("data-custom", tileObj.CustomData);

                var body = document.createElement("div");
                body.className = "textArea";

                var titleArea = document.createElement("div");
                titleArea.className = "titleArea";

                var iconLeft = document.createElement("span");
                var image = tileObj.Image;
                if (!image) {
                    var column = null;
                    state.Columns.forEach(function (f) {
                        if (f.Name === tileObj.ColumnName) {
                            column = f;
                            return true;
                        }
                    });
                    if (column && column.DefaultImage)
                        image = column.DefaultImage;
                    else
                        image = state.DefaultImage;
                }
                if (image) {
                    var site = window.location.pathname.split("/");
                    if (site.length > 1)
                        iconLeft.style.backgroundImage = "url(/" + site[1] + "/assets/image/" + image + ")";
                }
                iconLeft.className = "icon";

                var bodyTitle = document.createElement("span");
                bodyTitle.className = "title";
                bodyTitle.innerText = tileObj.Title;
                //titleArea.appendChild(iconLeft);
                titleArea.appendChild(bodyTitle);
                body.appendChild(titleArea);

                if (tileObj.Text) {
                    if (Array.isArray(tileObj.Text)) {
                        for (var i = 0; i < tileObj.Text.length; i++) {
                            var textSpan = document.createElement("span");
                            textSpan.innerText = tileObj.Text[i];
                            body.appendChild(textSpan);
                        }
                    }
                    else
                        console.error("Invalid Text value: array of strings is expected.");
                }

                var iconFirst = document.createElement("span");
                iconFirst.className = "icon-first";
                var iconLast = document.createElement("span");
                //iconLast.className = "icon-last";

                // Status icon
                var iconStatusHTML = "";

                var TEMPLATE = {
                    lotCountHTML: '<span class="eqp-lot-count" title="Lot Count">_LOT_COUNT_VALUE_</span>',
                    imgEqpUnavailable: '<img src="./assets/image/indicatorStatusStopped16.svg" class="eqp-status-icon" title="Unavailable">',
                    imgEqpAvailable: '<img src="./assets/image/indicatorStatusReady16.svg" class="eqp-status-icon" title="Available">',
                    imgPMDue: '<img src="./assets/image/indicatorStatusMaintenance16.svg" class="eqp-status-icon" title="PM Required">'
                };

                if (tileObj.CurrentAvailability == "2") {
                    iconStatusHTML += TEMPLATE.imgEqpUnavailable;
                }
                else {
                    iconStatusHTML += TEMPLATE.imgEqpAvailable;
                }

                // PM Required
                if (tileObj.RequiredPM)
                    iconStatusHTML += TEMPLATE.imgPMDue;

                iconLast.innerHTML = iconStatusHTML;
                body.appendChild(iconLast);

                tileDiv.appendChild(iconFirst);
                tileDiv.appendChild(body);

                colDiv.appendChild(tileDiv);

                return colDiv;
            }
        }

        return $("<div class='error'> There is no data to display. </div>");
    }

});

// Handles creation of the commandbar/sidebar action buttons on the new WIP Main page
function scsMatrixPageCommandBar() {
    CommandBarBase_Deco.call(this);
}

scsMatrixPageCommandBar.prototype = $.extend(Object.create(CommandBarBase_Deco.prototype), {

    initialize: function (commandBarObj) {
        CommandBarBase_Deco.prototype.initialize.apply(this, commandBarObj);
    },

    /* public virtual */
    GetCommandBarItems: function () {
        var me = this;
        this._commandBarItems.forEach(function (c) {
            if (c.Action && c.Action.Visible === undefined) {
                c.Action.Visible = me._isContainerDefined;
            }
        });
        return this.GetMergedCommandBarItems(this._commandBarItems);
    },

    GetCommandBarGlobals: function () {
        var me = this;
        return $.extend(CommandBarBase_Deco.prototype.GetCommandBarGlobals.call(this),
            {
                transactionThreshold: function () { return 7; },
                getLabels: function (cmdObj) {
                    var baseLabels = CommandBarBase_Deco.prototype.get_instance().labels || {};
                    $.extend(cmdObj._labels, baseLabels);
                    return cmdObj._labels;
                },

                showMobileMenu: true
            });
    },

    _commandBarItems: [
        {
            Id: "NewAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_ButtonAdd"); },
                Visible: function () { return true; }
            },
            Icon: { CSS: "matrix-new-action" },
            Name: { Label: "AddButton" }
        },
        {
            Id: "RefreshAction",
            Action: {
                RedirectClick: function () { return $("#ctl00_WebPartManager_SS_SetupB_SelectionWP_Selection_PageRefresh"); },
                Visible: function () { return true; }
            },
            Icon: { CSS: "matrix-refresh-action" },
            Name: { Label: "RefreshButton" }
        },
        {
            Id: "EditAction",
            Action: {
                RedirectClick: function () { return $("#edit_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid"); },
                Visible: function () { return true; },
                Disabled: function () { return this._isRowSelected(); }
            },
            Icon: { CSS: "matrix-edit-action" },
            Name: { Label: "Actions_Edit" },
            CSS: { Bar: function () { return this._isRowSelected() ? ["matrix-cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "DeleteAction",
            Action: {
                RedirectClick: function () { return $("#del_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid"); },
                Visible: function () { return true; },
                Disabled: function () { return this._isRowSelected(); }
            },
            Icon: { CSS: "matrix-delete-action" },
            Name: { Label: "DeleteAction" },
            CSS: { Bar: function () { return this._isRowSelected() ? ["matrix-cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "ViewAction",
            Action: {
                RedirectClick: function () { return $("#details_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid"); },
                Visible: function () { return true; },
                Disabled: function () { return this._isRowSelected(); }
            },
            Icon: { CSS: "matrix-view-action" },
            Name: { Label: "Lbl_ViewDetails" },
            CSS: { Bar: function () { return this._isRowSelected() ? ["matrix-cmdbar-aspNetDisabled"] : null; } }
        },
        {
            Id: "AuditTrailAction",
            Action: {
                RedirectClick: function () { return $("#audit_ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid"); },
                Visible: function () { return true; },
                Disabled: function () { return this._isRowSelected(); }
            },
            Icon: { CSS: "matrix-audit-trail-action" },
            Name: { Label: "CSICDOName_AuditTrail" },
            CSS: { Bar: function () { return this._isRowSelected() ? ["matrix-cmdbar-aspNetDisabled"] : null; } }
        }
    ],

    _isRowSelected: function () {
        var theGrid = jQuery("#ctl00_WebPartManager_SS_SetupB_SelectionWP_SelectionGrid");
        return theGrid[0].control.get_selectedCount() > 0 ? false : true;
    },
});