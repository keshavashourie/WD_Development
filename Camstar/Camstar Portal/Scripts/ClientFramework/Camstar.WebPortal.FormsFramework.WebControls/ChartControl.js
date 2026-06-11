// Copyright Siemens 2023  

/// <reference path="~/Scripts/jquery/d3.v3.min.js" />
/// <reference path="~/Scripts/jquery/nv.d3.min.js" />

Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.ChartControl = function (element)
{
    Camstar.WebPortal.FormsFramework.WebControls.ChartControl.initializeBase(this, [element]);

    this._controlElement = null;
    this._chartElement = null;
    this._zoomDataElement = null;
    this._chartMode = null;
    this._dataSet = null;
    this._seriesSet = null;
    this._extensions = null;

    this._titleLabel = null;
    this._xAxisLabel = null;
    this._yAxisLabel = null;
    this._xAxisTickFormat = null;
    this._yAxisTickFormat = null;

    this._xIsDate = null;
    this._yIsDate = null;
    this._isDiscrete = null; // without serie.
    this._isCustomTooltip = null;
    this._autoPostBack = null;
    this._showXAxis = null;
    this._showYAxis = null;
    this._showLegends = null;

    this._controlId = null;
    this._callStackKey = null;
    this._serverType = "Camstar.WebPortal.FormsFramework.WebControls.ChartControl, Camstar.WebPortal.FormsFramework.WebControls";
    this._chartObj = null;
    this._siemensColors = [0x0F789B, 0xE5C04C, 0xDB535A, 0x606A75, 0xE2894D, 0x7F4664, 0x689962, 0xE8AAB8, 0xA58F6F, 0x7FB2AC, 0x00557D, 0xA8852D, 0x993140, 0x32393F, 0xAA6130, 0x512D43, 0x476642, 0xAD687F, 0x705E4B, 0x4D7C73];
	this._Qty_Round = false;
},

