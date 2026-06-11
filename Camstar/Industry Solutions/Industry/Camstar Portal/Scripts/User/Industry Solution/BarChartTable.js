/*
***************************************************************************
© 2017 Siemens Product Lifecycle Management Software Inc.
This file should include all user-defined javascript functions. 
Functions defined in this file will over-ride any functions of the
same name written in any of the Camstar supplied javascript files.
***************************************************************************
*/
$(function () {
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
        good: '#5EB042',
        warning: '#FFC000',
        bad: '#FF0000'
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

    // call server to load all the labels we need
    function loadLabels() {
        // Build param to getLabels()
        var labelNames = [];
        
        $.each(_labels, function(name, val){
            labelNames.push({Name: name, Value: val});
        });

        // call server
        __page.getLabels(labelNames, function(response){
            if($.isArray(response)) {
                // success.  update the _labels object
                response.forEach(function(labelNameVal){
                    _labels[labelNameVal.Name] = labelNameVal.Value;
                });
            } else {
                console.error(response.Error);
                //__page.displayStatus(response.Error, "Error");
            }

            // Do this after we have all the localized strings
            createReport();
        });
    }

    // 
    function createReport() {

        d3.select("#WebPart_containerDiv").selectAll("*").remove();

        // These appear to refer to the hidden tables that store the data
        var reasonDowntimeTableId = 'ctl00_WebPartManager_isResourceOEETopReasons_isResourceOEEInquiry_isOEEDowntimeReason';
        var reasonDowntimeTHeadHTML = 
            '<tr>\
                <th title="Field #0">LineNumber</th>\
                <th title="Field #1">EmptyColumn</th>\
                <th title="Field #2">isReasonName</th>\
                <th title="Field #3">isReasonDuration</th>\
            </tr>';

        var oeeIndicatorTableId = 'ctl00_WebPartManager_isResourceOEETopReasons_isResourceOEEInquiry_isResourceOEEInquiryDetails';
        var oeeIndicatorHeadHTML = 
        '<tr>\
            <th title="Field #0">LineNumber</th>\
            <th title="Field #1">EmptyColumn</th>\
            <th title="Field #2">Resource</th>\
            <th title="Field #3">Availability</th>\
            <th title="Field #4">Performance</th>\
            <th title="Field #5">Quality</th>\
            <th title="Field #6">OEE</th>\
        </tr>';

        var reasonScrapAndReworkTableId = 'ctl00_WebPartManager_isResourceOEETopReasons_isResourceOEEInquiry_isResourceOEEScrapRw';
        var reasonScrapAndReworkTHeadHTML = 
        '<tr>\
            <th title="Field #0">LineNumber</th>\
            <th title="Field #1">EmptyColumn</th>\
            <th title="Field #2">Type</th>\
            <th title="Field #3">Reason</th>\
            <th title="Field #4">Quantity</th>\
        </tr>';

        var categoryDistributionTableId = 'ctl00_WebPartManager_isResourceOEETopReasons_isResourceOEEInquiry_isResourceOEECatDistribution';
        var categoryDistributionTHeadHTML = 
        '<tr>\
            <th title="Field #0">LineNumber</th>\
            <th title="Field #1">EmptyColumn</th>\
            <th title="Field #2">isCategoryName</th>\
            <th title="Field #3">isCategoryDuration</th>\
            <th title="Field #4">isCategoryType</th>\
            <th title="Field #5">isCategoryCode</th>\
        </tr>';

        var dataReasonDowntime = makeStructureReasonDowntime(processTable(reasonDowntimeTableId, reasonDowntimeTHeadHTML));
        var dataOEEIndicator = makeStructureForOEE(processTable(oeeIndicatorTableId, oeeIndicatorHeadHTML));
        var dataScrapAndRework = makeStructureScrapAndRework(processTable(reasonScrapAndReworkTableId, reasonScrapAndReworkTHeadHTML));
        var dataCategoryDistribution = makeStructureCategoryDistribution(processTable(categoryDistributionTableId, categoryDistributionTHeadHTML));
        var oeeAvailibility = 0;
        var oeePerformance = 0;
        var oeeQuality = 0;
        var oeeOEE = 0;
        if (dataOEEIndicator[0] != null) {
            oeeAvailibility = getSerieseValue(dataOEEIndicator[0].series[0]);// dataOEEIndicator[0].series[0].values[0];
            oeePerformance = getSerieseValue(dataOEEIndicator[0].series[1]);//dataOEEIndicator[0].series[1].values[0];
            oeeQuality = getSerieseValue(dataOEEIndicator[0].series[2]);//dataOEEIndicator[0].series[2].values[0];
            oeeOEE = getSerieseValue(dataOEEIndicator[0].series[3]);//dataOEEIndicator[0].series[3].values[0];
        }
        createOeeIndicators("#WebPart_containerDiv", 134, oeeAvailibility, oeePerformance, oeeQuality, oeeOEE);

        var columnsReasonDowntime = [_labels.isOEEReason, _labels.isOEEDuration, ""];
        var columnsReworkReason = [_labels.isOEEType, _labels.isOEEReason, _labels.isOEEQuantity, ""];
        
        createStatusCodeChart("#WebPart_containerDiv", dataCategoryDistribution);
        
        // Sort the data in descending order
        dataReasonDowntime.sort(function (a, b) { return d3.descending(parseInt(a[1]), parseInt(b[1])) });
        barChart("#WebPart_containerDiv", "DowntimeReason", dataReasonDowntime, columnsReasonDowntime, _labels.isDowntimeTopFaultReasons, [0], 1, 2);//,chartWidth);
        
        dataScrapAndRework.sort(function (a, b) { return d3.descending(parseInt(a[2]), parseInt(b[2])) });
        barChart("#WebPart_containerDiv", "ReworkReason", dataScrapAndRework, columnsReworkReason, _labels.isQualityTopScrapReasons, [0,1], 2, 2);//,chartWidth);
    }

    function getSerieseValue(series) {
        var value = 0;
        var length = series.values.length;
        for (var i = 0; i < length; i++) {
            value += parseFloat(series.values[i]);
        }
        value = value / length;
        return value;
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
    function makeStructureReasonDowntime(dataset) {
        var data = [];

        for (var i = 0; i < dataset.length; i++) {
            data.push([
                dataset[i].isReasonName, 
                dataset[i].isReasonDuration/60, 
                formatMMSS(dataset[i].isReasonDuration)
            ]);
        }

        return data;
        
        // TODO - move to utility class?
        function formatMMSS(seconds) {
            return [
                addLeading0(Math.floor(seconds / 60)),  // MM
                addLeading0(Math.floor(seconds % 60))    // SS
            ].join(':');
        }

        function addLeading0(timeNum) {
            var leadingChar = (timeNum < 10) ? '0' : '';
            return leadingChar + timeNum;
        }
    }

    function makeStructureScrapAndRework(dataset) {
        var data = [];
        for (var i = 0; i < dataset.length; i++) {

            data.push([dataset[i].Type, dataset[i].Reason, dataset[i].Quantity]);

        }
        return data;
    }

    function makeStructureForOEE(dataset) {
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
            serieAvailability.values.push(dataset[i].Availability);
            //Performance
            seriePerformance.values.push(dataset[i].Performance);
            //Quality
            serieQuality.values.push(dataset[i].Quality);;
            //OEE
            serieOEE.values.push(dataset[i].OEE);
            if (i === dataset.length - 1) {
                dataSeries.push({ labels: resource, series: [serieAvailability, seriePerformance, serieQuality, serieOEE] });
            }

        }
        return dataSeries;
    }

    function makeStructureCategoryDistribution(dataset) {
        var data = [];
        for (var i = 0; i < dataset.length; i++) {
            data.push({ 
                type: dataset[i].isCategoryName, 
                count: parseInt(dataset[i].isCategoryDuration), 
                loss: parseInt(dataset[i].isCategoryType) 
            });//,dataset[i].isCategoryType]);
        }
        return data;
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

    // Populate a horizontal bar chart
    function barChart(
        containerSelector,  // CSS selector for DOM element we will put the chart into      
        containerClassName, // CSS class to assign to the DOM element that contains the chart
        data,               // array of arrays - each one is a data line
        columnHeaderTexts,  // array of string with header text for each column
        titleText,
        dataLabelIndexes,   // array of indexes into data sub-array.  Indicates which are the data point label(s) to display (in the first columns)
        dataValueIndex,     // index into data sub-array.  Indicates which is the data value
        dataDisplayIndex    // index into data sub-array.  Indicates which is the data value to display
        )     
        //,chartWidth)
    {
        if (data[0] == null) {
            data = [["", "", 0]];
        };

        // Setup the scale for the values for display, use abs max as max value
        var x = d3.scale.linear()
            .domain([0, d3.max(data, function (d) { return Math.abs(d[dataValueIndex]); })])
            .range(["0", "100%"]);

        var divContainer = d3.select(containerSelector).append('div').attr("class", containerClassName);
        var title = divContainer
            .append('p')
            .attr('class', 'oee-report-section-header')
            .text(titleText);

        var table = divContainer.append("table");
        table.attr('class', 'oee-horiz-bar-chart');

        //insert head
        table.append('thead')
            .append('tr')
            .selectAll('th')
            .data(columnHeaderTexts).enter()
            .append('th')
            .attr('class', 'headresult')
            .text(function (column) { return column; });

        // Create a table with rows and bind a data row to each table row
        var tr = table.append('tbody').selectAll("tr.data")
            .data(data)
            .enter()
            .append("tr")
            .attr("class", "datarow");

        // Set the even columns
        //d3.selectAll(".datarow").filter(":nth-child(even)").attr("class", "datarow even");

        // Create column for the data point label BEFORE the data value
        dataLabelIndexes.forEach(function(dlIndex){
            tr.append("td")
            .attr("class", "data name")
            .text(function (d) { return d[dlIndex] });
        });

        // Create a column for the chart
        var chart = tr.append("td").attr("class", "OEEchart");//.attr("width", chartWidth);	
        
        // Create the value column
        tr.append("td")
            .attr("class", "data value")
            .text(function (d) { return d[dataDisplayIndex] })

        // Create the div structure of the chart
        chart
            .append("div")
            .attr("class", "container")
            .append("div")
            .attr("class", "positive");

        // Creates the positive div bar
        tr.select("div.positive")
            .style("width", "0%")
            .transition()
            .duration(500)
            .style("width", function (d) { return d[dataValueIndex] > 0 ? x(d[dataValueIndex]) : "0%"; })
            .style("background-color", "linear-gradient(-90deg, red, orange);");
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
                radialtext: _labels.isOEEAvailability
            },
            {
                percentage: Performance,
                id: "oee-performance-chart",
                radialtext: _labels.isOEEPerformance
            },
            {
                percentage: Quality,
                id: "oee-quality-chart",
                radialtext: _labels.isOEEQuality,
                scale: 1
            },
            {
                percentage: Oee,
                id: "oee-oee-chart",
                radialtext: _labels.isOEE,
                scale: 1.45
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
            if(end > 90) {
                dataColor = DATA_COLORS.good;
            } else if(end > 75) {
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

            // Add Group container
            var track = svg.append('g')
                .attr('transform', 'translate(' + svgSize / 2 + ',' + svgSize / 2 + ')');

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

            if (count < 0) {
                numberText.text(formatText(parseFloat(count / 100.0)));

            } else {
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
            }
            addCircleAndPercent(chartIndex+1); 
        }
    }


    // ***** S T A C K E D   B A R ****** 


    function createStatusCodeChart(idContainer, dataset) {
        if (dataset[0] == null) {

            dataset[0] = {
                type: '[no-data]',
                count: 0,
                loss: 0
            };
        };

        //Create Color

        // Generate 1000 random colors
        var randomColors = [];

        // Convert given RGB values (0-255) to hex string like '#RRGGBB'
        function rgbToHexColor(red, green, blue)
        {
            var decColor = (0x010000 * red) + (0x000100 * green) + (0x000001 * blue);
            // add leading 0's and grab the right-most 6 characters
            return '#' + ('000000' + decColor.toString(16)).substr(-6);
        }

        var r = 55;
        var g = 210;
        var b = 120;
        for (i = 0; i < 1000; i++) {
            randomColors.push(rgbToHexColor(r, g, b));
            r = Math.min(220, (r + 73) % 255);
            g = Math.min(220, (g + 51) % 255);
            b = Math.min(220, (b + 103) % 255);
        }

        // Sum up data and associate each "type" (Resource Status Code?) with a color
        var indexColor = [];
        var topforlegend = 10;
        var i = 0;
        var verde = "#009900";
        var colore = "";

        // Create the indexColor array for each type
        dataset.forEach(function (ds) {
            var found = false;
            for (i = 0; i < indexColor.length; i++) {
                if ((indexColor[i].type == ds.type) && !found) {
                    indexColor[i].value = indexColor[i].value + ds.count;
                    found = true;
                }
            }
            if (!found) {
                if (ds.loss > 0) { 
                    colore = randomColors[i]; 
                } else { 
                    colore = verde; 
                }
                indexColor.push({ 
                    type: ds.type, 
                    color: colore, 
                    value: ds.count 
                });
            }
        });

        // assign the related color (type/color/value) object to each item in the dataset
        dataset.forEach(function (ds) {
            ds.color = (indexColor.filter(function (d) { return d.type == ds.type })[0].color);
        });

        indexColor.sort(function(a, b) { parseInt(b.value) - parseInt(a.value); });
        indexColor = indexColor.slice(0, topforlegend);

        var divContainer = d3.select(idContainer);
        var chartContainer = divContainer.append('div').attr('id', 'status-code-chart');
        var divTitle = chartContainer.append('div').attr('class', 'TitleStackedBar');
        var divLegend = chartContainer.append('div').attr('class', 'LegendStackedBar');
        var divTab = chartContainer.append('div').attr('class', 'StackedBar');

        divTitle.append('p').text(_labels.isResStatusCodeDist);

        var margins = {
            top: 15,//(size/2.20)/1.50,
            left: 10,
            right: 10,
            bottom: 10
        }
        var svgHeight = 70;
        
        var svg = divTab.append('svg')
            .attr('width', '100%')
            .attr('height', svgHeight)
            .append('g');

        var svgWidth = $('.StackedBar svg').outerWidth(false);
        var width = svgWidth - margins.left - margins.right;
        var height = svgHeight - margins.top - margins.bottom;

        // start out y0 at 0. we increment it below for each
        // data item, so we know where to start each bar.
        var y0 = 0;
        dataset = dataset.map(function (d) {
            //d3.layout.stack expects an array of arrays
            //e.g., think of a stacked vertical bar chart.
            var element = [{
                x: d.type,
                y: d.count,
                y0: y0,
                color: d.color
            }];
            y0 = y0 + d.count; //set y0 for the next element.
            return element;
        });

        // grab the sum of our data values
        // for calculating percentage later (for display)
        var sum = d3.sum(dataset, function (d) { return d[0].y; });
        var stack = d3.layout.stack();
        stack(dataset);
        dataset = dataset.map(function (group) {
            return group.map(function (d) {
                // Invert the x and y values, and y0 becomes x0
                return {
                    x: d.y,
                    y: d.x,
                    x0: d.y0,
                    color: d.color
                };
            });
        });

        var xMax = d3.max(dataset, function (group) {
            return d3.max(group, function (d) {
                return d.x + d.x0;
            });
        });

        var xScale = d3.scale.linear()
            .domain([0, xMax])
            .range([0, width]);
        
        var groups = svg.selectAll('g')
            .data(dataset)
            .enter()
            .append('g')
            .attr('fill', function (d, i) {
                return d[0].color;
            });

        var rects = groups.selectAll('rect')
            .data(function (d) {
                return d;
            })
            .enter()
            .append('rect')
            .attr('x', function (d) {
                //console.log('x0=', d.x0, 'xScale(d.x0)=', xScale(d.x0));
                return xScale(d.x0) + margins.left;
            })
            .attr('y', margins.top)
            .attr('height', height)
            .attr('width', function (d) {
                //set chart's width.
                return xScale(d.x);
            });


        var piede = divTab.append('p');
        piede.append('span').text(_labels.isOEEStart);
        piede.append('span').attr('id', 'time-end').text(_labels.isOEEEnd);

        // Draw legend
        var legendRectSize = 18,
            legendSpacing = 4
            gapBetweenGroups = 15,
            spaceForLabels = 150,
            spaceForLegend = 200;

        var $divLegend = $(divLegend[0]);
        indexColor.forEach(function(statusCode){
            var statusCodeMarkup = 
                TEMPLATES.legendStatusCode
                    .replace(new RegExp('_STATUSCODECOLOR_'), statusCode.color)
                    .replace(new RegExp('_STATUSCODENAME_'), statusCode.type);

            $divLegend.append(statusCodeMarkup);
        });
    }

    $("#aspnetForm").submit(function () {
        setTimeout(function () { loadLabels(); }, 700);
    });
});
