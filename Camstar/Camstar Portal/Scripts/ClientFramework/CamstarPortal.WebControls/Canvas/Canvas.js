// Copyright Siemens 2023  

/// <reference path="../../MicrosoftAjaxExt.js"/>
/// <reference path="../../Camstar.UI/Control.js" />
/// <reference path="../../../jsPlumb/jsplumb.js" />
Type.registerNamespace("CamstarPortal.WebControls");

/******************* CamstarPortal.WebControls.CollapsableSection *******************/
CamstarPortal.WebControls.Canvas = function (element) {
    CamstarPortal.WebControls.Canvas.initializeBase(this, [element]);
    this._elements = null;
    this._commands = null;
    this._callStackKey = null;
    this._controlId = null;
    this._clientId = null;
    this._areaDivId = null;
    this._actionsDivId = null;
    this._readOnly = null;
    this._serverType = "CamstarPortal.WebControls.Canvas, CamstarPortal.WebControls";
    this._inProcFuncCounter = 0;//used to count & finish invocation of all the functions before page refresh is triggered (via "Save" for example).
},

    CamstarPortal.WebControls.Canvas.prototype =
    {
        initialize: function () {
            CamstarPortal.WebControls.Canvas.callBaseMethod(this, 'initialize');
            this._zoom = 1;
            var me = this;

            $(function () {
                setTimeout(function () {
                    me.build();
                }, 0);
            });
        },

        dispose: function () {
            this._elements = null;
            this._commands = null;
            this._callStackKey = null;
            this._controlId = null;
            this._serverType = null;
            this._areaDivId = null;
            this._actionsDivId = null;
            this._clientId = null;
            this._readOnly = null;
            jsPlumb.reset();

            CamstarPortal.WebControls.Canvas.callBaseMethod(this, 'dispose');
        },

        build: function () {
            var me = this;

            //resize canvas
            var $canvas = $("#" + me._clientId);

            var $canvasControl = $("#ctl00_WebPartManager_WorkflowDiagramGroupWP_CanvasControl_Canvas");
            var $popCanvasControl = $("#ctl00_WebPartManager_WorkflowCanvas_WP_CanvasControl_Canvas");

            if ($canvasControl[0] !== undefined) {
                if ($canvas[0].id == $canvasControl[0].id) {
                    $canvas[0].style.height = window.innerHeight * 0.8 + 'px';
                }
            }
            if ($popCanvasControl[0] !== undefined) {
                if ($canvas[0].id == $popCanvasControl[0].id) {
                    $canvas[0].style.height = window.innerHeight * 0.9 + 'px';
                }
            }

            var $SpecsControl = $("#ctl00_WebPartManager_WorkflowDiagramGroupWP_SpecsControl");
            var $WorkflowsControl = $("#ctl00_WebPartManager_WorkflowDiagramGroupWP_WorkflowsControl");
            var $popSpecsControl = $("#ctl00_WebPartManager_WorkflowCanvas_WP_SpecsControl");
            var $popWorkflowsControl = $("#ctl00_WebPartManager_WorkflowCanvas_WP_WorkflowsControl");

            if ($SpecsControl[0] !== undefined)
                $SpecsControl[0].style.height = ((window.innerHeight * 0.8) / 2) + 10 + 'px';
            if ($WorkflowsControl[0] !== undefined)
                $WorkflowsControl[0].style.height = ((window.innerHeight * 0.8) / 2) - 10 + 'px';

            if ($popSpecsControl[0] !== undefined)
                $popSpecsControl[0].style.height = ((window.innerHeight * 0.9) / 2) + 10 + 'px';
            if ($popWorkflowsControl[0] !== undefined)
                $popWorkflowsControl[0].style.height = ((window.innerHeight * 0.9) / 2) - 10 + 'px';

            var $specsPanel = $("#ctl00_WebPartManager_WorkflowDiagramGroupWP_SpecsControl_Panl");
            var $specsViewer = $(".viewer", $specsPanel);
            var $workflowsPanel = $("#ctl00_WebPartManager_WorkflowDiagramGroupWP_WorkflowsControl_Panl");
            var $workflowsViewer = $(".viewer", $workflowsPanel);

            if ($specsPanel[0] !== undefined)
                $specsViewer[0].style.height = ((window.innerHeight * 0.8) / 2) * 0.7 + 'px';
            if ($workflowsPanel[0] !== undefined)
                $workflowsViewer[0].style.height = ((window.innerHeight * 0.8) / 2) * 0.7 + 'px';

            var $popSpecsPanel = $("#ctl00_WebPartManager_WorkflowCanvas_WP_SpecsControl_Panl");
            var $popSpecsViewer = $(".viewer", $popSpecsPanel);
            var $popWorkflowsPanel = $("#ctl00_WebPartManager_WorkflowCanvas_WP_WorkflowsControl_Panl");
            var $popWorkflowsViewer = $(".viewer", $popWorkflowsPanel);

            if ($popSpecsPanel[0] !== undefined)
                $popSpecsViewer[0].style.height = ((window.innerHeight * 0.9) / 2) * 0.7 + 'px';
            if ($popWorkflowsPanel[0] !== undefined)
                $popWorkflowsViewer[0].style.height = ((window.innerHeight * 0.9) / 2) * 0.7 + 'px';

            //build content
            var canvas = $("#" + me._areaDivId);
            var toggleContainerContent = canvas.parents('div.content');
            if (toggleContainerContent.length > 0 && toggleContainerContent.is(':hidden')) {
                // need to repaint canvas when accordion section will be expanded.
                toggleContainerContent.bind('toggle', function () {
                    $(this).unbind();
                    var elements = $('div.step', $("#" + me._areaDivId));
                    $.each(elements, function (i, val) {
                        jsPlumb.revalidate(val);
                    });
                });
            }

            if (!this._readOnly) {
                canvas.bind('click', function (e) {
                    me.unselectAllElements();
                    me._sendRequest({}, "ResetSelection");
                    me.incrementInProcFuncCounter();
                });
                this.enableSettingsButton(false);
            }

            $(canvas).droppable({
                tolerance: "pointer",
                drop: function (event, ui) {
                    if (!ui.draggable.hasClass("step")) {
                        me.newElementAdded(ui);
                    }
                }
            });

            jsPlumb.ready(function () {

                me.jsPlumbIni();
                jsPlumb.setSuspendDrawing(true);
                var elements = me._elements;
                if (elements) {
                    var jsonElements = eval('(' + elements + ')'); // converts string to JSON array.
                    $.each(jsonElements, function (i, val) {
                        me.addCanvasElement(val, canvas);
                    }
                    );
                }

                var commands = me._commands;
                if (commands) {
                    var jsonCommands = eval('(' + commands + ')'); // converts string to JSON array.
                    $.each(jsonCommands, function (i, val) {
                        me.executeCanvasCommand(val, canvas);
                    }
                    );
                }

                jsPlumb.setSuspendDrawing(false, true);
                me.canvasLoaded();
            }
            );
        },

        jsPlumbIni: function () {
            jsPlumb.setContainer($("#" + this._areaDivId));
        },

        canvasLoaded: function () {

        },

        addCanvasElement: function (elementData, canvas) {
            var newElementDiv = this.createElementDiv(elementData);
            if (!canvas)
                canvas = $("#" + this._areaDivId);
            canvas.append(newElementDiv);
            this.makeElementDraggable(newElementDiv);
            this.newCanvasElementAdded(newElementDiv);

            return newElementDiv;
        },

        executeCanvasCommand: function (commandData, canvas) {
            var parameters = commandData.Parameters;
            if (commandData.CommandName === "Zoom") {
                this.zoomElements(parameters.X, parameters.Y);
            }
        },

        newCanvasElementAdded: function (div) {

        },

        newElementAdded: function (ui) {
            var canvas = $("#" + this._areaDivId);
            var title = "New Element";
            var position = { left: parseInt((ui.offset.left - canvas.offset().left) / this._zoom), top: parseInt((ui.offset.top - canvas.offset().top) / this._zoom) };
            var callParameters =
            {
                "X": Math.round(position.left),
                "Y": Math.round(position.top),
                "Title": title
            };
            this._sendRequest(callParameters, "NewElementAdded", "newElementAddedResponse");
            this.incrementInProcFuncCounter();
        },

        /* ZOOM */
        zoom: function (x, y) {
            this.zoomElements(x, y);
            this.zoomCanvas(x, y);
            this._zoom = x;
        },

        zoomCanvas: function (x, y) {
            var canvas = $("#" + this._areaDivId);
            var area = $("#" + this._clientId);
            var canvasWidth = area.width();
            if (x > 1)
                canvasWidth = Math.floor(canvasWidth * x);
            var canvasHeight = area.height();
            if (y > 1)
                canvasHeight = Math.floor(canvasHeight * x);
            canvas.css({ width: canvasWidth, height: canvasHeight });
        },

        zoomElements: function (x, y) {
            x = x / this._zoom;
            y = y / this._zoom;

            var canvas = $("#" + this._areaDivId);
            var elements = $('div.step', canvas);
            var me = this;
            $.each(elements, function (i, val) {
                var xPos = parseInt($(val).css("left"));
                var yPos = parseInt($(val).css("top"));

                $(val).css({ left: xPos * x });
                $(val).css({ top: yPos * y });
                me.zoomStep($(val), x, y);
                $(val).position().left = parseInt($(val).css("left"));
                $(val).position().top = parseInt($(val).css("top"));
                jsPlumb.revalidate(val);
            });
        },

        zoomStep: function (step, x, y) {
            step.width(step.width() * x);
            step.height(step.height() * y);
        },

        zoomFromToolbar: function (toolBarSelection) {
            var zoom = ($(toolBarSelection).val() / 100);
            this.zoom(zoom, zoom);
        },

        shrinkToFit: function (x, y) {
            this.zoom(x, y);
            this._zoom = 1;
        },

        /* END ZOOM */

        fit: function () {
            var canvas = $("#" + this._areaDivId);
            var elements = $('div.step', canvas);
            var me = this;
            $.each(elements, function (i, val) {
                var stepWidth = $(val).width() + 20 * me._zoom; // including 10 left/right margin.
                var stepHeight = $(val).height() + 20 * me._zoom; // including 10 top/bottom margin.

                var xPos = parseInt($(val).css("left"));
                var yPos = parseInt($(val).css("top"));

                var xCellIndex = Math.round(xPos / stepWidth);
                var yCellIndex = Math.round(yPos / stepHeight);

                $(val).css({ left: xCellIndex * stepWidth + 10 * me._zoom });
                $(val).css({ top: yCellIndex * stepHeight + 10 * me._zoom });
                $(val).position().left = parseInt($(val).css("left"));
                $(val).position().top = parseInt($(val).css("top"));
                jsPlumb.revalidate(val);
            });

            if (!me._readOnly) {
                var xList = "";
                var yList = "";
                var idList = "";
                $.each(elements, function (i, val) {
                    var x = parseInt(parseInt($(val).css("left")) / me._zoom);
                    var y = parseInt(parseInt($(val).css("top")) / me._zoom);
                    var id = $(val).attr('id');

                    xList += ";" + x;
                    yList += ";" + y;
                    idList += ";" + id;
                });

                var callParameters =
                {
                    "CanvasElementId": idList,
                    "X": xList,
                    "Y": yList
                };
                me._sendRequest(callParameters, "ElementsDragged", "dummyEvent");
                me.incrementInProcFuncCounter();
            }
        },

        makeElementDraggable: function (newElementDiv) {
            var me = this;
            jsPlumb.draggable(newElementDiv, {
                containtment: 'parent',

                stop: function (e) // stop dragging element event catched.
                {
                    var target = $(e.el);
                    target.attr('dropped', 'true');
                    if (me.correctPositionAfterDragging($(e.el)))
                        jsPlumb.revalidate(e.el);
                    if (!me._readOnly) {
                        var xPos = parseInt(parseInt(target.css("left")) / me._zoom);
                        var yPos = parseInt(parseInt(target.css("top")) / me._zoom);

                        var callParameters =
                        {
                            "CanvasElementId": target.attr('id'),
                            "X": xPos,
                            "Y": yPos
                        };
                        me._sendRequest(callParameters, "ElementDragged", "dummyEvent");
                        me.incrementInProcFuncCounter();
                    }
                },
            });
        },

        createElementDiv: function (info) {
            var me = this;
            var newElement = $('<' + info.TagName + '>').attr('id', info.Id).addClass('step');
            newElement.html(info.Title);
            var position = $.parseJSON('[' + info.Position + ']');
            newElement.css({ left: position[0], top: position[1] });
            newElement.bind('click', function (e) {
                var target = $(e.el);
                if (target.attr('dropped')) // element was dropped.
                {
                    target.removeAttr('dropped');
                }
                else {
                    me.elementSelected(target);
                }
                e.cancelBubble = true; //IE
                if (e.stopPropagation)
                    e.stopPropagation(); //other browsers
            });
            return newElement[0];
        },

        correctPositionAfterDragging: function (element) // when dropped outside the canvas.
        {
            var needRepaint = false;
            var canvas = $("#" + this._areaDivId);
            var xPos = parseInt(element.css("left"));
            var yPos = parseInt(element.css("top"));

            if (xPos + element.outerWidth(true) > canvas.width() - this.leftBorderOffset()) // out of the right border.
            {
                needRepaint = true;
            }
            if (xPos < this.leftBorderOffset()) // out of the left border.
            {
                element.css({ left: this.leftBorderOffset() });
                needRepaint = true;
            }
            if (yPos + element.outerHeight(true) > canvas.height() - this.rightBorderOffset()) // out of the bottom border.
            {
                needRepaint = true;
            }
            if (yPos < this.rightBorderOffset()) // out of the top border.
            {
                element.css({ top: this.rightBorderOffset() });
                needRepaint = true;
            }

            return needRepaint;
        },

        deleteSelectedElement: function () {
            var canvas = $("#" + this._areaDivId);
            var selected = $('div.selected', canvas);
            if (selected.length > 0) {
                var callParameters =
                {
                    "CanvasElementId": selected.attr('id')
                };
                this._sendRequest(callParameters, "DeleteElement", "dummyEvent");
                this.incrementInProcFuncCounter();
                selected.remove();
            }
        },

        elementSelected: function (el) {
            this.unselectAllElements();
            el.addClass('selected');
            this.enableSettingsButton(true);
        },

        unselectAllElements: function () {
            var canvas = $("#" + this._areaDivId);
            $('div.selected', canvas).removeClass('selected');
            this.enableSettingsButton(false);
        },

        enableSettingsButton: function (isEnabled) {

        },

        _sendRequest: function (callParameters, serverAction, callBackMethod, returnVal, trans_id) {
            var self = this;
            var recallSendRequest = function () {
                self._sendRequest(callParameters, serverAction, callBackMethod, returnVal, trans_id);
            };
            if (!__page._lock) {
                callParameters.Action = serverAction;
                //callParameters.trans_id = trans_id;
                callParameters.CallStackKey = getParameterByName("CallStackKey");
                callParameters.UniqueId = this._controlId;
                callParameters.ClientId = this._clientId;
                callParameters.CallBackMethod = callBackMethod;

                var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
                transition.set_command("ClientEntry");

                var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
                transition.set_commandParameters(callParamsString);
                transition.set_clientCallback(callBackMethod);
                transition.set_noModalImage(true);
                var communicator = new Camstar.Ajax.Communicator(transition, this);
                communicator.syncCall();
            }
            else {
                window.setTimeout(recallSendRequest, 100);
            }
            return returnVal;
        },

        processStatusData: function (statusData) {
            //the counter's value is examined in WaitForGridOpCompletionOnSave() to wait for all the functions to be completed before saving
            this.decrementInProcFuncCounter();
        },

        directUpdate: function (directUpdateData) {
            CamstarPortal.WebControls.Canvas.callBaseMethod(this, 'directUpdate');
            var directUpdateResponsedata = eval(directUpdateData.PropertyValue);
            if (directUpdateResponsedata) {
                if (directUpdateResponsedata.CallBackMethod) {
                    var callBackFun = this[directUpdateResponsedata.CallBackMethod];
                    if (callBackFun != undefined)
                        callBackFun.call(this, directUpdateResponsedata.ResponseData, directUpdateResponsedata.Action);
                    else if (directUpdateResponsedata.CallBackMethod != "dummyEvent")
                        this.showMessage("callback function " + directUpdateResponsedata.CallBackMethod + " is not found in the Canvas");
                }
                if (directUpdateResponsedata.ResponseStatus != "ok")
                    this.showMessage(directUpdateResponsedata.ResponseStatus);
            }
            this.decrementInProcFuncCounter();
        },

        dummyEvent: function () {

        },

        newElementAddedResponse: function (responseData, action) {
            var addedElement = this.addCanvasElement(responseData, null);
            if (this._zoom != 1) {
                this.zoomStep($(addedElement), this._zoom, this._zoom);
            }
        },

        leftBorderOffset: function () {
            return 0;
        },

        rightBorderOffset: function () {
            return 0;
        },
        incrementInProcFuncCounter: function () {
            this._inProcFuncCounter++;
        },
        decrementInProcFuncCounter: function () {
            this._inProcFuncCounter--;
        },

        get_elements: function () { return this._elements; },
        set_elements: function (value) { this._elements = value; },

        get_commands: function () { return this._commands; },
        set_commands: function (value) { this._commands = value; },

        get_callStackKey: function () { return this._callStackKey; },
        set_callStackKey: function (value) { this._callStackKey = value; },

        get_controlId: function () { return this._controlId; },
        set_controlId: function (value) { this._controlId = value; },

        get_clientId: function () { return this._clientId; },
        set_clientId: function (value) { this._clientId = value; },

        get_actionsDivId: function () { return this._actionsDivId; },
        set_actionsDivId: function (value) { this._actionsDivId = value; },

        get_areaDivId: function () { return this._areaDivId; },
        set_areaDivId: function (value) { this._areaDivId = value; },

        get_readOnly: function () { return this._readOnly; },
        set_readOnly: function (value) { this._readOnly = value; },

        get_serverType: function () { return this._serverType; },

        get_inProcFuncCounter: function () { return this._inProcFuncCounter; }
    },

    CamstarPortal.WebControls.Canvas.registerClass('CamstarPortal.WebControls.Canvas', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