Camstar.WebPortal.FormsFramework.WebControls.ChartControl.prototype =
{
    initialize: function ()
    {
        Camstar.WebPortal.FormsFramework.WebControls.ChartControl.callBaseMethod(this, "initialize");

        // Register control extensions
        if (this._extensions != null)
            eval(this._extensions);

        // Add 3d shadows for tooltips
        $(this._chartElement).addClass("with-3d-shadow");

        // Generate and display chart
        this.drawChart();
    },

    dispose: function ()
    {
        this._dataSet = null;
        this._controlElement = null;
        this._chartElement = null;
        this._zoomDataElement = null;

        Camstar.WebPortal.FormsFramework.WebControls.ChartControl.callBaseMethod(this, "dispose");
    },

    RefreshChart: function (chartType)
    {
        // Remove previously generated chart(s)
        $.map(nv.graphs, function (chart) { $(chart.container).remove(); });
        nv.graphs = [];

        this.set_ChartMode(chartType);
        this.drawChart();
    },
	
	    RefreshChart_HomePage: function (chartType)
    {
        // Remove previously generated chart(s)
        $.map(nv.graphs, function (chart) { $(chart.container).remove(); });
        nv.graphs = [];
		this._Qty_Round = true;
		
        this.set_ChartMode(chartType);
        this.drawChart();
    },

    // Convert dataset into NVD3 acceptable format
    nvd3Data: function () {
        var data = [];
        if (this._dataSet)
        {
            var me = this;
            this._isDiscrete = true;
            this._isCustomTooltip = false;
            var dataObject = {};
            this._dataSet.forEach(function (item) {
                if (typeof (item.ser) != 'undefined') {
                    me._isDiscrete = false;
                    return false;
                }
                return true;
            });
            var discreteSerie = "Discrete";
            if (this._isDiscrete)
                dataObject[discreteSerie] = [];

            this._dataSet.forEach(function (item) {
                var x = item.x;
                if (x && !isNaN(parseFloat(x.toString().replace(",", "."))))
                    x = item.x.toString().replace(",", ".");
                var y = item.y;
                if (y && !isNaN(parseFloat(y.toString().replace(",", "."))))
                    y = item.y.toString().replace(",", ".");
                if ($.isNumeric(x))
                    x = +x; // allows to render negative values as well.
                else {
                    if (me._xIsDate !== false) {
                        var dateTimeX = new Date(item.x.toString()).toLocaleDateString();
                        if (me.isValidDate(item.x.toString())) {
                            me._xIsDate = true;
                            x = dateTimeX;
                        }
                        else
                            me._xIsDate = false;
                    }
                }
                if ($.isNumeric(y))
                    y = +y; // allows to render negative values as well.
                else {
                    if (me._yIsDate !== false) {
                        var dateTimeY = new Date(item.y.toString()).toLocaleDateString();
                        if (me.isValidDate(item.y.toString())) {
                            me._yIsDate = true;
                            y = dateTimeY;
                        }
                        else
                            me._yIsDate = false;
                    }
                }
                var chartValue = { "x": x, "y": y };
                if (item.tt) // if custom tooltip specified.
                {
                    chartValue.toolTip = item.tt;
                    me._isCustomTooltip = true;
                }
                chartValue.value = item.val;
                if (me._isDiscrete)
                    dataObject[discreteSerie].push(chartValue);
                else {
                    if (item.ser) {
                        var serieSettings = me._seriesSet[item.ser.toString()];
                        if (typeof (dataObject[serieSettings.Key]) == "undefined")
                            dataObject[serieSettings.Key] = { "values": [] };
                        if (me._chartMode === "ScatterPlot") {
                            chartValue.size = 1;
                            if (serieSettings.Size && serieSettings.Size >= 1)
                                chartValue.size = serieSettings.Size;
                            chartValue.shape = "circle";
                            if (serieSettings.ShapeValue)
                                chartValue.shape = serieSettings.ShapeValue;
                        }
                        if (serieSettings.Color)
                            dataObject[serieSettings.Key].color = serieSettings.Color;

                        dataObject[serieSettings.Key].values.push(chartValue);
                    }
                }
            });
            if (this._isDiscrete)
                data.push({ key: discreteSerie, values: dataObject[discreteSerie] });
            else {
                var keys = Object.keys(dataObject);
                keys.forEach(function(key) {
                    var valuesObj = { key: key, values: dataObject[key].values };
                    if (dataObject[key].color)
                        valuesObj.color = dataObject[key].color;
                    data.push(valuesObj);
                });
            }
        }
        return data;
    },

    isValidDate: function (d) {
        var extendedIsoRegex = /^\s*((?:[+-]\d{6}|\d{4})-(?:\d\d-\d\d|W\d\d-\d|W\d\d|\d\d\d|\d\d))(?:(T| )(\d\d(?::\d\d(?::\d\d(?:[.,]\d+)?)?)?)([+-]\d\d(?::?\d\d)?|\s*Z)?)?$/,
            basicIsoRegex = /^\s*((?:[+-]\d{6}|\d{4})(?:\d\d\d\d|W\d\d\d|W\d\d|\d\d\d|\d\d|))(?:(T| )(\d\d(?:\d\d(?:\d\d(?:[.,]\d+)?)?)?)([+-]\d\d(?::?\d\d)?|\s*Z)?)?$/,
            rfc2822 = /^(?:(Mon|Tue|Wed|Thu|Fri|Sat|Sun),?\s)?(\d{1,2})\s(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s(\d{2,4})\s(\d\d):(\d\d)(?::(\d\d))?\s(?:(UT|GMT|[ECMP][SD]T)|([Zz])|([+-]\d{4}))$/;
        if (extendedIsoRegex.test(d) || basicIsoRegex.test(d) || rfc2822.test(d)) {
            return true;
        }
        else {
            return false;
        }
    },

    drawChart: function ()
    {
        if (this._dataSet.length !== 0)
        {
            if (this._chartMode === "Pie")
            {
                $(this._chartElement)
                    .css('overflow', 'auto')
                    .css('display', '-moz-box')
                ;
                var data = this.nvd3Data();
                for (var i = 0; i < data.length; i++)
                    nv.addGraph(this.createPieChart(data[i]));
            }
            else
            {
                $(this._chartElement)
                    .css('overflow', 'hidden')
                    .css('display', 'block')
                ;
                nv.addGraph(this.createChart());
            }
        }
        else
        {
            this.WriteSampleImage();
        }
        $(window).trigger('resize');
    },

    createChart: function ()
    {
        var me = this;
        var chartData = this.nvd3Data();

        return function () {
            var svgContainer = d3.select(me._chartElement)
                .append("svg")
                   .attr("width", $(me._controlElement).width())
                   .attr("height", $(me._controlElement).height());

            var chart = me.getChartModel();
            var options = {
                showMaxMin: false,
                useInteractiveGuideline: true,
                transitionDuration: 300,
                groupSpacing: 0.25,
            };
            if (me._siemensColors) {
                options.color = me.getColorFunc();
            }
            me.ensureChartOptions(options);
            chart.options(options);
            if (me._isDiscrete)
                chart.showLegend(false);
            
            if (me._xAxisTickFormat || me._xIsDate)
            {
                if (!me._xIsDate)
                {
                    chart.xAxis
                        .tickFormat(function(d) {
                            return d3.format(me._xAxisTickFormat)(d);
                        });
                }
                else
                {
                    if (!me._xAxisTickFormat)
                        me._xAxisTickFormat = "%m/%d/%y";

                    chart.xAxis
                        .tickFormat(function(d) {
                            return d3.time.format(me._xAxisTickFormat)(new Date(d));
                        });
                }
            }
            if (me._yAxisTickFormat || me._yIsDate)
            {
                if (!me._yIsDate)
                {
                    chart.yAxis
                        .tickFormat(function(d) {
                            return d3.format(me._yAxisTickFormat)(d);
                        });
                }
                else
                {
                    if (!me._yAxisTickFormat)
                        me._yAxisTickFormat = "%m/%d/%y";

                    chart.yAxis
                        .tickFormat(function(d) {
                            return d3.time.format(me._yAxisTickFormat)(d);
                        });
                }
            }
            if (me.drawChartTitle(svgContainer))
                chart.legend.margin({ top: 15, bottom: 10 });

            if (me._isCustomTooltip) {
                chart.tooltip.contentGenerator(function (p) {
                    return p.point.toolTip;
                });
            }
            if (me._showLegends === false)
                chart.showLegend(false);

            me.drawYAxisLabel(svgContainer);
            me.drawXAxisLabel(svgContainer);
			
			if (this._Qty_Round != false)
			{
				chart.valueFormat(d3.format(".0f"));
			}

            svgContainer
                .datum(chartData)
                .call(chart);

            me.chartCreated();
            me.addZoom({
                xAxis: chart.xAxis,
                yAxis: chart.yAxis,
                yDomain: chart.yDomain,
                xDomain: chart.xDomain,
                redraw: function () {
                    chart.update();
                    me.chartCreated();
                }
            });

            nv.utils.windowResize(chart.update);

            if (me._showXAxis === false) {
                var xAxis = d3.select(".nv-axis.nv-x");
                if (xAxis)
                    xAxis.remove();
            }
            if (me._showYAxis === false) {
                var yAxis = d3.select(".nv-axis.nv-y");
                if (yAxis)
                    yAxis.remove();
            }

            me._chartObj = chart;
            return chart;
        };
    },

    redrawChart: function () {
        if (this._chartObj && this._chartObj.update)
            this._chartObj.update();
    },

    getColorFunc: function () {
        var colorFunc = nv.utils.getColor();

        if (this._siemensColors) {
            // seems to be a problem in nv.d3 wrapper or I don't understand correct way to call it, or intent of the "getColor" function.
            // below looks to be the correct way to set a custom color function, but when called, it returns decimal color values and it needs to be hex.
            // the default color function converts the decimal values defined in d3 to "#XXXXXX" hex format.
            // do not know why this doesn't convert here, but easy enough to do manually.
            var getColorFunc = nv.utils.getColor(this._siemensColors);
            colorFunc = function (a, b) {
                var hexColor = (getColorFunc(a, b)).toString(16);
                while (hexColor.length < 6)
                    hexColor = '0' + hexColor;
                return '#' + hexColor;
            }
        }

        return colorFunc;
    },

    createPieChart: function (dataSet)
    {
        var me = this;
        var chartData = dataSet;
        return function ()
        {
            var svgContainer = d3.select(me._chartElement)
                   .append("svg")
                   .attr("width", $(me._controlElement).height())
                   .attr("height", $(me._controlElement).height());

            var chart = nv.models.pieChart();

            chart.options({
                x: function (d, i) { return d.x; },
                y: function (d, i) { return d.y; },
                labelType: 'percent'
            });

            if (me.drawChartTitle(svgContainer, chartData.key))
                chart.legend.margin({ top: 15 }); // add some space for title.

            svgContainer
                .datum(function () { return chartData.values; })
                .transition()
                .duration(1200)
                .call(chart);

            nv.utils.windowResize(chart.update);
            return chart;
        };
    },

    ensureChartOptions: function(options) {
        if (this._chartMode === "ScatterPlot") {
            options.useInteractiveGuideline = false;
        }
        if (this._isDiscrete) {
            if (this._chartMode === "Multibar") {
                options.staggerLabels = true;
                options.tooltips = false;
                options.showValues = true;
            }
        }
    },

    chartCreated: function () {
        if (this._chartMode === "ScatterPlot") {
            var rect = d3.select("defs > clipPath > rect");
            if (rect.length > 0) {
                var width = rect.attr("width");
                if (width) {
                    width = +width + 15;
                    rect.attr("width", width);
                }
                var height = rect.attr("height");
                if (height) {
                    height = +height + 15;
                    rect.attr("height", height);
                }
                rect.attr("x", -5).attr("y", -5);
            }
        }
    },

    getChartModel: function() {
        var chart;
        var me = this;
        if (this._chartMode === "Line")
            chart = nv.models.lineChart();
        else if (this._chartMode === "Multibar") {
            if (!this._isDiscrete)
                chart = nv.models.multiBarChart();
            else
                chart = nv.models.discreteBarChart();
        }
        else if (this._chartMode === "Stacked") {
            chart = nv.models.stackedAreaChart();
        }
        else if (this._chartMode === "ScatterPlot") {
            chart = nv.models.scatterChart();
            chart.clipEdge(true);
            chart.pointDomain([0, 100]);
            chart.scatter.dispatch.on("elementClick", function (e) {
                if (e.point && me._autoPostBack) {
                    __page.postback(me.get_controlId(), JSON.stringify({ X: e.point.x, Y: e.point.y, Value: e.point.value }));
                }
            });
        }
        else
            chart = nv.models.lineChart();

        return chart;
    },

    WriteSampleImage: function ()
    {
        var svgContainer = d3.select(this._chartElement)
            .append("svg")
            .attr("width", $(this._controlElement).width())
            .attr("height", $(this._controlElement).height());

        var svg = svgContainer
          .append("g")
        ;

        var groupByScale = d3.scale.linear()
            .range([0, $(this._controlElement).width() - 200], .1)
        ;

        var seriesScale = d3.scale.linear()
            .range([0, $(this._controlElement).height() - 100], .1)
        ;

        var groupByAxis = d3.svg.axis()
            .scale(groupByScale)
            .orient("bottom")
            .tickFormat(d3.format(",d"))
        ;

        var seriesAxis = d3.svg.axis()
            .scale(seriesScale)
            .orient("left")
            .tickFormat(d3.format(",d"))
        ;
        svg.append("g")
          .attr("class", "x axis")
          .attr("transform", "translate(50," + ($(this._controlElement).height() - 50) + ")")
          .call(groupByAxis)
        ;

        svg.append("g")
          .attr("class", "y axis")
          .attr("transform", "translate(50, 50)")
          .call(seriesAxis)
        ;

        this.drawChartTitle(svgContainer);
    },

    drawYAxisLabel: function (titlesContainer)
    {
        if (this.get_YAxisLabel()) {
            titlesContainer.append("text")
                .attr("transform", "rotate(-90)")
                .attr("y", 0)
                .attr("x", 0 - (titlesContainer.attr("height") / 2))
                .attr("dy", "1em")
                .text(this.get_YAxisLabel())
                .attr("class", "cs-axis-label")
                .style("text-anchor", "middle");
        }
    },

    drawXAxisLabel: function (titlesContainer)
    {
        if (this.get_XAxisLabel()) {
            titlesContainer.append("text")
                .attr("x", titlesContainer.attr("width") / 2)
                .attr("y", titlesContainer.attr("height") - 10)
                .style("text-anchor", "middle")
                .text(this.get_XAxisLabel())
                .attr("class", "cs-axis-label");
        }
    },

    drawChartTitle: function (titlesContainer, title)
    {
        if (this.get_TitleLabel() || title ) {
            titlesContainer.append("text")
                .attr("x", titlesContainer.attr('width') / 2)
                .attr("y", 0)
                .attr("dy", "1em")
                .style("text-anchor", "middle")
                .text(this.get_TitleLabel() || title)
                .attr("class", "cs-chart-title");
            return true;
        }
        return false;
    },


    addZoom: function(options) {
        var me = this;
        // parameters
        var yAxis = options.yAxis;
        var xAxis = options.xAxis;

        // scales
        var xScale = xAxis.scale();
        var yScale = yAxis.scale();

        var xDomain = options.xDomain || xScale.domain;
        var yDomain = options.yDomain || yScale.domain;
        var redraw = options.redraw;
        
        // min/max boundaries
        var x_boundary = xScale.domain().slice();
        var y_boundary = yScale.domain().slice();

        if (typeof xScale.nice === 'function') {
            // ensure nice axis
            xScale.nice();
            yScale.nice();
        }

        // create d3 zoom handler
        var d3zoom = d3.behavior.zoom();

        // zoom event handler
        function zoomed() {
            var yDomainVal = yScale.domain();
            var xDomainVal = xScale.domain();
            var zoomTrans = d3zoom.translate();
            var zoomValStr = JSON.stringify({ xDomain: xDomainVal, yDomain: yDomainVal, scale: d3zoom.scale(), zoomTrans: zoomTrans });
            $(me._zoomDataElement).val(zoomValStr);

            yDomain(yDomainVal);
            xDomain(xDomainVal);
            redraw();
        };

        // zoom event handler
        function unzoomed() {
            $(me._zoomDataElement).val("");
            xDomain(x_boundary);
            yDomain(y_boundary);
            redraw();
            d3zoom.scale(1);
            d3zoom.translate([0, 0]);
        };

        var scaleExtent = 50;
        // initialize wrapper
        d3zoom.x(xScale)
            .y(yScale)
            .scaleExtent([1, scaleExtent])
            .on('zoom', zoomed);

        // add handler
        d3.select(this._chartElement).call(d3zoom).on('dblclick.zoom', unzoomed);

        var zoomVal = $(me._zoomDataElement).val();
        if (zoomVal) {
            var zoomValues = JSON.parse(zoomVal);
            xDomain(zoomValues.xDomain);
            yDomain(zoomValues.yDomain);
            d3zoom.scale(zoomValues.scale);
            d3zoom.translate(zoomValues.zoomTrans);
            window.setTimeout(function () { redraw(); }, 0);
        }
    },

    _sendRequest: function (callParameters) {
        var self = this;
        var recallSendRequest = function () {
            self._sendRequest(callParameters);
        };
        if (!__page._lock) {
            callParameters.CallStackKey = this._callStackKey;
            callParameters.UniqueId = this._controlId;
            callParameters.ClientId = $(this._controlElement).attr("id");
            
            var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), this);
            transition.set_command("ClientEntry");

            var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
            transition.set_commandParameters(callParamsString);
            transition.set_clientCallback("dummyEvent");
            transition.set_noModalImage(true);
            var communicator = new Camstar.Ajax.Communicator(transition, this);
            communicator.syncCall();
        }
        else
            window.setTimeout(recallSendRequest, 100);
    },

    dummyEvent: function () {
    },

    processStatusData: function (statusData) {
    },

    //directUpdate: function (value) {
    //    Camstar.WebPortal.FormsFramework.WebControls.ChartControl.callBaseMethod(this, 'directUpdate');

    //    if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data))
    //    {
    //        this._dataSet = value.PropertyValue;
    //        this.drawChart();
    //    }
    //},

    get_ControlElement: function () { return this._controlElement; },
    set_ControlElement: function (value) { this._controlElement = value; },

    get_ChartElement: function () { return this._chartElement; },
    set_ChartElement: function (value) { this._chartElement = value; },

    get_ZoomDataElement: function () { return this._zoomDataElement; },
    set_ZoomDataElement: function (value) { this._zoomDataElement = value; },

    get_ChartMode: function () { return this._chartMode; },
    set_ChartMode: function (value) { this._chartMode = value; },

    get_DataSet: function () { return this._dataSet; },
    set_DataSet: function (value) { this._dataSet = value ? JSON.parse(value) : []; },

    get_SeriesSet: function () { return this._seriesSet; },
    set_SeriesSet: function (value) { this._seriesSet = value ? JSON.parse(value) : []; },

    get_Extensions: function () { return this._extensions; },
    set_Extensions: function (value) { this._extensions = value; },

    get_TitleLabel: function () { return this._titleLabel; },
    set_TitleLabel: function (value) { this._titleLabel = value; },

    get_XAxisLabel: function () { return this._xAxisLabel; },
    set_XAxisLabel: function (value) { this._xAxisLabel = value; },

    get_YAxisLabel: function () { return this._yAxisLabel; },
    set_YAxisLabel: function (value) { this._yAxisLabel = value; },

    get_XAxisTickFormat: function () { return this._xAxisTickFormat; },
    set_XAxisTickFormat: function (value) { this._xAxisTickFormat = value; },

    get_YAxisTickFormat: function () { return this._yAxisTickFormat; },
    set_YAxisTickFormat: function (value) { this._yAxisTickFormat = value; },

    get_AutoPostBack: function () { return this._autoPostBack; },
    set_AutoPostBack: function (value) { this._autoPostBack = value; },

    get_ShowXAxis: function () { return this._showXAxis; },
    set_ShowXAxis: function (value) { this._showXAxis = value; },

    get_ShowYAxis: function () { return this._showYAxis; },
    set_ShowYAxis: function (value) { this._showYAxis = value; },

    get_ShowLegends: function () { return this._showLegends; },
    set_ShowLegends: function (value) { this._showLegends = value; },

    get_controlId: function () { return this._controlId; },
    set_controlId: function (value) { this._controlId = value; },
    
    get_callStackKey: function () { return this._callStackKey; },
    set_callStackKey: function (value) { this._callStackKey = value; },

    get_serverType: function () { return this._serverType; }
}

Camstar.WebPortal.FormsFramework.WebControls.ChartControl.registerClass('Camstar.WebPortal.FormsFramework.WebControls.ChartControl', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
