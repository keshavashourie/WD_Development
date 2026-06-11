/*
***************************************************************************
© 2017 Siemens Product Lifecycle Management Software Inc.
This file should include all user-defined javascript functions. 
Functions defined in this file will over-ride any functions of the
same name written in any of the Camstar supplied javascript files.
***************************************************************************
*/
$(function () {

$.getScript( "scripts/jquery/d3.v3.min.js", function( data, textStatus, jqxhr ) {
  console.log( data ); // Data returned
  console.log( textStatus ); // Success
  console.log( jqxhr.status ); // 200
  console.log( "Load was performed." );
});

$.getScript("https://d3js.org/d3.v3.min.js", function () {
 

    var controlTableId = 'ctl00_WebPartManager_is_OEEInd2_WP_isResourceOEEInquiry_isResourceOEEInquiryDetails';

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

    function insertHead(idtable) {
        var table = document.getElementById(idtable);
        var header = table.createTHead();
        header.style.display = "none";
        header.innerHTML = '<tr><th title="Field #0">LineNumber</th><th title="Field #1">EmptyColumn</th><th title="Field #2">Resource</th><th title="Field #3">Availability</th><th title="Field #4">Performance</th><th title="Field #5">Quality</th><th title="Field #6">OEE</th></tr>';
    }

    function parseTable(table) {
        var headings = arrayify(table.tHead.rows[0].cells).map(function (heading) {
            return heading.innerText;
        });
        return arrayify(table.tBodies[0].rows).map(factory(headings));
    }

    function makeStructure(dataset) {
        var resource = [];
        var dataSeries = [];
        var resource = [];
        var serieAvailability = {
            label: "Availability",
            values: []
        };
        var seriePerformance = {
            label: "Performance",
            values: []
        };
        var serieQuality = {
            label: "Quality",
            values: []
        };
        var serieOEE = {
            label: "OEE",
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
            if (i == dataset.length - 1) {
                dataSeries.push({ labels: resource, series: [serieAvailability, seriePerformance, serieQuality, serieOEE] });
            }

        }
        return dataSeries;
    }

    function processTable(tablename) {
        var table = document.querySelector("table[id='" + tablename + "']");
        insertHead(tablename);
        var data = (parseTable(table));
        var dataclean = data.filter(function (item) {
            return item.LineNumber !== "" && item.LineNumber.indexOf("empty") === -1;
        });
        return makeStructure(dataclean);
    }

    d3.select("#WebPart_containerDiv").append("svg").attr("class", "chart");

    var dataset = [];
    var dataSeries = [];

    var precChoice = 0;
    var gapBetweenGroups = 50,
        spaceForLabels = 150,
        spaceForLegend = 200;

    function drawTheLines(data) {
        console.log('@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@');
        console.log(data);
        d3.select("svg").selectAll("*").remove();
        if (data) {
            var chartWidth = 700,
                barHeight = 20,
                groupHeight = barHeight * data.series.length;

            var zippedData = [];
            for (var i = 0; i < data.labels.length; i++) {
                for (var j = 0; j < data.series.length; j++) {
                    zippedData.push(parseFloat(data.series[j].values[i]));
                }
            }

            // Color scale
            var color = d3.scale.category10();

            var chartHeight = barHeight * zippedData.length + gapBetweenGroups * data.labels.length;


            var y = d3.scale.linear()
                .range([chartHeight + gapBetweenGroups, 0]);

			console.log("MAX->" + d3.max(zippedData));
			console.log(zippedData);
			console.log("^zippedData^");
            var x = d3.scale.linear()
                .domain([0, d3.max(zippedData)])
                .range([0, chartWidth]);


            var yAxis = d3.svg.axis()
                .scale(y)
                .tickFormat('')
                .tickSize(0)
                .orient("left");



            // Specify the chart area and dimensions
            var chart = d3.select(".chart")
                .attr("width", spaceForLabels + chartWidth + spaceForLegend)
                .attr("height", chartHeight);

            // Create bars
            var bar = chart.selectAll("g")
                .data(zippedData)
                .enter().append("g")
                .attr("transform", function (d, i) {
                    return "translate(" + spaceForLabels + "," + (i * barHeight + gapBetweenGroups * (0.5 + Math.floor(i / data.series.length))) + ")";
                });

            // Create rectangles of the correct width
            bar.append("rect")
                .attr("fill", function (d, i) {
                    var mod = (i % data.series.length)
                    /*var c = mod +4*mod-(mod == 0 ? 0 : mod);
                    alert(c +" " +color(c));
                    return color(c); */
                    return color(mod);
                })
                .attr("class", "bar")
                .attr("width", x)
                .attr("height", barHeight - 1);

            // Add text label in bar
            bar.append("text")
                .attr("x", function (d) { return x(d) + 25; })
                .attr("y", barHeight / 2)
                .attr("fill", "red")
                .attr("dy", ".35em")
                .text(function (d) { return d; });

            // Draw labels
            bar.append("text")
                .attr("class", "label")
                .attr("x", function (d) { return - 10; })
                .attr("y", groupHeight / 2)
                .attr("dy", ".35em")
                .text(function (d, i) {
                    if (i % data.series.length === 0)
                        return data.labels[Math.floor(i / data.series.length)];
                    else
                        return ""
                });

            chart.append("g")
                .attr("class", "y axis")
                .attr("transform", "translate(" + spaceForLabels + ", " + -gapBetweenGroups / 2 + ")")
                .call(yAxis);

            var xAxis = d3.svg.axis()
                .scale(x)
                .orient("bottom")
                .tickFormat(d3.format(".2s"));

            chart.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(" + spaceForLabels + ", " + data.labels.length * (groupHeight + gapBetweenGroups) + ")")
                .call(xAxis);
            /*	  
                  .append("text")
                  .attr("x", chartWidth)
                  .style("text-anchor", "end")
                  .style("font-weight","bold")
                  .style("fill","black")
                  .text("Percentage %");	 */

            // Draw legend
            var legendRectSize = 18,
                legendSpacing = 4;


            var legend = chart.selectAll('.legend')
                .data(data.series)
                .enter()
                .append('g')
                .attr('transform', function (d, i) {
                    var height = legendRectSize + legendSpacing;
                    var offset = -gapBetweenGroups / 2;
                    var horz = spaceForLabels + chartWidth + 100 - legendRectSize;
                    var vert = i * height - offset;
                    return 'translate(' + horz + ',' + vert + ')';
                });

            legend.append('rect')
                .attr('width', legendRectSize)
                .attr('height', legendRectSize)
                .style('fill', function (d, i) { return color(i); })
                .style('stroke', function (d, i) { return color(i); });

            legend.append('text')
                .attr('class', 'legend')
                .attr('x', legendRectSize + legendSpacing)
                .attr('y', legendRectSize - legendSpacing)
                .text(function (d) { return d.label; });
        }
    }

/*

    $('#ctl00_WebPartManager_isOEEIndicator_WP_isResourceOEEInquiry_isResourceFamily_Edit').on('change', function (e) {
        setTimeout('__doPostBack(\'ctl00$WebPartManager$isOEEIndicator_WP$isResourceOEEInquiry_isResourceFamily$Edit\',\'\')', 0);
        setTimeout(function () { drawTheLines(processTable(controlTableId)[0]);}, 500);
        
    });



*/

$("#aspnetForm").submit(function() {
     
		setTimeout(function () { drawTheLines(processTable(controlTableId)[0]);}, 500);
       
    
});


});
});
