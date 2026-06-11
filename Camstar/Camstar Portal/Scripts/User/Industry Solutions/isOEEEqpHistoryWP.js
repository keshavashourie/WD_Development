/*
***************************************************************************
© 2017 Siemens Product Lifecycle Management Software Inc.
This file should include all user-defined javascript functions. 
Functions defined in this file will over-ride any functions of the
same name written in any of the Camstar supplied javascript files.
***************************************************************************
*/
var isOEEEqpHistoryWP = (function () {
    'use strict';

    // Set with default values.  Values will be replaced with localized text
    var _labels = {
        isOEEQuality: 'Quality',
        isOEEAvailability: 'Availability',
        isOEEPerformance: 'Performance',
        isOEE: 'OEE',
        isQualityTopScrapReasons: 'Quality - Top Scrap / Rework Reasons',
        isDowntimeTopFaultReasons: 'Downtime - Top Fault Reasons',
        isResStatusCodeDist: 'Resource Status Code - Distribution',
        isOEEScrapped: 'Scrapped',
        isOEEStart: 'Start',
        isOEEEnd: 'End',
        isOEEReason: 'Reason',
        isOEEDuration: 'Duration',
        isOEEType: 'Type',
        isOEEQuantity: 'Quantity',
        isOEEScrap: 'Scrap',
        isOEERework: 'Rework'
    };

    // Red/yellow/green colors for the OEE chart data values
    var DATA_COLORS = {
        good: '#0a9b00',
        warning: '#fadc5a',
        bad: '#dc0000'
    };
    Object.freeze(DATA_COLORS);

    var TEMPLATES = {
        scrapCount: 
            '<div id="scrap-container">\
                <div id="scrap-count">_SCRAPPEDCOUNT_</div>\
                <div id="scrap-label">_SCRAPPEDLABEL_</div>\
            </div>',
        legendStatusCode:
            '<div class="status-code">\
                <div class="status-code-color" style="background-color:_STATUSCODECOLOR_"></div>\
                <div class="status-code-name">_STATUSCODENAME_</div>\
            </div>'
    };
    Object.freeze(TEMPLATES);


    // interface
    var isOEEEqpHistoryWPInterface = {
        initialize: initialize,
        refreshChart: refreshChart,
        get_controlId: get_controlId,
        get_serverType: get_serverType,
        loadLabels: loadLabels
    }
    return isOEEEqpHistoryWPInterface;

    // call server to load all the labels we need
    function initialize() {
        setTimeout(function () { refreshChart(null); }, 700);
    }

    function loadLabels(dataset) {
        // Build param to getLabels()
        var labelNames = [];

        $.each(_labels, function (name, val) {
            labelNames.push({ Name: name, Value: val });
        });

        createReport(dataset);
    }

    //refreshChart
    function refreshChart(eqpName) {
        var transition = new Camstar.Ajax.Transition(eval(Camstar.Ajax.RequestType.Command), isOEEEqpHistoryWP);
        transition.set_command("LoadOEEDetails");
        var callParameters =
        {
            "Equipment": eqpName,
            "Resource": $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isResourceOEEInquiry_Resource_ctl00")[0].value,
            "StartTime": $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isResourceOEEInquiry_isStartTime")[0].value,
            "EndTime": $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isResourceOEEInquiry_isEndTime")[0].value

        };
        var callParamsString = Sys.Serialization.JavaScriptSerializer.serialize(callParameters);
        transition.set_commandParameters(callParamsString);
        transition.set_clientCallback("loadLabels");

        var communicator = new Camstar.Ajax.Communicator(transition, isOEEEqpHistoryWP);
        communicator.syncCall();
        communicator.dispose();
    }

    function get_serverType() {
        return "Camstar.WebPortal.WebPortlets.Shopfloor.isOEEEqpHistoryWP, App_Code";
    }

    function get_controlId() {
        return "isOEEEqpHistoryWP";
    }

    function createReport(dataset) {
        d3.select("#ctl00_WebPartManager_isOEEEqpHistoryWP_containerDiv").selectAll("*").remove();

        var OEEData = null;
        if (dataset.Data && dataset.Data.HTML) {
            OEEData = JSON.parse(dataset.Data.HTML);
        }

        var dataOEEIndicator = makeStructureForOEE(OEEData);


        if (dataOEEIndicator[0] != null) {
            var oeeAvailibility = dataOEEIndicator[0].series[0].values[0];
            var oeePerformance = dataOEEIndicator[0].series[1].values[0];
            var oeeQuality = dataOEEIndicator[0].series[2].values[0];
            var oeeOEE = dataOEEIndicator[0].series[3].values[0];

        } else {
            var oeeAvailibility = 0;
            var oeePerformance = 0;
            var oeeQuality = 0;
            var oeeOEE = 0;
        }

        $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isEqpHist_Availability_ctl00")[0].value = oeeAvailibility;
        $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isEqpHist_Performance_ctl00")[0].value = oeePerformance;
        $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isEqpHist_Quality_ctl00")[0].value = oeeQuality;
        $("#ctl00_WebPartManager_isOEEEqpHistoryWP_isEqpHist_OEE_ctl00")[0].value = oeeOEE;

        createOeeIndicators("#ctl00_WebPartManager_isOEEEqpHistoryWP_containerDiv", 134, oeeAvailibility, oeePerformance, oeeQuality, oeeOEE);

      }

    function arrayify(collection) {
        return Array.prototype.slice.call(collection);
    }

    function factory(headings) {
        return function (row) {
            return arrayify(row.cells).reduce(function (prev, curr, i) {
                prev[headings[i]] = curr.innerText;
                return prev;
            }, {});
        }
    }

    // Add <thead> to table with the given ID.  Set contents to given markup
    function insertHead(idtable, theadInnerHtml) {
        var table = document.getElementById(idtable);
        if (!table) { 
            console.log(idtable, "not exist!"); 
        };
        var header = table.createTHead();
        header.style.display = "none";
        header.innerHTML = theadInnerHtml;
    }

    // Take rows from given table DOM element and create an array of data from them
    function parseTable(table) {
        var headings = arrayify(table.tHead.rows[0].cells).map(function (heading) {
            return heading.innerText;
        });
        return arrayify(table.tBodies[0].rows).map(factory(headings));
    }

    // Transform the given data set into an array of data
    function makeStructureForOEE(dataset) {
        if(dataset == null) return [];
        var dataSeries = []; // return value
        var resource = [];
        var serieAvailability = {
            values: []
        };
        var seriePerformance = {
            values: []
        };
        var serieQuality = {
            values: []
        };
        var serieOEE = {
            values: []
        };

        for (var i = 0; i < dataset.length; i++) {
            resource.push(dataset[i].Resource);
            //Availability
            if (dataset[i].isResourceOEEInquiryDetails_isAvailability !== undefined)
                serieAvailability.values.push(dataset[i].isResourceOEEInquiryDetails_isAvailability.Value);
            //Performance
            if (dataset[i].isResourceOEEInquiryDetails_isPerformance !== undefined)
                seriePerformance.values.push(dataset[i].isResourceOEEInquiryDetails_isPerformance.Value);
            //Quality
            if (dataset[i].isResourceOEEInquiryDetails_isQuality !== undefined)
                serieQuality.values.push(dataset[i].isResourceOEEInquiryDetails_isQuality.Value);
            //OEE
            if (dataset[i].isResourceOEEInquiryDetails_isOEE !== undefined)
                serieOEE.values.push(dataset[i].isResourceOEEInquiryDetails_isOEE.Value);
            if (i === dataset.length - 1) {
                dataSeries.push({ labels: resource, series: [serieAvailability, seriePerformance, serieQuality, serieOEE] });
            }

        }
        return dataSeries;
    }

    // Insert the given head and return array of table data
    // Used on the hidden tables that store the data
    function processTable(tableId, theadInnerHtml) {
        var table = document.querySelector("table[id='" + tableId + "']");
        insertHead(tableId, theadInnerHtml);
        var data = (parseTable(table));
        var dataclean = data.filter(function (item) {
            return item.LineNumber !== "" && item.LineNumber.indexOf("empty") === -1;
        });
        return (dataclean);
    }

    // Create circular OEE charts
    function createOeeIndicators(
        idContainer,        // Add charts to this
        Size,               // 
        Availibility, Performance, Quality, Oee) {      // percentage values for each chart

        // adjust 'scale' property to make individual charts bigger or smaller
        var oeeCharts = [
            {
                percentage: Availibility,
                id: "oee-availability-chart",
                radialtext: _labels.isOEEAvailability,
                scale: 0.8
            },
            {
                percentage: Performance,
                id: "oee-performance-chart",
                radialtext: _labels.isOEEPerformance,
                scale: 0.8
            },
            {
                percentage: Quality,
                id: "oee-quality-chart",
                radialtext: _labels.isOEEQuality,
                scale: 0.8
            },
            {
                percentage: Oee,
                id: "oee-oee-chart",
                radialtext: _labels.isOEE,
                scale: 1
            },
        ];

        addCircleContainerDivs(idContainer);
        addCircleAndPercent(0);

        // <div> with <p> for chart title
        function addCircleContainerDivs(idDivContainer) {

            var defaultHeight = Size;

            var idCamstarContainer = d3.select(idDivContainer);
            var divOeeIndicator = idCamstarContainer.append('div').attr("class", "OeeIndicator");
            divOeeIndicator
                .selectAll('div')
                .data(oeeCharts).enter()
                .append('div')
                .attr('class', function(d) { return 'progress ' + (d.class ? d.class : '');})
                .attr('id', function (d) { return d.id; })
                .style('width', function(d) { return calcWidth(d) + "px"; })
                .style('height', function(d) { 
                    var height = defaultHeight;
                    if(d.scale) {
                        height = Math.max(defaultHeight, calcWidth(d));
                    }
                    return height + 'px';
                })
                .append('p')
                .attr('class', "radial-text")
                .style('text-align', "center")
                .text(function (d) { return d.radialtext; });

            function calcWidth(d) {
                return d.scale ? d.scale*Size : Size;
            }
            //Scrap
            var THIS_SHOULD_BE_THE_SCRAP_COUNT = 33;
            var scrapCountMarkup = 
                TEMPLATES.scrapCount
                    .replace(new RegExp('_SCRAPPEDCOUNT_'), THIS_SHOULD_BE_THE_SCRAP_COUNT)
                    .replace(new RegExp('_SCRAPPEDLABEL_'), _labels.isOEEScrapped);

            // MEN TODO - uncomment this to add scrap count to the OEE indicator section
            //$(divOeeIndicator[0]).append(scrapCountMarkup);
        }

        // Recursively create the circular charts
        function addCircleAndPercent(chartIndex) {

            if(chartIndex >= oeeCharts.length){
                return;
            }

            var colors = {
                fill: '#2c3187',
                track: '#cec6bc',
                text: '#2c3187',
                stroke: '#FFFFFF',
            }

            var start = 0;
            var end = oeeCharts[chartIndex].percentage;

            // set red/yellow/green
            var dataColor = DATA_COLORS.bad;
            if (end > 90) {
                dataColor = DATA_COLORS.good;
            } else if (end > 75) {
                dataColor = DATA_COLORS.warning;
            }

            var endAngle = Math.PI * 2;
            var formatText = d3.format('.0%');
            // If we have a scale, adjust the size accordingly
            var scaleData = oeeCharts[chartIndex].scale;
            var scale = scaleData ? parseFloat(scaleData) : 1;
            var count = end;
            var progress = start;
            var step = (end < start) ? -0.01 : 0.01;

            //Define the circle
            var $wrapper = $('#' + oeeCharts[chartIndex].id);
            var svgSize = Math.min( 
                $wrapper.outerWidth(false),
                $wrapper.outerHeight(false) - $wrapper.find('p.radial-text').outerHeight(false));
            
            //setup SVG wrapper
            var svg = d3.select($wrapper[0])
                .append('svg')
                .attr('width', svgSize)
                .attr('height', svgSize);

            var transformSize = svgSize / 2 - 1;

            // Add Group container
            var track = svg.append('g')
                .attr('transform', 'translate(' + transformSize + ',' + transformSize + ')');

            var circleStroke = 7;

            // circle def reused for both the gray part and the colored part
            var outerRadius = (svgSize * 0.5) - circleStroke;
            var circle = d3.svg.arc()
                .startAngle(0)
                .innerRadius(Math.min(outerRadius * 0.8, svgSize * (0.25 + (scale-1)*0.1)))
                .outerRadius(outerRadius);

            //Setup track
            // Gray arc
            track.append('path')
                .attr('fill', colors.track)
                .attr('stroke', colors.stroke)
                .attr('stroke-width', circleStroke + 'px')
                .attr('d', circle.endAngle(endAngle));

            // Colored arc
            var value = track.append('path')
                .attr('class', 'radial-path')
                .attr('fill', dataColor)
                .attr('stroke', colors.stroke)
                .attr('stroke-width', circleStroke +'px');

            //Add text value
            var numberText = track.append('text')
                .attr('class', 'indicator-percent')
                .attr('fill', colors.text)
                .attr('text-anchor', 'middle')
                .style('font-size', function(){ return (16 * scale) + 'px'; })
                .attr('dy', '.3em');

            //Action - update the angle of the circular fill and the text
            function update(progress) {
                //update position of endAngle
                value.attr('d', circle.endAngle(endAngle * progress));
                //update text value
                numberText.text(formatText(progress));
            }

            (function iterate() {
                //call update to begin animation
                update(progress);
                if (count > 0) {
                    //reduce count till it reaches 0
                    count--;
                    //increase progress
                    progress += step;
                    //Control the speed of the fill
                    setTimeout(iterate, 10);
                }
            })();
                
            addCircleAndPercent(chartIndex+1); 
        }

    }
})();
