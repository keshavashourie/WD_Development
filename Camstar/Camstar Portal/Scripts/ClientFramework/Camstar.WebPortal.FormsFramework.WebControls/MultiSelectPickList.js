// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="PickLists/PickListCommon.js" />
/// <reference path="PickLists/PickListControl.js" />
/// <reference path="PickLists/PickListPanel.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList = function(element)
{
    Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList.initializeBase(this, [element]);

    //#region Properties
    this._initialData = null;
    this._filterColumn = '';
    this._footerDisplayText = '';
	this._pageCount = null;
    //#endregion
},

Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList.prototype =
{
    initialize: function()
    {
        Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList.callBaseMethod(this, 'initialize');        

        var me = this;        
        if (typeof (__page) !== 'undefined') {
            var lbls = [{ Name: 'Lbl_Name' }, { Name: 'Lbl_GridDisplayingToOfItems' }];
            this._setLabelValues(lbls, __page, me);
        }
        
        if (this._panel && this._panel._filter) {
            this._panel._view.control.isMultiPickList = true;

            var initialData = this.get_initialData();
            if (initialData) {
                var newItems = [];
                var jsonStr = JSON.stringify(initialData);
                var json = eval('(' + JSON.parse(jsonStr) + ')');
                if (json.length) {                    
                    $.each(json, function () {
                        var item = eval(this);
                        newItems.push(item);
                    });
                }               

                this._panel.set_initialData(newItems);
                this._panel.set_loaded(true);
                this._panel.load(false);
            }            
            $('.filter', me._panel._element).show();
        }
        else {
            $('.filter', this._panel._element).hide();
        }                
    },

    dispose: function()
    {
        Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList.callBaseMethod(this, 'dispose');
    },              

    initializeFilter: function (me) {
        if (me._panel && me._panel._filter) {
            me._panel._filterTextBox = $get(this._panel._filter.id + '_Fltc', me._panel._filter);
            $(me._panel._filterTextBox)
                .unbind('keyup')
                .unbind('keydown')
                .bind('keyup', function (e) { me.onFilterChanged(e); })
                .bind('keydown', function (e) { me._panel.onFilterChanging(e); });
        }        
    },
	
    onFilterChanged: function (e) {
        if (!this._pageCount)
            this._pageCount = this._panel._view.control._element.p.lastpage; //get the lastpage number i.e., total pages count 
        if (this._panel._filterCompleted) {
            if (this._panel._filterPrevValue != this._panel._filterTextBox.value) {
                this._panel._filterPrevValue = this._panel._filterTextBox.value;
                if (!this._panel._disableAutoFiltering && this._pageCount <= 1) {
                    // client filtering
                    this.filterItems(e, this);
                    this._panel.setCurrentPage(1);
                    this._panel._filterCompleted = true;
                }
                else {
                    this._panel._filterForceRequestData.value = '1';
                    this._panel._filterCompleted = false;
                    if (!this._panel._disableAutoFiltering) {
                        this._panel.updateInProgress = 'Started';
                        this._panel.setCurrentPage(1);
                        this._panel._filterPrevValue = this._panel._filterTextBox.value;
                        this._panel.requestData(true);
                    }
                }
            }
        }
    },

    filterItems: function (e, me) {        
            var panel = me._panel;
            var filterText = panel._filterTextBox.value.toLocaleLowerCase();
            if (filterText.lastIndexOf("%") == (filterText.length - 1)) {
                filterText = filterText.substring(0, filterText.length - 1);
            }
            var searchfromBegining = true;
            if (filterText.indexOf("%") == 0) {
                filterText = filterText.substring(1);
                searchfromBegining = false;
            }
            var totalFilteredItems = 0;
            var selectedItems = 0;
            var initialItems = me.get_initialData();
            var items = $('tr[role=row]', panel.get_element());
            $.each(items, function () {
                var txt = '';
                // For now, filtering looks in column with a value of Name, we should revisit to make it configurable
                var filterColumn = me.get_filterColumn();
                var cells = $('td[aria-describedby$=_' + filterColumn + ']', $(this));
                var cb = $('td[aria-describedby$=_cb]', $(this));
                if (cells) {
                    var cellVal = $(cells).first();
                    if (cellVal.length) {
                        txt = $(cellVal).first().text().trimStart().toLocaleLowerCase();
                        if (txt !== '') {
                            var isShowItem = true;
                            if (searchfromBegining) {
                                if (txt.startsWith(filterText)) {
                                    isShowItem = true;
                                } else {
                                    isShowItem = false;
                                }
                            } else {
                                if (txt.indexOf(filterText) != -1) {
                                    isShowItem = true;
                                } else {
                                    isShowItem = false;
                                }
                            }

                            if (!isShowItem) {
                                $(this).hide();
                            }
                            else {
                                $(this).show();
                                totalFilteredItems++;

                                var curRow = $(this).closest('tr');
                                var prevRow = curRow.prev(':visible');
                                if (typeof (prevRow) !== 'undefined') {
                                    var isPrevSecondary = $(prevRow).hasClass('ui-priority-secondary');
                                    var isCurSecondary = $(this).hasClass('ui-priority-secondary');
                                    var isCurPrimary = $(this).hasClass('ui-priority-primary');
                                    if (isCurPrimary || isCurSecondary) {
                                        if (isCurSecondary && isPrevSecondary) {
                                            $(this).removeClass('ui-priority-secondary');
                                            $(this).addClass('ui-priority-primary');
                                        }
                                        else if (!isCurSecondary && !isPrevSecondary) {
                                            $(this).removeClass('ui-priority-primary');
                                            $(this).addClass('ui-priority-secondary');
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            panel._totalRecords = totalFilteredItems;      
            me.__displayTotalItems(me, panel);        
    },
    
    get_initialData: function () {
        return this._initialData;
    },
    set_initialData: function (value) {
        this._initialData = value;
    },        
   
    get_filterColumn: function (){
        return this._filterColumn;
    },

    get_footerDisplayText: function(){
        return this._footerDisplayText;
    },

    __displayTotalItems: function (me, panel) {        
        var pagingInfo = $('.ui-paging-info', panel.get_element());
        if (pagingInfo && pagingInfo.length) {
            var info = $(pagingInfo).first();
            if (info) {
                var footerDisplayText = me._footerDisplayText;
                var txt = footerDisplayText.replace('{0}', panel._totalRecords);
                txt = txt.replace('{1}', panel._totalRecords);
                txt = txt.replace('{2}', panel._totalRecords);
                $(info).text(txt).show();
            }
        }        
    },

    _setLabelValues: function (lblNames, page, me) {                       
        page.getLabels(lblNames, function (resp) {
            if ($.isArray(resp)) {                    
                $.each(resp, function () {
                    var lblName = this.Name;
                    var lblValue = this.Value;
                    switch (lblName) {
                        case 'Lbl_Name':
                            me._filterColumn = lblValue;
                            break;
                        case 'Lbl_GridDisplayingToOfItems':
                            me._footerDisplayText = lblValue;
                            break;
                        default:
                            break;
                    }
                });
            }
            else {
                alert(resp.Error);
            }
            me.initializeFilter(me);
        });                                         
    },
},

Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList.registerClass('Camstar.WebPortal.FormsFramework.WebControls.MultiSelectPickList', Camstar.WebPortal.FormsFramework.WebControls.PickLists.PickListControl);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
