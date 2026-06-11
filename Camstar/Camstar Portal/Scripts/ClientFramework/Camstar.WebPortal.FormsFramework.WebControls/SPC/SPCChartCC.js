// © 2019 Siemens Product Lifecycle Management Software Inc.
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

(function () {
    if (typeof Highcharts === "undefined" || Highcharts._spcSanitized === true || !Highcharts.wrap) {
        return;
    }

    Highcharts.setOptions({
        accessibility: { enabled: false }
    });

    var sanitizeOptionsObject = function (obj) {
        if (!obj || typeof obj !== "object") return;

        if (Object.prototype.hasOwnProperty.call(obj, "data-name")) {
            obj.custom = obj.custom || {};
            obj.custom.dataName = obj["data-name"];
            delete obj["data-name"];
        }

        if (Object.prototype.hasOwnProperty.call(obj, "series_id")) {
            obj.custom = obj.custom || {};
            obj.custom.seriesId = obj.series_id;
            delete obj.series_id;
        }
    };

    Highcharts.wrap(Highcharts.Series.prototype, "setOptions", function (proceed, itemOptions) {
        sanitizeOptionsObject(itemOptions);
        if (itemOptions && Array.isArray(itemOptions.data)) {
            for (var i = 0; i < itemOptions.data.length; i++) {
                sanitizeOptionsObject(itemOptions.data[i]);
            }
        }
        return proceed.call(this, itemOptions);
    });

    var originalAttr = Highcharts.SVGElement.prototype.attr;
    Highcharts.SVGElement.prototype.attr = function (hash, value, complete) {
        if (typeof hash === "string") {
            if (value !== undefined && (hash === "data-name" || hash === "series_id")) {
                return this;
            }
            return originalAttr.call(this, hash, value, complete);
        }
        if (hash && typeof hash === "object") {
            if (Object.prototype.hasOwnProperty.call(hash, "data-name")) delete hash["data-name"];
            if (Object.prototype.hasOwnProperty.call(hash, "series_id")) delete hash["series_id"];
        }
        return originalAttr.call(this, hash, value, complete);
    };

    Highcharts._spcSanitized = true;
}());

Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC.initializeBase(this, [element]);

    this._cssClass = null;
    this._dataJson = null;
    this._options = null;
    this._chartType = null;
    this._spcServerUrl = null;
    this._spcName = null;
    this._metricName = null;
    this._controlId = null;
    this._clientId = null;
    this._docName = null;
    this._legendLocation = null;
    this._GroupName = null;
    this._isSave = null;

    this._measurements = [];
    this._subgroups = [];
    // Fast lookups
    this._indexById = {};
    this._indexBySeq = {};

    this._subgroup = 1;
    this._isFixedHeuristic = false;
};

Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC.prototype = {
    initialize: function () {
        Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC.callBaseMethod(this, "initialize");
        this._measurements = [];
        this._subgroups = [];
        this._indexById = {};
        this._indexBySeq = {};
        this._subgroup = 1;
        console.log("[SPC] initialize()");
        this.calculateAndDraw();
    },

    // which charts are treated as fixed (as on server)
    isFixedSubgroupChart: function () {
        var t = (this._chartType || "").toLowerCase();
        return (t === "xb_s" || t === "med" || t === "s" || t === "r" ||
            t === "xb" || t === "med_r" || t === "x" || t === "xb_r" ||
            this._isFixedHeuristic === true);
    },

    isAttributeChart: function () {
        var t = (this._chartType || "").toLowerCase();
        return (t === "p" || t === "np" || t === "c" || t === "u");
    },

    seriesClick: function (data) {
        var id = null;
        var subgroupIds = [];
        var point = null;

        // 1. Find the point in our index
        if (data.sequenceId !== undefined) {
            // Measurement click
            point = this._indexBySeq["m_" + data.sequenceId] || this._indexById[data.referenceId] || this._indexById[data.id];
            if (point) id = point.id;
            console.log("[SPC] seriesClick.measurement: seq=", data.sequenceId, "-> id=", id);
        }
        else if (!isNaN(data.subgroupNumber) || data.referenceId !== undefined || data.subgroupSequenceId !== undefined) {
            // Subgroup click
            // Try explicit subgroup sequence first
            if (data.subgroupSequenceId !== undefined) {
                point = this._indexBySeq["s_" + data.subgroupSequenceId];
            }
            // Then calculated sequence from number (1-based)
            if (!point && !isNaN(data.subgroupNumber)) {
                point = this._indexBySeq["s_" + (Number(data.subgroupNumber) + 1)];
            }
            // Fallbacks
            if (!point) point = this._indexBySeq["s_" + data.sequenceId];
            if (!point) point = this._indexById[data.referenceId] || this._indexById[data.id];

            // Calculate window logic
            var isAttribute = this.isAttributeChart();
            var n = isAttribute ? 1 : (this._subgroup || 1);
            var isFixed = this.isFixedSubgroupChart();
            var pool = (this._measurements.length > 0) ? this._measurements : this._subgroups;
            var sgNum0 = 0;

            // Determine 0-based index of the subgroup
            if (point && point.isSubgroup) {
                sgNum0 = this._subgroups.indexOf(point);
            } else {
                var num = Number(data.subgroupNumber);
                if (!isNaN(num) && num > 0) sgNum0 = Math.floor(num - 1);
            }
            if (sgNum0 < 0) sgNum0 = 0;

            // Set ID from found point or raw data
            id = (point && point.id) ? point.id : (data.referenceId || data.id);

            // Collect subgroupIds (window members)
            if (pool.length > 0) {
                var startIndex, endIndex;

                if (isFixed) {
                    // Fixed window logic: [i*n .. i*n + n - 1]
                    startIndex = sgNum0 * n;
                    endIndex = startIndex + (n - 1);
                    if (pool[startIndex] && !id) id = pool[startIndex].id;
                } else {
                    // Moving window logic: [i-n+1 .. i]
                    startIndex = sgNum0 - (n - 1);
                    endIndex = sgNum0;
                    if (startIndex < 0) startIndex = 0;
                    if (pool[endIndex] && !id) id = pool[endIndex].id;
                }

                // Clamp
                if (startIndex < 0) startIndex = 0;
                if (endIndex >= pool.length) endIndex = pool.length - 1;

                // Collect distinct IDs in window
                for (var i = startIndex; i <= endIndex; i++) {
                    var pid = pool[i] && pool[i].id;
                    if (pid && subgroupIds.indexOf(pid) === -1) subgroupIds.push(pid);
                }
            }

            // If we found a point object and still have no subgroupIds, add the point's ID
            if (subgroupIds.length === 0 && point && point.id) {
                subgroupIds.push(point.id);
            }
        }

        // Ensure clean ID string
        var finalId = (id !== null && id !== undefined) ? String(id).trim() : null;

        if (finalId) {
            if (subgroupIds.length === 0) subgroupIds.push(finalId);
            console.log("[SPC] seriesClick.call annotate:", this._spcName, finalId, this._metricName, subgroupIds);
            return annotate(this._spcName, finalId, this._metricName, subgroupIds);
        }
        return false;
    },

    calculateAndDraw: function () {
        var me = this;
        if (this._dataJson) {
            var decoded = atob(this._dataJson);
            var obj = JSON.parse(decoded);
            console.log("[SPC] calculateAndDraw: has-inline-data:", !!obj.subgroups);

            if (obj.subgroups) {
                me.drawControl(obj, obj.violationDocName);
            } else if (this._spcServerUrl) {
                $.ajax({
                    url: this._spcServerUrl,
                    contentType: "application/json",
                    method: "POST",
                    data: decoded,
                    success: function (data) {
                        console.log("[SPC] ajax success, drawing");
                        me.drawControl(data);
                    },
                    error: function (xhr) {
                        console.log("[SPC] ajax error:", xhr && xhr.responseText);
                    },
                    dataType: "JSON"
                });
            }
        } else {
            console.log("[SPC] calculateAndDraw: no _dataJson");
        }
    },

    drawControl: function (data, docName) {
        var me = this;
        var $control = $(this._element);
        if (this._cssClass && !$control.hasClass(this._cssClass)) $control.addClass(this._cssClass);

        if (!data) return;

        // server errors
        if (data.messages) {
            var errors = data.messages.filter(function (m) { return (m.level || "").toLowerCase() === "error"; });
            if (errors.length > 0) {
                var msg = errors[0].messageLong || errors[0].messageShort;
                console.log("[SPC] drawControl: server error:", msg);
                __page.displayStatus(msg, "Error");
                return;
            }
        }

        var options = JSON.parse(this._options || "{}");

        // --- Helper to index data ---
        var indexData = function (source, isSubgroup) {
            var result = [];
            if (!Array.isArray(source)) return result;

            for (var i = 0; i < source.length; i++) {
                var item = source[i];
                if (!item) {
                    result.push({ id: String(i + 1), sequenceId: i + 1, isSubgroup: isSubgroup });
                    continue;
                }

                // Resolve Sequence
                var seq = i + 1;
                var seqCandidates = [
                    item.sequenceID, item.SequenceID, item.sequenceId,
                    item.subgroupSequenceId, item.SubgroupSequenceId,
                    item.subgroupNumber, item.SubgroupNumber
                ];
                for (var k = 0; k < seqCandidates.length; k++) {
                    var num = Number(seqCandidates[k]);
                    if (!isNaN(num) && isFinite(num) && num > 0) {
                        seq = (Math.abs(num % 1) > 1e-6) ? Math.round(num) : num;
                        break;
                    }
                }

                // Resolve ID
                var id = null;
                var idCandidates = [
                    item.referenceID, item.ReferenceID, item.referenceId, item.ReferenceId,
                    item.id, item.ID
                ];
                if (isSubgroup) {
                    idCandidates.push(item.subgroupId, item.SubgroupId, item.SubgroupID, item.subgroupRef);
                }
                for (var c = 0; c < idCandidates.length; c++) {
                    var val = idCandidates[c];
                    if (val !== undefined && val !== null) {
                        var str = String(val).trim();
                        if (str.length > 0) { id = str; break; }
                    }
                }
                if (!id) id = String(seq);

                var entry = { id: id, sequenceId: seq, isSubgroup: isSubgroup };
                result.push(entry);

                // Add to maps
                if (id) me._indexById[id] = entry;
                // Prefix sequence to avoid collisions between measurements and subgroups if logic requires,
                // but simplified here: m_ for measurements, s_ for subgroups
                var seqKey = (isSubgroup ? "s_" : "m_") + seq;
                me._indexBySeq[seqKey] = entry;
            }
            return result;
        };

        if (options.xAxisLabelsRotation !== undefined) {
            var rot = parseInt(options.xAxisLabelsRotation);
            if (!isNaN(rot)) options.xAxisLabelsRotation = rot;
        }

        this._indexById = {};
        this._indexBySeq = {};
        this._measurements = indexData(data.measurements, false);
        this._subgroups = indexData(data.subgroups, true);

        if (data.specifications) {
            this._subgroup = data.specifications.subgroupSize > 0 ? data.specifications.subgroupSize : 1;
        }

        // infer chart type
        var inferredType = data.controlChartType ||
            (data.specifications ? data.specifications.controlChartType : null) ||
            options.chartType;
        if (inferredType) this._chartType = String(inferredType).toLowerCase();

        // heuristic for unexpected types
        var m = this._measurements.length;
        var sg = this._subgroups.length;
        this._isFixedHeuristic = (this._subgroup > 1 && sg > 0 && m > 0 && (sg * this._subgroup) <= m);

        // locale
        var supportedLanguages = ["en", "de"];
        if (options.locale === "browserLanguage") {
            var lang = (navigator.language || navigator.userLanguage || "").toLowerCase();
            if (lang.startsWith("en")) options.locale = "en";
            else if (lang.startsWith("de")) options.locale = "de";
            else if (lang.startsWith("fr")) options.locale = "fr";
            else if (lang.startsWith("es")) options.locale = "es";
            else if (lang.startsWith("pt")) options.locale = "pt";
            else if (lang.startsWith("ko")) options.locale = "ko";
            else if (lang.startsWith("ja")) options.locale = "ja";
        }
        if (supportedLanguages.indexOf((options.locale || "").toLowerCase()) < 0) {
            var page = window.__page || parent.__page || getCEP_top().__page;
            setTimeout(function () {
                page.getLabel("Lbl_SPCLanguageSupport", function (label) {
                    var message = label[0].Value.replace("#ChartTitle", options.chartTitle);
                    page.displayStatus(message, "Warning");
                });
            }, 100);
            options.locale = "en";
        }
        options.locale = (options.locale || "en").toLowerCase();

        // events + data
        options.data = data;
        options.onSeriesClick = this.seriesClick.bind(this);

        // legend
        try {
            var legendLocation = JSON.parse(this._legendLocation || "null");
            if (legendLocation) {
                if (typeof legendLocation.enabled !== 'undefined')
                    legendLocation.enabled = (legendLocation.enabled === 'true');
                if (legendLocation.y) legendLocation.y = +legendLocation.y;
                if (legendLocation.x) legendLocation.x = +legendLocation.x;
                options.legendSettings = legendLocation;
            }
        } catch (e) { console.log("[SPC] legend parse error:", e); }

        // render
        if (options.chartType === "HistogramChart" && options.histType !== null) {
            options.chartType = options.histType;
            $(this._element).histogramChart(options);
            this.renderHistogramParameters(data, $control);
        } else if (options.chartType === "SingleValueChart") {
            $(this._element).singleValueChart(options);
            if (options.displayDescriptiveStats) {
                this.renderDescriptiveStats(data, $control, options);
            }
        } else if (options.chartType === "ProbabilityPlot") {
            $(this._element).probabilityPlot(options);
        } else if (options.chartType === "AttributiveControlChart") {
            $(this._element).attrControlChart(options);
        } else if (options.chartType === "CumulativeSumChart") {
            $(this._element).cumulativeSumChart(options);
        } else if (options.chartType === "CumulativeCountChart") {
            $(this._element).cumulativeCountChart(options);
        } else if (options.chartType === "UCumulativeSumChart") {
            $(this._element).cumulatedUSumChart(options);
        } else if (options.chartType === "DefectPareto") {
            $(this._element).defectPareto(options);
        } else {
            options.chartType = data.controlChartType || (data.specifications ? data.specifications.controlChartType : options.chartType);
            $(this._element).chart(options);
            if (options.displayDescriptiveStats) {
                this.renderDescriptiveStats(data, $control, options);
            }
        }

        this.attachDocument(this._docName);

        // autosave (legacy behavior)
        var ctl = $control[0] && $control[0].control;
        if (ctl && ctl._isSave) {
            var chartName = ctl._GroupName || "";
            var checkExist = setInterval(function () {
                if ($('.highcharts-point').length) {
                    var el = $(".spcChartPanel").clone();
                    el.find(".highcharts-tooltip").css("visibility", "hidden");
                    var html = el.html();
                    $.ajax({
                        type: "POST",
                        url: 'DownloadFile.aspx/SaveChart',
                        data: "{name: '" + chartName + "', html: '" + html + "'}",
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        async: false,
                        success: function () { console.log("[SPC] autosave done"); }
                    });
                    clearInterval(checkExist);
                }
            }, 3000);
        }
    },

    refreshSPCChart: function (responseSection) {
        if (responseSection !== null) {
            var json = responseSection.Data.Message;
            if (json) {
                this._dataJson = json;
                console.log("[SPC] refreshSPCChart: got json, redraw");
                this.calculateAndDraw();
            }
        }
    },

    attachDocument: function (docName) {
        if (!docName) return;
        var cleanDocName = docName.replace(/^["']+|["']+$/g, '');
        var safeClass = cleanDocName.replace(/[^\w\-]/g, '_');

        var doc = $('<div class="spcChartDoc spcChartDoc-' + safeClass + '"></div>');
        var link = $("<a></a>", { href: "javascript:DownloadFile('" + cleanDocName + "')" }).text(cleanDocName);
        doc.append(link);

        var docContent = $(this._element).closest(".spcContainer").find(".spcChartDocsContent");
        docContent.append(doc);
        docContent.closest(".spcChartDocs").show();
    },

    renderHistogramParameters: function (data, $control) {
        if (!data || !data.result || !data.result.processValues) return;

        var pv = data.result.processValues;
        var specs = data.specifications || {};
        var subgroups = data.subgroups || [];

        var options = JSON.parse(this._options || "{}");
        var decimalPlaces = options.decimalPlaces || 4;

        var sigma = (pv.processSigma && pv.processSigma !== 0.0) ? pv.processSigma : (pv.sigmaEstimated || 0);
        var mean = pv.calculatedXbb || 0;

        var plus3StdDev = mean + 3 * sigma;
        var minus3StdDev = mean - 3 * sigma;
        var centerSpec = specs.target || specs.currentNominalValue || null;

        var formatNum = function (val) {
            if (val === null || val === undefined || val === "") return "";
            var num = parseFloat(val);
            if (isNaN(num)) return "";
            return num.toFixed(decimalPlaces);
        };

        var formatPercent = function (val) {
            if (val === null || val === undefined || val === "") return "";
            var num = parseFloat(val);
            if (isNaN(num)) return "";
            return num.toFixed(2) + "%";
        };

        var params = [
            { label: "Mean", value: formatNum(mean) },
            { label: "St.Dev.", value: formatNum(pv.calculatedSb) },
            { label: "+3 Std Dev.", value: formatNum(plus3StdDev) },
            { label: "-3 Std Dev.", value: formatNum(minus3StdDev) },
            { label: "Cases", value: pv.countOfValidValues || "" },
            { label: "Center Spec", value: formatNum(centerSpec) },
            { label: "Sigma (σ)", value: formatNum(sigma) },
            { label: "Upper Spec", value: formatNum(specs.currentUpperToleranceLimitAbs) },
            { label: "Lower Spec", value: formatNum(specs.currentLowerToleranceLimitAbs) },
            { label: "Subgroups", value: subgroups.length || "" },
            { label: "Capability Index (Cp)", value: formatNum(pv.cp) },
            { label: "Upper Index (Cpu)", value: formatNum(pv.cpkU) },
            { label: "Lower Index (Cpl)", value: formatNum(pv.cpkL) },
            { label: "Cpk Index", value: formatNum(pv.cpk) },
            { label: "Product above spec", value: formatPercent(pv.probabilityOfValuesLargerThanUpperToleranceInPercent) },
            { label: "Product below spec", value: formatPercent(pv.probabilityOfValuesLessThanLowerToleranceInPercent) },
            { label: "Total beyond spec", value: formatPercent((pv.probabilityOfValuesLargerThanUpperToleranceInPercent || 0) + (pv.probabilityOfValuesLessThanLowerToleranceInPercent || 0)) }
        ];

        var ownerId = this._clientId || this._controlId || (this._element && this._element.id) || "";
        var $scopeParent = $(this._element).parent();
        $scopeParent.find('.spcHistogramParameters[data-owner="' + ownerId + '"]').remove();

        var $table = $('<table class="spcHistogramParameters" style="width:100%; margin-top:20px; border-collapse:collapse;">');
        if (ownerId) $table.attr("data-owner", ownerId);
        var $tbody = $('<tbody>');

        var midPoint = Math.ceil(params.length / 2);
        for (var i = 0; i < midPoint; i++) {
            var $row = $('<tr>');
            var $leftCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
            $leftCell.append($('<strong>').text(params[i].label + ": "));
            $leftCell.append($('<span>').text(params[i].value));
            $row.append($leftCell);

            if (i + midPoint < params.length) {
                var $rightCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
                $rightCell.append($('<strong>').text(params[i + midPoint].label + ": "));
                $rightCell.append($('<span>').text(params[i + midPoint].value));
                $row.append($rightCell);
            } else {
                $row.append($('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">'));
            }
            $tbody.append($row);
        }

        $table.append($tbody);
        var chartWidth = $(this._element).outerWidth();
        if (chartWidth && chartWidth > 0) $table.css("width", chartWidth + "px");
        $(this._element).after($table);
    },

    renderDescriptiveStats: function (data, $control, options) {
        if (!data || !data.result || !data.result.processValues) return;

        var pv = data.result.processValues;
        var decimalPlaces = options.decimalPlaces || 4;

        var formatNum = function (val) {
            if (val === null || val === undefined) return "";
            var num = parseFloat(val);
            if (isNaN(num)) return "";
            return num.toFixed(decimalPlaces);
        };

        var variance = pv.calculatedSb ? Math.pow(pv.calculatedSb, 2) : null;

        var params = [
            { label: "Mean", value: formatNum(pv.calculatedXbb) },
            { label: "Std Dev.", value: formatNum(pv.calculatedSb) },
            { label: "Variance", value: formatNum(variance) },
            { label: "Range", value: formatNum(pv.range) },
            { label: "Min", value: formatNum(pv.calculatedMin) },
            { label: "Max", value: formatNum(pv.calculatedMax) },
            { label: "Valid Cases", value: pv.countOfValidValues || "" }
        ];

        var ownerId = this._clientId || this._controlId || (this._element && this._element.id) || "";
        var $scopeParent = $(this._element).parent();
        $scopeParent.find('.spcDescriptiveStats[data-owner="' + ownerId + '"]').remove();

        var $table = $('<table class="spcDescriptiveStats" style="width:100%; margin-top:20px; border-collapse:collapse;">');
        if (ownerId) $table.attr("data-owner", ownerId);
        var $tbody = $('<tbody>');

        var midPoint = Math.ceil(params.length / 2);
        for (var i = 0; i < midPoint; i++) {
            var $row = $('<tr>');
            var $leftCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
            $leftCell.append($('<strong>').text(params[i].label + ": "));
            $leftCell.append($('<span>').text(params[i].value));
            $row.append($leftCell);

            if (i + midPoint < params.length) {
                var $rightCell = $('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">');
                $rightCell.append($('<strong>').text(params[i + midPoint].label + ": "));
                $rightCell.append($('<span>').text(params[i + midPoint].value));
                $row.append($rightCell);
            } else {
                $row.append($('<td style="padding:5px 10px; border:1px solid #ddd; width:50%;">'));
            }
            $tbody.append($row);
        }

        $table.append($tbody);
        var chartWidth = $(this._element).outerWidth();
        if (chartWidth && chartWidth > 0) $table.css("width", chartWidth + "px");
        $(this._element).after($table);
    },

    dispose: function () {
        console.log("[SPC] dispose()");
        this._cssClass = null;
        this._dataJson = null;
        this._options = null;
        this._chartType = null;
        this._spcServerUrl = null;
        this._spcName = null;
        this._metricName = null;
        this._controlId = null;
        this._clientId = null;
        this._docName = null;
        this._measurements = null;
        this._subgroups = null;
        this._indexById = null;
        this._indexBySeq = null;
        this._GroupName = null;
        this._isSave = null;
        Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC.callBaseMethod(this, "dispose");
    },

    // --- getters/setters ---
    get_Hidden: function () { return this._element.style.display === "none"; },
    set_Hidden: function (v) { this._element.style.display = v ? "none" : ""; this._label.style.display = v ? "none" : ""; },

    get_cssClass: function () { return this._cssClass; }, set_cssClass: function (v) { this._cssClass = v; },
    get_chartType: function () { return this._chartType; }, set_chartType: function (v) { this._chartType = v; },
    get_spcServerUrl: function () { return this._spcServerUrl; }, set_spcServerUrl: function (v) { this._spcServerUrl = v; },
    get_options: function () { return this._options; }, set_options: function (v) { this._options = v; },
    get_legendLocation: function () { return this._legendLocation; }, set_legendLocation: function (v) { this._legendLocation = v; },
    get_data: function () { return this._dataJson; }, set_data: function (v) { this._dataJson = v; },
    get_metricName: function () { return this._metricName; }, set_metricName: function (v) { this._metricName = v; },
    get_spcName: function () { return this._spcName; }, set_spcName: function (v) { this._spcName = v; },
    get_GroupName: function () { return this._GroupName; }, set_GroupName: function (v) { this._GroupName = v; },
    get_isSave: function () { return this._isSave; }, set_isSave: function (v) { this._isSave = v; },
    get_controlId: function () { return this._controlId; }, set_controlId: function (v) { this._controlId = v; },
    get_clientId: function () { return this._clientId; }, set_clientId: function (v) { this._clientId = v; },
    get_docName: function () { return this._docName; }, set_docName: function (v) { this._docName = v; }
};

Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC.registerClass(
    "Camstar.WebPortal.FormsFramework.WebControls.SPCChartCC",
    Camstar.UI.Control
);

if (typeof (Sys) !== "undefined") Sys.Application.notifyScriptLoaded();
