// Copyright Siemens 2023  

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
Type.registerNamespace("CamstarPortal.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

/******************* CamstarPortal.WebControls.CollapsableSection *******************/
CamstarPortal.WebControls.ModelingObjectList = function (element) {
    CamstarPortal.WebControls.ModelingObjectList.initializeBase(this, [element]);
    this._mainDivId = null;
    this._treeModeId = null;
    this._mainDivInnerHtml = null;
    this._searchId = null;
    this._isDataGrouped = true;
    this._infoSpanId = null;
    this._objectCountId = null;
    this._extensions = null;
    this._customClickHandler = null;
    this.SelectedItem = null;
    this._name = null;
    this._isExport = null;
    this._isSelectionByGroup = null;
    this._inTheSameTab = null;
    this._changeMgtSaveId = null;
    this._selectedDefaultPkgId = null;
    this.ofLabel = null;
    this._groupHeaderHeight = 0;
    this._staticAreaHeight = 0;
},

CamstarPortal.WebControls.ModelingObjectList.prototype =
{
    initialize: function () {
        CamstarPortal.WebControls.ModelingObjectList.callBaseMethod(this, 'initialize');

        if (this._extensions != null)
            eval(this._extensions);

        var mainDiv = $('#' + this._mainDivId);
        var $mdlClass = mainDiv.closest(".modelingClassList");

        var me = this;

        $('a.panel-toggler', mainDiv).click(function (ev) {
            return me.expand(ev);
        });

        var treeMode = $find(this._treeModeId);
        if ($(treeMode))
            $(treeMode).unbind().bind('changed', me, me.treeModeChanged);

        var booleanChangeMgtSave = $find(this._changeMgtSaveId);
        if (booleanChangeMgtSave) {
            this.changeMgtSaveStateChanged(this, booleanChangeMgtSave.getState());
            $(booleanChangeMgtSave).unbind().bind('stateChanged', me, me.changeMgtSaveStateChanged);
        }

        var searchTextControl = $('#' + this._searchId, $mdlClass);
        $('input', searchTextControl).keyup(function () {
            me.filterClasses();
        });

        $('.modSearchButton', searchTextControl.parent()).click(function () {
            me.filterClasses();
        });

        $('#' + this._infoSpanId, $mdlClass).click(function (e) {
            return me.infoButtonClicked(e);
        });

        this._allClasses = mainDiv.find('a[groupname]');
        $.each($(this._allClasses), function () {
            $(this).unbind().bind('click', me, me.cdoClicked);
        });
        var objectsCount = $('#' + this._objectCountId, $mdlClass);
        var count = this._allClasses.length;
        $(objectsCount).text(count + " " + this.ofLabel + " " + count);

        // Set checkbox
        $('span[id$="_CDOList_chkHideObjects"] :checkbox', $mdlClass)
            .hide().before('<span class="ui-image-checkbox">&nbsp;</span>');

        $('span.ui-image-checkbox', $mdlClass).click(
            function () {
                var chk = $(this).parent().find('input:hidden');
                chk.show().click().hide();
                $(this).attr('state', chk.is(':checked') ? "checked" : "");
            });
        if (this._isExport) {
            if (this._isSelectionByGroup)
                this.displayCdoForExport(''); // don't display any items on control load since group is not selected yet.
            else
                this.displayCdoForExport(String.fromCharCode(2)); // display all items.
        }

        if ($('input', searchTextControl).val()) {
            this.filterClasses();
        }

        if (this._cdoDefId != null)
            this.clickOnCdo();

        // Object list initially collapsed
        if ($("a.panel-toggler", mainDiv).hasClass("collapsed")) {
            $("a.panel-toggler", mainDiv).parent().addClass("collapsed");
        }

        // Calc height 
        this._staticAreaHeight = $(">.view-mode-lbl", $mdlClass).outerHeight() +
            $(">.actions", $mdlClass).outerHeight(true) +
            $(">.searching", $mdlClass).outerHeight(true);

        if ($(">.view-mode-lbl", $mdlClass).next().is(":visible"))
            this._staticAreaHeight += $(">.view-mode-lbl", $mdlClass).next().outerHeight();

        this._groupHeaderHeight = $("a.panel-toggler", mainDiv).outerHeight(true);
        $("div.secondLevel", mainDiv).css("height", "calc( 100vh - " + (this._staticAreaHeight + this._groupHeaderHeight + 8 /*extra padding*/).toString() + "px )").css("display", "");
    },

    Click: function (method) {
        this._customClickHandler = method;
    },

    expand: function (e) {
        var clickedItem = $(e.target);
        if (clickedItem.hasClass('empty'))
            return;

        var t = clickedItem.hasClass('expanded');

        clickedItem.toggleClass('expanded', !t);
        clickedItem.toggleClass('collapsed', t);

        clickedItem.parent().toggleClass('expanded', !t);

        if(t)
            $('div > ul', clickedItem.parent()).fadeOut(200);
        else
            $('div > ul', clickedItem.parent()).fadeIn(200);
    },

    treeModeChanged: function (obj) {
        var editorValue = this._value.value;
        if (obj.data._isExport) {
            obj.data.displayCdoForExport(editorValue);
        }
        else {
            if (editorValue == "1")
                obj.data.ungroupData();
            else if (editorValue == "2")
                obj.data.groupData();
        }
    },

    changeMgtSaveStateChanged: function (obj, val) {
        var $me;
        if (obj.data)
            $me = obj.data.get_element();
        else
            $me = obj.get_element();

        var isOnSelected = val;
        if (isOnSelected) {
            $('.changeMgtOffLabelsDiv', $me).hide();
            $('.changeMgtOnLabelsDiv', $me).show();
        }
        else {
            $('.changeMgtOnLabelsDiv', $me).hide();
            $('.changeMgtOffLabelsDiv', $me).show();
        }
        if (typeof __page !== 'undefined') {
            __page.setSessionVariable("ModelingCDOList_ChangeMgtSave", isOnSelected, false);
            if (!isOnSelected) {
                obj.data.refreshDefaultChangePkgValue('');
                __page.setSessionVariable("ModelingCDOList_DefaultPkgID", null, false);
            }
            __page.saveSessionVariables(['ModelingCDOList_DefaultPkgID', 'ModelingCDOList_ChangeMgtSave']);
        }
    },

    groupData: function () {
        if (!this._isDataGrouped) {
            var mainDiv = $('#' + this._mainDivId);
            $('div.ungroup', mainDiv).remove();

            var me = this;
            $.each($('a', mainDiv), function () {
                $(this).unbind().bind('click', me, me.expand);
            });
            $.each($('a[cdoName]', mainDiv), function () {
                $(this).unbind().bind('click', me, me.cdoClicked);
            });
            var objectsCount = $('#' + this._objectCountId);
            var count = this._allClasses.length;
            $(objectsCount).text(count + " " + this.ofLabel + " " + count);

            this._isDataGrouped = true;
            $('.group', mainDiv).removeClass("hide-group");
        }
    },

    ungroupData: function (filteredClasses) {
        if (this._isDataGrouped || filteredClasses) {
            var mainDiv = $('#' + this._mainDivId);
            var groupUl = $('.group', mainDiv);

            var classes = groupUl.find('a[groupname]');
            if (filteredClasses)
                classes = filteredClasses;

            var objectsCount = $('#' + this._objectCountId);
            var countAll = this._allClasses.length;
            var coundDisplayed = classes.length;
            $(objectsCount).text(coundDisplayed + " " + this.ofLabel + " " + countAll);

            classes.sort(function (a, b) {
                var class1 = $(a).text().toLowerCase();
                var class2 = $(b).text().toLowerCase();
                return ((class1 < class2) ? -1 : ((class1 > class2) ? 1 : 0));
            });

            var classesHtml = "<div class='ungroup'>";
            $.each(classes, function () {
                var anchor = $(this);
                var el = anchor[0];
                if (el) {
                    var classHtml = "<a ";
                    for (var i = 0, attrs = el.attributes, l = attrs.length; i < l; i++) {
                        var attr = attrs.item(i);
                        var attrValue = attr.nodeValue;
                        if (attr.nodeName == 'class')
                            attrValue += ' ungroup';
                        //skip jQuery data ID attributes, which cause problems in IE 8, when reused
                        if (attr.nodeName.indexOf("jQuery") == -1)
                            classHtml += attr.nodeName + "='" + attrValue + "' ";
                    }
                    classHtml += ">" + anchor.text() + "</a>";
                    classesHtml += classHtml;
                }
            });
            classesHtml += "</div>";

            groupUl.addClass("hide-group");

            $('div.ungroup', mainDiv).remove();
            mainDiv.append(classesHtml);
            $('div.ungroup', mainDiv).css("height", "calc( 100vh - " + (this._staticAreaHeight + 4/*extra padding*/).toString() + "px )");

            var me = this;
            $.each($('a[cdoName]', $('div.ungroup')), function () {
                $(this).unbind().bind('click', me, me.cdoClicked);
            });
            this._isDataGrouped = false;
        }
    },

    displayCdoForExport: function (groupName) {
        $.each(this._allClasses, function () {
            $(this).addClass("hidden");
        });
        var coundDisplayed = 0;
        if (groupName) {
            if (groupName == String.fromCharCode(2)) // display all items.
            {
                $.each(this._allClasses, function () {
                    $(this).removeClass("hidden");
                    coundDisplayed++;
                });
            }
            else {
                $.each(this._allClasses, function () {
                    var separator = String.fromCharCode(2);
                    var groups = $(this).attr('groupname').split(separator);
                    if ($.inArray(groupName, groups) > -1) {
                        $(this).removeClass("hidden");
                        coundDisplayed++;
                    }
                });
            }
        }
        
        var objectsCount = $('#' + this._objectCountId);
        var countAll = this._allClasses.length;
        $(objectsCount).text(coundDisplayed + " " + this.ofLabel + " " + countAll);
    },

    filterClasses: function () {
        var searchText = $find(this._searchId).getValue();
        if (searchText || this._isExport) {
            var classes = $.grep(this._allClasses, function (value) {
                if ($(value).text().toLowerCase().match(searchText.toLowerCase()))
                    return value;
                return null;
            });
            this.ungroupData(classes);
        }
        else
            this.groupData();
    },


    infoButtonClicked: function () {
    },

    clickOnCdo: function () {
        var me = this;

        setTimeout(function () {
            var el = $("a[CDODefID=" + me._cdoDefId + "]");
            el.click();
        }, 100);

        var el = $("div[id$='mainObjectsList'] a.collapsed");
        el.click();
    },

    cdoClicked: function (e) {
        var me = e.data;
        me.markClickedCdo(e, this);
        var tabcont = $find('ctl00_WebPartManager_ModelingTabWP_TabContainer');
        if (tabcont) {
            var pageName = $(this).attr("PageName");
            var wip = "&wip=";
            if ($(this).attr("WIP"))
                wip += 'true';
            else
                wip += 'false';

            var curStackKey = __page.get_CallStackKey();
            var query = "ResetCallStack=true&id=" + $(this).attr("CDODefID") + '&isRDO=' + $(this).attr("IsRDO") + '&maint='
               + $(this).attr("maint") + wip + '&name=' + encodeURI($(this).text()) + '&CDOName=' + $(this).attr("CDOName")
               + '&maintTypeId=' + $(this).attr("maintTypeId") + '&InstanceID=' + (typeof ($(this).attr("InstanceID")) === 'undefined' ? "" : $(this).attr("InstanceID"));

            if (curStackKey)
                query += "&pStackId=" + curStackKey;

            if (me._inTheSameTab) {
                if (curStackKey)
                    query += "&CallStackKey=" + curStackKey;
            }

            OpenModelingPageWitinTab('ctl00_WebPartManager_ModelingTabWP_TabContainer', pageName, $(this).text(), query);
        }
        if (me._customClickHandler != null) {
            me.SelectedItem = this;
            var key = $(this).attr("CDODefID");
            window.cdoKey = key;
            return me._customClickHandler.call(this);
        }
        return false;
    },

    markClickedCdo: function (e, clickedAnchor) {
        var anchors = e.data._allClasses;
        if (!this._isDataGrouped) {
            var mainDiv = $('#' + e.data._mainDivId);
            anchors = $('a[cdoName]', mainDiv);
        }
        $.each(anchors, function () {
            if (clickedAnchor == this) {
                $(this).removeClass("clicked").addClass("clicked");
            }
            else {
                $(this).removeClass("clicked");
            }
        }
        );
    },

    refreshDefaultChangePkgValue: function (packageName) {
        var $defaultChangePkg = $('#' + this._selectedDefaultPkgId);
        if ($defaultChangePkg.length) {
            if (packageName)
                $defaultChangePkg.text(packageName);
            else
                $defaultChangePkg.text('');
        }
    },

    dispose: function () {
        this._mainDivId = null;
        this._treeModeId = null;
        this._selectedDefaultPkgId = null;
        this._mainDivInnerHtml = null;
        this._isDataGrouped = null;
        this._searchId = null;
        this._infoSpanId = null;
        this._objectCountId = null;
        this._isExport = null;
        this._isSelectionByGroup = null;
        this._name = null;
        this._changeMgtSaveId = null;
        CamstarPortal.WebControls.ModelingObjectList.callBaseMethod(this, 'dispose');
    },

    get_treeModeId: function () { return this._treeModeId; },
    set_treeModeId: function (value) { this._treeModeId = value; },

    get_selectedDefaultPkgId: function () { return this._selectedDefaultPkgId; },
    set_selectedDefaultPkgId: function (value) { this._selectedDefaultPkgId = value; },

    get_changeMgtSaveId: function () { return this._changeMgtSaveId; },
    set_changeMgtSaveId: function (value) { this._changeMgtSaveId = value; },

    get_searchId: function () { return this._searchId; },
    set_searchId: function (value) { this._searchId = value; },

    get_mainDivId: function () { return this._mainDivId; },
    set_mainDivId: function (value) { this._mainDivId = value; },

    get_infoSpanId: function () { return this._infoSpanId; },
    set_infoSpanId: function (value) { this._infoSpanId = value; },

    get_objectCountId: function () { return this._objectCountId; },
    set_objectCountId: function (value) { this._objectCountId = value; },

    get_name: function () { return this._name; },
    set_name: function (value) { this._name = value; },

    get_isExport: function () { return this._isExport; },
    set_isExport: function (value) { this._isExport = value; },

    get_isSelectionByGroup: function () { return this._isSelectionByGroup; },
    set_isSelectionByGroup: function (value) { this._isSelectionByGroup = value; },

    get_extensions: function () { return this._extensions; },
    set_extensions: function (value) { this._extensions = value; },

    get_inTheSameTab: function () { return this._inTheSameTab; },
    set_inTheSameTab: function (value) { this._inTheSameTab = value; },

    get_cdoDefId: function () { return this._cdoDefId; },
    set_cdoDefId: function (value) { this._cdoDefId = value; },

    get_ofLabel: function () { return this.ofLabel; },
    set_ofLabel: function (value) { this.ofLabel = value; }
};
CamstarPortal.WebControls.ModelingObjectList.registerClass('CamstarPortal.WebControls.ModelingObjectList', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
