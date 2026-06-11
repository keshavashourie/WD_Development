// Copyright Siemens 2023 

/// <reference path="../MicrosoftAjaxExt.js"/>
/// <reference path="../Camstar.UI/Control.js" />
/// <reference path="../../jquery/jquery.min.js" />
/// <reference path="../../jquery/jquery-ui.min.js" />
Type.registerNamespace('Camstar.WebPortal.PortalFramework');

//Constructor
Camstar.WebPortal.PortalFramework.WebPartBase = function(element)
{
    this.isWebPart = true;

    Camstar.WebPortal.PortalFramework.WebPartBase.initializeBase(this, [element]);

    /*** Private Member Variables ***/
    this._serverType = null;
    this._serverID = null;
    this._zoneElement = null;
    this._isResizable = false;

    this._dirtyFlagTrigger = true;
    this._width = null;
    this._commandBarSettings = null;
};

Camstar.WebPortal.PortalFramework.WebPartBase.prototype =
{
    initialize: function()
    {
        //call base intialize function
        Camstar.WebPortal.PortalFramework.WebPartBase.callBaseMethod(this, 'initialize');

        var webPartElement = this.get_element();
        var me = this;

        // add 'cell empty' style for cells with all invisible children.
        this._setEmptyCell(webPartElement.id);

        // Command Bar initailization
        var cmdtype = this.get_CommandBarSettings();
        if (cmdtype) {
            var p = getCEP_top().__page;
            if (p)
                p.setPageSideBar(cmdtype, webPartElement.ownerDocument);
        }

        if (this._isResizable)
        {
            $(function()
            {
                var zoneElement = $(me.get_zoneElement());
                var zoneContainer = zoneElement.parents('td');
                if (zoneContainer.length > 0)
                {
                    if (Camstars.Browser.IE) // ie doesn't calculate td's width properly.
                    {
                        var zoneContainerTable = zoneElement.parents('table');
                        if (zoneContainerTable.length > 0)
                        {
                            zoneContainerTable.css({ width: 'auto' });
                            zoneContainer.width(zoneContainer.width());
                            zoneContainerTable.css({ width: '100%' });
                        }
                    }
                    if (!zoneContainer.hasClass("vsplitter-container"))
                        zoneContainer.addClass("vsplitter-container");
                    me.makeResizable();
                }
            });
        }

        if ($(document.body).hasClass("Horizon-theme")) {
            var $wp = $(">.webpart", this.get_element());
            if ($wp.hasClass("webpart-containerstatus-m") || $wp.hasClass("ui-webpart-resource-status")) {
                // add to page's init
                $(">form", document.body)
                    .off("page.initialized")
                    .on("page.initialized", function () {
                        containerStatusM_adjustment();
                        resourceStatus_adjustment();
                    });
            }
        }
    },

    makeResizable: function()
    {
        var leftZone = $(this.get_zoneElement()).closest('td');
        if (leftZone.length > 0)
        {
            var rightZone = leftZone.next();
            if (rightZone.length > 0)
            {
                var me = this;                                                                                                   
                $(leftZone).filter(":visible").css({ display: 'inline-block', overflow: 'hidden' });
                if (!leftZone.is('.ui-resizable'))
                {
                    leftZone.attr('origWidth', $(leftZone).width());
                    leftZone.attr('actualWidth', $(leftZone).width());
                    leftZone.resizable({
                        helper: "ui-resizable-helper",
                        handles: 'e',
                        maxWidth: leftZone.width(),
                        stop: function(event, ui)
                        {
                            var deltaX = ui.size.width - ui.originalSize.width;
                            me.resizeContent(ui.element, deltaX);
                            leftZone.attr('actualWidth', ui.size.width);
                            $(ui.element).height('auto');
                        }
                    });

                    // init expand/collapse button for vertical splitter.
                    $('<div class="vSplitter-button"></div>').appendTo($('div.ui-resizable-e', leftZone)).click(function()
                    {
                        var currentWidth = $(leftZone).width();
                        var deltaX = 0;
                        if (currentWidth > 20) // need to collapse
                        {
                            deltaX = 10 - currentWidth;
                            $(leftZone).width(10);
                            leftZone.attr('actualWidth', 10);
                            leftZone.find("input").prop("readonly", true);
                        }
                        else // need to expand
                        {
                            deltaX = leftZone.attr('origWidth') - currentWidth;
                            $(leftZone).width(leftZone.attr('origWidth'));
                            leftZone.attr('actualWidth', leftZone.attr('origWidth'));
                            leftZone.find("input").prop("readonly", "");
                        }
                        me.resizeContent($(leftZone), deltaX);
                    });
                }

                // restores controls' sizes after post back.
                if (leftZone.attr('origWidth') && leftZone.attr('actualWidth'))
                {
                    var zoneWidth = leftZone.attr('actualWidth');
                    leftZone.width(zoneWidth);
                    if (zoneWidth != leftZone.attr('origWidth'))
                    {
                        setTimeout(function()
                        {
                            me.resizeContent(leftZone, zoneWidth - leftZone.attr('origWidth'));
                        }, 0);
                    }
                }

                //in collapse mode, postback causes visual artefacts because left zone 
                //input control receives focus and webpart scrolls. 
                //disabling all input controls prevents getting focus by collapsed left zone
                if ($(leftZone).width() == 10)
                    leftZone.find("input").prop("readonly", true);
                else
                    leftZone.find("input").prop("readonly", "");
            }
        }
    },

    resizeContent: function(leftZone, deltaX)
    {
        var divTwo = $(leftZone).next();
        // resizes grids.
        $('table.ui-jqgrid-btable', divTwo).filter(":visible").each(
            function()
            {
                var g = $find(this.id);
                if (g && g._width)
                {
                    g._width -= deltaX;
                    var theGrid = jQuery(g.GridID);
                    theGrid.setGridWidth(g._width, g._shrinkColumns);
                    g._fixScroll(g._gridID);
                }
            }
        );
        // resizes tab panels.
        $('.ui-tabs-panel', divTwo).filter(":visible").each(
            function()
            {
                $('#' + this.id).width($('#' + this.id).width() - deltaX);
            }
        );

        if ($(leftZone).width() <= 10)
            $('.vSplitter-button', leftZone).addClass('right');
        else
            $('.vSplitter-button', leftZone).removeClass('right');
    },

    dispose: function()
    {
        this._serverType = null;
        this._serverID = null;
        this._zoneElement = null;
        this._isResizable = false;

        Camstar.WebPortal.PortalFramework.WebPartBase.callBaseMethod(this, 'dispose');
    },

    _setEmptyCell: function(elementId)
    {
        $('.webpart .matrix', $get(elementId)).each(function(matrixNum, matrix) {
            if ($(matrix).is(":visible") == true) {
                $('tr, div.row', $(matrix)).each(function (rn, r) {
                    ($(r).children("td.cell, div.cell-m")).each(function (cn, c) {
                            var isToggleContainer = $(this).closest('div.toggle-container,div.accordionContent,.ui-tabs-panel').length > 0;
                            var visibleChildren = $(c).children(":visible");
                            if (visibleChildren.length == 0 && !isToggleContainer) {
                                $(c).addClass("empty");
                            }
                        })
                        .last().addClass('last-cell');
                });
            }
        });
    },

    /*** Public Properties ***/
    get_controlId: function() { return this._serverID; },
    set_controlId: function(value) { this._serverID = value; },

    get_serverType: function() { return this._serverType; },
    set_serverType: function(value) { this._serverType = value; },

    get_zoneElement: function() { return this._zoneElement; },
    set_zoneElement: function(value) { this._zoneElement = value; },

    get_isResizable: function() { return this._isResizable; },
    set_isResizable: function(value) { this._isResizable = value; },

    get_dirtyFlagTrigger: function() { return this._dirtyFlagTrigger; },
    set_dirtyFlagTrigger: function (value) { this._dirtyFlagTrigger = value; },

    get_width: function() { return this._width; },
    set_width: function(value) { this._width = value; },

    get_CommandBarSettings: function () { return this._commandBarSettings; },
    set_CommandBarSettings: function (value) { this._commandBarSettings = value; }
};

// Optional descriptor for JSON serialization.
Camstar.WebPortal.PortalFramework.WebPartBase.descriptor =
{
    properties:
    [
            { name: 'controlId', type: String }, // server ID
            { name: 'serverType', type: String },
            { name: 'zoneElement', type: String }
    ]
};

Camstar.WebPortal.PortalFramework.WebPartBase.registerClass('Camstar.WebPortal.PortalFramework.WebPartBase', Camstar.UI.Control);

//Notifiy ScriptManager that this is the end of the script
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
