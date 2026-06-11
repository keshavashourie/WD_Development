// Copyright Siemens 2025 

/// <reference path="../../MicrosoftAjaxExt.js"/>
/// <reference path="../../Camstar.UI/Control.js" />
/// <reference path="../../../jsPlumb/jsplumb.js" />

Type.registerNamespace("CamstarPortal.WebControls");

CamstarPortal.WebControls.WorkflowCanvas = function (element)
{
    CamstarPortal.WebControls.WorkflowCanvas.initializeBase(this, [element]);
},

CamstarPortal.WebControls.WorkflowCanvas.prototype =
{
    initialize: function ()
    {
        this._defaultConnectorStyle = { stroke: "rgba(0,128,0,1)", strokeWidth: 2 }; // green solid
        this._conditionalConnectorStyle = { stroke: "rgba(0,128,0,1)", strokeWidth: 2, "dashstyle": "2 2" }; // green dotted
        this._reworkConnectorStyle = { stroke: "rgba(255,0,0,0.5)", strokeWidth: 2, "dashstyle": "2 2" }; // red dotted
        this._selectedConnectorStyle = { stroke: "rgba(0,0,0,1)", strokeWidth: 2 }; // black solid
        this.Connections = {
            PRIMARY: { value: 0, name: "primary" },
            ALTERNATE: { value: 1, name: "conditional" },
            REWORK: { value: 2, name: "rework" },
            SELECTED: { value: 3, name: "selected" }
        };

        this._modelingPageQuery = null;
        this._modelingPageName = null;
        this._modelingPageTitle = null;

        CamstarPortal.WebControls.WorkflowCanvas.callBaseMethod(this, 'initialize');
        this._serverType = "CamstarPortal.WebControls.WorkflowCanvas, CamstarPortal.WebControls";
    },

    dispose: function ()
    {
        this._defaultConnectorStyle = null;
        this._conditionalConnectorStyle = null;
        this._reworkConnectorStyle = null;
        this._selectedConnectorStyle = null;
        this._disableReworkPath = null;
        this._modelingPageQuery = null;
        this._modelingPageName = null;
        this._modelingPageTitle = null;

        CamstarPortal.WebControls.WorkflowCanvas.callBaseMethod(this, 'dispose');
    },

    jsPlumbIni: function ()
    {
        var overlay = [
            ["Arrow", {
                location: 0.8,
                id: "arrow",
                length: 10,
                foldback: 1,
                width: 10
            }
            ]];
        var hoverStyle = { stroke: "#000", strokeWidth: 2 };

        jsPlumb.importDefaults({
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            Endpoint: ["Dot", { radius: 2}],
            HoverPaintStyle: hoverStyle,
            Container: $("#" + this._areaDivId),
            Connector: ["StateMachine", { curviness: 20}],
            ConnectionOverlays: overlay
        });

        jsPlumb.registerConnectionTypes({
            "primary": { paintStyle: this._defaultConnectorStyle, overlays: overlay, hoverPaintStyle: hoverStyle },
            "conditional": { paintStyle: this._conditionalConnectorStyle, overlays: overlay, hoverPaintStyle: hoverStyle },
            "rework": { paintStyle: this._reworkConnectorStyle, overlays: overlay, hoverPaintStyle: hoverStyle },
            "selected": { paintStyle: this._selectedConnectorStyle, overlays: overlay }
        });

        jsPlumb.setContainer($("#" + this._areaDivId));
    },

    build: function()
    {
        CamstarPortal.WebControls.WorkflowCanvas.callBaseMethod(this, 'build');
        var me = this;

        var $canvas = $("#" + this._clientId);
        if (Camstars.Browser.IE)
            $canvas.addClass("ie");

        if ($canvas.closest("body.body-modeling").length == 0) {
            // Adjust size to flyout
            var $flyoutContainer = $canvas.closest('div.ui-flyout-container');
            if ($flyoutContainer.length || $canvas.closest("body.frame-modeling").length != 0) {
                var $panel = $(".ui-flyout-panel", $flyoutContainer);
                if ($panel.length > 0) {
                    this.resize($panel, $canvas);
                    $flyoutContainer.bind('resized', function () {
                        me.resize($panel, $canvas);
                    });
                }
            }
            else {
                // Adjust to slideout panel
                var $sp = $canvas.closest('#scrollablepanel');
                if ($sp.length) {
                    this.resize($sp, $canvas, true);
                    $sp.unbind('panelResized').bind('panelResized', function () {
                        me.resize($sp, $canvas, true);
                    });
                    document.addEventListener('panelResized', function () {
                        me.resize($sp, $canvas, true);
                    });
                    var rszTime = null;
                    $(window).bind('resize', function () {
                        if (rszTime) clearTimeout(rszTime);
                        rszTime = setTimeout(me.resize($sp, $canvas, true), 250);
                    }).trigger('resize');
                }
            }
        }
    },

    resize: function ($panel, $canvas, isPanel)
    {
        var canvasHeight = $panel.height() - ($canvas.offset().top - $panel.offset().top) - 2;
        if (canvasHeight > 0)
            $canvas.height(canvasHeight);

        if (isPanel === true) {
            $canvas.height(canvasHeight-15);
            var canvasWidth = $panel.width() - ($canvas.offset().left - $panel.offset().left) - 4;
            if (canvasWidth > 0)
                $canvas.width(canvasWidth);
        }
    },

    executeCanvasCommand: function (commandData, canvas)
    {
        var parameters = commandData.Parameters;
        if (commandData.CommandName == "Connect")
        {
            var sourceDiv = $("#" + parameters.From, canvas);
            var targetDiv = $("#" + parameters.To, canvas);
            var connectionType = this.Connections.PRIMARY.name;
            if (parameters.Connection === this.Connections.ALTERNATE.value)
                connectionType = this.Connections.ALTERNATE.name;
            else if (parameters.Connection === this.Connections.REWORK.value)
                connectionType = this.Connections.REWORK.name;

            var conn = jsPlumb.connect({
                source: sourceDiv,
                target: targetDiv,
                type: connectionType,
                anchor:"AutoDefault",
                parameters: { "defaultType": connectionType }
            });
            if (!this._readOnly)
            {
                var me = this;
                conn.bind('click', function (el, e)
                {
                    me.connectionSelected(conn);
                    e.cancelBubble = true; //IE
                    if (e.stopPropagation)
                        e.stopPropagation(); //other browsers
                });
            }
        }
        else if (commandData.CommandName == "Zoom")
        {
            this.zoomElements(parameters.X, parameters.Y);
        }
        else if (commandData.CommandName == "CurrentStep")
        {
            var step = $("#" + parameters);
            if (step.length > 0)
            {
                step.addClass('selected');
                var canvasControl = $("#" + this._clientId);
                if (canvasControl.length > 0)
                {
                    canvasControl.scrollTop(step[0].offsetTop);
                    canvasControl.scrollLeft(step[0].offsetLeft);
                }
            }
        }
    },

    doubleRework: function (connectionInfo) // call only when the connection is being added.
    {
        var droppedConnectionType = this.getConnectionType(connectionInfo.connection.getParameter("defaultType"));
        var retVal = false;
        var sourceId, targetId;
        var fromConnections;
        var me = this;

        if (droppedConnectionType == this.Connections.REWORK.value)
        {
            if (!this._disableReworkPath)
            {
                sourceId = connectionInfo.sourceId;
                targetId = connectionInfo.targetId;
                fromConnections = jsPlumb.getConnections({ source: sourceId });
                $.each(fromConnections, function (i, val)
                {
                    var connectionType = val.getParameter("defaultType");
                    if (connectionType == me.Connections.REWORK.name && sourceId == val.sourceId && targetId == val.targetId)
                    {
                        retVal = true;
                        return false;
                    }
                });
            }
        }

        if (droppedConnectionType == this.Connections.PRIMARY.value)
        {
            sourceId = connectionInfo.sourceId;
            fromConnections = jsPlumb.getConnections({ source: sourceId });
            $.each(fromConnections, function (i, val)
            {
                var connectionType = val.getParameter("defaultType");
                if (connectionType == me.Connections.PRIMARY.name)
                {
                    retVal = true;
                    return false;
                }
            });
        }

        return retVal;
    },

    checkConnection: function (connectionInfo) // redraws Default to Alternate and back when necessary.
    {
        var sourceId = connectionInfo.sourceId;

        var fromConnections = jsPlumb.getConnections({ source: sourceId });
        var hasDefault = false;
        var hasAlternate = false;

        var me = this;
        $.each(fromConnections, function (i, val)
        {
            if ($(val).attr("id") != $(connectionInfo.connection).attr("id"))
            {
                var connectionType = me.getConnectionType(val.getParameter("defaultType"));
                if (connectionType == me.Connections.PRIMARY.value)
                    hasDefault = true;
                else if (connectionType == me.Connections.ALTERNATE.value)
                    hasAlternate = true;    
            }
        });

        var droppedConnectionType = this.getConnectionType(connectionInfo.connection.getParameter("defaultType"));
        var con = connectionInfo.connection;
        if (droppedConnectionType == this.Connections.PRIMARY.value)
        {
            if (hasDefault)
            {
                // mark the connection as alternate.
                con.setType(this.Connections.ALTERNATE.name);
                con.setParameter("defaultType", this.Connections.ALTERNATE.name);
                con.repaint();
            }
        }
        else if (droppedConnectionType == this.Connections.ALTERNATE.value)
        {
            if (!hasDefault)
            {
                // mark the connection as default.
                con.setType(this.Connections.PRIMARY.name);
                con.setParameter("defaultType", this.Connections.PRIMARY.name);
                con.repaint();
            }
        }
    },

    canvasLoaded: function ()
    {
        var me = this;
        if (!this._readOnly)
        {
            jsPlumb.bind("beforeDrop", function (info)
            {
                var newConnectionAdded = true;
                var connections = jsPlumb.getConnections({ source: info.sourceId, target: info.targetId });
                if (connections.length > 0)
                    newConnectionAdded = false; // from & to should be unique for path.
                return newConnectionAdded && !me.doubleRework(info);
            });
            jsPlumb.bind("connection", function (info)
            {
                me.newConnectionAdded(info);
            });
        }
    },

    createElementDiv: function (info)
    {
        var imagesFolder = "Images/Workflow/";
        var stepImagePath = "WFSpecStep";
        var divCss = 'step';
        if (info.IsSubWorkflow)
        {
            divCss += ' subFlow';
            stepImagePath = "WFSubWorkflowStep";
        }

        if (info.IsFirstStep)
        {
            if (info.IsSubWorkflow)
            {
                divCss += ' subFlowFirst';
                stepImagePath = "WFStartSubWorkflowStep";
            }
            else
            {
                divCss += ' first';
                stepImagePath = "WFStartSpecStep";
            }
        }
        var extension = ".svg";

        var newElement = $('<' + info.TagName + '>').attr('id', info.Id).addClass(divCss);
        var position = $.parseJSON('[' + info.Position + ']');
        newElement.css({ left: position[0], top: position[1] });
        newElement.attr('title', info.Description);
        var newElementHtml = "<img src='" + imagesFolder + stepImagePath + extension + "' />";

        var me = this;
        if (!this._readOnly)
        {
            newElement.bind('click', function (e)
            {
                var target = $(e.target).parent();
                if (target.attr('dropped')) // element was dropped.
                    target.removeAttr('dropped');
                else 
                {
                    if ($(target).hasClass('step'))
                        me.elementSelected(target);
                }
                e.cancelBubble = true; //IE
                if (e.stopPropagation)
                    e.stopPropagation(); //other browsers
            });
            var connectHtml = "<div class=\"con1\">" + "<img src=\"" + imagesFolder + "Primary" + extension + "\" /></div>";
            var connect2Html = "<div class=\"con2\">" + "<img src=\"" + imagesFolder + "Alternate" + extension + "\" /></div>";
            var connect3Html = "<div class=\"con3\">" + "<img src=\"" + imagesFolder + "Rework" + extension + "\" /></div>";
            newElementHtml += connectHtml + connect2Html;
            if (!this._disableReworkPath)
                newElementHtml += connect3Html;
        }

        newElementHtml += '<span class="Title">' + info.Title + '</span>';
        newElement.html(newElementHtml);
        
        return newElement[0];
    },

    newCanvasElementAdded: function (newElement) {
        if (!this._readOnly) {

            var dropOptionsCss = {
                tolerance: "touch",
                hoverClass: "dropHover",
                activeClass: "dragActive"
            };

            jsPlumb.makeTarget(newElement, {
                anchor: 'Continuous',
                dropOptions: { hoverClass: "step-dragHover" }
            });

            // create default connector.
            jsPlumb.makeSource(newElement, {
                filter: ".con1 > img",
                anchor: 'Continuous',
                connectorStyle: this._defaultConnectorStyle,
                connectionType: this.Connections.PRIMARY.name,
                detachable: false,
                parameters: { "defaultType": this.Connections.PRIMARY.name }
            });

            // create conditional connector.
            jsPlumb.makeSource(newElement, {
                filter: ".con2 > img",
                anchor: 'Continuous',
                connectorStyle: this._conditionalConnectorStyle,
                connectionType: this.Connections.ALTERNATE.name,
                detachable: false,
                parameters: { "defaultType": this.Connections.ALTERNATE.name }
            });

            if (!this._disableReworkPath) {
                // create rework connector.
                jsPlumb.makeSource(newElement, {
                    filter: ".con3 > img",
                    anchor: 'Continuous',
                    connectorStyle: this._reworkConnectorStyle,
                    connectionType: this.Connections.REWORK.name,
                    detachable: false,
                    parameters: { "defaultType": this.Connections.REWORK.name }
                });
            }
        }
    },

    newElementAdded: function (ui)
    {
        var canvas = $("#" + this._areaDivId);
        var li = ui.draggable;
        var name = '';
        var rev = '';
        var isSpec = true;
        if (li.length > 0) // dropped from PickListPanel.
        {
            var val = { key: li.attr('key'), tag: li.attr('tag') };
            if (val.tag != null)
            {
                var encodedValue = val.tag;
                var nameRevRor = eval('(' + decodeURIComponent(encodedValue) + ')');
                name = nameRevRor.Name;
                rev = nameRevRor.Revision;
            }
            else
                name = val.key;
            if (li.parents('span').length > 0)
                isSpec = li.parents('span').attr('id').toLowerCase().indexOf('workflows') < 0;
        }
        
        var left = ui.offset.left - canvas.offset().left;
        var top = ui.offset.top - canvas.offset().top;

        if (canvas.scrollLeft() > 0)
            left = left + canvas.scrollLeft();
        if (canvas.scrollTop() > 0)
            top = top + canvas.scrollTop();

        left = Math.max(0, left);
        top = Math.max(0, top);

        var position = { left: left, top:  top};        
        var callParameters =
        {
            "X": Math.round(position.left),
            "Y": Math.round(position.top),
            "Data": { "Name": name, "Rev": rev, "IsSpec": isSpec }
        };
        this._sendRequest(callParameters, "NewElementAdded", "newElementAddedResponse");
        this.incrementInProcFuncCounter();
        setDirtyFlag();
    },

    subworkflowPageRedirect: function ()
    {
        var canvas = $("#" + this._areaDivId);
        var selected = $('div.selected', canvas);
        if (selected.length > 0)
        {
            var callParameters =
            {
                "CanvasElementId": selected.attr('id')
            };
            this._sendRequest(callParameters, "EnsureSubworkflowDetails", "subworkflowPageRedirecting");
            this.incrementInProcFuncCounter();
        }
    },

    subworkflowPageRedirecting: function (responseData)
    {
        if (responseData)
        {
            var name = responseData.InstanceName;
            var revision = responseData.InstanceRev;
            if (name)
            {
                if (this._modelingPageQuery !== null && this._modelingPageName !== null)
                {
                    var maintQuery = this._modelingPageQuery + "&instName=" + encodeURIComponent(name).replace(/%20/g, "+");
                    if (revision)
                        maintQuery += "&instRev=" + encodeURIComponent(revision).replace(/%20/g, "+");
                    __page.openInNewTab(this._modelingPageName, maintQuery, this._modelingPageTitle, null, null);
                }
            }
        }
    },

    newConnectionAdded: function (conn)
    {
        var me = this;
        var connection = conn;
        if (conn.connection)
            connection = conn.connection;

        connection.bind('click', function (el, e)
        {
            me.connectionSelected(connection);
            e.cancelBubble = true; //IE
            if (e.stopPropagation)
                e.stopPropagation(); //other browsers
        });
        this.checkConnection(conn);
        this._sendRequest(this.getConnectionCallParameters(connection), "NewConnectionAdded", "dummyEvent");
        this.incrementInProcFuncCounter();
        setDirtyFlag();
    },

    viewSubworkflowDetails: function (pageName, query, caption)
    {
        if (this._modelingPageQuery === null)
            this._modelingPageQuery = query;
        if (this._modelingPageName === null)
            this._modelingPageName = pageName;
        if (this._modelingPageTitle === null)
            this._modelingPageTitle = caption;

        this.subworkflowPageRedirect();
    },

    deleteSelectedElement: function ()
    {
        var canvas = $("#" + this._areaDivId);
        var selected = $('div.selected', canvas);
        var me = this;
        if (selected.length > 0) // remove selected element
        {
            var callParameters =
            {
                "CanvasElementId": selected.attr('id')
            };
            this._sendRequest(callParameters, "DeleteElement", "dummyEvent");
            this.incrementInProcFuncCounter();
            jsPlumb.deleteConnectionsForElement(selected);
            selected.remove();
        }
        else // remove selected connection
        {
            var connections = jsPlumb.getConnections();
            var selectedConnection = null;
            $.each(connections, function (i, val)
            {
                if (val.getType() == me.Connections.SELECTED.name)
                {
                    selectedConnection = val;
                    return false;
                }
            });
            if (selectedConnection != null)
            {
                this._sendRequest(this.getConnectionCallParameters(selectedConnection), "DeleteConnection", "dummyEvent");
                this.incrementInProcFuncCounter();
                jsPlumb.deleteConnection(selectedConnection);
            }
        }
        this.enableSettingsButton(false);
    },

    elementSelected: function (el)
    {
        this.unselectAllElements();
        if (el)
        {
            var callParameters = { "Data": { "Name": el.attr("id")} };
            this._sendRequest(callParameters, "ElementSelected");
            this.incrementInProcFuncCounter();
            el.addClass('selected');
            this.enableSettingsButton(true, el);
        }
    },

    connectionSelected: function (conn)
    {
        this.unselectAllElements();
        this.enableSettingsButton(true);
        conn.setType(this.Connections.SELECTED.name);
        var commandData = this.getConnectionCallParameters(conn);
        this._sendRequest(commandData, "PathSelected");
        this.incrementInProcFuncCounter();
    },

    unselectAllElements: function ()
    {
        var canvas = $("#" + this._areaDivId);
        if ($('div.selected', canvas).length > 0) // unselect step.
        {
            $('div.selected', canvas).removeClass('selected');
        }
        else // unselect connection.
        {
            var connections = jsPlumb.getConnections();
            var me = this;
            $.each(connections, function (i, val)
            {
                if (val.getType() == me.Connections.SELECTED.name)
                {
                    val.setType(val.getParameter("defaultType"));
                    val.repaint();
                    return false;
                }
            });
        }
        this.enableSettingsButton(false);
    },

    enableSettingsButton: function (isEnabled, selectedEl)
    {
        var toolbar = $('table.workflowToolBar');
        if (toolbar.length > 0)
        {
            var settingsButton = $('.SettingsButton', toolbar);
            if (settingsButton)
            {
                if (isEnabled)
                {
                    settingsButton.css('visibility', 'visible');
                }
                else
                {
                    settingsButton.css('visibility', 'hidden');
                }
            }

            var viewSubworkflowButton = $(".ViewSubworkflowDetails", toolbar);
            if (viewSubworkflowButton.length)
            {
                if (isEnabled && selectedEl && selectedEl.hasClass("subFlow"))
                    viewSubworkflowButton.css("visibility", "visible");
                else
                    viewSubworkflowButton.css("visibility", "hidden");
            }
        }
    },

    getConnectionCallParameters: function (conn)
    {
        var fromId = conn.sourceId;
        var toId = conn.targetId;
        var connectionType = this.getConnectionType(conn.getParameter("defaultType"));
        var callParameters =
        {
            "FromStep": fromId,
            "ToStep": toId,
            "ConnectionType": connectionType
        };
        return callParameters;
    },

    getConnectionType: function (type)
    {
        var connectionType = this.Connections.PRIMARY.value;
        if (type == this.Connections.ALTERNATE.name)
            connectionType = this.Connections.ALTERNATE.value;
        else if (type == this.Connections.REWORK.name)
            connectionType = this.Connections.REWORK.value;
        return connectionType;
    },

    zoomStep: function (step, x, y)
    {
        step.width(step.width() * x);
        step.height(step.height() * y);
        if (!this._readOnly)
        {
            var connectDiametr = Math.round((step.height() - 2) / 3);
            var connect1 = $('div.con1', step);
            connect1.width(connectDiametr);
            connect1.height(connectDiametr);
            connect1.css({ left: -connectDiametr + 10 });
            var connect2 = $('div.con2', step);
            connect2.width(connectDiametr);
            connect2.height(connectDiametr);
            connect2.css({ left: -connectDiametr + 10, top: connectDiametr + 1 });
            var connect3 = $('div.con3', step);
            connect3.width(connectDiametr);
            connect3.height(connectDiametr);
            connect3.css({ left: -connectDiametr + 10, top: 2 * connectDiametr + 2 });
        }
    },

    leftBorderOffset: function ()
    {
        return 10;
    },

    rightBorderOffset: function ()
    {
        return 10;
    },

    get_disableReworkPath: function () { return this._disableReworkPath; },
    set_disableReworkPath: function (value) { this._disableReworkPath = value; },
},

CamstarPortal.WebControls.WorkflowCanvas.registerClass('CamstarPortal.WebControls.WorkflowCanvas', CamstarPortal.WebControls.Canvas);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
