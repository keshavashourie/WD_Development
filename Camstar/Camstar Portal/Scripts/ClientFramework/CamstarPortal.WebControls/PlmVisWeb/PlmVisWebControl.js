// Copyright Siemens 2023  

Type.registerNamespace("CamstarPortal.WebControls");

// QueryString Parameters:
//      name = The name of the document to load
//      revision = The revision of the document to load
CamstarPortal.WebControls.PlmVisWebControl = function(element) {
    CamstarPortal.WebControls.PlmVisWebControl.initializeBase(this, [element]);

    this.viewerManager = null;
    this.controlManager = null;
    this.painting = false;
    this.textTyping = false;

    this.cmd = null;
    this.isDirty = null;
    this.canvasOffsetXY = null;
    this.canvasSize = null;
    this.currentModelName = null;
    this._modelDocName = null;
    this._modelDocRev = null;
    this.startX = null;
    this.startY = null;
    this.lastX = null;
    this.lastY = null;

    this._serverType = "CamstarPortal.WebControls.PlmVisWebControl, CamstarPortal.WebControls";
    this.isResponsive = false;
},

CamstarPortal.WebControls.PlmVisWebControl.prototype =
    {
        initialize: function() {
            CamstarPortal.WebControls.PlmVisWebControl.callBaseMethod(this, "initialize");

            var me = this;
            this.isDirty = false;

            $("span[cmd]", $(this._element)).click(function() {
                return me.btnCommandClick(this);
            });

            this._canvasElement = this.getSnapshotCanvas()[0];
            this._canvasLayerElement = this.getCanvasLayer()[0];

            $([this._canvasElement, this._canvasLayerElement]).mousedown(function(e) {
                me.canvasMouseDown(e);
            });

            $([this._canvasElement, this._canvasLayerElement]).mousemove(function(e) {
                me.canvasMouseMove(e);
            });

            $([this._canvasElement, this._canvasLayerElement]).mouseup(function(e) {
                me.paintingStarted = false;
                me.layerCanvasStarted = false;
            });

            $([this._canvasElement, this._canvasLayerElement]).mouseleave(function(e) {
                me.paintingStarted = false;
            });

            var colorPickerControl = this.getColorPickerBtn();
            colorPickerControl.colorpicker({ color: "#000000", inline: true });
            colorPickerControl.on("change.color", function(e, c) { return me.colorChanged(e, c) });

            var $selectLineWidth = $("span.btnLineWeight > select", $(this._element));
            $selectLineWidth.change(function() {
                me.lineWidthChanged($(this).val());
            });

            this.hideAllButtons();
            $(".btnSnapshot", $(this._element)).show();

            //$([this._canvasElement, this._canvasLayerElement]).keydown(function (e) {
            //    if (e.keyCode === 27) // ESC key
            //    {
            //        me.cmd_rollBack(me.cmd);
            //    }
            //});

            $(function() {
                me.initViewerControl();
            });
        },
        getQueryStringParameterValue: function (param) {
            var urlParamNameIndex = 0;
            var urlParamValueIndex = 1;
            var value = null;

            var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
            for (var i = 0; i < url.length; i++) {
                var urlparam = url[i].split('=');
                if (urlparam[urlParamNameIndex] === param) {
                    value = decodeURIComponent(urlparam[urlParamValueIndex]);
                    break;
                }
            }

            return value;
        },
        initViewerControl: function () {
            this.ensureViewerManager();

            if ($('body', getCEP_top().document).hasClass("mobile")) {
                this.isResponsive = true;
            }

            var canvasWidth = this.getContentControl().width();
            var canvasHeight = this.getContentControl().height();

            if (this.isResponsive) {
                canvasHeight = screen.availHeight - 150;
                canvasWidth = screen.availWidth - 20;
            }

            this.controlManager.setSize(canvasWidth, canvasHeight);

            var $canvasSnapshot = $("canvas.canvasContentSnapshot", $(this._element));
            var $canvasLayer = $("canvas.canvasLayer", $(this._element));
            $canvasLayer[0].width = $canvasSnapshot[0].width = canvasWidth;
            $canvasLayer[0].height = $canvasSnapshot[0].height = canvasHeight;

            // Updated page url style gets each value as a querystring parameter
            var docName = this.getQueryStringParameterValue("name");
            var docRevision = this.getQueryStringParameterValue("revision");

            if (docName) {
                this._modelDocName = docName;
                this._modelDocRev = docRevision;
            } else {
                // DEPRECATED.
                // This is the previous method of getting data from the querystring. It gets both the doc name and revision from one string and then
                // pareses it. The problem is that if there are any other parameters in the querystring this popup will not work. So the change
                // was to break up the querystring and also add a new parser. However, if there is existing code that uses the 'old' method, this
                // code will take care of that.
                var drIndex = location.search.indexOf("&DocRef=");

                if (drIndex != -1) {
                    var dr = decodeURI(location.search.substr(drIndex + 8)).split(';');
                    this._modelDocName = dr[0];
                    this._modelDocRev = dr.length > 1 ? dr[1] : "";
                }
            }

            if (this._modelDocName)
                this.openModelFromDocMaint(this._modelDocName, this._modelDocRev);
            else if (this._modelName)
                this.openModel(this._modelName);
            else if (this._modelDocRefs) {
                var docRefs = eval(this._modelDocRefs);
                this.openModelFromDocMaintList(docRefs);
            }
        },
        dispose: function() {
            this.viewerManager.clearScene();
            this.viewerManager = null;
            this.pmiManager = null;
            this.controlManager = null;
            this._modelFileName = null;
            this._canvasElement = null;

            $(".btnSnapshot", $(this._element)).unbind();
            CamstarPortal.WebControls.PlmVisWebControl.callBaseMethod(this, "dispose");
        },

        ensureViewerManager: function() {
            var me = this;
            if (!this.controlManager) {
                this.controlManager = new PLMVisWeb.Control({
                    host: this.getContentControl()[0],
                    root: null
                });
            }
            if (!this.viewerManager) {
                this.viewerManager = this.controlManager.getExtensionManager(PLMVisWeb.Viewer);
                this.viewerManager.registerSelectionEvent(function(newPSids) { me.selectionPartsEvent(newPSids) });


                this.pmiManager = this.viewerManager.addExtension(PLMVisWeb.PMI);
            }
            this.viewerManager.clearScene();
        },

        openModel: function(modelName) {
            var me = this;
            this.currentModelName = modelName;
            var deferredObject = $.Deferred();

            this.viewerManager.open("CamstarUploads/Models/" + modelName, function(success, modelRootPsId) {
                if (success) {
                    me.renderModelByRootId(modelRootPsId);
                    // release resources.
                }
                deferredObject.resolve();
            });
            return deferredObject.promise();
        },

        importModel: function(modelName) {
            var me = this;
            var deferredObject = $.Deferred();
            this.currentModelName = modelName;
            me.viewerManager.importModel("CamstarUploads/Models/" + modelName, function(success, modelId) {
                if (success) {
                    me.viewerManager.setVisibilityByPsId(modelId, true);
                }
                deferredObject.resolve();
            });
            return deferredObject.promise();
        },

        openModelFromDocMaint: function(modelDocName, modelDocRev, isImport) {
            var me = this;

            if (!modelDocRev)
                modelDocRev = "";

            var deferredObject = $.Deferred();
            var bodFolder = this.getFromStorage(modelDocName, modelDocRev);
            if (bodFolder) {
                if (!isImport) {
                    me.openModel(bodFolder).then(function() { deferredObject.resolve() });
                }
                else {
                    me.importModel(bodFolder).then(function() { deferredObject.resolve() });
                }
            }
            else {
                var formData = new FormData();
                formData.append("docName", modelDocName);
                formData.append("docRev", modelDocRev);

                var loc = window.location;
                var appPath = loc.pathname.substr(0, loc.pathname.indexOf("/", 1));
                var url = loc.protocol + "//" + loc.host + appPath + "/Jt2Bod.ashx";

                var request = new XMLHttpRequest();
                request.overrideMimeType("application/json");
                request.open("POST", url, true);
                request.onreadystatechange = function() {
                    if (request.readyState === 4 && request.status == "200") {
                        if (request.response) {
                            me.pushToStorage(modelDocName, modelDocRev, request.response);
                            if (!isImport)
                                me.openModel(request.response).then(function() { deferredObject.resolve() });
                            else
                                me.importModel(request.response).then(function() { deferredObject.resolve() });
                        }
                        else
                            deferredObject.resolve();
                    }
                }
                request.send(formData);
            }
            return deferredObject.promise();
        },

        openModelFromDocMaintList: function(modelDocRefs) {
            var me = this;
            me.viewerManager.startRenderLoop();
            var promises = [];
            $.each(modelDocRefs, function(i, val) {
                promises.push(me.openModelFromDocMaint(val.Name, val.Revision, i > 0));
            });
            $.when.apply($, promises).then(function() {
                me.viewerManager.stopRenderLoop();
                me.viewerManager.fitAll();
            });
        },

        renderModelByRootId: function(modelRootPsId) {
            var me = this;
            if (this.viewerManager.modelHasPmi(modelRootPsId)) {
                this.pmiManager.loadPmiData(modelRootPsId, function(success) {
                    if (success)
                        me.pmiManager.setVisibilityByPsId(modelRootPsId, true);
                });
            }
            this.viewerManager.setVisibilityByPsId(modelRootPsId, true);
        },

        getFromStorage: function(docName, docRev) {
            var bodFolder = "";
            var data = $(this._hiddenMap).val();
            if (data) {
                $.each(data.split(";"), function(i, val) {
                    if (val) {
                        if (docName + "+" + docRev === val.split("-")[0]) {
                            bodFolder = val.split("-")[1];
                            return false;
                        }
                    }
                    return true;
                });
            }
            return bodFolder;
        },
        pushToStorage: function(docName, docRev, folder) {
            var data = $(this._hiddenMap).val();
            if (!data)
                data = "";
            data += docName + "+" + docRev + "-" + folder + ";";
            $(this._hiddenMap).val(data);
        },

        selectionPartsEvent: function(newPSids) {
            var me = this;
            var selectedItems = "";
            $.each(newPSids, function(i, val) {
                selectedItems += me.viewerManager.getNameByPsId(val) + " ";
            });
            if (selectedItems)
                alert(selectedItems);
        },

        setDirty: function(isDirty) {
            this.isDirty = isDirty;
            var snapShotControl = this.getSnapshotControl();
            if (isDirty) {
                if (!snapShotControl.hasClass("dirty"))
                    snapShotControl.addClass("dirty");
            }
            else
                snapShotControl.removeClass("dirty");
        },

        // Toolbar functions
        btnCommandClick: function(btn) {
            var cmd = $(btn).attr("cmd");
            var me = this;
            if (this.cmd !== cmd) {
                var trueJob = null;
                switch (cmd.toLowerCase()) {
                case "snapshot":
                {
                    this.showAllButtons();
                    $(btn).hide();
                    this.doSnapshot();
                    this.setDirty(false);
                    break;
                }
                case "clear":
                {
                    trueJob = function () {
                        me.cmd_rollBack(me.cmd);
                        me.doSnapshot();
                        me.setDirty(false);
                    };
                    if (this.isDirty) {
                        jConfirm("Are you sure you want to remove your changes?", null, function (r) {
                            if (r === true)
                                trueJob();
                        }, "Warning");
                    }
                    else
                        trueJob();
                    
                    break;
                }
                case "back":
                {
                    trueJob = function() {
                        me.cmd_complete(me.cmd);
                        me.backTo3d();
                        me.setDirty(false);
                    };
                    if (this.isDirty) {
                        jConfirm("Do you want to save your changes?", null, function(r) {
                            me.cmd_complete(me.cmd);
                            if (r === true)
                                me.saveSnapshot();
                            trueJob();
                        }, "Warning");
                    }
                    else {
                        me.cmd_complete(me.cmd);
                        trueJob();
                    }
                    break;
                }
                case "save":
                {
                    this.cmd_complete(this.cmd);
                    this.saveSnapshot();
                    this.setDirty(false);
                    break;
                }
                default:
                {
                    this.deactivateAllButtons();
                    this.cmd_complete(this.cmd);
                    $(btn).addClass("active");
                    this.cmd = cmd.toLowerCase();
                    break;
                }
                }
            }
            return false;
        },

        backTo3d: function() {
            this.getContentControl().show();
            this.getSnapshotControl().hide();

            this.hideAllButtons();
            $(".btnSnapshot", $(this._element)).show();
            this.cmd_rollBack();
            this.cmd = "";
        },

        doSnapshot: function () {
            var $canvasGl = this.getCanvasGL();
            var gl = $canvasGl[0].getContext("webgl", { preserveDrawingBuffer: true });
            if (!gl)
                gl = $canvasGl[0].getContext("experimental-webgl", { preserveDrawingBuffer: true }); // for IE11.
            if (gl) {
                this.getSnapshotControl().show();
                var $canvasSnapshot = this.getSnapshotCanvas();
                this.canvasSize = [$canvasSnapshot[0].width, $canvasSnapshot[0].height];

                this.canvas2dCtx = $canvasSnapshot[0].getContext("2d");
                this.canvas2dCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);

                var $canvasLayer = this.getCanvasLayer();
                this.canvasLayerCtx = $canvasLayer[0].getContext("2d");
                this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                $canvasLayer.hide();
                this.colorChanged(null, this.getColorPickerBtn().colorpicker("val"));
                $("span.btnLineWeight > select", $(this._element)).val(3);
                this.lineWidthChanged(3);

                var me = this;
                var img = new Image;
                img.onload = function() {
                    me.canvas2dCtx.drawImage(img, 0, 0);
                    me.canvasLayerCtx.lineJoin = me.canvas2dCtx.lineJoin = "round";
                    me.canvasOffsetXY = [$(me._canvasElement).offset().left, $(me._canvasElement).offset().top];
                };
                img.src = $canvasGl[0].toDataURL();
                this.getContentControl().hide();
            }
            this.deactivateAllButtons();
            this.cmd = "";

            return false;
        },

        saveSnapshot: function() {
            var $divSnapshot = this.getSnapshotControl();
            var canvasSnapshot = $("canvas", $divSnapshot)[0];

            var fileName = this.currentModelName + ".png";
            if (navigator.msSaveBlob) // IE10+
            {
                var image = canvasSnapshot.msToBlob();
                navigator.msSaveBlob(new Blob([image], { type: "image/png" }), fileName);
            }

            else { // other browsers.
                if ("download" in document.createElement("a")) {
                    var link = document.createElement("a");
                    document.body.appendChild(link);

                    link.style.display = "none";
                    link.download = fileName;
                    link.setAttribute("href", canvasSnapshot.toDataURL("image/png").replace("image/png", "image/octet-stream"));

                    link.click();
                    document.body.removeChild(link);
                }
            }
            this.deactivateAllButtons();
            this.cmd = "";
        },

        getContentControl: function() {
            return $(".content", $(this._element));
        },

        getSnapshotControl: function() {
            return $(".contentSnapshot", $(this._element));
        },

        getCanvasGL: function() {
            return $("canvas", this.getContentControl());
        },

        getSnapshotCanvas: function() {
            return $(".canvasContentSnapshot", $(this._element));
        },

        getCanvasLayer: function() {
            return $(".canvasLayer", $(this._element));
        },
        getColorPickerBtn: function() {
            return $("span[cmd].btnColorPicker input", $(this._element));
        },

        colorChanged: function(e, color) {
            this.cmd_complete(this.cmd);
            this.canvasLayerCtx.strokeStyle = this.canvas2dCtx.strokeStyle = color;
            this.canvas2dCtx.fillStyle = color;
            $(".plmVisTextArea", this.element).css("color", color);
        },

        lineWidthChanged: function(val) {
            this.cmd_complete(this.cmd);
            this.canvasLayerCtx.lineWidth = this.canvas2dCtx.lineWidth = +val;
        },

        canvasMouseDown: function(e) {
            this.cmd_complete(this.cmd);
            switch (this.cmd) {
            case "annotation":
            {
                this.canvas2dCtx.font = "normal 17px arial";
                this.toggleTypingArea(e.pageX, e.pageY);
                break;
            }
            case "paint":
            {
                this.paintingStarted = true;
                this.lastX = e.pageX - this.canvasOffsetXY[0];
                this.lastY = e.pageY - this.canvasOffsetXY[1];
                this.draw(this.lastX, this.lastY, false);
                break;
            }
            case "circle":
            case "rect":
            case "line":
            {
                this.layerCanvasStarted = true;
                this.getCanvasLayer().show();
                this.startX = e.pageX - this.canvasOffsetXY[0];
                this.startY = e.pageY - this.canvasOffsetXY[1];
            }
            }
        },

        canvasMouseMove: function(e) {
            if (this.paintingStarted || this.layerCanvasStarted) {
                var x = e.pageX - this.canvasOffsetXY[0];
                var y = e.pageY - this.canvasOffsetXY[1];
                this.draw(x, y, true);
                this.lastX = x;
                this.lastY = y;
            }
        },

        draw: function(x, y, dragging) {
            var ctx = null;
            if (this.cmd === "paint") {
                ctx = this.canvas2dCtx;
                ctx.beginPath();
                if (!dragging) {
                    ctx.moveTo(x - 1, y);
                }
                else {
                    ctx.moveTo(this.lastX, this.lastY);
                }

                ctx.lineTo(x, y);
                ctx.closePath();
                ctx.stroke();
            }
            else if (this.cmd === "circle") {
                this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                this.drawOval(this.canvasLayerCtx, this.startX, this.startY, x, y);
            }
            else if (this.cmd === "rect") {
                this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                this.drawRect(this.canvasLayerCtx, this.startX, this.startY, x, y);
            }
            else if (this.cmd === "line") {
                this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                this.drawLine(this.canvasLayerCtx, this.startX, this.startY, x, y);
            }
        },

        drawOval: function(ctx, x1, y1, x2, y2) {
            var radiusX = (x2 - x1) * 0.5,
                radiusY = (y2 - y1) * 0.5,
                centerX = x1 + radiusX,
                centerY = y1 + radiusY,
                step = 0.01;

            ctx.beginPath();
            ctx.moveTo(centerX + radiusX, centerY);

            for (var a = step; a < 2 * Math.PI - step; a += step) {
                ctx.lineTo(centerX + radiusX * Math.cos(a), centerY + radiusY * Math.sin(a));
            }

            ctx.closePath();
            ctx.stroke();
        },

        drawRect: function(ctx, x1, y1, x2, y2) {
            ctx.beginPath();
            ctx.rect(Math.min(x1, x2), Math.min(y1, y2), Math.abs(x1 - x2), Math.abs(y1 - y2));
            ctx.stroke();
        },

        drawLine: function(ctx, x1, y1, x2, y2) {
            ctx.beginPath();
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
            ctx.stroke();
        },

        hideTypingArea: function() {
            var $textArea = $(".plmVisTextArea", $(this._element));
            $textArea.val("");
            $textArea.hide();
        },

        toggleTypingArea: function(x, y) {
            var $textArea = $(".plmVisTextArea", $(this._element));
            if ($textArea.is(":visible"))
                $textArea.hide();
            else {
                $textArea.css({ top: y, left: x });
                $textArea.show();
                setTimeout(function() {
                    $textArea.focus();
                }, 0);
            }
        },

        saveAnnotation: function() {
            var $textArea = $(".plmVisTextArea", $(this._element));
            if ($textArea.is(":visible")) {
                if ($textArea.val()) {
                    var x = $textArea.position().left - this.canvasOffsetXY[0];
                    var y = $textArea.position().top - this.canvasOffsetXY[1];

                    this.fillText(x, y, $textArea.val());
                    $textArea.css({ width: "auto", height: "auto" });
                    $textArea.val("");
                }
            }
        },

        fillText: function(x, y, value) {
            this.canvas2dCtx.fillText(value, x, y + 18);
        },

        cmd_complete: function(cmd) {
            switch (cmd) {
            case "paint":
            {
                this.paintingStarted = false;
                this.lastX = this.lastY = null;
                this.setDirty(true);
                break;
            }
            case "annotation":
            {
                this.saveAnnotation();
                this.hideTypingArea();
                this.setDirty(true);
                break;
            }
            case "circle":
            case "rect":
            case "line":
            {
                if (this.startX && this.lastX && this.startY && this.lastY) {
                    if (cmd === "circle")
                        this.drawOval(this.canvas2dCtx, this.startX, this.startY, this.lastX, this.lastY);
                    else if (cmd === "rect")
                        this.drawRect(this.canvas2dCtx, this.startX, this.startY, this.lastX, this.lastY);
                    else if (cmd === "line")
                        this.drawLine(this.canvas2dCtx, this.startX, this.startY, this.lastX, this.lastY);
                    this.startX = this.lastX = this.startY = this.lastY = null;
                    this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                    this.getCanvasLayer().hide();
                }

                this.layerCanvasStarted = false;
                this.setDirty(true);
                break;
            }
            }
            this.cmdCompleted = true;
        },

        cmd_rollBack: function(cmd) {
            this.startX = this.startY = this.lastX = this.lastY = null;
            switch (cmd) {
            case "annotation":
            {
                this.hideTypingArea();
                break;
            }
            case "circle":
            case "rect":
            case "line":
            {
                this.canvasLayerCtx.clearRect(0, 0, this.canvasSize[0], this.canvasSize[1]);
                break;
            }
            }
        },

        deactivateAllButtons: function() {
            $("span[cmd]", $(this._element)).removeClass("active");
        },

        showAllButtons: function() {
            $("span[cmd], span.btnLineWeight, div.areaShape", $(this._element)).show();
        },

        hideAllButtons: function() {
            $("span[cmd], span.btnLineWeight, div.areaShape", $(this._element)).hide();
        },

        directUpdate: function(value) {
            CamstarPortal.WebControls.PlmVisWebControl.callBaseMethod(this, "directUpdate");

            if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data)) {
                this.openModel(value.PropertyValue);
            }
        },

        get_modelName: function() { return this._modelName; },
        set_modelName: function(value) { this._modelName = value; },

        get_modelDocName: function() { return this._modelDocName; },
        set_modelDocName: function(value) { this._modelDocName = value; },

        get_modelDocRev: function() { return this._modelDocRev; },
        set_modelDocRev: function(value) { this._modelDocRev = value; },

        get_modelDocRefs: function() { return this._modelDocRefs; },
        set_modelDocRefs: function(value) { this._modelDocRefs = value; },

        get_hiddenMap: function() { return this._hiddenMap; },
        set_hiddenMap: function(value) { this._hiddenMap = value; },

        get_serverType: function() { return this._serverType; }
    },

CamstarPortal.WebControls.PlmVisWebControl.registerClass('CamstarPortal.WebControls.PlmVisWebControl', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
